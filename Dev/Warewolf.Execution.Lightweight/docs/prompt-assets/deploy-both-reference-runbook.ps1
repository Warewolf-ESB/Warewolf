#Requires -Version 7.0
<#
    REFERENCE ONLY - the manual runbook the wrapper script is derived from.
    Supplied by the user 2026-08-27; corrected 2026-08-27 against the two orchestrators.
    Not intended to be run end-to-end as-is.

    The wrapper implements Step 0 (variables + extraction + validation), A2, B2, B3 and C2.
    C3 is retained because it is the primary consumer of the B3 handover values.

    ── CORRECTIONS APPLIED (each is a real defect in the original) ───────────────
    1  B3 read the Entra ids with `az ad app list --display-name`. The engine already
       writes them: Configure-WwExecutionAuth.ps1 persists ClientId / AppObjectId /
       SpObjectId / AppRoles to Configure-WwExecutionAuth.output.json
       (Configure-WwExecutionAuth.ps1:1903-1921). Read that file - a display-name
       lookup takes [0] of an ambiguous match and returns nothing if the app was
       named differently.
    2  $EngineAuthApp was an operator-chosen placeholder. It is NOT chosen: the name
       auto-derives to "<FunctionAppName>-auth" (Configure-WwExecutionAuth.ps1:962,
       976-977) and the engine summary records it as entraAppDisplayName. Derived, and
       used only to VERIFY the output file belongs to this app.
    3  $AiConn was read from an App Insights component the B2 call never asked to be
       created - B2 passed neither -AppInsightsName nor -EnableAppInsights. Both are
       now passed.
    4  The engine summary's appInsightsConnectionString is MASKED
       (Deploy-WwExecutionEngine.ps1:675-680), so it can never be used as the value for
       -AppInsightsConnectionString. It still has to come from `az monitor app-insights`.
    5  B3 picked the newest summary without checking it. Save-DeploySummary writes
       incrementally with status in-progress|completed|failed
       (Deploy-WwExecutionEngine.ps1:649-663), so an aborted run leaves a summary that
       looks current. status/dryRun are now asserted.
    6  $EngineUrl was hand-built from $EngineApp. The summary records the real one as
       `endpoint`; read it instead of assuming the azurewebsites.net form.
    7  C2 passed -Dockerfile. The parameter is -DockerfilePath
       (Deploy-WwQueueProcessor.ps1:117); -Dockerfile bound only by prefix abbreviation.
    8  -EncryptResources:$true was unconditional. The orchestrator's help says encrypt
       ONCE; later deploys stage the already-encrypted sources as-is.
    9  Invoke-Az was defined but never used, while the B3/C3 calls it exists for are
       exactly the ones whose JMESPath contains [ ].
    10 C3 re-queried the app-role id from Graph. The auth output file already carries
       AppRoles with their ids.
#>

# ══════════════════════════════════════════════════════════════════════════════
# Step 0: Preflight
# ══════════════════════════════════════════════════════════════════════════════

### helper - az mangles unquoted [ ] ( ) in JMESPath on Windows
function Invoke-Az {
    $quoted = foreach ($a in $args) {
        if ($a -is [string] -and $a -match '[()\[\]@?*<>|&^]') { '"' + ($a -replace '"','\"') + '"' }
        else { $a }
    }
    & az @quoted
}

# ── Identity / subscription ───────────────────────────────────────────────────
az login                                            # skip if already signed in
$Sub      = (az account show --query id       -o tsv)
$TenantId = (az account show --query tenantId -o tsv)
az account set --subscription $Sub

# ── Reused foundation (NEVER created, NEVER deleted) ──────────────────────────
$Rg        = '<resource group>'
$Loc       = '<resource location>'
$Kv        = '<key vault name>'
$KvSecret  = '<key vault secret name>'
$Acr       = '<azure container registry>'
$AcaEnv    = '<azure container apps environment>'
$Workspace = '<log analytics workspace>'          # not consumed below; ACA env already carries it

# ── Staging ───────────────────────────────────────────────────────────────────
$Stage         = '<local staging directory>'
$LogDir        = "$Stage\logs\e2e-wwengine-staging"   # dedicated: keeps this run's summaries unambiguous
$SecureConfig  = "$Stage\settings\secure.config"
$AuthConfig    = "$Stage\settings\Deploy-WwExecutionEngine.authconfig.json"
$LicenseConfig = "$Stage\settings\Warewolf License.secureconfig"
$WorkflowsSrc  = "$Stage\resources"
$SourceDir     = "$Stage\sources"                    # reused as-is
$TriggerDir    = "$Stage\triggers"

# ── CREATED by this run ───────────────────────────────────────────────────────
$EngineApp     = '<engine app name>'
$EngineStorage = '<engine app storage name>'
$EngineAi      = "$EngineApp-ai"                     # the orchestrator's own default for -AppInsightsName
# FIX 2: NOT an operator choice - Configure-WwExecutionAuth auto-derives it.
$EngineAuthApp = "$EngineApp-auth"                   # expected; verified against the auth output below

$EnginePublish = "$Stage\apps\AzureFunctionsPackage-3.0.2.110.zip" # zip or directory
$QpPublish     = "$Stage\apps\Warewolf-QueueProcessor-3.0.2.110"   # directory only; a zip must be extracted first
$QpPrefix      = '<queue processor app name prefix>'
$ImageRepo     = '<queue processor image repository name>'
$DockerFile    = "$Stage\build\Dockerfile"
# FIX 6: $EngineUrl is NOT hand-built - it is read from the engine summary in B3.
$EngineUrl     = $null

# FIX 8: first deploy of a given source set only. Leave $false afterwards - the
# sources are already WFAES-encrypted and are staged as-is (KeyVault args still required).
$EncryptResources = $false

# ── This run's dedicated queues (created by the publisher, non-durable) ───────
$QueueSuccess  = 'order-success-queue'
$QueueFailure  = 'order-failure-queue'

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null

## if $EnginePublish is a zip, extract it and repoint $EnginePublish at the extracted directory
## if $QpPublish     is a zip, extract it and repoint $QpPublish     at the extracted directory

# Verification
Test-Path "$EnginePublish\Warewolf.Execution.Lightweight.dll"     # True
Test-Path "$QpPublish\Warewolf.Execution.QueueProcessor.dll"      # True
(Get-ChildItem $QpPublish -Recurse -Filter *.bite).Count          # MUST be 0

# ══════════════════════════════════════════════════════════════════════════════
# Phase A2 - resolve the VERSIONLESS uri of the AMQP secret KEDA needs
# ══════════════════════════════════════════════════════════════════════════════

Invoke-Az keyvault secret show --vault-name $Kv --name 'rabbitmq-uri' --query "{name:name,enabled:attributes.enabled}" -o json
$vaultUri        = (az keyvault show --name $Kv --query properties.vaultUri -o tsv)
$RabbitSecretUri = ($vaultUri.TrimEnd('/')) + '/secrets/rabbitmq-uri'
$RabbitSecretUri

# ══════════════════════════════════════════════════════════════════════════════
# B2 - Deploy the engine for real
# ══════════════════════════════════════════════════════════════════════════════
# FIX 3: -EnableAppInsights / -AppInsightsName added, so the component B3 reads exists.
# FIX 8: -EncryptResources is now driven by the variable, not pinned to $true.

.\Deploy-WwExecutionEngine.ps1 `
  -SubscriptionId $Sub -TenantId $TenantId `
  -ResourceGroup $Rg -Location $Loc `
  -AppName $EngineApp -StorageAccount $EngineStorage `
  -PublishPath $EnginePublish `
  -AuthConfigPath $AuthConfig `
  -SecureConfigPath $SecureConfig `
  -LicenseConfigPath $LicenseConfig `
  -WorkflowsSourcePath $WorkflowsSrc `
  -KeyVaultName $Kv -KeyVaultSecretName $KvSecret `
  -EncryptResources:$EncryptResources -VerifyDecryption `
  -EnableAppInsights:$true -AppInsightsName $EngineAi `
  -LogDir $LogDir

# ══════════════════════════════════════════════════════════════════════════════
# B3 - Capture the identifiers Phase C, C3 and the rollback need
# ══════════════════════════════════════════════════════════════════════════════
# The engine deploy writes TWO machine-readable outputs. Read both; query Azure only
# for what neither records (the unmasked App Insights connection string).
#
#   1. $LogDir\deploy-WwExecutionEngine-<stamp>.summary.json   (per run, atomic,
#      rewritten after every phase - Deploy-WwExecutionEngine.ps1:649-727)
#        status, dryRun, runId, resourceTags, created, endpoint, appName,
#        appInsightsName, entraAppDisplayName, keyVault, appSettings (masked)
#      Consumed by Rollback-WwExecutionEngine.ps1 -SummaryPath.
#
#   2. Scripts\Configure-WwExecutionAuth.output.json           (Configure-WwExecutionAuth.ps1:1903-1921)
#        ClientId, AppObjectId, SpObjectId, Audience, Issuer, AppRoles[].id,
#        EntraAppDisplayName, FunctionAppName
#      *** FIXED PATH IN THE SCRIPTS FOLDER, OVERWRITTEN BY EVERY RUN. ***
#      It is NOT per-run and NOT in $LogDir: read it immediately after the deploy and
#      verify FunctionAppName before trusting it. Written only when auth provisioning
#      ran (no -SkipAuthProvisioning) and not on -DryRun.

# --- engine summary ---
$Summary = (Get-ChildItem $LogDir -Filter 'deploy-WwExecutionEngine-*.summary.json' |
            Where-Object Name -notmatch 'dryrun' | Sort-Object LastWriteTime | Select-Object -Last 1).FullName
if (-not $Summary) { throw "No non-dry-run engine summary found in $LogDir." }
$S = Get-Content $Summary -Raw | ConvertFrom-Json

# FIX 5: the newest summary is not necessarily a SUCCESSFUL one.
if ($S.status -ne 'completed') { throw "Engine deploy did not complete: status=$($S.status) lastPhase=$($S.lastPhase) error=$($S.error)" }
if ($S.dryRun)                 { throw "Newest summary is a dry run; refusing to hand its values to the QueueProcessor." }
if ($S.appName -ne $EngineApp) { throw "Summary is for '$($S.appName)', expected '$EngineApp'." }

$RunId     = $S.runId              # wwx-<stamp>  - also the rollback tag (wwx-test-run=<runId>)
$EngineUrl = $S.endpoint           # FIX 6
$EngineAi  = $S.appInsightsName    # authoritative name actually used

# --- auth output (the Entra identifiers) ---
$AuthOut = Join-Path $PSScriptRoot 'Configure-WwExecutionAuth.output.json'   # Scripts folder, fixed name
if (-not (Test-Path $AuthOut)) { throw "Auth output not found at $AuthOut - was the engine deployed with -SkipAuthProvisioning?" }
$A = Get-Content $AuthOut -Raw | ConvertFrom-Json

# FIX 1 + the overwrite hazard: prove this file is THIS app's before using it.
if ($A.FunctionAppName -ne $EngineApp) {
    throw "$AuthOut belongs to '$($A.FunctionAppName)', not '$EngineApp'. It is overwritten by every deploy - re-read it straight after the engine run."
}
if ($A.EntraAppDisplayName -ne $EngineAuthApp) {
    Write-Warning "Entra app is named '$($A.EntraAppDisplayName)', expected '$EngineAuthApp'."
}

$EngineAppId = $A.ClientId          # was: az ad app list --display-name ... [0].appId
$EngineSpId  = $A.SpObjectId        # was: az ad sp  list --display-name ... [0].id
$EngineAudience = $A.Audience       # api://<ClientId> - the token resource for B4/B5
$QpRoleId    = ($A.AppRoles | Where-Object value -eq 'Warewolf_QueueProcessor').id   # FIX 10

if (-not $EngineAppId) { throw 'ClientId missing from the auth output; refusing to deploy a worker that cannot authenticate.' }

# FIX 4: the summary's copy of this is masked - it has to be read from Azure.
$AiConn = (az monitor app-insights component show --app $EngineAi -g $Rg --query connectionString -o tsv)
if (-not $AiConn) { Write-Warning "No App Insights connection string for '$EngineAi'; omit -EnableAppInsights on the QueueProcessor rather than passing an empty one." }

"summary=$Summary`nrunId=$RunId`nendpoint=$EngineUrl`nappId=$EngineAppId`nspId=$EngineSpId`nroleId=$QpRoleId"

# ══════════════════════════════════════════════════════════════════════════════
# C2 - Deploy the QueueProcessor for real
# ══════════════════════════════════════════════════════════════════════════════
# FIX 7: -DockerfilePath, not -Dockerfile.

.\Deploy-WwQueueProcessor.ps1 `
  -ResourceGroup $Rg -Location $Loc `
  -AcaEnvironment $AcaEnv -AcrName $Acr `
  -PublishPath $QpPublish `
  -ImageRepository $ImageRepo `
  -AppNamePrefix $QpPrefix `
  -TriggerPath $TriggerDir `
  -QueueSourcePath $SourceDir `
  -EngineBaseUrl $EngineUrl `
  -EngineResourceAppId $EngineAppId `
  -EngineTenantId $TenantId `
  -KeyVaultName $Kv -KeyVaultSecretName $KvSecret -EncryptStagedSettings `
  -RabbitMqSecretUri $RabbitSecretUri -InlineRabbitMqSecret `
  -ScalingMode Elastic `
  -EnableAppInsights -AppInsightsConnectionString $AiConn `
  -DockerfilePath $DockerFile `
  -LogDir $LogDir

# ══════════════════════════════════════════════════════════════════════════════
# C3 - App-role grant (the primary consumer of the B3 handover values)
# ══════════════════════════════════════════════════════════════════════════════
# Out of the wrapper's scope, shown to make the handover contract concrete:
# $EngineSpId and $QpRoleId both come from the auth output file, not from Graph.

$apps = @(Invoke-Az containerapp list -g $Rg --query "[?starts_with(name,'$QpPrefix')].name" -o tsv)

foreach ($app in $apps) {
    $mi = az containerapp show --name $app -g $Rg --query identity.principalId -o tsv --only-show-errors
    if (-not $mi) { Write-Warning "$app has no managed identity; skipping"; continue }

    $bodyFile = New-TemporaryFile
    @{ principalId = $mi; resourceId = $EngineSpId; appRoleId = $QpRoleId } |
        ConvertTo-Json -Compress | Set-Content -Path $bodyFile -Encoding utf8NoBOM

    az rest --method POST `
      --uri "https://graph.microsoft.com/v1.0/servicePrincipals/$mi/appRoleAssignments" `
      --headers "Content-Type=application/json" `
      --body "@$bodyFile"

    Remove-Item $bodyFile
}
