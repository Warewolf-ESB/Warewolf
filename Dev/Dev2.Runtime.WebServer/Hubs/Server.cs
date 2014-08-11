using System;
using System.Security.Principal;
using Dev2.Workspaces;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Hubs
{
    // ReSharper disable InconsistentNaming
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
        readonly static Lazy<Server> TheInstance = new Lazy<Server>(() => new Server(GlobalHost.ConnectionManager.GetHubContext<EsbHub>().Clients, WorkspaceRepository.Instance));

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

        public void SendMemo(string memo, string connectionID = null)
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

        public void SendDebugState(string debugState, string connectionID = null)
        {
            if(string.IsNullOrEmpty(connectionID))
            {
                _clients.All.SendDebugState(debugState);
            }
            else
            {
                _clients.Client(connectionID).SendDebugState(debugState);
            }
        }

        public void SendWorkspaceID(Guid workspaceID, string connectionID = null)
        {
            if(string.IsNullOrEmpty(connectionID))
            {
                _clients.All.SendWorkspaceID(workspaceID);
            }
            else
            {
                _clients.Client(connectionID).SendWorkspaceID(workspaceID);
            }
        }

        public void SendServerID(Guid serverID, string connectionID = null)
        {
            if(string.IsNullOrEmpty(connectionID))
            {
                _clients.All.SendServerID(serverID);
            }
            else
            {
                _clients.Client(connectionID).SendServerID(serverID);
            }
        }
    }
}