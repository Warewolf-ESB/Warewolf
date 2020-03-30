/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Hubs;
using Warewolf.Common;
using Warewolf.Esb;

namespace Warewolf.Framework48.Tests
{
    [TestClass]
    public class ResourceCatalogProxyTests
    {
        [TestMethod]
        public void ResourceCatalogProxy_Construct()
        {
            var proxy = GetResourceCatalog();
            Assert.IsNotNull(proxy);
        }

        [TestMethod]
        public void ResourceCatalogProxy_GetResourceById_ReturnsResource()
        {
            var environmentConnection = GetConnection();
            var proxy = new ResourceCatalogProxy(environmentConnection);
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            var resource = proxy.GetResourceById<RabbitMQSource>(Guid.Empty, resourceId);

            Assert.IsTrue(resource.IsSource);
            Assert.AreEqual("test", resource.UserName);
            Assert.AreEqual("test", resource.Password);
        }

        [TestMethod]
        public void ResourceCatalogProxy_GetResourceById_ReturnsResource2()
        {
            var environmentConnection = GetConnection();
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            var req = environmentConnection.NewResourceRequest<RabbitMQSource>(Guid.Empty, resourceId);
            var resource = req.Result;

            Assert.IsTrue(resource.IsSource);
            Assert.AreEqual("test", resource.UserName);
            Assert.AreEqual("test", resource.Password);
        }

        [TestMethod]
        public void IHubProxy_GetResource_ReturnsResource()
        {
            var expected = "{\"$id\": \"1\",\"$type\": \"Dev2.Data.ServiceModel.RabbitMQSource, Dev2.Data\",\"UserName\": \"test\",\"Password\": \"test\",\"ResourceID\": \"5d82c480-505e-48e9-9915-aca0293be30c\"}";

            // setup mock hub proxy
            var mockProxy = new Mock<IHubProxy>();
            mockProxy.Setup(o => o.Invoke<Receipt>(It.IsAny<string>(), It.IsAny<object[]>())).Returns(Task<Receipt>.Factory.StartNew(() => new Receipt()));
            mockProxy.Setup(o => o.Invoke<string>(It.IsAny<string>(), It.IsAny<object[]>())).Returns(Task<string>.Factory.StartNew(() => expected));
            var proxy = mockProxy.Object;

            // run test
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            var request = new ResourceRequest<RabbitMQSource>(Guid.Empty, resourceId);
            var task = proxy.ExecReq2<RabbitMQSource>(request);
            var resource = task.Result;

            Assert.IsTrue(resource.IsSource);
            Assert.AreEqual("test", resource.UserName);
            Assert.AreEqual("test", resource.Password);
        }

        class AllClientsWarewolfServerHub : IWarewolfServerHub
        {
            private readonly List<IWarewolfServerHub> _others;
            private readonly IWarewolfServerHub _caller;

            public AllClientsWarewolfServerHub(IWarewolfServerHub caller, List<IWarewolfServerHub> others)
            {
                _caller = caller;
                _others = others;
            }

            public void ItemAddedMessage(string item)
            {
                _caller.ItemAddedMessage(item);
                _others.ForEach(o => o.ItemAddedMessage(item));
            }

            public void LeaderConfigChange()
            {
                _caller.LeaderConfigChange();
                _others.ForEach(o => o.LeaderConfigChange());
            }

            public void SendPermissionsMemo(string serializedMemo)
            {
                _caller.SendPermissionsMemo(serializedMemo);
                _others.ForEach(o => o.SendPermissionsMemo(serializedMemo));
            }

            public void SendDebugState(string serializedDebugState)
            {
                _caller.SendDebugState(serializedDebugState);
                _others.ForEach(o => o.SendDebugState(serializedDebugState));
            }

            public void SendWorkspaceID(Guid workspaceId)
            {
                _caller.SendWorkspaceID(workspaceId);
                _others.ForEach(o => o.SendWorkspaceID(workspaceId));
            }

            public void SendServerID(Guid serverId)
            {
                _caller.SendServerID(serverId);
                _others.ForEach(o => o.SendServerID(serverId));
            }

            public void SendConfigUpdateNotification()
            {
                _caller.SendConfigUpdateNotification();
                _others.ForEach(o => o.SendConfigUpdateNotification());
            }
        }

        (List<IWarewolfServerHub>, List<Mock<IWarewolfServerHub>>) SetupOthers()
        {
            var mocks = new List<Mock<IWarewolfServerHub>>
            {
                new Mock<IWarewolfServerHub>(),
                new Mock<IWarewolfServerHub>(),
                new Mock<IWarewolfServerHub>(),
            };
            return (mocks.Select(o => (o.Object)).ToList(), mocks);
        }
        (Mock<IHubCallerConnectionContext<IWarewolfServerHub>>, Mock<IWarewolfServerHub>, List<Mock<IWarewolfServerHub>>) SetupClients()
        {
            var mockCaller = new Mock<IWarewolfServerHub>();
            var caller = mockCaller.Object;
            var (others, otherMocks) = SetupOthers();
            var all = new AllClientsWarewolfServerHub(caller, others);
            var mockClients = new Mock<IHubCallerConnectionContext<IWarewolfServerHub>>();
            mockClients.Setup(o => o.User(It.IsAny<string>())).Returns(caller);
            mockClients.Setup(o => o.All).Returns(all);

            return (mockClients, mockCaller, otherMocks);
        }
        Mock<IPrincipal> SetupPrincipleMock()
        {
            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(o => o.Identity.Name).Returns("bob");
            return mockPrinciple;
        }
        private static Mock<IRequest> SetupMockRequest(Mock<IPrincipal> mockPrinciple)
        {
            var mockReq = new Mock<IRequest>();
            mockReq.Setup(o => o.User).Returns(mockPrinciple.Object);
            return mockReq;
        }

        [TestMethod]
        public void IHubProxy_SendDebugState_NotifiesOnChanges()
        {
            // setup mock hub proxy
            var hub = new EsbHub();
            var mockPrinciple = SetupPrincipleMock();
            var mockReq = SetupMockRequest(mockPrinciple);
            hub.Context = new HubCallerContext(mockReq.Object, "");
            var (mockClients, mockCaller, otherMocks) = SetupClients();
            hub.Clients = mockClients.Object;

            hub.SendDebugState(new DebugState());

            mockCaller.Verify(o => o.SendDebugState(It.IsAny<string>()), Times.Once);
            otherMocks.Apply(o => o.Verify(o1 => o1.SendDebugState(It.IsAny<string>()), Times.Never));
            //mockProxy.Setup(o => o.Subscribe(nameof(LeaderNotification))).Returns();
            //var proxy = mockProxy.Object;

          /*  // run test
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            var request = new EventRequest<LeaderNotification>(Guid.Empty);
            var watcher = proxy.Watch<LeaderNotification>(request);
            watcher.OnChange += (LeaderNotification notification) => { notified = true; };

            Assert.IsTrue(notified);*/

        }

        [TestMethod]
        public void IHubProxy_FollowLeader_NotifiesOnChanges()
        {
            // setup mock hub proxy
            var hub = new EsbHub();
            var mockPrinciple = SetupPrincipleMock();
            var mockReq = SetupMockRequest(mockPrinciple);
            hub.Context = new HubCallerContext(mockReq.Object, "");
            var (mockClients, mockCaller, otherMocks) = SetupClients();
            hub.Clients = mockClients.Object;

            hub.SendConfigUpdateNotification();

            mockCaller.Verify(o => o.SendConfigUpdateNotification(), Times.Once);
            otherMocks.Apply(o => o.Verify(o1 => o1.SendConfigUpdateNotification(), Times.Once));
            //mockProxy.Setup(o => o.Subscribe(nameof(LeaderNotification))).Returns();
            //var proxy = mockProxy.Object;

            /*  // run test
              var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
              var request = new EventRequest<LeaderNotification>(Guid.Empty);
              var watcher = proxy.Watch<LeaderNotification>(request);
              watcher.OnChange += (LeaderNotification notification) => { notified = true; };

              Assert.IsTrue(notified);*/

        }


        private static ResourceCatalogProxy GetResourceCatalog()
        {
            var environmentConnection = GetConnection();
            var proxy = new ResourceCatalogProxy(environmentConnection);
            return proxy;
        }

        private static IEnvironmentConnection GetConnection()
        {
            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            mockEnvironmentConnection.Setup(o => o.IsConnected).Returns(true);
            var returnValue =
                new StringBuilder("{\"$id\": \"1\",\"$type\": \"Dev2.Data.ServiceModel.RabbitMQSource, Dev2.Data\",\"UserName\": \"test\",\"Password\": \"test\",\"ResourceID\": \"5d82c480-505e-48e9-9915-aca0293be30c\"}");
            mockEnvironmentConnection.Setup(o => o.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<int>())).Returns(returnValue);
            mockEnvironmentConnection.Setup(o => o.ExecuteCommandAsync(It.IsAny<ICatalogRequest>(), It.IsAny<Guid>()))
                .ReturnsAsync(returnValue);

            var environmentConnection = mockEnvironmentConnection.Object;
            return environmentConnection;
        }
    }
}
