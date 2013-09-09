using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network;

namespace Unlimited.UnitTest.Framework.Dev2.Network
{
    public class MockStudioNetworkChannelContext : IStudioNetworkChannelContext
    {
        private INetworkOperator _networkOperator;
        private Guid _server;
        private Guid _account;

        public MockStudioNetworkChannelContext(INetworkOperator networkOperator, Guid account, Guid server)
        {
            _networkOperator = networkOperator;
            _server = server;
            _account = account;
        }

        public INetworkOperator NetworkOperator
        {
            get { return _networkOperator; }
        }

        public Guid Account
        {
            get { return _server; }
        }

        public Guid Server
        {
            get { return _server; }
        }
    }
}
