/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.Logging;
using System;
using System.Management.Automation;

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
        /// Gets Cold Start Log
        /// </summary>
        /// <param name="instanceId">instance id</param>
        /// <param name="keyId">key id</param>
        /// <returns>Cold start log with time stamp, instance id, key id</returns>
        public string GetColdStartLog(string instanceId, string keyId)
            => $"SECURITY_AUDIT | Event=ColdStart | InstanceId={instanceId} | KeyId={keyId} | Utc={DateTimeOffset.UtcNow}";


        /// <summary>
        /// Logged once per cold start after the AES key is successfully loaded
        /// from Key Vault.
        /// </summary>
        public void LogColdStart(string instanceId, string keyId)
            => _logger.LogInformation(GetColdStartLog(instanceId, keyId));

        /// <summary>
        /// Logged once per cold start after the AES key is successfully loaded
        /// from Key Vault.
        /// </summary>
        public void LogColdStart(string message)
            => _logger.LogInformation(message);


        /// <summary>
        /// Generates a formatted error log entry for a Key Vault error event.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the Key Vault instance associated with the error.</param>
        /// <param name="ex">The exception that triggered the error log entry.</param>
        /// <returns>A string containing the formatted error log entry, including the event type, instance identifier, and the
        /// current UTC timestamp.</returns>
        public string GetKeyVaultErrorLog(string instanceId)
            => $"SECURITY_AUDIT | Event=KeyVaultError | InstanceId={instanceId} | Utc={DateTimeOffset.UtcNow}";


        /// <summary>
        /// Logged when Key Vault initialisation fails (thrown after this call).
        /// </summary>
        public void LogKeyVaultError(string instanceId, Exception ex)
            => _logger.LogError(ex, GetKeyVaultErrorLog(instanceId));

        public void LogKeyVaultErrorAndMessage(string message, Exception ex)
            => _logger.LogError(ex, message);


        /// <summary>
        /// Optional: log when the AES hook decrypts a value (per-invocation).
        /// Keep disabled in high-throughput production to avoid log volume.
        /// </summary>
        public void LogDecryption(string instanceId)
        {
            _logger.LogDebug(message: GetDecryptionLog(instanceId));
        }

        /// <summary>
        /// Generates a formatted audit log entry for a decryption event associated with the specified instance
        /// identifier.
        /// </summary>
        /// <param name="instanceId">The unique identifier of the instance for which the decryption event is being logged.</param>
        /// <returns>A string containing the formatted audit log entry, including the event type, instance identifier, and the
        /// current UTC timestamp.</returns>
        public string GetDecryptionLog(string instanceId)
            => $"SECURITY_AUDIT | Event=DecryptionInvoked | InstanceId={instanceId} | Utc={DateTimeOffset.UtcNow}";
    }
}
