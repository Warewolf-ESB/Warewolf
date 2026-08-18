#Requires -Version 7.0
<#
.SYNOPSIS
    Per-message processing report for Warewolf QueueProcessor Container Apps: which replica
    handled each message, how long it took, and what the outcome was - reconciled against the
    Execution Engine's own logs.

.DESCRIPTION
    Reads a TIME WINDOW out of Log Analytics rather than tailing a live run, so it can re-render
    any past burst as long as the logs are still inside the workspace retention. Nothing is
    deployed, published or changed: this script is read-only.

    WHY THE CONTAINER LOGS AND NOT APP INSIGHTS (worker side)
    The worker's Dev2Logger lines - 'Queue execution starting/succeeded', 'durationMs=',
    'Dead-lettered' - never reach App Insights `traces`; only HttpClient DEPENDENCY telemetry
    does. ContainerAppConsoleLogs_CL is also the only source that spans EVERY replica, which
    `az containerapp logs show` (a single-replica live tail) cannot.

    THE JOIN KEYS
    Every worker log line is prefixed by QueueProcessorCorrelation.GetPrefix():

        [Replica:<r>] [Queue:<q>] [Tag:<deliveryTag>] [Txn:<correlationId>] [ExecutionId:<guid>] <message>

      Txn         - the AMQP BasicProperties.CorrelationId of the message. Stamp a unique value
                    per published message and this identifies the message end to end. Present on
                    EVERY line including dead-letter lines, whose own [ExecutionId:...] bracket is
                    the publisher's constant, not the per-message GUID.
      ExecutionId - the per-message GUID the AuditingConsumerDecorator generates and the forwarder
                    sends on as the 'Warewolf-Execution-Id' header. The engine parses it into
                    WorkflowExecutionRequest.ExecutionId and logs it in the SAME [ExecutionId:...]
                    form, so it is the cross-source key between worker and engine.

    ENGINE-SIDE EVIDENCE REQUIRES EXECUTIONLOGLEVEL=INFO
    AuditExecutionLogger only ever writes ERROR/FATAL, so at the default EXECUTIONLOGLEVEL=ERROR a
    SUCCESSFUL execution logs nothing at all. Raise the Function App setting to INFO before the
    burst or -IncludeEngine will correctly report zero engine lines for healthy messages.

    THE ENGINE'S STATUS CODE DOES NOT NEED -IncludeEngine
    The worker logs the engine's HTTP status and response body itself
    (EngineWorkflowClient.cs:154-159), so the EngStatus/EngBody columns are populated straight
    from the container logs even with no App Insights component, no engine correlation and no
    EXECUTIONLOGLEVEL change. Engine correlation ADDS the engine's own view; it is not the only
    way, nor the first way, to find out why a message was dead-lettered. A 401/403 in EngStatus
    in particular can NEVER be explained engine-side - the request never reached the app.

.PARAMETER LastMinutes
    Window ending now. Ignored when -StartUtc is supplied.

.PARAMETER StartUtc / -EndUtc
    Explicit window. -EndUtc defaults to now. Use these to re-render a historical run.

.PARAMETER ExpectedManifest
    Optional JSON array of { txn, queue, kind } describing what was published, where kind is
    'success' or 'failure'. Enables true reconciliation - missing, duplicated and unexpected
    messages - instead of counting what happens to be in the logs.

.EXAMPLE
    # Last 45 minutes, both workers, with engine correlation
    .\Get-WwQueueRunReport.ps1 -AppNamePrefix wwqp3- -IncludeEngine `
        -EngineAppInsightsName wwengine-e2e-th2teq-ai -LastMinutes 45

.EXAMPLE
    # Re-render a run that finished yesterday
    .\Get-WwQueueRunReport.ps1 -AppName wwqp3-ordersuccessqueue `
        -StartUtc '2026-08-10T18:00:00Z' -EndUtc '2026-08-10T18:40:00Z'
#>
[CmdletBinding()]
param(
    [string]   $ResourceGroup = 'DEV2',
    [string[]] $AppName,
    [string]   $AppNamePrefix,
    [string]   $AcaEnvironment = 'dev2-cae',
    [string]   $WorkspaceCustomerId,

    [datetime] $StartUtc,
    [datetime] $EndUtc,
    [int]      $LastMinutes = 60,

    [switch]   $IncludeEngine,
    [string]   $EngineAppInsightsName,

    [string]   $ExpectedManifest,
    [string]   $OutputDir,
    [string]   $RunLabel,
    [int]      $BodyPreviewChars = 60,
    [switch]   $ShowAllRows
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force

function Write-Head { param([string] $T)
    Write-Host ''
    Write-Host ('=' * 78) -ForegroundColor Cyan
    Write-Host "  $T" -ForegroundColor Cyan
    Write-Host ('=' * 78) -ForegroundColor Cyan
}
function Write-Sub  { param([string] $T) Write-Host ''; Write-Host "-- $T" -ForegroundColor Yellow }
function Write-Ok   { param([string] $T) Write-Host "  [+] $T" -ForegroundColor Green }
function Write-Note { param([string] $T) Write-Host "  [-] $T" -ForegroundColor DarkYellow }
function Write-Bad  { param([string] $T) Write-Host "  [x] $T" -ForegroundColor Red }

# ─────────────────────────────────────────────────────────────────────────────
# Resolve the window
# ─────────────────────────────────────────────────────────────────────────────
if (-not $EndUtc)   { $EndUtc   = (Get-Date).ToUniversalTime() }
if (-not $StartUtc) { $StartUtc = $EndUtc.AddMinutes(-$LastMinutes) }
$StartUtc = $StartUtc.ToUniversalTime()
$EndUtc   = $EndUtc.ToUniversalTime()
$kqlStart = $StartUtc.ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
$kqlEnd   = $EndUtc.ToString('yyyy-MM-ddTHH:mm:ss.fffZ')

if (-not $RunLabel) { $RunLabel = 'qrun-' + $EndUtc.ToString('yyyyMMdd-HHmmss') }
if (-not $OutputDir) {
    # G:\Deployment is the staging layout on the build machine, not a requirement. A reviewer on any
    # other machine gets a temp folder rather than a confusing "cannot find drive G" at the very end
    # of a successful run, after the expensive part is already done.
    $logRoot   = if (Test-Path -LiteralPath 'G:\Deployment\logs') { 'G:\Deployment\logs' }
                 else { Join-Path ([System.IO.Path]::GetTempPath()) 'ww-queue-runs' }
    $OutputDir = Join-Path $logRoot $RunLabel
}
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# A terminating error anywhere below (most commonly an `az` call inside Invoke-E2EAzJson, e.g.
# 'containerapp env show' resolving the workspace at Get-WwQueueRunReport.ps1:146) previously left
# $OutputDir with nothing in it: the real report/CSV are only written once at the very end, so the
# CI artifact for a FAILED run was always empty (an ADO "Processed 0 files" / few-byte upload is
# just the empty folder). Write a small diagnostic file here on any failure so the published
# artifact always carries the reason, then rethrow unchanged - this must not turn a real failure
# into a silent pass.
trap {
    $errPath = Join-Path $OutputDir ("queue-run-error-{0}.json" -f (Get-Date -Format 'yyyyMMdd-HHmmss'))
    try {
        [pscustomobject]@{
            runLabel         = $RunLabel
            generatedUtc     = (Get-Date).ToUniversalTime().ToString('o')
            windowStartUtc   = $StartUtc.ToString('o')
            windowEndUtc     = $EndUtc.ToString('o')
            resourceGroup    = $ResourceGroup
            appNamePrefix    = $AppNamePrefix
            apps             = $AppName
            error            = $_.Exception.Message
            scriptStackTrace = $_.ScriptStackTrace
        } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $errPath -Encoding utf8
        Write-Host "##[error]Get-WwQueueRunReport.ps1 failed: $($_.Exception.Message)"
        Write-Host "Diagnostic written to $errPath"
    } catch {
        Write-Host "##[warning]Failed to write diagnostic error report to '$errPath': $($_.Exception.Message)"
    }
    # No 'continue'/'break': fall through so the original terminating error still propagates and
    # the caller (the pipeline step) still fails exactly as before.
}

Write-Head "Queue run report - $RunLabel"
Write-Host "  Window (UTC)   : $kqlStart  ->  $kqlEnd  ($([int]($EndUtc - $StartUtc).TotalMinutes) min)"
Write-Host "  Resource group : $ResourceGroup"
Write-Host "  Output         : $OutputDir"

# ─────────────────────────────────────────────────────────────────────────────
# Resolve the apps in scope
# ─────────────────────────────────────────────────────────────────────────────
if (-not $AppName -or $AppName.Count -eq 0) {
    if (-not $AppNamePrefix) { throw 'Supply -AppName or -AppNamePrefix.' }
    $all = Invoke-E2EAzJson -AzArgs @('containerapp', 'list', '-g', $ResourceGroup)
    $AppName = @($all | Where-Object { $_.name -like "$AppNamePrefix*" } | ForEach-Object { $_.name })
    if ($AppName.Count -eq 0) { throw "No Container Apps in '$ResourceGroup' match prefix '$AppNamePrefix'." }
}
Write-Host "  Apps           : $($AppName -join ', ')"

# ─────────────────────────────────────────────────────────────────────────────
# Resolve the Log Analytics workspace behind the ACA environment
# ─────────────────────────────────────────────────────────────────────────────
if (-not $WorkspaceCustomerId) {
    $envObj = Invoke-E2EAzJson -AzArgs @('containerapp', 'env', 'show', '-g', $ResourceGroup, '-n', $AcaEnvironment)
    $WorkspaceCustomerId = $envObj.properties.appLogsConfiguration.logAnalyticsConfiguration.customerId
    if (-not $WorkspaceCustomerId) { throw "ACA environment '$AcaEnvironment' has no Log Analytics workspace configured." }
}
Write-Host "  Workspace      : $WorkspaceCustomerId"

# ─────────────────────────────────────────────────────────────────────────────
# Pull the raw worker log lines
#
# Parsing happens in PowerShell rather than in KQL extract() chains: the volume is small
# (hundreds of rows), and one regex per line-shape is far easier to keep in step with the
# C# log strings than a stack of nested extracts.
# ─────────────────────────────────────────────────────────────────────────────
$appList = ($AppName | ForEach-Object { "'$_'" }) -join ','

# THE REPLICA COLUMN IS ContainerGroupName_s, NOT ReplicaName_s. The latter does not exist on
# ContainerAppConsoleLogs_CL; projecting it makes the whole query fail, and because
# Invoke-E2ELogAnalytics passes -AllowFail that surfaces as an EMPTY RESULT rather than an error -
# i.e. a run that silently reports zero messages processed. Confirmed against the live schema.
$q = @"
ContainerAppConsoleLogs_CL
| where TimeGenerated between (datetime($kqlStart) .. datetime($kqlEnd))
| where ContainerAppName_s in ($appList)
| project TimeGenerated, ContainerAppName_s, RevisionName_s, ContainerGroupName_s, Log_s
| order by TimeGenerated asc
"@

Write-Sub 'Querying Log Analytics (worker containers)'
$rows = @(Invoke-E2ELogAnalytics -WorkspaceCustomerId $WorkspaceCustomerId -Query $q)
$rows = @($rows | Where-Object { $null -ne $_ })
Write-Ok "$($rows.Count) console log line(s) in window"

if ($rows.Count -eq 0) {
    Write-Bad 'No worker log lines in this window. Either the window is wrong, the app names are wrong, ACA log ingestion is still lagging (2-5 min is normal), or the KQL itself failed - -AllowFail turns a broken query into an empty result, so never read zero rows as "nothing happened".'
}

# ── Line shapes, mirroring the C# exactly ───────────────────────────────────
#    prefix  : QueueProcessorCorrelation.GetPrefix() + QueueProcessorLogSink.Write()
#    start   : AuditingConsumerDecorator.Consume
#    success : AuditingConsumerDecorator.Consume
#    failed  : AuditingConsumerDecorator.Consume (inner returned Failed)
#    threw   : AuditingConsumerDecorator.Consume (exception escaped)
#    deadltr : RabbitMqDeadLetterPublisher.PublishAsync
#    engine  : EngineWorkflowClient.PostSecureAsync
$rxPrefix  = '\[Replica:(?<replica>[^\]]*)\]\s*\[Queue:(?<queue>[^\]]*)\]\s*\[Tag:(?<tag>\d+)\]\s*\[Txn:(?<txn>[^\]]*)\]'
$rxExec    = '\[ExecutionId:(?<exec>[^\]]*)\]'
$rxStart   = "Queue execution starting\.\s*queue='(?<q>[^']*)'\s*workflow='(?<wf>[^']*)'\s*txn='(?<txn>[^']*)'\s*bytes=(?<bytes>\d+)\s*body=(?<body>.*)$"
$rxSuccess = "Queue execution succeeded\..*?workflow='(?<wf>[^']*)'.*?durationMs=(?<ms>\d+)\s*startedUtc=(?<started>\S+)"
$rxFailed  = "Queue execution failed.*?durationMs=(?<ms>\d+)"
$rxThrew   = "Queue execution threw.*?durationMs=(?<ms>\d+)"
$rxDeadLtr = "Dead-lettered\s+(?<bytes>\d+)\s+byte\(s\) to '(?<dlq>[^']*)'"
$rxDlqNew  = "Dead-letter queue '(?<dlq>[^']*)' does not exist - creating it"
$rxUnacked = "Consumer returned Failed for delivery (?<tag>\d+)"
$rxRefuse  = 'refusing to ack'

# THE line that says WHY a message was dead-lettered - EngineWorkflowClient.cs:154-159. Without
# it this report can only ever say 'DeadLettered(acked)', never the HTTP status the engine
# returned nor the body it returned it with, even though both are sitting in the console logs
# already pulled above. A dead-letter with no status is an unfalsifiable finding: 401 (rejected
# by EasyAuth before the app), 404 (workflow not in the deployed Resources) and 500 (workflow
# error, or a WOLF-8418 authorization denial) are indistinguishable without it.
$rxEngStatus = "Engine returned (?<code>\d{3}) for '(?<url>[^']*)'\s*\(classified (?<class>[A-Z]+)"
$rxEngBody   = 'Body:\s*(?<body>.*)$'

# Transport-side counterparts (EngineWorkflowClient.cs:169-177). These end in a REDELIVERY, not
# a dead-letter, so they explain a 'Failed(redelivered)' row the same way the above explains a
# dead-lettered one.
$rxEngFail   = "Engine call to '(?<url>[^']*)' (?<what>timed out after [^.]*|failed)\."

$messages = [ordered]@{}      # key: "app|txn|tag"  -> record
$lifecycle = [System.Collections.Generic.List[object]]::new()
$dlqCreated = [System.Collections.Generic.List[object]]::new()
$anomalies  = [System.Collections.Generic.List[object]]::new()

# Prefixed lines (i.e. lines belonging to a specific delivery) that match NONE of the shapes
# below. Silently dropping these is exactly how the engine-status line stayed invisible; counting
# them means the next diagnostic the C# grows shows up as a number instead of as nothing at all.
$unparsed = [System.Collections.Generic.List[object]]::new()

function Get-Key { param($app, $txn, $tag) "$app|$txn|$tag" }

foreach ($r in $rows) {
    $line = [string]$r.Log_s
    if ([string]::IsNullOrWhiteSpace($line)) { continue }

    $app      = [string]$r.ContainerAppName_s
    $revision = [string]$r.RevisionName_s
    $replicaC = [string]$r.ContainerGroupName_s   # the LA replica column - cross-check for [Replica:]
    $ts       = [datetime]$r.TimeGenerated

    $mPrefix = [regex]::Match($line, $rxPrefix)
    $mExec   = [regex]::Match($line, $rxExec)
    $execId  = if ($mExec.Success) { $mExec.Groups['exec'].Value } else { '' }

    # Startup / lifecycle lines carry no per-message prefix.
    if (-not $mPrefix.Success) {
        if ($line -match "Consuming queue '|Source catalog cached|Drain of |Cancelled consumer|still in flight|NOT using TLS|PRECONDITION_FAILED") {
            $lifecycle.Add([pscustomobject]@{ TimeUtc = $ts; App = $app; Revision = $revision; Replica = $replicaC; Line = $line })
        }
        if ($line -match $rxDlqNew) {
            $dlqCreated.Add([pscustomobject]@{ TimeUtc = $ts; App = $app; Dlq = [regex]::Match($line, $rxDlqNew).Groups['dlq'].Value })
        }
        continue
    }

    $replica = $mPrefix.Groups['replica'].Value
    $queue   = $mPrefix.Groups['queue'].Value
    $tag     = $mPrefix.Groups['tag'].Value
    $txn     = $mPrefix.Groups['txn'].Value
    $key     = Get-Key $app $txn $tag

    if (-not $messages.Contains($key)) {
        $messages[$key] = [pscustomobject]@{
            App           = $app
            Queue         = $queue
            Txn           = $txn
            DeliveryTag   = [int64]$tag
            ExecutionId   = ''
            Replica       = $replica
            ReplicaColumn = $replicaC
            Revision      = $revision
            Workflow      = ''
            Bytes         = 0
            BodyPreview   = ''
            FirstSeenUtc  = $ts
            StartedUtc    = $null
            EndedUtc      = $null
            DurationMs    = $null
            Outcome       = 'Unknown'
            DeadLettered  = $false
            DlqName       = ''
            StartLines    = 0
            RefusedAck    = $false
            UnackedFailed = $false
            # Worker-OBSERVED engine response. Distinct from EngineLogLines/EngineErrors added
            # further down, which come from the engine's own App Insights and are only ever
            # populated when -IncludeEngine is passed AND the execution ids correlate.
            EngineStatus    = ''
            EngineClass     = ''
            EngineUrl       = ''
            EngineBody      = ''
            EngineTransport = ''
        }
    }
    $m = $messages[$key]
    if ($execId -and $execId -notmatch '^QueueProcessor-') { $m.ExecutionId = $execId }
    if (-not $m.Revision) { $m.Revision = $revision }

    $mm = [regex]::Match($line, $rxStart)
    if ($mm.Success) {
        $m.StartLines++
        $m.Workflow    = $mm.Groups['wf'].Value
        $m.Bytes       = [int]$mm.Groups['bytes'].Value
        $body          = $mm.Groups['body'].Value
        $m.BodyPreview = if ($body.Length -gt $BodyPreviewChars) { $body.Substring(0, $BodyPreviewChars) + '...' } else { $body }
        if (-not $m.StartedUtc) { $m.StartedUtc = $ts }
        continue
    }

    $mm = [regex]::Match($line, $rxSuccess)
    if ($mm.Success) {
        $m.Outcome    = 'Succeeded'
        $m.DurationMs = [int]$mm.Groups['ms'].Value
        $m.EndedUtc   = $ts
        if ($mm.Groups['wf'].Success -and -not $m.Workflow) { $m.Workflow = $mm.Groups['wf'].Value }
        # startedUtc from the log is authoritative; TimeGenerated is ingestion-ordered.
        try { $m.StartedUtc = [datetime]::Parse($mm.Groups['started'].Value, $null, [System.Globalization.DateTimeStyles]::RoundtripKind) } catch { }
        continue
    }

    $mm = [regex]::Match($line, $rxFailed)
    if ($mm.Success) { $m.Outcome = 'Failed(redelivered)'; $m.DurationMs = [int]$mm.Groups['ms'].Value; $m.EndedUtc = $ts; continue }

    $mm = [regex]::Match($line, $rxThrew)
    if ($mm.Success) { $m.Outcome = 'Threw'; $m.DurationMs = [int]$mm.Groups['ms'].Value; $m.EndedUtc = $ts; continue }

    $mm = [regex]::Match($line, $rxDeadLtr)
    if ($mm.Success) {
        $m.DeadLettered = $true
        $m.DlqName      = $mm.Groups['dlq'].Value
        if ($m.Outcome -eq 'Unknown') { $m.Outcome = 'DeadLettered' }
        continue
    }

    # Deliberately BEFORE the anomaly fall-through: this is the diagnostic line, not an anomaly.
    $mm = [regex]::Match($line, $rxEngStatus)
    if ($mm.Success) {
        $m.EngineStatus = $mm.Groups['code'].Value
        $m.EngineClass  = $mm.Groups['class'].Value
        $m.EngineUrl    = $mm.Groups['url'].Value
        # The body is matched separately rather than as one expression: the C# puts a long
        # explanatory note between the URL and 'Body:', and that note is free to change without
        # the status capture - the part that matters - going dark.
        $mb = [regex]::Match($line, $rxEngBody)
        if ($mb.Success) { $m.EngineBody = $mb.Groups['body'].Value.Trim() }
        continue
    }

    $mm = [regex]::Match($line, $rxEngFail)
    if ($mm.Success) {
        $m.EngineTransport = $mm.Groups['what'].Value
        $m.EngineUrl       = $mm.Groups['url'].Value
        continue
    }

    if ($line -match $rxRefuse)  { $m.RefusedAck = $true;    $anomalies.Add([pscustomobject]@{ TimeUtc = $ts; App = $app; Txn = $txn; Line = $line }); continue }
    if ($line -match $rxUnacked) { $m.UnackedFailed = $true; $anomalies.Add([pscustomobject]@{ TimeUtc = $ts; App = $app; Txn = $txn; Line = $line }); continue }
    if ($line -match $rxDlqNew)  { $dlqCreated.Add([pscustomobject]@{ TimeUtc = $ts; App = $app; Dlq = [regex]::Match($line, $rxDlqNew).Groups['dlq'].Value }); continue }

    $unparsed.Add([pscustomobject]@{ TimeUtc = $ts; App = $app; Txn = $txn; Line = $line })
}

# A message that succeeded AND was dead-lettered is the business-failure path: the engine
# returned non-2xx, the forwarder dead-lettered the mapped body and STILL returned Success so
# the original is acked. Label it for what it is rather than losing it under 'Succeeded'.
foreach ($m in $messages.Values) {
    if ($m.DeadLettered -and $m.Outcome -eq 'Succeeded') { $m.Outcome = 'DeadLettered(acked)' }
}

$msgs = @($messages.Values | Sort-Object App, StartedUtc, DeliveryTag)
Write-Ok "$($msgs.Count) distinct message delivery(ies) reconstructed"

# ─────────────────────────────────────────────────────────────────────────────
# Engine-side correlation
# ─────────────────────────────────────────────────────────────────────────────
$engineByExec = @{}
$engineRequests = @()
$engineQueried = $false

if ($IncludeEngine) {
    if (-not $EngineAppInsightsName) {
        Write-Note '-IncludeEngine needs -EngineAppInsightsName; skipping engine correlation.'
    }
    else {
        Write-Sub "Querying Application Insights (engine: $EngineAppInsightsName)"
        $aiStart = $StartUtc.ToString('yyyy-MM-ddTHH:mm:ss.fffZ')
        $aiEnd   = $EndUtc.ToString('yyyy-MM-ddTHH:mm:ss.fffZ')

        $qt = "traces | where timestamp between (datetime($aiStart) .. datetime($aiEnd)) | extend exec = extract('\\[ExecutionId:([0-9a-fA-F-]{36})\\]', 1, message) | where isnotempty(exec) | summarize lines=count(), firstUtc=min(timestamp), lastUtc=max(timestamp), errors=countif(severityLevel >= 3) by exec"
        $qr = "requests | where timestamp between (datetime($aiStart) .. datetime($aiEnd)) | project timestamp, name, resultCode, duration, success, operation_Id | order by timestamp asc"

        try {
            $t = Invoke-E2EAzJson -AzArgs @('monitor', 'app-insights', 'query', '--app', $EngineAppInsightsName,
                                            '-g', $ResourceGroup, '--analytics-query', $qt) -AllowFail
            foreach ($tr in @($t.tables)) {
                $cols = @($tr.columns | ForEach-Object { $_.name })
                foreach ($rw in @($tr.rows)) {
                    $o = @{}
                    for ($i = 0; $i -lt $cols.Count; $i++) { $o[$cols[$i]] = $rw[$i] }
                    if ($o['exec']) { $engineByExec[[string]$o['exec']] = $o }
                }
            }
            $engineQueried = $true
            Write-Ok "$($engineByExec.Count) engine execution id(s) found in traces"
            if ($engineByExec.Count -eq 0) {
                Write-Note 'Zero engine trace rows. Most likely EXECUTIONLOGLEVEL is still ERROR on the Function App - AuditExecutionLogger writes only ERROR/FATAL, so successful executions log nothing.'
            }
        }
        catch { Write-Note "Engine traces query failed: $($_.Exception.Message.Split([Environment]::NewLine)[0])" }

        try {
            $rq = Invoke-E2EAzJson -AzArgs @('monitor', 'app-insights', 'query', '--app', $EngineAppInsightsName,
                                             '-g', $ResourceGroup, '--analytics-query', $qr) -AllowFail
            foreach ($tr in @($rq.tables)) {
                $cols = @($tr.columns | ForEach-Object { $_.name })
                foreach ($rw in @($tr.rows)) {
                    $o = [ordered]@{}
                    for ($i = 0; $i -lt $cols.Count; $i++) { $o[$cols[$i]] = $rw[$i] }
                    $engineRequests += [pscustomobject]$o
                }
            }
            Write-Ok "$($engineRequests.Count) engine HTTP request(s) in window"
        }
        catch { Write-Note "Engine requests query failed: $($_.Exception.Message.Split([Environment]::NewLine)[0])" }
    }
}

foreach ($m in $msgs) {
    $hit = if ($m.ExecutionId -and $engineByExec.ContainsKey($m.ExecutionId)) { $engineByExec[$m.ExecutionId] } else { $null }
    Add-Member -InputObject $m -NotePropertyName 'EngineLogLines' -NotePropertyValue ($(if ($hit) { [int]$hit['lines'] } else { 0 })) -Force
    Add-Member -InputObject $m -NotePropertyName 'EngineErrors'   -NotePropertyValue ($(if ($hit) { [int]$hit['errors'] } else { 0 })) -Force
    Add-Member -InputObject $m -NotePropertyName 'EngineSeen'     -NotePropertyValue ($null -ne $hit) -Force
}

# ─────────────────────────────────────────────────────────────────────────────
# Reconciliation against what was actually published
# ─────────────────────────────────────────────────────────────────────────────
$expected = @()
if ($ExpectedManifest -and (Test-Path -LiteralPath $ExpectedManifest)) {
    $expected = @(Get-Content -LiteralPath $ExpectedManifest -Raw | ConvertFrom-Json)
    Write-Ok "Expected manifest loaded: $($expected.Count) published message(s)"
}

$byTxn = @{}
foreach ($m in $msgs) {
    if (-not $m.Txn) { continue }
    if (-not $byTxn.ContainsKey($m.Txn)) { $byTxn[$m.Txn] = @() }
    $byTxn[$m.Txn] += $m
}

$missing    = @()
$duplicated = @()
$unexpected = @()
$mismatched = @()

if ($expected.Count -gt 0) {
    $expectedTxns = @{}
    foreach ($e in $expected) { $expectedTxns[[string]$e.txn] = $e }

    foreach ($e in $expected) {
        $t = [string]$e.txn
        $deliveries = @($byTxn[$t])
        $deliveries = @($deliveries | Where-Object { $null -ne $_ })
        if ($deliveries.Count -eq 0) { $missing += $e; continue }
        if ($deliveries.Count -gt 1) { $duplicated += [pscustomobject]@{ Txn = $t; Kind = $e.kind; Deliveries = $deliveries.Count } }

        $last = $deliveries[-1]
        $wantDead = ([string]$e.kind -eq 'failure')
        if ($wantDead -ne [bool]$last.DeadLettered) {
            $mismatched += [pscustomobject]@{
                Txn = $t; Queue = $e.queue; ExpectedKind = $e.kind
                Outcome = $last.Outcome; DeadLettered = $last.DeadLettered
            }
        }
    }
    foreach ($t in $byTxn.Keys) { if (-not $expectedTxns.ContainsKey($t)) { $unexpected += $t } }
}

# ─────────────────────────────────────────────────────────────────────────────
# Console report
# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Per-message processing'

foreach ($app in ($msgs | ForEach-Object { $_.App } | Sort-Object -Unique)) {
    $set = @($msgs | Where-Object { $_.App -eq $app })
    Write-Sub "$app  ($($set.Count) message(s))"
    $view = $set | Select-Object `
        @{n = 'Txn';       e = { if ($_.Txn.Length -gt 20) { $_.Txn.Substring($_.Txn.Length - 20) } else { $_.Txn } } },
        @{n = 'Tag';       e = { $_.DeliveryTag } },
        @{n = 'Replica';   e = { ($_.Replica -split '--')[-1] } },
        @{n = 'StartedUtc'; e = { if ($_.StartedUtc) { $_.StartedUtc.ToString('HH:mm:ss.fff') } else { '' } } },
        @{n = 'ms';        e = { $_.DurationMs } },
        @{n = 'Outcome';   e = { $_.Outcome } },
        @{n = 'DLQ';       e = { if ($_.DeadLettered) { 'yes' } else { '' } } },
        # The engine's own answer, as the WORKER saw it. This is the column that turns
        # 'DeadLettered(acked)' from a symptom into a cause.
        @{n = 'EngStatus'; e = { if ($_.EngineStatus) { $_.EngineStatus } elseif ($_.EngineTransport) { $_.EngineTransport } else { '' } } },
        @{n = 'EngLines';  e = { $_.EngineLogLines } },
        @{n = 'EngBody';   e = { if ($_.EngineBody.Length -gt $BodyPreviewChars) { $_.EngineBody.Substring(0, $BodyPreviewChars) + '...' } else { $_.EngineBody } } },
        @{n = 'Body';      e = { $_.BodyPreview } }
    if ($ShowAllRows -or $set.Count -le 80) { $view | Format-Table -AutoSize | Out-String -Width 240 | Write-Host }
    else {
        Write-Note "$($set.Count) rows - showing first 40 and last 10; pass -ShowAllRows for everything, or read the CSV."
        @($view | Select-Object -First 40) | Format-Table -AutoSize | Out-String -Width 240 | Write-Host
        @($view | Select-Object -Last 10)  | Format-Table -AutoSize | Out-String -Width 240 | Write-Host
    }
}

Write-Head 'Replica distribution and throughput'

$replicaStats = @()
foreach ($grp in ($msgs | Group-Object App, Replica)) {
    $set  = @($grp.Group)
    $durs = @($set | Where-Object { $null -ne $_.DurationMs } | ForEach-Object { $_.DurationMs })
    $starts = @($set | Where-Object { $null -ne $_.StartedUtc } | ForEach-Object { $_.StartedUtc })
    $ends   = @($set | Where-Object { $null -ne $_.EndedUtc }   | ForEach-Object { $_.EndedUtc })
    $span = if ($starts.Count -gt 0 -and $ends.Count -gt 0) {
        [math]::Round((($ends | Measure-Object -Maximum).Maximum - ($starts | Measure-Object -Minimum).Minimum).TotalSeconds, 1)
    } else { 0 }
    $replicaStats += [pscustomobject]@{
        App       = $set[0].App
        Replica   = ($set[0].Replica -split '--')[-1]
        Revision  = $set[0].Revision
        Messages  = $set.Count
        Succeeded = @($set | Where-Object { $_.Outcome -eq 'Succeeded' }).Count
        DeadLtr   = @($set | Where-Object { $_.DeadLettered }).Count
        MinMs     = if ($durs.Count) { ($durs | Measure-Object -Minimum).Minimum } else { $null }
        MedMs     = if ($durs.Count) { ([int](($durs | Sort-Object)[[math]::Floor($durs.Count / 2)])) } else { $null }
        MaxMs     = if ($durs.Count) { ($durs | Measure-Object -Maximum).Maximum } else { $null }
        SpanSec   = $span
    }
}
$replicaStats | Sort-Object App, Replica | Format-Table -AutoSize | Out-String -Width 200 | Write-Host

Write-Head 'Per-queue summary'

$queueStats = @()
foreach ($grp in ($msgs | Group-Object App)) {
    $set  = @($grp.Group)
    $durs = @($set | Where-Object { $null -ne $_.DurationMs } | ForEach-Object { $_.DurationMs }) | Sort-Object
    $starts = @($set | Where-Object { $null -ne $_.StartedUtc } | ForEach-Object { $_.StartedUtc })
    $ends   = @($set | Where-Object { $null -ne $_.EndedUtc }   | ForEach-Object { $_.EndedUtc })
    $wall = if ($starts.Count -gt 0 -and $ends.Count -gt 0) {
        [math]::Round((($ends | Measure-Object -Maximum).Maximum - ($starts | Measure-Object -Minimum).Minimum).TotalSeconds, 1)
    } else { 0 }
    $queueStats += [pscustomobject]@{
        App           = $grp.Name
        Queue         = $set[0].Queue
        Workflow      = @($set | Where-Object { $_.Workflow } | ForEach-Object { $_.Workflow } | Select-Object -Unique) -join ','
        Messages      = $set.Count
        Succeeded     = @($set | Where-Object { $_.Outcome -eq 'Succeeded' }).Count
        DeadLettered  = @($set | Where-Object { $_.DeadLettered }).Count
        Redelivered   = @($set | Where-Object { $_.StartLines -gt 1 }).Count
        UnknownOutcome= @($set | Where-Object { $_.Outcome -eq 'Unknown' }).Count
        Replicas      = @($set | ForEach-Object { $_.Replica } | Select-Object -Unique).Count
        MinMs         = if ($durs.Count) { $durs[0] } else { $null }
        MedMs         = if ($durs.Count) { $durs[[math]::Floor($durs.Count / 2)] } else { $null }
        P95Ms         = if ($durs.Count) { $durs[[math]::Min($durs.Count - 1, [math]::Floor($durs.Count * 0.95))] } else { $null }
        MaxMs         = if ($durs.Count) { $durs[-1] } else { $null }
        WallSec       = $wall
        MsgPerSec     = if ($wall -gt 0) { [math]::Round($set.Count / $wall, 2) } else { $null }
    }
}
$queueStats | Format-Table -AutoSize | Out-String -Width 220 | Write-Host

# ─────────────────────────────────────────────────────────────────────────────
Write-Head 'Reliability'

$total     = $msgs.Count
$succeeded = @($msgs | Where-Object { $_.Outcome -eq 'Succeeded' }).Count
$deadLtr   = @($msgs | Where-Object { $_.DeadLettered }).Count
$unknown   = @($msgs | Where-Object { $_.Outcome -eq 'Unknown' }).Count
$redeliv   = @($msgs | Where-Object { $_.StartLines -gt 1 }).Count
$refused   = @($msgs | Where-Object { $_.RefusedAck }).Count
$distinctTxn = @($msgs | Where-Object { $_.Txn } | ForEach-Object { $_.Txn } | Select-Object -Unique).Count

Write-Host "  Deliveries observed        : $total"
Write-Host "  Distinct transaction ids   : $distinctTxn"
Write-Host "  Succeeded (engine 2xx)     : $succeeded"
Write-Host "  Dead-lettered (engine non-2xx, acked) : $deadLtr"
Write-Host "  Unknown outcome            : $unknown"
Write-Host "  Redelivered (>1 start line): $redeliv"
Write-Host "  'refusing to ack' lines    : $refused"

# Engine responses AS THE WORKER SAW THEM. This block is the whole point of parsing the engine
# status line: a non-2xx count here, broken down by code, says what to go and fix. It needs no
# -IncludeEngine and no App Insights - it comes from the container logs already fetched.
$engStatuses = @($msgs | Where-Object { $_.EngineStatus } | Group-Object EngineStatus | Sort-Object Name)
$engTransport = @($msgs | Where-Object { $_.EngineTransport })

# Built as an ordered dictionary rather than by summing hashtables: ConvertTo-Json renders this
# as { "500": 1 }, which is what the CI step consumes.
$engineStatusCounts = [ordered]@{}
foreach ($g in $engStatuses) { $engineStatusCounts[$g.Name] = $g.Count }
if ($engStatuses.Count -gt 0 -or $engTransport.Count -gt 0) {
    Write-Host ''
    Write-Host '  Engine responses observed by the worker:'
    foreach ($g in $engStatuses) {
        $sample = @($g.Group | Where-Object { $_.EngineBody })[0]
        $hint = if ($sample) { "  e.g. $($sample.EngineBody.Substring(0, [math]::Min(120, $sample.EngineBody.Length)))" } else { '' }
        Write-Host ("    HTTP {0}  x{1}{2}" -f $g.Name, $g.Count, $hint)
    }
    if ($engTransport.Count -gt 0) {
        Write-Host ("    transport failures x{0} (no HTTP status - unreachable, TLS/DNS, token or timeout)" -f $engTransport.Count)
    }
    # 401/403 never reach the workflow, so no amount of engine-side log hunting will explain them.
    $frontDoor = @($msgs | Where-Object { $_.EngineStatus -in '401', '403' })
    if ($frontDoor.Count -gt 0) {
        Write-Note "$($frontDoor.Count) message(s) were rejected at the front door (401/403) - EasyAuth refused the worker's token before the engine app ran, so there is nothing to find in the engine's own logs. Check the Container App's managed identity, the audience/scope, and the engine's authsettingsV2."
    }
    $notFound = @($msgs | Where-Object { $_.EngineStatus -eq '404' })
    if ($notFound.Count -gt 0) {
        Write-Note "$($notFound.Count) message(s) got 404 - the workflow is not present in the DEPLOYED engine's Resources, regardless of whether it exists in the repo. Check the publish output, not secure.config."
    }
    $five = @($msgs | Where-Object { $_.EngineStatus -eq '500' })
    if ($five.Count -gt 0) {
        Write-Note "$($five.Count) message(s) got 500 - overloaded by design: a genuine workflow error, a WOLF-8418 authorization denial, or host exhaustion. Triage by the EngBody column above, not by the status."
    }
}
elseif ($deadLtr -gt 0) {
    Write-Bad "$deadLtr message(s) were dead-lettered but NO engine status line was parsed. The worker only dead-letters on a non-2xx, so that line should exist - either the log window clipped it, or EngineWorkflowClient's message changed and `$rxEngStatus no longer matches it."
}

if ($unparsed.Count -gt 0) {
    Write-Note "$($unparsed.Count) per-delivery log line(s) matched no known shape and were not interpreted (see the JSON artefact's 'unparsed'). If the worker grew a new diagnostic, teach this script about it rather than reading around it."
}

# Guard on $total: with zero deliveries every condition below is vacuously true, and the report
# would announce a clean run precisely when it found nothing at all.
if ($total -eq 0) {
    Write-Bad 'No deliveries were reconstructed - this is NOT a pass. Resolve the empty log query above before reading anything else in this report.'
}
elseif ($total -eq $distinctTxn -and $unknown -eq 0 -and $refused -eq 0) {
    # Scoped deliberately: this says nothing about messages that were published and NEVER delivered,
    # nor about wrong outcomes. Both are reconciliation findings and are reported below - do not read
    # this line as "the run passed".
    Write-Ok 'No duplicate processing: every delivery observed reached a terminal outcome exactly once.'
    Write-Note 'This covers only what was DELIVERED. Check the reconciliation section for messages that were published but never processed.'
} else {
    if ($total -ne $distinctTxn) { Write-Bad "Duplicate processing: $total deliveries for $distinctTxn distinct messages." }
    if ($unknown -gt 0)          { Write-Bad "$unknown message(s) have no terminal outcome line." }
    if ($refused -gt 0)          { Write-Bad "$refused message(s) hit the 'refusing to ack' path - dead-lettering itself failed." }
}

if ($expected.Count -gt 0) {
    Write-Sub 'Reconciliation against the published manifest'
    Write-Host "  Published        : $($expected.Count)"
    Write-Host "  Matched          : $($expected.Count - $missing.Count)"
    Write-Host "  Missing          : $($missing.Count)"
    Write-Host "  Duplicated       : $($duplicated.Count)"
    Write-Host "  Unexpected txns  : $($unexpected.Count)"
    Write-Host "  Outcome mismatch : $($mismatched.Count)"
    # Parenthesise the whole string: `Write-Bad 'x' + (...)` passes only 'x' to the cmdlet and
    # evaluates the concatenation as a separate, discarded statement.
    if ($missing.Count) { Write-Bad ('MISSING (' + $missing.Count + '): ' + ((@($missing | ForEach-Object { $_.txn }) | Select-Object -First 15) -join ', ')) }
    if ($mismatched.Count) { $mismatched | Format-Table -AutoSize | Out-String -Width 200 | Write-Host }
    if ($missing.Count -eq 0 -and $duplicated.Count -eq 0 -and $mismatched.Count -eq 0) {
        Write-Ok 'Every published message was processed exactly once with the expected outcome.'
    }
}

if ($dlqCreated.Count -gt 0) {
    Write-Sub 'Dead-letter queues created on demand'
    $dlqCreated | Sort-Object TimeUtc -Unique | Format-Table -AutoSize | Out-String -Width 160 | Write-Host
}

if ($anomalies.Count -gt 0) {
    Write-Sub 'Anomalies'
    $anomalies | Format-Table -AutoSize | Out-String -Width 220 | Write-Host
}

if ($lifecycle.Count -gt 0) {
    Write-Sub 'Replica lifecycle'
    $lifecycle | Select-Object TimeUtc, App, @{n='Replica';e={($_.Replica -split '--')[-1]}}, Line |
        Format-Table -AutoSize -Wrap | Out-String -Width 220 | Write-Host
}

if ($engineQueried) {
    Write-Sub 'Engine correlation'
    $seen = @($msgs | Where-Object { $_.EngineSeen }).Count
    Write-Host "  Messages with matching engine log lines : $seen / $total"
    if ($engineRequests.Count -gt 0) {
        $byCode = $engineRequests | Group-Object resultCode | ForEach-Object {
            [pscustomobject]@{ ResultCode = $_.Name; Count = $_.Count }
        }
        $byCode | Format-Table -AutoSize | Out-String -Width 120 | Write-Host
        $durs = @($engineRequests | ForEach-Object { [double]$_.duration } | Sort-Object)
        if ($durs.Count) {
            Write-Host ("  Engine request duration ms  min={0} med={1} max={2}" -f `
                [int]$durs[0], [int]$durs[[math]::Floor($durs.Count/2)], [int]$durs[-1])
        }
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Artefacts
# ─────────────────────────────────────────────────────────────────────────────
$stamp   = $EndUtc.ToString('yyyyMMdd-HHmmss')
$csvPath  = Join-Path $OutputDir "queue-run-messages-$stamp.csv"
$jsonPath = Join-Path $OutputDir "queue-run-report-$stamp.json"

$msgs | Select-Object App, Queue, Workflow, Txn, ExecutionId, DeliveryTag, Replica, Revision,
                      StartedUtc, EndedUtc, DurationMs, Outcome, DeadLettered, DlqName,
                      StartLines, Bytes, EngineStatus, EngineClass, EngineUrl, EngineTransport,
                      EngineLogLines, EngineErrors, EngineBody, BodyPreview |
    Export-Csv -LiteralPath $csvPath -NoTypeInformation -Encoding utf8

[pscustomobject]@{
    runLabel      = $RunLabel
    generatedUtc  = (Get-Date).ToUniversalTime().ToString('o')
    windowStartUtc= $StartUtc.ToString('o')
    windowEndUtc  = $EndUtc.ToString('o')
    resourceGroup = $ResourceGroup
    apps          = $AppName
    workspaceId   = $WorkspaceCustomerId
    engineAi      = $EngineAppInsightsName
    totals        = [pscustomobject]@{
        deliveries = $total; distinctTxn = $distinctTxn; succeeded = $succeeded
        deadLettered = $deadLtr; unknown = $unknown; redelivered = $redeliv; refusedAck = $refused
        # Worker-observed engine responses, keyed by status code, e.g. { "500": 1 }. Callers
        # (the CI verify step) read this to say WHY a run failed without re-parsing the CSV.
        engineStatusCounts = $engineStatusCounts
        engineTransportFailures = $engTransport.Count
    }
    perQueue      = $queueStats
    perReplica    = $replicaStats
    messages      = $msgs
    reconciliation= [pscustomobject]@{
        published = $expected.Count; missing = $missing; duplicated = $duplicated
        unexpected = $unexpected; mismatched = $mismatched
    }
    dlqCreated    = $dlqCreated
    anomalies     = $anomalies
    lifecycle     = $lifecycle
    engineRequests= $engineRequests
    unparsed      = $unparsed
} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding utf8

Write-Head 'Artefacts'
Write-Ok "CSV  : $csvPath"
Write-Ok "JSON : $jsonPath"
