/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Authorisation policy for a single named workflow, derived from the
/// <c>WindowsGroupPermissions</c> entries in <c>secure.config</c>.
/// </summary>
/// <remarks>
/// Role check uses OR logic — the caller must match at least ONE of
/// <see cref="AllowedGroups"/>.
/// Permission check uses AND logic — the matched group entry must have ALL flags
/// in <see cref="RequiredPermissions"/> set to <c>true</c>.
/// </remarks>
public sealed record WorkflowAuthPolicy(
    /// <summary>Workflow resource name (case-insensitive match), e.g. "Hello World".</summary>
    string WorkflowName,

    /// <summary>
    /// Groups (Windows group or Entra UPN/role) that have at least one permission
    /// entry for this workflow.  Uses OR logic — caller must be in at least one.
    /// Values come from <c>WindowsGroupPermission.WindowsGroup</c> in secure.config.
    /// </summary>
    IReadOnlyList<string> AllowedGroups,

    /// <summary>
    /// The specific permission flags the caller's matched group entry must hold.
    /// Uses AND logic — all flags must be present in the entry.
    /// </summary>
    WorkflowPermission RequiredPermissions,

    /// <summary>
    /// Full list of per-group permission entries for this workflow, used for
    /// detailed per-group permission validation beyond <see cref="RequiredPermissions"/>.
    /// </summary>
    IReadOnlyList<WorkflowGroupEntry> GroupEntries)
{
    /// <summary>
    /// Creates a <see cref="WorkflowAuthPolicy"/> from raw group-entry data.
    /// </summary>
    public static WorkflowAuthPolicy Create(
        string workflowName,
        IEnumerable<WorkflowGroupEntry> entries,
        WorkflowPermission requiredPermissions)
    {
        var entryList = entries.ToList().AsReadOnly();
        var groups    = entryList.Select(e => e.GroupName).Distinct(StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly();
        return new WorkflowAuthPolicy(workflowName, groups, requiredPermissions, entryList);
    }
}

/// <summary>
/// Per-group permission snapshot for a single workflow, built from a
/// <see cref="Security.PermissionEntry"/> record.
/// </summary>
public sealed record WorkflowGroupEntry(
    /// <summary>Windows group / Entra UPN or role name.</summary>
    string GroupName,

    /// <summary>Combined <see cref="WorkflowPermission"/> flags this group holds for the workflow.</summary>
    WorkflowPermission Permissions);
