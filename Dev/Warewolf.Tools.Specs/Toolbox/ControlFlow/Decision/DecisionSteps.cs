/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Decision
{
    [Binding]
    public class DecisionSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public DecisionSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            var decisionActivity = new DsfFlowDecisionActivity();
            scenarioContext.TryGetValue("mode", out Dev2DecisionMode mode);

            var decisionModels =
                scenarioContext.Get<List<Tuple<string, enDecisionType, string, string>>>("decisionModels");
            var dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = mode, TrueArmText = "YES", FalseArmText = "NO" };

            foreach(var dm in decisionModels)
            {
                var dev2Decision = new Dev2Decision
                    {
                        Col1 = dm.Item1 ?? string.Empty,
                        EvaluationFn = dm.Item2,
                        Col2 = dm.Item3 ?? string.Empty,
                        Col3 = dm.Item4 ?? string.Empty
                    };

                dds.AddModelItem(dev2Decision);
            }

            string modelData = dds.ToVBPersistableModel();
            scenarioContext.Add("modelData", modelData);

            decisionActivity.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData,
                                                          "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");

            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }


            var multiAssign = new DsfMultiAssignActivity();
            int row = 1;
            foreach(var variable in variableList)
            {
                multiAssign.FieldsCollection.Add(new ActivityDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
            FlowDecision x = new FlowDecision();
            x.Condition=decisionActivity;
            TestStartNode = new FlowStep
                {
                    Action = multiAssign,
                    Next = x
                };
            scenarioContext.Add("activity", decisionActivity);
        }

        [Given(@"a decision variable ""(.*)"" value ""(.*)""")]
        public void GivenADecisionVariableValue(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"Require all decisions to be true is ""(.*)""")]
        public void GivenRequireAllDecisionsToBeTrueIs(string p0)
        {
            if (p0 == "true")
            {
                scenarioContext.Add("mode", Dev2DecisionMode.AND);
            }
            else
            {
                scenarioContext.Add("mode", Dev2DecisionMode.OR);
            }
        }

        [Given(@"the decision mode is ""(.*)""")]
        public void GivenTheDecisionModeIs(string mode)
        {
            scenarioContext.Add("mode", (Dev2DecisionMode)Enum.Parse(typeof(Dev2DecisionMode), mode));
        }

        [Given(@"is ""(.*)"" ""(.*)"" ""(.*)""")]
        public void GivenIs(string variable1, string decision, string variable2)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), variable2, null
                    ));
        }

        [Given(@"I want to check ""(.*)""")]
        public void GivenIWantToCheck(string decision)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    null, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), null, null
                    ));
        }

        [Given(@"decide if ""(.*)"" ""(.*)""")]
        public void GivenDecideIf(string variable1, string decision)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), null, null
                    ));
        }

        [Given(@"check if ""(.*)"" ""(.*)"" ""(.*)"" and ""(.*)""")]
        public void GivenCheckIfAnd(string variable1, string decision, string variable2, string variable3)
        {
            scenarioContext.TryGetValue("decisionModels", out List<Tuple<string, enDecisionType, string, string>> decisionModels);

            if (decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                scenarioContext.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), variable2, variable3
                    ));
        }

        [When(@"the decision tool is executed")]
        public void WhenTheDecisionToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }
    }
}
