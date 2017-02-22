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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities
{
    [TestClass]
    public class ServiceTestHelperTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_NotServiceTestExecution_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(false, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, "MultiAssign",new ObservableCollection<IServiceTestOutput>(),StepType.Assert );
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject,serviceTestTestSteps, "");
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_NullServiceSteps_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            //------------Assert Preconditions-------------------
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject,null, "");
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_MockServiceSteps_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, "MultiAssign", new ObservableCollection<IServiceTestOutput>(), StepType.Mock);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, "");
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertStepIDNotMatching_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.NewGuid(), "MultiAssign", new ObservableCollection<IServiceTestOutput>(), StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.NewGuid().ToString());
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertStepForEach_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfForEachActivity).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertStepSelectAndApply_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfSelectAndApplyActivity).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertStepSequence_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfSequenceActivity).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertStepNullOutputs_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfMultiAssignActivity).Name, null, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertStepNoOutputs_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfMultiAssignActivity).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNull(serviceTestStepTO.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertNoDebugItems_NullResult_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello"
            };
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertNoDebugItems_Result_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.Empty, "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertNoDebugStates_NullResult_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.NewGuid(), "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello"
            };
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            var debugState = new DebugState { ID = Guid.NewGuid() };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID,dsfDataObject.TestName,debugState);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertNoDebugStates_Result_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var dsfDataObject = GetDataObject(true, Guid.NewGuid(), "Test1");
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(Guid.Empty, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            var debugState = new DebugState { ID = Guid.NewGuid() };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Assert Preconditions-------------------
            Assert.IsNull(serviceTestStepTO.Result);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, Guid.Empty.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_Result_SetResultPassed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(serviceTestOutputTO.Value) };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestPassed,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_MultipleValuesForCriteria_Result_SetResultPassed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "Is Between",
                From = "5",
                To = "10",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("8") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestStepTO.Result = new TestRunResult {RunTestResult = RunResult.TestFailed};
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestPassed,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_DebugDataObject_AssertWithStates_Result_SetResultPassed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(serviceTestOutputTO.Value) };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestPassed,serviceTestStepTO.Result.RunTestResult);
            Assert.AreEqual(1,debugState.AssertResultList.Count);
            Assert.AreEqual(1,debugState.AssertResultList[0].ResultsList.Count);
            Assert.AreEqual("Passed",debugState.AssertResultList[0].ResultsList[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_DebugDataObject_NoVariable_AssertWithStates_Result_SetResultPassed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            mockDataObject.Setup(o => o.IsDebugMode()).Returns(true);
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(serviceTestOutputTO.Value) };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid,serviceTestStepTO.Result.RunTestResult);
            Assert.AreEqual(1,debugState.AssertResultList.Count);
            Assert.AreEqual(1,debugState.AssertResultList[0].ResultsList.Count);
            Assert.AreEqual("Invalid: Nothing to assert.", debugState.AssertResultList[0].ResultsList[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_NullStepOutputs_AssertWithStates_Result_SetResultPassed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            IServiceTestOutput serviceTestOutputTO = null;
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("test") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestPassed,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_Result_SetOutputResultPassed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(serviceTestOutputTO.Value) };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestPassed,serviceTestStepTO.StepOutputs[0].Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_Result_SetResultFailed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestFailed,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_Result_SetOutputResultFailed()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[Bob]]",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestFailed,serviceTestStepTO.StepOutputs[0].Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyVariable_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyVariable_SetOutputResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid, serviceTestStepTO.StepOutputs[0].Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyValue_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[var]]",
                AssertOp = "=",
                Value = "",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestFailed,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyValue_SetOutputResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[var]]",
                AssertOp = "=",
                Value = "",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestFailed, serviceTestStepTO.StepOutputs[0].Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyVariableWithValue_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyVariableWithValue_SetOutputResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "",
                AssertOp = "=",
                Value = "hello",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestInvalid, serviceTestStepTO.StepOutputs[0].Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyVariableEmptyValue_SetResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "",
                AssertOp = "=",
                Value = "",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.TestPassed,serviceTestStepTO.Result.RunTestResult);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestHelper_UpdateDebugStateWithAssertions")]
        public void TestHelper_UpdateDebugStateWithAssertions_AssertWithStates_EmptyVariableEmptyValue_SetOutputResultInvalid()
        {
            //------------Setup for test--------------------------
            var mockDataObject = GetMockDataObject(true, Guid.NewGuid(), "Test1");
            var mockEnv = new Mock<IExecutionEnvironment>();
            var uniqueID = Guid.NewGuid();
            var serviceTestTestSteps = new List<IServiceTestStep>();
            var serviceTestOutputs = new ObservableCollection<IServiceTestOutput>();
            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "",
                AssertOp = "=",
                Value = "",
                Result = new TestRunResult { RunTestResult = RunResult.TestPassed }
            };
            var warewolfAtoms = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString("Bye") };
            mockEnv.Setup(environment => environment.EvalAsList(It.IsAny<string>(), 0, false)).Returns(warewolfAtoms);
            mockDataObject.Setup(o => o.Environment).Returns(mockEnv.Object);
            var dsfDataObject = mockDataObject.Object;
            serviceTestOutputs.Add(serviceTestOutputTO);
            var serviceTestStepTO = new ServiceTestStepTO(uniqueID, typeof(DsfMultiAssignActivity).Name, serviceTestOutputs, StepType.Assert);
            serviceTestTestSteps.Add(serviceTestStepTO);
            
            var debugState = new DebugState { ID = uniqueID };
            TestDebugMessageRepo.Instance.AddDebugItem(dsfDataObject.ResourceID, dsfDataObject.TestName, debugState);
            //------------Execute Test---------------------------
            ServiceTestHelper.UpdateDebugStateWithAssertions(dsfDataObject, serviceTestTestSteps, uniqueID.ToString());
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceTestStepTO.Result);
            Assert.AreEqual(RunResult.None, serviceTestStepTO.StepOutputs[0].Result.RunTestResult);
        }

        private static IDSFDataObject GetDataObject(bool isServiceTestExecution, Guid resourceId, string testName)
        {
            var mockDataObject = GetMockDataObject(isServiceTestExecution, resourceId, testName);
            var dsfDataObject = mockDataObject.Object;
            return dsfDataObject;
        }

        private static Mock<IDSFDataObject> GetMockDataObject(bool isServiceTestExecution, Guid resourceId, string testName)
        {
            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.IsServiceTestExecution).Returns(isServiceTestExecution);
            mockDataObject.Setup(o => o.ResourceID).Returns(resourceId);
            mockDataObject.Setup(o => o.TestName).Returns(testName);
            return mockDataObject;
        }
    }
}
