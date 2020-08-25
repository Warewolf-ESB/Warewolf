#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Comparer;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Utils;
using Dev2.Common.State;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    public class DsfActivity : DsfActivityAbstract<bool>,IEquatable<DsfActivity>
    {
        InArgument<string> _iconPath = string.Empty;
        string _previousInstanceId;
        ICollection<IServiceInput> _inputs;
        ICollection<IServiceOutputMapping> _outputs;

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

        public bool IsObject { get; set; }

        public string ObjectName { get; set; }

        public string ObjectResult { get; set; }

        string _serviceUri;
        InArgument<string> _friendlySourceName;

        InArgument<Guid> _environmentID;

        [NonSerialized]
        IAuthorizationService _authorizationService;
        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            UniqueID = dataObject.ForEachNestingLevel > 0 ? Guid.NewGuid().ToString() : UniqueID;
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

            var key = (dataObject.ExecutingUser, AuthorizationContext.Execute, resourceId.ToString());
            var isAuthorized = dataObject.AuthCache.GetOrAdd(key, (requestedKey) => AuthorizationService.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, resourceId));
            if (!isAuthorized)
            {
                
                var message = string.Format(ErrorResource.UserNotAuthorizedToExecuteException, dataObject.ExecutingUser.Identity.Name,ServiceName);
                tmpErrors.AddError(message);
                dataObject.Environment.AddError(message);
                throw new InvalidCredentialException(message);
            }
        }

        protected virtual void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
        }

        protected virtual void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            esbChannel.ExecuteSubRequest(dataObject, dataObject.WorkspaceID, inputs, outputs, out tmpErrors, update, !String.IsNullOrEmpty(OnErrorVariable));
        }

        public override IList<DsfForEachItem> GetForEachInputs() => throw new NotImplementedException();

        public override IList<DsfForEachItem> GetForEachOutputs() => throw new NotImplementedException();

        public override enFindMissingType GetFindMissingType() => enFindMissingType.DsfActivity;

#pragma warning disable S1541 // Methods and properties should not be too complex
        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var allErrors = new ErrorResultTO();

            dataObject.EnvironmentID = EnvironmentID?.Expression == null ? Guid.Empty : Guid.Parse(EnvironmentID.Expression.ToString());
            dataObject.RemoteServiceType = Type?.Expression?.ToString() ?? "";
            ParentServiceName = dataObject.ServiceName;


            var parentServiceName = string.Empty;
            var serviceName = string.Empty;
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

                parentServiceName = dataObject.ParentServiceName;
                serviceName = dataObject.ServiceName;
                dataObject.ParentServiceName = serviceName;

                _previousInstanceId = dataObject.ParentInstanceID;
                dataObject.ParentID = oldResourceId;

                dataObject.ParentInstanceID = UniqueID;
                dataObject.ParentWorkflowInstanceId = ParentWorkflowInstanceId;

                if (!DeferExecution)
                {
                    var tmpErrors = new ErrorResultTO();

                    var esbChannel = dataObject.EsbChannel;
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
                DebugOutput(dataObject, update, allErrors, parentServiceName, serviceName, oldResourceId);
            }

        }

        private void DebugOutput(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, string parentServiceName, string serviceName, Guid oldResourceId)
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
                var parser = DataListFactory.CreateOutputParser();
                var outputs = parser.Parse(OutputMapping);
                return outputs.Select(definition => definition.RawValue).ToList();
            }
            return Outputs.Select(mapping => mapping.MappedTo).ToList();
        }

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            var parser = DataListFactory.CreateInputParser();
            return GetDebugInputs(env, parser, update).Select(a => (DebugItem)a).ToList();

        }
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public List<IDebugItem> GetDebugInputs(IExecutionEnvironment env, IDev2LanguageParser parser, int update)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
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
                    var itemToAdd = new DebugItem();
                    if (tmpEntry.IsWarewolfAtomResult)
                    {
                        AddWarewolfAtomResults(results, serviceInput, tmpEntry, itemToAdd);
                    }
                    else
                    {
                        AddWarewolfAtomListResults(results, serviceInput, tmpEntry, itemToAdd);
                    }
                }
            }
            else
            {
                var inputs = parser.Parse(InputMapping);


                foreach (IDev2Definition dev2Definition in inputs)
                {
                    if (string.IsNullOrEmpty(dev2Definition.RawValue))
                    {
                        continue;
                    }
                    var tmpEntry = env.Eval(dev2Definition.RawValue, update);

                    var itemToAdd = new DebugItem();
                    if (tmpEntry.IsWarewolfAtomResult)
                    {
                        AddWarewolfAtomResults(results, dev2Definition, tmpEntry, itemToAdd);
                    }
                    else
                    {
                        AddWarewolfAtomList(results, dev2Definition, tmpEntry, itemToAdd);
                    }

                }
            }
            foreach (IDebugItem debugInput in results)
            {
                debugInput.FlushStringBuilder();
            }

            return results;
        }

        private void AddWarewolfAtomListResults(List<IDebugItem> results, IServiceInput serviceInput, CommonFunctions.WarewolfEvalResult tmpEntry, DebugItem itemToAdd)
        {
            if (tmpEntry is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListResult)
            {
                var variableName = serviceInput.Value;
                if (DataListUtil.IsValueRecordset(variableName))
                {
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

        private void AddWarewolfAtomResults(List<IDebugItem> results, IServiceInput serviceInput, CommonFunctions.WarewolfEvalResult tmpEntry, DebugItem itemToAdd)
        {
            if (tmpEntry is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfAtomResult)
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

        private void AddWarewolfAtomList(List<IDebugItem> results, IDev2Definition dev2Definition, CommonFunctions.WarewolfEvalResult tmpEntry, DebugItem itemToAdd)
        {
            if (tmpEntry is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult warewolfAtomListResult)
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

        private void AddWarewolfAtomResults(List<IDebugItem> results, IDev2Definition dev2Definition, CommonFunctions.WarewolfEvalResult tmpEntry, DebugItem itemToAdd)
        {
            if (tmpEntry is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult warewolfAtomResult)
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

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            GetDebugOutputsFromEnv(env, update);
            return _debugOutputs;
        }
        public void GetDebugOutputsFromEnv(IExecutionEnvironment environment, int update)
        {
            var results = new List<DebugItem>();
            if (IsObject)
            {
                if (!string.IsNullOrEmpty(ObjectName) && (!(this is DsfEnhancedDotNetDllActivity)) || !(this is DsfEnhancedDotNetDllActivityNew))
                {
                    var itemToAdd = new DebugItem();
                    AddDebugItem(new DebugEvalResult(ObjectName, "", environment, update), itemToAdd);
                    results.Add(itemToAdd);
                }
            }
            else
            {
                if (Outputs != null && Outputs.Count > 0)
                {
                    AddServiceOutputMappingsToResults(environment, update, results);
                }
                else
                {
                    AddRawOutputsToResults(environment, update, results);
                }
            }
            foreach (IDebugItem debugOutput in results)
            {
                debugOutput.FlushStringBuilder();
            }

            _debugOutputs = results;
        }

        void AddRawOutputsToResults(IExecutionEnvironment environment, int update, List<DebugItem> results)
        {
            var parser = DataListFactory.CreateOutputParser();
            var outputs = parser.Parse(OutputMapping);
            foreach (IDev2Definition dev2Definition in outputs)
            {
                try
                {
                    var itemToAdd = new DebugItem();
                    AddDebugItem(new DebugEvalResult(dev2Definition.RawValue, "", environment, update), itemToAdd);
                    results.Add(itemToAdd);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                }
            }
        }

        void AddServiceOutputMappingsToResults(IExecutionEnvironment environment, int update, List<DebugItem> results)
        {
            foreach (var serviceOutputMapping in Outputs)
            {
                try
                {
                    var itemToAdd = new DebugItem();
                    AddDebugItem(new DebugEvalResult(serviceOutputMapping.MappedTo, "", environment, update), itemToAdd);
                    results.Add(itemToAdd);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e.Message, e, GlobalConstants.WarewolfError);
                }
            }
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates) => throw new NotImplementedException();

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates) => throw new NotImplementedException();

      
        public bool Equals(DsfActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var inputsEqual = CommonEqualityOps.CollectionEquals(Inputs, other.Inputs, new ServiceInputComparer());
            var outputsEqual = CommonEqualityOps.CollectionEquals(Outputs, other.Outputs, new ServiceOutputMappingComparer());
            return base.Equals(other)
                && inputsEqual
                && outputsEqual
                && string.Equals(ServiceUri, other.ServiceUri)
                && string.Equals(ServiceName, other.ServiceName)
                && string.Equals(Category, other.Category)
                && RunWorkflowAsync == other.RunWorkflowAsync
                && IsObject == other.IsObject;
        }

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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Inputs != null ? Inputs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Outputs != null ? Outputs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServiceUri != null ? ServiceUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ServiceName != null ? ServiceName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Category != null ? Category.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RunWorkflowAsync.GetHashCode();
                hashCode = (hashCode * 397) ^ IsObject.GetHashCode();
                return hashCode;
            }
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new StateVariable[0];
        }
    }
}
