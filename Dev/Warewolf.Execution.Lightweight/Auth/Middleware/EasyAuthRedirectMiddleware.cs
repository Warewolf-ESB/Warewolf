/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Auth.Middleware;

/// <summary>
/// First middleware in the pipeline.
/// Handles the Azure Policy constraint that forces Easy Auth into RedirectToLoginPage mode.
/// Instead of allowing Easy Auth to redirect unauthenticated requests (which breaks API clients),
/// this middleware intercepts them and returns a clean 401 JSON response.
///
/// <list type="bullet">
///   <item><c>/public/*</c> routes are passed straight through with no token check.</item>
///   <item><c>/secure/*</c> routes without a token receive a 401 immediately.</item>
///   <item><c>/secure/*</c> routes with a valid token header are passed to the next middleware.</item>
/// </list>
/// </summary>
public sealed class EasyAuthRedirectMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<EasyAuthRedirectMiddleware> _logger;

    /// <summary>Initialises the middleware with a logger.</summary>
    public EasyAuthRedirectMiddleware(ILogger<EasyAuthRedirectMiddleware> logger)
        => _logger = logger;

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            await next(context);
            return;
        }

        var path = request.Url.AbsolutePath;

        // Public routes bypass all auth checks
        if (path.StartsWith(AuthConstants.PublicRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Public route {Path} — bypassing auth check", path);
            await next(context);
            return;
        }

        // Only enforce on /secure/* routes
        if (!path.StartsWith(AuthConstants.SecureRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // Check for Easy Auth principal header (set by Azure after token validation)
        var hasPrincipalHeader = request.Headers
            .TryGetValues(AuthConstants.ClientPrincipalHeader, out var principalValues)
            && principalValues.Any(v => !string.IsNullOrWhiteSpace(v));

        // Check for raw Bearer token in Authorization header
        var hasAuthHeader = request.Headers
            .TryGetValues("Authorization", out var authValues)
            && authValues.Any(v => v.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase));

        if (!hasPrincipalHeader && !hasAuthHeader)
        {
            _logger.LogWarning(
                "Unauthenticated request to protected route {Path} — returning 401", path);

            var response = request.CreateResponse(HttpStatusCode.Unauthorized);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add("WWW-Authenticate", "Bearer realm=\"warewolf\"");
            await response.WriteStringAsync(
                $"{{\"error\":\"unauthorized\",\"message\":\"A valid Bearer token is required.\",\"path\":\"{path}\"}}");

            context.GetInvocationResult().Value = response;
            return;
        }

        _logger.LogDebug("Authenticated request to {Path} — passing to next middleware", path);
        await next(context);
    }
}
