#Requires -Version 7.0
<#
.SYNOPSIS
    Polls the RabbitMQ Shovel Bridge's dynamic shovel and reports whether it is
    healthy ("running"), optionally emitting an Application Insights custom event
    so the result surfaces alongside the Lightweight engine's own telemetry (see
    docs/ShovelBridge-Architecture.md — "Shovel health monitoring").

.DESCRIPTION
    Configure-RabbitMqShovel.ps1 verifies the shovel is running ONCE, at
    configuration time. That is not enough on its own: a stalled or
    continuously-erroring shovel (e.g. after a Service Bus SAS key rotation that
    was not propagated to the Shovel, or the RabbitMQ source broker restarting)
    silently stops triggering workflow executions with no further signal. This
    script is meant to be run PERIODICALLY (via the customer/operator's own
    scheduler — Task Scheduler, cron, Azure Automation runbook, etc.) against the
    RabbitMQ Management HTTP API:

        GET /api/shovels/{vhost}

    and checks the named shovel's reported "state":
      - "running"      -> healthy. Exit 0. Optionally sends a heartbeat
                           "ShovelHealthCheck" custom event (healthy=true).
      - anything else,
        or missing      -> unhealthy. Sends a "ShovelHealthCheck" custom event
                           (healthy=false, state=<value or "not-configured">)
                           when Application Insights is configured, then throws
                           (non-zero exit) so the caller's scheduler can alert.

    WHY A STANDALONE SCRIPT, NOT AN AZURE FUNCTION TIMER TRIGGER: RabbitMQ here is
    the customer's/operator's own broker (often on-premises or in a network the
    Lightweight Function App cannot reach) — see "Why not extend QueueWorker.exe?"
    in docs/ShovelBridge-Architecture.md for the equivalent reasoning about the
    trigger side. A script the operator schedules wherever the RabbitMQ
    Management API IS reachable is the only topology-agnostic option.

    Application Insights delivery uses the plain HTTP ingestion ("Track") API
    directly (no SDK dependency) — same "az CLI + Invoke-RestMethod" style as the
    rest of this Scripts/ folder. Alerting itself (an Azure Monitor alert rule on
    `customEvents | where name == "ShovelHealthCheck" and
    customDimensions.healthy == "false"`) is a separate, one-time operator setup
    step — this script only emits the signal.

    Same conventions as the other Scripts/*.ps1 orchestrators: params-first /
    prompt-if-missing, -DryRun (prints what would be sent, mutates nothing),
    transcript + JSON run summary, and a -LoadFunctionsOnly test hook for Pester.

.PARAMETER RabbitMqManagementUri
    REQUIRED. Base URI of the RabbitMQ management API, e.g. http://localhost:15672.
.PARAMETER RabbitMqUsername / RabbitMqPassword
    REQUIRED. Credentials with at least "monitoring" tag (read-only is enough —
    this script never mutates broker state). -RabbitMqPassword is a SecureString;
    prompted securely when omitted.
.PARAMETER VHost
    RabbitMQ virtual host containing the shovel. Default: / (the default vhost).
.PARAMETER ShovelName
    Name of the dynamic shovel parameter to check. Default: wwexecution-shovel
    (matches Configure-RabbitMqShovel.ps1's default).
.PARAMETER AppInsightsConnectionString
    OPTIONAL. Application Insights connection string (the same one the Lightweight
    engine/worker use — see -AppInsightsName in the Deploy-*.ps1 scripts). When
    omitted, the script still checks health and still throws on an unhealthy
    shovel, it just does not emit telemetry.
.PARAMETER SendHeartbeatOnHealthy
    When set, also emits a "ShovelHealthCheck" (healthy=true) event on a healthy
    check — useful for an Azure Monitor "no heartbeat in N minutes" alert in
    addition to (or instead of) an explicit failure event. Default: off (only
    unhealthy checks emit telemetry).
.PARAMETER LogDir / NonInteractive / DryRun / LoadFunctionsOnly
    Logging output dir; unattended mode; preview-without-change; test hook.

.EXAMPLE
    # Scheduled every 5 minutes via Task Scheduler / cron against the same broker
    # Configure-RabbitMqShovel.ps1 was pointed at.
    ./Monitor-RabbitMqShovel.ps1 `
        -RabbitMqManagementUri http://localhost:15672 -RabbitMqUsername monitor -RabbitMqPassword (ConvertTo-SecureString $env:RABBITMQ_MONITOR_PASSWORD -AsPlainText -Force) `
        -AppInsightsConnectionString $env:APPLICATIONINSIGHTS_CONNECTION_STRING `
        -NonInteractive

.NOTES
    Read-only against RabbitMQ (GET only) — never mutates shovel/broker state.
    See docs/ShovelBridge-Architecture.md for the full bridge architecture and the
    "Known risks / open work" section this script closes out.
#>
[CmdletBinding()]
param(
    # RabbitMQ management API
    [string] $RabbitMqManagementUri,
    [string] $RabbitMqUsername,
    [securestring] $RabbitMqPassword,
    [string] $VHost = '/',
    [string] $ShovelName = 'wwexecution-shovel',

    # Telemetry (optional)
    [string] $AppInsightsConnectionString,
    [switch] $SendHeartbeatOnHealthy,

    # Logging / control
    [string] $LogDir,
    [switch] $NonInteractive,
    [switch] $DryRun,
    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers  (same conventions as Configure-RabbitMqShovel.ps1)
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

function Read-Required {
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

function Read-RequiredSecure {
    param([string] $Name, [securestring] $Current, [string] $Hint)
    if ($null -ne $Current -and $Current.Length -gt 0) { return $Current }
    if ($NonInteractive) {
        throw "Required secure value '$Name' was not supplied. Pass -$Name (SecureString) (running with -NonInteractive)."
    }
    $hintText = if ($Hint) { " ($Hint)" } else { '' }
    do {
        $v = Read-Host "  Enter $Name$hintText" -AsSecureString
    } while ($v.Length -eq 0)
    return $v
}

function ConvertFrom-SecureStringPlain {
    param([securestring] $Value)
    if ($null -eq $Value -or $Value.Length -eq 0) { return '' }
    $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try { return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
    finally { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

function Invoke-RabbitMqApi {
    <# Thin, read-only wrapper over Invoke-RestMethod against the RabbitMQ
       management API. This script never mutates broker state, so — unlike
       Configure-RabbitMqShovel.ps1's Invoke-RabbitMqApi — there is no -Mutating
       branch here. #>
    param(
        [Parameter(Mandatory)][string] $Path,
        [switch] $AllowFail
    )
    $uri = "$($RabbitMqManagementUri.TrimEnd('/'))$Path"
    $plainPassword = ConvertFrom-SecureStringPlain $RabbitMqPassword
    $pair = "${RabbitMqUsername}:${plainPassword}"
    $authHeader = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($pair)) }
    try {
        return Invoke-RestMethod -Method Get -Uri $uri -Headers $authHeader -TimeoutSec 30
    } catch {
        if ($AllowFail) { return $null }
        $detail = $_.Exception.Message
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $detail = $_.ErrorDetails.Message }
        throw "RabbitMQ management API call failed (GET $Path): $detail"
    } finally {
        $plainPassword = $null
    }
}

function Get-ShovelState {
    <# Returns the named shovel's reported "state" string, or $null when the
       shovel does not exist (not yet configured, or was removed). #>
    param([string] $VHostName, [string] $Name)
    $vhostForApi = if ($VHostName -eq '/') { '%2f' } else { [Uri]::EscapeDataString($VHostName) }
    $shovels = Invoke-RabbitMqApi -Path "/api/shovels/$vhostForApi" -AllowFail
    $mine = @($shovels) | Where-Object { $_.name -eq $Name }
    if (-not $mine) { return $null }
    return $mine[0].state
}

function ConvertFrom-AppInsightsConnectionString {
    <# Parses the ';'-delimited key=value connection string into its
       InstrumentationKey + IngestionEndpoint (defaulting the endpoint to the
       classic global one when the connection string omits it). #>
    param([Parameter(Mandatory)][string] $ConnectionString)
    $parts = @{}
    foreach ($seg in ($ConnectionString -split ';')) {
        if ([string]::IsNullOrWhiteSpace($seg)) { continue }
        $kv = $seg -split '=', 2
        if ($kv.Count -eq 2) { $parts[$kv[0].Trim()] = $kv[1].Trim() }
    }
    if (-not $parts.ContainsKey('InstrumentationKey') -or [string]::IsNullOrWhiteSpace($parts['InstrumentationKey'])) {
        throw "AppInsightsConnectionString is missing 'InstrumentationKey'."
    }
    $ingestionEndpoint = if ($parts.ContainsKey('IngestionEndpoint') -and -not [string]::IsNullOrWhiteSpace($parts['IngestionEndpoint'])) {
        $parts['IngestionEndpoint'].TrimEnd('/')
    } else {
        'https://dc.services.visualstudio.com'
    }
    return [pscustomobject]@{
        InstrumentationKey = $parts['InstrumentationKey']
        IngestionEndpoint  = $ingestionEndpoint
    }
}

function Send-AppInsightsEvent {
    <# Emits a single custom event via Application Insights' plain HTTP Track
       API (no SDK). A no-op when -AppInsightsConnectionString was not supplied.
       Under -DryRun, prints the request instead of sending it. Failures here
       are logged but never fail the overall health check — telemetry delivery
       is best-effort, not the source of truth (the throw on an unhealthy
       shovel is). #>
    param(
        [Parameter(Mandatory)][string] $EventName,
        [Parameter(Mandatory)][hashtable] $Properties
    )
    if ([string]::IsNullOrWhiteSpace($AppInsightsConnectionString)) {
        Write-Note 'AppInsightsConnectionString not supplied — skipping telemetry emission.'
        return
    }

    $parsed = ConvertFrom-AppInsightsConnectionString $AppInsightsConnectionString
    $ikeyNoDashes = $parsed.InstrumentationKey -replace '-', ''
    $envelope = [ordered]@{
        name = "Microsoft.ApplicationInsights.$ikeyNoDashes.Event"
        time = (Get-Date).ToUniversalTime().ToString('o')
        iKey = $parsed.InstrumentationKey
        tags = @{ 'ai.cloud.role' = 'ShovelHealthMonitor' }
        data = @{
            baseType = 'EventData'
            baseData = @{
                ver        = 2
                name       = $EventName
                properties = $Properties
            }
        }
    }
    $uri = "$($parsed.IngestionEndpoint)/v2/track"
    $bodyJson = $envelope | ConvertTo-Json -Depth 6

    if ($DryRun) {
        Write-Host "      [DRYRUN] POST $uri" -ForegroundColor DarkGray
        Write-Host "      [DRYRUN] event: $bodyJson" -ForegroundColor DarkGray
        return
    }

    try {
        Invoke-RestMethod -Method Post -Uri $uri -ContentType 'application/x-json-stream' -Body $bodyJson -TimeoutSec 15 | Out-Null
        Write-Ok "Emitted '$EventName' telemetry to Application Insights."
    } catch {
        Write-Note "Failed to send Application Insights telemetry (non-fatal): $($_.Exception.Message)"
    }
}

function Save-MonitorSummary {
    param(
        [ValidateSet('healthy', 'unhealthy', 'failed')] [string] $Status,
        [string] $State,
        [string] $ErrorMessage
    )
    if ([string]::IsNullOrWhiteSpace($summaryPath)) { return }
    $summary = [ordered]@{
        timestampUtc          = (Get-Date).ToUniversalTime().ToString('o')
        status                = $Status
        shovelState           = $State
        error                 = $ErrorMessage
        dryRun                = [bool]$DryRun
        rabbitMqManagementUri = $RabbitMqManagementUri
        vhost                 = $VHost
        shovelName            = $ShovelName
        telemetryConfigured   = -not [string]::IsNullOrWhiteSpace($AppInsightsConnectionString)
    }
    $tmp = "$summaryPath.tmp"
    $summary | ConvertTo-Json -Depth 6 | Set-Content -Path $tmp -Encoding UTF8
    Move-Item -LiteralPath $tmp -Destination $summaryPath -Force
}

# Test hook: stop here when only the helper functions are wanted (Pester).
if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight'

$RabbitMqManagementUri = Read-Required -Name 'RabbitMqManagementUri' -Current $RabbitMqManagementUri -Hint 'e.g. http://localhost:15672'
$RabbitMqUsername      = Read-Required -Name 'RabbitMqUsername'      -Current $RabbitMqUsername      -Hint 'monitoring user (read-only is sufficient)'
$RabbitMqPassword      = Read-RequiredSecure -Name 'RabbitMqPassword' -Current $RabbitMqPassword

if ([string]::IsNullOrWhiteSpace($AppInsightsConnectionString)) {
    Write-Note 'No -AppInsightsConnectionString supplied — health will still be checked (and an unhealthy shovel still throws), but no telemetry will be emitted.'
} else {
    Write-Ok 'Application Insights telemetry is configured.'
}

if (-not $LogDir) { $LogDir = Join-Path ([System.IO.Path]::GetTempPath()) 'wwexecution-shovel-logs' }
if (-not (Test-Path -LiteralPath $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$runStamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$logInfix     = if ($DryRun) { 'dryrun.' } else { '' }
$logFile      = Join-Path $LogDir "monitor-rabbitmq-shovel-$runStamp.${logInfix}log"
$summaryPath  = Join-Path $LogDir "monitor-rabbitmq-shovel-$runStamp.${logInfix}summary.json"
$transcriptOn = $false
try { Start-Transcript -Path $logFile -Append | Out-Null; $transcriptOn = $true; Write-Ok "Logging to $logFile" }
catch { Write-Note "Transcript not started (an outer transcript may be active): $($_.Exception.Message)" }

try {
    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Check shovel state
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Check shovel state'

    Write-Step "GET /api/shovels/{vhost=$VHost} (looking for '$ShovelName')"
    $state = Get-ShovelState -VHostName $VHost -Name $ShovelName

    $isHealthy = ($state -eq 'running')
    $stateForReport = if ($null -eq $state) { 'not-configured' } else { $state }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 2 — Report / alert
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Report'

    $eventProperties = @{
        shovelName    = $ShovelName
        vhost         = $VHost
        state         = $stateForReport
        healthy       = $isHealthy.ToString().ToLowerInvariant()
        checkedAtUtc  = (Get-Date).ToUniversalTime().ToString('o')
    }

    if ($isHealthy) {
        Write-Ok "Shovel '$ShovelName' is running."
        if ($SendHeartbeatOnHealthy) {
            Send-AppInsightsEvent -EventName 'ShovelHealthCheck' -Properties $eventProperties
        }
        Save-MonitorSummary -Status 'healthy' -State $stateForReport
        Write-Phase 'Shovel is healthy'
        return
    }

    Write-Note "Shovel '$ShovelName' is UNHEALTHY (state: '$stateForReport', expected 'running')."
    Send-AppInsightsEvent -EventName 'ShovelHealthCheck' -Properties $eventProperties
    Save-MonitorSummary -Status 'unhealthy' -State $stateForReport
    throw "Shovel '$ShovelName' (vhost '$VHost') is not running (state: '$stateForReport'). Check RabbitMQ logs / the management UI's Admin > Shovel Status page, and verify the Service Bus destination credential has not been rotated without updating the shovel (see docs/KeyRotationRunbook.md)."
}
catch {
    $errMsg = $_.Exception.Message
    try { Save-MonitorSummary -Status 'failed' -State $stateForReport -ErrorMessage $errMsg } catch { }
    throw
}
finally {
    if ($transcriptOn) { try { Stop-Transcript | Out-Null } catch { } }
}
