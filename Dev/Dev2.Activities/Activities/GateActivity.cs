/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Gates;
using Dev2.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
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
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities
{
    [ToolDescriptorInfo("ControlFlow-Gate", nameof(Gate), ToolType.Native, "8999E58B-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Gate")]
    public class GateActivity : DsfActivityAbstract<string>, IEquatable<GateActivity>
    {
        public GateActivity()
        {
            DisplayName = nameof(Gate);
            IsGate = true;
        }

        public string GateFailure { get; set; }

        public string GateRetryStrategy { get; set; }

        public Dev2DecisionStack Conditions { get; set; }
        /// <summary>
        /// Returns true if all conditions are passing
        /// Returns true if there are no conditions
        /// Returns false if any condition is failing
        /// Returns false if any variable does not exist
        /// Returns false if there is an exception of any kind
        /// </summary>
        private bool Passing
        {
            get
            {
                const bool errorIfNull = true;
                try
                {
                    if (!Conditions.TheStack.Any())
                    {
                        return true;
                    }
                    var stack = Conditions.TheStack.Select(a => ParseDecision(_dataObject.Environment, a, errorIfNull));

                    var factory = Dev2DecisionFactory.Instance();

                    var res = stack.SelectMany(a =>
                    {
                        if (a.EvaluationFn == enDecisionType.IsError)
                        {
                            return new[] { _dataObject.Environment.AllErrors.Count > 0 };
                        }
                        if (a.EvaluationFn == enDecisionType.IsNotError)
                        {
                            return new[] { _dataObject.Environment.AllErrors.Count == 0 };
                        }
                        IList<bool> ret = new List<bool>();
                        var iter = new WarewolfListIterator();
                        var c1 = new WarewolfAtomIterator(a.Cols1);
                        var c2 = new WarewolfAtomIterator(a.Cols2);
                        var c3 = new WarewolfAtomIterator(a.Cols3);
                        iter.AddVariableToIterateOn(c1);
                        iter.AddVariableToIterateOn(c2);
                        iter.AddVariableToIterateOn(c3);
                        while (iter.HasMoreData())
                        {
                            try
                            {
                                ret.Add(factory.FetchDecisionFunction(a.EvaluationFn).Invoke(new[] { iter.FetchNextValue(c1), iter.FetchNextValue(c2), iter.FetchNextValue(c3) }));
                            }
                            catch (Exception)
                            {
                                ret.Add(false);
                            }
                        }
                        return ret;
                    });
                    return res.All(o => o);
                }
                catch (Exception e)
                {
                    Dev2Logger.Warn("failed checking passing state of gate", e, _dataObject?.ExecutionID?.ToString());
                    return false;
                }
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
        public IDev2Activity RetryEntryPoint { get => _dataObject.Gates.First(o => o.UniqueID == RetryEntryPointId.ToString()); }

        public Guid RetryEntryPointId { get; set; }

        public GateOptions GateOptions { get; set; }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetOutputs()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<StateVariable> GetState()
        {
           return new[]
           {
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(Conditions),
                    Value = JsonConvert.SerializeObject(Conditions),
                },
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(GateFailure),
                    Value = GateFailure,
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
                    Value = Passing ? "true" : "false",
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
                    Value = GateOptions.ToString()
                },
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(_retryState.NumberOfRetries),
                    Value = _retryState.NumberOfRetries.ToString(),
                }
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

        IDSFDataObject _dataObject;
        public override IDev2Activity Execute(IDSFDataObject data, int update)
        {
            _dataObject = data;
            _dataObject.Gates.Add(this);

            if (_retryState.NumberOfRetries <= 0)
            {
                return ExecuteNormal(data, update);
            }

            // execute workflow that should be called on resume

            // reset Environment to state it was in the first time we executed this Gate

            // load selected retry algorithm

            return ExecuteRetry(data, update);
        }
        /// <summary>
        /// This Gate is being executed again due to some other Gate selecting this Gate to be
        /// executed again.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private static IDev2Activity ExecuteRetry(IDSFDataObject data, int update)
        {
            // if allowed to retry and its time for a retry return NextNode
            // otherwise schedule this environment and current activity to 
            // be executed at the calculated latter time


            // Gate has reached maximum retries.
            return null;
        }

        /// <summary>
        /// This Gate is being executed for the first time
        /// </summary>
        /// <param name="data"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public IDev2Activity ExecuteNormal(IDSFDataObject data, int update)
        {
            IDev2Activity next = null;
            bool stop = false;
            try
            {
                _debugInputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();

                //----------ExecuteTool--------------
                if (!Passing)
                {
                    var gateFailure = GateFailure ?? nameof(GateFailureAction.StopOnError);
                    switch (Enum.Parse(typeof(GateFailureAction), gateFailure))
                    {
                        case GateFailureAction.StopOnError:
                            stop = true;
                            Dev2Logger.Warn("execution stopped!", _dataObject?.ExecutionID?.ToString());
                            break;
                        case GateFailureAction.Retry:
                            var goBackToActivity = RetryEntryPoint.As<GateActivity>();

                            goBackToActivity.UpdateRetryState(this);
                            next = goBackToActivity;
                            break;
                        default:
                            throw new Exception("unknown gate failure option");
                    }
                }
                //------------------------

                if (!data.IsDebugMode())
                {
                    UpdateWithAssertions(data);
                }
            }
            catch (Exception ex)
            {
                data.Environment.AddError(ex.Message);
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
                return next; // retry has set a node that we should go back to retry
            }
            if (stop)
            {
                return null;
            }
            if (NextNodes != null && NextNodes.Any())
            {
                return NextNodes.First();
            }
            return null;
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

        /*protected override void OldExecuteTool(IDSFDataObject dataObject, int update)
        {
            var allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);

            dataObject.Settings = new Dev2WorkflowSettingsTO
            {
                EnableDetailedLogging = Config.Server.EnableDetailedLogging,
                LoggerType = LoggerType.JSON,
                KeepLogsForDays = 2,
                CompressOldLogFiles = true
            };
            IStateNotifier stateNotifier = null;
            var outerStateLogger = dataObject.StateNotifier;
            if (dataObject.Settings.EnableDetailedLogging)
            {
                stateNotifier = LogManager.CreateStateNotifier(dataObject);
                dataObject.StateNotifier = stateNotifier;
                stateNotifier?.LogPreExecuteState(this);
            }
            try
            {
                _worker.AddValidationErrors(allErrors);
                if (!allErrors.HasErrors())
                {
                    if (dataObject.IsDebugMode())
                    {
                        ExecuteToolAddDebugItems(dataObject, update);
                    }
                    _worker.ExecuteGate(dataObject, update);
                    stateNotifier?.LogPostExecuteState(this, null);
                }
            }
            catch (Exception e)
            {
                stateNotifier?.LogExecuteException(e, this);
                Dev2Logger.Error(nameof(Gate), e, GlobalConstants.WarewolfError);
                allErrors.AddError(e.Message);
            }
            finally
            {
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError(nameof(GateActivity), allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
                stateNotifier?.Dispose();
                dataObject.StateNotifier = outerStateLogger;
            }
        }*/

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
