using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Warewolf.Execution.ServiceBusWorker.Auth;

/// <summary>
/// A <see cref="DelegatingHandler"/> that transparently authenticates every outbound request
/// to the Warewolf Execution Engine.
///
/// On each request it:
///   1. Acquires an app-only (client-credentials) access token for the configured scope
///      (<c>api://{ResourceAppId}/.default</c>) using the injected <see cref="TokenCredential"/>
///      — Managed Identity in Azure, or the client-secret/az-CLI fallback locally.
///   2. Caches that token in-memory and reuses it until just before expiry
///      (refresh skew configurable via <see cref="WwExecutionOptions.TokenRefreshSkewSeconds"/>).
///   3. Injects <c>Authorization: Bearer &lt;token&gt;</c>, and — when configured — the
///      <c>x-functions-key</c> header required by <c>/services/*</c> routes.
///
/// This is the centrepiece of the sample: callers (the typed <c>WwExecutionClient</c>) never
/// touch tokens — they just make HTTP calls and this handler does the auth.
/// </summary>
public sealed class WwExecutionTokenHandler : DelegatingHandler
{
    private readonly TokenCredential _credential;
    private readonly WwExecutionOptions _options;
    private readonly ILogger<WwExecutionTokenHandler> _logger;

    // Single-flight guard so concurrent requests don't all hit Entra at once on a cold cache.
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly string[] _scopes;

    private AccessToken _cachedToken;

    public WwExecutionTokenHandler(
        TokenCredential credential,
        IOptions<WwExecutionOptions> options,
        ILogger<WwExecutionTokenHandler> logger)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopes = new[] { _options.EffectiveScope };
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Only the engine's /services/* routes require a function key; scope the header to those
        // paths so it is never leaked on /public or /secure calls.
        if (!string.IsNullOrWhiteSpace(_options.FunctionKey)
            && request.RequestUri is { } uri
            && uri.AbsolutePath.Contains("/services/", StringComparison.OrdinalIgnoreCase)
            && !request.Headers.Contains("x-functions-key"))
        {
            request.Headers.Add("x-functions-key", _options.FunctionKey);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (!IsExpired(_cachedToken))
        {
            return _cachedToken.Token;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Re-check after acquiring the lock — another caller may have refreshed it.
            if (!IsExpired(_cachedToken))
            {
                return _cachedToken.Token;
            }

            _logger.LogInformation(
                "Acquiring app-only token for scope {Scope} (audience api://{ResourceAppId}).",
                _options.EffectiveScope, _options.ResourceAppId);

            var context = new TokenRequestContext(_scopes, tenantId: _options.TenantId);
            _cachedToken = await _credential
                .GetTokenAsync(context, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation(
                "Acquired token; expires at {ExpiresOn:u} (refresh skew {Skew}s).",
                _cachedToken.ExpiresOn, _options.TokenRefreshSkewSeconds);

            return _cachedToken.Token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to acquire app-only token for the Warewolf Execution Engine. " +
                "Verify the caller's identity is assigned an app role on the engine's resource " +
                "service principal (a roleless caller is rejected) and that the tenant/scope are correct.");
            throw;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    /// <summary>
    /// A token is "expired" when it is unset or within the refresh-skew window of its expiry.
    /// </summary>
    private bool IsExpired(AccessToken token)
    {
        if (string.IsNullOrEmpty(token.Token))
        {
            return true;
        }

        var skew = TimeSpan.FromSeconds(Math.Max(0, _options.TokenRefreshSkewSeconds));
        return DateTimeOffset.UtcNow >= token.ExpiresOn - skew;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshLock.Dispose();
        }

        base.Dispose(disposing);
    }
}
