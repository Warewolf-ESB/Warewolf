/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Warewolf.ToolsSpecs.Toolbox.Recordset
{
    [Binding]
    public class CountSteps : RecordSetBases
    {
        readonly ScenarioContext scenarioContext;

        public CountSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException(nameof(scenarioContext));
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

            scenarioContext.TryGetValue("resultVariable", out string resultVariable);

            BuildShapeAndTestData();

            scenarioContext.TryGetValue("recordset", out string recordSetName);

            var recordset = scenarioContext.Get<string>("recordset");
            scenarioContext.TryGetValue("treaNullAsZero", out bool treaNullAsZero);
            scenarioContext.TryGetValue("activityMode", out DsfActivityAbstract<string> count);
            if (count != null)
            {
                count = new DsfCountRecordsetNullHandlerActivity
                {
                    RecordsetName = recordset,
                    CountNumber = string.IsNullOrEmpty(resultVariable) ? ResultVariable : resultVariable,
                    TreatNullAsZero = treaNullAsZero
                };
            }
            else
            {
                count = new DsfCountRecordsetActivity
                {
                    RecordsetName = recordset,
                    CountNumber = string.IsNullOrEmpty(resultVariable) ? ResultVariable : resultVariable
                };
            }

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
            var tableRows = table.Rows.ToList();

            if (tableRows.Count == 0)
            {
                var rs = table.Header.ToArray()[0];


                var isAdded = scenarioContext.TryGetValue("rs", out List<Tuple<string, string>> emptyRecordset);
                if (!isAdded)
                {
                    emptyRecordset = new List<Tuple<string, string>>();
                    scenarioContext.Add("rs", emptyRecordset);
                }
                emptyRecordset.Add(new Tuple<string, string>(rs, "row"));
            }

            foreach (TableRow t in tableRows)
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

        [When(@"the count tool is executed")]
        public void WhenTheCountToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the result count should be (.*)")]
        public void ThenTheResultCountShouldBe(string expectedResult)
        {
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            GetScalarValueFromEnvironment(DataObject.Environment, ResultVariable,
                                       out string actualValue, out string error);
            actualValue = string.IsNullOrEmpty(actualValue) ? "0" : actualValue;
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Given(@"treat null as Empty Recordset is selected")]
        public void GivenTreatNullAsEmptyRecordsetIsSelected()
        {
            scenarioContext.Add("treaNullAsZero", true);
        }

        [Given(@"treat null as Empty Recordset is not selected")]
        public void GivenTreatNullAsEmptyRecordsetIsNotSelected()
        {
            scenarioContext.Add("treaNullAsZero", false);
        }

        [Given(@"this feature")]
        public void GivenThisFeature()
        {

        }
        [Then(@"activity is DsfCountRecordsetNullHandlerActivity")]
        public void ThenActivityIsDsfCountRecordsetNullHandlerActivity()
        {
            scenarioContext.Add("activityMode", new DsfCountRecordsetNullHandlerActivity());
        }




    }
}
