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
    /// <c>BYPASS_SECURE_CONFIG=true</c> is set and <c>secure.config</c> is not
    /// effective (absent or blank).  The middleware treats this as open-access
    /// and passes the request through with a warning log.
    /// </summary>
    NoPolicyFound,

    /// <summary>
    /// <c>secure.config</c> is absent or contains zero permission entries AND
    /// <c>BYPASS_SECURE_CONFIG</c> is not explicitly set to <c>true</c>.
    /// This is a deployment error — return 503 Service Unavailable so that
    /// operators are alerted rather than silently granting or denying access.
    /// </summary>
    ConfigMissingDeny,
}

/// <summary>
/// Result returned by <see cref="IWorkflowPolicyMatcher.Evaluate"/>.
/// </summary>
/// <param name="Outcome">High-level decision.</param>
/// <param name="DenialReason">
/// Human-readable explanation when <see cref="Outcome"/> is
/// <see cref="PolicyMatchOutcome.Forbidden"/> or
/// <see cref="PolicyMatchOutcome.ConfigMissingDeny"/>; <c>null</c> otherwise.
/// </param>
/// <param name="MatchedEntry">
/// The first <see cref="ResolvedRolePolicy"/> whose <see cref="ResolvedRolePolicy.GroupName"/>
/// matched the caller; <c>null</c> when no group matched or the outcome is
/// <see cref="PolicyMatchOutcome.NoPolicyFound"/> / <see cref="PolicyMatchOutcome.ConfigMissingDeny"/>.
/// </param>
public sealed record PolicyMatchResult(
    PolicyMatchOutcome  Outcome,
    string?             DenialReason = null,
    ResolvedRolePolicy? MatchedEntry = null)
{
    /// <summary>Shorthand — caller is authorised.</summary>
    public static PolicyMatchResult Allow() => new(PolicyMatchOutcome.Allowed);

    /// <summary>Shorthand — group membership check failed (policy exists but caller has no matching role).</summary>
    public static PolicyMatchResult DenyGroup(string reason) =>
        new(PolicyMatchOutcome.Forbidden, reason);

    /// <summary>Shorthand — role matched but resolved permissions are insufficient for required flags.</summary>
    public static PolicyMatchResult DenyPermission(string reason, ResolvedRolePolicy matched) =>
        new(PolicyMatchOutcome.Forbidden, reason, matched);

    /// <summary>
    /// Shorthand — <c>BYPASS_SECURE_CONFIG=true</c> is active; config is not effective.
    /// Middleware passes request through in open-access mode with a warning log.
    /// </summary>
    public static PolicyMatchResult NoPolicy() => new(PolicyMatchOutcome.NoPolicyFound);

    /// <summary>
    /// Shorthand — <c>secure.config</c> is absent or blank and bypass is not set.
    /// Middleware returns 503 Service Unavailable (deployment error).
    /// </summary>
    public static PolicyMatchResult DenyConfigMissing(string reason) =>
        new(PolicyMatchOutcome.ConfigMissingDeny, reason);
}
