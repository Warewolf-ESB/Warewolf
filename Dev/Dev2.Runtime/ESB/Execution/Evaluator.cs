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
using Dev2.Activities.Debug;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Execution;
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
    internal interface IExecutingEvaluator
    {
        Guid ExecuteWf(IServiceTestModelTO test, WfApplicationUtils wfappUtils, ErrorResultTO invokeErrors, Dev2JsonSerializer serializer);
    }
    internal class Evaluator : IEvaluator
    {
        protected readonly IDSFDataObject _dataObject;
        protected readonly IResourceCatalog _resourceCatalog;
        protected readonly IWorkspace _workspace;
        protected readonly IBuilderSerializer _serializer;

        internal Evaluator(IDSFDataObject dataObj, IResourceCatalog resourceCat, IWorkspace theWorkspace)
            : this(dataObj, resourceCat, theWorkspace, new Dev2JsonSerializer())
        {
        }

        internal Evaluator(IDSFDataObject dataObj, IResourceCatalog resourceCat, IWorkspace theWorkspace, IBuilderSerializer serializer)
        {
            _dataObject = dataObj;
            _resourceCatalog = resourceCat;
            _workspace = theWorkspace;
            _serializer = serializer;
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

        IDev2Activity CloneResource(IDev2Activity resource)
        {
            var executionPlan = _serializer.SerializeToBuilder(resource);
            return _serializer.Deserialize<IDev2Activity>(executionPlan);
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

        static IDev2Activity ReplaceActivityWithMock(IDev2Activity resource, IServiceTestStep foundTestStep)
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

        static void RecursivelyMockRecursiveActivities(IDev2Activity activity, IServiceTestStep foundTestStep)
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
            else
            {
                if (foundTestStep.ActivityType == typeof(DsfSelectAndApplyActivity).Name && activity is DsfSelectAndApplyActivity selectAndApplyActivity && foundTestStep.Children != null)
                {
                    var replacement = MockActivityIfNecessary(selectAndApplyActivity.ApplyActivityFunc.Handler as IDev2Activity, foundTestStep.Children.ToList()) as Activity;
                    selectAndApplyActivity.ApplyActivityFunc.Handler = replacement;
                }
            }
        }

        static void RecursivelyMockChildrenOfASequence(IServiceTestStep foundTestStep, DsfSequenceActivity sequenceActivity)
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

    internal class ExecutingEvaluator : Evaluator, IExecutingEvaluator
    {
        protected readonly EsbExecuteRequest _request;

        internal ExecutingEvaluator(IDSFDataObject dataObj, IResourceCatalog resourceCat, IWorkspace theWorkspace, EsbExecuteRequest request)
            : base(dataObj, resourceCat, theWorkspace)
        {
            _request = request;
        }

        public Guid ExecuteWf(IServiceTestModelTO test, WfApplicationUtils wfappUtils, ErrorResultTO invokeErrors, Dev2JsonSerializer serializer)
        {
            var resourceId = _dataObject.ResourceID;

            Guid result;
            IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
            _dataObject.ExecutionToken = exeToken;

            if (_dataObject.IsDebugMode())
            {
                var debugState = wfappUtils.GetDebugState(_dataObject, StateType.Start, invokeErrors, interrogateInputs: true);
                wfappUtils.TryWriteDebug(_dataObject, debugState);
            }

            if (test is null)
            {
                throw new Exception($"Test {_dataObject.TestName} for Resource {_dataObject.ServiceName} ID {resourceId}");
            }

            var testRunResult = TryEval(resourceId, _dataObject, test);

            if (_dataObject.IsDebugMode())
            {
                if (!_dataObject.StopExecution)
                {
                    var debugState = wfappUtils.GetDebugState(_dataObject, StateType.End, invokeErrors, interrogateOutputs: true, durationVisible: true);
                    var outputDebugItem = new DebugItem();
                    if (test != null)
                    {
                        var msg = test.TestPassed ? Warewolf.Resource.Messages.Messages.Test_PassedResult : test.FailureMessage;
                        outputDebugItem.AddRange(new DebugItemServiceTestStaticDataParams(msg, test.TestFailing).GetDebugItemResult());
                    }
                    debugState.AssertResultList.Add(outputDebugItem);
                    wfappUtils.TryWriteDebug(_dataObject, debugState);
                }
                DebugState testAggregateDebugState;
                if (_dataObject.StopExecution && _dataObject.Environment.HasErrors())
                {
                    var existingErrors = _dataObject.Environment.FetchErrors();
                    _dataObject.Environment.AllErrors.Clear();
                    testAggregateDebugState = wfappUtils.GetDebugState(_dataObject, StateType.TestAggregate, new ErrorResultTO());
                    SetTestFailureBasedOnExpectedError(test, existingErrors);
                }
                else
                {
                    testAggregateDebugState = wfappUtils.GetDebugState(_dataObject, StateType.TestAggregate, new ErrorResultTO());
                    AggregateTestResult(resourceId, test);
                }

                var itemToAdd = new DebugItem();
                if (test != null)
                {
                    var msg = test.FailureMessage;
                    if (test.TestPassed)
                    {
                        msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                    }
                    itemToAdd.AddRange(new DebugItemServiceTestStaticDataParams(msg, test.TestFailing).GetDebugItemResult());
                }
                testAggregateDebugState.AssertResultList.Add(itemToAdd);
                wfappUtils.TryWriteDebug(_dataObject, testAggregateDebugState);

                if (testRunResult != null)
                {
                    if (test?.Result != null)
                    {
                        test.Result.DebugForTest = TestDebugMessageRepo.Instance.FetchDebugItems(resourceId, test.TestName);
                    }

                    _request.ExecuteResult = serializer.SerializeToBuilder(testRunResult);
                }
            }
            else
            {
                if (_dataObject.StopExecution && _dataObject.Environment.HasErrors())
                {
                    var existingErrors = _dataObject.Environment.FetchErrors();
                    _dataObject.Environment.AllErrors.Clear();
                    SetTestFailureBasedOnExpectedError(test, existingErrors);
                    _request.ExecuteResult = serializer.SerializeToBuilder(test);
                }
                else
                {
                    AggregateTestResult(resourceId, test);
                    if (test != null)
                    {
                        _request.ExecuteResult = serializer.SerializeToBuilder(test);
                    }
                }
            }
            result = _dataObject.DataListID;
            return result;
        }


        static void SetTestFailureBasedOnExpectedError(IServiceTestModelTO test, string existingErrors)
        {
            if (test is null)
            {
                return;
            }
            test.Result = new TestRunResult();
            test.FailureMessage = existingErrors;
            if (test.ErrorExpected)
            {
                if (test.FailureMessage.Contains(test.ErrorContainsText) && !string.IsNullOrEmpty(test.ErrorContainsText))
                {
                    test.TestPassed = true;
                    test.TestFailing = false;
                    test.Result.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    test.TestFailing = true;
                    test.TestPassed = false;
                    test.Result.RunTestResult = RunResult.TestFailed;
                    var assertError = string.Format(Warewolf.Resource.Messages.Messages.Test_FailureMessage_Error,
                        test.ErrorContainsText, test.FailureMessage);
                    test.FailureMessage = assertError;
                }
            }
            if (test.NoErrorExpected)
            {
                if (string.IsNullOrEmpty(test.FailureMessage))
                {
                    test.TestPassed = true;
                    test.TestFailing = false;
                    test.Result.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    test.TestFailing = true;
                    test.TestPassed = false;
                    test.Result.RunTestResult = RunResult.TestFailed;
                }
            }
        }

        static void AggregateTestResult(Guid resourceId, IServiceTestModelTO test)
        {
            UpdateTestWithStepValues(test);
            UpdateTestWithFinalResult(resourceId, test);
        }

        static void UpdateTestWithStepValues(IServiceTestModelTO test)
        {
            var testPassed = test.TestPassed;

            var serviceTestSteps = GetStepValues(test, out IEnumerable<IServiceTestStep> pendingSteps, out IEnumerable<IServiceTestStep> invalidSteps, out IEnumerable<IServiceTestStep> failingSteps);
            var failingOutputs = GetOutputValues(test, out IEnumerable<IServiceTestOutput> pendingOutputs, out IEnumerable<IServiceTestOutput> invalidOutputs);
            var invalidTestSteps = GetSteps(invalidSteps, pendingSteps, failingSteps, out IList<IServiceTestStep> pendingTestSteps, out IList<IServiceTestStep> failingTestSteps);
            var pendingTestOutputs = GetOutputs(pendingOutputs, invalidOutputs, failingOutputs, out IList<IServiceTestOutput> invalidTestOutputs, out IList<IServiceTestOutput> failingTestOutputs);

            var hasInvalidSteps = invalidTestSteps?.Any() ?? false;
            var hasPendingSteps = pendingTestSteps?.Any() ?? false;
            var hasFailingSteps = failingTestSteps?.Any() ?? false;
            var hasFailingOutputs = failingTestOutputs?.Any() ?? false;
            var hasPendingOutputs = pendingTestOutputs?.Any() ?? false;
            var hasInvalidOutputs = invalidTestOutputs?.Any() ?? false;
            var testStepPassed = TestPassedBasedOnSteps(hasPendingSteps, hasInvalidSteps, hasFailingSteps) && TestPassedBasedOnOutputs(hasPendingOutputs, hasInvalidOutputs, hasFailingOutputs);

            testPassed = testPassed && testStepPassed;

            var failureMessage = UpdateFailureMessage(hasPendingSteps, pendingTestSteps, hasInvalidSteps, invalidTestSteps, hasFailingSteps, failingTestSteps, hasPendingOutputs, pendingTestOutputs, hasInvalidOutputs, invalidTestOutputs, hasFailingOutputs, failingTestOutputs, serviceTestSteps);
            test.FailureMessage = failureMessage.ToString();
            test.TestFailing = !testPassed;
            test.TestPassed = testPassed;
            test.TestPending = false;
            test.TestInvalid = hasInvalidSteps;
        }

        static StringBuilder UpdateFailureMessage(bool hasPendingSteps, IList<IServiceTestStep> pendingTestSteps, bool hasInvalidSteps, IList<IServiceTestStep> invalidTestSteps, bool hasFailingSteps, IList<IServiceTestStep> failingTestSteps, bool hasPendingOutputs, IList<IServiceTestOutput> pendingTestOutputs, bool hasInvalidOutputs, IList<IServiceTestOutput> invalidTestOutputs, bool hasFailingOutputs, IList<IServiceTestOutput> failingTestOutputs, List<IServiceTestStep> serviceTestSteps)
        {
            var failureMessage = new StringBuilder();
            if (hasFailingSteps)
            {
                foreach (var serviceTestStep in failingTestSteps)
                {
                    failureMessage.AppendLine("Failed Step: " + serviceTestStep.StepDescription + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasInvalidSteps)
            {
                foreach (var serviceTestStep in invalidTestSteps)
                {
                    failureMessage.AppendLine("Invalid Step: " + serviceTestStep.StepDescription + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasPendingSteps)
            {
                foreach (var serviceTestStep in pendingTestSteps)
                {
                    failureMessage.AppendLine("Pending Step: " + serviceTestStep.StepDescription);
                }
            }

            if (hasFailingOutputs)
            {
                foreach (var serviceTestStep in failingTestOutputs)
                {
                    failureMessage.AppendLine("Failed Output For Variable: " + serviceTestStep.Variable + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasInvalidOutputs)
            {
                foreach (var serviceTestStep in invalidTestOutputs)
                {
                    failureMessage.AppendLine("Invalid Output for Variable: " + serviceTestStep.Variable);
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (hasPendingOutputs)
            {
                foreach (var serviceTestStep in pendingTestOutputs)
                {
                    failureMessage.AppendLine("Pending Output for Variable: " + serviceTestStep.Variable);
                }
            }


            if (serviceTestSteps != null)
            {
                failureMessage.AppendLine(string.Join("", serviceTestSteps.Where(step => !string.IsNullOrEmpty(step.Result?.Message)).Select(step => step.Result?.Message)));
            }
            return failureMessage;
        }

        static bool TestPassedBasedOnSteps(bool hasPendingSteps, bool hasInvalidSteps, bool hasFailingSteps) => !hasPendingSteps && !hasInvalidSteps && !hasFailingSteps;

        static bool TestPassedBasedOnOutputs(bool pending, bool invalid, bool failing) => !pending && !invalid && !failing;

        static IList<IServiceTestOutput> GetOutputs(IEnumerable<IServiceTestOutput> pendingOutputs, IEnumerable<IServiceTestOutput> invalidOutputs, IEnumerable<IServiceTestOutput> failingOutputs, out IList<IServiceTestOutput> invalidTestOutputs, out IList<IServiceTestOutput> failingTestOutputs)
        {
            var pendingTestOutputs = pendingOutputs as IList<IServiceTestOutput> ?? pendingOutputs?.ToList();
            invalidTestOutputs = invalidOutputs as IList<IServiceTestOutput> ?? invalidOutputs?.ToList();
            failingTestOutputs = failingOutputs as IList<IServiceTestOutput> ?? failingOutputs?.ToList();
            return pendingTestOutputs;
        }

        static IList<IServiceTestStep> GetSteps(IEnumerable<IServiceTestStep> invalidSteps, IEnumerable<IServiceTestStep> pendingSteps, IEnumerable<IServiceTestStep> failingSteps, out IList<IServiceTestStep> pendingTestSteps, out IList<IServiceTestStep> failingTestSteps)
        {
            var invalidTestSteps = invalidSteps as IList<IServiceTestStep> ?? invalidSteps?.ToList();
            pendingTestSteps = pendingSteps as IList<IServiceTestStep> ?? pendingSteps?.ToList();
            failingTestSteps = failingSteps as IList<IServiceTestStep> ?? failingSteps?.ToList();
            return invalidTestSteps;
        }

        static IEnumerable<IServiceTestOutput> GetOutputValues(IServiceTestModelTO test, out IEnumerable<IServiceTestOutput> pendingOutputs, out IEnumerable<IServiceTestOutput> invalidOutputs)
        {
            var failingOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestFailed);
            pendingOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestPending);
            invalidOutputs = test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestInvalid);
            var serviceTestOutputs = failingOutputs as IServiceTestOutput[] ?? failingOutputs?.ToArray();
            if (serviceTestOutputs?.Any() ?? false)
            {
                foreach (var serviceTestOutput in serviceTestOutputs)
                {
                    serviceTestOutput.Result.Message = DataListUtil.StripBracketsFromValue(serviceTestOutput.Result.Message);
                }
            }
            return serviceTestOutputs;
        }

        static List<IServiceTestStep> GetStepValues(IServiceTestModelTO test, out IEnumerable<IServiceTestStep> pendingSteps, out IEnumerable<IServiceTestStep> invalidSteps, out IEnumerable<IServiceTestStep> failingSteps)
        {
            var serviceTestSteps = test.TestSteps;
            pendingSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestPending);
            invalidSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestInvalid);
            failingSteps = serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestFailed);
            return serviceTestSteps;
        }

        static void UpdateTestWithFinalResult(Guid resourceId, IServiceTestModelTO test)
        {
            test.LastRunDate = DateTime.Now;

            var testRunResult = new TestRunResult { TestName = test.TestName };
            if (test.TestFailing)
            {
                testRunResult.RunTestResult = RunResult.TestFailed;
                testRunResult.Message = test.FailureMessage;
            }
            if (test.TestPassed)
            {
                testRunResult.RunTestResult = RunResult.TestPassed;
            }
            if (test.TestInvalid)
            {
                testRunResult.RunTestResult = RunResult.TestInvalid;
            }
            test.Result = testRunResult;
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () => { TestCatalog.Instance.SaveTest(resourceId, test); });
        }
    }
}
