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
        private DsfDeleteRecordActivity _delete;

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            variableList.Add(new Tuple<string, string>(ResultVariable, ""));

            BuildShapeAndTestData();

            var recordset = ScenarioContext.Current.Get<string>("recordset");
            _delete = new DsfDeleteRecordActivity
                {
                    RecordsetName = DataListUtil.RemoveLanguageBrackets(recordset),
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
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"an index ""(.*)"" exists with a value ""(.*)""")]
        public void GivenAnIndexExistsWithAValue(string variable, string value)
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

        [When(@"the delete tool is executed")]
        public void WhenTheDeleteToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess();
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the delete result should be ""(.*)""")]
        public void ThenTheDeleteResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"the recordset ""(.*)"" will be as follows")]
        public void ThenTheRecordsetWillBeAsFollows(string recordset, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string error;
            string recordsetName = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, recordset);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, recordset);
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordsetName,
                                                                           column, out error);

            Assert.AreEqual(tableRows.Count, recordSetValues.Count);

            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Given(@"I delete a record ""(.*)""")]
        public void GivenIDeleteARecord(string recordset)
        {
            ScenarioContext.Current.Add("recordset", recordset);
        }

        [Then(@"the delete execution has ""(.*)"" error")]
        public void ThenTheDeleteExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            bool actual = string.IsNullOrEmpty(FetchErrors(result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}