using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StringResponseWriter : ResponseWriter
    {
        readonly string _text;
        readonly MediaTypeHeaderValue _contentType;

        public StringResponseWriter(string text, string contentType)
            : this(text, MediaTypeHeaderValue.Parse(contentType))
        {
        }

        public StringResponseWriter(string text, MediaTypeHeaderValue contentType)
        {
            VerifyArgument.IsNotNull("mediaType", contentType);
            _text = text;
            _contentType = contentType;
        }

        public override void Write(ICommunicationContext context)
        {
            var buffer = Encoding.UTF8.GetBytes(_text);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.ContentType = _contentType.ToString();
            context.Response.ContentLength = buffer.Length;
        }

        public override void Write(WebServerContext context)
        {
            context.ResponseMessage.Content = new StringContent(_text);
            context.ResponseMessage.Content.Headers.ContentType = _contentType;
            UpdateContentDisposition(context.ResponseMessage);
        }

        void UpdateContentDisposition(HttpResponseMessage response)
        {
            var contentLength = Encoding.UTF8.GetByteCount(_text);
            if(contentLength > WebServerStartup.SizeCapForDownload)
            {
                string extension = null;
                var contentType = response.Content.Headers.ContentType;
                if(ContentTypes.Json.Equals(contentType))
                {
                    extension = "json";
                }
                else if(ContentTypes.Xml.Equals(contentType))
                {
                    extension = "xml";
                }
                if(extension != null)
                {
                    response.Content.Headers.ContentType = ContentTypes.ForceDownload;
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.Parameters.Add(new NameValueHeaderValue("filename", "Output." + extension));
                    response.Headers.Add("Server", "Dev2 Server");
                }
            }
        }
    }
}