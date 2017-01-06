/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.WebRequest
{
    [Binding]
    public class WebRequestSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public WebRequestSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            var resultVariable = ResultVariable;
            string resVar;
            if(scenarioContext.TryGetValue("resVar", out resVar))
            {
                resultVariable = resVar;
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
            BuildShapeAndTestData();

            string header;
            scenarioContext.TryGetValue("header", out header);
            string url;
            scenarioContext.TryGetValue("url", out url);
            string timeout;
            scenarioContext.TryGetValue("timeoutSeconds", out timeout);
            var webGet = new DsfWebGetRequestWithTimeoutActivity
                {
                    Result = resultVariable,
                    Url = url ?? "",
                    Headers = header ?? "",
                    TimeoutSeconds = String.IsNullOrEmpty(timeout) ? 100 : int.Parse(timeout),
                    TimeOutText = String.IsNullOrEmpty(timeout) ? "" : timeout
                };

            TestStartNode = new FlowStep
                {
                    Action = webGet
                };
            scenarioContext.Add("activity", webGet);
        }

        [Given(@"I have the url ""(.*)"" without timeout")]
        public void GivenIHaveTheUrl(string url)
        {
            scenarioContext.Add("url", url);
        }

        [When(@"the web request tool is executed")]
        public void WhenTheWebRequestToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"I have a web request variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAWebRequestVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have the Header ""(.*)""")]
        public void GivenIHaveTheHeader(string header)
        {
            scenarioContext.Add("header", header);
        }


        [Then(@"the result should contain the string ""(.*)""")]
        public void ThenTheResultShouldContainTheString(string expectedResult)
        {
            string error;
            string actualValue;
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            if (string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                var failureMessage = "Result does not contain " + expectedResult;
                Assert.IsNotNull(actualValue, failureMessage);
                Assert.IsTrue(actualValue.Contains(expectedResult), failureMessage);
            }
        }

        [Given(@"I have web request result as ""(.*)""")]
        public void GivenIHaveWebRequestResultAs(string resultVar)
        {
            scenarioContext.Add("resVar", resultVar);
        }


        [Given(@"I have the url ""(.*)"" with timeoutSeconds ""(.*)""")]
        public void GivenIHaveTheUrlWithTimeoutSeconds(string url, string timeoutSeconds)
        {
            scenarioContext.Add("url", url);
            scenarioContext.Add("timeoutSeconds", timeoutSeconds);
        }



    }
}
