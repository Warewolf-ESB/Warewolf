#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Resource.Errors;
using Dev2.Instrumentation;

namespace Warewolf.Studio.ViewModels
{
    public class ConnectControlViewModel : BindableBase, IConnectControlViewModel, IUpdatesHelp
    {
        bool _isConnected;
        bool _isConnecting;
        IServer _selectedConnection;
        ObservableCollection<IServer> _servers;
        bool _isLoading;
        Guid? _selectedId;
        readonly ObservableCollection<IServer> _existingServers;
        public IPopupController PopupController { get; set; }
        readonly IServerRepository _serverRepository;
        bool _canEditServer;
        bool _canCreateServer;
        private readonly IApplicationTracker _applicationTracker;

        public ConnectControlViewModel(IServer server, IEventAggregator aggregator)
            : this(server, aggregator, null, null)
        {
        }

        public ConnectControlViewModel(IServer server, IEventAggregator aggregator, ObservableCollection<IServer> servers)
            : this(server, aggregator, null, servers)
        {
        }

        public ConnectControlViewModel(IServer server, IEventAggregator aggregator, IPopupController popupController)
            : this(server, aggregator, popupController, null)
        {
        }

        public ConnectControlViewModel(IServer server, IEventAggregator aggregator, IPopupController popupController, ObservableCollection<IServer> servers)
        {
            PopupController = popupController;
            if (aggregator == null)
            {
                throw new ArgumentNullException(nameof(aggregator));
            }
            Server = server ?? throw new ArgumentNullException(nameof(server));
            _existingServers = servers;
            _serverRepository = CustomContainer.Get<IServerRepository>();
            LoadServers();
            SelectedConnection = server;
            EditConnectionCommand = new DelegateCommand(AllowConnectionEdit, CanExecuteMethod);
            NewConnectionCommand = new DelegateCommand(NewServer);
            if (Server.UpdateRepository != null)
            {
                Server.UpdateRepository.ServerSaved += UpdateRepositoryOnServerSaved;
            }
            ShouldUpdateActiveEnvironment = false;
            CanEditServer = true;
            CanCreateServer = true;
            _applicationTracker = CustomContainer.Get<IApplicationTracker>();
        }

        public bool ShouldUpdateActiveEnvironment { get; set; }

        public bool CanEditServer
        {
            get => _canEditServer;
            set
            {
                _canEditServer = value;
                OnPropertyChanged(() => CanEditServer);
            }
        }

        public bool CanCreateServer
        {
            get => _canCreateServer;
            set
            {
                _canCreateServer = value;
                OnPropertyChanged(() => CanEditServer);
            }
        }

        bool CanExecuteMethod() => SelectedConnection?.EnvironmentID != Guid.Empty;

        void UpdateRepositoryOnServerSaved(Guid savedServerID, bool isDeleted = false)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var currentServer = Servers.FirstOrDefault(server => server.EnvironmentID == savedServerID);
            var idx = -1;
            if (currentServer != null)
            {
                if (currentServer.IsConnected)
                {
                    currentServer.Disconnect();
                    ServerDisconnected?.Invoke(this, currentServer);
                }
                idx = Servers.IndexOf(currentServer);
                currentServer.NetworkStateChanged -= OnServerOnNetworkStateChanged;
                Servers.Remove(currentServer);

                var environmentViewModel = shellViewModel?.ExplorerViewModel?.Environments?.FirstOrDefault(model => model.ResourceId == currentServer.EnvironmentID);
                if (environmentViewModel != null)
                {
                    shellViewModel.ExplorerViewModel?.Environments?.Remove(environmentViewModel);
                }
            }
            if (isDeleted)
            {
                return;
            }
            var updatedServer = _serverRepository.Get(savedServerID);
            if (updatedServer == null)
            {
                return;
            }
            if (idx == -1)
            {
                idx = Servers.Count - 1;
            }
            updatedServer.NetworkStateChanged += OnServerOnNetworkStateChanged;
            Servers.Insert(idx, updatedServer);
            SelectedConnection = shellViewModel?.LocalhostServer;
        }

        public void LoadServers()
        {
            var servers = new ObservableCollection<IServer>();
            if (_existingServers != null)
            {
                servers = _existingServers;
            }
            else
            {
                var serverConnections = _serverRepository.All();
                if (serverConnections != null)
                {
                    servers.AddRange(serverConnections);
                }
            }

            if (Servers == null)
            {
                Servers = new ObservableCollection<IServer>();
            }
            RemoveServerDisconnect();
            Servers.Clear();
            Servers.AddRange(servers);
            SetupServerDisconnect();
            if (_selectedId != null && _selectedId != Guid.Empty)
            {
                var selectConnection = Servers.FirstOrDefault(server => server.EnvironmentID == _selectedId);
                SelectedConnection = null;
                SelectedConnection = selectConnection;
            }
        }

        void SetupServerDisconnect()
        {
            foreach (var server in Servers)
            {
                server.NetworkStateChanged += OnServerOnNetworkStateChanged;
            }
        }
        void RemoveServerDisconnect()
        {
            foreach (var server in Servers)
            {
                server.NetworkStateChanged -= OnServerOnNetworkStateChanged;
            }
        }
        void OnServerOnNetworkStateChanged(INetworkStateChangedEventArgs args, IServer server1)
        {
            if (args.State != ConnectionNetworkState.Connecting && args.State != ConnectionNetworkState.Connected && !server1.IsConnected)
            {
                if (SelectedConnection.EnvironmentID == server1.EnvironmentID)
                {
                    if (!IsConnecting && server1.EnvironmentID == Guid.Empty)
                    {
                        ServerHasDisconnected?.Invoke(this, server1);
                    }
                    IsConnected = false;
                    Application.Current.Dispatcher.Invoke(() => ServerDisconnected?.Invoke(this, SelectedConnection));
                    try
                    {
                        PopupController?.Show(ErrorResource.ServerconnectionDropped + Environment.NewLine + ErrorResource.EnsureConnectionToServerWorking
                            , ErrorResource.ServerDroppedErrorHeading, MessageBoxButton.OK, MessageBoxImage.Information, "", false, false, true, false, false, false);
                    }
                    catch (InvalidOperationException e)
                    {
                        if (e.Message != "The calling thread must be STA, because many UI components require this.")
                        {
                            throw e;
                        }
                    }
                }
            }
            else
            {
                if (args.State == ConnectionNetworkState.Connected)
                {
                    ServerReConnected?.Invoke(this, server1);
                }
            }
        }

        void AllowConnectionEdit()
        {
            if (SelectedConnection != null && SelectedConnection.AllowEdit)
            {
                Edit();
            }
        }

        async Task<bool> ConnectOrDisconnectAsync()
        {
            var isConnected = false;
            if (_selectedConnection == null)
            {
                return false;
            }

            if (_selectedConnection.IsConnected && _selectedConnection.HasLoaded)
            {
                IsLoading = false;
                IsConnecting = false;
                var localhostServer = Servers.FirstOrDefault(server => server.EnvironmentID == Guid.Empty);
                if (localhostServer != null)
                {
                    SelectedConnection = localhostServer;
                }
                IsConnected = false;
            }
            else
            {
                IsConnecting = true;
                IsConnected = false;
                IsLoading = true;
                isConnected = await TryConnectAsync(_selectedConnection).ConfigureAwait(true);
                IsConnected = _selectedConnection.IsConnected;
                IsConnecting = false;
                SetActiveEnvironment();
            }
            return isConnected;
        }

        async Task CheckVersionConflictAsync()
        {
            try
            {
                IsConnecting = true;
                var isConnected = await ConnectOrDisconnectAsync().ConfigureAwait(true);
                if (_selectedConnection.IsConnected && isConnected)
                {
                    Version.TryParse(_selectedConnection.GetServerVersion(), out Version sourceVersionNumber);
                    Version.TryParse(Resources.Languages.Core.CompareCurrentServerVersion, out Version destVersionNumber);
                    if (sourceVersionNumber != null && destVersionNumber != null && sourceVersionNumber < destVersionNumber)
                    {
                        PopupController.ShowConnectServerVersionConflict(sourceVersionNumber.ToString(),
                            destVersionNumber.ToString());
                    }

                }
            }
            catch
            {
                // ignored
            }
        }

        IServer Server { get; set; }
        public ObservableCollection<IServer> Servers
        {
            get => _servers;
            private set
            {
                _servers = value;
                OnPropertyChanged(() => Servers);
            }
        }
        public IServer SelectedConnection
        {
            get => _selectedConnection;
            set
            {
                if (value != null && !Equals(_selectedConnection, value))
                {
                    _selectedConnection = value;
                    if (value.EnvironmentID != Guid.Empty && !value.IsConnected)
                    {
                        var isConnected = CheckVersionConflictAsync();
                    }
                    // revulytics log if ware wolf store selected
                    if (!value.IsLocalHost && value.Connection.AppServerUri.ToString().Contains("store.warewolf.io"))
                    {
                        _applicationTracker?.TrackEvent(Resources.Languages.TrackEventMenu.EventCategory, Resources.Languages.TrackEventMenu.WarewolfStore);
                    }
                    SetActiveEnvironment();
                    OnPropertyChanged(() => SelectedConnection);
                    SelectedEnvironmentChanged?.Invoke(this, value.EnvironmentID);
                    var delegateCommand = EditConnectionCommand as DelegateCommand;
                    delegateCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        void SetActiveEnvironment()
        {
            if (_selectedConnection?.DisplayName != null)
            {
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                if (mainViewModel != null && ShouldUpdateActiveEnvironment)
                {
                    mainViewModel.ShouldUpdateActiveState = _selectedConnection.IsConnected;
                    mainViewModel.SetActiveServer(_selectedConnection.EnvironmentID);
                }
            }
        }

        void NewServer()
        {
            if (_applicationTracker != null)
            {
                _applicationTracker.TrackEvent(Resources.Languages.TrackEventMenu.EventCategory, Resources.Languages.TrackEventMenu.NewRemoteServer);
            }
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            if (mainViewModel != null && ShouldUpdateActiveEnvironment)
            {
                mainViewModel.SetActiveServer(_selectedConnection.EnvironmentID);
            }
            mainViewModel?.NewServerSource(string.Empty);
            IsConnected = false;
            OnPropertyChanged(() => SelectedConnection);
        }
        public ICommand EditConnectionCommand { get; private set; }
        public ICommand NewConnectionCommand { get; private set; }
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                _isConnected = value;
                OnPropertyChanged(() => IsConnected);
            }
        }
        public bool IsConnecting
        {
            get => _isConnecting;
            private set
            {
                _isConnecting = value;
                OnPropertyChanged(() => IsConnecting);
            }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(() => IsLoading);
            }
        }

        public async Task<bool> TryConnectAsync(IServer connection)
        {
            if (connection != null)
            {
                try
                {
                    await ConnectAsync(connection).ConfigureAwait(true); 

                    return _selectedConnection.IsConnected;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }
        async Task ConnectAsync(IServer connection)
        {
            var connected = await connection.ConnectAsync().ConfigureAwait(true);
            if (connected && connection.IsConnected)
            {
                if (ShouldUpdateActiveEnvironment)
                {
                    SetActiveServer(connection);
                }
            }
            else
            {
                var result = PopupController?.ShowConnectionTimeoutConfirmation(connection.DisplayName);
                if (result == MessageBoxResult.Yes)
                {
                    await TryConnectAsync(connection).ConfigureAwait(true);
                }
                else
                {
                    IsConnected = _selectedConnection.IsConnected;
                    IsConnecting = false;
                    IsLoading = false;
                    ServerDisconnected?.Invoke(this, connection);
                }
            }
            OnPropertyChanged(() => connection.IsConnected);
            if (ServerConnected != null && connected && connection.IsConnected)
            {
                ServerConnected?.Invoke(this, connection);
                if (ShouldUpdateActiveEnvironment)
                {
                    SetActiveServer(connection);
                }
            }
        }

        static void SetActiveServer(IServer connection)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.SetActiveServer(connection.EnvironmentID);
            mainViewModel?.OnActiveServerChanged();
        }

        public void Edit()
        {
            if (SelectedConnection != null)
            {
                var server = SelectedConnection;
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                if (server != null)
                {
                    _selectedId = SelectedConnection?.EnvironmentID;
                    if (_selectedId != null)
                    {
                        shellViewModel?.OpenResource(SelectedConnection.EnvironmentID, _serverRepository.Source.EnvironmentID, shellViewModel.LocalhostServer);
                    }
                }
            }
        }

        public event SelectedServerChanged SelectedEnvironmentChanged;

        public string NewConnectionToolTip => Resources.Languages.Tooltips.ConnectControlNewConnectionToolTip;
        public string EditConnectionToolTip => Resources.Languages.Tooltips.ConnectControlEditConnectionToolTip;
        public string ConnectionsToolTip => Resources.Languages.Tooltips.ConnectControlConnectionsToolTip;
        public EventHandler<IServer> ServerConnected { get; set; }
        public EventHandler<IServer> ServerHasDisconnected { get; set; }
        public EventHandler<IServer> ServerReConnected { get; set; }
        public EventHandler<IServer> ServerDisconnected { get; set; }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}