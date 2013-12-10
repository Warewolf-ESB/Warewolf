using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Activities.Specs.BaseTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs.Toolbox.Data.BaseConversion
{
    [Binding]
    public class BaseConversionSteps : RecordSetBases
    {
        private DsfBaseConvertActivity _baseConvert;

        public BaseConversionSteps()
            : base(new List<Tuple<string, string, string, string>>())
        {
        }

        private void BuildDataList()
        {
            BuildShapeAndTestData();

            _baseConvert = new DsfBaseConvertActivity();

            TestStartNode = new FlowStep
                {
                    Action = _baseConvert
                };

            int row = 1;
            foreach (dynamic variable in _variableList)
            {
                _baseConvert.ConvertCollection.Add(new BaseConvertTO(variable.Item1, variable.Item3, variable.Item4,
                                                                     variable.Item1, row));
                row++;
            }
        }

        [Given(@"I convert value ""(.*)"" from type ""(.*)"" to type ""(.*)""")]
        public void GivenIConvertValueFromTypeToType(string value, string fromType, string toType)
        {
            _variableList.Add(new Tuple<string, string, string, string>("[[var]]", value, fromType, toType));
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
            value = value.Replace("\"\"", "");
            GetScalarValueFromDataList(_result.DataListID, "var", out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"the base convert execution has ""(.*)"" error")]
        public void ThenTheBaseConvertExecutionHasError(string anError)
        {
            var expected = anError.Equals("NO");
            var actual = string.IsNullOrEmpty(FetchErrors(_result.DataListID));
            string message = string.Format("expected {0} error but an error was {1}", anError, actual ? "not found" : "found");
            Assert.AreEqual(expected, actual, message);
        }
    }
}