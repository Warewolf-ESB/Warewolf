/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Tunables for the in-process, secure Service Bus workflow trigger
/// (<c>Functions.ServiceBusWorkflowTriggerFunction</c> — Model A of
/// <c>Spec-Secure-ServiceBus-Triggered-Execution.md</c>).
///
/// <para>
/// The actual queue name and connection setting name are supplied directly to the
/// <c>[ServiceBusTrigger]</c> attribute via <c>%AppSetting%</c> indirection (the
/// standard Azure Functions mechanism for making a trigger binding configurable
/// without recompiling) — see <c>WAREWOLF_SERVICEBUS_TRIGGER_QUEUE</c> and
/// <c>ServiceBusConnection</c> in <c>local.settings.json</c> /
/// <c>docs/ServiceBusSecureTrigger-Architecture.md</c>. This options class only
/// carries values the trigger's own code (not the binding) needs at runtime.
/// </para>
///
/// <para>
/// To disable the trigger entirely on an engine that has no Service Bus namespace
/// provisioned, set the standard Azure Functions app setting
/// <c>AzureWebJobs.ServiceBusWorkflowTrigger.Disabled = true</c> — no code change
/// or extra flag required.
/// </para>
/// </summary>
public sealed class ServiceBusTriggerOptions
{
    /// <summary>
    /// Logical replay window for the jti dedupe cache: a jti already seen within this
    /// window is treated as a replay and the message is dead-lettered. Because the
    /// Hangfire-hash-backed store (see <c>Security.IServiceBusReplayAndResultStore</c>)
    /// has no built-in expiry, this value is advisory — it bounds how far back
    /// <see cref="Security.ServiceBusReplayAndResultStore"/> considers an entry "seen"
    /// when reasoning about token lifetime, not a hard storage TTL. Accepted trade-off:
    /// unbounded hash growth is a documented follow-up (no expiry-sweep job in this pass).
    /// </summary>
    public TimeSpan JtiReplayWindow { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Hard time budget for a single workflow execution
    /// (<c>Functions.ServiceBusWorkflowTriggerFunction</c>'s call into
    /// <c>IWorkflowExecutor.Execute</c>). If execution has not completed within this
    /// window, the trigger treats it as failed: the claim is released and a
    /// <see cref="TimeoutException"/> is thrown so Service Bus's own retry/backoff
    /// applies, exactly like the transient-failure/unexpected-exception paths — the
    /// message eventually dead-letters once <c>maxDeliveryCount</c> is exhausted instead
    /// of never reaching any terminal state (see the 1000-message ShovelBridge load test
    /// incidents of 2026-08-24/25, where a resource-starved Consumption-plan instance left
    /// executions running with no result ever recorded and nothing reaching the DLQ).
    ///
    /// <para>
    /// <b>Known limitation.</b> <c>IWorkflowExecutor.Execute</c> takes no
    /// <see cref="CancellationToken"/>, so a timed-out execution is abandoned, not
    /// cancelled — it keeps running in the background until it finishes on its own.
    /// </para>
    /// </summary>
    public TimeSpan ExecutionTimeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum number of workflow executions this instance will run concurrently. Excess
    /// concurrent trigger invocations wait for a free slot before calling
    /// <c>IWorkflowExecutor.Execute</c> — Service Bus's own durable queue absorbs the rest
    /// while they wait, so a burst is processed at a sustainable rate instead of
    /// overwhelming a single (typically Consumption-plan) instance's thread pool all at
    /// once. See <c>Infrastructure.ServiceCollectionExtensions</c> for the DI-singleton
    /// <see cref="System.Threading.SemaphoreSlim"/> sized from this value.
    /// </summary>
    public int MaxConcurrentExecutions { get; init; } = 8;

    /// <summary>
    /// Hard time budget for waiting on a free execution slot (the
    /// <see cref="MaxConcurrentExecutions"/> semaphore) before <c>Execute</c> is even
    /// called. Without this bound, a delivery queued behind a slot whose holder never
    /// finishes — a genuine deadlock inside <c>IWorkflowExecutor.Execute</c>, which
    /// <see cref="ExecutionTimeout"/> cannot reclaim (see that property's "Known
    /// limitation" note: a timed-out execution is abandoned, not cancelled, so its slot is
    /// never released) — would itself wait forever with no result, no error, and nothing
    /// ever reaching the dead-letter queue (see the 1000-message ShovelBridge load test
    /// incidents of 2026-08-24/25, where this exact silent-forever-wait was observed even
    /// after <see cref="ExecutionTimeout"/> and <see cref="MaxConcurrentExecutions"/> were
    /// both in place). On expiry the claim is released and a <see cref="TimeoutException"/>
    /// is thrown, same as <see cref="ExecutionTimeout"/> expiring.
    ///
    /// <para>
    /// Defaults to the same value as <see cref="ExecutionTimeout"/>: under healthy load a
    /// message may legitimately need to wait up to one full execution's worth of time for
    /// a slot to free, so setting this any shorter risks dead-lettering perfectly healthy,
    /// merely-queued messages.
    /// </para>
    /// </summary>
    public TimeSpan SlotWaitTimeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Reads tunables from environment variables.</summary>
    public static ServiceBusTriggerOptions FromEnvironment()
    {
        var windowRaw = Environment.GetEnvironmentVariable("WAREWOLF_SERVICEBUS_TRIGGER_JTI_WINDOW_HOURS");
        var window = double.TryParse(windowRaw, out var hours) && hours > 0
            ? TimeSpan.FromHours(hours)
            : TimeSpan.FromHours(24);

        var timeoutRaw = Environment.GetEnvironmentVariable("WAREWOLF_SERVICEBUS_TRIGGER_EXECUTION_TIMEOUT_SECONDS");
        var executionTimeout = double.TryParse(timeoutRaw, out var seconds) && seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : TimeSpan.FromMinutes(5);

        var maxConcurrentRaw = Environment.GetEnvironmentVariable("WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS");
        var maxConcurrentExecutions = int.TryParse(maxConcurrentRaw, out var maxConcurrent) && maxConcurrent > 0
            ? maxConcurrent
            : 8;

        var slotWaitTimeoutRaw = Environment.GetEnvironmentVariable("WAREWOLF_SERVICEBUS_TRIGGER_SLOT_WAIT_TIMEOUT_SECONDS");
        var slotWaitTimeout = double.TryParse(slotWaitTimeoutRaw, out var slotWaitSeconds) && slotWaitSeconds > 0
            ? TimeSpan.FromSeconds(slotWaitSeconds)
            : executionTimeout;

        return new ServiceBusTriggerOptions
        {
            JtiReplayWindow = window,
            ExecutionTimeout = executionTimeout,
            MaxConcurrentExecutions = maxConcurrentExecutions,
            SlotWaitTimeout = slotWaitTimeout,
        };
    }
}
