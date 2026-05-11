/*
 * Direct unit tests for JwtGenerator — the engine's HMAC-SHA256 JWT issuer.
 *
 * The existing security suite exercises JwtGenerator indirectly through
 * JwtTestHelper (which now delegates to GenerateToken). These tests pin the
 * format itself so a regression in the wire shape — header alg, payload claim
 * names, signature length, base64url encoding — is caught even if validation
 * still happens to round-trip.
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
    public class JwtGeneratorTests
    {
        static string NewKey()
        {
            using var hmac = new HMACSHA256();
            return Convert.ToBase64String(hmac.Key);
        }

        static byte[] Base64UrlDecode(string s)
        {
            var pad = s.Length % 4;
            if (pad > 0) s += new string('=', 4 - pad);
            return Convert.FromBase64String(s.Replace('-', '+').Replace('_', '/'));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_RoundTripsThroughJwtValidator()
        {
            var key   = NewKey();
            var token = JwtGenerator.GenerateToken(new[] { "TeamA", "TeamB" }, key);

            var groups = JwtValidator.GetUserGroups(token, key);

            Assert.IsNotNull(groups);
            CollectionAssert.AreEqual(new[] { "TeamA", "TeamB" }, System.Linq.Enumerable.ToArray(groups));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_ProducesThreeDotSeparatedBase64UrlParts()
        {
            var token = JwtGenerator.GenerateToken(new[] { "X" }, NewKey());
            var parts = token.Split('.');

            Assert.AreEqual(3, parts.Length, "JWT must have header.payload.signature");
            foreach (var p in parts)
            {
                StringAssert.Matches(p, new System.Text.RegularExpressions.Regex("^[A-Za-z0-9_-]+$"),
                    "Each segment must be base64url (no +, /, or = padding).");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_HeaderIsHs256Jwt()
        {
            var token  = JwtGenerator.GenerateToken(new[] { "X" }, NewKey());
            var header = Encoding.UTF8.GetString(Base64UrlDecode(token.Split('.')[0]));

            var obj = JObject.Parse(header);
            Assert.AreEqual("HS256", (string?)obj["alg"]);
            Assert.AreEqual("JWT",   (string?)obj["typ"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_PayloadCarriesAuthClaimAndUnixTimestamps()
        {
            var before = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var token   = JwtGenerator.GenerateToken(new[] { "G1" }, NewKey(), expireMinutes: 20);
            var after   = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var payload = JObject.Parse(
                Encoding.UTF8.GetString(Base64UrlDecode(token.Split('.')[1])));

            Assert.IsNotNull(payload[JwtValidator.AuthClaimKey], "auth claim must be present");
            var inner = JObject.Parse((string)payload[JwtValidator.AuthClaimKey]!);
            CollectionAssert.AreEqual(new[] { "G1" },
                System.Linq.Enumerable.ToArray(inner["UserGroups"]!.Values<string>()));

            var nbf = (long)payload["nbf"]!;
            var iat = (long)payload["iat"]!;
            var exp = (long)payload["exp"]!;
            Assert.IsTrue(nbf >= before && nbf <= after, $"nbf {nbf} out of window [{before},{after}]");
            Assert.AreEqual(nbf, iat, "iat should equal nbf on issuance");
            Assert.AreEqual(nbf + 20 * 60, exp, "exp should be nbf + expireMinutes*60");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_SignatureIsHmacSha256OfHeaderDotPayload()
        {
            var key      = NewKey();
            var token    = JwtGenerator.GenerateToken(new[] { "X" }, key);
            var parts    = token.Split('.');
            var signed   = Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]);

            using var hmac = new HMACSHA256(Convert.FromBase64String(key));
            var expected   = hmac.ComputeHash(signed);

            CollectionAssert.AreEqual(expected, Base64UrlDecode(parts[2]));
            Assert.AreEqual(32, expected.Length, "HMAC-SHA256 output is 32 bytes");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_NegativeExpireMinutes_IsRejectedByValidator()
        {
            var key   = NewKey();
            var token = JwtGenerator.GenerateToken(new[] { "X" }, key, expireMinutes: -1);

            Assert.IsNull(JwtValidator.GetUserGroups(token, key),
                "An already-expired token must validate as null.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_EmptyUserGroups_StillRoundTrips()
        {
            var key   = NewKey();
            var token = JwtGenerator.GenerateToken(Array.Empty<string>(), key);

            var groups = JwtValidator.GetUserGroups(token, key);
            Assert.IsNotNull(groups);
            Assert.AreEqual(0, groups.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GenerateToken_DifferentKeys_ProduceDifferentSignatures()
        {
            var a = JwtGenerator.GenerateToken(new[] { "X" }, NewKey());
            var b = JwtGenerator.GenerateToken(new[] { "X" }, NewKey());

            Assert.AreNotEqual(a.Split('.')[2], b.Split('.')[2],
                "Different keys must yield different signatures.");
        }
    }
}
