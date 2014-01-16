using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for AssignActivity
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ForEachActivityTest : BaseActivityUnitTest
    {
        private new static readonly IDataListCompiler Compiler = DataListFactory.CreateDataListCompiler();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(0));
        }

        [TestMethod]
        [TestCategory("ForEach,IterativeExecution,UnitTest")]
        [Description("Test to ensure we do not regress on the iterative execution of sub-services in a workflow")]
        [Owner("Trav")]
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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(2));
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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(0));
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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(5));
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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(5));
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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(0));

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
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(0));
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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(2));
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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(4));

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
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors), Times.Exactly(3));

        }

        #endregion Number Of Execution Tests

        #region Output Mapping Tests

        [TestMethod]
        public void ForEachWithNumericAndWrongInputExpectedExceptionInputStringNotInRightFormat()
        {

            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]",
                                                                                  "[[recset(3).rec2]]");
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
            ExecuteForEachProcess(out result);
            var res = Compiler.HasErrors(result.DataListID);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.IsTrue(res, "Numeric for each with malformed input did not throw an error");

        }

        [TestMethod]
        public void ForEachWithRangeAndWrongFromExpectedExceptionInputStringNotInRightFormat()
        {

            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]",
                                                                                  "[[recset(3).rec2]]");
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
            ExecuteForEachProcess(out result);
            // remove test datalist ;)
            var res = Compiler.HasErrors(result.DataListID);
            DataListRemoval(result.DataListID);
            Assert.IsTrue(res, "Range type for each with malformed 'from' parameter did not throw an error");

        }

        [TestMethod]
        public void ForEachWithRangeAndWrongToExpectedExceptionInputStringNotInRightFormat()
        {

            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]",
                                                                                  "[[recset(3).rec2]]");
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
            ExecuteForEachProcess(out result);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            Assert.IsTrue(Compiler.HasErrors(result.DataListID), "Range type for each with malformed 'to' parameter did not throw an error");

        }

        [TestMethod]
        public void ForEachWithCsvAndWrongInputExpectedExceptionInputStringNotInRightFormat()
        {

            string outputMapping = ActivityStrings.ForEach_Output_Mapping.Replace("[[recset().rec2]]",
                                                                                  "[[recset(3).rec2]]");
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
            ExecuteForEachProcess(out result);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
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

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
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

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
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

            var result = CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape, ActivityStrings.DebugDataListWithData, out inRes, out outRes);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, inRes.Count);
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
            DsfActivity activity = new DsfActivity
                {
                    ServiceName = "MyTestService",
                    InputMapping = ActivityStrings.ForEach_Input_Mapping,
                    OutputMapping = ActivityStrings.ForEach_Output_Mapping
                };

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private DsfActivity CreateWorkflow(string mapping, bool isInputMapping)
        {
            DsfActivity activity = new DsfActivity();
            if(isInputMapping)
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

        private void SetupArguments(string currentDL, string testData, enForEachType type, bool isInputMapping = false, string inputMapping = null, string from = null, string to = null, string csvIndexes = null, string numberExecutions = null)
        {
            var activityFunction = new ActivityFunc<string, bool>();
            DsfActivity activity;
            if(inputMapping != null)
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
