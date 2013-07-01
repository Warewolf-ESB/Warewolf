using Dev2;
using Dev2.Activities;
using Dev2.Common.Enums;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for AssignActivity
    /// </summary>
    [TestClass]
    public class ForEachActivityTest : BaseActivityUnitTest
    {
        private static readonly IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        public ForEachActivityTest()
            : base()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Number Of Execution Tests

        [TestMethod]
        public void NumberOfExecutionsWithNullParamsExpectedTotalExecutions0()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.NumOfExecution
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(0));
        }

        [TestMethod]
        public void NumberOfExecutionsWithNumericExpectedTotalExecutions2()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.NumOfExecution
                          , false
                          , null
                          , null
                          , null
                          , null
                          , "2"
                          );
            IDSFDataObject result;
             Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(10));
        }

        [TestMethod]
        public void NumberOfExecutionsWithNegativeNumberExpectedTotalExecutions0()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.NumOfExecution
                          , false
                          , null
                          , null
                          , null
                          , null
                          , "-2"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(0));
        }

        [TestMethod]
        public void NumberOfExecutionsWithRangeExpectedTotalExecutions5()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , null
                          , "5"
                          , "9"
                          );

            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(5));
        }

        [TestMethod]
        public void NumberOfExecutionsWithReversedRangeExpectedTotalExecutions5()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , null
                          , "9"
                          , "5"
                          );

            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(5));
        }

        [TestMethod]
        public void NumberOfExecutionsWithNegitiveNumberAsFromExpectedTotalExecutions15()
        {
            SetupArguments(
                             ActivityStrings.ForEachCurrentDataList
                           , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , null
                          , "-5"
                          , "9"
                           );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(0));
        }

        [TestMethod]
        public void NumberOfExecutionsWithNegitiveNumberAsToExpectedTotalExecutions0()
        {
            SetupArguments(
                             ActivityStrings.ForEachCurrentDataList
                           , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , null
                          , "5"
                          , "-9"
                           );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(0));
        }

        [TestMethod]
        public void NumberOfExecutionsWithCsvExpectedTotalExecutions2()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InCSV
                          , false
                          , null
                          , null
                          , null
                          , "6,9,"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(2));
        }

        [TestMethod]
        public void NumberOfExecutionsWithCsvExpectedTotalExecutions4()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InCSV
                          , false
                          , null
                          , null
                          , null
                          , "6,9,9,6"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(4));
        }

        [TestMethod]
        public void NumberOfExecutionsWithReverseOrderCsvExpectedTotalExecutions3()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InCSV
                          , false
                          , null
                          , null
                          , null
                          , "9,5,1,"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(3));
        }

        #endregion Number Of Execution Tests

        #region Output Mapping Tests

        [TestMethod]
        public void OutputMappingWithRecordSetWithNoIndexExpectedAllRecordSetValsMappedCorrectly()
        {
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , null
                          , "1"
                          , "1"
                          , null
                          , null
                          );


            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            List<string> expected = new List<string> { "recVal1"
                                                     , "recVal2"
                                                     , "recVal3"
                                                     , "recVal4"
                                                     , "recVal5"
                                                     , ""
                                                     };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "rec", out error);
            ErrorResultTO errors = new ErrorResultTO();
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(1));
            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());

        }

        [TestMethod]
        public void OutputMappingWithRecordSetwithStarExpectedOutputMappedToRecordSet()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(*).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , outputMapping
                          , "1"
                          , "1"
                          , null
                          , null
                          );


            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            List<string> expected = new List<string> { "recVal1", "recVal2", "recVal3", "recVal4", "recVal5" , "" };
            string error = string.Empty;
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "rec", out error);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(1));

            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
        }

        [TestMethod]
        public void OutputMappingWithRecordSetMappedToScalarExpectedForEachValueMappedForAllExecutions()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[var]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , outputMapping
                          , "1"
                          , "1"
                          , null
                          , null
                          );


            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            string expected = "recVal1";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "resultVar", out actual, out error);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(1));
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void OutputMappingWithRecordsetWithAnIndexExpectedForEachValuePassedInForAllExecutions()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(3).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.InRange
                          , false
                          , outputMapping
                          , "1"
                          , "1"
                          , null
                          , null
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);

            string expected = "recVal1";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "resultVar", out actual, out error);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteTransactionallyScopedRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), out errors), Times.Exactly(1));
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void ForEachWithNumericAndWrongInputExpectedExceptionInputStringNotInRightFormat()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(3).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.NumOfExecution
                          , false
                          , outputMapping
                          , null
                          , null
                          , null
                          , "[[1]]"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            Assert.IsTrue(Compiler.HasErrors(result.DataListID), "Numeric for each with malformed input did not throw an error");
        }

        [TestMethod]
        public void ForEachWithRangeAndWrongFromExpectedExceptionInputStringNotInRightFormat()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(3).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.NumOfExecution
                          , false
                          , outputMapping
                          , "[[5]]"
                          , "9"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            Assert.IsTrue(Compiler.HasErrors(result.DataListID), "Range type for each with malformed 'from' parameter did not throw an error");
        }

        [TestMethod]
        public void ForEachWithRangeAndWrongToExpectedExceptionInputStringNotInRightFormat()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(3).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.NumOfExecution
                          , false
                          , outputMapping
                          , "5"
                          , "[[9]]"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            Assert.IsTrue(Compiler.HasErrors(result.DataListID), "Range type for each with malformed 'to' parameter did not throw an error");
        }

        [TestMethod]
        public void ForEachWithCsvAndWrongInputExpectedExceptionInputStringNotInRightFormat()
        {
            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]", "[[recset(3).rec2]]");
            SetupArguments(
                            ActivityStrings.ForEachCurrentDataList
                          , ActivityStrings.ForEachDataListShape
                          , enForEachType.NumOfExecution
                          , false
                          , outputMapping
                          , null
                          , null
                          , "3, [[7]], 9,"
                          );
            IDSFDataObject result;
            Mock<IEsbChannel> coms = ExecuteForEachProcess(out result);
            Assert.IsTrue(Compiler.HasErrors(result.DataListID), "For each in csv with malformed csv did not throw an error");
        }

        #endregion Output Mapping Tests

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104
        /// Upgraded : Ashley Lewis Bug 9365
        /// </summary>
        [TestMethod]
        public void GetDebugInputOutputWithScalarsExpectedPass()
        {
            //Used recordset with a numeric index as a scalar because it the only place were i had numeric values and it evalues to a scalar 
            var act = new DsfForEachActivity { ForEachType = enForEachType.NumOfExecution, NumOfExections = "[[Numeric(1).num]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1,inRes.Count);
            Assert.AreEqual(5, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("No. of Executes", inRes[0].FetchResultsList()[0].Value, "Incorrect input mapping");
            Assert.AreEqual("Number", inRes[0].FetchResultsList()[1].Value, "Incorrect input mapping");
            Assert.AreEqual("[[Numeric(1).num]]", inRes[0].FetchResultsList()[2].Value, "Incorrect input mapping");
            Assert.AreEqual("=", inRes[0].FetchResultsList()[3].Value, "Incorrect input mapping");
            Assert.AreEqual("654", inRes[0].FetchResultsList()[4].Value, "Incorrect input mapping");

            Assert.AreEqual(0, outRes.Count);
        }

        [TestMethod]
        public void GetDebugInputOutputWithScalarCsvsExpectedPass()
        {
            //Used recordset with a numeric index as a scalar because it the only place were i had numeric values and it evalues to a scalar 
            var act = new DsfForEachActivity { ForEachType = enForEachType.InCSV, CsvIndexes = "[[Numeric(4).num]], [[Numeric(6).num]], [[Numeric(10).num]]," };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1,inRes.Count);
            Assert.AreEqual(5, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("* in CSV", inRes[0].FetchResultsList()[0].Value, "Incorrect input mapping");
            Assert.AreEqual("Csv Indexes", inRes[0].FetchResultsList()[1].Value, "Incorrect input mapping");
            Assert.AreEqual("[[Numeric(4).num]], [[Numeric(6).num]], [[Numeric(10).num]],", inRes[0].FetchResultsList()[2].Value, "Incorrect input mapping");
            Assert.AreEqual("=", inRes[0].FetchResultsList()[3].Value, "Incorrect input mapping");
            Assert.AreEqual("21, 1, 110,", inRes[0].FetchResultsList()[4].Value, "Incorrect input mapping");

            Assert.AreEqual(0, outRes.Count);
        }

        [TestMethod]
        public void GetDebugInputOutputWithScalarRangeExpectedPass()
        {
            //Used recordset with a numeric index as a scalar because it the only place were i had numeric values and it evalues to a scalar 
            var act = new DsfForEachActivity { ForEachType = enForEachType.InRange, From = "[[Numeric(6).num]]", To = "[[Numeric(4).num]]" };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1,inRes.Count);
            Assert.AreEqual(9, inRes[0].FetchResultsList().Count);
            Assert.AreEqual("* in Range", inRes[0].FetchResultsList()[0].Value, "Incorrect input mapping");
            Assert.AreEqual("From", inRes[0].FetchResultsList()[1].Value, "Incorrect input mapping");
            Assert.AreEqual("[[Numeric(6).num]]", inRes[0].FetchResultsList()[2].Value, "Incorrect input mapping");
            Assert.AreEqual("=", inRes[0].FetchResultsList()[3].Value, "Incorrect input mapping");
            Assert.AreEqual("1", inRes[0].FetchResultsList()[4].Value, "Incorrect input mapping");
            Assert.AreEqual("To", inRes[0].FetchResultsList()[5].Value, "Incorrect input mapping");
            Assert.AreEqual("[[Numeric(4).num]]", inRes[0].FetchResultsList()[6].Value, "Incorrect input mapping");
            Assert.AreEqual("=", inRes[0].FetchResultsList()[7].Value, "Incorrect input mapping");
            Assert.AreEqual("21", inRes[0].FetchResultsList()[8].Value, "Incorrect input mapping");

            Assert.AreEqual(0, outRes.Count);
        }

        #endregion

        #region Private Test Methods

        private DsfActivity CreateWorkflow()
        {
            DsfActivity activity = new DsfActivity();
            activity.ServiceName = "MyTestService";
            activity.InputMapping = ActivityStrings.ForEach_Input_Mapping;
            activity.OutputMapping = ActivityStrings.ForEach_Output_Mapping;

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private DsfActivity CreateWorkflow(string mapping, bool isInputMapping)
        {
            DsfActivity activity = new DsfActivity();
            if (isInputMapping)
            {
                activity.InputMapping = mapping;
                activity.OutputMapping = ActivityStrings.ForEach_Output_Mapping;
            }
            else
            {
                activity.InputMapping = ActivityStrings.ForEach_Input_Mapping;
                activity.OutputMapping = mapping;
            }
            activity.ServiceName = "MyTestService";

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        //private DsfActivity CreateWorkflowWithAssign()
        //{
        //    DsfMultiAssignActivity activity = new DsfMultiAssignActivity();
        //    //if (isInputMapping)
        //    //{
        //    //    activity.InputMapping = mapping;
        //    //    activity.OutputMapping = ActivityStrings.ForEach_Output_Mapping;
        //    //}
        //    //else
        //    //{
        //    //    activity.InputMapping = ActivityStrings.ForEach_Input_Mapping;
        //    //    activity.OutputMapping = mapping;
        //    //}
        //    //activity.ServiceName = "MyTestService";

        //    TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

        //    return activity;
        //}

        private void SetupArguments(string currentDL, string testData, enForEachType type, bool isInputMapping = false, string inputMapping = null, string from = null, string to = null, string csvIndexes = null, string numberExecutions = null)
        {
            var activityFunction = new ActivityFunc<string, bool>();
            DsfActivity activity;
            if (inputMapping != null)
            {
                activity = CreateWorkflow(inputMapping, isInputMapping);
            }
            else
            {
                activity = CreateWorkflow();
            }

            activityFunction.Handler = activity;

            TestStartNode = new FlowStep
            {
                Action = new DsfForEachActivity
                {
                    DataFunc = activityFunction,
                    ForEachType = type,
                    NumOfExections = numberExecutions,
                    From = from,
                    To = to,
                    CsvIndexes = csvIndexes,
                }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
