/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Centralised string constants for the WwExecution auth pipeline.
/// All magic strings live here — never hardcode them in middleware or functions.
/// </summary>
public static class AuthConstants
{
    // ── Claim type URIs ───────────────────────────────────────────────────────

    /// <summary>Entra ID object identifier claim.</summary>
    public const string ObjectIdentifier  = "http://schemas.microsoft.com/identity/claims/objectidentifier";

    /// <summary>Standard name claim URI.</summary>
    public const string Name              = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

    /// <summary>App roles claim name as used in Entra ID v2.0 tokens.</summary>
    public const string Roles             = "roles";

    /// <summary>Preferred username claim (UPN) in v2.0 tokens.</summary>
    public const string PreferredUsername = "preferred_username";

    /// <summary>Identity provider claim — set to "aad" by Easy Auth.</summary>
    public const string IdentityProvider  = "idp";

    /// <summary>Scope claim — present in delegated (user impersonation) tokens only.</summary>
    public const string Scope             = "scp";

    // ── FunctionContext.Items keys ────────────────────────────────────────────

    /// <summary>
    /// Key used to store the <see cref="WorkflowClaimsPrincipal"/>
    /// in <see cref="Microsoft.Azure.Functions.Worker.FunctionContext.Items"/>.
    /// </summary>
    public const string PrincipalContextKey = "WorkflowClaimsPrincipal";

    /// <summary>
    /// Key set in <see cref="Microsoft.Azure.Functions.Worker.FunctionContext.Items"/>
    /// by <c>WorkflowAuthorizationMiddleware</c> when the Development-only bypass header
    /// (<c>X-WW-Bypass-Auth: local-dev-bypass</c>) is honoured.
    /// Functions must check this flag to skip their own redundant auth check when
    /// the middleware already granted access via the bypass.
    /// </summary>
    public const string DevBypassContextKey = "WwDevBypassActive";

    // ── Route prefixes ────────────────────────────────────────────────────────

    /// <summary>Prefix for anonymous public routes.</summary>
    public const string PublicRoutePrefix = "/public/";

    /// <summary>Prefix for authenticated secure routes (JWT / Easy Auth).</summary>
    public const string SecureRoutePrefix = "/secure/";

    /// <summary>
    /// Prefix for function-key authenticated service routes (<c>/services/{*name}</c>).
    /// Policy matching is enforced on these routes in the same way as
    /// <see cref="SecureRoutePrefix"/> when a <c>secure.config</c> is loaded.
    /// </summary>
    public const string ServicesRoutePrefix = "/services/";

    // ── Easy Auth HTTP headers ────────────────────────────────────────────────

    /// <summary>Base64-encoded JSON principal injected by Easy Auth.</summary>
    public const string ClientPrincipalHeader     = "X-MS-CLIENT-PRINCIPAL";

    /// <summary>Display name injected by Easy Auth.</summary>
    public const string ClientPrincipalNameHeader = "X-MS-CLIENT-PRINCIPAL-NAME";

    /// <summary>Identity provider injected by Easy Auth.</summary>
    public const string ClientPrincipalIdpHeader  = "X-MS-CLIENT-PRINCIPAL-IDP";

    // ── Diagnostic logging toggle ─────────────────────────────────────────────

    /// <summary>
    /// Enables verbose diagnostic logging across all auth middleware.
    /// Compile-time constant: <c>true</c> for DEBUG (testing/troubleshooting),
    /// <c>false</c> for RELEASE so the JIT eliminates guarded blocks entirely
    /// — zero runtime overhead in production.
    /// (AUTH-11)
    /// </summary>
#if DEBUG
    public const bool VerboseAuthLogging = true;
#else
    public const bool VerboseAuthLogging = false;
#endif

    /// <summary>
    /// Enables verbose <c>Console.WriteLine</c> logging for all auth middleware.
    /// Output goes to stdout, which can be streamed live via <c>az webapp log tail</c>.
    /// Compile-time constant: <c>true</c> for DEBUG, <c>false</c> for RELEASE.
    /// </summary>
#if DEBUG
    public const bool VerboseConsoleAuthLogging = true;
#else
    public const bool VerboseConsoleAuthLogging = false;
#endif
}
