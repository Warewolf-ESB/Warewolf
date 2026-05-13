/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

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
        var secretManager = host.Services.GetRequiredService<KeyVaultSecretManager>();
        var audit         = host.Services.GetRequiredService<AuditLogger>();
        //var logger = host.Services.GetRequiredService<IExecutionLogger>();

        try
        {
            await secretManager.InitializeAsync().ConfigureAwait(false);

            var decryptionHelper = host.Services.GetRequiredService<FileDecryptionHelper>();
            DpapiWrapper.AesDecryptHook = decryptionHelper.DecryptConnectionString;

            // Resolve IExecutionLogger AFTER the AES hook is wired, because
            // the singleton factory reads encrypted .bite files that require
            // DpapiWrapper.AesDecryptHook to be set.
            var logger = host.Services.GetRequiredService<IExecutionLogger>();
            var log = audit.GetColdStartLog(config.InstanceId, secretManager.KeyId);
            logger.LogInfo(log);
            audit.LogColdStart(config.InstanceId, secretManager.KeyId);
        }
        catch (Exception ex)
        {
            var log = audit.GetKeyVaultErrorLog(config.InstanceId);
            // Use AuditLogger only here — IExecutionLogger may not be safe to
            // resolve if the AES hook failed to initialise.
            audit.LogKeyVaultErrorAndMessage(log, ex);
            throw; // Fail fast: cannot serve requests without the AES key.
        }
    }
}
