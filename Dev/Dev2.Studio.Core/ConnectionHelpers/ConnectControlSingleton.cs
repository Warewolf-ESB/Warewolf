
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.AppResources.Repositories;
using Dev2.Network;
using Dev2.Studio.Core;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Threading;

namespace Dev2.ConnectionHelpers
{
    public class ConnectControlSingleton : IConnectControlSingleton
    {
        readonly IStudioResourceRepository _studioResourceRepository;
        readonly IAsyncWorker _asyncWorker;
        readonly IEnvironmentModelProvider _serverProvider;
        static IConnectControlSingleton _instance;
        readonly IEnvironmentRepository _environmentRepository;
        public const string NewServerText = "New Remote Server...";
        public event EventHandler<ConnectionStatusChangedEventArg> ConnectedStatusChanged;
        public event EventHandler<ConnectedServerChangedEvent> ConnectedServerChanged;

        public static IConnectControlSingleton Instance
        {
            get
            {
                return _instance ?? (_instance = new ConnectControlSingleton(StudioResourceRepository.Instance,
                                                 new AsyncWorker(),
                                                 ServerProvider.Instance,
                                                 EnvironmentRepository.Instance));
            }
        }

        internal ConnectControlSingleton(IStudioResourceRepository studioResourceRepository,
                                         IAsyncWorker asyncWorker,
                                         IEnvironmentModelProvider serverProvider,
                                         IEnvironmentRepository environmentRepository)
        {
            VerifyArgument.IsNotNull("studioResourceRepository", studioResourceRepository);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);
            VerifyArgument.IsNotNull("serverProvider", serverProvider);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            _studioResourceRepository = studioResourceRepository;
            _asyncWorker = asyncWorker;
            _serverProvider = serverProvider;
            _environmentRepository = environmentRepository;
            Servers = new ObservableCollection<IConnectControlEnvironment>();
            LoadServers();
        }

        public ObservableCollection<IConnectControlEnvironment> Servers { get; set; }

        public void Remove(Guid environmentId)
        {
            var index = Servers.IndexOf(Servers.FirstOrDefault(s => s.EnvironmentModel.ID == environmentId));

            if(index != -1)
            {
                var selectedServer = Servers[index];
                if(selectedServer.IsConnected)
                {
                    Disconnect(selectedServer.EnvironmentModel);
                }

                _studioResourceRepository.RemoveEnvironment(environmentId);

                if(ConnectedServerChanged != null)
                {
                    var localhost = Servers.FirstOrDefault(s => s.EnvironmentModel.IsLocalHost);
                    Guid localhostId = localhost == null ? Guid.Empty : localhost.EnvironmentModel.ID;
                    ConnectedServerChanged(this, new ConnectedServerChangedEvent(localhostId));
                }
            }
        }

        public void Refresh(Guid environmentId)
        {
            var selectedEnvironment = Servers.FirstOrDefault(s => s.EnvironmentModel.ID == environmentId);
            if(selectedEnvironment != null)
            {
                var index = Servers.IndexOf(selectedEnvironment);
                if(index != -1)
                {
                    Connect(selectedEnvironment);
                }
            }
        }

        public void ToggleConnection(int selectedIndex)
        {
            if(selectedIndex != -1 && selectedIndex <= Servers.Count)
            {
                var selectedServer = Servers[selectedIndex];
                if(selectedServer != null)
                {
                    var environment = selectedServer.EnvironmentModel;
                    if(selectedServer.IsConnected)
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
            var connectControlEnvironment = Servers.FirstOrDefault(s => s.EnvironmentModel.ID == environmentId);
            var index = Servers.IndexOf(connectControlEnvironment);

            if(index != -1)
            {
                ToggleConnection(index);
            }
        }

        public void SetConnectionState(Guid environmentId, ConnectionEnumerations.ConnectedState connectedState)
        {
            if(ConnectedStatusChanged != null)
            {
                ConnectedStatusChanged(this, new ConnectionStatusChangedEventArg(connectedState, environmentId, false));
            }
        }

        public void EditConnection(int selectedIndex, Action<int> openWizard)
        {
            if(selectedIndex != -1 && selectedIndex <= Servers.Count)
            {
                var selectedServer = Servers[selectedIndex];
                var environmentModel = selectedServer.EnvironmentModel;
                if(environmentModel != null && environmentModel.Connection != null)
                {
                    var serverUri = environmentModel.Connection.AppServerUri;
                    var auth = environmentModel.Connection.AuthenticationType;
                    openWizard(selectedIndex);
                    var updatedServer = _environmentRepository.All().FirstOrDefault(e => e.ID == environmentModel.ID);
                    if (updatedServer != null && (!serverUri.Equals(updatedServer.Connection.AppServerUri) || auth != updatedServer.Connection.AuthenticationType))
                    {
                        if(ConnectedStatusChanged != null)
                        {
                            ConnectedStatusChanged(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, environmentModel.ID, false));
                        }

                        if(selectedServer.IsConnected)
                        {
                            _studioResourceRepository.Disconnect(environmentModel.ID);
                        }
                        selectedServer.EnvironmentModel = updatedServer;
                        _studioResourceRepository.Load(environmentModel.ID, _asyncWorker, ResourcesLoadedHandler);
                    }
                }
            }
        }

        public void ResourcesLoadedHandler(Guid environmentId)
        {
            if(ConnectedStatusChanged == null)
            {
                return;
            }

            var server = _serverProvider.Load().FirstOrDefault(s => s.ID == environmentId);

            if(server != null && server.IsConnected)
            {
                ConnectedStatusChanged(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Connected, environmentId, true));
            }
            else
            {
                ConnectedStatusChanged(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Disconnected, environmentId, true));
            }
        }

        private void Disconnect(IEnvironmentModel environment)
        {
            if(ConnectedStatusChanged != null)
            {
                ConnectedStatusChanged(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, environment.ID, false));
            }
            _studioResourceRepository.Disconnect(environment.ID);
            if(ConnectedStatusChanged != null)
            {
                ConnectedStatusChanged(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Disconnected, environment.ID, true));
            }
        }

        private void Connect(IConnectControlEnvironment selectedServer)
        {
            var environmentId = selectedServer.EnvironmentModel.ID;
            if(ConnectedStatusChanged != null)
            {
                ConnectedStatusChanged(this, new ConnectionStatusChangedEventArg(ConnectionEnumerations.ConnectedState.Busy, environmentId, false));
            }
            _studioResourceRepository.Load(environmentId, _asyncWorker, ResourcesLoadedHandler);
        }

        ConnectControlEnvironment CreateNewRemoteServerEnvironment()
        {
            return new ConnectControlEnvironment
            {
                EnvironmentModel = new EnvironmentModel(Guid.NewGuid(), new ServerProxy(new Uri("http://localhost:3142"))) { Name = NewServerText }
            };
        }

        void LoadServers()
        {
            Servers.Clear();
            Servers.Add(CreateNewRemoteServerEnvironment());
            var servers = _serverProvider.Load();
            foreach(var server in servers)
            {
                Servers.Add(new ConnectControlEnvironment
                {
                    EnvironmentModel = server,
                    IsConnected = server.IsConnected,
                    AllowEdit = !server.IsLocalHost
                });
            }
        }
    }
}
