/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dev2.Common;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Single JSON app setting controlling the RabbitMQ publish connection cache, following the
/// <see cref="SecurityFlags"/> / <see cref="DebugConfig"/> convention (one <c>WAREWOLF_*</c> variable
/// carrying every related tunable, so an operator flips the whole feature with one
/// <c>az functionapp config appsettings set</c>).
///
/// <code>
///   WAREWOLF_RABBITMQ_CACHE = {"enabled":true,"idleTimeout":"00:05:00","maxLifetime":"00:30:00",
///                              "topologyTtl":"00:00:30","maxConnections":4,"sweepInterval":"00:01:00"}
/// </code>
///
/// <para><b>Disabled by default and on any doubt</b> — absent, empty, malformed, or
/// <c>"enabled":false</c> all yield <see cref="Disabled"/>, in which case nothing is registered with
/// <c>AmbientRabbitMqConnectionCache</c> and every publish takes the long-standing
/// open-publish-dispose path. This mirrors <see cref="SecurityFlags"/>' safe-by-default posture.</para>
///
/// <para><b>Read once at startup</b>, unlike <see cref="SecurityFlags"/>, which is deliberately
/// re-parsed per call to stay hot-togglable. The difference is intentional: a cache's timeouts and
/// capacity are baked into the instance at construction, so pretending they can be hot-swapped would
/// be a lie. Changing this setting restarts the Function App, which re-reads it.</para>
/// </summary>
public sealed class RabbitMqCacheOptions
{
    public const string EnvVar = "WAREWOLF_RABBITMQ_CACHE";

    public static readonly TimeSpan DefaultIdleTimeout   = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan DefaultMaxLifetime   = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan DefaultTopologyTtl   = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan DefaultSweepInterval = TimeSpan.FromMinutes(1);
    public const int DefaultMaxConnections = 4;

    /// <summary>Channels kept per cached connection when <see cref="ChannelPool"/> is on.</summary>
    public const int DefaultMaxChannelsPerConnection = 32;

    /// <summary>
    /// Hard ceiling on <see cref="MaxConnections"/> regardless of configuration. Each cached
    /// connection costs a socket, an I/O thread and two heartbeat timers; on a Consumption instance
    /// capped at 1.5 GB an operator typo of <c>"maxConnections": 100000</c> must not be able to
    /// exhaust the host.
    /// </summary>
    public const int MaxAllowedConnections = 64;

    /// <summary>
    /// Hard ceiling on <see cref="MaxChannelsPerConnection"/>. Well below AMQP's negotiated
    /// channel-max (commonly 2047) so the pool can never be the thing that exhausts it.
    /// </summary>
    public const int MaxAllowedChannelsPerConnection = 256;

    /// <summary>The feature switch. False for every failure mode, by design.</summary>
    public bool Enabled { get; private init; }

    /// <summary>Sliding window: an entry unused for this long is evicted. Reset on every release.</summary>
    public TimeSpan IdleTimeout { get; private init; } = DefaultIdleTimeout;

    /// <summary>
    /// Absolute ceiling on a connection's age, measured from creation and never reset. Without it a
    /// continuously busy source would pin one connection for the life of the process.
    /// </summary>
    public TimeSpan MaxLifetime { get; private init; } = DefaultMaxLifetime;

    /// <summary>
    /// How long a successful topology verification is trusted before the passive declares are issued
    /// again. <see cref="TimeSpan.Zero"/> disables the memo entirely while keeping connection reuse —
    /// the setting to reach for if a queue is ever deleted out-of-band.
    /// </summary>
    public TimeSpan TopologyTtl { get; private init; } = DefaultTopologyTtl;

    /// <summary>Maximum number of cached connections. See <see cref="MaxAllowedConnections"/>.</summary>
    public int MaxConnections { get; private init; } = DefaultMaxConnections;

    /// <summary>How often expired entries are swept so idle sockets are released at their TTL.</summary>
    public TimeSpan SweepInterval { get; private init; } = DefaultSweepInterval;

    /// <summary>
    /// Reuse channels on a cached connection instead of opening and closing one per publish.
    ///
    /// <para>OFF by default, and gated SEPARATELY from <see cref="Enabled"/> on purpose: turning the
    /// connection cache on should not silently change channel behaviour as well. Enable the cache
    /// first, confirm it, then enable this.</para>
    ///
    /// <para>With the connection cached, <c>channel.open</c> + <c>channel.close</c> are the only
    /// broker round-trips a publish still pays - measured at 99.6% of the remaining cost against a
    /// remote broker. Pooling removes them. It has no effect unless <see cref="Enabled"/> is true,
    /// because an uncached publish has no connection to pool channels on.</para>
    /// </summary>
    public bool ChannelPool { get; private init; }

    /// <summary>
    /// Ceiling on pooled channels per connection. Beyond it a publish gets a private channel and
    /// disposes it, exactly as it does today - the pool degrades, it never fails or blocks a publish.
    ///
    /// <para>The pool only ever grows to the peak concurrency actually seen, so this is a safety
    /// ceiling rather than a dial to tune. AMQP negotiates its own channel-max per connection
    /// (commonly 2047); this stays far below it deliberately.</para>
    /// </summary>
    public int MaxChannelsPerConnection { get; private init; } = DefaultMaxChannelsPerConnection;

    /// <summary>The all-off instance returned for every absent/invalid configuration.</summary>
    public static RabbitMqCacheOptions Disabled { get; } = new();

    public static RabbitMqCacheOptions FromEnvironment() =>
        FromJson(Environment.GetEnvironmentVariable(EnvVar));

    /// <summary>
    /// Parses <paramref name="raw"/>. Separated from <see cref="FromEnvironment"/> so the parsing
    /// rules can be tested without mutating process-wide environment state, which would make the
    /// tests order-dependent under parallel execution.
    /// </summary>
    public static RabbitMqCacheOptions FromJson(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Disabled;
        }

        FileShape? shape;
        try
        {
            shape = JsonSerializer.Deserialize<FileShape>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
        }
        catch (JsonException ex)
        {
            // Same posture as SecurityFlags: a typo in an app setting must not take the host down,
            // and must not silently enable a feature either.
            Dev2Logger.Warn(
                $"RabbitMqCacheOptions failed to parse {EnvVar} — connection caching is DISABLED " +
                $"(safe default). ExceptionType={ex.GetType().Name}",
                "RabbitMqCacheOptions-FromJson");
            return Disabled;
        }

        if (shape is null || !shape.Enabled)
        {
            return Disabled;
        }

        var options = new RabbitMqCacheOptions
        {
            Enabled        = true,
            IdleTimeout    = PositiveOrDefault(shape.IdleTimeout,    DefaultIdleTimeout,    nameof(IdleTimeout)),
            MaxLifetime    = PositiveOrDefault(shape.MaxLifetime,    DefaultMaxLifetime,    nameof(MaxLifetime)),
            SweepInterval  = PositiveOrDefault(shape.SweepInterval,  DefaultSweepInterval,  nameof(SweepInterval)),
            // Zero is meaningful here (memo off), so only a NEGATIVE value is rejected.
            TopologyTtl    = NonNegativeOrDefault(shape.TopologyTtl, DefaultTopologyTtl,    nameof(TopologyTtl)),
            MaxConnections = ClampConnections(shape.MaxConnections),
            ChannelPool    = shape.ChannelPool,
            MaxChannelsPerConnection = ClampChannels(shape.MaxChannelsPerConnection),
        };

        // A max lifetime shorter than the idle window is almost certainly a mistake, and would make
        // the sliding window unobservable. Honour the stricter of the two rather than guessing.
        if (options.MaxLifetime < options.IdleTimeout)
        {
            Dev2Logger.Warn(
                $"{EnvVar}: maxLifetime ({options.MaxLifetime}) is shorter than idleTimeout " +
                $"({options.IdleTimeout}); maxLifetime governs and connections will be recycled at that age.",
                "RabbitMqCacheOptions-FromJson");
        }

        return options;
    }

    static TimeSpan PositiveOrDefault(TimeSpan? value, TimeSpan fallback, string name)
    {
        if (value is null)
        {
            return fallback;
        }
        if (value.Value <= TimeSpan.Zero)
        {
            Dev2Logger.Warn(
                $"{EnvVar}: {name} must be greater than zero (was {value.Value}); using {fallback}.",
                "RabbitMqCacheOptions-FromJson");
            return fallback;
        }
        return value.Value;
    }

    static TimeSpan NonNegativeOrDefault(TimeSpan? value, TimeSpan fallback, string name)
    {
        if (value is null)
        {
            return fallback;
        }
        if (value.Value < TimeSpan.Zero)
        {
            Dev2Logger.Warn(
                $"{EnvVar}: {name} cannot be negative (was {value.Value}); using {fallback}.",
                "RabbitMqCacheOptions-FromJson");
            return fallback;
        }
        return value.Value;
    }

    /// <summary>
    /// Ceiling on pooled channels per connection. Unlike connections, a channel is cheap - it is a
    /// multiplexed stream on an existing socket, not a socket - so the ceiling is higher and is a
    /// guard against runaway concurrency rather than a resource budget.
    /// </summary>
    static int ClampChannels(int? value)
    {
        if (value is null)
        {
            return DefaultMaxChannelsPerConnection;
        }
        if (value.Value < 1)
        {
            Dev2Logger.Warn(
                $"{EnvVar}: maxChannelsPerConnection must be at least 1 (was {value.Value}); " +
                $"using {DefaultMaxChannelsPerConnection}.",
                "RabbitMqCacheOptions-FromJson");
            return DefaultMaxChannelsPerConnection;
        }
        if (value.Value > MaxAllowedChannelsPerConnection)
        {
            Dev2Logger.Warn(
                $"{EnvVar}: maxChannelsPerConnection {value.Value} exceeds the " +
                $"{MaxAllowedChannelsPerConnection} ceiling; clamping. The pool only grows to the " +
                "peak concurrency seen, so a high ceiling buys nothing and risks the broker's own " +
                "negotiated channel-max.",
                "RabbitMqCacheOptions-FromJson");
            return MaxAllowedChannelsPerConnection;
        }
        return value.Value;
    }

    static int ClampConnections(int? value)
    {
        if (value is null)
        {
            return DefaultMaxConnections;
        }
        if (value.Value < 1)
        {
            Dev2Logger.Warn(
                $"{EnvVar}: maxConnections must be at least 1 (was {value.Value}); using {DefaultMaxConnections}.",
                "RabbitMqCacheOptions-FromJson");
            return DefaultMaxConnections;
        }
        if (value.Value > MaxAllowedConnections)
        {
            Dev2Logger.Warn(
                $"{EnvVar}: maxConnections {value.Value} exceeds the {MaxAllowedConnections} ceiling; " +
                "clamping. Each cached connection costs a socket, an I/O thread and two timers.",
                "RabbitMqCacheOptions-FromJson");
            return MaxAllowedConnections;
        }
        return value.Value;
    }

    public override string ToString() =>
        Enabled
            ? $"enabled, idle={IdleTimeout}, maxLifetime={MaxLifetime}, topologyTtl={TopologyTtl}, " +
              $"maxConnections={MaxConnections}, sweep={SweepInterval}, " +
              $"channelPool={(ChannelPool ? $"on (max {MaxChannelsPerConnection}/connection)" : "off")}"
            : "disabled";

    /// <summary>
    /// JSON binding target. Every field is nullable so "absent" is distinguishable from "explicitly
    /// set to zero", which matters for <c>topologyTtl</c> where zero is a real, meaningful value.
    /// </summary>
    sealed class FileShape
    {
        [JsonPropertyName("enabled")]        public bool Enabled { get; set; }
        [JsonPropertyName("idleTimeout")]    public TimeSpan? IdleTimeout { get; set; }
        [JsonPropertyName("maxLifetime")]    public TimeSpan? MaxLifetime { get; set; }
        [JsonPropertyName("topologyTtl")]    public TimeSpan? TopologyTtl { get; set; }
        [JsonPropertyName("maxConnections")] public int? MaxConnections { get; set; }
        [JsonPropertyName("channelPool")]    public bool ChannelPool { get; set; }
        [JsonPropertyName("maxChannelsPerConnection")] public int? MaxChannelsPerConnection { get; set; }
        [JsonPropertyName("sweepInterval")]  public TimeSpan? SweepInterval { get; set; }
    }
}
