#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end control load run on profiler.profile.request: pre-warm, publish N exact-shape
    messages, wait for the drain, reconcile against dbo.jobs1.

.DESCRIPTION
    WHY THIS EXISTS ALONGSIDE Invoke-WwQueueLoadTest.ps1. The harness publishes the RANDOMISED
    profiler body, which varies MandateCanTrack and ResultType. The true+Success combination reaches
    Profiler.usp_GetPublicHolidaysInDateRange, which DOES NOT EXIST in WarewolfDevOpsTestDb
    (OBJECT_ID returns -1), and such a message returns HTTP 200, is acked, and leaves NO row - it
    disappears silently. This script instead publishes the one body proven to execute cleanly, so a
    throughput number is not contaminated by that gap. Use the harness when you want the randomised
    business coverage; use this when you want a clean measurement.

    ONLY MandateId VARIES, because it is the database reconciliation key: the engine's logged
    document carries it and it is read back with JSON_VALUE(MessageContent, '$.MandateId'). Hold it
    constant and N rows become indistinguishable, making duplicate execution invisible.

    THE BASELINE IS CAPTURED BEFORE PUBLISHING, NOT ASSUMED. dbo.jobs1 is truncated between runs in
    this environment (observed twice on 2026-09-11), which reseeds the identity, so "rows that
    appeared" must be measured against a recorded high-water mark rather than a row count.

    THE DEAD-LETTER QUEUE IS ONLY MEASURED, NEVER READ. On 2026-09-11 a BasicGet/close read of this
    broker's DLQ - the pattern Invoke-WwQueueLoadTest.ps1 uses, which AMQP says requeues unacked
    messages on channel close - was followed by the queue going from 10 messages to 0 with no
    consumer attached. Cause unconfirmed, but until it is, this script reports DEPTH only. Pass
    -PeekDeadLetters to read txn ids anyway, accepting that risk.

.EXAMPLE
    $env:WWLOADTEST_SQLCONNECTION = '<connection string>'
    .\Run-WwProfilerExactLoad.ps1 -Count 1000
#>
[CmdletBinding()]
param(
    [ValidateRange(1, 100000)][int] $Count = 1000,

    # The known-good request. Its query string IS the payload contract - the 16 fields are parsed
    # from here so they cannot drift, and 'wid' is stripped as a transport concern.
    [string] $Url = 'https://wwuat3.azurewebsites.net/secure/ProfilerServices/ProfilerWrapper/RequestForProfiling.json?ForPaymentCycle=202402&MandateCanTrack=false&MandateDRI=22&MandateId=109800B6-2183-EB11-A812-000D3ABE1AD1&RelatedMandates=5B9800B6-2183-EB11-A812-000D3ABE1AD1&ResultDate=2024-01-27&ResultDebitRunId=22&ResultMethod=DebiCheck&ResultPaymentCycle=202401&ResultPresentedOnDate=2024-01-22&ResultType=Success&ScoreThisResult=true&IsFixedExtract=false&SourcePaymentId=1234&RequestType=&FASGroup=',

    [string] $QueueName       = 'profiler.profile.request',
    [string] $DeadLetterQueue = 'profiler.profile.error.request',
    [string] $SourceBitePath  = 'G:\uat3\sources\ProfilerRabbitMQSource.bite',
    [string] $PublishPath     = 'G:\uat3\Apps\QProcessor',
    [string] $LogRoot         = 'G:\uat3\logs',
    [string] $JobTable        = 'dbo.jobs1',

    # Engine pre-warm. 'Public/apis.json' loads the assemblies WITHOUT executing a workflow, which
    # is why it is no longer the default: it never populates the per-workflow pool (see
    # -PreWarmRoute). The workflow-route warm was the main source of the 'Insufficient memory' OOMs
    # on the Y1 plan, but that was at SIX concurrent executions - Invoke-WwEnginePreWarm.ps1's own
    # default, which this script used to inherit silently by never passing -TargetConcurrency. It
    # now passes -PreWarmConcurrency (2) explicitly. Pass -SkipPreWarm to measure a cold ramp on
    # purpose.
    [string] $EngineAppName   = 'wwuat3',
    [string] $ResourceGroup   = 'DEV2',
    # EMPTY MEANS "DERIVE THE WORKFLOW ROUTE FROM -Url", which is the default and is deliberate.
    # 'Public/apis.json' is discovery-only - WorkflowExecutor's OPENAPI branch generates the spec
    # from the DataList and explicitly does NO XAML load and NO execution - so it never populates
    # WorkflowExecutor._workflowPool, the per-workflow pool of compiled DynamicActivity + parsed
    # IDev2Activity chains that is the whole point of warming. That pool "grows to the peak
    # concurrency seen for that workflow and no further" (WorkflowExecutor.cs), so only executing
    # THIS workflow, concurrently, amortises the XAML compile and the parse. Pass
    # 'Public/apis.json' explicitly to get the old assemblies-only warm.
    [string] $PreWarmRoute,
    # The workflow inputs the warm-up sends. EMPTY MEANS "PARSE THEM FROM -Url", so the warm-up and
    # the publish cannot drift: -Url's query string already IS the payload contract for this script.
    # Supplying neither, against a workflow route, is the trap this exists to close - see the
    # fail-fast below and Invoke-WwEnginePreWarm.ps1's -InputParameters help.
    [hashtable] $PreWarmInputParameters,
    # Forwarded as Invoke-WwEnginePreWarm.ps1 -TargetConcurrency. NOT optional in practice: that
    # script defaults to 6, and 6 concurrent workflow-route warms is the documented main source of
    # the 'Insufficient memory' OOMs on the Y1 plan. 2 matches the engine's deployed
    # ServiceBusTrigger maxConcurrentExecutions, so the pool is warmed to exactly the depth a run
    # will actually rent - warming deeper only risks the OOM for pool slots nothing will use.
    [int]    $PreWarmConcurrency = 2,
    # Left empty on purpose: filled from WwLoadTest.Defaults.psd1 at run time so the engine's Entra
    # identity is defined in exactly one place. Pass them to override.
    [string] $EngineAppId,
    [string] $EngineTenantId,
    [switch] $SkipPreWarm,

    [string] $SqlConnectionString,
    [int]    $DrainTimeoutSeconds = 1800,
    [int]    $DrainPollSeconds    = 15,
    # Rows must stop rising for this long before the run is called finished. An EMPTY QUEUE IS NOT
    # THE SIGNAL: 2xx->ack and non-2xx->dead-letter+ack both drain it, so a run can look complete
    # while executions are still in flight.
    [int]    $DrainQuietSeconds   = 45,
    [switch] $PeekDeadLetters,

    # Queue depth is read over the MANAGEMENT HTTP API, not AMQP: RabbitMQ.Client 7.0.0 on .NET 10
    # stack-overflows in its socket reader while an AMQP connection is torn down, killing this
    # process. Required, because the management endpoint is frequently a different host from the
    # AMQP one (here: HTTPS through a proxy vs a raw TCP tunnel), so it cannot be derived from the
    # source .bite.
    [string] $RabbitMqManagementUri,

    [switch] $LoadFunctionsOnly          # test hook — define the pure helpers then return
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# ═════════════════════════════════════════════════════════════════════════════
# Pure helpers — no network, no SQL, no dot-sourcing. Defined ABOVE the
# -LoadFunctionsOnly return so Pester can exercise them without the script
# reaching its SQL-connection guard or loading Invoke-WwQueueLoadTest.ps1.
# ═════════════════════════════════════════════════════════════════════════════

function Get-WwProfilerFieldsFromUrl {
    <#
        Parses -Url's query string into the workflow's input fields.

        THIS IS THE SAME PARSE Publish-WwProfilerExact.ps1 DOES on the same -Url, and it is
        duplicated deliberately rather than shared: the obvious home is WwE2E.Common.psm1, but
        moving it there widens a load-test fix into a change to the module every deployment script
        imports. If a third caller ever needs it, promote it then.

        Empty values are KEPT, not dropped - 'RequestType=' and 'FASGroup=' are both legitimately
        empty in the canonical request, and a warm-up that omits them is not warming the same
        workflow shape the run will publish. 'wid' and friends are transport concerns the engine
        supplies itself, so they are stripped exactly as the publisher strips them.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Url)

    $reserved = @('wid', 'workflowName', 'workflowFilePath', 'isDebug')
    $fields   = @{}
    $query    = ([uri]$Url).Query.TrimStart('?')
    if (-not $query) { return $fields }

    foreach ($kv in $query -split '&') {
        if (-not $kv) { continue }
        $pair = $kv -split '=', 2
        $name = [uri]::UnescapeDataString($pair[0])
        if ($name -in $reserved) { continue }
        $fields[$name] = if ($pair.Count -eq 2) { [uri]::UnescapeDataString($pair[1]) } else { '' }
    }
    return $fields
}

function Get-WwProfilerPreWarmRoute {
    <#
        Derives the warm-up route from -Url by keeping the URL's OWN path and prefix (so a -Url
        pointing at /secure/... warms /secure/..., not some other route this script invented).
        The query string is dropped: Invoke-WwEnginePreWarm.ps1 POSTs the inputs as a body.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Url)

    return ([uri]$Url).AbsolutePath.TrimStart('/')
}

if ($LoadFunctionsOnly) { return }

$stamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$runDir    = Join-Path $LogRoot "exactload-$stamp"
$manifest  = Join-Path $runDir 'manifest.json'
$label     = "EXL$stamp"

# PHASE CLOCKS. A single wall-clock number cannot answer "how long did it take": pre-warm, publish
# and processing have completely different characters, and on a large burst the publish itself stops
# being negligible. Each phase is timed separately and all of them are persisted, so the analysis
# can report both the processing rate and a truthful end-to-end figure.
$runStartUtc   = (Get-Date).ToUniversalTime()
$preWarmStart  = $null; $preWarmEnd = $null
$publishStart  = $null; $publishEnd = $null
$drainStart    = $null; $drainEnd   = $null
$preWarmFailed = $false

function Head($t) { Write-Host ''; Write-Host "== $t" -ForegroundColor Cyan }
function Kv($k, $v) { Write-Host ("   {0,-26} {1}" -f $k, $v) }

# DOT-SOURCING RUNS IN THIS SCOPE, SO IT BINDS ITS OWN PARAMETERS OVER OURS.
# Invoke-WwQueueLoadTest.ps1 declares parameters that collide with seven of this script's -
# QueueName, DeadLetterQueue, JobTable, EngineAppName, ResourceGroup, SkipPreWarm and
# SqlConnectionString - and dot-sourcing it assigns ITS defaults (mostly empty) to those variables
# here. The first symptom was '$JobTable' going empty, which made the baseline query end in
# "AS n FROM " and SQL Server report 'Incorrect syntax near FROM'; SqlConnectionString being blanked
# at the same time was masked by the environment-variable fallback below.
# So snapshot every declared parameter, dot-source, then restore. Restoring by name from
# MyCommand.Parameters covers any parameter added later without this needing an edit.
$myParams = @{}
foreach ($name in $MyInvocation.MyCommand.Parameters.Keys) {
    $v = Get-Variable -Name $name -Scope 0 -ErrorAction SilentlyContinue
    if ($v) { $myParams[$name] = $v.Value }
}

. (Join-Path $ScriptDir 'Invoke-WwQueueLoadTest.ps1') -LoadFunctionsOnly
Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force

foreach ($name in $myParams.Keys) { Set-Variable -Name $name -Value $myParams[$name] -Scope 0 }

# Never accept the connection string as a plain command-line argument by preference: arguments are
# visible in the process list to every user on the box. The environment variable is the normal path.
if (-not $SqlConnectionString) { $SqlConnectionString = $env:WWLOADTEST_SQLCONNECTION }
if (-not $SqlConnectionString) {
    throw 'Set $env:WWLOADTEST_SQLCONNECTION (or pass -SqlConnectionString) so the run can be reconciled.'
}

# Invoke-WwSqlQuery returns a NESTED array. @() does not unroll it - @($result).Count is 1 for any
# number of rows, which silently reported "no rows" during a run that had in fact succeeded.
function Rows {
    <#
        RETRY IS NOT OPTIONAL HERE. Azure SQL drops connections transiently, and a drain loop polls
        for as long as the run lasts: a 15000-message run died at message 13671 of 15000, after 85
        minutes, on a single

            "error occurred during the pre-login handshake (provider: SSL Provider,
             error: 0 - The wait operation timed out)"

        One blip must never destroy an otherwise complete run. Every query retries with linear
        backoff, and -Tolerant additionally lets the DRAIN POLL give up on this iteration and simply
        poll again - a momentarily unreachable database says nothing about the run's progress.
    #>
    param([Parameter(Mandatory)][string] $Query, [int] $Retries = 4, [switch] $Tolerant)
    for ($attempt = 1; $attempt -le $Retries; $attempt++) {
        try {
            return Invoke-WwSqlQuery -ConnectionString $SqlConnectionString -Query $Query -TimeoutSec 180 |
                ForEach-Object { $_ }
        }
        catch {
            $first = ($_.Exception.Message -split "`n")[0].Trim()
            if ($attempt -lt $Retries) {
                Write-Host "   (sql attempt $attempt/$Retries failed, retrying: $first)" -ForegroundColor DarkYellow
                Start-Sleep -Seconds (2 * $attempt)
                continue
            }
            if ($Tolerant) {
                Write-Host "   (sql unavailable after $Retries attempts; will re-poll: $first)" -ForegroundColor DarkYellow
                return $null
            }
            throw
        }
    }
}

Head 'Step 1 - baseline'
$base = Rows "SELECT ISNULL(MAX(JobLogId),0) AS MaxId, COUNT(*) AS n FROM $JobTable"
$watermark = [int]$base.MaxId
Kv 'jobs1 max JobLogId' $watermark
Kv 'jobs1 rows' $base.n
Kv 'run directory' $runDir
$amqp = Resolve-E2EBrokerUri -SourceBitePath $SourceBitePath
if (-not $amqp) { throw "Could not resolve the broker URI from '$SourceBitePath'." }
Initialize-E2ERabbitClient -PublishPath $PublishPath
if (-not $RabbitMqManagementUri) {
    throw ('-RabbitMqManagementUri is required: queue depth is read over the management HTTP API ' +
           '(an AMQP connection teardown crashes this process on .NET 10). ' +
           'Example: -RabbitMqManagementUri https://rabbitmq.warewolf.online')
}
$brokerCtx = Get-E2EBrokerContextFromUri -AmqpUri $amqp
Kv 'management api' "$RabbitMqManagementUri (vhost '$($brokerCtx.VHost)')"

$q0 = Get-E2EQueueDepthHttp -ManagementUri $RabbitMqManagementUri -QueueName $QueueName `
        -VHost $brokerCtx.VHost -Credential $brokerCtx.Credential
$d0 = Get-E2EQueueDepthHttp -ManagementUri $RabbitMqManagementUri -QueueName $DeadLetterQueue `
        -VHost $brokerCtx.VHost -Credential $brokerCtx.Credential
Kv 'queue ready / consumers' "$($q0.Messages) / $($q0.Consumers)"
Kv 'dlq ready (baseline)'    $d0.Messages
if ($q0.Messages -gt 0) {
    Write-Host "   WARNING: $($q0.Messages) message(s) already queued - they will count as unexpected." -ForegroundColor Yellow
}

Head 'Step 2 - engine pre-warm'
if ($SkipPreWarm) { Kv 'skipped' 'by -SkipPreWarm' }
else {
    # The pre-warm mints its own bearer token by CLIENT CREDENTIALS, so it needs the engine's Entra
    # APP ID as well as its name - Invoke-WwEnginePreWarm.ps1:261-263 throws without both. Read the
    # guids from WwLoadTest.Defaults.psd1 instead of duplicating them here, so there is one source
    # of truth if the engine app is ever recreated.
    if (-not $EngineAppId -or -not $EngineTenantId) {
        $defaults = Import-PowerShellDataFile -Path (Join-Path $ScriptDir 'WwLoadTest.Defaults.psd1')
        if (-not $EngineAppId)    { $EngineAppId    = $defaults.EngineAppId }
        if (-not $EngineTenantId) { $EngineTenantId = $defaults.EngineTenantId }
    }
    Kv 'engine app id' $EngineAppId

    # Resolve route and inputs together: both default to being derived from -Url, so the warm-up
    # exercises exactly the workflow, and exactly the payload shape, the publish is about to send.
    if (-not $PreWarmRoute) { $PreWarmRoute = Get-WwProfilerPreWarmRoute -Url $Url }
    $warmInputs = if ($PreWarmInputParameters -and $PreWarmInputParameters.Count -gt 0) {
        $PreWarmInputParameters
    } else {
        Get-WwProfilerFieldsFromUrl -Url $Url
    }

    # FAIL FAST RATHER THAN WARM WITH AN EMPTY DATALIST. Invoke-WwEnginePreWarm.ps1's built-in body
    # carries only 'message', which no profiler workflow declares, so a workflow route with no
    # inputs executes against an empty datalist and EVERY call fails on the workflow's own missing
    # variables ("Scalar value { MandateDRI } is NULL ... Invalid Calculate") - 119/119 on
    # 2026-09-14, initially misread as a broken workflow fixture. Because the pre-warm is non-fatal
    # by design (see the try/catch below), that failure would otherwise be swallowed and the run
    # would report a successful warm-up having warmed nothing. Discovery routes are exempt: they
    # take no inputs.
    $isDiscoveryRoute = $PreWarmRoute -match 'apis\.json$'
    if (-not $isDiscoveryRoute -and $warmInputs.Count -eq 0) {
        throw ("-PreWarmRoute '$PreWarmRoute' is a workflow route but no inputs resolved: -Url has " +
               'no query string and -PreWarmInputParameters was not supplied. Pass one of them, or ' +
               "use -PreWarmRoute 'Public/apis.json' for a discovery-only (assemblies) warm that " +
               'does NOT populate the per-workflow pool.')
    }

    Kv 'pre-warm route' $PreWarmRoute
    Kv 'pre-warm inputs' "$($warmInputs.Count) field(s)$(if ($isDiscoveryRoute) { ' (discovery route - not sent)' })"
    Kv 'pre-warm concurrency' $PreWarmConcurrency

    # NON-FATAL BY DESIGN. A cold engine makes a run slower, not invalid - but an aborted script
    # leaves 1000 messages unpublished and nothing measured. Invoke-WwEnginePreWarm.ps1 promises to
    # exit 0 once its PARAMETERS validate; a parameter problem still throws, and that is exactly
    # what killed a 1000-message run before it published anything.
    $preWarmStart = (Get-Date).ToUniversalTime()
    try {
        $warmArgs = @{
            EngineAppName     = $EngineAppName
            EngineAppId       = $EngineAppId
            TenantId          = $EngineTenantId
            ResourceGroup     = $ResourceGroup
            WorkflowRoute     = $PreWarmRoute
            TargetConcurrency = $PreWarmConcurrency
        }
        # Discovery routes take no inputs; passing them would only build a body the route ignores.
        if (-not $isDiscoveryRoute) { $warmArgs['InputParameters'] = $warmInputs }
        & (Join-Path $ScriptDir 'Invoke-WwEnginePreWarm.ps1') @warmArgs
    }
    catch {
        Write-Host "   PRE-WARM FAILED (continuing cold): $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host '   Expect a slower first batch and higher invocation percentiles.' -ForegroundColor Yellow
        $preWarmFailed = $true
    }
    $preWarmEnd = (Get-Date).ToUniversalTime()
    Kv 'pre-warm seconds' ([math]::Round(($preWarmEnd - $preWarmStart).TotalSeconds, 1))
}

Head "Step 3 - publish $Count message(s)"
# RE-TAKE THE WATERMARK AFTER THE PRE-WARM, NOT BEFORE. Invoke-WwEnginePreWarm.ps1 says so itself:
# a warm-up aimed at a WORKFLOW route executes that workflow and writes its own rows. Those rows sit
# above a watermark taken in step 1 and would be reported as 'unexpected rows' - or, worse, quietly
# inflate the row count towards the expected total and mask real losses.
# THE DEFAULT ROUTE NOW DOES EXACTLY THAT. It was 'Public/apis.json' - discovery-only, writing
# nothing (confirmed: newRows stayed 0 across a 33-request warm-up) - which made this re-take look
# like cheap insurance. The default is now the workflow route derived from -Url, precisely so the
# warm-up executes the workflow and populates its pool, so it writes one row per warm-up call every
# run. This re-take is load-bearing, not insurance.
$preWarmWatermark = [int](Rows "SELECT ISNULL(MAX(JobLogId),0) AS MaxId FROM $JobTable").MaxId
if ($preWarmWatermark -ne $watermark) {
    Kv 'watermark moved by pre-warm' "$watermark -> $preWarmWatermark (warm-up wrote $($preWarmWatermark - $watermark) row(s))"
    $watermark = $preWarmWatermark
}
$publishStart = (Get-Date).ToUniversalTime()
# A DIRECT CALL, NOT A MANUAL CHILD PROCESS. Publish-WwProfilerExact.ps1 now isolates its own
# RabbitMQ.Client teardown crash internally - it launches itself as a child process and reads its
# result back from a file written before that teardown is attempted (see the SAFE TO CALL DIRECTLY
# note in its help) - so calling it in-process here can no longer bring this script down with it.
# Its returned object is already trustworthy, but this script still reconciles against the
# manifest below rather than that object, since the manifest is what Step 5 depends on regardless.
$pubScript = Join-Path $ScriptDir 'Publish-WwProfilerExact.ps1'
# -RabbitMqManagementUri is forwarded so the publisher PROVES the burst against the broker's own
# message_stats.publish counter instead of its own loop count. Without it a publish to a missing
# exchange is counted as sent and manifested, and the run measures a drain that never had anything
# to drain (2026-09-19: "published 20000", broker counter unmoved at 2, 311 of 20000 on the retry).
& $pubScript -Url $Url -QueueName $QueueName -Label $label -Count $Count `
    -MandateIdMode 'Unique' -ManifestPath $manifest -ScriptDir $ScriptDir `
    -SourceBitePath $SourceBitePath -PublishPath $PublishPath `
    -RabbitMqManagementUri $RabbitMqManagementUri | Out-Null

if (-not (Test-Path -LiteralPath $manifest)) {
    throw "The publisher wrote no manifest at '$manifest' - treat the run as having published nothing."
}
$publishedRows = @(Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json)
if ($publishedRows.Count -eq 0) { throw "Manifest '$manifest' is empty - nothing was published." }
$burst = [pscustomobject]@{
    Total = $publishedRows.Count
    Bytes = $publishedRows[0].bytes
}
if ($burst.Total -ne $Count) {
    Write-Warning "Publisher recorded $($burst.Total) of $Count message(s); reconciling against what the manifest actually lists."
}
$publishEnd = (Get-Date).ToUniversalTime()
# Kept for the existing throughput line below, which measures from the END of publishing.
$publishedUtc = Get-Date
Kv 'published' $burst.Total
Kv 'bytes each' $burst.Bytes
Kv 'publish seconds' ([math]::Round(($publishEnd - $publishStart).TotalSeconds, 1))
Kv 'publish rate (msg/s)' $(
    $ps = ($publishEnd - $publishStart).TotalSeconds
    if ($ps -gt 0) { [math]::Round($burst.Total / $ps, 1) } else { 'instant' })
Kv 'manifest' $manifest

Head 'Step 4 - drain'
# TWO INDEPENDENT CLOCKS, AND BOTH MUST BE SATISFIED.
#
# 'Rows stopped rising' is NOT on its own a finish signal. During a cold start the row count sits at
# 0 for tens of seconds while the replica boots, so a quiet timer started then is already satisfied
# the instant the queue empties - and the queue empties EARLY, because KEDA/QueueDeclare report only
# READY messages and a prefetched batch in flight is invisible. A 2-message smoke run hit exactly
# this: declared drained at 02:42:04, and both rows landed at 02:42:29 and 02:42:31, producing a
# false 'silent loss' verdict on a run that was completely clean.
#
# So: emptySince measures how long the queue has been empty (reset whenever work reappears), and
# rowsQuietSince measures how long the row count has been unchanged. Finishing needs both, plus at
# least one row, so a cold start cannot masquerade as completion.
$drainStart     = (Get-Date).ToUniversalTime()
$deadline       = (Get-Date).AddSeconds($DrainTimeoutSeconds)
$lastRows       = -1
$rowsQuietSince = $null
$emptySince     = $null
while ((Get-Date) -lt $deadline) {
    $q = Get-E2EQueueDepthHttp -ManagementUri $RabbitMqManagementUri -QueueName $QueueName `
            -VHost $brokerCtx.VHost -Credential $brokerCtx.Credential
    $d = Get-E2EQueueDepthHttp -ManagementUri $RabbitMqManagementUri -QueueName $DeadLetterQueue `
            -VHost $brokerCtx.VHost -Credential $brokerCtx.Credential
    $rowCount = Rows -Query "SELECT COUNT(*) AS n FROM $JobTable WHERE JobLogId > $watermark" -Tolerant
    if ($null -eq $rowCount) { Start-Sleep -Seconds $DrainPollSeconds; continue }
    $n = [int]$rowCount.n
    Write-Host ("   {0}  queue={1,-6} consumers={2,-3} dlq={3,-6} newRows={4}" -f `
        (Get-Date).ToUniversalTime().ToString('HH:mm:ss'), $q.Messages, $q.Consumers, $d.Messages, $n)

    if ($q.Messages -gt 0) { $emptySince = $null } elseif (-not $emptySince) { $emptySince = Get-Date }
    if ($n -ne $lastRows) { $rowsQuietSince = $null } elseif (-not $rowsQuietSince) { $rowsQuietSince = Get-Date }
    $lastRows = $n

    $emptyFor = if ($emptySince)     { ((Get-Date) - $emptySince).TotalSeconds }     else { 0 }
    $quietFor = if ($rowsQuietSince) { ((Get-Date) - $rowsQuietSince).TotalSeconds } else { 0 }

    if ($q.Messages -eq 0 -and $n -ge $Count) {
        Kv 'verdict' 'all rows present'
        break
    }
    if ($q.Messages -eq 0 -and $n -gt 0 -and $emptyFor -ge $DrainQuietSeconds -and $quietFor -ge $DrainQuietSeconds) {
        Kv 'verdict' "drained; empty ${DrainQuietSeconds}s and rows quiet ${DrainQuietSeconds}s ($n of $Count)"
        break
    }
    # Nothing recorded at all. Wait considerably longer before calling it, because the alternative
    # reading - a cold replica that has not finished its first message - is far more likely than
    # every single message vanishing, and that misread is what produced the false verdict above.
    if ($q.Messages -eq 0 -and $n -eq 0 -and $emptyFor -ge ($DrainQuietSeconds * 3)) {
        Kv 'verdict' "drained but NO rows after $([int]$emptyFor)s - see reconciliation"
        break
    }
    Start-Sleep -Seconds $DrainPollSeconds
}
$drainEnd = (Get-Date).ToUniversalTime()
$elapsed  = ((Get-Date) - $publishedUtc).TotalSeconds

Head 'Step 5 - reconciliation'
$man = @(Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json)
$db = Rows "SELECT JSON_VALUE(MessageContent,'`$.MandateId') AS MandateId, Status, HttpStatusCode, DurationMs, AttemptNumber FROM $JobTable WHERE JobLogId > $watermark AND ISJSON(MessageContent) = 1"
$dbIds = @($db | ForEach-Object { $_.MandateId })

$missing    = @($man | Where-Object { $dbIds -notcontains $_.mandateId })
$unexpected = @($dbIds | Where-Object { $_ -notin $man.mandateId })
$dupes      = $dbIds.Count - @($dbIds | Select-Object -Unique).Count
$durations  = @($db | ForEach-Object { [int]$_.DurationMs } | Sort-Object)

Kv 'published'            $man.Count
Kv 'rows appeared'        $dbIds.Count
Kv 'distinct MandateIds'  (@($dbIds | Select-Object -Unique).Count)
Kv 'MISSING (no row)'     $missing.Count
Kv 'unexpected rows'      $unexpected.Count
Kv 'duplicate executions' $dupes
Kv 'statuses'             ((@($db | ForEach-Object { $_.Status }) | Select-Object -Unique) -join ',')
Kv 'http codes'           ((@($db | ForEach-Object { $_.HttpStatusCode }) | Select-Object -Unique) -join ',')
Kv 'redeliveries (>1)'    (@($db | Where-Object { [int]$_.AttemptNumber -gt 1 }).Count)
if ($durations.Count) {
    Kv 'DurationMs min/med/max' ("{0} / {1} / {2}" -f $durations[0],
        $durations[[int][math]::Floor($durations.Count / 2)], $durations[-1])
}
Kv 'wall clock (s)' ([math]::Round($elapsed, 1))
if ($elapsed -gt 0) { Kv 'throughput (msg/s)' ([math]::Round($dbIds.Count / $elapsed, 2)) }

$dEnd = Get-E2EQueueDepthHttp -ManagementUri $RabbitMqManagementUri -QueueName $DeadLetterQueue `
          -VHost $brokerCtx.VHost -Credential $brokerCtx.Credential
Kv 'dlq ready (end)' "$($dEnd.Messages)  (baseline $($d0.Messages), delta $($dEnd.Messages - $d0.Messages))"

if ($PeekDeadLetters -and $dEnd.Messages -gt 0) {
    Write-Host '   Reading the DLQ - see the header note; this may not be non-destructive on this broker.' -ForegroundColor Yellow
    $s = New-E2EBrokerSession -AmqpUri $amqp -ClientName "wwqp-dlq-$label"
    try {
        $ch = New-E2EChannel -Connection $s.Connection -CancellationToken $s.Ct
        $seen = 0
        while ($seen -lt ($Count * 2 + 50)) {
            $got = $ch.BasicGetAsync($DeadLetterQueue, $false, $s.Ct).GetAwaiter().GetResult()
            if (-not $got) { break }
            $seen++
            Write-Host "     dlq txn: $(Get-WwDeadLetterTxnId -Properties $got.BasicProperties)"
        }
    } finally { Close-E2EBrokerSession -Session $s }
}

# The watermark is the ONE fact a later analysis cannot recover: dbo.jobs1 gets truncated between
# runs here, so "rows belonging to this run" is only answerable against the high-water mark taken
# before publishing. Persist it - with the window - so Measure-WwProfilerRun.ps1 needs nothing but
# the run directory.
$summary = [ordered]@{
    label          = $label
    queue          = $QueueName
    deadLetterQueue = $DeadLetterQueue
    jobTable       = $JobTable
    watermark      = $watermark
    published      = $man.Count
    requested      = $Count
    publishedUtc   = $publishedUtc.ToUniversalTime().ToString('o')
    finishedUtc    = (Get-Date).ToUniversalTime().ToString('o')
    wallClockSec   = [math]::Round($elapsed, 1)

    # Phase boundaries, so the analysis can separate "how fast did it process" from "how long did
    # the whole exercise take". publishStartUtc is the instant the first message hit the broker and
    # is the only correct origin for a publish-to-completion latency.
    runStartUtc    = $runStartUtc.ToString('o')
    preWarmStartUtc = $(if ($preWarmStart) { $preWarmStart.ToString('o') } else { $null })
    preWarmEndUtc  = $(if ($preWarmEnd)   { $preWarmEnd.ToString('o') }   else { $null })
    preWarmSec     = $(if ($preWarmStart -and $preWarmEnd) { [math]::Round(($preWarmEnd - $preWarmStart).TotalSeconds, 1) } else { 0 })
    publishStartUtc = $publishStart.ToString('o')
    publishEndUtc  = $publishEnd.ToString('o')
    publishSec     = [math]::Round(($publishEnd - $publishStart).TotalSeconds, 1)
    drainStartUtc  = $drainStart.ToString('o')
    drainEndUtc    = $drainEnd.ToString('o')
    drainSec       = [math]::Round(($drainEnd - $drainStart).TotalSeconds, 1)
    endToEndSec    = [math]::Round(((Get-Date).ToUniversalTime() - $runStartUtc).TotalSeconds, 1)
    rowsAppeared   = $dbIds.Count
    missing        = $missing.Count
    unexpected     = $unexpected.Count
    duplicates     = $dupes
    dlqBaseline    = $d0.Messages
    dlqEnd         = $dEnd.Messages
    manifestPath   = $manifest
    # Distinguish "warmed" from "meant to warm": a failed pre-warm changes how the latency
    # percentiles should be read, and that must survive into the analysis.
    preWarmed      = (-not $SkipPreWarm -and -not $preWarmFailed)
    preWarmFailed  = [bool]$preWarmFailed
    # HOW it warmed, not just whether. A discovery-route warm and a workflow-route warm produce
    # completely different first-batch percentiles (only the latter populates the per-workflow
    # pool), and the concurrency decides how DEEP that pool got - so two runs are not comparable
    # without these. Field count rather than the values: the values are the -Url, already recorded.
    preWarmRoute       = $PreWarmRoute
    preWarmInputCount  = $(if ($null -ne $warmInputs) { $warmInputs.Count } else { 0 })
    preWarmConcurrency = $PreWarmConcurrency
}
$summary | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $runDir 'run-summary.json') -Encoding utf8
Kv 'summary' (Join-Path $runDir 'run-summary.json')

$verdict = if ($missing.Count -eq 0 -and $unexpected.Count -eq 0 -and $dupes -eq 0 -and $dbIds.Count -eq $man.Count) { 'CLEAN' } else { 'REVIEW' }
Write-Host ''
Write-Host "VERDICT: $verdict" -ForegroundColor $(if ($verdict -eq 'CLEAN') { 'Green' } else { 'Yellow' })
if ($missing.Count) {
    $mf = Join-Path $runDir 'missing-mandateids.txt'
    $missing.mandateId | Set-Content -LiteralPath $mf -Encoding utf8
    Write-Host "   $($missing.Count) published message(s) left NO row and were NOT dead-lettered - silent loss. Ids: $mf" -ForegroundColor Yellow
}
