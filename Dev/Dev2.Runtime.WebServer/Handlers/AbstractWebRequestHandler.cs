using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Web;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Runtime.WebServer.TransferObjects;
using Dev2.Server.DataList.Translators;
using Dev2.Web;
using Dev2.Workspaces;

namespace Dev2.Runtime.WebServer.Handlers
{
    public abstract class AbstractWebRequestHandler : IRequestHandler
    {
        protected readonly List<DataListFormat> PublicFormats = new DataListTranslatorFactory().FetchAllFormats().Where(c => c.ContentType != "").ToList();
        string _location;
        public string Location { get { return _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); } }

        public abstract void ProcessRequest(ICommunicationContext ctx);

        protected static IResponseWriter CreateForm(WebRequestTO webRequest, string serviceName, string workspaceId, NameValueCollection headers, List<DataListFormat> publicFormats, IPrincipal user = null)
        {
            string executePayload;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
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

            ErrorResultTO errors;
            var allErrors = new ErrorResultTO();
            var dataObject = new DsfDataObject(webRequest.RawRequestPayload, GlobalConstants.NullDataListID, webRequest.RawRequestPayload) { IsFromWebServer = true, ExecutingUser = user, ServiceName = serviceName, WorkspaceID = workspaceGuid };

            // now bind any variables that are part of the path arguments ;)
            BindRequestVariablesToDataObject(webRequest, ref dataObject);

            // now process headers ;)
            if(headers != null)
            {
                ServerLogger.LogTrace("Remote Invoke");

                var isRemote = headers.Get(HttpRequestHeader.Cookie.ToString());
                var remoteId = headers.Get(HttpRequestHeader.From.ToString());

                if(isRemote != null && remoteId != null)
                {
                    if(isRemote.Equals(GlobalConstants.RemoteServerInvoke))
                    {
                        // we have a remote invoke ;)
                        dataObject.RemoteInvoke = true;
                    }

                    dataObject.RemoteInvokerID = remoteId;
                }
            }

            // now set the emition type ;)
            int loc;
            if(!String.IsNullOrEmpty(serviceName) && (loc = serviceName.LastIndexOf(".", StringComparison.Ordinal)) > 0)
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.XML;

                if(loc > 0)
                {
                    var typeOf = serviceName.Substring((loc + 1)).ToUpper();
                    EmitionTypes myType;
                    if(Enum.TryParse(typeOf, out myType))
                    {
                        dataObject.ReturnType = myType;
                    }

                    // adjust the service name to drop the type ;)

                    // avoid .wiz amendments ;)
                    if(!typeOf.ToLower().Equals(GlobalConstants.WizardExt))
                    {
                        serviceName = serviceName.Substring(0, loc);
                        dataObject.ServiceName = serviceName;
                    }

                }
            }
            else
            {
                // default it to xml
                dataObject.ReturnType = EmitionTypes.XML;
            }

            // ensure service gets set ;)
            if(dataObject.ServiceName == null)
            {
                dataObject.ServiceName = serviceName;
            }
            if(!String.IsNullOrEmpty(dataObject.ServiceName))
            {
                var resource = ResourceCatalog.Instance.GetResource(dataObject.WorkspaceID, dataObject.ServiceName);
                if(resource != null)
                {
                    dataObject.ResourceID = resource.ResourceID;
                }
            }
            var esbEndpoint = new EsbServicesEndpoint();

            // Build EsbExecutionRequest - Internal Services Require This ;)
            EsbExecuteRequest esbExecuteRequest = new EsbExecuteRequest { ServiceName = serviceName };

            ServerLogger.LogTrace("About to execute web request [ " + serviceName + " ] DataObject Payload [ " + dataObject.RawPayload + " ]");

            var executionDlid = esbEndpoint.ExecuteRequest(dataObject, esbExecuteRequest, workspaceGuid, out errors);
            allErrors.MergeErrors(errors);


            // Fetch return type ;)
            var formatter = publicFormats.FirstOrDefault(c => c.PublicFormatName == dataObject.ReturnType)
                            ?? publicFormats.FirstOrDefault(c => c.PublicFormatName == EmitionTypes.XML);

            // force it to XML if need be ;)

            // Fetch and convert DL ;)
            if(executionDlid != GlobalConstants.NullDataListID)
            {
                // a normal service request
                if(!esbExecuteRequest.WasInternalService)
                {
                    dataObject.DataListID = executionDlid;
                    dataObject.WorkspaceID = workspaceGuid;
                    dataObject.ServiceName = serviceName;


                    // some silly chicken thinks web request where a good idea for debug ;(
                    if(!dataObject.IsDebug || dataObject.RemoteInvoke)
                    {
                        executePayload = esbEndpoint.FetchExecutionPayload(dataObject, formatter, out errors);
                        allErrors.MergeErrors(errors);
                        compiler.UpsertSystemTag(executionDlid, enSystemTag.Dev2Error, allErrors.MakeDataListReady(),
                                                 out errors);
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


            ServerLogger.LogTrace("Execution Result [ " + executePayload + " ]");

            // Clean up the datalist from the server
            if(!dataObject.WorkflowResumeable && executionDlid != GlobalConstants.NullDataListID)
            {
                compiler.ForceDeleteDataListByID(executionDlid);
                if(dataObject.IsDebug && !dataObject.IsRemoteInvoke && !dataObject.RunWorkflowAsync)
                {
                    DataListRegistar.ClearDataList();
                }
                else
                {
                    foreach(var thread in dataObject.ThreadsToDispose)
                    {
                        DataListRegistar.DisposeScope(thread.Key, executionDlid);
                    }

                    DataListRegistar.DisposeScope(Thread.CurrentThread.ManagedThreadId, executionDlid);
                }

            }

            // old HTML throw back ;)
            if(dataObject.ReturnType == EmitionTypes.WIZ)
            {
                int start = (executePayload.IndexOf("<Dev2System.FormView>", StringComparison.Ordinal) + 21);
                int end = (executePayload.IndexOf("</Dev2System.FormView>", StringComparison.Ordinal));
                int len = (end - start);
                if(len > 0)
                {
                    if(dataObject.ReturnType == EmitionTypes.WIZ)
                    {
                        string tmp = executePayload.Substring(start, (end - start));
                        string result = CleanupHtml(tmp);
                        const string DocType = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">";
                        return new StringResponseWriter(String.Format("{0}\r\n{1}", DocType, result), ContentTypes.Html);
                    }
                }
            }

            // JSON Data ;)
            if(executePayload.IndexOf("</JSON>", StringComparison.Ordinal) >= 0)
            {
                int start = executePayload.IndexOf(GlobalConstants.OpenJSON, StringComparison.Ordinal);
                if(start >= 0)
                {
                    int end = executePayload.IndexOf(GlobalConstants.CloseJSON, StringComparison.Ordinal);
                    start += GlobalConstants.OpenJSON.Length;

                    executePayload = CleanupHtml(executePayload.Substring(start, (end - start)));
                    if(!String.IsNullOrEmpty(executePayload))
                    {
                        return new StringResponseWriter(executePayload, ContentTypes.Json);
                    }
                }
            }

            // else handle the format requested ;)
            return new StringResponseWriter(executePayload, formatter.ContentType);

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
            }
        }

        protected static string GetPostData(ICommunicationContext ctx, string postDataListId)
        {
            var baseStr = HttpUtility.UrlDecode(ctx.Request.Uri.ToString());
            if(baseStr != null)
            {
                var startIdx = baseStr.IndexOf("?", StringComparison.Ordinal);
                if(startIdx > 0)
                {
                    var payload = baseStr.Substring((startIdx + 1));
                    if(payload.IsXml())
                    {
                        return payload;
                    }
                }
            }

            // Not an XML payload - Handle it as a GET or POST request ;)
            if(ctx.Request.Method == "GET")
            {
                var pairs = ctx.Request.QueryString;
                return ExtractKeyValuePairs(pairs);
            }

            if(ctx.Request.Method == "POST")
            {
                using(var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                {
                    try
                    {
                        string data = reader.ReadToEnd();
                        if(DataListUtil.IsXml(data))
                        {
                            return data;
                        }

                        // very simple JSON check!!!
                        if(DataListUtil.IsJson(data))
                        {
                            throw new Exception("Unsupported Request : POST Request with JSON stream");
                        }

                        // Process POST key value pairs ;)
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
                                //We have DataWithout an Equals
                                if(keyValue[0].IsXml())
                                {
                                    pairs.Add(keyValue[0], keyValue[0]);
                                }
                            }
                        }

                        // so some silly chicken sent an empty body on POST with GET style query string parameters
                        // this silly chicken really needs to understand how the flipping request structure should work.
                        if(pairs.Count == 0)
                        {
                            pairs = ctx.Request.QueryString;
                        }

                        // we need to process it as key value pairs ;)
                        return ExtractKeyValuePairs(pairs);
                    }
                    catch(Exception ex)
                    {
                        ServerLogger.LogError("AbstractWebRequestHandler", ex);
                    }
                }
            }

            return string.Empty;
        }

        static string ExtractKeyValuePairs(NameValueCollection pairs)
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();
            // Extract request keys ;)
            foreach(var key in pairs.AllKeys)
            {
                if(key == "wid") //Don't add the Workspace ID to DataList
                {
                    continue;
                }
                if(key.IsXml())
                {
                    return key; //We have a workspace id and XML DataList
                }
                string error;
                bdl.TryCreateScalarTemplate(string.Empty, key, string.Empty, true, out error);
                if(!string.IsNullOrEmpty(error))
                {
                    "AbstractWebRequestHandler".LogError(error);
                }

                IBinaryDataListEntry entry;
                if(bdl.TryGetEntry(key, out entry, out error))
                {
                    var item = Dev2BinaryDataListFactory.CreateBinaryItem(pairs[key], key);
                    entry.TryPutScalar(item, out error);
                    if(!string.IsNullOrEmpty(error))
                    {
                        "AbstractWebRequestHandler".LogError(error);
                    }
                }
                else
                {
                    "AbstractWebRequestHandler".LogError(error);
                }
            }

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid pushedId = compiler.PushBinaryDataList(bdl.UID, bdl, out errors);

            if(pushedId != Guid.Empty)
            {
                var result = compiler.ConvertFrom(pushedId, DataListFormat.CreateFormat(GlobalConstants._XML), enTranslationDepth.Data, out errors);
                if(errors.HasErrors())
                {
                    "AbstractWebRequestHandler".LogError(errors.MakeDisplayReady());
                }

                return result;
            }

            "AbstractWebRequestHandler".LogError(errors.MakeDisplayReady());

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
