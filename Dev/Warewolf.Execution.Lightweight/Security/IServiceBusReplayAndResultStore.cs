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
}
