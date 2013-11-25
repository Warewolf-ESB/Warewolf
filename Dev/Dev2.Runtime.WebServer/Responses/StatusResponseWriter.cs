using System.Net;

namespace Dev2.Runtime.WebServer.Responses
{
    public class StatusResponseWriter : IResponseWriter
    {
        readonly HttpStatusCode _statusCode;

        public StatusResponseWriter(HttpStatusCode statusCode = HttpStatusCode.NoContent)
        {
            _statusCode = statusCode;
        }

        public void Write(ICommunicationContext context)
        {
            context.Response.Status = _statusCode;
        }

        public void Write(WebServerContext context)
        {
            context.ResponseMessage.StatusCode = _statusCode;
        }
    }
}