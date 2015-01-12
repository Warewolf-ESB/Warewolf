using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class ConnectControlViewModel
    {
        public ConnectControlViewModel(IServer server)
        {
            if(server == null)
            {
                throw new ArgumentNullException("server");
            }
            Server = server;
            Servers = Server.GetServerConnections();
        }

        public IServer Server { get; set; }
        public IList<IServer> Servers { get; set; }
        public IServer SelectedConnection { get; set; }
    }
}