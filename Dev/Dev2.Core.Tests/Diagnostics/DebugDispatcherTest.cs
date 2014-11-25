
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.ESB.WF;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DebugDispatcherTest
    {

        #region Add

        [TestMethod]
        public void AddWithNull()
        {

            var workspaceID = Guid.NewGuid();
            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Add(workspaceID, null);
            Assert.AreEqual(countBefore, DebugDispatcher.Instance.Count);

        }

        [TestMethod]
        public void AddWithWriter()
        {

            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();

            DebugDispatcher.Instance.Add(workspaceID, writer.Object);
            var theWriter = DebugDispatcher.Instance.Get(workspaceID);
            Assert.AreEqual(writer.Object, theWriter);

        }

        #endregion

        #region Remove

        [TestMethod]
        public void RemoveWithInvalidID()
        {

            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Remove(Guid.NewGuid());
            Assert.AreEqual(countBefore, DebugDispatcher.Instance.Count);

        }

        [TestMethod]
        public void RemoveWithValidID()
        {

            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            DebugDispatcher.Instance.Remove(workspaceID);
            var theWriter = DebugDispatcher.Instance.Get(workspaceID);
            Assert.IsNull(theWriter);

        }

        #endregion

        #region Get

        [TestMethod]
        public void GetWithInvalidID()
        {

            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var result = DebugDispatcher.Instance.Get(Guid.NewGuid());
            Assert.IsNull(result);

        }

        [TestMethod]
        public void GetWithValidID()
        {

            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var result = DebugDispatcher.Instance.Get(workspaceID);
            Assert.AreSame(writer.Object, result);

        }

        #endregion

        #region Write

        [TestMethod]
        public void WriteWithNull()
        {

            DebugDispatcher.Instance.Write(null);

            // No exception thrown
            Assert.IsTrue(true);

        }

        [TestMethod]
        public void WriteWithValidState()
        {

            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            writer.Setup(s => s.Write(It.IsAny<IDebugState>())).Verifiable();

            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var state = new Mock<IDebugState>();
            state.Setup(s => s.WorkspaceID).Returns(workspaceID);
            DebugDispatcher.Instance.Write(state.Object);

            // Write happens asynchronously on a separate thread
            Thread.Sleep(50);
            writer.Verify(s => s.Write(It.IsAny<IDebugState>()), Times.Exactly(1));

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DebugDispatcher_Write")]
        // ReSharper disable InconsistentNaming
        public void DebugDispatcher_Write_WhenRemoteInvoke_ExpectRemoteItemsAddedToRepo()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            writer.Setup(s => s.Write(It.IsAny<IDebugState>())).Verifiable();

            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var state = new Mock<IDebugState>();
            state.Setup(s => s.WorkspaceID).Returns(workspaceID);

            var remoteID = Guid.NewGuid();

            //------------Execute Test---------------------------
            DebugDispatcher.Instance.Write(state.Object, true, remoteID.ToString()); // queue remote item ;)

            //------------Assert Results-------------------------

            // Write happens asynchronously on a separate thread
            Thread.Sleep(50);
            writer.Verify(s => s.Write(It.IsAny<IDebugState>()), Times.Exactly(0));
            var items = RemoteDebugMessageRepo.Instance.FetchDebugItems(remoteID);
            Assert.AreEqual(1, items.Count);
            Assert.IsNotNull(items[0]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DebugDispatcher_Write")]
        // ReSharper disable InconsistentNaming
        public void DebugDispatcher_Write_WhenRemoteInvokeItemsPresent_ExpectRemoteItemsDispatched()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            writer.Setup(s => s.Write(It.IsAny<IDebugState>())).Verifiable();

            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var state = new Mock<IDebugState>();
            state.Setup(s => s.WorkspaceID).Returns(workspaceID);

            var remoteID = Guid.NewGuid();

            //------------Execute Test---------------------------
            DebugDispatcher.Instance.Write(state.Object, false, remoteID.ToString(), null, new List<IDebugState> { state.Object }); // queue remote item ;)

            //------------Assert Results-------------------------

            // Write happens asynchronously on a separate thread
            Thread.Sleep(50);
            writer.Verify(s => s.Write(It.IsAny<IDebugState>()), Times.Exactly(2));
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DebugDispatcher_Write")]
        // ReSharper disable InconsistentNaming
        public void DebugDispatcher_Write_WhenRemoteInvokeItemsPresentAfterEndMessage_ExpectRemoteItemsDispatchedBeforeEndMessage()
        // ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            writer.Setup(s => s.Write(It.IsAny<IDebugState>())).Verifiable();
            var writerList = new List<IDebugState>();
            // .Callback<ISqlBulkCopy, DataTable>((sqlBulkCopy, dataTable) => returnedDataTable = dataTable);
            // ReSharper disable ConvertClosureToMethodGroup
            writer.Setup(w => w.Write(It.IsAny<IDebugState>())).Callback<IDebugState>(ds =>
            {
                writerList.Add(ds);
            });
            // ReSharper restore ConvertClosureToMethodGroup

            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var origResourceID = Guid.NewGuid();
            var server = Guid.NewGuid();

            var state2 = new Mock<IDebugState>();
            state2.Setup(s => s.WorkspaceID).Returns(workspaceID);
            state2.Setup(s => s.OriginatingResourceID).Returns(origResourceID);
            state2.Setup(s => s.Server).Returns(server.ToString);
            state2.Setup(s => s.StateType).Returns(StateType.Message);

            var state1 = new Mock<IDebugState>();
            state1.Setup(s => s.WorkspaceID).Returns(workspaceID);
            state1.Setup(s => s.OriginatingResourceID).Returns(origResourceID);
            state1.Setup(s => s.Server).Returns(server.ToString);
            state1.Setup(s => s.StateType).Returns(StateType.Message);

            var utils = new WfApplicationUtils();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(d => d.RemoteDebugItems).Returns(new List<IDebugState> { state1.Object, state2.Object });
            dataObject.Setup(d => d.IsDebugMode()).Returns(true);
            dataObject.Setup(d => d.WorkspaceID).Returns(workspaceID);

            //------------Execute Test---------------------------

            ErrorResultTO errors;
            utils.DispatchDebugState(dataObject.Object, StateType.End, false, string.Empty, out errors);

            //------------Assert Results-------------------------

            // Write happens asynchronously on a separate thread
            Thread.Sleep(50);
            writer.Verify(s => s.Write(It.IsAny<IDebugState>()), Times.Exactly(3));

            Assert.AreEqual(3, writerList.Count);
            // Now ensure ordering is correct ;)
            Assert.AreEqual(state1.Object, writerList[0]);
            Assert.AreEqual(state2.Object, writerList[1]);
            // ensure end state last to dispatch ;)
            Assert.AreEqual(StateType.End, writerList[2].StateType);

        }


        #endregion

    }
}
