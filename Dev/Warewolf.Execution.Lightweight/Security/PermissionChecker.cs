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
    /// <para>
    /// Resolution mirrors <c>AuthorizationServiceBase.GetGroupPermissions</c> +
    /// <c>IsAuthorized</c> from the Warewolf server — adapted for the lightweight
    /// engine where Windows <c>IsInRole</c> is replaced by claim-based group matching
    /// against Entra ID role claims / HMAC-JWT group values carried in
    /// <see cref="WorkflowClaimsPrincipal.Groups"/>.
    /// </para>
    ///
    /// <b>Resource-override precedence (matches server behaviour):</b>
    /// <list type="bullet">
    ///   <item>
    ///     When a group has a resource-specific (<c>IsGlobal == false</c>) entry that
    ///     matches the requested workflow, that entry is the <em>only</em> source of
    ///     permissions for that group — global entries for the same group are
    ///     suppressed.  This prevents a broad global grant from silently overriding a
    ///     deliberately restrictive resource-specific denial.
    ///   </item>
    ///   <item>
    ///     Groups that have <em>no</em> resource-specific entry for the workflow
    ///     contribute their global entry (if any).
    ///   </item>
    /// </list>
    ///
    /// <b>Public group (anonymous / unauthenticated):</b><br/>
    /// The built-in <c>Public</c> group (<see cref="PermissionEntry.IsPublicGroup"/>) is
    /// always considered "in role" — it corresponds to <c>IsBuiltInGuestsForExecution</c>
    /// on the server and matches both anonymous callers and authenticated users who have
    /// not been granted a more specific entry.
    ///
    /// <b>Separator normalisation:</b><br/>
    /// <c>ResourceName</c> entries sourced from <c>secure.config</c> carry
    /// <c>ResourcePath</c> values that use back-slashes (<c>data\sales</c>).  Workflow
    /// names received from the file-system scanner or route segments use forward-slashes
    /// (<c>data/sales</c>) or bare names (<c>sales</c>).  <see cref="NamesMatch"/>
    /// normalises both sides before comparison.
    /// </summary>
    internal static class PermissionChecker
    {
        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when the workflow identified by <paramref name="workflowName"/>
        /// is visible on the public (<c>/Public/</c>) endpoint.
        ///
        /// <para>Mirrors <c>ServerAuthorizationService.IsAuthorizedImpl</c> for
        /// <c>WebServerRequestType.WebGetResourcesForExplorer</c> using
        /// <c>GlobalConstants.GenericPrincipal</c> + <c>AuthorizationContext.View</c>.</para>
        ///
        /// When no <c>secure.config</c> is loaded (<see cref="SecureConfigData.IsLoaded"/> ==
        /// <c>false</c>), all workflows are considered publicly visible (open-access mode).
        /// </summary>
        internal static bool HasPublicViewPermission(string workflowName, SecureConfigData config)
        {
            if (!config.IsLoaded)
                return true;

            return Evaluate(workflowName, config, isPublicRequest: true,
                            userGroups: null, static p => p.View);
        }

        /// <summary>
        /// Returns <c>true</c> when the workflow has <b>Execute</b> permission for the
        /// Public group.
        ///
        /// <para>Mirrors <c>ServerAuthorizationService.IsAuthorizedImpl</c> for
        /// <c>WebServerRequestType.WebExecutePublicWorkflow</c> which requires
        /// <c>AuthorizationContext.Execute</c> against <c>GlobalConstants.GenericPrincipal</c>.</para>
        /// </summary>
        internal static bool HasPublicExecutePermission(string workflowName, SecureConfigData config)
        {
            if (!config.IsLoaded)
                return true;

            return Evaluate(workflowName, config, isPublicRequest: true,
                            userGroups: null, static p => p.Execute);
        }

        /// <summary>
        /// Returns <c>true</c> when the workflow has <b>both</b> View <em>and</em>
        /// Execute permissions for the Public group.
        ///
        /// <para>Matches the server's <c>ApisJsonBuilder.BuildForPath</c> discovery loop
        /// which calls <c>IsAuthorized(GenericPrincipal, Execute)</c> AND
        /// <c>IsAuthorized(GenericPrincipal, View)</c> independently and includes the
        /// resource only when both return <c>true</c>.</para>
        ///
        /// <para>Implemented as a single O(N) pass via <see cref="EvaluateAll"/> rather
        /// than two independent <see cref="Evaluate"/> calls (which would be O(2N)).</para>
        /// </summary>
        internal static bool HasPublicDiscoveryPermission(string workflowName, SecureConfigData config)
        {
            if (!config.IsLoaded)
                return true;

            // Single pass: both Execute AND View must be satisfied — mirrors the two
            // independent IsAuthorized calls the server makes in ApisJsonBuilder.BuildForPath.
            return EvaluateAll(workflowName, config, isPublicRequest: true, userGroups: null,
                               static p => p.Execute,
                               static p => p.View);
        }

        /// <summary>
        /// Returns <c>true</c> when any group in <paramref name="userGroups"/> grants
        /// View access to <paramref name="workflowName"/>.
        ///
        /// <para>Mirrors <c>ApisJsonBuilder.BuildForPath</c> for
        /// <c>isPublic == false</c>: <c>IsAuthorized(OrginalExecutingUser, View, resource)</c>.
        /// Group membership is determined by Entra ID role claims / HMAC-JWT groups
        /// rather than Windows <c>IsInRole</c>.</para>
        /// </summary>
        internal static bool HasUserViewPermission(
            string                  workflowName,
            SecureConfigData        config,
            IReadOnlyList<string>   userGroups)
        {
            if (!config.IsLoaded)
                return true;

            return Evaluate(workflowName, config, isPublicRequest: false,
                            userGroups: userGroups, static p => p.View);
        }

        /// <summary>
        /// Returns <c>true</c> when any group in <paramref name="userGroups"/> grants
        /// <b>both</b> View <em>and</em> Execute access to <paramref name="workflowName"/>.
        ///
        /// <para>Mirrors the server's authenticated <c>ApisJsonBuilder.BuildForPath</c>
        /// discovery loop that calls <c>IsAuthorized(Execute)</c> AND
        /// <c>IsAuthorized(View)</c> independently.</para>
        ///
        /// <para>Implemented as a single O(N) pass via <see cref="EvaluateAll"/> rather
        /// than two independent <see cref="Evaluate"/> calls (which would be O(2N)).</para>
        /// </summary>
        internal static bool HasUserDiscoveryPermission(
            string                  workflowName,
            SecureConfigData        config,
            IReadOnlyList<string>   userGroups)
        {
            if (!config.IsLoaded)
                return true;

            // Single pass: both Execute AND View must be satisfied — mirrors the two
            // independent IsAuthorized calls the server makes in ApisJsonBuilder.BuildForPath.
            return EvaluateAll(workflowName, config, isPublicRequest: false, userGroups,
                               static p => p.Execute,
                               static p => p.View);
        }

        // ── Core evaluation engine ────────────────────────────────────────────────

        /// <summary>
        /// Single-pass multi-predicate variant of <see cref="Evaluate"/>.
        ///
        /// <para>
        /// Resolves the same two-phase override set as <see cref="Evaluate"/> but tests
        /// <em>all</em> supplied <paramref name="predicates"/> against the candidate
        /// entry set in one O(N) pass rather than requiring a separate O(N) scan per
        /// predicate.  Returns <c>true</c> only when <b>every</b> predicate is satisfied
        /// by at least one in-role candidate entry — equivalent to calling
        /// <see cref="Evaluate"/> independently for each predicate and AND-ing the
        /// results, but without the redundant work.
        /// </para>
        ///
        /// <para>
        /// Mirrors the server's <c>ApisJsonBuilder.BuildForPath</c> pattern of making two
        /// independent <c>IsAuthorized</c> calls (Execute and View) while avoiding the
        /// O(2N) cost of doing so.
        /// </para>
        /// </summary>
        static bool EvaluateAll(
            string                          workflowName,
            SecureConfigData                config,
            bool                            isPublicRequest,
            IReadOnlyList<string>?          userGroups,
            params Func<PermissionEntry, bool>[] predicates)
        {
            if (predicates.Length == 0)
                return true;

            var permissions = config.Permissions;

            // satisfied[i] tracks whether predicates[i] has been met by at least one entry.
            var satisfied = new bool[predicates.Length];
            var satisfiedCount = 0;

            // ── Phase 1: resource-specific entries ────────────────────────────────
            var overriddenGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in permissions)
            {
                if (p.IsGlobal)
                    continue;
                if (!NamesMatch(p.ResourceName, workflowName))
                    continue;

                overriddenGroups.Add(p.GroupName);

                if (!IsInRole(p, isPublicRequest, userGroups))
                    continue;

                // Test each unsatisfied predicate against this candidate.
                for (var i = 0; i < predicates.Length; i++)
                {
                    if (!satisfied[i] && predicates[i](p))
                    {
                        satisfied[i] = true;
                        satisfiedCount++;
                        if (satisfiedCount == predicates.Length)
                            return true;    // All predicates satisfied — early exit.
                    }
                }
            }

            // ── Phase 2: global entries not overridden ────────────────────────────
            foreach (var p in permissions)
            {
                if (!p.IsGlobal)
                    continue;
                if (overriddenGroups.Contains(p.GroupName))
                    continue;
                if (!IsInRole(p, isPublicRequest, userGroups))
                    continue;

                for (var i = 0; i < predicates.Length; i++)
                {
                    if (!satisfied[i] && predicates[i](p))
                    {
                        satisfied[i] = true;
                        satisfiedCount++;
                        if (satisfiedCount == predicates.Length)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Implements <c>AuthorizationServiceBase.GetGroupPermissions</c> +
        /// <c>IsAuthorized</c> for the lightweight engine.
        ///
        /// <para><b>Two-phase evaluation:</b></para>
        /// <list type="number">
        ///   <item>
        ///     <b>Resource phase</b> — collect all non-global entries whose
        ///     <c>ResourceName</c> matches <paramref name="workflowName"/> and whose
        ///     group passes <see cref="IsInRole"/>.  Record every group name that has a
        ///     resource-specific entry (the "override set").
        ///   </item>
        ///   <item>
        ///     <b>Global phase</b> — collect all global entries whose group is NOT in
        ///     the override set and passes <see cref="IsInRole"/>.
        ///   </item>
        /// </list>
        /// Returns <c>true</c> if any entry in the combined set satisfies
        /// <paramref name="permissionTest"/>.
        /// </summary>
        static bool Evaluate(
            string                          workflowName,
            SecureConfigData                config,
            bool                            isPublicRequest,
            IReadOnlyList<string>?          userGroups,
            Func<PermissionEntry, bool>     permissionTest)
        {
            var permissions = config.Permissions;

            // ── Phase 1: resource-specific entries ────────────────────────────────
            // Mirrors: permissionsForResource = matchedResources.Where(p => !p.IsServer)
            //                                                   .Where(p => IsInRole(principal, p))

            // Track which group names have a resource-specific entry for this workflow.
            // Those groups will have their global entry suppressed (server override rule).
            var overriddenGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in permissions)
            {
                if (p.IsGlobal)
                    continue;

                if (!NamesMatch(p.ResourceName, workflowName))
                    continue;

                // Record group as having a resource entry regardless of IsInRole result.
                // This mirrors server: even a denied resource entry suppresses the global
                // grant for that group.
                overriddenGroups.Add(p.GroupName);

                if (!IsInRole(p, isPublicRequest, userGroups))
                    continue;

                if (permissionTest(p))
                    return true;
            }

            // Groups not in overriddenGroups still get their global entry evaluated.

            // ── Phase 2: global entries not overridden ────────────────────────────
            // Mirrors: serverPermissionsNotOverridden = matchedResources
            //              .Where(permission => permissionsForResource.All(groupPermission =>
            //                  groupPermission.ResourceID == Guid.Empty ||
            //                  groupPermission.WindowsGroup != permission.WindowsGroup))
            //              .Where(permission => IsInRole(principal, permission))

            foreach (var p in permissions)
            {
                if (!p.IsGlobal)
                    continue;

                // If this group already has a resource-specific entry, skip it.
                if (overriddenGroups.Contains(p.GroupName))
                    continue;

                if (!IsInRole(p, isPublicRequest, userGroups))
                    continue;

                if (permissionTest(p))
                    return true;
            }

            return false;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Lightweight equivalent of <c>AuthorizationServiceBase.IsAuthorizedToConnect</c>.
        ///
        /// <para>
        /// Returns <c>true</c> when the caller represented by <paramref name="userGroups"/>
        /// has <em>any</em> permission whatsoever in <c>secure.config</c> — i.e. is a
        /// member of at least one group that has a non-zero permission entry (global or
        /// resource-specific).
        /// </para>
        ///
        /// <para>
        /// This is the <b>connect gate</b> for the <c>/apis.json</c> endpoint.  The
        /// server checks this before the per-resource discovery loop:
        /// <c>IsAuthorizedToConnect</c> → <c>IsAuthorized(AuthorizationContext.Any, principal,
        /// () =&gt; GetGroupPermissions(principal))</c>.
        /// <c>GetGroupPermissions(principal)</c> (no resource argument) scans <em>all</em>
        /// permissions — both global and resource-specific — which is why this method
        /// does the same rather than restricting to global entries only.
        /// </para>
        ///
        /// <para>
        /// When no <c>secure.config</c> is loaded (<see cref="SecureConfigData.IsLoaded"/>
        /// == <c>false</c>), the method returns <c>true</c> unconditionally (open-access
        /// mode).
        /// </para>
        /// </summary>
        internal static bool HasConnectPermission(
            SecureConfigData        config,
            IReadOnlyList<string>?  userGroups)
        {
            if (!config.IsLoaded)
                return true;

            return EvaluateGlobal(config, isPublicRequest: false, userGroups, HasAnyPermissionFlag);
        }

        /// <inheritdoc cref="HasConnectPermission(SecureConfigData,IReadOnlyList{string}?)"/>
        /// <remarks>Public-caller variant used when <c>?isPublic=true</c>.</remarks>
        internal static bool HasPublicConnectPermission(SecureConfigData config)
        {
            if (!config.IsLoaded)
                return true;

            return EvaluateGlobal(config, isPublicRequest: true, userGroups: null, HasAnyPermissionFlag);
        }

        /// <summary>
        /// Scans <b>all</b> permission entries (global and resource-specific) without
        /// name-matching — mirrors <c>GetGroupPermissions(principal)</c> (the no-resource
        /// overload) used by <c>IsAuthorizedToConnect</c>.
        ///
        /// <para>
        /// Returns <c>true</c> when any in-role entry satisfies
        /// <paramref name="permissionTest"/>.
        /// </para>
        /// </summary>
        static bool EvaluateGlobal(
            SecureConfigData                config,
            bool                            isPublicRequest,
            IReadOnlyList<string>?          userGroups,
            Func<PermissionEntry, bool>     permissionTest)
        {
            foreach (var p in config.Permissions)
            {
                if (!IsInRole(p, isPublicRequest, userGroups))
                    continue;
                if (permissionTest(p))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns <c>true</c> when the entry carries at least one non-default permission
        /// flag — mirrors <c>(p.Permissions &amp; contextPermissions) != 0</c> where
        /// <c>contextPermissions</c> is the expansion of <c>AuthorizationContext.Any</c>
        /// (all permission bits OR'd together).
        /// </summary>
        static bool HasAnyPermissionFlag(PermissionEntry p)
            => p.View || p.Execute || p.Contribute || p.DeployTo || p.DeployFrom || p.Administrator;

        /// <summary>
        /// Lightweight replacement for <c>AuthorizationServiceBase.IsInRole</c>.
        ///
        /// <para>Group membership is checked against Entra ID role claims or HMAC-JWT
        /// group values — Windows <c>IPrincipal.IsInRole</c> is not used.</para>
        ///
        /// <list type="bullet">
        ///   <item>
        ///     <b>Public group</b> (<see cref="PermissionEntry.IsPublicGroup"/>) always
        ///     returns <c>true</c> — mirrors <c>IsBuiltInGuestsForExecution</c> on the
        ///     server.  For public requests this is the only group that ever matches;
        ///     for authenticated requests it provides the open-access fallback.
        ///   </item>
        ///   <item>
        ///     <b>Authenticated groups</b> are compared case-insensitively against
        ///     <paramref name="userGroups"/> (Entra role claims or HMAC-JWT groups).
        ///   </item>
        /// </list>
        /// </summary>
        static bool IsInRole(
            PermissionEntry         entry,
            bool                    isPublicRequest,
            IReadOnlyList<string>?  userGroups)
        {
            // Public group always matches — equivalent to server IsBuiltInGuestsForExecution.
            if (entry.IsPublicGroup)
                return true;

            // For a pure public-path check we only care about the Public group.
            if (isPublicRequest)
                return false;

            // Authenticated path: match against claim-based groups (Entra roles / HMAC-JWT).
            return userGroups is not null && ContainsGroup(userGroups, entry.GroupName);
        }

        /// <summary>
        /// Compares a permission's <c>ResourceName</c> against a workflow's relative path
        /// using a path-exact, case-insensitive match after normalising directory separators.
        ///
        /// <para>
        /// <c>ResourceName</c> is sourced from <c>ResourcePath</c> in <c>secure.config</c>
        /// (e.g. <c>data\sales</c>) and the workflow name passed in is the relative path from
        /// <see cref="ApisJsonGenerator"/> or the route segment (e.g. <c>data/sales</c>).
        /// Back-slashes are normalised to forward-slashes on both sides before comparison.
        /// </para>
        ///
        /// <para>
        /// <b>Intentionally no bare-name fallback.</b>  A permission for <c>data\sales</c>
        /// targets the <c>sales</c> workflow inside the <c>data</c> folder and must
        /// <em>not</em> match a root-level <c>sales</c> workflow (different resource).
        /// Similarly, a bare-name permission <c>sales</c> must not match <c>data/sales</c>.
        /// </para>
        /// </summary>
        internal static bool NamesMatch(string permissionResourceName, string workflowName)
        {
            if (string.IsNullOrEmpty(permissionResourceName))
                return false;

            // Normalise back-slashes to forward-slashes so that "data\sales" == "data/sales".
            var normPerm = permissionResourceName.Replace('\\', '/');
            var normWf   = workflowName.Replace('\\', '/');

            return string.Equals(normPerm, normWf, StringComparison.OrdinalIgnoreCase);
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
