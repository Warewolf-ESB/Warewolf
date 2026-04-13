/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Stateless helper that extracts Entra (Azure AD) roles from either a
    /// validated EasyAuth principal header or a raw Entra Bearer token.
    ///
    /// <para>
    /// <b>Security note:</b> the RS256 signature of the raw Bearer token is NOT
    /// verified here.  This implementation relies on one of the following hosting
    /// guarantees:
    /// <list type="bullet">
    ///   <item>
    ///     Azure App Service / Functions <em>EasyAuth</em> is enabled and has already
    ///     validated the token — the result arrives via the
    ///     <c>X-MS-CLIENT-PRINCIPAL</c> request header (primary path).
    ///   </item>
    ///   <item>
    ///     A network-level policy (API Management, Application Gateway, etc.) rejects
    ///     invalid tokens before they reach this function — raw token path is used only
    ///     for local development where the calling client is trusted.
    ///   </item>
    /// </list>
    /// The <c>exp</c>, <c>iss</c>, and optionally <c>aud</c> / <c>tid</c> claims are
    /// validated locally in both paths.
    /// </para>
    ///
    /// <para>
    /// Role mapping: the extracted role names are matched against the
    /// <see cref="PermissionEntry.GroupName"/> values in <c>secure.config</c> using the
    /// same case-insensitive <see cref="PermissionChecker"/> logic as Windows groups.
    /// Administrators configure permissions by entering Entra app-role names (e.g.
    /// <c>"WorkflowExecutor"</c>) as group names in the Warewolf security settings.
    /// </para>
    /// </summary>
    internal static class EntraTokenValidator
    {
        // ── Claim key constants ───────────────────────────────────────────────────

        /// <summary>Standard JWT issuer claim.</summary>
        internal const string IssuerClaim = "iss";

        /// <summary>Standard JWT audience claim.</summary>
        internal const string AudienceClaim = "aud";

        /// <summary>Standard JWT expiry claim.</summary>
        internal const string ExpiryClaim = "exp";

        /// <summary>Tenant ID claim embedded in Entra v2 tokens.</summary>
        internal const string TenantIdClaim = "tid";

        /// <summary>App-roles claim array (<c>["RoleA","RoleB"]</c>).</summary>
        internal const string RolesClaim = "roles";

        /// <summary>Group-membership claim array (object-ID GUIDs or display names).</summary>
        internal const string GroupsClaim = "groups";

        // EasyAuth header + claim-type constants
        internal const string EasyAuthPrincipalHeader    = "X-MS-CLIENT-PRINCIPAL";
        internal const string EasyAuthAuthTypeField      = "auth_typ";
        internal const string EasyAuthClaimsField        = "claims";
        internal const string EasyAuthClaimTypeField     = "typ";
        internal const string EasyAuthClaimValueField    = "val";
        internal const string EasyAuthAadAuthType        = "aad";

        // Microsoft claim-type URI for role (used in EasyAuth claims array).
        internal const string MsRoleClaimType =
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        // ── Known Entra issuer prefixes ───────────────────────────────────────────

        static readonly string[] _entraIssuerPrefixes =
        [
            "https://login.microsoftonline.com/",
            "https://sts.windows.net/",
        ];

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the Entra roles for the incoming request, or <c>null</c> when the
        /// token / principal cannot be validated.
        ///
        /// <para>Resolution order:</para>
        /// <list type="number">
        ///   <item>
        ///     <c>X-MS-CLIENT-PRINCIPAL</c> header — set by Azure EasyAuth after it has
        ///     cryptographically validated the token.  Only accepted when the auth type is
        ///     <c>"aad"</c> (Azure AD / Entra).
        ///   </item>
        ///   <item>
        ///     Raw <c>Authorization: Bearer &lt;token&gt;</c> header — the JWT payload is
        ///     parsed directly.  The issuer must match a known Entra prefix.  See the class
        ///     summary for the security requirements of this path.
        ///   </item>
        /// </list>
        ///
        /// Returns <c>null</c> when the token is absent, malformed, expired, or fails
        /// the optional tenant / audience constraints.  Returns an empty list when the
        /// token is valid but carries no roles or groups.
        /// </summary>
        /// <param name="authorizationHeader">
        /// Value of the HTTP <c>Authorization</c> header.
        /// </param>
        /// <param name="easyAuthPrincipalHeader">
        /// Value of the <c>X-MS-CLIENT-PRINCIPAL</c> header set by Azure EasyAuth, or
        /// <c>null</c> when not present.
        /// </param>
        /// <param name="requiredTenantId">
        /// When non-empty, the token's <c>tid</c> claim (or issuer path segment) must
        /// match this value.  Sourced from <c>WAREWOLF_ENTRA_TENANT_ID</c>.
        /// </param>
        /// <param name="requiredAudience">
        /// When non-empty, the token's <c>aud</c> claim must equal this value.
        /// Sourced from <c>WAREWOLF_ENTRA_AUDIENCE</c>.
        /// </param>
        internal static IReadOnlyList<string>? GetRoles(
            string? authorizationHeader,
            string? easyAuthPrincipalHeader,
            string? requiredTenantId,
            string? requiredAudience)
        {
            // ── 1. EasyAuth path (preferred — signature already verified by platform) ──
            if (!string.IsNullOrWhiteSpace(easyAuthPrincipalHeader))
            {
                var easyAuthRoles = GetRolesFromEasyAuth(
                    easyAuthPrincipalHeader, requiredTenantId, requiredAudience);
                if (easyAuthRoles is not null)
                    return easyAuthRoles;
            }

            // ── 2. Raw Bearer token path (signature not verified — see class summary) ──
            return GetRolesFromToken(authorizationHeader, requiredTenantId, requiredAudience);
        }

        // ── EasyAuth path ─────────────────────────────────────────────────────────

        /// <summary>
        /// Parses the <c>X-MS-CLIENT-PRINCIPAL</c> header value and returns the
        /// Entra role names.  Returns <c>null</c> when the header is absent, malformed,
        /// or does not represent an Azure AD (Entra) authentication.
        /// </summary>
        internal static IReadOnlyList<string>? GetRolesFromEasyAuth(
            string? principalHeader,
            string? requiredTenantId  = null,
            string? requiredAudience  = null)
        {
            if (string.IsNullOrWhiteSpace(principalHeader))
                return null;

            try
            {
                var json      = Encoding.UTF8.GetString(Convert.FromBase64String(principalHeader));
                var principal = JObject.Parse(json);

                // Only accept Entra (AAD) authentication.
                var authTyp = principal[EasyAuthAuthTypeField]?.Value<string>();
                if (!string.Equals(authTyp, EasyAuthAadAuthType, StringComparison.OrdinalIgnoreCase))
                    return null;

                var claimsArray = principal[EasyAuthClaimsField] as JArray;
                if (claimsArray is null)
                    return Array.Empty<string>();

                var roles = new List<string>();
                foreach (var claim in claimsArray)
                {
                    var typ = claim[EasyAuthClaimTypeField]?.Value<string>();
                    var val = claim[EasyAuthClaimValueField]?.Value<string>();

                    if (string.IsNullOrEmpty(val))
                        continue;

                    if (string.Equals(typ, RolesClaim,      StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(typ, MsRoleClaimType, StringComparison.OrdinalIgnoreCase))
                    {
                        roles.Add(val);
                    }
                }

                return roles;
            }
            catch
            {
                return null;
            }
        }

        // ── Raw Bearer token path ─────────────────────────────────────────────────

        /// <summary>
        /// Parses a raw Entra Bearer token, validates its structural claims, and
        /// returns the role/group names.  The RS256 signature is <b>not</b> verified.
        ///
        /// Returns <c>null</c> when:
        /// <list type="bullet">
        ///   <item>the header or token is absent / malformed</item>
        ///   <item>the issuer does not match a known Entra prefix</item>
        ///   <item>the token has expired (<c>exp</c> claim)</item>
        ///   <item>the optional tenant / audience constraints are not met</item>
        /// </list>
        /// Returns an empty list when the token is valid but carries no roles or groups.
        /// </summary>
        internal static IReadOnlyList<string>? GetRolesFromToken(
            string? authorizationHeader,
            string? requiredTenantId = null,
            string? requiredAudience = null)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
                return null;

            var token = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorizationHeader[7..].Trim()
                : authorizationHeader.Trim();

            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            try
            {
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                var payload     = JObject.Parse(payloadJson);

                // ── Issuer must be a known Entra endpoint ─────────────────────────────
                var iss = payload[IssuerClaim]?.Value<string>();
                if (!IsEntraIssuer(iss))
                    return null;

                // ── Expiry ────────────────────────────────────────────────────────────
                var expToken = payload[ExpiryClaim];
                if (expToken is not null &&
                    expToken.Value<long>() < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    return null;

                // ── Optional tenant constraint ────────────────────────────────────────
                if (!string.IsNullOrWhiteSpace(requiredTenantId))
                {
                    var tid = payload[TenantIdClaim]?.Value<string>();
                    if (!IsTenantMatch(tid, iss, requiredTenantId))
                        return null;
                }

                // ── Optional audience constraint ──────────────────────────────────────
                if (!string.IsNullOrWhiteSpace(requiredAudience))
                {
                    var aud = payload[AudienceClaim]?.Value<string>();
                    if (!string.Equals(aud, requiredAudience, StringComparison.OrdinalIgnoreCase))
                        return null;
                }

                // ── Extract roles and groups ──────────────────────────────────────────
                var roles = new List<string>();

                if (payload[RolesClaim] is JArray rolesArray)
                    foreach (var r in rolesArray)
                    {
                        var v = r.Value<string>();
                        if (!string.IsNullOrEmpty(v))
                            roles.Add(v);
                    }

                if (payload[GroupsClaim] is JArray groupsArray)
                    foreach (var g in groupsArray)
                    {
                        var v = g.Value<string>();
                        if (!string.IsNullOrEmpty(v))
                            roles.Add(v);
                    }

                return roles;
            }
            catch
            {
                return null;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns <c>true</c> when <paramref name="issuer"/> starts with a known
        /// Microsoft Entra / Azure AD issuer prefix.
        /// </summary>
        static bool IsEntraIssuer(string? issuer)
        {
            if (string.IsNullOrWhiteSpace(issuer))
                return false;

            foreach (var prefix in _entraIssuerPrefixes)
                if (issuer.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Returns <c>true</c> when either the <c>tid</c> claim or the issuer URL
        /// contains <paramref name="requiredTenantId"/>.
        /// </summary>
        static bool IsTenantMatch(string? tid, string? issuer, string requiredTenantId)
        {
            if (!string.IsNullOrEmpty(tid) &&
                string.Equals(tid, requiredTenantId, StringComparison.OrdinalIgnoreCase))
                return true;

            // Fall back to checking the issuer URL for the tenant GUID path segment.
            return !string.IsNullOrEmpty(issuer) &&
                   issuer.Contains(requiredTenantId, StringComparison.OrdinalIgnoreCase);
        }

        static byte[] Base64UrlDecode(string s)
        {
            var padded = s.Replace('-', '+').Replace('_', '/');
            padded += (padded.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
            return Convert.FromBase64String(padded);
        }
    }
}
