using System;
using System.Linq;
using System.Security.Principal;

namespace Dev2.Services.Security
{
    public abstract class AuthorizationServiceBase : IAuthorizationService
    {
        readonly ISecurityService _securityService;

        protected AuthorizationServiceBase(ISecurityService securityService)
        {
            VerifyArgument.IsNotNull("SecurityService", securityService);
            _securityService = securityService;
            _securityService.PermissionsChanged += (s, e) => RaisePermissionsChanged();
        }

        public event EventHandler PermissionsChanged;

        public Permissions GetResourcePermissions(Guid resourceID)
        {
            return Permissions.None;
        }

        public void Load()
        {
            _securityService.Read();
        }

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);

        protected virtual void RaisePermissionsChanged()
        {
            if(PermissionsChanged != null)
            {
                PermissionsChanged(this, EventArgs.Empty);
            }
        }

        protected bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            var contextPermissions = context.ToPermissions();
            return _securityService.Permissions
                .Where(p => principal.IsInRole(p.WindowsGroup) && p.Matches(resource))
                .Any(p => (p.Permissions & contextPermissions) != 0);
        }
    }
}