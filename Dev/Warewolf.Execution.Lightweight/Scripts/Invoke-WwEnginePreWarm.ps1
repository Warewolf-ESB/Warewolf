#Requires -Version 7.0
<#
.SYNOPSIS
    Warms a Lightweight Execution Engine (Azure Functions Consumption plan) before a
    burst of concurrent messages/requests, so the burst is not absorbed by cold-start
    and scale-out.

.DESCRIPTION
    Serves TWO callers with different authentication shapes, unified into one script
    rather than forked into two:

      * pipeline-LOADTEST.yml's ShovelBridgeLoadTest_ExternalServiceBus job already mints
        an Entra access token for the daemon app immediately before running the load
        test, and passes it straight through via -EngineBaseUrl/-AccessToken - no extra
        token-minting dependency needed for that path.
      * Invoke-WwQueueLoadTest.ps1 (Container Apps / KEDA queue-processor path) has no
        pre-minted token of its own; it passes -EngineAppName/-EngineAppId (+
        -ResourceGroup/-TenantId) and this script mints one itself via
        WwE2E.Common.psm1's Get-E2EEngineToken, the same helper the E2E verification
        harness uses.

    Measured against `wwengine-e2e-ldi413` on 2026-08-12/13:

        first request after idle    64,757 ms      (cold start)
        requests 2-5                 3,1xx ms      (warm, sequential)
        RUN 1, no pre-warm, 10 replicas   17x502, 8x503, 6x504 - 31 of 100 messages discarded
        RUN 2, pre-warmed,   6 replicas   0 failures, 100/100

    Two phases, because they warm different things:

      PHASE A (sequential) wakes ONE instance and proves the workflow path is healthy.
      Stops early once latency is stable, so a warm engine costs a few seconds rather
      than a fixed toll.

      PHASE B (concurrent) forces the platform to SCALE OUT to roughly the number of
      instances the burst will need. Phase A alone is not enough: it leaves a single
      warm instance and the burst still pays scale-out for the rest.

      Phase B RAMPS toward -TargetConcurrency rather than opening at it (see
      Get-WarmupConcurrencyLadder): for -TargetConcurrency 20 over 3 rounds the ladder
      is 5 -> 10 -> 20. Opening straight at 20 against a Consumption-plan engine slams a
      still-single-instance app with 60 simultaneous executions, which is how the
      2026-08-24 pipeline-LOADTEST run recorded OK=22/60 at a 41.7s median. The LAST
      round is always the full -TargetConcurrency, so the final-round verdict still
      means "clean at target concurrency" and nothing weaker.

    NON-FATAL BY CONTRACT. This script always exits 0 once its parameters validate. An
    engine that will not warm is a finding to report, not a reason to abort the caller's
    stage - the caller decides whether to proceed, retry, or lower -TargetConcurrency.
    Parameter validation still throws: that is a configuration error, not a warm-up
    outcome.

    Every call executes the real workflow via its `Secure/{workflow}` HTTP route (same
    `WorkflowExecutor`/Roslyn-compile pool both the ServiceBus trigger and the queue
    processor call into), so IT WRITES REAL ROWS to whatever the workflow persists. Use
    `-MessagePrefix` to make them identifiable, and take a MAX(id) watermark after
    warming (not before) so the run's own rows can be counted separately.

.PARAMETER EngineBaseUrl
    Base URL of the target engine, e.g. https://warewolfserver-uat.azurewebsites.net (no
    trailing slash required). Either this or -EngineAppName is required.
.PARAMETER EngineAppName
    Function App name, used to derive the base URL (https://{EngineAppName}.azurewebsites.net)
    when -EngineBaseUrl is not given, and to mint a token via -EngineAppId when
    -AccessToken is not given.
.PARAMETER EngineAppId
    Entra application (client) ID of the engine's Easy Auth app registration. Required
    when minting a token (i.e. -AccessToken was not supplied).
.PARAMETER ResourceGroup
    Resource group containing -EngineAppName. Only used when minting a token.
.PARAMETER TenantId
    Entra tenant ID. Only used when minting a token.
.PARAMETER WorkflowRoute
    Route (relative to the resolved base URL) of the workflow to warm, via the engine's
    anonymous-but-token-checked Secure route (see WorkflowHttpFunction.cs). Default:
    'Secure/rabbit/RabbitProcess' - RabbitProcess's DataList exposes a single Input,
    'message', which is what Format-WarmupRequestBody populates.
.PARAMETER AccessToken
    A valid Entra bearer token for the engine's Secure route, as a SecureString. Either
    this or -EngineAppName/-EngineAppId (to mint one) is required.
.PARAMETER TargetConcurrency
    The concurrency the burst will produce, and the concurrency of Phase B's FINAL round.
    For the queue path this is maxReplicas x WORKER__MAXCONCURRENCY (with MaxConcurrency=1
    it is simply the replica count); for pipeline-LOADTEST.yml's ShovelBridge job this is
    $(PublishConcurrency), a proxy for the execution-side concurrency the Service Bus
    trigger will fan out to.
.PARAMETER MaxSequential / StableStreak / StableThresholdMs
    Phase A tuning: stop after StableStreak consecutive calls under StableThresholdMs,
    or after MaxSequential calls, whichever comes first.
.PARAMETER ConcurrentRounds
    Number of Phase B rounds. Concurrency ramps across them and reaches
    -TargetConcurrency on the last one.
.PARAMETER MessagePrefix
    Prefix for the identifiable warm-up messages (default 'WARMUP').
.PARAMETER TimeoutSec
    Per-call HTTP timeout. Exceeding it is recorded as a TIMEOUT result, not an error.
.PARAMETER DryRun
    Prints what would be sent (URL, body shape, phase plan, ramp ladder) without making
    any HTTP calls. Mutates nothing.
.PARAMETER LoadFunctionsOnly
    Test hook - dot-source this script with -LoadFunctionsOnly to load the pure helper
    functions (Get-WarmupUri, Format-WarmupRequestBody, Update-LatencyStreak,
    Test-WarmupRoundClean, ConvertTo-WarmupResult, Get-WarmupConcurrencyLadder,
    Format-WarmupCodeSummary) without running Phase A/B or making any network call.

.EXAMPLE
    $token = ConvertTo-SecureString $env:ShovelE2EAccessTokenValue -AsPlainText -Force
    .\Invoke-WwEnginePreWarm.ps1 -EngineBaseUrl 'https://warewolfserver-uat.azurewebsites.net' `
        -AccessToken $token -TargetConcurrency 20

.EXAMPLE
    .\Invoke-WwEnginePreWarm.ps1 -EngineAppName wwengine-e2e-ldi413 `
        -EngineAppId e0029ee1-e9a1-42b4-98e5-6585954340f8 -TargetConcurrency 6

.NOTES
    See docs/ShovelBridge-Architecture.md's 2026-08-16 entry for the failure signature
    (Service Bus MessageLockLost + Roslyn cold-start OutOfMemoryException +
    token-validation cancellations under a 1000-message burst) this script is intended to
    prevent by warming the target engine immediately before the burst is published, and
    its 2026-08-24 entry for the two defects fixed here (a per-call timeout killing the
    whole step, and Phase B opening at full concurrency).
#>
[CmdletBinding()]
param(
    [string]       $EngineBaseUrl,
    [string]       $EngineAppName,
    [string]       $EngineAppId,
    [string]       $ResourceGroup      = 'DEV2',
    [string]       $TenantId           = 'ca0cc53b-9af4-4067-bcdf-be9c648450d1',
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

# Normalises BOTH outcomes of a warm-up call into one shape, so a transport-level failure
# is a COUNTED RESULT rather than an error record.
#
# This is the fix for the 2026-08-24 pipeline-LOADTEST failure: -SkipHttpErrorCheck
# suppresses non-2xx STATUS CODES only. A -TimeoutSec expiry is a TaskCanceledException -
# an exception, not a response - so it sails straight past that switch. Combined with a
# script-level $ErrorActionPreference = 'Stop' it killed the run mid-Phase-B, before the
# non-fatal verdict at the bottom of this script was ever reached.
#
# Code 0 is the sentinel for "no HTTP response at all"; Test-WarmupRoundClean already
# treats any non-200 as unclean, so a timeout correctly makes a round dirty.
function ConvertTo-WarmupResult {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][long] $ElapsedMs,
        [AllowNull()] $Response,
        [AllowNull()] $Exception
    )
    if ($Exception) {
        $msg = if ($Exception -is [string]) { $Exception } else { [string]$Exception.Message }
        $msg = ($msg -replace '\s+', ' ').Trim()
        return [pscustomobject]@{
            Code     = 0
            Ms       = $ElapsedMs
            TimedOut = [bool]($msg -match 'Timeout|timed out|cancell?ed')
            Body     = $msg.Substring(0, [math]::Min(220, $msg.Length))
        }
    }
    $code = [int]$Response.StatusCode
    $body = ([string]$Response.Content -replace '\s+', ' ')
    [pscustomobject]@{
        Code     = $code
        Ms       = $ElapsedMs
        TimedOut = $false
        Body     = $(if ($code -ne 200) { $body.Substring(0, [math]::Min(220, $body.Length)) } else { '' })
    }
}

# Phase B's ramp. Round i of R runs at ceil(Target / 2^(R-i)), so 20 over 3 rounds is
# 5 -> 10 -> 20. The last round is FORCED to the full target regardless of rounding, so
# the final-round verdict keeps meaning "clean at target concurrency".
function Get-WarmupConcurrencyLadder {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int] $TargetConcurrency,
        [Parameter(Mandatory)][int] $Rounds
    )
    if ($Rounds -lt 1) { return @() }
    $ladder = @(foreach ($i in 1..$Rounds) {
        $divisor = [math]::Pow(2, $Rounds - $i)
        [math]::Max(1, [int][math]::Ceiling($TargetConcurrency / $divisor))
    })
    $ladder[-1] = [math]::Max(1, $TargetConcurrency)
    $ladder
}

# Renders a round's result codes for the log. Code 0 is not a status code, so it prints as
# TIMEOUT/ERROR rather than a misleading '0x3'.
function Format-WarmupCodeSummary {
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyCollection()][array] $Results)
    if ($Results.Count -eq 0) { return '(no results)' }
    ($Results |
        ForEach-Object {
            if ($_.Code -ne 0)   { [string]$_.Code }
            elseif ($_.TimedOut) { 'TIMEOUT' }
            else                 { 'ERROR' }
        } |
        Group-Object |
        Sort-Object Name |
        ForEach-Object { "$($_.Name)x$($_.Count)" }) -join ' '
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
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][array] $Results,
        [Parameter(Mandatory)][int] $ExpectedCount
    )
    ($Results.Count -eq $ExpectedCount) -and (@($Results | Where-Object { $_.Code -ne 200 }).Count -eq 0)
}

if ($LoadFunctionsOnly) { return }

if ([string]::IsNullOrWhiteSpace($EngineBaseUrl) -and [string]::IsNullOrWhiteSpace($EngineAppName)) {
    throw "-EngineBaseUrl is required (or -EngineAppName, to derive https://{EngineAppName}.azurewebsites.net)."
}
if (-not $AccessToken -and (-not $EngineAppName -or -not $EngineAppId)) {
    throw "-AccessToken is required (a SecureString bearer token), or -EngineAppName and -EngineAppId so one can be minted via WwE2E.Common.psm1's Get-E2EEngineToken."
}

$resolvedBaseUrl = if ($EngineBaseUrl) { $EngineBaseUrl } else { "https://$EngineAppName.azurewebsites.net" }

if ($AccessToken) {
    $plainToken = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($AccessToken))
}
else {
    Import-Module (Join-Path $PSScriptRoot 'WwE2E.Common.psm1') -Force
    $plainToken = Get-E2EEngineToken -TenantId $TenantId -EngineAppId $EngineAppId `
                                     -FunctionAppName $EngineAppName -ResourceGroup $ResourceGroup
}

$headers = @{ Authorization = "Bearer $plainToken"; 'Content-Type' = 'application/json' }
$uri     = Get-WarmupUri -BaseUrl $resolvedBaseUrl -Route $WorkflowRoute
$ladder  = @(Get-WarmupConcurrencyLadder -TargetConcurrency $TargetConcurrency -Rounds $ConcurrentRounds)

Write-Host "  Engine  : $uri"
Write-Host "  Auth    : $(if ($AccessToken) { '-AccessToken (supplied)' } else { 'minted via Get-E2EEngineToken' })"
Write-Host "  Target  : concurrency $TargetConcurrency"

if ($DryRun) {
    Write-Host "  [DryRun] Phase A: up to $MaxSequential sequential call(s), stop after $StableStreak consecutive < ${StableThresholdMs}ms."
    Write-Host "  [DryRun] Phase B: $ConcurrentRounds round(s), ramping concurrency $($ladder -join ' -> ')."
    Write-Host "  [DryRun] Sample body: $(Format-WarmupRequestBody -MessagePrefix $MessagePrefix -Label 'seq-1')"
    return
}

# -ErrorAction Stop is REQUIRED for the catch to fire inside a ForEach-Object -Parallel
# runspace, where $ErrorActionPreference defaults to 'Continue' (child runspaces do not
# inherit it) - without it an Invoke-WebRequest exception is written non-terminating and
# sails past the try/catch. Kept identical in both phases so they behave the same.
function Invoke-WarmupCall {
    param([string] $Label)
    $body = Format-WarmupRequestBody -MessagePrefix $MessagePrefix -Label $Label
    $sw   = [Diagnostics.Stopwatch]::StartNew()
    try {
        $r = Invoke-WebRequest $uri -Method Post -Headers $headers -Body $body `
                 -SkipHttpErrorCheck -TimeoutSec $TimeoutSec -ErrorAction Stop
        ConvertTo-WarmupResult -ElapsedMs $sw.ElapsedMilliseconds -Response $r
    }
    catch {
        ConvertTo-WarmupResult -ElapsedMs $sw.ElapsedMilliseconds -Exception $_.Exception
    }
}

function Format-WarmupCallLabel {
    param([Parameter(Mandatory)] $Result)
    if ($Result.Code -ne 0)   { 'HTTP {0}' -f $Result.Code }
    elseif ($Result.TimedOut) { 'TIMEOUT' }
    else                      { 'ERROR' }
}

# ── Phase A: sequential, until latency settles ──────────────────────────────
Write-Host ''
Write-Host "-- Phase A: sequential (stop after $StableStreak consecutive < ${StableThresholdMs}ms)" -ForegroundColor Yellow
$streak = 0
for ($i = 1; $i -le $MaxSequential; $i++) {
    $res = Invoke-WarmupCall "seq-$i"
    '   {0,2}  {1,-9} {2,7}ms' -f $i, (Format-WarmupCallLabel -Result $res), $res.Ms | Write-Host
    if ($res.Code -ne 200 -and $res.Body) { Write-Host "        $($res.Body)" -ForegroundColor DarkYellow }
    $streak = Update-LatencyStreak -CurrentStreak $streak -StatusCode $res.Code -ElapsedMs $res.Ms -ThresholdMs $StableThresholdMs
    if ($streak -ge $StableStreak) { Write-Host '   [+] latency stable' -ForegroundColor Green; break }
}
if ($streak -lt $StableStreak) {
    Write-Host "   [!] never reached $StableStreak stable calls - the engine may still be scaling; consider re-running" -ForegroundColor DarkYellow
}

# ── Phase B: ramping to the target concurrency, to force scale-out ──────────
Write-Host ''
Write-Host "-- Phase B: $ConcurrentRounds round(s), ramping concurrency $($ladder -join ' -> ')" -ForegroundColor Yellow

# ForEach-Object -Parallel relays a child runspace's errors into THIS scope's error stream,
# where a script-level 'Stop' would turn them terminating. The try/catch inside the
# scriptblock is the real fix; scoping the preference around the loop is the second line of
# defence, so this script cannot die on a warm-up outcome.
$resultFn = ${function:ConvertTo-WarmupResult}.ToString()
$clean = $false
foreach ($round in 1..$ConcurrentRounds) {
    $concurrency = $ladder[$round - 1]
    $n = $concurrency * 3
    $res = @(& {
        $ErrorActionPreference = 'Continue'
        1..$n | ForEach-Object -ThrottleLimit $concurrency -Parallel {
            # Functions do not cross into a parallel runspace; re-establish the one we need
            # from its source text so both phases share a single tested implementation.
            ${function:ConvertTo-WarmupResult} = $using:resultFn
            $h  = @{ Authorization = $using:headers.Authorization; 'Content-Type' = 'application/json' }
            $b  = @{ inputParameters = @{ message = ("$using:MessagePrefix-c$using:round-$_") } } | ConvertTo-Json -Compress
            $sw = [Diagnostics.Stopwatch]::StartNew()
            try {
                $r = Invoke-WebRequest $using:uri -Method Post -Headers $h -Body $b `
                         -SkipHttpErrorCheck -TimeoutSec $using:TimeoutSec -ErrorAction Stop
                ConvertTo-WarmupResult -ElapsedMs $sw.ElapsedMilliseconds -Response $r
            }
            catch {
                ConvertTo-WarmupResult -ElapsedMs $sw.ElapsedMilliseconds -Exception $_.Exception
            }
        }
    })

    $ok    = @($res | Where-Object { $_.Code -eq 200 }).Count
    $times = @($res | ForEach-Object { $_.Ms } | Sort-Object)
    $med   = if ($times.Count) { $times[[math]::Floor($times.Count / 2)] } else { 0 }
    $max   = if ($times.Count) { $times[-1] } else { 0 }
    '   round {0}: conc={1,-3} OK={2}/{3}  med={4}ms  max={5}ms   codes: {6}' -f `
        $round, $concurrency, $ok, $n, $med, $max, (Format-WarmupCodeSummary -Results $res) | Write-Host

    foreach ($bad in @($res | Where-Object { $_.Code -ne 200 } | Select-Object -First 2)) {
        Write-Host "      [$(Format-WarmupCallLabel -Result $bad)] $($bad.Body)" -ForegroundColor Red
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
Write-Host "        Take a MAX(id) watermark NOW, before publishing, so the run's own rows can be counted separately." -ForegroundColor DarkGray

# Explicit, so the non-fatal contract is enforceable and testable: a warm-up OUTCOME never
# fails the caller's stage. Only the parameter validation above throws.
exit 0
