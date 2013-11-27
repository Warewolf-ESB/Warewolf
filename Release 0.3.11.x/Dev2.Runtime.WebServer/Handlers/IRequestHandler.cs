namespace Dev2.Runtime.WebServer.Handlers
{
    public interface IRequestHandler
    {
        void ProcessRequest(ICommunicationContext ctx);
    }
}