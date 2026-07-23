# Azure Functions — Cold-Start & Scaling Performance Results

> Live experiment against `wwenginenewscriptai` (Consumption / Dynamic SKU, South Africa North, .NET 8 isolated worker).
> Driver: public Warewolf workflows discovered from `/public/apis.json` (`baseUrl` of each entry). No auth required.
> Harness: [`Measure-FunctionPerf.ps1`](./Measure-FunctionPerf.ps1). Client-side stopwatch is the primary signal; App Insights `traces`/`dependencies` + Azure Monitor metrics corroborate.
>
> Run date: 2026-07-22 (UTC). Targets: `/Public/Hello World.json?Name=…` and `/Public/Examples/Dice Roll Example/Dice Roll.json`.

---

## Summary Table

| Phase | Scenario | First success | Avg | p50 | p95 | Max | Notes |
|---|---|---|---|---|---|---|---|
| 1 | Cold start (stop→start) | **22,857 ms** | — | — | — | — | first 200 after host init; 403 during init |
| 2 | Warm steady-state (30 seq) | — | 687 ms | 623 ms | 827 ms | 894 ms | 1 instance, bimodal ~560/~790 ms |
| 3a | Sequential (40) | — | 752 ms | 768 ms | 948 ms | 2,161 ms | 30.6 s wall, **1.31 req/s**, 1 instance |
| 3b | Parallel (40 @20) | — | 7,870 ms | 1,355 ms | 50,306 ms | 52,241 ms | 54.3 s wall, **0.74 req/s**, scaled 1→4 instances |
| 3c | High concurrency (100 @50) | — | 11,909 ms | 11,538 ms | 21,154 ms | 21,646 ms | 26.6 s wall, **3.77 req/s**, 100/100 ok; warm instances absorbed the burst |
| 4 | Idle 16 min → cold start | **30,660 ms** | — | — | — | — | scale-to-zero; repeat cold starts on fresh instances |

---

## Phase 1 — Cold Start (stop / start)

Stopped the app, waited 60 s, started it (`20:51:29 UTC`), then fired requests immediately.

| Seq | Latency | HTTP | Interpretation |
|---|---|---|---|
| 1 | 721 ms | 403 | host still initializing (auth pipeline up before workflow engine) |
| 2 | **22,857 ms** | 200 | **true cold start** — CLR JIT/TieredPGO + Warewolf license load + host init |
| 3 | 937 ms | 200 | warming |
| 4 | 814 ms | 200 | warming |
| 5 | 580 ms | 200 | warm |

**Cold-start cost ≈ 22.9 s** for the first successful execution; ~33× slower than warm.
App Insights `traces` show the `Program initialization complete, starting host` sequence for the new instance at the start time.

## Phase 2 — Warm Steady-State

30 sequential requests on the hot instance: **30/30 200**, min 554 / avg 687 / p50 623 / p95 827 / max 894 ms.
Latency is bimodal (~560 ms and ~790 ms) — consistent alternation, likely per-request work vs a lighter cached path.

## Phase 3 — Sequential vs Parallel + Scale-Out

- **Sequential 40:** avg 752 ms, p95 948 ms, 30.6 s wall, **1.31 req/s** — served entirely by **1 warm instance**.
- **Parallel 40 (throttle 20):** all 200 but avg 7,870 ms, p50 1,355 ms, p95 **50,306 ms**, max **52,241 ms**, 54.3 s wall, **0.74 req/s**.

**Parallel was slower than sequential.** The 20-way concurrency burst exceeded the single warm instance's capacity, so the Consumption host **scaled out 1 → 4 instances**. Requests routed to the 3 newly-created instances paid a full cold-start (~50 s) while they spun up, producing the long tail.

Telemetry correlation (App Insights `dcount(cloud_RoleInstance)` per minute):

| Minute (UTC) | Distinct instances | Traces |
|---|---|---|
| 20:51 | 1 | 137 |
| 20:52 | 1 | 876 |
| 20:53 (parallel) | **4** | 1,295 |
| 20:54 | 3 | 108 |

Parallel-window instance breakdown: 1 pre-warm instance (1,185 traces) + 3 cold-started instances (149 / 87 / 87 traces).

**Takeaway:** on a Consumption plan, bursting concurrency does not improve throughput until the new instances finish cold-starting; a warm single instance out-performs a cold scale-out burst for short jobs.

## Phase 4 — Idle Scale-to-Zero → Cold Start

After **16 minutes of zero traffic** (20:55→21:11 UTC), the Consumption host de-allocated all instances. The next requests were fired sequentially:

| Seq | Latency | HTTP | Interpretation |
|---|---|---|---|
| 1 | **30,660 ms** | 200 | cold start from scale-to-zero (fresh instance) |
| 2 | 9,124 ms | 200 | JIT/TieredPGO hot-path still warming |
| 3 | 730 ms | 200 | warm |
| 4 | 30,574 ms | 200 | request routed to **another** freshly-spun instance → cold again |
| 5 | 30,975 ms | 200 | yet another fresh instance cold start |

**Idle scale-to-zero cold start ≈ 30.7 s** — notably **higher than the stop/start cold start (22.9 s)**. Platform-driven scale-from-zero provisions fully on demand, whereas an explicit `az functionapp start` warms part of the pipeline first. The repeated ~30 s spikes on seq 4–5 show the load balancer distributing the sequential requests across several new instances that each paid their own cold start.

Azure Monitor corroboration (App Insights raw-table queries were transiently unavailable at report time; the platform metrics pipeline was unaffected):

| Minute (UTC) | FunctionExecutionCount | Requests |
|---|---|---|
| 21:12 | 1 | — |
| 21:13 | 3 | 1 |
| 21:14 | 1 | 3 |
| 21:15 | — | 1 |

The staggered per-minute counts match the ~30 s spacing of the cold requests.

---

## Phase 3c — High Concurrency (100 requests @ throttle 50)

To push scale-out further than the Phase 3b 40-way burst, 100 requests were fired with a parallel throttle of 50 against the (already warm) `Dice Roll` workflow.

| Metric | Value |
|---|---|
| Requests | 100 (100 ok, **0 errors**) |
| Min / Avg / Max | 1,535 / 11,909 / 21,646 ms |
| p50 / p95 | 11,538 / 21,154 ms |
| Wall-clock | 26.6 s |
| Throughput | **3.77 req/s** |

Azure Monitor corroboration:

| Minute (UTC) | FunctionExecutionCount | Requests | Http2xx |
|---|---|---|---|
| 05:21 | 101 | 4 | 2 |
| 05:22 | — | 105 | 103 |

**Key finding — warm burst scales far better than cold burst.** Unlike Phase 3b (which hit cold-started instances and degraded to 0.74 req/s with a ~50 s p95 tail), this burst landed on already-warm instances and achieved **3.77 req/s (5× higher)** with **zero errors** despite 2.5× the request count and 2.5× the throttle. The latency spread (min 1.5 s → max 21.6 s) reflects queueing/concurrency pressure on a small warm pool plus incremental scale-out, not cold-start penalties. This confirms the dominant cost on this Consumption plan is **cold start**, not raw concurrency: keeping instances warm converts a throughput *regression* under load into a throughput *gain*.

### Instance scale-out (captured)

Instance scale-out **was captured** via a KQL union over the *populated* tables — `union traces, dependencies, performanceCounters | summarize dcount(cloud_RoleInstance)` — returning `-o json` (not `-o table`, which does not render single-row summaries) after allowing for ingestion lag. The burst scaled out to **4 concurrent instances**:

| Signal | Value |
|---|---|
| Distinct instances (`dcount(cloud_RoleInstance)`) | **4** |
| Telemetry samples in window | 2,107 (union of traces + dependencies + performanceCounters) |

Per-instance Warewolf performance counters over the burst (from the `performanceCounters` table — the CPU/memory source of record on Consumption):

| Counter | Instance range (avg → max) |
|---|---|
| `% Processor Time` | 0.33–0.50 % avg, up to **3.04 %** max |
| `% Processor Time Normalized` | 0.16–0.25 % avg, up to 1.52 % max |
| `Private Bytes` | 676 MB – 1.20 GB across instances |
| `Available Bytes` | 765 MB – 1.10 GB across instances |
| `IO Data Bytes/sec` | 366–448 avg, up to ~2.6 KB/s max |

CPU stayed low (<4 %) even at peak concurrency — the latency spread is dominated by **incremental scale-out / cold instances entering the pool**, not CPU saturation. This is the previously-missing scale-out record; the earlier gap was a query-method issue (`traces` alone with `-o table` and no ingestion wait), now fixed in the harness `-Correlate` step.

---

## Cross-cutting Notes


- **Concurrency does not help short jobs on Consumption:** a warm single instance (sequential, 1.31 req/s) beat a 20-way parallel burst (0.74 req/s) because the burst forced 3 new instances to cold-start (~50 s tail).
- **…but warm concurrency does help:** the 100-request @50 burst on already-warm instances hit **3.77 req/s with 0 errors** (Phase 3c) — 5× the cold 40-way burst. The differentiator is warm vs cold instances, not the degree of parallelism.
- **`requests` / `exceptions` App Insights tables stayed empty** throughout (host AI pipeline dormant — see `azure-functions-telemetry.md`). All App Insights correlation used `traces`/`dependencies`/`performanceCounters`; request/execution counts came from Azure Monitor.
- **Capturing instance scale-out correctly:** take `dcount(cloud_RoleInstance)` from a **union of the populated tables** (`traces`, `dependencies`, `performanceCounters`) — not `traces` alone — and read results with `-o json` (the `-o table` renderer drops single-row summaries). Allow 5–10 min for raw-table ingestion and retry; the harness `-Correlate` step automates the wait/retry and falls back to Azure Monitor `InstanceCount` if the raw tables still lag. Client-side stopwatch timings remain the authoritative latency record for all phases.
- Cold start (~23–31 s) is dominated by CLR warmup + Warewolf license/host init; `TieredPGO` means the first few warm requests are still slightly slower before hot-path JIT settles.
- For latency-sensitive workloads, an **always-ready / Premium plan** (or a periodic warm-up ping within the idle timeout) would eliminate both the initial and scale-out cold starts observed here.

## Reproduce

```powershell
cd Dev/Warewolf.Execution.Lightweight/docs/perf
# All four phases (stops/starts the app; ~30 min incl. 16-min idle):
pwsh -File ./Measure-FunctionPerf.ps1
# Individual phases:
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Warm -WarmRequests 30
pwsh -File ./Measure-FunctionPerf.ps1 -Phases Concurrency -SeqRequests 40 -ParRequests 40 -ParThrottle 20
# High-concurrency scale-out burst:
pwsh -File ./Measure-FunctionPerf.ps1 -Phases HighConcurrency -ParRequests 100 -ParThrottle 50
# Any run + telemetry correlation (instance scale-out + perf counters, with ingestion wait/retry):
pwsh -File ./Measure-FunctionPerf.ps1 -Phases HighConcurrency -ParRequests 100 -ParThrottle 50 -Correlate
```
Raw per-request CSVs and logs are written to `docs/perf/runs/`.
