
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
using System.ComponentModel;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.Toolbox.Utility.Random
{
    [Binding]
    public class RandomSteps : RecordSetBases
    {
        protected override void BuildDataList()
        {
            List<Tuple<string, string>> variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                ScenarioContext.Current.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            enRandomType randomType;
            ScenarioContext.Current.TryGetValue("randomType", out randomType);
            string length;
            ScenarioContext.Current.TryGetValue("length", out length);
            string rangeFrom;
            ScenarioContext.Current.TryGetValue("rangeFrom", out rangeFrom);
            string rangeTo;
            ScenarioContext.Current.TryGetValue("rangeTo", out rangeTo);

            var dsfRandom = new DsfRandomActivity
                {
                    RandomType = randomType,
                    Result = ResultVariable
                };

            if(!string.IsNullOrEmpty(length))
            {
                dsfRandom.Length = length;
            }

            if(!string.IsNullOrEmpty(rangeFrom))
            {
                dsfRandom.From = rangeFrom;
            }

            if(!string.IsNullOrEmpty(rangeTo))
            {
                dsfRandom.To = rangeTo;
            }

            TestStartNode = new FlowStep
                {
                    Action = dsfRandom
                };

            ScenarioContext.Current.Add("activity", dsfRandom);
        }

        [Given(@"I have a type as ""(.*)""")]
        public void GivenIHaveATypeAs(string randomType)
        {
            ScenarioContext.Current.Add("randomType", (enRandomType)Enum.Parse(typeof(enRandomType), randomType));
        }

        [Given(@"I have a length as ""(.*)""")]
        public void GivenIHaveALengthAs(string length)
        {
            ScenarioContext.Current.Add("length", length);
        }

        [Given(@"I have a range from ""(.*)"" to ""(.*)""")]
        public void GivenIHaveARangeFromTo(string rangeFrom, string rangeTo)
        {
            ScenarioContext.Current.Add("rangeFrom", rangeFrom);
            ScenarioContext.Current.Add("rangeTo", rangeTo);
        }


        [When(@"the random tool is executed")]
        public void WhenTheRandomToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            ScenarioContext.Current.Add("result", result);
        }

        [Then(@"the result from the random tool should be of type ""(.*)"" with a length of ""(.*)""")]
        public void ThenTheResultFromTheRandomToolShouldBeOfTypeWithALengthOf(string type, int length)
        {
            string error;
            string actualValue;
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(ResultVariable,
                                       out actualValue, out error);
            // ReSharper disable AssignNullToNotNullAttribute
            TypeConverter converter = TypeDescriptor.GetConverter(Type.GetType(type));
            // ReSharper restore AssignNullToNotNullAttribute
            converter.ConvertFrom(actualValue);
            if(length == 0)
            {
                Assert.IsTrue(String.IsNullOrEmpty(actualValue));
            }
            else
            {
                Assert.AreEqual(length, actualValue.Length);
            }
        }

        [Then(@"the random value will be ""(.*)""")]
        public void ThenTheRandomValueWillBe(string value)
        {
            string error;
            string actualValue;
            value = value.Replace('"', ' ').Trim();
            var result = ScenarioContext.Current.Get<IDSFDataObject>("result");
            GetScalarValueFromDataList(result.DataListID, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out actualValue, out error);
            Assert.AreEqual(value, actualValue);
        }
    }
}
