using System.Net;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Network;

namespace Dev2.Core.Tests.Network
{
    public class TestTcpClientHost : TcpClientHostBase
    {
        public TestTcpClientHost()
            : base(new EventPublisher())
        {
        }

        public override bool Ping(EndPoint endPoint)
        {
            return false;
        }

        public int StartReconnectTimerHitCount { get; private set; }
        protected override void StartReconnectTimer()
        {
            StartReconnectTimerHitCount++;
        }
    }
}
