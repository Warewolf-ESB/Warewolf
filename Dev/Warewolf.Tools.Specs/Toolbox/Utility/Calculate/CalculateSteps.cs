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
using Dev2.Data.Util;
using Dev2.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.Calculate
{
    [Binding]
    public class CalculateSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public CalculateSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            BuildInternal(false);
        }

        private void BuildInternal(bool isAggregate)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            var resultVariable = ResultVariable;
            string resVar;
            if(scenarioContext.TryGetValue("resVar", out resVar))
            {
                resultVariable = resVar;
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
            BuildShapeAndTestData();

            string formula;
            scenarioContext.TryGetValue("formula", out formula);
            if (isAggregate)
            {
                var calculate = new DsfAggregateCalculateActivity
                {
                    Result = resultVariable,
                    Expression = formula
                };
                TestStartNode = new FlowStep
                {
                    Action = calculate
                };
                scenarioContext.Add("activity", calculate);
            }
            else
            {
                var calculate = new DsfCalculateActivity
                {
                    Result = resultVariable,
                    Expression = formula
                };
                TestStartNode = new FlowStep
                {
                    Action = calculate
                };
                scenarioContext.Add("activity", calculate);
            }
            
        }

        [Given(@"I have the formula ""(.*)""")]
        public void GivenIHaveTheFormula(string formula)
        {
            scenarioContext.Add("formula", formula);
        }

        [When(@"the aggregate calculate tool is executed")]
        public void WhenTheAggregateCalculateToolIsExecuted()
        {
            BuildInternal(true);
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [When(@"the calculate tool is executed")]
        public void WhenTheCalculateToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"I have a calculate variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveACalculateVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }


        [Given(@"I have a calculate variable ""(.*)"" equal to")]
        public void GivenIHaveACalculateVariableEqualTo(string recordset, Table table)
        {
            List<TableRow> tableRows = table.Rows.ToList();
            for (int i = 0; i < tableRows.Count; i++)
            {
                List<Tuple<string, string>> variableList;
                scenarioContext.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(recordset, tableRows[i][0]));
            }
        }

        [Then(@"the calculate result should be ""(.*)""")]
        public void ThenTheCalculateResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);

            if (expectedResult == "[Now]")
            {

                DateTime.Parse(actualValue).Subtract(DateTime.Now).TotalMilliseconds.Should().BeLessThan(1000, "we expect DateTime.Now but give a 1 second grace period for the formula to complete");
                return;
            }
            if (expectedResult == "[Today]")
            {

                DateTime.Parse(actualValue).Day.Should().Be(DateTime.Now.Day, "we expect today's date");
                return;
            }
            if (expectedResult == "[Int]")
            {
                int outval;
                Assert.IsTrue(int.TryParse(actualValue, out outval));
                return;
            }
            if (string.IsNullOrEmpty(expectedResult))
            {
                actualValue.Should().BeNullOrEmpty("the expected value is null or empty");
            }
            else
            {
                actualValue.Should().Be(expectedResult);
            }
        }

        [Then(@"the calculate result should be null")]
        public void ThenTheCalculateResultShouldBeNull()
        {
        }

        [Given(@"I have the Example formula ""(.*)""")]
        public void GivenIHaveTheExampleFormula(string formula)
        {
            scenarioContext.Add("formula", formula);

        }


        [Then(@"the example output = ""(.*)""")]
        public void ThenTheExampleOutput(int p0)
        {
            scenarioContext.Pending();
        }

        [Given(@"calculate result as ""(.*)""")]
        public void GivenCalculateResultAs(string p0)
        {
            scenarioContext.Add("resVar", p0);
        }

    }
}
