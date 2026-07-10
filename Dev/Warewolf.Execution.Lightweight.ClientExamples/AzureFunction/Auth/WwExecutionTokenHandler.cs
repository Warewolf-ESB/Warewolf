using System.Net.Http.Headers;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace WwExecutionCaller.Auth;

/// <summary>
/// A <see cref="DelegatingHandler"/> that transparently acquires, caches, and refreshes an
/// app-only access token for the Warewolf Execution Engine and injects it as a
/// <c>Authorization: Bearer</c> header on every outgoing request. It also injects the
/// <c>x-functions-key</c> header for the engine's <c>/services/*</c> routes.
///
/// Token acquisition strategy:
///   1. Primary  — <see cref="TokenCredential"/> (DefaultAzureCredential): Managed Identity
///                 in Azure, Azure CLI locally. No secrets at rest.
///   2. Fallback — MSAL <see cref="IConfidentialClientApplication"/> client-credentials,
///                 enabled only when ClientId + ClientSecret are configured (local dev).
///
/// The handler caches the most recent token and only re-acquires when it is missing or within
/// the <see cref="ExpiryBuffer"/> of expiry. A <see cref="SemaphoreSlim"/> collapses concurrent
/// refreshes so only one token request is in flight at a time.
/// </summary>
public sealed class WwExecutionTokenHandler : DelegatingHandler
{
    /// <summary>Refresh the token this long before it actually expires, to avoid edge-of-expiry 401s.</summary>
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromMinutes(5);

    private readonly TokenCredential _credential;
    private readonly WwExecutionCallerOptions _options;
    private readonly ILogger<WwExecutionTokenHandler> _logger;
    private readonly string[] _scopes;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly Lazy<IConfidentialClientApplication?> _msalFallback;

    private AccessToken _cachedToken;

    public WwExecutionTokenHandler(
        TokenCredential credential,
        IOptions<WwExecutionCallerOptions> options,
        ILogger<WwExecutionTokenHandler> logger)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopes = [_options.EffectiveScope];
        _msalFallback = new Lazy<IConfidentialClientApplication?>(BuildMsalFallback);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await GetTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // The engine's /services/* routes additionally require a host function key.
        if (!string.IsNullOrWhiteSpace(_options.FunctionKey)
            && request.RequestUri is { } uri
            && uri.AbsolutePath.Contains("/services/", StringComparison.OrdinalIgnoreCase)
            && !request.Headers.Contains("x-functions-key"))
        {
            request.Headers.Add("x-functions-key", _options.FunctionKey);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Returns a valid cached token, acquiring/refreshing one if necessary.</summary>
    private async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        if (IsTokenValid(_cachedToken))
        {
            return _cachedToken.Token;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check: another caller may have refreshed while we waited on the lock.
            if (IsTokenValid(_cachedToken))
            {
                return _cachedToken.Token;
            }

            _cachedToken = await AcquireAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation(
                "Acquired Warewolf Execution Engine token (expires {Expiry:u}).",
                _cachedToken.ExpiresOn);
            return _cachedToken.Token;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static bool IsTokenValid(AccessToken token) =>
        !string.IsNullOrEmpty(token.Token) && token.ExpiresOn - ExpiryBuffer > DateTimeOffset.UtcNow;

    /// <summary>Acquires a fresh token: DefaultAzureCredential first, MSAL client-credentials as fallback.</summary>
    private async Task<AccessToken> AcquireAsync(CancellationToken cancellationToken)
    {
        try
        {
            var context = new TokenRequestContext(_scopes, tenantId: _options.TenantId);
            return await _credential.GetTokenAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (_msalFallback.Value is not null)
        {
            _logger.LogWarning(ex,
                "DefaultAzureCredential failed; falling back to MSAL client-credentials.");
            return await AcquireViaMsalAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<AccessToken> AcquireViaMsalAsync(CancellationToken cancellationToken)
    {
        var app = _msalFallback.Value!;
        var result = await app
            .AcquireTokenForClient(_scopes)
            .ExecuteAsync(cancellationToken)
            .ConfigureAwait(false);
        return new AccessToken(result.AccessToken, result.ExpiresOn);
    }

    private IConfidentialClientApplication? BuildMsalFallback()
    {
        if (!_options.HasClientSecretFallback)
        {
            return null;
        }

        _logger.LogInformation(
            "MSAL client-credentials fallback is configured for client {ClientId}.",
            _options.ClientId);

        return ConfidentialClientApplicationBuilder
            .Create(_options.ClientId)
            .WithClientSecret(_options.ClientSecret)
            .WithAuthority(_options.Authority)
            .Build();
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
