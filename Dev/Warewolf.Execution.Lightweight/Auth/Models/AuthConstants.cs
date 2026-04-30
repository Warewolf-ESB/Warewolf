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

    // ── Route prefixes ────────────────────────────────────────────────────────

    /// <summary>Prefix for anonymous public routes.</summary>
    public const string PublicRoutePrefix = "/public/";

    /// <summary>Prefix for authenticated secure routes.</summary>
    public const string SecureRoutePrefix = "/secure/";

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
    /// Set to <c>true</c> during testing/debugging, <c>false</c> for production.
    /// Because this is a <c>const</c>, the compiler eliminates guarded code blocks
    /// entirely when set to <c>false</c> — zero runtime overhead in production.
    /// </summary>
    public const bool VerboseAuthLogging = true;

    /// <summary>
    /// Enables verbose Console.WriteLine logging for all auth middleware.
    /// Output goes to stdout, which can be streamed live via <c>az webapp log tail</c>.
    /// Set to <c>true</c> during testing/debugging, <c>false</c> for production.
    /// </summary>
    public const bool VerboseConsoleAuthLogging = true;
}
