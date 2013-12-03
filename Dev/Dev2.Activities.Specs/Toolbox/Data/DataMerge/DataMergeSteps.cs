using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.DataMerge
{
    [Binding]
    public class DataMergeSteps : BaseActivityUnitTest
    {
        private DsfDataMergeActivity _dataMerge;
        private readonly List<Tuple<string, string, string, string>> _variableList = new List<Tuple<string, string, string, string>>();
        private IDSFDataObject _result;
        private const string ResultVariable = "[[result]]";

        private void BuildDataList()
        {
            _dataMerge = new DsfDataMergeActivity { Result = ResultVariable };
            
            TestStartNode = new FlowStep
            {
                Action = _dataMerge
            };

            var shape = new StringBuilder();
            shape.Append("<ADL>");

            var data = new StringBuilder();
            data.Append("<root>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                shape.Append(string.Format("<{0}/>", variableName));
                _dataMerge.MergeCollection.Add(new DataMergeDTO(variable.Item1, variable.Item2, variable.Item3, row, "", "Left"));
                data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item4));
                row++;
            }
            shape.Append(string.Format("<{0}></{0}>",  DataListUtil.RemoveLanguageBrackets(ResultVariable)));
            shape.Append("</ADL>");
            data.Append("</root>");

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }
        
        [Given(@"A variable ""(.*)"" with a value ""(.*)"" and merge type ""(.*)"" and string at as ""(.*)""")]
        public void GivenAVariableWithAValueAndMergeTypeAndStringAtAs(string variable, string value, string mergeType, string stringAt)
        {
            _variableList.Add(new Tuple<string, string, string, string>(variable, mergeType, stringAt, value));
        }

        
        [When(@"the data merge tool is executed")]
        public void WhenTheDataMergeToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the merged result is ""(.*)""")]
        public void ThenTheMergedResultIs(string value)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable), out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
