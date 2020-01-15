﻿/*
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
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;
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
            IsGate = true;
            if (GateOptions is null)
            {
                GateOptions = new GateOptions();
            }
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
                var res = Conditions.Select(a =>
                {
                    return a.Eval(GetArgumentsFunc, _dataObject.Environment.HasErrors());
                });
                return res.All(o => o);
            }
            catch (Exception e)
            {
                Dev2Logger.Warn("failed checking passing state of gate", e, _dataObject?.ExecutionID?.ToString());
                return false;
            }
        }

        IDSFDataObject _dataObject;
        IExecutionEnvironment _originalExecutionEnvironment;
        public override IDev2Activity Execute(IDSFDataObject data, int update)
        {
            _debugInputs?.Clear();
            _debugOutputs?.Clear();
            _dataObject = data;
            var allErrors = new ErrorResultTO();
            try
            {
                _stateNotifier?.LogPreExecuteState(this);

                //var firstExecution = !_dataObject.Gates.Contains(this);
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
                    if (GateOptions.GateOpts is AllowResumption gateOptionsResume)
                    {
                        _retryState.Item2 = gateOptionsResume.Strategy.Create().GetEnumerator();
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
                    return ExecuteNormal(data, update, allErrors);
                }

                // TODO: execute workflow that should be called on resume
                if (GateOptions.GateOpts is AllowResumption allowResumption)
                {
                    ExecuteRetryWorkflow(allowResumption);
                }

                _dataObject.Environment = _originalExecutionEnvironment;
                Dev2Logger.Debug("Gate: Resetting Environment Snapshot", data.ExecutionID.ToString());

                return ExecuteRetry(data, update, allErrors, _retryState.Item2);
            }
            catch (Exception e)
            {
                _stateNotifier?.LogExecuteException(e, this);
                throw;
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError(nameof(GateActivity), allErrors);
                    foreach (var errorString in allErrors.FetchErrors())
                    {
                        data.Environment.AddError(errorString);
                    }
                }
                if (data.IsDebugMode())
                {
                    DispatchDebugState(data, StateType.Before, update);
                    DispatchDebugState(data, StateType.After, update);
                }
            }
        }

        private void ExecuteRetryWorkflow(AllowResumption allowResumption)
        {
            var resumeEndpoint = allowResumption.SelectedActivity;
            //var activity = allowResumption.SelectedActivity as DsfNativeActivity<string>;
            var callbackDataObject = new DsfDataObject("", Guid.Empty)
            {
                Environment = new ExecutionEnvironment(),
                ParentServiceName = _dataObject.ServiceName
            };
            //activity.Execute(_dataObject, 0);
            //Dev2Logger.Debug("Gate: Execute callback workflow - " + resumeEndpoint.DisplayName, callbackDataObject.ExecutionID.ToString());
        }

        /// <summary>
        /// This Gate is being executed again due to some other Gate selecting this Gate to be
        /// executed again.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private IDev2Activity ExecuteRetry(IDSFDataObject data, int update, ErrorResultTO allErrors, IEnumerator<bool> _algo)
        {
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
        public IDev2Activity ExecuteNormal(IDSFDataObject data, int update, ErrorResultTO allErrors)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();

            var stop = false;
            IDev2Activity next = null;
            try
            {
                if (data.IsDebugMode())
                {
                    _debugInputs = CreateDebugInputs(data.Environment);
                    //DispatchDebugState(data, StateType.Before, 0);
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

                    var gateFailure = GateFailure;

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
            if (GateOptions.GateOpts is AllowResumption allowResumption)
            {
                _retryState.NumberOfRetries++;
            } else
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
            eq &= string.Equals(GateFailure, other.GateFailure);
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
                    Name = nameof(GateFailure),
                    Value = GateFailure.ToString(),
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
                },/*
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(_retryState.NumberOfRetries),
                    Value = _retryState.NumberOfRetries.ToString(),
                },*/
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

        List<DebugItem> CreateDebugInputs(IExecutionEnvironment env)
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

        static void AddInputDebugItemResultsAfterEvaluate(List<IDebugItem> result, ref string userModel, IExecutionEnvironment env, string expression, out ErrorResultTO error, DebugItem parent = null)
        {
            error = new ErrorResultTO();
            if (expression != null && DataListUtil.IsEvaluated(expression))
            {
                DebugOutputBase debugResult;
                if (error.HasErrors())
                {
                    debugResult = new DebugItemStaticDataParams("", expression, "");
                }
                else
                {
                    string expressiomToStringValue;
                    try
                    {
                        expressiomToStringValue = ExecutionEnvironment.WarewolfEvalResultToString(env.Eval(expression, 0));
                    }
                    catch (NullValueInVariableException)
                    {
                        expressiomToStringValue = "";
                    }
                    userModel = userModel.Replace(expression, expressiomToStringValue);
                    debugResult = new DebugItemWarewolfAtomResult(expressiomToStringValue, expression, "");
                }

                var itemResults = debugResult.GetDebugItemResult();

                var allReadyAdded = new List<IDebugItemResult>();

                itemResults.ForEach(a =>
                {
                    var found = result.SelectMany(r => r.FetchResultsList()).SingleOrDefault(r => r.Variable.Equals(a.Variable));
                    if (found != null)
                    {
                        allReadyAdded.Add(a);
                    }
                });

                allReadyAdded.ForEach(i => itemResults.Remove(i));

                if (parent == null)
                {
                    result.Add(new DebugItem(itemResults));
                }
                else
                {
                    parent.AddRange(itemResults);
                }
            }
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


        public GateFailureAction GateFailure { get; set; }
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
