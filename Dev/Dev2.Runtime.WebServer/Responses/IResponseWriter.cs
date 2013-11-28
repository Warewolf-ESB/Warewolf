
namespace Dev2.Runtime.WebServer.Responses
{
    public interface IResponseWriter
    {
        void Write(WebServerContext context);
    }
}