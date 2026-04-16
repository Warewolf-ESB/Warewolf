/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Stateless HMAC-SHA256 JWT generator for the lightweight execution engine.
    ///
    /// Produces tokens in the same format expected by <see cref="JwtValidator"/>:
    /// <code>
    ///   header.payload.signature   (all parts Base64Url-encoded)
    ///
    ///   payload JSON:
    ///   {
    ///     "http://schemas.microsoft.com/ws/2008/06/identity/claims/authentication":
    ///         "{\"UserGroups\":[\"Group1\",\"Group2\"]}",
    ///     "nbf": &lt;unix&gt;,
    ///     "exp": &lt;unix + expireMinutes&gt;,
    ///     "iat": &lt;unix&gt;
    ///   }
    /// </code>
    ///
    /// No <c>System.IdentityModel.Tokens.Jwt</c> dependency — the JWT structure is
    /// straightforward (RFC 7519) and a focused implementation avoids pulling in the
    /// full server's JWT stack.
    /// </summary>
    internal static class JwtGenerator
    {
        private const string HeaderJson = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";

        /// <summary>
        /// Generates a signed JWT token embedding <paramref name="userGroups"/> as a
        /// string array in the authentication claim.
        /// </summary>
        /// <param name="userGroups">
        /// The groups to embed.  Each entry becomes a plain string in the
        /// <c>UserGroups</c> JSON array inside the authentication claim.
        /// </param>
        /// <param name="base64SecretKey">
        /// Base-64–encoded HMAC-SHA256 signing key — the <c>SecretKey</c> from
        /// <c>secure.config</c>.
        /// </param>
        /// <param name="expireMinutes">Token lifetime in minutes (default 20).</param>
        /// <returns>A compact JWT string (<c>header.payload.signature</c>).</returns>
        internal static string GenerateToken(
            IReadOnlyList<string> userGroups,
            string base64SecretKey,
            int expireMinutes = 20)
        {
            var now     = DateTimeOffset.UtcNow;
            var nbf     = now.ToUnixTimeSeconds();
            var exp     = now.AddMinutes(expireMinutes).ToUnixTimeSeconds();

            // Build the inner auth-claim value: {"UserGroups":["Group1","Group2"]}
            var authClaimValue = JsonConvert.SerializeObject(new { UserGroups = userGroups });

            // Build the JWT payload object
            var payloadObj = new Dictionary<string, object>
            {
                [JwtValidator.AuthClaimKey] = authClaimValue,
                ["nbf"] = nbf,
                ["exp"] = exp,
                ["iat"] = nbf,
            };

            var headerEncoded  = Base64UrlEncode(Encoding.UTF8.GetBytes(HeaderJson));
            var payloadEncoded = Base64UrlEncode(
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payloadObj)));

            var signingInput = headerEncoded + "." + payloadEncoded;
            var key          = Convert.FromBase64String(base64SecretKey);

            using var hmac       = new HMACSHA256(key);
            var signature        = Base64UrlEncode(
                hmac.ComputeHash(Encoding.ASCII.GetBytes(signingInput)));

            return signingInput + "." + signature;
        }

        static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
    }
}
