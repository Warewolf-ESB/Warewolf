/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common.Interfaces;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Warewolf.Resource.Messages;
using Warewolf.Storage;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Evaluates a test's top-level <c>Outputs</c> and <c>NoErrorExpected</c>/<c>ErrorExpected</c>/
    /// <c>ErrorContainsText</c> expectations against the post-execution environment.
    ///
    /// <para>
    /// Unlike per-step <c>testSteps[]</c> assertions (already handled automatically by shared
    /// <c>Dev2.Activities</c> code — see <see cref="WorkflowExecutor.ExecuteTestActivityChain"/>'s
    /// remarks), a test's top-level <c>Outputs</c> are an orchestration-level concern with no
    /// activity to hook: on the full server this is <c>Evaluator.GetTestResults</c>/
    /// <c>ValidateError</c> (<c>Dev2.Runtime/ESB/Execution/Evaluator.cs:209-328</c>), ported here
    /// near-verbatim. The actual 36-operator comparators are reused via
    /// <see cref="Dev2DecisionFactory"/>/<see cref="FindRecsetOptions"/>/<see cref="DecisionDisplayHelper"/>
    /// — not reimplemented.
    /// </para>
    ///
    /// <para>
    /// <b>Canonical <c>Is Between</c>/<c>Not Between</c> operand order.</b> Uses
    /// <c>[actual, From, To]</c> — <c>Evaluator.cs</c>'s own order — rather than
    /// <c>Dev2.Activities.ServiceTestHelper.GetTestRunResults</c>'s reversed <c>[actual, To, From]</c>
    /// (a genuine inconsistency in the shared codebase between the two call sites; see
    /// docs/WorkflowTestFramework-Plan.md decision 5). <c>ErrorContainsText</c> matching is
    /// case-insensitive, matching <c>Evaluator.ValidateError</c>'s own rule (also chosen as
    /// canonical there, over <c>Evaluator.SetTestFailureBasedOnExpectedError</c>'s case-sensitive
    /// variant, which Lightweight has no equivalent code path for).
    /// </para>
    /// </summary>
    internal static class TestOutputEvaluator
    {
        internal static (bool Passed, string FailureMessage) Evaluate(IDSFDataObject dataObject, IServiceTestModelTO test)
        {
            var testPassed = true;
            var failureMessage = new StringBuilder();

            if (test.Outputs is { Count: > 0 })
            {
                var factory = Dev2DecisionFactory.Instance();
                var testRunResults = test.Outputs.SelectMany(output => GetTestRunResults(dataObject, output, factory)).ToList();
                testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed);
                if (!testPassed)
                {
                    failureMessage.Append(string.Join(
                        "",
                        testRunResults.Select(result => result.Message).Where(s => !string.IsNullOrEmpty(s))));
                }
            }

            testPassed = ValidateError(dataObject, test, testPassed, failureMessage);

            return (testPassed, failureMessage.ToString());
        }

        // Ports Evaluator.GetTestRunResults (Evaluator.cs:224-299) — canonical [actual, From, To]
        // operand order for 3-argument operators, per this type's remarks.
        static IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory)
        {
            var expressionType = output.AssertOp ?? string.Empty;
            var opt = FindRecsetOptions.FindMatch(expressionType);
            var decisionType = DecisionDisplayHelper.GetValue(expressionType);

            if (decisionType == enDecisionType.IsError)
            {
                var testResult = new TestRunResult();
                if (dataObject.Environment.AllErrors.Any())
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(Messages.Test_FailureResult).ToString();
                }

                return new[] { testResult };
            }

            if (decisionType == enDecisionType.IsNotError)
            {
                var testResult = new TestRunResult();
                var actMsg = dataObject.Environment.FetchErrors();
                if (string.IsNullOrWhiteSpace(actMsg))
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine("Failed: " + actMsg).ToString();
                }

                return new[] { testResult };
            }

            var value = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.Value) };
            var from = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.From) };
            var to = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.To) };

            IList<TestRunResult> ret = new List<TestRunResult>();
            var iter = new WarewolfListIterator();
            var variable = DataListUtil.AddBracketsToValueIfNotExist(output.Variable);
            var cols1 = dataObject.Environment.EvalAsList(variable, 0);
            var c1 = new WarewolfAtomIterator(cols1);
            var c2 = new WarewolfAtomIterator(value);
            var c3 = new WarewolfAtomIterator(to);
            if (opt.ArgumentCount > 2)
            {
                c2 = new WarewolfAtomIterator(from);
            }

            iter.AddVariableToIterateOn(c1);
            iter.AddVariableToIterateOn(c2);
            iter.AddVariableToIterateOn(c3);
            while (iter.HasMoreData())
            {
                var val1 = iter.FetchNextValue(c1);
                var val2 = iter.FetchNextValue(c2);
                var val3 = iter.FetchNextValue(c3);
                var assertResult = factory.FetchDecisionFunction(decisionType).Invoke(new[] { val1, val2, val3 });
                var testResult = new TestRunResult();
                if (assertResult)
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    var msg = DecisionDisplayHelper.GetFailureMessage(decisionType);
                    var actMsg = string.Format(msg, val2, variable, val1, val3);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                }

                output.Result = testResult;
                ret.Add(testResult);
            }

            return ret;
        }

        // Ports Evaluator.ValidateError (Evaluator.cs:301-328) verbatim.
        static bool ValidateError(IDSFDataObject dataObject, IServiceTestModelTO test, bool testPassed, StringBuilder failureMessage)
        {
            var fetchErrors = dataObject.Environment.FetchErrors();
            var hasErrors = dataObject.Environment.HasErrors();
            var result = testPassed;
            if (test.ErrorExpected)
            {
                var testErrorContainsText = test.ErrorContainsText ?? "";
                result = hasErrors && result &&
                         fetchErrors.ToLower(CultureInfo.InvariantCulture).Contains(testErrorContainsText.ToLower(CultureInfo.InvariantCulture));
                if (!result)
                {
                    failureMessage.Append(string.Format(Messages.Test_FailureMessage_Error, testErrorContainsText, fetchErrors));
                }
            }
            else if (test.NoErrorExpected)
            {
                result = !hasErrors && result;
                if (hasErrors)
                {
                    failureMessage.AppendLine(fetchErrors);
                }
            }

            return result;
        }
    }
}
