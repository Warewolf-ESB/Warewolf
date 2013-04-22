using System;
using System.Diagnostics;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Network
{
    [TestClass]
    [Ignore]
    public class TcpClientHostTests
    {
        #region ConnectAsync

        [TestMethod]
        public void ConnectAsyncWithNullAddressReturnsFalse()
        {
            ConnectAsyncTest(null, 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void ConnectAsyncWithInvalidAddressReturnsFalse()
        {
            ConnectAsyncTest("qwerty", 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void ConnectAsyncWithTooLongDnsAddressReturnsFalse()
        {
            const int AddressLength = 255 + 5;
            const string Input = "abcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();
            var address = string.Join("", Enumerable.Range(0, AddressLength).Select(x => Input[random.Next(0, Input.Length)]));

            ConnectAsyncTest(address, 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void ConnectAsyncWithInvalidDnsAddressReturnsFalse()
        {
            ConnectAsyncTest("RSAKLFSVRGEN", 0, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void ConnectAsyncWithValidDnsAddressReturnsTrue()
        {
            ConnectAsyncTest("RSAKLFSVRGENDEV", 80, true, NetworkState.Connecting, NetworkState.Online, false);
        }

        [TestMethod]
        public void ConnectAsyncWithUnavailablePortReturnsFalse()
        {
            ConnectAsyncTest("127.0.0.1", 111, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void ConnectAsyncWithInvalidPortReturnsFalse()
        {
            ConnectAsyncTest("127.0.0.1", -33, false, NetworkState.Connecting, NetworkState.Offline, true);
        }

        [TestMethod]
        public void ConnectAsyncWithValidIpAddressReturnsTrue()
        {
            ConnectAsyncTest("127.0.0.1", 80, true, NetworkState.Connecting, NetworkState.Online, false);
        }

        [TestMethod]
        public void IsConnectedWithLoggedOffHostExpectedReturnsFalse()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost();
            var task = host.ConnectAsync("RSAKLFSVRGENDEV", 80);
            task.Wait();

            //Execute and assert
            Assert.IsFalse(host.IsConnected, "isConnected returned true without being logged in");
        }

        [TestMethod]
        public void ConnectAsyncWithConnectedHostExpectedReturnsTrue()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost();
            var task = host.ConnectAsync("RSAKLFSVRGENDEV", 80);
            task.Wait();

            //Execute
            task = host.ConnectAsync(null, 0);
            try
            {
                task.Wait();
            }
            catch (AggregateException aex)
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

            ITcpClientHost host = new TcpClientHost();
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
        public void LoginAsyncWhenDisconnectedReturnsFalse()
        {
            ITcpClientHost host = new TcpClientHost();
            var loginTask = host.LoginAsync(null, null);
            Assert.IsFalse(loginTask.Result);
        }

        [TestMethod]
        public void LoginAsyncWithNullUserNameReturnsFalse()
        {
            LoginAyncTest(null, "1111", false, 0, AuthenticationResponse.Unspecified, true);
        }

        [TestMethod]
        public void LoginAsyncWithNullPasswordReturnsFalse()
        {
            LoginAyncTest("qwerty", null, false, 0, AuthenticationResponse.Unspecified, true);
        }

        //2013.04.15: Ashley Lewis - Should be moved to integration tests because they require a server at 127.0.0.1:77
        //[TestMethod]
        //public void LoginAsyncWithInvalidCredentialsReturnsFalse()
        //{
        //    LoginAyncTest("qwerty", "1111", false, 1, AuthenticationResponse.InvalidCredentials, true);
        //}

        //[TestMethod]
        //public void LoginAsyncWithValidCredentialsReturnsTrue()
        //{
        //    LoginAyncTest(null, null, true, 1, AuthenticationResponse.Success, false);
        //}

        //[TestMethod]
        //public void LoginAsyncWithValidCredentialsTwiceReturnsDoesNotAuthenticateTwice()
        //{
        //    LoginAyncTest(null, null, true, 1, AuthenticationResponse.Success, false, true);
        //}

        [TestMethod]
        public void LoginAsyncWithNullIdentityExpectedReturnFalse()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost();
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

            ITcpClientHost host = new TcpClientHost();
            try
            {
                host.LoginStateChanged += loginStateChangedHandler;
                var task = host
                .ConnectAsync("127.0.0.1", 77)
                .ContinueWith(connectTask =>
                {
                    if (connectTask.Result)
                    {
                        var loginTask = host.LoginAsync(userName, password);
                        return loginTask.Result;
                    }
                    return false;
                });
                try
                {
                    task.Wait();
                    if (retry)
                    {
                        task = host.LoginAsync(userName, password);
                        task.Wait();
                    }
                }
                catch (AggregateException aex)
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
        public void GetMessageAggregatorExpectedReturnsMessageAggregator()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost();

            //Execute
            var getAggregator = host.MessageAggregator;

            //Assert
            Assert.AreEqual(getAggregator.ToString(), new StudioNetworkMessageAggregator().ToString());
        }

        #endregion

        #region Message Broker

        [TestMethod]
        public void GetMessageBrokerExpectedReturnsMessageBroker()
        {
            //Initialize
            ITcpClientHost host = new TcpClientHost();

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
        public void AddDebugWriterWithDisposedHostExpectedObjectDisposedException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost();
            host.Dispose();

            //Execute
            host.AddDebugWriter(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddDebugWriterWithAuxiliaryHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(true);

            //Execute
            host.AddDebugWriter(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddDebugWriterWithOfflineHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost();
            host.Disconnect();

            //Execute
            host.AddDebugWriter(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddDebugWriterWithLoggedOffHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost();
            var task = host.ConnectAsync("RSAKLFSVRGENDEV", 80);
            task.Wait();

            //Execute
            host.AddDebugWriter(null);
        }

        #endregion

        #region Send Receive

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void SendReceiveWithDisposedHostExpectedObjectDisposedException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost();
            host.Dispose();

            //Execute
            var result = host.SendReceiveNetworkMessage(new ExecuteCommandMessage { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SendReceiveWithAuxiliaryHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost(true);

            //Execute
            var result = host.SendReceiveNetworkMessage(new ExecuteCommandMessage { });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SendWithDisconnectedHostExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost();
            host.Disconnect();

            //Execute
            host.Send(new Packet(new PacketTemplate(0,0,0)));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SendNetworkMessageWithNullMessageExpectedInvalidOperationException()
        {
            //Initialization
            ITcpClientHost host = new TcpClientHost();

            //Execute
            host.SendNetworkMessage(null);
        }

        #endregion

        #endregion
    }
}
