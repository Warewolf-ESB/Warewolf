using System;
using System.Activities.Statements;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.FormatNumber
{
    [Binding]
    public class FormatNumberSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfNumberFormatActivity _numberFormat;
        private IDSFDataObject _result;
        private decimal _number;
        private string _roundingType;
        private int _roundingDecimalPlaces;
        private int _decimalToShow;

        private void BuildDataList()
        {
            _numberFormat = new DsfNumberFormatActivity
            {
                Result = ResultVariable,
                Expression =  _number.ToString(),
                RoundingType = _roundingType,
                RoundingDecimalPlaces = _roundingDecimalPlaces.ToString(),
                DecimalPlacesToShow = _decimalToShow.ToString()
            };

            TestStartNode = new FlowStep
            {
                Action = _numberFormat
            };


            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have a number (.*)")]
        public void GivenIHaveANumber(Decimal number)
        {
            _number = number;
        }
        
        [Given(@"I selected rounding ""(.*)"" to (.*)")]
        public void GivenISelectedRoundingTo(string rounding, int to)
        {
            _roundingType = rounding;
            _roundingDecimalPlaces = to;
        }
        
        [Given(@"I want to show (.*) decimals")]
        public void GivenIWantToShowDecimals(int decimalToShow)
        {
            _decimalToShow = decimalToShow;
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
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}
