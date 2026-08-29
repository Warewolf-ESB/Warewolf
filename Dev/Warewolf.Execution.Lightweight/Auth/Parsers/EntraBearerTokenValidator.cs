/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth.Parsers;

/// <summary>
/// Shared RS256 bearer-token validation core against Microsoft Entra's published OIDC
/// metadata (issuer, audience, signature, lifetime).
///
/// <para>
/// This is the ONE validation code path used by every transport that accepts an Entra
/// token — <see cref="BearerTokenPrincipalParser"/> (HTTP <c>/secure</c> and
/// <c>/services</c> routes) and the Service Bus secure-trigger's message-level token
/// check. Extracted so both entry points can never silently diverge in what they accept:
/// a change here (e.g. a stricter clock-skew) takes effect for both transports at once —
/// mirrors the "one shared Authorize() component" principle applied one layer earlier, to
/// token validation itself.
/// </para>
///
/// <para>
/// Each instance is bound to one <see cref="EntraAuthOptions"/> (tenant + audience), so the
/// HTTP path and the Service Bus path — which intentionally use DIFFERENT, non-overlapping
/// audiences (see <see cref="ServiceBusEntraAuthOptions"/>) — each get their own validator
/// instance and their own cached OIDC signing-key set.
/// </para>
/// </summary>
public sealed class EntraBearerTokenValidator
{
    private readonly EntraAuthOptions _options;
    private readonly Lazy<ConfigurationManager<OpenIdConnectConfiguration>?> _configManager;
    private readonly JwtSecurityTokenHandler _handler = new() { MapInboundClaims = false };

    /// <summary>Initialises the validator for the supplied Entra options.</summary>
    public EntraBearerTokenValidator(EntraAuthOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _configManager = new Lazy<ConfigurationManager<OpenIdConnectConfiguration>?>(() =>
        {
            if (!_options.IsEnabled)
                return null;

            return new ConfigurationManager<OpenIdConnectConfiguration>(
                _options.MetadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = true })
            {
                AutomaticRefreshInterval = TimeSpan.FromHours(24),
                RefreshInterval          = TimeSpan.FromMinutes(5),
            };
        });
    }

    /// <summary>
    /// <c>true</c> when the bound <see cref="EntraAuthOptions"/> carries enough
    /// configuration (tenant + audience) to attempt validation at all. Callers should
    /// treat <c>false</c> as "reject — auth not configured", never as "allow".
    /// </summary>
    public bool IsEnabled => _options.IsEnabled;

    /// <summary>
    /// Validates <paramref name="rawToken"/> (the JWT without the <c>Bearer </c> prefix)
    /// against issuer, audience, signature, and lifetime. Throws
    /// <see cref="SecurityTokenException"/> (or a subclass, e.g.
    /// <see cref="SecurityTokenExpiredException"/>) on any validation failure so callers
    /// can distinguish "expired" from "otherwise invalid" without re-parsing the token.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="IsEnabled"/> is <c>false</c>.</exception>
    public async Task<ClaimsIdentity> ValidateAsync(string rawToken, CancellationToken cancellationToken)
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException(
                "Entra token validation is not configured (missing tenant/audience) for this validator instance.");
        }

        var configuration = await _configManager.Value!
            .GetConfigurationAsync(cancellationToken)
            .ConfigureAwait(false);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuers             = _options.ValidIssuers,
            ValidateAudience         = true,
            ValidAudiences           = _options.ValidAudiences,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys        = configuration.SigningKeys,
            ClockSkew                = TimeSpan.FromMinutes(2),
            NameClaimType            = ClaimTypes.Name,
            RoleClaimType            = ClaimTypes.Role,
        };

        var result = _handler.ValidateToken(rawToken, parameters, out _);

        // Re-project claims into the same normalised shape used by the EasyAuth parser so
        // downstream authorization is identical regardless of which auth path was used.
        return new ClaimsIdentity(
            NormalizeClaims(result.Claims),
            authenticationType: "Bearer",
            nameType:           ClaimTypes.Name,
            roleType:           ClaimTypes.Role);
    }

    /// <summary>Re-projects raw Entra claim types onto the standard <see cref="ClaimTypes"/> shape.</summary>
    internal static IEnumerable<Claim> NormalizeClaims(IEnumerable<Claim> source)
    {
        foreach (var claim in source)
        {
            var type = claim.Type switch
            {
                "oid"                           => ClaimTypes.NameIdentifier,
                AuthConstants.ObjectIdentifier   => ClaimTypes.NameIdentifier,
                "name"                           => ClaimTypes.Name,
                AuthConstants.Roles              => ClaimTypes.Role,
                _                                => claim.Type,
            };
            yield return new Claim(type, claim.Value);
        }
    }
}
