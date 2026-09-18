#Requires -Version 7.0
<#
.SYNOPSIS
    Asserts that a workflow run actually DELIVERED a message to its RabbitMQ sink
    queue, by comparing the queue's depth before and after the run and — when the
    queue is shallow enough for it to mean anything — confirming the run's own
    correlation token in the payload.

.DESCRIPTION
    Written for the post-deploy smoke test in
    Dev/.azure/ProfilerService-CI-UAT-LightweightExecutionEngine.yml, which proves
    ProfilerWrapper/RequestForProfiling reached its trailing 'RabbitMQ Publish'.

    WHY NOT JUST PEEK FOR THE TOKEN. That is what the pipeline did originally, and
    it is unsound on any queue with a backlog. The management API's
    POST /api/queues/{vhost}/{queue}/get always serves from the HEAD of the queue;
    a message published a moment ago is at the TAIL. Build 30750 peeked 50
    messages, 18 times, against a queue holding 19,100 — so it could only ever
    see the oldest ~900, reported "no message carrying MandateId ... appeared",
    and failed a deploy in which the publish had in fact succeeded. (Proven by
    purging that queue and re-running the identical assertion, which then passed
    on the first poll.) The sink is a load-test target with no consumer attached,
    so the backlog is the normal state, not an anomaly.

    WHAT THIS ASSERTS INSTEAD. The queue's depth is read BEFORE the run (via
    -ReportDepthOnly) and polled AFTER it until it has risen by at least one.
    That is backlog-independent: it holds at depth 0 and at depth 19,100 alike.

    Its cost, stated plainly because the pipeline relies on it: a depth delta is
    NOT run-attributable. A concurrent publisher to the same queue can satisfy it
    while this run's own publish failed, and a consumer draining faster than the
    run publishes can hide a delta that did occur. On this sink both are
    currently absent (consumers = 0, and the load-test pipeline targets its own
    '-loadtest'-suffixed queues), which is what makes the trade acceptable here.

    SO THE TOKEN SEARCH IS KEPT, as an opportunistic upgrade rather than the
    assertion: when the queue is at or below -PeekThreshold, the head window
    covers the whole queue, the search is meaningful, and a hit upgrades the
    result to run-attributable. Above that threshold the peek is SKIPPED rather
    than attempted-and-failed — peeking 50 of 19,100 cannot answer the question
    and only burns the poll window (it cost build 30750 ninety seconds of
    identical reads).

    DIAGNOSABILITY. Every outcome reports the actual depth. The original step
    logged "50 message(s) visible on the last read" — the count it had asked for,
    which is the same number whether the queue holds 50 messages or 19,100, and
    so said nothing at all. "depth 19100 -> 19101" ends the same investigation in
    one read.

.PARAMETER ManagementUri
    REQUIRED. Base URI of the RabbitMQ/LavinMQ management API, e.g.
    https://ostrich-01.lmq.cloudamqp.com. A trailing '/' is tolerated.
.PARAMETER QueueName
    REQUIRED. The sink queue to inspect.
.PARAMETER Username / Password
    REQUIRED. Management credentials with read access to the queue. -Password is a
    SecureString; the plaintext is materialised only for the Basic auth header and
    zeroed immediately after.
.PARAMETER VHost
    Virtual host containing the queue. Default: / (encoded as %2f, the one vhost
    that must not be left bare).
.PARAMETER DepthBefore
    REQUIRED unless -ReportDepthOnly. The depth captured before the run, as
    returned by an earlier -ReportDepthOnly call. A negative value means the
    before-read never happened, and is rejected rather than assumed to be 0 —
    defaulting it would turn a broken pre-step into a silent pass.
.PARAMETER CorrelationToken
    OPTIONAL. The run-scoped token expected in the message payload (the smoke
    test's MandateId). When omitted, only the depth delta is asserted.
.PARAMETER PeekThreshold
    Maximum queue depth at which the head peek is still meaningful. Default 200.
.PARAMETER TimeoutSeconds / PollIntervalSeconds
    Bounded window to wait for the delta. Defaults: 90 / 5.
.PARAMETER ReportDepthOnly
    Reads the queue depth, writes it to the pipeline as the only object on the
    success stream, and exits. This is the "before" half of the assertion.
.PARAMETER LoadFunctionsOnly
    Test hook — define helpers then return, performing no broker call.

.EXAMPLE
    # Before the run
    $before = ./Test-WwSinkDelivery.ps1 -ReportDepthOnly -ManagementUri $u -QueueName $q `
                  -VHost $v -Username $user -Password $pw

.EXAMPLE
    # After the run
    ./Test-WwSinkDelivery.ps1 -ManagementUri $u -QueueName $q -VHost $v -Username $user `
        -Password $pw -DepthBefore $before -CorrelationToken $mandateId

.NOTES
    Non-destructive against the broker: GET /api/queues/... and, at most, a
    POST .../get with ackmode 'reject_requeue_true'. Nothing here consumes,
    purges or declares.

    That ackmode is load-bearing, not incidental — see Get-QueueHeadMessages.
    'ack_requeue_true', which this check used originally and which reads as
    though it requeues, CONSUMES on LavinMQ (measured: depth 3 -> 0 on one peek).
    Anything that "tidies up" this value back to the more familiar spelling
    silently turns a verification step into a consumer of the queue it verifies.

    Runs on PowerShell 7+ ONLY, and not by preference — CloudAMQP's front end asks
    for a TLS renegotiation mid-request, which .NET Framework's HttpWebRequest
    (Windows PowerShell 5.1, under Invoke-RestMethod) refuses, reporting the
    generic 'Could not create SSL/TLS secure channel'. That is what failed build
    30747. .NET 8 handles it. Forcing Tls12 does not help; it is not a
    protocol-version problem.
#>
[CmdletBinding()]
param(
    [string] $ManagementUri,
    [string] $QueueName,
    [string] $Username,
    [securestring] $Password,
    [string] $VHost = '/',

    [int] $DepthBefore = -1,
    [string] $CorrelationToken,
    [int] $PeekThreshold = 200,
    [int] $TimeoutSeconds = 90,
    [int] $PollIntervalSeconds = 5,

    [switch] $ReportDepthOnly,
    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers  (same conventions as Monitor-RabbitMqShovel.ps1)
# ════════════════════════════════════════════════════════════════════════════

function Write-Step { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }

function ConvertFrom-SecureStringPlain {
    param([securestring] $Value)
    if ($null -eq $Value -or $Value.Length -eq 0) { return '' }
    $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try { return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
    finally { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

function Get-VHostForApi {
    <# '/' is the one vhost that must be encoded as %2f rather than left bare. #>
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Name)
    if ($Name -eq '/') { return '%2f' }
    return [Uri]::EscapeDataString($Name)
}

function Invoke-RabbitMqApi {
    <# Thin wrapper over Invoke-RestMethod against the management API. Only the
       two read shapes this script needs: a GET, and the POST .../get that peeks
       (and requeues) messages. #>
    param(
        [Parameter(Mandatory)][string] $Path,
        [ValidateSet('Get', 'Post')][string] $Method = 'Get',
        [string] $Body
    )
    $uri = "$($ManagementUri.TrimEnd('/'))$Path"
    $plainPassword = ConvertFrom-SecureStringPlain $Password
    try {
        $pair = "${Username}:${plainPassword}"
        $authHeader = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($pair)) }
        if ($Method -eq 'Post') {
            return Invoke-RestMethod -Method Post -Uri $uri -Headers $authHeader `
                       -ContentType 'application/json' -Body $Body -TimeoutSec 30
        }
        return Invoke-RestMethod -Method Get -Uri $uri -Headers $authHeader -TimeoutSec 30
    } catch {
        $detail = $_.Exception.Message
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $detail = $_.ErrorDetails.Message }
        throw "Sink verification could not reach the management API ($Method $Path): $detail"
    } finally {
        $plainPassword = $null
    }
}

function Get-QueueDepth {
    <# Total messages on the queue (ready + unacknowledged). #>
    param([Parameter(Mandatory)][string] $VHostName, [Parameter(Mandatory)][string] $Name)
    $q = Invoke-RabbitMqApi -Path "/api/queues/$(Get-VHostForApi $VHostName)/$([Uri]::EscapeDataString($Name))"
    return [int] $q.messages
}

function Get-QueueHeadMessages {
    <# Peeks up to $Count messages from the HEAD, without consuming them.

       ackmode 'reject_requeue_true', NOT 'ack_requeue_true'. The latter is what
       this check used originally, on the reasonable assumption that a mode with
       "requeue_true" in its name requeues. On the LavinMQ instance CloudAMQP
       silently migrated this broker to, it does NOT — measured on a throwaway
       queue, three messages in, one peek, depth 3 -> 0, and a second peek
       returning nothing. The queue's own stats corroborate it: reject 30,926,
       ack 0, and redeliver 0 — nothing was ever handed out twice, which is
       exactly what "requeued" would have produced.

       The practical consequence is that the ORIGINAL smoke test was destructive:
       its 18 reads of 50 destroyed up to 900 messages from the sink per build,
       while its comment claimed it "cannot swallow work belonging to a real
       consumer". Only 'reject_requeue_true' is actually non-destructive here
       (same probe: depth 3 -> 3), so that is what this uses. #>
    param(
        [Parameter(Mandatory)][string] $VHostName,
        [Parameter(Mandatory)][string] $Name,
        [int] $Count = 50
    )
    $body = (@{
        count    = $Count
        ackmode  = 'reject_requeue_true'
        encoding = 'auto'
        truncate = 50000
    } | ConvertTo-Json -Compress)
    $msgs = Invoke-RabbitMqApi -Method Post -Body $body `
                -Path "/api/queues/$(Get-VHostForApi $VHostName)/$([Uri]::EscapeDataString($Name))/get"
    return @($msgs)
}

function Wait-ForSinkDelivery {
    <# Polls until the queue is at least one message deeper than $Before, then
       (only when the head window covers the whole queue) tries to attribute the
       new message to this run. Throws when no delta appears in the window.

       Returns a summary object: Delivered / Attributable / DepthBefore /
       DepthAfter / Peeked / Polls. #>
    param(
        [Parameter(Mandatory)][string] $VHostName,
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][int]    $Before,
        [string] $Token,
        [int] $Threshold = 200,
        [int] $Timeout = 90,
        [int] $Interval = 5
    )

    $deadline     = (Get-Date).AddSeconds($Timeout)
    $polls        = 0
    $depth        = $Before
    $delivered    = $false
    $attributable = $false
    $peeked       = 0

    # do/while, so the queue is always read at least once however short the
    # window is — a timeout of 0 must still produce a verdict, not zero reads.
    do {
        $polls++
        $depth = Get-QueueDepth -VHostName $VHostName -Name $Name
        if ($depth -ge ($Before + 1)) { $delivered = $true; break }
        if ((Get-Date) -ge $deadline) { break }
        Start-Sleep -Seconds $Interval
    } while ($true)

    if (-not $delivered) {
        throw ("Sink verification FAILED: queue '$Name' did not grow within ${Timeout}s " +
               "(depth $Before before the run, $depth now, $polls read(s)). The workflow's " +
               'trailing RabbitMQ Publish did not deliver.')
    }

    Write-Ok "Sink delivered: depth $Before -> $depth after $polls read(s)."

    if ([string]::IsNullOrWhiteSpace($Token)) {
        Write-Note 'No correlation token supplied — asserted on the depth delta alone.'
    }
    elseif ($depth -gt $Threshold) {
        # Deliberately NOT attempted: /get serves from the head, the new message is
        # at the tail, and a miss here would prove nothing while costing the whole
        # poll window. See this script's .DESCRIPTION (build 30750).
        Write-Note ("Queue depth $depth exceeds the peek threshold $Threshold — skipping the " +
                    'payload search. The delta above is the assertion; it is not run-attributable.')
    }
    else {
        $msgs   = Get-QueueHeadMessages -VHostName $VHostName -Name $Name -Count $depth
        $peeked = @($msgs).Count
        foreach ($m in @($msgs)) {
            if ($m.PSObject.Properties.Name -contains 'payload' -and $m.payload -and
                ($m.payload -match [regex]::Escape($Token))) {
                $attributable = $true
                break
            }
        }
        if ($attributable) {
            Write-Ok "Sink CONFIRMED: a message carrying '$Token' is on '$Name' ($peeked peeked)."
        } else {
            Write-Note ("Depth grew but '$Token' was not among the $peeked message(s) peeked. " +
                        'Treating the delta as the assertion; the delivery is not run-attributable.')
        }
    }

    return [pscustomobject]@{
        Delivered    = $true
        Attributable = $attributable
        DepthBefore  = $Before
        DepthAfter   = $depth
        Peeked       = $peeked
        Polls        = $polls
    }
}

if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Main
# ════════════════════════════════════════════════════════════════════════════

foreach ($required in @(
        @{ Name = 'ManagementUri'; Value = $ManagementUri },
        @{ Name = 'QueueName';     Value = $QueueName },
        @{ Name = 'Username';      Value = $Username })) {
    if ([string]::IsNullOrWhiteSpace($required.Value)) {
        throw "Required value '$($required.Name)' was not supplied. Pass -$($required.Name) <value>."
    }
}

# Checked before any call so an unset pipeline secret fails here, naming itself,
# rather than as an opaque 401 from the broker.
if ($null -eq $Password -or $Password.Length -eq 0) {
    throw ('The sink broker password is empty, so delivery cannot be verified. Set it as a ' +
           'SECRET variable on the pipeline definition in Azure DevOps.')
}

if ($ReportDepthOnly) {
    $depth = Get-QueueDepth -VHostName $VHost -Name $QueueName
    Write-Step "Sink '$QueueName' depth before the run: $depth"
    return $depth
}

# A negative DepthBefore means the pre-run step never published one. Assuming 0
# would make any pre-existing message look like this run's delivery.
if ($DepthBefore -lt 0) {
    throw ('No pre-run sink depth was supplied (-DepthBefore). The step that captures it did ' +
           'not run or failed before publishing its value, so a delta cannot be computed.')
}

Write-Step "Verifying delivery to '$QueueName' (vhost '$VHost'), depth before the run: $DepthBefore"

Wait-ForSinkDelivery -VHostName $VHost -Name $QueueName -Before $DepthBefore `
    -Token $CorrelationToken -Threshold $PeekThreshold `
    -Timeout $TimeoutSeconds -Interval $PollIntervalSeconds
