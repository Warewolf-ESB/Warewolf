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
        private const string ResultVariable = "[[result]]";
        private string _criteria;
        private DsfFindRecordsActivity _findRecordsIndex;
        private string _searchType;

        public FindRecordIndexSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));
            _findRecordsIndex = new DsfFindRecordsActivity
                {
                    FieldsToSearch = RecordSetName + "()",
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

        [Then(@"the index result should be (.*)")]
        public void ThenTheIndexResultShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}