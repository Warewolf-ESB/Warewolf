using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.BaseConversion
{
    [Binding]
    public class BaseConversionSteps : BaseActivityUnitTest
    {
        private DsfBaseConvertActivity _baseConvert;
        private readonly List<Tuple<string,string,string, string>> _variableList = new List<Tuple<string,string,string, string>>();
        private IDSFDataObject _result;

        private void BuildDataList()
        {
            _baseConvert = new DsfBaseConvertActivity();

            TestStartNode = new FlowStep
            {
                Action = _baseConvert
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
                _baseConvert.ConvertCollection.Add(new BaseConvertTO(variable.Item1, variable.Item2, variable.Item3,
                                                                     variable.Item1, row));

                data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item4));
                row++;
            }
            shape.Append("</ADL>");
            data.Append("</root>");

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }
        
        [Given(@"I convert value ""(.*)"" from type ""(.*)"" to type ""(.*)""")]
        public void GivenIConvertValueFromTypeToType(string value, string fromType, string toType)
        {
            _variableList.Add(new Tuple<string, string, string, string>("[[var]]", fromType, toType, value));
        }
        
        [When(@"the base conversion tool is executed")]
        public void WhenTheBaseConversionToolIsExecuted()
        {
            BuildDataList();
            _result = ExecuteProcess();
        }
        
        [Then(@"the result is ""(.*)""")]
        public void ThenTheResultIs(string value)
        {
            string error;
            string actualValue;
            GetScalarValueFromDataList(_result.DataListID, "var", out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

    }
}
