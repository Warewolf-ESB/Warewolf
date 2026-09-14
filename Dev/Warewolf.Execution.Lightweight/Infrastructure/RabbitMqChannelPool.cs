/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Concurrent;
using System.Threading;
using Dev2.Common;
using RabbitMQ.Client;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Reusable AMQP channels for one cached connection.
///
/// <para><b>What this is for.</b> The connection cache removed the TCP connect and the AMQP handshake
/// — roughly six of the eight round-trips a publish used to cost. The two that remain are
/// <c>channel.open</c> and <c>channel.close</c>, and against a remote broker they are essentially the
/// entire remaining cost. Measured on CloudAMQP from this repo's benchmark harness:</para>
///
/// <list type="bullet">
///   <item>publish on an already-open channel — <b>0.03 ms</b></item>
///   <item>channel open + close, no publish — <b>323.28 ms</b></item>
///   <item>open + publish + close (what a publish costs today) — <b>324.71 ms</b></item>
/// </list>
///
/// <para>So 99.6% of a cached publish is the channel, not the message. Pooling removes it.</para>
///
/// <para><b>The safety rule this preserves.</b> <c>IModel</c> is NOT thread-safe in RabbitMQ.Client
/// 5.1.2 — concurrent publishes on one channel corrupt the frame stream — while <c>IConnection</c> is.
/// A pooled channel is therefore owned <b>exclusively</b> by one publish between
/// <see cref="Rent"/> and <see cref="Return"/>, which is the identical discipline
/// <c>WorkflowExecutor._workflowPool</c> already uses to hand each execution its own activity
/// instances. Nothing here is shared concurrently; only the connection underneath is, and that is
/// documented as thread-safe.</para>
///
/// <para><b>Never fails a publish.</b> Over the cap, the pool hands out a private channel the caller
/// disposes as before. Everything degrades to today's behaviour rather than throwing.</para>
/// </summary>
sealed class RabbitMqChannelPool : IDisposable
{
    const string ExecutionId = "RabbitMqChannelPool";

    readonly IConnection _connection;
    readonly int _maxChannels;
    readonly ConcurrentBag<IModel> _idle = new();

    /// <summary>
    /// Every channel counted in <see cref="_live"/>, i.e. the ones this pool issued against the cap.
    ///
    /// <para>Membership — not <c>IsOpen</c> — is what <see cref="Return"/> branches on. Without it the
    /// pool cannot tell its own channel from the PRIVATE one <see cref="Rent"/> hands out over the cap,
    /// and it got both halves wrong: a healthy private channel was parked in <see cref="_idle"/>, so
    /// the cap bounded nothing, and a faulted one decremented <see cref="_live"/> it had never
    /// incremented, so the counter drifted below zero until the cap check could no longer fire at all.
    /// </para>
    /// </summary>
    readonly ConcurrentDictionary<IModel, byte> _owned = new();

    int _live;          // channels this pool owns: idle + rented
    int _created;
    int _discarded;
    int _disposed;

    internal RabbitMqChannelPool(IConnection connection, int maxChannels)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _maxChannels = maxChannels < 1 ? 1 : maxChannels;
    }

    internal int ChannelsCreated => Volatile.Read(ref _created);
    internal int ChannelsDiscarded => Volatile.Read(ref _discarded);
    internal int IdleChannels => _idle.Count;

    /// <summary>
    /// Takes an idle channel, or opens one.
    ///
    /// <para>A channel the broker closed is never handed out. <c>EnsureTopology</c>'s
    /// <c>ExchangeDeclarePassive</c> raises a CHANNEL-level AMQP exception on a 404, which closes the
    /// channel server-side. That is harmless when every publish opens its own, and poison when they
    /// are pooled — so a closed channel is dropped here and a replacement opened.</para>
    /// </summary>
    internal IModel Rent()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return _connection.CreateModel();
        }

        while (_idle.TryTake(out var pooled))
        {
            if (IsUsable(pooled))
            {
                return pooled;
            }

            Release(pooled);
        }

        // Over the cap: serve a private channel rather than blocking or failing. The publish still
        // succeeds; it simply pays the round-trips it would have paid anyway. It is deliberately NOT
        // recorded in _owned, which is how Return knows to dispose it instead of pooling it.
        if (Interlocked.Increment(ref _live) > _maxChannels)
        {
            Interlocked.Decrement(ref _live);
            return _connection.CreateModel();
        }

        var channel = _connection.CreateModel();
        _owned[channel] = 0;
        Interlocked.Increment(ref _created);
        return channel;
    }

    /// <summary>
    /// Returns a healthy channel to the pool, or disposes a faulted one.
    ///
    /// <para>Tolerates a channel this pool never issued — the over-cap path above hands out private
    /// channels, and the caller cannot tell the difference. Such a channel is simply disposed, which
    /// is what the caller would have done itself. It is NOT pooled (it was never counted against the
    /// cap, so pooling it would let <see cref="_idle"/> grow past <see cref="_maxChannels"/>) and it
    /// does NOT decrement <see cref="_live"/> (it never incremented it).</para>
    /// </summary>
    internal void Return(IModel channel)
    {
        if (channel is null)
        {
            return;
        }

        if (!_owned.ContainsKey(channel))
        {
            Discard(channel);
            return;
        }

        if (Volatile.Read(ref _disposed) != 0 || !IsUsable(channel))
        {
            Release(channel);
            return;
        }

        _idle.Add(channel);
    }

    /// <summary>
    /// Drops a pool-owned channel: forgets it, frees its slot under the cap, and closes it. The
    /// <see cref="_owned"/> removal gates the rest, so two racing callers cannot decrement
    /// <see cref="_live"/> twice for one channel.
    /// </summary>
    void Release(IModel channel)
    {
        if (!_owned.TryRemove(channel, out _))
        {
            return;
        }

        Interlocked.Decrement(ref _live);
        Discard(channel);
    }

    static bool IsUsable(IModel channel)
    {
        try
        {
            return channel.IsOpen;
        }
        catch
        {
            // A fully torn-down channel can throw on the property itself.
            return false;
        }
    }

    void Discard(IModel channel)
    {
        Interlocked.Increment(ref _discarded);
        try
        {
            channel.Dispose();
        }
        catch (Exception ex)
        {
            // Disposing a channel the broker already closed throws. We are finished with it either
            // way, and rethrowing would surface inside an unrelated caller's publish.
            Dev2Logger.Warn(
                $"Disposing a pooled RabbitMQ channel threw; continuing. " +
                $"ExceptionType={ex.GetType().Name}, Message={ex.Message}",
                ExecutionId);
        }
    }

    /// <summary>
    /// Closes every idle channel. Channels currently rented are not tracked here — they are closed by
    /// the connection's own disposal, which is the only thing that can happen after the last lease is
    /// released.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        while (_idle.TryTake(out var channel))
        {
            Release(channel);
        }

        // Anything left is RENTED, and a rented channel is returned to a disposed pool, which disposes
        // it. Forgetting them here would make that return look private and skip the _live decrement,
        // so they stay in _owned and the map empties itself as the last publishes finish.
    }
}
