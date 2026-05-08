/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Provides workflow authorisation policies derived from <c>secure.config</c>
/// <c>WindowsGroupPermissions</c> entries.
///
/// <para>
/// <b>secure.config is mandatory.</b>  When the config is absent or contains zero
/// permission entries and <c>BYPASS_SECURE_CONFIG</c> is not <c>true</c>,
/// <see cref="GetPolicy"/> returns <see cref="PolicyLookupResult.ConfigMissing"/>
/// and every request is denied with 503 Service Unavailable.
/// </para>
///
/// <para>
/// <b>Permission resolution rules:</b>
/// <list type="bullet">
///   <item>
///     When the requested workflow has resource-specific entries in
///     <c>secure.config</c>, <b>only those entries</b> are used to determine the
///     caller's effective permissions (global entries are discarded for that workflow).
///   </item>
///   <item>
///     When no resource-specific entries exist, the global (server-wide) entries
///     are used as the active scope.
///   </item>
///   <item>
///     <b>Public role</b> — a <c>WindowsGroup</c> named <c>"Public"</c> in the
///     active scope auto-applies its permissions to every caller without a role match.
///   </item>
///   <item>
///     <b>Multi-role union</b> — when the caller holds multiple roles that all
///     appear in the active scope, their permissions are bitwise-ORed together.
///   </item>
///   <item>
///     <b>Super-admin bypass</b> — when <c>WAREWOLF_SUPER_ADMIN_ENABLED=true</c>
///     and any of the caller's roles holds the
///     <see cref="WorkflowPermission.Administrator"/> flag in the <i>global</i>
///     role map, <see cref="GetEffectivePermissions"/> returns
///     <see cref="WorkflowPermission.All"/> immediately.
///   </item>
/// </list>
/// </para>
/// </summary>
public interface IWorkflowAuthPolicyLoader
{
    /// <summary>
    /// Returns a <see cref="PolicyLookupResult"/> describing the authorisation
    /// policy for the named workflow.
    ///
    /// <list type="bullet">
    ///   <item><see cref="PolicyLookupResult.IsBypass"/> — config not effective,
    ///         <c>BYPASS_SECURE_CONFIG=true</c> set; middleware allows open-access.</item>
    ///   <item><see cref="PolicyLookupResult.IsConfigMissing"/> — config not effective,
    ///         bypass not set; middleware returns 503.</item>
    ///   <item><see cref="PolicyLookupResult.HasPolicyScope"/> with non-null
    ///         <see cref="PolicyLookupResult.Value"/> — policy resolved; proceed to
    ///         <see cref="GetEffectivePermissions"/>.</item>
    ///   <item><see cref="PolicyLookupResult.HasPolicyScope"/> with null
    ///         <see cref="PolicyLookupResult.Value"/> — workflow unconfigured; deny 403.</item>
    /// </list>
    /// </summary>
    PolicyLookupResult GetPolicy(string workflowName);

    /// <summary>
    /// Resolves the effective <see cref="WorkflowPermission"/> flags for a caller
    /// identified by <paramref name="callerRoles"/> against the active scope for
    /// <paramref name="workflowName"/>.
    ///
    /// <para>
    /// Only call this when <see cref="GetPolicy"/> returned a non-null
    /// <see cref="PolicyLookupResult.Value"/>.
    /// </para>
    /// </summary>
    /// <param name="workflowName">Workflow name (case-insensitive).</param>
    /// <param name="callerRoles">
    /// All role/group claim values of the authenticated principal, including the
    /// caller's UPN when applicable.
    /// </param>
    /// <returns>
    /// Bitwise OR of all permissions the caller inherits from matched roles and
    /// any Public entry in the active scope.  Returns
    /// <see cref="WorkflowPermission.None"/> when nothing matched.
    /// </returns>
    WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles);

    /// <summary>Total number of pre-built resource-scope workflow policies.</summary>
    int PolicyCount { get; }

    /// <summary>
    /// <c>true</c> when <c>secure.config</c> is loaded and contains at least one
    /// permission entry.  <c>false</c> means the config is absent or blank.
    /// </summary>
    bool IsConfigEffective { get; }

    /// <summary>
    /// Reloads policies from the underlying <c>secure.config</c> source atomically.
    /// Safe to call concurrently; used by <see cref="SecureConfigWatcher"/> for
    /// hot-reload.
    /// </summary>
    void Reload();
}

