/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  End-to-end unit tests for ExecuteTestTool, against a real compiled workflow and a real
 *  WorkflowExecutor (not a fake) — this is the regression guard for the whole Phase 3 design:
 *  Execute-permission gating, missing-test rejection, a Mock step's substituted value actually
 *  flowing into the final environment (proving TestMockActivityResolver + the never-pooled
 *  PreparedWorkflow design work together), a failing top-level Outputs assertion, and a regular
 *  (non-mocked) Assert step being evaluated automatically by the SHARED Dev2.Activities engine —
 *  no new per-step assertion code exists in this project; this is the proof it isn't needed.
 */

using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class ExecuteTestToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _root = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), "execute-test-tests-" + Guid.NewGuid().ToString("N"))).FullName;
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "false");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", null);
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        private sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
        {
            public bool IsConfigEffective { get; set; }
            public int PolicyCount => 0;
            public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } =
                (_, _) => WorkflowPermission.None;

            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
                EffectivePermissions(workflowName, callerRoles);

            public void Reload() { }
        }

        private sealed class NoOpSecretResolver : IMcpSecretResolver
        {
            public Task<string> ResolveAsync(string name, System.Threading.CancellationToken cancellationToken) =>
                throw new McpException($"Secret reference '${{{name}}}' could not be resolved.");
        }

        private sealed class NullExecutionLogger : IExecutionLogger
        {
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };
        static readonly NoOpSecretResolver NoSecrets = new();

        static IWorkflowExecutor NewExecutor() => new WorkflowExecutor(new NullExecutionLogger());

        static JsonElement TestOf(object obj) => System.Text.Json.JsonSerializer.SerializeToElement(obj);

        readonly string _assignActivityId = Guid.NewGuid().ToString();

        /// <summary>{ start -> Assign([[Result]] = "hello") } — a single regular activity, so a
        /// Mock step's substituted value is directly observable in the final environment.</summary>
        string CreateFixtureWorkflow(string name = "TargetWorkflow")
        {
            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
            var start = new Cell { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };
            var assign = new Cell
            {
                id = _assignActivityId,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = "Assign",
                    ["fields"] = fields,
                }
            };
            var edge = new Cell { id = "e1", shape = "edge", data = new Dictionary<string, object>(), Source = new Connector("start"), Target = new Connector(_assignActivityId) };

            var graph = new X6WorkflowSaveModel { ResourceName = name, Cells = new List<Cell> { start, assign, edge } };
            var body = JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
            var envelope = System.Text.Json.JsonSerializer.SerializeToElement(new
            {
                name,
                description = "",
                inputs = Array.Empty<object>(),
                outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
            });

            CreateWorkflowTool.Handle(HostConfig(), OpenPolicy, null, name, envelope, body);
            return name;
        }

        async Task CreateTest(string workflowName, object test) =>
            await CreateTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, workflowName, TestOf(test));

        // ── Tests: input validation / resolution / permission ──────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_BlankName_Throws() =>
            await ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, "   ", "AnyTest");

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_BlankTestName_Throws()
        {
            var name = CreateFixtureWorkflow();
            await ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, name, "   ");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_WorkflowNotFound_Throws() =>
            await ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, "NoSuchWorkflow", "AnyTest");

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_NoExecutePermission_Throws()
        {
            var name = CreateFixtureWorkflow();
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.View };

            await Assert.ThrowsExceptionAsync<McpException>(
                () => ExecuteTestTool.Handle(HostConfig(), loader, NewExecutor(), Principal("Developers"), name, "AnyTest"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_TestNotFound_Throws()
        {
            var name = CreateFixtureWorkflow();

            await Assert.ThrowsExceptionAsync<McpException>(
                () => ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, name, "NeverCreated"));
        }

        // ── Tests: mock substitution (Phase 3's core new behavior) ─────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_MockedStep_SubstitutedValueFlowsToOutput_TestPassed()
        {
            var name = CreateFixtureWorkflow();
            await CreateTest(name, new
            {
                testName = "MockPasses",
                outputs = new[] { new { variable = "[[Result]]", value = "mocked-value", assertOp = "=" } },
                testSteps = new[]
                {
                    new
                    {
                        activityId = _assignActivityId,
                        activityType = "DsfDotNetMultiAssignActivity",
                        type = "Mock",
                        stepOutputs = new[] { new { variable = "[[Result]]", value = "mocked-value" } },
                    },
                },
            });

            var result = await ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, name, "MockPasses");

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.TestPassed, result.Message);
            Assert.AreEqual("TestPassed", result.Result);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_MockedStep_OutputAssertsRealValue_TestFails()
        {
            // The real activity would produce "hello" — asserting that value while the step is
            // mocked to "mocked-value" must fail, proving the mock (not the real activity) ran.
            var name = CreateFixtureWorkflow("MockFailsWf");
            await CreateTest(name, new
            {
                testName = "MockFails",
                outputs = new[] { new { variable = "[[Result]]", value = "hello", assertOp = "=" } },
                testSteps = new[]
                {
                    new
                    {
                        activityId = _assignActivityId,
                        activityType = "DsfDotNetMultiAssignActivity",
                        type = "Mock",
                        stepOutputs = new[] { new { variable = "[[Result]]", value = "mocked-value" } },
                    },
                },
            });

            var result = await ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, name, "MockFails");

            Assert.IsTrue(result.IsSuccess, "Execution itself must still succeed — only the assertion fails.");
            Assert.IsFalse(result.TestPassed);
        }

        // ── Tests: per-step Assert, evaluated automatically by shared Dev2.Activities code ─────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_RegularAssertStep_MatchingRealOutput_MarkedTestPassed()
        {
            var name = CreateFixtureWorkflow("AssertStepWf");
            await CreateTest(name, new
            {
                testName = "AssertStep",
                testSteps = new[]
                {
                    new
                    {
                        activityId = _assignActivityId,
                        activityType = "DsfDotNetMultiAssignActivity",
                        type = "Assert",
                        stepOutputs = new[] { new { variable = "[[Result]]", value = "hello", assertOp = "=" } },
                    },
                },
            });

            var result = await ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, name, "AssertStep");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Steps.Count);
            Assert.AreEqual("TestPassed", result.Steps[0].Result,
                "The regular-activity Assert path is handled entirely by shared Dev2.Activities code (DsfNativeActivity.UpdateWithAssertions) — no new assertion logic exists in this project.");
            Assert.IsTrue(result.TestPassed);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_RegularAssertStep_MismatchedRealOutput_MarkedTestFailed()
        {
            var name = CreateFixtureWorkflow("AssertStepFailsWf");
            await CreateTest(name, new
            {
                testName = "AssertStepFails",
                testSteps = new[]
                {
                    new
                    {
                        activityId = _assignActivityId,
                        activityType = "DsfDotNetMultiAssignActivity",
                        type = "Assert",
                        stepOutputs = new[] { new { variable = "[[Result]]", value = "not-hello", assertOp = "=" } },
                    },
                },
            });

            var result = await ExecuteTestTool.Handle(HostConfig(), OpenPolicy, NewExecutor(), null, name, "AssertStepFails");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("TestFailed", result.Steps[0].Result);
            Assert.IsFalse(result.TestPassed);
        }
    }
}
