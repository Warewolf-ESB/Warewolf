using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            : base(new List<Tuple<string, string>>())
        {
        }

        private DsfDataMergeActivity _dataMerge;
        private readonly List<Tuple<string, string, string, string, string>> _mergeCollection = new List<Tuple<string, string, string, string, string>>();

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable , ""));

            _dataMerge = new DsfDataMergeActivity { Result = ResultVariable };
            
            TestStartNode = new FlowStep
            {
                Action = _dataMerge
            };

            int row = 1;
            foreach (var variable in _mergeCollection)
            {
                _dataMerge.MergeCollection.Add(new DataMergeDTO(variable.Item1, variable.Item2, variable.Item3, row , variable.Item4, variable.Item5));
                row++;
            }
        }

        [Given(@"a merge variable ""(.*)"" equal to ""(.*)""")]
        public void GivenAMergeVariableEqualTo(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"an Input ""(.*)"" and merge type ""(.*)"" and string at as ""(.*)"" and Padding ""(.*)"" and Alignment ""(.*)""")]
        public void GivenAnInputAndMergeTypeAndStringAtAsAndPaddingAndAlignment(string input, string mergeType, string stringAt, string padding, string alignment)
        {
           _mergeCollection.Add(new Tuple<string, string, string, string, string>(input , mergeType , stringAt, padding, alignment));
        }

        [Given(@"a merge recordset")]
        public void GivenAMergeRecordset(Table table)
        {
            var records = table.Rows.ToList();
            foreach (TableRow record in records)
            {
                _variableList.Add(new Tuple<string, string>(record[0], record[1]));
            }
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

        [Then(@"the data merge execution has ""(.*)"" error")]
        public void ThenTheDataMergeExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }

        [Then(@"the merged result is the same as file ""(.*)""")]
        public void ThenTheMergedResultIsTheSameAsFile(string fileName)
        {
            string resourceName = string.Format("Dev2.Activities.Specs.Toolbox.Data.DataMerge.{0}",
                                               fileName);
            var value = ReadFile(resourceName);
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable), out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

    }
}
