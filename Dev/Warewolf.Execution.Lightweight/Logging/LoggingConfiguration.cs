using System;
using System.IO;
using System.Text.Json;
using Dev2.Common;
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
    /// <para>
    /// WOLF-8516: the 6 boolean/string toggles below are merged into one <c>WAREWOLF_LOGGING_CONFIG</c>
    /// JSON app setting (previously 6 independent env vars) — see <see cref="RawFlags"/>. An
    /// operator changes several logging knobs together with one atomic
    /// <c>az functionapp config appsettings set</c> call instead of several. <c>EXECUTIONLOGLEVEL</c>
    /// (<see cref="ExecutionLogLevel"/>) and <c>ASPNETCORE_ENVIRONMENT</c> are unaffected — the former
    /// already has its own dedicated, differently-shaped resolution; the latter is the standard
    /// ASP.NET Core environment-name variable, not Warewolf-specific.
    /// </para>
    ///
    /// <para><b>Environment variables:</b></para>
    /// <list type="table">
    ///   <item><term>EXECUTIONLOGLEVEL</term><description>Minimum log level (default: Info)</description></item>
    ///   <item><term>WAREWOLF_LOGGING_CONFIG</term><description>JSON: console/appInsights/elasticsearch/performanceCounters/structuredLogs/elasticDebugMode (all default false, structuredLogs/elasticDebugMode further gated — see <see cref="RawFlags"/>)</description></item>
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

        /// <summary>
        /// Whether the AI <c>PerformanceCollectorModule</c> (process memory/CPU counters, feeding
        /// App Insights' <c>performanceCounters</c> table) is registered. Only takes effect when
        /// <see cref="RegisterApplicationInsightsSdk"/> is also <c>true</c>. Opt-in and off by
        /// default — deliberately not enabled on every deployment (extra collection overhead);
        /// see <c>pipeline-LOADTEST.yml</c>'s UAT deploy for the current opt-in caller.
        /// </summary>
        public bool EnablePerformanceCounters { get; init; }

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
        /// Reads all logging configuration from environment variables — the 6 toggles from
        /// <c>WAREWOLF_LOGGING_CONFIG</c> (see <see cref="RawFlags"/>), the rest from their own
        /// dedicated variables.
        /// </summary>
        public static LoggingConfiguration FromEnvironment()
        {
            var isDev = string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development", StringComparison.OrdinalIgnoreCase);

            var flags = RawFlags.FromEnvironment();

            return new LoggingConfiguration
            {
                EnableConsoleLogging = flags.Console,
                RegisterApplicationInsightsSdk = flags.AppInsights, // SDK + telemetry: explicit opt-in only
                EnableElasticsearch = flags.Elasticsearch,
                EnablePerformanceCounters = flags.PerformanceCounters,
                MinimumLevel = ExecutionLogLevel.Read(),
                StructuredJson = flags.StructuredLogs || !isDev,
                ElasticDebugMode = flags.ElasticDebugMode && isDev,
                IsDevelopment = isDev,
                ElasticsearchSettingsPath = Path.Combine(
                    AppContext.BaseDirectory, "Settings", "ElasticsearchLoggingSource.bite"),
            };
        }

        /// <summary>
        /// WOLF-8516: single JSON app setting replacing 6 individual logging toggle env vars
        /// (<c>ENABLECONSOLELOGGING</c>, <c>ENABLEAPPLICATIONINSIGHTS</c>,
        /// <c>ENABLEELASTICSEARCHLOGGING</c>, <c>ENABLEPERFORMANCECOUNTERS</c>,
        /// <c>STRUCTURED_LOGS</c>, <c>ELASTIC_DEBUG_MODE</c>). All fields default to <c>false</c>
        /// when absent — identical to the previous "env var not set" behaviour.
        /// <code>
        ///   WAREWOLF_LOGGING_CONFIG = {"console":true,"appInsights":false,"elasticsearch":false,"performanceCounters":false,"structuredLogs":true,"elasticDebugMode":false}
        /// </code>
        /// </summary>
        sealed class RawFlags
        {
            public const string EnvVar = "WAREWOLF_LOGGING_CONFIG";

            public bool Console { get; init; }
            public bool AppInsights { get; init; }
            public bool Elasticsearch { get; init; }
            public bool PerformanceCounters { get; init; }
            public bool StructuredLogs { get; init; }
            public bool ElasticDebugMode { get; init; }

            static readonly RawFlags AllFalse = new();

            public static RawFlags FromEnvironment()
            {
                var raw = Environment.GetEnvironmentVariable(EnvVar);
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return AllFalse;
                }

                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<RawFlags>(raw, options) ?? AllFalse;
                }
                catch (JsonException ex)
                {
                    Dev2Logger.Warn(
                        $"LoggingConfiguration failed to parse {EnvVar} — treating all flags as false " +
                        $"(safe default). ExceptionType={ex.GetType().Name}",
                        "LoggingConfiguration-FromEnvironment");
                    return AllFalse;
                }
            }
        }
    }
}
