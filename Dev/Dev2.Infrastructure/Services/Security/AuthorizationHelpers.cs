#pragma warning disable
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
using System.Web;
using Dev2.Common;
using Dev2.Common.Interfaces.Attribute;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;
using Warewolf.Data;

namespace Dev2.Services.Security
{
    public static class AuthorizationHelpers
    {
        public static string ToReason(this AuthorizationContext value) => value.ToReason(false);

        public static string ToReason(this AuthorizationContext value, bool isAuthorized)
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

        public static bool IsContributor(this Permissions permissions) => permissions.HasFlag(Permissions.Contribute) || permissions.HasFlag(Permissions.Administrator);

        public static bool CanDebug(this Permissions permissions) => permissions.IsContributor() ||
                   permissions.HasFlag(Permissions.View) && permissions.HasFlag(Permissions.Execute);

        public static Permissions ToPermissions(this AuthorizationContext context)
        {
            if (context == AuthorizationContext.None)
            {
                return Permissions.None;
            }
            var permission = Permissions.Administrator;

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
                permission |= Permissions.View | Permissions.Contribute;
            }
            if (context.HasFlag(AuthorizationContext.Any))
            {
                permission = Permissions.Administrator | Permissions.View | Permissions.Contribute | Permissions.Execute | Permissions.DeployFrom | Permissions.DeployTo;
            }

            return permission;
        }

        public static bool Matches(this WindowsGroupPermission permission, Guid resourceId)
        {
            if(permission.IsServer)
            {
                return true;
            }

            return permission.ResourceID == resourceId;
        }
        public static bool Matches(this WindowsGroupPermission permission, string resource)
        {
            if(permission.IsServer)
            {
                return true;
            }

            if (Guid.TryParse(resource, out Guid resourceId))
            {
                return permission.ResourceID == resourceId;
            }
            
            resource = resource?.Replace('/', '\\');
            if(string.IsNullOrEmpty(resource))
            {
                return true;
            }

            var p1 = resource;
            var p2 = "\\" + permission.ResourcePath;
            if (p2 is null)
            {
                return false;
            }
            var pathMatches = p1?.StartsWith(p2) ?? false;
            return pathMatches;
        }

        public static bool Matches(this WindowsGroupPermission permission, IWarewolfResource resource)
        {
            if(permission.IsServer)
            {
                return true;
            }

            var matchingResourceID = permission.ResourceID == resource.ResourceID;
            if (matchingResourceID)
            {
                return true;
            }

            var resourcePath = resource.FilePath?.Replace('/', '\\');
            if(string.IsNullOrEmpty(resourcePath))
            {
                return true;
            }

            var p1 = resourcePath;
            if (p1.StartsWith(EnvironmentVariables.ResourcePath))
            {
                p1 = p1.Replace(EnvironmentVariables.ResourcePath, "");
            }
            var p2 = "\\" + permission.ResourcePath;
            if (p2 is null)
            {
                return false;
            }
            var pathMatches = p1?.StartsWith(p2) ?? false;
            return pathMatches;
        }

        public static bool Matches(this WindowsGroupPermission permission, IAuthorizationRequest request)
        {
            if(permission.IsServer)
            {
                return true;
            }

            var resourcePath = HttpUtility.UrlDecode(request.ResourcePath?.Replace('/', '\\') ?? "");
            if(string.IsNullOrEmpty(resourcePath))
            {
                return true;
            }

            var p1 = resourcePath;
            if (p1.StartsWith(EnvironmentVariables.ResourcePath))
            {
                p1 = p1.Replace(EnvironmentVariables.ResourcePath, "");
            }
            var p2 = "\\" + permission.ResourcePath;
            if (p2 is null)
            {
                return false;
            }
            var pathMatches = p1?.StartsWith(p2, StringComparison.InvariantCultureIgnoreCase) ?? false;
            return pathMatches;
        }
    }
}
