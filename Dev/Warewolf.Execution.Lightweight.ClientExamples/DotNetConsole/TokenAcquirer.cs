using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace WwExecutionClient;

/// <summary>
/// Identifies which Entra ID token-acquisition flow to use. Each value maps to one method on
/// <see cref="TokenAcquirer"/>. The active flow is selected once at startup and reused by the
/// <see cref="Auth.BearerTokenHandler"/> for every outgoing request.
/// </summary>
public enum AuthFlow
{
    /// <summary>App-only (daemon). Confidential client + secret, scope <c>.default</c>. No signed-in user.</summary>
    ClientCredentials,

    /// <summary>Interactive user on a headless / CLI box. Public client, device-code, scope <c>user_impersonation</c>.</summary>
    DeviceCode,

    /// <summary>Interactive browser popup. Public client, scope <c>user_impersonation</c>.</summary>
    Interactive,

    /// <summary>Managed identity / DefaultAzureCredential. Use when running inside Azure.</summary>
    ManagedIdentity
}

/// <summary>
/// Acquires Entra ID access tokens for the Warewolf Execution Engine via several flows, all backed
/// by a single <b>persistent</b> MSAL token cache so tokens survive process restarts. Public-client
/// flows always try the cache first (<see cref="TrySilentAsync"/>) before prompting the user.
/// </summary>
public sealed class TokenAcquirer
{
    private readonly WwExecutionClientOptions _options;
    private readonly ILogger<TokenAcquirer> _logger;

    // One public-client app instance for all delegated/interactive flows (shares the cache).
    private readonly Lazy<Task<IPublicClientApplication>> _publicClient;

    // One confidential-client app instance for the daemon flow (only valid if a secret is configured).
    private readonly Lazy<Task<IConfidentialClientApplication>> _confidentialClient;

    public TokenAcquirer(IOptions<WwExecutionClientOptions> options, ILogger<TokenAcquirer> logger)
    {
        _options = options.Value;
        _logger = logger;

        _publicClient = new Lazy<Task<IPublicClientApplication>>(BuildPublicClientAsync);
        _confidentialClient = new Lazy<Task<IConfidentialClientApplication>>(BuildConfidentialClientAsync);
    }

    /// <summary>
    /// Acquires a token for the requested <paramref name="flow"/>. For public-client flows this tries the
    /// cache silently first and only falls back to the interactive path when no usable token is cached.
    /// </summary>
    public async Task<AuthenticationResult> AcquireAsync(AuthFlow flow, CancellationToken ct = default)
    {
        return flow switch
        {
            AuthFlow.ClientCredentials => await ClientCredentialsAsync(ct).ConfigureAwait(false),
            AuthFlow.DeviceCode        => await TrySilentThenAsync(_options.DelegatedScopes, () => DeviceCodeAsync(ct), ct).ConfigureAwait(false),
            AuthFlow.Interactive       => await TrySilentThenAsync(_options.DelegatedScopes, () => InteractiveAsync(ct), ct).ConfigureAwait(false),
            AuthFlow.ManagedIdentity   => await ManagedIdentityAsync(ct).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(flow), flow, "Unsupported auth flow.")
        };
    }

    // ---------------------------------------------------------------------
    //  Flow 1 - Client credentials (app-only / daemon)
    // ---------------------------------------------------------------------

    /// <summary>
    /// App-only flow for unattended daemons/services. Uses a confidential client + client secret and the
    /// <c>.default</c> scope. The caller is the application itself (no signed-in user); the engine must
    /// grant the client an <b>app role</b> on its resource service principal.
    /// </summary>
    public async Task<AuthenticationResult> ClientCredentialsAsync(CancellationToken ct = default)
    {
        if (!_options.HasClientSecret)
        {
            throw new InvalidOperationException(
                "ClientCredentials flow requires WwExecution:ClientSecret to be set.");
        }

        var app = await _confidentialClient.Value.ConfigureAwait(false);

        // MSAL caches app-only tokens internally; AcquireTokenForClient returns a cached token if still valid.
        var result = await app
            .AcquireTokenForClient(_options.AppOnlyScopes)
            .ExecuteAsync(ct)
            .ConfigureAwait(false);

        _logger.LogInformation("Acquired app-only token (client credentials). Expires {Expiry:u}.",
            result.ExpiresOn);
        return result;
    }

    // ---------------------------------------------------------------------
    //  Flow 2 - Device code (headless / CLI interactive)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Device-code flow for an interactive user on a box without a browser (CLI / SSH / container). MSAL
    /// prints a URL and a code; the user authenticates on another device. Scope <c>user_impersonation</c>.
    /// </summary>
    public async Task<AuthenticationResult> DeviceCodeAsync(CancellationToken ct = default)
    {
        var app = await _publicClient.Value.ConfigureAwait(false);

        var result = await app
            .AcquireTokenWithDeviceCode(_options.DelegatedScopes, deviceCodeCallback =>
            {
                // This message MUST be surfaced to the user verbatim.
                Console.WriteLine();
                Console.WriteLine(deviceCodeCallback.Message);
                Console.WriteLine();
                return Task.CompletedTask;
            })
            .ExecuteAsync(ct)
            .ConfigureAwait(false);

        _logger.LogInformation("Acquired delegated token (device code) for {User}. Expires {Expiry:u}.",
            result.Account?.Username, result.ExpiresOn);
        return result;
    }

    // ---------------------------------------------------------------------
    //  Flow 3 - Interactive (browser popup)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Interactive flow that opens the system browser for sign-in. Public client, scope
    /// <c>user_impersonation</c>. Requires a registered public-client redirect URI.
    /// </summary>
    public async Task<AuthenticationResult> InteractiveAsync(CancellationToken ct = default)
    {
        var app = await _publicClient.Value.ConfigureAwait(false);

        var result = await app
            .AcquireTokenInteractive(_options.DelegatedScopes)
            .WithPrompt(Prompt.SelectAccount)
            .ExecuteAsync(ct)
            .ConfigureAwait(false);

        _logger.LogInformation("Acquired delegated token (interactive) for {User}. Expires {Expiry:u}.",
            result.Account?.Username, result.ExpiresOn);
        return result;
    }

    // ---------------------------------------------------------------------
    //  Flow 4 - Managed identity / DefaultAzureCredential
    // ---------------------------------------------------------------------

    /// <summary>
    /// App-only flow for code running inside Azure (App Service, Functions, VM, Container Apps...). Uses
    /// the platform-provided managed identity via <see cref="DefaultAzureCredential"/>, requesting the
    /// engine's <c>.default</c> scope. No secret or cache needed. The identity needs an app-role assignment.
    /// </summary>
    /// <remarks>
    /// Returns the raw token wrapped in an <see cref="AuthenticationResult"/> so callers share a single type
    /// across flows. Locally, <see cref="DefaultAzureCredential"/> falls back to Azure CLI / VS / env vars.
    /// </remarks>
    public async Task<AuthenticationResult> ManagedIdentityAsync(CancellationToken ct = default)
    {
        TokenCredential credential = string.IsNullOrWhiteSpace(_options.ManagedIdentityClientId)
            ? new DefaultAzureCredential()
            : new ManagedIdentityCredential(_options.ManagedIdentityClientId);

        var context = new TokenRequestContext(new[] { _options.AppOnlyScope });
        AccessToken token = await credential.GetTokenAsync(context, ct).ConfigureAwait(false);

        _logger.LogInformation("Acquired token (managed identity / DefaultAzureCredential). Expires {Expiry:u}.",
            token.ExpiresOn);

        // Adapt Azure.Core's AccessToken to MSAL's AuthenticationResult for a uniform return type.
        return new AuthenticationResult(
            accessToken: token.Token,
            isExtendedLifeTimeToken: false,
            uniqueId: null,
            expiresOn: token.ExpiresOn,
            extendedExpiresOn: token.ExpiresOn,
            tenantId: _options.TenantId,
            account: null,
            idToken: null,
            scopes: _options.AppOnlyScopes,
            correlationId: Guid.NewGuid());
    }

    // ---------------------------------------------------------------------
    //  Flow 5 - Silent (cache-first)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Attempts to satisfy a delegated request entirely from the persistent MSAL cache - no prompt. MSAL
    /// silently refreshes via the cached refresh token if the access token is expired. Returns <c>null</c>
    /// when nothing usable is cached (caller should fall back to an interactive flow).
    /// </summary>
    public async Task<AuthenticationResult?> TrySilentAsync(IEnumerable<string>? scopes = null, CancellationToken ct = default)
    {
        var app = await _publicClient.Value.ConfigureAwait(false);
        var accounts = await app.GetAccountsAsync().ConfigureAwait(false);
        var account = accounts.FirstOrDefault();
        if (account is null)
        {
            _logger.LogDebug("No cached account available for silent token acquisition.");
            return null;
        }

        try
        {
            var result = await app
                .AcquireTokenSilent(scopes ?? _options.DelegatedScopes, account)
                .ExecuteAsync(ct)
                .ConfigureAwait(false);

            _logger.LogInformation("Acquired delegated token silently from cache for {User}. Expires {Expiry:u}.",
                result.Account?.Username, result.ExpiresOn);
            return result;
        }
        catch (MsalUiRequiredException ex)
        {
            // Expired/revoked refresh token, no cached token, or interaction required (e.g. MFA/consent).
            _logger.LogDebug(ex, "Silent acquisition requires interaction ({ErrorCode}).", ex.ErrorCode);
            return null;
        }
    }

    /// <summary>Silent-first wrapper: try the cache, otherwise run the supplied interactive factory.</summary>
    private async Task<AuthenticationResult> TrySilentThenAsync(
        string[] scopes,
        Func<Task<AuthenticationResult>> interactiveFactory,
        CancellationToken ct)
    {
        var silent = await TrySilentAsync(scopes, ct).ConfigureAwait(false);
        return silent ?? await interactiveFactory().ConfigureAwait(false);
    }

    // ---------------------------------------------------------------------
    //  Client construction + persistent cache wiring
    // ---------------------------------------------------------------------

    private async Task<IPublicClientApplication> BuildPublicClientAsync()
    {
        var app = PublicClientApplicationBuilder
            .Create(_options.ClientId)
            .WithAuthority(_options.Authority)
            .WithRedirectUri(_options.RedirectUri)
            .Build();

        await AttachPersistentCacheAsync(app.UserTokenCache).ConfigureAwait(false);
        return app;
    }

    private async Task<IConfidentialClientApplication> BuildConfidentialClientAsync()
    {
        if (!_options.HasClientSecret)
        {
            throw new InvalidOperationException(
                "Cannot build a confidential client: WwExecution:ClientSecret is not set.");
        }

        var app = ConfidentialClientApplicationBuilder
            .Create(_options.ClientId)
            .WithAuthority(_options.Authority)
            .WithClientSecret(_options.ClientSecret)
            .Build();

        // App-only tokens are cached in-process by MSAL; persisting the app cache is optional but cheap.
        await AttachPersistentCacheAsync(app.AppTokenCache).ConfigureAwait(false);
        return app;
    }

    /// <summary>
    /// Wires a cross-platform, encrypted (where the OS supports it) persistent token cache onto the supplied
    /// MSAL token cache so tokens survive process restarts. Cache file lives under the user's app-data dir.
    /// </summary>
    private async Task AttachPersistentCacheAsync(ITokenCache tokenCache)
    {
        var cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WwExecutionClient");
        Directory.CreateDirectory(cacheDir);

        var storageProperties = new StorageCreationPropertiesBuilder(_options.CacheFileName, cacheDir)
            // Linux fallback (keyring may be unavailable on headless boxes / containers).
            .WithLinuxKeyring(
                schemaName: "com.warewolf.execution.client.tokencache",
                collection: MsalCacheHelper.LinuxKeyRingDefaultCollection,
                secretLabel: "Warewolf Execution Client MSAL cache",
                attribute1: new KeyValuePair<string, string>("Version", "1"),
                attribute2: new KeyValuePair<string, string>("Product", "WwExecutionClient"))
            // macOS keychain.
            .WithMacKeyChain(
                serviceName: "com.warewolf.execution.client",
                accountName: "MSALCache")
            .Build();

        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties).ConfigureAwait(false);
        cacheHelper.RegisterCache(tokenCache);

        _logger.LogDebug("Persistent MSAL token cache registered at {Path}.",
            Path.Combine(cacheDir, _options.CacheFileName));
    }
}
