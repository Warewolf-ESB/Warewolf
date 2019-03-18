#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
        static WorkspaceRepository wRepository => WorkspaceRepository.Instance;
        static ResourceCatalog rCatalog => ResourceCatalog.Instance;
        
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
                    Dev2Logger.Error(ex, dataObject.ExecutionID.ToString());
                    errors.AddError(string.Format(ErrorResource.ServiceNotFound, dataObject.ServiceName));
                    return resultID;
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

            try
            {
                // Setup the invoker endpoint ;)
                Dev2Logger.Debug("Creating Invoker", dataObject.ExecutionID.ToString());
                using (var invoker = new EsbServiceInvoker(this,theWorkspace, request))
                {
                    // Should return the top level DLID
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
        
        static IResource GetResource(Guid workspaceId, Guid resourceId)
        {
            var resource = rCatalog.GetResource(workspaceId, resourceId) ?? rCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

            return resource;
        }

        static IResource GetResource(Guid workspaceId, string resourceName) => rCatalog.GetResource(workspaceId, resourceName) ?? rCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors, int update)
        {
            errors = null;
            var theWorkspace = wRepository.Get(workspaceId);
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
                dataObject.IsServiceTestExecution = false;
                var theWorkspace = wRepository.Get(workspaceId);
                var invoker = CreateEsbServicesInvoker(theWorkspace);
                ErrorResultTO invokeErrors;
                var oldID = dataObject.DataListID;
                errors = new ErrorResultTO();

                // local non-scoped execution ;)
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
                    }
                    if (executionContainer != null && !isLocal)
                    {
                        SetRemoteExecutionDataList(dataObject, executionContainer, errors);
                    }
                    if (executionContainer != null && !errors.HasErrors())
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
                    if (executionContainer != null)
                    {
                        errors.AddError(string.Format(ErrorResource.ResourceNotFound, dataObject.ServiceName));
                    }
                }
                dataObject.StartTime = oldStartTime;
                return new ExecutionEnvironment();
            }
            finally
            {
                dataObject.IsServiceTestExecution = wasTestExecution;
                dataObject.IsSubExecution = false;
            }
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
