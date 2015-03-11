
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
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for SortRecordsTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class SortRecordsTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Forward Sort Tests

        [TestMethod]
        public void FieldSpecifiedSortForwards_Numeric_Expected_Recordset_Sorted_Top_To_Bottom()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = "[[recset().Id]]", SelectedSort = "Forward" }

            };

            SetupArguments(
                            ActivityStrings.SortDataList_Shape
                          , ActivityStrings.SortDataList
                          , "[[recset().Id]]"
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "1"
                                                     , "1"
                                                     , "2"
                                                     , "3"
                                                     , "4"
                                                     , "6"
                                                     , "7"
                                                     , "8"
                                                     , "9"
                                                     , "10" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfSortRecordsActivity_Execute_Gaps")]
        public void FieldSpecifiedSortForwards_Numeric_GapsInNonSortedField_Expected_Recordset_Sorted_Top_To_Bottom()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = "[[recset().Id]]", SelectedSort = "Forward" }

            };

            SetupArguments(
                            ActivityStrings.SortDataList_Shape
                          , ActivityStrings.SortDataListGaps
                          , "[[recset().Id]]"
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { ""
                                                     , ""
                                                     , "1"
                                                     , "1"
                                                     , "2"
                                                     , "3"
                                                     , "6"
                                                     , "7"
                                                     , "9"
                                                     , "10" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfSortRecordsActivity_Execute_Gaps")]
        public void FieldSpecifiedSortForwards_String_GapsInNonSortedField_Expected_Recordset_Sorted_Backwards()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = "[[recset().Name]]", SelectedSort = "Forward" }

            };

            SetupArguments(
                            ActivityStrings.SortDataList_Shape
                          , ActivityStrings.SortDataListGaps
                          , "[[recset().Name]]"
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { ""
                                                     , ""
                                                     , ""
                                                     , "A"
                                                     , "B"
                                                     , "F"
                                                     , "F"
                                                     , "L"
                                                     , "Y"
                                                     , "Z" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Name", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }




        [TestMethod]
        public void SortActivity_DateTimeSortForward_Expected_RecordSetSortedAscendingDateTime()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = "[[recset().Time]]", SelectedSort = "Forward" }

            };

            SetupArguments(
                            ActivityStrings.SortDataList_Shape
                          , ActivityStrings.SortDataList
                          , "[[recset().Time]]"
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "Monday, November 17, 2008 02:11:59 AM"
                                                     , "Monday, November 17, 2008 04:11:59 AM"
                                                     , "Monday, November 17, 2008 05:11:59 AM"
                                                     , "Monday, November 17, 2008 09:11:59 AM"
                                                     , "Monday, November 17, 2008 10:11:59 AM"
                                                     , "Monday, November 17, 2008 11:11:59 AM"
                                                     , "Monday, November 17, 2008 05:11:59 PM"
                                                     , "Monday, November 17, 2008 11:10:59 PM"
                                                     , "Sunday, November 30, 2008 05:11:59 PM"
                                                     , "Wednesday, June 27, 2012 08:10:00 AM"  };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Time", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void String_ForwardSort_String_Expected_RecordSetSortedAscending()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = "[[recset().Name]]", SelectedSort = "Forward" }

            };

            SetupArguments(
                            ActivityStrings.SortDataList_Shape
                          , ActivityStrings.SortDataList
                          , "[[recset().Name]]"
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "A"
                                                     , "A"
                                                     , "A"
                                                     , "B"
                                                     , "C"
                                                     , "F"
                                                     , "F"
                                                     , "L"
                                                     , "Y"
                                                     , "Z" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Name", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void SortActivity_MultipleRecordSetContainingSameSortValue_DateTime_SortedWithTheRecordSetAppearingMultipleTimes()
        {
            SetupArguments(
                            ActivityStrings.SortActivity_SingleEmptyField_Shape
                          , ActivityStrings.SortActivity_SingleEmptyField
                          , "[[recordSet().Id]]"
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> {string.Empty 
                                                     ,"1"
                                                     , "2"
                                                     , "3"
                                                     , "4"
                                                     , "5"
                                                     , "6"
                                                     , "7"
                                                     , "8"
                                                     , "9"
                                                     , "11"
                                                     , "12"
                                                     , "13"
                                                     , "14"
                                                     , "15"
                                                     , "16"
                                                     , "17"
                                                     , "18"
                                                     , "19"
                                                     , "20" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recordSet", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void NoFieldSpecifiedSortForwards_Expected_No_Change()
        {
            SetupArguments(
                            TestData = ActivityStrings.SortDataList_Shape
                          , TestData = ActivityStrings.SortDataList
                          , ""
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "1", "4", "3", "10", "8", "6", "2", "7", "9", "1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion Forward Sort Tests

        #region Backward Sort Tests

        [TestMethod]
        public void FieldSpecifiedSortBackwards_Expected_Recordset_Sorted_Bottom_To_Top()
        {

            SetupArguments(
                            "<root><recset><Id/><Tel/><Name/><Time/></recset></root>"
                          , ActivityStrings.SortDataList
                          , "[[recset().Id]]"
                          , "Backwards"
                          );

            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "10", "9", "8", "7", "6", "4", "3", "2", "1", "1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void NoFieldSpecifiedSortBackwards_Expected_Reverse_Order()
        {
            SetupArguments(
                            TestData = ActivityStrings.SortDataList_Shape
                          , TestData = ActivityStrings.SortDataList
                          , ""
                          , "Backwards"
                          );
            IDSFDataObject result = ExecuteProcess();
            List<string> expected = new List<string> { "1", "4", "3", "10", "8", "6", "2", "7", "9", "1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        #endregion Backward Sort Tests

        #region Negative Test Cases

        [TestMethod]
        public void RecordsetDoesntExist_Expected_No_Change()
        {
            SetupArguments(
                            TestData = ActivityStrings.SortDataList_Shape
                          , TestData = ActivityStrings.SortDataList
                          , ""
                          , "Forward"
                          );
            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "1", "4", "3", "10", "8", "6", "2", "7", "9", "1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void FieldDoesntExistInRecordset_Expected_No_Change()
        {
            SetupArguments(
                            TestData = ActivityStrings.SortDataList_Shape
                          , TestData = ActivityStrings.SortDataList
                          , "[[recset().DoesntExisit]]"
                          , "Forward"
                          );

            IDSFDataObject result = ExecuteProcess();

            List<string> expected = new List<string> { "1", "4", "3", "10", "8", "6", "2", "7", "9", "1" };
            string error;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Error_Handled_Correctly()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = null, SelectedSort = null }

            };

            TestData = ActivityStrings.SortDataList;
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfSortRecordsActivity_Execute")]
        public void DsfSortRecordsActivity_Execute_MultipleColumnSort_ExpectError()
        {
            SetupArguments(
                            TestData = ActivityStrings.SortDataList_Shape
                          , TestData = ActivityStrings.SortDataList
                          , "[[recset().DoesntExisit]],[[recset().DoesntExisit]]"
                          , "Forward"
                          );

            TestData = ActivityStrings.SortDataList;
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.FetchErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.AreEqual("1 The sort field is invalid. You may only sort on a single RecordSet columns", res.Trim());
        }

        #endregion Negative Test Cases

        #region Get Input/Output Tests

        [TestMethod]
        public void SortRecords_GetInputs_Expected_Two_Input()
        {
            DsfSortRecordsActivity testAct = new DsfSortRecordsActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(2, res);
        }

        [TestMethod]
        public void SortRecords_GetOutputs_Expected_Zero_Output()
        {
            DsfSortRecordsActivity testAct = new DsfSortRecordsActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(0, res);
        }

        #endregion Get Input/Output Tests

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_UpdateForEachInputs")]
        public void DsfSortRecordsActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(SortField, act.SortField);
        }



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_UpdateForEachInputs")]
        public void DsfSortRecordsActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(SortField, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.SortField);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_UpdateForEachInputs")]
        public void DsfSortRecordsActivity_UpdateForEachInputs_1Update_UpdateInputPath()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            var tuple1 = new Tuple<string, string>(SortField, "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.SortField);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_UpdateForEachOutputs")]
        public void DsfSortRecordsActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(SortField, act.SortField);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_UpdateForEachOutputs")]
        public void DsfSortRecordsActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(SortField, act.SortField);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_UpdateForEachOutputs")]
        public void DsfSortRecordsActivity_UpdateForEachOutputs_1Updates_UpdateCommandResult()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            var tuple1 = new Tuple<string, string>(SortField, "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.SortField);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_GetForEachInputs")]
        public void DsfSortRecordsActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(SortField, dsfForEachItems[0].Name);
            Assert.AreEqual(SortField, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfSortRecordsActivity_GetForEachOutputs")]
        public void DsfSortRecordsActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            const string SortField = "[[Company().Name]]";
            var act = new DsfSortRecordsActivity { SortField = SortField };

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(SortField, dsfForEachItems[0].Name);
            Assert.AreEqual(SortField, dsfForEachItems[0].Value);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfSortRecordsActivity_Execute")]
        public void DsfSortRecordsActivity_Execute_GetDebugInputs_ExpectCorrectInputs()
        {
            DsfSortRecordsActivity act = SetupArgumentsReturnObj(
                            TestData = ActivityStrings.SortDataList_Shape
                          , TestData = ActivityStrings.SortDataList
                          , "[[recset().DoesntExisit]]"
                          , "Forward"
                          );

            TestData = ActivityStrings.SortDataList;
            IDSFDataObject result = ExecuteProcess(isDebug:true);
            var debug = act.GetDebugInputs(null);
            Assert.AreEqual(debug.Count,2);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
          }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfSortRecordsActivity_Execute")]
        public void DsfSortRecordsActivity_Execute_GetDebugOutputs_ExpectCorrectCount()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = "[[recset().Id]]", SelectedSort = "Forward" }

            };

            var act = SetupArgumentsReturnObj(
                            ActivityStrings.SortDataList_Shape
                          , ActivityStrings.SortDataList
                          , "[[recset().Id]]"
                          , "Forward"
                          
                          );
            IDSFDataObject result = ExecuteProcess(isDebug:true);



            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            var debugOut = act.GetDebugOutputs(null);
            Assert.AreEqual(1,debugOut.Count);
            Assert.AreEqual(10,debugOut[0].ResultsList.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfSortRecordsActivity_Execute")]
        public void DsfSortRecordsActivity_Execute_SingleRecordSetRuleCalled_ExpectNoResultsSortFieldIncorrect()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = "[[recset()]]", SelectedSort = "Forward" }

            };

            var act = SetupArgumentsReturnObj(
                            ActivityStrings.SortDataList_Shape
                          , ActivityStrings.SortDataList
                          , "[[recset()]]"
                          , "Forward"

                          );
            IDSFDataObject result = ExecuteProcess(isDebug: true);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            var debugOut = act.GetDebugOutputs(null);
            Assert.AreEqual(0, debugOut.Count);
        }
        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string sortField, string selectedSort)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfSortRecordsActivity { SortField = sortField, SelectedSort = selectedSort }
            };

            CurrentDl = currentDL;
            TestData = testData;
        }
        private DsfSortRecordsActivity SetupArgumentsReturnObj(string currentDL, string testData, string sortField, string selectedSort)
        {
            var act = new DsfSortRecordsActivity { SortField = sortField, SelectedSort = selectedSort };
            TestStartNode = new FlowStep
            {
                Action = act
            };

            CurrentDl = currentDL;
            TestData = testData;
            return  act;
        }
        #endregion Private Test Methods
    }
}
