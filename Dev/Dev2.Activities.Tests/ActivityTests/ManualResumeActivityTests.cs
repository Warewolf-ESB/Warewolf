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
using System.Linq;
using System.Security.Principal;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.State;
using Dev2.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Auditing;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class ManualResumeActivityTests : BaseActivityUnitTest
    {

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumeActivity))]
        public void ManualResumeActivity_Initialize()
        {
            var manualResumeActivity = new ManualResumeActivity();
            Assert.AreEqual("Manual Resume", manualResumeActivity.DisplayName);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumeActivity))]
        public void ManualResumeActivity_Equals_Set_OtherIsEqual_Returns_IsTrue()
        {
            var manualResumeActivity = new ManualResumeActivity();
            var manualResumeActivityOther = manualResumeActivity;
            var equal = manualResumeActivity.Equals(manualResumeActivityOther);
            Assert.IsTrue(equal);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumeActivity))]
        public void ManualResumeActivity_Equals_Set_BothAreObjects_Returns_IsFalse()
        {
            object manualResumeActivity = new ManualResumeActivity();
            var manualResumeActivityOther = new object();
            var equal = manualResumeActivity.Equals(manualResumeActivityOther);
            Assert.IsFalse(equal);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumeActivity))]
        public void ManualResumeActivity_Equals_Set_OtherIsObjectOfManualResumeActivityEqual_Returns_IsFalse()
        {
            var manualResumeActivity = new ManualResumeActivity();
            object manualResumeActivityOther = new ManualResumeActivity();
            var equal = manualResumeActivity.Equals(manualResumeActivityOther);
            Assert.IsFalse(equal);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumeActivity))]
        public void ManualResumeActivity_GetHashCode()
        {
            var manualResumeActivity = new ManualResumeActivity();
            var hashCode = manualResumeActivity.GetHashCode();
            Assert.IsNotNull(hashCode);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ManualResumeActivity))]
        public void ManualResumeActivity_GetState()
        {
            var manualResumeActivity = new ManualResumeActivity
            {
                Response = "[[result]]",
            };
            var stateItems = manualResumeActivity.GetState();

            Assert.AreEqual(1, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "Response",
                    Value = "[[result]]",
                    Type = StateVariable.StateType.Output
                },
            };

            var iter = manualResumeActivity.GetState().Select(
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
        [TestCategory(nameof(ManualResumeActivity))]
        public void ManualResumeActivity_Execute_Success()
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
            var manualResumeActivity = new ManualResumeActivity(config)
            {
                Result = "[[Response]]"
            };

            manualResumeActivity.SetStateNotifier(mockStateNotifier.Object);
            //------------Execute Test---------------------------
            manualResumeActivity.Execute(dataObject, 0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, env.Errors.Count);
            Assert.AreEqual("success", manualResumeActivity.Response);
        }
        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

    }
}