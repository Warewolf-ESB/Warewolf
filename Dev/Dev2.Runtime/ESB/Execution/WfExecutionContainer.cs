/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Threading;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.WF;
using Dev2.Runtime.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Workspaces;
using Dev2.Data;

namespace Dev2.Runtime.ESB.Execution
{
    public class WfExecutionContainer : EsbExecutionContainer
    {


        public WfExecutionContainer(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        /// <summary>
        /// Executes the specified errors.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <param name="update"></param>
        /// <returns></returns>
        public override Guid Execute(out ErrorResultTO errors, int update)
        {
            errors = new ErrorResultTO();
            var result = GlobalConstants.NullDataListID;
            DataObject.ExecutionID = DataObject.ExecutionID ?? Guid.NewGuid();
            var user = Thread.CurrentPrincipal;
            if (string.IsNullOrEmpty(DataObject.WebUrl))
            {
                DataObject.WebUrl = $"{EnvironmentVariables.WebServerUri}secure/{DataObject.ServiceName}.{DataObject.ReturnType}?" + DataObject.QueryString;
            }
            var dataObjectExecutionId = DataObject.ExecutionID.ToString();
            if (!DataObject.IsSubExecution)
            {
                Dev2Logger.Debug(string.Format(GlobalConstants.ExecuteWebRequestString, DataObject.ServiceName, user?.Identity?.Name, user?.Identity?.AuthenticationType, user?.Identity?.IsAuthenticated, DataObject.RawPayload), dataObjectExecutionId);
                Dev2Logger.Debug("Request URL [ " + DataObject.WebUrl + " ]", dataObjectExecutionId);
            }
            Dev2Logger.Debug("Entered Wf Container", dataObjectExecutionId);
            DataObject.ServiceName = ServiceAction.ServiceName;

            if (DataObject.ServerID == Guid.Empty)
            {
                DataObject.ServerID = HostSecurityProvider.Instance.ServerID;
            }
            var executionForServiceString = string.Format(GlobalConstants.ExecutionForServiceString, DataObject.ServiceName, DataObject.ResourceID, (DataObject.IsDebug ? "Debug" : "Execute"));
            Dev2Logger.Info("Started " + executionForServiceString, dataObjectExecutionId);
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
            var userPrinciple = Thread.CurrentPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => { result = ExecuteWf(); });
            foreach (var err in DataObject.Environment.Errors)
            {
                errors.AddError(err, true);
            }
            foreach (var err in DataObject.Environment.AllErrors)
            {
                errors.AddError(err, true);
            }

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
                    wfappUtils.DispatchDebugState(DataObject, StateType.Start, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, true, false, false);
                }
                if (CanExecute(DataObject.ResourceID, DataObject, AuthorizationContext.Execute))
                {
                    Eval(DataObject.ResourceID, DataObject);
                }
                if (DataObject.IsDebugMode())
                {
                    wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
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
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, DataObject.ExecutionID.ToString());
                DataObject.Environment.AddError(ex.Message);
                wfappUtils.DispatchDebugState(DataObject, StateType.End, DataObject.Environment.HasErrors(), DataObject.Environment.FetchErrors(), out invokeErrors, DataObject.StartTime, false, true);
            }
            return result;
        }

        public override bool CanExecute(Guid resourceId, IDSFDataObject dataObject, AuthorizationContext authorizationContext)
        {
            var isAuthorized = ServerAuthorizationService.Instance.IsAuthorized(dataObject.ExecutingUser, authorizationContext, resourceId.ToString());
            if (!isAuthorized)
            {
                dataObject.Environment.AddError(string.Format(Warewolf.Resource.Errors.ErrorResource.UserNotAuthorizedToExecuteException, dataObject.ExecutingUser.Identity.Name, dataObject.ServiceName));
            }
            return isAuthorized;
        }


        public void Eval(DynamicActivity flowchartProcess, IDSFDataObject dsfDataObject, int update)
        {
            var resource = new ActivityParser().Parse(flowchartProcess);

            EvalInner(dsfDataObject, resource, update);
        }

        void Eval(Guid resourceID, IDSFDataObject dataObject)
        {
            Dev2Logger.Debug("Getting Resource to Execute", dataObject.ExecutionID.ToString());
            var resource = ResourceCatalog.Instance.Parse(TheWorkspace.ID, resourceID, dataObject.ExecutionID.ToString());
            Dev2Logger.Debug("Got Resource to Execute", dataObject.ExecutionID.ToString());
            EvalInner(dataObject, resource, dataObject.ForEachUpdateValue);

        }
        public override IDSFDataObject Execute(IDSFDataObject inputs, IDev2Activity activity) => null;

        static void EvalInner(IDSFDataObject dsfDataObject, IDev2Activity resource, int update)
        {
            bool stoppedExecution = false;
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

                WorkflowExecutionWatcher.HasAWorkflowBeenExecuted = true;

                // TODO: if we wanted to skip to a particular part of the execution we need to
                //       arrange for "resource" to be set to the correct activity and load the
                //       old environment
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
            catch (Exception e)
            {
                throw;
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
    }
}
