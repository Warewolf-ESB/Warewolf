/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
        readonly string _text;
        readonly MediaTypeHeaderValue _contentType;
        readonly bool _enforceSizeCap;

        public StringResponseWriter(string text, string contentType)
            : this(text, MediaTypeHeaderValue.Parse(contentType))
        {
        }

        public StringResponseWriter(string text, MediaTypeHeaderValue contentType)
        {
            VerifyArgument.IsNotNull("mediaType", contentType);
            _text = text;
            _contentType = contentType;
            _enforceSizeCap = true;
        }
        
        public StringResponseWriter(string text, MediaTypeHeaderValue contentType,bool enforceSizeCap):this(text,contentType)
        {
            _enforceSizeCap = enforceSizeCap;
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
            if(contentLength > WebServerStartup.SizeCapForDownload && _enforceSizeCap)
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
