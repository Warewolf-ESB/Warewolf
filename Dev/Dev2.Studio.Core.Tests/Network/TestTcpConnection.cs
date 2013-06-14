using System;
using Caliburn.Micro;
using Dev2.Studio.Core.Network;
using Moq;

namespace Dev2.Core.Tests.Network
{
    public class TestTcpConnection : TcpConnection
    {
        bool _isConnected;

        public TestTcpConnection(Uri appServerUri, int webServerPort, bool isConnected = true)
            : this(appServerUri, webServerPort, isConnected, new TcpClientHost())
        {
        }

        public TestTcpConnection(Uri appServerUri, int webServerPort, bool isConnected, ITcpClientHost tcpClientHost)
            : this(appServerUri, webServerPort, isConnected, tcpClientHost, new Mock<IFrameworkSecurityContext>().Object, new Mock<IEventAggregator>().Object)
        {
        }

        public TestTcpConnection(Uri appServerUri, int webServerPort, bool isConnected, ITcpClientHost tcpClientHost, IFrameworkSecurityContext securityContext, IEventAggregator eventAggregator)
            : base(securityContext, appServerUri, webServerPort, eventAggregator)
        {
            _isConnected = isConnected;
            TCPHost = tcpClientHost;
            if(isConnected)
            {
                InitializeHost();
            }
        }

        public ITcpClientHost Host { get { return TCPHost; } }

        public override bool IsConnected { get { return _isConnected; } }

        public void SetIsConnected(bool isConnected)
        {
            _isConnected = isConnected;
        }
    }
}