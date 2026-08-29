/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ValidateWorkflowTool: the 7 spec-mandated checks — body JSON/shape parse,
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

        // ── collection fields (Assign.fields et al) ────────────────────────
        //
        // get_tool_schema documented these as "a JSON-encoded array", so callers sent a
        // string. The converter's `value as JArray` cast returned null for it and the
        // collection was dropped in silence — create_workflow reported created:true and the
        // activity produced nothing (Assign on warewolfserver-mcp, 2026-08-21). The string
        // form is now accepted; a value that is neither must fail loudly rather than lose data.

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_CollectionFieldAsJsonArrayString_IsValid()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new[] { new { name = "Message", kind = "scalar", fields = new string[0] } },
                outputs = new[] { new { name = "Result", kind = "scalar", fields = new string[0] } }
            });

            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object>
                {
                    ["displayName"] = "Assign",
                    ["fields"] = "[{\"FieldName\":\"[[Result]]\",\"FieldValue\":\"[[Message]]\",\"IndexNumber\":1}]"
                });
            var body = BodyOf("StringFieldsWf", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsTrue(result.Valid, string.Join("; ", result.Errors.Select(e => e.Message)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_CollectionFieldNotAnArray_ReturnsInvalid_NamingCellAndField()
        {
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign", ["fields"] = "not-an-array" });
            var body = BodyOf("BadFieldsWf", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            var issue = result.Errors.SingleOrDefault(e => e.Message.Contains("'fields'"));
            Assert.IsNotNull(issue, "expected an error naming the offending field; got: "
                + string.Join("; ", result.Errors.Select(e => e.Message)));
            StringAssert.Contains(issue.Message, "assign1");
            StringAssert.Contains(issue.Message, "not-an-array");
            Assert.AreEqual("/cells/1/data/fields", issue.Path);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_CollectionFieldIsJsonObjectNotArray_ReturnsInvalid()
        {
            // Valid JSON, but an object — must not be silently coerced into a one-row collection.
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object>
                {
                    ["displayName"] = "Assign",
                    ["fields"] = "{\"FieldName\":\"[[Result]]\"}"
                });
            var body = BodyOf("ObjFieldsWf", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("'fields'")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_CollectionFieldAbsent_ReportsNoCollectionError()
        {
            // Whether the field is required is the schema's business; this check only rejects
            // a value that is present and unusable.
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign" });
            var body = BodyOf("NoFieldsWf", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Errors.Any(e => e.Message.Contains("'fields'")),
                "an absent collection field must not raise a collection error");
        }

        // ── envelope name legality (F9) ──────────────────────────────────────

        /// <summary>
        /// F9: EnvelopeBiteWriter's rejection of an illegal XML element name was purely incidental
        /// (XElement construction throwing, caught and rewrapped) - validate_workflow never
        /// diagnosed it ahead of time, contradicting the tool description's claim that a payload
        /// passing validate_workflow is accepted by create_workflow.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_IllegalEnvelopeVariableName_ReturnsInvalid()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new[] { new { name = "1 bad name", kind = "scalar", fields = new string[0] } },
                outputs = new object[0]
            });
            var body = BodyOf("IllegalName", MakeStartNode());

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e =>
                e.Severity == "error" && e.Message.Contains("1 bad name") && e.Message.Contains("not a legal XML element name")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_IllegalRecordsetFieldName_ReturnsInvalid()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new[] { new { name = "Customers", kind = "recordset", fields = new[] { "bad field" } } },
                outputs = new object[0]
            });
            var body = BodyOf("IllegalFieldName", MakeStartNode());

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e =>
                e.Severity == "error" && e.Message.Contains("bad field") && e.Message.Contains("Customers") && e.Message.Contains("not a legal XML element name")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_LegalEnvelopeNames_AreUnaffected()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new[] { new { name = "Customers", kind = "recordset", fields = new[] { "Name" } } },
                outputs = new object[0]
            });
            var body = BodyOf("LegalNames", MakeStartNode());

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsFalse(result.Errors.Any(e => e.Message.Contains("not a legal XML element name")));
        }

        // ── newly-covered array fields (F9) ──────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_GetWebMethodHeaders_AsJsonArray_IsValid()
        {
            var node = MakeNode("get1", "webgetactivity", new Dictionary<string, object>
            {
                ["displayName"] = "GET",
                ["querystring"] = "search?q=warewolf",
                ["headers"] = new JArray(new JObject { ["Name"] = "Accept", ["Value"] = "application/json" })
            });
            var body = BodyOf("WebGetHeadersOk", MakeStartNode(), node, MakeEdge("e1", "start", "get1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Errors.Any(e => e.Message.Contains("'headers'")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_GetWebMethodHeaders_NotAnArray_ReturnsInvalid()
        {
            var node = MakeNode("get1", "webgetactivity", new Dictionary<string, object>
            {
                ["displayName"] = "GET",
                ["querystring"] = "search?q=warewolf",
                ["headers"] = "not-an-array"
            });
            var body = BodyOf("WebGetHeadersBad", MakeStartNode(), node, MakeEdge("e1", "start", "get1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("'headers'") && e.Message.Contains("get1")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SqlBulkInsertInputMappings_NotAnArray_ReturnsInvalid()
        {
            var node = MakeNode("bulk1", "dsfsqlbulkinsertactivity", new Dictionary<string, object>
            {
                ["displayName"] = "Bulk Insert",
                ["inputmappings"] = "not-an-array"
            });
            var body = BodyOf("BulkInsertBad", MakeStartNode(), node, MakeEdge("e1", "start", "bulk1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("'inputmappings'") && e.Message.Contains("bulk1")));
        }

        // ── output-mapping shape (GET/POST/PUT/DELETE Web Method, Advanced Recordset, ─────
        // Service (sub-workflow), every SQL/ODBC database activity — anything read via
        // CommonHelper.TryGetOutputs) ────────────────────────────────────────────────────
        //
        // CommonHelper.TryGetOutputs reads each element's MappedFrom/MappedTo/RecordSetName/Path
        // keys via JObject.Value<string>(...), silently defaulting any key it doesn't find to "" —
        // a caller-supplied shape like {name, mapsTo} (get_tool_schema never documented the real
        // key names) previously wrote an all-empty mapping with no error at all (observed against
        // warewolfserver-mcp: [[ResponseBody]] silently became MappedFrom: "").

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_GetWebMethodOutputs_WrongKeys_ReturnsInvalid_NamingCellAndField()
        {
            var node = MakeNode("get1", "webgetactivity", new Dictionary<string, object>
            {
                ["displayName"] = "GET",
                ["querystring"] = "search?q=warewolf",
                ["outputs"] = new JArray(new JObject { ["name"] = "[[ResponseBody]]", ["mapsTo"] = "" })
            });
            var body = BodyOf("WebGetOutputsBadKeys", MakeStartNode(), node, MakeEdge("e1", "start", "get1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            var issue = result.Errors.SingleOrDefault(e => e.Message.Contains("output-mapping shape"));
            Assert.IsNotNull(issue, "expected an output-mapping-shape error; got: "
                + string.Join("; ", result.Errors.Select(e => e.Message)));
            StringAssert.Contains(issue.Message, "get1");
            StringAssert.Contains(issue.Message, "outputs[0]");
            StringAssert.Contains(issue.Message, "MappedFrom");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_GetWebMethodOutputs_CorrectKeys_IsValid()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new object[0],
                outputs = new[] { new { name = "ResponseBody", kind = "scalar", fields = new string[0] } }
            });
            var node = MakeNode("get1", "webgetactivity", new Dictionary<string, object>
            {
                ["displayName"] = "GET",
                ["querystring"] = "search?q=warewolf",
                ["outputs"] = new JArray(new JObject
                {
                    ["MappedFrom"] = "[[ResponseBody]]",
                    ["MappedTo"] = "",
                    ["RecordSetName"] = ""
                })
            });
            var body = BodyOf("WebGetOutputsOk", MakeStartNode(), node, MakeEdge("e1", "start", "get1"));

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsFalse(result.Errors.Any(e => e.Message.Contains("output-mapping shape")),
                string.Join("; ", result.Errors.Select(e => e.Message)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SqlServerDatabaseOutputs_WrongKeys_ReturnsInvalid()
        {
            var node = MakeNode("sql1", "dsfsqlserverdatabaseactivity", new Dictionary<string, object>
            {
                ["displayName"] = "SQL",
                ["outputs"] = new JArray(new JObject { ["column"] = "Id" })
            });
            var body = BodyOf("SqlOutputsBadKeys", MakeStartNode(), node, MakeEdge("e1", "start", "sql1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("output-mapping shape") && e.Message.Contains("sql1")));
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

        /// <summary>
        /// Regression test: X6ToWorkflowConverter.BuildWorkflow compiles the "start" cell to a
        /// placeholder WriteLine activity and discards it via
        /// <c>flowchart.StartNode = startFlowNode.Next ?? startFlowNode</c> - if the start cell
        /// has no outgoing edge, that non-IDev2Activity placeholder becomes the literal
        /// StartNode, and ActivityParser.Parse crashed with an unhandled
        /// ArgumentNullException("source") at execution time (reproduced against
        /// warewolfserver-mcp and locally, 2026-08-20). This must be caught here - before
        /// create_workflow/edit_workflow ever persist the file - not just at execution.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_StartNodeWithNoOutgoingEdge_ReturnsInvalid()
        {
            var body = BodyOf("StartOnly", MakeStartNode());

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Severity == "error" &&
                e.Message.Contains("no outgoing connection", StringComparison.OrdinalIgnoreCase)));
        }

        // ── edge detection tolerates a missing `shape` (F7) ───────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EdgeWithExplicitShape_StillValidates()
        {
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign", ["fields"] = new JArray() });
            var body = BodyOf("ExplicitShapeEdge", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsTrue(result.Valid, string.Join("; ", result.Errors.Select(e => e.Message)));
        }

        /// <summary>
        /// get_workflow_schema documents shape:"edge" as required, but X6ToWorkflowConverter (the
        /// real compiler validate_workflow's final check defers to) still requires it strictly - a
        /// {source, target} cell with no shape genuinely cannot be authored as a connection. Before
        /// F7, the resulting "no outgoing connection" error gave no hint why: it looked identical
        /// to a workflow with no connection at all. The fix is a more specific message, not
        /// silently accepting the missing shape.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_StartNodeConnectedOnlyByShapelessCell_NamesTheMissingShapeAsCause()
        {
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign", ["fields"] = new JArray() });
            var shapelessEdge = new Cell
            {
                data = new Dictionary<string, object>(),
                Source = new Connector("start"),
                Target = new Connector("assign1")
            };
            var body = BodyOf("ShapelessEdge", MakeStartNode(), assign, shapelessEdge);

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Severity == "error" &&
                e.Message.Contains("shape", StringComparison.OrdinalIgnoreCase) &&
                e.Message.Contains("edge", StringComparison.OrdinalIgnoreCase)));
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

        // ── object sigil ('@') references (F4) ─────────────────────────────

        /// <summary>
        /// F4: SplitReference used to leave the '@' object sigil on the base name, so
        /// [[@Response]] never matched a declared 'Response' entry — an object-mode variable
        /// could not be declared and referenced at the same time. Confirms the fix.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ObjectSigilReference_AgainstDeclaredObject_ValidatesClean()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new object[0],
                outputs = new[] { new { name = "Response", kind = "object", fields = new string[0] } }
            });
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[@Response]]",
                ["FieldValue"] = "{}",
                ["IndexNumber"] = 1
            });
            var assign = MakeNode("assign1", "dsfdotnetmultiassignobjectactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign Object", ["fields"] = fields });
            var body = BodyOf("ObjectRef", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsTrue(result.Valid, string.Join("; ", result.Errors.Select(e => e.Message)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ObjectSigilReference_AgainstDeclaredScalar_ReturnsError_NamingKindMismatch()
        {
            var envelope = EnvelopeOf(new
            {
                inputs = new object[0],
                outputs = new[] { new { name = "Response", kind = "scalar", fields = new string[0] } }
            });
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[@Response]]",
                ["FieldValue"] = "{}",
                ["IndexNumber"] = 1
            });
            var assign = MakeNode("assign1", "dsfdotnetmultiassignobjectactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign Object", ["fields"] = fields });
            var body = BodyOf("ObjectKindMismatch", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(envelope, body);

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e =>
                e.Severity == "error" && e.Message.Contains("Response") && e.Message.Contains("scalar") && e.Message.Contains("object")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ObjectSigilReference_Undeclared_ReturnsError_NamingStrippedName()
        {
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[@Missing]]",
                ["FieldValue"] = "{}",
                ["IndexNumber"] = 1
            });
            var assign = MakeNode("assign1", "dsfdotnetmultiassignobjectactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign Object", ["fields"] = fields });
            var body = BodyOf("ObjectUndeclared", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, body);

            Assert.IsFalse(result.Valid);
            var error = result.Errors.Single(e => e.Severity == "error");
            Assert.IsTrue(error.Message.Contains("'Missing' is not declared"));
            Assert.IsFalse(error.Message.Contains("'@Missing' is not declared"));
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
            // A workflow that never references [[Unused]] anywhere - still needs a real step
            // connected to start (a degenerate start-only body is itself a hard error; see
            // Handle_StartNodeWithNoOutgoingEdge_ReturnsInvalid below).
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object>
                {
                    ["displayName"] = "Assign",
                    ["fields"] = new JArray(new JObject { ["FieldName"] = "NotAVariable", ["FieldValue"] = "hello", ["IndexNumber"] = 1 })
                });
            var body = BodyOf("UnusedInput", MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));

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
        // ── non-object envelope/body guards ───────────────────────────────
        //
        // Regression: `envelope`/`body` bind as a raw JsonElement, so a caller sending either as a
        // JSON *string* used to bind cleanly and then blow up deep inside ParseEnvelopeVariables,
        // where JsonElement.TryGetProperty throws InvalidOperationException ("requires an element
        // of type 'Object', but the target element has type 'String'"). McpApiFunctions.Invoke did
        // not catch that, so an MCP caller saw only a bare HTTP 500 with an empty body — observed
        // 2026-08-21 against warewolfserver-mcp. These assert a clean McpException (which Invoke
        // maps to a 400 carrying the message) for every non-object JsonValueKind.

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EnvelopeSentAsJsonString_ThrowsMcpException_NotInvalidOperationException()
        {
            var envelope = JsonDocument.Parse("\"{\\\"inputs\\\":[]}\"").RootElement;
            Assert.AreEqual(JsonValueKind.String, envelope.ValueKind);

            var ex = Assert.ThrowsException<McpException>(
                () => ValidateWorkflowTool.Handle(envelope, BodyOf("Wf", MakeStartNode())));

            StringAssert.Contains(ex.Message, "`envelope` must be a JSON object");
            StringAssert.Contains(ex.Message, "String");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_BodySentAsJsonString_ThrowsMcpException()
        {
            var body = JsonDocument.Parse("\"{\\\"cells\\\":[]}\"").RootElement;

            var ex = Assert.ThrowsException<McpException>(
                () => ValidateWorkflowTool.Handle(EmptyEnvelope, body));

            StringAssert.Contains(ex.Message, "`body` must be a JSON object");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_BothSentAsJsonStrings_ReportsBothInOneMessage()
        {
            var asString = JsonDocument.Parse("\"{}\"").RootElement;

            var ex = Assert.ThrowsException<McpException>(
                () => ValidateWorkflowTool.Handle(asString, asString));

            StringAssert.Contains(ex.Message, "`envelope` must be a JSON object");
            StringAssert.Contains(ex.Message, "`body` must be a JSON object");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EnvelopeSentAsArrayOrNumber_ThrowsMcpException()
        {
            var asArray = JsonDocument.Parse("[]").RootElement;
            var asNumber = JsonDocument.Parse("42").RootElement;
            var body = BodyOf("Wf", MakeStartNode());

            Assert.ThrowsException<McpException>(() => ValidateWorkflowTool.Handle(asArray, body));
            Assert.ThrowsException<McpException>(() => ValidateWorkflowTool.Handle(asNumber, body));
        }

        /// <summary>
        /// F6: get_workflow_definition used to emit envelope.inputs/outputs as bare strings — this
        /// is exactly that shape fed back in. It used to throw an unhandled
        /// InvalidOperationException (from TryGetProperty on a JSON string), which McpApiFunctions
        /// mapped to a raw 500; it must now be a structured McpException (→ 400) naming the array
        /// and index.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_StringShapedEnvelopeEntry_ThrowsMcpException_NamingArrayAndIndex()
        {
            var envelope = EnvelopeOf(new { inputs = new[] { "JustAString" }, outputs = Array.Empty<object>() });
            var body = BodyOf("Wf", MakeStartNode());

            var ex = Assert.ThrowsException<McpException>(
                () => ValidateWorkflowTool.Handle(envelope, body));

            StringAssert.Contains(ex.Message, "inputs[0]");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ProperObjectEnvelopeAndBody_StillValidatesNormally()
        {
            // Guards against the new ValueKind check rejecting the happy path it must let through.
            // The Assign node carries a displayName because X6ToWorkflowConverter.CreateAssignActivity
            // returns null without one, and an unconvertible node now fails the compile instead of
            // being dropped in silence - so a fixture missing it is not the happy path.
            var assign = MakeNode("assign1", "dsfdotnetmultiassignactivity",
                new Dictionary<string, object> { ["displayName"] = "Assign" });

            var result = ValidateWorkflowTool.Handle(EmptyEnvelope, BodyOf("Wf", MakeStartNode(),
                assign, MakeEdge("e1", "start", "assign1")));

            Assert.IsTrue(result.Valid, string.Join("; ", result.Errors.Select(e => e.Message)));
        }
    }
}
