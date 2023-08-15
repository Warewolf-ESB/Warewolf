/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Threading.Tasks;
using Dev2.Network;
using Dev2.SignalR.Wrappers;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Network
{
    public partial class ServerProxyWithFallbackTests
    {
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
            var p = new Warewolf.Testing.PrivateObject(serverProxy);
            var wrapped = new Mock<IEnvironmentConnection>();
            var fallback = new Mock<IEnvironmentConnection>();
            wrapped.Setup(a => a.Connect(It.IsAny<Guid>())).Throws(new FallbackException());
            p.SetField("_wrappedConnection", wrapped.Object);
            try
            {
                serverProxy.Connect(serverGuid);
            }

            catch

            {
                // ignored
            }

            var con = p.GetField("_wrappedConnection");
            Assert.IsNotNull(con);
        }

        [TestMethod, Timeout(5000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerProxy_Constructor")]
        public void ServerProxy_NoFallbackOnConnectIfNormalException()
        {
            //------------Setup for test--------------------------
            var serverProxy = new ServerProxy(new Uri("http://bob"));
            var serverGuid = Guid.NewGuid();
            var p = new Warewolf.Testing.PrivateObject(serverProxy);
            var wrapped = new Mock<IEnvironmentConnection>();
            var fallback = new Mock<IEnvironmentConnection>();
            wrapped.Setup(a => a.Connect(It.IsAny<Guid>())).Throws(new Exception());
            p.SetField("_wrappedConnection", wrapped.Object);

            try
            {
                serverProxy.Connect(serverGuid);
            }

            catch

            {
                // ignored
            }

            var con = p.GetField("_wrappedConnection");
            Assert.IsNotNull(con);
        }
    }
}