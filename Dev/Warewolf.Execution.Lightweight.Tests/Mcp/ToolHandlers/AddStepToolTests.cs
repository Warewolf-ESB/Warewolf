/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for AddStepTool: name resolution/not-found, Contribute permission
 *  gating, the bodyEditable precondition (reusing GetWorkflowDefinitionTool's
 *  gate), step normalization/validation, the end-to-end "simple append"
 *  success path (seeded via CreateWorkflowTool with a Pass-fidelity "Assign
 *  Object" workflow rather than a legacy fixture — see the class remarks in
 *  AddStepTool for why "Assign"/"Decision" fixtures cannot be used here), and
 *  the internal FindChainTail/BuildConnectingEdge/NormalizeStepData seams
 *  exposed specifically so Decision/Switch branch-validation rules can be
 *  tested independent of the current Decision-fidelity gate limitation.
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
    public class AddStepToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "add-step-tests-" + Guid.NewGuid().ToString("N"))).FullName;

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

        // ── Seed-workflow fixture builders ─────────────────────────────────────
        //
        // "Assign"/"Decision" are not Pass-fidelity (fidelity-allowlist.json), so any
        // workflow using them is bodyEditable:false and cannot seed an add_step test.
        // "Assign Object"/"Sequence"/"Switch" are all Pass-fidelity — we build seed
        // workflows via CreateWorkflowTool.Handle (a structurally different, proven
        // code path from parsing legacy XAML fixtures) using only those types.

        static Cell MakeStartNode(string id = "start") =>
            new() { id = id, shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };

        static Cell MakeAssignObject(string id, string displayName, JArray fields) =>
            new()
            {
                id = id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignobjectactivity",
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
        /// Writes a fresh, Pass-fidelity, bodyEditable:true workflow named <paramref name="name"/>
        /// containing a single { start -> Assign Object } chain, via CreateWorkflowTool.Handle
        /// (not a legacy fixture — see class remarks).
        /// </summary>
        private void SeedSimpleWorkflow(string name = "Seed")
        {
            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
            var body = BodyOf(name, MakeStartNode(), MakeAssignObject("assign1", "Assign Object", fields), MakeEdge("e1", "start", "assign1"));
            var envelope = EnvelopeOf(new
            {
                name,
                description = "seed workflow",
                inputs = Array.Empty<object>(),
                outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
            });

            CreateWorkflowTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), name, envelope, body);
        }

        static JsonElement StepOf(string shape, string? label = null, object? data = null) =>
            System.Text.Json.JsonSerializer.SerializeToElement(new { shape, label, data });

        /// <summary>
        /// Builds a valid "Assign Object" step payload with <c>data.type</c> supplied explicitly
        /// (as a real X6 payload from the Angular chatbot would — see the spec's own literal
        /// example, <c>{ id, shape, label, data: { type, displayname, ...fields } }</c>).
        /// <c>shape</c> here is a caller-facing cosmetic value only — <see cref="ToolCatalog.Resolve"/>
        /// matches against <c>data.type</c> aliases, not the Studio display name, so leaving
        /// <c>data.type</c> to be *derived* from a display-name-shaped <c>shape</c> (e.g. "Assign
        /// Object") would never resolve; that derivation path is instead covered directly by the
        /// <c>NormalizeStepData_DerivesTypeFromShape_WhenTypeOmitted</c> unit test below, using a
        /// <c>data.type</c>-alias-shaped <c>shape</c> value.
        /// </summary>
        static JsonElement AssignObjectStepOf(string? label = null, JArray? fields = null)
        {
            var step = new JObject
            {
                ["shape"] = "Assign Object",
                ["label"] = label,
                ["data"] = new JObject
                {
                    ["type"] = "dsfdotnetmultiassignobjectactivity",
                    ["fields"] = fields ?? new JArray(),
                },
            };
            return JsonDocument.Parse(step.ToString(Formatting.None)).RootElement;
        }

        private static AddStepResult Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name,
            JsonElement step,
            string? afterStepId = null,
            string? branch = null) =>
            AddStepTool.Handle(hostConfig, authPolicyLoader, user, name, step, afterStepId, branch);

        // ── Tests: input validation / not-found / permissions ─────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "   ", AssignObjectStepOf());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_UnknownName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "DoesNotExist", AssignObjectStepOf());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_StepNotAnObject_Throws()
        {
            SeedSimpleWorkflow("BadStep");
            var step = System.Text.Json.JsonSerializer.SerializeToElement("not-an-object");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "BadStep", step);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_StepMissingShape_Throws()
        {
            SeedSimpleWorkflow("MissingShape");
            var step = System.Text.Json.JsonSerializer.SerializeToElement(new { label = "No shape here" });

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "MissingShape", step);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_StepUnknownShape_Throws()
        {
            SeedSimpleWorkflow("UnknownShape");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "UnknownShape", StepOf("NotARealToolboxShape"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_SecureConfigEffective_NoContributePermission_ThrowsPermissionDenied()
        {
            SeedSimpleWorkflow("NoPermission");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.View,
            };

            Handle(HostConfig(), loader, Principal("Developers"), "NoPermission", AssignObjectStepOf());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_RealWorkflow_UsesNonPassActivity_NotEditable_Throws()
        {
            // "Hello World.bite" uses "Assign" — fidelity Status=PassBothFailedIdentically
            // (not Pass) — so bodyEditable is false, the exact precondition add_step shares
            // with get_workflow_definition.
            var source = FindBite("Resources - Release", "Resources", "Hello World.bite");
            if (source is null)
            {
                Assert.Inconclusive("Hello World.bite not found relative to the test assembly's base directory.");
                return;
            }

            File.Copy(source, Path.Combine(_root, "Hello World.bite"));

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Hello World", AssignObjectStepOf());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_AfterStepIdNotFound_Throws()
        {
            SeedSimpleWorkflow("AfterStepIdMissing");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "AfterStepIdMissing",
                AssignObjectStepOf(), afterStepId: "does-not-exist");
        }

        // ── Tests: success path (real end-to-end, seeded via CreateWorkflowTool) ──

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SimpleAppend_AttachesAfterChainTail_WhenAfterStepIdOmitted()
        {
            SeedSimpleWorkflow("SimpleAppend");

            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "world", ["IndexNumber"] = 1 });
            var step = AssignObjectStepOf("Second Assign", fields);

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "SimpleAppend", step);

            Assert.AreEqual("SimpleAppend", result.Name);
            Assert.IsTrue(result.Updated);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.StepId));

            var (bodyEditable, body, reason) = GetWorkflowDefinitionTool.BuildBody(Path.Combine(_root, "SimpleAppend.bite"), "SimpleAppend");
            Assert.IsTrue(bodyEditable, "the newly appended 'Assign Object' node is still Pass-fidelity; expected bodyEditable:true. Reason: " + reason);
            Assert.IsNotNull(body);

            var graph = JsonConvert.DeserializeObject<X6WorkflowSaveModel>(body!.Value.GetRawText())!;
            Assert.AreEqual(3, graph.Cells.Count(c => !string.Equals(c.shape, "edge", StringComparison.OrdinalIgnoreCase)),
                "expected start + original Assign Object + newly appended Assign Object = 3 nodes.");
            Assert.AreEqual(2, graph.Cells.Count(c => string.Equals(c.shape, "edge", StringComparison.OrdinalIgnoreCase)),
                "expected the original start->assign1 edge plus the new connecting edge = 2 edges.");

            var newNode = graph.Cells.First(c => string.Equals(c.id, result.StepId, StringComparison.Ordinal));
            Assert.AreEqual("Second Assign", newNode.data["displayname"]?.ToString());

            var newEdge = graph.Cells.First(e =>
                string.Equals(e.shape, "edge", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.Target?.Id, result.StepId, StringComparison.Ordinal));
            Assert.IsNotNull(newEdge.Source);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_AfterStepId_AttachesAtSpecifiedNode()
        {
            SeedSimpleWorkflow("AfterStepId");

            // Discover assign1's real generated id via a get_workflow_definition-shaped read.
            // Note: node.data.type here is NOT the clean lowercase alias for Assign Object nodes —
            // WorkflowToX6Converter's default-fallback (Constants.TYPE not set by
            // DsfDotNetMultiAssignObjectActivity.ToX6Json itself) stamps the raw assembly-qualified
            // Type instead (confirmed via a diagnostic dump: "Unlimited.Applications...
            // DsfDotNetMultiAssignObjectActivity, Dev2.Activities, Version=..."), unlike every other
            // activity's ToX6Json, which sets a clean Constants.*.ToLower() alias explicitly. This
            // pre-existing quirk doesn't affect add_step itself (it never equality-matches data.type
            // on existing nodes), so match on the clean `displayname` field instead here.
            var (_, body, _) = GetWorkflowDefinitionTool.BuildBody(Path.Combine(_root, "AfterStepId.bite"), "AfterStepId");
            var graph = JsonConvert.DeserializeObject<X6WorkflowSaveModel>(body!.Value.GetRawText())!;
            var assignNodeId = graph.Cells.First(c => c.data.TryGetValue("displayname", out var d) &&
                string.Equals(d?.ToString(), "Assign Object", StringComparison.OrdinalIgnoreCase)).id;

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "AfterStepId",
                AssignObjectStepOf("Appended"), afterStepId: assignNodeId);

            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_TailAlreadyHasOutgoingEdge_Throws()
        {
            SeedSimpleWorkflow("AlreadyWired");

            var (_, body, _) = GetWorkflowDefinitionTool.BuildBody(Path.Combine(_root, "AlreadyWired.bite"), "AlreadyWired");
            var graph = JsonConvert.DeserializeObject<X6WorkflowSaveModel>(body!.Value.GetRawText())!;
            var startNodeId = graph.Cells.First(c => c.data.TryGetValue("type", out var t) &&
                string.Equals(t?.ToString(), Constants.START, StringComparison.OrdinalIgnoreCase)).id;

            // start already has an outgoing edge to assign1 — attaching directly at `start`
            // (bypassing FindChainTail's walk via an explicit afterStepId) must be rejected.
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "AlreadyWired",
                AssignObjectStepOf(), afterStepId: startNodeId);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_PreservesServiceIdDisplayNameDataListAcrossSave()
        {
            SeedSimpleWorkflow("Preserve");
            var beforeXml = System.Xml.Linq.XElement.Load(Path.Combine(_root, "Preserve.bite"));
            var serviceId = beforeXml.Attribute("ID")!.Value;
            var versionBefore = int.Parse(beforeXml.Element("VersionInfo")!.Attribute("VersionNumber")!.Value);

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "Preserve", AssignObjectStepOf());

            var afterXml = System.Xml.Linq.XElement.Load(Path.Combine(_root, "Preserve.bite"));
            Assert.AreEqual(serviceId, afterXml.Attribute("ID")!.Value, "Service ID must be preserved across an add_step save.");
            Assert.AreEqual(versionBefore + 1, int.Parse(afterXml.Element("VersionInfo")!.Attribute("VersionNumber")!.Value),
                "VersionNumber must increment by exactly 1.");
            Assert.IsNotNull(afterXml.Element("DataList")!.Element("Result"), "the existing <DataList> (declared variables) must be preserved verbatim.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_WrittenWorkflow_IsImmediatelyResolvableViaWorkflowIndex()
        {
            SeedSimpleWorkflow("IndexRefresh");
            var workflowsDirectory = HostConfig().WorkflowsDirectory;

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, Principal("Developers"), "IndexRefresh", AssignObjectStepOf());

            var resolved = WorkflowIndex.Instance.Resolve(workflowsDirectory, "IndexRefresh");
            Assert.IsNotNull(resolved);
            StringAssert.EndsWith(resolved!, "IndexRefresh.bite");
        }

        // ── Tests: FindChainTail (mirrors graph.component.ts's findChainTail) ──

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindChainTail_LinearChain_ReturnsLastNode()
        {
            var a = MakeStartNode("a");
            var b = MakeAssignObject("b", "B", new JArray());
            var c = MakeAssignObject("c", "C", new JArray());
            var nodes = new List<Cell> { a, b, c };
            var edges = new List<Cell> { MakeEdge("e1", "a", "b"), MakeEdge("e2", "b", "c") };

            var tail = AddStepTool.FindChainTail(a, nodes, edges);

            Assert.AreEqual("c", tail.id);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindChainTail_ZeroOutgoingEdges_ReturnsStartItself()
        {
            var a = MakeStartNode("a");
            var nodes = new List<Cell> { a };
            var edges = new List<Cell>();

            var tail = AddStepTool.FindChainTail(a, nodes, edges);

            Assert.AreEqual("a", tail.id);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindChainTail_MultipleOutgoingEdges_StopsAtBranchNode()
        {
            var a = MakeStartNode("a");
            var b = MakeAssignObject("b", "B", new JArray()); // pretend Decision/Switch node with 2 outgoing edges
            var c = MakeAssignObject("c", "C", new JArray());
            var d = MakeAssignObject("d", "D", new JArray());
            var nodes = new List<Cell> { a, b, c, d };
            var edges = new List<Cell> { MakeEdge("e1", "a", "b"), MakeEdge("e2", "b", "c"), MakeEdge("e3", "b", "d") };

            var tail = AddStepTool.FindChainTail(a, nodes, edges);

            Assert.AreEqual("b", tail.id, "a node with >=2 outgoing edges must halt the walk, per findChainTail's rule.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindChainTail_TargetNodeMissing_ReturnsCurrent()
        {
            var a = MakeStartNode("a");
            var nodes = new List<Cell> { a }; // "b" intentionally absent from nodes
            var edges = new List<Cell> { MakeEdge("e1", "a", "b") };

            var tail = AddStepTool.FindChainTail(a, nodes, edges);

            Assert.AreEqual("a", tail.id);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FindChainTail_CycleDetected_ReturnsCurrentWithoutInfiniteLoop()
        {
            var a = MakeStartNode("a");
            var b = MakeAssignObject("b", "B", new JArray());
            var nodes = new List<Cell> { a, b };
            var edges = new List<Cell> { MakeEdge("e1", "a", "b"), MakeEdge("e2", "b", "a") };

            var tail = AddStepTool.FindChainTail(a, nodes, edges);

            Assert.AreEqual("a", tail.id, "revisiting 'a' must halt the walk rather than looping forever.");
        }

        // ── Tests: NormalizeStepData (get_tool_schema leniency rules) ──────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NormalizeStepData_DerivesTypeFromShape_WhenTypeOmitted()
        {
            var data = new JObject();

            AddStepTool.NormalizeStepData("dsfdotnetmultiassignobjectactivity", data, label: null);

            Assert.AreEqual("dsfdotnetmultiassignobjectactivity", (string?)data["type"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NormalizeStepData_DerivesDisplayNameFromLabel_WhenDisplayNameOmitted()
        {
            var data = new JObject();

            AddStepTool.NormalizeStepData("Assign Object", data, label: "My Custom Label");

            Assert.AreEqual("My Custom Label", (string?)data["displayname"]);
            Assert.AreEqual("My Custom Label", (string?)data["displayName"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NormalizeStepData_DerivesDisplayNameFromCatalogEntry_WhenLabelAlsoOmitted()
        {
            // data.type already resolved (as it would be after the shape-derivation step, or
            // when the caller supplies data.type directly) — displayname then derives from the
            // matching catalog entry's Name via ToolCatalog.Resolve(dataType).
            var data = new JObject { ["type"] = "dsfdotnetmultiassignobjectactivity" };

            AddStepTool.NormalizeStepData("dsfdotnetmultiassignobjectactivity", data, label: null);

            Assert.AreEqual("Assign Object", (string?)data["displayname"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NormalizeStepData_DecisionType_DerivesDisplayText()
        {
            var data = new JObject { ["type"] = "flowdecision" };

            AddStepTool.NormalizeStepData("Decision", data, label: "Is Valid?");

            Assert.AreEqual("Is Valid?", (string?)data["displaytext"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NormalizeStepData_HttpType_AliasesRequestUrlToQuerystring()
        {
            var data = new JObject { ["type"] = "webgetactivity", ["requestUrl"] = "https://example.com/api" };

            AddStepTool.NormalizeStepData("Web Get", data, label: null);

            Assert.AreEqual("https://example.com/api", (string?)data[Constants.WEBMETHOD_QUERYSTRING]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void NormalizeStepData_ExistingDisplayName_IsNotOverwritten()
        {
            var data = new JObject { ["displayname"] = "Already Set" };

            AddStepTool.NormalizeStepData("Assign Object", data, label: "Should Not Win");

            Assert.AreEqual("Already Set", (string?)data["displayname"]);
        }

        // ── Tests: BuildConnectingEdge (Decision/Switch branch-validation rules,
        //    tested directly against hand-built graphs — bypasses the bodyEditable
        //    fidelity gate, since "Decision" itself is not yet Pass-fidelity; see
        //    AddStepTool's class remarks for why this is the only way to reach
        //    this logic today). ──────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildConnectingEdge_PlainNode_NoBranch_ReturnsSequenceEdge()
        {
            var tail = MakeAssignObject("tail1", "Tail", new JArray());

            var edge = AddStepTool.BuildConnectingEdge(tail, branch: null, edges: new List<Cell>(), newNodeId: "new1");

            Assert.AreEqual("edge", edge.shape);
            Assert.AreEqual("tail1", edge.Source!.Id);
            Assert.AreEqual("new1", edge.Target!.Id);
            Assert.AreEqual(Constants.SEQUENCE, edge.data[Constants.TYPE]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void BuildConnectingEdge_PlainNode_AlreadyHasOutgoing_Throws()
        {
            var tail = MakeAssignObject("tail1", "Tail", new JArray());
            var existingEdges = new List<Cell> { MakeEdge("e1", "tail1", "other") };

            AddStepTool.BuildConnectingEdge(tail, branch: null, edges: existingEdges, newNodeId: "new1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void BuildConnectingEdge_Decision_MissingBranch_Throws()
        {
            var tail = new Cell { id = "dec1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "flowdecision" } };

            AddStepTool.BuildConnectingEdge(tail, branch: null, edges: new List<Cell>(), newNodeId: "new1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void BuildConnectingEdge_Decision_InvalidBranchValue_Throws()
        {
            var tail = new Cell { id = "dec1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "flowdecision" } };

            AddStepTool.BuildConnectingEdge(tail, branch: "maybe", edges: new List<Cell>(), newNodeId: "new1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildConnectingEdge_Decision_TrueArm_ReturnsEdgeWithDecisionFlags()
        {
            var tail = new Cell { id = "dec1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "flowdecision" } };

            var edge = AddStepTool.BuildConnectingEdge(tail, branch: "true", edges: new List<Cell>(), newNodeId: "new1");

            Assert.AreEqual("edge", edge.shape);
            Assert.AreEqual(Constants.TRUE, edge.label);
            Assert.IsTrue((bool)edge.data[Constants.ISDECISIONARM]);
            Assert.IsTrue((bool)edge.data[Constants.ISTRUEARM]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildConnectingEdge_Decision_FalseArm_ReturnsEdgeWithDecisionFlags()
        {
            var tail = new Cell { id = "dec1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "flowdecision" } };

            var edge = AddStepTool.BuildConnectingEdge(tail, branch: "FALSE", edges: new List<Cell>(), newNodeId: "new1");

            Assert.AreEqual(Constants.FALSE, edge.label);
            Assert.IsTrue((bool)edge.data[Constants.ISDECISIONARM]);
            Assert.IsFalse((bool)edge.data[Constants.ISTRUEARM]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void BuildConnectingEdge_Decision_ArmAlreadyWired_Throws()
        {
            var tail = new Cell { id = "dec1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "flowdecision" } };
            var existingEdges = new List<Cell>
            {
                Dev2.WorkflowConverters.CommonHelper.CreateEdge("dec1", "other", label: Constants.TRUE, isDecisionArm: true, isTrueArm: true),
            };

            AddStepTool.BuildConnectingEdge(tail, branch: "true", edges: existingEdges, newNodeId: "new1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildConnectingEdge_Decision_OtherArmStillAvailable_Succeeds()
        {
            var tail = new Cell { id = "dec1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "flowdecision" } };
            var existingEdges = new List<Cell>
            {
                Dev2.WorkflowConverters.CommonHelper.CreateEdge("dec1", "other", label: Constants.TRUE, isDecisionArm: true, isTrueArm: true),
            };

            var edge = AddStepTool.BuildConnectingEdge(tail, branch: "false", edges: existingEdges, newNodeId: "new1");

            Assert.AreEqual(Constants.FALSE, edge.label);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void BuildConnectingEdge_Switch_MissingBranch_Throws()
        {
            var tail = new Cell { id = "sw1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "dsfflowswitchactivity" } };

            AddStepTool.BuildConnectingEdge(tail, branch: null, edges: new List<Cell>(), newNodeId: "new1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildConnectingEdge_Switch_ReturnsLabeledEdge()
        {
            var tail = new Cell { id = "sw1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "dsfflowswitchactivity" } };

            var edge = AddStepTool.BuildConnectingEdge(tail, branch: "CaseA", edges: new List<Cell>(), newNodeId: "new1");

            Assert.AreEqual("edge", edge.shape);
            Assert.AreEqual("CaseA", edge.label);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void BuildConnectingEdge_Switch_CaseAlreadyWired_Throws()
        {
            var tail = new Cell { id = "sw1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "dsfflowswitchactivity" } };
            var existingEdges = new List<Cell> { Dev2.WorkflowConverters.CommonHelper.CreateEdge("sw1", "other", label: "CaseA") };

            AddStepTool.BuildConnectingEdge(tail, branch: "CaseA", edges: existingEdges, newNodeId: "new1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildConnectingEdge_Switch_DifferentCase_Succeeds()
        {
            var tail = new Cell { id = "sw1", shape = "rect", data = new Dictionary<string, object> { ["type"] = "dsfflowswitchactivity" } };
            var existingEdges = new List<Cell> { Dev2.WorkflowConverters.CommonHelper.CreateEdge("sw1", "other", label: "CaseA") };

            var edge = AddStepTool.BuildConnectingEdge(tail, branch: "CaseB", edges: existingEdges, newNodeId: "new1");

            Assert.AreEqual("CaseB", edge.label);
        }
    }
}
