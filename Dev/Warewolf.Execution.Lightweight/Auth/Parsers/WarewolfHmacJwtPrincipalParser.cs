/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Auth.Parsers;

/// <summary>
/// Validates the Warewolf HMAC-SHA256 <c>Authorization: Bearer</c> JWT issued by
/// the full Warewolf server's <c>JwtManager.GenerateToken</c> against the
/// <c>SecretKey</c> currently loaded from <c>secure.config</c>.
///
/// <para>
/// Sits in the parser chain <em>before</em> <see cref="BearerTokenPrincipalParser"/>
/// (which expects an Entra RS256 token).  Warewolf-signed tokens use a symmetric
/// key shared with the server bin and cannot be validated by the OIDC metadata
/// pipeline, so without this parser the middleware would always stamp an
/// anonymous principal and <see cref="Middleware.WorkflowAuthorizationMiddleware"/>
/// would return 401 for every <c>/Secure/*</c> execution route.
/// </para>
///
/// <para>
/// On success the parser projects each <c>UserGroups</c> entry from the token
/// payload as a <see cref="ClaimTypes.Role"/> claim, matching the shape produced
/// by the Easy Auth and Entra parsers so downstream group lookups in
/// <see cref="WorkflowClaimsPrincipal.Groups"/> are uniform across all auth paths.
/// </para>
/// </summary>
public sealed class WarewolfHmacJwtPrincipalParser : IPrincipalParser
{
    private const string AuthenticationType = "WarewolfHmac";

    private readonly ILogger<WarewolfHmacJwtPrincipalParser> _logger;

    /// <inheritdoc/>
    public string Name => "WarewolfHmac";

    /// <summary>Initialises the parser with a logger.</summary>
    public WarewolfHmacJwtPrincipalParser(ILogger<WarewolfHmacJwtPrincipalParser> logger)
        => _logger = logger;

    /// <inheritdoc/>
    public Task<WorkflowClaimsPrincipal?> TryParseAsync(
        HttpRequestData request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.TryGetValues("Authorization", out var headerValues))
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);

        var authHeader = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader))
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);

        // Read the live config (SecureConfigWatcher hot-reloads it) so a rotated
        // SecretKey is picked up without restarting the host.
        var config = SecureConfigLoader.Config;
        if (!config.IsLoaded || string.IsNullOrEmpty(config.SecretKey))
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);

        var groups = JwtValidator.GetUserGroups(authHeader, config.SecretKey);
        if (groups is null)
        {
            // Not a Warewolf-signed token (or signature/expiry invalid) — yield
            // to the next parser so the Entra RS256 path can have a go.
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);
        }

        var claims = new List<Claim>(groups.Count);
        foreach (var group in groups)
            if (!string.IsNullOrWhiteSpace(group))
                claims.Add(new Claim(ClaimTypes.Role, group));

        var identity = new ClaimsIdentity(
            claims,
            authenticationType: AuthenticationType,
            nameType:           ClaimTypes.Name,
            roleType:           ClaimTypes.Role);

        // Group membership identifies the caller — never logged, at any level.
        _logger.LogDebug("Warewolf HMAC JWT validated; groupCount={GroupCount}", groups.Count);

        return Task.FromResult<WorkflowClaimsPrincipal?>(new WorkflowClaimsPrincipal(identity));
    }
}
