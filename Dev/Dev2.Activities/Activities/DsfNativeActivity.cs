using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Simulation;
using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Unlimited.Applications.BusinessDesignStudio.Activities.Hosting;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public abstract class DsfNativeActivity<T> : NativeActivity<T>, IDev2ActivityIOMapping
    {
        ErrorResultTO errors;

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
        public string UniqueID { get; set; }

        protected string InstanceID;
        protected Variable<Guid> DataListExecutionID = new Variable<Guid>();

        readonly IDebugDispatcher _debugDispatcher;
        readonly bool _isExecuteAsync;
        string _previousParentInstanceID;
        IDebugState _debugState;
        bool _isOnDemandSimulation;

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
            InstanceID = Guid.NewGuid().ToString();
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
            _isOnDemandSimulation = false;
            var dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();


            if (dataObject != null && compiler != null)
            {
                compiler.ClearErrors(dataObject.DataListID);

                DataListExecutionID.Set(context, dataObject.DataListID);
            }



            if (dataObject != null)
            {
                _previousParentInstanceID = dataObject.ParentInstanceID;
                _isOnDemandSimulation = dataObject.IsOnDemandSimulation;
            }


            OnBeforeExecute(context);

            //Bug 8918 : Added so that debug only runs when executing in debug - Massimo.Guerrera
            if (dataObject != null && dataObject.IsDebug)
            {
                //DispatchDebugState(context, StateType.Before);
            }
            try
            {
                if (ShouldExecuteSimulation)
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
                if (!_isExecuteAsync || _isOnDemandSimulation)
                {
                    var resumable = dataObject != null && dataObject.WorkflowResumeable;
                    OnExecutedCompleted(context, false, resumable);
                }
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
            if (result != null && result.Value != null)
            {
                var dataListExecutionID = context.GetValue(DataListExecutionID);
                //var compiler = context.GetExtension<IDataListCompiler>();
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

                var dataObject = context.GetExtension<IDSFDataObject>();

                if (compiler != null && dataObject != null)
                {
                    var allErrors = new ErrorResultTO();
                    var dataList = compiler.FetchBinaryDataList(dataObject.DataListID, out errors);
                    allErrors.MergeErrors(errors);

                    compiler.Merge(dataList, result.Value, enDataListMergeTypes.Union, enTranslationDepth.Data, false, out errors);
                    allErrors.MergeErrors(errors);

                    compiler.Shape(dataListExecutionID, enDev2ArgumentType.Output, OutputMapping, out errors);
                    allErrors.MergeErrors(errors);

                    if (allErrors.HasErrors())
                    {
                        DisplayAndWriteError(rootInfo.ProxyName, allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
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

            if (dataListExecutionID == GlobalConstants.NullDataListID)
            {
                dataListExecutionID = dataObject.DataListID;
            }

            try
            {
                //Bug 8918 : Added so that debug only runs when executing in debug - Massimo.Guerrera
                if (dataObject != null && dataObject.IsDebug)
                {
                    DispatchDebugState(context, StateType.After);
                }
            }
            finally
            {
                if (!isResumable && !dataObject.IsDataListScoped)
                {
                    //compiler.ForceDeleteDataListByID(dataListExecutionID);
                    //compiler.DeleteDataListByID(dataListExecutionID);
                }
                else if (dataObject.ForceDeleteAtNextNativeActivityCleanup)
                {
                    // Used for webpages to signal a foce delete after checks of what would become a zombie datalist ;)
                    dataObject.ForceDeleteAtNextNativeActivityCleanup = false; // set back
                    compiler.ForceDeleteDataListByID(dataListExecutionID);
                }

                if (!dataObject.IsDataListScoped)
                {
                    dataObject.ParentInstanceID = _previousParentInstanceID;
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
            //var compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            return compiler.FetchBinaryDataList(dataObject.DataListID, out errors);
        }

        #endregion

        #region GetDebugInputs/Outputs

        public virtual IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        public virtual IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        #endregion

        #region DispatchDebugState

        public void DispatchDebugState(NativeActivityContext context, StateType stateType)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            //var compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            var dataList = compiler.FetchBinaryDataList(dataObject.DataListID, out errors);

            bool hasError = compiler.HasErrors(dataObject.DataListID);

            string errorMessage = String.Empty;
            if (hasError)
            {
                errorMessage = compiler.FetchErrors(dataObject.DataListID);
            }

            if (stateType == StateType.Before)
            {
                // Bug 8918 - _debugState should only ever be set if debug is requested otherwise it should be null 
                _debugState = new DebugState
                {
                    ID = InstanceID,
                    ParentID = dataObject.ParentInstanceID,
                    WorkspaceID = dataObject.WorkspaceID,
                    StateType = stateType,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    ActivityType = IsWorkflow ? ActivityType.Workflow : ActivityType.Step,
                    DisplayName = DisplayName,
                    IsSimulation = ShouldExecuteSimulation,
                    ServerID = dataObject.ServerID,
                    Server = string.Empty,
                    Version = string.Empty,
                    Name = GetType().Name,
                    HasError = hasError,
                    ErrorMessage = errorMessage
                };

                // Bug 8595 - Juries
                var type = GetType();
                var instance = Activator.CreateInstance(type);
                var activity = instance as Activity;
                if (activity != null)
                    _debugState.Name = activity.DisplayName;
                //End Bug 8595

                Copy(GetDebugInputs(dataList), _debugState.Inputs);
            }
            else
            {
                if (_debugState == null)
                {
                    _debugState = new DebugState
                    {
                        ID = InstanceID,
                        ParentID = dataObject.ParentInstanceID,
                        WorkspaceID = dataObject.WorkspaceID,
                        StateType = stateType,
                        StartTime = DateTime.Now,
                        EndTime = DateTime.Now,
                        ActivityType = IsWorkflow ? ActivityType.Workflow : ActivityType.Step,
                        DisplayName = DisplayName,
                        IsSimulation = ShouldExecuteSimulation,
                        ServerID = dataObject.ServerID,
                        Server = string.Empty,
                        Version = string.Empty,
                        Name = GetType().Name,
                        HasError = hasError,
                        ErrorMessage = errorMessage
                    };
                }

                _debugState.StateType = stateType;
                _debugState.EndTime = DateTime.Now;
                _debugState.HasError = hasError;
                _debugState.ErrorMessage = errorMessage;
                Copy(GetDebugOutputs(dataList), _debugState.Outputs);
            }

            if (!(_debugState.ActivityType == ActivityType.Workflow || _debugState.Name == "DsfForEachActivity"))
            {
                _debugState.StateType = StateType.All;

                // Only dispatch 'before state' if it is a workflow or foreach activity
                if (stateType == StateType.Before)
                {
                    return;
                }
            }

            switch (_debugState.StateType)
            {
                case StateType.Before:
                    _debugState.Outputs.Clear();
                    break;
                case StateType.After:
                    _debugState.Inputs.Clear();
                    break;
            }

            _debugDispatcher.Write(_debugState);

            if (stateType == StateType.After)
            {
                // Free up debug state
                _debugState = null;
            }
        }

        static void Copy<TItem>(IList<TItem> src, IList<TItem> dest)
        {
            if (src == null || dest == null)
            {
                return;
            }

            // ReSharper disable ForCanBeConvertedToForeach
            for (var i = 0; i < src.Count; i++)
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
            foreach (var e in errors.FetchErrors())
            {
                TraceWriter.WriteTrace(string.Format("--[ Execution Exception ]--\r\nService Name = {0}\r\nError Message = {1} \r\n--[ End Execution Exception ]--", serviceName, e));
            }
        }

        protected static string DisplayAndWriteError(string serviceName, Exception ex, string dataList)
        {
            var resultObj = new UnlimitedObject();

            try
            {
                resultObj.Add(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(dataList));
                var tmp = DataListUtil.CDATAWrapText(ex.Message);
                resultObj.GetElement("Error").SetValue(tmp);
            }
            catch (XmlException)
            {
                resultObj.GetElement("ADL").SetValue(@"<![CDATA[" + dataList + "]]>");
                resultObj.GetElement("Error").SetValue(@"<![CDATA[" + ex.Message + "]]>");
            }

            TraceWriter.WriteTrace(string.Format("--[ Execution Exception ]--\r\nService Name = {0}\r\nError Message = {1} " + "\r\nStack Trace = {2}\r\nDataList = {3}\r\n--[ End Execution Exception ]--", serviceName, ex.Message, ex.StackTrace, dataList));

            var result = resultObj.XmlString.Replace("&lt;", "<").Replace("&gt;", ">");

            return result;
        }

        #endregion

        #endregion

        #region GetForEachInputs/Outputs

        public virtual IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetDataListItemsForEach(context);
        }

        public virtual IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetDataListItemsForEach(context);
        }

        #endregion

        #region GetForEachItems

        protected IList<DsfForEachItem> GetForEachItems(NativeActivityContext context, StateType stateType, params string[] strings)
        {
            if (strings == null || strings.Length == 0)
            {
                return DsfForEachItem.EmptyList;
            }

            var items = GetDataListItemsForEach(context);
            var result = new List<DsfForEachItem>();

            foreach (var s in strings)
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                if (s.IndexOf("[[", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    if (s.IndexOf("()", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        var exactMatch = !s.EndsWith("()]]");
                        var matchText = exactMatch ? s : s.TrimStart('[').TrimEnd(']');

                        result.AddRange((from item in items
                                         where exactMatch
                                                   ? string.Compare(matchText, item.Name, StringComparison.OrdinalIgnoreCase) == 0
                                                   : item.Name.Contains(matchText) //Need to loop through all fields
                                         select new DsfForEachItem
                                         {
                                             GroupID = item.GroupID,
                                             Name = item.Name.Replace("(", "(" + item.RowIndex),
                                             Value = item.Value,
                                             RowIndex = item.RowIndex
                                         }));
                    }
                    else
                    {
                        result.Add(new DsfForEachItem
                        {
                            Name = s,
                            Value = items.Aggregate(s, (current, item) => current.Replace(item.Name, item.Value))
                        });
                    }
                }
                else
                {
                    result.Add(new DsfForEachItem
                    {
                        Name = string.Empty,
                        Value = s
                    });
                }
            }
            return result;
        }

        #endregion

        #region GetDataListItemsForEach

        static IList<DsfForEachItem> GetDataListItemsForEach(NativeActivityContext context)
        {
            var result = new List<DsfForEachItem>();

            var dataObject = context.GetExtension<IDSFDataObject>();
            //var compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            var dataList = compiler.FetchBinaryDataList(dataObject.DataListID, out errors);
            if (dataList == null)
            {
                return result;
            }

            var groupID = 1;
            foreach (var key in dataList.FetchAllUserKeys())
            {
                string error;
                IBinaryDataListEntry entry;
                if (dataList.TryGetEntry(key, out entry, out error))
                {
                    if (entry.IsRecordset)
                    {
                        var idxItr = entry.FetchRecordsetIndexes();

                        while (idxItr.HasMore())
                        {
                            var index = idxItr.FetchNextIndex();
                            var record = entry.FetchRecordAt(index, out error);
                            // ReSharper disable LoopCanBeConvertedToQuery
                            foreach (var recordField in record)
                            // ReSharper restore LoopCanBeConvertedToQuery
                            {
                                result.Add(new DsfForEachItem
                                {
                                    Name = string.Format("[[{0}().{1}]]", recordField.Namespace, recordField.FieldName),
                                    Value = recordField.TheValue,
                                    RowIndex = index,
                                    GroupID = groupID
                                });
                            }
                            groupID++;
                        }
                    }
                    else
                    {
                        var scalar = entry.FetchScalar();
                        result.Add(new DsfForEachItem
                        {
                            Name = string.Format("[[{0}]]", scalar.FieldName),
                            Value = scalar.TheValue
                        });
                    }
                }
            }
            return result;
        }

        #endregion

        #region Create Debug Item

        public IList<IDebugItemResult> CreateDebugItemsFromEntry(string expression, IBinaryDataListEntry dlEntry, Guid dlId, enDev2ArgumentType argumentType)
        {
            IList<IDebugItemResult> results = new List<IDebugItemResult>();

            if (!string.IsNullOrEmpty(expression))
            {

                if (!(expression.Contains("!~calculation~!") && expression.Contains("!~~calculation~!")))
                {
                    if (!expression.ContainsSafe("[["))
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
                    expression = expression.Replace("!~calculation~!", string.Empty).Replace("!~~calculation~!", string.Empty);
                }

                if (dlEntry.IsRecordset)
                {
                    foreach (var debugItem in CreateRecordsetDebugItems(expression, dlEntry))
                    {
                        results.Add(debugItem);
                    }
                }
                else
                {
                    if (DataListUtil.IsValueRecordset(expression) && (DataListUtil.GetRecordsetIndexType(expression) == enRecordsetIndexType.Blank))
                    {
                        IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                        IBinaryDataList dataList = compiler.FetchBinaryDataList(dlId, out errors);
                        IBinaryDataListEntry tmpEntry;
                        string error;

                        dataList.TryGetEntry(DataListUtil.ExtractRecordsetNameFromValue(expression), out tmpEntry, out error);
                        if (tmpEntry != null)
                        {
                            expression = expression.Replace("().", string.Concat("(", tmpEntry.FetchAppendRecordsetIndex() - 1, ")."));
                        }

                    }
                    IBinaryDataListItem item = dlEntry.FetchScalar();
                    foreach (var debugItem in CreateScalarDebugItems(expression, item.TheValue, dlId))
                    {
                        results.Add(debugItem);
                    }
                }
            }
            return results;
        }

        public IList<IDebugItemResult> CreateDebugItemsFromString(string expression, string value, Guid dlId, int iterationNumber, enDev2ArgumentType argumentType)
        {
            IList<IDebugItemResult> results = new List<IDebugItemResult>();
            ErrorResultTO errors = new ErrorResultTO();
            IList<IDebugItemResult> resultsToPush = new List<IDebugItemResult>();
            if (DataListUtil.IsValueRecordset(expression))
            {
                enRecordsetIndexType recsetIndexType = DataListUtil.GetRecordsetIndexType(expression);
                string recsetName = DataListUtil.ExtractRecordsetNameFromValue(expression);
                IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
                IBinaryDataList dataList = compiler.FetchBinaryDataList(dlId, out errors);
                IBinaryDataListEntry currentRecset;
                string error;
                dataList.TryGetEntry(recsetName, out currentRecset, out error);

                if (recsetIndexType == enRecordsetIndexType.Star)
                {
                    if (currentRecset != null)
                    {
                        resultsToPush = CreateRecordsetDebugItems(expression, currentRecset);
                    }
                }
                else if (recsetIndexType == enRecordsetIndexType.Blank)
                {
                    int recsetIndexToUse = 1;

                    if (currentRecset != null)
                    {
                        if (argumentType == enDev2ArgumentType.Input)
                        {
                            if (!currentRecset.IsEmpty())
                            {
                                recsetIndexToUse = currentRecset.FetchAppendRecordsetIndex() - 1;
                            }
                        }
                        else if (argumentType == enDev2ArgumentType.Output)
                        {
                            if (!currentRecset.IsEmpty())
                            {
                                recsetIndexToUse = currentRecset.FetchAppendRecordsetIndex();
                            }
                        }
                    }
                    recsetIndexToUse = recsetIndexToUse + iterationNumber;
                    expression = expression.Replace("().", string.Concat("(", recsetIndexToUse, ")."));

                    resultsToPush = CreateScalarDebugItems(expression, value, dlId);
                }
                else
                {
                    resultsToPush = CreateScalarDebugItems(expression, value, dlId);
                }
            }
            else
            {
                resultsToPush = CreateScalarDebugItems(expression, value, dlId);
            }

            foreach (var debugItemResult in resultsToPush)
            {
                results.Add(debugItemResult);
            }

            return results;
        }

        private IList<IDebugItemResult> CreateScalarDebugItems(string expression, string value, Guid dlId)
        {
            IList<IDebugItemResult> results = new List<IDebugItemResult>();

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

        private IList<IDebugItemResult> CreateRecordsetDebugItems(string expression, IBinaryDataListEntry dlEntry)
        {
            string initExpression = expression;
            IList<IDebugItemResult> results = new List<IDebugItemResult>();
            var fieldName = DataListUtil.ExtractFieldNameFromValue(expression);
            enRecordsetIndexType indexType = DataListUtil.GetRecordsetIndexType(expression);

            string error;
            if (indexType == enRecordsetIndexType.Blank && string.IsNullOrEmpty(fieldName))
            {
                indexType = enRecordsetIndexType.Star;
            }
            if (indexType == enRecordsetIndexType.Star)
            {
                var idxItr = dlEntry.FetchRecordsetIndexes();
                while (idxItr.HasMore())
                {
                    //string error;
                    var index = idxItr.FetchNextIndex();
                    var record = dlEntry.FetchRecordAt(index, out error);
                    int innerCount = 0;
                    // ReSharper disable LoopCanBeConvertedToQuery
                    foreach (var recordField in record)
                    // ReSharper restore LoopCanBeConvertedToQuery
                    {
                        if (string.IsNullOrEmpty(fieldName) ||
                            recordField.FieldName.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (indexType == enRecordsetIndexType.Star)
                            {
                                string recsetName = DataListUtil.CreateRecordsetDisplayValue(dlEntry.Namespace,
                                                                                             recordField
                                                                                                 .FieldName,
                                                                                             index.ToString(CultureInfo.InvariantCulture));
                                recsetName = DataListUtil.AddBracketsToValueIfNotExist(recsetName);
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = recsetName, GroupName = initExpression, GroupIndex = index });
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = initExpression, GroupIndex = index });
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = recordField.TheValue, GroupName = initExpression, GroupIndex = index });
                            }
                            else
                            {
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Variable, Value = DataListUtil.AddBracketsToValueIfNotExist(recordField.DisplayValue), GroupName = initExpression, GroupIndex = index });
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression, GroupName = initExpression, GroupIndex = index });
                                results.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = recordField.TheValue, GroupName = initExpression, GroupIndex = index });
                                //Add here
                            }
                        }
                        innerCount++;
                    }
                }
            }
            return results;
        }
        #endregion

        #region Get Debug State

        public IDebugState GetDebugState()
        {
            return _debugState;
        }

        #endregion

        #region Private Methods



        #endregion
    }
}
