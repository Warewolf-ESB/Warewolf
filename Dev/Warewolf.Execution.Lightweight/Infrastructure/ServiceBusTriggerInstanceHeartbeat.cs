/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Extensions.Hosting;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// WOLF-8512 (build 30653 addendum): periodically records this instance's liveness via
/// <see cref="IServiceBusReplayAndResultStore.RecordInstanceHeartbeat"/>, independent of any
/// single Service Bus correlation id.
///
/// <para>
/// <b>Why this exists.</b> <c>ServiceBusWorkflowTriggerFunction.RecordClaimStage</c> proves
/// which checkpoint one in-flight execution last reached, but a claim stuck at
/// <c>AboutToExecute</c> forever cannot by itself distinguish "the whole instance died" from
/// "this one execution hung while its instance stayed alive and kept serving other work" —
/// both leave an identical claim-row signature. This heartbeat answers that question
/// directly: if it keeps advancing after a claim stalls, the instance survived (the hang is
/// execution-local); if it stops at the same moment, the instance died with it. See
/// <c>docs/ServiceBusSecureTrigger-Architecture.md</c>'s "Silent-death diagnostic breadcrumb"
/// section.
/// </para>
/// </summary>
internal sealed class ServiceBusTriggerInstanceHeartbeat : BackgroundService
{
    private readonly IServiceBusReplayAndResultStore _store;
    private readonly SemaphoreSlim _executionConcurrencyLimiter;
    private readonly ServiceBusTriggerOptions _triggerOptions;
    private readonly HostEnvironmentConfig _config;

    public ServiceBusTriggerInstanceHeartbeat(
        IServiceBusReplayAndResultStore store,
        SemaphoreSlim executionConcurrencyLimiter,
        ServiceBusTriggerOptions triggerOptions,
        HostEnvironmentConfig config)
    {
        _store = store;
        _executionConcurrencyLimiter = executionConcurrencyLimiter;
        _triggerOptions = triggerOptions;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var inFlight = _triggerOptions.MaxConcurrentExecutions - _executionConcurrencyLimiter.CurrentCount;
                _store.RecordInstanceHeartbeat(_config.InstanceId, inFlight);
            }
            catch (Exception ex)
            {
                // Best-effort, same guarantee as RecordClaimStage/RecordInstanceHeartbeat
                // themselves - a heartbeat failure must never crash the host.
                Dev2Logger.Warn("ServiceBusTriggerInstanceHeartbeat tick failed - continuing.", ex, nameof(ServiceBusTriggerInstanceHeartbeat));
            }

            try
            {
                await Task.Delay(_triggerOptions.HeartbeatInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Host shutting down - expected.
            }
        }
    }
}
