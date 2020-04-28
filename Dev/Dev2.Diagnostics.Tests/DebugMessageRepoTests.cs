using System;
using Dev2.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class DebugMessageRepoTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenIsNew_ShouldInitiase()
        {
            //---------------Set up test pack-------------------
            var webDebugMessageRepo = DebugMessageRepo.Instance;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDebugMessageRepo);
            //---------------Execute Test ----------------------
            var instance = DebugMessageRepo.Instance;
            Assert.IsNotNull(instance);
            //---------------Test Result -----------------------
            Assert.IsTrue(ReferenceEquals(instance, webDebugMessageRepo));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddDebugItem_GivenValidArgs_ShouldAdd1Item()
        {
            //---------------Set up test pack-------------------
            var webDebugMessageRepo = DebugMessageRepo.Instance;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDebugMessageRepo);
            //---------------Execute Test ----------------------
            var debugState = new DebugState();
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            webDebugMessageRepo.AddDebugItem(clientId, sessionId, debugState);
            //---------------Test Result -----------------------
            var fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId, sessionId);
            Assert.AreEqual(1, fetchDebugItems.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddDebugItem_GivenValidArgsAndSessions_ShouldNotMixUpDebugStates()
        {
            //---------------Set up test pack-------------------
            var webDebugMessageRepo = DebugMessageRepo.Instance;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDebugMessageRepo);
            //---------------Execute Test ----------------------
            var id = Guid.NewGuid();
            var debugState = new DebugState() { ID = id };
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            webDebugMessageRepo.AddDebugItem(clientId, sessionId, debugState);
            var clientId1 = Guid.NewGuid();
            var sessionId1 = Guid.NewGuid();
            var id1 = Guid.NewGuid();
            var debugState1 = new DebugState() { ID = id1 };
            webDebugMessageRepo.AddDebugItem(clientId1, sessionId1, debugState1);
            //---------------Test Result -----------------------
            var fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId, sessionId);
            Assert.AreEqual(1, fetchDebugItems.Count);
            fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId1, sessionId1);
            Assert.AreEqual(1, fetchDebugItems.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void AddDebugItem_GivenKeyExists_ShouldNotMixUpDebugStates()
        {
            //---------------Set up test pack-------------------
            var webDebugMessageRepo = DebugMessageRepo.Instance;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webDebugMessageRepo);
            //---------------Execute Test ----------------------
            var id = Guid.NewGuid();
            var debugState = new DebugState() { ID = id };
            var clientId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();
            webDebugMessageRepo.AddDebugItem(clientId, sessionId, debugState);
            var clientId1 = Guid.NewGuid();
            var sessionId1 = Guid.NewGuid();
            var id1 = Guid.NewGuid();
            var debugState1 = new DebugState() { ID = id1 };
            webDebugMessageRepo.AddDebugItem(clientId1, sessionId1, debugState1);
            webDebugMessageRepo.AddDebugItem(clientId1, sessionId1, debugState1);
            //---------------Test Result -----------------------
            var fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId, sessionId);
            Assert.AreEqual(1, fetchDebugItems.Count);
            fetchDebugItems = webDebugMessageRepo.FetchDebugItems(clientId1, sessionId1);
            Assert.AreEqual(2, fetchDebugItems.Count);
        }
    }
}