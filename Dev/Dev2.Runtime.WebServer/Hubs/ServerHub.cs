using Microsoft.AspNet.SignalR;

namespace Dev2.Runtime.WebServer.Hubs
{
    // Instances of the Hub class are transient, you can't use them 
    // to maintain state from one method call to the next. Each time 
    // the server receives a method call from a client, a new instance
    // of your Hub class processes the message.
    public abstract class ServerHub : Hub
    {
        protected ServerHub()
            : this(Server.Instance)
        {
        }

        protected ServerHub(Server server)
        {
            Server = server;
        }

        protected Server Server { get; private set; }
    }
}