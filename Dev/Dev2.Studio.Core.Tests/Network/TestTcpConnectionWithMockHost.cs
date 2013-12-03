//using System;
//using Dev2.Network;
//using Dev2.Studio.Core.Network;
//using Moq;
//
//namespace Dev2.Core.Tests.Network
//{
//    public class TestTcpConnectionWithMockHost : TcpConnection
//    {
//        readonly bool _isConnected;
//        public const int NetworkTimeout = 2000;
//
//        public TestTcpConnectionWithMockHost(Uri appServerUri, int webServerPort, bool isConnected, int networkTimeout = NetworkTimeout)
//            : base(new Mock<IFrameworkSecurityContext>().Object, appServerUri, webServerPort, false, networkTimeout)
//        {
//            _isConnected = isConnected;
//            Host = new Mock<ITcpClientHost>();
//            Host.Setup(h => h.MessageAggregator).Returns(new Mock<IStudioNetworkMessageAggregator>().Object);
//        }
//
//        public override bool IsConnected { get { return _isConnected; } }
//
//        public Mock<ITcpClientHost> Host { get; private set; }
//        public int CreateHostHitCount { get; private set; }
//
//        protected override ITcpClientHost CreateHost(bool isAuxiliary)
//        {
//            CreateHostHitCount++;
//            return Host.Object;
//        }
//
//    }
//}