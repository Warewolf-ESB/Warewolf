/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Net;
using Dev2.Common;
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
    /// <summary>Initialises the middleware.</summary>
    public EasyAuthRedirectMiddleware()
    {
    }

    /// <inheritdoc/>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        const string executionId = "EasyAuthRedirectMiddleware";

        var request = await context.GetHttpRequestDataAsync();

        if (request is null)
        {
            Dev2Logger.Debug("EasyAuthRedirectMiddleware: No HTTP request data, passing to next middleware", executionId);
            await next(context);
            return;
        }

        var path = request.Url.AbsolutePath;

        // Public routes bypass all auth checks
        if (path.StartsWith(AuthConstants.PublicRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Public route detected: {path}, bypassing auth check", executionId);
            await next(context);
            return;
        }

        // Only enforce on /secure/* routes
        if (!path.StartsWith(AuthConstants.SecureRoutePrefix, StringComparison.OrdinalIgnoreCase))
        {
            Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Non-secure route: {path}, passing through", executionId);
            await next(context);
            return;
        }

        // apis.json discovery — always accessible without a token.
        // WorkflowAuthorizationMiddleware handles permission-filtering (empty list when no JWT).
        if (path.EndsWith("apis.json", StringComparison.OrdinalIgnoreCase))
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

        var isBrowser = LooksLikeBrowserNavigation(request);

        Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Path={path}, HasPrincipalHeader={hasPrincipalHeader}, HasAuthHeader={hasAuthHeader}, IsBrowser={isBrowser}", executionId);

        if (!hasPrincipalHeader && !hasAuthHeader)
        {
            if (isBrowser)
            {
                var redirect = $"/.auth/login/aad?post_login_redirect_uri={Uri.EscapeDataString(path + request.Url.Query)}";

                Dev2Logger.Info($"EasyAuthRedirectMiddleware: Browser navigation detected, redirecting to: {redirect}", executionId);

                var resp302 = request.CreateResponse(HttpStatusCode.Redirect);
                resp302.Headers.Add("Location", redirect);
                context.GetInvocationResult().Value = resp302;
                return;
            }

            Dev2Logger.Warn($"EasyAuthRedirectMiddleware: Unauthenticated request to protected route: {path}, returning 401", executionId);

            var response = request.CreateResponse(HttpStatusCode.Unauthorized);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add("WWW-Authenticate", "Bearer realm=\"warewolf\"");
            await response.WriteStringAsync(
                $"{{\"error\":\"unauthorized\",\"message\":\"A valid Bearer token is required.\",\"path\":\"{path}\"}}");

            context.GetInvocationResult().Value = response;
            return;
        }

        Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Authenticated request to {path}, passing to next middleware", executionId);
        await next(context);
    }

    static bool LooksLikeBrowserNavigation(HttpRequestData req)
    {
        var accept = req.Headers.TryGetValues("Accept", out var a) ? a.FirstOrDefault() : null;
        var fetchMode = req.Headers.TryGetValues("Sec-Fetch-Mode", out var f) ? f.FirstOrDefault() : null;
        return (accept?.Contains("text/html", StringComparison.OrdinalIgnoreCase) ?? false)
            && !string.Equals(fetchMode, "cors", StringComparison.OrdinalIgnoreCase);
    }
}
