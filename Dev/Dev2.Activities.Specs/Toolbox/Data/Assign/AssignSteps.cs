using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System.Globalization;

namespace Dev2.Activities.Specs.Toolbox.Data.Assign
{
    [Binding]
    public class AssignSteps : BaseActivityUnitTest
    {
        private readonly List<Tuple<string, string>> _variableList = new List<Tuple<string, string>>();
        private DsfMultiAssignActivity _multiAssign;
        private IDSFDataObject _result;
        private string _recordSetName;

        private void BuildDataList()
        {
            _multiAssign = new DsfMultiAssignActivity();

            TestStartNode = new FlowStep
                {
                    Action = _multiAssign
                };

            var shape = new StringBuilder();
            shape.Append("<ADL>");

            var data = new StringBuilder();
            data.Append("<ADL>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                _multiAssign.FieldsCollection.Add(new ActivityDTO(variable.Item1, variable.Item2, row, true));

                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                if (variableName.Contains("(") && variableName.Contains(")"))
                {
                    variableName = variableName.Replace("(", "").Replace(")", "").Replace("*", "");
                    var variableNameSplit = variableName.Split(".".ToCharArray());
                    shape.Append(string.Format("<{0}>", variableNameSplit[0]));
                    shape.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    shape.Append(string.Format("</{0}>", variableNameSplit[0]));

                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableNameSplit[1], variable.Item2));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));

                    _recordSetName = variableNameSplit[0];
                }
                else
                {
                    shape.Append(string.Format("<{0}/>", variableName));
                    data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item2));
                }
                row++;
            }
            
            shape.Append("</ADL>");
            data.Append("</ADL>");

            CurrentDl = shape.ToString();
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
            value = value.Replace('"', ' ').Trim();
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable), out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}