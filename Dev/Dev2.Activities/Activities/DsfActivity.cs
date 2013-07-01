using Dev2;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Network.Execution;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Util;
using enActionType = Dev2.DataList.Contract.enActionType;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    public class DsfActivity : DsfActivityAbstract<bool>
    {
        #region Const

        private const string _invokeSP = "InvokeStoredProc";

        #endregion

        #region Fields
        //private string uri = "http://localhost:786/dsf/";
        private InArgument<string> _iconPath = string.Empty;
        string _previousInstanceID;
        #endregion

        #region Constructors
        public DsfActivity()
            : base()
        {
        }

        public DsfActivity(string toolboxFriendlyName, string iconPath, string serviceName, string dataTags, string resultValidationRequiredTags, string resultValidationExpression)
            : base(serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException("serviceName");
            }
            ToolboxFriendlyName = toolboxFriendlyName;
            IconPath = iconPath;
            ServiceName = serviceName;
            DataTags = dataTags;
            ResultValidationRequiredTags = resultValidationRequiredTags;
            ResultValidationExpression = resultValidationExpression;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the help link.
        /// </summary>
        /// <value>
        /// The help link.
        /// </value>
        public InArgument<string> HelpLink { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the source.
        /// </summary>
        /// <value>
        /// The friendly name of the source.
        /// </value>
        public InArgument<string> FriendlySourceName
        {
            get
            {
                return _friendlySourceName;
            }
            set
            {
                _friendlySourceName = value;
            }
        }

        public InArgument<Guid> EnvironmentID
        {
            get
            {
                return _environmentID;
            }
            set
            {
                _environmentID = value;
            }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public InArgument<string> Type { get; set; }

        /// <summary>
        /// Gets or sets the action name.
        /// </summary>
        /// <value>
        /// The action name.
        /// </value>
        public InArgument<string> ActionName { get; set; }

        /// <summary>
        /// The Name of Dynamic Service Framework Service that will be invoked
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// The Tags that are required to invoke the Dynamic Service Framework Service
        /// </summary>
        public string DataTags { get; set; }
        /// <summary>
        /// The Tags are are required to be in the result of the service invocation 
        /// in order for the result to be interpreted as valid.
        /// </summary>
        public string ResultValidationRequiredTags { get; set; }
        /// <summary>
        /// The JScript expression that must evaluate to true (boolean) in order for the
        /// result to be interpreted as valid. 
        /// </summary>
        public string ResultValidationExpression { get; set; }
        public string Category { get; set; }
        public string Tags { get; set; }
        public bool DeferExecution { get; set; }
        /// <summary>
        /// Gets or sets the service URI.
        /// </summary>
        /// <value>
        /// The service URI.
        /// </value>
        public string ServiceUri
        {
            get
            {
                return _serviceUri;
            }
            set
            {
                _serviceUri = value;
            }
        }

        /// <summary>
        /// Gets or sets the service server.
        /// </summary>
        /// <value>
        /// The service server.
        /// </value>
        public Guid ServiceServer { get; set; }

        //2012.10.01 : massimo.guerrera - Change for the unlimited migration
        public InArgument<string> IconPath
        {
            get
            {
                return _iconPath;
            }

            set
            {
                _iconPath = value;
                OnPropertyChanged("IconPath");
            }
        }
        public string ToolboxFriendlyName { get; set; }
        public string AuthorRoles { get; set; }
        public string ActivityStateData { get; set; }
        public bool RemoveInputFromOutput { get; set; }


        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Overridden NativeActivity Methods

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        private bool _IsDebug = false;
        string _serviceUri;
        InArgument<string> _friendlySourceName;
        InArgument<Guid> _environmentID;

        protected override void OnExecute(NativeActivityContext context)
        {

            context.Properties.ToObservableCollection(); /// ???? Why is this here....

            bool createResumptionPoint = false;
            IEsbChannel esbChannel = context.GetExtension<IEsbChannel>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            Guid datalistID = DataListExecutionID.Get(context);
            ParentServiceName = dataObject.ServiceName;
            ParentWorkflowInstanceId = context.WorkflowInstanceId.ToString();

            string parentServiceName = string.Empty;
            string serviceName = string.Empty;
            bool isRemoteExecution = false;


            try
            {
                compiler.ClearErrors(dataObject.DataListID);

                if (!string.IsNullOrEmpty(ServiceUri))
                {
                    isRemoteExecution = true;
                }

                // Set Debug Mode Value
                string debugMode = compiler.EvaluateSystemEntry(datalistID, enSystemTag.BDSDebugMode, out errors);
                allErrors.MergeErrors(errors);

                bool.TryParse(debugMode, out _IsDebug);

                if (_IsDebug || dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    if (ServiceServer != Guid.Empty)
                    {
                        // we need to adjust the originating server id so debug reflect remote server instead of localhost ;)
                        dataObject.RemoteInvokerID = ServiceServer.ToString();
                    }

                    dataObject.RemoteServiceType = context.GetValue(Type);

                    DispatchDebugState(context, StateType.Before);
                }

                // scrub it clean ;)
                ScrubDataList(compiler, datalistID, context.WorkflowInstanceId.ToString(), out errors);
                allErrors.MergeErrors(errors);

                // set the parent service
                parentServiceName = dataObject.ParentServiceName;
                serviceName = dataObject.ServiceName;
                dataObject.ParentServiceName = serviceName;

                _previousInstanceID = dataObject.ParentInstanceID;
                //dataObject.ParentServiceName = ServiceName; 
                dataObject.ParentInstanceID = InstanceID;
                dataObject.ParentWorkflowInstanceId = ParentWorkflowInstanceId;

                if (!DeferExecution)
                {
                    // In all cases the ShapeOutput will have merged the execution data up into the current
                    ErrorResultTO tmpErrors = new ErrorResultTO();

                    if (esbChannel == null)
                    {
                        throw new Exception("FATAL ERROR : Null ESB channel!!");
                    }
                    else
                    {
                        // PBI 7913
                        if (datalistID != GlobalConstants.NullDataListID)
                        {
                            // 1) I need to build iterators to loop
                            Dev2ActivityIOIteration inputItr = new Dev2ActivityIOIteration();
                            Dev2ActivityIOIteration outputItr = new Dev2ActivityIOIteration();

                            int iterateTotal = 2;
                            // only iterate if we are not invoking from a for each ;)
                            if (!dataObject.IsDataListScoped)
                            {
                                iterateTotal = FetchMaxIterations(compiler, datalistID, out tmpErrors);
                            }
                            allErrors.MergeErrors(tmpErrors);

                            // save input mapping to restore later
                            string newInputs = InputMapping;
                            string newOutputs = OutputMapping;

                            int iterateIdx = 1;

                            // do we need to flag this as a remote workflow? ;)
                            if (isRemoteExecution)
                            {
                                dataObject.RemoteInvokeUri = ServiceUri;
                                // set remote execution target shape ;)
                                var shape = compiler.ShapeDev2DefinitionsToDataList(OutputMapping, enDev2ArgumentType.Output, false, out errors, true);
                                dataObject.RemoteInvokeResultShape = shape;

                            }

                            // 2) Then I need to manip input mapping to replace (*) with ([[idx]]) and invoke ;)
                            while (iterateIdx < iterateTotal)
                            {
                                // Set proper index ;)
                                string myInputMapping = inputItr.IterateMapping(newInputs, iterateIdx);


                                Guid subExeID = compiler.Shape(datalistID, enDev2ArgumentType.Input, myInputMapping, out tmpErrors);
                                allErrors.MergeErrors(tmpErrors);

                                dataObject.DataListID = subExeID;
                                dataObject.ServiceName = ServiceName; // set up for sub-exection ;)

                                // Execute Request
                                var resultID = esbChannel.ExecuteTransactionallyScopedRequest(dataObject, dataObject.WorkspaceID, out tmpErrors);
                                allErrors.MergeErrors(tmpErrors);

                                //if scoped reuse the same datalist form before execution
                                if (dataObject.IsDataListScoped)
                                {
                                    resultID = subExeID;
                                }

                                if (!dataObject.IsDataListScoped)
                                {
                                    compiler.DeleteDataListByID(resultID); // remove sub service DL  
                                }

                                iterateIdx++;
                            }

                            dataObject.DataListID = datalistID; // re-set DL ID
                            dataObject.ServiceName = ServiceName;
                        }
                    }

                    bool whereErrors = compiler.HasErrors(datalistID);

                    if (!whereErrors)
                    {
                        string entry = compiler.EvaluateSystemEntry(datalistID, enSystemTag.FormView, out errors);
                        allErrors.MergeErrors(errors);

                        if (entry != string.Empty)
                        {
                            createResumptionPoint = true;
                            //compiler.UpsertSystemTag(executionID, enSystemTag.FormView, string.Empty, out errors);
                            allErrors.MergeErrors(errors);
                        }
                    }

                    Result.Set(context, whereErrors);
                    HasError.Set(context, whereErrors);
                    IsValid.Set(context, whereErrors);

                    if ((IsWorkflow || IsUIStep) && createResumptionPoint && !_IsDebug)
                    {
                        dataObject.ServiceName = ServiceName;
                        dataObject.ParentServiceName = ParentServiceName;
                        dataObject.ParentInstanceID = ParentInstanceID.ToString();
                        dataObject.ParentWorkflowInstanceId = ParentWorkflowInstanceId;
                        dataObject.WorkflowInstanceId = context.WorkflowInstanceId.ToString();
                        dataObject.WorkflowResumeable = true;
                        context.CreateBookmark("dsfResumption", Resumed);


                        compiler.ConditionalMerge(DataListMergeFrequency.Always | DataListMergeFrequency.OnBookmark,
                            dataObject.DatalistOutMergeID, dataObject.DataListID, dataObject.DatalistOutMergeFrequency, dataObject.DatalistOutMergeType, dataObject.DatalistOutMergeDepth);
                        ExecutionStatusCallbackDispatcher.Instance.Post(dataObject.BookmarkExecutionCallbackID, ExecutionStatusCallbackMessageType.BookmarkedCallback);

                        // Signal DataList server to persist the data ;)
                        compiler.PersistResumableDataListChain(dataObject.DataListID);

                        // INFO : In these cases resumption handles the delete and shape ;)
                    }
                }
                else
                {
                    // TODO : Build instruction list....????
                }
            }
            finally
            {
                if (!dataObject.WorkflowResumeable || !dataObject.IsDataListScoped)
                {
                    // Handle Errors
                    if (allErrors.HasErrors())
                    {
                        DisplayAndWriteError("DsfBaseActivity", allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                    }
                }

                if (_IsDebug || dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID) || dataObject.RemoteInvoke)
                {
                    DispatchDebugState(context, StateType.After);
                }

                dataObject.ParentInstanceID = _previousInstanceID;
                dataObject.ParentServiceName = parentServiceName;
                dataObject.ServiceName = serviceName;
                dataObject.RemoteInvokeResultShape = string.Empty; // reset targnet shape ;)
                dataObject.RemoteInvokeUri = null; // re-set remote uri
                dataObject.RemoteInvokerID = string.Empty;
                dataObject.RemoteServiceType = string.Empty;

                compiler.ClearErrors(dataObject.DataListID);
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DsfActivity;
        }

        #endregion

        #region Private Methods

        private void ScrubDataList(IDataListCompiler compiler, Guid executionID, string workflowID, out ErrorResultTO invokeErrors)
        {
            ErrorResultTO errors = new ErrorResultTO();
            invokeErrors = new ErrorResultTO();
            // Strip System Tags
            compiler.UpsertSystemTag(executionID, enSystemTag.FormView, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.InstanceId, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.Bookmark, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.ParentWorkflowInstanceId, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.ParentServiceName, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.BDSDebugMode, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.ParentWorkflowInstanceId, workflowID, out errors);
            invokeErrors.MergeErrors(errors);
        }

        /// <summary>
        /// Fetches the max iterations.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="executionID">The execution ID.</param>
        /// <param name="allErrors">All errors.</param>
        /// <returns></returns>
        private int FetchMaxIterations(IDataListCompiler compiler, Guid executionID, out ErrorResultTO allErrors)
        {
            // Break the inputs apart into individual segments for use
            IDev2LanguageParser ilp = DataListFactory.CreateInputParser();
            int itTotal = 1;

            allErrors = new ErrorResultTO();
            ErrorResultTO tmpErrors;

            IList<IDev2Definition> defs = ilp.Parse(InputMapping);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(executionID, out tmpErrors);
            allErrors.MergeErrors(tmpErrors);
            bool foundRS = false;

            foreach (IDev2Definition d in defs)
            {
                if (d.RawValue != null)
                {
                    var idxType = DataListUtil.GetRecordsetIndexType(d.RawValue);
                    if (idxType == enRecordsetIndexType.Star)
                    {
                        string rs = DataListUtil.ExtractRecordsetNameFromValue(d.RawValue);
                        if (!string.IsNullOrEmpty(rs))
                        {
                            // find the total number of entries ;)
                            IBinaryDataListEntry entry;
                            string error;
                            if (bdl.TryGetEntry(rs, out entry, out error))
                            {
                                if (entry != null)
                                {
                                    foundRS = true;
                                    int tmpItrCnt = entry.FetchAppendRecordsetIndex();
                                    // set max iterations ;)
                                    if (tmpItrCnt > itTotal)
                                    {
                                        itTotal = tmpItrCnt;
                                    }
                                }
                                else
                                {
                                    allErrors.AddError("Fatal Error : Null entry returned for [ " + rs + " ]");
                                }
                            }

                            allErrors.AddError(error);
                        }
                    }
                }
            }

            // force all scalars mappings to execute once ;)
            if (!foundRS)
            {
                itTotal = 2;
            }

            return itTotal;
        }

        //private void NotifyApplicationHost(IApplicationMessage messageNotification, string instruction, string result, string transformedResult)
        //{
        //    //Notifications out from this activity for tracking purposes
        //    Notify(messageNotification, string.Format("<{0}>", this.DisplayName.Replace(" ", string.Empty)));

        //    Notify(messageNotification, string.Format("\r\n\t<{0}DSFINSTRUCTION>\r\n ", this.DisplayName.Replace(" ", string.Empty)));
        //    Notify(messageNotification, instruction);
        //    Notify(messageNotification, string.Format("\r\n\t</{0}DSFINSTRUCTION>\r\n", this.DisplayName.Replace(" ", string.Empty)));

        //    Notify(messageNotification, string.Format("\r\n\t<{0}DSFRESULT>\r\n ", this.DisplayName.Replace(" ", string.Empty)));
        //    Notify(messageNotification, string.Format("\r\n{0}\r\n", result));
        //    Notify(messageNotification, string.Format("\r\n\t</{0}DSFRESULT>\r\n", this.DisplayName.Replace(" ", string.Empty)));

        //    Notify(messageNotification, string.Format("\r\n\t<{0}DSFRESULT_TRANSFORMED>\r\n ", this.DisplayName.Replace(" ", string.Empty)));
        //    Notify(messageNotification, string.Format("\r\n{0}\r\n", transformedResult));
        //    Notify(messageNotification, string.Format("\r\n\t</{0}DSFRESULT_TRANSFORMED>\r\n", this.DisplayName.Replace(" ", string.Empty)));

        //    Notify(messageNotification, string.Format("</{0}>\r\n", this.DisplayName.Replace(" ", string.Empty)));
        //}

        #endregion

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetInputs()
        {
            IBinaryDataList result = null;
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            string inputDlString = compiler.GenerateWizardDataListFromDefs(InputMapping, enDev2ArgumentType.Input, false, out errors, true);
            string inputDlShape = compiler.GenerateWizardDataListFromDefs(InputMapping, enDev2ArgumentType.Input, false, out errors);
            if (!errors.HasErrors())
            {
                Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), inputDlString, inputDlShape, out errors);
                if (!errors.HasErrors())
                {
                    result = compiler.FetchBinaryDataList(dlID, out errors);
                }
                else
                {
                    string errorString = string.Join(",", errors.FetchErrors());
                    throw new Exception(errorString);
                }
            }
            else
            {
                string errorString = string.Join(",", errors.FetchErrors());
                throw new Exception(errorString);
            }

            return result;
        }

        public override IBinaryDataList GetOutputs()
        {
            IBinaryDataList result = null;
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            string outputDlString = compiler.GenerateWizardDataListFromDefs(OutputMapping, enDev2ArgumentType.Output, false, out errors, true);
            string outputDlShape = compiler.GenerateWizardDataListFromDefs(OutputMapping, enDev2ArgumentType.Output, false, out errors);
            if (!errors.HasErrors())
            {
                Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), outputDlString, outputDlShape, out errors);
                if (!errors.HasErrors())
                {
                    result = compiler.FetchBinaryDataList(dlID, out errors);
                }
                else
                {
                    string errorString = string.Join(",", errors.FetchErrors());
                    throw new Exception(errorString);
                }
            }
            else
            {
                string errorString = string.Join(",", errors.FetchErrors());
                throw new Exception(errorString);
            }

            return result;
        }

        public override IBinaryDataList GetWizardData()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            IBinaryDataList indl = GetInputs();
            IBinaryDataList outdl = GetOutputs();
            IBinaryDataList result = compiler.Merge(indl, outdl, enDataListMergeTypes.Union, enTranslationDepth.Data, true, out errors);

            return result;
        }

        #endregion Overridden ActivityAbstact Methods

        #region Debug IO
        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IDev2LanguageParser parser = DataListFactory.CreateInputParser();
            IList<IDev2Definition> inputs = parser.Parse(InputMapping);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            var results = new List<DebugItem>();
            foreach (IDev2Definition dev2Definition in inputs)
            {
                string displayName = dev2Definition.Name;
                if (!string.IsNullOrEmpty(dev2Definition.RecordSetName))
                {
                    displayName = dev2Definition.RecordSetName + "(*)." + dev2Definition.Name;
                }
                ErrorResultTO errors = new ErrorResultTO();
                IBinaryDataListEntry tmpEntry = compiler.Evaluate(dataList.UID, enActionType.User, dev2Definition.RawValue, false, out errors);

                DebugItem itemToAdd = new DebugItem();

                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = displayName });

                itemToAdd.AddRange(CreateDebugItemsFromEntry(dev2Definition.RawValue, tmpEntry, dataList.UID, enDev2ArgumentType.Input));
                results.Add(itemToAdd);
            }

            foreach (IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IDev2LanguageParser parser = DataListFactory.CreateOutputParser();
            IList<IDev2Definition> inputs = parser.Parse(OutputMapping);
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            var results = new List<DebugItem>();
            foreach (IDev2Definition dev2Definition in inputs)
            {
                ErrorResultTO errors = new ErrorResultTO();
                IBinaryDataListEntry tmpEntry = compiler.Evaluate(dataList.UID, enActionType.User, dev2Definition.RawValue, false, out errors);

                DebugItem itemToAdd = new DebugItem();

                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = dev2Definition.Name });

                itemToAdd.AddRange(CreateDebugItemsFromEntry(dev2Definition.RawValue, tmpEntry, dataList.UID, enDev2ArgumentType.Output));
                results.Add(itemToAdd);
            }

            foreach (IDebugItem debugOutput in results)
            {
                debugOutput.FlushStringBuilder();
            }

            return results;
        }

        #endregion

        #region Get ForEach Input/Output Updates

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
