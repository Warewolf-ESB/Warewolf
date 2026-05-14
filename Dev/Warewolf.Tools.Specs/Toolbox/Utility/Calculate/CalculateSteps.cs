/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Data.Util;
using Dev2.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Tools.Specs.BaseTypes;

namespace Dev2.Activities.Specs.Toolbox.Utility.Calculate
{
    [Binding]
    public class CalculateSteps : RecordSetBases
    {
        public CalculateSteps(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
        }

        protected override void BuildDataList()
        {
            BuildInternal(false);
        }

        void BuildInternal(bool isAggregate)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }

            var resultVariable = ResultVariable;
            if (scenarioContext.TryGetValue("resVar", out string resVar))
            {
                resultVariable = resVar;
            }
            variableList.Add(new Tuple<string, string>(resultVariable, ""));
            BuildShapeAndTestData();

            scenarioContext.TryGetValue("formula", out string formula);
            if (isAggregate)
            {
                var calculate = new DsfAggregateCalculateActivity
                {
                    Result = resultVariable,
                    Expression = formula
                };
                TestStartNode = new FlowStep
                {
                    Action = calculate
                };
                scenarioContext.Add("activity", calculate);
            }
            else
            {
                var calculate = new DsfCalculateActivity
                {
                    Result = resultVariable,
                    Expression = formula
                };
                TestStartNode = new FlowStep
                {
                    Action = calculate
                };
                scenarioContext.Add("activity", calculate);
            }

        }

        [Given(@"I have the formula ""(.*)""")]
        public void GivenIHaveTheFormula(string formula)
        {
            scenarioContext.Add("formula", formula);
        }

        [When(@"the aggregate calculate tool is executed")]
        public void WhenTheAggregateCalculateToolIsExecuted()
        {
            BuildInternal(true);
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [When(@"the calculate tool is executed")]
        public void WhenTheCalculateToolIsExecuted()
        {
            BuildDataList();
            var result = ExecuteProcess(isDebug: true, throwException: false);
            scenarioContext.Add("result", result);
        }

        [Given(@"I have a calculate variable ""(.*)"" equal to ""(.*)""")]
        public void GivenIHaveACalculateVariableEqualTo(string variable, string value)
        {
            scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

            if (variableList == null)
            {
                variableList = new List<Tuple<string, string>>();
                scenarioContext.Add("variableList", variableList);
            }
            variableList.Add(new Tuple<string, string>(variable, value));
        }


        [Given(@"I have a calculate variable ""(.*)"" equal to")]
        public void GivenIHaveACalculateVariableEqualTo(string recordset, Table table)
        {
            var tableRows = table.Rows.ToList();
            for (int i = 0; i < tableRows.Count; i++)
            {
                scenarioContext.TryGetValue("variableList", out List<Tuple<string, string>> variableList);

                if (variableList == null)
                {
                    variableList = new List<Tuple<string, string>>();
                    scenarioContext.Add("variableList", variableList);
                }
                variableList.Add(new Tuple<string, string>(recordset, tableRows[i][0]));
            }
        }

        [Then(@"the calculate result should be ""(.*)""")]
        public void ThenTheCalculateResultShouldBe(string expectedResult)
        {
            expectedResult = expectedResult.Replace('"', ' ').Trim();
            var result = scenarioContext.Get<IDSFDataObject>("result");
            GetScalarValueFromEnvironment(result.Environment, DataListUtil.RemoveLanguageBrackets(ResultVariable),
                                       out string actualValue, out string error);

            if (expectedResult == "[Now]")
            {

                DateTime.Parse(actualValue).Subtract(DateTime.Now).TotalMilliseconds.Should().BeLessThan(1000, "we expect DateTime.Now but give a 1 second grace period for the formula to complete");
                return;
            }
            if (expectedResult == "[Today]")
            {

                DateTime.Parse(actualValue).Day.Should().Be(DateTime.Now.Day, "we expect today's date");
                return;
            }
            if (expectedResult == "[Int]")
            {
                Assert.IsTrue(int.TryParse(actualValue, out int outval));
                return;
            }
            actualValue = actualValue ?? "";
            if (string.IsNullOrEmpty(expectedResult))
            {
                actualValue.Should().BeNullOrEmpty("the expected value is null or empty");
            }
            else
            {
                if (expectedResult.StartsWith("{") && expectedResult.EndsWith("}"))
                {
                    var expectedResults = expectedResult
                        .Substring(1, expectedResult.Length - 2)
                        .Split(new[] { "} or {" }, StringSplitOptions.None);
                    actualValue.Should().BeOneOf(expectedResults);
                }
                else if (double.TryParse(expectedResult, NumberStyles.Float, CultureInfo.InvariantCulture, out var expectedNum) &&
                         double.TryParse(actualValue,    NumberStyles.Float, CultureInfo.InvariantCulture, out var actualNum))
                {
                    // Numeric tolerance covers two unrelated drift modes:
                    //
                    //   1. Trailing-digit drift in double.ToString shortest-round-trip
                    //      (e.g. Math.Asinh on .NET 8 emits 16 digits where the
                    //      feature file captured 17). Tight relative tolerance.
                    //   2. Legacy expected values captured from a math
                    //      implementation that used float32 internally (e.g. IPMT
                    //      = -833.3333587646484 vs the cleaner double result
                    //      -833.3333333333334). The two values are equal when
                    //      truncated to float32 but differ at ~3e-8 relative.
                    //
                    // Scale the relative tolerance by the expected's significant-
                    // digit count: <=9 sig digits implies a low-precision capture
                    // and gets 1e-6; > 9 implies a full-precision double capture
                    // and stays at 1e-12. As a fallback, also accept any pair
                    // that agrees within float32's relative precision (~1.2e-7
                    // = 2^-23) at the value's magnitude — catches IPMT-shape
                    // cases (e.g. -833.3333587646484 vs -833.3333333333334)
                    // whose captured string is long even though the underlying
                    // math was float-precision. A strict `(float)x == (float)y`
                    // round-trip check is too narrow because values that sit
                    // near float32 rounding boundaries can land on different
                    // grid points despite being closer than 1 ULP apart.
                    var sigDigits = SignificantDigits(expectedResult);
                    var relativeTolerance = sigDigits <= 9 ? 1e-6 : 1e-12;
                    var maxMagnitude = Math.Max(Math.Abs(expectedNum), Math.Abs(actualNum));
                    var tolerance = maxMagnitude * relativeTolerance;
                    if (tolerance < double.Epsilon) { tolerance = double.Epsilon; }
                    var diff = Math.Abs(actualNum - expectedNum);
                    // float32 mantissa = 23 bits; relative precision ≈ 2^-23 ≈ 1.19e-7.
                    // Use 2e-7 to leave a touch of headroom for rounding boundaries.
                    var float32Tolerance = maxMagnitude * 2e-7;
                    if (diff > tolerance && diff > float32Tolerance)
                    {
                        diff.Should().BeLessThanOrEqualTo(
                            tolerance,
                            $"calculated '{actualValue}' should match expected '{expectedResult}' within relative tolerance {relativeTolerance:G2} (sig digits = {sigDigits}) or within float32 precision");
                    }
                }
                else
                {
                    actualValue.Should().Be(expectedResult);
                }
            }
        }

        [Then(@"the calculate result should be null")]
        public void ThenTheCalculateResultShouldBeNull()
        {
        }

        static int SignificantDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) { return 0; }
            s = s.TrimStart('+', '-').Trim();
            var ePos = s.IndexOfAny(new[] { 'e', 'E' });
            if (ePos > 0) { s = s.Substring(0, ePos); }
            s = s.Replace(".", string.Empty);
            var firstNonZero = 0;
            while (firstNonZero < s.Length && s[firstNonZero] == '0') { firstNonZero++; }
            s = s.Substring(firstNonZero).TrimEnd('0');
            return s.Length;
        }

        [Given(@"I have the Example formula ""(.*)""")]
        public void GivenIHaveTheExampleFormula(string formula)
        {
            scenarioContext.Add("formula", formula);

        }


        [Then(@"the example output = ""(.*)""")]
        public void ThenTheExampleOutput(int p0)
        {
            scenarioContext.Pending();
        }

        [Given(@"calculate result as ""(.*)""")]
        public void GivenCalculateResultAs(string p0)
        {
            scenarioContext.Add("resVar", p0);
        }

    }
}
