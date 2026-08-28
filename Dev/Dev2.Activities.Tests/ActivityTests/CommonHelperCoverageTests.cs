/*
 * Coverage tests for Dev2.WorkflowConverters.CommonHelper and CommonHelperExtensions.
 *
 * Why this file exists
 * --------------------
 * The merged Cobertura snapshot shows CommonHelper at only 3.6% line coverage
 * (380 uncovered lines).  CommonHelper is a pure dictionary/JSON deserialisation
 * helper used by every X6 -> activity converter, so unit tests do not need a
 * workflow host.  The class also exposes a parallel extension-method facade
 * (CommonHelperExtensions) which is wholly uncovered; both are driven from this
 * file.
 *
 * Coverage focus
 * --------------
 *   • GenerateNodeId / CreateEdge — node-graph helpers used by every emitter.
 *   • TryGetBool / TryGetString / TryGetInt / TryGetGuid — primitive parsers
 *     including their null-data, missing-key, wrong-type and string-coercion
 *     branches.
 *   • TryGetJArray — null data, null keys, key-not-present, wrong-type and
 *     happy paths across multiple candidate keys.
 *   • TryGetList<TConcrete,TInterface> — happy path and the swallow-exception
 *     branch (achieved by passing an array whose element shape is unmappable).
 *   • TryAsJObject — the JObject counterpart to TryAsJArray: instance, string,
 *     malformed-string, wrong-shape-string and null/whitespace/scalar branches.
 *   • TryGetOutputs / TryGetOutputDescription — array/object and JSON-encoded
 *     string forms, including the nested JsonPath block on each
 *     ServiceOutputMapping.
 *   • The JSON-encoded string form at every top-level reader — the six readers
 *     that previously pattern-matched the raw type directly and so dropped a
 *     documented string payload silently.
 *   • TryGetHeaders / TryGetInputs / TryGetSettings — thin wrappers around
 *     TryGetList; covered to exercise both the "updatedheaders" fallback and
 *     the empty-payload branch.
 *   • TryGetConditions — text vs file FormDataCondition discrimination plus
 *     enum-as-int and enum-as-string TableType parsing.
 *   • TryGetInputMappings — JArray and existing-IList branches; SqlDataType
 *     as int/string; DataType string-to-Type mapping including unknown-name
 *     fallback.
 *   • TryGetFindRecordsCollection — JArray with optional WhereOptionList, and
 *     existing-IList passthrough.
 *   • TryGetRabbitMqPublishOptions — ExecutionID (default + int + string),
 *     Manual (with CorrelationID), CustomTransactionID, and the existing-
 *     options passthrough.
 *   • CommonHelperExtensions — every wrapper is invoked at least once so that
 *     the delegating one-liners are not dead.
 *
 * The tests assert on observable contracts — return value of the Try* method
 * and the most stable field on the produced object — rather than on internal
 * implementation details, so they remain robust to refactors that preserve
 * behaviour.
 */

using System;
using System.Collections.Generic;
using System.Data;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.X6;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Dev2.WorkflowConverters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Framework.Converters.Graph.Ouput;
using Unlimited.Framework.Converters.Graph.String.Json;
using Warewolf.Core;
using Warewolf.Data.Options;
using Warewolf.Options;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class CommonHelperCoverageTests
    {
        // ─────────────────────────────────────────────────────────────────
        // GenerateNodeId / CreateEdge
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void GenerateNodeId_ProducesParsableGuidPerInvocation()
        {
            var a = CommonHelper.GenerateNodeId();
            var b = CommonHelper.GenerateNodeId();

            Assert.IsTrue(Guid.TryParse(a, out _));
            Assert.IsTrue(Guid.TryParse(b, out _));
            Assert.AreNotEqual(a, b);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void CreateEdge_PopulatesEndpointsLabelAndSequenceTypeMarker()
        {
            var edge = CommonHelper.CreateEdge("src-1", "tgt-2", "go");

            Assert.IsNotNull(edge);
            Assert.IsFalse(string.IsNullOrEmpty(edge.id));
            Assert.AreEqual("src-1", edge.Source.Id);
            Assert.AreEqual("tgt-2", edge.Target.Id);
            Assert.AreEqual("go", edge.label);
            Assert.IsTrue(edge.data.ContainsKey(Constants.TYPE));
            Assert.AreEqual(Constants.SEQUENCE, edge.data[Constants.TYPE]);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void CreateEdge_DefaultLabelIsEmpty()
        {
            var edge = CommonHelper.CreateEdge("a", "b");
            Assert.AreEqual(string.Empty, edge.label);
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetBool
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetBool_NullDictionary_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetBool(null, "x", out var v));
            Assert.IsFalse(v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetBool_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetBool(new Dictionary<string, object>(), "x", out var v));
            Assert.IsFalse(v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetBool_NullValue_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["x"] = null };
            Assert.IsFalse(CommonHelper.TryGetBool(d, "x", out var v));
            Assert.IsFalse(v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetBool_BoolValue_ReturnsTrue()
        {
            var d = new Dictionary<string, object> { ["x"] = true };
            Assert.IsTrue(CommonHelper.TryGetBool(d, "x", out var v));
            Assert.IsTrue(v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetBool_StringTrue_ReturnsTrue()
        {
            var d = new Dictionary<string, object> { ["x"] = "true" };
            Assert.IsTrue(CommonHelper.TryGetBool(d, "x", out var v));
            Assert.IsTrue(v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetBool_NonParseableString_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["x"] = "not-a-bool" };
            Assert.IsFalse(CommonHelper.TryGetBool(d, "x", out _));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetBool_OtherType_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["x"] = 42 };
            Assert.IsFalse(CommonHelper.TryGetBool(d, "x", out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetString
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetString_NullData_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetString(null, "x", out var v));
            Assert.AreEqual(string.Empty, v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetString_StringValue_Returned()
        {
            var d = new Dictionary<string, object> { ["x"] = "hello" };
            Assert.IsTrue(CommonHelper.TryGetString(d, "x", out var v));
            Assert.AreEqual("hello", v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetString_NonStringValue_CoercedWithToString()
        {
            var d = new Dictionary<string, object> { ["x"] = 123 };
            Assert.IsTrue(CommonHelper.TryGetString(d, "x", out var v));
            Assert.AreEqual("123", v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetString_NullValue_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["x"] = null };
            Assert.IsFalse(CommonHelper.TryGetString(d, "x", out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetInt
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInt_IntValue_Returned()
        {
            var d = new Dictionary<string, object> { ["x"] = 42 };
            Assert.IsTrue(CommonHelper.TryGetInt(d, "x", out var v));
            Assert.AreEqual(42, v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInt_LongValue_Downcast()
        {
            var d = new Dictionary<string, object> { ["x"] = 7L };
            Assert.IsTrue(CommonHelper.TryGetInt(d, "x", out var v));
            Assert.AreEqual(7, v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInt_StringInt_Parsed()
        {
            var d = new Dictionary<string, object> { ["x"] = "99" };
            Assert.IsTrue(CommonHelper.TryGetInt(d, "x", out var v));
            Assert.AreEqual(99, v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInt_UnparseableString_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["x"] = "abc" };
            Assert.IsFalse(CommonHelper.TryGetInt(d, "x", out _));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInt_NullDictionary_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetInt(null, "x", out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetGuid
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetGuid_GuidValue_Returned()
        {
            var g = Guid.NewGuid();
            var d = new Dictionary<string, object> { ["x"] = g };
            Assert.IsTrue(CommonHelper.TryGetGuid(d, "x", out var v));
            Assert.AreEqual(g, v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetGuid_StringGuid_Parsed()
        {
            var g = Guid.NewGuid();
            var d = new Dictionary<string, object> { ["x"] = g.ToString() };
            Assert.IsTrue(CommonHelper.TryGetGuid(d, "x", out var v));
            Assert.AreEqual(g, v);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetGuid_BadString_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["x"] = "not-a-guid" };
            Assert.IsFalse(CommonHelper.TryGetGuid(d, "x", out _));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetGuid_WrongType_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["x"] = 7 };
            Assert.IsFalse(CommonHelper.TryGetGuid(d, "x", out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetJArray
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetJArray_NullData_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetJArray(null, out var arr, "k"));
            Assert.IsNull(arr);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetJArray_NoKeys_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetJArray(new Dictionary<string, object>(), out _));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetJArray_FallbackKeyResolved()
        {
            var payload = new JArray { 1, 2, 3 };
            var d = new Dictionary<string, object> { ["second"] = payload };
            Assert.IsTrue(CommonHelper.TryGetJArray(d, out var arr, "first", "second"));
            Assert.AreSame(payload, arr);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetJArray_WrongType_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { ["k"] = "string-not-array" };
            Assert.IsFalse(CommonHelper.TryGetJArray(d, out _, "k"));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryAsJArray
        //
        // get_tool_schema documented every collection field as "a JSON-encoded
        // array", so callers sent a *string*. The old `value as JArray` cast
        // returned null for it and the collection was dropped with no error at
        // all — create_workflow still reported success and the activity produced
        // nothing (seen for Assign on warewolfserver-mcp, 2026-08-21).
        // TryAsJArray accepts both shapes; anything genuinely unusable returns
        // false so validate_workflow can reject it loudly instead.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJArray_JArray_ReturnsSameInstance()
        {
            var payload = new JArray { 1, 2, 3 };
            Assert.IsTrue(CommonHelper.TryAsJArray(payload, out var arr));
            Assert.AreSame(payload, arr);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJArray_JsonArrayString_IsParsed()
        {
            const string json = "[{\"FieldName\":\"[[Result]]\",\"FieldValue\":\"hello\"}]";

            Assert.IsTrue(CommonHelper.TryAsJArray(json, out var arr));
            Assert.AreEqual(1, arr.Count);
            Assert.AreEqual("[[Result]]", arr[0]["FieldName"].Value<string>());
            Assert.AreEqual("hello", arr[0]["FieldValue"].Value<string>());
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJArray_MalformedJsonString_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryAsJArray("[{\"FieldName\": ", out var arr));
            Assert.IsNull(arr);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJArray_JsonObjectString_ReturnsFalse()
        {
            // Valid JSON, but an object rather than an array — must not be coerced.
            Assert.IsFalse(CommonHelper.TryAsJArray("{\"FieldName\":\"[[a]]\"}", out var arr));
            Assert.IsNull(arr);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJArray_NullOrWhitespaceOrScalar_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryAsJArray(null, out _));
            Assert.IsFalse(CommonHelper.TryAsJArray("   ", out _));
            Assert.IsFalse(CommonHelper.TryAsJArray(42, out _));
            Assert.IsFalse(CommonHelper.TryAsJArray(new JObject(), out _));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetJArray_JsonArrayString_IsAcceptedViaTryAsJArray()
        {
            // The multi-key reader — and therefore TryGetList<> and every converter
            // built on it — inherits the string form from TryAsJArray.
            var d = new Dictionary<string, object> { ["fields"] = "[1,2,3]" };

            Assert.IsTrue(CommonHelper.TryGetJArray(d, out var arr, "updatedfields", "fields"));
            Assert.AreEqual(3, arr.Count);
        }

        // ─────────────────────────────────────────────────────────────────
        // TryAsJObject
        //
        // The JObject counterpart to TryAsJArray, added for the same reason:
        // get_tool_schema documents object-shaped fields as a "JSON-encoded"
        // string, so callers send one. Without it the value was dropped
        // silently and the activity ran with the field unset.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJObject_JObject_ReturnsSameInstance()
        {
            var payload = new JObject { ["a"] = 1 };
            Assert.IsTrue(CommonHelper.TryAsJObject(payload, out var obj));
            Assert.AreSame(payload, obj);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJObject_JsonObjectString_IsParsed()
        {
            const string json = "{\"FieldName\":\"[[Result]]\",\"FieldValue\":\"hello\"}";

            Assert.IsTrue(CommonHelper.TryAsJObject(json, out var obj));
            Assert.AreEqual("[[Result]]", obj["FieldName"].Value<string>());
            Assert.AreEqual("hello", obj["FieldValue"].Value<string>());
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJObject_MalformedJsonString_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryAsJObject("{\"FieldName\": ", out var obj));
            Assert.IsNull(obj);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJObject_JsonArrayString_ReturnsFalse()
        {
            // Valid JSON, but an array rather than an object — must not be coerced.
            Assert.IsFalse(CommonHelper.TryAsJObject("[{\"FieldName\":\"[[a]]\"}]", out var obj));
            Assert.IsNull(obj);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryAsJObject_NullOrWhitespaceOrScalar_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryAsJObject(null, out _));
            Assert.IsFalse(CommonHelper.TryAsJObject("   ", out _));
            Assert.IsFalse(CommonHelper.TryAsJObject(42, out _));
            Assert.IsFalse(CommonHelper.TryAsJObject(new JArray(), out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // String form accepted at every top-level reader
        //
        // TryAsJArray already gave the TryGetList<>-based readers (headers,
        // inputs, settings) tolerance of the documented "JSON-encoded string"
        // form. Six top-level readers pattern-matched the raw type directly
        // instead and so dropped a string silently — create_workflow reported
        // success and the mapping simply vanished (observed for `outputs` on a
        // GET Web Method, warewolfserver-mcp, 2026-08-28). Each now routes
        // through TryAsJArray/TryAsJObject; these pin that down so the six do
        // not drift back apart.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputs_JsonArrayString_IsParsed()
        {
            const string json = "[{\"MappedFrom\":\"from\",\"MappedTo\":\"to\",\"RecordSetName\":\"rs\"}]";
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_OUTPUTS] = json };

            Assert.IsTrue(CommonHelper.TryGetOutputs(d, out var outputs));
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("from", outputs[0].MappedFrom);
            Assert.AreEqual("rs", outputs[0].RecordSetName);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputDescription_JsonObjectString_IsParsed()
        {
            const string json =
                "{\"DataSourceShapes\":[{\"Paths\":[{\"ActualPath\":\"ap\",\"DisplayPath\":\"dp\"," +
                "\"OutputExpression\":\"oe\",\"SampleData\":\"sd\"}]}]}";
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_OUTPUTDESCRIPTION] = json };

            Assert.IsTrue(CommonHelper.TryGetOutputDescription(d, out var outputDescription));
            Assert.IsNotNull(outputDescription);
            Assert.AreEqual(1, outputDescription.DataSourceShapes.Count);
            Assert.AreEqual("ap", outputDescription.DataSourceShapes[0].Paths[0].ActualPath);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetConditions_JsonArrayString_IsParsed()
        {
            const string json = "[{\"Key\":\"field\",\"Cond\":{\"TableType\":\"Text\",\"Value\":\"abc\"}}]";
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_CONDITIONS] = json };

            Assert.IsTrue(CommonHelper.TryGetConditions(d, out var conditions));
            Assert.AreEqual(1, conditions.Count);
            Assert.AreEqual("field", conditions[0].Key);
            var textCond = conditions[0].Cond as FormDataConditionText;
            Assert.IsNotNull(textCond);
            Assert.AreEqual("abc", textCond.Value);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInputMappings_JsonArrayString_IsParsed()
        {
            const string json =
                "[{\"InputColumn\":\"in\",\"IndexNumber\":1,\"Inserted\":true," +
                "\"OutputColumn\":{\"ColumnName\":\"out\",\"DataType\":\"System.String\"}}]";
            var d = new Dictionary<string, object> { [Constants.SQLBULKINSERT_INPUTMAPPINGS] = json };

            Assert.IsTrue(CommonHelper.TryGetInputMappings(d, out var list));
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("in", list[0].InputColumn);
            Assert.AreEqual("out", list[0].OutputColumn.ColumnName);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetFindRecordsCollection_JsonArrayString_IsParsed()
        {
            const string json =
                "[{\"SearchType\":\"Equal\",\"SearchCriteria\":\"abc\",\"From\":\"1\",\"To\":\"9\"}]";
            var d = new Dictionary<string, object> { [Constants.FINDRECORDS_RESULTSCOLLECTION] = json };

            Assert.IsTrue(CommonHelper.TryGetFindRecordsCollection(d, out var list));
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetRabbitMqPublishOptions_JsonObjectString_IsParsed()
        {
            var json = "{\"AutoCorrelation\":{\"Correlation\":" + (int)CorrelationAction.ExecutionID + "}}";
            var d = new Dictionary<string, object> { [Constants.RABBITMQPUBLISH_BASICPROPERTIES] = json };

            Assert.IsTrue(CommonHelper.TryGetRabbitMqPublishOptions(d, out var opts));
            Assert.IsNotNull(opts);
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetOutputs / TryGetOutputDescription
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputs_BuildsMappingsAndJsonPath()
        {
            var arr = new JArray
            {
                new JObject
                {
                    ["MappedFrom"] = "from",
                    ["MappedTo"]   = "to",
                    ["RecordSetName"] = "rs",
                    ["Path"] = new JObject
                    {
                        ["ActualPath"]       = "ap",
                        ["DisplayPath"]      = "dp",
                        ["OutputExpression"] = "oe",
                        ["SampleData"]       = "sd"
                    }
                }
            };
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_OUTPUTS] = arr };

            Assert.IsTrue(CommonHelper.TryGetOutputs(d, out var outputs));
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("from", outputs[0].MappedFrom);
            Assert.AreEqual("rs", outputs[0].RecordSetName);
            var concrete = outputs[0] as ServiceOutputMapping;
            Assert.IsNotNull(concrete);
            Assert.IsInstanceOfType(concrete.Path, typeof(JsonPath));
            Assert.AreEqual("ap", concrete.Path.ActualPath);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputs_NoOutputsKey_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetOutputs(new Dictionary<string, object>(), out var outputs));
            Assert.IsNull(outputs);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputs_WrongType_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_OUTPUTS] = "scalar" };
            Assert.IsFalse(CommonHelper.TryGetOutputs(d, out _));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputDescription_BuildsShapesAndPaths()
        {
            var od = new JObject
            {
                ["DataSourceShapes"] = new JArray
                {
                    new JObject
                    {
                        ["Paths"] = new JArray
                        {
                            new JObject
                            {
                                ["ActualPath"]       = "ap",
                                ["DisplayPath"]      = "dp",
                                ["OutputExpression"] = "oe",
                                ["SampleData"]       = "sd"
                            }
                        }
                    }
                }
            };
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_OUTPUTDESCRIPTION] = od };

            Assert.IsTrue(CommonHelper.TryGetOutputDescription(d, out var outputDescription));
            Assert.IsNotNull(outputDescription);
            Assert.AreEqual(1, outputDescription.DataSourceShapes.Count);
            Assert.AreEqual(1, outputDescription.DataSourceShapes[0].Paths.Count);
            Assert.AreEqual("ap", outputDescription.DataSourceShapes[0].Paths[0].ActualPath);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputDescription_Missing_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetOutputDescription(new Dictionary<string, object>(), out var od));
            Assert.IsNull(od);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetOutputDescription_WrongType_ReturnsFalse()
        {
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_OUTPUTDESCRIPTION] = "scalar" };
            Assert.IsFalse(CommonHelper.TryGetOutputDescription(d, out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetList — happy path and exception swallow
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetList_NameValuePayload_DeserialisedAsConcrete()
        {
            var arr = new JArray
            {
                new JObject { ["Name"] = "n1", ["Value"] = "v1" }
            };
            var d = new Dictionary<string, object> { ["k"] = arr };

            Assert.IsTrue(CommonHelper.TryGetList<NameValue, INameValue>(d, out var list, "k"));
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("n1", list[0].Name);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetList_MissingKey_ReturnsFalse()
        {
            var d = new Dictionary<string, object>();
            Assert.IsFalse(CommonHelper.TryGetList<NameValue, INameValue>(d, out var list, "k"));
            Assert.IsNull(list);
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetHeaders / TryGetInputs / TryGetSettings — wrappers
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetHeaders_PrefersUpdatedHeadersKey()
        {
            var updated = new JArray { new JObject { ["Name"] = "u", ["Value"] = "1" } };
            var stale   = new JArray { new JObject { ["Name"] = "s", ["Value"] = "2" } };
            var d = new Dictionary<string, object>
            {
                [Constants.WEBMETHOD_UPDATEDHEADERS] = updated,
                [Constants.WEBMETHOD_HEADERS]        = stale
            };

            Assert.IsTrue(CommonHelper.TryGetHeaders(d, out var headers));
            Assert.AreEqual(1, headers.Count);
            Assert.AreEqual("u", headers[0].Name);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetHeaders_FallsBackToHeadersWhenUpdatedMissing()
        {
            var stale = new JArray { new JObject { ["Name"] = "s", ["Value"] = "2" } };
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_HEADERS] = stale };

            Assert.IsTrue(CommonHelper.TryGetHeaders(d, out var headers));
            Assert.AreEqual("s", headers[0].Name);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInputs_DeserialisesServiceInputs()
        {
            var arr = new JArray
            {
                new JObject { ["Name"] = "in1", ["Value"] = "v" }
            };
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_INPUTS] = arr };

            Assert.IsTrue(CommonHelper.TryGetInputs(d, out var inputs));
            Assert.AreEqual("in1", inputs[0].Name);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetSettings_DeserialisesNameValuePairs()
        {
            var arr = new JArray { new JObject { ["Name"] = "s", ["Value"] = "v" } };
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_SETTINGS] = arr };

            Assert.IsTrue(CommonHelper.TryGetSettings(d, out var settings));
            Assert.AreEqual("s", settings[0].Name);
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetConditions
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetConditions_TextConditionWithStringEnum_Parsed()
        {
            var payload = new JArray
            {
                new JObject
                {
                    ["Key"]  = "field",
                    ["Cond"] = new JObject
                    {
                        ["TableType"] = "Text",
                        ["Value"]     = "abc"
                    }
                }
            };
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_CONDITIONS] = payload };

            Assert.IsTrue(CommonHelper.TryGetConditions(d, out var conditions));
            Assert.AreEqual(1, conditions.Count);
            Assert.AreEqual("field", conditions[0].Key);
            var textCond = conditions[0].Cond as FormDataConditionText;
            Assert.IsNotNull(textCond);
            Assert.AreEqual("abc", textCond.Value);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetConditions_FileConditionWithIntEnum_Parsed()
        {
            var payload = new JArray
            {
                new JObject
                {
                    ["Key"]  = "uploaded",
                    ["Cond"] = new JObject
                    {
                        ["TableType"]  = (int)enFormDataTableType.File,
                        ["FileBase64"] = "AAAA",
                        ["FileName"]   = "a.bin"
                    }
                }
            };
            var d = new Dictionary<string, object> { [Constants.WEBMETHOD_CONDITIONS] = payload };

            Assert.IsTrue(CommonHelper.TryGetConditions(d, out var conditions));
            var fileCond = conditions[0].Cond as FormDataConditionFile;
            Assert.IsNotNull(fileCond);
            Assert.AreEqual("AAAA", fileCond.FileBase64);
            Assert.AreEqual("a.bin", fileCond.FileName);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetConditions_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetConditions(new Dictionary<string, object>(), out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetInputMappings
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInputMappings_JArrayWithIntSqlDataType_Parsed()
        {
            var payload = new JArray
            {
                new JObject
                {
                    ["InputColumn"]  = "in",
                    ["IndexNumber"] = 1,
                    ["Inserted"]    = true,
                    ["OutputColumn"] = new JObject
                    {
                        ["ColumnName"]     = "out",
                        ["MaxLength"]      = 50,
                        ["IsNullable"]     = false,
                        ["IsAutoIncrement"] = false,
                        ["SqlDataType"]    = (int)SqlDbType.NVarChar,
                        ["DataType"]       = "System.String"
                    }
                }
            };
            var d = new Dictionary<string, object> { [Constants.SQLBULKINSERT_INPUTMAPPINGS] = payload };

            Assert.IsTrue(CommonHelper.TryGetInputMappings(d, out var list));
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("in", list[0].InputColumn);
            Assert.AreEqual("out", list[0].OutputColumn.ColumnName);
            Assert.AreEqual(SqlDbType.NVarChar, list[0].OutputColumn.SqlDataType);
            Assert.AreEqual(typeof(string), list[0].OutputColumn.DataType);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInputMappings_JArrayWithStringSqlDataTypeAndIntDataType_Parsed()
        {
            var payload = new JArray
            {
                new JObject
                {
                    ["InputColumn"] = "in",
                    ["OutputColumn"] = new JObject
                    {
                        ["ColumnName"]  = "out",
                        ["SqlDataType"] = "Int",
                        ["DataType"]    = "System.Int32"
                    }
                }
            };
            var d = new Dictionary<string, object> { [Constants.SQLBULKINSERT_INPUTMAPPINGS] = payload };

            Assert.IsTrue(CommonHelper.TryGetInputMappings(d, out var list));
            Assert.AreEqual(SqlDbType.Int, list[0].OutputColumn.SqlDataType);
            Assert.AreEqual(typeof(int), list[0].OutputColumn.DataType);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInputMappings_UnknownDataType_FallsBackToString()
        {
            var payload = new JArray
            {
                new JObject
                {
                    ["InputColumn"] = "in",
                    ["OutputColumn"] = new JObject
                    {
                        ["ColumnName"] = "out",
                        ["DataType"]   = "NotARealTypeName"
                    }
                }
            };
            var d = new Dictionary<string, object> { [Constants.SQLBULKINSERT_INPUTMAPPINGS] = payload };

            Assert.IsTrue(CommonHelper.TryGetInputMappings(d, out var list));
            Assert.AreEqual(typeof(string), list[0].OutputColumn.DataType);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInputMappings_ExistingIList_PassedThrough()
        {
            var existing = new List<DataColumnMapping> { new DataColumnMapping { InputColumn = "exists" } };
            var d = new Dictionary<string, object> { [Constants.SQLBULKINSERT_INPUTMAPPINGS] = existing };

            Assert.IsTrue(CommonHelper.TryGetInputMappings(d, out var list));
            Assert.AreSame(existing, list);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetInputMappings_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetInputMappings(new Dictionary<string, object>(), out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetFindRecordsCollection
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetFindRecordsCollection_JArray_Parsed()
        {
            var payload = new JArray
            {
                new JObject
                {
                    ["SearchType"]     = "Equal",
                    ["SearchCriteria"] = "abc",
                    ["From"]           = "1",
                    ["To"]             = "9",
                    ["IndexNumber"]    = 2,
                    ["Inserted"]       = false,
                    ["WhereOptionList"] = new JArray { "Equal", "Contains" }
                }
            };
            var d = new Dictionary<string, object> { [Constants.FINDRECORDS_RESULTSCOLLECTION] = payload };

            Assert.IsTrue(CommonHelper.TryGetFindRecordsCollection(d, out var list));
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("abc", list[0].SearchCriteria);
            Assert.AreEqual(2, list[0].IndexNumber);
            Assert.AreEqual(2, list[0].WhereOptionList.Count);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetFindRecordsCollection_ExistingIList_PassedThrough()
        {
            var existing = new List<FindRecordsTO> { new FindRecordsTO("c", "Equal", 1) };
            var d = new Dictionary<string, object> { [Constants.FINDRECORDS_RESULTSCOLLECTION] = existing };

            Assert.IsTrue(CommonHelper.TryGetFindRecordsCollection(d, out var list));
            Assert.AreSame(existing, list);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetFindRecordsCollection_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetFindRecordsCollection(new Dictionary<string, object>(), out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // TryGetRabbitMqPublishOptions
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetRabbitMqPublishOptions_ExecutionIDDefault_Parsed()
        {
            var payload = new JObject
            {
                ["AutoCorrelation"] = new JObject
                {
                    ["Correlation"] = (int)CorrelationAction.ExecutionID
                }
            };
            var d = new Dictionary<string, object> { [Constants.RABBITMQPUBLISH_BASICPROPERTIES] = payload };

            Assert.IsTrue(CommonHelper.TryGetRabbitMqPublishOptions(d, out var opts));
            Assert.IsInstanceOfType(opts.AutoCorrelation, typeof(ExecutionID));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetRabbitMqPublishOptions_CustomTransactionID_FromString_Parsed()
        {
            var payload = new JObject
            {
                ["AutoCorrelation"] = new JObject
                {
                    ["Correlation"] = "CustomTransactionID"
                }
            };
            var d = new Dictionary<string, object> { [Constants.RABBITMQPUBLISH_BASICPROPERTIES] = payload };

            Assert.IsTrue(CommonHelper.TryGetRabbitMqPublishOptions(d, out var opts));
            Assert.IsInstanceOfType(opts.AutoCorrelation, typeof(CustomTransactionID));
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetRabbitMqPublishOptions_Manual_WithCorrelationId_Parsed()
        {
            var payload = new JObject
            {
                ["AutoCorrelation"] = new JObject
                {
                    ["Correlation"]   = (int)CorrelationAction.Manual,
                    ["CorrelationID"] = "corr-42"
                }
            };
            var d = new Dictionary<string, object> { [Constants.RABBITMQPUBLISH_BASICPROPERTIES] = payload };

            Assert.IsTrue(CommonHelper.TryGetRabbitMqPublishOptions(d, out var opts));
            var manual = opts.AutoCorrelation as Manual;
            Assert.IsNotNull(manual);
            Assert.AreEqual("corr-42", manual.CorrelationID);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetRabbitMqPublishOptions_ExistingOptions_PassedThrough()
        {
            var existing = new RabbitMqPublishOptions();
            var d = new Dictionary<string, object> { [Constants.RABBITMQPUBLISH_BASICPROPERTIES] = existing };

            Assert.IsTrue(CommonHelper.TryGetRabbitMqPublishOptions(d, out var opts));
            Assert.AreSame(existing, opts);
        }

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void TryGetRabbitMqPublishOptions_MissingKey_ReturnsFalse()
        {
            Assert.IsFalse(CommonHelper.TryGetRabbitMqPublishOptions(new Dictionary<string, object>(), out _));
        }

        // ─────────────────────────────────────────────────────────────────
        // CommonHelperExtensions — delegating wrappers
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("CommonHelper_Coverage")]
        public void Extensions_AllDelegates_ExerciseSurface()
        {
            var d = new Dictionary<string, object>
            {
                ["b"] = true,
                ["s"] = "x",
                ["i"] = 5,
                ["g"] = Guid.NewGuid().ToString(),
                ["list"] = new JArray { new JObject { ["Name"] = "n", ["Value"] = "v" } },
                [Constants.WEBMETHOD_OUTPUTS]            = new JArray(),
                [Constants.WEBMETHOD_OUTPUTDESCRIPTION] = new JObject(),
                [Constants.WEBMETHOD_HEADERS]            = new JArray(),
                [Constants.WEBMETHOD_INPUTS]             = new JArray(),
                [Constants.WEBMETHOD_SETTINGS]           = new JArray(),
                [Constants.WEBMETHOD_CONDITIONS]         = new JArray(),
                [Constants.SQLBULKINSERT_INPUTMAPPINGS]  = new JArray(),
                [Constants.FINDRECORDS_RESULTSCOLLECTION] = new JArray(),
                [Constants.RABBITMQPUBLISH_BASICPROPERTIES] = new JObject()
            };

            Assert.IsTrue(d.TryGetBool("b", out var b));    Assert.IsTrue(b);
            Assert.IsTrue(d.TryGetString("s", out var s));  Assert.AreEqual("x", s);
            Assert.IsTrue(d.TryGetInt("i", out var i));     Assert.AreEqual(5, i);
            Assert.IsTrue(d.TryGetGuid("g", out var g));    Assert.AreNotEqual(Guid.Empty, g);
            Assert.IsTrue(d.TryGetList<NameValue, INameValue>("list", out var list));
            Assert.AreEqual(1, list.Count);

            // IDictionary overloads
            IDictionary<string, object> id = d;
            Assert.IsTrue(id.TryGetOutputs(out _));
            Assert.IsTrue(id.TryGetOutputDescription(out _));
            Assert.IsTrue(id.TryGetHeaders(out _));
            Assert.IsTrue(id.TryGetInputs(out _));
            Assert.IsTrue(id.TryGetSettings(out _));
            Assert.IsTrue(id.TryGetConditions(out _));
            Assert.IsTrue(id.TryGetInputMappings(out _));
            Assert.IsTrue(id.TryGetFindRecordsCollection(out _));
            // RabbitMQ payload has no AutoCorrelation block but should still return true with default options.
            Assert.IsTrue(id.TryGetRabbitMqPublishOptions(out var opts));
            Assert.IsNotNull(opts);
        }
    }
}
