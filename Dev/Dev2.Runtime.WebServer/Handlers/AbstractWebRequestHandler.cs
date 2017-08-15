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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
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






namespace Dev2.Runtime.WebServer.Handlers
{
    public abstract class AbstractWebRequestHandler : IRequestHandler
    {
        string _location;
        private static IResourceCatalog _resourceCatalog;
        private static ITestCatalog _testCatalog;
        private static IDSFDataObject _dataObject;
        private static IAuthorizationService _authorizationService;
        private static IWorkspaceRepository _repository;
        public string Location => _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        public abstract void ProcessRequest(ICommunicationContext ctx);
        protected AbstractWebRequestHandler()
            : this(ResourceCatalog.Instance, TestCatalog.Instance)
        {
        }

        protected AbstractWebRequestHandler(IResourceCatalog catalog, ITestCatalog testCatalog,IAuthorizationService authorizationService)
            :this(catalog,testCatalog)
        {
            _authorizationService = authorizationService;
        }
        protected AbstractWebRequestHandler(IResourceCatalog catalog
                                            , ITestCatalog testCatalog
                                            , IDSFDataObject dataObject
                                            , IAuthorizationService authorizationService
                                            , IWorkspaceRepository repository)
                                            : this(catalog, testCatalog)
        {
            _dataObject = dataObject;
            _authorizationService = authorizationService;
            _repository = repository;
        }

        protected AbstractWebRequestHandler(IResourceCatalog catalog, ITestCatalog testCatalog)
        {
            _resourceCatalog = catalog;
            _testCatalog = testCatalog;
        }

        protected static IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        {
            var executePayload = "";

            var workspaceRepository = _repository ?? WorkspaceRepository.Instance;
            var workspaceGuid = SetWorkspaceId(workspaceId, workspaceRepository);

            var allErrors = new ErrorResultTO();
            var dataObject = _dataObject ?? new DsfDataObject(webRequest.RawRequestPayload, GlobalConstants.NullDataListID, webRequest.RawRequestPayload)
            {
                IsFromWebServer = true
                ,
                ExecutingUser = user
                ,
                ServiceName = serviceName
                ,
                WorkspaceID = workspaceGuid
            };
            dataObject.SetupForWebDebug(webRequest);
            webRequest.BindRequestVariablesToDataObject(ref dataObject);
            dataObject.SetupForRemoteInvoke(headers);
            dataObject.SetEmitionType(serviceName, headers);
            dataObject.SetupForTestExecution(webRequest, serviceName, headers);
            if (dataObject.ServiceName == null)
                dataObject.ServiceName = serviceName;
            IResource resource;
            dataObject.SetResourceNameAndId(_resourceCatalog, serviceName, out resource);
            dataObject.SetTestResourceIds(_resourceCatalog, webRequest, serviceName);
            var serializer = new Dev2JsonSerializer();
            var esbEndpoint = new EsbServicesEndpoint();
            dataObject.EsbChannel = esbEndpoint;

            var instance = _authorizationService ?? ServerAuthorizationService.Instance;
            var canExecute = dataObject.CanExecuteCurrentResource(resource, instance);

            // Build EsbExecutionRequest - Internal Services Require This ;)
            var esbExecuteRequest = new EsbExecuteRequest { ServiceName = serviceName };
            foreach (string key in webRequest.Variables)
            {
                esbExecuteRequest.AddArgument(key, new StringBuilder(webRequest.Variables[key]));
            }
            Dev2Logger.Debug("About to execute web request [ " + serviceName + " ] for User [ " + user?.Identity?.Name + " : " + user?.Identity?.AuthenticationType + " : " + user?.Identity?.IsAuthenticated + " ] with DataObject Payload [ " + dataObject.RawPayload + " ]");
            var executionDlid = GlobalConstants.NullDataListID;
            var formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            if (canExecute && dataObject.ReturnType != EmitionTypes.SWAGGER)
            {
                ErrorResultTO errors = null;
                Thread.CurrentPrincipal = user;
                var userPrinciple = user;
                if (dataObject.ReturnType == EmitionTypes.TEST && dataObject.TestName == "*")
                {
                    formatter = ServiceTestExecutor.ExecuteTests(serviceName, dataObject, formatter, userPrinciple, workspaceGuid, serializer, _testCatalog, _resourceCatalog, ref executePayload);
                    return new StringResponseWriter(executePayload, formatter.ContentType);
                }

                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => { executionDlid = esbEndpoint.ExecuteRequest(dataObject, esbExecuteRequest, workspaceGuid, out errors); });
                allErrors.MergeErrors(errors);
            }
            else if (!canExecute)
            {
                allErrors.AddError("Executing a service externally requires View and Execute permissions");
            }

            formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
            if (dataObject.IsServiceTestExecution)
            {
                executePayload = ServiceTestExecutor.SetupForTestExecution(serializer, esbExecuteRequest, dataObject);
                return new StringResponseWriter(executePayload, formatter.ContentType);
            }
            if (dataObject.IsDebugFromWeb)
            {
                var serialize = SetupForWebExecution(dataObject, serializer);
                return new StringResponseWriter(serialize, formatter.ContentType);
            }

            var unionedErrors = dataObject.Environment?.Errors?.Union(dataObject.Environment?.AllErrors) ?? new List<string>();
            foreach (var error in unionedErrors)
            {
                if (error.Length > 0)
                {
                    allErrors.AddError(error, true);
                }
            }

            var executionDto = new ExecutionDto
            {
                WebRequestTO = webRequest,
                ServiceName = serviceName,
                DataObject = dataObject,
                Request = esbExecuteRequest,
                DataListIdGuid = executionDlid,
                WorkspaceID = workspaceGuid,
                Resource = resource,
                DataListFormat = formatter,
                PayLoad = executePayload,
                Serializer = serializer,
                ErrorResultTO = allErrors
            };
            return executionDto.CreateResponseWriter();

        }

        private static string SetupForWebExecution(IDSFDataObject dataObject, Dev2JsonSerializer serializer)
        {
            var fetchDebugItems = WebDebugMessageRepo.Instance.FetchDebugItems(dataObject.ClientID, dataObject.DebugSessionID);
            var remoteDebugItems = fetchDebugItems?.Where(state => state.StateType != StateType.Duration).ToArray() ??
                                   new IDebugState[] { };
            var debugStates = DebugStateTreeBuilder.BuildTree(remoteDebugItems);
            var serialize = serializer.Serialize(debugStates);
            return serialize;
        }

        private static Guid SetWorkspaceId(string workspaceId, IWorkspaceRepository workspaceRepository)
        {
            Guid workspaceGuid;
            if (workspaceId != null)
            {
                if (!Guid.TryParse(workspaceId, out workspaceGuid))
                {
                    workspaceGuid = workspaceRepository.ServerWorkspace.ID;
                }
            }
            else
            {
                workspaceGuid = workspaceRepository.ServerWorkspace.ID;
            }
            return workspaceGuid;
        }
        protected static void RemoteInvoke(NameValueCollection headers, IDSFDataObject dataObject) => dataObject.SetupForRemoteInvoke(headers);

        protected static void BindRequestVariablesToDataObject(WebRequestTO request, ref IDSFDataObject dataObject)
        {
            request.BindRequestVariablesToDataObject(ref dataObject);
        }

        protected static string GetPostData(ICommunicationContext ctx)
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
                        Dev2Logger.Error("AbstractWebRequestHandler", ex);
                    }
                }
            }

            return string.Empty;
        }

        private static string ExtractKeyValuePairForPostMethod(ICommunicationContext ctx, StreamReader reader)
        {
            var data = reader.ReadToEnd();
            if (DataListUtil.IsXml(data) || DataListUtil.IsJson(data))
            {
                return data;
            }

            var pairs = new NameValueCollection(5);
            var keyValuePairs = data.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var keyValuePair in keyValuePairs)
            {
                var keyValue = keyValuePair.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length > 1)
                {
                    pairs.Add(keyValue[0], keyValue[1]);
                }
                else if (keyValue.Length == 1)
                {
                    if (keyValue[0].IsXml() || keyValue[0].IsJSON())
                    {
                        pairs.Add(keyValue[0], keyValue[0]);
                    }
                }
            }

            if (pairs.Count == 0)
            {
                pairs = ctx.Request.QueryString;
            }

            return ExtractKeyValuePairs(pairs, ctx.Request.BoundVariables);
        }

        private static string ExtractKeyValuePairForGetMethod(ICommunicationContext ctx, string payload)
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

        private static string CleanupXml(string baseStr)
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
                    if (txt.IsXml())
                    {
                        results.Add(arg + "=" + string.Format(GlobalConstants.XMLPrefix + "{0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(txt))));
                    }
                    else
                    {
                        results.Add($"{arg}={txt}");
                    }
                }

                return url + string.Join("&", results);
            }
            return baseStr;
        }

        private static string ExtractKeyValuePairs(NameValueCollection pairs, NameValueCollection boundVariables)
        {
            // Extract request keys ;)
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

        protected static string GetServiceName(ICommunicationContext ctx)
        {
            return ctx.GetServiceName();
        }

        
        protected static string GetWorkspaceID(ICommunicationContext ctx)
        {
            return ctx.GetWorkspaceID();
        }

        protected static string GetDataListID(ICommunicationContext ctx)
        {
            return ctx.GetDataListID();
        }

        protected static string GetBookmark(ICommunicationContext ctx)
        {
            return ctx.GetBookmark();
        }

        protected static string GetInstanceID(ICommunicationContext ctx)
        {
            return ctx.GetInstanceID();
        }

        protected static string GetWebsite(ICommunicationContext ctx)
        {
            return ctx.GetWebsite();
        }

        protected static string GetPath(ICommunicationContext ctx)
        {
            return ctx.GetPath();
        }

        protected static string GetClassName(ICommunicationContext ctx)
        {
            return ctx.GetClassName();
        }

        protected static string GetMethodName(ICommunicationContext ctx)
        {
            return ctx.GetMethodName();
        }
    }
}
