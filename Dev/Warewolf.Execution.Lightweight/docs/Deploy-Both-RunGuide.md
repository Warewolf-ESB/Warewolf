# Deploy-Both Run Guide — Execution Engine + QueueProcessor in one pass

How to use [`Scripts/Deploy-WwEngineAndQueueProcessor.ps1`](../Scripts/Deploy-WwEngineAndQueueProcessor.ps1):
a manual, PowerShell 7+ wrapper that deploys the Execution Engine, reads back what that deploy
produced, and wires the RabbitMQ QueueProcessor from it.

It provisions nothing itself. It calls two orchestrators and asserts the handover between them:

| | Script | Result |
|---|---|---|
| Engine | [`Deploy-WwExecutionEngine.ps1`](../Scripts/Deploy-WwExecutionEngine.ps1) | one Function App |
| Workers | [`Deploy-WwQueueProcessor.ps1`](../Scripts/Deploy-WwQueueProcessor.ps1) | one Container App **per trigger `.bite`**, autoscaled 0..N by KEDA |

---

## 1. Why a wrapper, and not `-DeployRabbitMqTriggers`

`Deploy-WwExecutionEngine.ps1` can deploy the workers itself via `-DeployRabbitMqTriggers`. This
wrapper makes the two calls **separately** so the values that pass between them are asserted before
the second call runs — the engine's deploy status, the identity of the auth output file, and the
presence of a client id. A worker deployed with an empty `-EngineResourceAppId` starts, consumes
messages, and fails every engine call; the assertions exist to make that impossible.

**Do not enable both paths.** If you set `-DeployRabbitMqTriggers` in the engine splat you will get
two sets of Container Apps.

---

## 2. Prerequisites — deployment (operator) machine

This runs from **any** folder on a machine with **no source checkout and no .NET SDK** — you
download zips and execute from, say, `G:\Deployment`. Nothing here needs git, Visual Studio or
`dotnet`.

### 2.1 Machine

| Need | Check | If missing |
|---|---|---|
| **PowerShell 7+** | `$PSVersionTable.PSVersion` | `winget install Microsoft.PowerShell` — Windows PowerShell 5.1 will **not** run these (`#Requires -Version 7.0`) |
| **Azure CLI** | `az version` | <https://aka.ms/installazurecliwindows> |
| `containerapp` extension | `az extension list --query "[].name" -o tsv` | `az extension add --name containerapp` (the worker orchestrator installs it too, but doing it now keeps the dry run clean) |
| Signed in | `az account show` | `az login`. **The script never calls `az login` itself** — it fails with a clear message instead, so an unattended run can't sit on a device-code prompt. |

`dotnet`, `func` and Docker are **not** required: the packages are pre-built, and the container
image is built server-side by `az acr build`.

### 2.2 What to download, and where it goes

Three separate artefacts. The layout matters — see the warning below.

```
G:\Deployment\
├─ Scripts\                    ← the COMPLETE Scripts folder from the repo/release zip
│    Deploy-WwEngineAndQueueProcessor.ps1     the script you run
│    Deploy-WwExecutionEngine.ps1             |
│    Deploy-WwQueueProcessor.ps1              |
│    Configure-WwExecutionAuth.ps1            |  resolved as SIBLINGS at run time
│    Setup-ApplicationInsights.ps1            |
│    Encrypt-Config.ps1                       |
│    WwE2E.Common.psm1              token helper for the verification steps (§6.0)
│    Generate-WorkflowIndex.ps1               optional (engine disk-scans without it)
│    Rollback-WwExecutionEngine.ps1           needed only to tear down
├─ apps\
│    AzureFunctionsPackage-<ver>.zip          engine build output (folder or .zip)
│    Warewolf-QueueProcessor-<ver>.zip        worker build output (folder or .zip)
├─ build\
│    Dockerfile                               ← from the QueueProcessor SOURCE, not the Scripts zip
├─ settings\
│    secure.config
│    Deploy-WwExecutionEngine.authconfig.json
│    Warewolf License.secureconfig
├─ resources\                  workflow .bite files
├─ sources\                    RabbitMQ source .bite files, named {sourceId}.bite
├─ triggers\                   queue trigger .bite files — ONE CONTAINER APP EACH
└─ logs\deploy-both\           created for you
```

> **Extract the whole `Scripts` folder, not just the three `Deploy-*.ps1` files.** The
> orchestrators resolve their helpers as siblings of themselves *at the moment they need them*,
> and do not pre-check — so a partial extraction would otherwise fail **mid-deploy, after
> resources exist**. The wrapper checks the full set up front and stops with a list:
>
> ```
> 3 required script(s) missing from 'G:\Deployment\Scripts':
>   Configure-WwExecutionAuth.ps1    Entra + Easy Auth provisioning; also writes the auth output this script reads
>   Setup-ApplicationInsights.ps1    App Insights provisioning (this wrapper always passes -EnableAppInsights)
>   Encrypt-Config.ps1               secure.config validation/encryption, and the QueueProcessor -EncryptStagedSettings pass
> ```

> **The `Dockerfile` is not in the Scripts zip.** It ships with the QueueProcessor source. Without
> `-DockerfilePath`, `Deploy-WwQueueProcessor.ps1` looks for it at a path two levels above its own
> folder *inside a source checkout* — which does not exist here. This wrapper always passes the
> path explicitly and fails in step 2 if the file is absent, before the image build starts.

The two publish outputs must be **different** build artefacts — the engine and the worker are
separate projects. A `.zip` is extracted to a sibling folder automatically; the worker's
`-PublishPath` must end up a **directory** (that orchestrator does not accept a zip).

### 2.3 Unblock the downloaded scripts

Files extracted from a zip that came from a browser, email or network share carry a
mark-of-the-web and are refused by PowerShell:

```powershell
Get-ChildItem G:\Deployment\Scripts\*.ps1 | Unblock-File
Get-ExecutionPolicy -List          # need at least RemoteSigned for this scope
# If it is Restricted, for THIS SESSION only:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process
```

`Unblock-File` silently does nothing when there is no mark — safe to run always.

### 2.4 Azure access

**Data-plane** access to the Key Vault — `Key Vault Secrets Officer` or `Key Vault Secrets User`.
Control-plane Owner/Contributor is *not* enough, and fails later in a less obvious place:

```powershell
$Kv = '<key vault name>'
$me = az ad signed-in-user show --query id -o tsv
az role assignment list --assignee $me --scope (az keyvault show --name $Kv --query id -o tsv) `
    --query "[].roleDefinitionName" -o tsv
```

You also need a Key Vault secret named `rabbitmq-uri` holding the AMQP uri KEDA reads, plus rights
to create a Function App, storage account, App Insights and Container Apps in the target
subscription, and to create an Entra app registration.

### 2.5 Verify the layout before you start

```powershell
$Root = 'G:\Deployment'
Set-Location "$Root\Scripts"

# Every hard dependency present?
'Deploy-WwEngineAndQueueProcessor.ps1','Deploy-WwExecutionEngine.ps1','Deploy-WwQueueProcessor.ps1',
'Configure-WwExecutionAuth.ps1','Setup-ApplicationInsights.ps1','Encrypt-Config.ps1' |
    ForEach-Object { '{0,-6} {1}' -f (Test-Path $_), $_ }

# Payload present?
Get-ChildItem "$Root\apps"; Test-Path "$Root\build\Dockerfile"
Get-ChildItem "$Root\settings"
(Get-ChildItem "$Root\triggers" -Filter *.bite).Count    # one Container App per file — must be > 0
```

---

## 3. Edit the variable block

Open the script and fill in every `<angle bracket>` value near the top. The run aborts up front and
lists **all** of them at once if any are left:

```
Edit the variable block - 3 value(s) are still placeholders:
  Rg                 = <resource group>
  Kv                 = <key vault name>
  QpPrefix           = <queue processor app name prefix>
```

Three values are deliberately **not** yours to choose — they are derived and then read back from the
engine's own outputs:

| Value | Where it really comes from |
|---|---|
| `$EngineUrl` | the summary's `endpoint` — never hand-build `https://<app>.azurewebsites.net` |
| `$EngineAi` | the summary's `appInsightsName` (defaults to `<AppName>-ai`) |
| `$EngineAuthApp` | `Configure-WwExecutionAuth` auto-derives `<AppName>-auth` |

For the `G:\Deployment` layout in §2.2, set `$Stage` and the rest follows — every other path in the
block is derived from it:

```powershell
$Stage         = 'G:\Deployment'                                     # everything below derives from this
$EnginePublish = 'G:\Deployment\apps\AzureFunctionsPackage-3.0.2.110.zip'
$QpPublish     = 'G:\Deployment\apps\Warewolf-QueueProcessor-3.0.2.110.zip'
$DockerFile    = 'G:\Deployment\build\Dockerfile'
```

`$LogDir`, `$SecureConfig`, `$AuthConfig`, `$LicenseConfig`, `$WorkflowsSrc`, `$SourceDir` and
`$TriggerDir` are all `"$Stage\..."` already — leave them unless your layout differs.

Below the variable block is a commented inventory of **every parameter the wrapper does not pass** —
38 of the engine's 58 and 23 of the QueueProcessor's 47, each with a one-line note. To use one,
uncomment the variable *and* add it to the matching splat.

> **Keep your edited copy.** On an operator machine the filled-in script *is* the deployment record
> for that environment — back it up alongside the handover file. If you deploy more than one
> environment from the same folder, keep one copy per environment
> (`Deploy-WwEngineAndQueueProcessor.UAT.ps1`, `...PROD.ps1`) — a copy must stay **inside the
> `Scripts` folder**, since the orchestrators are found relative to the running script.

---

## 4. Running it

Run from the `Scripts` folder:

```powershell
Set-Location G:\Deployment\Scripts
```

### See both plans, change nothing

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -DryRun -NonInteractive
```

Each child prints its own plan phase. Note the caveat: a dry run produces no auth output and only a
`*.dryrun.summary.json`, so there is nothing real to assert. The wrapper emits clearly-marked
**synthetic** handover values (`00000000-...`) purely so the QueueProcessor's plan can be reached,
and stamps `"synthetic": true` in the handover file. Never treat a dry-run handover file as real.

### First deploy of a new source set

`-EncryptResources` converts workflow / Elasticsearch / persistence sources from plain or DPAPI to
WFAES using the Key Vault key. **Do this once.**

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -EncryptResources
```

You get a `y/N` gate before each real deploy, showing the resolved parameter set:

```
  About to run: Deploy-WwExecutionEngine.ps1
    -AppName                      wwengine-uat
    -EncryptResources             True
    ...
  Proceed? (y/N):
```

### Every deploy after that

Sources are already encrypted and are staged as-is. `-KeyVaultName` / `-KeyVaultSecretName` are still
passed so the runtime can decrypt them.

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1
```

### Redeploy only the workers, against the engine already up

Step 5 still runs and still asserts — it resolves the handover values from the existing summary and
auth output rather than from a fresh deploy.

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -SkipEngine
```

### Engine only

The handover file is still written, so the worker deploy can be run later.

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -SkipQueueProcessor
```

### Unattended

```powershell
.\Deploy-WwEngineAndQueueProcessor.ps1 -NonInteractive
```

---

## 5. After the deploy — the app-role grant

This is the main consumer of the handover file, and the reason it exists. Each Container App's
managed identity should hold the engine app role `Warewolf_QueueProcessor`. The `spObjectId` and the
role id both come out of the file — no Graph lookup, no display-name guessing:

```powershell
$H = Get-Content "<LogDir>\deploy-both-<stamp>.handover.json" -Raw | ConvertFrom-Json
if ($H.synthetic) { throw 'Synthetic (dry-run) handover file.' }

foreach ($app in $H.queueProcessor.apps) {
    $mi = az containerapp show --name $app -g $H.engine.resourceGroup `
            --query identity.principalId -o tsv --only-show-errors
    if (-not $mi) { Write-Warning "$app has no managed identity; skipping"; continue }

    $body = New-TemporaryFile
    @{ principalId = $mi
       resourceId  = $H.entra.spObjectId
       appRoleId   = $H.entra.queueProcessorAppRoleId } |
        ConvertTo-Json -Compress | Set-Content $body -Encoding utf8NoBOM

    az rest --method POST `
      --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$mi/appRoleAssignments" `
      --headers "Content-Type=application/json" --body "@$body"

    Remove-Item $body
}
```

`secure.config` also needs a **per-workflow Execute row** for each trigger's `WorkflowName`. The
grant is defence in depth; the secure.config row is the one that governs execution.

---

## 6. Verifying

### 6.0 Getting a token — do NOT use `az account get-access-token`

On a **freshly created** engine app registration that fails, every time:

```
AADSTS65001: The user or administrator has not consented to use the application with ID
'04b07795-8ddb-461a-bbee-02f9e1bf7b46' named 'Microsoft Azure CLI'
```

The engine app does expose a delegated `user_impersonation` scope (Configure-WwExecutionAuth
Stage 3b), so `api://<clientId>` is a valid resource — but the **Azure CLI client is not in the new
app's `preAuthorizedApplications`**, so a delegated token needs interactive consent that has never
been given. The command in older runbooks appears to work only on long-lived registrations where
someone consented previously; it fails on every first deployment.

Use **client credentials** instead. The Easy Auth secret that `Configure-WwExecutionAuth.ps1` stores
in the Function App setting `MICROSOFT_PROVIDER_AUTHENTICATION_SECRET` lets the app authenticate **as
itself** — no consent, no directory change:

```powershell
Import-Module "$Root\Scripts\WwE2E.Common.psm1" -Force
$H = Get-Content "<LogDir>\deploy-both-<stamp>.handover.json" -Raw | ConvertFrom-Json

function Get-EngineToken {
    Get-E2EEngineToken -TenantId        $H.tenantId `
                       -EngineAppId     $H.entra.clientId `
                       -FunctionAppName $H.engine.appName `
                       -ResourceGroup   $H.engine.resourceGroup
}
$tok = Get-EngineToken
```

> **This token carries no app roles.** That is enough for any workflow resolving against the global
> permission map, because the Public group's server-level Execute is OR'd into the effective
> permissions. If you need a token that *does* carry a role, register a client app with
> `Configure-WwExecutionAuth-Clients.ps1` and use `Get-WwExecutionToken.ps1`.

**Alternative — grant the consent** (changes the directory; needs a user who may consent):

```powershell
az logout
az login --tenant $H.tenantId --scope "$($H.entra.audience)/.default"
$tok = az account get-access-token --resource $H.entra.audience --query accessToken -o tsv
```

```powershell
$H   = Get-Content "<LogDir>\deploy-both-<stamp>.handover.json" -Raw | ConvertFrom-Json
$url = $H.engine.endpoint
$tok = Get-EngineToken   # see below — NOT az account get-access-token

# Public  — anonymous, expect 200
(Invoke-WebRequest "$url/Public/Hello%20World.json?Name=e2e" -SkipHttpErrorCheck).StatusCode

# Secure  — expect 401 without a token, 200 with one
(Invoke-WebRequest "$url/Secure/Hello%20World.json?Name=e2e" -SkipHttpErrorCheck).StatusCode
(Invoke-WebRequest "$url/Secure/Hello%20World.json?Name=e2e" `
    -Headers @{ Authorization = "Bearer $tok" } -SkipHttpErrorCheck).StatusCode

# Services — expect 401 without the function key
(Invoke-WebRequest "$url/Services/Hello%20World.json" -SkipHttpErrorCheck).StatusCode
```

Reach every workflow a trigger names — this is the check that actually predicts whether the workers
will succeed. **A `500` here is not necessarily a server fault:** the engine surfaces authorization
denials as 500, not 403 (WOLF-8418), so a missing secure.config Execute row looks like a crash.

```powershell
foreach ($wf in (Get-ChildItem "$Root\triggers" -Filter *.bite |
                 ForEach-Object { (Get-Content $_.FullName -Raw | ConvertFrom-Json).WorkflowName })) {
    $route = ($wf -replace '\\','/') -split '/' | ForEach-Object { [uri]::EscapeDataString($_) }
    $r = Invoke-WebRequest "$url/Secure/$($route -join '/').json" `
                           -Headers @{ Authorization = "Bearer $tok" } -SkipHttpErrorCheck
    "{0,-40} {1}" -f $wf, $r.StatusCode
}
```

Revision health and the KEDA rules:

```powershell
foreach ($app in $H.queueProcessor.apps) {
    $rev = (az containerapp revision list --name $app -g $H.engine.resourceGroup -o json |
            ConvertFrom-Json) | Select-Object -First 1
    [pscustomobject]@{ app = $app; rev = $rev.name
                       healthy = $rev.properties.healthState; active = $rev.properties.active }

    $c = az containerapp show --name $app -g $H.engine.resourceGroup -o json | ConvertFrom-Json
    [pscustomobject]@{ app = $c.name
                       min = $c.properties.template.scale.minReplicas
                       max = $c.properties.template.scale.maxReplicas }
    $c.properties.template.scale.rules | ConvertTo-Json -Depth 6
}
```

> **Never pass `--query` to `az` from PowerShell on Windows.** `az` is a `.cmd` shim and cmd.exe
> mangles JMESPath. Quoting around it does not help — it makes `az` echo its own resolved command
> line to stdout, and that echo is then captured as data. A run of this script did exactly that and
> wrote az's command line into the handover file's `apps` array in place of the Container App names.
> Fetch `-o json`, pipe through `ConvertFrom-Json`, and filter in PowerShell. The script's
> `Invoke-AzJson` helper **throws** if it is handed `--query`, matching `WwE2E.Common.psm1`.

For the full scored end-to-end proof (publish → KEDA scale-from-zero → workflow completion), use the
E2E harness instead — `New-WwE2EStaging.ps1` / `Invoke-WwE2EVerification.ps1`, see the
`warewolf-e2e-verify` skill.

---

## 7. The handover file

Written to `<LogDir>\deploy-both-<stamp>.handover.json` after the capture step, then rewritten after
the worker deploy to add the Container App names. It merges the engine's two outputs:

| Source | Contributes |
|---|---|
| `deploy-WwExecutionEngine-<stamp>.summary.json` | `runId`, `endpoint`, `appName`, `appInsightsName`, `entraAppDisplayName` |
| `Configure-WwExecutionAuth.output.json` | `clientId`, `spObjectId`, `audience`, `queueProcessorAppRoleId` |
| this script | `rabbitMqSecretUri`, the resolved app names, both source paths |

```json
{
  "producedBy": "Deploy-WwEngineAndQueueProcessor.ps1",
  "synthetic": false,
  "runId": "wwx-20260827-101500",
  "resourceTags": ["wwx-test-run=wwx-20260827-101500"],
  "engine":   { "appName": "...", "endpoint": "https://...", "appInsightsName": "...",
                "entraAppDisplayName": "...", "summaryPath": "..." },
  "entra":    { "clientId": "...", "spObjectId": "...", "audience": "api://...",
                "queueProcessorAppRoleId": "...", "authOutputPath": "..." },
  "keyVault": { "name": "...", "secretName": "...", "rabbitMqSecretUri": "https://.../secrets/rabbitmq-uri" },
  "queueProcessor": { "appNamePrefix": "wwqp-", "apps": ["wwqp-orders", "wwqp-invoices"] },
  "notes": []
}
```

**Never written to it:** the App Insights connection string, the broker uri value, or any Key Vault
secret value. Names and uris only.

---

## 8. When it stops — what each assertion means

| Message | Cause | Fix |
|---|---|---|
| `Edit the variable block - N value(s) are still placeholders` | unedited `<...>` values | fill them in; all are listed at once |
| `QueueProcessor publish output contains N .bite file(s)` | a `.bite` in the publish output | remove them — they are baked into the image and compete with the staged trigger |
| `... does not contain Warewolf.Execution.QueueProcessor.dll` | engine and worker publish paths swapped | they are different csproj outputs; never the same folder |
| `No *.bite trigger files in <dir>` | empty trigger folder | the worker deploy would create nothing; zero matches is an error, never a silent no-op |
| `Secret 'rabbitmq-uri' not found ... (or no data-plane access)` | missing secret, or control-plane-only access | see §2 |
| `Engine deploy did not complete: status=failed ...` | the newest summary is an aborted run | the summary is rewritten after every phase; read `lastPhase`/`error`, fix, re-run |
| `Newest summary is a dry run` | only `-DryRun` has run in this `$LogDir` | run the engine for real, or point `$LogDir` at the run that did |
| `Summary is for app 'X', expected 'Y'` | `$LogDir` shared between environments | give each environment its own `$LogDir` |
| `<path> belongs to 'X', not 'Y'` | **the auth output is one fixed file in `Scripts\`, overwritten by every deploy** | it is not per-run; re-read it immediately after the engine run, and don't deploy two engines concurrently from one checkout |
| `Auth output not found ...` | engine ran with `-SkipAuthProvisioning`, or only as a dry run | the worker needs the client id; provision auth |
| `ClientId missing ... refusing to deploy a worker that cannot authenticate` | truncated/foreign auth output | re-run the engine deploy |

A warning rather than a stop:

- **`No App Insights connection string for '<name>'`** — the wrapper then **omits**
  `-EnableAppInsights` from the worker splat rather than passing it empty. Worth investigating: the
  engine splat asks for App Insights by name, so an empty result usually means the component was
  created under a different name.
- **`App role 'Warewolf_QueueProcessor' is not defined`** — your `authconfig.json` doesn't define it,
  so §5 has nothing to assign. The grant is optional; secure.config is not.

---

## 9. Rollback

The `runId` doubles as the resource tag `wwx-test-run=<runId>`, and rollback targets that tag only:

```powershell
$H = Get-Content "<LogDir>\deploy-both-<stamp>.handover.json" -Raw | ConvertFrom-Json
.\Rollback-WwExecutionEngine.ps1 -SummaryPath $H.engine.summaryPath
```

Container Apps are removed separately — they are not in the engine's summary:

```powershell
foreach ($app in $H.queueProcessor.apps) {
    az containerapp delete --name $app -g $H.engine.resourceGroup --yes
}
```

The resource group, Key Vault, ACR and ACA environment are **reused, never created and never
deleted** by this script. Do not delete them as part of a rollback.

---

## See also

- [`Scripts/README.md`](../Scripts/README.md) — the full script set
- [`docs/Deploy-RunGuide.md`](Deploy-RunGuide.md) — engine deploy in depth
- [`docs/prompt-assets/deploy-both-reference-runbook.ps1`](prompt-assets/deploy-both-reference-runbook.ps1) — the manual runbook this script was derived from, with its corrections annotated
- `warewolf-deploy`, `warewolf-e2e-verify` skills
