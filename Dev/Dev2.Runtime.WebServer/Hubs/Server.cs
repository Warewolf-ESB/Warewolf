/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
        // Singleton instance - lazy initialization is used to ensure that the creation is threadsafe
        private static readonly Lazy<Server> TheInstance =
            new Lazy<Server>(
                () =>
                    new Server(GlobalHost.ConnectionManager.GetHubContext<EsbHub>().Clients,
                        WorkspaceRepository.Instance));

        private readonly IHubConnectionContext _clients;
        private readonly IWorkspaceRepository _workspaceRepository;

        private Server(IHubConnectionContext clients, IWorkspaceRepository workspaceRepository)
        {
            VerifyArgument.IsNotNull("clients", clients);
            VerifyArgument.IsNotNull("workspaceRepository", workspaceRepository);
            _clients = clients;
            _workspaceRepository = workspaceRepository;
        }

        public static Server Instance
        {
            get { return TheInstance.Value; }
        }

        public Guid GetWorkspaceID(IIdentity identity)
        {
            return _workspaceRepository.GetWorkspaceID(identity as WindowsIdentity);
        }

        public void SendMemo(string memo, string connectionID = null)
        {
            if (string.IsNullOrEmpty(connectionID))
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
            if (string.IsNullOrEmpty(connectionID))
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
            if (string.IsNullOrEmpty(connectionID))
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
            if (string.IsNullOrEmpty(connectionID))
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