/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using NodaTime;
using System.Diagnostics;
using Dev2.Activities.DateAndTime;
using Moq;
using Warewolf.Storage;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class DotNetDateTimeActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region DateTime Tests

        [TestMethod]
        public void DateTime_NominalDateTimeInputs_Expected_DateTimeReturnedCorrectly()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2012/11/27 04:12:41 PM"
                         , "yyyy/MM/dd hh:mm:ss tt"
                         , "yyyy/MM/dd hh:mm:ss tt"
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
        public void DateTime_RecordSetData_Expected_EachRecordSetAppendedWithChangedDateTime()
        {
            const string currDL = @"<root><MyDateRecordSet><Date></Date></MyDateRecordSet></root>";
            const string testData = @"<root><MyDateRecordSet><Date>2012/11/27 04:12:41 PM</Date></MyDateRecordSet><MyDateRecordSet><Date>2012/12/27 04:12:41 PM</Date></MyDateRecordSet></root>";
            SetupArguments(currDL
                         , testData
                         , "[[MyDateRecordSet(*).Date]]"
                         , "yyyy/MM/dd hh:mm:ss tt"
                         , "yyyy/MM/dd hh:mm:ss tt"
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
        public void DateTimeAddSplitsExpectedDateTimeReturnedCorrectly()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , "2013/02/07 08:38:56.953 PM"
                         , "yyyy/MM/dd hh:mm:ss.FFFF tt"
                         , "yyyy/MM/dd hh:mm:ss.FFFF tt"
                         , "Milliseconds"
                         , 327
                         , "[[MyTestResult]]");

            var result = ExecuteProcess();

            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            // remove test datalist ;)

            const string expected = "2013/02/07 08:38:57.28 PM";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("DateTimeUnitTest")]
        [Owner("Massimo Guerrera")]

        public void DateTime_DateTimeUnitTest_ExecuteWithBlankInput_DateTimeNowIsUsed()

        {
            var now = DateTime.Now;

            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         , ""
                         , ""
                         , "Seconds"
                         , 10
                         , "[[MyTestResult]]");

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            var actualdt = DateTime.Parse(actual, CultureInfo.InvariantCulture);
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
                         , "FFFF"
                         , "Seconds"
                         , 10
                         , "[[MyTestResult]]");

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            if (actual == "0")
            {
                Thread.Sleep(11);

                result = ExecuteProcess();

                GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out actual, out error);

                Assert.AreNotEqual("0", actual);
            }
            Assert.AreNotEqual("0", actual);
        }

        [TestMethod]
        [TestCategory("DateTimeUnitTest")]

        public void DateTime_DateTimeUnitTest_ExecuteWithBlankInputAndSplitSecondsOutput_ValidateDateTimeFunction()

        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL, currDL, "", "", "FFFF", "Seconds", 10, "[[MyTestResult]]");

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
            if (actual == "0")
            {
                Thread.Sleep(11);

                result = ExecuteProcess();

                GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out actual, out error);

                Assert.AreNotEqual("0", actual);
            }
            Assert.AreNotEqual("0", actual);
        }
        #endregion DateTime Tests


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfDotNetDateTimeActivity
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

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_Execute_Blank_ShouldHaveNoErrorWithDebugOutput()
        {
            //------------Setup for test--------------------------
            const string varName = "[[dt]]";
            var act = new DsfDotNetDateTimeActivity
            {
                DateTime = "",
                InputFormat = "",
                OutputFormat = "",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = varName,
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            var dataMock = new Mock<IDSFDataObject>();
            dataMock.Setup(o => o.IsDebugMode()).Returns(() => true);
            var executionEnvironment = new ExecutionEnvironment();
            dataMock.Setup(o => o.Environment).Returns(executionEnvironment);
            var data = dataMock.Object;

            var timeBefore = DateTime.Now;
            //------------Execute Test---------------------------
            act.Execute(data, 0);
            //------------Assert Results-------------------------
            var timeAfter = DateTime.Now;

            var debugout = act.GetDebugOutputs(executionEnvironment, 0);
            var value = executionEnvironment.EvalAsListOfStrings(varName, 0);
            Assert.AreEqual(1,value.Count);
            DateTime datetimeResult;
            Assert.IsTrue(DateTime.TryParse(value[0], out datetimeResult),$"Failed to parse value: {value[0]} as a DateTime");
            Assert.IsTrue(timeBefore <= datetimeResult,$"{timeBefore} not <= {datetimeResult}");
            Assert.IsTrue(datetimeResult <= timeAfter,$"{datetimeResult} not <= {timeAfter}");

            Assert.AreEqual(false, debugout[0].ResultsList[0].HasError);
            Assert.AreEqual(varName, debugout[0].ResultsList[0].Variable);
            Assert.AreEqual(DebugItemResultType.Variable, debugout[0].ResultsList[0].Type);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_Execute_InvalidDateTime_ShouldHaveErrorWithDebugOutput()
        {
            //------------Setup for test--------------------------
            const string varName = "[[dt]]";
            var act = new DsfDotNetDateTimeActivity
            {
                DateTime = "a/p/R",
                InputFormat = "",
                OutputFormat = "",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = varName,
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            var dataMock = new Mock<IDSFDataObject>();
            dataMock.Setup(o => o.IsDebugMode()).Returns(() => true);
            var executionEnvironment = new ExecutionEnvironment();
            dataMock.Setup(o => o.Environment).Returns(executionEnvironment);
            var data = dataMock.Object;

            var timeBefore = DateTime.Now;

            //------------Execute Test---------------------------
            var activity = act.Execute(data, 0);
            //------------Assert Results-------------------------
            var timeAfter = DateTime.Now;

            var debugout = act.GetDebugOutputs(executionEnvironment, 0);
            var value = executionEnvironment.EvalAsListOfStrings(varName, 0);
            Assert.AreEqual(value.Count, 1);
            Assert.AreEqual("", value[0]);
            Assert.AreEqual(false, debugout[0].ResultsList[0].HasError);
            Assert.AreEqual(varName, debugout[0].ResultsList[0].Variable);
            Assert.AreEqual(DebugItemResultType.Variable, debugout[0].ResultsList[0].Type);

            Assert.AreEqual("The string was not recognized as a valid DateTime. There is an unknown word starting at index 0.", executionEnvironment.FetchErrors());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DsfDateTimeActivity_GetOutputs")]
        public void DsfDateTimeActivity_CurrentCulture_Called_ShouldPassAllDatesWithoutErrors()
        {
            //------------Setup for test--------------------------
            var faiCount = 0;
            var passCount = 0;
            var total = 0;
            var n = @"C:\Users\nkosinathi.sangweni\Desktop\New Text Document.txt";
            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (var culture in allCultures)
            {
                var cultures = new List<string>();
                foreach (var format in culture.DateTimeFormat.GetAllDateTimePatterns())
                {
                    cultures.Add(format);
                }

                var now = DateTime.Now;

                foreach (var item in cultures)
                {
                    const string currDL = @"<root><MyTestResult></MyTestResult></root>";
                    SetupArguments(currDL
                                 , currDL
                                 , now.ToString()
                                 , ""
                                 , item
                                 , "Years"
                                 , 10
                                 , "[[MyTestResult]]");

                    var result = ExecuteProcess();
                    var a = now.ToString(item);
                    GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);
                    total++;
                    try
                    {
                        // remove test datalist ;)
                        var allErrors = result.Environment.FetchErrors();
                        var hasErrors = string.IsNullOrEmpty(allErrors);
                        var asDate = DateTime.ParseExact(actual, item, culture, DateTimeStyles.AdjustToUniversal);


                        var localDate = new LocalDate(now.Year, now.Month, now.Day);
                        var localDate1 = new LocalDate(asDate.Year, asDate.Month, asDate.Day);
                        var period = Period.Between(localDate, localDate1);

                        Assert.IsTrue(hasErrors);
                        if (item.ToUpper().Contains("y".ToUpper()))
                        {
                            Assert.IsTrue(period.Years >= 9, "this format has failed " + item);
                        }
                        else
                        {
                            Assert.AreEqual(a, actual);
                        }

                        passCount++;
                        //using (var stream = File.AppendText(n))
                        //{                            
                        //    stream.WriteLine(item);
                        //    stream.Flush();
                        //}
                       
                    }
                    catch (Exception e)
                    {
                        // Debug.WriteLine(actual + " "+ item+" "+ culture);
                        faiCount++;
                    }
                }
            }
            Debug.WriteLine(faiCount + " " + "failures");
            Debug.WriteLine(passCount + " " + "Passed");
            Debug.WriteLine(total + " " + "total");

            //Assert.AreEqual(13590, faiCount);
            //Assert.AreEqual(15877, passCount);
            //Assert.AreEqual(29467, total);

        }

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string dateTime, string inputFormat, string outputFormat, string timeModifierType, int timeModifierAmount, string resultValue)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetDateTimeActivity
                {
                    DateTime = dateTime,
                    InputFormat = inputFormat,
                    OutputFormat = outputFormat,
                    TimeModifierType = timeModifierType,
                    TimeModifierAmount = timeModifierAmount,
                    Result = resultValue,
                    TimeModifierAmountDisplay = timeModifierAmount.ToString(CultureInfo.InvariantCulture)
                }
            };

            CurrentDl = currentDL;
            TestData = testData;
        }


        #endregion Private Test Methods

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeActivity_Equality")]
        public void DsfDotNetDateTimeActivity_Equal()
        {
            //------------Setup for test--------------------------
            var act1 = new DsfDotNetDateTimeActivity
            {
                DateTime = "",
                InputFormat = "",
                OutputFormat = "",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = "[[dt]]",
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            var act2 = new DsfDotNetDateTimeActivity
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
            //------------Assert Results-------------------------
            Assert.IsTrue(act1.Equals(act2));
            string tmp_holder;

            tmp_holder = act2.DateTime;
            act2.DateTime = "today";
            Assert.IsFalse(act1.Equals(act2));
            act2.DateTime = tmp_holder;

            tmp_holder = act2.InputFormat;
            act2.InputFormat = "today";
            Assert.IsFalse(act1.Equals(act2));
            act2.InputFormat = tmp_holder;

            tmp_holder = act2.OutputFormat;
            act2.OutputFormat = "today";
            Assert.IsFalse(act1.Equals(act2));
            act2.OutputFormat = tmp_holder;

            tmp_holder = act2.TimeModifierType;
            act2.TimeModifierType = "today";
            Assert.IsFalse(act1.Equals(act2));
            act2.TimeModifierType = tmp_holder;

            var num_tmp_holder = act2.TimeModifierAmount;
            act2.TimeModifierAmount = 2;
            Assert.IsFalse(act1.Equals(act2));
            act2.TimeModifierAmount = num_tmp_holder;

            tmp_holder = act2.TimeModifierAmountDisplay;
            act2.TimeModifierAmountDisplay = "today";
            Assert.IsFalse(act1.Equals(act2));
            act2.TimeModifierAmountDisplay = tmp_holder;

            Assert.IsTrue(act1.Equals(act2));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeActivity_Equality")]
        public void DsfDotNetDateTimeActivity_NotEqual()
        {
            //------------Setup for test--------------------------
            var act1 = new DsfDotNetDateTimeActivity
            {
                DateTime = "",
                InputFormat = "",
                OutputFormat = "",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = "[[dt]]",
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            var act2 = new DsfDotNetDateTimeActivity
            {
                DateTime = "",
                InputFormat = "",
                OutputFormat = "dd/MM/yyyy",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = "[[dt]]",
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(act1.Equals(act2));
        }
    }
}
