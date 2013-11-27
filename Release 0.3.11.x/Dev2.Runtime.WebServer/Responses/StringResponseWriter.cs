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

        public void Write(ICommunicationContext context)
        {
            var buffer = Encoding.UTF8.GetBytes(_text);
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.ContentType = _contentType.ToString();
            context.Response.ContentLength = buffer.Length;

            UpdateContentDisposition((contentType, contentDisposition, headers) =>
            {
                context.Response.ContentType = contentType.ToString();
                context.Response.AddHeader(headers.Key, headers.Value);
                context.Response.AddHeader("content-disposition", contentDisposition.ToString());
            });
        }

        public void Write(WebServerContext context)
        {
            context.ResponseMessage.Content = new StringContent(_text);
            context.ResponseMessage.Content.Headers.ContentType = _contentType;

            UpdateContentDisposition((contentType, contentDisposition, headers) =>
            {
                context.ResponseMessage.Content.Headers.ContentType = contentType;
                context.ResponseMessage.Content.Headers.ContentDisposition = contentDisposition;
                context.ResponseMessage.Headers.Add(headers.Key, headers.Value);
            });
        }

        void UpdateContentDisposition(Action<MediaTypeHeaderValue, ContentDispositionHeaderValue, KeyValuePair<string, string>> updateContentDisposition)
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
                    var headers = new KeyValuePair<string, string>("Server", "Dev2 Server");
                    var contentDisposition = new ContentDispositionHeaderValue("attachment");
                    contentDisposition.Parameters.Add(new NameValueHeaderValue("filename", "Output." + extension));

                    updateContentDisposition(ContentTypes.ForceDownload, contentDisposition, headers);
                }
            }
        }
    }
}