using System.Threading.Tasks;
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
        #region Overrides of Hub

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
        /// </returns>
        public override Task OnConnected()
        {
            return base.OnConnected();
            }

        #endregion


        public void SendMemo(Memo memo)
        {
            var serializedMemo = JsonConvert.SerializeObject(memo);
            Server.SendMemo(serializedMemo, Context.ConnectionId);
        }
    }
}