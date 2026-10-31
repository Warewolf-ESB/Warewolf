/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ListTestsTool: argument and View-permission checks (permission before existence),
 *  the empty and sorted listings with their counts, and a corrupt .test.json listed as invalid
 *  without failing the call.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System.IO;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class ListTestsToolTests
    {
        TestToolFixture _fixture = null!;

        [TestInitialize]
        public void Setup() => _fixture = new TestToolFixture();

        [TestCleanup]
        public void Cleanup() => _fixture.Dispose();

        ListTestsResult List(string name) => ListTestsTool.Handle(_fixture.HostConfig(), TestToolFixture.OpenPolicy, null, name);

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws() => List("   ");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_WorkflowNotFound_Throws() => List("NoSuchWorkflow");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NoViewPermission_Throws_WithTheSameErrorWhetherOrNotTheWorkflowExists()
        {
            var name = _fixture.CreateWorkflow();
            var loader = TestToolFixture.Granting(WorkflowPermission.None);

            var existing = Assert.ThrowsException<McpException>(
                () => ListTestsTool.Handle(_fixture.HostConfig(), loader, TestToolFixture.Principal("Developers"), name));
            var missing = Assert.ThrowsException<McpException>(
                () => ListTestsTool.Handle(_fixture.HostConfig(), loader, TestToolFixture.Principal("Developers"), "NoSuchWorkflow"));

            StringAssert.Contains(existing.Message, "permission");
            StringAssert.Contains(missing.Message, "permission");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ViewPermission_IsEnough()
        {
            var name = _fixture.CreateWorkflow();

            var result = ListTestsTool.Handle(_fixture.HostConfig(), TestToolFixture.Granting(WorkflowPermission.View), TestToolFixture.Principal("Viewers"), name);

            Assert.AreEqual(0, result.Tests.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WorkflowWithNoTests_ReturnsEmptyList()
        {
            var name = _fixture.CreateWorkflow();

            var result = List(name);

            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(0, result.Tests.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async System.Threading.Tasks.Task Handle_ListsTestsSortedByName_WithCounts()
        {
            var name = _fixture.CreateWorkflow();
            await _fixture.CreateTest(name, new
            {
                testName = "Zeta",
                enabled = false,
                inputs = new[] { new { variable = "[[a]]", value = "1" }, new { variable = "[[b]]", value = "2" } },
                outputs = new[] { new { variable = "[[Result]]", value = "hello", assertOp = "=" } },
                testSteps = new object[]
                {
                    new { activityId = _fixture.AssignActivityId, type = "Mock", children = new[] { new { activityId = _fixture.AssignActivityId, type = "Assert" } } },
                    new { activityId = _fixture.AssignActivityId, type = "Assert" },
                },
            });
            await _fixture.CreateTest(name, new { testName = "Alpha" });

            var tests = List(name).Tests;

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual(new TestSummary("Alpha", true, 0, 0, 0, 0), tests[0]);
            Assert.AreEqual(new TestSummary("Zeta", false, 2, 1, 1, 2), tests[1]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async System.Threading.Tasks.Task Handle_CorruptTestFile_IsListedAsInvalid_AndTheOthersStillList()
        {
            var name = _fixture.CreateWorkflow();
            await _fixture.CreateTest(name, new { testName = "Good" });
            File.WriteAllText(TestCatalog.TestFilePath(_fixture.Root, name, "Broken"), "{ not json");

            var tests = List(name).Tests;

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("Broken", tests[0].TestName);
            Assert.IsTrue(tests[0].Invalid);
            Assert.IsNotNull(tests[0].Error);
            Assert.IsNull(tests[0].Enabled);
            Assert.AreEqual(new TestSummary("Good", true, 0, 0, 0, 0), tests[1]);
        }
    }
}
