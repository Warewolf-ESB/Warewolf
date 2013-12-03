using System;
using System.Activities.Statements;
using System.Collections.Generic;
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
        
        [Given(@"I have a variable ""(.*)"" with xpath ""(.*)""")]
        public void GivenIHaveAVariableWithXpath(string variable, string xpath)
        {
            _variableList.Add(new Tuple<string, string>(variable, xpath));
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
    }
}
