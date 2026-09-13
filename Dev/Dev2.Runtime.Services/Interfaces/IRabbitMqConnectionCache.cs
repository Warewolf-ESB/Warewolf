/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

namespace Dev2.Runtime.Interfaces
{
    /// <summary>
    /// A lease on a (possibly cached) AMQP connection, held for the duration of ONE publish.
    ///
    /// <para><b>Disposing a lease does not dispose a cached connection</b> — the cache owns that and
    /// disposes it only when the last lease has been released and the entry has been evicted. An
    /// <i>uncached</i> lease (returned when the cache is over capacity) owns its connection and does
    /// dispose it, so the caller can treat every lease identically.</para>
    /// </summary>
    public interface IRabbitMqConnectionLease : IDisposable
    {
        /// <summary>The live connection. Never null for a lease that was handed out.</summary>
        IConnection Connection { get; }

        /// <summary>
        /// False when this lease wraps a throwaway connection (cache over capacity, or the cache
        /// failed). Callers do not need to branch on it — it exists for diagnostics and tests.
        /// </summary>
        bool IsCached { get; }

        /// <summary>
        /// True when <paramref name="topologyKey"/> was declared recently enough that the passive
        /// exchange/queue declares can be skipped. Always false for an uncached lease, so an
        /// uncached publish behaves exactly as it does with no cache at all.
        /// </summary>
        bool IsTopologyFresh(string topologyKey);

        /// <summary>
        /// Records that <paramref name="topologyKey"/>'s exchange, queue and binding were verified
        /// against the broker just now. Called only after a successful publish.
        /// </summary>
        void MarkTopologyDeclared(string topologyKey);

        /// <summary>
        /// Marks the underlying connection unusable, so it is evicted and never handed out again.
        /// Called when a publish fails: the connection may be half-dead and re-using it would
        /// convert one failure into a run of them.
        /// </summary>
        void Invalidate();
    }

    /// <summary>
    /// Caches AMQP connections across activity executions. Registered by the Azure Function host
    /// (<c>Warewolf.Execution.Lightweight</c>) when <c>WAREWOLF_RABBITMQ_CACHE</c> enables it, and
    /// never registered by the full Warewolf server — see <see cref="AmbientRabbitMqConnectionCache"/>.
    /// </summary>
    public interface IRabbitMqConnectionCache : IDisposable
    {
        /// <summary>
        /// Returns a lease on a connection for <paramref name="cacheKey"/>, creating one through
        /// <paramref name="connectionFactory"/> on a miss.
        ///
        /// <para><b>Never returns null and never throws for a cache-internal reason.</b> If the cache
        /// is full, disposed, or fails for any reason of its own, it returns an <i>uncached</i> lease
        /// wrapping a freshly created connection — i.e. it degrades to exactly the behaviour the
        /// caller would have had with no cache. Only <paramref name="connectionFactory"/> itself
        /// throwing (a genuinely unreachable broker) propagates to the caller.</para>
        /// </summary>
        IRabbitMqConnectionLease Acquire(string cacheKey, Func<IConnection> connectionFactory);
    }

    /// <summary>
    /// Ambient registry for <see cref="IRabbitMqConnectionCache"/>, deliberately identical in shape
    /// to <see cref="AmbientSourceLoader"/>.
    ///
    /// <para>Set once by the Azure Function host at startup when caching is enabled; <b>remains null
    /// on the full Warewolf server and whenever caching is disabled</b>, which is what keeps the
    /// server's publish path byte-for-byte unchanged without a single host-specific conditional in
    /// <c>Dev2.Activities</c>.</para>
    /// </summary>
    public static class AmbientRabbitMqConnectionCache
    {
        private static volatile IRabbitMqConnectionCache _current;

        /// <summary>Current cache, or <c>null</c> when running uncached (the default).</summary>
        public static IRabbitMqConnectionCache Current => _current;

        /// <summary>Registers <paramref name="cache"/> as the active connection cache.</summary>
        public static void Register(IRabbitMqConnectionCache cache) =>
            Interlocked.Exchange(ref _current, cache);

        /// <summary>Removes the registered cache (host shutdown, and tests).</summary>
        public static void Clear() =>
            Interlocked.Exchange(ref _current, null);
    }

    /// <summary>
    /// Builds the opaque cache key identifying one set of broker coordinates.
    ///
    /// <para><b>The cache never sees credentials.</b> The key is a SHA-256 digest over the coordinates
    /// plus a per-process random salt, so a password can be neither read back from a key nor
    /// recovered from a logged key by precomputing digests. Connections are built by a caller-supplied
    /// closure, so the cache holds no secret material of its own.</para>
    ///
    /// <para>The key deliberately covers <b>every</b> field that changes which broker or identity we
    /// connect as. Editing a source's password must not silently keep reusing a connection
    /// authenticated with the old one.</para>
    /// </summary>
    public static class RabbitMqCacheKey
    {
        // Per-process, never persisted. Keys only need to be stable for the lifetime of the
        // in-memory cache that uses them.
        private static readonly byte[] Salt = CreateSalt();

        private static byte[] CreateSalt()
        {
            var salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        /// <summary>
        /// Returns a stable hex digest for the given coordinates. Null fields are normalised to
        /// empty, and fields are length-prefixed so that ("ab","c") and ("a","bc") cannot collide.
        /// </summary>
        public static string For(string hostName, int port, string userName, string virtualHost, string password)
        {
            var sb = new StringBuilder();
            Append(sb, hostName);
            Append(sb, port.ToString(CultureInfo.InvariantCulture));
            Append(sb, userName);
            Append(sb, virtualHost);
            Append(sb, password);

            using (var sha = SHA256.Create())
            {
                var payload = Encoding.UTF8.GetBytes(sb.ToString());
                var buffer = new byte[Salt.Length + payload.Length];
                Buffer.BlockCopy(Salt, 0, buffer, 0, Salt.Length);
                Buffer.BlockCopy(payload, 0, buffer, Salt.Length, payload.Length);

                var hash = sha.ComputeHash(buffer);
                var hex = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }
                return hex.ToString();
            }
        }

        /// <summary>
        /// The only form of a cache key that may be written to a log: enough to correlate entries,
        /// far too little to attack.
        /// </summary>
        public static string ToLogSafe(string cacheKey) =>
            string.IsNullOrEmpty(cacheKey)
                ? "(none)"
                : cacheKey.Substring(0, Math.Min(8, cacheKey.Length));

        private static void Append(StringBuilder sb, string value)
        {
            value = value ?? string.Empty;
            sb.Append(value.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(value).Append('|');
        }
    }

    /// <summary>
    /// Applies the connection settings that make a <b>long-lived</b> RabbitMQ connection safe.
    ///
    /// <para>Applied only on the cached path. Short-lived connections do not need any of this, so the
    /// uncached path is left exactly as it always was.</para>
    ///
    /// <para><b>Each setting here fixes a measured leak, not a style preference.</b> All three were
    /// established by decoding the IL of the shipped RabbitMQ.Client 5.1.2 assembly — see
    /// <c>docs/RabbitMQ-ConnectionCache-Plan.md</c> §3.</para>
    /// </summary>
    public static class RabbitMqCachedConnectionSettings
    {
        /// <summary>
        /// Well-known client-properties key the RabbitMQ management UI displays as the connection
        /// name. Not a Warewolf convention — it is the AMQP client-properties field the broker reads.
        /// </summary>
        internal const string ConnectionNameKey = "connection_name";

        /// <summary>
        /// Configures <paramref name="factory"/> for a connection that will be cached and reused.
        /// Idempotent — the same values are written on every publish.
        /// </summary>
        public static void Apply(ConnectionFactory factory, string clientProvidedName)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            // WHY false, when recovery sounds like exactly what a cached connection wants:
            // an AutorecoveringConnection records every actively-declared exchange, queue and
            // binding in dictionaries keyed by NAME, and removes them only on an explicit delete.
            // Queue names here come from workflow data and can be unbounded, so on a connection
            // that lives for minutes this grows without limit. A plain Connection has no such
            // dictionaries at all. We do not need the library's recovery because the cache performs
            // its own liveness check and replaces a dead connection, which is both simpler and
            // observable in the cache's own counters.
            factory.AutomaticRecoveryEnabled = false;

            // Moot once recovery is off; set explicitly so the intent survives a future edit that
            // reconsiders the line above.
            factory.TopologyRecoveryEnabled = false;

            // WHY true: Connection.StartMainLoop passes this straight to Thread.IsBackground, and it
            // defaults to FALSE. A cached connection is never disposed between publishes, so with
            // the default its FOREGROUND I/O thread would keep the Functions host process alive and
            // delay or block shutdown.
            factory.UseBackgroundThreadsForIO = true;

            if (!string.IsNullOrWhiteSpace(clientProvidedName))
            {
                // Makes cached connections identifiable in the RabbitMQ management UI, matching the
                // convention RabbitMqDeadLetterPublisher already uses.
                //
                // RabbitMQ.Client 5.1.2 has no ConnectionFactory.ClientProvidedName property — that
                // arrived in 6.x. In 5.x the name is either passed to a CreateConnection(string)
                // overload or, as here, carried in the client-properties field table under the
                // well-known "connection_name" key, which is what the management UI reads. Using the
                // field table keeps the factory callable through the plain CreateConnection().
                if (factory.ClientProperties is null)
                {
                    factory.ClientProperties = new Dictionary<string, object>();
                }
                factory.ClientProperties[ConnectionNameKey] = clientProvidedName;
            }
        }
    }

    /// <summary>
    /// A lease that owns its connection outright: disposing it closes the connection.
    ///
    /// <para>Returned by a cache that is full, disposed, or degrading, so callers never have to
    /// special-case the fallback. <see cref="IsTopologyFresh"/> is always false, so an uncached
    /// publish performs the same passive declares it performs with no cache at all.</para>
    /// </summary>
    public sealed class UncachedRabbitMqConnectionLease : IRabbitMqConnectionLease
    {
        private readonly IConnection _connection;
        private int _disposed;

        public UncachedRabbitMqConnectionLease(IConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public IConnection Connection => _connection;

        public bool IsCached => false;

        public bool IsTopologyFresh(string topologyKey) => false;

        public void MarkTopologyDeclared(string topologyKey)
        {
            // Nothing to remember: this connection dies with the lease.
        }

        public void Invalidate()
        {
            // Already single-use; disposal below closes it regardless.
        }

        public void Dispose()
        {
            // Interlocked rather than a bool: a lease may be disposed from a finally on one thread
            // while an error path disposes it on another, and double-closing a connection throws.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            try
            {
                _connection.Dispose();
            }
            catch (Exception ex)
            {
                // Disposing a connection that the broker already tore down throws; the caller is
                // finished with it either way, and rethrowing here would mask the real error that
                // sent us down this path.
                Dev2Logger.Warn(
                    "Disposing an uncached RabbitMQ connection threw; continuing. " +
                    "ExceptionType=" + ex.GetType().Name + ", Message=" + ex.Message,
                    "RabbitMqConnectionLease");
            }
        }
    }
}
