
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class ReplaceActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Replace Positive Tests

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfReplaceActivity_Execute")]
        public void DsfReplaceActivity_Execute_MultipleResults_ExpectError()
        {
            SetupArguments(ActivityStrings.ReplaceSpecialCharsDataListWithData, ActivityStrings.ReplaceSpecialCharsDataListShape, "[[SpecialChar]]", @"\*+?|{[()^$# ", "It Worked", "[[res]],[[noot]]", false);

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
            Assert.IsNull(actual);
        }


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
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                string replacedRes;
                GetScalarValueFromDataList(result.DataListID, "SpecialChar", out replacedRes, out error);
                // remove test datalist ;)
                DataListRemoval(result.DataListID);

                Assert.AreEqual("It Worked", replacedRes);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<IBinaryDataListItem> dataListItems;
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<IBinaryDataListItem> dataListItems;
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);

                // remove test datalist ;)
                DataListRemoval(result.DataListID);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void Replace_With_Recset_ToFind_To_Replace_Expected_six_Replaced()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithData, ActivityStrings.ReplaceDataListShape, "[[recset1(*).field1]],[[Customers(*).FirstName]]", "[[ReplaceRecset(*).replace]]", "TEST", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"6";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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

            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "CompanyName", out actual, out error);
                // remove test datalist ;)
                DataListRemoval(result.DataListID);

                Assert.AreEqual("TheUnlimted", actual);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "CompanyName", out actual, out error);
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.AreEqual("Dev2", actual);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<IBinaryDataListItem> dataListItems;
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("barney", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);

                // remove test datalist ;)
                DataListRemoval(result.DataListID);

                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<IBinaryDataListItem> dataListItems;
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "FirstName", out dataListItems, out error);

                // remove test datalist ;)
                DataListRemoval(result.DataListID);

                Assert.AreEqual("Barney", dataListItems[0].TheValue);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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
            GetScalarValueFromDataList(result.DataListID, "Res", out actual, out error);

            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "Thing", out actual, out error);

                // remove test datalist ;)
                DataListRemoval(result.DataListID);

                Assert.AreEqual("testWallis", actual);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        [TestMethod]
        public void ReplaceScalarWithBracketExpectedOneReplace()
        {
            SetupArguments(@"<DataList><Thing>(0)</Thing><Res></Res></DataList>", @"<DataList><Thing></Thing><Res></Res></DataList>", @"[[Thing]]", @"(0)", @"+1", "[[Res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"1";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "Res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                GetScalarValueFromDataList(result.DataListID, "Thing", out actual, out error);
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.AreEqual("+1", actual);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<IBinaryDataListItem> dataListItems;
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out dataListItems, out error);
                Assert.AreEqual("Wallis", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field2", out dataListItems, out error);
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.AreEqual("Wallis", dataListItems[1].TheValue);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.Fail("The following errors occurred while retrieving datalist items\r\nerrors:{0}", error);
            }
        }

        //2013.06.12: Ashley Lewis for bug 9587 - replace handles spaces
        [TestMethod]
        public void ReplaceInTwoRecordsetsSeperatedBySpacesAndCommasExpectedTwoReplacesSuccess()
        {
            SetupArguments(ActivityStrings.ReplaceDataListWithSpacesInData, ActivityStrings.ReplaceDataListShapeForSpaces, "[[recset1(*).field]] [[Customers(*).Names]], [[ReplaceScalar]]", "Barney", "Wallis", "[[res]]", false);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"3";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            if(string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
                IList<IBinaryDataListItem> dataListItems;
                GetRecordSetFieldValueFromDataList(result.DataListID, "Customers", "Names", out dataListItems, out error);
                Assert.AreEqual("Wallis Buchan", dataListItems[0].TheValue);
                GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field", out dataListItems, out error);
                Assert.AreEqual("Wallis f2r1", dataListItems[0].TheValue);
                string scalarResult;
                GetScalarValueFromDataList(result.DataListID, "ReplaceScalar", out scalarResult, out error);
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
                Assert.AreEqual("Wallis abc123", scalarResult);
            }
            else
            {
                // remove test datalist ;)
                DataListRemoval(result.DataListID);
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
            const string expected = @"<InnerError>Please insert only variables into Fields To Search</InnerError>";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
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
            const string expected = @"<InnerError>Recordset index [ -1 ] is not greater than zero</InnerError>";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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
            const string expected = @"<InnerError>Recordset index [ 0 ] is not greater than zero</InnerError>";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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

        #region Get Input/Output Tests

        [TestMethod]
        public void CountRecordsetActivity_GetInputs_Expected_Four_Input()
        {
            DsfReplaceActivity testAct = new DsfReplaceActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(4, res);
        }

        [TestMethod]
        public void CountRecordsetActivity_GetOutputs_Expected_One_Output()
        {
            DsfReplaceActivity testAct = new DsfReplaceActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, res);

        }

        #endregion Get Input/Output Tests

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
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(FieldsToSearch, act.FieldsToSearch);
            Assert.AreEqual(Find, act.Find);
            Assert.AreEqual(ReplaceWith, act.ReplaceWith);
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
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2, tuple3 }, null);
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

            act.UpdateForEachOutputs(null, null);
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
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
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
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
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
