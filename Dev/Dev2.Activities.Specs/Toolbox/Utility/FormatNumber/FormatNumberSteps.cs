using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.FormatNumber
{
    [Binding]
    public class FormatNumberSteps : RecordSetBases
    {
        private string _decimalToShow;
        private string _number;
        private DsfNumberFormatActivity _numberFormat;
        private string _roundingDecimalPlaces;
        private string _roundingType;

        public FormatNumberSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));
            _numberFormat = new DsfNumberFormatActivity
                {
                    Result = ResultVariable,
                    Expression = _number,
                    RoundingType = _roundingType,
                    RoundingDecimalPlaces = _roundingDecimalPlaces,
                    DecimalPlacesToShow = _decimalToShow
                };

            TestStartNode = new FlowStep
                {
                    Action = _numberFormat
                };
        }

        [Given(@"I have a number (.*)")]
        public void GivenIHaveANumber(string number)
        {
            _number = number;
        }

        [Given(@"I selected rounding ""(.*)"" to (.*)")]
        public void GivenISelectedRoundingTo(string rounding, string to)
        {
            _roundingType = rounding;
            _roundingDecimalPlaces = to;
        }

        [Given(@"I want to show (.*) decimals")]
        public void GivenIWantToShowDecimals(string decimalToShow)
        {
            _decimalToShow = decimalToShow;
        }

        [Given(@"I have a formatnumber variable ""(.*)"" equal to (.*)")]
        public void GivenIHaveAFormatnumberVariableEqualTo(string variable, int value)
        {
            _variableList.Add(new Tuple<string, string>(variable, string.Empty));
        }

        [When(@"the format number is executed")]
        public void WhenTheFormatNumberIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the result (.*) will be returned")]
        public void ThenTheResultWillBeReturned(string result)
        {
            string error;
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }

        [Then(@"formart number execution has ""(.*)"" error")]
        public void ThenFormartNumberExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }

    }
}