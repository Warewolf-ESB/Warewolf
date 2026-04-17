/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Stateless HMAC-SHA256 JWT validator for the lightweight execution engine.
    ///
    /// Validates the token signature and expiry without any external NuGet dependency —
    /// JWT structure is well-defined (RFC 7519) so a focused implementation is preferred
    /// over pulling in the full <c>System.IdentityModel.Tokens.Jwt</c> stack.
    ///
    /// Token shape produced by <c>JwtManager.GenerateToken</c> in the Warewolf server:
    /// <code>
    ///   header.payload.signature   (all parts Base64Url-encoded)
    ///
    ///   payload JSON:
    ///   {
    ///     "http://schemas.microsoft.com/ws/2008/06/identity/claims/authentication":
    ///         "{\"UserGroups\": [\"Group1\", \"Group2\"]}",
    ///     "nbf": ...,
    ///     "exp": ...,
    ///     "iat": ...
    ///   }
    /// </code>
    /// </summary>
    internal static class JwtValidator
    {
        /// <summary>
        /// The claim key used by <c>JwtManager</c> to embed the JSON payload that contains
        /// <c>UserGroups</c>.  Mirrors <c>ClaimTypes.Authentication</c>.
        /// </summary>
        internal const string AuthClaimKey =
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/authentication";

        /// <summary>
        /// Validates a JWT bearer token against <paramref name="base64SecretKey"/> using
        /// HMAC-SHA256 and returns the list of user groups embedded in the token payload.
        ///
        /// Returns <c>null</c> when:
        /// <list type="bullet">
        ///   <item>the token or secret is absent</item>
        ///   <item>the token is malformed</item>
        ///   <item>the signature does not match</item>
        ///   <item>the token has expired (<c>exp</c> claim)</item>
        /// </list>
        ///
        /// Returns an empty list when the token is valid but carries no UserGroups.
        /// </summary>
        /// <param name="authorizationHeader">
        /// Value of the HTTP <c>Authorization</c> header, with or without the
        /// <c>Bearer </c> prefix.
        /// </param>
        /// <param name="base64SecretKey">
        /// Base-64–encoded HMAC-SHA256 key — the <c>SecretKey</c> from <c>secure.config</c>.
        /// </param>
        internal static IReadOnlyList<string>? GetUserGroups(
            string? authorizationHeader,
            string  base64SecretKey)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader) ||
                string.IsNullOrWhiteSpace(base64SecretKey))
                return null;

            // Strip "Bearer " prefix (case-insensitive).
            var token = authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorizationHeader[7..].Trim()
                : authorizationHeader.Trim();

            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            try
            {
                // ── 1. Verify HMAC-SHA256 signature ──────────────────────────────────
                var key          = Convert.FromBase64String(base64SecretKey);
                var signingInput = Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]);

                using var hmac        = new HMACSHA256(key);
                var computedSig       = Base64UrlEncode(hmac.ComputeHash(signingInput));

                if (!CryptographicEquals(computedSig, parts[2]))
                    return null;

                // ── 2. Decode and parse payload ───────────────────────────────────────
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                var payload     = JObject.Parse(payloadJson);

                // ── 3. Enforce expiry ─────────────────────────────────────────────────
                var expToken = payload["exp"];
                if (expToken is not null &&
                    expToken.Value<long>() < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    return null;

                // ── 4. Extract UserGroups from the authentication claim ────────────────
                var authClaimValue = payload[AuthClaimKey]?.Value<string>();
                if (string.IsNullOrWhiteSpace(authClaimValue))
                    return Array.Empty<string>();

                var authPayload = JObject.Parse(authClaimValue);
                var groups      = authPayload["UserGroups"]?.ToObject<List<string>>();
                return groups ?? (IReadOnlyList<string>)Array.Empty<string>();
            }
            catch
            {
                return null;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        static byte[] Base64UrlDecode(string s)
        {
            var padded = s.Replace('-', '+').Replace('_', '/');
            padded += (padded.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
            return Convert.FromBase64String(padded);
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing-oracle attacks.
        /// </summary>
        static bool CryptographicEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }
}
