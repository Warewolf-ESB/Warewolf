# 8520 — QueueProcessor Throughput Gap Closure Plan

**Goal:** bring the RabbitMQ QueueProcessor path (`wwqp5-runb-ordersuccessqueu-3879` → `wwengine-e2e-ldi413`) to parity with the Azure Service Bus shovel path (`wwexecution-secure-trigger-queue-e2e` → `WarewolfServer-UAT`), which drains 1000 messages in ≤ 2.5 minutes.

**Evidence provenance.** Every number below was measured, not estimated:

- QueueProcessor per-message latency — Log Analytics `ContainerAppConsoleLogs_CL`, workspace `405a4834-1554-4f2a-b5eb-31789fffaa9b`, parsing the `durationMs=` emitted by `AuditingConsumerDecorator.cs`.
- Engine-side latency / errors / memory / instance counts — `Microsoft.Web/sites` platform metrics (`HttpResponseTime`, `AverageResponseTime`, `Http5xx`, `MemoryWorkingSet`, `Requests`, `FunctionExecutionCount`, `Threads`).
- Instance counts — sample **count** on the per-instance `Threads` metric at `PT1M` (one sample per reporting instance per minute).
- Configuration — live `az containerapp show`, `az functionapp config appsettings list`, `az webapp auth show`, `az servicebus queue show`.

---

## 1. The gap, measured

Two 1000-message QueueProcessor runs on 2026-09-01, same app, same queue (`order-success-queue`), same workflow (`rabbit/RabbitProcess`):

| Run | In-flight cap | Wall clock | p50 | p95 | Calls > 20 s |
|---|---|---|---|---|---|
| 05:38:22 → 05:42:39 | 6 | **4 m 17 s** | 131 ms | 1.1 s | 34 (3.4 %) |
| 09:15:00 → 09:37:35 | 4 | **22 m 35 s** | 155 ms | 30.2 s | **166 (16.6 %)** |

Latency distribution for the 09:15 run is bimodal, and **every message succeeded (2xx)** — nothing failed, a sixth of the calls simply froze:

| Band | Count |
|---|---|
| < 0.5 s | 796 |
| 0.5 – 2 s | 15 |
| 2 – 5 s | 5 |
| 5 – 10 s | 15 |
| 10 – 25 s | 3 |
| **25 – 35 s** | **166** |

Service Bus path for comparison (`WarewolfServer-UAT`, 2026-09-02 08:09–08:12): `FunctionExecutionCount` 139 → 883 → **1869 in one minute (≈ 31 executions/s)**, run complete in ~3 minutes. The QueueProcessor's engine served **6–56 requests per minute (≈ 0.1–0.9 req/s)** across the whole 22-minute run — a ~34× demonstrated throughput gap.

---

## 2. Root cause

### 2.1 A hard 30-second server-side stall on ~16 % of calls

`wwengine-e2e-ldi413` platform metrics across the 09:15 window:

- `HttpResponseTime` **maximum pinned at 30.1 – 30.4 s in every 5-minute bin**
- `AverageResponseTime` 4.3 – 30.1 s per minute
- `Http5xx` = **0** throughout
- `MemoryWorkingSet` ≈ 265 MB of 1536 MB available

So the 30 s is spent **inside App Service**, not in the container, not in CloudAMQP, and not in the workflow (p50 = 131 ms). It is not memory pressure and it is not an error path. The slow calls also release in lockstep — 4 requests start together, all complete at T+30.1 s, the next 4 immediately stall — which is a shared server-side gate opening on a timer, not per-request work.

### 2.2 The stall tail is the entire deficit

| Run | Stall cost | Share of wall clock |
|---|---|---|
| 09:15 (C=4) | 166 × 30 s = 4 980 slot-s ÷ 4 = 1 245 s = 20.8 min | **92 % of 22.6 min** |
| 05:38 (C=6) | 34 × 30 s = 1 020 slot-s ÷ 6 = 170 s | **66 % of 4.3 min** |

At p50 = 131 ms, six in-flight slots should drain 1000 messages in ~22 s. Everything above that is the stall tail.

### 2.3 ROOT CAUSE (CONFIRMED) — a synchronous usage-API call on every execution

**This section replaces two earlier, wrong diagnoses.** Both were plausible inferences that measurement then killed, and both are recorded here because the elimination is what made the real cause findable:

| Discarded hypothesis | How it died |
|---|---|
| Host self-restart from the primary-host lease / health monitor | 23 downloaded instance logs: `Restarting host` **0 times**, `Host is unhealthy` **0 times** |
| Log-write back-pressure against the Azure Files content share | Cut ~65 % of log volume (W1.a/W1.a2) and the stall got **worse**, 16.6 % → 25.3 %. The content share was also measured **idle** during a run: 1 transaction/min at 5–7 ms |

**The actual cause, from App Insights `dependencies` (which only became available because W2.1 set `APPLICATIONINSIGHTS_CONNECTION_STRING`):**

Every workflow execution makes exactly **one** synchronous `POST /api/LogUsage` to
`warewolfusageapi.azurewebsites.net` — an **externally hosted service, not in this subscription**, so it cannot be scaled or warmed. Measured over 1427 invocations, 1427 calls, one per invocation:

| resultCode | calls | p50 | p95 | > 20 s |
|---|---|---|---|---|
| `200` | 1119 | **63 ms** | 950 ms | 12 |
| **`400`** | **308** | **30 048 ms** | 30 147 ms | **308 (all of them)** |

**Every HTTP 400 takes ~30 s to come back.** 308/1427 = **21.6 %**, matching the observed 25.3 % stall rate. The 400s are load-dependent — **zero** during the low-rate pre-warm, then a steady 13–24/min once concurrency rose — which reads as throttling by the external service, expressed as a 400 after a 30-second delay.

Corroboration: the `InProc | Invoke` dependency (the invocation itself) has p95 **30 199 ms**, just 133 ms more than the usage call's p95. The invocation is as slow as its slowest dependency and nothing else.

**The call path, all in source:**

```
UsagePublishMiddleware.Invoke                       ← Functions worker middleware
    await next(context)                             ← the workflow runs here (64 ms)
    finally {
        _usageEventEmitter.TrackWorkflowExecution() ← void, SYNCHRONOUS
          → UsageEventEmitter.TrackWorkflowExecution
            → IUsageTrackerSink.TrackEvent
              → UsageTracker.TrackEvent             ← blocking HTTP POST
    }
```

**This finally resolves the trace in §2.3's earlier draft** — `Execute completed` at 64 ms, then 30 s of silence, then `Executed 'Functions…' Duration=30098ms`. The workflow *had* finished; the middleware's `finally` was blocked on the usage POST, so the host could not record the invocation as complete. "The hold is after the function body returns, in the worker→host completion path" was correct; it just had no name.

**It is a contract violation, not just a slow call.** `UsagePublishMiddleware`'s own XML doc states `IUsageEventEmitter` implementations are *"documented as non-throwing/**non-blocking**"*, and `UsageEventEmitter`'s stated *"Workflow execution latency is not affected"*. `DefaultUsageTrackerSink.TrackEvent` called `UsageTracker.TrackEvent` inline. Both comments asserted a contract the code did not keep.

**Why `WAREWOLF_LICENSE_CHECK_ENABLED=false` is NOT the fix.** The licence gate (`WorkflowExecutor.cs:170`) calls `SubscriptionProvider.GetSubscriptionData()`, which builds an object from a cached singleton and makes **no HTTP call**. Setting that flag would have been a no-op — a fourth wrong mechanism, avoided only by reading the code before changing the setting.

### 2.4 Instance scaling is NOT the constraint — offered concurrency is

*This section supersedes an earlier draft that claimed the engine "never scaled out". That claim was wrong. The measurement below is what actually happened.*

Two counts, both from the `Threads` metric at `PT1M`: **warm** = number of instances reporting; **executing** = number of instances that ran at least one function in that minute (sample count on `FunctionExecutionCount`). The method is validated — both read `0` for `wwengine-e2e-ldi413` over an idle window (2026-09-01 02:00–03:00 UTC, `Requests` = 0).

| | Warm | Executing | Executions/min | Errors |
|---|---|---|---|---|
| `WarewolfServer-UAT`, burst peak | **72** (0 → 11 → 33 → 63 → 72 in 3 min) | **58** | **1869** | — |
| `wwengine-e2e-ldi413`, whole 22-min run | **~24, steady** | **8 – 12** | 20 – 56 | `Http2xx` 1058, `Http401` 0, `Http4xx` 0, `Http5xx` 0 |

**The HTTP engine had idle capacity for the entire run.** 24 instances warm, 8–12 of them used, 4–6 concurrent requests in flight, every request 2xx on the first attempt. There was no scale-out to be had because none was needed, and nothing in the configuration was blocking one (`functionAppScaleLimit` = 200 on both apps).

So the difference is not the engine's willingness to scale. It is **how much concurrency each path offers the engine**, and that follows from **which side of the boundary holds the backlog**:

| | Service Bus path | QueueProcessor path |
|---|---|---|
| Where the 1000 messages sit | the engine's **own** trigger queue | **CloudAMQP**, outside Azure's view |
| Who decides concurrency | the **engine**, from queue depth | the **worker**, from `replicas × MaxConcurrency` |
| Scaling formula | `ceil(messages ÷ maxConcurrentCalls)` = `ceil(1000 ÷ 16)` = **63** | KEDA `ceil(backlog ÷ value)` = `ceil(1000 ÷ 6)` = **167** |
| What actually happened | 72 warm / 58 executing — **the formula ran** | **clamped to `maxReplicas: 1`** |
| Offered concurrency | 58 × 16 = **~928** | 1 × 6 = **6** |

`maxConcurrentCalls` is 16 because that is the `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` 5.24.0 default and `host.json` does not override it. (`WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS=2` on UAT is read by **no code in `Dev/`** — see W5.4 — so it does not apply.) `ceil(1000/16) = 63` against a measured 72 warm / 58 executing is target-based scaling doing exactly what it documents.

**The single sharpest statement of the gap:** both paths compute the right number of workers from the backlog. The Service Bus path is allowed to act on 63. The QueueProcessor path computes 167 and is then **clamped to 1** by `maxReplicas`. The offered-concurrency ratio is **~155×**.

### 2.5 The engine also does strictly less work per message on the Service Bus path

`WarewolfServer-UAT` has Easy Auth **disabled entirely** and `BYPASS_SECURE_CONFIG=true` (no `IWorkflowPolicyMatcher` evaluation). `wwengine-e2e-ldi413` has Easy Auth **enabled** with `requireAuthentication=true` and `tokenStore.enabled=true`, plus full `secure.config` policy evaluation. The 2.5-minute figure is not measured against a like-for-like engine.

---

## 3. Challenge: does in-replica concurrency replace replicas?

**The premise as stated:** *"1 replica with concurrent requests set to 6 or 10 or any higher number the engine can manage replaces the need for multiple replicas — the queue worker just sends requests and waits, so multiple async threads serve the same purpose as multiple replicas."*

### 3.1 Where the premise is right

**On the mechanics of the QueueProcessor, it is correct, and the code backs it.** The forwarder is pure I/O wait — `EngineForwarder.Consume` → `EngineWorkflowClient.PostSecureAsync` → `await _httpClient.SendAsync`. The pump is genuinely async end-to-end:

- `RabbitMqMessagePump` gates in-flight work with `SemaphoreSlim _throttle` at `_maxConcurrency` permits, and `OnReceivedAsync` awaits without blocking a thread.
- `ConnectionFactory.ConsumerDispatchConcurrency = _maxConcurrency`, so RabbitMQ.Client 7.x actually dispatches deliveries in parallel rather than serially (the 5.1.2 loop did not).
- The class comment on `MaxConcurrency` calls the default of 1 "measured on-prem behaviour", not a technical limit.

So from the engine's point of view, *N async in-flight requests from one replica are indistinguishable from N replicas each holding one request.* Six replicas at concurrency 1 and one replica at concurrency 6 offer the engine exactly the same load. **For throughput, in-replica concurrency is the right primary lever and horizontal replicas are not needed.**

This also explains, in one sentence, why all three of the observed configurations disappointed equally: **1×6, 6×1 and 10×1 all present the engine with 6–10 concurrent requests.** They are the same experiment run three times.

### 3.2 Where the premise breaks

**(a) The number, not the packaging.** The premise says 6 or 10 concurrent requests from one replica "shifts the same load as Service Bus". It does not. Service Bus offered the engine **~928** concurrent (58 executing instances × 16 `maxConcurrentCalls`); the QueueProcessor offered **6**. That is **~155×**, and it is not a packaging difference — it is a magnitude difference. The premise is right that 6 in one replica equals 6 across six replicas. It is wrong that 6 is what the other path offers.

To genuinely match Service Bus with a single replica you would need `MaxConcurrency ≈ 928` in one 0.25-vCPU container on one AMQP channel. That is where (c), (e) and (f) below stop being theoretical.

**(b) Concurrency alone cannot reach the target at today's stall rate.** See §4 — you would need `C ≈ 34` while 16.6 % of calls cost 30 s. That is achievable on the *worker* side, but it means running the engine at 34 concurrent purely to absorb a defect, which is the wrong reason to buy capacity.

**(b′) The per-instance OOM ceiling is real but weaker than previously stated — and must be re-measured.** `Deploy-WwQueueProcessor.ps1:178-181` records, from 2026-08-12: *"concurrency 6 and 8 both scored 24/24, while 10 produced 'Insufficient memory to continue the execution of the program'."* An earlier draft of this plan treated that as a hard cap of 8 on the grounds that the engine would pile all concurrency onto one instance. **§2.4 disproves that reasoning:** the engine spread 4–6 concurrent requests across 8–12 instances out of 24 warm, so per-instance load was well under one concurrent execution. The Aug-12 ceiling was very likely an artefact of a colder or smaller engine, not a property of the current one — 10 concurrent spread over 8+ instances should not exhaust 1.5 GB each. **Re-measuring it is now a gate (W4.0), not a background question, and the plan no longer caps the `replicas × concurrency` product at 8.**

**(c) Concurrency does not scale the replica's own resources.** The replica runs at **0.25 vCPU / 0.5 GiB**. Ten replicas × 1 gives 2.5 vCPU aggregate; one replica × 10 gives 0.25 vCPU. For I/O-bound work this is mostly irrelevant — but not entirely: per-message TLS, the `WrapForEngine` JSON re-parse, `SHA256.HashData` when no `CorrelationId` is set, and `AuditingConsumerDecorator` logging the **full message body** at INFO into the ACA log shipper all cost CPU. ACA enforces CPU via CFS quota, and quota throttling produces exactly the latency inflation that gets misread as server slowness. The premise holds **only if CPU/memory are raised alongside concurrency**.

**(d) Blast radius and rolling updates.** One replica is one process, one AMQP connection, one channel. If it is CPU-throttled, OOMs, or loses its broker connection, throughput goes to zero rather than to (N-1)/N. Every ACA revision rollout and every scale-in SIGTERMs it. This is an availability argument, not a throughput one, but it is real for a production queue worker.

**(e) Concentrated duplicate-execution risk on drain.** `DrainAsync` waits for in-flight work up to `ShutdownGraceSeconds` (210 s) and deliberately leaves anything still running unacked — the pump logs that those workflows "may run twice". With `Prefetch == MaxConcurrency == 10` in a single replica, one SIGTERM puts up to 10 messages at risk of re-execution; at concurrency 1 it is one per replica. Higher in-replica concurrency **concentrates** that exposure rather than spreading it.

**(f) A single channel serialises acks.** All `BasicAckAsync` / `BasicNackAsync` calls go through one `IChannel`, whose outgoing RPCs are internally serialised. At 6–10 concurrency and ~130 ms latency (~75 acks/s) this is nowhere near a limit — but it is a real ceiling somewhere above a few hundred concurrent short workflows, so the premise does not extend indefinitely.

### 3.3 Verdict

**The mechanism is right; the number was three orders of magnitude short.** In-replica concurrency genuinely does substitute for replicas — that part of the premise is sound, and the plan adopts it. Replicas are not needed for throughput. What the premise misses is that `1 × 6` was never the Service Bus-equivalent load; `~928` was. The lever was correct and barely pulled.

**Adopted target shape: raise `replicas × MaxConcurrency` aggressively, weighted toward concurrency, in a measured ramp.** Concretely: **2 replicas**, with `MaxConcurrency` ramped `6 → 12 → 24 → 48` under OOM and latency watch (W4.0/W4.2). Two replicas rather than one is bought for (d) and (e) — fault isolation and halving the drain-duplicate window — at zero throughput cost. Beyond ~48 total, (f) single-channel ack serialisation and (c) container CPU become the next things to measure, and the honest alternative is more replicas rather than more threads in one.

The `replicas × MaxConcurrency` **product** is the number that matters, and `maxReplicas: 1` is what currently pins it to 6.

---

## 4. Throughput model — why the engine must be fixed first

`wall_clock ≈ [ (1−s)·N·t_fast + s·N·t_stall ] / C`, with `N = 1000`, `t_fast = 0.15 s`, `t_stall = 30 s`, `s` = stall rate, `C` = total concurrency.

The model validates against the observed run: at `s = 0.166, C = 4` it predicts 21.3 min against 22.6 min measured.

| Stall rate `s` | C=4 | C=6 | C=8 | C=10 | C=34 |
|---|---|---|---|---|---|
| **0.166** (09:15 run) | 21.3 min | 14.2 min | 10.6 min | 8.5 min | **2.5 min** |
| **0.034** (05:38 run) | 4.9 min | 3.2 min | 2.4 min | 2.0 min | 0.6 min |
| **0** (stalls eliminated) | 38 s | 25 s | 19 s | 15 s | 4 s |

For reference, the Service Bus path ran at an offered concurrency of **~928** (§2.4) — off the right-hand edge of this table entirely.

Read the two rows that matter:

- **Concurrency alone can get there, but for the wrong reason.** Hitting 2.5 min at today's 16.6 % stall rate needs **C ≈ 34**. That is reachable on the worker side once `maxReplicas` stops clamping the product — but it means provisioning 34 concurrent slots to absorb a defect rather than to do work, and it leaves the 30 s tail in place for every other consumer of that engine.
- **Fixing the engine gets there with the concurrency already deployed.** At `s = 0`, the *current* C=6 drains 1000 messages in ~25 s — an order of magnitude inside the 2.5-minute target.

**Therefore: W1 (engine host hygiene) and W2 (observability) still gate everything else** — not because concurrency cannot help, but because 30 s × 16.6 % is the term that dominates every column, and removing it is cheaper than out-provisioning it. Raise concurrency afterwards to buy headroom, not to paper over the stall.

---

## 5. Workstreams

### 5.0 Applicability — what can be applied to the live deployment as-is

Three categories. Most of the plan is category **A**; two items look like A and are not.

**A — Pure Azure control-plane. Apply now, no rebuild, no code.**

| Item | How |
|---|---|
| W1.a engine log levels | `AzureFunctionsJobHost__logging__logLevel__<Category>` app settings **override `host.json`** — the mechanism §8 already mandates and UAT already uses for `concurrency`/`healthMonitor`. No `host.json` edit, no redeploy. |
| W1.a `EXECUTIONLOGLEVEL=WARN` | engine app setting (read by `LoggingConfiguration.FromEnvironment()`) |
| W1.b `fileLoggingMode` | `AzureFunctionsJobHost__logging__fileLoggingMode=debugOnly` |
| W1.1–W1.3 concurrency / health monitor | `AzureFunctionsJobHost__concurrency__*`, `…__healthMonitor__enabled` |
| W1.5 delete `AzureWebJobsDashboard` | app-setting deletion |
| W1.6 Easy Auth token store off | `az webapp auth update` / authsettingsV2 patch |
| W1.d `WAREWOLF_WORKFLOW_POOL_MAX` | app setting (`ResolvePoolCap()` reads it via `Environment.GetEnvironmentVariable`; static, so read once per process — the setting change restarts the app anyway) |
| W2.1 `APPLICATIONINSIGHTS_CONNECTION_STRING` | app setting |
| W4.1 `maxReplicas` 1 → 2 | `az containerapp update --max-replicas 2` |
| W4.4 cpu / memory | `az containerapp update` |
| W4.5 KEDA `value` | `az containerapp update --scale-rule-metadata` |
| W4.6 `pollingInterval` | `properties.template.scale.pollingInterval` — reachable via `az containerapp update --yaml` or `az resource update`, but **no CLI flag and no script parameter** |
| W4.7 container `EXECUTIONLOGLEVEL` | container env var |

**Verification obligation for the `AzureFunctionsJobHost__*` rows:** `StartAsAzureFunction.ps1:715-721` records that these overrides *did not take effect* under local `func.exe`, which is why that script patches `host.json` directly for CI. On a real Function App the app-setting override is the documented and supported mechanism, and UAT relies on it — but after applying, **confirm from the startup log that the effective levels changed** before concluding W1.a had no effect.

**B — No source change, but requires a redeploy.**

| Item | Why, and the trap |
|---|---|
| **W4.3 trigger `Prefetch`** | The trigger `.bite` is **baked into the container image** (`Dockerfile:51` — "Config is BAKED IN, not mounted or fetched"), and there is **no environment override for `Prefetch`** — it comes only from `TriggerDefinition.Prefetch` → `ResolvedPrefetch`. Changing it means editing the `.bite` and building a new image. **See the blocker below — this is not optional.** |
| **W1.4 `WEBSITE_RUN_FROM_PACKAGE=1`** | **Not a flag flip.** This engine's content is an *extracted* `wwwroot` (host logs reference `C:\home\site\wwwroot\Resources\…`; Kudu traces show `api/zipdeploy`; there is no `SitePackages`). Setting the flag on an app with an extracted `wwwroot` breaks it — the platform then looks for `d:\home\data\SitePackages\packagename.txt`. Correct order: set the setting, **then** redeploy the package. |
| **§9's shipped fix** | The `QueueProcessorOptionsBinder` change is in source and tested, but the **running container is still the old image**. `WORKER__MAXDELIVERYATTEMPTS` / `WORKER__RETRYENGINEINTERNALERRORS` stay ignored on the live app until a new image is built and a new revision deployed. |

**C — Requires a source change.**

| Item | Scope |
|---|---|
| W1.c duplicate log sink | Engine logging registration. **May prove unnecessary** — if W1.a/W1.b clear Gate A, this is an optimisation (73 % of a much smaller volume), not a fix. Decide after Gate A. |
| **A `Prefetch` override (new — recommended)** | See the blocker below. Small and testable; removes the only code dependency from the entire W4 ramp. |
| W5.4 `WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS` | Code only if *implementing*; deleting the dead setting is category A. |
| §8 docs / `Deploy-WwExecutionEngine.ps1` / load-test guides | Durable follow-up so a fresh engine is not deployed with the same gap. Not needed to run the experiment. |

### 5.0.1 Blocker: raising `MaxConcurrency` alone does nothing

`RabbitMqMessagePump.cs:129` calls `BasicQosAsync(prefetchSize: 0, prefetchCount: _config.Prefetch, global: false)`. That is a **per-consumer broker-side limit on unacknowledged messages**, taken from the baked-in `.bite` value of **6**.

So with `WORKER__MAXCONCURRENCY=24`, the `SemaphoreSlim` would grant 24 permits but **the broker never delivers more than 6 unacked deliveries** — in-flight stays at 6. **W4.2 is inert above 6 without W4.3.** The whole W4 ramp (6 → 12 → 24 → 48) therefore needs a new container image *per step* as things stand, which makes it slow and unattractive.

**Recommendation — one small code change converts the whole ramp to pure config:** treat the trigger's `Prefetch` as the default and let an explicit `WORKER__PREFETCH` override it, or (simpler, and matching the optimum this plan already argues for) default the effective prefetch to `MaxConcurrency` when the trigger does not set one explicitly. Either keeps the existing `Prefetch > MaxConcurrency` warning meaningful.

**Not implemented — no code or tests written, awaiting go-ahead per `CLAUDE.md`.** Proposed tests: (1) `WORKER__PREFETCH` overrides the `.bite` value; (2) absent the override, the `.bite` value still wins, so no existing deployment changes behaviour; (3) the pump calls `BasicQosAsync` with the resolved value — assertable against the mocked `IChannel` the pump already exposes for this; (4) the `Prefetch > MaxConcurrency` warning still fires on the resolved value.

**If you would rather not touch code:** do W1 (all category A) and W4.1/W4.4/W4.5/W4.6 now, leave concurrency at 6, and see whether Gate A alone reaches the target — per §4, at `s = 0` the *current* C=6 drains 1000 messages in ~25 s. On that arithmetic the ramp may never be needed, which is the strongest argument for doing W1 first and re-measuring before writing any code.

### W1/W2/W4 — APPLIED 2026-09-03

Applied to the live resources with `Scripts/Apply-Ww8520GateAConfig.ps1 -Execute -SkipFileLoggingMode`. Verified by reading the resources back:

| | Applied |
|---|---|
| Engine app settings | `EXECUTIONLOGLEVEL=WARN`, `logLevel__default=Warning`, `logLevel__Host.Results=Information`, all three `concurrency`/`healthMonitor` flags `false`, `WAREWOLF_WORKFLOW_POOL_MAX=8`, `APPLICATIONINSIGHTS_CONNECTION_STRING` set, `AzureWebJobsDashboard` deleted |
| Easy Auth | `login.tokenStore.enabled=False`; **`requireAuthentication=True` and `aad.enabled=True` confirmed unchanged — auth not weakened** |
| Container App | `maxReplicas` 1→2, `cpu/memory` 0.25/0.5Gi→0.5/1Gi, `pollingInterval` 30→10s. `WORKER__MAXCONCURRENCY` left at 6 (inert without W4.3 — §5.0.1) |
| **W1.b deferred** | `fileLoggingMode` left at `always` on purpose, so the host log survives as the fallback diagnostic if Gate A fails. W1.a + `default=Warning` already remove ~75 % of the volume |

**Authoritative rollback:** `%TEMP%\ww8520-config-20260903-003700\Rollback-Ww8520GateAConfig.ps1`. The re-run's snapshot (`…-rerun-003916`) is **not** valid — it was taken after the first run had applied phases 2–3, so it captured `EXECUTIONLOGLEVEL=WARN` and a already-deleted `AzureWebJobsDashboard`. That file has been renamed `.NOT-AUTHORITATIVE` and the script now detects and refuses this case.

**Next:** run 1000 messages and evaluate Gate A below. Nothing else in W1 should be changed until that measurement exists.

### W1 — Engine host hygiene (dominant, do first)

Bring `wwengine-e2e-ldi413` in line with `WarewolfServer-UAT`. All app-settings changes, no code:

**Reordered after §2.3.** The logging changes now lead, because they address the measured dominant cost (the 30 s hold on a warm instance, ~10 % of executions) rather than the host-restart theory that the logs disproved. W1.1–W1.3 are retained but demoted: they are cheap parity with UAT and remove a known restart risk, not the fix.

**Where the 49 MB actually comes from.** Measured by parsing the 23 downloaded host logs — 144 474 lines, 49.0 MB:

| By level | | | By emitter | |
|---|---|---|---|---|
| **Information** | **39.1 MB (80 %)** | | `Time taken to process proc …` | 8.5 MB (17 %) |
| Warning | 1.3 MB (3 %) | | host's own lines | 8.3 MB (17 %) |
| Debug | 0.2 MB (2 267 lines) | | `SourceLoader` | 7.6 MB (15 %) |
| Error | 0.03 MB (69 lines) | | `DB proc result` | 5.2 MB (11 %) |
| **Trace** | **0.02 MB (44 lines)** | | `WorkflowExecutor` | 3.1 MB (6 %) |
| | | | `InstanceCorrelationMiddleware` | 2.4 MB (5 %) |

**This corrects an earlier draft of W1.a**, which proposed dropping the `Trace` categories. `Trace` is **44 lines in total** — that change would have saved nothing measurable and its failure would have been misread as "the logging theory is wrong". The volume is Warewolf's own **`Information`** output, so `EXECUTIONLOGLEVEL` is the lever, not the `host.json` category levels.

| # | Change | Rationale |
|---|---|---|
| **W1.a** | **`EXECUTIONLOGLEVEL=WARN`** on the engine | The single effective lever: removes `Time taken…`, `DB proc result`, `SourceLoader`, `WorkflowExecutor` and `InstanceCorrelationMiddleware` — **~27 MB of the 49 MB (55 %)** in one setting. |
| **W1.a2** | `AzureFunctionsJobHost__logging__logLevel__default=Warning`, **but `…__logLevel__Host.Results=Information`** | Removes the host's own Information chatter (8.3 MB + 2.3 MB `Request`) while deliberately **keeping the one line worth its bytes** — `Executed 'Functions.ExecuteSecureWorkflow' … Duration=Nms`, ~450 KB per run and the only per-invocation timing the engine emits. |
| ~~Trace → Warning on `Microsoft.Azure.WebJobs` etc.~~ | **Dropped** | 44 lines. Measured, not assumed. |
| **W1.b** | **`AzureFunctionsJobHost__logging__fileLoggingMode=debugOnly`** | `always` keeps synchronous content-share writes on the hot path in production; `debugOnly` is the Functions default. **Apply as a separate, later step** — see the Gate A note on losing the host log. |
| **W1.c** | Find and remove the duplicate sink | **73 %** of lines are the same text emitted twice — two providers registered for the same categories. **Code change (category C), and probably unnecessary:** after W1.a/W1.a2 it is 73 % of a much smaller volume. Decide after Gate A. |
| **W1.4** | `WEBSITE_RUN_FROM_PACKAGE=1` | Unset — so `wwwroot` sits on the same writable Azure Files share the logs are being written to. UAT has it. Now believed load-bearing, not just cold-start hygiene. |
| W1.5 | Delete `AzureWebJobsDashboard` | Deprecated; writes a row per invocation to table storage. UAT does not have it. |
| W1.6 | Easy Auth `login.tokenStore.enabled = false` | Bearer-token API traffic never reads the token store. **Keep `requireAuthentication=true`** — do not weaken auth. *(Note: measured 0 × `Http401`, so Easy Auth is not the stall; this is hygiene only.)* |
| W1.1–W1.3 | `AzureFunctionsJobHost__concurrency__dynamicConcurrencyEnabled=false`, `…__snapshotPersistenceEnabled=false`, `…__healthMonitor__enabled=false` | **Demoted.** `StartAsAzureFunction.ps1:695-735` documents these as a self-restart cause and UAT sets them, but 23 instance logs show `Restarting host` **0 times** — so this is parity and risk reduction, not the fix. |
| **W1.d** | Consider `WAREWOLF_WORKFLOW_POOL_MAX` (defaults to 8, unset on both engines) | Only relevant to cost (a), the first-execution compile. Raising it does not help a cold instance — the first execution must still compile — so treat as a follow-up, not part of Gate A. |

**Exit gate (Gate A).** Re-run 1000 messages unchanged at C=6 and confirm:

1. Calls > 20 s drop below 2 % — from the **QueueProcessor's own** `durationMs=` in Log Analytics.
2. `HttpResponseTime` **maximum** is no longer pinned at ~30 s — from **platform metrics**.

**Both measurements are deliberately independent of engine file logging**, because W1.a/W1.b remove the very lines an engine-side check would read. This is a real circularity in the experiment and it is resolved by measuring from outside: the client's `durationMs=` and the platform's `HttpResponseTime` both saw the 30 s stall originally (§1, §2.1), so both can confirm its absence.

**If Gate A fails**, then and only then turn the engine logging back up (`EXECUTIONLOGLEVEL=INFO`, `fileLoggingMode=always`), re-run, and use the host log to check whether the gap between `WorkflowExecutor Execute completed` and the matching `Executed 'Functions…'` line is still ~30 s. That distinguishes "the hold is not log-write back-pressure" from "we did not reduce the volume enough".

Expected wall clock at C=6 with `s < 0.02`: **under 1 minute**.

### W2 — Observability (do concurrently with W1; it is how W1 is proved)

| # | Change | Rationale |
|---|---|---|
| W2.1 | Set `APPLICATIONINSIGHTS_CONNECTION_STRING` on `wwengine-e2e-ldi413` | `wwengine-e2e-ldi413-ai` is **empty over 30 days**. This is why the 30 s has never been attributed. UAT has it. |
| W2.2 | ~~Read the host log for restart lines~~ — **DONE.** `az webapp log download -n wwengine-e2e-ldi413 -g DEV2` (no Kudu basic auth needed; `az` handles the credential). | **Complete, and it rewrote §2.3.** `Restarting host` 0×, `Host is unhealthy` 0×, so the restart theory is dead; the logs instead showed the workflow finishing in 64 ms with the invocation held 30 s, and the 49 MB / 73 %-duplicate log volume. Keep this command in the runbook — it is the only view that separates workflow time from invocation time. |
| W2.3 | Record the `Threads` sample-count instance ramp per run | The metric that exposed §2.4; make it part of the standard load-test report. |

**Exit gate:** the next load run can attribute the residual tail to a named component instead of "somewhere in App Service".

### W3 — Lift the engine's per-instance ceiling (only if W1 leaves a residual tail)

The OOM at concurrency 10 and the flat instance count are the same problem: on a Y1 Consumption plan the HTTP path cannot convert backlog into instances, so per-instance memory binds.

- **W3.1** Evaluate an Elastic Premium plan with `Always Ready` instances for the engine, sized so `minimumElasticInstanceCount ≥ 2`. This is the lever `ShovelBridge-Architecture.md:1130-1140` already flags (its Risk R3, "Consumption (Y1) plan: no alwaysOn, cold starts, capped scale-out") and it is the only change that makes total concurrency above 8 safe. **Cost/infra decision — needs a sponsor, out of scope for a code change.**
- **W3.2** If Consumption must stay: hold total concurrency at ≤ 8, and treat 2.5 min as unreachable without W1 succeeding completely.

### W4 — QueueProcessor tuning (after Gate A passes)

Deploy via `Deploy-WwQueueProcessor.ps1` — all of these are existing parameters, no script change needed for the values themselves:

| # | Current | Target | Parameter |
|---|---|---|---|
| **W4.0** | — | **Re-measure the per-instance OOM ceiling.** Step `MaxConcurrency` 6 → 12 → 24 → 48 on a single replica, 1000 messages each, watching engine `MemoryWorkingSet`, `Http5xx`, and executing-instance count. Stop at the first step that produces "Insufficient memory". This replaces the stale 2026-08-12 figure of 8 (§3.2 b′) | — |
| W4.1 | `maxReplicas: 1` | `2` — this is what unclamps the product | `-MaxReplicas 2` |
| W4.2 | `WORKER__MAXCONCURRENCY: 6` | highest clean step from W4.0, target `24`. **Inert on its own — see §5.0.1: the broker caps in-flight at the `.bite` `Prefetch` of 6.** Must move with W4.3 | `-MaxConcurrency <n>` |
| W4.3 | trigger `Prefetch: 6` | match `MaxConcurrency` exactly at every step. **Baked into the container image and has no env override**, so today each ramp step needs a new image. §5.0.1 proposes the small code change that removes this | trigger `.bite` (or a new `WORKER__PREFETCH`) |
| W4.4 | `cpu: 0.25 / memory: 0.5Gi` | `0.5 / 1.0Gi` (the script's own defaults); revisit above C=24 | `-Cpu 0.5 -Memory 1.0Gi` |
| W4.5 | KEDA `value: 6` | match `MaxConcurrency` — this makes the KEDA rule the exact analogue of Service Bus target-based scaling | `-TargetQueueLength <n>` |
| W4.6 | `pollingInterval: 30` | `5–10` | ACA scale setting — **no script parameter exists** |
| W4.7 | `EXECUTIONLOGLEVEL: INFO` | `WARN` **for throughput-ceiling runs only** | `-ExecutionLogLevel WARN` |

W4.7 matters more than it looks: `AuditingConsumerDecorator.Preview` logs up to 4 KB of every message body at INFO, and ACA ships all of it to Log Analytics from a 0.25 vCPU container. Note the trade-off — the `durationMs=` lines this entire analysis rests on are INFO. Keep INFO when measuring latency; drop to WARN only when measuring a throughput ceiling.

**Exit gate (Gate B):** 1000 messages in ≤ 2.5 min with zero dead-letters and no `OutOfMemoryException` in the engine.

### W5 — Make the benchmark honest

The 2.5-minute figure is not currently a like-for-like target:

- `wwexecution-secure-trigger-queue-e2e` holds **19 459 dead-lettered messages and 0 active**.
- `ShovelBridge-Architecture.md:1125-1132` records a **~15.7 % failure rate** on the 1000-message Service Bus run (51× `MessageLockLost`, 32× `OutOfMemoryException` in `ActivityParser.Parse`, 24× deliberate transient re-throw).
- UAT runs with Easy Auth off and `BYPASS_SECURE_CONFIG=true`.
- `WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS=2` on UAT is read by **no code in `Dev/`** — it is a dead setting, so the ServiceBus extension 5.24.0 default `maxConcurrentCalls = 16` per instance is what actually applies. Anyone reasoning from that setting is reasoning from a value that has no effect.

**W5.1** Re-baseline both paths on **successfully executed workflows**, not queue-drain time.
**W5.2** Either enable `secure.config` evaluation on UAT or document the exemption in the comparison, so the engine does equal work on both sides.
**W5.3** Purge the Service Bus dead-letter queue before the baseline run so failure counts are attributable to that run.
**W5.4** Delete or implement `WAREWOLF_SERVICEBUS_TRIGGER_MAX_CONCURRENT_EXECUTIONS`.

---

## 6. Sequencing and gates

```
W1 (engine host hygiene) ─┬─> GATE A: HttpResponseTime max no longer ~30s,
W2 (observability)  ──────┘           stalls < 2%, C=6 run < 1 min
                                             |
                          +------------------ + ------------------+
                    GATE A passes                          GATE A fails
                          |                                      |
                    W4 (QP tuning)                     W2.2 log evidence
                    W5 (benchmark parity)              -> W3 (plan capacity)
                          |
                    GATE B: 1000 msgs <= 2.5 min, 0 dead-letters
```

Do not start W4 before Gate A. The model in §4 shows W4's whole contribution at today's stall rate is 14.2 min → 10.6 min; after W1 it is 25 s → 19 s. Either way it is not what closes the gap.

---

## 7. Risks

| Risk | Impact | Mitigation |
|---|---|---|
| §2.3 diagnosis is wrong; the 30 s has another cause | W1 lands with no improvement | W2.2 proves or disproves it from the host log before W3 spend is authorised |
| Raising total concurrency triggers the engine OOM | Messages dead-lettered as HTTP 500 (`RetryEngineInternalErrors` is off by default, so a 500 is permanent — **and is currently ignored anyway**, §9) | **W4.0 ramps in steps and stops at the first failure**, rather than trusting the stale ceiling of 8. Fix §9 first if you want the 500-retry safety net to actually exist during the ramp |
| The ramp is run before Gate A | The 30 s tail is mistaken for a capacity limit, and capacity is bought to hide it | W4.0 runs only after Gate A. A stall that survives Gate A must be understood, not out-provisioned |
| `Prefetch` raised without `MaxConcurrency` | Messages park in one replica, invisible to KEDA and to the other replica; drain window widens | The loader already warns on `Prefetch > MaxConcurrency` — treat that warning as a deploy failure |
| **`MaxConcurrency` raised without `Prefetch`** — the likelier mistake, because `MaxConcurrency` is a control-plane setting and `Prefetch` is baked into the image | **A false negative that discredits the whole approach.** In-flight stays at 6 (§5.0.1), the run time does not move, and the natural conclusion is "concurrency does not help" or "we hit the OOM ceiling" — when nothing actually changed | Each W4.0 step must assert the effective value from the replica's own startup line: `Consuming queue '…' (prefetch N, maxConcurrency N, …)`. **If the two numbers differ, the step is void — do not record its result** |
| Disabling `dynamicConcurrencyEnabled` removes host self-throttling | Host accepts more concurrent work than it can hold → OOM | Precisely why the ≤ 8 product cap holds until W3 |
| `EXECUTIONLOGLEVEL=WARN` during a measured run | Loses the `durationMs=` telemetry this analysis depends on | W4.7 applies to throughput-ceiling runs only, never to diagnostic runs |
| Gate A passes but Gate B fails on the engine's cold-start Roslyn OOM | Dead-letters reappear at scale | W3.1, or enable `-RetryEngineInternalErrors` knowing it spends a delivery attempt on genuinely bad messages — and note it is currently **ignored** (§9) |

---

## 8. Change synchronisation

| Artefact | Needed? | Detail |
|---|---|---|
| **Engine app settings** | Yes | W1.a/W1.b/W1.1–W1.6, W2.1 on `wwengine-e2e-ldi413`. All category A — see §5.0 |
| **`host.json`** | **No** | Source-controlled `host.json` keeps `dynamicConcurrencyEnabled: true` and `fileLoggingMode: always` for other consumers. Override per-app via `AzureFunctionsJobHost__logging__*` / `__concurrency__*`, as UAT does. Changing the shipped file would affect every engine deployment. **If a future engine should ship quieter defaults, that is a separate decision with its own blast radius.** |
| **Container App config** | Yes | W4.1, W4.4–W4.7 are category A. **W4.2 is inert without W4.3** — §5.0.1 |
| **Trigger `.bite`** | Yes — **and it forces an image rebuild** | `Prefetch` on trigger `b6d1e4a2-51c7-4f3a-9e88-6c0a7d2f1b90` is baked into the image with no env override, so it cannot be changed from the control plane. §5.0.1 proposes a `WORKER__PREFETCH` override to remove this coupling |
| **Container image / new revision** | Yes, before §9 takes effect | §9's fix is in source and tested but the running container is the **old image**, so both `WORKER__*` keys stay ignored on the live app until a rebuild + new revision |
| **`Deploy-WwQueueProcessor.ps1`** | Values: no. One gap: yes | All targets map to existing parameters except `pollingInterval` (W4.6), which has no parameter — confirm before adding one |
| **`Deploy-WwExecutionEngine.ps1`** | Yes | Should emit W1.1–W1.5 and W2.1 so a fresh engine is not deployed with the same gap. This is the durable fix; the app-settings edits above are the immediate one |
| **`docs/LoadTest-Guide.md`, `docs/RUN-LoadTest-Runbook.md`** | Yes | Add the `Threads` sample-count instance-ramp measurement (W2.3) and the W5 honest-baseline procedure |
| **`docs/ShovelBridge-Architecture.md`** | Yes | Cross-reference this plan from its Risk R3 / capacity recommendation |
| **`Scripts/Tests/`** | Conditional | Only if `Deploy-WwQueueProcessor.ps1` gains a parameter |
| **Unit tests** | **Done for §9** | No production code change is proposed in W1–W5. §9's fix shipped with 7 tests; project at 132/132 |
| **`appsettings.json`** | **Done** | Aligned to the documented 180/210 and the two previously-dead keys added — §9.1 |

---

## 9. Defect found during investigation — **RESOLVED**

**Not part of the throughput gap — surfaced while reading the configuration path. Fixed and tested on approval; see §9.1 for what shipped.**

`Program.cs` builds `QueueProcessorOptions` with `AddOptions<QueueProcessorOptions>().Configure(o => …)` and **no `.Bind(...)`**, assigning keys explicitly. Two keys are never assigned:

- `WORKER:MAXDELIVERYATTEMPTS` → `o.MaxDeliveryAttempts` never set
- `WORKER:RETRYENGINEINTERNALERRORS` → `o.RetryEngineInternalErrors` never set

`Deploy-WwQueueProcessor.ps1:1262-1263` emits both as `WORKER__MAXDELIVERYATTEMPTS` / `WORKER__RETRYENGINEINTERNALERRORS`, and they are live on the container app. **They are silently ignored.** Values coincide with the property defaults (2 / false), so behaviour is correct today — but `-MaxDeliveryAttempts 1` or `-RetryEngineInternalErrors` would be a no-op, and `RetryEngineInternalErrors` is exactly the switch the §7 Gate-B risk row might want.

Related inconsistency: `Program.cs` defaults `ENGINE:TIMEOUTSECONDS` to **45** and `WORKER:SHUTDOWNGRACESECONDS` to **60**, while `QueueProcessorOptions` documents 180 and 210 with a detailed rationale for each. The `Configure` block wins, so those XML-doc rationales are misleading for anyone reading the options class. (The deployed container sets both explicitly, so production is unaffected.)

### 9.1 What shipped

**`.Bind(configuration)` was considered and rejected**, for two load-bearing reasons: the keys deliberately do not match the property names (`ENGINE:BASEURL` → `BaseUrl`, `WORKER:MAXCONCURRENCY` → `MaxConcurrency`), so binding needs a key-mapping layer regardless; and `Bind` treats an empty string as a value, so it would set `SettingsPath = ""` from the `appsettings.json` placeholder — the precise defect `ConfigurationValues` exists to prevent.

Instead the mapping was **extracted from the untestable top-level statement into `QueueProcessorOptionsBinder.Apply(IConfiguration, QueueProcessorOptions, bool)`**, keeping the `ConfigurationValues` semantics exactly. Changes:

| File | Change |
|---|---|
| `Configuration/QueueProcessorOptionsBinder.cs` | **New.** The whole mapping, now unit-testable. Assigns the two missing keys. |
| `Configuration/ConfigurationValues.cs` | Added `ReadBool(string?, bool @default)`. The existing single-argument overload collapses "unset" onto `false`, which is only correct while every boolean happens to default to `false`; the new one makes "unset" mean *keep the declared default*, matching `ReadInt`/`ReadString`. |
| `Program.cs` | `Configure` now delegates to the binder. Dropped the two now-unused local forwarders (`ReadString`, `ReadBool`) to avoid CS8321. |
| `appsettings.json` | **Behaviour change, see below.** `ENGINE:TIMEOUTSECONDS` 45 → 180, `WORKER:SHUTDOWNGRACESECONDS` 60 → 210; added `MAXDELIVERYATTEMPTS` / `RETRYENGINEINTERNALERRORS` so the newly-live contract is discoverable. |

**Defaults are now read off the options instance** (`ReadInt(cfg[...], o.EngineTimeoutSeconds)`) rather than restated as literals, so drift between the mapping and the documented property defaults is structurally impossible rather than merely fixed once.

**Deliberate behaviour change to flag.** A deployment that sets neither variable previously got a 45 s engine timeout and a 60 s drain window; it now gets the documented 180/210. Every deployment through `Deploy-WwQueueProcessor.ps1` is unaffected (it always emits both, defaulting to 180/210), and the live Container App already sets both explicitly. The affected case is a local run or a hand-rolled deployment — where 180/210 is the value the property documentation argues for, and 45 s is the timeout whose only recorded effect is the 2026-08-11 consumer stall. The nesting rule (`engine ≤ drain`) holds for both pairs.

### 9.2 Tests

`Dev/Warewolf.Execution.QueueProcessor.Tests/OptionsBinderTests.cs`, 7 tests — the 5 approved plus 2 covering paths the fix touched:

| # | Test | Covers |
|---|---|---|
| 1 | `Apply_MaxDeliveryAttempts_IsReadFromConfiguration` | approved #1 |
| 2 | `Apply_RetryEngineInternalErrors_IsReadFromConfiguration` | approved #2 |
| 3 | `Apply_EngineTimeoutSeconds_IsReadFromConfiguration` | approved #3 |
| 4 | `Apply_NothingConfigured_LeavesEveryDocumentedDefaultIntact` | approved #4, plus the `Validate` nesting rule on the defaults alone |
| 5 | `Apply_BlankValues_FallBackToDefaultsRatherThanWinning` | approved #5 |
| 6 | `Apply_UseSsl_IsTriStateNotBoolean` | **added** — the tri-state path the new `ReadBool` overload touches; unset must stay `null`, not collapse to `false` |
| 7 | `Apply_DeployScriptEnvVarNames_ReachTheSameOptions` | **added** — the actual defect surface. Tests 1–5 use the `:` key form the binder reads; this one sets the real `WORKER__MAXDELIVERYATTEMPTS` (double underscore, as the deploy script emits) through `AddEnvironmentVariables()` and proves the provider translates it onto the key the binder looks up. Without it, every other assertion could pass while the deployed variable still went nowhere. `[DoNotParallelize]`, env vars restored in `finally`. |

Test 4 deliberately asserts the documented **literals** (180, 210, 1, 2, false, 300), not a comparison against a fresh `QueueProcessorOptions` — the latter would be tautological now that the binder reads its fallbacks off the options object, and would pass even if both sides drifted together.

**Defect-reintroduction check** (`warewolf-test`: "a regression test that has never failed proves nothing"). The original defect was restored in the binder — the two keys unassigned, the 45/60 literals back — and the suite re-run:

- **With the defect: 5 failed, 2 passed.**
- **With the fix: 7 passed.**

The 2 that passed either way are #3 (`ENGINE:TIMEOUTSECONDS` was always read) and #6 (`UseSsl` behaviour is unchanged) — neither covers this defect, and that is stated rather than implied.

**Existing-test audit** (`warewolf-test`: watch for tests that encode the bug as intended behaviour). Four call sites set `EngineTimeoutSeconds`/`ShutdownGraceSeconds` to 45/60 — `ConfigurationTests.cs:389/402/418`, `EngineForwarderTests.cs:59`, `IdempotencyKeyTests.cs:75` — but all set them **explicitly on a constructed options object** to exercise the `Validate` nesting rule or a forwarder timeout. None asserts what the *default* is, so none encoded the defect and none needed changing.

**Verification:** full project **132/132 passing**. Pipeline parity confirmed — `Dev/.azure/pipeline.yml:361` runs `Warewolf.Execution.QueueProcessor.Tests` in a dedicated job with no category filters and no `-Start*` dependency, and these tests need none (in-memory configuration; the one env-var test is `[DoNotParallelize]` and self-restoring).

---

## 10. Open questions

1. **What exactly is the 30 s?** **Largely answered — see §2.3.** Two costs: a per-instance first-execution compile of 25–95 s, and a recurring ~30 s hold *after* a 64 ms workflow completes, releasing in bursts. Ruled out by measurement: host restarts (0× in 23 instance logs), instance count (24 warm, 8–12 used), per-instance contention (< 1 concurrent execution per executing instance), memory (265 MB of 1536 MB), errors and retries (1058× `Http2xx`, 0× 4xx/5xx/401 — Easy Auth rejects nothing), and `secure.config` I/O (cached behind `SecureConfigWatcher`). **Residual unknown:** whether the hold is specifically log-write back-pressure. W1.a–W1.c test that directly and cheaply; do not authorise W3 spend before they land.
2. ~~Why does the engine report a flat ~24 instances?~~ **Resolved.** The count is real (validated against an idle window reading 0) and the engine used 8–12 of the 24. Instance scaling was never the constraint — see §2.4, which replaced the earlier incorrect claim.
3. **Is the engine's OOM ceiling still ~8?** Now a gate, not a question — W4.0. §3.2 b′ explains why the old figure is probably an underestimate.
4. **Does the 2.5-minute Service Bus figure survive W5?** If the honest, successful-execution baseline is (say) 4 minutes, the gap is already smaller than it looks and W3 may be unnecessary.
5. **Should the QueueProcessor keep owning the backlog at all?** The structural alternative to W4 is to stop hand-feeding the engine over HTTP and let the engine's own trigger see the depth — i.e. shovel RabbitMQ → Service Bus and adopt the path that already works. That trades the HTTP hop and the worker's scaling responsibility for a second broker. Out of scope here, but it is the honest long-run comparison and §2.4 is the argument for putting it on the table.
