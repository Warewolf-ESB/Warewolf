/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth.Parsers;

/// <summary>
/// Builds a <see cref="WorkflowClaimsPrincipal"/> from the
/// <c>X-MS-CLIENT-PRINCIPAL</c> header injected by Azure Easy Auth.
/// The header is base64-encoded JSON whose signature has already been validated
/// by the Azure App Service authentication platform, so this parser performs
/// only structural decoding.
/// </summary>
public sealed class EasyAuthPrincipalParser : IPrincipalParser
{
    private readonly ILogger<EasyAuthPrincipalParser> _logger;

    /// <inheritdoc/>
    public string Name => "EasyAuth";

    /// <summary>Initialises the parser with a logger.</summary>
    public EasyAuthPrincipalParser(ILogger<EasyAuthPrincipalParser> logger) => _logger = logger;

    /// <inheritdoc/>
    public Task<WorkflowClaimsPrincipal?> TryParseAsync(HttpRequestData request, CancellationToken cancellationToken)
    {
        if (!request.Headers.TryGetValues(AuthConstants.ClientPrincipalHeader, out var values))
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);

        var encoded = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(encoded))
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            using var document = JsonDocument.Parse(json);
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

            return Task.FromResult<WorkflowClaimsPrincipal?>(new WorkflowClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to decode {Header} header — EasyAuth parser will yield to next strategy",
                AuthConstants.ClientPrincipalHeader);
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);
        }
    }

    private static string NormalizeClaimType(string typ) => typ switch
    {
        "http://schemas.microsoft.com/identity/claims/objectidentifier" => ClaimTypes.NameIdentifier,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"    => ClaimTypes.Name,
        "roles"                                                          => ClaimTypes.Role,
        _                                                                => typ,
    };
}
