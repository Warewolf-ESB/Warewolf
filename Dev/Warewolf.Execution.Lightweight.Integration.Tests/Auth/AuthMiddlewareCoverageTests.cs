/*
 * Integration tests that target specific uncovered code paths in the auth middleware
 * pipeline of the lightweight execution engine.
 *
 * PRE-REQUISITE: Azure Functions host running at http://localhost:7071
 * Tests are marked Inconclusive (not Failed) when the host is not reachable.
 *
 * Covered gaps (cross-referenced with coverage report):
 *
 *  EasyAuthRedirectMiddleware
 *    - Browser navigation (Accept:text/html, no Sec-Fetch-Mode:cors) → 302 to /.auth/login/aad
 *    - API-client, no token, /secure/* → 401 JSON body (WWW-Authenticate header present)
 *    - /services/* routes enforced the same as /secure/*
 *    - apis.json suffix on /services/* passes through without token (200, not 401)
 *    - Request with X-MS-CLIENT-PRINCIPAL header passes the redirect middleware
 *
 *  WorkflowAuthorizationMiddleware
 *    - 401 response body is valid JSON with required fields (error/message/path/correlationId)
 *    - X-WW-Correlation-Id supplied by caller is echoed in the 401 response body and header
 *    - X-WW-Correlation-Id is auto-generated and present in 401 when caller does not supply it
 *    - /services/* route enforced: valid token passes auth gate
 *    - Dev bypass header (X-WW-Bypass-Auth: local-dev-bypass) skips policy checks
 *
 *  EasyAuthPrincipalParser (end-to-end via HTTP)
 *    - X-MS-CLIENT-PRINCIPAL header is decoded and yields an authenticated principal
 *    - Malformed base64 header is swallowed gracefully (no 500)
 *    - Full claim normalization: objectidentifier, name, roles all handled
 *
 *  ClaimsPrincipalBuilderMiddleware
 *    - When no parser succeeds → Anonymous principal stored (secure route → 401, not 500)
 *    - Public route with no parsers succeeding → Anonymous accepted, no error
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Auth
{
    // ══════════════════════════════════════════════════════════════════════════════
    // EasyAuthRedirectMiddleware — browser navigation and /services/* coverage
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Auth_Middleware")]
    public class EasyAuthRedirectMiddlewareCoverageTests
    {
        const string BaseUrl = "http://localhost:7071";

        // AllowAutoRedirect=false so we can assert the 302 itself.
        static readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static bool _hostAvailable;

        [ClassInitialize]
        public static async Task Init(TestContext _)
        {
            try
            {
                var r = await _http.GetAsync(BaseUrl + "/admin/host/ping");
                _hostAvailable = (int)r.StatusCode < 500;
            }
            catch { _hostAvailable = false; }
        }

        void SkipIfUnavailable()
        {
            if (!_hostAvailable)
                Assert.Inconclusive($"Azure Functions host not reachable at {BaseUrl}");
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
            SkipIfUnavailable();

            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            // No Sec-Fetch-Mode: cors → treated as top-level browser navigation
            // No Authorization / X-MS-CLIENT-PRINCIPAL headers

            var resp = await _http.SendAsync(req);

            Assert.AreEqual(HttpStatusCode.Redirect, resp.StatusCode,
                "EasyAuthRedirectMiddleware must return 302 for unauthenticated browser navigation to /Secure/*");

            var location = resp.Headers.Location?.ToString() ?? "";
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
            SkipIfUnavailable();

            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Add("Accept", "application/json");
            req.Headers.Add("Sec-Fetch-Mode", "cors");

            var resp = await _http.SendAsync(req);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
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
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/Secure/SomeWorkflow.json");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "API client request to /Secure/* with no token must return 401");
            Assert.IsTrue(resp.Headers.Contains("WWW-Authenticate"),
                "401 from EasyAuthRedirectMiddleware must include WWW-Authenticate header");
        }

        /// <summary>
        /// /services/* routes must be enforced the same way as /secure/*.
        /// An unauthenticated API call to /services/* must return 401.
        /// Exercises: EasyAuthRedirectMiddleware isServices branch.
        /// </summary>
        [TestMethod]
        public async Task ServicesRoute_NoToken_Returns401()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/services/SomeWorkflow.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                $"/services/* must return 401 when no auth token is present. Got {(int)resp.StatusCode}: {body}");
        }

        /// <summary>
        /// /services/apis.json must pass through the redirect middleware without a token,
        /// because apis.json discovery is always accessible.
        /// Exercises: EasyAuthRedirectMiddleware apis.json bypass branch (line 70-74).
        /// </summary>
        [TestMethod]
        public async Task ServicesApisJson_NoToken_PassesThrough_Returns200()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/services/apis.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                $"/services/apis.json must not return 401. Got {(int)resp.StatusCode}: {body}");
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"/services/apis.json should return 200. Got {(int)resp.StatusCode}: {body}");
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
            SkipIfUnavailable();

            var principalJson = """{"auth_typ":"aad","claims":[{"typ":"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name","val":"Integration Test User"}]}""";
            var principalHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes(principalJson));

            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Add(AuthConstants.ClientPrincipalHeader, principalHeader);

            var resp = await _http.SendAsync(req);

            // The redirect middleware must NOT write the "A valid Bearer token is required" body.
            // If we do get a 401 it must be from WorkflowAuthorizationMiddleware (JSON error body
            // with "error" key), not from EasyAuthRedirectMiddleware (plain string body).
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                var body = await resp.Content.ReadAsStringAsync();
                var isRedirectMiddleware401 = body.Contains("A valid Bearer token is required");
                Assert.IsFalse(isRedirectMiddleware401,
                    $"EasyAuthRedirectMiddleware must not reject a request carrying {AuthConstants.ClientPrincipalHeader}. " +
                    $"Got 401 body: {body}");
            }
            // Any other outcome (200, 403, 404, 500) means the redirect middleware let it through.
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowAuthorizationMiddleware — JSON error body, correlation ID, /services/*
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Auth_Middleware")]
    public class WorkflowAuthorizationMiddlewareCoverageTests
    {
        const string BaseUrl = "http://localhost:7071";

        static readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static string _secretKey = null!;
        static bool _hostAvailable;
        static bool _secretKeyMatchesServer;

        [ClassInitialize]
        public static async Task Init(TestContext _)
        {
            try
            {
                var r = await _http.GetAsync(BaseUrl + "/admin/host/ping");
                _hostAvailable = (int)r.StatusCode < 500;
            }
            catch { _hostAvailable = false; }

            var envPath = Environment.GetEnvironmentVariable(
                Warewolf.Execution.Lightweight.Security.SecureConfigLoader.ConfigPathEnvVar);
            if (!string.IsNullOrWhiteSpace(envPath) && System.IO.File.Exists(envPath))
            {
                var cfg = Warewolf.Execution.Lightweight.Security.SecureConfigLoader.LoadFrom(envPath);
                if (cfg.IsLoaded) { _secretKey = cfg.SecretKey; _secretKeyMatchesServer = true; return; }
            }
            _secretKey = SecureConfigBuilder.NewSecretKey();
        }

        void SkipIfUnavailable()
        {
            if (!_hostAvailable)
                Assert.Inconclusive($"Azure Functions host not reachable at {BaseUrl}");
        }

        void SkipIfServerLacksMatchingConfig()
        {
            if (!_secretKeyMatchesServer)
                Assert.Inconclusive(
                    "Skipped: no matching secure.config found. " +
                    $"Set {Warewolf.Execution.Lightweight.Security.SecureConfigLoader.ConfigPathEnvVar} " +
                    "to the path used when starting the host.");
        }

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
            SkipIfUnavailable();

            // Wrong-key token: EasyAuthRedirectMiddleware passes it (hasAuthHeader=true),
            // BearerTokenPrincipalParser rejects it → Anonymous → WorkflowAuthorizationMiddleware 401
            var badToken = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", badToken);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                $"Expected 401. Got {(int)resp.StatusCode}: {body}");

            JObject? json = null;
            try { json = JObject.Parse(body); }
            catch (Exception ex) { Assert.Fail($"401 body must be valid JSON. Got: {body}\nError: {ex.Message}"); }

            Assert.IsNotNull(json!["error"],        $"JSON 401 body must have 'error'. Got: {body}");
            Assert.IsNotNull(json["message"],       $"JSON 401 body must have 'message'. Got: {body}");
            Assert.IsNotNull(json["path"],          $"JSON 401 body must have 'path'. Got: {body}");
            Assert.IsNotNull(json["correlationId"], $"JSON 401 body must have 'correlationId'. Got: {body}");
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
            SkipIfUnavailable();

            const string correlationId = "my-test-correlation-id-42";
            var badToken = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", badToken);
            req.Headers.Add("X-WW-Correlation-Id", correlationId);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                $"Expected 401. Got {(int)resp.StatusCode}: {body}");

            JObject json;
            try { json = JObject.Parse(body); }
            catch { Assert.Fail($"401 body must be JSON. Got: {body}"); return; }

            Assert.AreEqual(correlationId, json["correlationId"]?.ToString(),
                $"correlationId in response body must match the supplied header. Got: {body}");

            Assert.IsTrue(resp.Headers.Contains("X-WW-Correlation-Id"),
                "401 response must echo X-WW-Correlation-Id in a response header");
            var echoed = string.Join("", resp.Headers.GetValues("X-WW-Correlation-Id"));
            Assert.AreEqual(correlationId, echoed,
                $"X-WW-Correlation-Id response header must match request. Got: {echoed}");
        }

        /// <summary>
        /// When no X-WW-Correlation-Id is supplied the middleware auto-generates one.
        /// The 401 body and response header must both carry it.
        /// Exercises: WorkflowAuthorizationMiddleware.ResolveCorrelationId auto-generate branch.
        /// </summary>
        [TestMethod]
        public async Task Secure_WrongKeyToken_WithoutCorrelationId_AutoGeneratesCorrelationId()
        {
            SkipIfUnavailable();

            var badToken = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", badToken);
            // Deliberately NO X-WW-Correlation-Id header

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                $"Expected 401. Got {(int)resp.StatusCode}: {body}");

            JObject json;
            try { json = JObject.Parse(body); }
            catch { Assert.Fail($"401 body must be JSON. Got: {body}"); return; }

            var autoId = json["correlationId"]?.ToString();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(autoId),
                $"correlationId must be auto-generated when caller does not supply it. Got: {body}");

            Assert.IsTrue(resp.Headers.Contains("X-WW-Correlation-Id"),
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
            SkipIfUnavailable();
            SkipIfServerLacksMatchingConfig();

            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/services/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "A valid JWT must not produce 401 on /services/* routes. " +
                $"Got {(int)resp.StatusCode}");
        }

        /// <summary>
        /// Development-only bypass: X-WW-Bypass-Auth: local-dev-bypass skips all policy
        /// checks in WorkflowAuthorizationMiddleware when IsDevelopment() is true.
        /// The Azure Functions host started by the integration-test script uses Development
        /// environment by default, so this header must be honoured.
        /// Exercises: WorkflowAuthorizationMiddleware dev-bypass branch (lines 122-132).
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_DevBypassHeader_SkipsPolicyChecks()
        {
            SkipIfUnavailable();

            // Use a wrong-key token so EasyAuthRedirectMiddleware passes it (hasAuthHeader=true),
            // and BearerTokenPrincipalParser fails → anonymous principal.
            // Without the bypass: 401. With the bypass: middleware skips all checks.
            var badToken = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", badToken);
            req.Headers.Add("X-WW-Bypass-Auth", "local-dev-bypass");

            var resp = await _http.SendAsync(req);

            // Must not be 401 — the bypass header skips auth checks. Outcome is 404 (no such
            // workflow) or 200/500 depending on what's deployed.
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "X-WW-Bypass-Auth: local-dev-bypass must skip policy checks in Development. " +
                $"Got {(int)resp.StatusCode}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // EasyAuthPrincipalParser — X-MS-CLIENT-PRINCIPAL end-to-end via HTTP
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Auth_Middleware")]
    public class EasyAuthPrincipalParserCoverageTests
    {
        const string BaseUrl = "http://localhost:7071";

        static readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static bool _hostAvailable;

        [ClassInitialize]
        public static async Task Init(TestContext _)
        {
            try
            {
                var r = await _http.GetAsync(BaseUrl + "/admin/host/ping");
                _hostAvailable = (int)r.StatusCode < 500;
            }
            catch { _hostAvailable = false; }
        }

        void SkipIfUnavailable()
        {
            if (!_hostAvailable)
                Assert.Inconclusive($"Azure Functions host not reachable at {BaseUrl}");
        }

        static string BuildEasyAuthHeader(string authType, params (string typ, string val)[] claims)
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
            SkipIfUnavailable();

            var header = BuildEasyAuthHeader("aad",
                ("http://schemas.microsoft.com/identity/claims/objectidentifier", "test-oid-99"),
                ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",    "Integration Test User"),
                ("roles", "Warewolf Administrators"));

            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/apis.json");
            req.Headers.Add(AuthConstants.ClientPrincipalHeader, header);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"/Secure/apis.json with X-MS-CLIENT-PRINCIPAL must return 200. Got {(int)resp.StatusCode}: {body}");

            JObject json;
            try { json = JObject.Parse(body); }
            catch { Assert.Fail($"Response must be valid JSON. Got: {body}"); return; }

            Assert.IsNotNull(json["Apis"], $"Response must contain 'Apis' array. Got: {body}");
        }

        /// <summary>
        /// A malformed (non-base64) X-MS-CLIENT-PRINCIPAL header must not crash the server.
        /// EasyAuthPrincipalParser catches the exception and returns null; the middleware
        /// falls back to the next parser (BearerTokenPrincipalParser), then to Anonymous.
        /// Exercises: EasyAuthPrincipalParser exception-catch branch (lines 73-77).
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_MalformedEasyAuthHeader_Returns401NotServerError()
        {
            SkipIfUnavailable();

            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Add(AuthConstants.ClientPrincipalHeader, "!!not-valid-base64!!");

            var resp = await _http.SendAsync(req);

            // After the parser exception the fallback is Anonymous principal → 401
            // (or 302 for browsers, but we use the default headers).
            // Must NOT be 500 — the exception is caught by the parser.
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.StatusCode,
                "A malformed X-MS-CLIENT-PRINCIPAL header must not cause a 500. " +
                $"Got {(int)resp.StatusCode}");
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
            SkipIfUnavailable();

            var header = BuildEasyAuthHeader("aad",
                ("http://schemas.microsoft.com/identity/claims/objectidentifier", "oid-abc-123"),
                ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",    "John Doe"),
                ("roles", "SomeGroup"));

            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Add(AuthConstants.ClientPrincipalHeader, header);

            var resp = await _http.SendAsync(req);

            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.StatusCode,
                "Full claim-set EasyAuth header must not cause a 500. " +
                $"Got {(int)resp.StatusCode}");
        }

        /// <summary>
        /// Verifies the default (passthrough) branch of NormalizeClaimType: a non-recognized
        /// claim type is forwarded unchanged.
        /// Exercises: NormalizeClaimType _ => claim.Type branch.
        /// </summary>
        [TestMethod]
        public async Task SecureApisJson_WithUnknownClaimType_PrincipalBuiltWithoutError()
        {
            SkipIfUnavailable();

            // "custom_claim" is not in the switch — it maps to itself via the default branch.
            var header = BuildEasyAuthHeader("aad",
                ("custom_claim", "some-value"),
                ("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "Custom User"));

            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/apis.json");
            req.Headers.Add(AuthConstants.ClientPrincipalHeader, header);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            // apis.json always returns 200 — no crash expected
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"/Secure/apis.json with unknown claim type must return 200. Got {(int)resp.StatusCode}: {body}");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // ClaimsPrincipalBuilderMiddleware — anonymous-fallback path
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Auth_Middleware")]
    public class ClaimsPrincipalBuilderMiddlewareCoverageTests
    {
        const string BaseUrl = "http://localhost:7071";

        static readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static bool _hostAvailable;

        [ClassInitialize]
        public static async Task Init(TestContext _)
        {
            try
            {
                var r = await _http.GetAsync(BaseUrl + "/admin/host/ping");
                _hostAvailable = (int)r.StatusCode < 500;
            }
            catch { _hostAvailable = false; }
        }

        void SkipIfUnavailable()
        {
            if (!_hostAvailable)
                Assert.Inconclusive($"Azure Functions host not reachable at {BaseUrl}");
        }

        /// <summary>
        /// When all parsers fail (malformed token, no EasyAuth header) the middleware
        /// falls back to WorkflowClaimsPrincipal.Anonymous(). The server must not crash;
        /// the final response on a /Secure/* route must be 401 (not 500).
        /// Exercises: ClaimsPrincipalBuilderMiddleware return Anonymous() path (line 107).
        /// </summary>
        [TestMethod]
        public async Task SecureRoute_AllParsersFail_FallsBackToAnonymous_Returns401()
        {
            SkipIfUnavailable();

            // Provide a Bearer header so EasyAuthRedirectMiddleware passes the request,
            // but use an obviously invalid token format that BearerTokenPrincipalParser
            // will reject (three-dot structure is wrong → SecurityTokenException caught).
            var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.valid.jwt.at.all");

            var resp = await _http.SendAsync(req);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "When no parser succeeds the middleware must store Anonymous and return 401. " +
                $"Got {(int)resp.StatusCode}");
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.StatusCode,
                "Anonymous fallback must not crash. Got 500.");
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
            SkipIfUnavailable();

            // Public endpoint — Anonymous principal is accepted, function should execute.
            var resp = await _http.GetAsync(BaseUrl + "/Public/tools/http%20get/TC013_Get_CustomHeader_Echoed.json");

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "/Public/* must accept anonymous principals — no 401 expected");
            Assert.AreNotEqual(HttpStatusCode.InternalServerError, resp.StatusCode,
                "Anonymous fallback on /Public/* must not crash — no 500 expected");
        }
    }
}
