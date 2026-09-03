# Queue load-test runbook — manual execution

Reproduces **RUN 2** (2026-08-13): 100 unique messages, 6 replicas, **100/100 succeeded, 0 dead-lettered**.

Values below are RUN 2's. Substitute your own suffix/app names.

| | RUN 1 (10 replicas) | RUN 2 (6 replicas) |
|---|---|---|
| Succeeded | 68 | **100** |
| Dead-lettered | 32 | **0** |
| Drain time | ~4 min | **~90 s** |

Fewer replicas processed everything *and* finished faster — the RUN 1 failures were pure waste.

---

## 0. Prerequisites

```powershell
az account show                       # confirm subscription before anything creates resources
Import-Module G:\Deployment\Scripts\WwE2E.Common.psm1 -Force
```

| Thing | RUN 2 value |
|---|---|
| Engine | `wwengine-e2e-ldi413` |
| Engine Entra appId | `e0029ee1-e9a1-42b4-98e5-6585954340f8` |
| Worker | `wwqp4-ordersuccessqueue` |
| Queue / DLQ | `order-success-queue` / `order-success-queue-errors` |
| Broker | LavinMQ on CloudAMQP, vhost `bmkzdabu` |
| Run folder | `G:\Deployment\logs\loadtest-ldi413` |

---

## 1. Clean the baseline

Both the queues **and** the database, or the reconciliation cannot distinguish this run's rows.

```powershell
Import-Module G:\Deployment\Scripts\WwE2E.Common.psm1 -Force
Initialize-E2ERabbitClient -PublishPath G:\Deployment\apps\QueueProcessor
$uri = Resolve-E2EBrokerUri -SourceBitePath G:\Deployment\sources\RabbitMQSourceAshley.bite

$s = New-E2EBrokerSession -AmqpUri $uri -ClientName 'purge'
try {
    foreach ($q in 'order-success-queue','order-success-queue-errors') {
        $d = Get-E2EQueueDepth -Session $s -QueueName $q
        if ($d.Exists) {
            $ch = New-E2EChannel -Connection $s.Connection -CancellationToken $s.Ct
            $n  = $ch.QueuePurgeAsync($q, $s.Ct).GetAwaiter().GetResult()
            "purged $q : $n"
        }
    }
} finally { Close-E2EBrokerSession -Session $s }
```

Database (`Microsoft.Data.SqlClient` needs native SNI and will not load standalone — use Windows
PowerShell 5.1, which has `System.Data.SqlClient` built in):

```powershell
powershell.exe -NoProfile -Command @'
  $c = New-Object System.Data.SqlClient.SqlConnection('<connection string>')
  $c.Open(); $cmd = $c.CreateCommand()
  $cmd.CommandText = 'TRUNCATE TABLE dbo.jobs1; TRUNCATE TABLE dbo.jobs2;'
  $cmd.ExecuteNonQuery(); $c.Close()
'@
```

---

## 2. Set the concurrency cap

**`maxReplicas` comes from the trigger's `Concurrency`** — it is not a deploy flag. Edit a *copy* of
the trigger, never `G:\Deployment\triggers`:

```powershell
$run = 'G:\Deployment\logs\loadtest-ldi413'
New-Item -ItemType Directory -Force -Path "$run\triggers" | Out-Null
Copy-Item G:\Deployment\triggers\03fb9052-7fe4-4e8b-ac18-53779b0ebcba.bite "$run\triggers\" -Force

$f = "$run\triggers\03fb9052-7fe4-4e8b-ac18-53779b0ebcba.bite"
(Get-Content $f -Raw) -replace '"Concurrency":\s*\d+','"Concurrency": 6' | Set-Content $f -NoNewline

(Get-Content $f -Raw | ConvertFrom-Json) |
    Select-Object Name, QueueName, WorkflowName, Concurrency, Prefetch, DeadLetterQueue
```

**Concurrent engine requests = `maxReplicas` × `WORKER__MAXCONCURRENCY`.** With `MaxConcurrency=1`
that is exactly the replica count, so `Concurrency=6` caps engine concurrency at 6.

Why 6 — measured directly against the engine:

| Concurrency | Result |
|---|---|
| 6 | **24/24** |
| 8 | **24/24**, max 4.7 s |
| 10 | 26/30 — `Insufficient memory to continue the execution of the program` |

Memory rose **396 MB → 712 MB** across 10 concurrent executions (~32 MB each); Consumption gives
~1.5 GB per instance. 6 leaves clear margin.

---

## 3. Deploy the worker

```powershell
& G:\Deployment\Scripts\Deploy-WwQueueProcessor.ps1 `
    -ResourceGroup DEV2 -Location southafricanorth `
    -AcaEnvironment dev2-cae -AcrName tudev2containerregistry `
    -PublishPath G:\Deployment\apps\QueueProcessor `
    -DockerfilePath F:\Projects\Live\Warewolf\7580\Dev\Warewolf.Execution.QueueProcessor\Dockerfile `
    -ImageRepository 'warewolf/queueprocessor-ldi413' `
    -AppNamePrefix 'wwqp4-' `
    -TriggerPath "$run\triggers" `
    -QueueSourcePath G:\Deployment\sources `
    -EngineBaseUrl 'https://wwengine-e2e-ldi413.azurewebsites.net' `
    -EngineResourceAppId 'e0029ee1-e9a1-42b4-98e5-6585954340f8' `
    -EngineTenantId 'ca0cc53b-9af4-4067-bcdf-be9c648450d1' `
    -KeyVaultName WWExecutionEngine -KeyVaultSecretName WWExecutionEngineTestSecret `
    -RabbitMqSecretUri 'https://wwexecutionengine.vault.azure.net/secrets/rabbitmq-uri' `
    -InlineRabbitMqSecret -EncryptStagedSettings -ExecutionLogLevel INFO `
    -MaxDeliveryAttempts 2 -RbacPropagationSeconds 0 `
    -LogDir $run -NonInteractive
```

> The broker source is **baked into the image at build time**. Changing brokers requires a rebuild —
> updating Key Vault alone leaves the old endpoint inside the container.

Verify on the **live app**, not the plan output:

```powershell
$a = az containerapp show -g DEV2 -n wwqp4-ordersuccessqueue -o json | ConvertFrom-Json
$a.properties.template.scale.maxReplicas                      # 6
$a.properties.template.terminationGracePeriodSeconds          # 240
$a.properties.template.scale.rules.custom.metadata            # queueName / value=1
$a.properties.template.containers[0].env |
    Where-Object name -match 'TIMEOUT|GRACE|DELIVERY|CONCURRENCY|RETRY'
```

Expected: `ENGINE__TIMEOUTSECONDS=180`, `WORKER__SHUTDOWNGRACESECONDS=210`,
`WORKER__MAXCONCURRENCY=1`, `WORKER__MAXDELIVERYATTEMPTS=2`,
`WORKER__RETRYENGINEINTERNALERRORS=false`.

---

## 4. Pre-warm — do not skip

```powershell
& G:\Deployment\Scripts\Invoke-WwEnginePreWarm.ps1 `
    -EngineAppName wwengine-e2e-ldi413 `
    -EngineAppId   e0029ee1-e9a1-42b4-98e5-6585954340f8 `
    -TargetConcurrency 6
```

RUN 2 observed: first call **64,757 ms**, settling to ~3,100 ms by the fifth; then 18/18 clean at
concurrency 6. Every 502/503/504 in RUN 1 came from that cold window.

Phase B **ramps** to `-TargetConcurrency` rather than opening at it — for 6 that is `2 → 3 → 6`.
The last round is always the full target, so `[+] WARM — the final round was clean at concurrency
6` still means clean at 6.

The script **always exits 0**: an engine that will not warm is reported, not fatal. Read the
verdict rather than the exit code — if it prints `[!] the final round still had failures`, do not
publish. Re-run it, or lower `-TargetConcurrency`. A `TIMEOUT` in the codes summary is a counted
failed call, not a crash.

Do not publish until the final round is clean. **The warm-up executes the real workflow and writes
rows** — that is what the watermark in step 5 is for.

---

## 5. Watermark, then publish

```powershell
# BEFORE publishing - everything above this id belongs to the run
$q = 'SELECT ISNULL(MAX(JobLogId),0) FROM dbo.jobs1;'

$mf = "$run\manifest-run2.json"
(Get-Date).ToUniversalTime().ToString('o') | Set-Content "$run\run2-start-utc.txt" -NoNewline

& "$run\Publish-Burst.ps1" -QueueName 'order-success-queue' `
    -SuccessCount 100 -FailureCount 0 -Label 'R2' -ManifestPath $mf
```

`Publish-Burst.ps1` stamps a **unique AMQP `CorrelationId`** per message. That id becomes `[Txn:…]`
on every worker log line and is the only handle on a failure message, whose body is empty. Verify:

```powershell
$all = Get-Content $mf -Raw | ConvertFrom-Json
@($all | ForEach-Object txn  | Select-Object -Unique).Count   # 100
@($all | ForEach-Object body | Select-Object -Unique).Count   # 100
```

---

## 6. Monitor

```powershell
$s = New-E2EBrokerSession -AmqpUri $uri -ClientName 'watch'
try {
    (Get-E2EQueueDepth -Session $s -QueueName 'order-success-queue').Messages
    (Get-E2EQueueDepth -Session $s -QueueName 'order-success-queue-errors').Messages
} finally { Close-E2EBrokerSession -Session $s }

@(az containerapp replica list -g DEV2 -n wwqp4-ordersuccessqueue -o json | ConvertFrom-Json).Count
```

> An empty queue does **not** mean finished — messages can still be unacked and in flight. Poll until
> the database row count stops rising, then report.

RUN 2: replicas 0→6 within ~30 s, queue empty at ~90 s, DLQ 0 throughout.

---

## 7. Reports

```powershell
& G:\Deployment\Scripts\Get-WwQueueRunReport.ps1 `
    -AppNamePrefix 'wwqp4-' `
    -StartUtc '2026-08-13T07:45:00Z' -EndUtc '2026-08-13T07:55:00Z' `
    -ExpectedManifest "$run\manifest-run2.json" `
    -OutputDir $run -RunLabel 'run2'
```

Database side — count only above the watermark:

```sql
SELECT Status, COUNT(*) FROM dbo.jobs1 WHERE JobLogId > @watermark GROUP BY Status;
SELECT COUNT(*) AS Reprocessed FROM dbo.jobs1
 WHERE JobLogId > @watermark AND TRY_CONVERT(int, AttemptNumber) > 1;
WITH t AS (SELECT TRY_CONVERT(datetime2(3),StartedAtUtc,127) s,
                  TRY_CONVERT(datetime2(3),ProcessingAtUtc,127) p,
                  TRY_CONVERT(datetime2(3),FinishedAtUtc,127) f
           FROM dbo.jobs1 WHERE JobLogId > @watermark)
SELECT AVG(DATEDIFF(ms,s,p)) AS StartToProcessing,
       AVG(DATEDIFF(ms,p,f)) AS ProcessingToFinished FROM t;
```

Cross-check by extracting the txn from each row and intersecting with the manifest and the DLQ:

```sql
SELECT JSON_VALUE(MessageContent,'$.txn') AS Txn, Status, DurationMs
FROM dbo.jobs1 WHERE JobLogId > @watermark;
```

Four buckets, which must sum to the published count:
`jobs1` only (**clean**) · DLQ only (**failed before SQL**) · **both** (engine finished, response
lost — replaying duplicates committed work) · neither (**never processed**).

---

## Traps

- **Never trust the exit code.** A throwing PowerShell script leaves the previous `az` exit code in
  place. Read the log.
- **Zero log rows ≠ nothing happened.** `Invoke-E2ELogAnalytics` passes `-AllowFail`, so a bad column
  returns an *empty result* rather than an error. The replica column is `ContainerGroupName_s`;
  `ReplicaName_s` does not exist.
- **A drained queue proves nothing.** 2xx→ack and non-2xx→dead-letter+ack both drain it.
- **`terminationGracePeriodSeconds` is a template property**, not an env var. Verify it on the app.
- **Ad-hoc tokens expire mid-run** and surface as a wall of 401s. The worker refreshes its own.
- **`@($null).Count` is 1**, not 0 — it silently inflates "no rules"/"no replicas" counts.
