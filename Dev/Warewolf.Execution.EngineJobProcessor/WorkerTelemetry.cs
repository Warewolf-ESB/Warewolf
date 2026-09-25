/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.EngineJobProcessor
{
    /// <summary>
    /// Worker-side Application Insights registration, extracted from <c>Program.cs</c> so it can be
    /// unit tested (top-level statements cannot be).
    ///
    /// <para><b>Connection string: the STANDARD name.</b> Unlike the Execution Engine, which reads the
    /// deliberately non-standard <c>WAREWOLF_APPINSIGHTS_CONNECTION_STRING</c> to keep the Functions
    /// host's own AI pipeline dormant, this processor uses <see cref="ConnectionStringSettingName"/>.
    /// The host pipeline is what writes the <c>requests</c>/<c>exceptions</c> tables (one row per
    /// JobPoll/Reaper/Watchdog invocation), and the engine's reason for avoiding it - the host
    /// flattening captured worker stdout to Information - does not apply: no console provider is
    /// attached here, so <c>ILogger</c> output keeps its level. Deploy-WwJobProcessor.ps1 writes this
    /// name. Before this, the processor registered the SDK with no connection string while the
    /// deploy wrote only the engine's name, so nothing was ever sent.</para>
    ///
    /// <para><b>Level:</b> the AI SDK adds a provider rule that drops everything below Warning, which
    /// silently discarded every Information line the functions log. It is replaced with
    /// <see cref="MinimumLevel"/>, matching host.json's <c>logLevel.default</c>.</para>
    /// </summary>
    public static class WorkerTelemetry
    {
        public const string ConnectionStringSettingName = "APPLICATIONINSIGHTS_CONNECTION_STRING";

        public const LogLevel MinimumLevel = LogLevel.Information;

        /// <param name="connectionString">
        /// Normally <see cref="ConnectionStringSettingName"/> from the environment. Blank is legal
        /// (local runs, AI not provisioned): the SDK registers and sends nothing.
        /// </param>
        public static IServiceCollection AddWorkerTelemetry(this IServiceCollection services, string? connectionString)
        {
            services.AddApplicationInsightsTelemetryWorkerService(options =>
            {
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.ConnectionString = connectionString;
                }
            });
            services.ConfigureFunctionsApplicationInsights();
            services.Configure<LoggerFilterOptions>(options =>
                ApplicationInsightsLogFilter.Apply(options, MinimumLevel));
            return services;
        }
    }
}
