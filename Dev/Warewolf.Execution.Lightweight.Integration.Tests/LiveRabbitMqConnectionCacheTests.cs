/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Broker-gated live verification of the RabbitMQ publish connection cache, for the E2E harness.
    ///
    /// <para><b>What this proves that the unit tests cannot.</b> The unit suites run against fakes, so
    /// they pin the cache's logic but say nothing about a real AMQP connection: that a connection built
    /// with <c>AutomaticRecoveryEnabled=false</c> and <c>UseBackgroundThreadsForIO=true</c> actually
    /// connects, that it survives many publishes and real heartbeat traffic without being dropped, that
    /// a channel opened and closed per publish against a live broker leaks nothing the broker notices,
    /// and that the messages genuinely arrive.</para>
    ///
    /// <para><b>What it deliberately does NOT cover.</b> It drives the cache and the exact AMQP sequence
    /// <c>PublishRabbitMQActivity.PublishCore</c> issues, not the activity type itself — the activity
    /// needs an <c>IResourceCatalog</c> and an <c>IDSFDataObject</c>, and this project has no mocking
    /// package. The activity's own branching is covered by
    /// <c>PublishRabbitMQActivityCacheTests</c> in <c>Dev2.Activities.Tests</c>.</para>
    ///
    /// <para><b>Credentials are never committed.</b> The broker is supplied through the
    /// <c>WAREWOLF_E2E_RABBITMQ</c> environment variable in the Warewolf source format
    /// (<c>HostName=…;Port=…;UserName=…;Password=…;VirtualHost=…</c>). With the variable absent — which
    /// is every CI run — each test reports <b>Inconclusive</b> with the command to run it, rather than
    /// failing.</para>
    /// </summary>
    [TestClass]
    [TestCategory("LiveBroker")]
    public class LiveRabbitMqConnectionCacheTests
    {
        /// <summary>The queue, exchange and binding this verification owns. Created if absent.</summary>
        const string QueueName = "Test_Connection_Queue_For_PublishActivity";

        const string EnvVar = "WAREWOLF_E2E_RABBITMQ";
        const int PublishCount = 20;

        static BrokerSettings RequireBroker()
        {
            var raw = Environment.GetEnvironmentVariable(EnvVar);
            if (string.IsNullOrWhiteSpace(raw))
            {
                Assert.Inconclusive(
                    $"No live broker configured, so this verification was skipped (it did NOT pass).{Environment.NewLine}" +
                    $"To run it, set {EnvVar} to a Warewolf RabbitMQ source connection string and re-run:{Environment.NewLine}" +
                    $"  $env:{EnvVar} = 'HostName=<host>;Port=5672;UserName=<user>;Password=<pwd>;VirtualHost=<vhost>'{Environment.NewLine}" +
                    $"  dotnet test Warewolf.Execution.Lightweight.Integration.Tests --filter TestCategory=LiveBroker");
            }

            return BrokerSettings.Parse(raw);
        }

        /// <summary>
        /// Creates the exchange, queue and binding this verification publishes to.
        ///
        /// <para>Not optional, and not something the publish path can do for us on a brand-new queue in
        /// every case: the exchange is <b>direct and named after the queue</b>, and the binding uses an
        /// <b>empty routing key</b>. Without that binding the broker accepts every publish and silently
        /// discards it, which would make this test pass while proving nothing.</para>
        /// </summary>
        static void EnsureTopology(IConnection connection)
        {
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(QueueName, ExchangeType.Direct, durable: true, autoDelete: false,
                    arguments: null);
                channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false,
                    arguments: null);
                channel.QueueBind(QueueName, QueueName, "", new Dictionary<string, object>());
                channel.QueuePurge(QueueName);
            }
        }

        /// <summary>
        /// The exact per-publish sequence <c>PublishRabbitMQActivity.PublishCore</c> performs: open a
        /// channel, verify topology unless the memo says it is fresh, publish persistently with a
        /// correlation id, close the channel.
        /// </summary>
        static void PublishOnce(IConnection connection, string correlationId, IRabbitMqConnectionLease memo)
        {
            var topologyFresh = memo != null && memo.IsTopologyFresh(QueueName);

            var channel = connection.CreateModel();
            try
            {
                if (!topologyFresh)
                {
                    try
                    {
                        channel.ExchangeDeclarePassive(QueueName);
                    }
                    catch (OperationInterruptedException)
                    {
                        channel.Dispose();
                        channel = connection.CreateModel();
                        channel.ExchangeDeclare(QueueName, ExchangeType.Direct, true, false, null);
                    }

                    try
                    {
                        channel.QueueDeclarePassive(QueueName);
                    }
                    catch (OperationInterruptedException)
                    {
                        channel.Dispose();
                        channel = connection.CreateModel();
                        channel.QueueDeclare(QueueName, true, false, false, null);
                    }
                }

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.CorrelationId = correlationId;
                channel.BasicPublish(QueueName, "", properties, Encoding.UTF8.GetBytes("cache-verification " + correlationId));

                memo?.MarkTopologyDeclared(QueueName);
            }
            finally
            {
                channel?.Dispose();
            }
        }

        static List<string> DrainCorrelationIds(IConnection connection, int expected)
        {
            var seen = new List<string>();
            using (var channel = connection.CreateModel())
            {
                // A generous ceiling rather than a fixed count: BasicGet returns null the moment the
                // queue is empty, and stopping early would hide a message that never arrived.
                for (var i = 0; i < expected * 4; i++)
                {
                    var result = channel.BasicGet(QueueName, autoAck: true);
                    if (result is null)
                    {
                        break;
                    }
                    seen.Add(result.BasicProperties?.CorrelationId ?? "(none)");
                }
            }
            return seen;
        }

        [TestMethod]
        [Timeout(300000)]
        [Owner("Sehul Shah")]
        [TestCategory("LiveBroker")]
        public void LiveBroker_CachedPublishing_UsesOneConnectionAndDeliversEveryMessage()
        {
            var broker = RequireBroker();
            var options = RabbitMqCacheOptions.FromJson("{\"enabled\":true,\"topologyTtl\":\"00:00:30\"}");
            Assert.IsTrue(options.Enabled);

            using (var cache = new LightweightRabbitMqConnectionCache(options))
            using (var control = broker.CreateFactory("wwlw-verify-control").CreateConnection())
            {
                EnsureTopology(control);

                var key = RabbitMqCacheKey.For(broker.HostName, broker.Port, broker.UserName,
                    broker.VirtualHost, broker.Password);
                var sent = new List<string>();

                for (var i = 0; i < PublishCount; i++)
                {
                    var correlationId = "cached-" + i + "-" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    using (var lease = cache.Acquire(key, () => broker.CreateFactory("wwlw-verify-cached").CreateConnection()))
                    {
                        Assert.IsTrue(lease.IsCached, "publish " + i + " should have been served from the cache");
                        Assert.IsTrue(lease.Connection.IsOpen,
                            "the cached connection must still be open at publish " + i +
                            " — if it is not, recovery-off plus heartbeats is not surviving real traffic");

                        PublishOnce(lease.Connection, correlationId, lease);
                    }
                    sent.Add(correlationId);
                }

                // THE claim of the whole change, measured against a real broker.
                Assert.AreEqual(1, cache.ConnectionsCreated,
                    $"{PublishCount} publishes must cost ONE AMQP handshake");
                Assert.AreEqual(0, cache.ConnectionsDisposed, "nothing should have been churned mid-run");
                Assert.AreEqual(0, cache.UncachedFallbacks);

                var received = DrainCorrelationIds(control, PublishCount);

                // Reuse is worthless if messages stop arriving — and an unbound exchange would discard
                // them silently while every other assertion above still passed.
                CollectionAssert.AreEquivalent(sent, received,
                    $"expected all {PublishCount} messages to arrive; got {received.Count}");
            }
        }

        [TestMethod]
        [Timeout(300000)]
        [Owner("Sehul Shah")]
        [TestCategory("LiveBroker")]
        public void LiveBroker_CachedPublishing_IsFasterThanOpeningAConnectionPerPublish()
        {
            var broker = RequireBroker();

            using (var control = broker.CreateFactory("wwlw-verify-control").CreateConnection())
            {
                EnsureTopology(control);

                // Baseline: a full TCP + AMQP handshake per message, as the uncached path performs.
                // It uses the same factory settings as the cached run so that CONNECTION REUSE is the
                // only variable being measured, rather than confounding it with the recovery flags.
                var uncached = Stopwatch.StartNew();
                for (var i = 0; i < PublishCount; i++)
                {
                    using (var connection = broker.CreateFactory("wwlw-verify-uncached").CreateConnection())
                    {
                        PublishOnce(connection, "uncached-" + i, null);
                    }
                }
                uncached.Stop();

                var options = RabbitMqCacheOptions.FromJson("{\"enabled\":true,\"topologyTtl\":\"00:00:30\"}");
                var cachedElapsed = TimeSpan.Zero;

                using (var cache = new LightweightRabbitMqConnectionCache(options))
                {
                    var key = RabbitMqCacheKey.For(broker.HostName, broker.Port, broker.UserName,
                        broker.VirtualHost, broker.Password);

                    // Warm the connection and the topology memo first, so the measurement is of the
                    // steady state rather than of the one-off handshake.
                    using (var lease = cache.Acquire(key, () => broker.CreateFactory("wwlw-verify-cached").CreateConnection()))
                    {
                        PublishOnce(lease.Connection, "warmup", lease);
                    }

                    var cached = Stopwatch.StartNew();
                    for (var i = 0; i < PublishCount; i++)
                    {
                        using (var lease = cache.Acquire(key, () => broker.CreateFactory("wwlw-verify-cached").CreateConnection()))
                        {
                            PublishOnce(lease.Connection, "cached-" + i, lease);
                        }
                    }
                    cached.Stop();
                    cachedElapsed = cached.Elapsed;

                    Assert.AreEqual(1, cache.ConnectionsCreated);
                }

                Console.WriteLine(
                    $"[RabbitMQ cache] {PublishCount} publishes to {broker.HostName}: " +
                    $"uncached {uncached.ElapsedMilliseconds} ms, cached {cachedElapsed.TotalMilliseconds:F0} ms " +
                    $"({uncached.Elapsed.TotalMilliseconds / Math.Max(1, cachedElapsed.TotalMilliseconds):F1}x).");

                DrainCorrelationIds(control, PublishCount * 2);

                // Against a remote broker this is ~9 round-trips per message versus ~2, so the margin is
                // large. Asserted as a plain inequality rather than a ratio so a slow network or a busy
                // agent cannot turn a real improvement into a red build.
                Assert.IsTrue(cachedElapsed < uncached.Elapsed,
                    $"cached publishing ({cachedElapsed.TotalMilliseconds:F0} ms) should beat a connection " +
                    $"per publish ({uncached.ElapsedMilliseconds} ms)");
            }
        }

        [TestMethod]
        [Timeout(300000)]
        [Owner("Sehul Shah")]
        [TestCategory("LiveBroker")]
        public void LiveBroker_CachedConnection_SurvivesManyChannelsWithoutBrokerSideGrowth()
        {
            var broker = RequireBroker();
            var options = RabbitMqCacheOptions.FromJson("{\"enabled\":true}");

            using (var cache = new LightweightRabbitMqConnectionCache(options))
            using (var control = broker.CreateFactory("wwlw-verify-control").CreateConnection())
            {
                EnsureTopology(control);

                var key = RabbitMqCacheKey.For(broker.HostName, broker.Port, broker.UserName,
                    broker.VirtualHost, broker.Password);

                // 200 channels opened and closed on ONE connection. A channel that was not released
                // would exhaust the negotiated channel-max and throw here, which is the live
                // counterpart of the 10 000-cycle in-memory soak.
                const int channels = 200;
                for (var i = 0; i < channels; i++)
                {
                    using (var lease = cache.Acquire(key, () => broker.CreateFactory("wwlw-verify-channels").CreateConnection()))
                    {
                        PublishOnce(lease.Connection, "channel-" + i, lease);
                        Assert.IsTrue(lease.Connection.IsOpen, "connection died at channel " + i);
                    }
                }

                Assert.AreEqual(1, cache.ConnectionsCreated,
                    $"{channels} channels must have run over a single connection");

                var received = DrainCorrelationIds(control, channels);
                Assert.AreEqual(channels, received.Count,
                    "every publish over the reused connection must still have been delivered");
            }
        }

        /// <summary>Broker coordinates parsed from the Warewolf source connection-string format.</summary>
        sealed class BrokerSettings
        {
            internal string HostName { get; private set; } = "";
            internal int Port { get; private set; } = 5672;
            internal string UserName { get; private set; } = "";
            internal string Password { get; private set; } = "";
            internal string VirtualHost { get; private set; } = "/";

            internal static BrokerSettings Parse(string connectionString)
            {
                var settings = new BrokerSettings();
                foreach (var part in connectionString.Split(';'))
                {
                    var split = part.Split(new[] { '=' }, 2);
                    if (split.Length != 2)
                    {
                        continue;
                    }

                    var name = split[0].Trim();
                    var value = split[1].Trim();

                    if (name.Equals("HostName", StringComparison.OrdinalIgnoreCase)) settings.HostName = value;
                    else if (name.Equals("UserName", StringComparison.OrdinalIgnoreCase)) settings.UserName = value;
                    else if (name.Equals("Password", StringComparison.OrdinalIgnoreCase)) settings.Password = value;
                    else if (name.Equals("VirtualHost", StringComparison.OrdinalIgnoreCase)) settings.VirtualHost = value;
                    else if (name.Equals("Port", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var port))
                    {
                        settings.Port = port;
                    }
                }

                Assert.IsFalse(string.IsNullOrWhiteSpace(settings.HostName),
                    EnvVar + " did not contain a HostName");
                return settings;
            }

            internal ConnectionFactory CreateFactory(string clientName)
            {
                var factory = new ConnectionFactory
                {
                    HostName = HostName,
                    Port = Port,
                    UserName = UserName,
                    Password = Password,
                    VirtualHost = VirtualHost
                };

                // The production settings, applied by the same shared helper the activity calls, so
                // this verification cannot drift from what actually ships.
                RabbitMqCachedConnectionSettings.Apply(factory, clientName);
                return factory;
            }
        }
    }
}
