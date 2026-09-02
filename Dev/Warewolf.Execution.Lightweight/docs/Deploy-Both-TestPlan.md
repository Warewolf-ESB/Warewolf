# Manual test plan — `Deploy-WwEngineAndQueueProcessor.ps1`

Step-by-step verification, run by hand in **PowerShell 7+**. Ten stages, ordered by blast radius:
stages 0–4 touch **nothing in Azure**, stage 5 is **read-only**, stages 6–8 **create resources**.

Do them in order. Each stage has an **Expected** and a **STOP if** — do not carry a failure forward.

**This plan assumes an operator machine**: no source checkout, no .NET SDK, no git. Scripts and
builds arrive as zips and run from any folder — `G:\Deployment` throughout. Stages marked
*(developer machine only)* need the repo and are optional; everything else runs from the zips.

```powershell
# Paste once per session. Every stage below assumes these.
$Root    = 'G:\Deployment'
$Scripts = "$Root\Scripts"
$Wrapper = "$Scripts\Deploy-WwEngineAndQueueProcessor.ps1"
Set-Location $Scripts
$PSVersionTable.PSVersion      # must be 7.x
```

---

## Stage 0 — Environment preflight

```powershell
# 0.1  Tooling  (dotnet / func / Docker are NOT needed — packages are pre-built,
#                and the container image is built server-side by az acr build)
$PSNativeCommandArgumentPassing = 'Standard'
$PSVersionTable.PSVersion                                    # 7.x — 5.1 will NOT run these
az version --query '"azure-cli"' -o tsv                      # any recent
az extension list --query "[].name" -o tsv                   # want: containerapp

# 0.2  Unblock the downloaded scripts (mark-of-the-web blocks them otherwise)
Get-ChildItem "$Scripts\*.ps1" | Unblock-File
Get-ExecutionPolicy -List
# If Restricted, for THIS SESSION only:
#   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process

# 0.3  The extraction is COMPLETE — the orchestrators resolve these as siblings at run time
'Deploy-WwEngineAndQueueProcessor.ps1','Deploy-WwExecutionEngine.ps1','Deploy-WwQueueProcessor.ps1',
'Configure-WwExecutionAuth.ps1','Setup-ApplicationInsights.ps1','Encrypt-Config.ps1',
'WwE2E.Common.psm1',
'Generate-WorkflowIndex.ps1','Rollback-WwExecutionEngine.ps1' |
    ForEach-Object { '{0,-6} {1}' -f (Test-Path (Join-Path $Scripts $_)), $_ }

# 0.4  The payload
Get-ChildItem "$Root\apps"                                   # two DIFFERENT build artefacts
Test-Path "$Root\build\Dockerfile"                           # NOT in the Scripts zip
Get-ChildItem "$Root\settings"                               # secure.config, authconfig, licence
(Get-ChildItem "$Root\triggers" -Filter *.bite).Count        # must be > 0 — one app per file

# 0.5  Signed in, and on the right subscription
az account show --query "{name:name, id:id, tenant:tenantId}" -o json

# 0.6  DATA-plane access to the vault (control-plane Owner is NOT enough)
$Kv = '<key vault name>'
$me = az ad signed-in-user show --query id -o tsv
az role assignment list --assignee $me --scope (az keyvault show --name $Kv --query id -o tsv) `
    --query "[].roleDefinitionName" -o tsv

# 0.7  The broker secret KEDA reads
az keyvault secret show --vault-name $Kv --name rabbitmq-uri --query "attributes.enabled" -o tsv
```

**Expected** — PS 7.x; `containerapp` present; **all nine files `True` in 0.3**; a Dockerfile and
at least one trigger in 0.4; a subscription in 0.5; `Key Vault Secrets Officer` or
`Key Vault Secrets User` in 0.6; `true` in 0.7.

**STOP if**

- **Anything in 0.3 is `False`.** `Generate-WorkflowIndex.ps1` and `Rollback-WwExecutionEngine.ps1`
  are warnings, and `WwE2E.Common.psm1` is needed only for the token step in stages 6/8; the other six are hard stops. The orchestrators resolve them as siblings *when
  they need them*, so a partial extraction otherwise fails **mid-deploy, after resources exist**.
  Re-extract the complete `Scripts` folder.
- **`$Root\build\Dockerfile` is missing.** It ships with the QueueProcessor **source**, not the
  Scripts zip. Without it the image build in the worker's Phase 2 has nothing to build.
- **0.6 lists only `Owner`/`Contributor`.** The dry run in stage 5 fails at the secret read, and a
  real deploy fails later and less clearly. Get the data-plane role first.

> If `containerapp` is missing, the QueueProcessor orchestrator installs it itself — but installing
> it now (`az extension add --name containerapp`) keeps stage 5 clean.

> **Both guards are enforced by the script too**, not just by this checklist — the sibling set at
> startup (before any `az` call) and the Dockerfile in step 2 (before the image build). 0.3/0.4 just
> let you fix the folder before you start.

---

## Stage 1 — Offline: parse, and the test suite if you have it

No Azure, no files changed.

```powershell
# 1.1  Every script parses under PS7 — catches a truncated or corrupted download
Get-ChildItem "$Scripts\*.ps1" | ForEach-Object {
    $e = $null
    [System.Management.Automation.Language.Parser]::ParseFile($_.FullName, [ref]$null, [ref]$e) | Out-Null
    if ($e) { "PARSE FAIL {0}: L{1} {2}" -f $_.Name, $e[0].Extent.StartLineNumber, $e[0].Message }
    else    { "ok  {0}" -f $_.Name }
}

# 1.2  The wrapper exposes the expected switches
Get-Command $Wrapper -Syntax
```

**Expected** — every file `ok`; the syntax line shows `-EncryptResources`, `-SkipEngine`,
`-SkipQueueProcessor`, `-NonInteractive`, `-DryRun`.

### 1.3 *(developer machine only)* — the Pester suites

The `Tests` folder is not part of an operator deployment. Where it is available:

```powershell
Import-Module Pester -RequiredVersion 5.7.1 -Force
Invoke-Pester -Path .\Tests\Deploy-WwEngineAndQueueProcessor.Tests.ps1 -Output Detailed   # 55 / 0
Invoke-Pester -Path .\Tests\Deploy-WwExecutionEngine.Tests.ps1 -Output None -PassThru |
    Select-Object PassedCount, FailedCount                                                # 87 / 0
Invoke-Pester -Path .\Tests\Deploy-WwQueueProcessor.Tests.ps1 -Output None -PassThru |
    Select-Object PassedCount, FailedCount                                                # 75 / 0
```

**STOP if** — any parse failure in 1.1, or any test failure in 1.3. Nothing downstream is meaningful.

> No Pester on the operator machine is fine — stage 2 exercises the same decision logic directly,
> and needs nothing but the script itself.

---

## Stage 2 — Offline: prove the capture logic against fixtures

This is the heart of the script — reading the engine's two output files and refusing bad ones.
Fake outputs let you exercise it without a deploy.

> Run this in a **throwaway pwsh window**: dot-sourcing sets `Set-StrictMode -Version Latest` in
> your session, which makes ordinary interactive typos throw.

```powershell
$fx = "$env:TEMP\wwfx"; New-Item -ItemType Directory -Force -Path $fx | Out-Null
. $Wrapper -LoadFunctionsOnly          # defines the helpers, runs no step

# A good pair of engine outputs
@{ status='completed'; lastPhase='Phase 5'; error=$null; dryRun=$false
   runId='wwx-20260827-101500'; appName='my-engine'
   endpoint='https://my-engine.azurewebsites.net'
   appInsightsName='my-engine-ai'; entraAppDisplayName='my-engine-auth'
} | ConvertTo-Json | Set-Content "$fx\deploy-WwExecutionEngine-20260827-101500.summary.json"

@{ FunctionAppName='my-engine'; EntraAppDisplayName='my-engine-auth'
   ClientId='11111111-2222-3333-4444-555555555555'
   SpObjectId='bbbbbbbb-0000-0000-0000-000000000000'
   Audience='api://11111111-2222-3333-4444-555555555555'
   AppRoles=@(@{ value='Warewolf_QueueProcessor'; id='dddddddd-0000-0000-0000-000000000000' })
} | ConvertTo-Json -Depth 5 | Set-Content "$fx\auth.json"

# 2.1  Happy path
Resolve-WwEngineHandover -LogDir $fx -AuthOutputPath "$fx\auth.json" `
    -ExpectedAppName 'my-engine' -ExpectedAuthAppName 'my-engine-auth' |
    Format-List RunId, Endpoint, ClientId, SpObjectId, QueueProcessorRoleId

# 2.2  Wrong app — a shared LogDir between environments
try { Resolve-WwEngineHandover -LogDir $fx -AuthOutputPath "$fx\auth.json" -ExpectedAppName 'other' }
catch { "CAUGHT: $($_.Exception.Message)" }

# 2.3  An aborted engine run
$s = Get-Content "$fx\deploy-WwExecutionEngine-20260827-101500.summary.json" -Raw | ConvertFrom-Json
$s.status = 'failed'; $s.lastPhase = 'Phase 4'; $s.error = 'zip deploy failed'
$s | ConvertTo-Json | Set-Content "$fx\deploy-WwExecutionEngine-20260827-101500.summary.json"
try { Resolve-WwEngineHandover -LogDir $fx -AuthOutputPath "$fx\auth.json" -ExpectedAppName 'my-engine' }
catch { "CAUGHT: $($_.Exception.Message)" }
$s.status = 'completed'; $s.error = $null
$s | ConvertTo-Json | Set-Content "$fx\deploy-WwExecutionEngine-20260827-101500.summary.json"

# 2.4  THE ONE THAT MATTERS: an auth output left behind by a PREVIOUS deploy.
#      That file is a single fixed path in Scripts\, overwritten by every engine run.
$a = Get-Content "$fx\auth.json" -Raw | ConvertFrom-Json
$a.FunctionAppName = 'a-different-engine'
$a | ConvertTo-Json -Depth 5 | Set-Content "$fx\auth.json"
try { Resolve-WwEngineHandover -LogDir $fx -AuthOutputPath "$fx\auth.json" -ExpectedAppName 'my-engine' }
catch { "CAUGHT: $($_.Exception.Message)" }
```

**Expected**

| | |
|---|---|
| 2.1 | all five values printed, none empty |
| 2.2 | `CAUGHT: Summary is for app 'my-engine', expected 'other'.` |
| 2.3 | `CAUGHT: Engine deploy did not complete: status=failed lastPhase=Phase 4 error=zip deploy failed` |
| 2.4 | `CAUGHT: ...belongs to 'a-different-engine', not 'my-engine'...overwritten by every deploy` |

**STOP if** — any of 2.2–2.4 returns a value instead of throwing. That is the guard against wiring
a worker to the wrong engine. **Close this pwsh window when done** (StrictMode is still set).

---

## Stage 3 — Fill in the variable block

Open `Deploy-WwEngineAndQueueProcessor.ps1` and replace every `<angle bracket>` value near the top.

> **Back the edited script up.** On an operator machine the filled-in script *is* the deployment
> record for that environment. Deploying more than one environment from the same folder? Keep one
> copy each — but a copy must live **in the `Scripts` folder**, since the orchestrators are found
> relative to the running script:
> `Copy-Item $Wrapper "$Scripts\Deploy-WwEngineAndQueueProcessor.UAT.ps1"`, then point `$Wrapper` at it.
> *(On a developer machine, keep the filled-in values out of the commit.)*

| Variable | Value |
|---|---|
| `$Rg`, `$Loc` | existing resource group + region |
| `$Kv`, `$KvSecret` | vault, and the secret holding the **AES key** (not the broker uri) |
| `$Acr`, `$AcaEnv` | existing registry + Container Apps environment |
| `$Stage` | your deployment root — `G:\Deployment`; every other path in the block derives from it |
| `$EngineApp` | a **globally unique** Function App name |
| `$EngineStorage` | 3–24 lowercase chars |
| `$EnginePublish`, `$QpPublish` | the two publish outputs — **different csproj outputs, never the same folder** |
| `$QpPrefix`, `$ImageRepo` | e.g. `wwqp-`, `warewolf/queueprocessor` |

Leave `$Sub` / `$TenantId` empty to inherit from `az account show`. Do **not** fill in `$EngineAi`,
`$EngineAuthApp` or anything under *Resolved at runtime* — those are read back from the deploy.

The staging tree must already exist:

```powershell
$Stage = $Root                    # the variable block's $Stage — G:\Deployment
Get-ChildItem "$Stage\settings"   # secure.config, authconfig.json, Warewolf License.secureconfig
Get-ChildItem "$Stage\resources"  # workflow .bite files
Get-ChildItem "$Stage\sources"    # {sourceId}.bite for every QueueSourceId/QueueSinkId
Get-ChildItem "$Stage\triggers"   # queue trigger .bite files — ONE CONTAINER APP EACH
Test-Path     "$Stage\build\Dockerfile"
```

---

## Stage 4 — Guard rails (still no Azure)

Deliberately break things and confirm the script refuses. Each check aborts in **step 1 or 2**,
before any `az` call.

```powershell
# 4.1  Placeholder guard — temporarily blank one value, e.g. set $Rg back to '<resource group>'
.\Deploy-WwEngineAndQueueProcessor.ps1 -DryRun -NonInteractive
#     -> "Edit the variable block - 1 value(s) are still placeholders:  Rg = <resource group>"
#     Restore the real value before continuing.

# 4.2  Stray .bite guard — a .bite in the worker publish output is baked into the image
#      and competes with the staged trigger.
Set-Content "$QpPublish\oops.bite" '{}'
.\Deploy-WwEngineAndQueueProcessor.ps1 -DryRun -NonInteractive
#     -> "QueueProcessor publish output contains 1 .bite file(s); it must contain none."
Remove-Item "$QpPublish\oops.bite"

# 4.3  Swapped publish paths — point $QpPublish at the engine output, run, then restore.
#     -> "...does not contain Warewolf.Execution.QueueProcessor.dll - wrong publish output?"

# 4.4  Missing Dockerfile — the failure a source-tree default would have hidden until
#      the worker's Phase 2 image build.
Rename-Item "$Root\build\Dockerfile" 'Dockerfile.bak'
.\Deploy-WwEngineAndQueueProcessor.ps1 -DryRun -NonInteractive
#     -> "Dockerfile not found: G:\Deployment\build\Dockerfile. It ships with the
#         QueueProcessor source, not with the Scripts folder..."
Rename-Item "$Root\build\Dockerfile.bak" 'Dockerfile'

# 4.5  Partial extraction — the operator-machine failure mode. Prove the guard fires
#      BEFORE any az call, by running a copy from a folder holding only the three
#      Deploy-*.ps1 files.
$probe = "$env:TEMP\ww-partial"; New-Item -ItemType Directory -Force -Path $probe | Out-Null
Copy-Item "$Scripts\Deploy-WwEngineAndQueueProcessor.ps1",
          "$Scripts\Deploy-WwExecutionEngine.ps1",
          "$Scripts\Deploy-WwQueueProcessor.ps1" $probe -Force
& "$probe\Deploy-WwEngineAndQueueProcessor.ps1" -DryRun -NonInteractive
#     -> "3 required script(s) missing from '<probe>':
#           Configure-WwExecutionAuth.ps1 ... Setup-ApplicationInsights.ps1 ... Encrypt-Config.ps1 ..."
Remove-Item $probe -Recurse -Force
```

**Expected** — each run stops with the quoted message and **creates nothing**. 4.5 must stop
*before* `Step 1 - Preflight` prints — the check runs ahead of any `az` call, so it fires even with
an unedited variable block.

**STOP if** — any of these proceeds past step 2, or 4.5 reaches the preflight banner.

---

## Stage 5 — Dry run (read-only against Azure)

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -DryRun -NonInteractive
```

This *does* reach Azure, but only reads: `az account show`, the Key Vault secret check, the vault
uri. Both orchestrators then print their own plan phase and change nothing.

**Expected, in order**

```
═══ Step 1 - Preflight            [ok] Variable block filled in / Subscription … / Log directory …
═══ Step 2 - Publish paths        [ok] both .dll present, no stray .bite, N trigger file(s)
═══ Step 3 - Broker secret        [ok] Broker secret uri https://<vault>.vault.azure.net/secrets/rabbitmq-uri
═══ Deploy-WwExecutionEngine.ps1  ← the engine's Phase 0.5 PLAN, then "[DRYRUN] …" lines
═══ Step 5 - Capture              [warn] Dry run: using SYNTHETIC handover values …
                                  [ok]   Handover written to …\deploy-both-<stamp>.handover.json
═══ Deploy-WwQueueProcessor.ps1   ← the worker's Phase 0 plan, one app per trigger
═══ Done
```

Check the plan, not just the exit:

```powershell
$h = Get-Content "$Stage\logs\deploy-both\deploy-both-*.handover.json" -Raw | ConvertFrom-Json
$h.synthetic          # MUST be True here
```

**Expected** — the engine plan shows your app/storage/Key Vault; the worker plan lists **one app per
trigger** with `min=0`, `max=<trigger Concurrency>`; `synthetic` is `True`.

**STOP if** — the worker plan resolves **0 triggers**, or an app name collides with something already
in your ACA environment (`(az containerapp list -g $Rg -o json | ConvertFrom-Json).name`).

> `synthetic: true` is expected and correct here: a dry run writes no auth output, so the wrapper
> substitutes obvious `00000000-…` values purely so the worker's plan can be reached. Never use a
> dry-run handover file for the role grant.

---

## Stage 6 — 🔴 Real deploy, engine only

Split the first real run in two so you can inspect the handover before any worker exists.

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -SkipQueueProcessor -EncryptResources
```

`-EncryptResources` is **first run only** for a given set of sources; omit it on every later deploy.
You get a `y/N` gate showing the resolved parameter set — read it, then answer `y`.

**Verify the handover before going further:**

```powershell
$hp = (Get-ChildItem "$Stage\logs\deploy-both\deploy-both-*.handover.json" |
       Sort-Object LastWriteTime | Select-Object -Last 1).FullName
$h  = Get-Content $hp -Raw | ConvertFrom-Json
$h.synthetic                        # MUST be False now
$h.engine.endpoint                  # https://<your app>.azurewebsites.net
$h.entra.clientId                   # a real GUID, not 00000000-…
$h.entra.spObjectId                 # a real GUID
$h.entra.queueProcessorAppRoleId    # a real GUID (empty = authconfig lacks the role)
$h.keyVault.rabbitMqSecretUri
$h | ConvertTo-Json -Depth 8 | Select-String 'InstrumentationKey|amqp://'   # must find NOTHING
```

Smoke-test the engine:

```powershell
$url = $h.engine.endpoint
(Invoke-WebRequest "$url/Public/Hello%20World.json?Name=e2e" -SkipHttpErrorCheck).StatusCode   # 200
(Invoke-WebRequest "$url/Secure/Hello%20World.json?Name=e2e" -SkipHttpErrorCheck).StatusCode   # 401
# NOT az account get-access-token — AADSTS65001 on a fresh app registration. See RunGuide §6.0.
Import-Module "$Root\Scripts\WwE2E.Common.psm1" -Force
$tok = Get-E2EEngineToken -TenantId $h.tenantId -EngineAppId $h.entra.clientId `
                         -FunctionAppName $h.engine.appName -ResourceGroup $h.engine.resourceGroup
(Invoke-WebRequest "$url/Secure/Hello%20World.json?Name=e2e" `
    -Headers @{ Authorization = "Bearer $tok" } -SkipHttpErrorCheck).StatusCode                # 200
```

**STOP if** — `synthetic` is `True`, any Entra id is all-zero or empty, or the last call is not 200.
A worker deployed now would fail every engine call.

> If `queueProcessorAppRoleId` is empty, your `authconfig.json` doesn't define
> `Warewolf_QueueProcessor`. That only disables the optional grant in stage 8 — the deploy is still
> valid, because `secure.config` is what actually governs execution.

---

## Stage 7 — 🔴 Real deploy, workers only

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -SkipEngine
```

Step 5 re-reads and re-asserts the same two files rather than trusting stage 6's output — that is
the `-SkipEngine` path being exercised. Then confirm at the `y/N` gate.

```powershell
$h = Get-Content (Get-ChildItem "$Stage\logs\deploy-both\deploy-both-*.handover.json" |
     Sort-Object LastWriteTime | Select-Object -Last 1).FullName -Raw | ConvertFrom-Json
$h.queueProcessor.apps                                  # one per trigger

# NOTE: no --query. On Windows az is a .cmd shim and cmd.exe mangles JMESPath; quoting it makes az
# echo its own command line, which a previous run captured as data. Filter in PowerShell.
foreach ($app in $h.queueProcessor.apps) {
    $rev = (az containerapp revision list --name $app -g $h.engine.resourceGroup -o json |
            ConvertFrom-Json) | Select-Object -First 1
    [pscustomobject]@{ app = $app; rev = $rev.name
                       healthy = $rev.properties.healthState; active = $rev.properties.active }
}
```

**Expected** — one app per trigger file; newest revision `Healthy` + `True`; `minReplicas` 0.

**STOP if** — a revision crash-loops. Read `az containerapp logs show --name <app> -g $Rg --tail 50`
first: a `403 ForbiddenByRbac` on `getSecret` is Key Vault data-plane propagation, not a script bug —
the orchestrator's `-RbacPropagationSeconds` (default 60) covers it, but a slow tenant may need a
second run.

---

## Stage 8 — Post-deploy: the app-role grant

The point of the handover file — no Graph lookup, no display-name guessing:

```powershell
if ($h.synthetic) { throw 'Synthetic (dry-run) handover file.' }

foreach ($app in $h.queueProcessor.apps) {
    $mi = az containerapp show --name $app -g $h.engine.resourceGroup `
            --query identity.principalId -o tsv --only-show-errors
    if (-not $mi) { Write-Warning "$app has no managed identity; skipping"; continue }

    $body = New-TemporaryFile
    @{ principalId = $mi; resourceId = $h.entra.spObjectId; appRoleId = $h.entra.queueProcessorAppRoleId } |
        ConvertTo-Json -Compress | Set-Content $body -Encoding utf8NoBOM
    az rest --method POST `
      --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$mi/appRoleAssignments" `
      --headers "Content-Type=application/json" --body "@$body"
    Remove-Item $body
}
```

Then confirm each trigger's workflow is reachable — **this is the check that predicts whether the
workers succeed**:

```powershell
# NOT az account get-access-token — AADSTS65001 on a fresh app registration. See RunGuide §6.0.
Import-Module "$Root\Scripts\WwE2E.Common.psm1" -Force
$tok = Get-E2EEngineToken -TenantId $h.tenantId -EngineAppId $h.entra.clientId `
                         -FunctionAppName $h.engine.appName -ResourceGroup $h.engine.resourceGroup
foreach ($wf in (Get-ChildItem "$Stage\triggers" -Filter *.bite |
                 ForEach-Object { (Get-Content $_.FullName -Raw | ConvertFrom-Json).WorkflowName })) {
    $route = ($wf -replace '\\','/') -split '/' | ForEach-Object { [uri]::EscapeDataString($_) }
    $r = Invoke-WebRequest "$($h.engine.endpoint)/Secure/$($route -join '/').json" `
                           -Headers @{ Authorization = "Bearer $tok" } -SkipHttpErrorCheck
    "{0,-45} {1}" -f $wf, $r.StatusCode
}
```

**Expected** — 200 for every workflow.

**A 500 here is usually not a crash.** The engine surfaces authorization denials as 500, not 403
(WOLF-8418) — the usual cause is a missing per-workflow `Execute` row in `secure.config`.

---

## Stage 9 — Teardown

```powershell
foreach ($app in $h.queueProcessor.apps) {
    az containerapp delete --name $app -g $h.engine.resourceGroup --yes
}
.\Rollback-WwExecutionEngine.ps1 -SummaryPath $h.engine.summaryPath
```

Rollback targets the tag `wwx-test-run=<runId>` and removes **only what that run created**.

**Do not delete** the resource group, Key Vault, ACR or ACA environment — the script reuses them
and never creates them.

---

## Result sheet

| Stage | | Pass |
|---|---|---|
| 0 | Tooling, unblock, **complete extraction**, payload, login, KV data-plane role | ☐ |
| 1 | Every script parses (+ 55/87/75 tests on a developer machine) | ☐ |
| 2 | Capture accepts good outputs, refuses all four bad ones | ☐ |
| 3 | Variable block filled, deployment tree present | ☐ |
| 4 | Placeholder / stray-`.bite` / swapped-path / **missing-Dockerfile** / **partial-extraction** guards all abort | ☐ |
| 5 | Dry run reaches both plan phases; `synthetic: true` | ☐ |
| 6 | Engine deployed; handover has real ids; 200/401/200 | ☐ |
| 7 | One healthy Container App per trigger | ☐ |
| 8 | Roles granted; every trigger workflow returns 200 | ☐ |
| 9 | Torn down; shared infrastructure untouched | ☐ |
