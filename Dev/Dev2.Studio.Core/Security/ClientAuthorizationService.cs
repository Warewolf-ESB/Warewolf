using System;
using System.Linq;
using System.Security.Claims;
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
