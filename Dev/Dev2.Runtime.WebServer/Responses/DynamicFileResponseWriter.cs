using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Dev2.Runtime.WebServer.Responses
{
    public class DynamicFileResponseWriter : IResponseWriter
    {
        static readonly MediaTypeHeaderValue ContentType = ContentTypes.Html;

        readonly string _layoutFile;
        readonly string _contentPathToken;
        readonly string _contentPath;

        public DynamicFileResponseWriter(string layoutFile, string contentPathToken, string contentPath)
        {
            VerifyArgument.IsNotNull("layoutFile", layoutFile);
            VerifyArgument.IsNotNull("contentPathToken", contentPathToken);
            VerifyArgument.IsNotNull("contentPath", contentPath);
            _layoutFile = layoutFile;
            _contentPathToken = contentPathToken;
            _contentPath = contentPath;
        }

        public void Write(ICommunicationContext context)
        {
            // BUG 8593: 2013.02.17 - TWR - removed try/catch as this is handled by caller
            var content = GetContent();
            var buffer = Encoding.UTF8.GetBytes(content);
            context.Response.ContentType = ContentType.ToString();
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.ContentLength = buffer.Length;
        }

        public void Write(WebServerContext context)
        {
            var content = GetContent();
            context.ResponseMessage.Content = new StringContent(content);
            context.ResponseMessage.Content.Headers.ContentType = ContentType;
        }

        string GetContent()
        {
            var layoutContent = ReadLayoutFile();
            var builder = new StringBuilder(layoutContent);
            var content = builder.Replace(_contentPathToken, _contentPath).ToString();
            return content;
        }

        protected virtual string ReadLayoutFile()
        {
            return File.ReadAllText(_layoutFile);
        }
    }
}