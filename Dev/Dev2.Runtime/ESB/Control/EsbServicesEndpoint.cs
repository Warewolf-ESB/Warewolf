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
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Esb;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Runtime.ESB.Control
{
    public class EsbServicesEndpoint : IEsbWorkspaceChannel
    {
        readonly IEnvironmentOutputMappingManager _environmentOutputMappingManager;

        public EsbServicesEndpoint(IEnvironmentOutputMappingManager environmentOutputMappingManager)
        {
            _environmentOutputMappingManager = environmentOutputMappingManager;
        }

        public EsbServicesEndpoint()
            : this(new EnvironmentOutputMappingManager())
        {

        }

        public Guid ExecuteRequest(IDSFDataObject dataObject, EsbExecuteRequest request, Guid workspaceId, out ErrorResultTO errors, IInternalExecutionContext internalExecutionContext)
        {
            var resultID = GlobalConstants.NullDataListID;
            errors = new ErrorResultTO();
            IWorkspace theWorkspace = null;
            Common.Utilities.PerformActionInsideImpersonatedContext(Common.Utilities.ServerUser, () =>
            {
                theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            });
            if (internalExecutionContext != null)
            {
                // internalExecutionContext is null when the request did not come from an internal source, such as from AbstractWebRequestHandler.CreateForm
                internalExecutionContext.Workspace = theWorkspace;
            }

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
                    resultID = invoker.Invoke(dataObject, out var invokeErrors, internalExecutionContext);
                    errors.MergeErrors(invokeErrors);
                }
            }
            catch (Exception ex)
            {
                errors.AddError(ex.Message);
            }
            return resultID;
        }

        private static bool EnsureDataListIdIsSet(IDSFDataObject dataObject, Guid workspaceId, ErrorResultTO errors)
        {
            if (dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                var resource = GetResource(dataObject, workspaceId, errors);
                if (resource?.HasDataList ?? false)
                {
                    Dev2Logger.Debug("Remote Invoke", dataObject.ExecutionID.ToString());
                    Dev2Logger.Debug("Mapping Inputs from Environment", dataObject.ExecutionID.ToString());
                    ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(dataObject, dataObject.RawPayload, resource.DataList.ToString());
                }
                dataObject.RawPayload = new StringBuilder();

                // We need to create the parentID around the system
                dataObject.ParentThreadID = Thread.CurrentThread.ManagedThreadId;

            }
            return true;
        }

        private static IResource GetResource(IDSFDataObject dataObject, Guid workspaceId, ErrorResultTO errors)
        {
            try
            {
                var noResourceId = dataObject.ResourceID == Guid.Empty;
                return noResourceId
                    ? GetResourceByName(workspaceId, dataObject.ServiceName)
                    : GetResourceById(workspaceId, dataObject.ResourceID);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex, dataObject.ExecutionID.ToString());
                errors.AddError(string.Format(ErrorResource.ServiceNotFound, dataObject.ServiceName));
                return null;
            }
        }

        private static IResource GetResourceById(Guid workspaceId, Guid resourceId)
        {
            var resource = ResourceCatalog.Instance.GetResource(workspaceId, resourceId) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);

            return resource;
        }

        private static IResource GetResourceByName(Guid workspaceId, string resourceName) => ResourceCatalog.Instance.GetResource(workspaceId, resourceName) ?? ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceName);

        public void ExecuteLogErrorRequest(IDSFDataObject dataObject, Guid workspaceId, string uri, out ErrorResultTO errors, int update)
        {
            errors = null;
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var executionContainer = new RemoteWorkflowExecutionContainer(null, dataObject, theWorkspace, this);
            executionContainer.PerformLogExecution(uri, update);
        }

        public IExecutionEnvironment ExecuteSubRequest(IDSFDataObject dataObject, Guid workspaceId, string inputDefs, string outputDefs, out ErrorResultTO errors, int update, bool handleErrors)
        {
            var theWorkspace = WorkspaceRepository.Instance.Get(workspaceId);
            var invoker = new EsbServiceInvoker(this, theWorkspace);

            IExecutionEnvironment result;
            SubExecutionHelperBase helper;
            if (dataObject.RunWorkflowAsync)
            {
                helper = new SubExecutionHelperAsync(_environmentOutputMappingManager, workspaceId, invoker, dataObject, inputDefs, outputDefs);
            }
            else
            {
                helper = new SubExecutionHelper(_environmentOutputMappingManager, workspaceId, invoker, dataObject, inputDefs, outputDefs);
            }

            result = helper.Execute(update, handleErrors);
            errors = helper.ErrorResult;
            return result;
        }

        public void CreateNewEnvironmentFromInputMappings(IDSFDataObject dataObject, string inputDefs, int update) => dataObject.CreateNewEnvironmentFromInputMappings(inputDefs, update);

        private abstract class SubExecutionHelperBase
        {
            protected readonly IEnvironmentOutputMappingManager _environmentOutputMappingManager;
            protected readonly Guid _workspaceId;
            protected readonly EsbServiceInvoker _invoker;
            protected readonly IDSFDataObject _dataObject;
            protected readonly string _inputDefs;
            protected readonly string _outputDefs;

            protected readonly Guid _oldId;
            protected readonly bool _isLocal;
            protected readonly DateTime _oldStartTime;
            protected readonly ErrorResultTO _errors = new ErrorResultTO();
            public ErrorResultTO ErrorResult => _errors;

            protected SubExecutionHelperBase(IEnvironmentOutputMappingManager environmentOutputMappingManager, Guid workspaceId, EsbServiceInvoker invoker, IDSFDataObject dataObject, string inputDefs, string outputDefs)
            {
                _environmentOutputMappingManager = environmentOutputMappingManager;
                _workspaceId = workspaceId;
                _invoker = invoker;
                _dataObject = dataObject;
                _inputDefs = inputDefs;
                _outputDefs = outputDefs;

                _oldId = _dataObject.DataListID;
                _oldStartTime = _dataObject.StartTime;
                _isLocal = !_dataObject.IsRemoteWorkflow();
            }

            public IExecutionEnvironment Execute(int update, bool handleErrors)
            {
                var wasTestExecution = _dataObject.IsServiceTestExecution;
                _dataObject.IsSubExecution = true;
                try
                {
                    _dataObject.IsServiceTestExecution = false;

                    var principle = Thread.CurrentPrincipal;
                    Dev2Logger.Info("SUB-EXECUTION USER CONTEXT IS [ " + principle.Identity.Name + " ] FOR SERVICE  [ " + _dataObject.ServiceName + " ]", _dataObject.ExecutionID.ToString());
                    _dataObject.StartTime = DateTime.Now;

                    var result = ExecuteWorkflow(wasTestExecution, update, handleErrors);
                    if (result != null)
                    {
                        return result;
                    }

                    _dataObject.StartTime = _oldStartTime;
                    return new ExecutionEnvironment();
                }
                finally
                {
                    _dataObject.IsServiceTestExecution = wasTestExecution;
                    _dataObject.IsSubExecution = false;
                }
            }

            protected abstract IExecutionEnvironment ExecuteWorkflow(bool wasTestExecution, int update, bool handleErrors);
        }

        private sealed class SubExecutionHelper : SubExecutionHelperBase
        {
            public SubExecutionHelper(IEnvironmentOutputMappingManager environmentOutputMappingManager, Guid workspaceId, EsbServiceInvoker invoker, IDSFDataObject dataObject, string inputDefs, string outputDefs)
                : base(environmentOutputMappingManager, workspaceId, invoker, dataObject, inputDefs, outputDefs)
            { }

            protected override IExecutionEnvironment ExecuteWorkflow(bool wasTestExecution, int update, bool handleErrors)
            {
                if (_isLocal && GetResourceById(_workspaceId, _dataObject.ResourceID) == null && GetResourceByName(_workspaceId, _dataObject.ServiceName) == null)
                {
                    _errors.AddError(string.Format(ErrorResource.ResourceNotFound, _dataObject.ServiceName));
                    _dataObject.StartTime = _oldStartTime;
                    return null;
                }

                var executionContainer = _invoker.GenerateInvokeContainer(_dataObject, _dataObject.ServiceName, _isLocal, _oldId);
                _dataObject.IsServiceTestExecution = wasTestExecution;
                if (executionContainer != null)
                {
                    _dataObject.CreateNewEnvironmentFromInputMappings(_inputDefs, update);

                    ConfigureDataListIfRemote(executionContainer);
                    if (!_errors.HasErrors())
                    {
                        executionContainer.InstanceInputDefinition = _inputDefs;
                        executionContainer.InstanceOutputDefinition = _outputDefs;
                        executionContainer.Execute(out var invokeErrors, update);

                        var env = _environmentOutputMappingManager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(_dataObject, _outputDefs, update, handleErrors, _errors);

                        _errors.MergeErrors(invokeErrors);
                        var errorString = _dataObject.Environment.FetchErrors();
                        invokeErrors = ErrorResultTO.MakeErrorResultFromDataListString(errorString);
                        _errors.MergeErrors(invokeErrors);
                        _dataObject.StartTime = _oldStartTime;
                        return env;
                    }
                    _errors.AddError(string.Format(ErrorResource.ResourceNotFound, _dataObject.ServiceName));
                }
                return null;
            }

            private void ConfigureDataListIfRemote(IEsbExecutionContainer executionContainer)
            {
                if (!_isLocal && executionContainer is RemoteWorkflowExecutionContainer remoteContainer)
                {
                    var fetchRemoteResource = remoteContainer.FetchRemoteResource(_dataObject.ResourceID, _dataObject.ServiceName, _dataObject.IsDebugMode());
                    if (fetchRemoteResource != null)
                    {
                        fetchRemoteResource.DataList = fetchRemoteResource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                        var remoteDataList = fetchRemoteResource.DataList;
                        _dataObject.RemoteInvokeResultShape = new StringBuilder(remoteDataList);
                        _dataObject.ServiceName = fetchRemoteResource.ResourceCategory;
                    }
                    else
                    {
                        var message = string.Format(ErrorResource.ServiceNotFound, _dataObject.ServiceName);
                        _errors.AddError(message);
                    }
                }
            }
        }

        private class SubExecutionHelperAsync : SubExecutionHelperBase
        {
            public SubExecutionHelperAsync(IEnvironmentOutputMappingManager environmentOutputMappingManager, Guid workspaceId, EsbServiceInvoker invoker, IDSFDataObject dataObject, string inputDefs, string outputDefs)
                : base(environmentOutputMappingManager, workspaceId, invoker, dataObject, inputDefs, outputDefs)
            {
            }

            protected override IExecutionEnvironment ExecuteWorkflow(bool wasTestExecution, int update, bool handleErrors)
            {
                var clonedDataObject = _dataObject.Clone();
                var invokeErrors = new ErrorResultTO();
                var executionContainer = _invoker.GenerateInvokeContainer(clonedDataObject, clonedDataObject.ServiceName, _isLocal, _oldId);
                if (executionContainer != null)
                {
                    DoRemoteDataObjectConfiguration(invokeErrors, executionContainer);

                    if (!invokeErrors.HasErrors())
                    {
                        var shapeDefinitionsToEnvironment = DataListUtil.InputsToEnvironment(_dataObject.Environment, _inputDefs, update);
                        Task.Factory
                            .StartNew(() =>
                            {
                                Dev2Logger.Info("ASYNC EXECUTION USER CONTEXT IS [ " + Thread.CurrentPrincipal.Identity.Name + " ]", _dataObject.ExecutionID.ToString());
                                clonedDataObject.Environment = shapeDefinitionsToEnvironment;
                                executionContainer.Execute(out var error, update);
                                return clonedDataObject;
                            })
                            .ContinueWith(SetResultEnvironmentToNull);
                    }
                }
                else
                {
                    invokeErrors.AddError(ErrorResource.ResourceNotFound);
                }

                _dataObject.StartTime = _oldStartTime;
                _errors.MergeErrors(invokeErrors);
                return null;
            }

            private static void SetResultEnvironmentToNull(Task<IDSFDataObject> task)
            {
                if (task?.Result != null)
                {
                    task.Result.Environment = null;
                }
            }

            private void DoRemoteDataObjectConfiguration(ErrorResultTO invokeErrors, IEsbExecutionContainer executionContainer)
            {
                if (!_isLocal && executionContainer is RemoteWorkflowExecutionContainer remoteContainer)
                {
                    if (!remoteContainer.ServerIsUp())
                    {
                        invokeErrors.AddError("Asynchronous execution failed: Remote server unreachable");
                    }

                    var fetchRemoteResource = remoteContainer.FetchRemoteResource(_dataObject.ResourceID, _dataObject.ServiceName, _dataObject.IsDebugMode());
                    if (fetchRemoteResource != null)
                    {
                        fetchRemoteResource.DataList = fetchRemoteResource.DataList.Replace(GlobalConstants.SerializableResourceQuote, "\"").Replace(GlobalConstants.SerializableResourceSingleQuote, "'");
                        var remoteDataList = fetchRemoteResource.DataList;
                        _dataObject.RemoteInvokeResultShape = new StringBuilder(remoteDataList);
                        _dataObject.ServiceName = fetchRemoteResource.ResourceCategory;
                    }
                    else
                    {
                        var message = string.Format(ErrorResource.ServiceNotFound, _dataObject.ServiceName);
                        _errors.AddError(message);
                    }
                }
            }
        }
    }

    public static class IdsfDataObjectExtensionMethods
    {
        public static void CreateNewEnvironmentFromInputMappings(this IDSFDataObject dataObject, string inputDefs, int update)
        {
            var shapeDefinitionsToEnvironment = DataListUtil.InputsToEnvironment(dataObject.Environment, inputDefs, update);
            dataObject.PushEnvironment(shapeDefinitionsToEnvironment);
        }
    }
}
