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

namespace Dev2.Activities.Specs.Toolbox.Data.DataMerge
{
    [Binding]
    public class DataMergeSteps : RecordSetBases
    {
        public DataMergeSteps()
            : base(new List<Tuple<string, string, string, string>>())
        {
        }

        private DsfDataMergeActivity _dataMerge;

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string, string, string>(ResultVariable , "", "", ""));

            _dataMerge = new DsfDataMergeActivity { Result = ResultVariable };
            
            TestStartNode = new FlowStep
            {
                Action = _dataMerge
            };

            int row = 1;
            foreach (var variable in _variableList)
            {
                _dataMerge.MergeCollection.Add(new DataMergeDTO(variable.Item1, variable.Item3, variable.Item4, row, "", "Left"));
                row++;
            }
        }
        
        [Given(@"A variable ""(.*)"" with a value ""(.*)"" and merge type ""(.*)"" and string at as ""(.*)""")]
        public void GivenAVariableWithAValueAndMergeTypeAndStringAtAs(string variable, string value, string mergeType, string stringAt)
        {
            _variableList.Add(new Tuple<string, string, string, string>(variable, value, mergeType, stringAt));
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
            value = value.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable), out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
