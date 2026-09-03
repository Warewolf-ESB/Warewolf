/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Collections.Concurrent;
using Dev2.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Warewolf.Data;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Logging;
using Warewolf.Streams;

// Warewolf.Streams also declares an IConnection; alias to the broker one, exactly as
// Warewolf.Driver.RabbitMQ/RabbitConnection.cs:18 does for the same collision.
using IConnection = RabbitMQ.Client.IConnection;
using Headers = Warewolf.Data.Headers;

namespace Warewolf.Execution.QueueProcessor.Messaging
{
    /// <summary>Message pump abstraction so the host owns lifetime, not a client library.</summary>
    public interface IMessagePump : IAsyncDisposable
    {
        int InFlight { get; }
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops new deliveries, re-queues anything buffered but not started, and waits for
        /// in-flight work up to <paramref name="grace"/>. Returns the number of messages still
        /// in flight when the window elapsed (0 = clean drain).
        /// </summary>
        Task<int> DrainAsync(TimeSpan grace);
    }

    /// <summary>
    /// The replacement for <c>RabbitConnection.StartConsuming</c>
    /// (<c>Warewolf.Driver.RabbitMQ/RabbitConnection.cs:43-110</c>), rewritten on
    /// <b>RabbitMQ.Client 7.x</b> (decision #24). Behavioural parity with the old loop:
    /// <list type="bullet">
    ///   <item>manual ack (<c>autoAck: false</c>); ack ONLY on <see cref="ConsumerResult.Success"/>;</item>
    ///   <item>per-consumer QoS from the trigger's <c>Prefetch</c>;</item>
    ///   <item><c>QueueDeclare</c> with the trigger's durable/exclusive/autoDelete arguments;</item>
    ///   <item>a consumer-cancelled latch;</item>
    ///   <item>a periodic passive-declare watchdog.</item>
    /// </list>
    /// Deliberate improvements over the 5.1.2 loop: it is async end-to-end (no
    /// <c>resultTask.Wait()</c>), the watchdog reports a failure instead of throwing on a timer
    /// thread to kill the process, and shutdown is graceful (<see cref="DrainAsync"/>).
    /// </summary>
    public sealed class RabbitMqMessagePump : IMessagePump
    {
        const string ExecutionId = "QueueProcessor-Pump";
        static readonly TimeSpan WatchdogInterval = TimeSpan.FromMinutes(10);

        readonly ResolvedQueueConfiguration _config;
        readonly IConsumer _consumer;
        readonly int _maxConcurrency;
        readonly int _maxDeliveryAttempts;
        readonly IDeadLetterPublisher? _deadLetter;

        readonly SemaphoreSlim _throttle;
        readonly ConcurrentDictionary<ulong, byte> _inFlight = new();

        IConnection? _connection;
        IChannel? _channel;
        string? _consumerTag;
        Timer? _watchdog;

        volatile bool _draining;
        volatile bool _consumerCancelled;
        DateTime _consumerCancelledUtc = DateTime.MinValue;

        /// <summary>
        /// Opens the broker connection. Exists purely so the ack / nack / drain logic below can be
        /// tested against a mocked <see cref="IChannel"/> — everything the pump does after
        /// connecting goes through that interface, so this one delegate is the whole seam.
        /// Production always uses <see cref="CreateConnectionFactory"/>; nothing else may set it.
        /// </summary>
        readonly Func<CancellationToken, Task<IConnection>> _connect;

        public RabbitMqMessagePump(
            ResolvedQueueConfiguration config,
            IConsumer consumer,
            int maxConcurrency,
            int maxDeliveryAttempts = 2,
            IDeadLetterPublisher? deadLetter = null)
            : this(config, consumer, maxConcurrency, connect: null, maxDeliveryAttempts, deadLetter)
        {
        }

        internal RabbitMqMessagePump(
            ResolvedQueueConfiguration config,
            IConsumer consumer,
            int maxConcurrency,
            Func<CancellationToken, Task<IConnection>>? connect,
            int maxDeliveryAttempts = 2,
            IDeadLetterPublisher? deadLetter = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _maxConcurrency = Math.Max(1, maxConcurrency);
            _maxDeliveryAttempts = Math.Clamp(maxDeliveryAttempts, 1, 2);
            _deadLetter = deadLetter;
            _throttle = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);
            _connect = connect ?? (ct => CreateConnectionFactory().CreateConnectionAsync(ct));

            if (maxDeliveryAttempts > 2)
            {
                Dev2Logger.Warn(
                    $"MaxDeliveryAttempts={maxDeliveryAttempts} clamped to 2. Attempt counting uses the " +
                    "AMQP redelivered flag, which is a boolean and cannot express more than " +
                    "'first attempt' vs 'seen before'.", ExecutionId);
            }
        }

        public int InFlight => _inFlight.Count;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _connection = await _connect(cancellationToken).ConfigureAwait(false);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken)
                                        .ConfigureAwait(false);

            // Per-consumer prefetch, exactly as RabbitConfig.CreateChannel does
            // (global: false => per consumer, not per channel).
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _config.Prefetch, global: false,
                                         cancellationToken).ConfigureAwait(false);

            // NO QueueDeclare. A consumer must never assert the queue's arguments.
            //
            // Both on-prem consumers this replaces consume without declaring:
            //   * DsfConsumeRabbitMQActivity           - BasicQos -> BasicConsume, and the class has
            //                                            no IsDurable/IsExclusive/IsAutoDelete at all
            //   * RabbitConnection.StartConsuming      - BasicConsume directly (line 82); its only
            //     (Warewolf.Driver.RabbitMQ)             QueueDeclarePassive is a 10-minute liveness
            //                                            probe, mirrored by WatchdogTickAsync below
            //
            // An ACTIVE declare re-asserts durable/exclusive/autoDelete, and RabbitMQ rejects any
            // mismatch against the live queue with a channel-level 406:
            //   PRECONDITION_FAILED - inequivalent arg 'durable' for queue 'order-failure-queue'
            //   in vhost '/': received 'true' but current is 'false'
            // Durability is decided by whoever creates the queue - typically PublishRabbitMQActivity,
            // whose IsDurable is an unchecked-by-default designer checkbox - so from a consumer's
            // point of view it is arbitrary and none of its business. Consuming is unaffected by it:
            // durability governs survival of a broker restart, and message persistence is a separate
            // publisher-side flag.
            //
            // The queue must therefore pre-exist. If it does not, BasicConsumeAsync fails 404 and the
            // replica exits so the platform restarts it - the same contract as on-prem, where
            // DsfConsumeRabbitMQActivity raises RabbitQueueNotFound and the driver's watchdog throws.
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnReceivedAsync;
            consumer.UnregisteredAsync += OnUnregisteredAsync;

            try
            {
                _consumerTag = await _channel.BasicConsumeAsync(
                    queue: _config.QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                // Now that the pump no longer creates the queue, "not found" is a distinct and
                // actionable condition. Say so plainly, naming the queue - the raw AMQP text is
                // easy to mistake for a credentials or networking fault.
                throw new InvalidOperationException(
                    $"Queue '{_config.QueueName}' does not exist on the broker " +
                    $"({_config.Source.HostName}:{_config.Source.Port}, vhost '{_config.Source.VirtualHost}'). " +
                    "The worker consumes an existing queue and deliberately does not create one, " +
                    "matching the on-prem consumers. Create the queue (or publish to it once) and " +
                    "the replica will consume on its next start.", ex);
            }

            _watchdog = new Timer(_ => _ = WatchdogTickAsync(), null, WatchdogInterval, WatchdogInterval);

            Dev2Logger.Info(
                $"Consuming queue '{_config.QueueName}' (prefetch {_config.Prefetch}, " +
                $"maxConcurrency {_maxConcurrency}, consumerTag '{_consumerTag}')", ExecutionId);
        }

        async Task OnReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            // Once draining, refuse new work and hand it straight back so a surviving replica
            // can take it - the buffered-message requeue of plan §2.6.1 step 3.
            if (_draining)
            {
                await SafeNackAsync(eventArgs.DeliveryTag).ConfigureAwait(false);
                return;
            }

            await _throttle.WaitAsync().ConfigureAwait(false);
            _inFlight[eventArgs.DeliveryTag] = 0;

            var correlationId = eventArgs.BasicProperties?.CorrelationId ?? string.Empty;
            using var correlationScope = QueueProcessorCorrelation.Begin(new QueueProcessorCorrelation
            {
                QueueName = _config.QueueName,
                CustomTransactionId = correlationId,
                DeliveryTag = eventArgs.DeliveryTag,
            });

            try
            {
                var body = eventArgs.Body.ToArray();

                // Same header contract the on-prem loop populates
                // (RabbitConnection.cs:56-57) so the engine sees identical correlation.
                var headers = new Headers();
                headers["Warewolf-Custom-Transaction-Id"] = new[] { correlationId };

                // The AMQP redelivered flag is the ONLY signal that this message has been seen
                // before. The pump already uses it for attempt counting (HandleTransportFailureAsync);
                // forwarding it lets the engine skip its de-duplication lookup on the first attempt,
                // which is the overwhelming majority of deliveries. Boolean by protocol, so it can
                // only ever express 1 or 2 - the same ceiling documented on MaxDeliveryAttempts.
                headers["Warewolf-Delivery-Attempt"] =
                    new[] { eventArgs.Redelivered ? "2" : "1" };

                var result = await _consumer.Consume(body, headers).ConfigureAwait(false);

                if (result == ConsumerResult.Success)
                {
                    await _channel!.BasicAckAsync(eventArgs.DeliveryTag, multiple: false)
                                   .ConfigureAwait(false);
                }
                else
                {
                    await HandleTransportFailureAsync(eventArgs, body, "consumer returned Failed")
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(
                    $"Unhandled error processing delivery {eventArgs.DeliveryTag} on '{_config.QueueName}'",
                    ex, ExecutionId);

                // An escaped exception used to fall through and leave the delivery unacked, which
                // is the same permanent stall as the Failed path. Route it through the same policy.
                await HandleTransportFailureAsync(eventArgs, eventArgs.Body.ToArray(), $"exception: {ex.Message}")
                    .ConfigureAwait(false);
            }
            finally
            {
                _inFlight.TryRemove(eventArgs.DeliveryTag, out _);
                _throttle.Release();
            }
        }

        Task OnUnregisteredAsync(object sender, ConsumerEventArgs eventArgs)
        {
            if (!_consumerCancelled)
            {
                _consumerCancelled = true;
                _consumerCancelledUtc = DateTime.UtcNow;

                if (!_draining)
                {
                    Dev2Logger.Error(
                        $"Consumer for '{_config.QueueName}' was cancelled by the broker at " +
                        $"{_consumerCancelledUtc:O} (queue deleted?). The replica will exit so the " +
                        "platform restarts it.", ExecutionId);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Liveness probe surface: false once the broker has cancelled the consumer or the
        /// connection has dropped. The on-prem loop threw from a timer thread to crash the
        /// process; a container reports and exits cleanly instead.
        /// </summary>
        public bool IsHealthy =>
            !_consumerCancelled && (_connection?.IsOpen ?? false) && (_channel?.IsOpen ?? false);

        async Task WatchdogTickAsync()
        {
            if (_draining || _channel is null) return;

            try
            {
                if (_consumerCancelled)
                {
                    throw new InvalidOperationException(
                        $"Consumer cancelled at {_consumerCancelledUtc:O}.");
                }

                await _channel.QueueDeclarePassiveAsync(_config.QueueName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(
                    $"Watchdog failed for queue '{_config.QueueName}' - the replica is no longer able " +
                    "to consume and will be reported unhealthy.", ex, ExecutionId);
                _consumerCancelled = true;
            }
        }

        public async Task<int> DrainAsync(TimeSpan grace)
        {
            _draining = true;

            // 1. Stop new deliveries to THIS replica; the broker keeps feeding the survivors.
            if (_channel is not null && !string.IsNullOrEmpty(_consumerTag))
            {
                try
                {
                    await _channel.BasicCancelAsync(_consumerTag!, noWait: false).ConfigureAwait(false);
                    Dev2Logger.Info(
                        $"Cancelled consumer '{_consumerTag}' on '{_config.QueueName}'; draining " +
                        $"{InFlight} in-flight message(s) with a {grace.TotalSeconds:F0}s budget.",
                        ExecutionId);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Warn($"BasicCancel failed during drain: {ex.Message}", ExecutionId);
                }
            }

            // 2. Wait for in-flight work. Buffered-but-unstarted deliveries are nacked on
            //    arrival by OnReceivedAsync while _draining is true.
            var deadline = DateTime.UtcNow + grace;
            while (InFlight > 0 && DateTime.UtcNow < deadline)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }

            var stranded = InFlight;
            if (stranded > 0)
            {
                // The honest failure mode of at-least-once: these are deliberately left unacked,
                // so they are redelivered and re-executed. Make it visible, never silent.
                Dev2Logger.Warn(
                    $"Drain window of {grace.TotalSeconds:F0}s elapsed with {stranded} message(s) still " +
                    $"in flight on '{_config.QueueName}'. They will NOT be acked and will be redelivered, " +
                    "so those workflows may run twice. Increase Worker:ShutdownGraceSeconds or lower " +
                    "Engine:TimeoutSeconds.", ExecutionId);
            }
            else
            {
                Dev2Logger.Info($"Drain of '{_config.QueueName}' completed cleanly.", ExecutionId);
            }

            return stranded;
        }

        async Task SafeNackAsync(ulong deliveryTag, bool requeue = true)
        {
            try
            {
                if (_channel is not null)
                {
                    await _channel.BasicNackAsync(deliveryTag, multiple: false, requeue: requeue)
                                  .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // Not fatal: an un-nacked message is still unacked, so the broker redelivers it
                // when the connection closes moments later.
                Dev2Logger.Debug($"Nack during drain failed for {deliveryTag}: {ex.Message}", ExecutionId);
            }
        }

        /// <summary>
        /// Resolves a TRANSPORT-level failure - the engine could not be reached or did not answer,
        /// as distinct from a business failure, which the forwarder dead-letters and reports as
        /// Success. The delivery must always end acked, nacked or dead-lettered here: leaving it
        /// unacked is what deadlocked the consumer, because with <c>Prefetch=1</c> the broker
        /// sends nothing further while one message is outstanding.
        ///
        /// <para>Attempts are counted by the AMQP <c>redelivered</c> flag, so the ceiling is two.
        /// See <see cref="QueueProcessorOptions.MaxDeliveryAttempts"/>.</para>
        /// </summary>
        async Task HandleTransportFailureAsync(BasicDeliverEventArgs eventArgs, byte[] body, string reason)
        {
            var attemptsExhausted = _maxDeliveryAttempts <= 1 || eventArgs.Redelivered;

            if (!attemptsExhausted)
            {
                Dev2Logger.Warn(
                    $"Delivery {eventArgs.DeliveryTag} on '{_config.QueueName}' failed ({reason}); " +
                    "requeueing for one retry.", ExecutionId);
                await SafeNackAsync(eventArgs.DeliveryTag, requeue: true).ConfigureAwait(false);
                return;
            }

            if (_deadLetter is not null && _config.HasDeadLetter)
            {
                try
                {
                    var diagnostics = new Dictionary<string, object?>
                    {
                        ["x-warewolf-queue"]           = _config.QueueName,
                        ["x-warewolf-workflow"]        = _config.WorkflowPath,
                        ["x-warewolf-failure-reason"]  = reason,
                        ["x-warewolf-redelivered"]     = eventArgs.Redelivered,
                        ["x-warewolf-dead-lettered-utc"] = DateTime.UtcNow.ToString("O"),
                    };

                    await _deadLetter.PublishAsync(body, diagnostics, CancellationToken.None)
                                     .ConfigureAwait(false);

                    // Ack ONLY after the dead-letter publish succeeded, so the message is never
                    // acknowledged until a durable copy exists somewhere.
                    if (_channel is not null)
                    {
                        await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false)
                                      .ConfigureAwait(false);
                    }

                    Dev2Logger.Error(
                        $"Delivery {eventArgs.DeliveryTag} on '{_config.QueueName}' dead-lettered after " +
                        $"{_maxDeliveryAttempts} attempt(s) ({reason}).", ExecutionId);
                    return;
                }
                catch (Exception ex)
                {
                    // Same contract as EngineForwarder: if dead-lettering fails we must NOT ack, or
                    // the message is lost. Requeue so another replica can try.
                    Dev2Logger.Fatal(
                        $"Dead-letter publish failed for delivery {eventArgs.DeliveryTag} on " +
                        $"'{_config.QueueName}'; requeueing rather than losing it.", ex, ExecutionId);
                    await SafeNackAsync(eventArgs.DeliveryTag, requeue: true).ConfigureAwait(false);
                    return;
                }
            }

            // No dead-letter queue is configured for this trigger, so there is nowhere to put it.
            // Discarding is the lesser evil: requeueing forever would stall this consumer exactly
            // as before, and the message has already had every attempt the redelivered flag allows.
            Dev2Logger.Error(
                $"Delivery {eventArgs.DeliveryTag} on '{_config.QueueName}' failed ({reason}) and the " +
                "trigger declares NO dead-letter queue - discarding it to keep the consumer moving. " +
                "Configure DeadLetterQueue on the trigger to retain failures.", ExecutionId);
            await SafeNackAsync(eventArgs.DeliveryTag, requeue: false).ConfigureAwait(false);
        }

        ConnectionFactory CreateConnectionFactory()
        {
            var source = _config.Source;
            var factory = new ConnectionFactory
            {
                HostName = source.HostName,
                Port = source.Port,
                UserName = source.UserName,
                Password = source.Password,
                VirtualHost = source.VirtualHost,

                // Recovery is a real improvement over the on-prem loop, which relied on the
                // process dying and the supervisor restarting it.
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),

                // Attributable broker-side connections, per replica.
                ClientProvidedName =
                    $"wwqp/{QueueProcessorCorrelation.AppName}/{QueueProcessorCorrelation.ReplicaId}",

                // 1 keeps per-replica dispatch serial, reproducing the measured on-prem
                // behaviour; >1 is what makes MaxConcurrency real on 7.x.
                ConsumerDispatchConcurrency = (ushort)_maxConcurrency,
            };

            if (source.UseSsl)
            {
                factory.Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = source.HostName,
                };
            }

            return factory;
        }

        public async ValueTask DisposeAsync()
        {
            if (_watchdog is not null)
            {
                await _watchdog.DisposeAsync().ConfigureAwait(false);
                _watchdog = null;
            }

            if (_channel is not null)
            {
                try { await _channel.CloseAsync().ConfigureAwait(false); } catch { /* closing anyway */ }
                await _channel.DisposeAsync().ConfigureAwait(false);
                _channel = null;
            }

            if (_connection is not null)
            {
                try { await _connection.CloseAsync().ConfigureAwait(false); } catch { /* closing anyway */ }
                await _connection.DisposeAsync().ConfigureAwait(false);
                _connection = null;
            }

            _throttle.Dispose();
        }
    }
}
