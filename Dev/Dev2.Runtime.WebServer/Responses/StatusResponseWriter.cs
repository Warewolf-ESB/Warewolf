using System.Net;
using Dev2.Runtime.WebServer.Controllers;
using Unlimited.Applications.WebServer;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StatusResponseWriter : ResponseWriter
    {
        readonly HttpStatusCode _statusCode;

        public StatusResponseWriter(HttpStatusCode statusCode = HttpStatusCode.NoContent)
        {
            _statusCode = statusCode;
        }

        public override void Write(ICommunicationContext context)
        {
            context.Response.Status = _statusCode;
        }

        public override void Write(WebControllerContext context)
        {
            context.ResponseMessage.StatusCode = _statusCode;
        }        
    }
}