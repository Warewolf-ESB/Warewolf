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

        private void WriteWorkflow(string relativePath, string name, string comment = "", string dataListXml = "<DataList />", Guid? id = null)
        {
            var fullPath = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            var idAttribute = id is null ? string.Empty : $" ID=\"{id}\"";
            File.WriteAllText(fullPath,
                $"<Service{idAttribute} Name=\"{name}\" ResourceType=\"WorkflowService\">" +
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

        /// <summary>
        /// Studio's working variables (ColumnIODirection="None") were dropped from the envelope, so a
        /// workflow read here and re-submitted to edit_workflow failed validation on its own variables.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Envelope_WorkingScalar_GoesToVariablesOnly()
        {
            WriteWorkflow("Working.bite", "Working",
                dataListXml: "<DataList><Scratch Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" /></DataList>");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Working");

            var scratch = result.Envelope.Variables.Single(v => v.Name == "Scratch");
            Assert.AreEqual("scalar", scratch.Kind);
            Assert.IsFalse(result.Envelope.Inputs.Any(v => v.Name == "Scratch"));
            Assert.IsFalse(result.Envelope.Outputs.Any(v => v.Name == "Scratch"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Envelope_RecordsetWithWorkingField_IsSplitAcrossOutputsAndVariables()
        {
            WriteWorkflow("Mixed.bite", "Mixed",
                dataListXml: "<DataList><Rows Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\">" +
                             "<Total Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" />" +
                             "<Scratch Description=\"\" IsEditable=\"True\" ColumnIODirection=\"None\" />" +
                             "</Rows></DataList>");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Mixed");

            CollectionAssert.AreEqual(new[] { "Total" }, result.Envelope.Outputs.Single(v => v.Name == "Rows").Fields!.ToList());
            CollectionAssert.AreEqual(new[] { "Scratch" }, result.Envelope.Variables.Single(v => v.Name == "Rows").Fields!.ToList());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Envelope_ElementWithoutDirection_IsStillExcluded()
        {
            WriteWorkflow("NoDirection.bite", "NoDirection",
                dataListXml: "<DataList><Bare Description=\"\" IsEditable=\"True\" /></DataList>");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NoDirection");

            Assert.AreEqual(0, result.Envelope.Inputs.Count + result.Envelope.Outputs.Count + result.Envelope.Variables.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Envelope_WorkingVariables_RoundTripThroughTheEnvelopeWriter()
        {
            // The DataList create_workflow/edit_workflow write from an envelope reads back as that envelope.
            var envelope = new
            {
                inputs = new[] { new { kind = "scalar", name = "phrase", fields = (string[]?)null } },
                outputs = new[] { new { kind = "recordset", name = "rows", fields = (string[]?)new[] { "total" } } },
                variables = new[]
                {
                    new { kind = "object", name = "ResponseBody", fields = (string[]?)null },
                    new { kind = "recordset", name = "rows", fields = (string[]?)new[] { "scratch" } },
                }
            };
            var dataList = Warewolf.Execution.Lightweight.Mcp.EnvelopeBiteWriter.BuildDataListElement(JsonSerializer.SerializeToElement(envelope));
            WriteWorkflow("RoundTrip.bite", "RoundTrip", dataListXml: dataList.ToString());

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "RoundTrip");

            static string Shape(IEnumerable<EnvelopeVariableDto> variables) =>
                string.Join(";", variables.Select(v => $"{v.Kind}:{v.Name}:{string.Join(",", v.Fields ?? Array.Empty<string>())}").OrderBy(s => s));
            Assert.AreEqual("scalar:phrase:", Shape(result.Envelope.Inputs));
            Assert.AreEqual("recordset:rows:total", Shape(result.Envelope.Outputs));
            Assert.AreEqual("object:ResponseBody:;recordset:rows:scratch", Shape(result.Envelope.Variables));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NameCanBeResolvedViaNestedFolderPath()
        {
            WriteWorkflow(Path.Combine("Sub", "Nested.bite"), "Nested");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Sub/Nested");

            Assert.AreEqual("Nested", result.Name);
        }

        // ── Tests: lookup by ResourceID (the Web Studio chatbot's SelectedResourceIds) ──

        private static GetWorkflowDefinitionResult HandleById(
            HostEnvironmentConfig hostConfig, IWorkflowAuthPolicyLoader authPolicyLoader, ClaimsPrincipal? user, Guid id, string? format = null) =>
            GetWorkflowDefinitionTool.Handle(hostConfig, authPolicyLoader, user, id: id, format: format);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ById_ResolvesAWorkflowInANestedFolder()
        {
            var id = Guid.NewGuid();
            WriteWorkflow(Path.Combine("Sub", "Nested.bite"), "Nested", comment: "By id", id: id);

            var result = HandleById(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, id);

            Assert.AreEqual("Nested", result.Name);
            Assert.AreEqual("By id", result.Envelope.Description);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ById_FindsAWorkflowWrittenAfterTheIndexWasBuilt_AndNotOneDeletedSince()
        {
            var early = Guid.NewGuid();
            WriteWorkflow("Early.bite", "Early", id: early);
            var noView = new StubAuthPolicyLoader { IsConfigEffective = false };
            HandleById(HostConfig(), noView, null, early); // builds the cached index for this directory

            var late = Guid.NewGuid();
            WriteWorkflow("Late.bite", "Late", id: late);
            File.Delete(Path.Combine(_root, "Early.bite"));

            Assert.AreEqual("Late", HandleById(HostConfig(), noView, null, late).Name);
            var stale = Assert.ThrowsException<ModelContextProtocol.McpException>(() => HandleById(HostConfig(), noView, null, early));
            StringAssert.Contains(stale.Message, early.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ById_UnknownId_OrANonWorkflowResource_IsNotFound()
        {
            var sourceId = Guid.NewGuid();
            File.WriteAllText(Path.Combine(_root, "ASource.bite"), $"<Source ID=\"{sourceId}\" Name=\"ASource\" ResourceType=\"DbSource\"></Source>");
            var loader = new StubAuthPolicyLoader { IsConfigEffective = false };

            Assert.ThrowsException<ModelContextProtocol.McpException>(() => HandleById(HostConfig(), loader, null, Guid.NewGuid()));
            Assert.ThrowsException<ModelContextProtocol.McpException>(() => HandleById(HostConfig(), loader, null, sourceId));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ById_ChecksViewPermissionOnTheWorkflowsPath()
        {
            var secret = Guid.NewGuid();
            var visible = Guid.NewGuid();
            WriteWorkflow(Path.Combine("Ops", "Secret.bite"), "Secret", id: secret);
            WriteWorkflow(Path.Combine("Ops", "Visible.bite"), "Visible", id: visible);
            var checkedPaths = new List<string>();
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                {
                    checkedPaths.Add(path);
                    return path.Equals("Ops/Visible", StringComparison.OrdinalIgnoreCase) ? WorkflowPermission.View : WorkflowPermission.None;
                },
            };

            Assert.AreEqual("Visible", HandleById(HostConfig(), loader, Principal("Developers"), visible).Name);
            var denied = Assert.ThrowsException<ModelContextProtocol.McpException>(() => HandleById(HostConfig(), loader, Principal("Developers"), secret));
            StringAssert.Contains(denied.Message, "permission");
            CollectionAssert.Contains(checkedPaths, "Ops/Secret");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_IdTakesPrecedenceOverName()
        {
            var id = Guid.NewGuid();
            WriteWorkflow("ByName.bite", "ByName");
            WriteWorkflow("ById.bite", "ById", id: id);

            var result = GetWorkflowDefinitionTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "ByName", id);

            Assert.AreEqual("ById", result.Name);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EmptyIdAndBlankName_Throws()
        {
            var ex = Assert.ThrowsException<ModelContextProtocol.McpException>(() =>
                GetWorkflowDefinitionTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, " ", Guid.Empty));

            Assert.AreEqual("`name` or `id` is required.", ex.Message);
        }

        // ── Tests: format ──────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_FormatXaml_AlsoReturnsTheFullResourceXml_DefaultDoesNot()
        {
            var id = Guid.NewGuid();
            WriteWorkflow("Hello.bite", "Hello", comment: "Says hello", id: id);
            var loader = new StubAuthPolicyLoader { IsConfigEffective = false };

            var xaml = HandleById(HostConfig(), loader, null, id, format: "XAML");
            var x6 = HandleById(HostConfig(), loader, null, id, format: "x6");
            var unspecified = HandleById(HostConfig(), loader, null, id);

            Assert.AreEqual(File.ReadAllText(Path.Combine(_root, "Hello.bite")), xaml.Xaml);
            Assert.AreEqual("Says hello", xaml.Envelope.Description, "the envelope is still returned");
            Assert.IsNull(x6.Xaml);
            Assert.IsNull(unspecified.Xaml);
            Assert.IsFalse(JsonSerializer.Serialize(unspecified).Contains("\"xaml\""), "omitted from the response, not null");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_UnknownFormat_Throws()
        {
            WriteWorkflow("Hello.bite", "Hello");

            var ex = Assert.ThrowsException<ModelContextProtocol.McpException>(() =>
                GetWorkflowDefinitionTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Hello", format: "yaml"));

            StringAssert.Contains(ex.Message, "`format`");
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
            // EngineResources is the test project's copy of the engine's Resources folder.
            var source = FindBite("EngineResources", "hangfiredemo", "Manual Resumption Tool Example.bite");
            if (source is null)
            {
                Assert.Inconclusive("Manual Resumption Tool Example.bite not found relative to the test assembly's base directory.");
                return;
            }

            var destination = Path.Combine(_root, "Manual Resumption Tool Example.bite");
            File.Copy(source, destination);

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Manual Resumption Tool Example");

            Assert.IsFalse(result.BodyEditable,
                "Manual Resumption Tool Example.bite uses 'Manual Resumption', fidelity Status=PassBothFailedIdentically (not Pass); expected bodyEditable:false.");
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
