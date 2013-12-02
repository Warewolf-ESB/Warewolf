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

namespace Dev2.Activities.Specs.Toolbox.Recordset.FindRecordIndex
{
    [Binding]
    public class FindRecordIndexSteps : BaseActivityUnitTest
    {
        private DsfFindRecordsActivity _findRecordsIndex;
        private readonly List<Tuple<string, string>> _variableList = new List<Tuple<string, string>>();
        private const string ResultVariable = "[[result]]";
        private IDSFDataObject _result;
        private string _recordSetName;
        private string _searchType;
        private string _criteria;


        private void BuildDataList()
        {
            var shape = new StringBuilder();
            shape.Append("<ADL>");

            var data = new StringBuilder();
            data.Append("<ADL>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                if (variableName.Contains("(") && variableName.Contains(")"))
                {
                    variableName = variableName.Replace("(", "").Replace(")", "").Replace("*", "");
                    var variableNameSplit = variableName.Split(".".ToCharArray());
                    shape.Append(string.Format("<{0}>", variableNameSplit[0]));
                    shape.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    shape.Append(string.Format("</{0}>", variableNameSplit[0]));

                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableNameSplit[1], variable.Item2));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));

                    _recordSetName = variableNameSplit[0];
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

            shape.Append("</ADL>");
            data.Append("</ADL>");

            _findRecordsIndex = new DsfFindRecordsActivity
            {
                FieldsToSearch = _recordSetName + "()",
                Result = ResultVariable,
                SearchCriteria = _criteria,
                SearchType =  _searchType,
                RequireAllFieldsToMatch = false
            };

            TestStartNode = new FlowStep
            {
                Action = _findRecordsIndex
            };

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }


        [Given(@"I have the following recordset to search")]
        public void GivenIHaveTheFollowingRecordsetToSearch(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (var t in tableRows)
            {
                _variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }
        
        [Given(@"search type is ""(.*)"" and criteria is ""(.*)""")]
        public void GivenSearchTypeIsAndCriteriaIs(string searchType, string criteria)
        {
            _searchType = searchType;
            _criteria = criteria;
        }

        
        [When(@"the find records index tool is executed")]
        public void WhenTheFindRecordsIndexToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the delete result should be (.*)")]
        public void ThenTheDeleteResultShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}
