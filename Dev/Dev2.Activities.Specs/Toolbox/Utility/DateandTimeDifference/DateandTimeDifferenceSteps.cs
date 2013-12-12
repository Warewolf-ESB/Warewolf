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

            string inputFormat;
            ScenarioContext.Current.TryGetValue("inputFormat", out inputFormat);
            string input1;
            ScenarioContext.Current.TryGetValue("input1", out input1);
            string input2;
            ScenarioContext.Current.TryGetValue("input2", out input2);
            string outputIn;
            ScenarioContext.Current.TryGetValue("outputIn", out outputIn);
            
            var dateTimeDifference = new DsfDateTimeDifferenceActivity
                {
                    Result = ResultVariable,
                    InputFormat = inputFormat,
                    Input1 = input1,
                    Input2 = input2,
                    OutputType = outputIn
                };

            TestStartNode = new FlowStep
                {
                    Action = dateTimeDifference
                };
        }

        [Given(@"I have a first date ""(.*)""")]
        public void GivenIHaveAFirstDate(string input1)
        {
            ScenarioContext.Current.Add("input1", input1);
        }

        [Given(@"I have a second date ""(.*)""")]
        public void GivenIHaveASecondDate(string input2)
        {
            ScenarioContext.Current.Add("input2", input2);
        }

        [Given(@"I selected output in ""(.*)""")]
        public void GivenISelectedOutputIn(string outputIn)
        {
            ScenarioContext.Current.Add("outputIn", outputIn);
        }

        [Given(@"the date format as ""(.*)""")]
        public void GivenTheDateFormatAs(string inputFormat)
        {
            ScenarioContext.Current.Add("inputFormat", inputFormat);
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
            IDSFDataObject result = ExecuteProcess(throwException:false);
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
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError,
                                           actual ? "did not occur" : "did occur" + fetchErrors);
            Assert.AreEqual(expected, actual, message);
        }
    }
}