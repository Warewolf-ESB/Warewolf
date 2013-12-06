using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.XPath
{
    [Binding]
    public class XPathSteps : RecordSetBases
    {
        public XPathSteps()
            : base(new List<Tuple<string, string>>())
        {
        }

        private DsfXPathActivity _xPath;
        private string _xmlData;

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _xPath = new DsfXPathActivity
                {
                    SourceString = _xmlData
                };

            TestStartNode = new FlowStep
                {
                    Action = _xPath
                };

            int row = 1;
            foreach (var variable in _variableList)
            {
                _xPath.ResultsCollection.Add(new XPathDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
        }

        [Given(@"I have this xml '(.*)'")]
        public void GivenIHaveThisXml(string xmlData)
        {
            _xmlData = xmlData;
        }

        [Given(@"I have a variable ""(.*)"" output with xpath ""(.*)""")]
        public void GivenIHaveAVariableOutputWithXpath(string variable, string xpath)
        {
            _variableList.Add(new Tuple<string, string>(variable, xpath));
        }

        [Given(@"I have this xml '(.*)' in a variable ""(.*)""")]
        public void GivenIHaveThisXmlInAVariable(string xml, string variable)
        {
            _variableList.Add(new Tuple<string, string>(variable, xml));
        }
        
        [When(@"the xpath tool is executed")]
        public void WhenTheXpathToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }

        [Then(@"the variable ""(.*)"" should have a value ""(.*)""")]
        public void ThenTheVariableShouldHaveAValue(string variable, string value)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
        
        [Then(@"the xpath result for this varibale ""(.*)"" will be")]
        public void ThenTheXpathResultForThisVaribaleWillBe(string variable, Table table)
        {
            var recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
            var column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

            string error;
            var recordSetValues = RetrieveAllRecordSetFieldValues(_result.DataListID, recordset, column, out error);
            recordSetValues = recordSetValues.Where(i => !string.IsNullOrEmpty(i)).ToList();

            List<TableRow> tableRows = table.Rows.ToList();
            Assert.AreEqual(tableRows.Count, recordSetValues.Count);
            for (int i = 0; i < tableRows.Count; i++)
            {
                Assert.AreEqual(tableRows[i][1], recordSetValues[i]);
            }
        }

        [Then(@"the xpath execution has ""(.*)"" error")]
        public void ThenTheXpathExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }

    }
}
