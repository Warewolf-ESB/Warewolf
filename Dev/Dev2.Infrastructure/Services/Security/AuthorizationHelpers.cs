
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

            return attribute == null || String.IsNullOrEmpty(attribute.Reason) ? null : attribute.Reason;
        }

        public static bool IsContributor(this Permissions permissions)
        {
            return permissions.HasFlag(Permissions.Contribute) || permissions.HasFlag(Permissions.Administrator);

        }

        public static bool CanDebug(this Permissions permissions)
        {
            return permissions.IsContributor() ||
                   (permissions.HasFlag(Permissions.View) && permissions.HasFlag(Permissions.Execute));
        }

        public static Permissions ToPermissions(this AuthorizationContext context)
        {
            switch(context)
            {
                case AuthorizationContext.Administrator:
                    return Permissions.Administrator;

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

                case AuthorizationContext.Any:
                    return Permissions.Administrator | Permissions.View | Permissions.Contribute | Permissions.Execute | Permissions.DeployFrom | Permissions.DeployTo;
            }
            return Permissions.None;
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
            if(resource != null)
            {
                resource = resource.Replace('/', '\\');
            }
            if(string.IsNullOrEmpty(resource))
            {
                return true;
            }
            return permission.ResourceName.Contains("\\" + resource);
        }
    }
}
