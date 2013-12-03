using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Sort
{
    [Binding]
    public class SortSteps : RecordSetBases
    {
          public SortSteps()
            : base(new List<Tuple<string, string>>())
        {
            
        }

        private DsfSortRecordsActivity _sortRecords;
        private string _sortOrder;
        
        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _sortRecords = new DsfSortRecordsActivity
            {
               SortField = Recordset,
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
            foreach (var t in tableRows)
            {
                _variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"I sort a record ""(.*)""")]
        public void GivenISortARecord(string recordset)
        {
            Recordset = recordset;
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
            _result = ExecuteProcess();
        }
        
        [Then(@"the sorted recordset ""(.*)""  will be")]
        public void ThenTheSortedRecordsetWillBe(string variable, Table table)
        {
            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

            string error;
            var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, column, out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }
    }
}
