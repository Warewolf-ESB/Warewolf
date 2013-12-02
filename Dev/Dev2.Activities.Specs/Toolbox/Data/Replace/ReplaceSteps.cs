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
        private const string InFields = "[[sentence]]";
        private string _find;
        private string _replaceWith;
        private string _sentence;

        private void BuildDataList()
        {
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


            var shape = new StringBuilder();
            shape.Append("<ADL>");
            shape.Append(string.Format("<{0}/>", DataListUtil.RemoveLanguageBrackets(InFields)));
            shape.Append(string.Format("<{0}/>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            shape.Append("</ADL>");

            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}>{1}</{0}>", DataListUtil.RemoveLanguageBrackets(InFields), _sentence));
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");
            
            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have a sentence ""(.*)""")]
        public void GivenIHaveASentence(string sentence)
        {
            _sentence = sentence;
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

        [Then(@"the replaced result should be ""(.*)""")]
        public void ThenTheReplacedResultShouldBe(string result)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(InFields),
                                       out actualValue, out error);
            Assert.AreEqual(result, actualValue);
        }
    }
}
