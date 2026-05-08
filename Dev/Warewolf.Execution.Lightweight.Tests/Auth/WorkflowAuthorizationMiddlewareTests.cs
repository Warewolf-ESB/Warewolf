/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  TST-10 + MWA-06 — Workflow-name extraction edge cases.
 *
 *  Behavioural unit tests for <see cref="WorkflowAuthorizationMiddleware"/>.
 *  Full middleware Invoke() coverage requires HttpRequestData hooks that aren't
 *  publicly invokable (FunctionContext.GetHttpRequestDataAsync requires the
 *  Functions worker host); those scenarios are exercised end-to-end by the
 *  integration test project.  This class focuses on the deterministically
 *  testable workflow-name extraction logic.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Middleware;

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
}
