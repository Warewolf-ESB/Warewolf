using System;
using System.IO;

namespace Unlimited.Applications.WebServer.Responses
{
    public class BinaryCommunicationResponseWriter : CommunicationResponseWriter
    {
        private readonly byte[] _binary;

        public BinaryCommunicationResponseWriter(byte[] binary)
        {
            _binary = binary;
        }

        public override void Write(ICommunicationContext context)
        {
            base.Write(context);
            context.Response.ContentLength = _binary.Length;
            context.Response.OutputStream.Write(_binary, 0, _binary.Length);
        }
    }
}