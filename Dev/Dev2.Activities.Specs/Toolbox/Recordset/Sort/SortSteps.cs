using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Sort
{
    [Binding]
    public class SortSteps : RecordSetBases
    {
        private string _sortOrder;
        private DsfSortRecordsActivity _sortRecords;

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            var recordsetName = ScenarioContext.Current.Get<string>("recordset");
            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            _sortRecords = new DsfSortRecordsActivity
                {
                    SortField = recordsetName,
                    SelectedSort = _sortOrder
                };

            TestStartNode = new FlowStep
                {
                    Action = _sortRecords
                };
        }

        [Given(@"I have the following recordset to sort")]
        public void GivenIHaveTheFollowingRecordsetToSort(Table table)
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

        [Given(@"I sort a record ""(.*)""")]
        public void GivenISortARecord(string recordset)
        {
            ScenarioContext.Current.Add("recordset", recordset);
        }

        [Given(@"my sort order is ""(.*)""")]
        public void GivenMySortOrderIs(string sortOrder)
        {
            _sortOrder = sortOrder;
        }

        [When(@"the sort records tool is executed")]
        public void WhenTheSortRecordsToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess();
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the sorted recordset ""(.*)""  will be")]
        public void ThenTheSortedRecordsetWillBe(string variable, Table table)
        {
            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                           out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Then(@"the sort execution has ""(.*)"" error")]
        public void ThenTheSortExecutionHasError(string anError)
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