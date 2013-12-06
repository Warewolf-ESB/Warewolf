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

        public FindRecordsetIndexMultipleSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));
            _findRecordsMultipleIndex = new DsfFindRecordsMultipleCriteriaActivity
                {
                    FieldsToSearch = RecordSetName + "()",
                    ResultsCollection = _searchList,
                    RequireAllFieldsToMatch = false,
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
        
        [Given(@"search the recordset with type ""(.*)"" and criteria is ""(.*)""")]
        public void GivenSearchTheRecordsetWithTypeAndCriteriaIs(string searchType, string searchCriteria)
        {
            _row++;
            _searchList.Add(new FindRecordsTO(searchCriteria, searchType, _row));
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
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}
