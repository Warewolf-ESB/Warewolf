/*
 * Gap tests for EntraTokenValidator.GetRolesFromEasyAuth — the two branches
 * the spec suite never hits:
 *
 *   L173  claims array absent     → returns empty list (not null)
 *   L182  claim with empty value  → skipped (no role added)
 *
 * Plus the GetRolesFromToken catch block (L283-285) when payload decode throws.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class EntraTokenValidatorGapTests
    {
        static string Encode(JObject principal) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(principal.ToString(Newtonsoft.Json.Formatting.None)));

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetRolesFromEasyAuth_AadAuthTypeButNoClaimsArray_ReturnsEmpty()
        {
            // auth_typ=aad, but the principal carries no 'claims' field at all.
            var header = Encode(new JObject
            {
                [EntraTokenValidator.EasyAuthAuthTypeField] = EntraTokenValidator.EasyAuthAadAuthType,
            });

            var roles = EntraTokenValidator.GetRolesFromEasyAuth(header);

            Assert.IsNotNull(roles, "Missing claims array should yield empty list, not null.");
            Assert.AreEqual(0, roles.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetRolesFromEasyAuth_ClaimWithEmptyValue_IsSkipped()
        {
            // One claim with the right type but a blank value (should be skipped),
            // another with a real value (should be picked up).
            var header = Encode(new JObject
            {
                [EntraTokenValidator.EasyAuthAuthTypeField] = EntraTokenValidator.EasyAuthAadAuthType,
                [EntraTokenValidator.EasyAuthClaimsField]   = new JArray
                {
                    new JObject
                    {
                        [EntraTokenValidator.EasyAuthClaimTypeField]  = EntraTokenValidator.RolesClaim,
                        [EntraTokenValidator.EasyAuthClaimValueField] = "",
                    },
                    new JObject
                    {
                        [EntraTokenValidator.EasyAuthClaimTypeField]  = EntraTokenValidator.RolesClaim,
                        [EntraTokenValidator.EasyAuthClaimValueField] = "Admin",
                    },
                },
            });

            var roles = EntraTokenValidator.GetRolesFromEasyAuth(header);

            Assert.IsNotNull(roles);
            CollectionAssert.AreEqual(new[] { "Admin" }, System.Linq.Enumerable.ToArray(roles));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetRolesFromEasyAuth_NonAadAuthType_ReturnsNull()
        {
            var header = Encode(new JObject
            {
                [EntraTokenValidator.EasyAuthAuthTypeField] = "google",
            });

            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth(header));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetRolesFromEasyAuth_NullOrWhitespaceHeader_ReturnsNull()
        {
            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth(null));
            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth("   "));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetRolesFromEasyAuth_NotBase64_FallsIntoCatchAndReturnsNull()
        {
            // Convert.FromBase64String throws; outer catch returns null.
            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth("!!!not-base64!!!"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetRoles_MalformedBearerHeader_FallsIntoCatchAndReturnsNull()
        {
            // Raw bearer token path — a 3-part token with garbage payload trips
            // the JObject.Parse inside GetRolesFromToken; the catch returns null.
            // This exercises L283-285.
            var headerB64  = Convert.ToBase64String(Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}"""));
            var payloadB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("not json"));
            var sigB64     = "AAAA";
            var token      = "Bearer " + headerB64 + "." + payloadB64 + "." + sigB64;

            Assert.IsNull(EntraTokenValidator.GetRoles(
                authorizationHeader:     token,
                easyAuthPrincipalHeader: null,
                requiredTenantId:        null,
                requiredAudience:        null));
        }
    }
}
