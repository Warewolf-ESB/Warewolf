using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.CaseConversion
{
    [Binding]
    public class CaseConversionSteps : BaseActivityUnitTest
    {
        private DsfCaseConvertActivity _caseConvert;
        private readonly List<Tuple<string, string, string>> _variableList = new List<Tuple<string, string, string>>();
        private IDSFDataObject _result;

        private void BuildDataList()
        {
            _caseConvert = new DsfCaseConvertActivity();

            TestStartNode = new FlowStep
            {
                Action = _caseConvert
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
                _caseConvert.ConvertCollection.Add(new CaseConvertTO(variable.Item1, variable.Item2, variable.Item1, row));

                data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item3));
                row++;
            }
            shape.Append("</ADL>");
            data.Append("</root>");

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        [Given(@"I convert a sentence ""(.*)"" to ""(.*)""")]
        public void GivenIConvertASentenceTo(string sentence, string toCase)
        {
            _variableList.Add(new Tuple<string, string, string>("[[var]]", toCase, sentence));
        }
        
        [When(@"the case conversion tool is executed")]
        public void WhenTheCaseConversionToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the sentence will be ""(.*)""")]
        public void ThenTheSentenceWillBe(string value)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, "var", out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
