using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Delete
{
    [Binding]
    public class DeleteSteps : RecordSetBases
    {
        public DeleteSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private DsfDeleteRecordActivity _delete;

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _delete = new DsfDeleteRecordActivity
                {
                    RecordsetName = DataListUtil.RemoveLanguageBrackets(Recordset),
                    Result = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = _delete
                };
        }


        [Given(@"I have the following recordset")]
        public void GivenIHaveTheFollowingRecordset(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (TableRow t in tableRows)
            {
                _variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"an index ""(.*)"" exists with a value ""(.*)""")]
        public void GivenAnIndexExistsWithAValue(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the delete tool is executed")]
        public void WhenTheDeleteToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the delete result should be ""(.*)""")]
        public void ThenTheDeleteResultShouldBe(string result)
        {
            string error;
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }

        [Then(@"the recordset ""(.*)"" will be as follows")]
        public void ThenTheRecordsetWillBeAsFollows(string recordset, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string error;
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, RecordSetName,
                                                                           FieldName, out error);

            Assert.AreEqual(tableRows.Count, recordSetValues.Count);

            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Given(@"I delete a record ""(.*)""")]
        public void GivenIDeleteARecord(string recordset)
        {
            Recordset = recordset;
        }
    }
}