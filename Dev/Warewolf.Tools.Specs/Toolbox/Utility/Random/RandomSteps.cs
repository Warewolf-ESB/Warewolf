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
using System.ComponentModel;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.Random
{
    [Binding]
    public class RandomSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public RandomSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            var resultVariable = ResultVariable;
            string resVar;
            if (scenarioContext.TryGetValue("resVar", out resVar))
            {
                resultVariable = resVar;
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
            BuildShapeAndTestData();

            enRandomType randomType;
            scenarioContext.TryGetValue("randomType", out randomType);
            string length;
            scenarioContext.TryGetValue("length", out length);
            string rangeFrom;
            scenarioContext.TryGetValue("rangeFrom", out rangeFrom);
            string rangeTo;
            scenarioContext.TryGetValue("rangeTo", out rangeTo);

            var dsfRandom = new DsfRandomActivity
                {
                    RandomType = randomType,
                    Result = resultVariable
            };

            if (!string.IsNullOrEmpty(length))
            {
                dsfRandom.Length = length;
            }

            if (!string.IsNullOrEmpty(rangeFrom))
            {
                dsfRandom.From = rangeFrom;
            }

            if (!string.IsNullOrEmpty(rangeTo))
            {
                dsfRandom.To = rangeTo;
            }

            TestStartNode = new FlowStep
                {
                    Action = dsfRandom
                };

            scenarioContext.Add("activity", dsfRandom);
        }

        [Given(@"I have a type as ""(.*)""")]
        public void GivenIHaveATypeAs(string randomType)
        {
            scenarioContext.Add("randomType", (enRandomType)Enum.Parse(typeof(enRandomType), randomType));
        }

        [Given(@"I have a length as ""(.*)""")]
        public void GivenIHaveALengthAs(string length)
        {
            scenarioContext.Add("length", length);
        }

        [Given(@"I have a range from ""(.*)"" to ""(.*)""")]
        public void GivenIHaveARangeFromTo(string rangeFrom, string rangeTo)
        {
            scenarioContext.Add("rangeFrom", rangeFrom);
            scenarioContext.Add("rangeTo", rangeTo);
        }


        [When(@"the random tool is executed")]
        public void WhenTheRandomToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the result from the random tool should be of type ""(.*)"" with a length of ""(.*)""")]
        public void ThenTheResultFromTheRandomToolShouldBeOfTypeWithALengthOf(string type, int length)
        {
            string error;
            string actualValue;
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out actualValue, out error);
            // ReSharper disable AssignNullToNotNullAttribute
            TypeConverter converter = TypeDescriptor.GetConverter(Type.GetType(type));
            // ReSharper restore AssignNullToNotNullAttribute
            converter.ConvertFrom(actualValue);
            if (length == 0)
            {
                Assert.IsTrue(String.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(length, actualValue.Length);
            }
        }

        [Then(@"the result from the random tool should be of the same type as ""(.*)""")]
        public void ThenTheResultFromTheRandomToolShouldBeOfTheSameTypeAs(string type)
        {
            string error;
            string actualValue;
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, ResultVariable,
                                       out actualValue, out error);
            // ReSharper disable AssignNullToNotNullAttribute
            TypeConverter converter = TypeDescriptor.GetConverter(Type.GetType(type));
            // ReSharper restore AssignNullToNotNullAttribute
            if(actualValue != null)
            {
                converter.ConvertFrom(actualValue);
            }
        }

        [Then(@"the random value will be ""(.*)""")]
        public void ThenTheRandomValueWillBe(string value)
        {
            string error;
            string actualValue;
            value = value.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }

        [Then(@"the random value will be between ""(.*)"" and ""(.*)"" inclusive")]
        public void ThenTheRandomValueWillBeBetweenAndInclusive(Decimal from, Decimal to)
        {
            string error;
            string actualValue;
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            decimal d = decimal.Parse(actualValue);
            Assert.IsTrue(d >= from && d <= to);
        }

        [Given(@"I have a a random variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveAARandomVariableEqualTo(string variable, string value)
        {
            List<Tuple<string, string>> variableList;
            value = value.Replace('"', ' ').Trim();
            variable = variable.Replace('"', ' ').Trim();
            scenarioContext.TryGetValue("variableList", out variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }

        [Given(@"I have a random result variable as ""(.*)""")]
        public void GivenIHaveARandomResultVariableAs(string resultVar)
        {
            scenarioContext.Add("resVar", resultVar);
        }

    }
}
