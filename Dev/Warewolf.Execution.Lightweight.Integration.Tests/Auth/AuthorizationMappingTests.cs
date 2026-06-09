/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  End-to-end AUTHORIZATION-MAPPING tests: how a request principal's roles (WindowsGroups)
 *  combine with secure.config server-level and resource-level permission entries to ALLOW or
 *  DENY workflow EXECUTION on /public and /secure routes. These drive the real worker middleware
 *  pipeline + WorkflowPolicyMatcher + WorkflowAuthPolicyLoader in-process via
 *  LightweightInProcessHost, with a secure.config seeded per test and (for /secure) a Warewolf
 *  HMAC JWT carrying the caller's roles (as Entra ID would supply).
 *
 *  Scope rules under test (WorkflowAuthPolicyLoader):
 *    • Server-level (IsServer) grants apply to every workflow UNLESS the workflow has
 *      resource-level entries — in which case the RESOURCE scope is used EXCLUSIVELY for that
 *      workflow (the global grant is discarded). A resource entry only forms a policy when it
 *      carries the Execute flag, so a resource override is expressed as an Execute-bearing entry
 *      for a *different* group.
 *    • The "Public" group is always OR'd into the active scope's effective permissions.
 *    • Execution requires View AND Execute.
 *
 *  Deny status: WorkflowAuthorizationMiddleware (and the /public execution gate in
 *  WorkflowHttpFunction) currently wrap a policy denial as HTTP 500 — the 403 path is commented
 *  out to match the existing Warewolf server (middleware lines 239-247, WOLF-8418). These tests
 *  assert the current 500 contract; flip the deny expectations to 403 when that change lands.
 *
 *  [DoNotParallelize] — each test mutates the process-wide SecureConfigLoader singleton + env var.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Auth
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("Authorization_Mapping")]
    public class AuthorizationMappingTests
    {
        // Two deployed WorkflowService .bite files. The relative path is the resource key used by
        // both the discovery filter and resource-scoped permission matching.
        private const string WfA = "tools/http get/TC013_Get_CustomHeader_Echoed";
        private const string WfB = "tools/system info/TestGettingComputerName";

        private static Dictionary<string, string> Bearer(string token) =>
            new() { ["Authorization"] = "Bearer " + token };

        // ════════════════════════════════════════════════════════════════════════
        // /public execution — gated by the Public group's View+Execute (anonymous principal)
        // ════════════════════════════════════════════════════════════════════════

        // G1
        [TestMethod]
        public async Task Public_ServerViewExecute_Allows()
        {
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: true, Execute: true)));

            var (status, body) = await host.ExecutePublicAsync($"{WfA}.json");

            Assert.AreEqual(HttpStatusCode.OK, status,
                $"Public with server-level View+Execute must allow /public execution. Body: {body}");
        }

        // G2
        [TestMethod]
        public async Task Public_ServerViewOnly_Denied()
        {
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: true, Execute: false)));

            var (status, body) = await host.ExecutePublicAsync($"{WfA}.json");

            Assert.AreEqual(HttpStatusCode.InternalServerError, status,
                $"Public with View-only (no Execute) must be denied on /public execution (currently 500). Body: {body}");
        }

        // G3
        [TestMethod]
        public async Task Public_ServerExecuteOnly_Denied()
        {
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: false, Execute: true)));

            var (status, body) = await host.ExecutePublicAsync($"{WfA}.json");

            Assert.AreEqual(HttpStatusCode.InternalServerError, status,
                $"Public with Execute-only (no View) must be denied on /public execution (currently 500). Body: {body}");
        }

        // G4 — resource-only grant: allowed on the permitted workflow, denied elsewhere.
        [TestMethod]
        public async Task Public_ResourceOnly_AllowsPermittedWorkflowOnly()
        {
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: false, Execute: false),
                SecureConfigBuilder.ResourcePerm(SecureConfigBuilder.PublicGroup, WfA, View: true, Execute: true)));

            var a = await host.ExecutePublicAsync($"{WfA}.json");
            Assert.AreEqual(HttpStatusCode.OK, a.Status,
                $"Public resource View+Execute on WfA must allow it. Body: {a.Body}");

            var b = await host.ExecutePublicAsync($"{WfB}.json");
            Assert.AreEqual(HttpStatusCode.InternalServerError, b.Status,
                $"Public has no permission on WfB → deny (currently 500). Body: {b.Body}");
        }

        // G5 — resource scope overrides a server grant (deny on the resource-scoped workflow).
        [TestMethod]
        public async Task Public_ResourceScopeOverridesServerGrant_Denied()
        {
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: true, Execute: true),
                SecureConfigBuilder.ResourcePerm("OtherTeam", WfA, View: true, Execute: true)));

            var a = await host.ExecutePublicAsync($"{WfA}.json");
            Assert.AreEqual(HttpStatusCode.InternalServerError, a.Status,
                $"WfA has resource entries (OtherTeam only) → resource scope is exclusive → Public's server " +
                $"grant is overridden → deny (currently 500). Body: {a.Body}");

            var b = await host.ExecutePublicAsync($"{WfB}.json");
            Assert.AreEqual(HttpStatusCode.OK, b.Status,
                $"WfB has no resource entries → Public's server View+Execute applies → allow. Body: {b.Body}");
        }

        // ════════════════════════════════════════════════════════════════════════
        // /secure execution — gated by the caller role's View+Execute (authenticated principal)
        // ════════════════════════════════════════════════════════════════════════

        // G6
        [TestMethod]
        public async Task Secure_ServerViewExecute_Allows()
        {
            var key = SecureConfigBuilder.NewSecretKey();
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                key,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm("TeamA", View: true, Execute: true)));

            var resp = await host.SendThroughPipelineAsync("GET", $"/secure/{WfA}.json",
                Bearer(JwtTestHelper.ValidToken(key, "TeamA")));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"Role with server-level View+Execute must execute on /secure. Body: {resp.Body}");
        }

        // G7
        [TestMethod]
        public async Task Secure_ServerViewOnly_Denied()
        {
            var key = SecureConfigBuilder.NewSecretKey();
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                key,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm("TeamA", View: true, Execute: false)));

            var resp = await host.SendThroughPipelineAsync("GET", $"/secure/{WfA}.json",
                Bearer(JwtTestHelper.ValidToken(key, "TeamA")));

            Assert.AreEqual(HttpStatusCode.InternalServerError, resp.Status,
                $"Role with View-only (no Execute) must be denied on /secure execution (currently 500). Body: {resp.Body}");
        }

        // G8 — authenticated caller whose role has no secure.config entry.
        [TestMethod]
        public async Task Secure_NoMatchingGroup_Denied()
        {
            var key = SecureConfigBuilder.NewSecretKey();
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                key,
                SecureConfigBuilder.Admin(View: true))); // only Admin; "TeamA" is absent

            var resp = await host.SendThroughPipelineAsync("GET", $"/secure/{WfA}.json",
                Bearer(JwtTestHelper.ValidToken(key, "TeamA")));

            Assert.AreEqual(HttpStatusCode.InternalServerError, resp.Status,
                $"A role with no secure.config entry must be denied (currently 500). Body: {resp.Body}");
        }

        // G8b — resource-only role: allowed on its workflow, denied elsewhere.
        [TestMethod]
        public async Task Secure_ResourceOnlyRole_AllowsPermittedWorkflowOnly()
        {
            var key = SecureConfigBuilder.NewSecretKey();
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                key,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: false, Execute: false),
                SecureConfigBuilder.ResourcePerm("TeamA", WfA, View: true, Execute: true))); // no server grant for TeamA

            var token = JwtTestHelper.ValidToken(key, "TeamA");

            var a = await host.SendThroughPipelineAsync("GET", $"/secure/{WfA}.json", Bearer(token));
            Assert.AreEqual(HttpStatusCode.OK, a.Status,
                $"TeamA's resource View+Execute on WfA must allow it. Body: {a.Body}");

            var b = await host.SendThroughPipelineAsync("GET", $"/secure/{WfB}.json", Bearer(token));
            Assert.AreEqual(HttpStatusCode.InternalServerError, b.Status,
                $"TeamA has no permission on WfB → deny (currently 500). Body: {b.Body}");
        }

        // G9 — resource scope overrides a server grant on /secure (deny on resource-scoped workflow).
        [TestMethod]
        public async Task Secure_ResourceScopeOverridesServerGrant_Denied()
        {
            var key = SecureConfigBuilder.NewSecretKey();
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                key,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm("TeamA", View: true, Execute: true),
                SecureConfigBuilder.ResourcePerm("OtherTeam", WfA, View: true, Execute: true)));

            var token = JwtTestHelper.ValidToken(key, "TeamA");

            var a = await host.SendThroughPipelineAsync("GET", $"/secure/{WfA}.json", Bearer(token));
            Assert.AreEqual(HttpStatusCode.InternalServerError, a.Status,
                $"WfA's resource scope (OtherTeam only) overrides TeamA's server grant → deny (currently 500). Body: {a.Body}");

            var b = await host.SendThroughPipelineAsync("GET", $"/secure/{WfB}.json", Bearer(token));
            Assert.AreEqual(HttpStatusCode.OK, b.Status,
                $"WfB has no resource entries → TeamA's server View+Execute applies → allow. Body: {b.Body}");
        }

        // G10 — the Public group is always OR'd in, even on a resource-scoped /secure workflow.
        [TestMethod]
        public async Task Secure_PublicGroupAlwaysApplies_OnResourceScopedWorkflow()
        {
            var key = SecureConfigBuilder.NewSecretKey();
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                key,
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ResourcePerm(SecureConfigBuilder.PublicGroup, WfA, View: true, Execute: true)));

            // The caller's own role has no entry anywhere; only the Public group is granted on WfA.
            var resp = await host.SendThroughPipelineAsync("GET", $"/secure/{WfA}.json",
                Bearer(JwtTestHelper.ValidToken(key, "SomeUnmappedRole")));

            Assert.AreEqual(HttpStatusCode.OK, resp.Status,
                $"Public's resource View+Execute is always OR'd into the active scope → an authenticated caller " +
                $"with no own grant is allowed. Body: {resp.Body}");
        }

        // ════════════════════════════════════════════════════════════════════════
        // Discovery asymmetry — listing requires View+Execute (View-only is excluded)
        // ════════════════════════════════════════════════════════════════════════

        // G11
        [TestMethod]
        public async Task PublicApisJson_PublicViewOnly_ExcludesWorkflows()
        {
            using var host = LightweightInProcessHost.WithSettings(SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: true, Execute: false)));

            var (status, body) = await host.ExecutePublicAsync("apis.json");

            Assert.AreEqual(HttpStatusCode.OK, status, $"/Public/apis.json must be reachable. Body: {body}");
            var apis = JObject.Parse(body)["Apis"] as JArray;
            Assert.IsNotNull(apis, $"Response must contain an 'Apis' array. Body: {body}");
            Assert.AreEqual(0, apis!.Count,
                "Public with View-only (no Execute) must discover no workflows — discovery requires View AND Execute.");
        }
    }
}
