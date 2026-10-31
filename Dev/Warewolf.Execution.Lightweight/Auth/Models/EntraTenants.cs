/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Which Entra tenants an engine accepts sign-ins from, given the <c>tenantId</c> it was
/// configured with in <c>WAREWOLF_ENTRA_CONFIG</c>. A tenant GUID accepts that one tenant. The
/// multi-tenant aliases Entra itself uses accept more:
/// <list type="bullet">
///   <item><c>common</c>: any work or school tenant, and personal Microsoft accounts.</item>
///   <item><c>organizations</c>: any work or school tenant, but not personal Microsoft accounts.</item>
/// </list>
/// A shared engine that every Web Studio user can reach (warewolfserver) uses <c>common</c>.
/// What a caller may then do is still decided by secure.config, so an engine that accepts any
/// tenant should grant <c>Authenticated Users</c> no more than it means any stranger to have.
/// </summary>
public static class EntraTenants
{
    /// <summary>Any work or school tenant, and personal Microsoft accounts.</summary>
    public const string Common = "common";

    /// <summary>Any work or school tenant.</summary>
    public const string Organizations = "organizations";

    /// <summary>The tenant every personal Microsoft account's token is issued from.</summary>
    public const string ConsumersTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

    /// <summary><c>true</c> when <paramref name="configured"/> is one of the multi-tenant aliases.</summary>
    public static bool IsMultiTenant(string? configured) =>
        string.Equals(configured?.Trim(), Common, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(configured?.Trim(), Organizations, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// <c>true</c> when an engine configured with <paramref name="configured"/> accepts a token
    /// whose <c>tid</c> claim is <paramref name="tokenTenantId"/>. A multi-tenant alias still needs
    /// the token to name a tenant.
    /// </summary>
    public static bool Accepts(string? configured, string? tokenTenantId)
    {
        if (string.IsNullOrWhiteSpace(configured) || string.IsNullOrWhiteSpace(tokenTenantId))
            return false;

        var tenant = configured.Trim();
        if (string.Equals(tenant, Common, StringComparison.OrdinalIgnoreCase))
            return Guid.TryParse(tokenTenantId, out _);
        if (string.Equals(tenant, Organizations, StringComparison.OrdinalIgnoreCase))
            return Guid.TryParse(tokenTenantId, out _) &&
                   !string.Equals(tokenTenantId, ConsumersTenantId, StringComparison.OrdinalIgnoreCase);
        return string.Equals(tenant, tokenTenantId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The issuers Entra uses for tokens from <paramref name="tenantId"/> (v2 and v1).</summary>
    public static IReadOnlyCollection<string> IssuersFor(string tenantId) => new[]
    {
        $"https://login.microsoftonline.com/{tenantId}/v2.0",
        $"https://sts.windows.net/{tenantId}/",
    };
}
