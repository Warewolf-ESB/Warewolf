/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using ActivityUnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;


namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DateTimeActivityTests : BaseActivityUnitTest
    {
        [TestInitialize]
        public void PreConditions()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-ZA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-ZA");

            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        //Added for BUG 9494
        [TestMethod]
        [Timeout(60000)]
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

            var result = ExecuteProcess();
            const string Expected = "1978/07/23 03:30 PM";
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(Expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
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

            var result = ExecuteProcess();
            const string expected = "2012/11/28 02:12:41 AM";
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
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

            var result = ExecuteProcess();
            var firstDateTime = DateTime.Parse("2012/11/27 04:12:41 PM").AddHours(10);
            var firstDateTimeExpected = firstDateTime.ToString("yyyy/MM/dd hh:mm:ss tt");
            var secondDateTime = DateTime.Parse("2012/12/27 04:12:41 PM").AddHours(10);
            var secondDateTimeExpected = secondDateTime.ToString("yyyy/MM/dd hh:mm:ss tt");
            GetRecordSetFieldValueFromDataList(result.Environment, "MyDateRecordSet", "Date", out IList<string> actual, out string error);
            // remove test datalist ;)
            var firstResult = actual[2];
            var secondResult = actual[3];
            // Assert to a result please
            Assert.AreEqual(firstDateTimeExpected, firstResult);
            Assert.AreEqual(secondDateTimeExpected, secondResult);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DateTimeAddSplitsExpectedDateTimeReturnedCorrectly()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2013/02/07 08:38:56.953 PM"
                         , "yyyy/mm/dd 12h:min:ss.sp am/pm"
                         , "yyyy/mm/dd 12h:min:ss.sp am/pm"
                         , "Milliseconds"
                         , 327
                         , "[[MyTestResult]]");

            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            // remove test datalist ;)

            const string expected = "2013/02/07 08:38:57.280 PM";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(DsfDateTimeActivity))]
        [Owner("Rory McGuire")]
        public void DsfDateTimeActivity_ExecuteWithBlankInput_DateTimeNowIsUsed()
        {
            var startTime = DateTime.Now;

            Thread.Sleep(1200);

            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         , ""
                         , ""
                         , ""
                         , 0
                         , "[[MyTestResult]]");

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            var parsedResult = DateTime.Parse(actual);

            Thread.Sleep(1100);

            var endTime = DateTime.Now;

            Assert.IsTrue(endTime >= parsedResult, $"expected a time <= ({endTime.ToString("yyyy/MM/ddThh:mm:ss.fff zzz")}) but got: '{parsedResult.ToString("yyyy/MM/ddThh:mm:ss.fff zzz")}'");
            Assert.IsTrue(parsedResult >= startTime, $"expected a time >= ({startTime.ToString("yyyy/MM/ddThh:mm:ss.fff zzz")}) but got: '{parsedResult.ToString("yyyy/MM/ddThh:mm:ss.fff zzz")}'");
            Assert.IsTrue(endTime >= parsedResult && parsedResult >= startTime, $"expected a time between starting this test ({startTime}) and ({endTime}) but got: '{parsedResult}' with Start Time Timezone {startTime.ToString("zzz")}, End Time Timezone {endTime.ToString("zzz")}, and Parsed Result Timezone {parsedResult.ToString("zzz")}");
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory(nameof(DsfDateTimeActivity))]
        [Owner("Rory McGuire")]
        public void DsfDateTimeActivity_ExecuteWithBlankInputAndMonthsOutput_OutputNotZero()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         , ""
                         , "MM"
                         , "Months"
                         , 0
                         , "[[MyTestResult]]");

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            Assert.AreEqual(DateTime.Now.ToString("MMMM"), actual, "Month mismatch");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfDateTimeActivity
            {
                DateTime = "",
                InputFormat = "",
                OutputFormat = "",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = "[[dt]]",
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[dt]]", outputs[0]);
        }

        void SetupArguments(string currentDL, string testData, string dateTime, string inputFormat, string outputFormat, string timeModifierType, int timeModifierAmount, string resultValue)
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
    }
}
