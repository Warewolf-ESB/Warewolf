
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Data.Replace
{
    [Binding]
    public class ReplaceSteps : RecordSetBases
    {
        private string _inFields = "[[sentence]]";

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            string find;
            ScenarioContext.Current.TryGetValue("find", out find);
            string replaceWith;
            ScenarioContext.Current.TryGetValue("replaceWith", out replaceWith);

            string resultVar;
            ScenarioContext.Current.TryGetValue("resultVar", out resultVar);

            if (string.IsNullOrEmpty(resultVar))
            {
                resultVar = ResultVariable;
            }

            string sentence;
            if(ScenarioContext.Current.TryGetValue("sentence", out sentence))
            {
                _inFields = sentence;
            }

            var replace = new DsfReplaceActivity
                {
                    Result = resultVar,
                    FieldsToSearch = _inFields,
                    Find = find,
                    ReplaceWith = replaceWith
                };

            TestStartNode = new FlowStep
                {
                    Action = replace
                };
            ScenarioContext.Current.Add("activity", replace);
        }

        [Given(@"I have a sentence ""(.*)""")]
        public void GivenIHaveASentence(string sentence)
        {
            ScenarioContext.Current.Add("sentence", sentence);
        }

        [Given(@"replace result is ""(.*)""")]
        public void GivenReplaceResultIs(string resultVar)
        {
            ScenarioContext.Current.Add("resultVar", resultVar);
        }


        [Given(@"I have a replace variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAReplaceVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I want to find the characters ""(.*)""")]
        public void GivenIWantToFindTheCharacters(string find)
        {
            ScenarioContext.Current.Add("find", find);
        }

        [Given(@"I want to replace them with ""(.*)""")]
        public void GivenIWantToReplaceThemWith(string replaceWith)
        {
            ScenarioContext.Current.Add("replaceWith", replaceWith);
        }

        [When(@"the replace tool is executed")]
        public void WhenTheReplaceToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the replace result should be ""(.*)""")]
        public void ThenTheReplaceResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"""(.*)"" should be ""(.*)""")]
        public void ThenShouldBe(string variable, string value)
        {
            string error;
            string actualValue;
            value = value.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, variable,
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
