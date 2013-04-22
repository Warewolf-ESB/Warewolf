using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;

namespace Dev2.Network
{
    public class StudioNetworkChannelContext : IStudioNetworkChannelContext
    {
        private INetworkOperator _networkOperator;
        private Guid _server;
        private Guid _account;

        public StudioNetworkChannelContext(INetworkOperator networkOperator, Guid account, Guid server)
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
            get { return _account; }
        }

        public Guid Server
        {
            get { return _server; }
        }
    }
}
