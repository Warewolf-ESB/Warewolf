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
using Dev2.Interfaces;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;
using Dev2.Activities.DateAndTime;

namespace ActivityUnitTests.ActivityTests

{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    
    public class DotNetDateTimeDifferenceTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Positive Test Cases

        [TestMethod]
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
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeDifferenceActivity_Equality")]
        public void DsfDateTimeDifferenceActivity_Expect_NotEqual()
        {
            var activity1 = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "[[recset1(*).f1]]",
                Input2 = "[[recset2(*).f2]]",
                InputFormat = "dd/MM/yyyy",
                OutputType = "Days",
                Result = "[[resCol(*).res]]"
            };
            var activity2 = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "[[recset2(*).f1]]",
                Input2 = "[[recset1(*).f2]]",
                InputFormat = "MM/dd/yyyy",
                OutputType = "Months",
                Result = "[[resCol1(*).res]]"
            };
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfDateTimeDifferenceActivity_Equality")]
        public void DsfDateTimeDifferenceActivity_Expect_Equal()
        {
            var activity1 = new DsfDotNetDateTimeDifferenceActivity {
                Input1 = "[[recset1(*).f1]]",
                Input2 = "[[recset2(*).f2]]",
                InputFormat = "dd/MM/yyyy",
                OutputType = "Days",
                Result = "[[resCol(*).res]]"
            };
            var activity2 = new DsfDotNetDateTimeDifferenceActivity
            {
                Input1 = "[[recset1(*).f1]]",
                Input2 = "[[recset2(*).f2]]",
                InputFormat = "dd/MM/yyyy",
                OutputType = "Days",
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
