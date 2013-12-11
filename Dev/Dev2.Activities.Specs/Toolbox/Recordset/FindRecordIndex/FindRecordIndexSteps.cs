using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.FindRecordIndex
{
    [Binding]
    public class FindRecordIndexSteps : RecordSetBases
    {
        private string _criteria;
        private DsfFindRecordsActivity _findRecordsIndex;
        private string _searchType;

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            var recordsetName = ScenarioContext.Current.Get<string>("recordset");
            _findRecordsIndex = new DsfFindRecordsActivity
                {
                    FieldsToSearch = recordsetName + "()",
                    Result = ResultVariable,
                    SearchCriteria = _criteria,
                    SearchType = _searchType,
                    RequireAllFieldsToMatch = false
                };

            TestStartNode = new FlowStep
                {
                    Action = _findRecordsIndex
                };
        }

        [Given(@"I have the following recordset to search")]
        public void GivenIHaveTheFollowingRecordsetToSearch(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach (TableRow t in tableRows)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
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
            IDSFDataObject result = ExecuteProcess();
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the index result should be (.*)")]
        public void ThenTheIndexResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }
    }
}