/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Extension methods for <see cref="IHost"/> that handle the async Key Vault
/// initialisation that must complete before the host accepts traffic.
/// </summary>
internal static class KeyVaultStartupExtensions
{
    /// <summary>
    /// Fetches the AES key from Key Vault, wires the decryption hook into
    /// <see cref="Warewolf.Security.Encryption.DpapiWrapper"/>, and writes an audit
    /// entry.  Throws on failure so the host refuses to start without the key.
    /// </summary>
    /// <param name="host">The built <see cref="IHost"/> instance.</param>
    /// <param name="config">
    /// Environment configuration snapshot — supplies <see cref="HostEnvironmentConfig.InstanceId"/>
    /// for audit entries without requiring the caller to resolve it.
    /// </param>
    internal static async Task InitializeKeyVaultAsync(this IHost host, HostEnvironmentConfig config)
    {
        const string executionId = "KeyVaultStartupExtensions";

        Dev2Logger.Info($"KeyVaultStartupExtensions InitializeKeyVaultAsync starting for instance: {config.InstanceId}", executionId);

        var secretManager = host.Services.GetRequiredService<KeyVaultSecretManager>();
        var audit         = host.Services.GetRequiredService<AuditLogger>();

        try
        {
            Dev2Logger.Debug("KeyVaultStartupExtensions calling secretManager.InitializeAsync()", executionId);
            await secretManager.InitializeAsync().ConfigureAwait(false);

            var decryptionHelper = host.Services.GetRequiredService<FileDecryptionHelper>();
            DpapiWrapper.AesDecryptHook = decryptionHelper.DecryptConnectionString;

            // Encryption counterpart: every DpapiWrapper.Encrypt call in this host (e.g.
            // SuspendExecutionActivity persisting a workflow environment) produces Key
            // Vault-backed WFAES:: ciphertext instead of machine-bound Windows DPAPI.
            var encryptionHelper = host.Services.GetRequiredService<FileEncryptionHelper>();
            DpapiWrapper.AesEncryptHook = encryptionHelper.Encrypt;

            // KeyId is key-material metadata — never logged at Info/Error/Warning.
            Dev2Logger.Info("KeyVaultStartupExtensions AES decryption + encryption hooks wired.", executionId);

            var log = audit.GetColdStartLog(config.InstanceId, secretManager.KeyId);
            Dev2Logger.Info(log, executionId);
            audit.LogColdStart(config.InstanceId, secretManager.KeyId);

            Dev2Logger.Info($"KeyVaultStartupExtensions InitializeKeyVaultAsync completed successfully. InstanceId: {config.InstanceId}", executionId);
        }
        catch (Exception ex)
        {
            // The exception object itself is still not passed (the sinks would persist
            // ex.ToString() with the full stack); the message is logged for troubleshooting.
            Dev2Logger.Error($"KeyVaultStartupExtensions InitializeKeyVaultAsync failed for instance: {config.InstanceId}: {ex.Message}", executionId);

            var log = audit.GetKeyVaultErrorLog(config.InstanceId);
            Dev2Logger.Error(log, executionId);

            audit.LogKeyVaultErrorAndMessage(log, ex);
            throw; // Fail fast: cannot serve requests without the AES key.
        }
    }
}
