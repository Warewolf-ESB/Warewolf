/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Activities.Designers2.Decision;
using Dev2.Activities.Designers2.Switch;
using Dev2.Common;
using Dev2.Common.Serializers;
using Dev2.Common.State;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Messages;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.Utils
{
    [TestClass]
    public class ActivityHelperTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_Dev2Decision_SetArmTextDefaults_SetArmText_SetDisplayName()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var viewModel = new DecisionDesignerViewModel(CreateModelItem());
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "",
                FalseArmText = "",
                TrueArmText = "",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            dev2DecisionStack.TheStack = new List<Dev2Decision>();
            var decisionExpressionMessage = new ConfigureDecisionExpressionMessage();
            viewModel.Handle(decisionExpressionMessage);

            //------------Setup for test--------------------------

            ActivityHelper.SetArmTextDefaults(dev2DecisionStack);
            ActivityHelper.SetArmText(viewModel.ModelItem, dev2DecisionStack);
            ActivityHelper.SetDisplayName(viewModel.ModelItem, dev2DecisionStack);

            //------------Assert Results-------------------------
            var act = new DsfDecision { Conditions = dev2DecisionStack, And = true };
            var stateItems = act.GetState();
            Assert.AreEqual(5, stateItems.Count());
            var expectedResults = new[]
                      {
                new StateVariable
                {
                    Name = "Conditions",
                    Type = StateVariable.StateType.Input,
                    Value = serializer.Serialize(dev2DecisionStack)
                },
                new StateVariable
                {
                    Name="And",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = null
                },
                new StateVariable
                {
                    Name="TrueArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.TrueArm?.ToList())
                },
                new StateVariable
                {
                    Name="FalseArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.FalseArm?.ToList())
                }
            };

            var iter = act.GetState().Select((item, index) => new
            {
                value = item,
                expectValue = expectedResults[index]
            });
            Assert.AreEqual(expectedResults.Length, iter.Count());
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_Dev2Decision_Null_FalseArmText_SetArmText()
        {
            //---------------Set up test pack-------------------
            using (var viewModel = new DecisionDesignerViewModel(CreateModelItem()))
            {
                var dev2DecisionStack = new Dev2DecisionStack
                {
                    DisplayText = "",
                    FalseArmText = null,
                    TrueArmText = "",
                    Version = "2",
                    Mode = Dev2DecisionMode.AND,
                    TheStack = new List<Dev2Decision>()
                };
                var decisionExpressionMessage = new ConfigureDecisionExpressionMessage();
                viewModel.Handle(decisionExpressionMessage);

                //------------Setup for test--------------------------

                ActivityHelper.SetArmTextDefaults(dev2DecisionStack);
                Assert.AreEqual("False", dev2DecisionStack.FalseArmText);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_Dev2Decision_Null_TrueArmText_SetArmText()
        {
            //---------------Set up test pack-------------------
            using (var viewModel = new DecisionDesignerViewModel(CreateModelItem()))
            {
                var dev2DecisionStack = new Dev2DecisionStack
                {
                    DisplayText = "",
                    FalseArmText = "",
                    TrueArmText = null,
                    Version = "2",
                    Mode = Dev2DecisionMode.AND,
                    TheStack = new List<Dev2Decision>()
                };
                var decisionExpressionMessage = new ConfigureDecisionExpressionMessage();
                viewModel.Handle(decisionExpressionMessage);

                //------------Setup for test--------------------------

                ActivityHelper.SetArmTextDefaults(dev2DecisionStack);
                Assert.AreEqual("True", dev2DecisionStack.TrueArmText);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_WithKeyProperty_SetSwitchKeyProperty_Dev2Switch()
        {
            var uniqueId = Guid.NewGuid().ToString();
            var calcActivity = new DsfCalculateActivity { UniqueID = uniqueId };
            var flowStep = new FlowStep { Action = calcActivity };
            //---------------Set up test pack-------------------
            using (var viewModel = new SwitchDesignerViewModel(CreateSwitchModelItem(flowStep), "Switch"))
            {
                var mySwitch = new Dev2Switch
                {
                    SwitchExpression = "[[a]]"
                };

                var parentNodeProperty = viewModel.ModelItem.Properties["Cases"].Dictionary;

                //------------Setup for test--------------------------
                var switchCaseFirst = ModelItemUtils.CreateModelItem(parentNodeProperty.First());
                var switchCaseLast = ModelItemUtils.CreateModelItem(parentNodeProperty.Last());
                ActivityHelper.SetSwitchKeyProperty(mySwitch, switchCaseFirst);

                var modelItemFirst = switchCaseFirst.Properties["Value"].Value.Properties["Action"].Value;
                var modelItemLast = switchCaseLast.Properties["Value"].Value.Properties["Action"].Value;

                Assert.AreEqual(uniqueId, modelItemFirst.Properties["UniqueID"].ComputedValue);
                Assert.IsNull(modelItemLast);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_SetSwitchKeyProperty_Dev2Switch()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var viewModel = new DecisionDesignerViewModel(CreateModelItem());
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "",
                FalseArmText = "",
                TrueArmText = "",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            dev2DecisionStack.TheStack = new List<Dev2Decision>();
            var decisionExpressionMessage = new ConfigureDecisionExpressionMessage();
            viewModel.Handle(decisionExpressionMessage);

            var mySwitch = new Dev2Switch();
            //------------Setup for test--------------------------
            ActivityHelper.SetSwitchKeyProperty(mySwitch, viewModel.ModelItem);

            //------------Assert Results-------------------------
            var act = new DsfDecision { Conditions = dev2DecisionStack, And = true };
            var stateItems = act.GetState();
            Assert.AreEqual(5, stateItems.Count());
            var expectedResults = new[]
                      {
                new StateVariable
                {
                    Name = "Conditions",
                    Type = StateVariable.StateType.Input,
                    Value = serializer.Serialize(dev2DecisionStack)
                },
                new StateVariable
                {
                    Name="And",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = null
                },
                new StateVariable
                {
                    Name="TrueArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.TrueArm?.ToList())
                },
                new StateVariable
                {
                    Name="FalseArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.FalseArm?.ToList())
                }
            };

            var iter = act.GetState().Select((item, index) => new
            {
                value = item,
                expectValue = expectedResults[index]
            });
            Assert.AreEqual(expectedResults.Length, iter.Count());
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_InjectExpression_Dev2SwitchIsNull_ReturnNull()
        {
            //---------------Set up test pack-------------------
            var viewModel = new DecisionDesignerViewModel(CreateModelItem());
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "",
                FalseArmText = "",
                TrueArmText = "",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            dev2DecisionStack.TheStack = new List<Dev2Decision>();
            var decisionExpressionMessage = new ConfigureDecisionExpressionMessage();
            viewModel.Handle(decisionExpressionMessage);

            var mySwitch = new Dev2Switch();
            mySwitch = null;

            var expressionText1 = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                            "(\"", "aaaaaaa", "\",",
                                            GlobalConstants.InjectedDecisionDataListVariable,
                                            ")");
            var expressionText = viewModel.ModelItem.Properties[expressionText1];

            //------------Setup for test--------------------------
            var expr = ActivityHelper.InjectExpression(mySwitch, expressionText);
            Assert.IsNull(expr);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_InjectExpression_Dev2DecisionStackIsNull_ReturnNull()
        {
            //---------------Set up test pack-------------------
            var viewModel = new DecisionDesignerViewModel(CreateModelItem());
            var dev2DecisionStack = new Dev2DecisionStack();

            dev2DecisionStack.TheStack = new List<Dev2Decision>();
            var decisionExpressionMessage = new ConfigureDecisionExpressionMessage();
            viewModel.Handle(decisionExpressionMessage);

            dev2DecisionStack = null;
            var expressionText1 = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                    "(\"", "aaaaaaa", "\",",
                                                    GlobalConstants.InjectedDecisionDataListVariable,
                                                    ")");
            var expressionText = viewModel.ModelItem.Properties[expressionText1];

            //------------Setup for test--------------------------
            var expr = ActivityHelper.InjectExpression(dev2DecisionStack, expressionText);
            Assert.IsNull(expr);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_Dev2Switch_ExtractData_Null()
        {
            var inner2 = new DsfFlowSwitchActivity
            {
                ExpressionText = GlobalConstants.InjectedSwitchDataFetch
            };
            var val = ActivityHelper.ExtractData(inner2.ExpressionText);
            Assert.AreEqual("Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData", val);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_ExtractData_ExpectResult()
        {
            var expressionText = string.Join("", GlobalConstants.InjectedSwitchDataFetch,
                                                    "(\"", "aaaaaaa", "\",",
                                                    GlobalConstants.InjectedDecisionDataListVariable,
                                                    ")");
            var val = ActivityHelper.ExtractData(expressionText);
            Assert.AreEqual("aaaaaaa", val);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_InjectExpression_Dev2Switch()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var viewModel = new DecisionDesignerViewModel(CreateModelItem());
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "",
                FalseArmText = "",
                TrueArmText = "",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            dev2DecisionStack.TheStack = new List<Dev2Decision>();
            var decisionExpressionMessage = new ConfigureDecisionExpressionMessage();
            viewModel.Handle(decisionExpressionMessage);

            var mySwitch = new Dev2Switch();

            //------------Setup for test--------------------------
            var testAct = new DsfFlowSwitchActivity { ExpressionText = "" };
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);

            ActivityHelper.InjectExpression(mySwitch, prop.Object);

            //------------Assert Results-------------------------
            var act = new DsfDecision { Conditions = dev2DecisionStack, And = true };
            var stateItems = act.GetState();
            Assert.AreEqual(5, stateItems.Count());
            var expectedResults = new[]
                      {
                new StateVariable
                {
                    Name = "Conditions",
                    Type = StateVariable.StateType.Input,
                    Value = serializer.Serialize(dev2DecisionStack)
                },
                new StateVariable
                {
                    Name="And",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = null
                },
                new StateVariable
                {
                    Name="TrueArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.TrueArm?.ToList())
                },
                new StateVariable
                {
                    Name="FalseArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.FalseArm?.ToList())
                }
            };

            var iter = act.GetState().Select((item, index) => new
            {
                value = item,
                expectValue = expectedResults[index]
            });
            Assert.AreEqual(expectedResults.Length, iter.Count());
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ActivityHelper))]
        public void ActivityHelper_InjectExpression_Dev2DecisionStack()
        {
            //---------------Set up test pack-------------------
            var dev2DecisionStack = new Dev2DecisionStack()
            {
                DisplayText = "",
                FalseArmText = "",
                TrueArmText = "",
                Version = "2",
                Mode = Dev2DecisionMode.AND
            };
            dev2DecisionStack.TheStack = new List<Dev2Decision>();

            var testAct = new DsfFlowSwitchActivity { ExpressionText = "" };
            var prop = new Mock<ModelProperty>();
            prop.Setup(p => p.ComputedValue).Returns(testAct);

            ActivityHelper.InjectExpression(dev2DecisionStack, prop.Object);

            //------------Assert Results-------------------------
            var act = new DsfDecision { Conditions = dev2DecisionStack, And = true };
            var stateItems = act.GetState();
            Assert.AreEqual(5, stateItems.Count());
            var serializer = new Dev2JsonSerializer();
            var expectedResults = new[]
                      {
                new StateVariable
                {
                    Name = "Conditions",
                    Type = StateVariable.StateType.Input,
                    Value = serializer.Serialize(dev2DecisionStack)
                },
                new StateVariable
                {
                    Name="And",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = null
                },
                new StateVariable
                {
                    Name="TrueArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.TrueArm?.ToList())
                },
                new StateVariable
                {
                    Name="FalseArm",
                    Type = StateVariable.StateType.Output,
                    Value = ActivityHelper.GetSerializedStateValueFromCollection(act.FalseArm?.ToList())
                }
            };

            var iter = act.GetState().Select((item, index) => new
            {
                value = item,
                expectValue = expectedResults[index]
            });
            Assert.AreEqual(expectedResults.Length, iter.Count());
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        static ModelItem CreateModelItem()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDecision()
            {
                DisplayName = "A",
            });
            return modelItem;
        }

        static ModelItem CreateSwitchModelItem(FlowStep flowStep)
        {
            var dsfSwitch = new FlowSwitch<string>();
            var uniqueId = Guid.NewGuid();
            var activity = new DsfFlowSwitchActivity { UniqueID = uniqueId.ToString() };
            dsfSwitch.Expression = activity;
            dsfSwitch.Cases.Add("Case1", flowStep);
            dsfSwitch.Cases.Add("Case2", new FlowStep());
            dsfSwitch.Default = new FlowStep();

            var modelItem = ModelItemUtils.CreateModelItem(dsfSwitch);
            return modelItem;
        }
    }
}
