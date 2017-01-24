/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CyclomaticComplexity
// ReSharper disable FunctionComplexityOverflow

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    public class DsfActivity : DsfActivityAbstract<bool>
    {
        #region Fields
        private InArgument<string> _iconPath = string.Empty;
        string _previousInstanceId;
        private ICollection<IServiceInput> _inputs;
        private ICollection<IServiceOutputMapping> _outputs;
        #endregion

        #region Constructors
        public DsfActivity()
        {
        }

        public DsfActivity(string toolboxFriendlyName, string iconPath, string serviceName, string dataTags, string resultValidationRequiredTags, string resultValidationExpression)
            : base(serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentNullException(nameof(serviceName));
            }
            ToolboxFriendlyName = toolboxFriendlyName;
            IconPath = iconPath;
            ServiceName = serviceName;
            DataTags = dataTags;
            ResultValidationRequiredTags = resultValidationRequiredTags;
            ResultValidationExpression = resultValidationExpression;
            IsService = false;
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

        public bool IsObject { get; set; }

        public string ObjectName { get; set; }

        public string ObjectResult { get; set; }
        protected override bool CanInduceIdle => true;

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

            IEsbChannel esbChannel = DataObject.EsbChannel;
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            dataObject.EnvironmentID = context.GetValue(EnvironmentID);
            Guid datalistId = DataListExecutionID.Get(context);
            ParentWorkflowInstanceId = context.WorkflowInstanceId.ToString();
            dataObject.RemoteServiceType = context.GetValue(Type);
            var resourceId = context.GetValue(ResourceID);
            ErrorResultTO allErrors = new ErrorResultTO();


            ParentServiceName = dataObject.ServiceName;


            string parentServiceName = string.Empty;
            string serviceName = string.Empty;

            var isRemote = dataObject.IsRemoteWorkflow();

            if ((isRemote || dataObject.IsRemoteInvokeOverridden) && dataObject.EnvironmentID == Guid.Empty)
            {
                dataObject.IsRemoteInvokeOverridden = true;
            }

            var oldResourceId = dataObject.ResourceID;

            InitializeDebug(dataObject);

            try
            {
                if (ServiceServer != Guid.Empty)
                {
                    dataObject.RemoteInvokerID = ServiceServer.ToString();
                }


                dataObject.RunWorkflowAsync = RunWorkflowAsync;
                if (resourceId != Guid.Empty)
                {
                    dataObject.ResourceID = resourceId;
                }

                if (dataObject.IsDebugMode() || dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer)
                {
                    DispatchDebugState(dataObject, StateType.Before, 0);
                }



                // scrub it clean ;)

                // set the parent service
                parentServiceName = dataObject.ParentServiceName;
                serviceName = dataObject.ServiceName;
                dataObject.ParentServiceName = serviceName;

                _previousInstanceId = dataObject.ParentInstanceID;
                dataObject.ParentID = oldResourceId;

                dataObject.ParentInstanceID = UniqueID;
                dataObject.ParentWorkflowInstanceId = ParentWorkflowInstanceId;

                if (!DeferExecution)
                {
                    // In all cases the ShapeOutput will have merged the execution data up into the current
                    ErrorResultTO tmpErrors = new ErrorResultTO();

                    if (esbChannel == null)
                    {
                        throw new Exception(ErrorResource.NullESBChannel);
                    }
                    else
                    {
                        // NEW EXECUTION MODEL ;)
                        // PBI 7913
                        if (datalistId != GlobalConstants.NullDataListID)
                        {
                            BeforeExecutionStart(dataObject, allErrors);
                            allErrors.MergeErrors(tmpErrors);

                            dataObject.ServiceName = ServiceName; // set up for sub-exection ;)
                            dataObject.ResourceID = ResourceID.Expression == null ? Guid.Empty : Guid.Parse(ResourceID.Expression.ToString());

                            // Execute Request
                            ExecutionImpl(esbChannel, dataObject, InputMapping, OutputMapping, out tmpErrors, 0); // careful of zero if wf comes back

                            allErrors.MergeErrors(tmpErrors);

                            AfterExecutionCompleted(tmpErrors);
                            allErrors.MergeErrors(tmpErrors);
                            dataObject.DataListID = datalistId; // re-set DL ID
                            dataObject.ServiceName = ServiceName;
                        }


                    }

                    bool whereErrors = dataObject.Environment.HasErrors();

                    SetValues(context, whereErrors);
                }
            }
            finally
            {


                // Handle Errors
                if (allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfActivity", allErrors);
                    dataObject.Environment.AddError(allErrors.MakeDataListReady());
                    // add to datalist in variable specified
                    if (!String.IsNullOrEmpty(OnErrorVariable))
                    {
                        var upsertVariable = DataListUtil.AddBracketsToValueIfNotExist(OnErrorVariable);
                        dataObject.Environment.Assign(upsertVariable, allErrors.MakeDataListReady(), 0);
                    }
                }

                if (dataObject.IsDebugMode() || dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer)
                {
                    DispatchDebugState(dataObject, StateType.After, 0);
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

        void SetValues(NativeActivityContext context, bool whereErrors)
        {
            Result.Set(context, whereErrors);
            HasError.Set(context, whereErrors);
            IsValid.Set(context, whereErrors);
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
        public Guid SourceId { get; set; }
        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                if (value != null)
                {
                    _inputs = value;
                }
            }
        }
        public ICollection<IServiceOutputMapping> Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {
                if (value != null)
                {
                    _outputs = value;
                }
            }
        }

        protected virtual void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {
            var resourceId = ResourceID?.Expression == null ? Guid.Empty : Guid.Parse(ResourceID.Expression.ToString());
            if (resourceId == Guid.Empty || dataObject.ExecutingUser == null)
            {
                return;
            }
            var isAuthorized = AuthorizationService.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, resourceId.ToString());
            if (!isAuthorized)
            {
                var message = $"User: {dataObject.ExecutingUser.Identity.Name} does not have Execute Permission to resource {ServiceName}.";
                tmpErrors.AddError(message);
                dataObject.Environment.AddError(message);
                throw new InvalidCredentialException(message);
            }
        }

        protected virtual void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            if (DataObject != null)
            {
            }
        }

        protected virtual void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            esbChannel.ExecuteSubRequest(dataObject, dataObject.WorkspaceID, inputs, outputs, out tmpErrors, update, !String.IsNullOrEmpty(OnErrorVariable));
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

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            ErrorResultTO allErrors = new ErrorResultTO();

            dataObject.EnvironmentID = EnvironmentID?.Expression == null ? Guid.Empty : Guid.Parse(EnvironmentID.Expression.ToString());
            dataObject.RemoteServiceType = Type.Expression?.ToString() ?? "";
            ParentServiceName = dataObject.ServiceName;


            string parentServiceName = string.Empty;
            string serviceName = string.Empty;
            var isRemote = dataObject.IsRemoteWorkflow();

            if ((isRemote || dataObject.IsRemoteInvokeOverridden) && dataObject.EnvironmentID == Guid.Empty)
            {
                dataObject.IsRemoteInvokeOverridden = true;
            }

            var oldResourceId = dataObject.ResourceID;

            InitializeDebug(dataObject);

            try
            {
                if (ServiceServer != Guid.Empty)
                {
                    dataObject.RemoteInvokerID = ServiceServer.ToString();
                }


                dataObject.RunWorkflowAsync = RunWorkflowAsync;
                Guid resourceId = dataObject.ResourceID;
                if (resourceId != Guid.Empty)
                {
                    dataObject.ResourceID = resourceId;
                }

                parentServiceName = dataObject.ParentServiceName;
                serviceName = dataObject.ServiceName;
                dataObject.ParentServiceName = serviceName;

                _previousInstanceId = dataObject.ParentInstanceID;
                dataObject.ParentID = oldResourceId;

                dataObject.ParentInstanceID = UniqueID;
                dataObject.ParentWorkflowInstanceId = ParentWorkflowInstanceId;

                if (!DeferExecution)
                {
                    ErrorResultTO tmpErrors = new ErrorResultTO();

                    IEsbChannel esbChannel = dataObject.EsbChannel;
                    if (esbChannel == null)
                    {
                        throw new Exception(ErrorResource.NullESBChannel);
                    }


                    dataObject.ServiceName = ServiceName; // set up for sub-exection ;)
                    dataObject.ResourceID = ResourceID.Expression == null ? Guid.Empty : Guid.Parse(ResourceID.Expression.ToString());
                    BeforeExecutionStart(dataObject, allErrors);
                    if (dataObject.IsDebugMode() || dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer)
                    {
                        DispatchDebugStateAndUpdateRemoteServer(dataObject, StateType.Before, update);

                    }
                    allErrors.MergeErrors(tmpErrors);
                    ExecutionImpl(esbChannel, dataObject, InputMapping, OutputMapping, out tmpErrors, update);

                    allErrors.MergeErrors(tmpErrors);

                    AfterExecutionCompleted(tmpErrors);
                    allErrors.MergeErrors(tmpErrors);
                    dataObject.ServiceName = ServiceName;
                }
            }
            catch (Exception err)
            {
                dataObject.Environment.Errors.Add(err.Message);
            }
            finally
            {


                if (!dataObject.WorkflowResumeable || !dataObject.IsDataListScoped)
                {
                    // Handle Errors
                    if (allErrors.HasErrors())
                    {
                        DisplayAndWriteError("DsfActivity", allErrors);
                        foreach (var allError in allErrors.FetchErrors())
                        {
                            dataObject.Environment.Errors.Add(allError);
                        }
                        //dataObject.Environment.AddError(allErrors.MakeDataListReady());
                        // add to datalist in variable specified
                        if (!String.IsNullOrEmpty(OnErrorVariable))
                        {
                            var upsertVariable = DataListUtil.AddBracketsToValueIfNotExist(OnErrorVariable);
                            dataObject.Environment.Assign(upsertVariable, allErrors.MakeDataListReady(), update);
                        }
                    }
                }

                if (dataObject.IsDebugMode() || dataObject.RunWorkflowAsync && !dataObject.IsFromWebServer)
                {
                    var dt = DateTime.Now;
                    DispatchDebugState(dataObject, StateType.After, update, dt);
                    ChildDebugStateDispatch(dataObject);
                    _debugOutputs = new List<DebugItem>();
                    _debugOutputs = new List<DebugItem>();
                    DispatchDebugState(dataObject, StateType.Duration, update, dt);
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

        protected virtual void ChildDebugStateDispatch(IDSFDataObject dataObject)
        {
        }

        public override List<string> GetOutputs()
        {
            if (Outputs == null)
            {
                if (IsObject)
                {
                    return new List<string> { ObjectName };
                }
                IDev2LanguageParser parser = DataListFactory.CreateOutputParser();
                IList<IDev2Definition> outputs = parser.Parse(OutputMapping);
                return outputs.Select(definition => definition.MapsTo).ToList();
            }
            return Outputs.Select(mapping => mapping.MappedTo).ToList();
        }

        #endregion

        #region Private Methods

        #endregion

        #region Overridden ActivityAbstact Methods

        #endregion Overridden ActivityAbstact Methods

        #region Debug IO
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            IDev2LanguageParser parser = DataListFactory.CreateInputParser();
            return GetDebugInputs(env, parser, update).Select(a => (DebugItem)a).ToList();

        }
        public List<IDebugItem> GetDebugInputs(IExecutionEnvironment env, IDev2LanguageParser parser, int update)
        {
            var results = new List<IDebugItem>();
            if (Inputs != null && Inputs.Count > 0)
            {
                foreach (var serviceInput in Inputs)
                {
                    if (string.IsNullOrEmpty(serviceInput.Value))
                    {
                        continue;
                    }
                    var tmpEntry = env.Eval(serviceInput.Value, update);
                    DebugItem itemToAdd = new DebugItem();
                    if (tmpEntry.IsWarewolfAtomResult)
                    {
                        var warewolfAtomResult = tmpEntry as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (warewolfAtomResult != null)
                        {
                            var variableName = serviceInput.Value;
                            if (DataListUtil.IsEvaluated(variableName))
                            {
                                AddDebugItem(new DebugItemWarewolfAtomResult(warewolfAtomResult.Item.ToString(), DataListUtil.AddBracketsToValueIfNotExist(variableName), ""), itemToAdd);
                            }
                            else
                            {
                                AddDebugItem(new DebugItemStaticDataParams(warewolfAtomResult.Item.ToString(), ""), itemToAdd);
                            }
                        }
                        results.Add(itemToAdd);
                    }
                    else
                    {
                        var warewolfAtomListResult = tmpEntry as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        if (warewolfAtomListResult != null)
                        {
                            var variableName = serviceInput.Value;
                            if (DataListUtil.IsValueRecordset(variableName))
                            {
                                //variableName = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, "*");
                                AddDebugItem(new DebugItemWarewolfAtomListResult(warewolfAtomListResult, "", "", DataListUtil.AddBracketsToValueIfNotExist(variableName), "", "", "="), itemToAdd);
                            }
                            else
                            {
                                var warewolfAtom = warewolfAtomListResult.Item.Last();
                                AddDebugItem(new DebugItemWarewolfAtomResult(warewolfAtom.ToString(), DataListUtil.AddBracketsToValueIfNotExist(variableName), ""), itemToAdd);
                            }
                        }
                        results.Add(itemToAdd);
                    }
                }
            }
            else
            {
                IList<IDev2Definition> inputs = parser.Parse(InputMapping);


                foreach (IDev2Definition dev2Definition in inputs)
                {
                    if (string.IsNullOrEmpty(dev2Definition.RawValue))
                    {
                        continue;
                    }
                    var tmpEntry = env.Eval(dev2Definition.RawValue, update);

                    DebugItem itemToAdd = new DebugItem();
                    if (tmpEntry.IsWarewolfAtomResult)
                    {

                        var warewolfAtomResult = tmpEntry as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                        if (warewolfAtomResult != null)
                        {
                            var variableName = dev2Definition.Name;
                            if (!string.IsNullOrEmpty(dev2Definition.RecordSetName))
                            {
                                variableName = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, "1");
                            }
                            AddDebugItem(new DebugItemWarewolfAtomResult(warewolfAtomResult.Item.ToString(), DataListUtil.AddBracketsToValueIfNotExist(variableName), ""), itemToAdd);
                        }
                        results.Add(itemToAdd);
                    }
                    else
                    {

                        var warewolfAtomListResult = tmpEntry as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                        if (warewolfAtomListResult != null)
                        {
                            var variableName = dev2Definition.Name;
                            if (!string.IsNullOrEmpty(dev2Definition.RecordSetName))
                            {
                                variableName = DataListUtil.CreateRecordsetDisplayValue(dev2Definition.RecordSetName, dev2Definition.Name, "*");
                                AddDebugItem(new DebugItemWarewolfAtomListResult(warewolfAtomListResult, "", "", DataListUtil.AddBracketsToValueIfNotExist(variableName), "", "", "="), itemToAdd);
                            }
                            else
                            {
                                var warewolfAtom = warewolfAtomListResult.Item.Last();
                                AddDebugItem(new DebugItemWarewolfAtomResult(warewolfAtom.ToString(), DataListUtil.AddBracketsToValueIfNotExist(variableName), ""), itemToAdd);
                            }
                        }
                        results.Add(itemToAdd);
                    }

                }
            }
            foreach (IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        #region Overrides of DsfNativeActivity<bool>

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            GetDebugOutputsFromEnv(env, update);
            return _debugOutputs;
        }

        #endregion

        public void GetDebugOutputsFromEnv(IExecutionEnvironment environment, int update)
        {
            var results = new List<DebugItem>();
            if (IsObject)
            {
                if (!string.IsNullOrEmpty(ObjectName) && !(this is DsfEnhancedDotNetDllActivity))
                {
                    DebugItem itemToAdd = new DebugItem();
                    AddDebugItem(new DebugEvalResult(ObjectName, "", environment, update), itemToAdd);
                    results.Add(itemToAdd);
                }
            }
            else
            {
                if (Outputs != null && Outputs.Count > 0)
                {
                    foreach (var serviceOutputMapping in Outputs)
                    {
                        try
                        {
                            DebugItem itemToAdd = new DebugItem();
                            AddDebugItem(new DebugEvalResult(serviceOutputMapping.MappedTo, "", environment, update), itemToAdd);
                            results.Add(itemToAdd);
                        }
                        catch (Exception e)
                        {
                            Dev2Logger.Error(e.Message, e);
                        }
                    }
                }
                else
                {
                    IDev2LanguageParser parser = DataListFactory.CreateOutputParser();
                    IList<IDev2Definition> outputs = parser.Parse(OutputMapping);
                    foreach (IDev2Definition dev2Definition in outputs)
                    {
                        try
                        {
                            DebugItem itemToAdd = new DebugItem();
                            AddDebugItem(new DebugEvalResult(dev2Definition.RawValue, "", environment, update), itemToAdd);
                            results.Add(itemToAdd);
                        }
                        catch (Exception e)
                        {
                            Dev2Logger.Error(e.Message, e);
                        }
                    }
                }
            }
            foreach (IDebugItem debugOutput in results)
            {
                debugOutput.FlushStringBuilder();
            }

            _debugOutputs = results;
        }

        #endregion

        #region Get ForEach Input/Output Updates

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
