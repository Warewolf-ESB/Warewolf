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
/// </summary>
public sealed class WorkflowClaimsPrincipal : ClaimsPrincipal
{
    // Maps WorkflowPermission flag → Permission.* claim value
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

        UserName = FindFirst(ClaimTypes.Name)?.Value
                ?? FindFirst(AuthConstants.PreferredUsername)?.Value
                ?? string.Empty;

        IsUserToken    = Claims.Any(c => c.Type == AuthConstants.Scope);
        IsAppOnlyToken = !IsUserToken;
        CallerIdentity = IsUserToken ? UserName : $"app:{UserId}";

        var roleClaims = Claims
            .Where(c => c.Type is AuthConstants.Roles or ClaimTypes.Role)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // The Groups property surfaces all role claim values, including group memberships
        // matched against WindowsGroup entries in secure.config (e.g. email addresses).
        Groups = roleClaims.AsReadOnly();

        Permissions = roleClaims
            .Where(v => v.StartsWith("Permission.", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
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
    /// All role/group claim values assigned to this principal.
    /// Matched against <c>WindowsGroup</c> values in <c>secure.config</c>.
    /// e.g. ["alice@contoso.com", "Public"]
    /// </summary>
    public IReadOnlyList<string> Groups { get; }

    /// <summary>
    /// Permission app roles assigned to this principal (Permission.* prefix only).
    /// e.g. ["Permission.View", "Permission.Execute"]
    /// </summary>
    public IReadOnlyList<string> Permissions { get; }

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

    /// <summary>Returns true if this principal has the specified permission claim (case-insensitive).</summary>
    public bool HasPermission(string permissionValue) =>
        Permissions.Contains(permissionValue, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if this principal has ALL permission flags specified (AND logic).
    /// Each flag is mapped to its "Permission.*" claim value and checked individually.
    /// </summary>
    public bool HasPermissionFlag(WorkflowPermission permission)
    {
        if (permission == WorkflowPermission.None) return true;

        foreach (var (flag, claimValue) in PermissionFlagMap)
        {
            if (permission.HasFlag(flag) && !HasPermission(claimValue))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the combined <see cref="WorkflowPermission"/> flags for all
    /// Permission.* claims this principal holds.
    /// Useful for diagnostic responses and structured logging.
    /// </summary>
    public WorkflowPermission PermissionFlags
    {
        get
        {
            var flags = WorkflowPermission.None;
            foreach (var (flag, claimValue) in PermissionFlagMap)
            {
                if (HasPermission(claimValue))
                    flags |= flag;
            }
            return flags;
        }
    }

    /// <summary>
    /// Returns a dictionary mapping every known permission claim value to whether
    /// this principal holds it.  Useful for diagnostic logging and secure-endpoint
    /// response bodies.
    /// </summary>
    public Dictionary<string, bool> GetPermissionSummary() =>
        PermissionFlagMap.ToDictionary(
            kvp => kvp.Value,
            kvp => HasPermission(kvp.Value),
            StringComparer.OrdinalIgnoreCase);

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>Creates an unauthenticated anonymous principal with no claims.</summary>
    public static WorkflowClaimsPrincipal Anonymous() =>
        new(new ClaimsIdentity());

    // ── Overrides ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override string ToString() =>
        $"User:{UserName}|Groups:{string.Join(",", Groups)}|Perms:{Permissions.Count}";
}
