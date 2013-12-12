using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.ControlFlow.Switch
{
    [Binding]
    public class SwitchSteps : RecordSetBases
    {
        private DsfFlowSwitchActivity _flowSwitch;

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            variableList.Add(new Tuple<string, string>(ResultVariable, ""));

            BuildShapeAndTestData();
            _flowSwitch = new DsfFlowSwitchActivity
                {
                    ExpressionText =
                        string.Format(
                            "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"{0}\",AmbientDataList)",
                            (variableList).First().Item1)
                };

            TestStartNode = new FlowStep
                {
                    Action = _flowSwitch
                };
        }

        [Given(@"I need to switch on variable ""(.*)"" with the value ""(.*)""")]
        public void GivenINeedToSwitchOnVariableWithTheValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the switch tool is executed")]
        public void WhenTheSwitchToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the variable ""(.*)"""" will evaluate to ""(.*)""")]
        public void ThenTheVariableWillEvaluateTo(string variable, string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"the switch execution has ""(.*)"" error")]
        public void ThenTheSwitchExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError,
                                           actual ? "did not occur" : "did occur" + fetchErrors);
            Assert.AreEqual(expected, actual, message);
        }
    }
}