/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Communication;
using Dev2.Data;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Principal;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class EvaluatorTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanNotExecute()
        {
            var mockPrinciple = new Mock<IPrincipal>();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockWorkspace = new Mock<IWorkspace>();

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object);
            var serviceTest = evaluator.TryEval(EvaluatorTestSetup.HelloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Unauthorized to execute this resource.\r\n", serviceTest.FailureMessage);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Evaluator_TryEval_CanExecute_InvalidOperationException()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;
            Mock <IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.ResourceID).Returns(helloWorldId);
            mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockWorkspace = new Mock<IWorkspace>();

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object);
            evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("", serviceTest.FailureMessage);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_TestMockStep()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            var testStepTwo = new ServiceTestStepTO
            {
                ActivityID = helloWorldId,
                StepDescription = "StepTwo"
            };

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_TestMockDecisionStep()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfDecision>()).Returns(new DsfDecision());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfDecision);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Object reference not set to an instance of an object.\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_TestMockSwitchStep()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSwitch>()).Returns(new DsfSwitch());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfSwitch);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Object reference not set to an instance of an object.\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_TestMockSwitchStep_WithOutputs()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSwitch>()).Returns(new DsfSwitch());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfSwitch);

            var testOutput = new ServiceTestOutputTO
            {
                Variable = "[[a]]",
                AssertOp = "=",
                Value = "1"
            };

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo },
                Outputs = new List<IServiceTestOutput> { testOutput }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Failed: Assert Equal. Expected Equal To '1' for '[[a]]' but got ''\r\nObject reference not set to an instance of an object.\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual(1, serviceTest.Outputs.Count);
            Assert.AreEqual("=", serviceTest.Outputs[0].AssertOp);
            Assert.AreEqual("1", serviceTest.Outputs[0].Value);
            Assert.AreEqual("[[a]]", serviceTest.Outputs[0].Variable);
            Assert.IsNotNull(serviceTest.Outputs[0].Result);
            Assert.AreEqual(RunResult.TestFailed ,serviceTest.Outputs[0].Result.RunTestResult);
            Assert.AreEqual("Failed: Assert Equal. Expected Equal To '1' for '[[a]]' but got ''\r\n", serviceTest.Outputs[0].Result.Message);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_TestMockSwitchStep_WithOutputs_IsError()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSwitch>()).Returns(new DsfSwitch());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfSwitch);

            var testOutput = new ServiceTestOutputTO
            {
                Variable = "[[a]]",
                AssertOp = "There is An Error",
                Value = "1"
            };

            var serviceTestModelTO = new ServiceTestModelTO
            {
                ErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo },
                Outputs = new List<IServiceTestOutput> { testOutput }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Failed\r\nFailed: Expected Error containing '' but got 'Object reference not set to an instance of an object.'", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual(1, serviceTest.Outputs.Count);
            Assert.AreEqual("There is An Error", serviceTest.Outputs[0].AssertOp);
            Assert.AreEqual("1", serviceTest.Outputs[0].Value);
            Assert.AreEqual("[[a]]", serviceTest.Outputs[0].Variable);
            Assert.IsNull(serviceTest.Outputs[0].Result);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_TestMockSwitchStep_WithOutputs_IsNotError()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSwitch>()).Returns(new DsfSwitch());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfSwitch);

            var testOutput = new ServiceTestOutputTO
            {
                Variable = "[[a]]",
                AssertOp = "There is No Error",
                Value = "1"
            };

            var serviceTestModelTO = new ServiceTestModelTO
            {
                ErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo },
                Outputs = new List<IServiceTestOutput> { testOutput }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Failed: Object reference not set to an instance of an object.\r\nFailed: Expected Error containing '' but got 'Object reference not set to an instance of an object.'", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual(1, serviceTest.Outputs.Count);
            Assert.AreEqual("There is No Error", serviceTest.Outputs[0].AssertOp);
            Assert.AreEqual("1", serviceTest.Outputs[0].Value);
            Assert.AreEqual("[[a]]", serviceTest.Outputs[0].Variable);
            Assert.IsNull(serviceTest.Outputs[0].Result);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_MockActivityIfNecessary_DsfSequenceActivity()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSequenceActivity>()).Returns(new DsfSequenceActivity());

            var sequency = new DsfSequenceActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString(),
                Activities = new Collection<Activity> { new DsfRandomActivity() }
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(sequency);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(sequency)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(sequency);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfSequenceActivity);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Please ensure that you have entered an integer or decimal number for Start\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_MockActivityIfNecessary_DsfSequenceActivity_WithChildren()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSequenceActivity>()).Returns(new DsfSequenceActivity());

            var random = new DsfRandomActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString()
            };

            var sequency = new DsfSequenceActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString(),
                Activities = new Collection<Activity> { random }
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(sequency);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(sequency)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(sequency);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, true);
            testStepTwo.ActivityType = nameof(DsfSequenceActivity);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual(0, serviceTest.TestSteps[0].Children.Count);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);

            Assert.AreEqual(1, serviceTest.TestSteps[1].Children.Count);
            Assert.AreEqual("ChildStep", serviceTest.TestSteps[1].Children[0].StepDescription);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Children[0].Result.RunTestResult);
            Assert.AreEqual(helloWorldId, serviceTest.TestSteps[1].Children[0].ActivityID);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_MockActivityIfNecessary_DsfForEachActivity()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfForEachActivity>()).Returns(new DsfForEachActivity());

            var forEach = new DsfForEachActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString(),
                DataFunc = new ActivityFunc<string, bool>
                {
                    DisplayName = "Data Action",
                    Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
                }
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(forEach);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(forEach)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(forEach);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfForEachActivity);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("The FROM field is Required\r\nCannot execute a For Each with no content\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_MockActivityIfNecessary_DsfForEachActivity_WithChildren()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfForEachActivity>()).Returns(new DsfForEachActivity());

            var commonAssign = EvaluatorTestSetup.CommonAssign(EvaluatorTestSetup.HelloWorldId);

            var forEach = new DsfForEachActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString(),
                DataFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(forEach);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(forEach)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(forEach);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, true);
            testStepTwo.ActivityType = nameof(DsfForEachActivity);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("The FROM field is Required\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual(0, serviceTest.TestSteps[0].Children.Count);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);

            Assert.AreEqual(1, serviceTest.TestSteps[1].Children.Count);
            Assert.AreEqual("ChildStep", serviceTest.TestSteps[1].Children[0].StepDescription);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Children[0].Result.RunTestResult);
            Assert.AreEqual(helloWorldId, serviceTest.TestSteps[1].Children[0].ActivityID);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_MockActivityIfNecessary_DsfSelectAndApplyActivity()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSelectAndApplyActivity>()).Returns(new DsfSelectAndApplyActivity());

            var selectAndApply = new DsfSelectAndApplyActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString(),
                ApplyActivityFunc = new ActivityFunc<string, bool>
                {
                    DisplayName = "Data Action",
                    Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
                }
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(selectAndApply);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(selectAndApply)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(selectAndApply);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, false);
            testStepTwo.ActivityType = nameof(DsfSelectAndApplyActivity);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("DataSource cannot be empty\r\nAlias cannot be empty\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_MockActivityIfNecessary_DsfSelectAndApplyActivity_WithChildren()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSelectAndApplyActivity>()).Returns(new DsfSelectAndApplyActivity());

            var commonAssign = EvaluatorTestSetup.CommonAssign(EvaluatorTestSetup.HelloWorldId);

            var selectAndApply = new DsfSelectAndApplyActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString(),
                ApplyActivityFunc = new ActivityFunc<string, bool>
                {
                    Handler = commonAssign
                }
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(selectAndApply);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(selectAndApply)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(selectAndApply);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, true);
            testStepTwo.ActivityType = nameof(DsfSelectAndApplyActivity);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("DataSource cannot be empty\r\nAlias cannot be empty\r\n", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual(0, serviceTest.TestSteps[0].Children.Count);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);

            Assert.AreEqual(1, serviceTest.TestSteps[1].Children.Count);
            Assert.AreEqual("ChildStep", serviceTest.TestSteps[1].Children[0].StepDescription);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Children[0].Result.RunTestResult);
            Assert.AreEqual(helloWorldId, serviceTest.TestSteps[1].Children[0].ActivityID);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        public void Evaluator_TryEval_CanExecute_MockExecute()
        {
            var helloWorldId = EvaluatorTestSetup.HelloWorldId;

            Mock<IPrincipal> mockPrinciple = EvaluatorTestSetup.MockPrincipleTestUserIsInRole();

            Mock<IDSFDataObject> mockDataObject = EvaluatorTestSetup.MockDataObjectWithExecutionEnvironment(mockPrinciple);

            Mock<IDev2Activity> mockNextActivity = EvaluatorTestSetup.MockActivity();

            Mock<IDev2Activity> mockActivity = EvaluatorTestSetup.MockActivity();
            mockActivity.Setup(activity => activity.As<DsfSelectAndApplyActivity>()).Returns(new DsfSelectAndApplyActivity());
            mockActivity.Setup(activity => activity.Execute(It.IsAny<IDSFDataObject>(), It.IsAny<int>())).Returns(mockNextActivity.Object);

            var commonAssign = EvaluatorTestSetup.CommonAssign(EvaluatorTestSetup.HelloWorldId);

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            ServiceTestStepTO testStepOne = EvaluatorTestSetup.ServiceTestStepOne();

            ServiceTestStepTO testStepTwo = EvaluatorTestSetup.ServiceTestStepTwoWithStepOutput(true, true);
            testStepTwo.ActivityType = nameof(DsfSelectAndApplyActivity);

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true,
                TestSteps = new List<IServiceTestStep> { testStepOne, testStepTwo }
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object, mockBuilderSerializer.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("", serviceTest.FailureMessage);
            Assert.AreEqual(2, serviceTest.TestSteps.Count);

            Assert.AreEqual("StepOne", serviceTest.TestSteps[0].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[0].Type);
            Assert.IsNotNull(serviceTest.TestSteps[0].Result);
            Assert.IsNull(serviceTest.TestSteps[0].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[0].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[0].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[0].Result.TestName);

            Assert.AreEqual(0, serviceTest.TestSteps[0].Children.Count);

            Assert.AreEqual("StepTwo", serviceTest.TestSteps[1].StepDescription);
            Assert.AreEqual(StepType.Mock, serviceTest.TestSteps[1].Type);
            Assert.IsNotNull(serviceTest.TestSteps[1].Result);
            Assert.IsNull(serviceTest.TestSteps[1].Result.DebugForTest);
            Assert.IsNull(serviceTest.TestSteps[1].Result.Message);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Result.RunTestResult);
            Assert.IsNull(serviceTest.TestSteps[1].Result.TestName);

            Assert.AreEqual(1, serviceTest.TestSteps[1].Children.Count);
            Assert.AreEqual("ChildStep", serviceTest.TestSteps[1].Children[0].StepDescription);
            Assert.AreEqual(RunResult.TestPending, serviceTest.TestSteps[1].Children[0].Result.RunTestResult);
            Assert.AreEqual(helloWorldId, serviceTest.TestSteps[1].Children[0].ActivityID);
        }

        internal class EvaluatorTestSetup
        {
            public static Guid HelloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");

            public static Mock<IPrincipal> MockPrincipleTestUserIsInRole()
            {
                var mockPrinciple = new Mock<IPrincipal>();
                mockPrinciple.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
                mockPrinciple.Setup(u => u.Identity.Name).Returns("TestUser");
                return mockPrinciple;
            }

            public static Mock<IDSFDataObject> MockDataObjectWithExecutionEnvironment(Mock<IPrincipal> mockPrinciple)
            {
                var mockDataObject = new Mock<IDSFDataObject>();
                mockDataObject.Setup(o => o.ResourceID).Returns(HelloWorldId);
                mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);
                mockDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
                return mockDataObject;
            }

            public static Mock<IDev2Activity> MockActivity()
            {
                var mockActivity = new Mock<IDev2Activity>();
                mockActivity.Setup(activity => activity.ActivityId).Returns(HelloWorldId);
                mockActivity.Setup(activity => activity.UniqueID).Returns(HelloWorldId.ToString());
                return mockActivity;
            }

            public static ServiceTestStepTO ServiceTestStepOne()
            {
                var stepOneUniqueId = Guid.NewGuid();

                var testStepOne = new ServiceTestStepTO
                {
                    ActivityID = stepOneUniqueId,
                    StepDescription = "StepOne"
                };
                return testStepOne;
            }

            private static ServiceTestOutputTO StepOutput()
            {
                return new ServiceTestOutputTO
                {
                    Variable = GlobalConstants.ArmResultText,
                    Value = "Arm result"
                };
            }

            private static ServiceTestStepTO TestStepChild()
            {
                return new ServiceTestStepTO
                {
                    ActivityID = HelloWorldId,
                    StepDescription = "ChildStep"
                };
            }

            public static ServiceTestStepTO ServiceTestStepTwoWithStepOutput(bool includeStepOutputs, bool includeChildren)
            {
                var testStepTwo = new ServiceTestStepTO
                {
                    ActivityID = HelloWorldId,
                    StepDescription = "StepTwo"
                };
                if (includeStepOutputs)
                {
                    testStepTwo.StepOutputs = new ObservableCollection<IServiceTestOutput> { StepOutput() };
                }
                if (includeChildren)
                {
                    testStepTwo.Children = new ObservableCollection<IServiceTestStep> { TestStepChild() };
                }

                return testStepTwo;
            }

            public static DsfMultiAssignActivity CommonAssign(Guid? uniqueId = null)
            {
                return uniqueId.HasValue ? new DsfMultiAssignActivity { UniqueID = uniqueId.Value.ToString() } : new DsfMultiAssignActivity();
            }
        }
    }
}
