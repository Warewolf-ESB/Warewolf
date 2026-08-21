/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Pins WwExecutionClient's relative-URL construction (area/workflow/query encoding,
 *  slash-segmented workflow names) and its success/failure response handling. Uses a
 *  local stub HttpMessageHandler — no real network calls.
 */

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Warewolf.Execution.ServiceBusWorker;

namespace Warewolf.Execution.ServiceBusWorker.Tests;

[TestClass]
public class WwExecutionClientTests
{
    static WwExecutionClient NewClient(StubHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri("https://engine.test") },
        NullLogger<WwExecutionClient>.Instance);

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteSecureAsync_BuildsCorrectRelativeUrl_NoQuery()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        await NewClient(handler).ExecuteSecureAsync("Hello World", query: null, CancellationToken.None);

        // Uri.ToString() unescapes %20 back to a space for display; AbsoluteUri preserves escaping.
        Assert.AreEqual("https://engine.test/secure/Hello%20World.json", handler.LastRequestUri?.AbsoluteUri);
        Assert.AreEqual(HttpMethod.Get, handler.LastMethod);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteSecureAsync_BuildsCorrectRelativeUrl_WithQueryParams_UrlEncoded()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
        var query = new Dictionary<string, string?> { ["Name"] = "A B", ["Empty"] = null };

        await NewClient(handler).ExecuteSecureAsync("Hello", query, CancellationToken.None);

        var url = handler.LastRequestUri?.AbsoluteUri;
        StringAssert.StartsWith(url, "https://engine.test/secure/Hello.json?");
        StringAssert.Contains(url, "Name=A%20B");
        StringAssert.Contains(url, "Empty=");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteSecureAsync_WorkflowWithSlashSegments_EncodesEachSegmentIndependently()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        await NewClient(handler).ExecuteSecureAsync("data/sales report", query: null, CancellationToken.None);

        Assert.AreEqual("https://engine.test/secure/data/sales%20report.json", handler.LastRequestUri?.AbsoluteUri);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteServiceAsync_UsesServicesArea()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        await NewClient(handler).ExecuteServiceAsync("Hello", query: null, CancellationToken.None);

        Assert.AreEqual("https://engine.test/services/Hello.json", handler.LastRequestUri?.ToString());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecutePublicAsync_UsesPublicArea()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });

        await NewClient(handler).ExecutePublicAsync("Hello", query: null, CancellationToken.None);

        Assert.AreEqual("https://engine.test/public/Hello.json", handler.LastRequestUri?.ToString());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DataRow("")]
    [DataRow("   ")]
    public async Task ExecuteSecureAsync_EmptyOrWhitespaceWorkflowName_ThrowsArgumentException(string workflow)
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => NewClient(handler).ExecuteSecureAsync(workflow, query: null, CancellationToken.None));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteSecureAsync_SuccessResponse_ReturnsBody()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"Result":"ok"}""") });

        var body = await NewClient(handler).ExecuteSecureAsync("Hello", query: null, CancellationToken.None);

        Assert.AreEqual("""{"Result":"ok"}""", body);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ExecuteSecureAsync_NonSuccessResponse_ThrowsWwExecutionException_WithStatusCodeAndBody()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("""{"Error":"denied"}"""),
        });

        var ex = await Assert.ThrowsExceptionAsync<WwExecutionException>(
            () => NewClient(handler).ExecuteSecureAsync("Hello", query: null, CancellationToken.None));

        Assert.AreEqual(HttpStatusCode.InternalServerError, ex.StatusCode);
        Assert.AreEqual("""{"Error":"denied"}""", ex.ResponseBody);
        StringAssert.Contains(ex.RelativeUrl, "secure/Hello.json");
    }

    sealed class StubHandler : HttpMessageHandler
    {
        readonly Func<CancellationToken, Task<HttpResponseMessage>> _respond;

        public StubHandler(Func<CancellationToken, HttpResponseMessage> respond)
            : this(ct => Task.FromResult(respond(ct))) { }

        public StubHandler(Func<CancellationToken, Task<HttpResponseMessage>> respond) => _respond = respond;

        public Uri? LastRequestUri { get; private set; }
        public HttpMethod? LastMethod { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            LastMethod = request.Method;
            return _respond(cancellationToken);
        }
    }
}
