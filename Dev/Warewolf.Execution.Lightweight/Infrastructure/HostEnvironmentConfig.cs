/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json;
using Dev2.Common;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Immutable snapshot of all environment-variable configuration read at process
/// startup.  Centralises every <see cref="Environment.GetEnvironmentVariable"/>
/// call so <c>Program.cs</c> contains zero raw env-var reads and each variable
/// has exactly one canonical definition.
///
/// WOLF-8516: deploy-time-static values (Key Vault topology, <see cref="WorkflowsDirectory"/>,
/// <see cref="WorkflowPoolMax"/>, most of <see cref="ServiceBusTrigger"/>) are now sourced
/// SOLELY from the deploy-bundled <c>Settings/executionengine.settings.json</c> file, falling
/// back only to the hardcoded default when the file/field is absent — see
/// <see cref="LoadFileSettings"/>. The legacy standalone env vars for these fields
/// (<c>AZURE_KEYVAULT_NAME</c>, <c>KEYVAULT_SECRET_NAME</c>, <c>WorkflowsDirectory</c>,
/// <c>WAREWOLF_WORKFLOW_POOL_MAX</c>, and 5 of the 6 <c>WAREWOLF_SERVICEBUS_TRIGGER_*</c>
/// tunables) are no longer read at all — the deploy script MUST write this file for every
/// deployment; there is no env-var fallback if it is absent.
/// </summary>
public sealed class HostEnvironmentConfig
{
    // ── Defaults ────────────────────────────────────────────────────────────────

    public const string DefaultSecretName   = "dp-keyring-v1";
    public const string VaultUriTemplate    = "https://{0}.vault.azure.net/";
    public const string SettingsFileName    = "executionengine.settings.json";
    const int DefaultWorkflowPoolMax        = 8;

    // ── Factory ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads <c>Settings/executionengine.settings.json</c> (if present) and all relevant
    /// environment variables once and returns an immutable configuration snapshot.
    /// </summary>
    public static HostEnvironmentConfig Load(string? settingsDirectory = null)
    {
        var isDevelopment = IsDevelopmentEnvironment();
        var file = LoadFileSettings(settingsDirectory);
        // WOLF-8516: SkipFailureToRetrieveSecret merged into WAREWOLF_SECURITY_FLAGS.
        var securityFlags = SecurityFlags.FromEnvironment();
        // WOLF-8516: DEBUG_AZURE_KEYVAULT_SECRET / DEBUG_PRINCIPAL_TOKEN merged into
        // WAREWOLF_DEBUG_CONFIG — only parsed when isDevelopment, matching the previous
        // behaviour of never even reading these vars in production.
        var debugConfig = isDevelopment ? DebugConfig.FromEnvironment() : null;

        return new(
            workflowsDirectory:           file?.WorkflowsDirectory
                                          ?? Path.Combine(AppContext.BaseDirectory, "Resources"),
            vaultName:                    file?.KeyVaultName,
            secretName:                   file?.KeyVaultSecretName
                                          ?? DefaultSecretName,
            instanceId:                   Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                                          ?? Environment.MachineName,
            skipFailureToRetrieveSecret:  securityFlags.SkipFailureToRetrieveSecret,
            tenantId:                     Environment.GetEnvironmentVariable("AZURE_TENANT_ID"),
            managedIdentityClientId:      Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
            debugKeyVaultSecret:          NullIfEmpty(debugConfig?.KeyVaultSecret),
            isDevelopment:                isDevelopment,
            debugPrincipalToken:          NullIfEmpty(debugConfig?.PrincipalToken),
            workflowPoolMax:              file?.WorkflowPoolMax
                                          ?? DefaultWorkflowPoolMax,
            serviceBusTrigger:            ServiceBusTriggerFileOverrides.From(file?.ServiceBusTrigger));
    }

    /// <summary>
    /// Reads and deserialises <c>Settings/executionengine.settings.json</c> (JSON with
    /// comments, matching <c>Settings/persistencesettings.json</c>'s style). Missing file =
    /// <c>null</c> (safe default — every field falls back to its hardcoded default, NOT to an
    /// env var); malformed JSON is logged and treated the same as a missing file rather than
    /// failing startup.
    /// </summary>
    static ExecutionEngineFileSettings? LoadFileSettings(string? settingsDirectory)
    {
        const string executionId = "HostEnvironmentConfig-LoadFileSettings";
        try
        {
            settingsDirectory ??= Path.Combine(AppContext.BaseDirectory, "Settings");
            var settingsPath = Path.Combine(settingsDirectory, SettingsFileName);

            if (!File.Exists(settingsPath))
            {
                return null;
            }

            var json = File.ReadAllText(settingsPath);
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<ExecutionEngineFileSettings>(json, options);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Dev2Logger.Warn(
                $"HostEnvironmentConfig failed to read/parse '{SettingsFileName}' — falling back to " +
                $"hardcoded defaults for every value it would have supplied (no env-var fallback — " +
                $"see WOLF-8516). ExceptionType={ex.GetType().Name}",
                executionId);
            return null;
        }
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

    /// <summary>Shape of <c>Settings/executionengine.settings.json</c>.</summary>
    sealed class ExecutionEngineFileSettings
    {
        public string? KeyVaultName { get; set; }
        public string? KeyVaultSecretName { get; set; }
        public string? WorkflowsDirectory { get; set; }
        public int? WorkflowPoolMax { get; set; }
        public ServiceBusTriggerFileSection? ServiceBusTrigger { get; set; }
    }

    internal sealed class ServiceBusTriggerFileSection
    {
        public double? JtiWindowHours { get; set; }
        public double? ExecutionTimeoutSeconds { get; set; }
        public int? MaxConcurrentExecutions { get; set; }
        public double? SlotWaitTimeoutSeconds { get; set; }
        public double? SettlementTimeoutSeconds { get; set; }
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
    /// Source: <c>skipFailureToRetrieveSecret</c> in the <see cref="SecurityFlags"/> JSON app
    /// setting (WOLF-8516 — previously the standalone <c>SkipFailureToRetrieveSecret</c> env var).
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
    /// or when <c>keyVaultSecret</c> in the <see cref="DebugConfig"/> JSON app setting
    /// (WOLF-8516 — previously the standalone <c>DEBUG_AZURE_KEYVAULT_SECRET</c> env var) is
    /// absent/empty.
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
    /// Source: <c>principalToken</c> in the <see cref="DebugConfig"/> JSON app setting
    /// (WOLF-8516 — previously the standalone <c>DEBUG_PRINCIPAL_TOKEN</c> env var,
    /// <c>local.settings.json</c> only).
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

    /// <summary>
    /// Maximum prepared/compiled workflow instances retained per workflow path.
    /// Source: <c>Settings/executionengine.settings.json</c>'s <c>workflowPoolMax</c> field,
    /// defaulting to 8 when the file/field is absent — no env-var fallback (WOLF-8516).
    /// Consumed by <see cref="Execution.WorkflowExecutor"/>.
    /// </summary>
    public int WorkflowPoolMax { get; }

    /// <summary>
    /// File-sourced overrides for <see cref="Auth.Models.ServiceBusTriggerOptions"/>'s
    /// tunables (excluding <c>ClaimStaleAfter</c>, whose own resolution chain stays
    /// env-var-only — see <see cref="ExecutionEngineFileSettings"/>'s doc comment).
    /// </summary>
    public ServiceBusTriggerFileOverrides ServiceBusTrigger { get; }

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
        string? debugPrincipalToken,
        int     workflowPoolMax,
        ServiceBusTriggerFileOverrides serviceBusTrigger)
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
        WorkflowPoolMax             = workflowPoolMax;
        ServiceBusTrigger           = serviceBusTrigger;
    }
}

/// <summary>
/// File-sourced overrides for <see cref="Auth.Models.ServiceBusTriggerOptions.FromEnvironment"/>'s
/// simple tunables — each field is <c>null</c> when the settings file/section is absent or the
/// individual field wasn't set, in which case <c>FromEnvironment</c> falls back straight to its
/// hardcoded default for that field. No env-var fallback (WOLF-8516) — the 5 standalone env
/// vars these fields used to read directly were removed.
/// </summary>
public sealed class ServiceBusTriggerFileOverrides
{
    public static readonly ServiceBusTriggerFileOverrides None = new();

    public double? JtiWindowHours { get; private init; }
    public double? ExecutionTimeoutSeconds { get; private init; }
    public int? MaxConcurrentExecutions { get; private init; }
    public double? SlotWaitTimeoutSeconds { get; private init; }
    public double? SettlementTimeoutSeconds { get; private init; }

    internal static ServiceBusTriggerFileOverrides From(HostEnvironmentConfig.ServiceBusTriggerFileSection? section) =>
        section is null
            ? None
            : new ServiceBusTriggerFileOverrides
            {
                JtiWindowHours = section.JtiWindowHours,
                ExecutionTimeoutSeconds = section.ExecutionTimeoutSeconds,
                MaxConcurrentExecutions = section.MaxConcurrentExecutions,
                SlotWaitTimeoutSeconds = section.SlotWaitTimeoutSeconds,
                SettlementTimeoutSeconds = section.SettlementTimeoutSeconds,
            };
}
