/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Dev2.Common;
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
    /// <summary>Initialises the middleware.</summary>
    public ClaimsPrincipalBuilderMiddleware()
    {
    }

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        const string executionId = "ClaimsPrincipalBuilderMiddleware";

        var request = await context.GetHttpRequestDataAsync();

        if (request is not null)
        {
            var principal = BuildPrincipal(request);
            context.Items[AuthConstants.PrincipalContextKey] = principal;

            Dev2Logger.Debug(
                $"Principal built: User={principal.UserName} Authenticated={principal.Identity?.IsAuthenticated} Groups=[{string.Join(", ", principal.Groups)}] Permissions={principal.Permissions.Count}",
                executionId);
        }

        await next(context);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private WorkflowClaimsPrincipal BuildPrincipal(HttpRequestData request)
    {
        if (!request.Headers.TryGetValues(AuthConstants.ClientPrincipalHeader, out var headerValues))
        {
            return WorkflowClaimsPrincipal.Anonymous();
        }

        var encoded = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(encoded))
        {
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
            Dev2Logger.Warn(
                $"Failed to decode {AuthConstants.ClientPrincipalHeader} header — using anonymous principal",
                ex,
                "ClaimsPrincipalBuilderMiddleware");
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
