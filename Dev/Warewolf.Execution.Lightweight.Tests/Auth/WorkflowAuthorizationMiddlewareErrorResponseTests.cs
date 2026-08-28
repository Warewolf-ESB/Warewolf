/*
 * Error-response tests for WorkflowAuthorizationMiddleware.
 *
 * These exercise the status-code dispatch branches of Invoke() and the JSON body / headers /
 * correlation-id shape of each error response, by running the REAL middleware against an
 * HttpFunctionContext (which provides the SDK IFunctionBindingsFeature via FakeBindingsFeature,
 * so context.GetInvocationResult() works in-process — no production test-seam required) and
 * reading the captured HttpResponseData.
 *
 * Contract notes:
 *   • No authenticated principal      → 401 unauthorized   (flat body: error/message/path/correlationId)
 *   • Malformed /secure/ (no workflow) → 400 bad_request    (flat body)
 *   • ConfigMissingDeny               → 503 config_missing  (flat body + workflow)
 *   • NoPolicyFound (open-access)     → pass-through (next), no response written
 *   • Forbidden (policy denial)       → 500 wrapped error    (nested "Error" object) — the engine
 *     currently wraps denials as 500 to match the existing server (middleware lines 239-247,
 *     WOLF-8418); the 403 path is commented out. Flip to 403 when that lands. The wrapped error's
 *     Description is always the fixed string "Insufficient permissions." — the real DenialReason
 *     (caller UPN + group membership) is audit-only and never surfaced in the HTTP response.
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class WorkflowAuthorizationMiddlewareErrorResponseTests
{
    // ── Build / context helpers ───────────────────────────────────────────────

    private static WorkflowAuthorizationMiddleware Build(
        IWorkflowPolicyMatcher?       matcher  = null,
        IRouteAuthorizationRegistry?  registry = null,
        string                        envName  = "Production") =>
        new(
            matcher  ?? new ForbiddenMatcher(),
            registry ?? new NullRegistry(),
            new StubHostEnvironment(envName),
            new AuditLogger(NullLogger<AuditLogger>.Instance),
            NullLogger<WorkflowAuthorizationMiddleware>.Instance);

    private static HttpFunctionContext HttpCtx(string path, string functionName = "TestFn")
    {
        var ctx = new HttpFunctionContext();
        var req = new FakeHttpRequestData(ctx, new Uri($"https://x.test{path}"));
        ctx.SetHttpRequest(req);
        ctx.SetFunctionName(functionName);
        return ctx;
    }

    private static FunctionExecutionDelegate Skip => _ => Task.CompletedTask;

    private static void SetAuthenticatedPrincipal(HttpFunctionContext ctx)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "oid-001"),
            new Claim(ClaimTypes.Name,           "alice@x.com"),
        };
        ctx.Items[AuthConstants.PrincipalContextKey] = new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));
    }

    private static HttpResponseData CapturedResponse(HttpFunctionContext ctx) =>
        ctx.CapturedInvocationResult as HttpResponseData
        ?? throw new AssertFailedException("Middleware did not write a response (CapturedInvocationResult was null).");

    private static async Task<JsonElement> ReadBodyJson(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, leaveOpen: true);
        var json = await reader.ReadToEndAsync();
        return JsonDocument.Parse(json).RootElement;
    }

    // ── Dispatch branches inside Invoke() ─────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_NoAuthenticatedPrincipal_WritesUnauthorized()
    {
        var ctx = HttpCtx("/secure/hello"); // deliberately no principal in ctx.Items

        await Build().Invoke(ctx, Skip);

        var response = CapturedResponse(ctx);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        var body = await ReadBodyJson(response);
        Assert.AreEqual("unauthorized", body.GetProperty("error").GetString());
        Assert.IsTrue(body.TryGetProperty("path", out _),          "401 body must include 'path'.");
        Assert.IsTrue(body.TryGetProperty("correlationId", out _), "401 body must include 'correlationId'.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_AuthenticatedPrincipal_Forbidden_WritesWrapped500()
    {
        var ctx = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        await Build(matcher: new ForbiddenMatcher("group_mismatch")).Invoke(ctx, Skip);

        var response = CapturedResponse(ctx);
        // Current contract: policy denial is wrapped as HTTP 500 (403 is commented out — WOLF-8418).
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await ReadBodyJson(response);
        var err  = body.GetProperty("Error");
        Assert.AreEqual(500, err.GetProperty("Status").GetInt32());
        Assert.AreEqual("internal_server_error", err.GetProperty("Title").GetString());
        // DenialReason (caller UPN + group membership) is audit-only and must never reach the
        // HTTP response — the wrapped error always carries this fixed, PII-free description.
        Assert.AreEqual("Insufficient permissions.", err.GetProperty("Description").GetString(),
            "The wrapped error's Description must be the fixed, PII-free message — never the raw DenialReason.");
        Assert.IsTrue(err.TryGetProperty("CorrelationId", out _), "Wrapped error must include CorrelationId.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_ConfigMissingDeny_WritesServiceUnavailable()
    {
        var ctx = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        await Build(matcher: new ConfigMissingMatcher()).Invoke(ctx, Skip);

        var response = CapturedResponse(ctx);
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await ReadBodyJson(response);
        Assert.AreEqual("config_missing", body.GetProperty("error").GetString());
        Assert.AreEqual("hello", body.GetProperty("workflow").GetString(),
            "503 body must carry the workflow name via the extra bag.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_NoPolicyFound_OpenAccessMode_DoesNotWriteResponse()
    {
        // Open-access mode (BYPASS_SECURE_CONFIG=true equivalent) flows through to next() —
        // no error response should be written.
        var ctx = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        var nextCalled = false;
        await Build(matcher: new NoPolicyFoundMatcher())
            .Invoke(ctx, _ => { nextCalled = true; return Task.CompletedTask; });

        Assert.IsTrue(nextCalled, "Open-access (NoPolicyFound) must call next().");
        Assert.IsNull(ctx.CapturedInvocationResult, "Open-access must not write an error response.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_MalformedSecurePath_WritesBadRequest()
    {
        // /secure/ with no workflow segment → ExtractWorkflowName returns null → 400 BadRequest.
        var ctx = HttpCtx("/secure/");
        SetAuthenticatedPrincipal(ctx);

        await Build().Invoke(ctx, Skip);

        var response = CapturedResponse(ctx);
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadBodyJson(response);
        Assert.AreEqual("bad_request", body.GetProperty("error").GetString());
    }

    // ── Correlation ID resolution ─────────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_ProvidedCorrelationId_IsEchoed()
    {
        var ctx = HttpCtx("/secure/hello");
        var req = (FakeHttpRequestData)await ctx.GetHttpRequestDataAsync();
        req.AddHeader("X-WW-Correlation-Id", "caller-supplied-id");

        await Build().Invoke(ctx, Skip); // no principal → 401 path, which echoes correlation id

        var response = CapturedResponse(ctx);
        Assert.AreEqual("caller-supplied-id",
            response.Headers.GetValues("X-WW-Correlation-Id").Single(),
            "Caller-supplied correlation ID should be echoed verbatim, not replaced.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_NoProvidedCorrelationId_GeneratesShortGuid()
    {
        var ctx = HttpCtx("/secure/hello");

        await Build().Invoke(ctx, Skip);

        var response = CapturedResponse(ctx);
        var corr = response.Headers.GetValues("X-WW-Correlation-Id").Single();
        Assert.AreEqual(16, corr.Length, "Generated correlation IDs should be a 16-char N-format GUID prefix.");
        StringAssert.Matches(corr, new System.Text.RegularExpressions.Regex("^[a-f0-9]{16}$"));
    }

    // ── Stub matchers / host ──────────────────────────────────────────────────

    private sealed class ForbiddenMatcher : IWorkflowPolicyMatcher
    {
        private readonly string _reason;
        public ForbiddenMatcher(string reason = "Insufficient permissions.") => _reason = reason;
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => PolicyMatchResult.DenyGroup(_reason);
    }

    private sealed class ConfigMissingMatcher : IWorkflowPolicyMatcher
    {
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => PolicyMatchResult.DenyConfigMissing("secure.config is absent.");
    }

    private sealed class NoPolicyFoundMatcher : IWorkflowPolicyMatcher
    {
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => PolicyMatchResult.NoPolicy();
    }

    private sealed class NullRegistry : IRouteAuthorizationRegistry
    {
        public WorkflowPermission? GetRequiredPermissions(string functionName) => null;
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string env) => EnvironmentName = env;
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "test";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
