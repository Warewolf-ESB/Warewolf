/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Logging;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Tests.Logging
{
    /// <summary>
    /// Coverage for <see cref="ExecutionLogLevel"/> — the parser and gate that turn the
    /// <c>EXECUTIONLOGLEVEL</c> environment variable into the level honoured by every
    /// <see cref="IExecutionLogger"/>. The 8436 fix relies on this parsing being correct
    /// for both numeric and named inputs and on the higher-is-more-verbose comparison.
    /// </summary>
    [TestClass]
    public class ExecutionLogLevelTests
    {
        // ── Parse: named values ──────────────────────────────────────────────

        [DataTestMethod]
        [DataRow("TRACE", Dev2LogLevel.TRACE)]
        [DataRow("DEBUG", Dev2LogLevel.DEBUG)]
        [DataRow("INFO",  Dev2LogLevel.INFO)]
        [DataRow("WARN",  Dev2LogLevel.WARN)]
        [DataRow("ERROR", Dev2LogLevel.ERROR)]
        [DataRow("FATAL", Dev2LogLevel.FATAL)]
        [DataRow("OFF",   Dev2LogLevel.OFF)]
        [TestCategory("UnitTest")]
        public void Parse_NamedValue_IsHonoured(string raw, Dev2LogLevel expected)
        {
            Assert.AreEqual(expected, ExecutionLogLevel.Parse(raw));
        }

        [DataTestMethod]
        [DataRow("trace", Dev2LogLevel.TRACE)]
        [DataRow("Debug", Dev2LogLevel.DEBUG)]
        [DataRow("InFo",  Dev2LogLevel.INFO)]
        [TestCategory("UnitTest")]
        public void Parse_NamedValue_IsCaseInsensitive(string raw, Dev2LogLevel expected)
        {
            Assert.AreEqual(expected, ExecutionLogLevel.Parse(raw));
        }

        // ── Parse: numeric values ────────────────────────────────────────────

        [DataTestMethod]
        [DataRow("0", Dev2LogLevel.OFF)]
        [DataRow("1", Dev2LogLevel.FATAL)]
        [DataRow("2", Dev2LogLevel.ERROR)]
        [DataRow("3", Dev2LogLevel.WARN)]
        [DataRow("4", Dev2LogLevel.INFO)]
        [DataRow("5", Dev2LogLevel.DEBUG)]
        [DataRow("6", Dev2LogLevel.TRACE)]
        [TestCategory("UnitTest")]
        public void Parse_NumericValue_IsHonoured(string raw, Dev2LogLevel expected)
        {
            Assert.AreEqual(expected, ExecutionLogLevel.Parse(raw));
        }

        // ── Parse: fallbacks ─────────────────────────────────────────────────

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("not-a-level")]
        [DataRow("99")]
        [DataRow("-1")]
        [TestCategory("UnitTest")]
        public void Parse_InvalidOrEmpty_FallsBackToDefaultInfo(string? raw)
        {
            Assert.AreEqual(ExecutionLogLevel.Default, ExecutionLogLevel.Parse(raw));
            Assert.AreEqual(Dev2LogLevel.INFO, ExecutionLogLevel.Parse(raw));
        }

        // ── ShouldLog: verbose-gate semantics ────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ShouldLog_AtTrace_EmitsEveryLevel()
        {
            const Dev2LogLevel min = Dev2LogLevel.TRACE;

            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.TRACE, min));
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.DEBUG, min));
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.INFO,  min));
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.WARN,  min));
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.ERROR, min));
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.FATAL, min));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ShouldLog_AtInfo_SuppressesDebugAndTrace()
        {
            const Dev2LogLevel min = Dev2LogLevel.INFO;

            Assert.IsFalse(ExecutionLogLevel.ShouldLog(Dev2LogLevel.TRACE, min));
            Assert.IsFalse(ExecutionLogLevel.ShouldLog(Dev2LogLevel.DEBUG, min));
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.INFO,  min));
            Assert.IsTrue(ExecutionLogLevel.ShouldLog(Dev2LogLevel.ERROR, min));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ShouldLog_AtOff_SuppressesEverythingIncludingFatal()
        {
            const Dev2LogLevel min = Dev2LogLevel.OFF;

            Assert.IsFalse(ExecutionLogLevel.ShouldLog(Dev2LogLevel.FATAL, min));
            Assert.IsFalse(ExecutionLogLevel.ShouldLog(Dev2LogLevel.ERROR, min));
            Assert.IsFalse(ExecutionLogLevel.ShouldLog(Dev2LogLevel.INFO,  min));
        }

        // ── Read: environment integration ────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [DoNotParallelize]
        public void Read_HonoursEnvironmentVariable()
        {
            var original = Environment.GetEnvironmentVariable("EXECUTIONLOGLEVEL");
            try
            {
                Environment.SetEnvironmentVariable("EXECUTIONLOGLEVEL", "DEBUG");
                Assert.AreEqual(Dev2LogLevel.DEBUG, ExecutionLogLevel.Read());

                Environment.SetEnvironmentVariable("EXECUTIONLOGLEVEL", null);
                Assert.AreEqual(Dev2LogLevel.INFO, ExecutionLogLevel.Read());
            }
            finally
            {
                Environment.SetEnvironmentVariable("EXECUTIONLOGLEVEL", original);
            }
        }
    }
}
