
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Services.Security;

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

        List<IWindowsGroupPermission> GetPermissions(IPrincipal user);
    }
}
