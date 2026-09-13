# RabbitMQ Publish Connection Cache — Old vs New Benchmark

Measured live on **2026-09-13**. Branch `7580-ServerPlatformImprovements` @ `33a330e31a`.

This is the baseline measurement [`RabbitMQ-ConnectionCache-Plan.md`](RabbitMQ-ConnectionCache-Plan.md)
§12.2 records as *not yet performed*.

Part of the September 2026 benchmark series — see
[`Benchmark-Series-2026-09.md`](Benchmark-Series-2026-09.md) for the queue-to-engine throughput runs
measured alongside it.

---

## Result — 1000 publishes per implementation

| Implementation | Total time | Per message | Rate | Published | Errors | Connections opened |
|---|---:|---:|---:|---:|---:|---:|
| **OLD** — connection per publish | **1 340 695 ms** (22 m 21 s) | 1340.7 ms | 0.7 msg/s | 1000 | 0 | 1000 |
| **NEW** — cached connection | **294 770 ms** (4 m 55 s) | 294.8 ms | 3.4 msg/s | 1000 | 0 | **1** |

### **4.5× faster — 1045.9 s (17 m 26 s) saved over 1000 publishes**

A calibration run at 20 publishes per leg, executed first, gave **1327.3 ms → 304.5 ms (4.4×)** —
the ratio holds from 20 to 1000 with no drift and no broker throttling.

---

## What was measured

The real `PublishRabbitMQActivity.PerformExecution` — not a re-implementation of its AMQP sequence.
Both implementations ship in the same binary and are selected at runtime, so **one process, one
build, one broker** covered both legs and the only variable was the resolver:

| Leg | `ConnectionCacheResolver()` returns | Branch taken |
|---|---|---|
| OLD | `null` | `using (Connection = ConnectionFactory.CreateConnection())` — [`PublishRabbitMQActivity.cs:225-229`](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L225-L229) |
| NEW | `LightweightRabbitMqConnectionCache` | the cached branch — [`:243-258`](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L243-L258) |

In production that selection is made by **`WAREWOLF_RABBITMQ_CACHE`**. Absent, empty, malformed, or
`enabled=false` ⇒ the OLD path; that is the default
([`RabbitMqCacheOptions.cs:89-120`](../Infrastructure/RabbitMqCacheOptions.cs#L89-L120),
[`StartupOrchestrator.cs:199-212`](../Infrastructure/StartupOrchestrator.cs#L199-L212)).

## Environment

| | |
|---|---|
| Broker | CloudAMQP, `ostrich-01.lmq.cloudamqp.com:5672`, vhost `bmkzdabu` (credentials from the UAT3 source `.bite`, not recorded here) |
| Queue | `Test_Connection_Queue_For_PublishActivity` (durable, no consumers) |
| Engine build | `G:\UAT3\Apps\Engine`, published 2026-09-13 04:02 |
| Runtime | .NET 10, Windows 11 |
| Cache options | `{"enabled":true}` — all defaults (idle 5 min, maxLifetime 30 min, topologyTtl 30 s, maxConnections 4) |

Fresh activity and `ConnectionFactory` per leg: the cached path calls
`RabbitMqCachedConnectionSettings.Apply(...)` on the factory, so a shared instance would contaminate
the uncached measurement. One uncounted warm-up publish per leg absorbs JIT, DNS and the topology
declares. Legs ran in order OLD → NEW.

## Integrity of the numbers

- **Every publish verified.** `PerformExecution` returns `List<string>{"Success"}`; anything else was
  counted as an error. **0 errors across 2000 publishes.**
- **Broker agrees.** Queue depth rose by exactly the number published (84 → 1085 → 2086).
- **Reuse confirmed, not assumed.** `ConnectionsCreated == 1` for 1000 cached publishes.
- **Mechanism observed mid-run.** During the OLD leg the broker showed a momentary connection with
  `channels=0`; during the NEW leg, one persistent connection with `channels=1` — a channel per
  publish on a shared connection, which is what `PublishCore` does with a lease.

## Caveats

- **Remote-broker figure.** 1.34 s per publish is dominated by round-trip latency: ~9 round trips per
  message uncached versus ~2 cached. On a same-LAN broker the absolute saving per message would be
  much smaller, though the direction holds. This measures the topology UAT actually runs — Warewolf
  publishing to hosted CloudAMQP.
- **Shared broker.** Other UAT3 workers were active on other queues throughout. They do not touch this
  queue, but the broker was not idle.
- **Single run, single order.** No repeat in NEW → OLD order. The 20-publish calibration corroborates
  the ratio; a reversed run would rule out ordering effects entirely.
- **2086 messages left in the queue** (84 calibration + 2000 + warm-ups), durable, no consumers. Purge
  when no longer needed.

## Projected impact

`QueueName` and `Message` carry `[Inputs]`, so `DsfBaseActivity.TryExecute` calls `PerformExecution`
**once per iterator row** (Plan §1). At the rates above, a **500-row recordset publish**:

| | Time |
|---|---|
| OLD | ~11 min 10 s |
| NEW | ~2 min 27 s |

## Reproducing

The measurement used a throwaway harness outside the repo that references the published engine
assemblies and subclasses the activity exactly as
[`PublishRabbitMQActivityCacheTests`](../../Dev2.Activities.Tests/ActivityTests/RabbitMQ/Publish/PublishRabbitMQActivityCacheTests.cs)
does. **No repository code was modified to take these measurements.**

The committed equivalent is
[`LiveBroker_CachedPublishing_IsFasterThanOpeningAConnectionPerPublish`](../../Warewolf.Execution.Lightweight.Integration.Tests/LiveRabbitMqConnectionCacheTests.cs#L210),
which performs the same comparison at 20 publishes:

```powershell
$env:WAREWOLF_E2E_RABBITMQ = 'HostName=<host>;Port=5672;UserName=<user>;Password=<pwd>;VirtualHost=<vhost>'
cd Dev
dotnet test Warewolf.Execution.Lightweight.Integration.Tests --filter TestCategory=LiveBroker
```

Without `WAREWOLF_E2E_RABBITMQ` it reports **Inconclusive**, never a false pass.
