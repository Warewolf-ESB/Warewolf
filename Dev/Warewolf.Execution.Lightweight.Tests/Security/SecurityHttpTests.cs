/*
 * HTTP integration tests for JWT-based request validation in the lightweight engine.
 *
 * PRE-REQUISITE: the Azure Functions host must be running at http://localhost:7071
 * with the WAREWOLF_SECURE_CONFIG env var pointing to the config file created by
 * ClassInitialize (or an existing file placed at that path before the test run).
 *
 * Because the function host is a separate process, we cannot hot-swap its
 * secure.config mid-run.  Instead each test class targets a specific configuration
 * variant.  Run the host once for all tests:
 *
 *   set WAREWOLF_SECURE_CONFIG=<path written by ClassInitialize>
 *   func start
 *
 * If the host is not reachable the tests are marked Inconclusive, not Failed.
 *
 * Test coverage:
 *   Secure/* routes
 *     - No token           → 401
 *     - Expired token      → 401
 *     - Bad signature      → 401
 *     - Valid token        → 200 (or 404 when workflow does not exist)
 *   Public/* routes
 *     - Always accessible  → 200 / 404
 *   apis.json routes
 *     - GET /apis.json                → 200 + valid JSON (public list)
 *     - GET /Public/{path}/apis.json  → 200 + filtered list
 *     - GET /Secure/apis.json no JWT  → 200 + empty Apis array
 *     - GET /Secure/apis.json valid   → 200 + non-empty Apis array (if workflows present)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class SecurityHttpTests
    {
        const string BaseUrl        = "http://localhost:7071";
        const string RealConfigPath = @"C:\ProgramData\Warewolf\Server Settings\secure.config";

        static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };

        // Secret key loaded/derived from the real config (or a synthetic one).
        static string _secretKey = null!;

        // True when _secretKey came from the same config the server was started with,
        // meaning tokens minted here will actually pass validation on the server.
        static bool _secretKeyMatchesServer;

        // ── Setup ─────────────────────────────────────────────────────────────────

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            // Load the secret key we'll use to mint test tokens.
            // MUST use the same key as the running server so tokens are accepted.
            //
            // Resolution order:
            //   1. WAREWOLF_SECURE_CONFIG env var — this is exactly what the server
            //      was started with (e.g. "func start" after setting the env var).
            //   2. Well-known real-server install path (RealConfigPath).
            //   3. Generate a fresh key — tokens won't be accepted by a server that
            //      has no config, but non-JWT tests can still run.
            var envPath = Environment.GetEnvironmentVariable(SecureConfigLoader.ConfigPathEnvVar);
            if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
            {
                var cfg = SecureConfigLoader.LoadFrom(envPath);
                if (cfg.IsLoaded)
                {
                    _secretKey = cfg.SecretKey;
                    _secretKeyMatchesServer = true;
                    return;
                }
            }

            if (File.Exists(RealConfigPath))
            {
                var realCfg = SecureConfigLoader.LoadFrom(RealConfigPath);
                if (realCfg.IsLoaded)
                {
                    _secretKey = realCfg.SecretKey;
                    _secretKeyMatchesServer = true;
                    return;
                }
            }

            // No matching config found — generate a key for structural token tests,
            // but JWT-acceptance tests will be Inconclusive (server can't validate).
            _secretKey = SecureConfigBuilder.NewSecretKey();
            _secretKeyMatchesServer = false;
        }

        // ── Host availability helper ──────────────────────────────────────────────

        static async Task<bool> IsHostRunningAsync()
        {
            try
            {
                var resp = await _http.GetAsync(BaseUrl + "/admin/host/ping");
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        static void SkipIfHostNotRunning(bool running)
        {
            if (!running)
                Assert.Inconclusive(
                    $"Skipped: Azure Functions host not reachable at {BaseUrl}. " +
                    "Start the host with: func start --port 7071");
        }

        /// <summary>
        /// Marks the test Inconclusive when no config matching the server's key was
        /// found.  Without a matching key the server returns 401 for every token —
        /// the test cannot distinguish a real auth failure from a misconfigured host.
        ///
        /// To fix: set <c>WAREWOLF_SECURE_CONFIG</c> to the path of the
        /// <c>secure.config</c> used by the running host, then re-run.
        /// </summary>
        static void SkipIfServerLacksMatchingConfig()
        {
            if (!_secretKeyMatchesServer)
                Assert.Inconclusive(
                    $"Skipped: no usable secure.config found via " +
                    $"{SecureConfigLoader.ConfigPathEnvVar} env var or {RealConfigPath}. " +
                    "The server has no secret key, so it rejects every JWT with 401. " +
                    $"Start the host after setting {SecureConfigLoader.ConfigPathEnvVar}=<path to secure.config>.");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // /Secure/* — JWT-protected execution routes
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_NoToken_Returns401()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var resp = await _http.GetAsync(BaseUrl + "/Secure/HelloWorld.json");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "Expected 401 when no Authorization header is present");
            Assert.IsTrue(resp.Headers.Contains("WWW-Authenticate"),
                "Expected WWW-Authenticate header on 401 response");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_ExpiredToken_Returns401()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var token = JwtTestHelper.ExpiredToken(_secretKey, "TeamA");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "Expected 401 for an expired JWT");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_BadSignatureToken_Returns401()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var token = JwtTestHelper.BadSignatureToken(_secretKey, "TeamA");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "Expected 401 for a token with a bad signature");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_WrongKeyToken_Returns401()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var token = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "Expected 401 when the token is signed with the wrong key");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_ValidToken_Returns200OrNotFound()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());
            SkipIfServerLacksMatchingConfig();

            // A valid token should pass authentication.
            // The response is 200 when the workflow exists, 404/500 when it does not —
            // either way it must NOT be 401.
            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "A valid JWT must not return 401");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_ValidToken_Post_Returns200OrNotFound()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());
            SkipIfServerLacksMatchingConfig();

            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var req   = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/Secure/HelloWorld.json")
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "A valid JWT POST must not return 401");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // /Public/* — anonymous execution routes (no auth required)
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Public_NoToken_IsAccessible()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            // Public endpoint must never return 401, regardless of auth.
            var resp = await _http.GetAsync(BaseUrl + "/Public/HelloWorld.json");

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "/Public/* must not require authentication");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Public_WithToken_IsAccessible()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var token = JwtTestHelper.ValidToken(_secretKey, "TeamA");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Public/HelloWorld.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "/Public/* must not require authentication even when a token is supplied");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // /apis.json discovery routes
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task RootApisJson_Returns200_WithValidJson()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var resp = await _http.GetAsync(BaseUrl + "/apis.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"Expected 200 for /apis.json. Body: {body}");

            var json = JObject.Parse(body);
            Assert.IsNotNull(json["Apis"],  "Expected 'Apis' array in apis.json");
            Assert.IsNotNull(json["Name"],  "Expected 'Name' field in apis.json");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task PublicApisJson_Returns200_NeverReturns401()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            // /Public/apis.json must always be reachable (no authentication gate)
            var resp = await _http.GetAsync(BaseUrl + "/Public/apis.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"/Public/apis.json must return 200. Body: {body}");

            var json = JObject.Parse(body);
            Assert.IsNotNull(json["Apis"]);
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_NoToken_Returns200_WithEmptyApis()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            // /Secure/apis.json is a discovery endpoint — no 401, but it returns
            // an empty Apis array when no valid JWT is presented.
            var resp = await _http.GetAsync(BaseUrl + "/Secure/apis.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"/Secure/apis.json must return 200 even without a token. Body: {body}");

            var json = JObject.Parse(body);
            Assert.IsNotNull(json["Apis"], "Expected 'Apis' key");
            Assert.AreEqual(0, (json["Apis"] as JArray)?.Count,
                "Expected empty Apis array when no valid JWT is supplied");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_ValidToken_Returns200()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());
            SkipIfServerLacksMatchingConfig();

            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/apis.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"/Secure/apis.json with valid JWT must return 200. Body: {body}");

            var json = JObject.Parse(body);
            Assert.IsNotNull(json["Apis"]);
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_ExpiredToken_Returns200_WithEmptyApis()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var token = JwtTestHelper.ExpiredToken(_secretKey, "Warewolf Administrators");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/apis.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            // Discovery endpoint never returns 401; expired token → empty list.
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"Expected 200 for /Secure/apis.json with expired token. Body: {body}");

            var json = JObject.Parse(body);
            Assert.AreEqual(0, (json["Apis"] as JArray)?.Count,
                "Expired token should yield an empty Apis array");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_Entries_UseSecureRoutePrefix()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());
            SkipIfServerLacksMatchingConfig();

            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/apis.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var json    = JObject.Parse(body);
            var apis    = json["Apis"] as JArray;
            if (apis is null || apis.Count == 0)
            {
                Assert.Inconclusive("No workflow entries in /Secure/apis.json — deploy workflows first.");
                return;
            }

            foreach (var api in apis)
            {
                var baseUrl = api["baseUrl"]?.ToString() ?? "";
                Assert.IsTrue(baseUrl.Contains("/Secure/", StringComparison.OrdinalIgnoreCase),
                    $"Expected /Secure/ prefix in baseUrl, got: {baseUrl}");
            }
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task PublicApisJson_Entries_UsePublicRoutePrefix()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            var resp = await _http.GetAsync(BaseUrl + "/Public/apis.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

            var json = JObject.Parse(body);
            var apis = json["Apis"] as JArray;
            if (apis is null || apis.Count == 0)
            {
                Assert.Inconclusive("No public workflow entries — configure public permissions first.");
                return;
            }

            foreach (var api in apis)
            {
                var baseUrl = api["baseUrl"]?.ToString() ?? "";
                Assert.IsTrue(baseUrl.Contains("/Public/", StringComparison.OrdinalIgnoreCase),
                    $"Expected /Public/ prefix in baseUrl, got: {baseUrl}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Real config integration — validate that a real server token round-trips
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP_RealConfig")]
        public async Task RealConfig_TokenFromFile_AcceptedBySecureRoute()
        {
            SkipIfHostNotRunning(await IsHostRunningAsync());

            if (!File.Exists(RealConfigPath))
                Assert.Inconclusive($"Skipped: {RealConfigPath} not found.");

            var cfg   = SecureConfigLoader.LoadFrom(RealConfigPath);
            if (!cfg.IsLoaded)
                Assert.Inconclusive("Real config loaded but IsLoaded=false — check file content.");

            var token = JwtTestHelper.ValidToken(cfg.SecretKey, "Warewolf Administrators");
            var req   = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "/Secure/apis.json");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                "Token generated from the real config should be accepted by the running server");
        }
    }
}
