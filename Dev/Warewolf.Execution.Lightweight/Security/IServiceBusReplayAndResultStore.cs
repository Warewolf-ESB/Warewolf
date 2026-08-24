/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Security;

/// <summary>
/// Backing store for the secure Service Bus workflow trigger's replay protection,
/// business-idempotency dedupe, and correlation-id → result lookup (Model A —
/// <c>Spec-Secure-ServiceBus-Triggered-Execution.md</c> §6/§7).
///
/// <para>
/// One implementation (<see cref="ServiceBusReplayAndResultStore"/>) backs onto the
/// engine's existing Hangfire/SQL persistence store when <c>Config.Persistence.Enable</c>
/// is <c>true</c> (durable, safe across multiple engine instances), and falls back to an
/// in-process cache otherwise (documented single-instance-only caveat — see the class docs).
/// </para>
/// </summary>
public interface IServiceBusReplayAndResultStore
{
    /// <summary>
    /// Atomically registers <paramref name="jti"/> as seen. Returns <c>true</c> the
    /// FIRST time a given jti is registered (the message may proceed); <c>false</c> on
    /// every subsequent call for the same jti (replay — the message must be dead-lettered,
    /// never retried).
    /// </summary>
    bool TryRegisterJti(string jti);

    /// <summary>
    /// Looks up a previously persisted result for <paramref name="correlationId"/>.
    /// Used both for business-idempotency dedupe (skip re-execution of a message whose
    /// correlation id already has a terminal result) and by the polling endpoint.
    /// </summary>
    bool TryGetResult(string correlationId, out ServiceBusTriggerResult? result);

    /// <summary>Persists the terminal outcome for <paramref name="result"/>.<see cref="ServiceBusTriggerResult.CorrelationId"/>.</summary>
    void SaveResult(ServiceBusTriggerResult result);

    /// <summary>
    /// Atomically reserves <paramref name="correlationId"/> for processing. Returns
    /// <c>true</c> only when neither a completed result NOR an existing claim is present
    /// for this correlation id — i.e. this is the only delivery currently allowed to
    /// execute the workflow for it. Returns <c>false</c> when a result already exists
    /// (genuine duplicate — caller should complete without re-executing) or when another
    /// delivery already holds the claim (a concurrent/redelivered attempt is in flight —
    /// caller should abandon this delivery rather than race a second execution).
    ///
    /// <para>
    /// This closes the race the plain <see cref="TryGetResult"/> check-then-<see
    /// cref="SaveResult"/> pattern leaves open: <see cref="SaveResult"/> only happens
    /// after the whole workflow executes, so a redelivery landing in that window would
    /// otherwise always see "no result yet" and execute a second time.
    /// </para>
    ///
    /// <para>
    /// A claim older than the implementation's staleness threshold is treated as
    /// abandoned (the attempt holding it is presumed dead or permanently hung) and is
    /// taken over rather than rejected — see <c>ServiceBusReplayAndResultStore</c>'s
    /// "Claim staleness" class doc note.
    /// </para>
    /// </summary>
    bool TryClaim(string correlationId);

    /// <summary>
    /// Releases a claim taken by <see cref="TryClaim"/> without ever calling <see
    /// cref="SaveResult"/> — used on the transient-failure and unexpected-exception
    /// paths, which deliberately leave no persisted result so Service Bus's own
    /// retry/backoff can redeliver the message. Without releasing the claim here, that
    /// redelivery would be permanently blocked by the first (failed) attempt's stale
    /// claim. A no-op if no claim is held (e.g. <paramref name="correlationId"/> is
    /// empty, or a result was already saved instead).
    /// </summary>
    void ReleaseClaim(string correlationId);
}
