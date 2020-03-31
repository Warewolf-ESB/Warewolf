/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Tests
{
    [TestClass]
    [TestCategory(nameof(IWarewolfServerHub))]
    public class WarewolfServerHubTests
    {
        [TestMethod]
        public void IWarewolfServerHub_FollowLeader_NotifiesOnChanges()
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
            otherMocks.ForEach(o => o.Verify(o1 => o1.SendConfigUpdateNotification(), Times.Once));
        }

        [TestMethod]
        public void IWarewolfServerHub_SendDebugState_NotifiesOnChanges()
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
            otherMocks.ForEach(o => o.Verify(o1 => o1.SendDebugState(It.IsAny<string>()), Times.Never));
        }

        #region Setup
        protected class AllClientsWarewolfServerHub : IWarewolfServerHub
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

        protected (List<IWarewolfServerHub>, List<Mock<IWarewolfServerHub>>) SetupOthers()
        {
            var mocks = new List<Mock<IWarewolfServerHub>>
            {
                new Mock<IWarewolfServerHub>(),
                new Mock<IWarewolfServerHub>(),
                new Mock<IWarewolfServerHub>(),
            };
            return (mocks.Select(o => (o.Object)).ToList(), mocks);
        }
        protected (Mock<IHubCallerConnectionContext<IWarewolfServerHub>>, Mock<IWarewolfServerHub>, List<Mock<IWarewolfServerHub>>) SetupClients()
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
        protected Mock<IPrincipal> SetupPrincipleMock()
        {
            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(o => o.Identity.Name).Returns("bob");
            return mockPrinciple;
        }
        protected static Mock<IRequest> SetupMockRequest(Mock<IPrincipal> mockPrinciple)
        {
            var mockReq = new Mock<IRequest>();
            mockReq.Setup(o => o.User).Returns(mockPrinciple.Object);
            return mockReq;
        }
        #endregion
    }
}