/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StringResponseWriter : IResponseWriter
    {
        private readonly MediaTypeHeaderValue _contentType;
        private readonly string _text;

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

        private void UpdateContentDisposition(HttpResponseMessage responseMessage)
        {
            int contentLength = Encoding.UTF8.GetByteCount(_text);
            if (contentLength > WebServerStartup.SizeCapForDownload)
            {
                string extension = null;
                if (ContentTypes.Json.Equals(_contentType))
                {
                    extension = "json";
                }
                else if (ContentTypes.Xml.Equals(_contentType))
                {
                    extension = "xml";
                }
                if (extension != null)
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