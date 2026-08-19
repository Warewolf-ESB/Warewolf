/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Warewolf.Data;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Messaging;
using Warewolf.Streams;
using IConnection = RabbitMQ.Client.IConnection;
using Headers = Warewolf.Data.Headers;

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// The at-least-once contract of <see cref="RabbitMqMessagePump"/>, driven against a mocked
    /// <see cref="IChannel"/>. These are the rules that decide whether a scale-in loses work or
    /// runs a workflow twice, and none of them are observable from the configuration tests:
    ///
    /// <list type="bullet">
    ///   <item>ack ONLY on <see cref="ConsumerResult.Success"/> — a Failed result must stay unacked
    ///   so the broker redelivers it;</item>
    ///   <item>a delivery that arrives once draining has begun is nacked with <c>requeue: true</c>
    ///   and never handed to the consumer, so a surviving replica takes it;</item>
    ///   <item><c>DrainAsync</c> cancels the consumer first, then waits for in-flight work, and
    ///   reports whatever it could not finish rather than acking it.</item>
    /// </list>
    ///
    /// Deliveries are driven by capturing the <see cref="IAsyncBasicConsumer"/> the pump registers
    /// with <c>BasicConsumeAsync</c> and invoking it, which is exactly how the broker would.
    /// </summary>
    [TestClass]
    public class MessagePumpDrainTests
    {
        const string ConsumerTag = "amq.ctag-test";

        Mock<IChannel> _channel = null!;
        Mock<IConnection> _connection = null!;
        Mock<IConsumer> _consumer = null!;
        IAsyncBasicConsumer _registered = null!;

        [TestInitialize]
        public void Setup()
        {
            _channel = new Mock<IChannel>(MockBehavior.Loose);
            _connection = new Mock<IConnection>(MockBehavior.Loose);
            _consumer = new Mock<IConsumer>(MockBehavior.Loose);

            _connection
                .Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_channel.Object);

            // Capture the consumer the pump registers so deliveries can be pushed at it.
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

        static ResolvedQueueConfiguration Config(ushort prefetch = 1, bool durable = false)
        {
            var trigger = new TriggerDefinition
            {
                TriggerId = Guid.NewGuid(),
                Name = "PumpTrigger",
                QueueName = "pump-queue",
                WorkflowName = "folder\\Wf",
                Prefetch = prefetch.ToString(),
                Concurrency = 1,
                Options = new List<TriggerOption>
                {
                    new() { Name = "Durable", Value = durable },
                },
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

        RabbitMqMessagePump CreatePump(ResolvedQueueConfiguration config, int maxConcurrency = 1) =>
            new(config, _consumer.Object, maxConcurrency, _ => Task.FromResult(_connection.Object));

        async Task DeliverAsync(ulong deliveryTag, string body = "hello", string correlationId = "")
        {
            var props = new BasicProperties { CorrelationId = correlationId };
            await _registered.HandleBasicDeliverAsync(
                ConsumerTag, deliveryTag, redelivered: false, exchange: string.Empty,
                routingKey: "pump-queue", properties: props,
                body: new ReadOnlyMemory<byte>(System.Text.Encoding.UTF8.GetBytes(body)));
        }

        // ── Ack contract ─────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Delivery_ConsumerSuccess_IsAckedExactlyOnce()
        {
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ReturnsAsync(ConsumerResult.Success);

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);
            await DeliverAsync(42);

            _channel.Verify(c => c.BasicAckAsync(42, false, It.IsAny<CancellationToken>()), Times.Once);
            _channel.Verify(c => c.BasicNackAsync(It.IsAny<ulong>(), It.IsAny<bool>(), It.IsAny<bool>(),
                                                  It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Delivery_ConsumerFailed_IsNeverAcked()
        {
            // Acking a failure would silently DROP the message, so that part of the original
            // contract stands. What changed is the disposal: the pump used to leave the delivery
            // UNACKED, reasoning that "an unacked message returns when the channel closes" and
            // that nacking would hot-loop. Both halves were wrong - the channel does not close,
            // and with Prefetch=1 an outstanding message blocks every further delivery, so the
            // consumer stalled permanently. It is now nacked with requeue and dead-lettered once
            // the redelivered flag shows the retry is spent; see MessagePumpFailureHandlingTests.
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ReturnsAsync(ConsumerResult.Failed);

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);
            await DeliverAsync(7);

            _channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(),
                                                 It.IsAny<CancellationToken>()), Times.Never);
            _channel.Verify(c => c.BasicNackAsync(7, false, true, It.IsAny<CancellationToken>()), Times.Once,
                "the delivery must be resolved, not abandoned");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Delivery_ConsumerThrows_IsResolvedAndDoesNotKillThePump()
        {
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ThrowsAsync(new InvalidOperationException("boom"));

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);

            await DeliverAsync(9);   // must not propagate

            _channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(),
                                                 It.IsAny<CancellationToken>()), Times.Never);
            _channel.Verify(c => c.BasicNackAsync(9, false, true, It.IsAny<CancellationToken>()), Times.Once,
                "an escaped exception must not abandon the delivery either");
            Assert.AreEqual(0, pump.InFlight, "the slot must be released even on an exception");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Delivery_ForwardsTheCorrelationIdAsTheCustomTransactionHeader()
        {
            // IConsumer.Consume takes the headers as `object`, so the callback must match that
            // signature and cast - the pump always passes a Warewolf.Data.Headers.
            object? seen = null;
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .Callback((byte[] _, object h) => seen = h)
                     .ReturnsAsync(ConsumerResult.Success);

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);
            await DeliverAsync(1, correlationId: "txn-abc");

            var headers = seen as Headers;
            Assert.IsNotNull(headers, "the pump must pass Warewolf.Data.Headers");
            CollectionAssert.Contains(
                headers!["Warewolf-Custom-Transaction-Id", Array.Empty<string>()]!.ToList(), "txn-abc");
        }

        // ── Startup honours the trigger ──────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Start_AppliesPrefetchPerConsumerAndConsumesWithoutDeclaring()
        {
            var pump = CreatePump(Config(prefetch: 5, durable: true));
            await pump.StartAsync(CancellationToken.None);

            // global:false => per CONSUMER, matching the on-prem RabbitConfig.CreateChannel.
            _channel.Verify(c => c.BasicQosAsync(0, 5, false, It.IsAny<CancellationToken>()), Times.Once);

            _channel.Verify(c => c.BasicConsumeAsync(
                "pump-queue", false, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>?>(), It.IsAny<IAsyncBasicConsumer>(),
                It.IsAny<CancellationToken>()), Times.Once);

            // A consumer must NEVER actively declare the queue. This test previously asserted the
            // opposite and even documented the consequence - "a mismatch is a PRECONDITION_FAILED
            // that stops the replica consuming at all" - which is exactly what happened live:
            //   PRECONDITION_FAILED - inequivalent arg 'durable' for queue 'order-failure-queue'
            //   in vhost '/': received 'true' but current is 'false'
            // Whoever creates a queue fixes its arguments; for these queues that is usually
            // PublishRabbitMQActivity, whose IsDurable is an unchecked-by-default designer checkbox,
            // so from a consumer's point of view durability is arbitrary. Both on-prem consumers
            // agree: DsfConsumeRabbitMQActivity has no durability settings at all, and
            // RabbitConnection.StartConsuming calls BasicConsume directly.
            _channel.Verify(c => c.QueueDeclareAsync(
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>?>(), false, It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Start_DoesNotDeclareForANonDurableTriggerEither()
        {
            // Guards the inverse case, so the fix cannot be "declare only when durable is true".
            var pump = CreatePump(Config(durable: false));
            await pump.StartAsync(CancellationToken.None);

            _channel.Verify(c => c.QueueDeclareAsync(
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>?>(), false, It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Start_ReportsAMissingQueueByNameInsteadOfARawAmqpFault()
        {
            // Because the pump no longer creates the queue, "not found" is a real and actionable
            // condition. The raw AMQP 404 text reads like a credentials or networking problem, so it
            // must be translated into something that names the queue and the broker.
            var shutdown = new ShutdownEventArgs(ShutdownInitiator.Peer, 404, "NOT_FOUND - no queue");
            _channel
                .Setup(c => c.BasicConsumeAsync(
                    It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<bool>(), It.IsAny<IDictionary<string, object?>?>(),
                    It.IsAny<IAsyncBasicConsumer>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationInterruptedException(shutdown));

            var pump = CreatePump(Config());

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => pump.StartAsync(CancellationToken.None));

            StringAssert.Contains(ex.Message, "pump-queue");
            StringAssert.Contains(ex.Message, "does not exist");
        }

        // ── Drain ────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Drain_CancelsTheConsumerFirstSoTheBrokerFeedsSurvivingReplicas()
        {
            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);

            var stranded = await pump.DrainAsync(TimeSpan.FromSeconds(1));

            _channel.Verify(c => c.BasicCancelAsync(ConsumerTag, false, It.IsAny<CancellationToken>()),
                            Times.Once);
            Assert.AreEqual(0, stranded, "nothing was in flight, so the drain is clean");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Drain_DeliveryArrivingWhileDraining_IsRequeuedAndNeverConsumed()
        {
            // Step 3 of the drain: messages already buffered in the client are handed straight
            // back so a surviving replica can run them, instead of being executed by a replica
            // that is about to disappear.
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .ReturnsAsync(ConsumerResult.Success);

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);
            await pump.DrainAsync(TimeSpan.FromMilliseconds(200));

            await DeliverAsync(99);

            _channel.Verify(c => c.BasicNackAsync(99, false, true, It.IsAny<CancellationToken>()),
                            Times.Once);
            _consumer.Verify(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()), Times.Never);
            _channel.Verify(c => c.BasicAckAsync(It.IsAny<ulong>(), It.IsAny<bool>(),
                                                 It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Drain_InFlightWorkFinishingInsideTheGrace_IsAckedAndReportsNothingStranded()
        {
            var release = new TaskCompletionSource<ConsumerResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .Returns(() => release.Task);

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);

            var delivery = DeliverAsync(11);
            await WaitForInFlightAsync(pump, 1);

            var drain = pump.DrainAsync(TimeSpan.FromSeconds(10));
            release.SetResult(ConsumerResult.Success);       // completes inside the grace
            await delivery;

            Assert.AreEqual(0, await drain);
            _channel.Verify(c => c.BasicAckAsync(11, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Drain_WorkOutlastingTheGrace_ReportsItStrandedAndDoesNotAckIt()
        {
            // The honest failure mode of at-least-once. It must be REPORTED (the caller logs it and
            // it is the signal to raise ShutdownGraceSeconds or lower Engine:TimeoutSeconds), and
            // the message must not be acked - it will be redelivered and may run twice.
            var release = new TaskCompletionSource<ConsumerResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .Returns(() => release.Task);

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);

            var delivery = DeliverAsync(21);
            await WaitForInFlightAsync(pump, 1);

            var stranded = await pump.DrainAsync(TimeSpan.FromMilliseconds(300));

            Assert.AreEqual(1, stranded, "the in-flight message outlived the grace window");
            _channel.Verify(c => c.BasicAckAsync(21, false, It.IsAny<CancellationToken>()), Times.Never);

            release.SetResult(ConsumerResult.Success);
            await delivery;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Drain_BasicCancelFailing_StillDrainsRatherThanThrowing()
        {
            // A broker that has already dropped the channel must not turn a graceful shutdown into
            // an unhandled exception during SIGTERM handling.
            _channel.Setup(c => c.BasicCancelAsync(It.IsAny<string>(), It.IsAny<bool>(),
                                                   It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new IOException("channel gone"));

            var pump = CreatePump(Config());
            await pump.StartAsync(CancellationToken.None);

            var stranded = await pump.DrainAsync(TimeSpan.FromMilliseconds(200));

            Assert.AreEqual(0, stranded);
        }

        // ── Concurrency cap ──────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task MaxConcurrency_CapsInFlightWorkRegardlessOfHowManyDeliveriesArrive()
        {
            // MaxConcurrency 1 is the measured on-prem behaviour and what makes
            // maxReplicas = Concurrency the correct parity mapping. If the throttle leaked, a
            // single replica would run several workflows at once and the ceiling would be wrong.
            var release = new TaskCompletionSource<ConsumerResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _consumer.Setup(c => c.Consume(It.IsAny<byte[]>(), It.IsAny<Headers>()))
                     .Returns(() => release.Task);

            var pump = CreatePump(Config(prefetch: 5), maxConcurrency: 1);
            await pump.StartAsync(CancellationToken.None);

            var first = DeliverAsync(1);
            await WaitForInFlightAsync(pump, 1);
            var second = DeliverAsync(2);            // must queue behind the throttle

            await Task.Delay(150);
            Assert.AreEqual(1, pump.InFlight, "the second delivery must wait for the first to finish");

            release.SetResult(ConsumerResult.Success);
            await Task.WhenAll(first, second);
        }

        static async Task WaitForInFlightAsync(RabbitMqMessagePump pump, int expected)
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (pump.InFlight != expected && DateTime.UtcNow < deadline)
            {
                await Task.Delay(10);
            }

            Assert.AreEqual(expected, pump.InFlight, "timed out waiting for the expected in-flight count");
        }
    }
}
