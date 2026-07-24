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
/// </summary>
public sealed record ProcessorSettings
{
    public string? EngineResumeBaseUrl { get; init; }
    public string? EngineResumeScope { get; init; }
    public int ResumeTimeoutSeconds { get; init; } = 15;
    public bool AuthDisabled { get; init; }
    public int StaleMinutes { get; init; } = 15;

    public static ProcessorSettings FromEnvironment() => new()
    {
        EngineResumeBaseUrl = TrimmedOrNull(Environment.GetEnvironmentVariable("ENGINE_RESUME_BASEURL"))?.TrimEnd('/'),
        EngineResumeScope   = TrimmedOrNull(Environment.GetEnvironmentVariable("ENGINE_RESUME_SCOPE")),
        ResumeTimeoutSeconds = ParsePositiveInt(Environment.GetEnvironmentVariable("ENGINE_RESUME_TIMEOUT_SECONDS"), 15),
        AuthDisabled         = string.Equals(Environment.GetEnvironmentVariable("ENGINE_RESUME_AUTH_DISABLED"), "true", StringComparison.OrdinalIgnoreCase),
        StaleMinutes         = ParsePositiveInt(Environment.GetEnvironmentVariable("JOB_STALE_MINUTES"), 15),
    };

    static string? TrimmedOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    static int ParsePositiveInt(string? value, int fallback)
        => int.TryParse(value, out var parsed) && parsed > 0 ? parsed : fallback;
}
