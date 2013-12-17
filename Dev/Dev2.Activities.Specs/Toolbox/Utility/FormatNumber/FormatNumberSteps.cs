using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.FormatNumber
{
    [Binding]
    public class FormatNumberSteps : RecordSetBases
    {
        private void BuildDataList()
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

            if (variableList == null)
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
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result (.*) will be returned")]
        public void ThenTheResultWillBeReturned(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"formart number execution has ""(.*)"" error")]
        public void ThenFormartNumberExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError.ToLower(),
                                           actual ? "did not occur" : "did occur" + fetchErrors);
             Assert.IsTrue(expected == actual, message);
        }
    }
}