///*
// * Happy-path tests for BearerTokenPrincipalParser — the Entra OIDC validation
// * branch never runs in CI because the test environment has no Entra config,
// * so 46 / 91 lines were uncovered before this file. The internal test seam
// * constructor (added alongside these tests) lets us inject a deterministic
// * IConfigurationManager<OpenIdConnectConfiguration> backed by an in-memory
// * RSA signing key, then mint tokens signed with that key.
// *
// * Tests cover:
// *   - happy path TryParseAsync                     (L97-122)
// *   - NormalizeClaims projection                   (L143-158)
// *       - "oid"  / AuthConstants.ObjectIdentifier  → ClaimTypes.NameIdentifier
// *       - "name" / AuthConstants.PreferredUsername → ClaimTypes.Name
// *       - AuthConstants.Roles                      → ClaimTypes.Role
// *       - unknown claim types pass through unchanged
// *   - SecurityTokenExpiredException catch branch   (L124-127)
// *   - SecurityTokenException catch branch          (L129-132)
// *   - generic Exception catch branch               (L134-137)
// *   - no Authorization header                      (L72)
// *   - non-Bearer Authorization header              (L74-79)
// *   - Bearer present but Entra disabled            (L81-87)
// */
//
//using Microsoft.Extensions.Logging.Abstractions;
//using Microsoft.IdentityModel.Protocols;
//using Microsoft.IdentityModel.Protocols.OpenIdConnect;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Threading;
//using System.Threading.Tasks;
//using Warewolf.Execution.Lightweight.Auth.Models;
//using Warewolf.Execution.Lightweight.Auth.Parsers;
//
//namespace Warewolf.Execution.Lightweight.Tests.Auth
//{
//    [TestClass]
//    public class BearerTokenPrincipalParserHappyPathTests
//    {
//        const string TenantId = "00000000-0000-0000-0000-000000000001";
//        const string Audience = "api://test-app";
//        const string Issuer   = "https://login.microsoftonline.com/" + TenantId + "/v2.0";
//
//        static readonly RSA _rsa  = RSA.Create(2048);
//        static readonly RsaSecurityKey _signingKey =
//            new(_rsa) { KeyId = "test-kid" };
//
//        // Static OIDC config — used by the stub IConfigurationManager.
//        static OpenIdConnectConfiguration BuildStubConfig()
//        {
//            var cfg = new OpenIdConnectConfiguration { Issuer = Issuer };
//            cfg.SigningKeys.Add(_signingKey);
//            return cfg;
//        }
//
//        sealed class StaticConfigManager : IConfigurationManager<OpenIdConnectConfiguration>
//        {
//            readonly OpenIdConnectConfiguration _cfg = BuildStubConfig();
//            public Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel) =>
//                Task.FromResult(_cfg);
//            public void RequestRefresh() { }
//        }
//
//        static EntraAuthOptions EnabledOptions() => new()
//        {
//            TenantId = TenantId,
//            Audience = Audience,
//        };
//
//        static BearerTokenPrincipalParser NewParser(EntraAuthOptions? opts = null) =>
//            new(opts ?? EnabledOptions(),
//                NullLogger<BearerTokenPrincipalParser>.Instance,
//                new StaticConfigManager());
//
//        static string MintToken(IEnumerable<Claim> claims, TimeSpan? lifetime = null)
//        {
//            var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);
//            var now   = DateTime.UtcNow;
//            var life  = lifetime ?? TimeSpan.FromMinutes(20);
//            var exp   = now.Add(life);
//            // notBefore must precede expires; for negative-lifetime tokens push it
//            // far enough into the past that the JwtSecurityToken ctor doesn't reject it.
//            var nbf   = life >= TimeSpan.Zero ? now.AddMinutes(-1) : exp.AddMinutes(-1);
//            var jwt   = new JwtSecurityToken(
//                issuer:    Issuer,
//                audience:  Audience,
//                claims:    claims,
//                notBefore: nbf,
//                expires:   exp,
//                signingCredentials: creds);
//            return new JwtSecurityTokenHandler().WriteToken(jwt);
//        }
//
//        static FakeHttpRequestData NewRequest(string? authHeader = null)
//        {
//            var req = new FakeHttpRequestData(
//                new TestFunctionContext(), new Uri("https://x.test/secure/hello"));
//            if (authHeader is not null)
//                req.Headers.Add("Authorization", authHeader);
//            return req;
//        }
//
//        // ── Happy path ────────────────────────────────────────────────────────────
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_ValidToken_ReturnsAuthenticatedPrincipal()
//        {
//            var token = MintToken(new[]
//            {
//                new Claim("oid",  "user-oid-123"),
//                new Claim("name", "Alice Test"),
//                new Claim("roles", "WorkflowExecutor"),
//            });
//
//            var principal = await NewParser().TryParseAsync(
//                NewRequest("Bearer " + token), CancellationToken.None);
//
//            Assert.IsNotNull(principal);
//            Assert.IsTrue(principal.Identity!.IsAuthenticated);
//            Assert.AreEqual("Bearer", principal.Identity!.AuthenticationType);
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_ProjectsOidToNameIdentifier()
//        {
//            var token = MintToken(new[] { new Claim("oid", "user-oid-123") });
//
//            var principal = await NewParser().TryParseAsync(
//                NewRequest("Bearer " + token), CancellationToken.None);
//
//            Assert.IsNotNull(principal);
//            var nameId = principal.FindFirst(ClaimTypes.NameIdentifier);
//            Assert.IsNotNull(nameId, "'oid' must be projected to ClaimTypes.NameIdentifier");
//            Assert.AreEqual("user-oid-123", nameId.Value);
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_ProjectsNameToClaimTypesName()
//        {
//            var token = MintToken(new[]
//            {
//                new Claim("oid",  "x"),
//                new Claim("name", "Bob"),
//            });
//
//            var principal = await NewParser().TryParseAsync(
//                NewRequest("Bearer " + token), CancellationToken.None);
//
//            Assert.IsNotNull(principal);
//            Assert.AreEqual("Bob", principal.FindFirst(ClaimTypes.Name)?.Value);
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_ProjectsRolesClaimToClaimTypesRole()
//        {
//            var token = MintToken(new[]
//            {
//                new Claim("oid",   "x"),
//                new Claim("roles", "RoleA"),
//                new Claim("roles", "RoleB"),
//            });
//
//            var principal = await NewParser().TryParseAsync(
//                NewRequest("Bearer " + token), CancellationToken.None);
//
//            Assert.IsNotNull(principal);
//            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).OrderBy(v => v).ToArray();
//            CollectionAssert.AreEqual(new[] { "RoleA", "RoleB" }, roles);
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_UnknownClaimType_PassesThroughUnchanged()
//        {
//            // Unmapped claim types must survive the projection so downstream
//            // code that cares about them (e.g. custom tenant claims) still sees them.
//            var token = MintToken(new[]
//            {
//                new Claim("oid",                       "x"),
//                new Claim("custom-warewolf-claim",     "value-123"),
//            });
//
//            var principal = await NewParser().TryParseAsync(
//                NewRequest("Bearer " + token), CancellationToken.None);
//
//            Assert.IsNotNull(principal);
//            Assert.AreEqual("value-123", principal.FindFirst("custom-warewolf-claim")?.Value);
//        }
//
//        // ── Failure branches that yield null ──────────────────────────────────────
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_NoAuthorizationHeader_ReturnsNull()
//        {
//            Assert.IsNull(await NewParser().TryParseAsync(NewRequest(), CancellationToken.None));
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_NonBearerScheme_ReturnsNull()
//        {
//            Assert.IsNull(await NewParser()
//                .TryParseAsync(NewRequest("Basic dXNlcjpwYXNz"), CancellationToken.None));
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_EntraDisabled_ReturnsNullEvenWithBearer()
//        {
//            // No TenantId -> IsEnabled=false -> early-return at L81-87.
//            var parser = NewParser(new EntraAuthOptions { Audience = Audience });
//
//            Assert.IsNull(await parser
//                .TryParseAsync(NewRequest("Bearer dummy.token.value"), CancellationToken.None));
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_ExpiredToken_ReturnsNull()
//        {
//            // Lifetime well past the validator's 2-minute clock skew.
//            var token = MintToken(new[] { new Claim("oid", "x") }, lifetime: TimeSpan.FromMinutes(-10));
//
//            Assert.IsNull(await NewParser()
//                .TryParseAsync(NewRequest("Bearer " + token), CancellationToken.None));
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_TamperedSignature_ReturnsNull()
//        {
//            var token = MintToken(new[] { new Claim("oid", "x") });
//            var parts = token.Split('.');
//            // Replace the signature with a syntactically-valid base64url string of the
//            // same length so we exercise the signature-mismatch branch, not the parse
//            // failure branch.
//            var tampered = parts[0] + "." + parts[1] + "." + new string('A', parts[2].Length);
//
//            Assert.IsNull(await NewParser()
//                .TryParseAsync(NewRequest("Bearer " + tampered), CancellationToken.None));
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public async Task TryParseAsync_NotEvenAToken_ReturnsNull()
//        {
//            // Garbage string — JwtSecurityTokenHandler throws a generic exception that
//            // is caught by the broad Exception handler at L134-137.
//            Assert.IsNull(await NewParser()
//                .TryParseAsync(NewRequest("Bearer not-a-jwt-at-all"), CancellationToken.None));
//        }
//
//        [TestMethod]
//        [TestCategory("UnitTest")]
//        public void Name_IsBearer()
//        {
//            Assert.AreEqual("Bearer", NewParser().Name);
//        }
//    }
//}
