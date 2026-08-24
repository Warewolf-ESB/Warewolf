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
/// </summary>
public sealed class ServiceBusReplayAndResultStore : IServiceBusReplayAndResultStore
{
    private const string JtiHashKey = "sbtrigger:jti-seen";
    private const string ResultHashKeyPrefix = "sbtrigger:result:";
    private const string ResultFieldName = "json";
    private const string ClaimHashKeyPrefix = "sbtrigger:claim:";
    private const string ClaimFieldName = "claimedAtUtc";
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(10);

    private readonly bool _usePersistence;
    private readonly Lazy<JobStorage>? _jobStorage;

    private readonly ConcurrentDictionary<string, byte> _inMemoryJti = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _inMemoryResults = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _inMemoryClaims = new(StringComparer.Ordinal);

    public ServiceBusReplayAndResultStore()
    {
        _usePersistence = Config.Persistence?.Enable ?? false;
        if (_usePersistence)
        {
            _jobStorage = new Lazy<JobStorage>(
                HangfireStorageFactory.BuildFromPersistenceConfig,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    /// <summary>Test seam — injects a pre-built <see cref="JobStorage"/> (e.g. Hangfire's <c>MemoryStorage</c>) instead of resolving persistence config.</summary>
    internal ServiceBusReplayAndResultStore(JobStorage jobStorage)
    {
        _usePersistence = true;
        _jobStorage = new Lazy<JobStorage>(() => jobStorage);
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

        if (!_usePersistence)
        {
            if (_inMemoryResults.ContainsKey(correlationId))
            {
                return false;
            }

            return _inMemoryClaims.TryAdd(correlationId, 0);
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
            if (claimHash != null && claimHash.ContainsKey(ClaimFieldName))
            {
                // Another delivery already holds the claim and hasn't finished (or failed
                // without releasing it) yet.
                return false;
            }

            connection.SetRangeInHash(
                ClaimHashKeyPrefix + correlationId,
                new[] { new KeyValuePair<string, string>(ClaimFieldName, DateTimeOffset.UtcNow.ToString("O")) });
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
}
