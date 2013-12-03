using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Dev2.Services.Security
{
    public class AuthorizationServiceBase
    {
        readonly IEnumerable<WindowsGroupPermission> _groupPermissions;

        public AuthorizationServiceBase(IEnumerable<WindowsGroupPermission> groupPermissions)
        {
            _groupPermissions = groupPermissions;
        }

        public bool IsAuthorized(AuthorizationContext context, string resource)
        {
            var permissions = GetPermissions(context);
            return _groupPermissions
                .Where(gp => ClaimsPrincipal.Current.IsInRole(gp.WindowsGroup))
                .Any(groupPermission => (groupPermission.Permissions & permissions) != 0);
        }

        public static Permissions GetPermissions(AuthorizationContext context)
        {
            switch(context)
            {
                case AuthorizationContext.Save:
                    return Permissions.Administrator | Permissions.Contribute;
                case AuthorizationContext.Debug:
                    return Permissions.Administrator | Permissions.Contribute | Permissions.Execute;
                case AuthorizationContext.ViewInBrowser:
                    return Permissions.Administrator | Permissions.Contribute | Permissions.Execute;
            }
            return Permissions.None;
        }
    }
}