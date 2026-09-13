/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for EditTestTool: not-found rejection (inverse of create_test's already-exists
 *  check), Contribute-permission gating, and the success path — overwriting an existing
 *  .test.json in place without renaming.
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
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class EditTestToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "edit-test-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
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
                throw new McpException($"Secret reference '${{{name}}}' could not be resolved: no secret named '{name}' is staged.");
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };
        static readonly NoOpSecretResolver NoSecrets = new();

        static JsonElement TestOf(object obj) => System.Text.Json.JsonSerializer.SerializeToElement(obj);

        readonly string _assignActivityId = Guid.NewGuid().ToString();

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

        object ValidTest(string testName = "OutputIs10") => new
        {
            testName,
            outputs = new[] { new { variable = "[[Result]]", value = "hello", assertOp = "=" } },
        };

        async Task<string> CreateExistingTest(string workflowName, string testName = "OutputIs10")
        {
            await CreateTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, workflowName, TestOf(ValidTest(testName)));
            return testName;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_BlankName_Throws()
        {
            await EditTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, "   ", TestOf(ValidTest()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_WorkflowNotFound_Throws()
        {
            await EditTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, "NoSuchWorkflow", TestOf(ValidTest()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_TestNotFound_Throws()
        {
            var name = CreateFixtureWorkflow();

            await Assert.ThrowsExceptionAsync<McpException>(
                () => EditTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, name, TestOf(ValidTest("NeverCreated"))));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_NoContributePermission_Throws()
        {
            var name = CreateFixtureWorkflow();
            await CreateExistingTest(name);
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.View };

            await Assert.ThrowsExceptionAsync<McpException>(
                () => EditTestTool.Handle(HostConfig(), loader, NoSecrets, Principal("Developers"), name, TestOf(ValidTest())));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Valid_OverwritesExistingTest_ReturnsUpdatedTrue()
        {
            var name = CreateFixtureWorkflow();
            await CreateExistingTest(name);

            var updated = TestOf(new { testName = "OutputIs10", outputs = new[] { new { variable = "[[Result]]", value = "goodbye", assertOp = "=" } } });
            var result = await EditTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, name, updated);

            Assert.AreEqual(name, result.Name);
            Assert.AreEqual("OutputIs10", result.TestName);
            Assert.IsTrue(result.Updated);

            var written = TestCatalog.Load(HostConfig().WorkflowsDirectory, name, "OutputIs10")!;
            StringAssert.Contains(written, "goodbye");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Valid_DoesNotCreateASecondFile()
        {
            var name = CreateFixtureWorkflow();
            await CreateExistingTest(name);

            await EditTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, name, TestOf(ValidTest()));

            var testsDir = TestCatalog.TestsDirectoryFor(HostConfig().WorkflowsDirectory, name);
            Assert.AreEqual(1, Directory.GetFiles(testsDir, "*.test.json").Length);
        }
    }
}
