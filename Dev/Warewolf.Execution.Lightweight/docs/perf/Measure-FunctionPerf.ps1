<#
.SYNOPSIS
	Cold-start and scaling performance harness for the Warewolf Lightweight Execution Engine
	Azure Function App. Drives the app via its public Warewolf workflows (discovered from
	/public/apis.json) and measures four phases:

	  1. ColdStart   - stop then start the app; measure first-request latency after warm-up.
	  2. Warm        - steady-state latency once the instance is hot (sequential burst).
	  3. Concurrency - sequential vs parallel throughput and instance scale-out.
	  4. IdleColdStart - wait for scale-to-zero (default 15 min idle) then measure next request.

	Client-side stopwatch timings are the primary signal. Pass -Correlate (or add
	"Correlate" to -Phases) to run the telemetry-correlation step after the load phases:
	it queries Azure Monitor (FunctionExecutionCount/Requests/Http2xx/InstanceCount) and,
	after waiting out App Insights ingestion lag, retries a KQL union over the POPULATED
	tables (traces/dependencies/performanceCounters) to capture instance scale-out
	(dcount(cloud_RoleInstance)) and Warewolf server performance counters. The `requests`
	and `exceptions` App Insights tables are intentionally NOT queried — they stay empty in
	this app (custom WAREWOLF_APPINSIGHTS_CONNECTION_STRING). See azure-functions-telemetry.md.

.NOTES
	Uses only the anonymous /public/ route - no auth token required.
	Requires: Az CLI logged in (for stop/start), PowerShell 7+ (ForEach-Object -Parallel).
#>

[CmdletBinding()]
param(
	[string]   $BaseUrl       = "https://wwenginenewscriptai.azurewebsites.net",
	[string]   $ResourceGroup = "DEV2",
	[string]   $FunctionApp   = "wwenginenewscriptai",
	[string]   $AiName        = "wwenginenewscriptai-ai",
	[string[]] $Phases        = @("ColdStart", "Warm", "Concurrency", "IdleColdStart"),
	[int]      $WarmRequests  = 30,
	[int]      $SeqRequests   = 40,
	[int]      $ParRequests   = 40,
	[int]      $ParThrottle   = 20,
	[int]      $IdleMinutes   = 16,
	# Telemetry correlation: after the load phases, wait for App Insights ingestion then
	# query instance scale-out (dcount(cloud_RoleInstance)) + Azure Monitor execution counts.
	[switch]   $Correlate,
	[int]      $IngestWaitSeconds = 360,   # App Insights raw-table ingestion lag (5-10 min typical)
	[int]      $IngestRetries     = 6,
	[int]      $IngestRetryWait   = 60,
	[string]   $OutDir        = (Join-Path $PSScriptRoot "runs"),

	# ── NEW: small-batch and load-test parameters ──────────────────────────────────────────
	# SmallBatch : number of requests for Seq5 / Par5 phases  (default 5, per requirements)
	# LoadCount  : number of requests for LoadSeq / LoadPar   (default 100, per requirements)
	# LoadThrottle: max parallel threads for LoadPar phase
	[int]      $SmallBatch    = 5,
	[int]      $LoadCount     = 100,
	[int]      $LoadThrottle  = 50
)

$ErrorActionPreference = "Stop"
$runStartUtc = (Get-Date).ToUniversalTime()
$runStamp = $runStartUtc.ToString("yyyyMMdd-HHmmss")
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }
$log = Join-Path $OutDir "perf-$runStamp.log"
$csv = Join-Path $OutDir "perf-$runStamp.csv"
$results = [System.Collections.Generic.List[object]]::new()

function Write-Log {
	param([string]$Message)
	$line = "[{0}] {1}" -f (Get-Date).ToUniversalTime().ToString("HH:mm:ss.fff"), $Message
	Write-Host $line
	Add-Content -Path $log -Value $line
}

# --- Discover executable public workflow targets from /public/apis.json -----------------
function Get-Targets {
	Write-Log "Discovering public APIs from $BaseUrl/public/apis.json"
	$apis = (Invoke-WebRequest -Uri "$BaseUrl/public/apis.json" -UseBasicParsing -TimeoutSec 60).Content | ConvertFrom-Json
	Write-Log "Found $($apis.Apis.Count) public APIs"
	# Prefer known-good simple GET workflows; fall back to the first few baseUrls.
	$preferred = @(
		"$BaseUrl/Public/Hello World.json?Name=Warewolf",
		"$BaseUrl/Public/Examples/Dice Roll Example/Dice Roll.json"
	)
	return $preferred
}

# --- Single timed request ----------------------------------------------------------------
function Invoke-Timed {
	param([string]$Url, [string]$Phase, [int]$Seq = 0)
	$sw = [System.Diagnostics.Stopwatch]::StartNew()
	$status = 0; $ok = $false; $len = 0; $err = ""
	try {
		$r = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 120
		$sw.Stop(); $status = [int]$r.StatusCode; $ok = $true; $len = $r.Content.Length
	} catch {
		$sw.Stop()
		try { $status = [int]$_.Exception.Response.StatusCode.value__ } catch { $status = -1 }
		$err = $_.Exception.Message
	}
	[PSCustomObject]@{
		Phase      = $Phase
		Seq        = $Seq
		TimestampUtc = (Get-Date).ToUniversalTime().ToString("o")
		Ms         = [math]::Round($sw.Elapsed.TotalMilliseconds, 1)
		Status     = $status
		Ok         = $ok
		Bytes      = $len
		Url        = $Url
		Error      = $err
	}
}

function Add-Result { param($r) $results.Add($r) | Out-Null; Write-Log ("  {0,-13} seq={1,-3} {2,6}ms  http={3}" -f $r.Phase, $r.Seq, $r.Ms, $r.Status) }

function Show-Stats {
	param([string]$Phase, [object[]]$Rows)
	$ok = $Rows | Where-Object Ok
	if (-not $ok) { Write-Log "$Phase : no successful requests"; return }
	$ms = $ok.Ms | Sort-Object
	$p = { param($pct) $i = [math]::Ceiling(($pct/100.0)*$ms.Count)-1; if($i -lt 0){$i=0}; $ms[$i] }
	Write-Log ("$Phase STATS: n={0} ok={1} min={2} avg={3} p50={4} p95={5} max={6} (ms)" -f `
		$Rows.Count, $ok.Count, $ms[0], [math]::Round(($ms | Measure-Object -Average).Average,1), (& $p 50), (& $p 95), $ms[-1])
}

# --- Phase 1: Cold start (stop -> start -> first request) --------------------------------
function Invoke-ColdStartPhase {
	Write-Log "=== PHASE 1: COLD START (stop/start) ==="
	Write-Log "Stopping function app $FunctionApp ..."
	az functionapp stop --name $FunctionApp -g $ResourceGroup | Out-Null
	Write-Log "Stopped. Waiting 60s for full shutdown."
	Start-Sleep -Seconds 60
	Write-Log "Starting function app ..."
	az functionapp start --name $FunctionApp -g $ResourceGroup | Out-Null
	$startUtc = (Get-Date).ToUniversalTime()
	Write-Log "Started at $($startUtc.ToString('o')). Firing first request (cold)."
	$target = "$BaseUrl/Public/Hello World.json?Name=ColdStart"
	# First request(s) - the very first success is the cold-start figure.
	$firstSuccess = $null
	for ($i = 1; $i -le 15; $i++) {
		$r = Invoke-Timed -Url $target -Phase "ColdStart" -Seq $i
		Add-Result $r
		if ($r.Ok -and -not $firstSuccess) { $firstSuccess = $r }
		if ($r.Ok -and $i -ge 5) { break }   # a few more to see warm-up settle
		if (-not $r.Ok) { Start-Sleep -Seconds 3 }
	}
	if ($firstSuccess) { Write-Log ("COLD START first-success latency: {0} ms" -f $firstSuccess.Ms) }
	Show-Stats -Phase "ColdStart" -Rows ($results | Where-Object Phase -eq "ColdStart")
}

# --- Phase 2: Warm steady-state ----------------------------------------------------------
function Invoke-WarmPhase {
	Write-Log "=== PHASE 2: WARM STEADY-STATE ($WarmRequests sequential) ==="
	$target = "$BaseUrl/Public/Hello World.json?Name=Warm"
	for ($i = 1; $i -le $WarmRequests; $i++) { Add-Result (Invoke-Timed -Url $target -Phase "Warm" -Seq $i) }
	Show-Stats -Phase "Warm" -Rows ($results | Where-Object Phase -eq "Warm")
}

# --- Phase 3: Sequential vs Parallel + scale-out -----------------------------------------
function Invoke-ConcurrencyPhase {
	Write-Log "=== PHASE 3: SEQUENTIAL vs PARALLEL ==="
	$target = "$BaseUrl/Public/Examples/Dice Roll Example/Dice Roll.json"

	Write-Log "Sequential: $SeqRequests requests one-at-a-time"
	$seqSw = [System.Diagnostics.Stopwatch]::StartNew()
	for ($i = 1; $i -le $SeqRequests; $i++) { Add-Result (Invoke-Timed -Url $target -Phase "Sequential" -Seq $i) }
	$seqSw.Stop()
	Show-Stats -Phase "Sequential" -Rows ($results | Where-Object Phase -eq "Sequential")
	Write-Log ("Sequential wall-clock: {0}s  throughput: {1} req/s" -f `
		[math]::Round($seqSw.Elapsed.TotalSeconds,1), [math]::Round($SeqRequests/$seqSw.Elapsed.TotalSeconds,2))

	Write-Log "Parallel: $ParRequests requests, throttle=$ParThrottle"
	$parSw = [System.Diagnostics.Stopwatch]::StartNew()
	$parRows = 1..$ParRequests | ForEach-Object -Parallel {
		$u = $using:target
		$sw = [System.Diagnostics.Stopwatch]::StartNew()
		$status = 0; $ok = $false
		try { $r = Invoke-WebRequest -Uri $u -UseBasicParsing -TimeoutSec 120; $sw.Stop(); $status=[int]$r.StatusCode; $ok=$true }
		catch { $sw.Stop(); try { $status=[int]$_.Exception.Response.StatusCode.value__ } catch { $status=-1 } }
		[PSCustomObject]@{ Phase="Parallel"; Seq=$_; TimestampUtc=(Get-Date).ToUniversalTime().ToString("o"); Ms=[math]::Round($sw.Elapsed.TotalMilliseconds,1); Status=$status; Ok=$ok; Bytes=0; Url=$u; Error="" }
	} -ThrottleLimit $ParThrottle
	$parSw.Stop()
	foreach ($r in $parRows) { $results.Add($r) | Out-Null }
	Show-Stats -Phase "Parallel" -Rows $parRows
	Write-Log ("Parallel wall-clock: {0}s  throughput: {1} req/s" -f `
		[math]::Round($parSw.Elapsed.TotalSeconds,1), [math]::Round($ParRequests/$parSw.Elapsed.TotalSeconds,2))
}

# --- Phase 3b: High-concurrency burst (push scale-out further) ---------------------------
function Invoke-HighConcurrencyPhase {
    Write-Log "=== PHASE 3b: HIGH CONCURRENCY ($ParRequests requests, throttle=$ParThrottle) ==="
    $target = "$BaseUrl/Public/Examples/Dice Roll Example/Dice Roll.json"
    $parSw = [System.Diagnostics.Stopwatch]::StartNew()
    $parRows = 1..$ParRequests | ForEach-Object -Parallel {
        $u = $using:target
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $status = 0; $ok = $false
        try { $r = Invoke-WebRequest -Uri $u -UseBasicParsing -TimeoutSec 180; $sw.Stop(); $status=[int]$r.StatusCode; $ok=$true }
        catch { $sw.Stop(); try { $status=[int]$_.Exception.Response.StatusCode.value__ } catch { $status=-1 } }
        [PSCustomObject]@{ Phase="HighConcurrency"; Seq=$_; TimestampUtc=(Get-Date).ToUniversalTime().ToString("o"); Ms=[math]::Round($sw.Elapsed.TotalMilliseconds,1); Status=$status; Ok=$ok; Bytes=0; Url=$u; Error="" }
    } -ThrottleLimit $ParThrottle
    $parSw.Stop()
    foreach ($r in $parRows) { $results.Add($r) | Out-Null }
    Show-Stats -Phase "HighConcurrency" -Rows $parRows
    Write-Log ("HighConcurrency wall-clock: {0}s  throughput: {1} req/s  errors: {2}" -f `
        [math]::Round($parSw.Elapsed.TotalSeconds,1), [math]::Round($ParRequests/$parSw.Elapsed.TotalSeconds,2), (($parRows | Where-Object { -not $_.Ok }).Count))
}

# --- Phase 4: Idle scale-to-zero then cold start -----------------------------------------
function Invoke-IdleColdStartPhase {
	Write-Log "=== PHASE 4: IDLE $IdleMinutes min -> scale-to-zero -> cold start ==="
	Write-Log "Idling (no requests) for $IdleMinutes minutes..."
	Start-Sleep -Seconds ($IdleMinutes * 60)
	Write-Log "Idle complete. Firing request (expected cold after scale-to-zero)."
	$target = "$BaseUrl/Public/Hello World.json?Name=IdleCold"
	$firstSuccess = $null
	for ($i = 1; $i -le 10; $i++) {
		$r = Invoke-Timed -Url $target -Phase "IdleColdStart" -Seq $i
		Add-Result $r
		if ($r.Ok -and -not $firstSuccess) { $firstSuccess = $r }
		if ($r.Ok -and $i -ge 5) { break }
		if (-not $r.Ok) { Start-Sleep -Seconds 3 }
	}
	if ($firstSuccess) { Write-Log ("IDLE COLD START first-success latency: {0} ms" -f $firstSuccess.Ms) }
	Show-Stats -Phase "IdleColdStart" -Rows ($results | Where-Object Phase -eq "IdleColdStart")
}

# --- Telemetry correlation: instance scale-out + perf counters + execution counts -------
# Runs App Insights (KQL) and Azure Monitor queries after the load phases. Retries the
# raw-table queries because `traces`/`dependencies`/`performanceCounters` have 5-10 min
# ingestion lag. IMPORTANT: in this app the `requests`/`exceptions` tables stay EMPTY
# (custom WAREWOLF_APPINSIGHTS_CONNECTION_STRING keeps the host AI pipeline dormant), so
# scale-out is derived from the populated tables (dependencies + performanceCounters +
# traces) and corroborated by the Azure Monitor FunctionExecutionCount/Requests metrics.
function Invoke-AiQuery {
	param([string]$Kql)
	try {
		$json = az monitor app-insights query --app $AiName -g $ResourceGroup --analytics-query $Kql -o json 2>$null
		if (-not $json) { return $null }
		return ($json | ConvertFrom-Json)
	} catch { return $null }
}

function Get-AiRows {
	param($Result)
	if (-not $Result -or -not $Result.tables -or $Result.tables.Count -eq 0) { return @() }
	$t = $Result.tables[0]
	if (-not $t.rows -or $t.rows.Count -eq 0) { return @() }
	return $t.rows
}

function Invoke-CorrelationPhase {
	Write-Log "=== TELEMETRY CORRELATION (instance scale-out + perf counters) ==="
	$startIso = $runStartUtc.AddMinutes(-2).ToString("yyyy-MM-ddTHH:mm:ssZ")
	$endIso   = (Get-Date).ToUniversalTime().AddMinutes(2).ToString("yyyy-MM-ddTHH:mm:ssZ")

	# --- Azure Monitor first: available almost immediately, authoritative volume record ---
	$resource = "/subscriptions/$((az account show --query id -o tsv))/resourceGroups/$ResourceGroup/providers/Microsoft.Web/sites/$FunctionApp"
	Write-Log "Azure Monitor: FunctionExecutionCount / Requests / Http2xx ($startIso -> $endIso)"
	az monitor metrics list --resource $resource --metric "FunctionExecutionCount,Requests,Http2xx" `
		--start-time $startIso --end-time $endIso --interval PT1M -o table 2>$null |
		ForEach-Object { if ($_ -match '\S') { Write-Log "  AM| $_" } }

	# --- App Insights raw tables: wait for ingestion, then retry until instances appear ----
	Write-Log "Waiting $IngestWaitSeconds s for App Insights ingestion before scale-out query..."
	Start-Sleep -Seconds $IngestWaitSeconds

	# Union the POPULATED tables (traces/dependencies/performanceCounters) — never `requests`.
	$instKql = "union traces, dependencies, performanceCounters " +
		"| where timestamp between (datetime($startIso) .. datetime($endIso)) " +
		"| summarize instances=dcount(cloud_RoleInstance), samples=count(), " +
		"firstSeen=min(timestamp), lastSeen=max(timestamp)"

	$instances = 0
	for ($attempt = 1; $attempt -le $IngestRetries; $attempt++) {
		$rows = Get-AiRows (Invoke-AiQuery $instKql)
		if ($rows.Count -gt 0 -and [int]$rows[0][0] -gt 0) {
			$instances = [int]$rows[0][0]
			Write-Log ("Scale-out: instances={0} samples={1} first={2} last={3}" -f `
				$rows[0][0], $rows[0][1], $rows[0][2], $rows[0][3])
			break
		}
		Write-Log "  ingestion not ready (attempt $attempt/$IngestRetries); retry in $IngestRetryWait s"
		if ($attempt -lt $IngestRetries) { Start-Sleep -Seconds $IngestRetryWait }
	}
	if ($instances -eq 0) {
		Write-Log "  WARNING: instance scale-out unavailable from App Insights (ingestion lag). Falling back to Azure Monitor InstanceCount."
		az monitor metrics list --resource $resource --metric "InstanceCount" `
			--start-time $startIso --end-time $endIso --interval PT1M --aggregation Maximum -o table 2>$null |
			ForEach-Object { if ($_ -match '\S') { Write-Log "  AM| $_" } }
	}

	# Per-minute instance timeline from the populated tables (scale-out shape over the burst).
	$timelineKql = "union dependencies, performanceCounters, traces " +
		"| where timestamp between (datetime($startIso) .. datetime($endIso)) " +
		"| summarize instances=dcount(cloud_RoleInstance), samples=count() by bin(timestamp, 1m) " +
		"| order by timestamp asc"
	Write-Log "Instance timeline (per-minute):"
	foreach ($r in (Get-AiRows (Invoke-AiQuery $timelineKql))) { Write-Log ("  TL| {0}  instances={1} samples={2}" -f $r[0], $r[1], $r[2]) }

	# Warewolf server perf counters via App Insights performanceCounters table.
	$pcKql = "performanceCounters " +
		"| where timestamp between (datetime($startIso) .. datetime($endIso)) " +
		"| summarize avgValue=avg(value), maxValue=max(value) by name, cloud_RoleInstance " +
		"| order by name asc"
	Write-Log "Performance counters (per instance):"
	$pcRows = Get-AiRows (Invoke-AiQuery $pcKql)
	if ($pcRows.Count -eq 0) { Write-Log "  (no perfCounter samples yet — ingestion lag; rerun query later)" }
	foreach ($r in $pcRows) { Write-Log ("  PC| {0,-24} inst={1} avg={2} max={3}" -f $r[0], $r[1], $r[2], $r[3]) }
}

# ══════════════════════════════════════════════════════════════════════════════════════════
# NEW PHASES — added on top of the existing harness (existing phases untouched above)
# ══════════════════════════════════════════════════════════════════════════════════════════

# Helper: poll Azure Monitor InstanceCount with no ingestion lag.
# Used immediately after a parallel burst to capture scale-out in real time.
function Get-LiveInstanceCount {
	param([string]$StartIso, [string]$EndIso)
	$resource = "/subscriptions/$((az account show --query id -o tsv))/resourceGroups/$ResourceGroup/providers/Microsoft.Web/sites/$FunctionApp"
	try {
		$json = az monitor metrics list --resource $resource --metric "InstanceCount" `
			--start-time $StartIso --end-time $EndIso --interval PT1M `
			--aggregation Maximum -o json 2>$null | ConvertFrom-Json
		$max = ($json.value[0].timeseries[0].data | Where-Object { $_.maximum -gt 0 } | Measure-Object -Property maximum -Maximum).Maximum
		return [int]$max
	} catch { return 0 }
}

# ── NEW PHASE: Seq5 ── 5 sequential requests, time each individually ──────────────────────
# Satisfies requirement 2: execute 5 requests in sequence and measure time for each request.
function Invoke-Seq5Phase {
	param([string]$Label = "Seq5")
	Write-Log "=== PHASE $Label : $SmallBatch sequential requests (individual timing) ==="
	$target = "$BaseUrl/Public/Hello World.json?Name=$Label"
	for ($i = 1; $i -le $SmallBatch; $i++) {
		$r = Invoke-Timed -Url $target -Phase $Label -Seq $i
		Add-Result $r
		Write-Log ("  [$i/$SmallBatch] {0} ms  http={1}" -f $r.Ms, $r.Status)
	}
	Show-Stats -Phase $Label -Rows ($results | Where-Object Phase -eq $Label)
}

# ── NEW PHASE: Par5 ── 5 parallel requests, time each + immediate instance count check ───
# Satisfies requirements 3 and 4: parallel timing per-request + scale-out check.
function Invoke-Par5Phase {
	param([string]$Label = "Par5")
	Write-Log "=== PHASE $Label : $SmallBatch parallel requests (individual timing + instance check) ==="
	$target = "$BaseUrl/Public/Hello World.json?Name=$Label"
	$burstStart = (Get-Date).ToUniversalTime()

	$parRows = 1..$SmallBatch | ForEach-Object -Parallel {
		$u  = $using:target
		$lbl = $using:Label
		$sw = [System.Diagnostics.Stopwatch]::StartNew()
		$status = 0; $ok = $false; $err = ""
		try {
			$r = Invoke-WebRequest -Uri $u -UseBasicParsing -TimeoutSec 120
			$sw.Stop(); $status = [int]$r.StatusCode; $ok = $true
		} catch {
			$sw.Stop()
			try { $status = [int]$_.Exception.Response.StatusCode.value__ } catch { $status = -1 }
			$err = $_.Exception.Message
		}
		[PSCustomObject]@{
			Phase        = $lbl
			Seq          = $_
			TimestampUtc = (Get-Date).ToUniversalTime().ToString("o")
			Ms           = [math]::Round($sw.Elapsed.TotalMilliseconds, 1)
			Status       = $status
			Ok           = $ok
			Bytes        = 0
			Url          = $u
			Error        = $err
		}
	} -ThrottleLimit $SmallBatch

	$burstEnd = (Get-Date).ToUniversalTime()
	foreach ($r in $parRows) {
		$results.Add($r) | Out-Null
		Write-Log ("  [seq={0}] {1} ms  http={2}" -f $r.Seq, $r.Ms, $r.Status)
	}
	Show-Stats -Phase $Label -Rows $parRows

	# Immediate instance count poll — Azure Monitor has no ingestion lag
	Write-Log "Polling Azure Monitor InstanceCount (no ingestion lag)..."
	$startIso = $burstStart.AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
	$endIso   = $burstEnd.AddMinutes(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
	$instances = Get-LiveInstanceCount -StartIso $startIso -EndIso $endIso
	Write-Log ("  $Label scale-out: {0} instance(s) detected during burst" -f $(if ($instances -gt 0) { $instances } else { "unknown (metric not yet available — retry with -Correlate)" }))
	return $instances
}

# ── NEW PHASE: ColdVsWarm ── runs Seq5 + Par5 under both cold and warm conditions ─────────
# Satisfies requirement 5: measure 2, 3, 4 with cold start and warm start.
function Invoke-ColdVsWarmPhase {
	Write-Log "=== PHASE ColdVsWarm : Seq5 + Par5 under COLD then WARM conditions ==="

	# ── COLD RUN ──
	Write-Log "--- Cold run: stopping function app to force cold start ---"
	az functionapp stop --name $FunctionApp -g $ResourceGroup | Out-Null
	Write-Log "Stopped. Waiting 60s for full shutdown."
	Start-Sleep -Seconds 60
	az functionapp start --name $FunctionApp -g $ResourceGroup | Out-Null
	Write-Log "Started. Firing cold Seq5..."
	Invoke-Seq5Phase -Label "ColdSeq5"

	Write-Log "Firing cold Par5..."
	Invoke-Par5Phase -Label "ColdPar5"

	# ── WARM RUN ──
	Write-Log "--- Warm run: sending $WarmRequests warm-up requests to stabilise JIT ---"
	$warmTarget = "$BaseUrl/Public/Hello World.json?Name=WarmUp"
	for ($i = 1; $i -le $WarmRequests; $i++) {
		Invoke-WebRequest -Uri $warmTarget -UseBasicParsing -TimeoutSec 60 | Out-Null
	}
	Write-Log "Warm-up complete. Firing warm Seq5..."
	Invoke-Seq5Phase -Label "WarmSeq5"

	Write-Log "Firing warm Par5..."
	Invoke-Par5Phase -Label "WarmPar5"

	# ── COMPARISON SUMMARY ──
	Write-Log "=== ColdVsWarm COMPARISON ==="
	$phases = @("ColdSeq5", "WarmSeq5", "ColdPar5", "WarmPar5")
	foreach ($ph in $phases) {
		$rows = $results | Where-Object Phase -eq $ph | Where-Object Ok
		if ($rows) {
			$ms = $rows.Ms | Sort-Object
			Write-Log ("  {0,-12} n={1} first={2}ms avg={3}ms p95={4}ms max={5}ms" -f `
				$ph, $rows.Count, ($rows | Select-Object -First 1).Ms,
				[math]::Round(($ms | Measure-Object -Average).Average, 1),
				$ms[[math]::Max(0, [math]::Ceiling(0.95 * $ms.Count) - 1)],
				$ms[-1])
		}
	}
}

# ── NEW PHASE: LoadSeq100 ── 100 sequential requests load test ───────────────────────────
# Satisfies requirement 6: Load testing — 100 sequential requests.
function Invoke-LoadSeqPhase {
	Write-Log "=== PHASE LoadSeq : $LoadCount sequential requests (load test) ==="
	$target = "$BaseUrl/Public/Hello World.json?Name=LoadSeq"
	$wallSw = [System.Diagnostics.Stopwatch]::StartNew()
	$errors = 0
	for ($i = 1; $i -le $LoadCount; $i++) {
		$r = Invoke-Timed -Url $target -Phase "LoadSeq" -Seq $i
		Add-Result $r
		if (-not $r.Ok) { $errors++ }
		# Progress every 10 requests
		if ($i % 10 -eq 0) {
			$elapsed = [math]::Round($wallSw.Elapsed.TotalSeconds, 1)
			$rps     = [math]::Round($i / $wallSw.Elapsed.TotalSeconds, 2)
			Write-Log ("  Progress: {0}/{1}  errors={2}  elapsed={3}s  rps={4}" -f $i, $LoadCount, $errors, $elapsed, $rps)
		}
	}
	$wallSw.Stop()
	Show-Stats -Phase "LoadSeq" -Rows ($results | Where-Object Phase -eq "LoadSeq")
	Write-Log ("LoadSeq LOAD TEST: total={0} errors={1} wall={2}s throughput={3} req/s" -f `
		$LoadCount, $errors,
		[math]::Round($wallSw.Elapsed.TotalSeconds, 1),
		[math]::Round($LoadCount / $wallSw.Elapsed.TotalSeconds, 2))
}

# ── NEW PHASE: LoadPar100 ── 100 parallel requests load test ─────────────────────────────
# Satisfies requirement 7: Load testing — 100 parallel requests.
function Invoke-LoadParPhase {
	Write-Log "=== PHASE LoadPar : $LoadCount parallel requests (load test, throttle=$LoadThrottle) ==="
	$target   = "$BaseUrl/Public/Hello World.json?Name=LoadPar"
	$burstStart = (Get-Date).ToUniversalTime()
	$wallSw   = [System.Diagnostics.Stopwatch]::StartNew()

	$parRows = 1..$LoadCount | ForEach-Object -Parallel {
		$u = $using:target
		$sw = [System.Diagnostics.Stopwatch]::StartNew()
		$status = 0; $ok = $false; $err = ""
		try {
			$r = Invoke-WebRequest -Uri $u -UseBasicParsing -TimeoutSec 180
			$sw.Stop(); $status = [int]$r.StatusCode; $ok = $true
		} catch {
			$sw.Stop()
			try { $status = [int]$_.Exception.Response.StatusCode.value__ } catch { $status = -1 }
			$err = $_.Exception.Message
		}
		[PSCustomObject]@{
			Phase        = "LoadPar"
			Seq          = $_
			TimestampUtc = (Get-Date).ToUniversalTime().ToString("o")
			Ms           = [math]::Round($sw.Elapsed.TotalMilliseconds, 1)
			Status       = $status
			Ok           = $ok
			Bytes        = 0
			Url          = $u
			Error        = $err
		}
	} -ThrottleLimit $LoadThrottle

	$wallSw.Stop()
	$burstEnd = (Get-Date).ToUniversalTime()
	foreach ($r in $parRows) { $results.Add($r) | Out-Null }

	$errors = ($parRows | Where-Object { -not $_.Ok }).Count
	Show-Stats -Phase "LoadPar" -Rows $parRows
	Write-Log ("LoadPar LOAD TEST: total={0} errors={1} wall={2}s throughput={3} req/s" -f `
		$LoadCount, $errors,
		[math]::Round($wallSw.Elapsed.TotalSeconds, 1),
		[math]::Round($LoadCount / $wallSw.Elapsed.TotalSeconds, 2))

	# Instance count check after parallel burst
	Write-Log "Polling Azure Monitor InstanceCount after LoadPar burst..."
	$startIso = $burstStart.AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")
	$endIso   = $burstEnd.AddMinutes(2).ToString("yyyy-MM-ddTHH:mm:ssZ")
	$instances = Get-LiveInstanceCount -StartIso $startIso -EndIso $endIso
	Write-Log ("  LoadPar scale-out: {0} instance(s) detected during burst" -f $(if ($instances -gt 0) { $instances } else { "unknown — retry with -Correlate" }))
}

# --- Main --------------------------------------------------------------------------------
Write-Log "RUN $runStamp  app=$FunctionApp  base=$BaseUrl  phases=$($Phases -join ',')"
Get-Targets | ForEach-Object { Write-Log "  target: $_" }

# ── Existing phases (untouched) ──────────────────────────────────────────────────────────
if ($Phases -contains "ColdStart")       { Invoke-ColdStartPhase }
if ($Phases -contains "Warm")            { Invoke-WarmPhase }
if ($Phases -contains "Concurrency")     { Invoke-ConcurrencyPhase }
if ($Phases -contains "HighConcurrency") { Invoke-HighConcurrencyPhase }
if ($Phases -contains "IdleColdStart")   { Invoke-IdleColdStartPhase }
if ($Correlate -or ($Phases -contains "Correlate")) { Invoke-CorrelationPhase }

# ── New phases ───────────────────────────────────────────────────────────────────────────

# Fire 5 requests one after another and record how long each individual request takes.
# This tells you the steady-state response time when requests are not competing with each other.
if ($Phases -contains "Seq5")            { Invoke-Seq5Phase }

# Fire 5 requests all at the same time and record how long each one takes.
# Also checks Azure Monitor immediately after the burst to see how many instances
# the Function App scaled out to in order to handle the load.
if ($Phases -contains "Par5")            { Invoke-Par5Phase | Out-Null }

# Runs the 5-sequential and 5-parallel tests twice — once right after a forced cold start
# (app has been stopped and restarted, so the first requests pay the full startup cost)
# and again after the app has been warmed up (JIT compiled, caches hot).
# Prints a side-by-side comparison so you can see exactly how much cold start hurts.
if ($Phases -contains "ColdVsWarm")      { Invoke-ColdVsWarmPhase }

# Sends 100 requests back-to-back (one at a time) and measures the time for each.
# Reports total wall-clock time, requests per second, and error count.
# Use this to understand single-threaded throughput and spot latency drift over time.
if ($Phases -contains "LoadSeq")         { Invoke-LoadSeqPhase }

# Sends 100 requests all at once (up to $LoadThrottle threads at a time) and measures
# how long each request takes under real concurrency pressure.
# Also checks how many instances the Function App scaled out to during the burst.
# Use this to find your concurrency ceiling and scaling behaviour under load.
if ($Phases -contains "LoadPar")         { Invoke-LoadParPhase }

# Convenience shortcut — runs all five new phases above in one go.
if ($Phases -contains "AllNew") {
	Invoke-Seq5Phase
	Invoke-Par5Phase | Out-Null
	Invoke-ColdVsWarmPhase
	Invoke-LoadSeqPhase
	Invoke-LoadParPhase
}

$results | Export-Csv -Path $csv -NoTypeInformation
Write-Log "DONE. Rows=$($results.Count)  csv=$csv  log=$log"
Write-Host "`n=== SUMMARY (first-success / percentiles) ==="
$results | Group-Object Phase | ForEach-Object {
	$ok = $_.Group | Where-Object Ok
	if ($ok) {
		$ms = ($ok.Ms | Sort-Object)
		[PSCustomObject]@{
			Phase = $_.Name
			N     = $_.Count
			OK    = $ok.Count
			First_ms = ($_.Group | Where-Object Ok | Select-Object -First 1).Ms
			Min_ms= $ms[0]
			Avg_ms= [math]::Round(($ms | Measure-Object -Average).Average,1)
			Max_ms= $ms[-1]
		}
	}
} | Format-Table -AutoSize
