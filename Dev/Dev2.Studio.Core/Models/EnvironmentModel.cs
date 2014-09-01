using System;
using System.Network;
using Dev2.AppResources.Repositories;
using Dev2.Communication;
using Dev2.Providers.Logs;
using Dev2.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core.AppResources.Repositories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Workspaces;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Models
{

    public class EnvironmentModel : ObservableObject, IEnvironmentModel
    {
        IStudioResourceRepository _studioResourceRepo;
        IAuthorizationService _authorizationService;

        public event EventHandler<ConnectedEventArgs> IsConnectedChanged;
        public event EventHandler<ResourcesLoadedEventArgs> ResourcesLoaded;
        public event EventHandler AuthorizationServiceSet;

        protected virtual void OnAuthorizationServiceSet()
        {
            var handler = AuthorizationServiceSet;
            if(handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #region CTOR
        //, IWizardEngine wizardEngine
        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection)
            : this(id, environmentConnection, StudioResourceRepository.Instance)
        {
        }
        //, IWizardEngine wizardEngine
        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection, IStudioResourceRepository studioResourceRepository)
        {
            Initialize(id, environmentConnection, null, studioResourceRepository);
        }

        public EnvironmentModel(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, IStudioResourceRepository studioResourceRepository)
        {
            VerifyArgument.IsNotNull("resourceRepository", resourceRepository);
            Initialize(id, environmentConnection, resourceRepository, studioResourceRepository);
        }

        //, IWizardEngine wizardEngine
        void Initialize(Guid id, IEnvironmentConnection environmentConnection, IResourceRepository resourceRepository, IStudioResourceRepository studioResourceRepository)
        {
            VerifyArgument.IsNotNull("environmentConnection", environmentConnection);
            VerifyArgument.IsNotNull("studioResourceRepository", studioResourceRepository);
            _studioResourceRepo = studioResourceRepository;
            CanStudioExecute = true;

            ID = id; // The resource ID
            Connection = environmentConnection;

            // MUST set Connection before creating new ResourceRepository!!



            Connection.NetworkStateChanged += OnNetworkStateChanged;

            PermissionsModifiedService = new PermissionsModifiedService(Connection.ServerEvents);

            // MUST subscribe to Guid.Empty as memo.InstanceID is NOT set by server!
            PermissionsModifiedService.Subscribe(Guid.Empty, ReceivePermissionsModified);
            ResourceRepository = resourceRepository ?? new ResourceRepository(this);
        }

        void ReceivePermissionsModified(PermissionsModifiedMemo memo)
        {
            if(memo.ServerID == Connection.ServerID && !Name.Contains("localhost"))
            {
                var resourcePermissions = AuthorizationService.GetResourcePermissions(Guid.Empty);

                _studioResourceRepo.UpdateRootAndFoldersPermissions(resourcePermissions, ID);
            }
        }

        #endregion

        #region Properties

        public IAuthorizationService AuthorizationService
        {
            get
            {
                return _authorizationService;
            }
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

        public bool IsLocalHost { get { return IsLocalHostCheck(); } }
        public bool HasLoadedResources { get; private set; }

        public IEnvironmentConnection Connection { get; private set; }

        public string Name { get { return Connection.DisplayName; } set { Connection.DisplayName = value; } }

        public bool IsConnected { get { return Connection.IsConnected; } }

        public bool IsAuthorized { get { return Connection.IsAuthorized; } }
        public bool IsAuthorizedDeployFrom
        {
            get
            {
                return AuthorizationService.IsAuthorized(AuthorizationContext.DeployFrom, null);
            }
        }
        public bool IsAuthorizedDeployTo
        {
            get
            {
                return AuthorizationService.IsAuthorized(AuthorizationContext.DeployTo, null);
            }
        }

        public IResourceRepository ResourceRepository { get; private set; }
        public string DisplayName
        {
            get
            {
                return Name + " (" + Connection.WebServerUri + ")";
            }
        }
        public PermissionsModifiedService PermissionsModifiedService { get; set; }

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

            this.TraceInfo("Attempting to connect to [ " + Connection.AppServerUri + " ] ");
            Connection.Connect();
        }

        public void Connect(IEnvironmentModel other)
        {
            if(other == null)
            {
                throw new ArgumentNullException("other");
            }

            if(!other.IsConnected)
            {
                other.Connection.Connect();

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

        #endregion

        #region LoadResources

        public void RaiseResourcesLoaded()
        {
            RaiseLoadedResources();
        }

        public void LoadResources()
        {
            if(Connection.IsConnected && CanStudioExecute)
            {
                ResourceRepository.UpdateWorkspace(WorkspaceItemRepository.Instance.WorkspaceItems);
                HasLoadedResources = true;
            }
        }

        #endregion

        #region Event Handlers

        void RaiseIsConnectedChanged(bool isOnline)
        {
            if(IsConnectedChanged != null)
            {
                IsConnectedChanged(this, new ConnectedEventArgs { IsConnected = isOnline });
            }
            // ReSharper disable ExplicitCallerInfoArgument
            OnPropertyChanged("IsConnected");
            // ReSharper restore ExplicitCallerInfoArgument
        }
        void RaiseLoadedResources()
        {
            if(ResourcesLoaded != null)
            {
                ResourcesLoaded(this, new ResourcesLoadedEventArgs { Model = this });
            }
        }

        void OnNetworkStateChanged(object sender, NetworkStateEventArgs e)
        {
            RaiseNetworkStateChanged(e.ToState == NetworkState.Online || e.ToState == NetworkState.Connecting);
            if(e.ToState == NetworkState.Online)
            {
                AuthorizationService = CreateAuthorizationService(Connection);
                AuthorizationService.PermissionsChanged += OnAuthorizationServicePermissionsChanged;
                OnAuthorizationServicePermissionsChanged(this, EventArgs.Empty);
            }
        }

        void RaiseNetworkStateChanged(bool isOnline)
        {

            RaiseIsConnectedChanged(isOnline);
            if(!isOnline)
                HasLoadedResources = false;
        }

        #endregion

        #region IEquatable

        public bool Equals(IEnvironmentModel other)
        {
            if(other == null)
            {
                return false;
            }

            //Dont ever EVER check any other property here or the connect control will die and you will be beaten;)
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
            var isLocalConnection = environmentConnection.WebServerUri != null && !string.IsNullOrEmpty(environmentConnection.WebServerUri.AbsoluteUri) && environmentConnection.WebServerUri.AbsoluteUri.ToLower().Contains(Environment.MachineName.ToLower());
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
