/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Warewolf.Driver.Persistence;
using Warewolf.Execution.EngineJobProcessor.Services;

namespace Warewolf.Execution.EngineJobProcessor.Functions;

/// <summary>
/// WOLF-8512: alerts on Service Bus trigger claims that were taken and then never resolved —
/// the "silent death" signature the Execution Engine's claim-stage breadcrumb records but
/// nothing has ever watched for.
///
/// <para>
/// <b>Why this lives here, on a timer.</b> When the engine's host process dies mid-execution,
/// the message is left unsettled and Service Bus makes it available again once the lock
/// expires — but Service Bus only ever hands a message to a receiver that asks for it. A
/// Consumption-plan engine that has scaled to zero after a burst has no receiver, and nothing
/// in the system notices: the build-30655 investigation (2026-09-13) found three correlation
/// ids still frozen at their original claim 13 hours later, with no result, no dead-letter and
/// no alert. A TimerTrigger is deliberately NOT subject to that blind spot — the platform
/// wakes a Consumption app for a scheduled trigger regardless of queue depth — so this runs on
/// its own cadence in the JobProcessor, independent of whether the engine is up.
/// </para>
///
/// <para>
/// <b>Detect and alert only.</b> It does not touch the Service Bus message: no dead-lettering,
/// no forced re-execution. That matches the fail-only policy <see cref="ReaperFunction"/>
/// already applies to suspend/resume jobs — a partially-executed workflow may have side
/// effects, so recovery stays a human decision. The value here is that the decision can be
/// made in minutes from an alert instead of by after-the-fact SQL archaeology.
/// </para>
///
/// <para>
/// Reads go through raw SQL because Hangfire's storage API can only fetch a hash whose exact
/// key you already know, and this has to find claims by prefix — the same
/// <c>SELECT ... FROM HangFire.Hash WHERE [Key] LIKE 'sbtrigger:claim:%'</c> shape
/// <c>docs/ServiceBusSecureTrigger-Architecture.md</c> already documents for doing this by
/// hand. Writes (the alerted marker) go through Hangfire's own connection, so nothing in this
/// app invents its own way of writing that schema.
/// </para>
/// </summary>
public sealed class ServiceBusClaimWatchdogFunction
{
    // Mirrors the engine's own ServiceBusReplayAndResultStore constants. Deliberately
    // duplicated rather than shared: the JobProcessor does not reference the engine assembly
    // (only Warewolf.Driver.Persistence), and this codebase already prefers duplicating a
    // stable constant over forcing a dependency between the two apps — see the comment on
    // ServiceBusReplayAndResultStore.CurrentInstanceId for the same call.
    internal const string ClaimHashKeyPrefix = "sbtrigger:claim:";
    internal const string ResultHashKeyPrefix = "sbtrigger:result:";
    internal const string InstanceHashKeyPrefix = "sbtrigger:instance:";
    internal const string ClaimedAtFieldName = "claimedAtUtc";
    internal const string StageFieldName = "lastStage";
    internal const string StageAtFieldName = "lastStageAtUtc";
    internal const string StageInstanceIdFieldName = "lastStageInstanceId";
    internal const string StageProcessIdFieldName = "lastStageProcessId";
    internal const string InstanceHeartbeatAtFieldName = "lastHeartbeatUtc";
    internal const string InstanceProcessIdFieldName = "processId";

    /// <summary>
    /// Written into the claim's own hash once alerted, so a claim that stays stalled is
    /// reported once rather than on every tick forever. No new key: the marker travels with
    /// the row it describes, and disappears with it whenever the claim is released.
    /// </summary>
    internal const string AlertedAtFieldName = "watchdogAlertedAtUtc";

    internal const string StalledEventName = "ServiceBusClaimStalled";

    readonly JobStorageProvider _storageProvider;
    readonly ProcessorSettings _settings;
    readonly TelemetryClient _telemetry;
    readonly ILogger<ServiceBusClaimWatchdogFunction> _logger;

    public ServiceBusClaimWatchdogFunction(
        JobStorageProvider storageProvider,
        ProcessorSettings settings,
        TelemetryClient telemetry,
        ILogger<ServiceBusClaimWatchdogFunction> logger)
    {
        _storageProvider = storageProvider;
        _settings = settings;
        _telemetry = telemetry;
        _logger = logger;
    }

    [Function("ServiceBusClaimWatchdog")]
    public async Task Run([TimerTrigger("%SERVICEBUS_CLAIM_WATCHDOG_SCHEDULE%")] TimerInfo timerInfo)
    {
        if (!Config.Persistence.Enable)
        {
            _logger.LogDebug("ServiceBusClaimWatchdog | Persistence disabled — no claim store to watch.");
            return;
        }

        var cutoffUtc = DateTimeOffset.UtcNow.AddMinutes(-_settings.ClaimWatchdogAlertAfterMinutes);

        List<StalledClaim> stalled;
        try
        {
            stalled = await FindStalledClaimsAsync(cutoffUtc, _settings.ClaimWatchdogMaxAlertsPerTick + 1).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // A watchdog that takes the app down with it is worse than one that misses a tick.
            _logger.LogError(ex, "ServiceBusClaimWatchdog | Scan failed — skipping this tick. CutoffUtc={CutoffUtc:O}", cutoffUtc);
            return;
        }

        if (stalled.Count == 0)
        {
            _logger.LogInformation("ServiceBusClaimWatchdog | Completed | Stalled=0 | CutoffUtc={CutoffUtc:O}", cutoffUtc);
            return;
        }

        var truncated = stalled.Count > _settings.ClaimWatchdogMaxAlertsPerTick;
        var toAlert = truncated ? stalled.Take(_settings.ClaimWatchdogMaxAlertsPerTick).ToList() : stalled;

        foreach (var claim in toAlert)
        {
            Alert(claim);
            MarkAlerted(claim.CorrelationId);
        }

        if (truncated)
        {
            _logger.LogError(
                "ServiceBusClaimWatchdog | More than {MaxAlerts} stalled claims this tick — only the oldest {MaxAlerts} were alerted. " +
                "A backlog this size points at an engine-wide failure rather than isolated losses.",
                _settings.ClaimWatchdogMaxAlertsPerTick, _settings.ClaimWatchdogMaxAlertsPerTick);
        }

        _logger.LogInformation(
            "ServiceBusClaimWatchdog | Completed | Stalled={Stalled} Alerted={Alerted} | CutoffUtc={CutoffUtc:O}",
            stalled.Count, toAlert.Count, cutoffUtc);
    }

    void Alert(StalledClaim claim)
    {
        var stalledFor = DateTimeOffset.UtcNow - (claim.LastStageAtUtc ?? claim.ClaimedAtUtc);

        _logger.LogError(
            "ServiceBusClaimWatchdog | CorrelationId={CorrelationId} | STALLED for {StalledForMinutes:F1} min with no result | " +
            "LastStage={LastStage} | LastStageAtUtc={LastStageAtUtc:O} | ClaimedAtUtc={ClaimedAtUtc:O} | " +
            "InstanceId={InstanceId} | ProcessId={ProcessId} | Diagnosis={Diagnosis}",
            claim.CorrelationId, stalledFor.TotalMinutes, claim.LastStage ?? "(none recorded)",
            claim.LastStageAtUtc, claim.ClaimedAtUtc, claim.InstanceId ?? "(unknown)",
            claim.ProcessId ?? "(unknown)", claim.Diagnosis);

        var telemetryEvent = new EventTelemetry(StalledEventName);
        telemetryEvent.Properties["CorrelationId"] = claim.CorrelationId;
        telemetryEvent.Properties["LastStage"] = claim.LastStage ?? string.Empty;
        telemetryEvent.Properties["InstanceId"] = claim.InstanceId ?? string.Empty;
        telemetryEvent.Properties["ProcessId"] = claim.ProcessId ?? string.Empty;
        telemetryEvent.Properties["Diagnosis"] = claim.Diagnosis;
        telemetryEvent.Properties["ClaimedAtUtc"] = claim.ClaimedAtUtc.ToString("O");
        telemetryEvent.Metrics["StalledForMinutes"] = stalledFor.TotalMinutes;
        _telemetry.TrackEvent(telemetryEvent);
    }

    void MarkAlerted(string correlationId)
    {
        try
        {
            using var connection = _storageProvider.Storage.GetConnection();
            connection.SetRangeInHash(
                ClaimHashKeyPrefix + correlationId,
                new[] { new KeyValuePair<string, string>(AlertedAtFieldName, DateTimeOffset.UtcNow.ToString("O")) });
        }
        catch (Exception ex)
        {
            // Worst case the same claim is reported again next tick — far better than losing
            // the alert because the bookkeeping failed.
            _logger.LogWarning(ex, "ServiceBusClaimWatchdog | CorrelationId={CorrelationId} | Could not record the alerted marker; it may be reported again.", correlationId);
        }
    }

    /// <summary>
    /// Claims older than <paramref name="cutoffUtc"/> that have no result row and have not
    /// already been alerted, oldest first, with the instance heartbeat resolved for each.
    /// </summary>
    async Task<List<StalledClaim>> FindStalledClaimsAsync(DateTimeOffset cutoffUtc, int limit)
    {
        await using var connection = new SqlConnection(HangfireStorageFactory.ResolveConnectionString());
        await connection.OpenAsync().ConfigureAwait(false);

        var claims = await ReadStalledClaimsAsync(connection, cutoffUtc, limit).ConfigureAwait(false);
        if (claims.Count == 0)
        {
            return claims;
        }

        var heartbeats = await ReadInstanceHeartbeatsAsync(
            connection,
            claims.Select(c => c.InstanceId).Where(id => !string.IsNullOrEmpty(id)).Distinct(StringComparer.Ordinal)!).ConfigureAwait(false);

        foreach (var claim in claims)
        {
            claim.Diagnosis = Diagnose(claim, heartbeats);
        }

        return claims;
    }

    static async Task<List<StalledClaim>> ReadStalledClaimsAsync(SqlConnection connection, DateTimeOffset cutoffUtc, int limit)
    {
        // Filtering on claimedAtUtc in SQL keeps the scan proportional to what is actually
        // stalled rather than to every claim ever written.
        const string sql = $"""
            SELECT TOP (@Limit)
                   SUBSTRING(c.[Key], LEN('{ClaimHashKeyPrefix}') + 1, 4000) AS CorrelationId,
                   c.[Value]                                                 AS ClaimedAtUtc,
                   MAX(CASE WHEN d.[Field] = '{StageFieldName}'            THEN d.[Value] END) AS LastStage,
                   MAX(CASE WHEN d.[Field] = '{StageAtFieldName}'          THEN d.[Value] END) AS LastStageAtUtc,
                   MAX(CASE WHEN d.[Field] = '{StageInstanceIdFieldName}'  THEN d.[Value] END) AS InstanceId,
                   MAX(CASE WHEN d.[Field] = '{StageProcessIdFieldName}'   THEN d.[Value] END) AS ProcessId
            FROM HangFire.Hash c
            LEFT JOIN HangFire.Hash d ON d.[Key] = c.[Key]
            WHERE c.[Field] = '{ClaimedAtFieldName}'
              AND TRY_CONVERT(datetimeoffset, c.[Value], 127) < @CutoffUtc
              AND c.[Key] LIKE '{ClaimHashKeyPrefix}%'
              AND NOT EXISTS (
                    SELECT 1 FROM HangFire.Hash a
                    WHERE a.[Key] = c.[Key] AND a.[Field] = '{AlertedAtFieldName}')
              AND NOT EXISTS (
                    SELECT 1 FROM HangFire.Hash r
                    WHERE r.[Key] = '{ResultHashKeyPrefix}' + SUBSTRING(c.[Key], LEN('{ClaimHashKeyPrefix}') + 1, 4000))
            GROUP BY c.[Key], c.[Value]
            ORDER BY TRY_CONVERT(datetimeoffset, c.[Value], 127) ASC;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CutoffUtc", cutoffUtc);
        command.Parameters.AddWithValue("@Limit", limit);

        var results = new List<StalledClaim>();
        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            results.Add(new StalledClaim
            {
                CorrelationId  = reader.GetString(0),
                ClaimedAtUtc   = ParseTimestamp(reader.IsDBNull(1) ? null : reader.GetString(1)) ?? DateTimeOffset.MinValue,
                LastStage      = reader.IsDBNull(2) ? null : reader.GetString(2),
                LastStageAtUtc = ParseTimestamp(reader.IsDBNull(3) ? null : reader.GetString(3)),
                InstanceId     = reader.IsDBNull(4) ? null : reader.GetString(4),
                ProcessId      = reader.IsDBNull(5) ? null : reader.GetString(5),
            });
        }

        return results;
    }

    static async Task<Dictionary<string, InstanceHeartbeat>> ReadInstanceHeartbeatsAsync(SqlConnection connection, IEnumerable<string> instanceIds)
    {
        var heartbeats = new Dictionary<string, InstanceHeartbeat>(StringComparer.Ordinal);

        var ids = instanceIds.ToList();
        if (ids.Count == 0)
        {
            return heartbeats;
        }

        var parameterNames = ids.Select((_, index) => "@Id" + index).ToList();
        var sql = $"""
            SELECT h.[Key],
                   MAX(CASE WHEN h.[Field] = '{InstanceHeartbeatAtFieldName}' THEN h.[Value] END) AS LastHeartbeatUtc,
                   MAX(CASE WHEN h.[Field] = '{InstanceProcessIdFieldName}'   THEN h.[Value] END) AS ProcessId
            FROM HangFire.Hash h
            WHERE h.[Key] IN ({string.Join(", ", parameterNames.Select(name => $"'{InstanceHashKeyPrefix}' + {name}"))})
            GROUP BY h.[Key];
            """;

        await using var command = new SqlCommand(sql, connection);
        for (var index = 0; index < ids.Count; index++)
        {
            command.Parameters.AddWithValue(parameterNames[index], ids[index]);
        }

        await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            var instanceId = reader.GetString(0)[InstanceHashKeyPrefix.Length..];
            heartbeats[instanceId] = new InstanceHeartbeat(
                ParseTimestamp(reader.IsDBNull(1) ? null : reader.GetString(1)),
                reader.IsDBNull(2) ? null : reader.GetString(2));
        }

        return heartbeats;
    }

    /// <summary>
    /// Turns the claim + heartbeat pair into the distinction the breadcrumb exists to make:
    /// did the whole instance go, did only its worker process go, or is the instance alive and
    /// this one execution hung? Each implies a different fix, and guessing wrong costs hours
    /// (the build-30655 investigation is exactly this question answered by hand).
    /// </summary>
    internal static string Diagnose(StalledClaim claim, IReadOnlyDictionary<string, InstanceHeartbeat> heartbeats)
    {
        if (string.IsNullOrEmpty(claim.InstanceId) || !heartbeats.TryGetValue(claim.InstanceId, out var heartbeat))
        {
            return "NoInstanceHeartbeat: nothing is recording liveness for the instance that took this claim.";
        }

        if (!string.IsNullOrEmpty(claim.ProcessId)
            && !string.IsNullOrEmpty(heartbeat.ProcessId)
            && !string.Equals(claim.ProcessId, heartbeat.ProcessId, StringComparison.Ordinal))
        {
            return $"WorkerProcessRestarted: the instance is alive under process {heartbeat.ProcessId}, but this claim was taken by process {claim.ProcessId} — that process died mid-execution.";
        }

        var lastStageAt = claim.LastStageAtUtc ?? claim.ClaimedAtUtc;
        if (heartbeat.LastHeartbeatUtc is { } beatAt && beatAt > lastStageAt)
        {
            return "InstanceAliveExecutionStalled: the instance has kept heartbeating since this claim last moved, so the execution itself is hung rather than the host being gone.";
        }

        return "InstanceStoppedHeartbeating: the instance's own heartbeat stops at or before this claim's last checkpoint — the host went down with the execution.";
    }

    static DateTimeOffset? ParseTimestamp(string? value)
        => DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;

    internal sealed class StalledClaim
    {
        public required string CorrelationId { get; init; }
        public DateTimeOffset ClaimedAtUtc { get; init; }
        public string? LastStage { get; init; }
        public DateTimeOffset? LastStageAtUtc { get; init; }
        public string? InstanceId { get; init; }
        public string? ProcessId { get; init; }
        public string Diagnosis { get; set; } = string.Empty;
    }

    internal readonly record struct InstanceHeartbeat(DateTimeOffset? LastHeartbeatUtc, string? ProcessId);
}
