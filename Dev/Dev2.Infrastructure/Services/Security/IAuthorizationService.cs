using System;
using System.Security.Principal;

namespace Dev2.Services.Security
{
    public interface IAuthorizationService
    {
        event EventHandler PermissionsChanged;
        event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;
        bool IsAuthorized(AuthorizationContext context, string resource);
        bool IsAuthorized(IPrincipal user, AuthorizationContext context, string resource);
        bool IsAuthorized(IAuthorizationRequest request);
        Permissions GetResourcePermissions(Guid resourceID);

        void Remove(Guid resourceID);
    }
}