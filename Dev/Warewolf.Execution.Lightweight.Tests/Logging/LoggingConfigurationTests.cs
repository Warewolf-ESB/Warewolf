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
    ///     driven by <c>ENABLEAPPLICATIONINSIGHTS</c> ONLY — never by the legacy
    ///     <c>ENABLECONSOLELOGGING</c> alias — so console logging cannot silently
    ///     register the AI SDK and incur telemetry/billing (defect DEF-A).
    ///   </item>
    /// </list>
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class LoggingConfigurationTests
    {
        private static readonly string[] AllVars =
        {
            "ENABLEAPPLICATIONINSIGHTS", "ENABLECONSOLELOGGING", "ENABLEELASTICSEARCHLOGGING",
            "EXECUTIONLOGLEVEL", "STRUCTURED_LOGS", "ELASTIC_DEBUG_MODE", "ASPNETCORE_ENVIRONMENT"
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
        public void FromEnvironment_NoAiVars_BothAiFlagsFalse()
        {
            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsFalse(config.EnableApplicationInsights);
            Assert.IsFalse(config.RegisterApplicationInsightsSdk);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_EnableApplicationInsights_SetsBothFlags()
        {
            Environment.SetEnvironmentVariable("ENABLEAPPLICATIONINSIGHTS", "true");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.EnableApplicationInsights,
                "AI explicitly enabled → AzureExecutionLogger joins the composite.");
            Assert.IsTrue(config.RegisterApplicationInsightsSdk,
                "AI explicitly enabled → AI SDK must be registered.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_EnableConsoleLoggingOnly_DoesNotRegisterAiSdk()
        {
            // DEF-A regression guard: the legacy backward-compat alias must NOT trigger
            // AI SDK registration (telemetry ingestion / billing). It only keeps the
            // AzureExecutionLogger in the composite for log-routing compatibility.
            Environment.SetEnvironmentVariable("ENABLECONSOLELOGGING", "true");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.EnableApplicationInsights,
                "Backward compat: ENABLECONSOLELOGGING still enables the composite sink.");
            Assert.IsFalse(config.RegisterApplicationInsightsSdk,
                "DEF-A: ENABLECONSOLELOGGING must NOT register the AI SDK.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_BothAiVars_RegistersSdk()
        {
            Environment.SetEnvironmentVariable("ENABLEAPPLICATIONINSIGHTS", "true");
            Environment.SetEnvironmentVariable("ENABLECONSOLELOGGING", "true");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.EnableApplicationInsights);
            Assert.IsTrue(config.RegisterApplicationInsightsSdk);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void FromEnvironment_AiVarCaseInsensitive_IsHonoured()
        {
            Environment.SetEnvironmentVariable("ENABLEAPPLICATIONINSIGHTS", "TRUE");

            var config = LoggingConfiguration.FromEnvironment();

            Assert.IsTrue(config.RegisterApplicationInsightsSdk);
        }
    }
}
