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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Studio.AntiCorruptionLayer;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Warewolf.Studio.ViewModels
{
    public class ConnectControlViewModel : BindableBase, IConnectControlViewModel, IUpdatesHelp
    {
        bool _isConnected;
        bool _isConnecing;
        IServer _selectedConnection;
        bool _allowConnection;
        ObservableCollection<IServer> _servers;
        bool _isLoading;
        private Guid? _selectedId;
        private bool _shouldUpdateActiveEnvironment;

        public ConnectControlViewModel(IServer server, IEventAggregator aggregator)
        {
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server));
            }
            if (aggregator == null)
            {
                throw new ArgumentNullException(nameof(aggregator));
            }
            Server = server;
            LoadServers();
            SelectedConnection = server;
            EditConnectionCommand = new DelegateCommand(AllowConnectionEdit,CanExecuteMethod);
            ToggleConnectionStateCommand = new DelegateCommand(CheckVersionConflict);
            if (Server.UpdateRepository != null)
            {
                Server.UpdateRepository.ServerSaved += UpdateRepositoryOnServerSaved;
            }
            ShouldUpdateActiveEnvironment = false;
        }

        public bool ShouldUpdateActiveEnvironment
        {
            get { return _shouldUpdateActiveEnvironment; }
            set
            {
                _shouldUpdateActiveEnvironment = value; 
                
            }
        }

        private bool CanExecuteMethod()
        {
            return SelectedConnection.EnvironmentID != Guid.Empty;
        }

        void UpdateRepositoryOnServerSaved(Guid savedServerID)
        {
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
            }
            var updatedServer = Server.FetchServer(savedServerID);
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
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            SelectedConnection = shellViewModel?.LocalhostServer;
        }

        public void LoadServers()
        {
            var serverConnections = Server.GetAllServerConnections();
            var servers = new ObservableCollection<IServer> { CreateNewRemoteServerEnvironment() };
            if(serverConnections != null)
            {
                servers.AddRange(serverConnections);
            }
            if (Servers == null)
            {
                Servers = new ObservableCollection<IServer>();
            }
            RemoveServerDisconnect();
            Servers.Clear();
            Servers.AddRange(servers);
            SetupServerDisconnect();
            if (_selectedId != null && _selectedId!=Guid.Empty)
            {
                var selectConnection = Servers.FirstOrDefault(server => server.EnvironmentID == _selectedId);
                SelectedConnection = null;
                SelectedConnection = selectConnection;
            }
        }

        private void SetupServerDisconnect()
        {
            foreach(var server in Servers)
            {
                server.NetworkStateChanged += OnServerOnNetworkStateChanged;
            }
        }
        private void RemoveServerDisconnect()
        {
            foreach (var server in Servers)
            {
                server.NetworkStateChanged -= OnServerOnNetworkStateChanged;
            }
        }
        private void OnServerOnNetworkStateChanged(INetworkStateChangedEventArgs args, IServer server1)
        {
            if (args.State != ConnectionNetworkState.Connecting && args.State != ConnectionNetworkState.Connected && !server1.IsConnected)
            {

                if (SelectedConnection.EnvironmentID == server1.EnvironmentID)
                {
                    if (!IsConnecting && server1.EnvironmentID == Guid.Empty)
                    {
                        ServerHasDisconnected(this, server1);
                    }
                    SelectedConnection.DisplayName = SelectedConnection.ResourceName;
                    IsConnected = false;
                    ServerDisconnected?.Invoke(this, SelectedConnection);
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

        IServer CreateNewRemoteServerEnvironment()
        {
            return new Server
            {
                ResourceName = Resources.Languages.Core.NewServerLabel,
                EnvironmentID = Guid.NewGuid()
            };
        }

        void AllowConnectionEdit()
        {
            if (SelectedConnection == null)
            {
                return;
            }
            if (SelectedConnection.AllowEdit)
            {
                Edit();
            }
        }

        private async Task ConnectOrDisconnect()
        {
            if (SelectedConnection == null)
            {
                return;
            }
            
            if (SelectedConnection.IsConnected && SelectedConnection.HasLoaded)
            {
                IsLoading = false;
                IsConnecting = false;
                Disconnect(SelectedConnection);
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
                await Connect(SelectedConnection);
                IsConnected = SelectedConnection.IsConnected;
                IsConnecting = false;
                IsLoading = false;
                var mainViewModel = CustomContainer.Get<IShellViewModel>();
                SetActiveEnvironment(mainViewModel);
            }
        }

        async void CheckVersionConflict()
        {
            try
            {

                IsLoading = true;
                IsConnecting = true;
                await ConnectOrDisconnect();
                if (SelectedConnection.IsConnected)
                {
                    Version sourceVersionNumber;
                    Version.TryParse(SelectedConnection.GetServerVersion(), out sourceVersionNumber);
                    Version destVersionNumber;
                    Version.TryParse(Resources.Languages.Core.CompareCurrentServerVersion, out destVersionNumber);
                    if (sourceVersionNumber != null && destVersionNumber != null)
                    {
                        if (sourceVersionNumber < destVersionNumber)
                        {
                            var popupController = CustomContainer.Get<IPopupController>();
                            popupController.ShowConnectServerVersionConflict(sourceVersionNumber.ToString(),
                                destVersionNumber.ToString());
                            Disconnect(SelectedConnection);
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        private IServer Server { get; set; }
        public ObservableCollection<IServer> Servers
        {
            get
            {
                return _servers;
            }
            private set
            {
                _servers = value;
                OnPropertyChanged(() => Servers);
            }
        }
        public IServer SelectedConnection
        {
            get
            {
                return _selectedConnection;
            }
            set
            {
                if (value != null)
                {
                    var mainViewModel = CustomContainer.Get<IShellViewModel>();
                    UpdateBasedOnSelection(value, mainViewModel);
                    SetActiveEnvironment(mainViewModel);
                    OnPropertyChanged(() => SelectedConnection);
                    SelectedEnvironmentChanged?.Invoke(this, value.EnvironmentID);
                    var delegateCommand = EditConnectionCommand as DelegateCommand;
                    delegateCommand?.RaiseCanExecuteChanged();                    
                }
            }
        }

        private void UpdateBasedOnSelection(IServer value, IShellViewModel mainViewModel)
        {
            if (value.ResourceName != null && value.ResourceName.Equals(Resources.Languages.Core.NewServerLabel))
            {
                NewServer(mainViewModel);
            }
            else
            {
                _selectedConnection = value;
                AllowConnection = true;
                DisallowConnectionForLocalhost();
                IsConnected = _selectedConnection.IsConnected && _selectedConnection.HasLoaded;
            }
        }

        private void SetActiveEnvironment(IShellViewModel mainViewModel)
        {
            if (mainViewModel != null)
            {
                if (_selectedConnection?.ResourceName != null)
                {
                    if (ShouldUpdateActiveEnvironment && !_selectedConnection.ResourceName.Equals(Resources.Languages.Core.NewServerLabel))
                    {
                        mainViewModel.ShouldUpdateActiveState = _selectedConnection.IsConnected;
                        mainViewModel.SetActiveEnvironment(_selectedConnection.EnvironmentID);
                        mainViewModel.SetActiveServer(_selectedConnection);
                    }
                }
            }
        }

        private void DisallowConnectionForLocalhost()
        {
            if (_selectedConnection.EnvironmentID == Guid.Empty &&
                (_selectedConnection.ResourceName.Equals(Resources.Languages.Core.LocalhostLabel)
                 || _selectedConnection.ResourceName.Equals(Resources.Languages.Core.LocalhostConnectedLabel)
                    ))
            {
                AllowConnection = false;
            }
        }

        private void NewServer(IShellViewModel mainViewModel)
        {
            if (mainViewModel != null && ShouldUpdateActiveEnvironment)
            {
                mainViewModel.SetActiveEnvironment(_selectedConnection.EnvironmentID);
            }
            mainViewModel?.NewServerSource(string.Empty);
            IsConnected = false;
            AllowConnection = false;
            OnPropertyChanged(() => SelectedConnection);
        }

        public ICommand EditConnectionCommand { get; private set; }
        public ICommand ToggleConnectionStateCommand { get; private set; }
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            private set
            {
                _isConnected = value;
                OnPropertyChanged(() => IsConnected);
            }
        }
        public bool IsConnecting
        {
            get
            {
                return _isConnecing;
            }
            private set
            {
                _isConnecing = value;
                OnPropertyChanged(() => IsConnecting);                
            }
        }
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged(() => IsLoading);
            }
        }
        public bool AllowConnection
        {
            get
            {
                return _allowConnection;
            }
            private set
            {
                _allowConnection = value;
                OnPropertyChanged(() => AllowConnection);
            }
        }

        public async Task<bool> Connect(IServer connection)
        {
            if (connection != null)
            {
                try
                {
                    var connected = await connection.ConnectAsync();
                    if (connected)
                    {
                        if (ShouldUpdateActiveEnvironment)
                        {
                            SetActiveServer(connection);
                        }
                    }
                    else
                    {
                        var popupController = CustomContainer.Get<IPopupController>();
                        var result = popupController?.ShowConnectionTimeoutConfirmation(connection.DisplayName);
                        if (result == MessageBoxResult.Yes)
                        {
                            await Connect(connection);
                        }
                        else
                        {
                            ServerDisconnected?.Invoke(this, connection);
                        }
                    }
                    OnPropertyChanged(() => connection.IsConnected);
                    if (ServerConnected != null && connected)
                    {
                        ServerConnected(this, connection);
                        if (ShouldUpdateActiveEnvironment)
                        {
                            SetActiveServer(connection);
                        }
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private static void SetActiveServer(IServer connection)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.SetActiveEnvironment(connection.EnvironmentID);
            mainViewModel?.SetActiveServer(connection);
            mainViewModel?.OnActiveEnvironmentChanged();
        }

        private void Disconnect(IServer connection)
        {
            if (connection != null)
            {
                connection.Disconnect();
                OnPropertyChanged(() => connection.IsConnected);
                ServerDisconnected?.Invoke(this, connection);
            }
        }
        
        public void Edit()
        {
            if (SelectedConnection != null)
            {
                var server = SelectedConnection as Server;
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                if (server != null)
                {
                    _selectedId = SelectedConnection?.EnvironmentID;
                    if (_selectedId != null)
                    {
                        shellViewModel?.OpenResource(SelectedConnection.ResourceID, EnvironmentRepository.Instance.Source.ID, shellViewModel.LocalhostServer);
                        SelectedConnection = shellViewModel?.LocalhostServer;
                    }
                }
            }
        }

        public event SelectedServerChanged SelectedEnvironmentChanged;

        public string ToggleConnectionToolTip => Resources.Languages.Tooltips.ConnectControlToggleConnectionToolTip;
        public string EditConnectionToolTip => Resources.Languages.Tooltips.ConnectControlEditConnectionToolTip;
        public string ConnectionsToolTip => Resources.Languages.Tooltips.ConnectControlConnectionsToolTip;
        public EventHandler<IServer> ServerConnected { get; set; }
        public EventHandler<IServer> ServerHasDisconnected { get; set; }
        public EventHandler<IServer> ServerReConnected { get; set; }
        public EventHandler<IServer> ServerDisconnected { get; set; }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}