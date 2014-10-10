
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.Data.Parsers;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace Dev2.Tests
{
    //<summary>
    //Summary description for Dev2DataLanguageParser
    //</summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class Dev2DataLanguageParserTest
    {
        // <summary>
        //Gets or sets the test context which provides
        //information about and functionality for the current test run.
        //</summary>
        public TestContext TestContext { get; set; }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool isFromIntellisense = false)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList, false, null, isFromIntellisense);
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool addCompleteParts, bool isFromIntellisense = false)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList, addCompleteParts, null, isFromIntellisense);
        }

        private IList<IIntellisenseResult> ParseDataLanguageForIntellisense(string transform, string dataList, bool addCompleteParts, IntellisenseFilterOpsTO filterOps, bool isFromIntellisense = false)
        {
            return DataListFactory.CreateLanguageParser().ParseDataLanguageForIntellisense(transform, dataList, addCompleteParts, filterOps, isFromIntellisense);
        }

        
        #region recordset test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_ParseExpressionIntoParts_WhenMalformedRecordsetWithInvalidIndex_Expect2Errors()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();

            const string data = @"[[rec(&&,.a]]";
            const string dl = "<DataList><rec><a/></rec></DataList>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, dl, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);
            if(bdl != null)
            {
                var intillisenseParts = bdl.FetchIntellisenseParts();

                //------------Execute Test---------------------------
                var parts = dev2LanuageParser.ParseExpressionIntoParts(data, intillisenseParts);

                //------------Assert Results-------------------------
                Assert.AreEqual(1, parts.Count);
                StringAssert.Contains(parts[0].Message, "Recordset name [[rec(&&,]] contains invalid character(s)");
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_ParseExpressionIntoParts_WhenRecordsetWithInvalidIndex_Expect2Errors()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();

            const string data = @"[[rec(a,*).a]]";
            const string dl = "<DataList><rec><a/></rec></DataList>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, dl, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);
            if(bdl != null)
            {
                var intillisenseParts = bdl.FetchIntellisenseParts();

                //------------Execute Test---------------------------
                var parts = dev2LanuageParser.ParseExpressionIntoParts(data, intillisenseParts);

                //------------Assert Results-------------------------
                Assert.AreEqual(2, parts.Count);
                StringAssert.Contains(parts[0].Message, "Recordset index (a,*) contains invalid character(s)");
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_ParseExpressionIntoParts_WhenValid_ExpectCache()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            PrivateType p = new PrivateType(typeof(Dev2DataLanguageParser));
            var cache = p.GetStaticField("ExpressionCache") as Dictionary<string, IList<IIntellisenseResult>>;

            Assert.IsNotNull(cache);
            cache.Clear();
            Assert.AreEqual(cache.Count, 0);
            const string data = @"[[rec(*).a]]";
            const string dl = "<DataList><rec><a/></rec></DataList>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, dl, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);
            var intillisenseParts = bdl.FetchIntellisenseParts();
            //------------Execute Test---------------------------
            dev2LanuageParser.ParseExpressionIntoParts(data, intillisenseParts);
            //------------Assert Results-------------------------
            Assert.IsNotNull(cache);
            Assert.AreEqual(1,cache.Count);
                
        
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_ParseExpressionIntoParts_WhenInValid_ExpectNoCache()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            PrivateType p = new PrivateType(typeof(Dev2DataLanguageParser));
            var cache = p.GetStaticField("PayloadCache") as Dictionary<Tuple<string, string>, IList<IIntellisenseResult>>;

            Assert.IsNotNull(cache);
            cache.Clear();
            Assert.AreEqual(cache.Count, 0);
            const string data = @"[[rec(*123).a]]";
            const string dl = "<DataList><rec><a/></rec></DataList>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, dl, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);
            var intillisenseParts = bdl.FetchIntellisenseParts();
            //------------Execute Test---------------------------
            dev2LanuageParser.ParseExpressionIntoParts(data, intillisenseParts);
            //------------Assert Results-------------------------
            Assert.IsNotNull(cache);
            Assert.AreEqual(cache.Count, 0);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_Parse_Valid_ExpectCache()
        {
            //------------Setup for test--------------------------
            const string dl = "<ADL><rec><val/></rec></ADL>";
            const string payload = "[[rec().val]]";
            var dev2LanuageParser = new Dev2DataLanguageParser();
            PrivateType p = new PrivateType(typeof(Dev2DataLanguageParser));
            var cache = p.GetStaticField("PayloadCache") as Dictionary<Tuple<string,string>, IList<IIntellisenseResult>>;
            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);
            Assert.IsNotNull(cache);
            Assert.AreEqual(cache.Count,1);
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_Parse_IValid_ExpectNoCache()
        {
            //------------Setup for test--------------------------
            const string dl = "<ADL><rec><val/></rec></ADL>";
            const string payload = "[[rec().val]]";
            var dev2LanuageParser = new Dev2DataLanguageParser();
            PrivateType p = new PrivateType(typeof(Dev2DataLanguageParser));
            var cache = p.GetStaticField("ExpressionCache") as Dictionary<string, IList<IIntellisenseResult>>;
            Assert.IsNotNull(cache);
            cache.Clear();
            Assert.AreEqual(cache.Count, 0);
            ParseDataLanguageForIntellisense(payload, dl, true, false);
            Assert.IsNotNull(cache);
            Assert.AreEqual(cache.Count, 0);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2LanuageParser_Wrap")]
        public void Dev2LanuageParser_Wrap_ExceptionClearsCache()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            Dictionary<string,string> data = new Dictionary<string, string> { { "a", "b" } };

            try
            {
                // ReSharper disable CSharpWarnings::CS0162
                dev2LanuageParser.WrapAndClear(() => { throw new Exception(); }, data);
                // ReSharper restore CSharpWarnings::CS0162
                Assert.Fail("y u no throw exception");
            }
            catch(Exception)
            {
                
                Assert.AreEqual(data.Count,0);
            }

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Dev2LanuageParser_Wrap")]
        public void Dev2LanuageParser_Wrap_ValidDoesNothing()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            Dictionary<string, string> data = new Dictionary<string, string> { { "a", "b" } };

            dev2LanuageParser.WrapAndClear(() => "bob", data);

            Assert.AreEqual(data.Count, 1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_ParseExpressionIntoParts_WhenRecordsetWithInvalidIndexAndExtractCloseRegion_Expect2Errors()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();

            const string data = @"[[rec(23).[[var}]]]]";
            const string dl = "<DataList><rec><a/></rec></DataList>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, dl, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);
            if(bdl != null)
            {
                var intillisenseParts = bdl.FetchIntellisenseParts();

                //------------Execute Test---------------------------
                var parts = dev2LanuageParser.ParseExpressionIntoParts(data, intillisenseParts);

                //------------Assert Results-------------------------
                Assert.AreEqual(2, parts.Count);
                StringAssert.Contains(parts[1].Message, "Variable name [[var}]] contains invalid character(s)");
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2LanuageParser_Parse")]
        public void Dev2LanuageParser_ParseExpressionIntoParts_WhenMissingVariable_Expect1Error()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();

            const string data = @"[[]]";
            const string dl = "<DataList><rec><a/></rec></DataList>";
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), string.Empty, dl, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);
            if(bdl != null)
            {
                var intillisenseParts = bdl.FetchIntellisenseParts();

                //------------Execute Test---------------------------
                var parts = dev2LanuageParser.ParseExpressionIntoParts(data, intillisenseParts);

                //------------Assert Results-------------------------
                Assert.AreEqual(1, parts.Count);
                StringAssert.Contains(parts[0].Message, "Variable [[]] is missing a name");
            }
            else
            {
                Assert.Fail();
            }
        }

        #endregion

        #region Pos Test
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2LanuageParser_Parse")]

        public void Dev2LanuageParser_Parse_WhenCommaDelimited_ExpectThreeValidParts()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            const string data = @" [[rec().s]], [[rec().e]], [[rec().t]]";

            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseForActivityDataItems(data);

            //------------Assert Results-------------------------
            Assert.AreEqual(3, parts.Count);
        }


        [TestMethod]
        public void Range_Operation_ExpectedRangeOption_As_Valid_Index()
        {
            const string dataList = @"<ADL><sum></sum></ADL>";
            const string transform = "[[sum([[rs(1).f1:rs(5).f1]])]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(transform, dataList, true, false);

            Assert.IsTrue(result.Count == 2 && result[0].ErrorCode == enIntellisenseErrorCode.None && result[0].Option.DisplayValue == "[[sum(rs(1).f1:rs(5).f1)]]");
        }


        [TestMethod]
        public void IntellisenseWithScalars_Expected_Recordset_With_ScalarOptions()
        {
            const string dl = "<ADL> <rs><f1/></rs><myScalar/><myScalar2/> </ADL>";

            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense("[[rs(", dl, false, false);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(" / Select a specific row", result[0].Message);
            Assert.AreEqual("[[rs([[myScalar]])]]", result[0].Option.DisplayValue);
            Assert.AreEqual(" / Select a specific row", result[1].Message);
            Assert.AreEqual("[[rs([[myScalar2]])]]", result[1].Option.DisplayValue);
            Assert.AreEqual(" / Reference all rows in the Recordset ", result[2].Message);
            Assert.AreEqual("[[rs(*)]]", result[2].Option.DisplayValue);
        }

        [TestMethod]
        public void FormView_Is_Part()
        {
            const string dl = "<ADL> <FormView/> </ADL>";

            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense("[[FormView]]", dl, true, false);

            Assert.IsTrue(result.Count == 1);
        }

        [TestMethod]
        public void Return_Scalar_With_Open_Recordset()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/><regYear/></cars><pos/></ADL>";
            const string payload = "[[cars([[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[pos]]");
        }

        [TestMethod]
        public void Double_Open_Bracket_Returns_DataList()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 5 && results[0].Option.DisplayValue == "[[cars()]]" && results[1].Option.DisplayValue == "[[cars().reg]]" && results[2].Option.DisplayValue == "[[cars().colour]]" && results[3].Option.DisplayValue == "[[cars().year]]" && results[4].Option.DisplayValue == "[[cool]]");
        }


        [TestMethod]
        public void Matches_One_Scalar_In_DL()
        {
            const string dl = "<ADL><fName></fName><sName/></ADL>";
            const string payload = "[[f";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[fName]]");
        }

        [TestMethod]
        public void Matches_Two_Scalars_In_DL()
        {
            const string dl = "<ADL><fName></fName><foo></foo></ADL>";
            const string payload = "[[f";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].Option.DisplayValue == "[[fName]]" && results[1].Option.DisplayValue == "[[foo]]");
        }

        [TestMethod]
        public void Matches_One_Scalars_And_Recordset_Field_In_DL()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[s";


            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 5 && results[0].Option.DisplayValue == "[[surname]]" && results[1].Option.DisplayValue == "[[cars(" && results[2].Option.DisplayValue == "[[cars(*)]]" && results[4].Option.DisplayValue == "[[cars().topspeed]]");
        }

        //19.09.2012: massimo.guerrera - Added for bug not showing the decription entered in the datalist.
        [TestMethod]
        public void Check_Scalars_And_Recordsets_Have_Descriptions()
        {
            const string dl = @"<ADL><TestScalar Description=""this is a decription for TestScalar"" /><TestRecset Description=""this is a decription for TestRecset"">
  <TestField Description=""this is a decription for TestField"" />
</TestRecset></ADL>";
            const string payload = "[[";

            IntellisenseFilterOpsTO filterTo = new IntellisenseFilterOpsTO { FilterCondition = "", FilterType = enIntellisensePartType.All };
            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, false, filterTo);

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual("this is a decription for TestScalar / Select this variable", results[0].Option.Description);
            Assert.AreEqual("this is a decription for TestRecset / Select this record set", results[1].Option.Description);
            Assert.AreEqual("this is a decription for TestField / Select this record set field", results[2].Option.Description);
        }

        [TestMethod]
        public void Matches_Recordset_No_Close_With_Close()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars(";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(" / Select a specific row", results[0].Message);
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_No_Number()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars()";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars()]]" && results[1].Option.DisplayValue == "[[cars().reg]]" && results[2].Option.DisplayValue == "[[cars().colour]]" && results[3].Option.DisplayValue == "[[cars().year]]");
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_With_Number()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars(1)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars(1)]]" && results[1].Option.DisplayValue == "[[cars(1).reg]]" && results[2].Option.DisplayValue == "[[cars(1).colour]]" && results[3].Option.DisplayValue == "[[cars(1).year]]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2DataLanguageParser_ParseDataLanguageForIntellisense")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_WhenFromIntellisenseTrue_ExpectOnlyMatchingRecordsetFields()
        {
            //------------Setup for test--------------------------
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars().re";

            //------------Execute Test---------------------------

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, false, true);

            //------------Assert Results-------------------------

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(results[0].Option.DisplayValue, "[[cars().reg]]");
            Assert.AreEqual(results[1].Option.DisplayValue, "[[cars(*).reg]]");

        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_R_In_Field()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars(1).r";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 3 && results[0].Option.DisplayValue == "[[cars(1).reg]]" && results[1].Option.DisplayValue == "[[cars(1).colour]]" && results[2].Option.DisplayValue == "[[cars(1).year]]");
        }

        [TestMethod]
        public void Matches_Recordset_Fields_With_Close_RE_In_Field()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars(1).re";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[cars(1).reg]]");
        }

        [TestMethod]
        public void Not_Found_Recordset_With_Closed_Variable()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[carz(1).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);
        }

        [TestMethod]
        public void InvalidExpression_Recordset_With_Closed_Variable()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars(1.reg]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.SyntaxError);
        }

        [TestMethod]
        public void Not_Found_Recordset_Field_With_Closed_Variable()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars(1).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_And_Scalar_With_Closed_Variable()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars(1).rex]][[def]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_Embedded_For_Add()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[[[cars(1).rex]]]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_Embedded_For_Add_And_Find_Missing_Index()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[[[cars([[abc]]).rex]]]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Find_Recordset_Field_And_Embedded_Scalar_Reference_For_Add()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/></ADL>";
            const string payload = "[[cars([[myPos]]).rex]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 2 && results[0].ErrorCode == enIntellisenseErrorCode.FieldNotFound && results[1].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        //

        [TestMethod]
        public void Find_Recordset_Index_Non_Closed_Recordset()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/><myPos/><cCount/></ADL>";
            const string payload = "[[cars([[cCount]])";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 4 && results[0].Option.DisplayValue == "[[cars([[cCount]])]]" && results[1].Option.DisplayValue == "[[cars([[cCount]]).reg]]" && results[2].Option.DisplayValue == "[[cars([[cCount]]).colour]]" && results[3].Option.DisplayValue == "[[cars([[cCount]]).year]]");
        }

        [TestMethod]
        public void Find_Recordset_Index_As_Scalar()
        {
            const string dl = "<ADL><cars><reg/><colour/><year/></cars><cool/><myPos/><myCount/></ADL>";
            const string payload = "[[cars([[my";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true);

            Assert.IsTrue(results.Count == 2 && results[0].Option.DisplayValue == "[[myPos]]" && results[1].Option.DisplayValue == "[[myCount]]");
        }

        [TestMethod]
        public void Find_Recordset_And_Field_Closed_For_Return()
        {
            const string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            const string payload = "[[InjectedScript2().data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);

            Assert.IsTrue(results.Count == 1 && results[0].Option.DisplayValue == "[[InjectedScript2().data]]" && results[0].ErrorCode == enIntellisenseErrorCode.None);

        }

        [TestMethod]
        public void Find_Scalar_Closed_For_Return()
        {
            const string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            const string payload = "[[InjectedScript]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);

            Assert.IsTrue(results.Count == 4);

        }

        [TestMethod]
        public void Find_Scalar_In_Recordset_Closed_For_Return()
        {
            const string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2><pos/></ADL>";
            const string payload = "[[InjectedScript2([[pos]])]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);

            Assert.IsTrue(results.Count == 2);

        }

        [TestMethod]
        public void Find_Recordset_In_Recordset_As_Index_Closed_For_Return()
        {
            const string dl = "<ADL><InjectedScript><data/></InjectedScript><InjectedScript2><data/></InjectedScript2><pos/></ADL>";
            const string payload = "[[InjectedScript2([[InjectedScript().data]]).data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);

            Assert.IsTrue(results.Count == 2 && results[0].Option.RecordsetIndex == "InjectedScript().data");

        }

        [TestMethod]
        public void Star_Index_Is_Valid_Index()
        {
            const string dl = "<ADL><recset><f1/><f2/></recset></ADL>";
            const string payload = "[[recset(*).f1]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true, false);

            Assert.IsTrue(result.Count == 1 && result[0].Option.Recordset == "recset" && result[0].Option.RecordsetIndex == "*" && result[0].Option.Field == "f1");

        }

        // Bug : 5793 - Travis.Frisinger : 19.10.2012
        [TestMethod]
        public void Recordset_With_DataList_Index_Returns_Correctly()
        {
            const string dl = "<ADL><recset><f1/><f2/></recset><scalar/></ADL>";
            const string payload = "[[recset([[s";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, false, true);

            Assert.AreEqual(6, result.Count);
            Assert.AreEqual("[[recset(", result[0].Option.DisplayValue);
            Assert.AreEqual("[[recset(*).f1]]", result[2].Option.DisplayValue);
            Assert.AreEqual("[[scalar]]", result[5].Option.DisplayValue);
        }

        // Bug : 5793 - Travis.Frisinger : 19.10.2012
        [TestMethod]
        public void Recordset_With_Open_DataList_Index_Returns_Correctly()
        {
            const string dl = "<ADL><recset><f1/><f2/></recset><scalar/></ADL>";
            const string payload = "[[recset([[scalar).f2]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, false, false);

            Assert.IsTrue(result.Count == 4 && result[0].Option.DisplayValue == "[[recset([[scalar]])]]" && result[1].Option.DisplayValue == "[[recset([[scalar]]).f1]]" && result[2].Option.DisplayValue == "[[recset([[scalar]]).f2]]");
        }

        //2013.05.31: Ashley Lewis for bug 9472
        [TestMethod]
        public void RecordsetResultsExpectedReturnsCompleteRecordsetsOnlyResult()
        {
            const string dl = "<ADL><recset><f1/><f2/></recset></ADL>";
            const string payload = "[[rec";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, false, false);

            Assert.IsNotNull(result.FirstOrDefault(intellisenseResults => intellisenseResults.Option.DisplayValue == "[[recset()]]"));
        }

        //2013.06.25: Ashley Lewis for bug 9801 - Variable named error shouldn't necessarily get error intellisense result at least not in the case described below
        [TestMethod]
        public void ParseWithVariableNamedErrorExpectedNoErrorResults()
        {
            const string dl = "<ADL><Error/></ADL>";
            const string payload = "[[Error]]";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true, null);

            Assert.AreEqual(1, result.Count, "Dev2DataLanguageParser returned an incorrect number of results");
            Assert.AreEqual("[[Error]]", result[0].Option.DisplayValue, "Dev2DataLanguageParser returned an incorrect result");
            Assert.AreEqual(enIntellisenseResultType.Selectable, result[0].Type, "Dev2DataLanguageParser returned an incorrect result type");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_MakeParts")]
        public void Dev2DataLanguageParser_MakeParts_PayloadIsEmpty_NoParts()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.MakeParts("");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_MakeParts")]
        public void Dev2DataLanguageParser_MakeParts_PayloadIsNull_NoParts()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.MakeParts(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_MakePartsWithOutRecsetIndex")]
        public void Dev2DataLanguageParser_MakePartsWithOutRecsetIndex_PayloadIsNull_NoParts()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.MakePartsWithOutRecsetIndex(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_MakePartsWithOutRecsetIndex")]
        public void Dev2DataLanguageParser_MakePartsWithOutRecsetIndex_PayloadIsEmpty_NoParts()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.MakePartsWithOutRecsetIndex("");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseDataLanguageForIntellisense")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_PayloadIsNull_NoIntellisenseResult()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseDataLanguageForIntellisense(null, "");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseDataLanguageForIntellisense")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_PayloadIsEmpty_NoIntellisenseResult()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseDataLanguageForIntellisense("", "");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseDataLanguageForIntellisense")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_DataListIsNull_NoIntellisenseResult()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseDataLanguageForIntellisense(null, "");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseDataLanguageForIntellisense")]
        public void Dev2DataLanguageParser_ParseDataLanguageForIntellisense_DataListIsEmpty_NoIntellisenseResult()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseDataLanguageForIntellisense("", "");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseExpressionIntoParts")]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts_ExpressionIsEmpty_NoIntellisenseResult()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseExpressionIntoParts("", new List<IDev2DataLanguageIntellisensePart>());
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseExpressionIntoParts")]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts_ExpressionIsNull_NoIntellisenseResult()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseExpressionIntoParts(null, new List<IDev2DataLanguageIntellisensePart>());
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseExpressionIntoParts")]
        public void Dev2DataLanguageParser_ParseExpressionIntoParts_DataListPartsIsNull_NoIntellisenseResult()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseExpressionIntoParts("[[var]]", null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseForActivityDataItems")]
        public void Dev2DataLanguageParser_ParseForActivityDataItems_PayloadIsNull_NoDataListParts()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseForActivityDataItems(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ParseForActivityDataItems")]
        public void Dev2DataLanguageParser_ParseForActivityDataItems_PayloadIsEmpty_NoDataListParts()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var parts = dev2LanuageParser.ParseForActivityDataItems("");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, parts.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ValidateName")]
        public void Dev2DataLanguageParser_ValidateName_NameIsNull_Null()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var res = dev2LanuageParser.ValidateName(null, "");
            //------------Assert Results-------------------------
            Assert.IsNull(res);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Dev2DataLanguageParser_ValidateName")]
        public void Dev2DataLanguageParser_ValidateName_NotLatinCharacter_ShowMessageBox_TextMadeEmpty()
        {
            //------------Setup for test--------------------------          
            var dev2LanuageParser = new Dev2DataLanguageParser();
            const string Text = "?????????";
            //------------Execute Test---------------------------
            var intellisenseResult = dev2LanuageParser.ValidateName(Text, "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(intellisenseResult);
            Assert.AreEqual(enIntellisenseErrorCode.SyntaxError, intellisenseResult.ErrorCode);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("Dev2DataLanguageParser_ValidateName")]
        public void Dev2DataLanguageParser_ValidateName_NameIsEmpty_Null()
        {
            //------------Setup for test--------------------------
            var dev2LanuageParser = new Dev2DataLanguageParser();
            //------------Execute Test---------------------------
            var res = dev2LanuageParser.ValidateName("", "");
            //------------Assert Results-------------------------
            Assert.IsNull(res);
        }

        #endregion

        #region Negative Test

        [TestMethod]
        public void Fail_To_Find_Recordset_And_Field_With_Simular_Name()
        {
            const string dl = "<ADL><InjectedScript/><InjectedScript2><data/></InjectedScript2></ADL>";
            const string payload = "[[InjectedScript().data]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);

            Assert.IsTrue(results.Count == 1);
            Assert.IsTrue(results[0].Option.DisplayValue == "[[InjectedScript().data]]");
            Assert.IsTrue(results[0].ErrorCode == enIntellisenseErrorCode.NeitherRecordsetNorFieldFound);

        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Numeric_No_Close()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[cars(a";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Numeric_With_Close()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[cars(a)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.NonNumericRecordsetIndex);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Greater_Zero_No_Close()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[cars(-1";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_Invalid_Index_Not_Greater_Zero_With_Close()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[cars(-1)";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Scalar()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[abc";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.ScalarNotFound);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Recordset_No_Close()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[abc(";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error);
        }

        [TestMethod]
        public void Throws_Exception_No_Match_For_Recordset_With_Close()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[abc()";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 1 && results[0].Type == enIntellisenseResultType.Error && results[0].ErrorCode == enIntellisenseErrorCode.RecordsetNotFound);
        }

        [TestMethod]
        public void Single_Open_On_Dual_Region_Does_Not_Cause_Results()
        {
            const string dl = "<ADL><surname></surname><cars><topspeed></topspeed></cars></ADL>";
            const string payload = "[[cars()]][";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 0);
        }

        [TestMethod]
        public void Dual_Regions_With_RS_In_Second_Passes_Field_Validation()
        {
            const string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
            const string payload = "[[cars()]][[cars().colour]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.IsTrue(results.Count == 0);
        }

        // Travis.Frisinger - 24.01.2013 : Bug 7856
        // Ashley Lewis - 06.03.2013 : Bug 6731
        [TestMethod]
        public void SpaceChars_In_DataList_Region_Expect_Error()
        {
            const string dl = "<ADL><cars><reg/><colour/></cars><recordCount/></ADL>";
            const string payload = "[[a b ]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Error);
            Assert.AreEqual("Variable name [[a b ]] contains invalid character(s)", results[0].Message);
        }

        [TestMethod]
        public void MixedRegionsParseCorrectly()
        {
            const string dl = "<ADL><a/><b/></ADL>";
            const string payload = "[[a]] [[b]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);
            Assert.AreEqual(2, results.Count, "Did not detect space between language pieces correctly");
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Selectable && results[0].Option.DisplayValue == "[[a]]");
            Assert.IsTrue(results[1].Type == enIntellisenseResultType.Selectable && results[1].Option.DisplayValue == "[[b]]");

        }


        [TestMethod]
        public void MixedRegionsParseCorrectlyWithLeadingSpace()
        {
            const string dl = "<ADL><a/><b/></ADL>";
            const string payload = "abc: [[a]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Type == enIntellisenseResultType.Selectable && results[0].Option.DisplayValue == "[[a]]");

        }

        [TestMethod]
        public void CanDetectSpaceBetweenRecordsetAndFieldWhenBetweenDotAndField()
        {
            const string dl = "<ADL><rec><val/></rec></ADL>";
            const string payload = "abc: [[rec(). val]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(enIntellisenseResultType.Error, results[0].Type);
            Assert.AreEqual("Recordset field name  val contains invalid character(s)", results[0].Message);
        }

        [TestMethod]
        public void CanDetectSpaceBetweenRecordsetAndFieldWhenBeforeDot()
        {
            const string dl = "<ADL><rec><val/></rec></ADL>";
            const string payload = "abc: [[rec() .val]]";

            IList<IIntellisenseResult> results = ParseDataLanguageForIntellisense(payload, dl, true, false);
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(enIntellisenseResultType.Error, results[0].Type);
            Assert.AreEqual("Recordset name [[rec() ]] contains invalid character(s)", results[0].Message);
        }


        #endregion

        #region FindMissing

        //2013.05.31: Ashley Lewis for bug 9472
        [TestMethod]
        public void RecordsetResultsWithNoRecordsetInDataListExpectedReturnsNoResults()
        {
            const string dl = "<ADL></ADL>";
            const string payload = "[[rec";
            IList<IIntellisenseResult> result = ParseDataLanguageForIntellisense(payload, dl, true, false);

            Assert.IsNull(result.FirstOrDefault(intellisenseResults => intellisenseResults.Option.DisplayValue == "[[recset()]]"));
        }


        #endregion

        #region IntellisenseFactory Tests

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_NullRecordSetWithFieldName_ShouldReturnFieldNameWrappedInBrackets()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart(null, "test", "test", "0");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[test]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec", "f1", "test", "0");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec(0).f1]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithFieldNameWithIndex_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec", "f1", "test", "3");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec(3).f1]]", dataListValidationRecordsetPart.DisplayValue);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")] // THIS DOES NOT LOOK CORRECT
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareAndRoundBracketsWithNoFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec()]]", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[[[rec()]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")] // THIS DOES NOT LOOK CORRECT
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareAndRoundBracketsWithFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec()]]", "f3", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[[[rec().f3]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")] // THIS DOES NOT LOOK CORRECT
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareAndRoundBracketsWithFieldNameWithIndex_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec()]]", "f3", "test", "5");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[[[rec(5).f3]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        // IS THIS VALID!!!!
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithRoundBracketsWithNoFieldNameWithIndex_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec()", "", "test", "5");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec(5)]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithRoundBracketsWithNoFieldName_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec()", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec()]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_NoRecordSetWithRoundBracketsWithFieldNameWithStartRoundBracket_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("", "f1(", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[f1(", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithRoundNoBracketsWithNoFieldNameWithStartRoundBracket_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec()]]", dataListValidationRecordsetPart.DisplayValue);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IntellisenseFactory_CreateDataListValidationRecordsetPart")]
        public void IntellisenseFactory_CreateDataListValidationRecordsetPart_RecordSetWithSquareBracketsNoRound_ShouldReturnValidDisplayName()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dataListValidationRecordsetPart = IntellisenseFactory.CreateDataListValidationRecordsetPart("[[rec]]", "", "test", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("[[rec()]]", dataListValidationRecordsetPart.DisplayValue);
        }


        #endregion

    }
}
