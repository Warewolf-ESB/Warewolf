using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels.DummyModels;

namespace Warewolf.Studio.ViewModels
{
    public class ConnectControlViewModel : BindableBase, IConnectControlViewModel
    {
        bool _isConnected;
        bool _isConnecing;

        public ConnectControlViewModel(IServer server)
        {
            if(server == null)
            {
                throw new ArgumentNullException("server");
            }
            Server = server;
            Servers = Server.GetServerConnections();
            Servers = new List<IServer>();
            Servers.Add(new DummyServer());
            Servers.Add(server);
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
    }

    
}