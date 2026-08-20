/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for CreateWorkflowTool (warewolf-lee-mcp-v3-spec.md, "Tools" §
 *  create_workflow): required-parameter/validation delegation to
 *  ValidateWorkflowTool, name-already-exists rejection, Contribute
 *  permission gating (reusing ListWorkflowsTool's generalised rule), and the
 *  success path — a written .bite file that round-trips correctly through
 *  get_workflow_definition/list_workflows and is immediately resolvable via
 *  WorkflowIndex without a process restart.
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
    public class CreateWorkflowToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "create-wf-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches ListWorkflowsToolTests'/GetWorkflowDefinitionToolTests' conventions) ──

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

        // ── Fixture builders (mirrors ValidateWorkflowToolTests' shapes so a body that passes
        // validate_workflow's checks also compiles cleanly through X6ToWorkflowConverter). ──────

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

        static readonly JsonElement EmptyEnvelope = EnvelopeOf(new { inputs = Array.Empty<object>(), outputs = Array.Empty<object>() });

        /// <summary>A minimal valid { start -> assign } graph writing a literal into [[Result]].</summary>
        static JsonElement ValidBody(string resourceName = "NewWorkflow")
        {
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[Result]]",
                ["FieldValue"] = "hello",
                ["IndexNumber"] = 1
            });
            var assign = MakeAssign("assign1", "Assign", fields);
            return BodyOf(resourceName, MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));
        }

        static JsonElement ValidEnvelope() => EnvelopeOf(new
        {
            name = "NewWorkflow",
            description = "A freshly created workflow",
            inputs = Array.Empty<object>(),
            outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
        });

        private static CreateWorkflowResult Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name,
            JsonElement envelope,
            JsonElement body) =>
            CreateWorkflowTool.Handle(hostConfig, authPolicyLoader, user, name, envelope, body);

        // ── Tests: input validation ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "   ", ValidEnvelope(), ValidBody());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_InvalidBody_NoStartNode_ThrowsWithoutWriting()
        {
            var body = BodyOf("Broken", MakeAssign("assign1", "Assign", new JArray()));

            try
            {
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Broken", EmptyEnvelope, body);
            }
            finally
            {
                Assert.IsFalse(File.Exists(Path.Combine(_root, "Broken.bite")),
                    "A failed validation must reject without writing any file.");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_NameAlreadyExists_Throws()
        {
            File.WriteAllText(Path.Combine(_root, "Existing.bite"),
                "<Service Name=\"Existing\" ResourceType=\"WorkflowService\"><DataList /><Action Name=\"InvokeWorkflow\" Type=\"Workflow\"><XamlDefinition></XamlDefinition></Action></Service>");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Existing", ValidEnvelope(), ValidBody());
        }

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

            Handle(HostConfig(), loader, Principal("Developers"), "NoPermission", ValidEnvelope(), ValidBody());
        }

        // ── Tests: success path ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ValidEnvelopeAndBody_WritesFile_ReturnsCreatedTrue()
        {
            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NewWorkflow", ValidEnvelope(), ValidBody());

            Assert.AreEqual("NewWorkflow", result.Name);
            Assert.IsTrue(result.Created);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "NewWorkflow.bite")));
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

            var result = Handle(HostConfig(), loader, Principal("Developers"), "Allowed", ValidEnvelope(), ValidBody());

            Assert.IsTrue(result.Created);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WrittenFile_ContainsExpectedServiceShape()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Shaped", ValidEnvelope(), ValidBody("Shaped"));

            var contents = File.ReadAllText(Path.Combine(_root, "Shaped.bite"));
            var doc = System.Xml.Linq.XDocument.Parse(contents);
            var service = doc.Root!;

            Assert.AreEqual("Service", service.Name.LocalName);
            Assert.AreEqual("WorkflowService", service.Attribute("ResourceType")!.Value);
            Assert.IsFalse(string.IsNullOrWhiteSpace(service.Attribute("ID")!.Value));
            Assert.AreEqual("NewWorkflow", service.Attribute("Name")!.Value); // envelope.name wins over the file's own relative-path name
            Assert.AreEqual("A freshly created workflow", service.Element("Comment")!.Value);
            Assert.IsNotNull(service.Element("DataList")!.Element("Result"));
            Assert.AreEqual("Output", service.Element("DataList")!.Element("Result")!.Attribute("ColumnIODirection")!.Value);
            Assert.IsFalse(string.IsNullOrWhiteSpace(service.Element("Action")!.Element("XamlDefinition")!.Value));
            Assert.AreEqual("Save", service.Element("VersionInfo")!.Attribute("Reason")!.Value);
            Assert.AreEqual("1", service.Element("VersionInfo")!.Attribute("VersionNumber")!.Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WrittenWorkflow_RoundTripsThroughGetWorkflowDefinition()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "RoundTrip", ValidEnvelope(), ValidBody("RoundTrip"));

            var definition = GetWorkflowDefinitionTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "RoundTrip");

            Assert.AreEqual("NewWorkflow", definition.Envelope.Name); // envelope.name, not the file-path name
            Assert.AreEqual("A freshly created workflow", definition.Envelope.Description);
            CollectionAssert.Contains(definition.Envelope.Outputs.ToList(), "Result");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NestedFolderName_CreatesSubdirectory()
        {
            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Sub/Nested",
                ValidEnvelope(), ValidBody("Nested"));

            Assert.IsTrue(result.Created);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "Sub", "Nested.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WrittenWorkflow_IsImmediatelyResolvableViaWorkflowIndex()
        {
            var workflowsDirectory = HostConfig().WorkflowsDirectory;
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "IndexedNow", ValidEnvelope(), ValidBody("IndexedNow"));

            // AddOrUpdate should make this resolvable via the in-memory index cache without a
            // fresh WarmUp/disk scan being required.
            var resolved = WorkflowIndex.Instance.Resolve(workflowsDirectory, "IndexedNow");
            Assert.IsNotNull(resolved);
            StringAssert.EndsWith(resolved!, "IndexedNow.bite");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_CreatingSameNameTwice_SecondCallThrows()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Dup", ValidEnvelope(), ValidBody("Dup"));

            Assert.ThrowsException<McpException>(() =>
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Dup", ValidEnvelope(), ValidBody("Dup")));
        }

        // ── Tests: httpEndpoints (bug fix — callers must not have to guess the invocation path) ──

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ConfigMissing_NoBypassFlag_ReturnsNoUrls()
        {
            var previous = Environment.GetEnvironmentVariable("BYPASS_SECURE_CONFIG");
            Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", null);
            try
            {
                var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NoConfigWf", ValidEnvelope(), ValidBody("NoConfigWf"));

                Assert.IsNull(result.HttpEndpoints.PublicUrl,
                    "secure.config missing and BYPASS_SECURE_CONFIG not set: /Public/* is denied with a 500 (WorkflowPolicyMatcher.ConfigMissingDeny), so publicUrl must be omitted.");
                Assert.IsNull(result.HttpEndpoints.SecureUrl,
                    "secure.config missing: /Secure/* always 401s (no secret key to validate a JWT against), so secureUrl must be omitted.");
            }
            finally
            {
                Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", previous);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ConfigMissing_BypassFlagSet_ReturnsPublicUrlOnly()
        {
            var previous = Environment.GetEnvironmentVariable("BYPASS_SECURE_CONFIG");
            Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", "true");
            try
            {
                var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "BypassWf", ValidEnvelope(), ValidBody("BypassWf"));

                Assert.AreEqual("/Public/BypassWf", result.HttpEndpoints.PublicUrl,
                    "BYPASS_SECURE_CONFIG=true with config not effective is the one case that makes /Public/* truly open-access.");
                Assert.IsNull(result.HttpEndpoints.SecureUrl,
                    "/Secure/* always 401s without secure.config, bypass or not.");
            }
            finally
            {
                Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", previous);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_PublicGroupGrantsExecute_ReturnsBothUrls()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, roles) =>
                    path.Equals("PublicWf", StringComparison.OrdinalIgnoreCase) && !roles.Any()
                        ? WorkflowPermission.Contribute | WorkflowPermission.View | WorkflowPermission.Execute
                        : WorkflowPermission.Contribute,
            };

            var result = Handle(HostConfig(), loader, Principal("Developers"), "PublicWf", ValidEnvelope(), ValidBody("PublicWf"));

            Assert.AreEqual("/Secure/PublicWf", result.HttpEndpoints.SecureUrl);
            Assert.AreEqual("/Public/PublicWf", result.HttpEndpoints.PublicUrl);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_PublicGroupNotGranted_ReturnsSecureUrlOnly()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, roles) =>
                    roles.Any() ? WorkflowPermission.Contribute : WorkflowPermission.None,
            };

            var result = Handle(HostConfig(), loader, Principal("Developers"), "SecureOnlyWf", ValidEnvelope(), ValidBody("SecureOnlyWf"));

            Assert.AreEqual("/Secure/SecureOnlyWf", result.HttpEndpoints.SecureUrl);
            Assert.IsNull(result.HttpEndpoints.PublicUrl,
                "The Public group has no View+Execute grant for this workflow, so publicUrl must be omitted.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NestedFolderName_HttpEndpoints_UseForwardSlashPath()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, roles) =>
                    path.Equals("Sub/Deep", StringComparison.OrdinalIgnoreCase) && !roles.Any()
                        ? WorkflowPermission.Contribute | WorkflowPermission.View | WorkflowPermission.Execute
                        : WorkflowPermission.Contribute,
            };

            var result = Handle(HostConfig(), loader, Principal("Developers"), "Sub/Deep",
                ValidEnvelope(), ValidBody("Deep"));

            Assert.AreEqual("/Public/Sub/Deep", result.HttpEndpoints.PublicUrl);
            Assert.AreEqual("/Secure/Sub/Deep", result.HttpEndpoints.SecureUrl);
        }
    }
}
