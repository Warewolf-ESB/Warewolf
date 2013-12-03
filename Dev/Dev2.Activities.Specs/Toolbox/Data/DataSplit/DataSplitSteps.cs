using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityUnitTests;
using System;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        private string _recordSetName = "";
        private string _fieldName = "";
        
        private void BuildDataList()
        {
            _dataSplit = new DsfDataSplitActivity { SourceString = _stringToSplit };

            TestStartNode = new FlowStep
            {
                Action = _dataSplit
            };

            var data = new StringBuilder();
            data.Append("<root>");
            
            int row = 1;
            foreach (var variable in _variableList)
            {
                _dataSplit.ResultsCollection.Add(new DataSplitDTO(variable.Item1, variable.Item2, variable.Item3, row));

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
                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));
                    _recordSetName = variableNameSplit[0];
                    _fieldName = variableNameSplit[1];
                }
                else
                {
                    data.Append(string.Format("<{0}/>", variableName));
                }
                row++;
            }

            data.Append("</root>");
            CurrentDl = data.ToString();
            TestData = data.ToString();
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
        
        [Then(@"the split result will be")]
        public void ThenTheSplitResultWillBe(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string error;
            var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, _recordSetName, _fieldName, out error);

            Assert.AreEqual(tableRows.Count, recordSetValues.Count);

            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][0], recordSetValues[i]);
            }
        }
    }
}
