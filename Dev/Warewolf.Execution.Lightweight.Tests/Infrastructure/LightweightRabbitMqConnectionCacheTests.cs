/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure
{
    /// <summary>
    /// Covers <see cref="LightweightRabbitMqConnectionCache"/>.
    ///
    /// <para>Every test runs against an in-memory <see cref="FakeConnection"/> and a controllable
    /// clock. No broker, no sockets, no sleeping — a test that waited out a five-minute idle timeout
    /// is a test nobody would run, and a cache test racing a background timer is the classic flake.
    /// The sweep timer is therefore disabled and driven explicitly.</para>
    ///
    /// <para>The assertions are deliberately about <b>invariants</b> — object identity for reuse,
    /// dispose-exactly-once for lifetime, bounded counts for growth — rather than about symptoms.</para>
    /// </summary>
    [TestClass]
    public class LightweightRabbitMqConnectionCacheTests
    {
        const string KeyA = "aaaa1111";
        const string KeyB = "bbbb2222";

        // ── T12: the reason the feature exists ──────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_SecondAcquire_ReturnsTheVerySameConnectionInstance()
        {
            var f = new Fixture();

            IConnection first;
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                first = lease.Connection;
                Assert.IsTrue(lease.IsCached);
            }

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                // Identity, not a count: a cache that silently reconnected would still report one
                // acquire per call.
                Assert.AreSame(first, lease.Connection);
            }

            Assert.AreEqual(1, f.Cache.ConnectionsCreated);
            Assert.AreEqual(0, f.Cache.ConnectionsDisposed);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_DifferentKeys_DoNotShareAConnection()
        {
            var f = new Fixture();

            using (var a = f.Cache.Acquire(KeyA, f.NewConnection))
            using (var b = f.Cache.Acquire(KeyB, f.NewConnection))
            {
                Assert.AreNotSame(a.Connection, b.Connection,
                    "different broker coordinates must never share a connection");
            }

            Assert.AreEqual(2, f.Cache.ConnectionsCreated);
        }

        // ── T13 / T15 / T16: lifetime ───────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_IdleBeyondTimeout_ReplacesAndDisposesTheOldConnectionExactlyOnce()
        {
            var f = new Fixture(idle: TimeSpan.FromMinutes(5));

            IConnection first;
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                first = lease.Connection;
            }

            f.Clock.Advance(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.AreNotSame(first, lease.Connection, "an idle-expired connection must not be reused");
            }

            Assert.AreEqual(2, f.Cache.ConnectionsCreated);
            Assert.AreEqual(1, ((FakeConnection)first).DisposeCount,
                "the replaced connection must be disposed exactly once — not zero (leak), not twice (throw)");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_ContinuousUse_StillRecyclesAtTheAbsoluteMaxLifetime()
        {
            // The case a sliding window alone cannot catch: a source busy enough that the idle clock
            // never expires would otherwise pin one connection for the life of the process.
            var f = new Fixture(idle: TimeSpan.FromMinutes(5), lifetime: TimeSpan.FromMinutes(30));

            IConnection first;
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                first = lease.Connection;
            }

            for (var i = 0; i < 10; i++)
            {
                f.Clock.Advance(TimeSpan.FromMinutes(4));   // never idle-expires
                using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
                {
                    Assert.IsNotNull(lease.Connection);
                }
            }

            Assert.IsTrue(f.Cache.ConnectionsCreated > 1,
                "the absolute lifetime ceiling must force a reconnect even under continuous use");
            Assert.AreEqual(1, ((FakeConnection)first).DisposeCount);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_ClosedConnection_IsNeverHandedOut()
        {
            var f = new Fixture();

            IConnection first;
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                first = lease.Connection;
            }

            // The broker dropped it underneath us; heartbeats flip IsOpen.
            ((FakeConnection)first).Open = false;

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.AreNotSame(first, lease.Connection, "a dead connection must be replaced, not served");
                Assert.IsTrue(lease.Connection.IsOpen);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_ConnectionThrowingFromIsOpen_IsTreatedAsUnusableNotPropagated()
        {
            var f = new Fixture();

            IConnection first;
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                first = lease.Connection;
            }

            ((FakeConnection)first).ThrowOnIsOpen = true;

            // A broken connection must not surface its failure inside an unrelated caller's publish.
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.AreNotSame(first, lease.Connection);
            }
        }

        // ── T17: poisoning ──────────────────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_InvalidatedLease_EvictsAndDisposesOnRelease()
        {
            var f = new Fixture();

            IConnection first;
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                first = lease.Connection;
                lease.Invalidate();
                // Still alive here: the caller is mid-publish and holds the only reason it exists.
                Assert.AreEqual(0, ((FakeConnection)first).DisposeCount);
            }

            Assert.AreEqual(1, ((FakeConnection)first).DisposeCount,
                "releasing the last lease on a poisoned entry must dispose it");
            Assert.AreEqual(0, f.Cache.CachedEntryCount);

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.AreNotSame(first, lease.Connection);
            }
        }

        // ── T21: the topology memo ──────────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_TopologyMemo_IsFreshInsideItsTtlAndStaleAfter()
        {
            var f = new Fixture(topologyTtl: TimeSpan.FromSeconds(30));

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.IsFalse(lease.IsTopologyFresh("q"), "nothing is known before the first declare");
                lease.MarkTopologyDeclared("q");
                Assert.IsTrue(lease.IsTopologyFresh("q"));
                Assert.IsFalse(lease.IsTopologyFresh("other"), "the memo is per queue, not per connection");
            }

            f.Clock.Advance(TimeSpan.FromSeconds(29));
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.IsTrue(lease.IsTopologyFresh("q"), "the memo survives across leases on one connection");
            }

            f.Clock.Advance(TimeSpan.FromSeconds(2));
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.IsFalse(lease.IsTopologyFresh("q"),
                    "past the TTL the declares must run again, so an out-of-band deletion is re-detected");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_ZeroTopologyTtl_DisablesTheMemoButKeepsConnectionReuse()
        {
            var f = new Fixture(topologyTtl: TimeSpan.Zero);

            IConnection first;
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                first = lease.Connection;
                lease.MarkTopologyDeclared("q");
                Assert.IsFalse(lease.IsTopologyFresh("q"), "ttl=0 means always re-declare");
            }

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.AreSame(first, lease.Connection, "the connection is still reused");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_TopologyMemo_IsBoundedAndClearedOnInvalidate()
        {
            var f = new Fixture();

            const int cap = LightweightRabbitMqConnectionCache.MaxTopologyEntriesPerConnection;
            const int distinctNames = cap * 4;

            // Probed on THIS lease throughout. Re-acquiring after Invalidate would hand back a brand
            // new entry whose memo is empty anyway, so the assertion would pass without proving the
            // clear ever happened.
            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                // Queue names come from workflow data, so the memo is unbounded by nature. Far more
                // distinct names than the cap must not grow without limit.
                for (var i = 0; i < distinctNames; i++)
                {
                    lease.MarkTopologyDeclared("queue-" + i);
                }

                var fresh = 0;
                for (var i = 0; i < distinctNames; i++)
                {
                    if (lease.IsTopologyFresh("queue-" + i))
                    {
                        fresh++;
                    }
                }

                Assert.IsTrue(fresh > 0, "the memo must still be doing its job below the cap");
                Assert.IsTrue(fresh <= cap, $"memo grew to {fresh} entries, above the {cap} cap");

                lease.Invalidate();

                for (var i = 0; i < distinctNames; i++)
                {
                    Assert.IsFalse(lease.IsTopologyFresh("queue-" + i),
                        "what we believed about topology was learned over a connection we no longer trust");
                }
            }
        }

        // ── T18: capacity and LRU ───────────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_OverCapacity_EvictsTheLeastRecentlyUsedIdleEntry()
        {
            var f = new Fixture(maxConnections: 2);

            using (var l = f.Cache.Acquire("k1", f.NewConnection))
            {
                Assert.IsNotNull(l.Connection);
            }
            var oldest = f.Created[0];

            f.Clock.Advance(TimeSpan.FromSeconds(10));
            using (var l = f.Cache.Acquire("k2", f.NewConnection))
            {
                Assert.IsNotNull(l.Connection);
            }

            f.Clock.Advance(TimeSpan.FromSeconds(10));
            using (var l = f.Cache.Acquire("k3", f.NewConnection))
            {
                Assert.IsTrue(l.IsCached, "there was an idle entry to evict, so k3 must still be cached");
            }

            Assert.AreEqual(2, f.Cache.CachedEntryCount, "the cap must hold");
            Assert.AreEqual(1, oldest.DisposeCount, "the LRU entry is the one that goes");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_AtCapacityWithEveryEntryInUse_ServesUncachedRatherThanEvictingALiveConnection()
        {
            var f = new Fixture(maxConnections: 2);

            using (var a = f.Cache.Acquire("k1", f.NewConnection))
            using (var b = f.Cache.Acquire("k2", f.NewConnection))
            {
                Assert.IsTrue(a.IsCached);
                Assert.IsTrue(b.IsCached);

                using (var overflow = f.Cache.Acquire("k3", f.NewConnection))
                {
                    // Evicting an in-use entry would free nothing (its lease keeps the connection
                    // alive) and would throw away a warm one. Degrading is the correct trade.
                    Assert.IsFalse(overflow.IsCached, "the overflow publish must degrade, not fail");
                    Assert.IsFalse(overflow.IsTopologyFresh("q"),
                        "an uncached lease must report no topology, i.e. behave exactly like no cache");
                    Assert.IsNotNull(overflow.Connection);
                }

                // An uncached lease owns its connection, so releasing it must close it.
                Assert.AreEqual(1, f.Created[2].DisposeCount);
            }

            Assert.AreEqual(1, f.Cache.UncachedFallbacks);
            Assert.AreEqual(2, f.Cache.CachedEntryCount);
        }

        // ── T22: the factory's own failures must not be swallowed ───────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_ConnectionFactoryThrows_PropagatesAndLeavesNoHalfInsertedEntry()
        {
            var f = new Fixture();

            // An unreachable broker is a real error the workflow must see. Degrading to "uncached"
            // here would retry the same failure and report it twice, or hide it entirely.
            Assert.ThrowsException<TimeoutException>(() =>
                f.Cache.Acquire(KeyA, () => throw new TimeoutException("broker unreachable")));

            Assert.AreEqual(0, f.Cache.CachedEntryCount, "a failed creation must leave nothing behind");

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.IsNotNull(lease.Connection, "the cache must still work after a failed attempt");
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_NullFactory_Throws()
        {
            var f = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => f.Cache.Acquire(KeyA, null));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_BlankKey_DegradesToUncached()
        {
            var f = new Fixture();

            using (var lease = f.Cache.Acquire("", f.NewConnection))
            {
                Assert.IsFalse(lease.IsCached);
            }

            Assert.AreEqual(0, f.Cache.CachedEntryCount);
        }

        // ── T20: shutdown ───────────────────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_Dispose_ClosesEveryConnectionOnceAndKeepsServingUncached()
        {
            var f = new Fixture();

            using (var l = f.Cache.Acquire("k1", f.NewConnection))
            {
                Assert.IsNotNull(l.Connection);
            }
            using (var l = f.Cache.Acquire("k2", f.NewConnection))
            {
                Assert.IsNotNull(l.Connection);
            }

            f.Cache.Dispose();

            foreach (var c in f.Created)
            {
                Assert.AreEqual(1, c.DisposeCount,
                    "shutdown must release every socket, I/O thread and heartbeat timer, exactly once");
            }

            f.Cache.Dispose();
            foreach (var c in f.Created)
            {
                Assert.AreEqual(1, c.DisposeCount, "Dispose must be idempotent");
            }

            // A publish racing shutdown must still work rather than throw.
            using (var lease = f.Cache.Acquire("k3", f.NewConnection))
            {
                Assert.IsFalse(lease.IsCached);
                Assert.IsNotNull(lease.Connection);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_DisposeWhileLeaseOutstanding_DefersTheCloseUntilRelease()
        {
            var f = new Fixture();

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                f.Cache.Dispose();

                // THE use-after-dispose guard: shutdown must never close a connection someone is
                // actively publishing on. That would be an ObjectDisposedException mid-publish,
                // strictly worse than the leak this whole design is avoiding.
                Assert.AreEqual(0, ((FakeConnection)lease.Connection).DisposeCount);
                Assert.IsTrue(lease.Connection.IsOpen);
            }

            Assert.AreEqual(1, f.Created[0].DisposeCount, "and it must be closed once the lease releases");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_EvictedWhileLeaseOutstanding_StaysUsableUntilReleased()
        {
            // The reference-count invariant, exercised DETERMINISTICALLY.
            //
            // The concurrency stress test below cannot be relied on for this: with defect injection
            // it passed even with reference counting removed entirely, because the window in which
            // the sweeper evicts the exact entry a worker is holding is narrow and timing-dependent.
            // Forcing the eviction while a lease is provably open is what actually pins the invariant.
            var f = new Fixture(idle: TimeSpan.FromMinutes(5));

            using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                var connection = (FakeConnection)lease.Connection;

                f.Clock.Advance(TimeSpan.FromMinutes(6));
                f.Cache.Sweep();

                Assert.AreEqual(0, f.Cache.CachedEntryCount, "the entry must be unpublished");
                Assert.AreEqual(0, connection.DisposeCount,
                    "eviction must NOT close a connection that a live lease is publishing on — " +
                    "that is an ObjectDisposedException mid-publish, worse than the leak being avoided");
                Assert.IsTrue(lease.Connection.IsOpen, "the held lease must remain usable");
            }

            Assert.AreEqual(1, f.Created[0].DisposeCount,
                "and the last release must then close it, exactly once");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_DoubleDisposedLease_DoesNotDoubleReleaseTheReference()
        {
            var f = new Fixture();

            var lease = f.Cache.Acquire(KeyA, f.NewConnection);
            var connection = (FakeConnection)lease.Connection;
            lease.Dispose();
            lease.Dispose();   // a using block plus an error path can both fire

            Assert.AreEqual(0, connection.DisposeCount, "the entry is still cached and must stay alive");

            using (var again = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.AreSame(connection, again.Connection,
                    "a double release must not have driven the reference count below zero");
            }
        }

        // ── Sweep ───────────────────────────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_Sweep_ReleasesIdleSocketsAtTheirTtl()
        {
            var f = new Fixture(idle: TimeSpan.FromMinutes(5));

            using (var l = f.Cache.Acquire(KeyA, f.NewConnection))
            {
                Assert.IsNotNull(l.Connection);
            }
            Assert.AreEqual(1, f.Cache.CachedEntryCount);

            f.Clock.Advance(TimeSpan.FromMinutes(6));
            f.Cache.Sweep();

            Assert.AreEqual(0, f.Cache.CachedEntryCount,
                "an idle socket must be released at its TTL, not held until the instance recycles");
            Assert.AreEqual(1, f.Created[0].DisposeCount);
        }

        // ── T14: the leak soak ──────────────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(120000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_TenThousandPublishCycles_CreateExactlyOneConnectionAndLeakNothing()
        {
            const int cycles = 10000;
            var f = new Fixture();

            for (var i = 0; i < cycles; i++)
            {
                using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
                {
                    lease.MarkTopologyDeclared("queue-1");
                }
            }

            // This is the whole leak argument, asserted rather than reasoned about.
            Assert.AreEqual(1, f.Cache.ConnectionsCreated, "10 000 publishes must cost ONE handshake");
            Assert.AreEqual(0, f.Cache.ConnectionsDisposed, "nothing should have been churned");
            Assert.AreEqual(1, f.Cache.CachedEntryCount, "the entry map must not grow per publish");
            Assert.AreEqual(0, f.Cache.UncachedFallbacks);
            Assert.AreEqual(1, f.TopologyCount(KeyA), "one queue name must occupy one memo slot, not 10 000");

            f.Cache.Dispose();
            Assert.AreEqual(1, f.Cache.ConnectionsDisposed);
            Assert.AreEqual(1, f.Created.Count, "exactly one connection object was ever built");
        }

        // ── T19: the refcount invariant under concurrency ───────────────────────────────────────

        [TestMethod]
        [Timeout(120000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(LightweightRabbitMqConnectionCache))]
        public void Cache_ConcurrentAcquireWhileSweeping_NeverDisposesAConnectionInUse()
        {
            // The race the reference count exists for: a sweep deciding an entry is stale while other
            // threads are mid-publish on it. Getting this wrong is an ObjectDisposedException inside
            // a live publish, not a leak.
            var f = new Fixture(idle: TimeSpan.FromMilliseconds(1));
            var failures = new List<string>();
            var stop = false;

            var workers = new Task[8];
            for (var w = 0; w < workers.Length; w++)
            {
                workers[w] = Task.Run(() =>
                {
                    while (!Volatile.Read(ref stop))
                    {
                        using (var lease = f.Cache.Acquire(KeyA, f.NewConnection))
                        {
                            var connection = (FakeConnection)lease.Connection;

                            // Held leases must stay usable for their whole lifetime, whatever the
                            // sweeper concludes in the meantime.
                            if (connection.DisposeCount != 0)
                            {
                                lock (failures)
                                {
                                    failures.Add("a lease was handed a DISPOSED connection");
                                }
                            }

                            Thread.Yield();

                            if (connection.DisposeCount != 0)
                            {
                                lock (failures)
                                {
                                    failures.Add("a connection was disposed while its lease was live");
                                }
                            }
                        }
                    }
                });
            }

            var sweeper = Task.Run(() =>
            {
                while (!Volatile.Read(ref stop))
                {
                    f.Clock.Advance(TimeSpan.FromMilliseconds(5));
                    f.Cache.Sweep();
                    Thread.Yield();
                }
            });

            Thread.Sleep(2000);
            Volatile.Write(ref stop, true);
            Task.WaitAll(workers);
            sweeper.Wait();

            CollectionAssert.AreEqual(new string[0], failures.ToArray(),
                failures.Count == 0 ? "" : string.Join("; ", failures));

            f.Cache.Dispose();

            // Nothing may be disposed twice (throws on a real connection) and nothing may be missed.
            foreach (var c in f.Created)
            {
                Assert.AreEqual(1, c.DisposeCount,
                    "every connection created during the race must be disposed exactly once");
            }
        }

        // ── Fixtures and fakes ──────────────────────────────────────────────────────────────────

        sealed class TestClock
        {
            DateTimeOffset _now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            readonly object _gate = new object();

            internal DateTimeOffset Now
            {
                get { lock (_gate) { return _now; } }
            }

            internal void Advance(TimeSpan by)
            {
                lock (_gate) { _now = _now.Add(by); }
            }
        }

        sealed class Fixture
        {
            internal readonly TestClock Clock = new TestClock();
            internal readonly List<FakeConnection> Created = new List<FakeConnection>();
            internal readonly LightweightRabbitMqConnectionCache Cache;
            readonly object _gate = new object();

            internal Fixture(
                TimeSpan? idle = null,
                TimeSpan? lifetime = null,
                TimeSpan? topologyTtl = null,
                int maxConnections = 4)
            {
                var json =
                    "{\"enabled\":true," +
                    "\"idleTimeout\":\"" + Format(idle ?? TimeSpan.FromMinutes(5)) + "\"," +
                    "\"maxLifetime\":\"" + Format(lifetime ?? TimeSpan.FromMinutes(30)) + "\"," +
                    "\"topologyTtl\":\"" + Format(topologyTtl ?? TimeSpan.FromSeconds(30)) + "\"," +
                    "\"maxConnections\":" + maxConnections + "}";

                var options = RabbitMqCacheOptions.FromJson(json);
                Assert.IsTrue(options.Enabled, "fixture options must parse: " + json);

                // enableSweepTimer:false — the sweep is driven explicitly so no background callback
                // can fire in the middle of an assertion.
                Cache = new LightweightRabbitMqConnectionCache(options, () => Clock.Now, enableSweepTimer: false);
            }

            static string Format(TimeSpan value) =>
                (value < TimeSpan.Zero ? "-" : "") + value.Duration().ToString("hh\\:mm\\:ss");

            internal IConnection NewConnection()
            {
                var connection = new FakeConnection();
                lock (_gate)
                {
                    Created.Add(connection);
                }
                return connection;
            }

            /// <summary>
            /// Reads the live memo size for a key by acquiring a lease and probing it, so the test
            /// asserts against observable behaviour rather than reaching into private state.
            /// </summary>
            internal int TopologyCount(string key)
            {
                var count = 0;
                using (var lease = Cache.Acquire(key, NewConnection))
                {
                    for (var i = 0; i < LightweightRabbitMqConnectionCache.MaxTopologyEntriesPerConnection * 4; i++)
                    {
                        if (lease.IsTopologyFresh("queue-" + i))
                        {
                            count++;
                        }
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// In-memory <see cref="IConnection"/>. Only the members the cache actually touches
        /// (<see cref="IsOpen"/>, <see cref="Dispose"/>) carry behaviour; everything else throws so
        /// that any unexpected reliance on this fake shows up as a failure rather than a silent
        /// default.
        /// </summary>
        sealed class FakeConnection : IConnection
        {
            internal int DisposeCount;
            internal volatile bool Open = true;
            internal volatile bool ThrowOnIsOpen;

            public bool IsOpen
            {
                get
                {
                    if (ThrowOnIsOpen)
                    {
                        throw new InvalidOperationException("connection is broken");
                    }
                    return Open;
                }
            }

            public void Dispose()
            {
                Interlocked.Increment(ref DisposeCount);
                Open = false;
            }

            public IModel CreateModel() => throw new NotSupportedException("the cache never opens channels");

            public void Abort() => Dispose();
            public void Abort(ushort reasonCode, string reasonText) => Dispose();
            public void Abort(int timeout) => Dispose();
            public void Abort(ushort reasonCode, string reasonText, int timeout) => Dispose();
            public void Close() => Dispose();
            public void Close(ushort reasonCode, string reasonText) => Dispose();
            public void Close(int timeout) => Dispose();
            public void Close(ushort reasonCode, string reasonText, int timeout) => Dispose();

            public void HandleConnectionBlocked(string reason) => throw new NotSupportedException();
            public void HandleConnectionUnblocked() => throw new NotSupportedException();

            public bool AutoClose { get; set; }
            public ushort ChannelMax => 0;
            public IDictionary<string, object> ClientProperties => new Dictionary<string, object>();
            public ShutdownEventArgs CloseReason => null;
            public AmqpTcpEndpoint Endpoint => null;
            public uint FrameMax => 0;
            public ushort Heartbeat => 0;
            public AmqpTcpEndpoint[] KnownHosts => new AmqpTcpEndpoint[0];
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
    }
}
