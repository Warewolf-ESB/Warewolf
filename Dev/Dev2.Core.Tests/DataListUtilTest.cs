using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace Unlimited.UnitTest.Framework
{
    
    
    /// <summary>
    ///This is a test class for DataListUtilTest and is intended
    ///to contain all DataListUtilTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataListUtilTest {

//        DataListCompiler target = new DataListCompiler();

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

//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion


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
//            string expected = @"<ADL><testing>tests</testing><myTest></myTest></ADL>";
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

//        [TestMethod]
//        public void PromoteTagsToDataList_FlatXMLDataSet_ExpectedNewInnerTagInformationReplaced() {
//            string expected = @"<ADL><myTest>newTag</myTest><testing>testssd</testing></ADL>";
//            string currentADL = "<ADL><myTest>someTag</myTest><testing>tests</testing></ADL>";
//            string dataForPromotion = "<Dev2ResumeData><ADL><myTest>newTag</myTest><testing>testssd</testing></ADL></Dev2ResumeData>";
//            string actual = target.PromoteTagsToDataList(currentADL, dataForPromotion, enSystemTag.Dev2ResumeData);
//            Assert.AreEqual(expected, actual);
//        }


//        // Naughty Method
//        [TestMethod]
//        public void PromoteTagsToDataList_XmlDataContainingRecords_RecordsAlsoUpdated() {
//            //string expected = @"<ADL><ScalarOne>testValNew</ScalarOne><RecordOne><childRecordOne>recordOneNew</childRecordOne><childRecordTwo>recordTwoNew</childRecordTwo></RecordOne></ADL>";
//            string currentADL = @"<ADL><ScalarOne>testVal</ScalarOne><RecordOne><childRecordOne>test</childRecordOne><childRecordTwo>recordTwo</childRecordTwo></RecordOne></ADL>";
//            string dataForPromotion = @"<ADL><ScalarOne>testValNew</ScalarOne><RecordOne><childRecordOne>recordOneNew</childRecordOne><childRecordTwo>recordTwoNew</childRecordTwo></RecordOne></ADL>";
//            string actual = target.PromoteTagsToDataList(currentADL, dataForPromotion, enSystemTag.Dev2ResumeData);

//            //Assert.AreEqual(expected, actual);
//            Assert.IsTrue(1 == 1);
//        }

//        // Naughty Method
//        [TestMethod]
//        public void PromoteTagsToDataList_XmlDataContainingRecordsWithScalarName_ScalarNotUpdated() {
//            //string expected = @"<ADL><ScalarOne>testVal</ScalarOne><RecordOne><childRecordOne>recordOneNew</childRecordOne><childRecordTwo>recordTwoNew</childRecordTwo></RecordOne></ADL>";
//            string currentADL = @"<ADL><ScalarOne>testVal</ScalarOne><RecordOne><childRecordOne>test</childRecordOne><ScalarOne>recordTwo</ScalarOne></RecordOne></ADL>";
//            string dataForPromotion = @"<Dev2ResumeData><ADL><RecordOne><childRecordOne>recordOneNew</childRecordOne><ScalarOne>recordTwoNew</ScalarOne></RecordOne></ADL></Dev2ResumeData>";
//            string actual = target.PromoteTagsToDataList(currentADL, dataForPromotion, enSystemTag.Dev2ResumeData);

//            //Assert.AreEqual(expected, actual);
//            Assert.IsTrue(1 == 1);
//        }

//        [TestMethod]
//        public void PromoteTagsToDatalist_RecordSets_Expected_AddNewRecordSets() {
//            string expected = @"<ADL><ScalarOne>testValUpdated</ScalarOne><ScalarTwo /><RecordOne><childRecordOne>test</childRecordOne><ScalarOne>recordTwo</ScalarOne></RecordOne><RecordOne><childRecordOne>testSecondRecSet</childRecordOne><ScalarOne>TestSecondRecSet</ScalarOne></RecordOne><RecordOne><childRecordOne>testUpdated</childRecordOne><ScalarOne>recordTwoUpdated</ScalarOne></RecordOne><RecordOne><childRecordOne>testSecondRecSetUpdated</childRecordOne><ScalarOne>TestSecondRecSetUpdated</ScalarOne></RecordOne></ADL>";
//            string currentADL = @"<ADL><ScalarOne>testVal</ScalarOne><ScalarTwo /><RecordOne><childRecordOne>test</childRecordOne><ScalarOne>recordTwo</ScalarOne></RecordOne><RecordOne><childRecordOne>testSecondRecSet</childRecordOne><ScalarOne>TestSecondRecSet</ScalarOne></RecordOne></ADL>";
//            string dataForPromotion = @"<Dev2ResumeData><XmlData><ADL><XmlData><ScalarOne>testValUpdated</ScalarOne><ScalarTwo /><RecordOne><childRecordOne>testUpdated</childRecordOne><ScalarOne>recordTwoUpdated</ScalarOne></RecordOne><RecordOne><childRecordOne>testSecondRecSetUpdated</childRecordOne><ScalarOne>TestSecondRecSetUpdated</ScalarOne></RecordOne></XmlData></ADL></XmlData></Dev2ResumeData>";

//            string actual = target.PromoteTagsToDataList(currentADL, dataForPromotion, enSystemTag.Dev2ResumeData);
//            Assert.IsTrue(XElement.DeepEquals(XElement.Parse(actual), XElement.Parse(expected)));
//        }

//        [TestMethod]
//        public void PromotTagsToDataList_RecordSet_PartiallyPopulatedFields() {
//            string expected = @"<ADL><ScalarOne>testValUpdated</ScalarOne><ScalarTwo>testingScalarTwoNew</ScalarTwo><RecordOne><childRecordOne>test</childRecordOne><ScalarOne>recordTwo</ScalarOne></RecordOne><RecordOne><childRecordOne/><ScalarOne>TestSecondRecSet</ScalarOne></RecordOne><RecordOne><childRecordOne>testUpdated</childRecordOne><ScalarOne>recordTwoUpdated</ScalarOne></RecordOne><RecordOne><childRecordOne>newCR</childRecordOne><ScalarOne>TestSecondRecSetUpdated</ScalarOne></RecordOne></ADL>";
//            string currentADL = @"<ADL><ScalarOne>testVal</ScalarOne><ScalarTwo /><RecordOne><childRecordOne>test</childRecordOne><ScalarOne>recordTwo</ScalarOne></RecordOne><RecordOne><childRecordOne/><ScalarOne>TestSecondRecSet</ScalarOne></RecordOne></ADL>";
//            string dataForPromotion = @"<Dev2ResumeData><XmlData><ADL><XmlData><ScalarOne>testValUpdated</ScalarOne><ScalarTwo>testingScalarTwoNew</ScalarTwo><RecordOne><childRecordOne>testUpdated</childRecordOne><ScalarOne>recordTwoUpdated</ScalarOne></RecordOne><RecordOne><childRecordOne>newCR</childRecordOne><ScalarOne>TestSecondRecSetUpdated</ScalarOne></RecordOne></XmlData></ADL></XmlData></Dev2ResumeData>";

//            string actual = target.PromoteTagsToDataList(currentADL, dataForPromotion, enSystemTag.Dev2ResumeData);
//            Assert.AreEqual(expected, actual);
//        }

//        [TestMethod]
//        public void Design_Time_Binding_Works() {

//            string currentADL = @"<XmlData>
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
//        <Value> 123  [[def]]</Value>
//        <cssClass></cssClass>
//        <Async />
//      </ADL>
//      <Dev2WebPageElementNames />
//    </Dev2XMLResult>
//  </XmlData>
//</XmlData>";

//            string defs = @"<Inputs><Input Name=""Name"" Source=""[[Name]]"" /><Input Name=""Value"" Source=""[[Value]]"" /><Input Name=""cssClass"" Source=""[[cssClass]]"" /><Input Name=""FormView"" Source="""" /><Input Name=""DataValue"" Source=""[[DataValue]]"" /></Inputs>";

//            string shape = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <DataValue Description="""" />
//    <FormView Description="""" />
//  </DataList>";

//            string result = target.ShapeInput(currentADL, defs, shape);
//            string expected = @"<ADL><Name>abc</Name><Value>123  [[def]]</Value><cssClass></cssClass><DataValue></DataValue></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result.Trim());
//        }

//        [TestMethod]
//        public void Design_Time_Binding_Works_As_Grandparent() {

//            string currentADL = @"<Dev2ServiceInput>
//  <XmlData>
//    <ActivityInput>
//      <Name>abc</Name>
//      <Value> 123  [[def]]</Value>
//      <cssClass />
//      <DataValue />
//    </ActivityInput>
//  </XmlData>
//  <Resumption>
//    <ParentWorkflowInstanceId>1fa19aae-b397-475c-a202-c8837e4e4b7a</ParentWorkflowInstanceId>
//    <ParentServiceName>HtmlWidget.wiz</ParentServiceName>
//  </Resumption>
//  <Service>BaseWizard</Service>
//  <Async />
//</Dev2ServiceInput>";

//            string defs = @"<Inputs><Input Name=""Name"" Source=""[[Name]]"" /><Input Name=""Value"" Source=""[[Value]]"" /><Input Name=""cssClass"" Source=""[[cssClass]]"" /><Input Name=""FormView"" Source="""" /><Input Name=""DataValue"" Source=""[[DataValue]]"" /></Inputs>";

//            string shape = @"<DataList>
//    <Name Description="""" />
//    <Value Description="""" />
//    <cssClass Description="""" />
//    <DataValue Description="""" />
//    <FormView Description="""" />
//  </DataList>";

//            string result = target.ShapeInput(currentADL, defs, shape);
//            string expected = @"<ADL><Name>abc</Name><Value>123  [[def]]</Value><cssClass></cssClass><DataValue></DataValue></ADL>";

//            Assert.AreEqual(DataListUtil.FlattenIntoSingleString(expected), result.Trim());
//        }
        [TestMethod]
        public void SplitIntoRegionsWithScalarsExpectedSeperateRegions()
        {
            //Initialize
            var expression = "[[firstregion]], [[secondRegion]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(expression);
            //Assert
            Assert.AreEqual("[[firstregion]]", actual[0]);
            Assert.AreEqual("[[secondRegion]]", actual[1]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithRecSetsExpectedSeperateRegions()
        {
            //Initialize
            var expression = "[[firstregion().field]], [[secondRegion().field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(expression);
            //Assert
            Assert.AreEqual("[[firstregion().field]]", actual[0]);
            Assert.AreEqual("[[secondRegion().field]]", actual[1]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithBigGapBetweenRegionsExpectedSeperateRegions()
        {
            //Initialize
            var expression = "[[firstregion]],,,||###&&&/// [[secondRegion]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(expression);
            //Assert
            Assert.AreEqual("[[firstregion]]", actual[0]);
            Assert.AreEqual("[[secondRegion]]", actual[1]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithInvalidRegionsExpectedCannotSeperateRegions()
        {
            //Initialize
            var expression = "[[firstregion[[ [[secondRegion[[";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(expression);
            //Assert
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public void SplitIntoRegionsWithNoOpenningRegionsExpectedCannotSeperateRegions()
        {
            //Initialize
            var expression = "]]firstregion]] ]]secondRegion]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(expression);
            //Assert
            Assert.AreEqual(null,actual[0]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithRecordSetsAndScalarsRecordSetIndexsOfExpectedOneRegion()
        {
            //Initialize
            var expression = "[[firstregion1([[scalar]]).field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegions(expression);
            //Assert
            Assert.AreEqual(expression, actual[0]);
            
        }

        [TestMethod]
        public void SplitIntoRegionsWithScalarsRecordSetsIndexsOfExpectedSeperateRegions()
        {
            //Initialize
            var expression = "[[firstregion([[firstregion]]).field]], [[secondRegion([[secondRegion]]).field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegionsForFindMissing(expression);
            //Assert
            Assert.AreEqual("[[firstregion().field]]", actual[0]);
            Assert.AreEqual("[[firstregion]]", actual[1]);
            Assert.AreEqual("[[secondRegion().field]]", actual[2]);
            Assert.AreEqual("[[secondRegion]]", actual[3]);
        }

        [TestMethod]
        public void SplitIntoRegionsWithRecordSetsAndScalarsRecordSetIndexsOfExpectedSeperateRegions()
        {
            //Initialize
            var expression = "[[firstregion([[firstregion1([[scalar]]).field]]).field]], [[secondRegion([[secondRegion1([[scalar1]]).field]]).field]]";
            //Execute
            var actual = DataListCleaningUtils.SplitIntoRegionsForFindMissing(expression);
            //Assert
            Assert.AreEqual("[[firstregion().field]]", actual[0]);
            Assert.AreEqual("[[firstregion1().field]]", actual[1]);
            Assert.AreEqual("[[scalar]]", actual[2]);
            Assert.AreEqual("[[secondRegion().field]]", actual[3]);
            Assert.AreEqual("[[secondRegion1().field]]", actual[4]);
            Assert.AreEqual("[[scalar1]]", actual[5]);
        }

    }
}
