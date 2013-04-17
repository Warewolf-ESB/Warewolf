using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Diagnostics;
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
        public void ConstructorWithNullArgumentsThrowsArgumentNullException()
        {
            var connection = new TcpConnection(null, null, 0, null);
        }

        [TestMethod]
        public void ConstructorWithValidArgumentsInitializesProperties()
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
        }

        #endregion

        #region SendNetworkMessage

        [TestMethod]
        public void SendNetworkMessageWithMessageExpectedInvokesHost()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.SendNetworkMessage(It.IsAny<INetworkMessage>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.SendNetworkMessage(new TestMessage());
            host.Verify(h => h.SendNetworkMessage(It.IsAny<INetworkMessage>()));
        }

        #endregion

        #region SendReceiveNetworkMessage

        [TestMethod]
        public void SendReceiveNetworkMessageWithMessageExpectedInvokesHost()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.SendReceiveNetworkMessage(new TestMessage());
            host.Verify(h => h.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>()));
        }

        #endregion

        #region RecieveNetworkMessage

        [TestMethod]
        public void RecieveNetworkMessageWithByteReaderExpectedInvokesHost()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.RecieveNetworkMessage(It.IsAny<IByteReaderBase>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.RecieveNetworkMessage(It.IsAny<IByteReaderBase>());
            host.Verify(h => h.RecieveNetworkMessage(It.IsAny<IByteReaderBase>()));
        }

        #endregion

        #region ExecuteCommand

        [TestMethod]
        public void ExecuteCommandWithRequestExpectedInvokesHost()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>());
            host.Verify(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        public void ExecuteCommandWithManagementServicePayloadExpectedStripsTags()
        {
            const string RootTag = "Root";
            const string TestContent = "xxxxx";

            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(
                string.Format("<{0}><{1}>{2}</{1}></{0}>", RootTag, GlobalConstants.ManagementServicePayload, TestContent));

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            var actual = connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            Assert.AreEqual(TestContent, actual);
        }

        #endregion

        #region AddDebugWriter

        [TestMethod]
        public void AddDebugWriterWithWriterExpectedInvokesHost()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.AddDebugWriter(It.IsAny<IDebugWriter>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.AddDebugWriter(It.IsAny<IDebugWriter>());
            host.Verify(h => h.AddDebugWriter(It.IsAny<IDebugWriter>()));
        }

        #endregion

        #region AddDebugWriter

        [TestMethod]
        public void RemoveDebugWriterWithIDExpectedInvokesHost()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.RemoveDebugWriter(It.IsAny<Guid>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.RemoveDebugWriter(It.IsAny<Guid>());
            host.Verify(h => h.RemoveDebugWriter(It.IsAny<Guid>()));
        }

        #endregion

        #region Disconnect

        [TestMethod]
        public void DisconnectExpectedInvokesHostAndNulls()
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.Disconnect()).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, true, host.Object);
            connection.Disconnect();
            host.Verify(h => h.Disconnect());
            Assert.IsNull(connection.Host);
        }

        #endregion

        #region Connect

        [TestMethod]
        public void ConnectExpectedInvokesHost()
        {
            var invokedConnectAsync = false;
            var invokedLoginAsync = false;

            var securityContetxt = new Mock<IFrameworkSecurityContext>();
            var eventAggregator = new Mock<IEventAggregator>();
            var host = new Mock<ITcpClientHost>();
            //host.Setup(h => h.ConnectAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(
            //    () =>
            //    {
            //        invokedConnectAsync = true;
            //        return Task.Factory.FromResult(true);
            //    });
            //host.Setup(h => h.LoginAsync(It.IsAny<IIdentity>())).Returns(
            //    () => 
            //    {
            //        invokedLoginAsync = true;
            //        return new Task<bool>(() => true);
            //    });

            //var connection = new TestTcpConnection(AppServerUri, WebServerPort, false, host.Object, securityContetxt.Object, eventAggregator.Object);
            //connection.Connect();

            //Assert.IsTrue(invokedConnectAsync);
            //Assert.IsTrue(invokedLoginAsync);
            //Assert.IsNotNull(connection.Host);
            Assert.Inconclusive();
        }

        #endregion
    }
}
