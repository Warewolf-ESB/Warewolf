/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.ServiceBusWorker
{
    /// <summary>
    /// Worker-side Application Insights registration, extracted from <c>Program.cs</c> so it can be
    /// unit tested (top-level statements cannot be).
    ///
    /// <para>Until this existed the worker registered no AI SDK at all (the package was referenced,
    /// never wired), so its calls to the engine never appeared as <c>dependencies</c> and its own log
    /// lines had no worker-side path to App Insights.</para>
    ///
    /// <para><b>Connection string: the STANDARD name.</b> Unlike the Execution Engine, which reads the
    /// deliberately non-standard <c>WAREWOLF_APPINSIGHTS_CONNECTION_STRING</c> to keep the Functions
    /// host's own AI pipeline dormant, this worker uses <see cref="ConnectionStringSettingName"/>.
    /// The host pipeline is what writes the <c>requests</c>/<c>exceptions</c> tables (one row per
    /// queue message), and the engine's reason for avoiding it - the host flattening captured worker
    /// stdout to Information - does not apply: no console provider is attached here.
    /// Deploy-WwExecutionServiceBusWorker.ps1 writes this name.</para>
    ///
    /// <para><b>Level:</b> the AI SDK's default provider rule drops everything below Warning; it is
    /// replaced with <see cref="MinimumLevel"/>, matching host.json's <c>logLevel.default</c>.</para>
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
