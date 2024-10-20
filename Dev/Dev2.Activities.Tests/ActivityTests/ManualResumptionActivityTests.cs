/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.State;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Auditing;
using Warewolf.Common.NetStandard20;
using Warewolf.Driver.Persistence;
using Warewolf.Execution;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class ManualResumptionActivityTests : BaseActivityUnitTest
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Initialize()
        {
            var manualResumptionActivity = new ManualResumptionActivity();
            Assert.AreEqual("Manual Resumption", manualResumptionActivity.DisplayName);
            Assert.IsFalse(manualResumptionActivity.OverrideInputVariables);
            Assert.IsNull(manualResumptionActivity.SuspensionId);
            Assert.AreEqual("Data Action", manualResumptionActivity.OverrideDataFunc.DisplayName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Initialize_With_Values_OverrideInputVariables_True()
        {
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var manualResumptionActivity = new ManualResumptionActivity()
            {
                SuspensionId = "15",
                OverrideInputVariables = true,
                OverrideDataFunc = activityFunction,
                Response = "[[result]]"
            };

            Assert.AreEqual("Manual Resumption", manualResumptionActivity.DisplayName);
            Assert.AreEqual("15", manualResumptionActivity.SuspensionId);
            Assert.IsTrue(manualResumptionActivity.OverrideInputVariables);
            Assert.AreEqual("TestService", manualResumptionActivity.OverrideDataFunc.DisplayName);

            var result = manualResumptionActivity.GetOutputs();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(null, result[0]);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_GetChildrenNodes_ShouldReturnChildNode()
        {
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var manualResumptionActivity = new ManualResumptionActivity()
            {
                SuspensionId = "15",
                OverrideInputVariables = true,
                OverrideDataFunc = activityFunction,
                Response = "[[result]]"
            };

            Assert.AreEqual("Manual Resumption", manualResumptionActivity.DisplayName);
            Assert.AreEqual("15", manualResumptionActivity.SuspensionId);
            Assert.IsTrue(manualResumptionActivity.OverrideInputVariables);
            Assert.AreEqual("TestService", manualResumptionActivity.OverrideDataFunc.DisplayName);

            var result = manualResumptionActivity.GetChildrenNodes().ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(activity, result[0]);
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Initialize_With_Values_OverrideInputVariables_False()
        {
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var manualResumptionActivity = new ManualResumptionActivity()
            {
                SuspensionId = "15",
                OverrideInputVariables = false,
                OverrideDataFunc = activityFunction,
                Response = "[[result]]"
            };

            Assert.AreEqual("Manual Resumption", manualResumptionActivity.DisplayName);
            Assert.AreEqual("15", manualResumptionActivity.SuspensionId);
            Assert.IsFalse(manualResumptionActivity.OverrideInputVariables);
            Assert.AreEqual("TestService", manualResumptionActivity.OverrideDataFunc.DisplayName);

            var result = manualResumptionActivity.GetOutputs();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(null, result[0]);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Equals_Set_OtherIsEqual_Returns_IsTrue()
        {
            var manualResumptionActivity = new ManualResumptionActivity();
            var manualResumptionActivityOther = manualResumptionActivity;
            var equal = manualResumptionActivity.Equals(manualResumptionActivityOther);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Equals_Set_BothAreObjects_Returns_IsFalse()
        {
            object manualResumptionActivity = new ManualResumptionActivity();
            var manualResumptionActivityOther = new object();
            var equal = manualResumptionActivity.Equals(manualResumptionActivityOther);
            Assert.IsFalse(equal);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Equals_Set_OtherIsObjectOfManualResumptionActivityEqual_Returns_IsFalse()
        {
            var manualResumptionActivity = new ManualResumptionActivity();
            object manualResumptionActivityOther = new ManualResumptionActivity();
            var equal = manualResumptionActivity.Equals(manualResumptionActivityOther);
            Assert.IsFalse(equal);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_GetHashCode()
        {
            var manualResumptionActivity = new ManualResumptionActivity();
            var hashCode = manualResumptionActivity.GetHashCode();
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_GetState()
        {
            var manualResumptionActivity = new ManualResumptionActivity
            {
                Response = "[[result]]",
                OverrideInputVariables = false,
                SuspensionId = "321"
            };
            var stateItems = manualResumptionActivity.GetState();

            Assert.AreEqual(3, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Response",
                    Value = "[[result]]",
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = "SuspensionId",
                    Value = "321",
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "OverrideInputVariables",
                    Value = "False",
                    Type = StateVariable.StateType.Input
                },
            };

            var iter = manualResumptionActivity.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
            );

            foreach(var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_OverrideInputVariables_False_Success()
        {
            //------------Setup for test--------------------------
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            const string SuspensionId = "321";
            const bool OverrideInputVariables = false;

            var mockResumeJob = new Mock<IPersistenceExecution>();
            var mockExecutionLogPublish = new Mock<IExecutionLogPublisher>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, "")).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object, mockExecutionLogPublish.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = OverrideInputVariables,
                SuspensionId = SuspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            mockResumeJob.Verify(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, ""), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_OverrideInputVariables_VariableInput_True_Success()
        {
            //------------Setup for test--------------------------
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            const string SuspensionId = "321";
            const bool OverrideInputVariables = true;
            var environment = env.ToJson();
            var startActivityId = Guid.NewGuid();

            var mockPersistedValues = new Mock<IPersistedValues>();
            mockPersistedValues.Setup(o => o.SuspendedEnvironment).Returns(environment);
            mockPersistedValues.Setup(o => o.StartActivityId).Returns(startActivityId);

            var mockResumeJob = new Mock<IPersistenceExecution>();
            var mockExecutionLogPublish = new Mock<IExecutionLogPublisher>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, environment)).Verifiable();
            mockResumeJob.Setup(o => o.GetPersistedValues(SuspensionId)).Returns(mockPersistedValues.Object).Verifiable();
            mockResumeJob.Setup(o => o.ManualResumeWithOverrideJob(It.IsAny<IDSFDataObject>(), SuspensionId)).Returns(startActivityId.ToString()).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object, mockExecutionLogPublish.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = OverrideInputVariables,
                SuspensionId = SuspensionId,
                OverrideDataFunc = activityFunction,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataObject.Environment.AllErrors.Count);
            mockResumeJob.Verify(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, environment), Times.Never);
            mockResumeJob.Verify(o => o.GetPersistedValues(SuspensionId), Times.Once);
            mockResumeJob.Verify(o => o.ManualResumeWithOverrideJob(It.IsAny<IDSFDataObject>(), SuspensionId), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_OverrideInputVariables_InnerActivity_True_Success()
        {
            //------------Setup for test--------------------------
            var activity = CreateSequenceActivity();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            const string SuspensionId = "321";
            const bool OverrideInputVariables = true;
            var environment = env.ToJson();
            var startActivityId = Guid.NewGuid();

            var mockPersistedValues = new Mock<IPersistedValues>();
            mockPersistedValues.Setup(o => o.SuspendedEnvironment).Returns(environment);
            mockPersistedValues.Setup(o => o.StartActivityId).Returns(startActivityId);

            var mockResumeJob = new Mock<IPersistenceExecution>();
            var mockExecutionLogPublish = new Mock<IExecutionLogPublisher>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, environment)).Verifiable();
            mockResumeJob.Setup(o => o.GetPersistedValues(SuspensionId)).Returns(mockPersistedValues.Object).Verifiable();
            mockResumeJob.Setup(o => o.ManualResumeWithOverrideJob(It.IsAny<IDSFDataObject>(), SuspensionId)).Returns(GlobalConstants.Success).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object, mockExecutionLogPublish.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = OverrideInputVariables,
                SuspensionId = SuspensionId,
                OverrideDataFunc = activityFunction,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(GlobalConstants.Success, manualResumptionActivity.Response);
            mockResumeJob.Verify(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, environment), Times.Never);
            mockResumeJob.Verify(o => o.GetPersistedValues(SuspensionId), Times.Once);
            mockResumeJob.Verify(o => o.ManualResumeWithOverrideJob(It.IsAny<IDSFDataObject>(), SuspensionId), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_OverrideInputVariables_NonVariableInput_True_Success()
        {
            //------------Setup for test--------------------------
            var activity = CreateWorkflowNoVariable();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "1234", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            const string SuspensionId = "321";
            const bool OverrideInputVariables = true;
            var overrideEnvironment = env.ToJson();
            var startActivityId = Guid.NewGuid();

            var mockPersistedValues = new Mock<IPersistedValues>();
            mockPersistedValues.Setup(o => o.SuspendedEnvironment).Returns(overrideEnvironment);
            mockPersistedValues.Setup(o => o.StartActivityId).Returns(startActivityId);

            var mockResumeJob = new Mock<IPersistenceExecution>();
            var mockExecutionLogPublish = new Mock<IExecutionLogPublisher>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, overrideEnvironment)).Verifiable();
            mockResumeJob.Setup(o => o.GetPersistedValues(SuspensionId)).Returns(mockPersistedValues.Object).Verifiable();
            mockResumeJob.Setup(o => o.ManualResumeWithOverrideJob(It.IsAny<IDSFDataObject>(), SuspensionId)).Returns(startActivityId.ToString()).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object, mockExecutionLogPublish.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = OverrideInputVariables,
                SuspensionId = SuspensionId,
                OverrideDataFunc = activityFunction,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataObject.Environment.AllErrors.Count);
            mockResumeJob.Verify(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, overrideEnvironment), Times.Never);
            mockResumeJob.Verify(o => o.GetPersistedValues(SuspensionId), Times.Once);
            mockResumeJob.Verify(o => o.ManualResumeWithOverrideJob(It.IsAny<IDSFDataObject>(), SuspensionId), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_SuspensionID_IsNullOrWhiteSpace_FailWithMessage()
        {
            //------------Setup for test--------------------------
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var manualResumptionActivity = new ManualResumptionActivity(config, new Mock<IPersistenceExecution>().Object, new Mock<IExecutionLogPublisher>().Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = false,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataObject.Environment.AllErrors.Count);
            var errors = dataObject.Environment.AllErrors.ToList();
            Assert.AreEqual("", manualResumptionActivity.Response);
            Assert.AreEqual(errors[0], ErrorResource.ManualResumptionSuspensionIdBlank);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_PersistenceNotConfigured_FailWithMessage()
        {
            //------------Setup for test--------------------------
            var resourceId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);
            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = "workflowName",
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = "url",
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };
            var config = new PersistenceSettings
            {
                Enable = false
            };

            var manualResumptionActivity = new ManualResumptionActivity(config, new Mock<IPersistenceExecution>().Object, new Mock<IExecutionLogPublisher>().Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = false,
                SuspensionId = "321"
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataObject.Environment.AllErrors.Count);
            var errors = dataObject.Environment.AllErrors.ToList();
            Assert.AreEqual("", manualResumptionActivity.Response);
            Assert.AreEqual(errors[0], ErrorResource.PersistenceSettingsNoConfigured);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_GetFindMissingType_GivenIsNew_ShouldSetManualResumptionActivity()
        {
            //---------------Set up test pack-------------------

            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var manualResumptionActivity = new ManualResumptionActivity()
            {
                SuspensionId = "15",
                OverrideInputVariables = true,
                OverrideDataFunc = activityFunction,
                Response = "[[result]]"
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var enFindMissingType = manualResumptionActivity.GetFindMissingType();
            //---------------Test Result -----------------------
            Assert.AreEqual(enFindMissingType.ManualResumption, enFindMissingType);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_GetSuspendedEnvironment_Error_ManualResumptionSuspensionEnvBlank()
        {
            //------------Setup for test--------------------------
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            const string SuspensionId = "321";

            var mockResumeJob = new Mock<IPersistenceExecution>();
            var mockExecutionLogPublish = new Mock<IExecutionLogPublisher>();
            mockResumeJob.Setup(o => o.GetPersistedValues(SuspensionId)).Returns(new Mock<IPersistedValues>().Object).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object, mockExecutionLogPublish.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = true,
                SuspensionId = SuspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, env.AllErrors.Count);
            Assert.AreEqual(ErrorResource.ManualResumptionSuspensionEnvBlank, env.AllErrors.First());
            Assert.AreEqual("", manualResumptionActivity.Response);
            mockResumeJob.Verify(o => o.GetPersistedValues(SuspensionId), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_OverrideInputVariables_ResumeInputVarNames_Equals_SuspendVarInputString()
        {
            //------------Setup for test--------------------------
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();

            var overrideEnv = CreateExecutionEnvironment();
            overrideEnv.Assign("[[Input1]]", "ResumeInput1", 0);
            overrideEnv.Assign("[[Input2]]", "ResumeInput2", 0);

            const string SuspendedEnv = "{\"Environment\":{\"scalars\":{\"Input1\":\"Input1\",\"Input2\":\"Input2\"},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}";
            const string ExpectedEnv = "{\"Environment\":{\"scalars\":{\"Input1\":\"ResumeInput1\",\"Input2\":\"ResumeInput2\"},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}";

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = overrideEnv,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            const string SuspensionId = "321";
            const bool OverrideInputVariables = true;
            var startActivityId = Guid.NewGuid();

            var resumeObject = dataObject;
            resumeObject.StartActivityId = startActivityId;
            resumeObject.Environment = dataObject.Environment;

            var mockPersistedValues = new Mock<IPersistedValues>();
            mockPersistedValues.Setup(o => o.SuspendedEnvironment).Returns(SuspendedEnv);
            mockPersistedValues.Setup(o => o.StartActivityId).Returns(startActivityId);

            var mockResumeJob = new Mock<IPersistenceExecution>();
            var mockExecutionLogPublish = new Mock<IExecutionLogPublisher>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, SuspendedEnv)).Verifiable();
            mockResumeJob.Setup(o => o.GetPersistedValues(SuspensionId)).Returns(mockPersistedValues.Object).Verifiable();
            mockResumeJob.Setup(o => o.ManualResumeWithOverrideJob(resumeObject, SuspensionId)).Returns(startActivityId.ToString()).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object, mockExecutionLogPublish.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = OverrideInputVariables,
                SuspensionId = SuspensionId,
                OverrideDataFunc = activityFunction,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataObject.Environment.AllErrors.Count);
            Assert.AreNotEqual(overrideEnv.ToJson(), SuspendedEnv);
            Assert.AreEqual(overrideEnv.ToJson(), ExpectedEnv);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_OverrideInputVariables_ResumeInputVarNames_Equals_SuspendJsonInput()
        {
            //------------Setup for test--------------------------
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            const string WorkflowName = "workflowName";
            const string Url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();

            const string ExpectedEnv = "{\"Environment\":{\"scalars\":{\"Input1\":\"ResumeInput1\",\"Input2\":\"ResumeInput2\",\"SuspendId\":966},\"record_sets\":{},\"json_objects\":{\"obj\":{\"x\":\"4\",\"y\":\"5\",\"z\":\"6\"}}},\"Errors\":[],\"AllErrors\":[]}";

            var overrideEnv = CreateExecutionEnvironment();
            overrideEnv.FromJson(ExpectedEnv);

            var startActivityId = Guid.NewGuid();
            const string SuspendedEnv = "{\"Environment\":{\"scalars\":{\"Input1\":\"Input1\",\"Input2\":\"Input2\"},\"record_sets\":{},\"json_objects\":{\"obj\":{\"x\":\"1\",\"y\":\"2\",\"z\":\"3\"}}},\"Errors\":[],\"AllErrors\":[]}";
            var mockPersistedValues = new Mock<IPersistedValues>();
            mockPersistedValues.Setup(o => o.SuspendedEnvironment).Returns(SuspendedEnv);
            mockPersistedValues.Setup(o => o.StartActivityId).Returns(startActivityId);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentId = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = WorkflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = Url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentId,
                Environment = overrideEnv,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var config = new PersistenceSettings
            {
                Enable = true
            };
            const string SuspensionId = "321";
            const bool OverrideInputVariables = true;

            var resumeObject = dataObject;
            resumeObject.StartActivityId = startActivityId;
            resumeObject.Environment = dataObject.Environment;

            var mockResumeJob = new Mock<IPersistenceExecution>();
            var mockExecutionLogPublish = new Mock<IExecutionLogPublisher>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, SuspensionId, OverrideInputVariables, SuspendedEnv)).Verifiable();
            mockResumeJob.Setup(o => o.GetPersistedValues(SuspensionId)).Returns(mockPersistedValues.Object).Verifiable();
            mockResumeJob.Setup(o => o.ManualResumeWithOverrideJob(resumeObject, SuspensionId)).Returns(startActivityId.ToString()).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object, mockExecutionLogPublish.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = OverrideInputVariables,
                SuspensionId = SuspensionId,
                OverrideDataFunc = activityFunction,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataObject.Environment.AllErrors.Count);
            Assert.AreNotEqual(overrideEnv.ToJson(), SuspendedEnv);
            Assert.AreEqual(overrideEnv.ToJson(), ExpectedEnv);
        }

        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        DsfActivity CreateWorkflow()
        {
            var activity = new DsfActivity
            {
                ServiceName = "InnerService",
                DisplayName = "TestService",
                InputMapping = ActivityStrings.ManualResumption_Input_Mapping,
                OutputMapping = ActivityStrings.ManualResumption_Output_Mapping,
            };

            return activity;
        }

        DsfSequenceActivity CreateSequenceActivity()
        {
            var activities = new Collection<Activity>
            {
                new DsfDotNetMultiAssignActivity(),
                new DsfActivity(),
            };
            var dsfSequenceActivity = new DsfSequenceActivity
            {
                Activities = activities
            };
            return dsfSequenceActivity;
        }

        DsfActivity CreateWorkflowNoVariable()
        {
            var activity = new DsfActivity
            {
                ServiceName = "InnerService",
                DisplayName = "TestService",
                InputMapping = ActivityStrings.ManualResumption_Input_Mapping_NoVariable,
                OutputMapping = ActivityStrings.ManualResumption_Output_Mapping,
            };

            return activity;
        }
    }
}