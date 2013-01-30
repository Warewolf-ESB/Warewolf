using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network;

namespace Unlimited.UnitTest.Framework.Dev2.Network
{
    class MockServerNetworkChannelContext : IServerNetworkChannelContext<MockNetworkContext>
    {
        private MockNetworkContext _networkContext;
        private TCPServer<MockNetworkContext> _server;

        public MockServerNetworkChannelContext(MockNetworkContext networkContext, TCPServer<MockNetworkContext> server)
        {
            _networkContext = networkContext;
            _server = server;
        }

        public MockNetworkContext NetworkContext
        {
            get { return _networkContext; }
        }

        public TCPServer<MockNetworkContext> Server
        {
            get { return _server; }
        }
    }
}
