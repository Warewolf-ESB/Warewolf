using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
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

            if (DataListUtil.IsValueRecordset(variable))
            {
                var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
                var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, column, out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                Assert.AreEqual(recordSetValues[1], value);
            }
            else
            {
                string actualValue;
                value = value.Replace('"', ' ').Trim();
                GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                           out actualValue, out error);
                actualValue = actualValue.Replace('"', ' ').Trim();
                Assert.AreEqual(value, actualValue);
            }
        }

        [Then(@"the assign execution has ""(.*)"" error")]
        public void ThenTheAssignExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }

    }
}