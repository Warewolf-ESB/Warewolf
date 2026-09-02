/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for CreateTestTool: required-parameter validation, workflow-not-found rejection,
 *  Contribute-permission gating (same rule as create_workflow), activity-id validation against
 *  the target workflow's current body, StepType validation, name-already-exists rejection, the
 *  ${secret}-only password rule, and the success path — a written .test.json whose password (if
 *  any) is DPAPI-encrypted, never plaintext.
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
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class CreateTestToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "create-test-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches CreateWorkflowToolTests' conventions) ────────────

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

        private sealed class FakeSecretResolver : IMcpSecretResolver
        {
            private readonly Dictionary<string, string> _secrets = new(StringComparer.Ordinal);

            public FakeSecretResolver With(string name, string value)
            {
                _secrets[name] = value;
                return this;
            }

            public Task<string> ResolveAsync(string name, CancellationToken cancellationToken) =>
                _secrets.TryGetValue(name, out var value)
                    ? Task.FromResult(value)
                    : throw new McpException($"Secret reference '${{{name}}}' could not be resolved: no secret named '{name}' is staged.");
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };
        static readonly FakeSecretResolver NoSecrets = new();

        static JsonElement TestOf(object obj) => System.Text.Json.JsonSerializer.SerializeToElement(obj);

        Task<CreateTestResult> Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name,
            JsonElement test,
            IMcpSecretResolver? secretResolver = null) =>
            CreateTestTool.Handle(hostConfig, authPolicyLoader, secretResolver ?? NoSecrets, user, name, test);

        // ── Workflow fixture: { start -> Assign } with a real GUID node id, matching what a
        // real Studio-authored X6 graph looks like (X6ToWorkflowConverter stamps
        // dev2Activity.UniqueID = node.id verbatim, and every IServiceTestStep.ActivityID is
        // typed Guid — a non-GUID node id, like plain test-fixture strings such as "assign1",
        // would silently fail to round-trip through that Guid-typed field). ───────────────────

        readonly string _assignActivityId = Guid.NewGuid().ToString();

        string CreateFixtureWorkflow(string name = "TargetWorkflow")
        {
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[Result]]",
                ["FieldValue"] = "hello",
                ["IndexNumber"] = 1
            });
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

        object ValidMockStep() => new
        {
            activityId = _assignActivityId,
            activityType = "DsfDotNetMultiAssignActivity",
            type = "Mock",
            stepOutputs = new[] { new { variable = "[[Result]]", value = "mocked" } },
        };

        object ValidTest(string testName = "OutputIs10") => new
        {
            testName,
            inputs = Array.Empty<object>(),
            outputs = new[] { new { variable = "[[Result]]", value = "hello", assertOp = "=" } },
            testSteps = new[] { ValidMockStep() },
        };

        // ── Tests: input validation ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_BlankName_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "   ", TestOf(ValidTest()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_WorkflowNotFound_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NoSuchWorkflow", TestOf(ValidTest()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_BlankTestName_Throws()
        {
            var name = CreateFixtureWorkflow();
            await Handle(HostConfig(), OpenPolicy, null, name, TestOf(new { testName = "   " }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_TestNameWithSlash_Throws()
        {
            var name = CreateFixtureWorkflow();
            await Handle(HostConfig(), OpenPolicy, null, name, TestOf(new { testName = "Sub/Test" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_NoContributePermission_Throws()
        {
            var name = CreateFixtureWorkflow();
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.View };

            await Handle(HostConfig(), loader, Principal("Developers"), name, TestOf(ValidTest()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ActivityIdNotInBody_ThrowsWithoutWriting()
        {
            var name = CreateFixtureWorkflow("BadActivityIdWf");
            var test = TestOf(new
            {
                testName = "BadStep",
                testSteps = new[] { new { activityId = Guid.NewGuid().ToString(), activityType = "DsfDotNetMultiAssignActivity", type = "Mock" } },
            });

            await Assert.ThrowsExceptionAsync<McpException>(() => Handle(HostConfig(), OpenPolicy, null, name, test));
            Assert.IsFalse(TestCatalog.Exists(HostConfig().WorkflowsDirectory, name, "BadStep"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_InvalidStepType_ThrowsWithoutWriting()
        {
            var name = CreateFixtureWorkflow("BadStepTypeWf");
            var test = TestOf(new
            {
                testName = "BadType",
                testSteps = new[] { new { activityId = _assignActivityId, activityType = "DsfDotNetMultiAssignActivity", type = "NotARealType" } },
            });

            await Assert.ThrowsExceptionAsync<McpException>(() => Handle(HostConfig(), OpenPolicy, null, name, test));
            Assert.IsFalse(TestCatalog.Exists(HostConfig().WorkflowsDirectory, name, "BadType"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_LiteralPassword_Throws()
        {
            var name = CreateFixtureWorkflow("LiteralPasswordWf");
            var test = TestOf(new { testName = "HasPassword", password = "hunter2" });

            await Handle(HostConfig(), OpenPolicy, null, name, test);
        }

        // ── Tests: success path ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Valid_WritesTestFile_ReturnsCreatedTrue()
        {
            var name = CreateFixtureWorkflow();

            var result = await Handle(HostConfig(), OpenPolicy, null, name, TestOf(ValidTest()));

            Assert.AreEqual(name, result.Name);
            Assert.AreEqual("OutputIs10", result.TestName);
            Assert.IsTrue(result.Created);
            Assert.IsTrue(TestCatalog.Exists(HostConfig().WorkflowsDirectory, name, "OutputIs10"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_TestAlreadyExists_Throws()
        {
            var name = CreateFixtureWorkflow();
            await Handle(HostConfig(), OpenPolicy, null, name, TestOf(ValidTest()));

            await Assert.ThrowsExceptionAsync<McpException>(() => Handle(HostConfig(), OpenPolicy, null, name, TestOf(ValidTest())));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_WithContributePermission_Succeeds()
        {
            var name = CreateFixtureWorkflow("AllowedWf");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals(name, StringComparison.OrdinalIgnoreCase) ? WorkflowPermission.Contribute : WorkflowPermission.None,
            };

            var result = await Handle(HostConfig(), loader, Principal("Developers"), name, TestOf(ValidTest()));

            Assert.IsTrue(result.Created);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecretPlaceholderPassword_ResolvesAndEncrypts_NeverPlaintextOnDisk()
        {
            var name = CreateFixtureWorkflow("SecretPasswordWf");
            var resolver = new FakeSecretResolver().With("db-pwd", "s3cr3t-literal-value");
            var test = TestOf(new { testName = "HasSecret", password = "${db-pwd}" });

            var result = await Handle(HostConfig(), OpenPolicy, null, name, test, resolver);

            Assert.IsTrue(result.Created);
            var written = TestCatalog.Load(HostConfig().WorkflowsDirectory, name, "HasSecret")!;
            StringAssert.DoesNotMatch(written, new System.Text.RegularExpressions.Regex("s3cr3t-literal-value"));
            StringAssert.DoesNotMatch(written, new System.Text.RegularExpressions.Regex(@"\$\{db-pwd\}"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_WrittenFile_RoundTripsSchemaFields()
        {
            var name = CreateFixtureWorkflow("ShapedWf");

            await Handle(HostConfig(), OpenPolicy, null, name, TestOf(ValidTest("Shaped")));

            var json = TestCatalog.Load(HostConfig().WorkflowsDirectory, name, "Shaped")!;
            var doc = System.Text.Json.JsonDocument.Parse(json);
            Assert.AreEqual("Shaped", doc.RootElement.GetProperty("testName").GetString());
            Assert.AreEqual(1, doc.RootElement.GetProperty("testSteps").GetArrayLength());
            Assert.AreEqual(_assignActivityId, doc.RootElement.GetProperty("testSteps")[0].GetProperty("activityId").GetString());
        }
    }
}
