using System;
using System.Activities;
using System.Collections.Generic;
using System.Security.Authentication;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.Storage;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using enActionType = Dev2.DataList.Contract.enActionType;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    public class DsfActivity : DsfActivityAbstract<bool>
    {
        #region Fields
        private InArgument<string> _iconPath = string.Empty;
        string _previousInstanceID;
        #endregion

        #region Constructors
        public DsfActivity()
        {
        }

        public DsfActivity(string toolboxFriendlyName, string iconPath, string serviceName, string dataTags, string resultValidationRequiredTags, string resultValidationExpression)
            : base(serviceName)
        {
            if(string.IsNullOrEmpty(serviceName))
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
        // ReSharper disable ConvertToAutoProperty
        public InArgument<string> FriendlySourceName
        // ReSharper restore ConvertToAutoProperty
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

        // ReSharper disable ConvertToAutoProperty
        public InArgument<Guid> EnvironmentID
        // ReSharper restore ConvertToAutoProperty
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

        public InArgument<Guid> ResourceID { get; set; }

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

        public bool RunWorkflowAsync { get; set; }

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
        // ReSharper disable ConvertToAutoProperty
        public string ServiceUri
        // ReSharper restore ConvertToAutoProperty
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

        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }
        // ReSharper restore RedundantOverridenMember

        string _serviceUri;
        InArgument<string> _friendlySourceName;
        InArgument<Guid> _environmentID;
        [NonSerialized]
        IAuthorizationService _authorizationService;

        protected override void OnExecute(NativeActivityContext context)
        {

            // ???? Why is this here....
            context.Properties.ToObservableCollection();

            IEsbChannel esbChannel = context.GetExtension<IEsbChannel>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            ErrorResultTO allErrors = new ErrorResultTO();

            Guid datalistID = DataListExecutionID.Get(context);
            ParentServiceName = dataObject.ServiceName;
            ParentWorkflowInstanceId = context.WorkflowInstanceId.ToString();

            string parentServiceName = string.Empty;
            string serviceName = string.Empty;

            // BUG 9634 - 2013.07.17 - TWR - changed isRemoteExecution to check EnvironmentID instead
            dataObject.EnvironmentID = context.GetValue(EnvironmentID);
            var oldResourceID = dataObject.ResourceID;

            InitializeDebug(dataObject);

            try
            {
                compiler.ClearErrors(dataObject.DataListID);

                if(ServiceServer != Guid.Empty)
                {
                    // we need to adjust the originating server id so debug reflect remote server instead of localhost ;)
                    dataObject.RemoteInvokerID = ServiceServer.ToString();
                }

                dataObject.RemoteServiceType = context.GetValue(Type);
                dataObject.RunWorkflowAsync = RunWorkflowAsync;
                if(dataObject.IsDebugMode() || (dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer))
                {
                    DispatchDebugState(context, StateType.Before);
                }

                var resourceID = context.GetValue(ResourceID);
                if(resourceID != Guid.Empty)
                {
                    dataObject.ResourceID = resourceID;
                }

                // scrub it clean ;)
                ScrubDataList(compiler, datalistID, context.WorkflowInstanceId.ToString(), out errors);
                allErrors.MergeErrors(errors);

                // set the parent service
                parentServiceName = dataObject.ParentServiceName;
                serviceName = dataObject.ServiceName;
                dataObject.ParentServiceName = serviceName;

                _previousInstanceID = dataObject.ParentInstanceID;
                dataObject.ParentID = oldResourceID;
                dataObject.ParentInstanceID = UniqueID;
                dataObject.ParentWorkflowInstanceId = ParentWorkflowInstanceId;

                if(!DeferExecution)
                {
                    // In all cases the ShapeOutput will have merged the execution data up into the current
                    ErrorResultTO tmpErrors = new ErrorResultTO();

                    if(esbChannel == null)
                    {
                        throw new Exception("FATAL ERROR : Null ESB channel!!");
                    }
                    else
                    {
                        // NEW EXECUTION MODEL ;)
                        // PBI 7913
                        if(datalistID != GlobalConstants.NullDataListID)
                        {
                            BeforeExecutionStart(dataObject, allErrors);
                            allErrors.MergeErrors(tmpErrors);

                            dataObject.ServiceName = ServiceName; // set up for sub-exection ;)
                            dataObject.ResourceID = ResourceID.Expression == null ? Guid.Empty : Guid.Parse(ResourceID.Expression.ToString());

                            // Execute Request
                            ExecutionImpl(esbChannel, dataObject, InputMapping, OutputMapping, out tmpErrors);
                            allErrors.MergeErrors(tmpErrors);

                            AfterExecutionCompleted(tmpErrors);
                            allErrors.MergeErrors(tmpErrors);
                            dataObject.DataListID = datalistID; // re-set DL ID
                            dataObject.ServiceName = ServiceName;
                        }

                        // ** THIS IS A HACK OF NOTE, WE NEED TO ADDRESS THIS!
                        if(dataObject.IsDebugMode())
                        {
                            //Dont remove this it is here to fix the data not being returned correctly
                            string testData = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), enTranslationDepth.Data, out errors);
                            if(string.IsNullOrEmpty(testData))
                            {

                            }
                        }

                    }

                    bool whereErrors = compiler.HasErrors(datalistID);

                    Result.Set(context, whereErrors);
                    HasError.Set(context, whereErrors);
                    IsValid.Set(context, whereErrors);

                }
            }
            finally
            {
                dataObject.ResourceID = oldResourceID;

                if(!dataObject.WorkflowResumeable || !dataObject.IsDataListScoped)
                {
                    // Handle Errors
                    if(allErrors.HasErrors())
                    {
                        DisplayAndWriteError("DsfActivity", allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                        // add to datalist in variable specified
                        if(!String.IsNullOrEmpty(OnErrorVariable))
                        {
                            var upsertVariable = DataListUtil.AddBracketsToValueIfNotExist(OnErrorVariable);
                            compiler.Upsert(dataObject.DataListID, upsertVariable, allErrors.MakeDataListReady(), out errors);
                        }
                    }
                }

                if(dataObject.IsDebugMode() || (dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer))
                {
                    DispatchDebugState(context, StateType.After);
                }

                dataObject.ParentInstanceID = _previousInstanceID;
                dataObject.ParentServiceName = parentServiceName;
                dataObject.ServiceName = serviceName;
                dataObject.RemoteInvokeResultShape = string.Empty; // reset targnet shape ;)
                dataObject.RunWorkflowAsync = false;
                compiler.ClearErrors(dataObject.DataListID);
            }
        }

        internal IAuthorizationService AuthorizationService
        {
            get
            {
                return _authorizationService ?? (_authorizationService = ServerAuthorizationService.Instance);
            }
            set
            {
                _authorizationService = value;
            }
        }

        protected virtual void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {
            var resourceID = ResourceID == null || ResourceID.Expression == null ? Guid.Empty : Guid.Parse(ResourceID.Expression.ToString());
            if(resourceID == Guid.Empty || dataObject.ExecutingUser == null)
            {
                return;
            }
            var isAuthorized = AuthorizationService.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, resourceID.ToString());
            if(!isAuthorized)
            {
                var message = string.Format("User: {0} does not have Execute Permission to resource {1}.", dataObject.ExecutingUser.Identity.Name, ServiceName);
                tmpErrors.AddError(message);
                throw new InvalidCredentialException(message);
            }
        }

        protected virtual void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
        }

        protected virtual Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            ServerLogger.LogMessage("PRE-SUB_EXECUTE SHAPE MEMORY USAGE [ " + BinaryDataListStorageLayer.GetUsedMemoryInMB().ToString("####.####") + " MBs ]");

            var resultID = esbChannel.ExecuteSubRequest(dataObject, dataObject.WorkspaceID, inputs, outputs, out tmpErrors);
            ServerLogger.LogMessage("POST-SUB_EXECUTE SHAPE MEMORY USAGE [ " + BinaryDataListStorageLayer.GetUsedMemoryInMB().ToString("####.####") + " MBs ]");

            return resultID;
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.DsfActivity;
        }

        #endregion

        #region Private Methods

        private void ScrubDataList(IDataListCompiler compiler, Guid executionID, string workflowID, out ErrorResultTO invokeErrors)
        {
            ErrorResultTO errors;
            invokeErrors = new ErrorResultTO();
            // Strip System Tags

            compiler.UpsertSystemTag(executionID, enSystemTag.InstanceId, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.Bookmark, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.ParentWorkflowInstanceId, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.ParentServiceName, string.Empty, out errors);
            invokeErrors.MergeErrors(errors);

            compiler.UpsertSystemTag(executionID, enSystemTag.ParentWorkflowInstanceId, workflowID, out errors);
            invokeErrors.MergeErrors(errors);
        }

        #endregion

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetInputs()
        {
            IBinaryDataList result;
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            string inputDlString = compiler.GenerateWizardDataListFromDefs(InputMapping, enDev2ArgumentType.Input, false, out errors, true);
            string inputDlShape = compiler.GenerateWizardDataListFromDefs(InputMapping, enDev2ArgumentType.Input, false, out errors);
            if(!errors.HasErrors())
            {
                Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), inputDlString, inputDlShape, out errors);
                if(!errors.HasErrors())
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
            IBinaryDataList result;
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            string outputDlString = compiler.GenerateWizardDataListFromDefs(OutputMapping, enDev2ArgumentType.Output, false, out errors, true);
            string outputDlShape = compiler.GenerateWizardDataListFromDefs(OutputMapping, enDev2ArgumentType.Output, false, out errors);
            if(!errors.HasErrors())
            {
                Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), outputDlString, outputDlShape, out errors);
                if(!errors.HasErrors())
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

        #endregion Overridden ActivityAbstact Methods

        #region Debug IO
        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IDev2LanguageParser parser = DataListFactory.CreateInputParser();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            return GetDebugInputs(dataList, compiler, parser);

        }
        public List<DebugItem> GetDebugInputs(IBinaryDataList dataList, IDataListCompiler compiler, IDev2LanguageParser parser)
        {
            IList<IDev2Definition> inputs = parser.Parse(InputMapping);

            var results = new List<DebugItem>();
            foreach(IDev2Definition dev2Definition in inputs)
            {
                ErrorResultTO errors;
                IBinaryDataListEntry tmpEntry = compiler.Evaluate(dataList.UID, enActionType.User, dev2Definition.RawValue, false, out errors);

                DebugItem itemToAdd = new DebugItem();
                AddDebugItem(new DebugItemVariableParams(dev2Definition.RawValue, "", tmpEntry, dataList.UID), itemToAdd);

                if(errors.HasErrors())
                {
                    itemToAdd.FlushStringBuilder();
                    throw new DebugCopyException(errors.MakeDisplayReady(), itemToAdd);
                }
                results.Add(itemToAdd);

            }

            foreach(IDebugItem debugInput in results)
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
            foreach(IDev2Definition dev2Definition in inputs)
            {
                ErrorResultTO errors;
                IBinaryDataListEntry tmpEntry = compiler.Evaluate(dataList.UID, enActionType.User, dev2Definition.RawValue, false, out errors);

                if(tmpEntry != null)
                {
                    DebugItem itemToAdd = new DebugItem();
                    AddDebugItem(new DebugItemVariableParams(dev2Definition.RawValue, "", tmpEntry, dataList.UID), itemToAdd);
                    results.Add(itemToAdd);
                }
                else
                {
                    if(errors.HasErrors())
                    {
                        throw new Exception(errors.MakeDisplayReady());
                    }
                }
            }

            foreach(IDebugItem debugOutput in results)
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
