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

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// The dead-letter path — code that runs ONLY after the engine has already failed, which is
    /// exactly why a defect here can survive every happy-path test and every successful E2E run.
    ///
    /// <para>These tests exist because of a live incident on 2026-08-11. When the dead-letter queue
    /// did not yet exist, the 404 handler called a dispose helper that nulled BOTH the channel and
    /// the connection, then dereferenced <c>_connection!</c> — throwing
    /// <see cref="NullReferenceException"/> every time. The null-forgiving <c>!</c> suppressed the
    /// one compiler warning that would have caught it.</para>
    ///
    /// <para>The consequence was total rather than partial: a DLQ is BY DEFINITION absent the first
    /// time anything fails, so every first dead-letter on a fresh deployment threw,
    /// <c>EngineForwarder</c> refused to ack (deliberately, so the message is not lost), and the
    /// broker redelivered forever — a queue that never drained and replicas pinned at
    /// <c>maxReplicas</c>. Observed: 26 messages looping, DLQ never created.</para>
    /// </summary>
    [TestClass]
    public class RabbitMqDeadLetterPublisherTests
    {
        const string RealTrigger = "triggers-mandate.bite";
        const string RealSource = "0b142714-8f6d-41b7-9832-2aefa8c731ec.bite";

        static string SettingsDir => Path.Combine(AppContext.BaseDirectory, "TestResources");
        static string ResourcePath(string n) => Path.Combine(SettingsDir, "sources", n);
        static string TriggerPath(string n) => Path.Combine(SettingsDir, "triggers", n);

        static ResolvedQueueConfiguration BuildConfig()
        {
            var trigger = new TriggerBiteReader().Read(TriggerPath(RealTrigger));
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(RealSource));
            return new ResolvedQueueConfiguration
            {
                Trigger = trigger,
                Source = source,
                DeadLetterSource = source,
            };
        }

        /// <summary>
        /// A connection whose channels can be told to fail the passive declare with a 404, mimicking
        /// "dead-letter queue does not exist". Tracks how many channels were opened, so a test can
        /// prove the channel — and only the channel — was replaced.
        /// </summary>
        sealed class FakeBroker
        {
            public int ChannelsOpened { get; private set; }
            public int ConnectionsOpened { get; private set; }
            public bool ConnectionDisposed { get; private set; }
            public List<string> DeclaredQueues { get; } = new();
            public List<bool> DeclaredDurable { get; } = new();
            public int PassiveDeclares { get; private set; }
            public int Publishes { get; private set; }

            /// <summary>Passive declare throws 404 until the queue has been actively declared.</summary>
            public bool QueueExists { get; set; }

            public Mock<IConnection> Connection { get; } = new();

            public FakeBroker()
            {
                Connection.SetupGet(c => c.IsOpen).Returns(true);
                Connection.Setup(c => c.DisposeAsync())
                          .Callback(() => ConnectionDisposed = true)
                          .Returns(ValueTask.CompletedTask);

                Connection
                    .Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions?>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => { ChannelsOpened++; return NewChannel(); });
            }

            public Task<IConnection> ConnectAsync(CancellationToken ct)
            {
                ConnectionsOpened++;
                return Task.FromResult(Connection.Object);
            }

            IChannel NewChannel()
            {
                var ch = new Mock<IChannel>();
                ch.SetupGet(c => c.IsOpen).Returns(true);
                ch.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask);

                ch.Setup(c => c.QueueDeclarePassiveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Returns((string q, CancellationToken _) =>
                  {
                      PassiveDeclares++;
                      if (QueueExists) { return Task.FromResult(new QueueDeclareOk(q, 0, 0)); }
                      // Mirrors the broker: a failed passive declare is a channel-level 404.
                      throw new OperationInterruptedException(
                          new ShutdownEventArgs(ShutdownInitiator.Peer, 404, "NOT_FOUND - no queue",
                                                cause: null, cancellationToken: default));
                  });

                ch.Setup(c => c.QueueDeclareAsync(
                        It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<IDictionary<string, object?>?>(), It.IsAny<bool>(), It.IsAny<bool>(),
                        It.IsAny<CancellationToken>()))
                  .Returns((string q, bool durable, bool _, bool __, IDictionary<string, object?>? ___,
                            bool ____, bool _____, CancellationToken ______) =>
                  {
                      DeclaredQueues.Add(q);
                      DeclaredDurable.Add(durable);
                      QueueExists = true;
                      return Task.FromResult(new QueueDeclareOk(q, 0, 0));
                  });

                ch.Setup(c => c.BasicPublishAsync(
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                        It.IsAny<BasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>(),
                        It.IsAny<CancellationToken>()))
                  .Callback(() => Publishes++)
                  .Returns(ValueTask.CompletedTask);

                return ch.Object;
            }
        }

        static readonly Dictionary<string, object?> NoDiagnostics = new();

        // ── The regression ───────────────────────────────────────────────────

        /// <summary>
        /// THE regression test. Before the fix this threw NullReferenceException from
        /// EnsureChannelAsync, because the 404 handler nulled the connection and then used it.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_WhenDeadLetterQueueIsAbsent_CreatesItAndPublishes_WithoutNullReference()
        {
            var broker = new FakeBroker { QueueExists = false };
            await using var sut = new RabbitMqDeadLetterPublisher(BuildConfig(), broker.ConnectAsync);

            await sut.PublishAsync(new byte[] { 1, 2, 3 }, NoDiagnostics, CancellationToken.None);

            Assert.AreEqual(1, broker.DeclaredQueues.Count,
                "the absent dead-letter queue should have been created exactly once");
            Assert.AreEqual(1, broker.Publishes, "the message should have been published to the DLQ");
        }

        /// <summary>
        /// A 404 is a CHANNEL-level error: the broker closes the channel and leaves the connection
        /// open. Recovery must therefore replace only the channel — the bug replaced (and destroyed)
        /// both.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_WhenDeadLetterQueueIsAbsent_ReplacesOnlyTheChannel_NotTheConnection()
        {
            var broker = new FakeBroker { QueueExists = false };
            await using var sut = new RabbitMqDeadLetterPublisher(BuildConfig(), broker.ConnectAsync);

            await sut.PublishAsync(new byte[] { 1 }, NoDiagnostics, CancellationToken.None);

            Assert.AreEqual(1, broker.ConnectionsOpened,
                "the connection should be established once and survive a channel-level 404");
            Assert.IsFalse(broker.ConnectionDisposed,
                "a channel-level 404 must NOT tear down the connection");
            Assert.AreEqual(2, broker.ChannelsOpened,
                "expected the original channel plus one replacement after the failed passive declare");
        }

        // ── Existing-queue path ──────────────────────────────────────────────

        /// <summary>
        /// When the DLQ already exists the passive declare succeeds and arguments are NOT re-asserted.
        /// Re-asserting them is a 406 PRECONDITION_FAILED whenever the queue was created by someone
        /// else — typically PublishRabbitMQActivity, whose IsDurable is an unchecked-by-default box.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_WhenDeadLetterQueueExists_DoesNotRedeclareIt()
        {
            var broker = new FakeBroker { QueueExists = true };
            await using var sut = new RabbitMqDeadLetterPublisher(BuildConfig(), broker.ConnectAsync);

            await sut.PublishAsync(new byte[] { 1 }, NoDiagnostics, CancellationToken.None);

            Assert.AreEqual(0, broker.DeclaredQueues.Count,
                "an existing dead-letter queue must be used as-is, never re-declared");
            Assert.AreEqual(1, broker.ChannelsOpened, "no channel replacement is needed when the passive declare succeeds");
            Assert.AreEqual(1, broker.Publishes);
        }

        /// <summary>The DLQ is created with the trigger's DeadLetterOptions durability.</summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_WhenCreatingDeadLetterQueue_UsesTriggerDeadLetterDurability()
        {
            var config = BuildConfig();
            var broker = new FakeBroker { QueueExists = false };
            await using var sut = new RabbitMqDeadLetterPublisher(config, broker.ConnectAsync);

            await sut.PublishAsync(new byte[] { 1 }, NoDiagnostics, CancellationToken.None);

            Assert.AreEqual(1, broker.DeclaredDurable.Count);
            Assert.AreEqual(config.DeadLetterDurable, broker.DeclaredDurable[0],
                "the DLQ must be created with the durability the trigger declares");
        }

        // ── Channel reuse ────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_CalledTwice_ReusesTheOpenChannel()
        {
            var broker = new FakeBroker { QueueExists = true };
            await using var sut = new RabbitMqDeadLetterPublisher(BuildConfig(), broker.ConnectAsync);

            await sut.PublishAsync(new byte[] { 1 }, NoDiagnostics, CancellationToken.None);
            await sut.PublishAsync(new byte[] { 2 }, NoDiagnostics, CancellationToken.None);

            Assert.AreEqual(1, broker.ChannelsOpened, "the open channel should be reused");
            Assert.AreEqual(1, broker.ConnectionsOpened);
            Assert.AreEqual(2, broker.Publishes);
        }

        // ── No-loss guarantee ────────────────────────────────────────────────

        /// <summary>
        /// If the broker is unreachable, PublishAsync must THROW rather than swallow. EngineForwarder
        /// relies on that to refuse the ack, so the message is redelivered instead of silently lost.
        /// The 2026-08-11 incident was ugly precisely because this guarantee held while the create
        /// path was broken — messages looped rather than vanishing.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_WhenBrokerIsUnreachable_Throws_SoTheMessageIsNotAcked()
        {
            await using var sut = new RabbitMqDeadLetterPublisher(
                BuildConfig(),
                _ => throw new BrokerUnreachableException(new Exception("no route to broker")));

            await Assert.ThrowsExceptionAsync<BrokerUnreachableException>(
                () => sut.PublishAsync(new byte[] { 1 }, NoDiagnostics, CancellationToken.None));
        }

        /// <summary>
        /// A trigger with no dead-letter queue configured is a no-op, not a failure: the forwarder
        /// still acks and the payload survives only in the logs.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PublishAsync_WhenTriggerHasNoDeadLetterQueue_DoesNothing()
        {
            var config = BuildConfig();
            config.Trigger.DeadLetterQueue = null;
            var broker = new FakeBroker();
            await using var sut = new RabbitMqDeadLetterPublisher(config, broker.ConnectAsync);

            await sut.PublishAsync(new byte[] { 1 }, NoDiagnostics, CancellationToken.None);

            Assert.AreEqual(0, broker.ConnectionsOpened, "no broker connection should be made at all");
            Assert.AreEqual(0, broker.Publishes);
        }
    }
}
