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
        private string _recordSetName;
        private string _fieldName;

        private void BuildDataList()
        {
            _xPath = new DsfXPathActivity
                {
                    SourceString = _xmlData
                };

            TestStartNode = new FlowStep
                {
                    Action = _xPath
                };

            var shape = new StringBuilder();
            shape.Append("<ADL>");

            var data = new StringBuilder();
            data.Append("<ADL>");

            int row = 1;
            foreach (var variable in _variableList)
            {
                _xPath.ResultsCollection.Add(new XPathDTO(variable.Item1, variable.Item2, row, true));
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                if (variableName.Contains("(") && variableName.Contains(")"))
                {
                    var startIndex = variableName.IndexOf("(");
                    var endIndex = variableName.IndexOf(")");

                    int i = (endIndex - startIndex) - 1;

                    if (i > 0)
                    {
                        variableName = variableName.Remove(startIndex + 1, i);
                    }

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
