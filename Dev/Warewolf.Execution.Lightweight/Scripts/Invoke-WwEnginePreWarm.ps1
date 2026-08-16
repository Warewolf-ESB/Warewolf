#Requires -Version 7.0
<#
.SYNOPSIS
    Warms a Lightweight Execution Engine (Azure Functions Consumption plan) before a
    burst of concurrent messages/requests, so the burst is not absorbed by cold-start
    and scale-out.

.DESCRIPTION
    Adapted from `origin/8504-Execution-Engine-Queue-Processor-End-to-end-testing`'s
    script of the same name (commit `bcca73646e`), which was built and measured for the
    Container Apps/KEDA queue-processor path but targets exactly the same underlying
    problem this repo's ServiceBus-triggered load test hits: on the Consumption (Y1 /
    Dynamic) plan the platform adds instances gradually and sheds load while it does. A
    burst arriving into that window does not fail because a workflow is wrong - it fails
    because the request/message never reaches a warm instance in time. The original
    script measured, against `wwengine-e2e-ldi413` on 2026-08-12/13:

        first request after idle    64,757 ms      (cold start)
        requests 2-5                 3,1xx ms      (warm, sequential)
        RUN 1, no pre-warm, 10 replicas   17x502, 8x503, 6x504 - 31 of 100 messages discarded
        RUN 2, pre-warmed,   6 replicas   0 failures, 100/100

    This adaptation is intentionally self-contained rather than a straight port: the
    original depends on `WwE2E.Common.psm1` (its own `Get-E2EEngineToken` helper), which
    was never ported to this branch. `pipeline-LOADTEST.yml` already mints an Entra
    access token for the same daemon app (`ShovelE2EDaemonClientSecret`) immediately
    before running the load test - this script accepts that token directly via
    `-AccessToken` instead of minting its own, so no new pipeline secret or dependency is
    needed.

    Two phases, because they warm different things:

      PHASE A (sequential) wakes ONE instance and proves the workflow path is healthy.
      Stops early once latency is stable, so a warm engine costs a few seconds rather
      than a fixed toll.

      PHASE B (concurrent, at the target concurrency) forces the platform to SCALE OUT
      to roughly the number of instances the burst will need. Phase A alone is not
      enough: it leaves a single warm instance and the burst still pays scale-out for
      the rest.

    Every call executes the real workflow via its `Secure/{workflow}` HTTP route (same
    `WorkflowExecutor`/Roslyn-compile pool the ServiceBus trigger uses - see
    `docs/ShovelBridge-Architecture.md`'s 2026-08-16 entry), so IT WRITES REAL ROWS to
    whatever the workflow persists. Use `-MessagePrefix` to make them identifiable.

.PARAMETER EngineBaseUrl
    REQUIRED. Base URL of the target engine, e.g. https://warewolfserver-uat.azurewebsites.net
    (no trailing slash required).
.PARAMETER WorkflowRoute
    Route (relative to EngineBaseUrl) of the workflow to warm, via the engine's
    anonymous-but-token-checked Secure route (see WorkflowHttpFunction.cs). Default:
    'Secure/rabbit/RabbitProcess' - the same workflow the ShovelBridge load test drives.
.PARAMETER AccessToken
    REQUIRED. A valid Entra bearer token for the engine's Secure route, as a SecureString.
    Minted the same way the caller already mints one for the workflow-execution
    verification leg (client-credentials against the ShovelBridge E2E daemon app) - this
    script does not mint its own.
.PARAMETER TargetConcurrency
    The concurrency the burst will produce. For pipeline-LOADTEST.yml's
    ShovelBridgeLoadTest_ExternalServiceBus job this is $(PublishConcurrency) (the
    publish-side concurrency), used here as a proxy for the execution-side concurrency
    the Service Bus trigger will fan out to.
.PARAMETER MaxSequential / StableStreak / StableThresholdMs
    Phase A tuning: stop after StableStreak consecutive calls under StableThresholdMs,
    or after MaxSequential calls, whichever comes first.
.PARAMETER ConcurrentRounds
    Number of Phase B rounds run at TargetConcurrency.
.PARAMETER MessagePrefix
    Prefix for the identifiable warm-up messages (default 'WARMUP').
.PARAMETER TimeoutSec
    Per-call HTTP timeout.
.PARAMETER DryRun
    Prints what would be sent (URL, body shape, phase plan) without making any HTTP
    calls. Mutates nothing.
.PARAMETER LoadFunctionsOnly
    Test hook — dot-source this script with -LoadFunctionsOnly to load the pure helper
    functions (Get-WarmupUri, Format-WarmupRequestBody, Update-LatencyStreak,
    Test-WarmupRoundClean) without running Phase A/B or making any network call.

.EXAMPLE
    $token = ConvertTo-SecureString $env:ShovelE2EAccessTokenValue -AsPlainText -Force
    .\Invoke-WwEnginePreWarm.ps1 -EngineBaseUrl 'https://warewolfserver-uat.azurewebsites.net' `
        -AccessToken $token -TargetConcurrency 20

.NOTES
    See docs/ShovelBridge-Architecture.md's 2026-08-16 entry for the failure signature
    (Service Bus MessageLockLost + Roslyn cold-start OutOfMemoryException +
    token-validation cancellations under a 1000-message burst) this script is intended to
    prevent by warming the target engine immediately before the burst is published.
#>
[CmdletBinding()]
param(
    [string]       $EngineBaseUrl,
    [string]       $WorkflowRoute      = 'Secure/rabbit/RabbitProcess',
    [securestring] $AccessToken,

    [int]    $TargetConcurrency  = 6,
    [int]    $MaxSequential      = 14,
    [int]    $StableStreak       = 4,
    [int]    $StableThresholdMs  = 5000,
    [int]    $ConcurrentRounds   = 3,
    [string] $MessagePrefix      = 'WARMUP',
    [int]    $TimeoutSec         = 240,

    [switch] $DryRun,
    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

$ErrorActionPreference = 'Stop'

function Get-WarmupUri {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $BaseUrl, [Parameter(Mandatory)][string] $Route)
    "$($BaseUrl.TrimEnd('/'))/$($Route.TrimStart('/'))"
}

function Format-WarmupRequestBody {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $MessagePrefix, [Parameter(Mandatory)][string] $Label)
    @{ inputParameters = @{ message = "$MessagePrefix-$Label" } } | ConvertTo-Json -Compress
}

# Pure streak tracker for Phase A: a 200 under the threshold extends the streak,
# anything else resets it. Kept separate from the HTTP call so it's independently testable.
function Update-LatencyStreak {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int] $CurrentStreak,
        [Parameter(Mandatory)][int] $StatusCode,
        [Parameter(Mandatory)][long] $ElapsedMs,
        [Parameter(Mandatory)][int] $ThresholdMs
    )
    if ($StatusCode -eq 200 -and $ElapsedMs -lt $ThresholdMs) { $CurrentStreak + 1 } else { 0 }
}

# Pure round-cleanliness check for Phase B: true only when every result in the round was
# a 200, and the round produced the expected number of results.
function Test-WarmupRoundClean {
    [CmdletBinding()]
    param([Parameter(Mandatory)][array] $Results, [Parameter(Mandatory)][int] $ExpectedCount)
    ($Results.Count -eq $ExpectedCount) -and (@($Results | Where-Object { $_.Code -ne 200 }).Count -eq 0)
}

if ($LoadFunctionsOnly) { return }

if ([string]::IsNullOrWhiteSpace($EngineBaseUrl)) { throw "-EngineBaseUrl is required." }
if (-not $AccessToken) { throw "-AccessToken is required (a SecureString bearer token for the engine's Secure route)." }

$plainToken = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($AccessToken))
$headers = @{ Authorization = "Bearer $plainToken"; 'Content-Type' = 'application/json' }
$uri = Get-WarmupUri -BaseUrl $EngineBaseUrl -Route $WorkflowRoute

Write-Host "  Engine  : $uri"
Write-Host "  Target  : concurrency $TargetConcurrency"

if ($DryRun) {
    Write-Host "  [DryRun] Phase A: up to $MaxSequential sequential call(s), stop after $StableStreak consecutive < ${StableThresholdMs}ms."
    Write-Host "  [DryRun] Phase B: $ConcurrentRounds round(s) at concurrency $TargetConcurrency ($($TargetConcurrency * 3) calls/round)."
    Write-Host "  [DryRun] Sample body: $(Format-WarmupRequestBody -MessagePrefix $MessagePrefix -Label 'seq-1')"
    return
}

function Invoke-WarmupCall {
    param([string] $Label)
    $body = Format-WarmupRequestBody -MessagePrefix $MessagePrefix -Label $Label
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $r  = Invoke-WebRequest $uri -Method Post -Headers $headers -Body $body -SkipHttpErrorCheck -TimeoutSec $TimeoutSec
    [pscustomobject]@{ Code = [int]$r.StatusCode; Ms = $sw.ElapsedMilliseconds }
}

# ── Phase A: sequential, until latency settles ──────────────────────────────
Write-Host ''
Write-Host "-- Phase A: sequential (stop after $StableStreak consecutive < ${StableThresholdMs}ms)" -ForegroundColor Yellow
$streak = 0
for ($i = 1; $i -le $MaxSequential; $i++) {
    $res = Invoke-WarmupCall "seq-$i"
    '   {0,2}  HTTP {1}  {2,7}ms' -f $i, $res.Code, $res.Ms | Write-Host
    $streak = Update-LatencyStreak -CurrentStreak $streak -StatusCode $res.Code -ElapsedMs $res.Ms -ThresholdMs $StableThresholdMs
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
        $h = @{ Authorization = $using:headers.Authorization; 'Content-Type' = 'application/json' }
        $b = @{ inputParameters = @{ message = ("$using:MessagePrefix-c$using:round-$_") } } | ConvertTo-Json -Compress
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
    $clean = Test-WarmupRoundClean -Results $res -ExpectedCount $n
}

Write-Host ''
if ($clean) {
    Write-Host "  [+] WARM - the final round was clean at concurrency $TargetConcurrency. Safe to publish." -ForegroundColor Green
} else {
    # Not fatal by design - a cold-start failure here is exactly what this script exists to
    # absorb before the real burst. The caller decides whether to proceed, retry, or lower
    # -TargetConcurrency rather than this script silently failing the whole pipeline stage.
    Write-Host "  [!] the final round still had failures. Publishing now may reproduce them." -ForegroundColor Red
    Write-Host "      Re-run this script, or lower -TargetConcurrency." -ForegroundColor Red
}
Write-Host "  NOTE: warm-up executed the real workflow, so it has written rows tagged '$MessagePrefix-*'." -ForegroundColor DarkGray
