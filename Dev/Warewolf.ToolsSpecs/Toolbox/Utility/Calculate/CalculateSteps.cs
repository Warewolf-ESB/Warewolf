
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
using Dev2.Data.Util;
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
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string formula;
            ScenarioContext.Current.TryGetValue("formula", out formula);

            var calculate = new DsfCalculateActivity
                {
                    Result = ResultVariable,
                    Expression = formula
                };

            TestStartNode = new FlowStep
                {
                    Action = calculate
                };
            ScenarioContext.Current.Add("activity", calculate);
        }

        [Given(@"I have the formula ""(.*)""")]
        public void GivenIHaveTheFormula(string formula)
        {
            ScenarioContext.Current.Add("formula", formula);
        }

        [When(@"the calculate tool is executed")]
        public void WhenTheCalculateToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Given(@"I have a calculate variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveACalculateVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
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
                ScenarioContext.Current.TryGetValue("variableList", out variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    ScenarioContext.Current.Add("variableList", variableList);
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
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
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

        [Given(@"I have the Example formula '(.*)'")]
        public void GivenIHaveTheExampleFormula(string formula)
        {
            ScenarioContext.Current.Add("formula", formula);

        }


        [Then(@"the example output = '(.*)'")]
        public void ThenTheExampleOutput(int p0)
        {
            ScenarioContext.Current.Pending();
        }


    }
}
