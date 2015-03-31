
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
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class IndexActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Index Positive Tests

        [TestMethod]
        public void Index_Recordset_With_Index_Expected_Index_Of_Four_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(1).field1]]", "First Occurrence", "ney", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            const string expected = "4";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void Index_Recordset_With_Star_And_Star_Search_Criteria_Expected_Index_Of_Four_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShapeWithThreeRecordsets, ActivityStrings.IndexDataListWithDataAndThreeRecordsets,
                           "[[Customers(*).FirstName]]", "First Occurrence", "[[recset1(*).field1]]", "Left To Right", "[[results(*).resField]]", "0");
            IDSFDataObject result = ExecuteProcess();

            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "results", "resField", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(24, actual.Count);

        }

       

        /// <summary>
        /// This method takes a recordset as input and outputs a single value
        /// </summary>
        [TestMethod]
        public void Index_Recordset_With_Star_And_Star_Search_Criteria_Numeric_Return_Field_Expected_RowWithValuesAsCSV()
        {
            const int Expected = 1;
            string error;
            //Create datalist
            SetupArguments(ActivityStrings.IndexDataListShapeWithThreeRecordsets, ActivityStrings.IndexDataListWithDataAndThreeRecordsets,
                           "[[Customers(*).FirstName]]", "First Occurrence", "[[recset1(*).field1]]", "Left To Right", "[[results(1).resField]]", "0");
            //Execute Find Index
            IDSFDataObject result = ExecuteProcess();

            //Get the result from Find Index
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "results", "resField", out error);

            //Datalist dispose
            DataListRemoval(result.DataListID);

            //check that there is only one row
            Assert.AreEqual(Expected, actual.Count);

        }

        [TestMethod]
        public void Index_Recordset_With_No_Index_Expected_Index_Of_One_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1().field1]]", "First Occurrence", "f1", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            const string Expected = "1";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);

        }

        [TestMethod]
        public void Index_Recordset_With_Star_Expected_Six_Different_Indexs_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(*).field1]]", "First Occurrence", "f1", "Left To Right", "[[Customers(*).FirstName]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "Customers", "FirstName", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(6, actual.Count);
            Assert.AreEqual("-1", actual[0]);

        }
        /// <summary>
        /// getting a scalar, returning a scalar
        /// </summary>
        [TestMethod]
        public void Index_Scalar_Expected_Index_Of_Four_Returned()
        {
            const string Expected = "4";

            string actual;
            string error;

            //create datalist
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[CompanyName]]", "First Occurrence", "2", "Left To Right", "[[res]]", "0");
            //run the tool
            IDSFDataObject result = ExecuteProcess();
            
            //get the result
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            //datalist dispose
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        public void Index_Scalar_RightToLeft_Expected_Index_Of_One_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[CompanyName]]", "First Occurrence", "2", "Right to Left", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            const string Expected = "1";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);

        }

        [TestMethod]
        public void Index_Scalar_Text_Not_Found_Expected_Index_Of_Negative_One_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[CompanyName]]", "First Occurrence", "zz", "Right to Left", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            const string Expected = "-1";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);

        }

        #endregion Index Positive Tests

        #region Index Negative Tests

        [TestMethod]
        public void Index_Recordset_With_Zero_Index_Expected_Error_In_Error_tag_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(0).field1]]", "First Occurrence", "f1", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            const string Expected = "<InnerError>Recordset index [ 0 ] is not greater than zero</InnerError>";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);

        }

        [TestMethod]
        public void Index_Recordset_With_Negative_Index_Expected_Error_In_Error_Tag_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListShape,
                           "[[recset1(-1).field1]]", "First Occurrence", "f1", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            const string Expected = "<InnerError>Recordset index [ -1 ] is not greater than zero</InnerError>";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);

        }

        [TestMethod]
        public void Index_Raw_Data_AllOccurrences_Expected_Success()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListShape,
                           "ABCFDEFGH", "All Occurrences", "F", "Left To Right", "[[res]]", "0");
            IDSFDataObject result = ExecuteProcess();
            const string Expected = "4,7";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "res", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("IndexActivity_Execute")]
        public void IndexActivity_Execute_MultipleResultsFields_Error()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListShape,
                           "ABCFDEFGH", "All Occurrences", "F", "Left To Right", "[[res]][[bes]]", "0");
            IDSFDataObject result = ExecuteProcess();
           


            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Dev2System.Dev2Error", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(actual.Contains("The result field only allows a single result"));
        }

        [TestMethod]
        public void Index_Recordset_With_Star_AllOccurrences_Expected_Seven_Different_Indexs_Returned()
        {
            SetupArguments(ActivityStrings.IndexDataListShape, ActivityStrings.IndexDataListWithData,
                           "[[recset1(*).field2]]", "All Occurrences", "2", "Left To Right", "[[Customers(*).FirstName]]", "0");
            IDSFDataObject result = ExecuteProcess();
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "Customers", "FirstName", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(7, actual.Count);
            Assert.AreEqual("2", actual[1]);

        }


        #endregion Index Negative Tests

        #region Get Input/Output Tests

        [TestMethod]
        public void IndexActivity_GetInputs_Expected_Five_Input()
        {
            DsfIndexActivity testAct = new DsfIndexActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(6, inputs.FetchAllEntries().Count);
        }

        [TestMethod]
        public void IndexActivity_GetOutputs_Expected_One_Output()
        {
            DsfIndexActivity testAct = new DsfIndexActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, outputs.FetchAllEntries().Count);
        }

        #endregion Get Input/Output Tests


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfIndexActivity_UpdateForEachInputs")]
        public void DsfIndexActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string inField = "[[CompanyName]]";
            const string characters = "2";
            var act = new DsfIndexActivity { InField = inField, Index = "First Occurance", Characters = characters, Direction = "Left To Right", Result = "[[res]]" };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(inField, act.InField);
            Assert.AreEqual(characters, act.Characters);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfIndexActivity_UpdateForEachInputs")]
        public void DsfIndexActivity_UpdateForEachInputs_MoreThan1Updates_Updates()
        {
            //------------Setup for test--------------------------
            const string inField = "[[CompanyName]]";
            const string characters = "2";
            var act = new DsfIndexActivity { InField = inField, Index = "First Occurance", Characters = characters, Direction = "Left To Right", Result = "[[res]]" };

            var tuple1 = new Tuple<string, string>(characters, "Test");
            var tuple2 = new Tuple<string, string>(inField, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.InField);
            Assert.AreEqual("Test", act.Characters);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfIndexActivity_UpdateForEachOutputs")]
        public void DsfIndexActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfIndexActivity { InField = "[[CompanyName]]", Index = "First Occurance", Characters = "2", Direction = "Left To Right", Result = result };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfIndexActivity_UpdateForEachOutputs")]
        public void DsfIndexActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfIndexActivity { InField = "[[CompanyName]]", Index = "First Occurance", Characters = "2", Direction = "Left To Right", Result = result };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfIndexActivity_UpdateForEachOutputs")]
        public void DsfIndexActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            var act = new DsfIndexActivity { InField = "[[CompanyName]]", Index = "First Occurance", Characters = "2", Direction = "Left To Right", Result = "[[res]]" };

            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfIndexActivity_GetForEachInputs")]
        public void DsfIndexActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string inField = "[[CompanyName]]";
            const string characters = "2";
            var act = new DsfIndexActivity { InField = inField, Index = "First Occurance", Characters = characters, Direction = "Left To Right", Result = "[[res]]" };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, dsfForEachItems.Count);
            Assert.AreEqual(inField, dsfForEachItems[0].Name);
            Assert.AreEqual(inField, dsfForEachItems[0].Value);
            Assert.AreEqual(characters, dsfForEachItems[1].Name);
            Assert.AreEqual(characters, dsfForEachItems[1].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfIndexActivity_GetForEachOutputs")]
        public void DsfIndexActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfIndexActivity { InField = "[[CompanyName]]", Index = "First Occurance", Characters = "2", Direction = "Left To Right", Result = result };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(result, dsfForEachItems[0].Name);
            Assert.AreEqual(result, dsfForEachItems[0].Value);
        }


        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string inField, string index, string characters, string direction, string resultValue, string startIndex)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfIndexActivity
                {
                    StartIndex = startIndex
                ,
                    InField = inField
                ,
                    Index = index
                ,
                    Characters = characters
                ,
                    Direction = direction
                ,
                    Result = resultValue
                }
            };

            CurrentDl = currentDL;
            TestData = testData;
        }

        #endregion Private Test Methods

    }
}
