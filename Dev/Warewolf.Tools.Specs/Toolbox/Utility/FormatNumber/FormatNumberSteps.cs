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
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.FormatNumber
{
    [Binding]
    public class FormatNumberSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public FormatNumberSteps(ScenarioContext scenarioContext)
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

            var resultVariable = ResultVariable;
            string resVar;
            if (scenarioContext.TryGetValue("resVar", out resVar))
            {
                resultVariable = resVar;
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
            BuildShapeAndTestData();

            string number;
            scenarioContext.TryGetValue("number", out number);
            string roundingType;
            scenarioContext.TryGetValue("rounding", out roundingType);
            string roundingDecimalPlaces;
            scenarioContext.TryGetValue("to", out roundingDecimalPlaces);
            string decimalToShow;
            scenarioContext.TryGetValue("decimalToShow", out decimalToShow);

            var numberFormat = new DsfNumberFormatActivity
                {
                    Result = resultVariable,
                    Expression = number,
                    RoundingType = roundingType,
                    RoundingDecimalPlaces = roundingDecimalPlaces,
                    DecimalPlacesToShow = decimalToShow
                };

            TestStartNode = new FlowStep
                {
                    Action = numberFormat
                };
            scenarioContext.Add("activity", numberFormat);
        }

        [Given(@"I have a number (.*)")]
        public void GivenIHaveANumber(string number)
        {
            scenarioContext.Add("number", number.Replace('"', ' ').Trim());
        }

        [Given(@"I selected rounding ""(.*)"" to (.*)")]
        public void GivenISelectedRoundingTo(string rounding, string to)
        {
            scenarioContext.Add("rounding", rounding.Replace('"', ' ').Trim());
            scenarioContext.Add("to", to.Replace('"', ' ').Trim());
        }

        [Given(@"I want to show (.*) decimals with value ""(.*)""")]
        public void GivenIWantToShowDecimalsWithValue(string p0, string decimalToShow)
        {
            scenarioContext.Add("decimalToShow", decimalToShow.Replace('"', ' ').Trim());
        }

        [Given(@"I want to show ""(.*)"" decimals with values ""(.*)""")]
        public void GivenIWantToShowDecimalsWithValues(string p0, string decimalToShow)
        {
            scenarioContext.Add("decimalToShow", decimalToShow.Replace('"', ' ').Trim());
        }

        [Given(@"I have a formatnumber variable ""(.*)"" equal to (.*)")]
        public void GivenIHaveAFormatnumberVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            value = value.Replace('"', ' ').Trim();
            variable = variable.Replace('"', ' ').Trim();
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the format number is executed")]
        public void WhenTheFormatNumberIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the result (.*) will be returned")]
        public void ThenTheResultWillBeReturned(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            if(string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(expectedResult, actualValue);
            }
        }

        [Given(@"I have a formatnumber result is ""(.*)""")]
        public void GivenIHaveAFormatnumberResultIs(string resultVar)
        {
            scenarioContext.Add("resVar", resultVar);
        }

    }
}
