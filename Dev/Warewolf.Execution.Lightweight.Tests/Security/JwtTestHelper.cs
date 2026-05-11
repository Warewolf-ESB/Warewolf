/*
 * Test helper — NOT production code.
 *
 * Thin wrappers around the production JwtGenerator. Tampering / wrong-key
 * variants operate on a valid token's output so the surface they exercise is
 * always the same path that real callers (LoginFunction etc.) take.
 */

using System;
using System.Security.Cryptography;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    public static class JwtTestHelper
    {
        /// <summary>
        /// Generates a valid JWT for the given user groups, signed with
        /// <paramref name="base64SecretKey"/>.
        /// </summary>
        public static string ValidToken(string base64SecretKey, params string[] userGroups) =>
            JwtGenerator.GenerateToken(userGroups, base64SecretKey, expireMinutes: 20);

        /// <summary>
        /// Generates an already-expired token (exp = 10 minutes in the past).
        /// <see cref="JwtValidator.GetUserGroups"/> must return <c>null</c> for this.
        /// </summary>
        public static string ExpiredToken(string base64SecretKey, params string[] userGroups) =>
            JwtGenerator.GenerateToken(userGroups, base64SecretKey, expireMinutes: -10);

        /// <summary>
        /// Token with one extra char appended to the payload — payload bytes no
        /// longer match the signed input, so signature verification must fail.
        /// </summary>
        public static string TamperedPayloadToken(string base64SecretKey, params string[] userGroups)
        {
            var parts = ValidToken(base64SecretKey, userGroups).Split('.');
            return parts[0] + "." + parts[1] + "X." + parts[2];
        }

        /// <summary>Replaces the signature with random bytes; payload intact.</summary>
        public static string BadSignatureToken(string base64SecretKey, params string[] userGroups)
        {
            var parts     = ValidToken(base64SecretKey, userGroups).Split('.');
            var randomSig = new byte[32];
            RandomNumberGenerator.Fill(randomSig);
            return parts[0] + "." + parts[1] + "." + Base64UrlEncode(randomSig);
        }

        /// <summary>Signs the token with a key that differs from the validator's.</summary>
        public static string WrongKeyToken(string validatorKey, params string[] userGroups)
        {
            using var hmac = new HMACSHA256();
            return JwtGenerator.GenerateToken(userGroups, Convert.ToBase64String(hmac.Key));
        }

        public static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
    }
}
