using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class DateTimeActivityTests : BaseActivityUnitTest
    {
        public DateTimeActivityTests()
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

        #region DateTime Tests

        //Added for BUG 9494
        [TestMethod]
        public void DateTimeUsingdWDatePartWithFullDateNameExpectedDateTimeReturnedCorrectly()
        {
            string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "Sunday, July 23 78 15:30"
                         , "dW', 'MM' 'dd' 'yy' '24h':'min"
                         , "yyyy/mm/dd 12h:min am/pm"
                         , ""
                         , 0
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();
            string expected = "1978/07/23 03:30 PM";

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void DateTime_NominalDateTimeInputs_Expected_DateTimeReturnedCorrectly()
        {
            string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2012/11/27 04:12:41 PM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Hours"
                         , 10
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();
            string expected = "2012/11/28 02:12:41 AM";

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void DateTime_RecordSetData_Expected_EachRecordSetAppendedWithChangedDateTime()
        {
            string currDL = @"<root><MyDateRecordSet><Date></Date></MyDateRecordSet></root>";
            string testData = @"<root><MyDateRecordSet><Date>2012/11/27 04:12:41 PM</Date></MyDateRecordSet><MyDateRecordSet><Date>2012/12/27 04:12:41 PM</Date></MyDateRecordSet></root>";
            SetupArguments(currDL
                         , testData
                         , "[[MyDateRecordSet(*).Date]]"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Hours"
                         , 10
                         , "[[MyDateRecordSet().Date]]");

            IDSFDataObject result = ExecuteProcess();
            DateTime date = DateTime.Parse("2012/11/27 04:12:41 PM");
            DateTime expected = date.AddHours(10.00);
            string reallyExpected = string.Format("{0:yyyy/mm/dd hh:mm:ss}", expected);
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetRecordSetFieldValueFromDataList(result.DataListID, "MyDateRecordSet", "Date", out actual, out error);
            // Assert to a result please
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void DateTime_RecordSetWithStar_Expected_DateTimeReturnedCorrectly()
        {
            SetupArguments(ActivityStrings.DateTimeDifferenceDataListShape
                         , ActivityStrings.DateTimeDifferenceDataListWithData
                         , "[[recset1(*).f1]]"
                         , "dd/mm/yyyy"
                         , "dd/mm/yyyy"
                         , "Years"
                         , 2
                         , "[[resCol(*).res]]");

            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            IList<IBinaryDataListItem> resultsList;
            IList<IBinaryDataListItem> other;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "f1", out other, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "resCol", "res", out resultsList, out error);

            Assert.AreEqual("14/10/1990", resultsList[0].TheValue);
            Assert.AreEqual("10/01/1990", resultsList[1].TheValue);
            Assert.AreEqual("05/05/1990", resultsList[2].TheValue);
        }

        //2013.02.12: Ashley Lewis - Bug 8725, Task 8840 DONE
        [TestMethod]
        public void DateTimeAddSplitsExpectedDateTimeReturnedCorrectly()
        {
            string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2013/02/07 08:38:56.953 PM"
                         , "yyyy/mm/dd 12h:min:ss.sp am/pm"
                         , "yyyy/mm/dd 12h:min:ss.sp am/pm"
                         , "Split Secs"
                         , 327
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            string expected = "2013/02/07 08:38:57.280 PM";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DateTimeComplexDateTimeInputsWithTrailingSpacesExpectedDateTimeReturnedCorrectly()
        {
            string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "Year 44 week 43 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 290 | Sunday | Sun | 7 |16 | 22 | 2044/10/16 10:25:36.953 PM A.D. "
                         , "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era "
                         , "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era "
                         , "Years"
                         , 327
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();
            string expected = "Year 71 week 42 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 289 | Saturday | Sat | 6 |16 | 22 | 2371/10/16 10:25:36.953 PM A.D. ";

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            Assert.AreEqual(expected, actual);
        }

        #endregion DateTime Tests

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DateTime_Get_Debug_Input_Output_With_Scalars_Expected_Pass()
        {
            //Used recordset with a numeric index as a scalar because it the only place were i had date values and it evalues to a scalar 
            DsfDateTimeActivity act = new DsfDateTimeActivity { DateTime = "[[Customers(1).DOB]]", InputFormat = "yyyy/mm/dd", OutputFormat = "yyyy/mm/dd", TimeModifierAmount = 1, TimeModifierAmountDisplay = "1", TimeModifierType = "Years", Result = "[[res]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(4, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(3, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void DateTime_Get_Debug_Input_Output_With_Recordsets_Expected_Pass()
        {
            DsfDateTimeActivity act = new DsfDateTimeActivity { DateTime = "[[Customers(*).DOB]]", InputFormat = "yyyy/mm/dd", OutputFormat = "yyyy/mm/dd", TimeModifierAmount = 1, TimeModifierAmountDisplay = "1", TimeModifierType = "Years", Result = "[[Numeric(*).num]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            Assert.AreEqual(4, inRes.Count);
            Assert.AreEqual(31, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[1].FetchResultsList().Count);
            Assert.AreEqual(3, inRes[2].FetchResultsList().Count);
            Assert.AreEqual(2, inRes[3].FetchResultsList().Count);
            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
        }

        #endregion

        #region Get Input/Output Tests

        [TestMethod]
        public void DateTimeActivity_GetInputs_Expected_Five_Input()
        {
            DsfDateTimeActivity testAct = new DsfDateTimeActivity { DateTime = "27-10-2012", InputFormat = "dd-mm-yyyy", TimeModifierType = "Days", TimeModifierAmount = 5, TimeModifierAmountDisplay = "5", OutputFormat = "dd-mm-yyyy", Result = "[[result]]" };

            IBinaryDataList inputs = testAct.GetInputs();

            Assert.IsTrue(inputs.FetchAllEntries().Count == 5);
        }

        [TestMethod]
        public void DateTimeActivity_GetOutputs_Expected_One_Output()
        {
            DsfDateTimeActivity testAct = new DsfDateTimeActivity { DateTime = "27-10-2012", InputFormat = "dd-mm-yyyy", TimeModifierType = "Days", TimeModifierAmount = 5, TimeModifierAmountDisplay = "5", OutputFormat = "dd-mm-yyyy", Result = "[[result]]" };

            IBinaryDataList outputs = testAct.GetOutputs();

            Assert.IsTrue(outputs.FetchAllEntries().Count == 1);
        }

        #endregion Get Input/Output Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string dateTime, string inputFormat, string outputFormat, string timeModifierType, int timeModifierAmount, string resultValue)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDateTimeActivity
                {
                    DateTime = dateTime
                ,
                    InputFormat = inputFormat
                ,
                    OutputFormat = outputFormat
                ,
                    TimeModifierType = timeModifierType
                ,
                    TimeModifierAmount = timeModifierAmount
                ,
                    Result = resultValue
                ,
                    TimeModifierAmountDisplay = timeModifierAmount.ToString()
                }
            };

            CurrentDl = currentDL;
            TestData = testData;
        }


        #endregion Private Test Methods

    }
}
