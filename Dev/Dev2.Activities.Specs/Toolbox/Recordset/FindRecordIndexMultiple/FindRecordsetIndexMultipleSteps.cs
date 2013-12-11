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
        private readonly List<FindRecordsTO> _searchList = new List<FindRecordsTO>();
        private string _fieldsToSearch;
        private DsfFindRecordsMultipleCriteriaActivity _findRecordsMultipleIndex;
        private bool _requireAllFieldsToMatch;
        private bool _requireAllTrue;
        private int _row;

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            variableList.Add(new Tuple<string, string>(ResultVariable, ""));

            BuildShapeAndTestData();

            var recordsetName = ScenarioContext.Current.Get<string>("recordset");

            _findRecordsMultipleIndex = new DsfFindRecordsMultipleCriteriaActivity
                {
                    FieldsToSearch = string.IsNullOrEmpty(_fieldsToSearch) ? recordsetName + "()" : _fieldsToSearch,
                    ResultsCollection = _searchList,
                    RequireAllTrue = _requireAllTrue,
                    RequireAllFieldsToMatch = _requireAllFieldsToMatch,
                    Result = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = _findRecordsMultipleIndex
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
            foreach (TableRow t in tableRows)
            {
                _fieldsToSearch += t[0] + ",";
            }
            if (_fieldsToSearch.EndsWith(","))
            {
                _fieldsToSearch = _fieldsToSearch.Remove(_fieldsToSearch.Length - 1);
            }
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
            _row++;
            _searchList.Add(new FindRecordsTO(searchCriteria, searchType, _row));
        }

        [Given(@"is between search the recordset with type ""(.*)"" and criteria is ""(.*)"" and ""(.*)""")]
        public void GivenIsBetweenSearchTheRecordsetWithTypeAndCriteriaIsAnd(string searchType, string from, string to)
        {
            _row++;
            _searchList.Add(new FindRecordsTO(string.Empty, searchType, _row, false, false, from, to));
        }

        [Given(@"when all row true is ""(.*)""")]
        public void GivenWhenAllRowTrueIs(bool matchAllFields)
        {
            _requireAllTrue = matchAllFields;
        }

        [Given(@"when requires all fields to match is ""(.*)""")]
        public void GivenWhenRequiresAllFieldsToMatchIs(bool requireAllFieldsToMatch)
        {
            _requireAllFieldsToMatch = requireAllFieldsToMatch;
        }


        [When(@"the find records index multiple tool is executed")]
        public void WhenTheFindRecordsIndexMultipleToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess();
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