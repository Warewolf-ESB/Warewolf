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
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.Decision;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Services.Security;
using Dev2.Web;
using Dev2.Workspaces;
using Newtonsoft.Json.Linq;
using Warewolf.Storage;
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable CyclomaticComplexity
// ReSharper disable FunctionComplexityOverflow
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Runtime.WebServer.Handlers
{
    public abstract class AbstractWebRequestHandler : IRequestHandler
    {
        string _location;
        public string Location => _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        public abstract void ProcessRequest(ICommunicationContext ctx);

        protected static IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        {
            var executePayload = "";
            Guid workspaceGuid;

            if(workspaceId != null)
            {
                if(!Guid.TryParse(workspaceId, out workspaceGuid))
                {
                    workspaceGuid = WorkspaceRepository.Instance.ServerWorkspace.ID;
                }
            }
            else
            {
                workspaceGuid = WorkspaceRepository.Instance.ServerWorkspace.ID;
            }

            var allErrors = new ErrorResultTO();
            IDSFDataObject dataObject = new DsfDataObject(webRequest.RawRequestPayload, GlobalConstants.NullDataListID, webRequest.RawRequestPayload) { IsFromWebServer = true, ExecutingUser = user, ServiceName = serviceName, WorkspaceID = workspaceGuid };

            // now bind any variables that are part of the path arguments ;)
            BindRequestVariablesToDataObject(webRequest, ref dataObject);

            // now process headers ;)
            if(headers != null)
            {
                Dev2Logger.Debug("Remote Invoke");

                var isRemote = headers.Get(HttpRequestHeader.Cookie.ToString());
                var remoteId = headers.Get(HttpRequestHeader.From.ToString());

                if(isRemote != null && remoteId != null)
                {
                    if(isRemote.Equals(GlobalConstants.RemoteServerInvoke))
                    {
                        // we have a remote invoke ;)
                        dataObject.RemoteInvoke = true;
                    }
                    if(isRemote.Equals(GlobalConstants.RemoteDebugServerInvoke))
                    {
                        // we have a remote invoke ;)
                        dataObject.RemoteNonDebugInvoke = true;
                    }

                    dataObject.RemoteInvokerID = remoteId;
                }
            }

            // now set the emition type ;)

            int loc;
            if((!string.IsNullOrEmpty(serviceName) && (loc = serviceName.LastIndexOf(".", StringComparison.Ordinal)) > 0))
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.XML;

                if(loc > 0)
                {
                    var typeOf = serviceName.Substring(loc + 1).ToUpper();
                    EmitionTypes myType;
                    if(Enum.TryParse(typeOf, out myType))
                    {
                        dataObject.ReturnType = myType;
                    }

                    if(typeOf.StartsWith("tests", StringComparison.InvariantCultureIgnoreCase))
                    {
                        dataObject.IsServiceTestExecution = true;
                        var idx = serviceName.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
                        if(idx > loc)
                        {
                            var testName = serviceName.Substring(idx + 1).ToUpper();
                            dataObject.TestName = string.IsNullOrEmpty(testName) ? "*" : testName;
                        }
                        else
                        {
                            dataObject.TestName = "*";
                        }
                        dataObject.ReturnType = EmitionTypes.TEST;
                    }

                    if(typeOf.Equals("api", StringComparison.OrdinalIgnoreCase))
                    {
                        dataObject.ReturnType = EmitionTypes.SWAGGER;
                    }
                    serviceName = serviceName.Substring(0, loc);
                    dataObject.ServiceName = serviceName;
                }
            }
            else
            {
                if(headers != null)
                {
                    var contentType = headers.Get("Content-Type");
                    if(string.IsNullOrEmpty(contentType))
                    {
                        contentType = headers.Get("ContentType");
                    }
                    if(!string.IsNullOrEmpty(contentType))
                    {
                        if(contentType.ToLowerInvariant().Contains("json"))
                        {
                            dataObject.ReturnType = EmitionTypes.JSON;
                        }
                        if(contentType.ToLowerInvariant().Contains("xml"))
                        {
                            dataObject.ReturnType = EmitionTypes.XML;
                        }
                    }
                }
                else
                {
                    dataObject.ReturnType = EmitionTypes.XML;
                }
            }

            if(IsRunAllTestsRequest(webRequest, serviceName))
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.TEST;
                dataObject.IsServiceTestExecution = true;
                dataObject.TestName = "*";
            }
            else
            {
                if(headers != null)
                {
                    var contentType = headers.Get("Content-Type");
                    if(string.IsNullOrEmpty(contentType))
                    {
                        contentType = headers.Get("ContentType");
                    }
                    if(!string.IsNullOrEmpty(contentType) && !dataObject.IsServiceTestExecution)
                    {
                        if(contentType.ToLowerInvariant().Contains("json"))
                        {
                            dataObject.ReturnType = EmitionTypes.JSON;
                        }
                        if(contentType.ToLowerInvariant().Contains("xml"))
                        {
                            dataObject.ReturnType = EmitionTypes.XML;
                        }
                    }
                }
                else
                {
                    dataObject.ReturnType = EmitionTypes.XML;
                }
            }
            if(dataObject.ServiceName == null)
            {
                dataObject.ServiceName = serviceName;
            }
            IResource resource = null;
            Guid resourceID;
            if(Guid.TryParse(serviceName, out resourceID))
            {
                resource = ResourceCatalog.Instance.GetResource(dataObject.WorkspaceID, resourceID);
                if(resource != null)
                {
                    dataObject.ServiceName = resource.ResourceName;
                    dataObject.ResourceID = resource.ResourceID;
                    dataObject.SourceResourceID = resource.ResourceID;
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(dataObject.ServiceName))
                {
                    resource = ResourceCatalog.Instance.GetResource(dataObject.WorkspaceID, dataObject.ServiceName);
                    if(resource != null)
                    {
                        dataObject.ResourceID = resource.ResourceID;
                        dataObject.SourceResourceID = resource.ResourceID;
                    }
                }
            }
            if(IsRunAllTestsRequest(webRequest, serviceName))
            {
                var pathOfAllResources = GetForAllResources(webRequest);
                dataObject.ResourceID = Guid.Empty;
                if(string.IsNullOrEmpty(pathOfAllResources))
                {
                    var resources = ResourceCatalog.Instance.GetResources(GlobalConstants.ServerWorkspaceID);
                    dataObject.TestsResourceIds = resources.Select(p => p.ResourceID).ToList();
                }
                else
                {
                    var resources = ResourceCatalog.Instance.GetResources(GlobalConstants.ServerWorkspaceID);
                    var resourcesToRunTestsFor = resources.Where(a => a.GetResourcePath(GlobalConstants.ServerWorkspaceID).StartsWith(pathOfAllResources, StringComparison.InvariantCultureIgnoreCase));
                    dataObject.TestsResourceIds = resourcesToRunTestsFor.Select(p => p.ResourceID).ToList();
                }
            }
            var serializer = new Dev2JsonSerializer();
            var esbEndpoint = new EsbServicesEndpoint();
            dataObject.EsbChannel = esbEndpoint;
            var canExecute = true;
            if(ServerAuthorizationService.Instance != null && dataObject.ReturnType != EmitionTypes.TEST)
            {
                var authorizationService = ServerAuthorizationService.Instance;
                var hasView = authorizationService.IsAuthorized(AuthorizationContext.View, dataObject.ResourceID.ToString());
                var hasExecute = authorizationService.IsAuthorized(AuthorizationContext.Execute, dataObject.ResourceID.ToString());
                canExecute = (hasExecute && hasView) || ((dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke) && hasExecute) || (resource != null && resource.ResourceType == "ReservedService");
            }
            // Build EsbExecutionRequest - Internal Services Require This ;)
            var esbExecuteRequest = new EsbExecuteRequest { ServiceName = serviceName };
            foreach(string key in webRequest.Variables)
            {
                esbExecuteRequest.AddArgument(key, new StringBuilder(webRequest.Variables[key]));
            }
            Dev2Logger.Debug("About to execute web request [ " + serviceName + " ] for User [ "+user?.Identity?.Name+" : "+user?.Identity?.AuthenticationType+" : "+ user?.Identity?.IsAuthenticated + " ] with DataObject Payload [ " + dataObject.RawPayload + " ]");
            var executionDlid = GlobalConstants.NullDataListID;
            var formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
            if(canExecute && dataObject.ReturnType != EmitionTypes.SWAGGER)
            {
                ErrorResultTO errors = null;
                Thread.CurrentPrincipal = user;
                var userPrinciple = user;
                if(dataObject.ReturnType == EmitionTypes.TEST && dataObject.TestName == "*")
                {
                    if(dataObject.TestsResourceIds?.Any() ?? false)
                    {
                        foreach(var testsResourceId in dataObject.TestsResourceIds)
                        {
                            var allTests = TestCatalog.Instance.Fetch(testsResourceId);
                            var taskList = new List<Task>();
                            var testResults = new List<IServiceTestModelTO>();
                            foreach(var test in allTests)
                            {
                                dataObject.ResourceID = testsResourceId;
                                var dataObjectClone = dataObject.Clone();
                                dataObjectClone.Environment = new ExecutionEnvironment();
                                dataObjectClone.TestName = test.TestName;
                                var res = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, testsResourceId);
                                var resourcePath = res.GetResourcePath(GlobalConstants.ServerWorkspaceID).Replace("\\", "/");

                                var lastTask = GetTaskForTestExecution(resourcePath, userPrinciple, workspaceGuid, serializer, testResults, dataObjectClone);
                                taskList.Add(lastTask);
                            }
                            Task.WaitAll(taskList.ToArray());

                            formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                            var objArray = new List<JObject>();
                            foreach(var testRunResult in testResults)
                            {
                                if(testRunResult != null)
                                {
                                    var resObj = BuildTestResultForWebRequest(testRunResult);
                                    objArray.Add(resObj);
                                }
                            }

                            executePayload = executePayload + Environment.NewLine + serializer.Serialize(objArray);
                        }
                        dataObject.ResourceID = Guid.Empty;
                    }
                    else
                    {
                        var allTests = TestCatalog.Instance.Fetch(dataObject.ResourceID);
                        var taskList = new List<Task>();
                        var testResults = new List<IServiceTestModelTO>();
                        foreach(var test in allTests.Where(to => to.Enabled))
                        {
                            var dataObjectClone = dataObject.Clone();
                            dataObjectClone.Environment = new ExecutionEnvironment();
                            dataObjectClone.TestName = test.TestName;
                            var lastTask = GetTaskForTestExecution(serviceName, userPrinciple, workspaceGuid, serializer, testResults, dataObjectClone);
                            taskList.Add(lastTask);
                        }
                        Task.WaitAll(taskList.ToArray());

                        formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                        var objArray = new List<JObject>();
                        foreach(var testRunResult in testResults)
                        {
                            if(testRunResult != null)
                            {
                                var resObj = BuildTestResultForWebRequest(testRunResult);
                                objArray.Add(resObj);
                            }
                        }

                        executePayload = serializer.Serialize(objArray);
                    }

                    Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
                    dataObject.Environment = null;
                    return new StringResponseWriter(executePayload, formatter.ContentType);
                }
                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => { executionDlid = esbEndpoint.ExecuteRequest(dataObject, esbExecuteRequest, workspaceGuid, out errors); });
                allErrors.MergeErrors(errors);
            }
            else if(!canExecute)
            {
                allErrors.AddError("Executing a service externally requires View and Execute permissions");
            }

            if(dataObject.IsServiceTestExecution)
            {
                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                var result = serializer.Deserialize<ServiceTestModelTO>(esbExecuteRequest.ExecuteResult);
                if(result != null)
                {
                    var resObj = BuildTestResultForWebRequest(result);
                    executePayload = serializer.Serialize(resObj);
                    Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
                    dataObject.Environment = null;
                }
                else
                {
                    executePayload = serializer.Serialize(new JObject());
                }
                return new StringResponseWriter(executePayload, formatter.ContentType);
            }

            foreach(var error in dataObject.Environment.Errors.Union(dataObject.Environment.AllErrors))
            {
                if(error.Length > 0)
                {
                    allErrors.AddError(error, true);
                }
            }

            if(!dataObject.Environment.HasErrors())
            {
                if(!esbExecuteRequest.WasInternalService)
                {
                    dataObject.DataListID = executionDlid;
                    dataObject.WorkspaceID = workspaceGuid;
                    dataObject.ServiceName = serviceName;

                    if(!dataObject.IsDebug || dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke)
                    {
                        if(resource?.DataList != null)
                        {
                            if(dataObject.ReturnType == EmitionTypes.JSON)
                            {
                                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                                executePayload = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject, resource.DataList.ToString(), 0);
                            }
                            else if(dataObject.ReturnType == EmitionTypes.XML)
                            {
                                executePayload = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObject, resource.DataList.ToString(), 0);
                            }
                            else if(dataObject.ReturnType == EmitionTypes.SWAGGER)
                            {
                                formatter = DataListFormat.CreateFormat("SWAGGER", EmitionTypes.SWAGGER, "application/json");
                                executePayload = ExecutionEnvironmentUtils.GetSwaggerOutputForService(resource, resource.DataList.ToString(), webRequest.WebServerUrl);
                            }
                        }
                    }
                    else
                    {
                        executePayload = string.Empty;
                    }
                }
                else
                {
                    // internal service request we need to return data for it from the request object ;)

                    executePayload = string.Empty;
                    var msg = serializer.Deserialize<ExecuteMessage>(esbExecuteRequest.ExecuteResult);

                    if(msg != null)
                    {
                        executePayload = msg.Message.ToString();
                    }

                    // out fail safe to return different types of data from services ;)
                    if(string.IsNullOrEmpty(executePayload))
                    {
                        executePayload = esbExecuteRequest.ExecuteResult.ToString();
                    }
                }
            }
            else
            {
                if(dataObject.ReturnType == EmitionTypes.XML)
                {
                    executePayload =
                        "<FatalError> <Message> An internal error occurred while executing the service request </Message>";
                    executePayload += allErrors.MakeDataListReady();
                    executePayload += "</FatalError>";
                }
                else
                {
                    executePayload =
                        "{ \"FatalError\": \"An internal error occurred while executing the service request\",";
                    executePayload += allErrors.MakeDataListReady(false);
                    executePayload += "}";
                }
            }

            Dev2Logger.Debug("Execution Result [ " + executePayload + " ]");
            if (!dataObject.Environment.HasErrors() && esbExecuteRequest.WasInternalService)
            {

                if (executePayload.IsJSON())
                {
                    formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                }else if (executePayload.IsXml())
                {
                    formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");
                }                
            }
            Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
            dataObject.Environment = null;
            return new StringResponseWriter(executePayload, formatter.ContentType);
        }

        private static string GetForAllResources(WebRequestTO webRequest)
        {
            var publicvalue = webRequest.Variables["isPublic"];
            var isPublic = bool.Parse(publicvalue ?? "False");
            var path = "";
            var webServerUrl = webRequest.WebServerUrl;
            if (isPublic)
            {
                var pathStartIndex = webServerUrl.IndexOf("public/", StringComparison.InvariantCultureIgnoreCase);
                path = webServerUrl.Substring(pathStartIndex).Replace("/.tests", "").Replace("public", "").Replace("Public", "").TrimStart('/').TrimEnd('/');
            }
            if (!isPublic)
            {
                var pathStartIndex = webServerUrl.IndexOf("secure/", StringComparison.InvariantCultureIgnoreCase);
                path = webServerUrl.Substring(pathStartIndex).Replace("/.tests", "").Replace("secure", "").Replace("Secure", "").TrimStart('/').TrimEnd('/');
            }
            return path.Replace("/", "\\");
        }

        private static bool IsRunAllTestsRequest(WebRequestTO webRequest, string serviceName)
        {
            return !string.IsNullOrEmpty(serviceName) && serviceName == "*" && webRequest.WebServerUrl.EndsWith("/.tests", StringComparison.InvariantCultureIgnoreCase);
        }

        private static async Task GetTaskForTestExecution(string serviceName, IPrincipal userPrinciple, Guid workspaceGuid, Dev2JsonSerializer serializer, List<IServiceTestModelTO> testResults, IDSFDataObject dataObjectClone)
        {
            var lastTask = Task.Run(() =>
            {
                var interTestRequest = new EsbExecuteRequest { ServiceName = serviceName };
                var dataObjectToUse = dataObjectClone;
                Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () =>
                {
                    var esbEndpointClone = new EsbServicesEndpoint();
                    ErrorResultTO errs;
                    esbEndpointClone.ExecuteRequest(dataObjectToUse, interTestRequest, workspaceGuid, out errs);
                });
                var result = serializer.Deserialize<ServiceTestModelTO>(interTestRequest.ExecuteResult);
                if (result == null)
                {
                    if (interTestRequest.ExecuteResult != null)
                    {
                        var r = serializer.Deserialize<TestRunResult>(interTestRequest.ExecuteResult.ToString()) ?? new TestRunResult {TestName = dataObjectToUse.TestName};
                        result = new ServiceTestModelTO { Result = r, TestName = r.TestName };
                    }
                }
                Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObjectToUse.DataListID);
                dataObjectToUse.Environment = null;
                testResults.Add(result);
            });
            await lastTask;
        }

        private static JObject BuildTestResultForWebRequest(IServiceTestModelTO result)
        {
            var resObj = new JObject { { "Test Name", result.TestName } };
            if (result.Result.RunTestResult == RunResult.TestPassed)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_PassedResult);
            }
            else if (result.Result.RunTestResult == RunResult.TestFailed)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_FailureResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine,""));
            }
            else if (result.Result.RunTestResult == RunResult.TestInvalid)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_InvalidResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestResourceDeleted)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_ResourceDeleteResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }
            else if (result.Result.RunTestResult == RunResult.TestResourcePathUpdated)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_ResourcpathUpdatedResult);
                resObj.Add("Message", result.Result.Message.Replace(Environment.NewLine, ""));
            }

            else if (result.Result.RunTestResult == RunResult.TestPending)
            {
                resObj.Add("Result", Warewolf.Resource.Messages.Messages.Test_PendingResult);
                resObj.Add("Message", result.Result.Message);
            }
            return resObj;
        }

        protected static void BindRequestVariablesToDataObject(WebRequestTO request, ref IDSFDataObject dataObject)
        {
            if (dataObject != null && request != null)
            {
                if (!string.IsNullOrEmpty(request.Bookmark))
                {
                    dataObject.CurrentBookmarkName = request.Bookmark;
                }

                if (!string.IsNullOrEmpty(request.InstanceID))
                {
                    Guid tmpId;
                    if (Guid.TryParse(request.InstanceID, out tmpId))
                    {
                        dataObject.WorkflowInstanceId = tmpId;
                    }
                }

                if (!string.IsNullOrEmpty(request.ServiceName) && string.IsNullOrEmpty(dataObject.ServiceName))
                {
                    dataObject.ServiceName = request.ServiceName;
                }
                foreach (string key in request.Variables)
                {
                    dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(key), request.Variables[key], 0);
                }

            }
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

            if (ctx.Request.Method == "POST")
            {
                using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    try
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
                    catch (Exception ex)
                    {
                        Dev2Logger.Error("AbstractWebRequestHandler", ex);
                    }
                }
            }

            return string.Empty;
        }

        static string CleanupXml(string baseStr)
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

        static string ExtractKeyValuePairs(NameValueCollection pairs, NameValueCollection boundVariables)
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

            var errors = new ErrorResultTO();

            if (errors.HasErrors())
            {
                Dev2Logger.Error(errors.MakeDisplayReady());
            }
            return string.Empty;
        }

        protected static string GetServiceName(ICommunicationContext ctx)
        {
            var serviceName = ctx.Request.BoundVariables["servicename"];
            return serviceName;
        }

        // ReSharper disable InconsistentNaming
        protected static string GetWorkspaceID(ICommunicationContext ctx)
        {
            return ctx.Request.QueryString["wid"];
        }

        protected static string GetDataListID(ICommunicationContext ctx)
        {
            return ctx.Request.QueryString[GlobalConstants.DLID];
        }

        protected static string GetBookmark(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["bookmark"];
        }

        protected static string GetInstanceID(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["instanceid"];
        }

        protected static string GetWebsite(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["website"];
        }

        protected static string GetPath(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["path"];
        }

        protected static string GetClassName(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["name"];
        }

        protected static string GetMethodName(ICommunicationContext ctx)
        {
            return ctx.Request.BoundVariables["action"];
        }
    }
}
