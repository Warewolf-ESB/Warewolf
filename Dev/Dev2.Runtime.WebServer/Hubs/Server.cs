using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Communication;
using Dev2.Workspaces;
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
        readonly static Lazy<Server> TheInstance = new Lazy<Server>(() => new Server(GlobalHost.ConnectionManager.GetHubContext<ResourcesHub>().Clients, WorkspaceRepository.Instance));

        readonly IHubConnectionContext _clients;
        readonly IWorkspaceRepository _workspaceRepository;

        Server(IHubConnectionContext clients, IWorkspaceRepository workspaceRepository)
        {
            VerifyArgument.IsNotNull("clients", clients);
            VerifyArgument.IsNotNull("workspaceRepository", workspaceRepository);
            _clients = clients;
            _workspaceRepository = workspaceRepository;
        }

        public Guid GetWorkspaceID(IIdentity identity)
        {
            return _workspaceRepository.GetWorkspaceID(identity as WindowsIdentity);
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