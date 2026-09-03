#Requires -Version 7.0
<#
.SYNOPSIS
    Applies the category-A (control-plane only) changes from
    Dev/Docs/8520-QueueProcessor-Throughput-Gap-Closure-Plan.md to a live engine + Container App.

.DESCRIPTION
    Every change here is an Azure control-plane change: app settings, an Easy Auth flag, and
    Container App properties. NOTHING is rebuilt, republished or re-imaged, and no source file is
    touched. See the plan's section 5.0 for why each item qualifies, and for the items that
    deliberately are NOT here.

    DELIBERATELY EXCLUDED - these are NOT control-plane changes and this script will not attempt them:

      * WEBSITE_RUN_FROM_PACKAGE=1  - the engine currently runs an EXTRACTED wwwroot (host logs
        reference C:\home\site\wwwroot\..., Kudu shows api/zipdeploy, there is no SitePackages).
        Setting the flag without redeploying the package BREAKS the app, because the platform then
        looks for d:\home\data\SitePackages\packagename.txt. Correct order is: set the setting, then
        redeploy. Do it with Deploy-WwExecutionEngine.ps1, not here.

      * Trigger Prefetch - baked into the container image (QueueProcessor Dockerfile:51, "Config is
        BAKED IN") with no environment override. Raising WORKER__MAXCONCURRENCY alone is INERT:
        RabbitMqMessagePump.cs:129 calls BasicQosAsync(prefetchCount: <.bite Prefetch>), a
        per-consumer broker-side cap on unacknowledged messages. See plan section 5.0.1.

      * The QueueProcessor's own WORKER__MAXDELIVERYATTEMPTS / WORKER__RETRYENGINEINTERNALERRORS
        binding fix - shipped in source but the running container is still the OLD image, so those
        two keys remain ignored until a rebuild + new revision.

.PARAMETER Execute
    Actually apply. WITHOUT this switch the script only prints what it would do (dry run is the
    default, matching the other scripts in this folder).

.PARAMETER SkipFileLoggingMode
    Do not apply W1.b (fileLoggingMode=debugOnly). Use this for the FIRST Gate A run if you want the
    engine's host log to remain available as a fallback diagnostic - see the Gate A note in the plan.

.PARAMETER SnapshotDir
    Where to write the pre-change snapshot and the generated rollback script.
    Defaults to a timestamped folder under the current directory.

.EXAMPLE
    ./Apply-Ww8520GateAConfig.ps1
    Dry run. Prints every command and the current value it would replace.

.EXAMPLE
    ./Apply-Ww8520GateAConfig.ps1 -Execute -SkipFileLoggingMode
    Applies W1.a/W1.a2 and the Container App changes, leaving file logging on so the host log stays
    usable if Gate A fails.
#>
[CmdletBinding()]
param(
    [string] $EngineApp     = 'wwengine-e2e-ldi413',
    [string] $EngineRg      = 'DEV2',
    [string] $ContainerApp  = 'wwqp5-runb-ordersuccessqueu-3879',
    [string] $ContainerRg   = 'DEV2',

    # W4.6 - KEDA polling interval. 30s quantises every scale decision.
    [ValidateRange(5, 300)]
    [int]    $PollingInterval = 10,

    # W4.4 - the deploy script's own defaults; the live app is below them at 0.25/0.5Gi.
    [string] $Cpu           = '0.5',
    [string] $Memory        = '1.0Gi',

    [switch] $Execute,
    [switch] $SkipFileLoggingMode,
    [string] $SnapshotDir
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$mode = if ($Execute) { 'EXECUTE' } else { 'DRY RUN' }
if (-not $SnapshotDir) {
    $SnapshotDir = Join-Path (Get-Location) ("ww8520-config-{0:yyyyMMdd-HHmmss}" -f (Get-Date))
}

function Write-Head([string] $Text) {
    Write-Host ''
    Write-Host "== $Text" -ForegroundColor Cyan
}
function Write-Step([string] $Id, [string] $Text) {
    Write-Host ("  [{0}] {1}" -f $Id, $Text) -ForegroundColor Gray
}
function Invoke-Az([string[]] $AzArgs, [string] $Why) {
    # NOT $Args - that is a PowerShell automatic variable and the parameter silently never binds,
    # which made every dry-run line print a bare 'az'.
    $rendered = 'az ' + ($AzArgs -join ' ')
    if (-not $Execute) {
        Write-Host "    WOULD RUN: $rendered" -ForegroundColor DarkYellow
        return
    }
    Write-Host "    RUN: $rendered" -ForegroundColor DarkGray
    $out = & az @AzArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "az failed ($Why): $out"
    }
}

Write-Host ''
Write-Host "8520 Gate A - category-A configuration only  [$mode]" -ForegroundColor White
Write-Host "  engine        : $EngineApp (rg $EngineRg)"
Write-Host "  container app : $ContainerApp (rg $ContainerRg)"
Write-Host "  snapshot dir  : $SnapshotDir"
if (-not $Execute) {
    Write-Host '  No changes will be made. Re-run with -Execute to apply.' -ForegroundColor Yellow
}

# ─────────────────────────────────────────────────────────────────────────────
# Phase 0 - pre-flight
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Phase 0 - pre-flight'

if (-not (Get-Command az -ErrorAction SilentlyContinue)) { throw 'az CLI not found on PATH.' }
$account = az account show -o json 2>$null | ConvertFrom-Json
if (-not $account) { throw 'Not logged in. Run: az login' }
Write-Host "  subscription: $($account.name) ($($account.id))"

New-Item -ItemType Directory -Force -Path $SnapshotDir | Out-Null

# ─────────────────────────────────────────────────────────────────────────────
# Phase 1 - snapshot BEFORE anything changes, and generate a rollback script
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Phase 1 - snapshot + rollback generation'

$engineSettingsRaw = az functionapp config appsettings list -n $EngineApp -g $EngineRg -o json | ConvertFrom-Json
$engineSettings = @{}
foreach ($s in $engineSettingsRaw) { $engineSettings[$s.name] = $s.value }

$engineSettingsRaw | ConvertTo-Json -Depth 5 |
    Set-Content (Join-Path $SnapshotDir 'engine-appsettings.before.json') -Encoding UTF8
az webapp auth show -n $EngineApp -g $EngineRg -o json |
    Set-Content (Join-Path $SnapshotDir 'engine-authsettings.before.json') -Encoding UTF8
az containerapp show -n $ContainerApp -g $ContainerRg -o json |
    Set-Content (Join-Path $SnapshotDir 'containerapp.before.json') -Encoding UTF8

Write-Host "  wrote 3 snapshot files (app settings snapshot includes SECRET VALUES - treat as sensitive)" -ForegroundColor Yellow

# The settings this script touches, with the value to restore. An absent setting rolls back to
# a delete, not to an empty string - those are different states.
$touched = @(
    'EXECUTIONLOGLEVEL'
    'AzureFunctionsJobHost__logging__logLevel__default'
    'AzureFunctionsJobHost__logging__logLevel__Host.Results'
    'AzureFunctionsJobHost__logging__fileLoggingMode'
    'AzureFunctionsJobHost__concurrency__dynamicConcurrencyEnabled'
    'AzureFunctionsJobHost__concurrency__snapshotPersistenceEnabled'
    'AzureFunctionsJobHost__healthMonitor__enabled'
    'APPLICATIONINSIGHTS_CONNECTION_STRING'
    'WAREWOLF_WORKFLOW_POOL_MAX'
    'AzureWebJobsDashboard'
)

$restore = [System.Collections.Generic.List[string]]::new()
$delete = [System.Collections.Generic.List[string]]::new()
foreach ($k in $touched) {
    if ($engineSettings.ContainsKey($k)) {
        # Values are re-read from the snapshot file at rollback time, never inlined here, so no
        # secret is written into the rollback script.
        $restore.Add($k)
    } else {
        $delete.Add($k)
    }
}

# 'properties.login...' NOT 'login...': the authV2 extension nests the whole payload under
# 'properties', and the shorter path returns EMPTY rather than erroring - which previously made
# the generated rollback capture 'false' for a token store that was actually ON.
$priorTokenStore = az webapp auth show -n $EngineApp -g $EngineRg `
    --query 'properties.login.tokenStore.enabled' -o tsv 2>$null
if ([string]::IsNullOrWhiteSpace($priorTokenStore)) {
    throw ("Could not read the current Easy Auth token-store state for '$EngineApp'. " +
           'Refusing to continue: without it the generated rollback would guess, and a ' +
           'rollback that guesses at an auth setting is worse than no rollback. ' +
           "Check: az webapp auth show -n $EngineApp -g $EngineRg --query properties.login.tokenStore.enabled")
}
$priorTokenStore = $priorTokenStore.ToLowerInvariant()
Write-Host "  captured prior Easy Auth token-store state: $priorTokenStore" 

# ── Guard: is this snapshot actually pre-change? ─────────────────────────────
# A re-run after a partial failure captures values an earlier run already applied, and the
# rollback generated from it would "restore" the CHANGED state. Detect the fingerprint of an
# earlier run and refuse to present this snapshot as authoritative.
$alreadyApplied = @()
if ($engineSettings['EXECUTIONLOGLEVEL'] -eq 'WARN') { $alreadyApplied += 'EXECUTIONLOGLEVEL=WARN' }
foreach ($k in @(
    'AzureFunctionsJobHost__logging__logLevel__default',
    'AzureFunctionsJobHost__concurrency__dynamicConcurrencyEnabled',
    'WAREWOLF_WORKFLOW_POOL_MAX')) {
    if ($engineSettings.ContainsKey($k)) { $alreadyApplied += $k }
}

if ($alreadyApplied.Count -gt 0) {
    Write-Host ''
    Write-Host '  !! THIS SNAPSHOT IS NOT PRE-CHANGE !!' -ForegroundColor Red
    Write-Host '  These settings already carry this script''s target values, so a previous run' -ForegroundColor Red
    Write-Host '  applied them:' -ForegroundColor Red
    foreach ($a in $alreadyApplied) { Write-Host "    - $a" -ForegroundColor Red }
    Write-Host '  The rollback generated here would restore the CHANGED state, not the original.' -ForegroundColor Red
    Write-Host '  Roll back with the FIRST run''s snapshot directory instead.' -ForegroundColor Red
    Write-Host ''
    $marker = Join-Path $SnapshotDir 'ROLLBACK-NOT-AUTHORITATIVE.txt'
    @(
        'This snapshot was taken AFTER an earlier run of Apply-Ww8520GateAConfig.ps1 had already'
        'applied some settings, so Rollback-Ww8520GateAConfig.ps1 in this folder restores the'
        'CHANGED state and is NOT a valid rollback.'
        ''
        'Use the earliest snapshot directory for this change instead.'
        ''
        'Settings already carrying target values when this snapshot was taken:'
    ) + ($alreadyApplied | ForEach-Object { "  - $_" }) | Set-Content $marker -Encoding UTF8
    Write-Host "  wrote marker: $marker" -ForegroundColor Red
}

$caIdForRollback = az containerapp show -n $ContainerApp -g $ContainerRg --query id -o tsv
$scale = (az containerapp show -n $ContainerApp -g $ContainerRg --query 'properties.template.scale' -o json | ConvertFrom-Json)
$res = (az containerapp show -n $ContainerApp -g $ContainerRg --query 'properties.template.containers[0].resources' -o json | ConvertFrom-Json)

$rollback = @"
#Requires -Version 7.0
# Auto-generated rollback for Apply-Ww8520GateAConfig.ps1
# Generated : $(Get-Date -Format o)
# Engine    : $EngineApp (rg $EngineRg)
# Container : $ContainerApp (rg $ContainerRg)
#
# Restores every setting this script touched to the value captured in engine-appsettings.before.json,
# and the Container App scale/resource properties to their pre-change values.
`$ErrorActionPreference = 'Stop'
`$here = Split-Path -Parent `$MyInvocation.MyCommand.Path
`$before = @{}
(Get-Content (Join-Path `$here 'engine-appsettings.before.json') -Raw | ConvertFrom-Json) |
    ForEach-Object { `$before[`$_.name] = `$_.value }

# --- settings that EXISTED before and must be restored to their prior value ---
`$toRestore = @($(($restore | ForEach-Object { "'$_'" }) -join ', '))
if (`$toRestore.Count -gt 0) {
    `$pairs = `$toRestore | ForEach-Object { "`$_=`$(`$before[`$_])" }
    az functionapp config appsettings set -n $EngineApp -g $EngineRg --settings @pairs -o none
}

# --- settings that did NOT exist before and must be DELETED, not blanked ---
`$toDelete = @($(($delete | ForEach-Object { "'$_'" }) -join ', '))
if (`$toDelete.Count -gt 0) {
    az functionapp config appsettings delete -n $EngineApp -g $EngineRg --setting-names @toDelete -o none
}

# --- Easy Auth token store (value captured before the change) ---
az webapp auth update -n $EngineApp -g $EngineRg --enable-token-store $priorTokenStore -o none

# --- Container App: replicas + resources ---
az containerapp update -n $ContainerApp -g $ContainerRg ``
    --min-replicas $($scale.minReplicas) --max-replicas $($scale.maxReplicas) ``
    --cpu $($res.cpu) --memory $($res.memory) -o none

# --- Container App: pollingInterval is a TEMPLATE property with no CLI flag, so it is
#     patched through ARM both on the way out and on the way back. Omitting it here is how a
#     "successful" rollback quietly leaves the scale cadence changed. ---
az resource update --ids '$caIdForRollback' ``
    --set properties.template.scale.pollingInterval=$($scale.pollingInterval) -o none

Write-Host 'Rollback complete. Re-check with: az functionapp config appsettings list -n $EngineApp -g $EngineRg -o table'
"@

$rollbackPath = Join-Path $SnapshotDir 'Rollback-Ww8520GateAConfig.ps1'
$rollback | Set-Content $rollbackPath -Encoding UTF8
Write-Host "  wrote rollback script: $rollbackPath"

# ─────────────────────────────────────────────────────────────────────────────
# Phase 2 - W1.a / W1.a2 / W1.b : cut the log volume  (the actual fix under test)
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Phase 2 - W1.a/W1.a2/W1.b  engine log volume'

Write-Host '  Measured composition of the 49 MB written per 1000-message run:' -ForegroundColor DarkGray
Write-Host '    Information 39.1 MB (80%) | Warning 1.3 MB | Debug 0.2 MB | Trace 0.02 MB (44 lines)' -ForegroundColor DarkGray
Write-Host '  => EXECUTIONLOGLEVEL is the lever. The host.json Trace categories are 44 lines and are' -ForegroundColor DarkGray
Write-Host '     deliberately NOT touched.' -ForegroundColor DarkGray

Write-Step 'W1.a' ("EXECUTIONLOGLEVEL: '{0}' -> 'WARN'   (~27 MB / 55% of the volume)" -f `
    ($engineSettings['EXECUTIONLOGLEVEL'] ?? '<unset>'))
Write-Step 'W1.a2' ("logLevel__default -> 'Warning', logLevel__Host.Results -> 'Information'")
Write-Host "         Host.Results is kept ON PURPOSE: it emits the per-invocation" -ForegroundColor DarkGray
    Write-Host '         the per-invocation "Executed ... Duration=Nms" line (~450 KB/run), the only' -ForegroundColor DarkGray
Write-Host '         per-invocation timing the engine produces.' -ForegroundColor DarkGray

$logSettings = @(
    'EXECUTIONLOGLEVEL=WARN'
    'AzureFunctionsJobHost__logging__logLevel__default=Warning'
    'AzureFunctionsJobHost__logging__logLevel__Host.Results=Information'
)

if ($SkipFileLoggingMode) {
    Write-Step 'W1.b' 'SKIPPED (-SkipFileLoggingMode) - file logging stays on as a fallback diagnostic'
} else {
    Write-Step 'W1.b' "fileLoggingMode: 'always' (host.json) -> 'debugOnly'"
    Write-Host '         NOTE: this removes the host log Gate A would otherwise fall back to.' -ForegroundColor Yellow
    Write-Host '         Gate A is measured from the QueueProcessor durationMs= and the platform' -ForegroundColor DarkGray
    Write-Host '         HttpResponseTime metric, both of which are unaffected.' -ForegroundColor DarkGray
    $logSettings += 'AzureFunctionsJobHost__logging__fileLoggingMode=debugOnly'
}

Invoke-Az (@('functionapp', 'config', 'appsettings', 'set', '-n', $EngineApp, '-g', $EngineRg,
             '--settings') + $logSettings + @('-o', 'none')) 'W1.a log volume'

# ─────────────────────────────────────────────────────────────────────────────
# Phase 3 - W1.1-W1.3 / W1.5 / W2.1 / W1.d : parity + observability
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Phase 3 - W1.1-W1.3, W1.5, W2.1, W1.d  parity + observability'

Write-Step 'W1.1-3' 'dynamicConcurrency / snapshotPersistence / healthMonitor -> false (parity with UAT)'
Write-Host '         DEMOTED: 23 instance logs show "Restarting host" 0 times, so this is risk' -ForegroundColor DarkGray
Write-Host '         reduction and UAT parity, not the fix.' -ForegroundColor DarkGray

$aiConn = $engineSettings['WAREWOLF_APPINSIGHTS_CONNECTION_STRING']
if ([string]::IsNullOrWhiteSpace($aiConn)) {
    Write-Step 'W2.1' 'SKIPPED - WAREWOLF_APPINSIGHTS_CONNECTION_STRING is unset, nothing to copy from'
} else {
    Write-Step 'W2.1' 'APPLICATIONINSIGHTS_CONNECTION_STRING <- WAREWOLF_APPINSIGHTS_CONNECTION_STRING'
    Write-Host '         The engine AI resource has been empty for 30 days: the Warewolf sink is' -ForegroundColor DarkGray
    Write-Host '         configured but the FUNCTIONS HOST telemetry is not. This turns it on.' -ForegroundColor DarkGray
}

Write-Step 'W1.d' 'WAREWOLF_WORKFLOW_POOL_MAX=8 (explicit; matches the code default in ResolvePoolCap)'
Write-Host '         Set explicitly only so it is visible and tunable later. Does NOT help a cold' -ForegroundColor DarkGray
Write-Host '         instance - the first execution must still compile.' -ForegroundColor DarkGray

$paritySettings = @(
    'AzureFunctionsJobHost__concurrency__dynamicConcurrencyEnabled=false'
    'AzureFunctionsJobHost__concurrency__snapshotPersistenceEnabled=false'
    'AzureFunctionsJobHost__healthMonitor__enabled=false'
    'WAREWOLF_WORKFLOW_POOL_MAX=8'
)
if (-not [string]::IsNullOrWhiteSpace($aiConn)) {
    $paritySettings += "APPLICATIONINSIGHTS_CONNECTION_STRING=$aiConn"
}

Invoke-Az (@('functionapp', 'config', 'appsettings', 'set', '-n', $EngineApp, '-g', $EngineRg,
             '--settings') + $paritySettings + @('-o', 'none')) 'W1.1-3 / W2.1 / W1.d'

if ($engineSettings.ContainsKey('AzureWebJobsDashboard')) {
    Write-Step 'W1.5' 'delete AzureWebJobsDashboard (deprecated; one table-storage row per invocation)'
    Invoke-Az @('functionapp', 'config', 'appsettings', 'delete', '-n', $EngineApp, '-g', $EngineRg,
                '--setting-names', 'AzureWebJobsDashboard', '-o', 'none') 'W1.5'
} else {
    Write-Step 'W1.5' 'SKIPPED - AzureWebJobsDashboard already absent'
}

# ─────────────────────────────────────────────────────────────────────────────
# Phase 4 - W1.6 : Easy Auth token store off (authentication stays REQUIRED)
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Phase 4 - W1.6  Easy Auth token store'

Write-Step 'W1.6' 'token store -> false. requireAuthentication stays TRUE - auth is NOT weakened.'
Write-Host '         Hygiene only: measured 0 x Http401 and 0 x Http4xx, so Easy Auth is not the stall.' -ForegroundColor DarkGray
# '--enable-token-store', NOT '--token-store': the authV2 extension renamed it and the old
# form fails with "unrecognized arguments" rather than being ignored.
Invoke-Az @('webapp', 'auth', 'update', '-n', $EngineApp, '-g', $EngineRg,
            '--enable-token-store', 'false', '-o', 'none') 'W1.6'

# ─────────────────────────────────────────────────────────────────────────────
# Phase 5 - W4.1 / W4.4 / W4.5 / W4.6 : Container App
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Phase 5 - W4.1/W4.4/W4.5/W4.6  Container App'

Write-Host '  NOT DONE HERE, on purpose:' -ForegroundColor Yellow
Write-Host '    WORKER__MAXCONCURRENCY is left at 6. Raising it is INERT while the baked-in trigger' -ForegroundColor Yellow
Write-Host '    Prefetch is 6 - the broker caps unacked deliveries per consumer (plan 5.0.1).' -ForegroundColor Yellow

Write-Step 'W4.1' ("maxReplicas: {0} -> 2   (minReplicas stays {1})" -f $scale.maxReplicas, $scale.minReplicas)
Write-Host '         CAVEAT: Deploy-WwQueueProcessor.ps1 DERIVES maxReplicas from the trigger .bite' -ForegroundColor Yellow
Write-Host "         Concurrency field (currently 1), so the next deploy reverts this unless you" -ForegroundColor Yellow
Write-Host '         pass -MaxReplicas 2 explicitly.' -ForegroundColor Yellow

Write-Step 'W4.4' ("cpu/memory: {0}/{1} -> {2}/{3}  (the deploy script's own defaults)" -f `
    $res.cpu, $res.memory, $Cpu, $Memory)

Invoke-Az @('containerapp', 'update', '-n', $ContainerApp, '-g', $ContainerRg,
            '--max-replicas', '2', '--cpu', $Cpu, '--memory', $Memory, '-o', 'none') 'W4.1/W4.4'

Write-Step 'W4.6' ("scale.pollingInterval: {0}s -> {1}s" -f $scale.pollingInterval, $PollingInterval)
Write-Host '         pollingInterval is a TEMPLATE property, so --set-env-vars cannot carry it and' -ForegroundColor DarkGray
Write-Host '         there is no CLI flag - it is patched through the ARM resource directly. This is' -ForegroundColor DarkGray
Write-Host '         the same class of trap as -TerminationGracePeriodSeconds, which was inert until' -ForegroundColor DarkGray
Write-Host '         2026-08-11 for exactly this reason.' -ForegroundColor DarkGray

$caId = az containerapp show -n $ContainerApp -g $ContainerRg --query id -o tsv
Invoke-Az @('resource', 'update', '--ids', $caId,
            '--set', "properties.template.scale.pollingInterval=$PollingInterval",
            '-o', 'none') 'W4.6'

Write-Step 'W4.5' 'KEDA value left at 6 to match the unchanged MaxConcurrency/Prefetch of 6'
Write-Host '         Raise it only in lockstep with MaxConcurrency AND Prefetch, or KEDA starts' -ForegroundColor DarkGray
Write-Host '         replicas that find an empty queue.' -ForegroundColor DarkGray

# ─────────────────────────────────────────────────────────────────────────────
# Phase 6 - verification
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Phase 6 - verify'

if (-not $Execute) {
    Write-Host '  (dry run - nothing to verify)' -ForegroundColor DarkYellow
} else {
    Write-Host '  Effective engine settings:'
    az functionapp config appsettings list -n $EngineApp -g $EngineRg `
        --query "[?starts_with(name,'AzureFunctionsJobHost') || name=='EXECUTIONLOGLEVEL' || name=='WAREWOLF_WORKFLOW_POOL_MAX' || name=='APPLICATIONINSIGHTS_CONNECTION_STRING'].{name:name}" `
        -o table

    Write-Host '  Container App scale/resources:'
    az containerapp show -n $ContainerApp -g $ContainerRg `
        --query '{min:properties.template.scale.minReplicas,max:properties.template.scale.maxReplicas,poll:properties.template.scale.pollingInterval,cpu:properties.template.containers[0].resources.cpu,mem:properties.template.containers[0].resources.memory}' `
        -o table
}

Write-Host ''
Write-Host 'MANDATORY POST-CHECK before trusting a Gate A result' -ForegroundColor White
Write-Host '  1. The AzureFunctionsJobHost__* override mechanism is NOT self-evident. StartAsAzureFunction.ps1'
Write-Host '     records that these overrides did not take effect under local func.exe. On a real Function'
Write-Host '     App it is the supported mechanism and UAT relies on it - but CONFIRM the effective log'
Write-Host '     levels changed before concluding W1.a had no effect. If the engine log volume for the'
Write-Host '     next run is still ~49 MB, the override did not apply and the experiment is void.'
Write-Host '  2. Confirm the replica reports matching numbers in its startup line:'
Write-Host "       Consuming queue 'order-success-queue' (prefetch 6, maxConcurrency 6, ...)"
Write-Host '     If prefetch and maxConcurrency ever differ, that run is void (plan section 5.0.1).'
Write-Host ''
Write-Host 'GATE A - measure from OUTSIDE the engine, since the fix removes the engine-side evidence:'
Write-Host '  1. QueueProcessor durationMs= in Log Analytics: calls > 20 s must fall below 2%.'
Write-Host '  2. Platform metric HttpResponseTime MAXIMUM must no longer sit at ~30 s.'
Write-Host ''
Write-Host "Rollback: $rollbackPath" -ForegroundColor White
Write-Host ''
