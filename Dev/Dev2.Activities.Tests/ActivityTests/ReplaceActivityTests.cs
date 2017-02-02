/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class ReplaceActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Replace Positive Tests


        //Added for - Bug 9937
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [Description("Replace special chars")]
        [TestCategory("DsfReplaceActivity")]
        public void ReplaceActivity_UnitTest_WithAllSpecialChars_ReplaceSuccessful()
        {
            SetupArguments(ActivityStrings.ReplaceSpecialCharsDataListWithData, ActivityStrings.ReplaceSpecialCharsDataListShape, "[[SpecialChar]]", @"\*+?|{[()^$# ", "It Worked", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"1";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                string replacedRes;
                GetScalarValueFromEnvironment(result.Environment, "SpecialChar", out replacedRes, out error);
                // remove test datalist ;)

                Assert.AreEqual("It Worked", replacedRes);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_In_Two_Recordset_Fields_Expected_Two_Replaces_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"2";
            string actual;

            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<string> dataListItems;
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);
                // remove test datalist ;)
                Assert.AreEqual("Wallis", dataListItems[0]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_In_Two_Recordset_Fields_With_Space_Between_Expected_Two_Replaces_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]], [[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"2";
            string actual;
            string error;

            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<string> dataListItems;
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);

                // remove test datalist ;)
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_In_Scalar_Field_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[CompanyName]]", "Dev2", "TheUnlimted", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"1";
            string actual;

            string error;

            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromEnvironment(result.Environment, "CompanyName", out actual, out error);
                // remove test datalist ;)

                Assert.AreEqual("TheUnlimted", actual);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_In_Scalar_Field_With_CaseMatch_On_Expected_Zero_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[CompanyName]]", "dev2", "TheUnlimted", "[[res]]", true);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"0";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromEnvironment(result.Environment, "CompanyName", out actual, out error);
                // remove test datalist ;)
                Assert.AreEqual("Dev2", actual);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_Recordset_Fields_With_CaseMatch_On_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "Barney", "Wallis", "[[res]]", true);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"1";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<string> dataListItems;
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("barney", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);

                // remove test datalist ;)

                Assert.AreEqual("Wallis", dataListItems[0]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_Recordset_Fields_With_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(1).field1]],[[Customers(2).FirstName]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"1";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<string> dataListItems;
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "Customers", "FirstName", out dataListItems, out error);

                // remove test datalist ;)

                Assert.AreEqual("Barney", dataListItems[0]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_Scalar_With_Backlash_Expected_One_Replace()
        {
            SetupArguments(@"<DataList><Thing>testlol\</Thing><Res></Res></DataList>", @"<DataList><Thing></Thing><Res></Res></DataList>", @"[[Thing]]", @"lol\", @"Wallis", "[[Res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"1";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "Res", out actual, out error);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromEnvironment(result.Environment, "Thing", out actual, out error);

                // remove test datalist ;)

                Assert.AreEqual("testWallis", actual);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        //2013.02.12: Ashley Lewis - Bug 8800
        [TestMethod]
        public void ReplaceInAllRecordsetFieldsExpectedTwoReplacesSuccess()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData.Replace("f2r2", "barney"), ActivityStrings.ReplaceDataListShape, "[[recset1(*)]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"2";
            string actual;

            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<string> dataListItems;
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0]);
                GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field2", out dataListItems, out error);
                // remove test datalist ;)
                Assert.AreEqual("Wallis", dataListItems[1]);
            }
            else
            {
                // remove test datalist ;)
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

     
        #endregion Replace Positive Tests

        #region Replace Negative Tests

        [TestMethod]
        public void ReplaceRawStringAsInputExpectedFriendlyErrorMessage()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "rawstringdata", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Please insert only variables into Fields To Search";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, GlobalConstants.ErrorPayload, out actual, out error);
            // remove test datalist ;)
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_Recordset_Field_With_Negative_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(-1).field1]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Recordset index [ -1 ] is not greater than zero";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_Recordset_Field_With_Zero_Index_Expected_One_Replace_Success()
        {
            SetupArguments(ActivityStrings.ReplaceDataListShape, ActivityStrings.ReplaceDataListWithData, "[[recset1(0).field1]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Recordset index [ 0 ] is not greater than zero";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, GlobalConstants.ErrorPayload, out actual, out error);
            // remove test datalist ;)

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        #endregion Replace Negative Tests

        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachInputs")]
        public void DsfReplaceActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(FieldsToSearch, act.FieldsToSearch);
            Assert.AreEqual(Find, act.Find);
            Assert.AreEqual(ReplaceWith, act.ReplaceWith);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetOutputs")]
        public void GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[res]]", outputs[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachInputs")]
        public void DsfReplaceActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            var tuple1 = new Tuple<string, string>(FieldsToSearch, "Test");
            var tuple2 = new Tuple<string, string>(Find, "Test2");
            var tuple3 = new Tuple<string, string>(ReplaceWith, "Test3");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.Find);
            Assert.AreEqual("Test", act.FieldsToSearch);
            Assert.AreEqual("Test3", act.ReplaceWith);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachOutputs")]
        public void DsfReplaceActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachOutputs")]
        public void DsfReplaceActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(Result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_UpdateForEachOutputs")]
        public void DsfReplaceActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_GetForEachInputs")]
        public void DsfReplaceActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(3, dsfForEachItems.Count);
            Assert.AreEqual(FieldsToSearch, dsfForEachItems[0].Name);
            Assert.AreEqual(FieldsToSearch, dsfForEachItems[0].Value);
            Assert.AreEqual(Find, dsfForEachItems[1].Name);
            Assert.AreEqual(Find, dsfForEachItems[1].Value);
            Assert.AreEqual(ReplaceWith, dsfForEachItems[2].Name);
            Assert.AreEqual(ReplaceWith, dsfForEachItems[2].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfReplaceActivity_GetForEachOutputs")]
        public void DsfReplaceActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string FieldsToSearch = "[[Numeric(1).num]]";
            const string Find = "Up";
            const string Result = "[[res]]";
            const string ReplaceWith = "2";
            var act = new DsfReplaceActivity { FieldsToSearch = FieldsToSearch, Find = Find, ReplaceWith = ReplaceWith, Result = Result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(Result, dsfForEachItems[0].Name);
            Assert.AreEqual(Result, dsfForEachItems[0].Value);
        }



        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string fieldsToSearch, string find, string replaceWith, string result, bool caseMatch)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfReplaceActivity { FieldsToSearch = fieldsToSearch, Find = find, ReplaceWith = replaceWith, Result = result, CaseMatch = caseMatch }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
