/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  TST-10 + MWA-06 — Workflow-name extraction edge cases.
 *  Also covers the non-HTTP invocation pass-through path of Invoke().
 *
 *  Behavioural unit tests for <see cref="WorkflowAuthorizationMiddleware"/>.
 *  Full middleware Invoke() coverage for HTTP paths requires IFunctionBindingsFeature
 *  (internal to the SDK); those scenarios are exercised end-to-end by the
 *  integration test project.  This class focuses on the deterministically
 *  testable workflow-name extraction logic and the non-HTTP pass-through.
 */

using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
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
public class WorkflowAuthorizationMiddlewareTests
{
    [TestMethod]
    public void TST10_ExtractWorkflowName_SecurePath_ReturnsLowercaseName()
    {
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/HelloWorld", isSecure: true);
        Assert.AreEqual("helloworld", name);
    }

    [TestMethod]
    public void TST10_ExtractWorkflowName_ServicesPath_ReturnsLowercaseName()
    {
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/services/Ping", isSecure: false);
        Assert.AreEqual("ping", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_StripsExtension()
    {
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/HelloWorld.json", isSecure: true);
        Assert.AreEqual("helloworld", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_StripsXmlAndApiSuffixes()
    {
        Assert.AreEqual("helloworld",
            WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/HelloWorld.xml", isSecure: true));
        Assert.AreEqual("helloworld",
            WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/HelloWorld.api", isSecure: true));
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_SubFolder_ReturnsFirstSegment()
    {
        // The middleware enforces policy on the first path segment after the prefix.
        // Sub-folder routes flow through that same segment-based check.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/folder/HelloWorld", isSecure: true);
        Assert.AreEqual("folder", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_TrailingQueryString_Ignored()
    {
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/HelloWorld?wid=abc", isSecure: true);
        Assert.AreEqual("helloworld", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_UrlEncodedSpace_PreservesEncoding()
    {
        // The middleware does NOT URL-decode — secure.config entries are matched
        // against the raw, lower-cased path segment.  Documenting current behaviour.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/Hello%20World", isSecure: true);
        Assert.AreEqual("hello%20world", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_EmptyAfterPrefix_ReturnsNull()
    {
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/", isSecure: true);
        Assert.IsNull(name);
    }

    // ── Invoke — null request (non-HTTP trigger) ──────────────────────────

    private static WorkflowAuthorizationMiddleware BuildMiddleware() =>
        new(
            policyMatcher:   new StubPolicyMatcher(),
            routeRegistry:   new StubRouteRegistry(),
            hostEnvironment: new StubHostEnvironment("Production"),
            auditLogger:     new AuditLogger(NullLogger<AuditLogger>.Instance),
            logger:          NullLogger<WorkflowAuthorizationMiddleware>.Instance);

    [TestMethod]
    public async Task Invoke_NonHttpTrigger_NullRequest_CallsNext()
    {
        // FakeInvocationFeatures → GetHttpRequestDataAsync() returns null
        // → middleware should pass straight through to next().
        var middleware     = BuildMiddleware();
        var context        = new TestFunctionContext();
        var called         = new bool[1];
        FunctionExecutionDelegate next = ctx => { called[0] = true; return Task.CompletedTask; };

        await middleware.Invoke(context, next);

        Assert.IsTrue(called[0], "next() must be called for non-HTTP invocations");
    }

    // ── Stubs ─────────────────────────────────────────────────────────────

    private sealed class StubPolicyMatcher : IWorkflowPolicyMatcher
    {
        public PolicyMatchResult Evaluate(string workflowName, WorkflowClaimsPrincipal principal,
            WorkflowPermission requiredPermissions = WorkflowPermission.View | WorkflowPermission.Execute) =>
            new(PolicyMatchOutcome.Allowed, null);
    }

    private sealed class StubRouteRegistry : IRouteAuthorizationRegistry
    {
        public WorkflowPermission? GetRequiredPermissions(string functionName) => null;
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string envName) => EnvironmentName = envName;
        public string EnvironmentName   { get; set; }
        public string ApplicationName   { get; set; } = "test";
        public string ContentRootPath   { get; set; } = ".";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
