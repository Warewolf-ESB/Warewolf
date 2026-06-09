using System.Linq;
using Microsoft.Extensions.Logging;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Configures the Application Insights logging filter so that the
    /// <c>EXECUTIONLOGLEVEL</c> environment variable — not the AI SDK's built-in
    /// <see cref="LogLevel.Warning"/> default — controls which entries reach the
    /// Application Insights <c>traces</c> table and the Azure Portal Live Log Stream.
    ///
    /// <para><b>Why a dedicated helper:</b> the logic was previously inlined in
    /// <c>Program.cs</c> where it could not be unit-tested and silently relied on
    /// <see cref="System.Linq.Enumerable.FirstOrDefault{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>,
    /// removing only the <i>first</i> AI rule. Extracting it makes the behaviour
    /// idempotent, scoped, and verifiable in isolation.</para>
    ///
    /// <para><b>Scope guarantee:</b> a provider-scoped <see cref="LoggerFilterRule"/>
    /// is added for the AI provider only. Console, Elasticsearch, Audit and any other
    /// registered provider are never touched, so framework <c>Microsoft.*</c> /
    /// <c>System.*</c> Debug noise is not forced into every sink.</para>
    /// </summary>
    public static class ApplicationInsightsLogFilter
    {
        /// <summary>
        /// Fully-qualified provider name MEL uses to key the Application Insights
        /// logger provider when matching <see cref="LoggerFilterRule.ProviderName"/>.
        /// </summary>
        public const string AiProviderName =
            "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider";

        /// <summary>
        /// Replaces every existing Application Insights provider rule with a single
        /// targeted rule gated at <paramref name="minimumLevel"/>.
        ///
        /// <para>Idempotent: calling it repeatedly leaves exactly one AI rule at the
        /// requested level. Null-safe: a <c>null</c> <paramref name="options"/> is ignored.</para>
        /// </summary>
        /// <param name="options">The MEL filter options to mutate (from DI <c>Configure&lt;LoggerFilterOptions&gt;</c>).</param>
        /// <param name="minimumLevel">The minimum level the AI provider should emit (typically <c>loggingConfig.MelMinimumLevel</c>).</param>
        public static void Apply(LoggerFilterOptions options, LogLevel minimumLevel)
        {
            if (options is null)
                return;

            // Remove ALL AI provider rules (not just the first) so the result is
            // deterministic regardless of how many rules the AI SDK registered.
            var existing = options.Rules
                .Where(r => r.ProviderName == AiProviderName)
                .ToList();

            foreach (var rule in existing)
                options.Rules.Remove(rule);

            // Add a single targeted rule for the AI provider only.
            options.Rules.Add(new LoggerFilterRule(
                providerName: AiProviderName,
                categoryName: null,
                logLevel:     minimumLevel,
                filter:       null));
        }
    }
}
