/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.SignalR.Wrappers;
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
        [TestMethod, Timeout(3000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_ExecuteCommand")]
        public void ServerProxy_ExecuteCommand_WithArgs_ShouldInvokeCorrectly()
        {
            //------------Setup for test--------------------------
            const string serverMsg = "server result";
            var mockHubProxy = new Mock<IHubProxyWrapper>();
            var receipt = new Receipt();
            mockHubProxy.Setup(proxy => proxy.Invoke<Receipt>("ExecuteCommand", It.IsAny<Envelope>(), It.IsAny<bool>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(receipt));
            mockHubProxy.Setup(proxy => proxy.Invoke<string>("FetchExecutePayloadFragment", It.IsAny<FutureReceipt>())).Returns(Task.FromResult(serverMsg));
            var serverProxy = new TestServerProxy();
            serverProxy.SetEsbProxy(mockHubProxy.Object);
            //------------Execute Test---------------------------
            var resultOfExecution = serverProxy.ExecuteCommand(new StringBuilder("some payload"), Guid.NewGuid());
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
    }
}
