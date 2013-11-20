using System.Collections.Specialized;
using Unlimited.Applications.WebServer;
using Unlimited.Applications.WebServer.Responses;

namespace Dev2.Runtime.WebServer
{
    public class WebCommunicationContext : ICommunicationContext
    {

        #region Implementation of ICommunicationContext

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public WebCommunicationContext(WebCommunicationRequest request)
        {
            Request = request;
            //Response = response;
        }

        public ICommunicationRequest Request { get; private set; }
        public ICommunicationResponse Response { get; private set; }

        public void Send(CommunicationResponseWriter response)
        {
        }

        public NameValueCollection FetchHeaders()
        {
            return null;
        }

        #endregion
    }
}