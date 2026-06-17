/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.Lightweight.Tests.Logging
{
    /// <summary>
    /// Coverage for <see cref="ApplicationInsightsLogFilter"/> — the helper extracted from
    /// <c>Program.cs</c> for the 8436 fix. These tests verify the four guarantees the AI
    /// level-gate depends on:
    /// <list type="number">
    ///   <item>The AI SDK's built-in Warning rule is removed.</item>
    ///   <item>Exactly one AI rule remains, set to the configured level.</item>
    ///   <item>Rules for other providers (Console, Elastic) are never touched.</item>
    ///   <item>The operation is idempotent and null-safe.</item>
    /// </list>
    /// </summary>
    [TestClass]
    public class ApplicationInsightsLogFilterTests
    {
        private const string AiProvider = ApplicationInsightsLogFilter.AiProviderName;
        private const string ConsoleProvider =
            "Microsoft.Extensions.Logging.Console.ConsoleLoggerProvider";

        private static LoggerFilterRule? AiRule(LoggerFilterOptions options) =>
            options.Rules.SingleOrDefault(r => r.ProviderName == AiProvider);

        // ── Rule replacement ─────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_RemovesExistingAiWarningRule_AndAddsTargetLevel()
        {
            var options = new LoggerFilterOptions();
            // Simulate the AI SDK's default Warning gate.
            options.Rules.Add(new LoggerFilterRule(AiProvider, null, LogLevel.Warning, null));

            ApplicationInsightsLogFilter.Apply(options, LogLevel.Trace);

            var rule = AiRule(options);
            Assert.IsNotNull(rule, "Exactly one AI rule must remain.");
            Assert.AreEqual(LogLevel.Trace, rule!.LogLevel,
                "AI rule must be set to the configured level, replacing the Warning default.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_NoExistingAiRule_AddsTargetRule()
        {
            var options = new LoggerFilterOptions();

            ApplicationInsightsLogFilter.Apply(options, LogLevel.Debug);

            var rule = AiRule(options);
            Assert.IsNotNull(rule);
            Assert.AreEqual(LogLevel.Debug, rule!.LogLevel);
            Assert.IsNull(rule.CategoryName, "Rule must be provider-scoped, not category-scoped.");
        }

        // ── Provider scoping ─────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_DoesNotModifyOtherProviderRules()
        {
            var options = new LoggerFilterOptions();
            var consoleRule = new LoggerFilterRule(ConsoleProvider, null, LogLevel.Information, null);
            options.Rules.Add(consoleRule);

            ApplicationInsightsLogFilter.Apply(options, LogLevel.Trace);

            var survivingConsole = options.Rules.SingleOrDefault(r => r.ProviderName == ConsoleProvider);
            Assert.IsNotNull(survivingConsole, "Console provider rule must be untouched.");
            Assert.AreSame(consoleRule, survivingConsole,
                "The exact Console rule instance must remain — scoping is AI-only.");
            Assert.AreEqual(LogLevel.Information, survivingConsole!.LogLevel);
        }

        // ── Idempotency ──────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_CalledTwice_LeavesExactlyOneAiRule()
        {
            var options = new LoggerFilterOptions();

            ApplicationInsightsLogFilter.Apply(options, LogLevel.Debug);
            ApplicationInsightsLogFilter.Apply(options, LogLevel.Trace);

            var aiRules = options.Rules.Where(r => r.ProviderName == AiProvider).ToList();
            Assert.AreEqual(1, aiRules.Count, "Repeated Apply must not accumulate AI rules.");
            Assert.AreEqual(LogLevel.Trace, aiRules[0].LogLevel,
                "The most recent level must win.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_MultiplePreexistingAiRules_AllRemovedAndCollapsedToOne()
        {
            var options = new LoggerFilterOptions();
            // The old FirstOrDefault-based logic would only remove ONE of these.
            options.Rules.Add(new LoggerFilterRule(AiProvider, null, LogLevel.Warning, null));
            options.Rules.Add(new LoggerFilterRule(AiProvider, null, LogLevel.Error, null));

            ApplicationInsightsLogFilter.Apply(options, LogLevel.Information);

            var aiRules = options.Rules.Where(r => r.ProviderName == AiProvider).ToList();
            Assert.AreEqual(1, aiRules.Count,
                "ALL pre-existing AI rules must be removed, not just the first.");
            Assert.AreEqual(LogLevel.Information, aiRules[0].LogLevel);
        }

        // ── Null-safety ──────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Apply_NullOptions_DoesNotThrow()
        {
            ApplicationInsightsLogFilter.Apply(null!, LogLevel.Trace);
        }

        // ── Provider-name constant guard ─────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AiProviderName_MatchesFullyQualifiedSdkProviderType()
        {
            // Guards against a silent rename: if the AI SDK provider type FQN changes,
            // the filter rule would no longer match and the gate would silently break.
            Assert.AreEqual(
                "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider",
                ApplicationInsightsLogFilter.AiProviderName);
        }
    }
}
