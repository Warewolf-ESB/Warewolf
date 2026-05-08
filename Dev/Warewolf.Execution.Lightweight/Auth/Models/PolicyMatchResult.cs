/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// The outcome of a single policy-match evaluation performed by
/// <see cref="IWorkflowPolicyMatcher"/>.
/// </summary>
public enum PolicyMatchOutcome
{
    /// <summary>
    /// The caller satisfies all group and permission requirements for the
    /// requested workflow — execution may proceed.
    /// </summary>
    Allowed,

    /// <summary>
    /// A policy exists for the workflow but the caller does not meet the
    /// group membership or permission requirements — return 403.
    /// </summary>
    Forbidden,

    /// <summary>
    /// No policy was found for the workflow name.
    /// The caller's middleware should decide whether to allow or deny
    /// (currently: allow, so missing config doesn't lock everything out).
    /// </summary>
    NoPolicyFound,
}

/// <summary>
/// Result returned by <see cref="IWorkflowPolicyMatcher.Evaluate"/>.
/// </summary>
/// <param name="Outcome">High-level decision.</param>
/// <param name="DenialReason">
/// Human-readable explanation when <see cref="Outcome"/> is
/// <see cref="PolicyMatchOutcome.Forbidden"/>; <c>null</c> otherwise.
/// </param>
/// <param name="MatchedGroup">
/// The first group entry whose name matched the caller; <c>null</c> when
/// no group matched or the outcome is <see cref="PolicyMatchOutcome.NoPolicyFound"/>.
/// </param>
public sealed record PolicyMatchResult(
    PolicyMatchOutcome Outcome,
    string?            DenialReason  = null,
    WorkflowGroupEntry? MatchedGroup = null)
{
    /// <summary>Shorthand — caller is authorised.</summary>
    public static PolicyMatchResult Allow() => new(PolicyMatchOutcome.Allowed);

    /// <summary>Shorthand — group membership check failed.</summary>
    public static PolicyMatchResult DenyGroup(string reason) =>
        new(PolicyMatchOutcome.Forbidden, reason);

    /// <summary>Shorthand — permission flags check failed.</summary>
    public static PolicyMatchResult DenyPermission(string reason, WorkflowGroupEntry matched) =>
        new(PolicyMatchOutcome.Forbidden, reason, matched);

    /// <summary>Shorthand — no policy registered for the workflow.</summary>
    public static PolicyMatchResult NoPolicy() => new(PolicyMatchOutcome.NoPolicyFound);
}
