using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.WF;
using Dev2.Common.X6;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class WorkflowToX6ConverterTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_ProcessFlowSwitch")]
		public void WorkflowToX6Converter_ProcessFlowSwitch_ShouldCreateSwitchNode()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[variable]]\",AmbientDataList)"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity
            };

            flowSwitch.Cases.Add("Case1", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Cases.Add("Case2", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Default = new FlowStep { Action = new DsfMultiAssignActivity() };

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            // Should have start node + switch node + action nodes
            Assert.IsTrue(deserializedResult.Nodes.Count >= 2);
            
            // Check for switch node
            var switchNode = deserializedResult.Nodes.Find(n => n.data.ContainsKey(Constants.TYPE) && 
                                                               n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            Assert.IsNotNull(switchNode);
            Assert.AreEqual(Constants.POLYGON, switchNode.shape);
            // The switch node label follows the activity's DisplayName (see ShouldExtractSwitchVariable).
            Assert.AreEqual("Test Switch", switchNode.label);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_ProcessFlowSwitch")]
        public void WorkflowToX6Converter_ProcessFlowSwitch_WithNullExpression_ShouldHandleGracefully()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitch = new FlowSwitch<string>
            {
                Expression = null
            };

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            Assert.IsTrue(deserializedResult.Nodes.Count >= 1); // At least start node
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_ProcessFlowSwitch")]
        public void WorkflowToX6Converter_ProcessFlowSwitch_ShouldCreateCorrectEdges()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[variable]]\",AmbientDataList)"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity
            };

            var case1Action = new DsfMultiAssignActivity();
            var case2Action = new DsfMultiAssignActivity(); 
            var defaultAction = new DsfMultiAssignActivity();

            flowSwitch.Cases.Add("Case1", new FlowStep { Action = case1Action });
            flowSwitch.Cases.Add("Case2", new FlowStep { Action = case2Action });
            flowSwitch.Default = new FlowStep { Action = defaultAction };

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            // Should have edges for each case and default
            Assert.IsTrue(deserializedResult.Edges.Count >= 3);
            
            // Check for labeled edges
            var case1Edge = deserializedResult.Edges.Find(e => e.label == "Case1");
            var case2Edge = deserializedResult.Edges.Find(e => e.label == "Case2");
            var defaultEdge = deserializedResult.Edges.Find(e => e.label == "Default");
            
            Assert.IsNotNull(case1Edge);
            Assert.IsNotNull(case2Edge);
            Assert.IsNotNull(defaultEdge);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_ProcessFlowSwitch")]
        public void WorkflowToX6Converter_ProcessFlowSwitch_ShouldExtractSwitchVariable()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[myVariable]]\",AmbientDataList)",
                DisplayName = "My Test Switch"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity
            };

            flowSwitch.Cases.Add("Value1", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Default = new FlowStep { Action = new DsfMultiAssignActivity() };

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            var switchNode = deserializedResult.Nodes.Find(n => n.data.ContainsKey(Constants.TYPE) && 
                                                               n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            Assert.IsNotNull(switchNode);
            
            // Should have the display name
            Assert.AreEqual("My Test Switch", switchNode.label);
            Assert.AreEqual("My Test Switch", switchNode.data[Constants.DISPLAYNAME]);
            
            // Should have switch expression data
            Assert.IsTrue(switchNode.data.ContainsKey("switchExpression"));
            var switchExpressionJson = switchNode.data["switchExpression"].ToString();
            var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExpressionJson);
            Assert.AreEqual("myVariable", switchExpression.SwitchVariable.ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_ProcessFlowSwitch")]
		public void WorkflowToX6Converter_ProcessFlowSwitch_ShouldHandleEmptyExpressionText()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "", // Empty expression
                DisplayName = "Empty Expression Switch"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity
            };

            flowSwitch.Cases.Add("Value1", new FlowStep { Action = new DsfMultiAssignActivity() });

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            var switchNode = deserializedResult.Nodes.Find(n => n.data.ContainsKey(Constants.TYPE) && 
                                                               n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            Assert.IsNotNull(switchNode);
            
            // Should handle empty expression gracefully
            Assert.AreEqual("Empty Expression Switch", switchNode.label);
            
            // Should have fallback switch expression
            Assert.IsTrue(switchNode.data.ContainsKey("switchExpression"));
            var switchExpressionJson = switchNode.data["switchExpression"].ToString();
            var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExpressionJson);
            Assert.AreEqual("variable", switchExpression.SwitchVariable.ToString()); // Fallback value
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_ProcessFlowSwitch")]
        public void WorkflowToX6Converter_ProcessFlowSwitch_ShouldIncludeCasesInSwitchExpression()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[status]]\",AmbientDataList)"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity
            };

            flowSwitch.Cases.Add("Active", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Cases.Add("Inactive", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Cases.Add("Pending", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Default = new FlowStep { Action = new DsfMultiAssignActivity() };

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            var switchNode = deserializedResult.Nodes.Find(n => n.data.ContainsKey(Constants.TYPE) && 
                                                               n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            Assert.IsNotNull(switchNode);
            
            // Should have switch expression with cases
            Assert.IsTrue(switchNode.data.ContainsKey("switchExpression"));
            var switchExpressionJson = switchNode.data["switchExpression"].ToString();
            var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExpressionJson);
            
            Assert.AreEqual("status", switchExpression.SwitchVariable.ToString());
            Assert.AreEqual("Default", switchExpression.DefaultCase.ToString());
            
            // Should have 3 cases
            var cases = switchExpression.Cases;
            Assert.AreEqual(3, cases.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_ProcessFlowSwitch")]
		public void WorkflowToX6Converter_ProcessFlowSwitch_ShouldHandleNullDefaultCase()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Test Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[variable]]\",AmbientDataList)"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity,
                Default = null // No default case
            };

            flowSwitch.Cases.Add("Case1", new FlowStep { Action = new DsfMultiAssignActivity() });

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            var switchNode = deserializedResult.Nodes.Find(n => n.data.ContainsKey(Constants.TYPE) && 
                                                               n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            Assert.IsNotNull(switchNode);
            
            // Should handle null default case
            Assert.IsTrue(switchNode.data.ContainsKey("switchExpression"));
            var switchExpressionJson = switchNode.data["switchExpression"].ToString();
            var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExpressionJson);
            
            // DefaultCase should be null when no default is provided. Deserialized as dynamic,
            // a JSON null surfaces as a JValue of type Null rather than a CLR null reference.
            var defaultCase = switchExpression.DefaultCase;
            Assert.IsTrue(defaultCase == null || defaultCase.Type == Newtonsoft.Json.Linq.JTokenType.Null);
        }

        // ─────────────────────────────────────────────────────────────────
        // Legacy activity type identity (round-trip fidelity)
        //
        // Each of these activities has a "DotNet" sibling class, and each used to serialize itself
        // using its sibling's type constant. Because X6ToWorkflowConverter dispatches on that
        // constant, a round trip silently rebuilt the activity as the DotNet class — a different
        // implementation with different behaviour. For DsfDateTimeActivity that surfaced as the
        // round-tripped workflow failing with "Could not parse input datetime with given input
        // format" while the original succeeded.
        // ─────────────────────────────────────────────────────────────────

        static string TypeOf(Action<Cell> serialise)
        {
            var cell = new Cell { id = Guid.NewGuid().ToString(), data = new Dictionary<string, object>() };
            serialise(cell);
            return cell.data[Constants.TYPE] as string;
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_LegacyTypeIdentity")]
        public void ToX6Json_LegacyDateTimeActivity_EmitsItsOwnTypeNotTheDotNetSibling()
        {
            var emitted = TypeOf(cell => new DsfDateTimeActivity { DisplayName = "Date and Time" }.ToX6Json(cell));

            Assert.AreEqual(Constants.DSFDATETIMEACTIVITY.ToLower(), emitted,
                "A legacy DsfDateTimeActivity must round-trip back as itself, not as DsfDotNetDateTimeActivity.");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_LegacyTypeIdentity")]
        public void ToX6Json_LegacyDateTimeDifferenceActivity_EmitsItsOwnType()
        {
            var emitted = TypeOf(cell => new DsfDateTimeDifferenceActivity { DisplayName = "Date and Time Difference" }.ToX6Json(cell));

            Assert.AreEqual(Constants.DSFDATETIMEDIFFERENCEACTIVITY.ToLower(), emitted);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_LegacyTypeIdentity")]
        public void ToX6Json_LegacyAggregateCalculateActivity_EmitsItsOwnType()
        {
            var emitted = TypeOf(cell => new DsfAggregateCalculateActivity { DisplayName = "Aggregate Calculate" }.ToX6Json(cell));

            Assert.AreEqual(Constants.DSFAGGREGATECALCULATEACTIVITY.ToLower(), emitted);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_LegacyTypeIdentity")]
        public void ToX6Json_LegacyCalculateActivity_EmitsItsOwnType()
        {
            var emitted = TypeOf(cell => new DsfCalculateActivity { DisplayName = "Calculate" }.ToX6Json(cell));

            Assert.AreEqual(Constants.DSFCALCULATEACTIVITY.ToLower(), emitted,
                "A legacy DsfCalculateActivity must round-trip back as itself, not as DsfDotNetCalculateActivity.");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_LegacyTypeIdentity")]
        public void ToX6Json_LegacyGatherSystemInformationActivity_EmitsItsOwnType()
        {
            var emitted = TypeOf(cell => new DsfGatherSystemInformationActivity { DisplayName = "Gather System Information" }.ToX6Json(cell));

            Assert.AreEqual(Constants.DSFGATHERSYSTEMINFORMATIONACTIVITY.ToLower(), emitted);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WorkflowToX6Converter_LegacyTypeIdentity")]
        public void ToX6Json_LegacyDsfDecisionActivity_EmitsItsOwnTypeNotFlowDecision()
        {
            var decision = new DsfDecision
            {
                Conditions = new Dev2DecisionStack
                {
                    DisplayText = "Is x == 42",
                    TheStack = new List<Dev2Decision>()
                }
            };

            var emitted = TypeOf(cell => decision.ToX6Json(cell));

            Assert.AreEqual(Constants.DSFDECISION.ToLower(), emitted,
                "A legacy DsfDecision must round-trip back as itself, not as DsfFlowDecisionActivity (Constants.FLOWDECISION).");
        }

    }
}
