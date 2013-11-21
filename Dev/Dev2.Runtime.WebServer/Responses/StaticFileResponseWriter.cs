using System;
using System.IO;
using Dev2.Runtime.WebServer.Controllers;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StaticFileResponseWriter : ResponseWriter
    {
        readonly string _contentType;
        readonly string _file;
        readonly int _chunkSize;

        public StaticFileResponseWriter(string file, string mimeType, int chunkSize = 1024)
        {
            _file = file;
            _contentType = mimeType;
            _chunkSize = chunkSize;
        }

        public override void Write(ICommunicationContext context)
        {
            context.Response.ContentType = _contentType;
            Stream outputStream = context.Response.OutputStream;

            try
            {
                long contentLength = 0;

                using(Stream stream = File.Open(_file, FileMode.Open, FileAccess.Read, FileShare.Inheritable))
                {
                    byte[] buffer = new byte[_chunkSize];
                    long length = contentLength = stream.Length;
                    long start;

                    int remainder = (int)(length - ((length / _chunkSize) * _chunkSize));

                    for(start = 0L; start < length; start += _chunkSize)
                    {
                        int amount = stream.Read(buffer, 0, _chunkSize);
                        outputStream.Write(buffer, 0, amount);
                    }

                    if(remainder != 0)
                    {
                        int amount = stream.Read(buffer, 0, remainder);
                        outputStream.Write(buffer, 0, amount);
                    }
                }

                context.Response.ContentLength = contentLength;
            }
            catch(Exception)
            {
                context.Response.Status = (System.Net.HttpStatusCode)404;
            }
        }

        public override void Write(WebServerContext context)
        {
            var stream = new HttpFileStream(_file, _chunkSize);
            context.ResponseMessage.Content = stream.CreatePushStreamContent(_contentType);
        }
    }
}