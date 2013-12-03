using System;
using System.Threading.Tasks;
using Dev2.Communication;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Network
{
    public partial class ServerProxyTests
    {
        //Given a ServerProxy
        //When I Execute the ExecuteCommand
        //Then the EsbProxy is invoked with ExecuteCommand
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_ExecuteCommand")]
        public void ServerProxy_ExecuteCommand_WithArgs_ShouldInvokeCorrectly()
        {
            //------------Setup for test--------------------------
            var mockHubProxy = new Mock<IHubProxy>();
            const string ExpectedResult = "YaY";
            mockHubProxy.Setup(proxy => proxy.Invoke<string>("ExecuteCommand", It.IsAny<Envelope>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new Task<string>(() => ExpectedResult));
            var serverProxy = new TestServerProxy();
            serverProxy.SetEsbProxy(mockHubProxy.Object);
            //------------Execute Test---------------------------
            var resultOfExecution = serverProxy.ExecuteCommand("some payload", Guid.NewGuid(), Guid.NewGuid());
            //------------Assert Results-------------------------
            mockHubProxy.VerifyAll();
            Assert.AreEqual(ExpectedResult, resultOfExecution);
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
            var mockHubProxy = new Mock<IHubProxy>();
            mockHubProxy.Setup(proxy => proxy.Invoke("AddDebugWriter", It.IsAny<Guid>())).Returns(new Task(() => { }));
            var serverProxy = new TestServerProxy();
            serverProxy.SetEsbProxy(mockHubProxy.Object);
            //------------Execute Test---------------------------
            serverProxy.AddDebugWriter(Guid.NewGuid());
            //------------Assert Results-------------------------
            mockHubProxy.VerifyAll();
        }
    }
}