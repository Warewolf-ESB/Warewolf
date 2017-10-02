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
using System.Globalization;
using System.Threading;
using ActivityUnitTests;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;


namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
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
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);

            // remove test datalist ;)

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
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);

            // remove test datalist ;)

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
            GetRecordSetFieldValueFromDataList(result.Environment, "MyDateRecordSet", "Date", out IList<string> actual, out string error);
            // remove test datalist ;)
            var firstResult = actual[2];
            var secondResult = actual[3];
            // Assert to a result please
            Assert.AreEqual(firstDateTimeExpected, firstResult);
            Assert.AreEqual(secondDateTimeExpected, secondResult);
        }
        
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

            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            // remove test datalist ;)

            const string expected = "2013/02/07 08:38:57.280 PM";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("DateTimeUnitTest")]
        [Owner("Massimo Guerrera")]
        
        public void DateTime_DateTimeUnitTest_ExecuteWithBlankInput_DateTimeNowIsUsed()

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
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            DateTime actualdt = DateTime.Parse(actual);
            var timeSpan = actualdt - now;

            Assert.IsTrue(timeSpan.TotalMilliseconds >= 9000, timeSpan.TotalMilliseconds + " is not >= 9000");
        }

        [TestMethod]
        [TestCategory("DateTimeUnitTest")]
        [Owner("Massimo Guerrera")]
        
        public void DateTime_DateTimeUnitTest_ExecuteWithBlankInputAndSplitSecondsOutput_OutputNotZero()

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
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            if (actual == "0")
            {
                Thread.Sleep(11);

                result = ExecuteProcess();

                GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out actual, out error);

                Assert.IsTrue(actual != "0");
            }
            Assert.IsTrue(actual != "0");
        }
        #endregion DateTime Tests


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfDateTimeActivity
            {
                DateTime = "", InputFormat = "", OutputFormat = "",TimeModifierType = "",
                TimeModifierAmount = 1, Result = "[[dt]]",
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[dt]]", outputs[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_AddDays_ShouldNotChangeAMtoPMValues()
        {
            //------------Setup for test--------------------------
            var expected = new DateTime(2017, 10, 20, 0, 0, 0, 0).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " hh:mm:ss.f tt");
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "10/25/2017 12:00:00 AM"
                         , ""
                         , ""
                         , "Days"
                         , -5
                         , "[[MyTestResult]]");
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "[[MyTestResult]]", out actual, out error);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_AddDays_ShouldNotChangePMtoAMValues()
        {
            //------------Setup for test--------------------------
            var dateTimeVal = new DateTime(2017, 10, 20, 0, 0, 0, 0).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " hh:mm:ss.f tt");
            var expected = dateTimeVal.Replace("AM","PM");
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "10/25/2017 12:00:00 PM"
                         , ""
                         , ""
                         , "Days"
                         , -5
                         , "[[MyTestResult]]");
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "[[MyTestResult]]", out actual, out error);
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, actual);
        }

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
