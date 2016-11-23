/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Activities.Debug;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities
{

    [ToolDescriptorInfo("ControlFlow-Sequence", "Sequence", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Sequence_Tags")]
    public class DsfSequenceActivity : DsfActivityAbstract<string>
    {
        private readonly Sequence _innerSequence = new Sequence();
        string _previousParentID;
        private Guid _originalUniqueID;

        public DsfSequenceActivity()
        {
            DisplayName = "Sequence";
            Activities = new Collection<Activity>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddChild(_innerSequence);
        }


        public Collection<Activity> Activities
        {
            get;
            set;
        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }
        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            return DebugItem.EmptyList;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachInputs(updates);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachOutputs(updates);
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var forEachInputs = new List<DsfForEachItem>();

            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    forEachInputs.AddRange(innerActivity.GetForEachInputs());
                }
            }

            return forEachInputs;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var forEachOutputs = new List<DsfForEachItem>();

            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    forEachOutputs.AddRange(innerActivity.GetForEachOutputs());
                }
            }

            return forEachOutputs;
        }

        #endregion

        #region Overrides of DsfNativeActivity<string>

        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentID = dataObject.ParentInstanceID;
        }
        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            var isNestedForEach = dataObject.ForEachNestingLevel > 0;
            if (!isNestedForEach || _originalUniqueID==Guid.Empty)
            {
                _originalUniqueID = WorkSurfaceMappingId;
            }
            UniqueID = isNestedForEach ? Guid.NewGuid().ToString() : UniqueID;
        }
        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            dataObject.ForEachNestingLevel++;
            InitializeDebug(dataObject);
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, 0);
            }
            dataObject.ParentInstanceID = UniqueID;
            dataObject.IsDebugNested = true;
            _innerSequence.Activities.Clear();
            foreach (var dsfActivity in Activities)
            {
                _innerSequence.Activities.Add(dsfActivity);
            }

            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After, 0);
            }
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _previousParentID = dataObject.ParentInstanceID;
            dataObject.ForEachNestingLevel++;
            InitializeDebug(dataObject);
            if(dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, update);
            }
            dataObject.ParentInstanceID = UniqueID;
            dataObject.IsDebugNested = true;
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After, update);
            }
            var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == _originalUniqueID);
            var serviceTestSteps = serviceTestStep?.Children;
            foreach (var dsfActivity in Activities)
            {
                var act = dsfActivity as IDev2Activity;
                if (act != null)
                {
                    act.Execute(dataObject, update);
                    if (dataObject.IsServiceTestExecution)
                    {
                        UpdateDebugStateWithAssertions(dataObject, serviceTestSteps?.ToList(),Guid.Parse(act.UniqueID));
                    }
                }
            }
            if (dataObject.IsServiceTestExecution)
            {
                if (serviceTestStep != null)
                {
                    var testRunResult = new TestRunResult();
                    GetFinalTestRunResult(serviceTestStep, testRunResult);
                    serviceTestStep.Result = testRunResult;
                }
            }
            OnCompleted(dataObject);
            if (dataObject.IsDebugMode())
            {
                _debugOutputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();
                DispatchDebugState(dataObject, StateType.Duration, update);
            }
        }

        private static void GetFinalTestRunResult(IServiceTestStep serviceTestStep, TestRunResult testRunResult)
        {
            ObservableCollection<TestRunResult> resultList = new ObservableCollection<TestRunResult>();
            foreach (var testStep in serviceTestStep.Children)
            {
                if (testStep.Result != null)
                {
                    resultList.Add(testStep.Result);
                }
            }

            if (resultList.Count == 0)
            {
                testRunResult.RunTestResult = RunResult.TestPassed;
            }
            else
            {
                testRunResult.RunTestResult = RunResult.TestInvalid;

                var testRunResults = resultList.Any(runResult => runResult.RunTestResult == RunResult.TestInvalid);
                if (testRunResults)
                {
                    testRunResult.Message = Warewolf.Resource.Messages.Messages.Test_InvalidResult;
                    testRunResult.RunTestResult = RunResult.TestInvalid;
                }
                else
                {
                    testRunResults = resultList.All(runResult => runResult.RunTestResult == RunResult.TestPassed);
                    if (testRunResults)
                    {
                        testRunResult.Message = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                        testRunResult.RunTestResult = RunResult.TestPassed;
                    }
                    else
                    {
                        testRunResult.Message = Warewolf.Resource.Messages.Messages.Test_FailureResult;
                        testRunResult.RunTestResult = RunResult.TestFailed;
                    }
                }
            }
        }

        void OnCompleted(IDSFDataObject dataObject)
        {
            dataObject.IsDebugNested = false;
            dataObject.ParentInstanceID = _previousParentID;
            dataObject.ForEachNestingLevel--;
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.Sequence;
        }

        #endregion

        private void UpdateDebugStateWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps, Guid childId)
        {
            if (dataObject.IsServiceTestExecution && serviceTestTestSteps != null)
            {
                var stepToBeAsserted = serviceTestTestSteps.FirstOrDefault(step => step.Type == StepType.Assert && step.UniqueId == childId && step.ActivityType != typeof(DsfForEachActivity).Name && step.ActivityType != typeof(DsfSelectAndApplyActivity).Name && step.ActivityType != typeof(DsfSequenceActivity).Name);
                if (stepToBeAsserted?.StepOutputs != null && stepToBeAsserted.StepOutputs.Count > 0)
                {
                    if (stepToBeAsserted.Result != null)
                    {
                        stepToBeAsserted.Result.RunTestResult = RunResult.TestInvalid;
                    }
                    var debugItems = TestDebugMessageRepo.Instance.GetDebugItems(dataObject.ResourceID, dataObject.TestName);
                    debugItems = debugItems.Where(state => state.ID == stepToBeAsserted.UniqueId).ToList();
                    var debugStates = debugItems.LastOrDefault();
                    var factory = Dev2DecisionFactory.Instance();
                    var res = stepToBeAsserted.StepOutputs.SelectMany(output => GetTestRunResults(dataObject, output, factory, debugStates));
                    var testRunResults = res as IList<TestRunResult> ?? res.ToList();
                    var testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed);
                    var serviceTestFailureMessage = string.Join("", testRunResults.Select(result => result.Message));

                    var finalResult = new TestRunResult();
                    if (testPassed)
                    {
                        finalResult.RunTestResult = RunResult.TestPassed;
                    }
                    if (testRunResults.Any(result => result.RunTestResult == RunResult.TestFailed))
                    {
                        finalResult.RunTestResult = RunResult.TestFailed;
                        finalResult.Message = serviceTestFailureMessage;
                    }
                    if (testRunResults.Any(result => result.RunTestResult == RunResult.TestInvalid))
                    {
                        finalResult.RunTestResult = RunResult.TestInvalid;
                        finalResult.Message = serviceTestFailureMessage;
                    }
                    stepToBeAsserted.Result = finalResult;
                    dataObject.StopExecution = !testPassed;
                }
            }
        }

        private IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory, IDebugState debugState)
        {
            if (output == null)
            {
                return new List<TestRunResult>();
            }
            if (output.Result != null)
            {
                output.Result.RunTestResult = RunResult.TestInvalid;
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
                    var actMsg = string.Format(msg, val1, val2, val3);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                }
                if (dataObject.IsDebugMode())
                {
                    var msg = testResult.Message;
                    if (testResult.RunTestResult == RunResult.TestPassed)
                    {
                        msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                    }
                    var hasError = testResult.RunTestResult == RunResult.TestFailed;
                    var debugItemStaticDataParams = new DebugItemServiceTestStaticDataParams(msg, hasError);
                    DebugItem itemToAdd = new DebugItem();
                    itemToAdd.AddRange(debugItemStaticDataParams.GetDebugItemResult());

                    if (debugState.AssertResultList != null)
                    {
                        bool addItem = debugState.AssertResultList.Select(debugItem => debugItem.ResultsList.Where(debugItemResult => debugItemResult.Value == Warewolf.Resource.Messages.Messages.Test_PassedResult)).All(debugItemResults => !debugItemResults.Any());

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

        public Guid GetOriginalID()
        {
            return _originalUniqueID;
        }
    }
}
