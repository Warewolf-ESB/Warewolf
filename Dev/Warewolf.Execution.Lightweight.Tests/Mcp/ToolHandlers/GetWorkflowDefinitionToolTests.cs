/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetWorkflowDefinitionTool: name resolution/not-found, View
 *  permission gating (reusing ListWorkflowsTool's rule), envelope shape,
 *  and the bodyEditable fidelity gate (both a real Pass-only workflow and
 *  a real workflow using a non-Pass activity type).
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetWorkflowDefinitionToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "gwd-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches ListWorkflowsToolTests' conventions) ────────

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

        // ── Helpers ────────────────────────────────────────────────────────────

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        private void WriteWorkflow(string relativePath, string name, string comment = "", string dataListXml = "<DataList />")
        {
            var fullPath = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath,
                $"<Service Name=\"{name}\" ResourceType=\"WorkflowService\">" +
                $"<Comment>{comment}</Comment>{dataListXml}<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                "<XamlDefinition></XamlDefinition></Action></Service>");
        }

        /// <summary>
        /// Walks parents of the test assembly's base directory looking for a known committed
        /// workflow's <c>.bite</c> file, mirroring
        /// <c>WorkflowExecutorEndToEndTests.FindHelloWorldBite</c>'s ancestor-walk pattern.
        /// </summary>
        static string? FindBite(params string[] relativeSegments)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (var hops = 0; dir != null && hops < 10; hops++, dir = dir.Parent)
            {
                var candidate = Path.Combine(new[] { dir.FullName }.Concat(relativeSegments).ToArray());
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                var devCandidate = Path.Combine(new[] { dir.FullName, "Dev" }.Concat(relativeSegments).ToArray());
                if (File.Exists(devCandidate))
                {
                    return devCandidate;
                }
            }
            return null;
        }

        private static GetWorkflowDefinitionResult Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name) =>
            GetWorkflowDefinitionTool.Handle(hostConfig, authPolicyLoader, user, name);

        // ── Tests: input validation / not-found / permissions ─────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_BlankName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "   ");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_UnknownName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "DoesNotExist");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_NonWorkflowResourceType_ThrowsNotFound()
        {
            var fullPath = Path.Combine(_root, "NotAWorkflow.bite");
            File.WriteAllText(fullPath, "<Service Name=\"NotAWorkflow\" ResourceType=\"Source\"></Service>");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NotAWorkflow");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_SecureConfigEffective_NoViewPermission_ThrowsPermissionDenied()
        {
            WriteWorkflow("Secret.bite", "Secret");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.None,
            };

            Handle(HostConfig(), loader, null, "Secret");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_WithViewPermission_Succeeds()
        {
            WriteWorkflow("Visible.bite", "Visible");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Visible", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.View
                        : WorkflowPermission.None,
            };

            var result = Handle(HostConfig(), loader, Principal("Developers"), "Visible");

            Assert.AreEqual("Visible", result.Name);
        }

        // ── Tests: envelope shape ──────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Envelope_ExtractsNameDescriptionInputsOutputs()
        {
            WriteWorkflow("Hello.bite", "Hello", comment: "Says hello",
                dataListXml: "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                             "<Message Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></DataList>");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Hello");

            Assert.AreEqual("Hello", result.Envelope.Name);
            Assert.AreEqual("Says hello", result.Envelope.Description);
            Assert.IsTrue(result.Envelope.Inputs.Any(v => v.Name == "Name" && v.Kind == "scalar"));
            Assert.IsTrue(result.Envelope.Outputs.Any(v => v.Name == "Message" && v.Kind == "scalar"));
        }

        /// <summary>
        /// F6: get_workflow_definition previously flattened inputs/outputs to bracket-notation
        /// strings (DataListTO's shape) — cheap to derive, but not re-submittable: an object
        /// entry's kind and a recordset's declared fields were both lost. Confirms all three kinds
        /// now round-trip, including a recordset's per-field direction.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Envelope_DistinguishesScalarObjectAndRecordsetKinds()
        {
            WriteWorkflow("Kinds.bite", "Kinds", comment: "Exercises all three kinds",
                dataListXml: "<DataList>" +
                             "<PlainScalar Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                             "<JsonObject Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" IsJson=\"True\"><![CDATA[{}]]></JsonObject>" +
                             "<Customers Description=\"\" IsEditable=\"True\">" +
                             "<Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                             "<Age Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                             "</Customers>" +
                             "</DataList>");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Kinds");

            var plainScalar = result.Envelope.Inputs.Single(v => v.Name == "PlainScalar");
            Assert.AreEqual("scalar", plainScalar.Kind);
            Assert.IsNull(plainScalar.Fields);

            var jsonObject = result.Envelope.Outputs.Single(v => v.Name == "JsonObject");
            Assert.AreEqual("object", jsonObject.Kind);

            var customersInput = result.Envelope.Inputs.Single(v => v.Name == "Customers");
            Assert.AreEqual("recordset", customersInput.Kind);
            CollectionAssert.AreEqual(new[] { "Name" }, customersInput.Fields!.ToList());

            var customersOutput = result.Envelope.Outputs.Single(v => v.Name == "Customers");
            Assert.AreEqual("recordset", customersOutput.Kind);
            CollectionAssert.AreEqual(new[] { "Age" }, customersOutput.Fields!.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NameCanBeResolvedViaNestedFolderPath()
        {
            WriteWorkflow(Path.Combine("Sub", "Nested.bite"), "Nested");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Sub/Nested");

            Assert.AreEqual("Nested", result.Name);
        }

        // ── Tests: bodyEditable fidelity gate, using real committed fixtures ──

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_RealWorkflow_AllActivitiesPassFidelity_BodyEditableTrueWithCells()
        {
            var source = FindBite("Resources - ServerTests", "Resources", "Merge Acceptance Tests", "WorkFlowWithOneObject.bite");
            if (source is null)
            {
                Assert.Inconclusive("WorkFlowWithOneObject.bite not found relative to the test assembly's base directory.");
                return;
            }

            var destination = Path.Combine(_root, "WorkFlowWithOneObject.bite");
            File.Copy(source, destination);

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "WorkFlowWithOneObject");

            Assert.IsTrue(result.BodyEditable,
                "WorkFlowWithOneObject.bite only uses 'Assign Object'/'Sequence', both fidelity Status=Pass; expected bodyEditable:true. Reason: " + result.NonEditableReason);
            Assert.IsNull(result.NonEditableReason);
            Assert.IsNotNull(result.Body);

            var bodyJson = result.Body!.Value;
            Assert.AreEqual("WorkFlowWithOneObject", bodyJson.GetProperty("resourcename").GetString());
            Assert.IsTrue(bodyJson.GetProperty("cells").GetArrayLength() > 0);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_RealWorkflow_UsesNonPassActivity_BodyEditableFalseWithReason()
        {
            var source = FindBite("Resources - Release", "Resources", "Hello World.bite");
            if (source is null)
            {
                Assert.Inconclusive("Hello World.bite not found relative to the test assembly's base directory.");
                return;
            }

            var destination = Path.Combine(_root, "Hello World.bite");
            File.Copy(source, destination);

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Hello World");

            Assert.IsFalse(result.BodyEditable,
                "Hello World.bite uses 'Assign', fidelity Status=PassBothFailedIdentically (not Pass); expected bodyEditable:false.");
            Assert.IsNotNull(result.NonEditableReason);
            Assert.IsNull(result.Body);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_MissingXamlDefinition_BodyEditableFalseWithReason()
        {
            // WriteWorkflow's synthetic fixture has an empty <XamlDefinition/> element.
            WriteWorkflow("NoXaml.bite", "NoXaml");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NoXaml");

            Assert.IsFalse(result.BodyEditable);
            Assert.IsNotNull(result.NonEditableReason);
            Assert.IsNull(result.Body);
        }
    }
}
