using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;

namespace Unlimited.UnitTest.Framework.Dev2.Network
{
    public class MockNetworkServer : TCPServer<MockNetworkContext>
    {
        public MockNetworkServer()
            : base("", null)
        {
        }

        protected override void OnExecuteCommand(MockNetworkContext context, ByteBuffer payload, Packet writer)
        {
            throw new NotImplementedException();
        }

        protected override string OnExecuteCommand(MockNetworkContext context, string payload, Guid dataListID)
        {
            throw new NotImplementedException();
        }
    }
}
