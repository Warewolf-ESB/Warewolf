/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ValidateWorkflowTool (warewolf-lee-mcp-v3-spec.md, "Tools" §
 *  validate_workflow): the 7 spec-mandated checks — body JSON/shape parse,
 *  start-node presence, per-node activity-type resolution, Decision/Switch
 *  branch completeness, envelope⇄body variable cross-references (undeclared
 *  = error, unused = warning), and the final real-compile safety net — using
 *  the same Cell/X6WorkflowSaveModel/Connector shapes and Newtonsoft
 *  serialization X6ToWorkflowConverterCoverageTests.cs uses, so fixtures here
 *  round-trip through the very same production converter validate_workflow's
 *  last check calls into.
 */

using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class ValidateWorkflowToolTests
    {
        // ── fixture builders ──────────────────────────────────────────────

        static Cell MakeNode(string id, string type, Dictionary<string, object> extraData = null)
        {
            var data = new Dictionary<string, object> { ["type"] = type };
            if (extraData != null)
            {
                foreach (var kv in extraData)
                {
                    data[kv.Key] = kv.Value;
                }
            }
            return new Cell { id = id, shape = "rect", data = data };
        }

        static Cell MakeStartNode(string id = "start") =>
            new Cell { id = id, shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };

        static Cell MakeEdge(string id, string sourceId, string targetId, Dictionary<string, object> data = null, string label = null) =>
            new Cell
            {
                id = id,
                shape = "edge",
                data = data ?? new Dictionary<string, object>(),
                Source = new Connector(sourceId),
                Target = new Connector(targetId),
                label = label
            };

        static JsonElement BodyOf(string resourceName, params Cell[] cells)
        {
            var graph = new X6WorkflowSaveModel { ResourceName = resourceName, Cells = new List<Cell>(cells) };
            var json = JsonConvert.SerializeObject(graph);
            return JsonDocument.Parse(json).RootElement;
        }

        static JsonElement EnvelopeOf(object envelopeObj) => System.Text.Json.JsonSerializer.SerializeToElement(envelopeObj);

        static readonly JsonElement EmptyEnvelope = EnvelopeOf(new { inputs = new object[0], outputs = new object[0] });

        // ── required-parameter guards ─────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Throws_WhenEnvelopeMissing()
        {
            Assert.ThrowsException<McpException>(() => ValidateWorkflowTool.Handle(default, BodyOf("Wf", MakeStartNode())));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Throws_WhenBodyMissing()
        {
            Assert.ThrowsException<McpException>(() => ValidateWorkflowTool.Handle(EmptyEnvelope, default));
        }

        // ── valid minimal workflow ─────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ValidMinimalWorkflow_ReturnsValidWithNoErrors()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new[] { new { name = "Message", kind = "scalar", fields = new string[0] } },
                outputs = new[] { new { name = "Result", kind = "scalar", fields = new string[0] } }
            });

            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[Result]]",
                ["FieldValue"] = "[[Message]]",
                ["IndexNumber"] = 1
            });
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign", ["fields"] = fields });
            var body = BodyOf("ValidWf", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsTrue(result.Valid, string.Join("; ", result.Errors.Select(e => e.Message)));
            Assert.AreEqual(0, result.Errors.Count);
        }

        // ── body parse / shape ─────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EmptyBody_ReturnsInvalid_WithNoReachableStartNodeError()
        {
            // X6WorkflowSaveModel.Cells field-initializes to an empty list, so "{}" deserializes
            // to a graph with zero cells (not a null Cells array) — the resulting error is "no
            // reachable start node", not "no cells array" (that path needs an explicit `"cells":
            // null`, covered by Handle_NullCellsArray_ReturnsInvalid_WithNoCellsError below).
            var body = JsonDocument.Parse("{}").RootElement;

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("no reachable start node")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NullCellsArray_ReturnsInvalid_WithNoCellsError()
        {
            var body = JsonDocument.Parse("{\"resourcename\":\"Null\",\"cells\":null}").RootElement;

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("no `cells` array")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_MalformedBodyShape_ReturnsInvalid_AsStructuredError_NotException()
        {
            // "cells" as a string (not an array) cannot deserialize into List<Cell>; the
            // resulting Newtonsoft exception must be caught and reported as a structured
            // error, never propagated to the MCP caller.
            var body = JsonDocument.Parse("{\"resourcename\":\"Bad\",\"cells\":\"not-an-array\"}").RootElement;

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("not a valid X6 graph")));
        }

        // ── start node presence ───────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NoStartNode_ReturnsInvalid_WithMissingStartError()
        {
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign", ["fields"] = new JArray() });
            var body = BodyOf("NoStart", assign);

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("no reachable start node")));
        }

        // ── unresolved activity type ───────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_UnknownActivityType_ReturnsInvalid_NamingTheCell()
        {
            var bogus = MakeNode("bogus1", "totally-not-a-real-activity-type");
            var body = BodyOf("Unknown", MakeStartNode(), bogus, MakeEdge("e1", "start", "bogus1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e =>
                e.Message.Contains("unrecognised activity type") && e.Message.Contains("bogus1")));
        }

        // ── Decision branch completeness ──────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_DecisionMissingFalseBranch_ReturnsInvalid()
        {
            var decision = MakeNode("dec1", "flowdecision");
            var trueEdge = MakeEdge("e-true", "dec1", "start",
                new Dictionary<string, object> { [Constants.ISDECISIONARM] = true, [Constants.ISTRUEARM] = true });
            var body = BodyOf("DecisionGap", MakeStartNode(), decision,
                MakeEdge("e0", "start", "dec1"), trueEdge);

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("dec1") && e.Message.Contains("False branch")));
            Assert.IsFalse(result.Errors.Any(e => e.Message.Contains("True branch")),
                "The True branch was supplied and must not also be reported missing.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_DecisionBranches_IgnoreEdgeLabelText()
        {
            // A "true"/"false" labeled edge without isDecisionArm/isTrue data must NOT satisfy
            // the check — X6ToWorkflowConverter.HandleDecisionConnection ignores label for Decision.
            var decision = MakeNode("dec1", "flowdecision");
            var labeledOnly = MakeEdge("e-true", "dec1", "start", label: "true");
            var body = BodyOf("DecisionLabelOnly", MakeStartNode(), decision,
                MakeEdge("e0", "start", "dec1"), labeledOnly);

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("True branch")));
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("False branch")));
        }

        // ── Switch case completeness ──────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SwitchWithZeroCases_ReturnsInvalid()
        {
            var switchNode = MakeNode("sw1", "dsfflowswitchactivity");
            var unlabeledEdge = MakeEdge("e-out", "sw1", "start");
            var body = BodyOf("SwitchGap", MakeStartNode(), switchNode,
                MakeEdge("e0", "start", "sw1"), unlabeledEdge);

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("sw1") && e.Message.Contains("no labeled case edges")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SwitchWithOneCaseKeyEdge_Passes()
        {
            var switchNode = MakeNode("sw1", "dsfflowswitchactivity");
            var caseEdge = MakeEdge("e-out", "sw1", "start",
                new Dictionary<string, object> { ["caseKey"] = "Case1" });
            var body = BodyOf("SwitchOk", MakeStartNode(), switchNode,
                MakeEdge("e0", "start", "sw1"), caseEdge);

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Errors.Any(e => e.Message.Contains("no labeled case edges")));
        }

        // ── envelope ⇄ body variable cross-references ─────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_UndeclaredBodyReference_ReturnsInvalid_AsError()
        {
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[NeverDeclared]]",
                ["FieldValue"] = "1",
                ["IndexNumber"] = 1
            });
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign", ["fields"] = fields });
            var body = BodyOf("Undeclared", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e =>
                e.Severity == "error" && e.Message.Contains("NeverDeclared") && e.Message.Contains("not declared")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_UnusedEnvelopeInput_ReturnsWarningOnly_ValidStaysTrue()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new[] { new { name = "Unused", kind = "scalar", fields = new string[0] } },
                outputs = new object[0]
            });
            // A workflow that never references [[Unused]] anywhere.
            var body = BodyOf("UnusedInput", MakeStartNode());

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsTrue(result.Valid, string.Join("; ", result.Errors.Select(e => e.Message)));
            Assert.IsTrue(result.Errors.Any(e => e.Severity == "warning" && e.Message.Contains("Unused") && e.Message.Contains("never referenced")));
            Assert.IsFalse(result.Errors.Any(e => e.Severity == "error"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_RecordsetFieldReference_UndeclaredField_ReturnsError()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new[] { new { name = "Customers", kind = "recordset", fields = new[] { "Name" } } },
                outputs = new object[0]
            });
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[Result]]",
                ["FieldValue"] = "[[Customers(1).Age]]",
                ["IndexNumber"] = 1
            });
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign", ["fields"] = fields });
            var body = BodyOf("RecordsetGap", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e =>
                e.Message.Contains("Age") && e.Message.Contains("Customers") && e.Message.Contains("no such field declared")));
        }

        // ── final real-compile safety net ─────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_HeuristicallyValidButUnwiredActivityType_FailsAtFinalCompileStep()
        {
            // "Calculate" (dsfdotnetcalculateactivity) resolves via ToolCatalog.Resolve (so the
            // per-node heuristic check passes) but has no case in
            // X6ToWorkflowConverter.CreateActivityFromNode's switch, so the real compile throws
            // UnsupportedActivityTypeException — the safety net this last check exists for.
            var calculate = MakeNode("calc1", "dsfdotnetcalculateactivity");
            var body = BodyOf("CalculateGap", MakeStartNode(), calculate, MakeEdge("e1", "start", "calc1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("failed to compile")));
        }
    }
}
