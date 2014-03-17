using System.Security.Principal;
using Dev2.Services.Security;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestAuthorizationServiceBase : AuthorizationServiceBase
    {
        public TestAuthorizationServiceBase(ISecurityService securityService, bool isLocalConnection = true)
            : base(securityService, isLocalConnection)
        {
        }

        public int RaisePermissionsChangedHitCount { get; private set; }
        public int RaisePermissionsModifiedHitCount { get; private set; }

        public IPrincipal User { get; set; }

        protected override void RaisePermissionsChanged()
        {
            RaisePermissionsChangedHitCount++;
            base.RaisePermissionsChanged();
        }

        protected override void OnPermissionsModified(PermissionsModifiedEventArgs e)
        {
            RaisePermissionsModifiedHitCount++;
            base.OnPermissionsModified(e);
        }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(User, context, resource);
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
        {
            return IsAuthorized(request.User, AuthorizationContext.Any, request.QueryString["rid"]);
        }

        public bool TestIsAuthorizedToConnect(IPrincipal principal)
        {
            return IsAuthorizedToConnect(principal);
        }

        protected override void OnDisposed()
        {
        }
    }
}