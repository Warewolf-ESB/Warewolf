/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Net;
using System.Text.Json;
using Dev2.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

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
/// <remarks>Initialises the middleware.</remarks>
public sealed class EasyAuthRedirectMiddleware : IFunctionsWorkerMiddleware
{
    // True when running under func start / VS debugger (no EasyAuth platform available).
    // In development the full auth pipeline still runs — DebugPrincipalParser injects
    // the principal from DEBUG_PRINCIPAL_TOKEN so group/permission checks behave
    // identically to the cloud environment.
    private readonly bool _isDevelopment;

    // True when DEBUG_PRINCIPAL_TOKEN is configured. Only then does the development
    // bypass make sense — otherwise there is no principal to inject and the request
    // must be treated as unauthenticated (401/302) exactly as in production. This
    // matters for integration tests which run `func start` (defaults to Development)
    // but deliberately omit DEBUG_PRINCIPAL_TOKEN to exercise the real auth path.
    private readonly bool _hasDebugPrincipalToken;

    public EasyAuthRedirectMiddleware(HostEnvironmentConfig config)
    {
        _isDevelopment          = config.IsDevelopment;
        _hasDebugPrincipalToken = !string.IsNullOrWhiteSpace(config.DebugPrincipalToken);
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

        // Only enforce on protected routes (/secure/* and /services/*).
        // (AUTH-10) /services/* receives the same 401-or-redirect treatment as
        // /secure/* so API callers and browsers get a consistent experience.
        var isSecure   = path.StartsWith(AuthConstants.SecureRoutePrefix,   StringComparison.OrdinalIgnoreCase);
        var isServices = path.StartsWith(AuthConstants.ServicesRoutePrefix, StringComparison.OrdinalIgnoreCase);
        if (!isSecure && !isServices)
        {
            Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Non-secure route: {path}, passing through", executionId);
            await next(context);
            return;
        }

        // Secure or Public apis.json must be processed through secure or public route
        // apis.json discovery — always accessible without a token.
        // WorkflowAuthorizationMiddleware handles permission-filtering (empty list when no JWT).
        //if (path.EndsWith("apis.json", StringComparison.OrdinalIgnoreCase))
        //{
        //    await next(context);
        //    return;
        //}

        // Check for Easy Auth principal header (set by Azure after token validation)
        var hasPrincipalHeader = request.Headers
            .TryGetValues(AuthConstants.ClientPrincipalHeader, out var principalValues)
            && principalValues.Any(v => !string.IsNullOrWhiteSpace(v));

        // Check for raw Bearer token in Authorization header
        var hasAuthHeader = request.Headers
            .TryGetValues("Authorization", out var authValues)
            && authValues.Any(v => v.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase));

        var isBrowser = LooksLikeBrowserNavigation(request);

        Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Path={path}, HasPrincipalHeader={hasPrincipalHeader}, HasAuthHeader={hasAuthHeader}, IsBrowser={isBrowser}, IsDevelopment={_isDevelopment}", executionId);

        // In development with DEBUG_PRINCIPAL_TOKEN configured there is no EasyAuth
        // platform — pass every request through so DebugPrincipalParser can inject
        // the principal from the env var. Without a debug token there is nothing to
        // inject, so enforce the normal 401/302 behaviour.
        if (_isDevelopment && _hasDebugPrincipalToken && !hasPrincipalHeader && !hasAuthHeader)
        {
            Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Development environment with DEBUG_PRINCIPAL_TOKEN, no auth headers on {path} — delegating to DebugPrincipalParser", executionId);
            await next(context);
            return;
        }

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
            // Serialised rather than interpolated: the raw request path was written straight
            // into hand-built JSON, so a path containing a quote broke the document and
            // permitted response injection. Same three properties, same order, now escaped.
            await response.WriteStringAsync(JsonSerializer.Serialize(new
            {
                error   = "unauthorized",
                message = "A valid Bearer token is required.",
                path,
            }));

            context.GetInvocationResult().Value = response;
            return;
        }

        Dev2Logger.Debug($"EasyAuthRedirectMiddleware: Authenticated request to {path}, passing to next middleware", executionId);
        await next(context);
    }

    /// <summary>
    /// Heuristic: is this request a top-level browser navigation that should be
    /// redirected to the Easy Auth login page?  Marked <c>internal</c> for unit
    /// testing — pure function of request headers, no side effects.
    /// </summary>
    internal static bool LooksLikeBrowserNavigation(HttpRequestData req)
    {
        var accept = req.Headers.TryGetValues("Accept", out var a) ? a.FirstOrDefault() : null;
        var fetchMode = req.Headers.TryGetValues("Sec-Fetch-Mode", out var f) ? f.FirstOrDefault() : null;
        return (accept?.Contains("text/html", StringComparison.OrdinalIgnoreCase) ?? false)
            && !string.Equals(fetchMode, "cors", StringComparison.OrdinalIgnoreCase);
    }
}
