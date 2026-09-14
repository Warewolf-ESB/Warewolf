# RabbitMQ Publish: connection caching for the Lightweight Execution Engine

**Ticket scope.** Make `PublishRabbitMQActivity` publish as fast as possible on the Azure Functions
**Consumption (free-grant)** plan by reusing the AMQP connection across executions, behind a
default-OFF switch owned by the Execution Engine so the on-prem **Server is provably unaffected**.

**Status.** **Implemented and verified** (2026-09-12). 196 automated tests green, negative controls
confirmed, and measured against a live broker at **5.5× faster** — see §11.

**Companion diagram:** [`RabbitMQ-ConnectionCache-Diagram.md`](./RabbitMQ-ConnectionCache-Diagram.md)

---

## 1. Decisions taken (confirmed by requester, 2026-09-12)

| # | Decision | Chosen |
|---|---|---|
| 1 | Config channel | **`WAREWOLF_RABBITMQ_CACHE`** — one JSON app setting, `WAREWOLF_*` prefixed, mirroring `SecurityFlags` / `DebugConfig` |
| 2 | Cache depth | **Connection only by default.** A fresh `IModel` per publish — no channel pool, no locking. See the amendment below: an opt-in channel pool was added after this plan was signed off, and ships **off**. |
| 3 | Topology declares | **Short-TTL memo** (default 30 s) skips the two passive declares while fresh |
| 4 | Connection TTL | **Sliding idle (5 min) + absolute max lifetime (30 min)** |
| 5 | Activity scope | **`PublishRabbitMQActivity` only.** `DsfPublishRabbitMQActivity` and `DsfConsumeRabbitMQActivity` untouched |
| 6 | Over-cap policy | **Cap + LRU evict, then fall back to uncached.** The cache never fails a publish |
| 7 | Database caching | **Out of scope** — see §8 for why it would be the wrong lever anyway |

### Amendment — channel pooling (2026-09-14)

Decision 2 above said "no channel pool". The implementation added one anyway
(`RabbitMqChannelPool`), because with the connection cached, `channel.open` + `channel.close` are
the only broker round-trips a publish still pays and they measure as ~99.6% of what remains
(323.3 ms of 324.7 ms against a remote broker). The benchmark is decisive:

| Configuration | Throughput | Per publish |
|---|---|---|
| `enabled` + `channelPool` | 19,262 msg/s | 0.03 ms |
| `enabled` only | 3.3 msg/s | 294.8 ms |
| neither | 0.7 msg/s | 1,340.7 ms |

**It nevertheless ships OFF** (`channelPool:false` in `Deploy-WwExecutionEngine.ps1`'s
`-RabbitMqCacheConfig` default), for two reasons:

1. Decision 2 was signed off by the requester. Reversing it in a deploy-script default rather than
   here is how an unreviewed decision reaches every engine — which is exactly what happened between
   2026-09-13 and the 2026-09-14 review.
2. The pool shipped with a cap that enforced nothing. `Rent()` hands out a *private* channel over the
   cap, and `Return()` pooled it anyway and decremented a counter it had never incremented, so
   `_live` drifted below zero, the cap check stopped firing, and the idle bag grew without bound —
   against a broker whose default `channel_max` is 2047 per connection. Fixed on 2026-09-14 (the pool
   now tracks the channels it issued) and covered by `RabbitMqChannelPoolTests`.

**To turn it on:** amend decision 2 with the requester, then set `channelPool:true` in the deploy
default. The code path is correct and tested; what is missing is the sign-off, not the engineering.

---

## 2. Why this is worth doing — measured in round-trips

Per published message, against a broker with no TLS:

| Phase | RTT today | Cached, memo cold | Cached, memo warm |
|---|---|---|---|
| TCP connect + AMQP handshake (`Start`/`Tune`/`Open`) | ~4 | 0 | 0 |
| `CreateModel` (channel.open) | 1 | 1 | 1 |
| `ExchangeDeclarePassive` | 1 | 1 | 0 |
| `QueueDeclarePassive` | 1 | 1 | 0 |
| `BasicPublish` (fire-and-forget) | 0 | 0 | 0 |
| channel.close | 1 | 1 | 1 |
| connection.close | 1 | 0 | 0 |
| **Total** | **~9** | **~4** | **~2** |

Plus, per execution today: one **thread** spin-up and tear-down (`Connection.StartMainLoop`) and two
heartbeat `Timer`s created and destroyed.

**The multiplier that makes this urgent.** `QueueName` and `Message` carry `[Inputs]`
([PublishRabbitMQActivity.cs:62](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L62),
[:70](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L70)), so
`DsfBaseActivity.TryExecute` calls `PerformExecution` **once per iterator row**
([DsfBaseActivity.cs:129-145](../../Dev2.Activities/Activities/DsfBaseActivity.cs#L129-L145)).
A recordset publish of 500 rows currently performs **500 full connect/handshake/disconnect cycles**,
sequentially. After this change: one connection, 500 channels, ~2 RTT each.
Confidence: **high — read directly in both files.**

---

## 3. Library facts established by inspection (not recollection)

All verified against `RabbitMQ.Client` **5.1.2** ([Dev2.Activities.csproj:636-638](../../Dev2.Activities/Dev2.Activities.csproj#L636-L638))
by reflecting over and decoding the IL of the shipped assembly. These drive every safety decision in §5.

| # | Fact | How established | Consequence |
|---|---|---|---|
| F1 | `AutomaticRecoveryEnabled` defaults **true**; `TopologyRecoveryEnabled` defaults **true** | property defaults on a fresh `ConnectionFactory` | A cached connection would be an `AutorecoveringConnection` unless we opt out |
| F2 | `AutorecoveringConnection` holds `m_models` (`List`), `m_recordedExchanges`, `m_recordedQueues` (`IDictionary`), `m_recordedBindings` (`ConcurrentDictionary`) | field reflection | Four candidate growth sites on a long-lived connection |
| F3 | `IModel.Dispose()` → `AutorecoveringModel.Abort()` → `ModelBase.Abort` **+ `UnregisterModel`** → `List.Remove` | IL decode of the explicit `System.IDisposable.Dispose` | **`m_models` does NOT leak** when we dispose each per-publish channel |
| F4 | `ExchangeDeclarePassive` / `QueueDeclarePassive` record **nothing**; `ExchangeDeclare`, `QueueDeclare`, `QueueBind` call `RecordExchange` / `RecordQueue` / `RecordBinding` | IL decode, call-token resolution | Recorded topology grows **per distinct queue name**, never shrinks (removal only on explicit delete/unbind) — a real unbounded leak for dynamically-named queues |
| F5 | `UseBackgroundThreadsForIO` defaults **false**, and `Connection.StartMainLoop` calls `Thread.set_IsBackground` with it | property default + IL decode | A cached connection would hold a **foreground** thread that can block Function host process exit |
| F6 | Plain `Connection` has **no** recorded-entity dictionaries — only `m_sessionManager`, `m_shutdownReport` | field reflection | Disabling recovery removes the F4 leak class entirely |
| F7 | `SessionManager.HandleSessionShutdown` → `IDictionary.Remove` + `IntAllocator.Free` | IL decode | Channel numbers and session-map entries are reclaimed on channel close |
| F8 | `SessionBase.OnSessionShutdown` calls `Connection.remove_ConnectionShutdown` | IL decode | Each channel **unsubscribes its own handler** — no event-handler accumulation across thousands of channels on one connection |
| F9 | Per connection: 1 mainloop `Thread`, `_heartbeatReadTimer` + `_heartbeatWriteTimer`, 1 `ManualResetEvent`, 1 `AutoResetEvent`, socket + frame buffers | field reflection | Budget ≈ **0.5–1 MB committed** per cached connection; cap 4 ⇒ ~2–4 MB of a 1.5 GB instance |

**F3 + F7 + F8 together are the result that makes decision #2 safe:** with a cached connection and a
fresh channel per publish, *every* per-channel resource is released on `Dispose`. The
"connection cached, channel per publish" shape has **no per-channel accumulation at all**.

---

## 4. Design

### 4.1 New contract — `Dev2.Runtime.Services/Interfaces/IRabbitMqConnectionCache.cs`

Placed beside `IOnDemandSourceLoader` in namespace `Dev2.Runtime.Interfaces`, deliberately mirroring
the `AmbientSourceLoader` precedent the requester pointed at. `Dev2.Activities` already references
`Dev2.Runtime.Services` ([Dev2.Activities.csproj:542-545](../../Dev2.Activities/Dev2.Activities.csproj#L542-L545))
and that project already pins the same `RabbitMQ.Client` 5.1.2.

```csharp
namespace Dev2.Runtime.Interfaces
{
    /// A lease on a (possibly) cached AMQP connection. Dispose returns it; it does NOT
    /// dispose a cached connection — the cache owns that.
    public interface IRabbitMqConnectionLease : IDisposable
    {
        IConnection Connection { get; }
        bool IsCached { get; }

        /// True when topologyKey was declared recently enough to skip the passive
        /// declares. Always false for an uncached lease.
        bool IsTopologyFresh(string topologyKey);
        void MarkTopologyDeclared(string topologyKey);

        /// Marks the underlying connection unusable so it is evicted, never handed out again.
        void Invalidate();
    }

    public interface IRabbitMqConnectionCache : IDisposable
    {
        /// Never throws for cache reasons and never returns null: on ANY cache failure it
        /// returns an uncached lease wrapping a freshly created connection, i.e. today's behaviour.
        IRabbitMqConnectionLease Acquire(string cacheKey, Func<IConnection> connectionFactory);
    }

    /// Ambient registry, identical in shape to AmbientSourceLoader. Null on the Server.
    public static class AmbientRabbitMqConnectionCache
    {
        public static IRabbitMqConnectionCache Current { get; }
        public static void Register(IRabbitMqConnectionCache cache);
        public static void Clear();
    }
}
```

**The cache never sees credentials.** The caller passes an opaque `cacheKey` (a salted SHA-256 of
host/port/user/vhost/password, hex) and a `Func<IConnection>` closure that builds the connection.
Only the first 8 hex chars are ever logged.

### 4.2 Cache entry and lifetime

```text
CacheEntry
  IConnection Connection
  long        CreatedTicks        // absolute max-lifetime clock
  long        LastUsedTicks       // sliding idle clock, bumped on release
  int         RefCount            // starts at 1 = the cache's own reference
  bool        Poisoned            // set by Invalidate(); never handed out again
  ConcurrentDictionary<string,long> DeclaredTopology   // topologyKey -> expiry ticks
```

**Reference counting is mandatory, and it is a correctness requirement, not an optimisation.**
`IConnection` is shared across concurrent executions, so eviction must never dispose a connection
another thread is mid-publish on — that is a use-after-dispose crash, strictly worse than a leak.
`Acquire` takes a ref with a compare-exchange loop that refuses to resurrect a zero-refcount entry;
eviction drops only the cache's own reference; the connection is disposed by whoever takes the count
to zero.

**Liveness gate.** An entry is handed out only when `Connection.IsOpen && !Poisoned && not idle-expired
&& not lifetime-expired`. Otherwise it is evicted and replaced.

**Sweep.** A single `Timer` per cache instance (one object, not one per entry), default 60 s period,
evicts expired entries so idle sockets are released at the TTL rather than lingering until the
Consumption instance is recycled — this is politeness to the broker, which allocates per-connection
buffers. `Acquire` also sweeps opportunistically so correctness never depends on the timer.

### 4.3 Connection settings applied to cached connections only

Applied by the Lightweight implementation when it builds a connection, driven by F1/F4/F5:

| Setting | Value | Why |
|---|---|---|
| `AutomaticRecoveryEnabled` | **false** | Yields a plain `Connection` (F6) with **no** recorded-topology dictionaries, killing the F4 unbounded-growth class outright. The cache does its own liveness-check-and-replace, which is both simpler and observable |
| `TopologyRecoveryEnabled` | **false** | Moot once recovery is off; set explicitly so the intent survives a future edit |
| `UseBackgroundThreadsForIO` | **true** | F5 — a cached connection must never hold a foreground thread that delays Function host shutdown |
| `RequestedHeartbeat` | 60 (default) | Keeps dead-peer detection so `IsOpen` is meaningful for the liveness gate |
| `ClientProvidedName` | `wwlw-pub/{instanceId}` | Makes cached connections identifiable in the RabbitMQ management UI, matching `RabbitMqDeadLetterPublisher`'s convention |

### 4.4 Activity change

`PerformExecution`'s body is extracted into one private `PublishCore(...)` used by **both** paths, so
the cached and uncached paths cannot drift:

```csharp
var cache = AmbientRabbitMqConnectionCache.Current;   // null on the Server => today's path
if (cache is null)
{
    using (Connection = ConnectionFactory.CreateConnection())   // unchanged
    {
        PublishCore(Connection, queueName, message, correlationId, memo: null);
    }
}
else
{
    using (var lease = cache.Acquire(BuildCacheKey(RabbitMQSource), () => ConnectionFactory.CreateConnection()))
    {
        Connection = lease.Connection;      // NOT disposed here — the cache owns it
        try   { PublishCore(lease.Connection, queueName, message, correlationId, lease); }
        catch { lease.Invalidate(); throw; }
    }
}
```

`PublishCore` keeps the existing channel logic verbatim — including the closed-channel retry at
[:205-219](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L205-L219)
and [:225-234](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L225-L234)
— with exactly one addition: the two passive declares are skipped when
`memo?.IsTopologyFresh(queueName) == true`, and `memo?.MarkTopologyDeclared(queueName)` is called
after a successful publish.

With `memo: null` the emitted broker calls are **byte-for-byte what they are today**, which is what
keeps the 11 existing tests in
[`PublishRabbitMQActivityTests.cs`](../../Dev2.Activities.Tests/ActivityTests/RabbitMQ/Publish/PublishRabbitMQActivityTests.cs)
green with no edits: `CreateConnection` Times.Once, `CreateModel` Times.Once / Exactly(2), the
passive-declare-then-recreate-channel assertions, and the disposal assertions.

### 4.5 Configuration — `WAREWOLF_RABBITMQ_CACHE`

New `Warewolf.Execution.Lightweight/Infrastructure/RabbitMqCacheOptions.cs`, modelled directly on
`SecurityFlags.FromEnvironment()` (same JSON-with-safe-fallback shape, same
`Dev2Logger.Warn`-and-continue on malformed input):

```json
{
  "enabled": true,
  "channelPool": false,
  "maxChannelsPerConnection": 32,
  "idleTimeout": "00:05:00",
  "maxLifetime": "00:30:00",
  "topologyTtl": "00:00:30",
  "maxConnections": 4,
  "sweepInterval": "00:01:00"
}
```

`channelPool` / `maxChannelsPerConnection` were added after this plan was signed off — see the
amendment to decision 2 above. `channelPool` defaults to `false` both in `RabbitMqCacheOptions` and
in the deploy script; `maxChannelsPerConnection` is a ceiling, not a target, and is inert while
pooling is off.

**Absent, empty, malformed, or `enabled:false` ⇒ `enabled = false`** — the cache is never registered,
`AmbientRabbitMqConnectionCache.Current` stays null, and every activity takes today's path. Safe by
default, exactly like `SecurityFlags`' all-false posture.

Read **once at startup** (unlike `SecurityFlags`, which is deliberately re-read per call): a cache's
TTLs and cap cannot be hot-swapped without rebuilding the cache, so a restart is the honest contract.
This is called out because it differs from the neighbouring precedent.

### 4.6 Registration and shutdown

In `StartupOrchestrator`, alongside the existing `Register*` steps: build the cache when enabled, call
`AmbientRabbitMqConnectionCache.Register(cache)`, and register `cache.Dispose()` on
`IHostApplicationLifetime.ApplicationStopping` (the orchestrator already receives `IHost` for
`RegisterResumptionExecutor`) so every socket and thread is released on a graceful stop.

---

## 5. Memory-leak analysis

Every row is either eliminated by construction or has a named mitigation. Rows marked **evidence** are
settled by §3.

| # | Vector | Verdict | Mitigation |
|---|---|---|---|
| L1 | `m_models` grows per channel on a cached connection | **Not a leak** (evidence F3) — and absent entirely once recovery is off (F6) | — |
| L2 | Recorded exchanges/queues/bindings grow per distinct queue name | **Real** (evidence F4) | `AutomaticRecoveryEnabled=false` ⇒ plain `Connection` has no such dictionaries (F6) |
| L3 | Foreground mainloop thread blocks process exit | **Real** (evidence F5) | `UseBackgroundThreadsForIO = true` + `Dispose` on `ApplicationStopping` |
| L4 | Session-map entries / channel numbers per channel | **Not a leak** (evidence F7) | — |
| L5 | `ConnectionShutdown` handlers accumulate per channel | **Not a leak** (evidence F8) | — |
| L6 | Unbounded cache entries across many sources | **Real** | `maxConnections` cap (default 4) + LRU eviction + TTL sweep |
| L7 | Topology memo grows per distinct queue name | **Real** | Per-entry expiry ticks, swept with the entry; memo capped and cleared wholesale on overflow |
| L8 | Connection disposed while another thread publishes | **Crash, worse than a leak** | Interlocked refcount; dispose only at zero; no resurrection of a zero-count entry |
| L9 | Evicted-but-referenced connection never disposed | **Real** | Eviction drops the cache ref; the last lease release disposes. Disposal is in a `finally` so a throwing publish cannot strand it |
| L10 | Static cache rooted for process life | **By design, bounded** | Single instance, capped contents, disposed on `ApplicationStopping` |
| L11 | Credentials retained in cache keys | **Security, not memory** | Key is a salted SHA-256 hex digest; connections are built by a caller-supplied closure; only 8 chars logged |
| L12 | A poisoned connection handed out repeatedly | **Real** | `Invalidate()` on any publish exception; `IsOpen` checked at acquire |

**Verification method.** A dedicated soak test asserts the invariants directly rather than
eyeballing them — see the test plan, item T14: 10,000 sequential publishes through a fake connection
must yield **exactly 1** `CreateConnection`, **10,000** `CreateModel`, **10,000** channel `Dispose`,
and a cache whose entry count, refcount, and memo size are all at their expected bounded values at the
end.

---

## 6. Behaviour change and its bound

The **only** externally visible change when the cache is enabled: for up to `topologyTtl` (default
30 s) after a successful publish, a queue or exchange **deleted out-of-band** is not re-detected, so
those publishes go to a now-missing exchange and — with `mandatory` unset and no publisher confirms —
are silently dropped.

- Bounded to 30 s, configurable, and set to `00:00:00` to disable the memo entirely while keeping
  connection reuse.
- Disabled outright when the cache is off, which is the default.
- **Pre-existing, not introduced:** the same silent drop already happens today whenever an exchange
  and queue both exist but are unbound, because `QueueBind` runs only when one of them was newly
  created ([:235-238](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L235-L238)).
  Publisher confirms would fix both; that was scoped out as item (c) and is **not** in this change.

---

## 7. Server (`Dev2.Server`) impact: none, by construction

`AmbientRabbitMqConnectionCache.Register` is called **only** from `StartupOrchestrator` in
`Warewolf.Execution.Lightweight`. The Server never calls it, so `Current` is null there and
`PerformExecution` takes the `using (Connection = ConnectionFactory.CreateConnection())` branch it
takes today. This is the same containment already relied on for `AmbientSourceLoader`, which is
likewise null on the Server by design.

Worth restating explicitly: the Server's shared-activity-instance concurrency defect
(`ResourceActivityCache` hands one activity graph to concurrent executions) is **not** addressed or
worsened here — the cache is keyed by broker coordinates, not by activity instance, and the mutable
`ConnectionFactory` field remains exactly as exposed as it is today on that host.

---

## 8. Database connection caching — why it is the wrong lever (scoped out)

Recorded here so the question is not reopened without this evidence.

`Microsoft.Data.SqlClient` **already pools connections**:
[`SqlConnectionWrapper.CreateConnectionString`](../../Dev2.Services.Sql/SqlConnectionWrapper.cs#L31-L37)
sets `Pooling = true, MaxPoolSize = 100`, and
[`SqlServer.cs:116-122`](../../Dev2.Services.Sql/SqlServer.cs#L116-L122) disposes the `SqlConnection`,
which **returns it to the pool rather than closing a socket**. A Warewolf-level `SqlConnection` cache
would duplicate the driver's pool, defeat its health/reset logic, and risk exhausting Azure SQL's
server-side connection limits.

If stored-procedure latency is the real target, the candidates worth measuring first are pool-key
fragmentation (any per-execution variance in the connection string splits the pool) and per-call
parameter/metadata discovery — neither of which is a connection-caching problem.
Confidence: **high — pooling settings and disposal read directly in both files.**

---

## 9. Files touched

| File | Change |
|---|---|
| `Dev2.Runtime.Services/Interfaces/IRabbitMqConnectionCache.cs` | **new** — contract + `AmbientRabbitMqConnectionCache` |
| `Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs` | extract `PublishCore`, add the cached branch, add `BuildCacheKey` |
| `Warewolf.Execution.Lightweight/Infrastructure/RabbitMqCacheOptions.cs` | **new** — `WAREWOLF_RABBITMQ_CACHE` parsing |
| `Warewolf.Execution.Lightweight/Infrastructure/LightweightRabbitMqConnectionCache.cs` | **new** — the implementation |
| `Warewolf.Execution.Lightweight/Infrastructure/StartupOrchestrator.cs` | register + dispose on `ApplicationStopping` |
| `Dev2.Activities.Tests/.../PublishRabbitMQActivityTests.cs` | **add** cases; existing 11 unmodified |
| `Warewolf.Execution.Lightweight.Tests/RabbitMqCacheOptionsTests.cs` | **new** |
| `Warewolf.Execution.Lightweight.Tests/LightweightRabbitMqConnectionCacheTests.cs` | **new** |

---

## 10. Change-synchronisation checklist (per CLAUDE.md)

| Artefact | Action |
|---|---|
| Docs | This plan + the diagram; a `WAREWOLF_RABBITMQ_CACHE` row in the deployment settings documentation |
| Deploy scripts | **None required** — absent var ⇒ disabled. Flagged for confirmation: should `Deploy-WwExecutionEngine.ps1` optionally set it? |
| `warewolf-architecture` skill | Add the ambient-cache pattern next to the existing `AmbientSourceLoader` note |
| Server | No change, by §7 |
| Consume / legacy publish activities | Deliberately unchanged, per decision #5 |

---

## 11. Verification results (2026-09-12)

### 11.1 Measured against a live broker

Three broker-gated tests in
[`LiveRabbitMqConnectionCacheTests.cs`](../../Warewolf.Execution.Lightweight.Integration.Tests/LiveRabbitMqConnectionCacheTests.cs),
run against a real CloudAMQP broker publishing to `Test_Connection_Queue_For_PublishActivity`:

| Verification | Result |
|---|---|
| 20 publishes, cached | **1 AMQP handshake**, 0 churn, 0 fallbacks; all 20 correlation ids consumed back off the queue |
| 20 publishes, uncached vs cached | **32,209 ms → 5,807 ms — 5.5×** (≈1,610 ms → ≈290 ms per message) |
| 200 channels over one cached connection | Connection stayed open throughout; all 200 messages delivered; channel-max never exhausted |

The 5.5× is the round-trip table in §2 showing up as wall-clock against a remote broker. It is a
**lower** bound for the recordset case, where the per-row handshake is the dominant cost.

### 11.2 Automated tests

| Suite | Tests | Result |
|---|---|---|
| `Dev2.Activities.Tests` — every RabbitMQ test (incl. the 11 pre-existing publish tests, **unmodified**) | 159 | green |
| `Warewolf.Execution.Lightweight.Tests` — cache + options | 37 | green |
| `Warewolf.Execution.Lightweight.Integration.Tests` — live broker | 3 | green (Inconclusive without `WAREWOLF_E2E_RABBITMQ`) |

### 11.3 Negative controls

Per the `warewolf-test` rule that *a regression test that has never failed proves nothing*, each
defect was reintroduced behind a temporary hook and the paired tests confirmed to fail.

| Defect reintroduced | Tests that failed |
|---|---|
| Per-publish channel never disposed | 4 — `NoCacheRegistered…`, `CacheRegistered…`, `PublishThrows…`, `ManyRows…` |
| Recovery left on + foreground I/O thread (L2, L3) | 1 — `CachedPath_AppliesTheLongLivedConnectionSettings` |
| Uncached lease stops owning its connection | 1 — `UncachedLease_DisposesTheConnectionAndAlwaysDeclares` |
| Eviction disposes without reference counting (L8) | 2 — `DisposeWhileLeaseOutstanding…`, `EvictedWhileLeaseOutstanding…` |
| Topology memo never expires | 1 — `TopologyMemo_IsFreshInsideItsTtlAndStaleAfter` |
| Capacity cap not enforced | 2 — `OverCapacity_EvictsTheLeastRecentlyUsedIdleEntry`, `AtCapacityWithEveryEntryInUse…` |

**Two findings came out of this exercise rather than being confirmed by it:**

1. **The 11 pre-existing tests all still passed with the channel-leak defect injected.** Channel
   disposal was never covered before; it is now.
2. **The concurrency stress test passed with reference counting removed entirely.** Its window for
   evicting the exact entry a worker holds is narrow and timing-dependent, so it was proving less than
   it appeared to. `Cache_EvictedWhileLeaseOutstanding_StaysUsableUntilReleased` was added to force
   that eviction deterministically; it fails under the defect with the correct diagnosis. The stress
   test is retained for what it does prove — no exceptions or double-disposal under real contention.

### 11.4 Running the live verification

Credentials are never committed. Supply the broker through the environment; with the variable absent
the tests report **Inconclusive**, never a false pass.

```powershell
$env:WAREWOLF_E2E_RABBITMQ = 'HostName=<host>;Port=5672;UserName=<user>;Password=<pwd>;VirtualHost=<vhost>'
cd Dev
dotnet test Warewolf.Execution.Lightweight.Integration.Tests --filter "TestCategory=LiveBroker"
Remove-Item Env:\WAREWOLF_E2E_RABBITMQ
```

The verification creates its own topology (direct exchange named after the queue, plus a binding on an
**empty routing key**) and purges the queue before each run, so it is safe to re-run and does not
depend on anything the E2E staging step provisions.

---

## 12. CI-parity pass (2026-09-13)

Run the way the pipeline runs it: `Compile.ps1 -ServerTests` → `Bin\ServerTests\TestRun.ps1`, with each
job's real `-Projects` / `-ExcludeProjects` / `-Filter` / `-RetryCount 6` arguments from
`Dev/.azure/pipeline.yml`.

| CI job | Tests | Passed | Failed |
|---|---|---|---|
| Activities Tests (`-UseRegionalSettings`) | 4,113 | **4,113** | 0 |
| Lightweight + JobProcessor + ServiceBusWorker + QueueProcessor | 1,565 | 1,564 | 1 (green on retry) |
| Unit Tests (broad, + Elasticsearch + RabbitMQ) | 5,540 | 5,519 | 17 |
| **Total** | **11,218** | **11,196** | — |

All 10 new activity tests and all 37 cache/options tests passed inside the real jobs, and the 11
pre-existing publish tests passed **unmodified**.

### 12.1 Two build-layout traps this pass exposed

**Neither is a product defect; both will bite the next person who tries this.**

1. **`Compile.ps1` is not a wrapper around `dotnet publish` — its post-publish steps are load-bearing.**
   `ServerTests.sln` builds consumers of *both* `RabbitMQ.Client` 5.1.2 and 7.1.2, which emit an
   identically-named DLL into one flat directory. `Compile.ps1` re-pins 5.1.2 and republishes
   `QueueProcessor.Tests` into an isolated subfolder
   ([Compile.ps1:435-495](../../../Compile.ps1#L435-L495)). Running `dotnet publish` on the solution
   directly leaves **7.1.2** in place, `RabbitMQ.Client.NetworkConnection` (5.1.2-only) fails to load,
   and MSTest then discovers **zero** tests in the *entire* Lightweight test assembly — every class,
   including ones with no RabbitMQ reference, because one bad type kills `GetTypes()`. Applying the
   two steps took discovery from **0 → 1,565 tests**.

2. **A full F: drive fails the publish with no compiler error at all.** The first `Compile.ps1` run
   died after writing 921 files, with no `error` line anywhere in its output — the failure only showed
   as a bare exit 1 and two missing test assemblies.

### 12.2 The 17 broad-job failures are environmental — evidence, not assertion

None mentions RabbitMQ, `Dev2.Activities`, or the cache.

| Failure(s) | Evidence it is the machine, not the code |
|---|---|
| `DateTimeResultTO_Constructor_Sets_TimeZone` | Asserts the zone is *"South Africa, Ireland or Azure Hosted Build Agents"*; got **India Standard Time** — a hardcoded expectation |
| `CreateTaskService_GivenValidArgs…` | Expected *"user name or password is incorrect"*, got *"your domain isn't available"* — host is off the corporate network |
| `ServerSchedulerFactory_CreateModel` | `UnauthorizedAccessException` — Task Scheduler needs elevation; this job ran non-elevated |
| 10 × `GetLogFileServiceHandlerTest` | `IOException: …test_warewolf_server.log… used by another process` — shared-temp race in `[TestInitialize]` |
| `SubscriptionConfig_ForBasePath…` | Same shared-temp file-lock pattern |
| `EmailSource_Send_ExpectedException` | `System.TimeoutException` — SMTP reachability |
| `ServiceTestExecutionContainer_…` | `Could not load file or assembly 'System.Activities.resources'` — satellite assembly absent from the flat publish |
| `UsageLogger_TrackUsage_Failed` | Usage tracking requires network egress |

`Warewolf.MergeParser.Tests` reports *"No test is available"*: its assembly loads cleanly (6 types) and
its csproj has no RabbitMQ reference — it simply contains no discoverable `[TestMethod]`.

**Caveat, stated plainly:** these were classified from their exception text and by confirming none
touches changed code. A pre-change baseline run was **not** performed, so "pre-existing" is a
well-evidenced inference rather than a measurement.

### 12.3 Machine changes made to achieve parity

Both are reversible and neither is required by the product:

| Change | Why | Revert |
|---|---|---|
| `Modify` on `C:\ProgramData\Warewolf\Server Settings\persistencesettings.json` for the current user | `ManualResumption*` / `SuspendExecution*` write it; CI agents run elevated | `icacls "<path>" /remove:g "<user>"` |
| Regional settings set to CI's values — notably `sDecimal` `,` → `.` | `DateTime.ParseExact` of `…53.7555` fails when the decimal separator is a comma; this, not the locale, was the real cause of the date failures | restore from `regional-backup.json` |
