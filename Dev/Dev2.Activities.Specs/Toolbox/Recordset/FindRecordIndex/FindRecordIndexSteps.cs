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
        private void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string recordsetName;
            ScenarioContext.Current.TryGetValue("recordset", out recordsetName);
            
            var searchType = ScenarioContext.Current.Get<string>("searchType");
            var criteria = ScenarioContext.Current.Get<string>("criteria");

            var findRecordsIndex = new DsfFindRecordsActivity
                {
                    FieldsToSearch = recordsetName + "()",
                    Result = ResultVariable,
                    SearchCriteria = criteria,
                    SearchType = searchType,
                    RequireAllFieldsToMatch = false
                };

            TestStartNode = new FlowStep
                {
                    Action = findRecordsIndex
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
            ScenarioContext.Current.Add("searchType", searchType);
            ScenarioContext.Current.Add("criteria", criteria);
        }


        [When(@"the find records index tool is executed")]
        public void WhenTheFindRecordsIndexToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the findindex record index has ""(.*)"" error")]
        public void ThenTheFindindexRecordIndexHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found" + fetchErrors);
            Assert.AreEqual(expected, actual, message);
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