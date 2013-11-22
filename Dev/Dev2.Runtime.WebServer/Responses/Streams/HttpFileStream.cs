using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Dev2.Runtime.WebServer.Responses.Streams
{
    public class HttpFileStream : HttpPushContentStream
    {
        readonly string _fileName;

        public HttpFileStream(string fileName, HttpResponseMessage response, MediaTypeHeaderValue contentType, int chunkSize = DefaultChunkSize)
            : base(response, contentType, chunkSize)
        {
            VerifyArgument.IsNotNull("fileName", fileName);
            _fileName = fileName;
        }

        protected override Stream OpenInputStream()
        {
            return File.Open(_fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
