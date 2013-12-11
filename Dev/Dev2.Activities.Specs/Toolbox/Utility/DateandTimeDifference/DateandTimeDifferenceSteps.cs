using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.DateandTimeDifference
{
    [Binding]
    public class DateandTimeDifferenceSteps : RecordSetBases
    {
        private DsfDateTimeDifferenceActivity _dateTimeDifference;

        private string _input1;
        private string _input2;
        private string _inputFormat;
        private string _outputIn;

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

            _dateTimeDifference = new DsfDateTimeDifferenceActivity
                {
                    Result = ResultVariable,
                    InputFormat = _inputFormat,
                    Input1 = _input1,
                    Input2 = _input2,
                    OutputType = _outputIn
                };

            TestStartNode = new FlowStep
                {
                    Action = _dateTimeDifference
                };
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

        [Given(@"I have a DateAndTimeDifference variable ""(.*)"" equal to (.*)")]
        public void GivenIHaveADateAndTimeDifferenceVariableEqualTo(string variable, string value)
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

        [When(@"the datetime difference tool is executed")]
        public void WhenTheDatetimeDifferenceToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess();
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the difference should be ""(.*)""")]
        public void ThenTheDifferenceShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"datetimediff execution has ""(.*)"" error")]
        public void ThenDatetimediffExecutionHasError(string anError)
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