/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Security;

namespace Dev2.Services.Security
{
    public static class AuthorizationHelpers
    {
        public static string ToReason(this AuthorizationContext value, bool isAuthorized = false)
        {
            //
            // MUST return null and NOT empty string as the result is used as TargetNullValue in bindings!
            //
            if(isAuthorized)
            {
                return null;
            }

            var field = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(field, typeof(ReasonAttribute)) as ReasonAttribute;

            return String.IsNullOrEmpty(attribute?.Reason) ? null : attribute.Reason;
        }

        public static bool IsContributor(this Permissions permissions)
        {
            return permissions.HasFlag(Permissions.Contribute) || permissions.HasFlag(Permissions.Administrator);

        }


        public static bool CanDebug(this Permissions permissions)
        {
            return permissions.IsContributor() ||
                   permissions.HasFlag(Permissions.View) && permissions.HasFlag(Permissions.Execute);
        }

        public static Permissions ToPermissions(this AuthorizationContext context)
        {
            if (context == AuthorizationContext.None)
            {
                return Permissions.None;
            }
            Permissions permission = Permissions.Administrator;

            if (context.HasFlag(AuthorizationContext.DeployTo))
            {
                permission |= Permissions.DeployTo;
            }
            if (context.HasFlag(AuthorizationContext.Contribute))
            {
                permission |= Permissions.Contribute;
            }
            if (context.HasFlag(AuthorizationContext.DeployFrom))
            {
                permission |= Permissions.DeployFrom;
            }
            if (context.HasFlag(AuthorizationContext.Execute))
            {
                permission |= Permissions.Execute | Permissions.Contribute;
            }
            if (context.HasFlag(AuthorizationContext.View))
            {
                permission |= Permissions.View | Permissions.Contribute;;
            }
            if (context.HasFlag(AuthorizationContext.Any))
            {
                permission = Permissions.Administrator | Permissions.View | Permissions.Contribute | Permissions.Execute | Permissions.DeployFrom | Permissions.DeployTo;
            }

            return permission;
        }

        public static bool Matches(this WindowsGroupPermission permission, string resource)
        {
            if(permission.IsServer)
            {
                return true;
            }

            Guid resourceId;
            if(Guid.TryParse(resource, out resourceId))
            {
                return permission.ResourceID == resourceId;
            }

            // ResourceName is in the format: {categoryName}\{resourceName}
            resource = resource?.Replace('/', '\\');
            if(string.IsNullOrEmpty(resource))
            {
                return true;
            }
            return permission.ResourceName.Contains("\\" + resource);
        }
    }
}
