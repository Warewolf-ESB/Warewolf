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
        private void BuildDataList()
        {
            var variableList = ScenarioContext.Current.Get<List<Tuple<string, string>>>("variableList");
            BuildShapeAndTestData();

            string xmlData;
              ScenarioContext.Current.TryGetValue("xmlData", out xmlData);

            var xPath = new DsfXPathActivity
                {
                    SourceString = xmlData
                };

            TestStartNode = new FlowStep
                {
                    Action = xPath
                };

            int row = 1;
            foreach (var variable in variableList)
            {
                xPath.ResultsCollection.Add(new XPathDTO(variable.Item1, variable.Item2, row, true));
                row++;
            }
        }

        [Given(@"I have this xml '(.*)'")]
        public void GivenIHaveThisXml(string xmlData)
        {
            ScenarioContext.Current.Add("xmlData", xmlData);
        }

        [Given(@"I have a variable ""(.*)"" output with xpath ""(.*)""")]
        public void GivenIHaveAVariableOutputWithXpath(string variable, string xpath)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, xpath));
        }

        [Given(@"I have this xml '(.*)' in a variable ""(.*)""")]
        public void GivenIHaveThisXmlInAVariable(string xml, string variable)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, xml));
        }

        [When(@"the xpath tool is executed")]
        public void WhenTheXpathToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(throwException:false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the variable ""(.*)"" should have a value ""(.*)""")]
        public void ThenTheVariableShouldHaveAValue(string variable, string value)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(variable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"the xpath result for this varibale ""(.*)"" will be")]
        public void ThenTheXpathResultForThisVaribaleWillBe(string variable, Table table)
        {
            string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable);
            string column = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable);

            string error;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            List<string> recordSetValues = RetrieveAllRecordSetFieldValues(result.DataListID, recordset, column,
                                                                           out error);
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
            bool expected = anError.Equals("NO");
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            string fetchErrors = FetchErrors(result.DataListID);
            bool actual = string.IsNullOrEmpty(fetchErrors);
            string message = string.Format("expected {0} error but it {1}", anError,
                                           actual ? "did not occur" : "did occur" + fetchErrors);
            Assert.AreEqual(expected, actual, message);
        }
    }
}