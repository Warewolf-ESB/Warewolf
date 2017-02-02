/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Net;
using System.Threading.Tasks;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.SignalR.Wrappers;
using Dev2.SignalR.Wrappers.New;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Network
{
    [TestClass]
    public partial class ServerProxyTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_Constructor_DefaultConstruction_ShouldHaveEsbProxy()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serverProxy = new TestServerProxy();
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverProxy);
            Assert.IsNotNull(serverProxy.HubConnection);
            Assert.IsNotNull(serverProxy.EsbProxy);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_Constructor_ParameterUserNamePasswordWebserverURI_ShouldHaveEsbProxy()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serverProxy = new TestServerProxy("http://localhost:8080", "some user", "some password");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serverProxy);
            Assert.IsNotNull(serverProxy.HubConnection);
            Assert.IsNotNull(serverProxy.EsbProxy);
            Assert.AreEqual("some user", serverProxy.UserName);
            Assert.AreEqual("some password", serverProxy.Password);
            Assert.AreEqual(AuthenticationType.User, serverProxy.AuthenticationType);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_ExecuteCommand")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerProxy_ExecuteCommand_WhenNullPayload_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxy();
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
            var serverProxy = new TestServerProxy();
            //------------Assert Results-------------------------
            var subscription = serverProxy.EsbProxy.Subscribe("SendMemo");
            Assert.IsNotNull(subscription);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_StateChange")]
        public void ServerProxy_StateChange_FromConnectedToReconnecting_IsAuthorizedFalse()
        {
            //------------Setup for test--------------------------
            bool _permissionsChangedFired = false;

            var serverProxy = new TestServerProxy();
            serverProxy.PermissionsChanged += (sender, args) =>
                {
                    _permissionsChangedFired = true;
                };
            bool authorisedBeforeStateChange = serverProxy.IsAuthorized;
            //------------Execute Test---------------------------
            serverProxy.CallHubConnectionChanged(new StateChangeWrapped(ConnectionStateWrapped.Connected, ConnectionStateWrapped.Reconnecting));
            //------------Assert Results-------------------------
            Assert.IsTrue(authorisedBeforeStateChange);
            Assert.IsFalse(serverProxy.IsAuthorized);
            Assert.IsTrue(_permissionsChangedFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_Constructor_DefaultConstruction_ShouldHaveEsbProxyWithSendDebugStateSubscription()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serverProxy = new TestServerProxy();
            //------------Assert Results-------------------------
            var subscription = serverProxy.EsbProxy.Subscribe("SendDebugState");
            Assert.IsNotNull(subscription);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ServerProxy_Connect")]
        public void ServerProxy_ConnectSetsId()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serverProxy = new TestServerProxy();
            var x = Guid.NewGuid();
            try
            {
                serverProxy.Connect(x);
            }
// ReSharper disable EmptyGeneralCatchClause
            catch
// ReSharper restore EmptyGeneralCatchClause
            {
                
             
            }
            
            //------------Assert Results-------------------------
            Assert.AreEqual(x,serverProxy.ID);
        }
        
        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_IsLocalhost")]
        public void ServerProxy_IsLocalHost_DisplayNamelocalhost_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxy
            {
                IsConnected = true,
                DisplayName = "localhost"
            };
            //------------Execute Test---------------------------
            var isLocalHost = serverProxy.IsLocalHost;

            //------------Assert Results-------------------------
            Assert.IsTrue(serverProxy.IsConnected);
            Assert.IsTrue(isLocalHost);
        }

        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_IsLocalhost")]
        public void ServerProxy_IsLocalHost_DisplayNameLoCaLhOst_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxy
            {
                IsConnected = true,
                DisplayName = "LoCaLhOst"
            };
            //------------Execute Test---------------------------
            var isLocalHost = serverProxy.IsLocalHost;

            //------------Assert Results-------------------------
            Assert.IsTrue(serverProxy.IsConnected);
            Assert.IsTrue(isLocalHost);
        }

        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_IsLocalhost")]
        public void ServerProxy_IsLocalHost_DisplayNamelocalhostConnected_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxy
            {
                IsConnected = true,
                DisplayName = "localhost (Connected)"
            };
            //------------Execute Test---------------------------
            var isLocalHost = serverProxy.IsLocalHost;

            //------------Assert Results-------------------------
            Assert.IsTrue(serverProxy.IsConnected);
            Assert.IsTrue(isLocalHost);
        }

        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_IsLocalhost")]
        public void ServerProxy_IsLocalHost_DisplayNamelocalhost2_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxy
            {
                IsConnected = true,
                DisplayName = "localhost 2"
            };
            //------------Execute Test---------------------------
            var isLocalHost = serverProxy.IsLocalHost;

            //------------Assert Results-------------------------
            Assert.IsTrue(serverProxy.IsConnected);
            Assert.IsFalse(isLocalHost);
        }

        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_IsLocalhost")]
        public void ServerProxy_IsLocalHost_DisplayNameEmpty_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxy
            {
                IsConnected = true,
                DisplayName = ""
            };
            //------------Execute Test---------------------------
            var isLocalHost = serverProxy.IsLocalHost;

            //------------Assert Results-------------------------
            Assert.IsTrue(serverProxy.IsConnected);
            Assert.IsFalse(isLocalHost);
        }
        
        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_IsLocalhost")]
        public void ServerProxy_IsLocalHost_DisplayNameNull_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var serverProxy = new TestServerProxy
            {
                IsConnected = true,
                DisplayName = null
            };
            //------------Execute Test---------------------------
            var isLocalHost = serverProxy.IsLocalHost;

            //------------Assert Results-------------------------
            Assert.IsTrue(serverProxy.IsConnected);
            Assert.IsFalse(isLocalHost);
        }

    }

    internal class TestServerProxy : ServerProxyWithoutChunking
    {
        // TODO: Move this constructor to a test class!!
        public TestServerProxy(string uri, string userName, string password)
            : base(uri, userName, password)
        {
        }
        public TestServerProxy()
            : base("http://localhost:8080", CredentialCache.DefaultCredentials, new SynchronousAsyncWorker())
        {

        }

        public void CallHubConnectionChanged(IStateChangeWrapped stateChange)
        {
            HubConnectionStateChanged(stateChange);
        }

        public void SetEsbProxy(IHubProxyWrapper hubProxy)
        {
            EsbProxy = hubProxy;
        }


        #region Overrides of ServerProxy


        protected override void Wait(Task task)
        {
            task.Start();
        }

        #endregion
    }
}
