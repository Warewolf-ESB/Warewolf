/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for EnvelopeBiteWriter's <DataList> construction — specifically that a recordset
 *  declaration carries the attributes ExecutionEnvironmentUtils needs to emit it as an output.
 */

using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Tests.Mcp
{
    [TestClass]
    public class EnvelopeBiteWriterTests
    {
        static JsonElement Envelope(object envelope) => JsonSerializer.SerializeToElement(envelope);

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildDataListElement_RecordsetElement_CarriesColumnIoDirection()
        {
            // Without a direction ON THE ELEMENT, GetJsonForEnvironmentWithColumnIoDirection skips
            // the whole recordset, so every execution silently returned no recordset outputs.
            var dataList = EnvelopeBiteWriter.BuildDataListElement(Envelope(new
            {
                inputs = new object[0],
                outputs = new[]
                {
                    new { kind = "recordset", name = "results", fields = new[] { "input", "match" } }
                }
            }));

            var results = dataList.Element("results");
            Assert.IsNotNull(results, "the recordset must be declared");
            Assert.AreEqual("Output", results!.Attribute("ColumnIODirection")?.Value);
            Assert.IsNotNull(results.Attribute("Description"));
            Assert.IsNotNull(results.Attribute("IsEditable"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildDataListElement_RecordsetFields_EachCarryTheirOwnDirection()
        {
            var dataList = EnvelopeBiteWriter.BuildDataListElement(Envelope(new
            {
                inputs = new object[0],
                outputs = new[]
                {
                    new { kind = "recordset", name = "results", fields = new[] { "input", "match" } }
                }
            }));

            var fields = dataList.Element("results")!.Elements().ToList();
            Assert.AreEqual(2, fields.Count);
            CollectionAssert.AreEquivalent(
                new[] { "input", "match" },
                fields.Select(f => f.Name.LocalName).ToArray());
            Assert.IsTrue(fields.All(f => f.Attribute("ColumnIODirection")?.Value == "Output"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildDataListElement_RecordsetInBothArrays_IsBothAtElementLevel()
        {
            var dataList = EnvelopeBiteWriter.BuildDataListElement(Envelope(new
            {
                inputs = new[]
                {
                    new { kind = "recordset", name = "items", fields = new[] { "value" } }
                },
                outputs = new[]
                {
                    new { kind = "recordset", name = "items", fields = new[] { "value" } }
                }
            }));

            var items = dataList.Element("items");
            Assert.AreEqual("Both", items?.Attribute("ColumnIODirection")?.Value);
            Assert.AreEqual("Both", items?.Element("value")?.Attribute("ColumnIODirection")?.Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildDataListElement_RecordsetSplitAcrossArrays_IsBothAtElementLevelAndPerFieldBelow()
        {
            // A recordset read in one field and written in another: the element is "Both" so the
            // entry is considered for either direction, while the fields stay one-directional.
            var dataList = EnvelopeBiteWriter.BuildDataListElement(Envelope(new
            {
                inputs = new[]
                {
                    new { kind = "recordset", name = "rows", fields = new[] { "given" } }
                },
                outputs = new[]
                {
                    new { kind = "recordset", name = "rows", fields = new[] { "computed" } }
                }
            }));

            var rows = dataList.Element("rows");
            Assert.AreEqual("Both", rows?.Attribute("ColumnIODirection")?.Value);
            Assert.AreEqual("Input", rows?.Element("given")?.Attribute("ColumnIODirection")?.Value);
            Assert.AreEqual("Output", rows?.Element("computed")?.Attribute("ColumnIODirection")?.Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildRecordsetVariableElement_CarriesColumnIoDirection()
        {
            // add_step's auto-declared recordset needs the same element-level direction.
            var element = EnvelopeBiteWriter.BuildRecordsetVariableElement("rows", "v");

            Assert.AreEqual("Both", element.Attribute("ColumnIODirection")?.Value);
            Assert.AreEqual("Both", element.Element("v")?.Attribute("ColumnIODirection")?.Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildDataListElement_ScalarElement_IsUnchanged()
        {
            var dataList = EnvelopeBiteWriter.BuildDataListElement(Envelope(new
            {
                inputs = new[] { new { kind = "scalar", name = "text", fields = new string[0] } },
                outputs = new object[0]
            }));

            var text = dataList.Element("text");
            Assert.AreEqual("Input", text?.Attribute("ColumnIODirection")?.Value);
            Assert.IsFalse(text!.HasElements, "a scalar must not gain field children");
        }
    }
}
