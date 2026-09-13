/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using RabbitMQ.Client;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Caches AMQP connections across activity executions so a publish costs a channel round-trip
/// instead of a full TCP + AMQP handshake. Registered only by this Function host, and only when
/// <see cref="RabbitMqCacheOptions"/> enables it — the full Warewolf server never registers a cache
/// and is therefore unaffected.
///
/// <para><b>What is cached, and what is deliberately not.</b> The <c>IConnection</c> only. Channels
/// (<c>IModel</c>) are created per publish and disposed immediately, because <c>IModel</c> is not
/// thread-safe in RabbitMQ.Client 5.1.2 while <c>IConnection</c> is. That asymmetry is what lets
/// concurrent executions share one connection with no locking at all.</para>
///
/// <para><b>Why a reference count rather than a lock.</b> Eviction and publishing race by nature: a
/// sweep can decide an entry is stale while another thread is mid-publish on it. Disposing the
/// connection there would be an <c>ObjectDisposedException</c> in the middle of a live publish —
/// strictly worse than the leak we are avoiding. So eviction only unpublishes the entry from the map
/// and drops the cache's own reference; the connection is disposed by whichever party takes the count
/// to zero, which may be a lease releasing long after the eviction.</para>
///
/// <para><b>Failure posture: never break a publish.</b> Every cache-internal failure degrades to an
/// uncached lease, i.e. exactly the behaviour the caller would have had with no cache. Only the
/// caller's own connection factory throwing — a genuinely unreachable broker — propagates.</para>
/// </summary>
public sealed class LightweightRabbitMqConnectionCache : IRabbitMqConnectionCache
{
    const string ExecutionId = "RabbitMqConnectionCache";

    /// <summary>
    /// Ceiling on remembered topology names per connection. The memo is keyed by queue name, which
    /// is caller-supplied and may be computed from workflow data, so it is unbounded by nature. At
    /// the cap the memo is cleared wholesale: it is a pure optimisation, so the entire cost of
    /// dropping it is one extra pair of passive declares per queue.
    /// </summary>
    internal const int MaxTopologyEntriesPerConnection = 256;

    /// <summary>Bounded retry for the insert race in <see cref="AcquireCore"/>.</summary>
    const int MaxAcquireAttempts = 3;

    readonly RabbitMqCacheOptions _options;
    readonly Func<DateTimeOffset> _clock;
    readonly ConcurrentDictionary<string, CacheEntry> _entries = new(StringComparer.Ordinal);
    readonly Timer? _sweepTimer;

    int _disposed;
    int _connectionsCreated;
    int _connectionsDisposed;
    int _uncachedFallbacks;

    /// <param name="clock">
    /// Injectable so expiry can be tested by advancing a fake clock instead of sleeping. A test that
    /// sleeps for a five-minute idle timeout is a test nobody runs.
    /// </param>
    /// <param name="enableSweepTimer">
    /// False in tests, which call <see cref="Sweep"/> directly. A background timer firing mid-assertion
    /// is the classic source of a flaky cache test.
    /// </param>
    public LightweightRabbitMqConnectionCache(
        RabbitMqCacheOptions options,
        Func<DateTimeOffset>? clock = null,
        bool enableSweepTimer = true)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _clock = clock ?? (() => DateTimeOffset.UtcNow);

        if (enableSweepTimer && _options.SweepInterval > TimeSpan.Zero)
        {
            // One timer for the whole cache, not one per entry. Its only job is to release idle
            // sockets at their TTL rather than leaving them pinned until the Consumption instance is
            // recycled, which would be impolite to a broker that allocates per-connection buffers.
            _sweepTimer = new Timer(_ => SweepSafely(), null, _options.SweepInterval, _options.SweepInterval);
        }
    }

    // ── Diagnostics, asserted by the tests that guard the leak invariants ────────────────────────

    internal int CachedEntryCount => _entries.Count;
    internal int ConnectionsCreated => Volatile.Read(ref _connectionsCreated);
    internal int ConnectionsDisposed => Volatile.Read(ref _connectionsDisposed);
    internal int UncachedFallbacks => Volatile.Read(ref _uncachedFallbacks);

    /// <summary>Channels opened across every cached connection. 0 when pooling is off.</summary>
    internal int ChannelsCreated
    {
        get
        {
            var total = 0;
            foreach (var kvp in _entries)
            {
                total += kvp.Value.ChannelPool?.ChannelsCreated ?? 0;
            }
            return total;
        }
    }

    /// <summary>Pooled channels dropped because the broker had closed them.</summary>
    internal int ChannelsDiscarded
    {
        get
        {
            var total = 0;
            foreach (var kvp in _entries)
            {
                total += kvp.Value.ChannelPool?.ChannelsDiscarded ?? 0;
            }
            return total;
        }
    }

    // ── Public surface ──────────────────────────────────────────────────────────────────────────

    public IRabbitMqConnectionLease Acquire(string cacheKey, Func<IConnection> connectionFactory)
    {
        if (connectionFactory is null)
        {
            throw new ArgumentNullException(nameof(connectionFactory));
        }

        if (string.IsNullOrEmpty(cacheKey) || Volatile.Read(ref _disposed) != 0)
        {
            return Uncached(connectionFactory);
        }

        // Distinguishes "the broker is unreachable" (must surface to the caller) from "the cache
        // itself misbehaved" (must degrade silently). Without this flag a catch-all here would
        // swallow a genuine connection failure and retry it, turning one clear error into two.
        var factoryFaulted = false;
        Func<IConnection> guarded = () =>
        {
            try
            {
                return connectionFactory();
            }
            catch
            {
                factoryFaulted = true;
                throw;
            }
        };

        try
        {
            return AcquireCore(cacheKey, guarded);
        }
        catch (Exception ex)
        {
            if (factoryFaulted)
            {
                throw;
            }

            Dev2Logger.Warn(
                $"Connection cache failed for key {RabbitMqCacheKey.ToLogSafe(cacheKey)}; falling back to an " +
                $"uncached connection. ExceptionType={ex.GetType().Name}, Message={ex.Message}",
                ExecutionId);
            return Uncached(connectionFactory);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _sweepTimer?.Dispose();

        foreach (var kvp in _entries)
        {
            EvictEntry(kvp.Key, kvp.Value);
        }

        Dev2Logger.Info(
            $"Connection cache disposed. Created={ConnectionsCreated}, Disposed={ConnectionsDisposed}, " +
            $"UncachedFallbacks={UncachedFallbacks}.",
            ExecutionId);
    }

    // ── Core ────────────────────────────────────────────────────────────────────────────────────

    IRabbitMqConnectionLease AcquireCore(string key, Func<IConnection> factory)
    {
        for (var attempt = 0; attempt < MaxAcquireAttempts; attempt++)
        {
            if (_entries.TryGetValue(key, out var existing))
            {
                if (IsUsable(existing) && TryAddRef(existing))
                {
                    return new CachedLease(this, key, existing);
                }

                // Stale, closed, poisoned, or racing with its own disposal. Unpublish it so the
                // next loop iteration creates a replacement.
                EvictEntry(key, existing);
            }

            if (_entries.Count >= _options.MaxConnections && !TryEvictIdlest())
            {
                // At capacity and every cached connection is actively in use. Evicting one of those
                // would not free anything (its lease keeps it alive) and would only destroy a warm
                // entry, so serve this publish uncached instead.
                return Uncached(factory);
            }

            var connection = factory();
            var entry = new CacheEntry(connection, _clock().UtcTicks,
                _options.ChannelPool
                    ? new RabbitMqChannelPool(connection, _options.MaxChannelsPerConnection)
                    : null);

            if (_entries.TryAdd(key, entry))
            {
                Interlocked.Increment(ref _connectionsCreated);
                return new CachedLease(this, key, entry);
            }

            // Another thread inserted first. Drop ours rather than leaking it, then loop and take
            // theirs. Zero the count first so nothing can hand this entry out.
            Volatile.Write(ref entry.RefCount, 0);
            DisposeConnection(connection);
        }

        return Uncached(factory);
    }

    IRabbitMqConnectionLease Uncached(Func<IConnection> factory)
    {
        Interlocked.Increment(ref _uncachedFallbacks);
        return new UncachedRabbitMqConnectionLease(factory());
    }

    bool IsUsable(CacheEntry entry)
    {
        if (Volatile.Read(ref entry.Poisoned) != 0)
        {
            return false;
        }

        var now = _clock().UtcTicks;

        // Absolute ceiling, measured from creation and never reset: without it a continuously busy
        // source would pin one connection for the life of the process.
        if (now - Volatile.Read(ref entry.CreatedTicks) > _options.MaxLifetime.Ticks)
        {
            return false;
        }

        // Sliding window, reset on every release.
        if (now - Volatile.Read(ref entry.LastUsedTicks) > _options.IdleTimeout.Ticks)
        {
            return false;
        }

        try
        {
            return entry.Connection.IsOpen;
        }
        catch (Exception)
        {
            // A disposed or broken connection can throw from IsOpen. Treat it as unusable rather
            // than letting the exception escape into the caller's publish path.
            return false;
        }
    }

    /// <summary>
    /// Takes a reference, refusing to resurrect an entry whose count has already reached zero.
    /// That refusal is what makes <see cref="ReleaseRef"/>'s dispose-at-zero safe: once an entry has
    /// been retired, no later acquire can bring it back and hand out a connection about to be closed.
    /// </summary>
    static bool TryAddRef(CacheEntry entry)
    {
        while (true)
        {
            var current = Volatile.Read(ref entry.RefCount);
            if (current <= 0)
            {
                return false;
            }
            if (Interlocked.CompareExchange(ref entry.RefCount, current + 1, current) == current)
            {
                return true;
            }
        }
    }

    void ReleaseRef(CacheEntry entry)
    {
        if (Interlocked.Decrement(ref entry.RefCount) == 0)
        {
            // Channels first. Disposing the connection would close them anyway, but doing it in this
            // order keeps the pool's own counters honest and closes each channel cleanly rather than
            // having the broker tear them down underneath it.
            entry.ChannelPool?.Dispose();
            DisposeConnection(entry.Connection);
        }
    }

    /// <summary>
    /// Unpublishes <paramref name="entry"/> and drops the cache's own reference. Does NOT dispose the
    /// connection if leases are still outstanding — see the class remarks.
    /// </summary>
    void EvictEntry(string key, CacheEntry entry)
    {
        // The key/value overload matters: it removes only if this exact instance is still mapped, so
        // a concurrent replacement is never evicted by a stale decision, and the reference below is
        // dropped exactly once.
        if (_entries.TryRemove(new KeyValuePair<string, CacheEntry>(key, entry)))
        {
            entry.Topology.Clear();
            ReleaseRef(entry);
        }
    }

    /// <summary>
    /// Evicts the least-recently-used entry that has no outstanding leases. Entries in active use are
    /// never chosen: evicting one frees no capacity (its lease keeps the connection alive) and would
    /// only throw away a warm connection.
    /// </summary>
    bool TryEvictIdlest()
    {
        string? victimKey = null;
        CacheEntry? victim = null;
        var oldest = long.MaxValue;

        foreach (var kvp in _entries)
        {
            // 1 == the cache's own reference only, i.e. nobody is publishing on it.
            if (Volatile.Read(ref kvp.Value.RefCount) != 1)
            {
                continue;
            }

            var lastUsed = Volatile.Read(ref kvp.Value.LastUsedTicks);
            if (victim is null || lastUsed < oldest)
            {
                oldest = lastUsed;
                victimKey = kvp.Key;
                victim = kvp.Value;
            }
        }

        if (victim is null || victimKey is null)
        {
            return false;
        }

        EvictEntry(victimKey, victim);
        return true;
    }

    void ReleaseLease(string key, CacheEntry entry)
    {
        Volatile.Write(ref entry.LastUsedTicks, _clock().UtcTicks);

        if (Volatile.Read(ref entry.Poisoned) != 0)
        {
            EvictEntry(key, entry);
        }

        ReleaseRef(entry);
    }

    void DisposeConnection(IConnection connection)
    {
        Interlocked.Increment(ref _connectionsDisposed);
        try
        {
            connection.Dispose();
        }
        catch (Exception ex)
        {
            // Disposing a connection the broker already tore down throws. We are finished with it
            // either way, and rethrowing here would surface inside an unrelated caller's publish.
            Dev2Logger.Warn(
                $"Disposing a cached RabbitMQ connection threw; continuing. " +
                $"ExceptionType={ex.GetType().Name}, Message={ex.Message}",
                ExecutionId);
        }
    }

    /// <summary>Evicts every entry that is no longer usable. Internal so tests can drive it directly.</summary>
    internal void Sweep()
    {
        foreach (var kvp in _entries)
        {
            if (!IsUsable(kvp.Value))
            {
                EvictEntry(kvp.Key, kvp.Value);
            }
        }
    }

    void SweepSafely()
    {
        try
        {
            Sweep();
        }
        catch (Exception ex)
        {
            // A timer callback that throws takes the process down. The sweep is opportunistic —
            // Acquire re-checks usability anyway — so swallowing here costs nothing but a log line.
            Dev2Logger.Warn(
                $"Connection cache sweep failed; entries will be re-checked on next acquire. " +
                $"ExceptionType={ex.GetType().Name}, Message={ex.Message}",
                ExecutionId);
        }
    }

    // ── Types ───────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// One cached connection plus its clocks, reference count and topology memo. Fields rather than
    /// properties because <see cref="Interlocked"/> and <see cref="Volatile"/> operate on fields.
    /// </summary>
    sealed class CacheEntry
    {
        internal readonly IConnection Connection;
        internal readonly ConcurrentDictionary<string, long> Topology = new(StringComparer.Ordinal);

        internal long CreatedTicks;
        internal long LastUsedTicks;

        /// <summary>Starts at 2: one reference for the cache itself, one for the acquiring caller.</summary>
        internal int RefCount;

        internal int Poisoned;

        /// <summary>
        /// Reusable channels for this connection, or null when channel pooling is off. Owned by the
        /// entry - not by a lease - because channels must outlive the publish that opened them,
        /// which is the entire point.
        /// </summary>
        internal readonly RabbitMqChannelPool ChannelPool;

        internal CacheEntry(IConnection connection, long nowTicks, RabbitMqChannelPool channelPool)
        {
            Connection = connection;
            CreatedTicks = nowTicks;
            LastUsedTicks = nowTicks;
            RefCount = 2;
            ChannelPool = channelPool;
        }
    }

    sealed class CachedLease : IRabbitMqConnectionLease
    {
        readonly LightweightRabbitMqConnectionCache _cache;
        readonly string _key;
        readonly CacheEntry _entry;
        int _disposed;

        internal CachedLease(LightweightRabbitMqConnectionCache cache, string key, CacheEntry entry)
        {
            _cache = cache;
            _key = key;
            _entry = entry;
        }

        public IConnection Connection => _entry.Connection;

        public bool IsCached => true;

        public bool IsTopologyFresh(string topologyKey)
        {
            if (_cache._options.TopologyTtl <= TimeSpan.Zero || string.IsNullOrEmpty(topologyKey))
            {
                return false;
            }

            return _entry.Topology.TryGetValue(topologyKey, out var expiresAtTicks)
                   && expiresAtTicks > _cache._clock().UtcTicks;
        }

        public void MarkTopologyDeclared(string topologyKey)
        {
            if (_cache._options.TopologyTtl <= TimeSpan.Zero || string.IsNullOrEmpty(topologyKey))
            {
                return;
            }

            var topology = _entry.Topology;
            if (topology.Count >= MaxTopologyEntriesPerConnection && !topology.ContainsKey(topologyKey))
            {
                topology.Clear();
            }

            topology[topologyKey] = _cache._clock().UtcTicks + _cache._options.TopologyTtl.Ticks;
        }

        /// <summary>
        /// A pooled channel when pooling is on, otherwise a brand-new one - the historical behaviour.
        /// Exclusively owned by this publish until <see cref="ReturnChannel"/>, which is what keeps a
        /// non-thread-safe <c>IModel</c> safe while the connection underneath is shared.
        /// </summary>
        public IModel RentChannel() =>
            _entry.ChannelPool is null ? _entry.Connection.CreateModel() : _entry.ChannelPool.Rent();

        /// <summary>
        /// Hands the channel back to the pool, or disposes it when pooling is off. The pool drops any
        /// channel the broker has closed rather than recycling it.
        /// </summary>
        public void ReturnChannel(IModel channel)
        {
            if (_entry.ChannelPool is null)
            {
                channel?.Dispose();
                return;
            }

            _entry.ChannelPool.Return(channel);
        }

        public void Invalidate()
        {
            Volatile.Write(ref _entry.Poisoned, 1);

            // Whatever we believed about this connection's topology was learned over a connection we
            // no longer trust. A replacement must re-verify from scratch.
            _entry.Topology.Clear();

            // Same reasoning for the channels: they are multiplexed on the very connection we have
            // just declared unusable, so keeping them idle would hand a later publish a channel on a
            // half-dead socket. Safe to do while other leases are still using this entry - a disposed
            // pool opens a private channel per Rent and disposes it on Return, which is exactly the
            // unpooled behaviour.
            _entry.ChannelPool?.Dispose();
        }

        public void Dispose()
        {
            // A lease can legitimately be disposed twice — a using block plus an error path — and a
            // double release would drop the reference count below zero and dispose a live connection.
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _cache.ReleaseLease(_key, _entry);
        }
    }
}
