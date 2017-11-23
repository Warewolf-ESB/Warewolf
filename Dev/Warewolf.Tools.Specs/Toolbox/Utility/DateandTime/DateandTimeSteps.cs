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
using System.Globalization;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.DateandTime
{
    [Binding]
    public class DateandTimeSteps : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public DateandTimeSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
        }

        protected override void BuildDataList()
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            variableList.Add(new Tuple<string, string>(ResultVariable, ""));
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("inputDate", out string inputDate);
            scenarioContext.TryGetValue("inputFormat", out string inputFormat);
            scenarioContext.TryGetValue("outputFormat", out string outputFormat);
            scenarioContext.TryGetValue("timeModifierType", out string timeModifierType);
            scenarioContext.TryGetValue("timeModifierAmount", out string timeModifierAmount);

            //Ashley: Windows Server 2008 is too outdated to know GMT was renamed to UTC.
            if (Environment.OSVersion.ToString() == "Microsoft Windows NT 6.0.6002 Service Pack 2")
            {
                inputDate = inputDate.Replace("(UTC+", "(GMT+").Replace("(UTC-", "(GMT-");
            }

            var dateTime = new DsfDateTimeActivity
            {
                Result = ResultVariable,
                DateTime = inputDate,
                InputFormat = inputFormat,
                OutputFormat = outputFormat,
                TimeModifierType = timeModifierType,
                TimeModifierAmountDisplay = timeModifierAmount
            };

            TestStartNode = new FlowStep
            {
                Action = dateTime
            };

            scenarioContext.Add("activity", dateTime);
        }


        [Given(@"I have a date ""(.*)""")]
        public void GivenIHaveADate(string inputDate)
        {
            scenarioContext.Add("inputDate", inputDate);
        }
        [Given(@"I have a Date time variable ""(.*)"" with value ""(.*)""")]
        public void GivenIHaveADateTimeVariableWithValue(string name, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(name, value));
        }




        [Given(@"the input format as ""(.*)""")]
        public void GivenTheInputFormatAs(string inputFormat)
        {
            scenarioContext.Add("inputFormat", inputFormat);
        }

        [Given(@"I selected Add time as ""(.*)"" with a value of (.*)")]
        public void GivenISelectedAddTimeAsWithAValueOf(string timeModifierType, string timeModifierAmount)
        {
            scenarioContext.Add("timeModifierType", timeModifierType);
            scenarioContext.Add("timeModifierAmount", timeModifierAmount);
        }

        [Given(@"the output format as ""(.*)""")]
        public void GivenTheOutputFormatAs(string outputFormat)
        {
            scenarioContext.Add("outputFormat", outputFormat);
        }

        [When(@"the datetime tool is executed")]
        public void WhenTheDatetimeToolIsExecuted()
        {
            BuildDataList();
            IDSFDataObject result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Then(@"the datetime result should be a ""(.*)""")]
        public void ThenTheDatetimeResultShouldBeA(string type)
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);

            TypeConverter converter = TypeDescriptor.GetConverter(Type.GetType(type));

            converter.ConvertFrom(actualValue);
        }

        [Then(@"the datetime result should be ""(.*)""")]
        public void ThenTheDatetimeResultShouldBe(string expectedResult)
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);
            if (actualValue != null)
            {
                //Ashley: Windows Server 2008 is too outdated to know GMT was renamed to UTC.
                actualValue = actualValue.Replace("(GMT+", "(UTC+").Replace("(GMT-", "(UTC-");
                if (expectedResult.Contains("A.D.") || expectedResult.Contains("AD"))
                {
                    var eraValue = CultureInfo.InvariantCulture.DateTimeFormat.GetEra("A.D.");
                    if (eraValue == -1) //The Era value does not use punctuation
                    {
                        actualValue = actualValue.Replace("A.D.", "AD");
                        expectedResult = expectedResult.Replace("A.D.", "AD");
                    }
                    else
                    {
                        actualValue = actualValue.Replace("AD", "A.D.");
                        expectedResult = expectedResult.Replace("AD", "A.D.");
                    }
                }
                if (expectedResult.Contains("B.C.") || expectedResult.Contains("BC"))
                {
                    var eraValue = CultureInfo.InvariantCulture.DateTimeFormat.GetEra("B.C.");
                    if (eraValue == -1) //The Era value does not use punctuation
                    {
                        actualValue = actualValue.Replace("B.C.", "BC");
                        expectedResult = expectedResult.Replace("B.C.", "BC");
                    }
                    else
                    {
                        actualValue = actualValue.Replace("BC", "B.C.");
                        expectedResult = expectedResult.Replace("BC", "B.C.");
                    }

                }
            }
            if (string.IsNullOrEmpty(expectedResult))
            {
                Assert.IsTrue(string.IsNullOrEmpty(actualValue));
            }
            else
            {
                if (actualValue != null)
                {
                    actualValue = actualValue.Replace('"', ' ').Trim();
                    Assert.AreEqual(expectedResult, actualValue);
                }
            }
        }

        [Then(@"the datetime result should contain milliseconds")]
        public void ThenTheDatetimeResultShouldContainMilliseconds()
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");

            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);
            Assert.IsTrue(actualValue.Contains("."));


        }

        [Then(@"the datetime result should contain milliseconds Standard Format")]
        public void ThenTheDatetimeResultShouldContainMillisecondsStandardFormat()
        {
            var result = scenarioContext.Get<IDSFDataObject>("result");

            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);

            var parsed = DateTime.Parse(actualValue, CultureInfo.InvariantCulture);
            var milliseconds = parsed.Millisecond;
            Assert.AreEqual(0, milliseconds);
        }


    }
}
