
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var baseConvert = new DsfBaseConvertActivity();

            TestStartNode = new FlowStep
                {
                    Action = baseConvert
                };

            int row = 1;

            var baseCollection = ScenarioContext.Current.Get<List<Tuple<string, string, string>>>("baseCollection");

            foreach(dynamic variable in baseCollection)
            {
                baseConvert.ConvertCollection.Add(new BaseConvertTO(variable.Item1, variable.Item2, variable.Item3,
                                                                    variable.Item1, row));
                row++;
            }
            ScenarioContext.Current.Add("activity", baseConvert);
        }

        [Given(@"I have a convert variable ""(.*)"" with a value of ""(.*)""")]
        [Given(@"I have a convert variable '(.*)' with a value of '(.*)'")]
        public void GivenIHaveAConvertVariableWithAValueOf(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I convert a variable ""(.*)"" from type ""(.*)"" to type ""(.*)""")]
        [Given(@"I convert a variable ""(.*)"" from type '(.*)' to type '(.*)'")]
        [Given(@"I convert a variable '(.*)' from type '(.*)' to type '(.*)'")]
        public void GivenIConvertAVariableFromTypeToType(string variable, string fromType, string toType)
        {
            List<Tuple<string, string, string>> baseCollection;
            ScenarioContext.Current.TryGetValue("baseCollection", out baseCollection);

            if(baseCollection == null)
            {
                baseCollection = new List<Tuple<string, string, string>>();
                ScenarioContext.Current.Add("baseCollection", baseCollection);
            }

            baseCollection.Add(new Tuple<string, string, string>(variable, fromType, toType));
        }

        [When(@"the base conversion tool is executed")]
        public void WhenTheBaseConversionToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result is ""(.*)""")]
        public void ThenTheResultIs(string value)
        {
            string error;
            string actualValue;
            value = value.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment( "[[var]]", out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
