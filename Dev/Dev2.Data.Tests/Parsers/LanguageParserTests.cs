/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Parsers
{
    [TestClass]
    public class LanguageParserTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_Empty_MappingDefinition()
        {
            const string elementTag = "";
            const string mapsTo = "";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            var mappingDefinition = string.Empty;

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_MappingDefinition()
        {
            const string elementTag = "";
            const string mapsTo = "";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<DataList></DataList>";

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_MapsTo_False_ElementTag_MappingDefinition()
        {
            const string elementTag = "Input";
            const string mapsTo = "Source";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Inputs><Input Name=""Name"" Source="""" IsObject=""False"" /></Inputs>";

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_MapsTo_True_ElementTag_MappingDefinition()
        {
            const string elementTag = "Output";
            const string mapsTo = "MapsTo";
            const bool defaultValueToMapsTo = true;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Outputs><Output Name=""Message"" MapsTo="""" Value="""" IsObject=""False"" /></Outputs>";

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0].DefaultValue);
            Assert.IsFalse(result[0].EmptyToNull);
            Assert.IsFalse(result[0].IsEvaluated);
            Assert.IsFalse(result[0].IsJsonArray);
            Assert.IsFalse(result[0].IsObject);
            Assert.IsFalse(result[0].IsRecordSet);
            Assert.IsFalse(result[0].IsRequired);
            Assert.IsFalse(result[0].IsTextResponse);
            Assert.AreEqual("", result[0].MapsTo);
            Assert.AreEqual("Message", result[0].Name);
            Assert.AreEqual("", result[0].RawValue);
            Assert.AreEqual("", result[0].RecordSetName);
            Assert.AreEqual("", result[0].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_MapsTo_Contains_Brackets()
        {
            const string elementTag = "Output";
            const string mapsTo = "MapsTo";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Outputs><Output Name=""[[Message]]"" MapsTo="""" Value=""world"" IsObject=""False"" /></Outputs>";

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0].DefaultValue);
            Assert.IsFalse(result[0].EmptyToNull);
            Assert.IsTrue(result[0].IsEvaluated);
            Assert.IsFalse(result[0].IsJsonArray);
            Assert.IsFalse(result[0].IsObject);
            Assert.IsFalse(result[0].IsRecordSet);
            Assert.IsFalse(result[0].IsRequired);
            Assert.IsFalse(result[0].IsTextResponse);
            Assert.AreEqual("Message", result[0].MapsTo);
            Assert.AreEqual("[[Message]]", result[0].Name);
            Assert.AreEqual("world", result[0].RawValue);
            Assert.AreEqual("", result[0].RecordSetName);
            Assert.AreEqual("world", result[0].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_Value_Contains_Brackets()
        {
            const string elementTag = "Output";
            const string mapsTo = "MapsTo";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Outputs><Output Name=""[[Message]]"" MapsTo="""" Value=""[[world]]"" IsObject=""False"" /></Outputs>";

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0].DefaultValue);
            Assert.IsFalse(result[0].EmptyToNull);
            Assert.IsTrue(result[0].IsEvaluated);
            Assert.IsFalse(result[0].IsJsonArray);
            Assert.IsFalse(result[0].IsObject);
            Assert.IsFalse(result[0].IsRecordSet);
            Assert.IsFalse(result[0].IsRequired);
            Assert.IsFalse(result[0].IsTextResponse);
            Assert.AreEqual("Message", result[0].MapsTo);
            Assert.AreEqual("[[Message]]", result[0].Name);
            Assert.AreEqual("[[world]]", result[0].RawValue);
            Assert.AreEqual("", result[0].RecordSetName);
            Assert.AreEqual("world", result[0].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_Value_Contains_Brackets_DefaultValueToMapsTo_True()
        {
            const string elementTag = "Output";
            const string mapsTo = "MapsTo";
            const bool defaultValueToMapsTo = true;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Outputs><Output Name=""[[Message]]"" MapsTo=""[[NewMessage]]"" Value=""[[world]]"" IsObject=""False"" /></Outputs>";

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("", result[0].DefaultValue);
            Assert.IsFalse(result[0].EmptyToNull);
            Assert.IsTrue(result[0].IsEvaluated);
            Assert.IsFalse(result[0].IsJsonArray);
            Assert.IsFalse(result[0].IsObject);
            Assert.IsFalse(result[0].IsRecordSet);
            Assert.IsFalse(result[0].IsRequired);
            Assert.IsFalse(result[0].IsTextResponse);
            Assert.AreEqual("NewMessage", result[0].MapsTo);
            Assert.AreEqual("[[Message]]", result[0].Name);
            Assert.AreEqual("[[NewMessage]]", result[0].RawValue);
            Assert.AreEqual("", result[0].RecordSetName);
            Assert.AreEqual("NewMessage", result[0].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_Recordset_DefaultValueMapsTo_True()
        {
            const string elementTag = "Input";
            const string mapsTo = "Source";
            const bool defaultValueToMapsTo = true;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Inputs><Input Name=""a"" Source="""" IsObject=""False"" Recordset=""rec"" /><Input Name=""b"" Source="""" IsObject=""False"" Recordset=""rec"" /></Inputs>";

            var result = languageParser.Parse(mappingDefinition);
            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("", result[0].DefaultValue);
            Assert.IsFalse(result[0].EmptyToNull);
            Assert.IsFalse(result[0].IsEvaluated);
            Assert.IsFalse(result[0].IsJsonArray);
            Assert.IsFalse(result[0].IsObject);
            Assert.IsTrue(result[0].IsRecordSet);
            Assert.IsFalse(result[0].IsRequired);
            Assert.IsFalse(result[0].IsTextResponse);
            Assert.AreEqual("", result[0].MapsTo);
            Assert.AreEqual("a", result[0].Name);
            Assert.AreEqual("", result[0].RawValue);
            Assert.AreEqual("rec", result[0].RecordSetName);
            Assert.AreEqual("", result[0].Value);

            Assert.AreEqual("", result[1].DefaultValue);
            Assert.IsFalse(result[1].EmptyToNull);
            Assert.IsFalse(result[1].IsEvaluated);
            Assert.IsFalse(result[1].IsJsonArray);
            Assert.IsFalse(result[1].IsObject);
            Assert.IsTrue(result[1].IsRecordSet);
            Assert.IsFalse(result[1].IsRequired);
            Assert.IsFalse(result[1].IsTextResponse);
            Assert.AreEqual("", result[1].MapsTo);
            Assert.AreEqual("b", result[1].Name);
            Assert.AreEqual("", result[1].RawValue);
            Assert.AreEqual("rec", result[1].RecordSetName);
            Assert.AreEqual("", result[1].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_Recordset_DefaultValueMapsTo_False()
        {
            const string elementTag = "Output";
            const string mapsTo = "MapsTo";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Outputs><Output Name=""a"" MapsTo="""" Value=""1"" IsObject=""False"" Recordset=""rec"" /><Output Name=""b"" MapsTo="""" Value=""2"" IsObject=""False"" Recordset=""rec"" /></Outputs>";

            var result = languageParser.Parse(mappingDefinition);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("", result[0].DefaultValue);
            Assert.IsFalse(result[0].EmptyToNull);
            Assert.IsFalse(result[0].IsEvaluated);
            Assert.IsFalse(result[0].IsJsonArray);
            Assert.IsFalse(result[0].IsObject);
            Assert.IsTrue(result[0].IsRecordSet);
            Assert.IsFalse(result[0].IsRequired);
            Assert.IsFalse(result[0].IsTextResponse);
            Assert.AreEqual("a", result[0].MapsTo);
            Assert.AreEqual("a", result[0].Name);
            Assert.AreEqual("1", result[0].RawValue);
            Assert.AreEqual("rec", result[0].RecordSetName);
            Assert.AreEqual("1", result[0].Value);

            Assert.AreEqual("", result[1].DefaultValue);
            Assert.IsFalse(result[1].EmptyToNull);
            Assert.IsFalse(result[1].IsEvaluated);
            Assert.IsFalse(result[1].IsJsonArray);
            Assert.IsFalse(result[1].IsObject);
            Assert.IsTrue(result[1].IsRecordSet);
            Assert.IsFalse(result[1].IsRequired);
            Assert.IsFalse(result[1].IsTextResponse);
            Assert.AreEqual("b", result[1].MapsTo);
            Assert.AreEqual("b", result[1].Name);
            Assert.AreEqual("2", result[1].RawValue);
            Assert.AreEqual("rec", result[1].RecordSetName);
            Assert.AreEqual("2", result[1].Value);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_Input_ChildNodes_IsRequired_True()
        {
            const string elementTag = "Input";
            const string mapsTo = "Source";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Inputs><Input Name=""a"" Source="""" IsObject=""False"" Recordset=""rec""><Validator Type=""Required"">True</Validator></Input><Input Name=""b"" Source="""" IsObject=""False"" Recordset=""rec"" /></Inputs>";

            var result = languageParser.Parse(mappingDefinition);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(LanguageParser))]
        public void LanguageParser_Parse_Output_ChildNodes_IsRequired_True()
        {
            const string elementTag = "Output";
            const string mapsTo = "MapsTo";
            const bool defaultValueToMapsTo = false;

            var languageParser = new LanguageParserTesting(elementTag, mapsTo, defaultValueToMapsTo);

            const string mappingDefinition = @"<Outputs><Output Name=""a"" MapsTo="""" Value=""1"" Recordset=""rec""><Validator Type=""NotRequired"">False</Validator></Output><Output Name=""b"" MapsTo="""" Value=""2"" IsObject=""False"" Recordset=""rec""><Validator Type=""Required"">True</Validator></Output></Outputs>";

            var result = languageParser.Parse(mappingDefinition);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual("", result[0].DefaultValue);
            Assert.IsFalse(result[0].EmptyToNull);
            Assert.IsFalse(result[0].IsEvaluated);
            Assert.IsFalse(result[0].IsJsonArray);
            Assert.IsFalse(result[0].IsObject);
            Assert.IsTrue(result[0].IsRecordSet);
            Assert.IsFalse(result[0].IsRequired);
            Assert.IsFalse(result[0].IsTextResponse);
            Assert.AreEqual("a", result[0].MapsTo);
            Assert.AreEqual("a", result[0].Name);
            Assert.AreEqual("1", result[0].RawValue);
            Assert.AreEqual("rec", result[0].RecordSetName);
            Assert.AreEqual("1", result[0].Value);

            Assert.AreEqual("", result[1].DefaultValue);
            Assert.IsFalse(result[1].EmptyToNull);
            Assert.IsFalse(result[1].IsEvaluated);
            Assert.IsFalse(result[1].IsJsonArray);
            Assert.IsFalse(result[1].IsObject);
            Assert.IsTrue(result[1].IsRecordSet);
            Assert.IsTrue(result[1].IsRequired);
            Assert.IsFalse(result[1].IsTextResponse);
            Assert.AreEqual("b", result[1].MapsTo);
            Assert.AreEqual("b", result[1].Name);
            Assert.AreEqual("2", result[1].RawValue);
            Assert.AreEqual("rec", result[1].RecordSetName);
            Assert.AreEqual("2", result[1].Value);
        }
    }

    class LanguageParserTesting : LanguageParser
    {
        public LanguageParserTesting(string elementTag, string mapsTo, bool defaultValueToMapsTo)
            : base(elementTag, mapsTo, defaultValueToMapsTo)
        {
        }
    }
}
