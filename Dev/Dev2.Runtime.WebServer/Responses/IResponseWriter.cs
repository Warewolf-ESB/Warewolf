
namespace Dev2.Runtime.WebServer.Responses
{
    public interface IResponseWriter
    {
        void Write(ICommunicationContext context);

        void Write(WebServerContext context);
    }
}