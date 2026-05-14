/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  TST-11 — EasyAuthRedirectMiddleware browser-vs-API discrimination.
 *
 *  Full Invoke() coverage requires the Functions worker host to bind
 *  HttpRequestData; that is exercised end-to-end in the integration tests.
 *  These unit tests pin down the deterministic browser-detection heuristic.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Middleware;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EasyAuthRedirectMiddlewareTests
{
    private static FakeHttpRequestData NewRequest(string path = "/secure/hello")
        => new(new TestFunctionContext(), new Uri($"https://x.test{path}"));

    [TestMethod]
    public void TST11_BrowserNavigation_DetectedFromAcceptHeader()
    {
        var req = NewRequest();
        req.AddHeader("Accept", "text/html,application/xhtml+xml");
        Assert.IsTrue(EasyAuthRedirectMiddleware.LooksLikeBrowserNavigation(req));
    }

    [TestMethod]
    public void TST11_CorsFetch_NotTreatedAsBrowserNavigation()
    {
        var req = NewRequest();
        req.AddHeader("Accept", "text/html");
        req.AddHeader("Sec-Fetch-Mode", "cors");
        Assert.IsFalse(EasyAuthRedirectMiddleware.LooksLikeBrowserNavigation(req));
    }

    [TestMethod]
    public void TST11_ApiCaller_JsonAccept_NotBrowser()
    {
        var req = NewRequest();
        req.AddHeader("Accept", "application/json");
        Assert.IsFalse(EasyAuthRedirectMiddleware.LooksLikeBrowserNavigation(req));
    }

    [TestMethod]
    public void TST11_NoAcceptHeader_NotBrowser()
    {
        var req = NewRequest();
        Assert.IsFalse(EasyAuthRedirectMiddleware.LooksLikeBrowserNavigation(req));
    }

    [TestMethod]
    public void TST11_NavigateMode_TreatedAsBrowser()
    {
        // Sec-Fetch-Mode=navigate is the explicit "top-level navigation" signal.
        var req = NewRequest();
        req.AddHeader("Accept", "text/html");
        req.AddHeader("Sec-Fetch-Mode", "navigate");
        Assert.IsTrue(EasyAuthRedirectMiddleware.LooksLikeBrowserNavigation(req));
    }
}
