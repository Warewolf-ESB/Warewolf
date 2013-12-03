using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Sort
{
    [Binding]
    public class SortSteps : BaseActivityUnitTest
    {
        private DsfSortRecordsActivity _sortRecords;
        private readonly List<Tuple<string, string>> _variableList = new List<Tuple<string, string>>();
        private IDSFDataObject _result;
        private string _sortOrder;
        private string _recordsetField;
        private string _recordset;


        private void BuildDataList()
        {
            var shape = new StringBuilder();
            shape.Append("<root>");

            var data = new StringBuilder();
            data.Append("<root>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                if (variableName.Contains("(") && variableName.Contains(")"))
                {
                    var startIndex = variableName.IndexOf("(");
                    var endIndex = variableName.IndexOf(")");

                    int i = (endIndex - startIndex) - 1;

                    if (i > 0)
                    {
                        variableName = variableName.Remove(startIndex + 1, i);
                    }

                    variableName = variableName.Replace("(", "").Replace(")", "").Replace("*", "");
                    var variableNameSplit = variableName.Split(".".ToCharArray());
                    shape.Append(string.Format("<{0}>", variableNameSplit[0]));
                    shape.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    shape.Append(string.Format("</{0}>", variableNameSplit[0]));

                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableNameSplit[1], variable.Item2));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));

                    _recordsetField = variableNameSplit[1];
                }
                else
                {
                    shape.Append(string.Format("<{0}/>", variableName));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item2));
                }
                row++;
            }
            
            shape.Append("</root>");
            data.Append("</root>");

            _sortRecords = new DsfSortRecordsActivity
            {
               SortField = _recordset,
              SelectedSort = _sortOrder
            };

            TestStartNode = new FlowStep
            {
                Action = _sortRecords
            };

            CurrentDl = shape.ToString();
            TestData = data.ToString();
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
            _recordset = recordset;
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
        public void ThenTheSortedRecordsetWillBe(string recordset, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string error;

            var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, _recordsetField, out error);

            Assert.AreEqual(tableRows.Count, recordSetValues.Count);

            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }
    }
}
