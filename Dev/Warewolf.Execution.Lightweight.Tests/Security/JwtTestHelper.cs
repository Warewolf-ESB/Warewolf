/*
 * Test helper — NOT production code.
 *
 * Produces RFC 7519 JWT tokens signed with HMAC-SHA256.
 * The payload shape mirrors what JwtManager.GenerateToken emits so that
 * JwtValidator.GetUserGroups can round-trip them in tests.
 *
 * Token structure:
 *   header   = {"alg":"HS256","typ":"JWT"}
 *   payload  = {
 *     "http://schemas.microsoft.com/ws/2008/06/identity/claims/authentication":
 *         "{\"UserGroups\":[\"GroupA\",\"GroupB\"]}",
 *     "nbf": <unix>,
 *     "exp": <unix>,
 *     "iat": <unix>
 *   }
 *   signature = HMAC-SHA256( base64url(header) + "." + base64url(payload), key )
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    internal static class JwtTestHelper
    {
        static readonly string _headerEncoded =
            Base64UrlEncode(Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}"""));

        // ── Token factories ───────────────────────────────────────────────────────

        /// <summary>
        /// Generates a valid JWT for the given user groups, signed with
        /// <paramref name="base64SecretKey"/>.
        /// </summary>
        internal static string ValidToken(string base64SecretKey, params string[] userGroups) =>
            BuildToken(base64SecretKey, userGroups, expireMinutes: 20);

        /// <summary>
        /// Generates an already-expired token (exp = 10 minutes in the past).
        /// <see cref="JwtValidator.GetUserGroups"/> must return <c>null</c> for this.
        /// </summary>
        internal static string ExpiredToken(string base64SecretKey, params string[] userGroups) =>
            BuildToken(base64SecretKey, userGroups, expireMinutes: -10);

        /// <summary>
        /// Generates a token with a tampered payload: the signature still covers the
        /// original payload, so the signature verification must fail.
        /// </summary>
        internal static string TamperedPayloadToken(string base64SecretKey, params string[] userGroups)
        {
            var token  = ValidToken(base64SecretKey, userGroups);
            var parts  = token.Split('.');

            // Flip one bit in the payload by appending a char.
            var tampered = parts[1] + "X";
            return parts[0] + "." + tampered + "." + parts[2];
        }

        /// <summary>
        /// Generates a token whose signature has been replaced with random bytes.
        /// The payload is intact but the signature check must fail.
        /// </summary>
        internal static string BadSignatureToken(string base64SecretKey, params string[] userGroups)
        {
            var token = ValidToken(base64SecretKey, userGroups);
            var parts = token.Split('.');

            var randomSig = new byte[32];
            RandomNumberGenerator.Fill(randomSig);
            return parts[0] + "." + parts[1] + "." + Base64UrlEncode(randomSig);
        }

        /// <summary>
        /// Generates a token signed with a <em>different</em> key than the one
        /// the validator will use.
        /// </summary>
        internal static string WrongKeyToken(string validatorKey, params string[] userGroups)
        {
            using var hmac       = new HMACSHA256();
            var differentKey     = Convert.ToBase64String(hmac.Key);
            return BuildToken(differentKey, userGroups, expireMinutes: 20);
        }

        // ── Core builder ─────────────────────────────────────────────────────────

        static string BuildToken(string base64SecretKey, string[] userGroups, int expireMinutes)
        {
            var now    = DateTimeOffset.UtcNow;
            var nbf    = now.ToUnixTimeSeconds();
            var exp    = now.AddMinutes(expireMinutes).ToUnixTimeSeconds();

            // Auth claim value: inner JSON string matching JwtManager's payload shape.
            var authClaimValue = JsonConvert.SerializeObject(new { UserGroups = userGroups });

            var payloadObj = new JObject
            {
                [JwtValidator.AuthClaimKey] = authClaimValue,
                ["nbf"] = nbf,
                ["exp"] = exp,
                ["iat"] = nbf,
            };

            var payloadEncoded = Base64UrlEncode(
                Encoding.UTF8.GetBytes(payloadObj.ToString(Formatting.None)));

            var sigInput = Encoding.ASCII.GetBytes(_headerEncoded + "." + payloadEncoded);
            var key      = Convert.FromBase64String(base64SecretKey);

            using var hmac = new HMACSHA256(key);
            var sig        = Base64UrlEncode(hmac.ComputeHash(sigInput));

            return _headerEncoded + "." + payloadEncoded + "." + sig;
        }

        // ── Encoding ──────────────────────────────────────────────────────────────

        internal static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
    }
}
