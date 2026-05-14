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

        // ── Authorization audit (MWA-05 / OBS-02) ─────────────────────────────

        /// <summary>
        /// (MWA-05 / OBS-02) Emits a structured audit event for an authentication
        /// or authorization outcome.  The event is logged at Warning so it is
        /// included in default Application Insights retention.
        ///
        /// Cost-aware: a single structured log statement, no extra HTTP calls.
        /// </summary>
        /// <param name="outcome">"401" or "403".</param>
        /// <param name="caller">Caller identity (UPN or "app:{oid}"); never include tokens.</param>
        /// <param name="workflow">Workflow name in scope, or empty.</param>
        /// <param name="path">Request path.</param>
        /// <param name="reason">Denial reason; never include secret material.</param>
        /// <param name="correlationId">Per-request correlation id.</param>
        public void LogAuthOutcome(
            string outcome,
            string caller,
            string workflow,
            string path,
            string reason,
            string correlationId)
        {
            // Structured log so KQL queries can filter on outcome / workflow / caller.
            _logger.LogWarning(
                "SECURITY_AUDIT | Event=AuthOutcome | Outcome={Outcome} | Caller={Caller} | " +
                "Workflow={Workflow} | Path={Path} | Reason={Reason} | CorrelationId={CorrelationId} | Utc={Utc}",
                outcome, caller, workflow, path, reason, correlationId, DateTimeOffset.UtcNow);
        }
    }
}
