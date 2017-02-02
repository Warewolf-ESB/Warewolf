/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Data.BaseConversion
{
    [Binding]
    public class BaseConversionSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public BaseConversionSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            BuildShapeAndTestData();

            var baseConvert = new DsfBaseConvertActivity();

            TestStartNode = new FlowStep
                {
                    Action = baseConvert
                };

            int row = 1;

            var baseCollection = scenarioContext.Get<List<Tuple<string, string, string>>>("baseCollection");

            foreach(dynamic variable in baseCollection)
            {
                baseConvert.ConvertCollection.Add(new BaseConvertTO(variable.Item1, variable.Item2, variable.Item3,
                                                                    variable.Item1, row));
                row++;
            }
            scenarioContext.Add("activity", baseConvert);
        }

        [Given(@"I have a convert variable ""(.*)"" with a value of ""(.*)""")]
        public void GivenIHaveAConvertVariableWithAValueOf(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I convert a variable ""(.*)"" from type ""(.*)"" to type ""(.*)""")]
        public void GivenIConvertAVariableFromTypeToType(string variable, string fromType, string toType)
        {
            List<Tuple<string, string, string>> baseCollection;
            scenarioContext.TryGetValue("baseCollection", out baseCollection);

            if(baseCollection == null)
            {
                baseCollection = new List<Tuple<string, string, string>>();
                scenarioContext.Add("baseCollection", baseCollection);
            }

            baseCollection.Add(new Tuple<string, string, string>(variable, fromType, toType));
        }

        [When(@"the base conversion tool is executed")]
        public void WhenTheBaseConversionToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the result is ""(.*)""")]
        public void ThenTheResultIs(string value)
        {
            string error;
            string actualValue;
            value = value.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, "[[var]]", out actualValue, out error);
            if (string.IsNullOrEmpty(value))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(value, actualValue);
            }
        }
    }
}
