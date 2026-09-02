#Requires -Version 7.0
<#
.SYNOPSIS
    Deploys the Warewolf Execution Service Bus worker — the dedicated Azure Function
    App (WwExecutionServiceBusWorker, published from
    Warewolf.Execution.ServiceBusWorker) that is triggered by
    an Azure Service Bus queue message and calls the Execution Engine's /secure or
    /public route on the message's behalf. Also provisions the Service Bus namespace,
    queue and dead-letter/authorization settings this worker (and the "shovel bridge"
    RabbitMQ producer feeding it) require.

.DESCRIPTION
    A self-contained orchestrator that mirrors Deploy-WwJobProcessor.ps1's conventions
    (Write-Phase helpers, Invoke-Az wrapper, masked summary + transcript, "params first,
    prompt if missing", -DryRun) but is scoped to the Service Bus worker:

      * NO persistence settings / Key Vault / DbSource encryption (the worker holds no
        secrets of its own — it authenticates OUTBOUND to the engine via Managed Identity,
        exactly like the AzureFunction/JobProcessor daemons, and inbound Service Bus
        listen auth is Managed Identity by default too).
      * PROVISIONS the Service Bus namespace + queue (with dead-lettering and a
        configurable max delivery count) if they do not already exist.
      * Grants the worker's system-assigned managed identity the "Azure Service Bus
        Data Receiver" RBAC role on the queue, and wires the identity-based trigger
        connection (ServiceBusConnection__fullyQualifiedNamespace +
        ServiceBusConnection__credential=managedidentity) — no SAS secret needed for
        the Function App itself.
      * OPTIONALLY creates a Send-only SAS authorization rule scoped to the queue
        (default name: shovel-send) and prints (masked) its primary connection string —
        this is the credential a RabbitMQ Shovel plugin uses as its AMQP 1.0 destination
        (see docs/ShovelBridge-Architecture.md). The worker itself never needs this SAS
        rule; it exists purely for the RabbitMQ-side producer.
      * Applies the worker's app settings (WwExecution:* — see WwExecutionOptions.cs)
        so it can call the engine.

    ROLE REGISTRATION is a SEPARATE operator step (mirrors the engine/JobProcessor
    pattern): grant the worker's system-assigned managed identity the engine app role
    Warewolf_ClientApps with
        Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon -DaemonUseManagedIdentity `
            -DaemonFunctionAppName <thisApp> -DaemonFunctionAppResourceGroup <rg> `
            -AppRolesToAssign Warewolf_ClientApps
    and add the matching secure.config Execute row for the workflow(s) the worker calls
    (or rely on the Public group when using the "public" message route). See
    docs/KB-ClientApps-Configuration.md section 2.6.

    There is deliberately NO companion Rollback-WwExecutionServiceBusWorker.ps1 (same as
    the JobProcessor) — the run summary JSON records exactly what this run created for a
    manual/az-CLI teardown; see the "created" block in the summary file.

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
    REQUIRED (no default). Globally-unique Function App name for the worker.
.PARAMETER PublishPath
    REQUIRED. Folder or .zip of the worker's Release publish output
    (dotnet publish WwExecutionServiceBusWorker.csproj -c Release -o <path>).
.PARAMETER PublishMethod
    Auto (zip-deploy, default) | Func | Zip.
.PARAMETER AppInsightsName / EnableAppInsights
    Application Insights (defaults to "<AppName>-ai"; enabled by default).
.PARAMETER ServiceBusNamespace
    REQUIRED (no default). Service Bus namespace name (3-50 chars, globally unique).
.PARAMETER ServiceBusQueueName
    Queue the worker's ServiceBusTrigger listens on. Default: wwexecution-queue.
    Applied as the WAREWOLF_SERVICEBUS_TRIGGER_QUEUE app setting, which
    WorkflowQueueTrigger.cs's [ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%", ...)]
    binds to via the standard Azure Functions %AppSetting% indirection — overriding this
    param now actually retargets the trigger, not just queue provisioning.
.PARAMETER ServiceBusSku
    Service Bus namespace SKU. Default: Standard (required for topics/RBAC parity;
    Basic also works for a single queue but cannot host authorization rules per-queue
    on some tiers — Standard is the safe default).
.PARAMETER ServiceBusMaxDeliveryCount
    Max delivery attempts before the broker dead-letters a message. Default: 10.
.PARAMETER ServiceBusLockDurationSeconds
    Peek-lock duration for the queue. Default: 300 (5 minutes — generous headroom for
    the worker's engine HTTP call + retry).
.PARAMETER ServiceBusTriggerMaxConcurrentCalls
    Max messages the worker processes concurrently. Default: 16 (matches host.json's
    committed extensions.serviceBus.maxConcurrentCalls). Applied as the
    AzureFunctionsJobHost__extensions__serviceBus__maxConcurrentCalls app setting — the
    standard Azure Functions override convention for host.json values, so it takes effect
    without editing/republishing host.json. NOTE: this override path is unconfirmed against
    a live Function App for the serviceBus extension specifically in this codebase (other
    host.json sections have confirmed prior art, see ShovelBridge-Architecture.md); the
    committed host.json value remains the source of truth if it does not take effect.
.PARAMETER ServiceBusTriggerPrefetchCount
    Messages pre-fetched per replica ahead of maxConcurrentCalls. Default: 0 (matches
    host.json). Same override mechanism/caveat as ServiceBusTriggerMaxConcurrentCalls.
.PARAMETER ServiceBusTriggerMaxAutoLockRenewalMinutes
    How long the worker keeps auto-renewing a message's peek-lock while processing it.
    Default: 5 (matches host.json's 00:05:00). Same override mechanism/caveat.
.PARAMETER ServiceBusTriggerAutoCompleteMessages
    Whether the host auto-completes a message on successful return. Default: true
    (matches host.json — WorkflowQueueTrigger relies on this rather than explicit
    message-actions completion). Same override mechanism/caveat.
.PARAMETER CreateShovelSendRule / ShovelSendRuleName
    When set (default: true), creates a Send-only queue-scoped SAS authorization rule
    for the RabbitMQ Shovel plugin's AMQP 1.0 destination credential. The connection
    string is printed masked at the end of the run; retrieve the real value with:
        az servicebus queue authorization-rule keys list --resource-group <rg> `
            --namespace-name <ns> --queue-name <queue> --name <ShovelSendRuleName> `
            --query primaryConnectionString -o tsv
.PARAMETER UseManagedIdentityForServiceBus
    Default: true. When true, the worker listens via its system-assigned managed
    identity (ServiceBusConnection__fullyQualifiedNamespace +
    ServiceBusConnection__credential=managedidentity) — no SAS secret in app settings.
    When false, a Listen-scoped SAS authorization rule is created instead and its
    connection string is applied as the plain ServiceBusConnection app setting
    (development only — prefer Managed Identity in production).
.PARAMETER WwExecutionBaseUrl
    REQUIRED. Execution Engine base URL, e.g. https://WWExecutionEngine.azurewebsites.net.
.PARAMETER WwExecutionTenantId / WwExecutionResourceAppId / WwExecutionScope
    Entra tenant + the engine's app-registration id (drives token audience/scope).
    WwExecutionTenantId defaults to -TenantId when omitted.
.PARAMETER WwExecutionFunctionKey
    Optional x-functions-key for /services/* routes (blank unless the worker calls that route).
.PARAMETER WwExecutionManagedIdentityClientId
    Optional user-assigned MI client id for calling the engine. Leave blank for
    system-assigned (recommended default).
.PARAMETER LogDir / NonInteractive / DryRun / LoadFunctionsOnly
    Logging output dir; unattended mode; preview-without-change; test hook.

.NOTES
    Publish first (this script does NOT build):
      dotnet publish Dev/Warewolf.Execution.ServiceBusWorker/Warewolf.Execution.ServiceBusWorker.csproj -c Release -o D:\SbWorker\Publish
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
    # Service Bus provisioning
    [string] $ServiceBusNamespace,
    [string] $ServiceBusQueueName = 'wwexecution-queue',
    [ValidateSet('Basic','Standard','Premium')] [string] $ServiceBusSku = 'Standard',
    [nullable[int]] $ServiceBusMaxDeliveryCount,
    [nullable[int]] $ServiceBusLockDurationSeconds,
    # Service Bus trigger binding options (host.json extensions.serviceBus overrides)
    [nullable[int]] $ServiceBusTriggerMaxConcurrentCalls,
    [nullable[int]] $ServiceBusTriggerPrefetchCount,
    [nullable[int]] $ServiceBusTriggerMaxAutoLockRenewalMinutes,
    [nullable[bool]] $ServiceBusTriggerAutoCompleteMessages,
    [nullable[bool]] $CreateShovelSendRule,
    [string] $ShovelSendRuleName = 'shovel-send',
    [nullable[bool]] $UseManagedIdentityForServiceBus,
    # WwExecution (engine caller) settings
    [string] $WwExecutionBaseUrl,
    [string] $WwExecutionTenantId,
    [string] $WwExecutionResourceAppId,
    [string] $WwExecutionScope,
    [string] $WwExecutionFunctionKey,
    [string] $WwExecutionManagedIdentityClientId,
    # Logging / feature env vars
    [nullable[bool]] $EnableConsoleLogging,
    # Logging output / control
    [string] $LogDir,
    [switch] $NonInteractive,
    [switch] $DryRun,
    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers  (kept byte-for-byte consistent with Deploy-WwJobProcessor.ps1)
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
    # Merging stderr into the output stream via 2>&1 does NOT stop it being promoted
    # to a terminating NativeCommandError while the script-level
    # $ErrorActionPreference is 'Stop' - Windows PowerShell 5.1 converts merged
    # stderr lines to ErrorRecord objects and still honours 'Stop' for them, so an
    # expected-on-first-run "EntityNotFound" from an -AllowFail probe (e.g. a
    # 'show' command used to check whether a resource already exists) would abort
    # the whole script before $LASTEXITCODE below is ever checked. Scope
    # $ErrorActionPreference to 'Continue' for just this call (function-local,
    # reverts automatically on return) so only the exit code decides success/fail.
    $ErrorActionPreference = 'Continue'
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
        temp -> Move-Item), after every phase and from the outer catch, so a manual
        teardown always has an authoritative record of what THIS run created.
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
    $shovelConnMasked = if ($doCreateShovelRule) {
        if ([string]::IsNullOrEmpty($shovelConnectionString)) { '<pending — created/read on a real run>' } else { Get-MaskedValue $shovelConnectionString }
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
        serviceBusNamespace       = $ServiceBusNamespace
        serviceBusQueueName       = $ServiceBusQueueName
        serviceBusSku             = $ServiceBusSku
        serviceBusMaxDeliveryCount = $maxDeliveryCount
        serviceBusLockDurationSeconds = $lockDurationSeconds
        serviceBusTriggerMaxConcurrentCalls = $triggerMaxConcurrentCalls
        serviceBusTriggerPrefetchCount = $triggerPrefetchCount
        serviceBusTriggerMaxAutoLockRenewalDuration = $triggerMaxAutoLockRenewalIso
        serviceBusTriggerAutoCompleteMessages = $triggerAutoCompleteMessages
        useManagedIdentityForServiceBus = $useMiForServiceBus
        shovelSendRuleCreated  = $doCreateShovelRule
        shovelSendRuleName     = ($doCreateShovelRule ? $ShovelSendRuleName : $null)
        shovelSendConnectionString = $shovelConnMasked
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

$runId        = "wwsb-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$ResourceTags = @("wwx-test-run=$runId", 'wwx-purpose=servicebusworker-shovel-bridge')
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
$enableAppInsights = Resolve-Toggle -Name 'EnableAppInsights'       -Current $EnableAppInsights       -Default $true -Prompt 'Provision + enable Application Insights?'
$enableConsole     = Resolve-Toggle -Name 'EnableConsoleLogging'    -Current $EnableConsoleLogging    -Default $true -Prompt 'Enable console logging?'
$doCreateShovelRule = Resolve-Toggle -Name 'CreateShovelSendRule'   -Current $CreateShovelSendRule    -Default $true -Prompt 'Create a Send-only SAS rule on the queue for the RabbitMQ Shovel plugin?'
$useMiForServiceBus = Resolve-Toggle -Name 'UseManagedIdentityForServiceBus' -Current $UseManagedIdentityForServiceBus -Default $true -Prompt 'Use Managed Identity for the worker''s Service Bus LISTEN connection (recommended)?'

# ── Service Bus provisioning inputs ─────────────────────────────────────────────
$ServiceBusNamespace = Read-Required -Name 'ServiceBusNamespace' -Current $ServiceBusNamespace -Hint 'globally-unique Service Bus namespace name'
$maxDeliveryCount    = if ($null -ne $ServiceBusMaxDeliveryCount)    { [int]$ServiceBusMaxDeliveryCount }    else { 10 }
$lockDurationSeconds = if ($null -ne $ServiceBusLockDurationSeconds) { [int]$ServiceBusLockDurationSeconds } else { 300 }

# ── Service Bus trigger binding options (host.json extensions.serviceBus overrides) ────
# Defaults match host.json's committed values exactly, so a deploy with no overrides
# behaves identically to today.
$triggerMaxConcurrentCalls   = if ($null -ne $ServiceBusTriggerMaxConcurrentCalls)   { [int]$ServiceBusTriggerMaxConcurrentCalls }   else { 16 }
$triggerPrefetchCount        = if ($null -ne $ServiceBusTriggerPrefetchCount)        { [int]$ServiceBusTriggerPrefetchCount }        else { 0 }
$triggerMaxAutoLockRenewalMinutes = if ($null -ne $ServiceBusTriggerMaxAutoLockRenewalMinutes) { [int]$ServiceBusTriggerMaxAutoLockRenewalMinutes } else { 5 }
$triggerMaxAutoLockRenewalIso = [TimeSpan]::FromMinutes($triggerMaxAutoLockRenewalMinutes).ToString('hh\:mm\:ss')
$triggerAutoCompleteMessages  = if ($null -ne $ServiceBusTriggerAutoCompleteMessages) { [bool]$ServiceBusTriggerAutoCompleteMessages } else { $true }

# ── WwExecution (engine caller) settings ────────────────────────────────────────
$WwExecutionBaseUrl      = Read-Required -Name 'WwExecutionBaseUrl'      -Current $WwExecutionBaseUrl      -Hint 'Execution Engine base URL, e.g. https://<engine>.azurewebsites.net'
$WwExecutionTenantId     = if (-not [string]::IsNullOrWhiteSpace($WwExecutionTenantId))     { $WwExecutionTenantId }     else { $TenantId }
$WwExecutionResourceAppId = Read-Required -Name 'WwExecutionResourceAppId' -Current $WwExecutionResourceAppId -Hint "engine's app-registration (client) id"

# ── Build the environment-variable (app settings) map ──────────────────────────
$appSettings = [ordered]@{}
$secretSettingNames = New-Object System.Collections.Generic.HashSet[string]

$appSettings['FUNCTIONS_WORKER_RUNTIME']  = 'dotnet-isolated'
$appSettings['ENABLECONSOLELOGGING']      = ($enableConsole ? 'true' : 'false')
$appSettings['ENABLEAPPLICATIONINSIGHTS'] = ($enableAppInsights ? 'true' : 'false')

# WwExecution:* — bound to WwExecutionOptions (see WwExecutionOptions.cs); the worker
# uses double-underscore config-key nesting exactly like local.settings.json.
$appSettings['WwExecution__BaseUrl']       = $WwExecutionBaseUrl
$appSettings['WwExecution__TenantId']      = $WwExecutionTenantId
$appSettings['WwExecution__ResourceAppId'] = $WwExecutionResourceAppId
if (-not [string]::IsNullOrWhiteSpace($WwExecutionScope)) {
    $appSettings['WwExecution__Scope'] = $WwExecutionScope
}
if (-not [string]::IsNullOrWhiteSpace($WwExecutionFunctionKey)) {
    $appSettings['WwExecution__FunctionKey'] = $WwExecutionFunctionKey
    $secretSettingNames.Add('WwExecution__FunctionKey') | Out-Null
}
if (-not [string]::IsNullOrWhiteSpace($WwExecutionManagedIdentityClientId)) {
    $appSettings['WwExecution__ManagedIdentityClientId'] = $WwExecutionManagedIdentityClientId
}
# UseClientSecretFallback is intentionally NEVER set here — Managed Identity only for
# a deployed worker. Local dev's local.settings.json is where the secret fallback lives.
$appSettings['WwExecution__UseClientSecretFallback'] = 'false'

# WAREWOLF_SERVICEBUS_TRIGGER_QUEUE — the %AppSetting% indirection WorkflowQueueTrigger.cs's
# [ServiceBusTrigger] attribute binds to (mirrors ServiceBusWorkflowTriggerFunction.cs's
# in-engine "Model A" trigger). Overriding -ServiceBusQueueName now actually retargets the
# trigger, not just queue provisioning.
$appSettings['WAREWOLF_SERVICEBUS_TRIGGER_QUEUE'] = $ServiceBusQueueName

# Service Bus trigger binding options — the standard Azure Functions host.json override
# convention (AzureFunctionsJobHost__extensions__<section>__<setting>). Defaults match
# host.json's committed values, so an unoverridden deploy behaves identically to today.
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__maxConcurrentCalls']       = "$triggerMaxConcurrentCalls"
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__prefetchCount']            = "$triggerPrefetchCount"
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__maxAutoLockRenewalDuration'] = $triggerMaxAutoLockRenewalIso
$appSettings['AzureFunctionsJobHost__extensions__serviceBus__autoCompleteMessages']     = ($triggerAutoCompleteMessages ? 'true' : 'false')

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
Write-Host ("    {0,-28}: {1}" -f 'Service Bus namespace', "$ServiceBusNamespace ($ServiceBusSku)")
Write-Host ("    {0,-28}: {1}" -f 'Service Bus queue', "$ServiceBusQueueName (maxDelivery=$maxDeliveryCount, lock=${lockDurationSeconds}s, DLQ on)")
Write-Host ("    {0,-28}: {1}" -f 'Trigger binding', "maxConcurrentCalls=$triggerMaxConcurrentCalls, prefetchCount=$triggerPrefetchCount, maxAutoLockRenewal=$triggerMaxAutoLockRenewalIso, autoComplete=$triggerAutoCompleteMessages")
Write-Host ("    {0,-28}: {1}" -f 'Worker listen auth', ($useMiForServiceBus ? 'Managed Identity (recommended)' : 'SAS connection string (dev only)'))
Write-Host ("    {0,-28}: {1}" -f 'Shovel Send SAS rule', ($doCreateShovelRule ? "$ShovelSendRuleName (for the RabbitMQ Shovel bridge)" : 'not created'))
Write-Host ("    {0,-28}: {1}" -f 'LogDir', $LogDir)
Write-Host ("    {0,-28}: {1}" -f 'Run tag', "wwx-test-run=$runId")
Write-Host ("    {0,-28}: {1}" -f 'DryRun', $DryRun)
Write-Host ''
Write-Host '  ── Environment variables (App Settings) to apply ─────────────────────' -ForegroundColor White
foreach ($k in $appSettings.Keys) {
    $shown = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    Write-Host ("    {0,-34}= {1}" -f $k, $shown)
}
Write-Host ("    {0,-34}= {1}" -f 'ServiceBusConnection*', ($useMiForServiceBus ? '(identity-based — set in Phase 2)' : '(SAS connection string — set in Phase 2)'))
if ($enableAppInsights) {
    Write-Host ("    {0,-34}= {1}" -f 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING', '(auto-read from the App Insights resource)')
}
Write-Host ''
Write-Note 'Role registration is a SEPARATE step: grant this app''s managed identity the engine'
Write-Note 'role Warewolf_ClientApps (Configure-WwExecutionAuth-Clients.ps1 -ClientType Daemon'
Write-Note '-DaemonUseManagedIdentity -AppRolesToAssign Warewolf_ClientApps) + the matching'
Write-Note 'secure.config Execute row for the workflow(s) called. See docs/KB-ClientApps-Configuration.md 2.6.'
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
$logFile     = Join-Path $LogDir "deploy-WwExecutionServiceBusWorker-$runStamp.${logInfix}log"
$summaryPath = Join-Path $LogDir "deploy-WwExecutionServiceBusWorker-$runStamp.${logInfix}summary.json"
try { Start-Transcript -Path $logFile -Append | Out-Null; $transcriptOn = $true; Write-Ok "Logging to $logFile" }
catch { Write-Note "Transcript not started (an outer transcript may be active): $($_.Exception.Message)" }

$baseUrl                = "https://$AppName.azurewebsites.net"
$StagingDir             = $PublishDir
$aiConnectionString     = $null
$shovelConnectionString = $null
$script:DeployLastPhase = 'Phase 0.5  Plan'

Save-DeploySummary -Status 'in-progress'

try {
    Invoke-Az @('account', 'set', '--subscription', $SubscriptionId) -Mutating | Out-Null

    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Infrastructure (resource group, storage, function app, App Insights)
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Infrastructure (resource group, storage, function app, App Insights)'
    $script:DeployLastPhase = 'Phase 1  Infrastructure'

    # 1.1 Resource group
    $rgExists = (Invoke-Az @('group', 'exists', '--name', $ResourceGroup)) -join ''
    if ($rgExists -eq 'true') {
        Write-Ok "Resource group '$ResourceGroup' already exists."
        $created['resourceGroup'] = $false
    } else {
        $created['resourceGroup'] = $true
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
        $created['storageAccount'] = $true
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
        $created['functionApp'] = $true
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
    if ($enableAppInsights) {
        $aiPreExists = [bool](Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail)
        $created['appInsights'] = (-not $aiPreExists)
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
    # Phase 2 — Service Bus namespace, queue, dead-lettering, auth rules
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Service Bus namespace + queue provisioning'
    $script:DeployLastPhase = 'Phase 2  Service Bus provisioning'

    # 2.1 Namespace
    $nsExists = Invoke-Az @('servicebus', 'namespace', 'show', '--name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($nsExists) {
        Write-Ok "Service Bus namespace '$ServiceBusNamespace' already exists."
        $created['serviceBusNamespace'] = $false
    } else {
        $created['serviceBusNamespace'] = $true
        Write-Step "Creating Service Bus namespace '$ServiceBusNamespace' ($ServiceBusSku)"
        Invoke-Az (@(
            'servicebus', 'namespace', 'create',
            '--name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--location', $Location,
            '--sku', $ServiceBusSku, '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Service Bus namespace created.'
    }

    # 2.2 Queue — dead-lettering on max delivery count exhaustion, configurable lock duration.
    $queueExists = Invoke-Az @('servicebus', 'queue', 'show', '--name', $ServiceBusQueueName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    $lockDurationIso = "PT${lockDurationSeconds}S"
    if ($queueExists) {
        Write-Ok "Queue '$ServiceBusQueueName' already exists."
        $created['serviceBusQueue'] = $false
    } else {
        $created['serviceBusQueue'] = $true
        Write-Step "Creating queue '$ServiceBusQueueName' (maxDeliveryCount=$maxDeliveryCount, lockDuration=$lockDurationIso, DLQ-on-exceed-max-delivery)"
        Invoke-Az @(
            'servicebus', 'queue', 'create',
            '--name', $ServiceBusQueueName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup,
            '--max-delivery-count', "$maxDeliveryCount", '--lock-duration', $lockDurationIso,
            '--enable-dead-lettering-on-message-expiration', 'true'
        ) -Mutating | Out-Null
        Write-Ok 'Queue created.'
    }

    # 2.3 Listen auth for the worker — Managed Identity (recommended) or SAS fallback.
    if ($useMiForServiceBus) {
        Write-Note 'Worker listen auth: Managed Identity — RBAC role assignment happens in Phase 3 (after the identity exists).'
    } else {
        $listenRuleName = 'wwexecutionworker-listen'
        $listenRuleExists = [bool](Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'show', '--name', $listenRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '-o', 'json') -AllowFail)
        $created['serviceBusListenRule'] = (-not $listenRuleExists)
        if (-not $listenRuleExists) {
            Write-Step "Creating Listen-only SAS authorization rule '$listenRuleName' on the queue"
            Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'create', '--name', $listenRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--rights', 'Listen') -Mutating | Out-Null
        }
        $script:listenConnectionString = if ($DryRun) { '<dryrun-listen-connection-string>' } else {
            (Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'keys', 'list', '--name', $listenRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--query', 'primaryConnectionString', '-o', 'tsv') -AllowFail) -join ''
        }
        Write-Ok "Listen SAS rule ready (connection string applied to app settings in Phase 3)."
    }

    # 2.4 Send-only SAS rule for the RabbitMQ Shovel plugin's AMQP 1.0 destination.
    # Least-privilege by design: the Shovel only ever needs to PUBLISH to this queue.
    if ($doCreateShovelRule) {
        $shovelRuleExists = [bool](Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'show', '--name', $ShovelSendRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '-o', 'json') -AllowFail)
        $created['shovelSendRule'] = (-not $shovelRuleExists)
        if (-not $shovelRuleExists) {
            Write-Step "Creating Send-only SAS authorization rule '$ShovelSendRuleName' on the queue (for the RabbitMQ Shovel bridge)"
            Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'create', '--name', $ShovelSendRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--rights', 'Send') -Mutating | Out-Null
        } else {
            Write-Ok "Send-only SAS rule '$ShovelSendRuleName' already exists."
        }
        $shovelConnectionString = if ($DryRun) { '<dryrun-shovel-connection-string>' } else {
            (Invoke-Az @('servicebus', 'queue', 'authorization-rule', 'keys', 'list', '--name', $ShovelSendRuleName, '--namespace-name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--queue-name', $ServiceBusQueueName, '--query', 'primaryConnectionString', '-o', 'tsv') -AllowFail) -join ''
        }
        Write-Ok 'Shovel Send SAS rule ready — see the masked summary at the end for retrieval instructions.'
    } else {
        Write-Note 'Shovel Send SAS rule not requested (-CreateShovelSendRule:$false).'
    }

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 3 — Managed identity + Service Bus RBAC
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Managed identity + Service Bus RBAC'
    $script:DeployLastPhase = 'Phase 3  Managed identity + RBAC'

    Write-Step 'Enabling system-assigned managed identity on the Function App'
    $idJson = Invoke-Az @('functionapp', 'identity', 'assign', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -Mutating
    $funcPrincipalId = if ($DryRun) { '<dryrun-principal-id>' } else { ($idJson | ConvertFrom-Json).principalId }

    if ($useMiForServiceBus) {
        $nsId = if ($DryRun) { '<dryrun-namespace-id>' } else { (Invoke-Az @('servicebus', 'namespace', 'show', '--name', $ServiceBusNamespace, '--resource-group', $ResourceGroup, '--query', 'id', '-o', 'tsv')) -join '' }
        Write-Step 'Assigning RBAC: Azure Service Bus Data Receiver -> Function App managed identity (namespace scope)'
        Invoke-Az @('role', 'assignment', 'create', '--role', 'Azure Service Bus Data Receiver', '--assignee', $funcPrincipalId, '--scope', $nsId) -Mutating | Out-Null
        Write-Ok 'RBAC assigned. (Propagation can take a couple of minutes before the trigger connects successfully.)'
    } else {
        Write-Note 'Managed Identity RBAC skipped (worker uses the SAS Listen connection string instead).'
    }

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4 — Apply app settings
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Apply app settings'
    $script:DeployLastPhase = 'Phase 4  App settings'

    if ($useMiForServiceBus) {
        $appSettings['ServiceBusConnection__fullyQualifiedNamespace'] = "$ServiceBusNamespace.servicebus.windows.net"
        $appSettings['ServiceBusConnection__credential']              = 'managedidentity'
        if (-not [string]::IsNullOrWhiteSpace($WwExecutionManagedIdentityClientId)) {
            $appSettings['ServiceBusConnection__clientId'] = $WwExecutionManagedIdentityClientId
        }
    } else {
        $appSettings['ServiceBusConnection'] = $script:listenConnectionString
        $secretSettingNames.Add('ServiceBusConnection') | Out-Null
    }

    Write-Step 'Applying environment variables (App Settings)'
    $settingsArgs = @($appSettings.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" })
    Invoke-Az (@('functionapp', 'config', 'appsettings', 'set', '--name', $AppName, '--resource-group', $ResourceGroup, '--settings') + $settingsArgs + @('-o', 'none')) -Mutating | Out-Null
    Write-Ok "$($appSettings.Count) app setting(s) applied."

    Save-DeploySummary -Status 'in-progress'

    # ════════════════════════════════════════════════════════════════════════
    # Phase 5 — Deploy the package directory to the Function App
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 5  Deploy package to Function App'
    $script:DeployLastPhase = 'Phase 5  Deploy package'

    $useFunc = $false
    switch ($PublishMethod) {
        'Func' {
            if (-not (Test-CommandExists 'func')) { throw "PublishMethod 'Func' requires Azure Functions Core Tools (func). Install: https://aka.ms/azfunc-install" }
            $useFunc = $true
        }
        'Auto' { $useFunc = $false }
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
        $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) "wwexecutionservicebusworker-$AppName-$runStamp.zip"
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

    # ════════════════════════════════════════════════════════════════════════
    # Phase 6 — Verify
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 6  Verify'
    $script:DeployLastPhase = 'Phase 6  Verify'

    Write-Ok "Function App: $baseUrl (Service Bus-triggered — no public HTTP route to probe)"
    Write-Note "Confirm the trigger registered: az functionapp function list --name $AppName --resource-group $ResourceGroup -o table"
    Write-Note "Confirm queue depth: az servicebus queue show --name $ServiceBusQueueName --namespace-name $ServiceBusNamespace --resource-group $ResourceGroup --query countDetails"

    Save-DeploySummary -Status 'completed'
    Write-Ok "Summary written to $summaryPath"

    Write-Phase ($DryRun ? 'Dry-run complete (no cloud changes made)' : 'Deployment complete')
    Write-Host "  App              : $AppName" -ForegroundColor White
    Write-Host "  RG               : $ResourceGroup" -ForegroundColor White
    Write-Host "  Endpoint         : $baseUrl" -ForegroundColor White
    Write-Host "  Service Bus queue: $ServiceBusNamespace / $ServiceBusQueueName" -ForegroundColor White
    Write-Host "  Summary          : $summaryPath" -ForegroundColor White
    Write-Host ''
    if ($doCreateShovelRule) {
        Write-Note "RabbitMQ Shovel destination credential — retrieve with:"
        Write-Note "  az servicebus queue authorization-rule keys list --resource-group $ResourceGroup ``"
        Write-Note "      --namespace-name $ServiceBusNamespace --queue-name $ServiceBusQueueName --name $ShovelSendRuleName ``"
        Write-Note "      --query primaryConnectionString -o tsv"
        Write-Note "Convert to the Shovel's AMQP 1.0 destination URI per docs/ShovelBridge-Architecture.md."
    }
    Write-Note 'NEXT: register this app''s managed identity for the engine role Warewolf_ClientApps'
    Write-Note '(see docs/KB-ClientApps-Configuration.md 2.6) before the worker can call the engine.'
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
