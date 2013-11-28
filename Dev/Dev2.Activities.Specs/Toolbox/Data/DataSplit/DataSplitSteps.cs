using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using System;
using Dev2.DataList.Contract;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.DataSplit
{
    [Binding]
    public class DataSplitSteps : BaseActivityUnitTest
    {
        private DsfDataSplitActivity _dataSplit;
        private readonly List<Tuple<string, string, string>> _variableList = new List<Tuple<string, string, string>>();
        private IDSFDataObject _result;
        private string _stringToSplit;
        
        private void BuildDataList()
        {
            _dataSplit = new DsfDataSplitActivity { SourceString = _stringToSplit };

            TestStartNode = new FlowStep
            {
                Action = _dataSplit
            };

            var data = new StringBuilder();
            data.Append("<ADL>");

            var testData = new StringBuilder();
            testData.Append("<root>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                data.Append(string.Format("<{0}/>", variableName));
                _dataSplit.ResultsCollection.Add(new DataSplitDTO(variable.Item1, variable.Item2, variable.Item3, row));
                row++;
            }

            data.Append("</ADL>");
            testData.Append("</root>");

            CurrentDl = data.ToString();
            TestData = testData.ToString();
        }

        [Given(@"A string to split with value ""(.*)""")]
        public void GivenAStringToSplitWithValue(string stringToSplit)
        {
            _stringToSplit = stringToSplit;
        }

        [Given(@"assign to variable ""(.*)"" split type ""(.*)"" at ""(.*)""")]
        public void GivenAssignToVariableSplitTypeAt(string variable, string splitType, string splitAt)
        {
            _variableList.Add(new Tuple<string, string, string>(variable, splitType, splitAt));
        }

        
        [When(@"the data split tool is executed")]
        public void WhenTheDataSplitToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the split result is ""(.*)""")]
        public void ThenTheSplitResultIs(string p0)
        {
            var foreachItems = _dataSplit.GetForEachOutputs();
        }
    }
}
