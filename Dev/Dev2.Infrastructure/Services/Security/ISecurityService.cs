/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Services.Security
{
    public interface ISecurityService : IDisposable
    {
        event EventHandler PermissionsChanged;

        IReadOnlyList<WindowsGroupPermission> Permissions { get; }
        TimeSpan TimeOutPeriod { get; set; }

        void Read();
        event EventHandler<PermissionsModifiedEventArgs> PermissionsModified;

        void Remove(Guid resourceId);
        bool IsAuthorized(AuthorizationContext context, IPrincipal principal, Func<IEnumerable<WindowsGroupPermission>> getGroupPermissions);
        List<WindowsGroupPermission> GetDefaultPermissions(IPrincipal principal);
        List<WindowsGroupPermission> GetResourcePermissions(IPrincipal principal, string resource);
        List<WindowsGroupPermission> GetAdditionalPermissions(IPrincipal principal, string resource);
    }
}
