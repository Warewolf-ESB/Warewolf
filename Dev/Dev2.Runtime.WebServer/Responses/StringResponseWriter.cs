using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.UI.WebControls;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StringResponseWriter : IResponseWriter
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

        public void Write(WebServerContext context)
        {
            context.ResponseMessage.Content = new StringContent(_text);
            context.ResponseMessage.Content.Headers.ContentType = _contentType;

            UpdateContentDisposition(context.ResponseMessage);
        }

        void UpdateContentDisposition(HttpResponseMessage responseMessage)
        {
            var contentLength = Encoding.UTF8.GetByteCount(_text);
            if(contentLength > WebServerStartup.SizeCapForDownload)
            {
                string extension = null;
                if(ContentTypes.Json.Equals(_contentType))
                {
                    extension = "json";
                }
                else if(ContentTypes.Xml.Equals(_contentType))
                {
                    extension = "xml";
                }
                if(extension != null)
                {
                    var contentDisposition = new ContentDispositionHeaderValue("attachment");
                    contentDisposition.Parameters.Add(new NameValueHeaderValue("filename", "Output." + extension));

                    responseMessage.Content.Headers.ContentType = ContentTypes.ForceDownload;
                    responseMessage.Content.Headers.ContentDisposition = contentDisposition;
                    responseMessage.Headers.Add("Server", "Dev2 Server");
                }
            }
        }
    }
}