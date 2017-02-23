using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Activities.SelectAndApply;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Resource.Messages;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public static class ServiceTestHelper
    {
        private static IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory, IDebugState debugState)
        {
            if (output == null)
            {
                var testResult = new TestRunResult
                {
                    RunTestResult = RunResult.None
                };
                return new List<TestRunResult> { testResult };
            }
            if(string.IsNullOrEmpty(output.Variable) && string.IsNullOrEmpty(output.Value))
            {
                var testResult = new TestRunResult
                {
                    RunTestResult = RunResult.None
                };
                output.Result = testResult;
                return new List<TestRunResult> { testResult };
            }
            if (output.Result != null)
            {
                output.Result.RunTestResult = RunResult.TestInvalid;
            }
            if (string.IsNullOrEmpty(output.Variable))
            {
                var testResult = new TestRunResult
                {
                    RunTestResult = RunResult.TestInvalid,
                    Message = Messages.Test_NothingToAssert
                };
                output.Result = testResult;
                if (dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(testResult.Message, true);
                    DebugItem itemToAdd = new DebugItem();
                    itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());
                    debugState.AssertResultList.Add(itemToAdd);
                }
                return new List<TestRunResult> { testResult };
            }
            IFindRecsetOptions opt = FindRecsetOptions.FindMatch(output.AssertOp);
            var decisionType = DecisionDisplayHelper.GetValue(output.AssertOp);
            var value = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.Value) };
            var from = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.From) };
            var to = new List<DataStorage.WarewolfAtom> { DataStorage.WarewolfAtom.NewDataString(output.To) };

            IList<TestRunResult> ret = new List<TestRunResult>();
            var iter = new WarewolfListIterator();
            var cols1 = dataObject.Environment.EvalAsList(DataListUtil.AddBracketsToValueIfNotExist(output.Variable), 0);
            var c1 = new WarewolfAtomIterator(cols1);
            var c2 = new WarewolfAtomIterator(value);
            var c3 = new WarewolfAtomIterator(@from);
            if (opt.ArgumentCount > 2)
            {
                c2 = new WarewolfAtomIterator(to);
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
                    var actMsg = string.Format(msg, val2, output.Variable, val1, val3);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                }
                if (dataObject.IsDebugMode())
                {
                    var msg = testResult.Message;
                    if (testResult.RunTestResult == RunResult.TestPassed)
                    {
                        msg = Messages.Test_PassedResult;
                    }

                    var hasError = testResult.RunTestResult == RunResult.TestFailed;

                    var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(msg, hasError);
                    DebugItem itemToAdd = new DebugItem();
                    itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());

                    if (debugState.AssertResultList != null)
                    {
                        bool addItem = debugState.AssertResultList.Select(debugItem => debugItem.ResultsList.Where(debugItemResult => debugItemResult.Value == Messages.Test_PassedResult)).All(debugItemResults => !debugItemResults.Any());

                        if (addItem)
                        {
                            debugState.AssertResultList.Add(itemToAdd);
                        }
                    }
                }
                output.Result = testResult;
                ret.Add(testResult);
            }
            return ret;

        }

        public static void UpdateDebugStateWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps,string childUniqueID)
        {
            if (dataObject.IsServiceTestExecution && serviceTestTestSteps != null)
            {
                var stepToBeAsserted = serviceTestTestSteps.FirstOrDefault(step => step.Type == StepType.Assert && step.UniqueId == Guid.Parse(childUniqueID) && step.ActivityType != typeof(DsfForEachActivity).Name && step.ActivityType != typeof(DsfSelectAndApplyActivity).Name && step.ActivityType != typeof(DsfSequenceActivity).Name);
                if (stepToBeAsserted != null)
                {
                    GetStepOutputResults(dataObject, stepToBeAsserted);
                }
            }
        }

        private static void GetStepOutputResults(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
        {
            if (stepToBeAsserted?.StepOutputs != null && stepToBeAsserted.StepOutputs.Count > 0)
            {
                if (stepToBeAsserted.Result != null)
                {
                    stepToBeAsserted.Result.RunTestResult = RunResult.TestInvalid;
                }
                else
                {
                    stepToBeAsserted.Result = new TestRunResult { RunTestResult = RunResult.TestInvalid };
                }
                var states = TestDebugMessageRepo.Instance.GetDebugItems(dataObject.ResourceID, dataObject.TestName);
                if (states != null)
                {
                    states = states.Where(state => state.ID == stepToBeAsserted.UniqueId).ToList();
                    var debugState = states.LastOrDefault();
                    UpdateDebugStateWithAssertion(dataObject, stepToBeAsserted, debugState);
                }
            }
        }

        private static void UpdateDebugStateWithAssertion(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted, IDebugState debugState)
        {
            if(debugState != null)
            {
                var factory = Dev2DecisionFactory.Instance();
                var res = stepToBeAsserted.StepOutputs.SelectMany(output => GetTestRunResults(dataObject, output, factory, debugState));
                var testRunResults = res as IList<TestRunResult> ?? res.ToList();
                var testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed || result.RunTestResult == RunResult.None);
                var serviceTestFailureMessage = string.Join("", testRunResults.Select(result => result.Message));

                UpdateBasedOnFinalResult(dataObject, stepToBeAsserted, testPassed, testRunResults, serviceTestFailureMessage);
            }
        }

        public static void UpdateBasedOnFinalResult(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted, bool testPassed, IList<TestRunResult> testRunResults, string serviceTestFailureMessage)
        {
            var finalResult = new TestRunResult();
            if(testPassed)
            {
                finalResult.RunTestResult = RunResult.TestPassed;
            }
            if(testRunResults.Any(result => result.RunTestResult == RunResult.TestFailed))
            {
                finalResult.RunTestResult = RunResult.TestFailed;
                finalResult.Message = serviceTestFailureMessage;
            }
            if(testRunResults.Any(result => result.RunTestResult == RunResult.TestInvalid))
            {
                finalResult.RunTestResult = RunResult.TestInvalid;
                finalResult.Message = serviceTestFailureMessage;
            }
            stepToBeAsserted.Result = finalResult;
            dataObject.StopExecution = !testPassed;
        }
    }
}