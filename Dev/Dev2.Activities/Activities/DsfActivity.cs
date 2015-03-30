
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Decision;
using Dev2.Data.Storage;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    public class DsfActivity : DsfActivityAbstract<bool>
    {
        #region Fields
        private InArgument<string> _iconPath = string.Empty;
        string _previousInstanceId;
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
        // ReSharper disable InconsistentNaming
        public InArgument<Guid> EnvironmentID
        // ReSharper restore InconsistentNaming
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

        // ReSharper disable InconsistentNaming
        public InArgument<Guid> ResourceID { get; set; }
        // ReSharper restore InconsistentNaming

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
        // ReSharper disable InconsistentNaming
        InArgument<Guid> _environmentID;
        // ReSharper restore InconsistentNaming
        [NonSerialized]
        IAuthorizationService _authorizationService;
        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            UniqueID = dataObject.ForEachNestingLevel > 0 ? Guid.NewGuid().ToString() : UniqueID;
        }
        protected override void OnExecute(NativeActivityContext context)
        {

            // ???? Why is this here....
            context.Properties.ToObservableCollection();

            IEsbChannel esbChannel = context.GetExtension<IEsbChannel>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            ErrorResultTO allErrors = new ErrorResultTO();

            Guid datalistId = DataListExecutionID.Get(context);
            ParentServiceName = dataObject.ServiceName;
            ParentWorkflowInstanceId = context.WorkflowInstanceId.ToString();

            string parentServiceName = string.Empty;
            string serviceName = string.Empty;

            // BUG 9634 - 2013.07.17 - TWR - changed isRemoteExecution to check EnvironmentID instead
            // This is now the wrong behavior - We need to keep the original EnvironmentID when not a remote workflow
            // This is because we put and empty GUID in when designing against a remote server that uses it's resources
            // The first time through this value is set correctly when executing those designed resource from our localhost
            // If we change it as per what was here, we always get a localhost tag instead of the remote host we are design against
            var isRemote = dataObject.IsRemoteWorkflow();
            dataObject.EnvironmentID = context.GetValue(EnvironmentID);
            if((isRemote || dataObject.IsRemoteInvokeOverridden) && dataObject.EnvironmentID == Guid.Empty)
            {
                dataObject.IsRemoteInvokeOverridden = true;
            }

            var oldResourceId = dataObject.ResourceID;

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

                var resourceId = context.GetValue(ResourceID);
                if(resourceId != Guid.Empty)
                {
                    dataObject.ResourceID = resourceId;
                }

                // scrub it clean ;)
                ScrubDataList(compiler, datalistId, context.WorkflowInstanceId.ToString(), out errors);
                allErrors.MergeErrors(errors);

                // set the parent service
                parentServiceName = dataObject.ParentServiceName;
                serviceName = dataObject.ServiceName;
                dataObject.ParentServiceName = serviceName;

                _previousInstanceId = dataObject.ParentInstanceID;
                dataObject.ParentID = oldResourceId;

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
                        if(datalistId != GlobalConstants.NullDataListID)
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
                            dataObject.DataListID = datalistId; // re-set DL ID
                            dataObject.ServiceName = ServiceName;
                        }

                        // ** THIS IS A HACK OF NOTE, WE NEED TO ADDRESS THIS!
                        if(dataObject.IsDebugMode())
                        {
                            //Dont remove this it is here to fix the data not being returned correctly
                            string testData = compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), enTranslationDepth.Data, out errors).ToString();
                            if(string.IsNullOrEmpty(testData))
                            {

                            }
                        }

                    }

                    bool whereErrors = compiler.HasErrors(datalistId);

                    Result.Set(context, whereErrors);
                    HasError.Set(context, whereErrors);
                    IsValid.Set(context, whereErrors);

                }
            }
            finally
            {


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

                dataObject.ParentInstanceID = _previousInstanceId;
                dataObject.ParentServiceName = parentServiceName;
                dataObject.ServiceName = serviceName;
                dataObject.RemoteInvokeResultShape = new StringBuilder(); // reset targnet shape ;)
                dataObject.RunWorkflowAsync = false;
                dataObject.RemoteInvokerID = Guid.Empty.ToString();
                dataObject.EnvironmentID = Guid.Empty;
                dataObject.ResourceID = oldResourceId;
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
            var resourceId = ResourceID == null || ResourceID.Expression == null ? Guid.Empty : Guid.Parse(ResourceID.Expression.ToString());
            if(resourceId == Guid.Empty || dataObject.ExecutingUser == null)
            {
                return;
            }
            var isAuthorized = AuthorizationService.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, resourceId.ToString());
            if(!isAuthorized)
            {
                var message = string.Format("User: {0} does not have Execute Permission to resource {1}.", dataObject.ExecutingUser.Identity.Name, ServiceName);
                tmpErrors.AddError(message);
                throw new InvalidCredentialException(message);
            }
        }

        protected virtual void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(DataObject.DataListID);
        }

        protected virtual Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            Dev2Logger.Log.Info("PRE-SUB_EXECUTE SHAPE MEMORY USAGE [ " + BinaryDataListStorageLayer.GetUsedMemoryInMb().ToString("####.####") + " MBs ]");

            var resultId = esbChannel.ExecuteSubRequest(dataObject, dataObject.WorkspaceID, inputs, outputs, out tmpErrors);
            Dev2Logger.Log.Info("POST-SUB_EXECUTE SHAPE MEMORY USAGE [ " + BinaryDataListStorageLayer.GetUsedMemoryInMb().ToString("####.####") + " MBs ]");
            GetDebugOutputsFromEnv(resultId);
            return Guid.NewGuid();
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

        private void ScrubDataList(IDataListCompiler compiler, Guid executionId, string workflowId, out ErrorResultTO invokeErrors)
        {
            ErrorResultTO errors;
            invokeErrors = new ErrorResultTO();
            // Strip System Tags
            
            compiler.UpsertSystemTag(executionId, enSystemTag.ParentWorkflowInstanceId, workflowId, out errors);
            invokeErrors.MergeErrors(errors);
        }

        #endregion

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetInputs()
        {
            IBinaryDataList result;
            ErrorResultTO errors;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            var inputDlString = compiler.GenerateWizardDataListFromDefs(InputMapping, enDev2ArgumentType.Input, false, out errors, true);
            var inputDlShape = compiler.GenerateWizardDataListFromDefs(InputMapping, enDev2ArgumentType.Input, false, out errors);
            if(!errors.HasErrors())
            {
                Guid dlId = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), inputDlString, inputDlShape, out errors);
                if(!errors.HasErrors())
                {
                    result = compiler.FetchBinaryDataList(dlId, out errors);
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

            var outputDlString = compiler.GenerateWizardDataListFromDefs(OutputMapping, enDev2ArgumentType.Output, false, out errors, true);
            var outputDlShape = compiler.GenerateWizardDataListFromDefs(OutputMapping, enDev2ArgumentType.Output, false, out errors);
            if(!errors.HasErrors())
            {
                Guid dlId = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), outputDlString, outputDlShape, out errors);
                if(!errors.HasErrors())
                {
                    result = compiler.FetchBinaryDataList(dlId, out errors);
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
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env)
        {
            IDev2LanguageParser parser = DataListFactory.CreateInputParser();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            return GetDebugInputs(env, parser).Select(a => (DebugItem)a).ToList();

        }
        public List<IDebugItem> GetDebugInputs( IExecutionEnvironment env, IDev2LanguageParser parser)
        {
            IList<IDev2Definition> inputs = parser.Parse(InputMapping);

            var results = new List<IDebugItem>();
            foreach(IDev2Definition dev2Definition in inputs)
            {
               
                var tmpEntry = env.Eval( dev2Definition.RawValue);

                DebugItem itemToAdd = new DebugItem();
                if (tmpEntry.IsWarewolfAtomResult)
                {
        
                    var warewolfAtomResult = tmpEntry as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                    if (warewolfAtomResult != null)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(warewolfAtomResult.Item), dev2Definition.RawValue, "="), itemToAdd);
                    }
                    results.Add(itemToAdd);
                }
                else
                {
                   
                    var warewolfAtomListResult = tmpEntry as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                    if (warewolfAtomListResult != null)
                    {
                        foreach (var result in warewolfAtomListResult.Item)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(dev2Definition.RawValue, ExecutionEnvironment.WarewolfAtomToString(result), "="), itemToAdd);
                        }

                    }
                    results.Add(itemToAdd);
                }

            

           
                results.Add(itemToAdd);

            }

            foreach(IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        #region Overrides of DsfNativeActivity<bool>

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env)
        {
            GetDebugOutputsFromEnv(env);
            return _debugOutputs;
        }

        #endregion

        public void  GetDebugOutputsFromEnv(IExecutionEnvironment environment)
        {
            IDev2LanguageParser parser = DataListFactory.CreateOutputParser();
            IList<IDev2Definition> inputs = parser.Parse(OutputMapping);


            var results = new List<DebugItem>();
            foreach(IDev2Definition dev2Definition in inputs)
            {
                try
                {
                    var tmpEntry = environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(dev2Definition.Name));
                

                if(tmpEntry.IsWarewolfAtomResult)
                {
                    DebugItem itemToAdd = new DebugItem();
                    var warewolfAtomResult = tmpEntry as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomResult;
                    if(warewolfAtomResult != null)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString( warewolfAtomResult.Item),dev2Definition.RawValue, ""), itemToAdd);
                    }
                    results.Add(itemToAdd);
                }
                else
                {
                    DebugItem itemToAdd = new DebugItem();
                    var warewolfAtomListResult = tmpEntry as WarewolfDataEvaluationCommon.WarewolfEvalResult.WarewolfAtomListresult;
                    if (warewolfAtomListResult != null)
                    {
                        foreach (var result in warewolfAtomListResult.Item)
                        {
                            AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(result), dev2Definition.RawValue, ""), itemToAdd);
                        }
                       
                    }
                    results.Add(itemToAdd);
                }
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(e.Message,e);
                }
            }

            foreach(IDebugItem debugOutput in results)
            {
                debugOutput.FlushStringBuilder();
            }

            _debugOutputs = results;
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
