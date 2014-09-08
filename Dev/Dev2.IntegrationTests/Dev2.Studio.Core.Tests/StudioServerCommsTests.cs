using System;
using System.Threading;
using Dev2.Network;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            Thread.Sleep(100);
            conn.Connect();
            Thread.Sleep(500);
            bool afterReconnection = conn.IsConnected;

            Assert.IsTrue(afterReconnection);

            conn.Disconnect();
        }


        #endregion Environment Connection Tests

        #region LoginAyncTest

        #endregion

        #region CreateConnection

        static IEnvironmentConnection CreateConnection()
        {
            return CreateConnection(ServerSettings.DsfAddress);
        }

        static IEnvironmentConnection CreateConnection(string appServerUri)
        {

            return new ServerProxy(new Uri(appServerUri));
        }

        #endregion
    }
}