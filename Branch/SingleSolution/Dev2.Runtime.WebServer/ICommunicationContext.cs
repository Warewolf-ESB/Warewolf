using System.Collections.Specialized;
using Dev2.Runtime.WebServer.Responses;

namespace Dev2.Runtime.WebServer
{
    public interface ICommunicationContext
    {
        ICommunicationRequest Request { get; }
        ICommunicationResponse Response { get; }

        void Send(IResponseWriter response);

        NameValueCollection FetchHeaders();
    }
}
