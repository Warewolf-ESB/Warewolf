/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.IO;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Stateless helper that answers permission questions against a loaded
    /// <see cref="SecureConfigData"/> instance.
    ///
    /// Permission resolution rules (mirrors the Warewolf server's
    /// <c>ServerAuthorizationService</c> logic, simplified for the lightweight engine):
    ///
    /// <list type="bullet">
    ///   <item>
    ///     A workflow is <b>publicly visible</b> when:
    ///     <list type="bullet">
    ///       <item>No <c>secure.config</c> is loaded (open-access mode).</item>
    ///       <item>The built-in <c>Public</c> group has a global View permission
    ///             (<c>IsGlobal == true</c>).</item>
    ///       <item>The built-in <c>Public</c> group has a resource-specific View
    ///             permission whose <c>ResourceName</c> matches the workflow name.</item>
    ///     </list>
    ///   </item>
    ///   <item>
    ///     A workflow is <b>accessible to a JWT user</b> when any group in
    ///     <paramref name="userGroups"/> has a global View permission, or has a
    ///     resource-specific View permission matching the workflow name.
    ///   </item>
    /// </list>
    /// </summary>
    internal static class PermissionChecker
    {
        /// <summary>
        /// Returns <c>true</c> when the workflow identified by <paramref name="workflowName"/>
        /// is visible on the public (<c>/Public/</c>) endpoint.
        ///
        /// When no <c>secure.config</c> is loaded (<see cref="SecureConfigData.IsLoaded"/> ==
        /// <c>false</c>), all workflows are considered publicly visible.
        /// </summary>
        internal static bool HasPublicViewPermission(string workflowName, SecureConfigData config)
        {
            if (!config.IsLoaded)
                return true;

            foreach (var perm in config.Permissions)
            {
                if (!perm.IsPublicGroup || !perm.View)
                    continue;

                if (perm.IsGlobal)
                    return true;

                if (NamesMatch(perm.ResourceName, workflowName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> when any group in <paramref name="userGroups"/> grants
        /// View access to <paramref name="workflowName"/>.
        ///
        /// Administrators (groups with a global View permission) always see everything.
        /// When the Public group has global View, any authenticated user is considered
        /// to have view access (open-access mode with a valid token).
        /// </summary>
        internal static bool HasUserViewPermission(
            string                  workflowName,
            SecureConfigData        config,
            IReadOnlyList<string>   userGroups)
        {
            if (!config.IsLoaded)
                return true;

            // If Public has global View, any authenticated user may view any workflow.
            if (HasPublicViewPermission(workflowName, config))
                return true;

            foreach (var perm in config.Permissions)
            {
                if (!perm.View)
                    continue;

                if (!ContainsGroup(userGroups, perm.GroupName))
                    continue;

                if (perm.IsGlobal)
                    return true;

                if (NamesMatch(perm.ResourceName, workflowName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> when any group in <paramref name="userGroups"/> grants
        /// discovery access (View <b>or</b> Execute) to <paramref name="workflowName"/>.
        ///
        /// Discovery differs from View: a workflow an authenticated user can <em>execute</em>
        /// should also be visible to them in listings, even if the Public group has no
        /// View permission for it.
        /// </summary>
        internal static bool HasUserDiscoveryPermission(
            string                  workflowName,
            SecureConfigData        config,
            IReadOnlyList<string>   userGroups)
        {
            if (!config.IsLoaded)
                return true;

            // If Public has global View, any authenticated user may discover any workflow.
            if (HasPublicViewPermission(workflowName, config))
                return true;

            foreach (var perm in config.Permissions)
            {
                // Discovery requires at least View or Execute.
                if (!perm.View && !perm.Execute)
                    continue;

                if (!ContainsGroup(userGroups, perm.GroupName))
                    continue;

                if (perm.IsGlobal)
                    return true;

                if (NamesMatch(perm.ResourceName, workflowName))
                    return true;
            }

            return false;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Compares <paramref name="permissionResourceName"/> against
        /// <paramref name="workflowName"/> using case-insensitive ordinal comparison.
        ///
        /// The workflow name may be a relative path with extension (e.g.
        /// <c>tools/Hello World</c>); the permission ResourceName is typically just the
        /// bare display name (e.g. <c>Hello World</c>).  Both forms are tried.
        /// </summary>
        static bool NamesMatch(string permissionResourceName, string workflowName)
        {
            if (string.IsNullOrEmpty(permissionResourceName))
                return false;

            // Exact match on whatever was stored (may already be a path).
            if (string.Equals(permissionResourceName, workflowName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Strip directory and extension from the workflow name for a bare-name comparison.
            var bareName = Path.GetFileNameWithoutExtension(workflowName);
            return string.Equals(permissionResourceName, bareName, StringComparison.OrdinalIgnoreCase);
        }

        static bool ContainsGroup(IReadOnlyList<string> groups, string groupName)
        {
            for (var i = 0; i < groups.Count; i++)
            {
                if (string.Equals(groups[i], groupName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
