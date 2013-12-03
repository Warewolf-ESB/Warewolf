using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.CaseConversion
{
    [Binding]
    public class CaseConversionSteps : RecordSetBases
    {
        private DsfCaseConvertActivity _caseConvert;

        public CaseConversionSteps()
            : base(new List<Tuple<string, string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _caseConvert = new DsfCaseConvertActivity();

            TestStartNode = new FlowStep
                {
                    Action = _caseConvert
                };

            int row = 1;
            foreach (dynamic variable in _variableList)
            {
                _caseConvert.ConvertCollection.Add(new CaseConvertTO(variable.Item1, variable.Item3, variable.Item1, row));
                row++;
            }
        }

        [Given(@"I convert a sentence ""(.*)"" to ""(.*)""")]
        public void GivenIConvertASentenceTo(string sentence, string toCase)
        {
            _variableList.Add(new Tuple<string, string, string>("[[var]]", sentence, toCase));
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