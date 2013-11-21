using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Unlimited.Applications.WebServer;

namespace Dev2.Runtime.WebServer.Controllers
{
    public class WebControllerResponse : ICommunicationResponse
    {
        public WebControllerResponse(HttpResponseMessage response)
        {
            Response = response;
        }

        public HttpResponseMessage Response { get; private set; }

        #region Private Implementation of ICommunicationResponse

        HttpStatusCode ICommunicationResponse.Status { get { return (HttpStatusCode)0; } set { } }
        string ICommunicationResponse.Reason { get { return null; } set { } }
        string ICommunicationResponse.ContentType { get { return null; } set { } }
        long ICommunicationResponse.ContentLength { get { return 0; } set { } }
        Encoding ICommunicationResponse.Encoding { get { return null; } set { } }
        Stream ICommunicationResponse.OutputStream { get { return null; } }

        void ICommunicationResponse.AddHeader(string name, string value)
        {
        }

        #endregion
    }
}