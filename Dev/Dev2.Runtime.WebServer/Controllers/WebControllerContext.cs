using System.Collections.Specialized;
using System.Net.Http;
using Dev2.Runtime.WebServer.Responses;
using Unlimited.Applications.WebServer;

namespace Dev2.Runtime.WebServer.Controllers
{
    public class WebControllerContext : ICommunicationContext
    {
        readonly HttpRequestMessage _request;

        public WebControllerContext(HttpRequestMessage request, NameValueCollection requestPaths)
        {
            _request = request;
            ResponseMessage = request.CreateResponse();
            Request = new WebControllerRequest(request, requestPaths);
            Response = new WebControllerResponse(ResponseMessage);
        }

        public HttpResponseMessage ResponseMessage { get; private set; }

        public ICommunicationRequest Request { get; private set; }
        public ICommunicationResponse Response { get; private set; }

        public void Send(ResponseWriter response)
        {
            response.Write(this);
        }

        public NameValueCollection FetchHeaders()
        {
            var result = new NameValueCollection();
            foreach(var header in _request.Headers)
            {
                result.Add(header.Key, string.Join("; ", header.Value));
            }
            return result;
        }
    }
}