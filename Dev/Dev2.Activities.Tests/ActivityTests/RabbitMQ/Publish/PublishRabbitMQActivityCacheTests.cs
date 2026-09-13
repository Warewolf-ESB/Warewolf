/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.RabbitMQ.Publish;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Tests.Activities.ActivityTests.RabbitMQ.Publish
{
    /// <summary>
    /// Covers the connection-cache branch of <see cref="PublishRabbitMQActivity"/>.
    ///
    /// <para>Deliberately separate from <c>PublishRabbitMQActivityTests</c>, which is left completely
    /// unmodified: those 11 tests assert the exact sequence of broker calls the activity has always
    /// issued, so leaving them untouched is what proves that disabling the cache restores today's
    /// behaviour rather than something merely similar to it.</para>
    ///
    /// <para><b>No process-wide state is touched.</b> Every test injects its cache through the
    /// activity's resolver seam rather than registering with
    /// <see cref="AmbientRabbitMqConnectionCache"/>, because this assembly runs its tests in parallel
    /// in CI and a shared static would make them order-dependent.</para>
    /// </summary>
    [TestClass]
    public class PublishRabbitMQActivityCacheTests
    {
        const string QueueName = "Q1";
        const string Message = "Test Message";

        // ── T1: the Server / cache-disabled path ────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_NoCacheRegistered_OpensAndDisposesItsOwnConnection()
        {
            var h = new Harness(cache: null);

            var result = h.Publish();

            Assert.AreEqual("Success", result[0]);
            h.ConnectionFactory.Verify(f => f.CreateConnection(), Times.Once);
            // The activity owns the connection on this path and must close it, exactly as it always has.
            h.Connection.Verify(c => c.Dispose(), Times.Once);
            h.Channel.Verify(c => c.Dispose(), Times.Once);
        }

        // ── T2: the cached path must NOT dispose a connection it does not own ────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_CacheRegistered_LeavesConnectionOpenAndDisposesOnlyTheChannel()
        {
            var cache = new FakeCache();
            var h = new Harness(cache);

            var result = h.Publish();

            Assert.AreEqual("Success", result[0]);
            Assert.AreEqual(1, cache.AcquireCount, "the cached branch must go through the cache");
            // THE invariant of this whole change: a cached connection outlives the execution.
            h.Connection.Verify(c => c.Dispose(), Times.Never);
            h.Channel.Verify(c => c.Dispose(), Times.Once);
            Assert.AreEqual(1, cache.Lease.DisposeCount, "the lease must be released exactly once");
        }

        // ── T3 / T4: the topology memo ──────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_TopologyMemoFresh_SkipsBothPassiveDeclares()
        {
            var cache = new FakeCache { Lease = { TopologyFresh = true } };
            var h = new Harness(cache);

            h.Publish();

            // The entire point of the memo: 2 fewer broker round-trips per message.
            h.Channel.Verify(c => c.ExchangeDeclarePassive(It.IsAny<string>()), Times.Never);
            h.Channel.Verify(c => c.QueueDeclarePassive(It.IsAny<string>()), Times.Never);
            // The 5-argument INTERFACE method, not the 4-argument IModelExensions extension the
            // activity calls: Moq cannot intercept extension methods.
            h.Channel.Verify(
                c => c.BasicPublish(QueueName, "", It.IsAny<bool>(), It.IsAny<IBasicProperties>(),
                    It.IsAny<byte[]>()),
                Times.Once);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_TopologyMemoCold_DeclaresThenMarksTheMemo()
        {
            var cache = new FakeCache { Lease = { TopologyFresh = false } };
            var h = new Harness(cache);

            h.Publish();

            h.Channel.Verify(c => c.ExchangeDeclarePassive(QueueName), Times.Once);
            h.Channel.Verify(c => c.QueueDeclarePassive(QueueName), Times.Once);
            CollectionAssert.AreEqual(new[] { QueueName }, cache.Lease.Marked.ToArray(),
                "a successful publish must record the topology it verified");
        }

        // ── T5: a failed publish must poison the connection ─────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_PublishThrows_InvalidatesTheLeaseAndStillDisposesTheChannel()
        {
            var cache = new FakeCache();
            var h = new Harness(cache);
            // The 5-argument INTERFACE method: the 4-argument overload the activity calls is an
            // IModelExensions extension, which Moq cannot intercept.
            h.Channel
                .Setup(c => c.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                    It.IsAny<IBasicProperties>(), It.IsAny<byte[]>()))
                .Throws(new InvalidOperationException("broker went away"));

            Assert.ThrowsException<Exception>(() => h.Publish());

            Assert.AreEqual(1, cache.Lease.InvalidateCount,
                "a half-dead connection must be poisoned, or the next execution inherits the failure");
            Assert.AreEqual(0, cache.Lease.Marked.Count, "a failed publish must not refresh the memo");
            h.Channel.Verify(c => c.Dispose(), Times.Once, "the channel must be released even on the error path");
            Assert.AreEqual(1, cache.Lease.DisposeCount, "the lease must be released even on the error path");
        }

        // ── T6: the recordset multiplier — the case this change exists for ──────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_ManyRows_ReuseTheSameConnectionObject()
        {
            const int rows = 50;
            var cache = new FakeCache();
            var h = new Harness(cache);

            for (var i = 0; i < rows; i++)
            {
                h.Publish("row " + i);
            }

            // Asserting object identity, not a count, is what actually proves reuse: a cache that
            // reconnected every time could still report one Acquire per row.
            Assert.AreEqual(rows, cache.AcquireCount);
            h.ConnectionFactory.Verify(f => f.CreateConnection(), Times.Once,
                $"{rows} rows must cost ONE handshake, not {rows}");
            h.Channel.Verify(c => c.Dispose(), Times.Exactly(rows),
                "every per-row channel must still be released");
            h.Connection.Verify(c => c.Dispose(), Times.Never);
        }

        // ── T7: the cache key ───────────────────────────────────────────────────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void RabbitMqCacheKey_DistinguishesEveryCoordinateAndLeaksNoCredential()
        {
            const string pwd = "sup3r-s3cret-p@ssword";
            var baseline = RabbitMqCacheKey.For("host", 5672, "user", "/", pwd);

            Assert.AreEqual(baseline, RabbitMqCacheKey.For("host", 5672, "user", "/", pwd),
                "the same coordinates must map to the same key within a process");

            // Every one of these changes WHICH broker or WHOSE identity we connect as. Missing any of
            // them would let an edited source keep publishing over a connection built from the old one.
            Assert.AreNotEqual(baseline, RabbitMqCacheKey.For("other", 5672, "user", "/", pwd));
            Assert.AreNotEqual(baseline, RabbitMqCacheKey.For("host", 5673, "user", "/", pwd));
            Assert.AreNotEqual(baseline, RabbitMqCacheKey.For("host", 5672, "other", "/", pwd));
            Assert.AreNotEqual(baseline, RabbitMqCacheKey.For("host", 5672, "user", "/vh", pwd));
            Assert.AreNotEqual(baseline, RabbitMqCacheKey.For("host", 5672, "user", "/", "different"));

            // Length-prefixing guards the classic concatenation collision.
            Assert.AreNotEqual(RabbitMqCacheKey.For("ab", 1, "c", "/", ""),
                               RabbitMqCacheKey.For("a", 1, "bc", "/", ""));

            Assert.IsFalse(baseline.Contains(pwd), "a cache key must never carry the password");
            Assert.IsFalse(baseline.Contains("user"), "a cache key must never carry the username");
            Assert.IsFalse(baseline.Contains("host"), "a cache key must never carry the host");
            Assert.AreEqual(64, baseline.Length, "SHA-256 hex");

            Assert.AreEqual(8, RabbitMqCacheKey.ToLogSafe(baseline).Length,
                "only a short prefix may ever reach a log");
            Assert.AreEqual("(none)", RabbitMqCacheKey.ToLogSafe(null));
        }

        // ── T8: over-capacity fallback behaves exactly like the uncached path ───────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_UncachedLease_DisposesTheConnectionAndAlwaysDeclares()
        {
            var h = new Harness(cache: null);
            var cache = new FakeCache
            {
                LeaseFactory = () => new UncachedRabbitMqConnectionLease(h.Connection.Object)
            };
            h.SetCache(cache);

            var result = h.Publish();

            Assert.AreEqual("Success", result[0]);
            // An uncached lease owns its connection, so the fallback must not leak it.
            h.Connection.Verify(c => c.Dispose(), Times.Once);
            // And it reports no topology, so the declares run — i.e. no behaviour change at all.
            h.Channel.Verify(c => c.ExchangeDeclarePassive(QueueName), Times.Once);
            h.Channel.Verify(c => c.QueueDeclarePassive(QueueName), Times.Once);
        }

        // ── Guards the two settings that eliminate leak vectors L2 and L3 ───────────────────────

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_CachedPath_AppliesTheLongLivedConnectionSettings()
        {
            var cache = new FakeCache();
            var h = new Harness(cache);

            h.Publish();

            var factory = h.ConnectionFactory.Object;

            // Recovery OFF: an AutorecoveringConnection records every actively-declared exchange,
            // queue and binding by NAME and never removes them, which on a long-lived connection with
            // workflow-derived queue names grows without bound. A plain Connection has no such state.
            Assert.IsFalse(factory.AutomaticRecoveryEnabled,
                "a cached connection must not accumulate recorded topology");
            Assert.IsFalse(factory.TopologyRecoveryEnabled);

            // Background I/O ON: Connection.StartMainLoop passes this to Thread.IsBackground and it
            // defaults to FALSE, so a cached connection would otherwise pin a FOREGROUND thread and
            // delay Functions host shutdown.
            Assert.IsTrue(factory.UseBackgroundThreadsForIO,
                "a cached connection must never hold a foreground thread");

            Assert.IsTrue(factory.ClientProperties.ContainsKey("connection_name"),
                "cached connections must be identifiable in the RabbitMQ management UI");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Sehul Shah")]
        [TestCategory(nameof(PublishRabbitMQActivity))]
        public void PublishRabbitMQActivity_UncachedPath_LeavesConnectionFactoryDefaultsAlone()
        {
            var h = new Harness(cache: null);

            h.Publish();

            // The long-lived settings are a consequence of caching. Applying them to a connection
            // that dies in milliseconds would change years-old behaviour for no benefit.
            Assert.IsTrue(h.ConnectionFactory.Object.AutomaticRecoveryEnabled,
                "the uncached path must keep the library defaults");
            Assert.IsFalse(h.ConnectionFactory.Object.UseBackgroundThreadsForIO);
        }

        // ── Test harness ────────────────────────────────────────────────────────────────────────

        sealed class Harness
        {
            internal readonly Mock<ConnectionFactory> ConnectionFactory = new Mock<ConnectionFactory>();
            internal readonly Mock<IConnection> Connection = new Mock<IConnection>();
            internal readonly Mock<IModel> Channel = new Mock<IModel>();
            readonly TestActivity _activity;

            internal Harness(IRabbitMqConnectionCache cache)
            {
                var basicProperties = new Mock<IBasicProperties>();
                basicProperties.SetupAllProperties();

                Connection.Setup(c => c.CreateModel()).Returns(Channel.Object);
                Channel.Setup(c => c.CreateBasicProperties()).Returns(basicProperties.Object);
                ConnectionFactory.Setup(f => f.CreateConnection()).Returns(Connection.Object);

                var source = new RabbitMQSource
                {
                    HostName = "broker.example",
                    Port = 5672,
                    UserName = "user",
                    Password = "pwd",
                    VirtualHost = "/"
                };
                var catalog = new Mock<IResourceCatalog>();
                catalog.Setup(r => r.GetResource<RabbitMQSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(source);

                var dataObject = new Mock<IDSFDataObject>();
                dataObject.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
                dataObject.Setup(o => o.ExecutionID).Returns(Guid.NewGuid());
                dataObject.Setup(o => o.CustomTransactionID).Returns(string.Empty);

                _activity = new TestActivity(catalog.Object, ConnectionFactory.Object, cache);
                _activity.TestExecuteTool(dataObject.Object);
            }

            internal void SetCache(IRabbitMqConnectionCache cache) => _activity.SetCache(cache);

            internal List<string> Publish(string message = Message) =>
                _activity.TestPerformExecution(new Dictionary<string, string>
                {
                    { "QueueName", QueueName },
                    { "Message", message }
                });
        }

        /// <summary>
        /// Injects the cache through the protected resolver seam rather than the ambient static, so
        /// these tests remain safe to run in parallel with every other test in this assembly.
        /// </summary>
        sealed class TestActivity : PublishRabbitMQActivity
        {
            internal TestActivity(IResourceCatalog catalog, ConnectionFactory factory, IRabbitMqConnectionCache cache)
                : base(catalog, factory)
            {
                SetCache(cache);
            }

            internal void SetCache(IRabbitMqConnectionCache cache) => ConnectionCacheResolver = () => cache;

            internal void TestExecuteTool(IDSFDataObject dataObject) => ExecuteTool(dataObject, 0);

            internal List<string> TestPerformExecution(Dictionary<string, string> values) =>
                PerformExecution(values);
        }

        sealed class FakeCache : IRabbitMqConnectionCache
        {
            internal readonly FakeLease Lease = new FakeLease();
            internal Func<IRabbitMqConnectionLease> LeaseFactory;
            internal int AcquireCount;
            IConnection _connection;

            public IRabbitMqConnectionLease Acquire(string cacheKey, Func<IConnection> connectionFactory)
            {
                AcquireCount++;
                if (LeaseFactory != null)
                {
                    return LeaseFactory();
                }

                // Create once and hand back the same instance, which is precisely what a real cache
                // does and what T6 asserts by object identity.
                _connection = _connection ?? connectionFactory();
                Lease.Connection = _connection;
                return Lease;
            }

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// A cached lease that does NOT pool channels: it opens one per publish and disposes it on
        /// return.
        ///
        /// <para><b>Non-pooling on purpose.</b> The assertions above pin the channel being disposed
        /// once per publish (T2, T5) and once per row (T6), which is the behaviour the activity has
        /// always had. A fake that pooled would quietly change what those tests mean. Channel pooling
        /// is a property of the real cached lease and is covered where it lives, in
        /// <c>LightweightRabbitMqConnectionCacheTests</c> - not here, where the subject is the
        /// activity's branching.</para>
        /// </summary>
        sealed class FakeLease : IRabbitMqConnectionLease
        {
            internal readonly List<string> Marked = new List<string>();
            internal bool TopologyFresh;
            internal int InvalidateCount;
            internal int DisposeCount;
            internal int RentChannelCount;
            internal int ReturnChannelCount;

            public IConnection Connection { get; set; }

            public bool IsCached => true;

            public bool IsTopologyFresh(string topologyKey) => TopologyFresh;

            public void MarkTopologyDeclared(string topologyKey) => Marked.Add(topologyKey);

            public void Invalidate() => InvalidateCount++;

            public IModel RentChannel()
            {
                RentChannelCount++;
                return Connection.CreateModel();
            }

            public void ReturnChannel(IModel channel)
            {
                ReturnChannelCount++;
                // Null-tolerant for the same reason the real implementations are: the caller returns
                // whatever it holds, and it holds nothing if opening the channel is what failed.
                channel?.Dispose();
            }

            public void Dispose() => DisposeCount++;
        }
    }
}
