using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Count
{
    [Binding]
    public class CountSteps : RecordSetBases
    {
        private const string ResultVariable = "[[result]]";
        private DsfCountRecordsetActivity _count;

        public CountSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _count = new DsfCountRecordsetActivity
                {
                    RecordsetName = RecordSetName + "()",
                    CountNumber = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = _count
                };
        }

        [Given(@"I have a recordset with this shape")]
        public void GivenIHaveARecordsetWithThisShape(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (TableRow t in tableRows)
            {
                _variableList.Add(new Tuple<string, string>(t[0], ""));
            }
        }

        [When(@"the count tool is executed")]
        public void WhenTheCountToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the result count should be (.*)")]
        public void ThenTheResultCountShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            actualValue = string.IsNullOrEmpty(actualValue) ? "0" : actualValue;
            Assert.AreEqual(result , actualValue);
        }
    }
}