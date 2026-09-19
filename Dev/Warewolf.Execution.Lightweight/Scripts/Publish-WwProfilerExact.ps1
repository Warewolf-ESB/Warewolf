#Requires -Version 7.0
<#
.SYNOPSIS
    Publishes N messages whose bodies are the exact input set of a known-good engine URL, with only
    MandateId varied so each message is a distinguishable record.

.DESCRIPTION
    A CONTROL BURST. The body is built by parsing the supplied URL's own query string, so the 15
    fixed values cannot drift from the request they reproduce - no retyping, no transcription error.

    WHY MandateId IS THE FIELD THAT VARIES. It is the database reconciliation key: the engine's
    logged document carries it and Invoke-WwQueueLoadTest.ps1 reads it back with
    JSON_VALUE(MessageContent, '$.MandateId'). Varying any other field would leave every row with
    the same key, so N rows could not be told apart and a duplicate execution would be invisible.
    Everything else - MandateCanTrack=false and ResultType=Success in particular, the combination
    observed to succeed - is held constant so the run stays on the proven path.

    'wid' is dropped: it is the Studio's workspace id and a TRANSPORT concern, not a workflow input
    (WorkflowFunctionHelper.cs:35-36 holds it in a reserved set and strips it before binding).

    Publish semantics mirror Publish-WwQueueBurst.ps1 (same module, exchange = queue name with an
    empty routing key, persistent BasicProperties with the txn as CorrelationId) and the manifest
    uses the same schema, so the load-test reconciliation can read it.

    THE EXCHANGE IS NOT OPTIONAL, AND ITS ABSENCE USED TO BE SILENT. Publishing to exchange =
    <queue name> is fidelity to PublishRabbitMQActivity (Dev2.Activities/Activities/RabbitMQ/Publish/
    PublishRabbitMQActivity.cs:312), which is the producer this burst stands in for - but it means a
    queue declared WITHOUT its same-named direct exchange and '' binding cannot be reached at all.
    RabbitMQ answers 404 on the channel, and with mandatory=false and no publisher confirms nothing
    surfaces it: BasicPublishAsync returns, $published increments, and a manifest is written for
    messages the broker never received. Measured 2026-09-19 against a queue a pipeline had declared
    with PUT /api/queues/... - "published 20000", broker publish counter unmoved at 2.

    So this script now CREATES the topology it needs (Confirm-E2EQueueTopology; -SkipTopologyEnsure
    opts out) and, when -RabbitMqManagementUri is supplied, PROVES the publish against the broker's
    own message_stats.publish counter rather than its own loop count.

    SAFE TO CALL DIRECTLY. RabbitMQ.Client 7.0.0 on .NET 10 reliably stack-overflows tearing down
    the AMQP connection - an uncatchable StackOverflowException that kills the hosting process
    outright, even after every message published cleanly. A plain invocation of this script is
    therefore really a LAUNCHER: it re-invokes itself as a child `pwsh` process (-AsChildProcess)
    that does the actual publish and may legitimately crash during its own teardown, and reads the
    child's result back from a file written BEFORE that teardown is attempted - never from the
    child's exit code. -AsChildProcess and -ResultPath are the internal plumbing for that
    re-invocation and are not meant to be passed by hand.

.PARAMETER MandateIdMode
    Unique - a fresh GUID per message (default). Records are distinguishable.
    Fixed  - the URL's own MandateId on every message. Reproduces one request verbatim, but N
             messages then share one key and cannot be reconciled apart. Only useful for N=1.
#>
[CmdletBinding()]
param(
    # NOT [Parameter(Mandatory)] - a mandatory parameter would PROMPT when the script is dot-sourced
    # with -LoadFunctionsOnly, which hangs an unattended test run. They are enforced below instead,
    # after the helpers-only exit.
    [string] $Url,
    [string] $QueueName,
    [string] $Label,
    [string] $ManifestPath,
    [string] $ScriptDir,
    [ValidateRange(1, 100000)][int] $Count = 1,
    [ValidateSet('Unique', 'Fixed')][string] $MandateIdMode = 'Unique',
    [string] $SourceBitePath,
    [string] $PublishPath,

    # Topology + proof. Both default to the SAFE behaviour (ensure, and prove when the management
    # URI is known) because the failure they close is silent by nature - see the header.
    [switch] $SkipTopologyEnsure,
    # Optional: the RabbitMQ MANAGEMENT base URI (e.g. https://rabbitmq.warewolf.online). Supplying
    # it turns on the publish proof. Read in the LAUNCHER, over HTTP only, so it never drags
    # RabbitMQ.Client into the parent process.
    [string] $RabbitMqManagementUri,

    # Internal re-invocation plumbing - see SAFE TO CALL DIRECTLY above. Never pass these by hand.
    [switch] $AsChildProcess,
    [string] $ResultPath,

    # Dot-source the pure helpers for unit testing without touching a broker or spawning a process.
    [switch] $LoadFunctionsOnly
)

$ErrorActionPreference = 'Stop'

function Get-WwProfilerExactChildArgs {
    <#
        Builds the pwsh argument list for the isolated child worker from whatever the outer
        invocation actually received. -AsChildProcess, -ResultPath and -LoadFunctionsOnly are
        launcher-only concerns and must never be forwarded as bound values: the launcher always
        mints its own -ResultPath and sets -AsChildProcess itself, and -LoadFunctionsOnly never
        reaches this point (the launcher branch only runs after that early return).
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $ScriptPath,
        [Parameter(Mandatory)][hashtable] $BoundParameters,
        [Parameter(Mandatory)][string] $ResultPath
    )
    $exclude = @('AsChildProcess', 'ResultPath', 'LoadFunctionsOnly')
    $childArgs = [System.Collections.Generic.List[string]]::new()
    $childArgs.AddRange([string[]]@('-NoProfile', '-File', $ScriptPath))
    foreach ($key in $BoundParameters.Keys) {
        if ($key -in $exclude) { continue }
        $value = $BoundParameters[$key]
        if ($value -is [switch]) {
            if ($value.IsPresent) { $childArgs.Add("-$key") }
        } else {
            $childArgs.Add("-$key")
            $childArgs.Add([string]$value)
        }
    }
    $childArgs.AddRange([string[]]@('-AsChildProcess', '-ResultPath', $ResultPath))
    return , $childArgs.ToArray()
}

function Get-WwProfilerExactChildResult {
    <#
        Reads the worker's result file. THE CHILD'S EXIT CODE IS NEVER TRUSTED: the stack-overflow
        described above crashes the child AFTER a fully successful publish, so a non-zero or
        crashed exit is expected and meaningless on its own. The result file is written before that
        teardown is attempted, so its presence is what "the child actually finished publishing"
        means; its absence means the child died before that point, which - unlike the teardown
        crash - is a real failure worth surfacing.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $ResultPath)
    if (-not (Test-Path -LiteralPath $ResultPath)) {
        throw ("The publish worker process exited without writing a result to '$ResultPath' - it " +
               'crashed or was killed before finishing the publish. Check -ManifestPath for what was ' +
               'actually delivered before this happened.')
    }
    return Get-Content -LiteralPath $ResultPath -Raw | ConvertFrom-Json
}

function Format-WwProfilerExactBody {
    <#
        Wraps the fixture's fields in the engine's documented {"inputs":{...}} envelope
        (ServiceBusWorkflowMessage / ServiceBusSecureTrigger-Architecture.md's "Message contract").

        A FLAT body is the trap this guards, and it fails differently depending on the engine build:
          * Pre-WOLF-8517 builds (what wwuat3 ran on 2026-09-14) require a 'workflow' field and
            dead-letter a flat body as Malformed - "Message is missing the required 'workflow'
            field" - before it ever executes. That is what dead-lettered 4998/5000 that day.
          * Current builds accept it, but (before the ResolveRawInputPayload fix) bound NOTHING
            from it: valid JSON, Inputs=null, so the workflow executed against an EMPTY DATALIST
            and failed on its own missing variables - "Scalar value { MandateDRI } is NULL ...
            Invalid Calculate", which reads like a broken workflow rather than an unbound payload.
        The envelope is correct for both. The identical payload over HTTP has always worked,
        because the HTTP routes bind a flat body verbatim.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)] $Fields)
    [ordered]@{ inputs = $Fields } | ConvertTo-Json -Compress -Depth 5
}

if ($LoadFunctionsOnly) { return }

foreach ($required in @(
    @{ N = 'Url'; V = $Url }, @{ N = 'QueueName'; V = $QueueName }, @{ N = 'Label'; V = $Label },
    @{ N = 'ManifestPath'; V = $ManifestPath }, @{ N = 'ScriptDir'; V = $ScriptDir }
)) {
    if ([string]::IsNullOrWhiteSpace($required.V)) { throw "-$($required.N) is required." }
}

# ═════════════════════════════════════════════════════════════════════════════
# Launcher - runs in the CALLER's process. Never loads RabbitMQ.Client, so it cannot be the one
# that stack-overflows; see SAFE TO CALL DIRECTLY in the help above.
# ═════════════════════════════════════════════════════════════════════════════
if (-not $AsChildProcess) {
    $tmpResult = Join-Path ([System.IO.Path]::GetTempPath()) "wwprofilerexact-$([guid]::NewGuid().ToString('N')).json"

    # THE PROOF IS TAKEN IN THE LAUNCHER, NOT THE CHILD, for two reasons. The child writes its
    # result file BEFORE tearing the connection down (it is expected to crash there), so a check
    # placed after the publish loop would be skipped on exactly the runs that crash; and the
    # counter must be read AFTER the client's buffered publishes have flushed, which only the
    # connection close guarantees. The launcher reads plain HTTP, so it stays RabbitMQ.Client-free.
    $proofCtx = $null
    if ($RabbitMqManagementUri) {
        Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force
        $amqpForProof = Resolve-E2EBrokerUri -SourceBitePath $SourceBitePath
        if (-not $amqpForProof) { throw "Could not resolve the broker URI from '$SourceBitePath' for the publish proof." }
        $brokerCtx = Get-E2EBrokerContextFromUri -AmqpUri $amqpForProof
        $proofCtx = [pscustomobject]@{
            VHost      = $brokerCtx.VHost
            Credential = $brokerCtx.Credential
            Before     = Get-E2EQueuePublishCount -ManagementUri $RabbitMqManagementUri -QueueName $QueueName `
                            -VHost $brokerCtx.VHost -Credential $brokerCtx.Credential
        }
    }

    try {
        $childArgs = Get-WwProfilerExactChildArgs -ScriptPath $PSCommandPath -BoundParameters $PSBoundParameters -ResultPath $tmpResult

        # Streams are CAPTURED, not passed through: a successful run legitimately ends in the known
        # RabbitMQ.Client/.NET 10 stack-overflow crash described above, which would otherwise dump a
        # ~200-frame native stack trace to the caller's console for a run that fully succeeded.
        # Captured output is only surfaced below if the result file - the real signal - is missing.
        $childOutput = & pwsh @childArgs 2>&1 | Out-String
        $result = Get-WwProfilerExactChildResult -ResultPath $tmpResult

        if ($proofCtx) {
            # POLLED, not read once: message_stats comes from the management stats database, which
            # refreshes on an interval rather than synchronously with the publish - see
            # Wait-E2EPublishDelta for the measurement that proves it.
            $delta = Wait-E2EPublishDelta -ManagementUri $RabbitMqManagementUri -QueueName $QueueName `
                        -VHost $proofCtx.VHost -Credential $proofCtx.Credential `
                        -Before $proofCtx.Before -Expected ([int]$result.Total)
            $result | Add-Member -NotePropertyName 'PublishProofDelta' -NotePropertyValue $delta -Force
        }

        return $result
    }
    catch {
        throw "$($_.Exception.Message)`n`nChild process output:`n$childOutput"
    }
    finally {
        if (Test-Path -LiteralPath $tmpResult) { Remove-Item -LiteralPath $tmpResult -Force -ErrorAction SilentlyContinue }
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Worker - only ever reached via -AsChildProcess, i.e. inside the isolated child process spawned
# above. May legitimately crash the process it runs in during teardown; see the note in the
# `finally` block below for why the result and manifest are both written ahead of that.
# ═════════════════════════════════════════════════════════════════════════════
if (-not $ResultPath) { throw '-ResultPath is required when -AsChildProcess is set (internal use only).' }

Import-Module (Join-Path $ScriptDir 'WwE2E.Common.psm1') -Force

# ── The fixed field set, taken from the URL itself ────────────────────────────
$reserved = @('wid', 'workflowName', 'workflowFilePath', 'isDebug')
$template = [ordered]@{}
foreach ($kv in ([uri]$Url).Query.TrimStart('?') -split '&') {
    if (-not $kv) { continue }
    $pair = $kv -split '=', 2
    $name = [uri]::UnescapeDataString($pair[0])
    if ($name -in $reserved) { continue }
    $template[$name] = if ($pair.Count -eq 2) { [uri]::UnescapeDataString($pair[1]) } else { '' }
}
if (-not $template.Contains('MandateId')) { throw 'The URL carries no MandateId; it is the database reconciliation key.' }

$slug = ($QueueName -replace '[^a-zA-Z]', '').ToLowerInvariant()
if (-not $slug) { $slug = 'queue' }
$slug = $slug.Substring(0, [math]::Min(6, $slug.Length))

# ── Manifest built and validated BEFORE the connection is opened ─────────────
$manifest = [System.Collections.Generic.List[object]]::new()
for ($i = 1; $i -le $Count; $i++) {
    $fields = [ordered]@{}
    foreach ($k in $template.Keys) { $fields[$k] = $template[$k] }
    if ($MandateIdMode -eq 'Unique') {
        $fields['MandateId'] = [guid]::NewGuid().ToString().ToUpperInvariant()
    }
    $body = Format-WwProfilerExactBody -Fields $fields
    $txn  = '{0}-{1}-S-{2:d4}-{3}' -f $Label, $slug, $i, ([guid]::NewGuid().ToString('N').Substring(0, 6))
    $manifest.Add([pscustomobject]@{
        txn = $txn; queue = $QueueName; kind = 'success'
        bytes = [Text.Encoding]::UTF8.GetByteCount($body); body = $body
        mandateId = [string]$fields['MandateId']
    })
}
# Same guards as New-WwBurstManifest: a collision in either key makes reconciliation meaningless.
$dTxn = @($manifest.txn | Select-Object -Unique).Count
if ($dTxn -ne $manifest.Count) { throw "Duplicate txn ids ($dTxn unique of $($manifest.Count))." }
if ($MandateIdMode -eq 'Unique') {
    $dMid = @($manifest.mandateId | Select-Object -Unique).Count
    if ($dMid -ne $manifest.Count) { throw "Duplicate MandateIds ($dMid unique of $($manifest.Count))." }
}

$amqp = Resolve-E2EBrokerUri -SourceBitePath $SourceBitePath
if (-not $amqp) { throw "Could not resolve the broker URI from '$SourceBitePath'." }

Initialize-E2ERabbitClient -PublishPath $PublishPath
$session      = New-E2EBrokerSession -AmqpUri $amqp -ClientName "wwqp-exact-$Label"

# BEFORE THE FIRST PUBLISH, not after a failure: the exchange/binding this script publishes through
# must exist, and their absence is silent (see the header). Existing objects are never redeclared,
# so this cannot disturb a rig that is already correct.
if (-not $SkipTopologyEnsure) {
    $topology = Confirm-E2EQueueTopology -Session $session -QueueName $QueueName -Durable $true
    Write-Verbose "topology: $($topology.Summary)"
}

$publishedUtc = (Get-Date).ToUniversalTime()
$published    = 0
try {
    $ch = New-E2EChannel -Connection $session.Connection -CancellationToken $session.Ct
    foreach ($m in $manifest) {
        $props = [RabbitMQ.Client.BasicProperties]::new()
        $props.CorrelationId = $m.txn
        $props.Persistent    = $true
        $bytes = [ReadOnlyMemory[byte]][Text.Encoding]::UTF8.GetBytes($m.body)
        $ch.BasicPublishAsync($QueueName, '', $false, $props, $bytes, $session.Ct).GetAwaiter().GetResult() | Out-Null
        $published++
    }
}
finally {
    # Written BEFORE the teardown, and still only for what actually went out ($published), so a
    # mid-burst drop never records undelivered messages. It must precede the close calls because
    # RabbitMQ.Client 7.0.0 on .NET 10 reliably stack-overflows in its socket reader during
    # connection shutdown (StreamPipeReader/SocketAsyncEventArgs, on a threadpool thread), killing
    # the process at a nondeterministic point. Writing after the close lost the manifest entirely
    # on most runs even though every message had been delivered.
    if ($published -gt 0) {
        $existing = @()
        if (Test-Path -LiteralPath $ManifestPath) { $existing = @(Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json) }
        $dir = Split-Path -Parent $ManifestPath
        if ($dir -and -not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
        (@($existing) + @($manifest | Select-Object -First $published)) | ConvertTo-Json -Depth 5 |
            Set-Content -LiteralPath $ManifestPath -Encoding utf8
    }
    # The launcher's Get-WwProfilerExactChildResult reads THIS file, never the exit code, for
    # exactly the reason in the comment above: the close calls below are expected to crash the
    # process on a large run. Written here, ahead of them, for the same reason the manifest is.
    [pscustomobject]@{
        Queue = $QueueName; Label = $Label; Total = $published; MandateIdMode = $MandateIdMode
        Fields = $template.Count; Bytes = $manifest[0].bytes
        Broker = $session.Describe; PublishedUtc = $publishedUtc.ToString('o'); ManifestPath = $ManifestPath
    } | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath $ResultPath -Encoding utf8

    # Close the CHANNEL before the connection. RabbitMQ.Client 7.x buffers BasicPublishAsync, and
    # tearing the connection down with a channel still open loses the buffered messages AND crashes
    # the host (exit 139, no catchable exception) - which used to happen right here, before the
    # manifest below was ever written.
    if ($ch) {
        try {
            $ch.CloseAsync([uint16]200, 'Goodbye', $false,
                           [Threading.CancellationToken]::None).GetAwaiter().GetResult() | Out-Null
        } catch {
            Write-Verbose "channel close failed: $($_.Exception.Message)"
        }
        $ch.Dispose()
    }
    Close-E2EBrokerSession -Session $session
}
if ($published -ne $manifest.Count) { throw "Published only $published of $($manifest.Count)." }

# Reached only when the teardown above did NOT crash (e.g. a small Count). Re-read rather than
# rebuild, so there is exactly one place that defines the result's shape.
return Get-Content -LiteralPath $ResultPath -Raw | ConvertFrom-Json
