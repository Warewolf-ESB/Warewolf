using Dev2.Runtime.WebServer.Controllers;

namespace Dev2.Runtime.WebServer.Responses
{
    public abstract class ResponseWriter
    {
        public const string HtmlContentType = "text/html; charset=utf-8";

        public abstract void Write(ICommunicationContext context);

        public abstract void Write(WebServerContext context);
    }
}