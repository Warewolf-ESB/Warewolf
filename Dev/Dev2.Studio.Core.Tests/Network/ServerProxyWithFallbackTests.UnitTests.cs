using System;
using System.Net;
using Dev2.Network;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Core.Tests.Network
{
    [TestClass]
    public partial class ServerProxyWithFallbackTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_Constructor_DefaultConstruction_ShouldHaveEsbProxy()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serverProxy = new TestServerProxyWithFallback();
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverProxy);
            Assert.IsNotNull(serverProxy.HubConnection);
            Assert.IsNotNull(serverProxy.EsbProxy);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_ExecuteCommand")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerProxy_ExecuteCommand_WhenNullPayload_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxyWithFallback();
            //------------Execute Test---------------------------
            serverProxy.ExecuteCommand(null, Guid.NewGuid());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_Constructor_DefaultConstruction_ShouldHaveEsbProxyWithSendMemoSubscription()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serverProxy = new TestServerProxyWithFallback();
            //------------Assert Results-------------------------
            var subscription = serverProxy.EsbProxy.Subscribe("SendMemo");
            Assert.IsNotNull(subscription);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_Constructor_DefaultConstruction_ShouldHaveEsbProxyWithSendDebugStateSubscription()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serverProxy = new TestServerProxyWithFallback();
            //------------Assert Results-------------------------
            var subscription = serverProxy.EsbProxy.Subscribe("SendDebugState");
            Assert.IsNotNull(subscription);
        }        
        
    }

    internal class TestServerProxyWithFallback : ServerProxy
    {
        // TODO: Move this constructor to a test class!!
        public TestServerProxyWithFallback(string uri, string userName, string password)
            : base(uri, userName, password)
        {
        }
        public TestServerProxyWithFallback()
            : base("http://localhost:8080", CredentialCache.DefaultCredentials, new SynchronousAsyncWorker())
        {

        }





        #region Overrides of ServerProxy



        #endregion
    }
}