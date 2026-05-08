/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Default implementation of <see cref="IWorkflowPolicyMatcher"/>.
///
/// Applies the group-OR / permission-AND rules described in
/// <c>secure.config</c> against the loaded <see cref="IWorkflowAuthPolicyLoader"/>
/// policies.
///
/// Replace or decorate this class to change matching behaviour without touching
/// middleware or HTTP function code.
/// </summary>
public sealed class WorkflowPolicyMatcher : IWorkflowPolicyMatcher
{
    private readonly IWorkflowAuthPolicyLoader _policyLoader;

    /// <summary>Creates a new matcher backed by the supplied policy loader.</summary>
    public WorkflowPolicyMatcher(IWorkflowAuthPolicyLoader policyLoader)
        => _policyLoader = policyLoader;

    /// <inheritdoc/>
    public PolicyMatchResult Evaluate(
        string                  workflowName,
        WorkflowClaimsPrincipal principal,
        WorkflowPermission      requiredPermissions = WorkflowPermission.View | WorkflowPermission.Execute)
    {
        // No policies loaded → open-access mode; caller decides whether to allow.
        var policy = _policyLoader.GetPolicy(workflowName);
        if (policy is null)
            return PolicyMatchResult.NoPolicy();

        // ── Group check (OR logic) ────────────────────────────────────────────
        // A caller matches a group entry when:
        //   (a) the group name appears in the caller's role/group claims, OR
        //   (b) the group name equals the caller's UPN (direct UPN entries).
        if (!principal.IsInAnyGroup(policy.AllowedGroups))
        {
            return PolicyMatchResult.DenyGroup(
                $"Caller '{principal.CallerIdentity}' is not in any allowed group " +
                $"[{string.Join(", ", policy.AllowedGroups)}] for workflow '{workflowName}'.");
        }

        // ── Permission check (AND logic) ──────────────────────────────────────
        // Find the first group entry whose name matches the caller (UPN or group
        // claim), then verify it holds all required permission flags.
        var matchedEntry = policy.GroupEntries.FirstOrDefault(e =>
            string.Equals(e.GroupName, principal.UserName, StringComparison.OrdinalIgnoreCase) ||
            principal.IsInGroup(e.GroupName));

        if (matchedEntry is not null &&
            !matchedEntry.Permissions.HasFlag(requiredPermissions))
        {
            return PolicyMatchResult.DenyPermission(
                $"Caller '{principal.CallerIdentity}' is in group '{matchedEntry.GroupName}' " +
                $"but holds permissions [{matchedEntry.Permissions}]; " +
                $"required [{requiredPermissions}] for workflow '{workflowName}'.",
                matchedEntry);
        }

        return PolicyMatchResult.Allow();
    }
}
