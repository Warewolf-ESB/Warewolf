using System.IO;
using System.Text;
using Dev2.Runtime.WebServer.Controllers;

namespace Dev2.Runtime.WebServer.Responses
{
    public class DynamicFileResponseWriter : ResponseWriter
    {
        const string ContentType = "text/html; charset=utf-8";

        readonly string _layoutFile;
        readonly string _contentPathToken;
        readonly string _contentPath;

        public DynamicFileResponseWriter(string layoutFile, string contentPathToken, string contentPath, int chunkSize = 1024)
        {
            _layoutFile = layoutFile;
            _contentPathToken = contentPathToken;
            _contentPath = contentPath;
        }

        public override void Write(ICommunicationContext context)
        {
            // BUG 8593: 2013.02.17 - TWR - removed try/catch as this is handled by caller
            var content = GetContent();
            var buffer = Encoding.UTF8.GetBytes(content);
            context.Response.ContentType = ContentType;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.ContentLength = buffer.Length;
        }

        public override void Write(WebServerContext context)
        {
            var content = GetContent();
            var stream = new HttpTextStream(content);
            context.ResponseMessage.Content = stream.CreatePushStreamContent(ContentType);
        }

        string GetContent()
        {
            var layoutContent = File.ReadAllText(_layoutFile);
            var builder = new StringBuilder(layoutContent);
            var content = builder.Replace(_contentPathToken, _contentPath).ToString();
            return content;
        }
    }
}