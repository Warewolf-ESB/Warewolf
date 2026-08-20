/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Mcp
{
    /// <summary>
    /// Live, out-of-process integration tests for the MCP REST endpoints
    /// (<c>Functions/McpApiFunctions.cs</c>, <c>/mcp-api/{tool_name}</c>) against an
    /// already-deployed, externally reachable Warewolf Execution Engine (an Azure
    /// Function App), as opposed to <c>InProcess.LightweightInProcessHost</c> which hosts
    /// the engine in-process for the rest of this project's tests.
    ///
    /// <para>
    /// <b>Why out-of-process at all.</b> These prove the tools work end-to-end through the
    /// real Azure Functions host/worker gRPC relay and network path — the exact layer the
    /// retired <c>/mcp</c> JSON-RPC endpoint fell foul of (see <c>McpApiFunctions</c>'s XML
    /// docs for the chunked-transfer host bug, Azure/azure-functions-host#7930). The plain-
    /// REST design adopted here (ordinary string-bodied JSON, always carrying
    /// <c>Content-Length</c>) is exactly what sidesteps that bug, and only a real
    /// out-of-process call can demonstrate that.
    /// </para>
    ///
    /// <para>
    /// <b>Configuration.</b> Reads the target engine's base URL from the
    /// <c>WAREWOLF_MCP_EXTERNAL_BASE_URL</c> environment variable (e.g.
    /// <c>https://warewolfserver.azurewebsites.net</c>, no trailing slash). When unset, or
    /// when the engine is unreachable, every test degrades to <c>Inconclusive</c> rather
    /// than failing — this suite is never meant to run as part of the in-process/no-external-
    /// dependency default (see this project's other tests), only opted into by a
    /// pipeline/user that has an engine to point it at.
    /// </para>
    ///
    /// <para>
    /// <b>Deliberately read-only / idempotent.</b> These tests only call tools that do not
    /// mutate the deployed instance's <c>Resources</c> (no <c>create_workflow</c>,
    /// <c>edit_workflow</c>, <c>deploy_workflow</c>, <c>add_step</c>, <c>add_source</c>,
    /// <c>edit_source</c>) so the same shared engine can be hit repeatedly (e.g. on every
    /// pipeline run) without accumulating state or racing other jobs targeting it.
    /// </para>
    /// </summary>
    [TestClass]
    [TestCategory("LiveIntegration_McpApi")]
    public class McpApiExternalTests
    {
        internal const string BaseUrlEnvVar = "WAREWOLF_MCP_EXTERNAL_BASE_URL";

        static HttpClient _http = null!;
        static string? _baseUrl;
        static bool _engineAvailable;

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            _baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnvVar)?.TrimEnd('/');
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                _engineAvailable = false;
                return;
            }

            // Probe with the cheapest possible call (list_tools needs no Resources/workflow
            // state) so a genuinely unreachable/cold instance degrades every test to
            // Inconclusive instead of a wall of misleading network-error failures.
            try
            {
                var response = _http.PostAsync($"{_baseUrl}/mcp-api/list_tools",
                    new StringContent("{}", Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
                _engineAvailable = response.IsSuccessStatusCode;
            }
            catch
            {
                _engineAvailable = false;
            }
        }

        [ClassCleanup]
        public static void ClassCleanup() => _http?.Dispose();

        static void SkipIfUnavailable()
        {
            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                Assert.Inconclusive(
                    $"{BaseUrlEnvVar} is not set — skipping external MCP API integration tests. " +
                    "Set it to a deployed engine's base URL (e.g. https://warewolfserver.azurewebsites.net) to run them.");
            }
            if (!_engineAvailable)
            {
                Assert.Inconclusive(
                    $"External engine at '{_baseUrl}' did not respond successfully to a list_tools probe — skipping.");
            }
        }

        static async Task<(int StatusCode, JsonDocument Json)> PostAsync(string tool, string jsonBody)
        {
            var response = await _http.PostAsync($"{_baseUrl}/mcp-api/{tool}",
                new StringContent(jsonBody, Encoding.UTF8, "application/json"));
            var text = await response.Content.ReadAsStringAsync();
            return ((int)response.StatusCode, JsonDocument.Parse(text));
        }

        [TestMethod]
        public async Task ListTools_ReturnsNonEmptyToolCatalog()
        {
            SkipIfUnavailable();

            var (status, json) = await PostAsync("list_tools", "{}");

            Assert.AreEqual(200, status);
            var tools = json.RootElement.GetProperty("tools");
            Assert.IsTrue(tools.GetArrayLength() > 0, "Expected at least one tool in the catalog.");
            Assert.IsTrue(
                tools.EnumerateArray().Any(t => t.GetProperty("name").GetString() == "Assign"),
                "Expected the well-known 'Assign' tool to be present in the catalog.");
        }

        [TestMethod]
        public async Task ListWorkflows_ReturnsWorkflowsEnvelope()
        {
            SkipIfUnavailable();

            var (status, json) = await PostAsync("list_workflows", "{}");

            Assert.AreEqual(200, status);
            Assert.AreEqual(JsonValueKind.Array, json.RootElement.GetProperty("workflows").ValueKind);
        }

        [TestMethod]
        public async Task GetToolSchema_KnownTool_ReturnsSchema()
        {
            SkipIfUnavailable();

            var (status, json) = await PostAsync("get_tool_schema", "{\"tool_name\":\"Assign\"}");

            Assert.AreEqual(200, status);
            Assert.AreEqual("Assign", json.RootElement.GetProperty("tool_name").GetString());
            Assert.AreEqual(JsonValueKind.Object, json.RootElement.GetProperty("schema").ValueKind);
        }

        [TestMethod]
        public async Task GetToolSchema_UnknownTool_Returns400WithErrorContract()
        {
            SkipIfUnavailable();

            var (status, json) = await PostAsync("get_tool_schema", "{\"tool_name\":\"DoesNotExist\"}");

            Assert.AreEqual(400, status);
            Assert.AreEqual("bad_request", json.RootElement.GetProperty("error").GetString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("message").GetString()));
            Assert.IsFalse(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("correlationId").GetString()));
        }

        [TestMethod]
        public async Task GetWorkflowSchema_ReturnsEnvelopeAndBodySchema()
        {
            SkipIfUnavailable();

            var (status, json) = await PostAsync("get_workflow_schema", "{}");

            Assert.AreEqual(200, status);
            Assert.AreEqual(JsonValueKind.Object, json.RootElement.GetProperty("envelope_schema").ValueKind);
            Assert.AreEqual(JsonValueKind.Object, json.RootElement.GetProperty("body_schema").ValueKind);
        }

        [TestMethod]
        public async Task ValidateWorkflow_GraphWithNoStartNode_ReturnsStructuredInvalidResult()
        {
            SkipIfUnavailable();

            const string body =
                "{\"envelope\":{\"name\":\"t\",\"inputs\":[],\"outputs\":[]},\"body\":{\"resourcename\":\"t\",\"cells\":[]}}";

            var (status, json) = await PostAsync("validate_workflow", body);

            Assert.AreEqual(200, status);
            Assert.IsFalse(json.RootElement.GetProperty("valid").GetBoolean());
            var errors = json.RootElement.GetProperty("errors");
            Assert.IsTrue(errors.GetArrayLength() > 0, "Expected at least one validation error for a startless graph.");
        }

        [TestMethod]
        public async Task ValidateWorkflow_MissingEnvelope_Returns400WithErrorContract()
        {
            SkipIfUnavailable();

            var (status, json) = await PostAsync("validate_workflow", "{}");

            Assert.AreEqual(400, status);
            Assert.AreEqual("bad_request", json.RootElement.GetProperty("error").GetString());
        }
    }
}
