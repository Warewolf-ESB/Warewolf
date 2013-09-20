using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Network;
using System.Security.Principal;
using System.Text;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests
{
    // BUG 8801 + 8796
    // Sashen.Naidoo : 13-02-2012 : Tests the Studio can always create a connection to the server
    //                              & that the Studio always performs it's connection actions in a 
    //                              synchronous fashion.
    [TestClass]
    public class StudioServerCommsTests
    {                      
        #region Environment Connection Tests

        // Sashen.Naidoo: 13-02-2012 : Bug 8801 + Bug 8796
        // This is to ensure that the studio's environment connection does not go haywire when initialized
        // The issue was with the TCPDispatchClient that the EnvironmentConnection was
        // The Dispatch client would have to use Asynchronous method calls in an asynchronous fashion to the Studio.
        // And the server would send the studio a ClientDetails message on Connection (required by the Studio)
        // that was just ignored by the Studio.
        [TestMethod]
        public void EnvironmentConnectionWithServerAuthenticationExpectedClientDetailsRecieved()
        {            
            IEnvironmentConnection conn = CreateConnection();         

            conn.Connect();
            // The IsConnected property of the EnvironmentConnection references the TCPDispatch Client
            // Only if the connection to the server is successfully made by the dispatch client will the
            // IsConnected message return true
            Assert.IsTrue(conn.IsConnected);

            conn.Disconnect();
        }



        [TestMethod]
        public void EnvironmentConnectionReconnectToServerExpecetedClientConnectionSuccessful()
        {            
            IEnvironmentConnection conn = CreateConnection();

            conn.Connect();
            conn.Disconnect();
            bool beforeReconnection = conn.IsConnected;
            conn.Connect();
            bool afterReconnection = conn.IsConnected;

            Assert.AreNotEqual(beforeReconnection, afterReconnection);
            Assert.IsTrue(afterReconnection);

            conn.Disconnect();
        }


        // Sashen.Naidoo: 13-02-2012 : Bug 8081 
        // A reconnection spam used to cause a set of issues in the Studio
        // This was how the bug replicated itself, because the studio did not wait for the
        // server to return information
        [TestMethod]
       
        public void EnvironmentConnectionReconnectionSpamExpectedAlwaysReconnects()
        {
            // We will perform 10 connections and check if the studio can always connect to the server           
            IEnvironmentConnection environmentConn = CreateConnection();

            List<bool> actualConnections = new List<bool>();
            List<bool> expectedConnections = new List<bool>();
            for (int i = 0; i < 5; i++)
            {
                environmentConn.Connect();
                expectedConnections.Add(true);
                actualConnections.Add(environmentConn.IsConnected);
                environmentConn.Disconnect();
            }
            CollectionAssert.AreEqual(expectedConnections, actualConnections);
        }


        #endregion Environment Connection Tests

        #region TCPDispatchedClient Tests

        // Sashe.Naidoo: 13-02-2012 : Bug 8801 -This test ensures that the asynchronous connection operations on the server,
        //                            happen synchronously on the Studio, if it fails, either the server is unavailable
        //                            or the Studio did not perform a TCP login synchronously (as can be seen no waits are 
        //                            performed in the test)
        [TestMethod]
        public void TcpDispatchedClientLoginExpectedLoginResponseFromServer()
        {
            //TCPDispatchedClient dispatchClient = new TCPDispatchedClient("TestConnection");
            //Uri hostname = new Uri(ServerSettings.DsfAddress);
            //dispatchClient.Connect(hostname.Host, hostname.Port); 
            //dispatchClient.Login("myTestuser", "pwd");
            Assert.Inconclusive("This test is redundant as it is tested else where");
            //Assert.IsTrue(dispatchClient.LoggedIn, "The TCPDispatchedClient was unable to login to the server");
        }

        // Sashen.Naidoo : 13-02-2012 : Bug 8791 : This test ensures that the TCPDispatchedClient can still make a valid 
        //                                         a valid connection to the Server as it previously did, it just checks 
        //                                         that the TCPDispatchedClient only creates a connection
        [TestMethod]
        public void TcpDispatchedClientConnectExpectedConnectionEstablishedToServer() {
            //TCPDispatchedClient dispatchClient = new TCPDispatchedClient("TestConnection");
            //Uri hostname = new Uri(ServerSettings.DsfAddress);
            //dispatchClient.Connect(hostname.Host, hostname.Port);
            Assert.Inconclusive("This test is redundant as it is tested else where");
            //Assert.IsTrue(dispatchClient.Connections[0].Alive, "An error occured during the connect call on the TCPDispatchClient");
        }

        // Sashen.Naidoo : 13-02-2012 : Bug 8791 : Test to check that invalid URI's throws an exception in the TCPDispatchedClient
        //                                         this is to check that the correct exception is thrown.
        [TestMethod]
        //[ExpectedException(typeof(InvalidOperationException))]
        public void TcpDispatchedClientLoginUnavailableServerExpected()
        {
            //TCPDispatchedClient dispatchClient = new TCPDispatchedClient("TestConnection");
            //Uri hostname = new Uri("http://somewhereOutThere:99/ddd");
            //dispatchClient.Connect(hostname.Host, hostname.Port);
            //dispatchClient.Login("myTestuser", "pwd");

            Assert.Inconclusive("This test is redundant as it is tested else where");
        }

        #endregion TCPDispatchedClient Tests

        #region LoginAsync

        [TestMethod]
        public void LoginAsyncWithInvalidCredentialsReturnsFalse()
        {
            LoginAyncTest("qwerty", "1111", false, 1, AuthenticationResponse.InvalidCredentials, true);
        }

        [TestMethod]
        public void LoginAsyncWithValidCredentialsReturnsTrue()
        {
            LoginAyncTest("OnlyThePasswordMatters", "abc123xyz", true, 1, AuthenticationResponse.Success, false);
        }

        [TestMethod]
        public void LoginAsyncWithValidCredentialsTwiceReturnsDoesNotAuthenticateTwice()
        {
            LoginAyncTest("OnlyThePasswordMatters", "abc123xyz", true, 1, AuthenticationResponse.Success, false, true);
        }

        [TestMethod]
        public void LoginAsyncWithNullIdentityExpectedReturnFalse()
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
                    if (connectTask.Result)
                    {
                        var loginTask = host.LoginAsync(userName, password);
                        return loginTask.Result;
                    }
                    return false;
                });
                try
                {
                    task.Wait(GlobalConstants.NetworkTimeOut);
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
                Assert.AreEqual(task.Result, expectedResult, expectedResult ? "Could not log into server" : "Should not be logged into server");
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

        #region CreateConnection

        static TcpConnection CreateConnection(bool isAuxiliary = false)
        {
            return CreateConnection(ServerSettings.DsfAddress,isAuxiliary);
        }

        static TcpConnection CreateConnection(string appServerUri, bool isAuxiliary = false)
        {
            var securityContetxt = new Mock<IFrameworkSecurityContext>();
            securityContetxt.Setup(c => c.UserIdentity).Returns(WindowsIdentity.GetCurrent());

            return new TcpConnection(securityContetxt.Object, new Uri(appServerUri), Int32.Parse(ServerSettings.WebserverPort), isAuxiliary);
        }

        #endregion
    }
}