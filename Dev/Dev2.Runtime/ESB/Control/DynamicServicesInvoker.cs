#region Change Log

//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    The data layer of the Dynamic Service Engine
//                  This is where all actions get executed.

#endregion

using System;
using System.Linq;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;
using enActionType = Dev2.DynamicServices.enActionType;

namespace Dev2.Runtime.ESB
{

    #region Dynamic Invocation Class - Invokes Dynamic Endpoint and returns responses to the Caller


    public class DynamicServicesInvoker : IDynamicServicesInvoker, IDisposable
    {
        #region Fields
        private readonly IEsbChannel _esbChannel;

        private readonly IWorkspace _workspace;

        private readonly EsbExecuteRequest _request;

        #endregion

        // 2012.10.17 - 5782: TWR - Changed to work off the workspace host and made read only

        public bool IsLoggingEnabled
        {
            get { return true; }
        }

        #region Constructors

        public DynamicServicesInvoker()
        {
        }

        public DynamicServicesInvoker(IEsbChannel esbChannel,
                                      IFrameworkDuplexDataChannel managementChannel,
                                      IWorkspace workspace, EsbExecuteRequest request)
        {
            _esbChannel = esbChannel;
            if(managementChannel != null)
            {
            }

            // 2012.10.17 - 5782: TWR - Added workspace parameter
            _workspace = workspace;

            _request = request;
        }

        public DynamicServicesInvoker(IEsbChannel esbChannel,
                                      IFrameworkDuplexDataChannel managementChannel,
                                      IWorkspace workspace)
            : this(esbChannel, managementChannel, workspace, null)
        {

        }

        #endregion

        #region Travis New Methods

        /// <summary>
        /// Invokes the specified service as per the dataObject against theHost
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Can only execute workflows from web browser</exception>
        public Guid Invoke(IDSFDataObject dataObject, out ErrorResultTO errors)
        {
            Guid result = GlobalConstants.NullDataListID;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            errors = new ErrorResultTO();

            // BUG 9706 - 2013.06.22 - TWR : added pre debug dispatch
            if(compiler.HasErrors(dataObject.DataListID))
            {
                errors.AddError(compiler.FetchErrors(dataObject.DataListID));
                DispatchDebugErrors(errors, dataObject, StateType.Before);
            }
            errors.ClearErrors();
            try
            {
                Guid serviceID = dataObject.ResourceID;

                // we need to get better at getting this ;)

                string serviceName = dataObject.ServiceName;
                if(serviceID == Guid.Empty && string.IsNullOrEmpty(serviceName))
                {
                    errors.AddError(Resources.DynamicServiceError_ServiceNotSpecified);
                }
                else
                {

                    try
                    {
                        var sl = new ServiceLocator();
                        DynamicService theService;
                        if(serviceID == Guid.Empty)
                        {
                            theService = sl.FindService(serviceName, _workspace.ID);
                        }
                        else
                        {
                            theService = sl.FindService(serviceID, _workspace.ID);
                        }

                        if(theService == null)
                        {
                            errors.AddError("Service [ " + serviceName + " ] not found.");
                        }
                        else if(theService.Actions.Count <= 1)
                        {
                            #region Execute ESB container

                            ServiceAction theStart = theService.Actions.FirstOrDefault();
                            if(theStart != null && ((theStart.ActionType != enActionType.InvokeManagementDynamicService &&
                                                     theStart.ActionType != enActionType.Workflow) && dataObject.IsFromWebServer))
                            {
                                throw new Exception("Can only execute workflows from web browser");
                            }

                            MapServiceActionDependencies(theStart, sl);

                            ErrorResultTO invokeErrors;
                            // Invoke based upon type ;)
                            theStart.DataListSpecification = theService.DataListSpecification;
                            EsbExecutionContainer container = GenerateContainer(theStart, dataObject, _workspace);
                            result = container.Execute(out invokeErrors);
                            errors.MergeErrors(invokeErrors);

                            #endregion
                        }
                        else
                        {
                            errors.AddError("Malformed Service [ " + serviceID + " ] it contains multiple actions");
                        }
                    }
                    catch(Exception e)
                    {
                        errors.AddError(e.Message);
                    }
                    finally
                    {
                        ErrorResultTO tmpErrors;
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error,
                        errors.MakeDataListReady(), out tmpErrors);

                        if(errors.HasErrors())
                        {
                            this.LogError(errors.MakeDisplayReady());
                        }
                    }
                }
            }
            finally
            {
                // BUG 9706 - 2013.06.22 - TWR : added
                DispatchDebugErrors(errors, dataObject, StateType.End);
            }
            return result;
        }

        /// <summary>
        /// Generates the invoke container.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="serviceID">The service unique identifier.</param>
        /// <param name="isLocalInvoke">if set to <c>true</c> [is local invoke].</param>
        /// <param name="masterDataListID">The master data list unique identifier.</param>
        /// <returns></returns>
        public EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, Guid serviceID, bool isLocalInvoke, Guid masterDataListID = default(Guid))
        {
            if(isLocalInvoke)
            {
                ServiceLocator sl = new ServiceLocator();
                DynamicService theService = sl.FindService(serviceID, _workspace.ID);
                EsbExecutionContainer executionContainer = null;


                if(theService != null && theService.Actions.Any())
                {
                    ServiceAction sa = theService.Actions.FirstOrDefault();
                    MapServiceActionDependencies(sa, sl);
                    executionContainer = GenerateContainer(sa, dataObject, _workspace);
                }

                return executionContainer;
            }
            // we need a remote container ;)
            // TODO : Set Output description for shaping ;)
            return GenerateContainer(new ServiceAction { ActionType = enActionType.RemoteService }, dataObject, null);
        }

        /// <summary>
        /// Generates the invoke container.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="isLocalInvoke">if set to <c>true</c> [is local invoke].</param>
        /// <param name="masterDataListID">The master data list unique identifier.</param>
        /// <returns></returns>
        public EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListID = default(Guid))
        {
            if(isLocalInvoke)
            {
                ServiceLocator sl = new ServiceLocator();
                DynamicService theService = sl.FindService(serviceName, _workspace.ID);
                EsbExecutionContainer executionContainer = null;


                if(theService != null && theService.Actions.Any())
                {
                    ServiceAction sa = theService.Actions.FirstOrDefault();
                    MapServiceActionDependencies(sa, sl);
                    executionContainer = GenerateContainer(sa, dataObject, _workspace);
                }

                return executionContainer;
            }
            // we need a remote container ;)
            // TODO : Set Output description for shaping ;)
            return GenerateContainer(new ServiceAction { ActionType = enActionType.RemoteService }, dataObject, null);
        }

        private EsbExecutionContainer GenerateContainer(ServiceAction serviceAction, IDSFDataObject dataObj, IWorkspace theWorkspace)
        {
            // set the ID for later use ;)
            dataObj.WorkspaceID = _workspace.ID;

            EsbExecutionContainer result = null;

            switch(serviceAction.ActionType)
            {
                case enActionType.InvokeManagementDynamicService:
                    result = new InternalServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel, _request);
                    break;

                case enActionType.InvokeStoredProc:
                    result = new DatabaseServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;
                case enActionType.InvokeWebService:
                    result = new WebServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;

                case enActionType.Plugin:
                    result = new PluginServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;

                case enActionType.Workflow:
                    result = new WfExecutionContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;

                case enActionType.RemoteService:
                    result = new RemoteWorkflowExecutionContainer(serviceAction, dataObj, null, _esbChannel);
                    break;
            }

            return result;
        }

        private void MapServiceActionDependencies(ServiceAction serviceAction, ServiceLocator serviceLocator)
        {

            if(serviceAction.ServiceID == Guid.Empty)
            {
                if(!string.IsNullOrWhiteSpace(serviceAction.ServiceName))
                {
                    serviceAction.Service = serviceLocator.FindService(serviceAction.ServiceName, _workspace.ID);
                }
            }
            else
            {
                serviceAction.Service = serviceLocator.FindService(serviceAction.ServiceID, _workspace.ID);
            }

            if(!string.IsNullOrWhiteSpace(serviceAction.SourceName))
            {
                serviceAction.Source = serviceLocator.FindSourceByName(serviceAction.SourceName, _workspace.ID);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion

        #region DispatchDebugErrors

        // BUG 9706 - 2013.06.22 - TWR : refactored
        void DispatchDebugErrors(ErrorResultTO errors, IDSFDataObject dataObject, StateType stateType)
        {
            if(errors.HasErrors() && (dataObject.IsDebug || dataObject.RemoteInvoke))
            {
                Guid parentInstanceID;
                Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceID);

                var debugState = new DebugState
                {
                    ID = dataObject.DataListID,
                    ParentID = parentInstanceID,
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
                    ClientID = dataObject.ClientID,
                    Server = string.Empty,
                    Version = string.Empty,
                    Name = GetType().Name,
                    HasError = errors.HasErrors(),
                    ErrorMessage = errors.MakeDisplayReady()
                };

                DebugDispatcher.Instance.Write(debugState, dataObject.RemoteInvoke, dataObject.RemoteInvokerID);
            }
        }

        #endregion

    }

    #endregion
}
