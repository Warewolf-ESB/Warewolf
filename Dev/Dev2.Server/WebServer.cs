using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime;
using Dev2.Runtime.Diagnostics;
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
        public ServiceInvoker ServiceInvoker { get { return _serviceInvoker; } }

        #endregion

        #region Static Members

        #region GetQueryStringValue

        #endregion

        #endregion

        #region Instance Fields
        private EsbServicesEndpoint _esbEndpoint;
        private string _endpointAddress;
        private HttpServer _server;
        private StudioNetworkServer _network;

        readonly ServiceInvoker _serviceInvoker = new ServiceInvoker();
        readonly WebRequestHandler _webRequestHandler = new WebRequestHandler();

        #endregion

        #region Constructors
        public WebServer(Dev2Endpoint[] endPoints, StudioNetworkServer server)
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
            ctx.Request.BoundVariables["action"] = "Save";
            _webRequestHandler.InvokeService(ctx);
        }

        #endregion

        #region PostServiceHandler

        void PostServiceHandler(HttpServer sender, ICommunicationContext ctx)
        {
            _webRequestHandler.InvokeService(ctx);
        }

        #endregion

        #region InvokeService

        #endregion

        #endregion

        #region Request Handlers


        private void GET_POST_CLIENT_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            _webRequestHandler.Get(ctx);
        }

        private void POST_CLIENT_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            _webRequestHandler.Post(ctx);
        }

        private void POST_SERVICES_Handler(HttpServer sender, ICommunicationContext ctx)
        {
            _webRequestHandler.Post(ctx);
        }

        private void POST_BOOKMARK_Handler(HttpServer sender, ICommunicationContext ctx)
        {
           _webRequestHandler.Post(ctx);
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
