using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.WebServer.Responses
{
    public class XmlCommunicationResponseWriter : BinaryCommunicationResponseWriter
    {
        public XmlCommunicationResponseWriter(string message)
            : base(Encoding.UTF8.GetBytes(message))
        {
        }

        public override void Write(ICommunicationContext context)
        {
            base.Write(context);
            context.Response.ContentType = "text/xml; charset=utf-8";
        }
    }
}
