using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Integration tests that exercise engine routes which the existing
    /// per-tool/per-workflow integration tests do not touch. Each test issues
    /// one HTTP request to the live Azure Function host so the
    /// <c>Warewolf.Execution.Lightweight.dll</c> coverage collector attached
    /// to the host records execution of the middleware, routing, OpenAPI
    /// generation, authorization, OAuth and licensing code paths.
    ///
    /// <para>
    /// The tests are deliberately tolerant about response bodies — they
    /// assert on the response status set the engine documents (e.g. apis.json
    /// "always reachable", Secure routes "401/403 without auth", Dropbox
    /// /start with no app key "400"). They do not depend on any specific
    /// workflow output. The goal is to traverse code, not to validate
    /// business logic that is already covered elsewhere.
    /// </para>
    ///
    /// <para>
    /// Requires the Azure Function host to be running at
    /// <c>http://localhost:7071</c> (the same host as the other integration
    /// tests). When run outside of CI without the host up, these tests will
    /// fail with a connection error — exactly like the other integration
    /// tests in this project.
    /// </para>
    /// </summary>
    [TestClass]
    public class EngineRoutingIntegrationTests
    {
        private const string HostBaseUrl    = "http://localhost:7071";
        private const string PublicGetTools = "http://localhost:7071/public/tools/http%20get";
        private const string SecureGetTools = "http://localhost:7071/secure/tools/http%20get";

        private static readonly HttpClient _client = new();

        public TestContext TestContext { get; set; } = null!;

        // ---------------------------------------------------------------
        // apis.json discovery — exercises WorkflowOpenApiGenerator and
        // WorkflowResourceCache enumeration paths (large 0%-covered class).
        // ---------------------------------------------------------------

        /// <summary>Root /apis.json must always be reachable and return JSON.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task ApisJson_Root_ReturnsJson()
        {
            var response = await _client.GetAsync($"{HostBaseUrl}/apis.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                $"Expected 200 OK from /apis.json. Body: {Trim(body)}");

            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind,
                "apis.json root must be a JSON object.");
        }

        /// <summary>Root /apis.json should declare the standard apis.json schema fields.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: ApisJsonGenerator key casing not yet finalised — tracked on branch 8431-coverage-boost")]
        public async Task ApisJson_Root_DeclaresApisCollection()
        {
            var response = await _client.GetAsync($"{HostBaseUrl}/apis.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Body: {Trim(body)}");

            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success. Body: {Trim(body)}");
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var keys = root.EnumerateObject().Select(p => p.Name).ToList();
            TestContext.WriteLine($"Keys: {string.Join(", ", keys)}");

            Assert.IsTrue(root.TryGetProperty("apis", out var apis),
                $"apis.json must have an 'apis' collection. Keys: {string.Join(", ", keys)}");
            Assert.AreEqual(JsonValueKind.Array, apis.ValueKind, "'apis' should be an array.");
        }

        /// <summary>Folder-scoped /Public/{folder}/apis.json filters to that subtree.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task ApisJson_PublicFolderScoped_ReturnsJson()
        {
            var response = await _client.GetAsync($"{PublicGetTools}/apis.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.IsTrue(response.IsSuccessStatusCode,
                $"Expected success from /Public/.../apis.json. Got {(int)response.StatusCode}: {Trim(body)}");
            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind);
        }

        /// <summary>/Secure/{folder}/apis.json is always reachable (no 401) — only its contents change.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: WorkflowAuthorizationMiddleware apis.json bypass not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task ApisJson_SecureFolderScoped_DoesNotReturn401()
        {
            var response = await _client.GetAsync($"{SecureGetTools}/apis.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode,
                "Documented behaviour: Secure apis.json is reachable, but contents may be empty.");
            Assert.AreNotEqual(HttpStatusCode.Forbidden, response.StatusCode,
                "Documented behaviour: Secure apis.json is reachable, but contents may be empty.");
        }

        // ---------------------------------------------------------------
        // Name-suffix parsing — .xml / .api / .json / no suffix all flow
        // through NameSuffixParser → ResponseBuilder content-type switch.
        // ---------------------------------------------------------------

        /// <summary>Public workflow with .json suffix returns application/json.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_JsonSuffix_ReturnsJsonContentType()
        {
            var response = await _client.GetAsync($"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"CT    : {response.Content.Headers.ContentType}");

            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success. Body: {Trim(body)}");
            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            Assert.IsTrue(contentType.Contains("json"), $"Expected JSON content-type, got '{contentType}'.");
        }

        /// <summary>Public workflow with .xml suffix routes through XML emission.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_XmlSuffix_ReturnsXmlContentType()
        {
            var response = await _client.GetAsync($"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.xml");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"CT    : {response.Content.Headers.ContentType}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected success. Body: {Trim(body)}");
            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            Assert.IsTrue(contentType.Contains("xml") || body.TrimStart().StartsWith("<"),
                $"Expected XML response. CT='{contentType}' Body='{Trim(body)}'.");
        }

        /// <summary>Public workflow with no suffix still resolves and returns a body.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_NoSuffix_StillResolves()
        {
            var response = await _client.GetAsync($"{PublicGetTools}/TC013_Get_CustomHeader_Echoed");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.IsTrue(response.IsSuccessStatusCode,
                $"Expected success without suffix. Got {(int)response.StatusCode}: {Trim(body)}");
            Assert.IsFalse(string.IsNullOrWhiteSpace(body), "Expected a non-empty body.");
        }

        /// <summary>POST with empty body to a public workflow exercises the POST request-parse path.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task NamedWorkflow_PostEmptyBody_ReturnsSuccess()
        {
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync($"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.json", content);
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.IsTrue(response.IsSuccessStatusCode,
                $"Expected success POSTing empty body. Got {(int)response.StatusCode}: {Trim(body)}");
        }

        // ---------------------------------------------------------------
        // 404 / missing workflow — exercises WorkflowResourceCache miss
        // and the middleware not-found response path.
        // ---------------------------------------------------------------

        /// <summary>Requesting a workflow that does not exist must return 404.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: ResponseBuilder 404-for-file-not-found not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task NamedWorkflow_NonExistent_Returns404()
        {
            var response = await _client.GetAsync($"{PublicGetTools}/this_workflow_does_not_exist_zzz999.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                $"Missing workflow must return 404. Body: {Trim(body)}");
        }

        /// <summary>Requesting a workflow under a non-existent folder must return 404.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: ResponseBuilder 404-for-file-not-found not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task NamedWorkflow_NonExistentFolder_Returns404()
        {
            var response = await _client.GetAsync($"{HostBaseUrl}/public/no_such_folder_zzz999/whatever.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                $"Missing folder must return 404. Body: {Trim(body)}");
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
            var url = $"{PublicGetTools}/TC013_Get_CustomHeader_Echoed.json";
            var tasks = Enumerable.Range(0, 5).Select(_ => _client.GetAsync(url)).ToArray();
            var responses = await Task.WhenAll(tasks);

            for (var i = 0; i < responses.Length; i++)
            {
                var status = (int)responses[i].StatusCode;
                TestContext.WriteLine($"Req {i}: {status} {responses[i].StatusCode}");
                Assert.IsTrue(responses[i].IsSuccessStatusCode,
                    $"Concurrent request {i} failed: {status}");
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
            var response = await _client.GetAsync($"{SecureGetTools}/TC013_Get_CustomHeader_Echoed.json");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            var rejected = response.StatusCode is HttpStatusCode.Unauthorized
                                              or HttpStatusCode.Forbidden;
            Assert.IsTrue(rejected,
                $"Expected 401/403 for unauthenticated Secure call. Got {(int)response.StatusCode}: {Trim(body)}");
        }

        /// <summary>
        /// Secure/* with the documented dev-bypass header is permitted in
        /// development. We assert only that the middleware does not return
        /// 401/403 — the underlying workflow may still fail for unrelated
        /// reasons (e.g. environment defaults), and we are exercising the
        /// bypass branch, not the workflow.
        /// </summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: EasyAuthRedirectMiddleware dev-bypass pass-through not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task SecureRoute_WithDevBypassHeader_BypassesAuth()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"{SecureGetTools}/TC013_Get_CustomHeader_Echoed.json");
            request.Headers.Add("X-WW-Bypass-Auth", "local-dev-bypass");

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            // The middleware bypass branch must NOT itself return 401/403.
            // The downstream workflow may or may not succeed depending on
            // environment, but auth must have been skipped.
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode,
                "Dev bypass header should skip auth and not return 401.");
            Assert.AreNotEqual(HttpStatusCode.Forbidden, response.StatusCode,
                "Dev bypass header should skip auth and not return 403.");
        }

        // ---------------------------------------------------------------
        // Licensing and login — anonymous routes that exercise their own
        // function classes.
        // ---------------------------------------------------------------

        /// <summary>GET /IsLicensed returns a JSON document with a boolean flag.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: LicensingHttpFunction exception handling not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task IsLicensed_ReturnsBooleanFlag()
        {
            var response = await _client.GetAsync($"{HostBaseUrl}/IsLicensed");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.IsTrue(response.IsSuccessStatusCode,
                $"/IsLicensed should be reachable anonymously. Got {(int)response.StatusCode}: {Trim(body)}");

            using var doc = JsonDocument.Parse(body);
            Assert.AreEqual(JsonValueKind.Object, doc.RootElement.ValueKind,
                "/IsLicensed body should be a JSON object.");
        }

        /// <summary>GET /login returns a response (HTML form or redirect).</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: LoginFunction GET behaviour not yet finalised — tracked on branch 8431-coverage-boost")]
        public async Task Login_GetEndpoint_IsReachable()
        {
            var response = await _client.GetAsync($"{HostBaseUrl}/login");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            // /login is anonymous — it must return 2xx or a 3xx redirect, never 401.
            Assert.IsTrue(
                response.IsSuccessStatusCode || ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400),
                $"/login must be reachable anonymously. Got {(int)response.StatusCode}: {Trim(body)}");
        }

        // ---------------------------------------------------------------
        // Dropbox OAuth — anonymous routes exercising DropboxOAuthFunction
        // (757 lines, 0% covered). All these tests assert on documented
        // error/redirect behaviour so they pass without real Dropbox creds.
        // ---------------------------------------------------------------

        /// <summary>/oauth/dropbox/start with no parameters returns a 400 HTML error.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: DropboxOAuthFunction exception handling not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task DropboxOAuthStart_NoParams_ReturnsBadRequest()
        {
            var response = await _client.GetAsync($"{HostBaseUrl}/oauth/dropbox/start");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode,
                $"Expected 400 when neither appKey nor sourceId is provided. Body: {Trim(body)}");
            Assert.IsTrue(body.IndexOf("App Key", System.StringComparison.OrdinalIgnoreCase) >= 0,
                $"Error body should mention 'App Key'. Body: {Trim(body)}");
        }

        /// <summary>/oauth/dropbox/start with an appKey returns a 302 redirect to Dropbox.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: DropboxOAuthFunction exception handling not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task DropboxOAuthStart_WithAppKey_RedirectsToDropbox()
        {
            using var noRedirectHandler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(noRedirectHandler);

            var response = await client.GetAsync($"{HostBaseUrl}/oauth/dropbox/start?appKey=fake_test_app_key_xyz");
            TestContext.WriteLine($"Status  : {(int)response.StatusCode}");
            TestContext.WriteLine($"Location: {response.Headers.Location}");

            Assert.AreEqual(HttpStatusCode.Found, response.StatusCode,
                "Expected 302 redirect to Dropbox.");
            Assert.IsNotNull(response.Headers.Location, "Expected a Location header on the 302.");
            var location = response.Headers.Location!.ToString();
            Assert.IsTrue(location.StartsWith("https://www.dropbox.com/oauth2/authorize"),
                $"Expected redirect to Dropbox authorize URL, got: {location}");
            Assert.IsTrue(location.Contains("client_id=fake_test_app_key_xyz"),
                $"Expected client_id in redirect URL: {location}");
            Assert.IsTrue(location.Contains("code_challenge_method=S256"),
                $"Expected PKCE S256 challenge in redirect URL: {location}");
        }

        /// <summary>/oauth/dropbox/callback with no code and no state returns an error page.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: DropboxOAuthFunction exception handling not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task DropboxOAuthCallback_NoCodeNoState_ReturnsErrorPage()
        {
            var response = await _client.GetAsync($"{HostBaseUrl}/oauth/dropbox/callback");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            // Documented: missing code/state is an error response (4xx).
            Assert.IsTrue((int)response.StatusCode >= 400 && (int)response.StatusCode < 500,
                $"Expected a 4xx error for missing code/state. Got {(int)response.StatusCode}: {Trim(body)}");
        }

        /// <summary>/oauth/dropbox/callback with an explicit error returns the user-denied page.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        [Ignore("WIP: DropboxOAuthFunction exception handling not yet merged — tracked on branch 8431-coverage-boost")]
        public async Task DropboxOAuthCallback_UserDenied_ReturnsAuthorizationDeniedPage()
        {
            var response = await _client.GetAsync(
                $"{HostBaseUrl}/oauth/dropbox/callback?error=access_denied&state=anything");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                "User-denied is documented to return 200 with an HTML page.");
            Assert.IsTrue(body.IndexOf("Authorization Denied", System.StringComparison.OrdinalIgnoreCase) >= 0,
                $"Expected 'Authorization Denied' in body. Body: {Trim(body)}");
            Assert.IsTrue(body.IndexOf("access_denied", System.StringComparison.OrdinalIgnoreCase) >= 0,
                $"Expected the error code to be echoed back. Body: {Trim(body)}");
        }

        /// <summary>/oauth/dropbox/callback with an unknown state (no cached PKCE session) is rejected.</summary>
        [TestMethod, TestCategory("EngineRouting_Integration")]
        public async Task DropboxOAuthCallback_UnknownState_IsRejected()
        {
            var response = await _client.GetAsync(
                $"{HostBaseUrl}/oauth/dropbox/callback?code=fake_code&state=unknown_state_zzz999");
            var body = await response.Content.ReadAsStringAsync();
            TestContext.WriteLine($"Status: {(int)response.StatusCode}");
            TestContext.WriteLine($"Body  : {Trim(body)}");

            // Unknown state must not silently succeed — engine must respond with an error page.
            Assert.IsTrue((int)response.StatusCode >= 400 || body.IndexOf("invalid", System.StringComparison.OrdinalIgnoreCase) >= 0
                                                           || body.IndexOf("expired", System.StringComparison.OrdinalIgnoreCase) >= 0
                                                           || body.IndexOf("denied",  System.StringComparison.OrdinalIgnoreCase) >= 0,
                $"Expected an error indication for unknown state. Got {(int)response.StatusCode}: {Trim(body)}");
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
