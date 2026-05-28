/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Immutable snapshot of all environment-variable configuration read at process
/// startup.  Centralises every <see cref="Environment.GetEnvironmentVariable"/>
/// call so <c>Program.cs</c> contains zero raw env-var reads and each variable
/// has exactly one canonical definition.
/// </summary>
public sealed class HostEnvironmentConfig
{
    // ── Defaults ────────────────────────────────────────────────────────────────

    public const string DefaultSecretName   = "dp-keyring-v1";
    public const string VaultUriTemplate    = "https://{0}.vault.azure.net/";

    // ── Factory ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads all relevant environment variables once and returns an immutable
    /// configuration snapshot.
    /// </summary>
    public static HostEnvironmentConfig Load()
    {
        var isDevelopment = IsDevelopmentEnvironment();
        return new(
            workflowsDirectory:           Environment.GetEnvironmentVariable("WorkflowsDirectory")
                                          ?? Path.Combine(AppContext.BaseDirectory, "Resources"),
            vaultName:                    Environment.GetEnvironmentVariable("AZURE_KEYVAULT_NAME"),
            secretName:                   Environment.GetEnvironmentVariable("KEYVAULT_SECRET_NAME")
                                          ?? DefaultSecretName,
            instanceId:                   Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                                          ?? Environment.MachineName,
            skipFailureToRetrieveSecret:  string.Equals(
                                              Environment.GetEnvironmentVariable("SkipFailureToRetrieveSecret"),
                                              "true",
                                              StringComparison.OrdinalIgnoreCase),
            tenantId:                     Environment.GetEnvironmentVariable("AZURE_TENANT_ID"),
            managedIdentityClientId:      Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
            debugKeyVaultSecret:          isDevelopment
                                          ? NullIfEmpty(Environment.GetEnvironmentVariable("DEBUG_AZURE_KEYVAULT_SECRET"))
                                          : null,
            isDevelopment:                isDevelopment,
            debugPrincipalToken:          isDevelopment
                                          ? NullIfEmpty(Environment.GetEnvironmentVariable("DEBUG_PRINCIPAL_TOKEN"))
                                          : null);
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static bool IsDevelopmentEnvironment()
    {
        static bool IsDevValue(string? v) =>
            string.Equals(v, "Development", StringComparison.OrdinalIgnoreCase);

        return IsDevValue(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"))
            || IsDevValue(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
    }

    // ── Properties ───────────────────────────────────────────────────────────────

    /// <summary>Absolute or relative path to the directory containing workflow files.</summary>
    public string WorkflowsDirectory { get; }

    /// <summary>
    /// When <c>true</c> a failure to retrieve the AES key from Key Vault is treated
    /// as a non-fatal degradation: the host starts but all encrypted <c>.bite</c> sources
    /// will fail to decrypt at runtime.
    ///
    /// Set to <c>true</c> only in development or disaster-recovery scenarios where
    /// connectivity to Key Vault is unavailable and unencrypted sources are acceptable.
    ///
    /// <b>Default: <c>false</c></b> — the host refuses to start without the AES key.
    /// </summary>
    public bool SkipFailureToRetrieveSecret { get; }

    /// <summary>
    /// Azure AD tenant ID used in development to pin the credential chain to the correct tenant.
    /// Source: <c>AZURE_TENANT_ID</c> environment variable.
    /// </summary>
    public string? TenantId { get; }

    /// <summary>
    /// Client ID of a User-Assigned Managed Identity.  When <c>null</c> the
    /// System-Assigned identity is used.  Source: <c>AZURE_CLIENT_ID</c> environment variable.
    /// </summary>
    public string? ManagedIdentityClientId { get; }

    /// <summary>
    /// <c>true</c> when the host is running locally or inside a development container.
    /// Drives the credential strategy selected by <see cref="KeyVaultCredentialFactory"/>.
    /// </summary>
    public bool IsDevelopment { get; }

    /// <summary>
    /// When set in development, the Key Vault secret value is taken directly from this
    /// property — no network call to Azure Key Vault is made.  <c>null</c> in production
    /// or when the <c>DEBUG_AZURE_KEYVAULT_SECRET</c> environment variable is absent/empty.
    /// </summary>
    public string? DebugKeyVaultSecret { get; }

    /// <summary>
    /// Base64-encoded <c>X-MS-CLIENT-PRINCIPAL</c> payload for local debugging.
    /// Obtained by calling <c>https://&lt;your-site&gt;/.auth/me</c>, taking the first
    /// element's <c>clientPrincipal</c> JSON object and base64-encoding it.
    ///
    /// When set in development, <see cref="Parsers.DebugPrincipalParser"/> injects
    /// this as the authenticated principal so that the full auth pipeline
    /// (group resolution, permission checks) runs with real Azure roles — without
    /// requiring a live EasyAuth-enabled App Service locally.
    ///
    /// Source: <c>DEBUG_PRINCIPAL_TOKEN</c> environment variable (<c>local.settings.json</c>).
    /// <b>Never set in production.</b>
    /// </summary>
    public string? DebugPrincipalToken { get; }

    /// <summary>
    /// Pre-built credential options snapshot for Key Vault authentication.
    /// Consumed by <see cref="KeyVaultCredentialFactory.Create"/>.
    /// </summary>
    public KeyVaultCredentialOptions CredentialOptions => new()
    {
        IsDevelopment           = IsDevelopment,
        TenantId                = TenantId,
        ManagedIdentityClientId = ManagedIdentityClientId,
    };

    /// <summary>Azure Key Vault vault name.  <c>null</c> when encryption is disabled.</summary>
    public string? VaultName { get; }

    /// <summary>Key Vault secret name that holds the AES-256-GCM key ring.</summary>
    public string SecretName { get; }

    /// <summary>
    /// Stable per-instance identifier used in audit log entries.
    /// Defaults to <see cref="Environment.MachineName"/> outside Azure.
    /// </summary>
    public string InstanceId { get; }

    /// <summary>
    /// <c>true</c> when <see cref="VaultName"/> is set and AES-256-GCM decryption
    /// of <c>.bite</c> source files should be activated at startup.
    /// </summary>
    public bool EncryptionEnabled => !string.IsNullOrWhiteSpace(VaultName);

    /// <summary>
    /// Fully-qualified Key Vault URI (e.g. <c>https://my-vault.vault.azure.net/</c>).
    /// Only valid when <see cref="EncryptionEnabled"/> is <c>true</c>.
    /// </summary>
    public string VaultUri => string.Format(VaultUriTemplate, VaultName);

    // ── Constructor

    HostEnvironmentConfig(
        string  workflowsDirectory,
        string? vaultName,
        string  secretName,
        string  instanceId,
        bool    skipFailureToRetrieveSecret,
        string? tenantId,
        string? managedIdentityClientId,
        string? debugKeyVaultSecret,
        bool    isDevelopment,
        string? debugPrincipalToken)
    {
        WorkflowsDirectory          = workflowsDirectory;
        VaultName                   = vaultName;
        SecretName                  = secretName;
        InstanceId                  = instanceId;
        SkipFailureToRetrieveSecret = skipFailureToRetrieveSecret;
        TenantId                    = tenantId;
        ManagedIdentityClientId     = managedIdentityClientId;
        DebugKeyVaultSecret         = debugKeyVaultSecret;
        IsDevelopment               = isDevelopment;
        DebugPrincipalToken         = debugPrincipalToken;
    }
}
