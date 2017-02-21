using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Controller;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public class Server : Resource, IServer, INotifyPropertyChanged
    {
        readonly Guid _serverId;
        readonly IExplorerRepository _proxyLayer;
        IList<IToolDescriptor> _tools;
        readonly IEnvironmentModel _environmentModel;
        bool _hasloaded;
        string _version;
        Dictionary<string, string> _serverInformation;
        private string _minversion;
        private List<IWindowsGroupPermission> _permissions;

        public Server(IEnvironmentModel environmentModel)
        {
            EnvironmentConnection = environmentModel.Connection;
            EnvironmentID = environmentModel.ID;
            _serverId = EnvironmentConnection.ServerID;
            ResourceID = environmentModel.ID;
            var communicationControllerFactory = new CommunicationControllerFactory();
            _proxyLayer = new StudioServerProxy(communicationControllerFactory, EnvironmentConnection);
            UpdateRepository = new StudioResourceUpdateManager(communicationControllerFactory, EnvironmentConnection);
            EnvironmentConnection.PermissionsModified += RaisePermissionsModifiedEvent;
            ResourceName = EnvironmentConnection.DisplayName;
            EnvironmentConnection.NetworkStateChanged += RaiseNetworkStateChangeEvent;
            EnvironmentConnection.ItemAddedMessageAction += ItemAdded;
            _environmentModel = environmentModel;
        }

        public Server(IExplorerRepository repository, IEnvironmentModel environmentModel)
            : this(environmentModel)
        {
            _proxyLayer = repository;
        }

        public bool CanDeployTo => _environmentModel.IsAuthorizedDeployTo;

        public bool CanDeployFrom => _environmentModel.IsAuthorizedDeployFrom;

        public Permissions GetPermissions(Guid resourceID)
        {
            return _environmentModel.AuthorizationService.GetResourcePermissions(resourceID);
        }
        public Server()
        {
        }

        public Guid EnvironmentID { get; set; }
        public Guid? ServerID => EnvironmentConnection.ServerID;

        public bool HasLoaded
        {
            get
            {
                return _hasloaded;
            }
            private set
            {

                _hasloaded = value;
                OnPropertyChanged("IsConnected");
                OnPropertyChanged("DisplayName");
            }
        }

        void ItemAdded(IExplorerItem obj)
        {
            ItemAddedEvent?.Invoke(obj);
        }

        public Dictionary<string, string> GetServerInformation()
        {
            if (!EnvironmentConnection.IsConnected)
            {
                EnvironmentConnection.Connect(Guid.Empty);
            }
            _serverInformation = ProxyLayer.AdminManagerProxy.GetServerInformation();

            return _serverInformation;
        }

        public string GetServerVersion()
        {
            if (!EnvironmentConnection.IsConnected)
            {
                EnvironmentConnection.Connect(Guid.Empty);
            }
            _version = ProxyLayer.AdminManagerProxy.GetServerVersion();

            return _version;
        }

        public string GetMinSupportedVersion()
        {
            if (_minversion == null)
            {
                if (!EnvironmentConnection.IsConnected)
                {
                    EnvironmentConnection.Connect(Guid.Empty);
                }
                _minversion = ProxyLayer.AdminManagerProxy.GetMinSupportedServerVersion();
            }

            return _minversion;
        }


        void RaiseNetworkStateChangeEvent(object sender, System.Network.NetworkStateEventArgs e)
        {
            NetworkStateChanged?.Invoke(new NetworkStateChangedEventArgs(e), this);
        }

        void RaisePermissionsModifiedEvent(object sender, List<WindowsGroupPermission> windowsGroupPermissions)
        {
            PermissionsChanged?.Invoke(new PermissionsChangedArgs(windowsGroupPermissions.Cast<IWindowsGroupPermission>().ToList()));
            Permissions = windowsGroupPermissions.Select(permission => permission as IWindowsGroupPermission).ToList();
        }

        #region Implementation of IServer

        public void Connect()
        {
            if (!EnvironmentConnection.IsConnected)
            {
                EnvironmentConnection.Connect(_serverId);
                OnPropertyChanged("IsConnected");
                OnPropertyChanged("DisplayName");
            }
        }

        public async Task<bool> ConnectAsync()
        {
            var connected = await EnvironmentConnection.ConnectAsync(_serverId);
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("DisplayName");
            return connected;
        }

        public string DisplayName
        {
            get
            {
                if (EnvironmentConnection != null)
                {
                    var displayName = EnvironmentConnection.DisplayName;
                    if (IsConnected && (HasLoaded || EnvironmentConnection.IsLocalHost))
                    {
                        if (!displayName.Contains(Resources.Languages.Core.ConnectedLabel))
                        {
                            displayName += Resources.Languages.Core.ConnectedLabel;
                        }
                    }
                    else if (!IsConnected && (HasLoaded || EnvironmentConnection.IsLocalHost))
                    {
                        displayName = EnvironmentConnection.DisplayName.Replace("(Connected)", "");
                    }
                    return displayName;
                }

                return EnvironmentConnection?.DisplayName ?? Resources.Languages.Core.NewServerLabel;
            }
            // ReSharper disable once ValueParameterNotUsed
            set
            {
                EnvironmentConnection.DisplayName = DisplayName;
                OnPropertyChanged();
            }
        }

        public async Task<IExplorerItem> LoadExplorer(bool reloadCatalogue = false)
        {
            var result = await ProxyLayer.LoadExplorer(reloadCatalogue);
            HasLoaded = true;
            return result;
        }

        public Task<List<string>> LoadExplorerDuplicates()
        {
            var result = ProxyLayer.LoadExplorerDuplicates();
            HasLoaded = true;
            return result;
        }

        public IList<IServer> GetServerConnections()
        {
            var environmentModels = EnvironmentRepository.Instance.ReloadServers();
            return environmentModels.Select(environmentModel => new Server(environmentModel)).Cast<IServer>().ToList();
        }

        public IServer FetchServer(Guid savedServerID)
        {
            var environmentModels = EnvironmentRepository.Instance.ReloadAllServers();
            var requiredEnv = environmentModels.FirstOrDefault(model => model.ID == savedServerID);
            if (requiredEnv != null)
            {
                return new Server(requiredEnv);
            }
            return null;
        }

        public IList<IServer> GetAllServerConnections()
        {
            var environmentModels = EnvironmentRepository.Instance.ReloadAllServers();
            return environmentModels.Select(environmentModel => new Server(environmentModel)).Cast<IServer>().ToList();
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

        public IQueryManager QueryProxy => _proxyLayer.QueryManagerProxy;

        public bool IsConnected => EnvironmentConnection != null && EnvironmentConnection.IsConnected;

        public bool AllowEdit => EnvironmentConnection != null && !EnvironmentConnection.IsLocalHost;

        public void Disconnect()
        {
            EnvironmentConnection.Disconnect();
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("DisplayName");
        }

        public List<IWindowsGroupPermission> Permissions
        {
            get
            {
                if (_permissions == null)
                {
                    try
                    {
                        if (IsConnected)
                        {
                            _permissions = ProxyLayer.QueryManagerProxy.FetchPermissions();
                            PermissionsChanged?.Invoke(new PermissionsChangedArgs(_permissions.ToList()));
                        }
                    }
                    catch (Exception)
                    {

                        //ignore
                    }
                }
                return _permissions;
            }
            set
            {
                _permissions = value;
            }
        }

        public event PermissionsChanged PermissionsChanged;
        public event NetworkStateChanged NetworkStateChanged;
        public event ItemAddedEvent ItemAddedEvent;

        public IStudioUpdateManager UpdateRepository { get; }

        public IExplorerRepository ProxyLayer => _proxyLayer;
        public IEnvironmentConnection EnvironmentConnection { get; set; }

        public IEnvironmentModel EnvironmentModel => _environmentModel;

        #endregion

        #region Overrides of Resource

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return DisplayName;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}