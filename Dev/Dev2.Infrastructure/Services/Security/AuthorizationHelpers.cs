#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using Dev2.Common.Interfaces.Attribute;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Security;

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
            return permission.ResourceName.Contains("\\" + resource);
        }
    }
}
