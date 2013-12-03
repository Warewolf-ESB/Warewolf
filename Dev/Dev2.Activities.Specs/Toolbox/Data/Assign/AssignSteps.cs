using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.Assign
{
    [Binding]
    public class AssignSteps : RecordSetBases
    {
        public AssignSteps(): base (new List<Tuple<string, string>>())
        {
            
        }
        private DsfMultiAssignActivity _multiAssign;

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _multiAssign = new DsfMultiAssignActivity();

            TestStartNode = new FlowStep
                {
                    Action = _multiAssign
                };

            int row = 1;
            
            foreach (var variable in _variableList)
            {
                _multiAssign.FieldsCollection.Add(new ActivityDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
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
            value = value.Replace('"', ' ').Trim();
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable), out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}