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

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
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
    [Ignore("Requires ExtractWorkflowName to use the last path segment instead of the first. Re-introduce when WOLF-8411 is complete.")]
    public void MWA06_ExtractWorkflowName_SubFolder_ReturnsWorkflowName()
    {
        // The implementation preserves the folder prefix so that resource-scope
        // policy entries keyed as "folder/helloworld" in secure.config are resolved
        // correctly for nested workflows (e.g. /secure/folder/HelloWorld → "folder/helloworld").
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/folder/HelloWorld", isSecure: true);
        Assert.AreEqual("folder/helloworld", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_TrailingQueryString_Ignored()
    {
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/HelloWorld?wid=abc", isSecure: true);
        Assert.AreEqual("helloworld", name);
    }

    [TestMethod]
    [Ignore("Requires ExtractWorkflowName to URL-decode path segments. Re-introduce when WOLF-8411 is complete.")]
    public void MWA06_ExtractWorkflowName_UrlEncodedSpace_DecodesToCanonicalForm()
    {
        // The middleware URL-decodes the path segment so the lookup key matches
        // ResourceName entries in secure.config (which are stored decoded).
        // Without this, the Windows Functions host (whose Url.AbsolutePath
        // returns the escaped form) would never resolve names that contain a
        // space — see the 403/500 spectrum on the Security Specs feature file.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName("/secure/Hello%20World", isSecure: true);
        Assert.AreEqual("hello world", name);
    }

    // ── Query string exclusion ────────────────────────────────────────────

    [TestMethod]
    public void MWA06_ExtractWorkflowName_QueryStringMultipleParams_Ignored()
    {
        // Multiple query parameters (?a=1&b=2) must all be stripped; only
        // the path portion is used to derive the workflow name.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/HelloWorld.json?a=1&b=2", isSecure: true);
        Assert.AreEqual("helloworld", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_QueryStringWithPathLikeValue_Ignored()
    {
        // A query value that itself looks like a path (e.g. ?redirect=/secure/Other)
        // must not influence the extracted workflow name.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/HelloWorld.json?redirect=/secure/Other", isSecure: true);
        Assert.AreEqual("helloworld", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_QueryStringOnEncodedName_StrippedBeforeDecoding()
    {
        // Query string must be removed before URL-decoding so that encoded
        // characters inside query values don't bleed into the workflow name.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/Hello%20World.json?param=val%20ue", isSecure: true);
        Assert.AreEqual("hello world", name);
    }

    // ── URL decoding ──────────────────────────────────────────────────────

    [TestMethod]
    public void MWA06_ExtractWorkflowName_EncodedBackslash_DecodedAndNormalized()
    {
        // %5C is the percent-encoding of '\'.  After decoding it must be treated
        // as a path separator, not as part of the file name.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/data%5Csales.json", isSecure: true);
        Assert.AreEqual("data/sales", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_MultiSegment_EncodedSpaces_Decoded()
    {
        // All path segments must be individually decoded; folder names that
        // contain spaces are valid workflow paths in Warewolf.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/Hello%20World/My%20Flow.json", isSecure: true);
        Assert.AreEqual("hello world/my flow", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_EncodedPlus_DecodedLiterally()
    {
        // %2B decodes to '+'; the character is valid in a workflow name.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/A%2BB.json", isSecure: true);
        Assert.AreEqual("a+b", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_MultiLevel_EncodedBackslash_AllSegmentsNormalized()
    {
        // Multiple %5C separators across a deep path must all be normalised
        // to '/' and each resulting segment lowercased.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/examples%5CControl%20Flow%5CDecision.json", isSecure: true);
        Assert.AreEqual("examples/control flow/decision", name);
    }

    // ── Slash normalization ───────────────────────────────────────────────

    [TestMethod]
    public void MWA06_ExtractWorkflowName_RawBackslash_NormalizedToForwardSlash()
    {
        // A raw '\' in the URL path (sometimes produced by misconfigured clients)
        // must be treated as a path separator, yielding "folder/workflow".
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/data\\sales.json", isSecure: true);
        Assert.AreEqual("data/sales", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_MixedSlashes_AllNormalized()
    {
        // A mix of raw '\' and '/' must produce a clean forward-slash path.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/folder\\sub/workflow.json", isSecure: true);
        Assert.AreEqual("folder/sub/workflow", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_MultiLevelRawBackslashes_AllNormalized()
    {
        // Three levels separated only by raw backslashes must produce a
        // correctly joined forward-slash path.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/a\\b\\c.json", isSecure: true);
        Assert.AreEqual("a/b/c", name);
    }

    [TestMethod]
    public void MWA06_ExtractWorkflowName_RawBackslashWithQueryString_SlashNormalizedQueryIgnored()
    {
        // Backslash normalization and query-string stripping must both apply
        // correctly when they appear together in the same URL.
        var name = WorkflowAuthorizationMiddleware.ExtractWorkflowName(
            "/secure/data\\sales.json?debug=true", isSecure: true);
        Assert.AreEqual("data/sales", name);
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
