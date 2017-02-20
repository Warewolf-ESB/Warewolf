/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming
namespace Dev2.Runtime.ESB.Control
{

    /// <summary>
    /// Amended as per PBI 7913
    /// </summary>
    /// IEsbActivityChannel
    public class EsbServicesEndpoint :  IEsbWorkspaceChannel
    {
        public EsbServicesEndpoint(IEnvironmentOutputMappingManager environmentOutputMappingManager)
        {
            _environmentOutputMappingManager = environmentOutputMappingManager;
        }

        public EsbServicesEndpoint()
            :this(new EnvironmentOutputMappingManager())
        {
            
        }
        private readonly IEnvironmentOutputMappingManager _environmentOutputMappingManager;
        private static WorkspaceRepository wRepository => WorkspaceRepository.Instance;
        private static ResourceCatalog rCatalog => ResourceCatalog.Instance;
        /// <summary>
        /// Executes the request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="request"></param>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errors)
        {

            var resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = null;
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () =>
            {
                theWorkspace = wRepository.Get(workspaceId);
            });
            // If no DLID, we need to make it based upon the request ;)
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                IResource resource;
                try
                {
                    resource = dataObject.ResourceID == Guid.Empty ? GetResource(workspaceId, dataObject.ServiceName) : GetResource(workspaceId, dataObject.ResourceID);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex);
                    errors.AddError(string.Format(ErrorResource.ServiceNotFound, dataObject.ServiceName));
                    return resultID;
                }

                if (resource?.DataList != null)
                {
                    Dev2Logger.Debug("Mapping Inputs from Environment");
                    ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(dataObject, dataObject.RawPayload, resource.DataList.ToString());
                }
                dataObject.RawPayload = new StringBuilder();

                // We need to create the parentID around the system ;)
                dataObject.ParentThreadID = Thread.CurrentThread.ManagedThreadId;

            }

            try
            {
                // Setup the invoker endpoint ;)
                Dev2Logger.Debug("Creating Invoker");
                using (var invoker = new EsbServiceInvoker(this,theWorkspace, request))
                {
                    // Should return the top level DLID
                    ErrorResultTO invokeErrors;
                    resultID = invoker.Invoke(dataObject, out invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }
            return resultID;
        }


        private static IResource GetResource(Guid workspaceId, Guid resourceId)
        {
            var resource = rCatalog.GetResource(workspaceId, resourceId) ?? rCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

            return resource;
        }

        

        private static IResource GetResource(Guid workspaceId, string resourceName)
        {
            var resource = rCatalog.GetResource(workspaceId, resourceName) ?? rCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);
            return resource;
        }



        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors, int update)
        {
            errors = null;
            var theWorkspace = wRepository.Get(workspaceId);
            var executionContainer = CreateExecutionContainer(dataObject, theWorkspace);
            executionContainer.PerformLogExecution(uri, update);
        }

        private RemoteWorkflowExecutionContainer CreateExecutionContainer(IDSFDataObject dataObject, IWorkspace theWorkspace)
        {
            return new RemoteWorkflowExecutionContainer(null, dataObject, theWorkspace, this);
        }

        /// <summary>
        /// Executes the sub request.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="inputDefs">The input defs.</param>
        /// <param name="outputDefs">The output defs.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="update"></param>
        /// <param name="handleErrors"> buble up errors or not</param>
        /// <returns></returns>
        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors, int update, bool handleErrors)
        {
            var theWorkspace = wRepository.Get(workspaceId);
            var invoker = CreateEsbServicesInvoker(theWorkspace);
            ErrorResultTO invokeErrors;
            var oldID = dataObject.DataListID;
            errors = new ErrorResultTO();

            // local non-scoped execution ;)
            var isLocal = !dataObject.IsRemoteWorkflow();

            var principle = Thread.CurrentPrincipal;
            Dev2Logger.Info("SUB-EXECUTION USER CONTEXT IS [ " + principle.Identity.Name + " ] FOR SERVICE  [ " + dataObject.ServiceName + " ]");
            var oldStartTime = dataObject.StartTime;
            dataObject.StartTime = DateTime.Now;
            if (dataObject.RunWorkflowAsync)
            {

                ExecuteRequestAsync(dataObject, inputDefs, invoker, isLocal, oldID, out invokeErrors, update);
                dataObject.StartTime = oldStartTime;
                errors.MergeErrors(invokeErrors);
            }
            else
            {
                if (isLocal)
                {
                    if (GetResource(workspaceId, dataObject.ResourceID) == null && GetResource(workspaceId, dataObject.ServiceName) == null)
                    {
                        errors.AddError(string.Format(ErrorResource.ResourceNotFound, dataObject.ServiceName));
                        dataObject.StartTime = oldStartTime;
                        return null;
                    }
                }

                var executionContainer = invoker.GenerateInvokeContainer(dataObject, dataObject.ServiceName, isLocal, oldID);
                if (executionContainer != null)
                {
                    CreateNewEnvironmentFromInputMappings(dataObject, inputDefs, update);
                    if (!isLocal)
                    {
                        SetRemoteExecutionDataList(dataObject, executionContainer, errors);
                    }
                    if (!errors.HasErrors())
                    {
                        executionContainer.InstanceInputDefinition = inputDefs;
                        executionContainer.InstanceOutputDefinition = outputDefs;
                        executionContainer.Execute(out invokeErrors, update);
                        var env = UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dataObject, outputDefs, update, handleErrors, errors);

                        errors.MergeErrors(invokeErrors);
                        string errorString = dataObject.Environment.FetchErrors();
                        invokeErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                        errors.MergeErrors(invokeErrors);
                        dataObject.StartTime = oldStartTime;
                        return env;
                    }
                    errors.AddError(string.Format(ErrorResource.ResourceNotFound, dataObject.ServiceName));
                }
            }
            dataObject.StartTime = oldStartTime;
            return new ExecutionEnvironment();
        }

       

        public IExecutionEnvironment UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(IDSFDataObject dataObject, string outputDefs, int update, bool handleErrors, ErrorResultTO errors)
        {
            var executionEnvironment = _environmentOutputMappingManager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dataObject, outputDefs, update, handleErrors, errors);
            return executionEnvironment;
        }

        public void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update)
        {   
            var shapeDefinitionsToEnvironment = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs, update);
            dataObject.PushEnvironment(shapeDefinitionsToEnvironment);
        }

        private static void SetRemoteExecutionDataList(IDSFDataObject dataObject, IEsbExecutionContainer executionContainer, ErrorResultTO errors)
        {
            var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
            if (remoteContainer != null)
            {
                var fetchRemoteResource = remoteContainer.FetchRemoteResource(dataObject.ResourceID, dataObject.ServiceName, dataObject.IsDebugMode());
                if (fetchRemoteResource != null)
                {
                    fetchRemoteResource.DataList = fetchRemoteResource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                    var remoteDataList = fetchRemoteResource.DataList;
                    dataObject.RemoteInvokeResultShape = new StringBuilder(remoteDataList);
                    dataObject.ServiceName = fetchRemoteResource.ResourceCategory;
                }
                else
                {
                    var message = string.Format(ErrorResource.ServiceNotFound, dataObject.ServiceName);
                    errors.AddError(message);
                }
            }
        }

        private void ExecuteRequestAsync(IDSFDataObject dataObject, string inputDefs, IEsbServiceInvoker invoker, bool isLocal, Guid oldID, out ErrorResultTO invokeErrors, int update)
        {
            var clonedDataObject = dataObject.Clone();
            invokeErrors = new ErrorResultTO();
            var executionContainer = invoker.GenerateInvokeContainer(clonedDataObject, clonedDataObject.ServiceName, isLocal, oldID);
            if (executionContainer != null)
            {
                if (!isLocal)
                {
                    var remoteContainer = executionContainer as RemoteWorkflowExecutionContainer;
                    if (remoteContainer != null)
                    {
                        if (!remoteContainer.ServerIsUp())
                        {
                            invokeErrors.AddError("Asynchronous execution failed: Remote server unreachable");
                        }
                        SetRemoteExecutionDataList(dataObject, executionContainer, invokeErrors);
                    }
                }
                if (!invokeErrors.HasErrors())
                {
                    var shapeDefinitionsToEnvironment = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs, update);
                    Task.Factory.StartNew(() =>
                    {
                        Dev2Logger.Info("ASYNC EXECUTION USER CONTEXT IS [ " + Thread.CurrentPrincipal.Identity.Name + " ]");
                        ErrorResultTO error;
                        clonedDataObject.Environment = shapeDefinitionsToEnvironment;
                        executionContainer.Execute(out error, update);
                        return clonedDataObject;
                    }).ContinueWith(task =>
                    {
                        if (task.Result != null)
                        {
                            task.Result.Environment = null;
                        }
                    });

                }
            }
            else
            {
                invokeErrors.AddError(ErrorResource.ResourceNotFound);
            }

        }

        protected virtual IEsbServiceInvoker CreateEsbServicesInvoker(IWorkspace theWorkspace)
        {
            return new EsbServiceInvoker(this, theWorkspace);
        }

    }
}
