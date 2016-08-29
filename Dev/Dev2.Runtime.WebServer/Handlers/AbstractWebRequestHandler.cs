/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Web;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.Decision;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
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
        public string Location => _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        public abstract void ProcessRequest(ICommunicationContext ctx);

        protected static IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, IPrincipal user = null)
        {
            //lock(ExecutionObject)
            {
                string executePayload = "";
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
                var dataObject = new DsfDataObject(webRequest.RawRequestPayload, GlobalConstants.NullDataListID, webRequest.RawRequestPayload) { IsFromWebServer = true, ExecutingUser = user, ServiceName = serviceName, WorkspaceID = workspaceGuid };

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
                        if (isRemote.Equals(GlobalConstants.RemoteServerInvoke) )
                        {
                            // we have a remote invoke ;)
                            dataObject.RemoteInvoke = true;
                        }
                        if (isRemote.Equals(GlobalConstants.RemoteDebugServerInvoke))
                        {
                            // we have a remote invoke ;)
                            dataObject.RemoteNonDebugInvoke = true;
                        }

                        dataObject.RemoteInvokerID = remoteId;
                    }
                }

                // now set the emition type ;)
                
                
                    int loc;
                    if (!String.IsNullOrEmpty(serviceName) && (loc = serviceName.LastIndexOf(".", StringComparison.Ordinal)) > 0)
                    {
                        // default it to xml
                        dataObject.ReturnType = EmitionTypes.XML;

                        if (loc > 0)
                        {
                            var typeOf = serviceName.Substring(loc + 1).ToUpper();
                            EmitionTypes myType;
                            if (Enum.TryParse(typeOf, out myType))
                            {
                                dataObject.ReturnType = myType;
                            }

                            // adjust the service name to drop the type ;)

                            // avoid .wiz amendments ;)
                            if (!typeOf.ToLower().Equals(GlobalConstants.WizardExt))
                            {
                                serviceName = serviceName.Substring(0, loc);
                                dataObject.ServiceName = serviceName;
                            }

                            if (typeOf.Equals("api", StringComparison.OrdinalIgnoreCase))
                            {
                                dataObject.ReturnType = EmitionTypes.SWAGGER;
                            }

                        }
                    }
                    else
                    {
                        if (headers != null)
                        {
                            var contentType = headers.Get("Content-Type");
                            if (String.IsNullOrEmpty(contentType))
                            {
                                contentType = headers.Get("Accept");
                            }
                            if (String.IsNullOrEmpty(contentType))
                            {
                                contentType = headers.Get("ContentType");
                            }
                            if (!String.IsNullOrEmpty(contentType))
                            {
                                if (contentType.ToLowerInvariant().Contains("json"))
                                {
                                    dataObject.ReturnType = EmitionTypes.JSON;
                                }
                                if (contentType.ToLowerInvariant().Contains("xml"))
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
                
                // ensure service gets set ;)
                if(dataObject.ServiceName == null)
                {
                    dataObject.ServiceName = serviceName;
                }
                IResource resource = null;
                if(!String.IsNullOrEmpty(dataObject.ServiceName))
                {
                    resource = ResourceCatalog.Instance.GetResource(dataObject.WorkspaceID, dataObject.ServiceName) ?? ResourceCatalog.Instance.GetResourceBasedOnPath(dataObject.ServiceName);
                    if(resource != null)
                    {
                        dataObject.ResourceID = resource.ResourceID;

                    }
                }
                var esbEndpoint = new EsbServicesEndpoint();
                dataObject.EsbChannel = esbEndpoint;
                var canExecute = true;
                if(ServerAuthorizationService.Instance != null)
                {
                    var authorizationService = ServerAuthorizationService.Instance;
                    var hasView = authorizationService.IsAuthorized(AuthorizationContext.View, dataObject.ResourceID.ToString());
                    var hasExecute = authorizationService.IsAuthorized(AuthorizationContext.Execute, dataObject.ResourceID.ToString());
                    canExecute = (hasExecute && hasView) || ((dataObject.RemoteInvoke || dataObject.RemoteNonDebugInvoke) && hasExecute) || (resource != null && resource.ResourceType == "ReservedService");
                }
                // Build EsbExecutionRequest - Internal Services Require This ;)
                EsbExecuteRequest esbExecuteRequest = new EsbExecuteRequest { ServiceName = serviceName };
                foreach(string key in webRequest.Variables)
                {
                    esbExecuteRequest.AddArgument(key, new StringBuilder(webRequest.Variables[key]));
                }
                Dev2Logger.Debug("About to execute web request [ " + serviceName + " ] DataObject Payload [ " + dataObject.RawPayload + " ]");
                var executionDlid = GlobalConstants.NullDataListID;
                if (canExecute && dataObject.ReturnType != EmitionTypes.SWAGGER)
                {
                    ErrorResultTO errors = null;
                    // set correct principle ;)
                    Thread.CurrentPrincipal = user;
                    var userPrinciple = user;                   
                    Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => { executionDlid = esbEndpoint.ExecuteRequest(dataObject, esbExecuteRequest, workspaceGuid, out errors); });
                    allErrors.MergeErrors(errors);
                }
                else if(!canExecute)
                {
                    allErrors.AddError("Executing a service externally requires View and Execute permissions");
                }
                foreach (var error in dataObject.Environment.Errors.Union(dataObject.Environment.AllErrors))
                {
                    if(error.Length>0)
                    allErrors.AddError(error,true);
                }
                // Fetch return type ;)
                var formatter = DataListFormat.CreateFormat("XML", EmitionTypes.XML, "text/xml");

                // force it to XML if need be ;)

                // Fetch and convert DL ;)
                if(!dataObject.Environment.HasErrors())
                {
                    // a normal service request
                    if(!esbExecuteRequest.WasInternalService)
                    {
                        dataObject.DataListID = executionDlid;
                        dataObject.WorkspaceID = workspaceGuid;
                        dataObject.ServiceName = serviceName;
                        
                        if(!dataObject.IsDebug || dataObject.RemoteInvoke ||  dataObject.RemoteNonDebugInvoke)
                        {
                            if (dataObject.ReturnType == EmitionTypes.JSON)
                            {
                                formatter = DataListFormat.CreateFormat("JSON", EmitionTypes.JSON, "application/json");
                                executePayload = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject, resource.DataList.ToString(), 0);
                            }
                            else if (dataObject.ReturnType == EmitionTypes.XML)
                            {
                                executePayload = ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObject, resource.DataList.ToString(), 0);
                            }else if(dataObject.ReturnType == EmitionTypes.SWAGGER)
                            {
                                formatter = DataListFormat.CreateFormat("SWAGGER", EmitionTypes.SWAGGER, "application/json");
                                executePayload = ExecutionEnvironmentUtils.GetSwaggerOutputForService(resource, resource.DataList.ToString(),webRequest.WebServerUrl);
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
                        var serializer = new Dev2JsonSerializer();
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
                        // convert output to JSON ;)
                        executePayload =
                            "{ \"FatalError\": \"An internal error occurred while executing the service request\",";
                        executePayload += allErrors.MakeDataListReady(false);
                        executePayload += "}";
                    }
                }


                Dev2Logger.Debug("Execution Result [ " + executePayload + " ]");


                // JSON Data ;)
                if(executePayload.IndexOf("</JSON>", StringComparison.Ordinal) >= 0)
                {
                    int start = executePayload.IndexOf(GlobalConstants.OpenJSON, StringComparison.Ordinal);
                    if(start >= 0)
                    {
                        int end = executePayload.IndexOf(GlobalConstants.CloseJSON, StringComparison.Ordinal);
                        start += GlobalConstants.OpenJSON.Length;

                        executePayload = CleanupHtml(executePayload.Substring(start, end - start));
                        if(!String.IsNullOrEmpty(executePayload))
                        {
                            return new StringResponseWriter(executePayload, ContentTypes.Json);
                        }
                    }
                }
                Dev2DataListDecisionHandler.Instance.RemoveEnvironment(dataObject.DataListID);
                dataObject.Environment = null;
                return new StringResponseWriter(executePayload, formatter.ContentType);
            }
        }

        protected static void BindRequestVariablesToDataObject(WebRequestTO request, ref DsfDataObject dataObject)
        {
            if(dataObject != null && request != null)
            {
                if(!string.IsNullOrEmpty(request.Bookmark))
                {
                    dataObject.CurrentBookmarkName = request.Bookmark;
                }

                if(!string.IsNullOrEmpty(request.InstanceID))
                {
                    Guid tmpId;
                    if(Guid.TryParse(request.InstanceID, out tmpId))
                    {
                        dataObject.WorkflowInstanceId = tmpId;
                    }
                }

                if(!string.IsNullOrEmpty(request.ServiceName) && string.IsNullOrEmpty(dataObject.ServiceName))
                {
                    dataObject.ServiceName = request.ServiceName;
                }
                foreach(string key in request.Variables)
                {
                    dataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(key),request.Variables[key],0);   
            }
                
        }
        }

        protected static string GetPostData(ICommunicationContext ctx)
        {
            var baseStr = HttpUtility.UrlDecode(ctx.Request.Uri.ToString());
            baseStr = HttpUtility.UrlDecode(CleanupXml(baseStr));
            string payload=null;
            if (baseStr != null)
            {
                var startIdx = baseStr.IndexOf("?", StringComparison.Ordinal);
                if(startIdx > 0)
                {
                    payload = baseStr.Substring(startIdx + 1);
                    if(payload.IsXml() || payload.IsJSON())
                    {
                        return payload;
                    }
                }
            }

            if(ctx.Request.Method == "GET")
            {
                if(payload != null)
                {
                    var keyValuePairs = payload.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach(var keyValuePair in keyValuePairs)
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
                return ExtractKeyValuePairs(pairs,ctx.Request.BoundVariables);
            }

            if(ctx.Request.Method == "POST")
            {
                using(var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    try
                    {
                        string data = reader.ReadToEnd();
                        if (DataListUtil.IsXml(data) || DataListUtil.IsJson(data))
                        {
                            return data;
                        }

                        

                        NameValueCollection pairs = new NameValueCollection(5);
                        var keyValuePairs = data.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        foreach(var keyValuePair in keyValuePairs)
                        {
                            var keyValue = keyValuePair.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                            if(keyValue.Length > 1)
                            {
                                pairs.Add(keyValue[0], keyValue[1]);
                            }
                            else if(keyValue.Length == 1)
                            {
                                if(keyValue[0].IsXml() || keyValue[0].IsJSON())
                                {
                                    pairs.Add(keyValue[0], keyValue[0]);
                                }
                            }
                        }

                        if(pairs.Count == 0)
                        {
                            pairs = ctx.Request.QueryString;
                        }

                        return ExtractKeyValuePairs(pairs,ctx.Request.BoundVariables);
                    }
                    catch(Exception ex)
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
                var query = baseStr.Substring(startQueryString+1);
                if(query.IsJSON())
                {
                    return baseStr;
                }
                NameValueCollection args = HttpUtility.ParseQueryString(query);
                var url = baseStr.Substring(0, startQueryString + 1);
                List<string> results = new List<string>();
                foreach (var arg in args.AllKeys)
                {
                    var txt = args[arg];
                    if(txt.IsXml())
                    {
                        results.Add(arg + "=" + string.Format(GlobalConstants.XMLPrefix + "{0}", Convert.ToBase64String(Encoding.UTF8.GetBytes(txt))));
                }
                else
                {
                        results.Add(string.Format("{0}={1}", arg, txt));
                }
            }

                return url + string.Join("&", results);
            }
            return baseStr;
        }

        static string ExtractKeyValuePairs(NameValueCollection pairs, NameValueCollection boundVariables)
        {
            // Extract request keys ;)
            foreach(var key in pairs.AllKeys)
            {
                if(key == "wid") //Don't add the Workspace ID to DataList
            {
                    continue;
                }
                if(key.IsXml() || key.IsJSON() || (key.ToLowerInvariant().Contains("<DataList>".ToLowerInvariant()) && key.ToLowerInvariant().Contains("<\\DataList>".ToLowerInvariant())))
                {
                    return key; //We have a workspace id and XML DataList
                }
                boundVariables.Add(key,pairs[key]);

            }

            ErrorResultTO errors = new ErrorResultTO();


            Dev2Logger.Error(errors.MakeDisplayReady());

            return string.Empty;
        }

        static string CleanupHtml(string result)
        {
            var html = result;

            html = html.Replace("&amp;amp;", "&");
            html = html.Replace("&lt;", "<").Replace("&gt;", ">");
            html = html.Replace("lt;", "<").Replace("gt;", ">");
            html = html.Replace("&amp;gt;", ">").Replace("&amp;lt;", "<");
            html = html.Replace("&amp;amp;amp;lt;", "<").Replace("&amp;amp;amp;gt;", ">");
            html = html.Replace("&amp;amp;lt;", "<").Replace("&amp;amp;gt;", ">");
            html = html.Replace("&<", "<").Replace("&>", ">");
            html = html.Replace("&quot;", "\"");

            return html;
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
