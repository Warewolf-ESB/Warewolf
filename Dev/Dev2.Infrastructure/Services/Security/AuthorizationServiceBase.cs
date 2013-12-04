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
            _securityService.Changed += OnSecurityServiceChanged;
            _securityService.Read();
        }

        protected abstract void OnSecurityServiceChanged(object sender, EventArgs args);

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);

        protected bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            var contextPermissions = context.ToPermissions();
            return _securityService.Permissions
                .Where(p => principal.IsInRole(p.WindowsGroup) && p.Matches(resource))
                .Any(p => (p.Permissions & contextPermissions) != 0);
        }
    }
}