namespace Warewolf.Interfaces.Auditing
{
    public interface IWebSocketServerFactory
    {
        IWebSocketServerWrapper New(string serverLoggingAddress);
    }

    public class WebSocketServerFactory : IWebSocketServerFactory
    {
        public IWebSocketServerWrapper New(string serverLoggingAddress)
        {
            return new WebSocketServerWrapper(serverLoggingAddress);
        }
    }
}
