#Requires -Version 7.0
<#
.SYNOPSIS
    Asserts that a deployed ShovelBridge actually CARRIES a message: RabbitMQ
    source queue -> RabbitMQ Shovel -> Azure Service Bus -> the engine's own
    ServiceBusTrigger, without dead-lettering it.

.DESCRIPTION
    Written for the post-deploy smoke test in
    Dev/.azure/ProfilerService-CI-UAT-LightweightExecutionEngine.yml. It is the
    QUEUE-path counterpart to Test-WwSinkDelivery.ps1: that script proves the
    workflow's trailing 'RabbitMQ Publish' reached the sink, but the HTTP smoke
    test that drives it never touches the bridge at all, so a completely dead
    bridge used to leave the pipeline green.

    THE FAILURE THIS EXISTS TO CATCH (2026-09-18, wwprofilerlw). The deploy
    declared its shovel with src-uri amqp://...@rabbitmq.warewolf.online:5672 —
    the broker's PUBLIC name. That URI is dialled BY THE BROKER NODE ITSELF, and
    only 443/management is published on that name, so the source link never
    connected: the shovel sat in state 'starting' forever, the source queue had
    consumers = 0, and nothing moved. Every working shovel on that broker dials
    amqp://localhost:5672 instead (visible as peer 127.0.0.1 in
    /api/connections). Configure-RabbitMqShovel.ps1's Phase 3 only WARNS when
    the shovel never reaches 'running', and the pipeline had no step after it,
    so the build reported success over a bridge that could not carry a message.

    WHAT IS ASSERTED, AND WHY IN THIS ORDER. One message is published to the
    source queue over the management HTTP API, then:

      1. the source queue must return to its baseline depth  -> the SHOVEL
         consumed it (this is the leg that was broken);
      2. the Service Bus queue's deadLetterMessageCount must NOT rise -> the
         engine's ServiceBusTrigger ACCEPTED the message. A dead-letter here is
         the WOLF-8517 contract-mismatch signature: the bridge works perfectly
         and every message is discarded (Malformed / Failed:ExecutionFailed);
      3. its activeMessageCount must return to 0 -> the trigger settled the
         delivery rather than leaving it to redeliver.

    A DRAINED SOURCE QUEUE PROVES NOTHING ON ITS OWN, which is why 2 and 3 are
    here: the shovel drains the queue whether Service Bus accepts the message or
    not, and the trigger drains Service Bus whether the workflow succeeded or
    was dead-lettered. The sink assertion (Test-WwSinkDelivery.ps1, run after
    this script with the same -CorrelationToken) is what proves the workflow
    then actually RAN. The three steps together cover publish -> shovel ->
    Service Bus -> trigger -> workflow -> sink.

    NOTHING IS CONSUMED OR PURGED. The only write is the single publish; every
    other call is a GET (management API) or 'az servicebus queue show'.

.PARAMETER ManagementUri
    REQUIRED. Base URI of the SOURCE broker's management API, e.g.
    https://rabbitmq.warewolf.online. A trailing '/' is tolerated.
.PARAMETER SourceQueue
    REQUIRED. The RabbitMQ queue the shovel consumes from.
.PARAMETER Username / Password
    REQUIRED. Management credentials able to read the queue and publish to the
    default exchange. -Password is a SecureString; the plaintext is materialised
    only for the Basic auth header and zeroed immediately after.
.PARAMETER VHost
    Virtual host containing the source queue. Default: / (encoded as %2f, the
    one vhost that must not be left bare).
.PARAMETER InputFields
    REQUIRED. The workflow's input fields, wrapped as {"inputs":{...}} — the
    post-WOLF-8517 message contract. A FLAT body is the trap this guards:
    WOLF-8517 removed the 'workflow' field from ServiceBusWorkflowMessage (the
    name now comes solely from Settings/servicebustrigger.bite), and a flat body
    binds as Inputs=null, executing against an EMPTY datalist rather than
    failing cleanly.
.PARAMETER CorrelationToken
    OPTIONAL but recommended. The run-scoped value (the smoke test's MandateId)
    echoed in this script's own output, so the same token can be handed to
    Test-WwSinkDelivery.ps1 and the two assertions describe one message.
.PARAMETER ServiceBusNamespace / ServiceBusQueueName / ResourceGroup
    OPTIONAL as a set. When all three are supplied, assertions 2 and 3 above are
    made via 'az servicebus queue show' (so the step must run under an
    az-authenticated context — AzureCLI@2, not a bare pwsh step). When omitted,
    only the shovel leg is asserted and the script says so rather than implying
    a fuller proof than it made.
.PARAMETER BridgeTimeoutSeconds
    How long the shovel is given to consume the published message. Default 120.
    A healthy shovel takes well under a second; this window is sized for a
    reconnect, not for throughput.
.PARAMETER ExecutionTimeoutSeconds
    How long the engine is given to settle the Service Bus delivery. Default
    300, sized for a COLD START of a Y1/Dynamic Function App plus the workflow
    itself — the deploy has just restarted the app, so the first queue-triggered
    execution of a build is always the slowest one.
.PARAMETER PollIntervalSeconds
    Seconds between polls. Default 5.
.PARAMETER LoadFunctionsOnly
    Test hook — define helpers then return, performing no broker or az call.

.EXAMPLE
    ./Test-WwShovelBridgeDelivery.ps1 `
        -ManagementUri https://rabbitmq.warewolf.online -SourceQueue profiler.profile.request.DEV `
        -Username testuser -Password $pw -InputFields $fields -CorrelationToken $mandateId `
        -ServiceBusNamespace wwprofilerlwsb -ServiceBusQueueName profiler.profile.request.DEV -ResourceGroup DEV2

.NOTES
    Publishing over the MANAGEMENT API, not AMQP, is deliberate: RabbitMQ.Client
    7.0.0 stack-overflows in its socket reader while an AMQP connection is torn
    down on .NET 10, killing the host process with no catchable exception (the
    same reason Run-WwProfilerExactLoad.ps1 reads depth over HTTP). The publish
    is made with delivery_mode 2 so the message is persistent, matching what the
    real publisher sends to a durable queue.
#>
[CmdletBinding()]
param(
    [string] $ManagementUri,
    [string] $SourceQueue,
    [string] $Username,
    [securestring] $Password,
    [string] $VHost = '/',

    [hashtable] $InputFields,
    [string] $CorrelationToken,

    [string] $ServiceBusNamespace,
    [string] $ServiceBusQueueName,
    [string] $ResourceGroup,

    [int] $BridgeTimeoutSeconds    = 120,
    [int] $ExecutionTimeoutSeconds = 300,
    [int] $PollIntervalSeconds     = 5,

    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers  (same conventions as Test-WwSinkDelivery.ps1)
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

function Format-WwBridgeMessageBody {
    <#
        The engine's documented {"inputs":{...}} envelope — the same shape
        Publish-WwProfilerExact.ps1 sends, and the reason that script carries a
        long note about flat bodies. Keys are emitted in sorted order so the
        body is byte-stable across runs and a diff of two builds' logs is
        readable; the engine binds by name, so order is presentation only.

        EMPTY VALUES ARE KEPT, NOT DROPPED. 'RequestType' and 'FASGroup' are
        both legitimately empty in the canonical profiler request, and a smoke
        test that omits them is not exercising the shape the real publisher
        sends.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][hashtable] $Fields)

    $ordered = [ordered]@{}
    foreach ($key in ($Fields.Keys | Sort-Object)) {
        $ordered[$key] = if ($null -eq $Fields[$key]) { '' } else { [string]$Fields[$key] }
    }
    return ([ordered]@{ inputs = $ordered } | ConvertTo-Json -Compress -Depth 5)
}

function New-WwBridgePublishRequest {
    <#
        The management API's publish body. Routed through the DEFAULT exchange
        (amq.default) with routing_key = the queue name, which is how a plain
        queue publish is expressed over HTTP.

        delivery_mode 2 (persistent) matches what the real publisher sends to
        this durable queue: a transient message would be dropped on a broker
        restart and the smoke test would report a bridge failure that never
        happened.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $RoutingKey,
        [Parameter(Mandatory)][string] $Body
    )
    return [ordered]@{
        properties       = [ordered]@{ delivery_mode = 2 }
        routing_key      = $RoutingKey
        payload          = $Body
        payload_encoding = 'string'
    }
}

function Get-WwBridgeVerdict {
    <#
        The whole decision, as a pure function of the numbers — no clock, no
        I/O — so every outcome is unit-testable and every message can name the
        actual depths that produced it.

        ORDER MATTERS. The dead-letter check comes FIRST because a dead-lettered
        message also drains the source queue and also leaves activeMessageCount
        at 0: judged on drain alone, the WOLF-8517 contract mismatch (every
        message discarded, bridge perfectly healthy) reads as a pass.

        A negative -DeadLetterBefore/-ActiveNow means "Service Bus was not
        inspected" (the namespace/queue/resource-group set was not supplied) and
        those two assertions are skipped rather than assumed to have passed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int] $SourceDepth,
        [Parameter(Mandatory)][int] $SourceBaseline,
        [int] $DeadLetterBefore = -1,
        [int] $DeadLetterNow    = -1,
        [int] $ActiveNow        = -1,
        [switch] $BridgeWindowExpired,
        [switch] $ExecutionWindowExpired
    )

    if ($DeadLetterBefore -ge 0 -and $DeadLetterNow -gt $DeadLetterBefore) {
        return [pscustomobject]@{
            Done = $true; Failed = $true; Stage = 'Engine'
            Reason = ("the message reached Service Bus and was DEAD-LETTERED " +
                      "(deadLetterMessageCount $DeadLetterBefore -> $DeadLetterNow). The bridge is " +
                      'healthy; the engine rejected the message — check the ServiceBusTriggerOutcome ' +
                      'traces for Malformed (payload contract) or Failed:ExecutionFailed (workflow).')
        }
    }

    if ($SourceDepth -gt $SourceBaseline) {
        if ($BridgeWindowExpired) {
            return [pscustomobject]@{
                Done = $true; Failed = $true; Stage = 'Shovel'
                Reason = ("the shovel did not consume the message (source queue depth $SourceBaseline " +
                          "before the publish, $SourceDepth now). Check the shovel's state and its " +
                          "src-uri: it is dialled BY THE BROKER NODE, so it must be the broker's own " +
                          'loopback address, not a public name that only publishes 443.')
            }
        }
        return [pscustomobject]@{ Done = $false; Failed = $false; Stage = 'Shovel'; Reason = 'waiting for the shovel to consume' }
    }

    if ($ActiveNow -lt 0) {
        return [pscustomobject]@{
            Done = $true; Failed = $false; Stage = 'Shovel'
            Reason = 'the shovel consumed the message (Service Bus was not inspected)'
        }
    }

    if ($ActiveNow -gt 0) {
        if ($ExecutionWindowExpired) {
            return [pscustomobject]@{
                Done = $true; Failed = $true; Stage = 'Engine'
                Reason = ("the message reached Service Bus but the trigger never settled it " +
                          "(activeMessageCount $ActiveNow). The bridge is healthy; the engine is not " +
                          'consuming — check WAREWOLF_SERVICEBUS_TRIGGER_QUEUE and the app''s state.')
            }
        }
        return [pscustomobject]@{ Done = $false; Failed = $false; Stage = 'Engine'; Reason = 'waiting for the trigger to settle the delivery' }
    }

    return [pscustomobject]@{
        Done = $true; Failed = $false; Stage = 'Complete'
        Reason = 'the shovel delivered it and the trigger settled it without dead-lettering'
    }
}

function Invoke-RabbitMqApi {
    <# Thin wrapper over Invoke-RestMethod against the management API — a GET,
       and the POST that publishes one message. #>
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
        throw "ShovelBridge verification could not reach the management API ($Method $Path): $detail"
    } finally {
        $plainPassword = $null
    }
}

function Get-QueueDepth {
    <# Total messages on the queue (ready + unacknowledged). Unacked is included
       on purpose: a message the shovel has taken but not yet confirmed has NOT
       left the bridge, and counting only messages_ready would call that a
       successful hand-off. #>
    param([Parameter(Mandatory)][string] $VHostName, [Parameter(Mandatory)][string] $Name)
    $q = Invoke-RabbitMqApi -Path "/api/queues/$(Get-VHostForApi $VHostName)/$([Uri]::EscapeDataString($Name))"
    return [int] $q.messages
}

function Publish-BridgeMessage {
    <# Publishes one message and asserts the broker ROUTED it. 'routed: false'
       means the default exchange found no queue by that name — a typo or a
       queue that was never declared — and would otherwise look exactly like a
       shovel that consumed instantly. #>
    param(
        [Parameter(Mandatory)][string] $VHostName,
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][string] $Body
    )
    $request = New-WwBridgePublishRequest -RoutingKey $Name -Body $Body
    $result  = Invoke-RabbitMqApi -Method Post -Body ($request | ConvertTo-Json -Compress -Depth 5) `
                   -Path "/api/exchanges/$(Get-VHostForApi $VHostName)/amq.default/publish"

    $routed = $false
    if ($null -ne $result -and $result.PSObject.Properties.Name -contains 'routed') { $routed = [bool]$result.routed }
    if (-not $routed) {
        throw ("ShovelBridge verification FAILED at the publish: the broker did not route the message to " +
               "'$Name' on vhost '$VHostName'. The queue does not exist under that exact name, so the " +
               'bridge was never exercised.')
    }
    return $true
}

function Get-ServiceBusQueueCounts {
    <# activeMessageCount / deadLetterMessageCount via az. Returns $null when the
       namespace/queue/resource-group set was not supplied, which the caller
       reports as "Service Bus not inspected" rather than as a pass. #>
    param(
        [string] $Namespace,
        [string] $Queue,
        [string] $Group
    )
    if ([string]::IsNullOrWhiteSpace($Namespace) -or [string]::IsNullOrWhiteSpace($Queue) -or
        [string]::IsNullOrWhiteSpace($Group)) { return $null }

    $raw = & az servicebus queue show --name $Queue --namespace-name $Namespace --resource-group $Group -o json 2>&1

    # Read through Get-Variable, not $LASTEXITCODE directly: it is an automatic
    # variable that does not EXIST until the session has run a native command,
    # and Set-StrictMode -Version Latest turns that first read into a
    # "variable is not set" error — which would surface as a bogus az failure
    # on a clean pwsh step.
    $exit = Get-Variable -Name LASTEXITCODE -Scope Global -ValueOnly -ErrorAction SilentlyContinue
    if ($null -eq $exit) { $exit = 0 }
    if ($exit -ne 0) {
        throw ("ShovelBridge verification could not read Service Bus queue '$Queue' on '$Namespace' " +
               "(exit $exit): $raw. This step must run under an az-authenticated context " +
               '(AzureCLI@2, not a bare pwsh step).')
    }
    # '2>&1' above is kept for the throw's sake, but stderr MUST NOT reach the value being
    # parsed: az writes warnings to stderr, merged stderr arrives as ErrorRecords, and a
    # single warning line ahead of the JSON makes ConvertFrom-Json fail with "Unexpected
    # character encountered while parsing value: W". That is exactly how build 30759 died
    # inside Enable-ServiceBusSecureTrigger.ps1, whose Invoke-Az now splits the streams the
    # same way.
    $json = @($raw | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] })
    $parsed = ($json | Out-String) | ConvertFrom-Json
    return [pscustomobject]@{
        Active     = [int]$parsed.countDetails.activeMessageCount
        DeadLetter = [int]$parsed.countDetails.deadLetterMessageCount
    }
}

if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Main
# ════════════════════════════════════════════════════════════════════════════

foreach ($required in @(
        @{ Name = 'ManagementUri'; Value = $ManagementUri },
        @{ Name = 'SourceQueue';   Value = $SourceQueue },
        @{ Name = 'Username';      Value = $Username })) {
    if ([string]::IsNullOrWhiteSpace($required.Value)) {
        throw "Required value '$($required.Name)' was not supplied. Pass -$($required.Name) <value>."
    }
}

# Checked before any call so an unset pipeline secret fails here, naming itself,
# rather than as an opaque 401 from the broker.
if ($null -eq $Password -or $Password.Length -eq 0) {
    throw ('The source broker password is empty, so the bridge cannot be verified. Set it as a ' +
           'SECRET variable on the pipeline definition in Azure DevOps.')
}

if ($null -eq $InputFields -or $InputFields.Count -eq 0) {
    throw ('-InputFields is empty. A bridge test that publishes no workflow inputs proves the ' +
           'transport and nothing else: the trigger would execute against an empty datalist and ' +
           'the failure would read as a broken workflow.')
}

$inspectServiceBus = -not ([string]::IsNullOrWhiteSpace($ServiceBusNamespace) -or
                           [string]::IsNullOrWhiteSpace($ServiceBusQueueName) -or
                           [string]::IsNullOrWhiteSpace($ResourceGroup))

Write-Host ''
Write-Host 'ShovelBridge delivery verification' -ForegroundColor Cyan
Write-Step "source queue : $SourceQueue (vhost '$VHost') @ $ManagementUri"
if ($inspectServiceBus) { Write-Step "service bus  : $ServiceBusQueueName @ $ServiceBusNamespace ($ResourceGroup)" }
else { Write-Note 'Service Bus was not supplied — only the shovel leg will be asserted.' }
if ($CorrelationToken) { Write-Step "correlation  : $CorrelationToken" }

$sourceBaseline = Get-QueueDepth -VHostName $VHost -Name $SourceQueue
$sbBaseline     = Get-ServiceBusQueueCounts -Namespace $ServiceBusNamespace -Queue $ServiceBusQueueName -Group $ResourceGroup
$dlqBefore      = if ($sbBaseline) { $sbBaseline.DeadLetter } else { -1 }
Write-Step ("baseline     : source depth $sourceBaseline" +
            $(if ($sbBaseline) { ", service bus active $($sbBaseline.Active) / dead-letter $($sbBaseline.DeadLetter)" } else { '' }))

$body = Format-WwBridgeMessageBody -Fields $InputFields
Write-Step "publishing 1 message ($([Text.Encoding]::UTF8.GetByteCount($body)) bytes)"
Publish-BridgeMessage -VHostName $VHost -Name $SourceQueue -Body $body | Out-Null
$publishedAt = Get-Date
Write-Ok 'Published and routed to the source queue.'

$verdict = $null
$polls   = 0
do {
    $polls++
    $elapsed     = ((Get-Date) - $publishedAt).TotalSeconds
    $sourceDepth = Get-QueueDepth -VHostName $VHost -Name $SourceQueue
    $sbNow       = Get-ServiceBusQueueCounts -Namespace $ServiceBusNamespace -Queue $ServiceBusQueueName -Group $ResourceGroup

    $verdict = Get-WwBridgeVerdict -SourceDepth $sourceDepth -SourceBaseline $sourceBaseline `
                   -DeadLetterBefore $dlqBefore `
                   -DeadLetterNow   $(if ($sbNow) { $sbNow.DeadLetter } else { -1 }) `
                   -ActiveNow       $(if ($sbNow) { $sbNow.Active } else { -1 }) `
                   -BridgeWindowExpired:($elapsed -ge $BridgeTimeoutSeconds) `
                   -ExecutionWindowExpired:($elapsed -ge $ExecutionTimeoutSeconds)

    Write-Host ("     {0,4}s  source={1,-4} {2}{3}" -f [int]$elapsed, $sourceDepth,
                $(if ($sbNow) { "sbActive=$($sbNow.Active) sbDlq=$($sbNow.DeadLetter)  " } else { '' }),
                $verdict.Stage)

    if ($verdict.Done) { break }
    Start-Sleep -Seconds $PollIntervalSeconds
} while ($true)

if ($verdict.Failed) {
    throw "ShovelBridge verification FAILED at the $($verdict.Stage) leg after $polls poll(s): $($verdict.Reason)"
}

Write-Ok "ShovelBridge verified ($polls poll(s)): $($verdict.Reason)."
if (-not $inspectServiceBus) {
    Write-Note ('Only the shovel leg was proven. Pass -ServiceBusNamespace/-ServiceBusQueueName/' +
                '-ResourceGroup to also assert the trigger accepted and settled the message.')
}

return [pscustomobject]@{
    Delivered        = $true
    Stage            = $verdict.Stage
    Polls            = $polls
    SourceBaseline   = $sourceBaseline
    CorrelationToken = $CorrelationToken
    ServiceBusChecked = $inspectServiceBus
}
