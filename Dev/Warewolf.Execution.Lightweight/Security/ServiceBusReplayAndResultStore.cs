/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Collections.Concurrent;
using Dev2.Common;
using Hangfire;
using Hangfire.Storage;
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
///
/// <para>
/// <b>Liveness-aware takeover (WOLF-8512, build 30658).</b> Waiting out the staleness window
/// is correct but slow: it is a worst-case bound (11 minutes by default) chosen so it can never
/// cut short a genuinely-still-running execution, which costs several wasted redeliveries when
/// the owner is in fact already dead. <see cref="TryClaim"/> therefore also consults the owning
/// instance's heartbeat row before refusing a claim — see <c>IsClaimOrphaned</c>. The decisive
/// signal is a heartbeat arriving from a different PROCESS id than the one recorded on the
/// claim: that proves the claiming worker process was replaced, with no timing guesswork. In
/// build 30658 the Functions host (and therefore the role instance) survived while the isolated
/// worker crashed underneath it, so a per-instance check alone would not have caught it.
/// Anything ambiguous is reported as "still held" — a false takeover means duplicate execution.
/// </para>
/// </summary>
public sealed class ServiceBusReplayAndResultStore : IServiceBusReplayAndResultStore
{
    private const string JtiHashKey = "sbtrigger:jti-seen";
    private const string ResultHashKeyPrefix = "sbtrigger:result:";
    private const string ResultFieldName = "json";
    private const string ClaimHashKeyPrefix = "sbtrigger:claim:";
    private const string ClaimFieldName = "claimedAtUtc";
    // WOLF-8512 (build 30658): who took the claim. Distinct from the Stage* fields below,
    // which every later checkpoint overwrites - these two are written once, by TryClaim, and
    // are what the liveness-aware takeover compares the owner's heartbeat row against.
    private const string ClaimInstanceIdFieldName = "claimedByInstanceId";
    private const string ClaimProcessIdFieldName = "claimedByProcessId";
    private const string StageFieldName = "lastStage";
    private const string StageAtFieldName = "lastStageAtUtc";
    private const string StageInstanceIdFieldName = "lastStageInstanceId";
    private const string StageProcessIdFieldName = "lastStageProcessId";
    // WOLF-8512 (build 30658): ReleaseClaim's tombstone - what the claim had reached when it
    // was handed back, kept because removing the hash outright erased the whole breadcrumb
    // trail for every transiently-failed correlation id.
    private const string ReleasedAtFieldName = "releasedAtUtc";
    private const string ReleasedStageFieldName = "releasedAtStage";
    private const string ReleasedByInstanceIdFieldName = "releasedByInstanceId";
    private const string ReleasedByProcessIdFieldName = "releasedByProcessId";
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
    private static readonly string DefaultInstanceId =
        Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Environment.MachineName;

    private static readonly string DefaultProcessId = Environment.ProcessId.ToString();

    /// <summary>
    /// This store's view of "who am I". Instance fields rather than statics so tests can drive
    /// the liveness-aware takeover in <see cref="TryClaim"/> deterministically — a second store
    /// constructed with a different identity stands in for a different instance/worker process,
    /// which is otherwise impossible to simulate in one test process.
    /// </summary>
    private readonly string _instanceId;

    private readonly string _processId;

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

    /// <summary>
    /// Fallback for <see cref="_instanceLivenessTimeout"/>, matching
    /// <c>ServiceBusTriggerOptions.DefaultInstanceLivenessTimeout</c>. Production wiring always
    /// passes the resolved value explicitly.
    /// </summary>
    internal static readonly TimeSpan DefaultInstanceLivenessTimeout = TimeSpan.FromSeconds(30);

    private readonly bool _usePersistence;
    private readonly Lazy<JobStorage>? _jobStorage;
    private readonly Func<DateTimeOffset> _clock;
    private readonly TimeSpan _claimStaleAfter;
    private readonly TimeSpan _instanceLivenessTimeout;

    private readonly ConcurrentDictionary<string, byte> _inMemoryJti = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _inMemoryResults = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, DateTimeOffset> _inMemoryClaims = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, (string Stage, DateTimeOffset AtUtc)> _inMemoryClaimStages = new(StringComparer.Ordinal);

    public ServiceBusReplayAndResultStore(TimeSpan? claimStaleAfter = null, TimeSpan? instanceLivenessTimeout = null)
        : this(() => DateTimeOffset.UtcNow, claimStaleAfter, instanceLivenessTimeout)
    {
    }

    /// <summary>Test seam — injects a controllable clock so claim-staleness tests don't need to sleep for the real TTL. Persistence mode is still resolved from <c>Config.Persistence</c>, same as the public constructor.</summary>
    internal ServiceBusReplayAndResultStore(
        Func<DateTimeOffset> clock,
        TimeSpan? claimStaleAfter = null,
        TimeSpan? instanceLivenessTimeout = null,
        string? instanceId = null,
        string? processId = null)
    {
        _clock = clock;
        _claimStaleAfter = claimStaleAfter ?? DefaultClaimStaleAfter;
        _instanceLivenessTimeout = instanceLivenessTimeout ?? DefaultInstanceLivenessTimeout;
        _instanceId = instanceId ?? DefaultInstanceId;
        _processId = processId ?? DefaultProcessId;
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

    /// <summary>
    /// Test seam — as above, plus a controllable clock for deterministic stale-claim tests
    /// against the Hangfire-backed path, and (WOLF-8512, build 30658) an overridable
    /// instance/process identity so a test can construct a SECOND store that stands in for a
    /// different instance or a restarted worker process sharing the same storage. That is the
    /// only way to drive <see cref="TryClaim"/>'s liveness-aware takeover in one test process.
    /// </summary>
    internal ServiceBusReplayAndResultStore(
        JobStorage jobStorage,
        Func<DateTimeOffset> clock,
        TimeSpan? claimStaleAfter = null,
        TimeSpan? instanceLivenessTimeout = null,
        string? instanceId = null,
        string? processId = null)
    {
        _usePersistence = true;
        _jobStorage = new Lazy<JobStorage>(() => jobStorage);
        _clock = clock;
        _claimStaleAfter = claimStaleAfter ?? DefaultClaimStaleAfter;
        _instanceLivenessTimeout = instanceLivenessTimeout ?? DefaultInstanceLivenessTimeout;
        _instanceId = instanceId ?? DefaultInstanceId;
        _processId = processId ?? DefaultProcessId;
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
                if (now - claimedAt <= _claimStaleAfter
                    && !IsClaimOrphaned(connection, claimHash, correlationId, now))
                {
                    // Another delivery already holds the claim, hasn't finished (or failed
                    // without releasing it) yet, not long enough ago to treat as abandoned,
                    // and its owner still looks alive.
                    return false;
                }
                // Else: stale by the clock, OR provably orphaned by the liveness check - the
                // attempt that took this claim is dead. Fall through and overwrite it below;
                // safe because we still hold this correlation id's distributed lock.
            }

            connection.SetRangeInHash(
                ClaimHashKeyPrefix + correlationId,
                new[]
                {
                    new KeyValuePair<string, string>(ClaimFieldName, now.ToString("O")),
                    new KeyValuePair<string, string>(ClaimInstanceIdFieldName, _instanceId),
                    new KeyValuePair<string, string>(ClaimProcessIdFieldName, _processId),
                });
            return true;
        }
    }

    /// <summary>
    /// WOLF-8512 (build 30658): decides whether a claim that is NOT yet stale by the clock is
    /// nonetheless provably orphaned, so the caller can take it over on the very next
    /// redelivery instead of waiting out <see cref="_claimStaleAfter"/>.
    ///
    /// <para>
    /// Two signals, strongest first:
    /// </para>
    /// <list type="number">
    ///   <item><b>Process replaced.</b> The owning instance is heartbeating, but from a
    ///         DIFFERENT process id than the one that took the claim. The claiming worker
    ///         process is therefore gone and its in-flight execution went with it. This needs no
    ///         timing judgement and cannot false-positive on a slow execution — it is exactly
    ///         what happened on instances 813073bd and 5ae10c76 in build 30658, where the
    ///         isolated worker crashed and restarted under a live Functions host (the role
    ///         instance kept logging, so a per-instance check alone would have missed it).</item>
    ///   <item><b>Instance gone.</b> The owning instance has not heartbeated within
    ///         <see cref="_instanceLivenessTimeout"/> — it scaled in or died outright.</item>
    /// </list>
    ///
    /// <para>
    /// Deliberately conservative: with no owner recorded, or no heartbeat row for that owner at
    /// all, we cannot distinguish dead from merely quiet, so we report "not orphaned" and let
    /// the existing <see cref="_claimStaleAfter"/> backstop handle it. A false positive here
    /// means two concurrent executions of the same message — for the ProfilerMain load test
    /// that is a duplicate DB row, which the test explicitly asserts against — so "don't know"
    /// must always mean "leave it alone". Best-effort throughout: any failure reading the
    /// heartbeat row is treated as "not orphaned".
    /// </para>
    /// </summary>
    private bool IsClaimOrphaned(
        IStorageConnection connection,
        IReadOnlyDictionary<string, string> claimHash,
        string correlationId,
        DateTimeOffset now)
    {
        try
        {
            if (!claimHash.TryGetValue(ClaimInstanceIdFieldName, out var ownerInstanceId)
                || string.IsNullOrWhiteSpace(ownerInstanceId))
            {
                // Claim predates this field (or was written by an older engine build) - no
                // owner to check. Time-based staleness remains the only backstop.
                return false;
            }

            var instanceHash = connection.GetAllEntriesFromHash(InstanceHashKeyPrefix + ownerInstanceId);
            if (instanceHash == null
                || !instanceHash.TryGetValue(InstanceHeartbeatAtFieldName, out var heartbeatRaw))
            {
                // The owner has never heartbeated. Can't tell "died before its first tick"
                // from "heartbeat service disabled", so don't steal.
                return false;
            }

            claimHash.TryGetValue(ClaimProcessIdFieldName, out var claimProcessId);
            instanceHash.TryGetValue(InstanceProcessIdFieldName, out var livingProcessId);

            if (!string.IsNullOrWhiteSpace(claimProcessId)
                && !string.IsNullOrWhiteSpace(livingProcessId)
                && !string.Equals(claimProcessId, livingProcessId, StringComparison.Ordinal))
            {
                Dev2Logger.Warn(
                    $"ServiceBusReplayAndResultStore.TryClaim taking over an orphaned claim: instance '{ownerInstanceId}' is alive but now runs process '{livingProcessId}', while the claim was taken by process '{claimProcessId}' — that worker process is gone.",
                    correlationId);
                return true;
            }

            var lastHeartbeat = DateTimeOffset.Parse(heartbeatRaw, null, System.Globalization.DateTimeStyles.RoundtripKind);
            if (now - lastHeartbeat > _instanceLivenessTimeout)
            {
                Dev2Logger.Warn(
                    $"ServiceBusReplayAndResultStore.TryClaim taking over an orphaned claim: owning instance '{ownerInstanceId}' last heartbeated at {lastHeartbeat:O}, more than {_instanceLivenessTimeout} ago.",
                    correlationId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            // Diagnostic-quality data driving a correctness decision - if it is unreadable,
            // fail safe to "not orphaned" rather than risk a duplicate execution.
            Dev2Logger.Warn(
                "ServiceBusReplayAndResultStore.IsClaimOrphaned could not evaluate claim liveness — treating the claim as still held.",
                ex,
                correlationId);
            return false;
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
            // WOLF-8512 (build 30658): removing the hash outright also destroyed the
            // RecordClaimStage breadcrumb trail, so a correlation id that failed transiently
            // and was released left nothing to diagnose - the release erased the evidence of
            // how far it had got. Capture the last stage first, then re-write it as a
            // tombstone. Deliberately WITHOUT ClaimFieldName, so TryClaim still sees this
            // correlation id as unclaimed and a redelivery proceeds normally.
            var previous = connection.GetAllEntriesFromHash(ClaimHashKeyPrefix + correlationId);
            var stageAtRelease = previous != null && previous.TryGetValue(StageFieldName, out var stage)
                ? stage
                : "(no stage recorded)";

            using (var transaction = connection.CreateWriteTransaction())
            {
                transaction.RemoveHash(ClaimHashKeyPrefix + correlationId);
                transaction.Commit();
            }

            try
            {
                connection.SetRangeInHash(
                    ClaimHashKeyPrefix + correlationId,
                    new[]
                    {
                        new KeyValuePair<string, string>(ReleasedAtFieldName, _clock().ToString("O")),
                        new KeyValuePair<string, string>(ReleasedStageFieldName, stageAtRelease),
                        new KeyValuePair<string, string>(ReleasedByInstanceIdFieldName, _instanceId),
                        new KeyValuePair<string, string>(ReleasedByProcessIdFieldName, _processId),
                    });
            }
            catch (Exception ex)
            {
                // Pure diagnostics - the release itself already committed above, which is the
                // part correctness depends on.
                Dev2Logger.Warn(
                    "ServiceBusReplayAndResultStore.ReleaseClaim could not write the release tombstone — the claim was still released.",
                    ex,
                    correlationId);
            }
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
                    new KeyValuePair<string, string>(StageInstanceIdFieldName, _instanceId),
                    new KeyValuePair<string, string>(StageProcessIdFieldName, _processId),
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
                    new KeyValuePair<string, string>(InstanceProcessIdFieldName, _processId),
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

    /// <summary>
    /// Test seam: reads back the tombstone <see cref="ReleaseClaim"/> leaves behind — the stage
    /// the released claim had reached, and who released it. <c>null</c>s when no claim for
    /// <paramref name="correlationId"/> has been released (in-memory mode always returns
    /// <c>null</c>s: the tombstone is only written on the Hangfire-backed path, matching this
    /// class's other diagnostic-only fields).
    /// </summary>
    internal (string? Stage, DateTimeOffset? AtUtc, string? InstanceId) GetReleaseTombstoneForTests(string correlationId)
    {
        if (!_usePersistence)
        {
            return (null, null, null);
        }

        using var connection = _jobStorage!.Value.GetConnection();
        var hash = connection.GetAllEntriesFromHash(ClaimHashKeyPrefix + correlationId);
        if (hash == null || !hash.TryGetValue(ReleasedAtFieldName, out var atRaw))
        {
            return (null, null, null);
        }

        hash.TryGetValue(ReleasedStageFieldName, out var stage);
        hash.TryGetValue(ReleasedByInstanceIdFieldName, out var instanceId);
        return (stage, DateTimeOffset.Parse(atRaw, null, System.Globalization.DateTimeStyles.RoundtripKind), instanceId);
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
