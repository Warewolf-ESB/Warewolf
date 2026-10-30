/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetTestTool: argument, existence and View-permission checks, the definition coming
 *  back as create_test saved it, and a stored password never coming back (only passwordSet).
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System.Linq;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetTestToolTests
    {
        TestToolFixture _fixture = null!;

        [TestInitialize]
        public void Setup() => _fixture = new TestToolFixture();

        [TestCleanup]
        public void Cleanup() => _fixture.Dispose();

        GetTestResult Get(string name, string testName) => GetTestTool.Handle(_fixture.HostConfig(), TestToolFixture.OpenPolicy, null, name, testName);

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws() => Get("   ", "AnyTest");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankTestName_Throws() => Get(_fixture.CreateWorkflow(), "   ");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_PathLikeTestName_Throws() => Get(_fixture.CreateWorkflow(), "../TargetWorkflow");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_WorkflowNotFound_Throws() => Get("NoSuchWorkflow", "AnyTest");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_TestNotFound_Throws()
        {
            var name = _fixture.CreateWorkflow();

            var error = Assert.ThrowsException<McpException>(() => Get(name, "NeverCreated"));

            StringAssert.Contains(error.Message, "NeverCreated");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_NoViewPermission_Throws()
        {
            var name = _fixture.CreateWorkflow();
            await _fixture.CreateTest(name, new { testName = "OutputIs10" });

            Assert.ThrowsException<McpException>(() => GetTestTool.Handle(
                _fixture.HostConfig(), TestToolFixture.Granting(WorkflowPermission.Execute), TestToolFixture.Principal("Runners"), name, "OutputIs10"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ReturnsTheDefinitionCreateTestSaved()
        {
            var name = _fixture.CreateWorkflow();
            await _fixture.CreateTest(name, new
            {
                testName = "OutputIs10",
                inputs = new[] { new { variable = "[[a]]", value = "4" } },
                outputs = new[] { new { variable = "[[Result]]", value = "hello", assertOp = "Contains" } },
                testSteps = new[] { new { activityId = _fixture.AssignActivityId, type = "Mock", stepOutputs = new[] { new { variable = "[[Result]]", value = "mocked" } } } },
            });

            var result = Get(name, "OutputIs10");

            Assert.AreEqual(name, result.Name);
            Assert.IsFalse(result.PasswordSet);
            Assert.AreEqual("OutputIs10", result.Test.TestName);
            Assert.AreEqual("4", result.Test.Inputs!.Single().Value);
            Assert.AreEqual("Contains", result.Test.Outputs!.Single().AssertOp);
            var step = result.Test.TestSteps!.Single();
            Assert.AreEqual(_fixture.AssignActivityId, step.ActivityId);
            Assert.AreEqual("Mock", step.Type);
            Assert.AreEqual("mocked", step.StepOutputs!.Single().Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_StoredPassword_IsNeverReturned()
        {
            var name = _fixture.CreateWorkflow();
            // Written directly: create_test would DPAPI-encrypt a resolved secret, which needs Windows.
            TestCatalog.Save(_fixture.Root, name, "HasSecret", "{\"testName\":\"HasSecret\",\"authenticationType\":\"User\",\"userName\":\"svc\",\"password\":\"AQAAANCMnd8BFdERjHoAwE\"}");

            var result = Get(name, "HasSecret");

            Assert.IsTrue(result.PasswordSet);
            Assert.IsNull(result.Test.Password);
            Assert.AreEqual("svc", result.Test.UserName);
        }
    }
}
