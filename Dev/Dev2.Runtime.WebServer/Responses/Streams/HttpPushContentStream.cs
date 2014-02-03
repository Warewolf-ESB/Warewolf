using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Dev2.Runtime.WebServer.Responses.Streams
{
    public abstract class HttpPushContentStream
    {
        public const int DefaultChunkSize = 65536;

        readonly HttpResponseMessage _response;
        readonly MediaTypeHeaderValue _contentType;
        readonly int _chunkSize;

        protected HttpPushContentStream(HttpResponseMessage response, MediaTypeHeaderValue contentType, int chunkSize = DefaultChunkSize)
        {
            VerifyArgument.IsNotNull("response", response);
            VerifyArgument.IsNotNull("mediaType", contentType);
            _response = response;
            _contentType = contentType;
            _chunkSize = chunkSize;
        }

        public void Write()
        {
            _response.Content = new PushStreamContent((Action<Stream, HttpContent, TransportContext>)WriteToStream, _contentType);
        }

        protected abstract Stream OpenInputStream();

        async void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
        {
            try
            {
                var buffer = new byte[_chunkSize];
                using(var inputStream = OpenInputStream())
                {
                    var length = (int)inputStream.Length;
                    var bytesRead = 1;

                    while(length > 0 && bytesRead > 0)
                    {
                        bytesRead = inputStream.Read(buffer, 0, Math.Min(length, buffer.Length));
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                        length -= bytesRead;
                    }
                }
            }
            catch(HttpException)
            {
            }
            finally
            {
                outputStream.Close();
                outputStream.Dispose();
            }
        }

    }
}