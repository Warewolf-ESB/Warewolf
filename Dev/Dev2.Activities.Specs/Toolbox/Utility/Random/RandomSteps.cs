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
        private DsfRandomActivity _dsfRandom;
        private const string ResultVariable = "[[result]]";
        private IDSFDataObject _result;
        private enRandomType _randomType;
        private string _length;
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
            _randomType = (enRandomType)Enum.Parse(typeof(enRandomType), randomType);
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
            _result = ExecuteProcess();
        }
        
        [Then(@"the result from the random tool should be of type ""(.*)"" with a length of ""(.*)""")]
        public void ThenTheResultFromTheRandomToolShouldBeOfTypeWithALengthOf(string type, int length)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            var converter = TypeDescriptor.GetConverter(Type.GetType(type));
            var res = converter.ConvertFrom(actualValue);
            Assert.AreEqual(length , actualValue.Length);
        }

        [Then(@"the random value will be ""(.*)""")]
        public void ThenTheRandomValueWillBe(string value)
        {
            string error;
            string actualValue;
            value = value.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"random execution has ""(.*)"" error")]
        public void ThenRandomExecutionHasError(string anError)
        {
            var expected = anError.Equals("AN");
            var actual = !string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            Assert.AreEqual(expected, actual);
        }

    }
}
