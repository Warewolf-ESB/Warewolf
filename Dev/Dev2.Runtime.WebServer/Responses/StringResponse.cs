using System.Text;
using System.Collections.Generic;

namespace Unlimited.Applications.WebServer.Responses
{
    public class StringCommunicationResponseWriter : BinaryCommunicationResponseWriter
    {
        private string _contentType;

        public StringCommunicationResponseWriter(string message)
            : this(message, "text/html; charset=utf-8")
        {
        }

        public StringCommunicationResponseWriter(string message, string mimeType)
            : base(Encoding.UTF8.GetBytes(message))
        {
            _contentType = mimeType;
        }

        public override void Write(ICommunicationContext context)
        {
            base.Write(context);
            context.Response.ContentType = _contentType;
        }
    }
}