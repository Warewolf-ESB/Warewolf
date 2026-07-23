/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using Warewolf.Execution.EngineJobProcessor.Services;

namespace Warewolf.Execution.EngineJobProcessor.Tests;

/// <summary>
/// Pins the fire-and-forget dispatch contract (architecture §4.2): 202 → Dispatched,
/// 409 → AlreadyClaimed (benign duplicate), anything else / timeout / network error →
/// Failed — and <c>TryResumeAsync</c> never throws, so one bad job cannot abort a poll
/// tick. Uses a stub <see cref="HttpMessageHandler"/>; auth disabled (token acquisition
/// is Azure.Identity's responsibility, not re-tested here).
/// </summary>
[TestClass]
[DoNotParallelize] // EngineResumeClient's ctor validation reads process-global Config.Persistence
public class EngineResumeClientTests
{
    static ProcessorSettings Settings(int timeoutSeconds = 5) => new()
    {
        EngineResumeBaseUrl = "https://engine.test",
        AuthDisabled = true,
        ResumeTimeoutSeconds = timeoutSeconds,
    };

    static EngineResumeClient NewClient(StubHandler handler, int timeoutSeconds = 5) => new(
        new HttpClient(handler),
        credential: null,
        Settings(timeoutSeconds),
        NullLogger<EngineResumeClient>.Instance);

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task TryResumeAsync_202_Dispatched_AndTargetsResumeRoute()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        var outcome = await NewClient(handler).TryResumeAsync("job-42", CancellationToken.None);

        Assert.AreEqual(ResumeDispatchOutcome.Dispatched, outcome);
        Assert.AreEqual("https://engine.test/secure/resume/job-42", handler.LastRequestUri?.ToString());
        Assert.AreEqual(HttpMethod.Post, handler.LastMethod);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task TryResumeAsync_200_Dispatched_EngineExecutedSynchronously()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));

        var outcome = await NewClient(handler).TryResumeAsync("job-42", CancellationToken.None);

        Assert.AreEqual(ResumeDispatchOutcome.Dispatched, outcome,
            "The engine's resume route responds 200 on synchronous completion — that is a successful dispatch.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task TryResumeAsync_409_AlreadyClaimed()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Conflict));

        var outcome = await NewClient(handler).TryResumeAsync("job-42", CancellationToken.None);

        Assert.AreEqual(ResumeDispatchOutcome.AlreadyClaimed, outcome);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task TryResumeAsync_ServerError_Failed()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var outcome = await NewClient(handler).TryResumeAsync("job-42", CancellationToken.None);

        Assert.AreEqual(ResumeDispatchOutcome.Failed, outcome);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task TryResumeAsync_NetworkException_Failed_NotThrown()
    {
        var handler = new StubHandler(_ => Task.FromException<HttpResponseMessage>(new HttpRequestException("connection refused")));

        var outcome = await NewClient(handler).TryResumeAsync("job-42", CancellationToken.None);

        Assert.AreEqual(ResumeDispatchOutcome.Failed, outcome);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task TryResumeAsync_SlowEngine_AckTimeout_NotThrown()
    {
        var handler = new StubHandler(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(30), ct); // far beyond the 1s ack timeout
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        var outcome = await NewClient(handler, timeoutSeconds: 1).TryResumeAsync("job-42", CancellationToken.None);

        Assert.AreEqual(ResumeDispatchOutcome.AckTimeout, outcome,
            "A slow (synchronously executing) engine maps to AckTimeout — not Failed, never a hang or throw; " +
            "the job-state machine reconciles on the next tick.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task TryResumeAsync_JobIdIsUriEscaped()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted));

        await NewClient(handler).TryResumeAsync("job/../42 x", CancellationToken.None);

        StringAssert.Contains(handler.LastRequestUri!.AbsoluteUri, "job%2F..%2F42%20x",
            "Job ids must be escaped so they cannot alter the request path.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Ctor_PersistenceEnabled_MissingBaseUrl_FailsFast()
    {
        using var _ = TestSupport.SwapPersistence(enable: true);

        Assert.ThrowsException<InvalidOperationException>(() => new EngineResumeClient(
            new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted))),
            credential: null,
            new ProcessorSettings { AuthDisabled = true }, // no ENGINE_RESUME_BASEURL
            NullLogger<EngineResumeClient>.Instance));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Ctor_PersistenceDisabled_MissingConfig_DoesNotThrow()
    {
        using var restore = TestSupport.SwapPersistence(enable: false);

        _ = new EngineResumeClient(
            new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Accepted))),
            credential: null,
            new ProcessorSettings(), // nothing configured — inert client on a disabled processor
            NullLogger<EngineResumeClient>.Instance);
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
