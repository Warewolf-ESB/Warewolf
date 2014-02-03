using System.IO;
using System.Net.Http.Headers;
using Dev2.Runtime.WebServer.Responses.Streams;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StaticFileResponseWriter : IResponseWriter
    {
        readonly string _file;
        readonly MediaTypeHeaderValue _contentType;
        readonly int _chunkSize;

        public StaticFileResponseWriter(string file, string contentType, int chunkSize = 1024)
        {
            VerifyArgument.IsNotNull("file", file);
            VerifyArgument.IsNotNull("contentType", contentType);
            _file = file;
            _contentType = MediaTypeHeaderValue.Parse(contentType);
            _chunkSize = chunkSize;
        }

        public void Write(WebServerContext context)
        {
            var stream = new HttpFileStream(OpenFileStream, context.ResponseMessage, _contentType, _chunkSize);
            stream.Write();

        }

        protected virtual Stream OpenFileStream()
        {
            return File.Open(_file, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}