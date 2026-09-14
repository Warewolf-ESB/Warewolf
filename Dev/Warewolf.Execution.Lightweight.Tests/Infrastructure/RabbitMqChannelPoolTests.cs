/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure
{
    /// <summary>
    /// Covers <see cref="RabbitMqChannelPool"/>, which had no tests at all until the 2026-09-14
    /// review — while <c>Deploy-WwExecutionEngine.ps1</c> was shipping <c>channelPool:true</c> to
    /// every engine.
    ///
    /// <para><b>The invariant these tests exist to hold.</b> The pool may hand out a channel it does
    /// not own (the over-cap path deliberately serves a private one rather than blocking a publish),
    /// but it must never then treat that channel as its own. It used to do exactly that: a healthy
    /// private channel was parked in the idle bag, so <c>maxChannelsPerConnection</c> bounded nothing,
    /// and a faulted one decremented the live count it had never incremented, so the counter drifted
    /// below zero until the cap check could not fire at all and the bag grew without limit. Against a
    /// broker whose default <c>channel_max</c> is 2047 per connection, that ends the connection.</para>
    ///
    /// <para>Everything runs against an in-memory <see cref="FakeConnection"/> — no broker, no
    /// sockets, no sleeping — and asserts <b>object identity and bounded counts</b> rather than
    /// symptoms, so the suite is deterministic and cannot flake.</para>
    /// </summary>
    [TestClass]
    public class RabbitMqChannelPoolTests
    {
        const string Cat = nameof(RabbitMqChannelPool);
        const string Owner = "Ashley Lewis";

        // ── The reason the pool exists ───────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Rent_AfterAReturn_HandsBackTheVerySameChannel()
        {
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 4);

            var first = pool.Rent();
            pool.Return(first);

            Assert.AreSame(first, pool.Rent(), "a returned, healthy channel must be reused — reuse is the entire point of pooling.");
            Assert.AreEqual(1, conn.CreatedCount, "reusing a channel must not open another one.");
        }

        // ── The cap: what used to be unenforceable ───────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Rent_OverTheCap_ServesAPrivateChannelRatherThanBlockingThePublish()
        {
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 1);

            var pooled = pool.Rent();
            var priv = pool.Rent();

            Assert.AreNotSame(pooled, priv, "over the cap the pool must still serve a channel, just not one of its own.");
            Assert.AreEqual(2, conn.CreatedCount);
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Return_AHealthyOverCapChannel_DisposesItInsteadOfPoolingIt()
        {
            // THE REGRESSION. A private channel looks identical to a pooled one from the caller's
            // side, so the pool has to remember which ones it issued. When it did not, this channel
            // was added to the idle bag and the cap stopped bounding anything at all.
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 1);

            var pooled = pool.Rent();
            var priv = (FakeModel)pool.Rent();

            pool.Return(priv);

            Assert.AreEqual(0, pool.IdleChannels, "a channel the pool never issued must not be parked in the pool.");
            Assert.AreEqual(1, priv.DisposeCount, "it must be disposed instead — exactly what the caller would have done.");

            pool.Return(pooled);
            Assert.AreEqual(1, pool.IdleChannels, "the pool's OWN channel still recycles normally.");
            Assert.AreSame(pooled, pool.Rent());
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void IdleChannels_NeverExceedsTheCap_HoweverManyOverCapCyclesRun()
        {
            // The invariant, asserted directly. Before the fix the idle bag grew by the over-cap
            // surplus on every cycle, because each private channel was pooled on return.
            var conn = new FakeConnection();
            const int cap = 2;
            using var pool = new RabbitMqChannelPool(conn, cap);

            for (var cycle = 0; cycle < 50; cycle++)
            {
                var rented = new List<IModel>();
                for (var i = 0; i < cap + 3; i++)
                {
                    rented.Add(pool.Rent());
                }

                foreach (var channel in rented)
                {
                    pool.Return(channel);
                }

                Assert.IsTrue(pool.IdleChannels <= cap,
                    $"cycle {cycle}: the pool parked {pool.IdleChannels} channels with a cap of {cap}.");
            }
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Return_AFaultedOverCapChannel_DoesNotFreeASlotItNeverTook()
        {
            // The drift. Decrementing the live count for a channel that never incremented it pushed
            // the counter below zero, after which `Increment(_live) > _maxChannels` could never be
            // true again and the cap was gone for the life of the connection.
            var conn = new FakeConnection();
            const int cap = 1;
            using var pool = new RabbitMqChannelPool(conn, cap);

            for (var cycle = 0; cycle < 25; cycle++)
            {
                var pooled = pool.Rent();
                var priv = (FakeModel)pool.Rent();   // over the cap

                priv.Open = false;                   // the broker killed it mid-publish
                pool.Return(priv);
                pool.Return(pooled);
            }

            // ChannelsCreated is the assertion that actually catches the drift, and the reason this
            // test asserts it rather than the idle count: with the counter wrongly freed, the pool
            // believes a slot is available on every cycle and opens a BRAND NEW channel it did not
            // need — one per cycle, indefinitely — while the idle count sits innocently at 1.
            Assert.IsTrue(pool.ChannelsCreated <= 2,
                $"the pool opened {pool.ChannelsCreated} channels over {25} cycles that should have reused one; " +
                "the live count was decremented for a channel the pool never issued.");

            Assert.IsTrue(pool.IdleChannels <= cap,
                $"the pool parked {pool.IdleChannels} channels with a cap of {cap}.");

            var stillOpen = conn.Created.Count(c => c.Open && c.DisposeCount == 0);
            Assert.IsTrue(stillOpen <= cap,
                $"{stillOpen} channels are still open against a cap of {cap}; the pool is leaking channels to the broker.");
        }

        // ── Health: a channel the broker closed is never handed out ──────────────────────────────

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Return_AFaultedOwnedChannel_DiscardsItAndFreesItsSlot()
        {
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 1);

            var first = (FakeModel)pool.Rent();
            first.Open = false;
            pool.Return(first);

            Assert.AreEqual(0, pool.IdleChannels, "a closed channel must never be recycled.");
            Assert.AreEqual(1, first.DisposeCount);
            Assert.AreEqual(1, pool.ChannelsDiscarded);

            // The slot it held must have been released, or the pool would be permanently full.
            var replacement = pool.Rent();
            Assert.AreNotSame(first, replacement);
            Assert.AreEqual(2, pool.ChannelsCreated, "the discarded channel's slot must be reusable.");
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Rent_DropsAnIdleChannelTheBrokerClosedWhileItSat_AndOpensAReplacement()
        {
            // EnsureTopology's passive declare raises a CHANNEL-level 404 that closes the channel
            // server-side. Harmless when every publish opens its own; poison when they are pooled.
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 2);

            var first = (FakeModel)pool.Rent();
            pool.Return(first);
            first.Open = false;                 // closed by the broker while idle

            var next = (FakeModel)pool.Rent();

            Assert.AreNotSame(first, next, "a channel closed while idle must not be handed out.");
            Assert.IsTrue(next.Open);
            Assert.AreEqual(1, first.DisposeCount);
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void IsUsable_ToleratesAChannelThatThrowsFromIsOpen()
        {
            // A fully torn-down channel throws on the property itself; that must not escape into a
            // caller's publish path.
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 2);

            var first = (FakeModel)pool.Rent();
            first.ThrowOnIsOpen = true;

            pool.Return(first);

            Assert.AreEqual(0, pool.IdleChannels);
            Assert.AreEqual(1, first.DisposeCount);
        }

        // ── Lifetime ─────────────────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Dispose_ClosesEveryIdleChannel()
        {
            var conn = new FakeConnection();
            var pool = new RabbitMqChannelPool(conn, 4);

            var a = (FakeModel)pool.Rent();
            var b = (FakeModel)pool.Rent();
            pool.Return(a);
            pool.Return(b);

            pool.Dispose();

            Assert.AreEqual(0, pool.IdleChannels);
            Assert.AreEqual(1, a.DisposeCount);
            Assert.AreEqual(1, b.DisposeCount);
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Dispose_IsIdempotent()
        {
            var conn = new FakeConnection();
            var pool = new RabbitMqChannelPool(conn, 2);

            var a = (FakeModel)pool.Rent();
            pool.Return(a);

            pool.Dispose();
            pool.Dispose();

            Assert.AreEqual(1, a.DisposeCount, "a second Dispose must not close an already-closed channel again.");
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void AfterDispose_RentServesAPrivateChannelAndReturnDisposesIt()
        {
            // Invalidate() disposes the pool while other leases are still publishing on the entry.
            // Those publishes must keep working, unpooled — which is the pre-cache behaviour.
            var conn = new FakeConnection();
            var pool = new RabbitMqChannelPool(conn, 2);
            pool.Dispose();

            var channel = (FakeModel)pool.Rent();
            Assert.IsTrue(channel.Open, "a disposed pool must still serve a working channel.");

            pool.Return(channel);
            Assert.AreEqual(1, channel.DisposeCount, "and dispose it on return rather than pooling it.");
            Assert.AreEqual(0, pool.IdleChannels);
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Return_Null_IsANoOp()
        {
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 2);

            pool.Return(null);

            Assert.AreEqual(0, pool.IdleChannels);
            Assert.AreEqual(0, pool.ChannelsDiscarded);
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Constructor_ClampsANonsensicalCapToOne()
        {
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 0);

            var a = pool.Rent();
            pool.Return(a);

            Assert.AreEqual(1, pool.IdleChannels, "a cap below 1 must behave as 1, not as 'no pooling at all'.");
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void Constructor_RejectsANullConnection()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new RabbitMqChannelPool(null, 4));
        }

        // ── Concurrency: the discipline that makes a non-thread-safe IModel safe ─────────────────

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void ConcurrentRenters_NeverHoldTheSameChannelAtTheSameTime()
        {
            // IModel is NOT thread-safe in RabbitMQ.Client 5.1.2 — two publishes sharing one channel
            // corrupt the frame stream. Exclusive ownership between Rent and Return is the whole
            // safety argument for sharing the connection, so it is asserted on object identity.
            var conn = new FakeConnection();
            using var pool = new RabbitMqChannelPool(conn, 4);

            var held = new HashSet<IModel>();
            var collisions = 0;

            Parallel.For(0, 500, _ =>
            {
                var channel = pool.Rent();
                lock (held)
                {
                    if (!held.Add(channel))
                    {
                        Interlocked.Increment(ref collisions);
                    }
                }

                Thread.SpinWait(50);

                lock (held)
                {
                    held.Remove(channel);
                }
                pool.Return(channel);
            });

            Assert.AreEqual(0, collisions, "two renters held the same IModel concurrently — the frame stream would corrupt.");
        }

        [TestMethod, Timeout(60000), Owner(Owner), TestCategory(Cat)]
        public void ConcurrentChurn_LeavesThePoolBoundedByItsCap()
        {
            var conn = new FakeConnection();
            const int cap = 3;
            using var pool = new RabbitMqChannelPool(conn, cap);

            Parallel.For(0, 800, i =>
            {
                var channel = pool.Rent();

                // Every so often the broker kills a channel mid-publish, exercising both the owned
                // and the private discard paths under contention.
                if (i % 7 == 0)
                {
                    ((FakeModel)channel).Open = false;
                }

                pool.Return(channel);
            });

            Assert.IsTrue(pool.IdleChannels <= cap,
                $"the pool settled holding {pool.IdleChannels} channels with a cap of {cap}.");

            var stillOpen = conn.Created.Count(c => c.Open && c.DisposeCount == 0);
            Assert.IsTrue(stillOpen <= cap,
                $"{stillOpen} channels remain open against a cap of {cap}.");
        }

        // ── Fakes ────────────────────────────────────────────────────────────────────────────────

        sealed class FakeConnection : IConnection
        {
            readonly List<FakeModel> _created = new();

            internal IReadOnlyList<FakeModel> Created
            {
                get { lock (_created) { return _created.ToArray(); } }
            }

            internal int CreatedCount
            {
                get { lock (_created) { return _created.Count; } }
            }

            public IModel CreateModel()
            {
                var model = new FakeModel();
                lock (_created)
                {
                    _created.Add(model);
                }
                return model;
            }

            public bool IsOpen => true;
            public void Dispose() { }

            public void Abort() { }
            public void Abort(ushort reasonCode, string reasonText) { }
            public void Abort(int timeout) { }
            public void Abort(ushort reasonCode, string reasonText, int timeout) { }
            public void Close() { }
            public void Close(ushort reasonCode, string reasonText) { }
            public void Close(int timeout) { }
            public void Close(ushort reasonCode, string reasonText, int timeout) { }

            public void HandleConnectionBlocked(string reason) => throw new NotSupportedException();
            public void HandleConnectionUnblocked() => throw new NotSupportedException();

            public bool AutoClose { get; set; }
            public ushort ChannelMax => 0;
            public IDictionary<string, object> ClientProperties => new Dictionary<string, object>();
            public ShutdownEventArgs CloseReason => null;
            public AmqpTcpEndpoint Endpoint => null;
            public uint FrameMax => 0;
            public ushort Heartbeat => 0;
            public AmqpTcpEndpoint[] KnownHosts => Array.Empty<AmqpTcpEndpoint>();
            public IProtocol Protocol => null;
            public IDictionary<string, object> ServerProperties => new Dictionary<string, object>();
            public IList<ShutdownReportEntry> ShutdownReport => new List<ShutdownReportEntry>();
            public string ClientProvidedName => "fake";
            public ConsumerWorkService ConsumerWorkService => null;
            public int LocalPort => 0;
            public int RemotePort => 0;

            public event EventHandler<CallbackExceptionEventArgs> CallbackException { add { } remove { } }
            public event EventHandler<EventArgs> RecoverySucceeded { add { } remove { } }
            public event EventHandler<ConnectionRecoveryErrorEventArgs> ConnectionRecoveryError { add { } remove { } }
            public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked { add { } remove { } }
            public event EventHandler<ShutdownEventArgs> ConnectionShutdown { add { } remove { } }
            public event EventHandler<EventArgs> ConnectionUnblocked { add { } remove { } }
        }

        sealed class FakeModel : IModel
        {
            int _disposeCount;

            internal volatile bool Open = true;
            internal volatile bool ThrowOnIsOpen;

            internal int DisposeCount => Volatile.Read(ref _disposeCount);

            public bool IsOpen
            {
                get
                {
                    if (ThrowOnIsOpen)
                    {
                        throw new InvalidOperationException("channel is torn down");
                    }
                    return Open;
                }
            }

            public void Dispose()
            {
                Interlocked.Increment(ref _disposeCount);
                Open = false;
            }

            public bool IsClosed => !Open;

            // Nothing below is exercised by the pool, which only ever opens, checks and closes a
            // channel. Throwing rather than no-op'ing keeps it that way: if the pool ever starts
            // using the AMQP surface, these say so loudly instead of silently passing.
            public int ChannelNumber => 0;
            public ShutdownEventArgs CloseReason => null;
            public IBasicConsumer DefaultConsumer { get; set; }
            public ulong NextPublishSeqNo => 0;
            public TimeSpan ContinuationTimeout { get; set; }

            public void Abort() => throw new NotSupportedException();
            public void Abort(ushort replyCode, string replyText) => throw new NotSupportedException();
            public void BasicAck(ulong deliveryTag, bool multiple) => throw new NotSupportedException();
            public void BasicCancel(string consumerTag) => throw new NotSupportedException();
            public string BasicConsume(string queue, bool autoAck, string consumerTag, bool noLocal, bool exclusive, IDictionary<string, object> arguments, IBasicConsumer consumer) => throw new NotSupportedException();
            public BasicGetResult BasicGet(string queue, bool autoAck) => throw new NotSupportedException();
            public void BasicNack(ulong deliveryTag, bool multiple, bool requeue) => throw new NotSupportedException();
            public void BasicPublish(string exchange, string routingKey, bool mandatory, IBasicProperties basicProperties, byte[] body) => throw new NotSupportedException();
            public void BasicQos(uint prefetchSize, ushort prefetchCount, bool global) => throw new NotSupportedException();
            public void BasicRecover(bool requeue) => throw new NotSupportedException();
            public void BasicRecoverAsync(bool requeue) => throw new NotSupportedException();
            public void BasicReject(ulong deliveryTag, bool requeue) => throw new NotSupportedException();
            public void Close() => throw new NotSupportedException();
            public void Close(ushort replyCode, string replyText) => throw new NotSupportedException();
            public void ConfirmSelect() => throw new NotSupportedException();
            public uint ConsumerCount(string queue) => throw new NotSupportedException();
            public IBasicProperties CreateBasicProperties() => throw new NotSupportedException();
            public IBasicPublishBatch CreateBasicPublishBatch() => throw new NotSupportedException();
            public void ExchangeBind(string destination, string source, string routingKey, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void ExchangeBindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void ExchangeDeclareNoWait(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void ExchangeDeclarePassive(string exchange) => throw new NotSupportedException();
            public void ExchangeDelete(string exchange, bool ifUnused) => throw new NotSupportedException();
            public void ExchangeDeleteNoWait(string exchange, bool ifUnused) => throw new NotSupportedException();
            public void ExchangeUnbind(string destination, string source, string routingKey, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void ExchangeUnbindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public uint MessageCount(string queue) => throw new NotSupportedException();
            public void QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void QueueBindNoWait(string queue, string exchange, string routingKey, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public QueueDeclareOk QueueDeclare(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void QueueDeclareNoWait(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public QueueDeclareOk QueueDeclarePassive(string queue) => throw new NotSupportedException();
            public uint QueueDelete(string queue, bool ifUnused, bool ifEmpty) => throw new NotSupportedException();
            public void QueueDeleteNoWait(string queue, bool ifUnused, bool ifEmpty) => throw new NotSupportedException();
            public uint QueuePurge(string queue) => throw new NotSupportedException();
            public void QueueUnbind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments) => throw new NotSupportedException();
            public void TxCommit() => throw new NotSupportedException();
            public void TxRollback() => throw new NotSupportedException();
            public void TxSelect() => throw new NotSupportedException();
            public bool WaitForConfirms() => throw new NotSupportedException();
            public bool WaitForConfirms(TimeSpan timeout) => throw new NotSupportedException();
            public bool WaitForConfirms(TimeSpan timeout, out bool timedOut) => throw new NotSupportedException();
            public void WaitForConfirmsOrDie() => throw new NotSupportedException();
            public void WaitForConfirmsOrDie(TimeSpan timeout) => throw new NotSupportedException();

            public event EventHandler<BasicAckEventArgs> BasicAcks { add { } remove { } }
            public event EventHandler<BasicNackEventArgs> BasicNacks { add { } remove { } }
            public event EventHandler<EventArgs> BasicRecoverOk { add { } remove { } }
            public event EventHandler<BasicReturnEventArgs> BasicReturn { add { } remove { } }
            public event EventHandler<CallbackExceptionEventArgs> CallbackException { add { } remove { } }
            public event EventHandler<FlowControlEventArgs> FlowControl { add { } remove { } }
            public event EventHandler<ShutdownEventArgs> ModelShutdown { add { } remove { } }
        }
    }
}
