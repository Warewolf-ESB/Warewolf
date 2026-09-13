# Execution Summary — E2E run `wwengine2` / `wwqp2-`

Running record for [Deploy-E2E-Execution-StepByStep.md](Deploy-E2E-Execution-StepByStep.md).
Teardown: [Deploy-E2E-Rollback-Commands.md](Deploy-E2E-Rollback-Commands.md).

> **Every finding below is now automated.** [E2E-Harness-README.md](E2E-Harness-README.md) describes
> `New-WwE2EStaging.ps1` + `Invoke-WwE2EVerification.ps1`, which reproduce this run in two commands
> and score the criteria — including the corrections for **S9**, **S11**, **S12** and the `az --query`
> trap that cost the most time here.

> ### ⚠️ Superseded in part by the 2026-08-11 run — see [Part 6](#part-6--2026-08-11-reliability-run-wwqp3-)
> The 18 criteria passing on 2026-08-06 did **not** mean messages were processed reliably. A later run
> at real burst volume found two defects that the criteria could not see, because both are invisible
> to a queue-depth check: a drained queue and a scaled-back-to-zero worker look identical whether the
> messages succeeded or were quietly dead-lettered. Read Part 6 before treating this record as current.

| | |
|---|---|
| Run type | Isolated parallel run — leaves the 2026-08-05 deployment untouched |
| Analysis completed | **2026-08-06** (read-only, live Azure) |
| Deployment executed | ✅ **2026-08-06** — all 18 criteria passed (see §2.7 and Part 5) |
| Operator | `Sehul.Shah@theunlimited.co.za` |
| Subscription | `South Africa Subscription` — `dd0bc517-5cc7-4b56-bd6a-68e6140db7b3` |
| Tenant | `The Unlimited` — `ca0cc53b-9af4-4067-bcdf-be9c648450d1` |
| Resource group / region | `DEV2` / `southafricanorth` |

---

## Part 1 — Analysis (complete, evidence-backed)

### 1.1 What was analysed

| Subject | Source |
|---|---|
| Execution Engine | [Dev/Warewolf.Execution.Lightweight/](../) — Azure Functions v4 isolated, `routePrefix: ""`, routes `/Public/*` (anonymous), `/Secure/*` (Entra JWT via Easy Auth), `/Services/*` (function key) |
| Queue Processor | [Dev/Warewolf.Execution.QueueProcessor/](../../Warewolf.Execution.QueueProcessor/) — .NET 8 worker, one Container App per RabbitMQ trigger, autoscaled 0→N by the ACA-internal KEDA `rabbitmq` scaler |
| Runbook | [Deploy-E2E-Verification-Runbook.md](Deploy-E2E-Verification-Runbook.md) — 1351 lines, Phases A–F |
| Deploy tooling | [Scripts/Deploy-WwExecutionEngine.ps1](../Scripts/Deploy-WwExecutionEngine.ps1), [Deploy-WwQueueProcessor.ps1](../Scripts/Deploy-WwQueueProcessor.ps1), [Rollback-WwExecutionEngine.ps1](../Scripts/Rollback-WwExecutionEngine.ps1) |

### 1.2 Live Azure baseline (queried 2026-08-06)

**The 2026-08-05 run was never torn down.**

| Resource | State | Consequence |
|---|---|---|
| `wwengine1` | **Stopped** → `403 Site Disabled` on `/Public` *and* `/Secure` | Looks like an auth failure; it is not |
| `stwwengine1`, `wwengine1-ai` | present, tagged `wwx-test-run=wwx-20260805-040246` | rollback targets of the *old* run only |
| `wwengine1-auth` | appId `ab0810f7-8caa-46cf-b3d9-c5dd6a1429b4`, SP `a5db272b-88bd-4697-a6f2-70ac158095f1` | directory object, survives resource teardown |
| `wwqp-ordersuccessqueue` | rev `…--0000009` `Healthy`, 0 replicas, cpu 0.5, max 3 | **live scale rule on `order-success-queue`** |
| `wwqp-orderfailurequeue` | `Healthy`, 0 replicas, cpu 0.5, max 1 | **live scale rule on `order-failure-queue`** |
| ACR `warewolf/queueprocessor` | tag `20260805-220738` | must not be deleted by this run |

Shared platform, all verified fit for purpose and **reused, never created**: `DEV2` (`Succeeded`),
`WWExecutionEngine` + `WWExecutionEngineTestSecret` (enabled), `rabbitmq-uri` (**already exists**,
created 2026-08-04), `tudev2containerregistry`, `dev2-cae` (`Succeeded`, `vnet: null`,
`[Consumption]`, workspace cid `405a4834-1554-4f2a-b5eb-31789fffaa9b`), `workspace-2028k`.
Dev broker `4.tcp.eu.ngrok.io:20313` — **TCP reachable**. Local Warewolf Server on `:3142` — up (401).

### 1.3 Findings that changed the plan

| # | Finding | Evidence | Effect on the plan |
|---|---|---|---|
| **F1** | The old `wwqp-*` apps hold live scale rules on the **same queues** the triggers use, and forward to a **stopped** engine — they would take a share of Phase E's messages and **dead-letter and ack** them | `az containerapp show` scale rules + `wwengine1` `state: Stopped` | This run uses **dedicated queue names** (`*-e2e`); the old apps never wake |
| **F2** | The worker's pump performs **no `QueueDeclare`** — only `BasicQos` → `BasicConsume` | [RabbitMqMessagePump.cs:113-142](../../Warewolf.Execution.QueueProcessor/Messaging/RabbitMqMessagePump.cs#L113-L142) | A `Durable=true` trigger **can** consume a publisher-created **non-durable** queue — no `PRECONDITION_FAILED`. Became **acceptance criterion 8** |
| **F3** | `PublishRabbitMQActivity` declares passively first and only creates a missing queue, with `IsDurable=False` | [PublishRabbitMQActivity.cs:210-224](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L210-L224) | New queues are created **non-durable by the publisher**; the queue must pre-exist before `BasicConsume` and before the KEDA passive declare → step **E0** added |
| **F4** | The DLQ publisher also declares passive-first, creating with `DeadLetterDurable` only when genuinely absent | [RabbitMqDeadLetterPublisher.cs:118-146](../../Warewolf.Execution.QueueProcessor/Messaging/RabbitMqDeadLetterPublisher.cs#L118-L146) | The failure path is safe against the same durability mismatch |
| **F5** | The runbook's **E2** command is wrong for this workflow: `RabbitPublish`'s DataList inputs are **`queue`** and **`total`** (body `hello [[c]]`), not `QueueName` | `RabbitPublish.bite` `<DataList>` | E2 is **one** call `?queue=<q>&total=5`, not five calls. **Doc-sync item — see §4** |
| **F6** | `-EncryptResources` works on a **temp staging copy** | [Deploy-WwExecutionEngine.ps1:1322-1327](../Scripts/Deploy-WwExecutionEngine.ps1#L1322-L1327), [:1403-1415](../Scripts/Deploy-WwExecutionEngine.ps1#L1403-L1415) | `C:\ProgramData\Warewolf\Resources` is never modified — the shared workflow tree is safe |
| **F7** | The staged RabbitMQ source is **DPAPI**-encrypted | `RabbitMQSourceAshley.bite` `ConnectionString="AQAAANCMnd8BFdERjHoAwE/Cl+s…"` | `-EncryptStagedSettings` must run **on this machine**; a DPAPI blob can never be read in the Linux container |
| **F8** | Both triggers already have `Prefetch = 1` | trigger `.bite` files | §0.1 of the runbook needs **no** trigger edits; no prefetch advisory expected |
| **F9** | Both triggers map the whole message to an input named `message` (**no `@` prefix**) | trigger `Inputs[]` | Sent as a JSON body, which the engine binds — no `multipart/form-data` start-up refusal |
| **F10** | `Warewolf_QueueProcessor` holds a **global** (`IsServer=true`) `Execute` row, and `rabbit\*` has no Execute-bearing resource entry | staged `secure.config` | The app-role grant is **defence in depth, not a prerequisite**; adding a per-workflow row would **narrow** access |
| **F11** | Rollback must use the summary whose `created.functionApp = true` | `deploy-WwExecutionEngine-20260805-040247.summary.json` vs `-215600` (all `false` but `entraApp`) | Recorded as a hard warning in the rollback doc §0 |
| **F12** | This run **doubles** the ACA footprint while the old apps remain | core arithmetic vs `dev2-cae` tenants | Environment peak **5.0 cores**; actual steady-state stays at the 0.5-core always-on floor |
| **F13** | The engine has an **uncommitted** change adding inbound `multipart/form-data` binding, raw XML/JSON **query-string** payloads, and `wid` as a reserved key (+168/−3) | [WorkflowFunctionHelper.cs](../Http/WorkflowFunctionHelper.cs) vs `HEAD e6dbaa751c` | **Deployed by decision** — the run exercises it. The engine is therefore *ahead of HEAD*, and the source runbook's "multipart ❌ not supported" table is stale (**S6**) |
| **F14** | The worker's `@`-prefixed `MapEntireMessage` **startup guard was removed** in the same working tree, replaced by a `DEPLOYMENT DEPENDENCY` comment | [ResolvedQueueConfiguration.cs:106-121](../../Warewolf.Execution.QueueProcessor/Configuration/ResolvedQueueConfiguration.cs#L106-L121) | Consistent with F13. Residual risk logged as **S7**: a worker pointed at an engine built *before* F13 now dead-letters every message from such a trigger with no runtime detection |
| **F16** ⚠️ | **`PublishRabbitMQActivity` cannot create broker topology.** A failed passive declare closes the channel; the active declare is then issued on that dead channel and throws `AlreadyClosedException` | Reproduced live: `Already closed: … code=404 'NOT_FOUND - no exchange 'order-success-queue-e2e''`, `classId=40 methodId=10` (`exchange.declare`). Corroborated by [RabbitMqDeadLetterPublisher.cs:139-148](../../Warewolf.Execution.QueueProcessor/Messaging/RabbitMqDeadLetterPublisher.cs#L139-L148), which names this activity as the broken reference it deliberately avoids copying | Invalidated §0.6's assumption that the publisher creates exchange+queue+binding. Topology created out-of-band (**D3**); logged as defect **S11** |
| **F15** | Both new engine code paths appear **untested** | zero test references to `TryGetRawQueryPayload`; every `multipart` hit in the Lightweight test projects is the Web POST *activity* posting outbound to httpbin, not inbound binding | Logged as **S8** — a test plan is required before any test is written |

**Neither F13 nor F14 affects this run's correctness.** Both triggers map the whole message to an
input named `message` with **no `@` prefix**, so the forwarder sends a JSON body
([EngineWorkflowClient.cs:80-85](../../Warewolf.Execution.QueueProcessor/Engine/EngineWorkflowClient.cs#L80-L85))
and the multipart path is never taken in either direction.

### 1.4 Isolation contract

**Created:** `wwengine2`, `stwwengine2`, `wwengine2-ai`, Entra app `wwengine2-auth` (+ SP + Easy Auth
client secret), `wwqp2-ordersuccessqueue`, `wwqp2-orderfailurequeue`, ACR repo
`warewolf/queueprocessor-e2e`, broker queues `order-success-queue-e2e` / `order-failure-queue-e2e`
(+ `-errors`), local `G:\Deployment\triggers-e2e`.

**Reused, never created or deleted:** `DEV2`, `WWExecutionEngine`, `WWExecutionEngineTestSecret`,
`rabbitmq-uri`, `dev2-cae`, `tudev2containerregistry`, `workspace-2028k`, `G:\Deployment\sources`,
`G:\Deployment\settings`, `C:\ProgramData\Warewolf\Resources`.

**Untouched:** `wwengine1*`, `wwqp-*`, `warewolf/queueprocessor`, `sharepoint-wiremock`,
`exchange-wiremock`, `tudev2-kubernetes`, every `WarewolfServer*`.

### 1.5 Expected scale shape

| App | Trigger | Queue | Workflow | min | max | `value` | `mode` / `protocol` |
|---|---|---|---|---|---|---|---|
| `wwqp2-ordersuccessqueue` | `OrderSuccessQueue` | `order-success-queue-e2e` | `rabbit\RabbitProcess` | 0 | **3** | **1** | `QueueLength` / `amqp` |
| `wwqp2-orderfailurequeue` | `OrderFailureQueue` | `order-failure-queue-e2e` | `rabbit\RabbitProcessFailure` | 0 | **1** | **1** | `QueueLength` / `amqp` |

`desiredReplicas = clamp(ceil(queueLength / value), min, max)` → with `value = 1`,
`min(queueLength, Concurrency)`. No `activationValue` (default 0 — a single message wakes the app).

---

## Part 2 — Execution record

> Fill each row as the step completes. Status: ⬜ not run · 🟦 running · ✅ pass · ❌ fail · ⏭️ skipped.

### 2.1 Preparation

| Step | Action | Status | Actual / notes |
|---|---|---|---|
| 0 | Session variables / `az login` confirmed | ✅ | `Sehul.Shah@theunlimited.co.za`, sub `dd0bc517-…`, tenant `ca0cc53b-…` |
| 0.1 | Preflight — tooling | ✅ | pwsh `7.6.4`, az `2.87.0`, dotnet SDK `10.0.302` (+ `8.0.423` installed), extensions `application-insights, authV2, containerapp, log-analytics` — `containerapp` present, no `az extension add` needed |
| 0.1 | Preflight — settings inputs | ✅ | `secure.config`, `authconfig.json`, `Warewolf License.secureconfig` all present |
| 0.1 | Preflight — trigger workflows staged | ✅ | `rabbit\RabbitProcess` **True**, `rabbit\RabbitProcessFailure` **True** |
| 0.1 | Preflight — Azure names free | ✅ | `wwengine2` absent · `stwwengine2` `nameAvailable: true` · `wwengine2-auth` 0 matches · no `wwqp2-*` · no `queueprocessor-e2e` repo |
| 0.1 | Preflight — disk | ✅ | `G:` 6.8 GB free; old publishes 287.1 MB (engine, 736 files) + 106.5 MB (worker, 419 files) |
| 0.2 | 🟡 Isolated trigger copies → `G:\Deployment\triggers-e2e` | ✅ | `2bdec488-…` → `order-success-queue-e2e` (`OrderSuccessQueue`, conc 3, prefetch 1, DLQ `…-errors`, Durable True, `$type` TriggerQueue preserved, input `message`, MapEntireMessage True) · `3ecb4759-…` → `order-failure-queue-e2e` (conc 1). Both `QueueSourceId` = `QueueSinkId` = `fa5f49d7-…` |
| 0.2 | Originals unmodified | ✅ | `03fb9052`/`12345678` still `2026-08-05 14:38`, queues still `order-success-queue` / `order-failure-queue` |
| 0.3 | 🟡 `dotnet publish` engine → `apps\ExecutionEngine2` | ✅ | exit 0 · **0 errors**, 139 pre-existing CS nullable warnings (8 in `WorkflowFunctionHelper`, all `CS8600/8601/8625` nullability) · 736 files / 287.1 MB — **same file count as the 2026-08-05 publish**. Confirms F13 **compiles**: `MultipartReader` / `Microsoft.Net.Http.Headers` resolve transitively via `Dev2.Common`'s `Microsoft.AspNetCore.App` |
| 0.3 | 🟡 `dotnet publish` worker → `apps\QueueProcessor2` | ✅ | exit 0 · **0 errors**, 4 CS warnings (nullability in the shared `KeyVaultSecretManager.cs`) · **no warning on `ResolvedQueueConfiguration.cs`**, so F14's guard removal left no dead locals · 419 files / 106.5 MB — same file count as 2026-08-05 |
| 0.3 | Post-publish invariants | ✅ | `Warewolf.Execution.Lightweight.dll` ✅ · `Warewolf.Execution.QueueProcessor.dll` + `.exe` ✅ · **worker `.bite` count = 0** ✅ (committed `Settings/` sample correctly excluded from publish) · no `Settings\` in worker publish ✅ · publish paths differ ✅ |
| 0.3 | Engine publish has **no** `Resources/` folder | ✅ | Verified absent (as in the 2026-08-05 publish). The build's `workflow-index.json: 129 entries` writes to `Dev\Resources - Release\Resources\`, **not** the publish output — so `-WorkflowsSourcePath` remains mandatory in Phase B |

### 2.2 Phase A — reused resources (creates nothing)

| Check | Expected | Status | Actual |
|---|---|---|---|
| RG state | `Succeeded` | ⬜ | |
| `WWExecutionEngineTestSecret` enabled | `true` | ⬜ | |
| ACR loc / sku / publicNet / state | `southafricanorth` / `Basic` / `Enabled` / `Succeeded` | ⬜ | |
| ACA env state / loc / vnet / profiles | `Succeeded` / `South Africa North` / `null` / `[Consumption]` | ⬜ | |
| Vault data-plane role | `Key Vault Secrets Officer` (or User) | ⬜ | |
| `$RabbitSecretUri` | `https://wwexecutionengine.vault.azure.net/secrets/rabbitmq-uri` | ⬜ | |
| `wwqp2-*` absent from `dev2-cae` | empty | ⬜ | |
| Core quota vs 5.0 peak | headroom | ⬜ | |

### 2.3 Phase B — Execution Engine

| Step | Action | Status | Actual |
|---|---|---|---|
| B1 | 🟢 Dry run — plan reviewed | ✅ | exit 0, `Dry-run complete (no cloud changes made)`. Run tag `wwx-test-run=wwx-20260806-185324`. Plan: `wwengine2` / `stwwengine2` / `wwengine2-ai` (all to be **created**), RG `DEV2` **already exists**, Consumption Y1 / Windows / dotnet-isolated 8 / Functions v4, `Standard_LRS`. Auth config loaded (7 groups, 4 user assignments). Elasticsearch, persistence, JobProcessor and RabbitMQ-triggers all **off** (worker deploys separately in Phase C). Endpoint `https://wwengine2.azurewebsites.net`. Log: `…\e2e-wwengine2\deploy-WwExecutionEngine-20260806-185354.dryrun.log` |
| B1 | Encryption preview | ✅ | Key `key-2026-08-06-v2` retrieved · **100** `.bite` scanned, **11** to encrypt (**5 DPAPI decrypted OK**, 6 plaintext), 11 encrypted / 0 failed, **11/11 verified decryptable in-memory**, no plaintext written to disk. `secure.config` was `[Plaintext]` → auto-encrypted. `workflow-index.json` generated with **100** entries |
| B1 | Vault RBAC grant is a no-op | ✅ | Operator already holds `Key Vault Secrets Officer` (+ `Secrets User`, `Crypto Officer`) on the vault, so Phase 3's `Secrets Officer` grant adds nothing and leaves **nothing to roll back** |
| B2 | 🔴 Deploy | ✅ | exit 0 · `status: completed` · `lastPhase: Phase 5  Verify` · run tag **`wwx-test-run=wwx-20260806-190737`** on all three resources (verified) · `EXECUTIONLOGLEVEL=ERROR` applied as directed · deployed via `az zip-deploy` · **`GET /apis.json → 200`** |
| B2 | `created` map (rollback authority) | ✅ | `storageAccount=True`, `functionApp=True`, `appInsights=True`, `entraApp=True`, `resourceGroup=False`, `keyVault=False` — this summary **is** the authoritative rollback target |
| B2 | Encryption result | ✅ | Key `key-2026-08-06-v2` · **100** scanned, **11** encrypted (**5 DPAPI** decrypted OK, 6 plaintext), 0 failed, **11/11 verified decryptable in-memory** · `workflow-index.json` 100 entries · `secure.config` auto-encrypted from `[Plaintext]` |
| B2 | Auth provisioning | ✅ | Entra app `wwengine2-auth` created · `user_impersonation` scope exposed · **7 app roles** reconciled · **4 user role assignments** created (Ashley→Adminstrators, Yogesh→Developers, Aakash→Operators, Sehul→wwusers) · Easy Auth migrated **v1→v2**, Microsoft provider configured, `--action AllowAnonymous`, token store **enabled** · client secret `easyauth-202608061913` expires **2027-08-06** in `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` |
| B3 | Summary path | ✅ | `G:\Deployment\logs\e2e-wwengine2\deploy-WwExecutionEngine-20260806-190738.summary.json` |
| B3 | `runId` | ✅ | `wwx-20260806-190737` |
| B3 | `$EngineAppId` (clientId) | ✅ | `9f191c9b-3ea9-47ae-b2bd-2233600ea175` |
| B3 | Entra app **objectId** | ✅ | `817fa4c5-2b53-4c7d-a959-93959f5fb525` |
| B3 | `$EngineSpId` (SP objectId) | ✅ | `a1e41b8c-463d-4f21-bd44-6fd6495db967` — needed by the Phase C app-role assignment |
| B3 | Engine Function App MI principalId | ✅ | `a417cec1-0f95-4212-8e49-f04cfd0e69ec` |
| B3 | App Insights | ✅ | `wwengine2-ai` created, InstrumentationKey `24c6f31a-ebe0-4b99-a346-b8e4894…` |
| B3 | `Configure-WwExecutionAuth.output.json` | ✅ | written to `Scripts\`, and **gitignored** (`.gitignore:164`) — no repo dirt |
| B4-1 | `/Public/Hello World.json?Name=e2e` anonymous | **200** ✅ | `{"Message": "Hello e2e."}` |
| B4-2 | `/Secure/Hello World.json` no token | **401** ✅ | `{"error":"unauthorized","message":"A valid Bearer token is required."}` |
| B4-3 | `/Secure/Hello World.json` with token | **200** ✅ | `{"Message": "Hello e2e-secure."}`. **`az account get-access-token` does NOT work on a fresh registration** — `AADSTS65001 consent_required`, because the Azure CLI client `04b07795-…` is not pre-authorized (**S9**). Obtained instead by **client credentials** using the app's own Easy Auth secret: `aud=9f191c9b-…`, **`roles=(none)`**, `expires_in=3599` |
| B4-3 | `/Secure/rabbit/RabbitProcess.json?message=secure-e2e` with token | **200** ✅ | `{"output": "secure-e2e"}` — a **roleless** app-only token executes both workflows, independently re-proving the §0.3 authorization scope rule and **predicting that Phase C's app-role grant is unnecessary** |
| B4-3 | Audience form accepted | ℹ️ | Easy Auth is configured with `--allowed-token-audiences api://9f191c9b-…`, and the v2 token carries the **bare GUID** `aud=9f191c9b-…`. Accepted — both are legitimate audience forms for the same app, and Easy Auth runs `AllowAnonymous` so the engine's own JWT validation is authoritative |
| B4-4 | `/Services/Hello World.json` no key | **401** ✅ | |
| B5 | `rabbit/RabbitProcess` via `/Public` | **500 + workflow body** ✅ | `Scalar value { message } is NULL: { output }` → **found, authorized, executed** |
| B5 | `rabbit/RabbitProcess.json?message=hello-e2e` | **200** ✅ | `{"output": "hello-e2e"}` — **stronger than the runbook's check**: the workflow runs and echoes its input, so Phase E's payload path is proven before any message is published |
| B5 | `rabbit/RabbitProcessFailure` via `/Public` | **500 + workflow body** ✅ | `Scalar value { message } is NULL: { output }` → found, authorized, executed |
| B6 | `/Public/rabbit/RabbitPublish.json` reachable | ✅ **not 404** | 500 `TO range must be a whole number from 1 onwards.` → route found and **executing**; confirms `total` drives the publish loop and must be **≥ 1** |

### 2.4 Phase C — QueueProcessor

| Step | Action | Status | Actual |
|---|---|---|---|
| C1 | 🟢 Dry run | ✅ | exit 0. `Triggers resolved : 2` · `Sources to stage : 1` (`fa5f49d7-…` ← `RabbitMQSourceAshley.bite`, referenced by **both** triggers — no gap, so no `Unresolved RabbitMQ source(s)` abort) · `Peak cores (max × cpu) : 2` |
| C1 | Plan scale lines | ✅ | `wwqp2-ordersuccessqueue queue='order-success-queue-e2e' max=3 min=0 value=1 prefetch=1` · `wwqp2-orderfailurequeue queue='order-failure-queue-e2e' max=1 min=0 value=1 prefetch=1` — **exactly as predicted in §1.5** |
| C1 | No prefetch advisory, no rename advisory | ✅ | Both triggers already `Prefetch = 1`; the two trigger `Name`s differ so no collision escalation — names derived at step 1 (trigger `Name`) |
| C1 | Image isolation | ✅ | `tudev2containerregistry.azurecr.io/warewolf/queueprocessor-e2e:20260806-192849` — **`-e2e` suffix present**, so the 2026-08-05 `warewolf/queueprocessor:20260805-220738` image is untouched |
| C1 | Encryption modes | ✅ | Both documented modes fired: source `fa5f49d7-….bite` **(attribute)**, both triggers **(whole-file)** — then verified before the image build |
| C1 | CAE mitigation | ✅ | `Secret 'rabbitmq-connection' set inline from Key Vault … (no runtime keyvaultref, so CAE cannot block it)` |
| C1 | Engine wiring | ✅ | `ENGINE__BASEURL=https://wwengine2.azurewebsites.net`, `ENGINE__RESOURCEAPPID=9f191c9b-…`, `ENGINE__SCOPE=api://9f191c9b-…/.default`, **`ENGINE__TENANTID` set** (so no `ENGINE__TENANTID is not set` warning expected at cold start) · timeouts `engine 45s ≤ drain 60s < termination 90s` · worker `EXECUTIONLOGLEVEL=INFO` (kept, so the queue-execution lines are visible) · App Insights wired to the **same component** as the engine, which is what makes the E4 `dependencies` query work |
| C1 | KEDA rule preview | ✅ | `queueName=<queue> mode=QueueLength value=1 protocol=amqp` + `--scale-rule-auth host=rabbitmq-connection`, **no `activationValue`** |
| C1 | `[-] No managed identity principalId resolved …` ×2 | ℹ️ **dry-run artifact** | The grant helper requires the app to exist first ([Deploy-WwQueueProcessor.ps1:370-382](../Scripts/Deploy-WwQueueProcessor.ps1#L370-L382)); under `-DryRun` no app is created, so no principalId can resolve. Not a defect — the real run performs both grants |
| C1 | `az group create --name DEV2` in Phase 1 | ℹ️ **verified harmless** | The worker script ensures the RG **unconditionally**, unlike the engine script which checks first. `DEV2` has `tags: null` and the same location, so the call is a true no-op here. Logged as **S10** |
| C2 | 🔴 Deploy | ✅ | exit 0 · image **`tudev2containerregistry.azurecr.io/warewolf/queueprocessor-e2e:20260806-193854`** · both apps `status: deployed` · summary `deploy-WwQueueProcessor-20260806-194713.summary.json` |
| C2 | Staged-settings encryption verified **before** the image build | ✅ | source by attribute + both triggers whole-file, then re-read with `-VerifyOnly`: `OK (whole file)`, 1/1 decrypt OK, 0 failed per file |
| C2 | RBAC grants landed | ✅ | Per app: `Granted 'AcrPull' on 'tudev2containerregistry' … (verified)` and `Granted 'Key Vault Secrets User' on 'wwexecutionengine' … (verified)` — 4 writes total. Confirms C1's "grants skipped" note was purely a dry-run artifact |
| C2 | CAE mitigation applied | ✅ | `Secret 'rabbitmq-connection' set inline from Key Vault … (no runtime keyvaultref, so CAE cannot block it)` — per app |
| C2 | `wwqp2-ordersuccessqueue` principalId | ✅ | `0b2f4962-42ad-4e62-8e83-a4df401627c8` |
| C2 | `wwqp2-orderfailurequeue` principalId | ✅ | `d9123970-e7b4-45a2-aaf0-37467bb550cb` |
| C2 | QP summary path | ✅ | `G:\Deployment\logs\e2e-wwengine2\deploy-WwQueueProcessor-20260806-194713.summary.json` |
| C3 | App-role grant (optional) | ⏭️ **skipped** | Deliberate. B4-3 proved a **roleless** app-only token returns 200 on both `Hello World` and `rabbit/RabbitProcess`, so the grant is defence-in-depth, not a prerequisite. The script's closing note still prompts for it |
| C4 | Provisioning verified | ✅ | Both apps carry the `-e2e` image and a resolved system-assigned MI. **Workload not yet exercised** — that is E0's job, since `Healthy` at `minReplicas 0` proves nothing |

### 2.5 Phase D — scale rules

Verified live with `az containerapp show` (2026-08-06):

| App | min | max | rules | rule name / type | queueName | mode | value | protocol | activationValue | auth | Status |
|---|---|---|---|---|---|---|---|---|---|---|---|
| `wwqp2-ordersuccessqueue` | **0** | **3** | **1** | `rabbitmq-backlog` / `rabbitmq` | `order-success-queue-e2e` | `QueueLength` | **1** | `amqp` | **absent** | `host=rabbitmq-connection` | ✅ |
| `wwqp2-orderfailurequeue` | **0** | **1** | **1** | `rabbitmq-backlog` / `rabbitmq` | `order-failure-queue-e2e` | `QueueLength` | **1** | `amqp` | **absent** | `host=rabbitmq-connection` | ✅ |

Both run image `…/warewolf/queueprocessor-e2e:20260806-193854`. MIs: `0b2f4962-…` / `d9123970-…`.
Effective rule per app: `desiredReplicas = clamp(ceil(queueLength / 1), 0, max)` = `min(queueLength, max)`.

**Criteria 5, 6 and 7 met:** one app per trigger, exactly one `rabbitmq` rule each, `min = 0`,
`max` = the trigger's `Concurrency` (3 / 1), `value = MaxConcurrency = 1`, and **no `activationValue`**
— so a single message wakes either app.

### 2.5a Broker topology this run will create (§0.6 requirement)

| Object | Created by | Durability | Mechanism |
|---|---|---|---|
| exchange `order-success-queue-e2e` (direct) | `PublishRabbitMQActivity` on first publish | **non-durable** | `ExchangeDeclarePassive` → on 404 `ExchangeDeclare(name, Direct, false, false)` |
| queue `order-success-queue-e2e` | same | **non-durable** | `QueueDeclarePassive` → on 404 `QueueDeclare(name, false, false, false)` |
| binding queue→exchange, routingKey `""` | same | — | `QueueBind(name, name, "")`, only when either object was newly created |
| `order-failure-queue-e2e` + its exchange + binding | same | **non-durable** | identical path |
| DLQ `order-success-queue-e2e-errors` | `RabbitMqDeadLetterPublisher`, only if a message is dead-lettered | **durable** (`DeadLetterOptions.Durable = true`) | passive-first, then create on 404 |
| DLQ exchange / binding | **none needed** | — | publishes to `exchange: ""` (AMQP **default exchange**), which routes implicitly by queue name |

A deliberately **mixed-durability** topology: work queues non-durable (publisher-created), DLQs
durable (worker-created). Not a conflict — each object is declared by exactly one party, and the
consumer asserts nothing (**F2**).

### 2.6 Phase E — end-to-end proof

| Step | Action | Expected | Status | Actual |
|---|---|---|---|---|
| E0-pre | 🔴 **Broker topology created out-of-band** (see **D3**/**S11**) | ✅ | `PublishRabbitMQActivity` **cannot** create topology, so both exchanges (direct, non-durable), both queues (non-durable) and both bindings (routingKey `''`) were declared via a one-off AMQP script. Broker: **RabbitMQ 3.13.7** |
| E0-pre | Topology verified on a **fresh** connection | ✅ | `ExchangeDeclarePassive` → EXISTS ×2 · `QueueDeclarePassive` → EXISTS ×2, `messages=0 consumers=0` |
| E0 | 🔴 Engine publish 1 msg per queue | **200 ×2** ✅ | `{"output": "Success", "c": 1}` for both — the same call that previously failed with `NOT_FOUND - no exchange` |
| E0 | **Binding routes** (the object AMQP cannot query) | ✅ | Post-publish depth: `order-success-queue-e2e messages=1` — the message traversed exchange → binding → queue. `order-failure-queue-e2e messages=0 consumers=1`, already consumed |
| E0 | KEDA activation 0→1, **unaided** | ✅ | Both apps reached **1 running replica** with no manual pin — so the `--min-replicas 1` step in the plan proved **unnecessary** and was never run |
| E0 | Cold-start log order: catalog → loader → resolved → pump | ✅ **in order** | `Source catalog cached 1 RabbitMQ source(s) … fa5f49d7-… -> amqp://testuser@4.tcp.eu.ngrok.io:20313/` → `QueueConfigurationLoader loading triggers from '/app/Settings/triggers'` → `resolved trigger 'OrderSuccessQueue' (2bdec488-…): queue='order-success-queue-e2e', workflow='rabbit/RabbitProcess', prefetch=1, concurrency=3, durable=True, deadLetter='order-success-queue-e2e-errors', mapEntireMessage=True, tls=False` → `Consuming queue 'order-success-queue-e2e' (prefetch 1, maxConcurrency 1, consumerTag 'amq.ctag-yMtP_…')` |
| E0 | Key Vault path works from inside the container | ✅ | `KeyVaultSecretManager … CredentialType: DefaultAzureCredential` → `Fetching secret 'WWExecutionEngineTestSecret'` → `key material parsed successfully. KeyId: key-2026-08-06-v2` → `Key Vault AES decrypt hook wired (WFAES:: values are now readable)` — proves the C2 `Key Vault Secrets User` grant and WFAES staging both work |
| E0 | Connection logged without password | ✅ | `amqp://testuser@4.tcp.eu.ngrok.io:20313/` — **no password** in any line |
| E0 | No `ENGINE__TENANTID is not set` warning | ✅ **absent** | |
| E0 | **No `PRECONDITION_FAILED — inequivalent arg 'durable'`** | ✅ **absent** | **Criterion 8 proven**: `durable=True` triggers consumed **non-durable** queues on both apps, because the pump never declares (**F2**) |
| E0 | TLS advisory | ⚠️ **expected** | `Broker connection … is NOT using TLS … Production requires AMQPS - see the go-live gate` — the documented opt-in default, logged once per app |
| E0 | Execution succeeded, both apps | ✅ | `body=hello 1` (7 bytes) → `POST https://wwengine2.azurewebsites.net/secure/rabbit/RabbitProcess.json` → **200** → `Queue execution succeeded … durationMs=897`. Failure app: `RabbitProcessFailure.json` → **200** → `durationMs=2377` |
| E0 | Engine accepted the worker MI with **no app role** | ✅ | Both `POST /secure/…` returned **200** with no `Warewolf_QueueProcessor` assignment — confirms the C3 skip and re-proves the scope rule at runtime |
| E1 | Zero baseline | ✅ **0 / 0** | Both apps at 0 replicas, both queues `msgs=0 cons=0` at 20:09:02 |
| E2 | 🔴 Publish `total=5` | **200** ✅ | `{"output": "Success", "c": 5}`, returned in 10 s |
| E3 | Activation (leaves 0 unaided) | ✅ **yes** | `20:09:43 → 0`, `20:09:59 → 1`. No manual intervention — confirms `activationValue` is at its default |
| E3 | **Peak replicas** | ✅ **3** | `20:10:15 → 3`, exactly `min(ceil(5/1), 3)`. Full cycle observed: **0 → 1 → 3 → 0** (back to 0 at `20:16:07`). Held at 3 for ~5.5 min after draining — ACA's ~5 min cool-down, **not** a fault |
| E3 | Per-replica independence | ✅ | **4 distinct replicas** of the success app each logged their own `Consuming queue 'order-success-queue-e2e' (prefetch 1, maxConcurrency 1, consumerTag …)`: `-2dbpv`, `-smbxz`, `-gvtfw`, `-v659q` |
| E3a | `T` — median `durationMs` | ✅ **897 ms** | Success app: 6 succeeded, median **897 ms**, min 263 ms, max **28,614 ms**. The max is a cold-start replica's first message — the measured price of `minReplicas = 0`. Failure app: 1 succeeded, 2,377 ms. Source: `ContainerAppConsoleLogs_CL` across all replicas |
| E3b | Does `QueueLength` count unacked? | ✅ **READY ONLY** | Measured on a throwaway queue (`e2e-unacked-probe`, since deleted): publish → `MessageCount=1`; message held **unacked** → **`MessageCount=0`**; `Nack(requeue=true)` → `MessageCount=1`. **Unacked messages are invisible to the scaler**, so a long-running message can be scaled away underneath itself and the E7 drain path is the only protection. Current budget is safe: max observed 28.6 s < `EngineTimeout 45 s` ≤ `ShutdownGrace 60 s` < `Termination 90 s` |
| E4 | `Queue execution succeeded` count | ✅ **6** | 1 (E0) + 5 (E2) on the success app; 1 on the failure app. Zero `Queue execution failed`, zero `Dead-lettered`, zero `PRECONDITION_FAILED` across all replicas |
| E4 | App Insights `dependencies` | ✅ | `resultCode=200  success=True  POST /secure/rabbit/RabbitProcess.json  calls=6`. ℹ️ The worker's `Dev2Logger` lines do **not** reach App Insights `traces` (only HttpClient dependency telemetry does) — container logs / Log Analytics are the source for `durationMs`. Logged as **S12** |
| E5 | Main queue drained; DLQ did **not** grow | ✅ | Both work queues `msgs=0 cons=0`; **both DLQs never came into existence at all** (`order-success-queue-e2e-errors` / `-failure-…` absent) — the strongest possible form of "nothing was dead-lettered" |
| E6 | Scale back to zero | ✅ | `20:16:07 → 0` replicas, within the documented ~5 min cool-down |
| E2b | 🔴 Publish **36** messages, restart mid-drain | ✅ | Burst published while 3 replicas were consuming; `az containerapp revision restart --revision wwqp2-ordersuccessqueue--0000002` → `Restart succeeded` at 20:48:26 local |
| E7 | 🔴 Forced restart drains cleanly | ✅ | **All three replicas** logged `Cancelled consumer 'amq.ctag-…' on 'order-success-queue-e2e'; draining 0 in-flight message(s) with a 60s budget.` → `Drain of 'order-success-queue-e2e' completed cleanly.` at 15:18:48 / 15:19:09 / 15:19:29 UTC. **No `Drain window … still in flight`** |
| E7 | No loss, no double execution | ✅ | Totals over 4 h: `starting=42 succeeded=42 failed=0 deadLettered=0` — exactly 6 (E0+E2) + 36 (E7). In the E7 window: **executions=36, distinctBodies=36** → every message executed **exactly once**; no redelivery, no duplication |
| E7 | Replicas exceeded `maxReplicas` transiently | ℹ️ **expected** | Observed **5** replicas (max=3) during the restart, because ACA starts new replicas while the old ones drain. Settled back to 3, then 0 |
| E7 | Caveat on drain depth | ⚠️ | Each replica reported `draining 0 in-flight message(s)`: with `prefetch=1` and `T ≈ 897 ms` the in-flight window is very short, so the drain path **engaged and reported correctly but never consumed the 60 s budget**. Correctness here rests on the 36/36 distinct-body result, not on the grace budget being exercised. A genuinely long workflow would be needed to stress `-ShutdownGraceSeconds` |
| E7 | Aborted publish client did not truncate the burst | ℹ️ | The publishing job was killed when its PowerShell session ended, yet all 36 messages arrived — the engine completed the workflow server-side after the client disconnected |

### 2.7 Acceptance criteria

| # | Criterion | Status |
|---|---|---|
| 1 | `/Public/*` 200 anonymously | ✅ |
| 2 | `/Secure/*` 401 without token, 200 with | ✅ |
| 3 | Both trigger workflows resolve | ✅ |
| 4 | Both revisions `Healthy`; catalog + loader + pump logged; no tenant warning | ✅ |
| 5 | One app per trigger, exactly one `rabbitmq` rule each | ✅ |
| 6 | `min = 0`; `max` = 3 / 1; `value` = 1 | ✅ |
| 7 | No `activationValue` | ✅ |
| 8 | **`Durable=true` trigger consumes a non-durable queue** | ✅ |
| 9 | Replicas at 0 on an empty queue | ✅ |
| 10 | Publishing raises replicas above 0 unaided | ✅ |
| 11 | **Peak replicas = 3** | ✅ |
| 12 | `T` recorded (median **897 ms**) | ✅ |
| 13 | Unacked-message semantics recorded (**ready only**) | ✅ |
| 14 | One success per message; `resultCode=200` | ✅ |
| 15 | Queue drains; DLQ does not grow (**never created**) | ✅ |
| 16 | Replicas return to 0 | ✅ |
| 17 | Forced restart drains cleanly | ✅ |
| 18 | 2026-08-05 run untouched | ✅ |

## ✅ All 18 criteria proven.

Two qualifications, stated so the result is not over-read:

1. **Criterion 8's premise changed.** The queues are non-durable because **this run created them that
   way** (D3), not because `PublishRabbitMQActivity` did — it *cannot* (**F16**/**S11**). The property
   under test is unaffected: a `durable=True` trigger consumed a non-durable queue with no
   `PRECONDITION_FAILED`, on both apps.
2. **Criterion 17 was not stressed.** The drain path engaged and reported cleanly on all three
   replicas, but each had **0 messages in flight**, so `-ShutdownGraceSeconds` was never consumed. No
   loss or duplication occurred (36 executions / 36 distinct bodies), but a long-running workflow would
   be needed to prove the grace budget itself. This matters *because* of **E3b**: unacked messages are
   invisible to the scaler, so the drain path is the only protection for in-flight work.

### 2.7a Criterion 18 — isolation verified (2026-08-06)

| Check | Result |
|---|---|
| Old Container Apps | `wwqp-ordersuccessqueue` min=0 max=3, `wwqp-orderfailurequeue` min=0 max=1 — **unchanged**, both still on `warewolf/queueprocessor:20260805-220738` |
| This run's apps | both on `warewolf/queueprocessor-e2e:20260806-193854` — separate repository ✅ |
| `dev2-cae` tenants | exactly **6**: `sharepoint-wiremock`, `exchange-wiremock`, the 2 old apps, the 2 new apps |
| Neighbours | `sharepoint-wiremock` min=1 max=1 cpu=0.5 · `exchange-wiremock` min=0 max=1 cpu=0.5 — **unchanged** |
| Old engine resources | `wwengine1`, `stwwengine1`, `wwengine1-ai` all still tagged `wwx-20260805-040246` |
| Old Entra app | `wwengine1-auth` → `ab0810f7-8caa-46cf-b3d9-c5dd6a1429b4`, intact |
| ACR repositories | `warewolf`, `warewolf/queueprocessor` **and** `warewolf/queueprocessor-e2e` all present |
| Shared platform | `dev2-cae` `Succeeded` · `WWExecutionEngineTestSecret` enabled |

**Two observations that are *not* attributable to this run, recorded for accuracy:**

1. **`wwengine1` is now `Running`** (it was `Stopped` at the session baseline). The activity log records
   `Microsoft.Web/sites/stop/action` then `start/action`, both `Succeeded`, at **13:51:20–13:51:26 UTC**.
   No command in this run targeted `wwengine1` — it was an out-of-band action. Caveat: the activity-log
   `caller` is the same identity this run's `az` commands use, so the log alone cannot distinguish
   operator actions from automation; the attribution rests on the command history, not the log.
2. **`order-success-queue` (the OLD queue) holds 2 messages with 0 consumers**, and
   **`order-failure-queue` does not exist on the broker at all.** Every publish in this run targeted a
   `*-e2e` queue, so these messages did not come from here — but this queue's depth was never sampled
   at the session baseline, so that cannot be proven from measurements taken here.
   *Side effect worth knowing:* `wwqp-orderfailurequeue`'s scale rule points at a queue that does not
   exist, so its KEDA passive declare cannot succeed and that app can never scale — a latent fault in
   the 2026-08-05 deployment, unrelated to this run.

### 2.8 Teardown

| Step | Action | Status | Actual |
|---|---|---|---|
| F1 | 🔴 `az containerapp delete` ×2 (`wwqp2-` only) | ⬜ | |
| F2 | 🟢 `Rollback-WwExecutionEngine.ps1 -DryRun` reviewed | ⬜ | |
| F2 | 🔴 `Rollback-WwExecutionEngine.ps1` executed | ⬜ | |
| F3 | 🔴 `az acr repository delete --repository warewolf/queueprocessor-e2e` | ⬜ | |
| F4 | App Insights removed | ⬜ | by step 2 / separately |
| F5 | Entra app `wwengine2-auth` removed | ⬜ | by step 2 / separately |
| F6 | 🔴 Orphaned role assignments cleaned (KV + ACR ×2) | ⬜ | |
| F7 | Broker queues cleaned (optional) | ⬜ | |
| F8 | 🟡 `triggers-e2e` removed (optional) | ⬜ | |
| F9 | 🟢 §9 verification — **all of it** | ⬜ | |

---

## Part 3 — Deviations, incidents and decisions

| # | When | What happened | Resolution |
|---|---|---|---|
| D1 | Step 0.3 | Publishing over `G:\Deployment\apps\{ExecutionEngine,QueueProcessor}` would leave stale files behind and would make the "0 `.bite` files" check an *inherited* result rather than a real test of the current csproj | Published to **fresh** `ExecutionEngine2` / `QueueProcessor2` instead; the 2026-08-05 output is retained as a known-good fallback. Step 0 variables in the step-by-step updated to match |
| D2 | Before Step 0.3 | An **uncommitted** engine change (F13) and a matching worker change (F14) were found in the working tree, ahead of `HEAD e6dbaa751c` | Operator directed that the working tree be deployed as-is. Recorded as F13–F15 and sync items S6–S8; no source file was modified by this session |
| D3 | E0 | **The engine publish failed**: `Already closed: … code=404, text='NOT_FOUND - no exchange 'order-success-queue-e2e' in vhost '/'', classId=40, methodId=10`. `PublishRabbitMQActivity` **cannot create broker topology** — a failed passive declare closes the channel and it reissues the active declare on that dead channel (**F16**/**S11**). The plan's §0.6 assumption that the publisher would create exchange+queue+binding was **wrong** | Topology created out-of-band with a one-off AMQP script (operator-selected): 2 direct non-durable exchanges, 2 non-durable queues, 2 bindings with routingKey `''`. Chosen non-durable deliberately, to preserve the §0.6 test. Added to the rollback doc §7 |
| D4 | E0 | The AMQP script first failed with `BrokerUnreachableException: None of the specified endpoints were reachable`, which looked like a stale ngrok tunnel | Misleading error. Root cause was two frames down: `FileNotFoundException: System.Threading.RateLimiting, Version=8.0.0.0`. RabbitMQ.Client 7.1.2 needs it, and it ships in the **`Microsoft.AspNetCore.App` shared framework** — correctly **absent** from the worker publish (the container's `aspnet:8.0` base image supplies it), but also absent from PowerShell's base runtime. Fixed by loading it from `…\Microsoft.AspNetCore.App\8.0.29\`. **Not a publish defect.** A raw AMQP probe had already confirmed the broker healthy (`connection.start` frame returned) |

---

## Part 4 — Change-synchronization items

Raised by this analysis; **not yet applied** — each needs approval, and any that touches script
behaviour needs its `Tests/` reconciled first.

| # | Artefact | Change | Reason |
|---|---|---|---|
| S1 | [Deploy-E2E-Verification-Runbook.md](Deploy-E2E-Verification-Runbook.md) §5 **E2** | Replace the 5-iteration `?QueueName=$queue` loop with one call `?queue=<q>&total=5` | **F5** — `RabbitPublish`'s DataList inputs are `queue` and `total`; `QueueName` binds to nothing, so the documented command publishes to an empty queue name |
| S2 | Same runbook, §0/§3 | Add a note that a queue created by `PublishRabbitMQActivity` is **non-durable**, and that a `Durable=true` trigger still consumes it because the pump never declares | **F2/F3** — the §7 `PRECONDITION_FAILED` row implies the opposite and misdirects triage |
| S3 | Same runbook, §6 Phase F | Note that when an engine is deployed twice under one name, only the **first** summary carries `created.*=true`; rollback must use that one | **F11** — the newest summary looks authoritative and silently under-deletes |
| S4 | Same runbook, §2 route table | Record that a **Stopped** Function App returns `403 Site Disabled` on every route | Observed on `wwengine1`; reads as an auth failure and is not in the triage table |
| S5 | [Scripts/README.md](../Scripts/README.md) | Cross-link the three documents produced here | Keeps the script-set overview complete |
| S6 | Same runbook, §2 binding table + the `multipart` warning box | `multipart/form-data` is now **supported**, and the "worker refuses to start" box no longer describes the code. Also add the two newly-bound forms: raw XML/JSON **query string** | **F13/F14** — the table now contradicts the code in this working tree |
| S7 | [ResolvedQueueConfiguration.cs](../../Warewolf.Execution.QueueProcessor/Configuration/ResolvedQueueConfiguration.cs) | Consider a **runtime** engine-capability check to replace the removed compile-time guard | **F14** — the `DEPLOYMENT DEPENDENCY` comment documents the risk but nothing detects it; a worker against an older engine dead-letters every message silently, which is the exact failure the guard existed to prevent |
| **S11** ⚠️ | [PublishRabbitMQActivity.cs:199-220](../../Dev2.Activities/Activities/RabbitMQ/Publish/PublishRabbitMQActivity.cs#L199-L220) | **Defect — the RabbitMQ Publish tool can never create a queue or exchange.** `ExchangeDeclarePassive` on a missing exchange returns 404, which **closes the channel**; the `catch` then calls `ExchangeDeclare` on that same dead channel and throws `AlreadyClosedException`, so nothing is created and the publish fails. The `QueueDeclarePassive`/`QueueDeclare` pair below it has the identical flaw. Fix by declaring on a **fresh channel**, exactly as [RabbitMqDeadLetterPublisher.cs:139-148](../../Warewolf.Execution.QueueProcessor/Messaging/RabbitMqDeadLetterPublisher.cs#L139-L148) already does — that file's comment names this activity as the broken reference: *"PublishRabbitMQActivity reuses the closed one and would throw AlreadyClosedException instead of creating the queue; do not repeat that."* | **Reproduced live 2026-08-06** on `order-success-queue-e2e`. Affects the full Server and the engine equally — any first publish to a new queue fails. Masked in normal use because queues are usually pre-created by the Studio or an earlier consumer. This is shared activity code, so it needs a plan + unit-test plan agreed before any change |
| S9 | [Deploy-E2E-Verification-Runbook.md](Deploy-E2E-Verification-Runbook.md) §2, step 3 | `az account get-access-token --resource "api://$EngineAppId"` **fails on a freshly created app registration** with `AADSTS65001 consent_required` — the Azure CLI client `04b07795-8ddb-461a-bbee-02f9e1bf7b46` is not in the new app's `preAuthorizedApplications`. It works on `wwengine1-auth` only because consent was granted there previously. Document the working alternatives: client-credentials with the app's own Easy Auth secret (used here), pre-authorizing the CLI client, or an interactive `az login --scope api://<appId>/.default` | Measured 2026-08-06 on `wwengine2-auth`. As written, the runbook's verification step cannot succeed on any first-time deployment |
| S12 | [Deploy-E2E-Verification-Runbook.md](Deploy-E2E-Verification-Runbook.md) §3 (log-retrieval note) | The App Insights `dependencies` query works and is good evidence of engine result codes, but the worker's own `Dev2Logger` lines (`Queue execution succeeded`, `durationMs=`, `Drain of …`) do **not** appear in App Insights `traces`. Document `ContainerAppConsoleLogs_CL` in Log Analytics as the source for execution counts, `durationMs` and drain lines — it also spans **all** replicas, which a single-replica `az containerapp logs show` cannot | Measured: the `traces` query returned 0 rows while `ContainerAppConsoleLogs_CL` returned all 42 executions. The runbook currently implies App Insights is sufficient |
| S13 | [Deploy-E2E-Verification-Runbook.md](Deploy-E2E-Verification-Runbook.md) §5 E7 | Note that a forced restart transiently exceeds `maxReplicas` (observed **5** against `max=3`) because ACA starts replacements while old replicas drain — otherwise it reads as a scale-rule violation. Also note that with `prefetch=1` and a sub-second workflow, E7 will usually report `draining 0 in-flight message(s)`, i.e. it does **not** exercise the grace budget | Observed 2026-08-06 |
| S10 | [Deploy-WwQueueProcessor.ps1](../Scripts/Deploy-WwQueueProcessor.ps1) Phase 1 | Ensure the resource group **conditionally** (check-then-create), matching `Deploy-WwExecutionEngine.ps1`, which prints `Resource group 'DEV2' already exists.` and skips. Low priority | The worker script issues `az group create` unconditionally against what may be a **shared** group. Verified a no-op for `DEV2` (`tags: null`, same location), but the engine script's check-first pattern is the safer contract and the asymmetry is surprising |
| S8 | [Warewolf.Execution.Lightweight.Tests](../../Warewolf.Execution.Lightweight.Tests/) | **Unit tests needed** for `ParseMultipartAsync` (untyped part → text, typed part → Base64, malformed body → binds nothing, reserved names skipped) and `TryGetRawQueryPayload` (valid XML/JSON → payload; `?a=<b` and `?a={b` → **fall through**, not captured) | **F15** — both paths are untested, and the fall-through cases are precisely where a cheap first-character sniff would have been wrong |

*Nothing in `Dev/` was modified by this work — the three documents are the only files added. **S1–S6**
are documentation-only and need no test changes. **S7** is a behavioural change and **S8** is a test
addition; per the repo's test rule, both need a plan presented and approved before any code or test
is written. The engine and worker source changes carried into this deployment (**F13/F14**) were
authored outside this session and are deployed at the operator's explicit direction.*

---

## Part 5 — Sign-off

| | |
|---|---|
| Analysis | ✅ complete — 2026-08-06 |
| Deployment executed | ✅ **2026-08-06** — engine `wwengine2` (run `wwx-20260806-190737`) + 2 Container Apps `wwqp2-*` |
| Verification passed | ✅ **all 18 criteria**, with the two qualifications in §2.7 |
| Teardown completed | ⬜ — deployment left standing; see [Deploy-E2E-Rollback-Commands.md](Deploy-E2E-Rollback-Commands.md) |
| Shared resources confirmed intact | ✅ mid-run (§2.7a); **re-confirm after teardown** via rollback doc §9 |
| Operator sign-off | ⬜ |

### Headline result

The Azure queue path works end to end. A message published through the **Execution Engine** reaches
RabbitMQ, **KEDA scales the QueueProcessor from 0 → N** on queue depth alone, each replica executes its
message against `/secure/*` on the engine, the queue drains, and the apps **return to 0**. Measured
`T = 897 ms`; peak replicas matched `min(ceil(messages / value), maxReplicas)` exactly; **42
executions, 42 successes, 0 failures, 0 dead-letters**, and no dead-letter queue was ever created.

### What the run found that the runbook did not predict

| | Impact |
|---|---|
| **S11** — `PublishRabbitMQActivity` cannot create a queue or exchange (**defect**, reproduced live) | Blocked the run until topology was created out-of-band. Affects the full Server too |
| **S9** — `az account get-access-token` cannot work against a fresh app registration | The runbook's `/Secure` verification step is unrunnable as written on any first deployment |
| **E3b** — `QueueLength`/`protocol=amqp` counts **ready only** | Previously unmeasured. Decides how tight `-ShutdownGraceSeconds` may safely be |
| **S12** — worker `Dev2Logger` traces never reach App Insights `traces` | `durationMs` and execution counts must come from Log Analytics, not App Insights |
| **F13–F15** — engine deployed **ahead of `HEAD`** with untested multipart/raw-query paths | Deployed at operator direction; tests still owed (**S8**) |

---

## Part 6 — 2026-08-11 reliability run (`wwqp3-`)

| | |
|---|---|
| Purpose | Container-replica processing at burst volume, with per-message timing and replica attribution |
| Engine | `wwengine-e2e-th2teq` (reused, redeployed in place) |
| Workers | `wwqp3-ordersuccessqueue` (max 3), `wwqp3-orderfailurequeue` (max 1) |
| Workload | 30 valid + 5 empty per queue, ×2 queues = **70 messages**, each with a unique AMQP `CorrelationId` |
| Report | `Get-WwQueueRunReport.ps1` — see [E2E-Harness-README.md](E2E-Harness-README.md) |

### First run — 2 of 70 defects, both invisible to the 18 criteria

| | Before fix | After fix |
|---|---|---|
| Processed | 38 / 70 | **70 / 70** |
| Succeeded | 16 | **60** |
| Dead-lettered | 21 (**16 wrongly**) | **10** (exactly the intended) |
| Missing / stalled | **34** | **0** |
| Outcome mismatches | 16 | **0** |

### Defect A — the engine could not execute one workflow concurrently

Valid messages returned HTTP 500 with `Object reference not set to an instance of an object.` and
`Error with variables in input. [[JobLogId]]`. Isolated from the queue path entirely with direct HTTP:

| Concurrency | Result |
|---|---|
| sequential ×10 | 10/10 → 200 |
| 3 | 3/3 → 200 |
| 4 / 6 / 10 | 3/4, 4/6, 8/10 |
| **SQL-free workflow at 20** | **20/20 → 200** |

**Root cause.** `WorkflowExecutor` cached one `DynamicActivity` per file path and handed it to every
execution at once, on the stated reasoning that "all runtime state flows through `DsfDataObject`". That
is false: `ActivityParser.Parse` clones nothing — it walks the cached `Flowchart` and returns references
to the **same** `Dsf*Activity` objects — and all six database activities hold per-execution state in an
instance field (`ServiceExecution = new DatabaseServiceExecution(dataObject)` in `BeforeExecutionStart`,
read back in `ExecutionImpl`). Concurrent executions overwrite each other, and the loser runs against the
winner's data object, leaving its own output variable unwritten. An Assign-only workflow is unaffected
because it has no such state — which is why every prior single-message test passed.

**Fix (Lightweight only, by decision).** Each execution now *rents* a prepared workflow and returns it in
a `finally`; the pool grows to peak concurrency and no further, so both the XAML compile and the parse
stay amortised. Applied at **three** call sites: `WorkflowExecutor`, `ResumptionExecutor` and
`LightweightEsbChannel` (nested sub-workflows — the most exposed, since one sub-workflow usually has
several parents).

> **Still latent for the on-prem Server.** `Dev2.Activities` was deliberately not changed, so
> `DsfDatabaseActivity`, `DsfMySqlDatabaseActivity`, `DsfODBCDatabaseActivity`,
> `DsfOracleDatabaseActivity`, `DsfPostgreSqlActivity` and `DsfSqlServerDatabaseActivity` all still hold
> `ServiceExecution` in an instance field, and `ResourceActivityCache` shares parsed chains the same way.
> Warrants its own ticket.

### Defect B — `Prefetch=1` plus a failed delivery deadlocked the consumer

One engine call exceeded the 45 s timeout; the pump left the delivery unacked expecting redelivery "when
the channel drops". The channel does not drop, and with `Prefetch=1` the broker sends nothing further —
**34 messages stranded for 20+ minutes** with a healthy replica and an attached consumer. KEDA cannot
recover it: a non-empty queue keeps the replica alive and nothing forces the restart that would requeue.

**Fix.** Every delivery now ends acked, nacked or dead-lettered. Escaped exceptions took the same
abandoning path and route through the same policy. `-MaxDeliveryAttempts` (default 2) is clamped to 1–2
because the AMQP `redelivered` flag is a boolean.

### Timeout chain re-sized

45/60/90 → **180/210/240**, against the engine's `functionTimeout` of 600. Measured, not guessed: max
11.5 s at the deployed concurrency of 4; 153 s under a 2.5× overload. Separately, `-TerminationGracePeriodSeconds`
was found to be **validated and printed but never applied** — a template property that `--set-env-vars`
cannot carry — so every earlier deployment silently ran on ACA's 30 s default.

### Verification

- `Warewolf.Execution.Lightweight.Tests` **679 passed**, `Warewolf.Execution.QueueProcessor.Tests` **94 passed**.
- New tests proven non-vacuous by temporarily reintroducing each defect: **4/8** pool tests and **11/11**
  pump tests failed, then passed once restored.
- Live: `JobLogId`-unbound = 0 at concurrency 4/6/10; both queues drained; `order-failure-queue-errors`
  created on demand; 70/70 reconciled with 0 missing and 0 mismatches.

### What this run says about the 18 criteria

Every one of them passed on 2026-08-06 while Defect A was already present. The criteria score
**provisioning, scaling and drainage** — none of which can distinguish a message that succeeded from one
that was dead-lettered and acked, because both drain the queue and both scale back to zero. Reliability
needs per-message reconciliation against what was actually published, which is what
`Get-WwQueueRunReport.ps1 -ExpectedManifest` now provides.
