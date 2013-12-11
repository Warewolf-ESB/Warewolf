using System;
using System.Activities.Statements;
using System.ComponentModel;
using System.Text;
using ActivityUnitTests;
using Dev2.Common.Enums;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.Random
{
    [Binding]
    public class RandomSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfRandomActivity _dsfRandom;
        //private IDSFDataObject _result;
        private string _length;
        private enRandomType _randomType;
        private string _rangeFrom;
        private string _rangeTo;

        private void BuildDataList()
        {
            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            _dsfRandom = new DsfRandomActivity
                {
                    RandomType = _randomType,
                    Result = ResultVariable
                };

            if (!string.IsNullOrEmpty(_length))
            {
                _dsfRandom.Length = _length;
            }

            if (!string.IsNullOrEmpty(_rangeFrom))
            {
                _dsfRandom.From = _rangeFrom;
            }

            if (!string.IsNullOrEmpty(_rangeTo))
            {
                _dsfRandom.To = _rangeTo;
            }

            TestStartNode = new FlowStep
                {
                    Action = _dsfRandom
                };

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }


        [Given(@"I have a type as ""(.*)""")]
        public void GivenIHaveATypeAs(string randomType)
        {
            _randomType = (enRandomType) Enum.Parse(typeof (enRandomType), randomType);
        }

        [Given(@"I have a length as ""(.*)""")]
        public void GivenIHaveALengthAs(string length)
        {
            _length = length;
        }

        [Given(@"I have a range from ""(.*)"" to ""(.*)""")]
        public void GivenIHaveARangeFromTo(string rangeFrom, string rangeTo)
        {
            _rangeFrom = rangeFrom;
            _rangeTo = rangeTo;
        }


        [When(@"the random tool is executed")]
        public void WhenTheRandomToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess();
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result from the random tool should be of type ""(.*)"" with a length of ""(.*)""")]
        public void ThenTheResultFromTheRandomToolShouldBeOfTypeWithALengthOf(string type, int length)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            TypeConverter converter = TypeDescriptor.GetConverter(Type.GetType(type));
            object res = converter.ConvertFrom(actualValue);
            Assert.AreEqual(length, actualValue.Length);
        }

        [Then(@"the random value will be ""(.*)""")]
        public void ThenTheRandomValueWillBe(string value)
        {
            string error;
            string actualValue;
            value = value.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"random execution has ""(.*)"" error")]
        public void ThenRandomExecutionHasError(string anError)
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