using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.Execution;
using Dev2.Simulation;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Hosting;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public abstract class DsfNativeActivity<T> : NativeActivity<T>, IDev2ActivityIOMapping, IDev2Activity
    {
        protected ErrorResultTO errors;

        // TODO: Remove legacy properties - when we've figured out how to load files when these are not present
        [GeneralSettings("IsSimulationEnabled")]
        public bool IsSimulationEnabled { get; set; }
        public IDSFDataObject DataObject { get { return null; } set { value = null; } }
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

        /// <summary>
        /// UniqueID is the InstanceID and MUST be a guid.
        /// </summary>
        public string UniqueID { get; set; }

        // PBI 6602 - On Error properties
        [FindMissing]
        public string OnErrorVariable { get; set; }
        public string OnErrorWorkflow { get; set; }
        public bool IsEndedOnError { get; set; }

        protected Variable<Guid> DataListExecutionID = new Variable<Guid>();

        internal readonly IDebugDispatcher _debugDispatcher;
        readonly bool _isExecuteAsync;
        string _previousParentInstanceID;
        IDebugState _debugState;
        bool _isOnDemandSimulation;

        //Added for decisions checking errors bug 9704
        ErrorResultTO _tmpErrors = new ErrorResultTO();

        // I need to cache recordset data to build up later iteations ;)
        private IDictionary<string, string> _rsCachedValues = new Dictionary<string, string>();

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

            if(dataObject != null && compiler != null)
            {
                string errorString = compiler.FetchErrors(dataObject.DataListID, true);
                _tmpErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                if(!(this is DsfFlowDecisionActivity))
                {                    
                    compiler.ClearErrors(dataObject.DataListID);
                }

                DataListExecutionID.Set(context, dataObject.DataListID);
            }

            if(dataObject != null)
            {
                _previousParentInstanceID = dataObject.ParentInstanceID;
                _isOnDemandSimulation = dataObject.IsOnDemandSimulation;
            }


            OnBeforeExecute(context);

            try
            {
                if(ShouldExecuteSimulation)
                {
                    OnExecuteSimulation(context);
                }
                else
                {
                    OnExecute(context);
                }
            }
            finally
            {
                if(!_isExecuteAsync || _isOnDemandSimulation)
                {
                    var resumable = dataObject != null && dataObject.WorkflowResumeable;
                    OnExecutedCompleted(context, false, resumable);
                    string errorString = compiler.FetchErrors(dataObject.DataListID, true);
                    string currentError = compiler.FetchErrors(dataObject.DataListID, false);
                    ErrorResultTO _tmpErrorsAfter = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                    _tmpErrors.MergeErrors(_tmpErrorsAfter);
                    if(_tmpErrors.HasErrors())
                    {
                        if(!(this is DsfFlowDecisionActivity))
                        {
                            if(compiler != null)
                            {
                                if(dataObject != null)
                                {
                                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, _tmpErrors.MakeDataListReady(), out errors);
                                    if(!String.IsNullOrEmpty(currentError))
                                    {
                                        PerformCustomErrorHandling(context, compiler, dataObject, currentError, _tmpErrors);
                                }
                            }
                        }
                    }
                    }

                }
            }
        }

        void PerformCustomErrorHandling(NativeActivityContext context, IDataListCompiler compiler, IDSFDataObject dataObject, string currentError, ErrorResultTO tmpErrors)
        {
            try
            {
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
                compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, tmpErrors.MakeDataListReady(), out errors);
            }
            finally
            {
                if(!String.IsNullOrEmpty(OnErrorVariable))
                {
                    compiler.Upsert(dataObject.DataListID, OnErrorVariable, currentError, out tmpErrors);
                }
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
                    var dataList = compiler.FetchBinaryDataList(dataObject.DataListID, out errors);
                    allErrors.MergeErrors(errors);

                    compiler.Merge(dataList, result.Value, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errors);
                    allErrors.MergeErrors(errors);

                    compiler.Shape(dataListExecutionID, enDev2ArgumentType.Output, OutputMapping, out errors);
                    allErrors.MergeErrors(errors);

                    if(allErrors.HasErrors())
                    {
                        DisplayAndWriteError(rootInfo.ProxyName, allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
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

            if(dataListExecutionID == GlobalConstants.NullDataListID)
            {
                dataListExecutionID = dataObject.DataListID;
            }
            else if(dataObject.ForceDeleteAtNextNativeActivityCleanup)
            {
                // Used for webpages to signal a foce delete after checks of what would become a zombie datalist ;)
                dataObject.ForceDeleteAtNextNativeActivityCleanup = false; // set back
                compiler.ForceDeleteDataListByID(dataListExecutionID);
            }

            if(!dataObject.IsDataListScoped)
            {
                dataObject.ParentInstanceID = _previousParentInstanceID;
            }

            dataObject.NumberOfSteps = dataObject.NumberOfSteps + 1;
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

            var dataList = compiler.FetchBinaryDataList(dataObject.DataListID, out errors);

            bool hasError = false;
            string errorMessage = string.Empty;            

            Guid remoteID;
            Guid.TryParse(dataObject.RemoteInvokerID, out remoteID);

            if(stateType == StateType.Before)
            {
                // Bug 8918 - _debugState should only ever be set if debug is requested otherwise it should be null 
                InitializeDebugState(stateType, dataObject, remoteID, hasError, errorMessage);

                // Bug 8595 - Juries
                var type = GetType();
                var instance = Activator.CreateInstance(type);
                var activity = instance as Activity;
                if(activity != null)
                    _debugState.Name = activity.DisplayName;
                //End Bug 8595

                Copy(GetDebugInputs(dataList), _debugState.Inputs);
            }
            else
            {
                if(!(this is DsfFlowDecisionActivity))
                {
                    hasError = compiler.HasErrors(dataObject.DataListID);

                    errorMessage = String.Empty;
                    if(hasError)
                    {
                        errorMessage = compiler.FetchErrors(dataObject.DataListID);
                    }
                }
                else
                {
                    errorMessage = compiler.FetchErrors(dataObject.DataListID, true);
                    ErrorResultTO fullErrorList = ErrorResultTO.MakeErrorResultFromDataListString(errorMessage);
                    if(fullErrorList.FetchErrors().Count != _tmpErrors.FetchErrors().Count)
                    {
                        hasError = true;
                    }
                    else
                    {
                        errorMessage = compiler.FetchErrors(dataObject.DataListID);
                    }
                }

                if(_debugState == null)
                {
                    InitializeDebugState(stateType, dataObject, remoteID, hasError, errorMessage);
                }

                _debugState.NumberOfSteps = IsWorkflow ? dataObject.NumberOfSteps : 0;
                _debugState.StateType = stateType;
                _debugState.EndTime = DateTime.Now;
                _debugState.HasError = hasError;
                _debugState.ErrorMessage = errorMessage;
                Copy(GetDebugOutputs(dataList), _debugState.Outputs);
            }

            if(!(_debugState.ActivityType == ActivityType.Workflow || _debugState.Name == "DsfForEachActivity") && remoteID == Guid.Empty)
            {
                _debugState.StateType = StateType.All;

                // Only dispatch 'before state' if it is a workflow or foreach activity or a remote activity ;)
                if(stateType == StateType.Before)
                {
                    return;
                }
            }

            // We know that if a if it is not a workflow it must be a service ;)
            if(dataObject.RemoteServiceType != "Workflow" && !String.IsNullOrWhiteSpace(dataObject.RemoteServiceType))
            {
                _debugState.ActivityType = ActivityType.Service;
            }

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
            _debugDispatcher.Write(_debugState, dataObject.RemoteInvoke, dataObject.RemoteInvokerID, dataObject.ParentInstanceID, dataObject.RemoteDebugItems);

            if(stateType == StateType.After)
            {
                // Free up debug state
                _debugState = null;
            }
        }

        protected void InitializeDebugState(StateType stateType, IDSFDataObject dataObject, Guid remoteID, bool hasError, string errorMessage)
        {
            Guid parentInstanceID;
            Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceID);

            _debugState = new DebugState
            {
                ID = Guid.Parse(UniqueID),
                ParentID = parentInstanceID,
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

        static void Copy<TItem>(List<TItem> src, List<TItem> dest)
        {
            if(src == null || dest == null)
            {
                return;
            }

            // ReSharper disable ForCanBeConvertedToForeach
            for(var i = 0; i < src.Count; i++)
            // ReSharper restore ForCanBeConvertedToForeach
            {
                dest.Add(src[i]);
            }
        }

        #endregion

        #region Static Helper methods

        #region DisplayAndWriteError

        protected static void DisplayAndWriteError(string serviceName, ErrorResultTO errors)
        {
            foreach(var e in errors.FetchErrors())
            {
                ServerLogger.LogError(string.Format("--[ Execution Exception ]--\r\nService Name = {0}\r\nError Message = {1} \r\n--[ End Execution Exception ]--", serviceName, e));
            }
        }

        protected static string DisplayAndWriteError(string serviceName, Exception ex, string dataList)
        {
            var resultObj = new UnlimitedObject();

            try
            {
                resultObj.Add(new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(dataList));
                var tmp = DataListUtil.CDATAWrapText(ex.Message);
                resultObj.GetElement("Error").SetValue(tmp);
            }
            catch(XmlException)
            {
                resultObj.GetElement("ADL").SetValue(@"<![CDATA[" + dataList + "]]>");
                resultObj.GetElement("Error").SetValue(@"<![CDATA[" + ex.Message + "]]>");
            }

            ServerLogger.LogError(string.Format("--[ Execution Exception ]--\r\nService Name = {0}\r\nError Message = {1} " + "\r\nStack Trace = {2}\r\nDataList = {3}\r\n--[ End Execution Exception ]--", serviceName, ex.Message, ex.StackTrace, dataList));

            var result = resultObj.XmlString.Replace("&lt;", "<").Replace("&gt;", ">");

            return result;
        }

        #endregion

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

        //Added for BUG 9473 - Massimo Guerrera
        public List<DebugItemResult> CreateDebugItemsFromRecordsetWithIndexAndNoField(string expression, IBinaryDataListEntry dlEntry, Guid dlId, enDev2ArgumentType argumentType, int indexToUse = -1)
        {
            List<DebugItemResult> results = new List<DebugItemResult>();
            string error = string.Empty;
            if(!string.IsNullOrEmpty(expression))
            {
                var rsType = DataListUtil.GetRecordsetIndexType(expression);
                if(dlEntry.IsRecordset
                    && (DataListUtil.IsValueRecordset(expression)
                    && (rsType == enRecordsetIndexType.Numeric)))
                {
                    int rsindex;
                    if(int.TryParse(DataListUtil.ExtractIndexRegionFromRecordset(expression), out rsindex))
                    {
                        IList<IBinaryDataListItem> binaryDataListItems = dlEntry.FetchRecordAt(rsindex, out error);
                        if(binaryDataListItems != null)
                        {
                            int innerCounter = 1;

                            foreach(var binaryDataListItem in binaryDataListItems)
                            {
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = DataListUtil.AddBracketsToValueIfNotExist(binaryDataListItem.DisplayValue), GroupName = expression, GroupIndex = innerCounter });
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = expression, GroupIndex = innerCounter });
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = binaryDataListItem.TheValue, GroupName = expression, GroupIndex = innerCounter });

                                innerCounter++;
                            }
                        }
                    }

                }
            }
            return results;
        }

        public List<DebugItemResult> CreateDebugItemsFromEntry(string expression, IBinaryDataListEntry dlEntry, Guid dlId, enDev2ArgumentType argumentType, int indexToUse = -1)
        {
            List<DebugItemResult> results = new List<DebugItemResult>();

            // We need to break into parts and process each in turn?!


            if(!string.IsNullOrEmpty(expression))
            {

                if(
                    !(expression.Contains(GlobalConstants.CalculateTextConvertPrefix) &&
                      expression.Contains(GlobalConstants.CalculateTextConvertSuffix)))
                {
                    if(!expression.ContainsSafe("[["))
                    {
                        results.Add(new DebugItemResult
                            {
                                Type = DebugItemResultType.Value,
                                Value = expression
                            });
                        return results;
                    }
                }
                else
                {
                    expression =
                        expression.Replace(GlobalConstants.CalculateTextConvertPrefix, string.Empty)
                                  .Replace(GlobalConstants.CalculateTextConvertSuffix, string.Empty);
                }

                // TODO : Fix this to handle using the complex expression junk

                // handle our standard debug output ;)
                if(dlEntry.ComplexExpressionAuditor == null)
                {

                    var rsType = DataListUtil.GetRecordsetIndexType(expression);
                    if(dlEntry.IsRecordset
                        && (DataListUtil.IsValueRecordset(expression)
                            && (rsType == enRecordsetIndexType.Star
                                ||
                                (rsType == enRecordsetIndexType.Blank &&
                                 DataListUtil.ExtractFieldNameFromValue(expression) == string.Empty))))
                    {
                        // Added IsEmpty check for Bug 9263 ;)
                        if(!dlEntry.IsEmpty())
                        {
                            var collection = CreateRecordsetDebugItems(expression, dlEntry, string.Empty, -1);

                            foreach(var debugItem in collection)
                            {
                                results.Add(debugItem);
                            }
                        }
                    }
                    else
                    {
                        if(DataListUtil.IsValueRecordset(expression) &&
                            (DataListUtil.GetRecordsetIndexType(expression) == enRecordsetIndexType.Blank))
                        {
                            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                            IBinaryDataList dataList = compiler.FetchBinaryDataList(dlId, out errors);
                            IBinaryDataListEntry tmpEntry;
                            string error;
                            if(indexToUse == -1)
                            {
                                dataList.TryGetEntry(DataListUtil.ExtractRecordsetNameFromValue(expression),
                                                     out tmpEntry, out error);
                                if(tmpEntry != null)
                                {
                                    expression = expression.Replace("().",
                                                                    string.Concat("(",
                                                                                  tmpEntry.FetchAppendRecordsetIndex() -
                                                                                  1, ")."));
                                }
                            }
                            else
                            {
                                expression = expression.Replace("().", string.Concat("(", indexToUse, ")."));
                            }
                        }
                        IBinaryDataListItem item = dlEntry.FetchScalar();
                        CreateScalarDebugItems(expression, item.TheValue, dlId, results);
                    }
                }
                else
                {
                    // Complex expressions are handled differently ;)
                    var auditor = dlEntry.ComplexExpressionAuditor;

                    int idx = 1;
                    
                    foreach(var item in auditor.FetchAuditItems())
                    {
                        var grpIdx = idx;
                        var groupName = item.RawExpression;
                        var displayExpression = item.RawExpression;
                        if(displayExpression.Contains("()."))
                        {
                            displayExpression = displayExpression.Replace("().", string.Concat("(", auditor.GetMaxIndex(), ")."));
                        }
                        if(displayExpression.Contains("(*)."))
                        {
                            displayExpression = displayExpression.Replace("(*).", string.Concat("(", idx, ")."));
                        }
                        
                        results.Add(new DebugItemResult
                        {
                            Type = DebugItemResultType.Variable,
                            Value = displayExpression,
                            GroupName = groupName,
                            GroupIndex = grpIdx
                        });
                        results.Add(new DebugItemResult
                        {
                            Type = DebugItemResultType.Label,
                            Value = GlobalConstants.EqualsExpression,
                            GroupName = groupName,
                            GroupIndex = grpIdx
                        });
                        results.Add(new DebugItemResult
                        {
                            Type = DebugItemResultType.Value,
                            Value = item.BoundValue,
                            GroupName = groupName,
                            GroupIndex = grpIdx
                        });

                        idx++;

                    }
                }
            }

            return results;
        }

        public IList<DebugItemResult> CreateDebugItemsFromString(string expression, string value, Guid dlId, int iterationNumber, enDev2ArgumentType argumentType)
        {
            var results = new List<DebugItemResult>();
            ErrorResultTO errors = new ErrorResultTO();
            IList<DebugItemResult> resultsToPush = new List<DebugItemResult>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IBinaryDataList dataList = compiler.FetchBinaryDataList(dlId, out errors);
            if(DataListUtil.IsValueRecordset(expression))
            {
                enRecordsetIndexType recsetIndexType = DataListUtil.GetRecordsetIndexType(expression);
                string recsetName = DataListUtil.ExtractRecordsetNameFromValue(expression);             
                IBinaryDataListEntry currentRecset;
                string error;
                dataList.TryGetEntry(recsetName, out currentRecset, out error);

                if(recsetIndexType == enRecordsetIndexType.Star)
                {
                    if(currentRecset != null)
                    {
                        resultsToPush = CreateRecordsetDebugItems(expression, currentRecset, value, iterationNumber);
                    }
                }
                else if(recsetIndexType == enRecordsetIndexType.Blank)
                {
                    int recsetIndexToUse = 1;

                    if(currentRecset != null)
                    {
                        if(argumentType == enDev2ArgumentType.Input)
                        {
                            if(!currentRecset.IsEmpty())
                            {
                                recsetIndexToUse = currentRecset.FetchAppendRecordsetIndex() - 1;
                            }
                        }
                        else if(argumentType == enDev2ArgumentType.Output)
                        {
                            if(!currentRecset.IsEmpty())
                            {
                                recsetIndexToUse = currentRecset.FetchAppendRecordsetIndex();
                            }
                        }
                    }
                    recsetIndexToUse = recsetIndexToUse + iterationNumber;
                    expression = expression.Replace("().", string.Concat("(", recsetIndexToUse, ")."));
                    resultsToPush = string.IsNullOrEmpty(value) ? CreateDebugItemsFromEntry(expression, currentRecset, dlId, argumentType) : CreateScalarDebugItems(expression, value, dlId);
                }
                else
                {
                    resultsToPush = string.IsNullOrEmpty(value) ? CreateDebugItemsFromEntry(expression, currentRecset, dlId, argumentType) : CreateScalarDebugItems(expression, value, dlId);
                }
            }
            else
            {
                IBinaryDataListEntry binaryDataListEntry;
                string error;
                dataList.TryGetEntry(expression, out binaryDataListEntry, out error);
                resultsToPush = string.IsNullOrEmpty(value) ? CreateDebugItemsFromEntry(expression, binaryDataListEntry, dlId, argumentType) : CreateScalarDebugItems(expression, value, dlId);
            }

            foreach(var debugItemResult in resultsToPush)
            {
                results.Add(debugItemResult);
            }

            return results;
        }

        private IList<DebugItemResult> CreateScalarDebugItems(string expression, string value, Guid dlId, IList<DebugItemResult> results = null)
        {
            if(results == null)
            {
                results = new List<DebugItemResult>();
            }

            results.Add(new DebugItemResult
            {
                Type = DebugItemResultType.Variable,
                Value = expression
            });
            results.Add(new DebugItemResult
            {
                Type = DebugItemResultType.Label,
                Value = GlobalConstants.EqualsExpression
            });
            results.Add(new DebugItemResult
            {
                Type = DebugItemResultType.Value,
                Value = value
            });

            return results;
        }

        private IList<DebugItemResult> CreateRecordsetDebugItems(string expression, IBinaryDataListEntry dlEntry, string value, int iterCnt)
        {

            var results = new List<DebugItemResult>();

            if(dlEntry.ComplexExpressionAuditor == null)
            {
                string initExpression = expression;
                
                var fieldName = DataListUtil.ExtractFieldNameFromValue(expression);
                enRecordsetIndexType indexType = DataListUtil.GetRecordsetIndexType(expression);
                string error;
                if(indexType == enRecordsetIndexType.Blank && string.IsNullOrEmpty(fieldName))
                {
                    indexType = enRecordsetIndexType.Star;
                }
                if(indexType == enRecordsetIndexType.Star)
                {
                    var idxItr = dlEntry.FetchRecordsetIndexes();
                    while(idxItr.HasMore())
                    {
                        GetValues(dlEntry, value, iterCnt, idxItr, indexType, results, initExpression, fieldName);

                    }
                }
            }
            else
            {
                // Complex expressions are handled differently ;)
               var auditor = dlEntry.ComplexExpressionAuditor;
               enRecordsetIndexType indexType = DataListUtil.GetRecordsetIndexType(expression);

                foreach(var item in auditor.FetchAuditItems())
               {
                   var grpIdx = -1;

                   try
                   {
                       grpIdx = Int32.Parse(DataListUtil.ExtractIndexRegionFromRecordset(item.TokenBinding));
                   }
                    catch(Exception)
                   {
                       // Best effort ;)
                   }

                    if(indexType == enRecordsetIndexType.Star)
                   {
                       var displayExpression = item.Expression.Replace(item.Token, item.RawExpression);
                       results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = displayExpression, GroupName = displayExpression, GroupIndex = grpIdx });
                       results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = displayExpression, GroupIndex = grpIdx });
                       results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = item.BoundValue, GroupName = displayExpression, GroupIndex = grpIdx });
                   }
               }
            }

            return results;
        }

        void GetValues(IBinaryDataListEntry dlEntry, string value, int iterCnt, IIndexIterator idxItr, enRecordsetIndexType indexType, IList<DebugItemResult> results, string initExpression, string fieldName = null)
        {
            string error;
            var index = idxItr.FetchNextIndex();
            if(string.IsNullOrEmpty(fieldName))
            {
                var record = dlEntry.FetchRecordAt(index, out error);
                // ReSharper disable LoopCanBeConvertedToQuery
                foreach(var recordField in record)
                // ReSharper restore LoopCanBeConvertedToQuery
                {
                    GetValue(dlEntry, value, iterCnt, fieldName, indexType, results, initExpression, recordField, index, false);
                }
            }
            else
            {
                var recordField = dlEntry.TryFetchRecordsetColumnAtIndex(fieldName, index, out error);
                bool ignoreCompare = false;

                if(recordField == null)
                {
                    if(dlEntry.Columns.Count == 1)
                    {
                        recordField = dlEntry.TryFetchIndexedRecordsetUpsertPayload(index, out error);
                        ignoreCompare = true;
                    }  
                }

                GetValue(dlEntry, value, iterCnt, fieldName, indexType, results, initExpression, recordField, index, ignoreCompare);               
            }
        }

        void GetValue(IBinaryDataListEntry dlEntry, string value, int iterCnt, string fieldName, enRecordsetIndexType indexType, IList<DebugItemResult> results, string initExpression, IBinaryDataListItem recordField, int index, bool ignoreCompare)
        {

            if(!ignoreCompare)
            {
                OldGetValue(dlEntry, value, iterCnt, fieldName, indexType, results, initExpression, recordField, index);
            }
            else
            {
                NewGetValue(dlEntry, value, iterCnt, fieldName, indexType, results, initExpression, recordField, index);
            }
           
        }

        /// <summary>
        /// A new version of GetValue since Evaluate will now handle complex expressions it is now possible to create gnarly looking debug items
        /// This method handles these ;)
        /// </summary>
        /// <param name="dlEntry">The dl entry.</param>
        /// <param name="value">The value.</param>
        /// <param name="iterCnt">The iter CNT.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="indexType">Type of the index.</param>
        /// <param name="results">The results.</param>
        /// <param name="initExpression">The init expression.</param>
        /// <param name="recordField">The record field.</param>
        /// <param name="index">The index.</param>
        void NewGetValue(IBinaryDataListEntry dlEntry, string value, int iterCnt, string fieldName, enRecordsetIndexType indexType, IList<DebugItemResult> results, string initExpression, IBinaryDataListItem recordField, int index)
        {

            string injectVal = string.Empty;
            var auditorObj = dlEntry.ComplexExpressionAuditor;

            if(indexType == enRecordsetIndexType.Star && auditorObj != null)
            {
                var auditData = auditorObj.FetchAuditItems();
                if(index <= auditData.Count && index > 0)
                {
                    var useData = auditData[index - 1];
                    var instanceData = useData.TokenBinding;
                    injectVal = useData.BoundValue;

                    results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = instanceData, GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = injectVal, GroupName = initExpression, GroupIndex = index });    
                }
                else
                {
                    string recsetName = DataListUtil.CreateRecordsetDisplayValue(dlEntry.Namespace,
                    recordField.FieldName,
                    index.ToString(CultureInfo.InvariantCulture));
                    recsetName = DataListUtil.AddBracketsToValueIfNotExist(recsetName);
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = recsetName, GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = injectVal, GroupName = initExpression, GroupIndex = index });    
                }
                
            }
            else
            {

                injectVal = recordField.TheValue;

                var displayValue = recordField.DisplayValue;

                if(displayValue.IndexOf(GlobalConstants.NullEntryNamespace, StringComparison.Ordinal) >= 0)
                {
                    displayValue = DataListUtil.CreateRecordsetDisplayValue("Evaluated", GlobalConstants.EvaluationRsField, index.ToString());
                }

                results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = DataListUtil.AddBracketsToValueIfNotExist(displayValue), GroupName = initExpression, GroupIndex = index });
                results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = initExpression, GroupIndex = index });
                results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = injectVal, GroupName = initExpression, GroupIndex = index });
                //Add here
            }

        }

        void OldGetValue(IBinaryDataListEntry dlEntry, string value, int iterCnt, string fieldName, enRecordsetIndexType indexType, IList<DebugItemResult> results, string initExpression, IBinaryDataListItem recordField, int index)
        {
            if((string.IsNullOrEmpty(fieldName) || recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase)))
            {
                string injectVal = recordField.TheValue;
                if(!string.IsNullOrEmpty(value) && recordField.ItemCollectionIndex == (iterCnt + 1))
                {
                    injectVal = value;
                    _rsCachedValues[recordField.DisplayValue] = injectVal;
                }
                else if(string.IsNullOrEmpty(injectVal) && recordField.ItemCollectionIndex != (iterCnt + 1))
                {
                    // is it in the cache? ;)
                    _rsCachedValues.TryGetValue(recordField.DisplayValue, out injectVal);
                    if(injectVal == null)
                    {
                        injectVal = string.Empty;
                    }
                }

                if(indexType == enRecordsetIndexType.Star)
                {
                    string recsetName = DataListUtil.CreateRecordsetDisplayValue(dlEntry.Namespace,
                        recordField.FieldName,
                        index.ToString(CultureInfo.InvariantCulture));
                    recsetName = DataListUtil.AddBracketsToValueIfNotExist(recsetName);
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = recsetName, GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = injectVal, GroupName = initExpression, GroupIndex = index });
                }
                else
                {
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = DataListUtil.AddBracketsToValueIfNotExist(recordField.DisplayValue), GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = initExpression, GroupIndex = index });
                    results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = injectVal, GroupName = initExpression, GroupIndex = index });
                    //Add here
                }
            }
        }

        #endregion

        #region Get Debug State

        public IDebugState GetDebugState()
        {
            return _debugState;
        }

        #endregion
    }
}
