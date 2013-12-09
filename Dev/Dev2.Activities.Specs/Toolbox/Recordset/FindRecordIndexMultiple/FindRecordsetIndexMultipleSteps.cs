using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.FindRecordIndexMultiple
{
    [Binding]
    public class FindRecordsetIndexMultipleSteps: RecordSetBases
    {
        private DsfFindRecordsMultipleCriteriaActivity _findRecordsMultipleIndex;
        List<FindRecordsTO> _searchList = new List<FindRecordsTO>();        
        int _row = 0;
        bool _requireAllTrue;
        bool _requireAllFieldsToMatch = false;
        string _fieldsToSearch;

        public FindRecordsetIndexMultipleSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));
            _findRecordsMultipleIndex = new DsfFindRecordsMultipleCriteriaActivity
                {
                    FieldsToSearch = string.IsNullOrEmpty(_fieldsToSearch) ? RecordSetName + "()" : _fieldsToSearch,
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
            foreach(TableRow t in tableRows)
            {
                _variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"the fields to search is")]
        public void GivenTheFieldsToSearchIs(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach(TableRow t in tableRows)
            {
                _fieldsToSearch += t[0] + ",";
            }
            if(_fieldsToSearch.EndsWith(","))
            {
                _fieldsToSearch = _fieldsToSearch.Remove(_fieldsToSearch.Length - 1);
            }
        }

        
        [Given(@"I have the following recordset in my datalist")]
        public void GivenIHaveTheFollowingRecordsetInMyDatalist(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            foreach(TableRow t in tableRows)
            {
                _variableList.Add(new Tuple<string, string>(t[0], t[1]));
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
            _result = ExecuteProcess();
        }
        
        [Then(@"the find records index multiple result should be (.*)")]
        public void ThenTheFindRecordsIndexMultipleResultShouldBe(string result)
        {
            string error;
            if(DataListUtil.IsValueRecordset(ResultVariable))
            {
                var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, ResultVariable);
                var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, ResultVariable);
                var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, column, out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                Assert.AreEqual(recordSetValues[1], result);
            }
            else
            {
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
        }

        [Then(@"the find record index has ""(.*)"" error")]
        public void ThenTheFindRecordIndexHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }        

    }
}
