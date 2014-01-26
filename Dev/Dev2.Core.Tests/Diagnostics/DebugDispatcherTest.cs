using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Unlimited.UnitTest.Framework.Diagnostics
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

            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);
            IDebugWriter theWriter = DebugDispatcher.Instance.Get(workspaceID);
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

            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Remove(workspaceID);
            IDebugWriter theWriter = DebugDispatcher.Instance.Get(workspaceID);
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
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var state = new Mock<IDebugState>();
            state.Setup(s => s.WorkspaceID).Returns(workspaceID);
            state.Setup(s => s.Write(writer.Object)).Verifiable();
            DebugDispatcher.Instance.Write(state.Object);

            // Write happens asynchronously on a separate thread
            Thread.Sleep(50);
            state.Verify(s => s.Write(writer.Object), Times.Exactly(1));

        }

        #endregion

    }
}
