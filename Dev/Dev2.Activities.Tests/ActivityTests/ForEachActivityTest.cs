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
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ActivityUnitTests;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for AssignActivity
    /// </summary>
    [TestClass]
    public class ForEachActivityTest : BaseActivityUnitTest
    {



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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(0));
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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(2));
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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(0));
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

            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(5));
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

            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(5));
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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)

            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(0));

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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(0));
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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(2));
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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(4));

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
            var coms = ExecuteForEachProcess(out IDSFDataObject result);
            ErrorResultTO errors;
            // remove test datalist ;)
            coms.Verify(c => c.ExecuteSubRequest(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), out errors, It.IsAny<int>(), false), Times.Exactly(3));

        }

        #endregion Number Of Execution Tests

        #region Output Mapping Tests

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfForEach_UpdateDebugParentID")]
        
        public void DsfForEach_UpdateDebugParentID_UniqueIdNotSameIfNestingLevelNotChanged()

        {
            var dataObject = new DsfDataObject(CurrentDl, Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = true,
            };

            var act = new DsfForEachActivity();
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreNotEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfForEach_UpdateDebugParentID")]
        
        public void DsfForEach_UpdateDebugParentID_UniqueIdNotSameIfNestingLevelIncreased()

        {
            var dataObject = new DsfDataObject(CurrentDl, Guid.NewGuid())
            {
                // NOTE: WorkflowApplicationFactory.InvokeWorkflowImpl() will use HostSecurityProvider.Instance.ServerID 
                //       if this is NOT provided which will cause the tests to fail!
                ServerID = Guid.NewGuid(),
                IsDebug = true,
                ForEachNestingLevel = 1
            };

            var act = new DsfForEachActivity();
            var originalGuid = Guid.NewGuid();
            act.UniqueID = originalGuid.ToString();
            act.UpdateDebugParentID(dataObject);
            Assert.AreNotEqual(originalGuid.ToString(), act.UniqueID);
            Assert.AreEqual(act.GetWorkSurfaceMappingId(), originalGuid);


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfForEach_UpdateDebugParentID")]
        
        public void DsfForEach_Execute_IncrementsAndChangesId_IdNotChangedIfNestingLevelIsZero()

        {
            var id = SetupArguments(
                             ActivityStrings.ForEachCurrentDataList
                           , ActivityStrings.ForEachDataListShape
                           , enForEachType.InCSV
                           , false
                           , null
                           , null
                           , null
                           , "9,5,1,"
                           );
            var x = id.UniqueID;
            ExecuteForEachProcess(out IDSFDataObject result, true, -1);
            Assert.AreEqual(x, id.UniqueID);
            // remove test datalist ;)
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DsfForEachActivity_Execute")]
        public void DsfForEachActivity_Execute_ServiceTestExecution_WithNoUniqueId()
        {
            var uniqueId = Guid.NewGuid();

            var env = new ExecutionEnvironment();
            var data = new Mock<IDSFDataObject>();
            data.Setup(o => o.Environment).Returns(() => env);
            data.Setup(o => o.IsServiceTestExecution).Returns(() => true);

            var serviceTestMock = new Mock<IServiceTestModelTO>();
            var step = new ServiceTestStepTO();
            var step_child = new ServiceTestStepTO {
                ActivityID = uniqueId
            };
            step.Children = new ObservableCollection<IServiceTestStep>() { step_child };
            var testSteps = new List<IServiceTestStep>
            {
                step
            };
            serviceTestMock.Setup(o => o.TestSteps).Returns(() => testSteps);
            var serviceTest = serviceTestMock.Object;
            data.Setup(o => o.ServiceTest).Returns(() => serviceTest);
            var ob = new DsfForEachActivity
            {
                UniqueID = uniqueId.ToString()
            };
            ob.Execute(data.Object, 0);

            Assert.AreEqual("Passed", step_child.Result.Message);
            Assert.AreEqual(RunResult.TestPassed, step_child.Result.RunTestResult);
        }

        #endregion Output Mapping Tests

        #region Private Test Methods

        DsfActivity CreateWorkflow()
        {
            var activity = new DsfActivity
            {
                ServiceName = "MyTestService",
                InputMapping = ActivityStrings.ForEach_Input_Mapping,
                OutputMapping = ActivityStrings.ForEach_Output_Mapping

            };

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        DsfActivity CreateWorkflow(string mapping, bool isInputMapping)
        {
            var activity = new DsfActivity();
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

        DsfForEachActivity SetupArguments(string currentDl, string testData, enForEachType type, bool isInputMapping = false, string inputMapping = null, string from = null, string to = null, string csvIndexes = null, string numberExecutions = null)
        {
            var activityFunction = new ActivityFunc<string, bool>();
            var activity = inputMapping != null ? CreateWorkflow(inputMapping, isInputMapping) : CreateWorkflow();

            activityFunction.Handler = activity;
            var id = Guid.NewGuid().ToString();
            var dsfForEachActivity = new DsfForEachActivity
            {
                DataFunc = activityFunction,
                ForEachType = type,
                NumOfExections = numberExecutions,
                From = @from,
                To = to,
                CsvIndexes = csvIndexes,
                UniqueID = id
            };
            TestStartNode = new FlowStep
            {
                Action = dsfForEachActivity
            };

            CurrentDl = testData;
            TestData = currentDl;
            return dsfForEachActivity;
        }

        #endregion Private Test Methods
    }
}
