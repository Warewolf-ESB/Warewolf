using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;

namespace Unlimited.Applications.WebServer.Responses
{
    public class BufferedStringResponse : CommunicationResponseWriter
    {
        private string _contentType;
        private string _data;
        private int _chunkSize;

        public BufferedStringResponse(string data, string mimeType, int chunkSize = 10240)
        {
            _data = data;
            _contentType = mimeType;
            _chunkSize = chunkSize;
        }

        public override void Write(ICommunicationContext context)
        {
            base.Write(context);
            context.Response.ContentType = _contentType;
            Stream outputStream = context.Response.OutputStream;

            try
            {
                long contentLength = 0;

                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(_data)))
                {
                    byte[] buffer = new byte[_chunkSize];
                    long length = contentLength = stream.Length;
                    long start;

                    int remainder = (int)(length - ((length / _chunkSize) * _chunkSize));

                    for (start = 0L; start < length; start += _chunkSize)
                    {
                        int amount = stream.Read(buffer, 0, _chunkSize);
                        outputStream.Write(buffer, 0, amount);
                    }

                    if (remainder != 0)
                    {
                        int amount = stream.Read(buffer, 0, remainder);
                        outputStream.Write(buffer, 0, amount);
                    }
                }

                context.Response.ContentLength = contentLength;
            }
            catch (Exception ex)
            {
                ServerLogger.LogError(ex);
                context.Response.Status = (System.Net.HttpStatusCode)404;
            }
        }
    }
}