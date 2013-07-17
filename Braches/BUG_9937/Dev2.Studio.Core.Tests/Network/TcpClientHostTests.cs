using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Communication;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Network
{
    [TestClass]
    public class TcpClientHostTests
    {
        #region ConnectAsync

        [TestMethod]
        public void TcpClientHostConnectAsyncWithNullAddressReturnsFalse()
        {
            ConnectAsyncTest(null, 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithInvalidAddressReturnsFalse()
        {
            ConnectAsyncTest("qwerty", 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithTooLongDnsAddressReturnsFalse()
        {
            const int AddressLength = 255 + 5;
            const string Input = "abcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();
            var address = string.Join("", Enumerable.Range(0, AddressLength).Select(x => Input[random.Next(0, Input.Length)]));

            ConnectAsyncTest(address, 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithInvalidDnsAddressReturnsFalse()
        {
            ConnectAsyncTest("RSAKLFSVRGEN", 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithValidDnsAddressReturnsTrue()
        {

            ConnectAsyncTest("RSAKLFSVRGENDEV", 80, true, NetworkState.Connecting, NetworkState.Online, false);
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithUnavailablePortReturnsFalse()
        {
            ConnectAsyncTest("127.0.0.1", 111, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithInvalidPortReturnsFalse()
        {
            ConnectAsyncTest("127.0.0.1", -33, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithValidIpAddressReturnsTrue()
        {
            ConnectAsyncTest("192.168.104.11", 80, true, NetworkState.Connecting, NetworkState.Online, false);
        }

        [TestMethod]
        public void TcpClientHostIsConnectedWithLoggedOffHostExpectedReturnsFalse()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            var task = host.ConnectAsync("RSAKLFSVRGENDEV", 80);
            task.Wait();

            //Execute and assert
            Assert.IsFalse(host.IsConnected, "isConnected returned true without being logged in");
        }

        [TestMethod]
        public void TcpClientHostConnectAsyncWithConnectedHostExpectedReturnsTrue()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            var task = host.ConnectAsync("RSAKLFSVRGENDEV", 80);
            task.Wait();

            //Execute
            task = host.ConnectAsync(null, 0);
            try
            {
                task.Wait();
            }
            catch(AggregateException aex)
            {
                var errors = new StringBuilder("Unhandled ConnectAsync Errors : ");
                aex.Handle(ex =>
                {
                    errors.AppendLine(ex.Message);
                    return true;
                });
                Assert.Fail(errors.ToString());
            }

            //Assert
            Assert.IsTrue(task.Result);
        }

        #endregion

        #region ConnectAsyncTest

        static void ConnectAsyncTest(string hostAddress, int hostPort, bool expectedResult, NetworkState expectedFromState, NetworkState expectedToState, bool expectedIsError)
        {
            var stateChangedCount = 0;
            EventHandler<NetworkStateEventArgs> networkStateChangedHandler = (sender, args) =>
            {
                if(stateChangedCount++ == 0)
                {
                    Assert.AreEqual(NetworkState.Offline, args.FromState);
                    Assert.AreEqual(NetworkState.Connecting, args.ToState);
                    Assert.AreEqual(false, args.IsError);
                }
                else
                {
                    Assert.AreEqual(expectedFromState, args.FromState);
                    Assert.AreEqual(expectedToState, args.ToState);
                    Assert.AreEqual(expectedIsError, args.IsError);
                }
                if(args.IsError)
                {
                    Debug.WriteLine(args.Message);
                }
            };

            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            try
            {
                host.NetworkStateChanged += networkStateChangedHandler;
                var task = host.ConnectAsync(hostAddress, hostPort);
                try
                {
                    task.Wait();
                }
                catch(AggregateException aex)
                {
                    var errors = new StringBuilder("Unhandled ConnectAsync Errors : ");
                    aex.Handle(ex =>
                    {
                        errors.AppendLine(ex.Message);
                        return true;
                    });
                    Assert.Fail(errors.ToString());
                }
                Assert.AreEqual(task.Result, expectedResult);
            }
            finally
            {
                host.NetworkStateChanged -= networkStateChangedHandler;
                host.Disconnect();
                host.Dispose();
            }
        }

        #endregion

        #region LoginAsync

        [TestMethod]
        public void TcpClientHostLoginAsyncWhenDisconnectedReturnsFalse()
        {
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            var loginTask = host.LoginAsync(null, null);
            Assert.IsFalse(loginTask.Result);
        }

        [TestMethod]
        public void TcpClientHostLoginAsyncWithNullUserNameReturnsFalse()
        {
            LoginAyncTest(null, "1111", false, 0, AuthenticationResponse.Unspecified, true);
        }

        [TestMethod]
        public void TcpClientHostLoginAsyncWithNullPasswordReturnsFalse()
        {
            LoginAyncTest("qwerty", null, false, 0, AuthenticationResponse.Unspecified, true);
        }

        //2013.04.15: Ashley Lewis - Should be moved to integration tests because they require a server at 127.0.0.1:77
        //[TestMethod]
        //public void TcpClientHostLoginAsyncWithInvalidCredentialsReturnsFalse()
        //{
        //    LoginAyncTest("qwerty", "1111", false, 1, AuthenticationResponse.InvalidCredentials, true);
        //}

        //[TestMethod]
        //public void TcpClientHostLoginAsyncWithValidCredentialsReturnsTrue()
        //{
        //    LoginAyncTest(null, null, true, 1, AuthenticationResponse.Success, false);
        //}

        //[TestMethod]
        //public void TcpClientHostLoginAsyncWithValidCredentialsTwiceReturnsDoesNotAuthenticateTwice()
        //{
        //    LoginAyncTest(null, null, true, 1, AuthenticationResponse.Success, false, true);
        //}

        [TestMethod]
        public void TcpClientHostLoginAsyncWithNullIdentityExpectedReturnFalse()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            var task = host.ConnectAsync("RSAKLFSVRGENDEV", 80);
            task.Wait();

            //Execute
            var loginTask = host.LoginAsync(null);
            loginTask.Wait();

            //Assert
            Assert.IsFalse(loginTask.Result, "Host logged in with null Identity");
        }

        #endregion

        #region LoginAyncTest

        static void LoginAyncTest(string userName, string password, bool expectedResult, int expectedStateChangedCount, AuthenticationResponse expectedStateReply, bool expectedStateIsError, bool retry = false)
        {
            var stateChangedCount = 0;
            EventHandler<LoginStateEventArgs> loginStateChangedHandler = (sender, args) =>
            {
                stateChangedCount++;
                Debug.WriteLine("Reply    : {0}", args.Reply);
                Debug.WriteLine("LoggedIn : {0}", args.LoggedIn);
                Debug.WriteLine("Message  : {0}", (object)args.Message);

                Assert.AreEqual(expectedResult, args.LoggedIn);
                Assert.AreEqual(expectedStateReply, args.Reply);
                Assert.AreEqual(expectedStateIsError, args.IsError);
            };

            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            try
            {
                host.LoginStateChanged += loginStateChangedHandler;
                var task = host
                .ConnectAsync("127.0.0.1", 77)
                .ContinueWith(connectTask =>
                {
                    if(connectTask.Result)
                    {
                        var loginTask = host.LoginAsync(userName, password);
                        return loginTask.Result;
                    }
                    return false;
                });
                try
                {
                    task.Wait();
                    if(retry)
                    {
                        task = host.LoginAsync(userName, password);
                        task.Wait();
                    }
                }
                catch(AggregateException aex)
                {
                    var errors = new StringBuilder("Unhandled LoginAsync Errors : ");
                    aex.Handle(ex =>
                    {
                        errors.AppendLine(ex.Message);
                        return true;
                    });
                    Assert.Fail(errors.ToString());
                }
                Assert.AreEqual(task.Result, expectedResult);
                Assert.AreEqual(expectedStateChangedCount, stateChangedCount);
            }
            finally
            {
                host.LoginStateChanged -= loginStateChangedHandler;
                host.Disconnect();
                host.Dispose();
            }
        }

        #endregion

        #region Message Aggregator

        [TestMethod]
        public void TcpClientHostGetMessageAggregatorExpectedReturnsMessageAggregator()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);

            //Execute
            var getAggregator = host.MessageAggregator;

            //Assert
            Assert.AreEqual(getAggregator.ToString(), new StudioNetworkMessageAggregator().ToString());
        }

        #endregion

        #region Message Broker

        [TestMethod]
        public void TcpClientHostGetMessageBrokerExpectedReturnsMessageBroker()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);

            //Execute
            var getBroker = host.MessageBroker;

            //Assert
            Assert.AreEqual(getBroker.ToString(), new NetworkMessageBroker().ToString());
        }

        #endregion

        #region Expected Exceptions

        #region Add Debug Writer

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TcpClientHostAddDebugWriterWithDisposedHostExpectedObjectDisposedException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            host.Dispose();

            //Execute
            host.AddDebugWriter();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TcpClientHostAddDebugWriterWithAuxiliaryHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object, true);

            //Execute
            host.AddDebugWriter();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TcpClientHostAddDebugWriterWithOfflineHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            host.Disconnect();

            //Execute
            host.AddDebugWriter();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TcpClientHostAddDebugWriterWithLoggedOffHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            var task = host.ConnectAsync("RSAKLFSVRGENDEV", 80);
            task.Wait();

            //Execute
            host.AddDebugWriter();
        }

        #endregion

        #region Send Receive

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TcpClientHostSendReceiveWithDisposedHostExpectedObjectDisposedException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            host.Dispose();

            //Execute
            var result = host.SendReceiveNetworkMessage(new ExecuteCommandMessage { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TcpClientHostSendReceiveWithAuxiliaryHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object, true);

            //Execute
            var result = host.SendReceiveNetworkMessage(new ExecuteCommandMessage { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TcpClientHostSendWithDisconnectedHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);
            host.Disconnect();

            //Execute
            host.Send(new Packet(new PacketTemplate(0, 0, 0)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TcpClientHostSendNetworkMessageWithNullMessageExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(new Mock<IEventPublisher>().Object);

            //Execute
            host.SendNetworkMessage(null);
        }

        #endregion

        #endregion

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Constructor with null event provider throws exception.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpClientHostConstructor_UnitTest_NullEventProvider_Exception()
        // ReSharper restore InconsistentNaming
        {
            var host = new TestTcpClientHostWithEvents(null);
        }

        [TestMethod]
        [Description("Constructor with non-null event provider does not throw exception.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpClientHostConstructor_UnitTest_NonNullEventProvider_DoesNotThrowException()
        // ReSharper restore InconsistentNaming
        {
            var host = new TestTcpClientHostWithEvents(new Mock<IEventPublisher>().Object);
        }

        #endregion
        
        #region OnEventProviderClientMessageReceived

        [TestMethod]
        [Description("Messsage is published on event provider using PublishObject method.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpClientHostOnEventProviderClientMessageReceived_UnitTest_EventPublishingUsesPublishObject_True()
        // ReSharper restore InconsistentNaming
        {
            var memo = new Memo { InstanceID = Guid.NewGuid() };

            var envelopeStr = memo.ToString(new JsonSerializer());
            var reader = new ByteBuffer();
            reader.Write(envelopeStr);
            reader.Seek(0, SeekOrigin.Begin);

            var op = new Mock<INetworkOperator>();
            var publisher = new Mock<IEventPublisher>();
            publisher.Setup(p => p.PublishObject(It.IsAny<object>())).Verifiable();

            var host = new TestTcpClientHostWithEvents(publisher.Object);
            host.TestOnEventProviderClientMessageReceived(op.Object, reader);

            publisher.Verify(p => p.PublishObject(It.IsAny<object>()), Times.Once());
        }


        [TestMethod]
        [Description("Messsage is published on event provider.")]
        [TestCategory("UnitTest")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void TcpClientHostOnEventProviderClientMessageReceived_UnitTest_EventPublishing_IsPublished()
        // ReSharper restore InconsistentNaming
        {
            var memo = new Memo { InstanceID = Guid.NewGuid() };

            var publisher = new EventPublisher();
            var subscription = publisher.GetEvent<Memo>().Subscribe(m => Assert.AreEqual(memo, m));


            var envelopeStr = memo.ToString(new JsonSerializer());
            var reader = new ByteBuffer();
            reader.Write(envelopeStr);
            reader.Seek(0, SeekOrigin.Begin);

            var op = new Mock<INetworkOperator>();

            var host = new TestTcpClientHostWithEvents(publisher);
            host.TestOnEventProviderClientMessageReceived(op.Object, reader);

            subscription.Dispose();
        }

        #endregion

    }
}
