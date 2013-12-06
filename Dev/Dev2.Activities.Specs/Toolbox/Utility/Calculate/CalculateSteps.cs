using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityUnitTests;
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
        public CalculateSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private DsfCalculateActivity _calculate;
        private string _formula;

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));
            _calculate = new DsfCalculateActivity
                {
                    Result = ResultVariable,
                    Expression = _formula
                };

            TestStartNode = new FlowStep
                {
                    Action = _calculate
                };
        }

        [Given(@"I have the formula ""(.*)""")]
        public void GivenIHaveTheFormula(string formula)
        {
            _formula = formula;
        }
        
        [When(@"the calculate tool is executed")]
        public void WhenTheCalculateToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Given(@"I have a calculate variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveACalculateVariableEqualTo(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, string.Empty));
        }


        [Given(@"I have a calculate variable ""(.*)"" equal to")]
        public void GivenIHaveACalculateVariableEqualTo(string recordset, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            for (int i = 0; i < tableRows.Count; i++)
            {
                _variableList.Add(new Tuple<string, string>(recordset, tableRows[i][0]));
            }
        }

        [Then(@"the calculate result should be ""(.*)""")]
        public void ThenTheCalculateResultShouldBe(string result)
        {
            string error;
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}
