using System.ComponentModel.DataAnnotations;

namespace WwExecutionClient;

/// <summary>
/// Strongly-typed options bound from the <c>WwExecution</c> section of <c>appsettings.json</c>
/// (and any overriding configuration source - environment variables, user-secrets, CLI args).
/// </summary>
public sealed class WwExecutionClientOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "WwExecution";

    /// <summary>Base URL of the Warewolf Execution Engine, e.g. <c>https://WWExecutionEngine.azurewebsites.net</c>.</summary>
    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Entra ID tenant (directory) ID. Forms the authority <c>https://login.microsoftonline.com/{TenantId}</c>.</summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Application (client) ID of the ENGINE's app registration. Tokens are requested for the audience
    /// <c>api://{ResourceAppId}</c>; the engine validates that its tokens carry this audience.
    /// </summary>
    [Required]
    public string ResourceAppId { get; set; } = string.Empty;

    /// <summary>Application (client) ID of THIS client's app registration.</summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Confidential-client secret. Required only for the client-credentials (daemon) flow.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Redirect URI used by the interactive (browser) flow. <c>http://localhost</c> picks a free port.</summary>
    public string RedirectUri { get; set; } = "http://localhost";

    /// <summary>Azure Functions key, sent as <c>x-functions-key</c> for <c>/services/*</c> routes. Optional otherwise.</summary>
    public string? FunctionKey { get; set; }

    /// <summary>Optional user-assigned managed-identity client ID. Empty ⇒ system-assigned / default credential.</summary>
    public string? ManagedIdentityClientId { get; set; }

    /// <summary>File name of the persistent MSAL token cache.</summary>
    public string CacheFileName { get; set; } = "ww_execution_msal_cache.bin";

    /// <summary>Workflow name exercised by the demo (unencoded; the client URI-encodes it).</summary>
    public string SampleWorkflow { get; set; } = "Hello World";

    // ----- Derived values (not bound directly) -----

    /// <summary>Entra authority URL: <c>https://login.microsoftonline.com/{TenantId}</c>.</summary>
    public string Authority => $"https://login.microsoftonline.com/{TenantId}";

    /// <summary>Audience / App ID URI of the engine resource: <c>api://{ResourceAppId}</c>.</summary>
    public string ResourceAppIdUri => $"api://{ResourceAppId}";

    /// <summary>Delegated (user) scope: <c>api://{ResourceAppId}/user_impersonation</c>.</summary>
    public string DelegatedScope => $"{ResourceAppIdUri}/user_impersonation";

    /// <summary>App-only (daemon) scope: <c>api://{ResourceAppId}/.default</c>.</summary>
    public string AppOnlyScope => $"{ResourceAppIdUri}/.default";

    /// <summary>Scopes array for delegated/user flows.</summary>
    public string[] DelegatedScopes => new[] { DelegatedScope };

    /// <summary>Scopes array for app-only flows.</summary>
    public string[] AppOnlyScopes => new[] { AppOnlyScope };

    /// <summary>Whether a confidential-client secret has been supplied.</summary>
    public bool HasClientSecret => !string.IsNullOrWhiteSpace(ClientSecret);

    /// <summary>Whether a function key has been supplied (needed for <c>/services/*</c>).</summary>
    public bool HasFunctionKey => !string.IsNullOrWhiteSpace(FunctionKey);
}
