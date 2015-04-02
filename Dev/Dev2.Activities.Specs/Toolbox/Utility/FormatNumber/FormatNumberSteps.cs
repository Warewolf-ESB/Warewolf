
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
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.FormatNumber
{
    [Binding]
    public class FormatNumberSteps : RecordSetBases
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

            string number;
            ScenarioContext.Current.TryGetValue("number", out number);
            string roundingType;
            ScenarioContext.Current.TryGetValue("rounding", out roundingType);
            string roundingDecimalPlaces;
            ScenarioContext.Current.TryGetValue("to", out roundingDecimalPlaces);
            string decimalToShow;
            ScenarioContext.Current.TryGetValue("decimalToShow", out decimalToShow);

            var numberFormat = new DsfNumberFormatActivity
                {
                    Result = ResultVariable,
                    Expression = number,
                    RoundingType = roundingType,
                    RoundingDecimalPlaces = roundingDecimalPlaces,
                    DecimalPlacesToShow = decimalToShow
                };

            TestStartNode = new FlowStep
                {
                    Action = numberFormat
                };
            ScenarioContext.Current.Add("activity", numberFormat);
        }

        [Given(@"I have a number (.*)")]
        public void GivenIHaveANumber(string number)
        {
            ScenarioContext.Current.Add("number", number.Replace('"', ' ').Trim());
        }

        [Given(@"I selected rounding ""(.*)"" to (.*)")]
        public void GivenISelectedRoundingTo(string rounding, string to)
        {
            ScenarioContext.Current.Add("rounding", rounding.Replace('"', ' ').Trim());
            ScenarioContext.Current.Add("to", to.Replace('"', ' ').Trim());
        }

        [Given(@"I want to show (.*) decimals")]
        public void GivenIWantToShowDecimals(string decimalToShow)
        {
            ScenarioContext.Current.Add("decimalToShow", decimalToShow.Replace('"', ' ').Trim());
        }

        [Given(@"I have a formatnumber variable ""(.*)"" equal to (.*)")]
        public void GivenIHaveAFormatnumberVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            value = value.Replace('"', ' ').Trim();
            variable = variable.Replace('"', ' ').Trim();
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the format number is executed")]
        public void WhenTheFormatNumberIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result (.*) will be returned")]
        public void ThenTheResultWillBeReturned(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
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
    }
}
