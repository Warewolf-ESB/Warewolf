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

    /// <summary>Reads tunables from environment variables.</summary>
    public static ServiceBusTriggerOptions FromEnvironment()
    {
        var windowRaw = Environment.GetEnvironmentVariable("WAREWOLF_SERVICEBUS_TRIGGER_JTI_WINDOW_HOURS");
        var window = double.TryParse(windowRaw, out var hours) && hours > 0
            ? TimeSpan.FromHours(hours)
            : TimeSpan.FromHours(24);

        return new ServiceBusTriggerOptions { JtiReplayWindow = window };
    }
}
