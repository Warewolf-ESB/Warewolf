using System;
using System.Collections.Specialized;
using System.Net.Http;
using Unlimited.Applications.WebServer;
using Unlimited.Applications.WebServer.Responses;

namespace Dev2.Runtime.WebServer
{
    public class WebCommunicationContext : ICommunicationContext
    {
        readonly HttpRequestMessage _request;

        public WebCommunicationContext(HttpRequestMessage request, NameValueCollection boundVariables)
        {
            _request = request;
            Request = new WebCommunicationRequest(request, boundVariables);
        }

        public ICommunicationRequest Request { get; private set; }
        public ICommunicationResponse Response { get { throw new NotImplementedException(); } }

        public void Send(CommunicationResponseWriter response)
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