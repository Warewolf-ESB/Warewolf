using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for SortRecordsTest
    /// </summary>
    [TestClass]
    public class SortRecordsTest : BaseActivityUnitTest
    {
        public SortRecordsTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        #region Additional test attributes
        [ClassInitialize()]
        public static void BaseActivityUnitTestInitialize(TestContext testContext)
        {
            //            var pathToRedis = Path.Combine(testContext.DeploymentDirectory, "redis-server.exe");
            //            if (_redisProcess == null) _redisProcess = Process.Start(pathToRedis);
        }

        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void BaseActivityUnitTestCleanup()
        {
            //if(_redisProcess != null)
            //{
            //    _redisProcess.Kill();
            //}
        }

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Time", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Name", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recordSet", "Id", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

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
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);

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

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #endregion Negative Test Cases

        #region Get Input/Output Tests

        [TestMethod]
        public void SortRecords_GetInputs_Expected_Two_Input()
        {
            DsfSortRecordsActivity testAct = new DsfSortRecordsActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 2);
        }

        [TestMethod]
        public void SortRecords_GetOutputs_Expected_Zero_Output()
        {
            DsfSortRecordsActivity testAct = new DsfSortRecordsActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 0);
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

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
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

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
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
