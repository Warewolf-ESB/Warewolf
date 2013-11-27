using System;
using Dev2.Communication;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Hubs
{
    public class Server
    {
        public static Server Instance
        {
            get
            {
                return TheInstance.Value;
            }
        }

        // Singleton instance - lazy initialization is used to ensure that the creation is threadsafe
        readonly static Lazy<Server> TheInstance = new Lazy<Server>(() => new Server(GlobalHost.ConnectionManager.GetHubContext<ResourcesHub>().Clients));

        readonly IHubConnectionContext _clients;

        Server(IHubConnectionContext clients)
        {
            _clients = clients;
        }

        public void SendMemo(Memo memo, string connectionID = null)
        {
            if(string.IsNullOrEmpty(connectionID))
            {
                _clients.All.SendMemo(memo);
            }
            else
            {
                _clients.Client(connectionID).SendMemo(memo);
            }
        }

    }
}