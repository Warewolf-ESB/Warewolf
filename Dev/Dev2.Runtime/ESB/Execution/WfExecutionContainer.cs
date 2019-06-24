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
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Warewolf.Storage.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    public abstract class WfExecutionContainerBase : EsbExecutionContainer
    {
        protected WfExecutionContainerBase(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
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

            var user = Thread.CurrentPrincipal;
            var dataObjectExecutionId = DataObject.ExecutionID.ToString();
            if (!DataObject.IsSubExecution)
            {
                Dev2Logger.Debug(string.Format(GlobalConstants.ExecuteWebRequestString, DataObject.ServiceName, user?.Identity?.Name, user?.Identity?.AuthenticationType, user?.Identity?.IsAuthenticated, DataObject.RawPayload), dataObjectExecutionId);
                Dev2Logger.Debug("Request URL [ " + DataObject.WebUrl + " ]", dataObjectExecutionId);
            }
            Dev2Logger.Debug("Entered Wf Container", dataObjectExecutionId);
            DataObject.ServiceName = ServiceAction.ServiceName;

            var executionForServiceString = string.Format(GlobalConstants.ExecutionForServiceString, DataObject.ServiceName, DataObject.ResourceID, (DataObject.IsDebug ? "Debug" : "Execute"));
            Dev2Logger.Info("Started " + executionForServiceString, dataObjectExecutionId);
            SetExecutionOrigin();

            var userPrinciple = Thread.CurrentPrincipal;
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
            var wfappUtils = new WfApplicationUtils();
            ErrorResultTO invokeErrors; 
            try
            {
                IExecutionToken exeToken = new ExecutionToken { IsUserCanceled = false };
                DataObject.ExecutionToken = exeToken;
                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.Start, out invokeErrors, true, false, false);
                }
                if (CanExecute(DataObject.ResourceID, DataObject, AuthorizationContext.Execute))
                {
                    Eval(DataObject.ResourceID, DataObject);
                }
                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);
                }
                result = DataObject.DataListID;
            }
            catch (InvalidWorkflowException iwe)
            {
                Dev2Logger.Error(iwe, DataObject.ExecutionID.ToString());
                var msg = iwe.Message;

                var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                var errorMessage = start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message;
                DataObject.Environment.AddError(errorMessage);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, DataObject.ExecutionID.ToString());
                DataObject.Environment.AddError(ex.Message);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, out invokeErrors);
            }
            return result;
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            var isAuthorized = ServerAuthorizationService.Instance.IsAuthorized(dataObject.ExecutingUser, authorizationContext, resourceId.ToString());
            if (!isAuthorized)
            {
                dataObject.Environment.AddError(string.Format(Warewolf.Resource.Errors.ErrorResource.UserNotAuthorizedToExecuteException, dataObject.ExecutingUser?.Identity.Name, dataObject.ServiceName));
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
        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        override protected void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            var outerStateLogger = dsfDataObject.StateNotifier;

            try
            {
                dsfDataObject.Settings = new Dev2WorkflowSettingsTO
                {
                    EnableDetailedLogging = Config.Server.EnableDetailedLogging,
                    LoggerType = LoggerType.JSON,
                    KeepLogsForDays = 2,
                    CompressOldLogFiles = true
                };
                dsfDataObject.StateNotifier = LogManager.CreateStateNotifier(dsfDataObject);

                AddExecutionToExecutionManager(dsfDataObject, resource);

                WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;

                Dev2Logger.Debug("Starting Execute", GlobalConstants.WarewolfDebug);
                dsfDataObject.StateNotifier.LogPreExecuteState(resource);

                IDev2Activity next;
                IDev2Activity lastActivity;
                try
                {
                    lastActivity = resource;
                    next = resource.Execute(dsfDataObject, update);
                    dsfDataObject.StateNotifier.LogPostExecuteState(resource, next);
                }
                catch (Exception e)
                {
                    dsfDataObject.StateNotifier.LogExecuteException(e, resource);
                    throw;
                }

                ExecuteNode(dsfDataObject, update, ref next, ref lastActivity);
            }
            finally
            {
                var exe = CustomContainer.Get<IExecutionManager>();
                exe?.CompleteExecution();

                dsfDataObject.StateNotifier?.Dispose();
                dsfDataObject.StateNotifier = null;
                if (outerStateLogger != null)
                {
                    dsfDataObject.StateNotifier = outerStateLogger;
                }
            }
        }

        static void AddExecutionToExecutionManager(IDSFDataObject dsfDataObject, IDev2Activity resource)
        {
            var exe = CustomContainer.Get<IExecutionManager>();
            Dev2Logger.Debug("Got Execution Manager", GlobalConstants.WarewolfDebug);
            if (exe != null)
            {
                if (!exe.IsRefreshing || dsfDataObject.IsSubExecution)
                {
                    Dev2Logger.Debug("Adding Execution to Execution Manager", GlobalConstants.WarewolfDebug);
                    exe.AddExecution();
                    Dev2Logger.Debug("Added Execution to Execution Manager", GlobalConstants.WarewolfDebug);
                }
                else
                {
                    Dev2Logger.Debug("Waiting", GlobalConstants.WarewolfDebug);
                    exe.Wait();
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
            bool stoppedExecution = false;
            Dev2Logger.Debug("Executed first node", GlobalConstants.WarewolfDebug);
            while (next != null)
            {
                if (dsfDataObject.StopExecution)
                {
                    stoppedExecution = true;
                    break;
                }

                dsfDataObject.StateNotifier.LogPreExecuteState(next);
                var current = next;
                lastActivity = current;
                try
                {
                    next = current.Execute(dsfDataObject, update);
                    dsfDataObject.StateNotifier.LogPostExecuteState(current, next);
                }
                catch (Exception e)
                {
                    dsfDataObject.StateNotifier.LogExecuteException(e, current);
                    throw;
                }
                dsfDataObject.Environment.AllErrors.UnionWith(dsfDataObject.Environment?.Errors);
            }

            if (!stoppedExecution)
            {
                dsfDataObject.StateNotifier.LogExecuteCompleteState(lastActivity);
            }
            else
            {
                dsfDataObject.StateNotifier.LogStopExecutionState(lastActivity);
            }
        }

        protected override void Eval(Guid resourceID, IDSFDataObject dataObject)
        {
            Dev2Logger.Debug("Getting Resource to Execute", dataObject.ExecutionID.ToString());

            var hasVersionOverride = false;
            if (!string.IsNullOrWhiteSpace(dataObject.VersionNumber))
            {
                hasVersionOverride = true;
            }

            var resumeVersionNumber = dataObject.VersionNumber;
            if (resumeVersionNumber is null || string.IsNullOrWhiteSpace(resumeVersionNumber))
            {
                resumeVersionNumber = ResourceCatalog.Instance.GetLatestVersionNumberForResource(resourceId: dataObject.ResourceID).ToString();
            }

            IDev2Activity startActivity;
            if (hasVersionOverride)
            {
                var resourceObject = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, dataObject.ResourceID, resumeVersionNumber);
                startActivity = ResourceCatalog.Instance.Parse(TheWorkspace.ID, resourceID, dataObject.ExecutionID.ToString(), resourceObject);
            } else
            {
                startActivity = ResourceCatalog.Instance.Parse(TheWorkspace.ID, resourceID, dataObject.ExecutionID.ToString());
            }

            Dev2Logger.Debug("Got Resource to Execute", dataObject.ExecutionID.ToString());
            EvalInner(dataObject, startActivity, dataObject.ForEachUpdateValue);
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
