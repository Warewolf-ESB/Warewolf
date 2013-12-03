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
        }

        protected abstract void OnSecurityServiceChanged(object sender, EventArgs args);

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);

        protected bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            var permissions = GetPermissions(context);
            return _securityService.Permissions
                .Where(p => principal.IsInRole(p.WindowsGroup) && Matches(p, resource))
                .Any(groupPermission => (groupPermission.Permissions & permissions) != 0);
        }

        static bool Matches(WindowsGroupPermission permission, string resource)
        {
            if(permission.IsServer || string.IsNullOrEmpty(resource))
            {
                return true;
            }

            Guid resourceID;
            if(Guid.TryParse(resource, out resourceID))
            {
                return permission.ResourceID == resourceID;
            }

            // ResourceName is in the format: {categoryName}\{resourceName}
            return permission.ResourceName.EndsWith("\\" + resource);
        }

        public static Permissions GetPermissions(AuthorizationContext context)
        {
            switch(context)
            {
                case AuthorizationContext.View:
                    return Permissions.Administrator | Permissions.Contribute | Permissions.View;

                case AuthorizationContext.Execute:
                    return Permissions.Administrator | Permissions.Contribute | Permissions.Execute;

                case AuthorizationContext.Contribute:
                    return Permissions.Administrator | Permissions.Contribute;

                case AuthorizationContext.DeployTo:
                    return Permissions.Administrator | Permissions.DeployTo;

                case AuthorizationContext.DeployFrom:
                    return Permissions.Administrator | Permissions.DeployFrom;
            }
            return Permissions.None;
        }
    }
}