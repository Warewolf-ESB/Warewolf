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
        private DsfMultiAssignActivity _multiAssign;

        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            BuildShapeAndTestData();

            _multiAssign = new DsfMultiAssignActivity();

            TestStartNode = new FlowStep
                {
                    Action = _multiAssign
                };

            int row = 1;
            foreach (var variable in variableList)
            {
                _multiAssign.FieldsCollection.Add(new ActivityDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
        }

        [Given(@"I assign the value (.*) to a variable ""(.*)""")]
        public void GivenIAssignTheValueToAVariable(string value, string variable)
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

        [When(@"the assign tool is executed")]
        public void WhenTheAssignToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the value of ""(.*)"" equals (.*)")]
        public void ThenTheValueOfEquals(string variable, string value)
        {
            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");

            if (DataListUtil.IsValueRecordset(variable))
            {
                string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
                string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);
                List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                               out error);
                recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();
                Assert.AreEqual(recordSetValues[1], value);
            }
            else
            {
                string actualValue;
                value = value.Replace('"', ' ').Trim();
                GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                           out actualValue, out error);
                actualValue = actualValue.Replace('"', ' ').Trim();
                Assert.AreEqual(value, actualValue);
            }
        }

        [Then(@"the assign execution has ""(.*)"" error")]
        public void ThenTheAssignExecutionHasError(string anError)
        {
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            bool expected = anError.Equals("NO");
            bool actual = string.IsNullOrEmpty(FetchErrors(result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}