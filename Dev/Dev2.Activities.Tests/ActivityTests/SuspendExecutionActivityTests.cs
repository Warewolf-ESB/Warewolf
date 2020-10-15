using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.State;
using Dev2.Data.Interfaces.Enums;
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
    public class SuspendExecutionActivityTests
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
            Assert.AreEqual("Data Action", suspendExecutionActivity.DataFunc.DisplayName);
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
                DataFunc = activityFunction,
                Response = "[[result]]"
            };

            Assert.AreEqual("Suspend Execution", suspendExecutionActivity.DisplayName);
            Assert.AreEqual("15", suspendExecutionActivity.PersistValue);
            Assert.AreEqual(enSuspendOption.SuspendForDays, suspendExecutionActivity.SuspendOption);
            Assert.IsTrue(suspendExecutionActivity.AllowManualResumption);
            Assert.AreEqual("TestService", suspendExecutionActivity.DataFunc.DisplayName);

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
        public void SuspendExecutionActivity_Execute_SuspendForDays_Success()
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

            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.ServiceName).Returns(workflowName);
            dataObject.Setup(o => o.ResourceID).Returns(resourceId);
            dataObject.Setup(o => o.WorkflowInstanceId).Returns(workflowInstanceId);
            dataObject.Setup(o => o.WebUrl).Returns(url);
            dataObject.Setup(o => o.Environment).Returns(env);

            var mockActivity = new Mock<IDev2Activity>();
            mockActivity.Setup(o => o.UniqueID).Returns(nextNodeId.ToString());
            var dev2Activities = new List<IDev2Activity> {mockActivity.Object};
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
                DataFunc = activityFunction,
                Response = "[[result]]",
                NextNodes = dev2Activities,
            };

            suspendExecutionActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            var response = suspendExecutionActivity.Execute(dataObject.Object, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(response,suspendExecutionActivity.Response);
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