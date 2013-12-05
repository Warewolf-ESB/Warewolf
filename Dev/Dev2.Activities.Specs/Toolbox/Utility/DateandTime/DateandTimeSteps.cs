using System;
using System.Activities.Statements;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.DateandTime
{
    [Binding]
    public class DateandTimeSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfDateTimeActivity _dateTime;
        private IDSFDataObject _result;

        private string _inputDate;
        private string _inputFormat;
        private string _timeModifierType;
        private int _timeModifierAmount;
        private string _outputFormat;

        private void BuildDataList()
        {
            _dateTime = new DsfDateTimeActivity
            {
                Result = ResultVariable,
                DateTime = _inputDate ,
                InputFormat = _inputFormat, 
                OutputFormat = _outputFormat,
                TimeModifierType = _timeModifierType,
                TimeModifierAmountDisplay = _timeModifierAmount.ToString()
            };

            TestStartNode = new FlowStep
            {
                Action = _dateTime
            };


            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }


        [Given(@"I have a date ""(.*)""")]
        public void GivenIHaveADate(string inputDate)
        {
            _inputDate = inputDate;
        }
        
        [Given(@"the input format as ""(.*)""")]
        public void GivenTheInputFormatAs(string inputFormat)
        {
            _inputFormat = inputFormat;
        }
        
        [Given(@"I selected Add time as ""(.*)"" with a value of ""(.*)""")]
        public void GivenISelectedAddTimeAsWithAValueOf(string datePart, int value)
        {
            _timeModifierType = datePart;
            _timeModifierAmount = value;
        }
        
        [Given(@"the output format as ""(.*)""")]
        public void GivenTheOutputFormatAs(string outputFormat)
        {
            _outputFormat = outputFormat;
        }
        
        [When(@"the datetime tool is executed")]
        public void WhenTheDatetimeToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the date should be ""(.*)""")]
        public void ThenTheDateShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }

    }
}
