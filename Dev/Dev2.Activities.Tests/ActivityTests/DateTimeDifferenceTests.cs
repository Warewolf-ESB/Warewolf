
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Dev2;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable CheckNamespace
namespace ActivityUnitTests.ActivityTests
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Summary description for DateTimeDifferenceTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class DateTimeDifferenceTests : BaseActivityUnitTest
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
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            const string expected = "209";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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
                         , "dd/mm/yyyy"
                         , "Days"
                         , "[[resCol(*).res]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            string error;
            IList<IBinaryDataListItem> results;
            GetRecordSetFieldValueFromDataList(result.DataListID, "resCol", "res", out results, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual("8847", results[0].TheValue);
            Assert.AreEqual("9477", results[1].TheValue);
            Assert.AreEqual("9090", results[2].TheValue);
        }

        //2013.03.11: Ashley Lewis - PBI 9167 Moved to positive tests
        [TestMethod]
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
            IDSFDataObject result = ExecuteProcess();
            const string expected = "209";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "Result", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("DateTimeDifferenceUnitTest")]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void DateTimeDifference_DateTimeDifferenceUnitTest_ExecuteWithBlankInput_DateTimeNowIsUsed()
        // ReSharper restore InconsistentNaming
        {
            const string currDL = @"<root><MyTestResult></MyTestResult></root>";
            SetupArguments(currDL
                         , currDL
                         , ""
                         , ""
                         , ""
                         , "Seconds"
                         , "[[MyTestResult]]");

            IDSFDataObject result = ExecuteProcess();

            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "MyTestResult", out actual, out error);

            Assert.AreEqual("0", actual);
        }

        #endregion Positive Test Cases

        #region Error Test Cases

        [TestMethod]
        public void Input1_Not_Matching_InputFormat_Expected_Error()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/mm/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void Input2_Not_Matching_InputFormat_Expected_Error()
        {

            SetupArguments(
                            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                          , ActivityStrings.DateTimeDiff_DataListShape
                          , "2012/03/05 09:20:30 AM"
                          , "2012 07:15:50 AM"
                          , "yyyy/mm/dd 12h:min:ss am/pm"
                          , "Days"
                          , "[[Result]]"
                          );

            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            Assert.IsTrue(res);
        }

        [TestMethod]
        public void Invalid_InputFormat_Expected_Error()
        {
            SetupArguments(
                           "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                         , ActivityStrings.DateTimeDiff_DataListShape
                         , "2012/03/05 09:20:30 AM"
                         , "2012/10/01 07:15:50 AM"
                         , "yyyy/wrongFromat/dd 12h:min:ss am/pm"
                         , "Days"
                         , "[[Result]]"
                         );
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void ErrorHandeling_Expected_ErrorTags()
        {
            SetupArguments(
                            "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                          , ActivityStrings.DateTimeDiff_DataListShape
                          , "2012/10/01 07:15:50 AM"
                          , "2012/10/01 07:15:50 AM"
                          , "yyyy/mm/dd 12h:min:ss am/pm"
                          , "Days"
                          , "[[//().rec]]"
                          );

            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        #endregion Error Test Cases

        #region Get Input/Output Tests

        [TestMethod]
        public void DateTimeDifference_GetInputs_Expected_Four_Input()
        {
            DsfDateTimeDifferenceActivity testAct = new DsfDateTimeDifferenceActivity { Input1 = "27-10-2012", Input2 = "28-10-2012", InputFormat = "dd-mm-yyyy", OutputType = "Years", Result = "[[result]]" };

            IBinaryDataList inputs = testAct.GetInputs();

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(4, inputs.FetchAllEntries().Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_ExpectError()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/[[03]]/05 09:20:30 AM";
            const string input2 = "2012/10/01 07:15:50 AM";
            AssertError(input1, input2);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_InvalidScalar_ExpectError()
        {
            //------------Setup for test--------------------------
            const string input1 = "[[2012/[[03]]/05 09:20:30 AM]]";
            const string input2 = "2012/10/01 07:15:50 AM";
            AssertError(input1, input2);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_Input2_InvalidScalar_ExpectError()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/10/01 07:15:50 AM";
            const string input2 = "2012/[[10]]/01 07:15:50 AM";
            AssertError(input1, input2);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_Input2_InvalidEncompassingScalar_ExpectError()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/10/01 07:15:50 AM";
            const string input2 = "[[2012/10/01 07:15:50 AM]]";
            AssertError(input1, input2);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_Input2_InvalidEncompassingRecSet_ExpectError()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/10/01 07:15:50 AM";
            const string input2 = "[[1().a]]";
            AssertError(input1, input2);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_Input2_InvalidEncompassingScalar_Success()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/10/01 07:15:50 AM";
            const string input2 = "[[a]]";
            AssertError(input1, input2,false);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_Input2_InvalidEncompassingRecSet_Success()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/10/01 07:15:50 AM";
            const string input2 = "[[rec().a]]";
            AssertError(input1, input2,false);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_Input2_PartialRecSet_Success()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/10/01 07:15:50 AM";
            const string input2 = "2012/[[rec().a]]/01";
            AssertError(input1, input2, false);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_LanguageInputs_Input2_PartialScalar_Success()
        {
            //------------Setup for test--------------------------
            const string input1 = "2012/10/01 07:15:50 AM";
            const string input2 = "2012/[[a]]/01";
            AssertError(input1, input2, false);
        }
// ReSharper disable UnusedParameter.Local
        void AssertError(string input1, string input2, bool expectError = true)
// ReSharper restore UnusedParameter.Local
        {
            SetupArguments(
                "<root>" + ActivityStrings.DateTimeDiff_DataListShape + "</root>"
                , ActivityStrings.DateTimeDiff_DataListShape
                , input1
                , input2
                , "yyyy/mm/dd 12h:min:ss am/pm"
                , "Days"
                , "[[Result]]"
                );

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.FetchErrors(result.DataListID);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            //------------Assert Results-------------------------
            DataListRemoval(result.DataListID);

            if(expectError)
                Assert.IsTrue(res.Contains("begins with a number"));
            else
            {
                Assert.IsTrue(!res.Contains("begins with a number"));
            }
        }

        [TestMethod]
        public void DateTimeDifference_GetOutputs_Expected_One_Output()
        {
            DsfDateTimeDifferenceActivity testAct = new DsfDateTimeDifferenceActivity { Input1 = "27-10-2012", Input2 = "28-10-2012", InputFormat = "dd-mm-yyyy", OutputType = "Years", Result = "[[result]]" };

            IBinaryDataList outputs = testAct.GetOutputs();

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, outputs.FetchAllEntries().Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDateTimeDifference_Execute")]
        public void DsfDateTimeDifference_Execute_MultipleResults_ExpectErrors()
        {
            SetupArguments(
                           ActivityStrings.DateTimeDifferenceDataListWithData
                         , ActivityStrings.DateTimeDifferenceDataListShape
                         , "[[recset1(*).f1]]"
                         , "[[recset2(*).f2]]"
                         , "dd/mm/yyyy"
                         , "Days"
                         , "[[resCol(*).res]][[g]]"
                         );

            IDSFDataObject result = ExecuteProcess();
            string error;
            IList<IBinaryDataListItem> results;
            GetRecordSetFieldValueFromDataList(result.DataListID, "resCol", "res", out results, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #endregion Get Input/Output Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string input1, string input2, string inputFormat, string outputType, string result)
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
