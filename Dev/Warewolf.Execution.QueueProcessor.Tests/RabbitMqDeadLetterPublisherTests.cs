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
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Messaging;
using IConnection = RabbitMQ.Client.IConnection;

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// <see cref="RabbitMqDeadLetterPublisher"/> is the highest-risk untested path in the
    /// container: it is the only place that reacts to the engine reporting a business failure,
    /// and it recovers from a missing dead-letter queue by closing and reopening a channel mid
    /// operation. These tests drive that recovery against mocked <see cref="IConnection"/>/
    /// <see cref="IChannel"/> objects, exactly as <see cref="MessagePumpDrainTests"/> does for the
    /// message pump.
    /// </summary>
    [TestClass]
    public class RabbitMqDeadLetterPublisherTests
    {
        Mock<IConnection> _connection = null!;
        Mock<IChannel> _channel = null!;

        [TestInitialize]
        public void Setup()
        {
            _connection = new Mock<IConnection>(MockBehavior.Loose);
            _channel = new Mock<IChannel>(MockBehavior.Loose);

            _connection
                .Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_channel.Object);
        }

        static ResolvedQueueConfiguration Config(string? deadLetterQueue = "dlq", bool deadLetterDurable = true)
        {
            var trigger = new TriggerDefinition
            {
                TriggerId = Guid.NewGuid(),
                Name = "DeadLetterTrigger",
                QueueName = "main-queue",
                WorkflowName = "folder\\Wf",
                DeadLetterQueue = deadLetterQueue,
                Concurrency = 1,
                DeadLetterOptions = new List<TriggerOption>
                {
                    new() { Name = "Durable", Value = deadLetterDurable },
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

        RabbitMqDeadLetterPublisher CreatePublisher(ResolvedQueueConfiguration config) =>
            new(config, _ => Task.FromResult(_connection.Object));

        static IReadOnlyDictionary<string, object?> Diagnostics() =>
            new Dictionary<string, object?> { ["StatusCode"] = "500" };

        // ── No dead-letter queue configured ──────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_NoDeadLetterQueueConfigured_IsANoOpThatNeverTouchesTheBroker()
        {
            var publisher = CreatePublisher(Config(deadLetterQueue: null));

            await publisher.PublishAsync(new byte[] { 1 }, Diagnostics(), CancellationToken.None);

            _connection.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()),
                                Times.Never);
        }

        // ── Passive probe succeeds (queue already exists) ───────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_QueueAlreadyExists_PublishesOnTheProbedChannelWithoutDeclaring()
        {
            _channel
                .Setup(c => c.QueueDeclarePassiveAsync("dlq", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueueDeclareOk("dlq", 0, 0));

            var publisher = CreatePublisher(Config());

            await publisher.PublishAsync(new byte[] { 1, 2, 3 }, Diagnostics(), CancellationToken.None);

            // Only ONE channel is ever created - the passive probe succeeded, so there is no
            // reason to close it and open a fresh one.
            _connection.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()),
                                Times.Once);
            _channel.Verify(c => c.QueueDeclareAsync(
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object?>?>(), It.IsAny<bool>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Never);
            _channel.Verify(c => c.BasicPublishAsync<BasicProperties>(
                string.Empty, "dlq", false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        // ── Passive probe fails with 404 (queue does not exist yet) ─────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_QueueMissing_ClosesOnlyTheBrokenChannelAndDeclaresOnAFreshOne()
        {
            // A failed passive declare closes the channel server-side. The publisher must not
            // reuse it for the active declare, but it also must not tear down and reconnect the
            // CONNECTION - only a fresh CHANNEL is needed. This is the exact recovery path that
            // used to throw a NullReferenceException the first time a dead-letter queue needed
            // on-demand creation, because the shared teardown helper disposed the connection too.
            var shutdown = new ShutdownEventArgs(ShutdownInitiator.Peer, 404, "NOT_FOUND - no queue 'dlq'");
            _channel
                .SetupSequence(c => c.QueueDeclarePassiveAsync("dlq", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationInterruptedException(shutdown));

            var secondChannel = new Mock<IChannel>(MockBehavior.Loose);
            _connection
                .SetupSequence(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_channel.Object)
                .ReturnsAsync(secondChannel.Object);

            var publisher = CreatePublisher(Config(deadLetterDurable: true));

            await publisher.PublishAsync(new byte[] { 9 }, Diagnostics(), CancellationToken.None);

            // Exactly two channels were opened on the SAME connection - no second connection.
            _connection.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()),
                                Times.Exactly(2));
            _connection.Verify(c => c.DisposeAsync(), Times.Never,
                "the connection is still healthy and must be reused, not torn down");

            // The broken channel is closed; the active declare and the publish happen on the
            // fresh one, with the trigger's DeadLetterOptions durability.
            _channel.Verify(c => c.DisposeAsync(), Times.Once);
            secondChannel.Verify(c => c.QueueDeclareAsync(
                "dlq", true, false, false, null, false, false, It.IsAny<CancellationToken>()), Times.Once);
            secondChannel.Verify(c => c.BasicPublishAsync<BasicProperties>(
                string.Empty, "dlq", false, It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _channel.Verify(c => c.BasicPublishAsync<BasicProperties>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_SecondCallReusesTheOpenChannelWithoutProbingAgain()
        {
            _channel
                .Setup(c => c.QueueDeclarePassiveAsync("dlq", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueueDeclareOk("dlq", 0, 0));
            _channel.SetupGet(c => c.IsOpen).Returns(true);

            var publisher = CreatePublisher(Config());

            await publisher.PublishAsync(new byte[] { 1 }, Diagnostics(), CancellationToken.None);
            await publisher.PublishAsync(new byte[] { 2 }, Diagnostics(), CancellationToken.None);

            _connection.Verify(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()),
                                Times.Once);
            _channel.Verify(c => c.QueueDeclarePassiveAsync("dlq", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
