using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System;
using System.Threading.Tasks;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;

namespace Warewolf.Execution.Lightweight.Logging
{
    /// <summary>
    /// <see cref="IExecutionLogger"/> implementation that persists every log entry
    /// as an <see cref="ElasticsearchLogDocument"/> in an Elasticsearch index.
    ///
    /// Inherits correlation-enrichment logic from <see cref="ExecutionLoggerBase"/>.
    /// Indexing is fire-and-forget: the calling thread is never blocked, and a
    /// failure to reach Elasticsearch is silently swallowed so it never disrupts
    /// workflow execution.
    /// </summary>
    public sealed class ElasticsearchExecutionLogger : ExecutionLoggerBase
    {
        readonly ElasticsearchClient _client;
        readonly string _indexName;

        /// <summary>Re-entrancy guard to prevent recursive logging when this logger fails.</summary>
        [ThreadStatic]
        static bool _isLoggingFailure;

        public ElasticsearchExecutionLogger(ElasticsearchLoggingOptions options,
                                            Dev2LogLevel minimumLevel = ExecutionLogLevel.Default)
            : base(minimumLevel)
        {
            ArgumentNullException.ThrowIfNull(options);

            _indexName = options.IndexName ?? throw new ArgumentException("IndexName must be set.", nameof(options));

            var uri = new Uri(options.Uri ?? throw new ArgumentException("Uri must be set.", nameof(options)));
            var settings = new ElasticsearchClientSettings(uri);

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
                settings = settings.Authentication(new ApiKey(options.ApiKey));
            else if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
                settings = settings.Authentication(new BasicAuthentication(options.Username, options.Password));

            // EnableDebugMode captures full HTTP request/response — memory and perf issue in production.
            if (options.EnableDebugMode)
                settings = settings.EnableDebugMode();

            _client = new ElasticsearchClient(settings);
        }

        public override void LogDebug(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "debug",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public override void LogDebug(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level        = "debug",
                Message      = message,
                ExecutionId  = executionId,
                ErrorMessage = exception?.Message,
                StackTrace   = exception?.ToString(),
            });
        }


        public override void LogInfo(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "info",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public override void LogInfo(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level        = "info",
                Message      = message,
                ExecutionId  = executionId,
                ErrorMessage = exception?.Message,
                StackTrace   = exception?.ToString(),
            });
        }

        public override void LogWarning(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "warn",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public override void LogWarning(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level        = "warn",
                Message      = message,
                ExecutionId  = executionId,
                ErrorMessage = exception?.Message,
                StackTrace   = exception?.ToString(),
            });
        }

        public override void LogError(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "error",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public override void LogError(string activityName, Exception ex, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level        = "error",
                Message      = $"[{activityName}] {ex?.Message}",
                ExecutionId  = executionId,
                ActivityName = activityName,
                ErrorMessage = ex?.Message,
                StackTrace   = ex?.ToString(),
            });
        }


        public override void LogFatal(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "fatal",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public override void LogFatal(string message, Exception exception, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level        = "fatal",
                Message      = message,
                ExecutionId  = executionId,
                ErrorMessage = exception?.Message,
                StackTrace   = exception?.ToString(),
            });
        }


        void IndexFireAndForget(ElasticsearchLogDocument doc)
        {
            if (_client is null)
            {
                LogFailureSafe("[ElasticsearchLogger] Client is null — skipping index.");
                return;
            }

            doc = EnrichWithCorrelation(doc);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _client
                        .IndexAsync(doc, idx => idx.Index(_indexName))
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    LogFailureSafe($"[ElasticsearchLogger] Exception indexing document: {e.GetType().Name} — {e.Message}");
                }
            });
        }

        /// <summary>
        /// Logs an Elasticsearch failure without re-entrancy. Uses Dev2Logger.Warn
        /// but guards against recursive calls back into this logger via CompositeExecutionLogger.
        /// </summary>
        static void LogFailureSafe(string message)
        {
            if (_isLoggingFailure) return;
            _isLoggingFailure = true;
            try
            {
                Dev2.Common.Dev2Logger.Warn(message, "ElasticsearchLogger");
            }
            finally
            {
                _isLoggingFailure = false;
            }
        }

        public override void LogError(Exception ex, string log)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level        = "error",
                Message      = log,
                ErrorMessage = ex?.Message,
                StackTrace   = ex?.ToString(),
            });
        }

        public override void LogInfo(string message)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            this.LogInfo(message, new Guid());
        }
    }
}

