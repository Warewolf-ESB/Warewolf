/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.QueueProcessor.Configuration;

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// Regression cover for the configuration contract between
    /// <c>Deploy-WwQueueProcessor.ps1</c> and <see cref="QueueProcessorOptionsBinder"/>.
    ///
    /// <para><b>The defect these exist for.</b> The mapping used to be a lambda inside
    /// <c>Program.cs</c>'s <c>AddOptions&lt;&gt;().Configure(o =&gt; …)</c>. Top-level statements
    /// cannot be unit tested, so nothing checked that every key the deploy script emits is a key
    /// the worker reads — and two were not. <c>WORKER__MAXDELIVERYATTEMPTS</c> and
    /// <c>WORKER__RETRYENGINEINTERNALERRORS</c> were emitted by the script (lines 1262-1263), were
    /// live on the Container App, and were <b>never assigned</b>. Because their deployed values
    /// happened to equal the property defaults (2 and false), the worker behaved correctly and the
    /// gap was invisible — <c>-MaxDeliveryAttempts 1</c> and <c>-RetryEngineInternalErrors</c> were
    /// silent no-ops.</para>
    ///
    /// <para>The same lambda also defaulted <c>ENGINE:TIMEOUTSECONDS</c> to 45 and
    /// <c>WORKER:SHUTDOWNGRACESECONDS</c> to 60 while <see cref="QueueProcessorOptions"/> documented
    /// 180 and 210 — and since <c>Configure</c> wins, the rationale written on those properties
    /// described behaviour that could not happen.
    /// <see cref="Apply_NothingConfigured_LeavesEveryDocumentedDefaultIntact"/> is the test that
    /// would have caught it.</para>
    /// </summary>
    [TestClass]
    public class OptionsBinderTests
    {
        /// <summary>
        /// Configuration built the way <c>Program.cs</c> builds it, minus the environment layer, so
        /// a test can inject keys without touching process-wide state. Keys use the ':' form the
        /// binder reads; <see cref="Apply_DeployScriptEnvVarNames_ReachTheSameOptions"/> proves the
        /// '__' environment form translates onto exactly these.
        /// </summary>
        static IConfiguration ConfigWith(params (string Key, string Value)[] entries) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(entries.Select(e =>
                    new KeyValuePair<string, string?>(e.Key, e.Value)))
                .Build();

        /// <summary>
        /// Applies configuration onto a fresh options instance, exactly as the host does.
        /// <c>isDevelopment: false</c> mirrors a deployed replica.
        /// </summary>
        static QueueProcessorOptions Bind(params (string Key, string Value)[] entries)
        {
            var options = new QueueProcessorOptions();
            QueueProcessorOptionsBinder.Apply(ConfigWith(entries), options, isDevelopment: false);
            return options;
        }

        // ── The two keys that were emitted and never read ────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_MaxDeliveryAttempts_IsReadFromConfiguration()
        {
            // WORKER__MAXDELIVERYATTEMPTS=1 means "dead-letter on the first transport failure,
            // never retry". Before the fix this was discarded and the worker requeued once anyway.
            var options = Bind(("WORKER:MAXDELIVERYATTEMPTS", "1"));

            Assert.AreEqual(1, options.MaxDeliveryAttempts,
                "WORKER__MAXDELIVERYATTEMPTS must reach MaxDeliveryAttempts - the deploy script " +
                "emits it and -MaxDeliveryAttempts 1 was previously a silent no-op");
            Assert.AreEqual(1, options.EffectiveMaxDeliveryAttempts,
                "1 is inside what the AMQP redelivered flag can express, so it must survive clamping");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_RetryEngineInternalErrors_IsReadFromConfiguration()
        {
            // This is the switch that decides whether an engine HTTP 500 is retried instead of
            // dead-lettered - the safety net during a concurrency ramp, since host memory
            // exhaustion surfaces as 500. It has to actually be readable to be a safety net.
            var options = Bind(("WORKER:RETRYENGINEINTERNALERRORS", "true"));

            Assert.IsTrue(options.RetryEngineInternalErrors,
                "WORKER__RETRYENGINEINTERNALERRORS=true must reach RetryEngineInternalErrors");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_EngineTimeoutSeconds_IsReadFromConfiguration()
        {
            var options = Bind(("ENGINE:TIMEOUTSECONDS", "90"));

            Assert.AreEqual(90, options.EngineTimeoutSeconds,
                "ENGINE__TIMEOUTSECONDS must reach EngineTimeoutSeconds");
        }

        // ── The regression test that would have caught the whole class of defect ─────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_NothingConfigured_LeavesEveryDocumentedDefaultIntact()
        {
            // Deliberately asserts the documented LITERALS rather than comparing against a fresh
            // QueueProcessorOptions. Comparing two instances would be tautological now that the
            // binder reads its fallbacks off the options object - it would pass even if both sides
            // drifted together. These numbers are the ones the property XML docs justify.
            var options = Bind();

            Assert.AreEqual(180, options.EngineTimeoutSeconds,
                "documented default is 180s, sized from measured engine latency; Program.cs used to " +
                "default this to 45, which caused the 2026-08-11 consumer stall");
            Assert.AreEqual(210, options.ShutdownGraceSeconds,
                "documented default is 210s so it tracks the 180s engine timeout; Program.cs used to " +
                "default this to 60");
            Assert.AreEqual(1, options.MaxConcurrency, "documented default in-flight cap");
            Assert.AreEqual(2, options.MaxDeliveryAttempts,
                "documented default: requeue once, dead-letter if it fails again");
            Assert.IsFalse(options.RetryEngineInternalErrors,
                "documented default: a 500 is dead-lettered, not retried, because 500 is overloaded");
            Assert.AreEqual(300, options.TokenRefreshSkewSeconds, "documented token refresh skew");
            Assert.AreEqual("triggers", options.TriggersSubPath);
            Assert.AreEqual("sources", options.SourcesSubPath);
            Assert.AreEqual("*.bite", options.TriggerFilter);
            Assert.AreEqual(
                Path.Combine(AppContext.BaseDirectory, "Settings"), options.SettingsPath,
                "the settings tree is anchored to the assembly directory, never the working directory");
            Assert.IsNull(options.UseSsl, "unset TLS leaves the decision to the source .bite");

            // The nesting rule the defaults must satisfy on their own: an engine call that can
            // outlive the drain window turns every scale-in into a duplicate execution. A pair of
            // defaults that fails this would crash the worker at startup, not at the first message.
            options.BaseUrl = "https://engine";
            options.ResourceAppId = "app";
            options.Validate(terminationGracePeriodSeconds: 0);
        }

        // ── "Empty means unset" — the rule ConfigurationValues exists for ───────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_BlankValues_FallBackToDefaultsRatherThanWinning()
        {
            // appsettings.json ships keys as "" placeholders so the contract is discoverable, and
            // ACA turns an unset variable into an empty string as readily as into a missing one.
            // A blank must therefore lose to the default, which plain '??' would not do.
            var options = Bind(
                ("QUEUE:SETTINGSPATH", "   "),
                ("QUEUE:TRIGGERSSUBPATH", ""),
                ("QUEUE:TRIGGERFILTER", ""),
                ("ENGINE:TIMEOUTSECONDS", ""),
                ("WORKER:MAXCONCURRENCY", ""),
                ("WORKER:MAXDELIVERYATTEMPTS", ""),
                ("WORKER:RETRYENGINEINTERNALERRORS", ""));

            Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "Settings"), options.SettingsPath,
                "a whitespace QUEUE__SETTINGSPATH previously beat the default and resolved the " +
                "settings tree against the process working directory");
            Assert.AreEqual("triggers", options.TriggersSubPath);
            Assert.AreEqual("*.bite", options.TriggerFilter);
            Assert.AreEqual(180, options.EngineTimeoutSeconds);
            Assert.AreEqual(1, options.MaxConcurrency);
            Assert.AreEqual(2, options.MaxDeliveryAttempts);
            Assert.IsFalse(options.RetryEngineInternalErrors);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_UseSsl_IsTriStateNotBoolean()
        {
            // Three distinct outcomes, not two: unset must stay null so the source .bite decides,
            // rather than collapsing onto false and silently downgrading a TLS broker connection.
            Assert.IsNull(Bind().UseSsl, "absent must mean 'let the source decide'");
            Assert.IsNull(Bind(("RABBITMQ:USESSL", "  ")).UseSsl, "blank must mean the same as absent");
            Assert.AreEqual(true, Bind(("RABBITMQ:USESSL", "true")).UseSsl);
            Assert.AreEqual(false, Bind(("RABBITMQ:USESSL", "false")).UseSsl);
        }

        // ── The deploy script's own variable names, through the real env-var provider ────

        [TestMethod]
        [TestCategory("UnitTest")]
        [DoNotParallelize]
        public void Apply_DeployScriptEnvVarNames_ReachTheSameOptions()
        {
            // The tests above use the ':' key form the binder reads. This one closes the loop back
            // to what Deploy-WwQueueProcessor.ps1 actually emits - 'WORKER__MAXDELIVERYATTEMPTS'
            // with a double underscore - and proves the EnvironmentVariablesConfigurationProvider
            // translates it onto the key the binder looks up. Without this, every assertion above
            // could pass while the deployed variable still went nowhere.
            //
            // Mutates process-wide state, hence [DoNotParallelize] and the finally block.
            const string attempts = "WORKER__MAXDELIVERYATTEMPTS";
            const string retry = "WORKER__RETRYENGINEINTERNALERRORS";
            const string timeout = "ENGINE__TIMEOUTSECONDS";

            var savedAttempts = Environment.GetEnvironmentVariable(attempts);
            var savedRetry = Environment.GetEnvironmentVariable(retry);
            var savedTimeout = Environment.GetEnvironmentVariable(timeout);

            try
            {
                Environment.SetEnvironmentVariable(attempts, "1");
                Environment.SetEnvironmentVariable(retry, "true");
                Environment.SetEnvironmentVariable(timeout, "120");

                var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
                var options = new QueueProcessorOptions();
                QueueProcessorOptionsBinder.Apply(configuration, options, isDevelopment: false);

                Assert.AreEqual(1, options.MaxDeliveryAttempts,
                    $"{attempts} is what the deploy script emits; it must land on MaxDeliveryAttempts");
                Assert.IsTrue(options.RetryEngineInternalErrors,
                    $"{retry} is what the deploy script emits; it must land on RetryEngineInternalErrors");
                Assert.AreEqual(120, options.EngineTimeoutSeconds,
                    $"{timeout} is what the deploy script emits; it must land on EngineTimeoutSeconds");
            }
            finally
            {
                Environment.SetEnvironmentVariable(attempts, savedAttempts);
                Environment.SetEnvironmentVariable(retry, savedRetry);
                Environment.SetEnvironmentVariable(timeout, savedTimeout);
            }
        }
    }
}
