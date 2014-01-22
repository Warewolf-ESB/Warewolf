using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for SortRecordsTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
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

        #region GetDebugInputs/Outputs

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void SortRecord_Get_Debug_Input_Output_With_Recordset_Using_Star_Index_With_Field_Expected_Pass()
        {
            DsfSortRecordsActivity act = new DsfSortRecordsActivity { SortField = "[[Customers(*).DOB]]", SelectedSort = "Forward" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);


            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(30, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 9479 
        /// </summary>
        [TestMethod]
        public void SortRecordGetDebugInputOutputWithRecordsetUsingBlankIndexWithFieldExpectedPass()
        {
            DsfSortRecordsActivity act = new DsfSortRecordsActivity { SortField = "[[Customers().DOB]]", SelectedSort = "Forward" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("Sort Field", inRes[0].ResultsList[0].Value);
            Assert.AreEqual("[[Customers(1).DOB]]", inRes[0].ResultsList[1].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[2].Value);
            Assert.AreEqual("1983/02/12", inRes[0].ResultsList[3].Value);
            Assert.AreEqual("[[Customers(2).DOB]]", inRes[0].ResultsList[4].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[5].Value);
            Assert.AreEqual("1981/05/15", inRes[0].ResultsList[6].Value);
            Assert.AreEqual("[[Customers(3).DOB]]", inRes[0].ResultsList[7].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[8].Value);
            Assert.AreEqual("1975/04/01", inRes[0].ResultsList[9].Value);
            Assert.AreEqual("[[Customers(4).DOB]]", inRes[0].ResultsList[10].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[11].Value);
            Assert.AreEqual("1981/10/01", inRes[0].ResultsList[12].Value);
            Assert.AreEqual("[[Customers(5).DOB]]", inRes[0].ResultsList[13].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[14].Value);
            Assert.AreEqual("1981/06/11", inRes[0].ResultsList[15].Value);
            Assert.AreEqual("[[Customers(6).DOB]]", inRes[0].ResultsList[16].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[17].Value);
            Assert.AreEqual("1982/09/15", inRes[0].ResultsList[18].Value);
            Assert.AreEqual("[[Customers(7).DOB]]", inRes[0].ResultsList[19].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[20].Value);
            Assert.AreEqual("1988/10/14", inRes[0].ResultsList[21].Value);
            Assert.AreEqual("[[Customers(8).DOB]]", inRes[0].ResultsList[22].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[23].Value);
            Assert.AreEqual("1988/09/23", inRes[0].ResultsList[24].Value);
            Assert.AreEqual("[[Customers(9).DOB]]", inRes[0].ResultsList[25].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[26].Value);
            Assert.AreEqual("1984/11/25", inRes[0].ResultsList[27].Value);
            Assert.AreEqual("[[Customers(10).DOB]]", inRes[0].ResultsList[28].Value);
            Assert.AreEqual("=", inRes[0].ResultsList[29].Value);
            Assert.AreEqual("1986/12/12", inRes[0].ResultsList[30].Value);

            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(30, outRes[0].FetchResultsList().Count);
            Assert.AreEqual("[[Customers(1).DOB]]", outRes[0].ResultsList[0].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[1].Value);
            Assert.AreEqual("1975/04/01", outRes[0].ResultsList[2].Value);
            Assert.AreEqual("[[Customers(2).DOB]]", outRes[0].ResultsList[3].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[4].Value);
            Assert.AreEqual("1981/05/15", outRes[0].ResultsList[5].Value);
            Assert.AreEqual("[[Customers(3).DOB]]", outRes[0].ResultsList[6].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[7].Value);
            Assert.AreEqual("1981/06/11", outRes[0].ResultsList[8].Value);
            Assert.AreEqual("[[Customers(4).DOB]]", outRes[0].ResultsList[9].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[10].Value);
            Assert.AreEqual("1981/10/01", outRes[0].ResultsList[11].Value);
            Assert.AreEqual("[[Customers(5).DOB]]", outRes[0].ResultsList[12].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[13].Value);
            Assert.AreEqual("1982/09/15", outRes[0].ResultsList[14].Value);
            Assert.AreEqual("[[Customers(6).DOB]]", outRes[0].ResultsList[15].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[16].Value);
            Assert.AreEqual("1983/02/12", outRes[0].ResultsList[17].Value);
            Assert.AreEqual("[[Customers(7).DOB]]", outRes[0].ResultsList[18].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[19].Value);
            Assert.AreEqual("1984/11/25", outRes[0].ResultsList[20].Value);
            Assert.AreEqual("[[Customers(8).DOB]]", outRes[0].ResultsList[21].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[22].Value);
            Assert.AreEqual("1986/12/12", outRes[0].ResultsList[23].Value);
            Assert.AreEqual("[[Customers(9).DOB]]", outRes[0].ResultsList[24].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[25].Value);
            Assert.AreEqual("1988/09/23", outRes[0].ResultsList[26].Value);
            Assert.AreEqual("[[Customers(10).DOB]]", outRes[0].ResultsList[27].Value);
            Assert.AreEqual("=", outRes[0].ResultsList[28].Value);
            Assert.AreEqual("1988/10/14", outRes[0].ResultsList[29].Value);
        }

        #endregion

        // ReSharper disable InconsistentNaming

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

        #endregion Private Test Methods
    }
}
