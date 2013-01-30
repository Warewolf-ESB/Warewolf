using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices;
using Dev2.Runtime;
using Dev2.Runtime.Diagnostics;
using DEV2.MultiPartFormPasser;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Unlimited.Applications.DynamicServicesHost;
using Unlimited.Applications.WebServer;
using Unlimited.Applications.WebServer.Responses;
using Unlimited.Framework;

namespace Dev2
{
    public sealed class WebServer : IFrameworkWebServer
    {
        #region Location

        string _location;

        public string Location
        {
            get
            {
                if(_location == null)
                {
                    _location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
                return _location;
            }
        }

        #endregion

        #region Static Members

        #region GetQueryStringValue

        static string GetQueryStringValue(ICommunicationContext ctx, string key)
        {
            var item = ctx.Request.QueryString[key];
            if(item != null)
            {
                return item.Value;
            }
            return string.Empty;
        }

        #endregion

        private static string GetPostData(ICommunicationContext ctx, dynamic serviceRequest, string postDataListID)
        {
            var formData = new UnlimitedObject();

            bool isXmlData = false;
             
            try
            {
                string baseStr = HttpUtility.UrlDecode(ctx.Request.Uri.ToString());
                int startIdx = baseStr.IndexOf("?");
                if(startIdx > 0)
                {
                    XmlDocument xDoc = new XmlDocument();
                    string payload = baseStr.Substring((startIdx + 1));
                    xDoc.LoadXml(payload);
                    formData = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(payload);
                    isXmlData = true;
                }
            }
            catch(Exception)
            {
            }

            if(!isXmlData)
            {
                if(ctx.Request.QueryString != HttpFramework.HttpInput.Empty)
                {
                    foreach(HttpFramework.HttpInputItem item in ctx.Request.QueryString)
                    {
                        formData.CreateElement(item.Name).SetValue(item.Value);
                    }
                }

                if((!string.IsNullOrEmpty(ctx.Request.ContentType)) && ctx.Request.ContentType.ToUpper().Contains("MULTIPART"))
                {
                    var parser = new MultipartDataParser(ctx.Request.InputStream, ctx.Request.ContentType, ctx.Request.ContentEncoding);
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
                                    //var newFile = formData.CreateElement("File");
                                    formData.CreateElement(result.FormKey).SetValue(Convert.ToBase64String(fileObj.ValueVar));
                                    formData.CreateElement(string.Format("{0}_filename", result.FormKey)).SetValue(fileObj.FileName);
                                    //formData.Add(newFile);
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
                        using(var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
                        {
                            try
                            {
                                data = reader.ReadToEnd();
                            }
                            catch(Exception)
                            {
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
                            var keyValuePairs = data.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                            foreach(string keyValuePair in keyValuePairs)
                            {
                                var keyValue = keyValuePair.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                string formFieldValue = string.Empty;
                                if(keyValue.Length > 1)
                                {
                                    formFieldValue = HttpUtility.UrlDecode(keyValue[1]);
                                    try
                                    {
                                        formFieldValue = XElement.Parse(formFieldValue).ToString();
                                    }
                                    catch
                                    {
                                    }
                                }
                                formData.CreateElement(keyValue[0]).SetValue(formFieldValue);
                            }
                        }
                    }
                    catch
                    {
                    }

                }
            }

            // Still need to remvove the rubish from the string
            string tmpOut = formData.XmlString.Replace("<XmlData>", GlobalConstants.PostDataStart).Replace("</XmlData>", GlobalConstants.PostDataEnd).Replace("<XmlData />", "");
            // Replace the DataList tag for test... still a hack
            tmpOut = tmpOut.Replace("<DataList>", string.Empty).Replace("</DataList>", string.Empty);

            return tmpOut;
        }

        private static string CleanupHtml(string result)
        {
            string html = result;

            html = html.Replace("&amp;amp;", "&");
            //html = HttpUtility.HtmlDecode(html);
            html = html.Replace("&lt;", "<").Replace("&gt;", ">");
            html = html.Replace("lt;", "<").Replace("gt;", ">");
            html = html.Replace("&amp;gt;", ">").Replace("&amp;lt;", "<");
            html = html.Replace("&amp;amp;amp;lt;", "<").Replace("&amp;amp;amp;gt;", ">");
            html = html.Replace("&amp;amp;lt;", "<").Replace("&amp;amp;gt;", ">");
            html = html.Replace("&<", "<").Replace("&>", ">");
            // Travis : Remove the CDATA region so we can render this on the screen
            //html = DataListUtil.CDATAUnwrapHTML(html);

            return html;
        }

        private static string Cleanup(string formValue)
        {
            string returnVal = formValue;

            returnVal = returnVal.Replace("&apos;", "'");
            returnVal = returnVal.Replace("&quot;", "\\");
            returnVal = returnVal.Replace("&amp;", "&");
            returnVal = returnVal.Replace("&lt;", "<");
            returnVal = returnVal.Replace("&gt;", ">");
            returnVal = returnVal.Replace("&", "&amp;");
            returnVal = returnVal.Replace("\"", "&quot;");
            returnVal = returnVal.Replace("'", "&apos;");
            returnVal = returnVal.Replace("<", "&lt;");
            returnVal = returnVal.Replace("<", "&gt;");

            return returnVal;
        }
        #endregion

        #region Instance Fields
        private DynamicServicesEndpoint _dsf;
        private string _endpointAddress;
        private HttpServer _server;
        private StudioNetworkServer _network;

        readonly ServiceInvoker _serviceInvoker = new ServiceInvoker();

        #endregion

        #region Constructors
        public WebServer(IPEndPoint[] endPoints, StudioNetworkServer server)
        {
            _server = new HttpServer(endPoints);
            _network = server;
        }
        #endregion

        #region [Start/Stop] Handling
        public void Start()
        {
            StartNetworkServer();
            MapContextToDirectoryStructure();
        }

        public void Start(string endpointAddress)
        {
            _endpointAddress = endpointAddress;
            StartNetworkServer();
            MapContextToDirectoryStructure();
        }

        public void Stop()
        {
            _server.Dispose();
        }
        #endregion

        #region Registration Handling

        private void MapContextToDirectoryStructure()
        {
            //TrevorCake
            //_server.AddHandler("GET", "/services/{servicename}?postdlid={dlid}", GET_CLIENT_SERVICES_Handler);
            //_server.AddHandler("GET", "/services/{servicename}?wid={clientid}", GET_CLIENT_SERVICES_Handler);
            _server.AddHandler("GET", "/services/{servicename}", GET_POST_CLIENT_SERVICES_Handler);
            _server.AddHandler("POST", "/services/{servicename}?wid={clientid}", POST_CLIENT_SERVICES_Handler);
            _server.AddHandler("POST", "/services/{servicename}", POST_SERVICES_Handler);
            _server.AddHandler("GET", "/icons/{*path}", GET_ICONS_Handler);
            _server.AddHandler("POST", "/icons/{*path}", POST_ICONS_Handler);
            _server.AddHandler("GET", "/list/themes/*", GET_LIST_THEMES_Handler);
            _server.AddHandler("GET", "/list/icons/*", GET_LIST_ICONS_Handler);
            _server.AddHandler("GET", "/themes/*", GET_THEMES_Handler);
            _server.AddHandler("GET", "/css/{*path}", GET_CSS_Handler);
            _server.AddHandler("GET", "/scripts/{*path}", GET_SCRIPTS_Handler);
            _server.AddHandler("POST", "/services/{servicename}/instances/{instanceid}/bookmarks/{bookmark}", POST_BOOKMARK_Handler);
            _server.AddHandler("GET", "/services/{servicename}/instances/{instanceid}/bookmarks/{bookmark}", POST_BOOKMARK_Handler);

            //
            // TWR: New website handlers - get processed in order
            //
            _server.AddHandler("GET", "/{website}/{path}/scripts/*", GetResourceHandler);
            _server.AddHandler("GET", "/{website}/{path}/content/*", GetResourceHandler);
            _server.AddHandler("GET", "/{website}/{path}/images/*", GetResourceHandler);
            _server.AddHandler("GET", "/{website}/{path}/views/*", GetResourceHandler);
            _server.AddHandler("GET", "/{website}/{*path}", GetResourceHandler);

            _server.AddHandler("POST", "/{website}/{path}/service/{name}/save", PostServiceSaveHandler);
            _server.AddHandler("POST", "/{website}/{path}/service/{name}/{action}", PostServiceHandler);

        }

        #endregion

        #region New website handlers

        #region GetResourceHandler

        void GetResourceHandler(HttpServer sender, ICommunicationContext ctx)
        {
            var uriString = ctx.Request.Uri.OriginalString;
            var website = ctx.Request.BoundVariables["website"];
            var path = ctx.Request.BoundVariables["path"];
            var extension = Path.GetExtension(uriString);

            if(string.IsNullOrEmpty(extension))
            {
                //
                // REST request e.g. http://localhost:1234/wwwroot/sources/server
                //
                const string ContentToken = "getParameterByName(\"content\")";

                var layoutFilePath = string.Format("{0}\\webs\\{1}\\layout.htm", Location, website);
                var contentPath = string.Format("\"/{0}/views/{1}.htm\"", website, path);

                ctx.Send(new DynamicFileCommunicationResponseWriter(layoutFilePath, ContentToken, contentPath));
                return;
            }

            // Should get url's with the following signatures
            //
            // http://localhost:1234/wwwroot/sources/Scripts/jquery-1.7.1.js
            // http://localhost:1234/wwwroot/sources/Content/Site.css
            // http://localhost:1234/wwwroot/sources/images/error.png
            // http://localhost:1234/wwwroot/sources/Views/Dialogs/SaveDialog.htm
            // http://localhost:1234/wwwroot/views/sources/server.htm
            //
            // We support only 1 level below the Views folder 
            // If path is a string without a backslash then we are processing the following request
            //       http://localhost:1234/wwwroot/views/sources/server.htm
            // If path is a string with a backslash then we are processing the following request
            //       http://localhost:1234/wwwroot/sources/Views/Dialogs/SaveDialog.htm
            //
            if(!string.IsNullOrEmpty(path) && path.IndexOf('/') == -1)
            {
                uriString = uriString.Replace(path, "");
            }
            var result = GetFileFromPath(new Uri(uriString));

            ctx.Send(result);
        }

        #endregion

        #region PostServiceSaveHandler

        void PostServiceSaveHandler(HttpServer sender, ICommunicationContext ctx)
        {
            InvokeService(ctx, "Save");
        }

        #endregion

        #region PostServiceHandler

        void PostServiceHandler(HttpServer sender, ICommunicationContext ctx)
        {
            InvokeService(ctx, ctx.Request.BoundVariables["action"]);
        }

        #endregion

        #region InvokeService

        void InvokeService(ICommunicationContext ctx, string methodName)
        {
            // Read post data which is expected to be JSON
            string args;
            using(var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
            {
                args = reader.ReadToEnd();
            }

            var className = ctx.Request.BoundVariables["name"];
            var dataListID = GetQueryStringValue(ctx, "dlid");
            var workspaceID = GetQueryStringValue(ctx, "wid");

            dynamic result;
            try
            {
                Guid workspaceGuid;
                Guid.TryParse(workspaceID, out workspaceGuid);

                Guid dataListGuid;
                Guid.TryParse(dataListID, out dataListGuid);

                //
                // NOTE: result.ToString() MUST return JSON
                //
                result = _serviceInvoker.Invoke(className, methodName, args, workspaceGuid, dataListGuid);
            }
            catch(Exception ex)
            {
                result = new ValidationResult
                {
                    ErrorMessage = ex.Message
                };
            }
            ctx.Send(new StringCommunicationResponseWriter(result.ToString(), "application/json"));
        }

        #endregion

        #endregion

        #region Request Handlers

        //TrevorCake
        private void GET_POST_CLIENT_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            string postdlid = string.Empty;
            string wid = string.Empty;

            if (ctx.Request.QueryString[GlobalConstants.DLID] != null)
            {
                postdlid = ctx.Request.QueryString[GlobalConstants.DLID].Value;
            }

            if(ctx.Request.QueryString["wid"] != null)
            {
                wid = ctx.Request.QueryString["wid"].Value;
            }

            if (postdlid != null)
            {
                ctx.Send(Post(ctx, ctx.Request.BoundVariables["servicename"], string.Empty, string.Empty, wid, postdlid));
            }
            else
            {
                ctx.Send(Get(ctx, ctx.Request.BoundVariables["servicename"], wid));    
            }
        }

        //private void GET_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        //{
        //    ctx.Send(Get(ctx, ctx.Request.BoundVariables["servicename"]));
        //}

        private void POST_CLIENT_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(Post(ctx, ctx.Request.BoundVariables["servicename"], ctx.Request.BoundVariables["clientid"]));
        }

        private void POST_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(Post(ctx, ctx.Request.BoundVariables["servicename"]));
        }

        private void GET_ICONS_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(GetFile(ctx, string.Format("{0}\\icons\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ctx.Request.BoundVariables["path"]), "image/png"));
        }

        private void POST_ICONS_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(GetFile(ctx, string.Format("{0}\\icons\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ctx.Request.BoundVariables["path"]), "image/png"));
        }

        private void GET_LIST_THEMES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(DirectoryList(ctx));
        }

        private void GET_LIST_ICONS_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(DirectoryListJSON(ctx));
        }

        private void GET_THEMES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(GetFileFromPath(ctx, "themes", ctx.Request.Uri));
        }

        private void GET_CSS_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(GetFile(ctx, string.Format("{0}\\css\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ctx.Request.BoundVariables["path"]), "text/css"));
        }

        private void GET_SCRIPTS_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(GetFile(ctx, string.Format("{0}\\scripts\\{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ctx.Request.BoundVariables["path"]), "text/javascript"));
        }

        private void POST_BOOKMARK_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(Post(ctx, ctx.Request.BoundVariables["servicename"], ctx.Request.BoundVariables["instanceid"], ctx.Request.BoundVariables["bookmark"]));
        }
        #endregion

        #region Server Handling
        private void StartNetworkServer()
        {
            List<string> enpoints = new List<string>
            {
                WebServerResources.LocalServerAddress,
                string.Format(WebServerResources.PublicServerAddressFormat, Environment.MachineName),
                _endpointAddress,
            };
            StartNetworkServer(enpoints);
        }

        private void StartNetworkServer(IList<string> endpointAddresses)
        {
            _dsf = _network.Channel;

            if (endpointAddresses == null || endpointAddresses.Count == 0)
            {
                throw new ArgumentException("No TCP Addresses configured for application server");
            }

            var entries = endpointAddresses.Where(s => !string.IsNullOrWhiteSpace(s)).Select(a =>
                {
                    int port;
                    IPHostEntry entry;

                    try
                    {
                        Uri uri = new Uri(a);
                        string dns = uri.DnsSafeHost;
                        port = uri.Port;
                        entry = Dns.GetHostEntry(dns);
                    }
                    catch
                    {
                        port = 0;
                        entry = null;
                    }

                    if (entry == null || entry.AddressList == null || entry.AddressList.Length == 0)
                    {
                        ServerLifecycleManager.WriteLine(string.Format("'{0}' is an invalid address, listening not started for this entry.", a));
                        return null;
                    }

                    return new Tuple<IPHostEntry, int>(entry, port);

                }).Where(e => e != null).ToList();

            
            if (!entries.Any())
            {
                throw new ArgumentException("No vailid TCP Addresses configured for application server");
            }

            List<IPAddress> startedIPAddresses = new List<IPAddress>();
            foreach(var entry in entries)
            {
                for (int i = 0; i < entry.Item1.AddressList.Length; i++)
                {
                    IPAddress current = entry.Item1.AddressList[i];

                    if (current.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !startedIPAddresses.Contains(current))
                    {
                        if (_network.Start(new System.Network.ListenerConfig(current.ToString(), entry.Item2, 10)))
                        {
                            ServerLifecycleManager.WriteLine(string.Format("{0} listening on {1}", _network, current + ":" + entry.Item2.ToString()));
                            startedIPAddresses.Add(current);
                        }
                    }
                }                
            }

            if (startedIPAddresses.Count == 0)
            {
                ServerLifecycleManager.WriteLine(string.Format("{0} failed to start on {1}", _network, _endpointAddress));
            }
        }

        private CommunicationResponseWriter CreateForm(dynamic d, string clientID)
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            // strip out &amp; and replace correctly
            //string correctedURI = d.XmlString.Replace("&lt;", "<").Replace("&gt;", ">");
            string correctedURI = d.XmlString.Replace("&", "").Replace(GlobalConstants.PostDataStart, "").Replace(GlobalConstants.PostDataEnd, "");
            string executePayload = null;

            if(clientID != null)
            {
                Guid clientGuid;

                if(Guid.TryParse(clientID, out clientGuid))
                {
                    executePayload = _dsf.ExecuteCommand(correctedURI, Workspaces.WorkspaceRepository.Instance.Get(clientGuid) ?? Workspaces.WorkspaceRepository.Instance.ServerWorkspace, GlobalConstants.NullDataListID);
                }
                else
                    executePayload = _dsf.ExecuteCommand(correctedURI, GlobalConstants.NullDataListID);
            }
            else
            {
                executePayload = _dsf.ExecuteCommand(correctedURI, GlobalConstants.NullDataListID);
            }


            // Travis.Frisinger - 404 hack
            if(executePayload.IndexOf("<InnerError>", StringComparison.Ordinal) > 0 && executePayload.IndexOf("' Not Found", StringComparison.Ordinal) > 0)
            {

                string docType = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">";
                
                return new StringCommunicationResponseWriter(string.Format("{0}\r\n{1}", docType, "<center><h1>404</h1>\r\n"+WebServerResources.ERROR_404_Message+"</center>"));
            }

            // TODO : Allow return type to be specified as a parameter... Default to XML
            string result = executePayload;

            if(executePayload.IndexOf("<Dev2System.FormView>", StringComparison.Ordinal) >= 0)
            {
                int start = (executePayload.IndexOf("<Dev2System.FormView>") + 21);
                int end = (executePayload.IndexOf("</Dev2System.FormView>"));
                int len = (end - start);
                if(len > 0)
                {
                    string tmp = executePayload.Substring(start, (end - start));
                    result = CleanupHtml(tmp);
                    string docType = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">";
                    return new StringCommunicationResponseWriter(string.Format("{0}\r\n{1}", docType, result));

                }
                else
                {
                    return new StringCommunicationResponseWriter(result, "text/xml");
                }
            }
            else if(executePayload.IndexOf("</JSON>") >= 0)
            {
                // we have a JSON payload, quite a silly way to do this...
                //json = json.Replace("\r\n", string.Empty).Replace(",])", "])");

                //return new StringCommunicationResponseWriter(json, "application/json");
                //json = json.Replace("\r\n", string.Empty).Replace(",])", "])");

                int start = result.IndexOf(GlobalConstants.OpenJSON);
                if(start >= 0)
                {
                    int end = result.IndexOf(GlobalConstants.CloseJSON);
                    start += GlobalConstants.OpenJSON.Length;

                    result = CleanupHtml(result.Substring(start, (end - start)));
                }

                return new StringCommunicationResponseWriter(result, "application/json");
            }
            else
            {
                return new StringCommunicationResponseWriter(result, "text/xml");
            }
        }
        #endregion

        #region GET Handling
        private CommunicationResponseWriter Get(ICommunicationContext ctx, string serviceName)
        {
            dynamic d = new UnlimitedObject();
            d.Service = serviceName;
            string url = ctx.Request.Uri.ToString();
            d.WebServerUrl = ctx.Request.Uri.ToString();
            d.Dev2WebServer = string.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);

            string data = GetPostData(ctx, d, Guid.Empty.ToString());

            if(!string.IsNullOrEmpty(data))
            {
                d.Add(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(data));
            }

            return CreateForm(d, null);

        }

        private CommunicationResponseWriter Get(ICommunicationContext ctx, string serviceName, string clientID)
        {
            dynamic d = new UnlimitedObject();
            d.Service = serviceName;
            string url = ctx.Request.Uri.ToString();
            d.WebServerUrl = ctx.Request.Uri.ToString();
            d.Dev2WebServer = string.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);

            string data = GetPostData(ctx, d, Guid.Empty.ToString());

            if(!string.IsNullOrEmpty(data))
            {
                d.Add(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(data));
            }

            return CreateForm(d, clientID);

        }
        #endregion

        #region POST Handling
        private CommunicationResponseWriter Post(ICommunicationContext ctx, string serviceName)
        {
            return Post(ctx, serviceName, string.Empty, string.Empty);
        }

        private CommunicationResponseWriter Post(ICommunicationContext ctx, string serviceName, string workspaceID)
        {
            return Post(ctx, serviceName, string.Empty, string.Empty, workspaceID, Guid.Empty.ToString());
        }

        private CommunicationResponseWriter Post(ICommunicationContext ctx, string serviceName, string instanceId, string bookmark)
        {
            return Post(ctx, serviceName, instanceId, bookmark, null, Guid.Empty.ToString());
        }

        private CommunicationResponseWriter Post(ICommunicationContext ctx, string serviceName, string instanceId, string bookmark, string workspaceID, string postDataListID)
        {
            UnlimitedObject formData = null;

            dynamic d = new UnlimitedObject();

            string xml = GetPostData(ctx, d, postDataListID);

            if(!string.IsNullOrEmpty(xml))
            {
                formData = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xml);
            }

            d.Service = serviceName;
            d.InstanceId = instanceId;
            d.Bookmark = bookmark;
            d.WebServerUrl = ctx.Request.Uri.ToString();
            d.Dev2WebServer = string.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);
            if(formData != null)
            {
                d.AddResponse(formData);
            }

            return CreateForm(d, workspaceID);
        }
        #endregion

        #region Directory Handling
        private CommunicationResponseWriter DirectoryList(ICommunicationContext ctx)
        {
            string directory = string.Format("{0}{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetDirectoryName(ctx.Request.Uri.LocalPath));

            directory = directory.Replace("\\list\\", "\\");

            if(Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).ToList();
                List<string> prunedFiles = new List<string>();
                files.ForEach(c => prunedFiles.Add(string.Format("{0}/{1}", Path.GetDirectoryName(c).Replace(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), string.Empty), Path.GetFileName(c)).Replace("\\", "/")));

                UnlimitedObject data = new UnlimitedObject("DirectoryList");

                prunedFiles.ForEach(c => data.CreateElement("File").SetValue(c));

                return new XmlCommunicationResponseWriter(data.XmlString);

            }
            else
            {
                return new NotFoundCommunicationResponseWriter();
            }
        }

        private CommunicationResponseWriter DirectoryListJSON(ICommunicationContext ctx)
        {
            string directory = string.Format("{0}{1}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetDirectoryName(ctx.Request.Uri.LocalPath));

            directory = directory.Replace("\\list\\", "\\");

            StringBuilder result = new StringBuilder("[{\"title\":\"icons\", \"isFolder\": true, \"key\":\"icons\", \"children\":[");

            if(Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);

                foreach(string f in files)
                {
                    result.Append("{ \"title\": ");
                    result.Append("\"");
                    int start = f.LastIndexOf("\\");
                    if(start > 0)
                    {
                        string toAdd = f.Substring((start + 1));
                        result.Append(toAdd);
                        result.Append("\"");
                        result.Append(", \"isFolder\" : false, \"key\":\"");
                        result.Append(toAdd);
                        result.Append("\"");
                    }
                    result.Append("},");
                }

                int len = result.Length;

                result = result.Remove((len - 1), 1); // trim trailing ,

                result.Append("]}]");

            }
            else
            {
                return new NotFoundCommunicationResponseWriter();
            }

            return new StringCommunicationResponseWriter(result.ToString());
            //return result.ToString();
        }
        #endregion

        #region File Handling

        CommunicationResponseWriter GetFileFromPath(ICommunicationContext ctx)
        {
            return GetFileFromPath(ctx.Request.Uri);
        }

        CommunicationResponseWriter GetFileFromPath(Uri uri)
        {
            var filePath = string.Format("{0}\\Webs{1}\\{2}",
                Location,
                Path.GetDirectoryName(uri.LocalPath),
                Path.GetFileName(uri.LocalPath));
            return GetFileFromPath(filePath);
        }

        CommunicationResponseWriter GetFileFromPath(ICommunicationContext ctx, string folder, Uri uri)
        {
            var filePath = string.Format("{0}{1}\\{2}",
                Location,
                Path.GetDirectoryName(uri.LocalPath),
                Path.GetFileName(uri.LocalPath));
            return GetFileFromPath(filePath);
        }

        private CommunicationResponseWriter GetFileFromPath(string filePath)
        {
            var supportedFileExtensions = ConfigurationManager.AppSettings["SupportedFileExtensions"];
            var extension = Path.GetExtension(filePath);
            var ext = string.IsNullOrEmpty(extension) ? "" : extension;
            var isSupportedExtensionList = supportedFileExtensions.Split(new[] { ',' })
                .ToList()
                .Where(supportedExtension => supportedExtension.Trim().Equals(ext, StringComparison.InvariantCultureIgnoreCase));

            if(string.IsNullOrEmpty(supportedFileExtensions) || !isSupportedExtensionList.Any())
            {
                return new NotFoundCommunicationResponseWriter();
            }

            if(File.Exists(filePath))
            {
                var contentType = "text/html";
                switch(ext.ToLower())
                {
                    case ".js":
                        contentType = "text/javascript";
                        break;

                    case ".css":
                        contentType = "text/css";
                        break;

                    case ".ico":
                        contentType = "image/x-icon";
                        break;

                    case ".bm":
                    case ".bmp":
                        contentType = "image/bmp";
                        break;

                    case ".gif":
                        contentType = "image/gif";
                        break;

                    case ".jpeg":
                    case ".jpg":
                        contentType = "image/jpg";
                        break;

                    case ".tiff":
                        contentType = "image/tiff";
                        break;

                    case ".png":
                        contentType = "image/png";
                        break;

                    case ".htm":
                    case ".html":
                        contentType = "text/html";
                        break;

                    default:
                        return new NotFoundCommunicationResponseWriter();
                }

                return new StaticFileCommunicationResponseWriter(filePath, contentType);
            }


            return new NotFoundCommunicationResponseWriter();
        }


        private CommunicationResponseWriter GetFile(ICommunicationContext ctx, string uri, string contentType)
        {
            if(File.Exists(uri))
            {
                return new StaticFileCommunicationResponseWriter(uri, contentType);
            }
            else
            {
                return new NotFoundCommunicationResponseWriter();
            }
        }
        #endregion
    }
}
