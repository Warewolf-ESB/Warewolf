using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using DEV2.MultiPartFormPasser;
using Dev2.Web;
using Dev2.Workspaces;
using HttpFramework;
using Unlimited.Applications.WebServer;
using Unlimited.Applications.WebServer.Responses;
using Unlimited.Framework;

namespace Dev2
{
    public class WebRequestHandler
    {
        public static string GetPostData(ICommunicationContext ctx, string postDataListID)
        {
            var queryString = new NameValueCollection();
            if(ctx.Request.QueryString != HttpInput.Empty)
            {
                foreach(HttpInputItem item in ctx.Request.QueryString)
                {
                    queryString.Add(item.Name, item.Value);
                }
            }
            return GetPostData(ctx.Request.Uri, queryString, ctx.Request.InputStream, ctx.Request.ContentType, ctx.Request.ContentEncoding, postDataListID);
        }

        public static string GetPostData(Uri requestUri, NameValueCollection queryString, Stream requestStream, string contentType, Encoding contentEncoding, string postDataListID)
        {
            var formData = new UnlimitedObject();

            bool isXmlData = false;

            string baseStr = HttpUtility.UrlDecode(requestUri.ToString());
            if(baseStr != null)
            {
                int startIdx = baseStr.IndexOf("?", StringComparison.Ordinal);
                if(startIdx > 0)
                {

                    string payload = baseStr.Substring((startIdx + 1));

                    if(payload.IsXml())
                    {

                        formData = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(payload);
                        isXmlData = true;
                    }

                }
            }

            if(!isXmlData)
            {
                if(queryString != null && queryString.HasKeys())
                {
                    foreach(var key in queryString.AllKeys)
                    {
                        var value = queryString[key];
                        formData.CreateElement(key).SetValue(value);
                    }
                }

                if((!String.IsNullOrEmpty(contentType)) && contentType.ToUpper().Contains("MULTIPART"))
                {
                    var parser = new MultipartDataParser(requestStream, contentType, contentEncoding);
                    var results = parser.ParseStream();

                    results.ForEach(result =>
                    {
                        if(result.FormValue is TextObj)
                        {
                            var textObj = result.FormValue as TextObj;
                            if(textObj != null)
                            {

                                formData.CreateElement(result.FormKey).SetValue(textObj.ValueVar);
                            }
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
                        Guid dlid = new Guid(postDataListID);
                        ErrorResultTO errors;
                        string error;
                        IDataListServer datalListServer = DataListFactory.CreateDataListServer();
                        IBinaryDataList dataList = datalListServer.ReadDatalist(dlid, out errors);
                        datalListServer.DeleteDataList(dlid, false);
                        IBinaryDataListEntry dataListEntry;
                        dataList.TryGetEntry(GlobalConstants.PostData, out dataListEntry, out error);
                        data = dataListEntry.FetchScalar().TheValue;
                    }
                    else
                    {
                        using(var reader = new StreamReader(requestStream, contentEncoding))
                        {
                            try
                            {
                                data = reader.ReadToEnd();
                            }
                            catch(Exception ex)
                            {
                                ServerLogger.LogError(ex);
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

                            foreach(string keyValuePair in keyValuePairs)
                            {
                                var keyValue = keyValuePair.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                string formFieldValue = String.Empty;
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
                                    formFieldValue = HttpUtility.UrlDecode(keyValue[1]);
                                    try
                                    {
                                        formFieldValue = XElement.Parse(formFieldValue).ToString();
                                    }
                                    catch(Exception ex)
                                    {
                                        ServerLogger.LogError(ex);
                                    }
                                    formData.CreateElement(keyValue[0]).SetValue(formFieldValue);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        ServerLogger.LogError(ex);
                    }

                }
            }

            // Still need to remvove the rubish from the string
            string tmpOut = formData.XmlString.Replace("<XmlData>", GlobalConstants.PostDataStart).Replace("</XmlData>", GlobalConstants.PostDataEnd).Replace("<XmlData />", "");
            // Replace the DataList tag for test... still a hack
            tmpOut = tmpOut.Replace("<DataList>", String.Empty).Replace("</DataList>", String.Empty);

            return tmpOut;
        }


        public static CommunicationResponseWriter CreateForm(dynamic d, string serviceName, string clientID, NameValueCollection headers, List<DataListFormat> publicFormats)
        {
            var response = CreateResponse(d, serviceName, clientID, headers, publicFormats);
            return new StringCommunicationResponseWriter(response.Content, response.ContentType);

        }

        public static WebResponse<string> CreateResponse(dynamic d, string serviceName, string clientID, NameValueCollection headers, List<DataListFormat> publicFormats)
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
            string executePayload = null;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid clientGuid;

            if(clientID != null)
            {
                if(!Guid.TryParse(clientID, out clientGuid))
                {
                    clientGuid = WorkspaceRepository.Instance.ServerWorkspace.ID;
                }
            }
            else
            {
                clientGuid = WorkspaceRepository.Instance.ServerWorkspace.ID;
            }

            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            IDSFDataObject dataObject = new DsfDataObject(correctedUri, GlobalConstants.NullDataListID, payload);
            dataObject.IsFromWebServer = true;

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

            Guid executionDlid = esbEndpoint.ExecuteRequest(dataObject, clientGuid, out errors);
            allErrors.MergeErrors(errors);

            // Fetch return type ;)
            DataListFormat formater = publicFormats.FirstOrDefault(c => c.PublicFormatName == dataObject.ReturnType);

            // force it to XML if need be ;)
            if(formater == null)
            {
                formater = publicFormats.FirstOrDefault(c => c.PublicFormatName == EmitionTypes.XML);
            }

            // Fetch and convert DL ;)
            if(executionDlid != GlobalConstants.NullDataListID)
            {
                dataObject.DataListID = executionDlid;
                dataObject.WorkspaceID = clientGuid;
                dataObject.ServiceName = serviceName;


                executePayload = esbEndpoint.FetchExecutionPayload(dataObject, formater, out errors);

                allErrors.MergeErrors(errors);
                compiler.UpsertSystemTag(executionDlid, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
            }
            else
            {
                if(dataObject.ReturnType == EmitionTypes.XML)
                {

                    executePayload = "<FatalError> <Message> An internal error occured while executing the service request </Message>";
                    executePayload += allErrors.MakeDataListReady();
                    executePayload += "</FatalError>";
                }
                else
                {
                    // convert output to JSON ;)
                    executePayload = "{ \"FatalError\": \"An internal error occured while executing the service request\",";
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
                        const string docType = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">";
                        return new WebResponse<string> { Content = String.Format("{0}\r\n{1}", docType, result), ContentType = "text/html; charset=utf-8" };
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
                        return new WebResponse<string> { Content = executePayload, ContentType = "application/json" };
                    }
                }
            }

            // else handle the format requested ;)
            return new WebResponse<string> { Content = executePayload, ContentType = formater.ContentType };
        }


        static string CleanupHtml(string result)
        {
            string html = result;

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
    }
}
