using Dev2.Communication;
using Dev2.Runtime.WebServer.Security;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace Dev2.Runtime.WebServer.Hubs
{
    [AuthorizeHub]
    [HubName("resources")]
    public class ResourcesHub : ServerHub
    {
        public void SendMemo(Memo memo)
        {
            var serializedMemo = JsonConvert.SerializeObject(memo);
            Server.SendMemo(serializedMemo, Context.ConnectionId);
        }
    }
}