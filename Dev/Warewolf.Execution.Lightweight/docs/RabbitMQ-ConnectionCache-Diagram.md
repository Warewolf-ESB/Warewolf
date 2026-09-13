# RabbitMQ connection cache — how it fits the Execution Engine, Dev2.Activities and the Server

Companion to [`RabbitMQ-ConnectionCache-Plan.md`](./RabbitMQ-ConnectionCache-Plan.md).

---

## 1. Component fitment

The load-bearing idea: **the shared library declares the contract, the Execution Engine supplies the
implementation, the Server supplies nothing.** `Dev2.Activities` depends only on the interface, so the
Server keeps today's behaviour without a single conditional referring to a host.

```mermaid
flowchart TB
    subgraph SHARED["Dev2.Runtime.Services — shared contract, no implementation"]
        IFACE["IRabbitMqConnectionCache<br/>IRabbitMqConnectionLease"]
        AMBIENT["AmbientRabbitMqConnectionCache<br/><i>static registry, null by default</i>"]
        PRECEDENT["AmbientSourceLoader<br/><i>existing precedent, same shape</i>"]
    end

    subgraph ACT["Dev2.Activities — one code path, two behaviours"]
        PUB["PublishRabbitMQActivity<br/>PerformExecution"]
        CORE["PublishCore(connection, queue, msg, corrId, memo)<br/><i>identical broker calls in both paths</i>"]
    end

    subgraph LW["Warewolf.Execution.Lightweight — Azure Functions, Consumption"]
        OPTS["RabbitMqCacheOptions<br/>WAREWOLF_RABBITMQ_CACHE"]
        IMPL["LightweightRabbitMqConnectionCache<br/><i>refcount + TTL + LRU + memo</i>"]
        ORCH["StartupOrchestrator"]
        LIFE["IHostApplicationLifetime<br/>ApplicationStopping"]
    end

    subgraph SRV["Dev2.Server — Windows service"]
        SRVSTART["Server startup<br/><b>never registers a cache</b>"]
    end

    BROKER[("RabbitMQ broker")]

    AMBIENT -.->|"mirrors"| PRECEDENT
    PUB --> CORE
    PUB -->|"reads .Current"| AMBIENT
    IMPL -->|"implements"| IFACE
    OPTS -->|"enabled?"| ORCH
    ORCH -->|"Register(cache)"| AMBIENT
    ORCH --> LIFE
    LIFE -->|"Dispose()"| IMPL
    SRVSTART -.->|"no call — Current stays null"| AMBIENT
    CORE --> BROKER
    IMPL -->|"owns cached IConnection"| BROKER

    style IMPL fill:#1f6feb,color:#fff
    style AMBIENT fill:#8250df,color:#fff
    style SRVSTART fill:#6e7781,color:#fff
    style OPTS fill:#1a7f37,color:#fff
```

**Reading the diagram.** The only arrow that differs between hosts is `Register(cache)`. Everything in
`Dev2.Activities` is host-agnostic: it asks `AmbientRabbitMqConnectionCache.Current` and gets either a
cache (Execution Engine, when enabled) or `null` (Server, always; Execution Engine when disabled).

---

## 2. The decision at publish time

```mermaid
flowchart TD
    START(["PerformExecution<br/>per iterator row"]) --> RESOLVE["Resolve RabbitMQSource<br/>stamp ConnectionFactory"]
    RESOLVE --> CHECK{"AmbientRabbitMqConnectionCache<br/>.Current == null?"}

    CHECK -->|"yes — Server,<br/>or cache disabled"| OLD["using (Connection = CreateConnection())<br/><b>today's path, unchanged</b>"]
    OLD --> COREA["PublishCore(memo: null)<br/>always declares"]
    COREA --> DISPOSE["dispose channel<br/>dispose connection"]
    DISPOSE --> DONE(["Success"])

    CHECK -->|"no — Execution Engine,<br/>cache enabled"| ACQ["cache.Acquire(key, factory)"]
    ACQ --> HIT{"live entry?<br/>IsOpen && !Poisoned<br/>&& !idle-expired<br/>&& !lifetime-expired"}
    HIT -->|"hit"| REF["AddRef (interlocked)"]
    HIT -->|"miss"| EVICT["evict stale entry<br/>drop cache ref"]
    EVICT --> CAP{"at maxConnections?"}
    CAP -->|"no"| NEW["create connection<br/>recovery OFF, background IO ON"]
    CAP -->|"yes, LRU evictable"| LRU["evict LRU"] --> NEW
    CAP -->|"yes, all in use"| UNCACHED["<b>uncached lease</b><br/>fall back to today's path"]
    NEW --> REF
    REF --> COREB["PublishCore(memo: lease)"]
    UNCACHED --> COREB

    COREB --> MEMO{"memo.IsTopologyFresh<br/>(queueName)?"}
    MEMO -->|"fresh"| PUBLISH["CreateModel → BasicPublish<br/><b>~2 RTT</b>"]
    MEMO -->|"cold / no memo"| DECL["CreateModel → 2x passive declare<br/>→ declare/bind if missing<br/>→ BasicPublish"]
    DECL --> MARK["MarkTopologyDeclared"]
    MARK --> RELEASE
    PUBLISH --> RELEASE["dispose channel only<br/>Release → bump LastUsed, DecRef"]
    RELEASE --> DONE

    COREB -.->|"any exception"| INVAL["lease.Invalidate()<br/>poison + evict"] -.-> RETHROW(["throw"])

    style OLD fill:#6e7781,color:#fff
    style PUBLISH fill:#1a7f37,color:#fff
    style UNCACHED fill:#9a6700,color:#fff
    style INVAL fill:#cf222e,color:#fff
```

---

## 3. Cache entry lifecycle — where disposal actually happens

The states exist to make one guarantee explicit: **a connection is disposed exactly once, by whichever
party takes the reference count to zero.** Eviction alone never disposes, because another thread may be
mid-publish on that connection.

```mermaid
stateDiagram-v2
    [*] --> Live: create connection<br/>RefCount = 1 (cache's own ref)

    Live --> Live: Acquire → AddRef<br/>Release → DecRef + bump LastUsed

    Live --> Evicted: idle-expired (5 min)<br/>lifetime-expired (30 min)<br/>!IsOpen<br/>LRU pressure<br/>Invalidate()

    Evicted --> Disposed: RefCount reaches 0
    Live --> Disposed: cache.Dispose() and RefCount 0

    Disposed --> [*]: socket closed<br/>background IO thread ends<br/>heartbeat timers disposed

    note right of Evicted
        Removed from the map, so never
        handed out again — but the
        connection is still ALIVE for
        any lease still holding a ref.
    end note

    note right of Live
        A zero-refcount entry is never
        resurrected: AddRef uses a
        compare-exchange loop that
        fails on 0.
    end note
```

---

## 4. What is bounded, and by what

| Resource | Bound | Enforced by |
|---|---|---|
| Cached connections | `maxConnections` (default 4) | cap + LRU eviction |
| Connection age | `maxLifetime` (default 30 min) | absolute clock, checked at acquire + sweep |
| Idle connection | `idleTimeout` (default 5 min) | sliding clock, bumped on release |
| Topology memo entries | per-entry expiry (`topologyTtl`, default 30 s) + size cap | swept with the entry |
| Channels per connection | 1 per in-flight publish | fresh `IModel`, disposed in `finally` |
| Sweep timers | exactly 1 per cache instance | single `Timer`, disposed with the cache |
| Recorded topology (`m_recorded*`) | **does not exist** | `AutomaticRecoveryEnabled = false` ⇒ plain `Connection` |
| Foreground threads | **zero** | `UseBackgroundThreadsForIO = true` |

---

## 5. Deployment view

```mermaid
flowchart LR
    OP["Operator"] -->|"az functionapp config appsettings set<br/>WAREWOLF_RABBITMQ_CACHE={...}"| APP["Function App<br/>app settings"]
    APP -->|"read once at startup"| BOOT["Program.cs → StartupOrchestrator"]
    BOOT --> Q{"enabled?"}
    Q -->|"true"| ON["Register cache<br/><b>~2 RTT per publish</b>"]
    Q -->|"false / absent / malformed"| OFF["No registration<br/><b>~9 RTT per publish, today's behaviour</b>"]

    style ON fill:#1a7f37,color:#fff
    style OFF fill:#6e7781,color:#fff
```

Changing the setting restarts the Function App, which is the intended contract — TTLs and the cap are
baked into the cache instance at construction (see plan §4.5). No deploy-script change is required:
an absent variable means disabled.
