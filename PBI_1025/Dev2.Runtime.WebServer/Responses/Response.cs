using System;
using System.Collections.Generic;
using System.IO;

namespace Unlimited.Applications.WebServer.Responses
{
    public class CommunicationResponseWriter
    {
        public CommunicationResponseWriter()
        {
        }

        public virtual void Write(ICommunicationContext context)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
        }
    }
}