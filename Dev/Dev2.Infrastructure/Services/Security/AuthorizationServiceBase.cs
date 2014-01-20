using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Dev2.Services.Security
{
    public abstract class AuthorizationServiceBase : DisposableObject, IAuthorizationService
    {
        readonly ISecurityService _securityService;

        protected AuthorizationServiceBase(ISecurityService securityService)
        {
            VerifyArgument.IsNotNull("SecurityService", securityService);
            _securityService = securityService;
            _securityService.PermissionsChanged += (s, e) => RaisePermissionsChanged();
            _securityService.PermissionsModified += (s, e) => OnPermissionsModified(e);
        }

        public event EventHandler PermissionsChanged;
        public event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;

        public Permissions GetResourcePermissions(Guid resourceID)
        {
            var groupPermissions = GetGroupPermissions(ClaimsPrincipal.Current, resourceID.ToString());
            var result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
        }

        public virtual void Remove(Guid resourceID)
        {
            _securityService.Remove(resourceID);
        }

        public abstract bool IsAuthorized(AuthorizationContext context, string resource);
        public abstract bool IsAuthorized(IAuthorizationRequest request);

        protected virtual void RaisePermissionsChanged()
        {
            if(PermissionsChanged != null)
            {
                PermissionsChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnPermissionsModified(PermissionsModifiedEventArgs e)
        {
            var handler = PermissionsModified;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected bool IsAuthorizedToConnect(IPrincipal principal)
        {
            return IsAuthorized(AuthorizationContext.Any, () => GetGroupPermissions(principal));
        }

        public bool IsAuthorized(IPrincipal principal, AuthorizationContext context, string resource)
        {
            return IsAuthorized(context, () => GetGroupPermissions(principal, resource));
        }

        static bool IsAuthorized(AuthorizationContext context, Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions)
        {
            var contextPermissions = context.ToPermissions();
            var groupPermissions = getGroupPermissions();
            return groupPermissions.Any(p => (p.Permissions & contextPermissions) != 0);
        }

        IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            return _securityService.Permissions.Where(p => principal.IsInRole(p.WindowsGroup) && p.Matches(resource));
        }

        IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal)
        {
            return _securityService.Permissions.Where(p => principal.IsInRole(p.WindowsGroup));
        }
    }
}