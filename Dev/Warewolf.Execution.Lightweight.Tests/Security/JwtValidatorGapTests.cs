/*
 * Gap tests for JwtValidator — pins the three branches the security spec
 * suite never reaches:
 *
 *   L108     auth claim present but empty/whitespace      → returns empty list
 *   L114-116 catch block on malformed payload JSON        → returns null
 *   L141     CryptographicEquals length-mismatch fast path → signature rejection
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class JwtValidatorGapTests
    {
        static string NewKey()
        {
            using var hmac = new HMACSHA256();
            return Convert.ToBase64String(hmac.Key);
        }

        static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // Build a token whose payload contains an empty auth-claim string, signed
        // correctly so the signature check passes and we exercise the "claim
        // present but empty" branch (L108).
        static string TokenWithEmptyAuthClaim(string key)
        {
            var header = Base64UrlEncode(Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}"""));
            var nbf    = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var payloadObj = new JObject
            {
                [JwtValidator.AuthClaimKey] = "",
                ["nbf"] = nbf, ["exp"] = nbf + 600, ["iat"] = nbf,
            };
            var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadObj.ToString(Newtonsoft.Json.Formatting.None)));
            using var hmac = new HMACSHA256(Convert.FromBase64String(key));
            var sig = Base64UrlEncode(hmac.ComputeHash(Encoding.ASCII.GetBytes(header + "." + payload)));
            return header + "." + payload + "." + sig;
        }

        // A 3-part token whose payload base64-decodes to non-JSON. parts.Length == 3
        // so the early return at L80 doesn't fire — execution reaches the inner
        // try/catch and the JObject.Parse throw is swallowed (L114-116).
        static string TokenWithMalformedPayload(string key)
        {
            var header  = Base64UrlEncode(Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}"""));
            var payload = Base64UrlEncode(Encoding.UTF8.GetBytes("not valid json {[}"));
            using var hmac = new HMACSHA256(Convert.FromBase64String(key));
            var sig = Base64UrlEncode(hmac.ComputeHash(Encoding.ASCII.GetBytes(header + "." + payload)));
            return header + "." + payload + "." + sig;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUserGroups_EmptyAuthClaim_ReturnsEmptyList()
        {
            var key = NewKey();
            var token = TokenWithEmptyAuthClaim(key);

            var groups = JwtValidator.GetUserGroups(token, key);

            Assert.IsNotNull(groups, "Empty auth claim should map to empty list, not null.");
            Assert.AreEqual(0, groups.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUserGroups_MalformedPayload_ReturnsNull()
        {
            var key = NewKey();

            Assert.IsNull(JwtValidator.GetUserGroups(TokenWithMalformedPayload(key), key));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUserGroups_ShortSignatureSegment_FailsLengthCheck()
        {
            // CryptographicEquals takes the length-mismatch early-exit when the
            // token's signature segment is not a 32-byte HMAC (43 base64url chars).
            // Hand-craft a syntactically-valid 3-part token with a 4-char signature.
            var key   = NewKey();
            var token = JwtTestHelper.ValidToken(key, "G1");
            var parts = token.Split('.');
            var truncated = parts[0] + "." + parts[1] + "." + parts[2].Substring(0, 4);

            Assert.IsNull(JwtValidator.GetUserGroups(truncated, key));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUserGroups_BearerPrefix_IsStripped()
        {
            var key   = NewKey();
            var token = JwtTestHelper.ValidToken(key, "G1");

            var groups = JwtValidator.GetUserGroups("Bearer " + token, key);

            Assert.IsNotNull(groups);
            CollectionAssert.AreEqual(new[] { "G1" }, System.Linq.Enumerable.ToArray(groups));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUserGroups_NullInputs_ReturnNull()
        {
            Assert.IsNull(JwtValidator.GetUserGroups(null, NewKey()));
            Assert.IsNull(JwtValidator.GetUserGroups("  ", NewKey()));
            Assert.IsNull(JwtValidator.GetUserGroups(JwtTestHelper.ValidToken(NewKey(), "X"), ""));
        }
    }
}
