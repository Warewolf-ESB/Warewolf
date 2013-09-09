#region Change Log

//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    The data layer of the Dynamic Service Engine
//                  This is where all actions get executed.

#endregion

using System;
using System.Linq;
using System.Transactions;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
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

        private readonly IFrameworkDuplexDataChannel _managementChannel;
        private readonly IWorkspace _workspace;

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
                                      IWorkspace workspace)
        {
            _esbChannel = esbChannel;
            if(managementChannel != null)
            {
                _managementChannel = managementChannel;
            }

            // 2012.10.17 - 5782: TWR - Added workspace parameter
            _workspace = workspace;
        }

        #endregion

        #region Travis New Methods

        /// <summary>
        /// Invokes the specified service as per the dataObject against theHost
        /// </summary>
        /// <param name="theHost">The host.</param>
        /// <param name="dataObject">The data object.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
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
                string serviceName = dataObject.ServiceName;
                if(string.IsNullOrEmpty(serviceName))
                {
                    errors.AddError(Resources.DynamicServiceError_ServiceNotSpecified);
                }
                else
                {
                    // Place into a transactional scope
                    using(var transactionScope = new TransactionScope())
                    {
                        try
                        {
                            var sl = new ServiceLocator();
                            var theService = sl.FindServiceByName(serviceName, _workspace.ID);

                            if(theService == null)
                            {
                                errors.AddError("Service [ " + serviceName + " ] not found.");
                            }
                            else if(theService.Actions.Count <= 1)
                            {
                                #region Execute ESB container

                                ServiceAction theStart = theService.Actions.FirstOrDefault();
                                if((theStart.ActionType != enActionType.InvokeManagementDynamicService && theStart.ActionType != enActionType.Workflow) && dataObject.IsFromWebServer)
                                {
                                    throw new Exception("Can only execute workflows from web browser");
                                }
                                MapServiceActionDependencies(theStart, sl);

                                ErrorResultTO invokeErrors = new ErrorResultTO();
                                // Invoke based upon type ;)
                                EsbExecutionContainer container = GenerateContainer(theStart, dataObject, _workspace);
                                result = container.Execute(out invokeErrors);
                                errors.MergeErrors(invokeErrors);

                                // Ensure there are no errors so we can complete transaction
                                if(!compiler.HasErrors(dataObject.DataListID) && result != GlobalConstants.NullDataListID && !invokeErrors.HasErrors())
                                {
                                    transactionScope.Complete();
                                }

                                #endregion
                            }
                            else
                            {
                                errors.AddError("Malformed Service [ " + serviceName + " ] it contains multiple actions");
                            }
                        }
                        catch(Exception e)
                        {
                            errors.AddError(e.Message);
                        }
                        finally
                        {
                            ErrorResultTO tmpErrors;
                            compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, errors.MakeDataListReady(), out tmpErrors);
                            transactionScope.Dispose();
                        }
                    }
                }
            }
            finally
            {
                // BUG 9706 - 2013.06.22 - TWR : added
                DispatchDebugErrors(errors, dataObject, StateType.After);
            }

            return result;
        }

        /// <summary>
        /// Generates the invoke container.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        public EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, string serviceName, bool isLocalInvoke)
        {
            if(isLocalInvoke)
            {
                ServiceLocator sl = new ServiceLocator();
                DynamicService theService = sl.FindServiceByName(serviceName, _workspace.ID);
                EsbExecutionContainer executionContainer = null;


                if(theService != null && theService.Actions.Any())
                {
                    ServiceAction sa = theService.Actions.FirstOrDefault();
                    MapServiceActionDependencies(sa, sl);
                    executionContainer = GenerateContainer(sa, dataObject, _workspace);
                }

                return executionContainer;
            }
            else
            {
                // we need a remote container ;)
                // TODO : Set Output description for shaping ;)
                return GenerateContainer(new ServiceAction() { ActionType = enActionType.RemoteService }, dataObject, null);
            }
        }

        private EsbExecutionContainer GenerateContainer(ServiceAction serviceAction, IDSFDataObject dataObj, IWorkspace theWorkspace)
        {
            // set the ID for later use ;)
            dataObj.WorkspaceID = _workspace.ID;

            EsbExecutionContainer result = null;

            switch(serviceAction.ActionType)
            {
                case enActionType.InvokeManagementDynamicService:
                    result = new InternalServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
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
            
            if(!string.IsNullOrWhiteSpace(serviceAction.ServiceName))
            {
                serviceAction.Service = serviceLocator.FindServiceByName(serviceAction.ServiceName, _workspace.ID);
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
            //throw new NotImplementedException();
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
