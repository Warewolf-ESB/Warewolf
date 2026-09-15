#Requires -Version 7.0
<#
.SYNOPSIS
    Publishes a burst of messages to a Warewolf queue over AMQP and writes a manifest of exactly
    what was sent.

.DESCRIPTION
    The manifest is what turns the run report from a COUNT into a RECONCILIATION. Without it a
    report can only describe the messages it happens to find in the logs, so a message that was
    published and never delivered at all is invisible - which is precisely the failure mode worth
    catching.

    THE CORRELATION ID IS THE ONLY RELIABLE PER-MESSAGE KEY
    Every message carries a unique BasicProperties.CorrelationId. RabbitMqMessagePump copies it
    into the 'Warewolf-Custom-Transaction-Id' header, and QueueProcessorCorrelation renders it as
    [Txn:...] on EVERY worker log line for that delivery - including the dead-letter lines, whose
    own [ExecutionId:...] bracket is the publisher's constant rather than the per-message GUID.
    It is also the only handle on a deliberate FAILURE message, whose body is empty by design and
    therefore has no content to match on.

    WHERE MESSAGES ARE PUBLISHED
    To the exchange NAMED AFTER THE QUEUE with an EMPTY routing key, matching what
    PublishRabbitMQActivity does. The exchange, queue and binding must already exist - this script
    does not create them (Initialize-E2EQueueTopology does). An exchange and queue WITHOUT the
    binding silently discard every message, so a missing binding looks exactly like a broker that
    accepted the publish and lost it.

.PARAMETER SuccessCount
    Messages with a valid JSON body. Each gets distinct content, so duplicate execution is
    detectable in the database as well as in the logs.

.PARAMETER FailureCount
    Messages with an EMPTY body. MapEntireMessage + EmptyIsNull makes [[message]] null, and
    usp_jobs1_LogStart / usp_jobs2_LogStart both RAISERROR on a null @MessageContent, so the engine
    returns 500 deterministically. Use these to exercise the dead-letter path on purpose.

.PARAMETER ManifestPath
    JSON array of { txn, queue, kind, bytes, body }. APPENDED to if the file already exists, so
    several bursts can share one manifest.

.EXAMPLE
    # 100 successes, no deliberate failures - the RUN 2 shape
    .\Publish-WwQueueBurst.ps1 -QueueName order-success-queue -SuccessCount 100 -Label R2 `
        -ManifestPath G:\Deployment\logs\myrun\manifest.json

.EXAMPLE
    # A mixed burst: 30 valid, 5 that must dead-letter
    .\Publish-WwQueueBurst.ps1 -QueueName order-success-queue -SuccessCount 30 -FailureCount 5 `
        -Label B1 -ManifestPath .\manifest.json
#>
[CmdletBinding()]
param(
    # NOT [Parameter(Mandatory)] - a mandatory parameter would PROMPT when the script is dot-sourced
    # with -LoadFunctionsOnly, which hangs an unattended test run. They are enforced below instead,
    # after the helpers-only exit.
    [string] $QueueName,
    [ValidateRange(0, 100000)][int] $SuccessCount = 30,
    [ValidateRange(0, 100000)][int] $FailureCount = 0,
    [string] $Label,

    [string] $AmqpUri,
    [string] $SourceBitePath,
    [string] $PublishPath = 'G:\Deployment\apps\QueueProcessor',

    [string] $ManifestPath,

    # WHICH PAYLOAD SHAPE TO PUBLISH. Each queue's trigger declares its own input contract, so one
    # body shape cannot serve every queue:
    #   Mandate  - the mandate/collection document (order-success-queue and similar).
    #   Profiler - the FLAT 16-field body that trigger 9923dc51 (profiler.profile.request) binds,
    #              one property per declared workflow input.
    # Default stays Mandate so existing order-success runs are unaffected.
    [ValidateSet('Mandate', 'Profiler')]
    [string] $BodyShape = 'Mandate',

    # Dot-source the pure helpers for unit testing without touching a broker.
    [switch] $LoadFunctionsOnly
)

$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# ═════════════════════════════════════════════════════════════════════════════
# Pure helpers - no broker, no filesystem
# ═════════════════════════════════════════════════════════════════════════════

function Get-WwBurstQueueSlug {
    <#
        A short, stable, letters-only fragment of the queue name for the txn prefix.

        Guarded against an empty result: 'q-123' has no letters at all, and Substring(0, 6) on the
        resulting empty string throws. A queue named entirely in digits is unusual but a crash at
        message 1 of a 100-message run is a poor way to discover it.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $QueueName, [int] $Length = 6)

    $letters = ($QueueName -replace '[^a-zA-Z]', '').ToLowerInvariant()
    if ([string]::IsNullOrEmpty($letters)) { return 'queue' }
    return $letters.Substring(0, [math]::Min($Length, $letters.Length))
}

function New-WwBurstTxn {
    <#
        The per-message transaction id: <label>-<slug>-<S|F>-<index>-<random>.

        The random tail matters. Without it a second run with the same label produces identical
        txns, and the report would reconcile this run's messages against the previous run's log
        lines - reporting a clean pass built entirely from stale evidence.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Label,
        [Parameter(Mandatory)][string] $Slug,
        [Parameter(Mandatory)][ValidateSet('success', 'failure')][string] $Kind,
        [Parameter(Mandatory)][int] $Index
    )
    $k = if ($Kind -eq 'success') { 'S' } else { 'F' }
    '{0}-{1}-{2}-{3:d4}-{4}' -f $Label, $Slug, $k, $Index, ([guid]::NewGuid().ToString('N').Substring(0, 6))
}

function New-WwBurstBody {
    <#
        The production mandate/collection payload. Distinct JSON content per message, so duplicate
        EXECUTION is visible in the database rows and not only in the worker logs - here that comes
        from MandateId (a fresh GUID per message) plus InsertedOn.

        MandateId IS THE RECONCILIATION KEY. Invoke-WwQueueLoadTest.ps1 reads it back out of the
        stored message with JSON_VALUE(MessageContent, '$.MandateId'), so it must be unique per
        message and must never be removed from this body. The AMQP CorrelationId carries the
        separate manifest 'txn' instead (see the publish loop), which is what dead-letter
        attribution and the worker's [Txn:...] log prefix use. Two identifiers, two purposes:
        MandateId reconciles against the DATABASE, txn reconciles against the DLQ and the logs.

        -Depth 5 is REQUIRED, not decorative: ConvertTo-Json defaults to depth 2 and would render
        the nested ClearWinnerStrategy objects as type names instead of JSON.

        [ordered] preserves the field order of the source samples. Nothing depends on the order,
        but a diff against a captured production message is unreadable without it.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $MandateId,
        [ValidateSet('Mandate', 'Profiler')][string] $Shape = 'Mandate'
    )

    # ── Profiler shape ────────────────────────────────────────────────────────────────
    # A FLAT object with one property per input declared by trigger 9923dc51
    # (profiler.profile.request -> ProfilerServices\ProfilerWrapper\RequestForProfiling).
    #
    # It must be flat and the names must match, because MessageToInputsMapper binds by looking up
    # each declared input's Value against a TOP-LEVEL property (MessageToInputsMapper.cs:49):
    #     jObject.Properties().FirstOrDefault(p => p.Name == input.messageValue)
    # A nested document cannot bind, and the trigger must have MapEntireMessage = false - with true
    # the mapper puts the raw message into inputs[0] ONLY (MessageToInputsMapper.cs:31-33) and the
    # other 15 inputs stay unbound, which dead-lettered all 10 messages of 'uat2-nowarm-profiler-1'.
    #
    # THE 19-FIELD MANDATE DOCUMENT IS THIS WORKFLOW'S OUTPUT, NOT ITS INPUT. Publishing it here
    # bound only 4 of the 16 inputs. RequestForProfiling.bite BUILDS the mandate fields from these
    # inputs - DebitRunID <- [[MandateDRI]], ExtractScheduleId <- [[ResultPaymentCycle]][[DRI]],
    # SentOn <- [[ResultPresentedOnDate]] - and every mandate-only field is ColumnIODirection="None"
    # in its DataList, an internal variable that cannot be passed in. The canonical wire document is
    # proved by ProfilerWrapper\Queue\RequestForProfilingConsume.bite, which reads
    # [[@RequestData.<Name>]] once per input. Use the Mandate shape only on a queue whose trigger
    # actually declares those names.
    #
    # EVERY VALUE IS A JSON STRING, the numeric-looking ones included, because that is what the
    # supplied production sample does ("ForPaymentCycle": "202402", "SourcePaymentId": "1234").
    # Warewolf scalars are strings regardless, so quoting costs nothing and keeps a published body
    # byte-comparable against a captured one.
    #
    # THE DATES, CYCLES AND RUN IDS COME FROM ONE ANCHOR rather than being rolled independently.
    # They agree in the sample and must keep agreeing: ResultPresentedOnDate 2024-01-22 has day 22,
    # which is both ResultDebitRunId and MandateDRI; its year-month 202401 is ResultPaymentCycle;
    # ForPaymentCycle 202402 is the next cycle; ResultDate 2024-01-27 follows presentation. Rolling
    # them separately produces contradictions the workflow then acts on - ExtractScheduleId is
    # literally ResultPaymentCycle concatenated with the DRI, so a mismatched pair yields a
    # schedule id that never existed, which is worse than an obviously wrong value.
    if ($Shape -eq 'Profiler') {
        $inv = [System.Globalization.CultureInfo]::InvariantCulture

        # Day 1-28 only. Days 29-31 are absent from some months, and the presented date's day IS
        # the debit run id, so an impossible date would silently become an impossible DRI.
        $dri       = Get-Random -Minimum 1 -Maximum 29
        $presented = (Get-Date '2024-01-01').AddMonths((Get-Random -Minimum 0 -Maximum 24)).AddDays($dri - 1)
        $resultOn  = $presented.AddDays((Get-Random -Minimum 0 -Maximum 8))

        return [ordered]@{
            # Declared in the trigger's own input order, which is also the sample's order.
            ForPaymentCycle       = $presented.AddMonths(1).ToString('yyyyMM', $inv)
            MandateCanTrack       = @('true', 'false')     | Get-Random   # GetCollectionMethod IsEqual 'false'; lowercased by ProfilerMain
            MandateDRI            = [string]$dri
            MandateId             = $MandateId                            # reconciliation key: JSON_VALUE(MessageContent,'$.MandateId')
            RelatedMandates       = [guid]::NewGuid().ToString().ToUpperInvariant()  # a DIFFERENT mandate's id, per the sample
            ResultDate            = $resultOn.ToString('yyyy-MM-dd', $inv)
            ResultDebitRunId      = [string]$dri
            ResultMethod          = @('DebiCheck', 'DO')    | Get-Random  # TestFramework CSV header: ResultMethod-debicheck/do
            ResultPaymentCycle    = $presented.ToString('yyyyMM', $inv)
            ResultPresentedOnDate = $presented.ToString('yyyy-MM-dd', $inv)
            ResultType            = @('Success', 'Failed')  | Get-Random  # GetCollectionMethod IsEqual 'Success'
            ScoreThisResult       = 'true'                                # PINNED: the scoring branch is skipped when false (GetMandateDRIs IsEqual 'true')
            IsFixedExtract        = @('true', 'false')      | Get-Random  # GetTrackingPeriod IsEqual 'true'
            SourcePaymentId       = (Get-Random -Minimum 1000 -Maximum 10000).ToString($inv)  # 4-digit id, per the sample's "1234"
            # PINNED EMPTY, and deliberately not randomised. The earlier 'LearningPhase'/'DEFAULT'
            # domain was taken from the 19-field mandate document - which is this workflow's OUTPUT,
            # not its input - so it was never evidence about a profiler request. Every actual
            # request sample has both empty (the known-good /secure/...RequestForProfiling.json call)
            # or omits them entirely, and that call is the only shape confirmed to execute cleanly.
            # Empty still BINDS: the property is present, and EmptyIsNull is never enforced at
            # runtime (TriggerDefinition.cs:90-91 declares it; nothing in the worker or engine reads
            # it). Restore a non-empty value only against a request sample that actually carries one.
            RequestType           = ''
            FASGroup              = ''
        } | ConvertTo-Json -Compress
    }

    # Machine-local time, matching the sample values ('09/11/2026 01:58:19.451'), with
    # milliseconds - the higher resolution also keeps InsertedOn near-unique in its own right.
    #
    # InvariantCulture is REQUIRED. In a .NET format string '/' is the culture-specific DATE
    # SEPARATOR PLACEHOLDER, not a literal: on a machine whose culture separates with '-' this
    # renders '09-11-2026 04:06:01.657' and the consumer sees the wrong format. Invariant pins the
    # separator to '/'. (Escaping as 'MM\/dd\/yyyy' also works but is easy to drop in an edit.)
    $insertedOn = (Get-Date).ToString('MM/dd/yyyy HH:mm:ss.fff',
                                      [System.Globalization.CultureInfo]::InvariantCulture)

    # ── RANDOMISED per message, structure fixed ──────────────────────────────────────
    # Every one of the 19 top-level fields and all 7 fields of the single ClearWinnerStrategy
    # element are always present; only the VALUES vary. Randomising matters for two reasons: the
    # harness asserts distinct bodies so that duplicate EXECUTION shows up as duplicate database
    # rows, and identical payloads cannot reveal value-dependent behaviour.
    #
    # The string value sets are taken from the two known-good messages supplied by the operator
    # rather than invented, so every generated value is one that has actually been seen on this
    # queue. Numerics and dates are randomised inside ranges consistent with those samples.
    #
    # MandateId is NOT random here - it is the caller-supplied reconciliation key
    # (JSON_VALUE(MessageContent,'$.MandateId')) and must stay unique per message.
    $rndDate = { param($from, $to) (Get-Date '2024-01-01').AddDays((Get-Random -Minimum $from -Maximum $to)) }
    $presentOn = & $rndDate 30 90
    $sentOn    = & $rndDate 0 30
    $extractOn = & $rndDate 0 90
    $inv       = [System.Globalization.CultureInfo]::InvariantCulture

    [ordered]@{
        MandateId             = $MandateId
        InsertedOn            = $insertedOn
        CollectionMethod      = @('DebitOrder', 'N10')                  | Get-Random
        DebitRunID            = Get-Random -Minimum 1  -Maximum 100
        PresentForCollectionOn = $presentOn.ToString('yyyy-MM-dd', $inv)
        Profile               = @('Weekday', 'Weekend')                 | Get-Random
        TrackingDays          = Get-Random -Minimum 0  -Maximum 31
        IsOverride            = Get-Random -Minimum 0  -Maximum 2
        IsValid               = Get-Random -Minimum 0  -Maximum 2
        SentOn                = $sentOn.ToString('yyyy-MM-dd', $inv)
        ApplicableMonth       = Get-Random -Minimum 1  -Maximum 13
        CollectionMonth       = Get-Random -Minimum 1  -Maximum 13
        ExtractScheduleId     = $extractOn.ToString('yyyyMMdd', $inv)
        ForPaymentCycle       = [int]('{0}{1:d2}' -f (Get-Random -Minimum 2024 -Maximum 2027), (Get-Random -Minimum 1 -Maximum 13))
        FASGroup              = @('', 'DEFAULT')                        | Get-Random
        ServiceType           = @('TWODAY', '')                         | Get-Random
        RequestType           = @('', 'LearningPhase')                  | Get-Random
        RecommendationTypes   = @('LearningPhase', 'InitialsTracking')  | Get-Random
        ClearWinnerStrategy   = @(
            [ordered]@{
                CollectionMethod       = @('DebitOrder', 'N10')         | Get-Random
                HighestFromDRI         = Get-Random -Minimum 1 -Maximum 32
                HighestToDRI           = Get-Random -Minimum 1 -Maximum 32
                PresentForCollectionOn = (& $rndDate 30 90).ToString('yyyy-MM-dd', $inv)
                Score                  = Get-Random -Minimum 0 -Maximum 10
                ServiceType            = @('TWODAY', '')                | Get-Random
                TrackingDays           = Get-Random -Minimum 0 -Maximum 31
            }
        )
    } | ConvertTo-Json -Compress -Depth 5
}

function New-WwBurstManifest {
    <#
        Builds the full manifest in memory BEFORE anything is published, so the txn/body uniqueness
        invariants can be asserted while a mistake still costs nothing. Publishing 100 messages and
        then discovering a collision means re-cleaning the queues and the database.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $QueueName,
        [Parameter(Mandatory)][string] $Label,
        [int] $SuccessCount = 0,
        [int] $FailureCount = 0,
        [ValidateSet('Mandate', 'Profiler')][string] $BodyShape = 'Mandate'
    )
    $slug = Get-WwBurstQueueSlug -QueueName $QueueName
    $list = [System.Collections.Generic.List[object]]::new()

    for ($i = 1; $i -le $SuccessCount; $i++) {
        $txn = New-WwBurstTxn -Label $Label -Slug $slug -Kind 'success' -Index $i
        # Uppercase, unbraced - the form the production samples use.
        $mandateId = [guid]::NewGuid().ToString().ToUpperInvariant()
        $body = New-WwBurstBody -MandateId $mandateId -Shape $BodyShape
        $list.Add([pscustomobject]@{ txn = $txn; mandateId = $mandateId; queue = $QueueName
                                     kind = 'success'; bytes = $body.Length; body = $body })
    }
    for ($i = 1; $i -le $FailureCount; $i++) {
        $txn = New-WwBurstTxn -Label $Label -Slug $slug -Kind 'failure' -Index $i
        # Deliberate failures are published with an EMPTY body, so they carry no MandateId and can
        # never appear in the database. They are tracked through txn on the dead-letter queue.
        $list.Add([pscustomobject]@{ txn = $txn; mandateId = $null; queue = $QueueName
                                     kind = 'failure'; bytes = 0; body = '' })
    }

    $distinct = @($list | ForEach-Object txn | Select-Object -Unique).Count
    if ($distinct -ne $list.Count) {
        throw "Manifest has duplicate transaction ids ($distinct unique of $($list.Count)). Reconciliation would be meaningless."
    }
    # MandateId is the DATABASE reconciliation key, so a collision there is just as fatal as a txn
    # collision - two messages would be indistinguishable in dbo.jobs1.
    $successIds = @($list | Where-Object { $_.kind -eq 'success' } | ForEach-Object mandateId)
    $distinctMandates = @($successIds | Select-Object -Unique).Count
    if ($distinctMandates -ne $successIds.Count) {
        throw "Manifest has duplicate MandateIds ($distinctMandates unique of $($successIds.Count)). Database reconciliation would be meaningless."
    }
    return , $list.ToArray()
}

if ($LoadFunctionsOnly) { return }

# ═════════════════════════════════════════════════════════════════════════════
# Publish
# ═════════════════════════════════════════════════════════════════════════════

Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force

foreach ($required in @(@{ N = 'QueueName'; V = $QueueName }, @{ N = 'Label'; V = $Label }, @{ N = 'ManifestPath'; V = $ManifestPath })) {
    if ([string]::IsNullOrWhiteSpace($required.V)) { throw "-$($required.N) is required." }
}
if ($SuccessCount + $FailureCount -eq 0) { throw 'Nothing to publish: -SuccessCount and -FailureCount are both 0.' }

if (-not $AmqpUri) {
    if (-not $SourceBitePath) { throw 'Supply -AmqpUri, or -SourceBitePath pointing at a RabbitMQ source .bite.' }
    $AmqpUri = Resolve-E2EBrokerUri -SourceBitePath $SourceBitePath
    if (-not $AmqpUri) {
        throw ("Could not resolve the broker URI from '$SourceBitePath'. A WFAES-encrypted source cannot be " +
               'read without the Key Vault key - pass -AmqpUri explicitly.')
    }
}

# Built and validated before the connection is even opened.
$manifest = New-WwBurstManifest -QueueName $QueueName -Label $Label -SuccessCount $SuccessCount -FailureCount $FailureCount -BodyShape $BodyShape

Initialize-E2ERabbitClient -PublishPath $PublishPath
$session = New-E2EBrokerSession -AmqpUri $AmqpUri -ClientName "wwqp-burst-$Label"

$publishedUtc = (Get-Date).ToUniversalTime()
$published    = 0

try {
    $ch = New-E2EChannel -Connection $session.Connection -CancellationToken $session.Ct
    $ct = $session.Ct

    foreach ($m in $manifest) {
        $props = [RabbitMQ.Client.BasicProperties]::new()
        $props.CorrelationId = $m.txn
        $props.Persistent    = $true

        $bytes = if ($m.kind -eq 'failure') {
            [ReadOnlyMemory[byte]]([byte[]]@())
        } else {
            [ReadOnlyMemory[byte]][Text.Encoding]::UTF8.GetBytes($m.body)
        }

        # Exchange = queue name, routing key = empty. See the header note.
        $ch.BasicPublishAsync($QueueName, '', $false, $props, $bytes, $ct).GetAwaiter().GetResult() | Out-Null
        $published++
    }
}
finally {
    Close-E2EBrokerSession -Session $session

    # Written in the finally block ON PURPOSE. If the connection drops at message 60 those 60 are
    # really on the queue, and a manifest that omits them would have the report classify every one
    # as an 'unexpected' delivery. Only what was actually published is recorded.
    if ($published -gt 0) {
        $existing = @()
        if (Test-Path -LiteralPath $ManifestPath) {
            $existing = @(Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json)
        }
        $all = @($existing) + @($manifest | Select-Object -First $published)
        $dir = Split-Path -Parent $ManifestPath
        if ($dir -and -not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
        $all | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $ManifestPath -Encoding utf8
    }
}

if ($published -ne $manifest.Count) {
    throw "Published only $published of $($manifest.Count) message(s). The manifest records what actually went out."
}

[pscustomobject]@{
    Queue         = $QueueName
    Label         = $Label
    Success       = $SuccessCount
    Failure       = $FailureCount
    Total         = $published
    Broker        = $session.Describe
    PublishedUtc  = $publishedUtc.ToString('o')
    ManifestPath  = $ManifestPath
    ManifestTotal = @(Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json).Count
}
