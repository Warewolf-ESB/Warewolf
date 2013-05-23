using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2;
using Dev2.DataList.Contract;

namespace Unlimited.UnitTest.Framework
{
    //<summary>
    //Summary description for Dev2DataLanguageParser
    //</summary>
    [TestClass]
    public class Dev2DataLanguageParserTest
    {
        public Dev2DataLanguageParserTest()
        {

            //TODO: Add constructor logic here

        }

        private TestContext testContextInstance;

        // <summary>
        //Gets or sets the test context which provides
        //information about and functionality for the current test run.
        //</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList);
        }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool addCompleteParts)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList, addCompleteParts);
        }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool addCompleteParts, IntellisenseFilterOpsTO filterOps)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList, addCompleteParts, filterOps);
        }

        private IList<IIntellisenseResult> ParseForMissingDataListItems(IList<IDataListVerifyPart> parts, string dataList)
        {
            return DataListFactory.CreateLanguageParser().ParseForMissingDataListItems(parts, dataList);
        }

        #region Pos Test

        [TestMethod()]
        public void Range_Operation_ExpectedRangeOption_As_Valid_Index()
        {
            string dataList = @"<ADL><sum></sum></ADL>";
            string transform = "[[sum([[rs(1).f1:rs(5).f1]])]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(transform, dataList, true);

            Assert.IsTrue(result.Count == 2 && result[0].ErrorCode == enIntellisenseErrorCode.None && result[0].Option.DisplayValue == "[[sum(rs(1).f1:rs(5).f1)]]");
        }


        [TestMethod]
        public void IntellisenseWithScalars_Expected_Recordset_With_ScalarOptions()
        {
            string dl = "<ADL> <rs><f1/></rs><myScalar/><myScalar2/> </ADL>";

            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense("[[rs(", dl, true);

            Assert.IsTrue(result.Count == 3 && result[0].Option.DisplayValue == "[[rs([[myScalar]])]]" && result[1].Option.DisplayValue == "[[rs([[myScalar2]])]]");
        }

        [TestMethod]
        public void FormView_Is_Part()
        {
            string dl = "<ADL> <FormView/> </ADL>";

            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense("[[FormView]]", dl, true);

            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void Return_Scalar_With_Open_Recordset()
        {
            string dl = "<ADL><cars><reg/><colour/><year/><regYear/></cars><pos/></ADL>";
            string payload = "[[cars([[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[pos]]");
        }

        [TestMethod]
        public void Double_Open_Bracket_Returns_DataList()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 5 && results[0].Option.DisplayValue == "[[cars()]]" && results[1].Option.DisplayValue == "[[cars().reg]]" && results[2].Option.DisplayValue == "[[cars().colour]]" && results[3].Option.DisplayValue == "[[cars().year]]" && results[4].Option.DisplayValue == "[[cool]]");
        }


        [TestMethod]
        public void Matches_One_Scalar_In_DL()
        {
            string dl = "<ADL><fName></fName><sName/></ADL>";
            string payload = "[[f";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[fName]]");
        }

        [TestMethod]
        public void Matches_Two_Scalars_In_DL()
        {
            string dl = "<ADL><fName></fName><foo></foo></ADL>";
            string payload = "[[f";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].Option.DisplayValue == "[[fName]]" && results[1].Option.DisplayValue == "[[foo]]");
        }

        [TestMethod]
        public void Matches_One_Scalars_And_Recordset_Field_In_DL()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[s";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 5 && results[0].Option.DisplayValue == "[[surname]]" && results[1].Option.DisplayValue == "[[cars(" && results[2].Option.DisplayValue == "[[cars(*)]]" && results[4].Option.DisplayValue == "[[cars().topspeed]]");
        }

        //19.09.2012: massimo.guerrera - Added for bug not showing the decription entered in the datalist.
        [TestMethod]
        public void Check_Scalars_And_Recordsets_Have_Descriptions()
        {
            string dl = @"<ADL><TestScalar Description=""this is a decription for TestScalar"" /><TestRecset Description=""this is a decription for TestRecset"">
  <TestField Description=""this is a decription for TestField"" />
</TestRecset></ADL>";
            string payload = "[[";

            IntellisenseFilterOpsTO filterTo = new IntellisenseFilterOpsTO();
            filterTo.FilterCondition = "";
            filterTo.FilterType = enIntellisensePartType.All;
            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, false, filterTo);

            Assert.IsTrue(results.Count == 3 && results[0].Option.Description == "this is a decription for TestScalar" && results[1].Option.Description == "this is a decription for TestRecset" && results[2].Option.Description == "this is a decription for TestField");
        }

        [TestMethod]
        public void Matches_Recordset_No_Close_With_Close()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2);
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_No_Number()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars()";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars()]]" && results[1].Option.DisplayValue == "[[cars().reg]]" && results[2].Option.DisplayValue == "[[cars().colour]]" && results[3].Option.DisplayValue == "[[cars().year]]");
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_With_Number()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars(1)]]" && results[1].Option.DisplayValue == "[[cars(1).reg]]" && results[2].Option.DisplayValue == "[[cars(1).colour]]" && results[3].Option.DisplayValue == "[[cars(1).year]]");
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_R_In_Field()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).r";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 3 && results[0].Option.DisplayValue == "[[cars(1).reg]]" && results[1].Option.DisplayValue == "[[cars(1).colour]]" && results[2].Option.DisplayValue == "[[cars(1).year]]");
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_RE_In_Field()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).re";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[cars(1).reg]]");
        }

        [TestMethod]
        public void Not_Found_Recordset_With_Closed_Variable()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[carz(1).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);
        }

        [TestMethod]
        public void Not_Found_Recordset_Field_With_Closed_Variable()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_And_Scalar_With_Closed_Variable()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars(1).rex]][[def]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_Embedded_For_Add()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[[[cars(1).rex]]]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_Embedded_For_Add_And_Find_Missing_Index()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[[[cars([[abc]]).rex]]]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_And_Embedded_Scalar_Reference_For_Add()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            string payload = "[[cars([[myPos]]).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        //

        [TestMethod]
        public void Find_Recordset_Index_Non_Closed_Recordset()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/><myPos/><cCount/></ADL>";
            string payload = "[[cars([[cCount]])";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars([[cCount]])]]" && results[1].Option.DisplayValue == "[[cars([[cCount]]).reg]]" && results[2].Option.DisplayValue == "[[cars([[cCount]]).colour]]" && results[3].Option.DisplayValue == "[[cars([[cCount]]).year]]");
        }

        [TestMethod]
        public void Find_Recordset_Index_As_Scalar()
        {
            string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/><myPos/><myCount/></ADL>";
            string payload = "[[cars([[my";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].Option.DisplayValue == "[[myPos]]" && results[1].Option.DisplayValue == "[[myCount]]");
        }

        [TestMethod]
        public void Find_Recordset_And_Field_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            string payload = "[[InjectedScript2().data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[InjectedScript2().data]]" && results[0].ErrorCode == enIntellisenseErrorCode.None);

        }

        [TestMethod]
        public void Find_Scalar_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            string payload = "[[InjectedScript]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(results.Count == 4);

        }

        [TestMethod]
        public void Find_Scalar_In_Recordset_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2><pos/></ADL>";
            string payload = "[[InjectedScript2([[pos]])]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(results.Count == 2);

        }

        [TestMethod]
        public void Find_Recordset_In_Recordset_As_Index_Closed_For_Return()
        {
            string dl = "<ADL><InjectedScript><data/></InjectedScript><InjectedScript2><data/></InjectedScript2><pos/></ADL>";
            string payload = "[[InjectedScript2([[InjectedScript().data]]).data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(results.Count == 2 && results[0].Option.RecordsetIndex == "InjectedScript().data");

        }

        [TestMethod]
        public void Star_Index_Is_Valid_Index()
        {
            string dl = "<ADL><recset><f1/><f2/></recset></ADL>";
            string payload = "[[recset(*).f1]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(result.Count == 1 && result[0].Option.Recordset == "recset" && result[0].Option.RecordsetIndex == "*" && result[0].Option.Field == "f1");

        }

        // Bug : 5793 - Travis.Frisinger : 19.10.2012
        [TestMethod]
        public void Recordset_With_DataList_Index_Returns_Correctly()
        {
            string dl = "<ADL><recset><f1/><f2/></recset><scalar/></ADL>";
            string payload = "[[recset([[s";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(result.Count == 6 && result[0].Option.DisplayValue == "[[recset(" && result[1].Option.DisplayValue == "[[recset(*)]]" && result[5].Option.DisplayValue == "[[scalar]]");

        }

        // Bug : 5793 - Travis.Frisinger : 19.10.2012
        [TestMethod]
        public void Recordset_With_Open_DataList_Index_Returns_Correctly()
        {
            string dl = "<ADL><recset><f1/><f2/></recset><scalar/></ADL>";
            string payload = "[[recset([[scalar).f2]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(result.Count == 4 && result[0].Option.DisplayValue == "[[recset([[scalar]])]]" && result[1].Option.DisplayValue == "[[recset([[scalar]]).f1]]" && result[2].Option.DisplayValue == "[[recset([[scalar]]).f2]]");

        }

        #endregion

        #region Negative Test
        [TestMethod]
        public void Fail_To_Find_Recordset_And_Field_With_Simular_Name()
        {
            string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            string payload = "[[InjectedScript().data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1);
            Assert.IsTrue(results[0].Option.DisplayValue == "[[InjectedScript().data]]");
            Assert.IsTrue(results[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);

        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Numeric_No_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(a";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Numeric_With_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(a)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.NonNumericRecordsetIndex);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Greater_Zero_No_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(-1";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Greater_Zero_With_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars(-1)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Scalar()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[abc";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Recordset_No_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[abc(";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Recordset_With_Close()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[abc()";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.RecordsetNotFound);
        }

        [TestMethod]
        public void Error_On_Non_Recordset_Notation_With_Valid_Recordset()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.InvalidRecordsetNotation);
        }

        [TestMethod]
        public void Error_On_Non_Recordset_Notation_With_Valid_Recordset_New_Open_Region()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars]][[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 4 && results[0].Type == enIntellisenseResultType.Selectable && results[0].Option.DisplayValue == "[[surname]]" && results[3].ErrorCode == enIntellisenseErrorCode.InvalidRecordsetNotation);
        }

        [TestMethod]
        public void Single_Open_On_Dual_Region_Does_Not_Cause_Results()
        {
            string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            string payload = "[[cars()]][";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 0);
        }

        //

        [TestMethod]
        public void Dual_Regions_With_RS_In_Second_Passes_Field_Validation()
        {
            string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
            string payload = "[[cars()]][[cars().colour]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 0);
        }

        //// Travis.Frisinger - 24.01.2013 : Bug 7856
        //[TestMethod]
        //public void Unclosed_Region_With_Recursive_Region_Left_Open_Expect_Error()
        //{
        //    string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
        //    string payload = "[[test[[var]]";

        //    IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
        //    Assert.IsTrue(results.Count == 2);
        //    Assert.AreEqual(enIntellisenseResultType.Error, results[0].Type);
        //    Assert.AreEqual(enIntellisenseResultType.Error, results[1].Type);

        //    Assert.AreEqual(" [[test]] does not exist in your Data List", results[0].Message);
        //    Assert.AreEqual(" [[var]] does not exist in your Data List", results[1].Message);
        //}


        //// Travis.Frisinger - 24.01.2013 : Bug 7856
        //[TestMethod]
        //public void Closed_Region_With_Recursive_Region_Left_Open_Expect_Error()
        //{
        //    string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
        //    string payload = "[[test[[var]]]]";

        //    IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
        //    Assert.IsTrue(results.Count == 2);
        //    Assert.AreEqual(enIntellisenseResultType.Error, results[0].Type);
        //    Assert.AreEqual(enIntellisenseResultType.Error, results[1].Type);

        //    Assert.AreEqual(" [[test]] does not exist in your Data List", results[0].Message);
        //    Assert.AreEqual(" [[var]] does not exist in your Data List", results[1].Message);
        //}

        //// Travis.Frisinger - 24.01.2013 : Bug 7856
        //[TestMethod]
        //public void SpecialChar_In_DataList_Region_Expect_Error()
        //{
        //    string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
        //    string payload = "[[@]]";

        //    IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
        //    Assert.AreEqual(1, results.Count);
        //    Assert.IsTrue(results[0].Type == enIntellisenseResultType.Error);
        //    Assert.AreEqual("[[@]] contains invalid characters", results[0].Message);
        //}

        //// Travis.Frisinger - 24.01.2013 : Bug 7856
        //[TestMethod]
        //public void NoChars_In_DataList_Region_Expect_Error()
        //{
        //    string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
        //    string payload = "[[]]";

        //    IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
        //    Assert.AreEqual(1, results.Count);
        //    Assert.IsTrue(results[0].Type == enIntellisenseResultType.Error);
        //    Assert.AreEqual("Empty DataList region", results[0].Message);
        //}


        // Travis.Frisinger - 24.01.2013 : Bug 7856
        // Ashley Lewis - 06.03.2013 : Bug 6731
        [TestMethod]
        public void SpaceChars_In_DataList_Region_Expect_Error()
        {
            string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
            string payload = "[[a b ]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Error);
            Assert.AreEqual(" [[a b ]] contains a space, this is an invalid character for a variable name", results[0].Message);
        }

        #endregion

        #region FindMissing
        [TestMethod]
        public void Verify_Region_Find__Two_Missing_Scalars()
        {

            string DL = "<ADL><fname/><lname/></ADL>";

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            parts.Add(IntellisenseFactory.CreateDataListValidationScalarPart("abc"));
            parts.Add(IntellisenseFactory.CreateDataListValidationScalarPart("def"));

            IList<IIntellisenseResult> result = ParseForMissingDataListItems(parts, DL);

            Assert.IsTrue(result.Count == 2 && result[0].ErrorCode == enIntellisenseErrorCode.ScalarNotFound && result[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Verify_Region_Find_Missing_Recordset()
        {

            string DL = "<ADL><fname/><lname/></ADL>";

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            parts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart("cars", "abc"));

            IList<IIntellisenseResult> result = ParseForMissingDataListItems(parts, DL);

            Assert.IsTrue(result.Count == 1 && result[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);
        }

        /*
        [TestMethod]
        public void Verify_Region_Find_Missing_Recordset_Field()
        {

            string DL = "<ADL><fname/><lname/><cars><foo/></cars></ADL>";

            IList<IDataListVerifyPart> parts = new List<IDataListVerifyPart>();

            parts.Add(IntellisenseFactory.CreateDataListValidationRecordsetPart("cars", "bar"));

            IList<IIntellisenseResult> result = ParseForMissingDataListItems(parts, DL);

            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }*/

        #endregion

        #region ParseForActivityDataItemsTest

        /*
        [TestMethod]
        public void Find_Two_Parts_Recordset_As_Index_Of_Recordset()
        {
            string payload = "[[InjectedScript2([[InjectedScript().data]]).data]]";

            IList<string> results = ParseForActivityDataItems(payload);

            Assert.IsTrue(results.Count == 2);

        }

        [TestMethod]
        public void Find_Two_Parts_Scalar_As_Index_Of_Recordset()
        {
            string payload = "[[InjectedScript2([[pos]]).data]]";

            IList<string> results = ParseForActivityDataItems(payload);

            Assert.IsTrue(results.Count == 2);
        }

        [TestMethod]
        public void Find_Three_Parts_Recordset_As_Index_Of_Recordset()
        {
            string payload = "[[InjectedScript2([[InjectedScript([[abc]]).data]]).data]]";

            IList<string> results = ParseForActivityDataItems(payload);

            Assert.IsTrue(results.Count == 3);
        }
        */

        #endregion
    }
}
