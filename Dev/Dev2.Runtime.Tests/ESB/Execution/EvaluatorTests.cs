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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Principal;
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
            var helloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");
            var mockPrinciple = new Mock<IPrincipal>();

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.ResourceID).Returns(helloWorldId);
            mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);
            mockDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockWorkspace = new Mock<IWorkspace>();

            var serviceTestModelTO = new ServiceTestModelTO
            {
                NoErrorExpected = true
            };

            var evaluator = new Evaluator(mockDataObject.Object, mockResourceCatalog.Object, mockWorkspace.Object);
            var serviceTest = evaluator.TryEval(helloWorldId, mockDataObject.Object, serviceTestModelTO);

            Assert.AreEqual("Unauthorized to execute this resource.\r\n", serviceTest.FailureMessage);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Evaluator))]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Evaluator_TryEval_CanExecute_InvalidOperationException()
        {
            var helloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
            mockPrinciple.Setup(u => u.Identity.Name).Returns("TestUser");

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
            var helloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
            mockPrinciple.Setup(u => u.Identity.Name).Returns("TestUser");

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.ResourceID).Returns(helloWorldId);
            mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);
            mockDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(activity => activity.ActivityId).Returns(helloWorldId);
            mockActivity.Setup(activity => activity.UniqueID).Returns(helloWorldId.ToString());

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
            var helloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
            mockPrinciple.Setup(u => u.Identity.Name).Returns("TestUser");

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.ResourceID).Returns(helloWorldId);
            mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);
            mockDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(activity => activity.ActivityId).Returns(helloWorldId);
            mockActivity.Setup(activity => activity.UniqueID).Returns(helloWorldId.ToString());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            var stepOneUniqueId = Guid.NewGuid();

            var testStepOne = new ServiceTestStepTO
            {
                UniqueId = stepOneUniqueId,
                StepDescription = "StepOne"
            };
            var testStepTwo = new ServiceTestStepTO
            {
                UniqueId = helloWorldId,
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
            var helloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
            mockPrinciple.Setup(u => u.Identity.Name).Returns("TestUser");

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.ResourceID).Returns(helloWorldId);
            mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);
            mockDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(activity => activity.ActivityId).Returns(helloWorldId);
            mockActivity.Setup(activity => activity.UniqueID).Returns(helloWorldId.ToString());
            mockActivity.Setup(activity => activity.As<DsfDecision>()).Returns(new DsfDecision());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            var stepOneUniqueId = Guid.NewGuid();

            var testStepOne = new ServiceTestStepTO
            {
                UniqueId = stepOneUniqueId,
                StepDescription = "StepOne"
            };

            var stepOutput = new ServiceTestOutputTO
            {
                Variable = GlobalConstants.ArmResultText,
                Value = "Arm result"
            };

            var testStepTwo = new ServiceTestStepTO
            {
                ActivityType = nameof(DsfDecision),
                UniqueId = helloWorldId,
                StepDescription = "StepTwo",
                StepOutputs = new ObservableCollection<IServiceTestOutput> { stepOutput }
            };

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
            var helloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
            mockPrinciple.Setup(u => u.Identity.Name).Returns("TestUser");

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.ResourceID).Returns(helloWorldId);
            mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);
            mockDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(activity => activity.ActivityId).Returns(helloWorldId);
            mockActivity.Setup(activity => activity.UniqueID).Returns(helloWorldId.ToString());
            mockActivity.Setup(activity => activity.As<DsfSwitch>()).Returns(new DsfSwitch());

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(mockActivity.Object);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(mockActivity.Object)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(mockActivity.Object);

            var stepOneUniqueId = Guid.NewGuid();

            var testStepOne = new ServiceTestStepTO
            {
                UniqueId = stepOneUniqueId,
                StepDescription = "StepOne"
            };

            var stepOutput = new ServiceTestOutputTO
            {
                Variable = GlobalConstants.ArmResultText,
                Value = "Arm result"
            };

            var testStepTwo = new ServiceTestStepTO
            {
                ActivityType = nameof(DsfSwitch),
                UniqueId = helloWorldId,
                StepDescription = "StepTwo",
                StepOutputs = new ObservableCollection<IServiceTestOutput> { stepOutput }
            };

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
        public void Evaluator_TryEval_CanExecute_ReplaceActivityWithMock_MockActivityIfNecessary()
        {
            var helloWorldId = Guid.Parse("acb75027-ddeb-47d7-814e-a54c37247ec1");

            var mockPrinciple = new Mock<IPrincipal>();
            mockPrinciple.Setup(u => u.IsInRole(It.IsAny<string>())).Returns(true);
            mockPrinciple.Setup(u => u.Identity.Name).Returns("TestUser");

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.ResourceID).Returns(helloWorldId);
            mockDataObject.Setup(o => o.ExecutingUser).Returns(mockPrinciple.Object);
            mockDataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(activity => activity.ActivityId).Returns(helloWorldId);
            mockActivity.Setup(activity => activity.UniqueID).Returns(helloWorldId.ToString());
            mockActivity.Setup(activity => activity.As<DsfSequenceActivity>()).Returns(new DsfSequenceActivity());

            var sequency = new DsfSequenceActivity
            {
                ActivityId = helloWorldId,
                UniqueID = helloWorldId.ToString(),
                Activities = new Collection<System.Activities.Activity> { new DsfRandomActivity() }
            };

            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(resourceCatalog => resourceCatalog.Parse(Guid.Empty, helloWorldId)).Returns(sequency);
            var mockWorkspace = new Mock<IWorkspace>();

            var mockBuilderSerializer = new Mock<IBuilderSerializer>();
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.SerializeToBuilder(sequency)).Returns(new System.Text.StringBuilder());
            mockBuilderSerializer.Setup(builderSerializer => builderSerializer.Deserialize<IDev2Activity>(It.IsAny<System.Text.StringBuilder>())).Returns(sequency);

            var stepOneUniqueId = Guid.NewGuid();

            var testStepOne = new ServiceTestStepTO
            {
                UniqueId = stepOneUniqueId,
                StepDescription = "StepOne"
            };

            var stepOutput = new ServiceTestOutputTO
            {
                Variable = GlobalConstants.ArmResultText,
                Value = "Arm result"
            };

            var testStepTwo = new ServiceTestStepTO
            {
                ActivityType = nameof(DsfSequenceActivity),
                UniqueId = helloWorldId,
                StepDescription = "StepTwo",
                StepOutputs = new ObservableCollection<IServiceTestOutput> { stepOutput }
            };

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

        //TODO:
        // foundTestStep.ActivityType == typeof(DsfForEachActivity).Name
        // foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name
    }
}
