using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Runtime.ServiceModel;
using Dev2.Common.Interfaces.ServerDialogue;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Studio.AntiCorruptionLayer;

namespace Warewolf.Studio.ViewModels
{
    public class ConnectControlViewModel : BindableBase, IConnectControlViewModel
    {
        bool _isConnected;
        bool _isConnecing;

        public ConnectControlViewModel(IServer server,IEventAggregator aggregator)
        {
            if(server == null)
            {
                throw new ArgumentNullException("server");
            }
            if(aggregator == null)
            {
                throw  new ArgumentNullException("aggregator");
            }
            Server = server;
            Servers = Server.GetServerConnections();
            Servers = new List<IServer> { server };
            SelectedConnection = server;
            aggregator.GetEvent<ServerAddedEvent>().Subscribe(ServerAdded);
            EditConnectionCommand = new DelegateCommand(Edit);
            ToggleConnectionStateCommand = new DelegateCommand(() =>
            {
                if(SelectedConnection == null)
                {
                    return;
                }
                if (SelectedConnection.IsConnected())
                {
                    SelectedConnection.Disconnect();
                }
                else
                {
                    IsConnecting = true;
                    IsConnected = false;
                    SelectedConnection.Connect();
                    IsConnected = true;
                    IsConnecting = false;
                }
            });
        }

        void ServerAdded(IServerSource server)
        {
            if(server.AuthenticationType== AuthenticationType.User)
                Servers.Add(new Server(server.Address,server.UserName,server.Password){ResourceName = server.Name});
            else if(server.AuthenticationType == AuthenticationType.Windows)
            {
                Servers.Add(new Server(server.Address, new NetworkCredential()) { ResourceName = server.Name });
            }
            else
            {
                Servers.Add(new Server(new Uri(server.Address)) { ResourceName = server.Name });
            }
            OnPropertyChanged(()=>Servers);
        }

        public IServer Server { get; set; }
        public IList<IServer> Servers { get; set; }
        public IServer SelectedConnection { get; set; }
        public ICommand EditConnectionCommand { get; set; }
        public ICommand ToggleConnectionStateCommand { get; set; }
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
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
            set
            {
                _isConnecing = value;
                OnPropertyChanged(()=>IsConnecting);
               
            }
        }

        public void Connect(IServer connection)
        {
            if (connection != null)
            {
                connection.Connect();
            }
        }

        public void Disconnect(IServer connection)
        {
            if (connection != null)
            {
                connection.Disconnect();
            }
        }

        public void Refresh()
        {
            if (SelectedConnection != null)
            {
                SelectedConnection.Load();
            }
        }

        public void Edit()
        {
            if (SelectedConnection != null)
            {
                SelectedConnection.Edit();
            }
        }

        public string ToggleConnectionToolTip
        {
            get {   return Resources.Languages.Core.ConnectControlToggleConnectionToolTip;}
        }
        public string EditConnectionToolTip
        {
            get { return Resources.Languages.Core.ConnectControlEditConnectionToolTip; }
        }
        public string ConnectionsToolTip
        {
            get { return Resources.Languages.Core.ConnectControlConnectionsToolTip; }
        }
    }

    
}