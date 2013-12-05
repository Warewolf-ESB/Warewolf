using System;
using System.Activities.Statements;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.Calculate
{
    [Binding]
    public class CalculateSteps : BaseActivityUnitTest
    {
        private const string ResultVariable = "[[result]]";
        private DsfCalculateActivity _calculate;
        private IDSFDataObject _result;
        private string _formula;

        private void BuildDataList()
        {
            _calculate = new DsfCalculateActivity
            {
                Result = ResultVariable,
                Expression = _formula
            };

            TestStartNode = new FlowStep
            {
                Action = _calculate
            };


            var data = new StringBuilder();
            data.Append("<root>");
            data.Append(string.Format("<{0}></{0}>", DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            data.Append("</root>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }

        [Given(@"I have the formula ""(.*)""")]
        public void GivenIHaveTheFormula(string formula)
        {
            _formula = formula;
        }
        
        [When(@"the calculate tool is executed")]
        public void WhenTheCalculateToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the result should be (.*)")]
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
