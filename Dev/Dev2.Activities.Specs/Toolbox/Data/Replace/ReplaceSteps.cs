using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.Replace
{
    [Binding]
    public class ReplaceSteps : RecordSetBases
    {
        private const string InFields = "[[sentence]]";
        private string _find;
        private DsfReplaceActivity _replace;
        private string _replaceWith;
        private string _sentence;

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            variableList.Add(new Tuple<string, string>(InFields, _sentence));
            variableList.Add(new Tuple<string, string>(ResultVariable, ""));

            BuildShapeAndTestData();

            _replace = new DsfReplaceActivity
                {
                    Result = ResultVariable,
                    FieldsToSearch = InFields,
                    Find = _find,
                    ReplaceWith = _replaceWith
                };

            TestStartNode = new FlowStep
                {
                    Action = _replace
                };
        }

        [Given(@"I have a sentence ""(.*)""")]
        public void GivenIHaveASentence(string sentence)
        {
            _sentence = sentence;
        }

        [Given(@"I have a replace variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAReplaceVariableEqualTo(string variable, string value)
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

        [Given(@"I want to find the characters ""(.*)""")]
        public void GivenIWantToFindTheCharacters(string find)
        {
            _find = find;
        }

        [Given(@"I want to replace them with ""(.*)""")]
        public void GivenIWantToReplaceThemWith(string replaceWith)
        {
            _replaceWith = replaceWith;
        }

        [When(@"the replace tool is executed")]
        public void WhenTheReplaceToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the replace result should be ""(.*)""")]
        public void ThenTheReplaceResultShouldBe(string expectedResult)
        {
            string error;
            string actualValue;
            expectedResult = expectedResult.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(expectedResult, actualValue);
        }

        [Then(@"""(.*)"" should be ""(.*)""")]
        public void ThenShouldBe(string variable, string value)
        {
            string error;
            string actualValue;
            value = value.Replace("\"\"", "");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"the replace execution has ""(.*)"" error")]
        public void ThenTheReplaceExecutionHasError(string anError)
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