/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Security.Claims;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Strongly-typed ClaimsPrincipal for Warewolf.Execution.Lightweight.
/// Built by <see cref="Middleware.ClaimsPrincipalBuilderMiddleware"/> from the
/// X-MS-CLIENT-PRINCIPAL header injected by Azure Easy Auth.
/// Supports both delegated (user impersonation) and app-only (client credentials) tokens.
///
/// <para>
/// <b>Permission model.</b>  The Entra token carries only <em>role</em> claims
/// (e.g. <c>"Developers"</c>, <c>"DevOps"</c>).  Permissions are <b>not</b> encoded
/// in the token; they are resolved at request time by
/// <see cref="IWorkflowAuthPolicyLoader.GetEffectivePermissions"/> against
/// <c>secure.config</c> and stamped onto this principal via
/// <see cref="SetResolvedPermissions"/>.
/// </para>
/// </summary>
public sealed class WorkflowClaimsPrincipal : ClaimsPrincipal
{
    // Maps WorkflowPermission flag → human-readable label for logging/diagnostics
    private static readonly IReadOnlyDictionary<WorkflowPermission, string> PermissionFlagMap =
        new Dictionary<WorkflowPermission, string>
        {
            [WorkflowPermission.View]          = "Permission.View",
            [WorkflowPermission.Execute]       = "Permission.Execute",
            [WorkflowPermission.Contribute]    = "Permission.Contribute",
            [WorkflowPermission.DeployTo]      = "Permission.DeployTo",
            [WorkflowPermission.DeployFrom]    = "Permission.DeployFrom",
            [WorkflowPermission.Administrator] = "Permission.Administrator",
        };

    /// <summary>Initialises a new instance from a <see cref="ClaimsIdentity"/>.</summary>
    public WorkflowClaimsPrincipal(ClaimsIdentity identity) : base(identity)
    {
        UserId   = FindFirst(AuthConstants.ObjectIdentifier)?.Value
                ?? FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;

        UserName = FindFirst(AuthConstants.PreferredUsername)?.Value
                ?? FindFirst(ClaimTypes.Name)?.Value
                ?? string.Empty;

        IsUserToken    = Claims.Any(c => c.Type == AuthConstants.Scope);
        IsAppOnlyToken = !IsUserToken;
        CallerIdentity = IsUserToken ? UserName : $"app:{UserId}";

        // Groups = all role claim values from the Entra token.
        // These are matched against WindowsGroup entries in secure.config.
        var roleClaims = Claims
            .Where(c => c.Type is AuthConstants.Roles or ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        Groups = roleClaims.AsReadOnly();

        // Permissions are resolved from secure.config at request time, not from the token.
        // SetResolvedPermissions() is called by WorkflowPolicyMatcher after resolution.
        Permissions = WorkflowPermission.None;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Entra ID object ID of the user or app service principal.</summary>
    public string UserId { get; }

    /// <summary>UPN or display name of the signed-in user. Empty for app-only tokens.</summary>
    public string UserName { get; }

    /// <summary>
    /// True when the token was issued via user impersonation (delegated flow).
    /// The "scp" claim is present only in delegated tokens.
    /// </summary>
    public bool IsUserToken { get; }

    /// <summary>True when the token was issued via client credentials (app-only flow).</summary>
    public bool IsAppOnlyToken { get; }

    /// <summary>
    /// Human-readable caller label for logging.
    /// UserName for delegated tokens; "app:{UserId}" for app-only tokens.
    /// </summary>
    public string CallerIdentity { get; }

    /// <summary>
    /// All role/group claim values assigned to this principal from the Entra token.
    /// Matched against <c>WindowsGroup</c> values in <c>secure.config</c>.
    /// e.g. ["Developers", "DevOps"]
    /// </summary>
    public IReadOnlyList<string> Groups { get; }

    /// <summary>
    /// Effective <see cref="WorkflowPermission"/> flags resolved from
    /// <c>secure.config</c> for the current request's workflow.
    ///
    /// Populated by <see cref="SetResolvedPermissions"/> after
    /// <see cref="IWorkflowAuthPolicyLoader.GetEffectivePermissions"/> runs.
    /// Value is <see cref="WorkflowPermission.None"/> until resolution completes.
    /// </summary>
    public WorkflowPermission Permissions { get; private set; }

    // ── Permission resolution ─────────────────────────────────────────────────

    /// <summary>
    /// Stamps the resolved <see cref="WorkflowPermission"/> flags onto this principal.
    /// Called once per request by <see cref="WorkflowPolicyMatcher"/> after
    /// <see cref="IWorkflowAuthPolicyLoader.GetEffectivePermissions"/> returns.
    /// </summary>
    public void SetResolvedPermissions(WorkflowPermission resolved) =>
        Permissions = resolved;

    // ── Group / role methods ──────────────────────────────────────────────────

    /// <summary>Returns true if this principal is in the specified group (case-insensitive).</summary>
    public bool IsInGroup(string group) =>
        Groups.Contains(group, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if this principal is in at least ONE of the specified groups (OR logic).
    /// Also matches by <see cref="UserName"/> (UPN) to support email-address group entries
    /// from secure.config.
    /// </summary>
    public bool IsInAnyGroup(IEnumerable<string> groups) =>
        groups.Any(g =>
            IsInGroup(g) ||
            string.Equals(UserName, g, StringComparison.OrdinalIgnoreCase));

    // ── Permission flag methods ───────────────────────────────────────────────

    /// <summary>
    /// Returns true if this principal's resolved permissions contain ALL flags
    /// in <paramref name="permission"/> (AND logic).
    /// </summary>
    public bool HasPermissionFlag(WorkflowPermission permission)
    {
        if (permission == WorkflowPermission.None) return true;
        return Permissions.HasFlag(permission);
    }

    /// <summary>
    /// The resolved <see cref="WorkflowPermission"/> flags — alias for
    /// <see cref="Permissions"/> for backward-compatible callers.
    /// </summary>
    public WorkflowPermission PermissionFlags => Permissions;

    /// <summary>
    /// Returns a dictionary mapping every known permission label to whether
    /// this principal holds it.  Useful for diagnostic logging and response bodies.
    /// </summary>
    public Dictionary<string, bool> GetPermissionSummary() =>
        PermissionFlagMap.ToDictionary(
            kvp => kvp.Value,
            kvp => Permissions.HasFlag(kvp.Key),
            StringComparer.OrdinalIgnoreCase);

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>Creates an unauthenticated anonymous principal with no claims.</summary>
    public static WorkflowClaimsPrincipal Anonymous() =>
        new(new ClaimsIdentity());

    // ── Overrides ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override string ToString() =>
        $"User:{UserName}|Groups:{string.Join(",", Groups)}|Perms:{Permissions}";
}

