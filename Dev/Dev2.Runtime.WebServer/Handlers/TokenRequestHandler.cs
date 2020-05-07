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
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Workspaces;
using Warewolf;
using Warewolf.Auditing;
using Warewolf.Data;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.WebServer.Handlers
{
    public class TokenRequestHandler : IRequestHandler
    {
        readonly IDataObjectFactory _dataObjectFactory;
        readonly IAuthorizationService _authorizationService;
        readonly IWorkspaceRepository _workspaceRepository;
        readonly IResourceCatalog _resourceCatalog;

        public TokenRequestHandler()
            : this(ResourceCatalog.Instance)
        {
        }

        protected TokenRequestHandler(IResourceCatalog resourceCatalog)
            : this(resourceCatalog, WorkspaceRepository.Instance, ServerAuthorizationService.Instance, new DataObjectFactory())
        {
        }

        protected TokenRequestHandler(IResourceCatalog resourceCatalog, IWorkspaceRepository workspaceRepository, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory)
        {
            _resourceCatalog = resourceCatalog;
            _workspaceRepository = workspaceRepository;
            _authorizationService = authorizationService;
            _dataObjectFactory = dataObjectFactory;
        }


        public virtual void ProcessRequest(ICommunicationContext ctx)
        {
            LoadSecuritySettings();

            var serviceName = ctx.GetServiceName();
            var instanceId = ctx.GetInstanceID();
            var bookmark = ctx.GetBookmark();
            var workspaceId = ctx.GetWorkspaceID();

            var requestTo = new WebRequestTO
            {
                ServiceName = serviceName,
                InstanceID = instanceId,
                Bookmark = bookmark,
                WebServerUrl = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}/public/{OverrideResource.Name}",
                Dev2WebServer = $"{ctx.Request.Uri.Scheme}://{ctx.Request.Uri.Authority}"
            };

            var data = SubmittedData.GetPostData(ctx);
            if (!string.IsNullOrEmpty(data))
            {
                requestTo.RawRequestPayload = data;
            }

            var variables = ctx.Request.BoundVariables;
            if (variables != null)
            {
                foreach (string key in variables)
                {
                    requestTo.Variables.Add(key, variables[key]);
                }
            }

            Thread.CurrentPrincipal = ctx.Request.User;
            var response = ExecuteWorkflow(requestTo, OverrideResource.Name, workspaceId, ctx.FetchHeaders(), ctx.Request.User);
            ctx.Send(response);
        }


        private static List<WindowsGroupPermission> WindowsPermissions { get; set; }

        private static INamedGuid OverrideResource { get; set; }

        protected static void LoadSecuritySettings()
        {
            var reader = new SecurityRead();
            var result = reader.Execute(null, null);
            var serializer = new Dev2JsonSerializer();
            var securitySettings = serializer.Deserialize<SecuritySettingsTO>(result);
            OverrideResource = securitySettings.AuthenticationOverrideWorkflow;
            WindowsPermissions = securitySettings.WindowsGroupPermissions;
        }

        protected IResponseWriter ExecuteWorkflow(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user)
        {
            var a = new Executor(_workspaceRepository, _resourceCatalog, _authorizationService, _dataObjectFactory);
            var isValid = a.TryExecute(webRequest, serviceName, workspaceId, headers, user);
            if (isValid != null)
            {
                return new ExceptionResponseWriter(System.Net.HttpStatusCode.InternalServerError, "");
            }

            var response = a.BuildResponse(webRequest, serviceName);
            return CreateEncryptedResponse(response);
        }

        private static IResponseWriter CreateEncryptedResponse(string payload)
        {
            var formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            var rs = new StringResponseWriterFactory();
            if (payload.Length > 0)
            {
                var encryptedPayload = DpapiWrapper.Encrypt(payload);
                return rs.New(encryptedPayload, formatter.ContentType);
            }
            else
            {
                return rs.New("", formatter.ContentType);
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
                    var keyValuePairs = payload.Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries).ToList();
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
                var keyValuePairs = data.Split(new[] {"&"}, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var keyValuePair in keyValuePairs)
                {
                    var keyValue = keyValuePair.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries);
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

            public Executor(IWorkspaceRepository workspaceRepository, IResourceCatalog resourceCatalog, IAuthorizationService authorizationService, IDataObjectFactory dataObjectFactory)
            {
                _repository = workspaceRepository;
                _resourceCatalog = resourceCatalog;
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
                _dataObject.ReturnType = EmitionTypes.JSON;
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

                PrepareDataObject(webRequest, serviceName, new NameValueCollection(), user, _workspaceGuid, out _resource);

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

                if (_canExecute)
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

            internal string BuildResponse(WebRequestTO webRequest, string serviceName)
            {
                DataListFormat formatter;

                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
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

            private string DefaultExecutionResponse(ExecutionDto executionDto)
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
                return executionDtoExtensions.CreatePayloadResponse();
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