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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
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
using Dev2.Simulation;
using Dev2.Util;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities.Hosting;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable ReturnTypeCanBeEnumerable.Global

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // ReSharper disable once RedundantExtendsListEntry
    public abstract class DsfNativeActivity<T> : NativeActivity<T>, IDev2ActivityIOMapping, IDev2Activity, IEquatable<DsfNativeActivity<T>>
    {
        protected ErrorResultTO errorsTo;
        [GeneralSettings("IsSimulationEnabled")]
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public bool IsSimulationEnabled { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper disable RedundantAssignment
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        // ReSharper disable MemberCanBeProtected.Global
        public IDSFDataObject DataObject { get { return null; } set { value = null; } }
        // ReSharper restore MemberCanBeProtected.Global
        // ReSharper restore RedundantAssignment
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        // ReSharper disable UnusedMember.Global
        public IDataListCompiler Compiler { get; set; }
        // ReSharper restore UnusedMember.Global
        [JsonIgnore]
        public InOutArgument<List<string>> AmbientDataList { get; set; }

        public string InputMapping { get; set; }

        public string OutputMapping { get; set; }

        public bool IsWorkflow { get; set; }
        public bool IsService { get; set; }
        public string ParentServiceName { get; set; }
        // ReSharper disable UnusedMember.Global
        public string ParentServiceID { get; set; }
        // ReSharper restore UnusedMember.Global
        public string ParentWorkflowInstanceId { get; set; }
        public SimulationMode SimulationMode { get; set; }
        public string ScenarioID { get; set; }
        protected Guid WorkSurfaceMappingId { get; set; }
        /// <summary>
        /// UniqueID is the InstanceID and MUST be a guid.
        /// </summary>
        public string UniqueID { get; set; }

        [FindMissing]
        public string OnErrorVariable { get; set; }
        [FindMissing]
        public string OnErrorWorkflow { get; set; }
        public bool IsEndedOnError { get; set; }

        protected Variable<Guid> DataListExecutionID = new Variable<Guid>();
        protected List<DebugItem> _debugInputs = new List<DebugItem>(10000);
        protected List<DebugItem> _debugOutputs = new List<DebugItem>(10000);
        protected List<DebugItem> _assertResultList = new List<DebugItem>(10000);

        readonly IDebugDispatcher _debugDispatcher;
        readonly bool _isExecuteAsync;
        string _previousParentInstanceID;
        IDebugState _debugState;
        bool _isOnDemandSimulation;
        private IResourceCatalog _resourceCatalog;
        ErrorResultTO _tmpErrors = new ErrorResultTO();

        protected IDebugState DebugState => _debugState;
        // protected for testing!

        #region ShouldExecuteSimulation

        bool ShouldExecuteSimulation => _isOnDemandSimulation
            ? SimulationMode != SimulationMode.Never
            : SimulationMode == SimulationMode.Always;

        #endregion

        #region Ctor

        protected DsfNativeActivity(bool isExecuteAsync, string displayName)
            : this(isExecuteAsync, displayName, DebugDispatcher.Instance)
        {

        }

        protected DsfNativeActivity(bool isExecuteAsync, string displayName, IDebugDispatcher debugDispatcher)
        {
            if (debugDispatcher == null)
            {
                throw new ArgumentNullException("debugDispatcher");
            }

            if (!string.IsNullOrEmpty(displayName))
            {
                DisplayName = displayName;
            }

            _debugDispatcher = debugDispatcher;
            _isExecuteAsync = isExecuteAsync;

            // This will get overwritten when rehydrating
            UniqueID = Guid.NewGuid().ToString();
        }

        #endregion

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

        #region CacheMetadata

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddImplementationVariable(DataListExecutionID);
            metadata.AddDefaultExtensionProvider(() => new WorkflowInstanceInfo());
        }

        #endregion

        #region Execute

        // 4423 : TWR - sealed so that this cannot be overridden
        protected sealed override void Execute(NativeActivityContext context)
        {
            Dev2Logger.Debug(String.Format("Start {0}", GetType().Name));
            _tmpErrors = new ErrorResultTO();
            _isOnDemandSimulation = false;
            var dataObject = context.GetExtension<IDSFDataObject>();



            // we need to register this child thread with the DataListRegistar so we can scope correctly ;)


            string errorString = dataObject.Environment.FetchErrors();
            _tmpErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
            if (!(this is DsfFlowDecisionActivity))
            {
            }

            DataListExecutionID.Set(context, dataObject.DataListID);


            _previousParentInstanceID = dataObject.ParentInstanceID;
            _isOnDemandSimulation = dataObject.IsOnDemandSimulation;

            OnBeforeExecute(context);

            try
            {
                var className = GetType().Name;
                Tracker.TrackEvent(TrackerEventGroup.ActivityExecution, className);

                if (ShouldExecuteSimulation)
                {
                    OnExecuteSimulation(context);
                }
                else
                {
                    OnExecute(context);
                }
            }
            catch (Exception ex)
            {

                Dev2Logger.Error("OnExecute", ex);
                errorString = ex.Message;
                var errorResultTO = new ErrorResultTO();
                errorResultTO.AddError(errorString);
                if (dataObject.Environment != null)
                {
                    dataObject.Environment.AddError(errorResultTO.MakeDataListReady());
                }
            }
            finally
            {
                if (!_isExecuteAsync || _isOnDemandSimulation)
                {
                    var resumable = dataObject.WorkflowResumeable;
                    OnExecutedCompleted(context, false, resumable);
                    if (dataObject.Environment != null)
                    {
                        DoErrorHandling(dataObject, 0); // old wf code
                    }
                }

            }
        }

        protected void DoErrorHandling(IDSFDataObject dataObject, int update)
        {
            string errorString = dataObject.Environment.FetchErrors();
            _tmpErrors.AddError(errorString);
            if (_tmpErrors.HasErrors())
            {
                if (!(this is DsfFlowDecisionActivity))
                {
                    if (!String.IsNullOrEmpty(errorString))
                    {
                        PerformCustomErrorHandling(dataObject, errorString, update);
                    }
                }
            }
        }

        void PerformCustomErrorHandling(IDSFDataObject dataObject, string currentError, int update)
        {
            try
            {
                if (!String.IsNullOrEmpty(OnErrorVariable))
                {
                    dataObject.Environment.Assign(OnErrorVariable, currentError, update);
                }
                if (!String.IsNullOrEmpty(OnErrorWorkflow))
                {
                    var esbChannel = dataObject.EsbChannel;
                    ErrorResultTO tmpErrors;
                    esbChannel.ExecuteLogErrorRequest(dataObject, dataObject.WorkspaceID, OnErrorWorkflow, out tmpErrors, update);
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

        void PerformStopWorkflow(IDSFDataObject dataObject)
        {
            dataObject.StopExecution = true;
            var service = ExecutableServiceRepository.Instance.Get(dataObject.WorkspaceID, dataObject.ResourceID);
            if (service != null)
            {
                Guid parentInstanceID;
                Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceID);
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
                DebugDispatcher.Instance.Write(debugState, dataObject.IsServiceTestExecution, dataObject.TestName);
            }
        }

        #endregion

        #region OnBeforeExecute

        protected virtual void OnBeforeExecute(NativeActivityContext context)
        {
        }

        #endregion

        #region OnExecute

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected abstract void OnExecute(NativeActivityContext context);

        #endregion

        #region OnExecuteSimulation

        /// <summary>
        /// When overridden runs the activity's simulation logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        // ReSharper disable VirtualMemberNeverOverriden.Global
        protected virtual void OnExecuteSimulation(NativeActivityContext context)
        // ReSharper restore VirtualMemberNeverOverriden.Global
        {
            var rootInfo = context.GetExtension<WorkflowInstanceInfo>();

            var key = new SimulationKey
            {
                WorkflowID = rootInfo.ProxyName,
                ActivityID = UniqueID,
                ScenarioID = ScenarioID
            };
            var result = SimulationRepository.Instance.Get(key);
            if (result != null && result.Value != null)
            {

                var dataObject = context.GetExtension<IDSFDataObject>();

                if (dataObject != null)
                {
                    var allErrors = new ErrorResultTO();
                    allErrors.MergeErrors(errorsTo);

                    allErrors.MergeErrors(errorsTo);

                    allErrors.MergeErrors(errorsTo);

                    if (allErrors.HasErrors())
                    {
                        DisplayAndWriteError(rootInfo.ProxyName, allErrors);
                        dataObject.Environment.AddError(allErrors.MakeDataListReady());
                    }
                }
            }
        }

        #endregion

        #region OnExecutedCompleted

        protected virtual void OnExecutedCompleted(NativeActivityContext context, bool hasError, bool isResumable)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();

            if (dataObject.ForceDeleteAtNextNativeActivityCleanup)
            {
                // Used for web-pages to signal a force delete after checks of what would become a zombie datalist ;)
                dataObject.ForceDeleteAtNextNativeActivityCleanup = false; // set back
            }

            if (!dataObject.IsDebugNested)
            {
                dataObject.ParentInstanceID = _previousParentInstanceID;
            }

            dataObject.NumberOfSteps = dataObject.NumberOfSteps + 1;
            //Disposes of all used data lists 

            int threadID = Thread.CurrentThread.ManagedThreadId;

            if (dataObject.IsDebugMode())
            {
                List<Guid> datlistIds;
                if (!dataObject.ThreadsToDispose.TryGetValue(threadID, out datlistIds))
                {
                    dataObject.ThreadsToDispose.Add(threadID, new List<Guid> { dataObject.DataListID });
                }
                else
                {
                    if (!datlistIds.Contains(dataObject.DataListID))
                    {
                        datlistIds.Add(dataObject.DataListID);
                        dataObject.ThreadsToDispose[threadID] = datlistIds;
                    }
                }
            }

        }

        #endregion

        #region ForEach Mapping

        public abstract void UpdateForEachInputs(IList<Tuple<string, string>> updates);

        public abstract void UpdateForEachOutputs(IList<Tuple<string, string>> updates);

        #endregion

        #region GetDebugInputs/Outputs

        public virtual List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            return DebugItem.EmptyList;
        }

        public virtual List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            return DebugItem.EmptyList;
        }
        #endregion

        #region DispatchDebugState

        public void DispatchDebugState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime = null, DateTime? endTime = null, bool decision = false)
        {
            bool clearErrors = false;
            try
            {
                Guid remoteID;
                Guid.TryParse(dataObject.RemoteInvokerID, out remoteID);

                clearErrors = Dispatch(dataObject, stateType, update, startTime, endTime, remoteID);

                if (_debugState != null && _debugState.StateType != StateType.Duration && !(_debugState.ActivityType == ActivityType.Workflow || decision || _debugState.Name == "DsfDecision") && remoteID == Guid.Empty)
                {
                    _debugState.StateType = StateType.All;

                    if (stateType == StateType.Before)
                    {
                        return;
                    }
                }

                if (dataObject.RemoteServiceType != "Workflow" && !string.IsNullOrWhiteSpace(dataObject.RemoteServiceType))
                {
                    if (_debugState != null)
                    {
                        _debugState.ActivityType = ActivityType.Service;
                    }
                }

                DebugCleanUp(dataObject, stateType, remoteID);
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

        private void DebugCleanUp(IDSFDataObject dataObject, StateType stateType, Guid remoteID)
        {
            if (_debugState != null)
            {
                _debugState.ClientID = dataObject.ClientID;
                _debugState.OriginatingResourceID = dataObject.ResourceID;
                _debugState.SourceResourceID = dataObject.SourceResourceID;
                _debugDispatcher.Write(_debugState, dataObject.IsServiceTestExecution, dataObject.TestName, dataObject.RemoteInvoke, dataObject.RemoteInvokerID, dataObject.ParentInstanceID, dataObject.RemoteDebugItems);

                if (stateType == StateType.After)
                {
                    _debugState = null;
                }
            }
        }

        private bool Dispatch(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime, DateTime? endTime, Guid remoteID)
        {
            var clearErrors = false;
            if (stateType == StateType.Before)
            {
                DispatchForBeforeState(dataObject, stateType, update, startTime, endTime, remoteID);
            }
            else
            {
                clearErrors = DispatchForAfterState(dataObject, stateType, update, startTime, endTime, remoteID);
            }
            return clearErrors;
        }

        private bool DispatchForAfterState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime, DateTime? endTime, Guid remoteID)
        {
            bool hasError = dataObject.Environment.Errors.Any();
            var clearErrors = hasError;
            var errorMessage = string.Empty;
            if (hasError)
            {
                errorMessage = string.Join(Environment.NewLine, dataObject.Environment.Errors.Distinct());
            }

            if (_debugState == null)
            {
                InitializeDebugState(stateType, dataObject, remoteID, hasError, errorMessage, startTime, endTime);
            }
            else
            {
                Dev2Logger.Debug("Debug already initialised");
            }

            if (_debugState != null)
            {
                _debugState.NumberOfSteps = IsWorkflow ? dataObject.NumberOfSteps : 0;
                _debugState.StateType = stateType;
                if (endTime != null)
                {
                    _debugState.StartTime = startTime ?? DateTime.Now;
                    _debugState.EndTime = endTime.Value;
                }
                else
                {
                    _debugState.EndTime = startTime ?? DateTime.Now;
                }
                _debugState.HasError = hasError;
                _debugState.ErrorMessage = errorMessage;
                try
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
                catch (Exception e)
                {
                    Dev2Logger.Error("Debug Dispatch Error", e);
                    AddErrorToDataList(e, dataObject);
                    errorMessage = dataObject.Environment.FetchErrors();
                    _debugState.ErrorMessage = errorMessage;
                    _debugState.HasError = true;
                }
            }
            return clearErrors;
        }

        private void DispatchForBeforeState(IDSFDataObject dataObject, StateType stateType, int update, DateTime? startTime, DateTime? endTime, Guid remoteID)
        {
            if (_debugState == null)
            {
                InitializeDebugState(stateType, dataObject, remoteID, false, "", startTime, endTime);
            }
            else
            {
                _debugState.StateType = stateType;
                Dev2Logger.Info("Debug Already Started");
            }

                if (_debugState != null)
                {
                    var type = GetType();
                    var instance = Activator.CreateInstance(type);
                    var activity = instance as Activity;
                    if (activity != null)
                    {
                        _debugState.Name = IsWorkflow ? ActivityType.Workflow.GetDescription() : IsService ? ActivityType.Service.GetDescription() : ActivityType.Step.GetDescription();
                    }
                    var act = instance as DsfActivity;
                    try
                    {
                        var debugInputs = GetDebugInputs(dataObject.Environment, update);
                        Copy(debugInputs, _debugState.Inputs);
                    }
                    catch (Exception err)
                    {
                        Dev2Logger.Error("DispatchDebugState", err);
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

                if (dataObject.RemoteServiceType == "Workflow" && act != null && !_debugState.HasError)
                {
                    var debugItem = new DebugItem();
                    var debugItemResult = new DebugItemResult { Type = DebugItemResultType.Value, Label = "Execute workflow asynchronously: ", Value = dataObject.RunWorkflowAsync ? "True" : "False" };
                    debugItem.Add(debugItemResult);
                    _debugState.Inputs.Add(debugItem);
                }
            }
        }

        private void UpdateDebugWithAssertions(IDSFDataObject dataObject)
        {
            if (dataObject.IsServiceTestExecution)
            {
                var stepToBeAsserted = dataObject.ServiceTest?.TestSteps?.FirstOrDefault(step => step.Type == StepType.Assert && step.UniqueId == Guid.Parse(UniqueID) && step.ActivityType != typeof(DsfForEachActivity).Name && step.ActivityType != typeof(DsfSelectAndApplyActivity).Name && step.ActivityType != typeof(DsfSequenceActivity).Name);
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
                            RunTestResult = RunResult.TestPassed
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
            var testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed);
            var serviceTestFailureMessage = string.Join("", testRunResults.Select(result => result.Message));

            UpdateStepWithFinalResult(dataObject, stepToBeAsserted, testPassed, testRunResults, serviceTestFailureMessage);
        }

        private static void UpdateStepWithFinalResult(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted, bool testPassed, IList<TestRunResult> testRunResults, string serviceTestFailureMessage)
        {
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

        private void SwitchAssertion(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
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
                    var msg = Warewolf.Resource.Messages.Messages.Test_FailureResult;
                    if (assertPassed)
                    {
                        msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                    }
                    var hasError = msg == Warewolf.Resource.Messages.Messages.Test_FailureResult;
                    AddDebugAssertResultItem(new DebugItemServiceTestStaticDataParams(msg, hasError));
                }
                else
                {
                    dataObject.Environment.AddError(Warewolf.Resource.Messages.Messages.Test_FailureResult);
                }
            }
        }

        private void DecisionAssertion(IDSFDataObject dataObject, IServiceTestStep stepToBeAsserted)
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
                    var msg = Warewolf.Resource.Messages.Messages.Test_FailureResult;
                    if (assertPassed)
                    {
                        msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                    }
                    var hasError = msg == Warewolf.Resource.Messages.Messages.Test_FailureResult;
                    AddDebugAssertResultItem(new DebugItemServiceTestStaticDataParams(msg, hasError));
                }
                else
                {
                    dataObject.Environment.AddError(Warewolf.Resource.Messages.Messages.Test_FailureResult);
                }
            }
        }

        protected void UpdateWithAssertions(IDSFDataObject dataObject)
        {
            if (dataObject.IsServiceTestExecution)
            {
                bool assertPassed;
                var serviceTestSteps = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children?? new List<IServiceTestStep>().ToObservableCollection());
                var assertSteps = serviceTestSteps?.Where(step => step.Type == StepType.Assert
                                                                             && step.UniqueId == Guid.Parse(UniqueID)
                                                                             && step.ActivityType != typeof(DsfForEachActivity).Name
                                                                             && step.ActivityType != typeof(DsfSelectAndApplyActivity).Name
                                                                             && step.ActivityType != typeof(DsfSequenceActivity).Name) ?? new List<IServiceTestStep>();
                foreach (var stepToBeAsserted in assertSteps)
                {
                    if (stepToBeAsserted?.StepOutputs != null && stepToBeAsserted.StepOutputs.Count > 0)
                    {

                        if (stepToBeAsserted.Result != null)
                        {
                            stepToBeAsserted.Result.RunTestResult = RunResult.TestPending;
                        }
                        if (stepToBeAsserted.ActivityType == typeof(DsfDecision).Name)
                        {
                            var serviceTestOutput = stepToBeAsserted.StepOutputs[0];

                            if (serviceTestOutput.Result != null)
                            {
                                serviceTestOutput.Result.RunTestResult = RunResult.TestPending;
                            }
                            var dsfDecision = this as DsfDecision;
                            if (dsfDecision != null)
                            {
                                assertPassed = dsfDecision.Result == serviceTestOutput.Value;
                                if (dataObject.ServiceTest != null)
                                {
                                    dataObject.ServiceTest.TestPassed = assertPassed;
                                    dataObject.ServiceTest.TestFailing = !assertPassed;
                                    
                                    SetPassResult(dataObject, assertPassed, serviceTestOutput, stepToBeAsserted);
                                }
                            }
                        }
                        else if (stepToBeAsserted.ActivityType == typeof(DsfSwitch).Name)
                        {
                            var serviceTestOutput = stepToBeAsserted.StepOutputs[0];
                            if (serviceTestOutput.Result != null)
                            {
                                serviceTestOutput.Result.RunTestResult = RunResult.TestPending;
                            }
                            var dsfDecision = this as DsfSwitch;
                            if (dsfDecision != null)
                            {
                                assertPassed = dsfDecision.Result == serviceTestOutput.Value;
                                if (dataObject.ServiceTest != null)
                                {
                                    dataObject.ServiceTest.TestPassed = assertPassed;
                                    dataObject.ServiceTest.TestFailing = !assertPassed;
                                    SetPassResult(dataObject, assertPassed, serviceTestOutput, stepToBeAsserted);
                                }
                            }
                        }
                        else
                        {
                            var factory = Dev2DecisionFactory.Instance();
                            var testRunResults = stepToBeAsserted.StepOutputs.SelectMany(output => GetTestRunResults(dataObject, output, factory)).ToList();
                            var testPassed = testRunResults.All(result => result.RunTestResult == RunResult.TestPassed);
                            var serviceTestFailureMessage = string.Join("", testRunResults.Select(result => result.Message));
                            var finalResult = new TestRunResult();
                            if (testPassed)
                            {
                                finalResult.RunTestResult = RunResult.TestPassed;
                                if(stepToBeAsserted.Result != null)
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
                    }
                }
            }
        }

        private static void SetPassResult(IDSFDataObject dataObject, bool assertPassed, IServiceTestOutput serviceTestOutput, IServiceTestStep stepToBeAsserted)
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
                serviceTestOutput.Result.Message = Warewolf.Resource.Messages.Messages.Test_FailureResult;
                stepToBeAsserted.Result.RunTestResult = RunResult.TestFailed;
                dataObject.Environment.AddError(Warewolf.Resource.Messages.Messages.Test_FailureResult);
            }
            UpdateStepWithFinalResult(dataObject, stepToBeAsserted, assertPassed, new List<TestRunResult> { stepToBeAsserted.Result }, "");
        }

        private IEnumerable<TestRunResult> GetTestRunResults(IDSFDataObject dataObject, IServiceTestOutput output, Dev2DecisionFactory factory)
        {
            if (output == null)
            {
                var testResult = new TestRunResult();
                testResult.RunTestResult = RunResult.TestPassed;
                return new List<TestRunResult> {testResult};
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
            var variable = DataListUtil.AddBracketsToValueIfNotExist(output.Variable);
            var cols1 = dataObject.Environment.EvalAsList(variable, 0);
            var c1 = new WarewolfAtomIterator(cols1);
            var c2 = new WarewolfAtomIterator(value);
            var c3 = new WarewolfAtomIterator(from);
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
                if (decisionType == enDecisionType.IsBetween)
                {
                    val1 = iter.FetchNextValue(c1);
                    val2 = iter.FetchNextValue(c3);
                    val3 = iter.FetchNextValue(c2);
                }
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
                    var actMsg = string.Format(msg, val1, variable, val2, val3);
                    testResult.Message = new StringBuilder(testResult.Message).AppendLine(actMsg).ToString();
                    if (testResult.Message.EndsWith(Environment.NewLine))
                    {
                        testResult.Message = testResult.Message.Replace(Environment.NewLine, "").Replace("\r", "");
                    }
                }
                if (dataObject.IsDebugMode())
                {
                    var msg = testResult.Message;
                    if (testResult.RunTestResult == RunResult.TestPassed)
                    {
                        msg = Warewolf.Resource.Messages.Messages.Test_PassedResult;
                        msg += ": " + output.Variable + " " + output.AssertOp + " " + output.Value;
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
                string errorMessage = string.Empty;
                Guid remoteID;
                Guid.TryParse(dataObject.RemoteInvokerID, out remoteID);
                InitializeDebugState(StateType.Before, dataObject, remoteID, false, errorMessage);
            }
        }

        protected void DispatchDebugStateAndUpdateRemoteServer(IDSFDataObject dataObject, StateType before, int update)
        {
            if (_debugState != null)
            {
                Guid remoteID;
                Guid.TryParse(dataObject.RemoteInvokerID, out remoteID);
                var res = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, remoteID);
                string name = remoteID != Guid.Empty ? res != null ? res.ResourceName : "localhost" : "localhost";
                _debugState.Server = name;
            }
            DispatchDebugState(dataObject, before, update);
        }

        protected void InitializeDebugState(StateType stateType, IDSFDataObject dataObject, Guid remoteID, bool hasError, string errorMessage, DateTime? startTime = null, DateTime? endTime = null)
        {
            Guid parentInstanceID;
            Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceID);
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
                if (resource != null)
                {
                    name = resource.ResourceName;
                }
                else
                {
                    name = remoteID.ToString();
                }
            }
            else
            {
                name = "localhost";
            }

            switch (dataObject.RemoteServiceType)
            {
                case "DbService":
                    IsService = true;
                    break;
                case "PluginService":
                    IsService = true;
                    break;
                case "WebService":
                    IsService = true;
                    break;
            }

            var type = GetType();
            string typeName = type.Name;
            
            _debugState = new DebugState
            {
                ID = Guid.Parse(UniqueID),
                ParentID = parentInstanceID,
                WorkSurfaceMappingId = WorkSurfaceMappingId,
                WorkspaceID = dataObject.WorkspaceID,
                StateType = stateType,
                ActualType = typeName,
                StartTime = startTime ?? DateTime.Now,
                EndTime = endTime ?? DateTime.Now,
                ActivityType = IsWorkflow ? ActivityType.Workflow : ActivityType.Step,
                DisplayName = DisplayName,
                IsSimulation = ShouldExecuteSimulation,
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

            // ReSharper disable ForCanBeConvertedToForeach
            dest.AddRange(src);
        }

        #endregion

        #region DisplayAndWriteError

        protected static void DisplayAndWriteError(string serviceName, ErrorResultTO errors)
        {
            var errorBuilder = new StringBuilder();
            foreach (var e in errors.FetchErrors())
            {
                errorBuilder.AppendLine(string.Format("--[ Execution Exception ]--\r\nService Name = {0}\r\nError Message = {1} \r\n--[ End Execution Exception ]--", serviceName, e));
            }
            Dev2Logger.Error("DsfNativeActivity", new Exception(errorBuilder.ToString()));
        }

        #endregion

        #region GetForEachInputs/Outputs

        public abstract IList<DsfForEachItem> GetForEachInputs();

        public abstract IList<DsfForEachItem> GetForEachOutputs();

        #endregion

        #region GetForEachItems

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

        #endregion

        #region GetDataListItemsForEach

        #endregion

        #region GetFindMissingEnum

        public virtual enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }

        public virtual IDev2Activity Execute(IDSFDataObject data, int update)
        {
            try
            {
                var className = GetType().Name;
                Tracker.TrackEvent(TrackerEventGroup.ActivityExecution, className);
                _debugInputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();
                _assertResultList = new List<DebugItem>();
                ExecuteTool(data, update);
                if (!data.IsDebugMode())
                {
                    UpdateWithAssertions(data);
                }
            }
            catch (Exception ex)
            {
                data.Environment.AddError(ex.Message);
                Dev2Logger.Error("OnExecute", ex);

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

        #endregion

        protected abstract void ExecuteTool(IDSFDataObject dataObject, int update);

        public abstract List<string> GetOutputs();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<IDev2Activity> NextNodes { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Guid ActivityId { get; set; }



        #region Create Debug Item

        protected void AddDebugInputItem(DebugOutputBase parameters)
        {
            IDebugItem itemToAdd = new DebugItem();
            itemToAdd.AddRange(parameters.GetDebugItemResult());
            _debugInputs.Add((DebugItem)itemToAdd);
        }

        protected void AddDebugOutputItem(DebugOutputBase parameters)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.AddRange(parameters.GetDebugItemResult());
            _debugOutputs.Add(itemToAdd);
        }

        protected void AddDebugAssertResultItem(DebugOutputBase parameters)
        {
            DebugItem itemToAdd = new DebugItem();
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
                Dev2Logger.Error(e);
            }
        }

        #endregion

        #region Get Debug State

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public IDebugState GetDebugState()
        {
            return _debugState;
        }

        #endregion

        #region workSurfaceMappingId
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public Guid GetWorkSurfaceMappingId()
        {
            return WorkSurfaceMappingId;
        }
        #endregion

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public virtual IList<IActionableErrorInfo> PerformValidation()
        {
            return new List<IActionableErrorInfo>();
        }

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
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

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((DsfNativeActivity<T>)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return UniqueID?.GetHashCode() ?? 0;
        }

        public static bool operator ==(DsfNativeActivity<T> left, DsfNativeActivity<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DsfNativeActivity<T> left, DsfNativeActivity<T> right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
