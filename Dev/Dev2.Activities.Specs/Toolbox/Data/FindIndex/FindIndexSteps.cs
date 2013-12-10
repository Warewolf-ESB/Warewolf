using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.FindIndex
{
    [Binding]
    public class FindIndexSteps : RecordSetBases
    {
        private string _characters;
        private string _direction;
        private DsfIndexActivity _findIndex;
        private string _inField;
        private string _index;

        public FindIndexSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData(new Tuple<string, string>(ResultVariable, ""));

            _findIndex = new DsfIndexActivity
                {
                    Result = ResultVariable,
                    InField = _inField,
                    Index = _index,
                    Characters = _characters,
                    Direction = _direction
                };

            TestStartNode = new FlowStep
                {
                    Action = _findIndex
                };
        }

        [Given(@"the sentence ""(.*)""")]
        public void GivenTheSentence(string sentence)
        {
            _inField = sentence;
        }

        [Given(@"I selected Index ""(.*)""")]
        public void GivenISelectedIndex(string index)
        {
            _index = index;
        }

        [Given(@"I search for characters ""(.*)""")]
        public void GivenISearchForCharacters(string characters)
        {
            _characters = characters;
        }

        [Given(@"I selected direction as ""(.*)""")]
        public void GivenISelectedDirectionAs(string direction)
        {
            _direction = direction;
        }
        
        [Given(@"I have a findindex variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAFindindexVariableEqualTo(string variable, string value)
        {
            _variableList.Add(new Tuple<string, string>(variable, value));
        }

        [When(@"the data find index tool is executed")]
        public void WhenTheDataFindIndexToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the find index result is ""(.*)""")]
        public void ThenTheFindIndexResultIs(string results)
        {
            string error;
            string actualValue;
            results = results.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(results, actualValue);
        }
        
        [Then(@"the find index result is")]
        public void ThenTheFindIndexResultIs(Table table)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);

            var records = actualValue.Split(',').ToList();
            List<TableRow> tableRows = table.Rows.ToList();

            Assert.AreEqual(tableRows.Count, records.Count);

            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], records[i]);
            }
        }
        
        [Then(@"the find index execution has ""(.*)"" error")]
        public void ThenTheFindIndexExecutionHasError(string anError)
        {
            bool expected = anError.Equals("NO");
            bool actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError,
                                           actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}