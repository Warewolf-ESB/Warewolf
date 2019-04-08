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
using Warewolf.Storage.Interfaces;


namespace Dev2.Runtime.ESB.Control
{
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
        readonly IEnvironmentOutputMappingManager _environmentOutputMappingManager;

        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errors)
        {

            var resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = null;
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () =>
            {
                theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            });

            var dataListOkay = EnsureDataListIdIsSet(dataObject, workspaceId, errors);
            if (!dataListOkay)
            {
                return resultID;
            }

            try
            {
                Dev2Logger.Debug("Creating Invoker", dataObject.ExecutionID.ToString());
                using (var invoker = new EsbServiceInvoker(this, theWorkspace, request))
                {
                    resultID = invoker.Invoke(dataObject, out ErrorResultTO invokeErrors);
                    errors.MergeErrors(invokeErrors);
                }
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }
            return resultID;
        }

        static bool EnsureDataListIdIsSet(IDSFDataObject dataObject, Guid workspaceId, ErrorResultTO errors)
        {
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                IResource resource;
                try
                {
                    resource = dataObject.ResourceID == Guid.Empty ? GetResource(workspaceId, dataObject.ServiceName) : GetResource(workspaceId, dataObject.ResourceID);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex, dataObject.ExecutionID.ToString());
                    errors.AddError(string.Format(ErrorResource.ServiceNotFound, dataObject.ServiceName));
                    return false;
                }

                if (resource?.DataList != null)
                {
                    Dev2Logger.Debug("Remote Invoke", dataObject.ExecutionID.ToString());
                    Dev2Logger.Debug("Mapping Inputs from Environment", dataObject.ExecutionID.ToString());
                    ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(dataObject, dataObject.RawPayload, resource.DataList.ToString());
                }
                dataObject.RawPayload = new StringBuilder();

                // We need to create the parentID around the system ;)
                dataObject.ParentThreadID = Thread.CurrentThread.ManagedThreadId;

            }
            return true;
        }

        static IResource GetResource(Guid workspaceId, Guid resourceId)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceId) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

            return resource;
        }

        static IResource GetResource(Guid workspaceId, string resourceName) => ResourceCatalog.Instance.GetResource(workspaceId, resourceName) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors, int update)
        {
            errors = null;
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var executionContainer = CreateExecutionContainer(dataObject, theWorkspace);
            executionContainer.PerformLogExecution(uri, update);
        }

        RemoteWorkflowExecutionContainer CreateExecutionContainer(IDSFDataObject dataObject, IWorkspace theWorkspace) => new RemoteWorkflowExecutionContainer(null, dataObject, theWorkspace, this);
        
        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors, int update, bool handleErrors)
        {
            var wasTestExecution = dataObject.IsServiceTestExecution;
            dataObject.IsSubExecution = true;
            try
            {
                return Asdf(dataObject, workspaceId, inputDefs, outputDefs, out errors, update, handleErrors, wasTestExecution);
            }
            finally
            {
                dataObject.IsServiceTestExecution = wasTestExecution;
                dataObject.IsSubExecution = false;
            }
        }

        private IExecutionEnvironment Asdf(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors, int update, bool handleErrors, bool wasTestExecution)
        {
            dataObject.IsServiceTestExecution = false;
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var invoker = CreateEsbServicesInvoker(theWorkspace);
            ErrorResultTO invokeErrors;
            var oldID = dataObject.DataListID;
            errors = new ErrorResultTO();

            var isLocal = !dataObject.IsRemoteWorkflow();

            var principle = Thread.CurrentPrincipal;
            Dev2Logger.Info("SUB-EXECUTION USER CONTEXT IS [ " + principle.Identity.Name + " ] FOR SERVICE  [ " + dataObject.ServiceName + " ]", dataObject.ExecutionID.ToString());
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
                if (isLocal && GetResource(workspaceId, dataObject.ResourceID) == null && GetResource(workspaceId, dataObject.ServiceName) == null)
                {
                    errors.AddError(string.Format(ErrorResource.ResourceNotFound, dataObject.ServiceName));
                    dataObject.StartTime = oldStartTime;
                    return null;
                }


                var executionContainer = invoker.GenerateInvokeContainer(dataObject, dataObject.ServiceName, isLocal, oldID);
                dataObject.IsServiceTestExecution = wasTestExecution;
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
                        var errorString = dataObject.Environment.FetchErrors();
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

        static void SetRemoteExecutionDataList(IDSFDataObject dataObject, IEsbExecutionContainer executionContainer, ErrorResultTO errors)
        {
            if (executionContainer is RemoteWorkflowExecutionContainer remoteContainer)
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

        void ExecuteRequestAsync(IDSFDataObject dataObject, string inputDefs, IEsbServiceInvoker invoker, bool isLocal, Guid oldID, out ErrorResultTO invokeErrors, int update)
        {
            var clonedDataObject = dataObject.Clone();
            invokeErrors = new ErrorResultTO();
            var executionContainer = invoker.GenerateInvokeContainer(clonedDataObject, clonedDataObject.ServiceName, isLocal, oldID);
            if (executionContainer != null)
            {
                if (!isLocal && executionContainer is RemoteWorkflowExecutionContainer remoteContainer)
                {
                    if (!remoteContainer.ServerIsUp())
                    {
                        invokeErrors.AddError("Asynchronous execution failed: Remote server unreachable");
                    }
                    SetRemoteExecutionDataList(dataObject, executionContainer, invokeErrors);
                }

                if (!invokeErrors.HasErrors())
                {
                    var shapeDefinitionsToEnvironment = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs, update);
                    Task.Factory.StartNew(() =>
                    {
                        Dev2Logger.Info("ASYNC EXECUTION USER CONTEXT IS [ " + Thread.CurrentPrincipal.Identity.Name + " ]", dataObject.ExecutionID.ToString());
                        clonedDataObject.Environment = shapeDefinitionsToEnvironment;
                        executionContainer.Execute(out ErrorResultTO error, update);
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

        protected virtual IEsbServiceInvoker CreateEsbServicesInvoker(IWorkspace theWorkspace) => new EsbServiceInvoker(this, theWorkspace);
    }
}
