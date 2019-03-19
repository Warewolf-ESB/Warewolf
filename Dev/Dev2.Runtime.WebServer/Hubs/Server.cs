#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Principal;
using Dev2.Workspaces;

namespace Dev2.Runtime.WebServer.Hubs
{
    
    public class Server
    {
        public static Server Instance => TheInstance.Value;

        // Singleton instance - lazy initialization is used to ensure that the creation is threadsafe
        readonly static Lazy<Server> TheInstance = new Lazy<Server>(() => new Server(WorkspaceRepository.Instance));

       readonly IWorkspaceRepository _workspaceRepository;

        Server(IWorkspaceRepository workspaceRepository)
        {
            VerifyArgument.IsNotNull("workspaceRepository", workspaceRepository);
            _workspaceRepository = workspaceRepository;
        }

        public Guid GetWorkspaceID(IIdentity identity) => _workspaceRepository.GetWorkspaceID(identity as WindowsIdentity);
    }
}
