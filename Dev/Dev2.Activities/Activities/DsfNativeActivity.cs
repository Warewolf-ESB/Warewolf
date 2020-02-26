#pragma warning disable
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Instrumentation;
using Dev2.Interfaces;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Util;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities.Hosting;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Resource.Messages;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;
using System.Activities.Statements;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.State;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public abstract class DsfNativeActivity<T> : NativeActivity<T>, IDev2ActivityIOMapping, IEquatable<DsfNativeActivity<T>>
    {
        protected ErrorResultTO _errorsTo;
        [GeneralSettings("IsSimulationEnabled")]
        public bool IsSimulationEnabled { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDSFDataObject DataObject { get => null; set => value = null; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDataListCompiler Compiler { get; set; }
        [JsonIgnore]
        public InOutArgument<List<string>> AmbientDataList { get; set; }
        public string InputMapping { get; set; }
        public string OutputMapping { get; set; }
        public bool IsWorkflow { get; set; }
        public bool IsService { get; set; }
        public string ParentServiceName { get; set; }
        public string ParentServiceID { get; set; }
        public string ParentWorkflowInstanceId { get; set; }
        public _simulationMode SimulationMode { get; set; }
        public enum _simulationMode
        {
            OnDemand,
            Never,
            Always
        }
        public string ScenarioID { get; set; }
        protected Guid WorkSurfaceMappingId { get; set; }
        public string UniqueID { get; set; }
        [FindMissing]
        public string OnErrorVariable { get; set; }
        [FindMissing]
        public string OnErrorWorkflow { get; set; }
        public bool IsEndedOnError { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        protected Variable<Guid> DataListExecutionID = new Variable<Guid>();
#pragma warning restore IDE1006 // Naming Styles
        protected List<DebugItem> _debugInputs = new List<DebugItem>(10000);
        protected List<DebugItem> _debugOutputs = new List<DebugItem>(10000);

        readonly IDebugDispatcher _debugDispatcher;
        protected readonly bool _isExecuteAsync;
        string _previousParentInstanceID;
        IDebugState _debugState;
        protected bool _isOnDemandSimulation;
        IResourceCatalog _resourceCatalog;

        protected IDebugState DebugState => _debugState;

        protected DsfNativeActivity(bool isExecuteAsync, string displayName)
            : this(isExecuteAsync, displayName, DebugDispatcher.Instance)
        {
        }

        protected DsfNativeActivity(bool isExecuteAsync, string displayName, IDebugDispatcher debugDispatcher)
        {
            if (debugDispatcher == null)
            {
                throw new ArgumentNullException(nameof(debugDispatcher));
            }

            if (!string.IsNullOrEmpty(displayName))
            {
                DisplayName = displayName;
            }

            _debugDispatcher = debugDispatcher;
            _isExecuteAsync = isExecuteAsync;
            UniqueID = Guid.NewGuid().ToString();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IResourceCatalog ResourceCatalog
        {
            protected get
            {
                return _resourceCatalog ?? Dev2.Runtime.Hosting.ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalog = value;
            }
        }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(DataListExecutionID);
            metadata.AddDefaultExtensionProvider(() => new WorkflowInstanceInfo());
        }

        protected override void Execute(NativeActivityContext context)
        {
        }

        protected void DoErrorHandling(IDSFDataObject dataObject, int update)
        {
            if (dataObject.Environment.HasErrors() && !(this is DsfFlowDecisionActivity))
            {
                var errorString = "";
                if (dataObject.Environment.AllErrors.Count > 0)
                {
                    errorString = string.Join(Environment.NewLine, dataObject.Environment.AllErrors.Last());
                }
                if (dataObject.Environment.Errors.Count > 0)
                {
                    errorString += string.Join(Environment.NewLine, dataObject.Environment.Errors.Last());
                }
                if (!string.IsNullOrEmpty(errorString))
                {
                    PerformCustomErrorHandling(dataObject, errorString, update);
                }
            }
        }

        void PerformCustomErrorHandling(IDSFDataObject dataObject, string currentError, int update)
        {
            try
            {
                if (!string.IsNullOrEmpty(OnErrorVariable))
                {
                    dataObject.Environment.Assign(OnErrorVariable, currentError, update);
                }
                if (!string.IsNullOrEmpty(OnErrorWorkflow))
                {
                    var esbChannel = dataObject.EsbChannel;
                    esbChannel.ExecuteLogErrorRequest(dataObject, dataObject.WorkspaceID, OnErrorWorkflow, out ErrorResultTO tmpErrors, update);
                    if (tmpErrors != null)
                    {
                        dataObject.Environment.AddError(tmpErrors.MakeDisplayReady());
                    }
                }
            }
            catch (Exception e)
            {
                dataObject.Environment.AddError(e.Message);
            }
            finally
            {
                if (IsEndedOnError)
                {
                    PerformStopWorkflow(dataObject);
                }
            }
        }

        public virtual string GetDisplayName() => DisplayName;

        void PerformStopWorkflow(IDSFDataObject dataObject)
        {
            dataObject.StopExecution = true;
            var service = ExecutableServiceRepository.Instance.Get(dataObject.WorkspaceID, dataObject.ResourceID);
            if (service != null)
            {
                Guid.TryParse(dataObject.ParentInstanceID, out Guid parentInstanceID);
                var debugState = new DebugState
                {
                    ID = dataObject.DataListID,
                    ParentID = parentInstanceID,
                    WorkspaceID = dataObject.WorkspaceID,
                    StateType = StateType.End,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    ActivityType = ActivityType.Workflow,
                    DisplayName = dataObject.ServiceName,
                    IsSimulation = dataObject.IsOnDemandSimulation,
                    ServerID = dataObject.ServerID,
                    ClientID = dataObject.ClientID,
                    OriginatingResourceID = dataObject.ResourceID,
                    OriginalInstanceID = dataObject.OriginalInstanceID,
                    SourceResourceID = dataObject.SourceResourceID,
                    Server = string.Empty,
                    Version = string.Empty,
                    SessionID = dataObject.DebugSessionID,
                    EnvironmentID = dataObject.DebugEnvironmentId,
                    Name = IsWorkflow ? ActivityType.Workflow.GetDescription() : IsService ? ActivityType.Service.GetDescription() : ActivityType.Step.GetDescription(),
                    ErrorMessage = "Termination due to error in activity",
                    HasError = true
                };
                DebugDispatcher.Instance.Write(new WriteArgs { debugState = debugState, isTestExecution = dataObject.IsServiceTestExecution, isDebugFromWeb = dataObject.IsDebugFromWeb, testName = dataObject.TestName });
            }
        }


        protected virtual void OnBeforeExecute(NativeActivityContext context)
        {
        }

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected abstract void OnExecute(NativeActivityContext context);


        protected void OnExecutedCompleted(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();

            if (dataObject.ForceDeleteAtNextNativeActivityCleanup)
            {
                dataObject.ForceDeleteAtNextNativeActivityCleanup = false;
            }

            if (!dataObject.IsDebugNested)
            {
                dataObject.ParentInstanceID = _previousParentInstanceID;
            }

            dataObject.NumberOfSteps = dataObject.NumberOfSteps + 1;

        }

        public abstract void UpdateForEachInputs(IList<Tuple<string, string>> updates);
        public abstract void UpdateForEachOutputs(IList<Tuple<string, string>> updates);

        public virtual List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => DebugItem.EmptyList;

        public virtual List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update) => DebugItem.EmptyList;
        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, int update) => DispatchDebugState(dataObject, stateType, update, null, null, false);

        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime) => DispatchDebugState(dataObject, stateType, update, startTime, null, false);

        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime, DateTime? endTime) => DispatchDebugState(dataObject, stateType, update, startTime, endTime, false);

#pragma warning disable S1541 // Methods and properties should not be too complex
        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime, DateTime? endTime, bool decision)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var clearErrors = false;
            try
            {
                Guid.TryParse(dataObject.RemoteInvokerID, out Guid remoteID);

                clearErrors = Dispatch(dataObject, stateType, update, startTime, endTime, remoteID);

                if (_debugState != null && _debugState.StateType != StateType.Duration && !(_debugState.ActivityType == ActivityType.Workflow || decision || _debugState.Name == "DsfDecision") && remoteID == Guid.Empty)
                {
                    _debugState.StateType = StateType.All;

                    if (stateType == StateType.Before)
                    {
                        return;
                    }
                }

                if (dataObject.RemoteServiceType != "Workflow" && !string.IsNullOrWhiteSpace(dataObject.RemoteServiceType) && _debugState != null)
                {
                    _debugState.ActivityType = ActivityType.Service;
                }


                DebugCleanUp(dataObject, stateType);
            }
            finally
            {
                if (clearErrors)
                {
                    foreach (var error in dataObject.Environment.Errors)
                    {
                        dataObject.Environment.AllErrors.Add(error);

                    }
                    dataObject.Environment.Errors.Clear();
                }
            }
        }

        void DebugCleanUp(IDSFDataObject dataObject, StateType stateType)
        {
            if (_debugState != null)
            {
                _debugState.ClientID = dataObject.ClientID;
                _debugState.OriginatingResourceID = dataObject.ResourceID;
                _debugState.SourceResourceID = dataObject.SourceResourceID;
                DispatchDebugState(_debugState, dataObject);
                if (stateType == StateType.After)
                {
                    _debugState = null;
                }
            }
        }

        protected void DispatchDebugState(IDebugState state, IDSFDataObject dataObject)
        {
            if (state != null)
            {
                _debugDispatcher.Write( new WriteArgs { debugState = state, isTestExecution = dataObject.IsServiceTestExecution, isDebugFromWeb = dataObject.IsDebugFromWeb, testName = dataObject.TestName, isRemoteInvoke = dataObject.RemoteInvoke, remoteInvokerId = dataObject.RemoteInvokerID, parentInstanceId = dataObject.ParentInstanceID, remoteDebugItems = dataObject.RemoteDebugItems });
            } 
        }

        bool Dispatch(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime, DateTime? endTime, Guid remoteID)
        {
            var clearErrors = false;
            if (stateType == StateType.Before)
            {
                DispatchForBeforeState(dataObject, stateType, update, startTime, remoteID);
            }
            else
            {
                clearErrors = DispatchForAfterState(dataObject, stateType, update, endTime, remoteID);
            }
            return clearErrors;
        }

        bool DispatchForAfterState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? endTime, Guid remoteID)
        {
            var hasError = dataObject.Environment.Errors.Any();
            var clearErrors = hasError;
            var errorMessage = string.Empty;
            if (hasError)
            {
                errorMessage = string.Join(Environment.NewLine, dataObject.Environment.Errors.Distinct());
            }

            if (_debugState == null)
            {
                InitializeDebugState(stateType, dataObject, remoteID, hasError, errorMessage);
            }

            if (_debugState != null)
            {
                if (stateType != StateType.Before)
                {
                    if (endTime == null)
                    {
                        endTime = DateTime.Now;
                    }
                    _debugState.EndTime = endTime.Value;
                }

                _debugState.NumberOfSteps = IsWorkflow ? dataObject.NumberOfSteps : 0;
                _debugState.StateType = stateType;
                _debugState.HasError = hasError;
                _debugState.ErrorMessage = errorMessage;
                try
                {
                    TryDispatchDebugOutput(dataObject, update);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error("Debug Dispatch Error", e, GlobalConstants.WarewolfError);
                    AddErrorToDataList(e, dataObject);
                    errorMessage = dataObject.Environment.FetchErrors();
                    _debugState.ErrorMessage = errorMessage;
                    _debugState.HasError = true;
                }
            }
            return clearErrors;
        }

        private void TryDispatchDebugOutput(IDSFDataObject dataObject, int update)
        {
            if (dataObject.RunWorkflowAsync && !_debugState.HasError)
            {
                var debugItem = new DebugItem();
                var debugItemResult = new DebugItemResult { Type = DebugItemResultType.Value, Value = "Asynchronous execution started" };
                debugItem.Add(debugItemResult);
                _debugState.Outputs.Add(debugItem);
                _debugState.NumberOfSteps = 0;
            }
            else
            {
                if (_debugState.StateType != StateType.Duration)
                {
                    UpdateDebugWithAssertions(dataObject);
                }
                var debugOutputs = GetDebugOutputs(dataObject.Environment, update);
                Copy(debugOutputs, _debugState.Outputs);
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        void DispatchForBeforeState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime, Guid remoteID)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (_debugState == null)
            {
                InitializeDebugState(stateType, dataObject, remoteID, false, "");
            }
            else
            {
                _debugState.StateType = stateType;
                Dev2Logger.Info("Debug Already Started", GlobalConstants.WarewolfInfo);
            }

            if (_debugState != null)
            {
                if (stateType == StateType.Before)
                {
                    if (startTime == null)
                    {
                        startTime = DateTime.Now;
                    }
                    if (_debugState.StartTime == DateTime.MinValue)
                    {
                        _debugState.StartTime = startTime.Value;
                    }
                }
                _debugState.Name = IsWorkflow ? ActivityType.Workflow.GetDescription() : IsService ? ActivityType.Service.GetDescription() : ActivityType.Step.GetDescription();
                try
                {
                    var debugInputs = GetDebugInputs(dataObject.Environment, update);
                    Copy(debugInputs, _debugState.Inputs);
                }
                catch (Exception err)
                {
                    Dev2Logger.Error("DispatchDebugState", err, GlobalConstants.WarewolfError);
                    AddErrorToDataList(err, dataObject);
                    var errorMessage = dataObject.Environment.FetchErrors();
                    _debugState.ErrorMessage = errorMessage;
                    _debugState.HasError = true;
                    var debugError = err as DebugCopyException;
                    if (debugError != null)
                    {
                        _debugState.Inputs.Add(debugError.Item);
                    }
                }

                if (dataObject.RemoteServiceType == "Workflow" && !_debugState.HasError)
                {
                    var debugItem = new DebugItem();
                    var debugItemResult = new DebugItemResult { Type = DebugItemResultType.Value, Label = "Execute workflow asynchronously: ", Value = dataObject.RunWorkflowAsync ? "True" : "False" };
                    debugItem.Add(debugItemResult);
                    _debugState.Inputs.Add(debugItem);
                }
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        void UpdateDebugWithAssertions(IDSFDataObject dataObject)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (dataObject.IsServiceTestExecution)
            {
                var serviceTestSteps = dataObject.ServiceTest?.TestSteps;
                var stepToBeAsserted = serviceTestSteps?.FirstOrDefault(step => step.Type == StepType.Assert && step.UniqueId == Guid.Parse(UniqueID) && step.ActivityType != typeof(DsfForEachActivity).Name && step.ActivityType != typeof(DsfSelectAndApplyActivity).Name && step.ActivityType != typeof(DsfSequenceActivity).Name && step.ActivityType != typeof(DsfEnhancedDotNetDllActivity).Name);
                if (stepToBeAsserted?.StepOutputs != null && stepToBeAsserted.StepOutputs.Count > 0)
                {
                    if (stepToBeAsserted.Result != null)
                    {
                        stepToBeAsserted.Result.RunTestResult = RunResult.TestInvalid;
                    }
                    if (stepToBeAsserted.ActivityType == typeof(DsfDecision).Name)
                    {
                        DecisionAssertion(dataObject, stepToBeAsserted);
                    }
                    else if (stepToBeAsserted.ActivityType == typeof(DsfSwitch).Name)
                    {
                        SwitchAssertion(dataObject, stepToBeAsserted);
                    }
                    else
                    {
                        RegularActivityAssertion(dataObject, stepToBeAsserted);
                    }
                }
                else
                {
                    if (stepToBeAsserted != null)
                    {
                        stepToBeAsserted.Result = new TestRunResult
                        {
                            RunTestResult = RunResult.TestPending
                        };
                    }
                }
            }
        }

        protected void RegularActivityAssertion(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
        {
            var factory = Dev2DecisionFactory.Instance();
            var res = stepToBeAsserted.StepOutputs.SelectMany(output => GetTestRunResults(dataObject, output, factory));
            var testRunResults = res as IList<TestRunResult> ?? res.ToList();
            var testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed || result.RunTestResult == RunResult.None);
            var serviceTestFailureMessage = string.Join("", testRunResults.Select(result => result.Message));

            UpdateStepWithFinalResult(dataObject, stepToBeAsserted, testPassed, testRunResults, serviceTestFailureMessage);
        }

        static void UpdateStepWithFinalResult(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted, bool testPassed, IList<TestRunResult> testRunResults, string serviceTestFailureMessage)
        {
            ServiceTestHelper.UpdateBasedOnFinalResult(dataObject, stepToBeAsserted, testPassed, testRunResults, serviceTestFailureMessage);
        }

        void SwitchAssertion(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
        {
            var serviceTestOutput = stepToBeAsserted.StepOutputs[0];
            if (serviceTestOutput.Result == null)
            {
                serviceTestOutput.Result = new TestRunResult();
            }
            serviceTestOutput.Result.RunTestResult = RunResult.TestInvalid;
            var dsfSwitch = this as DsfSwitch;
            if (dsfSwitch != null)
            {
                var assertPassed = dsfSwitch.Result == serviceTestOutput.Value;
                serviceTestOutput.Result.RunTestResult = assertPassed ? RunResult.TestPassed : RunResult.TestFailed;
                UpdateStepWithFinalResult(dataObject, stepToBeAsserted, assertPassed, new List<TestRunResult> { serviceTestOutput.Result }, "");
                if (dataObject.IsDebugMode())
                {
                    var msg = Messages.Test_FailureResult;
                    if (assertPassed)
                    {
                        msg = Messages.Test_PassedResult;
                    }
                    var hasError = msg == Messages.Test_FailureResult;
                    AddDebugAssertResultItem(new DebugItemServiceTestStaticDataParams(msg, hasError));
                }
                else
                {
                    dataObject.Environment.AddError(Messages.Test_FailureResult);
                }
            }
        }

        void DecisionAssertion(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
        {
            var serviceTestOutput = stepToBeAsserted.StepOutputs[0];

            if (serviceTestOutput.Result == null)
            {
                serviceTestOutput.Result = new TestRunResult();
            }
            serviceTestOutput.Result.RunTestResult = RunResult.TestInvalid;
            var dsfDecision = this as DsfDecision;
            if (dsfDecision != null)
            {
                var assertPassed = dsfDecision.Result == serviceTestOutput.Value;
                serviceTestOutput.Result.RunTestResult = assertPassed ? RunResult.TestPassed : RunResult.TestFailed;
                UpdateStepWithFinalResult(dataObject, stepToBeAsserted, assertPassed, new List<TestRunResult> { serviceTestOutput.Result }, "");
                if (dataObject.IsDebugMode())
                {
                    var msg = Messages.Test_FailureResult;
                    if (assertPassed)
                    {
                        msg = Messages.Test_PassedResult;
                    }
                    var hasError = msg == Messages.Test_FailureResult;
                    AddDebugAssertResultItem(new DebugItemServiceTestStaticDataParams(msg, hasError));
                }
                else
                {
                    dataObject.Environment.AddError(Messages.Test_FailureResult);
                }
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        protected void UpdateWithAssertions(IDSFDataObject dataObject)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (dataObject.IsServiceTestExecution)
            {
                var serviceTestSteps = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children ?? new List<IServiceTestStep>().ToObservableCollection());
                var testSteps = serviceTestSteps as IList<IServiceTestStep> ?? serviceTestSteps?.ToList();
                var assertSteps = testSteps?.Where(step => step.Type == StepType.Assert
                                                                             && step.UniqueId == Guid.Parse(UniqueID)
                                                                             && step.ActivityType != typeof(DsfForEachActivity).Name
                                                                             && step.ActivityType != typeof(DsfSelectAndApplyActivity).Name
                                                                             && step.ActivityType != typeof(DsfSequenceActivity).Name) ?? new List<IServiceTestStep>();
                foreach (var stepToBeAsserted in assertSteps)
                {
                    UpdateStep(dataObject, stepToBeAsserted);
                }
            }
        }

        private void UpdateStep(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
        {
            if (stepToBeAsserted?.StepOutputs != null && stepToBeAsserted.StepOutputs.Count > 0)
            {
                if (stepToBeAsserted.Result != null)
                {
                    stepToBeAsserted.Result.RunTestResult = RunResult.TestPending;
                }
                if (stepToBeAsserted.ActivityType == typeof(DsfDecision).Name)
                {
                    UpdateForDecision(dataObject, stepToBeAsserted);
                }
                else if (stepToBeAsserted.ActivityType == typeof(DsfSwitch).Name)
                {
                    UpdateForSwitch(dataObject, stepToBeAsserted);
                }
                else
                {
                    UpdateForRegularActivity(dataObject, stepToBeAsserted);
                }
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        protected void UpdateForRegularActivity(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var factory = Dev2DecisionFactory.Instance();
            var testRunResults = stepToBeAsserted.StepOutputs.SelectMany(output => GetTestRunResults(dataObject, output, factory)).ToList();
            var testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed || result.RunTestResult == RunResult.None);
            var serviceTestFailureMessage = string.Join("", testRunResults.Select(result => result.Message));
            var finalResult = new TestRunResult();
            if (testPassed)
            {
                finalResult.RunTestResult = RunResult.TestPassed;
                if (stepToBeAsserted.Result != null)
                {
                    stepToBeAsserted.Result.RunTestResult = RunResult.TestPassed;
                }
            }
            if (testRunResults.Any(result => result.RunTestResult == RunResult.TestFailed))
            {
                finalResult.RunTestResult = RunResult.TestFailed;
                finalResult.Message = serviceTestFailureMessage;
                if (stepToBeAsserted.Result != null)
                {
                    stepToBeAsserted.Result.RunTestResult = RunResult.TestFailed;
                }
            }
            if (testRunResults.Any(result => result.RunTestResult == RunResult.TestInvalid))
            {
                finalResult.RunTestResult = RunResult.TestInvalid;
                finalResult.Message = serviceTestFailureMessage;
                if (stepToBeAsserted.Result != null)
                {
                    stepToBeAsserted.Result.RunTestResult = RunResult.TestInvalid;
                }
            }
            if (testRunResults.Any(result => result.RunTestResult == RunResult.TestPending))
            {
                finalResult.RunTestResult = RunResult.TestPending;
                finalResult.Message = serviceTestFailureMessage;
                if (stepToBeAsserted.Result != null)
                {
                    stepToBeAsserted.Result.RunTestResult = RunResult.TestPending;
                }
            }
            if (dataObject.ServiceTest != null)
            {
                dataObject.ServiceTest.Result = finalResult;
                dataObject.ServiceTest.TestFailing = !testPassed;
                dataObject.ServiceTest.FailureMessage = serviceTestFailureMessage;
                dataObject.ServiceTest.TestPassed = testPassed;
            }
            dataObject.StopExecution = !testPassed;
        }

        void UpdateForSwitch(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
        {
            var serviceTestOutput = stepToBeAsserted.StepOutputs[0];
            if (serviceTestOutput.Result != null)
            {
                serviceTestOutput.Result.RunTestResult = RunResult.TestPending;
            }
            var dsfDecision = this as DsfSwitch;
            if (dsfDecision != null)
            {
                var assertPassed = dsfDecision.Result == serviceTestOutput.Value;
                if (dataObject.ServiceTest != null)
                {
                    dataObject.ServiceTest.TestPassed = assertPassed;
                    dataObject.ServiceTest.TestFailing = !assertPassed;
                    SetPassResult(dataObject, assertPassed, serviceTestOutput, stepToBeAsserted);
                }
            }
        }

        void UpdateForDecision(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
        {
            var serviceTestOutput = stepToBeAsserted.StepOutputs[0];

            if (serviceTestOutput.Result != null)
            {
                serviceTestOutput.Result.RunTestResult = RunResult.TestPending;
            }
            var dsfDecision = this as DsfDecision;
            if (dsfDecision != null)
            {
                var assertPassed = dsfDecision.Result == serviceTestOutput.Value;
                if (dataObject.ServiceTest != null)
                {
                    dataObject.ServiceTest.TestPassed = assertPassed;
                    dataObject.ServiceTest.TestFailing = !assertPassed;

                    SetPassResult(dataObject, assertPassed, serviceTestOutput, stepToBeAsserted);
                }
            }
        }

        static void SetPassResult(IDSFDataObject dataObject, bool assertPassed, IServiceTestOutput serviceTestOutput, IServiceTestStep stepToBeAsserted)
        {
            if (assertPassed)
            {
                serviceTestOutput.Result.RunTestResult = RunResult.TestPassed;
                dataObject.ServiceTest.Result.RunTestResult = RunResult.TestPassed;
                dataObject.ServiceTest.TestPassed = true;
                stepToBeAsserted.Result.RunTestResult = RunResult.TestPassed;
            }
            if (!assertPassed)
            {
                serviceTestOutput.Result.RunTestResult = RunResult.TestFailed;
                serviceTestOutput.Result.RunTestResult = RunResult.TestFailed;
                dataObject.ServiceTest.Result.RunTestResult = RunResult.TestFailed;
                dataObject.ServiceTest.TestFailing = true;
                serviceTestOutput.Result.Message = Messages.Test_FailureResult;
                stepToBeAsserted.Result.RunTestResult = RunResult.TestFailed;
                dataObject.Environment.AddError(Messages.Test_FailureResult);
            }
            UpdateStepWithFinalResult(dataObject, stepToBeAsserted, assertPassed, new List<TestRunResult> { stepToBeAsserted.Result }, "");
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (output == null)
            {
                var testResult = new TestRunResult
                {
                    RunTestResult = RunResult.None
                };
                return new List<TestRunResult> { testResult };
            }
            if (string.IsNullOrEmpty(output.Variable) && string.IsNullOrEmpty(output.Value))
            {
                var testResult = new TestRunResult
                {
                    RunTestResult = RunResult.None
                };
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
                AddDebugAssertResultItem(new DebugItemServiceTestStaticDataParams(testResult.Message, true));
                return new List<TestRunResult> { testResult };
            }
            var opt = FindRecsetOptions.FindMatch(output.AssertOp);
            var decisionType = DecisionDisplayHelper.GetValue(output.AssertOp);

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
                var variableValue = iter.FetchNextValue(c1);
                var val2 = iter.FetchNextValue(c2);
                var val3 = iter.FetchNextValue(c3);
                var assertResult = factory.FetchDecisionFunction(decisionType).Invoke(new[] { variableValue, val2, val3 });
                var testResult = new TestRunResult();
                if (assertResult)
                {
                    testResult.RunTestResult = RunResult.TestPassed;
                }
                else
                {
                    testResult.RunTestResult = RunResult.TestFailed;
                    var msg = DecisionDisplayHelper.GetFailureMessage(decisionType);
                    var actMsg = string.Format(msg, val2, variable, variableValue, val3);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                    if (testResult.Message.EndsWith(Environment.NewLine, StringComparison.CurrentCulture))
                    {
                        testResult.Message = testResult.Message.Replace(Environment.NewLine, "").Replace("\r", "");
                    }
                }
                if (dataObject.IsDebugMode())
                {
                    var msg = testResult.Message;
                    if (testResult.RunTestResult == RunResult.TestPassed)
                    {
                        msg = Messages.Test_PassedResult;
                        msg += opt.ArgumentCount > 2 ? ": " + output.Variable + " " + output.AssertOp + " " + output.From + " and " + output.To : ": " + output.Variable + " " + output.AssertOp + " " + output.Value;
                    }
                    var hasError = testResult.RunTestResult == RunResult.TestFailed;
                    AddDebugAssertResultItem(new DebugItemServiceTestStaticDataParams(msg, hasError));
                }
                output.Result = testResult;
                ret.Add(testResult);
            }
            return ret;
        }

        void AddErrorToDataList(Exception err, IDSFDataObject dataObject)
        {
            var errorString = err.Message;
            dataObject.Environment.Errors.Add(errorString);
        }

        protected void InitializeDebug(IDSFDataObject dataObject)
        {
            if (dataObject.IsDebugMode())
            {
                var errorMessage = string.Empty;
                Guid.TryParse(dataObject.RemoteInvokerID, out Guid remoteID);
                InitializeDebugState(StateType.Before, dataObject, remoteID, false, errorMessage);
            }
        }

        protected void DispatchDebugStateAndUpdateRemoteServer(IDSFDataObject dataObject, StateType before, int update)
        {
            if (_debugState != null)
            {
                Guid.TryParse(dataObject.RemoteInvokerID, out Guid remoteID);
                var res = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, remoteID);
                var name = remoteID != Guid.Empty ? res != null ? res.ResourceName : "localhost" : "localhost";
                _debugState.Server = name;
            }
            DispatchDebugState(dataObject, before, update);
        }

        protected string GetServerName() => _debugState?.Server;

#pragma warning disable S1541 // Methods and properties should not be too complex
        protected void InitializeDebugState(StateType stateType, IDSFDataObject dataObject, Guid remoteID, bool hasError, string errorMessage)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            Guid.TryParse(dataObject.ParentInstanceID, out Guid parentInstanceID);
            if (stateType != StateType.Duration)
            {
                UpdateDebugParentID(dataObject);
            }
            if (remoteID != Guid.Empty)
            {
                UniqueID = Guid.NewGuid().ToString();
            }

            string name;
            if (remoteID != Guid.Empty)
            {
                var resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, remoteID);
                name = resource != null ? resource.ResourceName : remoteID.ToString();
            }
            else
            {
                name = "localhost";
            }

            switch (dataObject.RemoteServiceType)
            {
                case "DbService":
                case "PluginService":
                case "WebService":
                    IsService = true;
                    break;
                default:
                    break;
            }

            var type = GetType();
            var typeName = type.Name;
            _debugState = new DebugState
            {
                ID = Guid.Parse(UniqueID),
                ParentID = parentInstanceID,
                WorkSurfaceMappingId = WorkSurfaceMappingId,
                WorkspaceID = dataObject.WorkspaceID,
                StateType = stateType,
                ActualType = typeName,
                StartTime = DateTime.Now,
                ActivityType = IsWorkflow ? ActivityType.Workflow : ActivityType.Step,
                DisplayName = DisplayName,
                IsSimulation = false,
                ServerID = dataObject.ServerID,
                OriginatingResourceID = dataObject.ResourceID,
                OriginalInstanceID = dataObject.OriginalInstanceID,
                Server = name,
                Version = string.Empty,
                Name = IsWorkflow ? ActivityType.Workflow.GetDescription() : IsService ? ActivityType.Service.GetDescription() : ActivityType.Step.GetDescription(),
                HasError = hasError,
                ErrorMessage = errorMessage,
                EnvironmentID = dataObject.DebugEnvironmentId,
                SessionID = dataObject.DebugSessionID
            };
        }

        public virtual void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
        }
        static void Copy<TItem>(IEnumerable<TItem> src, List<TItem> dest)
        {
            if (src == null || dest == null)
            {
                return;
            }

            dest.AddRange(src);
        }

        protected static void DisplayAndWriteError(string serviceName, IErrorResultTO errors)
        {
            var errorBuilder = new StringBuilder();
            foreach (var e in errors.FetchErrors())
            {
                errorBuilder.AppendLine($"--[ Execution Exception ]--\r\nService Name = {serviceName}\r\nError Message = {e} \r\n--[ End Execution Exception ]--");
            }
            Dev2Logger.Error("DsfNativeActivity", new Exception(errorBuilder.ToString()), GlobalConstants.WarewolfError);
        }

        public abstract IList<DsfForEachItem> GetForEachInputs();
        public abstract IList<DsfForEachItem> GetForEachOutputs();

        protected IList<DsfForEachItem> GetForEachItems(params string[] strings)
        {
            if (strings == null || strings.Length == 0)
            {
                return DsfForEachItem.EmptyList;
            }

            return (from s in strings
                    where !string.IsNullOrEmpty(s)
                    select new DsfForEachItem
                    {
                        Name = s,
                        Value = s
                    }).ToList();
        }

        public virtual enFindMissingType GetFindMissingType() => enFindMissingType.StaticActivity;

        public virtual IDev2Activity Execute(IDSFDataObject data, int update)
        {
            try
            {
                _debugInputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();
                ExecuteTool(data, update);
                if (!data.IsDebugMode())
                {
                    UpdateWithAssertions(data);
                }
            }
            catch (Exception ex)
            {
                data.Environment.AddError(ex.Message);
                Dev2Logger.Error("OnExecute", ex, GlobalConstants.WarewolfError);

            }
            finally
            {
                if (!_isExecuteAsync || _isOnDemandSimulation)
                {
                    DoErrorHandling(data, update);
                }
            }
            if (NextNodes != null && NextNodes.Any())
            {
                return NextNodes.First();
            }
            return null;
        }

        protected abstract void ExecuteTool(IDSFDataObject dataObject, int update);

        public abstract List<string> GetOutputs();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<IDev2Activity> NextNodes { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guid ActivityId { get; set; }

        public bool IsGate { get; set; }

        public virtual FlowNode GetFlowNode()
        {
            var flowStep = new FlowStep { Action = this as Activity };
            return flowStep;
        }

        public virtual IEnumerable<IDev2Activity> GetNextNodes() => NextNodes ?? new List<IDev2Activity>();

        public virtual List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> ArmConnectors()
        {
            var armConnectors = new List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)>();
            foreach (var next in GetNextNodes())
            {
                armConnectors.Add(($"{GetDisplayName()} -> {next.GetDisplayName()}", null, UniqueID, next.UniqueID));
            }
            return armConnectors;
        }

        public virtual IEnumerable<IDev2Activity> GetChildrenNodes()
        {
            var nextNodes = new List<IDev2Activity>();
            return nextNodes;
        }

        protected void AddDebugInputItem(DebugOutputBase parameters)
        {
            IDebugItem itemToAdd = new DebugItem();
            itemToAdd.AddRange(parameters.GetDebugItemResult());
#pragma warning disable S3215
            _debugInputs.Add((DebugItem)itemToAdd);
#pragma warning restore S3215
        }

        protected void AddDebugOutputItem(DebugOutputBase parameters)
        {
            var itemToAdd = new DebugItem();
            itemToAdd.AddRange(parameters.GetDebugItemResult());
            _debugOutputs.Add(itemToAdd);
        }

        protected void AddDebugAssertResultItem(DebugOutputBase parameters)
        {
            var itemToAdd = new DebugItem();
            itemToAdd.AddRange(parameters.GetDebugItemResult());
            _debugState.AssertResultList.Add(itemToAdd);
        }

        protected void AddDebugItem(DebugOutputBase parameters, DebugItem debugItem)
        {
            try
            {
                var debugItemResults = parameters.GetDebugItemResult();
                debugItem.AddRange(debugItemResults);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        public IDebugState GetDebugState() => _debugState;

        public Guid GetWorkSurfaceMappingId() => WorkSurfaceMappingId;

        public abstract IEnumerable<StateVariable> GetState();

        public virtual IList<IActionableErrorInfo> PerformValidation() => new List<IActionableErrorInfo>();

        public bool Equals(DsfNativeActivity<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(UniqueID, other.UniqueID);
        }

        public abstract override bool Equals(object obj);

        public override int GetHashCode() => UniqueID?.GetHashCode() ?? 0;
        T1 IDev2Activity.As<T1>() => this as T1;

        public static bool operator ==(DsfNativeActivity<T> left, DsfNativeActivity<T> right) => Equals(left, right);

        public static bool operator !=(DsfNativeActivity<T> left, DsfNativeActivity<T> right) => !Equals(left, right);
    }
}
