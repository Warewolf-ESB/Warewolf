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
        Distinct JSON content per message, so duplicate EXECUTION is visible in the database rows
        and not only in the worker logs. orderId carries the index and amount varies with it.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Label,
        [Parameter(Mandatory)][string] $QueueName,
        [Parameter(Mandatory)][string] $Txn,
        [Parameter(Mandatory)][int] $Index
    )
    @{
        orderId = ('{0}-S-{1:d4}' -f $Label, $Index)
        amount  = (100 + $Index)
        queue   = $QueueName
        txn     = $Txn
    } | ConvertTo-Json -Compress
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
        [int] $FailureCount = 0
    )
    $slug = Get-WwBurstQueueSlug -QueueName $QueueName
    $list = [System.Collections.Generic.List[object]]::new()

    for ($i = 1; $i -le $SuccessCount; $i++) {
        $txn  = New-WwBurstTxn  -Label $Label -Slug $slug -Kind 'success' -Index $i
        $body = New-WwBurstBody -Label $Label -QueueName $QueueName -Txn $txn -Index $i
        $list.Add([pscustomobject]@{ txn = $txn; queue = $QueueName; kind = 'success'; bytes = $body.Length; body = $body })
    }
    for ($i = 1; $i -le $FailureCount; $i++) {
        $txn = New-WwBurstTxn -Label $Label -Slug $slug -Kind 'failure' -Index $i
        $list.Add([pscustomobject]@{ txn = $txn; queue = $QueueName; kind = 'failure'; bytes = 0; body = '' })
    }

    $distinct = @($list | ForEach-Object txn | Select-Object -Unique).Count
    if ($distinct -ne $list.Count) {
        throw "Manifest has duplicate transaction ids ($distinct unique of $($list.Count)). Reconciliation would be meaningless."
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
$manifest = New-WwBurstManifest -QueueName $QueueName -Label $Label -SuccessCount $SuccessCount -FailureCount $FailureCount

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
