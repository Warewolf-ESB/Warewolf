using System;
using System.Activities.Statements;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.Replace
{
    [Binding]
    public class ReplaceSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfReplaceActivity _replace;
        private IDSFDataObject _result;
        private string _inFields;
        private string _find;
        private string _replaceWith;

        private void BuildDataList()
        {
            _replace = new DsfReplaceActivity
            {
                Result = ResultVariable,
                FieldsToSearch = _inFields,
                Find = _find,
                ReplaceWith = _replaceWith
            };

            TestStartNode = new FlowStep
            {
                Action = _replace
            };


            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have a sentence ""(.*)""")]
        public void GivenIHaveASentence(string inFields)
        {
            _inFields = inFields;
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
            _result = ExecuteProcess();
        }
        
        [Then(@"the result should be ""(.*)""")]
        public void ThenTheResultShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}
