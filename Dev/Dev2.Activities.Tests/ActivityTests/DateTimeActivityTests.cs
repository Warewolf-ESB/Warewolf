
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using ActivityUnitTests;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DateTimeActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region DateTime Tests

        //Added for BUG 9494
        [TestMethod]
        public void DateTimeUsingdWDatePartWithFullDateNameExpectedDateTimeReturnedCorrectly()
        {
            const string CurrDl = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(CurrDl
                         , CurrDl
                         , "Sunday, July 23 78 15:30"
                         , "dW', 'MM' 'dd' 'yy' '24h':'min"
                         , "yyyy/mm/dd 12h:min am/pm"
                         , ""
                         , 0
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();
            const string Expected = "1978/07/23 03:30 PM";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(Expected, actual);

        }

        [TestMethod]
        public void DateTime_NominalDateTimeInputs_Expected_DateTimeReturnedCorrectly()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2012/11/27 04:12:41 PM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Hours"
                         , 10
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();
            const string expected = "2012/11/28 02:12:41 AM";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void DateTime_RecordSetData_Expected_EachRecordSetAppendedWithChangedDateTime()
        {
            const string currDL = @"<root><MyDateRecordSet><Date></Date></MyDateRecordSet></root>";
            const string testData = @"<root><MyDateRecordSet><Date>2012/11/27 04:12:41 PM</Date></MyDateRecordSet><MyDateRecordSet><Date>2012/12/27 04:12:41 PM</Date></MyDateRecordSet></root>";
            SetupArguments(currDL
                         , testData
                         , "[[MyDateRecordSet(*).Date]]"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Hours"
                         , 10
                         , "[[MyDateRecordSet().Date]]");

            IDSFDataObject result = ExecuteProcess();
            DateTime firstDateTime = DateTime.Parse("2012/11/27 04:12:41 PM").AddHours(10);
            string firstDateTimeExpected = firstDateTime.ToString("yyyy/MM/dd hh:mm:ss tt");
            DateTime secondDateTime = DateTime.Parse("2012/12/27 04:12:41 PM").AddHours(10);
            string secondDateTimeExpected = secondDateTime.ToString("yyyy/MM/dd hh:mm:ss tt");
            IList<IBinaryDataListItem> actual;
            string error;
            GetRecordSetFieldValueFromDataList(result.DataListID, "MyDateRecordSet", "Date", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            var firstResult = actual[2].TheValue;
            var secondResult = actual[3].TheValue;
            // Assert to a result please
            Assert.AreEqual(firstDateTimeExpected, firstResult);
            Assert.AreEqual(secondDateTimeExpected, secondResult);
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

            string error;
            IList<IBinaryDataListItem> resultsList;
            IList<IBinaryDataListItem> other;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "f1", out other, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "resCol", "res", out resultsList, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("14/10/1990", resultsList[0].TheValue);
            Assert.AreEqual("10/01/1990", resultsList[1].TheValue);
            Assert.AreEqual("05/05/1990", resultsList[2].TheValue);
        }

        //2013.02.12: Ashley Lewis - Bug 8725, Task 8840 DONE
        [TestMethod]
        public void DateTimeAddSplitsExpectedDateTimeReturnedCorrectly()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2013/02/07 08:38:56.953 PM"
                         , "yyyy/mm/dd 12h:min:ss.sp am/pm"
                         , "yyyy/mm/dd 12h:min:ss.sp am/pm"
                         , "Split Secs"
                         , 327
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            const string expected = "2013/02/07 08:38:57.280 PM";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DateTimeComplexDateTimeInputsWithTrailingSpacesExpectedDateTimeReturnedCorrectly()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "Year 44 week 43 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 290 | Sunday | Sun | 7 |16 | 22 | 2044/10/16 10:25:36.953 PM A.D. "
                         , "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era "
                         , "'Year' yy 'week' ww 'yearweak' ZZZ | ZZ | Z | MM | M | m | dy | DW | dW | dw |d | 24h | yyyy/mm/dd 12h:min:ss.sp am/pm Era "
                         , "Years"
                         , 327
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();
            const string expected = "Year 71 week 42 yearweak (UTC+02:00) Harare, Pretoria | South Africa Standard Time | South Africa Standard Time | October | Oct | 10 | 289 | Saturday | Sat | 6 |16 | 22 | 2371/10/16 10:25:36.953 PM A.D. ";

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("DateTimeUnitTest")]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void DateTime_DateTimeUnitTest_ExecuteWithBlankInput_DateTimeNowIsUsed()
        // ReSharper restore InconsistentNaming
        {
            DateTime now = DateTime.Now;

            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         , ""
                         , ""
                         , "Seconds"
                         , 10
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);
            DateTime actualdt = DateTime.Parse(actual);
            var timeSpan = actualdt - now;

            Assert.IsTrue(timeSpan.TotalMilliseconds >= 9000, timeSpan.TotalMilliseconds + " is not >= 9000");
        }

        [TestMethod]
        [TestCategory("DateTimeUnitTest")]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void DateTime_DateTimeUnitTest_ExecuteWithBlankInputAndSplitSecondsOutput_OutputNotZero()
        // ReSharper restore InconsistentNaming
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         , ""
                         , "sp"
                         , "Seconds"
                         , 10
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);
            if(actual == "0")
            {
                Thread.Sleep(11);

                result = ExecuteProcess();

                GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

                Assert.IsTrue(actual != "0");
            }
            Assert.IsTrue(actual != "0");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeTool_Execute")]
        public void DsfDateTimeTool_Execute_MultipleResults_ExpectError()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2012/11/27 04:12:41 PM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Hours"
                         , 10
                         , "[[MyTestResult]][[moot]]");

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(Compiler.HasErrors(result.DataListID)); 
        }

        #endregion DateTime Tests

        #region Get Input/Output Tests

        [TestMethod]
        public void DateTimeActivity_GetInputs_Expected_Five_Input()
        {
            DsfDateTimeActivity testAct = new DsfDateTimeActivity { DateTime = "27-10-2012", InputFormat = "DD-mm-yyyy", TimeModifierType = "Days", TimeModifierAmount = 5, TimeModifierAmountDisplay = "5", OutputFormat = "DD-mm-yyyy", Result = "[[result]]" };

            IBinaryDataList inputs = testAct.GetInputs();

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(5, inputs.FetchAllEntries().Count);
        }

        [TestMethod]
        public void DateTimeActivity_GetOutputs_Expected_One_Output()
        {
            DsfDateTimeActivity testAct = new DsfDateTimeActivity { DateTime = "27-10-2012", InputFormat = "DD-mm-yyyy", TimeModifierType = "Days", TimeModifierAmount = 5, TimeModifierAmountDisplay = "5", OutputFormat = "DD-mm-yyyy", Result = "[[result]]" };

            IBinaryDataList outputs = testAct.GetOutputs();

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, outputs.FetchAllEntries().Count);
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
                    TimeModifierAmountDisplay = timeModifierAmount.ToString(CultureInfo.InvariantCulture)
                }
            };

            CurrentDl = currentDL;
            TestData = testData;
        }


        #endregion Private Test Methods

    }
}
