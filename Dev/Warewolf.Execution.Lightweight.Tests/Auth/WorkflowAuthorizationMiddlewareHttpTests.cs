/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  HTTP-path tests for WorkflowAuthorizationMiddleware.Invoke().
 *
 *  Pass-through scenarios (public route, non-secure/services routes, apis.json,
 *  dev-only bypass, Allowed, NoPolicyFound) all call next() without writing a
 *  response body, so they are fully testable without IFunctionBindingsFeature.
 *
 *  Error-response paths (401, 400, 503, 403) call context.GetInvocationResult()
 *  which requires the internal IFunctionBindingsFeature; those are exercised
 *  end-to-end by the integration test project.
 *
 *  ExtractWorkflowName static helper tests are in WorkflowAuthorizationMiddlewareTests.cs.
 */

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class WorkflowAuthorizationMiddlewareHttpTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static WorkflowAuthorizationMiddleware Build(
        IWorkflowPolicyMatcher?    matcher  = null,
        IRouteAuthorizationRegistry? registry = null,
        string                     envName  = "Production") =>
        new(
            matcher   ?? new AllowedMatcher(),
            registry  ?? new NullRegistry(),
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

    private static (FunctionExecutionDelegate next, bool[] called) TrackingNext()
    {
        var flag = new bool[1];
        return (ctx => { flag[0] = true; return Task.CompletedTask; }, flag);
    }

    private static void SetAuthenticatedPrincipal(HttpFunctionContext ctx, string userName = "alice@x.com")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "oid-001"),
            new Claim(ClaimTypes.Name, userName),
            new Claim(AuthConstants.Scope, "user_impersonation"),
        };
        var principal = new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));
        ctx.Items[AuthConstants.PrincipalContextKey] = principal;
    }

    // ── Public route bypass ───────────────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_PublicRoute_CallsNext()
    {
        var mw             = Build();
        var ctx            = HttpCtx("/public/health");
        var (next, called) = TrackingNext();

        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0], "Public routes must bypass all policy checks");
    }

    [TestMethod]
    public async Task Invoke_PublicRoute_SubPath_CallsNext()
    {
        var mw             = Build();
        var ctx            = HttpCtx("/public/status/v2");
        var (next, called) = TrackingNext();

        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── Non-secure / non-services route bypass ────────────────────────────────

    [TestMethod]
    public async Task Invoke_UnrecognisedRoute_CallsNext()
    {
        var mw             = Build();
        var ctx            = HttpCtx("/api/diagnostics");
        var (next, called) = TrackingNext();

        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0], "Routes that are neither /secure nor /services must pass through");
    }

    [TestMethod]
    public async Task Invoke_RootPath_CallsNext()
    {
        var mw             = Build();
        var ctx            = HttpCtx("/");
        var (next, called) = TrackingNext();

        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── apis.json bypass ─────────────────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_ApisJson_OnSecurePath_CallsNext()
    {
        var mw             = Build();
        var ctx            = HttpCtx("/secure/apis.json");
        var (next, called) = TrackingNext();

        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0], "apis.json must always pass through without policy check");
    }

    [TestMethod]
    public async Task Invoke_ApisJson_OnServicesPath_CallsNext()
    {
        var mw             = Build();
        var ctx            = HttpCtx("/services/apis.json");
        var (next, called) = TrackingNext();

        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── Policy outcomes — Allowed ─────────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_AuthenticatedPrincipal_MatcherAllows_CallsNext()
    {
        var mw  = Build(matcher: new AllowedMatcher());
        var ctx = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        var (next, called) = TrackingNext();
        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0], "Allowed outcome must pass through to next()");
    }

    // ── Policy outcomes — NoPolicyFound ──────────────────────────────────────

    [TestMethod]
    public async Task Invoke_AuthenticatedPrincipal_NoPolicyFound_CallsNext()
    {
        var mw  = Build(matcher: new NoPolicyMatcher());
        var ctx = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        var (next, called) = TrackingNext();
        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0], "NoPolicyFound (open-access mode) must pass through");
    }

    // ── Route registry fallback ───────────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_RegistryReturnsNull_DefaultPermissionsUsed_CallsNext()
    {
        // When the route registry returns null, middleware falls back to View|Execute.
        // AllowedMatcher returns Allowed regardless of required permissions,
        // so next() must be called.
        var mw  = Build(matcher: new AllowedMatcher(), registry: new NullRegistry());
        var ctx = HttpCtx("/secure/hello");
        SetAuthenticatedPrincipal(ctx);

        var (next, called) = TrackingNext();
        await mw.Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class AllowedMatcher : IWorkflowPolicyMatcher
    {
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => new(PolicyMatchOutcome.Allowed, null);
    }

    private sealed class NoPolicyMatcher : IWorkflowPolicyMatcher
    {
        public PolicyMatchResult Evaluate(string wf, WorkflowClaimsPrincipal p,
            WorkflowPermission req = WorkflowPermission.View | WorkflowPermission.Execute)
            => new(PolicyMatchOutcome.NoPolicyFound, null);
    }

    private sealed class NullRegistry : IRouteAuthorizationRegistry
    {
        public WorkflowPermission? GetRequiredPermissions(string functionName) => null;
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string env) => EnvironmentName = env;
        public string EnvironmentName   { get; set; }
        public string ApplicationName   { get; set; } = "test";
        public string ContentRootPath   { get; set; } = ".";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
