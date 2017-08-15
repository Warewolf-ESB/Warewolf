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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Network;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Controller;
using Dev2.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Interfaces;





namespace Dev2.Studio.Core.Models
{

    public class Server : ObservableObject, IServer
    {
        IAuthorizationService _authorizationService;
        IEnvironmentConnection _connection;
        IList<IToolDescriptor> _tools;
        string _version;
        private string _minversion;
        Dictionary<string, string> _serverInformation;
        private IExplorerRepository _proxyLayer;
        public event EventHandler<ConnectedEventArgs> IsConnectedChanged;
        public event EventHandler<ResourcesLoadedEventArgs> ResourcesLoaded;
        public event EventHandler AuthorizationServiceSet;

        private void OnAuthorizationServiceSet()
        {
            var handler = AuthorizationServiceSet;
            handler?.Invoke(this, EventArgs.Empty);
        }

        #region CTOR        
        public Server(Guid id, IEnvironmentConnection environmentConnection)
        {
            Initialize(id, environmentConnection, null);
        }

        public Server(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository)
        {
            VerifyArgument.IsNotNull("resourceRepository", resourceRepository);
            Initialize(id, environmentConnection, resourceRepository);
        }

        void Initialize(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository)
        {
            VerifyArgument.IsNotNull("environmentConnection", environmentConnection);
            CanStudioExecute = true;
            EnvironmentID = id;
            Connection = environmentConnection;
            ResourceRepository = resourceRepository ?? new ResourceRepository(this);
            var communicationControllerFactory = new CommunicationControllerFactory();
            _proxyLayer = new StudioServerProxy(communicationControllerFactory, Connection);
            UpdateRepository = new StudioResourceUpdateManager(communicationControllerFactory, Connection);
            Connection.PermissionsModified += RaisePermissionsModifiedEvent;
            Connection.NetworkStateChanged += RaiseNetworkStateChangeEvent;
            Connection.ItemAddedMessageAction += ItemAdded;
        }

        void RaiseNetworkStateChangeEvent(object sender, NetworkStateEventArgs e)
        {
            NetworkStateChanged?.Invoke(new NetworkStateChangedEventArgs(e), this);
        }

        void RaisePermissionsModifiedEvent(object sender, List<WindowsGroupPermission> windowsGroupPermissions)
        {
            Permissions = windowsGroupPermissions.Select(permission => permission as IWindowsGroupPermission).ToList();
            PermissionsChanged?.Invoke(new PermissionsChangedArgs(windowsGroupPermissions.Cast<IWindowsGroupPermission>().ToList()));
        }


        void ItemAdded(IExplorerItem obj)
        {
            ItemAddedEvent?.Invoke(obj);
        }

        #endregion

        #region Properties

        public IAuthorizationService AuthorizationService
        {
            get { return _authorizationService ?? (_authorizationService = CreateAuthorizationService(Connection)); }
            set
            {
                _authorizationService = value;
                OnAuthorizationServiceSet();
            }
        }

        public bool CanStudioExecute { get; set; }

        public Guid EnvironmentID { get; private set; }

        public bool IsLocalHostCheck()
        {
            return Connection.IsLocalHost;
        }

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

        public string Name
        {
            get { return Connection.DisplayName; }
            set
            {
                Connection.DisplayName = value;
            }
        }

        public bool IsConnected => Connection.IsConnected;

        public bool IsAuthorized => Connection.IsAuthorized;

        public bool IsAuthorizedDeployFrom => AuthorizationService.IsAuthorized(AuthorizationContext.DeployFrom, null);

        public bool IsAuthorizedDeployTo => AuthorizationService.IsAuthorized(AuthorizationContext.DeployTo, null);

        public IResourceRepository ResourceRepository { get; private set; }
        public string DisplayName
        {
            get
            {
                if (Connection != null)
                {
                    var displayName = Connection.DisplayName;
                    if (IsConnected && (HasLoaded || Connection.IsLocalHost))
                    {
                        if (!displayName.Contains(Warewolf.Studio.Resources.Languages.Core.ConnectedLabel))
                        {
                            displayName += Warewolf.Studio.Resources.Languages.Core.ConnectedLabel;
                        }
                    }
                    else if (IsConnected)
                    {
                        displayName += Warewolf.Studio.Resources.Languages.Core.ConnectedLabel;
                    }
                    else if (!IsConnected && (HasLoaded || !Connection.IsLocalHost))
                    {
                        displayName = Connection.DisplayName.Replace("(Connected)", "");
                    }
                    return displayName;
                }

                return Connection?.DisplayName ?? "Default Name";
            }
            
            set
            {
                Connection.DisplayName = DisplayName;
                OnPropertyChanged();
            }
        }

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

            Dev2Logger.Debug("Attempting to connect to [ " + Connection.AppServerUri + " ] ", "Warewolf Debug");
            Connection.Connect(EnvironmentID);
        }

        public void Connect(IServer other)
        {
            if(other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if(!other.IsConnected)
            {
                other.Connection.Connect(EnvironmentID);

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
                OnPropertyChanged("DisplayName");
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
            OnPropertyChanged("IsConnected");
        }

        
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

        public bool Equals(IServer other)
        {
            if(other == null)
            {
                return false;
            }
            if (EnvironmentID != other.EnvironmentID)
            {
                return false;
            }
            var isEqual = other.Connection.Equals(Connection);
            return isEqual;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IServer);
        }

        public override int GetHashCode()
        {
            return EnvironmentID.GetHashCode();
        }

        #endregion

        protected virtual IAuthorizationService CreateAuthorizationService(IEnvironmentConnection environmentConnection)
        {
            var isLocalConnection = !string.IsNullOrEmpty(environmentConnection.WebServerUri?.AbsoluteUri) && environmentConnection.WebServerUri.AbsoluteUri.ToLower().Contains(Environment.MachineName.ToLower());
            return new ClientAuthorizationService(new ClientSecurityService(environmentConnection), isLocalConnection);
        }

        void OnAuthorizationServicePermissionsChanged(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("IsAuthorizedDeployTo");
            OnPropertyChanged("IsAuthorizedDeployFrom");

        }

        public async Task<IExplorerItem> LoadExplorer(bool reloadCatalogue = false)
        {
            var result = await ProxyLayer.LoadExplorer(reloadCatalogue);
            HasLoaded = true;
            return result;
        }

        public IList<IToolDescriptor> LoadTools()
        {
            if (_tools == null || _tools.Count == 0)
            {
                _tools = ProxyLayer.QueryManagerProxy.FetchTools();
            }
            return _tools;
        }

        public IExplorerRepository ExplorerRepository => ProxyLayer;
        public IStudioUpdateManager UpdateRepository { get; private set; }
        public IQueryManager QueryProxy => _proxyLayer.QueryManagerProxy;
        public bool AllowEdit => Connection != null && !Connection.IsLocalHost;
        public List<IWindowsGroupPermission> Permissions { get; set; }
        public Guid? ServerID => Connection.ServerID;
        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;
        public event ItemAddedEvent ItemAddedEvent;

        public string GetServerVersion()
        {
            if (!Connection.IsConnected)
            {
                Connection.Connect(Guid.Empty);
            }
            if (_version == null)
            {
                _version = ProxyLayer.AdminManagerProxy.GetServerVersion();
            }
            return _version;
        }

        public async Task<bool> ConnectAsync()
        {
            var connected = await Connection.ConnectAsync(EnvironmentID);
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("DisplayName");
            return connected;
        }

        public bool HasLoaded { get; private set; }
        public bool CanDeployTo => IsAuthorizedDeployTo;
        public bool CanDeployFrom => IsAuthorizedDeployFrom;
        public IExplorerRepository ProxyLayer
        {
            get
            {
                return _proxyLayer;
            }
            set
            {
                _proxyLayer = value;
            }
        }
        public Permissions UserPermissions { get; set; }

        public string GetMinSupportedVersion()
        {
            if (_minversion == null)
            {
                if (!Connection.IsConnected)
                {
                    Connection.Connect(Guid.Empty);
                }
                _minversion = ProxyLayer.AdminManagerProxy.GetMinSupportedServerVersion();
            }

            return _minversion;
        }

        public Task<List<string>> LoadExplorerDuplicates()
        {
            var result = ProxyLayer.LoadExplorerDuplicates();
            HasLoaded = true;
            return result;
        }

        public Permissions GetPermissions(Guid resourceID)
        {
            return AuthorizationService.GetResourcePermissions(resourceID);
        }

        public Dictionary<string, string> GetServerInformation()
        {
            if (!Connection.IsConnected)
            {
                Connection.Connect(Guid.Empty);
            }
            if (_serverInformation == null)
            {
                _serverInformation = ProxyLayer.AdminManagerProxy.GetServerInformation();
            }
            return _serverInformation;
        }
        
        public IVersionInfo VersionInfo { get; set; }        
    }
}
