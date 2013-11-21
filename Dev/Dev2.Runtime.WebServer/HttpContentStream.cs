using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using HttpFramework.Exceptions;

namespace Dev2.Runtime.WebServer
{
    public abstract class HttpContentStream
    {
        readonly int _chunkSize;

        protected HttpContentStream(int chunkSize = 65536)
        {
            _chunkSize = chunkSize;
        }

        public async void WriteToStream(Stream outputStream, HttpContent content, TransportContext context)
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
            catch(HttpException ex)
            {
                return;
            }
            finally
            {
                outputStream.Close();
            }
        }

        protected abstract Stream OpenInputStream();

        public PushStreamContent CreatePushStreamContent(string contentType)
        {
            var mediaType = MediaTypeHeaderValue.Parse(contentType);
            return CreatePushStreamContent(mediaType);
        }

        public PushStreamContent CreatePushStreamContent(MediaTypeHeaderValue mediaType)
        {
            return new PushStreamContent((Action<Stream, HttpContent, TransportContext>)WriteToStream, mediaType);
        }
    }
}