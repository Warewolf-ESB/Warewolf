/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Runtime.Services;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Workspaces;
using Warewolf.Execution;
using Warewolf.Security;

namespace Dev2.Runtime.WebServer.Executor
{
    public class Executor : ExecutorBase
    {
        public Executor(IWorkspaceRepository workspaceRepository, IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IServiceTestExecutor serviceTestExecutor, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory, IEsbChannelFactory esbChannelFactory, IJwtManager jwtManager)
                : base(workspaceRepository, resourceCatalog, testCatalog, testCoverageCatalog, serviceTestExecutor, authorizationService, dataObjectFactory, esbChannelFactory, jwtManager)
            {
            }

            public override IResponseWriter BuildResponse(WebRequestTO webRequest, string serviceName)
            {
                if (_dataObject.IsServiceTestExecution)
                {
                    return ServiceTestExecutionResponse(out _executePayload, _dataObject, _serializer, _canExecute);
                }

                if (_dataObject.IsDebugFromWeb)
                {
                    return DebugFromWebExecutionResponse(_dataObject, _serializer);
                }

                DataListFormat formatter;
                if (webRequest.ServiceName.EndsWith(".xml") || _dataObject.ReturnType == EmitionTypes.XML)
                {
                    formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
                }
                else
                {
                    formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                }

                var executionDto = new ExecutionDto
                {
                    WebRequestTO = webRequest,
                    ServiceName = serviceName,
                    DataObject = _dataObject,
                    DataListIdGuid = _executionDataListId,
                    WorkspaceID = _workspaceGuid,
                    Resource = _resource,
                    DataListFormat = formatter,
                    PayLoad = _executePayload ?? string.Empty,
                    Serializer = _serializer,
                };
                return DefaultExecutionResponse(executionDto);
            }

            private IResponseWriter DefaultExecutionResponse(ExecutionDto executionDto)
            {
                var allErrors = new ErrorResultTO();

                var currentErrors = executionDto.DataObject.Environment?.Errors?.Union(executionDto.DataObject.Environment?.AllErrors);
                if (currentErrors != null)
                {
                    foreach (var error in currentErrors)
                    {
                        if (error.Length > 0)
                        {
                            allErrors.AddError(error, true);
                        }
                    }
                }

                executionDto.Request = _esbExecuteRequest;

                executionDto.ErrorResultTO = allErrors;

                var executionDtoExtensions = new ExecutionDtoExtensions(executionDto);
                return executionDtoExtensions.CreateResponseWriter(new StringResponseWriterFactory());
            }

            private IResponseWriter ServiceTestExecutionResponse(out string executePayload, IDSFDataObject dataObject, Dev2JsonSerializer serializer, bool canExecute)
            {
                var formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                if (!canExecute)
                {
                    executePayload = string.Empty;
                    return new StringResponseWriter(dataObject.Environment.FetchErrors(), formatter.ContentType);
                }

                executePayload = ServiceTestExecutor.SetupForTestExecution(serializer, _esbExecuteRequest, dataObject);
                return new StringResponseWriter(executePayload, formatter.ContentType);
            }

            private static IResponseWriter DebugFromWebExecutionResponse(IDSFDataObject dataObject, Dev2JsonSerializer serializer)
            {
                var formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                var serialize = SetupForWebExecution(dataObject, serializer);
                return new StringResponseWriter(serialize, formatter.ContentType);
            }

            static string SetupForWebExecution(IDSFDataObject dataObject, Dev2JsonSerializer serializer)
            {
                var fetchDebugItems = WebDebugMessageRepo.Instance.FetchDebugItems(dataObject.ClientID, dataObject.DebugSessionID);
                var remoteDebugItems = fetchDebugItems?.Where(state => state.StateType != StateType.Duration).ToArray() ??
                                       new IDebugState[] { };
                var debugStates = DebugStateTreeBuilder.BuildTree(remoteDebugItems);
                var serialize = serializer.Serialize(debugStates);
                return serialize;
            }
    }
}
