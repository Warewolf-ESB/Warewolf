/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Immutable configuration snapshot used by the Bearer token validation pipeline.
/// Sourced from the merged <see cref="EntraIdentityOptions"/> JSON app setting (WOLF-8516;
/// previously 3 independent env vars — still described in the historical form in
/// <c>EasyAuth-Runbook.md</c>):
/// <list type="bullet">
///   <item><c>tenantId</c> – Entra tenant GUID.</item>
///   <item><c>audience</c>  – Expected <c>aud</c> claim (e.g. <c>api://{clientId}</c>).
///   A bare identifier with no URI scheme (e.g. just the client GUID) is auto-prefixed with
///   <c>api://</c> — see <see cref="Audience"/> — since that is the <c>aud</c> value Entra
///   actually issues for an <c>api://{clientId}/.default</c> scope request.</item>
///   <item><c>clientId</c> – Optional alternative audience.</item>
/// </list>
/// </summary>
public class EntraAuthOptions
{
    /// <summary>Entra tenant GUID. <c>null</c> when not configured (token path disabled).</summary>
    public string? TenantId { get; init; }

    private readonly string? _audience;

    /// <summary>
    /// Expected token audience (<c>aud</c> claim). A value with no URI scheme (e.g. a bare
    /// client GUID such as <c>05794411-b275-4801-97ac-8b078ed7196c</c>) is normalised to
    /// <c>api://{value}</c> on assignment, so a misconfigured <c>WAREWOLF_ENTRA_AUDIENCE</c>
    /// still matches the <c>aud</c> claim Entra actually issues instead of rejecting every
    /// caller with 401.
    /// </summary>
    public string? Audience
    {
        get => _audience;
        init => _audience = NormalizeAudience(value);
    }

    /// <summary>Optional alternative audience (typically the bare client GUID).</summary>
    public string? ClientId { get; init; }

    /// <summary>
    /// Prefixes <paramref name="value"/> with <c>api://</c> when it is non-empty and does not
    /// already specify a URI scheme (already <c>api://...</c>, <c>https://...</c>, etc.).
    /// </summary>
    private static string? NormalizeAudience(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Contains("://", StringComparison.Ordinal)
            ? value
            : $"api://{value}";

    /// <summary>
    /// <c>true</c> when sufficient configuration is present to attempt RS256 validation
    /// against Microsoft's public OIDC metadata.
    /// </summary>
    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(TenantId) &&
        (!string.IsNullOrWhiteSpace(Audience) || !string.IsNullOrWhiteSpace(ClientId));

    /// <summary>v2.0 OIDC metadata document URL for the configured tenant.</summary>
    public string MetadataAddress =>
        $"https://login.microsoftonline.com/{TenantId}/v2.0/.well-known/openid-configuration";

    /// <summary>Acceptable issuer URLs for tokens issued to this tenant (v1 + v2).</summary>
    public IReadOnlyCollection<string> ValidIssuers => new[]
    {
        $"https://login.microsoftonline.com/{TenantId}/v2.0",
        $"https://sts.windows.net/{TenantId}/",
    };

    /// <summary>Audiences accepted on the <c>aud</c> claim.</summary>
    public IReadOnlyCollection<string> ValidAudiences =>
        new[] { Audience, ClientId }
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!)
            .ToArray();

    /// <summary>
    /// Reads the configuration from the merged <see cref="EntraIdentityOptions.EnvVar"/> JSON
    /// app setting (WOLF-8516 — previously 3 independent env-var reads).
    /// </summary>
    public static EntraAuthOptions FromEnvironment()
    {
        var entra = EntraIdentityOptions.FromEnvironment();
        return new()
        {
            TenantId = entra.TenantId,
            Audience = entra.Audience,
            ClientId = entra.ClientId,
        };
    }
}
