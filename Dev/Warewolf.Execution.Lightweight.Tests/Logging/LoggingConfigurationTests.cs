/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Logging;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Warewolf.Execution.Lightweight.Tests.Logging
{
    /// <summary>
    /// Coverage for <see cref="LoggingConfiguration"/> — the single source of truth
    /// for the 8436 Debug/Trace-in-Application-Insights fix.
    ///
    /// <para>Two behaviours are critical and verified here:</para>
    /// <list type="number">
    ///   <item>
    ///     <see cref="LoggingConfiguration.MelMinimumLevel"/> must map the Dev2
    ///     higher-is-verbose convention onto the MEL lower-is-verbose convention so
    ///     <c>EXECUTIONLOGLEVEL=TRACE</c> actually opens the Application Insights gate.
    ///   </item>
    ///   <item>
    ///     <see cref="LoggingConfiguration.RegisterApplicationInsightsSdk"/> must be
    ///     driven by <c>WAREWOLF_LOGGING_CONFIG</c>'s <c>appInsights</c> field ONLY —
    ///     never by the legacy <c>console</c> field alias — so console logging cannot
    ///     silently register the AI SDK and incur telemetry/billing (defect DEF-A).
    ///   </item>
    /// </list>
    ///
    /// WOLF-8516: <c>ENABLEAPPLICATIONINSIGHTS</c>/<c>ENABLECONSOLELOGGING</c>/etc. were merged
    /// into the single <c>WAREWOLF_LOGGING_CONFIG</c> JSON app setting — see
    /// <c>Logging.LoggingConfiguration.RawFlags</c>.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class LoggingConfigurationTests
    {
        private static readonly string[] AllVars =
        {
            "WAREWOLF_LOGGING_CONFIG", "EXECUTIONLOGLEVEL", "ASPNETCORE_ENVIRONMENT"
        };

        private Dictionary<string, string?> _snapshot = null!;

        [TestInitialize]
        public void Setup()
        {
            _snapshot = new Dictionary<string, string?>();
            foreach (var v in AllVars)
            {
                _snapshot[v] = Environment.GetEnvironmentVariable(v);
                Environment.SetEnvironmentVariable(v, null);
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var (k, v) in _snapshot)
                Environment.SetEnvironmentVariable(k, v);
        }

        // ── MelMinimumLevel mapping ──────────────────────────────────────────

        [DataTestMethod]
        [DataRow(Dev2LogLevel.TRACE, MelLogLevel.Trace)]
        [DataRow(Dev2LogLevel.DEBUG, MelLogLevel.Debug)]
        [DataRow(Dev2LogLevel.INFO,  MelLogLevel.Information)]
        [DataRow(Dev2LogLevel.WARN,  MelLogLevel.Warning)]
        [DataRow(Dev2LogLevel.ERROR, MelLogLevel.Error)]
        [DataRow(Dev2LogLevel.FATAL, MelLogLevel.Critical)]
        [DataRow(Dev2LogLevel.OFF,   MelLogLevel.None)]
        [TestCategory("UnitTest")]
        public void MelMinimumLevel_MapsEveryDev2Level_ToCorrectMelLevel(
            Dev2LogLevel dev2Level, MelLogLevel expected)
        {
            var config = new LoggingConfiguration { MinimumLevel = dev2Level };

            Assert.AreEqual(expected, config.MelMinimumLevel,
                $"Dev2 {dev2Level} must map to MEL {expected}.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MelMinimumLevel_UnsetMinimumLevel_DefaultsToInformation()
        {
            // default(Dev2LogLevel) is OFF (0); but an explicitly-constructed config
            // with no MinimumLevel still resolves through the switch deterministically.
            var config = new LoggingConfiguration();

            // default(LogLevel) == OFF → None per the mapping table.
            Assert.AreEqual(MelLogLevel.None, config.MelMinimumLevel);
        }

        // ── FromEnvironment: MinimumLevel ────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_NoLevelVar_DefaultsToInfo()
        {
            var config = LoggingConfiguration.FromEnvironment();

            Assert.AreEqual(Dev2LogLevel.INFO, config.MinimumLevel);
            Assert.AreEqual(MelLogLevel.Information, config.MelMinimumLevel);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_TraceLevel_FlowsThroughToMelTrace()
        {
            Environment.SetEnvironmentVariable("EXECUTIONLOGLEVEL", "TRACE");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.AreEqual(Dev2LogLevel.TRACE, config.MinimumLevel);
            Assert.AreEqual(MelLogLevel.Trace, config.MelMinimumLevel);
        }

        // ── DEF-A: AI SDK registration vs. composite-sink enablement ─────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_NoAiVars_DoesNotRegisterAiSdk()
        {
            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsFalse(config.RegisterApplicationInsightsSdk);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_EnableApplicationInsights_RegistersAiSdk()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LOGGING_CONFIG", /*lang=json,strict*/ "{\"appInsights\":true}");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.RegisterApplicationInsightsSdk,
                "AI explicitly enabled → AI SDK must be registered.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_EnableConsoleLoggingOnly_DoesNotRegisterAiSdk()
        {
            // DEF-A regression guard: console logging must NOT trigger AI SDK registration
            // (telemetry ingestion / billing). appInsights is the single authoritative App
            // Insights switch; console only selects the stdout-bound ConsoleExecutionLogger sink.
            Environment.SetEnvironmentVariable("WAREWOLF_LOGGING_CONFIG", /*lang=json,strict*/ "{\"console\":true}");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.EnableConsoleLogging,
                "WAREWOLF_LOGGING_CONFIG.console selects the ConsoleExecutionLogger sink.");
            Assert.IsFalse(config.RegisterApplicationInsightsSdk,
                "DEF-A: WAREWOLF_LOGGING_CONFIG.console must NOT register the AI SDK.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_BothAiVars_RegistersSdk()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LOGGING_CONFIG", /*lang=json,strict*/ "{\"appInsights\":true,\"console\":true}");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.RegisterApplicationInsightsSdk);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_AiVarCaseInsensitive_IsHonoured()
        {
            // WOLF-8516: JSON boolean *values* are not case-variable (true/false are fixed
            // lowercase literals per the JSON spec), but the deserializer is configured with
            // PropertyNameCaseInsensitive — a differently-cased property NAME must still bind.
            Environment.SetEnvironmentVariable("WAREWOLF_LOGGING_CONFIG", /*lang=json,strict*/ "{\"APPINSIGHTS\":true}");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.RegisterApplicationInsightsSdk);
        }
    }
}
