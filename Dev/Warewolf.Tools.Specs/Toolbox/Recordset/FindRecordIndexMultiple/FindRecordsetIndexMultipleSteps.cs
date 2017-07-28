/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.ToolsSpecs.Toolbox.Recordset.FindRecordIndexMultiple
{
    [Binding]
    public class FindRecordsetIndexMultipleSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public FindRecordsetIndexMultipleSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string fieldsToSearch;
            scenarioContext.TryGetValue("fieldsToSearch", out fieldsToSearch);
            List<FindRecordsTO> searchList;
            scenarioContext.TryGetValue("searchList", out searchList);
            bool requireAllTrue;
            scenarioContext.TryGetValue("requireAllTrue", out requireAllTrue);
            bool requireAllFieldsToMatch;
            scenarioContext.TryGetValue("requireAllFieldsToMatch", out requireAllFieldsToMatch);

            var findRecordsMultipleIndex = new DsfFindRecordsMultipleCriteriaActivity
                {
                    FieldsToSearch = fieldsToSearch,
                    ResultsCollection = searchList,
                    RequireAllTrue = requireAllTrue,
                    RequireAllFieldsToMatch = requireAllFieldsToMatch,
                    Result = ResultVariable
                };
            string updatedResultField;
            if(scenarioContext.TryGetValue("specifiedResult",out updatedResultField))
            {
                if (!string.IsNullOrEmpty(updatedResultField))
                {
                    findRecordsMultipleIndex.Result = updatedResultField;
                }
            }

            TestStartNode = new FlowStep
                {
                    Action = findRecordsMultipleIndex
                };
            scenarioContext.Add("activity", findRecordsMultipleIndex);
        }

        [Then(@"the find records index multiple result should is ""(.*)""")]
        public void ThenTheFindRecordsIndexMultipleResultShouldIs(string resultVar)
        {
            scenarioContext.Add("specifiedResult", resultVar);
        }


        [Given(@"I have the following recordset to search for multiple criteria")]
        public void GivenIHaveTheFollowingRecordsetToSearchForMultipleCriteria(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            var rs = table.Header.ToArray()[0];
            var field = table.Header.ToArray()[1];

            if(tableRows.Count == 1)
            {
                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = scenarioContext.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow t in tableRows)
            {
                List<Tuple<string, string>> variableList;
                scenarioContext.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"the fields to search is")]
        public void GivenTheFieldsToSearchIs(Table table)
        {
            var fieldToSearch = table.Rows.Aggregate("", (current, tableRow) => current + tableRow["field"] + ",");
            fieldToSearch = fieldToSearch.TrimEnd(',');
            scenarioContext.Add("fieldsToSearch", fieldToSearch);
        }

        [Given(@"the fields to search is ""(.*)""")]
        [Given(@"field to search is ""(.*)""")]
        public void GivenFieldToSearchIs(string fieldToSearch)
        {
            scenarioContext.Add("fieldsToSearch", fieldToSearch);
        }


        [Given(@"I have the following recordset in my datalist")]
        public void GivenIHaveTheFollowingRecordsetInMyDatalist(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();

            if(tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = scenarioContext.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow t in tableRows)
            {
                List<Tuple<string, string>> variableList;
                scenarioContext.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
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

        int GetRowCount()
        {
            int row;
            bool rowAdded = scenarioContext.TryGetValue("row", out row);
            if(rowAdded)
            {
                scenarioContext.Add("row", row);
            }

            row++;
            return row;
        }

        [Given(@"is between search the recordset with type ""(.*)"" and criteria is ""(.*)"" and ""(.*)""")]
        public void GivenIsBetweenSearchTheRecordsetWithTypeAndCriteriaIsAnd(string searchType, string from, string to)
        {
            var row = GetRowCount();
            var searchList = GetSearchList();
            searchList.Add(new FindRecordsTO(string.Empty, searchType, row, false, from, to));
        }

        private List<FindRecordsTO> GetSearchList()
        {
            List<FindRecordsTO> searchList;
            scenarioContext.TryGetValue("searchList", out searchList);

            if(searchList == null)
            {
                searchList = new List<FindRecordsTO>();
                scenarioContext.Add("searchList", searchList);
            }
            return searchList;
        }

        [Given(@"when match all search criteria is ""(.*)""")]
        public void GivenWhenMatchAllSearchCriteriaIs(bool requireAllTrue)
        {
            scenarioContext.Add("requireAllTrue", requireAllTrue);
        }

        [Given(@"when requires all fields to match is ""(.*)""")]
        public void GivenWhenRequiresAllFieldsToMatchIs(bool requireAllFieldsToMatch)
        {
            scenarioContext.Add("requireAllFieldsToMatch", requireAllFieldsToMatch);
        }


        [When(@"the find records index multiple tool is executed")]
        public void WhenTheFindRecordsIndexMultipleToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the find records index multiple result should be (.*)")]
        public void ThenTheFindRecordsIndexMultipleResultShouldBe(string expectedResult)
        {
            string error;
            var result = scenarioContext.Get<IDSFDataObject>("result");

            if(DataListUtil.IsValueRecordset(ResultVariable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, ResultVariable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, ResultVariable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.Environment, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                
                Assert.AreEqual(recordSetValues[1], expectedResult);
            }
            else
            {
                string actualValue;
                expectedResult = expectedResult.Replace('"', ' ').Trim();
                GetScalarValueFromEnvironment(result.Environment, ResultVariable,out actualValue,out error);

                if(string.IsNullOrEmpty(expectedResult))
                {
                    Assert.AreEqual("-1",actualValue);
                }
                else
                {
                    Assert.AreEqual(expectedResult, actualValue);
                }
            }
        }
    }
}
