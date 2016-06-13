
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Recordset.Count
{
    [Binding]
    public class CountSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public CountSteps(ScenarioContext scenarioContext)
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

            string resultVariable;
            scenarioContext.TryGetValue("resultVariable", out resultVariable);

            BuildShapeAndTestData();

            string recordSetName;
            scenarioContext.TryGetValue("recordset", out recordSetName);
            
            var recordset = scenarioContext.Get<string>("recordset");

            var count = new DsfCountRecordsetActivity
                {
                    RecordsetName = recordset,
                    CountNumber = string.IsNullOrEmpty(resultVariable) ? ResultVariable : resultVariable
                };

            TestStartNode = new FlowStep
                {
                    Action = count
                };
            scenarioContext.Add("activity", count);
        }

        [Given(@"count on record ""(.*)""")]
        public void GivenCountOnRecord(string recordset)
        {
            scenarioContext.Add("recordset", recordset);
        }

        [Given(@"I have a recordset with this shape")]
        public void GivenIHaveARecordsetWithThisShape(Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();

            if(tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];

                List<Tuple<string, string>> emptyRecordset;

                bool isAdded = scenarioContext.TryGetValue("rs", out emptyRecordset);
                if(!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                     scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, "row"));
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

        [Given(@"I have a complex type with this shape")]
        public void GivenIHaveAComplexTypeWithThisShape()
        {
            throw new NotImplementedException("This step definition is not yet implemented and is required for this test to pass. - Ashley");
        }

        [When(@"the count tool is executed")]
        public void WhenTheCountToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the result count should be (.*)")]
        public void ThenTheResultCountShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            GetScalarValueFromEnvironment(DataObject.Environment, ResultVariable,
                                       out actualValue, out error);
            actualValue = string.IsNullOrEmpty(actualValue) ? "0" : actualValue;
            Assert.AreEqual(expectedResult, actualValue);
        }
    }
}
