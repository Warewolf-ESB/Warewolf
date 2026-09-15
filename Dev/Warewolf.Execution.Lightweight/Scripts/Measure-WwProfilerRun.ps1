#Requires -Version 7.0
<#
.SYNOPSIS
    Performance and reliability report for a run produced by Run-WwProfilerExactLoad.ps1.

.DESCRIPTION
    Reads run-summary.json from the run directory, so nothing has to be typed by hand. The watermark
    recorded there is load-bearing: dbo.jobs1 is truncated between runs in this environment, so
    "rows belonging to this run" is only answerable against the high-water mark captured BEFORE
    publishing, never from a row count.

    FOUR CLOCKS, NOT ONE. A single wall-clock figure cannot answer "how long did it take", so each
    phase is reported separately and then together:
      - pre-warm        loading the engine's assemblies before any real traffic
      - publish         getting N messages onto the broker
      - processing      first completion to last completion, which is the throughput number
      - end to end      the whole exercise, pre-warm included

    LATENCY IS MEASURED FROM THE FIRST PUBLISH, because for a burst that is the only meaningful
    origin: message 15000 sits in the queue while message 1 executes, and its completion time is
    dominated by queue wait, not by work. So publish-to-completion is reported ALONGSIDE the
    workflow's own DurationMs, and the gap between them IS the queueing cost.

    PERFORMANCE IS MEASURED FROM TWO INDEPENDENT SOURCES because disagreement is itself a finding:
      - dbo.jobs1 DurationMs   - what the WORKFLOW believes it spent.
      - engine trace durations - the whole Function invocation, including the request pipeline,
                                 source loading and any cold start. This is what a caller waits
                                 for, and it was ~10 000 ms against a workflow DurationMs of
                                 ~150 ms on a cold instance.

    RELIABILITY IS NOT AN HTTP CODE. The engine returns 200 for a workflow that errored internally:
    on 2026-09-11 a message returned 200, was acked, was NOT dead-lettered, and left no database row
    - silent loss. So every published message is classified from the MANIFEST outwards, and the
    categories are deliberately exhaustive.

    THE DEAD-LETTER QUEUE IS MEASURED, NEVER READ. A BasicGet/close read of this broker's DLQ - the
    pattern Invoke-WwQueueLoadTest.ps1 uses, which AMQP says requeues unacked messages on channel
    close - was followed by that queue going from 10 messages to 0 with no consumer attached. Until
    that is understood, only the depth delta is used. -PeekDeadLetters opts into the risk.

    KQL STRING LITERALS USE SINGLE QUOTES ON PURPOSE: az on Windows strips inner DOUBLE quotes from
    an argument, so a query containing "Insufficient memory" arrives unquoted and fails to parse.

.EXAMPLE
    $env:WWLOADTEST_SQLCONNECTION = '<connection string>'
    .\Measure-WwProfilerRun.ps1 -RunDir G:\UAT2\logs\exactload-20260911-131225
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $RunDir,
    [string] $SqlConnectionString,
    [string] $EngineAppInsightsName = 'wwuat2-ai',
    [string] $ResourceGroup         = 'DEV2',
    [string] $AppInsightsAppId,
    [int]    $WindowPadMinutes      = 2,
    [int]    $CurveRows             = 30,
    [switch] $PeekDeadLetters,
    [string] $ExportCsv
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

function Head($t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Kv($k, $v) { Write-Host ("   {0,-32} {1}" -f $k, $v) }
function Hms([double]$sec) {
    if ($null -eq $sec) { return 'n/a' }
    $ts = [timespan]::FromSeconds($sec)
    '{0:00}:{1:00}:{2:00}  ({3}s)' -f [int]$ts.TotalHours, $ts.Minutes, $ts.Seconds, [math]::Round($sec, 1)
}

$summaryPath = Join-Path $RunDir 'run-summary.json'
if (-not (Test-Path -LiteralPath $summaryPath)) {
    throw "No run-summary.json in '$RunDir'. Was the run produced by Run-WwProfilerExactLoad.ps1?"
}
$run = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json

# Dot-sourcing binds the other script's parameters over ours (it declares JobTable, QueueName,
# ResourceGroup and more), so snapshot and restore. See the same note in Run-WwProfilerExactLoad.ps1.
$myParams = @{}
foreach ($name in $MyInvocation.MyCommand.Parameters.Keys) {
    $v = Get-Variable -Name $name -Scope 0 -ErrorAction SilentlyContinue
    if ($v) { $myParams[$name] = $v.Value }
}
. (Join-Path $ScriptDir 'Invoke-WwQueueLoadTest.ps1') -LoadFunctionsOnly
foreach ($name in $myParams.Keys) { Set-Variable -Name $name -Value $myParams[$name] -Scope 0 }

if (-not $SqlConnectionString) { $SqlConnectionString = $env:WWLOADTEST_SQLCONNECTION }
if (-not $SqlConnectionString) { throw 'Set $env:WWLOADTEST_SQLCONNECTION (or pass -SqlConnectionString).' }

# Invoke-WwSqlQuery returns a NESTED array; @() does not unroll it, which silently reports "no rows".
function Rows {
    # Same transient-failure reasoning as Run-WwProfilerExactLoad.ps1: a pre-login handshake timeout
    # killed a run after 85 minutes. The analysis reads large result sets, so it retries too.
    param([Parameter(Mandatory)][string] $Query, [int] $Retries = 4)
    for ($attempt = 1; $attempt -le $Retries; $attempt++) {
        try {
            return Invoke-WwSqlQuery -ConnectionString $SqlConnectionString -Query $Query -TimeoutSec 300 |
                ForEach-Object { $_ }
        }
        catch {
            if ($attempt -eq $Retries) { throw }
            Write-Host "   (sql attempt $attempt/$Retries failed, retrying)" -ForegroundColor DarkYellow
            Start-Sleep -Seconds (2 * $attempt)
        }
    }
}

$wm    = [int]$run.watermark
$table = $run.jobTable
$where = "JobLogId > $wm AND ISJSON(MessageContent) = 1"
# SQL needs an unambiguous literal; 126 is the ISO 8601 style that CONVERT(datetime2, ...) accepts
# regardless of server language settings.
$pub   = ([datetime]$run.publishStartUtc).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fff')

Head 'Run'
Kv 'label'                  $run.label
Kv 'queue'                  $run.queue
Kv 'requested / published'  "$($run.requested) / $($run.published)"
Kv 'watermark (JobLogId >)' $wm
Kv 'pre-warmed'             "$($run.preWarmed)$(if ($run.preWarmFailed) { '  (PRE-WARM FAILED - read latency accordingly)' })"

# ── 4. End-to-end and per-phase time ────────────────────────────────────────
Head 'Timing - phases'
Kv 'pre-warm'    (Hms $run.preWarmSec)
Kv 'publish'     "$(Hms $run.publishSec)   -> $(if ([double]$run.publishSec -gt 0) { [math]::Round($run.published / [double]$run.publishSec, 1) } else { 'n/a' }) msg/s onto the broker"
Kv 'drain (observed)' (Hms $run.drainSec)
Kv 'END TO END (all phases)' (Hms $run.endToEndSec)
Kv 'first publish (UTC)' $run.publishStartUtc

# The DATABASE's own view of the processing window. This is narrower than the drain phase, which
# also contains the poll interval and the quiet period the runner waits out before declaring the
# run finished - counting those as processing time would understate throughput.
$span = Rows @"
SELECT COUNT(*) AS n,
       CONVERT(varchar(23), MIN(TRY_CONVERT(datetime2, StartedAtUtc)),  126) AS FirstStart,
       CONVERT(varchar(23), MAX(TRY_CONVERT(datetime2, FinishedAtUtc)), 126) AS LastFinish,
       DATEDIFF(millisecond, MIN(TRY_CONVERT(datetime2, StartedAtUtc)), MAX(TRY_CONVERT(datetime2, FinishedAtUtc))) AS SpanMs,
       DATEDIFF(millisecond, CONVERT(datetime2, '$pub'), MIN(TRY_CONVERT(datetime2, StartedAtUtc))) AS FirstPickupMs
FROM $table WHERE $where
"@
if ([int]$span.n -gt 0) {
    $procSec = [double]$span.SpanMs / 1000.0
    Head 'Timing - processing window (from the database)'
    Kv 'first start (UTC)'   $span.FirstStart
    Kv 'last finish (UTC)'   $span.LastFinish
    Kv 'time to first pickup' (Hms ([double]$span.FirstPickupMs / 1000.0))
    Kv 'processing span'     (Hms $procSec)
    Kv 'throughput (span)'   $(if ($procSec -gt 0) { "$([math]::Round([int]$span.n / $procSec, 2)) msg/s" } else { 'sub-second' })
    Kv 'throughput (end to end)' $(if ([double]$run.endToEndSec -gt 0) { "$([math]::Round([int]$span.n / [double]$run.endToEndSec, 2)) msg/s" } else { 'n/a' })
}

# ── 1. Publish-to-completion, and the workflow's own time ───────────────────
Head 'Performance - publish to completion (per message)'
$e2e = Rows @"
WITH t AS (
    SELECT DATEDIFF(millisecond, CONVERT(datetime2, '$pub'), TRY_CONVERT(datetime2, FinishedAtUtc)) AS ms
    FROM $table
    WHERE $where AND TRY_CONVERT(datetime2, FinishedAtUtc) IS NOT NULL
)
SELECT COUNT(*) AS n, MIN(ms) AS MinMs, MAX(ms) AS MaxMs, CAST(AVG(ms*1.0) AS decimal(14,1)) AS AvgMs,
       MAX(p50) AS p50, MAX(p90) AS p90, MAX(p95) AS p95, MAX(p99) AS p99
FROM (
    SELECT ms,
           PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY ms) OVER () AS p50,
           PERCENTILE_CONT(0.90) WITHIN GROUP (ORDER BY ms) OVER () AS p90,
           PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY ms) OVER () AS p95,
           PERCENTILE_CONT(0.99) WITHIN GROUP (ORDER BY ms) OVER () AS p99
    FROM t
) x
"@
if ([int]$e2e.n -gt 0) {
    Kv 'measured'               $e2e.n
    Kv 'min / avg / max'        "$([math]::Round($e2e.MinMs/1000,2))s / $([math]::Round($e2e.AvgMs/1000,2))s / $([math]::Round($e2e.MaxMs/1000,2))s"
    Kv 'p50 / p90 / p95 / p99'  "$([math]::Round($e2e.p50/1000,2))s / $([math]::Round($e2e.p90/1000,2))s / $([math]::Round($e2e.p95/1000,2))s / $([math]::Round($e2e.p99/1000,2))s"
    Write-Host '   NOTE: includes queue wait. On a burst the tail is queueing, not slow execution.' -ForegroundColor DarkGray
}

Head 'Performance - workflow DurationMs (execution only)'
$perf = Rows @"
WITH t AS (
    SELECT TRY_CONVERT(int, DurationMs) AS ms
    FROM $table WHERE $where AND TRY_CONVERT(int, DurationMs) IS NOT NULL
)
SELECT COUNT(*) AS n, MIN(ms) AS MinMs, MAX(ms) AS MaxMs, CAST(AVG(ms*1.0) AS decimal(14,1)) AS AvgMs,
       MAX(p50) AS p50, MAX(p90) AS p90, MAX(p95) AS p95, MAX(p99) AS p99
FROM (
    SELECT ms,
           PERCENTILE_CONT(0.50) WITHIN GROUP (ORDER BY ms) OVER () AS p50,
           PERCENTILE_CONT(0.90) WITHIN GROUP (ORDER BY ms) OVER () AS p90,
           PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY ms) OVER () AS p95,
           PERCENTILE_CONT(0.99) WITHIN GROUP (ORDER BY ms) OVER () AS p99
    FROM t
) x
"@
if ([int]$perf.n -gt 0) {
    Kv 'measured'              $perf.n
    Kv 'min / avg / max (ms)'  "$($perf.MinMs) / $($perf.AvgMs) / $($perf.MaxMs)"
    Kv 'p50 / p90 / p95 / p99' "$($perf.p50) / $($perf.p90) / $($perf.p95) / $($perf.p99)"
}

Head 'Performance - completions per second'
$tp = Rows @"
SELECT CONVERT(varchar(19), TRY_CONVERT(datetime2, FinishedAtUtc), 126) AS Sec, COUNT(*) AS n
FROM $table WHERE $where AND TRY_CONVERT(datetime2, FinishedAtUtc) IS NOT NULL
GROUP BY CONVERT(varchar(19), TRY_CONVERT(datetime2, FinishedAtUtc), 126)
ORDER BY Sec
"@
$buckets = @($tp | ForEach-Object { $_ })
if ($buckets.Count) {
    $counts = @($buckets | ForEach-Object { [int]$_.n }) | Sort-Object
    $peak   = $counts[-1]
    $median = $counts[[int][math]::Floor($counts.Count / 2)]
    Kv 'active seconds'   $buckets.Count
    Kv 'peak msg/s'       $peak
    Kv 'median msg/s'     $median
    # A sustained rate that decays part-way through is the signature of replica recycling, so show
    # the shape rather than only the average. Buckets are sampled evenly - head and tail of a long
    # run are exactly where a stall shows up.
    $step = [math]::Max(1, [int][math]::Ceiling($buckets.Count / $CurveRows))
    Write-Host "   curve (every $step" "second bucket):"
    for ($i = 0; $i -lt $buckets.Count; $i += $step) {
        $b = $buckets[$i]
        Write-Host ("     {0}  {1,5}  {2}" -f $b.Sec, $b.n, ('#' * [math]::Min(50, [int]$b.n)))
    }
}

# ── 3. Reliability: classify every published message ────────────────────────
Head 'Reliability - every published message classified'
$man    = @(Get-Content -LiteralPath $run.manifestPath -Raw | ConvertFrom-Json)
$dbRows = Rows "SELECT JSON_VALUE(MessageContent,'`$.MandateId') AS MandateId, Status, HttpStatusCode, TRY_CONVERT(int, AttemptNumber) AS Attempt FROM $table WHERE $where"
$dbList = @($dbRows | ForEach-Object { $_ })

$byId = @{}
foreach ($r in $dbList) {
    $id = [string]$r.MandateId
    if (-not $id) { continue }
    if (-not $byId.ContainsKey($id)) { $byId[$id] = [System.Collections.Generic.List[object]]::new() }
    $byId[$id].Add($r)
}

$ok = 0; $retried = 0; $dupExec = 0; $badStatus = 0; $notProcessed = [System.Collections.Generic.List[string]]::new()
foreach ($m in $man) {
    $id = [string]$m.mandateId
    if (-not $byId.ContainsKey($id)) { $notProcessed.Add($id); continue }
    $rs = $byId[$id]
    if ($rs.Count -gt 1) { $dupExec++; continue }                       # executed more than once
    $r = $rs[0]
    if ($r.Status -ne 'FINISHED') { $badStatus++; continue }             # recorded but not finished
    if ([int]$r.Attempt -gt 1)    { $retried++;   continue }             # redelivered, then succeeded
    $ok++
}
$unexpected = @($byId.Keys | Where-Object { $_ -notin $man.mandateId }).Count

# Re-probe the dead-letter queue LIVE rather than trusting the summary. If the runner hit its drain
# timeout, its dlqEnd was captured while the worker was still failing messages, so a stale value
# would understate dead-lettering - and dead-lettered-versus-silently-lost is the single most
# important distinction in this report. Passive declare only; the queue is never read.
$dlqLive = $null
try {
    Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force
    $stage = Split-Path -Parent $ScriptDir
    $amqpProbe = Resolve-E2EBrokerUri -SourceBitePath (Join-Path $stage 'sources\ProfilerRabbitMQSource.bite')
    Initialize-E2ERabbitClient -PublishPath (Join-Path $stage 'Apps\QProcessor')
    $ps = New-E2EBrokerSession -AmqpUri $amqpProbe -ClientName 'wwqp-measure-probe'
    try {
        $dlqLive  = (Get-E2EQueueDepth -Session $ps -QueueName $run.deadLetterQueue).Messages
        $mainLive = (Get-E2EQueueDepth -Session $ps -QueueName $run.queue)
    } finally { Close-E2EBrokerSession -Session $ps }
}
catch { Write-Host "   (could not probe the broker: $($_.Exception.Message))" -ForegroundColor DarkYellow }

$dlqEndUsed = if ($null -ne $dlqLive) { [int]$dlqLive } else { [int]$run.dlqEnd }
$dlqDelta   = $dlqEndUsed - [int]$run.dlqBaseline

Kv 'PUBLISHED'                       $man.Count
Kv '  processed correctly (1st try)' "$ok  ($([math]::Round(100.0*$ok/[math]::Max(1,$man.Count),2))%)"
Kv '  reprocessed then succeeded'    $retried
Kv '  executed more than once'       $dupExec
Kv '  recorded but not FINISHED'     $badStatus
Kv '  NOT processed (no row)'        $notProcessed.Count
Kv 'unexpected rows (not published)' $unexpected
Kv 'dead-letter delta'               "$dlqDelta  (baseline $($run.dlqBaseline) -> now $dlqEndUsed$(if ($null -ne $dlqLive -and [int]$dlqLive -ne [int]$run.dlqEnd) { "; summary said $($run.dlqEnd)" }))"
if ($mainLive -and [int]$mainLive.Messages -gt 0) {
    Write-Host "   STILL DRAINING: $($mainLive.Messages) message(s) remain on '$($run.queue)' - these numbers are a snapshot, not a final result." -ForegroundColor Yellow
}
$accounted = $ok + $retried + $dupExec + $badStatus + $notProcessed.Count
Kv 'categories sum to published'     "$accounted / $($man.Count)  $(if ($accounted -eq $man.Count) { 'OK' } else { 'MISMATCH' })"
Kv 'statuses seen'                   ((@($dbList | ForEach-Object { $_.Status })         | Select-Object -Unique) -join ',')
Kv 'http codes seen'                 ((@($dbList | ForEach-Object { $_.HttpStatusCode }) | Select-Object -Unique) -join ',')

if ($notProcessed.Count) {
    $nf = Join-Path $RunDir 'not-processed-mandateids.txt'
    $notProcessed | Set-Content -LiteralPath $nf -Encoding utf8
    Write-Host ''
    if ($dlqDelta -ge $notProcessed.Count) {
        Write-Host "   $($notProcessed.Count) not processed, and the DLQ grew by $dlqDelta - consistent with dead-lettering (bodies preserved, safe to replay)." -ForegroundColor Yellow
    }
    elseif ($dlqDelta -gt 0) {
        Write-Host "   $($notProcessed.Count) not processed but the DLQ only grew by $dlqDelta - so $($notProcessed.Count - $dlqDelta) were acked with NO row and NO dead-letter: SILENT LOSS." -ForegroundColor Red
    }
    else {
        Write-Host "   $($notProcessed.Count) not processed and the DLQ did not grow at all: SILENT LOSS - acked, no row, not replayable." -ForegroundColor Red
    }
    Write-Host "   ids: $nf" -ForegroundColor Yellow
}

if ($ExportCsv) {
    $dbList | Select-Object MandateId, Status, HttpStatusCode, Attempt |
        Export-Csv -LiteralPath $ExportCsv -NoTypeInformation -Encoding utf8
    Kv 'rows exported' $ExportCsv
}

# ── 2. The log side of the measurement ──────────────────────────────────────
Head 'Engine telemetry (Application Insights)'
if (-not $AppInsightsAppId) {
    $AppInsightsAppId = az monitor app-insights component show -g $ResourceGroup -a $EngineAppInsightsName --query appId -o tsv 2>$null
}
if (-not $AppInsightsAppId) { Kv 'skipped' 'could not resolve the App Insights appId' }
else {
    # The window must end at the LAST COMPLETION, not when the runner stopped watching. A drain
    # timeout ends the script while the worker keeps processing, so trusting finishedUtc would cut
    # the telemetry off mid-run and undercount exceptions and OOMs on exactly the runs where they
    # matter most. Take whichever is later.
    $endCandidates = @([datetime]$run.finishedUtc)
    if ($span -and $span.LastFinish) { $endCandidates += [datetime]$span.LastFinish }
    $t0  = ([datetime]$run.publishStartUtc).ToUniversalTime().AddMinutes(-$WindowPadMinutes).ToString('yyyy-MM-dd HH:mm:ss')
    $t1  = (($endCandidates | Sort-Object | Select-Object -Last 1)).ToUniversalTime().AddMinutes($WindowPadMinutes).ToString('yyyy-MM-dd HH:mm:ss')
    $win = "timestamp between (datetime($t0) .. datetime($t1))"
    Kv 'window (UTC)' "$t0  ->  $t1"

    function Kql($q) {
        $raw = az monitor app-insights query --app $AppInsightsAppId --analytics-query $q -o json 2>&1
        $j = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
        if (-not $j.tables) { Write-Host "   (query failed) $raw" -ForegroundColor DarkYellow; return }
        $t = $j.tables[0]; $names = $t.columns.name
        foreach ($r in $t.rows) {
            $o = [ordered]@{}
            for ($i = 0; $i -lt $names.Count; $i++) { $o[$names[$i]] = $r[$i] }
            [pscustomobject]$o
        }
    }

    $oom = Kql "exceptions | where $win | where outerMessage has 'Insufficient memory' | summarize c=count()"
    Kv 'OOM (Insufficient memory)' $(if ($oom) { $oom.c } else { 'n/a' })
    $exc = Kql "exceptions | where $win | summarize c=count()"
    Kv 'exceptions in window'      $(if ($exc) { $exc.c } else { 'n/a' })
    $okf = Kql "traces | where $win | where message has 'WorkflowExecutor Execute completed' | extend s=extract(@'IsSuccess: (\w+)', 1, message) | summarize c=count() by s"
    if ($okf) { Kv 'WorkflowExecutor IsSuccess' (($okf | ForEach-Object { "$($_.s)=$($_.c)" }) -join '  ') }
    $inv = Kql "traces | where $win | where message has 'Request completed for function' | extend ms=toint(extract(@'in (\d+)ms', 1, message)) | summarize c=count(), p50=percentile(ms,50), p95=percentile(ms,95), p99=percentile(ms,99), mx=max(ms)"
    if ($inv) {
        Kv 'engine invocations'            $inv.c
        Kv 'invocation p50/p95/p99/max ms' "$($inv.p50) / $($inv.p95) / $($inv.p99) / $($inv.mx)"
    }
    $top = Kql "exceptions | where $win | summarize c=count() by problemId | order by c desc | take 8"
    if ($top) {
        Write-Host '   top exception problemIds:'
        foreach ($t in $top) { Write-Host ("     {0,7}  {1}" -f $t.c, $t.problemId) }
    }
}

if ($PeekDeadLetters -and $dlqDelta -gt 0) {
    Head 'Dead-letter queue contents'
    Write-Host '   Reading the DLQ - see the header note; this may not be non-destructive on this broker.' -ForegroundColor Yellow
    Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force
    $amqp = Resolve-E2EBrokerUri -SourceBitePath (Join-Path (Split-Path -Parent $ScriptDir) 'sources\ProfilerRabbitMQSource.bite')
    Initialize-E2ERabbitClient -PublishPath (Join-Path (Split-Path -Parent $ScriptDir) 'Apps\QProcessor')
    $sess = New-E2EBrokerSession -AmqpUri $amqp -ClientName 'wwqp-dlq-measure'
    try {
        $ch = New-E2EChannel -Connection $sess.Connection -CancellationToken $sess.Ct
        $seen = 0
        while ($seen -lt ($dlqDelta + 50)) {
            $got = $ch.BasicGetAsync($run.deadLetterQueue, $false, $sess.Ct).GetAwaiter().GetResult()
            if (-not $got) { break }
            $seen++
            Write-Host "     $(Get-WwDeadLetterTxnId -Properties $got.BasicProperties)"
        }
    } finally { Close-E2EBrokerSession -Session $sess }
}

Head 'Verdict'
$clean = ($notProcessed.Count -eq 0 -and $dupExec -eq 0 -and $badStatus -eq 0 -and $unexpected -eq 0 -and $dlqDelta -eq 0)
Write-Host $(if ($clean) { "RELIABILITY: CLEAN  ($ok/$($man.Count) processed correctly)" }
             else { "RELIABILITY: REVIEW  (correct=$ok retried=$retried dup=$dupExec notFinished=$badStatus missing=$($notProcessed.Count) dlq=$dlqDelta)" }) `
    -ForegroundColor $(if ($clean) { 'Green' } else { 'Yellow' })
