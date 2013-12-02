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
        private string _recordSetName;
        private string _sortOrder;


        private void BuildDataList()
        {
            var data = new StringBuilder();
            data.Append("<root>");
            
            int row = 1;
            foreach (var variable in _variableList)
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                if (variableName.Contains("(") && variableName.Contains(")"))
                {
                    variableName = variableName.Replace("(", "").Replace(")", "").Replace("*", "");
                    var variableNameSplit = variableName.Split(".".ToCharArray());
                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));
                    _recordSetName = variableNameSplit[0];
                }
                else
                {
                    data.Append(string.Format("<{0}/>", variableName));
                }
                row++;
            }
            
            data.Append("</root>");

            _sortRecords = new DsfSortRecordsActivity
            {
               SortField = _recordSetName + "()",
              SelectedSort = _sortOrder
            };

            TestStartNode = new FlowStep
            {
                Action = _sortRecords
            };

            CurrentDl = data.ToString();
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
        
        [Then(@"the recordser will be")]
        public void ThenTheRecordserWillBe(Table table)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, _recordSetName,
                                       out actualValue, out error);

            List<TableRow> tableRows = table.Rows.ToList();


            Assert.Fail("Not sure how the sort should work!!!");
            //Assert.AreEqual(dataRows.Count, tableRows.Count);

            //for (int i = 0; i < dataRows.Count; i++)
            //{
            //    Assert.AreEqual(dataRows[i][0].ToString(), tableRows[i][0]);
            //    Assert.AreEqual(dataRows[i][1].ToString(), tableRows[i][1]);
            //    Assert.AreEqual(dataRows[i][2].ToString(), tableRows[i][2]);
            //}
        }
    }
}
