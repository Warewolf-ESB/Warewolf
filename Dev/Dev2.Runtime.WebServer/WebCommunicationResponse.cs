using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Unlimited.Applications.WebServer;

namespace Dev2.Runtime.WebServer
{
    public class WebCommunicationResponse : ICommunicationResponse
    {
        public WebCommunicationResponse(HttpResponseMessage response)
        {
            Response = response;
        }

        public HttpResponseMessage Response { get; private set; }


        public HttpStatusCode Status { get; set; }
        public string Reason { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
        public Encoding Encoding { get; set; }
        public Stream OutputStream { get; private set; }

        public void AddHeader(string name, string value)
        {
        }
    }
}