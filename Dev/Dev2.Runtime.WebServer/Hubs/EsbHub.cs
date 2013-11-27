using Dev2.Communication;
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Hubs
{
    [AuthorizeHub]
    [HubName("esb")]
    public class EsbHub : ServerHub
    {
        public EsbHub()
        {
        }

        public EsbHub(Server server)
            : base(server)
        {
        }

        public void SendMemo(Memo memo)
        {
            Server.SendMemo(memo, Context.ConnectionId);
        }
    }
}