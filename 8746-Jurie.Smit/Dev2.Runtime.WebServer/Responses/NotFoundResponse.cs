using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Unlimited.Applications.WebServer.Responses
{
    public class NotFoundCommunicationResponseWriter : StatusCommunicationResponseWriter
    {
        public NotFoundCommunicationResponseWriter()
            : base(404)
        {
        }
    }
}
