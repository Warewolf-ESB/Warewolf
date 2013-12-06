
using System;

namespace Dev2.Services.Security
{
    public interface IAuthorizationService
    {
        event EventHandler PermissionsChanged;

        bool IsAuthorized(AuthorizationContext context, string resource);

        Permissions GetResourcePermissions(Guid resourceID);

        void Load();
    }
}