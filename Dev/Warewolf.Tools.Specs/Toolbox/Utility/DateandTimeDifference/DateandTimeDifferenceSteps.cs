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
using System.Globalization;
using System.Threading;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.DateandTimeDifference
{
    [Binding]
    public class DateandTimeDifferenceSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public DateandTimeDifferenceSteps(ScenarioContext scenarioContext)
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

            string resVar;
            if (!scenarioContext.TryGetValue("resultVar", out resVar))
            {
                resVar = ResultVariable;
            }

            string inputFormat;
            scenarioContext.TryGetValue("inputFormat", out inputFormat);
            string input1;
            scenarioContext.TryGetValue("input1", out input1);
            string input2;
            scenarioContext.TryGetValue("input2", out input2);
            string outputIn;
            scenarioContext.TryGetValue("outputIn", out outputIn);

            var dateTimeDifference = new DsfDateTimeDifferenceActivity
                {
                    Result = resVar,
                    InputFormat = string.IsNullOrEmpty(inputFormat) ? "" : inputFormat,
                    Input1 = string.IsNullOrEmpty(input1) ? "" : input1,
                    Input2 = string.IsNullOrEmpty(input2) ? "" : input2,
                    OutputType = outputIn
                };

            TestStartNode = new FlowStep
                {
                    Action = dateTimeDifference
                };
            scenarioContext.Add("activity", dateTimeDifference);
        }

        [Given(@"DateTimeDifference result variable is ""(.*)""")]
        public void GivenDateTimeDifferenceResultVariableIs(string p0)
        {
            scenarioContext.Add("resultVar", p0);
        }

        [Given(@"I have a first date ""(.*)""")]
        public void GivenIHaveAFirstDate(string input1)
        {
            scenarioContext.Add("input1", input1);
        }

        [Given(@"I have a second date ""(.*)""")]
        public void GivenIHaveASecondDate(string input2)
        {
            scenarioContext.Add("input2", input2);
        }

        [Given(@"I selected output in ""(.*)""")]
        public void GivenISelectedOutputIn(string outputIn)
        {
            scenarioContext.Add("outputIn", outputIn);
        }

        [Given(@"I have date time difference variable ""(.*)"" with value ""(.*)""")]
        public void GivenIHaveDateTimeDifferenceVariableWithValue(string name, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(name, value));
        }


        [Given(@"the date format as ""(.*)""")]
        public void GivenTheDateFormatAs(string inputFormat)
        {
            scenarioContext.Add("inputFormat", inputFormat);
        }

        [Given(@"I have a DateAndTimeDifference variable ""(.*)"" equal to (.*)")]
        public void GivenIHaveADateAndTimeDifferenceVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the datetime difference tool is executed")]
        public void WhenTheDatetimeDifferenceToolIsExecuted()
        {
            var currentCulture = new CultureInfo("en-ZA");
            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentCulture;
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the difference should be ""(.*)""")]
        public void ThenTheDifferenceShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            string resVar;
            if (!scenarioContext.TryGetValue("resultVar", out resVar))
            {
                resVar = ResultVariable;
            }
            GetScalarValueFromEnvironment(result.Environment, resVar,
                                       out actualValue, out error);
            if (string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(expectedResult, actualValue);
            }
        }
    }
}
