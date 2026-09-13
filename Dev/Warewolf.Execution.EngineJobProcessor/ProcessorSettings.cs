/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.EngineJobProcessor;

/// <summary>
/// Immutable environment-variable configuration for the JobProcessor.
///
/// Timer cadences are NOT here — <c>%JOB_POLL_SCHEDULE%</c> and
/// <c>%JOB_REAPER_SCHEDULE%</c> are resolved natively by the TimerTrigger binding.
///
/// | App setting | Default | Purpose |
/// |---|---|---|
/// | ENGINE_RESUME_BASEURL          | (required to dispatch) | Execution Engine base URL, e.g. https://myengine.azurewebsites.net |
/// | ENGINE_RESUME_SCOPE            | (required unless auth disabled) | Token scope, e.g. api://&lt;engine-app-id&gt;/.default |
/// | ENGINE_RESUME_TIMEOUT_SECONDS  | 15      | HTTP ack timeout — the processor never waits for workflow completion |
/// | ENGINE_RESUME_AUTH_DISABLED    | false   | Development only: skip bearer token acquisition |
/// | JOB_STALE_MINUTES              | 15      | Reaper threshold: Processing older than this → Failed (fail-only) |
/// | SERVICEBUS_CLAIM_WATCHDOG_ALERT_AFTER_MINUTES | 30 | Claim watchdog threshold: a claim this old with no result is alerted |
/// | SERVICEBUS_CLAIM_WATCHDOG_MAX_ALERTS_PER_TICK | 50 | Cap on alerts raised per tick, so one bad run cannot flood the logs |
/// </summary>
public sealed record ProcessorSettings
{
    public string? EngineResumeBaseUrl { get; init; }
    public string? EngineResumeScope { get; init; }
    public int ResumeTimeoutSeconds { get; init; } = 15;
    public bool AuthDisabled { get; init; }
    public int StaleMinutes { get; init; } = 15;

    /// <summary>
    /// How old a claim with no recorded result must be before the watchdog treats it as
    /// stalled. Deliberately well above the engine's own <c>ClaimStaleAfter</c> (which governs
    /// when a REDELIVERY may take a claim over): a claim younger than that is still expected to
    /// recover by itself, and alerting on it would just be noise.
    /// </summary>
    public int ClaimWatchdogAlertAfterMinutes { get; init; } = 30;

    public int ClaimWatchdogMaxAlertsPerTick { get; init; } = 50;

    public static ProcessorSettings FromEnvironment() => new()
    {
        EngineResumeBaseUrl = TrimmedOrNull(Environment.GetEnvironmentVariable("ENGINE_RESUME_BASEURL"))?.TrimEnd('/'),
        EngineResumeScope   = TrimmedOrNull(Environment.GetEnvironmentVariable("ENGINE_RESUME_SCOPE")),
        ResumeTimeoutSeconds = ParsePositiveInt(Environment.GetEnvironmentVariable("ENGINE_RESUME_TIMEOUT_SECONDS"), 15),
        AuthDisabled         = string.Equals(Environment.GetEnvironmentVariable("ENGINE_RESUME_AUTH_DISABLED"), "true", StringComparison.OrdinalIgnoreCase),
        StaleMinutes         = ParsePositiveInt(Environment.GetEnvironmentVariable("JOB_STALE_MINUTES"), 15),
        ClaimWatchdogAlertAfterMinutes = ParsePositiveInt(Environment.GetEnvironmentVariable("SERVICEBUS_CLAIM_WATCHDOG_ALERT_AFTER_MINUTES"), 30),
        ClaimWatchdogMaxAlertsPerTick  = ParsePositiveInt(Environment.GetEnvironmentVariable("SERVICEBUS_CLAIM_WATCHDOG_MAX_ALERTS_PER_TICK"), 50),
    };

    static string? TrimmedOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    static int ParsePositiveInt(string? value, int fallback)
        => int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}
