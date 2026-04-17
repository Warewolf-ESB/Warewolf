/*
 * Unit tests for EntraTokenValidator.
 *
 * Coverage:
 *   GetRolesFromToken
 *     - valid token with roles        → returns roles
 *     - valid token with groups       → returns group IDs
 *     - valid token, roles + groups   → returns all
 *     - valid token, no roles/groups  → returns empty list
 *     - expired token                 → null
 *     - wrong issuer (not Entra)      → null
 *     - non-Entra Warewolf JWT        → null
 *     - missing/empty header          → null
 *     - malformed token               → null
 *     - tenant constraint met         → returns roles
 *     - tenant constraint violated    → null
 *     - audience constraint met       → returns roles
 *     - audience constraint violated  → null
 *     - Bearer prefix stripped        → returns roles
 *
 *   GetRolesFromEasyAuth
 *     - valid aad principal header    → returns roles from claims
 *     - non-aad auth type             → null
 *     - malformed header              → null
 *     - null/empty header             → null
 *     - Microsoft role claim type URI → accepted as role
 *
 *   GetRoles (combined)
 *     - EasyAuth header present and valid → uses EasyAuth, ignores Bearer
 *     - EasyAuth absent, valid Bearer     → falls back to Bearer token
 *     - both absent                       → null
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class EntraTokenValidatorTests
    {
        // ── Shared constants ──────────────────────────────────────────────────────

        const string SampleTenantId  = "ca0cc53b-9af4-4067-bcdf-be9c648450d1";
        const string SampleClientId  = "05794411-b275-4801-97ac-8b078ed7196c";
        const string SampleIssuerV2  = "https://login.microsoftonline.com/" + SampleTenantId + "/v2.0";
        const string SampleIssuerSTS = "https://sts.windows.net/" + SampleTenantId + "/";

        // ══════════════════════════════════════════════════════════════════════════
        // GetRolesFromToken — happy-path cases
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_ValidToken_WithRoles_ReturnsRoles()
        {
            var token  = BuildEntraToken(SampleIssuerV2, roles: ["WorkflowExecutor", "ReportViewer"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "WorkflowExecutor");
            CollectionAssert.Contains(result as System.Collections.ICollection, "ReportViewer");
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_ValidToken_WithGroups_ReturnsGroupIds()
        {
            var groupId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
            var token   = BuildEntraToken(SampleIssuerV2, groups: [groupId]);
            var result  = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, groupId);
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_ValidToken_RolesAndGroups_ReturnsBoth()
        {
            var token = BuildEntraToken(SampleIssuerV2,
                roles:  ["RoleA"],
                groups: ["GroupGuid"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result!.Count);
            CollectionAssert.Contains(result as System.Collections.ICollection, "RoleA");
            CollectionAssert.Contains(result as System.Collections.ICollection, "GroupGuid");
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_ValidToken_NoRolesOrGroups_ReturnsEmptyList()
        {
            var token  = BuildEntraToken(SampleIssuerV2);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result!.Count);
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_STSIssuer_IsAccepted()
        {
            var token  = BuildEntraToken(SampleIssuerSTS, roles: ["Executor"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "Executor");
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_BearerPrefixStripped()
        {
            var token  = BuildEntraToken(SampleIssuerV2, roles: ["Admin"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "Admin");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GetRolesFromToken — rejection cases
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_ExpiredToken_ReturnsNull()
        {
            var token  = BuildEntraToken(SampleIssuerV2, roles: ["Role1"], expireMinutes: -10);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNull(result);
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_WrongIssuer_ReturnsNull()
        {
            var token  = BuildEntraToken("https://example.com/not-entra", roles: ["Role1"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token);

            Assert.IsNull(result);
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_WarewolfJwt_ReturnsNull()
        {
            // A Warewolf HMAC-SHA256 JWT has no Entra issuer.
            var warewolfToken = JwtTestHelper.ValidToken(SecureConfigBuilder.NewSecretKey(), "TeamA");
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + warewolfToken);

            Assert.IsNull(result);
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_NullHeader_ReturnsNull()
        {
            Assert.IsNull(EntraTokenValidator.GetRolesFromToken(null));
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_EmptyHeader_ReturnsNull()
        {
            Assert.IsNull(EntraTokenValidator.GetRolesFromToken(""));
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_MalformedToken_ReturnsNull()
        {
            Assert.IsNull(EntraTokenValidator.GetRolesFromToken("not.a.valid.jwt.at.all"));
            Assert.IsNull(EntraTokenValidator.GetRolesFromToken("onlytwoparts.here"));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GetRolesFromToken — optional tenant constraint
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_TenantConstraint_Matching_ReturnsRoles()
        {
            var token  = BuildEntraToken(SampleIssuerV2, tenantId: SampleTenantId, roles: ["Exec"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token,
                requiredTenantId: SampleTenantId);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "Exec");
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_TenantConstraint_Mismatch_ReturnsNull()
        {
            var token  = BuildEntraToken(SampleIssuerV2, tenantId: SampleTenantId, roles: ["Exec"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token,
                requiredTenantId: "99999999-9999-9999-9999-999999999999");

            Assert.IsNull(result);
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_TenantConstraint_MatchViaIssuerUrl()
        {
            // Token has no explicit tid claim but the issuer URL contains the tenant.
            var token  = BuildEntraToken(SampleIssuerV2, roles: ["Exec"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token,
                requiredTenantId: SampleTenantId);

            Assert.IsNotNull(result);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GetRolesFromToken — optional audience constraint
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_AudienceConstraint_Matching_ReturnsRoles()
        {
            var token  = BuildEntraToken(SampleIssuerV2, audience: SampleClientId, roles: ["Exec"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token,
                requiredAudience: SampleClientId);

            Assert.IsNotNull(result);
        }

        [TestMethod, TestCategory("Entra_Token")]
        public void GetRolesFromToken_AudienceConstraint_Mismatch_ReturnsNull()
        {
            var token  = BuildEntraToken(SampleIssuerV2, audience: SampleClientId, roles: ["Exec"]);
            var result = EntraTokenValidator.GetRolesFromToken("Bearer " + token,
                requiredAudience: "wrong-audience");

            Assert.IsNull(result);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GetRolesFromEasyAuth
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Entra_EasyAuth")]
        public void GetRolesFromEasyAuth_ValidAadPrincipal_ReturnsRoles()
        {
            var header = BuildEasyAuthHeader("aad",
                ("roles", "WorkflowExecutor"),
                ("roles", "ReportViewer"));

            var result = EntraTokenValidator.GetRolesFromEasyAuth(header);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "WorkflowExecutor");
            CollectionAssert.Contains(result as System.Collections.ICollection, "ReportViewer");
        }

        [TestMethod, TestCategory("Entra_EasyAuth")]
        public void GetRolesFromEasyAuth_MsRoleClaimType_IsAccepted()
        {
            var header = BuildEasyAuthHeader("aad",
                (EntraTokenValidator.MsRoleClaimType, "FinanceViewer"));

            var result = EntraTokenValidator.GetRolesFromEasyAuth(header);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "FinanceViewer");
        }

        [TestMethod, TestCategory("Entra_EasyAuth")]
        public void GetRolesFromEasyAuth_NoRoleClaims_ReturnsEmptyList()
        {
            var header = BuildEasyAuthHeader("aad",
                ("name", "John Doe"),
                ("preferred_username", "john@example.com"));

            var result = EntraTokenValidator.GetRolesFromEasyAuth(header);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result!.Count);
        }

        [TestMethod, TestCategory("Entra_EasyAuth")]
        public void GetRolesFromEasyAuth_NonAadAuthType_ReturnsNull()
        {
            // GitHub or Google OAuth should not be accepted as Entra.
            var header = BuildEasyAuthHeader("github",
                ("roles", "SomeRole"));

            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth(header));
        }

        [TestMethod, TestCategory("Entra_EasyAuth")]
        public void GetRolesFromEasyAuth_NullHeader_ReturnsNull()
        {
            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth(null));
        }

        [TestMethod, TestCategory("Entra_EasyAuth")]
        public void GetRolesFromEasyAuth_MalformedBase64_ReturnsNull()
        {
            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth("!!!not-base64!!!"));
        }

        [TestMethod, TestCategory("Entra_EasyAuth")]
        public void GetRolesFromEasyAuth_ValidBase64ButInvalidJson_ReturnsNull()
        {
            var notJson = Convert.ToBase64String(Encoding.UTF8.GetBytes("this is not json"));
            Assert.IsNull(EntraTokenValidator.GetRolesFromEasyAuth(notJson));
        }

        // ══════════════════════════════════════════════════════════════════════════
        // GetRoles — combined resolution
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Entra_Combined")]
        public void GetRoles_EasyAuthPresent_UsesEasyAuth()
        {
            var easyAuthHeader = BuildEasyAuthHeader("aad", ("roles", "EasyAuthRole"));

            // Pass a Bearer token with different roles to confirm EasyAuth wins.
            var bearerToken = BuildEntraToken(SampleIssuerV2, roles: ["BearerRole"]);

            var result = EntraTokenValidator.GetRoles(
                "Bearer " + bearerToken,
                easyAuthHeader,
                requiredTenantId: null,
                requiredAudience: null);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "EasyAuthRole");
            CollectionAssert.DoesNotContain(result as System.Collections.ICollection, "BearerRole");
        }

        [TestMethod, TestCategory("Entra_Combined")]
        public void GetRoles_NoEasyAuth_FallsBackToBearer()
        {
            var bearerToken = BuildEntraToken(SampleIssuerV2, roles: ["BearerRole"]);

            var result = EntraTokenValidator.GetRoles(
                "Bearer " + bearerToken,
                easyAuthPrincipalHeader: null,
                requiredTenantId: null,
                requiredAudience: null);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "BearerRole");
        }

        [TestMethod, TestCategory("Entra_Combined")]
        public void GetRoles_BothAbsent_ReturnsNull()
        {
            Assert.IsNull(EntraTokenValidator.GetRoles(
                authorizationHeader:      null,
                easyAuthPrincipalHeader:  null,
                requiredTenantId:         null,
                requiredAudience:         null));
        }

        [TestMethod, TestCategory("Entra_Combined")]
        public void GetRoles_InvalidEasyAuth_FallsBackToBearer()
        {
            // EasyAuth header is non-AAD (GitHub) → falls back to the Bearer token.
            var nonAadEasyAuth = BuildEasyAuthHeader("github", ("roles", "GitHubRole"));
            var bearerToken    = BuildEntraToken(SampleIssuerV2, roles: ["EntraRole"]);

            var result = EntraTokenValidator.GetRoles(
                "Bearer " + bearerToken,
                nonAadEasyAuth,
                requiredTenantId: null,
                requiredAudience: null);

            Assert.IsNotNull(result);
            CollectionAssert.Contains(result as System.Collections.ICollection, "EntraRole");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // PermissionChecker integration — Entra roles map to secure.config entries
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Entra_PermissionChecker")]
        public void EntraRole_MatchesPermissionEntryGroupName()
        {
            // Build a config where "WorkflowExecutor" has View on "MyWorkflow".
            var key = SecureConfigBuilder.NewSecretKey();
            var path = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.GroupBasedAccess(key,
                [("WorkflowExecutor", ["MyWorkflow"])]));

            try
            {
                var config = SecureConfigLoader.LoadFrom(path);
                var entraRoles = new List<string> { "WorkflowExecutor" };

                Assert.IsTrue(PermissionChecker.HasUserViewPermission("MyWorkflow", config, entraRoles));
                Assert.IsFalse(PermissionChecker.HasUserViewPermission("OtherWorkflow", config, entraRoles));
            }
            finally
            {
                try { System.IO.File.Delete(path); } catch { }
            }
        }

        [TestMethod, TestCategory("Entra_PermissionChecker")]
        public void EntraRole_CaseInsensitiveMatch()
        {
            var key  = SecureConfigBuilder.NewSecretKey();
            var path = SecureConfigBuilder.WriteTempConfig(
                SecureConfigBuilder.GroupBasedAccess(key,
                [("workflowexecutor", ["MyWorkflow"])]));

            try
            {
                var config     = SecureConfigLoader.LoadFrom(path);
                var entraRoles = new List<string> { "WorkflowExecutor" };   // different casing

                Assert.IsTrue(PermissionChecker.HasUserViewPermission("MyWorkflow", config, entraRoles));
            }
            finally
            {
                try { System.IO.File.Delete(path); } catch { }
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds a JWT with an Entra-shaped payload.  The signature is a random
        /// placeholder — <see cref="EntraTokenValidator"/> does not verify RS256
        /// signatures and relies on EasyAuth or a hosting-layer guarantee instead.
        /// </summary>
        static string BuildEntraToken(
            string   issuer,
            string?  tenantId       = null,
            string?  audience       = null,
            string[] roles          = null!,
            string[] groups         = null!,
            int      expireMinutes  = 20)
        {
            var now = DateTimeOffset.UtcNow;
            var exp = now.AddMinutes(expireMinutes).ToUnixTimeSeconds();

            var payload = new JObject
            {
                [EntraTokenValidator.IssuerClaim]  = issuer,
                [EntraTokenValidator.ExpiryClaim]  = exp,
            };

            if (!string.IsNullOrEmpty(tenantId))
                payload[EntraTokenValidator.TenantIdClaim] = tenantId;

            if (!string.IsNullOrEmpty(audience))
                payload[EntraTokenValidator.AudienceClaim] = audience;

            if (roles is { Length: > 0 })
                payload[EntraTokenValidator.RolesClaim] = new JArray(roles);

            if (groups is { Length: > 0 })
                payload[EntraTokenValidator.GroupsClaim] = new JArray(groups);

            var header  = Base64UrlEncode(Encoding.UTF8.GetBytes("""{"alg":"RS256","typ":"JWT"}"""));
            var body    = Base64UrlEncode(Encoding.UTF8.GetBytes(payload.ToString(Formatting.None)));
            var fakeSig = Base64UrlEncode(Encoding.UTF8.GetBytes("fake-signature-not-verified"));

            return header + "." + body + "." + fakeSig;
        }

        /// <summary>
        /// Builds a Base64-encoded <c>X-MS-CLIENT-PRINCIPAL</c> header value with the
        /// given auth type and claim type/value pairs.
        /// </summary>
        static string BuildEasyAuthHeader(string authType, params (string typ, string val)[] claims)
        {
            var claimsArray = new JArray();
            foreach (var (typ, val) in claims)
            {
                claimsArray.Add(new JObject
                {
                    [EntraTokenValidator.EasyAuthClaimTypeField]  = typ,
                    [EntraTokenValidator.EasyAuthClaimValueField] = val,
                });
            }

            var principal = new JObject
            {
                [EntraTokenValidator.EasyAuthAuthTypeField] = authType,
                [EntraTokenValidator.EasyAuthClaimsField]   = claimsArray,
            };

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(principal.ToString(Formatting.None)));
        }

        static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
    }
}
