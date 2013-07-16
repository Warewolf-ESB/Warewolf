using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Network
{
    [TestClass]
    public class TcpConnectionTests
    {
        static readonly Uri AppServerUri = new Uri("http://127.0.0.1:77/dsf");
        static readonly Uri WebServerUri = new Uri("http://127.0.0.1:1234");

        const int WebServerPort = 1234;

        #region Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TcpConnectionConstructorWithNullArgumentsThrowsArgumentNullException()
        {
            var connection = new TcpConnection(null, null, 0, null);
        }

        [TestMethod]
        public void TcpConnectionConstructorWithValidArgumentsInitializesProperties()
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            var eventAggregator = new Mock<IEventAggregator>();

            var connection = new TcpConnection(securityContext.Object, AppServerUri, WebServerPort, eventAggregator.Object);

            Assert.AreEqual(securityContext.Object, connection.SecurityContext);
            Assert.AreEqual(eventAggregator.Object, connection.EventAggregator);
            Assert.AreEqual(AppServerUri, connection.AppServerUri);
            Assert.AreEqual(WebServerUri, connection.WebServerUri);
            Assert.IsNull(connection.MessageBroker);
            Assert.IsNull(connection.MessageAggregator);
            Assert.IsNotNull(connection.ServerEvents);
        }

        #endregion

        #region SendNetworkMessage

        [TestMethod]
        public void TcpConnectionSendNetworkMessageWithMessageExpectedInvokesHost()
        {
            var host = CreateTcpClientHost();
            host.Setup(h => h.SendNetworkMessage(It.IsAny<INetworkMessage>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.SendNetworkMessage(new TestMessage());
            host.Verify(h => h.SendNetworkMessage(It.IsAny<INetworkMessage>()));
        }

        #endregion

        #region SendReceiveNetworkMessage

        [TestMethod]
        public void TcpConnectionSendReceiveNetworkMessageWithMessageExpectedInvokesHost()
        {
            var host = CreateTcpClientHost();
            host.Setup(h => h.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.SendReceiveNetworkMessage(new TestMessage());
            host.Verify(h => h.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>()));
        }

        #endregion

        #region RecieveNetworkMessage

        [TestMethod]
        public void TcpConnectionRecieveNetworkMessageWithByteReaderExpectedInvokesHost()
        {
            var host = CreateTcpClientHost();
            host.Setup(h => h.RecieveNetworkMessage(It.IsAny<IByteReaderBase>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.RecieveNetworkMessage(It.IsAny<IByteReaderBase>());
            host.Verify(h => h.RecieveNetworkMessage(It.IsAny<IByteReaderBase>()));
        }

        #endregion

        #region ExecuteCommand

        [TestMethod]
        public void TcpConnectionExecuteCommandWithRequestExpectedInvokesHost()
        {
            var host = CreateTcpClientHost();
            host.Setup(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>());
            host.Verify(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        public void TcpConnectionExecuteCommandWithManagementServicePayloadExpectedStripsTags()
        {
            const string RootTag = "Root";
            const string TestContent = "xxxxx";

            var host = CreateTcpClientHost();
            host.Setup(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(
                string.Format("<{0}><{1}>{2}</{1}></{0}>", RootTag, GlobalConstants.ManagementServicePayload, TestContent));

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            var actual = connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            Assert.AreEqual(TestContent, actual);
        }

        #endregion

        #region Disconnect

        [TestMethod]
        public void TcpConnectionDisconnectExpectedInvokesHostAndNulls()
        {
            var host = CreateTcpClientHost();
            host.Setup(h => h.Disconnect()).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.Disconnect();
            host.Verify(h => h.Disconnect());
            Assert.IsNull(connection.Host);
        }

        #endregion

        #region Connect

        [TestMethod]
        [ExpectedException(typeof(Exception), "")]
        public void TcpConnectionConnectWhenResultFalseThrowsException()
        {
            var host = CreateTcpClientHost();
            host.Setup(h => h.ConnectAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(() => new Task<bool>(() => false)).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.SetIsConnected(false);
            connection.Connect();

        }

        #endregion

        #region ServerStateChanged

        // PBI 9228: TWR - 2013.04.17

        [TestMethod]
        public void TcpConnectionServerStateChangedWhenRaisedByHostExpectedRaisedOnConnection()
        {
            var eventCount = 0;

            var host = CreateTcpClientHost();
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.ServerStateChanged += (sender, args) =>
            {
                eventCount++;
            };
            host.Raise(h => h.ServerStateChanged += null, new ServerStateEventArgs(ServerState.Offline));
            host.Raise(h => h.ServerStateChanged += null, new ServerStateEventArgs(ServerState.Offline));
            Assert.AreEqual(2, eventCount);
        }

        #endregion

        #region CreateTcpClientHost

        static Mock<ITcpClientHost> CreateTcpClientHost()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.MessageAggregator).Returns(new Mock<IStudioNetworkMessageAggregator>().Object);
            host.Setup(h => h.MessageBroker).Returns(new Mock<INetworkMessageBroker>().Object);
            return host;
        }

        #endregion

    }
}
