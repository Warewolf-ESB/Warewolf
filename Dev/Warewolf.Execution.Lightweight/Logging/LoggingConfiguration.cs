using System;
using System.IO;
using Dev2.Data.Interfaces.Enums;

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
        /// <summary>Whether the Application Insights / AzureExecutionLogger sink is enabled.</summary>
        public bool EnableApplicationInsights { get; init; }

        /// <summary>Whether the Elasticsearch sink is enabled.</summary>
        public bool EnableElasticsearch { get; init; }

        /// <summary>Minimum log level gate shared by all sinks.</summary>
        public LogLevel MinimumLevel { get; init; }

        /// <summary>Whether console output should be structured JSON (ECS-compatible).</summary>
        public bool StructuredJson { get; init; }

        /// <summary>Whether Elasticsearch HTTP debug mode is enabled (never in production).</summary>
        public bool ElasticDebugMode { get; init; }

        /// <summary>Whether the current environment is Development.</summary>
        public bool IsDevelopment { get; init; }

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
                EnableApplicationInsights = IsEnabled("ENABLEAPPLICATIONINSIGHTS")
                    || IsEnabled("ENABLECONSOLELOGGING"), // backward compat
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
