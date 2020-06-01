/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowStep_ContainingNoActivity_ShouldNotAddedAsWorkflowNode()
        {
            var flowNodes = new Collection<FlowNode>
            {
               new FlowStep()
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(0, nodes.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowStep_ContainingDsfCommentActivity_ShouldExcludeDsfCommentActivity()
        {
            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");

            var flowNodes = new Collection<FlowNode>
            {
               new FlowStep 
               { 
                   Action = new DsfCommentActivity 
                   { 
                       ActivityId = commentActivityId, 
                       UniqueID = commentActivityId.ToString(), 
                       DisplayName = "Comment (this activity should not be part of the coverage)" 
                   } 
               }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(0, nodes.Count);
            Assert.AreEqual(0, sut.WorkflowNodes.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_Given_WorkflowNodesForHtml_Executed_WorkflowNodes_FlowStep_ContainingDsfCommentActivity_ShouldExcludeDsfCommentActivity()
        {
            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");

            var flowNodes = new Collection<FlowNode>
            {
               new FlowStep 
               { 
                   Action = new DsfCommentActivity 
                   { 
                       ActivityId = commentActivityId, 
                       UniqueID = commentActivityId.ToString(), 
                       DisplayName = "Comment (this activity should not be part of the coverage)" 
                   } 
               }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(0, nodes.Count);
            
            Assert.AreEqual(0, sut.WorkflowNodes.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowStep_ContainingNestedDsfCommentActivity_ShouldExcludeDsfCommentActivity()
        {
            var commentActivityId = Guid.Parse("18db5fd4-a36c-44e2-b96c-e8a52ab9cd0d");
            var assignActivityId = Guid.Parse("fd46f7a1-4533-40a0-a7a5-3b8b4a0d46cd");

            var flowNodes = new Collection<FlowNode>
            {
               new FlowStep 
               { 
                   Action =  new DsfMultiAssignActivity 
                   { 
                       ActivityId = assignActivityId, 
                       UniqueID = assignActivityId.ToString(), 
                       DisplayName = "Assign (user info)" 
                   }, 
                   Next = new FlowStep 
                   { 
                       Action = new DsfCommentActivity 
                       { 
                           ActivityId = commentActivityId, 
                           UniqueID = commentActivityId.ToString(), 
                           DisplayName = "Comment (this activity should not be part of the coverage)" 
                       }
                   } 
               }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(assignActivityId, nodes.First().ActivityID);
            Assert.IsNull(nodes.Find(o=> o.ActivityID == commentActivityId));

            Assert.AreEqual(1, sut.WorkflowNodes.Count);
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

            var jsonSerializer = new Dev2JsonSerializer();
            var flowDecisionActivity = new DsfFlowDecisionActivity
            {
                ExpressionText = jsonSerializer.Serialize(dev2DecisionStack)
            };

            var flowNodes = new Collection<FlowNode>
            {
                new FlowDecision(flowDecisionActivity)
                {
                    DisplayName = "Decision (sdf)",
                    True = new FlowStep { Action = new DsfMultiAssignActivity { DisplayName = "Assign (success)" } },
                    False = new FlowStep { Action = new DsfMultiAssignActivity
                    {
                        DisplayName = "Assign (fail)",
                        NextNodes = new List<IDev2Activity>
                        {
                            new DsfCommentActivity { DisplayName = "Comment (this activity should not be part of the coverage)" },
                            new DsfMultiAssignActivity { DisplayName = "Assign (child node)" }
                        }
                    }}
                }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(2, nodes[0].NextNodes.Count);

            Assert.AreEqual(3, sut.WorkflowNodes.Count);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_Given_WorkflowNodesForHtml_Executed_WorkflowNodes_FlowDecision_HandlingNestedObjects_ShouldReturnAllNodes()
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

            var jsonSerializer = new Dev2JsonSerializer();
            var flowDecisionActivity = new DsfFlowDecisionActivity
            {
                ExpressionText = jsonSerializer.Serialize(dev2DecisionStack)
            };

            var flowNodes = new Collection<FlowNode>
            {
                new FlowDecision(flowDecisionActivity)
                {
                    DisplayName = "Decision (sdf)",
                    True = new FlowStep { Action = new DsfMultiAssignActivity { DisplayName = "Assign (success)" } },
                    False = new FlowStep { Action = new DsfMultiAssignActivity
                    {
                        DisplayName = "Assign (fail)",
                        NextNodes = new List<IDev2Activity>
                        {
                            new DsfCommentActivity { DisplayName = "Comment (this activity should not be part of the coverage)" },
                            new DsfMultiAssignActivity { DisplayName = "Assign (child node)" }
                        }
                    }}
                }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(2, nodes[0].NextNodes.Count);

            Assert.AreEqual(3, sut.WorkflowNodes.Count);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_FlowDecision_HandlingNested_FlowDecision_ShouldSuccess()
        {
            var dev2DecisionStackParent = GetDecisionStackParent();
            var dev2DecisionStackChild = GetDecisionChild();

            var jsonSerializer = new Dev2JsonSerializer();
            var flowNodes = new Collection<FlowNode>
            {
                new FlowDecision()
                {
                    Condition = new DsfFlowDecisionActivity
                    {
                        ExpressionText = jsonSerializer.Serialize(dev2DecisionStackParent)
                    },
                    DisplayName = "Decision (parent)",
                    True = new FlowStep
                    {
                        Action = new DsfMultiAssignActivity
                        {
                            DisplayName = "Assign (success)"
                        }
                    },
                    False = new FlowDecision()
                    {
                        Condition = new DsfFlowDecisionActivity
                        {
                            ExpressionText = jsonSerializer.Serialize(dev2DecisionStackChild)
                        },
                        DisplayName = "Decision (child)",
                        True = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-True Arm"
                            }
                        },
                        False = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-False Arm"
                            }
                        }
                    }
                }
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);
            Assert.AreEqual(2, nodes[0].NextNodes.Count);

            Assert.AreEqual(5, sut.WorkflowNodes.Count);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodesForHtml_When_FlowSwitch_HandlingNestedObjects_Executed_WorkflowNodes_ShouldReturnAllNodesInWorkflow()
        {
            var jsonSerializer = new Dev2JsonSerializer();
            var dev2DecisionStackParent = GetDecisionStackParent();
            var dev2DecisionStackChild = GetDecisionChild();

            var switchId = Guid.Parse("0d723deb-402d-43ec-bdbc-97c645a3a8f1");
            var flowSwitch = new FlowSwitch<string>()
            {
                Expression = new DsfFlowSwitchActivity
                {
                    ActivityId = switchId,
                    UniqueID = switchId.ToString(),
                    DisplayName = "Switch (dsf)"
                }
            };
            flowSwitch.Cases
                .Add("Case1", new FlowStep 
                { 
                    Action = new DsfMultiAssignActivity 
                    { 
                        DisplayName = "Assign (case 1)" 
                    } 
                });
            flowSwitch.Cases
                .Add("Case2", new FlowStep 
                { 
                    Action = new DsfMultiAssignActivity 
                    {
                        DisplayName = "Assign (case 2)" 
                    } 
                });
            flowSwitch.Cases
                .Add("Case3",new FlowDecision()
                {
                    Condition = new DsfFlowDecisionActivity
                    {
                        ExpressionText = jsonSerializer.Serialize(dev2DecisionStackParent)
                    },
                    DisplayName = "Decision (parent)",
                    True = new FlowStep
                    {
                        Action = new DsfMultiAssignActivity
                        {
                            DisplayName = "Assign (success)"
                        }
                    },
                    False = new FlowDecision()
                    {
                        Condition = new DsfFlowDecisionActivity
                        {
                            ExpressionText = jsonSerializer.Serialize(dev2DecisionStackChild)
                        },
                        DisplayName = "Decision (child)",
                        True = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-True Arm"
                            }
                        },
                        False = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-False Arm"
                            }
                        }
                    }
                });
            flowSwitch.Default = new FlowStep();

            var flowNodes = new Collection<FlowNode>
            {
                flowSwitch
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodesForHtml;

            Assert.AreEqual(1, nodes.Count);

            Assert.AreEqual(8, sut.WorkflowNodes.Count);
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Workflow))]
        public void Workflow_WorkflowNodes_When_FlowSwitch_HandlingNestedObjects_Executed_WorkflowNodesForHtml_ShouldReturnAllNodesInWorkflow()
        {
            var jsonSerializer = new Dev2JsonSerializer();
            var dev2DecisionStackParent = GetDecisionStackParent();
            var dev2DecisionStackChild = GetDecisionChild();

            var switchId = Guid.Parse("0d723deb-402d-43ec-bdbc-97c645a3a8f1");
            var flowSwitch = new FlowSwitch<string>()
            {
                Expression = new DsfFlowSwitchActivity
                {
                    ActivityId = switchId,
                    UniqueID = switchId.ToString(),
                    DisplayName = "Switch (dsf)"
                }
            };
            flowSwitch.Cases
                .Add("Case1", new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        DisplayName = "Assign (case 1)"
                    }
                });
            flowSwitch.Cases
                .Add("Case2", new FlowStep
                {
                    Action = new DsfMultiAssignActivity
                    {
                        DisplayName = "Assign (case 2)"
                    }
                });
            flowSwitch.Cases
                .Add("Case3", new FlowDecision()
                {
                    Condition = new DsfFlowDecisionActivity
                    {
                        ExpressionText = jsonSerializer.Serialize(dev2DecisionStackParent)
                    },
                    DisplayName = "Decision (parent)",
                    True = new FlowStep
                    {
                        Action = new DsfMultiAssignActivity
                        {
                            DisplayName = "Assign (success)"
                        }
                    },
                    False = new FlowDecision()
                    {
                        Condition = new DsfFlowDecisionActivity
                        {
                            ExpressionText = jsonSerializer.Serialize(dev2DecisionStackChild)
                        },
                        DisplayName = "Decision (child)",
                        True = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-True Arm"
                            }
                        },
                        False = new FlowStep
                        {
                            Action = new DsfMultiAssignActivity
                            {
                                DisplayName = "Assign (Decision child)-False Arm"
                            }
                        }
                    }
                });
            flowSwitch.Default = new FlowStep();

            var flowNodes = new Collection<FlowNode>
            {
                flowSwitch
            };

            var sut = new Workflow(flowNodes);
            var nodes = sut.WorkflowNodes;

             Assert.AreEqual(8, nodes.Count);

            Assert.AreEqual(1, sut.WorkflowNodesForHtml.Count);
        }


        private static Dev2DecisionStack GetDecisionChild()
        {
            return new Dev2DecisionStack
            {
                TheStack = new List<Dev2Decision>
                {
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("aChild")
                        }
                    },
                    new Dev2Decision
                    {
                        Cols1 = new List<DataStorage.WarewolfAtom>
                        {
                            DataStorage.WarewolfAtom.NewDataString("aChild")
                        }
                    }
                },
                DisplayText = "aChild",
                FalseArmText = "ErrorArm",
                TrueArmText = "true Arm",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
        }

        private static Dev2DecisionStack GetDecisionStackParent()
        {
            return new Dev2DecisionStack
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
        }
    }
}
