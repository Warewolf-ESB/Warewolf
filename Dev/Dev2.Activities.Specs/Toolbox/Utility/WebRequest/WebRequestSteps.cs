using System;
using System.Activities.Statements;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.WebRequest
{
    [Binding]
    public class WebRequestSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfWebGetRequestActivity _webGet;
        private IDSFDataObject _result;
        private string _url;

        private void BuildDataList()
        {
            _webGet = new DsfWebGetRequestActivity
            {
                Result = ResultVariable,
                Url = _url
            };

            TestStartNode = new FlowStep
            {
                Action = _webGet
            };

            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have the url ""(.*)""")]
        public void GivenIHaveTheUrl(string url)
        {
            _url = url;
        }
        
        [When(@"the web request tool is executed")]
        public void WhenTheWebRequestToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the result should have the string ""(.*)""")]
        public void ThenTheResultShouldHaveTheString(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }
    }
}
