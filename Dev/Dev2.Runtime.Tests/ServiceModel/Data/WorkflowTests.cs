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
using Dev2.Common.Serializers;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class WorkflowTests
    {

        private Guid _decisionActivityId = Guid.Parse("5b8969d8-9fe4-4059-9dad-bfa0606ff60c");
        private Guid _decisionTrueArmId = Guid.Parse("b372a48c-edf9-45f7-b30e-781b6d223030");
        private Guid _decisionFalseArmId = Guid.Parse("89b0151c-112c-4d28-810d-f3898a763b86");

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowStep_ContainingDsfCommentActivity_ShouldExcludeDsfCommentActivity()
        {
            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");

            var flowNodes = new Collection<FlowNode>
            {
               new FlowStep { Action = new DsfCommentActivity { ActivityId = commentActivityId, UniqueID = commentActivityId.ToString(), DisplayName = "Comment (this activity should not be part of the coverage)" } }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(0, nodes.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowStep_ContainingNestedDsfCommentActivity_ShouldExcludeDsfCommentActivity()
        {
            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");
            var assignActivityId = Guid.Parse("fd46f7a1-4533-40a0-a7a5-3b8b4a0d46cd");

            var commentActitivity = new DsfCommentActivity { ActivityId = commentActivityId, UniqueID = commentActivityId.ToString(), DisplayName = "Comment (this activity should not be part of the coverage)" };
            var assignActivity = new DsfMultiAssignActivity { ActivityId = assignActivityId, UniqueID = assignActivityId.ToString(), DisplayName = "Assign (user info)" };
            var flowNodes = new Collection<FlowNode>
            {
               new FlowStep { Action =  assignActivity, Next = new FlowStep { Action = commentActitivity } }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(assignActivityId, nodes.First().ActivityID);
            Assert.IsNull(nodes.Find(o=> o.ActivityID == commentActivityId));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowDecision_HandlingNestedObjects_ShouldSuccess()
        {
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "a",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };

            var trueArm = new DsfMultiAssignActivity
            {
                ActivityId = _decisionTrueArmId,
                UniqueID = _decisionTrueArmId.ToString(),
                DisplayName = "Assign (success)",
            };

            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");
            var commentActitivity = new DsfCommentActivity 
            { 
                ActivityId = commentActivityId, 
                UniqueID = commentActivityId.ToString(), 
                DisplayName = "Comment (this activity should not be part of the coverage)" 
            };

            var falseArm = new DsfMultiAssignActivity
            {
                ActivityId = _decisionFalseArmId,
                UniqueID = _decisionFalseArmId.ToString(),
                DisplayName = "Assign (fail)",
                NextNodes = new List<IDev2Activity> { commentActitivity, new DsfMultiAssignActivity() }
            };

            var jsonSerializer = new Dev2JsonSerializer();
            var flowDecisionActivity = new DsfFlowDecisionActivity
            {
                ExpressionText = jsonSerializer.Serialize(dev2DecisionStack)
            };
            var flowDecision = new FlowDecision(flowDecisionActivity)
            {
                DisplayName = "Decision (sdf)",
                True = new FlowStep { Action = trueArm },
                False = new FlowStep { Action = falseArm }
            };

            var flowNodes = new Collection<FlowNode>
            {
                flowDecision
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(2, nodes[0].NextNodes.Count);

        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowDecision_HandlingNested_FlowDecision_ShouldSuccess()
        {
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "a",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };

            var trueArm = new DsfMultiAssignActivity
            {
                ActivityId = _decisionTrueArmId,
                UniqueID = _decisionTrueArmId.ToString(),
                DisplayName = "Assign (success)",
            };

            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");
            var commentActitivity = new DsfCommentActivity
            {
                ActivityId = commentActivityId,
                UniqueID = commentActivityId.ToString(),
                DisplayName = "Comment (this activity should not be part of the coverage)"
            };

            var falseArm = new DsfMultiAssignActivity
            {
                ActivityId = _decisionFalseArmId,
                UniqueID = _decisionFalseArmId.ToString(),
                DisplayName = "Assign (fail)",
                NextNodes = new List<IDev2Activity> { commentActitivity, new DsfMultiAssignActivity() }
            };

            var jsonSerializer = new Dev2JsonSerializer();
            var flowDecisionActivity = new DsfFlowDecisionActivity
            {
                ExpressionText = jsonSerializer.Serialize(dev2DecisionStack)
            };

            var flowDecisionInner = new FlowDecision(flowDecisionActivity)
            {
                DisplayName = "Decision (child)",
                True = new FlowStep { Action = trueArm },
                False = new FlowStep { Action = falseArm }
            };

            var flowDecision = new FlowDecision(flowDecisionActivity)
            {
                DisplayName = "Decision (parent)",
                True = new FlowStep { Action = trueArm },
                False = flowDecisionInner
            };

            var flowNodes = new Collection<FlowNode>
            {
                flowDecision
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(2, nodes[0].NextNodes.Count);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowSwitch_HandlingNestedObjects_ShouldSuccess()
        {
            var dev2DecisionStack = new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("a")
                        }
                    }
                },
                DisplayText = "a",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };

            var assignSuccess = new DsfMultiAssignActivity
            {
                ActivityId = _decisionTrueArmId,
                UniqueID = _decisionTrueArmId.ToString(),
                DisplayName = "Assign (success)",
            };

            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");
            var commentActitivity = new DsfCommentActivity
            {
                ActivityId = commentActivityId,
                UniqueID = commentActivityId.ToString(),
                DisplayName = "Comment (this activity should not be part of the coverage)"
            };

            var assignFalse = new DsfMultiAssignActivity
            {
                ActivityId = _decisionFalseArmId,
                UniqueID = _decisionFalseArmId.ToString(),
                DisplayName = "Assign (fail)",
                NextNodes = new List<IDev2Activity> { commentActitivity, new DsfMultiAssignActivity() }
            };

            var decisionActivity = new DsfDecision 
            {
                ActivityId = _decisionActivityId,
                UniqueID = _decisionActivityId.ToString(),
                DisplayName = "Decision (inside switch)",
                Conditions = dev2DecisionStack,
                FalseArm = new List<IDev2Activity> { assignFalse },
                TrueArm = new List<IDev2Activity> { assignSuccess },
            };

            var jsonSerializer = new Dev2JsonSerializer();
            var flowDecisionActivity = new DsfFlowDecisionActivity
            {
                ExpressionText = jsonSerializer.Serialize(dev2DecisionStack)
            };
            var flowDecision = new FlowDecision(flowDecisionActivity)
            {
                DisplayName = "Decision (sdf)",
                True = new FlowStep { Action = assignSuccess },
                False = new FlowStep { Action = assignFalse }
            };

            var flowSwitch = new FlowSwitch<string>();

            flowSwitch.Cases.Add("Case1", new FlowStep { Action = assignSuccess });
            flowSwitch.Cases.Add("Case2", new FlowStep { Action = decisionActivity });
            flowSwitch.Cases.Add("Case3", flowDecision);
            flowSwitch.Default = new FlowStep();
       
            var flowNodes = new Collection<FlowNode>
            {
                flowSwitch
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual("Assign (success)", nodes[0].NextNodes[0].StepDescription);
            Assert.AreEqual("a", nodes[0].NextNodes[1].StepDescription);
            Assert.AreEqual("Decision (sdf)", nodes[0].NextNodes[2].StepDescription);

        }
    }
}
