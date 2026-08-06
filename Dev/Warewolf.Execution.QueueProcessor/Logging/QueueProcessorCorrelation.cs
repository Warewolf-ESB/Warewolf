/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

namespace Warewolf.Execution.QueueProcessor.Logging
{
    /// <summary>
    /// Ambient correlation for one message being processed. The container equivalent of the
    /// engine's <c>InstanceCorrelationContext</c>, but keyed on the worker's unit of work
    /// (a queue delivery) instead of an Azure Functions invocation.
    ///
    /// Set once per delivery by the message pump, read by
    /// <see cref="QueueProcessorLogSink"/> and by <c>Dev2Logger.CorrelationPrefixProvider</c>
    /// so legacy <c>Dev2Logger</c> call-sites inherit the same context.
    /// </summary>
    public sealed class QueueProcessorCorrelation
    {
        static readonly AsyncLocal<QueueProcessorCorrelation?> _current = new();

        /// <summary>Stable for the life of the replica; identifies which replica logged.</summary>
        public static string ReplicaId { get; } =
            Environment.GetEnvironmentVariable("CONTAINER_APP_REPLICA_NAME")
            ?? Environment.GetEnvironmentVariable("HOSTNAME")
            ?? Environment.MachineName;

        /// <summary>Container App name, when running in ACA.</summary>
        public static string AppName { get; } =
            Environment.GetEnvironmentVariable("CONTAINER_APP_NAME") ?? string.Empty;

        public static QueueProcessorCorrelation? Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public string ExecutionId { get; init; } = string.Empty;
        public string CustomTransactionId { get; init; } = string.Empty;
        public string QueueName { get; init; } = string.Empty;
        public ulong DeliveryTag { get; init; }

        /// <summary>Scopes a correlation context to the current async flow.</summary>
        public static IDisposable Begin(QueueProcessorCorrelation context)
        {
            var previous = Current;
            Current = context;
            return new Scope(previous);
        }

        /// <summary>
        /// Single-string correlation prefix, low structured-parameter count by design.
        /// Wired to <c>Dev2Logger.CorrelationPrefixProvider</c> at startup.
        /// </summary>
        public static string GetPrefix()
        {
            var ctx = Current;
            return ctx is null
                ? $"[Replica:{ReplicaId}]"
                : $"[Replica:{ReplicaId}] [Queue:{ctx.QueueName}] [Tag:{ctx.DeliveryTag}] [Txn:{ctx.CustomTransactionId}]";
        }

        sealed class Scope : IDisposable
        {
            readonly QueueProcessorCorrelation? _previous;
            bool _disposed;

            public Scope(QueueProcessorCorrelation? previous) => _previous = previous;

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                Current = _previous;
            }
        }
    }
}
