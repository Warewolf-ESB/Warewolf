using System;
using System.Security.Principal;
using Dev2.Common.Interfaces.Security;

namespace Dev2.Services.Security
{
    public interface IAuthorizationService
    {
        event EventHandler PermissionsChanged;
        event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;
        ISecurityService SecurityService { get; }

        bool IsAuthorized(AuthorizationContext context, string resource);
        bool IsAuthorized(IPrincipal user, AuthorizationContext context, string resource);
        bool IsAuthorized(IAuthorizationRequest request);
        Permissions GetResourcePermissions(Guid resourceId);

        void Remove(Guid resourceId);

        string JsonPermissions();
    }
}