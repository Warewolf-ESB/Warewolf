/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Http;

namespace Warewolf.Execution.Lightweight.Functions
{
    /// <summary>
    /// Azure Function entry point for MCP-over-HTTP (Streamable HTTP transport),
    /// mounted at <c>/mcp</c> — a sibling, top-level route alongside <c>/Secure/*</c>,
    /// <c>/Public/*</c> and <c>/Services/*</c> (see <c>warewolf-lee-mcp-v3-spec.md</c>,
    /// "Hosting &amp; transport").
    ///
    /// <para>
    /// <b>Hosting model.</b> Built on the low-level <c>ModelContextProtocol.Core</c>
    /// SDK rather than <c>ModelContextProtocol.AspNetCore</c>'s <c>MapMcp()</c>, because
    /// Azure Functions v4 isolated-worker does not use the ASP.NET Core hosting model.
    /// Each POST request gets its own short-lived <see cref="StreamableHttpServerTransport"/>
    /// (<c>Stateless = true</c>) and <see cref="McpServer"/> — there is no cross-request
    /// MCP session state, matching the stateless, one-shot nature of a Functions invocation.
    /// </para>
    ///
    /// <para>
    /// <b>Authentication.</b> The same <see cref="Auth.Middleware.EasyAuthRedirectMiddleware"/> →
    /// <see cref="Auth.Middleware.ClaimsPrincipalBuilderMiddleware"/> pipeline that fronts
    /// <c>/Secure/*</c> runs in front of <c>/mcp</c> too (both are unconditional,
    /// path-independent middleware), producing the same <see cref="WorkflowClaimsPrincipal"/>.
    /// Unlike <c>/Secure/*</c>, <see cref="Auth.Middleware.WorkflowAuthorizationMiddleware"/>
    /// does not recognise <c>/mcp</c> (it only gates <c>/secure/</c> and <c>/services/</c>
    /// path prefixes), because its per-workflow-permission model doesn't fit MCP's shape —
    /// one route multiplexing many tool calls via JSON-RPC, most of which don't name a
    /// workflow at the HTTP layer. This function therefore performs its own baseline
    /// authentication gate: when <c>secure.config</c> is effective, an authenticated
    /// principal is required before any JSON-RPC message is processed (401 otherwise),
    /// mirroring the middleware's own "no authenticated principal" check. When
    /// <c>secure.config</c> is not effective, the instance is in open-access mode and
    /// every request proceeds unauthenticated, exactly as <c>/Public/*</c> does today.
    /// Per-tool-call permission checks (e.g. <c>add_step</c> requiring Contribute) are
    /// added alongside each tool implementation as tools are built out; <c>list_workflows</c>
    /// (the first registered tool) performs its own per-item View-permission filter — see
    /// <see cref="Mcp.ToolHandlers.ListWorkflowsTool"/>.
    /// </para>
    /// </summary>
    public sealed class McpFunction
    {
        readonly McpServerOptions _serverOptions;
        readonly ILoggerFactory _loggerFactory;
        readonly IServiceProvider _serviceProvider;
        readonly IWorkflowAuthPolicyLoader _authPolicyLoader;
        readonly ILogger<McpFunction> _logger;

        public McpFunction(
            McpServerOptions serverOptions,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ILogger<McpFunction> logger)
        {
            _serverOptions = serverOptions;
            _loggerFactory = loggerFactory;
            _serviceProvider = serviceProvider;
            _authPolicyLoader = authPolicyLoader;
            _logger = logger;
        }

        /// <summary>
        /// Handles the MCP Streamable HTTP endpoint. Only POST (JSON-RPC request/response)
        /// is supported in this increment — GET (the optional SSE notification stream) is
        /// rejected because <see cref="StreamableHttpServerTransport.Stateless"/> is <c>true</c>
        /// and does not support server-initiated messages.
        /// </summary>
        [Function("Mcp")]
        public async Task<HttpResponseData> HandleMcp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "mcp")] HttpRequestData req,
            FunctionContext context)
        {
            var correlationId = HttpResponseHelper.ResolveCorrelationId(req);

            if (req.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                return await WriteErrorResponse(req, HttpStatusCode.MethodNotAllowed,
                    "method_not_allowed",
                    "The MCP SSE notification stream (GET) is not supported by this server; use POST for JSON-RPC requests.",
                    correlationId);
            }

            if (!TryAuthenticate(context, out var principal))
            {
                _logger.LogWarning("MCP request rejected: no authenticated principal (correlationId={CorrelationId})", correlationId);
                return await WriteErrorResponse(req, HttpStatusCode.Unauthorized,
                    "unauthorized", "Authentication required.", correlationId);
            }

            JsonRpcMessage? message;
            try
            {
                message = await JsonSerializer.DeserializeAsync<JsonRpcMessage>(
                    req.Body, McpJsonUtilities.DefaultOptions, context.CancellationToken);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "MCP request rejected: malformed JSON-RPC body (correlationId={CorrelationId})", correlationId);
                return await WriteErrorResponse(req, HttpStatusCode.BadRequest,
                    "bad_request", "Request body is not a valid JSON-RPC message.", correlationId);
            }

            if (message is null)
            {
                return await WriteErrorResponse(req, HttpStatusCode.BadRequest,
                    "bad_request", "Request body is empty.", correlationId);
            }

            if (principal is not null)
            {
                message.Context = new JsonRpcMessageContext { User = principal };
            }

            await using var transport = new StreamableHttpServerTransport(_loggerFactory)
            {
                Stateless = true,
            };

            var server = McpServer.Create(transport, _serverOptions, _loggerFactory, _serviceProvider);
            var runTask = server.RunAsync(context.CancellationToken);

            var response = req.CreateResponse();
            response.Headers.Add(HttpResponseHelper.CorrelationIdHeader, correlationId);

            bool wroteBody;
            try
            {
                wroteBody = await transport.HandlePostRequestAsync(message, response.Body, context.CancellationToken);
            }
            finally
            {
                // Completing the transport lets the McpServer.RunAsync message loop
                // observe end-of-stream and finish; awaiting it here bounds the
                // background pump's lifetime to this single request/response.
                await transport.DisposeAsync();
                try
                {
                    await runTask;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "MCP server run loop ended with an exception after request handling (correlationId={CorrelationId})", correlationId);
                }
            }

            if (wroteBody)
            {
                response.StatusCode = HttpStatusCode.OK;
                // StreamableHttpServerTransport.HandlePostRequestAsync writes SSE-framed
                // events ("event: message\ndata: {...}\n\n"), not bare JSON — mirrors the
                // official ModelContextProtocol.AspNetCore handler's InitializeSseResponse.
                response.Headers.Add("Content-Type", "text/event-stream");
                response.Headers.Add("Cache-Control", "no-cache,no-store");
            }
            else
            {
                // No JsonRpcRequest was present (e.g. a notification or a response) —
                // per StreamableHttpServerTransport.HandlePostRequestAsync's contract,
                // the caller should reply with an empty 202 Accepted.
                response.StatusCode = HttpStatusCode.Accepted;
            }

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
                // Open-access mode — no secure.config, or config has zero permission
                // entries. Mirrors /Public/* behaviour: every caller proceeds.
                return true;
            }

            return principal?.Identity?.IsAuthenticated == true;
        }

        static async Task<HttpResponseData> WriteErrorResponse(
            HttpRequestData req, HttpStatusCode statusCode,
            string error, string message, string correlationId)
        {
            // Function methods must return their own HttpResponseData directly (unlike
            // middleware, which sets context.GetInvocationResult().Value) — build the
            // same flat { error, message, path, correlationId } shape inline. Serialized
            // manually (rather than via WriteAsJsonAsync) because that extension requires
            // an ObjectSerializer registered on the Functions worker, which isn't
            // guaranteed to be configured — every other Function in this codebase avoids
            // it for the same reason (see HttpResponseHelper.WriteErrorAsync).
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
    }
}
