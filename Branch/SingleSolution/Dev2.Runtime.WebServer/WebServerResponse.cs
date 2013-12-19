using System.Net.Http;

namespace Dev2.Runtime.WebServer
{
    public class WebServerResponse : ICommunicationResponse
    {
        public WebServerResponse(HttpResponseMessage response)
        {
            VerifyArgument.IsNotNull("response", response);
            Response = response;
        }

        public HttpResponseMessage Response { get; private set; }
    }
}