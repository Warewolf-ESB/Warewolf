using System;
using System.Net;
using System.Network;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Network;

namespace Dev2.Core.Tests.Network
{
    public class TestTcpClientHostWithEvents : TcpClientHostBase
    {
        public TestTcpClientHostWithEvents(IEventPublisher eventPublisher)
            : base(eventPublisher)
        {
        }

        #region Overrides of TcpClientHostBase

        public override bool Ping(EndPoint endPoint)
        {
            return false;
        }

        #endregion

        public void TestOnEventProviderClientMessageReceived(INetworkOperator op, ByteBuffer reader)
        {
            OnEventProviderClientMessageReceived(op, reader);
        }
    }
}
