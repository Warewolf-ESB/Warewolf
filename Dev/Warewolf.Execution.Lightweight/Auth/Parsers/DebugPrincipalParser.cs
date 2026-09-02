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
/// <b>Development-only</b> principal parser that injects a fixed, pre-authenticated
/// <see cref="WorkflowClaimsPrincipal"/> sourced from the <c>principalToken</c> field of the
/// <c>WAREWOLF_DEBUG_CONFIG</c> JSON app setting (WOLF-8516; formerly the standalone
/// <c>DEBUG_PRINCIPAL_TOKEN</c> env var) rather than from the inbound HTTP request.
///
/// <para>
/// <b>Purpose.</b>  When running the Azure Function locally (e.g. under
/// <c>func start</c> or the VS debugger) there is no Azure App Service EasyAuth
/// platform to inject the <c>X-MS-CLIENT-PRINCIPAL</c> header.  This parser
/// bridges that gap by letting a developer pin a real Entra-issued token obtained
/// from a live deployment's <c>/.auth/me</c> endpoint so that group membership,
/// secure.config permission matching, and apis.json filtering all behave identically
/// to the cloud environment.
/// </para>
///
/// <para>
/// <b>How to obtain the token value.</b>
/// <list type="number">
///   <item>
///     Sign in to the deployed App Service (e.g.
///     <c>https://wwexecutiondev.azurewebsites.net/.auth/me</c>) using the
///     same identity you want to debug as.
///   </item>
///   <item>
///     The response is a JSON array.  Copy the <c>clientPrincipal</c> object
///     from the first element — it has the shape:
///     <code>
///     {
///       "auth_typ": "aad",
///       "name_typ": "...",
///       "role_typ": "...",
///       "claims": [ { "typ": "...", "val": "..." }, ... ]
///     }
///     </code>
///   </item>
///   <item>
///     Base64-encode that JSON string (UTF-8, no line breaks) and paste the
///     result as the <c>principalToken</c> field of <c>WAREWOLF_DEBUG_CONFIG</c> in
///     <c>local.settings.json</c>:
///     <code>
///     # PowerShell helper:
///     [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($clientPrincipalJson))
///     </code>
///   </item>
/// </list>
/// </para>
///
/// <para>
/// <b>Security.</b>  This parser is registered in the <see cref="IPrincipalParser"/>
/// chain <em>only</em> when <c>IsDevelopment=true</c>
/// (<see cref="Infrastructure.HostEnvironmentConfig.IsDevelopment"/>).  It is
/// never active in staging or production environments.
/// </para>
/// </summary>
public sealed class DebugPrincipalParser : IPrincipalParser
{
    // WOLF-8516: label used only in log messages below — the actual value now comes from the
    // principalToken field of WAREWOLF_DEBUG_CONFIG (see Infrastructure.DebugConfig), not this
    // name directly.
    internal const string EnvVarName = "WAREWOLF_DEBUG_CONFIG.principalToken";

    private readonly string? _encodedToken;
    private readonly ILogger<DebugPrincipalParser> _logger;

    // Cached result: parse the env var once at construction time (it never changes
    // during a debug session) so there is zero per-request overhead.
    private readonly WorkflowClaimsPrincipal? _cachedPrincipal;

    /// <inheritdoc/>
    public string Name => "DebugPrincipal";

    /// <summary>
    /// Initialises the parser with the base64-encoded principal token and a logger.
    /// <paramref name="encodedToken"/> is sourced from
    /// <see cref="Infrastructure.HostEnvironmentConfig.DebugPrincipalToken"/> which reads
    /// <c>WAREWOLF_DEBUG_CONFIG</c>'s <c>principalToken</c> field only in development
    /// environments.
    /// </summary>
    public DebugPrincipalParser(string? encodedToken, ILogger<DebugPrincipalParser> logger)
    {
        _encodedToken = encodedToken;
        _logger       = logger;

        if (!string.IsNullOrWhiteSpace(encodedToken))
        {
            _cachedPrincipal = TryBuildPrincipal(encodedToken, logger);

            if (_cachedPrincipal?.Identity?.IsAuthenticated == true)
            {
                _logger.LogInformation(
                    "[DebugPrincipal] Loaded fixed debug principal: User={User} Groups=[{Groups}]",
                    _cachedPrincipal.UserName,
                    string.Join(", ", _cachedPrincipal.Groups));
            }
            else
            {
                _logger.LogWarning(
                    "[DebugPrincipal] {EnvVar} is set but could not produce an authenticated principal. " +
                    "Ensure the value is the base64-encoded clientPrincipal JSON from /.auth/me.",
                    EnvVarName);
            }
        }
        else
        {
            _logger.LogDebug("[DebugPrincipal] {EnvVar} is not set — parser inactive.", EnvVarName);
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The inbound <paramref name="request"/> is intentionally ignored — the principal
    /// comes entirely from the env var so every local request is authenticated as the
    /// same debug identity.
    /// </remarks>
    public Task<WorkflowClaimsPrincipal?> TryParseAsync(
        HttpRequestData   request,
        CancellationToken cancellationToken)
        => Task.FromResult(_cachedPrincipal?.Identity?.IsAuthenticated == true
            ? _cachedPrincipal
            : null);

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Decodes the base64-encoded <c>X-MS-CLIENT-PRINCIPAL</c> JSON and builds a
    /// <see cref="WorkflowClaimsPrincipal"/>.  Uses the exact same parsing logic as
    /// <see cref="EasyAuthPrincipalParser"/> so role/claim extraction is identical
    /// to the production EasyAuth path.
    /// </summary>
    private static WorkflowClaimsPrincipal? TryBuildPrincipal(
        string                           encoded,
        ILogger<DebugPrincipalParser>    logger)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // ── Detect input format and normalise to a (providerName, claimsArray) pair ──
            //
            // Three supported shapes:
            //
            //  (A) EasyAuth v2 — full /.auth/me response array (pasted directly):
            //        [ { "access_token": "...", "provider_name": "aad",
            //            "user_claims": [ { "typ": "...", "val": "..." }, ... ],
            //            "user_id": "..." } ]
            //
            //  (B) EasyAuth v1 — /.auth/me with clientPrincipal wrapper:
            //        [ { "clientPrincipal": { "auth_typ": "aad", "claims": [...] } } ]
            //
            //  (C) Raw X-MS-CLIENT-PRINCIPAL header value:
            //        { "auth_typ": "aad", "claims": [ { "typ": "...", "val": "..." }, ... ] }

            string?     providerName;
            JsonElement claimsArray;

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var first = root[0];

                if (first.TryGetProperty("user_claims", out claimsArray))
                {
                    // ── (A) EasyAuth v2 ──────────────────────────────────────────
                    providerName = first.TryGetProperty("provider_name", out var pn)
                        ? pn.GetString() : null;
                }
                else if (first.TryGetProperty("clientPrincipal", out var cp) &&
                         cp.TryGetProperty("claims", out claimsArray))
                {
                    // ── (B) EasyAuth v1 ──────────────────────────────────────────
                    providerName = cp.TryGetProperty("auth_typ", out var at)
                        ? at.GetString() : null;
                }
                else
                {
                    logger.LogWarning(
                        "[DebugPrincipal] Unrecognised /.auth/me array format in {EnvVar}.",
                        EnvVarName);
                    return null;
                }
            }
            else if (root.ValueKind == JsonValueKind.Object &&
                     root.TryGetProperty("claims", out claimsArray))
            {
                // ── (C) Raw X-MS-CLIENT-PRINCIPAL ────────────────────────────────
                providerName = root.TryGetProperty("auth_typ", out var at)
                    ? at.GetString() : null;
            }
            else
            {
                logger.LogWarning(
                    "[DebugPrincipal] Unrecognised JSON format in {EnvVar}. " +
                    "Expected a /.auth/me array or an X-MS-CLIENT-PRINCIPAL object.",
                    EnvVarName);
                return null;
            }

            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(providerName))
                claims.Add(new Claim(AuthConstants.IdentityProvider, providerName));

            foreach (var element in claimsArray.EnumerateArray())
            {
                var typ = element.TryGetProperty("typ", out var t) ? t.GetString() : null;
                var val = element.TryGetProperty("val", out var v) ? v.GetString() : null;
                if (!string.IsNullOrEmpty(typ) && val is not null)
                    claims.Add(new Claim(NormalizeClaimType(typ), val));
            }

            var identity = new ClaimsIdentity(
                claims,
                authenticationType: "DebugEasyAuth",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            return new WorkflowClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[DebugPrincipal] Failed to decode {EnvVar} — check that the value is " +
                "valid base64-encoded UTF-8 JSON from /.auth/me",
                EnvVarName);
            return null;
        }
    }

    /// <summary>
    /// Normalises claim type URIs to the short-form names expected by
    /// <see cref="WorkflowClaimsPrincipal"/> — mirrors
    /// <see cref="EasyAuthPrincipalParser"/>'s own normalisation so role extraction
    /// is consistent across both parsers.
    /// </summary>
    private static string NormalizeClaimType(string typ) => typ switch
    {
        "http://schemas.microsoft.com/identity/claims/objectidentifier" => ClaimTypes.NameIdentifier,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"    => ClaimTypes.Name,
        "roles"                                                          => ClaimTypes.Role,
        _                                                                => typ,
    };
}
