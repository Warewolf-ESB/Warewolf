/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using System;
using System.Management.Automation;

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

        public AuditLogger() { }

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
            => Dev2Logger.Info(GetColdStartLog(instanceId, keyId), AuditExecutionId);

        /// <summary>
        /// Logged once per cold start after the AES key is successfully loaded
        /// from Key Vault.
        /// </summary>
        public void LogColdStart(string message)
            => Dev2Logger.Info(message, AuditExecutionId);


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
            => Dev2Logger.Error(GetKeyVaultErrorLog(instanceId), ex, AuditExecutionId);

        public void LogKeyVaultErrorAndMessage(string message, Exception ex)
            => Dev2Logger.Error(message, ex, AuditExecutionId);


        /// <summary>
        /// Optional: log when the AES hook decrypts a value (per-invocation).
        /// Keep disabled in high-throughput production to avoid log volume.
        /// </summary>
        public void LogDecryption(string instanceId)
        {
            Dev2Logger.Debug(GetDecryptionLog(instanceId), AuditExecutionId);
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
            Dev2Logger.Warn(
                $"SECURITY_AUDIT | Event=AuthOutcome | Outcome={outcome} | Caller={caller} | " +
                $"Workflow={workflow} | Path={path} | Reason={reason} | CorrelationId={correlationId} | Utc={DateTimeOffset.UtcNow}",
                AuditExecutionId);
        }
    }
}
