/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Text;
using Dev2.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Warewolf.Execution.QueueProcessor.Configuration;

namespace Warewolf.Execution.QueueProcessor.Messaging
{
    public interface IDeadLetterPublisher : IAsyncDisposable
    {
        Task PublishAsync(byte[] body, IReadOnlyDictionary<string, object?> diagnostics, CancellationToken ct);
    }

    /// <summary>
    /// Publishes business failures to the trigger's dead-letter queue — the behaviour
    /// <c>WarewolfWebRequestForwarder</c> provides on-prem
    /// (<c>WarewolfWebRequestForwarder.cs:58-68</c>) via <c>Program.DeadLetterPublisher</c>.
    ///
    /// <para>One deliberate improvement: the connection and channel are <b>long-lived</b>.
    /// The on-prem publisher opened a fresh connection and channel for every single failure
    /// (<c>QueueWorker/Program.cs:273-279</c>), which is expensive and, under a burst of
    /// failures, hammers the broker.</para>
    ///
    /// <para>Payload parity: the <b>mapped</b> post body is published, not the raw frame,
    /// matching <c>WarewolfWebRequestForwarder.cs:64</c>. Diagnostic headers (status code,
    /// correlation, attempt) are added so an operator can triage without correlating logs.</para>
    /// </summary>
    public sealed class RabbitMqDeadLetterPublisher : IDeadLetterPublisher
    {
        const string ExecutionId = "QueueProcessor-DeadLetter";

        readonly ResolvedQueueConfiguration _config;
        readonly SemaphoreSlim _gate = new(1, 1);

        IConnection? _connection;
        IChannel? _channel;

        public RabbitMqDeadLetterPublisher(ResolvedQueueConfiguration config)
            => _config = config ?? throw new ArgumentNullException(nameof(config));

        public async Task PublishAsync(
            byte[] body, IReadOnlyDictionary<string, object?> diagnostics, CancellationToken ct)
        {
            if (!_config.HasDeadLetter)
            {
                Dev2Logger.Warn(
                    "Engine reported a business failure but the trigger defines no dead-letter queue; " +
                    "the message will be acked and its payload is only in the logs.", ExecutionId);
                return;
            }

            var channel = await EnsureChannelAsync(ct).ConfigureAwait(false);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = diagnostics.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value is string s ? (object?)Encoding.UTF8.GetBytes(s) : kvp.Value),
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _config.DeadLetterQueueName!,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: ct).ConfigureAwait(false);

            Dev2Logger.Info(
                $"Dead-lettered {body.Length} byte(s) to '{_config.DeadLetterQueueName}'.", ExecutionId);
        }

        async Task<IChannel> EnsureChannelAsync(CancellationToken ct)
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            await _gate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_channel is { IsOpen: true })
                {
                    return _channel;
                }

                await DisposeChannelAsync().ConfigureAwait(false);

                var source = _config.DeadLetterSource;
                var factory = new ConnectionFactory
                {
                    HostName = source.HostName,
                    Port = source.Port,
                    UserName = source.UserName,
                    Password = source.Password,
                    VirtualHost = source.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    ClientProvidedName =
                        $"wwqp-dlq/{QueueProcessorCorrelationName()}",
                };

                if (source.UseSsl)
                {
                    factory.Ssl = new SslOption { Enabled = true, ServerName = source.HostName };
                }

                _connection = await factory.CreateConnectionAsync(ct).ConfigureAwait(false);
                _channel = await _connection.CreateChannelAsync(cancellationToken: ct).ConfigureAwait(false);

                // PASSIVE FIRST, then create only if absent.
                //
                // The previous code declared ACTIVELY with the trigger's DeadLetterOptions and
                // claimed "the arguments match whatever already exists" - an assumption that does
                // not hold. Whoever creates a queue fixes its arguments, and for these queues that
                // is usually PublishRabbitMQActivity, whose IsDurable is an unchecked-by-default
                // designer checkbox. Re-asserting a different value is a channel-level 406:
                //   PRECONDITION_FAILED - inequivalent arg 'durable' for queue '<dlq>'
                // which would break the FAILURE path only - the worst place to find a bug, because
                // the happy path keeps working and messages silently stop being dead-lettered.
                //
                // A passive declare asserts existence WITHOUT asserting arguments, so an existing
                // DLQ is used exactly as it is. The trigger's DeadLetterDurable then applies only
                // where it legitimately can: creating a queue that is not there yet.
                try
                {
                    await _channel.QueueDeclarePassiveAsync(_config.DeadLetterQueueName!, ct)
                                  .ConfigureAwait(false);
                }
                catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
                {
                    // A failed passive declare CLOSES the channel, and AlreadyClosedException derives
                    // from OperationInterruptedException - so the active declare must go on a FRESH
                    // channel. PublishRabbitMQActivity reuses the closed one and would throw
                    // AlreadyClosedException instead of creating the queue; do not repeat that.
                    Dev2Logger.Info(
                        $"Dead-letter queue '{_config.DeadLetterQueueName}' does not exist - creating it " +
                        $"with durable={_config.DeadLetterDurable} from the trigger's DeadLetterOptions.",
                        ExecutionId);

                    await DisposeChannelAsync().ConfigureAwait(false);
                    // _connection was assigned above; the compiler's flow analysis cannot see that
                    // through the try/catch.
                    _channel = await _connection!.CreateChannelAsync(cancellationToken: ct).ConfigureAwait(false);

                    await _channel.QueueDeclareAsync(
                        queue: _config.DeadLetterQueueName!,
                        durable: _config.DeadLetterDurable,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        passive: false,
                        noWait: false,
                        cancellationToken: ct).ConfigureAwait(false);
                }

                return _channel;
            }
            finally
            {
                _gate.Release();
            }
        }

        static string QueueProcessorCorrelationName()
            => Logging.QueueProcessorCorrelation.ReplicaId;

        async Task DisposeChannelAsync()
        {
            if (_channel is not null)
            {
                try { await _channel.DisposeAsync().ConfigureAwait(false); } catch { /* replacing */ }
                _channel = null;
            }

            if (_connection is not null)
            {
                try { await _connection.DisposeAsync().ConfigureAwait(false); } catch { /* replacing */ }
                _connection = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeChannelAsync().ConfigureAwait(false);
            _gate.Dispose();
        }
    }
}
