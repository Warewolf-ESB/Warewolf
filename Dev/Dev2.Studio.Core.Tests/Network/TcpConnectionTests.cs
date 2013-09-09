using System;
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
    [TestClass, System.Runtime.InteropServices.GuidAttribute("C7C2360E-A9B1-4D7A-99F5-DC4FACC58C21")]
    public class TcpConnectionTests
    {
        static readonly Uri AppServerUri = new Uri("http://127.0.0.1:77/dsf");
        static readonly Uri WebServerUri = new Uri("http://127.0.0.1:1234");

        const int WebServerPort = 1234;

        #region Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TcpConnectionConstructor_NullArgumentsThrowsArgumentNullException()
        {
            var connection = new TcpConnection(null, null, 0);
        }

        [TestMethod]
        public void TcpConnectionConstructor_ValidArgumentsInitializesProperties()
        {
            var securityContext = new Mock<IFrameworkSecurityContext>();
            var eventAggregator = new Mock<IEventAggregator>();

            var connection = new TcpConnection(securityContext.Object, AppServerUri, WebServerPort);

            Assert.AreEqual(securityContext.Object, connection.SecurityContext);
            Assert.AreEqual(AppServerUri, connection.AppServerUri);
            Assert.AreEqual(WebServerUri, connection.WebServerUri);
            Assert.IsNull(connection.MessageBroker);
            Assert.IsNull(connection.MessageAggregator);
            Assert.IsNotNull(connection.ServerEvents);
        }

        #endregion

        #region SendNetworkMessage

        [TestMethod]
        public void TcpConnection_SendNetworkMessage_Message_InvokesHost()
        {
            var host = CreateTcpClientHost(true);
            host.Setup(h => h.SendNetworkMessage(It.IsAny<INetworkMessage>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.SendNetworkMessage(new TestMessage());
            host.Verify(h => h.SendNetworkMessage(It.IsAny<INetworkMessage>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TcpConnection_SendNetworkMessage")]
        public void TcpConnection_SendNetworkMessage_NullHost_DoesNotThrowException()
        {
            //------------Setup for test--------------------------
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);

            //------------Execute Test---------------------------
            try
            {
                connection.SendNetworkMessage(It.IsAny<INetworkMessage>());
            }
            catch(Exception ex)
            {
                //------------Assert Results-------------------------
                Assert.Fail(ex.Message);
            }
        }

        #endregion

        #region SendReceiveNetworkMessage

        [TestMethod]
        public void TcpConnection_SendReceiveNetworkMessage_Message_InvokesHost()
        {
            var host = CreateTcpClientHost(true);
            host.Setup(h => h.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.SendReceiveNetworkMessage(new TestMessage());
            host.Verify(h => h.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TcpConnection_SendReceiveNetworkMessage")]
        public void TcpConnection_SendReceiveNetworkMessage_NullHost_DoesNotThrowException()
        {
            //------------Setup for test--------------------------
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);

            //------------Execute Test---------------------------
            try
            {
                var result = connection.SendReceiveNetworkMessage(It.IsAny<INetworkMessage>());
                Assert.IsNull(result);
            }
            catch(Exception ex)
            {
                //------------Assert Results-------------------------
                Assert.Fail(ex.Message);
            }
        }

        #endregion

        #region RecieveNetworkMessage

        [TestMethod]
        public void TcpConnection_RecieveNetworkMessage_ByteReader_InvokesHost()
        {
            var host = CreateTcpClientHost(true);
            host.Setup(h => h.RecieveNetworkMessage(It.IsAny<IByteReaderBase>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.RecieveNetworkMessage(It.IsAny<IByteReaderBase>());
            host.Verify(h => h.RecieveNetworkMessage(It.IsAny<IByteReaderBase>()));
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TcpConnection_RecieveNetworkMessage")]
        public void TcpConnection_RecieveNetworkMessage_NullHost_DoesNotThrowException()
        {
            //------------Setup for test--------------------------
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);

            //------------Execute Test---------------------------
            try
            {
                var result = connection.RecieveNetworkMessage(It.IsAny<IByteReaderBase>());
                Assert.IsNull(result);
            }
            catch(Exception ex)
            {
                //------------Assert Results-------------------------
                Assert.Fail(ex.Message);
            }
        }

        #endregion

        #region ExecuteCommand

        [TestMethod]
        public void TcpConnection_ExecuteCommand_Request_InvokesHost()
        {
            var host = CreateTcpClientHost(true);
            host.Setup(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>());
            host.Verify(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        public void TcpConnection_ExecuteCommand_ManagementServicePayload_StripsTags()
        {
            const string RootTag = "Root";
            const string TestContent = "xxxxx";

            var host = CreateTcpClientHost(true);
            host.Setup(h => h.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(
                string.Format("<{0}><{1}>{2}</{1}></{0}>", RootTag, GlobalConstants.ManagementServicePayload, TestContent));

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            var actual = connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            Assert.AreEqual(TestContent, actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TcpConnection_ExecuteCommand")]
        public void TcpConnection_ExecuteCommand_NullHost_EmptyString()
        {
            //------------Setup for test--------------------------
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);

            //------------Execute Test---------------------------
            var actual = connection.ExecuteCommand(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, actual);
        }

        #endregion

        #region Disconnect

        [TestMethod]
        public void TcpConnectionDisconnect_InvokesHostAndNulls()
        {
            var host = CreateTcpClientHost(true);
            host.Setup(h => h.Disconnect()).Verifiable();

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.Disconnect();
            host.Verify(h => h.Disconnect());
            Assert.IsNull(connection.Host);
        }

        #endregion

        #region Connect

        [TestMethod]
        [TestCategory("TcpConnection_Connect")]
        [Description("TcpConnection Connect must invoke TcpClientHost correctly: ConnectAsync followed by LoginAsync.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_ConnectImplementation_InvokesTcpClientHostCorrectly()
        // ReSharper restore InconsistentNaming
        {
            var host = new Mock<TestTcpClientHostAsync>();
            host.Object.ConnectAsyncDelay = 0;
            host.Object.ConnectAsyncResult = true;
            host.Object.LoginAsyncResult = true;

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.Connect();

            Assert.AreEqual(1, host.Object.ConnectAsyncHitCount, "Connect did not invoke ConnectAsync.");
            Assert.AreEqual(1, host.Object.LoginAsyncHitCount, "Connect did not invoke LoginAsync.");
        }

        [TestMethod]
        [TestCategory("TcpConnection_Connect")]
        [Description("TcpConnection Connect must not invoke TcpClientHost.LoginAsync when connection to server fails.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_ConnectFails_DoesNotInvokeLoginAsync()
        // ReSharper restore InconsistentNaming
        {
            var host = new Mock<TestTcpClientHostAsync>();
            host.Object.ConnectAsyncDelay = 0;
            host.Object.ConnectAsyncResult = false;
            host.Object.LoginAsyncResult = true;

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.Connect();

            Assert.AreEqual(1, host.Object.ConnectAsyncHitCount, "Connect did not invoke ConnectAsync.");
            Assert.AreEqual(0, host.Object.LoginAsyncHitCount, "Connect did invoke LoginAsync.");
        }

        [TestMethod]
        [TestCategory("TcpConnection_Connect")]
        [Description("TcpConnection Connect must disconnect when connection to server fails.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_ConnectFails_DoesInvokeDisconnect()
        // ReSharper restore InconsistentNaming
        {
            var host = new Mock<TestTcpClientHostAsync>();
            host.Object.ConnectAsyncDelay = 0;
            host.Object.ConnectAsyncResult = false;
            host.Object.LoginAsyncResult = false;

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.Connect();

            Assert.AreEqual(1, connection.DisconnectHitCount, "Disconnect must be invoked when connection to server fails.");
        }

        [TestMethod]
        [TestCategory("TcpConnection_Connect")]
        [Description("TcpConnection Connect must disconnect when connection to server times out.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_ConnectTimesOut_DoesInvokeDisconnect()
        // ReSharper restore InconsistentNaming
        {
            var host = new Mock<TestTcpClientHostAsync>();
            host.Object.ConnectAsyncDelay = 100;
            host.Object.ConnectAsyncResult = false;
            host.Object.LoginAsyncResult = false;

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.Connect();

            Assert.AreEqual(1, connection.DisconnectHitCount, "Disconnect must be invoked when connection to server times out.");
        }


        [TestMethod]
        [TestCategory("TcpConnection_Connect")]
        [Description("TcpConnection Connect must not disconnect when connection to server succeeds.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_ConnectSucceeds_DoesNotInvokeDisconnect()
        // ReSharper restore InconsistentNaming
        {

            var host = new Mock<TestTcpClientHostAsync>();
            host.Object.ConnectAsyncDelay = 0;
            host.Object.ConnectAsyncResult = true;
            host.Object.LoginAsyncResult = true;

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.Connect();

            Assert.AreEqual(0, connection.DisconnectHitCount, "Disconnect must be not be invoked when connection to server succeeds.");
        }

        #endregion

        #region Verify

        [TestMethod]
        [TestCategory("TcpConnection_Verify")]
        [Description("TcpConnection Verify must create a new instance of TcpClientHost.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_VerifyImplementation_CreatesNewTcpClientHost()
        // ReSharper restore InconsistentNaming
        {
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);

            connection.Verify(null);

            Assert.AreEqual(1, connection.CreateHostHitCount, "Verify did not create a new host.");
        }

        [TestMethod]
        [TestCategory("TcpConnection_Verify")]
        [Description("TcpConnection Verify does not fire any StateChanged events.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_VerifyImplementation_DoesNotFireStateChangedEvents()
        // ReSharper restore InconsistentNaming
        {
            var serverStateChangedHitCount = 0;
            var networkStateChangedHitCount = 0;
            var loginStateChangedHitCount = 0;

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);
            connection.ServerStateChanged += (sender, args) => { serverStateChangedHitCount++; };
            connection.NetworkStateChanged += (sender, args) => { networkStateChangedHitCount++; };
            connection.LoginStateChanged += (sender, args) => { loginStateChangedHitCount++; };

            connection.Verify(null);

            Assert.AreEqual(0, serverStateChangedHitCount, "Verify fired a ServerStateChanged event.");
            Assert.AreEqual(0, networkStateChangedHitCount, "Verify fired a NetworkStateChanged event.");
            Assert.AreEqual(0, loginStateChangedHitCount, "Verify fired a LoginStateChanged event.");
        }

        [TestMethod]
        [TestCategory("TcpConnection_Verify")]
        [Description("TcpConnection Verify must publish a DesignValidationMemo when verification fails.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_VerifyFailed_InvokesCallback_CorrectResult()
        // ReSharper restore InconsistentNaming
        {
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);
            connection.Verify(result =>
            {
                Assert.AreEqual(ConnectResult.ConnectFailed, result, "Verify invoked callback with wrong result.");
            });
        }

        [TestMethod]
        [TestCategory("TcpConnection_Verify")]
        [Description("TcpConnection Verify must not publish a DesignValidationMemo when verification succeeded.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_VerifySucceeded_InvokesCallback_CorrectResult()
        // ReSharper restore InconsistentNaming
        {
            var host = new Mock<TestTcpClientHostAsync>();
            host.Object.ConnectAsyncDelay = 0;
            host.Object.ConnectAsyncResult = true;
            host.Object.LoginAsyncResult = true;

            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
            connection.Verify(result =>
            {
                Assert.AreEqual(ConnectResult.Success, result, "Verify invoked callback with wrong result.");
            });
        }

        #endregion

        #region StartAutoConnect

        [TestMethod]
        [TestCategory("TcpConnection_StartAutoConnect")]
        [Description("TcpConnection StartAutoConnect must invoke TcpClientHost.StartReconnectHeartbeat if not connected.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_StartAutoConnect_DisconnectedHost_InvokesStartReconnectHeartbeat()
        // ReSharper restore InconsistentNaming
        {
            var connection = new TestTcpConnectionWithMockHost(AppServerUri, WebServerPort, false);
            connection.Host.Setup(h => h.StartReconnectHeartbeat(It.IsAny<string>(), It.IsAny<int>())).Verifiable();

            connection.StartAutoConnect();

            connection.Host.Verify(h => h.StartReconnectHeartbeat(It.IsAny<string>(), It.IsAny<int>()), "StartAutoConnect did not invoke TcpClientHost StartReconnectHeartbeat.");
        }

        [TestMethod]
        [TestCategory("TcpConnection_StartAutoConnect")]
        [Description("TcpConnection StartAutoConnect must not invoke TcpClientHost.StartReconnectHeartbeat if connected.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_StartAutoConnect_ConnectedHost_DoesNotInvokeStartReconnectHeartbeat()
        // ReSharper restore InconsistentNaming
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.StartReconnectHeartbeat(It.IsAny<string>(), It.IsAny<int>())).Verifiable();

            var connection = new TestTcpConnectionWithMockHost(AppServerUri, WebServerPort, true);
            connection.StartAutoConnect();

            host.Verify(h => h.StartReconnectHeartbeat(It.IsAny<string>(), It.IsAny<int>()), Times.Never(), "StartAutoConnect did invoke TcpClientHost StartReconnectHeartbeat.");
        }

        [TestMethod]
        [TestCategory("TcpConnection_StartAutoConnect")]
        [Description("TcpConnection StartAutoConnect must create TcpClientHost if null.")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpConnection_UnitTest_StartAutoConnect_NullHost_CreatesHost()
        // ReSharper restore InconsistentNaming
        {
            var connection = new TestTcpConnectionWithMockHost(AppServerUri, WebServerPort, false);
            connection.StartAutoConnect();

            Assert.AreEqual(1, connection.CreateHostHitCount, "StartAutoConnect did not create TcpClientHost.");
        }

        #endregion

        #region ServerStateChanged

        // PBI 9228: TWR - 2013.04.17

        [TestMethod]
        public void TcpConnectionServerStateChangedWhenRaisedByHost_RaisedOnConnection()
        {
            var eventCount = 0;

            var host = CreateTcpClientHost(true);
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, host.Object);
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

        static Mock<ITcpClientHost> CreateTcpClientHost(bool isConnected)
        {
            var host = new Mock<ITcpClientHost>();
            host.Setup(h => h.MessageAggregator).Returns(new Mock<IStudioNetworkMessageAggregator>().Object);
            host.Setup(h => h.MessageBroker).Returns(new Mock<INetworkMessageBroker>().Object);
            host.Setup(h => h.IsConnected).Returns(isConnected);

            return host;
        }

        #endregion


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TcpConnection_AddDebugWriter")]
        public void TcpConnection_AddDebugWriter_NullHost_DoesNotThrowException()
        {
            //------------Setup for test--------------------------
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);

            //------------Execute Test---------------------------
            try
            {
                connection.AddDebugWriter();
            }
            catch(Exception ex)
            {
                //------------Assert Results-------------------------
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("TcpConnection_RemoveDebugWriter")]
        public void TcpConnection_RemoveDebugWriter_NullHost_DoesNotThrowException()
        {
            //------------Setup for test--------------------------
            var connection = new TestTcpConnection(AppServerUri, WebServerPort, null);

            //------------Execute Test---------------------------
            try
            {
                connection.RemoveDebugWriter();
            }
            catch(Exception ex)
            {
                //------------Assert Results-------------------------
                Assert.Fail(ex.Message);
            }
        }
    }
}
