/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decision;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Workspaces;
using Warewolf.Auditing;
using Warewolf.Data;

namespace Dev2.Runtime.WebServer.Handlers
{
    public abstract class AbstractWebRequestHandler : IRequestHandler
    {
        readonly IResourceCatalog _resourceCatalog;
        readonly ITestCatalog _testCatalog;
        readonly ITestCoverageCatalog _testCoverageCatalog;
        readonly IDataObjectFactory _dataObjectFactory;
        readonly IAuthorizationService _authorizationService;
        readonly IWorkspaceRepository _workspaceRepository;

        string _location;
        public string Location => _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        public abstract void ProcessRequest(ICommunicationContext ctx);
        protected AbstractWebRequestHandler()
            : this(ResourceCatalog.Instance, TestCatalog.Instance, TestCoverageCatalog.Instance)
        {
        }

        protected AbstractWebRequestHandler(IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog)
            : this(resourceCatalog, testCatalog, testCoverageCatalog, WorkspaceRepository.Instance, ServerAuthorizationService.Instance, new DataObjectFactory())
        {
        }

        protected AbstractWebRequestHandler(IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory)
        {
            _resourceCatalog = resourceCatalog;
            _testCatalog = testCatalog;
            _testCoverageCatalog = testCoverageCatalog;
            _workspaceRepository = workspaceRepository;
            _authorizationService = authorizationService;
            _dataObjectFactory = dataObjectFactory;
        }

#pragma warning disable CC0044
        protected IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers) => CreateForm(webRequest, serviceName, workspaceId, headers, null);
#pragma warning restore CC0044

        protected IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user)
        {
            var a = new Executor(_workspaceRepository, _resourceCatalog, _testCatalog, _testCoverageCatalog, _authorizationService, _dataObjectFactory);
            var response = a.TryExecute(webRequest, serviceName, workspaceId, headers, user);
            return response ?? a.BuildResponse(webRequest, serviceName);
        }

        private class Executor
        {
            private string _executePayload;
            private Guid _workspaceGuid;
            private Guid _executionDataListId;
            private IDSFDataObject _dataObject;
            private IWarewolfResource _resource;
            private Dev2JsonSerializer _serializer;
            private bool _canExecute;
            private EsbExecuteRequest _esbExecuteRequest;
            readonly IAuthorizationService _authorizationService;
            readonly IDataObjectFactory _dataObjectFactory;
            readonly IResourceCatalog _resourceCatalog;
            readonly IWorkspaceRepository _repository;
            readonly ITestCatalog _testCatalog;
            readonly ITestCoverageCatalog _testCoverageCatalog;

            public Executor(IWorkspaceRepository workspaceRepository, IResourceCatalog resourceCatalog, ITestCatalog testCatalog, ITestCoverageCatalog testCoverageCatalog, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory)
            {
                _repository = workspaceRepository;
                _resourceCatalog = resourceCatalog;
                _testCatalog = testCatalog;
                _testCoverageCatalog = testCoverageCatalog;
                _authorizationService = authorizationService;
                _dataObjectFactory = dataObjectFactory;
            }

            private Guid EnsureWorkspaceIdValid(string workspaceId)
            {
                if (workspaceId is null)
                {
                    return _repository.ServerWorkspace.ID;
                }

                return !Guid.TryParse(workspaceId, out var workspaceGuid)
                    ? _repository.ServerWorkspace.ID
                    : workspaceGuid;
            }

            private void PrepareDataObject(WebRequestTO webRequest, string serviceName, NameValueCollection headers, IPrincipal user, Guid workspaceGuid, out IWarewolfResource resource)
            {
                var uri = string.IsNullOrWhiteSpace(webRequest.WebServerUrl) ? new Uri("https://test/") : new Uri(webRequest.WebServerUrl);
                _dataObject = _dataObjectFactory.New(workspaceGuid, user, serviceName, webRequest);
                _dataObject.OriginalServiceName = serviceName;
                _dataObject.SetHeaders(headers);
                _dataObject.SetupForWebDebug(webRequest);
                webRequest.BindRequestVariablesToDataObject(ref _dataObject);
                _dataObject.SetupForRemoteInvoke(headers);
                _dataObject.SetEmissionType(uri, serviceName, headers);
                _dataObject.SetupForTestExecution(serviceName, headers);
                if (_dataObject.ServiceName == null)
                {
                    _dataObject.ServiceName = serviceName;
                }

                _dataObject.SetResourceNameAndId(_resourceCatalog, serviceName, out resource);
                _dataObject.SetTestResourceIds(_resourceCatalog.NewContextualResourceCatalog(_authorizationService, workspaceGuid), webRequest, serviceName, resource);
                _dataObject.WebUrl = webRequest.WebServerUrl;
                _dataObject.EsbChannel = new EsbServicesEndpoint();

                if (_dataObject.Settings is null)
                {
                    _dataObject.Settings = new Dev2WorkflowSettingsTO
                    {
                        EnableDetailedLogging = Config.Server.EnableDetailedLogging,
                        LoggerType = LoggerType.JSON,
                        KeepLogsForDays = 2,
                        CompressOldLogFiles = true
                    };
                }
                
                var stateNotifier = CustomContainer.Get<IStateNotifierFactory>()?.New(_dataObject);
                _dataObject.StateNotifier = stateNotifier;
            }

            internal IResponseWriter TryExecute(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user)
            {
                _workspaceGuid = EnsureWorkspaceIdValid(workspaceId);
                _serializer = new Dev2JsonSerializer();

                PrepareDataObject(webRequest, serviceName, headers, user, _workspaceGuid, out _resource);
                var isTestCoverage = _dataObject.ReturnType == EmitionTypes.Cover || _dataObject.ReturnType == EmitionTypes.CoverJson;
                var isTestRun = (_dataObject.ReturnType == EmitionTypes.TEST ||
                                 _dataObject.ReturnType == EmitionTypes.TRX) && _dataObject.TestName == "*";

                if (isTestRun)
                {
                    return ExecuteAsTest(user);
                }

                if (isTestCoverage)
                {
                    return ExecuteAsCoverage(webRequest, serviceName, _resource);
                }

                if (_resource is null)
                {
                    var msg = string.Format(Warewolf.Resource.Errors.ErrorResource.ServiceNotFound, serviceName);
                    _dataObject.Environment.AddError(msg);
                    _dataObject.ExecutionException = new Exception(msg);
                    _executionDataListId = GlobalConstants.NullDataListID;
                    return null;
                }

                _canExecute = _dataObject.CanExecuteCurrentResource(_resource, _authorizationService);
                if (!_canExecute)
                {
                    var errorMessage =
                        string.Format(
                            Warewolf.Resource.Errors.ErrorResource.UserNotAuthorizedToExecuteOuterWorkflowException,
                            _dataObject.ExecutingUser.Identity.Name, _dataObject.ServiceName);
                    _dataObject.Environment.AddError(errorMessage);
                    _dataObject.ExecutionException = new Exception(errorMessage);
                }

                _executionDataListId = GlobalConstants.NullDataListID;

                if (_canExecute && _dataObject.ReturnType != EmitionTypes.SWAGGER)
                {
                    Thread.CurrentPrincipal = user;

                    _executionDataListId = DoExecution(webRequest, serviceName, _workspaceGuid, _dataObject, user);
                }

                return null;
            }

            private Guid DoExecution(WebRequestTO webRequest, string serviceName, Guid workspaceGuid, IDSFDataObject dataObject, IPrincipal userPrinciple)
            {
                _esbExecuteRequest = CreateEsbExecuteRequestFromWebRequest(webRequest, serviceName);
                var executionDataListId = Guid.Empty;

                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
                {
                    executionDataListId = dataObject.EsbChannel.ExecuteRequest(dataObject, _esbExecuteRequest, workspaceGuid, out _);
                    _executePayload = _esbExecuteRequest.ExecuteResult.ToString();
                });

                return executionDataListId;
            }

            private IResponseWriter ExecuteAsTest(IPrincipal userPrinciple)
            {
                var formatter = ServiceTestExecutor.ExecuteTests(_dataObject, userPrinciple, _workspaceGuid, _serializer, _testCatalog, _resourceCatalog, out _executePayload, _testCoverageCatalog);
                return new StringResponseWriter(_executePayload ?? string.Empty, formatter.ContentType);
            }

            private IResponseWriter ExecuteAsCoverage(WebRequestTO webRequest, string serviceName, IWarewolfResource resource)
            {
                try
                {
                    var coverageDataContext = new CoverageDataContext(_dataObject.ResourceID, _dataObject.ReturnType, webRequest.WebServerUrl);
                    coverageDataContext.SetTestCoverageResourceIds(_resourceCatalog.NewContextualResourceCatalog(_authorizationService, _workspaceGuid), webRequest, serviceName, resource);
                    var formatter = ServiceTestCoverageExecutor.GetTestCoverageReports(coverageDataContext, _workspaceGuid, _serializer, _testCoverageCatalog, _resourceCatalog, out _executePayload);
                    return new StringResponseWriter(_executePayload ?? string.Empty, formatter.ContentType);
                }
                finally
                {
                    Dev2DataListDecisionHandler.Instance.RemoveEnvironment(_dataObject.DataListID);
                    _dataObject.Environment = null;
                }
            }

            internal IResponseWriter BuildResponse(WebRequestTO webRequest, string serviceName)
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
                } else
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

            private static EsbExecuteRequest CreateEsbExecuteRequestFromWebRequest(WebRequestTO webRequest, string serviceName)
            {
                var esbExecuteRequest = new EsbExecuteRequest
                {
                    
                    ServiceName = serviceName,
                };
                foreach (string key in webRequest.Variables)
                {
                    esbExecuteRequest.AddArgument(key, new StringBuilder(webRequest.Variables[key]));
                }

                return esbExecuteRequest;
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

        internal static class SubmittedData
        {
            internal static string GetPostData(ICommunicationContext ctx)
            {
                var baseStr = HttpUtility.UrlDecode(ctx.Request.Uri.ToString());
                baseStr = HttpUtility.UrlDecode(CleanupXml(baseStr));
                string payload = null;
                if (baseStr != null)
                {
                    var startIdx = baseStr.IndexOf("?", StringComparison.Ordinal);
                    if (startIdx > 0)
                    {
                        payload = baseStr.Substring(startIdx + 1);
                        if (payload.IsXml() || payload.IsJSON())
                        {
                            return payload;
                        }
                    }
                }

                if (ctx.Request.Method == "GET")
                {
                    return ExtractKeyValuePairForGetMethod(ctx, payload);
                }

                if (ctx.Request.Method == "POST")
                {
                    using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                    {
                        try
                        {
                            return ExtractKeyValuePairForPostMethod(ctx, reader);
                        }
                        catch (Exception ex)
                        {
                            Dev2Logger.Error(nameof(AbstractWebRequestHandler), ex, GlobalConstants.WarewolfError);
                        }
                    }
                }

                return string.Empty;
            }

            internal static string CleanupXml(string baseStr)
            {
                if (baseStr.Contains("?"))
                {
                    var startQueryString = baseStr.IndexOf("?", StringComparison.Ordinal);
                    var query = baseStr.Substring(startQueryString + 1);
                    if (query.IsJSON())
                    {
                        return baseStr;
                    }
                    var args = HttpUtility.ParseQueryString(query);
                    var url = baseStr.Substring(0, startQueryString + 1);
                    var results = new List<string>();
                    foreach (var arg in args.AllKeys)
                    {
                        var txt = args[arg];
                        results.Add(txt.IsXml() ? arg + "=" + string.Format(GlobalConstants.XMLPrefix + "{0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(txt))) : $"{arg}={txt}");
                    }

                    return url + string.Join("&", results);
                }
                return baseStr;
            }

            internal static string ExtractKeyValuePairForGetMethod(ICommunicationContext ctx, string payload)
            {
                if (payload != null)
                {
                    var keyValuePairs = payload.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (var keyValuePair in keyValuePairs)
                    {
                        if (keyValuePair.StartsWith("wid="))
                        {
                            continue;
                        }
                        if (keyValuePair.IsXml() || keyValuePair.IsJSON() || (keyValuePair.ToLowerInvariant().Contains("<DataList>".ToLowerInvariant()) && keyValuePair.ToLowerInvariant().Contains("</DataList>".ToLowerInvariant())))
                        {
                            return keyValuePair;
                        }
                    }
                }
                var pairs = ctx.Request.QueryString;
                return ExtractKeyValuePairs(pairs, ctx.Request.BoundVariables);
            }
            static string ExtractKeyValuePairForPostMethod(ICommunicationContext ctx, StreamReader reader)
            {
                var data = reader.ReadToEnd();
                if (DataListUtil.IsXml(data) || DataListUtil.IsJson(data))
                {
                    return data;
                }

                var pairs = ExtractArgumentsFromDataListOrQueryString(ctx, data);

                return ExtractKeyValuePairs(pairs, ctx.Request.BoundVariables);
            }

            private static NameValueCollection ExtractArgumentsFromDataListOrQueryString(ICommunicationContext ctx, string data)
            {
                var pairs = new NameValueCollection(5);
                var keyValuePairs = data.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var keyValuePair in keyValuePairs)
                {
                    var keyValue = keyValuePair.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length > 1)
                    {
                        pairs.Add(keyValue[0], keyValue[1]);
                    }
                    else
                    {
                        if (keyValue.Length == 1 && (keyValue[0].IsXml() || keyValue[0].IsJSON()))
                        {
                            pairs.Add(keyValue[0], keyValue[0]);
                        }
                    }
                }

                if (pairs.Count == 0)
                {
                    pairs = ctx.Request.QueryString;
                }

                return pairs;
            }

            internal static string ExtractKeyValuePairs(NameValueCollection pairs, NameValueCollection boundVariables)
            {
                // Extract request keys
                foreach (var key in pairs.AllKeys)
                {
                    if (key == "wid") //Don't add the Workspace ID to DataList
                    {
                        continue;
                    }
                    if (key.IsXml() || key.IsJSON() || (key.ToLowerInvariant().Contains("<DataList>".ToLowerInvariant()) && key.ToLowerInvariant().Contains("<\\DataList>".ToLowerInvariant())))
                    {
                        return key; //We have a workspace id and XML DataList
                    }
                    boundVariables.Add(key, pairs[key]);

                }

                return string.Empty;
            }
        }

        public interface IDataObjectFactory
        {
#pragma warning disable CC0044
            IDSFDataObject New(Guid workspaceGuid, IPrincipal user, string serviceName, WebRequestTO webRequest);
#pragma warning restore CC0044
        }

        private class DataObjectFactory : IDataObjectFactory
        {
#pragma warning disable CC0044
            public IDSFDataObject New(Guid workspaceGuid, IPrincipal user, string serviceName, WebRequestTO webRequest) =>
#pragma warning restore CC0044
                new DsfDataObject(webRequest.RawRequestPayload, GlobalConstants.NullDataListID, webRequest.RawRequestPayload)
                {
                    IsFromWebServer = true,
                    ExecutingUser = user,
                    ServiceName = serviceName,
                    WorkspaceID = workspaceGuid
                };
        }
    }

}
