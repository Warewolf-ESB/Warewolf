using System;
using System.IO;
using Dev2.Data.Interfaces.Enums;
using MelLogLevel = Microsoft.Extensions.Logging.LogLevel;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Central logging configuration record — single source of truth for all
    /// logging-related settings. All loggers read from this record rather than
    /// calling <see cref="Environment.GetEnvironmentVariable"/> directly.
    ///
    /// <para><b>Environment variables:</b></para>
    /// <list type="table">
    ///   <item><term>EXECUTIONLOGLEVEL</term><description>Minimum log level (default: Info)</description></item>
    ///   <item><term>ENABLEAPPLICATIONINSIGHTS</term><description>Add AzureExecutionLogger to composite (default: false)</description></item>
    ///   <item><term>ENABLEELASTICSEARCHLOGGING</term><description>Add ElasticsearchExecutionLogger to composite (default: false)</description></item>
    ///   <item><term>STRUCTURED_LOGS</term><description>Console output as JSON (default: true in Azure, false locally)</description></item>
    ///   <item><term>ELASTIC_DEBUG_MODE</term><description>Enable Elastic HTTP debug tracing (default: false)</description></item>
    ///   <item><term>ASPNETCORE_ENVIRONMENT</term><description>Selects logging profile (Development vs Production)</description></item>
    /// </list>
    /// </summary>
    public sealed record LoggingConfiguration
    {
        /// <summary>
        /// The single authoritative Application Insights switch, driven by
        /// <c>ENABLEAPPLICATIONINSIGHTS</c>. When <c>true</c> the worker-side AI SDK is
        /// registered (<c>AzureExecutionLogger</c> + <c>ConfigureFunctionsApplicationInsights</c>
        /// + the targeted <see cref="Microsoft.Extensions.Logging.LoggerFilterOptions"/> rule)
        /// and supplied the connection string from <c>WAREWOLF_APPINSIGHTS_CONNECTION_STRING</c>.
        ///
        /// <para>The worker SDK is the <b>only</b> path to App Insights: the standard
        /// <c>APPLICATIONINSIGHTS_CONNECTION_STRING</c> setting (which would auto-enable the
        /// Functions host's own AI pipeline) is deliberately never used, so registering the AI
        /// SDK — and its telemetry ingestion / billing — is an explicit opt-in only.</para>
        /// </summary>
        public bool RegisterApplicationInsightsSdk { get; init; }

        /// <summary>Whether the Elasticsearch sink is enabled.</summary>
        public bool EnableElasticsearch { get; init; }
        public bool EnableConsoleLogging { get; init; }

        /// <summary>Minimum log level gate shared by all sinks.</summary>
        public Dev2LogLevel MinimumLevel { get; init; }

        /// <summary>Whether console output should be structured JSON (ECS-compatible).</summary>
        public bool StructuredJson { get; init; }

        /// <summary>Whether Elasticsearch HTTP debug mode is enabled (never in production).</summary>
        public bool ElasticDebugMode { get; init; }

        /// <summary>Whether the current environment is Development.</summary>
        public bool IsDevelopment { get; init; }

        /// <summary>
        /// Maps the Dev2 <see cref="MinimumLevel"/> to its MEL equivalent for use in
        /// <see cref="Microsoft.Extensions.Logging.LoggerFilterOptions"/> and the
        /// bootstrap <see cref="Microsoft.Extensions.Logging.ILoggerFactory"/>.
        ///
        /// Dev2 convention: higher numeric value = more verbose (TRACE=6, OFF=0).
        /// MEL convention:  lower  numeric value = more verbose (Trace=0, None=6).
        /// </summary>
        public MelLogLevel MelMinimumLevel => MinimumLevel switch
        {
            Dev2LogLevel.TRACE => MelLogLevel.Trace,
            Dev2LogLevel.DEBUG => MelLogLevel.Debug,
            Dev2LogLevel.INFO  => MelLogLevel.Information,
            Dev2LogLevel.WARN  => MelLogLevel.Warning,
            Dev2LogLevel.ERROR => MelLogLevel.Error,
            Dev2LogLevel.FATAL => MelLogLevel.Critical,
            Dev2LogLevel.OFF   => MelLogLevel.None,
            _                  => MelLogLevel.Information,
        };

        /// <summary>Absolute path to the Elasticsearch <c>.bite</c> config file.</summary>
        public string ElasticsearchSettingsPath { get; init; } = string.Empty;

        /// <summary>
        /// Reads all logging configuration from environment variables.
        /// </summary>
        public static LoggingConfiguration FromEnvironment()
        {
            var isDev = string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development", StringComparison.OrdinalIgnoreCase);

            return new LoggingConfiguration
            {
                EnableConsoleLogging = IsEnabled("ENABLECONSOLELOGGING"),
                RegisterApplicationInsightsSdk = IsEnabled("ENABLEAPPLICATIONINSIGHTS"), // SDK + telemetry: explicit opt-in only
                EnableElasticsearch = IsEnabled("ENABLEELASTICSEARCHLOGGING"),
                MinimumLevel = ExecutionLogLevel.Read(),
                StructuredJson = IsEnabled("STRUCTURED_LOGS") || !isDev,
                ElasticDebugMode = IsEnabled("ELASTIC_DEBUG_MODE") && isDev,
                IsDevelopment = isDev,
                ElasticsearchSettingsPath = Path.Combine(
                    AppContext.BaseDirectory, "Settings", "ElasticsearchLoggingSource.bite"),
            };
        }

        static bool IsEnabled(string key) =>
            string.Equals(Environment.GetEnvironmentVariable(key), "true", StringComparison.OrdinalIgnoreCase);
    }
}
