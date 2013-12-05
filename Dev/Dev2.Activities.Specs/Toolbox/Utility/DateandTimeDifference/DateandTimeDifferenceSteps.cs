using System;
using System.Activities.Statements;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.DateandTimeDifference
{
    [Binding]
    public class DateandTimeDifferenceSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfDateTimeDifferenceActivity _dateTimeDifference;
        private IDSFDataObject _result;

        private string _inputFormat;
        private string _input1;
        private string _input2;
        private string _outputIn;

        private void BuildDataList()
        {
            _dateTimeDifference = new DsfDateTimeDifferenceActivity
            {
                Result = ResultVariable,
                InputFormat = _inputFormat,
                Input1 = _input1 , 
                Input2 = _input2 ,
                OutputType = _outputIn 
            };

            TestStartNode = new FlowStep
            {
                Action = _dateTimeDifference
            };


            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have a first date ""(.*)""")]
        public void GivenIHaveAFirstDate(string input1)
        {
            _input1 = input1;
        }
        
        [Given(@"I have a second date ""(.*)""")]
        public void GivenIHaveASecondDate(string input2)
        {
            _input2 = input2;
        }
        
        [Given(@"I selected output in ""(.*)""")]
        public void GivenISelectedOutputIn(string outputIn)
        {
            _outputIn = outputIn;
        }
        
        [Given(@"the date format as ""(.*)""")]
        public void GivenTheDateFormatAs(string inputFormat)
        {
            _inputFormat = inputFormat;
        }

        
        [When(@"the datetime difference tool is executed")]
        public void WhenTheDatetimeDifferenceToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the difference should be ""(.*)""")]
        public void ThenTheDifferenceShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}
