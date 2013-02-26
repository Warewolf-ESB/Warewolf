using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Diagnostics;

namespace Unlimited.UnitTest.Framework.Diagnostics
{
    [TestClass]
    public class DebugDispatcherTest
    {
        
        static object l = new object();

        object _testGuard = new object();
        [TestInitialize]
        public void TestInit()
        {
            Monitor.Enter(_testGuard);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            Monitor.Exit(_testGuard);
        }

        #region Add

        [TestMethod]
        public void AddWithNull()
        {
            lock (l)
            {
            var workspaceID = Guid.NewGuid();
            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Add(workspaceID, null);
            Assert.AreEqual(countBefore, DebugDispatcher.Instance.Count);
        }
        }

        [TestMethod]
        public void AddWithWriter()
        {
            lock (l)
            {
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();

            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);
            IDebugWriter theWriter = DebugDispatcher.Instance.Get(workspaceID);
            Assert.AreEqual(writer.Object, theWriter);
        }
        }

        #endregion

        #region Remove

        [TestMethod]
        public void RemoveWithInvalidID()
        {
            lock (l)
            {
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Remove(Guid.NewGuid());
            Assert.AreEqual(countBefore, DebugDispatcher.Instance.Count);
        }
        }

        [TestMethod]
        public void RemoveWithValidID()
        {
            lock (l)
            {
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var countBefore = DebugDispatcher.Instance.Count;
            DebugDispatcher.Instance.Remove(workspaceID);
            IDebugWriter theWriter = DebugDispatcher.Instance.Get(workspaceID);
            Assert.IsNull(theWriter);
        }
        }

        #endregion

        #region Get

        [TestMethod]
        public void GetWithInvalidID()
        {
            lock (l)
            {
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var result = DebugDispatcher.Instance.Get(Guid.NewGuid());
            Assert.IsNull(result);
        }
        }

        [TestMethod]
        public void GetWithValidID()
        {
            lock (l)
            {
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var result = DebugDispatcher.Instance.Get(workspaceID);
            Assert.AreSame(writer.Object, result);
        }
        }

        #endregion

        #region Write

        [TestMethod]
        public void WriteWithNull()
        {
            lock (l)
            {
            var task = DebugDispatcher.Instance.Write(null);
            Assert.IsNull(task);
        }
        }

        [TestMethod]
        public void WriteWithValidState()
        {
            lock (l)
            {
            var workspaceID = Guid.NewGuid();
            var writer = new Mock<IDebugWriter>();
            DebugDispatcher.Instance.Add(workspaceID, writer.Object);

            var state = new Mock<IDebugState>();
            state.Setup(s => s.WorkspaceID).Returns(workspaceID);
            state.Setup(s => s.Write(writer.Object)).Verifiable();
            var task = DebugDispatcher.Instance.Write(state.Object);
                Task.WaitAll(new[] {task});
            state.Verify(s => s.Write(writer.Object), Times.Exactly(1));
        }
        }

        #endregion

    }
}
