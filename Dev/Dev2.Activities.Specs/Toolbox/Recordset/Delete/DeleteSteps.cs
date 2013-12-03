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

namespace Dev2.Activities.Specs.Toolbox.Recordset.Delete
{
    [Binding]
    public class DeleteSteps : BaseActivityUnitTest
    {
        private DsfDeleteRecordActivity _delete;
        private readonly List<Tuple<string ,string>>  _variableList = new List<Tuple<string ,string>>();
        private const string ResultVariable = "[[result]]";
        private IDSFDataObject _result;
        private string _recordset;
        private string _recordsetField;
       

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


            shape.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));

            shape.Append("</root>");
            data.Append("</root>");

            _delete = new DsfDeleteRecordActivity
            {
                RecordsetName = DataListUtil.RemoveLanguageBrackets(_recordset),
                Result = ResultVariable
            };

            TestStartNode = new FlowStep
            {
                Action = _delete
            };

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }
        
        [Given(@"I have the following recordset")]
        public void GivenIHaveTheFollowingRecordset(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (var t in tableRows)
            {
                _variableList.Add(new Tuple<string ,string>(t[0], t[1]));
            }
        }
        
        [When(@"the delete tool is executed")]
        public void WhenTheDeleteToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the delete result should be ""(.*)""")]
        public void ThenTheDeleteResultShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }

        [Then(@"the recordset ""(.*)"" will be as follows")]
        public void ThenTheRecordsetWillBeAsFollows(string recordset, Table table)
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

        [Given(@"I delete a record ""(.*)""")]
        public void GivenIDeleteARecord(string recordset)
        {
            _recordset = recordset;
        }
    }
}
