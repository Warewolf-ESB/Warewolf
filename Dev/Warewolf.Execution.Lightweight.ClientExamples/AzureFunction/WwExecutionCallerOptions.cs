using System.ComponentModel.DataAnnotations;

namespace WwExecutionCaller;

/// <summary>
/// Strongly-typed configuration for the downstream Warewolf Execution Engine client.
/// Bound from the <c>WwExecution</c> configuration section (see <c>local.settings.json</c>
/// locally, or App Settings / Key Vault references in Azure).
/// </summary>
public sealed class WwExecutionCallerOptions
{
    /// <summary>Configuration section name these options bind from.</summary>
    public const string SectionName = "WwExecution";

    /// <summary>
    /// Base URL of the Warewolf Execution Engine function app,
    /// e.g. <c>https://WWExecutionEngine.azurewebsites.net</c>. No trailing slash required.
    /// </summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://WWExecutionEngine.azurewebsites.net";

    /// <summary>Entra ID tenant (directory) GUID that issues tokens.</summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// The engine's resource (server) app registration Client ID. Used to derive the
    /// audience (<c>api://{ResourceAppId}</c>) and the default app-only scope.
    /// </summary>
    [Required]
    public string ResourceAppId { get; set; } = string.Empty;

    /// <summary>
    /// App-only scope requested for the downstream call. Defaults to
    /// <c>api://{ResourceAppId}/.default</c> when left blank.
    /// </summary>
    public string? Scope { get; set; }

    /// <summary>
    /// Daemon/client app registration Client ID used by the MSAL client-credentials
    /// <em>fallback</em>. Leave blank in Azure — Managed Identity is used instead.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// Client secret paired with <see cref="ClientId"/> for the MSAL fallback.
    /// NEVER commit a real secret. Local dev only — prefer <c>az login</c> + DefaultAzureCredential.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Host-level function key required by the engine's <c>/services/*</c> routes
    /// (sent as the <c>x-functions-key</c> header). Optional — only needed when calling /services.
    /// </summary>
    public string? FunctionKey { get; set; }

    /// <summary>
    /// Workflow name invoked by the timer trigger (URL-decoded, e.g. <c>Hello World</c>).
    /// </summary>
    public string ScheduledWorkflow { get; set; } = "Hello World";

    /// <summary>
    /// Effective scope: explicit <see cref="Scope"/> if set, otherwise
    /// <c>api://{ResourceAppId}/.default</c>.
    /// </summary>
    public string EffectiveScope =>
        string.IsNullOrWhiteSpace(Scope) ? $"api://{ResourceAppId}/.default" : Scope!;

    /// <summary>Entra ID authority URL for the configured tenant.</summary>
    public string Authority => $"https://login.microsoftonline.com/{TenantId}";

    /// <summary>True when the MSAL client-credentials fallback is fully configured.</summary>
    public bool HasClientSecretFallback =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);
}
