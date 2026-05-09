/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Tests for ClaimsPrincipalBuilderMiddleware.
 *
 *  Scope: the middleware Invoke() path where GetHttpRequestDataAsync() returns
 *  null (non-HTTP invocations — timer triggers, queue triggers, etc.).  The full
 *  HTTP path requires IFunctionBindingsFeature, which is internal to the SDK
 *  and is exercised end-to-end in the integration test project.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class ClaimsPrincipalBuilderMiddlewareTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ClaimsPrincipalBuilderMiddleware BuildMiddleware(
        params IPrincipalParser[] parsers) =>
        new(parsers, NullLogger<ClaimsPrincipalBuilderMiddleware>.Instance);

    /// <summary>
    /// Tracks whether FunctionExecutionDelegate (next) was invoked.
    /// </summary>
    private static (FunctionExecutionDelegate delegate_, bool[] called) TrackingNext()
    {
        var flag = new bool[1];
        return (ctx => { flag[0] = true; return Task.CompletedTask; }, flag);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Constructor_WithNoParsers_DoesNotThrow()
    {
        var middleware = BuildMiddleware();
        Assert.IsNotNull(middleware);
    }

    [TestMethod]
    public void Constructor_WithParsers_DoesNotThrow()
    {
        var middleware = BuildMiddleware(new StubParser("stub", result: null));
        Assert.IsNotNull(middleware);
    }

    // ── Invoke — null request (non-HTTP trigger) ──────────────────────────────

    [TestMethod]
    public async Task Invoke_NonHttpTrigger_NullRequest_CallsNext()
    {
        // Arrange — FakeInvocationFeatures returns null for IFunctionBindingsFeature
        // so GetHttpRequestDataAsync() returns null (non-HTTP invocation).
        var middleware = BuildMiddleware();
        var context    = new TestFunctionContext();
        var (next, called) = TrackingNext();

        // Act
        await middleware.Invoke(context, next);

        // Assert
        Assert.IsTrue(called[0], "next() should be called for non-HTTP invocations");
    }

    [TestMethod]
    public async Task Invoke_NonHttpTrigger_DoesNotSetPrincipalInItems()
    {
        var middleware = BuildMiddleware();
        var context    = new TestFunctionContext();
        var (next, _)  = TrackingNext();

        await middleware.Invoke(context, next);

        Assert.IsFalse(context.Items.ContainsKey(AuthConstants.PrincipalContextKey),
            "Principal should not be stored for non-HTTP invocations");
    }

    [TestMethod]
    public async Task Invoke_NonHttpTrigger_ZeroParsers_CallsNext()
    {
        var middleware = BuildMiddleware();
        var context    = new TestFunctionContext();
        var (next, called) = TrackingNext();

        await middleware.Invoke(context, next);

        Assert.IsTrue(called[0]);
    }

    // ── Stub parser for constructor tests ─────────────────────────────────────

    private sealed class StubParser : IPrincipalParser
    {
        private readonly WorkflowClaimsPrincipal? _result;
        public string Name { get; }

        public StubParser(string name, WorkflowClaimsPrincipal? result)
        {
            Name    = name;
            _result = result;
        }

        public Task<WorkflowClaimsPrincipal?> TryParseAsync(
            HttpRequestData request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_result);
    }
}
