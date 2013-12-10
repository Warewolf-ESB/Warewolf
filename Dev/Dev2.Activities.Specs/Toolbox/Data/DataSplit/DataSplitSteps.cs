using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
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
        private readonly List<Tuple<string, string, string>> _splitCollection = new List<Tuple<string, string, string>>();

        public DataSplitSteps()
            : base(new List<Tuple<string, string>>())
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
            foreach (dynamic variable in _splitCollection)
            {
                _dataSplit.ResultsCollection.Add(new DataSplitDTO(variable.Item1, variable.Item2, variable.Item3, row));
                row++;
            }
        }

        [Given(@"A file ""(.*)"" to split")]
        public void GivenAFileToSplit(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.Data.DataSplit.{0}",
                                               fileName);
            _stringToSplit = ReadFile(resourceName);
        }

        [Given(@"A string to split with value ""(.*)""")]
        public void GivenAStringToSplitWithValue(string stringToSplit)
        {
            _stringToSplit = stringToSplit;
        }

        [Given(@"assign to variable ""(.*)"" split type ""(.*)"" at ""(.*)""")]
        public void GivenAssignToVariableSplitTypeAt(string variable, string splitType, string splitAt)
        {
            _variableList.Add(new Tuple<string, string>(variable, ""));
            _splitCollection.Add(new Tuple<string, string, string>(variable ,splitType, splitAt));
        }
        
        [Given(@"assign to variable ""(.*)"" split type as ""(.*)"" at ""(.*)"" and escape ""(.*)"" and include is ""(.*)""")]
        public void GivenAssignToVariableSplitTypeAsAtAndEscapeAndIncludeIs(string variable, string splitType, string at, string escape, string include)
        {
            //Note that both the escape and include are not implemeted on the activity this will pass once these are implemented
            ScenarioContext.Current.Pending();
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

        [Then(@"the data split execution has ""(.*)"" error")]
        public void ThenTheDataSplitExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }

        [Then(@"the split result for ""(.*)"" will be ""(.*)""")]
        public void ThenTheSplitResultForWillBe(string variable, string value)
        {
            string actualValue;
            string error;
            value = value.Replace('"', ' ').Trim();
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            actualValue = actualValue.Replace('"', ' ').Trim();
            Assert.AreEqual(value, actualValue);
        }
    }
}