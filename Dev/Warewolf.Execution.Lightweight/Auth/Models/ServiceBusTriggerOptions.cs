/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json;

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

    /// <summary>
    /// WOLF-8512: hard time budget for a single settlement call
    /// (<c>ServiceBusMessageActions.CompleteMessageAsync</c>/<c>DeadLetterMessageAsync</c>).
    /// The trigger settles with an independent <see cref="CancellationTokenSource"/> built from
    /// this value over <see cref="CancellationToken.None"/> — never the host invocation's own
    /// token — so a shutdown/drain that has already cancelled the host token cannot also abort
    /// settlement of an outcome that was already persisted.
    ///
    /// <para>
    /// <b>Why a timeout here is safe.</b> The trigger always calls
    /// <c>IServiceBusReplayAndResultStore.SaveResult</c> before settling. If settlement itself
    /// times out, the message is simply left unsettled: the Service Bus lock expires, the
    /// message redelivers, and the idempotency dedupe check finds the already-saved result and
    /// completes it then — self-healing at the cost of one extra delivery, never a lost or
    /// duplicated outcome.
    /// </para>
    /// </summary>
    public TimeSpan SettlementTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// WOLF-8512: how long a claim taken by <c>IServiceBusReplayAndResultStore.TryClaim</c> is
    /// honoured before a new delivery is allowed to take it over as abandoned (see
    /// <see cref="Security.ServiceBusReplayAndResultStore"/>'s "Claim staleness" doc comment).
    ///
    /// <para>
    /// Defaults to the resolved host <c>functionTimeout</c> (see
    /// <see cref="ResolveHostFunctionTimeout()"/>) plus one minute of lock-renewal margin — NOT
    /// a fixed constant — so this stays correct automatically across a Consumption-to-Flex
    /// Consumption move or a <c>functionTimeout</c> change, with zero code change. An operator
    /// can still override it directly via
    /// <c>WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES</c>, bypassing this inference
    /// entirely.
    /// </para>
    /// </summary>
    public TimeSpan ClaimStaleAfter { get; init; } = ResolveDefaultClaimStaleAfter();

    private const string ClaimStaleAfterOverrideMinutesVar = "WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES";
    private const string FunctionTimeoutAppSettingVar = "AzureFunctionsJobHost__functionTimeout";
    private const string WebsiteSkuVar = "WEBSITE_SKU";

    // WOLF-8512: the on-disk host.json read (tier 3 of ResolveHostFunctionTimeout) is the only
    // I/O-bound step in this resolution chain, and ServiceBusTriggerOptions is constructed with
    // a bare `new ServiceBusTriggerOptions()` throughout the test suite - a property initializer
    // without this cache would hit disk on every single construction. The tier tests below
    // never touch this field: they call the (env, readFile) overloads directly.
    private static readonly Lazy<TimeSpan> DefaultClaimStaleAfterLazy = new(
        () => ResolveClaimStaleAfter(Environment.GetEnvironmentVariable, ReadHostJsonFileOrNull));

    private static TimeSpan ResolveDefaultClaimStaleAfter() => DefaultClaimStaleAfterLazy.Value;

    /// <summary>
    /// WOLF-8512: describes how <see cref="ClaimStaleAfter"/>'s default was resolved — the
    /// value AND which tier produced it — so callers (see
    /// <c>Infrastructure.ServiceCollectionExtensions.AddCoreServices</c>) can log it at startup
    /// for App Insights auditability. Deliberately independent of <see cref="DefaultClaimStaleAfterLazy"/>:
    /// this is a diagnostic call made once at startup, not a hot path needing the cache.
    /// </summary>
    internal static (TimeSpan ClaimStaleAfter, string Tier) DescribeClaimStaleAfterResolution()
    {
        var overrideRaw = Environment.GetEnvironmentVariable(ClaimStaleAfterOverrideMinutesVar);
        if (double.TryParse(overrideRaw, out var overrideMinutes) && overrideMinutes > 0)
        {
            return (TimeSpan.FromMinutes(overrideMinutes), ClaimStaleAfterOverrideMinutesVar);
        }

        var (timeout, tier) = ResolveHostFunctionTimeout();
        return (timeout + TimeSpan.FromMinutes(1), tier);
    }

    /// <summary>
    /// WOLF-8512: computes <see cref="ClaimStaleAfter"/>'s default — the explicit
    /// <c>WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES</c> override when set (bypassing
    /// everything else), otherwise <see cref="ResolveHostFunctionTimeout(Func{string, string?}, Func{string, string?})"/>
    /// plus one minute of lock-renewal margin. Takes <paramref name="env"/>/<paramref name="readFile"/>
    /// so it can be driven deterministically in tests without mutating real environment
    /// variables or touching disk.
    /// </summary>
    internal static TimeSpan ResolveClaimStaleAfter(Func<string, string?> env, Func<string, string?> readFile)
    {
        var overrideRaw = env(ClaimStaleAfterOverrideMinutesVar);
        if (double.TryParse(overrideRaw, out var overrideMinutes) && overrideMinutes > 0)
        {
            return TimeSpan.FromMinutes(overrideMinutes);
        }

        return ResolveHostFunctionTimeout(env, readFile).Timeout + TimeSpan.FromMinutes(1);
    }

    /// <summary>Convenience overload resolving against the real environment and disk.</summary>
    internal static (TimeSpan Timeout, string Tier) ResolveHostFunctionTimeout() =>
        ResolveHostFunctionTimeout(Environment.GetEnvironmentVariable, ReadHostJsonFileOrNull);

    /// <summary>
    /// WOLF-8512: resolves the host's actual <c>functionTimeout</c> — the deadline the Functions
    /// host itself enforces — in strict priority order, stopping at the first hit. The hosting
    /// plan only BOUNDS this value (see the class doc's SKU table); it does not determine it, so
    /// SKU is deliberately the second-to-last tier, not the first.
    ///
    /// <list type="number">
    ///   <item>
    ///     <description><paramref name="env"/>(<c>AzureFunctionsJobHost__functionTimeout</c>) —
    ///     the standard app-setting form of the <c>host.json</c> value; authoritative when an
    ///     operator overrides the timeout per environment without redeploying.</description>
    ///   </item>
    ///   <item>
    ///     <description><c>host.json</c> on disk (<paramref name="readFile"/>, read from
    ///     <c>Path.Combine(AppContext.BaseDirectory, "host.json")</c> — deployed next to the
    ///     assembly, so this is the value the host is actually enforcing). Any read/parse
    ///     failure falls through to the next tier rather than throwing during DI
    ///     construction.</description>
    ///   </item>
    ///   <item>
    ///     <description><paramref name="env"/>(<c>WEBSITE_SKU</c>) plan-default fallback — only
    ///     reached when <c>functionTimeout</c> is genuinely absent from <c>host.json</c>:
    ///     <c>Dynamic</c> → 5 min; anything else recognised (<c>FlexConsumption</c>,
    ///     <c>ElasticPremium</c>, a dedicated plan) → 30 min.</description>
    ///   </item>
    ///   <item>
    ///     <description>Hard fallback — 10 min, if every probe above fails.</description>
    ///   </item>
    /// </list>
    /// </summary>
    internal static (TimeSpan Timeout, string Tier) ResolveHostFunctionTimeout(Func<string, string?> env, Func<string, string?> readFile)
    {
        var appSettingRaw = env(FunctionTimeoutAppSettingVar);
        if (TimeSpan.TryParse(appSettingRaw, out var appSettingTimeout) && appSettingTimeout > TimeSpan.Zero)
        {
            return (appSettingTimeout, FunctionTimeoutAppSettingVar);
        }

        var hostJsonPath = Path.Combine(AppContext.BaseDirectory, "host.json");
        var hostJsonRaw = readFile(hostJsonPath);
        if (!string.IsNullOrWhiteSpace(hostJsonRaw))
        {
            try
            {
                using var document = JsonDocument.Parse(hostJsonRaw);
                if (document.RootElement.TryGetProperty("functionTimeout", out var element)
                    && TimeSpan.TryParse(element.GetString(), out var hostJsonTimeout)
                    && hostJsonTimeout > TimeSpan.Zero)
                {
                    return (hostJsonTimeout, "host.json");
                }
            }
            catch (JsonException)
            {
                // Malformed host.json - fall through to the next tier rather than throw during
                // options construction.
            }
        }

        var sku = env(WebsiteSkuVar);
        if (string.Equals(sku, "Dynamic", StringComparison.OrdinalIgnoreCase))
        {
            return (TimeSpan.FromMinutes(5), "WEBSITE_SKU=Dynamic");
        }
        if (!string.IsNullOrWhiteSpace(sku))
        {
            return (TimeSpan.FromMinutes(30), $"WEBSITE_SKU={sku}");
        }

        return (TimeSpan.FromMinutes(10), "hard fallback");
    }

    private static string? ReadHostJsonFileOrNull(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

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

        var settlementTimeoutRaw = Environment.GetEnvironmentVariable("WAREWOLF_SERVICEBUS_TRIGGER_SETTLEMENT_TIMEOUT_SECONDS");
        var settlementTimeout = double.TryParse(settlementTimeoutRaw, out var settlementSeconds) && settlementSeconds > 0
            ? TimeSpan.FromSeconds(settlementSeconds)
            : TimeSpan.FromSeconds(30);

        return new ServiceBusTriggerOptions
        {
            JtiReplayWindow = window,
            ExecutionTimeout = executionTimeout,
            MaxConcurrentExecutions = maxConcurrentExecutions,
            SlotWaitTimeout = slotWaitTimeout,
            SettlementTimeout = settlementTimeout,
            // ClaimStaleAfter intentionally omitted here - its own property initializer already
            // resolves WAREWOLF_SERVICEBUS_TRIGGER_CLAIM_STALE_AFTER_MINUTES / the host.json
            // functionTimeout chain (see ResolveClaimStaleAfter), so FromEnvironment() and the
            // bare constructor stay consistent with each other automatically.
        };
    }
}
