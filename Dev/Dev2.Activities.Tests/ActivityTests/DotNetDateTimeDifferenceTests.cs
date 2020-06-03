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
using Dev2.Interfaces;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;
using Dev2.Activities.DateAndTime;
using System.Linq;
using Dev2.Common.State;

namespace ActivityUnitTests.ActivityTests

{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]

    public class DotNetDateTimeDifferenceTests : BaseActivityUnitTest
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

        #region Positive Test Cases

        [TestMethod]
        [Timeout(60000)]
        public void Positive_With_Normal_Params_Expected_Positive()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012/03/05 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/MM/dd hh:mm:ss tt"
                         , "Days"
                         , "[[Result]]"
                         );

            var result = ExecuteProcess();
            const string expected = "209";
            GetScalarValueFromEnvironment(result.Environment, "Result", out string actual, out string error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void Positive_UsingRecorsetWithStar_Expected_Positive()
        {
            SetupArguments(
                           ActivityStrings.DateTimeDifferenceDataListWithData
                         , ActivityStrings.DateTimeDifferenceDataListShape
                         , "[[recset1(*).f1]]"
                         , "[[recset2(*).f2]]"
                         , "dd/MM/yyyy"
                         , "Days"
                         , "[[resCol(*).res]]"
                         );

            var result = ExecuteProcess();
            GetRecordSetFieldValueFromDataList(result.Environment, "resCol", "res", out IList<string> results, out string error);
            // remove test datalist ;)

            Assert.AreEqual("8847", results[0]);
            Assert.AreEqual("9477", results[1]);
            Assert.AreEqual("9090", results[2]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void Blank_InputFormat_Expected_Error()
        {
            SetupArguments(
                              "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                            , ActivityStrings.DateTimeDiff_DataListShape
                            , DateTime.Now.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat)
                            , DateTime.Now.AddDays(209).ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat)
                            , ""
                            , "Days"
                            , "[[Result]]"
                            );
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "Result", out string actual, out string error);

            // remove test datalist ;)
            Assert.IsNull(actual);
        }
        #endregion Positive Test Cases

        #region Error Test Cases

        #endregion Error Test Cases

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDateTimeDifferenceActivity_GetOutputs")]
        public void DsfDateTimeDifferenceActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            var act = new DsfDotNetDateTimeDifferenceActivity { Input1 = "", Input2 = "", InputFormat = "", OutputType = "", Result = "[[dtd]]" };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[dtd]]", outputs[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeDifferenceActivity_Equality")]
        public void DsfDateTimeDifferenceActivity_Expect_NotEqual()
        {
            var id = Guid.NewGuid().ToString();
            var activity1 = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "[[recset1(*).f1]]",
                Input2 = "[[recset2(*).f2]]",
                InputFormat = "dd/MM/yyyy",
                OutputType = "Days",
                UniqueID = id,
                Result = "[[resCol(*).res]]"
            };
            var activity2 = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "[[recset2(*).f1]]",
                Input2 = "[[recset1(*).f2]]",
                InputFormat = "MM/dd/yyyy",
                OutputType = "Months",
                UniqueID = id,
                Result = "[[resCol1(*).res]]"
            };
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeDifferenceActivity_Equality")]
        public void DsfDateTimeDifferenceActivity_Expect_Equal()
        {
            var id = Guid.NewGuid().ToString();
            var activity1 = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "[[recset1(*).f1]]",
                Input2 = "[[recset2(*).f2]]",
                InputFormat = "dd/MM/yyyy",
                OutputType = "Days",
                UniqueID = id,
                Result = "[[resCol(*).res]]"
            };
            var activity2 = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "[[recset1(*).f1]]",
                Input2 = "[[recset2(*).f2]]",
                InputFormat = "dd/MM/yyyy",
                OutputType = "Days",
                UniqueID = id,
                Result = "[[resCol(*).res]]"
            };
            var activity1_otherref = activity1;
            Assert.IsTrue(activity1.Equals(activity1_otherref));
            Assert.IsTrue(activity1.Equals(activity2));
            string tmp_holder;

            tmp_holder = activity2.Input1;
            activity2.Input1 = "[[notsame(*).f1]]";
            Assert.IsFalse(activity1.Equals(activity2));
            activity2.Input1 = tmp_holder;

            tmp_holder = activity2.Input2;
            activity2.Input2 = "[[notsame(*).f2]]";
            Assert.IsFalse(activity1.Equals(activity2));
            activity2.Input2 = tmp_holder;

            tmp_holder = activity2.InputFormat;
            activity2.InputFormat = "MM/dd/yyyy";
            Assert.IsFalse(activity1.Equals(activity2));
            activity2.InputFormat = tmp_holder;

            tmp_holder = activity2.OutputType;
            activity2.OutputType = "Minutes";
            Assert.IsFalse(activity1.Equals(activity2));
            activity2.OutputType = tmp_holder;

            tmp_holder = activity2.Result;
            activity2.Result = "[[res(*).res]]";
            Assert.IsFalse(activity1.Equals(activity2));
            activity2.Result = tmp_holder;

            Assert.IsTrue(activity1.Equals(activity2));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory("DsfDotNetDateTimeDifferenceActivity_GetState")]
        public void DsfDotNetDateTimeDifferenceActivity_GetState_ReturnsStateVariable()
        {
            //------------Setup for test--------------------------
            var dotNetDateTimeDifferenceActivity = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "date1",
                Input2 = "date2",
                InputFormat = "yyyy/MM/dd",
                OutputType = "Years",
                Result = "DateChanged"
            };
            //------------Execute Test---------------------------
            var stateItems = dotNetDateTimeDifferenceActivity.GetState();
            Assert.AreEqual(5, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Input1",
                    Type = StateVariable.StateType.Input,
                    Value = "date1"
                },
                new StateVariable
                {
                    Name = "Input2",
                    Type = StateVariable.StateType.Input,
                    Value = "date2"
                },
                new StateVariable
                {
                    Name="InputFormat",
                    Type = StateVariable.StateType.Input,
                    Value = "yyyy/MM/dd"
                },
                new StateVariable
                {
                    Name="OutputType",
                    Type = StateVariable.StateType.Input,
                    Value = "Years"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = "DateChanged"
                }
            };

            var iter = dotNetDateTimeDifferenceActivity.GetState().Select(
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

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string input1, string input2, string inputFormat, string outputType, string result)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDotNetDateTimeDifferenceActivity { Input1 = input1, Input2 = input2, InputFormat = inputFormat, OutputType = outputType, Result = result }
            };
            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}