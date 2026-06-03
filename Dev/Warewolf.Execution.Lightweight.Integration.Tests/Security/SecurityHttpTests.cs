/*
 * In-process HTTP integration tests for JWT-based request validation in the lightweight engine.
 *
 * No longer requires a running engine on http://localhost:7071. LightweightInProcessHost runs
 * the real worker middleware pipeline (EasyAuthRedirect → ClaimsPrincipalBuilder →
 * WorkflowAuthorization → function) in-process against a secure.config seeded per test, and the
 * Warewolf HMAC JWT parser validates tokens minted with the host's SecretKey — so the
 * previously-required "matching server config" is now guaranteed and the SkipIf guards are gone.
 *
 * Config seeded here (mirrors the original Studio setup the comments described):
 *   • "Warewolf Administrators" — global full permissions
 *   • "Azure Functions Users"   — global View+Execute
 *   • "Public"                  — global Execute (no global View) + resource View on "Hello World"
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Security
{
    [TestClass]
    [DoNotParallelize]
    public class SecurityHttpTests
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
                SecureConfigBuilder.ServerPerm("Azure Functions Users", View: true, Execute: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: false, Execute: true),
                SecureConfigBuilder.ResourcePerm(SecureConfigBuilder.PublicGroup, "Hello World", View: true));
            _host = LightweightInProcessHost.WithSettings(settings);
        }

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        private static Dictionary<string, string> Bearer(string token) =>
            new() { ["Authorization"] = "Bearer " + token };

        // ══════════════════════════════════════════════════════════════════════════
        // /Secure/* — JWT-protected execution routes
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_NoToken_Returns401()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                "Expected 401 when no Authorization header is present");
            Assert.IsTrue(resp.Headers.ContainsKey("WWW-Authenticate"),
                "Expected WWW-Authenticate header on 401 response");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_ExpiredToken_Returns401()
        {
            var token = JwtTestHelper.ExpiredToken(_secretKey, "TeamA");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status, "Expected 401 for an expired JWT");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_BadSignatureToken_Returns401()
        {
            var token = JwtTestHelper.BadSignatureToken(_secretKey, "TeamA");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status, "Expected 401 for a token with a bad signature");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_WrongKeyToken_Returns401()
        {
            var token = JwtTestHelper.WrongKeyToken(_secretKey, "TeamA");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                "Expected 401 when the token is signed with the wrong key");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_ValidToken_Returns200OrNotFound()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/HelloWorld.json", Bearer(token));

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status, "A valid JWT must not return 401");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_ValidToken_Post_Returns200OrNotFound()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var resp  = await _host.SendThroughPipelineAsync("POST", "/Secure/HelloWorld.json", Bearer(token));

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status, "A valid JWT POST must not return 401");
        }

        // ── Group permission tests ─────────────────────────────────────────────────

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_AzureFunctionsUsers_ValidToken_Returns200OrNotFound()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Azure Functions Users");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/Hello%20World.json", Bearer(token));

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status, "'Azure Functions Users' JWT must not return 401");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_AzureFunctionsUsers_ShowsWorkflows()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Azure Functions Users");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"Expected 200 for /Secure/apis.json. Body: {resp.Body}");

            var apis = JObject.Parse(resp.Body)["Apis"] as JArray;
            Assert.IsNotNull(apis);

            if (apis.Count == 0)
                Assert.Inconclusive("No .bite files deployed to Resources\\ — deploy workflows first.");

            foreach (var api in apis)
            {
                var baseUrl = api["baseUrl"]?.ToString() ?? "";
                Assert.IsTrue(baseUrl.Contains("/Secure/", System.StringComparison.OrdinalIgnoreCase),
                    $"Expected /Secure/ in baseUrl, got: {baseUrl}");
            }
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Secure_UnknownGroup_ValidToken_PassesAuthGate()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "UnknownGroup");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/Hello%20World.json", Bearer(token));

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                "A valid JWT must pass the auth gate regardless of group membership");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_UnknownGroup_ReturnsEmptyApis()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "UnknownGroup");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"Discovery endpoint must not return 401. Body: {resp.Body}");

            Assert.AreEqual(0, (JObject.Parse(resp.Body)["Apis"] as JArray)?.Count,
                "A group with no configured permissions must see an empty Apis array");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_PublicGroupJwt_ShowsOnlyResourcePermittedWorkflows()
        {
            // /Secure/apis.json returns only the workflows the caller's role can both View AND
            // Execute (discovery requires both — PermissionChecker.HasUserDiscoveryPermission).
            // Grant the role a RESOURCE-scoped View+Execute on exactly one deployed workflow (and
            // an explicit no-permission server entry so SecureConfigLoader does not auto-add a
            // Guests grant), then present a JWT carrying that role (roles == WindowsGroups, as
            // Entra ID would supply). Discovery must list that one workflow and no others.
            const string role               = "Public";
            const string permittedWorkflow  = "tools/http get/TC013_Get_CustomHeader_Echoed"; // relative path
            const string permittedName      = "TC013_Get_CustomHeader_Echoed";                 // workflow Name attribute

            _host.Dispose();
            var settings = SecureConfigBuilder.Build(
                _secretKey,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(role, View: false, Execute: false),
                SecureConfigBuilder.ResourcePerm(role, permittedWorkflow, View: true, Execute: true));
            _host = LightweightInProcessHost.WithSettings(settings);

            var token = JwtTestHelper.ValidToken(_secretKey, role);
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"Expected 200. Body: {resp.Body}");

            var apis = JObject.Parse(resp.Body)["Apis"] as JArray;
            Assert.IsNotNull(apis, $"Response must contain an 'Apis' array. Body: {resp.Body}");
            Assert.AreEqual(1, apis!.Count,
                $"Role '{role}' has resource View+Execute on exactly one workflow, so /Secure/apis.json " +
                $"must list only that workflow. Body: {resp.Body}");

            var api = apis[0];
            Assert.AreEqual(permittedName, api["Name"]?.ToString(),
                $"The single discovered workflow must be the resource-permitted one. Body: {resp.Body}");
            var apiBaseUrl = api["baseUrl"]?.ToString() ?? "";
            Assert.IsTrue(apiBaseUrl.Contains("/Secure/", System.StringComparison.OrdinalIgnoreCase),
                $"Expected /Secure/ in baseUrl, got: {apiBaseUrl}");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // /Public/* — anonymous execution routes (no auth required)
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Public_NoToken_IsAccessible()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/Public/HelloWorld.json");

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status, "/Public/* must not require authentication");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task Public_WithToken_IsAccessible()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "TeamA");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Public/HelloWorld.json", Bearer(token));

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                "/Public/* must not require authentication even when a token is supplied");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // /apis.json discovery routes
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task RootApisJson_Returns200_WithValidJson()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/apis.json");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"Expected 200 for /apis.json. Body: {resp.Body}");

            var json = JObject.Parse(resp.Body);
            Assert.IsNotNull(json["Apis"], "Expected 'Apis' array in apis.json");
            Assert.IsNotNull(json["Name"], "Expected 'Name' field in apis.json");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task PublicApisJson_Returns200_NeverReturns401()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/Public/apis.json");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"/Public/apis.json must return 200. Body: {resp.Body}");
            Assert.IsNotNull(JObject.Parse(resp.Body)["Apis"]);
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_NoToken_Returns401()
        {
            var resp = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json");

            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.Status,
                $"/Secure/apis.json must return 401 without a token. Body: {resp.Body}");
            StringAssert.Contains(resp.Body, "unauthorized",
                $"Expected the 401 body to contain the 'unauthorized' error code. Body: {resp.Body}");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_ValidToken_Returns200()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"/Secure/apis.json with valid JWT must return 200. Body: {resp.Body}");
            Assert.IsNotNull(JObject.Parse(resp.Body)["Apis"]);
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_ExpiredToken_Returns200_WithEmptyApis()
        {
            var token = JwtTestHelper.ExpiredToken(_secretKey, "Warewolf Administrators");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"Expected 200 for /Secure/apis.json with expired token. Body: {resp.Body}");
            Assert.AreEqual(0, (JObject.Parse(resp.Body)["Apis"] as JArray)?.Count,
                "Expired token should yield an empty Apis array");
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task SecureApisJson_Entries_UseSecureRoutePrefix()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", Bearer(token));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status);

            var apis = JObject.Parse(resp.Body)["Apis"] as JArray;
            if (apis is null || apis.Count == 0)
            {
                Assert.Inconclusive("No workflow entries in /Secure/apis.json — deploy workflows first.");
                return;
            }

            foreach (var api in apis)
            {
                var baseUrl = api["baseUrl"]?.ToString() ?? "";
                Assert.IsTrue(baseUrl.Contains("/Secure/", System.StringComparison.OrdinalIgnoreCase),
                    $"Expected /Secure/ prefix in baseUrl, got: {baseUrl}");
            }
        }

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task PublicApisJson_Entries_UsePublicRoutePrefix()
        {
            // /Public/apis.json lists workflows for which the built-in Public group holds BOTH
            // View AND Execute (discovery requires both — PermissionChecker.HasPublicDiscoveryPermission).
            // Seed Public with server-wide View+Execute so the deployed workflows are discoverable.
            _host.Dispose();
            var settings = SecureConfigBuilder.Build(
                _secretKey,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: true, Execute: true));
            _host = LightweightInProcessHost.WithSettings(settings);

            var resp = await _host.SendThroughPipelineAsync("GET", "/Public/apis.json");

            Assert.AreEqual(HttpStatusCode.OK, resp.Status, $"/Public/apis.json must return 200. Body: {resp.Body}");

            var apis = JObject.Parse(resp.Body)["Apis"] as JArray;
            Assert.IsNotNull(apis, $"Response must contain an 'Apis' array. Body: {resp.Body}");
            Assert.IsTrue(apis!.Count > 0,
                "With the Public group granted View+Execute, /Public/apis.json must list the deployed workflows.");

            foreach (var api in apis)
            {
                var baseUrl = api["baseUrl"]?.ToString() ?? "";
                Assert.IsTrue(baseUrl.Contains("/Public/", System.StringComparison.OrdinalIgnoreCase),
                    $"Expected /Public/ prefix in baseUrl, got: {baseUrl}");
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Token round-trip through the secure route (in-process equivalent of the
        // original real-server-install test).
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod, TestCategory("Security_HTTP")]
        public async Task RealConfig_TokenFromFile_AcceptedBySecureRoute()
        {
            var token = JwtTestHelper.ValidToken(_secretKey, "Warewolf Administrators");
            var resp  = await _host.SendThroughPipelineAsync("GET", "/Secure/apis.json", Bearer(token));

            Assert.AreNotEqual(HttpStatusCode.Unauthorized, resp.Status,
                "A token generated from the active secure.config should be accepted by the secure route");
        }
    }
}
