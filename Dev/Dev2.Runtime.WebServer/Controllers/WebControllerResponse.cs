using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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


        public HttpStatusCode Status { get { return Response.StatusCode; } set { Response.StatusCode = value; } }
        public string Reason { get { return Response.ReasonPhrase; } set { Response.ReasonPhrase = value; } }
        public string ContentType { get { return Response.Content.Headers.ContentType.ToString(); } set { Response.Content.Headers.ContentType = new MediaTypeHeaderValue(value); } }
        public long ContentLength { get { return Response.Content.Headers.ContentLength.HasValue ? Response.Content.Headers.ContentLength.Value : 0L; } set { Response.Content.Headers.ContentLength = value; } }
        public Encoding Encoding
        {
            get
            {
                return Response.Content.GetContentEncoding();
            }
            set
            {
                Response.Content.Headers.ContentEncoding.Clear();
                Response.Content.Headers.ContentEncoding.Add(value.WebName);
            }
        }

        public Stream OutputStream
        {
            get
            {
                Stream stream = new MemoryStream();
                Response.Content = new StreamContent(stream);
                return stream;
            }
        }

        public void AddHeader(string name, string value)
        {
            if(String.Equals(name, "content-type", StringComparison.OrdinalIgnoreCase))
            {
                ContentType = value;
            }
            else if(String.Equals(name, "content-length", StringComparison.OrdinalIgnoreCase))
            {
                ContentLength = Int64.Parse(value);
            }
            else
            {
                Response.Headers.Add(name, value);
            }
        }
    }
}