/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Hosting-skeleton tests for McpFunction (warewolf-lee-mcp-v3-spec.md,
 *  "Hosting & transport"): route method gating, the baseline authentication
 *  gate that mirrors /Secure/*'s "authenticated principal required" check,
 *  the MCP JSON-RPC request/response plumbing (initialize, tools/list), and
 *  end-to-end tools/call wiring for the list_workflows tool (schema exclusion
 *  of DI-bound parameters, permission filtering, pagination).
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Functions;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Tests.Auth;

namespace Warewolf.Execution.Lightweight.Tests.Functions
{
    [TestClass]
    public class McpFunctionTests
    {
        // ── Test doubles ──────────────────────────────────────────────────────

        private sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
        {
            public bool IsConfigEffective { get; set; }
            public int PolicyCount => 0;
            public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

            /// <summary>Overridable per-test; defaults to "no permissions" (deny everything).</summary>
            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } =
                (_, _) => WorkflowPermission.None;

            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
                EffectivePermissions(workflowName, callerRoles);

            public void Reload() { }
        }

        private static WorkflowClaimsPrincipal AuthenticatedPrincipal() =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice"), new Claim(AuthConstants.Scope, "user_impersonation") },
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        // ── Helpers ───────────────────────────────────────────────────────────

        private static McpFunction NewFunction(bool configEffective, IServiceProvider? services = null)
        {
            services ??= new ServiceCollection().BuildServiceProvider();
            return new McpFunction(
                McpServerOptionsFactory.Create(services),
                NullLoggerFactory.Instance,
                services,
                new StubAuthPolicyLoader { IsConfigEffective = configEffective },
                NullLogger<McpFunction>.Instance);
        }

        /// <summary>
        /// Builds an <see cref="McpFunction"/> backed by a real service provider
        /// (<see cref="HostEnvironmentConfig"/> pointed at <paramref name="workflowsDirectory"/>
        /// plus <paramref name="authPolicyLoader"/>) so that <c>list_workflows</c>'s DI-bound
        /// tool parameters resolve, and so the SAME <see cref="IWorkflowAuthPolicyLoader"/>
        /// instance backs both the baseline HTTP-layer auth gate and the tool's per-item
        /// permission filter.
        /// </summary>
        private static McpFunction NewFunctionWithTools(string workflowsDirectory, StubAuthPolicyLoader authPolicyLoader)
        {
            Environment.SetEnvironmentVariable("WorkflowsDirectory", workflowsDirectory);
            IServiceProvider services;
            try
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(HostEnvironmentConfig.Load());
                serviceCollection.AddSingleton<IWorkflowAuthPolicyLoader>(authPolicyLoader);
                services = serviceCollection.BuildServiceProvider();
            }
            finally
            {
                Environment.SetEnvironmentVariable("WorkflowsDirectory", null);
            }

            return new McpFunction(
                McpServerOptionsFactory.Create(services),
                NullLoggerFactory.Instance,
                services,
                authPolicyLoader,
                NullLogger<McpFunction>.Instance);
        }

        private static (HttpFunctionContext Context, FakeHttpRequestData Request) NewRequest(
            string method, string? jsonBody = null, WorkflowClaimsPrincipal? principal = null)
        {
            var ctx = new HttpFunctionContext();
            if (principal is not null)
            {
                ctx.Items[AuthConstants.PrincipalContextKey] = principal;
            }

            var req = new FakeHttpRequestData(ctx, new Uri("https://engine.test/mcp"), method);
            if (jsonBody is not null)
            {
                var bytes = Encoding.UTF8.GetBytes(jsonBody);
                req.Body.Write(bytes, 0, bytes.Length);
                req.Body.Position = 0;
            }
            return (ctx, req);
        }

        private static async Task<(HttpStatusCode Status, string Body)> Invoke(
            McpFunction function, HttpFunctionContext ctx, FakeHttpRequestData req)
        {
            var response = await function.HandleMcp(req, ctx);
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            var text = await reader.ReadToEndAsync();
            return (response.StatusCode, text);
        }

        /// <summary>
        /// Extracts the JSON payload from an SSE-framed body written by
        /// <c>StreamableHttpServerTransport.HandlePostRequestAsync</c>
        /// (<c>"event: message\ndata: {...}\n\n"</c>), matching the framing the official
        /// <c>ModelContextProtocol.AspNetCore</c> handler also uses (see
        /// <c>StreamableHttpHandler.InitializeSseResponse</c>).
        /// </summary>
        private static string ExtractSseJsonData(string sseBody)
        {
            foreach (var line in sseBody.Split('\n'))
            {
                if (line.StartsWith("data:", StringComparison.Ordinal))
                {
                    return line.Substring("data:".Length).Trim();
                }
            }
            Assert.Fail($"No SSE 'data:' line found in body: {sseBody}");
            return string.Empty;
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_Get_405_MethodNotAllowed()
        {
            var function = NewFunction(configEffective: false);
            var (ctx, req) = NewRequest("GET");

            var (status, _) = await Invoke(function, ctx, req);

            Assert.AreEqual(HttpStatusCode.MethodNotAllowed, status);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_SecureConfigEffective_NoPrincipal_401_Unauthorized()
        {
            var function = NewFunction(configEffective: true);
            var (ctx, req) = NewRequest("POST", InitializeRequestBody());

            var (status, body) = await Invoke(function, ctx, req);

            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
            Assert.AreEqual("unauthorized", JObject.Parse(body)["error"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_OpenAccessMode_NoPrincipal_InitializeSucceeds()
        {
            var function = NewFunction(configEffective: false);
            var (ctx, req) = NewRequest("POST", InitializeRequestBody());

            var (status, body) = await Invoke(function, ctx, req);

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(ExtractSseJsonData(body));
            Assert.AreEqual(McpServerOptionsFactory.ServerName, json["result"]?["serverInfo"]?["name"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_SecureConfigEffective_AuthenticatedPrincipal_InitializeSucceeds()
        {
            var function = NewFunction(configEffective: true);
            var (ctx, req) = NewRequest("POST", InitializeRequestBody(), AuthenticatedPrincipal());

            var (status, body) = await Invoke(function, ctx, req);

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(ExtractSseJsonData(body));
            Assert.AreEqual(McpServerOptionsFactory.ServerName, json["result"]?["serverInfo"]?["name"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_MalformedJsonBody_400_BadRequest()
        {
            var function = NewFunction(configEffective: false);
            var (ctx, req) = NewRequest("POST", "{ not valid json");

            var (status, body) = await Invoke(function, ctx, req);

            Assert.AreEqual(HttpStatusCode.BadRequest, status);
            Assert.AreEqual("bad_request", JObject.Parse(body)["error"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_ToolsList_ReturnsListWorkflowsTool_SchemaExcludesDiBoundParams()
        {
            var loader = new StubAuthPolicyLoader { IsConfigEffective = false };
            var function = NewFunctionWithTools(Path.Combine(Path.GetTempPath(), "mcp-tools-list-" + Guid.NewGuid().ToString("N")), loader);

            var (initCtx, initReq) = NewRequest("POST", InitializeRequestBody());
            await Invoke(function, initCtx, initReq);

            var (ctx, req) = NewRequest("POST", "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"tools/list\",\"params\":{}}");
            var (status, body) = await Invoke(function, ctx, req);

            Assert.AreEqual(HttpStatusCode.OK, status);
            var tools = JObject.Parse(ExtractSseJsonData(body))["result"]?["tools"] as JArray;
            Assert.IsNotNull(tools);
            // list_workflows, list_tools, get_workflow_definition, get_workflow_schema,
            // get_tool_schema, and validate_workflow are all registered.
            Assert.AreEqual(6, tools!.Count);

            var tool = tools!.Single(t => t["name"]?.ToString() == "list_workflows");

            var schemaProperties = tool["inputSchema"]?["properties"] as JObject;
            Assert.IsNotNull(schemaProperties);
            var propertyNames = schemaProperties!.Properties().Select(p => p.Name).ToList();

            CollectionAssert.AreEquivalent(new[] { "folder", "cursor", "pageSize" }, propertyNames);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_ToolsCall_ListWorkflows_OpenAccessMode_ReturnsAllWorkflows()
        {
            var workflowsDir = Path.Combine(Path.GetTempPath(), "mcp-lw-open-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workflowsDir);
            try
            {
                File.WriteAllText(Path.Combine(workflowsDir, "Hello.bite"),
                    "<Service Name=\"Hello\" ResourceType=\"WorkflowService\"><Comment>Says hello</Comment>" +
                    "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                    "<Message Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></DataList></Service>");

                var loader = new StubAuthPolicyLoader { IsConfigEffective = false };
                var function = NewFunctionWithTools(workflowsDir, loader);

                var (initCtx, initReq) = NewRequest("POST", InitializeRequestBody());
                await Invoke(function, initCtx, initReq);

                var (ctx, req) = NewRequest("POST",
                    "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"tools/call\",\"params\":{\"name\":\"list_workflows\",\"arguments\":{}}}");
                var (status, body) = await Invoke(function, ctx, req);

                Assert.AreEqual(HttpStatusCode.OK, status);
                var json = JObject.Parse(ExtractSseJsonData(body));
                var resultText = json["result"]?["content"]?[0]?["text"]?.ToString();
                Assert.IsNotNull(resultText, $"Expected result.content[0].text in: {json}");
                var payload = JObject.Parse(resultText!);

                var workflows = payload["workflows"] as JArray;
                Assert.IsNotNull(workflows);
                Assert.AreEqual(1, workflows!.Count);
                Assert.AreEqual("Hello", workflows[0]["name"]?.ToString());
                Assert.AreEqual("Says hello", workflows[0]["description"]?.ToString());
                Assert.IsFalse((bool)workflows[0]["bodyEditable"]!);
            }
            finally
            {
                Directory.Delete(workflowsDir, recursive: true);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task HandleMcp_ToolsCall_ListWorkflows_SecureConfigEffective_FiltersByViewPermission()
        {
            var workflowsDir = Path.Combine(Path.GetTempPath(), "mcp-lw-secure-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workflowsDir);
            try
            {
                File.WriteAllText(Path.Combine(workflowsDir, "Visible.bite"),
                    "<Service Name=\"Visible\" ResourceType=\"WorkflowService\"></Service>");
                File.WriteAllText(Path.Combine(workflowsDir, "Hidden.bite"),
                    "<Service Name=\"Hidden\" ResourceType=\"WorkflowService\"></Service>");

                var loader = new StubAuthPolicyLoader
                {
                    IsConfigEffective = true,
                    EffectivePermissions = (path, _) =>
                        path.Equals("Visible", StringComparison.OrdinalIgnoreCase)
                            ? WorkflowPermission.View
                            : WorkflowPermission.None,
                };
                var function = NewFunctionWithTools(workflowsDir, loader);

                var (initCtx, initReq) = NewRequest("POST", InitializeRequestBody(), AuthenticatedPrincipal());
                await Invoke(function, initCtx, initReq);

                var (ctx, req) = NewRequest(
                    "POST",
                    "{\"jsonrpc\":\"2.0\",\"id\":2,\"method\":\"tools/call\",\"params\":{\"name\":\"list_workflows\",\"arguments\":{}}}",
                    AuthenticatedPrincipal());
                var (status, body) = await Invoke(function, ctx, req);

                Assert.AreEqual(HttpStatusCode.OK, status);
                var json = JObject.Parse(ExtractSseJsonData(body));
                var resultText = json["result"]?["content"]?[0]?["text"]?.ToString();
                Assert.IsNotNull(resultText, $"Expected result.content[0].text in: {json}");
                var workflows = JObject.Parse(resultText!)["workflows"] as JArray;
                Assert.IsNotNull(workflows);
                Assert.AreEqual(1, workflows!.Count);
                Assert.AreEqual("Visible", workflows[0]["name"]?.ToString());
            }
            finally
            {
                Directory.Delete(workflowsDir, recursive: true);
            }
        }

        private static string InitializeRequestBody() =>
            "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"initialize\",\"params\":{" +
            "\"protocolVersion\":\"2024-11-05\",\"capabilities\":{}," +
            "\"clientInfo\":{\"name\":\"test-client\",\"version\":\"1.0\"}}}";
    }
}
