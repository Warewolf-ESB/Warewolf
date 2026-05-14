/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// The post-union, post-scope resolved permission state for a single role
/// within an authorisation policy.
///
/// Built by <see cref="IWorkflowAuthPolicyLoader"/> after:
/// <list type="number">
///   <item>Grouping all <c>secure.config</c> entries for the same <c>WindowsGroup</c>.</item>
///   <item>Unioning <see cref="WorkflowPermission"/> flags within each group.</item>
///   <item>Selecting the active scope (resource-specific or global).</item>
/// </list>
///
/// <para>
/// When <see cref="IsPublic"/> is <c>true</c>, no role match is required —
/// every caller automatically receives these permissions.
/// </para>
/// </summary>
public sealed record ResolvedRolePolicy(
    /// <summary>
    /// Windows group or Entra role/UPN name exactly as written in
    /// <c>WindowsGroupPermission.WindowsGroup</c>.
    /// </summary>
    string GroupName,

    /// <summary>
    /// <c>true</c> when <see cref="GroupName"/> equals <c>"Public"</c>
    /// (case-insensitive).  Public entries grant their
    /// <see cref="EffectivePermissions"/> to every caller without a role match.
    /// </summary>
    bool IsPublic,

    /// <summary>
    /// Combined <see cref="WorkflowPermission"/> flags for this role after
    /// unioning all matching <c>secure.config</c> entries within the active scope.
    /// </summary>
    WorkflowPermission EffectivePermissions)
{
    /// <summary>
    /// Creates a <see cref="ResolvedRolePolicy"/> and automatically derives
    /// <see cref="IsPublic"/> from <paramref name="groupName"/>.
    /// </summary>
    public static ResolvedRolePolicy Create(string groupName, WorkflowPermission effectivePermissions) =>
        new(groupName,
            string.Equals(groupName, "Public", StringComparison.OrdinalIgnoreCase),
            effectivePermissions);
}
