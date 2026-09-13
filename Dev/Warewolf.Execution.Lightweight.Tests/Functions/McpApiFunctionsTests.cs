/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Wrapper-plumbing tests for McpApiFunctions (the REST replacement for the
 *  retired /mcp JSON-RPC endpoint): the baseline authentication gate,
 *  malformed-JSON-body handling, McpException -> 400 mapping, and one
 *  happy-path round-trip per all 18 /mcp-api/{tool_name} routes verifying
 *  correct request-body binding into each tool handler's Handle(...) call
 *  and correct response JSON shape. Business-rule coverage for each tool
 *  (permission gating, validation, edge cases) lives in that tool's own
 *  dedicated *ToolTests.cs under Mcp/ToolHandlers/ - this file only proves
 *  the HTTP wrapper binds/dispatches/serializes correctly.
 */

using Dev2.Common.X6;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Functions;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Tests.Auth;
using Warewolf.Execution.Lightweight.Tests.TestSupport;
using ModelContextProtocol;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Warewolf.Execution.Lightweight.Tests.Functions
{
    [TestClass]
    public class McpApiFunctionsTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "mcp-api-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles ─────────────────────────────────────────────────────

        private sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
        {
            public bool IsConfigEffective { get; set; }
            public int PolicyCount => 0;
            public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } =
                (_, _) => WorkflowPermission.All;

            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
                EffectivePermissions(workflowName, callerRoles);

            public void Reload() { }
        }

        private sealed class FakeSecretResolver : IMcpSecretResolver
        {
            public Task<string> ResolveAsync(string name, CancellationToken cancellationToken) =>
                throw new McpException($"Secret reference '${{{name}}}' could not be resolved: no secret named '{name}' is staged.");
        }

        /// <summary>
        /// Captures log entries so the unhandled-exception path can assert it actually logs — the
        /// point of that catch block is diagnosability, so a silent 500 would defeat it.
        /// </summary>
        private sealed class CapturingLogger<T> : ILogger<T>
        {
            public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                Entries.Add((logLevel, formatter(state, exception), exception));
        }

        private sealed class FakeWorkflowExecutor : IWorkflowExecutor
        {
            private readonly Func<WorkflowExecutionRequest, WorkflowExecutionResult> _impl;
            public FakeWorkflowExecutor(Func<WorkflowExecutionRequest, WorkflowExecutionResult> impl) => _impl = impl;

            public WorkflowExecutionResult Execute(string workflowFilePath, Dictionary<string, string>? inputs = null) =>
                throw new NotSupportedException("ExecuteWorkflowTool only calls the WorkflowExecutionRequest overload.");

            public WorkflowExecutionResult Execute(WorkflowExecutionRequest request) => _impl(request);

            public TestExecutionResult ExecuteTest(TestExecutionRequest request) =>
                throw new NotSupportedException("McpApiFunctionsTests does not exercise execute_test.");
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        // ── Function/env builders ────────────────────────────────────────────

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        private McpApiFunctions NewFunctions(
            IWorkflowAuthPolicyLoader? authPolicyLoader = null,
            IWorkflowExecutor? workflowExecutor = null,
            IMcpSecretResolver? secretResolver = null,
            ILogger<McpApiFunctions>? logger = null)
        {
            var services = new ServiceCollection();
            services.AddSingleton(secretResolver ?? new FakeSecretResolver());
            var provider = services.BuildServiceProvider();

            return new McpApiFunctions(
                HostConfig(),
                authPolicyLoader ?? new StubAuthPolicyLoader { IsConfigEffective = false },
                workflowExecutor ?? new FakeWorkflowExecutor(
                    _ => throw new InvalidOperationException("Executor should not have been invoked.")),
                provider,
                logger ?? NullLogger<McpApiFunctions>.Instance);
        }

        private static (HttpFunctionContext Context, FakeHttpRequestData Request) NewRequest(
            string route, string? jsonBody = null, WorkflowClaimsPrincipal? principal = null)
        {
            var ctx = new HttpFunctionContext();
            if (principal is not null)
            {
                ctx.Items[AuthConstants.PrincipalContextKey] = principal;
            }

            var req = new FakeHttpRequestData(ctx, new Uri($"https://engine.test/mcp-api/{route}"), "POST");
            if (jsonBody is not null)
            {
                var bytes = Encoding.UTF8.GetBytes(jsonBody);
                req.Body.Write(bytes, 0, bytes.Length);
                req.Body.Position = 0;
            }
            return (ctx, req);
        }

        private static async Task<(HttpStatusCode Status, string Body)> Invoke(
            Task<HttpResponseData> call)
        {
            var response = await call;
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            var text = await reader.ReadToEndAsync();
            return (response.StatusCode, text);
        }

        // ── Fixture builders (mirrors CreateWorkflowToolTests'/DeployWorkflowToolTests' shapes) ──

        static Cell MakeStartNode(string id = "start") =>
            new() { id = id, shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };

        static Cell MakeAssign(string id, string displayName, JArray fields) =>
            new()
            {
                id = id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = displayName,
                    ["fields"] = fields,
                }
            };

        static Cell MakeAssignObject(string id, string displayName, JArray fields) =>
            new()
            {
                id = id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignobjectactivity",
                    ["displayName"] = displayName,
                    ["fields"] = fields,
                }
            };

        static Cell MakeEdge(string id, string sourceId, string targetId) =>
            new() { id = id, shape = "edge", data = new Dictionary<string, object>(), Source = new Connector(sourceId), Target = new Connector(targetId) };

        static JsonElement BodyOf(string resourceName, params Cell[] cells)
        {
            var graph = new X6WorkflowSaveModel { ResourceName = resourceName, Cells = new List<Cell>(cells) };
            return JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
        }

        static JsonElement EnvelopeOf(object envelopeObj) => JsonSerializer.SerializeToElement(envelopeObj);

        static JsonElement ValidEnvelope(string name = "NewWorkflow") => EnvelopeOf(new
        {
            name,
            description = "A freshly created workflow",
            inputs = Array.Empty<object>(),
            outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
        });

        static JsonElement ValidBody(string resourceName = "NewWorkflow")
        {
            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
            var assign = MakeAssign("assign1", "Assign", fields);
            return BodyOf(resourceName, MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));
        }

        private void SeedWorkflowBite(string name, string description = "")
        {
            File.WriteAllText(Path.Combine(_root, name + ".bite"),
                $"<Service Name=\"{name}\" ResourceType=\"WorkflowService\">" +
                $"<Comment>{description}</Comment><DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                "<Message Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></DataList>" +
                "<Action Name=\"InvokeWorkflow\" Type=\"Workflow\"><XamlDefinition></XamlDefinition></Action></Service>");
        }

        private void SeedEditableWorkflow(string name)
        {
            CreateWorkflowTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false },
                Principal("Developers"), name, ValidEnvelope(name), ValidBody(name));
        }

        /// <summary>
        /// Seeds a workflow using "Assign Object" nodes only (Pass-fidelity — see
        /// AddStepToolTests' remarks), so the resulting .bite round-trips with
        /// bodyEditable:true and add_step can append to it.
        /// </summary>
        private void SeedAssignObjectWorkflow(string name)
        {
            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
            var body = BodyOf(name, MakeStartNode(), MakeAssignObject("assign1", "Assign Object", fields), MakeEdge("e1", "start", "assign1"));
            CreateWorkflowTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false },
                Principal("Developers"), name, ValidEnvelope(name), body);
        }

        private string BuildValidBiteContent(string resourceName)
        {
            var scratchRoot = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), "mcp-api-deploy-fixture-" + Guid.NewGuid().ToString("N"))).FullName;
            try
            {
                var scratchConfig = McpToolTestHostConfig.ForWorkflowsDirectory(scratchRoot);

                CreateWorkflowTool.Handle(scratchConfig, new StubAuthPolicyLoader { IsConfigEffective = false },
                    null, resourceName, ValidEnvelope(resourceName), ValidBody(resourceName));

                return File.ReadAllText(Path.Combine(scratchRoot, resourceName + ".bite"));
            }
            finally
            {
                Directory.Delete(scratchRoot, recursive: true);
            }
        }

        private static WorkflowExecutionResult SuccessExecutionResult()
        {
            var result = new WorkflowExecutionResult { IsSuccess = true, ExecutionId = Guid.NewGuid() };
            result.PayloadWriter = async (stream, ct) =>
            {
                result.Outputs = new Dictionary<string, object> { ["Result"] = "hello" };
                var bytes = Encoding.UTF8.GetBytes("{\"Result\":\"hello\"}");
                await stream.WriteAsync(bytes, 0, bytes.Length, ct);
            };
            return result;
        }

        // ── Tests: auth gate / malformed body / tool-level McpException mapping ──

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ListWorkflows_SecureConfigEffective_NoPrincipal_401_Unauthorized()
        {
            var function = NewFunctions(authPolicyLoader: new StubAuthPolicyLoader { IsConfigEffective = true });
            var (ctx, req) = NewRequest("list_workflows", "{}");

            var (status, body) = await Invoke(function.ListWorkflows(req, ctx));

            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
            Assert.AreEqual("unauthorized", JObject.Parse(body)["error"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ListWorkflows_OpenAccessMode_NoPrincipal_Succeeds()
        {
            SeedWorkflowBite("Hello", "Says hello");
            var function = NewFunctions();
            var (ctx, req) = NewRequest("list_workflows", "{}");

            var (status, body) = await Invoke(function.ListWorkflows(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var workflows = JObject.Parse(body)["workflows"] as JArray;
            Assert.IsNotNull(workflows);
            Assert.AreEqual(1, workflows!.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task CreateWorkflow_MalformedJsonBody_400_BadRequest()
        {
            var function = NewFunctions();
            var (ctx, req) = NewRequest("create_workflow", "{ not valid json");

            var (status, body) = await Invoke(function.CreateWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.BadRequest, status);
            Assert.AreEqual("bad_request", JObject.Parse(body)["error"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetWorkflowDefinition_ToolLevelMcpException_MapsTo400_WithMessagePassthrough()
        {
            var function = NewFunctions();
            var (ctx, req) = NewRequest("get_workflow_definition", "{\"name\":\"\"}");

            var (status, body) = await Invoke(function.GetWorkflowDefinition(req, ctx));

            Assert.AreEqual(HttpStatusCode.BadRequest, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("bad_request", json["error"]?.ToString());
            Assert.AreEqual("`name` is required.", json["message"]?.ToString());
        }

        // ── Tests: happy-path round-trip, one per route ──────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ListTools_HappyPath()
        {
            var function = NewFunctions();
            var (ctx, req) = NewRequest("list_tools");

            var (status, body) = await Invoke(function.ListTools(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var tools = JObject.Parse(body)["tools"] as JArray;
            Assert.IsNotNull(tools);
            Assert.IsTrue(tools!.Any(t => t["name"]?.ToString() == "Assign"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetWorkflowDefinition_HappyPath()
        {
            SeedWorkflowBite("Hello", "Says hello");
            var function = NewFunctions();
            var (ctx, req) = NewRequest("get_workflow_definition", "{\"name\":\"Hello\"}");

            var (status, body) = await Invoke(function.GetWorkflowDefinition(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            Assert.AreEqual("Hello", JObject.Parse(body)["name"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetWorkflowUrl_ToolLevelMcpException_MapsTo400_WithMessagePassthrough()
        {
            var function = NewFunctions();
            var (ctx, req) = NewRequest("get_workflow_url", "{\"name\":\"\"}");

            var (status, body) = await Invoke(function.GetWorkflowUrl(req, ctx));

            Assert.AreEqual(HttpStatusCode.BadRequest, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("bad_request", json["error"]?.ToString());
            Assert.AreEqual("`name` is required.", json["message"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetWorkflowUrl_HappyPath_ReturnsSecureUrlOnly()
        {
            SeedWorkflowBite("Hello", "Says hello");
            var function = NewFunctions(authPolicyLoader: new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.View,
            });
            var (ctx, req) = NewRequest("get_workflow_url", "{\"name\":\"Hello\"}", Principal("Developers"));

            var (status, body) = await Invoke(function.GetWorkflowUrl(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("Hello", json["name"]?.ToString());
            Assert.AreEqual("/Secure/Hello", json["url"]?.ToString());
            Assert.IsNull(json["publicUrl"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetWorkflowSchema_HappyPath()
        {
            var function = NewFunctions();
            var (ctx, req) = NewRequest("get_workflow_schema");

            var (status, body) = await Invoke(function.GetWorkflowSchema(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            Assert.IsNotNull(JObject.Parse(body)["envelope_schema"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task GetToolSchema_HappyPath()
        {
            var function = NewFunctions();
            var (ctx, req) = NewRequest("get_tool_schema", "{\"tool_name\":\"Assign\"}");

            var (status, body) = await Invoke(function.GetToolSchema(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            Assert.AreEqual("Assign", JObject.Parse(body)["tool_name"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ValidateWorkflow_HappyPath()
        {
            var function = NewFunctions();
            var payload = JsonSerializer.Serialize(new { envelope = ValidEnvelope(), body = ValidBody() });
            var (ctx, req) = NewRequest("validate_workflow", payload);

            var (status, body) = await Invoke(function.ValidateWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            Assert.AreEqual(true, JObject.Parse(body)["valid"]?.ToObject<bool>());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task CreateWorkflow_HappyPath()
        {
            var function = NewFunctions();
            var payload = JsonSerializer.Serialize(new { name = "NewWf", envelope = ValidEnvelope("NewWf"), body = ValidBody("NewWf") });
            var (ctx, req) = NewRequest("create_workflow", payload);

            var (status, body) = await Invoke(function.CreateWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("NewWf", json["name"]?.ToString());
            Assert.AreEqual(true, json["created"]?.ToObject<bool>());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task EditWorkflow_HappyPath()
        {
            SeedEditableWorkflow("ToEdit");
            var function = NewFunctions();
            var payload = JsonSerializer.Serialize(new { name = "ToEdit", envelope = ValidEnvelope("ToEdit"), body = ValidBody("ToEdit") });
            var (ctx, req) = NewRequest("edit_workflow", payload);

            var (status, body) = await Invoke(function.EditWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("ToEdit", json["name"]?.ToString());
            Assert.AreEqual(true, json["updated"]?.ToObject<bool>());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DeployWorkflow_HappyPath()
        {
            var biteContent = BuildValidBiteContent("Deployed");
            var function = NewFunctions();
            var payload = JsonSerializer.Serialize(new { name = "Deployed", biteContent });
            var (ctx, req) = NewRequest("deploy_workflow", payload);

            var (status, body) = await Invoke(function.DeployWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("Deployed", json["name"]?.ToString());
            Assert.AreEqual(true, json["deployed"]?.ToObject<bool>());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DeleteWorkflow_HappyPath()
        {
            SeedEditableWorkflow("ToDelete");
            var function = NewFunctions();
            var payload = JsonSerializer.Serialize(new { name = "ToDelete" });
            var (ctx, req) = NewRequest("delete_workflow", payload);

            var (status, body) = await Invoke(function.DeleteWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("ToDelete", json["name"]?.ToString());
            Assert.AreEqual(true, json["deleted"]?.ToObject<bool>());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task AddStep_HappyPath()
        {
            SeedAssignObjectWorkflow("SimpleAppend");
            var function = NewFunctions();
            var stepPayload = new
            {
                shape = "Assign Object",
                label = "Second Assign",
                data = new
                {
                    type = "dsfdotnetmultiassignobjectactivity",
                    fields = new[] { new { FieldName = "[[Result]]", FieldValue = "world", IndexNumber = 1 } },
                },
            };
            var payload = JsonSerializer.Serialize(new { name = "SimpleAppend", step = stepPayload });
            var (ctx, req) = NewRequest("add_step", payload);

            var (status, body) = await Invoke(function.AddStep(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status, "Response body: " + body);
            var json = JObject.Parse(body);
            Assert.AreEqual("SimpleAppend", json["name"]?.ToString());
            Assert.AreEqual(true, json["updated"]?.ToObject<bool>());
            Assert.IsFalse(string.IsNullOrWhiteSpace(json["stepId"]?.ToString()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task AddSource_HappyPath()
        {
            var function = NewFunctions();
            var payload = JsonSerializer.Serialize(new { name = "NewSrc", sourceType = "Redis", config = new { HostName = "localhost" } });
            var (ctx, req) = NewRequest("add_source", payload);

            var (status, body) = await Invoke(function.AddSource(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("NewSrc", json["name"]?.ToString());
            Assert.AreEqual(true, json["created"]?.ToObject<bool>());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task EditSource_HappyPath()
        {
            await AddSourceTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false },
                new FakeSecretResolver(), Principal("Developers"), "ExistingSrc", "Redis",
                JsonSerializer.SerializeToElement(new { HostName = "localhost" }));

            var function = NewFunctions();
            var payload = JsonSerializer.Serialize(new { name = "ExistingSrc", sourceType = "Redis", config = new { HostName = "127.0.0.1" } });
            var (ctx, req) = NewRequest("edit_source", payload);

            var (status, body) = await Invoke(function.EditSource(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("ExistingSrc", json["name"]?.ToString());
            Assert.AreEqual(true, json["updated"]?.ToObject<bool>());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ExecuteWorkflow_HappyPath()
        {
            SeedWorkflowBite("Workflow1");
            var executor = new FakeWorkflowExecutor(_ => SuccessExecutionResult());
            var function = NewFunctions(workflowExecutor: executor);
            var (ctx, req) = NewRequest("execute_workflow", "{\"name\":\"Workflow1\"}");

            var (status, body) = await Invoke(function.ExecuteWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.OK, status);
            var json = JObject.Parse(body);
            Assert.AreEqual("success", json["status"]?.ToString());
            Assert.IsFalse(string.IsNullOrWhiteSpace(json["executionId"]?.ToString()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task SetVar_HappyPath()
        {
            var varName = "MCP_API_TESTS_" + Guid.NewGuid().ToString("N");
            try
            {
                var function = NewFunctions();
                var payload = JsonSerializer.Serialize(new { name = varName, value = "123" });
                var (ctx, req) = NewRequest("set_var", payload);

                var (status, body) = await Invoke(function.SetVar(req, ctx));

                Assert.AreEqual(HttpStatusCode.OK, status);
                var json = JObject.Parse(body);
                Assert.AreEqual(varName, json["name"]?.ToString());
                Assert.AreEqual(true, json["set"]?.ToObject<bool>());
            }
            finally
            {
                Environment.SetEnvironmentVariable(varName, null);
            }
        }

        // set_license and get_license_status are deliberately NOT covered by a wrapper
        // round-trip test here, unlike every other route in this file: both go through the real,
        // process-wide static SubscriptionProvider.Instance singleton (see
        // docs/SubscriptionConfig-IsolatedWorker-Resolution-Spec.md and LicensingHttpFunction,
        // which uses the same singleton directly, not DI). A SetLicense call mutates that shared
        // static state for the rest of this test process, which was confirmed to break unrelated
        // license-gate tests elsewhere in this assembly (WorkflowExecutorTests'
        // Execute_LicenseCheck* tests) when both ran in the same parallel run. Their request-body
        // binding is a mechanical one-line pass-through identical in shape to every other route
        // here; full business-rule coverage (permission gating, partial-update semantics, status
        // validation, secret resolution) lives in SetLicenseToolTests.cs/GetLicenseStatusToolTests.cs
        // against a fully mocked ISubscriptionProvider, with no shared/global state.
        // ── unhandled-exception mapping ──────────────────────────────────────
        //
        // Regression: Invoke used to catch only McpException and JsonException, so anything else a
        // tool handler threw escaped to the Functions host, which answers with a bare 500 carrying
        // an empty body and no log entry of its own. An MCP caller got literally `failed: 500` with
        // nothing to act on — diagnosing the ValidateWorkflowTool.ParseEnvelopeVariables
        // InvalidOperationException on 2026-08-21 needed Application Insights instead of the
        // response. Invoke now catches everything, logs it against the correlation id, and returns
        // the same structured error body as every other failure path.

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task UnhandledException_Returns500_WithStructuredBodyNamingTheException()
        {
            SeedWorkflowBite("Workflow1");
            var executor = new FakeWorkflowExecutor(
                _ => throw new InvalidOperationException("boom from the executor"));
            var function = NewFunctions(workflowExecutor: executor);
            var (ctx, req) = NewRequest("execute_workflow", "{\"name\":\"Workflow1\"}");

            var (status, body) = await Invoke(function.ExecuteWorkflow(req, ctx));

            Assert.AreEqual(HttpStatusCode.InternalServerError, status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(body), "the 500 must carry a body, not be empty");

            var json = JObject.Parse(body);
            Assert.AreEqual("internal_error", json["error"]?.ToString());
            StringAssert.Contains(json["message"]?.ToString(), nameof(InvalidOperationException));
            StringAssert.Contains(json["message"]?.ToString(), "boom from the executor");
            Assert.IsFalse(string.IsNullOrWhiteSpace(json["correlationId"]?.ToString()));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task UnhandledException_IsLoggedAsError_WithTheOriginalException()
        {
            SeedWorkflowBite("Workflow1");
            var logger = new CapturingLogger<McpApiFunctions>();
            var executor = new FakeWorkflowExecutor(
                _ => throw new InvalidOperationException("boom from the executor"));
            var function = NewFunctions(workflowExecutor: executor, logger: logger);
            var (ctx, req) = NewRequest("execute_workflow", "{\"name\":\"Workflow1\"}");

            await Invoke(function.ExecuteWorkflow(req, ctx));

            var errorEntry = logger.Entries.SingleOrDefault(e => e.Level == LogLevel.Error);
            Assert.IsNotNull(errorEntry.Message, "the unhandled exception must be logged at Error level");
            Assert.IsInstanceOfType(errorEntry.Exception, typeof(InvalidOperationException));
            StringAssert.Contains(errorEntry.Message, "unhandled exception");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task McpException_StillMapsTo400_NotSwallowedByTheCatchAll()
        {
            // The catch-all must sit *after* the McpException/JsonException arms, so a validation
            // failure keeps its 400 + specific message rather than degrading to a generic 500.
            var function = NewFunctions();
            var (ctx, req) = NewRequest("get_workflow_definition", "{\"name\":\"NoSuchWorkflow\"}");

            var (status, body) = await Invoke(function.GetWorkflowDefinition(req, ctx));

            Assert.AreEqual(HttpStatusCode.BadRequest, status);
            Assert.AreEqual("bad_request", JObject.Parse(body)["error"]?.ToString());
        }
    }
}
