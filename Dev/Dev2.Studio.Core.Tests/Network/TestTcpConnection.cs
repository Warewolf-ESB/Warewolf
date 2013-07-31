using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Studio.Core.Network;
using Moq;

namespace Dev2.Core.Tests.Network
{
    public class TestTcpConnection : TcpConnection
    {
        public const int NetworkTimeout = 2000;

        public TestTcpConnection(Uri appServerUri, int webServerPort, ITcpClientHost tcpClientHost, int networkTimeout = NetworkTimeout)
            : base(new Mock<IFrameworkSecurityContext>().Object, appServerUri, webServerPort, new Mock<IEventAggregator>().Object, false, networkTimeout)
        {
            TCPHost = tcpClientHost;
            if(tcpClientHost != null && tcpClientHost.IsConnected)
            {
                InitializeHost();
            }
        }

        public ITcpClientHost Host { get { return TCPHost; } }
        public int DisconnectHitCount { get; set; }
        public int CreateHostHitCount { get; set; }

        protected override bool WaitForConnection(Task<bool> connection)
        {
            return connection.Wait(NetworkTimeout);
        }

        protected override ITcpClientHost CreateHost(bool isAuxiliary)
        {
            CreateHostHitCount++;
            return TCPHost ?? base.CreateHost(isAuxiliary);
        }

        public override void Disconnect()
        {
            DisconnectHitCount++;
            base.Disconnect();
        }
    }
}