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

            IDSFDataObject result = ExecuteProcess();
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

            IDSFDataObject result = ExecuteProcess();
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
            IDSFDataObject result = ExecuteProcess();
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

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string input1, string input2, string inputFormat, string outputType, string result)
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
