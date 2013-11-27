using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Dev2.Runtime.WebServer.Responses.Streams
{
    public class HttpFileStream : HttpPushContentStream
    {
        readonly Func<Stream> _openInputStream;    

        public HttpFileStream(Func<Stream> openInputStream, HttpResponseMessage response, MediaTypeHeaderValue contentType, int chunkSize = DefaultChunkSize)
            : base(response, contentType, chunkSize)
        {
            VerifyArgument.IsNotNull("openInputStream", openInputStream);
            _openInputStream = openInputStream;
        }

        protected override Stream OpenInputStream()
        {
            return _openInputStream();
        }
    }
}
