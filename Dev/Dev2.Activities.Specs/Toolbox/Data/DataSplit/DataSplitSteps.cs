using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.DataSplit
{
    [Binding]
    public class DataSplitSteps : RecordSetBases
    {
        private DsfDataSplitActivity _dataSplit;
        private string _stringToSplit;

        public DataSplitSteps()
            : base(new List<Tuple<string, string, string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _dataSplit = new DsfDataSplitActivity {SourceString = _stringToSplit};

            TestStartNode = new FlowStep
                {
                    Action = _dataSplit
                };

            int row = 1;
            foreach (dynamic variable in _variableList)
            {
                _dataSplit.ResultsCollection.Add(new DataSplitDTO(variable.Item1, variable.Item3, variable.Item4, row));
                row++;
            }
        }

        [Given(@"A string to split with value ""(.*)""")]
        public void GivenAStringToSplitWithValue(string stringToSplit)
        {
            _stringToSplit = stringToSplit;
        }

        [Given(@"assign to variable ""(.*)"" split type ""(.*)"" at ""(.*)""")]
        public void GivenAssignToVariableSplitTypeAt(string variable, string splitType, string splitAt)
        {
            _variableList.Add(new Tuple<string, string, string, string>(variable, "", splitType, splitAt));
        }

        [When(@"the data split tool is executed")]
        public void WhenTheDataSplitToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the split result will be")]
        public void ThenTheSplitResultWillBe(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string error;
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, RecordSetName,
                                                                           FieldName, out error);

            Assert.AreEqual(tableRows.Count, recordSetValues.Count);

            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][0], recordSetValues[i]);
            }
        }
    }
}