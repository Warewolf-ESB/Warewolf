/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Network;
using Dev2.Common;
using Dev2.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Models
{

    public class EnvironmentModel : ObservableObject, IEnvironmentModel
    {
        IAuthorizationService _authorizationService;
        IEnvironmentConnection _connection;

        public event EventHandler<ConnectedEventArgs> IsConnectedChanged;
        public event EventHandler<ResourcesLoadedEventArgs> ResourcesLoaded;
        public event EventHandler AuthorizationServiceSet;
        public event EventHandler WorkflowSaved;

        protected virtual void OnAuthorizationServiceSet()
        {
            var handler = AuthorizationServiceSet;
            handler?.Invoke(this, EventArgs.Empty);
        }

        #region CTOR        
        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection)
        {
            Initialize(id, environmentConnection, null);
        }

        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository)
        {
            VerifyArgument.IsNotNull("resourceRepository", resourceRepository);
            Initialize(id, environmentConnection, resourceRepository);
        }

        void Initialize(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository)
        {
            VerifyArgument.IsNotNull("environmentConnection", environmentConnection);
            CanStudioExecute = true;
            ID = id; // The resource ID
            Connection = environmentConnection;
            ResourceRepository = resourceRepository ?? new ResourceRepository(this);
        }

       

        #endregion

        #region Properties

        public IAuthorizationService AuthorizationService
        {
            get { return _authorizationService ?? (_authorizationService = CreateAuthorizationService(Connection)); }
            private set
            {
                _authorizationService = value;
                OnAuthorizationServiceSet();
            }
        }

        public bool CanStudioExecute { get; set; }

        public Guid ID { get; private set; }

        public bool IsLocalHostCheck()
        {
            return Connection.IsLocalHost;
        }

        public string Category { get; set; }

        public bool IsLocalHost => IsLocalHostCheck();
        public bool HasLoadedResources { get; private set; }

        public IEnvironmentConnection Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                if(_connection != null)
                {
                    _connection.NetworkStateChanged -= OnNetworkStateChanged;
                }
                _connection = value;
                if(_connection != null)
                {
                    _connection.NetworkStateChanged += OnNetworkStateChanged;
                }
            }
        }

        public string Name { get { return Connection.DisplayName; } 
            set
            {
                Connection.DisplayName = value;
            } }

        public bool IsConnected => Connection.IsConnected;

        public bool IsAuthorized => Connection.IsAuthorized;

        public bool IsAuthorizedDeployFrom => AuthorizationService.IsAuthorized(AuthorizationContext.DeployFrom, null);

        public bool IsAuthorizedDeployTo => AuthorizationService.IsAuthorized(AuthorizationContext.DeployTo, null);

        public IResourceRepository ResourceRepository { get; private set; }
        public string DisplayName => Name + " (" + Connection.WebServerUri + ")";

        #endregion

        #region Connect

        public void Connect()
        {
            if(Connection.IsConnected)
            {
                return;
            }
            if(string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Name_Not_Provided));
            }

            Dev2Logger.Debug("Attempting to connect to [ " + Connection.AppServerUri + " ] ");
            Connection.Connect(ID);
        }

        public void Connect(IEnvironmentModel other)
        {
            if(other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if(!other.IsConnected)
            {
                other.Connection.Connect(ID);

                if(!other.IsConnected)
                {
                    throw new InvalidOperationException("Environment failed to connect.");
                }
            }
            Connect();
        }

        #endregion

        #region Disconnect

        public void Disconnect()
        {
            if(Connection.IsConnected)
            {
                Connection.Disconnect();
            }
        }

        #endregion

        #region ForceLoadResources

        public void ForceLoadResources()
        {
            if(Connection.IsConnected && CanStudioExecute)
            {
                ResourceRepository.ForceLoad();
                HasLoadedResources = true;
            }
        }

        public void FireWorkflowSaved()
        {
            var handler = WorkflowSaved;
            handler?.Invoke(this,EventArgs.Empty);
        }

        #endregion

        #region LoadResources

        public void LoadResources()
        {
            if(Connection.IsConnected && CanStudioExecute)
            {
                ResourceRepository.UpdateWorkspace();
                HasLoadedResources = true;
            }
        }

        #endregion

        #region Event Handlers

        void RaiseIsConnectedChanged(bool isOnline)
        {
            IsConnectedChanged?.Invoke(this, new ConnectedEventArgs { IsConnected = isOnline });
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsConnected");
            // ReSharper restore ExplicitCallerInfoArgument
        }
        // ReSharper disable once ArrangeTypeMemberModifiers
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        void RaiseLoadedResources()
        {
            ResourcesLoaded?.Invoke(this, new ResourcesLoadedEventArgs { Model = this });
        }

        void OnNetworkStateChanged(object sender, NetworkStateEventArgs e)
        {
            RaiseNetworkStateChanged(e.ToState == NetworkState.Online || e.ToState == NetworkState.Connecting);
            if(e.ToState == NetworkState.Connecting || e.ToState == NetworkState.Offline)
            {
                if(AuthorizationService != null)
                {
                    AuthorizationService.PermissionsChanged -= OnAuthorizationServicePermissionsChanged;
                }
            }
            if(e.ToState == NetworkState.Online)
            {
                if(AuthorizationService == null)
                {
                    AuthorizationService = CreateAuthorizationService(Connection);
                    AuthorizationService.PermissionsChanged += OnAuthorizationServicePermissionsChanged;
                    OnAuthorizationServicePermissionsChanged(null, new EventArgs());
                }
            }
        }

        void RaiseNetworkStateChanged(bool isOnline)
        {

            RaiseIsConnectedChanged(isOnline);
            if(!isOnline)
            {
                HasLoadedResources = false;

            }
        }

        #endregion

        #region IEquatable

        public bool Equals(IEnvironmentModel other)
        {
            if(other == null)
            {
                return false;
            }

            //don't ever EVER check any other property here or the connect control will die and you will be beaten;)
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IEnvironmentModel);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        #endregion

        protected virtual IAuthorizationService CreateAuthorizationService(IEnvironmentConnection environmentConnection)
        {
            var isLocalConnection = !string.IsNullOrEmpty(environmentConnection.WebServerUri?.AbsoluteUri) && environmentConnection.WebServerUri.AbsoluteUri.ToLower().Contains(Environment.MachineName.ToLower());
            return new ClientAuthorizationService(new ClientSecurityService(environmentConnection), isLocalConnection);
        }

        void OnAuthorizationServicePermissionsChanged(object sender, EventArgs eventArgs)
        {
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsAuthorizedDeployTo");
            OnPropertyChanged("IsAuthorizedDeployFrom");

        }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
