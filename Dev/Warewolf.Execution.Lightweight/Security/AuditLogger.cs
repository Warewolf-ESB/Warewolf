/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Extensions.Logging;

namespace Warewolf.Execution.Lightweight.Security
{
    /// <summary>
    /// Lightweight structured audit logger for security-relevant events.
    ///
    /// Routes all entries through <see cref="Dev2Logger"/> so they reach the
    /// <see cref="Logging.AuditExecutionLogger"/> sink (always present in
    /// <see cref="Logging.CompositeExecutionLogger"/>).
    ///
    /// Invariants:
    ///   ❌ Never logs key material (raw bytes or base64).
    ///   ❌ Never logs decrypted connection-string values.
    ///   ✅ Logs only metadata: timestamps, instance IDs, and key IDs.
    /// </summary>
    public sealed class AuditLogger
    {
        const string AuditExecutionId = "AuditLogger";

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
            => _logger.LogInformation(GetColdStartLog(instanceId, keyId), AuditExecutionId);

        /// <summary>
        /// Logged once per cold start after the AES key is successfully loaded
        /// from Key Vault.
        /// </summary>
        public void LogColdStart(string message)
            => _logger.LogInformation(message, AuditExecutionId);


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
            _logger.LogDebug(GetDecryptionLog(instanceId), AuditExecutionId);
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
            _logger.LogWarning(
                $"SECURITY_AUDIT | Event=AuthOutcome | Outcome={outcome} | Caller={caller} | " +
                $"Workflow={workflow} | Path={path} | Reason={reason} | CorrelationId={correlationId} | Utc={DateTimeOffset.UtcNow}",
                AuditExecutionId);
        }

        /// <summary>
        /// (Spec-Secure-ServiceBus-Triggered-Execution.md §8) Emits a structured audit
        /// event for a secure Service Bus workflow-trigger message outcome — the
        /// message-level counterpart of <see cref="LogAuthOutcome"/> for the HTTP paths.
        /// Logged at Warning so it is included in default Application Insights retention.
        /// </summary>
        /// <param name="status">Terminal status, e.g. "Succeeded", "Denied", "InvalidToken", "Malformed".</param>
        /// <param name="caller">Caller identity resolved from the validated token (UPN or "app:{oid}"); never the raw token.</param>
        /// <param name="workflow">Workflow name in scope, or empty when the message was malformed before it could be read.</param>
        /// <param name="reason">Denial/failure reason; never include token or secret material.</param>
        /// <param name="correlationId">The message's correlation id.</param>
        public void LogServiceBusOutcome(
            string status,
            string caller,
            string workflow,
            string reason,
            string correlationId)
        {
            _logger.LogWarning(
                $"SECURITY_AUDIT | Event=ServiceBusTriggerOutcome | Status={status} | Caller={caller} | " +
                $"Workflow={workflow} | Reason={reason} | CorrelationId={correlationId} | Utc={DateTimeOffset.UtcNow}",
                AuditExecutionId);
        }
    }
}
