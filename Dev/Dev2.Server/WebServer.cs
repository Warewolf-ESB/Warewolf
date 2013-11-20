using Unlimited.Applications.WebServer;

namespace Dev2
{
    public sealed class WebServer : IFrameworkWebServer
    {
        readonly Dev2Endpoint[] _endPoints;
        WebRequestHandler _webRequestHandler;
        HttpServer _server;

        public WebServer(Dev2Endpoint[] endPoints)
        {
            _endPoints = endPoints;
        }

        public void Start()
        {
            _webRequestHandler = new WebRequestHandler();
            _server = new HttpServer(_endPoints);
            MapContextToDirectoryStructure();
        }

        public void Stop()
        {
            _server.Dispose();
            _server = null;
            _webRequestHandler = null;
        }

        void MapContextToDirectoryStructure()
        {
            _server.AddHandler("GET", "/services/{servicename}", _webRequestHandler.Get);
            _server.AddHandler("POST", "/services/{servicename}?wid={clientid}", _webRequestHandler.Post);
            _server.AddHandler("POST", "/services/{servicename}", _webRequestHandler.Post);
            _server.AddHandler("POST", "/services/{servicename}/instances/{instanceid}/bookmarks/{bookmark}", _webRequestHandler.Post);
            _server.AddHandler("GET", "/services/{servicename}/instances/{instanceid}/bookmarks/{bookmark}", _webRequestHandler.Post);

            //
            // TWR: New website handlers - get processed in order
            //
            _server.AddHandler("GET", "/{website}/{path}/scripts/*", _webRequestHandler.GetWebResource);
            _server.AddHandler("GET", "/{website}/{path}/content/*", _webRequestHandler.GetWebResource);
            _server.AddHandler("GET", "/{website}/{path}/images/*", _webRequestHandler.GetWebResource);
            _server.AddHandler("GET", "/{website}/{path}/views/*", _webRequestHandler.GetWebResource);
            _server.AddHandler("GET", "/{website}/{*path}", _webRequestHandler.GetWebResource);

            _server.AddHandler("POST", "/{website}/{path}/service/{name}/{action}", _webRequestHandler.InvokeService);
        }
    }
}
