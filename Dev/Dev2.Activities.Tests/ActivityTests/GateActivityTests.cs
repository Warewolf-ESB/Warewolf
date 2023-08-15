/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Activities;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Auditing;
using Warewolf.Data.Options;
using Warewolf.Options;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class GateActivityTests
    {
        static IExecutionEnvironment CreateExecutionEnvironment()
        {
            return new ExecutionEnvironment();
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Construct_Default_Execute_Returns_GateActivity()
        {
            var env = CreateExecutionEnvironment();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.Environment).Returns(env);
            dataObject.Setup(o => o.Settings.EnableDetailedLogging).Returns(true);
            var gatesList = new Dictionary<IDev2Activity, (RetryState, IEnumerator<bool>)>();
            dataObject.Setup(o => o.Gates).Returns(gatesList);

            var activity = new GateActivity();
            activity.Execute(dataObject.Object, 0);
            Assert.AreEqual("Gate", activity.DisplayName);
            Assert.AreEqual(1, gatesList.Count);
            Assert.AreEqual(activity, gatesList.Keys.First());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Equals_Set_OtherIsNull_Returns_IsFalse()
        {
            var gateActivity = new GateActivity();
            var gateActivityEqual = gateActivity.Equals(null);
            Assert.IsFalse(gateActivityEqual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Equals_Set_OtherisEqual_Returns_IsTrue()
        {
            var gateActivity = new GateActivity();
            var gateActivityOther = gateActivity;
            var gateActivityEqual = gateActivity.Equals(gateActivityOther);
            Assert.IsTrue(gateActivityEqual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Equals_Set_BothAreObjects_Returns_IsFalse()
        {
            object gateActivity = new GateActivity();
            var other = new object();
            var gateActivityEqual = gateActivity.Equals(other);
            Assert.IsFalse(gateActivityEqual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Equals_Set_OtherisObjectofGateActivityEqual_Returns_IsFalse()
        {
            var gateActivity = new GateActivity();
            object other = new GateActivity();
            var gateActivityEqual = gateActivity.Equals(other);
            Assert.IsFalse(gateActivityEqual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_GetHashCode()
        {
            var gateActivityActivity = new GateActivity { };
            {
                var hashCode = gateActivityActivity.GetHashCode();
                Assert.IsNotNull(hashCode);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenNoConditions_ExpectDetailedLog()
        {
            var expectedNextActivity = new Mock<IDev2Activity>();

            //---------------Set up test pack-------------------

            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                Conditions = new List<ConditionExpression>(),
                NextNodes = new List<IDev2Activity> { expectedNextActivity.Object },
            };


            var dataObject = new DsfDataObject("", Guid.NewGuid());

            var result = act.Execute(dataObject, 0);

            Assert.AreEqual(expectedNextActivity.Object, result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        public void GateActivity_GetChildrenNodes_Given_Activities_ActivityTools()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var childNode = new GateActivity
            {
                DisplayName = "Gate inside a Gate",
                DataFunc = new ActivityFunc<string, bool>
                {
                    DisplayName = "test inner inner activity",
                    Handler = new DsfSwitch { }
                }
            };

            var activity = new GateActivity()
            {
                UniqueID = uniqueId,
                DataFunc = new ActivityFunc<string, bool>
                {
                    DisplayName = "test display name",
                    Handler = childNode
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(activity.GetChildrenNodes());
            //---------------Execute Test ----------------------
            var nodes = activity.GetChildrenNodes();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual("Gate inside a Gate", childNode.DisplayName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GetDebugInputs()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                Conditions = new List<ConditionExpression>
                {
                    new ConditionExpression { Left = "[[a]]", Cond = new ConditionMatch { MatchType = enDecisionType.IsNull } },
                    new ConditionExpression { Left = "[[a]]", Cond = new ConditionBetween { MatchType = enDecisionType.IsBetween, From = "1", To = "10"} },
                },
            };

            var dataObject = new DsfDataObject("", Guid.NewGuid()) { IsDebug = true };

            var result = act.Execute(dataObject, 0);
            var inputs = act.GetDebugInputs(dataObject.Environment, 0);
            Assert.IsNotNull(inputs);
            Assert.AreEqual(1, inputs.Count);
            Assert.AreEqual("Allow If", inputs[0].ResultsList[0].Label);
            Assert.AreEqual("[[a]] Is NULL\n AND \n[[a]] is greater than 1 and less than 10", inputs[0].ResultsList[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GetDebugOutputs()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                Conditions = new List<ConditionExpression>(),
            };

            var dataObject = new DsfDataObject("", Guid.NewGuid());

            var result = act.Execute(dataObject, 0);
            var outputs = act.GetDebugOutputs(dataObject.Environment, 0);
            Assert.IsNotNull(outputs);
            Assert.AreEqual(0, outputs.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenFailingCondition_ExpectDetailedLog()
        {
            var expectedNextActivity = new Mock<IDev2Activity>();

            //---------------Set up test pack-------------------
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var conditions = new List<ConditionExpression>();
            conditions.Add(condition);

            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                Conditions = conditions,
                NextNodes = new List<IDev2Activity> { expectedNextActivity.Object },
            };

            var dataObject = new DsfDataObject("", Guid.NewGuid());

            var result = act.Execute(dataObject, 0);

            Assert.IsNull(result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenFailingConditionVarNotExistsWithRetry_ExpectRetryGate()
        {
            var expectedNextActivity = new Mock<IDev2Activity>();
            var expectedRetryActivity = new GateActivity();
            expectedRetryActivity.GateOptions.GateOpts = new Continue();

            var expectedRetryActivityId = Guid.NewGuid();
            expectedRetryActivity.UniqueID = expectedRetryActivityId.ToString();

            //---------------Set up test pack-------------------
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var conditions = new List<ConditionExpression>();
            conditions.Add(condition);

            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                RetryEntryPointId = expectedRetryActivityId,
                Conditions = conditions,
                NextNodes = new List<IDev2Activity> { expectedNextActivity.Object },
            };
            expectedRetryActivity.Conditions = conditions;

            var dataObject = new DsfDataObject("", Guid.NewGuid());
            dataObject.Environment.Assign("[[nota]]", "bob", 0);
            dataObject.Gates[expectedRetryActivity] = (new RetryState(), null);

            var result = act.Execute(dataObject, 0);

            Assert.AreEqual(expectedRetryActivity, result, "execution should proceed to RetryEntryPoint if gate fails");

            Assert.AreEqual(1, dataObject.Gates[expectedRetryActivity].Item1.NumberOfRetries);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenFailingConditionVarNotExistsWithRetryInvalidGate_ExpectRetryGateFailure()
        {
            var expectedNextActivity = new Mock<IDev2Activity>();
            var expectedRetryActivity = new GateActivity();
            var expectedRetryActivityId = Guid.NewGuid();
            expectedRetryActivity.UniqueID = expectedRetryActivityId.ToString();
            expectedRetryActivity.GateOptions = new GateOptions { GateOpts = new EndWorkflow() };

            //---------------Set up test pack-------------------
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var conditions = new List<ConditionExpression>();
            conditions.Add(condition);

            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                RetryEntryPointId = expectedRetryActivityId,
                Conditions = conditions,
                NextNodes = new List<IDev2Activity> { expectedNextActivity.Object },
            };
            expectedRetryActivity.Conditions = conditions;

            var dataObject = new DsfDataObject("", Guid.NewGuid());
            dataObject.Environment.Assign("[[nota]]", "bob", 0);
            dataObject.Gates[expectedRetryActivity] = (new RetryState(), null);

            var result = act.Execute(dataObject, 0);

            Assert.IsTrue(dataObject.Environment.HasErrors());
            var errors = dataObject.Environment.Errors.ToArray();
            Assert.AreEqual(2, errors.Length);
            Assert.AreEqual("gate conditions failed", errors[0]);
            Assert.AreEqual("cannot update retry state of a non-resumable gate", errors[1]);

            Assert.IsNull(result, "execution should not proceed to RetryEntryPoint if entrypoint is not resumable");

            Assert.AreEqual(0, dataObject.Gates[expectedRetryActivity].Item1.NumberOfRetries);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenFailingConditionWithStopOnError_ExpectRetryGate()
        {
            var expectedNextActivity = new Mock<IDev2Activity>();

            //---------------Set up test pack-------------------
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var conditions = new List<ConditionExpression>();
            conditions.Add(condition);

            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                Conditions = conditions,
                NextNodes = new List<IDev2Activity> { expectedNextActivity.Object },
            };

            var dataObject = new DsfDataObject("", Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "notbob", 0);

            var result = act.Execute(dataObject, 0);

            Assert.IsNull(result, "execution should stop if gate fails and StopOnError is set");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenFailingConditionWithRetry_ExpectRetryGateAsNext()
        {
            var expectedNextActivity = new Mock<IDev2Activity>();
            var expectedRetryActivity = new GateActivity();
            var expectedRetryActivityId = Guid.NewGuid();
            expectedRetryActivity.UniqueID = expectedRetryActivityId.ToString();
            expectedRetryActivity.GateOptions.GateOpts = new Continue();

            //---------------Set up test pack-------------------
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var conditions = new List<ConditionExpression>();
            conditions.Add(condition);

            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                RetryEntryPointId = expectedRetryActivityId,
                Conditions = conditions,
                NextNodes = new List<IDev2Activity> { expectedNextActivity.Object },
            };
            expectedRetryActivity.Conditions = conditions;

            var dataObject = new DsfDataObject("", Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "notbob", 0);
            dataObject.Gates[expectedRetryActivity] = (new RetryState(), null);

            var result = act.Execute(dataObject, 0);

            Assert.AreNotEqual(expectedNextActivity.Object, result, "execution should not proceed as normal if gate fails and StopOnError is set");

            Assert.AreEqual(1, dataObject.Gates[expectedRetryActivity].Item1.NumberOfRetries);
            Assert.AreEqual(expectedRetryActivity, result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenPassingConditions_ExpectDetailedLog()
        {
            var expectedNextActivity = new Mock<IDev2Activity>();
            var expectedRetryActivity = new GateActivity();
            var expectedRetryActivityId = Guid.NewGuid();
            expectedRetryActivity.UniqueID = expectedRetryActivityId.ToString();

            //---------------Set up test pack-------------------
            var condition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var conditions = new List<ConditionExpression>();
            conditions.Add(condition);

            //------------Setup for test--------------------------
            var act = new GateActivity
            {
                Conditions = conditions,
                RetryEntryPointId = expectedRetryActivityId,
                NextNodes = new List<IDev2Activity> { expectedNextActivity.Object },
            };
            expectedRetryActivity.Conditions = conditions;

            var dataObject = new DsfDataObject("", Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "bob", 0);
            dataObject.Gates[expectedRetryActivity] = (new RetryState(), null);

            var result = act.Execute(dataObject, 0);

            Assert.AreNotEqual(expectedRetryActivity, result, "execution should proceed as normal if gate passes and Retry is set");

            Assert.AreEqual(0, dataObject.Gates[expectedRetryActivity].Item1.NumberOfRetries, "number of retries should not change if gate passes");
            Assert.AreEqual(expectedNextActivity.Object, result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenPassingConditionsOnFirstGateAndPassingSecondGate_ExpectThirdNode()
        {
            var firstGateId = Guid.NewGuid();
            var secondGateId = Guid.NewGuid();
            var failingCondition = new ConditionExpression
            {
                Left = "[[somebob]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "another bob" }
            };
            var failingConditions = new List<ConditionExpression>();
            failingConditions.Add(failingCondition);

            var thirdNode = new Mock<IDev2Activity>().Object;
            var secondGate = new GateActivity
            {
                UniqueID = secondGateId.ToString(),
                RetryEntryPointId = firstGateId,
                Conditions = failingConditions,
                NextNodes = new List<IDev2Activity> { thirdNode },
            };

            //---------------Set up test pack-------------------
            var passingCondition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var passingConditions = new List<ConditionExpression>();
            passingConditions.Add(passingCondition);

            //------------Setup for test--------------------------
            var firstGate = new GateActivity
            {
                UniqueID = firstGateId.ToString(),
                Conditions = passingConditions,
                NextNodes = new List<IDev2Activity> { secondGate },
            };

            var dataObject = new DsfDataObject("", Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "bob", 0);

            var result = firstGate.Execute(dataObject, 0);

            Assert.AreEqual(secondGate, result);

            dataObject.Environment.Assign("[[somebob]]", "another bob", 0);
            result = result.Execute(dataObject, 0);

            Assert.AreEqual(thirdNode, result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenPassingConditionsOnFirstGateAndFailingSecondGate_ExpectDetailedLog()
        {
            var firstGateId = Guid.NewGuid();
            var secondGateId = Guid.NewGuid();

            var failingCondition = new ConditionExpression
            {
                Left = "[[somebob]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "notbob" }
            };
            var failingConditions = new List<ConditionExpression>
            {
                failingCondition
            };

            var thirdNode = new Mock<IDev2Activity>().Object;
            var secondGate = new GateActivity
            {
                UniqueID = secondGateId.ToString(),
                RetryEntryPointId = firstGateId,
                Conditions = failingConditions,
                NextNodes = new List<IDev2Activity> { thirdNode },
            };

            //---------------Set up test pack-------------------
            var passingCondition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var passingConditions = new List<ConditionExpression>
            {
                passingCondition
            };

            //------------Setup for test--------------------------
            var firstGate = new GateActivity
            {
                UniqueID = firstGateId.ToString(),
                Conditions = passingConditions,
                NextNodes = new List<IDev2Activity> { secondGate },
                GateOptions = new GateOptions()
                {
                    GateOpts = new Continue()
                }
            };

            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            firstGate.SetStateNotifier(mockStateNotifier.Object);

            var dataObject = new DsfDataObject("", Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "bob", 0);

            var result = firstGate.Execute(dataObject, 0);
            dataObject.Environment.Assign("[[a]]", "ralph", 0);  
            
            Assert.AreEqual("ralph", dataObject.Environment.EvalAsListOfStrings("[[a]]", 0)[0]);
            Assert.AreEqual(secondGate, result);

            result = result.Execute(dataObject, 0);

            Assert.AreEqual(firstGate, result);

            result = result.Execute(dataObject, 0);

            Assert.AreEqual("ralph", dataObject.Environment.EvalAsListOfStrings("[[a]]", 0)[0], "should use the same environment to keep up with each execution enviroment variable changes");
            Assert.AreEqual(secondGate, result);

            dataObject.Environment.Assign("[[somebob]]", "notbob", 0);

            result = result.Execute(dataObject, 0);

            Assert.AreEqual(thirdNode, result);

            mockStateNotifier.Verify(o => o.LogActivityExecuteState(It.IsAny<IDev2Activity>()), Times.Exactly(2));
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Yogesh Rajpurohit")]
        [TestCategory(nameof(GateActivity))]
        public void GateActivity_Execute_GivenPassingConditionsOnFirstGateAndFailingSecondGate_DebugOutput()
        {
            var firstGateId = Guid.NewGuid();
            var secondGateId = Guid.NewGuid();

            var failingCondition = new ConditionExpression
            {
                Left = "[[somebob]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "notbob" }
            };
            var failingConditions = new List<ConditionExpression>
            {
                failingCondition
            };

            var thirdNode = new Mock<IDev2Activity>().Object;
            var secondGate = new GateActivity
            {
                DisplayName = "Gate2",
                UniqueID = secondGateId.ToString(),
                RetryEntryPointId = firstGateId,
                Conditions = failingConditions,
                NextNodes = new List<IDev2Activity> { thirdNode },
            };

            //---------------Set up test pack-------------------
            var passingCondition = new ConditionExpression
            {
                Left = "[[a]]",
                Cond = new ConditionMatch { MatchType = enDecisionType.IsEqual, Right = "bob" }
            };
            var passingConditions = new List<ConditionExpression>
            {
                passingCondition
            };

            //------------Setup for test--------------------------
            var firstGate = new GateActivity
            {
                DisplayName = "Gate1",
                UniqueID = firstGateId.ToString(),
                Conditions = passingConditions,
                DataFunc = new ActivityFunc<string, bool> { Handler = new DsfDotNetMultiAssignActivity { FieldsCollection = new List<ActivityDTO> { new ActivityDTO { FieldName = "InnerNode" , FieldValue = "InnderValue"  } } } },
                NextNodes = new List<IDev2Activity> { secondGate },
                GateOptions = new GateOptions()
                {
                    GateOpts = new Continue()
                }
            };
            var mockStateNotifier = new Mock<IStateNotifier>();
            mockStateNotifier.Setup(stateNotifier => stateNotifier.LogActivityExecuteState(It.IsAny<IDev2Activity>()));

            firstGate.SetStateNotifier(mockStateNotifier.Object);             

            var env = CreateExecutionEnvironment();
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.IsDebugMode()).Returns(true);
            dataObject.Setup(o => o.Environment).Returns(env);
            dataObject.Setup(o => o.Settings.EnableDetailedLogging).Returns(true);

            dataObject.Object.Environment.Assign("[[a]]", "bob", 0);

            var gatesList = new Dictionary<IDev2Activity, (RetryState, IEnumerator<bool>)>();
            dataObject.Setup(o => o.Gates).Returns(gatesList);

            //Executing First Gate
            var result = firstGate.Execute(dataObject.Object, 0);
            Assert.AreEqual("Gate2", result.GetDisplayName());
            Assert.AreEqual(1, gatesList.Count);          
            var outputFirst = firstGate.GetDebugOutputs(dataObject.Object.Environment, 0);
            Assert.AreEqual("Conditions passed", outputFirst[0].ResultsList[0].Value);
            Assert.AreEqual("Executing", outputFirst[1].ResultsList[0].Value);

            //Executing Second Gate with failed condition
            result = result.Execute(dataObject.Object, 0);
            Assert.AreEqual("Gate1", result.GetDisplayName());
            Assert.AreEqual(2, gatesList.Count);            
            var outputSecond = secondGate.GetDebugOutputs(dataObject.Object.Environment, 0);
            Assert.AreEqual("Conditions failed", outputSecond[0].ResultsList[0].Value);
            Assert.AreEqual("Retry gate", outputSecond[1].ResultsList[0].Value);

            //Check Inner Nodes
            var innerNodes = result.GetChildrenNodes();
            Assert.AreEqual(1, innerNodes.Count());
            Assert.AreEqual("Assign", innerNodes.First().GetDisplayName());



            //Executing First Gate again.
            result = result.Execute(dataObject.Object, 0);
            Assert.AreEqual("Gate2", result.GetDisplayName());
            Assert.AreEqual(2, gatesList.Count);            
            var outputThird = firstGate.GetDebugOutputs(dataObject.Object.Environment, 0);
            Assert.AreEqual("Retry: 1", outputThird[0].ResultsList[0].Value);
            Assert.AreEqual("ExecuteRetry", outputThird[1].ResultsList[0].Value);

            //Executing Second Gate again.
            dataObject.Object.Environment.Assign("[[somebob]]", "notbob", 0);
            result = result.Execute(dataObject.Object, 0);        
        }
    }
}
