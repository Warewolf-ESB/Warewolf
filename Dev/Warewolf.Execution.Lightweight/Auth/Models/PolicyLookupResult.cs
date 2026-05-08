/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Discriminated-union result returned by
/// <see cref="IWorkflowAuthPolicyLoader.GetPolicy"/>.
///
/// Three mutually exclusive cases:
/// <list type="bullet">
///   <item>
///     <see cref="Bypass"/> — <c>secure.config</c> is not effective (absent or blank)
///     AND <c>BYPASS_SECURE_CONFIG=true</c> is explicitly set.
///     The middleware allows the request through in open-access mode with a warning log.
///   </item>
///   <item>
///     <see cref="ConfigMissing"/> — <c>secure.config</c> is not effective AND the
///     bypass env-var is not set.  This is a deployment error; the middleware returns
///     503 Service Unavailable.
///   </item>
///   <item>
///     <see cref="Policy"/> — <c>secure.config</c> is effective.
///     <see cref="Value"/> is non-null when a policy was resolved for the requested
///     workflow (resource-scope or global-scope); <c>null</c> when the workflow has
///     no entries in either scope (caller is denied with 403).
///   </item>
/// </list>
/// </summary>
public sealed class PolicyLookupResult
{
    // ── Private constructor ────────────────────────────────────────────────────

    private PolicyLookupResult(PolicyLookupKind kind, WorkflowAuthPolicy? value = null)
    {
        Kind  = kind;
        Value = value;
    }

    // ── Kind enum ─────────────────────────────────────────────────────────────

    private enum PolicyLookupKind { Bypass, ConfigMissing, Policy }

    // ── Internal kind ─────────────────────────────────────────────────────────

    private PolicyLookupKind Kind { get; }

    // ── Public surface ────────────────────────────────────────────────────────

    /// <summary>
    /// <c>true</c> when <c>secure.config</c> is not effective and
    /// <c>BYPASS_SECURE_CONFIG=true</c> is set — open-access mode.
    /// </summary>
    public bool IsBypass => Kind == PolicyLookupKind.Bypass;

    /// <summary>
    /// <c>true</c> when <c>secure.config</c> is absent or blank and the bypass
    /// env-var is not set — deployment error, return 503.
    /// </summary>
    public bool IsConfigMissing => Kind == PolicyLookupKind.ConfigMissing;

    /// <summary>
    /// <c>true</c> when <c>secure.config</c> is effective (loaded and non-empty).
    /// <see cref="Value"/> may still be <c>null</c> if the workflow is unconfigured.
    /// </summary>
    public bool HasPolicyScope => Kind == PolicyLookupKind.Policy;

    /// <summary>
    /// The resolved <see cref="WorkflowAuthPolicy"/> when <see cref="HasPolicyScope"/>
    /// is <c>true</c>; <c>null</c> when the workflow is unconfigured (→ 403).
    /// Always <c>null</c> for <see cref="IsBypass"/> and <see cref="IsConfigMissing"/>.
    /// </summary>
    public WorkflowAuthPolicy? Value { get; }

    // ── Factories ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="IsBypass"/> result — config not effective, bypass active.
    /// </summary>
    public static PolicyLookupResult Bypass() =>
        new(PolicyLookupKind.Bypass);

    /// <summary>
    /// Creates a <see cref="IsConfigMissing"/> result — config not effective, bypass not set.
    /// </summary>
    public static PolicyLookupResult ConfigMissing() =>
        new(PolicyLookupKind.ConfigMissing);

    /// <summary>
    /// Creates a <see cref="HasPolicyScope"/> result.
    /// Pass <c>null</c> when the workflow is unconfigured (no resource or global entries).
    /// </summary>
    public static PolicyLookupResult FromPolicy(WorkflowAuthPolicy? policy) =>
        new(PolicyLookupKind.Policy, policy);
}
