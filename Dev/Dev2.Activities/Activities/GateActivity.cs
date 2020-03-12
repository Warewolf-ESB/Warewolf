/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Data.TO;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;
using Warewolf.Resource.Messages;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    enum Gate { }
    [ToolDescriptorInfo("ControlFlow-Gate", nameof(Gate), ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Gate")]
    public class GateActivity : DsfActivityAbstract<string>, IEquatable<GateActivity>, IStateNotifierRequired
    {
        private IStateNotifier _stateNotifier = null;

        public GateActivity()
            : base(nameof(Gate))
        {
            DisplayName = nameof(Gate);
            if (GateOptions is null)
            {
                GateOptions = new GateOptions();
            }
            DataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
            };
        }

        /// <summary>
        /// Returns true if all conditions are passing
        /// Returns true if there are no conditions
        /// Returns false if any condition is failing
        /// Returns false if any variable does not exist
        /// Returns false if there is an exception of any kind
        /// </summary>
        private bool Passing(int update)
        {
            if (!Conditions.Any())
            {
                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(nameof(Passing), "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                return true;
            }

            try
            {
                var res = Conditions.Select(a => a.Eval(GetArgumentsFunc, _dataObject.Environment.HasErrors()));
                return res.All(o => o);
            }
            catch (Exception e)
            {
                Dev2Logger.Warn("failed checking passing state of gate", e, _dataObject?.ExecutionID?.ToString());
                return false;
            }
        }

        private Guid _originalUniqueId;
        private string _previousParentId;
        private IDSFDataObject _dataObject;
        private IExecutionEnvironment _originalExecutionEnvironment;
        public override IDev2Activity Execute(IDSFDataObject data, int update)
        {
            _previousParentId = data.ParentInstanceID;
            _debugInputs?.Clear();
            _debugOutputs?.Clear();
            _dataObject = data;
            var allErrors = new ErrorResultTO();
            try
            {
                _stateNotifier?.LogPreExecuteState(this);

                bool firstExecution = true;
                if (_dataObject.Gates.TryGetValue(this, out (RetryState, IEnumerator<bool>) _retryState))
                {
                    firstExecution = false;
                } else
                {
                    _retryState = (new RetryState(), null);
                }
                var isRetry = _retryState.Item1.NumberOfRetries > 0;

                if (firstExecution)
                {
                    if (GateOptions.GateOpts is Continue onResume)
                    {
                        _retryState.Item2 = onResume.Strategy.Create().GetEnumerator();
                    }
                    _dataObject.Gates.Add(this, _retryState);
                    _originalExecutionEnvironment = data.Environment.Snapshot();
                }
                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Retry: " + _retryState.Item1.NumberOfRetries.ToString(), "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);

                }
                if (firstExecution || !isRetry)
                {
                    if (_retryState.Item1.NumberOfRetries > 0)
                    {
                        throw new GateException("gate execution corrupt: first execution with invalid number of retries");
                    }
                    if (_dataObject.IsDebugMode())
                    {
                        var debugItemStaticDataParams = new DebugItemStaticDataParams(nameof(ExecuteNormal), "", true);
                        AddDebugOutputItem(debugItemStaticDataParams);
                    }

                    if (_dataObject.IsServiceTestExecution && _originalUniqueId == Guid.Empty)
                    {
                        _originalUniqueId = Guid.Parse(UniqueID);
                    }

                    return ExecuteNormal(data, update, allErrors);
                }

                return ExecuteRetry(data, update, allErrors, _retryState.Item2);
            }
            catch (Exception e)
            {
                _stateNotifier?.LogExecuteException(e, this);
                throw;
            }
            finally
            {
                HandleErrors(data, allErrors);
                if (data.IsDebugMode())
                {
                    DispatchDebugState(data, StateType.Before, update);
                    DispatchDebugState(data, StateType.After, update);
                }
            }
        }

        private static void GetFinalTestRunResult(IServiceTestStep serviceTestStep, TestRunResult testRunResult)
        {
            var resultList = new ObservableCollection<TestRunResult>();
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

                var testRunResults = resultList.Where(runResult => runResult.RunTestResult == RunResult.TestInvalid).ToList();
                if (testRunResults.Count > 0)
                {
                    testRunResult.Message = string.Join(Environment.NewLine, testRunResults.Select(result => result.Message));
                    testRunResult.RunTestResult = RunResult.TestInvalid;
                }
                else
                {
                    var passed = resultList.All(runResult => runResult.RunTestResult == RunResult.TestPassed);
                    if (passed)
                    {
                        testRunResult.Message = Messages.Test_PassedResult;
                        testRunResult.RunTestResult = RunResult.TestPassed;
                    }
                    else
                    {
                        testRunResult.Message = Messages.Test_FailureResult;
                        testRunResult.RunTestResult = RunResult.TestFailed;
                    }
                }
            }
        }

        private static void HandleErrors(IDSFDataObject data, ErrorResultTO allErrors)
        {
            var hasErrors = allErrors.HasErrors();
            if (!hasErrors)
            {
                return;
            }
            DisplayAndWriteError(nameof(GateActivity), allErrors);
            foreach (var errorString in allErrors.FetchErrors())
            {
                data.Environment.AddError(errorString);
            }
        }

        private void BeforeExecuteRetryWorkflow()
        {
            _dataObject.ForEachNestingLevel++;
            _dataObject.ParentInstanceID = UniqueID;
            _dataObject.IsDebugNested = true;
        }

        private void ExecuteRetryWorkflow()
        {
            if (DataFunc.Handler is IDev2Activity act)
            {
                act.Execute(_dataObject, 0);
                Dev2Logger.Debug("Gate: Execute callback workflow - " + act.GetDisplayName(), _dataObject.ExecutionID.ToString());
            }
        }

        private void ExecuteRetryWorkflowCompleted()
        {
            _dataObject.IsDebugNested = false;
            _dataObject.ParentInstanceID = _previousParentId;
            _dataObject.ForEachNestingLevel--;
        }

        /// <summary>
        /// This Gate is being executed again due to some other Gate selecting this Gate to be
        /// executed again.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private IDev2Activity ExecuteRetry(IDSFDataObject data, int update, IErrorResultTO allErrors, IEnumerator<bool> _algo)
        {
            if (GateOptions.GateOpts is Continue)
            {
                BeforeExecuteRetryWorkflow();
                ExecuteRetryWorkflow();
                ExecuteRetryWorkflowCompleted();
            }

            _dataObject.Environment = _originalExecutionEnvironment;
            Dev2Logger.Debug("Gate: Reset Environment Snapshot", data.ExecutionID.ToString());
            
            if (_dataObject.IsDebugMode())
            {
                var debugItemStaticDataParams = new DebugItemStaticDataParams(nameof(ExecuteRetry), "", true);
                AddDebugOutputItem(debugItemStaticDataParams);
            }

            // if allowed to retry and its time for a retry return NextNode
            // otherwise schedule this environment and current activity to 
            // be executed at the calculated latter time
            var retryAllowed = _algo.MoveNext() && _algo.Current;
            if (retryAllowed)
            {
                return NextNodes.First();
            } else
            {
                allErrors.AddError("retry count exceeded");
            }

            // Gate has reached maximum retries.
            return null;
        }

        /// <summary>
        /// This Gate is being executed for the first time
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private IDev2Activity ExecuteNormal(IDSFDataObject data, int update, IErrorResultTO allErrors)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            var stop = false;
            IDev2Activity next = null;
            try
            {
                if (data.IsDebugMode())
                {
                    _debugInputs = CreateDebugInputs();
                }

                //----------ExecuteTool--------------
                var passing = Passing(update);
                if (passing)
                {
                    if (_dataObject.IsDebugMode())
                    {
                        var debugItemStaticDataParams = new DebugItemStaticDataParams("Conditions passed", "", true);
                        AddDebugOutputItem(debugItemStaticDataParams);
                    }
                }
                else 
                {
                    if (_dataObject.IsDebugMode())
                    {
                        var debugItemStaticDataParams = new DebugItemStaticDataParams("Conditions failed", "", true);
                        AddDebugOutputItem(debugItemStaticDataParams);
                    }

                    var msg = "gate conditions failed";
                    allErrors.AddError(msg);
                    var canRetry = RetryEntryPointId != Guid.Empty;

                    var gateFailure = canRetry ? GateFailureAction.Retry : GateFailureAction.StopProcessing;

                    switch (gateFailure)
                    {
                        case GateFailureAction.StopProcessing:
                            msg = "execution stopped";
                            allErrors.AddError(msg);
                            Dev2Logger.Warn("execution stopped!", _dataObject?.ExecutionID?.ToString());
                            stop = true;
                            break;
                        case GateFailureAction.Retry:
                            if (canRetry)
                            {
                                var goBackToActivity = GetRetryEntryPoint().As<GateActivity>();
                                goBackToActivity.UpdateRetryState(this, _dataObject.Gates[goBackToActivity].Item1);
                                next = goBackToActivity;
                            }
                            else
                            {
                                msg = "invalid retry config: no gate selected";
                                allErrors.AddError(msg);
                                Dev2Logger.Warn($"execution stopped! {msg}", _dataObject?.ExecutionID?.ToString());
                                stop = true;
                            }
                            break;
                        default:
                            msg = "unknown gate failure option";
                            allErrors.AddError(msg);
                            throw new Exception(msg);
                    }
                }

                if (!data.IsDebugMode())
                {
                    UpdateWithAssertions(data);
                }
                var serviceTestStep = _dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.ActivityID == _originalUniqueId);

                if (_dataObject.IsServiceTestExecution && serviceTestStep != null)
                {
                    var testRunResult = new TestRunResult();
                    GetFinalTestRunResult(serviceTestStep, testRunResult);
                    serviceTestStep.Result = testRunResult;
                }
            }
            catch (Exception ex)
            {
                allErrors.AddError(ex.Message);
                Dev2Logger.Error(nameof(OnExecute), ex, GlobalConstants.WarewolfError);
                stop = true;
            }
            finally
            {
                if (!_isExecuteAsync || _isOnDemandSimulation)
                {
                    DoErrorHandling(data, update);
                }
            }
            if (next != null)
            {
                if (data.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Retry gate", "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                return next; // retry has set a node that we should go back to retry
            }
            if (stop)
            {
                if (data.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Stop execution", "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                return null;
            }
            if (NextNodes != null && NextNodes.Any())
            {
                if (data.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Executing", "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                return NextNodes.First();
            }
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            throw new Exception("this should not be reached");
        }

        private void UpdateRetryState(GateActivity gateActivity, RetryState _retryState)
        {
            if (GateOptions.GateOpts is Continue)
            {
                _retryState.NumberOfRetries++;
            }
            else
            {
                throw new GateException("cannot update retry state of a non-resumable gate");
            }
        }

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public void SetStateNotifier(IStateNotifier stateNotifier)
        {
            if (_stateNotifier is null)
            {
                _stateNotifier = stateNotifier;
            }
        }

        public bool Equals(GateActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var eq = base.Equals(other);
            eq &= GateOptions.Equals(other.GateOptions);
            return eq;
        }

        public override bool Equals(object obj)
        {
            if (obj is GateActivity act)
            {
                return Equals(act);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397);
                hashCode = ((hashCode * 397) ^ (UniqueID != null ? UniqueID.GetHashCode() : 0));

                return hashCode;
            }
        }

        public override List<string> GetOutputs() => new List<string>();
        public override IEnumerable<StateVariable> GetState()
        {
            var serializer = new Dev2JsonSerializer();
            return new[]
           {
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(Conditions),
                    Value = serializer.Serialize(Conditions),
                },
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(Passing),
                    Value = Passing(0) ? "true" : "false",
                },
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(RetryEntryPointId),
                    Value = RetryEntryPointId.ToString()
                },
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(GateOptions),
                    Value = GateOptions?.ToString()
                },
                 new StateVariable
                {
                    Name = nameof(NextNodes),
                    Type = StateVariable.StateType.Output,
                    Value = serializer.Serialize(NextNodes)
                },
           };
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }
        private IEnumerable<string[]> GetArgumentsFunc(string col1s, string col2s, string col3s)
        {
            var col1 = _dataObject.Environment.EvalAsList(col1s, 0, false);
            var col2 = _dataObject.Environment.EvalAsList(col2s ?? "", 0, false);
            var col3 = _dataObject.Environment.EvalAsList(col3s ?? "", 0, false);

            var iter = new WarewolfListIterator();
            var c1 = new WarewolfAtomIterator(col1);
            var c2 = new WarewolfAtomIterator(col2);
            var c3 = new WarewolfAtomIterator(col3);
            iter.AddVariableToIterateOn(c1);
            iter.AddVariableToIterateOn(c2);
            iter.AddVariableToIterateOn(c3);

            while (iter.HasMoreData())
            {
                var item = new string[] { iter.FetchNextValue(c1), iter.FetchNextValue(c2), iter.FetchNextValue(c3) };
                yield return item;
            }
            yield break;
        }
        /// <summary>
        /// Where should we send execution if this gate fails and not set to StopOnFailure
        /// </summary>
        private IDev2Activity GetRetryEntryPoint() => _dataObject.Gates.Keys.First(o => o.UniqueID == RetryEntryPointId.ToString());


        #region debugstuff
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => _debugInputs;

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update) => _debugOutputs;

        private List<DebugItem> CreateDebugInputs()
        {
            var result = new List<DebugItem>();

            var allErrors = new ErrorResultTO();

            try
            {
                var dds = Conditions.GetEnumerator();
                var text = new StringBuilder();
                if (dds.MoveNext() && dds.Current.Cond.MatchType != enDecisionType.Choose)
                {
                    dds.Current.RenderDescription(text);
                }
                while (dds.MoveNext())
                {
                    var conditionExpression = dds.Current;
                    if (conditionExpression.Cond.MatchType == enDecisionType.Choose)
                    {
                        continue;
                    }

                    text.Append("\n AND \n");
                    conditionExpression.RenderDescription(text);
                }

                var itemToAdd = new DebugItem();
                var s = text.ToString();
                if (string.IsNullOrWhiteSpace(s))
                {
                    AddDebugItem(new DebugItemStaticDataParams(s, "Always Allow"), itemToAdd);
                }
                else
                {
                    AddDebugItem(new DebugItemStaticDataParams(s, "Allow If"), itemToAdd);
                }
                result.Add(itemToAdd);
            }
            catch (JsonSerializationException e)
            {
                Dev2Logger.Warn(e.Message, "Warewolf Warn");
            }
            catch (Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                if (allErrors.HasErrors())
                {
                    var serviceName = GetType().Name;
                    DisplayAndWriteError(serviceName, allErrors);
                }
            }

            return result;
        }

        #endregion debugstuff

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return new List<DsfForEachItem>();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return new List<DsfForEachItem>();
        }

        public ActivityFunc<string, bool> DataFunc { get; set; }
        public IList<ConditionExpression> Conditions { get; set; }

        public Guid RetryEntryPointId { get; set; }

        public GateOptions GateOptions { get; set; }
    }

    public class GateException : Exception
    {
        public GateException(string message) : base(message)
        {
        }
    }
}
