using System.ComponentModel.DataAnnotations;

namespace Warewolf.Execution.ClientExamples.AzureServiceBus;

/// <summary>
/// Strongly-typed configuration for calling the Warewolf Execution Engine.
/// Bound from the <c>WwExecution</c> configuration section (see <c>appsettings.json</c> /
/// <c>local.settings.json</c> and the worker's app settings in Azure).
/// </summary>
public sealed class WwExecutionOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "WwExecution";

    /// <summary>
    /// Base URL of the engine, e.g. <c>https://WWExecutionEngine.azurewebsites.net</c>.
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = "https://WWExecutionEngine.azurewebsites.net";

    /// <summary>
    /// Entra (Azure AD) tenant id that owns both the caller and the engine app registration.
    /// Authority becomes <c>https://login.microsoftonline.com/{TenantId}</c>.
    /// </summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Application (client) id of the engine's app registration. The issued token's
    /// audience (<c>aud</c>) is <c>api://{ResourceAppId}</c>.
    /// </summary>
    [Required]
    public string ResourceAppId { get; set; } = string.Empty;

    /// <summary>
    /// App-only scope requested for the client-credentials flow. Defaults to
    /// <c>api://{ResourceAppId}/.default</c> when left blank — see <see cref="EffectiveScope"/>.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Resolved scope. Uses <see cref="Scope"/> when supplied, otherwise derives the
    /// standard <c>.default</c> scope from <see cref="ResourceAppId"/>.
    /// </summary>
    public string EffectiveScope =>
        string.IsNullOrWhiteSpace(Scope) ? $"api://{ResourceAppId}/.default" : Scope!;

    /// <summary>
    /// Function key sent as the <c>x-functions-key</c> header for <c>/services/*</c> routes.
    /// Optional — only required when calling <c>/services</c>.
    /// </summary>
    public string? FunctionKey { get; set; }

    /// <summary>
    /// Local-dev / client-secret fallback. When <see cref="UseClientSecretFallback"/> is
    /// <c>true</c> the worker authenticates with <see cref="ClientId"/> +
    /// <see cref="ClientSecret"/> instead of Managed Identity. In Azure leave this off and
    /// rely on the system/user-assigned Managed Identity via <c>DefaultAzureCredential</c>.
    /// </summary>
    public bool UseClientSecretFallback { get; set; }

    /// <summary>Daemon app (client) id — used only when <see cref="UseClientSecretFallback"/> is set.</summary>
    public string? ClientId { get; set; }

    /// <summary>Daemon client secret — used only when <see cref="UseClientSecretFallback"/> is set. Never commit a real value.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Optional user-assigned Managed Identity client id. Leave blank to use the
    /// system-assigned identity (or whatever <c>DefaultAzureCredential</c> resolves locally).
    /// </summary>
    public string? ManagedIdentityClientId { get; set; }

    /// <summary>
    /// Refresh the cached token this many seconds before it actually expires, to avoid
    /// racing an expiry mid-request.
    /// </summary>
    public int TokenRefreshSkewSeconds { get; set; } = 120;
}
