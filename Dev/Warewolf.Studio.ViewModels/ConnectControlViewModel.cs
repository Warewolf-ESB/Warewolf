using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    public class ConnectControlViewModel : IConnectControlViewModel
    {
        public ConnectControlViewModel(IServer server)
        {
            if(server == null)
            {
                throw new ArgumentNullException("server");
            }
            Server = server;
            Servers = Server.GetServerConnections();
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
                    SelectedConnection.Connect();
                }
            });
        }

        public IServer Server { get; set; }
        public IList<IServer> Servers { get; set; }
        public IServer SelectedConnection { get; set; }
        public ICommand EditConnectionCommand { get; set; }
        public ICommand ToggleConnectionStateCommand { get; set; }

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