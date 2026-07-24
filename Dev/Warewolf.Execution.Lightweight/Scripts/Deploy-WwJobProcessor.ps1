#Requires -Version 7.0
<#
.SYNOPSIS
    Deploys the Warewolf ExecutionEngineJobProcessor — the dedicated Azure Function
    App (Warewolf.Execution.EngineJobProcessor) that polls Hangfire SQL storage for
    due Scheduled suspend/resume jobs and fire-and-forget POSTs them to the Execution
    Engine's secured /secure/resume/{jobId} route, and reaps stale Processing jobs to
    Failed (fail-only). Replaces the on-prem hangfireserver.exe worker loop.

.DESCRIPTION
    A self-contained orchestrator that mirrors Deploy-WwExecutionEngine.ps1's
    conventions (Write-Phase helpers, Invoke-Az wrapper, masked summary + transcript,
    "params first, prompt if missing", -DryRun) but is scoped to the processor:

      * NO secure.config / workflow resources / Elasticsearch (the processor exposes
        only timer triggers — it authenticates OUTBOUND to the engine via managed
        identity; it has no inbound HTTP routes of its own to protect).
      * Stages the SAME persistence settings pair as the engine
        (persistencesettings.json + persistencesettingsdbsource.bite) into Settings\,
        WFAES-encrypting the DbSource ConnectionString exactly like the engine's
        Elasticsearch source. Both source files are PROMPTED when not passed.
      * Applies the processor's app settings (JOB_POLL_SCHEDULE / JOB_REAPER_SCHEDULE /
        JOB_STALE_MINUTES + ENGINE_RESUME_BASEURL / ENGINE_RESUME_SCOPE /
        ENGINE_RESUME_TIMEOUT_SECONDS / ENGINE_RESUME_AUTH_DISABLED) and the Key Vault
        settings the persistence DbSource decryption needs.

    ROLE REGISTRATION is a SEPARATE operator step (mirrors the engine): grant the
    processor's system-assigned managed identity the engine app role Warewolf_JobProcessor
    with
        Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity `
            -DaemonFunctionAppName <thisApp> -DaemonFunctionAppResourceGroup <rg> `
            -AppRolesToAssign Warewolf_JobProcessor
    and add the matching global-scope Execute row to the engine's secure.config
    (the two-part authorization contract — see docs/Deploy-EndToEnd-Runbook.md).

.PARAMETER SubscriptionId
    Azure subscription. Resolved from `az account show` when omitted.
.PARAMETER TenantId
    Entra tenant. Resolved from `az account show` when omitted.
.PARAMETER ResourceGroup
    REQUIRED (no default). Target resource group.
.PARAMETER Location
    REQUIRED (no default). Azure region, e.g. southafricanorth.
.PARAMETER StorageAccount
    REQUIRED (no default). Storage account backing the Function App (3-24 lowercase).
.PARAMETER AppName
    REQUIRED (no default). Globally-unique Function App name for the processor.
.PARAMETER PublishPath
    REQUIRED. Folder or .zip of the processor's Release publish output
    (dotnet publish Warewolf.Execution.EngineJobProcessor -c Release -o <path>).
.PARAMETER PublishMethod
    Auto (zip-deploy, default) | Func | Zip.
.PARAMETER AppInsightsName / EnableAppInsights
    Application Insights (defaults to "<AppName>-ai"; enabled by default).
.PARAMETER PersistenceSettingsPath
    persistencesettings.json (Enable/scheduler/flags). PROMPTED when omitted.
.PARAMETER PersistenceDbSourcePath
    persistencesettingsdbsource.bite (Hangfire SQL DbSource; ConnectionString
    WFAES-encrypted). PROMPTED when omitted.
.PARAMETER EncryptResources / VerifyDecryption / KeyVaultName / KeyVaultSecretName / GenerateNewKey
    Same WFAES/Key Vault contract as the engine deploy — the DbSource ConnectionString
    is encrypted with the engine's Key Vault AES key so the processor decrypts it at
    runtime through the shared DpapiWrapper.AesDecryptHook.
.PARAMETER JobPollSchedule / JobReaperSchedule / JobStaleMinutes
    Timer cadences (NCRONTAB) and the stale-Processing threshold (minutes).
.PARAMETER EngineResumeBaseUrl / EngineResumeScope / EngineResumeTimeoutSeconds / EngineResumeAuthDisabled
    The engine resume dispatch target, the MI token scope, the ack timeout, and the
    dev-only auth bypass.
.PARAMETER LogDir / NonInteractive / DryRun / LoadFunctionsOnly
    Logging output dir; unattended mode; preview-without-change; test hook.

.NOTES
    Publish first (this script does NOT build):
      dotnet publish Dev/Warewolf.Execution.EngineJobProcessor/Warewolf.Execution.EngineJobProcessor.csproj -c Release -o D:\JobProcessor\Publish
#>
[CmdletBinding()]
param(
    # Targeting (REQUIRED — no defaults; prompted when interactive)
    [string] $SubscriptionId,
    [string] $TenantId,
    [string] $ResourceGroup,
    [string] $Location,
    [string] $StorageAccount,
    [string] $AppName,
    # Publish source (folder or .zip)
    [string] $PublishPath,
    [ValidateSet('Auto','Func','Zip')] [string] $PublishMethod = 'Auto',
    # Application Insights
    [string] $AppInsightsName,
    [nullable[bool]] $EnableAppInsights,
    # Persistence settings pair (PROMPTED when not passed)
    [string] $PersistenceSettingsPath,
    [string] $PersistenceDbSourcePath,
    # Encryption / Key Vault
    [nullable[bool]] $EncryptResources,
    [switch] $VerifyDecryption,
    [string] $KeyVaultName,
    [string] $KeyVaultSecretName,
    [switch] $GenerateNewKey,
    # JobProcessor app settings (poller + reaper + resume dispatch)
    [string] $JobPollSchedule = '0 */1 * * * *',
    [string] $JobReaperSchedule = '0 */5 * * * *',
    [nullable[int]] $JobStaleMinutes,
    [string] $EngineResumeBaseUrl,
    [string] $EngineResumeScope,
    [nullable[int]] $EngineResumeTimeoutSeconds,
    [nullable[bool]] $EngineResumeAuthDisabled,
    # Logging / feature env vars
    [nullable[bool]] $EnableConsoleLogging,
    [ValidateSet('TRACE','DEBUG','INFO','WARN','ERROR','FATAL','OFF')] [string] $ExecutionLogLevel = 'INFO',
    [nullable[bool]] $StructuredLogs,
    # Logging output / control
    [string] $LogDir,
    [switch] $NonInteractive,
    [switch] $DryRun,
    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers  (kept byte-for-byte consistent with Deploy-WwExecutionEngine.ps1)
# ════════════════════════════════════════════════════════════════════════════

function Write-Phase {
    param([string] $Title)
    Write-Host ''
    Write-Host ('═' * 76) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ('═' * 76) -ForegroundColor Cyan
}

function Write-Step { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }

function Test-CommandExists {
    param([string] $Name)
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

function Confirm-Yes {
    param([string] $Message, [bool] $DefaultYes = $true)
    if ($NonInteractive) { return $DefaultYes }
    $suffix = if ($DefaultYes) { '[Y/n]' } else { '[y/N]' }
    $answer = Read-Host "  $Message $suffix"
    if ([string]::IsNullOrWhiteSpace($answer)) { return $DefaultYes }
    return $answer -imatch '^y'
}

function Read-Required {
    <#
        Return $Current if set; otherwise prompt (interactive) or throw
        (non-interactive) for a value the pipeline cannot proceed without.
    #>
    param([string] $Name, [string] $Current, [string] $Hint)
    if (-not [string]::IsNullOrWhiteSpace($Current)) { return $Current }
    if ($NonInteractive) {
        throw "Required value '$Name' was not supplied. Pass -$Name <value> (running with -NonInteractive)."
    }
    $hintText = if ($Hint) { " ($Hint)" } else { '' }
    do {
        $v = Read-Host "  Enter $Name$hintText"
    } while ([string]::IsNullOrWhiteSpace($v))
    return $v.Trim()
}

function Resolve-Toggle {
    param([string] $Name, [nullable[bool]] $Current, [bool] $Default, [string] $Prompt)
    if ($null -ne $Current) { return [bool]$Current }
    if ($NonInteractive)    { return $Default }
    $msg = if ($Prompt) { $Prompt } else { "Enable $Name" }
    return (Confirm-Yes $msg $Default)
}

function Get-MaskedValue {
    param([string] $Value)
    if ([string]::IsNullOrEmpty($Value)) { return '' }
    if ($Value.Length -le 6) { return '******' }
    return ('{0}…(masked, len={1})' -f $Value.Substring(0, 3), $Value.Length)
}

function Format-AzArgsForLog {
    param([string[]] $Arguments)
    $flagRegex = '(?i)^(--password|--client-secret|--secret)$'
    $secretNameRegex = '(?i)((^|_)(PASSWORD|SECRET|TOKEN|KEY)$|CONNECTION_?STRING)'
    $rendered  = New-Object System.Collections.Generic.List[string]
    $maskNext  = $false
    foreach ($a in $Arguments) {
        if ($maskNext)            { $rendered.Add('***REDACTED***'); $maskNext = $false; continue }
        if ($a -match $flagRegex) { $rendered.Add($a); $maskNext = $true; continue }
        if ($a -match '^([A-Za-z0-9_.\-]+)=(.+)$') {
            $name = $Matches[1]
            if ($name -match $secretNameRegex) { $rendered.Add("$name=***REDACTED***") } else { $rendered.Add($a) }
            continue
        }
        $rendered.Add($a)
    }
    return ($rendered -join ' ')
}

function Invoke-Az {
    <#
        Thin wrapper over the az CLI.
          -Mutating  : changes cloud state; skipped (echoed) under -DryRun.
          -AllowFail : non-zero exit returns $null instead of throwing (probes).
        Logged/echoed args are secret-redacted via Format-AzArgsForLog.
    #>
    param(
        [Parameter(Mandatory)][string[]] $Args,
        [switch] $Mutating,
        [switch] $AllowFail
    )
    if ($Mutating -and $DryRun) {
        Write-Host "      [DRYRUN] az $(Format-AzArgsForLog $Args)" -ForegroundColor DarkGray
        return $null
    }
    $out = & az @Args 2>&1
    if ($LASTEXITCODE -ne 0) {
        if ($AllowFail) { return $null }
        throw "az CLI failed ($LASTEXITCODE): az $(Format-AzArgsForLog $Args)`n$($out | Out-String)"
    }
    return $out
}

function Invoke-ChildScript {
    param(
        [Parameter(Mandatory)][string] $Path,
        [hashtable] $Parameters = @{},
        [string] $Label
    )
    $name = if ($Label) { $Label } else { Split-Path $Path -Leaf }
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required sibling script not found: $Path"
    }
    if ($DryRun) {
        $rendered = ($Parameters.GetEnumerator() | ForEach-Object { "-$($_.Key)" }) -join ' '
        Write-Host "      [DRYRUN] & '$name' $rendered" -ForegroundColor DarkGray
        return
    }
    Write-Step "Invoking $name"
    & $Path @Parameters
    if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
        throw "$name exited with code $LASTEXITCODE."
    }
}

function Save-DeploySummary {
    <#
        Write the run summary to $summaryPath crash-safely and incrementally (atomic
        temp -> Move-Item), after every phase and from the outer catch, so a rollback
        always has an authoritative record of what THIS run created vs. found.
    #>
    param(
        [ValidateSet('in-progress', 'completed', 'failed')]
        [string] $Status = 'in-progress',
        [string] $ErrorMessage
    )
    if ([string]::IsNullOrWhiteSpace($summaryPath)) { return }

    $maskedSettings = [ordered]@{}
    foreach ($k in $appSettings.Keys) {
        $maskedSettings[$k] = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    }
    $aiConnMasked = if ($enableAppInsights) {
        if ([string]::IsNullOrEmpty($aiConnectionString)) { '<pending — created/read on a real run>' }
        elseif ($aiConnectionString -like '<pending*')     { $aiConnectionString }
        else                                               { Get-MaskedValue $aiConnectionString }
    } else { $null }

    $summary = [ordered]@{
        timestampUtc    = (Get-Date).ToUniversalTime().ToString('o')
        status          = $Status
        lastPhase       = $script:DeployLastPhase
        error           = $ErrorMessage
        dryRun          = [bool]$DryRun
        runId           = $runId
        resourceTags    = $ResourceTags
        created         = $created
        subscriptionId  = $SubscriptionId
        tenantId        = $TenantId
        resourceGroup   = $ResourceGroup
        location        = $Location
        storageAccount  = $StorageAccount
        appName         = $AppName
        appInsightsName = ($enableAppInsights ? $AppInsightsName : $null)
        appInsightsConnectionString = $aiConnMasked
        endpoint        = $baseUrl
        publishPath     = $PublishPath
        publishDir      = $StagingDir
        publishMethod   = $PublishMethod
        appInsights     = $enableAppInsights
        persistenceSettings = $PersistenceSettingsPath
        persistenceDbSource = $PersistenceDbSourcePath
        encryptResources = $doEncryptResources
        verifyDecryption = [bool]$VerifyDecryption
        keyVault        = ($kvRequired ? @{ name = $KeyVaultName; secret = $KeyVaultSecretName } : $null)
        appSettings     = $maskedSettings
    }
    $tmp = "$summaryPath.tmp"
    $summary | ConvertTo-Json -Depth 6 | Set-Content -Path $tmp -Encoding UTF8
    Move-Item -LiteralPath $tmp -Destination $summaryPath -Force
}

# ════════════════════════════════════════════════════════════════════════════
# Path resolution
# ════════════════════════════════════════════════════════════════════════════

$ScriptDir         = $PSScriptRoot
$AppInsightsScript = Join-Path $ScriptDir 'Setup-ApplicationInsights.ps1'
$EncryptScript     = Join-Path $ScriptDir 'Encrypt-Config.ps1'

$PersistenceSettingsName = 'persistencesettings.json'
$PersistenceDbSourceName = 'persistencesettingsdbsource.bite'

# Test hook: stop here when only the helper functions are wanted (Pester).
if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight'

if (-not (Test-CommandExists 'az')) {
    throw 'Azure CLI (az) not found. Install: https://aka.ms/InstallAzureCli'
}
$acct = az account show -o json 2>$null | ConvertFrom-Json
if (-not $acct) {
    throw 'Not logged in to Azure. Run: az login'
}
if (-not $SubscriptionId) { $SubscriptionId = $acct.id }
if (-not $TenantId)       { $TenantId       = $acct.tenantId }
Write-Ok "Subscription $SubscriptionId (tenant $TenantId)"

# ════════════════════════════════════════════════════════════════════════════
# Phase 0.5 — PLAN  (resolve every decision; no cloud/state change yet)
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0.5  Plan (resolve all settings before any change)'

$runId        = "wwjp-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$ResourceTags = @("wwx-test-run=$runId", 'wwx-purpose=jobprocessor-live-test')
$created      = [ordered]@{}

# ── Targeting (required) ──────────────────────────────────────────────────────
$AppName        = Read-Required -Name 'AppName'        -Current $AppName        -Hint 'globally-unique Function App name'
$ResourceGroup  = Read-Required -Name 'ResourceGroup'  -Current $ResourceGroup
$Location       = Read-Required -Name 'Location'       -Current $Location        -Hint 'e.g. southafricanorth'
$StorageAccount = Read-Required -Name 'StorageAccount' -Current $StorageAccount  -Hint '3-24 lowercase chars'
if (-not $AppInsightsName) { $AppInsightsName = "$AppName-ai" }

# ── Publish source (folder or .zip) ───────────────────────────────────────────
$PublishPath = Read-Required -Name 'PublishPath' -Current $PublishPath -Hint 'folder or .zip of the Release publish output'
if (-not (Test-Path -LiteralPath $PublishPath)) {
    throw "PublishPath not found: $PublishPath"
}
$publishItem  = Get-Item -LiteralPath $PublishPath
$publishIsZip = (-not $publishItem.PSIsContainer) -and ($publishItem.Extension -ieq '.zip')
if ($publishItem.PSIsContainer) {
    $PublishDir = $publishItem.FullName
} elseif ($publishIsZip) {
    $PublishDir = Join-Path $publishItem.DirectoryName $publishItem.BaseName
} else {
    throw "PublishPath must be a folder or a .zip file: $PublishPath"
}

if (-not $LogDir) { $LogDir = Join-Path (Split-Path $PublishDir -Parent) 'deploy-logs' }

# ── Feature toggles ────────────────────────────────────────────────────────────
$enableAppInsights  = Resolve-Toggle -Name 'EnableAppInsights'   -Current $EnableAppInsights   -Default $true  -Prompt 'Provision + enable Application Insights?'
$enableConsole      = Resolve-Toggle -Name 'EnableConsoleLogging' -Current $EnableConsoleLogging -Default $true  -Prompt 'Enable console logging?'
$structuredLogs     = Resolve-Toggle -Name 'StructuredLogs'      -Current $StructuredLogs       -Default $true  -Prompt 'Structured (JSON) console logs?'
$doEncryptResources = Resolve-Toggle -Name 'EncryptResources'    -Current $EncryptResources    -Default $false -Prompt 'Encrypt the persistence DbSource ConnectionString now? (encrypt once; leave off if already encrypted)'
$authDisabled       = Resolve-Toggle -Name 'EngineResumeAuthDisabled' -Current $EngineResumeAuthDisabled -Default $false -Prompt 'DISABLE resume-route auth (development only)?'

# ── Key Vault requirement (so the DbSource ConnectionString decrypts at runtime) ─
# Same contract as the engine deploy: the vault is wired whenever this run encrypts
# OR a vault name was supplied (already-encrypted .bite still needs runtime decrypt).
$kvRequired = $doEncryptResources -or (-not [string]::IsNullOrWhiteSpace($KeyVaultName))
if ($kvRequired) {
    $KeyVaultName       = Read-Required -Name 'KeyVaultName'       -Current $KeyVaultName       -Hint 'Key Vault the processor decrypts the DbSource with (3-24 chars)'
    $KeyVaultSecretName = Read-Required -Name 'KeyVaultSecretName' -Current $KeyVaultSecretName -Hint 'AES key secret name (must match the engine)'
}

# ── Persistence settings pair (PROMPTED when not passed; exact filenames) ───────
$PersistenceSettingsPath = Read-Required -Name 'PersistenceSettingsPath' -Current $PersistenceSettingsPath -Hint "path to $PersistenceSettingsName"
if (-not (Test-Path -LiteralPath $PersistenceSettingsPath -PathType Leaf)) {
    throw "PersistenceSettingsPath not found (must be a file): $PersistenceSettingsPath"
}
$psLeaf = Split-Path $PersistenceSettingsPath -Leaf
if ($psLeaf -ine $PersistenceSettingsName) {
    throw "Persistence settings must be named exactly '$PersistenceSettingsName' (the app reads that exact path); got '$psLeaf'."
}

$PersistenceDbSourcePath = Read-Required -Name 'PersistenceDbSourcePath' -Current $PersistenceDbSourcePath -Hint "path to $PersistenceDbSourceName"
if (-not (Test-Path -LiteralPath $PersistenceDbSourcePath -PathType Leaf)) {
    throw "PersistenceDbSourcePath not found (must be a file): $PersistenceDbSourcePath"
}
$dbLeaf = Split-Path $PersistenceDbSourcePath -Leaf
if ($dbLeaf -ine $PersistenceDbSourceName) {
    throw "Persistence DbSource must be named exactly '$PersistenceDbSourceName' (the app reads that exact path); got '$dbLeaf'."
}

# ── Resume dispatch settings ────────────────────────────────────────────────────
$EngineResumeBaseUrl = Read-Required -Name 'EngineResumeBaseUrl' -Current $EngineResumeBaseUrl -Hint 'Execution Engine base URL, e.g. https://<engine>.azurewebsites.net'
if (-not $authDisabled) {
    $EngineResumeScope = Read-Required -Name 'EngineResumeScope' -Current $EngineResumeScope -Hint 'MI token scope, e.g. api://<engine-app-id>/.default'
}
$resumeTimeout = if ($null -ne $EngineResumeTimeoutSeconds) { [int]$EngineResumeTimeoutSeconds } else { 15 }
$staleMinutes  = if ($null -ne $JobStaleMinutes)            { [int]$JobStaleMinutes }            else { 15 }

# ── Build the environment-variable (app settings) map ──────────────────────────
$appSettings = [ordered]@{}
$secretSettingNames = New-Object System.Collections.Generic.HashSet[string]

$appSettings['ASPNETCORE_ENVIRONMENT']     = 'Production'
$appSettings['EXECUTIONLOGLEVEL']          = $ExecutionLogLevel
$appSettings['ENABLECONSOLELOGGING']       = ($enableConsole ? 'true' : 'false')
$appSettings['STRUCTURED_LOGS']            = ($structuredLogs ? 'true' : 'false')
$appSettings['ENABLEAPPLICATIONINSIGHTS']  = ($enableAppInsights ? 'true' : 'false')
# Poller + reaper cadences and the stale-Processing threshold.
$appSettings['JOB_POLL_SCHEDULE']          = $JobPollSchedule
$appSettings['JOB_REAPER_SCHEDULE']        = $JobReaperSchedule
$appSettings['JOB_STALE_MINUTES']          = "$staleMinutes"
# Resume dispatch (processor -> engine /secure/resume/{jobId}).
$appSettings['ENGINE_RESUME_BASEURL']         = $EngineResumeBaseUrl
$appSettings['ENGINE_RESUME_TIMEOUT_SECONDS'] = "$resumeTimeout"
$appSettings['ENGINE_RESUME_AUTH_DISABLED']   = ($authDisabled ? 'true' : 'false')
if (-not $authDisabled) {
    $appSettings['ENGINE_RESUME_SCOPE'] = $EngineResumeScope
}
# Key Vault (so the DbSource ConnectionString decrypts at runtime — matches the engine).
if ($kvRequired) {
    $appSettings['AZURE_KEYVAULT_NAME']  = $KeyVaultName
    $appSettings['KEYVAULT_SECRET_NAME'] = $KeyVaultSecretName
}

# ── Settings summary + single confirmation ─────────────────────────────────────
Write-Host ''
Write-Host '  ── Resolved deployment settings ──────────────────────────────────────' -ForegroundColor White
Write-Host ("    {0,-28}: {1}" -f 'SubscriptionId', $SubscriptionId)
Write-Host ("    {0,-28}: {1}" -f 'TenantId', $TenantId)
Write-Host ("    {0,-28}: {1}" -f 'ResourceGroup', $ResourceGroup)
Write-Host ("    {0,-28}: {1}" -f 'Location', $Location)
Write-Host ("    {0,-28}: {1}" -f 'StorageAccount', $StorageAccount)
Write-Host ("    {0,-28}: {1}" -f 'AppName', $AppName)
Write-Host ("    {0,-28}: {1}" -f 'PublishPath', $PublishPath)
Write-Host ("    {0,-28}: {1}" -f 'PublishDir', ($publishIsZip ? "$PublishDir  (extracted from zip)" : $PublishDir))
Write-Host ("    {0,-28}: {1}" -f 'PublishMethod', $PublishMethod)
Write-Host ("    {0,-28}: {1}" -f 'App Insights', ($enableAppInsights ? "enabled ($AppInsightsName)" : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'persistencesettings.json', $PersistenceSettingsPath)
Write-Host ("    {0,-28}: {1}" -f 'persistence DbSource .bite', $PersistenceDbSourcePath)
Write-Host ("    {0,-28}: {1}" -f 'Encrypt DbSource (this run)', ($doEncryptResources ? 'YES (WFAES via Key Vault)' : 'no (staged as-is; assumed already encrypted)'))
Write-Host ("    {0,-28}: {1}" -f 'Verify decryption', ($doEncryptResources ? ($VerifyDecryption ? 'yes (in-memory)' : 'no') : 'n/a'))
if ($kvRequired) {
    $kvPurpose = $doEncryptResources ? 'encrypt now + runtime decrypt' : 'runtime decrypt of already-encrypted DbSource'
    Write-Host ("    {0,-28}: {1}" -f 'Key Vault', "$KeyVaultName / secret '$KeyVaultSecretName' ($kvPurpose)")
}
Write-Host ("    {0,-28}: {1}" -f 'Resume auth', ($authDisabled ? 'DISABLED (dev only)' : 'enabled (MI bearer token)'))
Write-Host ("    {0,-28}: {1}" -f 'LogDir', $LogDir)
Write-Host ("    {0,-28}: {1}" -f 'Run tag', "wwx-test-run=$runId  (rollback targets this tag only)")
Write-Host ("    {0,-28}: {1}" -f 'DryRun', $DryRun)
Write-Host ''
Write-Host '  ── Environment variables (App Settings) to apply ─────────────────────' -ForegroundColor White
foreach ($k in $appSettings.Keys) {
    $shown = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    Write-Host ("    {0,-34}= {1}" -f $k, $shown)
}
if ($enableAppInsights) {
    Write-Host ("    {0,-34}= {1}" -f 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING', '(auto-read from the App Insights resource)')
}
Write-Host ''
Write-Note 'Role registration is a SEPARATE step: grant this app''s managed identity the engine'
Write-Note 'role Warewolf_JobProcessor (Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon'
Write-Note '-DaemonUseManagedIdentity -AppRolesToAssign Warewolf_JobProcessor) + the matching'
Write-Note 'global Execute row in the engine''s secure.config. See docs/Deploy-EndToEnd-Runbook.md.'
Write-Host ''

if (-not (Confirm-Yes 'Proceed with this deployment?' $true)) {
    Write-Note 'Aborted by user.'
    return
}

# ════════════════════════════════════════════════════════════════════════════
# Begin work — transcript log + incremental summary (dry-run and real runs).
# ════════════════════════════════════════════════════════════════════════════

$runStamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$transcriptOn = $false
if (-not (Test-Path -LiteralPath $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$logInfix    = if ($DryRun) { 'dryrun.' } else { '' }
$logFile     = Join-Path $LogDir "deploy-WwJobProcessor-$runStamp.${logInfix}log"
$summaryPath = Join-Path $LogDir "deploy-WwJobProcessor-$runStamp.${logInfix}summary.json"
try { Start-Transcript -Path $logFile -Append | Out-Null; $transcriptOn = $true; Write-Ok "Logging to $logFile" }
catch { Write-Note "Transcript not started (an outer transcript may be active): $($_.Exception.Message)" }

$baseUrl                = "https://$AppName.azurewebsites.net"
$StagingDir             = $PublishDir
$aiConnectionString     = $null
$script:DeployLastPhase = 'Phase 0.5  Plan'
$script:GenerateNewKey  = [bool]$GenerateNewKey

Save-DeploySummary -Status 'in-progress'

try {
    Invoke-Az @('account', 'set', '--subscription', $SubscriptionId) -Mutating | Out-Null

    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Infrastructure
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Infrastructure (resource group, storage, function app, App Insights)'
    $script:DeployLastPhase = 'Phase 1  Infrastructure'

    # 1.1 Resource group
    $rgExists = (Invoke-Az @('group', 'exists', '--name', $ResourceGroup)) -join ''
    if ($rgExists -eq 'true') {
        Write-Ok "Resource group '$ResourceGroup' already exists."
        $created['resourceGroup'] = $false
    } else {
        $created['resourceGroup'] = $true   # intent recorded BEFORE creating (crash-safe rollback)
        Write-Step "Creating resource group '$ResourceGroup' in '$Location'"
        Invoke-Az (@('group', 'create', '--name', $ResourceGroup, '--location', $Location, '--tags') + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Resource group created.'
    }

    # 1.2 Storage account
    $stExists = Invoke-Az @('storage', 'account', 'show', '--name', $StorageAccount, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($stExists) {
        Write-Ok "Storage account '$StorageAccount' already exists."
        $created['storageAccount'] = $false
    } else {
        $created['storageAccount'] = $true   # intent recorded BEFORE creating
        Write-Step "Creating storage account '$StorageAccount'"
        Invoke-Az (@(
            'storage', 'account', 'create',
            '--name', $StorageAccount, '--resource-group', $ResourceGroup, '--location', $Location,
            '--sku', 'Standard_LRS', '--kind', 'StorageV2', '--min-tls-version', 'TLS1_2', '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Storage account created.'
    }

    # 1.3 Function App (Consumption Y1, .NET 8 isolated, Functions v4)
    $appExists = Invoke-Az @('functionapp', 'show', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($appExists) {
        Write-Ok "Function App '$AppName' already exists."
        $created['functionApp'] = $false
    } else {
        $created['functionApp'] = $true   # intent recorded BEFORE creating
        Write-Step "Creating Function App '$AppName' (Consumption Y1, dotnet-isolated 8, Functions v4)"
        Invoke-Az (@(
            'functionapp', 'create',
            '--name', $AppName, '--resource-group', $ResourceGroup, '--consumption-plan-location', $Location,
            '--storage-account', $StorageAccount, '--runtime', 'dotnet-isolated', '--runtime-version', '8',
            '--functions-version', '4', '--https-only', 'true', '--os-type', 'Windows', '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Function App created.'
    }

    # 1.4 Application Insights
    $aiConnectionString = $null
    if ($enableAppInsights) {
        $aiPreExists = [bool](Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail)
        $created['appInsights'] = (-not $aiPreExists)   # intent recorded BEFORE provisioning
        Invoke-ChildScript -Path $AppInsightsScript -Label 'Setup-ApplicationInsights.ps1' -Parameters @{
            ResourceGroup   = $ResourceGroup
            FunctionAppName = $AppName
            Location        = $Location
            AppInsightsName = $AppInsightsName
        }
        if ($created['appInsights'] -and -not $DryRun) {
            Invoke-Az (@('resource', 'tag', '--resource-group', $ResourceGroup, '--name', $AppInsightsName, '--resource-type', 'microsoft.insights/components', '--tags') + $ResourceTags) -Mutating | Out-Null
        }
        $aiConnectionString = (Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '--query', 'connectionString', '-o', 'tsv') -AllowFail) -join ''
        if ([string]::IsNullOrWhiteSpace($aiConnectionString)) { $aiConnectionString = '<pending — created/read on a real run>' }
        Write-Ok ("Application Insights configured ({0} existing component)." -f ($aiPreExists ? 'reused' : 'created'))
    } else {
        Write-Note 'Application Insights disabled.'
    }

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 2 — Key Vault wiring + managed identity
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Key Vault + managed identity'
    $script:DeployLastPhase = 'Phase 2  Key Vault + managed identity'

    # The processor's system-assigned MI is REQUIRED: it authenticates to the engine's
    # resume route (role Warewolf_JobProcessor) and reads the AES key from Key Vault.
    Write-Step 'Enabling system-assigned managed identity on the Function App'
    $idJson = Invoke-Az @('functionapp', 'identity', 'assign', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -Mutating
    $funcPrincipalId = if ($DryRun) { '<dryrun-principal-id>' } else { ($idJson | ConvertFrom-Json).principalId }

    $keyReachable = $false
    if ($kvRequired) {
        $vaultExists = [bool](Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '-o', 'json') -AllowFail)
        $created['keyVault'] = (-not $vaultExists)   # intent recorded BEFORE creating
        if (-not $vaultExists) {
            if (-not $doEncryptResources) {
                throw ("Key Vault '$KeyVaultName' does not exist, and -EncryptResources was not set. " +
                       "A vault is required so the processor can decrypt the already-encrypted DbSource at runtime. " +
                       "Run once with -EncryptResources to create the vault + key, or supply the engine's existing vault.")
            }
            Write-Step "Creating Key Vault '$KeyVaultName' (RBAC authorization)"
            Invoke-Az (@('keyvault', 'create', '--name', $KeyVaultName, '--resource-group', $ResourceGroup, '--location', $Location, '--enable-rbac-authorization', 'true', '--tags') + $ResourceTags) -Mutating | Out-Null
            $script:GenerateNewKey = $true
        }
        $vaultId = if ($DryRun) { '<dryrun-vault-id>' } else { (Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '--query', 'id', '-o', 'tsv')) -join '' }

        # ALWAYS: the processor's managed identity must read the key at runtime.
        Write-Step 'Assigning RBAC: Key Vault Secrets User -> Function App managed identity'
        Invoke-Az @('role', 'assignment', 'create', '--role', 'Key Vault Secrets User', '--assignee', $funcPrincipalId, '--scope', $vaultId) -Mutating | Out-Null

        # ONLY when encrypting now: the operator needs Secrets Officer to read/write the key.
        if ($doEncryptResources) {
            $devOid = if ($DryRun) { '<dryrun-dev-oid>' } else { (Invoke-Az @('ad', 'signed-in-user', 'show', '--query', 'id', '-o', 'tsv')) -join '' }
            Write-Step 'Assigning RBAC: Key Vault Secrets Officer -> current user (for encryption)'
            Invoke-Az @('role', 'assignment', 'create', '--role', 'Key Vault Secrets Officer', '--assignee', $devOid, '--scope', $vaultId) -Mutating | Out-Null
            if (-not $DryRun) { Write-Note 'RBAC propagation can take ~1-2 min before the secret can be written.'; Start-Sleep -Seconds 30 }
        }

        # Whether REAL encryption can run now (dry-run only when vault + secret already exist).
        if (-not $DryRun) {
            $keyReachable = $true
        } elseif ([bool](Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '-o', 'json') -AllowFail)) {
            $keyReachable = [bool](Invoke-Az @('keyvault', 'secret', 'show', '--vault-name', $KeyVaultName, '--name', $KeyVaultSecretName, '-o', 'json') -AllowFail)
        }
    } else {
        Write-Note 'No Key Vault in play — the DbSource ConnectionString must be plaintext (development only).'
    }

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 3 — Stage the persistence settings pair + apply app settings
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Stage persistence settings + environment variables'
    $script:DeployLastPhase = 'Phase 3  Stage + env'

    # 3.0 Resolve the STAGING directory — a FRESH, dedicated copy under the OS temp dir
    # that we prepare and (on a real run) upload. The processor's publish OUTPUT is NEVER
    # mutated, so the zip is built cleanly every run and the processor stages in a
    # SEPARATE directory from the engine (different app + different publish tree).
    #   real run -> removed after a successful upload (Phase 4).
    #   dry run  -> kept as the inspectable preview artifact (path printed at the end).
    $stageSuffix = if ($DryRun) { '-dryrun' } else { '' }
    $StagingDir  = Join-Path ([System.IO.Path]::GetTempPath()) "wwjobprocessor-stage-$AppName-$runStamp$stageSuffix"
    Write-Step "$($DryRun ? 'Dry-run: building preview artifact' : 'Staging deploy artifact') in '$StagingDir' (your publish output is left untouched)"
    if (Test-Path -LiteralPath $StagingDir) { Remove-Item -LiteralPath $StagingDir -Recurse -Force }
    New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null
    if ($publishIsZip) {
        Expand-Archive -LiteralPath $PublishPath -DestinationPath $StagingDir -Force
    } else {
        Copy-Item -Path (Join-Path $PublishDir '*') -Destination $StagingDir -Recurse -Force
    }
    Write-Ok "Publish output copied to staging dir '$StagingDir'."

    # Local helper: encrypt a path, then OPTIONALLY prove the engine's key decrypts it
    # (verification is IN MEMORY — no plaintext on disk).
    function Invoke-EncryptAndVerify {
        param([string] $TargetPath, [string] $Label)
        $encryptParams = @{ FilePath = $TargetPath; VaultName = $KeyVaultName; SecretName = $KeyVaultSecretName; NonInteractive = $true; NoBackup = $true }
        if ($script:GenerateNewKey) { $encryptParams['GenerateKeys'] = $true }
        Write-Step "Encrypting ($Label) via Key Vault '$KeyVaultName'"
        & $EncryptScript @encryptParams
        if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "Encrypt-Config.ps1 ($Label) failed ($LASTEXITCODE)." }
        if ($VerifyDecryption) {
            & $EncryptScript -FilePath $TargetPath -VaultName $KeyVaultName -SecretName $KeyVaultSecretName -VerifyOnly -NonInteractive
            if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "Encrypt-Config.ps1 (verify $Label) failed ($LASTEXITCODE)." }
            Write-Ok "Verified (in-memory) the engine's Key Vault key decrypts: $Label"
        } else {
            Write-Note "Decrypt verification skipped for $Label (pass -VerifyDecryption to enable)."
        }
        $script:GenerateNewKey = $false   # only the first encryption generates the key
    }

    $settingsDir = Join-Path $StagingDir 'Settings'
    if (-not (Test-Path -LiteralPath $settingsDir)) { New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null }

    # 3.1 persistencesettings.json — staged AS-IS (flags/enable; no secrets in it).
    $psDest = Join-Path $settingsDir $PersistenceSettingsName
    Write-Step "Staging '$PersistenceSettingsName' -> '$psDest'"
    Copy-Item -LiteralPath $PersistenceSettingsPath -Destination $psDest -Force
    Write-Ok 'persistencesettings.json staged.'

    # 3.2 persistencesettingsdbsource.bite — staged; ConnectionString WFAES-encrypted
    # ONLY when -EncryptResources (else staged as-is / assumed already encrypted).
    $dbDest = Join-Path $settingsDir $PersistenceDbSourceName
    Write-Step "Staging '$PersistenceDbSourceName' -> '$dbDest'"
    Copy-Item -LiteralPath $PersistenceDbSourcePath -Destination $dbDest -Force
    if (-not $doEncryptResources) {
        Write-Note 'DbSource staged AS-IS (source encryption disabled; assumed already encrypted).'
    } elseif (-not $keyReachable) {
        Write-Note 'DbSource staged UNENCRYPTED — Key Vault key not reachable; encryption deferred to a real run.'
    } else {
        Invoke-EncryptAndVerify -TargetPath $dbDest -Label 'persistence DbSource'
        Write-Ok 'DbSource ConnectionString encrypted (WFAES via Key Vault).'
    }

    # 3.3 Apply environment variables (App Settings).
    Write-Step 'Applying environment variables (App Settings)'
    $settingsArgs = @($appSettings.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" })
    Invoke-Az (@('functionapp', 'config', 'appsettings', 'set', '--name', $AppName, '--resource-group', $ResourceGroup, '--settings') + $settingsArgs + @('-o', 'none')) -Mutating | Out-Null
    Write-Ok "$($appSettings.Count) app setting(s) applied."

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4 — Deploy the package directory to the Function App
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Deploy package to Function App'
    $script:DeployLastPhase = 'Phase 4  Deploy package'

    $useFunc = $false
    switch ($PublishMethod) {
        'Func' {
            if (-not (Test-CommandExists 'func')) { throw "PublishMethod 'Func' requires Azure Functions Core Tools (func). Install: https://aka.ms/azfunc-install" }
            $useFunc = $true
        }
        'Auto' { $useFunc = $false }   # pre-built artifact -> zip-deploy
        'Zip'  { $useFunc = $false }
    }

    if ($useFunc) {
        if ($DryRun) {
            Write-Host "      [DRYRUN] (cd '$StagingDir') func azure functionapp publish $AppName --dotnet-isolated --no-build" -ForegroundColor DarkGray
        } else {
            Push-Location $StagingDir
            try {
                & func azure functionapp publish $AppName --dotnet-isolated --no-build
                if ($LASTEXITCODE -ne 0) { throw "func publish failed ($LASTEXITCODE)." }
            } finally { Pop-Location }
            Write-Ok 'Deployed via func.'
        }
    } else {
        $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) "wwjobprocessor-$AppName-$runStamp.zip"
        if ($DryRun) {
            Write-Host "      [DRYRUN] Compress-Archive '$StagingDir\*' -> '$zipPath'" -ForegroundColor DarkGray
            Write-Host "      [DRYRUN] az functionapp deployment source config-zip --name $AppName --resource-group $ResourceGroup --src '$zipPath'" -ForegroundColor DarkGray
        } else {
            if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
            Write-Step 'Creating deployment package'
            Compress-Archive -Path (Join-Path $StagingDir '*') -DestinationPath $zipPath -Force
            try {
                Invoke-Az @('functionapp', 'deployment', 'source', 'config-zip', '--name', $AppName, '--resource-group', $ResourceGroup, '--src', $zipPath) -Mutating | Out-Null
                Write-Ok 'Deployed via az zip-deploy.'
            } finally { if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force } }
        }
    }

    # Real run: the staging copy has been uploaded — remove it (it lives under the OS
    # temp dir, not the publish tree). The dry-run preview is intentionally KEPT.
    if (-not $DryRun -and (Test-Path -LiteralPath $StagingDir)) {
        Remove-Item -LiteralPath $StagingDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Ok "Staging dir removed (publish output was never modified)."
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 5 — Verify
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 5  Verify'
    $script:DeployLastPhase = 'Phase 5  Verify'

    Write-Ok "Function App: $baseUrl (timer-triggered — no public HTTP route to probe)"
    Write-Note 'The JobPoll + JobReaper timers begin on the next schedule tick after a warm start.'
    Write-Note "Confirm functions registered: az functionapp function list --name $AppName --resource-group $ResourceGroup -o table"

    Save-DeploySummary -Status 'completed'
    Write-Ok "Summary written to $summaryPath"

    Write-Phase ($DryRun ? 'Dry-run complete (no cloud changes made)' : 'Deployment complete')
    Write-Host "  App      : $AppName" -ForegroundColor White
    Write-Host "  RG       : $ResourceGroup" -ForegroundColor White
    Write-Host "  Endpoint : $baseUrl" -ForegroundColor White
    Write-Host "  Artifact : $($DryRun ? "$StagingDir  (dry-run preview)" : 'staged under OS temp, uploaded, then removed (publish output untouched)')" -ForegroundColor White
    Write-Host "  Summary  : $summaryPath" -ForegroundColor White
    Write-Host ''
    Write-Note 'NEXT: register this app''s managed identity for the engine role Warewolf_JobProcessor'
    Write-Note '(see docs/Deploy-EndToEnd-Runbook.md — JobProcessor section) before jobs can dispatch.'
    Write-Host ''
}
catch {
    $script:DeployStatus = 'failed'
    $errMsg = $_.Exception.Message
    Write-Note "Deployment FAILED in '$script:DeployLastPhase': $errMsg"
    try {
        Save-DeploySummary -Status 'failed' -ErrorMessage $errMsg
        Write-Note "Failure summary written to $summaryPath."
    } catch {
        Write-Note "Could not write failure summary: $($_.Exception.Message)"
    }
    throw
}
finally {
    if ($transcriptOn) { try { Stop-Transcript | Out-Null } catch { } }
}
