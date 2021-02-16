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
using System.Collections.Generic;
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
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;
using Warewolf.Auditing;
using Warewolf.Driver.Persistence;
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
        public void ManualResumptionActivity_Equals_Set_OtherIsObjectOfmanualResumptionActivityEqual_Returns_IsFalse()
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

            foreach (var entry in iter)
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";
            var overrideInputVariables = false;

            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, suspensionId, overrideInputVariables, "")).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = overrideInputVariables,
                SuspensionId = suspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            mockResumeJob.Verify(o => o.ResumeJob(dataObject, suspensionId, overrideInputVariables, ""), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_OverrideInputVariables_True_Failed_InnerActivityHasNoContent()
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";
            var overrideInputVariables = true;
            var environment = env.ToJson();
            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, suspensionId, overrideInputVariables, environment)).Verifiable();
            mockResumeJob.Setup(o => o.GetSuspendedEnvironment(suspensionId)).Returns("Anything").Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = overrideInputVariables,
                SuspensionId = suspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataObject.Environment.AllErrors.Count);
            var errors = dataObject.Environment.AllErrors.ToList();
            Assert.AreEqual(ErrorResource.InnerActivityWithNoContentError, manualResumptionActivity.Response);
            Assert.AreEqual(errors[0], "<InnerError>" + ErrorResource.InnerActivityWithNoContentError + "</InnerError>");
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";
            var overrideInputVariables = true;
            var environment = env.ToJson();
            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, suspensionId, overrideInputVariables, environment)).Verifiable();
            mockResumeJob.Setup(o => o.GetSuspendedEnvironment(suspensionId)).Returns("Anything").Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = overrideInputVariables,
                SuspensionId = suspensionId,
                OverrideDataFunc = activityFunction,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataObject.Environment.AllErrors.Count);
            mockResumeJob.Verify(o => o.ResumeJob(dataObject, suspensionId, overrideInputVariables, environment), Times.Once);
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
            var workflowName = "workflowName";
            var url = "http://localhost:3142/secure/WorkflowResume";
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";
            var overrideInputVariables = true;
            var overrideEnvironment = env.ToJson();
            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.ResumeJob(dataObject, suspensionId, overrideInputVariables, overrideEnvironment)).Verifiable();
            mockResumeJob.Setup(o => o.GetSuspendedEnvironment(suspensionId)).Returns("Anything").Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = overrideInputVariables,
                SuspensionId = suspensionId,
                OverrideDataFunc = activityFunction,
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataObject.Environment.AllErrors.Count);
            mockResumeJob.Verify(o => o.ResumeJob(dataObject, suspensionId, overrideInputVariables, overrideEnvironment), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_SuspensionID_IsNullOrWhiteSpace_FailWithMessage()
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var manualResumptionActivity = new ManualResumptionActivity(config, new Mock<IPersistenceExecution>().Object)
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
            Assert.AreEqual(ErrorResource.ManualResumptionSuspensionIdBlank, manualResumptionActivity.Response);
            Assert.AreEqual(errors[0], "<InnerError>" + ErrorResource.ManualResumptionSuspensionIdBlank + "</InnerError>");
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

            var manualResumptionActivity = new ManualResumptionActivity(config, new Mock<IPersistenceExecution>().Object)
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
            Assert.AreEqual(ErrorResource.PersistenceSettingsNoConfigured, manualResumptionActivity.Response);
            Assert.AreEqual(errors[0], "<InnerError>" + ErrorResource.PersistenceSettingsNoConfigured + "</InnerError>");
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
        public void ManualResumptionActivity_ExecuteOverrideDataFunc_Override_Variables()
        {
            //---------------Set up test pack-------------------

            var activity = CreateWorkflow();
            var activityFunction = new ActivityFunc<string, bool>
            {
                DisplayName = activity.DisplayName,
                Handler = activity,
            };

            const string dataList = "{\"Environment\":{\"scalars\":{\"a\":1,\"b\":2,\"c\":3},\"record_sets\":{\"rec\":{\"WarewolfPositionColumn\":[1],\"d\":[\"4\"],\"e\":[\"5\"],\"f\":[\"6\"]}},\"json_objects\":{\"obj\":{\"x\":{\"y\":{\"z\":1}}}}},\"Errors\":[],\"AllErrors\":[]}";

            var manualResumptionActivity = new ManualResumptionActivity()
            {
                SuspensionId = "15",
                OverrideInputVariables = true,
                OverrideDataFunc = activityFunction,
                Response = "[[result]]"
            };

            var mockDev2Activity = new Mock<IDev2ActivityIOMapping>();
            mockDev2Activity.Setup(o => o.InputMapping).Returns("<Inputs><Input Name=\"a\" Source=\"[[a]]\" IsObject=\"False\" /><Input Name=\"b\" Source=\"[[b]]\" IsObject=\"False\" /><Input Name=\"c\" Source=\"[[c]]\" IsObject=\"False\" /><Input Name=\"d\" Source=\"[[rec(*).d]]\" IsObject=\"False\" Recordset=\"rec\" /><Input Name=\"e\" Source=\"[[rec(*).e]]\" IsObject=\"False\" Recordset=\"rec\" /><Input Name=\"f\" Source=\"[[rec(*).f]]\" IsObject=\"False\" Recordset=\"rec\" /><Input Name=\"@obj\" Source=\"[[@obj]]\" IsObject=\"True\" /></Inputs>");
            var forEachInnerActivityTo = new ForEachInnerActivityTO(mockDev2Activity.Object);

            var nameCount = 0;
            var valueCount = 0;
            var expectedNameList = new List<string>
            {
                "[[a]]",
                "[[b]]",
                "[[c]]",
                "[[rec(1).d]]",
                "[[rec(1).e]]",
                "[[rec(1).f]]",
                "[[@obj]]",
            };
            var expectedValueList = new List<string>
            {
                "100",
                "50",
                "3",
                "200",
                "5",
                "6",
                "{\"l\":{\"m\":{\"o\":4}}}",
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(o => o.EvalToExpression(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(() =>
                {
                    var expectedName = expectedNameList[nameCount];
                    nameCount++;
                    return expectedName;
                });
            mockExecutionEnvironment.Setup(o => o.Eval(It.IsAny<string>(), It.IsAny<int>(), false, true))
                .Returns(() =>
                {
                    var expectedValue = expectedValueList[valueCount];
                    valueCount++;
                    return CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString(expectedValue));
                });

            var mockDataObject = new Mock<IDSFDataObject>();
            mockDataObject.Setup(o => o.Environment).Returns(mockExecutionEnvironment.Object);

            var environmentToJson = manualResumptionActivity.ExecuteOverrideDataFunc(forEachInnerActivityTo, dataList, mockDataObject.Object);

            Assert.AreEqual("{\"Environment\":{\"scalars\":{\"a\":100,\"b\":50,\"c\":3},\"record_sets\":{\"rec\":{\"WarewolfPositionColumn\":[1],\"d\":[200],\"e\":[5],\"f\":[6]}},\"json_objects\":{\"obj\":{\"l\":{\"m\":{\"o\":4}}}}},\"Errors\":[],\"AllErrors\":[]}", environmentToJson);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_GetSuspendedEnvironment_Error_ManualResumptionSuspensionEnvBlank()
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";

            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.GetSuspendedEnvironment(suspensionId)).Returns(string.Empty).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = true,
                SuspensionId = suspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, env.AllErrors.Count);
            Assert.AreEqual("<InnerError>" + ErrorResource.ManualResumptionSuspensionEnvBlank + "</InnerError>", env.AllErrors.First());
            Assert.AreEqual(ErrorResource.ManualResumptionSuspensionEnvBlank, manualResumptionActivity.Response);
            mockResumeJob.Verify(o => o.GetSuspendedEnvironment(suspensionId), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_GetSuspendedEnvironment_Error_ManualResumptionAlreadyResumed()
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";
            var errMsg = "Failed: " + ErrorResource.ManualResumptionAlreadyResumed;
            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.GetSuspendedEnvironment(suspensionId)).Returns(errMsg).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = true,
                SuspensionId = suspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, env.AllErrors.Count);
            Assert.AreEqual("<InnerError>" + errMsg + "</InnerError>", env.AllErrors.First());
            Assert.AreEqual(errMsg, manualResumptionActivity.Response);
            mockResumeJob.Verify(o => o.GetSuspendedEnvironment(suspensionId), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_GetSuspendedEnvironment_Error_Enqueued()
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";
            var errMsg = "Failed: " + ErrorResource.ManualResumptionEnqueued;
            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.GetSuspendedEnvironment(suspensionId)).Returns(errMsg).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = true,
                SuspensionId = suspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, env.AllErrors.Count);
            Assert.AreEqual("<InnerError>" + errMsg + "</InnerError>", env.AllErrors.First());
            Assert.AreEqual(errMsg, manualResumptionActivity.Response);
            mockResumeJob.Verify(o => o.GetSuspendedEnvironment(suspensionId), Times.Once);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumptionActivity))]
        public void ManualResumptionActivity_Execute_GetSuspendedEnvironment_Error_Processing()
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
            var config = new PersistenceSettings
            {
                Enable = true
            };
            var suspensionId = "321";
            var errMsg = "Failed: " + ErrorResource.ManualResumptionProcessing;
            var mockResumeJob = new Mock<IPersistenceExecution>();
            mockResumeJob.Setup(o => o.GetSuspendedEnvironment(suspensionId)).Returns(errMsg).Verifiable();
            var manualResumptionActivity = new ManualResumptionActivity(config, mockResumeJob.Object)
            {
                Response = "[[result]]",
                OverrideInputVariables = true,
                SuspensionId = suspensionId
            };

            manualResumptionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumptionActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, env.AllErrors.Count);
            Assert.AreEqual("<InnerError>" + errMsg + "</InnerError>", env.AllErrors.First());
            Assert.AreEqual(errMsg, manualResumptionActivity.Response);
            mockResumeJob.Verify(o => o.GetSuspendedEnvironment(suspensionId), Times.Once);
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