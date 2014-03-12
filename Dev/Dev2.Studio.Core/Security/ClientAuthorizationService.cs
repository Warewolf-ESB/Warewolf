using System.Security.Claims;
using Dev2.Services.Security;

namespace Dev2.Security
{
    public class ClientAuthorizationService : AuthorizationServiceBase
    {
        public ClientAuthorizationService(ISecurityService securityService, bool isLocalConnection)
            : base(securityService, isLocalConnection)
        {
        }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(ClaimsPrincipal.Current, context, resource);
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
