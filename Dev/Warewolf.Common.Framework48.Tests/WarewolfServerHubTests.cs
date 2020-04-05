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
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.WebServer.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Client;
using Warewolf.Data;
using Warewolf.Options;
using Service = Warewolf.Service;

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

            var changeNotification = new ChangeNotification();
            hub.Write(changeNotification);

            mockCaller.Verify(o => o.ChangeNotification(changeNotification), Times.Once);
            otherMocks.ForEach(o => o.Verify(o1 => o1.ChangeNotification(changeNotification), Times.Once));
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

        [TestMethod]
        public void IWarewolfServerHub_FetchResourcesAffectedMemo_ExpectNoAffectedResources()
        {
            var resourceId = Guid.NewGuid();
            var expectedList = new List<ICompileMessageTO>();
            
            // setup mock hub proxy
            var hub = new EsbHub();
            var mockPrinciple = SetupPrincipleMock();
            var mockReq = SetupMockRequest(mockPrinciple);
            hub.Context = new HubCallerContext(mockReq.Object, "");
            var (mockClients, mockCaller, otherMocks) = SetupClients();
            hub.Clients = mockClients.Object;
            
            var result = hub.FetchResourcesAffectedMemo(resourceId);
            
            // TODO: this should assert no a real affected memo, but we can't call SendResourcesAffectedMemo
            Assert.IsNull(result.Result);
        }

        [TestMethod]
        public void IWarewolfServerHub_AddDebugWriter_ExpectTaskCompleted()
        {
            var workspaceId = Guid.NewGuid();
            
            // setup mock hub proxy
            var hub = new EsbHub();
            var mockPrinciple = SetupPrincipleMock();
            var mockReq = SetupMockRequest(mockPrinciple);
            hub.Context = new HubCallerContext(mockReq.Object, "");
            var (mockClients, mockCaller, otherMocks) = SetupClients();
            hub.Clients = mockClients.Object;
            
            var result = hub.AddDebugWriter(workspaceId); // this hub is now associated with this workspace for debug messages
            result.Wait();
            Assert.IsNull(result.Exception);
            Assert.IsTrue(result.IsCompleted);
            Assert.IsFalse(result.IsFaulted);
        }

        [TestMethod]
        public void IWarewolfServerHub_FetchExecutePayloadFragment_()
        {
            // setup mock hub proxy
            var hub = new EsbHub();
            var mockPrinciple = SetupPrincipleMock();
            var mockReq = SetupMockRequest(mockPrinciple);
            hub.Context = new HubCallerContext(mockReq.Object, "");
            var (mockClients, mockCaller, otherMocks) = SetupClients();
            hub.Clients = mockClients.Object;
            
            var result = hub.FetchExecutePayloadFragment(new FutureReceipt());

            Assert.IsNull(result.Exception);
            Assert.IsTrue(result.IsCompleted);
        }

        [TestMethod]
        public void EsbHub_ExecuteCommand_GivenAnInternalService_ShouldHaveNotEmptyResult()
        {
            var workspaceId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();

            // setup mock hub proxy
            var hub = new EsbHub();
            var mockPrinciple = SetupPrincipleMock();
            Utilities.ServerUser = mockPrinciple.Object;

            var mockReq = SetupMockRequest(mockPrinciple);
            hub.Context = new HubCallerContext(mockReq.Object, connectionId.ToString());
            var (mockClients, mockCaller, otherMocks) = SetupClients();
            hub.Clients = mockClients.Object;

            var req = new EsbExecuteRequest
            {
                ServiceName = "FindOptionsBy",
            };
            req.AddArgument(Service.OptionsService.ParameterName, new StringBuilder(Service.OptionsService.GateResume));
            var serializer = new Dev2JsonSerializer();
            var envelope = new Envelope
            {
                Content = serializer.Serialize(req),
                PartID = 0,
                Type = typeof(Envelope),
            };
            var messageId = Guid.NewGuid();
            var endOfStream = false;
            var dataListId = Guid.Empty;
            var resultTask = hub.ExecuteCommand(envelope, endOfStream, workspaceId, dataListId, messageId);
            var result = resultTask.Result;
            
            Assert.IsNull(resultTask.Exception);
            Assert.IsTrue(resultTask.IsCompleted);
            
            Assert.AreEqual(0, result.PartID);
            Assert.AreEqual(1, result.ResultParts);

            var fetchResult = ResultsCache.Instance.FetchResult(new FutureReceipt() {PartID = 0, RequestID = messageId, User = "bob"});

            var options = serializer.Deserialize<IOption[]>(fetchResult);
            Assert.IsNotNull(options, $"expected a deserializable response to be cached");
            Assert.AreEqual(1, options.Length);

            var allHubClients = mockClients.Object.All as AllClientsWarewolfServerHub;
            Assert.AreEqual(0, allHubClients?.TotalMessageCount);
        }

        [TestMethod]
        [TestCategory(nameof(Service.Cluster))]
        public void EsbHub_ExecuteCommand_GivenValidSubscriptionRequest_ShouldNotifyOfChanges()
        {
            var workspaceId = Guid.NewGuid();
            var connectionId = Guid.NewGuid();

            // setup mock hub proxy
            var hub = new EsbHub();
            var mockPrinciple = SetupPrincipleMock();
            Utilities.ServerUser = mockPrinciple.Object;

            var mockReq = SetupMockRequest(mockPrinciple);
            hub.Context = new HubCallerContext(mockReq.Object, connectionId.ToString());
            var (mockClients, mockCaller, otherMocks) = SetupClients();
            hub.Clients = mockClients.Object;

            var req = new EsbExecuteRequest
            {
                ServiceName = nameof(Service.Cluster.ClusterJoinRequest),
            };
            req.AddArgument(Service.Cluster.ClusterJoinRequest.Key, new StringBuilder(Config.Cluster.Key));
            var serializer = new Dev2JsonSerializer();
            var envelope = new Envelope
            {
                Content = serializer.Serialize(req),
                PartID = 0,
                Type = typeof(Envelope),
            };
            var messageId = Guid.NewGuid();
            var endOfStream = false;
            var dataListId = Guid.Empty;
            var resultTask = hub.ExecuteCommand(envelope, endOfStream, workspaceId, dataListId, messageId);
            var result = resultTask.Result;
            
            Assert.IsNull(resultTask.Exception);
            Assert.IsTrue(resultTask.IsCompleted);
            
            Assert.IsNotNull(result, "internal service unexpectedly returned null");
            Assert.AreEqual(0, result.PartID);
            Assert.AreEqual(1, result.ResultParts);

            var fetchResult = ResultsCache.Instance.FetchResult(new FutureReceipt() {PartID = 0, RequestID = messageId, User = "bob"});

            var response = serializer.Deserialize<ClusterJoinResponse>(fetchResult);
            Assert.IsNotNull(response, $"expected a deserializable response to be cached");
            Assert.AreNotEqual(Guid.Empty, response.Token);

            var allHubClients = mockClients.Object.All as AllClientsWarewolfServerHub;
            Assert.AreEqual(1, allHubClients?.TotalMessageCount);
            mockCaller.Verify(o => o.ChangeNotification(It.IsAny<ChangeNotification>()), Times.Once);
            otherMocks.ForEach(o => o.Verify(o1 => o1.ChangeNotification(It.IsAny<ChangeNotification>()), Times.Once));
        }

        #region Setup
        protected class AllClientsWarewolfServerHub : IWarewolfServerHub
        {
            private readonly List<IWarewolfServerHub> _others;
            private readonly IWarewolfServerHub _caller;
            private int _totalMessageCount = 0;
            public int TotalMessageCount => _totalMessageCount;

            public AllClientsWarewolfServerHub(IWarewolfServerHub caller, List<IWarewolfServerHub> others)
            {
                _caller = caller;
                _others = others;
                _totalMessageCount = 0;
            }

            public void ItemAddedMessage(string item)
            {
                ++_totalMessageCount;
                _caller.ItemAddedMessage(item);
                _others.ForEach(o => o.ItemAddedMessage(item));
            }

            public void LeaderConfigChange()
            {
                ++_totalMessageCount;
                _caller.LeaderConfigChange();
                _others.ForEach(o => o.LeaderConfigChange());
            }

            public void SendPermissionsMemo(string serializedMemo)
            {
                ++_totalMessageCount;
                _caller.SendPermissionsMemo(serializedMemo);
                _others.ForEach(o => o.SendPermissionsMemo(serializedMemo));
            }

            public void SendDebugState(string serializedDebugState)
            {
                ++_totalMessageCount;
                _caller.SendDebugState(serializedDebugState);
                _others.ForEach(o => o.SendDebugState(serializedDebugState));
            }

            public void SendWorkspaceID(Guid workspaceId)
            {
                ++_totalMessageCount;
                _caller.SendWorkspaceID(workspaceId);
                _others.ForEach(o => o.SendWorkspaceID(workspaceId));
            }

            public void SendServerID(Guid serverId)
            {
                ++_totalMessageCount;
                _caller.SendServerID(serverId);
                _others.ForEach(o => o.SendServerID(serverId));
            }

            public void ChangeNotification(ChangeNotification changeNotification)
            {
                ++_totalMessageCount;
                _caller.ChangeNotification(changeNotification);
                _others.ForEach(o => o.ChangeNotification(changeNotification));
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