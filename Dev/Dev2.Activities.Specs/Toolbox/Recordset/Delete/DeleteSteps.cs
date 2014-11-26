
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Delete
{
    [Binding]
    public class DeleteSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            var recordset = ScenarioContext.Current.Get<string>("recordset");
            var delete = new DsfDeleteRecordActivity
                {
                    RecordsetName = recordset,
                    Result = ResultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = delete
                };
            ScenarioContext.Current.Add("activity", delete);
        }

        [Given(@"I have the following recordset")]
        public void GivenIHaveTheFollowingRecordset(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();

            if(tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow t in tableRows)
            {
                List<Tuple<string, string>> variableList;
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if(variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"an index ""(.*)"" exists with a value ""(.*)""")]
        public void GivenAnIndexExistsWithAValue(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            value = value.Replace('"', ' ').Trim();
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have a delete variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveADeleteVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            value = value.Replace('"', ' ').Trim();
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }


        [When(@"the delete tool is executed")]
        public void WhenTheDeleteToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the delete result should be ""(.*)""")]
        public void ThenTheDeleteResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            if(string.IsNullOrEmpty(expectedResult))
            {
                expectedResult = null;
            }
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"the recordset ""(.*)"" will be as follows")]
        public void ThenTheRecordsetWillBeAsFollows(string recordset, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            string error;
            string recordsetName = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, recordset);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, recordset);
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordsetName,
                                                                           column, out error);
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);

            for(int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Given(@"I delete a record ""(.*)""")]
        public void GivenIDeleteARecord(string recordset)
        {
            ScenarioContext.Current.Add("recordset", recordset);
        }
    }
}
