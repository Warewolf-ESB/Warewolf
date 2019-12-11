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
using Dev2.Activities.Gates;
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
using Dev2.Interfaces;
using Newtonsoft.Json;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Data.Options;
using Warewolf.Data.Options.Enums;
using Warewolf.Options;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-Gate", nameof(Gate), ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Gate")]
    public class GateActivity : DsfFlowNodeActivity<bool>, IEquatable<GateActivity>, IStateNotifierRequired
    {
        private IStateNotifier _stateNotifier = null;
        public void SetStateNotifier(IStateNotifier stateNotifier)
        {
            if (_stateNotifier is null)
            {
                _stateNotifier = stateNotifier;
            }
        }

        public GateActivity()
            : base(nameof(Gate))
        {
            DisplayName = nameof(Gate);
            IsGate = true;
        }

        public GateFailureAction GateFailure { get; set; }


        public string GateRetryStrategy { get; set; }

        public IList<ConditionExpression> Conditions { get; set; }

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
                    if (_dataObject.IsDebugMode())
                    {
                        var debugItemStaticDataParams = new DebugItemStaticDataParams(a.Left, "", true);
                        AddDebugOutputItem(debugItemStaticDataParams);
                    }

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

        static Dev2Decision ParseDecision(IExecutionEnvironment env, Dev2Decision decision, bool errorIfNull)
        {
            var col1 = env.EvalAsList(decision.Col1, 0, errorIfNull);
            var col2 = env.EvalAsList(decision.Col2 ?? "", 0, errorIfNull);
            var col3 = env.EvalAsList(decision.Col3 ?? "", 0, errorIfNull);
            return new Dev2Decision { Cols1 = col1, Cols2 = col2, Cols3 = col3, EvaluationFn = decision.EvaluationFn };
        }

        /// <summary>
        /// Where should we send execution if this gate fails and not set to StopOnFailure
        /// </summary>
        private IDev2Activity GetRetryEntryPoint => _dataObject.Gates.First(o => o.UniqueID == RetryEntryPointId.ToString());

        public Guid RetryEntryPointId { get; set; }

        public GateOptions GateOptions { get; set; }

        public override List<string> GetOutputs() => new List<string>();
        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update) => _debugOutputs;
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
                    Name = nameof(GateRetryStrategy),
                    Value = GateRetryStrategy,
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
                    Type = StateVariable.StateType.Input,
                    Name = nameof(_retryState.NumberOfRetries),
                    Value = _retryState.NumberOfRetries.ToString(),
                },
                 new StateVariable
                {
                    Name = "next",
                    Type = StateVariable.StateType.Output,
                    Value = serializer.Serialize(Next)
                },
                  new StateVariable
                {
                    Name = "stop",
                    Type = StateVariable.StateType.Output,
                    Value = serializer.Serialize(Stop)
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
        public void SetDebugInputs(List<DebugItem> debugInputs)
        {
            _debugInputs = debugInputs;
        }

        public void SetDebugOutputs(List<DebugItem> debugOutputs)
        {
            _debugOutputs = debugOutputs;
        }
        IDSFDataObject _dataObject;
        IEnumerator<bool> _algo;
        public override IDev2Activity Execute(IDSFDataObject data, int update)
        {
            if (GateOptions is null)
            {
                GateOptions = new GateOptions();
            }

            try
            {
                _stateNotifier?.LogPreExecuteState(this);

                _dataObject = data;
                if (!_dataObject.Gates.Contains(this))
                {
                    _dataObject.Gates.Add(this);
                    if (GateOptions.GateOpts is AllowResumption gateOptionsResume)
                    {
                        _algo = gateOptionsResume.Strategy.Create().GetEnumerator();
                    }
                }
                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams("Rerty: " + _retryState.NumberOfRetries.ToString(), "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);

                }
                if (_retryState.NumberOfRetries <= 0)
                {
                    if (_dataObject.IsDebugMode())
                    {
                        var debugItemStaticDataParams = new DebugItemStaticDataParams(nameof(ExecuteNormal), "", true);
                        AddDebugOutputItem(debugItemStaticDataParams);
                        DispatchDebugState(_dataObject, StateType.Before, update);
                        DispatchDebugState(_dataObject, StateType.After, update);
                    }
                    return ExecuteNormal(data, update);
                }

                // execute workflow that should be called on resume

                // reset Environment to state it was in the first time we executed this Gate

                // load selected retry algorithm
                if (_dataObject.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(nameof(ExecuteRetry), "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                    DispatchDebugState(_dataObject, StateType.Before, update);
                    DispatchDebugState(_dataObject, StateType.After, update);

                }
                return ExecuteRetry(data, update);
            }
            catch (Exception e)
            {
                _stateNotifier?.LogExecuteException(e, this);
                throw;
            }
        }
        /// <summary>
        /// This Gate is being executed again due to some other Gate selecting this Gate to be
        /// executed again.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private IDev2Activity ExecuteRetry(IDSFDataObject data, int update)
        {
            // todo: if _algo is null then this gate did not get set to a resumable gate?

            // if allowed to retry and its time for a retry return NextNode
            // otherwise schedule this environment and current activity to 
            // be executed at the calculated latter time
            if (_algo.MoveNext() && _algo.Current)
            {
                return NextNodes.First();
            }

            // Gate has reached maximum retries.
            return null;
        }
        public IDev2Activity Next { get; set; }
        public bool Stop { get; set; } = false;
        /// <summary>
        /// This Gate is being executed for the first time
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public IDev2Activity ExecuteNormal(IDSFDataObject data, int update)
        {
            try
            {
                _debugInputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();
                Stop = false;
                Next = null;

                UpdateConditions();
                if (data.IsDebugMode())
                {
                    _debugInputs = CreateDebugInputs(data.Environment);
                    DispatchDebugState(data, StateType.Before, 0);
                }

                //----------ExecuteTool--------------
                if (!Passing(update))
                {
                    var canRetry = RetryEntryPointId != Guid.Empty;

                    var gateFailure = GateFailure;

                    switch (gateFailure)
                    {
                        case GateFailureAction.StopProcessing:
                            data.Environment.AddError("stop on error with no resume");
                            Dev2Logger.Warn("execution stopped!", _dataObject?.ExecutionID?.ToString());
                            Stop = true;
                            break;
                        case GateFailureAction.Retry:
                            if (canRetry)
                            {
                                var goBackToActivity = GetRetryEntryPoint.As<GateActivity>();
                                goBackToActivity.UpdateRetryState(this);
                                Next = goBackToActivity;
                            }
                            else
                            {
                                const string msg = "invalid retry config: no gate selected";
                                data.Environment.AddError(msg);
                                Dev2Logger.Warn($"execution stopped! {msg}", _dataObject?.ExecutionID?.ToString());
                                Stop = true;
                            }
                            break;
                        default:
                            throw new Exception("unknown gate failure option");
                    }
                }

                if (!data.IsDebugMode())
                {
                    UpdateWithAssertions(data);
                }
                else
                {
                    Result = nameof(Execute);
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(Result, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
            }
            catch (Exception ex)
            {
                Result = ex.Message;
                data.Environment.AddError(ex.Message);
                Dev2Logger.Error(nameof(OnExecute), ex, GlobalConstants.WarewolfError);
                Stop = true;
            }
            finally
            {
                if (!_isExecuteAsync || _isOnDemandSimulation)
                {
                    DoErrorHandling(data, update);
                }
            }
            if (Next != null)
            {
                Result = nameof(Next);
                if (data.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(Result, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                return Next; // retry has set a node that we should go back to retry
            }
            if (Stop)
            {
                Result = nameof(Stop);
                if (data.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(Result, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                return null;
            }
            if (NextNodes != null && NextNodes.Any())
            {
                Result = nameof(NextNodes);
                if (data.IsDebugMode())
                {
                    var debugItemStaticDataParams = new DebugItemStaticDataParams(Result, "", true);
                    AddDebugOutputItem(debugItemStaticDataParams);
                }
                return NextNodes.First();
            }
            return null;
        }
        public new string Result { get; set; }

        private void UpdateConditions()
        {
            var rawText = ExpressionText;

            if (rawText != null)
            {
                //  TODO: Not sure what is coming through so not what to do here
                var activityTextjson = rawText.Substring(rawText.IndexOf("{", StringComparison.Ordinal)).Replace(@""",AmbientDataList)", "").Replace("\"", "!");

                var activityText = Dev2DecisionStack.FromVBPersitableModelToJSON(activityTextjson);
                var conditionStack = JsonConvert.DeserializeObject<List<ConditionExpression>>(activityText);

                Conditions = conditionStack;
            }
        }

        List<DebugItem> CreateDebugInputs(IExecutionEnvironment env)
        {
            var result = new List<IDebugItem>();

            var allErrors = new ErrorResultTO();

            try
            {
                var dds = Conditions;
                //ErrorResultTO error = null;  //TODO: Add Correctly
                var userModel = "";          //TODO: Add Correctly
                //allErrors.MergeErrors(error);

                foreach (ConditionExpression conditionExpression in dds)
                {
                    //TODO: 
                    //AddInputDebugItemResultsAfterEvaluate(result, ref userModel, env, conditionExpression.Cond., out error);
                    //allErrors.MergeErrors(error);
                    //AddInputDebugItemResultsAfterEvaluate(result, ref userModel, env, conditionExpression.Col2, out error);
                    //allErrors.MergeErrors(error);
                    //AddInputDebugItemResultsAfterEvaluate(result, ref userModel, env, conditionExpression.Col3, out error);
                    //allErrors.MergeErrors(error);
                }

                var itemToAdd = new DebugItem();

                userModel = userModel.Replace("OR", " OR\r\n")
                                     .Replace("AND", " AND\r\n")
                                     .Replace("\r\n ", "\r\n")
                                     .Replace("\r\n\r\n", "\r\n")
                                     .Replace("  ", " ");

                AddDebugItem(new DebugItemStaticDataParams(userModel, "Statement"), itemToAdd);
                result.Add(itemToAdd);

                itemToAdd = new DebugItem();
                //TODO: 
                //AddDebugItem(new DebugItemStaticDataParams(dds. == Dev2DecisionMode.AND ? "YES" : "NO", "Require all decisions to be true"), itemToAdd);
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

            var val = result.Select(a => a as DebugItem).ToList();
            return val;
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


        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            throw new Exception("this should not be reached");
        }


        class RetryState
        {
            public int NumberOfRetries { get; set; }
        }
        readonly RetryState _retryState = new RetryState();

        private void UpdateRetryState(GateActivity gateActivity)
        {
            _retryState.NumberOfRetries++;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public static void ExecuteToolAddDebugItems(IDSFDataObject dataObject, int update)
        {
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
            eq &= string.Equals(GateRetryStrategy, other.GateRetryStrategy);
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

                return hashCode;
            }
        }
    }
}
