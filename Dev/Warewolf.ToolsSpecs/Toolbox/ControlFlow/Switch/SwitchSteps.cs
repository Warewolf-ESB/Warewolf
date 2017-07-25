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
using System.Linq;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Switch
{
    [Binding]
    public class SwitchSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public SwitchSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            var variableList = scenarioContext.Get<List<Tuple<string, string>>>("variableList");
            variableList.Add(new Tuple<string, string>(ResultVariable, ""));

            BuildShapeAndTestData();
            var flowSwitch = new DsfFlowSwitchActivity
                {
                    ExpressionText =
                        string.Format(
                            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"{0}\",AmbientDataList)",
                            variableList.First().Item1),
                };
            var sw = new FlowSwitch<string>();
            sw.Expression = flowSwitch;
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
                    Next = sw
                };

            scenarioContext.Add("activity", flowSwitch);
        }

        [Given(@"I need to switch on variable ""(.*)"" with the value ""(.*)""")]
        public void GivenINeedToSwitchOnVariableWithTheValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the switch tool is executed")]
        public void WhenTheSwitchToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the variable ""(.*)"" will evaluate to ""(.*)""")]
        public void ThenTheVariableWillEvaluateTo(string variable, string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, variable,
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }
    }
}
