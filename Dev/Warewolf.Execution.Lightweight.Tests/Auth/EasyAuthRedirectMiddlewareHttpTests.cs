/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  HTTP-path tests for EasyAuthRedirectMiddleware.Invoke().
 *  Covers pass-through scenarios that call next() without writing a response —
 *  these paths are fully testable without IFunctionBindingsFeature.
 *
 *  Error-response paths (302 browser redirect, 401 JSON) call
 *  context.GetInvocationResult() which requires the internal
 *  IFunctionBindingsFeature and are exercised end-to-end by the integration
 *  test project.
 *
 *  The LooksLikeBrowserNavigation helper and non-HTTP pass-through are
 *  already covered in EasyAuthRedirectMiddlewareTests.cs.
 */

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EasyAuthRedirectMiddlewareHttpTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static EasyAuthRedirectMiddleware NewMiddleware() =>
        new(NullLogger<EasyAuthRedirectMiddleware>.Instance);

    private static (HttpFunctionContext ctx, FakeHttpRequestData req) HttpCtx(string path)
    {
        var ctx = new HttpFunctionContext();
        var req = new FakeHttpRequestData(ctx, new Uri($"https://x.test{path}"));
        ctx.SetHttpRequest(req);
        return (ctx, req);
    }

    private static (FunctionExecutionDelegate next, bool[] called) TrackingNext()
    {
        var flag = new bool[1];
        return (ctx => { flag[0] = true; return Task.CompletedTask; }, flag);
    }

    // ── Public route bypass ───────────────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_PublicRoute_CallsNext()
    {
        var (ctx, _) = HttpCtx("/public/health");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0], "Public route must pass straight through");
    }

    [TestMethod]
    public async Task Invoke_PublicRoute_SubPath_CallsNext()
    {
        var (ctx, _)       = HttpCtx("/public/ping/v2");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── Non-secure / non-services route bypass ────────────────────────────────

    [TestMethod]
    public async Task Invoke_UnrecognisedRoute_NotSecureOrServices_CallsNext()
    {
        var (ctx, _)       = HttpCtx("/api/status");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0], "Unrecognised routes must pass straight through");
    }

    [TestMethod]
    public async Task Invoke_RootPath_CallsNext()
    {
        var (ctx, _)       = HttpCtx("/");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── apis.json bypass ─────────────────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_ApisJson_OnSecurePath_CallsNext()
    {
        var (ctx, _)       = HttpCtx("/secure/apis.json");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0], "apis.json must always pass through without auth");
    }

    [TestMethod]
    public async Task Invoke_ApisJson_OnServicesPath_CallsNext()
    {
        var (ctx, _)       = HttpCtx("/services/apis.json");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── Authenticated — EasyAuth principal header ─────────────────────────────

    [TestMethod]
    public async Task Invoke_SecureRoute_WithEasyAuthPrincipalHeader_CallsNext()
    {
        var (ctx, req)     = HttpCtx("/secure/hello-world");
        req.AddHeader(AuthConstants.ClientPrincipalHeader, "dGVzdA=="); // non-empty base64
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0], "Authenticated request (EasyAuth header) must pass to next");
    }

    [TestMethod]
    public async Task Invoke_ServicesRoute_WithEasyAuthPrincipalHeader_CallsNext()
    {
        var (ctx, req)     = HttpCtx("/services/my-workflow");
        req.AddHeader(AuthConstants.ClientPrincipalHeader, "dGVzdA==");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }

    // ── Authenticated — Bearer token header ──────────────────────────────────

    [TestMethod]
    public async Task Invoke_SecureRoute_WithBearerAuthHeader_CallsNext()
    {
        var (ctx, req)     = HttpCtx("/secure/workflow-x");
        req.AddHeader("Authorization", "Bearer eyJ.test.token");
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0], "Request with Bearer token must pass to next");
    }

    [TestMethod]
    public async Task Invoke_SecureRoute_BearerSchemeIsCaseInsensitive_CallsNext()
    {
        var (ctx, req)     = HttpCtx("/secure/workflow-x");
        req.AddHeader("Authorization", "bearer eyJ.test.token"); // lowercase
        var (next, called) = TrackingNext();

        await NewMiddleware().Invoke(ctx, next);

        Assert.IsTrue(called[0]);
    }
}
