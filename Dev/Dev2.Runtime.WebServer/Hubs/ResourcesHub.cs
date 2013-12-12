using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.WebServer.Security;
using Dev2.Workspaces;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace Dev2.Runtime.WebServer.Hubs
{
    [AuthorizeHub]
    [HubName("resources")]
    public class ResourcesHub : ServerHub
    {
        public async Task<string> Save(string resourceXml, Guid workspaceID, Guid dataListID)
        {
            var currentUser = Context.User;
            if(currentUser != null)
            {
            }
            var task = Task.Factory.StartNew(() =>
            {
                var esb = new SaveResource();

                var theWorkspace = new Workspace(workspaceID);
                var values = new Dictionary<string, string>
                {
                    { "ResourceXml", resourceXml }, 
                    { "WorkspaceID", workspaceID.ToString() }, 
                    { "Roles", "All" }
                };

                return esb.Execute(values, theWorkspace);
            });
            return await task;
        }

        public void SendMemo(Memo memo)
        {
            var serializedMemo = JsonConvert.SerializeObject(memo);
            Server.SendMemo(serializedMemo, Context.ConnectionId);
        }
    }
}