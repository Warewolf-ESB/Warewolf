/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Immutable snapshot of all environment-variable configuration read at process
/// startup.  Centralises every <see cref="Environment.GetEnvironmentVariable"/>
/// call so <c>Program.cs</c> contains zero raw env-var reads and each variable
/// has exactly one canonical definition.
/// </summary>
internal sealed class HostEnvironmentConfig
{
    // ── Defaults ────────────────────────────────────────────────────────────────

    internal const string DefaultSecretName   = "dp-keyring-v1";
    internal const string VaultUriTemplate    = "https://{0}.vault.azure.net/";

    // ── Factory ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads all relevant environment variables once and returns an immutable
    /// configuration snapshot.
    /// </summary>
    internal static HostEnvironmentConfig Load() => new(
        workflowsDirectory: Environment.GetEnvironmentVariable("WorkflowsDirectory")
                            ?? Path.Combine(AppContext.BaseDirectory, "Resources"),
        vaultName:          Environment.GetEnvironmentVariable("AZURE_KEYVAULT_NAME"),
        secretName:         Environment.GetEnvironmentVariable("KEYVAULT_SECRET_NAME")
                            ?? DefaultSecretName,
        instanceId:         Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                            ?? Environment.MachineName);

    // ── Properties ───────────────────────────────────────────────────────────────

    /// <summary>Absolute or relative path to the directory containing workflow files.</summary>
    internal string WorkflowsDirectory { get; }

    /// <summary>Azure Key Vault vault name.  <c>null</c> when encryption is disabled.</summary>
    internal string? VaultName { get; }

    /// <summary>Key Vault secret name that holds the AES-256-GCM key ring.</summary>
    internal string SecretName { get; }

    /// <summary>
    /// Stable per-instance identifier used in audit log entries.
    /// Defaults to <see cref="Environment.MachineName"/> outside Azure.
    /// </summary>
    internal string InstanceId { get; }

    /// <summary>
    /// <c>true</c> when <see cref="VaultName"/> is set and AES-256-GCM decryption
    /// of <c>.bite</c> source files should be activated at startup.
    /// </summary>
    internal bool EncryptionEnabled => !string.IsNullOrWhiteSpace(VaultName);

    /// <summary>
    /// Fully-qualified Key Vault URI (e.g. <c>https://my-vault.vault.azure.net/</c>).
    /// Only valid when <see cref="EncryptionEnabled"/> is <c>true</c>.
    /// </summary>
    internal string VaultUri => string.Format(VaultUriTemplate, VaultName);

    // ── Constructor ──────────────────────────────────────────────────────────────

    HostEnvironmentConfig(
        string  workflowsDirectory,
        string? vaultName,
        string  secretName,
        string  instanceId)
    {
        WorkflowsDirectory = workflowsDirectory;
        VaultName          = vaultName;
        SecretName         = secretName;
        InstanceId         = instanceId;
    }
}
