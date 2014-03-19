using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.WebServer.Responses;
using Dev2.Server.DataList.Translators;
using Dev2.Web;
using Dev2.Workspaces;
using DEV2.MultiPartFormPasser;
using Unlimited.Framework;

namespace Dev2.Runtime.WebServer.Handlers
{
    public abstract class AbstractWebRequestHandler : IRequestHandler
    {
        protected readonly List<DataListFormat> PublicFormats = new DataListTranslatorFactory().FetchAllFormats().Where(c => c.ContentType != "").ToList();
        string _location;
        public string Location { get { return _location ?? (_location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); } }

        public abstract void ProcessRequest(ICommunicationContext ctx);

        protected static IResponseWriter CreateForm(dynamic d, string serviceName, string workspaceID, NameValueCollection headers, List<DataListFormat> publicFormats, IPrincipal user = null)
        {
            // properly setup xml extraction ;)
            string payload = String.Empty;
            if(d.PostData is string)
            {
                payload = d.PostData;
                payload = payload.Replace(GlobalConstants.PostDataStart, "").Replace(GlobalConstants.PostDataEnd, "");
                payload = payload.Replace("<Payload>", "<XmlData>").Replace("</Payload>", "</XmlData>");
            }


            string correctedUri = d.XmlString.Replace("&", "").Replace(GlobalConstants.PostDataStart, "").Replace(GlobalConstants.PostDataEnd, "");
            correctedUri = correctedUri.Replace("<Payload>", "<XmlData>").Replace("</Payload>", "</XmlData>");
            string executePayload;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid workspaceGuid;

            if(workspaceID != null)
            {
                if(!Guid.TryParse(workspaceID, out workspaceGuid))
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
            var dataObject = new DsfDataObject(correctedUri, GlobalConstants.NullDataListID, payload) { IsFromWebServer = true };
            dataObject.ExecutingUser = user;
            dataObject.ServiceName = serviceName;

            // now process headers ;)
            if(headers != null)
            {
                var isRemote = headers.Get(HttpRequestHeader.Cookie.ToString());
                var remoteID = headers.Get(HttpRequestHeader.From.ToString());

                if(isRemote != null && remoteID != null)
                {
                    if(isRemote.Equals(GlobalConstants.RemoteServerInvoke))
                    {
                        // we have a remote invoke ;)
                        dataObject.RemoteInvoke = true;
                    }

                    dataObject.RemoteInvokerID = remoteID;
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

            var esbEndpoint = new EsbServicesEndpoint();

            // Build EsbExecutionRequest - Internal Services Require This ;)
            EsbExecuteRequest esbExecuteRequest = new EsbExecuteRequest { ServiceName = serviceName };

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

                    // out fail safe to return differnt types of data from services ;)
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
                        "<FatalError> <Message> An internal error occured while executing the service request </Message>";
                    executePayload += allErrors.MakeDataListReady();
                    executePayload += "</FatalError>";
                }
                else
                {
                    // convert output to JSON ;)
                    executePayload =
                        "{ \"FatalError\": \"An internal error occured while executing the service request\",";
                    executePayload += allErrors.MakeDataListReady(false);
                    executePayload += "}";
                }
            }


            // Clean up the datalist from the server
            if(!dataObject.WorkflowResumeable && executionDlid != GlobalConstants.NullDataListID)
            {
                compiler.ForceDeleteDataListByID(executionDlid);
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

        protected static string GetPostData(ICommunicationContext ctx, string postDataListID)
        {
            var formData = new UnlimitedObject();

            var isXmlData = false;

            var baseStr = HttpUtility.UrlDecode(ctx.Request.Uri.ToString());
            if(baseStr != null)
            {
                var startIdx = baseStr.IndexOf("?", StringComparison.Ordinal);
                if(startIdx > 0)
                {
                    var payload = baseStr.Substring((startIdx + 1));
                    if(payload.IsXml())
                    {
                        formData = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(payload);
                        isXmlData = true;
                    }
                }
            }

            if(!isXmlData)
            {
                foreach(var key in ctx.Request.QueryString.AllKeys)
                {
                    formData.CreateElement(key).SetValue(ctx.Request.QueryString[key]);
                }

                if((!String.IsNullOrEmpty(ctx.Request.ContentType)) && ctx.Request.ContentType.ToUpper().Contains("MULTIPART"))
                {
                    var parser = new MultipartDataParser(ctx.Request.InputStream, ctx.Request.ContentType, ctx.Request.ContentEncoding);
                    var results = parser.ParseStream();

                    results.ForEach(result =>
                    {
                        var textObj = result.FormValue as TextObj;
                        if(textObj != null)
                        {
                            formData.CreateElement(result.FormKey).SetValue(textObj.ValueVar);
                        }
                        else
                        {
                            var fileObj = result.FormValue as FileObj;
                            if(fileObj != null)
                            {
                                if(fileObj.ValueVar.LongLength > 0)
                                {
                                    formData.CreateElement(result.FormKey).SetValue(Convert.ToBase64String(fileObj.ValueVar));
                                    formData.CreateElement(String.Format("{0}_filename", result.FormKey)).SetValue(fileObj.FileName);
                                }
                            }
                        }
                    });
                }
                else
                {
                    string data;
                    if(postDataListID != null && new Guid(postDataListID) != Guid.Empty)
                    {
                        //TrevorCake
                        var dlid = new Guid(postDataListID);
                        ErrorResultTO errors;
                        string error;
                        var datalListServer = DataListFactory.CreateDataListServer();
                        var dataList = datalListServer.ReadDatalist(dlid, out errors);
                        datalListServer.DeleteDataList(dlid, false);
                        IBinaryDataListEntry dataListEntry;
                        dataList.TryGetEntry(GlobalConstants.PostData, out dataListEntry, out error);
                        data = dataListEntry.FetchScalar().TheValue;
                    }
                    else
                    {
                        using(var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                        {
                            try
                            {
                                data = reader.ReadToEnd();
                            }
                            catch(Exception ex)
                            {
                                ServerLogger.LogError("AbstractWebRequestHandler", ex);
                                data = "";
                            }
                        }
                    }

                    try
                    {
                        if(DataListUtil.IsXml(data))
                        {
                            formData.Add(new UnlimitedObject(XElement.Parse(data)));
                        }
                        else if(data.StartsWith("{") && data.EndsWith("}")) // very simple JSON check!!!
                        {
                            formData.CreateElement("Args").SetValue(data);
                        }
                        else
                        {
                            var keyValuePairs = data.Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            foreach(var keyValuePair in keyValuePairs)
                            {
                                var keyValue = keyValuePair.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                if(keyValue.Length > 1)
                                {
                                    if(keyValue[1].StartsWith("{") && keyValue[keyValue.Length - 1].EndsWith("}"))
                                    {
                                        var parameterName = keyValue[0];
                                        var jsonData = keyValue.ToList();
                                        jsonData.Remove(parameterName);
                                        formData.CreateElement(parameterName).SetValue(String.Join("=", jsonData));
                                        continue;
                                    }
                                    var formFieldValue = HttpUtility.UrlDecode(keyValue[1]);
                                    try
                                    {
                                        // ReSharper disable AssignNullToNotNullAttribute
                                        formFieldValue = XElement.Parse(formFieldValue).ToString();
                                        // ReSharper restore AssignNullToNotNullAttribute
                                    }
                                    catch(Exception ex)
                                    {
                                        ServerLogger.LogError("AbstractWebRequestHandler", ex);
                                    }
                                    formData.CreateElement(keyValue[0]).SetValue(formFieldValue);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        ServerLogger.LogError("AbstractWebRequestHandler", ex);
                    }
                }
            }

            // Still need to remvove the rubish from the string
            var tmpOut = formData.XmlString.Replace("<XmlData>", GlobalConstants.PostDataStart).Replace("</XmlData>", GlobalConstants.PostDataEnd).Replace("<XmlData />", "");
            // Replace the DataList tag for test... still a hack
            tmpOut = tmpOut.Replace("<DataList>", String.Empty).Replace("</DataList>", String.Empty);

            return tmpOut;
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
