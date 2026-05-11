/*
 * Gap tests for AuditLogger.
 *
 * The full-sweep run showed only 4 / 18 lines covered — the constructor and one
 * Log* method. Every other emitter (cold start, key-vault error, decryption,
 * auth outcome) was dormant because the security spec suite never reaches the
 * code paths that call them under coverage instrumentation.
 *
 * These tests exercise each Get_ / Log_ method directly with a captured
 * ILogger<AuditLogger> so the SECURITY_AUDIT line format and severity are
 * pinned. Anything that breaks the on-disk log shape will fail a test, which
 * is the desired behaviour for a forensic logger.
 */

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Security
{
    [TestClass]
    public class AuditLoggerGapTests
    {
        sealed class CapturedLog
        {
            public LogLevel Level    { get; init; }
            public string   Message  { get; init; } = "";
            public Exception? Error  { get; init; }
        }

        sealed class CapturingLogger : ILogger<AuditLogger>
        {
            public List<CapturedLog> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter)
                => Entries.Add(new CapturedLog { Level = logLevel, Message = formatter(state, exception), Error = exception });

            sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }

        static (AuditLogger, CapturingLogger) NewAuditLogger()
        {
            var sink = new CapturingLogger();
            return (new AuditLogger(sink), sink);
        }

        // ── Constructor ───────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Constructor_NullLogger_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AuditLogger(null!));
        }

        // ── Cold start ────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetColdStartLog_FormatContainsInstanceAndKeyId()
        {
            var (audit, _) = NewAuditLogger();
            var line = audit.GetColdStartLog("instance-1", "key-A");

            StringAssert.Contains(line, "SECURITY_AUDIT");
            StringAssert.Contains(line, "Event=ColdStart");
            StringAssert.Contains(line, "InstanceId=instance-1");
            StringAssert.Contains(line, "KeyId=key-A");
            StringAssert.Contains(line, "Utc=");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogColdStart_EmitsInformation_WithExpectedShape()
        {
            var (audit, sink) = NewAuditLogger();
            audit.LogColdStart("i-1", "k-1");

            Assert.AreEqual(1, sink.Entries.Count);
            Assert.AreEqual(LogLevel.Information, sink.Entries[0].Level);
            StringAssert.Contains(sink.Entries[0].Message, "Event=ColdStart");
            StringAssert.Contains(sink.Entries[0].Message, "InstanceId=i-1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogColdStart_StringOverload_EmitsMessageVerbatim()
        {
            var (audit, sink) = NewAuditLogger();
            audit.LogColdStart("plain message");

            Assert.AreEqual(1, sink.Entries.Count);
            Assert.AreEqual(LogLevel.Information, sink.Entries[0].Level);
            Assert.AreEqual("plain message", sink.Entries[0].Message);
        }

        // ── Key Vault error ───────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetKeyVaultErrorLog_FormatContainsInstance()
        {
            var (audit, _) = NewAuditLogger();
            var line = audit.GetKeyVaultErrorLog("inst-7");

            StringAssert.Contains(line, "Event=KeyVaultError");
            StringAssert.Contains(line, "InstanceId=inst-7");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogKeyVaultError_EmitsErrorLevel_WithException()
        {
            var (audit, sink) = NewAuditLogger();
            var ex = new InvalidOperationException("boom");

            audit.LogKeyVaultError("inst-7", ex);

            Assert.AreEqual(1, sink.Entries.Count);
            Assert.AreEqual(LogLevel.Error, sink.Entries[0].Level);
            Assert.AreSame(ex, sink.Entries[0].Error);
            StringAssert.Contains(sink.Entries[0].Message, "Event=KeyVaultError");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogKeyVaultErrorAndMessage_EmitsErrorWithProvidedMessageAndException()
        {
            var (audit, sink) = NewAuditLogger();
            var ex = new Exception("inner");

            audit.LogKeyVaultErrorAndMessage("custom failure detail", ex);

            Assert.AreEqual(1, sink.Entries.Count);
            Assert.AreEqual(LogLevel.Error, sink.Entries[0].Level);
            Assert.AreEqual("custom failure detail", sink.Entries[0].Message);
            Assert.AreSame(ex, sink.Entries[0].Error);
        }

        // ── Decryption ────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetDecryptionLog_FormatContainsEventAndInstance()
        {
            var (audit, _) = NewAuditLogger();
            var line = audit.GetDecryptionLog("inst-d");

            StringAssert.Contains(line, "Event=DecryptionInvoked");
            StringAssert.Contains(line, "InstanceId=inst-d");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogDecryption_EmitsDebugLevel()
        {
            var (audit, sink) = NewAuditLogger();
            audit.LogDecryption("inst-d");

            Assert.AreEqual(1, sink.Entries.Count);
            Assert.AreEqual(LogLevel.Debug, sink.Entries[0].Level,
                "Per-invocation decryption logging must stay at Debug — Info would explode log volume.");
            StringAssert.Contains(sink.Entries[0].Message, "Event=DecryptionInvoked");
        }

        // ── Auth outcome (MWA-05 / OBS-02) ────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogAuthOutcome_EmitsWarningWithEveryField()
        {
            var (audit, sink) = NewAuditLogger();

            audit.LogAuthOutcome(
                outcome:       "403",
                caller:        "alice@contoso",
                workflow:      "Hello",
                path:          "/secure/Hello",
                reason:        "no_matching_policy",
                correlationId: "corr-1");

            Assert.AreEqual(1, sink.Entries.Count);
            Assert.AreEqual(LogLevel.Warning, sink.Entries[0].Level,
                "Auth outcomes must be Warning so Application Insights default retention keeps them.");
            var msg = sink.Entries[0].Message;
            StringAssert.Contains(msg, "Event=AuthOutcome");
            StringAssert.Contains(msg, "Outcome=403");
            StringAssert.Contains(msg, "Caller=alice@contoso");
            StringAssert.Contains(msg, "Workflow=Hello");
            StringAssert.Contains(msg, "Path=/secure/Hello");
            StringAssert.Contains(msg, "Reason=no_matching_policy");
            StringAssert.Contains(msg, "CorrelationId=corr-1");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogAuthOutcome_NeverIncludesTokenOrSecret()
        {
            // Invariant from the AuditLogger XML doc: caller/reason must never carry
            // token material. Test that the message doesn't accidentally include
            // anything resembling a JWT or "Bearer ".
            var (audit, sink) = NewAuditLogger();

            audit.LogAuthOutcome("401", "(anonymous)", "", "/secure/x",
                                 "no_authenticated_principal", "corr-2");

            var msg = sink.Entries.Single().Message;
            StringAssert.Contains(msg, "(anonymous)");
            Assert.IsFalse(msg.Contains("Bearer ", StringComparison.OrdinalIgnoreCase),
                "Audit lines must not carry the literal Bearer prefix.");
            Assert.IsFalse(msg.Contains("eyJ"),
                "Audit lines must not carry a JWT header (eyJ…).");
        }
    }
}
