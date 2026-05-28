/*
 * Error-response tests for EasyAuthRedirectMiddleware — exercises the two
 * branches that write a response and call context.GetInvocationResult():
 *
 *   - browser navigation without auth → 302 to /.auth/login/aad?post_login_redirect_uri=...
 *   - API/non-browser request without auth → 401 with JSON body + WWW-Authenticate
 *
 * The production code calls GetInvocationResult().Value = response, which
 * requires the SDK-internal IFunctionBindingsFeature.  FakeBindingsFeature
 * implements that interface at runtime via DispatchProxy and is registered
 * automatically by HttpFunctionContext, so no production-code changes are
 * needed.  The captured response is read back via ctx.CapturedInvocationResult.
 */

using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EasyAuthRedirectMiddlewareErrorResponseTests
{
    static EasyAuthRedirectMiddleware Build() => new(HostEnvironmentConfig.Load());

    static HttpFunctionContext Ctx(string path, Action<FakeHttpRequestData>? configure = null)
    {
        var ctx = new HttpFunctionContext();
        var req = new FakeHttpRequestData(ctx, new Uri($"https://x.test{path}"));
        configure?.Invoke(req);
        ctx.SetHttpRequest(req);
        ctx.SetFunctionName("TestFn");
        return ctx;
    }

    static FunctionExecutionDelegate Skip => _ => Task.CompletedTask;

    static async Task<string> ReadBody(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    static HttpResponseData CapturedResponse(HttpFunctionContext ctx) =>
        (HttpResponseData)ctx.CapturedInvocationResult!;

    // ── 302 redirect (browser navigation, no auth) ────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_BrowserNavWithoutAuth_RedirectsToAadLogin()
    {
        var mw  = Build();
        var ctx = Ctx("/secure/Hello", req => req.AddHeader("Accept", "text/html,application/xhtml+xml"));

        await mw.Invoke(ctx, Skip);

        var response = CapturedResponse(ctx);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

        var location = response.Headers.GetValues("Location").Single();
        StringAssert.Contains(location, "/.auth/login/aad");
        StringAssert.Contains(location, "post_login_redirect_uri=");
        StringAssert.Contains(location, Uri.EscapeDataString("/secure/Hello"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_BrowserNav_PreservesQueryStringInRedirect()
    {
        var mw  = Build();
        var ctx = new HttpFunctionContext();
        var req = new FakeHttpRequestData(ctx, new Uri("https://x.test/secure/Hello?name=World&debug=1"));
        req.AddHeader("Accept", "text/html");
        ctx.SetHttpRequest(req);
        ctx.SetFunctionName("TestFn");

        await mw.Invoke(ctx, Skip);

        var location = CapturedResponse(ctx).Headers.GetValues("Location").Single();
        StringAssert.Contains(location, Uri.EscapeDataString("?name=World&debug=1"),
            "Query string must survive into post_login_redirect_uri so the user lands on the same URL.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_BrowserNav_WithSecFetchModeCors_Returns401_NotRedirect()
    {
        // Sec-Fetch-Mode: cors means it's an XHR/fetch from a browser, not a
        // top-level navigation — must return 401 JSON, not 302.
        var mw  = Build();
        var ctx = Ctx("/secure/Hello", req =>
        {
            req.AddHeader("Accept",         "text/html");
            req.AddHeader("Sec-Fetch-Mode", "cors");
        });

        await mw.Invoke(ctx, Skip);

        Assert.AreEqual(HttpStatusCode.Unauthorized, CapturedResponse(ctx).StatusCode);
    }

    // ── 401 JSON (API client, no auth) ────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_ApiClientWithoutAuth_Returns401Json()
    {
        var mw  = Build();
        var ctx = Ctx("/secure/Hello", req => req.AddHeader("Accept", "application/json"));

        await mw.Invoke(ctx, Skip);

        var response = CapturedResponse(ctx);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual("application/json", response.Headers.GetValues("Content-Type").Single());
        Assert.AreEqual("Bearer realm=\"warewolf\"",
            response.Headers.GetValues("WWW-Authenticate").Single());

        var body = await ReadBody(response);
        StringAssert.Contains(body, "\"error\":\"unauthorized\"");
        StringAssert.Contains(body, "\"path\":\"/secure/Hello\"");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_ServicesRouteWithoutAuth_Returns401()
    {
        // /services/* should get the same 401 as /secure/* per the AUTH-10 note.
        var mw  = Build();
        var ctx = Ctx("/services/Foo");

        await mw.Invoke(ctx, Skip);

        Assert.AreEqual(HttpStatusCode.Unauthorized, CapturedResponse(ctx).StatusCode);
    }

    // ── Pass-through (auth present) ──────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_BearerToken_PassesThrough()
    {
        var mw  = Build();
        var ctx = Ctx("/secure/Hello", req => req.AddHeader("Authorization", "Bearer some-token"));

        var nextCalled = false;
        await mw.Invoke(ctx, _ => { nextCalled = true; return Task.CompletedTask; });

        Assert.IsTrue(nextCalled, "Bearer-authenticated request must pass to next middleware.");
        Assert.IsNull(ctx.CapturedInvocationResult, "No 401/302 should be written for authenticated requests.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Invoke_EasyAuthPrincipalHeader_PassesThrough()
    {
        var mw  = Build();
        var ctx = Ctx("/secure/Hello", req =>
            req.AddHeader(AuthConstants.ClientPrincipalHeader, "any-non-empty-value"));

        var nextCalled = false;
        await mw.Invoke(ctx, _ => { nextCalled = true; return Task.CompletedTask; });

        Assert.IsTrue(nextCalled);
        Assert.IsNull(ctx.CapturedInvocationResult);
    }
}
