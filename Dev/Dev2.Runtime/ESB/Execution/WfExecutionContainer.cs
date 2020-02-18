#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Threading;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
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
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    public abstract class WfExecutionContainerBase : EsbExecutionContainer
    {
        protected readonly IResourceCatalog _resourceCatalog;
        protected WfExecutionContainerBase(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : this(sa, dataObj, theWorkspace, esbChannel, ResourceCatalog.Instance)
        {
        }

        protected WfExecutionContainerBase(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _resourceCatalog = resourceCatalog;
            DataObject.Settings = GetWorkflowSetting();
            DataObject.DataList = sa.DataListSpecification;
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

            var user = Thread.CurrentPrincipal;
            var dataObjectExecutionId = DataObject.ExecutionID.ToString();
            if (!DataObject.IsSubExecution)
            {
                var userIdentity = user?.Identity;
                Dev2Logger.Debug(string.Format(GlobalConstants.ExecuteWebRequestString, DataObject.ServiceName, userIdentity?.Name, userIdentity?.AuthenticationType, userIdentity?.IsAuthenticated, DataObject.RawPayload), dataObjectExecutionId);
                Dev2Logger.Debug("Request URL [ " + DataObject.WebUrl + " ]", dataObjectExecutionId);
            }
            Dev2Logger.Debug("Entered Wf Container", dataObjectExecutionId);
            DataObject.ServiceName = ServiceAction.ServiceName;

            var executionForServiceString = string.Format(GlobalConstants.ExecutionForServiceString, DataObject.ServiceName, DataObject.ResourceID, (DataObject.IsDebug ? "Debug" : "Execute"));
            Dev2Logger.Info("Started " + executionForServiceString, dataObjectExecutionId);
            SetExecutionOrigin();

            var userPrinciple = Thread.CurrentPrincipal; // TODO: can we remove this second call the get_CurrentPrincipal
            var result = GlobalConstants.NullDataListID;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => { result = ExecuteWf(); });

            errors = AddErrors();

            var executionTypeString = DataObject.IsSubExecution ? "Completed Sub " : "Completed ";
            Dev2Logger.Info(executionTypeString + executionForServiceString, dataObjectExecutionId);
            return result;
        }

        Guid ExecuteWf()
        {
            var result = new Guid();
            DataObject.StartTime = DateTime.Now;
            var wfappUtils = new WfApplicationUtils(_resourceCatalog);
            ErrorResultTO invokeErrors;
            var executionId = DataObject.ExecutionID.ToString();

            try
            {
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                DataObject.ExecutionToken = exeToken;
                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.Start, out invokeErrors, true, false, false);
                }
                var resourceId = DataObject.ResourceID;
                if (CanExecute(resourceId, DataObject, AuthorizationContext.Execute))
                {
                    Eval(resourceId, DataObject);
                }
                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);
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
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, executionId);
                DataObject.Environment.AddError(ex.Message);
                DataObject.ExecutionException = ex;
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);
            }
            return result;
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            var key = (dataObject.ExecutingUser, AuthorizationContext.Execute, resourceId.ToString());
            var isAuthorized = dataObject.AuthCache.GetOrAdd(key, (requestedKey) => ServerAuthorizationService.Instance.IsAuthorized(dataObject.ExecutingUser, AuthorizationContext.Execute, resourceId.ToString()));
            if (!isAuthorized)
            {
                dataObject.Environment.AddError(string.Format(ErrorResource.UserNotAuthorizedToExecuteException, dataObject.ExecutingUser?.Identity.Name, dataObject.ServiceName));
            }
            return isAuthorized;
        }

        public void Eval(DynamicActivity flowchartProcess, IDSFDataObject dsfDataObject, int update)
        {
            var resource = new ActivityParser().Parse(flowchartProcess);
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
                DataObject.WebUrl = $"{EnvironmentVariables.WebServerUri}secure/{DataObject.ServiceName}.{DataObject.ReturnType}?" + DataObject.QueryString;
            }
            if (DataObject.ServerID == Guid.Empty)
            {
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;
            }
        }

        ErrorResultTO AddErrors()
        {
            var errors = new ErrorResultTO();
            foreach (var err in DataObject.Environment.Errors)
            {
                errors.AddError(err, true);
            }
            foreach (var err in DataObject.Environment.AllErrors)
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
        private readonly IStateNotifier _stateNotifier;
        readonly IExecutionManager _executionManager;

        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : this(sa, dataObj, theWorkspace, esbChannel, CustomContainer.Get<IExecutionManager>())
        {
        }

        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, IExecutionManager executionManager)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _executionManager = executionManager; //TODO: [code smell] this should not appear in both CTOR 

        }
        //TODO: The work on the UI as per EsbExecutionContainer.cs might resolve this code smell CTOR
        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, IExecutionManager executionManager, IStateNotifier stateNotifier)
           : this(sa, dataObj, theWorkspace, esbChannel)
        {
            _stateNotifier = stateNotifier;
            _executionManager = executionManager;
        }

        override protected void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            var outerStateLogger = dsfDataObject.StateNotifier;

            IStateNotifier stateNotifier = null;

            try
            {
                var dataListTO = new DataListTO(dsfDataObject.DataList.ToString());

                //TODO: [WIP] The UI changes suggested above resolves the move of this code?                
                //dsfDataObject.Settings = new Dev2WorkflowSettingsTO
                //{
                //    EnableDetailedLogging = Config.Server.EnableDetailedLogging,
                //    LoggerType = LoggerType.JSON,
                //    KeepLogsForDays = 2,
                //    CompressOldLogFiles = true
                //};


                AddExecutionToExecutionManager(dsfDataObject, resource);

                WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;

                Dev2Logger.Debug("Starting Execute", GlobalConstants.WarewolfDebug);
                stateNotifier?.LogPreExecuteState(resource);
                //log the start of execution
                if (dsfDataObject.Settings.EnableDetailedLogging)
                {
                    stateNotifier = _stateNotifier; //LogManager.CreateStateNotifier(dsfDataObject); //TODO: (DI): LogManager.CreateStateNotifier() inject for testing
                    dsfDataObject.StateNotifier = stateNotifier;

                    LogInputOutputVariables(dataListTO.Inputs, dsfDataObject);

                    stateNotifier?.LogAdditionalDetail(resource, dsfDataObject.ExecutionID.ToString()); //TODO: The table of information as per clients expectations is blocking this, 
                }

                IDev2Activity next;
                IDev2Activity lastActivity;
                try
                {
                    lastActivity = resource;
                    next = resource.Execute(dsfDataObject, update);
                    stateNotifier?.LogPostExecuteState(resource, next);
                }
                catch (Exception e)
                {
                    stateNotifier?.LogExecuteException(e, resource);
                    throw;
                }

                ExecuteNode(dsfDataObject, update, ref next, ref lastActivity);
                //log successful completion
                if (dsfDataObject.Settings.EnableDetailedLogging)
                {
                    stateNotifier = _stateNotifier; //LogManager.CreateStateNotifier(dsfDataObject); //TODO: (DI): LogManager.CreateStateNotifier() inject for testing
                    dsfDataObject.StateNotifier = stateNotifier;

                    LogInputOutputVariables(dataListTO.Outputs, dsfDataObject);

                    stateNotifier?.LogAdditionalDetail(resource, dsfDataObject.ExecutionID.ToString()); //TODO: The table of information as per clients expectations is blocking this, 
                }
            }
            //catch log detailed exception
            catch (Exception ex)
            {
                if (dsfDataObject.Settings.EnableDetailedLogging)
                {
                    stateNotifier = _stateNotifier; //LogManager.CreateStateNotifier(dsfDataObject); //TODO: (DI): LogManager.CreateStateNotifier() inject for testing
                    dsfDataObject.StateNotifier = stateNotifier;
                    //this should take Exception data from the resource
                    stateNotifier?.LogExecuteException(ex, resource); //TODO: The table of information as per clients expectations is blocking this, 
                }
            }

            finally
            {
                _executionManager?.CompleteExecution();

                stateNotifier?.Dispose();
                dsfDataObject.StateNotifier = outerStateLogger;
            }
        }


        void AddExecutionToExecutionManager(IDSFDataObject dsfDataObject, IDev2Activity resource)
        {
            Dev2Logger.Debug("Got Execution Manager", GlobalConstants.WarewolfDebug);
            if (_executionManager != null)
            {
                if (!_executionManager.IsRefreshing || dsfDataObject.IsSubExecution)
                {
                    Dev2Logger.Debug("Adding Execution to Execution Manager", GlobalConstants.WarewolfDebug);
                    _executionManager.AddExecution();
                    Dev2Logger.Debug("Added Execution to Execution Manager", GlobalConstants.WarewolfDebug);
                }
                else
                {
                    Dev2Logger.Debug("Waiting", GlobalConstants.WarewolfDebug);
                    _executionManager.Wait();
                    Dev2Logger.Debug("Continued Execution", GlobalConstants.WarewolfDebug);

                }
            }
            if (resource == null)
            {
                throw new InvalidOperationException(GlobalConstants.NoStartNodeError);
            }
        }

        static void ExecuteNode(IDSFDataObject dsfDataObject, int update, ref IDev2Activity next, ref IDev2Activity lastActivity)
        {
            var stateNotifier = dsfDataObject.StateNotifier;
            var environment = dsfDataObject.Environment;

            bool stoppedExecution = false;
            Dev2Logger.Debug("Executed first node", GlobalConstants.WarewolfDebug);
            while (next != null)
            {
                if (dsfDataObject.StopExecution)
                {
                    //On error detailed logging
                    dsfDataObject.ExecutionException = new Exception(dsfDataObject.Environment.FetchErrors());
                    stoppedExecution = true;
                    break;
                }

                stateNotifier?.LogPreExecuteState(next);
                var current = next;
                lastActivity = current;
                try
                {
                    next = current.Execute(dsfDataObject, update);
                    stateNotifier?.LogPostExecuteState(current, next);
                }
                catch (Exception e)
                {
                    stateNotifier?.LogExecuteException(e, current);
                    throw;
                }
                environment.AllErrors.UnionWith(environment.Errors);
            }

            if (!stoppedExecution)
            {
                stateNotifier?.LogExecuteCompleteState(lastActivity);
            }
            else
            {
                stateNotifier?.LogStopExecutionState(lastActivity);
            }
        }

        protected override void Eval(Guid resourceId, IDSFDataObject dataObject)
        {
            var executionId = dataObject.ExecutionID.ToString();
            var versionNumber = dataObject.VersionNumber;

            Dev2Logger.Debug("Getting Resource to Execute", executionId);

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
                    resumeVersionNumber = _resourceCatalog.GetLatestVersionNumberForResource(resourceId: resourceId);
                }

                var resourceObject = _resourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId, resumeVersionNumber.ToString());
                startActivity = _resourceCatalog.Parse(TheWorkspace.ID, resourceId, executionId, resourceObject);
            }
            else
            {
                startActivity = _resourceCatalog.Parse(TheWorkspace.ID, resourceId, executionId);
            }

            Dev2Logger.Debug("Got Resource to Execute", executionId);
            EvalInner(dataObject, startActivity, dataObject.ForEachUpdateValue);
        }

        void LogInputOutputVariables(List<string> variables, IDSFDataObject dsfDataObject)
        {
            foreach (var variable in variables)
            {
                if (DataListUtil.IsValueRecordset(DataListUtil.AddBracketsToValueIfNotExist(variable)))
                {
                    var warewolfEvalResult = dsfDataObject.Environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(variable), 0);

                    if (warewolfEvalResult.IsWarewolfAtomListresult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recordSetResult)
                    {
                        foreach (var item in recordSetResult.Item)
                        {
                            //TODO: Log item
                        }
                    }
                }
                else
                {
                    var warewolfEvalResult = dsfDataObject.Environment.Eval(DataListUtil.AddBracketsToValueIfNotExist(variable), 0);

                    if (warewolfEvalResult.IsWarewolfAtomResult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult && !scalarResult.Item.IsNothing)
                    {
                        //TODO: Log scalarResult.Item
                    }
                    else
                    {
                        var evalResult = dsfDataObject.Environment.EvalForJson("[[@" + variable + "]]");

                        //TODO: Log evalResult
                    }
                }
            }
        }
    }


    public class ResumableExecutionContainerFactory : IResumableExecutionContainerFactory
    {
        public IResumableExecutionContainer New(Guid startActivityId, ServiceAction sa, DsfDataObject dataObject)
        {
            return new ResumableExecutionContainer(startActivityId, sa, dataObject);
        }
    }
    public class ResumableExecutionContainer : WfExecutionContainer, IResumableExecutionContainer
    {
        readonly Guid _resumeActivityId;
        readonly IExecutionEnvironment _resumeEnvironment;

        public ResumableExecutionContainer(Guid resumeActivityId, ServiceAction sa, IDSFDataObject dataObject)
            : this(resumeActivityId, dataObject.Environment, sa, dataObject, WorkspaceRepository.Instance.ServerWorkspace, new EsbServicesEndpoint())
        {

        }

        public ResumableExecutionContainer(Guid resumeActivityId, IExecutionEnvironment env, ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
            _resumeActivityId = resumeActivityId;
            _resumeEnvironment = env;
        }

        protected override void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            var startAtActivity = FindActivity(resource) ?? throw new InvalidWorkflowException($"Resume Node not found. UniqueID:{_resumeActivityId}");
            dsfDataObject.Environment = _resumeEnvironment;
            base.EvalInner(dsfDataObject, startAtActivity, update);
        }

        private IDev2Activity FindActivity(IDev2Activity resource)
        {
            var allNodes = new ActivityParser().ParseToLinkedFlatList(resource);
            return allNodes.FirstOrDefault(p => p.UniqueID == _resumeActivityId.ToString());
        }
    }
}
