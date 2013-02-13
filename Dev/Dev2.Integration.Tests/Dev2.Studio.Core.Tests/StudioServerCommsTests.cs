using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Studio.Core;
using System.Network;
using Dev2.Integration.Tests.MEF;
using Dev2.Composition;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests
{
    // BUG 8801 + 8796
    // Sashen.Naidoo : 13-02-2012 : Tests the Studio can always create a connection to the server
    //                              & that the Studio always performs it's connection actions in a 
    //                              synchronous fashion.
    [TestClass]
    public class StudioServerCommsTests
    {
        #region Test Members

        private ImportServiceContext importServiceContext;

        #endregion Test Members

        #region Addition Test Attributes

        [ClassInitialize]
        public static void StudioServerCommsInitialize(TestContext testContext)
        {
        }

        [TestInitialize]
        public void StudioServerCommsInitialize()
        {
            if (importServiceContext == null)
            {
                importServiceContext = CompositionInitializer.DefaultInitialize();
            }
        }

        #endregion Additional Test Attributes

        #region Environment Connection Tests

        // Sashen.Naidoo: 13-02-2012 : Bug 8801 + Bug 8796
        // This is to ensure that the studio's environment connection does not go haywire when initialized
        // The issue was with the TCPDispatchClient that the EnvironmentConnection was
        // The Dispatch client would have to use Asynchronous method calls in an asynchronous fashion to the Studio.
        // And the server would send the studio a ClientDetails message on Connection (required by the Studio)
        // that was just ignored by the Studio.
        [TestMethod]
        public void EnvironmentConnection_WithServerAuthentication_Expected_ClientDetailsRecieved()
        {

            EnvironmentConnection conn = SetupEnvironmentConnection();
            conn.Address = new Uri(ServerSettings.DsfAddress);

            conn.Connect();
            // The IsConnected property of the EnvironmentConnection references the TCPDispatch Client
            // Only if the connection to the server is successfully made by the dispatch client will the
            // IsConnected message return true
            Assert.IsTrue(conn.IsConnected);

            conn.Disconnect();
        }



        [TestMethod]
        public void EnvironmentConnection_ReconnectToServer_Expeceted_ClientConnectionSuccessful()
        {
            EnvironmentConnection conn = SetupEnvironmentConnection();

            conn.Connect();
            conn.Disconnect();
            bool beforeReconnection = conn.IsConnected;
            conn.Connect();
            bool afterReconnection = conn.IsConnected;

            Assert.AreNotEqual(beforeReconnection, afterReconnection);
            Assert.IsTrue(afterReconnection);

            conn.Disconnect();
        }


        // A reconnection spam used to cause a set of issues in the Studio
        // This was how the bug replicated itself, because the studio did not wait for the
        // server to return information
        [TestMethod]
        public void EnvironmentConnection_ReconnectionSpam_Expected_AlwaysReconnects()
        {
            // We will perform 10 connections and check if the studio can always connect to the server
            EnvironmentConnection environmentConn = SetupEnvironmentConnection();
            System.Collections.Generic.List<bool> actualConnections = new System.Collections.Generic.List<bool>();
            System.Collections.Generic.List<bool> expectedConnections = new System.Collections.Generic.List<bool>();
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
        public void TCPDispatchedClient_Login_Expected_LoginResponseFromServer()
        {
            TCPDispatchedClient dispatchClient = new TCPDispatchedClient("TestConnection");
            Uri hostname = new Uri(ServerSettings.DsfAddress);
            dispatchClient.Connect(hostname.Host, hostname.Port); 
            dispatchClient.Login("myTestuser", "pwd");
            Assert.IsTrue(dispatchClient.LoggedIn, "The TCPDispatchedClient was unable to login to the server");
        }

        [TestMethod]
        public void TCPDispatchedClient_Connect_Expected_ConnectionEstablishedToServer() {
            TCPDispatchedClient dispatchClient = new TCPDispatchedClient("TestConnection");
            Uri hostname = new Uri(ServerSettings.DsfAddress);
            dispatchClient.Connect(hostname.Host, hostname.Port); 
            Assert.IsTrue(dispatchClient.Connections[0].Alive, "An error occured during the connect call on the TCPDispatchClient");
        }


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TCPDispatchedClient_Login_UnavailableServer_Expected_()
        {
            TCPDispatchedClient dispatchClient = new TCPDispatchedClient("TestConnection");
            Uri hostname = new Uri("http://somewhereOutThere:99/ddd");
            dispatchClient.Connect(hostname.Host, hostname.Port);
            dispatchClient.Login("myTestuser", "pwd");
        }

        #endregion TCPDispatchedClient Tests

        #region Private Test Methods

        private EnvironmentConnection SetupEnvironmentConnection()
        {
            EnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);

            return conn;
        }

        #endregion Private Test Methods

    }
}
