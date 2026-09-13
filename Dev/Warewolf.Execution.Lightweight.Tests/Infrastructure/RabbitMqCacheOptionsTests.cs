/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure
{
    /// <summary>
    /// Covers <see cref="RabbitMqCacheOptions"/>, whose single most important property is that it
    /// fails CLOSED: every ambiguous input must leave the cache disabled, because a misread app
    /// setting silently enabling connection reuse in production is a far worse outcome than the
    /// feature not turning on.
    /// </summary>
    [TestClass]
    public class RabbitMqCacheOptionsTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        [DataRow(null, DisplayName = "absent")]
        [DataRow("", DisplayName = "empty")]
        [DataRow("   ", DisplayName = "whitespace")]
        public void RabbitMqCacheOptions_AbsentOrBlank_IsDisabled(string raw)
        {
            var options = RabbitMqCacheOptions.FromJson(raw);

            Assert.IsFalse(options.Enabled, "the default posture must be uncached");
            Assert.AreSame(RabbitMqCacheOptions.Disabled, options);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        [DataRow("{not json", DisplayName = "truncated")]
        [DataRow("[]", DisplayName = "array not object")]
        [DataRow("{\"enabled\":\"yes\"}", DisplayName = "wrong type for enabled")]
        [DataRow("{\"enabled\":true,\"idleTimeout\":\"five minutes\"}", DisplayName = "unparseable TimeSpan")]
        public void RabbitMqCacheOptions_Malformed_IsDisabledAndDoesNotThrow(string raw)
        {
            // A typo in an app setting must never take the Function host down, and must never be
            // resolved in favour of turning the feature on.
            var options = RabbitMqCacheOptions.FromJson(raw);

            Assert.IsFalse(options.Enabled);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        public void RabbitMqCacheOptions_ExplicitlyDisabled_IgnoresEveryOtherSetting()
        {
            var options = RabbitMqCacheOptions.FromJson(
                "{\"enabled\":false,\"idleTimeout\":\"01:00:00\",\"maxConnections\":32}");

            Assert.IsFalse(options.Enabled);
            Assert.AreSame(RabbitMqCacheOptions.Disabled, options);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        public void RabbitMqCacheOptions_FullyPopulated_BindsEveryField()
        {
            var options = RabbitMqCacheOptions.FromJson(
                "{\"enabled\":true,\"idleTimeout\":\"00:07:00\",\"maxLifetime\":\"00:45:00\"," +
                "\"topologyTtl\":\"00:00:45\",\"maxConnections\":6,\"sweepInterval\":\"00:02:00\"}");

            Assert.IsTrue(options.Enabled);
            Assert.AreEqual(TimeSpan.FromMinutes(7), options.IdleTimeout);
            Assert.AreEqual(TimeSpan.FromMinutes(45), options.MaxLifetime);
            Assert.AreEqual(TimeSpan.FromSeconds(45), options.TopologyTtl);
            Assert.AreEqual(6, options.MaxConnections);
            Assert.AreEqual(TimeSpan.FromMinutes(2), options.SweepInterval);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        public void RabbitMqCacheOptions_OnlyEnabled_UsesEveryDocumentedDefault()
        {
            var options = RabbitMqCacheOptions.FromJson("{\"enabled\":true}");

            Assert.IsTrue(options.Enabled);
            Assert.AreEqual(RabbitMqCacheOptions.DefaultIdleTimeout, options.IdleTimeout);
            Assert.AreEqual(RabbitMqCacheOptions.DefaultMaxLifetime, options.MaxLifetime);
            Assert.AreEqual(RabbitMqCacheOptions.DefaultTopologyTtl, options.TopologyTtl);
            Assert.AreEqual(RabbitMqCacheOptions.DefaultMaxConnections, options.MaxConnections);
            Assert.AreEqual(RabbitMqCacheOptions.DefaultSweepInterval, options.SweepInterval);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        public void RabbitMqCacheOptions_ZeroTopologyTtl_IsHonouredBecauseItMeansMemoOff()
        {
            // Zero is a REAL value here, not "unset": it keeps connection reuse while restoring the
            // passive declares on every publish. Defaulting it away would remove the documented
            // escape hatch for a broker whose queues are managed out-of-band.
            var options = RabbitMqCacheOptions.FromJson("{\"enabled\":true,\"topologyTtl\":\"00:00:00\"}");

            Assert.IsTrue(options.Enabled);
            Assert.AreEqual(TimeSpan.Zero, options.TopologyTtl);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        public void RabbitMqCacheOptions_OutOfRangeValues_FallBackOrClamp()
        {
            var negatives = RabbitMqCacheOptions.FromJson(
                "{\"enabled\":true,\"idleTimeout\":\"-00:05:00\",\"maxLifetime\":\"00:00:00\"," +
                "\"topologyTtl\":\"-00:00:30\",\"sweepInterval\":\"-00:01:00\"}");

            Assert.AreEqual(RabbitMqCacheOptions.DefaultIdleTimeout, negatives.IdleTimeout, "negative idle");
            Assert.AreEqual(RabbitMqCacheOptions.DefaultMaxLifetime, negatives.MaxLifetime, "zero lifetime");
            Assert.AreEqual(RabbitMqCacheOptions.DefaultTopologyTtl, negatives.TopologyTtl, "negative ttl");
            Assert.AreEqual(RabbitMqCacheOptions.DefaultSweepInterval, negatives.SweepInterval, "negative sweep");

            Assert.AreEqual(RabbitMqCacheOptions.DefaultMaxConnections,
                RabbitMqCacheOptions.FromJson("{\"enabled\":true,\"maxConnections\":0}").MaxConnections);
            Assert.AreEqual(RabbitMqCacheOptions.DefaultMaxConnections,
                RabbitMqCacheOptions.FromJson("{\"enabled\":true,\"maxConnections\":-3}").MaxConnections);

            // The ceiling exists because each cached connection costs a socket, an I/O thread and two
            // timers on an instance capped at 1.5 GB. A typo must not be able to exhaust the host.
            Assert.AreEqual(RabbitMqCacheOptions.MaxAllowedConnections,
                RabbitMqCacheOptions.FromJson("{\"enabled\":true,\"maxConnections\":100000}").MaxConnections);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        public void RabbitMqCacheOptions_IsLenientAboutCasingCommentsAndUnknownFields()
        {
            var options = RabbitMqCacheOptions.FromJson(
                "{ /* operator note */ \"Enabled\":true, \"IDLETIMEOUT\":\"00:09:00\", \"future\":123, }");

            Assert.IsTrue(options.Enabled);
            Assert.AreEqual(TimeSpan.FromMinutes(9), options.IdleTimeout,
                "property matching must be case-insensitive, as it is for every other WAREWOLF_* setting");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        [DoNotParallelize]
        public void RabbitMqCacheOptions_FromEnvironment_ReadsTheDocumentedVariableName()
        {
            // DoNotParallelize, not a CannotParallelize category: the CI filter excludes that
            // category, which would silently stop this test from ever running.
            var original = Environment.GetEnvironmentVariable(RabbitMqCacheOptions.EnvVar);
            try
            {
                Environment.SetEnvironmentVariable(RabbitMqCacheOptions.EnvVar,
                    "{\"enabled\":true,\"maxConnections\":3}");

                var options = RabbitMqCacheOptions.FromEnvironment();

                Assert.IsTrue(options.Enabled);
                Assert.AreEqual(3, options.MaxConnections);
                Assert.AreEqual("WAREWOLF_RABBITMQ_CACHE", RabbitMqCacheOptions.EnvVar,
                    "the deployed variable name is a contract with the deploy scripts and docs");
            }
            finally
            {
                Environment.SetEnvironmentVariable(RabbitMqCacheOptions.EnvVar, original);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(RabbitMqCacheOptions))]
        public void RabbitMqCacheOptions_ToString_DoesNotPretendDisabledIsConfigured()
        {
            Assert.AreEqual("disabled", RabbitMqCacheOptions.Disabled.ToString());
            StringAssert.StartsWith(RabbitMqCacheOptions.FromJson("{\"enabled\":true}").ToString(), "enabled");
        }
    }
}
