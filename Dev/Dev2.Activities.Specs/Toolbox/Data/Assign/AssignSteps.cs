using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.Assign
{
    [Binding]
    public class AssignSteps : BaseActivityUnitTest
    {
        private readonly List<Tuple<string, string>> _variableList = new List<Tuple<string, string>>();
        private DsfMultiAssignActivity _multiAssign;
        private IDSFDataObject _result;

        private void BuildDataList()
        {
            _multiAssign = new DsfMultiAssignActivity();

            TestStartNode = new FlowStep
                {
                    Action = _multiAssign
                };

            var data = new StringBuilder();
            data.Append("<ADL>");
            foreach (var variable in _variableList)
            {
                data.Append(string.Format("<{0}/>", DataListUtil.RemoveLanguageBrackets(variable.Item1)));
                _multiAssign.FieldsCollection.Add(new ActivityDTO(variable.Item1, variable.Item2, 1, true));
            }
            data.Append("</ADL>");

            CurrentDl = data.ToString();
            TestData = data.ToString();
        }
        
        [Given(@"I assign the value (.*) to a variable ""(.*)""")]
        public void GivenIAssignTheValueToAVariable(string value, string variable)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }
        
        [When(@"the assign tool is executed")]
        public void WhenTheAssignToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the value of ""(.*)"" equals (.*)")]
        public void ThenTheValueOfEquals(string variable, string value)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable), out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }


        [Then(@"the value of \[\[value]] equals HelloWorld")]
        public void ThenTheValueOfValueEqualsHelloWorld()
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, "value", out actualValue, out error);
            Assert.AreEqual("HelloWorld", actualValue);
        }
    }
}