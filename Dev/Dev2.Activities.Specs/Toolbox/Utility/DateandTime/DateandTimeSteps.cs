using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.DateandTime
{
    [Binding]
    public class DateandTimeSteps : RecordSetBases
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

            string inputDate;
            ScenarioContext.Current.TryGetValue("inputDate", out inputDate);
            string inputFormat;
            ScenarioContext.Current.TryGetValue("inputFormat", out inputFormat);
            string outputFormat;
            ScenarioContext.Current.TryGetValue("outputFormat", out outputFormat);
            string timeModifierType;
            ScenarioContext.Current.TryGetValue("timeModifierType", out timeModifierType);
            string timeModifierAmount;
            ScenarioContext.Current.TryGetValue("timeModifierAmount", out timeModifierAmount);

            var dateTime = new DsfDateTimeActivity
                {
                    Result = ResultVariable,
                    DateTime = inputDate,
                    InputFormat = inputFormat,
                    OutputFormat = outputFormat,
                    TimeModifierType = timeModifierType,
                    TimeModifierAmountDisplay = timeModifierAmount
                };

            TestStartNode = new FlowStep
                {
                    Action = dateTime
                };
        }


        [Given(@"I have a date ""(.*)""")]
        public void GivenIHaveADate(string inputDate)
        {
            ScenarioContext.Current.Add("inputDate", inputDate);
        }

        [Given(@"the input format as ""(.*)""")]
        public void GivenTheInputFormatAs(string inputFormat)
        {
            ScenarioContext.Current.Add("inputFormat", inputFormat);
        }

        [Given(@"I selected Add time as ""(.*)"" with a value of (.*)")]
        public void GivenISelectedAddTimeAsWithAValueOf(string timeModifierType, string timeModifierAmount)
        {
            ScenarioContext.Current.Add("timeModifierType", timeModifierType);
            ScenarioContext.Current.Add("timeModifierAmount", timeModifierAmount);
        }

        [Given(@"the output format as ""(.*)""")]
        public void GivenTheOutputFormatAs(string outputFormat)
        {
            ScenarioContext.Current.Add("outputFormat", outputFormat);
        }

        [When(@"the datetime tool is executed")]
        public void WhenTheDatetimeToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException: false);
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