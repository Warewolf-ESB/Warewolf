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
using Dev2.Network;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;


namespace Dev2.ConnectionHelpers
{
    public class ConnectControlSingleton : IConnectControlSingleton
    {
        readonly IEnvironmentModelProvider _serverProvider;
        static IConnectControlSingleton _instance;
        readonly IServerRepository _serverRepository;
        public const string NewServerText = "New Remote Server...";
        public event EventHandler<ConnectionStatusChangedEventArg> ConnectedStatusChanged;
        public event EventHandler<ConnectedServerChangedEvent> ConnectedServerChanged;
        public event EventHandler<ConnectedServerChangedEvent> AfterReload;
        public static IConnectControlSingleton Instance
        {
            get
            {
                return _instance ?? (_instance = new ConnectControlSingleton(ServerProvider.Instance, CustomContainer.Get<IServerRepository>()));
            }
        }

        public ConnectControlSingleton(IEnvironmentModelProvider serverProvider, IServerRepository serverRepository)
        {
            VerifyArgument.IsNotNull("serverProvider", serverProvider);
            VerifyArgument.IsNotNull("environmentRepository", serverRepository);
            _serverProvider = serverProvider;
            _serverRepository = serverRepository;
            Servers = new ObservableCollection<IConnectControlEnvironment>();
            LoadServers();
        }

        public ObservableCollection<IConnectControlEnvironment> Servers { get; set; }

        public void Remove(Guid environmentId)
        {
            var index = Servers.IndexOf(Servers.FirstOrDefault(s => s.Server.EnvironmentID == environmentId));

            if (index != -1)
            {
                var selectedServer = Servers[index];
                if (selectedServer.IsConnected)
                {
                    Disconnect(selectedServer.Server);
                }


                if (ConnectedServerChanged != null)
                {
                    var localhost = Servers.FirstOrDefault(s => s.Server.IsLocalHost);
                    Guid localhostId = localhost?.Server.EnvironmentID ?? Guid.Empty;
                    ConnectedServerChanged(this, new ConnectedServerChangedEvent(localhostId));
                }
            }
        }


        public void EditConnection(int selectedIndex, Action<int> openWizard)
        {
            if (selectedIndex != -1 && selectedIndex <= Servers.Count)
            {
                var selectedServer = Servers[selectedIndex];
                var environmentModel = selectedServer.Server;
                if (environmentModel?.Connection != null)
                {
                    var serverUri = environmentModel.Connection.AppServerUri;
                    var auth = environmentModel.Connection.AuthenticationType;
                    openWizard(selectedIndex);
                    var updatedServer = _serverRepository.All().FirstOrDefault(e => e.EnvironmentID == environmentModel.EnvironmentID);
                    if (updatedServer != null && (!serverUri.Equals(updatedServer.Connection.AppServerUri) || auth != updatedServer.Connection.AuthenticationType))
                    {
                        ConnectedStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, environmentModel.EnvironmentID, false));

                        selectedServer.Server = updatedServer;
                    }
                }
            }
        }

        public void Refresh(Guid environmentId)
        {
            var selectedEnvironment = Servers.FirstOrDefault(s => s.Server.EnvironmentID == environmentId);
            if (selectedEnvironment != null)
            {
                var index = Servers.IndexOf(selectedEnvironment);
                if (index != -1)
                {
                    Connect(selectedEnvironment);
                }
            }
        }

        public void ToggleConnection(int selectedIndex)
        {
            if (selectedIndex != -1 && selectedIndex <= Servers.Count)
            {
                var selectedServer = Servers[selectedIndex];
                if (selectedServer != null)
                {
                    var environment = selectedServer.Server;
                    if (selectedServer.IsConnected)
                    {
                        Disconnect(environment);
                    }
                    else
                    {
                        Connect(selectedServer);
                    }
                }
            }
        }

        public void ToggleConnection(Guid environmentId)
        {
            var connectControlEnvironment = Servers.FirstOrDefault(s => s.Server.EnvironmentID == environmentId);
            var index = Servers.IndexOf(connectControlEnvironment);

            if (index != -1)
            {
                ToggleConnection(index);
            }
        }

        public void SetConnectionState(Guid environmentId, ConnectionEnumerations.ConnectedState connectedState)
        {
            ConnectedStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArg(connectedState, environmentId, false));
        }

        private void Disconnect(IServer environment)
        {
            ConnectedStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, environment.EnvironmentID, false));
            ConnectedStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Disconnected, environment.EnvironmentID, true));
        }

        private void Connect(IConnectControlEnvironment selectedServer)
        {
            var environmentId = selectedServer.Server.EnvironmentID;
            ConnectedStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, environmentId, false));
        }

        ConnectControlEnvironment CreateNewRemoteServerEnvironment()
        {
            return new ConnectControlEnvironment
            {
                Server = new Server(Guid.NewGuid(), new ServerProxy(new Uri("http://localhost:3142"))) { Name = NewServerText }
            };
        }

        public void ReloadServer()
        {
            Servers.Clear();
            Servers.Add(CreateNewRemoteServerEnvironment());

            var servers = _serverProvider.ReloadServers();
            foreach (var server in servers)
            {
                Servers.Add(new ConnectControlEnvironment
                {
                    Server = server,
                    IsConnected = server.IsConnected,
                    AllowEdit = !server.IsLocalHost
                });
            }
            AfterReload?.Invoke(this, new ConnectedServerChangedEvent(Guid.Empty));
        }
        void LoadServers()
        {
            Servers.Clear();
            Servers.Add(CreateNewRemoteServerEnvironment());
            var servers = _serverProvider.Load();
            foreach (var server in servers)
            {
                Servers.Add(new ConnectControlEnvironment
                {
                    Server = server,
                    IsConnected = server.IsConnected,
                    AllowEdit = !server.IsLocalHost
                });
            }
        }
    }
}
