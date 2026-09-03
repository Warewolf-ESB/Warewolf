/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Warewolf.Data;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Messaging;
using Warewolf.Streams;
using IConnection = RabbitMQ.Client.IConnection;
using Headers = Warewolf.Data.Headers;

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// What the pump does when the engine call fails at TRANSPORT level - a timeout or a socket
    /// error, as opposed to a non-2xx response, which <c>EngineForwarder</c> dead-letters and
    /// reports as Success.
    ///
    /// <para><b>The defect these cover.</b> The pump used to leave a Failed delivery unacked,
    /// on the reasoning that "the broker redelivers it when the channel drops". The channel does
    /// not drop, and with <c>Prefetch=1</c> the broker will not deliver anything else while one
    /// message is outstanding - so a single engine timeout stalled the consumer forever. Measured
    /// live 2026-08-11: 34 messages stranded for 20+ minutes behind one unacked message, with a
    /// healthy replica and an attached consumer. KEDA cannot break the deadlock either, because a
    /// non-empty queue keeps the replica alive and nothing forces the restart that would requeue
    /// the message.</para>
    ///
    /// <para>The invariant every test here defends: a delivery must ALWAYS end acked, nacked or
    /// dead-lettered. Never simply abandoned.</para>
    ///
    /// <para>Attempts are counted by the AMQP <c>redelivered</c> flag, which is a boolean, so the
    /// ceiling is two attempts - see <see cref="QueueProcessorOptions.MaxDeliveryAttempts"/>.</para>
    /// </summary>
    [TestClass]
    public class MessagePumpFailureHandlingTests
    {
        const string ConsumerTag = "amq.ctag-failure-test";

        Mock<IChannel> _channel = null!;
        Mock<IConnection> _connection = null!;
        Mock<IConsumer> _consumer = null!;
        FakeDeadLetterPublisher _deadLetter = null!;
        IAsyncBasicConsumer _registered = null!;

        [TestInitialize]
        public void Setup()
        {
            _channel = new Mock<IChannel>(MockBehavior.Loose);
            _connection = new Mock<IConnection>(MockBehavior.Loose);
            _consumer = new Mock<IConsumer>(MockBehavior.Loose);
            _deadLetter = new FakeDeadLetterPublisher();

            _connection
                .Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_channel.Object);

            _channel
                .Setup(c => c.BasicConsumeAsync(
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<IDictionary<string, object?>?>(),
                    It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()))
                .Callback((string _, bool _, string _, bool _, bool _,
                           IDictionary<string, object?>? _, IAsyncBasicConsumer consumer, CancellationToken _) =>
                          _registered = consumer)
                .ReturnsAsync(ConsumerTag);
        }

        /// <summary>Records what was dead-lettered, and can be told to fail on demand.</summary>
        sealed class FakeDeadLetterPublisher : IDeadLetterPublisher
        {
            public List<byte[]> Published { get; } = new();
            public List<IReadOnlyDictionary<string, object?>> Diagnostics { get; } = new();
            public Exception? ThrowOnPublish { get; set; }

            public Task PublishAsync(byte[] body, IReadOnlyDictionary<string, object?> diagnostics, CancellationToken ct)
            {
                if (ThrowOnPublish is not null) throw ThrowOnPublish;
                Published.Add(body);
                Diagnostics.Add(diagnostics);
                return Task.CompletedTask;
            }

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }

        static ResolvedQueueConfiguration Config(string? deadLetterQueue = "pump-queue-errors")
        {
            var trigger = new TriggerDefinition
            {
                TriggerId = Guid.NewGuid(),
                Name = "FailureTrigger",
                QueueName = "pump-queue",
                WorkflowName = "folder\\Wf",
                Prefetch = "1",
                Concurrency = 1,
                DeadLetterQueue = deadLetterQueue,
                Options = new List<TriggerOption> { new() { Name = "Durable", Value = true } },
            };

            var source = new RabbitMqSourceOptions
            {
                SourceId = Guid.NewGuid(),
                HostName = "localhost",
                Port = 5672,
                UserName = "u",
                Password = "p",
                VirtualHost = "/",
            };

            return new ResolvedQueueConfiguration
            {
                Trigger = trigger, Source = source, DeadLetterSource = source,
            };
        }

        RabbitMqMessagePump CreatePump(
            ResolvedQueueConfiguration? config = null,
            int maxDeliveryAttempts = 2,
            bool withDeadLetter = true) =>
            new(config ?? Config(),
                _consumer.Object,
                maxConcurrency: 1,
                connect: _ => Task.FromResult(_connection.Object),
                maxDeliveryAttempts: maxDeliveryAttempts,
                deadLetter: withDeadLetter ? _deadLetter : null);

        async Task DeliverAsync(ulong deliveryTag, bool redelivered = false, string body = "hello",
                                string? correlationId = "txn-1")
        {
            await _registered.HandleBasicDeliverAsync(
                ConsumerTag, deliveryTag, redelivered, exchange: string.Empty,
                routingKey: "pump-queue", properties: new BasicProperties { CorrelationId = correlationId },
                body: new ReadOnlyMemory<byte>(System.Text.Encoding.UTF8.GetBytes(body)));
        }

        void ConsumerFails() =>
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ReturnsAsync(ConsumerResult.Failed);

        // ── The anti-deadlock invariant ──────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task FirstFailure_IsNackedWithRequeue_NotLeftUnacked()
        {
            // THE REGRESSION GUARD. Leaving this delivery unacked is what stalled the consumer:
            // with Prefetch=1 the broker sends nothing further until it is resolved.
            ConsumerFails();
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(7);

            _channel.Verify(c => c.BasicNackAsync(7, false, true, It.IsAny<CancellationToken>()), Times.Once,
                "a first transport failure must be requeued so the consumer keeps flowing");
            _channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(),
                                                 It.IsAny<CancellationToken>()), Times.Never,
                "acking a failure would silently drop the message");
            Assert.AreEqual(0, _deadLetter.Published.Count, "a first attempt must not be dead-lettered");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ConsumerThrows_IsNackedWithRequeue_NotLeftUnacked()
        {
            // An escaped exception used to fall through the catch and abandon the delivery, which
            // is the same permanent stall by a different route.
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ThrowsAsync(new InvalidOperationException("boom"));
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(9);

            _channel.Verify(c => c.BasicNackAsync(9, false, true, It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(0, pump.InFlight, "the concurrency slot must still be released");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task AfterAFailure_TheNextDeliveryIsStillProcessed()
        {
            // The behavioural symptom of the deadlock, expressed without a broker: the pump must
            // remain willing to take more work after resolving a failure.
            _consumer.SetupSequence(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ReturnsAsync(ConsumerResult.Failed)
                     .ReturnsAsync(ConsumerResult.Success);

            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(1);
            await DeliverAsync(2);

            _channel.Verify(c => c.BasicNackAsync(1, false, true, It.IsAny<CancellationToken>()), Times.Once);
            _channel.Verify(c => c.BasicAckAsync(2, false, It.IsAny<CancellationToken>()), Times.Once,
                "the delivery after a failure must be processed and acked");
        }

        // ── Attempt exhaustion ───────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task RedeliveredFailure_IsDeadLetteredAndThenAcked()
        {
            ConsumerFails();
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(11, redelivered: true);

            Assert.AreEqual(1, _deadLetter.Published.Count, "the second attempt must be dead-lettered");
            CollectionAssert.AreEqual(System.Text.Encoding.UTF8.GetBytes("hello"), _deadLetter.Published[0],
                "the original body must be preserved");
            _channel.Verify(c => c.BasicAckAsync(11, false, It.IsAny<CancellationToken>()), Times.Once,
                "ack only AFTER a durable copy exists in the dead-letter queue");
            _channel.Verify(c => c.BasicNackAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(),
                                                  It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task MaxAttemptsOne_DeadLettersOnTheVeryFirstFailure()
        {
            ConsumerFails();
            var pump = CreatePump(maxDeliveryAttempts: 1);
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(3, redelivered: false);

            Assert.AreEqual(1, _deadLetter.Published.Count,
                "MaxDeliveryAttempts=1 means never retry");
            _channel.Verify(c => c.BasicAckAsync(3, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DeadLetterDiagnostics_RecordTheReasonAndRedeliveredFlag()
        {
            ConsumerFails();
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(12, redelivered: true);

            var diag = _deadLetter.Diagnostics.Single();
            Assert.AreEqual("pump-queue", diag["x-warewolf-queue"]);
            Assert.AreEqual(true, diag["x-warewolf-redelivered"]);
            StringAssert.Contains(diag["x-warewolf-failure-reason"]?.ToString() ?? string.Empty, "Failed",
                "an operator must be able to tell a transport failure from a business failure");
        }


        // ── Transaction-id attribution on a TRANSPORT-failure dead-letter (2026-09-03) ──

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task TransportFailureDeadLetter_CarriesTheTransactionId()
        {
            // Until 2026-09-03 this path wrote FIVE diagnostic fields - queue, workflow,
            // failure-reason, redelivered, dead-lettered-utc - and no transaction id in any form.
            //
            // That makes a transport-failure dead-letter impossible to tie back to what was
            // published, and it is the harder case to lose: a transport failure means the engine
            // never confirmed anything, so the dead-letter is the ONLY record that the delivery
            // happened at all. (EngineForwarder's BUSINESS-failure dead-letter has always carried
            // the id, which is why the 10 000-message run's ten dead-letters were still
            // identifiable from the worker log.)
            ConsumerFails();
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(21, redelivered: true, correlationId: "orders-S-5583-88023f");

            var diag = _deadLetter.Diagnostics.Single();
            Assert.IsTrue(diag.ContainsKey(RabbitMqDeadLetterPublisher.TransactionIdHeader),
                "a transport-failure dead-letter must carry the transaction id, or it cannot be " +
                "reconciled against what was published");
            Assert.AreEqual("orders-S-5583-88023f",
                diag[RabbitMqDeadLetterPublisher.TransactionIdHeader],
                "the id must be the publisher-assigned CorrelationId from the original delivery");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task TransportFailureDeadLetter_NoCorrelationId_OmitsTheKeyEntirely()
        {
            // Omitted, never written as blank. RabbitMqDeadLetterPublisher promotes this key onto
            // BasicProperties.CorrelationId; an empty value would be promoted as an empty string and
            // defeat the header-then-CorrelationId fallback in every reader, which stops at a
            // present-but-useless value rather than falling through.
            ConsumerFails();
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(22, redelivered: true, correlationId: null);

            var diag = _deadLetter.Diagnostics.Single();
            Assert.IsFalse(diag.ContainsKey(RabbitMqDeadLetterPublisher.TransactionIdHeader),
                "an absent CorrelationId must leave the key out, not write an empty string");

            // The rest of the diagnostics must still be intact - omitting the id must not
            // accidentally short-circuit the other five fields an operator triages from.
            Assert.AreEqual("pump-queue", diag["x-warewolf-queue"]);
            Assert.AreEqual(true, diag["x-warewolf-redelivered"]);
        }

        // ── Failure of the failure path ──────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task DeadLetterPublishFails_MessageIsRequeuedAndNeverAcked()
        {
            // Same contract as EngineForwarder: if dead-lettering fails we must not ack, or the
            // message is lost outright.
            ConsumerFails();
            _deadLetter.ThrowOnPublish = new InvalidOperationException("broker down");
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(13, redelivered: true);

            _channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(),
                                                 It.IsAny<CancellationToken>()), Times.Never,
                "never ack a message that was not safely stored");
            _channel.Verify(c => c.BasicNackAsync(13, false, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task NoDeadLetterQueueConfigured_DiscardsRatherThanStalling()
        {
            // With nowhere to put the message, requeueing forever would reproduce the original
            // deadlock. Discarding is the deliberate lesser evil and is logged as an error.
            ConsumerFails();
            var pump = CreatePump(Config(deadLetterQueue: null));
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(14, redelivered: true);

            _channel.Verify(c => c.BasicNackAsync(14, false, false, It.IsAny<CancellationToken>()), Times.Once,
                "requeue:false so the consumer is not blocked by a message it cannot store");
            Assert.AreEqual(0, _deadLetter.Published.Count);
        }

        // ── Success path is untouched ────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Success_IsStillAckedAndNeverDeadLettered()
        {
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ReturnsAsync(ConsumerResult.Success);
            var pump = CreatePump();
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(21, redelivered: true);   // redelivered must not matter on success

            _channel.Verify(c => c.BasicAckAsync(21, false, It.IsAny<CancellationToken>()), Times.Once);
            _channel.Verify(c => c.BasicNackAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(),
                                                  It.IsAny<CancellationToken>()), Times.Never);
            Assert.AreEqual(0, _deadLetter.Published.Count);
        }

        // ── Option clamping ──────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MaxDeliveryAttempts_IsClampedToWhatTheRedeliveredFlagCanExpress()
        {
            Assert.AreEqual(2, new QueueProcessorOptions().EffectiveMaxDeliveryAttempts,
                "default is one retry then dead-letter");
            Assert.AreEqual(2, new QueueProcessorOptions { MaxDeliveryAttempts = 10 }.EffectiveMaxDeliveryAttempts,
                "the redelivered flag is a boolean; higher values cannot be honoured");
            Assert.AreEqual(1, new QueueProcessorOptions { MaxDeliveryAttempts = 1 }.EffectiveMaxDeliveryAttempts);
            Assert.AreEqual(1, new QueueProcessorOptions { MaxDeliveryAttempts = 0 }.EffectiveMaxDeliveryAttempts,
                "0 or negative must not disable acking altogether");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task MaxAttemptsAboveTwo_BehavesAsTwo_RatherThanRetryingForever()
        {
            ConsumerFails();
            var pump = CreatePump(maxDeliveryAttempts: 10);
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(31, redelivered: true);

            Assert.AreEqual(1, _deadLetter.Published.Count,
                "a clamped value must still terminate on the redelivered attempt");
        }
    }
}
