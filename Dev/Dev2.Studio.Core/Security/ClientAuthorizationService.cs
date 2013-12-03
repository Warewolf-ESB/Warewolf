using System;
using Dev2.Services.Security;

namespace Dev2.Security
{
    public class ClientAuthorizationService : AuthorizationServiceBase
    {
        public ClientAuthorizationService(ISecurityService securityService)
            : base(securityService)
        {
        }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return true;
            //return IsAuthorized(ClaimsPrincipal.Current, context, resource);
        }

        protected override void OnSecurityServiceChanged(object sender, EventArgs args)
        {
        }
    }
}
