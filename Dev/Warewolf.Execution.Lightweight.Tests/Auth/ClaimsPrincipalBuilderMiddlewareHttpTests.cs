/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  HTTP-path tests for ClaimsPrincipalBuilderMiddleware.
 *  Covers:
 *  - HTTP request present, no parsers → Anonymous stored in context.Items
 *  - HTTP request present, parser returns null → Anonymous stored
 *  - HTTP request present, parser succeeds → authenticated principal stored
 *  - HTTP request present, first parser throws → second parser used
 *  - HTTP request present, first parser throws, second returns null → Anonymous
 *  - next() is always called after principal resolution
 *  - Principal stored under correct context key
 *
 *  The non-HTTP (null request) paths are already covered in
 *  ClaimsPrincipalBuilderMiddlewareTests.cs.
 */

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class ClaimsPrincipalBuilderMiddlewareHttpTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ClaimsPrincipalBuilderMiddleware Build(params IPrincipalParser[] parsers) =>
        new(parsers, NullLogger<ClaimsPrincipalBuilderMiddleware>.Instance);

    private static HttpFunctionContext HttpContext(string path = "/secure/hello")
    {
        var ctx = new HttpFunctionContext();
        var req = new FakeHttpRequestData(ctx, new Uri($"https://x.test{path}"));
        ctx.SetHttpRequest(req);
        return ctx;
    }

    private static (FunctionExecutionDelegate next, bool[] called) TrackingNext()
    {
        var flag = new bool[1];
        return (ctx => { flag[0] = true; return Task.CompletedTask; }, flag);
    }

    private static WorkflowClaimsPrincipal AuthenticatedPrincipal(string name = "alice@x.com")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "oid-001"),
            new Claim(ClaimTypes.Name, name),
            new Claim(AuthConstants.Scope, "user_impersonation"),
        };
        return new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task HttpPath_NoParsers_AnonymousStoredInItems()
    {
        var middleware      = Build(); // no parsers
        var ctx             = HttpContext();
        var (next, called)  = TrackingNext();

        await middleware.Invoke(ctx, next);

        Assert.IsTrue(called[0], "next() must be called");
        Assert.IsTrue(ctx.Items.ContainsKey(AuthConstants.PrincipalContextKey),
            "Principal must be stored in context.Items for HTTP requests");

        var principal = ctx.Items[AuthConstants.PrincipalContextKey] as WorkflowClaimsPrincipal;
        Assert.IsNotNull(principal);
        Assert.IsFalse(principal!.Identity?.IsAuthenticated ?? false,
            "Anonymous principal must not be authenticated");
    }

    [TestMethod]
    public async Task HttpPath_ParserReturnsNull_AnonymousStored()
    {
        var middleware     = Build(new NullParser());
        var ctx            = HttpContext();
        var (next, called) = TrackingNext();

        await middleware.Invoke(ctx, next);

        Assert.IsTrue(called[0]);
        var principal = ctx.Items[AuthConstants.PrincipalContextKey] as WorkflowClaimsPrincipal;
        Assert.IsNotNull(principal);
        Assert.IsFalse(principal!.Identity?.IsAuthenticated ?? false);
    }

    [TestMethod]
    public async Task HttpPath_ParserSucceeds_AuthenticatedPrincipalStored()
    {
        var expected   = AuthenticatedPrincipal("bob@x.com");
        var middleware = Build(new FixedParser(expected));
        var ctx        = HttpContext();
        var (next, _)  = TrackingNext();

        await middleware.Invoke(ctx, next);

        var stored = ctx.Items[AuthConstants.PrincipalContextKey] as WorkflowClaimsPrincipal;
        Assert.IsNotNull(stored);
        Assert.IsTrue(stored!.Identity?.IsAuthenticated == true);
        Assert.AreEqual("bob@x.com", stored.UserName);
    }

    [TestMethod]
    public async Task HttpPath_FirstParserThrows_SecondParserUsed()
    {
        var expected   = AuthenticatedPrincipal("charlie@x.com");
        var middleware = Build(new ThrowingParser(), new FixedParser(expected));
        var ctx        = HttpContext();
        var (next, _)  = TrackingNext();

        await middleware.Invoke(ctx, next);

        var stored = ctx.Items[AuthConstants.PrincipalContextKey] as WorkflowClaimsPrincipal;
        Assert.IsNotNull(stored);
        Assert.AreEqual("charlie@x.com", stored!.UserName);
    }

    [TestMethod]
    public async Task HttpPath_FirstParserThrows_SecondReturnsNull_AnonymousStored()
    {
        var middleware = Build(new ThrowingParser(), new NullParser());
        var ctx        = HttpContext();
        var (next, _)  = TrackingNext();

        await middleware.Invoke(ctx, next);

        var stored = ctx.Items[AuthConstants.PrincipalContextKey] as WorkflowClaimsPrincipal;
        Assert.IsNotNull(stored);
        Assert.IsFalse(stored!.Identity?.IsAuthenticated ?? false);
    }

    [TestMethod]
    public async Task HttpPath_FirstParserSucceeds_SecondParserNeverCalled()
    {
        var expected       = AuthenticatedPrincipal();
        var secondParser   = new TrackingParser();
        var middleware     = Build(new FixedParser(expected), secondParser);
        var ctx            = HttpContext();
        var (next, _)      = TrackingNext();

        await middleware.Invoke(ctx, next);

        Assert.AreEqual(0, secondParser.CallCount,
            "Second parser must be short-circuited when first parser returns authenticated principal");
    }

    [TestMethod]
    public async Task HttpPath_NextAlwaysCalled_EvenWhenAllParsersFail()
    {
        var middleware     = Build(new ThrowingParser(), new ThrowingParser());
        var ctx            = HttpContext();
        var (next, called) = TrackingNext();

        await middleware.Invoke(ctx, next);

        Assert.IsTrue(called[0], "next() must be called even when all parsers throw");
    }

    // ── Stub parsers ──────────────────────────────────────────────────────────

    private sealed class NullParser : IPrincipalParser
    {
        public string Name => "Null";
        public Task<WorkflowClaimsPrincipal?> TryParseAsync(HttpRequestData r, CancellationToken ct)
            => Task.FromResult<WorkflowClaimsPrincipal?>(null);
    }

    private sealed class FixedParser : IPrincipalParser
    {
        private readonly WorkflowClaimsPrincipal _result;
        public string Name => "Fixed";
        public FixedParser(WorkflowClaimsPrincipal result) => _result = result;
        public Task<WorkflowClaimsPrincipal?> TryParseAsync(HttpRequestData r, CancellationToken ct)
            => Task.FromResult<WorkflowClaimsPrincipal?>(_result);
    }

    private sealed class ThrowingParser : IPrincipalParser
    {
        public string Name => "Throwing";
        public Task<WorkflowClaimsPrincipal?> TryParseAsync(HttpRequestData r, CancellationToken ct)
            => throw new InvalidOperationException("Simulated parser failure");
    }

    private sealed class TrackingParser : IPrincipalParser
    {
        public int CallCount { get; private set; }
        public string Name => "Tracking";
        public Task<WorkflowClaimsPrincipal?> TryParseAsync(HttpRequestData r, CancellationToken ct)
        {
            CallCount++;
            return Task.FromResult<WorkflowClaimsPrincipal?>(null);
        }
    }
}
