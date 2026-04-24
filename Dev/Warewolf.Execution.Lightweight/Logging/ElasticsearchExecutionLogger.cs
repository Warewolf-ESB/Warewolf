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
    /// Indexing is fire-and-forget: the calling thread is never blocked, and a
    /// failure to reach Elasticsearch is silently swallowed so it never disrupts
    /// workflow execution.
    /// </summary>
    public sealed class ElasticsearchExecutionLogger : IExecutionLogger
    {
        readonly ElasticsearchClient _client;
        readonly string _indexName;
        readonly Dev2LogLevel _minimumLevel;
 
        public ElasticsearchExecutionLogger(ElasticsearchLoggingOptions options,
                                            Dev2LogLevel minimumLevel = ExecutionLogLevel.Default)
        {
            ArgumentNullException.ThrowIfNull(options);
            _minimumLevel = minimumLevel;

             _indexName = options.IndexName ?? throw new ArgumentException("IndexName must be set.", nameof(options));

            var uri = new Uri(options.Uri ?? throw new ArgumentException("Uri must be set.", nameof(options)));
            var settings = new ElasticsearchClientSettings(uri);

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
                settings = settings.Authentication(new ApiKey(options.ApiKey));
            else if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
                settings = settings.Authentication(new BasicAuthentication(options.Username, options.Password));

            settings = settings.EnableDebugMode();

            _client = new ElasticsearchClient(settings);
        }

        bool ShouldLog(Dev2LogLevel level) => ExecutionLogLevel.ShouldLog(level, _minimumLevel);

        public void LogDebug(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.DEBUG)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "debug",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public void LogDebug(string message, Exception exception, Guid executionId)
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


        public void LogInfo(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "info",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public void LogInfo(string message, Exception exception, Guid executionId)
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

        public void LogWarning(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.WARN)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "warn",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public void LogWarning(string message, Exception exception, Guid executionId)
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

        public void LogError(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "error",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public void LogError(string activityName, Exception ex, Guid executionId)
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


        public void LogFatal(string message, Guid executionId)
        {
            if (!ShouldLog(Dev2LogLevel.FATAL)) return;
            IndexFireAndForget(new ElasticsearchLogDocument
            {
                Level       = "fatal",
                Message     = message,
                ExecutionId = executionId,
            });
        }

        public void LogFatal(string message, Exception exception, Guid executionId)
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
                Console.WriteLine("[ElasticsearchLogger] Client is null — skipping index.");
                return;
            }

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
                    Console.WriteLine($"[ElasticsearchLogger] Exception indexing document: {e.GetType().Name} — {e.Message}");
                }
            });
        }

        public void LogError(Exception ex, string log)
        {
            if (!ShouldLog(Dev2LogLevel.ERROR)) return;
            var exception = new Exception(log, ex);
            this.LogError("", exception, new Guid());
        }

        public void LogInfo(string message)
        {
            if (!ShouldLog(Dev2LogLevel.INFO)) return;
            this.LogInfo(message, new Guid());
        }
    }
}

