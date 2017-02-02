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
using System.Threading;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
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

            DebugDispatcher.Instance.Write(null,false,"");

            // No exception thrown
            Assert.IsTrue(true);

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
            DebugDispatcher.Instance.Write(state.Object, false,"",true, remoteID.ToString()); // queue remote item ;)

            //------------Assert Results-------------------------

            // Write happens asynchronously on a separate thread
            Thread.Sleep(50);
            writer.Verify(s => s.Write(It.IsAny<IDebugState>()), Times.Exactly(0));
            var items = RemoteDebugMessageRepo.Instance.FetchDebugItems(remoteID);
            Assert.AreEqual(1, items.Count);
            Assert.IsNotNull(items[0]);
        }


        #endregion

    }
}
