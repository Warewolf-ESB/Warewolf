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
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Dev2.Activities;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Runtime.ESB.Execution
{
    internal interface IEvaluator
    {
        IServiceTestModelTO TryEval(Guid resourceId, IDSFDataObject dataObject, IServiceTestModelTO serviceTest);
    }
    internal class Evaluator : IEvaluator
    {
        readonly IDSFDataObject _dataObject;
        readonly IResourceCatalog _resourceCatalog;
        readonly IWorkspace _workspace;

        internal Evaluator(IDSFDataObject dataObj, IResourceCatalog resourceCat, IWorkspace theWorkspace)
        {
            _dataObject = dataObj;
            _resourceCatalog = resourceCat;
            _workspace = theWorkspace;
        }

        void UpdateToPending(IList<IServiceTestStep> testSteps)
        {
            if (testSteps is null)
            {
                return;
            }
            foreach (var serviceTestStep in testSteps)
            {
                UpdateToPending(serviceTestStep);
            }
        }

        void UpdateToPending(IServiceTestStep serviceTestStep)
        {
            if (serviceTestStep is null)
            {
                return;
            }
            if (serviceTestStep.Result != null)
            {
                serviceTestStep.Result.RunTestResult = RunResult.TestPending;
            }
            else
            {
                serviceTestStep.Result = new TestRunResult { RunTestResult = RunResult.TestPending };
            }
            UpdateToPending(serviceTestStep.StepOutputs);
            if (serviceTestStep.Children != null && serviceTestStep.Children.Count > 0)
            {
                UpdateToPending(serviceTestStep.Children);
            }
        }

        static void UpdateToPending(IEnumerable<IServiceTestOutput> stepOutputs)
        {
            var serviceTestOutputs = stepOutputs as IList<IServiceTestOutput> ?? stepOutputs.ToList();
            if (serviceTestOutputs.Count > 0)
            {
                foreach (var serviceTestOutput in serviceTestOutputs)
                {
                    UpdateToPending(serviceTestOutput);
                }
            }
        }

        static void UpdateToPending(IServiceTestOutput serviceTestOutput)
        {
            if (serviceTestOutput?.Result != null)
            {
                serviceTestOutput.Result.RunTestResult = RunResult.TestPending;
            }
            else
            {
                if (serviceTestOutput != null)
                {
                    serviceTestOutput.Result = new TestRunResult { RunTestResult = RunResult.TestPending };
                }
            }
        }

        public IServiceTestModelTO TryEval(Guid resourceId, IDSFDataObject dataObject, IServiceTestModelTO serviceTest)
        {
            var testPassed = true;
            var canExecute = true;
            var failureMessage = new StringBuilder();
            if (ServerAuthorizationService.Instance != null)
            {
                var authorizationService = ServerAuthorizationService.Instance;
                var hasView = authorizationService.IsAuthorized(_dataObject.ExecutingUser, AuthorizationContext.View, _dataObject.ResourceID.ToString());
                var hasExecute = authorizationService.IsAuthorized(_dataObject.ExecutingUser, AuthorizationContext.Execute, _dataObject.ResourceID.ToString());
                canExecute = hasExecute && hasView;
            }
            if (!canExecute)
            {
                dataObject.Environment.AllErrors.Add("Unauthorized to execute this resource.");
            }
            else
            {
                if (!dataObject.StopExecution)
                {
                    var startActivity = LoadResource(resourceId);
                    PrepareForEval(dataObject, serviceTest);
                    Evaluate(dataObject, startActivity, dataObject.ForEachUpdateValue, serviceTest.TestSteps);
                    GetTestResults(dataObject, serviceTest, ref testPassed, ref failureMessage);
                }
            }
            ValidateError(serviceTest, testPassed, failureMessage);
            serviceTest.FailureMessage = failureMessage.ToString();
            return serviceTest;
        }

        IDev2Activity LoadResource(Guid resourceId)
        {
            Dev2Logger.Debug("Getting Resource to Execute", GlobalConstants.WarewolfDebug);
            var startActivity = _resourceCatalog.Parse(_workspace.ID, resourceId);
            startActivity = CloneResource(startActivity);
            if (startActivity is null)
            {
                throw new InvalidOperationException(GlobalConstants.NoStartNodeError);
            }
            Dev2Logger.Debug("Got Resource to Execute", GlobalConstants.WarewolfDebug);
            return startActivity;
        }

        static IDev2Activity CloneResource(IDev2Activity resource)
        {
            var serializer = new Dev2JsonSerializer();
            var executionPlan = serializer.SerializeToBuilder(resource);
            return serializer.Deserialize<IDev2Activity>(executionPlan);
        }

        void PrepareForEval(IDSFDataObject dataObject, IServiceTestModelTO serviceTest)
        {
            dataObject.ServiceTest = serviceTest;
            UpdateToPending(serviceTest.TestSteps);
        }

        static void Evaluate(IDSFDataObject dsfDataObject, IDev2Activity activity, int update, List<IServiceTestStep> testSteps)
        {
            WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;

            var activityOrMock = MockActivityIfNecessary(activity, testSteps);
            var next = activityOrMock.Execute(dsfDataObject, update);
            while (next != null)
            {
                if (!dsfDataObject.StopExecution)
                {
                    next = MockActivityIfNecessary(next, testSteps);
                    next = next.Execute(dsfDataObject, update);
                    foreach (var error in dsfDataObject.Environment.Errors)
                    {
                        dsfDataObject.Environment.AllErrors.Add(error);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        static void GetTestResults(IDSFDataObject dataObject, IServiceTestModelTO test, ref bool testPassed, ref StringBuilder failureMessage)
        {
            if (test.Outputs is null)
            {
                return;
            }
            var dev2DecisionFactory = Dev2DecisionFactory.Instance();
            var testRunResults = test.Outputs.SelectMany(output => GetTestRunResults(dataObject, output, dev2DecisionFactory)).ToList();
            testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed);
            if (!testPassed)
            {
                failureMessage = failureMessage.Append(string.Join("", testRunResults.Select(result => result.Message).Where(s => !string.IsNullOrEmpty(s)).ToList()));
            }
        }

        static IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory)
        {
            var expressionType = output.AssertOp ?? string.Empty;
            var opt = FindRecsetOptions.FindMatch(expressionType);
            var decisionType = DecisionDisplayHelper.GetValue(expressionType);

            if (decisionType == enDecisionType.IsError)
            {
                var hasErrors = dataObject.Environment.AllErrors.Count > 0;
                var testResult = new TestRunResult();
                if (hasErrors)
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    var msg = DecisionDisplayHelper.GetFailureMessage(decisionType);
                    var actMsg = string.Format(msg);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                }
                return new[] { testResult };
            }
            if (decisionType == enDecisionType.IsNotError)
            {
                var noErrors = dataObject.Environment.AllErrors.Count == 0;
                var testResult = new TestRunResult();
                if (noErrors)
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    var msg = DecisionDisplayHelper.GetFailureMessage(decisionType);
                    var actMsg = string.Format(msg);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
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

        void ValidateError(IServiceTestModelTO test, bool testPassed, StringBuilder failureMessage)
        {
            var fetchErrors = _dataObject.Environment.FetchErrors();
            var hasErrors = _dataObject.Environment.HasErrors();
            var result = testPassed;
            if (test.ErrorExpected)
            {
                var testErrorContainsText = test.ErrorContainsText ?? "";
                result = hasErrors && result && fetchErrors.ToLower(CultureInfo.InvariantCulture).Contains(testErrorContainsText.ToLower(CultureInfo.InvariantCulture));
                if (!result)
                {
                    failureMessage.Append(string.Format(Warewolf.Resource.Messages.Messages.Test_FailureMessage_Error, testErrorContainsText, fetchErrors));
                }
            }
            else
            {
                if (test.NoErrorExpected)
                {
                    result = !hasErrors && result;
                    if (hasErrors)
                    {
                        failureMessage.AppendLine(fetchErrors);
                    }
                }
            }
            test.TestPassed = result;
            test.TestFailing = !result;
        }

        /// <summary>
        /// Depending on whether an activity should be run or mocked this method replaces the activity with a mock
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="testSteps"></param>
        /// <returns></returns>
        static IDev2Activity MockActivityIfNecessary(IDev2Activity activity, List<IServiceTestStep> testSteps)
        {
            IDev2Activity overriddenActivity = null;
            var foundTestStep = testSteps?.FirstOrDefault(step => activity != null && step.UniqueId.ToString() == activity.UniqueID);
            if (foundTestStep != null)
            {
                var shouldMock = foundTestStep.Type == StepType.Mock;
                var shouldRecursivelyMock = foundTestStep.ActivityType == typeof(DsfSequenceActivity).Name
                                            || foundTestStep.ActivityType == typeof(DsfForEachActivity).Name
                                            || foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name;

                if (shouldMock && !shouldRecursivelyMock)
                {
                    overriddenActivity = ReplaceActivityWithMock(activity, foundTestStep);
                }
                else
                {
                    RecursivelyMockRecursiveActivities(activity, foundTestStep);
                }
            }
            return overriddenActivity ?? activity;
        }

        private static IDev2Activity ReplaceActivityWithMock(IDev2Activity resource, IServiceTestStep foundTestStep)
        {
            IDev2Activity overriddenActivity = null;
            if (foundTestStep.ActivityType == typeof(DsfDecision).Name)
            {
                var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == GlobalConstants.ArmResultText);
                if (serviceTestOutput != null)
                {
                    overriddenActivity = new TestMockDecisionStep(resource.As<DsfDecision>()) { NameOfArmToReturn = serviceTestOutput.Value };
                }
            }
            else if (foundTestStep.ActivityType == typeof(DsfSwitch).Name)
            {
                var serviceTestOutput = foundTestStep.StepOutputs.FirstOrDefault(output => output.Variable == GlobalConstants.ArmResultText);
                if (serviceTestOutput != null)
                {
                    overriddenActivity = new TestMockSwitchStep(resource.As<DsfSwitch>()) { ConditionToUse = serviceTestOutput.Value };
                }
            }
            else
            {
                overriddenActivity = new TestMockStep(resource, foundTestStep.StepOutputs.ToList());
            }

            return overriddenActivity;
        }

        private static void RecursivelyMockRecursiveActivities(IDev2Activity activity, IServiceTestStep foundTestStep)
        {
            if (foundTestStep.ActivityType == typeof(DsfSequenceActivity).Name)
            {
                if (activity is DsfSequenceActivity sequenceActivity)
                {
                    RecursivelyMockChildrenOfASequence(foundTestStep, sequenceActivity);
                }
            }
            else if (foundTestStep.ActivityType == typeof(DsfForEachActivity).Name && activity is DsfForEachActivity forEach && foundTestStep.Children != null)
            {
                var replacement = MockActivityIfNecessary(forEach.DataFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                forEach.DataFunc.Handler = replacement;
            }
            else if (foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name && activity is DsfSelectAndApplyActivity selectAndApplyActivity && foundTestStep.Children != null)
            {
                var replacement = MockActivityIfNecessary(selectAndApplyActivity.ApplyActivityFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                selectAndApplyActivity.ApplyActivityFunc.Handler = replacement;
            }
        }

        private static void RecursivelyMockChildrenOfASequence(IServiceTestStep foundTestStep, DsfSequenceActivity sequenceActivity)
        {
            var acts = sequenceActivity.Activities;

            for (int index = 0; index < acts.Count; index++)
            {
                var activity = acts[index];
                if (foundTestStep.Children != null)
                {
                    var replacement = MockActivityIfNecessary(activity as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                    acts[index] = replacement;
                }
            }
        }
    }
}