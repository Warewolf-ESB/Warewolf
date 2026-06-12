/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Logging;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Tests.Logging
{
    /// <summary>
    /// Coverage for <see cref="AzureExecutionLogger"/> — the Application Insights sink at
    /// the centre of the 8436 fix. Two behaviours are verified:
    /// <list type="number">
    ///   <item><b>Level gating</b> — <see cref="ExecutionLoggerBase.ShouldLog"/> suppresses
    ///   entries below the configured minimum (so a quiet level never reaches AI).</item>
    ///   <item><b>Structured parameters (defect #6)</b> — <c>Correlation</c>,
    ///   <c>ExecutionId</c>, <c>ActivityName</c> and <c>Message</c> are emitted as
    ///   <i>separate</i> message-template values (queryable <c>customDimensions</c> in AI),
    ///   not collapsed into one interpolated string.</item>
    /// </list>
    /// </summary>
    [TestClass]
    public class AzureExecutionLoggerTests
    {
        private static readonly Guid ExecId =
            Guid.Parse("11111111-2222-3333-4444-555555555555");

        private static AzureExecutionLogger BuildLogger(
            out CapturingLogger<AzureExecutionLogger> captured,
            Dev2LogLevel minimumLevel)
        {
            captured = new CapturingLogger<AzureExecutionLogger>();
            return new AzureExecutionLogger(captured, minimumLevel);
        }

        // ── Constructor guard ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Ctor_NullLogger_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new AzureExecutionLogger(null!));
        }

        // ── Level gating ─────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogDebug_AtInfoLevel_IsSuppressed()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.INFO);

            logger.LogDebug("debug message", ExecId);

            Assert.AreEqual(0, captured.Entries.Count,
                "Debug must not reach AI when the minimum level is INFO.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogTrace_AtInfoLevel_IsSuppressed()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.INFO);

            logger.LogTrace("trace message", ExecId);

            Assert.AreEqual(0, captured.Entries.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogDebug_AtDebugLevel_IsEmittedAtDebug()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.DEBUG);

            logger.LogDebug("debug message", ExecId);

            Assert.AreEqual(1, captured.Entries.Count);
            Assert.AreEqual(LogLevel.Debug, captured.Entries[0].Level);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogTrace_AtTraceLevel_IsEmittedAtTrace()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.TRACE);

            logger.LogTrace("trace message", ExecId);

            Assert.AreEqual(1, captured.Entries.Count);
            Assert.AreEqual(LogLevel.Trace, captured.Entries[0].Level);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllLevels_AtTrace_EmitEveryEntryWithCorrectMelLevel()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.TRACE);

            logger.LogTrace("t", ExecId);
            logger.LogDebug("d", ExecId);
            logger.LogInfo("i", ExecId);
            logger.LogWarning("w", ExecId);
            logger.LogError("e", ExecId);
            logger.LogFatal("f", ExecId);

            var levels = captured.Entries.Select(e => e.Level).ToList();
            CollectionAssert.AreEqual(
                new[]
                {
                    LogLevel.Trace, LogLevel.Debug, LogLevel.Information,
                    LogLevel.Warning, LogLevel.Error, LogLevel.Critical
                },
                levels);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllLevels_AtOff_SuppressEverythingIncludingFatal()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.OFF);

            logger.LogInfo("i", ExecId);
            logger.LogError("e", ExecId);
            logger.LogFatal("f", ExecId);

            Assert.AreEqual(0, captured.Entries.Count,
                "OFF must suppress every entry, including Fatal.");
        }

        // ── Structured parameters (defect #6) ────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogInfo_EmitsExecutionIdAndMessage_AsSeparateDimensions()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.INFO);

            logger.LogInfo("hello world", ExecId);

            var entry = captured.Entries.Single();
            Assert.IsTrue(entry.Values.ContainsKey("Correlation"), "Correlation dimension missing.");
            Assert.AreEqual(ExecId, (Guid)entry.Values["ExecutionId"]!,
                "ExecutionId must be a separate structured value.");
            Assert.AreEqual("hello world", (string)entry.Values["Message"]!,
                "Message must be a separate structured value.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogInfo_ExecutionId_IsNotInterpolatedIntoMessage()
        {
            // Regression guard for defect #6: the previous implementation collapsed
            // everything into "{Message}" via string interpolation, so ExecutionId was
            // NOT a queryable customDimension. The Message dimension must hold ONLY the
            // raw message — never the guid.
            var logger = BuildLogger(out var captured, Dev2LogLevel.INFO);

            logger.LogInfo("pure message", ExecId);

            var message = (string)captured.Entries.Single().Values["Message"]!;
            Assert.AreEqual("pure message", message);
            StringAssert.DoesNotMatch(message,
                new System.Text.RegularExpressions.Regex(ExecId.ToString()),
                "ExecutionId must not be embedded inside the Message dimension.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogInfo_MessageOnlyOverload_EmitsCorrelationAndMessage_NoExecutionId()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.INFO);

            logger.LogInfo("startup message");

            var entry = captured.Entries.Single();
            Assert.IsTrue(entry.Values.ContainsKey("Correlation"));
            Assert.AreEqual("startup message", (string)entry.Values["Message"]!);
            Assert.IsFalse(entry.Values.ContainsKey("ExecutionId"),
                "The message-only overload must not emit an ExecutionId dimension.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogError_WithActivityName_EmitsActivityNameDimension()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.ERROR);
            var ex = new InvalidOperationException("boom");

            logger.LogError("MyActivity", ex, ExecId);

            var entry = captured.Entries.Single();
            Assert.AreEqual("MyActivity", (string)entry.Values["ActivityName"]!,
                "ActivityName must be a separate queryable dimension.");
            Assert.AreEqual(ExecId, (Guid)entry.Values["ExecutionId"]!);
            Assert.AreSame(ex, entry.Exception, "The exception must be passed through to MEL.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogError_WithException_PassesExceptionThrough()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.ERROR);
            var ex = new Exception("db down");

            logger.LogError(ex, "context message");

            var entry = captured.Entries.Single();
            Assert.AreSame(ex, entry.Exception);
            Assert.AreEqual("context message", (string)entry.Values["Message"]!);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LogFatal_WithException_EmitsAtCriticalWithExecutionId()
        {
            var logger = BuildLogger(out var captured, Dev2LogLevel.FATAL);
            var ex = new Exception("fatal");

            logger.LogFatal("fatal message", ex, ExecId);

            var entry = captured.Entries.Single();
            Assert.AreEqual(LogLevel.Critical, entry.Level);
            Assert.AreSame(ex, entry.Exception);
            Assert.AreEqual(ExecId, (Guid)entry.Values["ExecutionId"]!);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Test double: an ILogger that captures the structured state of each call.
        // ─────────────────────────────────────────────────────────────────────

        private sealed class CapturingLogger<T> : ILogger<T>
        {
            public List<CapturedEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull
                => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                var values = new Dictionary<string, object?>(StringComparer.Ordinal);

                // FormattedLogValues implements IReadOnlyList<KeyValuePair<string, object>>;
                // nullability is erased at runtime so the non-nullable generic arg matches.
                if (state is IReadOnlyList<KeyValuePair<string, object>> kvps)
                {
                    foreach (var kv in kvps)
                        values[kv.Key] = kv.Value;
                }

                Entries.Add(new CapturedEntry(
                    logLevel, values, exception, formatter(state, exception)));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }

        private sealed record CapturedEntry(
            LogLevel Level,
            IReadOnlyDictionary<string, object?> Values,
            Exception? Exception,
            string Formatted);
    }
}
