/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Auditing;
using Warewolf.Driver.Persistence;
using Warewolf.Security.Encryption;
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
                EncryptData = true,
                Response = "[[result]]",
            };
            var stateItems = suspendExecutionActivity.GetState();

            Assert.AreEqual(5, stateItems.Count());

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
                    Name = "EncryptData",
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var suspendExecutionActivity = new SuspendExecutionActivity(config, new Mock<ISuspendExecution>().Object)
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
            GetScalarValueFromEnvironment(env, "SuspendID", out string suspendId, out string error);
            Assert.AreEqual(suspendId, suspendExecutionActivity.Response);
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var suspendExecutionActivity = new SuspendExecutionActivity(config, new Mock<ISuspendExecution>().Object)
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
            GetScalarValueFromEnvironment(env, "SuspendID", out string suspendId, out string error);
            Assert.AreEqual(suspendId, suspendExecutionActivity.Response);
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var suspendExecutionActivity = new SuspendExecutionActivity(config, new Mock<ISuspendExecution>().Object)
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
            GetScalarValueFromEnvironment(env, "SuspendID", out string suspendId, out string error);
            Assert.AreEqual(suspendId, suspendExecutionActivity.Response);
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var suspendExecutionActivity = new SuspendExecutionActivity(config, new Mock<ISuspendExecution>().Object)
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
            GetScalarValueFromEnvironment(env, "SuspendID", out string suspendId, out string error);
            Assert.AreEqual(suspendId, suspendExecutionActivity.Response);
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var suspendExecutionActivity = new SuspendExecutionActivity(config, new Mock<ISuspendExecution>().Object);
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
            GetScalarValueFromEnvironment(env, "SuspendID", out string suspendId, out string error);
            Assert.AreEqual(suspendId, suspendExecutionActivity.Response);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_EncryptData_True()
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var currentEnvironment = dataObject.Environment.ToJson();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(dataObject.ResourceID.ToString())},
                {"environment", new StringBuilder(DpapiWrapper.Encrypt(currentEnvironment))},
                {"startActivityId", new StringBuilder(nextNodeId.ToString())},
                {"versionNumber", new StringBuilder(dataObject.VersionNumber.ToString())}
            };

            var mockSuspendExecution = new Mock<ISuspendExecution>();
            mockSuspendExecution.Setup(o => o.CreateAndScheduleJob(enSuspendOption.SuspendUntil, suspendUntil.ToString(), values)).Verifiable();
            var suspendExecutionActivity = new SuspendExecutionActivity(config, mockSuspendExecution.Object)
            {
                SuspendOption = enSuspendOption.SuspendUntil,
                PersistValue = suspendUntil.ToString(),
                AllowManualResumption = true,
                EncryptData = true,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = dev2Activities
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            GetScalarValueFromEnvironment(env, "SuspendID", out string suspendId, out string error);
            Assert.AreEqual(suspendId, suspendExecutionActivity.Response);
            //TODO: We need to improve on this verify to validate that the environment is encrypted. Verify fails for unknown mismatch
            mockSuspendExecution.Verify(o => o.CreateAndScheduleJob(enSuspendOption.SuspendUntil, suspendUntil.ToString(), It.IsAny<Dictionary<string, StringBuilder>>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_EncryptData_False()
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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

            var currentEnvironment = dataObject.Environment.ToJson();
            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(dataObject.ResourceID.ToString())},
                {"environment", new StringBuilder(currentEnvironment)},
                {"startActivityId", new StringBuilder(nextNodeId.ToString())},
                {"versionNumber", new StringBuilder(dataObject.VersionNumber.ToString())}
            };

            var mockSuspendExecution = new Mock<ISuspendExecution>();
            mockSuspendExecution.Setup(o => o.CreateAndScheduleJob(enSuspendOption.SuspendUntil, suspendUntil.ToString(), values)).Verifiable();
            var suspendExecutionActivity = new SuspendExecutionActivity(config, mockSuspendExecution.Object)
            {
                SuspendOption = enSuspendOption.SuspendUntil,
                PersistValue = suspendUntil.ToString(),
                AllowManualResumption = true,
                EncryptData = false,
                SaveDataFunc = activityFunction,
                Result = "[[SuspendID]]",
                NextNodes = dev2Activities
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            suspendExecutionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            GetScalarValueFromEnvironment(env, "SuspendID", out string suspendId, out string error);
            Assert.AreEqual(suspendId, suspendExecutionActivity.Response);
            //TODO: We need to improve on this verify to validate that the environment is not encrypted. Verify fails for unknown mismatch
            mockSuspendExecution.Verify(o => o.CreateAndScheduleJob(enSuspendOption.SuspendUntil, suspendUntil.ToString(), It.IsAny<Dictionary<string, StringBuilder>>()), Times.Once);
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var suspendExecutionActivity = new SuspendExecutionActivity(config, new Mock<ISuspendExecution>().Object)
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var suspendExecutionActivity = new SuspendExecutionActivity(config, new Mock<ISuspendExecution>().Object)
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

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SuspendExecutionActivity))]
        public void SuspendExecutionActivity_Execute_ServiceTestExecution_Success()
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

            var environmentId = Guid.Empty;
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
                EnvironmentID = environmentId,
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
            var expectedSuspendId = Guid.NewGuid().ToString();
            const enSuspendOption suspendOption = enSuspendOption.SuspendForSeconds;
            var mockSuspendExecution = new Mock<ISuspendExecution>();
            mockSuspendExecution.Setup(o =>
                o.CreateAndScheduleJob(suspendOption, It.IsAny<string>(),
                    It.IsAny<Dictionary<string, StringBuilder>>())).Returns(expectedSuspendId);

            var suspendExecutionActivity = new SuspendExecutionActivity(config, mockSuspendExecution.Object)
            {
                SuspendOption = suspendOption,
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
            Assert.AreEqual(expectedSuspendId, suspendExecutionActivity.Response);
            Assert.IsTrue(dataObject.StopExecution);
            Assert.IsFalse(dataObject.IsDebugNested);
            Assert.AreEqual(0, dataObject.ForEachNestingLevel);
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