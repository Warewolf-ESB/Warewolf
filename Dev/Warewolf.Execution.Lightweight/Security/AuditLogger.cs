/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.Logging;
using System;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Lightweight structured audit logger for security-relevant events.
    ///
    /// Writes structured log entries via <see cref="ILogger"/> so entries flow
    /// to Application Insights (free tier ≤ 5 GB/month) automatically when the
    /// APPINSIGHTS_INSTRUMENTATIONKEY / APPLICATIONINSIGHTS_CONNECTION_STRING
    /// app setting is present.
    ///
    /// Invariants:
    ///   ❌ Never logs key material (raw bytes or base64).
    ///   ❌ Never logs decrypted connection-string values.
    ///   ✅ Logs only metadata: timestamps, instance IDs, and key IDs.
    /// </summary>
    public sealed class AuditLogger
    {
        readonly ILogger<AuditLogger> _logger;

        public AuditLogger(ILogger<AuditLogger> logger)
            => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Logged once per cold start after the AES key is successfully loaded
        /// from Key Vault.
        /// </summary>
        public void LogColdStart(string instanceId, string keyId)
            => _logger.LogInformation(
                "SECURITY_AUDIT | Event=ColdStart | InstanceId={InstanceId} | KeyId={KeyId} | Utc={Utc}",
                instanceId, keyId, DateTimeOffset.UtcNow);

        /// <summary>
        /// Logged when Key Vault initialisation fails (thrown after this call).
        /// </summary>
        public void LogKeyVaultError(string instanceId, Exception ex)
            => _logger.LogError(ex,
                "SECURITY_AUDIT | Event=KeyVaultError | InstanceId={InstanceId} | Utc={Utc}",
                instanceId, DateTimeOffset.UtcNow);

        /// <summary>
        /// Optional: log when the AES hook decrypts a value (per-invocation).
        /// Keep disabled in high-throughput production to avoid log volume.
        /// </summary>
        public void LogDecryption(string instanceId)
            => _logger.LogDebug(
                "SECURITY_AUDIT | Event=DecryptionInvoked | InstanceId={InstanceId} | Utc={Utc}",
                instanceId, DateTimeOffset.UtcNow);
    }
}
