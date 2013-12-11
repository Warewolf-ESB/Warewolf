using System;
using System.Activities.Statements;
using System.ComponentModel;
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
        //private IDSFDataObject _result;

        private string _inputDate;
        private string _inputFormat;
        private string _outputFormat;
        private string _timeModifierAmount;
        private string _timeModifierType;

        private void BuildDataList()
        {
            _dateTime = new DsfDateTimeActivity
                {
                    Result = ResultVariable,
                    DateTime = _inputDate,
                    InputFormat = _inputFormat,
                    OutputFormat = _outputFormat,
                    TimeModifierType = _timeModifierType,
                    TimeModifierAmountDisplay = _timeModifierAmount
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

        [Given(@"I selected Add time as ""(.*)"" with a value of (.*)")]
        public void GivenISelectedAddTimeAsWithAValueOf(string datePart, string value)
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
            IDSFDataObject result = ExecuteProcess();
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the datetime result should be a ""(.*)""")]
        public void ThenTheDatetimeResultShouldBeA(string type)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            TypeConverter converter = TypeDescriptor.GetConverter(Type.GetType(type));
            object res = converter.ConvertFrom(actualValue);
        }


        [Then(@"the datetime result should be ""(.*)""")]
        public void ThenTheDatetimeResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            expectedResult = expectedResult.Replace("\"\"", "");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"datetime execution has ""(.*)"" error")]
        public void ThenDatetimeExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            bool actual = string.IsNullOrEmpty(FetchErrors(result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}