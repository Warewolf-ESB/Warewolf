/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Warewolf.Execution.Lightweight.Auth.Models;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;

namespace Warewolf.Execution.Lightweight.Auth.Parsers;

/// <summary>
/// Validates an <c>Authorization: Bearer &lt;jwt&gt;</c> header against Microsoft Entra
/// using the published OIDC metadata for the configured tenant (RS256 signing keys
/// are fetched and cached automatically by <see cref="ConfigurationManager{T}"/>).
///
/// On success an authenticated <see cref="WorkflowClaimsPrincipal"/> is produced
/// with claims normalised to the same shape as the Easy Auth pipeline, so the
/// downstream <see cref="Middleware.WorkflowAuthorizationMiddleware"/> can apply
/// the same group / permission checks regardless of which authentication path
/// the caller used.
/// </summary>
public sealed class BearerTokenPrincipalParser : IPrincipalParser
{
    private const string AuthenticationType = "Bearer";
    private const string BearerScheme       = "Bearer ";

    private readonly EntraAuthOptions _options;
    private readonly ILogger<BearerTokenPrincipalParser> _logger;
    private readonly Lazy<ConfigurationManager<OpenIdConnectConfiguration>?> _configManager;
    private readonly JwtSecurityTokenHandler _handler = new() { MapInboundClaims = false };

    /// <inheritdoc/>
    public string Name => "Bearer";

    /// <summary>Initialises the parser with Entra options and a logger.</summary>
    public BearerTokenPrincipalParser(
        EntraAuthOptions options,
        ILogger<BearerTokenPrincipalParser> logger)
    {
        _options = options;
        _logger  = logger;

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

    /// <inheritdoc/>
    public async Task<WorkflowClaimsPrincipal?> TryParseAsync(
        HttpRequestData request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.TryGetValues("Authorization", out var headerValues))
            return null;

        var raw = headerValues.FirstOrDefault(v =>
            !string.IsNullOrWhiteSpace(v) &&
            v.StartsWith(BearerScheme, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(raw))
            return null;

        if (!_options.IsEnabled)
        {
            _logger.LogWarning(
                "Bearer token received but Entra auth is not configured — set " +
                "WAREWOLF_ENTRA_TENANT_ID and WAREWOLF_ENTRA_AUDIENCE to enable token validation");
            return null;
        }

        var token = raw[BearerScheme.Length..].Trim();

        try
        {
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

            var result = _handler.ValidateToken(token, parameters, out _);

            // Re-project claims into the same normalised shape produced by the EasyAuth
            // parser so downstream authorization is identical for both auth paths.
            var identity = new ClaimsIdentity(
                NormalizeClaims(result.Claims),
                authenticationType: AuthenticationType,
                nameType:           ClaimTypes.Name,
                roleType:           ClaimTypes.Role);

            _logger.LogDebug("Bearer token validated for {User}", identity.Name ?? "(unknown)");
            return new WorkflowClaimsPrincipal(identity);
        }
        catch (SecurityTokenExpiredException stee)
        {
            _logger.LogWarning(stee, "Bearer token validation failed - token expired — yielding to next strategy");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Bearer token validation failed — yielding to next strategy");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unexpected error validating Bearer token — yielding to next strategy");
            return null;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static IEnumerable<Claim> NormalizeClaims(IEnumerable<Claim> source)
    {
        foreach (var claim in source)
        {
            var type = claim.Type switch
            {
                "oid"              => ClaimTypes.NameIdentifier,
                AuthConstants.ObjectIdentifier => ClaimTypes.NameIdentifier,
                "name"             => ClaimTypes.Name,
                AuthConstants.PreferredUsername => ClaimTypes.Name,
                AuthConstants.Roles            => ClaimTypes.Role,
                _                  => claim.Type,
            };
            yield return new Claim(type, claim.Value);
        }
    }
}
