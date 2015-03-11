
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Unique
{
    [Binding]
    public class UniqueSteps : RecordSetBases
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

            string inField;
            ScenarioContext.Current.TryGetValue("inField", out inField);
            string returnField;
            ScenarioContext.Current.TryGetValue("returnField", out returnField);
            string resultVariable;
            ScenarioContext.Current.TryGetValue("resultVariable", out resultVariable);

            var unique = new DsfUniqueActivity
                {
                    InFields = inField,
                    ResultFields = returnField,
                    Result = resultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = unique
                };
            ScenarioContext.Current.Add("activity", unique);
        }

        [Given(@"I have the following empty recordset")]
        public void GivenIHaveTheFollowingEmptyRecordset(Table table)
        {
            FillDataset(table);
        }

        [Given(@"I have the following duplicated recordset")]
        public void GivenIHaveTheFollowingDuplicatedRecordset(Table table)
        {
            FillDataset(table);
        }

        private static void FillDataset(Table table)
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

        [Given(@"I want to find unique in field ""(.*)"" with the return field ""(.*)""")]
        public void GivenIWantToFindUniqueInFieldWithTheReturnField(string inField, string returnField)
        {
            ScenarioContext.Current.Add("inField", inField);
            ScenarioContext.Current.Add("returnField", returnField);
        }

        [Given(@"The result variable is ""(.*)""")]
        public void GivenTheResultVariableIs(string resultVariable)
        {
            ScenarioContext.Current.Add("resultVariable", resultVariable);

            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
        }

        [When(@"the unique tool is executed")]
        public void WhenTheUniqueToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the unique result will be")]
        public void ThenTheUniqueResultWillBe(Table table)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string resultVariable = ScenarioContext.Current.Get<string>("resultVariable");

            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, resultVariable);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, resultVariable);

            //string error;

            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                           out error);

            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for(int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }
    }
}
