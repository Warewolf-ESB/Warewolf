using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime;
using Dev2.Runtime.Diagnostics;
using Dev2.Server.DataList.Translators;
using Dev2.Web;
using Unlimited.Applications.DynamicServicesHost;
using Unlimited.Applications.WebServer;
using Unlimited.Applications.WebServer.Responses;
using Unlimited.Framework;

namespace Dev2
{
    public sealed class WebServer : IFrameworkWebServer
    {

        #region Translator

        private DataListTranslatorFactory tdf = new DataListTranslatorFactory();
        private List<DataListFormat> publicFormats;

        #endregion

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

        #endregion

        #region Instance Fields
        private EsbServicesEndpoint _esbEndpoint;
        private string _endpointAddress;
        private HttpServer _server;
        private StudioNetworkServer _network;

        readonly ServiceInvoker _serviceInvoker = new ServiceInvoker();

        #endregion

        #region Constructors
        public WebServer(Dev2Endpoint[] endPoints, StudioNetworkServer server)
        {
            _server = new HttpServer(endPoints);
            _network = server;
            publicFormats = tdf.FetchAllFormats().Where(c => c.ContentType != "").ToList();
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
            _server.AddHandler("GET", "/services/{servicename}", GET_POST_CLIENT_SERVICES_Handler);
            _server.AddHandler("POST", "/services/{servicename}?wid={clientid}", POST_CLIENT_SERVICES_Handler);
            _server.AddHandler("POST", "/services/{servicename}", POST_SERVICES_Handler);
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


            if(uriString.IndexOf("wwwroot") < 0)
            {
                // http://127.0.0.1:1234/services/"/themes/system/js/json2.js"

                GET_POST_CLIENT_SERVICES_Handler(sender, ctx);
                return;
            }

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


        private void GET_POST_CLIENT_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            string postdlid = string.Empty;
            string wid = string.Empty;

            if(ctx.Request.QueryString[GlobalConstants.DLID] != null)
            {
                postdlid = ctx.Request.QueryString[GlobalConstants.DLID].Value;
            }

            if(ctx.Request.QueryString["wid"] != null)
            {
                wid = ctx.Request.QueryString["wid"].Value;
            }

            if(postdlid != null)
            {
                ctx.Send(Post(ctx, ctx.Request.BoundVariables["servicename"], string.Empty, string.Empty, wid, postdlid));
            }
            else
            {
                ctx.Send(Get(ctx, ctx.Request.BoundVariables["servicename"], wid));
            }
        }

        private void POST_CLIENT_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(Post(ctx, ctx.Request.BoundVariables["servicename"], ctx.Request.BoundVariables["clientid"]));
        }

        private void POST_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            ctx.Send(Post(ctx, ctx.Request.BoundVariables["servicename"]));
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
            _esbEndpoint = _network.Channel;

            if(endpointAddresses == null || endpointAddresses.Count == 0)
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
                    catch(Exception ex)
                    {
                        ServerLogger.LogError(ex);
                        port = 0;
                        entry = null;
                    }

                    if(entry == null || entry.AddressList == null || entry.AddressList.Length == 0)
                    {
                        ServerLifecycleManager.WriteLine(string.Format("'{0}' is an invalid address, listening not started for this entry.", a));
                        return null;
                    }

                    return new Tuple<IPHostEntry, int>(entry, port);

                }).Where(e => e != null).ToList();


            if(!entries.Any())
            {
                throw new ArgumentException("No vailid TCP Addresses configured for application server");
            }

            List<IPAddress> startedIPAddresses = new List<IPAddress>();
            foreach(var entry in entries)
            {
                for(int i = 0; i < entry.Item1.AddressList.Length; i++)
                {
                    IPAddress current = entry.Item1.AddressList[i];

                    if(current.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !startedIPAddresses.Contains(current))
                    {
                        if(_network.Start(new System.Network.ListenerConfig(current.ToString(), entry.Item2, 10)))
                        {
                            ServerLifecycleManager.WriteLine(string.Format("{0} listening on {1}", _network, current + ":" + entry.Item2.ToString()));
                            startedIPAddresses.Add(current);
                        }
                    }
                }
            }

            if(startedIPAddresses.Count == 0)
            {
                ServerLifecycleManager.WriteLine(string.Format("{0} failed to start on {1}", _network, _endpointAddress));
            }

        }


        #endregion

        #region GET Handling

        private CommunicationResponseWriter Get(ICommunicationContext ctx, string serviceName, string clientID)
        {
            dynamic d = new UnlimitedObject();
            d.Service = serviceName;

            d.WebServerUrl = ctx.Request.Uri.ToString();
            d.Dev2WebServer = string.Format("{0}://{1}", ctx.Request.Uri.Scheme, ctx.Request.Uri.Authority);

            string data = WebRequestHandler.GetPostData(ctx, Guid.Empty.ToString());

            if(!string.IsNullOrEmpty(data))
            {
                d.PostData = data;
                d.Add(new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(data));
            }

            return WebRequestHandler.CreateForm(d, serviceName, clientID, ctx.FetchHeaders(), publicFormats);

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

            string xml = WebRequestHandler.GetPostData(ctx, postDataListID);

            if(!string.IsNullOrEmpty(xml))
            {
                formData = new UnlimitedObject().GetStringXmlDataAsUnlimitedObject(xml);
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

            return WebRequestHandler.CreateForm(d, serviceName, workspaceID, ctx.FetchHeaders(), publicFormats);
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
