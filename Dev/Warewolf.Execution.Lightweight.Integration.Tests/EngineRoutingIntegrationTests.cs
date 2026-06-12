using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Integration tests that exercise engine routes which the existing
    /// per-tool/per-workflow integration tests do not touch.
    ///
    /// <para>
    /// The WORKFLOW / apis.json / Secure route families now run in-process
    /// through <see cref="LightweightInProcessHost"/>, which composes the real
    /// worker middleware pipeline (EasyAuthRedirect → ClaimsPrincipalBuilder →
    /// WorkflowAuthorization → function dispatch) and dispatches by route. No
    /// external Azure Function host on http://localhost:7071 is required for
    /// those tests.
    /// </para>
    ///
    /// <para>
    /// The special-function families (Licensing / Login / Dropbox OAuth) are
    /// still [Ignore]d: the in-process harness only wires
    /// <c>WorkflowHttpFunction</c>, not the LicensingHttpFunction /
    /// LoginFunction / DropboxOAuthFunction classes, so they require a running
    /// engine and are out of the current scope (Phase 3 — WOLF-8418).
    /// </para>
    ///
    /// <para>
    /// The tests are deliberately tolerant about response bodies — they assert
    /// on the documented response status (e.g. apis.json "always reachable",
    /// Secure routes "401/403 without auth"). They do not depend on any specific
    /// workflow output. The goal is to traverse code, not to validate business
    /// logic that is already covered elsewhere.
    /// </para>
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class EngineRoutingIntegrationTests
    {
        // Named-workflow under the "tools/http get" folder; decoded space — the
        // pipeline accepts URL-decoded paths directly.
        private const string PublicGetTools = "/public/tools/http get";
        private const string SecureGetTools = "/secure/tools/http get";

        private LightweightInProcessHost _host = null!;

        public TestContext TestContext { get; set; } = null!;

        [TestInitialize]
        public void Init() => _host = LightweightInProcessHost.WithPublicExecuteAll();

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        // ---------------------------------------------------------------
        // apis.json discovery — exercises WorkflowOpenApiGenerator and
        // WorkflowResourceCache enumeration paths (large 0%-covered class).
        // ---------------------------------------------------------------

        /// <summary>Root /apis.json must always be reachable and return JSON.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task ApisJson_Root_ReturnsJson()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/apis.json");
            TestContext.WriteLine($"Status: {(int)resp.Status} {resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"Expected 200 OK from /apis.json. Body: {Trim(resp.Body)}");

            using var doc = JsonDocument.Parse(resp.Body);
            Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind,
                "apis.json root must be a JSON object.");
        }

        /// <summary>Root /apis.json should declare the standard apis.json schema fields.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task ApisJson_Root_DeclaresApisCollection()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/apis.json");
            TestContext.WriteLine($"Body: {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"Expected success. Body: {Trim(resp.Body)}");
            using var doc = JsonDocument.Parse(resp.Body);
            var root = doc.RootElement;

            var keys = root.EnumerateObject().Select(p => p.Name).ToList();
            TestContext.WriteLine($"Keys: {string.Join(", ", keys)}");

            Assert.IsTrue(root.TryGetProperty("Apis", out var apis),
                $"apis.json must have an 'apis' collection. Keys: {string.Join(", ", keys)}");
            Assert.AreEqual(JsonValueKind.Array, apis.ValueKind, "'apis' should be an array.");
        }

        /// <summary>Folder-scoped /Public/{folder}/apis.json filters to that subtree.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task ApisJson_PublicFolderScoped_ReturnsJson()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", $"{PublicGetTools}/apis.json");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"Expected success from /Public/.../apis.json. Got {(int)resp.Status}: {Trim(resp.Body)}");
            using var doc = JsonDocument.Parse(resp.Body);
            Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind);
        }

        /// <summary>
        /// /Secure/{folder}/apis.json without a token is rejected with 401 — the current contract,
        /// matching SecurityHttpTests.SecureApisJson_NoToken_Returns401. The documented
        /// "always reachable, empty contents" apis.json bypass is unmerged (WOLF-8418).
        /// </summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task ApisJson_SecureFolderScoped_NoToken_Returns401()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", $"{SecureGetTools}/apis.json");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"Secure apis.json without a token returns 401 (current contract). Body: {Trim(resp.Body)}");
        }

        // ---------------------------------------------------------------
        // Name-suffix parsing — .xml / .api / .json / no suffix all flow
        // through NameSuffixParser → ResponseBuilder content-type switch.
        // ---------------------------------------------------------------

        /// <summary>Public workflow with .json suffix returns application/json.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_JsonSuffix_ReturnsJsonContentType()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", $"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.json");
            resp.Headers.TryGetValue("Content-Type", out var contentType);
            contentType ??= string.Empty;
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"CT    : {contentType}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"Expected success. Body: {Trim(resp.Body)}");
            Assert.IsTrue(contentType.Contains("json"), $"Expected JSON content-type, got '{contentType}'.");
        }

        /// <summary>Public workflow with .xml suffix routes through XML emission.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_XmlSuffix_ReturnsXmlContentType()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", $"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.xml");
            resp.Headers.TryGetValue("Content-Type", out var contentType);
            contentType ??= string.Empty;
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"CT    : {contentType}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"Expected success. Body: {Trim(resp.Body)}");
            Assert.IsTrue(contentType.Contains("xml") || resp.Body.TrimStart().StartsWith("<"),
                $"Expected XML response. CT='{contentType}' Body='{Trim(resp.Body)}'.");
        }

        /// <summary>Public workflow with no suffix still resolves and returns a body.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_NoSuffix_StillResolves()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", $"{PublicGetTools}/TC013_Get_CustomHeader_Echoed");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"Expected success without suffix. Got {(int)resp.Status}: {Trim(resp.Body)}");
            Assert.IsFalse(string.IsNullOrWhiteSpace(resp.Body), "Expected a non-empty body.");
        }

        /// <summary>POST with empty body to a public workflow exercises the POST request-parse path.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_PostEmptyBody_ReturnsSuccess()
        {
            var resp = await _host.SendThroughPipelineAsync("POST", $"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.json");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"Expected success POSTing empty body. Got {(int)resp.Status}: {Trim(resp.Body)}");
        }

        // ---------------------------------------------------------------
        // 404 / missing workflow — exercises WorkflowResourceCache miss
        // and the middleware not-found response path.
        // ---------------------------------------------------------------

        /// <summary>
        /// A non-existent workflow currently surfaces as 500 with an error body — the current
        /// contract, matching CoreInfra.ExecutePublicWorkflow_NonExistentWorkflow_Returns500WithErrorBody.
        /// The documented 404-for-missing-file response is unmerged (WOLF-8418).
        /// </summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_NonExistent_Returns500WithErrorBody()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", $"{PublicGetTools}/this_workflow_does_not_exist_zzz999.json");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.InternalServerError, resp.Status,
                $"Missing workflow currently returns 500. Body: {Trim(resp.Body)}");
        }

        /// <summary>
        /// A workflow under a non-existent folder currently returns 500 (404-for-missing-file
        /// is unmerged — WOLF-8418).
        /// </summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_NonExistentFolder_Returns500()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/public/no_such_folder_zzz999/whatever.json");
            TestContext.WriteLine($"Status: {(int)resp.Status}");

            Assert.AreEqual(HttpStatusCode.InternalServerError, resp.Status,
                $"Missing folder currently returns 500. Body: {Trim(resp.Body)}");
        }

        // ---------------------------------------------------------------
        // Cache concurrency — issuing the same workflow request several
        // times in parallel exercises WorkflowResourceCache concurrency
        // paths after the cache is warm.
        // ---------------------------------------------------------------

        /// <summary>Multiple concurrent requests against the same workflow all succeed.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_ConcurrentRequests_AllSucceed()
        {
            var path = $"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.json";

            // Sequential calls against the shared in-process singletons avoid
            // shared-singleton races while still exercising the warm-cache path.
            for (var i = 0; i < 5; i++)
            {
                var resp = await _host.SendThroughPipelineAsync("GET", path);
                TestContext.WriteLine($"Req {i}: {(int)resp.Status} {resp.Status}");
                Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                    $"Concurrent request {i} failed: {(int)resp.Status}");
            }
        }

        // ---------------------------------------------------------------
        // Authorization middleware — Secure/* without credentials must
        // be rejected; dev-bypass header must let it through.
        // ---------------------------------------------------------------

        /// <summary>Secure/* without credentials is rejected by WorkflowAuthorizationMiddleware.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task SecureRoute_NoCredentials_IsRejected()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", $"{SecureGetTools}/TC013_Get_CustomHeader_Echoed.json");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            var rejected = resp.Status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
            Assert.IsTrue(rejected,
                $"Expected 401/403 for unauthenticated Secure call. Got {(int)resp.Status}: {Trim(resp.Body)}");
        }

        /// <summary>
        /// Secure/* with the dev-bypass header currently still returns 401: the
        /// EasyAuthRedirect dev-bypass pass-through is unmerged (WOLF-8418), so an
        /// unauthenticated secure request is rejected even with the bypass header.
        /// </summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task SecureRoute_WithDevBypassHeader_CurrentlyRejected_Returns401()
        {
            var headers = new Dictionary<string, string> { ["X-WW-Bypass-Auth"] = "local-dev-bypass" };
            var resp = await _host.SendThroughPipelineAsync(
                "GET", $"{SecureGetTools}/TC013_Get_CustomHeader_Echoed.json", headers, isDevelopment: true);
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            // Current contract: the dev-bypass pass-through in EasyAuthRedirectMiddleware is not
            // yet merged, so the unauthenticated secure request is rejected with 401 (WOLF-8418).
            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"Dev-bypass pass-through is unmerged; secure route still returns 401. Body: {Trim(resp.Body)}");
        }

        // ---------------------------------------------------------------
        // Licensing and login — anonymous routes exercising their own
        // function classes, now wired into the in-process harness (Phase 3).
        // ---------------------------------------------------------------

        /// <summary>GET /IsLicensed returns a JSON document with a boolean flag.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task IsLicensed_ReturnsBooleanFlag()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/IsLicensed");
            TestContext.WriteLine($"Status: {(int)resp.Status} {resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"/IsLicensed should be reachable anonymously. Got {(int)resp.Status}: {Trim(resp.Body)}");

            using var doc = JsonDocument.Parse(resp.Body);
            Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind,
                "/IsLicensed body should be a JSON object.");
        }

        /// <summary>GET /login returns a response (HTML form or redirect), never 401.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task Login_GetEndpoint_IsReachable()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/login");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"/login is anonymous and must never return 401. Got {(int)resp.Status}: {Trim(resp.Body)}");
        }

        // ---------------------------------------------------------------
        // Dropbox OAuth — anonymous routes exercising DropboxOAuthFunction
        // (now wired into the in-process harness — Phase 3).
        // ---------------------------------------------------------------

        /// <summary>/oauth/dropbox/start with no parameters returns a 400 HTML error.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task DropboxOAuthStart_NoParams_ReturnsBadRequest()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/oauth/dropbox/start");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.BadRequest, resp.Status,
                $"Expected 400 when neither appKey nor sourceId is provided. Body: {Trim(resp.Body)}");
            Assert.IsTrue(resp.Body.IndexOf("App Key", System.StringComparison.OrdinalIgnoreCase) >= 0,
                $"Error body should mention 'App Key'. Body: {Trim(resp.Body)}");
        }

        /// <summary>/oauth/dropbox/start with an appKey returns a 302 redirect to Dropbox.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task DropboxOAuthStart_WithAppKey_RedirectsToDropbox()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/oauth/dropbox/start?appKey=fake_test_app_key_xyz");
            TestContext.WriteLine($"Status  : {(int)resp.Status}");

            Assert.AreEqual(HttpStatusCode.Found, resp.Status, "Expected 302 redirect to Dropbox.");
            Assert.IsTrue(resp.Headers.TryGetValue("Location", out var location) && !string.IsNullOrEmpty(location),
                "Expected a Location header on the 302.");
            TestContext.WriteLine($"Location: {location}");
            Assert.IsTrue(location!.StartsWith("https://www.dropbox.com/oauth2/authorize"),
                $"Expected redirect to Dropbox authorize URL, got: {location}");
            Assert.IsTrue(location.Contains("client_id=fake_test_app_key_xyz"),
                $"Expected client_id in redirect URL: {location}");
            Assert.IsTrue(location.Contains("code_challenge_method=S256"),
                $"Expected PKCE S256 challenge in redirect URL: {location}");
        }

        /// <summary>/oauth/dropbox/callback with no code and no state returns an error page.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task DropboxOAuthCallback_NoCodeNoState_ReturnsErrorPage()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/oauth/dropbox/callback");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.IsTrue((int)resp.Status >= 400 && (int)resp.Status < 500,
                $"Expected a 4xx error for missing code/state. Got {(int)resp.Status}: {Trim(resp.Body)}");
        }

        /// <summary>/oauth/dropbox/callback with an explicit error returns the user-denied page.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task DropboxOAuthCallback_UserDenied_ReturnsAuthorizationDeniedPage()
        {
            var resp = await _host.SendThroughPipelineAsync(
                "GET", "/oauth/dropbox/callback?error=access_denied&state=anything");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                "User-denied is documented to return 200 with an HTML page.");
            Assert.IsTrue(resp.Body.IndexOf("Authorization Denied", System.StringComparison.OrdinalIgnoreCase) >= 0,
                $"Expected 'Authorization Denied' in body. Body: {Trim(resp.Body)}");
            Assert.IsTrue(resp.Body.IndexOf("access_denied", System.StringComparison.OrdinalIgnoreCase) >= 0,
                $"Expected the error code to be echoed back. Body: {Trim(resp.Body)}");
        }

        /// <summary>/oauth/dropbox/callback with an unknown state (no cached PKCE session) is rejected.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task DropboxOAuthCallback_UnknownState_IsRejected()
        {
            var resp = await _host.SendThroughPipelineAsync(
                "GET", "/oauth/dropbox/callback?code=fake_code&state=unknown_state_zzz999");
            TestContext.WriteLine($"Status: {(int)resp.Status}");
            TestContext.WriteLine($"Body  : {Trim(resp.Body)}");

            var isEmptyNoContent = resp.Status == HttpStatusCode.NoContent && string.IsNullOrEmpty(resp.Body);
            Assert.IsTrue((int)resp.Status >= 400
                          || resp.Body.IndexOf("invalid", System.StringComparison.OrdinalIgnoreCase) >= 0
                          || resp.Body.IndexOf("expired", System.StringComparison.OrdinalIgnoreCase) >= 0
                          || resp.Body.IndexOf("denied",  System.StringComparison.OrdinalIgnoreCase) >= 0
                          || isEmptyNoContent,
                $"Expected an error indication (or empty 204) for unknown state. Got {(int)resp.Status}: {Trim(resp.Body)}");
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static string Trim(string body)
            => body is null ? "<null>"
                            : body.Length <= 400 ? body
                                                 : body.Substring(0, 400) + "...";
    }
}
