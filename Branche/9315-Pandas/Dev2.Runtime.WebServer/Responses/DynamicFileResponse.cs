using System.IO;
using System.Text;

namespace Unlimited.Applications.WebServer.Responses
{
    public class DynamicFileCommunicationResponseWriter : CommunicationResponseWriter
    {
        readonly string _layoutFile;
        readonly string _contentPathToken;
        readonly string _contentPath;

        public DynamicFileCommunicationResponseWriter(string layoutFile, string contentPathToken, string contentPath, int chunkSize = 1024)
        {
            _layoutFile = layoutFile;
            _contentPathToken = contentPathToken;
            _contentPath = contentPath;
        }

        public override void Write(ICommunicationContext context)
        {
            base.Write(context);

            // BUG 8593: 2013.02.17 - TWR - removed try/catch as this is handled by caller
            var layoutContent = File.ReadAllText(_layoutFile);
            var builder = new StringBuilder(layoutContent);
            var content = builder.Replace(_contentPathToken, _contentPath).ToString();
            var buffer = Encoding.UTF8.GetBytes(content);
            context.Response.ContentType = "text/html";
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.ContentLength = buffer.Length;
        }
    }
}