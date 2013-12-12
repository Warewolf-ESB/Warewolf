using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.Calculate
{
    [Binding]
    public class CalculateSteps : RecordSetBases
    {
        //private DsfCalculateActivity _calculate;
        //private string _formula;

        private void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string formula;
            ScenarioContext.Current.TryGetValue("formula", out formula);

            var calculate = new DsfCalculateActivity
                {
                    Result = ResultVariable,
                    Expression = formula
                };

            TestStartNode = new FlowStep
                {
                    Action = calculate
                };
        }

        [Given(@"I have the formula ""(.*)""")]
        public void GivenIHaveTheFormula(string formula)
        {
            ScenarioContext.Current.Add("formula", formula);
        }

        [When(@"the calculate tool is executed")]
        public void WhenTheCalculateToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I have a calculate variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveACalculateVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, string.Empty));
        }


        [Given(@"I have a calculate variable ""(.*)"" equal to")]
        public void GivenIHaveACalculateVariableEqualTo(string recordset, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            for (int i = 0; i < tableRows.Count; i++)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(recordset, tableRows[i][0]));
            }
        }

        [Then(@"the calculate result should be ""(.*)""")]
        public void ThenTheCalculateResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"the calculate execution has ""(.*)"" error")]
        public void ThenTheCalculateExecutionHasError(string anError)
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