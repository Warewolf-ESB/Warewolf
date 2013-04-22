using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network;

namespace Dev2.DynamicServices.Network
{
    public class ServerNetworkChannelContext : IServerNetworkChannelContext<StudioNetworkSession>
    {
        private StudioNetworkSession _networkContext;
        private TCPServer<StudioNetworkSession> _server;

        public ServerNetworkChannelContext(StudioNetworkSession networkContext, TCPServer<StudioNetworkSession> server)
        {
            _networkContext = networkContext;
            _server = server;
        }

        public StudioNetworkSession NetworkContext
        {
            get { return _networkContext; }
        }

        public TCPServer<StudioNetworkSession> Server
        {
            get { return _server; }
        }
    }
}
