using System;
using System.IO;
using System.Net.Http.Headers;
using Dev2.Runtime.WebServer.Responses.Streams;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StaticFileResponseWriter : ResponseWriter
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

        public override void Write(ICommunicationContext context)
        {
            context.Response.ContentType = _contentType.ToString();
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
            var stream = new HttpFileStream(_file, context.ResponseMessage, _contentType, _chunkSize);
            stream.Write();
        }
    }
}