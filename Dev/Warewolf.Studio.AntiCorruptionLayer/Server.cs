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
        readonly StudioServerProxy _proxyLayer;
        IList<IToolDescriptor> _tools;
        readonly IEnvironmentModel _environmentModel;
        bool _hasloaded;
        string _version;
        private string _minversion;
        private List<IWindowsGroupPermission> _permissions;

        public Server(IEnvironmentModel environmentModel)
        {
            EnvironmentConnection = environmentModel.Connection;
            EnvironmentID = environmentModel.ID;
            _serverId = EnvironmentConnection.ServerID;
            var communicationControllerFactory = new CommunicationControllerFactory();
            _proxyLayer = new StudioServerProxy(communicationControllerFactory, EnvironmentConnection);
            UpdateRepository = new StudioResourceUpdateManager(communicationControllerFactory, EnvironmentConnection);
            EnvironmentConnection.PermissionsModified += RaisePermissionsModifiedEvent;
            ResourceName = EnvironmentConnection.DisplayName;
            EnvironmentConnection.NetworkStateChanged += RaiseNetworkStateChangeEvent;
            EnvironmentConnection.ItemAddedMessageAction += ItemAdded;
            environmentModel.WorkflowSaved += (sender, args) => UpdateRepository.FireItemSaved();
            _environmentModel = environmentModel;
        }

        public bool CanDeployTo => _environmentModel.IsAuthorizedDeployTo;

        public bool CanDeployFrom => _environmentModel.IsAuthorizedDeployFrom;

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

        public string GetServerVersion()
        {
            if (_version == null)
            {
                if (!EnvironmentConnection.IsConnected)
                {
                    EnvironmentConnection.Connect(Guid.Empty);
                }
                _version = ProxyLayer.AdminManagerProxy.GetServerVersion();
            }

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
                var displayName = Resources.Languages.Core.NewServerLabel;
                if (EnvironmentConnection != null)
                {
                    displayName = EnvironmentConnection.DisplayName;
                    if (IsConnected && (HasLoaded || EnvironmentConnection.IsLocalHost))
                    {
                        if (!displayName.Contains(Resources.Languages.Core.ConnectedLabel))
                        {
                            displayName += Resources.Languages.Core.ConnectedLabel;
                        }
                    }
                }

                return displayName;
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

        public IList<IServer> GetServerConnections()
        {
            var environmentModels = EnvironmentRepository.Instance.ReloadServers();
            return environmentModels.Select(environmentModel => new Server(environmentModel)).Cast<IServer>().ToList();
        }

        public IList<IToolDescriptor> LoadTools()
        {
            return _tools ?? (_tools = ProxyLayer.QueryManagerProxy.FetchTools());
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