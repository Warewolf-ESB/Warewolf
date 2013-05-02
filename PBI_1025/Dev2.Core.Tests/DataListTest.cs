using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Xml.Linq;
using Dev2;
using Unlimited.UnitTest.Framework;

namespace DataListTset {
//    /// <summary>
//    /// Summary description for check-in
//    /// </summary>
//    [TestClass]
//    public class DataListTest {

//        private static DataListCompiler dlUtil = new DataListCompiler();
//        private static DataListCompiler target = new DataListCompiler();
//        private static DataListCompiler dlu = new DataListCompiler();

//        private TestContext testContextInstance;

//        /// <summary>
//        ///Gets or sets the test context which provides
//        ///information about and functionality for the current test run.
//        ///</summary>
//        public TestContext TestContext {
//            get {
//                return testContextInstance;
//            }
//            set {
//                testContextInstance = value;
//            }
//        }

//        #region Output Mapping Test

//        [TestMethod]
//        public void TestOutputParsingScalar() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingScalar);

//            IDev2Definition d = defs[0];

//            Assert.IsTrue(defs.Count == 3 && d.MapsTo == "fname" && d.Value == "firstName" && d.Name == "FirstName" && !d.IsRecordSet);

//        }

//        [TestMethod]
//        public void TestOutputParsingRecordSet() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
//            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs);

//            IDev2Definition d = defs[0];

//            Assert.IsTrue(d.IsRecordSet && recCol.RecordSetNames[0] == "Person" && recCol.RecordSets[0].Columns[0].Value == "ppl().firstName");

//        }

//        [TestMethod]
//        public void TestRecordSetCollectionCreationEnsure1Set() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
//            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs);

//            Assert.IsTrue((recCol.RecordSetNames.Count == 1));
//        }

//        [TestMethod]
//        public void TestScalarRecordSetMixedParsing() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingMixed);
//            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs);
//            IList<IDev2Definition> scalarList = DataListFactory.CreateScalarList(defs);


//            Assert.IsTrue(scalarList.Count == 1 && recCol.RecordSetNames.Count == 1);
//        }

//        [TestMethod]
//        public void Ensure_Correct_Operation_With_Null_DL() {
//            string currentADL = ParserStrings.nullDLInput;
//            string preADL = ParserStrings.nullPreExecuteDLInput;

//            string result = dlUtil.ShapeOutput(currentADL, "<ADL></ADL>", ParserStrings.nullOutputMapping, null);

//            Assert.AreEqual(ParserStrings.nullResult, result);
//        }

//        [TestMethod]
//        public void OutputShape_With_OutputMapping() {

//            string defs = @"<Outputs><Output Name=""Dev2CustomDataService"" MapsTo=""Dev2CustomDataService"" Value="""" /><Output Name=""Dev2WebpartType"" MapsTo=""Dev2WebpartType"" Value="""" /><Output Name=""Dev2CustomDataRowDelimiter"" MapsTo=""Dev2CustomDataRowDelimiter"" Value="""" /><Output Name=""Dev2BindingData"" MapsTo=""Dev2BindingData"" Value=""[[BindingData]]"" /><Output Name=""Dev2LineBreak"" MapsTo=""Dev2LineBreak"" Value="""" /><Output Name=""Dev2DisplayElement"" MapsTo=""Dev2DisplayElement"" Value="""" /><Output Name=""Dev2OptionElement"" MapsTo=""Dev2OptionElement"" Value="""" /><Output Name=""Dev2ServiceParameters"" MapsTo=""Dev2ServiceParameters"" Value="""" /><Output Name=""Dev2PartName"" MapsTo=""Dev2PartName"" Value="""" /></Outputs>";

//            string result = dlUtil.ShapeOutput(TestStrings.shapedoutput_with_mapping_currentADL, TestStrings.shapedoutput_with_mapping_preADL, defs, TestStrings.shapedoutput_with_mapping_DL);

//            Assert.AreEqual(TestStrings.shapedoutput_with_mapping_result, result);
//        }

//        // New
//        [TestMethod]
//        public void Output_To_Recordset_Last_Record() { // -ok

//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string adl = TestStrings.sampleADLMixedCompile;

//            string defs = @"<Outputs><Output Name=""firstName"" MapsTo=""firstName"" Value=""[[Person().fName]]"" Recordset=""ppl"" /></Outputs>";

//            string shape = @"<DL><Person><fName/><lName/><age/></Person></DL>";

//            string pre = @"<ADL><Person><fName>Bob</fName><lName>Bunker</lName><age>52</age></Person></ADL>";

//            string expected = "<ADL><Person><fName>Bob</fName><lName>Bunker</lName><age>52</age></Person><Person><fName>Travis</fName><lName/><age/></Person><Person><fName>Leanne</fName><lName/><age/></Person></ADL>";

//            string result = compiler.ShapeOutput(adl, pre, defs, shape);

//            Assert.AreEqual(expected, result);

//        }

//        [TestMethod]
//        public void Output_To_Recordset_Star() { // -ok

//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string adl = TestStrings.sampleADLMixedCompile;

//            string defs = @"<Outputs><Output Name=""firstName"" MapsTo=""firstName"" Value=""[[Person(*).fName]]"" Recordset=""ppl"" /><Output Name=""lastName"" MapsTo=""lastName"" Value=""[[Person(*).lName]]"" Recordset=""ppl"" /></Outputs>";

//            string shape = @"<DL><Person><fName/><lName/><age/></Person></DL>";

//            string expected = "<ADL><Person><fName>Travis</fName><lName>Frisinger</lName><age/></Person><Person><fName>Leanne</fName><lName>Frisinger</lName><age/></Person></ADL>";

//            string result = compiler.ShapeOutput(adl, "<ADL></ADL>", defs, shape);

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);

//        }

//        [TestMethod]
//        public void Output_To_Recordset_Index() {

//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string adl = TestStrings.sampleADLMixedCompile;

//            string defs = @"<Outputs><Output Name=""firstName"" MapsTo=""firstName"" Value=""[[Person(1).fName]]"" Recordset=""ppl"" /><Output Name=""lastName"" MapsTo=""lastName"" Value=""[[Person(1).lName]]"" Recordset=""ppl"" /></Outputs>";

//            string shape = @"<DL><Person><fName/><lName/><age/></Person></DL>";

//            string expected = "<ADL><Person><fName>Leanne</fName><lName>Frisinger</lName><age/></Person></ADL>";

//            string result = compiler.ShapeOutput(adl, "<ADL></ADL>", defs, shape);

//            Assert.AreEqual(expected, result);

//        }

//        [TestMethod]
//        public void Output_To_Recordset_Negative_Index() { // -ok

//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string adl = TestStrings.sampleADLMixedCompile;

//            string defs = @"<Outputs><Output Name=""firstName"" MapsTo=""firstName"" Value=""[[Person(-1).fName]]"" Recordset=""ppl"" /><Output Name=""lastName"" MapsTo=""lastName"" Value=""[[Person(-1).lName]]"" Recordset=""ppl"" /></Outputs>";

//            string shape = @"<DL><Person><fName/><lName/><age/></Person></DL>";

//            try {
//                string result = compiler.ShapeOutput(adl, "<ADL></ADL>", defs, shape);

//                Assert.Fail();
//            }
//            catch (Exception) {
//                Assert.IsTrue(1 == 1);
//            }

//        }

//        [TestMethod]
//        public void Output_To_Recordset_To_Scalar() { // -ok

//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string adl = TestStrings.sampleADLMixedCompile;

//            string defs = @"<Outputs><Output Name=""firstName"" MapsTo=""firstName"" Value=""[[myScalar]]"" Recordset=""ppl"" /></Outputs>";

//            string shape = @"<DL><Person><fName/><lName/><age/></Person><myScalar0/><myScalar/></DL>";

//            string expected = "<ADL><Person><fName/><lName/><age/></Person><myScalar0/><myScalar>Leanne</myScalar></ADL>";

//            string result = compiler.ShapeOutput(adl, "<ADL></ADL>", defs, shape);

//            Assert.AreEqual(expected, result);

//        }
//        // End New

//        #endregion

//        #region Activity Output Parsing

//        [TestMethod]
//        public void TestDataStreamParseBasedUponRecordSetOutput() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
//            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
//            dataParser.ParseDataStream(TestStrings.sampleDataRecordSet, defs);

//            IRecordSetCollection inst = dataParser.ParsedData.Recordsets;

//            Assert.IsTrue((dataParser.ParsedData.Scalars.Count == 0) && (inst.RecordSetNames.Count == 1) && (inst.RecordSets[0].Columns.Count > 0));
//        }

//        [TestMethod]
//        public void ExtractScalarBlank() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.blankOutput);


//            Assert.IsTrue(defs.Count == 1 && defs[0].MapsTo == "ABC");
//        }

//        [TestMethod]
//        public void TestDataStreamParseBasedUponScalarOutput() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingScalar);
//            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
//            dataParser.ParseDataStream(TestStrings.sampleDataScalar, defs);

//            Assert.IsTrue((dataParser.ParsedData.Recordsets.RecordSets.Count == 0) && (dataParser.ParsedData.Scalars.Count == 3) && (dataParser.ParsedData.Scalars[0].Value != null));
//        }

//        [TestMethod]
//        public void TestDataStreamParseBasedUponMixedOutput() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingMixed);

//            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
//            dataParser.ParseDataStream(TestStrings.sampleDataMixed, defs);

//            Assert.IsTrue((dataParser.ParsedData.Recordsets.RecordSets.Count == 1) && (dataParser.ParsedData.Scalars.Count == 1));
//        }

//        [TestMethod]
//        public void TestDataStreamParseBasedUponRecordSetDataForScalar() {
//            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingScalar);

//            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
//            dataParser.ParseDataStream(TestStrings.sampleDataRecordSet, defs);

//            Assert.IsTrue((dataParser.ParsedData.Recordsets.RecordSets.Count == 0) && (dataParser.ParsedData.Scalars.Count == 3));
//        }

//        #endregion

//        #region Util Test
//        [TestMethod]
//        public void Extract_System_Tag_Value() {

//            string adl = @"<XmlData>
//  <Service>HtmlWidget.wiz</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/HtmlWidget.wiz</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <ADL>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <Name>abc</Name>
//        <Value> 123  </Value>
//        <cssClass></cssClass>
//        <Async />
//      </ADL>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>abc</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";

//            string result = target.ExtractSystemTag(enSystemTag.Service, adl);

//            string expected = @"HtmlWidget.wiz";

//            Assert.AreEqual(expected, result);
//        }


//        [TestMethod]
//        public void StripDoubleBracketNormal() {
//            string canidate = "[[abc()]]";

//            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

//            Assert.AreEqual(result, "abc");
//        }

//        [TestMethod]
//        public void StripDoubleBracketNone() {
//            string canidate = "abc()";

//            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

//            Assert.AreEqual(canidate, "abc()");
//        }

//        [TestMethod]
//        public void StripDoubleBracketRecursiveEval() {
//            string canidate = "[[[[abc()]]]]";

//            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

//            Assert.AreEqual(result, "[[[[abc()]]]]");
//        }

//        /// <summary>
//        ///A test for ShapeInput
//        ///</summary>
//        [TestMethod()]
//        public void ShapeInputTest() {
//            string currentADL = "<ADL><Test>something</Test><testing>tests</testing></ADL>";
//            string InputDefs = null;
//            string dlShape = "<ADL><testing></testing><myTest></myTest></ADL>";
//            string expected = @"<ADL><testing>tests</testing><myTest></myTest></ADL>"; // TODO: Initialize to an appropriate value
//            string actual;
//            actual = target.ShapeInput(currentADL, InputDefs, dlShape);
//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void ShapeOutputTest_ExtraDataListOption_DataListwithShapedOutput() {

//            string currentADL = "<ADL><Test>something</Test><testing>tests</testing></ADL>";
//            string InputDefs = null;
//            string dlShape = "<ADL><testing></testing><myTest></myTest></ADL>";
//            string expected = @"<ADL><testing>tests</testing><myTest></myTest></ADL>";
//            string actual;
//            actual = target.ShapeOutput(currentADL, currentADL, InputDefs, dlShape);
//            Assert.AreEqual(expected, actual);
//        }
//        #endregion

//        #region Input Mapping Test

//        [TestMethod]
//        public void TestInputMappingWithDefaultValue() {
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(TestStrings.inputsWithDefaultValue);
//            IDataListCompiler dlCompiler = DataListFactory.CreateDataListCompiler();
//            string newShape = @"<ADL><Person/><RecordCount/></ADL>";
//            string result = dlCompiler.ExtractInputSegments(TestStrings.sampleADLMixedCompile, inputs, newShape);

//            string expected = @"<ADL><Person>Travis.Frisinger</Person><RecordCount>2</RecordCount></ADL>";

//            Assert.AreEqual(expected, result.Trim());

//        }

//        [TestMethod]
//        public void TestInputMappingWithStaticSource() {
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(TestStrings.inputsWithStaticData);
//            IDataListCompiler dlCompiler = DataListFactory.CreateDataListCompiler();
//            string newShape = @"<ADL><Person/><RecordCount/></ADL>";
//            string result = dlCompiler.ExtractInputSegments(TestStrings.sampleADLMixedCompile, inputs, newShape);

//            string expected = @"<ADL><Person>Travis.Frisinger</Person><RecordCount>2</RecordCount></ADL>";

//            Assert.AreEqual(expected, result.Trim());

//        }

//        [TestMethod]
//        public void TestInputMappingWithRequireMissingSourceAndDefault() {
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(TestStrings.inputWithNoMapping);
//            IDataListCompiler dlCompiler = DataListFactory.CreateDataListCompiler();

//            string newShape = @"<ADL><Person/><RecordCount/></ADL>";
//            try {
//                dlCompiler.ExtractInputSegments(TestStrings.sampleADLMixedCompile, inputs, newShape);
//                Assert.Fail();
//            }
//            catch (Exception) {
//                Assert.IsTrue(true);
//            }

//        }



//        #endregion

//        #region Input Extraction Test

//        [TestMethod]
//        public void TestInputExtractionScalar() {
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(TestStrings.sampleActivityInputScalar);
//            string newShape = @"<ADL><fname/><lname/></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLScalarCompile, inputs, newShape);

//            string expected = @"<ADL><fname>Travis</fname><lname>Frisinger</lname></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        [TestMethod]
//        public void TestInputExtractionRecordSet() {
//            string inp = @"<Inputs><Input Name=""fName"" Source=""[[ppl().firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl().lastName]]"" Recordset=""Person"" /><Input Name=""years"" Source=""[[ppl().years]]"" Recordset=""Person"" /></Inputs>";
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(inp);

//            string newShape = @"<ADL><Person><fName/><lName/><years/></Person></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLRecordSetCompile, inputs, newShape);

//            string expected = @"<ADL><Person><fName>Leanne</fName><lName>Frisinger</lName><years>25</years></Person></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);
//        }

//        [TestMethod]
//        public void TestInputExtractionMixed() {
//            string inp = @"<Inputs><Input Name=""fName"" Source=""[[ppl().firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl().lastName]]"" Recordset=""Person"" /><Input Name=""years"" Source=""[[ppl().years]]"" Recordset=""Person"" /><Input Name=""RecordCount"" Source=""[[recordCount]]"" /></Inputs>";
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(inp);
//            string newShape = @"<ADL><Person><fName/><lName/><years/></Person><RecordCount/><RecordCount/></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLMixedPopulated, inputs, newShape);

//            string expected = @"<ADL><Person><fName>Hanna</fName><lName>Davis</lName><years>26</years></Person><RecordCount>3</RecordCount></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);
//        }

//        [TestMethod]
//        public void Input_Extraction_With_Recordset_And_Static() {
//            string inp = @"<Inputs><Input Name=""fName"" Source=""[[ppl().firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl().lastName]]"" Recordset=""Person"" /><Input Name=""years"" Source=""999"" Recordset=""Person"" /><Input Name=""RecordCount"" Source=""[[recordCount]]"" /></Inputs>";
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(inp);
//            string newShape = @"<ADL><Person><fName/><lName/><years/></Person><RecordCount/><RecordCount/></ADL>";

//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLMixedPopulated, inputs, newShape);

//            string expected = @"<ADL><Person><fName>Hanna</fName><lName>Davis</lName><years>999</years></Person><RecordCount>3</RecordCount></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);
//        }

//        // New
//        //_____________________________________________________________________________________________________________________________________


//        [TestMethod]
//        public void Input_Mapping_With_Recordset_No_Star() {
//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string currentADL = TestStrings.sampleADLMixedCompile;

//            string inputMapping = @"<Inputs><Input Name=""fName"" Source=""[[ppl().firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl().lastName]]"" Recordset=""Person"" /><Input Name=""age"" Source=""[[ppl().years]]"" Recordset=""Person"" /></Inputs>";

//            string shape = "<DL><Person><fName/><lName/><age/></Person></DL>";

//            string result = compiler.ShapeInput(currentADL, inputMapping, shape);

//            string expected = @"<ADL><Person><fName>Leanne</fName><lName>Frisinger</lName><age>25</age></Person></ADL>";

//            Assert.AreEqual(expected, result);
//        }

//        [TestMethod]
//        public void Input_Mapping_With_Recordset_Star() {
//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string currentADL = TestStrings.sampleADLMixedCompile;

//            string inputMapping = @"<Inputs><Input Name=""fName"" Source=""[[ppl(*).firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl(*).lastName]]"" Recordset=""Person"" /><Input Name=""age"" Source=""[[ppl(*).years]]"" Recordset=""Person"" /></Inputs>";

//            string shape = "<DL><Person><fName/><lName/><age/></Person></DL>";

//            string result = compiler.ShapeInput(currentADL, inputMapping, shape);

//            string expected = @"<ADL><Person><fName>Travis</fName><lName>Frisinger</lName><age>30</age></Person><Person><fName>Leanne</fName><lName>Frisinger</lName><age>25</age></Person></ADL>";

//            Assert.AreEqual(expected, result);
//        }

//        [TestMethod]
//        public void Input_Mapping_With_Recordset_Index() {
//            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

//            string currentADL = TestStrings.sampleADLMixedCompile;

//            string inputMapping = @"<Inputs><Input Name=""fName"" Source=""[[ppl(2).firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl(2).lastName]]"" Recordset=""Person"" /><Input Name=""age"" Source=""[[ppl(2).years]]"" Recordset=""Person"" /></Inputs>";

//            string shape = "<DL><Person><fName/><lName/><age/></Person></DL>";

//            string result = compiler.ShapeInput(currentADL, inputMapping, shape);

//            string expected = @"<ADL><Person><fName>Leanne</fName><lName>Frisinger</lName><age>25</age></Person></ADL>";

//            Assert.AreEqual(expected, result);
//        }

//        [TestMethod]
//        public void TestInputExtractionIndexOutOfRange() {
//            string inp = @"<Inputs><Input Name=""fName"" Source=""[[ppl(5).firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl(5).lastName]]"" Recordset=""Person"" /><Input Name=""years"" Source=""[[ppl().years]]"" Recordset=""Person"" /><Input Name=""RecordCount"" Source=""[[recordCount]]"" /></Inputs>";
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(inp);
//            string newShape = @"<ADL><Person><fName/><lName/><years/></Person><RecordCount/><RecordCount/></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLMixedPopulated, inputs, newShape);

//            string expected = @"<ADL><Person><fName></fName><lName></lName><years>26</years></Person><RecordCount>3</RecordCount></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);
//        }

//        [TestMethod]
//        public void TestInputExtractionStarToScalar() {//
//            string inp = @"<Inputs><Input Name=""fName"" Source=""[[ppl().firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl().lastName]]"" Recordset=""Person"" /><Input Name=""years"" Source=""[[ppl().years]]"" Recordset=""Person"" /><Input Name=""RecordCount"" Source=""[[recordCount]]"" /><Input Name=""testScalar"" Source=""[[ppl(*).years]]"" Recordset="""" /></Inputs>";
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(inp);
//            string newShape = @"<ADL><Person><fName/><lName/><years/></Person><RecordCount/><RecordCount/><testScalar></testScalar></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLMixedPopulated, inputs, newShape);

//            string expected = @"<ADL><Person><fName>Hanna</fName><lName>Davis</lName><years>26</years></Person><RecordCount>3</RecordCount><testScalar>26</testScalar></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);
//        }

//        [TestMethod]
//        public void TestInputExtractionStarToStar() {//
//            string inp = @"<Inputs><Input Name=""fName"" Source=""[[ppl(*).firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl(*).lastName]]"" Recordset=""Person"" /><Input Name=""year"" Source=""[[ppl(*).years]]"" Recordset=""Person"" /><Input Name=""RecordCount"" Source=""[[recordCount]]"" /></Inputs>";
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(inp);
//            string newShape = @"<ADL><Person><fName/><lName/><year/></Person><RecordCount/><RecordCount/><testScalar></testScalar></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLMixedPopulated, inputs, newShape);

//            string expected = @"<ADL><Person><fName>Bob</fName><lName>Smith</lName><year>29</year></Person><Person><fName>Greg</fName><lName>Jacobs</lName><year>31</year></Person><Person><fName>Hanna</fName><lName>Davis</lName><year>26</year></Person><RecordCount>3</RecordCount><testScalar></testScalar></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);
//        }

//        [TestMethod]
//        public void TestInputExtractionEvaluatedIndex() {//
//            string inp = @"<Inputs><Input Name=""fName"" Source=""[[ppl().firstName]]"" Recordset=""Person"" /><Input Name=""lName"" Source=""[[ppl().lastName]]"" Recordset=""Person"" /><Input Name=""years"" Source=""[[ppl().years]]"" Recordset=""Person"" /><Input Name=""RecordCount"" Source=""[[recordCount]]"" /><Input Name=""testScalar"" Source=""[[ppl([[recordCount]]).years]]"" Recordset="""" /></Inputs>";
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(inp);
//            string newShape = @"<ADL><Person><fName/><lName/><years/></Person><RecordCount/><RecordCount/><testScalar></testScalar></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLMixedPopulated, inputs, newShape);

//            string expected = @"<ADL><Person><fName>Hanna</fName><lName>Davis</lName><years>26</years></Person><RecordCount>3</RecordCount><testScalar>26</testScalar></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result);
//        }

//        //_____________________________________________________________________________________________________________________________________

//        [TestMethod]
//        public void TestInputExtractionScalarWithMixed() {
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(TestStrings.sampleActivityInputScalar);
//            string newShape = @"<ADL><fname/><lname/></ADL>";
//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleADLMixedPopulated, inputs, newShape);

//            string expected = @"<ADL><fname></fname><lname></lname></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        [TestMethod]
//        public void TestInputExtractionBlankScalar() {
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(TestStrings.sampleActivityInputScalar);

//            try {
//                string newShape = @"<ADL><fname/><lname/></ADL>";
//                string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleInputExtractionBlankScalar, inputs, newShape);
//                Assert.Fail();
//            }
//            catch (Exception) {
//                Assert.IsTrue(true);
//            }
//        }

//        [TestMethod]
//        public void TestInputExtractionMissingData() {
//            IList<IDev2Definition> inputs = DataListFactory.CreateInputParser().Parse(TestStrings.sampleActivityInputScalar);

//            try {
//                string newShape = @"<ADL><fname/><lname/></ADL>";
//                string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.sampleInputExtractionMissingScalar, inputs, newShape);
//                Assert.Fail();
//            }
//            catch (Exception) {
//                Assert.IsTrue(true);
//            }
//        }

//        [TestMethod]
//        public void Extract_Inputs_With_Name_In_ADL() {
//            string dl = @"<DataList>
//  <Name Description="""" />
//  <Value Description="""" />
//  <cssClass Description="""" />
//  <DataValue Description="""" />
//  <FormView Description="""" />
//</DataList>";

//            string inputs = @"<Inputs><Input Name=""Name"" Source=""[[Name]]"" /><Input Name=""Value"" Source=""[[Value]]"" /><Input Name=""cssClass"" Source=""[[cssClass]]"" /><Input Name=""FormView"" Source="""" /><Input Name=""DataValue"" Source=""[[DataValue]]"" /></Inputs>";

//            IList<IDev2Definition> defs = DataListFactory.CreateInputParser().Parse(inputs);

//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.name_in_input_shape_adl, defs, dl);

//            string expected = @"<ADL><Name>abc</Name><Value>def</Value><cssClass></cssClass><DataValue></DataValue></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        //<span><input type="text" id="Dev2elementName" name="Dev2elementName" value="[[Dev2elementName2]]" class="requiredClass" /><font color="red">*</font></span> 
//        [TestMethod]
//        public void Extract_Inputs_With_HTML_In_Scalar() {
//            string dl = @"<DataList>
//  <Name Description="""" />
//  <Value Description="""" />
//  <cssClass Description="""" />
//  <DataValue Description="""" />
//  <FormView Description="""" />
//</DataList>";

//            string inputs = @"<Inputs><Input Name=""Name"" Source=""[[Name]]"" /><Input Name=""Value"" Source=""[[Value]]"" /><Input Name=""cssClass"" Source=""[[cssClass]]"" /><Input Name=""DataValue"" Source=""[[DataValue]]"" /></Inputs>";

//            IList<IDev2Definition> defs = DataListFactory.CreateInputParser().Parse(inputs);

//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.html_in_scalar_adl, defs, dl);

//            string expected = @"<ADL><Name>abc</Name><Value><span><input type=""text"" id=""Dev2elementName"" name=""Dev2elementName"" value=""[[Dev2elementName2]]"" class=""requiredClass"" /><font color=""red"">*</font></span></Value><cssClass></cssClass><DataValue></DataValue></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        [TestMethod]
//        public void Extract_Inputs_With_HTML_In_Scalar_Non_Same_Mapping() {
//            string dl = @"<DataList>
//  <Name2 Description="""" />
//  <Value2 Description="""" />
//  <cssClass2 Description="""" />
//  <DataValue2 Description="""" />
//  <FormView Description="""" />
//</DataList>";

//            string inputs = @"<Inputs><Input Name=""Name2"" Source=""[[Name]]"" /><Input Name=""Value2"" Source=""[[Value]]"" /><Input Name=""cssClass2"" Source=""[[cssClass]]"" /><Input Name=""DataValue2"" Source=""[[DataValue]]"" /></Inputs>";

//            IList<IDev2Definition> defs = DataListFactory.CreateInputParser().Parse(inputs);

//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.html_in_scalar_adl, defs, dl);

//            string expected = @"<ADL><Name2>abc</Name2><Value2><span><input type=""text"" id=""Dev2elementName"" name=""Dev2elementName"" value=""[[Dev2elementName2]]"" class=""requiredClass"" /><font color=""red"">*</font></span></Value2><cssClass2></cssClass2><DataValue2></DataValue2></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        [TestMethod]
//        public void Extract_Blank_Input_Not_Bracket_Value() {

//            string inputs = @"<Inputs><Input Name=""required"" Source="""" /><Input Name=""validationClass"" Source="""" /><Input Name=""cssClass"" Source="""" /><Input Name=""Dev2customStyle"" Source=""[[Dev2customStyleLabel]]"" /></Inputs>";

//            IList<IDev2Definition> defs = DataListFactory.CreateInputParser().Parse(inputs);

//            string dl = @"<DataList>
//    <Dev2displayTextLabel Description="""" />
//    <ErrorMsg Description="""" />
//    <Fragment Description="""" />
//    <Dev2elementNameLabel Description="""" />
//    <cssClass Description="""" />
//    <tabIndex Description="""" />
//    <customStyle Description="""" />
//    <Dev2heightLabel Description="""" />
//    <Dev2widthLabel Description="""" />
//    <Dev2customStyleLabel Description="""" />
//  </DataList>";

//            string result = DataListFactory.CreateDataListCompiler().ExtractInputSegments(TestStrings.input_adl_extract_blank, defs, dl);

//            string expected = @"<ADL><Dev2displayTextLabel></Dev2displayTextLabel><ErrorMsg></ErrorMsg><Fragment></Fragment><Dev2elementNameLabel></Dev2elementNameLabel><cssClass></cssClass><tabIndex></tabIndex><customStyle></customStyle><Dev2heightLabel></Dev2heightLabel><Dev2widthLabel></Dev2widthLabel><Dev2customStyleLabel></Dev2customStyleLabel></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        #endregion

//        #region System Tag Operations
//        [TestMethod]
//        public void TestSystemTagInsert() {
//            string result = DataListFactory.CreateSentinel().UpsertSystemTag(TestStrings.sampleADLMixedPopulated, enSystemTag.InstanceId, TestStrings.workflowID);

//            Assert.AreEqual(TestStrings.systemSingleTagInjectResult, result);
//        }

//        [TestMethod]
//        public void TestSystemTagUpdate() {
//            string result = DataListFactory.CreateSentinel().UpsertSystemTag(TestStrings.systemSingleTagInjectResult, enSystemTag.InstanceId, TestStrings.replaceWorkflowID);

//            Assert.AreEqual(TestStrings.systemSingleTagUpdateResult, result);
//        }

//        [TestMethod]
//        public void TestSystemTagBlank() {
//            string result = DataListFactory.CreateSentinel().BlankSystemTag(TestStrings.systemSingleTagInjectResult, enSystemTag.InstanceId);

//            Assert.AreEqual(TestStrings.systemSingleTagBlankResult, result);
//        }

//        [TestMethod]
//        public void TestSystemTagRemove() {
//            string result = DataListFactory.CreateSentinel().RemoveSystemTag(TestStrings.systemSingleTagInjectResult, enSystemTag.InstanceId);

//            Assert.AreEqual(TestStrings.systemSingleTagRemoveResult, result);
//        }


//        [TestMethod]
//        public void TestSystemTagRemoveNotFoundTag() {
//            IDataListSentinel sent = DataListFactory.CreateSentinel();
//            string result = sent.UpsertSystemTag(TestStrings.sampleADLMixedPopulated, enSystemTag.InstanceId, TestStrings.workflowID);

//            try {
//                result = sent.RemoveSystemTag(result, enSystemTag.ParentWorkflowInstanceId);
//                Assert.Fail();
//            }
//            catch (Exception) {
//                Assert.IsTrue(true);
//            }
//        }

//        [TestMethod]
//        public void TestSystemTagBlankNotFoundTag() {
//            IDataListSentinel sent = DataListFactory.CreateSentinel();
//            string result = sent.UpsertSystemTag(TestStrings.sampleADLMixedPopulated, enSystemTag.InstanceId, TestStrings.workflowID);

//            try {
//                result = sent.BlankSystemTag(result, enSystemTag.ParentWorkflowInstanceId);
//                Assert.Fail();
//            }
//            catch (Exception) {
//                Assert.IsTrue(true);
//            }
//        }

//        [TestMethod]
//        public void SystemTagCarryOverMultInvoke() {
//            IDataListSentinel sent = DataListFactory.CreateSentinel();
//            string result = sent.UpsertSystemTag(TestStrings.sampleADLMixedPopulated, enSystemTag.FormView, "<div>test</div>");

//            result = DataListFactory.CreateSentinel().UpsertSystemTag(result, enSystemTag.Service, "foobar");

//            result = DataListFactory.CreateSentinel().ReshapeDataList(result, TestStrings.sampleADLMixed);

//            result = DataListFactory.CreateSentinel().ReshapeDataList(result, TestStrings.sampleADLMixed);

//            Assert.AreEqual(TestStrings.sysTagMultCarry, result);
//        }

//        [TestMethod]
//        public void TestSystemTagCarryOver() {
//            IDataListSentinel sent = DataListFactory.CreateSentinel();
//            string result = sent.UpsertSystemTag(TestStrings.sampleADLMixedPopulated, enSystemTag.FormView, "<div>test</div>");

//            result = DataListFactory.CreateSentinel().ReshapeDataList(result, TestStrings.sampleADLMixed);

//            Assert.AreEqual(TestStrings.systemTagCarryOverResult, result);
//        }

//        [TestMethod]
//        public void TestCompositeSystemTags() {
//            IDataListSentinel sent = DataListFactory.CreateSentinel();
//            string result = sent.UpsertSystemTag(TestStrings.compositeSystemTagPayload, enSystemTag.FormView, "<div>test</div>");

//            result = DataListFactory.CreateSentinel().ReshapeDataList(result, TestStrings.sampleADLMixed);

//            Assert.AreEqual(TestStrings.compositeSystemTagResult, result);
//        }

//        #endregion

//        #region Reshape Test
//        [TestMethod]
//        public void Label_Nested_Config_Input() {

//            string defs = @"<Inputs><Input Name=""required"" Source="""" /><Input Name=""validationClass"" Source="""" /><Input Name=""cssClass"" Source="""" /><Input Name=""Dev2customStyle"" Source=""[[Dev2customStyleLabel]]"" /></Inputs>";

//            string shape = @"<DataList>
//    <Dev2displayTextLabel Description="""" />
//    <ErrorMsg Description="""" />
//    <Fragment Description="""" />`
//    <Dev2elementNameLabel Description="""" />
//    <cssClass Description="""" />
//    <tabIndex Description="""" />
//    <customStyle Description="""" />
//    <Dev2heightLabel Description="""" />
//    <Dev2widthLabel Description="""" />
//    <Dev2customStyleLabel Description="""" />
//  </DataList>";

//            string result = dlUtil.ShapeInput(TestStrings.label_current_adl, defs, shape);

//            string expected = @"<ADL><required></required><validationClass></validationClass><cssClass></cssClass><Dev2customStyle>advancedRegion</Dev2customStyle></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        [TestMethod]
//        public void DropDown_Recusive_Eval() {

//            string expr = @"{{
//var r = ""[[Dev2DropDownStaticOptions]]"".replace(""<itemCollection>"").replace(""</itemCollection>"",""""); 
// var sOn = ""<item>"";
// var elms = r.split(sOn);
// var res = """";
// 
//  for(var i = 1; i < elms.length; i++){
//   var pos = elms[i].indexOf(""</item>"");
//   var data = elms[i].substr(0, pos);
//   var select = """";
//                                 if(data === ""[[[[Dev2elementNameDropDownList]]]]""){
//                      select=""selected=\""true\"""";
//                                  }
//                        res += '<option value=""'+data+'"" ' + select + '>'+data+'</option>';
//  }
//}}
//";

//            string shape = @"<DataList>
//    <Dev2elementNameDropDownList Description="""" />
//    <ErrorMsg Description="""" />
//    <Dev2tabIndexDropdown Description="""" />
//    <Dev2toolTipdropDown Description="""" />
//    <Dev2customStyledropDown Description="""" />
//    <Dev2widthDropdown Description="""" />
//    <Dev2heightDropdown Description="""" />
//    <Dev2displayTextDropDownList Description="""" />
//    <allowEditDD Description="""" />
//    <showTextDD Description="""" />
//    <requiredDD Description="""" />
//    <Fragment Description="""" />
//    <InjectedLabel Description="""" />
//    <toolTip Description="""" />
//    <tabIndex Description="""" />
//    <cssClass Description="""" />
//    <customStyle Description="""" />
//    <readOnly Description="""" />
//    <InjectStar Description="""" />
//    <Dev2customScriptDD Description="""" />
//    <SelectOptions Description="""" />
//    <BindingData Description="""" />
//    <Dev2FromServiceDD Description="""" />
//    <showText Description="""" />
//    <displayText Description="""" />
//    <Dev2DropDownStaticOptions Description="""" />
//    <DevCustomDataServiceDD Description="""" />
//    <Dev2WebpartType Description="""" />
//    <Dev2CustomDataRowDelimiterDD Description="""" />
//    <Dev2BindingData Description="""" />
//    <Dev2LineBreak Description="""" />
//    <Dev2CustomDataRowDisplayDD Description="""" />
//    <Dev2CustomDataOptionFieldDD Description="""" />
//    <Dev2ServiceParametersDD Description="""" />
//    <Dev2CustomDataService Description="""" />
//    <Dev2CustomDataRowDelimiter Description="""" />
//    <Dev2DisplayElement Description="""" />
//    <Dev2OptionElement Description="""" />
//    <Dev2ServiceParameters Description="""" />
//    <Dev2PartName Description="""" />
//  </DataList>
//  ";

//            string curADL = @"<ADL><Resumption><ParentWorkflowInstanceId>2cdf50b2-129f-41f1-9d14-a61b5a3350ee</ParentWorkflowInstanceId><ParentServiceName>Drop Down List</ParentServiceName></Resumption><Service>InjectLabel_New</Service><WebXMLConfiguration><Dev2DesignTimeBinding>true</Dev2DesignTimeBinding><WebPart><WebPartServiceName>Drop Down List</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>6</RowIndex><Dev2XMLResult><ADL><ADL><Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer><Dev2fromServiceDD>no</Dev2fromServiceDD><listItem>dd/mm/yyyy</listItem><listItem>dd/mm/yy</listItem><listItem>mm/dd/yyyy</listItem><listItem>mm/dd/yy</listItem><listItem>yyyy/mm/dd</listItem><listItem>yy/mm/dd</listItem><Async /></ADL><dropDownWizardElementNameLabel></dropDownWizardElementNameLabel><dropDownWizardDisplayTextLabel></dropDownWizardDisplayTextLabel><dropDownWizardAllowEditLabel></dropDownWizardAllowEditLabel><dropDownWizardFromServiceLabel></dropDownWizardFromServiceLabel><ButtonClicked></ButtonClicked><Dev2customDataAddDD></Dev2customDataAddDD><Dev2customDataDeleteDD></Dev2customDataDeleteDD><tabIndexLabel></tabIndexLabel><tooltipLabel></tooltipLabel><customStyleLabel></customStyleLabel><widthLabel></widthLabel><heightLabel></heightLabel><customScriptLabel></customScriptLabel><ButtonClickedDDAdvanced></ButtonClickedDDAdvanced><Dev2CustomDataRowDelimiterLabel></Dev2CustomDataRowDelimiterLabel><Dev2CustomDataRowDisplayFieldLabel></Dev2CustomDataRowDisplayFieldLabel><Dev2CustomDataOptionFieldLabel></Dev2CustomDataOptionFieldLabel><Dev2CustomDataServiceLabel></Dev2CustomDataServiceLabel><Dev2ServiceInputLabel></Dev2ServiceInputLabel><simpleOptions></simpleOptions><DataSource></DataSource><Dev2Services></Dev2Services><ButtonClickedCancel></ButtonClickedCancel><ButtonClickedDone></ButtonClickedDone></ADL></Dev2XMLResult></WebPart><Dev2WebpartBindingData><ActivityInput><requiredDP /><allowEditDP /><showTextDP /><Name /><datePickerWizardElementNameLabel /><datePickerWizardDisplayTextLabel /><datePickerWizardAllowEditLabel /><Dev2elementNameDatePicker>abc, [[def]]</Dev2elementNameDatePicker><Dev2displayTextDatePicker /><ButtonClickedCancel /><ButtonClickedDone /><tabIndexLabel /><Dev2tabIndexDatepicker /><customStyleLabel /><Dev2customStyledatePicker /><tooltipLabel /><Dev2toolTipdatePicker /><widthLabel /><Dev2widthDatepicker /><heightLabel /><Dev2heightDatepicker /><customScriptLabel /><Dev2customScriptDP /><ButtonClickedDPAdvanced /><Dev2DateFormatLabel /><Dev2DateFormat>mm/dd/yyyy</Dev2DateFormat></ActivityInput></Dev2WebpartBindingData></WebXMLConfiguration><Dev2elementNameDropDownList>Dev2DateFormat</Dev2elementNameDropDownList><ErrorMsg></ErrorMsg><Dev2tabIndexDropdown></Dev2tabIndexDropdown><Dev2toolTipdropDown></Dev2toolTipdropDown><Dev2customStyledropDown>advancedRegion</Dev2customStyledropDown><Dev2widthDropdown></Dev2widthDropdown><Dev2heightDropdown></Dev2heightDropdown><Dev2displayTextDropDownList></Dev2displayTextDropDownList><allowEditDD>yes</allowEditDD><showTextDD>on</showTextDD><requiredDD>on</requiredDD><Fragment></Fragment><InjectedLabel></InjectedLabel><toolTip></toolTip><tabIndex></tabIndex><cssClass>class=""requiredClass advancedRegion""</cssClass><customStyle></customStyle><readOnly></readOnly><InjectStar><font color=""red"">*</font></InjectStar><Dev2customScriptDD></Dev2customScriptDD><SelectOptions></SelectOptions><BindingData></BindingData><Dev2FromServiceDD></Dev2FromServiceDD><showText>on</showText><displayText></displayText><Dev2DropDownStaticOptions><itemCollection><item>dd/mm/yyyy</item><item>dd/mm/yy</item><item>mm/dd/yyyy</item><item>mm/dd/yy</item><item>yyyy/mm/dd</item><item>yy/mm/dd</item></itemCollection></Dev2DropDownStaticOptions><DevCustomDataServiceDD></DevCustomDataServiceDD><Dev2WebpartType></Dev2WebpartType><Dev2CustomDataRowDelimiterDD></Dev2CustomDataRowDelimiterDD><Dev2BindingData></Dev2BindingData><Dev2LineBreak></Dev2LineBreak><Dev2CustomDataRowDisplayDD></Dev2CustomDataRowDisplayDD><Dev2CustomDataOptionFieldDD></Dev2CustomDataOptionFieldDD><Dev2ServiceParametersDD></Dev2ServiceParametersDD><Dev2CustomDataService></Dev2CustomDataService><Dev2CustomDataRowDelimiter></Dev2CustomDataRowDelimiter><Dev2DisplayElement></Dev2DisplayElement><Dev2OptionElement></Dev2OptionElement><Dev2ServiceParameters></Dev2ServiceParameters><Dev2PartName></Dev2PartName></ADL>";

//            string preADL = @"<XmlData>
//  <ADL>
//    <Resumption>
//      <ParentWorkflowInstanceId>2cdf50b2-129f-41f1-9d14-a61b5a3350ee</ParentWorkflowInstanceId>
//      <ParentServiceName>Drop Down List</ParentServiceName>
//    </Resumption>
//    <Service>InjectLabel_New</Service>
//    <WebXMLConfiguration>
//      <Dev2DesignTimeBinding>true</Dev2DesignTimeBinding>
//      <WebPart>
//        <WebPartServiceName>Drop Down List</WebPartServiceName>
//        <ColumnIndex>1</ColumnIndex>
//        <RowIndex>6</RowIndex>
//        <Dev2XMLResult>
//          <ADL>
//            <ADL>
//              <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//              <Dev2fromServiceDD>no</Dev2fromServiceDD>
//              <listItem>dd/mm/yyyy</listItem>
//              <listItem>dd/mm/yy</listItem>
//              <listItem>mm/dd/yyyy</listItem>
//              <listItem>mm/dd/yy</listItem>
//              <listItem>yyyy/mm/dd</listItem>
//              <listItem>yy/mm/dd</listItem>
//              <Async />
//            </ADL>
//            <dropDownWizardElementNameLabel></dropDownWizardElementNameLabel>
//            <dropDownWizardDisplayTextLabel></dropDownWizardDisplayTextLabel>
//            <dropDownWizardAllowEditLabel></dropDownWizardAllowEditLabel>
//            <dropDownWizardFromServiceLabel></dropDownWizardFromServiceLabel>
//            <ButtonClicked></ButtonClicked>
//            <Dev2customDataAddDD></Dev2customDataAddDD>
//            <Dev2customDataDeleteDD></Dev2customDataDeleteDD>
//            <tabIndexLabel></tabIndexLabel>
//            <tooltipLabel></tooltipLabel>
//            <customStyleLabel></customStyleLabel>
//            <widthLabel></widthLabel>
//            <heightLabel></heightLabel>
//            <customScriptLabel></customScriptLabel>
//            <ButtonClickedDDAdvanced></ButtonClickedDDAdvanced>
//            <Dev2CustomDataRowDelimiterLabel></Dev2CustomDataRowDelimiterLabel>
//            <Dev2CustomDataRowDisplayFieldLabel></Dev2CustomDataRowDisplayFieldLabel>
//            <Dev2CustomDataOptionFieldLabel></Dev2CustomDataOptionFieldLabel>
//            <Dev2CustomDataServiceLabel></Dev2CustomDataServiceLabel>
//            <Dev2ServiceInputLabel></Dev2ServiceInputLabel>
//            <simpleOptions></simpleOptions>
//            <DataSource></DataSource>
//            <Dev2Services></Dev2Services>
//            <ButtonClickedCancel></ButtonClickedCancel>
//            <ButtonClickedDone></ButtonClickedDone>
//          </ADL>
//        </Dev2XMLResult>
//      </WebPart>
//      <Dev2WebpartBindingData>
//        <ActivityInput>
//          <requiredDP />
//          <allowEditDP />
//          <showTextDP />
//          <Name />
//          <datePickerWizardElementNameLabel />
//          <datePickerWizardDisplayTextLabel />
//          <datePickerWizardAllowEditLabel />
//          <Dev2elementNameDatePicker>abc, [[def]]</Dev2elementNameDatePicker>
//          <Dev2displayTextDatePicker />
//          <ButtonClickedCancel />
//          <ButtonClickedDone />
//          <tabIndexLabel />
//          <Dev2tabIndexDatepicker />
//          <customStyleLabel />
//          <Dev2customStyledatePicker />
//          <tooltipLabel />
//          <Dev2toolTipdatePicker />
//          <widthLabel />
//          <Dev2widthDatepicker />
//          <heightLabel />
//          <Dev2heightDatepicker />
//          <customScriptLabel />
//          <Dev2customScriptDP />
//          <ButtonClickedDPAdvanced />
//          <Dev2DateFormatLabel />
//          <Dev2DateFormat>mm/dd/yyyy</Dev2DateFormat>
//        </ActivityInput>
//      </Dev2WebpartBindingData>
//    </WebXMLConfiguration>
//    <Dev2elementNameDropDownList>Dev2DateFormat</Dev2elementNameDropDownList>
//    <ErrorMsg></ErrorMsg>
//    <Dev2tabIndexDropdown></Dev2tabIndexDropdown>
//    <Dev2toolTipdropDown></Dev2toolTipdropDown>
//    <Dev2customStyledropDown>advancedRegion</Dev2customStyledropDown>
//    <Dev2widthDropdown></Dev2widthDropdown>
//    <Dev2heightDropdown></Dev2heightDropdown>
//    <Dev2displayTextDropDownList></Dev2displayTextDropDownList>
//    <allowEditDD>yes</allowEditDD>
//    <showTextDD>on</showTextDD>
//    <requiredDD>on</requiredDD>
//    <Fragment></Fragment>
//    <toolTip></toolTip>
//    <tabIndex></tabIndex>
//    <cssClass>class=""requiredClass advancedRegion""</cssClass>
//    <customStyle></customStyle>
//    <readOnly></readOnly>
//    <InjectStar>
//      <font color=""red"">*</font>
//    </InjectStar>
//    <Dev2customScriptDD></Dev2customScriptDD>
//    <SelectOptions></SelectOptions>
//    <BindingData></BindingData>
//    <Dev2FromServiceDD></Dev2FromServiceDD>
//    <Dev2DropDownStaticOptions>
//      <itemCollection>
//        <item>dd/mm/yyyy</item>
//        <item>dd/mm/yy</item>
//        <item>mm/dd/yyyy</item>
//        <item>mm/dd/yy</item>
//        <item>yyyy/mm/dd</item>
//        <item>yy/mm/dd</item>
//      </itemCollection>
//    </Dev2DropDownStaticOptions>
//    <DevCustomDataServiceDD></DevCustomDataServiceDD>
//    <Dev2WebpartType></Dev2WebpartType>
//    <Dev2CustomDataRowDelimiterDD></Dev2CustomDataRowDelimiterDD>
//    <Dev2BindingData></Dev2BindingData>
//    <Dev2LineBreak></Dev2LineBreak>
//    <Dev2CustomDataRowDisplayDD></Dev2CustomDataRowDisplayDD>
//    <Dev2CustomDataOptionFieldDD></Dev2CustomDataOptionFieldDD>
//    <Dev2ServiceParametersDD></Dev2ServiceParametersDD>
//    <Dev2CustomDataService></Dev2CustomDataService>
//    <Dev2CustomDataRowDelimiter></Dev2CustomDataRowDelimiter>
//    <Dev2DisplayElement></Dev2DisplayElement>
//    <Dev2OptionElement></Dev2OptionElement>
//    <Dev2ServiceParameters></Dev2ServiceParameters>
//    <Dev2PartName></Dev2PartName>
//    <InjectedLabel />
//    <showText>on</showText>
//    <displayText />
//  </ADL>
//</XmlData>";

//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(expr, shape, curADL, preADL);
//            string expected = @"<option value=""dd/mm/yyyy"" >dd/mm/yyyy</option><option value=""dd/mm/yy"" >dd/mm/yy</option><option value=""mm/dd/yyyy"" selected=""true"">mm/dd/yyyy</option><option value=""mm/dd/yy"" >mm/dd/yy</option><option value=""yyyy/mm/dd"" >yyyy/mm/dd</option><option value=""yy/mm/dd"" >yy/mm/dd</option>";

//            Assert.AreEqual(expected, result);
//        }

//        [TestMethod]
//        public void TestReshapScalar() {
//            string currentDataList = TestStrings.sampleADLMixedPopulated;
//            string shape = TestStrings.sampleADLScalar;

//            string result = DataListFactory.CreateSentinel().ReshapeDataList(currentDataList, shape);

//            Assert.AreEqual(TestStrings.reshapedScalarADLResult, result);
//        }

//        [TestMethod]
//        public void TestReshapRecordSet() {
//            string currentDataList = TestStrings.sampleADLMixedPopulated;
//            string shape = TestStrings.sampleADLRecordSet;

//            string result = DataListFactory.CreateSentinel().ReshapeDataList(currentDataList, shape);

//            Assert.AreEqual(TestStrings.reshapedRecordSetADLResult, result);
//        }

//        [TestMethod]
//        public void TestReshapeMixed() {
//            string currentDataList = TestStrings.sampleADLMixedPopulated;
//            string shape = TestStrings.sampleADLMixed;

//            string result = DataListFactory.CreateSentinel().ReshapeDataList(currentDataList, shape);

//            Assert.AreEqual(TestStrings.reshapedMixedADLResult, result);
//        }

//        [TestMethod]
//        public void Sample_BDS_Invoke() {
//            string currentDataList = "<ADL><x></x><JSON>abc</JSON></ADL>";
//            string shape = "<ADL><JSON Description=\"\"/></ADL>";

//            string result = DataListFactory.CreateSentinel().ReshapeDataList(currentDataList, shape);

//            Assert.AreEqual("<ADL><JSON>abc</JSON></ADL>", result);
//        }
//        #endregion

//        #region Dev2DefinitionToShape

//        [TestMethod]
//        public void Dev2InputToScalarShape() {
//            string result = DataListFactory.CreateSentinel().ShapeDev2DefinitionsToDataList(TestStrings.sampleActivityInputScalar, enDev2ArgumentType.Input);

//            Assert.AreEqual(TestStrings.Dev2InputScalarShapedToADL, result);
//        }

//        [TestMethod]
//        public void Dev2OutputToScalarShape() {
//            string result = DataListFactory.CreateSentinel().ShapeDev2DefinitionsToDataList(TestStrings.sampleActivityMappingScalar, enDev2ArgumentType.Output);

//            Assert.AreEqual(TestStrings.Dev2OutputScalarToADL, result);
//        }

//        [TestMethod]
//        public void Dev2OutputToRecordSetShape() {
//            string result = DataListFactory.CreateSentinel().ShapeDev2DefinitionsToDataList(TestStrings.sampleActivityMappingRecordSets, enDev2ArgumentType.Output);

//            Assert.AreEqual(TestStrings.Dev2OutputRecordSetToADL, result);
//        }

//        [TestMethod] // ReTest
//        public void Dev2OutputToMixedShape() {
//            string result = DataListFactory.CreateSentinel().ShapeDev2DefinitionsToDataList(TestStrings.sampleActivityMappingMixed, enDev2ArgumentType.Output);

//            Assert.AreEqual(TestStrings.Dev2OutputMixedToADL, result);
//        }

//        #endregion

//        #region DataList To IO Mapping
//        [TestMethod]
//        public void CreateInputMappingFromDataList() {
//            string result = DataListFactory.GenerateMappingFromDataList(TestStrings.Dev2OutputMixedToADL, enDev2ArgumentType.Input);

//            Assert.AreEqual(TestStrings.dataListToInputMappingResult, result);
//        }

//        [TestMethod]
//        public void CreateOutputMappingFromDataList() {
//            string result = DataListFactory.GenerateMappingFromDataList(TestStrings.Dev2OutputMixedToADL, enDev2ArgumentType.Output);

//            Assert.AreEqual(TestStrings.dataListToOutputMappingResult, result);
//        }

//        [TestMethod]
//        public void CreateInputMappingFromWebpage() {
//            string result = DataListFactory.GenerateMappingFromWebpage(TestStrings.Dev2WebpageXMLConfiguration, enDev2ArgumentType.Input);
//            string expected = XElement.Parse(TestStrings.Dev2WebpageToInputMappingResult).ToString();
//            string actual = XElement.Parse(result).ToString();
//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void CreateOutputMappingFromWebpage() {
//            string result = DataListFactory.GenerateMappingFromWebpage(TestStrings.Dev2WebpageXMLConfiguration, enDev2ArgumentType.Output);
//            string expected = XElement.Parse(TestStrings.Dev2WebpageToOutputMappingResult).ToString();
//            string actual = XElement.Parse(result).ToString();
//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void WebpageXML_ExtractElements_ExpectMultipleElements() {
//            string returnValue = string.Empty;
//            var displayNameNodeMatch = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(TestStrings.Dev2WebpageXMLConfiguration).xmlData as XElement;
//            string displayNameNodeName = "dev2elementname";
//            var displayNameNodes = displayNameNodeMatch.DescendantsAndSelf().Where(c => c.Name.ToString().ToUpper().Contains(displayNameNodeName.ToUpper()));
//            Assert.IsTrue(displayNameNodes.Any());
//        }

//        [TestMethod]
//        public void ShapeInput_WithBindingDataAndResumptionTags_Expected_DataBindingCorrectly() {
//            string currentADL = @"<Dev2ServiceInput>
//  <XmlData>
//    <ActivityInput>
//      <DatabaseType />
//      <Error />
//      <Result />
//      <XML />
//      <Dev2DatabaseServiceSetupInformationSchema />
//      <InformationSchemaResult />
//      <Dev2DatabaseServiceSetupSourceLabel />
//      <Dev2DatabaseServiceSetupFilterLabel />
//      <Dev2DatabaseServiceSetupFilter />
//      <Dev2DatabaseServiceSetupActivityLabel />
//      <Dev2DatabaseServicSetupCancelButton />
//      <Dev2DatabaseServiceSetupBackButton />
//      <Dev2DatabaseServiceSetupDoneButton />
//      <Dev2DatabaseServiceSetupNextButton />
//      <ParameterName />
//      <Dev2ServiceDetailsSource>Cities Database</Dev2ServiceDetailsSource>
//      <Dev2ServiceDetailsName>teteeet</Dev2ServiceDetailsName>
//    </ActivityInput>
//  </XmlData>
//  <Resumption>
//    <ParentWorkflowInstanceId>7c275c3e-2327-4f6f-83da-c10b15ab2a15</ParentWorkflowInstanceId>
//    <ParentServiceName>NewDatabaseService</ParentServiceName>
//  </Resumption>
//  <Service>DatabaseServiceSetup</Service>
//  <Async />
//</Dev2ServiceInput>";
//            string inputMapping = @"<Inputs><Input Name=""Dev2ServiceDetailsName"" Source=""[[Dev2ServiceDetailsName]]"" /><Input Name=""Dev2ServiceDetailsSource"" Source=""[[Dev2ServiceDetailsSource]]"" /><Input Name=""DatabaseType"" Source="""" /><Input Name=""Dev2DatabaseServiceSetupInformationSchema"" Source=""[[Dev2DatabaseServiceSetupInformationSchema]]"" /><Input Name=""Dev2DatabaseServiceSetupSourceLabel"" Source="""" /><Input Name=""Dev2DatabaseServiceSetupFilterLabel"" Source="""" /><Input Name=""Dev2DatabaseServiceSetupFilter"" Source="""" /><Input Name=""Dev2DatabaseServicSetupCancelButton"" Source="""" /><Input Name=""Dev2DatabaseServiceSetupBackButton"" Source="""" /><Input Name=""Dev2DatabaseServiceSetupDoneButton"" Source="""" /><Input Name=""Dev2DatabaseServiceSetupNextButton"" Source="""" /></Inputs>";
//            string dataList = @"<DataList>
//    <DatabaseType Description="""" />
//    <Error Description="""" />
//    <Result Description="""" />
//    <XML Description="""" />
//    <Dev2DatabaseServiceSetupInformationSchema Description="""" />
//    <InformationSchemaResult Description="""" />
//    <Dev2DatabaseServiceSetupSourceLabel Description="""" />
//    <Dev2DatabaseServiceSetupFilterLabel Description="""" />
//    <Dev2DatabaseServiceSetupFilter Description="""" />
//    <Dev2DatabaseServiceSetupActivityLabel Description="""" />
//    <Dev2DatabaseServicSetupCancelButton Description="""" />
//    <Dev2DatabaseServiceSetupBackButton Description="""" />
//    <Dev2DatabaseServiceSetupDoneButton Description="""" />
//    <Dev2DatabaseServiceSetupNextButton Description="""" />
//    <ProcedureParameters Description="""">
//      <ParameterName Description="""" />
//    </ProcedureParameters>
//    <Dev2ServiceDetailsSource Description="""" />
//    <Dev2ServiceDetailsName Description="""" />
//  </DataList>";
//            string actual = target.ShapeInput(currentADL, inputMapping, dataList);

//            string expected = @"<ADL><Dev2ServiceDetailsName>teteeet</Dev2ServiceDetailsName><Dev2ServiceDetailsSource>Cities Database</Dev2ServiceDetailsSource><DatabaseType></DatabaseType><Dev2DatabaseServiceSetupInformationSchema></Dev2DatabaseServiceSetupInformationSchema><Dev2DatabaseServiceSetupSourceLabel></Dev2DatabaseServiceSetupSourceLabel><Dev2DatabaseServiceSetupFilterLabel></Dev2DatabaseServiceSetupFilterLabel><Dev2DatabaseServiceSetupFilter></Dev2DatabaseServiceSetupFilter><Dev2DatabaseServicSetupCancelButton></Dev2DatabaseServicSetupCancelButton><Dev2DatabaseServiceSetupBackButton></Dev2DatabaseServiceSetupBackButton><Dev2DatabaseServiceSetupDoneButton></Dev2DatabaseServiceSetupDoneButton><Dev2DatabaseServiceSetupNextButton></Dev2DatabaseServiceSetupNextButton></ADL>";

//            Assert.AreEqual(expected, actual.Trim());

//        }

//        [TestMethod]
//        public void TextBox_HtmlWidget_Binding() {
//            string curADL = @"<Dev2ServiceInput>
//  <WebXMLConfiguration>
//    <WebPart>
//      <WebPartServiceName>HtmlWidget</WebPartServiceName>
//      <ColumnIndex>1</ColumnIndex>
//      <RowIndex>1</RowIndex>
//      <Dev2XMLResult>
//        <ADL>
//          <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//          <Name></Name>
//          <Value>
//            <span>
//              <input type=""text"" id=""Dev2elementName"" name=""Dev2elementName"" value=""[[Dev2elementName]]"" class=""requiredClass"" />
//              <font color=""red"">*</font>
//            </span>
//          </Value>
//          <cssClass></cssClass>
//          <Async />
//        </ADL>
//      </Dev2XMLResult>
//    </WebPart>
//    <Dev2WebpartBindingData>
//      <XmlData>
//        <ActivityInput>
//          <Fragment><![CDATA[]]></Fragment>
//          <Dev2elementName>abc</Dev2elementName>
//          <partValidationName>validationTB</partValidationName>
//        </ActivityInput>
//      </XmlData>
//    </Dev2WebpartBindingData>
//  </WebXMLConfiguration>
//  <Service>HtmlWidget</Service>
//</Dev2ServiceInput>";

//            string shape = @"<DataList>
//    <DataValue Description="""" />
//    <cssClass Description="""" />
//    <Fragment Description="""" />
//    <Value Description="""" />
//  </DataList>";

//            string result = dlUtil.ShapeInput(curADL, null, shape);

//            string expectd = @"<ADL><Service>HtmlWidget</Service><WebXMLConfiguration><WebPart><WebPartServiceName>HtmlWidget</WebPartServiceName><ColumnIndex>1</ColumnIndex><RowIndex>1</RowIndex><Dev2XMLResult><ADL><Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer><Name></Name><Async /></ADL></Dev2XMLResult></WebPart><Dev2WebpartBindingData><ActivityInput><Dev2elementName>abc</Dev2elementName><partValidationName>validationTB</partValidationName></ActivityInput></Dev2WebpartBindingData></WebXMLConfiguration><DataValue></DataValue><cssClass></cssClass><Fragment><![CDATA[]]></Fragment><Value><span><input type=""text"" id=""Dev2elementName"" name=""Dev2elementName"" value=""[[Dev2elementName]]"" class=""requiredClass"" /><font color=""red"">*</font></span></Value></ADL>";

//            Assert.AreEqual(expectd, result.Trim());

//        }

//        #endregion

//        #region DataList From Defs
//        [TestMethod]
//        public void DataList_From_Defs_Scalar() {
//            // GenerateDataListFromDefs

//            IList<IDev2Definition> defs = new List<IDev2Definition>();

//            defs.Add(DataListFactory.CreateDefinition("test1", "test1a", "test1b", false, "", false, "test1b"));
//            defs.Add(DataListFactory.CreateDefinition("test2", "test2a", "test2b", false, "", false, "test2b"));

//            string result = DataListFactory.CreateDataListCompiler().GenerateDataListFromDefs(defs);

//            Assert.AreEqual(result.Replace(Environment.NewLine, ""), @"<ADL><test1/><test2/></ADL>");
//        }

//        [TestMethod]
//        public void DataList_From_Defs_Recordset() {
//            // GenerateDataListFromDefs

//            IList<IDev2Definition> defs = new List<IDev2Definition>();

//            defs.Add(DataListFactory.CreateDefinition("test1", "test1a", "test1b", "testRS", false, "", false, "test1b", false));
//            defs.Add(DataListFactory.CreateDefinition("test2", "test2a", "test2b", "testRS", false, "", false, "test2b", false));

//            string result = DataListFactory.CreateDataListCompiler().GenerateDataListFromDefs(defs);

//            Assert.AreEqual(result.Replace(Environment.NewLine, ""), @"<ADL><testRS><test1/><test2/></testRS></ADL>");
//        }

//        [TestMethod]
//        public void DataList_From_Defs_Recordset_And_Scalar() {
//            // GenerateDataListFromDefs

//            IList<IDev2Definition> defs = new List<IDev2Definition>();

//            defs.Add(DataListFactory.CreateDefinition("test1", "test1a", "test1b", "testRS", false, "", false, "test1b", false));
//            defs.Add(DataListFactory.CreateDefinition("test2", "test2a", "test2b", "testRS", false, "", false, "test2b", false));

//            defs.Add(DataListFactory.CreateDefinition("test3", "test3a", "test3b", false, "", false, "test3b"));

//            string result = DataListFactory.CreateDataListCompiler().GenerateDataListFromDefs(defs);

//            Assert.AreEqual(result.Replace(Environment.NewLine, ""), @"<ADL><test3/><testRS><test1/><test2/></testRS></ADL>");
//        }

//        #endregion

//        #region Shape Methods
//        [TestMethod]
//        public void Mult_Resume_Shape_Input() {
//            string currentADL = ParserStrings.input_shape_mult_resume_adl;
//            string shape = ParserStrings.input_shape_mult_resume_dl;

//            string result = dlUtil.ShapeInput(currentADL, null, shape);


//            Assert.AreEqual(ParserStrings.input_shape_mult_resume_result, result.Trim());

//        }


//        // Travis.Frisinger - Added For Test : Removed for production since other test already cover this feature
//        [TestMethod]
//        public void Webpage_Resumption_Avoids_Nested_RS_Creation_Expect_WellformedDL(){
//            string currentADL = ParserStrings.webpageResumptionDL;
//            IDataListCompiler c = new DataListCompiler();
//            string defs = null;

//            string shape = @"<DataList>
//    <Dev2EntryPoint Description="""" />
//    <Dev2StepCount Description="""" />
//    <Dev2Heading Description="""" />
//    <Dev2Step1 Description="""" />
//    <Dev2Steps Description="""" />
//    <Dev2Back Description="""" />
//    <Dev2Next Description="""" />
//    <Dev2Step2 Description="""" />
//    <Dev2Step3 Description="""" />
//    <Dev2Step4 Description="""" />
//    <Dev2Step5 Description="""" />
//    <Dev2GoNext Description="""" />
//    <Dev2Failed Description="""" />
//    <Dev2Passed Description="""" />
//    <Dev2Step6 Description="""" />
//  </DataList>";

//            string result = c.ShapeOutput(currentADL, "<ADL></ADL>", defs, shape);

//            string expected = "<ADL><Resumption><ParentWorkflowInstanceId>86ede153-ae30-4aca-9c63-534da99f1f59</ParentWorkflowInstanceId><ParentServiceName>Release Process</ParentServiceName></Resumption><Bookmark>dsfResumption</Bookmark><InstanceId>a7bac018-01b1-4e32-9e07-d70047331c49</InstanceId><Dev2ResumeData><Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer><Async/></Dev2ResumeData><Dev2Passed>Passed</Dev2Passed><Dev2EntryPoint/><Dev2StepCount>1</Dev2StepCount><Dev2Heading/><Dev2Step1/><Dev2Steps/><Dev2Back/><Dev2Next/><Dev2Step2/><Dev2Step3/><Dev2Step4/><Dev2Step5/><Dev2GoNext/><Dev2Failed/><Dev2Step6/></ADL>";

//            Assert.AreEqual(expected, result);
//        }

//        [TestMethod]
//        public void Transfer_Execution_Result_Into_Shape() {
//            string currentADL = ParserStrings.dlOutputMappingADL;
//            string preExeADL = ParserStrings.dlOutputMappingPreExecute;
//            //string outputs = ParserStrings.dlOutputMappingOutMapping;
//            string dl = ParserStrings.dlOutputDataList;

//            string result = dlUtil.ShapeOutput(currentADL, preExeADL, "", dl);

//            Assert.AreEqual(ParserStrings.dlOutputMappingResult, result);
//        }

//        [TestMethod]
//        public void Transfer_Execution_Result_Into_Shape_When_Is_FormView() {
//            string currentADL = ParserStrings.formViewResult;
//            string preExeADL = ParserStrings.formViewPreExeResult;
//            string outputs = null;
//            string dl = ParserStrings.formViewDL;


//            string result = dlUtil.ShapeOutput(currentADL, preExeADL, outputs, dl);

//            Assert.AreEqual(ParserStrings.formViewExecutionResult, result);
//        }

//        [TestMethod]
//        public void Single_Tag_DL_Strip_Mult() {
//            string currentADL = ParserStrings.dlInputADL;
//            string dl = ParserStrings.dlInputDataList;

//            string result = dlUtil.ShapeInput(currentADL, null, dl);

//            string expected = @"<ADL><Service>StyleInject</Service><WebServerUrl>http://localhost:1234/services/Label_Wizard/instances/1558c9b6-91aa-4b92-9c9a-bcca411b46f3/bookmarks/dsfResumption</WebServerUrl><WebXMLConfiguration><WebPart><WebPartServiceName>Label</WebPartServiceName><ColumnIndex>0</ColumnIndex><RowIndex>4</RowIndex><Dev2XMLResult><Dev2tabIndexLabel /><Dev2widthLabel /><Async /></Dev2XMLResult></WebPart><Dev2WebpartBindingData><ActivityInput></ActivityInput></Dev2WebpartBindingData></WebXMLConfiguration><Dev2displayTextLabel>Tab Index</Dev2displayTextLabel><ErrorMsg></ErrorMsg><Fragment></Fragment><Dev2elementNameLabel>tabIndexLabel</Dev2elementNameLabel><cssClass>class=""advancedRegion""</cssClass><tabIndex></tabIndex><customStyle></customStyle><Dev2heightLabel></Dev2heightLabel><Dev2widthLabel></Dev2widthLabel><Dev2customStyleLabel>advancedRegion</Dev2customStyleLabel></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        [TestMethod]
//        public void Single_Tag_DL_Strip_From_Sys_Tags() {
//            string currentADL = ParserStrings.dlInputClean;
//            string dl = ParserStrings.dlInputDataList;

//            string result = dlUtil.ShapeInput(currentADL, null, dl);

//            Assert.AreEqual(ParserStrings.dlInputResult, result.Trim());
//        }
//        [TestMethod]
//        public void Transfer_System_Tags() {
//            string currentADL = ParserStrings.dlOutputMappingADL;

//            string result = dlUtil.ExtractSystemTagRegion(currentADL, ParserStrings.dlOutputDataList);

//            Assert.AreEqual(ParserStrings.sysTagTransferResult, result);
//        }

//        [TestMethod]
//        public void Input_Shaped_Via_Defs() {

//            string result = dlUtil.ShapeInput(TestStrings.inputShappingCurrentADL, TestStrings.inputShappingDefs, TestStrings.inputShappingDL);

//            string expected = @"<ADL><required></required><validationClass></validationClass><cssClass></cssClass><Dev2customStyle>testButtonCustomStyle</Dev2customStyle></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }

//        [TestMethod]
//        public void Input_With_Nested_Data() {
//            string mapping = @"<Inputs><Input Name=""Dev2tabIndex"" Source=""[[Dev2tabIndexButton]]"" /><Input Name=""tabIndex"" Source="""" /></Inputs>";


//            string result = dlUtil.ShapeInput(TestStrings.input_mapping_with_blank_adl, mapping, TestStrings.input_mapping_with_blank_shape);

//            string expected = @"<ADL><Dev2tabIndex>1</Dev2tabIndex><tabIndex></tabIndex></ADL>";

//            Assert.AreEqual(expected, result.Trim());
//        }


//        [TestMethod]
//        public void Resume_Tag_Matched_Transfered_And_Deleted() {
//            string currentADL = @"<XmlData>
//  <XmlData />
//  <Dev2ResumeData>
//    <XmlData>
//      <InstanceId>fb4a013c-1bfa-4c51-957e-8866062449ca</InstanceId>
//      <Bookmark>dsfResumption</Bookmark>
//      <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//      <XmlData>
//        <Dev2elementName>abc</Dev2elementName>
//        <Dev2displayText>123</Dev2displayText>
//        <allowEditTB>yes</allowEditTB>
//        <Dev2validationErrMsgTB>asdf</Dev2validationErrMsgTB>
//        <Dev2customScriptTB></Dev2customScriptTB>
//      </XmlData>
//      <Async />
//    </XmlData>
//  </Dev2ResumeData>
//</XmlData>";

//            string dl = @"<DataList>
//    <Dev2elementName Description="""" />
//    <partValidationName Description="""" />
//    <textboxWizardDisplayTextLabel Description="""" />
//    <textboxWizardAllowEditLabel Description="""" />
//    <ButtonClickedTBAdvanced Description="""" />
//    <tabIndexLabel Description="""" />
//    <textboxWizardElementNameLabel Description="""" />
//    <validationLabel Description="""" />
//    <heightLabel Description="""" />
//    <widthLabel Description="""" />
//    <customStyleLabel Description="""" />
//    <tooltipLabel Description="""" />
//    <maxCharsLabel Description="""" />
//    <autocompleteLabel Description="""" />
//    <customScriptLabel Description="""" />
//    <Dev2customScriptTB Description="""" />
//    <Dev2autoCompleteLabel Description="""" />
//    <dataRowDelimTBLabel Description="""" />
//    <displayFieldNameLabel Description="""" />
//    <relativeFieldNameLabel Description="""" />
//    <Dev2LinkedInfoLabel Description="""" />
//    <Dev2LinkeInfoLabel2 Description="""" />
//    <validationOptionsInjection Description="""" />
//    <ButtonClickedCancel Description="""" />
//    <ButtonClickedDone Description="""" />
//    <ABC Description="""" />
//    <Fragment Description="""" />
//  </DataList>";

//            string preADL = "<ADL></ADL>";

//            string result = dlUtil.ShapeOutput(currentADL, preADL, "<Outputs/>", dl);

//            Assert.AreEqual(TestStrings.resume_transfer_and_delete_result, result);

//        }

//        [TestMethod]
//        public void Nested_HTML_Fragment_Is_Ignored_By_ReorganizeCurrentADLWithShape() {

//            string currentADL = @"<sr>
//  <sr>
//    <ADL>
//      <Resumption>
//        <ParentWorkflowInstanceId>88d807aa-f3ca-4e22-9118-e4b7a8b54c48</ParentWorkflowInstanceId>
//        <ParentServiceName>File</ParentServiceName>
//      </Resumption>
//      <Service>InjectRequiredTracking</Service>
//      <required>on</required>
//      <InjectStar>
//        <font color=""red"">*</font>
//      </InjectStar>
//    </ADL>
//  </sr>
//</sr>";

//            string preADL = @"<XmlData>
//  <ADL>
//    <Resumption>
//      <ParentWorkflowInstanceId>88d807aa-f3ca-4e22-9118-e4b7a8b54c48</ParentWorkflowInstanceId>
//      <ParentServiceName>File</ParentServiceName>
//    </Resumption>
//    <Service>InjectLabel_New</Service>
//    <WebServerUrl>http://localhost:1234/services/File?lt;DataListgt;lt;Dev2elementNameFile Description=""gt;testFileUploadElementNamelt;/Dev2elementNameFilegt;lt;ErrorMsg Description=""gt;testFileUploadErrorMessagelt;/ErrorMsggt;lt;transformedData Description="" /gt;lt;fileDisplayText Description=""gt;testFileUploadDisplayTextlt;/fileDisplayTextgt;lt;Dev2tabIndexfile Description=""gt;1lt;/Dev2tabIndexfilegt;lt;Dev2toolTipfile Description=""gt;testFileUploadToolTiplt;/Dev2toolTipfilegt;lt;Dev2customStylefile Description=""gt;testFileUploadCustomStylelt;/Dev2customStylefilegt;lt;Dev2heightFile Description=""gt;4lt;/Dev2heightFilegt;lt;Dev2widthFile Description=""gt;4lt;/Dev2widthFilegt;lt;showTextF Description=""gt;onlt;/showTextFgt;lt;requiredF Description=""gt;onlt;/requiredFgt;lt;Fragment Description=""gt;testFileUploadFragmentlt;/Fragmentgt;lt;accept Description="" /gt;lt;toolTip Description="" /gt;lt;tabIndex Description="" /gt;lt;cssClass Description="" /gt;lt;customStyle Description="" /gt;lt;InjectedLabel Description=""gt;testFileUploadInjectedLabellt;/InjectedLabelgt;lt;InjectStar Description=""gt;testFileUploadInjectStarlt;/InjectStargt;lt;Dev2customScriptF Description=""gt;testFileUploadCustomScriptlt;/Dev2customScriptFgt;lt;Dev2FileTypes Description=""gt;testFileUploadFileTypeslt;/Dev2FileTypesgt;lt;validationClass Description="" /gt;lt;/DataListgt;</WebServerUrl>
//    <Dev2elementNameFile>testFileUploadElementName</Dev2elementNameFile>
//    <ErrorMsg>testFileUploadErrorMessage</ErrorMsg>
//    <transformedData></transformedData>
//    <fileDisplayText>testFileUploadDisplayText</fileDisplayText>
//    <Dev2tabIndexfile>1</Dev2tabIndexfile>
//    <Dev2toolTipfile>testFileUploadToolTip</Dev2toolTipfile>
//    <Dev2customStylefile>testFileUploadCustomStyle</Dev2customStylefile>
//    <Dev2heightFile>4</Dev2heightFile>
//    <Dev2widthFile>4</Dev2widthFile>
//    <showTextF>on</showTextF>
//    <requiredF>on</requiredF>
//    <Fragment>testFileUploadFragment</Fragment>
//    <accept>accept=""testFileUploadFileTypes/*""</accept>
//    <toolTip>title=""testFileUploadToolTip""</toolTip>
//    <tabIndex>tabindex=""1""</tabIndex>
//    <cssClass>class=""requiredClass testFileUploadCustomStyle""</cssClass>
//    <customStyle>style=""height:4px;width:4px;""</customStyle>
//    <InjectStar>testFileUploadInjectStar</InjectStar>
//    <Dev2customScriptF>testFileUploadCustomScript</Dev2customScriptF>
//    <Dev2FileTypes>testFileUploadFileTypes</Dev2FileTypes>
//    <validationClass></validationClass>
//    <InjectedLabel>testFileUploadDisplayText</InjectedLabel>
//    <showText>on</showText>
//    <displayText>testFileUploadDisplayText</displayText>
//    <Resumption>
//      <ParentWorkflowInstanceId>88d807aa-f3ca-4e22-9118-e4b7a8b54c48</ParentWorkflowInstanceId>
//      <ParentServiceName>File</ParentServiceName>
//    </Resumption>
//  </ADL>
//</XmlData>";

//            string defs = @"<Outputs><Output Name=""required"" MapsTo=""required"" Value="""" /><Output Name=""InjectStar"" MapsTo=""InjectStar"" Value=""[[InjectStar]]"" /></Outputs>";

//            string shape = @"<DataList>
//    <Dev2elementNameFile Description="""" />
//    <ErrorMsg Description="""" />
//    <transformedData Description="""" />
//    <fileDisplayText Description="""" />
//    <Dev2tabIndexfile Description="""" />
//    <Dev2toolTipfile Description="""" />
//    <Dev2customStylefile Description="""" />
//    <Dev2heightFile Description="""" />
//    <Dev2widthFile Description="""" />
//    <showTextF Description="""" />
//    <requiredF Description="""" />
//    <Fragment Description="""" />
//    <accept Description="""" />
//    <toolTip Description="""" />
//    <tabIndex Description="""" />
//    <cssClass Description="""" />
//    <customStyle Description="""" />
//    <InjectedLabel Description="""" />
//    <InjectStar Description="""" />
//    <Dev2customScriptF Description="""" />
//    <Dev2FileTypes Description="""" />
//    <validationClass Description="""" />
//    <showText Description="""" />
//    <displayText Description="""" />
//  </DataList>";

//            string result = dlUtil.ShapeOutput(currentADL, preADL, defs, shape);

//            string expected = @"<ADL><Resumption><ParentWorkflowInstanceId>88d807aa-f3ca-4e22-9118-e4b7a8b54c48</ParentWorkflowInstanceId><ParentServiceName>File</ParentServiceName></Resumption><Service>InjectLabel_New</Service><WebServerUrl>http://localhost:1234/services/File?lt;DataListgt;lt;Dev2elementNameFile Description=""gt;testFileUploadElementNamelt;/Dev2elementNameFilegt;lt;ErrorMsg Description=""gt;testFileUploadErrorMessagelt;/ErrorMsggt;lt;transformedData Description="" /gt;lt;fileDisplayText Description=""gt;testFileUploadDisplayTextlt;/fileDisplayTextgt;lt;Dev2tabIndexfile Description=""gt;1lt;/Dev2tabIndexfilegt;lt;Dev2toolTipfile Description=""gt;testFileUploadToolTiplt;/Dev2toolTipfilegt;lt;Dev2customStylefile Description=""gt;testFileUploadCustomStylelt;/Dev2customStylefilegt;lt;Dev2heightFile Description=""gt;4lt;/Dev2heightFilegt;lt;Dev2widthFile Description=""gt;4lt;/Dev2widthFilegt;lt;showTextF Description=""gt;onlt;/showTextFgt;lt;requiredF Description=""gt;onlt;/requiredFgt;lt;Fragment Description=""gt;testFileUploadFragmentlt;/Fragmentgt;lt;accept Description="" /gt;lt;toolTip Description="" /gt;lt;tabIndex Description="" /gt;lt;cssClass Description="" /gt;lt;customStyle Description="" /gt;lt;InjectedLabel Description=""gt;testFileUploadInjectedLabellt;/InjectedLabelgt;lt;InjectStar Description=""gt;testFileUploadInjectStarlt;/InjectStargt;lt;Dev2customScriptF Description=""gt;testFileUploadCustomScriptlt;/Dev2customScriptFgt;lt;Dev2FileTypes Description=""gt;testFileUploadFileTypeslt;/Dev2FileTypesgt;lt;validationClass Description="" /gt;lt;/DataListgt;</WebServerUrl><Dev2elementNameFile>testFileUploadElementName</Dev2elementNameFile><ErrorMsg>testFileUploadErrorMessage</ErrorMsg><transformedData></transformedData><fileDisplayText>testFileUploadDisplayText</fileDisplayText><Dev2tabIndexfile>1</Dev2tabIndexfile><Dev2toolTipfile>testFileUploadToolTip</Dev2toolTipfile><Dev2customStylefile>testFileUploadCustomStyle</Dev2customStylefile><Dev2heightFile>4</Dev2heightFile><Dev2widthFile>4</Dev2widthFile><showTextF>on</showTextF><requiredF>on</requiredF><Fragment>testFileUploadFragment</Fragment><accept>accept=""testFileUploadFileTypes/*""</accept><toolTip>title=""testFileUploadToolTip""</toolTip><tabIndex>tabindex=""1""</tabIndex><cssClass>class=""requiredClass testFileUploadCustomStyle""</cssClass><customStyle>style=""height:4px;width:4px;""</customStyle><Dev2customScriptF>testFileUploadCustomScript</Dev2customScriptF><Dev2FileTypes>testFileUploadFileTypes</Dev2FileTypes><validationClass></validationClass><InjectedLabel>testFileUploadDisplayText</InjectedLabel><showText>on</showText><displayText>testFileUploadDisplayText</displayText><Resumption><ParentWorkflowInstanceId>88d807aa-f3ca-4e22-9118-e4b7a8b54c48</ParentWorkflowInstanceId><ParentServiceName>File</ParentServiceName></Resumption><InjectStar>
//  <font color=""red"">*</font>
//</InjectStar></ADL>";

//            //Assert.IsTrue(1 == 1);
//            Assert.AreEqual(expected, result);
//        }

//        #endregion

//        #region Webpage Binding
//        [TestMethod]
//        public void Webpage_Parses_Output_Defs() {
//            string payload = ParserStrings.webpageIOTest;

//            IList<IDev2Definition> defs = DataListFactory.CreateDataListCompiler().GenerateDefsFromWebpage(payload);


//            Assert.IsTrue(defs != null && defs.Count == 1 && defs[0].Name == "ABC");

//        }

//        [TestMethod]
//        public void Webpart_Binds_No_Execute_Result_Transfer() {
//            string currentADL = TestStrings.sampleWebpartNoTransferCurrentADL;
//            string preExecuteADL = TestStrings.sampleWebpartNoTransferPreExecuteADL;
//            string outputDefs = TestStrings.sampleWebpartBuildNoTransferOutputDefs;
//            string shape = TestStrings.sampleWebpartBuildNoTransferDLShape;

//            string result = dlUtil.ShapeOutput(currentADL, preExecuteADL, outputDefs, shape);

//            //Assert.IsTrue(1 == 1);
//            Assert.AreEqual(TestStrings.sampleWebpartBuildNoUpdateResult, result);
//        }

//        [TestMethod]
//        public void Webpart_Binds_Execute_Result_Transfer() {
//            string currentADL = TestStrings.sampleWebpartOverWriteTransferCurrentADL;
//            string preExecuteADL = TestStrings.sampleWebpartOverWriteTransferPreExecuteADL;
//            string outputDefs = TestStrings.sampleWebpartBuildNoTransferOutputDefs;
//            string shape = TestStrings.sampleWebpartBuildNoTransferDLShape;

//            string result = dlUtil.ShapeOutput(currentADL, preExecuteADL, outputDefs, shape);

//            //Assert.IsTrue(1 == 1);
//            Assert.AreEqual(TestStrings.sampleWebpartBuildWithUpdateResult, result);
//        }
//        #endregion

//        #region Sundry Test
//        [TestMethod]
//        public void StripCrapWithDataInNaughtyTags_ExpectWellFormedXML() {
//            string adl = @"<XmlData>
//  <XmlData>
//    <XmlData>
//      <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//      <XmlData>
//        <Dev2ServiceDetailsName>aBC</Dev2ServiceDetailsName>
//        <Dev2ServiceDetailsWorkType>Plugin</Dev2ServiceDetailsWorkType>
//        <Dev2ServiceDetailsSource>Anything To Xml Hook Plugin</Dev2ServiceDetailsSource>
//        <Dev2ServiceDetailsCategory></Dev2ServiceDetailsCategory>
//        <Dev2ServiceDetailsHelp></Dev2ServiceDetailsHelp>
//        <Dev2ServiceDetailsIcon></Dev2ServiceDetailsIcon>
//        <Dev2ServiceDetailsDescription></Dev2ServiceDetailsDescription>
//        <Dev2ServiceDetailsTags></Dev2ServiceDetailsTags>
//        <Dev2ServiceDetailsTooltipText></Dev2ServiceDetailsTooltipText>
//      </XmlData>
//      <Async />
//    </XmlData>
//  </XmlData>
//  <Dev2ResumeData>
//    <XmlData>
//      <InstanceId>844d8d39-e455-499b-ae54-2dff031eb628</InstanceId>
//      <Bookmark>dsfResumption</Bookmark>
//      <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//      <XmlData>
//        <Dev2ServiceDetailsName>aBC</Dev2ServiceDetailsName>
//        <Dev2ServiceDetailsWorkType>Plugin</Dev2ServiceDetailsWorkType>
//        <Dev2ServiceDetailsSource>Anything To Xml Hook Plugin</Dev2ServiceDetailsSource>
//        <Dev2ServiceDetailsCategory></Dev2ServiceDetailsCategory>
//        <Dev2ServiceDetailsHelp></Dev2ServiceDetailsHelp>
//        <Dev2ServiceDetailsIcon></Dev2ServiceDetailsIcon>
//        <Dev2ServiceDetailsDescription></Dev2ServiceDetailsDescription>
//        <Dev2ServiceDetailsTags></Dev2ServiceDetailsTags>
//        <Dev2ServiceDetailsTooltipText></Dev2ServiceDetailsTooltipText>
//      </XmlData>
//      <Async />
//    </XmlData>
//  </Dev2ResumeData>
//</XmlData>";

//            string expected = @"<ADL><Dev2ServiceDetailsName>aBC</Dev2ServiceDetailsName>
//        <Dev2ServiceDetailsWorkType>Plugin</Dev2ServiceDetailsWorkType>
//        <Dev2ServiceDetailsSource>Anything To Xml Hook Plugin</Dev2ServiceDetailsSource>
//        <Dev2ServiceDetailsCategory></Dev2ServiceDetailsCategory>
//        <Dev2ServiceDetailsHelp></Dev2ServiceDetailsHelp>
//        <Dev2ServiceDetailsIcon></Dev2ServiceDetailsIcon>
//        <Dev2ServiceDetailsDescription></Dev2ServiceDetailsDescription>
//        <Dev2ServiceDetailsTags></Dev2ServiceDetailsTags>
//        <Dev2ServiceDetailsTooltipText></Dev2ServiceDetailsTooltipText>
//      
//      <Async />
//    
//  
//  <Dev2ResumeData>
//    
//      <InstanceId>844d8d39-e455-499b-ae54-2dff031eb628</InstanceId>
//      <Bookmark>dsfResumption</Bookmark>
//      
//      
//        <Dev2ServiceDetailsName>aBC</Dev2ServiceDetailsName>
//        <Dev2ServiceDetailsWorkType>Plugin</Dev2ServiceDetailsWorkType>
//        <Dev2ServiceDetailsSource>Anything To Xml Hook Plugin</Dev2ServiceDetailsSource>
//        <Dev2ServiceDetailsCategory></Dev2ServiceDetailsCategory>
//        <Dev2ServiceDetailsHelp></Dev2ServiceDetailsHelp>
//        <Dev2ServiceDetailsIcon></Dev2ServiceDetailsIcon>
//        <Dev2ServiceDetailsDescription></Dev2ServiceDetailsDescription>
//        <Dev2ServiceDetailsTags></Dev2ServiceDetailsTags>
//        <Dev2ServiceDetailsTooltipText></Dev2ServiceDetailsTooltipText>
//      
//      <Async />
//    
//  </Dev2ResumeData></ADL>";

//            string[] nTags = { "<Dev2WebServer>", "</Dev2WebServer>" };

//            string result = new DataListCompiler().StripCrap(adl, nTags);

//            Assert.AreEqual(expected, result);
//        }


//        [TestMethod]
//        public void Extract_Index_From_Recordset() {
//            string shape = @"<DL><recset><a/></recset></DL>";

//            string exp = "[[recset(1)]]";

//            IDev2DataLanguageParser parser = new Dev2DataLanguageParser();
//            IList<IIntellisenseResult> results = parser.ParseDataLanguageForIntellisense(exp, shape, true);

//            Assert.IsTrue(results.Count == 1 && results[0].Option.Recordset == "recset" && results[0].Option.RecordsetIndex == "1");

//        }

//        [TestMethod]
//        public void Extract_Recordset() {
//            string shape = @"<DL><recset><a/></recset></DL>";

//            string exp = "[[recset()]]";

//            IDev2DataLanguageParser parser = new Dev2DataLanguageParser();
//            IList<IIntellisenseResult> results = parser.ParseDataLanguageForIntellisense(exp, shape, true);

//            Assert.IsTrue(results.Count == 1 && results[0].Option.Recordset == "recset" && results[0].Option.RecordsetIndex == "");

//        }

//        [TestMethod]
//        public void Ensure_Input_Mapping_Transfers() {
//            string input = @"<XmlData>
//  <Service>Button.wiz</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/Button.wiz</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <Dev2ResumeData>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <Dev2elementNameButton>ButtonClickedabcd</Dev2elementNameButton>
//        <displayTextButton>sdf</displayTextButton>
//        <btnType>submit</btnType>
//        <customButtonCode>customButtonCode</customButtonCode>
//        <Dev2tabIndexButton></Dev2tabIndexButton>
//        <Dev2toolTipButton></Dev2toolTipButton>
//        <Dev2customStyleButton></Dev2customStyleButton>
//        <Dev2widthButton></Dev2widthButton>
//        <Dev2heightButton></Dev2heightButton>
//        <Async />
//      </Dev2ResumeData>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>ButtonClickedabcd</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";

//            string mapping = ParserStrings.sampleString;

//            string result = dlUtil.ShapeInput(input, mapping, ParserStrings.sampleDL);

//            Assert.IsTrue(result != null);

//        }

//        [TestMethod]
//        public void MergeForGhostServiceInvoke_Positive() {

//            string actual = dlu.MergeForGhostServiceInvoke(ParserStrings.dataListShape, ParserStrings.combinedDataListShape);
//            string expected = @"<DataListShape><testNodeA>3</testNodeA><testNodeB>1</testNodeB><testNodeC>3</testNodeC><testNodeD>1</testNodeD><testNodeE>1</testNodeE></DataListShape>";
//            Assert.AreEqual(expected, actual);
//        }


//        [TestMethod]
//        public void MergeForGhostServiceInvoke_Extra_CombinedDataListItems() {
//            string actual = dlu.MergeForGhostServiceInvoke(ParserStrings.dataListShape, ParserStrings.combinedDataListShapeWithExtras);
//            string expected = @"<DataListShape><testNodeA>3</testNodeA><testNodeB>1</testNodeB><testNodeC>3</testNodeC><testNodeD>1</testNodeD><testNodeE>1</testNodeE><testNodeF>1</testNodeF><testNodeG>1</testNodeG></DataListShape>";
//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void MergeForGhostServiceInvoke_Extra_DataListShapeItem() {
//            string actual = dlu.MergeForGhostServiceInvoke(ParserStrings.dataListShapeWithExtras, ParserStrings.combinedDataListShape);
//            string expected = @"<DataListShape><testNodeA>3</testNodeA><testNodeB>1</testNodeB><testNodeC>3</testNodeC><testNodeD>1</testNodeD><testNodeE>1</testNodeE></DataListShape>";
//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void Ensure_Correct_Ordering() {
//            string payload = "[[testRecSet1(1).testRec1]]";
//            string eval = "testRecValue1";
//            string shape = @"<ADL>
//<fname></fname>
//<lname></lname>
//<testValue1>bob</testValue1>
//<testName1></testName1>
//<testName2></testName2>
//<testName3></testName3>
//<testName4></testName4>
//<testName5></testName5>
//<testName6></testName6>
//<testName7></testName7>
//<testName8></testName8>
//<testName9></testName9>
//<testName10></testName10>
//<testRecSet1>
//	<testRec1></testRec1>
//	<testRec2></testRec2>
//</testRecSet1>
//<testRecSet1>
//	<testRec1></testRec1>
//	<testRec2></testRec2>
//</testRecSet1>
//</ADL>";
//            string datalist = @"<XmlData>
//  <ADL>
//    <fname></fname>
//    <lname></lname>
//    <testValue1>bob</testValue1>
//    <testName1>bob</testName1>
//    <testName2>testValue2</testName2>
//    <testName3>testValue3</testName3>
//    <testName4>testValue4</testName4>
//    <testName5>testValue5</testName5>
//    <testName6>testValue6</testName6>
//    <testName7>testValue7</testName7>
//    <testName8>testValue8</testName8>
//    <testName9>testValue9</testName9>
//    <testName10>testValue10</testName10>
//    <testRecSet1>
//      <testRec1></testRec1>
//      <testRec2></testRec2>
//    </testRecSet1>
//    <testRecSet1>
//      <testRec1></testRec1>
//      <testRec2></testRec2>
//    </testRecSet1>
//  </ADL>
//</XmlData>";

//            string expected = @"<ADL>
//  
//    <testRecSet1>
//      <testRec1>testRecValue1</testRec1>
//      <testRec2></testRec2>
//    </testRecSet1>
//    <testRecSet1>
//      <testRec1></testRec1>
//      <testRec2></testRec2>
//    </testRecSet1>
//  
//  
//    <fname></fname>
//    <lname></lname>
//    <testValue1>bob</testValue1>
//    <testName1>bob</testName1>
//    <testName2>testValue2</testName2>
//    <testName3>testValue3</testName3>
//    <testName4>testValue4</testName4>
//    <testName5>testValue5</testName5>
//    <testName6>testValue6</testName6>
//    <testName7>testValue7</testName7>
//    <testName8>testValue8</testName8>
//    <testName9>testValue9</testName9>
//    <testName10>testValue10</testName10>
//  
//</ADL>";

//            string result = DataListFactory.CreateDataListCompiler().UpsertDataList(payload, eval, shape, datalist);

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), DataListUtil.FlattenIntoSingleString(result));
//        }

//        #endregion

//        #region Evaluation

//        [TestMethod]
//        public void Evaluate_Recordset_Where_Empty() {
//            // GenerateDataListFromDefs
//            string dataListShape = @"<DataList>
//    <innerrecset Description="""">
//      <innerrec Description="""" />
//      <innerlol Description="""" />
//      <innerdate Description="""" />
//    </innerrecset>
//    <testing Description="""">
//      <test Description="""" />
//    </testing>
//    <abc Description="""" />
//  </DataList>";
//            string currentDl = @"<ADL>
//  
//    <testing>
//      <test>last_Record</test>
//    </testing>
//    <testing>
//      <test />
//    </testing>
//  
//  
//    <innerrecset>
//      <innerrec>test1</innerrec>
//      <innerlol>testlol1</innerlol>
//      <innerdate />
//    </innerrecset>
//    <innerrecset>
//      <innerrec>Static_Scalar</innerrec>
//      <innerlol />
//      <innerdate />
//    </innerrecset>
//    <Resumption>
//      <ParentWorkflowInstanceId>dbe0ab79-cf9d-437a-a794-1c5af7b0d198</ParentWorkflowInstanceId>
//      <ParentServiceName>NewForEachNumber</ParentServiceName>
//    </Resumption>
//    <Service>TestForEachOutput</Service>
//    <abc>Static_Scalar</abc>
//  
//</ADL>";
//            string preDl = @"<Dev2ServiceInput>
//  <XmlData>
//    <ADL>
//      <innerdate></innerdate>
//      <abc>Static_Scalar</abc>
//      <innerrecset>
//        <innerrec>test1</innerrec>
//        <innerlol>testlol1</innerlol>
//      </innerrecset>
//      <testing>
//        <test>last_Record</test>
//      </testing>
//      <testing>
//        <test />
//      </testing>
//    </ADL>
//  </XmlData>
//  <Resumption>
//    <ParentWorkflowInstanceId>dbe0ab79-cf9d-437a-a794-1c5af7b0d198</ParentWorkflowInstanceId>
//    <ParentServiceName>NewForEachNumber</ParentServiceName>
//  </Resumption>
//  <Service>TestForEachOutput</Service>
//  <Async />
//</Dev2ServiceInput>";
//            string eval = @"[[testing().test]]";
//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(eval, dataListShape, currentDl, preDl);
//            Assert.AreEqual(result, "");
//        }

//        [TestMethod]
//        public void Evaluate_Recordset_With_Star() {
//            // GenerateDataListFromDefs
//            string dataListShape = @"<DataList>
//    <innerrecset Description="""">
//      <innerrec Description="""" />
//      <innerlol Description="""" />
//      <innerdate Description="""" />
//    </innerrecset>
//    <testing Description="""">
//      <test Description="""" />
//    </testing>
//    <abc Description="""" />
//  </DataList>";
//            string currentDl = @"<ADL>
//  
//    <testing>
//      <test>last_Record</test>
//    </testing>
//    <testing>
//      <test />
//    </testing>
//  
//  
//    <innerrecset>
//      <innerrec>test1</innerrec>
//      <innerlol>testlol1</innerlol>
//      <innerdate />
//    </innerrecset>
//    <innerrecset>
//      <innerrec>Static_Scalar</innerrec>
//      <innerlol />
//      <innerdate />
//    </innerrecset>
//    <Resumption>
//      <ParentWorkflowInstanceId>dbe0ab79-cf9d-437a-a794-1c5af7b0d198</ParentWorkflowInstanceId>
//      <ParentServiceName>NewForEachNumber</ParentServiceName>
//    </Resumption>
//    <Service>TestForEachOutput</Service>
//    <abc>Static_Scalar</abc>
//  
//</ADL>";
//            string preDl = @"<Dev2ServiceInput>
//  <XmlData>
//    <ADL>
//      <innerdate></innerdate>
//      <abc>Static_Scalar</abc>
//      <innerrecset>
//        <innerrec>test1</innerrec>
//        <innerlol>testlol1</innerlol>
//      </innerrecset>
//      <testing>
//        <test>last_Record</test>
//      </testing>
//      <testing>
//        <test />
//      </testing>
//    </ADL>
//  </XmlData>
//  <Resumption>
//    <ParentWorkflowInstanceId>dbe0ab79-cf9d-437a-a794-1c5af7b0d198</ParentWorkflowInstanceId>
//    <ParentServiceName>NewForEachNumber</ParentServiceName>
//  </Resumption>
//  <Service>TestForEachOutput</Service>
//  <Async />
//</Dev2ServiceInput>";
//            string eval = @"[[testing(*).test]]";
//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(eval, dataListShape, currentDl, preDl);
//            Assert.AreEqual(result, "<test>last_Record</test><test />");
//        }

//        [TestMethod]
//        public void BaseWizard_Can_Bind() {
//            string dl = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <FormView Description="""" />
//    <DataValue Description="""" />
//  </DataList>";

//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(TestStrings.base_wizard, dl, TestStrings.base_wiard_data, TestStrings.base_wizard_data_pre);

//            Assert.AreEqual(TestStrings.base_wizard_expected, result);
//        }

//        // Bug : 5995
//        /// <summary>
//        /// Test ensures that Evaulation are not made to System datalist items in design mode
//        /// </summary>
//        [TestMethod]
//        public void Dev2WebServer_DesignModeComplexExpression_Expected_Not_To_Bind()
//        {
//            string dl = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <FormView Description="""" />
//    <DataValue Description="""" />
//  </DataList>";

//            string adl = @"<XmlData>
//  <Service>Button.wiz</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/Button.wiz</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <ADL>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <buttonScriptRegionLang>ace/mode/javascript</buttonScriptRegionLang>
//        <Async />
//        <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
//        <displayTextButton>Click Me</displayTextButton>
//        <Dev2tabIndexButton></Dev2tabIndexButton>
//        <Dev2toolTipButton></Dev2toolTipButton>
//        <Dev2heightButton></Dev2heightButton>
//        <Dev2customStyleButton></Dev2customStyleButton>
//        <Dev2widthButton></Dev2widthButton>
//        <btnType>submit</btnType>
//        <buttonScriptRegionuseRegion>alert(""""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");</buttonScriptRegionuseRegion>
//        <buttonScriptRegionsaveRegion>alert(""hello, I am a well formed custom script"");__BREAK____BREAK____BREAK__alert(""[[Dev2WebServer]]"");</buttonScriptRegionsaveRegion>
//        <buttonWizardNameLabel />
//        <buttonWizardDisplayTextLabel />
//        <buttonWizardTypeOfLabel />
//        <tabIndexLabel />
//        <Dev2toolTipLabel />
//        <Dev2height />
//        <Dev2widthLabel />
//        <Dev2customStyleLabel />
//        <Dev2CustomScriptLbl />
//        <buttonScriptRegion />
//      </ADL>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>ButtonClicked</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";

//            string exp = @"alert(""hello, I am a well formed custom script"");   alert(""abc[[Dev2WebServer]]def"");";

//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(exp, dl, adl, "<ADL></ADL>");

//            Assert.AreEqual(@"alert(""hello, I am a well formed custom script"");   alert(""abc[[Dev2WebServer]]def"");", result);
//        }

//        // Bug : 5995
//        [TestMethod]
//        public void EvaluateFromDataList_DesignMode_Expected_NoEvaluationPerformed()
//        {
//            string dl = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <FormView Description="""" />
//    <DataValue Description="""" />
//  </DataList>";

//            string adl = @"<XmlData>
//  <Service>Button.wiz</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/Button.wiz</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <ADL>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <buttonScriptRegionLang>ace/mode/javascript</buttonScriptRegionLang>
//        <Async />
//        <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
//        <displayTextButton>Click Me</displayTextButton>
//        <Dev2tabIndexButton></Dev2tabIndexButton>
//        <Dev2toolTipButton></Dev2toolTipButton>
//        <Dev2heightButton></Dev2heightButton>
//        <Dev2customStyleButton></Dev2customStyleButton>
//        <Dev2widthButton></Dev2widthButton>
//        <btnType>submit</btnType>
//        <buttonScriptRegionuseRegion>alert(""""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");</buttonScriptRegionuseRegion>
//        <buttonScriptRegionsaveRegion>alert(""hello, I am a well formed custom script"");__BREAK____BREAK____BREAK__alert(""[[Dev2WebServer]]"");</buttonScriptRegionsaveRegion>
//        <buttonWizardNameLabel />
//        <buttonWizardDisplayTextLabel />
//        <buttonWizardTypeOfLabel />
//        <tabIndexLabel />
//        <Dev2toolTipLabel />
//        <Dev2height />
//        <Dev2widthLabel />
//        <Dev2customStyleLabel />
//        <Dev2CustomScriptLbl />
//        <buttonScriptRegion />
//      </ADL>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>ButtonClicked</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";

//            string exp = @"alert(""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");";

//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(exp, dl, adl, "<ADL></ADL>");

//            Assert.AreEqual(@"alert(""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");", result);
//        }

//          /// Sashen: 19-10-2012 - PBI: 5995 - Test fails - ExpectedNotToEvaluate 
//          /// Travis: 22.10.2012 - This was due to malformed XML, not an issue with the system!
//          /// 
////        /// <summary>
////        /// Tests to ensure that all datalist evaluations occur for items that should be evaluated
////        /// </summary>
//        [TestMethod]
//        public void EvaluateFromDataList_DesignMode_NonSystemTags_Expected_ItemsEvaluated()
//        {
//            string dl = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <FormView Description="""" />
//    <DataValue Description="""" />
//    <buttonWizardNameLabel Description="" />
//  </DataList>";

//            string adl = @"<XmlData>
//  <Service>Button.wiz</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/Button.wiz</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <ADL>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <buttonScriptRegionLang>ace/mode/javascript</buttonScriptRegionLang>
//        <Async/>
//        <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
//        <displayTextButton>Click Me</displayTextButton>
//        <Dev2tabIndexButton></Dev2tabIndexButton>
//        <Dev2toolTipButton></Dev2toolTipButton>
//        <Dev2heightButton></Dev2heightButton>
//        <Dev2customStyleButton></Dev2customStyleButton>
//        <Dev2widthButton></Dev2widthButton>
//        <btnType>submit</btnType>
//        <buttonScriptRegionuseRegion>alert(""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");</buttonScriptRegionuseRegion>
//        <buttonScriptRegionsaveRegion>alert(""hello, I am a well formed custom script"");__BREAK____BREAK____BREAK__alert(""[[Dev2WebServer]]"");</buttonScriptRegionsaveRegion>
//        <buttonWizardNameLabel>10</buttonWizardNameLabel>
//        <buttonWizardDisplayTextLabel></buttonWizardDisplayTextLabel>
//        <buttonWizardTypeOfLabel></buttonWizardTypeOfLabel>
//        <tabIndexLabel></tabIndexLabel>
//        <Dev2toolTipLabel></Dev2toolTipLabel>
//        <Dev2height></Dev2height>
//        <Dev2widthLabel></Dev2widthLabel>
//        <Dev2customStyleLabel></Dev2customStyleLabel>
//        <Dev2CustomScriptLbl></Dev2CustomScriptLbl>
//        <buttonScriptRegion></buttonScriptRegion>
//      </ADL>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>ButtonClicked</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";
//            string exp = @"alert(""hello, I am a well formed custom script"");   alert(""[[buttonWizardNameLabel]]"");";
//            string actual = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(exp, dl, adl, "<ADL></ADL>");

//            Assert.AreEqual(@"alert(""hello, I am a well formed custom script"");   alert(""[[buttonWizardNameLabel]]"");", actual);

//        }

//        /// <summary>
//        /// Tests to ensure that datalist items are evaluated when they do not exist in the datalist
//        /// </summary>
//        [TestMethod]
//        public void EvaluateFromDataList_DesignMode_TagDoNotExistInADL_Expected_ItemEvaluatedToEmpty() {
//            string dl = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <FormView Description="""" />
//    <DataValue Description="""" />
//    
//  </DataList>";

//            string adl = @"<XmlData>
//  <Service>Button</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/Button.</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <ADL>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <buttonScriptRegionLang>ace/mode/javascript</buttonScriptRegionLang>
//        <Async />
//        <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
//        <displayTextButton>Click Me</displayTextButton>
//        <Dev2tabIndexButton></Dev2tabIndexButton>
//        <Dev2toolTipButton></Dev2toolTipButton>
//        <Dev2heightButton></Dev2heightButton>
//        <Dev2customStyleButton></Dev2customStyleButton>
//        <Dev2widthButton></Dev2widthButton>
//        <btnType>submit</btnType>
//        <buttonScriptRegionuseRegion>alert(""""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");</buttonScriptRegionuseRegion>
//        <buttonScriptRegionsaveRegion>alert(""hello, I am a well formed custom script"");__BREAK____BREAK____BREAK__alert(""[[Dev2WebServer]]"");</buttonScriptRegionsaveRegion>
//        <buttonWizardNameLabel />
//        <buttonWizardDisplayTextLabel />
//        <buttonWizardTypeOfLabel />
//        <tabIndexLabel />
//        <Dev2toolTipLabel />
//        <Dev2height />
//        <Dev2widthLabel />
//        <Dev2customStyleLabel />
//        <Dev2CustomScriptLbl />
//        <buttonScriptRegion />
//        <UnitTestItemToEvaluate>test</UnitTestItemToEvaluate>
//      </ADL>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>ButtonClicked</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";
//            string exp = @"alert(""hello, I am a well formed custom script"");   alert(""[[UnitTestItemToEvaluate]]"");";
//            string actual = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(exp, dl, adl, "<ADL></ADL>");

//            Assert.AreEqual(@"alert(""hello, I am a well formed custom script"");   alert("""");", actual);

//        }

//        /// <summary>
//        /// Tests to ensure that datalist items are evaluated when they do not exist in the datalist
//        /// </summary>
//        [TestMethod]
//        public void EvaluateFromDataList_DesignMode_TagDoNotExistInDataListShape_Expected_ItemEvaluatedToEmpty() {
//            string dl = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <FormView Description="""" />
//    <DataValue Description="""" />
//  </DataList>";

//            string adl = @"<XmlData>
//  <Service>Button.wiz</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/Button.wiz</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <ADL>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <buttonScriptRegionLang>ace/mode/javascript</buttonScriptRegionLang>
//        <Async />
//        <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
//        <displayTextButton>Click Me</displayTextButton>
//        <Dev2tabIndexButton></Dev2tabIndexButton>
//        <Dev2toolTipButton></Dev2toolTipButton>
//        <Dev2heightButton></Dev2heightButton>
//        <Dev2customStyleButton></Dev2customStyleButton>
//        <Dev2widthButton></Dev2widthButton>
//        <btnType>submit</btnType>
//        <buttonScriptRegionuseRegion>alert(""""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");</buttonScriptRegionuseRegion>
//        <buttonScriptRegionsaveRegion>alert(""hello, I am a well formed custom script"");__BREAK____BREAK____BREAK__alert(""[[Dev2WebServer]]"");</buttonScriptRegionsaveRegion>
//        <buttonWizardNameLabel />
//        <buttonWizardDisplayTextLabel />
//        <buttonWizardTypeOfLabel />
//        <tabIndexLabel />
//        <Dev2toolTipLabel />
//        <Dev2height />
//        <Dev2widthLabel />
//        <Dev2customStyleLabel />
//        <Dev2CustomScriptLbl />
//        <buttonScriptRegion />
//      </ADL>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>ButtonClicked</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";
//            string exp = @"alert(""hello, I am a well formed custom script"");   alert(""[[UnitTestItemToEvaluate]]"");";
//            string actual = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(exp, dl, adl, "<ADL></ADL>");

//            Assert.AreEqual(@"alert(""hello, I am a well formed custom script"");   alert(""[[UnitTestItemToEvaluate]]"");", actual);

//        }

//        // Bug : 5995
//        [TestMethod]
//        public void Dev2WebServer_NotDesignMode_Expected_To_Bind()
//        {
//            string dl = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <FormView Description="""" />
//    <DataValue Description="""" />
//  </DataList>";

//            string adl = @"<XmlData>
//  <Service>Button</Service>
//  <InstanceId></InstanceId>
//  <Bookmark></Bookmark>
//  <WebServerUrl>http://127.0.0.1:1234/services/Button</WebServerUrl>
//  <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//  <XmlData>
//    <Dev2XMLResult>
//      <ADL>
//        <Dev2WebServer>http://127.0.0.1:1234</Dev2WebServer>
//        <buttonScriptRegionLang>ace/mode/javascript</buttonScriptRegionLang>
//        <Async />
//        <Dev2elementNameButton>ButtonClicked</Dev2elementNameButton>
//        <displayTextButton>Click Me</displayTextButton>
//        <Dev2tabIndexButton></Dev2tabIndexButton>
//        <Dev2toolTipButton></Dev2toolTipButton>
//        <Dev2heightButton></Dev2heightButton>
//        <Dev2customStyleButton></Dev2customStyleButton>
//        <Dev2widthButton></Dev2widthButton>
//        <btnType>submit</btnType>
//        <buttonScriptRegionuseRegion>alert(""""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");</buttonScriptRegionuseRegion>
//        <buttonScriptRegionsaveRegion>alert(""hello, I am a well formed custom script"");__BREAK____BREAK____BREAK__alert(""[[Dev2WebServer]]"");</buttonScriptRegionsaveRegion>
//        <buttonWizardNameLabel />
//        <buttonWizardDisplayTextLabel />
//        <buttonWizardTypeOfLabel />
//        <tabIndexLabel />
//        <Dev2toolTipLabel />
//        <Dev2height />
//        <Dev2widthLabel />
//        <Dev2customStyleLabel />
//        <Dev2CustomScriptLbl />
//        <buttonScriptRegion />
//      </ADL>
//      <Dev2WebPageElementNames>
//        <Dev2ElementName>ButtonClicked</Dev2ElementName>
//      </Dev2WebPageElementNames>
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";

//            string exp = @"alert(""hello, I am a well formed custom script"");   alert(""[[Dev2WebServer]]"");";

//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(exp, dl, adl, "<ADL></ADL>");

//            Assert.AreEqual(@"alert(""hello, I am a well formed custom script"");   alert(""http://127.0.0.1:1234"");", result);
//        }

//        [TestMethod]
//        public void Strange_Checkbox_Wizard_Binding_Issue_Can_Bind() {

//            string exp = @"<span class=""requiredClass "">
// <input type=""text"" name=""Dev2elementNameCheckbox"" id=""Dev2elementNameCheckbox"" value=""[[Dev2elementNameCheckbox]]"" maxlength=""""   class=""requiredClass ""  /> <font color=""red"">*</font>
//</span> <input type=""hidden"" id=""Dev2elementNameCheckboxErrMsg"" value="""" />";

//            string shape = @"<DataList>
//    <DataSource Description="""" />
//    <Dev2Services Description="""" />
//    <simpleOptions Description="""" />
//    <checkBoxItem Description="""" />
//    <Services Description="""" />
//    <ResourceName Description="""" />
//    <Roles Description="""" />
//    <ResourceType Description="""" />
//    <checkboxWizardElementNameLabel Description="""" />
//    <checkboxWizardAllowEdit Description="""" />
//    <checkboxWizardAlignLabel Description="""" />
//    <Dev2elementNameCheckbox Description="""" />
//    <checkboxWizardFromServiceLabel Description="""" />
//    <ButtonClickedAddRadio Description="""" />
//    <ButtonClickedDelete Description="""" />
//    <tabIndexLabel Description="""" />
//    <Dev2tabIndexCheckbox Description="""" />
//    <tooltipLabel Description="""" />
//    <Dev2toolTipCheckbox Description="""" />
//    <ButtonClickedCancel Description="""" />
//    <ButtonClickedDone Description="""" />
//    <customStyleLabel Description="""" />
//    <Dev2customStyleCheckbox Description="""" />
//    <widthLabel Description="""" />
//    <Dev2widthCheckbox Description="""" />
//    <heightLabel Description="""" />
//    <Dev2heightCheckbox Description="""" />
//    <customScriptLabel Description="""" />
//    <Dev2customScriptCheckbox Description="""" />
//    <ButtonClickedCBAdvanced Description="""" />
//    <Dev2CustomDataBindingServiceLabel Description="""" />
//    <DevCustomDataServiceCB Description="""" />
//    <Dev2CustomDataRowDelimiterLabel Description="""" />
//    <Dev2CustomDataRowDelimiterCB Description="""" />
//    <Dev2customDataBindingDisplayFieldNameLabel Description="""" />
//    <Dev2CustomDataRowDisplayCB Description="""" />
//    <Dev2CustomDataBindingOptionFieldLabel Description="""" />
//    <Dev2CustomDataOptionFieldCB Description="""" />
//    <Dev2CustomDataBindingDataServiceInputLabel Description="""" />
//    <Dev2ServiceParametersCB Description="""" />
//    <Dev2CheckboxStaticOptions Description="""" />
//  </DataList>
//  ";

//            string curADL = @"<sr>
//  <sr>
//    <ADL>
//      <Service>SetReadOnly</Service>
//      <WebXMLConfiguration>
//        <Dev2DesignTimeBinding>true</Dev2DesignTimeBinding>
//        <WebPart>
//          <WebPartServiceName>Textbox</WebPartServiceName>
//          <ColumnIndex>1</ColumnIndex>
//          <RowIndex>1</RowIndex>
//          <Dev2XMLResult>
//            <Dev2ResumeData>
//              <Dev2maxChars></Dev2maxChars>
//              <Dev2SearchTermTagNameTB></Dev2SearchTermTagNameTB>
//              <Async />
//            </Dev2ResumeData>
//          </Dev2XMLResult>
//        </WebPart>
//        <Dev2WebpartBindingData>
//          <ActivityInput></ActivityInput>
//        </Dev2WebpartBindingData>
//      </WebXMLConfiguration>
//      <Dev2elementName>Dev2elementNameCheckbox</Dev2elementName>
//      <ErrorMsg></ErrorMsg>
//      <Dev2displayText></Dev2displayText>
//      <showTextTB></showTextTB>
//      <requiredTB>on</requiredTB>
//      <Dev2tabIndexTextbox></Dev2tabIndexTextbox>
//      <Dev2toolTipTextbox></Dev2toolTipTextbox>
//      <validationTB></validationTB>
//      <Dev2widthTextbox></Dev2widthTextbox>
//      <Dev2heightTextbox></Dev2heightTextbox>
//      <allowEditTB>yes</allowEditTB>
//      <Dev2customStyleTextbox></Dev2customStyleTextbox>
//      <Fragment><![CDATA[<span class=""requiredClass "">
// <input type=""text"" name=""Dev2elementNameCheckbox"" id=""Dev2elementNameCheckbox"" value=""[[Dev2elementNameCheckbox]]"" maxlength=""""   class=""requiredClass ""  /> <font color=""red"">*</font>
//</span>
//
//
//
//<input type=""hidden"" id=""Dev2elementNameCheckboxErrMsg"" value="""" />]]></Fragment>
//      <cssClass>class=""requiredClass ""</cssClass>
//      <InjectedLabel></InjectedLabel>
//      <Dev2maxCharsTB></Dev2maxCharsTB>
//      <toolTip></toolTip>
//      <tabIndex></tabIndex>
//      <customStyle></customStyle>
//      <readOnly></readOnly>
//      <InjectStar>
//        <font color=""red"">*</font>
//      </InjectStar>
//      <Dev2customScriptTB></Dev2customScriptTB>
//      <Inject></Inject>
//      <Dev2validationErrMsgTB></Dev2validationErrMsgTB>
//      <autoTB>no</autoTB>
//      <Dev2DisplayNameTB></Dev2DisplayNameTB>
//      <Dev2RelFieldNameTB></Dev2RelFieldNameTB>
//      <Dev2RowDelimTB></Dev2RowDelimTB>
//      <Dev2autoComplete></Dev2autoComplete>
//      <showText></showText>
//      <displayText></displayText>
//    </ADL>
//  </sr>
//</sr>";

//            string preADL = @"<ADL>
//  
//    <ActivityInput>
//      <DataSource />
//      <Dev2Services />
//      <simpleOptions />
//      <checkBoxItem />
//      <Services />
//      <ResourceName />
//      <Roles />
//      <ResourceType />
//      <checkboxWizardElementNameLabel />
//      <checkboxWizardAllowEdit />
//      <checkboxWizardAlignLabel />
//      <Dev2elementNameCheckbox>myCB</Dev2elementNameCheckbox>
//      <checkboxWizardFromServiceLabel />
//      <ButtonClickedAddRadio />
//      <ButtonClickedDelete />
//      <tabIndexLabel />
//      <Dev2tabIndexCheckbox />
//      <tooltipLabel />
//      <Dev2toolTipCheckbox />
//      <ButtonClickedCancel />
//      <ButtonClickedDone />
//      <customStyleLabel />
//      <Dev2customStyleCheckbox />
//      <widthLabel />
//      <Dev2widthCheckbox />
//      <heightLabel />
//      <Dev2heightCheckbox />
//      <customScriptLabel />
//      <Dev2customScriptCheckbox />
//      <ButtonClickedCBAdvanced />
//      <Dev2CustomDataBindingServiceLabel />
//      <DevCustomDataServiceCB />
//      <Dev2CustomDataRowDelimiterLabel />
//      <Dev2CustomDataRowDelimiterCB />
//      <Dev2customDataBindingDisplayFieldNameLabel />
//      <Dev2CustomDataRowDisplayCB />
//      <Dev2CustomDataBindingOptionFieldLabel />
//      <Dev2CustomDataOptionFieldCB />
//      <Dev2CustomDataBindingDataServiceInputLabel />
//      <Dev2ServiceParametersCB />
//      <Dev2CheckboxStaticOptions />
//    </ActivityInput>
//  
//  <Resumption>
//    <ParentWorkflowInstanceId>1de94592-4c54-489d-911f-fa8e1c569f49</ParentWorkflowInstanceId>
//    <ParentServiceName>Checkbox.wiz</ParentServiceName>
//  </Resumption>
//  <Service>Checkbox_Wizard</Service>
//  <Async />
//</ADL>";

//            string result = DataListFactory.CreateDataListCompiler().EvaluateFromDataList(exp, shape, curADL, preADL);

//            string expected = @"<span class=""requiredClass "">
// <input type=""text"" name=""Dev2elementNameCheckbox"" id=""Dev2elementNameCheckbox"" value="""" maxlength=""""   class=""requiredClass ""  /> <font color=""red"">*</font>
//</span> <input type=""hidden"" id=""Dev2elementNameCheckboxErrMsg"" value="""" />";

//            Assert.AreEqual(expected, result);
//        }

//        [TestMethod]
//        public void InnerForEachInputShaping() {

//            string currentADL = @"<ADL>
//  <testing>
//    <test>testVal1</test>
//  </testing>
//  
//    <recset>
//      <rec>recVal1</rec>
//      <rec2>rec2Val1</rec2>
//    </recset>
//    <recset>
//      <rec>recVal2</rec>
//      <rec2>rec2Val2</rec2>
//    </recset>
//    <recset>
//      <rec>recVal3</rec>
//      <rec2 />
//    </recset>
//    <recset>
//      <rec>recVal4</rec>
//      <rec2 />
//    </recset>
//    <recset>
//      <rec>recVal5</rec>
//      <rec2 />
//    </recset>
//    <Service>NewForEachNumber</Service>
//    <WebServerUrl>http://localhost:1234/services/NewForEachNumber</WebServerUrl>
//    <var>Static_Scalar</var>
//    <resultVar></resultVar>
//  
//</ADL>";

//            string defs = @"<Inputs><Input Name=""innerrec"" Source="""" Recordset=""innerrecset"" /><Input Name=""innerrec2"" Source="""" Recordset=""innerrecset"" /><Input Name=""innerdate"" Source="""" Recordset=""innerrecset"" /><Input Name=""innertest"" Source=""[[recset(1).rec2]]"" Recordset=""innertesting"" /><Input Name=""innerScalar"" Source=""[[recset(1).rec]]"" /></Inputs>";

//            string shape = @"<DataList>
//    <var Description="""" />
//    <recset Description="""">
//      <rec Description="""" />
//      <rec2 Description="""" />
//    </recset>
//    <testing Description="""">
//      <test Description="""" />
//    </testing>
//    <resultVar Description="""" />
//  </DataList>";

//            string result = DataListFactory.CreateDataListCompiler().ShapeInput(currentADL, defs, shape);

//            string expected = @"<ADL><innerScalar>recVal1</innerScalar><innerrecset><innerrec /><innerrec2 /><innerdate /></innerrecset><innertesting><innertest>rec2Val1</innertest></innertesting></ADL>";

//            Assert.AreEqual(expected, result);
//        }


////        [TestMethod]
////        public void x() {
////            string token = "[[recset(1).rec]]";

////            string currentDataList = @"<ADL>
////  <testing>
////    <test>testVal1</test>
////  </testing>
////  
////    <recset>
////      <rec>recVal1</rec>
////      <rec2>rec2Val1</rec2>
////    </recset>
////    <recset>
////      <rec>recVal2</rec>
////      <rec2>rec2Val2</rec2>
////    </recset>
////    <recset>
////      <rec>recVal3</rec>
////      <rec2 />
////    </recset>
////    <recset>
////      <rec>recVal4</rec>
////      <rec2 />
////    </recset>
////    <recset>
////      <rec>recVal5</rec>
////      <rec2 />
////    </recset>
////    <Service>NewForEachNumber</Service>
////    <WebServerUrl>http://localhost:1234/services/NewForEachNumber</WebServerUrl>
////    <var>Static_Scalar</var>
////    <resultVar></resultVar>
////  
////</ADL>";

////            string cleanedShape = @"<ADL><innerScalar /><recset><rec/><rec2/></recset><innerrecset><innerrec /><innerrec2 /><innerdate /></innerrecset><innertesting><innertest /></innertesting><recset><rec2 /></recset></ADL>";

////            DataListCompiler compiler = new DataListCompiler();
////            string tmp = compiler.EvaluateFromDataList(token, cleanedShape, currentDataList, "<ADL></ADL>", true); ;

////            string expected = @"";

////            Assert.AreEqual(expected, tmp);
////        }
//        #endregion

   // }
}
