/*
 * Error-response tests for WorkflowAuthorizationMiddleware.
 *
 * The production WriteErrorAsync calls FunctionContext.GetInvocationResult(),
 * which requires the SDK-internal IFunctionBindingsFeature. External tests
 * cannot implement it, so the entire error path (401 / 400 / 403 / 503 —
 * roughly half of WorkflowAuthorizationMiddleware.cs) was dormant.
 *
 * Two seams added alongside these tests make the error path testable:
 *   1. An optional `responseWriter` constructor parameter on the middleware
 *      lets tests capture what would have been set on InvocationResult.
 *   2. WriteErrorAsync was split into BuildErrorResponseAsync (pure response
 *      construction) and a thin wrapper that calls the writer.
 *
 * BuildErrorResponseAsync covers the JSON body / headers / correlation ID
 * shape. The captured-writer tests cover the dispatch branches in Invoke()
 * that pick which status code to return.
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
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
    // ── Test seam: a writer that captures the response instead of setting it ──

    sealed class CapturedResponses
    {
        public HttpResponseData? Last { get; set; }
        public int Count             { get; set; }
        public Action<FunctionContext, HttpResponseData> Writer => (_, r) =>
        {
            Last = r;
            Count++;
        };
    }

    static WorkflowAuthorizationMiddleware Build(
        CapturedResponses                  captured,
        IWorkflowPolicyMatcher?            matcher  = null,
        IRouteAuthorizationRegistry?       registry = null,
        string                             envName  = "Production") =>
        new(
            matcher  ?? new ForbiddenMatcher(),
            registry ?? new NullRegistry(),
            new StubHostEnvironment(envName),
            new AuditLogger(NullLogger<AuditLogger>.Instance),
            NullLogger<WorkflowAuthorizationMiddleware>.Instance,
            responseWriter: captured.Writer);

    static HttpFunctionContext HttpCtx(string path, string functionName = "TestFn")
    {
        var ctx = new HttpFunctionContext();
        var req = new FakeHttpRequestData(ctx, new Uri($"https://x.test{path}"));
        ctx.SetHttpRequest(req);
        ctx.SetFunctionName(functionName);
        return ctx;
    }

    static FunctionExecutionDelegate Skip => _ => Task.CompletedTask;

    static void SetAuthenticatedPrincipal(HttpFunctionContext ctx)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "oid-001"),
            new Claim(ClaimTypes.Name,           "alice@x.com"),
        };
        ctx.Items[AuthConstants.PrincipalContextKey] = new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));
    }

    static async Task<JsonElement> ReadBodyJson(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, leaveOpen: true);
        var json = await reader.ReadToEndAsync();
        return JsonDocument.Parse(json).RootElement;
    }

    // ── BuildErrorResponseAsync: pure response builder ────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task BuildErrorResponseAsync_SetsStatusAndJsonContentType()
    {
        var req = new FakeHttpRequestData(new HttpFunctionContext(),
            new Uri("https://x.test/secure/hello"));

        var response = await WorkflowAuthorizationMiddleware.BuildErrorResponseAsync(
            req, HttpStatusCode.Forbidden, "forbidden",
            "Insufficient permissions.", "/secure/hello", "corr-1");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        var ct = response.Headers.GetValues("Content-Type").Single();
        Assert.AreEqual("application/json", ct);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task BuildErrorResponseAsync_EchoesCorrelationIdInHeader()
    {
        var req = new FakeHttpRequestData(new HttpFunctionContext(),
            new Uri("https://x.test/secure/hello"));

        var response = await WorkflowAuthorizationMiddleware.BuildErrorResponseAsync(
            req, HttpStatusCode.Unauthorized, "unauthorized",
            "Authentication required.", "/secure/hello", "corr-xyz");

        Assert.AreEqual("corr-xyz", response.Headers.GetValues("X-WW-Correlation-Id").Single());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task BuildErrorResponseAsync_BodyHasCanonicalFields()
    {
        var req = new FakeHttpRequestData(new HttpFunctionContext(),
            new Uri("https://x.test/secure/hello"));

        var response = await WorkflowAuthorizationMiddleware.BuildErrorResponseAsync(
            req, HttpStatusCode.BadRequest, "bad_request",
            "Could not determine workflow name from path.", "/secure/",  "c1");
        var body = await ReadBodyJson(response);

        Assert.AreEqual("bad_request",                                   body.GetProperty("error").GetString());
        Assert.AreEqual("Could not determine workflow name from path.",  body.GetProperty("message").GetString());
        Assert.AreEqual("/secure/",                                      body.GetProperty("path").GetString());
        Assert.AreEqual("c1",                                            body.GetProperty("correlationId").GetString());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task BuildErrorResponseAsync_MergesExtraBag_LowercasesKeys()
    {
        var req = new FakeHttpRequestData(new HttpFunctionContext(),
            new Uri("https://x.test/secure/hello"));

        var response = await WorkflowAuthorizationMiddleware.BuildErrorResponseAsync(
            req, HttpStatusCode.ServiceUnavailable, "config_missing",
            "secure.config is absent.", "/secure/hello", "c2",
            extra: new { workflow = "Hello", retryAfter = 30 });
        var body = await ReadBodyJson(response);

        Assert.AreEqual("Hello", body.GetProperty("workflow").GetString(),
            "Anonymous-type property names must be lowercased into the body.");
        Assert.AreEqual(30,      body.GetProperty("retryafter").GetInt32());
    }

    // ── Dispatch branches inside Invoke() ─────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_NoAuthenticatedPrincipal_WritesUnauthorized()
    {
        var captured = new CapturedResponses();
        var mw       = Build(captured);
        var ctx      = HttpCtx("/secure/hello");
        // deliberately no principal in ctx.Items

        await mw.Invoke(ctx, Skip);

        Assert.AreEqual(1, captured.Count);
        Assert.AreEqual(HttpStatusCode.Unauthorized, captured.Last!.StatusCode);
        var body = await ReadBodyJson(captured.Last);
        Assert.AreEqual("unauthorized", body.GetProperty("error").GetString());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_AuthenticatedPrincipal_Forbidden_WritesForbidden()
    {
        var captured = new CapturedResponses();
        var mw       = Build(captured, matcher: new ForbiddenMatcher("group_mismatch"));
        var ctx      = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        await mw.Invoke(ctx, Skip);

        Assert.AreEqual(1, captured.Count);
        Assert.AreEqual(HttpStatusCode.Forbidden, captured.Last!.StatusCode);
        var body = await ReadBodyJson(captured.Last);
        Assert.AreEqual("forbidden",       body.GetProperty("error").GetString());
        Assert.AreEqual("group_mismatch",  body.GetProperty("message").GetString());
        Assert.AreEqual("hello",           body.GetProperty("workflow").GetString());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_ConfigMissingDeny_WritesServiceUnavailable()
    {
        var captured = new CapturedResponses();
        var mw       = Build(captured, matcher: new ConfigMissingMatcher());
        var ctx      = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        await mw.Invoke(ctx, Skip);

        Assert.AreEqual(1, captured.Count);
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, captured.Last!.StatusCode);
        var body = await ReadBodyJson(captured.Last);
        Assert.AreEqual("config_missing", body.GetProperty("error").GetString());
        Assert.AreEqual("hello",          body.GetProperty("workflow").GetString());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_NoPolicyFound_OpenAccessMode_DoesNotWriteResponse()
    {
        // Open-access mode (BYPASS_SECURE_CONFIG=true equivalent) flows through
        // to next() — no error response should be written.
        var captured = new CapturedResponses();
        var mw       = Build(captured, matcher: new NoPolicyFoundMatcher());
        var ctx      = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        var nextCalled = false;
        await mw.Invoke(ctx, _ => { nextCalled = true; return Task.CompletedTask; });

        Assert.IsTrue(nextCalled,            "Open-access must call next()");
        Assert.AreEqual(0, captured.Count,   "Open-access must not write an error response.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_MalformedSecurePath_WritesBadRequest()
    {
        // /secure/ with no workflow segment → ExtractWorkflowName returns null
        // → 400 BadRequest written.
        var captured = new CapturedResponses();
        var mw       = Build(captured);
        var ctx      = HttpCtx("/secure/");
        SetAuthenticatedPrincipal(ctx);

        await mw.Invoke(ctx, Skip);

        Assert.AreEqual(1, captured.Count);
        Assert.AreEqual(HttpStatusCode.BadRequest, captured.Last!.StatusCode);
        var body = await ReadBodyJson(captured.Last);
        Assert.AreEqual("bad_request", body.GetProperty("error").GetString());
    }

    // ── Correlation ID resolution ─────────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_ProvidedCorrelationId_IsEchoed()
    {
        var captured = new CapturedResponses();
        var mw       = Build(captured);
        var ctx      = HttpCtx("/secure/hello");
        var req      = (FakeHttpRequestData)await ctx.GetHttpRequestDataAsync();
        req.AddHeader("X-WW-Correlation-Id", "caller-supplied-id");

        await mw.Invoke(ctx, Skip);

        Assert.AreEqual("caller-supplied-id",
            captured.Last!.Headers.GetValues("X-WW-Correlation-Id").Single(),
            "Caller-supplied correlation ID should be echoed verbatim, not replaced.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_NoProvidedCorrelationId_GeneratesShortGuid()
    {
        var captured = new CapturedResponses();
        var mw       = Build(captured);
        var ctx      = HttpCtx("/secure/hello");

        await mw.Invoke(ctx, Skip);

        var corr = captured.Last!.Headers.GetValues("X-WW-Correlation-Id").Single();
        Assert.AreEqual(16, corr.Length, "Generated correlation IDs should be a 16-char N-format GUID prefix.");
        StringAssert.Matches(corr,
            new System.Text.RegularExpressions.Regex("^[a-f0-9]{16}$"));
    }

    // ── Matchers / stubs ──────────────────────────────────────────────────────

    sealed class ForbiddenMatcher : IWorkflowPolicyMatcher
    {
        readonly string _reason;
        public ForbiddenMatcher(string reason = "Insufficient permissions.") => _reason = reason;
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => new(PolicyMatchOutcome.Forbidden, _reason);
    }

    sealed class ConfigMissingMatcher : IWorkflowPolicyMatcher
    {
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => new(PolicyMatchOutcome.ConfigMissingDeny, null);
    }

    sealed class NoPolicyFoundMatcher : IWorkflowPolicyMatcher
    {
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => new(PolicyMatchOutcome.NoPolicyFound, null);
    }

    sealed class NullRegistry : IRouteAuthorizationRegistry
    {
        public WorkflowPermission? GetRequiredPermissions(string functionName) => null;
    }

    sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string env) => EnvironmentName = env;
        public string EnvironmentName   { get; set; }
        public string ApplicationName   { get; set; } = "test";
        public string ContentRootPath   { get; set; } = ".";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
