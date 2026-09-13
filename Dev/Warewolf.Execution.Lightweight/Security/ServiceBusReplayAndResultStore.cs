/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Collections.Concurrent;
using Dev2.Common;
using Hangfire;
using Newtonsoft.Json;
using Warewolf.Driver.Persistence;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Security;

/// <summary>
/// <see cref="IServiceBusReplayAndResultStore"/> backed by the engine's existing
/// Hangfire/SQL persistence store (<c>Config.Persistence</c>, the same store used for
/// suspend/resume) when persistence is enabled, or an in-process cache otherwise.
///
/// <para>
/// No new SQL schema/migration is introduced: entries are stored as generic Hangfire
/// hash rows (<c>IStorageConnection.GetAllEntriesFromHash</c> /
/// <c>SetRangeInHash</c>), the same mechanism Hangfire itself uses for extension data.
/// jti registration is guarded by a Hangfire distributed lock scoped to the specific
/// jti so the check-then-set is atomic across multiple engine instances sharing one
/// SQL Server persistence store.
/// </para>
///
/// <para>
/// <b>In-memory fallback caveat.</b> When <c>Config.Persistence.Enable</c> is
/// <c>false</c> (no durable store configured), jti replay protection and idempotency
/// dedupe are process-local only — safe for a single engine instance, but NOT safe
/// across multiple instances behind a load balancer or multiple Consumption-plan
/// workers, where a replayed message could reach a different instance than the one
/// that first processed it. This is documented, accepted scope for this pass (see
/// <c>docs/ServiceBusSecureTrigger-Architecture.md</c>); operators that need
/// multi-instance-safe replay protection must enable persistence.
/// </para>
///
/// <para>
/// <b>Known limitation.</b> Hangfire hash entries have no built-in TTL/auto-eviction.
/// jti and result rows accumulate indefinitely; a follow-up cleanup job (or manual
/// periodic purge) is a documented, accepted follow-up — implementing a full
/// expiry-sweep is out of scope for this pass.
/// </para>
///
/// <para>
/// <b>Claim staleness.</b> A claim taken by <see cref="TryClaim"/> is normally released
/// by <see cref="ReleaseClaim"/> (transient/unexpected-failure paths) or superseded by a
/// saved result (success/terminal-failure paths). If the attempt holding the claim dies
/// without doing either — e.g. the host process recycles mid-execution, or the execution
/// itself hangs indefinitely with no cancellation path back into this code — the claim
/// would otherwise block every future redelivery of that correlation id forever, with no
/// result ever recorded (see the 1000-message ShovelBridge load test incident of
/// 2026-08-24: 21 correlation ids stuck exactly this way). <see cref="TryClaim"/>
/// therefore treats a claim older than the configured claim-staleness window (see
/// <see cref="DefaultClaimStaleAfter"/> and each constructor's optional
/// <c>claimStaleAfter</c> parameter — normally
/// <see cref="Auth.Models.ServiceBusTriggerOptions.ClaimStaleAfter"/>) as abandoned and
/// lets a new delivery take it over.
/// </para>
/// </summary>
public sealed class ServiceBusReplayAndResultStore : IServiceBusReplayAndResultStore
{
    private const string JtiHashKey = "sbtrigger:jti-seen";
    private const string ResultHashKeyPrefix = "sbtrigger:result:";
    private const string ResultFieldName = "json";
    private const string ClaimHashKeyPrefix = "sbtrigger:claim:";
    private const string ClaimFieldName = "claimedAtUtc";
    private const string StageFieldName = "lastStage";
    private const string StageAtFieldName = "lastStageAtUtc";
    private const string StageInstanceIdFieldName = "lastStageInstanceId";
    private const string StageProcessIdFieldName = "lastStageProcessId";
    private const string InstanceHashKeyPrefix = "sbtrigger:instance:";
    private const string InstanceHeartbeatAtFieldName = "lastHeartbeatUtc";
    private const string InstanceInFlightFieldName = "inFlightExecutions";
    private const string InstanceProcessIdFieldName = "processId";
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// WOLF-8512: this instance's stable identifier, for <see cref="RecordClaimStage"/>'s
    /// and <see cref="RecordInstanceHeartbeat"/>'s durable rows. Deliberately mirrors
    /// <c>HostEnvironmentConfig.InstanceId</c>'s exact resolution rather than taking a
    /// dependency on it: this store is constructed in many tests via
    /// <c>new ServiceBusReplayAndResultStore(...)</c> with no <c>HostEnvironmentConfig</c>
    /// available, and — as documented on <c>InstanceCorrelationMiddleware.InstanceId</c> —
    /// this codebase already keeps more than one independently-resolved instance-id
    /// constant rather than force a shared dependency between unrelated layers.
    /// </summary>
    private static readonly string CurrentInstanceId =
        Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;

    private static readonly string CurrentProcessId = Environment.ProcessId.ToString();

    private readonly ConcurrentDictionary<string, (int InFlight, DateTimeOffset AtUtc)> _inMemoryInstanceHeartbeats = new(StringComparer.Ordinal);

    /// <summary>
    /// Fallback used when no <c>claimStaleAfter</c> is supplied to a constructor — i.e. only
    /// when the caller opted out of <see cref="Auth.Models.ServiceBusTriggerOptions.ClaimStaleAfter"/>'s
    /// own runtime-resolved default (see that property's doc comment). Kept as a fixed 20
    /// minutes purely as a last-resort safety net matching this class's historical behaviour;
    /// production wiring (<c>Infrastructure.ServiceCollectionExtensions</c>) always passes the
    /// resolved value explicitly.
    /// </summary>
    internal static readonly TimeSpan DefaultClaimStaleAfter = TimeSpan.FromMinutes(20);

    private readonly bool _usePersistence;
    private readonly Lazy<JobStorage>? _jobStorage;
    private readonly Func<DateTimeOffset> _clock;
    private readonly TimeSpan _claimStaleAfter;

    private readonly ConcurrentDictionary<string, byte> _inMemoryJti = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _inMemoryResults = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, DateTimeOffset> _inMemoryClaims = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, (string Stage, DateTimeOffset AtUtc)> _inMemoryClaimStages = new(StringComparer.Ordinal);

    public ServiceBusReplayAndResultStore(TimeSpan? claimStaleAfter = null)
        : this(() => DateTimeOffset.UtcNow, claimStaleAfter)
    {
    }

    /// <summary>Test seam — injects a controllable clock so claim-staleness tests don't need to sleep for the real TTL. Persistence mode is still resolved from <c>Config.Persistence</c>, same as the public constructor.</summary>
    internal ServiceBusReplayAndResultStore(Func<DateTimeOffset> clock, TimeSpan? claimStaleAfter = null)
    {
        _clock = clock;
        _claimStaleAfter = claimStaleAfter ?? DefaultClaimStaleAfter;
        _usePersistence = Config.Persistence?.Enable ?? false;
        if (_usePersistence)
        {
            _jobStorage = new Lazy<JobStorage>(
                HangfireStorageFactory.BuildFromPersistenceConfig,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    /// <summary>Test seam — injects a pre-built <see cref="JobStorage"/> (e.g. Hangfire's <c>MemoryStorage</c>) instead of resolving persistence config.</summary>
    internal ServiceBusReplayAndResultStore(JobStorage jobStorage, TimeSpan? claimStaleAfter = null)
        : this(jobStorage, () => DateTimeOffset.UtcNow, claimStaleAfter)
    {
    }

    /// <summary>Test seam — as above, plus a controllable clock for deterministic stale-claim tests against the Hangfire-backed path.</summary>
    internal ServiceBusReplayAndResultStore(JobStorage jobStorage, Func<DateTimeOffset> clock, TimeSpan? claimStaleAfter = null)
    {
        _usePersistence = true;
        _jobStorage = new Lazy<JobStorage>(() => jobStorage);
        _clock = clock;
        _claimStaleAfter = claimStaleAfter ?? DefaultClaimStaleAfter;
    }

    /// <inheritdoc/>
    public bool TryRegisterJti(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            // No jti on the token to dedupe on. Whether that is acceptable is a token-
            // issuance policy decision, not something this store enforces.
            return true;
        }

        if (!_usePersistence)
        {
            return _inMemoryJti.TryAdd(jti, 0);
        }

        using var connection = _jobStorage!.Value.GetConnection();
        using (connection.AcquireDistributedLock($"sbtrigger:jti-lock:{jti}", LockTimeout))
        {
            var existing = connection.GetAllEntriesFromHash(JtiHashKey);
            if (existing != null && existing.ContainsKey(jti))
            {
                return false;
            }

            connection.SetRangeInHash(
                JtiHashKey,
                new[] { new KeyValuePair<string, string>(jti, DateTimeOffset.UtcNow.ToString("O")) });
            return true;
        }
    }

    /// <inheritdoc/>
    public bool TryGetResult(string correlationId, out ServiceBusTriggerResult? result)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            result = null;
            return false;
        }

        string? json = null;
        if (!_usePersistence)
        {
            _inMemoryResults.TryGetValue(correlationId, out json);
        }
        else
        {
            using var connection = _jobStorage!.Value.GetConnection();
            var hash = connection.GetAllEntriesFromHash(ResultHashKeyPrefix + correlationId);
            hash?.TryGetValue(ResultFieldName, out json);
        }

        if (string.IsNullOrEmpty(json))
        {
            result = null;
            return false;
        }

        result = JsonConvert.DeserializeObject<ServiceBusTriggerResult>(json);
        return result != null;
    }

    /// <inheritdoc/>
    public void SaveResult(ServiceBusTriggerResult result)
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));
        if (string.IsNullOrWhiteSpace(result.CorrelationId))
            return;

        var json = JsonConvert.SerializeObject(result);

        if (!_usePersistence)
        {
            _inMemoryResults[result.CorrelationId] = json;
            return;
        }

        using var connection = _jobStorage!.Value.GetConnection();
        connection.SetRangeInHash(
            ResultHashKeyPrefix + result.CorrelationId,
            new[] { new KeyValuePair<string, string>(ResultFieldName, json) });
    }

    /// <inheritdoc/>
    public bool TryClaim(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            // No correlation id to dedupe on - never treated as a claim conflict, same
            // convention TryRegisterJti uses for an empty jti.
            return true;
        }

        var now = _clock();

        if (!_usePersistence)
        {
            if (_inMemoryResults.ContainsKey(correlationId))
            {
                return false;
            }

            if (_inMemoryClaims.TryAdd(correlationId, now))
            {
                return true;
            }

            // A claim already exists - if it is old enough that the attempt holding it
            // must be dead or permanently hung (see ClaimStaleAfter), steal it. The
            // TryUpdate only succeeds if the claim timestamp we just read is still the
            // current one, so concurrent stale-claim attempts still yield exactly one
            // winner.
            if (_inMemoryClaims.TryGetValue(correlationId, out var claimedAt)
                && now - claimedAt > _claimStaleAfter
                && _inMemoryClaims.TryUpdate(correlationId, now, claimedAt))
            {
                return true;
            }

            return false;
        }

        using var connection = _jobStorage!.Value.GetConnection();
        using (connection.AcquireDistributedLock($"sbtrigger:corr-lock:{correlationId}", LockTimeout))
        {
            var resultHash = connection.GetAllEntriesFromHash(ResultHashKeyPrefix + correlationId);
            if (resultHash != null && resultHash.ContainsKey(ResultFieldName))
            {
                // A terminal result already exists - genuine duplicate, not a race.
                return false;
            }

            var claimHash = connection.GetAllEntriesFromHash(ClaimHashKeyPrefix + correlationId);
            if (claimHash != null && claimHash.TryGetValue(ClaimFieldName, out var claimedAtRaw))
            {
                var claimedAt = DateTimeOffset.Parse(claimedAtRaw, null, System.Globalization.DateTimeStyles.RoundtripKind);
                if (now - claimedAt <= _claimStaleAfter)
                {
                    // Another delivery already holds the claim and hasn't finished (or
                    // failed without releasing it) yet, and not long enough ago to treat
                    // as abandoned.
                    return false;
                }
                // Else: stale - the attempt that took this claim is presumed dead/hung.
                // Fall through and overwrite it below; safe because we still hold this
                // correlation id's distributed lock.
            }

            connection.SetRangeInHash(
                ClaimHashKeyPrefix + correlationId,
                new[] { new KeyValuePair<string, string>(ClaimFieldName, now.ToString("O")) });
            return true;
        }
    }

    /// <inheritdoc/>
    public void ReleaseClaim(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        if (!_usePersistence)
        {
            _inMemoryClaims.TryRemove(correlationId, out _);
            return;
        }

        using var connection = _jobStorage!.Value.GetConnection();
        using (connection.AcquireDistributedLock($"sbtrigger:corr-lock:{correlationId}", LockTimeout))
        {
            using var transaction = connection.CreateWriteTransaction();
            transaction.RemoveHash(ClaimHashKeyPrefix + correlationId);
            transaction.Commit();
        }
    }

    /// <inheritdoc/>
    public void RecordClaimStage(string correlationId, string stage)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        var now = _clock();

        if (!_usePersistence)
        {
            _inMemoryClaimStages[correlationId] = (stage, now);
            return;
        }

        // Best-effort: merged into the SAME hash TryClaim already wrote (no new key, no
        // lock — this is a diagnostic breadcrumb, not something correctness depends on),
        // so a failure here must never fail or delay the execution it is observing. See
        // the interface doc comment for why this exists.
        try
        {
            using var connection = _jobStorage!.Value.GetConnection();
            connection.SetRangeInHash(
                ClaimHashKeyPrefix + correlationId,
                new[]
                {
                    new KeyValuePair<string, string>(StageFieldName, stage),
                    new KeyValuePair<string, string>(StageAtFieldName, now.ToString("O")),
                    // WOLF-8512 (build 30653 addendum): which instance/process reached this
                    // stage, so a future recurrence can tell whether two stuck correlation ids
                    // died on the SAME instance (see RecordInstanceHeartbeat's doc comment for
                    // the other half of this question).
                    new KeyValuePair<string, string>(StageInstanceIdFieldName, CurrentInstanceId),
                    new KeyValuePair<string, string>(StageProcessIdFieldName, CurrentProcessId),
                });
        }
        catch (Exception ex)
        {
            Dev2Logger.Warn($"ServiceBusReplayAndResultStore.RecordClaimStage failed to persist stage '{stage}' — continuing without it.", ex, correlationId);
        }
    }

    /// <inheritdoc/>
    public void RecordInstanceHeartbeat(string instanceId, int inFlightExecutions)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            return;
        }

        var now = _clock();

        if (!_usePersistence)
        {
            _inMemoryInstanceHeartbeats[instanceId] = (inFlightExecutions, now);
            return;
        }

        // Best-effort, same guarantee as RecordClaimStage - see that method's comment.
        try
        {
            using var connection = _jobStorage!.Value.GetConnection();
            connection.SetRangeInHash(
                InstanceHashKeyPrefix + instanceId,
                new[]
                {
                    new KeyValuePair<string, string>(InstanceHeartbeatAtFieldName, now.ToString("O")),
                    new KeyValuePair<string, string>(InstanceInFlightFieldName, inFlightExecutions.ToString()),
                    new KeyValuePair<string, string>(InstanceProcessIdFieldName, CurrentProcessId),
                });
        }
        catch (Exception ex)
        {
            Dev2Logger.Warn($"ServiceBusReplayAndResultStore.RecordInstanceHeartbeat failed to persist heartbeat for instance '{instanceId}' — continuing without it.", ex, instanceId);
        }
    }

    /// <summary>Test seam: reads back what <see cref="RecordClaimStage"/> last wrote for <paramref name="correlationId"/> — <c>null</c> stage if none was ever recorded.</summary>
    internal (string? Stage, DateTimeOffset? AtUtc) GetClaimStageForTests(string correlationId)
    {
        if (!_usePersistence)
        {
            return _inMemoryClaimStages.TryGetValue(correlationId, out var entry)
                ? (entry.Stage, entry.AtUtc)
                : (null, null);
        }

        using var connection = _jobStorage!.Value.GetConnection();
        var hash = connection.GetAllEntriesFromHash(ClaimHashKeyPrefix + correlationId);
        if (hash == null || !hash.TryGetValue(StageFieldName, out var stage))
        {
            return (null, null);
        }

        var atUtc = hash.TryGetValue(StageAtFieldName, out var atRaw)
            ? DateTimeOffset.Parse(atRaw, null, System.Globalization.DateTimeStyles.RoundtripKind)
            : (DateTimeOffset?)null;
        return (stage, atUtc);
    }

    /// <summary>
    /// Test seam: reads back the instance/process identity <see cref="RecordClaimStage"/>
    /// last wrote alongside <paramref name="correlationId"/>'s stage — <c>null</c>s if no
    /// stage was ever recorded (in-memory mode always returns <c>null</c>s: instance/process
    /// identity is only persisted on the Hangfire-backed path, matching this class's other
    /// diagnostic-only fields).
    /// </summary>
    internal (string? InstanceId, string? ProcessId) GetClaimInstanceForTests(string correlationId)
    {
        if (!_usePersistence)
        {
            return (null, null);
        }

        using var connection = _jobStorage!.Value.GetConnection();
        var hash = connection.GetAllEntriesFromHash(ClaimHashKeyPrefix + correlationId);
        if (hash == null)
        {
            return (null, null);
        }

        hash.TryGetValue(StageInstanceIdFieldName, out var instanceId);
        hash.TryGetValue(StageProcessIdFieldName, out var processId);
        return (instanceId, processId);
    }

    /// <summary>Test seam: reads back what <see cref="RecordInstanceHeartbeat"/> last wrote for <paramref name="instanceId"/> — <c>null</c>s if none was ever recorded.</summary>
    internal (int? InFlightExecutions, DateTimeOffset? AtUtc) GetInstanceHeartbeatForTests(string instanceId)
    {
        if (!_usePersistence)
        {
            return _inMemoryInstanceHeartbeats.TryGetValue(instanceId, out var entry)
                ? (entry.InFlight, entry.AtUtc)
                : (null, null);
        }

        using var connection = _jobStorage!.Value.GetConnection();
        var hash = connection.GetAllEntriesFromHash(InstanceHashKeyPrefix + instanceId);
        if (hash == null || !hash.TryGetValue(InstanceHeartbeatAtFieldName, out var atRaw))
        {
            return (null, null);
        }

        var inFlight = hash.TryGetValue(InstanceInFlightFieldName, out var inFlightRaw) && int.TryParse(inFlightRaw, out var parsed)
            ? parsed
            : (int?)null;
        var atUtc = DateTimeOffset.Parse(atRaw, null, System.Globalization.DateTimeStyles.RoundtripKind);
        return (inFlight, atUtc);
    }
}
