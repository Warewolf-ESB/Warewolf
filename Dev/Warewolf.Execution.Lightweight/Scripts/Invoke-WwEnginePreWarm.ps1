#Requires -Version 7.0
<#
.SYNOPSIS
    Warms an Azure Functions Consumption-plan Execution Engine before a burst, so the burst is not
    absorbed by cold-start and scale-out.

.DESCRIPTION
    On the Consumption (Y1 / Dynamic) plan the platform adds instances gradually and sheds load while
    it does. A burst arriving into that window does not fail because the workflow is wrong - it fails
    because the request never reaches the workflow. Measured on 2026-08-12/13 against
    wwengine-e2e-ldi413:

        first request after idle    64,757 ms      (cold start)
        requests 2-5                 3,1xx ms      (warm, sequential)
        RUN 1, no pre-warm, 10 replicas   17x502, 8x503, 6x504 - 31 of 100 messages discarded
        RUN 2, pre-warmed,   6 replicas   0 failures, 100/100

    Two phases, because they warm different things:

      PHASE A (sequential) wakes ONE instance and proves the workflow path is healthy. Stops early
      once latency is stable, so a warm engine costs a few seconds rather than a fixed toll.

      PHASE B (concurrent, at the target concurrency) forces the platform to SCALE OUT to roughly the
      number of instances the burst will need. Phase A alone is not enough: it leaves a single warm
      instance and the burst still pays scale-out for the rest.

    Every call executes the real workflow, so IT WRITES REAL ROWS to whatever the workflow persists.
    Use -MessagePrefix to make them identifiable, and take a MAX(id) watermark after warming so the
    run's own rows can be counted separately.

.PARAMETER TargetConcurrency
    The concurrency the burst will produce. For the queue path this is maxReplicas x
    WORKER__MAXCONCURRENCY - with MaxConcurrency=1 it is simply the replica count.

.EXAMPLE
    .\Invoke-WwEnginePreWarm.ps1 -EngineAppName wwengine-e2e-ldi413 `
        -EngineAppId e0029ee1-e9a1-42b4-98e5-6585954340f8 -TargetConcurrency 6
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $EngineAppName,
    [Parameter(Mandatory)][string] $EngineAppId,
    [string] $ResourceGroup    = 'DEV2',
    [string] $TenantId         = 'ca0cc53b-9af4-4067-bcdf-be9c648450d1',
    [string] $WorkflowRoute    = 'Secure/rabbit/RabbitProcess.json',

    [int]    $TargetConcurrency = 6,
    [int]    $MaxSequential     = 14,
    [int]    $StableStreak      = 4,
    [int]    $StableThresholdMs = 5000,
    [int]    $ConcurrentRounds  = 3,
    [string] $MessagePrefix     = 'WARMUP',
    [int]    $TimeoutSec        = 240
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) 'WwE2E.Common.psm1') -Force

$uri = "https://$EngineAppName.azurewebsites.net/$($WorkflowRoute.TrimStart('/'))"
Write-Host "  Engine  : $uri"
Write-Host "  Target  : concurrency $TargetConcurrency"

# The token is fetched ONCE here and reused. It is short-lived: a long warm-up followed by a long
# burst can outlive it, which surfaces as a wall of HTTP 401 rather than anything to do with load.
# The worker refreshes its own token (WwExecutionTokenHandler); ad-hoc callers like this one do not.
$token = Get-E2EEngineToken -TenantId $TenantId -EngineAppId $EngineAppId `
                            -FunctionAppName $EngineAppName -ResourceGroup $ResourceGroup
$headers = @{ Authorization = "Bearer $token"; 'Content-Type' = 'application/json' }

function Invoke-One {
    param([string] $Label)
    $body = @{ inputParameters = @{ message = ('{"orderId":"' + $Label + '","amount":1}') } } | ConvertTo-Json -Compress
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $r  = Invoke-WebRequest $uri -Method Post -Headers $headers -Body $body -SkipHttpErrorCheck -TimeoutSec $TimeoutSec
    [pscustomobject]@{ Code = [int]$r.StatusCode; Ms = $sw.ElapsedMilliseconds }
}

# ── Phase A: sequential, until latency settles ──────────────────────────────
Write-Host ''
Write-Host "-- Phase A: sequential (stop after $StableStreak consecutive < ${StableThresholdMs}ms)" -ForegroundColor Yellow
$streak = 0
for ($i = 1; $i -le $MaxSequential; $i++) {
    $res = Invoke-One "$MessagePrefix-seq-$i"
    '   {0,2}  HTTP {1}  {2,7}ms' -f $i, $res.Code, $res.Ms | Write-Host
    if ($res.Code -eq 200 -and $res.Ms -lt $StableThresholdMs) { $streak++ } else { $streak = 0 }
    if ($streak -ge $StableStreak) { Write-Host '   [+] latency stable' -ForegroundColor Green; break }
}
if ($streak -lt $StableStreak) {
    Write-Host "   [!] never reached $StableStreak stable calls - the engine may still be scaling; consider re-running" -ForegroundColor DarkYellow
}

# ── Phase B: at the target concurrency, to force scale-out ──────────────────
Write-Host ''
Write-Host "-- Phase B: $ConcurrentRounds round(s) at concurrency $TargetConcurrency" -ForegroundColor Yellow
$clean = $false
foreach ($round in 1..$ConcurrentRounds) {
    $n = $TargetConcurrency * 3
    $res = 1..$n | ForEach-Object -ThrottleLimit $TargetConcurrency -Parallel {
        $h = @{ Authorization = "Bearer $using:token"; 'Content-Type' = 'application/json' }
        $b = @{ inputParameters = @{ message = ('{"orderId":"' + $using:MessagePrefix + '-c' + $using:round + '-' + $_ + '","amount":1}') } } | ConvertTo-Json -Compress
        $sw = [Diagnostics.Stopwatch]::StartNew()
        $r  = Invoke-WebRequest $using:uri -Method Post -Headers $h -Body $b -SkipHttpErrorCheck -TimeoutSec $using:TimeoutSec
        $c  = ([string]$r.Content -replace '\s+', ' ')
        [pscustomobject]@{
            Code = [int]$r.StatusCode
            Ms   = $sw.ElapsedMilliseconds
            Body = $(if ([int]$r.StatusCode -ne 200) { $c.Substring(0, [math]::Min(220, $c.Length)) } else { '' })
        }
    }
    $ok    = @($res | Where-Object { $_.Code -eq 200 }).Count
    $times = @($res | ForEach-Object { $_.Ms } | Sort-Object)
    $codes = ($res | Group-Object Code | ForEach-Object { "$($_.Name)x$($_.Count)" }) -join ' '
    '   round {0}: OK={1}/{2}  med={3}ms  max={4}ms   codes: {5}' -f `
        $round, $ok, $n, $times[[math]::Floor($times.Count / 2)], $times[-1], $codes | Write-Host

    foreach ($bad in @($res | Where-Object { $_.Code -ne 200 } | Select-Object -First 2)) {
        Write-Host "      [$($bad.Code)] $($bad.Body)" -ForegroundColor Red
    }
    $clean = ($ok -eq $n)
}

Write-Host ''
if ($clean) {
    Write-Host "  [+] WARM - the final round was clean at concurrency $TargetConcurrency. Safe to publish." -ForegroundColor Green
} else {
    # Not fatal, but the caller should decide rather than discover it mid-burst.
    Write-Host "  [!] the final round still had failures. Publishing now will likely reproduce them." -ForegroundColor Red
    Write-Host "      Re-run this script, or lower -TargetConcurrency." -ForegroundColor Red
}
Write-Host "  NOTE: warm-up executed the real workflow, so it has written rows. Take a MAX(id)" -ForegroundColor DarkGray
Write-Host "        watermark NOW, before publishing, so the run's own rows can be counted." -ForegroundColor DarkGray
