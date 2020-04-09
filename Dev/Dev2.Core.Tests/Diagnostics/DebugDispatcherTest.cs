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
using System.Threading;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Logging;
using Dev2.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Warewolf.Data;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    public class DebugDispatcherTest
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_AddWithNull()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var countBefore = debugDispatcher.Count;

            debugDispatcher.Add(workspaceID, null);

            Assert.AreEqual(countBefore, debugDispatcher.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_AddWithWriter()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();

            debugDispatcher.Add(workspaceID, writer.Object);

            var theWriter = debugDispatcher.Get(workspaceID);

            Assert.AreEqual(writer.Object, theWriter);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_AddAfterShutdown_DoesNotAdd()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();

            Assert.AreEqual(0, debugDispatcher.Count);

            debugDispatcher.Shutdown();
            debugDispatcher.Add(workspaceID, writer.Object);

            var theWriter = debugDispatcher.Get(workspaceID);

            Assert.AreEqual(0, debugDispatcher.Count);
            Assert.AreEqual(null, theWriter);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_RemoveWithInvalidID()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            debugDispatcher.Add(workspaceID, writer.Object);

            var countBefore = debugDispatcher.Count;
            debugDispatcher.Remove(Guid.NewGuid());
            Assert.AreEqual(countBefore, debugDispatcher.Count);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_RemoveWithValidID()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            debugDispatcher.Add(workspaceID, writer.Object);

            debugDispatcher.Remove(workspaceID);
            var theWriter = debugDispatcher.Get(workspaceID);
            Assert.IsNull(theWriter);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_GetWithInvalidID()
        {
            var debugDispatcher = new DebugDispatcherImplementation();

            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            debugDispatcher.Add(workspaceID, writer.Object);

            var result = debugDispatcher.Get(Guid.NewGuid());
            Assert.IsNull(result);

        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_GetWithValidID()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            debugDispatcher.Add(workspaceID, writer.Object);

            var result = debugDispatcher.Get(workspaceID);

            Assert.AreSame(writer.Object, result);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_WriteWithNull()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            debugDispatcher.Write(new WriteArgs { testName = "" });

            // No exception thrown
            Assert.IsTrue(true);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_Write()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            writer.Setup(s => s.Write(It.IsAny<IDebugNotification>())).Verifiable();
            debugDispatcher.Add(workspaceID, writer.Object);

            var mockState = new Mock<IDebugState>();
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            mockState.Setup(o => o.WorkspaceID).Returns(workspaceID);
            mockState.Setup(o => o.ClientID).Returns(clientId);
            mockState.Setup(o => o.SessionID).Returns(sessionId);
            mockState.Setup(o => o.IsFinalStep()).Returns(true);

            var expectedJson = JsonConvert.SerializeObject(mockState.Object, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });

            var writeArgs = new WriteArgs
            {
                debugState = mockState.Object
            };

            debugDispatcher.Write(writeArgs);
            debugDispatcher.Write(writeArgs);
            debugDispatcher.Write(writeArgs);

            Thread.Sleep(50);
            writer.Verify(s => s.Write(expectedJson), Times.Exactly(3));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_Write_WhenRemoteInvoke_ExpectRemoteItemsAddedToRepo()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            writer.Setup(s => s.Write(It.IsAny<IDebugNotification>())).Verifiable();

            debugDispatcher.Add(workspaceID, writer.Object);

            var state = new Mock<IDebugState>();
            state.Setup(s => s.WorkspaceID).Returns(workspaceID);

            var remoteID = Guid.NewGuid();

            var writeArgs = new WriteArgs
            {
                debugState = state.Object,
                isRemoteInvoke = true,
                remoteInvokerId = remoteID.ToString()
            };

            //------------Execute Test---------------------------
            debugDispatcher.Write(writeArgs);

            //------------Assert Results-------------------------

            // Write happens asynchronously on a separate thread
            Thread.Sleep(50);
            writer.Verify(s => s.Write(It.IsAny<IDebugNotification>()), Times.Exactly(0));
            var items = RemoteDebugMessageRepo.Instance.FetchDebugItems(remoteID);
            Assert.AreEqual(1, items.Count);
            Assert.IsNotNull(items[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_Write_IsTestExecution()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var mockState = new Mock<IDebugState>();
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var sourceResourceId = Guid.NewGuid();

            mockState.Setup(o => o.WorkspaceID).Returns(workspaceID);
            mockState.Setup(o => o.SourceResourceID).Returns(sourceResourceId);
            mockState.Setup(o => o.ClientID).Returns(clientId);
            mockState.Setup(o => o.SessionID).Returns(sessionId);
            mockState.Setup(o => o.IsFinalStep()).Returns(true);

            debugDispatcher.Write(new WriteArgs { debugState = mockState.Object, isTestExecution = true, testName = "testname1" });

            var items = TestDebugMessageRepo.Instance.FetchDebugItems(sourceResourceId, "testname1");

            Assert.AreEqual(1, items.Count);
            Assert.AreSame(mockState.Object, items[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_Write_IsDebugFromWeb()
        {
            var debugDispatcher = new DebugDispatcherImplementation();
            var workspaceID = Guid.NewGuid();
            var mockState = new Mock<IDebugState>();
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            mockState.Setup(o => o.WorkspaceID).Returns(workspaceID);
            mockState.Setup(o => o.ClientID).Returns(clientId);
            mockState.Setup(o => o.SessionID).Returns(sessionId);
            mockState.Setup(o => o.IsFinalStep()).Returns(true);

            debugDispatcher.Write(new WriteArgs { debugState = mockState.Object, isDebugFromWeb = true, testName = "testname" });

            var items = WebDebugMessageRepo.Instance.FetchDebugItems(clientId, sessionId);

            Assert.AreEqual(1, items.Count);
            Assert.AreSame(mockState.Object, items[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_Write_RemoteInvokeDebugItems()
        {
            var mockLogger = new Mock<ILogger>();
            var debugDispatcher = new DebugDispatcherImplementation(mockLogger.Object);

            var workspaceID = Guid.NewGuid();
            var mockState = new Mock<IDebugState>();
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            var originatingResourceID = Guid.NewGuid();

            mockState.Setup(o => o.WorkspaceID).Returns(workspaceID);
            mockState.Setup(o => o.OriginatingResourceID).Returns(originatingResourceID);
            mockState.Setup(o => o.ClientID).Returns(clientId);
            mockState.Setup(o => o.SessionID).Returns(sessionId);

            var remoteDebugItemsSessionId = Guid.NewGuid();
            var remoteDebugItem1 = new DebugState
            {
                ParentID = Guid.Empty,
                SessionID = remoteDebugItemsSessionId,
            };
            var remoteDebugItem2 = new DebugState
            {
                SessionID = remoteDebugItemsSessionId,
            };
            var remoteDebugItems = new List<IDebugState>
            {
                remoteDebugItem1,
                remoteDebugItem2,
            };

            var remoteInvokerId = Guid.NewGuid();
            var parentInstanceId = Guid.NewGuid();
            debugDispatcher.Write(new WriteArgs { debugState = mockState.Object, testName = "testname2", remoteInvokerId = remoteInvokerId.ToString(), parentInstanceId = parentInstanceId.ToString(), remoteDebugItems = remoteDebugItems });

            var items = DebugMessageRepo.Instance.FetchDebugItems(clientId, sessionId);

            Assert.AreEqual(1, items.Count);
            Assert.AreSame(mockState.Object, items[0]);

            items = DebugMessageRepo.Instance.FetchDebugItems(clientId, remoteDebugItemsSessionId);

            Assert.AreEqual(2, items.Count);
            Assert.AreSame(remoteDebugItem1, items[0]);
            Assert.AreSame(remoteDebugItem2, items[1]);

            Assert.AreEqual(workspaceID, items[0].WorkspaceID);
            Assert.AreEqual(originatingResourceID, items[0].OriginatingResourceID);
            Assert.AreEqual(clientId, items[0].ClientID);
            Assert.AreEqual(remoteInvokerId, items[0].EnvironmentID);
            Assert.AreEqual(null, items[0].ParentID);

            Assert.AreEqual(workspaceID, items[1].WorkspaceID);
            Assert.AreEqual(originatingResourceID, items[1].OriginatingResourceID);
            Assert.AreEqual(clientId, items[1].ClientID);
            Assert.AreEqual(remoteInvokerId, items[1].EnvironmentID);
            Assert.AreEqual(null, items[1].ParentID);

            mockLogger.Verify(o => o.Debug("EnvironmentID: 00000000-0000-0000-0000-000000000000 Debug:", "Warewolf Debug"), Times.Once);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(DebugDispatcher))]
        public void DebugDispatcher_Instance()
        {
            var instance1 = DebugDispatcher.Instance;
            var instance2 = DebugDispatcher.Instance;

            Assert.AreSame(instance1, instance2);
        }
    }
}
