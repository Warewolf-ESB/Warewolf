/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Dev2.Studio.Interfaces;
using Warewolf.Data;


namespace Dev2.Security
{
    public class ClientAuthorizationService : AuthorizationServiceBase
    {
        readonly IEnvironmentConnection _environmentConnection;

        public ClientAuthorizationService(ISecurityService securityService, bool isLocalConnection)
            : base(securityService, isLocalConnection)
        {
            if (securityService is ClientSecurityService clientSecurityService)
            {
                _environmentConnection = clientSecurityService.EnvironmentConnection;
            }
        }

        public override Permissions GetResourcePermissions(Guid resourceId)
        {
            var principal = _environmentConnection == null ? ClaimsPrincipal.Current : _environmentConnection.Principal;
            var groupPermissions = GetGroupPermissions(principal, resourceId).ToList();
            var result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
        }

        protected override IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, Guid resourceId)
        {
            var serverPermissions = _securityService.Permissions;
            var serverOnlyPermissions = serverPermissions.Where(permission => permission.IsServer || permission.ResourceID==Guid.Empty);
            if (principal == null)
            {
                serverOnlyPermissions= serverOnlyPermissions.Where(permission => permission.IsBuiltInGuests);
            }

            if (resourceId == Guid.Empty)
            {
                return serverOnlyPermissions;
            }
            var resourcePermissions = serverPermissions.Where(p => p.Matches(resourceId) && !p.IsServer).ToList();
            if (resourcePermissions.Any())
            {
                if (principal == null)
                {
                    return resourcePermissions.Where(permission => permission.IsBuiltInGuestsForExecution);
                }
                return resourcePermissions;
            }
            return serverOnlyPermissions;
        }
        // protected override IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        // {
        //     var serverPermissions = _securityService.Permissions;
        //     var serverOnlyPermissions = serverPermissions.Where(permission => permission.IsServer || permission.ResourceID==Guid.Empty);
        //     if (principal == null)
        //     {
        //         serverOnlyPermissions= serverOnlyPermissions.Where(permission => permission.IsBuiltInGuests);
        //     }
        //     if (Guid.TryParse(resource, out Guid resourceId))
        //     {
        //         if (resourceId == Guid.Empty)
        //         {
        //             return serverOnlyPermissions;
        //         }
        //         var resourcePermissions = serverPermissions.Where(p => p.Matches(resource) && !p.IsServer).ToList();
        //         if (resourcePermissions.Any())
        //         {
        //             if (principal == null)
        //             {
        //                 return resourcePermissions.Where(permission => permission.IsBuiltInGuestsForExecution);
        //             }
        //             return resourcePermissions;
        //         }
        //     }
        //     return serverOnlyPermissions;
        // }

        // public override bool IsAuthorized(AuthorizationContext context, string resource)
        // {
        //     var x =IsAuthorized(_environmentConnection.Principal, context, resource);
        //     return x;
        // }
        public override bool IsAuthorized(AuthorizationContext context, Guid resourceId)
        {
            var x =IsAuthorized(_environmentConnection.Principal, context, resourceId);
            return x;
        }

        public override bool IsAuthorized(AuthorizationContext context, IWarewolfResource resource)
        {
            var x =IsAuthorized(_environmentConnection.Principal, context, resource);
            return x;
        }

        public override bool IsAuthorized(IAuthorizationRequest request) => false;

        protected override void OnDisposed()
        {
        }
    }
}
