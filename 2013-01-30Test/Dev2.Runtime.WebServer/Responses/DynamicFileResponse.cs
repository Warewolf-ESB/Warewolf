using System;
using System.IO;
using System.Text;

namespace Unlimited.Applications.WebServer.Responses
{
    public class DynamicFileCommunicationResponseWriter : CommunicationResponseWriter
    {
        readonly string _layoutFile;
        readonly string _contentPathToken;
        readonly string _contentPath;
        readonly int _chunkSize;

        public DynamicFileCommunicationResponseWriter(string layoutFile, string contentPathToken, string contentPath, int chunkSize = 1024)
        {
            _layoutFile = layoutFile;
            _contentPathToken = contentPathToken;
            _contentPath = contentPath;
            _chunkSize = chunkSize;
        }

        public override void Write(ICommunicationContext context)
        {
            base.Write(context);
            var layoutContent = File.ReadAllText(_layoutFile);
            var builder = new StringBuilder(layoutContent);
            var content = builder.Replace(_contentPathToken, _contentPath).ToString();

            var buffer = Encoding.UTF8.GetBytes(content);

            try
            {
                context.Response.ContentType = "text/html";
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.ContentLength = buffer.Length;
            }
            catch(Exception)
            {
                context.Response.Status = (System.Net.HttpStatusCode)404;
            }
        }
    }
}