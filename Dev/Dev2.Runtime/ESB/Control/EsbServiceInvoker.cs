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

using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dev2.Runtime.ESB.Management;
using Warewolf.Esb;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB

{
    #region Invokes Endpoint and returns responses to the Caller

    public class EsbServiceInvoker : IEsbServiceInvoker, IDisposable
    {
        readonly IServiceLocator _serviceLocator;

        #region Fields

        readonly IEsbChannel _esbChannel;

        readonly IWorkspace _workspace;

        readonly EsbExecuteRequest _request;

        #endregion Fields

        readonly ConcurrentDictionary<Guid, ServiceAction> _cache = new ConcurrentDictionary<Guid, ServiceAction>();

        #region Constructors

        public EsbServiceInvoker(IEsbChannel esbChannel, IWorkspace workspace)
            : this(esbChannel, workspace, null)
        {
        }

        public EsbServiceInvoker(IEsbChannel esbChannel, IWorkspace workspace, EsbExecuteRequest request)
            : this(new ServiceLocator())
        {
            _esbChannel = esbChannel;

            _workspace = workspace;

            _request = request;
        }

        EsbServiceInvoker(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        #endregion Constructors
                
        public Guid Invoke(IDSFDataObject dataObject, out ErrorResultTO errors, IInternalExecutionContext internalExecutionContext)
        {
            var result = GlobalConstants.NullDataListID;
            var time = new Stopwatch();
            time.Start();
            errors = new ErrorResultTO();
            if (dataObject.Environment.HasErrors())
            {
                errors.AddError(dataObject.Environment.FetchErrors());
                DispatchDebugErrors(errors, dataObject, StateType.Before);
            }
            errors.ClearErrors();
            try
            {
                errors = TryInvokeService(dataObject, errors, internalExecutionContext);
            }
            finally
            {
                time.Stop();
                ServerStats.IncrementTotalRequests();
                ServerStats.IncrementTotalTime(time.ElapsedMilliseconds);
                DispatchDebugErrors(errors, dataObject, StateType.End);
            }
            return result;
        }

        ErrorResultTO TryInvokeService(IDSFDataObject dataObject, ErrorResultTO errors, IInternalExecutionContext internalExecutionContext)
        {
            var serviceId = dataObject.ResourceID;

            var serviceName = dataObject.ServiceName;
            if (serviceId == Guid.Empty && string.IsNullOrEmpty(serviceName))
            {
                errors.AddError(Resources.DynamicServiceError_ServiceNotSpecified);
            }
            else
            {
                try
                {
                    errors = InvokeService(dataObject, errors, serviceId, serviceName, internalExecutionContext);
                }
                catch (Exception e)
                {
                    errors.AddError(e.Message);
                }
                finally
                {
                    var environment = dataObject.Environment;
                    if (environment.HasErrors())
                    {
                        var errorString = environment.FetchErrors();
                        var executionErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                        errors.MergeErrors(executionErrors);
                    }

                    environment.AddError(errors.MakeDataListReady());

                    if (errors.HasErrors())
                    {
                        Dev2Logger.Error(errors.MakeDisplayReady(), GlobalConstants.WarewolfError);
                    }
                }
            }

            return errors;
        }

        private ErrorResultTO InvokeService(IDSFDataObject dataObject, ErrorResultTO errors, Guid serviceId, string serviceName, IInternalExecutionContext internalExecutionContext)
        {
            Dev2Logger.Debug("Finding service", dataObject.ExecutionID.ToString());
            var theService = serviceId == Guid.Empty ? _serviceLocator.FindService(serviceName, _workspace.ID) : _serviceLocator.FindService(serviceId, _workspace.ID);

            if (theService == null)
            {
                if (!dataObject.IsServiceTestExecution)
                {
                    theService = _serviceLocator.FindService(serviceName, GlobalConstants.ServerWorkspaceID);
                }
                if (theService == null)
                {
                    if (dataObject.IsServiceTestExecution)
                    {
                        var testResult = new ServiceTestModelTO
                        {
                            Result = new TestRunResult
                            {
                                RunTestResult = RunResult.TestResourceDeleted,
                                Message = "Resource has been deleted",
                                DebugForTest = new List<IDebugState>(),
                                TestName = dataObject.TestName
                            },
                            TestPassed = false,
                            TestInvalid = true,
                            FailureMessage = "Resource has been deleted",
                            TestName = dataObject.TestName,
                        };
                        var ser = new Dev2JsonSerializer();
                        _request.ExecuteResult = ser.SerializeToBuilder(testResult);
                    }

                    errors.AddError(string.Format(ErrorResource.ServiceNotFound, serviceName));
                }
            }
            else if (theService.Actions.Count <= 1)
            {
                #region Execute ESB container

                var theStart = theService.Actions.FirstOrDefault();
                if (theStart != null && theStart.ActionType != enActionType.InvokeManagementDynamicService && theStart.ActionType != enActionType.Workflow && dataObject.IsFromWebServer)
                {
                    throw new Exception(ErrorResource.CanOnlyExecuteWorkflowsFromWebBrowser);
                }
                Dev2Logger.Debug("Mapping Action Dependencies", dataObject.ExecutionID.ToString());
                MapServiceActionDependencies(theStart);

                if (theStart != null)
                {
                    theStart.Service = theService;
                    theStart.DataListSpecification = theService.DataListSpecification;
                    Dev2Logger.Debug("Getting container", dataObject.ExecutionID.ToString());
                    var container = GenerateContainer(theStart, dataObject, _workspace, internalExecutionContext);
                    container.Execute(out errors, 0);
                }

                #endregion Execute ESB container
            }
            else
            {
                errors.AddError(string.Format(ErrorResource.MalformedService, serviceId));
            }

            return errors;
        }

        public IEsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, Guid serviceId, bool isLocalInvoke) => GenerateInvokeContainer(dataObject, serviceId, isLocalInvoke, default(Guid));

        public IEsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, Guid serviceId, bool isLocal, Guid masterDataListId)
        {
            if (isLocal)
            {
                ServiceAction sa;
                if (_cache.ContainsKey(dataObject.ResourceID))
                {
                    sa = _cache[dataObject.ResourceID];
                    return GenerateContainer(sa, dataObject, _workspace);
                }

                var theService = _serviceLocator.FindService(serviceId, _workspace.ID);
                if (theService != null && theService.Actions.Any())
                {
                    sa = theService.Actions.FirstOrDefault();
                    if (sa != null)
                    {
                        MapServiceActionDependencies(sa);
                        sa.Service = theService;
                        _cache.TryAdd(dataObject.ResourceID, sa);
                        return GenerateContainer(sa, dataObject, _workspace);
                    }
                }

                return null;
            }
            return GenerateContainer(new ServiceAction { ActionType = enActionType.RemoteService }, dataObject, null);
        }

        public IEsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, string serviceName, bool isLocalInvoke) => GenerateInvokeContainer(dataObject, serviceName, isLocalInvoke, default(Guid));

        public IEsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, string serviceName, bool isLocalInvoke, Guid masterDataListId)
        {
            if (isLocalInvoke)
            {
                if (_cache.ContainsKey(dataObject.ResourceID))
                {
                    var sa = _cache[dataObject.ResourceID];
                    return GenerateContainer(sa, dataObject, _workspace);
                }
                else
                {
                    return GenerateInvokeContainerAndCache(dataObject, serviceName);
                }
            }
            return GenerateContainer(new ServiceAction { ActionType = enActionType.RemoteService }, dataObject, null);
        }

        private IEsbExecutionContainer GenerateInvokeContainerAndCache(IDSFDataObject dataObject, string serviceName)
        {
            var resourceId = dataObject.ResourceID;
            Dev2Logger.Debug($"Getting DynamicService: {serviceName}", dataObject.ExecutionID.ToString());
            var theService = GetService(serviceName, resourceId);
            IEsbExecutionContainer executionContainer = null;

            if (theService != null && theService.Actions.Any())
            {
                var sa = theService.Actions.FirstOrDefault();
                if (sa != null)
                {
                    MapServiceActionDependencies(sa);
                    sa.Service = theService;
                    _cache.TryAdd(dataObject.ResourceID, sa);
                    executionContainer = GenerateContainer(sa, dataObject, _workspace);
                }
            }

            return executionContainer;
        }

        DynamicService GetService(string serviceName, Guid resourceId)
        {
            try
            {
                if (resourceId == Guid.Empty)
                {
                    return _serviceLocator.FindService(serviceName, _workspace.ID) ?? _serviceLocator.FindService(serviceName, GlobalConstants.ServerWorkspaceID); //Check the workspace is it something we are working on if not use the server version
                }
                return _serviceLocator.FindService(resourceId, _workspace.ID) ?? _serviceLocator.FindService(resourceId, GlobalConstants.ServerWorkspaceID); //Check the workspace is it something we are working on if not use the server version
            }
            catch (Exception)
            {
                //Internal services
                return null;
            }
        }

        IEsbExecutionContainer GenerateContainer(ServiceAction serviceAction, IDSFDataObject dataObj, IWorkspace theWorkspace)
        {
            return GenerateContainer(serviceAction, dataObj, theWorkspace, null);
        }
        IEsbExecutionContainer GenerateContainer(ServiceAction serviceAction, IDSFDataObject dataObj, IWorkspace theWorkspace, IInternalExecutionContext internalExecutionContext)
        {
            // set the ID for later use ;)
            dataObj.WorkspaceID = _workspace.ID;

            IEsbExecutionContainer result = null;
            if (dataObj.IsServiceTestExecution)
            {
                result = new ServiceTestExecutionContainer(serviceAction, dataObj, _workspace, _esbChannel, _request);
            }
            else
            {
                switch (serviceAction.ActionType)
                {
                    case enActionType.InvokeManagementDynamicService:
                        result = new InternalServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel, _request, internalExecutionContext);
                        break;

                    case enActionType.Workflow:
                        result = new PerfmonExecutionContainer(new WfExecutionContainer(serviceAction, dataObj, theWorkspace, _esbChannel));
                        break;

                    case enActionType.RemoteService:
                        result = new RemoteWorkflowExecutionContainer(serviceAction, dataObj, null, _esbChannel);
                        break;

                    default:
                        result = null;
                        break;
                }
            }
            return result;
        }

        void MapServiceActionDependencies(ServiceAction serviceAction)
        {
            if (!string.IsNullOrWhiteSpace(serviceAction?.SourceName))
            {
                serviceAction.Source = _serviceLocator.FindSourceByName(serviceAction.SourceName, _workspace.ID);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion IDisposable Members

        #region DispatchDebugErrors

        void DispatchDebugErrors(ErrorResultTO errors, IDSFDataObject dataObject, StateType stateType)
        {
            if (errors.HasErrors() && dataObject.IsDebugMode())
            {
                Guid.TryParse(dataObject.ParentInstanceID, out Guid parentInstanceId);

                var debugState = new DebugState
                {
                    ID = dataObject.DataListID,
                    ParentID = parentInstanceId,
                    WorkspaceID = dataObject.WorkspaceID,
                    StateType = stateType,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    ActivityType = ActivityType.Workflow,
                    DisplayName = dataObject.ServiceName,
                    IsSimulation = dataObject.IsOnDemandSimulation,
                    ServerID = dataObject.ServerID,
                    OriginatingResourceID = dataObject.ResourceID,
                    OriginalInstanceID = dataObject.OriginalInstanceID,
                    SessionID = dataObject.DebugSessionID,
                    EnvironmentID = dataObject.EnvironmentID,
                    SourceResourceID = dataObject.SourceResourceID,
                    ClientID = dataObject.ClientID,
                    Server = string.Empty,
                    Version = string.Empty,
                    Name = GetType().Name,
                    HasError = errors.HasErrors(),
                    ErrorMessage = errors.MakeDisplayReady()
                };

                DebugDispatcher.Instance.Write( new WriteArgs { debugState = debugState, isTestExecution = dataObject.IsServiceTestExecution, isDebugFromWeb = dataObject.IsDebugFromWeb, testName = dataObject.TestName, isRemoteInvoke = dataObject.RemoteInvoke, remoteInvokerId = dataObject.RemoteInvokerID });
            }
        }

        #endregion DispatchDebugErrors
    }

    #endregion Invokes Endpoint and returns responses to the Caller
}
