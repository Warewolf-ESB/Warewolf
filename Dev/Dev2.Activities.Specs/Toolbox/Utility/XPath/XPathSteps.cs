using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Utility.XPath
{
    [Binding]
    public class XPathSteps : BaseActivityUnitTest
    {
        private DsfXPathActivity _xPath;
        private readonly List<Tuple<string, string>> _variableList = new List<Tuple<string, string>>();
        private IDSFDataObject _result;
        private string _xmlData;
        //private string _variable;
        //private string _xpath;
        private string _recordSetName;
        private string _fieldName;

        private void BuildDataList()
        {
            _xPath = new DsfXPathActivity {SourceString = _xmlData};

            TestStartNode = new FlowStep
            {
                Action = _xPath
            };

            var data = new StringBuilder();
            data.Append("<root>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);

                if (variableName.Contains("(*)") || variableName.Contains("()"))
                {
                    variableName = variableName.Replace("(*)", "").Replace("()", "");
                    var variableNameSplit = variableName.Split(".".ToCharArray());
                    data.Append(string.Format("<{0}>", variableNameSplit[0]));
                    data.Append(string.Format("<{0}/>", variableNameSplit[1]));
                    data.Append(string.Format("</{0}>", variableNameSplit[0]));

                    _recordSetName = variableNameSplit[0];
                    _fieldName = variableNameSplit[1];
                }
                else
                {
                    data.Append(string.Format("<{0}/>", variableName));
                }
                _xPath.ResultsCollection.Add(new XPathDTO(variable.Item1, variable.Item2, row));
                row++;
            }

            data.Append("</root>");
            CurrentDl = data.ToString();
            TestData = data.ToString();
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
        
        [Then(@"the variable \[\[firstNum]] should have a value ""(.*)""")]
        public void ThenTheVariableFirstNumShouldHaveAValue(string result)
        {
            
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
