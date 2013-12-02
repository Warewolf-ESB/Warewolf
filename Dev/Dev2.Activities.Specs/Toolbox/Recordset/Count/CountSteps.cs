using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Count
{
    [Binding]
    public class CountSteps : BaseActivityUnitTest
    {
        private DsfCountRecordsetActivity _count;
        private readonly List<string> _variableList = new List<string>();
        private const string ResultVariable = "[[result]]";
        private IDSFDataObject _result;
        private string _recordSetName;
        
        private void BuildDataList()
        {
            var data = new StringBuilder();
            data.Append("<root>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                //_dataSplit.ResultsCollection.Add(new DataSplitDTO(variable.Item1, variable.Item2, variable.Item3, row));

                string variableName = DataListUtil.RemoveLanguageBrackets(variable);
                if (variableName.Contains("(") && variableName.Contains(")"))
                {
                    variableName = variableName.Replace("(", "").Replace(")", "").Replace("*", "");
                    var variableNameSplit = variableName.Split(".".ToCharArray());
                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));
                    _recordSetName = variableNameSplit[0];
                   // _fieldName = variableNameSplit[1];
                }
                else
                {
                    data.Append(string.Format("<{0}/>", variableName));
                }
                row++;
            }

            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            _count = new DsfCountRecordsetActivity
            {
                RecordsetName = _recordSetName + "()",
                CountNumber = ResultVariable
            };

            TestStartNode = new FlowStep
            {
                Action = _count
            };
           
            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have a recordset with this shape")]
        public void GivenIHaveARecordsetWithThisShape(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (var t in tableRows)
            {
                _variableList.Add(t[0]);
            }
        }

        [When(@"the count tool is executed")]
        public void WhenTheCountToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the result count should be (.*)")]
        public void ThenTheResultCountShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }
    }
}
