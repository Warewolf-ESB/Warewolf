
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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Security
{
    public class ClientAuthorizationService : AuthorizationServiceBase
    {
        readonly IEnvironmentConnection _environmentConnection;

        public ClientAuthorizationService(ISecurityService securityService, bool isLocalConnection)
            : base(securityService, isLocalConnection)
        {
            var clientSecurityService = securityService as ClientSecurityService;
            if(clientSecurityService != null)
            {
                _environmentConnection = clientSecurityService.EnvironmentConnection;
            }
        }

        public override Permissions GetResourcePermissions(Guid resourceId)
        {
            var principal = _environmentConnection == null ? ClaimsPrincipal.Current : _environmentConnection.Principal;
            var groupPermissions = GetGroupPermissions(principal, resourceId.ToString()).ToList();
            var result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
        }


        protected override IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            var serverPermissions = _securityService.Permissions;
            var serverOnlyPermissions = serverPermissions.Where(permission => permission.IsServer || permission.ResourceID==Guid.Empty);
            
            Guid resourceId;
            if (Guid.TryParse(resource, out resourceId))
            {
                if (resourceId == Guid.Empty)
                {
                    return serverOnlyPermissions;
                }
                var resourcePermissions = serverPermissions.Where(p => p.Matches(resource) && !p.IsServer).ToList();
                if (resourcePermissions.Any())
                {
                    return resourcePermissions;
                }
            }
            return serverOnlyPermissions;
        }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(_environmentConnection.Principal, context, resource);
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
        {
            return false;
        }

        protected override void OnDisposed()
        {
        }
    }
}
