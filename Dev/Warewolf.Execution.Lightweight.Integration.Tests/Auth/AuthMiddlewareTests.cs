/*
 * In-process integration tests that target specific code paths in the auth middleware
 * pipeline of the lightweight execution engine.
 *
 * No longer requires a running engine on http://localhost:7071. LightweightInProcessHost
 * runs the real worker middleware pipeline (EasyAuthRedirect → ClaimsPrincipalBuilder →
 * WorkflowAuthorization → function dispatch) in-process against a secure.config seeded per
 * test. The Warewolf HMAC JWT parser validates tokens minted with the host's SecretKey, so
 * the previously-required "matching server config" is now guaranteed and the SkipIf guards
 * are gone.
 *
 * Covered gaps (cross-referenced with coverage report):
 *
 *  EasyAuthRedirectMiddleware
 *    - Browser navigation (Accept:text/html, no Sec-Fetch-Mode:cors) → 302 to /.auth/login/aad
 *    - API-client, no token, /secure/* → 401 JSON body (WWW-Authenticate header present)
 *    - /services/* routes enforced the same as /secure/*
 *    - Request with X-MS-CLIENT-PRINCIPAL header passes the redirect middleware
 *
 *  WorkflowAuthorizationMiddleware
 *    - 401 response body is valid JSON with required fields (error/message/path/correlationId)
 *    - X-WW-Correlation-Id supplied by caller is echoed in the 401 response body and header
 *    - X-WW-Correlation-Id is auto-generated and present in 401 when caller does not supply it
 *    - /services/* route enforced: valid token passes auth gate
 *    - 403 Forbidden when authenticated caller has no matching group in secure.config
 *    - 403 response body schema matches 401 (error/message/path/correlationId/workflow)
 *    - X-WW-Correlation-Id echoed in 403 body and response header
 *    - Development-only X-WW-Bypass-Auth header skips policy checks (open-access branch)
 *
 *  EasyAuthPrincipalParser (end-to-end via the pipeline)
 *    - X-MS-CLIENT-PRINCIPAL header is decoded and yields an authenticated principal
 *    - Malformed base64 header is swallowed gracefully (no 500)
 *    - Full claim normalization: objectidentifier, name, roles all handled
 *
 *  ClaimsPrincipalBuilderMiddleware
 *    - When no parser succeeds → Anonymous principal stored (secure route → 401, not 500)
 *    - Public route with no parsers succeeding → Anonymous accepted, no error
 *
 *  BearerTokenPrincipalParser
 *    - When Entra is not configured the HMAC parser yields Anonymous for a wrong-key token → 401
 *
 *  SecureConfigWatcher (hot-reload)
 *    - Covered by unit tests; the file-watch reload is not applicable to the in-process harness.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Auth
{
    // ══════════════════════════════════════════════════════════════════════════════
    // EasyAuthRedirectMiddleware — browser navigation and /services/* coverage
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [DoNotParallelize]
    [TestCategory("Auth_Middleware")]
    public class EasyAuthRedirectMiddlewareCoverageTests
    {
        private LightweightInProcessHost _host = null!;

        [TestInitialize]
        public void Init()
        {
            var settings = SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm("Azure Functions Users", View: true, Execute: true));
            _host = LightweightInProcessHost.WithSettings(settings);
        }

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        private static string BuildEasyAuthHeader(string authType, params (string typ, string val)[] claims)
        {
            var claimsJson = string.Join(",",
                System.Linq.Enumerable.Select(claims, c =>
                    $"{{\"typ\":\"{c.typ}\",\"val\":\"{c.val}\"}}"));
            var json = $"{{\"auth_typ\":\"{authType}\",\"claims\":[{claimsJson}]}}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        // ── Browser-navigation redirect (EasyAuthRedirectMiddleware lines 107-128) ─

        /// <summary>
        /// When a browser navigates to a /Secure/* route without any auth token the
        /// EasyAuthRedirectMiddleware detects the browser via Accept:text/html and
        /// issues a 302 redirect to /.auth/login/aad instead of 401.
        /// Exercises: EasyAuthRedirectMiddleware.LooksLikeBrowserNavigation → true branch.
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_BrowserNavigation_NoToken_Returns302ToLoginPage()
        {
            var headers = new Dictionary<string, string>
            {
                // No Sec-Fetch-Mode: cors → treated as top-level browser navigation.
                // No Authorization / X-MS-CLIENT-PRINCIPAL headers.
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            };

            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", headers);

            Assert.AreEqual(HttpStatusCode.Redirect, resp.Status,
                "EasyAuthRedirectMiddleware must return 302 for unauthenticated browser navigation to /Secure/*");

            var location = resp.Headers.TryGetValue("Location", out var loc) ? loc : "";
            Assert.IsTrue(location.Contains("/.auth/login/aad", StringComparison.OrdinalIgnoreCase),
                $"302 Location must point to /.auth/login/aad. Got: {location}");
            Assert.IsTrue(location.Contains("post_login_redirect_uri", StringComparison.OrdinalIgnoreCase),
                $"Location must include post_login_redirect_uri. Got: {location}");
        }

        /// <summary>
        /// XHR/fetch (Sec-Fetch-Mode: cors) to /Secure/* without a token must NOT redirect;
        /// it must return 401 so the client library can handle it programmatically.
        /// Exercises: LooksLikeBrowserNavigation → false branch (cors fetch-mode).
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_FetchRequest_NoToken_Returns401NotRedirect()
        {
            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["Sec-Fetch-Mode"] = "cors",
            };

            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", headers);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                "A fetch/XHR request to /Secure/* without a token must return 401, not 302");
        }

        /// <summary>
        /// An API client (no text/html Accept, no Bearer) to /Secure/* gets a 401 with
        /// the WWW-Authenticate header, not a redirect.
        /// Exercises: EasyAuthRedirectMiddleware path where isBrowser=false → 401 JSON branch.
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_ApiClient_NoToken_Returns401WithWwwAuthenticate()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/AnyWorkflow.json");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                "API client request to /Secure/* with no token must return 401");

            var json = System.Text.Json.JsonDocument.Parse(resp.Body).RootElement;

            Assert.AreEqual("unauthorized", json.GetProperty("error").GetString(),
                "Response body 'error' must be 'unauthorized'");
        }

        /// <summary>
        /// /services/* routes must be enforced the same way as /secure/*.
        /// An unauthenticated API call to /services/* must return 401.
        /// Exercises: EasyAuthRedirectMiddleware isServices branch.
        /// </summary>
        [TestMethod]
        public async Task ServicesRoute_NoToken_Returns401()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/services/SomeWorkflow.json");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"/services/* must return 401 when no auth token is present. Got {(int)resp.Status}: {resp.Body}");
        }

        /// <summary>
        /// /services/apis.json must require authentication — EasyAuthRedirectMiddleware
        /// rejects unauthenticated callers on /services/* (and /secure/*) with a 401
        /// JSON error before the discovery endpoint is reached.
        /// Exercises: EasyAuthRedirectMiddleware unauthenticated /services branch.
        /// </summary>
        [TestMethod]
        public async Task ServicesApisJson_NoToken_Returns401()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/services/apis.json");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"/services/apis.json must return 401 without a token. Got {(int)resp.Status}: {resp.Body}");
            StringAssert.Contains(resp.Body, "unauthorized",
                $"Expected the 401 body to contain the 'unauthorized' error code. Body: {resp.Body}");
        }

        /// <summary>
        /// A request carrying the X-MS-CLIENT-PRINCIPAL header (Easy Auth path) must
        /// pass through the redirect middleware even without a Bearer token, because
        /// the principal header signals that the Azure auth platform already validated
        /// the caller.
        /// Exercises: EasyAuthRedirectMiddleware hasPrincipalHeader=true branch.
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_WithEasyAuthHeader_PassesThroughRedirectMiddleware()
        {
            var principalHeader = BuildEasyAuthHeader("aad",
                ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "Integration Test User"));

            var headers = new Dictionary<string, string>
            {
                [AuthConstants.ClientPrincipalHeader] = principalHeader,
            };

            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", headers);

            // The redirect middleware must NOT write the "A valid Bearer token is required" body.
            // If we do get a 401 it must be from WorkflowAuthorizationMiddleware (JSON error body
            // with "error" key), not from EasyAuthRedirectMiddleware (plain string body).
            if (resp.Status == HttpStatusCode.Unauthorized)
            {
                var isRedirectMiddleware401 = resp.Body.Contains("A valid Bearer token is required");
                Assert.IsFalse(isRedirectMiddleware401,
                    $"EasyAuthRedirectMiddleware must not reject a request carrying {AuthConstants.ClientPrincipalHeader}. " +
                    $"Got 401 body: {resp.Body}");
            }
            // Any other outcome (200, 403, 404, 500) means the redirect middleware let it through.
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowAuthorizationMiddleware — JSON error body, correlation ID, /services/*
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [DoNotParallelize]
    [TestCategory("Auth_Middleware")]
    public class WorkflowAuthorizationMiddlewareCoverageTests
    {
        private LightweightInProcessHost _host = null!;
        private string _secretKey = null!;

        [TestInitialize]
        public void Init()
        {
            _secretKey = SecureConfigBuilder.NewSecretKey();
            var settings = SecureConfigBuilder.Build(
                _secretKey,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm("Azure Functions Users", View: true, Execute: true));
            _host = LightweightInProcessHost.WithSettings(settings);
        }

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        private static Dictionary<string, string> Bearer(string token) =>
            new() { ["Authorization"] = "Bearer " + token };

        // ── 401 JSON body structure (WorkflowAuthorizationMiddleware.WriteErrorAsync) ─

        /// <summary>
        /// A bearer token that passes EasyAuthRedirectMiddleware (hasAuthHeader=true) but
        /// fails validation → anonymous principal → WorkflowAuthorizationMiddleware emits
        /// a JSON 401 body with the required fields.
        /// Exercises: WriteErrorAsync and the "no authenticated principal" path (lines 138-153).
        /// </summary>
        [TestMethod]
        public async Task Secure_WrongKeyToken_401BodyIsValidJsonWithRequiredFields()
        {
            // Wrong-key token: EasyAuthRedirectMiddleware passes it (hasAuthHeader=true),
            // the HMAC parser rejects it → Anonymous → WorkflowAuthorizationMiddleware 401
            var badToken = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(badToken));

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"Expected 401. Got {(int)resp.Status}: {resp.Body}");

            JObject? json = null;
            try { json = JObject.Parse(resp.Body); }
            catch (Exception ex) { Assert.Fail($"401 body must be valid JSON. Got: {resp.Body}\nError: {ex.Message}"); }

            Assert.IsNotNull(json!["error"],        $"JSON 401 body must have 'error'. Got: {resp.Body}");
            Assert.IsNotNull(json["message"],       $"JSON 401 body must have 'message'. Got: {resp.Body}");
            Assert.IsNotNull(json["path"],          $"JSON 401 body must have 'path'. Got: {resp.Body}");
            Assert.IsNotNull(json["correlationId"], $"JSON 401 body must have 'correlationId'. Got: {resp.Body}");
        }

        /// <summary>
        /// When the caller supplies X-WW-Correlation-Id the same value must appear in
        /// the 401 response body's "correlationId" field and in the X-WW-Correlation-Id
        /// response header.
        /// Exercises: WorkflowAuthorizationMiddleware.ResolveCorrelationId caller-supplied branch.
        /// </summary>
        [TestMethod]
        public async Task Secure_WrongKeyToken_WithCorrelationIdHeader_EchoesCorrelationId()
        {
            const string correlationId = "my-test-correlation-id-42";
            var badToken = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var headers = Bearer(badToken);
            headers["X-WW-Correlation-Id"] = correlationId;

            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", headers);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"Expected 401. Got {(int)resp.Status}: {resp.Body}");

            JObject json;
            try { json = JObject.Parse(resp.Body); }
            catch { Assert.Fail($"401 body must be JSON. Got: {resp.Body}"); return; }

            Assert.AreEqual(correlationId, json["correlationId"]?.ToString(),
                $"correlationId in response body must match the supplied header. Got: {resp.Body}");

            Assert.IsTrue(resp.Headers.ContainsKey("X-WW-Correlation-Id"),
                "401 response must echo X-WW-Correlation-Id in a response header");
            Assert.AreEqual(correlationId, resp.Headers["X-WW-Correlation-Id"],
                $"X-WW-Correlation-Id response header must match request. Got: {resp.Headers["X-WW-Correlation-Id"]}");
        }

        /// <summary>
        /// When no X-WW-Correlation-Id is supplied the middleware auto-generates one.
        /// The 401 body and response header must both carry it.
        /// Exercises: WorkflowAuthorizationMiddleware.ResolveCorrelationId auto-generate branch.
        /// </summary>
        [TestMethod]
        public async Task Secure_WrongKeyToken_WithoutCorrelationId_AutoGeneratesCorrelationId()
        {
            var badToken = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            // Deliberately NO X-WW-Correlation-Id header
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(badToken));

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"Expected 401. Got {(int)resp.Status}: {resp.Body}");

            JObject json;
            try { json = JObject.Parse(resp.Body); }
            catch { Assert.Fail($"401 body must be JSON. Got: {resp.Body}"); return; }

            var autoId = json["correlationId"]?.ToString();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(autoId),
                $"correlationId must be auto-generated when caller does not supply it. Got: {resp.Body}");

            Assert.IsTrue(resp.Headers.ContainsKey("X-WW-Correlation-Id"),
                "Auto-generated correlation ID must be echoed in X-WW-Correlation-Id response header");
        }

        /// <summary>
        /// /services/* routes are enforced by WorkflowAuthorizationMiddleware.
        /// A valid JWT must not be 401'd by the authorization middleware on /services/*.
        /// Exercises: WorkflowAuthorizationMiddleware isSecure/isServices branch.
        /// </summary>
        [TestMethod]
        public async Task ServicesRoute_ValidToken_PassesAuthorizationMiddleware()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var resp = await _host.SendThroughPipelineAsync("GET", "/services/HelloWorld.json", Bearer(token));

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                "A valid JWT must not produce 401 on /services/* routes. " +
                $"Got {(int)resp.Status}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // EasyAuthPrincipalParser — X-MS-CLIENT-PRINCIPAL end-to-end via the pipeline
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [DoNotParallelize]
    [TestCategory("Auth_Middleware")]
    public class EasyAuthPrincipalParserCoverageTests
    {
        private LightweightInProcessHost _host = null!;

        [TestInitialize]
        public void Init()
        {
            var settings = SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true));
            _host = LightweightInProcessHost.WithSettings(settings);
        }

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        private static string BuildEasyAuthHeader(string authType, params (string typ, string val)[] claims)
        {
            var claimsJson = string.Join(",",
                System.Linq.Enumerable.Select(claims, c =>
                    $"{{\"typ\":\"{c.typ}\",\"val\":\"{c.val}\"}}"));
            var json = $"{{\"auth_typ\":\"{authType}\",\"claims\":[{claimsJson}]}}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// A request with a valid X-MS-CLIENT-PRINCIPAL header must be decoded by
        /// EasyAuthPrincipalParser. The /Secure/apis.json discovery endpoint returns 200
        /// regardless of auth; a 200 here confirms the parser ran without crashing and
        /// produced an authenticated principal that was accepted by the middleware chain.
        /// Exercises: EasyAuthPrincipalParser.TryParseAsync happy path (lines 44-70).
        /// </summary>
        [TestMethod]
        public async Task SecureApisJson_WithEasyAuthHeader_Returns200()
        {
            var header = BuildEasyAuthHeader("aad",
                ("http://schemas.microsoft.com/identity/claims/objectidentifier", "test-oid-99"),
                ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",    "Integration Test User"),
                ("roles", "Warewolf Administrators"));

            var headers = new Dictionary<string, string> { [AuthConstants.ClientPrincipalHeader] = header };
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", headers);

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"/Secure/apis.json with X-MS-CLIENT-PRINCIPAL must return 200. Got {(int)resp.Status}: {resp.Body}");

            JObject json;
            try { json = JObject.Parse(resp.Body); }
            catch { Assert.Fail($"Response must be valid JSON. Got: {resp.Body}"); return; }

            Assert.IsNotNull(json["Apis"], $"Response must contain 'Apis' array. Got: {resp.Body}");
        }

        /// <summary>
        /// A malformed (non-base64) X-MS-CLIENT-PRINCIPAL header must not crash the server.
        /// EasyAuthPrincipalParser catches the exception and returns null; the middleware
        /// falls back to the next parser (HMAC JWT parser), then to Anonymous.
        /// Exercises: EasyAuthPrincipalParser exception-catch branch (lines 73-77).
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_MalformedEasyAuthHeader_Returns401NotServerError()
        {
            var headers = new Dictionary<string, string>
            {
                [AuthConstants.ClientPrincipalHeader] = "!!not-valid-base64!!",
            };

            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", headers);

            // After the parser exception the fallback is Anonymous principal → 401
            // (or 302 for browsers, but we use the default headers).
            // Must NOT be 500 — the exception is caught by the parser.
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.Status,
                "A malformed X-MS-CLIENT-PRINCIPAL header must not cause a 500. " +
                $"Got {(int)resp.Status}");
        }

        /// <summary>
        /// Verifies the three explicit NormalizeClaimType mappings in EasyAuthPrincipalParser:
        ///   objectidentifier URI → NameIdentifier
        ///   name URI            → Name
        ///   "roles"             → Role
        /// An authenticated principal built from these claims must result in a non-500 response.
        /// Exercises: NormalizeClaimType switch (line 82-87) all three explicit branches.
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_WithFullClaimSet_PrincipalBuiltWithoutError()
        {
            var header = BuildEasyAuthHeader("aad",
                ("http://schemas.microsoft.com/identity/claims/objectidentifier", "oid-abc-123"),
                ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",    "John Doe"),
                ("roles", "SomeGroup"));

            // Use /Secure/apis.json — always returns 200 regardless of workflow inventory.
            // The purpose of this test is to verify that EasyAuthPrincipalParser.NormalizeClaimType
            // handles all three explicit claim-type mappings without crashing (no 500).
            var headers = new Dictionary<string, string> { [AuthConstants.ClientPrincipalHeader] = header };
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", headers);

            // Intent: NormalizeClaimType handles all three explicit mappings and the parser
            // builds an AUTHENTICATED principal WITHOUT crashing. The principal authenticates
            // (so not 401/Anonymous) and no unhandled error occurs (not 500). The caller's
            // group ("SomeGroup") may legitimately lack connect permission → 403 from the
            // discovery connect-gate; that still proves the principal was built and accepted.
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                "Full claim-set EasyAuth header must build an authenticated principal (not fall back to Anonymous/401). " +
                $"Got {(int)resp.Status}: {resp.Body}");

            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.Status,
                "Full claim-set EasyAuth header must not cause a 500. " +
                $"Got {(int)resp.Status}: {resp.Body}");
        }

        /// <summary>
        /// Verifies the default (passthrough) branch of NormalizeClaimType: a non-recognized
        /// claim type is forwarded unchanged.
        /// Exercises: NormalizeClaimType _ => claim.Type branch.
        /// </summary>
        [TestMethod]
        public async Task SecureApisJson_WithUnknownClaimType_PrincipalBuiltWithoutError()
        {
            // "custom_claim" is not in the switch — it maps to itself via the default branch.
            var header = BuildEasyAuthHeader("aad",
                ("custom_claim", "some-value"),
                ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "Custom User"));

            var headers = new Dictionary<string, string> { [AuthConstants.ClientPrincipalHeader] = header };
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", headers);

            // Intent: the default (passthrough) NormalizeClaimType branch must not crash.
            // This principal carries no role claim, so the discovery connect-gate may deny it
            // (403) once a real secure.config is loaded — the key assertion is that no
            // unhandled error (500) occurs while building/normalizing the principal.
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.Status,
                $"/Secure/apis.json with unknown claim type must not cause a 500. Got {(int)resp.Status}: {resp.Body}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // ClaimsPrincipalBuilderMiddleware — anonymous-fallback path
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [DoNotParallelize]
    [TestCategory("Auth_Middleware")]
    public class ClaimsPrincipalBuilderMiddlewareCoverageTests
    {
        private LightweightInProcessHost _host = null!;

        [TestInitialize]
        public void Init()
        {
            // Public group has server-wide View+Execute so /Public/* routes execute for anonymous callers.
            _host = LightweightInProcessHost.WithPublicExecuteAll();
        }

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        private static Dictionary<string, string> Bearer(string token) =>
            new() { ["Authorization"] = "Bearer " + token };

        /// <summary>
        /// When all parsers fail (malformed token, no EasyAuth header) the middleware
        /// falls back to WorkflowClaimsPrincipal.Anonymous(). The server must not crash;
        /// the final response on a /Secure/* route must be 401 (not 500).
        /// Exercises: ClaimsPrincipalBuilderMiddleware return Anonymous() path (line 107).
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_AllParsersFail_FallsBackToAnonymous_Returns401()
        {
            // Provide a Bearer header so EasyAuthRedirectMiddleware passes the request,
            // but use an obviously invalid token format that the HMAC parser
            // will reject (three-dot structure is wrong → exception caught).
            var resp = await _host.SendThroughPipelineAsync(
                "GET", "/Secure/HelloWorld.json", Bearer("not.a.valid.jwt.at.all"));

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                "When no parser succeeds the middleware must store Anonymous and return 401. " +
                $"Got {(int)resp.Status}");
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.Status,
                "Anonymous fallback must not crash. Got 500.");
        }

        /// <summary>
        /// A syntactically valid JWT signed with an arbitrary (wrong) key must fail HMAC
        /// validation, yielding Anonymous → WorkflowAuthorizationMiddleware emits a 401.
        /// (In-process equivalent of the original "Entra not configured" path: the HMAC
        /// parser rejects a token it cannot validate rather than attempting OIDC.)
        /// Exercises: HMAC parser wrong-key rejection → Anonymous → 401.
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_EntraNotConfigured_BearerTokenYieldsAnonymous_Returns401()
        {
            // Mint a syntactically valid JWT signed with an arbitrary key the host does not know.
            var arbitraryToken = JwtTestHelper.ValidToken(SecureConfigBuilder.NewSecretKey(), "SomeGroup");
            var resp = await _host.SendThroughPipelineAsync(
                "GET", "/Secure/HelloWorld.json", Bearer(arbitraryToken));

            // Parser yields Anonymous → WorkflowAuthorizationMiddleware → 401.
            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                "A token signed with an unknown key must yield Anonymous → 401. " +
                $"Got {(int)resp.Status}");
        }

        /// <summary>
        /// Public routes accept Anonymous principals. With no auth headers at all, both
        /// parsers fast-exit with null, the middleware stores Anonymous, and the public
        /// route function executes normally.
        /// Exercises: the "no headers" fast-exit paths in both parsers, then Anonymous storage.
        /// </summary>
        [TestMethod]
        public async Task PublicRoute_NoParsersSucceed_FallsBackToAnonymous_NoError()
        {
            // Public endpoint — Anonymous principal is accepted, function should execute.
            var resp = await _host.SendThroughPipelineAsync(
                "GET", "/Public/tools/http%20get/TC013_Get_CustomHeader_Echoed.json");

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                "/Public/* must accept anonymous principals — no 401 expected");
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.Status,
                "Anonymous fallback on /Public/* must not crash — no 500 expected");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowAuthorizationMiddleware — 403 Forbidden and open-access coverage
    //
    // Coverage targets (cross-referenced with report):
    //   WorkflowAuthorizationMiddleware.cs
    //     Forbidden branch (lines ~237-276)        — DenyGroup / DenyPermission → 403
    //     Development bypass branch (lines ~120-130) — X-WW-Bypass-Auth open-access
    //     WriteErrorAsync with HttpStatusCode.Forbidden
    //     AuditLogger.LogAuthOutcome("403", ...)   — structured audit log entry
    //   WorkflowClaimsPrincipal
    //     PermissionFlagMap static initialiser (lines 30-39) — exercised via resolved principal
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [DoNotParallelize]
    [TestCategory("Auth_Middleware")]
    public class WorkflowPolicyEnforcementCoverageTests
    {
        private LightweightInProcessHost _host = null!;
        private string _secretKey = null!;

        [TestInitialize]
        public void Init()
        {
            _secretKey = SecureConfigBuilder.NewSecretKey();
            // Admin only — a caller in any other group has no matching policy entry → 403.
            var settings = SecureConfigBuilder.Build(
                _secretKey,
                SecureConfigBuilder.Admin(View: true));
            _host = LightweightInProcessHost.WithSettings(settings);
        }

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        private static Dictionary<string, string> Bearer(string token) =>
            new() { ["Authorization"] = "Bearer " + token };

        // ── 403 Forbidden — DenyGroup path (WorkflowAuthorizationMiddleware lines ~237-276) ─

        /// <summary>
        /// A valid JWT whose role claims contain no group that appears in secure.config
        /// must produce a 403 Forbidden from WorkflowAuthorizationMiddleware.
        ///
        /// The caller is authenticated (token validates via the HMAC parser) but has no
        /// matching entry in the active policy scope → DenyGroup → 403.
        ///
        /// Exercises:
        ///   WorkflowAuthorizationMiddleware.Invoke — Forbidden (DenyGroup) branch
        ///   AuditLogger.LogAuthOutcome("403", …)   — structured 403 audit event
        ///   WorkflowAuthorizationMiddleware.WriteErrorAsync with HttpStatusCode.Forbidden
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_AuthenticatedCallerWithNoMatchingGroup_Returns403()
        {
            // A group name that is absent from the seeded secure.config (Admin only).
            var token = JwtTestHelper.ValidToken(_secretKey, "NonExistentGroup-a7f8c9d0e1b2");
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.Forbidden, resp.Status,
                "An authenticated caller whose group is absent from secure.config must receive 403. " +
                $"Got {(int)resp.Status}: {resp.Body}");
        }

        /// <summary>
        /// The 403 response body must be valid JSON and contain every required structural
        /// field — mirroring the 401 body schema but additionally including a "workflow" field.
        ///
        /// Exercises: WorkflowAuthorizationMiddleware.WriteErrorAsync with the extra=workflow
        /// anonymous-object overload (the only call-site that passes "extra").
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_ForbiddenResponse_BodyIsValidJsonWithRequiredFields()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "NonExistentGroup-a7f8c9d0e1b2");
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.Forbidden, resp.Status,
                $"Expected 403 to test body structure. Got {(int)resp.Status}: {resp.Body}");

            JObject? json = null;
            try { json = JObject.Parse(resp.Body); }
            catch (Exception ex) { Assert.Fail($"403 body must be valid JSON. Got: {resp.Body}\nError: {ex.Message}"); }

            Assert.IsNotNull(json!["error"],        $"JSON 403 body must have 'error'. Got: {resp.Body}");
            Assert.IsNotNull(json["message"],       $"JSON 403 body must have 'message'. Got: {resp.Body}");
            Assert.IsNotNull(json["path"],          $"JSON 403 body must have 'path'. Got: {resp.Body}");
            Assert.IsNotNull(json["correlationId"], $"JSON 403 body must have 'correlationId'. Got: {resp.Body}");
            Assert.IsNotNull(json["workflow"],      $"JSON 403 body must have 'workflow' (present only on 403). Got: {resp.Body}");
            Assert.AreEqual("forbidden", json["error"]?.ToString(),
                $"403 body error field must be 'forbidden'. Got: {json["error"]}");
        }

        /// <summary>
        /// The X-WW-Correlation-Id header supplied by the caller must be echoed in the 403
        /// response body and response header — identical behaviour to the 401 correlation path.
        ///
        /// Exercises: WorkflowAuthorizationMiddleware.ResolveCorrelationId caller-supplied branch
        /// followed by WriteErrorAsync with the extra=workflow object for a 403 response.
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_ForbiddenResponse_CorrelationIdEchoed()
        {
            const string correlationId = "forbidden-cov-test-42";
            var token = JwtTestHelper.ValidToken(_secretKey, "NonExistentGroup-a7f8c9d0e1b2");
            var headers = Bearer(token);
            headers["X-WW-Correlation-Id"] = correlationId;

            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", headers);

            Assert.AreEqual(HttpStatusCode.Forbidden, resp.Status,
                $"Expected 403 to test correlation ID. Got {(int)resp.Status}: {resp.Body}");

            JObject json;
            try { json = JObject.Parse(resp.Body); }
            catch { Assert.Fail($"403 body must be JSON. Got: {resp.Body}"); return; }

            Assert.AreEqual(correlationId, json["correlationId"]?.ToString(),
                $"correlationId in 403 body must match the caller-supplied header. Got: {resp.Body}");

            Assert.IsTrue(resp.Headers.ContainsKey("X-WW-Correlation-Id"),
                "403 response must echo X-WW-Correlation-Id in a response header");
            Assert.AreEqual(correlationId, resp.Headers["X-WW-Correlation-Id"],
                $"X-WW-Correlation-Id response header must match request. Got: {resp.Headers["X-WW-Correlation-Id"]}");
        }

        /// <summary>
        /// In Development the header <c>X-WW-Bypass-Auth: local-dev-bypass</c> skips all
        /// policy checks, so an authenticated caller whose group would normally be denied
        /// (DenyGroup → 403) must instead pass through. The in-process equivalent of the
        /// original out-of-process BYPASS_SECURE_CONFIG open-access test.
        ///
        /// Exercises: WorkflowAuthorizationMiddleware Development bypass branch (lines ~120-130).
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_BypassEnabled_AuthenticatedCaller_PassesWithoutPolicy()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "AnyGroupForBypassTest");
            var headers = Bearer(token);
            headers["X-WW-Bypass-Auth"] = "local-dev-bypass";

            var resp = await _host.SendThroughPipelineAsync(
                "GET", "/Secure/HelloWorld.json", headers, isDevelopment: true);

            // With the dev bypass active the middleware calls next() regardless of group.
            // Acceptable outcomes: 200 (workflow exists), 404 (workflow missing), or other
            // non-policy outcomes. NOT 401 or 403.
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"Dev bypass must skip the auth gate — no 401 expected. Got {(int)resp.Status}: {resp.Body}");
            Assert.AreNotEqual(HttpStatusCode.Forbidden, resp.Status,
                $"Dev bypass must skip policy checks — no 403 expected. Got {(int)resp.Status}: {resp.Body}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // SecureConfigWatcher — hot-reload lifecycle coverage
    //
    // The SecureConfigWatcher is a hosted service that watches the on-disk secure.config
    // for changes and triggers SecureConfigLoader.Reload + IWorkflowAuthPolicyLoader.Reload
    // within a debounce window. This behaviour is inherent to the FileSystemWatcher +
    // debounce timer and is exercised by unit tests; it is not meaningfully reproducible
    // through the in-process request pipeline harness (which seeds a static config per test
    // and has no running file watcher). These tests are therefore ignored here.
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [DoNotParallelize]
    [TestCategory("Auth_Middleware")]
    public class SecureConfigWatcherHotReloadTests
    {
        /// <summary>
        /// Writing a new config to the path watched by SecureConfigWatcher must trigger
        /// SecureConfigLoader.Reload() + IWorkflowAuthPolicyLoader.Reload() within the
        /// 500 ms debounce window. A subsequent request must reflect the new policy.
        /// </summary>
        [TestMethod]
        [Ignore("SecureConfigWatcher hot-reload is covered by unit tests; not applicable to the in-process pipeline harness")]
        public Task SecureConfigWatcher_FileChange_TriggersReload_PolicyReflected() => Task.CompletedTask;

        /// <summary>
        /// Writing a second config change within the debounce window (500 ms) must produce
        /// exactly one reload, not two. After the debounce the server must use the final
        /// config written, not an intermediate state.
        /// </summary>
        [TestMethod]
        [Ignore("SecureConfigWatcher hot-reload is covered by unit tests; not applicable to the in-process pipeline harness")]
        public Task SecureConfigWatcher_RapidDoubleWrite_ProducesSingleReload() => Task.CompletedTask;
    }
}
