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