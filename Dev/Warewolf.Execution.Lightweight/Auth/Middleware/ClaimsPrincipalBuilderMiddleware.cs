/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth.Middleware;

/// <summary>
/// Second middleware in the pipeline.
/// Reads the X-MS-CLIENT-PRINCIPAL header injected by Azure Easy Auth,
/// decodes it from base64 JSON, and builds a <see cref="WorkflowClaimsPrincipal"/>
/// stored in <see cref="FunctionContext.Items"/> for downstream use.
/// Stores an anonymous principal if the header is absent or invalid — never throws.
/// </summary>
public sealed class ClaimsPrincipalBuilderMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ClaimsPrincipalBuilderMiddleware> _logger;

    /// <summary>Initialises the middleware with a logger.</summary>
    public ClaimsPrincipalBuilderMiddleware(ILogger<ClaimsPrincipalBuilderMiddleware> logger)
        => _logger = logger;

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is not null)
        {
            var principal = BuildPrincipal(request);
            context.Items[AuthConstants.PrincipalContextKey] = principal;

            _logger.LogDebug(
                "Principal built: User={User} Authenticated={Auth} Groups=[{Groups}] Permissions={PermCount}",
                principal.UserName,
                principal.Identity?.IsAuthenticated,
                string.Join(", ", principal.Groups),
                principal.Permissions.Count);

            if (AuthConstants.VerboseAuthLogging)
            {
                try
                {
                    if (AuthConstants.VerboseConsoleAuthLogging)
                    {
                        Console.WriteLine($"WorkflowClaimsPrincipal=>: {principal.ToString()}");
                    }

                    _logger.LogInformation("WorkflowClaimsPrincipal=>" + principal.ToString());

                    var identity = principal.Identity as ClaimsIdentity;
                    var roles = string.Join(", ", principal.Identities
                        .SelectMany(i => i.Claims)
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value));
                    var groups = string.Join(", ", principal.Groups);
                    var perms = string.Join(", ", principal.Permissions);
                    var claimCount = identity?.Claims.Count() ?? 0;

                    _logger.LogInformation(
                        "[AuthDiag] ClaimsPrincipal: User={User} AuthType={AuthType} IsAuthenticated={IsAuth} " +
                        "Roles=[{Roles}] Groups=[{Groups}] Permissions=[{Perms}] ClaimCount={ClaimCount}",
                        principal.UserName,
                        identity?.AuthenticationType ?? "(none)",
                        identity?.IsAuthenticated ?? false,
                        roles, groups, perms, claimCount);

                    if (AuthConstants.VerboseConsoleAuthLogging)
                    {
                        Console.WriteLine($"[AuthDiag] ClaimsPrincipal: User={principal.UserName} AuthType={identity?.AuthenticationType ?? "(none)"} IsAuthenticated={identity?.IsAuthenticated ?? false} Roles=[{roles}] Groups=[{groups}] Permissions=[{perms}] ClaimCount={claimCount}");
                    }

                    foreach (var claim in identity?.Claims ?? Enumerable.Empty<Claim>())
                    {
                        _logger.LogDebug("[AuthDiag] Claim: Type={Type} Value={Value}", claim.Type, claim.Value);
                        if (AuthConstants.VerboseConsoleAuthLogging)
                            Console.WriteLine($"[AuthDiag] Claim: Type={claim.Type} Value={claim.Value}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AuthDiag] Failed to log principal details — diagnostic logging error (non-fatal)");
                }
            }
        }

        await next(context);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private WorkflowClaimsPrincipal BuildPrincipal(HttpRequestData request)
    {
        if (!request.Headers.TryGetValues(AuthConstants.ClientPrincipalHeader, out var headerValues))
        {
            if (AuthConstants.VerboseAuthLogging)
            {
                try
                {
                    _logger.LogInformation("[AuthDiag] {Header} header not present — returning Anonymous principal", AuthConstants.ClientPrincipalHeader);
                    if (AuthConstants.VerboseConsoleAuthLogging)
                        Console.WriteLine($"[AuthDiag] {AuthConstants.ClientPrincipalHeader} header not present — returning Anonymous principal");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AuthDiag] Diagnostic logging error (non-fatal)");
                }
            }
            return WorkflowClaimsPrincipal.Anonymous();
        }

        var encoded = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(encoded))
        {
            if (AuthConstants.VerboseAuthLogging)
            {
                try
                {
                    _logger.LogInformation("[AuthDiag] {Header} header present but empty — returning Anonymous principal", AuthConstants.ClientPrincipalHeader);
                    if (AuthConstants.VerboseConsoleAuthLogging)
                        Console.WriteLine($"[AuthDiag] {AuthConstants.ClientPrincipalHeader} header present but empty — returning Anonymous principal");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AuthDiag] Diagnostic logging error (non-fatal)");
                }
            }
            return WorkflowClaimsPrincipal.Anonymous();
        }

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var claims = new List<Claim>();

            if (root.TryGetProperty("auth_typ", out var authTyp))
                claims.Add(new Claim(AuthConstants.IdentityProvider, authTyp.GetString() ?? string.Empty));

            if (root.TryGetProperty("claims", out var claimsArray))
            {
                foreach (var element in claimsArray.EnumerateArray())
                {
                    var typ = element.TryGetProperty("typ", out var t) ? t.GetString() : null;
                    var val = element.TryGetProperty("val", out var v) ? v.GetString() : null;

                    if (!string.IsNullOrEmpty(typ) && val is not null)
                        claims.Add(new Claim(NormalizeClaimType(typ), val));
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "EasyAuth",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            return new WorkflowClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to decode {Header} header — using anonymous principal",
                AuthConstants.ClientPrincipalHeader);
            return WorkflowClaimsPrincipal.Anonymous();
        }
    }

    private static string NormalizeClaimType(string typ) => typ switch
    {
        "http://schemas.microsoft.com/identity/claims/objectidentifier" => ClaimTypes.NameIdentifier,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
        "roles" => ClaimTypes.Role,
        _ => typ,
    };
}
