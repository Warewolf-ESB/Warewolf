using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.Data.Translators;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests.ConverterTest
{
    /// <summary>
    /// Summary description for DataListConverterTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListConverterTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("XML_Without_SystemTags_MethodName")]
        public void XML_Without_SystemTags_ConvertTo_WhenSingleRecordset_ExpectNoErrors()
        {
            //------------Setup for test--------------------------
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            const string data = "<DataList><hero index=\"1\"><name>Chuck Norris</name><pushups>All of them</pushups></hero></DataList>";
            const string shape = @"<DataList>
  <hero Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
    <name Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <pushups Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
  </hero>
  <rect Description="""" IsEditable=""True"" ColumnIODirection=""None"">
    <set Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
  </rect>
</DataList>";
            //------------Execute Test---------------------------
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), data, shape, out errors);
            var result = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);

            //------------Assert Results-------------------------
            const string expected = "<DataList><hero><name>Chuck Norris</name><pushups>All of them</pushups></hero></DataList>";

            StringAssert.Contains(result, expected);
        }

        [TestMethod]
        public void CanConvertFromDataListXMLTranslatorWithOutSystemTags()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), "<root><scalar>s1</scalar><rs><val>1</val><val>2</val></rs></root>", "<root><scalar/><rs><val/></rs></root>", out errors);
            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);

            const string Expected = "<DataList><scalar>s1</scalar><rs><val>2</val></rs></DataList>";

            Assert.AreEqual(Expected, data, "Expected [ " + Expected + " ] but got [ " + data + " ]");
        }

        [TestMethod]
        public void CanConvertFromWithSingleScalarDataListXMLTranslatorWithOutSystemTags()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), "<root><scalar>s1</scalar></root>", "<root><scalar/></root>", out errors);
            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);

            const string Expected = "<DataList><scalar>s1</scalar></DataList>";

            Assert.AreEqual(Expected, data, "Expected [ " + Expected + " ] but got [ " + data + " ]");
        }

        #region JSONClasses

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class TestClassRsWithScalar
        {


            public string scalar { get; set; }

            public IList<TestClassRS> rs { get; set; }
        }

        private class TestClassRS
        {
            public string val { get; set; }
        }

        #endregion

        [TestMethod]
        public void CanConvertFromDataListToJsonSingleScalarSingleRecordsetColumn()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), "<root><scalar>s1</scalar><rs><val>1</val></rs><rs><val>2</val></rs></root>", "<root><scalar/><rs><val/></rs></root>", out errors);
            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._JSON), enTranslationDepth.Data, out errors);

            var result = JsonConvert.DeserializeObject<TestClassRsWithScalar>(data);

            Assert.AreEqual("s1", result.scalar);
            Assert.AreEqual("1", result.rs[0].val);
            Assert.AreEqual("2", result.rs[1].val);
        }

        [TestMethod]
        public void CanConvertFromDataListToJsonMultipleScalarsMultipleRecordsets()
        {
            var compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<root><scalar>s1</scalar><otherScalar>s2</otherScalar><rs><val>1</val><otherVal>ABC</otherVal></rs><rs><val>2</val><otherVal>ZZZ</otherVal></rs><otherRS><val>1</val><myVal>ABC</myVal></otherRS><otherRS><val>90</val><myVal>123</myVal></otherRS></root>", "<root><scalar/><otherScalar/><rs><val/><otherVal/></rs><otherRS><val/><myVal/></otherRS></root>", out errors);
            var data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._JSON), enTranslationDepth.Data, out errors);

            const string expected = "{\"scalar\":\"s1\",\"otherScalar\":\"s2\",\"rs\" : [{\"val\":\"1\",\"otherVal\":\"ABC\"}, {\"val\":\"2\",\"otherVal\":\"ZZZ\"}],\"otherRS\" : [{\"val\":\"1\",\"myVal\":\"ABC\"}, {\"val\":\"90\",\"myVal\":\"123\"}]}";

            Assert.AreEqual(expected, data, "Expected [ " + expected + " ] but got [ " + data + " ]");
        }

        [TestMethod]
        public void CanConvertErrorResultTojson()
        {
            ErrorResultTO errors = new ErrorResultTO();
            errors.AddError("Error 1");
            errors.AddError("Error 2");

            var result = errors.MakeDataListReady(false);
            const string expected = "\"errors\": [ \"Error 1\",\"Error 2\"]";


            Assert.AreEqual(expected, result, "Expected [ " + expected + " but got [ " + result + " ]");
        }

        [TestMethod]
        public void CanDataListHelperCreateTargetShape()
        {
            // BuildTargetShape
            const string targetShape = "<DataList><result Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></DataList>";

            ErrorResultTO invokeErrors;
            TranslatorUtils tu = new TranslatorUtils();
            var dl = tu.TranslateShapeToObject(targetShape, false, out invokeErrors);
            var keys = dl.FetchAllUserKeys();

            Assert.AreEqual(0, invokeErrors.FetchErrors().Count);
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual("result", keys[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataList_FetchAllRecordSetKeys")]
        public void BinaryDataList_FetchAllRecordSetKeys_WhenHasRecordSetAndScalars_ShouldReturnOnlyRecordSets()
        {
            //------------Setup for test--------------------------
            // BuildTargetShape
            const string targetShape = "<DataList><result Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /><rs><Col1/><Col2/></rs></DataList>";
            ErrorResultTO invokeErrors;
            TranslatorUtils tu = new TranslatorUtils();
            var dl = tu.TranslateShapeToObject(targetShape, false, out invokeErrors);
            //------------Execute Test---------------------------
            var keys = dl.FetchAllRecordSetKeys();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, invokeErrors.FetchErrors().Count);
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual("rs", keys[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataList_FetchAllRecordSetKeys")]
        public void BinaryDataList_FetchAllRecordSetKeys_WhenHasScalars_ShouldReturn0()
        {
            //------------Setup for test--------------------------
            // BuildTargetShape
            const string targetShape = "<DataList><result Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></DataList>";
            ErrorResultTO invokeErrors;
            TranslatorUtils tu = new TranslatorUtils();
            var dl = tu.TranslateShapeToObject(targetShape, false, out invokeErrors);
            //------------Execute Test---------------------------
            var keys = dl.FetchAllRecordSetKeys();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, invokeErrors.FetchErrors().Count);
            Assert.AreEqual(0, keys.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BinaryDataList_FetchAllRecordSetKeys")]
        public void BinaryDataList_FetchAllRecordSetKeys_WhenHasRecordSetsOnly_ShouldReturn0()
        {
            //------------Setup for test--------------------------
            // BuildTargetShape
            const string targetShape = "<DataList><rs><Col1/><Col2/></rs><rec><Col4/><Col5/></rec></DataList>";
            ErrorResultTO invokeErrors;
            TranslatorUtils tu = new TranslatorUtils();
            var dl = tu.TranslateShapeToObject(targetShape, false, out invokeErrors);
            //------------Execute Test---------------------------
            var keys = dl.FetchAllRecordSetKeys();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, invokeErrors.FetchErrors().Count);
            Assert.AreEqual(2, keys.Count);
            Assert.AreEqual("rs", keys[0]);
            Assert.AreEqual("rec", keys[1]);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TranslatorUtils_TranslateShapeToObject")]
        public void TranslatorUtils_TranslateShapeToObject_whenRsBothAndColumnsMixed_FullRSPresentInShape()
        {
            //------------Setup for test--------------------------
            var translatorUtils = new TranslatorUtils();

            const string shape = @"<DataList>
  <rs Description="""" IsEditable=""True"" ColumnIODirection=""Both"">
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <val Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
  </rs>
</DataList>";

            //------------Execute Test---------------------------

            ErrorResultTO invokeErrors;
            var dl = translatorUtils.TranslateShapeToObject(shape, false, out invokeErrors);

            //------------Assert Results-------------------------

            IBinaryDataListEntry entry;
            string error;
            dl.TryGetEntry("rs", out entry, out error);

            var keys = dl.FetchAllUserKeys();

            Assert.AreEqual(0, invokeErrors.FetchErrors().Count);
            Assert.AreEqual(1, keys.Count);
            Assert.AreEqual("rs", keys[0]);
            Assert.AreEqual("result", entry.Columns[0].ColumnName);
            Assert.AreEqual("val", entry.Columns[1].ColumnName);

        }

        [TestMethod]
        [TestCategory("DataTableTranslator_UnitTest")]
        [Description("Test that a DataTable will convert to a DataList")]
        [Owner("Travis Frisinger")]
        public void CanConvertDataTableToDataList()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // build up DataTable
            DataTable dbData = new DataTable("rs");

            dbData.Columns.Add("val", typeof(string));
            dbData.Columns.Add("otherVal", typeof(int));

            dbData.Rows.Add("aaa", 1);
            dbData.Rows.Add("zzz", 2);

            // Execute Translator
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, "<root><rs><val/><otherVal/></rs></root>", out errors);

            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.AreEqual("<DataList><rs><val>aaa</val><otherVal>1</otherVal></rs><rs><val>zzz</val><otherVal>2</otherVal></rs></DataList>", data);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_ToDataTable")]
        // ReSharper disable once InconsistentNaming
        public void DataListTranslator_ToDataTable_WithDataList_PopulatedDataTable()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            var dbData = new DataTable("rs");
            dbData.Columns.Add("val", typeof(string));
            dbData.Columns.Add("otherVal", typeof(int));
            dbData.Rows.Add("aaa", 1);
            dbData.Rows.Add("zzz", 2);
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, "<root><rs><val/><otherVal/></rs></root>", out errors);
            var fetchBinaryDataList = compiler.FetchBinaryDataList(dlID, out errors);
            //------------Execute Test---------------------------
            var convertToDataTable = compiler.ConvertToDataTable(fetchBinaryDataList, "rs", out errors);
            //------------Assert Results-------------------------
            Assert.IsNotNull(convertToDataTable);
            Assert.AreEqual(2, convertToDataTable.Columns.Count);
            Assert.IsTrue(convertToDataTable.Columns.Contains("val"));
            Assert.IsTrue(convertToDataTable.Columns.Contains("otherVal"));
            Assert.AreEqual(2, convertToDataTable.Rows.Count);
            Assert.AreEqual("aaa", convertToDataTable.Rows[0]["val"]);
            Assert.AreEqual("1", convertToDataTable.Rows[0]["otherVal"]);
            Assert.AreEqual("zzz", convertToDataTable.Rows[1]["val"]);
            Assert.AreEqual("2", convertToDataTable.Rows[1]["otherVal"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_ToDataTable")]
        // ReSharper disable once InconsistentNaming
        public void DataListTranslator_ToDataTable_WithDataListWithIgnoreBlankRows_PopulatedDataTableWithoutBlankRows()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<root><rs><val>aaa</val><otherVal>1</otherVal></rs><rs><val></val><otherVal></otherVal></rs><rs><val>zzz</val><otherVal>2</otherVal></rs></root>", "<root><rs><val/><otherVal/></rs></root>", out errors);
            var fetchBinaryDataList = compiler.FetchBinaryDataList(dlID, out errors);
            //------------Execute Test---------------------------
            var convertToDataTable = compiler.ConvertToDataTable(fetchBinaryDataList, "rs", out errors);
            //------------Assert Results-------------------------
            Assert.IsNotNull(convertToDataTable);
            Assert.AreEqual(2, convertToDataTable.Columns.Count);
            Assert.IsTrue(convertToDataTable.Columns.Contains("val"));
            Assert.IsTrue(convertToDataTable.Columns.Contains("otherVal"));
            Assert.AreEqual(2, convertToDataTable.Rows.Count);
            Assert.AreEqual("aaa", convertToDataTable.Rows[0]["val"]);
            Assert.AreEqual("1", convertToDataTable.Rows[0]["otherVal"]);
            Assert.AreEqual("zzz", convertToDataTable.Rows[1]["val"]);
            Assert.AreEqual("2", convertToDataTable.Rows[1]["otherVal"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_ToDataTable")]
        // ReSharper disable once InconsistentNaming
        public void DataListTranslator_ToDataTable_WithDataListWithPopulateBlankRows_PopulatedDataTableWithoutBlankRows()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<root><rs><val>aaa</val><otherVal>1</otherVal></rs><rs><val></val><otherVal></otherVal></rs><rs><val>zzz</val><otherVal>2</otherVal></rs></root>", "<root><rs><val/><otherVal/></rs></root>", out errors);
            var fetchBinaryDataList = compiler.FetchBinaryDataList(dlID, out errors);
            //------------Execute Test---------------------------
            var convertToDataTable = compiler.ConvertToDataTable(fetchBinaryDataList, "rs", out errors, PopulateOptions.PopulateBlankRows);
            //------------Assert Results-------------------------
            Assert.IsNotNull(convertToDataTable);
            Assert.AreEqual(2, convertToDataTable.Columns.Count);
            Assert.IsTrue(convertToDataTable.Columns.Contains("val"));
            Assert.IsTrue(convertToDataTable.Columns.Contains("otherVal"));
            Assert.AreEqual(3, convertToDataTable.Rows.Count);
            Assert.AreEqual("aaa", convertToDataTable.Rows[0]["val"]);
            Assert.AreEqual("1", convertToDataTable.Rows[0]["otherVal"]);
            Assert.AreEqual("", convertToDataTable.Rows[1]["val"]);
            Assert.AreEqual("", convertToDataTable.Rows[1]["otherVal"]);
            Assert.AreEqual("zzz", convertToDataTable.Rows[2]["val"]);
            Assert.AreEqual("2", convertToDataTable.Rows[2]["otherVal"]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_ToDataTable")]
        public void DataListTranslator_ToDataTable_WhenNullDataList_ExpectError()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            //------------Execute Test---------------------------
            var convertToDataTable = compiler.ConvertToDataTable(null, "rs", out errors);
            //------------Assert Results-------------------------
            Assert.IsNull(convertToDataTable);
            Assert.AreEqual(1, errors.FetchErrors().Count);
            Assert.AreEqual("Value cannot be null.\r\nParameter name: input", errors.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_ToDataTable")]
        public void DataListTranslator_ToDataTable_WhenNullRecsetName_ExpectError()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            //------------Execute Test---------------------------
            var convertToDataTable = compiler.ConvertToDataTable(null, null, out errors);
            //------------Assert Results-------------------------
            Assert.IsNull(convertToDataTable);
            Assert.AreEqual(1, errors.FetchErrors().Count);
            Assert.AreEqual("Value cannot be null.\r\nParameter name: recsetName", errors.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListTranslator_ToDataTable")]
        public void DataListTranslator_ToDataTable_WhenEmptyRecsetName_ExpectError()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            //------------Execute Test---------------------------
            var convertToDataTable = compiler.ConvertToDataTable(null, "", out errors);
            //------------Assert Results-------------------------
            Assert.IsNull(convertToDataTable);
            Assert.AreEqual(1, errors.FetchErrors().Count);
            Assert.AreEqual("Value cannot be null.\r\nParameter name: recsetName", errors.FetchErrors()[0]);
        }


        [TestMethod]
        [TestCategory("DataTableTranslator_UnitTest")]
        [Description("Test that a DataTableTranslator will throw exception on mult rec set in shape")]
        [Owner("Travis Frisinger")]
        public void DataTableToDataListThrowsExceptionOnMultipleRecordsets()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // build up DataTable
            DataTable dbData = new DataTable("rs");

            dbData.Columns.Add("val", typeof(string));
            dbData.Columns.Add("otherVal", typeof(int));

            dbData.Rows.Add("aaa", 1);
            dbData.Rows.Add("zzz", 2);

            // Execute Translator
            ErrorResultTO errors;
            compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, "<root><rs><val/><otherVal/></rs><rs2><val/></rs2></root>", out errors);

            Assert.AreEqual(1, errors.FetchErrors().Count, "Did not return the correct number of errors");
            Assert.AreEqual("DataTable translator can only map to a single recordset!", errors.FetchErrors()[0], "Did not return the correct error message");
        }

        [TestMethod]
        [TestCategory("DataTableTranslator_UnitTest")]
        [Description("Test that a DataTableTranslator can convert to a single scalar value")]
        [Owner("Travis Frisinger")]
        public void CanConvertDataTableToDataListWithScalar()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            // build up DataTable
            DataTable dbData = new DataTable("rs");

            dbData.Columns.Add("scalar", typeof(string));

            dbData.Rows.Add("aaa");

            // Execute Translator
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, "<root><scalar/></root>", out errors);

            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.AreEqual("<DataList><scalar>aaa</scalar></DataList>", data);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("ErrorResultTO_MakeDatalistReady")]
        public void ErrorResultTO_MakeDatalistReady_CannotSetUnknownMemberError_MessageConvertedToOutdatedServerError()
        {
            var errorResultTO = new ErrorResultTO();
            errorResultTO.AddError("Cannot set unknown member");
            //------------Execute Test---------------------------

            var result = errorResultTO.MakeDataListReady(false);
            const string expected = "\"errors\": [ \"Resource has unrecognized formatting, this Warewolf Server may be to outdated to read this resource.\"]";

            // Assert Message Converted To Outdated Server Error
            Assert.AreEqual(expected, result, "Error message not relevent");
        }

        [TestMethod]
        [TestCategory("DataTableTranslator_UnitTest")]
        [Description("Test that a DataTableTranslator can convert to a single scalar value")]
        [Owner("Hagashen Naidu")]
        public void Populate_WithScalarValuesInDatatable_AddsToDataList()
        {
            //-------------------Setup ----------------------------------
            const string outputDefs = "<Outputs><Output Name=\"One\" MapsTo=\"[[One]]\" Value=\"[[One]]\" /><Output Name=\"Two\" MapsTo=\"[[Two]]\" Value=\"[[Two]]\" /><Output Name=\"Three\" MapsTo=\"[[Three]]\" Value=\"[[Three]]\" /></Outputs>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), "<root></root>", "<root><One/><Two/><Three/></root>", out errors);
            // build up DataTable
            DataTable dbData = new DataTable("rs");
            dbData.Columns.Add("One", typeof(string));
            dbData.Columns.Add("Two", typeof(string));
            dbData.Columns.Add("Three", typeof(string));
            dbData.Rows.Add("aaa", "bbb", "ccc");

            // Execute Translator
            Guid dlID = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, outputDefs, dataListID, out errors);

            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.AreEqual("<DataList><One>aaa</One><Two>bbb</Two><Three>ccc</Three></DataList>", data);
        }


        [TestMethod]
        [TestCategory("DataTableTranslator_UnitTest")]
        [Description("Test that a DataTableTranslator can convert to a single scalar value")]
        [Owner("Hagashen Naidu")]
        public void Populate_TwiceWithStarIndex_AddsToCorrectIndex()
        {
            //-------------------Setup ----------------------------------
            const string outputDefs = "<Outputs><Output Name=\"[[rs(*).num]]\" MapsTo=\"[[rec(*).num]]\" Value=\"[[rs(*).num]]\" /></Outputs>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), "<root></root>", "<root><rs><num></num></rs></root>", out errors);
            // build up DataTable
            DataTable dbData = new DataTable("rec");
            dbData.Columns.Add("num", typeof(string));
            dbData.Rows.Add("One");
            dbData.Rows.Add("Two");
            dbData.Rows.Add("Three");

            // Execute Translator
            var dlID = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, outputDefs, dataListID, out errors);
            dlID = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, outputDefs, dlID, out errors);

            var data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.AreEqual("<DataList><rs><num>One</num></rs><rs><num>Two</num></rs><rs><num>Three</num></rs></DataList>", data);
        }

        [TestMethod]
        [TestCategory("DataTableTranslator_UnitTest")]
        [Description("Test that a DataTableTranslator can convert to a single scalar value")]
        [Owner("Hagashen Naidu")]
        public void Populate_TwiceWithAppendIndex_AddsToEnd()
        {
            //-------------------Setup ----------------------------------
            const string outputDefs = "<Outputs><Output Name=\"[[rs().num]]\" MapsTo=\"[[rec(*).num]]\" Value=\"[[rs().num]]\" /></Outputs>";
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), "<root></root>", "<root><rs><num></num></rs></root>", out errors);
            // build up DataTable
            DataTable dbData = new DataTable("rec");
            dbData.Columns.Add("num", typeof(string));
            dbData.Rows.Add("One");
            dbData.Rows.Add("Two");
            dbData.Rows.Add("Three");

            // Execute Translator
            Guid dlID = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, outputDefs, dataListID, out errors);
            dlID = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, outputDefs, dlID, out errors);

            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.AreEqual("<DataList><rs><num>One</num></rs><rs><num>Two</num></rs><rs><num>Three</num></rs><rs><num>One</num></rs><rs><num>Two</num></rs><rs><num>Three</num></rs></DataList>", data);
        }

        [TestMethod]
        [TestCategory("DataTableTranslator_UnitTest")]
        [Description("Test that a DataTableTranslator can convert to a single scalar value")]
        [Owner("Hagashen Naidu")]
        public void Populate_TwiceWithNumericIndex_PutsDataAtIndex()
        {
            //-------------------Setup ----------------------------------
            const string outputDefs = "<Outputs><Output Name=\"[[rs(2).num]]\" MapsTo=\"[[rec(*).num]]\" Value=\"[[rs(2).num]]\" Recordset=\"rs\"/></Outputs>";
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), "<root></root>", "<root><rs><num></num></rs></root>", out errors);
            // build up DataTable
            DataTable dbData = new DataTable("rec");
            dbData.Columns.Add("num", typeof(string));
            dbData.Rows.Add("One");
            dbData.Rows.Add("Two");
            dbData.Rows.Add("Three");

            // Execute Translator
            Guid dlID = compiler.PopulateDataList(DataListFormat.CreateFormat(GlobalConstants._DATATABLE), dbData, outputDefs, dataListID, out errors);

            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);

            Assert.AreEqual("<DataList><rs><num>One</num></rs><rs><num>Two</num></rs><rs><num>Three</num></rs></DataList>", data);
        }
    }
}
