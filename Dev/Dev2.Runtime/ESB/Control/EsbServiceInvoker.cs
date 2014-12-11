
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;

// ReSharper disable CheckNamespace
namespace Dev2.Runtime.ESB
// ReSharper restore CheckNamespace
{

    #region Invokes Endpoint and returns responses to the Caller


    public class EsbServiceInvoker : IEsbServiceInvoker, IDisposable
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

        public EsbServiceInvoker()
        {
        }

        public EsbServiceInvoker(IEsbChannel esbChannel,
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

        public EsbServiceInvoker(IEsbChannel esbChannel,
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
            var result = GlobalConstants.NullDataListID;
            var compiler = DataListFactory.CreateDataListCompiler();
            var time = new System.Diagnostics.Stopwatch();
            time.Start();
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
                var serviceId = dataObject.ResourceID;

                // we need to get better at getting this ;)

                var serviceName = dataObject.ServiceName;
                if(serviceId == Guid.Empty && string.IsNullOrEmpty(serviceName))
                {
                    errors.AddError(Warewolf.Studio.Resources.Languages.Services.DynamicServiceError_ServiceNotSpecified);
                }
                else
                {

                    try
                    {
                        var sl = new ServiceLocator();
                        var theService = serviceId == Guid.Empty ? sl.FindService(serviceName, _workspace.ID) : sl.FindService(serviceId, _workspace.ID);

                        if(theService == null)
                        {
                            errors.AddError("Service [ " + serviceName + " ] not found.");
                        }
                        else if(theService.Actions.Count <= 1)
                        {
                            #region Execute ESB container

                            var theStart = theService.Actions.FirstOrDefault();
                            if(theStart != null && ((theStart.ActionType != Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService &&
                                                     theStart.ActionType != Common.Interfaces.Core.DynamicServices.enActionType.Workflow) && dataObject.IsFromWebServer))
                            {
                                throw new Exception("Can only execute workflows from web browser");
                            }

                            MapServiceActionDependencies(theStart, sl);

                            // Invoke based upon type ;)
                            if(theStart != null)
                            {
                                theStart.DataListSpecification = theService.DataListSpecification;
                                var container = GenerateContainer(theStart, dataObject, _workspace);
                                ErrorResultTO invokeErrors;
                                result = container.Execute(out invokeErrors);
                                errors.MergeErrors(invokeErrors);
                            }
                            #endregion
                        }
                        else
                        {
                            errors.AddError("Malformed Service [ " + serviceId + " ] it contains multiple actions");
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
                            Dev2Logger.Log.Error(errors.MakeDisplayReady());
                        }
                    }
                }
            }
            finally
            {
                time.Stop();
                Common.Interfaces.ServerStats.IncrementTotalRequests();
                Common.Interfaces.ServerStats.IncrementTotalTime(time.ElapsedMilliseconds);
                // BUG 9706 - 2013.06.22 - TWR : added
                DispatchDebugErrors(errors, dataObject, StateType.End);
            }
            return result;
        }

        /// <summary>
        /// Generates the invoke container.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="serviceId">The service unique identifier.</param>
        /// <param name="isLocalInvoke">if set to <c>true</c> [is local invoke].</param>
        /// <param name="masterDataListId">The master data list unique identifier.</param>
        /// <returns></returns>
        public EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, Guid serviceId, bool isLocalInvoke, Guid masterDataListId = default(Guid))
        {
            if(isLocalInvoke)
            {
                ServiceLocator sl = new ServiceLocator();
                var theService = sl.FindService(serviceId, _workspace.ID);
                EsbExecutionContainer executionContainer = null;


                if(theService != null && theService.Actions.Any())
                {
                    var sa = theService.Actions.FirstOrDefault();
                    MapServiceActionDependencies(sa, sl);
                    executionContainer = GenerateContainer(sa, dataObject, _workspace);
                }

                return executionContainer;
            }
            // we need a remote container ;)
            // TODO : Set Output description for shaping ;)
            return GenerateContainer(new ServiceAction { ActionType = Common.Interfaces.Core.DynamicServices.enActionType.RemoteService }, dataObject, null);
        }

        /// <summary>
        /// Generates the invoke container.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="isLocalInvoke">if set to <c>true</c> [is local invoke].</param>
        /// <param name="masterDataListId">The master data list unique identifier.</param>
        /// <returns></returns>
        public EsbExecutionContainer GenerateInvokeContainer(IDSFDataObject dataObject, String serviceName, bool isLocalInvoke, Guid masterDataListId = default(Guid))
        {
            if(isLocalInvoke)
            {
                ServiceLocator sl = new ServiceLocator();
                var resourceId = dataObject.ResourceID;
                DynamicService theService = GetService(serviceName, resourceId, sl);
                EsbExecutionContainer executionContainer = null;


                if(theService != null && theService.Actions.Any())
                {
                    var sa = theService.Actions.FirstOrDefault();
                    MapServiceActionDependencies(sa, sl);
                    executionContainer = GenerateContainer(sa, dataObject, _workspace);
                }

                return executionContainer;
            }
            // we need a remote container ;)
            // TODO : Set Output description for shaping ;)
            return GenerateContainer(new ServiceAction { ActionType = Common.Interfaces.Core.DynamicServices.enActionType.RemoteService }, dataObject, null);
        }

        DynamicService GetService(string serviceName, Guid resourceId, ServiceLocator sl)
        {
            try
            {
                if(resourceId == Guid.Empty)
                {
                    return sl.FindService(serviceName, _workspace.ID) ?? sl.FindService(serviceName, GlobalConstants.ServerWorkspaceID); //Check the workspace is it something we are working on if not use the server version
                }
                return sl.FindService(resourceId, _workspace.ID) ?? sl.FindService(resourceId, GlobalConstants.ServerWorkspaceID); //Check the workspace is it something we are working on if not use the server version
            }catch(Exception)
            {
                //Internal services
                return null;
            }
        }

        private EsbExecutionContainer GenerateContainer(ServiceAction serviceAction, IDSFDataObject dataObj, IWorkspace theWorkspace)
        {
            // set the ID for later use ;)
            dataObj.WorkspaceID = _workspace.ID;

            EsbExecutionContainer result = null;

            switch(serviceAction.ActionType)
            {
                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService:
                    result = new InternalServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel, _request);
                    break;

                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeStoredProc:
                    result = new DatabaseServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeWebService:
                    result = new WebServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;

                case Common.Interfaces.Core.DynamicServices.enActionType.Plugin:
                    result = new PluginServiceContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;

                case Common.Interfaces.Core.DynamicServices.enActionType.Workflow:
                    result = new WfExecutionContainer(serviceAction, dataObj, theWorkspace, _esbChannel);
                    break;

                case Common.Interfaces.Core.DynamicServices.enActionType.RemoteService:
                    result = new RemoteWorkflowExecutionContainer(serviceAction, dataObj, null, _esbChannel);
                    break;
            }

            return result;
        }

        private void MapServiceActionDependencies(ServiceAction serviceAction, ServiceLocator serviceLocator)
        {

            serviceAction.Service = GetService(serviceAction.ServiceName, serviceAction.ServiceID, serviceLocator);
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
            if(errors.HasErrors() && (dataObject.IsDebugMode()))
            {
                Guid parentInstanceId;
                Guid.TryParse(dataObject.ParentInstanceID, out parentInstanceId);

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
