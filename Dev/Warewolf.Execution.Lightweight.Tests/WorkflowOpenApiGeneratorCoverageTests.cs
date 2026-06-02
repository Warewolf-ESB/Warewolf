/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Coverage uplift tests for <see cref="WorkflowOpenApiGenerator"/>.
 *
 *  Reaches the internal static class via InternalsVisibleTo. Exercises:
 *
 *    * Scalar inputs/outputs   → type:string in parameters / responses
 *    * IsJson="true" variables → type:object (Input / Output / Both directions)
 *    * Recordset inputs/outputs → object schema with per-field string properties
 *    * Malformed and missing inputs   → catch paths return safe shells
 *    * URL handling: .api extension stripped, query string preserved in path key,
 *      base URL composed from scheme + host
 *
 *  No host required: tests build temp workflow XML files and call Generate()
 *  directly, then assert the resulting OpenAPI JObject structure.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    [TestCategory("WorkflowOpenApiGenerator_Coverage")]
    public class WorkflowOpenApiGeneratorCoverageTests
    {
        private readonly List<string> _tempFiles = new();

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var f in _tempFiles)
            {
                try { if (File.Exists(f)) File.Delete(f); } catch { }
            }
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private string WriteWorkflow(string dataListInner)
        {
            var path = Path.Combine(Path.GetTempPath(), "wf_oag_" + Guid.NewGuid().ToString("N") + ".xml");
            File.WriteAllText(path, $"<Workflow><DataList>{dataListInner}</DataList></Workflow>");
            _tempFiles.Add(path);
            return path;
        }

        private static JObject InvokeGenerate(string path, string name, string url) =>
            JObject.Parse(WorkflowOpenApiGenerator.Generate(path, name, new Uri(url)));

        private static JArray Parameters(JObject spec, string path) =>
            (JArray)spec["paths"]![path]!["get"]!["parameters"]!;

        private static JObject ResponseProperties(JObject spec, string path) =>
            (JObject)spec["paths"]![path]!["get"]!["responses"]!["200"]!
                ["content"]!["application/json"]!["schema"]!["properties"]!;

        // ════════════════════════════════════════════════════════════════════
        // URL / spec shell
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Generate_StaticSpecFields_AreSet()
        {
            var wf = WriteWorkflow("");
            var spec = InvokeGenerate(wf, "Hello", "http://srv:9090/Public/Hello.api");

            Assert.AreEqual("3.0.1", (string?)spec["openapi"]);
            Assert.AreEqual("1", (string?)spec["info"]!["version"]);
            // baseUrl is scheme + host (port intentionally omitted by the generator).
            Assert.AreEqual("http://srv", (string?)((JArray)spec["servers"]!)[0]!["url"]);
            // Title/description use the full request URI.
            Assert.AreEqual("http://srv:9090/Public/Hello.api", (string?)spec["info"]!["title"]);
            Assert.AreEqual("http://srv:9090/Public/Hello.api", (string?)spec["info"]!["description"]);
        }

        [TestMethod]
        public void Generate_DotApiExtension_StrippedFromPathKey()
        {
            var wf = WriteWorkflow("");
            var spec = InvokeGenerate(wf, "Hello", "http://srv/Public/Hello.api");

            Assert.IsNotNull(spec["paths"]!["/Public/Hello"]);
        }

        [TestMethod]
        public void Generate_QueryString_PreservedInPathKey()
        {
            var wf = WriteWorkflow("");
            var spec = InvokeGenerate(wf, "Hi", "http://srv/Public/Hi.api?foo=1&bar=2");

            Assert.IsNotNull(spec["paths"]!["/Public/Hi?foo=1&bar=2"]);
        }

        [TestMethod]
        public void Generate_UpperCaseDotApi_AlsoStripped()
        {
            var wf = WriteWorkflow("");
            var spec = InvokeGenerate(wf, "Hi", "http://srv/Public/Hi.API");

            Assert.IsNotNull(spec["paths"]!["/Public/Hi"]);
        }

        // ════════════════════════════════════════════════════════════════════
        // Failure / catch paths
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Generate_NonExistentFile_StillProducesValidSpec()
        {
            var missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");
            var spec = InvokeGenerate(missing, "Missing", "http://srv/Public/Missing.api");

            // No DataList → empty parameters and empty response properties.
            var pathKey = "/Public/Missing";
            Assert.AreEqual(0, Parameters(spec, pathKey).Count);
            Assert.AreEqual(0, ResponseProperties(spec, pathKey).Count);
        }

        [TestMethod]
        public void Generate_MalformedXml_StillProducesValidSpec()
        {
            var path = Path.Combine(Path.GetTempPath(), "wf_oag_bad_" + Guid.NewGuid().ToString("N") + ".xml");
            File.WriteAllText(path, "<not closed");
            _tempFiles.Add(path);

            var spec = InvokeGenerate(path, "Bad", "http://srv/Public/Bad.api");

            Assert.AreEqual(0, Parameters(spec, "/Public/Bad").Count);
        }

        [TestMethod]
        public void Generate_FileWithoutDataListElement_FallsBackToEmpty()
        {
            var path = Path.Combine(Path.GetTempPath(), "wf_oag_nod_" + Guid.NewGuid().ToString("N") + ".xml");
            File.WriteAllText(path, "<Workflow><Other/></Workflow>");
            _tempFiles.Add(path);

            var spec = InvokeGenerate(path, "Nod", "http://srv/Public/Nod.api");

            Assert.AreEqual(0, Parameters(spec, "/Public/Nod").Count);
        }

        // ════════════════════════════════════════════════════════════════════
        // Scalar variables → type:string
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Generate_ScalarInput_AppearsAsStringQueryParam()
        {
            var wf = WriteWorkflow(@"<Name ColumnIODirection=""Input""></Name>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("Name", (string?)p[0]!["name"]);
            Assert.AreEqual("query", (string?)p[0]!["in"]);
            Assert.AreEqual(true, (bool?)p[0]!["required"]);
            Assert.AreEqual("string", (string?)p[0]!["schema"]!["type"]);
        }

        [TestMethod]
        public void Generate_ScalarOutput_AppearsAsStringResponseProperty()
        {
            var wf = WriteWorkflow(@"<Greeting ColumnIODirection=""Output""></Greeting>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var props = ResponseProperties(spec, "/Public/Wf");
            Assert.AreEqual("string", (string?)props["Greeting"]!["type"]);
        }

        [TestMethod]
        public void Generate_ScalarBoth_AppearsInBothInputAndOutput()
        {
            var wf = WriteWorkflow(@"<Both ColumnIODirection=""Both""></Both>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("Both", (string?)p[0]!["name"]);

            var props = ResponseProperties(spec, "/Public/Wf");
            Assert.AreEqual("string", (string?)props["Both"]!["type"]);
        }

        // ════════════════════════════════════════════════════════════════════
        // IsJson="true" variables → type:object
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Generate_JsonObjectInput_TypedAsObjectInParameters()
        {
            var wf = WriteWorkflow(@"<Person ColumnIODirection=""Input"" IsJson=""true""></Person>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("object", (string?)p[0]!["schema"]!["type"]);
        }

        [TestMethod]
        public void Generate_JsonObjectOutput_TypedAsObjectInResponses()
        {
            var wf = WriteWorkflow(@"<Result ColumnIODirection=""Output"" IsJson=""true""></Result>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var props = ResponseProperties(spec, "/Public/Wf");
            Assert.AreEqual("object", (string?)props["Result"]!["type"]);
        }

        [TestMethod]
        public void Generate_JsonObjectBoth_TypedAsObjectBothSides()
        {
            var wf = WriteWorkflow(@"<Payload ColumnIODirection=""Both"" IsJson=""true""></Payload>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual("object", (string?)p[0]!["schema"]!["type"]);
            var props = ResponseProperties(spec, "/Public/Wf");
            Assert.AreEqual("object", (string?)props["Payload"]!["type"]);
        }

        [TestMethod]
        public void Generate_IsJsonAttribute_IsCaseInsensitive()
        {
            // IsJson="True" (capital T) should still classify as object.
            var wf = WriteWorkflow(@"<P ColumnIODirection=""Input"" IsJson=""True""></P>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual("object", (string?)p[0]!["schema"]!["type"]);
        }

        [TestMethod]
        public void Generate_IsJsonFalse_StillTypedAsString()
        {
            var wf = WriteWorkflow(@"<P ColumnIODirection=""Input"" IsJson=""false""></P>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual("string", (string?)p[0]!["schema"]!["type"]);
        }

        // ════════════════════════════════════════════════════════════════════
        // Recordsets → object schema with per-field string properties
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Generate_RecordsetInput_HasFieldPropertiesAsStrings()
        {
            var wf = WriteWorkflow(@"
                <Cars ColumnIODirection=""Input"">
                    <Make ColumnIODirection=""Input""></Make>
                    <Model ColumnIODirection=""Input""></Model>
                </Cars>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("Cars", (string?)p[0]!["name"]);
            var schema = (JObject)p[0]!["schema"]!;
            Assert.AreEqual("object", (string?)schema["type"]);

            var fieldProps = (JObject)schema["properties"]!;
            Assert.AreEqual("string", (string?)fieldProps["Make"]!["type"]);
            Assert.AreEqual("string", (string?)fieldProps["Model"]!["type"]);
        }

        [TestMethod]
        public void Generate_RecordsetOutput_HasFieldPropertiesAsStrings()
        {
            var wf = WriteWorkflow(@"
                <Animals ColumnIODirection=""Output"">
                    <Name ColumnIODirection=""Output""></Name>
                    <Sound ColumnIODirection=""Output""></Sound>
                </Animals>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var props = ResponseProperties(spec, "/Public/Wf");
            var animals = (JObject)props["Animals"]!;
            Assert.AreEqual("object", (string?)animals["type"]);
            Assert.AreEqual("string", (string?)animals["properties"]!["Name"]!["type"]);
            Assert.AreEqual("string", (string?)animals["properties"]!["Sound"]!["type"]);
        }

        [TestMethod]
        public void Generate_RecordsetBoth_AppearsAsInputAndOutput()
        {
            var wf = WriteWorkflow(@"
                <Items ColumnIODirection=""Both"">
                    <Sku ColumnIODirection=""Both""></Sku>
                </Items>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual(1, p.Count);
            Assert.AreEqual("Items", (string?)p[0]!["name"]);

            var props = ResponseProperties(spec, "/Public/Wf");
            Assert.IsNotNull(props["Items"]);
            Assert.AreEqual("string", (string?)props["Items"]!["properties"]!["Sku"]!["type"]);
        }

        // ════════════════════════════════════════════════════════════════════
        // Mixed everything (scalar + json + recordset together)
        // ════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Generate_MixedScalarJsonRecordsetInputs_AllPresent()
        {
            var wf = WriteWorkflow(@"
                <FirstName ColumnIODirection=""Input""></FirstName>
                <Address ColumnIODirection=""Input"" IsJson=""true""></Address>
                <Tags ColumnIODirection=""Input"">
                    <Label ColumnIODirection=""Input""></Label>
                </Tags>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var p = Parameters(spec, "/Public/Wf");
            Assert.AreEqual(3, p.Count, "Expected 3 inputs: FirstName, Address, Tags");

            // Collect by name → schema type.
            var byName = new Dictionary<string, string?>();
            foreach (var item in p)
                byName[(string)item!["name"]!] = (string?)item!["schema"]!["type"];

            Assert.AreEqual("string", byName["FirstName"]);
            Assert.AreEqual("object", byName["Address"]);
            Assert.AreEqual("object", byName["Tags"]);
        }

        [TestMethod]
        public void Generate_ResponseEnvelope_HasStandardShape()
        {
            var wf = WriteWorkflow(@"<X ColumnIODirection=""Output""></X>");
            var spec = InvokeGenerate(wf, "Wf", "http://srv/Public/Wf.api");

            var resp200 = spec["paths"]!["/Public/Wf"]!["get"]!["responses"]!["200"]!;
            Assert.AreEqual("Success", (string?)resp200["description"]);
            var schema = resp200["content"]!["application/json"]!["schema"]!;
            Assert.AreEqual("object", (string?)schema["type"]);
            Assert.IsNotNull(schema["properties"]);
        }
    }
}
