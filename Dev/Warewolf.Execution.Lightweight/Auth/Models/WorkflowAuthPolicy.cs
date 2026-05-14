/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Authorisation policy for a single named workflow (or the server-wide global
/// scope), derived from the <c>WindowsGroupPermissions</c> entries in
/// <c>secure.config</c>.
///
/// <para>
/// Permission resolution rules:
/// <list type="bullet">
///   <item>
///     <b>Public role</b> — when <see cref="HasPublicRole"/> is <c>true</c>,
///     every caller automatically receives the Public entry's
///     <see cref="ResolvedRolePolicy.EffectivePermissions"/> without needing a
///     role match.
///   </item>
///   <item>
///     <b>Role matching — OR logic</b> — the caller must match at least ONE
///     non-Public entry in <see cref="RolePolicies"/> (by group name or UPN).
///   </item>
///   <item>
///     <b>Permission union</b> — effective permissions for the caller are the
///     bitwise OR of all matched role entries (including Public if present).
///   </item>
///   <item>
///     <b>Required permission — AND logic</b> — the combined permissions must
///     contain ALL flags in the route's <c>RequiredPermissions</c> declaration.
///   </item>
/// </list>
/// </para>
/// </summary>
public sealed record WorkflowAuthPolicy(
    /// <summary>Workflow resource name (lowercase), e.g. "hello world".</summary>
    string WorkflowName,

    /// <summary>
    /// Per-role resolved permission entries for this policy scope.
    /// Each entry carries the post-union <see cref="WorkflowPermission"/> flags
    /// for one <c>WindowsGroup</c> within the active scope (resource or global).
    /// </summary>
    IReadOnlyList<ResolvedRolePolicy> RolePolicies,

    /// <summary>
    /// The minimum permission flags a caller must hold after resolution.
    /// Stored for diagnostic logging; the actual check is performed by
    /// <see cref="IWorkflowPolicyMatcher"/>.
    /// </summary>
    WorkflowPermission RequiredPermissions)
{
    // ── Derived convenience properties ────────────────────────────────────────

    /// <summary>
    /// <c>true</c> when at least one entry in <see cref="RolePolicies"/> is the
    /// Public role — meaning every caller automatically receives those permissions.
    /// </summary>
    public bool HasPublicRole => RolePolicies.Any(e => e.IsPublic);

    /// <summary>
    /// All distinct group names in <see cref="RolePolicies"/>.
    /// Preserved for backward-compatible diagnostic logging and test assertions.
    /// </summary>
    public IReadOnlyList<string> AllowedGroups =>
        RolePolicies
            .Select(e => e.GroupName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

    /// <summary>
    /// <see cref="RolePolicies"/> projected as <see cref="WorkflowGroupEntry"/> instances
    /// for backward compatibility with any existing callers that read <c>GroupEntries</c>.
    /// </summary>
    public IReadOnlyList<WorkflowGroupEntry> GroupEntries =>
        RolePolicies
            .Select(e => new WorkflowGroupEntry(e.GroupName, e.EffectivePermissions))
            .ToList()
            .AsReadOnly();

    // ── Factories ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="WorkflowAuthPolicy"/> from a sequence of
    /// <see cref="ResolvedRolePolicy"/> entries.
    /// </summary>
    public static WorkflowAuthPolicy Create(
        string workflowName,
        IEnumerable<ResolvedRolePolicy> rolePolicies,
        WorkflowPermission requiredPermissions)
    {
        var list = rolePolicies.ToList().AsReadOnly();
        return new WorkflowAuthPolicy(workflowName, list, requiredPermissions);
    }

    /// <summary>
    /// Backward-compatible factory that accepts <see cref="WorkflowGroupEntry"/> instances.
    /// Converts each entry to a <see cref="ResolvedRolePolicy"/> via
    /// <see cref="ResolvedRolePolicy.Create"/>.
    /// </summary>
    public static WorkflowAuthPolicy Create(
        string workflowName,
        IEnumerable<WorkflowGroupEntry> entries,
        WorkflowPermission requiredPermissions)
    {
        var rolePolicies = entries
            .Select(e => ResolvedRolePolicy.Create(e.GroupName, e.Permissions))
            .ToList()
            .AsReadOnly();
        return new WorkflowAuthPolicy(workflowName, rolePolicies, requiredPermissions);
    }
}

/// <summary>
/// Per-group permission snapshot for a single workflow scope, built from a
/// <see cref="Security.PermissionEntry"/> record.
/// Retained for backward compatibility — prefer <see cref="ResolvedRolePolicy"/>.
/// </summary>
public sealed record WorkflowGroupEntry(
    /// <summary>Windows group / Entra UPN or role name.</summary>
    string GroupName,

    /// <summary>Combined <see cref="WorkflowPermission"/> flags this group holds.</summary>
    WorkflowPermission Permissions);

