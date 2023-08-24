/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Dev2.Runtime.Subscription;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ServiceModel.Data;
using ServiceStack.Common.Extensions;
using Warewolf.Auditing;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using System.Runtime.Serialization;

namespace Dev2.Runtime.ESB.Execution
{
    public abstract class WfExecutionContainerBase : EsbExecutionContainer
    {
        protected readonly IResourceCatalog _resourceCatalog;

        protected WfExecutionContainerBase(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace,
            IEsbChannel esbChannel)
            : this(sa, dataObj, theWorkspace, esbChannel, ResourceCatalog.Instance)
        {
        }

        protected WfExecutionContainerBase(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace,
            IEsbChannel esbChannel, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _resourceCatalog = resourceCatalog;
            DataObject.Settings = GetWorkflowSetting();
        }

        /// <summary>
        /// Execute using workflow and parameters as defined in DataObject
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="update"></param>
        /// <returns>Resulting DataListId</returns>
        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            SetDataObjectProperties();

            var user = DataObject.ExecutingUser ?? Thread.CurrentPrincipal;
            var dataObjectExecutionId = DataObject.ExecutionID.ToString();
            if (!DataObject.IsSubExecution)
            {
                var userIdentity = user?.Identity;
                Dev2Logger.Debug(
                    string.Format(GlobalConstants.ExecuteWebRequestString, DataObject.ServiceName, userIdentity?.Name,
                        userIdentity?.AuthenticationType, userIdentity?.IsAuthenticated, DataObject.RawPayload),
                    dataObjectExecutionId);
                Dev2Logger.Debug("Request URL [ " + DataObject.WebUrl + " ]", dataObjectExecutionId);
            }

            Dev2Logger.Debug("Entered Wf Container", dataObjectExecutionId);
            DataObject.ServiceName = ServiceAction.ServiceName;

            var executionForServiceString = string.Format(GlobalConstants.ExecutionForServiceString,
                DataObject.ServiceName, DataObject.ResourceID, (DataObject.IsDebug ? "Debug" : $"Execute"));
            Dev2Logger.Info("Started " + executionForServiceString, dataObjectExecutionId);
            SetExecutionOrigin();

            var result = GlobalConstants.NullDataListID;
            Common.Utilities.PerformActionInsideImpersonatedContext(user, () => { result = ExecuteWf(); });

            errors = AddErrors();

            var executionTypeString = DataObject.IsSubExecution ? "Completed Sub " : "Completed ";
            Dev2Logger.Info(executionTypeString + executionForServiceString, dataObjectExecutionId);
            return result;
        }

        Guid ExecuteWf()
        {
            var result = Guid.NewGuid();
            DataObject.StartTime = DateTime.Now;
            var wfappUtils = new WfApplicationUtils(_resourceCatalog);
            var executionId = DataObject.ExecutionID.ToString();

            try
            {
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                DataObject.ExecutionToken = exeToken;
                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.Start, out _, true, false, false);
                }

                var resourceId = DataObject.ResourceID;
                if (CanExecute(resourceId, DataObject, AuthorizationContext.Execute))
                {
                    Eval(resourceId, DataObject);
                }

                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.End, out _);
                }

                result = DataObject.DataListID;
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Error(iwe, executionId);
                var msg = iwe.Message;

                var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                var errorMessage = start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message;
                DataObject.Environment.AddError(errorMessage);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out _);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, executionId);
                DataObject.Environment.AddError(ex.Message);
                DataObject.ExecutionException = ex;
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out _);
            }

            return result;
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject,
            AuthorizationContext authorizationContext)
        {
            var key = (dataObject.ExecutingUser, AuthorizationContext.Execute, resourceId.ToString());
            var isAuthorized = dataObject.AuthCache.GetOrAdd(key,
                (requestedKey) => ServerAuthorizationService.Instance.IsAuthorized(dataObject.ExecutingUser,
                    AuthorizationContext.Execute, dataObject.Resource));
            if (!isAuthorized)
            {
                dataObject.Environment.AddError(string.Format(ErrorResource.UserNotAuthorizedToExecuteException,
                    dataObject.ExecutingUser?.Identity.Name, dataObject.ServiceName));
            }

            return isAuthorized;
        }

        public void Eval(DynamicActivity flowchartProcess, IDSFDataObject dsfDataObject, int update)
        {
            var resource = CustomContainer.Get<IActivityParser>()?.Parse(flowchartProcess) ??
                           new ActivityParser().Parse(flowchartProcess);
            EvalInner(dsfDataObject, resource, update);
        }

        protected abstract void Eval(Guid resourceID, IDSFDataObject dataObject);
        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;

        protected abstract void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update);

        void SetDataObjectProperties()
        {
            DataObject.ExecutionID = DataObject.ExecutionID ?? Guid.NewGuid();
            if (string.IsNullOrEmpty(DataObject.WebUrl))
            {
                DataObject.WebUrl =
                    $"{EnvironmentVariables.WebServerUri}secure/{DataObject.ServiceName}.{DataObject.ReturnType}?" +
                    DataObject.QueryString;
            }

            if (DataObject.ServerID == Guid.Empty)
            {
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;
            }
        }

        ErrorResultTO AddErrors()
        {
            var errors = new ErrorResultTO();
            var errorsList = DataObject.Environment.Errors.ToList();
            foreach (var err in errorsList)
            {
                errors.AddError(err, true);
            }

            var allErrorsList = DataObject.Environment.AllErrors.ToList();
            foreach (var err in allErrorsList)
            {
                errors.AddError(err, true);
            }

            return errors;
        }

        void SetExecutionOrigin()
        {
            if (!string.IsNullOrWhiteSpace(DataObject.ParentServiceName))
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Workflow;
                DataObject.ExecutionOriginDescription = DataObject.ParentServiceName;
            }
            else if (DataObject.IsDebug)
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.Debug;
            }
            else
            {
                DataObject.ExecutionOrigin = ExecutionOrigin.External;
            }
        }
    }

    public class WfExecutionContainer : WfExecutionContainerBase
    {
        readonly IExecutionManager _executionManager;
        readonly ISubscriptionProvider _subscriptionProvider;

        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, ISubscriptionProvider subscriptionProvider)
            : this(sa, dataObj, theWorkspace, esbChannel, subscriptionProvider, CustomContainer.Get<IExecutionManager>())
		{
			_subscriptionProvider = subscriptionProvider ?? SubscriptionProvider.Instance;
		}

        private WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, ISubscriptionProvider subscriptionProvider, IExecutionManager executionManager)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _executionManager = executionManager;
            _subscriptionProvider = subscriptionProvider ?? SubscriptionProvider.Instance;
        }

        override protected void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            try
            {
                AddExecutionToExecutionManager(dsfDataObject, resource);

                WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;

                Dev2Logger.Debug("Starting Execute", dsfDataObject.ExecutionID?.ToString());
				dsfDataObject.StateNotifier?.LogExecuteStartState(resource);

				var lastActivity = resource;

                ExecuteNode(dsfDataObject, update, ref resource, ref lastActivity, _subscriptionProvider);

                if (!dsfDataObject.StopExecution)
                {
                    dsfDataObject.StateNotifier?.LogExecuteCompleteState(lastActivity);
                    dsfDataObject.ExecutionException = null;
                }
            }
            finally
            {
                dsfDataObject.StateNotifier?.Dispose();
                _executionManager?.CompleteExecution();
                ServerStats.IncrementTotalExecutions();
            }
        }

        private static IDev2Activity ExecuteTool(IDSFDataObject dsfDataObject, IDev2Activity activity, int update)
        {
            if (activity is IStateNotifierRequired stateNotifierRequired)
            {
                stateNotifierRequired.SetStateNotifier(dsfDataObject.StateNotifier);
            }

            var result = activity.Execute(dsfDataObject, update);
            return result;
        }

        void AddExecutionToExecutionManager(IDSFDataObject dsfDataObject, IDev2Activity resource)
        {
            Dev2Logger.Debug("Got Execution Manager", dsfDataObject.ExecutionID?.ToString());
            if (_executionManager != null)
            {
                if (!_executionManager.IsRefreshing || dsfDataObject.IsSubExecution)
                {
                    Dev2Logger.Debug("Adding Execution to Execution Manager", dsfDataObject.ExecutionID?.ToString());
                    _executionManager.AddExecution();
                    Dev2Logger.Debug("Added Execution to Execution Manager", dsfDataObject.ExecutionID?.ToString());
                }
                else
                {
                    Dev2Logger.Debug("Waiting", dsfDataObject.ExecutionID?.ToString());
                    _executionManager.Wait();
                    Dev2Logger.Debug("Continued Execution", dsfDataObject.ExecutionID?.ToString());
                }
            }

            if (resource == null)
            {
                throw new InvalidOperationException(GlobalConstants.NoStartNodeError);
            }
        }

        private static void ExecuteNode(IDSFDataObject dsfDataObject, int update, ref IDev2Activity next,
            ref IDev2Activity lastActivity, ISubscriptionProvider subscriptionProvider)
        {
            var environment = dsfDataObject.Environment;
            try
            {
                if (!subscriptionProvider.IsLicensed)
                {
                    dsfDataObject.Environment.AddError(ErrorResource.InvalidLicense);
                    dsfDataObject.ExecutionException = new Exception(ErrorResource.InvalidLicense);
                    Dev2Logger.Error(ErrorResource.InvalidLicense, dsfDataObject.ExecutionID?.ToString());
                    return;
                }

                Dev2Logger.Debug("Executing first node", dsfDataObject.ExecutionID?.ToString());
                while (next != null)
				{
					dsfDataObject.StateNotifier?.LogExecuteActivityStartState(next);

					var current = next;
                    lastActivity = current;
                    next = ExecuteTool(dsfDataObject, current, update);
                    environment.AllErrors.UnionWith(environment.Errors);
                    
                    dsfDataObject.StateNotifier?.LogExecuteActivityCompleteState(lastActivity);

                    if (dsfDataObject.StopExecution)
                    {
                        if (dsfDataObject.Environment.FetchErrors().Length > 1)
                        {
                            dsfDataObject.ExecutionException = new Exception(dsfDataObject.Environment.FetchErrors());
                        }

                        dsfDataObject.StateNotifier?.LogStopExecutionState(lastActivity);
                        break;
                    }
                }
            }
            catch (Exception exception)
			{
				Dev2Logger.Error(exception.Message, dsfDataObject.ExecutionID?.ToString());
                dsfDataObject.ExecutionException = new Exception(dsfDataObject.Environment.FetchErrors());
                dsfDataObject.StateNotifier?.LogExecuteException(new SerializableException(exception), lastActivity);
            }
        }

        protected override void Eval(Guid resourceID, IDSFDataObject dataObject)
        {
            var executionId = dataObject.ExecutionID.ToString();
            var versionNumber = dataObject.VersionNumber;

            Dev2Logger.Debug("Getting Resource to Execute", executionId);

            var resourcesNew = new System.Collections.Concurrent.ConcurrentDictionary<Guid, List<IResource>>();
            foreach (var r in _resourceCatalog.WorkspaceResources)
            {
                var resourceList = r.Value as List<IResource>;
                var resourceListCopy = new List<IResource>();
                foreach (var b in resourceList)
                {
                    var resourceToAdd = b;
                    if (resourceToAdd is Workflow)
                    {
                        resourceToAdd = ((Workflow)resourceToAdd).Clone();
                    }
                    resourceListCopy.Add(resourceToAdd);
                }

                resourcesNew.AddOrUpdate(r.Key, id => r.Value, (id, resources) => resourceListCopy);
            }

            using (var catalog = new ResourceCatalog(resourcesNew, _resourceCatalog.GetServerVersionRepository(), _resourceCatalog.GetCatalogPluginContainer()))
            {
                var hasVersionOverride = false;
                if (versionNumber != 0)
                {
                    hasVersionOverride = true;
                }

                IDev2Activity startActivity;
                if (hasVersionOverride)
                {
                    var resumeVersionNumber = versionNumber;
                    if (resumeVersionNumber == 0)
                    {
                        resumeVersionNumber = catalog.GetLatestVersionNumberForResource(resourceId: resourceID);
                    }

                    var resourceObject = catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceID,
                        resumeVersionNumber.ToString());
                    startActivity = catalog.Parse(TheWorkspace.ID, resourceID, executionId, resourceObject);
                }
                else
                {
                    startActivity = catalog.Parse(TheWorkspace.ID, resourceID, executionId);
                }

                Dev2Logger.Debug("Got Resource to Execute", executionId);
                EvalInner(dataObject, startActivity, dataObject.ForEachUpdateValue);
            }
        }
    }

    public class ResumableExecutionContainerFactory : IResumableExecutionContainerFactory
    {
        public IResumableExecutionContainer New(Guid startActivityId, ServiceAction sa, DsfDataObject dataObject)
        {
            return new ResumableExecutionContainer(startActivityId, sa, dataObject);
        }

        public IResumableExecutionContainer New(Guid startActivityId, ServiceAction sa, DsfDataObject dataObject,
            IWorkspace workspace)
        {
            return new ResumableExecutionContainer(startActivityId, sa, dataObject, workspace);
        }
    }

    public class ResumableExecutionContainer : WfExecutionContainer, IResumableExecutionContainer
    {
        readonly Guid _resumeActivityId;
        IExecutionEnvironment _resumeEnvironment;

        public ResumableExecutionContainer(Guid resumeActivityId, ServiceAction sa, IDSFDataObject dataObject)
            : this(resumeActivityId, dataObject.Environment, sa, dataObject, WorkspaceRepository.Instance.ServerWorkspace, new EsbServicesEndpoint(), SubscriptionProvider.Instance)
        {
        }

        public ResumableExecutionContainer(Guid resumeActivityId, ServiceAction sa, IDSFDataObject dataObject,
            IWorkspace workspace)
            : this(resumeActivityId, dataObject.Environment, sa, dataObject, workspace, new EsbServicesEndpoint(), SubscriptionProvider.Instance)
        {
        }

        public ResumableExecutionContainer(Guid resumeActivityId, IExecutionEnvironment env, ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, ISubscriptionProvider subscriptionProvider)
            : base(sa, dataObj, theWorkspace, esbChannel, subscriptionProvider)
        {
            _resumeActivityId = resumeActivityId;
            _resumeEnvironment = env;
        }

        protected override void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            var startAtActivity = FindActivity(resource) ??
                                  throw new InvalidWorkflowException(
                                      $"Resume Node not found. UniqueID:{_resumeActivityId}");
            dsfDataObject.Environment = _resumeEnvironment;
            dsfDataObject.StartActivityId = Guid.Empty;

            var stateNotifier = new StateNotifier();
            using (var listener = new StateAuditLogger(new WebSocketPool()))
            {
                stateNotifier.Subscribe(listener.NewStateListener(dsfDataObject));
                dsfDataObject.StateNotifier = stateNotifier;

                base.EvalInner(dsfDataObject, startAtActivity, update);
            }
        }

        private IDev2Activity FindActivity(IDev2Activity resource)
        {
            var allNodes = new ActivityParser().ParseToLinkedFlatList(resource);
            return allNodes.FirstOrDefault(p => p.UniqueID == _resumeActivityId.ToString());
        }

        ~ResumableExecutionContainer()
        {
            ReleaseUnmanagedResources();
        }
    
        private void ReleaseUnmanagedResources()
        {
            _resumeEnvironment = null;
            this.Request = null;
            this.DataObject = null;
            this.ServiceAction = null;
            this.TheWorkspace = null;
            this.InstanceInputDefinition = null;
            this.InstanceOutputDefinition = null;
        }
    
        public void Dispose()
        {
            
        }
    }

}