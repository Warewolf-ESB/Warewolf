/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for DeleteTestTool: argument, existence and Contribute-permission checks, a missing test
 *  reported rather than treated as a no-op success, and the .tests directory going with the last test.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class DeleteTestToolTests
    {
        TestToolFixture _fixture = null!;

        [TestInitialize]
        public void Setup() => _fixture = new TestToolFixture();

        [TestCleanup]
        public void Cleanup() => _fixture.Dispose();

        DeleteTestResult Delete(string name, string testName) => DeleteTestTool.Handle(_fixture.HostConfig(), TestToolFixture.OpenPolicy, null, name, testName);

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws() => Delete("   ", "AnyTest");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankTestName_Throws() => Delete(_fixture.CreateWorkflow(), "   ");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_WorkflowNotFound_Throws() => Delete("NoSuchWorkflow", "AnyTest");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_TestNotFound_Throws_RatherThanReportingADelete()
        {
            var name = _fixture.CreateWorkflow();

            var error = Assert.ThrowsException<McpException>(() => Delete(name, "NeverCreated"));

            StringAssert.Contains(error.Message, "nothing was deleted");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ViewOnlyPermission_Throws_AndLeavesTheTest()
        {
            var name = _fixture.CreateWorkflow();
            await _fixture.CreateTest(name, new { testName = "OutputIs10" });

            Assert.ThrowsException<McpException>(() => DeleteTestTool.Handle(
                _fixture.HostConfig(), TestToolFixture.Granting(WorkflowPermission.View | WorkflowPermission.Execute), TestToolFixture.Principal("Viewers"), name, "OutputIs10"));

            Assert.IsTrue(TestCatalog.Exists(_fixture.Root, name, "OutputIs10"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Valid_DeletesTheTest_AndListTestsNoLongerShowsIt()
        {
            var name = _fixture.CreateWorkflow();
            await _fixture.CreateTest(name, new { testName = "Keep" });
            await _fixture.CreateTest(name, new { testName = "Drop" });

            var result = Delete(name, "Drop");

            Assert.AreEqual(name, result.Name);
            Assert.AreEqual("Drop", result.TestName);
            Assert.IsTrue(result.Deleted);
            var remaining = ListTestsTool.Handle(_fixture.HostConfig(), TestToolFixture.OpenPolicy, null, name).Tests.Select(t => t.TestName);
            CollectionAssert.AreEqual(new[] { "Keep" }, remaining.ToArray());
            Assert.IsTrue(Directory.Exists(TestCatalog.TestsDirectoryFor(_fixture.Root, name)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_DeletingTheLastTest_RemovesTheTestsDirectory()
        {
            var name = _fixture.CreateWorkflow();
            await _fixture.CreateTest(name, new { testName = "Only" });

            Delete(name, "Only");

            Assert.IsFalse(Directory.Exists(TestCatalog.TestsDirectoryFor(_fixture.Root, name)));
        }
    }
}
