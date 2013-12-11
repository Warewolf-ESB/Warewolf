using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.FindRecordIndexMultiple
{
    [Binding]
    public class FindRecordsetIndexMultipleSteps : RecordSetBases
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

            string fieldsToSearch;
            ScenarioContext.Current.TryGetValue("fieldsToSearch", out fieldsToSearch);
            List<FindRecordsTO> searchList;
            ScenarioContext.Current.TryGetValue("searchList", out searchList);
            bool requireAllTrue;
            ScenarioContext.Current.TryGetValue("requireAllTrue", out requireAllTrue);
            bool requireAllFieldsToMatch;
            ScenarioContext.Current.TryGetValue("requireAllFieldsToMatch", out requireAllFieldsToMatch);

            string recordsetName;
            ScenarioContext.Current.TryGetValue("recordset", out recordsetName);
            
            var findRecordsMultipleIndex = new DsfFindRecordsMultipleCriteriaActivity
                {
                    FieldsToSearch = string.IsNullOrEmpty(fieldsToSearch) ? recordsetName + "()" : fieldsToSearch,
                    ResultsCollection = searchList,
                    RequireAllTrue = requireAllTrue,
                    RequireAllFieldsToMatch = requireAllFieldsToMatch,
                    Result = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = findRecordsMultipleIndex
                };
        }

        [Given(@"I have the following recordset to search for multiple criteria")]
        public void GivenIHaveTheFollowingRecordsetToSearchForMultipleCriteria(Table table)
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
        
        [Given(@"the fields to search is")]
        public void GivenTheFieldsToSearchIs(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string fieldsToSearch = string.Empty;

            foreach (TableRow t in tableRows)
            {
                fieldsToSearch += t[0] + ",";
            }
            if (fieldsToSearch.EndsWith(","))
            {
                fieldsToSearch = fieldsToSearch.Remove(fieldsToSearch.Length - 1);
            }
            ScenarioContext.Current.Add("fieldsToSearch", fieldsToSearch);
        }


        [Given(@"I have the following recordset in my datalist")]
        public void GivenIHaveTheFollowingRecordsetInMyDatalist(Table table)
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


        [Given(@"search the recordset with type ""(.*)"" and criteria is ""(.*)""")]
        public void GivenSearchTheRecordsetWithTypeAndCriteriaIs(string searchType, string searchCriteria)
        {
            var row = GetRowCount();
            var searchList = GetSearchList();
            searchList.Add(new FindRecordsTO(searchCriteria, searchType, row));
        }

        private static int GetRowCount()
        {
            int row;
            bool rowAdded = ScenarioContext.Current.TryGetValue("row", out row);
            if (rowAdded)
            {
                ScenarioContext.Current.Add("row", row);
            }

            row++;
            return row;
        }

        [Given(@"is between search the recordset with type ""(.*)"" and criteria is ""(.*)"" and ""(.*)""")]
        public void GivenIsBetweenSearchTheRecordsetWithTypeAndCriteriaIsAnd(string searchType, string from, string to)
        {
            var row = GetRowCount();
            var searchList = GetSearchList();
            searchList.Add(new FindRecordsTO(string.Empty, searchType, row, false, false, from, to));
        }

        private  List<FindRecordsTO> GetSearchList()
        {
            List<FindRecordsTO> searchList;
            ScenarioContext.Current.TryGetValue("searchList", out searchList);

            if (searchList == null)
            {
                searchList = new List<FindRecordsTO>();
                ScenarioContext.Current.Add("searchList", searchList);
            }
            return searchList;
        }

        [Given(@"when all row true is ""(.*)""")]
        public void GivenWhenAllRowTrueIs(bool requireAllTrue)
        {
            ScenarioContext.Current.Add("requireAllTrue", requireAllTrue);
        }

        [Given(@"when requires all fields to match is ""(.*)""")]
        public void GivenWhenRequiresAllFieldsToMatchIs(bool requireAllFieldsToMatch)
        {
            ScenarioContext.Current.Add("requireAllFieldsToMatch", requireAllFieldsToMatch);
        }


        [When(@"the find records index multiple tool is executed")]
        public void WhenTheFindRecordsIndexMultipleToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the find records index multiple result should be (.*)")]
        public void ThenTheFindRecordsIndexMultipleResultShouldBe(string expectedResult)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if (DataListUtil.IsValueRecordset(ResultVariable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, ResultVariable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, ResultVariable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                Assert.AreEqual(recordSetValues[1], expectedResult);
            }
            else
            {
                string actualValue;
                expectedResult = expectedResult.Replace("\"\"", "");
                GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                           out actualValue, out error);
                Assert.AreEqual(expectedResult, actualValue);
            }
        }

        [Then(@"the find record index has ""(.*)"" error")]
        public void ThenTheFindRecordIndexHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            bool actual = string.IsNullOrEmpty(FetchErrors(result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}