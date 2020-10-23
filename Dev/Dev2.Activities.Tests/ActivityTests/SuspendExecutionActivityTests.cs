using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.State;
using Dev2.Data.Interfaces.Enums;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Auditing;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class SuspendExecutionActivityTests : BaseActivityUnitTest
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Initialize()
        {
            var suspendExecutionActivity = new SuspendExecutionActivity();
            Assert.AreEqual("Suspend Execution", suspendExecutionActivity.DisplayName);
            Assert.IsNull(suspendExecutionActivity.PersistValue);
            Assert.AreEqual(enSuspendOption.SuspendUntil, suspendExecutionActivity.SuspendOption);
            Assert.IsFalse(suspendExecutionActivity.AllowManualResumption);
            Assert.AreEqual("Data Action", suspendExecutionActivity.SaveDataFunc.DisplayName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Initialize_With_Values()
        {
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var suspendExecutionActivity = new SuspendExecutionActivity
            {
                SuspendOption = enSuspendOption.SuspendForDays,
                PersistValue = "15",
                AllowManualResumption = true,
                SaveDataFunc = activityFunction,
                Response = "[[result]]"
            };

            Assert.AreEqual("Suspend Execution", suspendExecutionActivity.DisplayName);
            Assert.AreEqual("15", suspendExecutionActivity.PersistValue);
            Assert.AreEqual(enSuspendOption.SuspendForDays, suspendExecutionActivity.SuspendOption);
            Assert.IsTrue(suspendExecutionActivity.AllowManualResumption);
            Assert.AreEqual("TestService", suspendExecutionActivity.SaveDataFunc.DisplayName);

            var result = suspendExecutionActivity.GetOutputs();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(null, result[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Equals_Set_OtherIsNull_Returns_IsFalse()
        {
            var suspendExecutionActivity = new SuspendExecutionActivity();
            var equals = suspendExecutionActivity.Equals(null);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Equals_Set_OtherIsEqual_Returns_IsTrue()
        {
            var suspendExecutionActivity = new SuspendExecutionActivity();
            var suspendExecutionActivityOther = suspendExecutionActivity;
            var equal = suspendExecutionActivity.Equals(suspendExecutionActivityOther);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Equals_Set_BothAreObjects_Returns_IsFalse()
        {
            object suspendExecutionActivity = new SuspendExecutionActivity();
            var suspendExecutionActivityOther = new object();
            var equal = suspendExecutionActivity.Equals(suspendExecutionActivityOther);
            Assert.IsFalse(equal);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Equals_Set_OtherIsObjectOfSuspendExecutionActivityEqual_Returns_IsFalse()
        {
            var suspendExecutionActivity = new SuspendExecutionActivity();
            object suspendExecutionActivityOther = new SuspendExecutionActivity();
            var equal = suspendExecutionActivity.Equals(suspendExecutionActivityOther);
            Assert.IsFalse(equal);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_GetHashCode()
        {
            var suspendExecutionActivity = new SuspendExecutionActivity();
            var hashCode = suspendExecutionActivity.GetHashCode();
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_GetState()
        {
            var suspendExecutionActivity = new SuspendExecutionActivity
            {
                SuspendOption = enSuspendOption.SuspendForDays,
                PersistValue = "15",
                AllowManualResumption = true,
                Response = "[[result]]",
            };
            var stateItems = suspendExecutionActivity.GetState();

            Assert.AreEqual(4, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "SuspendOption",
                    Value = enSuspendOption.SuspendForDays.ToString(),
                    Type = StateVariable.StateType.Input,
                },
                new StateVariable
                {
                    Name = "PersistValue",
                    Value = "15",
                    Type = StateVariable.StateType.Input,
                },
                new StateVariable
                {
                    Name = "AllowManualResumption",
                    Value = "True",
                    Type = StateVariable.StateType.Input,
                },
                new StateVariable
                {
                    Name = "Response",
                    Value = "[[result]]",
                    Type = StateVariable.StateType.Output
                },
            };

            var iter = suspendExecutionActivity.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
            );

            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_SuspendForDays_SaveSuspendIDIntoEnv_Success()
        {
            //------------Setup for test--------------------------
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentID = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = workflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var dev2Activities = new List<IDev2Activity> {mockActivity.Object};
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspendExecutionActivity = new SuspendExecutionActivity(config)
            {
                SuspendOption = enSuspendOption.SuspendForDays,
                PersistValue = "15",
                AllowManualResumption = true,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = dev2Activities,
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            GetScalarValueFromEnvironment(env, "SuspendID", out string SuspendID, out string error);
            Assert.AreEqual(SuspendID, suspendExecutionActivity.Response);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_SuspendForMonths_SaveSuspendIDIntoEnv_Success()
        {
            //------------Setup for test--------------------------
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentID = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = workflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var dev2Activities = new List<IDev2Activity> {mockActivity.Object};
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspendExecutionActivity = new SuspendExecutionActivity(config)
            {
                SuspendOption = enSuspendOption.SuspendForMonths,
                PersistValue = "15",
                AllowManualResumption = true,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = dev2Activities,
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            GetScalarValueFromEnvironment(env, "SuspendID", out string SuspendID, out string error);
            Assert.AreEqual(SuspendID, suspendExecutionActivity.Response);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_SuspendForMinutes_SaveSuspendIDIntoEnv_Success()
        {
            //------------Setup for test--------------------------
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentID = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = workflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var dev2Activities = new List<IDev2Activity> {mockActivity.Object};
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspendExecutionActivity = new SuspendExecutionActivity(config)
            {
                SuspendOption = enSuspendOption.SuspendForMinutes,
                PersistValue = "15",
                AllowManualResumption = true,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = dev2Activities,
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            GetScalarValueFromEnvironment(env, "SuspendID", out string SuspendID, out string error);
            Assert.AreEqual(SuspendID, suspendExecutionActivity.Response);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_SuspendForSeconds_SaveSuspendIDIntoEnv_Success()
        {
            //------------Setup for test--------------------------
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentID = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = workflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var dev2Activities = new List<IDev2Activity> {mockActivity.Object};
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspendExecutionActivity = new SuspendExecutionActivity(config)
            {
                SuspendOption = enSuspendOption.SuspendForSeconds,
                PersistValue = "20",
                AllowManualResumption = true,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = dev2Activities
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            GetScalarValueFromEnvironment(env, "SuspendID", out string SuspendID, out string error);
            Assert.AreEqual(SuspendID, suspendExecutionActivity.Response);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_SuspendUntil_SaveSuspendIDIntoEnv_Success()
        {
            //------------Setup for test--------------------------
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentID = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = workflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var dev2Activities = new List<IDev2Activity> {mockActivity.Object};
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            var suspendUntil = DateTime.Now.AddDays(1);
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspendExecutionActivity = new SuspendExecutionActivity(config);
            suspendExecutionActivity.SuspendOption = enSuspendOption.SuspendUntil;
            suspendExecutionActivity.PersistValue = suspendUntil.ToString();
            suspendExecutionActivity.AllowManualResumption = true;
            suspendExecutionActivity.SaveDataFunc = activityFunction;
            suspendExecutionActivity.Result = "[[SuspendID]]";
            suspendExecutionActivity.NextNodes = dev2Activities;

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            GetScalarValueFromEnvironment(env, "SuspendID", out string SuspendID, out string error);
            Assert.AreEqual(SuspendID, suspendExecutionActivity.Response);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_NextNodesIsNull_FailWithMessage()
        {
            //------------Setup for test--------------------------
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentID = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = workflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspendUntil = DateTime.Now.AddDays(1);
            var suspendExecutionActivity = new SuspendExecutionActivity(config)
            {
                SuspendOption = enSuspendOption.SuspendUntil,
                PersistValue = suspendUntil.ToString(),
                AllowManualResumption = true,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = null
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            var errors = env.AllErrors.ToList();
            Assert.AreEqual(errors[0], "<InnerError>At least 1 activity is required after Suspend Execution.</InnerError>");
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_PersistenceNotConfigured_FailWithMessage()
        {
            //------------Setup for test--------------------------
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
            var resourceId = Guid.NewGuid();
            var nextNodeId = Guid.NewGuid();
            var workflowInstanceId = Guid.NewGuid();
            var env = CreateExecutionEnvironment();
            env.Assign("[[UUID]]", "public", 0);
            env.Assign("[[JourneyName]]", "whatever", 0);

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            var environmentID = Guid.Empty;
            User = new Mock<IPrincipal>().Object;
            var dataObject = new DsfDataObject(CurrentDl, ExecutionId)
            {
                ServiceName = workflowName,
                ResourceID = resourceId,
                WorkflowInstanceId = workflowInstanceId,
                WebUrl = url,
                ServerID = Guid.NewGuid(),
                ExecutingUser = User,
                IsDebug = true,
                EnvironmentID = environmentID,
                Environment = env,
                IsRemoteInvokeOverridden = false,
                DataList = new StringBuilder(CurrentDl),
                IsServiceTestExecution = true
            };

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var dev2Activities = new List<IDev2Activity> {mockActivity.Object};
            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };
            var suspendUntil = DateTime.Now.AddDays(1);
            var config = new PersistenceSettings
            {
                Enable = false
            };
            var suspendExecutionActivity = new SuspendExecutionActivity(config)
            {
                SuspendOption = enSuspendOption.SuspendUntil,
                PersistValue = suspendUntil.ToString(),
                AllowManualResumption = true,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = dev2Activities
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            suspendExecutionActivity.Execute(dataObject, 0);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            var errors = env.AllErrors.ToList();
            Assert.AreEqual(errors[0], "<InnerError>Could not find persistence config. Please configure in Persistence Settings.</InnerError>");
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
                InputMapping = ActivityStrings.ForEach_Input_Mapping,
                OutputMapping = ActivityStrings.ForEach_Output_Mapping,
            };

            return activity;
        }
    }
}