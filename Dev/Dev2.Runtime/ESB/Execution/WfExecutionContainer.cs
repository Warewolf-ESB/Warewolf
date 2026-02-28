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
using System.IO;
using System.Xml.Linq;
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
using System.Linq;
using System.Threading;
using Dev2.Runtime.Subscription;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Auditing;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using System.Runtime.Serialization;
using Dev2.Common.Interfaces.Core.DynamicServices;
using System.Text;

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
                    dsfDataObject.ExecutionException = new Exception(ErrorResource.InvalidLicense);
                    Dev2Logger.Error(ErrorResource.InvalidLicense, dsfDataObject.ExecutionID?.ToString());
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
				Dev2Logger.Error(exception, dsfDataObject.ExecutionID?.ToString());
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

    /// <summary>
    /// Workflow execution container for Azure Functions environments.
    ///
    /// Differs from <see cref="WfExecutionContainer"/> in two ways:
    /// <list type="bullet">
    ///   <item><b>Authorization:</b> <see cref="CanExecute"/> always returns
    ///         <see langword="true"/> — Azure Functions enforces auth at the
    ///         HTTP-trigger level, so the Windows-identity check is not applicable.</item>
    ///   <item><b>Resource loading:</b> when a <see cref="DynamicActivity"/> is supplied
    ///         at construction time the normal <see cref="ResourceCatalog"/> /
    ///         <see cref="WorkspaceRepository"/> initialization is bypassed entirely;
    ///         the pre-parsed activity is used directly.</item>
    /// </list>
    /// </summary>
    public class AzureFunctionExecutionContainer : WfExecutionContainer
    {
        private readonly DynamicActivity _preloadedActivity;

        public AzureFunctionExecutionContainer(
            ServiceAction sa,
            IDSFDataObject dataObj,
            IWorkspace theWorkspace,
            IEsbChannel esbChannel,
            ISubscriptionProvider subscriptionProvider,
            DynamicActivity preloadedActivity = null)
            : base(sa, dataObj, theWorkspace, esbChannel, subscriptionProvider)
        {
            _preloadedActivity = preloadedActivity;
        }

        /// <summary>
        /// Always returns <see langword="true"/>: Azure Functions auth is enforced at
        /// the trigger level, not inside the execution pipeline.
        /// </summary>
        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject,
            AuthorizationContext authorizationContext) => true;

        /// <summary>
        /// Uses the pre-loaded <see cref="DynamicActivity"/> when available, bypassing
        /// <see cref="ResourceCatalog"/>.  Falls back to a file-system scan by
        /// <paramref name="resourceID"/> when none was supplied.
        /// </summary>
        protected override void Eval(Guid resourceID, IDSFDataObject dataObject)
        {
            if (_preloadedActivity != null)
            {
                base.Eval(_preloadedActivity, dataObject, dataObject.ForEachUpdateValue);
                return;
            }

            // Fallback: scan EnvironmentVariables.ResourcePath for a .bite file
            // whose XML ID attribute matches resourceID.
            var bitePath = FindBiteFileById(resourceID);
            if (bitePath == null)
                throw new InvalidWorkflowException($"No .bite file found for resource ID {resourceID}.");

            base.Eval(LoadDynamicActivity(bitePath, dataObject.ServiceName), dataObject, dataObject.ForEachUpdateValue);
        }

        private static string FindBiteFileById(Guid resourceID)
        {
            var root = AzureFunctionWorkflowRunner.ResourceBasePath ?? EnvironmentVariables.ResourcePath;
            if (!Directory.Exists(root))
                return null;

            return Directory.EnumerateFiles(root, "*.bite", SearchOption.AllDirectories)
                            .FirstOrDefault(f =>
                            {
                                try
                                {
                                    return string.Equals(
                                        XElement.Load(f).Attribute("ID")?.Value,
                                        resourceID.ToString(),
                                        StringComparison.OrdinalIgnoreCase);
                                }
                                catch { return false; }
                            });
        }

        /// <summary>
        /// Loads and parses a <see cref="DynamicActivity"/> from a <c>.bite</c> file on disk.
        /// </summary>
        internal static DynamicActivity LoadDynamicActivity(string bitePath, string workflowName)
        {
            Console.WriteLine($"[AzureFunc] LoadDynamicActivity: '{workflowName}' from '{bitePath}'");

            var xml = XElement.Load(bitePath);
            var action = xml.Descendants("Action").FirstOrDefault()
                ?? throw new InvalidWorkflowException($"No <Action> element in '{bitePath}'.");

            // Use .Value to get the HTML-decoded inner XAML text (<Activity ...>...</Activity>).
            // ElementSafeStringBuilder would return elm.ToString() which re-wraps the content
            // in <XamlDefinition>...</XamlDefinition>, causing the parser to see the wrong root.
            var xamlContent = action.Element("XamlDefinition")?.Value
                ?? throw new InvalidWorkflowException($"No <XamlDefinition> content in '{bitePath}'.");

            Console.WriteLine($"[AzureFunc] LoadDynamicActivity: XAML length={xamlContent.Length}, starts with: {xamlContent.Substring(0, Math.Min(120, xamlContent.Length))}");

            try
            {
                var xamlDefinition = new StringBuilder(xamlContent);

                // Strip WPF designer-only elements (VirtualizedContainerService.HintSize,
                // sap:WorkflowViewStateService.ViewState, VisualBasic.Settings) that CoreWF
                // on .NET 8 does not recognise and throws on.
                Console.WriteLine($"[AzureFunc] LoadDynamicActivity: calling RemoveWindowsElements ...");
                Dev2.DynamicServices.Objects.Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);
                Console.WriteLine($"[AzureFunc] LoadDynamicActivity: XAML length after strip={xamlDefinition.Length}");

                // Load directly as a DynamicActivity — <Activity x:Class="..."> root is
                // deserialised by ActivityXamlServices.Load to a DynamicActivity.
                using var stream = xamlDefinition.EncodeForXmlDocument(tryUnicodeFirst: false);
                Console.WriteLine($"[AzureFunc] LoadDynamicActivity: calling ActivityXamlServices.Load ...");
                var activity = System.Activities.XamlIntegration.ActivityXamlServices.Load(stream);
                Console.WriteLine($"[AzureFunc] LoadDynamicActivity: loaded type={activity?.GetType().FullName}");

                return activity as DynamicActivity
                    ?? throw new InvalidWorkflowException(
                        $"Loaded activity is not a DynamicActivity for '{workflowName}'. Actual type: {activity?.GetType().FullName}");
            }
            catch (InvalidWorkflowException) { throw; }
            catch (Exception ex)
            {
                Console.WriteLine($"[AzureFunc] LoadDynamicActivity FAILED: {ex.GetType().FullName}: {ex.Message}");
                Console.WriteLine($"[AzureFunc] Stack: {ex.StackTrace}");
                for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
                    Console.WriteLine($"[AzureFunc]   --> inner: {inner.GetType().FullName}: {inner.Message}");
                throw new InvalidWorkflowException($"XAML parse failed for '{workflowName}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Entry point for executing a single Warewolf workflow from inside an Azure
    /// Functions app.
    ///
    /// <para><b>Skipped</b> compared to a full server start-up:</para>
    /// <list type="bullet">
    ///   <item>HTTP routing / SSL — Azure Functions handles this.</item>
    ///   <item><see cref="ResourceCatalog"/> / <see cref="WorkspaceRepository"/>
    ///         initialization — the workflow <c>.bite</c> file is read directly from
    ///         <see cref="EnvironmentVariables.ResourcePath"/>.</item>
    ///   <item>Studio debug-UI dispatch — <see cref="IDSFDataObject.IsDebug"/> is
    ///         always <see langword="false"/>, so
    ///         <see cref="WfApplicationUtils.DispatchDebugState"/> is never called.</item>
    ///   <item><see cref="ServerAuthorizationService"/> Windows-identity check — see
    ///         <see cref="AzureFunctionExecutionContainer.CanExecute"/>.</item>
    /// </list>
    ///
    /// <para><b>Retained:</b></para>
    /// <list type="bullet">
    ///   <item>XAML parsing and ActivityParser execution pipeline.</item>
    ///   <item>Subscription / license check via <see cref="ISubscriptionProvider.IsLicensed"/>.</item>
    ///   <item>Audit state notifications via <see cref="StateNotifier"/> /
    ///         <see cref="StateAuditLogger"/> — silently no-ops when no audit
    ///         WebSocket endpoint is configured.</item>
    /// </list>
    /// </summary>
    public static class AzureFunctionWorkflowRunner
    {
        /// <summary>
        /// Overrides the root directory used to locate <c>.bite</c> workflow files.
        /// When <see langword="null"/> (the default), <see cref="EnvironmentVariables.ResourcePath"/>
        /// is used.  Set this once in Azure Functions startup to
        /// <c>Path.Combine(AppContext.BaseDirectory, "Resources")</c> so that workflow
        /// files bundled alongside the deployment are found without any server-runtime
        /// path configuration.
        /// </summary>
        public static string ResourceBasePath { get; set; }

        /// <summary>
        /// Executes the named workflow and returns its result.
        /// </summary>
        /// <param name="workflowName">
        ///   Name used to locate <c>{workflowName}.bite</c> under
        ///   <see cref="EnvironmentVariables.ResourcePath"/>.
        /// </param>
        /// <param name="inputJson">
        ///   Optional JSON / XML payload written into the execution environment before
        ///   the workflow starts.  Pass <see langword="null"/> for no inputs.
        /// </param>
        /// <returns>
        ///   <c>resultId</c> — DataListID of the completed execution.<br/>
        ///   <c>errors</c>   — Any errors raised during execution (empty on success).
        /// </returns>
        /// <summary>
        /// Executes the named workflow and returns its result, errors, and output variable values.
        /// </summary>
        public static (Guid resultId, ErrorResultTO errors, Dictionary<string, string> outputs) ExecuteWorkflow(
            string workflowName, string inputXml = null)
        {
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: '{workflowName}'");
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: ResourceBasePath='{ResourceBasePath}'");

            var bitePath = FindBiteFile(workflowName);
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: bite file='{bitePath ?? "(not found)"}'");

            if (bitePath == null)
            {
                var notFound = new ErrorResultTO();
                notFound.AddError($"No .bite file found for workflow '{workflowName}' under '{ResourceBasePath ?? EnvironmentVariables.ResourcePath}'.");
                return (GlobalConstants.NullDataListID, notFound, new Dictionary<string, string>());
            }

            var (resourceId, resourceName) = ReadResourceMetadata(bitePath);
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: resourceId={resourceId}, resourceName='{resourceName}'");

            // Build the DataList payload: always include the full schema from the bite file so
            // every variable (e.g. [[Name]], [[Message]]) is defined before execution starts.
            // Without this, Warewolf raises "variable { X } not found" for any referenced var.
            var dataListPayload = BuildDataListPayload(bitePath, inputXml);
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: dataListPayload='{dataListPayload}'");

            // Parse the XAML once here so the container can use it without any catalog access.
            var dynamicActivity = AzureFunctionExecutionContainer.LoadDynamicActivity(bitePath, workflowName);
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: DynamicActivity loaded OK");

            var sa = new ServiceAction
            {
                ActionType = enActionType.Workflow,
                ServiceName = resourceName,
                ServiceID = resourceId,
            };

            var dataObject = new DsfDataObject(dataListPayload, Guid.NewGuid())
            {
                ResourceID = resourceId,
                ServiceName = resourceName,
                WorkspaceID = GlobalConstants.ServerWorkspaceID,
                ExecutingUser = Thread.CurrentPrincipal,
                ExecutionID = Guid.NewGuid(),
                IsDebug = false,
                ServerID = Guid.NewGuid(),
                WebUrl = $"azurefunc://{workflowName}",
            };

            // Populate the execution environment with input values so that [[Variable]]
            // references resolve correctly during workflow execution.
            // DsfDataObject(rawPayload, ...) seeds only the legacy DataList; the
            // Warewolf.Storage execution environment (dataObject.Environment) must be
            // populated separately via Assign().
            if (!string.IsNullOrWhiteSpace(inputXml))
            {
                try
                {
                    var inputEl = XElement.Parse(inputXml);
                    foreach (var el in inputEl.Elements())
                    {
                        var varName = $"[[{el.Name.LocalName}]]";
                        var varValue = el.Value;
                        Console.WriteLine($"[AzureFunc] ExecuteWorkflow: assigning {varName}='{varValue}'");
                        dataObject.Environment.Assign(varName, varValue, 0);
                    }
                }
                catch (Exception assignEx)
                {
                    Console.WriteLine($"[AzureFunc] ExecuteWorkflow: failed assigning inputs to environment: {assignEx.Message}");
                }
            }

            var stateNotifier = new StateNotifier();
            using var auditLogger = new StateAuditLogger(new WebSocketPool());
            stateNotifier.Subscribe(auditLogger.NewStateListener(dataObject));
            dataObject.StateNotifier = stateNotifier;

            var container = new AzureFunctionExecutionContainer(
                sa,
                dataObject,
                new Workspace(GlobalConstants.ServerWorkspaceID),
                new EsbServicesEndpoint(),
                SubscriptionProvider.Instance,
                dynamicActivity);

            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: calling container.Execute ...");
            var resultId = container.Execute(out var errors, 0);
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: Execute returned resultId={resultId}, errors={errors?.FetchErrors()?.Count ?? 0}");

            // Extract output variable values from the data object's environment after execution.
            var outputs = ExtractOutputs(bitePath, dataObject);
            Console.WriteLine($"[AzureFunc] ExecuteWorkflow: outputs={string.Join(", ", outputs.Select(kv => $"{kv.Key}={kv.Value}"))}");

            return (resultId, errors, outputs);
        }

        /// <summary>
        /// Reads the DataList schema from the bite file and merges any input values supplied
        /// as a DataList XML string (e.g. &lt;DataList&gt;&lt;Name&gt;Ash&lt;/Name&gt;&lt;/DataList&gt;).
        /// Returns a well-formed DataList XML with every variable from the schema pre-defined.
        /// </summary>
        private static string BuildDataListPayload(string bitePath, string inputXml)
        {
            var biteXml = XElement.Load(bitePath);
            var schemaEl = biteXml.Element("DataList");
            if (schemaEl == null)
                return inputXml ?? string.Empty;

            // Parse any supplied input values
            var inputValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(inputXml))
            {
                try
                {
                    var inputEl = XElement.Parse(inputXml);
                    foreach (var el in inputEl.Elements())
                        inputValues[el.Name.LocalName] = el.Value;
                }
                catch { /* ignore malformed input; proceed with empty inputs */ }
            }

            // Build <DataList> with every schema variable defined, input values merged in
            var sb = new StringBuilder("<DataList>");
            foreach (var varEl in schemaEl.Elements())
            {
                var name = varEl.Name.LocalName;
                inputValues.TryGetValue(name, out var value);
                sb.Append($"<{name}>{System.Security.SecurityElement.Escape(value ?? string.Empty)}</{name}>");
            }
            sb.Append("</DataList>");
            return sb.ToString();
        }

        /// <summary>
        /// Reads output variable values from the data object's execution environment.
        /// </summary>
        private static Dictionary<string, string> ExtractOutputs(string bitePath, DsfDataObject dataObject)
        {
            var outputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var biteXml = XElement.Load(bitePath);
                var schemaEl = biteXml.Element("DataList");
                if (schemaEl == null || dataObject.Environment == null)
                    return outputs;

                foreach (var varEl in schemaEl.Elements())
                {
                    // Only include variables that are declared as Output or Both.
                    // Variables with ColumnIODirection="Input" (or "None") are internal
                    // inputs and should not appear in the HTTP response.
                    var direction = varEl.Attribute("ColumnIODirection")?.Value ?? "Both";
                    if (direction.Equals("Input", StringComparison.OrdinalIgnoreCase)
                        || direction.Equals("None", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var name = varEl.Name.LocalName;
                    try
                    {
                        var result = dataObject.Environment.EvalAsListOfStrings($"[[{name}]]", 0);
                        if (result?.Count > 0)
                            outputs[name] = string.Join(",", result);
                    }
                    catch { /* skip variables that can't be evaluated */ }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AzureFunc] ExtractOutputs failed: {ex.Message}");
            }
            return outputs;
        }

        private static string FindBiteFile(string workflowName)
        {
            var root = ResourceBasePath ?? EnvironmentVariables.ResourcePath;
            if (!Directory.Exists(root))
                return null;

            return Directory.EnumerateFiles(root, $"{workflowName}.bite", SearchOption.AllDirectories)
                            .FirstOrDefault();
        }

        private static (Guid resourceId, string resourceName) ReadResourceMetadata(string bitePath)
        {
            var xml = XElement.Load(bitePath);
            Guid.TryParse(xml.Attribute("ID")?.Value, out var id);
            var name = xml.Element("DisplayName")?.Value
                       ?? Path.GetFileNameWithoutExtension(bitePath);
            return (id, name);
        }
    }

}