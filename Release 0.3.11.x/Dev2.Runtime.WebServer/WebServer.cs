using Dev2.Runtime.WebServer.Handlers;

namespace Dev2.Runtime.WebServer
{
    public sealed class WebServer : IFrameworkWebServer
    {
        readonly Dev2Endpoint[] _endPoints;
        IRequestHandler _webGetRequestHandler;
        IRequestHandler _webPostRequestHandler;
        IRequestHandler _websiteResourceHandler;
        IRequestHandler _websiteServiceHandler;
        HttpServer _server;

        public WebServer(Dev2Endpoint[] endPoints)
        {
            _endPoints = endPoints;
        }

        public void Start()
        {
            _webGetRequestHandler = new WebGetRequestHandler();
            _webPostRequestHandler = new WebPostRequestHandler();
            _websiteResourceHandler = new WebsiteResourceHandler();
            _websiteServiceHandler = new WebsiteServiceHandler();

            _server = new HttpServer(_endPoints);
            MapContextToDirectoryStructure();
        }

        public void Stop()
        {
            _server.Dispose();
            _server = null;
            _webGetRequestHandler = null;
            _webPostRequestHandler = null;
            _websiteResourceHandler = null;
            _websiteServiceHandler = null;
        }

        void MapContextToDirectoryStructure()
        {
            _server.AddHandler("GET", "/services/{servicename}", _webGetRequestHandler.ProcessRequest);
            _server.AddHandler("POST", "/services/{servicename}?wid={clientid}", _webPostRequestHandler.ProcessRequest);
            _server.AddHandler("POST", "/services/{servicename}", _webPostRequestHandler.ProcessRequest);
            _server.AddHandler("POST", "/services/{servicename}/instances/{instanceid}/bookmarks/{bookmark}", _webPostRequestHandler.ProcessRequest);
            _server.AddHandler("GET", "/services/{servicename}/instances/{instanceid}/bookmarks/{bookmark}", _webPostRequestHandler.ProcessRequest);

            //
            // TWR: New website handlers - get processed in order
            //
            _server.AddHandler("GET", "/{website}/{path}/scripts/*", _websiteResourceHandler.ProcessRequest);
            _server.AddHandler("GET", "/{website}/{path}/content/*", _websiteResourceHandler.ProcessRequest);
            _server.AddHandler("GET", "/{website}/{path}/images/*", _websiteResourceHandler.ProcessRequest);
            _server.AddHandler("GET", "/{website}/{path}/views/*", _websiteResourceHandler.ProcessRequest);
            _server.AddHandler("GET", "/{website}/{*path}", _websiteResourceHandler.ProcessRequest);

            _server.AddHandler("POST", "/{website}/{path}/service/{name}/{action}", _websiteServiceHandler.ProcessRequest);
        }
    }
}
