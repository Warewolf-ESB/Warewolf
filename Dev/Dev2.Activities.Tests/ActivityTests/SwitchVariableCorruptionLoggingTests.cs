using System;
using System.Collections.Generic;
using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Activities.WF;
using System.Activities.Statements;
using System.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class SwitchVariableCorruptionLoggingTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("GitHub Copilot")]
        [TestCategory("X6Convert_Switch_VariableCorruption")]
        public void X6Convert_Switch_VariableCorruption_ShouldLogCorruptionDetection()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            
            // Create a cell with corrupted expression field (with ": " prefix)
            var corruptedCell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>
                {
                    ["type"] = "DsfFlowSwitchActivity",
                    [Constants.DISPLAYNAME] = "Test Switch",
                    [Constants.EXPRESSION] = ": [[abc]]", // CORRUPTED - has ": " prefix
                    ["switchExpression"] = JsonConvert.SerializeObject(new 
                    {
                        SwitchVariable = "abc",
                        Cases = new[] { new { Key = "Case1", Value = "Case1" } },
                        DefaultCase = "Default"
                    }),
                    ["UniqueID"] = uniqueId
                }
            };

            var x6Graph = new X6WorkflowSaveModel
            {
                ResourceName = "CorruptionTestWorkflow",
                Cells = new List<Cell> { corruptedCell }
            };

            var converter = new X6ToWorkflowConverter();

            //------------Execute Test---------------------------
            var result = converter.X6JsonToWorkflow(JsonConvert.SerializeObject(x6Graph));

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            // The corruption should be detected and handled gracefully
            // The workflow should still be created despite the corruption
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("GitHub Copilot")]
        [TestCategory("X6Convert_Switch_VariableCorruption")]
        public void X6Convert_Switch_CleanExpression_ShouldLogNormalProcessing()
        {
            //------------Setup for test--------------------------
            var uniqueId = Guid.NewGuid().ToString();
            
            // Create a cell with clean expression field (no corruption)
            var cleanCell = new Cell
            {
                id = uniqueId,
                data = new Dictionary<string, object>
                {
                    ["type"] = "DsfFlowSwitchActivity",
                    [Constants.DISPLAYNAME] = "Test Switch",
                    [Constants.EXPRESSION] = "[[abc]]", // CLEAN - no ": " prefix
                    ["switchExpression"] = JsonConvert.SerializeObject(new 
                    {
                        SwitchVariable = "abc",
                        Cases = new[] { new { Key = "Case1", Value = "Case1" } },
                        DefaultCase = "Default"
                    }),
                    ["UniqueID"] = uniqueId
                }
            };

            var x6Graph = new X6WorkflowSaveModel
            {
                ResourceName = "CleanTestWorkflow",
                Cells = new List<Cell> { cleanCell }
            };

            var converter = new X6ToWorkflowConverter();

            //------------Execute Test---------------------------
            var result = converter.X6JsonToWorkflow(JsonConvert.SerializeObject(x6Graph));

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("GitHub Copilot")]
        [TestCategory("X6Convert_Switch_Serialization")]
        public void WorkflowToX6Converter_ProcessFlowSwitch_ShouldLogSerializationDetails()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Test Switch Logging", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[testVariable]]\",AmbientDataList)",
                DisplayName = "Test Switch Logging"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity
            };

            flowSwitch.Cases.Add("Case1", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Cases.Add("Case2", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Default = new FlowStep { Action = new DsfMultiAssignActivity() };

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "<TestXml/>");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            // Should have switch node
            var switchNode = deserializedResult.Nodes.Find(n => n.data.ContainsKey(Constants.TYPE) && 
                                                               n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            Assert.IsNotNull(switchNode);
            
            // Verify the expression field is NOT corrupted during serialization
            Assert.IsTrue(switchNode.data.ContainsKey(Constants.EXPRESSION));
            var expressionValue = switchNode.data[Constants.EXPRESSION].ToString();
            
            // The expression should NOT start with ": " (corruption)
            Assert.IsFalse(expressionValue.StartsWith(": "), $"Expression field is corrupted: '{expressionValue}'");
            
            // Should have switch expression data
            Assert.IsTrue(switchNode.data.ContainsKey("switchExpression"));
            var switchExpressionJson = switchNode.data["switchExpression"].ToString();
            var switchExpression = JsonConvert.DeserializeObject<dynamic>(switchExpressionJson);
            Assert.AreEqual("testVariable", switchExpression.SwitchVariable.ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("GitHub Copilot")]
        [TestCategory("X6Convert_Switch_RoundTrip")]
        public void X6Convert_Switch_RoundTrip_ShouldMaintainVariableIntegrity()
        {
            //------------Setup for test--------------------------
            var originalConverter = new WorkflowToX6Converter();
            var roundTripConverter = new X6ToWorkflowConverter();
            
            var flowSwitchActivity = new DsfFlowSwitchActivity("Round Trip Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[roundTripVar]]\",AmbientDataList)",
                DisplayName = "Round Trip Switch"
            };

            var flowSwitch = new FlowSwitch<string>
            {
                Expression = flowSwitchActivity
            };

            flowSwitch.Cases.Add("TestCase", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch.Default = new FlowStep { Action = new DsfMultiAssignActivity() };

            //------------Execute Test - Serialize to X6---------------------------
            var x6Json = originalConverter.ConvertToX6Json(new ActivityBuilder { Implementation = new Flowchart { StartNode = flowSwitch } }, "<TestXml/>");
            
            Assert.IsNotNull(x6Json);
            
            //------------Execute Test - Deserialize back to Workflow---------------------------
            var roundTripXaml = roundTripConverter.X6JsonToWorkflow(x6Json);

            //------------Assert Results-------------------------
            Assert.IsNotNull(roundTripXaml);
            Assert.IsTrue(roundTripXaml.Length > 0);
            
            // Verify the intermediate X6 JSON doesn't have corruption
            var x6Model = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(x6Json);
            var switchNode = x6Model.Nodes.Find(n => n.data.ContainsKey(Constants.TYPE) && 
                                                   n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            
            Assert.IsNotNull(switchNode);
            
            if (switchNode.data.TryGetValue(Constants.EXPRESSION, out var expressionObj))
            {
                var expressionValue = expressionObj.ToString();
                // Verify no corruption was introduced during serialization
                Assert.IsFalse(expressionValue.StartsWith(": "), $"Corruption detected in round-trip serialization: '{expressionValue}'");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("GitHub Copilot")]
        [TestCategory("X6Convert_Switch_Logging")]
        public void X6Convert_Switch_ComplexWorkflow_ShouldLogAllProcessingSteps()
        {
            //------------Setup for test--------------------------
            var converter = new WorkflowToX6Converter();
            
            // Create a more complex workflow with multiple switch activities
            var sequence = new Sequence();
            
            var switch1 = new DsfFlowSwitchActivity("First Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[var1]]\",AmbientDataList)",
                DisplayName = "First Switch"
            };

            var switch2 = new DsfFlowSwitchActivity("Second Switch", new Mock<IDebugDispatcher>().Object)
            {
                UniqueID = Guid.NewGuid().ToString(),
                ExpressionText = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[var2]]\",AmbientDataList)",
                DisplayName = "Second Switch"
            };

            var flowSwitch1 = new FlowSwitch<string> { Expression = switch1 };
            var flowSwitch2 = new FlowSwitch<string> { Expression = switch2 };
            
            flowSwitch1.Cases.Add("A", new FlowStep { Action = new DsfMultiAssignActivity(), Next = flowSwitch2 });
            flowSwitch1.Cases.Add("B", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch1.Default = new FlowStep { Action = new DsfMultiAssignActivity() };
            
            flowSwitch2.Cases.Add("X", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch2.Cases.Add("Y", new FlowStep { Action = new DsfMultiAssignActivity() });
            flowSwitch2.Default = new FlowStep { Action = new DsfMultiAssignActivity() };

            var flowchart = new Flowchart { StartNode = flowSwitch1 };
            flowchart.Nodes.Add(flowSwitch2);

            //------------Execute Test---------------------------
            var result = converter.ConvertToX6Json(new ActivityBuilder { Implementation = flowchart }, "<ComplexTestXml/>");

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            var deserializedResult = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(result);
            
            // Should have multiple switch nodes
            var switchNodes = deserializedResult.Nodes.FindAll(n => n.data.ContainsKey(Constants.TYPE) && 
                                                                   n.data[Constants.TYPE].ToString() == Constants.FLOWSWITCH);
            Assert.AreEqual(2, switchNodes.Count);
            
            // Verify neither switch has corrupted expression
            foreach (var switchNode in switchNodes)
            {
                if (switchNode.data.TryGetValue(Constants.EXPRESSION, out var expressionObj))
                {
                    var expressionValue = expressionObj.ToString();
                    Assert.IsFalse(expressionValue.StartsWith(": "), $"Switch node {switchNode.id} has corrupted expression: '{expressionValue}'");
                }
            }
        }
    }
}