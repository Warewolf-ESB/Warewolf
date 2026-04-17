using System;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// Document written to the Elasticsearch index for every log entry.
    /// Field names follow Elastic Common Schema (ECS) conventions so the index
    /// works in Kibana / Discover out-of-the-box without extra mappings.
    /// </summary>
    public sealed class ElasticsearchLogDocument
    {
        /// <summary>
        /// UTC timestamp of the entry — ECS <c>@timestamp</c>.
        /// </summary>
        [JsonPropertyName("@timestamp")]
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>ECS <c>log.level</c>: <c>error</c>, <c>warn</c>, or <c>info</c>.</summary>
        [JsonPropertyName("log.level")]
        public string Level { get; init; } = "info";

        /// <summary>Human-readable log message.</summary>
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        /// <summary>Workflow execution correlation ID.</summary>
        [JsonPropertyName("execution.id")]
        public Guid ExecutionId { get; init; }

        /// <summary>
        /// Name of the activity or service step, e.g. <c>MssqlSqlExecution</c>.
        /// </summary>
        [JsonPropertyName("activity.name")]
        public string? ActivityName { get; init; }

        /// <summary>Short exception message — ECS <c>error.message</c> (errors only).</summary>
        [JsonPropertyName("error.message")]
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// Full CLR stack trace including inner exceptions — ECS <c>error.stack_trace</c>
        /// (errors only).
        /// </summary>
        [JsonPropertyName("error.stack_trace")]
        public string? StackTrace { get; init; }
    }
}
