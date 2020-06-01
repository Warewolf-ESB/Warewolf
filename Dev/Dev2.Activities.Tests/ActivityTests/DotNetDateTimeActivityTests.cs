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
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using System.Diagnostics;
using Dev2.Activities.DateAndTime;
using Moq;
using Warewolf.Storage;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.State;
using System.Linq;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    public class DotNetDateTimeActivityTests : BaseActivityUnitTest
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

        #region DateTime Tests

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
                TimeModifierAmount = 0,
                Result = varName,
                TimeModifierAmountDisplay = 0.ToString(CultureInfo.InvariantCulture)
            };
            var dataMock = new Mock<IDSFDataObject>();
            dataMock.Setup(o => o.IsDebugMode()).Returns(() => true);
            var executionEnvironment = new ExecutionEnvironment();
            dataMock.Setup(o => o.Environment).Returns(executionEnvironment);
            var data = dataMock.Object;

            //------------Execute Test---------------------------
            act.Execute(data, 0);
            //------------Assert Results-------------------------
            var debugout = act.GetDebugOutputs(executionEnvironment, 0);
            var value = executionEnvironment.EvalAsListOfStrings(varName, 0);
            Assert.AreEqual(1,value.Count);
            bool canParseDateTime = DateTime.TryParse(value[0], new DateTimeFormatInfo() { ShortDatePattern = "dd/MM/yyyy", LongTimePattern = "hh:mm:ss.tt", }, DateTimeStyles.None, out DateTime datetimeResult);
            if (!canParseDateTime)
            {
                canParseDateTime = DateTime.TryParse(value[0], new DateTimeFormatInfo() { ShortDatePattern = "MM/dd/yyyy", LongTimePattern = "hh:mm:ss.tt", }, DateTimeStyles.None, out datetimeResult);
            }
            Assert.IsTrue(canParseDateTime, $"Failed to parse value: {value[0]} as a DateTime");
            Assert.AreEqual(false, debugout[0].ResultsList[0].HasError);
            Assert.AreEqual(varName, debugout[0].ResultsList[0].Variable);
            Assert.AreEqual(DebugItemResultType.Variable, debugout[0].ResultsList[0].Type);
        }


        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeActivity_Equality")]
        public void DsfDotNetDateTimeActivity_Equal()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid().ToString();
            var act1 = new DsfDotNetDateTimeActivity
            {
                DateTime = "",
                InputFormat = "",
                OutputFormat = "",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = "[[dt]]",
                UniqueID = id,
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
                UniqueID = id,
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
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeActivity_Equality")]
        public void DsfDotNetDateTimeActivity_NotEqual()
        {
            //------------Setup for test--------------------------
            var id = Guid.NewGuid().ToString();
            var act1 = new DsfDotNetDateTimeActivity
            {
                DateTime = "",
                InputFormat = "",
                OutputFormat = "",
                TimeModifierType = "",
                TimeModifierAmount = 1,
                Result = "[[dt]]",
                UniqueID = id,
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
                UniqueID = id,
                TimeModifierAmountDisplay = 1.ToString(CultureInfo.InvariantCulture)
            };
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(act1.Equals(act2));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfDotNetDateTimeActivity_GetState")]
        public void DsfDotNetDateTimeActivity_GetState_ReturnsStateVariable()
        {
            //------------Setup for test--------------------------
            var calculateActivity = new DsfDotNetDateTimeActivity
            {
                DateTime = "2018/07/20",
                InputFormat = "yyyy/MM/dd",
                OutputFormat = "yyyy/MM/dd",
                TimeModifierType = "days",
                TimeModifierAmountDisplay = "days",
                TimeModifierAmount = 1,
                Result = "TimeChanged"
            };
            //------------Execute Test---------------------------
            var stateItems = calculateActivity.GetState();
            Assert.AreEqual(7, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name="DateTime",
                    Type = StateVariable.StateType.Input,
                    Value = "2018/07/20"
                },
                new StateVariable
                {
                    Name="InputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = "yyyy/MM/dd"
                },
                new StateVariable
                {
                    Name="OutputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = "yyyy/MM/dd"
                },
                new StateVariable
                {
                    Name="TimeModifierType",
                    Type = StateVariable.StateType.Input,
                    Value = "days"
                },
                new StateVariable
                {
                    Name="TimeModifierAmountDisplay",
                    Type = StateVariable.StateType.Input,
                    Value = "days"
                },
                new StateVariable
                {
                    Name="TimeModifierAmount",
                    Type = StateVariable.StateType.Input,
                    Value = "1"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "TimeChanged"
                }
            };

            var iter = calculateActivity.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }
    }
}
