
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Decision
{
    [Binding]
    public class DecisionSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();
            var decisionActivity = new DsfFlowDecisionActivity();
            Dev2DecisionMode mode;
            ScenarioContext.Current.TryGetValue("mode", out mode);

            var decisionModels =
                ScenarioContext.Current.Get<List<Tuple<string, enDecisionType, string, string>>>("decisionModels");
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
            ScenarioContext.Current.Add("modelData", modelData);

            decisionActivity.ExpressionText = string.Join("", GlobalConstants.InjectedDecisionHandler, "(\"", modelData,
                                                          "\",", GlobalConstants.InjectedDecisionDataListVariable, ")");

            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }


            var multiAssign = new DsfMultiAssignActivity();
            int row = 1;
            foreach(var variable in variableList)
            {
                multiAssign.FieldsCollection.Add(new ActivityDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }

            TestStartNode = new FlowStep
                {
                    Action = multiAssign,
                    Next = new FlowStep
                        {
                            Action = decisionActivity
                        }
                };
            ScenarioContext.Current.Add("activity", decisionActivity);
        }

        [Given(@"a decision variable ""(.*)"" value ""(.*)""")]
        public void GivenADecisionVariableValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"the decision mode is ""(.*)""")]
        public void GivenTheDecisionModeIs(string mode)
        {
            ScenarioContext.Current.Add("mode", (Dev2DecisionMode)Enum.Parse(typeof(Dev2DecisionMode), mode));
        }

        [Given(@"is ""(.*)"" ""(.*)"" ""(.*)""")]
        public void GivenIs(string variable1, string decision, string variable2)
        {
            List<Tuple<string, enDecisionType, string, string>> decisionModels;
            ScenarioContext.Current.TryGetValue("decisionModels", out decisionModels);

            if(decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                ScenarioContext.Current.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), variable2, null
                    ));
        }

        [Given(@"I want to check ""(.*)""")]
        public void GivenIWantToCheck(string decision)
        {
            List<Tuple<string, enDecisionType, string, string>> decisionModels;
            ScenarioContext.Current.TryGetValue("decisionModels", out decisionModels);

            if(decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                ScenarioContext.Current.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    null, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), null, null
                    ));
        }

        [Given(@"decide if ""(.*)"" ""(.*)""")]
        public void GivenDecideIf(string variable1, string decision)
        {
            List<Tuple<string, enDecisionType, string, string>> decisionModels;
            ScenarioContext.Current.TryGetValue("decisionModels", out decisionModels);

            if(decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                ScenarioContext.Current.Add("decisionModels", decisionModels);
            }

            decisionModels.Add(
                new Tuple<string, enDecisionType, string, string>(
                    variable1, (enDecisionType)Enum.Parse(typeof(enDecisionType), decision), null, null
                    ));
        }

        [Given(@"check if ""(.*)"" ""(.*)"" ""(.*)"" and ""(.*)""")]
        public void GivenCheckIfAnd(string variable1, string decision, string variable2, string variable3)
        {
            List<Tuple<string, enDecisionType, string, string>> decisionModels;
            ScenarioContext.Current.TryGetValue("decisionModels", out decisionModels);

            if(decisionModels == null)
            {
                decisionModels = new List<Tuple<string, enDecisionType, string, string>>();
                ScenarioContext.Current.Add("decisionModels", decisionModels);
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
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the decision result should be ""(.*)""")]
        public void ThenTheDecisionResultShouldBe(string expectedRes)
        {
            var modelData = ScenarioContext.Current.Get<string>("modelData");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            bool actual = new Dev2DataListDecisionHandler().ExecuteDecisionStack(modelData,
                                                                                 new List<string>
                                                                                     {
                                                                                         result.DataListID.ToString()
                                                                                     });
            bool expected = Boolean.Parse(expectedRes);
            Assert.AreEqual(expected, actual);
        }
    }
}
