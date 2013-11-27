using System.Collections.Specialized;
using System.Net.Http;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Runtime.WebServer
{
    public class WebServerContext : ICommunicationContext
    {
        readonly HttpRequestMessage _request;

        public WebServerContext(HttpRequestMessage request, NameValueCollection requestPaths)
        {
            _request = request;
            ResponseMessage = request.CreateResponse();
            Request = new WebServerRequest(request, requestPaths);
            Response = new WebServerResponse(ResponseMessage);
        }

        public HttpResponseMessage ResponseMessage { get; private set; }

        public ICommunicationRequest Request { get; private set; }
        public ICommunicationResponse Response { get; private set; }

        public void Send(IResponseWriter response)
        {
            VerifyArgument.IsNotNull("response", response);
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