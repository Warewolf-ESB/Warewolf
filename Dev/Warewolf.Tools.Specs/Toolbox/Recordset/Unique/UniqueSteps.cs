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
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.ToolsSpecs.Toolbox.Recordset.Unique
{
    [Binding]
    public class UniqueSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;

        public UniqueSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("inField", out string inField);
            scenarioContext.TryGetValue("returnField", out string returnField);
            scenarioContext.TryGetValue("resultVariable", out string resultVariable);

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
            scenarioContext.Add("activity", unique);
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

        void FillDataset(Table table)
        {
            var tableRows = table.Rows.ToList();

            if (tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];
                var field = table.Header.ToArray()[1];


                var isAdded = scenarioContext.TryGetValue("rs", out List<Tuple<string, string>> emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, field));
            }

            foreach(TableRow t in tableRows)
            {
                scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(t[0], t[1]));
            }
        }

        [Given(@"I want to find unique in field ""(.*)"" with the return field ""(.*)""")]
        public void GivenIWantToFindUniqueInFieldWithTheReturnField(string inField, string returnField)
        {
            scenarioContext.Add("inField", inField);
            scenarioContext.Add("returnField", returnField);
        }

        [Given(@"The result variable is ""(.*)""")]
        public void GivenTheResultVariableIs(string resultVariable)
        {
            scenarioContext.Add("resultVariable", resultVariable);

            //List<Tuple<string, string>> variableList;
            //scenarioContext.TryGetValue("variableList", out variableList);

            //if(variableList == null)
            //{
            //    variableList = new List<Tuple<string, string>>();
            //    scenarioContext.Add("variableList", variableList);
            //}
            //variableList.Add(new Tuple<string, string>(resultVariable, ""));
        }

        [When(@"the unique tool is executed")]
        public void WhenTheUniqueToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the unique result will be")]
        public void ThenTheUniqueResultWillBe(Table table)
        {
       
            var result = scenarioContext.Get<IDSFDataObject>("result");
            var resultVariable = scenarioContext.Get<string>("resultVariable");

            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, resultVariable);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, resultVariable);

            //string error;

            var recordSetValues = result.Environment.EvalAsListOfStrings(result.Environment.ToStar(resultVariable), 0);

            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            var tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for(int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }
    }
}
