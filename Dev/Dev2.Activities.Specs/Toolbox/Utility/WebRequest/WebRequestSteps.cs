using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.WebRequest
{
    [Binding]
    public class WebRequestSteps : RecordSetBases
    {
        public WebRequestSteps()
            : base(new List<Tuple<string, string>>())
        {
        }
        
        private DsfWebGetRequestActivity _webGet;
        private string _url;
        private string _header;

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _webGet = new DsfWebGetRequestActivity
            {
                Result = ResultVariable,
                Url = _url,
                Headers = _header
            };

            TestStartNode = new FlowStep
            {
                Action = _webGet
            };
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

        [Given(@"I have a web request variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAWebRequestVariableEqualTo(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have the Header ""(.*)""")]
        public void GivenIHaveTheHeader(string header)
        {
            _header = header;
        }

        
        [Then(@"the result should contain the string ""(.*)""")]
        public void ThenTheResultShouldContainTheString(string result)
        {
            string error;
            string actualValue;
            result = result.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.IsTrue(actualValue.Contains(result));
        }
    }
}
