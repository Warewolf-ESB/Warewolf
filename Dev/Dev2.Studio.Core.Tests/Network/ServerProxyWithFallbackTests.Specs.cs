using System;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Explorer;
using Dev2.Network;
using Dev2.SignalR.Wrappers;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Network
{
    public partial class ServerProxyWithFallbackTests
    {
        //Given a ServerProxy
        //When I Execute the ExecuteCommand
        //Then the EsbProxy is invoked with ExecuteCommand
        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_ExecuteCommand")]
        public void ServerProxy_ExecuteCommand_WithArgs_ShouldInvokeCorrectly()
        {
            //------------Setup for test--------------------------
            var serverMsg = "server result";
            var mockHubProxy = new Mock<IHubProxyWrapper>();
            var ExpectedResult = new Receipt {PartID = 0, ResultParts = 1};
            mockHubProxy.Setup(proxy => proxy.Invoke<Receipt>("ExecuteCommand", It.IsAny<Envelope>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new Task<Receipt>(() => ExpectedResult));
            mockHubProxy.Setup(proxy => proxy.Invoke<string>("FetchExecutePayloadFragment", It.IsAny<FutureReceipt>())).Returns(new Task<string>(() => serverMsg));
            var serverProxy = new TestServerProxy();
            serverProxy.SetEsbProxy(mockHubProxy.Object);
            //------------Execute Test---------------------------
            var resultOfExecution = serverProxy.ExecuteCommand(new StringBuilder("some payload"), Guid.NewGuid(), Guid.NewGuid());
            //------------Assert Results-------------------------
            mockHubProxy.VerifyAll();
            Assert.AreEqual(serverMsg, resultOfExecution.ToString());
        }

        //Given a ServerProxy
        //When I Execute the AddDebugWriter
        //Then the EsbProxy is invoked with AddDebugWriter
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_AddDebugWriter")]
        public void ServerProxy_AddDebugWriter_WithArgs_ShouldInvokeCorrectly()
        {
            //------------Setup for test--------------------------
            var mockHubProxy = new Mock<IHubProxyWrapper>();
            mockHubProxy.Setup(proxy => proxy.Invoke("AddDebugWriter", It.IsAny<Guid>())).Returns(new Task(() => { }));
            var serverProxy = new TestServerProxy();
            serverProxy.SetEsbProxy(mockHubProxy.Object);
            //------------Execute Test---------------------------
            serverProxy.AddDebugWriter(Guid.NewGuid());
            //------------Assert Results-------------------------
            mockHubProxy.VerifyAll();
        }


        [TestMethod, Timeout(5000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_FallbackOnConnectWithError()
        {
            //------------Setup for test--------------------------
            var serverProxy = new ServerProxy(new Uri("http://bob"));
            var serverGuid = Guid.NewGuid();
            PrivateObject p = new PrivateObject(serverProxy);
            var wrapped = new Mock<IEnvironmentConnection>();
            var fallback = new Mock<IEnvironmentConnection>();
            wrapped.Setup(a => a.Connect(It.IsAny<Guid>())).Throws(new FallbackException());
            p.SetField("_wrappedConnection",wrapped.Object);
            p.SetField("_fallbackConnection", fallback.Object);
            
            try
            {
                serverProxy.Connect(serverGuid);

            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {


            }
            var con = p.GetField("_wrappedConnection");
            Assert.AreEqual(fallback.Object,con);
           
        }

        [TestMethod, Timeout(5000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_FallbackOnConnect()
        {
            //------------Setup for test--------------------------
            var serverProxy = new ServerProxy(new Uri("http://bob"));
            var serverGuid = Guid.NewGuid();
            PrivateObject p = new PrivateObject(serverProxy);
            var wrapped = new Mock<IEnvironmentConnection>();
            var fallback = new Mock<IEnvironmentConnection>();
           
            p.SetField("_wrappedConnection", wrapped.Object);
            p.SetField("_fallbackConnection", fallback.Object);

            try
            {
                serverProxy.Connect(serverGuid);

            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {


            }
            var con = p.GetField("_wrappedConnection");
            Assert.AreNotEqual(fallback.Object, con);

        }


        [TestMethod, Timeout(5000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_NoFallbackOnConnectIfNormalException()
        {
            //------------Setup for test--------------------------
            var serverProxy = new ServerProxy(new Uri("http://bob"));
            var serverGuid = Guid.NewGuid();
            PrivateObject p = new PrivateObject(serverProxy);
            var wrapped = new Mock<IEnvironmentConnection>();
            var fallback = new Mock<IEnvironmentConnection>();
            wrapped.Setup(a => a.Connect(It.IsAny<Guid>())).Throws(new Exception());
            p.SetField("_wrappedConnection", wrapped.Object);
            p.SetField("_fallbackConnection", fallback.Object);

            try
            {
                serverProxy.Connect(serverGuid);

            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {


            }
            var con = p.GetField("_wrappedConnection");
            Assert.AreNotEqual(fallback.Object, con);

        }
    }
}