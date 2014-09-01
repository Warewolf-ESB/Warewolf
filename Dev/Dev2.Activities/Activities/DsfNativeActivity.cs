using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Enums;
using Dev2.Instrumentation;
using Dev2.Runtime.Execution;
using Dev2.Simulation;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Hosting;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public abstract class DsfNativeActivity<T> : NativeActivity<T>, IDev2ActivityIOMapping, IDev2Activity
    {
        protected ErrorResultTO errorsTo;
        // TODO: Remove legacy properties - when we've figured out how to load files when these are not present
        [GeneralSettings("IsSimulationEnabled")]
        public bool IsSimulationEnabled { get; set; }
        // ReSharper disable RedundantAssignment
        public IDSFDataObject DataObject { get { return null; } set { value = null; } }
        // ReSharper restore RedundantAssignment
        public IDataListCompiler Compiler { get; set; }
        // END TODO: Remove legacy properties 

        public InOutArgument<List<string>> AmbientDataList { get; set; }

        // Moved into interface ;)
        public string InputMapping { get; set; }

        public string OutputMapping { get; set; }

        public bool IsWorkflow { get; set; }
        public string ParentServiceName { get; set; }
        public string ParentServiceID { get; set; }
        public string ParentWorkflowInstanceId { get; set; }
        public SimulationMode SimulationMode { get; set; }
        public string ScenarioID { get; set; }
        protected Guid WorkSurfaceMappingId { get; set; }
        /// <summary>
        /// UniqueID is the InstanceID and MUST be a guid.
        /// </summary>
        public string UniqueID { get; set; }

        // PBI 6602 - On Error properties
        [FindMissing]
        public string OnErrorVariable { get; set; }
        [FindMissing]
        public string OnErrorWorkflow { get; set; }
        public bool IsEndedOnError { get; set; }

        protected Variable<Guid> DataListExecutionID = new Variable<Guid>();
        protected List<DebugItem> _debugInputs = new List<DebugItem>();
        protected List<DebugItem> _debugOutputs = new List<DebugItem>();


        internal readonly IDebugDispatcher _debugDispatcher;
        readonly bool _isExecuteAsync;
        string _previousParentInstanceID;
        IDebugState _debugState;
        bool _isOnDemandSimulation;

        //Added for decisions checking errors bug 9704
        ErrorResultTO _tmpErrors = new ErrorResultTO();

        protected IDebugState DebugState { get { return _debugState; } } // protected for testing!

        #region ShouldExecuteSimulation

        bool ShouldExecuteSimulation
        {
            get
            {
                return _isOnDemandSimulation
                    ? SimulationMode != SimulationMode.Never
                    : SimulationMode == SimulationMode.Always;
            }
        }

        #endregion

        #region Ctor

        protected DsfNativeActivity(bool isExecuteAsync, string displayName)
            : this(isExecuteAsync, displayName, DebugDispatcher.Instance)
        {
        }

        protected DsfNativeActivity(bool isExecuteAsync, string displayName, IDebugDispatcher debugDispatcher)
        {
            if(debugDispatcher == null)
            {
                throw new ArgumentNullException("debugDispatcher");
            }

            if(!string.IsNullOrEmpty(displayName))
            {
                DisplayName = displayName;
            }

            _debugDispatcher = debugDispatcher;
            _isExecuteAsync = isExecuteAsync;

            // This will get overwritten when rehydrating
            UniqueID = Guid.NewGuid().ToString();
        }

        #endregion

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
        protected override sealed void Execute(NativeActivityContext context)
        {
            _tmpErrors = new ErrorResultTO();
            _isOnDemandSimulation = false;
            var dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();


            // we need to register this child thread with the DataListRegistar so we can scope correctly ;)
            DataListRegistar.RegisterActivityThreadToParentId(dataObject.ParentThreadID, Thread.CurrentThread.ManagedThreadId);

            if(compiler != null)
            {
                string errorString = compiler.FetchErrors(dataObject.DataListID, true);
                _tmpErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                if(!(this is DsfFlowDecisionActivity))
                {
                    compiler.ClearErrors(dataObject.DataListID);
                }

                DataListExecutionID.Set(context, dataObject.DataListID);
            }

            _previousParentInstanceID = dataObject.ParentInstanceID;
            _isOnDemandSimulation = dataObject.IsOnDemandSimulation;

            OnBeforeExecute(context);

            try
            {
                var className = GetType().Name;
                Tracker.TrackEvent(TrackerEventGroup.ActivityExecution, className);

                if(ShouldExecuteSimulation)
                {
                    OnExecuteSimulation(context);
                }
                else
                {
                    OnExecute(context);
                }
            }
            catch(Exception ex)
            {
                var errorString = ex.Message;
                var errorResultTO = new ErrorResultTO();
                errorResultTO.AddError(errorString);
                if(compiler != null)
                {
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, errorResultTO.MakeDataListReady(), out errorsTo);
                }
            }
            finally
            {
                if(!_isExecuteAsync || _isOnDemandSimulation)
                {
                    var resumable = dataObject.WorkflowResumeable;
                    OnExecutedCompleted(context, false, resumable);
                    if(compiler != null)
                    {
                        DoErrorHandling(context, compiler, dataObject);
                    }
                }
            }
        }

        protected void DoErrorHandling(NativeActivityContext context, IDataListCompiler compiler, IDSFDataObject dataObject)
        {
            string errorString = compiler.FetchErrors(dataObject.DataListID, true);
            string currentError = compiler.FetchErrors(dataObject.DataListID);
            ErrorResultTO _tmpErrorsAfter = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
            _tmpErrors.MergeErrors(_tmpErrorsAfter);
            if(_tmpErrors.HasErrors())
            {
                if(!(this is DsfFlowDecisionActivity))
                {
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, _tmpErrors.MakeDataListReady(), out errorsTo);
                    if(!String.IsNullOrEmpty(currentError))
                    {
                        PerformCustomErrorHandling(context, compiler, dataObject, currentError, _tmpErrors);
                    }
                }
            }
        }

        void PerformCustomErrorHandling(NativeActivityContext context, IDataListCompiler compiler, IDSFDataObject dataObject, string currentError, ErrorResultTO tmpErrors)
        {
            try
            {
                if(!String.IsNullOrEmpty(OnErrorVariable))
                {
                    compiler.Upsert(dataObject.DataListID, OnErrorVariable, currentError, out tmpErrors);
                }
                if(!String.IsNullOrEmpty(OnErrorWorkflow))
                {
                    var esbChannel = context.GetExtension<IEsbChannel>();
                    esbChannel.ExecuteLogErrorRequest(dataObject, dataObject.WorkspaceID, OnErrorWorkflow, out tmpErrors);
                }
            }
            catch(Exception e)
            {
                if(tmpErrors == null)
                {
                    tmpErrors = new ErrorResultTO();
                }
                tmpErrors.AddError(e.Message);
                compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, tmpErrors.MakeDataListReady(), out errorsTo);
            }
            finally
            {
                if(IsEndedOnError)
                {
                    PerformStopWorkflow(context, dataObject);
                }
            }
        }

        void PerformStopWorkflow(NativeActivityContext context, IDSFDataObject dataObject)
        {
            var service = ExecutableServiceRepository.Instance.Get(dataObject.WorkspaceID, dataObject.ResourceID);
            if(service != null)
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
                    Server = string.Empty,
                    Version = string.Empty,
                    SessionID = dataObject.DebugSessionID,
                    EnvironmentID = dataObject.EnvironmentID,
                    Name = GetType().Name,
                    ErrorMessage = "Termination due to error in activity",
                    HasError = true
                };
                DebugDispatcher.Instance.Write(debugState);
                context.MarkCanceled();
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
        protected virtual void OnExecuteSimulation(NativeActivityContext context)
        {
            var rootInfo = context.GetExtension<WorkflowInstanceInfo>();

            var key = new SimulationKey
            {
                WorkflowID = rootInfo.ProxyName,
                ActivityID = UniqueID,
                ScenarioID = ScenarioID
            };
            var result = SimulationRepository.Instance.Get(key);
            if(result != null && result.Value != null)
            {
                var dataListExecutionID = context.GetValue(DataListExecutionID);
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                var dataObject = context.GetExtension<IDSFDataObject>();

                if(compiler != null && dataObject != null)
                {
                    var allErrors = new ErrorResultTO();
                    var dataList = compiler.FetchBinaryDataList(dataObject.DataListID, out errorsTo);
                    allErrors.MergeErrors(errorsTo);

                    compiler.Merge(dataList, result.Value, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errorsTo);
                    allErrors.MergeErrors(errorsTo);

                    compiler.Shape(dataListExecutionID, enDev2ArgumentType.Output, OutputMapping, out errorsTo);
                    allErrors.MergeErrors(errorsTo);

                    if(allErrors.HasErrors())
                    {
                        DisplayAndWriteError(rootInfo.ProxyName, allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errorsTo);
                    }
                }
            }
        }

        #endregion

        #region OnExecutedCompleted

        protected virtual void OnExecutedCompleted(NativeActivityContext context, bool hasError, bool isResumable)
        {
            var dataListExecutionID = DataListExecutionID.Get(context);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var dataObject = context.GetExtension<IDSFDataObject>();

            if(dataObject.ForceDeleteAtNextNativeActivityCleanup)
            {
                // Used for web-pages to signal a force delete after checks of what would become a zombie datalist ;)
                dataObject.ForceDeleteAtNextNativeActivityCleanup = false; // set back
                compiler.ForceDeleteDataListByID(dataListExecutionID);
            }

            if(!dataObject.IsDebugNested)
            {
                dataObject.ParentInstanceID = _previousParentInstanceID;
            }

            dataObject.NumberOfSteps = dataObject.NumberOfSteps + 1;
            //Disposes of all used data lists 

            int threadID = Thread.CurrentThread.ManagedThreadId;

            if(dataObject.IsDebugMode())
            {
                List<Guid> datlistIds;
                if(!dataObject.ThreadsToDispose.TryGetValue(threadID, out datlistIds))
                {
                    dataObject.ThreadsToDispose.Add(threadID, new List<Guid> { dataObject.DataListID });
                }
                else
                {
                    if(!datlistIds.Contains(dataObject.DataListID))
                    {
                        datlistIds.Add(dataObject.DataListID);
                        dataObject.ThreadsToDispose[threadID] = datlistIds;
                    }
                }
            }

        }

        #endregion

        #region ForEach Mapping

        public abstract void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context);

        public abstract void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context);

        #endregion

        #region GetDataList

        protected static IBinaryDataList GetDataList(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            return compiler.FetchBinaryDataList(dataObject.DataListID, out errors);
        }

        #endregion

        #region GetDebugInputs/Outputs

        public virtual List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        public virtual List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        #endregion

        #region DispatchDebugState

        public void DispatchDebugState(NativeActivityContext context, StateType stateType)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            var dataList = compiler.FetchBinaryDataList(dataObject.DataListID, out errorsTo);

            Guid remoteID;
            Guid.TryParse(dataObject.RemoteInvokerID, out remoteID);

            if(stateType == StateType.Before)
            {
                // Bug 8595 - Juries
                var type = GetType();
                var instance = Activator.CreateInstance(type);
                var activity = instance as Activity;
                if(activity != null)
                {
                    _debugState.Name = activity.DisplayName;

                }
                var act = instance as DsfActivity;
                //End Bug 8595
                try
                {
                    Copy(GetDebugInputs(dataList), _debugState.Inputs);
                }
                catch(DebugCopyException err)
                {

                    _debugState.ErrorMessage = err.Message;
                    _debugState.HasError = true;
                    _debugState.Inputs.Add(err.Item);
                }

                if(dataObject.RemoteServiceType == "Workflow" && act != null && !_debugState.HasError)
                {
                    var debugItem = new DebugItem();
                    var debugItemResult = new DebugItemResult { Type = DebugItemResultType.Value, Label = "Execute workflow asynchronously: ", Value = dataObject.RunWorkflowAsync ? "True" : "False" };
                    debugItem.Add(debugItemResult);
                    _debugState.Inputs.Add(debugItem);
                }
            }
            else
            {
                bool hasError = compiler.HasErrors(dataObject.DataListID);

                var errorMessage = String.Empty;
                if(hasError)
                {
                    errorMessage = compiler.FetchErrors(dataObject.DataListID);
                }

                if(_debugState == null)
                {
                    InitializeDebugState(stateType, dataObject, remoteID, hasError, errorMessage);
                }

                if(_debugState != null)
                {
                    _debugState.NumberOfSteps = IsWorkflow ? dataObject.NumberOfSteps : 0;
                    _debugState.StateType = stateType;
                    _debugState.EndTime = DateTime.Now;
                    _debugState.HasError = hasError;
                    _debugState.ErrorMessage = errorMessage;
                    try
                    {
                        if(dataObject.RunWorkflowAsync && !_debugState.HasError)
                        {
                            var debugItem = new DebugItem();
                            var debugItemResult = new DebugItemResult { Type = DebugItemResultType.Value, Value = "Asynchronous execution started" };
                            debugItem.Add(debugItemResult);
                            _debugState.Outputs.Add(debugItem);
                            _debugState.NumberOfSteps = 0;
                        }
                        else
                        {
                            Copy(GetDebugOutputs(dataList), _debugState.Outputs);
                        }
                    }
                    catch(Exception e)
                    {
                        _debugState.ErrorMessage = e.Message;
                        _debugState.HasError = true;
                    }
                }
            }

            if(_debugState != null && (!(_debugState.ActivityType == ActivityType.Workflow || _debugState.Name == "DsfForEachActivity") && remoteID == Guid.Empty))
            {
                _debugState.StateType = StateType.All;

                // Only dispatch 'before state' if it is a workflow or for each activity or a remote activity ;)
                if(stateType == StateType.Before)
                {
                    return;
                }
            }

            // We know that if a if it is not a workflow it must be a service ;)
            if(dataObject.RemoteServiceType != "Workflow" && !String.IsNullOrWhiteSpace(dataObject.RemoteServiceType))
            {
                if(_debugState != null)
                {
                    _debugState.ActivityType = ActivityType.Service;
                }
            }

            if(_debugState != null)
            {
                switch(_debugState.StateType)
                {
                    case StateType.Before:
                        _debugState.Outputs.Clear();
                        break;
                    case StateType.After:
                        _debugState.Inputs.Clear();
                        break;
                }

                // BUG 9706 - 2013.06.22 - TWR : refactored from here to DebugDispatcher
                _debugState.ClientID = dataObject.ClientID;
                _debugDispatcher.Write(_debugState, dataObject.RemoteInvoke, dataObject.RemoteInvokerID, dataObject.ParentInstanceID, dataObject.RemoteDebugItems);

                if(stateType == StateType.After)
                {
                    // Free up debug state
                    _debugState = null;
                }
            }
        }

        protected void InitializeDebug(IDSFDataObject dataObject)
        {
            if(dataObject.IsDebugMode())
            {
                string errorMessage = string.Empty;
                Guid remoteID;
                Guid.TryParse(dataObject.RemoteInvokerID, out remoteID);
                InitializeDebugState(StateType.Before, dataObject, remoteID, false, errorMessage);
            }
        }

        protected void InitializeDebugState(StateType stateType, IDSFDataObject dataObject, Guid remoteID, bool hasError, string errorMessage)
        {
            Guid parentInstanceID;
            Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceID);
            UpdateDebugParentID(dataObject);
            _debugState = new DebugState
            {
                ID = Guid.Parse(UniqueID),
                ParentID = parentInstanceID,
                WorkSurfaceMappingId = WorkSurfaceMappingId,
                WorkspaceID = dataObject.WorkspaceID,
                StateType = stateType,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                ActivityType = IsWorkflow ? ActivityType.Workflow : ActivityType.Step,
                DisplayName = DisplayName,
                IsSimulation = ShouldExecuteSimulation,
                ServerID = dataObject.ServerID,
                OriginatingResourceID = dataObject.ResourceID,
                OriginalInstanceID = dataObject.OriginalInstanceID,
                Server = remoteID.ToString(),
                Version = string.Empty,
                Name = GetType().Name,
                HasError = hasError,
                ErrorMessage = errorMessage,
                EnvironmentID = dataObject.EnvironmentID,
                SessionID = dataObject.DebugSessionID
            };
        }


        public virtual void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
        }
        static void Copy<TItem>(IEnumerable<TItem> src, List<TItem> dest)
        {
            if(src == null || dest == null)
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
            foreach(var e in errors.FetchErrors())
            {
                errorBuilder.AppendLine(string.Format("--[ Execution Exception ]--\r\nService Name = {0}\r\nError Message = {1} \r\n--[ End Execution Exception ]--", serviceName, e));
            }
            ServerLogger.LogError("DsfNativeActivity", new Exception(errorBuilder.ToString()));
        }

        #endregion

        #region GetForEachInputs/Outputs

        public abstract IList<DsfForEachItem> GetForEachInputs();

        public abstract IList<DsfForEachItem> GetForEachOutputs();

        #endregion

        #region GetForEachItems

        protected IList<DsfForEachItem> GetForEachItems(params string[] strings)
        {
            if(strings == null || strings.Length == 0)
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

        #endregion

        #region Create Debug Item

        //The Plan muahahaha
        // DebugController
        //    AddInput
        //    AddOutput
        //    AddItem
        //    GetOutputs
        //    GetInputs
        //    AddError
        //    DispatchDebug

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

        protected void AddDebugItem(DebugOutputBase parameters, DebugItem debugItem)
        {
            var debugItemResults = parameters.GetDebugItemResult();
            debugItem.AddRange(debugItemResults);
        }

        #endregion

        #region Get Debug State

        public IDebugState GetDebugState()
        {
            return _debugState;
        }

        #endregion

        #region workSurfaceMappingId
        public Guid GetWorkSurfaceMappingId()
        {
            return WorkSurfaceMappingId;
        }
        #endregion

    }
}
