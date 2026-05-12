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
/// <para>
/// Evaluation flow:
/// <list type="number">
///   <item>
///     Call <see cref="IWorkflowAuthPolicyLoader.GetPolicy"/> to obtain a
///     <see cref="PolicyLookupResult"/>.
///   </item>
///   <item>
///     <b>Bypass</b> (<c>BYPASS_SECURE_CONFIG=true</c>, config not effective) →
///     <see cref="PolicyMatchResult.NoPolicy()"/>; middleware allows open-access.
///   </item>
///   <item>
///     <b>ConfigMissing</b> (config absent/blank, bypass not set) →
///     <see cref="PolicyMatchResult.DenyConfigMissing"/>; middleware returns 503.
///   </item>
///   <item>
///     <b>Policy(null)</b> (workflow unconfigured) →
///     <see cref="PolicyMatchResult.DenyGroup"/>; middleware returns 403.
///   </item>
///   <item>
///     <b>Policy(non-null)</b> → call
///     <see cref="IWorkflowAuthPolicyLoader.GetEffectivePermissions"/> to union
///     all permissions from matched roles (and Public if present).
///   </item>
///   <item>
///     Stamp resolved permissions onto the principal via
///     <see cref="WorkflowClaimsPrincipal.SetResolvedPermissions"/>.
///   </item>
///   <item>
///     Check <c>effectivePermissions.HasFlag(requiredPermissions)</c> → Allow or DenyPermission.
///   </item>
/// </list>
/// </para>
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
        var lookup = _policyLoader.GetPolicy(workflowName);

        // ── BYPASS_SECURE_CONFIG=true — open-access mode ──────────────────────
        if (lookup.IsBypass)
            return PolicyMatchResult.NoPolicy();

        // ── Config absent or blank — deployment error ─────────────────────────
        if (lookup.IsConfigMissing)
            return PolicyMatchResult.DenyConfigMissing(
                "secure.config is absent or contains no permission entries. " +
                "Set BYPASS_SECURE_CONFIG=true to enable open-access mode explicitly, " +
                "or provide a valid secure.config.");

        // ── Workflow unconfigured — deny 403 ──────────────────────────────────
        if (lookup.Value is null)
            return PolicyMatchResult.DenyGroup(
                $"Workflow '{workflowName}' has no entries in secure.config. " +
                "Add a WindowsGroupPermissions entry to grant access.");

        // ── Resolve effective permissions for this caller ─────────────────────
        // Collect all role identifiers: group claims + UPN for direct-UPN entries.
        var callerRoles = principal.Groups
            .Append(principal.UserName)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var effectivePermissions = _policyLoader.GetEffectivePermissions(workflowName, callerRoles);

        // ── No matching role and no Public entry in active scope ──────────────
        if (effectivePermissions == WorkflowPermission.None)
            return PolicyMatchResult.DenyGroup(
                $"Caller '{principal.CallerIdentity}' has no matching role or Public entry " +
                $"in the active scope for workflow '{workflowName}'. " +
                $"Caller roles: [{string.Join(", ", principal.Groups)}].");

        // ── Stamp resolved permissions onto the principal ─────────────────────
        principal.SetResolvedPermissions(effectivePermissions);

        // ── Permission sufficiency check (OR logic on multi-flag required) ────
        // A caller is allowed when their resolved permissions overlap with the
        // required-permission set in at least one bit.  This mirrors the
        // Warewolf Security Specs contract where `[RequireWorkflowPermission(
        // View | Execute)]` on /Secure/{name} means "View OR Execute is enough",
        // not "View AND Execute" — a user with only View on a resource is
        // expected to be able to access /Secure/<that-resource>.
        // Single-flag requirements (e.g. Administrator) still behave identically
        // because the overlap reduces to the same bit being set.
        if ((effectivePermissions & requiredPermissions) == WorkflowPermission.None)
        {
            // Find the first matched role entry to surface in the denial reason.
            var firstMatched = lookup.Value.RolePolicies
                .FirstOrDefault(e =>
                    e.IsPublic ||
                    principal.IsInGroup(e.GroupName) ||
                    string.Equals(e.GroupName, principal.UserName, StringComparison.OrdinalIgnoreCase));

            return PolicyMatchResult.DenyPermission(
                $"Caller '{principal.CallerIdentity}' resolved permissions [{effectivePermissions}] " +
                $"do not satisfy required [{requiredPermissions}] for workflow '{workflowName}'.",
                firstMatched!);
        }

        return PolicyMatchResult.Allow();
    }
}

