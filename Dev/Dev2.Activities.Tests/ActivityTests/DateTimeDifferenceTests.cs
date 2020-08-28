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
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    [TestClass]
    public class DateTimeDifferenceTests : BaseActivityUnitTest
    {
        [TestInitialize]
        public void PreConditions()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-ZA");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-ZA");

            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            Assert.AreEqual("en-ZA", System.Threading.Thread.CurrentThread.CurrentUICulture.Name);
        }

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
                         , "yyyy/mm/dd 12h:min:ss am/pm"
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
                         , "dd/mm/yyyy"
                         , "Days"
                         , "[[resCol(*).res]]"
                         );

            var result = ExecuteProcess();
            GetRecordSetFieldValueFromDataList(result.Environment, "resCol", "res", out IList<string> results, out string error);

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
                            , DateTime.Now.ToString(CultureInfo.InvariantCulture)
                            , DateTime.Now.AddDays(209).ToString(CultureInfo.InvariantCulture)
                            , ""
                            , "Days"
                            , "[[Result]]"
                            );
            var result = ExecuteProcess();
            const string expected = "209";
            GetScalarValueFromEnvironment(result.Environment, "Result", out string actual, out string error);

            Assert.AreEqual(expected, actual,error);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("DateTimeDifferenceUnitTest")]
        [Owner("Massimo Guerrera")]
        public void DateTimeDifference_DateTimeDifferenceUnitTest_ExecuteWithBlankInput_DateTimeNowIsUsed()
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         , ""
                         , ""
                         , "Seconds"
                         , "[[MyTestResult]]");

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "MyTestResult", out string actual, out string error);

            Assert.AreEqual("0", actual);
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
            var act = new DsfDateTimeDifferenceActivity { Input1 = "", Input2 = "", InputFormat = "", OutputType = "", Result = "[[dtd]]" };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[dtd]]", outputs[0]);
        }

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string input1, string input2, string inputFormat, string outputType, string result)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDateTimeDifferenceActivity { Input1 = input1, Input2 = input2, InputFormat = inputFormat, OutputType = outputType, Result = result }
            };
            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
