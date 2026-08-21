/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Pins WwExecutionTokenHandler's token acquisition/caching/refresh behaviour and its
 *  scoped injection of the Authorization + x-functions-key headers. Uses a mocked
 *  Azure.Core.TokenCredential (never talks to Entra) and a local stub inner handler.
 */

using Azure.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Execution.ServiceBusWorker.Auth;

namespace Warewolf.Execution.ServiceBusWorker.Tests;

[TestClass]
public class WwExecutionTokenHandlerTests
{
    static WwExecutionOptions Options(string? functionKey = null, int refreshSkewSeconds = 120) => new()
    {
        BaseUrl = "https://engine.test",
        TenantId = "tenant-1",
        ResourceAppId = "resource-app-1",
        FunctionKey = functionKey,
        TokenRefreshSkewSeconds = refreshSkewSeconds,
    };

    static AccessToken TokenExpiringIn(TimeSpan span, string token = "token-value") =>
        new(token, DateTimeOffset.UtcNow.Add(span));

    static (WwExecutionTokenHandler handler, InnerStubHandler inner, Mock<TokenCredential> credential) NewHandler(
        WwExecutionOptions? options = null, AccessToken? firstToken = null)
    {
        var credential = new Mock<TokenCredential>();
        credential
            .Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstToken ?? TokenExpiringIn(TimeSpan.FromMinutes(30)));

        var inner = new InnerStubHandler();
        var handler = new WwExecutionTokenHandler(
            credential.Object,
            Microsoft.Extensions.Options.Options.Create(options ?? Options()),
            NullLogger<WwExecutionTokenHandler>.Instance)
        {
            InnerHandler = inner,
        };

        return (handler, inner, credential);
    }

    static HttpMessageInvoker Invoker(WwExecutionTokenHandler handler) => new(handler);

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_FirstCall_AcquiresTokenAndSetsAuthorizationHeader()
    {
        var (handler, _, credential) = NewHandler(firstToken: TokenExpiringIn(TimeSpan.FromMinutes(30), "abc123"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/Hello.json");

        await Invoker(handler).SendAsync(request, CancellationToken.None);

        Assert.AreEqual("Bearer", request.Headers.Authorization?.Scheme);
        Assert.AreEqual("abc123", request.Headers.Authorization?.Parameter);
        credential.Verify(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_CachedTokenNotExpired_ReusesToken_DoesNotCallCredentialAgain()
    {
        var (handler, _, credential) = NewHandler(firstToken: TokenExpiringIn(TimeSpan.FromMinutes(30)));
        var invoker = Invoker(handler);

        using (var r1 = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/A.json"))
            await invoker.SendAsync(r1, CancellationToken.None);
        using (var r2 = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/B.json"))
            await invoker.SendAsync(r2, CancellationToken.None);

        credential.Verify(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Once,
            "A non-expired cached token must be reused across calls.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_TokenExpired_RefetchesToken()
    {
        var credential = new Mock<TokenCredential>();
        credential.SetupSequence(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TokenExpiringIn(TimeSpan.FromSeconds(-1), "expired-token"))
            .ReturnsAsync(TokenExpiringIn(TimeSpan.FromMinutes(30), "fresh-token"));

        var inner = new InnerStubHandler();
        var handler = new WwExecutionTokenHandler(credential.Object,
            Microsoft.Extensions.Options.Options.Create(Options()), NullLogger<WwExecutionTokenHandler>.Instance)
        { InnerHandler = inner };
        var invoker = Invoker(handler);

        using var r1 = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/A.json");
        await invoker.SendAsync(r1, CancellationToken.None);
        using var r2 = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/B.json");
        await invoker.SendAsync(r2, CancellationToken.None);

        Assert.AreEqual("fresh-token", r2.Headers.Authorization?.Parameter);
        credential.Verify(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_WithinRefreshSkewWindow_TreatedAsExpired_Refetches()
    {
        var credential = new Mock<TokenCredential>();
        // Token is still technically valid (expires in 30s) but the configured skew is 120s,
        // so it must be treated as expired and refetched.
        credential.SetupSequence(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TokenExpiringIn(TimeSpan.FromSeconds(30), "about-to-expire"))
            .ReturnsAsync(TokenExpiringIn(TimeSpan.FromMinutes(30), "refreshed"));

        var inner = new InnerStubHandler();
        var handler = new WwExecutionTokenHandler(credential.Object,
            Microsoft.Extensions.Options.Options.Create(Options(refreshSkewSeconds: 120)),
            NullLogger<WwExecutionTokenHandler>.Instance)
        { InnerHandler = inner };
        var invoker = Invoker(handler);

        using var r1 = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/A.json");
        await invoker.SendAsync(r1, CancellationToken.None);
        using var r2 = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/B.json");
        await invoker.SendAsync(r2, CancellationToken.None);

        Assert.AreEqual("refreshed", r2.Headers.Authorization?.Parameter);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_ServicesRoute_WithFunctionKey_AddsFunctionKeyHeader()
    {
        var (handler, _, _) = NewHandler(Options(functionKey: "fk-123"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/services/Hello.json");

        await Invoker(handler).SendAsync(request, CancellationToken.None);

        Assert.AreEqual("fk-123", request.Headers.GetValues("x-functions-key").Single());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_SecureRoute_DoesNotAddFunctionKeyHeader_EvenIfConfigured()
    {
        var (handler, _, _) = NewHandler(Options(functionKey: "fk-123"));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/Hello.json");

        await Invoker(handler).SendAsync(request, CancellationToken.None);

        Assert.IsFalse(request.Headers.Contains("x-functions-key"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_NoFunctionKeyConfigured_ServicesRoute_NoHeaderAdded()
    {
        var (handler, _, _) = NewHandler(Options(functionKey: null));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/services/Hello.json");

        await Invoker(handler).SendAsync(request, CancellationToken.None);

        Assert.IsFalse(request.Headers.Contains("x-functions-key"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task SendAsync_ConcurrentCalls_SingleFlightsTokenAcquisition()
    {
        var callCount = 0;
        var credential = new Mock<TokenCredential>();
        credential.Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                Interlocked.Increment(ref callCount);
                await Task.Delay(50); // widen the race window
                return TokenExpiringIn(TimeSpan.FromMinutes(30));
            });

        var inner = new InnerStubHandler();
        var handler = new WwExecutionTokenHandler(credential.Object,
            Microsoft.Extensions.Options.Options.Create(Options()), NullLogger<WwExecutionTokenHandler>.Instance)
        { InnerHandler = inner };
        var invoker = Invoker(handler);

        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/A.json");
            await invoker.SendAsync(request, CancellationToken.None);
        });
        await Task.WhenAll(tasks);

        Assert.AreEqual(1, callCount, "Concurrent callers on a cold cache must single-flight the token acquisition.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task GetTokenAsync_CredentialThrows_ExceptionLoggedAndRethrown()
    {
        var credential = new Mock<TokenCredential>();
        credential.Setup(c => c.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("no roles assigned"));

        var inner = new InnerStubHandler();
        var handler = new WwExecutionTokenHandler(credential.Object,
            Microsoft.Extensions.Options.Options.Create(Options()), NullLogger<WwExecutionTokenHandler>.Instance)
        { InnerHandler = inner };
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://engine.test/secure/A.json");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => Invoker(handler).SendAsync(request, CancellationToken.None));
    }

    sealed class InnerStubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }
}
