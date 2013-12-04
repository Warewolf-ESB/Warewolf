using System;
using System.Security.Principal;
using Dev2.Services.Security;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestAuthorizationServiceBase : AuthorizationServiceBase
    {
        public TestAuthorizationServiceBase(ISecurityService securityService)
            : base(securityService)
        {
        }

        public int OnSecurityServiceChangedHitCount { get; private set; }
        protected override void OnSecurityServiceChanged(object sender, EventArgs args)
        {
            OnSecurityServiceChangedHitCount++;
        }

        public IPrincipal User { get; set; }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(User, context, resource);
        }
    }
}