/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Runtime.Subscription;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Http;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Functions
{
    /// <summary>
    /// Plain REST wrappers around the 16 workflow-authoring and licensing tool handlers under
    /// <c>Mcp/ToolHandlers/</c>, one HTTP-triggered POST route per tool under
    /// <c>/mcp-api/{tool_name}</c>. Replaces the retired <c>/mcp</c> JSON-RPC/SSE
    /// endpoint (formerly <c>McpFunction</c>).
    ///
    /// <para>
    /// <b>Why this exists instead of the MCP JSON-RPC transport.</b> The official
    /// <c>ModelContextProtocol.Client</c> SDK always issues chunked-transfer POST
    /// bodies on .NET (<c>McpHttpClient.CreatePostBodyContent</c> uses
    /// <c>JsonContent.Create</c>, which never sets <c>Content-Length</c>). Azure
    /// Functions' isolated-worker gRPC relay
    /// (<c>azure-functions-host</c>'s <c>GrpcMessageConversionExtensions.ToRpcHttp</c>)
    /// only forwards the HTTP body to the worker when <c>request.ContentLength &gt; 0</c>
    /// — a chunked request has no <c>Content-Length</c> header at all, so the gate is
    /// false and the body is silently dropped before it ever reaches this process. This
    /// is a confirmed, open, host-side bug (Azure/azure-functions-host#7930) reproduced
    /// against both <c>func start</c> and real deployed Function Apps — nothing in this
    /// codebase can fix it. Rather than work around an upstream Microsoft bug, the MCP
    /// protocol surface (JSON-RPC framing, SSE transport) has been retired from this
    /// engine entirely; these tools are now exposed as ordinary REST endpoints with
    /// normal string-bodied JSON request/response payloads (no chunking risk). A
    /// separate, plain Node/Express MCP server (<c>warewolf-devops-mcp</c>) calls these
    /// endpoints over <c>fetch()</c> (auto <c>Content-Length</c>, never chunked) to
    /// re-expose them over the real MCP protocol — Node's own HTTP server has no
    /// host/worker relay, so it is immune to this bug.
    /// </para>
    ///
    /// <para>
    /// <b>Authentication.</b> Same baseline gate the retired <c>McpFunction</c> used:
    /// when <c>secure.config</c> is effective, an authenticated principal (built by
    /// <see cref="Auth.Middleware.ClaimsPrincipalBuilderMiddleware"/>) is required (401
    /// otherwise); in open-access mode every caller proceeds. Per-tool permission
    /// checks (Contribute/Execute/Administrator/View) are enforced inside each tool
    /// handler exactly as before — unchanged by this move. Callers are expected to
    /// forward their own Entra bearer token on each request so per-user authorization
    /// still applies (no service-identity passthrough).
    /// </para>
    ///
    /// <para>
    /// <b>Error mapping.</b> A tool handler throwing <see cref="McpException"/> (its
    /// established mechanism for parameter validation, not-found and permission-denied
    /// errors alike) is mapped to <c>400 Bad Request</c> with the same flat
    /// <c>{error,message,path,correlationId}</c> body <see cref="HttpResponseHelper"/>
    /// uses elsewhere in this codebase.
    /// </para>
    /// </summary>
    public sealed class McpApiFunctions
    {
        static readonly JsonSerializerOptions RequestJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        readonly HostEnvironmentConfig _hostConfig;
        readonly IWorkflowAuthPolicyLoader _authPolicyLoader;
        readonly IWorkflowExecutor _workflowExecutor;
        readonly IServiceProvider _serviceProvider;
        readonly ILogger<McpApiFunctions> _logger;

        public McpApiFunctions(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            IWorkflowExecutor workflowExecutor,
            IServiceProvider serviceProvider,
            ILogger<McpApiFunctions> logger)
        {
            _hostConfig = hostConfig;
            _authPolicyLoader = authPolicyLoader;
            _workflowExecutor = workflowExecutor;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Resolves <see cref="IMcpSecretResolver"/> lazily from DI rather than accepting it
        /// as a constructor parameter — that interface is <c>internal</c>, and a <c>public</c>
        /// Function class (required for Functions Worker discovery) cannot expose an
        /// internal type in a public constructor's signature (CS0051).
        /// </summary>
        IMcpSecretResolver SecretResolver => _serviceProvider.GetRequiredService<IMcpSecretResolver>();

        [Function("McpApiListWorkflows")]
        public Task<HttpResponseData> ListWorkflows(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/list_workflows")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<ListWorkflowsRequest>(req, ct) ?? new ListWorkflowsRequest();
                return ListWorkflowsTool.Handle(_hostConfig, _authPolicyLoader, principal, p.Folder, p.Cursor, p.PageSize);
            });

        [Function("McpApiListTools")]
        public Task<HttpResponseData> ListTools(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/list_tools")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, (_, _) => Task.FromResult<object>(ListToolsTool.Handle()));

        [Function("McpApiGetWorkflowDefinition")]
        public Task<HttpResponseData> GetWorkflowDefinition(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/get_workflow_definition")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<NameRequest>(req, ct) ?? new NameRequest();
                return GetWorkflowDefinitionTool.Handle(_hostConfig, _authPolicyLoader, principal, p.Name);
            });

        [Function("McpApiGetWorkflowSchema")]
        public Task<HttpResponseData> GetWorkflowSchema(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/get_workflow_schema")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, (_, _) => Task.FromResult<object>(GetWorkflowSchemaTool.Handle()));

        [Function("McpApiGetToolSchema")]
        public Task<HttpResponseData> GetToolSchema(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/get_tool_schema")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (_, ct) =>
            {
                var p = await ReadBody<ToolNameRequest>(req, ct) ?? new ToolNameRequest();
                return GetToolSchemaTool.Handle(p.ToolName);
            });

        [Function("McpApiValidateWorkflow")]
        public Task<HttpResponseData> ValidateWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/validate_workflow")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (_, ct) =>
            {
                var p = await ReadBody<EnvelopeBodyRequest>(req, ct) ?? new EnvelopeBodyRequest();
                return ValidateWorkflowTool.Handle(p.Envelope, p.Body);
            });

        [Function("McpApiCreateWorkflow")]
        public Task<HttpResponseData> CreateWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/create_workflow")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<NamedEnvelopeBodyRequest>(req, ct) ?? new NamedEnvelopeBodyRequest();
                return CreateWorkflowTool.Handle(_hostConfig, _authPolicyLoader, principal, p.Name, p.Envelope, p.Body);
            });

        [Function("McpApiEditWorkflow")]
        public Task<HttpResponseData> EditWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/edit_workflow")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<NamedEnvelopeBodyRequest>(req, ct) ?? new NamedEnvelopeBodyRequest();
                return EditWorkflowTool.Handle(_hostConfig, _authPolicyLoader, principal, p.Name, p.Envelope, p.Body);
            });

        [Function("McpApiDeployWorkflow")]
        public Task<HttpResponseData> DeployWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/deploy_workflow")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<DeployWorkflowRequest>(req, ct) ?? new DeployWorkflowRequest();
                return DeployWorkflowTool.Handle(_hostConfig, _authPolicyLoader, principal, p.Name, p.BiteContent, p.Overwrite);
            });

        [Function("McpApiAddStep")]
        public Task<HttpResponseData> AddStep(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/add_step")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<AddStepRequest>(req, ct) ?? new AddStepRequest();
                return AddStepTool.Handle(_hostConfig, _authPolicyLoader, principal, p.Name, p.Step, p.AfterStepId, p.Branch);
            });

        [Function("McpApiAddSource")]
        public Task<HttpResponseData> AddSource(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/add_source")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<SourceRequest>(req, ct) ?? new SourceRequest();
                return await AddSourceTool.Handle(_hostConfig, _authPolicyLoader, SecretResolver, principal, p.Name, p.SourceType, p.Config, ct);
            });

        [Function("McpApiEditSource")]
        public Task<HttpResponseData> EditSource(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/edit_source")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<SourceRequest>(req, ct) ?? new SourceRequest();
                return await EditSourceTool.Handle(_hostConfig, _authPolicyLoader, SecretResolver, principal, p.Name, p.SourceType, p.Config, ct);
            });

        [Function("McpApiExecuteWorkflow")]
        public Task<HttpResponseData> ExecuteWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/execute_workflow")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<ExecuteWorkflowRequest>(req, ct) ?? new ExecuteWorkflowRequest();
                return await ExecuteWorkflowTool.Handle(_hostConfig, _authPolicyLoader, _workflowExecutor, principal, p.Name, p.Inputs, ct);
            });

        [Function("McpApiSetVar")]
        public Task<HttpResponseData> SetVar(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/set_var")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<SetVarRequest>(req, ct) ?? new SetVarRequest();
                return SetVarTool.Handle(_authPolicyLoader, principal, p.Name, p.Value);
            });

        [Function("McpApiSetLicense")]
        public Task<HttpResponseData> SetLicense(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/set_license")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, async (principal, ct) =>
            {
                var p = await ReadBody<SetLicenseRequest>(req, ct) ?? new SetLicenseRequest();
                return await SetLicenseTool.Handle(
                    _authPolicyLoader, SecretResolver, SubscriptionProvider.Instance, principal,
                    p.CustomerId, p.PlanId, p.SubscriptionId, p.MarketplaceResourceId, p.Status,
                    p.SubscriptionKey, p.StopExecutions, ct);
            });

        [Function("McpApiGetLicenseStatus")]
        public Task<HttpResponseData> GetLicenseStatus(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "mcp-api/get_license_status")] HttpRequestData req,
            FunctionContext context)
            => Invoke(req, context, (_, _) => Task.FromResult<object>(GetLicenseStatusTool.Handle(SubscriptionProvider.Instance)));

        // ── Shared plumbing ──────────────────────────────────────────────────

        /// <summary>
        /// Runs the baseline authentication gate, invokes <paramref name="callback"/>
        /// with the resolved principal, and maps its result (or any thrown
        /// <see cref="McpException"/>/malformed-JSON error) onto an
        /// <see cref="HttpResponseData"/> — the same 401/400 shapes the retired
        /// <c>McpFunction</c> used to produce.
        /// </summary>
        async Task<HttpResponseData> Invoke(
            HttpRequestData req, FunctionContext context,
            Func<WorkflowClaimsPrincipal?, CancellationToken, Task<object>> callback)
        {
            var correlationId = HttpResponseHelper.ResolveCorrelationId(req);

            if (!TryAuthenticate(context, out var principal))
            {
                _logger.LogWarning("MCP API request rejected: no authenticated principal (correlationId={CorrelationId})", correlationId);
                return await WriteErrorResponse(req, HttpStatusCode.Unauthorized,
                    "unauthorized", "Authentication required.", correlationId);
            }

            object result;
            try
            {
                result = await callback(principal, context.CancellationToken);
            }
            catch (McpException ex)
            {
                _logger.LogWarning(ex, "MCP API request rejected by tool handler (correlationId={CorrelationId})", correlationId);
                return await WriteErrorResponse(req, HttpStatusCode.BadRequest,
                    "bad_request", ex.Message, correlationId);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "MCP API request rejected: malformed JSON body (correlationId={CorrelationId})", correlationId);
                return await WriteErrorResponse(req, HttpStatusCode.BadRequest,
                    "bad_request", "Request body is not valid JSON.", correlationId);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add(HttpResponseHelper.CorrelationIdHeader, correlationId);
            await response.WriteStringAsync(JsonSerializer.Serialize(result));
            return response;
        }

        /// <summary>
        /// Resolves the <see cref="WorkflowClaimsPrincipal"/> built by
        /// <see cref="Auth.Middleware.ClaimsPrincipalBuilderMiddleware"/> and applies the
        /// same baseline authentication requirement <see cref="Auth.Middleware.WorkflowAuthorizationMiddleware"/>
        /// applies to <c>/Secure/*</c>: when <c>secure.config</c> is effective, an
        /// authenticated principal is mandatory; otherwise the instance is in
        /// open-access mode and every caller is allowed through.
        /// </summary>
        bool TryAuthenticate(FunctionContext context, out WorkflowClaimsPrincipal? principal)
        {
            context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var principalObj);
            principal = principalObj as WorkflowClaimsPrincipal;

            if (!_authPolicyLoader.IsConfigEffective)
            {
                return true;
            }

            return principal?.Identity?.IsAuthenticated == true;
        }

        static async Task<HttpResponseData> WriteErrorResponse(
            HttpRequestData req, HttpStatusCode statusCode,
            string error, string message, string correlationId)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");
            response.Headers.Add(HttpResponseHelper.CorrelationIdHeader, correlationId);
            await response.WriteStringAsync(JsonSerializer.Serialize(new
            {
                error,
                message,
                path = req.Url.AbsolutePath,
                correlationId,
            }, HttpResponseHelper.CamelCaseOptions));
            return response;
        }

        static async Task<T?> ReadBody<T>(HttpRequestData req, CancellationToken ct)
        {
            if (req.Body is null)
            {
                return default;
            }

            using var reader = new StreamReader(req.Body, leaveOpen: true);
            var text = await reader.ReadToEndAsync(ct);
            if (string.IsNullOrWhiteSpace(text))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(text, RequestJsonOptions);
        }
    }

    // ── Request DTOs ─────────────────────────────────────────────────────────
    // One record per tool, mirroring its Handle(...) parameter names. Every
    // property defaults so an empty/whitespace-only body still binds to a valid
    // instance — the tool handlers themselves already validate required fields
    // (e.g. `name`) via McpException, so no duplicate validation is needed here.

    sealed record ListWorkflowsRequest(string? Folder = null, string? Cursor = null, int? PageSize = null);

    sealed record NameRequest(string Name = "");

    sealed record ToolNameRequest([property: JsonPropertyName("tool_name")] string ToolName = "");

    sealed record EnvelopeBodyRequest(JsonElement Envelope = default, JsonElement Body = default);

    sealed record NamedEnvelopeBodyRequest(string Name = "", JsonElement Envelope = default, JsonElement Body = default);

    sealed record DeployWorkflowRequest(string Name = "", string BiteContent = "", bool Overwrite = false);

    sealed record AddStepRequest(string Name = "", JsonElement Step = default, string? AfterStepId = null, string? Branch = null);

    sealed record SourceRequest(string Name = "", string SourceType = "", JsonElement Config = default);

    sealed record ExecuteWorkflowRequest(string Name = "", JsonElement? Inputs = null);

    sealed record SetVarRequest(string Name = "", string? Value = null);

    sealed record SetLicenseRequest(
        string? CustomerId = null,
        string? PlanId = null,
        string? SubscriptionId = null,
        string? MarketplaceResourceId = null,
        string? Status = null,
        string? SubscriptionKey = null,
        bool? StopExecutions = null);
}
