using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.SelectAndApply
{
    [ToolDescriptorInfo("SelectApply", "Select and apply", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8FA3E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Loop Constructs", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_LoopConstruct_Select and Apply_Tags")]
    public class DsfSelectAndApplyActivity : DsfActivityAbstract<bool>
    {
        public DsfSelectAndApplyActivity()
        {
            DisplayName = "Select and apply";
            ApplyActivityFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>(string.Format("explicitData_{0}", DateTime.Now.ToString("yyyyMMddhhmmss")))
            };
        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(ApplyActivityFunc);

            base.CacheMetadata(metadata);
        }

        #region Overrides of DsfNativeActivity<bool>
        [FindMissing]
        public string DataSource { get; set; }
        [FindMissing]
        public string Alias { get; set; }
        public ActivityFunc<string, bool> ApplyActivityFunc { get; set; }

        private string _previousParentId;
        private string _childUniqueID;

        /// <summary>
        /// When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            UniqueID = dataObject.ForEachNestingLevel > 0 ? Guid.NewGuid().ToString() : UniqueID;
        }

        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentId = dataObject.ParentInstanceID;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Alias);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Alias.Replace("*", ""));
        }

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            return _debugOutputs;
        }

        #endregion Get Debug Inputs/Outputs

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            ErrorResultTO allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);

            if(string.IsNullOrEmpty(DataSource))
            {
                allErrors.AddError(ErrorResource.DataSourceEmpty);
            }
            if(string.IsNullOrEmpty(Alias))
            {
                allErrors.AddError(string.Format(ErrorResource.CanNotBeEmpty, "Alias"));
            }
            if(allErrors.HasErrors())
            {
                DisplayAndWriteError("DsfSelectAndApplyActivity", allErrors);
                foreach(var fetchError in allErrors.FetchErrors())
                {
                    dataObject.Environment.AddError(fetchError);
                }
                return;
            }
            var startTime = DateTime.Now;
            _previousParentId = dataObject.ParentInstanceID;
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            dataObject.ForEachNestingLevel++;
            var ds = dataObject.Environment.ToStar(DataSource);
            var expressions = dataObject.Environment.GetIndexes(ds);
            if(expressions.Count == 0)
            {
                expressions.Add(ds);
            }
            try
            {
                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(new DebugItemStaticDataParams(Alias, "As", DataSource));
                }

                var executionEnvironment = new ScopedEnvironment(dataObject.Environment, ds, Alias);

                //Push the new environment
                dataObject.PushEnvironment(executionEnvironment);
                dataObject.ForEachNestingLevel++;
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                }
                dataObject.ParentInstanceID = UniqueID;
                dataObject.IsDebugNested = true;
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.After, update);
                }
                foreach(var exp in expressions)
                {
                    //Assign the warewolfAtom to Alias using new environment
                    executionEnvironment.SetDataSource(exp);

                    var exeAct = ApplyActivityFunc.Handler as IDev2Activity;
                    _childUniqueID = exeAct.UniqueID;
                    exeAct?.Execute(dataObject, 0);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error("DSFSelectAndApply", e);
                allErrors.AddError(e.Message);
            }
            finally
            {
                dataObject.PopEnvironment();

                dataObject.ForEachNestingLevel--;
                if(allErrors.HasErrors())
                {
                    if(allErrors.HasErrors())
                    {
                        DisplayAndWriteError("DsfSelectAndApplyActivity", allErrors);
                        foreach(var fetchError in allErrors.FetchErrors())
                        {
                            dataObject.Environment.AddError(fetchError);
                        }
                    }
                }
                if(dataObject.IsDebugMode())
                {
                    foreach(var expression in expressions)
                    {
                        var data = dataObject.Environment.Eval(expression, update);
                        if(data.IsWarewolfAtomListresult)
                        {
                            var lst = data as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                            AddDebugOutputItem(new DebugItemWarewolfAtomListResult(lst, "", "", expression, "", "", "="));
                        }
                        else
                        {
                            if(data.IsWarewolfAtomResult)
                            {
                                var atom = data as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                                if(atom != null)
                                    AddDebugOutputItem(new DebugItemWarewolfAtomResult(atom.Item.ToString(), expression, ""));
                            }
                        }
                    }
                    var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == WorkSurfaceMappingId);
                    var serviceTestSteps = serviceTestStep?.Children;
                    UpdateDebugStateWithAssertions(dataObject, serviceTestSteps?.ToList());
                    DispatchDebugState(dataObject, StateType.End, update, startTime, DateTime.Now);
                }
                dataObject.ParentInstanceID = _previousParentId;
                dataObject.IsDebugNested = false;
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.ForEach;
        }
        #endregion Overrides of DsfNativeActivity<bool>

        private void UpdateDebugStateWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps)
        {
            if (dataObject.IsServiceTestExecution && serviceTestTestSteps != null)
            {
                var stepToBeAsserted = serviceTestTestSteps.FirstOrDefault(step => step.Type == StepType.Assert && step.UniqueId == Guid.Parse(_childUniqueID) && step.ActivityType != typeof(DsfForEachActivity).Name && step.ActivityType != typeof(DsfSelectAndApplyActivity).Name && step.ActivityType != typeof(DsfSequenceActivity).Name);
                if (stepToBeAsserted?.StepOutputs != null && stepToBeAsserted.StepOutputs.Count > 0)
                {
                    if (stepToBeAsserted.Result != null)
                    {
                        stepToBeAsserted.Result.RunTestResult = RunResult.TestInvalid;
                    }
                    else
                    {
                        var debugItems = TestDebugMessageRepo.Instance.GetDebugItems(dataObject.ResourceID, dataObject.TestName);
                        debugItems = debugItems.Where(state => state.ID == stepToBeAsserted.UniqueId).ToList();
                        var debugStates = debugItems?.LastOrDefault();
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
                        dataObject.ServiceTest.Result = finalResult;
                        dataObject.ServiceTest.TestFailing = !testPassed;
                        dataObject.ServiceTest.FailureMessage = serviceTestFailureMessage;
                        dataObject.ServiceTest.TestPassed = testPassed;
                        dataObject.StopExecution = !testPassed;
                    }
                }
            }
        }

        private IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory, IDebugState debugState)
        {
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
                    debugState.AssertResultList.Add(itemToAdd);
                }
                output.Result = testResult;
                ret.Add(testResult);
            }
            return ret;
        }
    }
}