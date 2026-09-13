# Rollback Commands — E2E run `wwengine2` / `wwqp2-` (isolated parallel run)

Phase F for [Deploy-E2E-Execution-StepByStep.md](Deploy-E2E-Execution-StepByStep.md). Removes
**only** what that run created, leaving the shared platform and the **2026-08-05 deployment** intact.

> ## ⛔ Six commands that must never be run against this resource group
>
> | Forbidden | Why |
> |---|---|
> | `az group delete --name DEV2` | `DEV2` is shared: AKS `tudev2-kubernetes`, `WarewolfServer`, `WarewolfServer-UAT`, `WarewolfServerExecution`, Key Vault `WWExecutionEngine`, ACA env `dev2-cae`, ACR `tudev2containerregistry` |
> | `az containerapp env delete --name dev2-cae` | Shared — hosts `sharepoint-wiremock`, `exchange-wiremock` **and the 2026-08-05 `wwqp-*` apps** |
> | `az acr delete --name tudev2containerregistry` | Holds the AKS ingress images and every connector-test image |
> | `az acr repository delete --repository warewolf` | A **different, pre-existing, shared** repository |
> | `az acr repository delete --repository warewolf/queueprocessor` | The **2026-08-05 run's** image — this run uses `warewolf/queueprocessor-e2e` |
> | `az keyvault secret delete --name WWExecutionEngineTestSecret` | The AES key other deployments depend on |
>
> Also never delete `workspace-2028k` (backs `dev2-cae`; this run's rows age out on the 30-day
> retention) or the `rabbitmq-uri` secret (created 2026-08-04 by an earlier run, **reused** here).

---

## 0. Prerequisites

Re-establish the session variables from §1 of the step-by-step, then hydrate this run's identifiers.
**If the shell was closed, recover them from the summaries rather than retyping:**

```powershell
$Rg        = 'DEV2'
$LogDir    = 'G:\Deployment\logs\e2e-wwengine2'
$EngineApp = 'wwengine2'
$EngineAi  = 'wwengine2-ai'
$QpPrefix  = 'wwqp2-'
$ImageRepo = 'warewolf/queueprocessor-e2e'
$Kv        = 'WWExecutionEngine'

# Authoritative engine summary - the run that CREATED the resources (created.functionApp = true)
$Summary = (Get-ChildItem $LogDir -Filter 'deploy-WwExecutionEngine-*.summary.json' |
            Where-Object Name -notmatch 'dryrun' | Sort-Object LastWriteTime | Select-Object -Last 1).FullName
$s = Get-Content $Summary -Raw | ConvertFrom-Json
$s | Select-Object runId, appName, storageAccount, appInsightsName -ExpandProperty created |
     Format-List
$EngineAppId = (az ad app list --display-name 'wwengine2-auth' --query "[0].appId" -o tsv)
```

> ⚠️ **Pick the summary whose `created` map says `true`.** If the engine was deployed more than once
> under the same name, only the **first** run's summary records
> `storageAccount / functionApp / appInsights = true`; a later run's map is all `false` except
> `entraApp`, and pointing rollback at it would delete only the Entra app while leaving the Function
> App, storage and App Insights behind. Verify before running step 2:
> ```powershell
> Get-ChildItem $LogDir -Filter 'deploy-WwExecutionEngine-*.summary.json' |
>   Where-Object Name -notmatch 'dryrun' | ForEach-Object {
>     $j = Get-Content $_.FullName -Raw | ConvertFrom-Json
>     [pscustomobject]@{ File=$_.Name; runId=$j.runId
>                        fnApp=$j.created.functionApp; storage=$j.created.storageAccount
>                        ai=$j.created.appInsights; entra=$j.created.entraApp }
>   } | Format-Table -AutoSize
> ```

---

## 1. Container Apps created by this run (one per trigger)

```powershell
# PREVIEW - must list EXACTLY wwqp2-ordersuccessqueue and wwqp2-orderfailurequeue
az containerapp list -g $Rg --query "[?starts_with(name,'$QpPrefix')].name" -o tsv

# Capture the managed-identity principalIds BEFORE deleting (step 6 cleans their role assignments)
$QpPrincipals = @(az containerapp list -g $Rg --query "[?starts_with(name,'$QpPrefix')].identity.principalId" -o tsv)
$QpPrincipals

# 🔴 DELETE
az containerapp list -g $Rg --query "[?starts_with(name,'$QpPrefix')].name" -o tsv |
    ForEach-Object { az containerapp delete --name $_ --resource-group $Rg --yes }
```

**Expected:** two deletions. The `wwqp-` (no `2`) apps must remain.

⛔ The prefix filter is `wwqp2-`, **not** `wwqp`. `wwqp` would also match the 2026-08-05 apps.

---

## 2. Engine, storage and App Insights — summary-driven

`Rollback-WwExecutionEngine.ps1` deletes only resources whose `wwx-test-run` tag matches this run
**and** whose `created` flag is `true`, in dependency order, then runs a leak check. Pre-existing
resources are preserved by design.

```powershell
cd Dev\Warewolf.Execution.Lightweight\Scripts

# 🟢 REVIEW FIRST - prints every delete without executing it
.\Rollback-WwExecutionEngine.ps1 -SummaryPath $Summary -DryRun

# 🔴 EXECUTE
.\Rollback-WwExecutionEngine.ps1 -SummaryPath $Summary
```

**Expected to be deleted:** Easy Auth disabled + its 4 app settings removed, Entra app
`wwengine2-auth` (cascading its SP, role assignments and Easy Auth client secret), `wwengine2-ai`,
`wwengine2`, `stwwengine2`.

**Expected to be preserved:** `DEV2` (`created.resourceGroup = false`), `WWExecutionEngine`
(`created.keyVault = false`) and every secret in it.

> Do **not** pass `-DeleteResourceGroup`. It is honoured only when the run created the group — but
> never rely on that guard for a group this shared.

---

## 3. The image repository created by this run

```powershell
# CONFIRM the exact repo name first
az acr repository list --name tudev2containerregistry -o tsv

# 🔴 DELETE - note the -e2e suffix
az acr repository delete --name tudev2containerregistry --repository $ImageRepo --yes
```

⛔ **Not** `--repository warewolf` and **not** `--repository warewolf/queueprocessor`. Those are the
shared repo and the 2026-08-05 run's image.

---

## 4. App Insights (only if step 2 left it behind)

Step 2 normally removes it. Its `managed-wwengine2-ai-ws` workspace is a hidden resource owned by the
component and is removed with it — no separate delete.

```powershell
az monitor app-insights component show --app $EngineAi -g $Rg --query name -o tsv 2>$null
az monitor app-insights component delete --app $EngineAi -g $Rg 2>$null      # 🔴 only if present
```

---

## 5. Entra directory objects (outside the resource group)

Step 2 deletes the Entra app when the summary records `created.entraApp = true`. Verify, and clean up
only if it survived:

```powershell
az ad app list --display-name 'wwengine2-auth' --query "[].{name:displayName,appId:appId}" -o table
# If still present:
az ad app delete --id $EngineAppId          # 🔴 cascades SP, app-role assignments and secrets
```

⛔ **Never target `wwengine1-auth`** (appId `ab0810f7-8caa-46cf-b3d9-c5dd6a1429b4`) — that belongs to
the 2026-08-05 run.

---

## 6. Orphaned role assignments from the Container App identities

Deleting a Container App removes its managed identity, which leaves its assignments orphaned rather
than dangerous — but clean them so the vault's and registry's access lists stay readable.

```powershell
$kvScope  = (az keyvault show --name $Kv --query id -o tsv)
$acrScope = (az acr show --name tudev2containerregistry -g $Rg --query id -o tsv)

foreach ($p in $QpPrincipals) {
    az role assignment delete --assignee-object-id $p --scope $kvScope  2>$null   # 🔴 Key Vault Secrets User
    az role assignment delete --assignee-object-id $p --scope $acrScope 2>$null   # 🔴 AcrPull
}

# Anything left that no longer resolves to a live identity:
az role assignment list --scope $kvScope --query "[?principalType=='ServicePrincipal'].{principal:principalId,role:roleDefinitionName}" -o table
```

If `$QpPrincipals` was not captured in step 1, recover the ids from
`$LogDir\deploy-WwQueueProcessor-<stamp>.summary.json` (`queueProcessorApps[].principalId`).

---

## 7. RabbitMQ queues created by this run (broker-side, optional)

E0/E2 created four queues on the dev broker via `PublishRabbitMQActivity` (non-durable):
`order-success-queue-e2e`, `order-failure-queue-e2e`, and their `-errors` dead-letter queues.

**Actually created on this run** (broker: RabbitMQ 3.13.7 at the `rabbitmq-uri` endpoint):

| Object | Name | Durable | Created by |
|---|---|---|---|
| exchange (direct) | `order-success-queue-e2e` | no | the one-off AMQP setup script (**D3**) |
| exchange (direct) | `order-failure-queue-e2e` | no | same |
| queue | `order-success-queue-e2e` | no | same |
| queue | `order-failure-queue-e2e` | no | same |
| binding | each queue → same-named exchange, routingKey `''` | — | same |
| DLQ | `order-success-queue-e2e-errors` / `-failure-…` | **yes** | only if a message was dead-lettered |

> These had to be created out-of-band because `PublishRabbitMQActivity` **cannot** create topology
> (defect **S11**). Do not expect a future run to self-provision them.

The exchanges and work queues are **non-durable**, so a broker restart removes them by itself. The
DLQs are **durable** and will survive. Delete explicitly for a clean slate — management UI, or the
same AMQP approach used to create them:

```powershell
# Load the client the same way the setup script did (RateLimiting comes from the ASPNET shared framework)
$pub='G:\Deployment\apps\QueueProcessor2'
$aspnet=(Get-ChildItem 'C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App' -Directory |
         Where-Object Name -like '8.*' | Sort-Object Name | Select-Object -Last 1).FullName
$probe=@($pub,$aspnet)
[System.Runtime.Loader.AssemblyLoadContext]::Default.add_Resolving({ param($ctx,$name)
  foreach ($d in $probe) { $p=Join-Path $d "$($name.Name).dll"; if (Test-Path $p) { return $ctx.LoadFromAssemblyPath($p) } }; $null })
Add-Type -Path "$aspnet\System.Threading.RateLimiting.dll"; Add-Type -Path "$pub\RabbitMQ.Client.dll"

$ct=[System.Threading.CancellationToken]::None
$uri=[Uri](az keyvault secret show --vault-name WWExecutionEngine --name rabbitmq-uri --query value -o tsv)
$ui=$uri.UserInfo.Split(':',2)
$f=[RabbitMQ.Client.ConnectionFactory]::new()
$f.HostName=$uri.Host; $f.Port=$uri.Port; $f.UserName=$ui[0]; $f.Password=$ui[1]; $f.VirtualHost='/'
$c=$f.CreateConnectionAsync('wwqp-e2e-teardown',$ct).GetAwaiter().GetResult()
$ch=$c.CreateChannelAsync([RabbitMQ.Client.CreateChannelOptions]::Default,$ct).GetAwaiter().GetResult()
# NOTE the arities - both differ from the obvious guess, and a wrong one fails at teardown time:
#   QueueDeleteAsync(queue, ifUnused, ifEmpty, noWait, ct)   -> FIVE arguments
#   ExchangeDeleteAsync(exchange, ifUnused, noWait, ct)      -> FOUR arguments
# There is also no single-argument IChannel.CloseAsync(ct); dispose the connection instead.
foreach ($q in @('order-success-queue-e2e','order-failure-queue-e2e',
                 'order-success-queue-e2e-errors','order-failure-queue-e2e-errors')) {
    try { $ch.QueueDeleteAsync($q,$false,$false,$false,$ct).GetAwaiter().GetResult() | Out-Null; "deleted queue $q" } catch { "queue $q absent" }
}
foreach ($x in @('order-success-queue-e2e','order-failure-queue-e2e')) {
    try { $ch.ExchangeDeleteAsync($x,$false,$false,$ct).GetAwaiter().GetResult() | Out-Null; "deleted exchange $x" } catch { "exchange $x absent" }
}
$c.Dispose()
```

> Or use the harness helper, which encapsulates both arities:
> ```powershell
> Import-Module .\WwE2E.Common.psm1
> Initialize-E2ERabbitClient -PublishPath 'G:\Deployment\apps\QueueProcessor2'
> $s = New-E2EBrokerSession -AmqpUri (az keyvault secret show --vault-name WWExecutionEngine --name rabbitmq-uri --query value -o tsv)
> Remove-E2EQueueTopology -Session $s -QueueName 'order-success-queue-e2e' -DeadLetterQueue 'order-success-queue-e2e-errors'
> Remove-E2EQueueTopology -Session $s -QueueName 'order-failure-queue-e2e' -DeadLetterQueue 'order-failure-queue-e2e-errors'
> Close-E2EBrokerSession -Session $s
> ```

⛔ **Never delete `order-success-queue` / `order-failure-queue` (no `-e2e`)** — those belong to the
2026-08-05 run.

---

## 8. Local artefacts (🟡 optional, non-Azure)

```powershell
Remove-Item 'G:\Deployment\triggers-e2e' -Recurse -Force      # this run's isolated trigger copies
# Keep $LogDir - the summaries are the audit trail for this run.
```

⛔ Do **not** delete `G:\Deployment\triggers`, `G:\Deployment\sources`, `G:\Deployment\settings`, or
anything under `C:\ProgramData\Warewolf\Resources` — all are reused inputs, and none was modified by
this run.

---

## 9. Post-teardown verification

Everything below must hold. **Run all of it** — a partial teardown that looks clean is the failure
mode this section exists to catch.

```powershell
# a) Nothing of this run's remains
az resource list -g $Rg --query "[?starts_with(name,'wwqp2-') || name=='wwengine2' || name=='stwwengine2' || name=='wwengine2-ai'].name" -o tsv
#    ^ MUST be EMPTY

az ad app list --display-name 'wwengine2-auth' --query "length(@)" -o tsv        # 0
az acr repository list --name tudev2containerregistry -o tsv | Select-String 'queueprocessor-e2e'   # no match

# b) The 2026-08-05 run is intact
az containerapp list -g $Rg --query "[?starts_with(name,'wwqp-')].name" -o tsv
#    ^ MUST print wwqp-ordersuccessqueue and wwqp-orderfailurequeue
az functionapp show -n wwengine1 -g $Rg --query "{name:name,state:state}" -o json   # present (state Stopped)
az ad app list --display-name 'wwengine1-auth' --query "[0].appId" -o tsv          # ab0810f7-8caa-46cf-b3d9-c5dd6a1429b4
az acr repository list --name tudev2containerregistry -o tsv | Select-String '^warewolf'
#    ^ MUST still show warewolf AND warewolf/queueprocessor

# c) Shared platform intact
az group show --name $Rg --query properties.provisioningState -o tsv                                # Succeeded
az containerapp env show --name dev2-cae -g $Rg --query properties.provisioningState -o tsv         # Succeeded
az containerapp list -g $Rg --query "[?contains(properties.environmentId,'dev2-cae')].name" -o tsv
#    ^ MUST print exactly: sharepoint-wiremock, exchange-wiremock, wwqp-ordersuccessqueue, wwqp-orderfailurequeue
az keyvault secret show --vault-name $Kv --name WWExecutionEngineTestSecret --query attributes.enabled -o tsv  # true
az keyvault secret show --vault-name $Kv --name rabbitmq-uri --query attributes.enabled -o tsv                 # true
az acr show --name tudev2containerregistry -g $Rg --query provisioningState -o tsv                             # Succeeded
az monitor log-analytics workspace show -g $Rg -n workspace-2028k --query provisioningState -o tsv 2>$null      # Succeeded
az aks show -g $Rg -n tudev2-kubernetes --query provisioningState -o tsv 2>$null                                # Succeeded
az functionapp list -g $Rg --query "[?starts_with(name,'WarewolfServer')].name" -o tsv                          # all present
```

---

## 10. Partial-rollback recipes

Not every situation needs a full teardown.

| Situation | Do this |
|---|---|
| Phase C failed; engine is fine | Steps **1**, **3**, **6** only. Leave the engine and its Entra app in place and re-run `Deploy-WwQueueProcessor.ps1`. |
| Phase B failed part-way | Re-run `Deploy-WwExecutionEngine.ps1` — it is idempotent and repairs a half-configured app. Roll back only if you want a different name. |
| Just stop the cost, keep the deployment | `az functionapp stop -n wwengine2 -g DEV2`. Container Apps at `min 0` already cost nothing when the queues are empty. **This is what happened to `wwengine1` — a stopped app returns `403 Site Disabled`, which looks like an auth failure.** |
| Re-run Phase E only | Do nothing here. Publish again; the apps scale from 0. |
| Wrong scale values deployed | `az containerapp update -n <app> -g DEV2 --scale-rule-*` per §4.8 of the source runbook, or fix the trigger and re-run Phase C. No teardown needed. |

---

## 11. Full sequence (copy-paste order)

```
0.  Hydrate variables + pick the summary with created.functionApp = true
1.  🔴 az containerapp delete  x2   (wwqp2- prefix ONLY)
2.  🟢 Rollback-WwExecutionEngine.ps1 -DryRun    → review
2.  🔴 Rollback-WwExecutionEngine.ps1            → engine, storage, App Insights, Entra app
3.  🔴 az acr repository delete --repository warewolf/queueprocessor-e2e
4.  🔴 az monitor app-insights component delete  (only if step 2 left it)
5.  🔴 az ad app delete                          (only if step 2 left it)
6.  🔴 az role assignment delete  x4             (KV + ACR, per identity)
7.  ⚪ broker queue cleanup                       (optional; non-durable, dies with the broker)
8.  🟡 Remove-Item G:\Deployment\triggers-e2e     (optional)
9.  🟢 §9 verification - ALL of it
```

**Never** `az group delete` · **Never** `az containerapp env delete` · **Never** `az acr delete` ·
**Never** touch `wwengine1*`, `wwqp-*`, `warewolf/queueprocessor`, `WWExecutionEngineTestSecret`.
