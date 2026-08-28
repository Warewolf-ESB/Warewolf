/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for DeployWorkflowTool (deploy_workflow): required-parameter
 *  checks, structural/compile validation of a caller-supplied .bite document,
 *  the overwrite gate (existing workflow + overwrite:false rejected,
 *  overwrite:true replaces in place), Contribute permission gating (reusing
 *  ListWorkflowsTool's generalised rule, matching create_workflow/edit_workflow),
 *  and the success path — the supplied biteContent is written to disk
 *  verbatim, round-trips through get_workflow_definition, and is immediately
 *  resolvable via WorkflowIndex without a process restart.
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
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class DeployWorkflowToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "deploy-wf-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches the sibling *ToolTests' conventions) ────────

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

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        private HostEnvironmentConfig HostConfig()
        {
            Environment.SetEnvironmentVariable("WorkflowsDirectory", _root);
            try
            {
                return HostEnvironmentConfig.Load();
            }
            finally
            {
                Environment.SetEnvironmentVariable("WorkflowsDirectory", null);
            }
        }

        // ── Valid-biteContent fixture (built via CreateWorkflowTool.Handle into a throwaway
        // scratch directory, then read back off disk — proves the fixture is genuinely a
        // valid, compilable .bite document rather than a hand-rolled approximation). ────────

        static Cell MakeStartNode(string id = "start") =>
            new() { id = id, shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };

        static Cell MakeAssign(string id, string displayName, JArray fields) =>
            new()
            {
                id = id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = displayName,
                    ["fields"] = fields,
                }
            };

        static Cell MakeEdge(string id, string sourceId, string targetId) =>
            new() { id = id, shape = "edge", data = new Dictionary<string, object>(), Source = new Connector(sourceId), Target = new Connector(targetId) };

        static JsonElement BodyOf(string resourceName, params Cell[] cells)
        {
            var graph = new X6WorkflowSaveModel { ResourceName = resourceName, Cells = new List<Cell>(cells) };
            return JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
        }

        static JsonElement EnvelopeOf(object envelopeObj) => System.Text.Json.JsonSerializer.SerializeToElement(envelopeObj);

        /// <summary>
        /// Builds a fresh, valid, compilable .bite document's full XML text — independent of
        /// this test class's own <see cref="_root"/> — by writing it via
        /// <see cref="CreateWorkflowTool.Handle"/> into a throwaway scratch directory and
        /// reading the result back off disk.
        /// </summary>
        static string BuildValidBiteContent(string resourceName)
        {
            var scratchRoot = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), "deploy-wf-fixture-" + Guid.NewGuid().ToString("N"))).FullName;
            try
            {
                Environment.SetEnvironmentVariable("WorkflowsDirectory", scratchRoot);
                HostEnvironmentConfig scratchConfig;
                try
                {
                    scratchConfig = HostEnvironmentConfig.Load();
                }
                finally
                {
                    Environment.SetEnvironmentVariable("WorkflowsDirectory", null);
                }

                var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
                var body = BodyOf(resourceName, MakeStartNode(), MakeAssign("assign1", "Assign", fields), MakeEdge("e1", "start", "assign1"));
                var envelope = EnvelopeOf(new
                {
                    name = resourceName,
                    description = "A deployable workflow",
                    inputs = Array.Empty<object>(),
                    outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
                });

                CreateWorkflowTool.Handle(scratchConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null, resourceName, envelope, body);

                return File.ReadAllText(Path.Combine(scratchRoot, resourceName + ".bite"));
            }
            finally
            {
                Directory.Delete(scratchRoot, recursive: true);
            }
        }

        private static DeployWorkflowResult Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name,
            string biteContent,
            bool overwrite = false) =>
            DeployWorkflowTool.Handle(hostConfig, authPolicyLoader, user, name, biteContent, overwrite);

        // ── Tests: input validation ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "   ", BuildValidBiteContent("X"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankBiteContent_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NewWorkflow", "   ");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_MalformedXml_ThrowsWithoutWriting()
        {
            try
            {
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Malformed",
                    "<Service ResourceType=\"WorkflowService\">");
            }
            finally
            {
                Assert.IsFalse(File.Exists(Path.Combine(_root, "Malformed.bite")));
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_WrongResourceType_ThrowsWithoutWriting()
        {
            try
            {
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "WrongType",
                    "<Service ResourceType=\"Source\"><DataList /></Service>");
            }
            finally
            {
                Assert.IsFalse(File.Exists(Path.Combine(_root, "WrongType.bite")));
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_NoXamlDefinition_ThrowsWithoutWriting()
        {
            try
            {
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NoXaml",
                    "<Service ResourceType=\"WorkflowService\"><DataList /></Service>");
            }
            finally
            {
                Assert.IsFalse(File.Exists(Path.Combine(_root, "NoXaml.bite")));
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_UncompilableXaml_ThrowsWithoutWriting()
        {
            try
            {
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "BadXaml",
                    "<Service ResourceType=\"WorkflowService\"><DataList /><Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                    "<XamlDefinition>not valid xaml</XamlDefinition></Action></Service>");
            }
            finally
            {
                Assert.IsFalse(File.Exists(Path.Combine(_root, "BadXaml.bite")));
            }
        }

        // ── Tests: overwrite gate ───────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_ExistingWorkflow_OverwriteFalse_ThrowsWithoutModifyingFile()
        {
            var path = Path.Combine(_root, "Existing.bite");
            File.WriteAllText(path, "ORIGINAL CONTENT");

            try
            {
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Existing",
                    BuildValidBiteContent("Existing"), overwrite: false);
            }
            finally
            {
                Assert.AreEqual("ORIGINAL CONTENT", File.ReadAllText(path),
                    "A rejected overwrite:false call must not modify the existing file.");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ExistingWorkflow_OverwriteTrue_ReplacesFile_ReturnsOverwrittenTrue()
        {
            var path = Path.Combine(_root, "Existing.bite");
            File.WriteAllText(path, "ORIGINAL CONTENT");
            var newContent = BuildValidBiteContent("Existing");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Existing",
                newContent, overwrite: true);

            Assert.IsTrue(result.Deployed);
            Assert.IsTrue(result.Overwritten);
            Assert.AreEqual(newContent, File.ReadAllText(path));
        }

        // ── Tests: permission gating ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_SecureConfigEffective_NoContributePermission_ThrowsPermissionDenied()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.View,
            };

            Handle(HostConfig(), loader, Principal("Developers"), "NoPermission", BuildValidBiteContent("NoPermission"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_WithContributePermission_Succeeds()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Allowed", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.Contribute
                        : WorkflowPermission.None,
            };

            var result = Handle(HostConfig(), loader, Principal("Developers"), "Allowed", BuildValidBiteContent("Allowed"));

            Assert.IsTrue(result.Deployed);
            Assert.IsFalse(result.Overwritten);
        }

        // ── Tests: success path ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NewWorkflow_WritesFile_ReturnsDeployedTrueOverwrittenFalse()
        {
            var content = BuildValidBiteContent("NewWorkflow");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NewWorkflow", content);

            Assert.AreEqual("NewWorkflow", result.Name);
            Assert.IsTrue(result.Deployed);
            Assert.IsFalse(result.Overwritten);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "NewWorkflow.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WrittenFile_ContentMatchesSuppliedBiteContentVerbatim()
        {
            var content = BuildValidBiteContent("Verbatim");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Verbatim", content);

            Assert.AreEqual(content, File.ReadAllText(Path.Combine(_root, "Verbatim.bite")),
                "deploy_workflow must write the supplied biteContent as-is, without re-composing it.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NestedFolderName_CreatesSubdirectory()
        {
            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Sub/Nested",
                BuildValidBiteContent("Nested"));

            Assert.IsTrue(result.Deployed);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "Sub", "Nested.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WrittenWorkflow_IsImmediatelyResolvableViaWorkflowIndex()
        {
            var workflowsDirectory = HostConfig().WorkflowsDirectory;
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "IndexedNow", BuildValidBiteContent("IndexedNow"));

            // AddOrUpdate should make this resolvable via the in-memory index cache without a
            // fresh WarmUp/disk scan being required.
            var resolved = WorkflowIndex.Instance.Resolve(workflowsDirectory, "IndexedNow");
            Assert.IsNotNull(resolved);
            StringAssert.EndsWith(resolved!, "IndexedNow.bite");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WrittenWorkflow_RoundTripsThroughGetWorkflowDefinition()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "RoundTrip", BuildValidBiteContent("RoundTrip"));

            var definition = GetWorkflowDefinitionTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "RoundTrip");

            Assert.AreEqual("RoundTrip", definition.Envelope.Name);
            Assert.AreEqual("A deployable workflow", definition.Envelope.Description);
            Assert.IsTrue(definition.Envelope.Outputs.Any(v => v.Name == "Result"));
        }
    }
}
