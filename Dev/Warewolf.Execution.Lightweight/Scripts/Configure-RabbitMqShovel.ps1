#Requires -Version 7.0
<#
.SYNOPSIS
    Configures a RabbitMQ "Shovel" that bridges messages from an existing RabbitMQ
    source queue to the Azure Service Bus queue consumed by the
    Warewolf Execution Service Bus worker (WwExecutionServiceBusWorker /
    Deploy-WwExecutionServiceBusWorker.ps1) — the "shovel bridge" that lets a
    RabbitMQ-based trigger reach the serverless Lightweight execution engine, which
    has no native RabbitMQ trigger binding (see docs/ShovelBridge-Architecture.md).

.DESCRIPTION
    RabbitMQ's Shovel plugin can forward messages from an AMQP 0.9.1 source queue to
    an AMQP 1.0 destination (Microsoft's documented pattern for bridging to Service
    Bus: https://learn.microsoft.com/azure/service-bus-messaging/service-bus-integrate-lightweight-messaging-with-shovel).
    This script configures a DYNAMIC shovel (a runtime parameter, not a config-file
    static shovel) via the RabbitMQ Management HTTP API:

        PUT /api/parameters/shovel/{vhost}/{name}
        { "value": {
            "src-protocol": "amqp091", "src-uri": "amqp://user:pass@host:5672/vhost",
            "src-queue": "<SourceQueue>",
            "dest-protocol": "amqp10",
            "dest-uri": "amqps://<policy>:<key>@<namespace>.servicebus.windows.net:5671/?sasl=plain",
            "dest-address": "<ServiceBusQueueName>",
            "ack-mode": "on-confirm", "reconnect-delay": <seconds>
        } }

    Same conventions as the other Scripts/*.ps1 orchestrators: params-first /
    prompt-if-missing, -DryRun (prints the exact HTTP call, mutates nothing), a masked
    settings summary + single confirmation, transcript + JSON run summary, and a
    -LoadFunctionsOnly test hook for Pester.

    PREREQUISITE (NOT automated by this script): the rabbitmq_shovel and
    rabbitmq_shovel_management plugins must already be enabled on the broker —
        rabbitmq-plugins enable rabbitmq_shovel rabbitmq_shovel_management
    This is a one-time, broker-host-level admin action (SSH/RDP to the broker or its
    container image), not something the Management HTTP API can do remotely on a
    stock RabbitMQ install. The script PROBES for the plugin (Phase 1) and fails with
    that exact instruction if it's missing, rather than silently no-op'ing.

    SECOND PREREQUISITE (also NOT automated by this script, and easy to miss because
    it produces no error until you actually run the shovel): the broker needs an
    advanced.config entry so its AMQP 1.0 client (amqp10_client, used for the
    dest-protocol=amqp10 connection to Service Bus) does RFC 6125-correct wildcard
    hostname matching. Erlang's *default* TLS peer verification (verify_peer, which
    applies whenever dest-uri omits an explicit '?verify=' override, as this script's
    dest-uri always does) does a LITERAL match against the certificate's SANs and does
    NOT expand wildcards — Azure Service Bus presents *.servicebus.windows.net /
    servicebus.windows.net, which never literal-matches a real namespace FQDN. Without
    this fix, EVERY shovel built by this script against a real Service Bus namespace
    will fail with the Shovel going to 'terminated', reason "failed to connect to
    destination" (root-caused and reproduced live against a real namespace — see
    docs/ShovelBridge-Architecture.md). Add this to the broker's advanced.config
    (typically %APPDATA%\RabbitMQ\advanced.config on Windows, or
    /etc/rabbitmq/advanced.config on Linux) and restart the broker:
        [
          {amqp10_client, [
            {ssl_options, [
              {customize_hostname_check, [
                {match_fun, public_key:pkix_verify_hostname_match_fun(https)}
              ]}
            ]}
          ]}
        ].
    This keeps full certificate chain + hostname validation (verify_peer, no downgrade
    to verify_none) while correctly accepting the wildcard SAN — verified against the
    real WarewolfShovelBridgeTesting namespace. If you cannot modify the broker's
    config (e.g. a managed/shared broker you don't control), pass -DestUriVerifyNone
    to append '&verify=verify_none' to the dest-uri instead — this disables ALL peer
    certificate validation (not just the hostname check), not just as documentation
    but as an actual opt-in switch. NOT recommended outside of diagnosing/confirming
    this exact TLS issue against a broker you don't control; prefer the advanced.config
    fix above wherever possible.

    THIRD PREREQUISITE, separate from and additional to the above (also not automated by
    this script): on Erlang/OTP 26+ brokers, the dest-uri's default verify_peer TLS mode
    ALSO requires an explicit CA trust store, or the Shovel crash-loops with
    '{cacerts, undefined}' in the broker's log — OTP 26 no longer falls back to an
    implicit/system CA store. This produces a near-instant fail/retry loop that the
    Management API's shovel-status summary reports only as "starting" (the same
    transient state seen during a normal connection attempt), so it can look like a
    hanging connection rather than a crash-loop unless you check the broker's own log.
    Root-caused live against the real WarewolfShovelBridgeTesting namespace: applying
    ONLY the advanced.config hostname-check fix above did not resolve it; the broker log
    showed '{cacerts, undefined}' as the actual reason. Fix: pass -DestUriCaCertFile
    pointing at a CA bundle path readable by the broker (e.g.
    /etc/ssl/certs/ca-certificates.crt, the standard system bundle on the Debian-based
    official RabbitMQ Docker image) to append '&cacertfile=<path>' to the dest-uri. See
    docs/ShovelBridge-Architecture.md "Security" for full detail.

    The destination SAS credential should be the queue-scoped Send-only rule created
    by Deploy-WwExecutionServiceBusWorker.ps1 (default name: shovel-send) — least
    privilege: the Shovel only ever needs to publish, never to manage or receive.
    Pass the key directly (-ServiceBusSasKey) or let this script fetch it live via
    `az` (-ServiceBusResourceGroup) so the key is never typed/stored separately.

.PARAMETER RabbitMqManagementUri
    REQUIRED. Base URI of the RabbitMQ management API, e.g. http://localhost:15672
    (or https://<broker>:15671 for a TLS-enabled management listener).
.PARAMETER RabbitMqUsername / RabbitMqPassword
    REQUIRED. Credentials with the "administrator" (or at least "policymaker" +
    "monitoring") tag — dynamic shovel parameters are configured via the same ACL as
    policies. -RabbitMqPassword is a SecureString; prompted securely when omitted.
.PARAMETER VHost
    RabbitMQ virtual host containing the source queue. Default: / (the default vhost).
.PARAMETER ShovelName
    Name of the dynamic shovel parameter. Default: wwexecution-shovel.
.PARAMETER SourceQueue
    REQUIRED. Name of the existing RabbitMQ queue to shovel messages FROM (the
    RabbitMQ-side trigger queue — the producer/publisher side is unchanged by this
    script; it keeps publishing to this queue exactly as it does today).
.PARAMETER SourceHost / SourcePort
    AMQP 0.9.1 endpoint of the SOURCE broker (this same RabbitMQ instance, almost
    always). SourceHost REQUIRED; SourcePort defaults to 5672.
.PARAMETER SourceUsername / SourcePassword
    Credentials the Shovel uses to CONSUME the source queue. Defaults to
    -RabbitMqUsername/-RabbitMqPassword when omitted (same broker, same admin creds);
    override to use a least-privilege consumer account instead.
.PARAMETER ServiceBusNamespace
    REQUIRED. Destination Service Bus namespace (e.g. the one provisioned by
    Deploy-WwExecutionServiceBusWorker.ps1).
.PARAMETER ServiceBusQueueName
    Destination queue name. Default: wwexecution-queue (matches
    WorkflowQueueTrigger.cs's [ServiceBusTrigger("wwexecution-queue", ...)] and
    Deploy-WwExecutionServiceBusWorker.ps1's default).
.PARAMETER ServiceBusSasKeyName
    Name of the queue-scoped SAS authorization rule to authenticate the Shovel's AMQP
    1.0 connection. Default: shovel-send (the Send-only rule created by
    Deploy-WwExecutionServiceBusWorker.ps1).
.PARAMETER ServiceBusSasKey
    The SAS rule's primary key value (SecureString). Supply this OR
    -ServiceBusResourceGroup (to fetch it live via az) — not both.
.PARAMETER ServiceBusResourceGroup
    Resource group containing -ServiceBusNamespace; when supplied (and -ServiceBusSasKey
    is not), the script calls
        az servicebus queue authorization-rule keys list ... --query primaryKey
    to fetch the raw key live (never written to disk) and URL-encodes it into the
    destination URI itself.
.PARAMETER DestUriVerifyNone
    Opt-in fallback for the advanced.config TLS-hostname-check prerequisite documented
    above: appends '&verify=verify_none' to the dest-uri, disabling ALL AMQP 1.0 peer
    certificate validation (not just the hostname check) for the Shovel's connection to
    Service Bus. Only use this when you cannot modify the broker's advanced.config (e.g.
    a managed/shared broker) and need to confirm the hostname-check bug is the cause, or
    unblock a diagnostic run — NOT recommended for a broker you control or for
    production use. Prefer the advanced.config fix in the header comment above instead.
.PARAMETER DestUriCaCertFile
    Fix for the Erlang/OTP 26+ '{cacerts, undefined}' prerequisite documented above:
    appends '&cacertfile=<path>' to the dest-uri, pointing the Shovel's AMQP 1.0 TLS
    client at an explicit CA bundle (e.g. /etc/ssl/certs/ca-certificates.crt) readable
    by the broker process. Unlike -DestUriVerifyNone, this keeps full certificate
    validation — it is the recommended fix, not a fallback, whenever the broker's log
    shows '{cacerts, undefined}'. Path is not validated locally (it must exist on the
    broker host, which this script does not have filesystem access to).
.PARAMETER AckMode
    Shovel acknowledgement mode: on-confirm (default, safest — waits for the
    destination's publisher confirm before acking the source message), on-publish, or
    no-ack.
.PARAMETER ReconnectDelaySeconds
    Seconds the Shovel waits before reconnecting after a dropped link. Default: 5.
.PARAMETER SrcPrefetchCount
    Source-side prefetch (in-flight message cap). Default: 5.
.PARAMETER LogDir / NonInteractive / DryRun / LoadFunctionsOnly
    Logging output dir; unattended mode; preview-without-change; test hook.

.EXAMPLE
    # Local dev — RabbitMQ started by TestRun.ps1 -StartRabbitMQServer (management API
    # on localhost:15672, test/test creds), fetching the Service Bus key live via az.
    ./Configure-RabbitMqShovel.ps1 `
        -RabbitMqManagementUri http://localhost:15672 -RabbitMqUsername test -RabbitMqPassword (ConvertTo-SecureString test -AsPlainText -Force) `
        -SourceQueue orders-trigger-queue -SourceHost localhost `
        -ServiceBusNamespace ns-wwsb-test99 -ServiceBusResourceGroup rg-test-shovel `
        -DryRun

.NOTES
    This script does NOT create the RabbitMQ source queue (it must already exist —
    typically the same queue an existing producer or Studio-authored process already
    publishes to) and does NOT create the Service Bus destination queue (see
    Deploy-WwExecutionServiceBusWorker.ps1 for that side).
#>
[CmdletBinding()]
param(
    # RabbitMQ management API
    [string] $RabbitMqManagementUri,
    [string] $RabbitMqUsername,
    [securestring] $RabbitMqPassword,
    [string] $VHost = '/',
    [string] $ShovelName = 'wwexecution-shovel',

    # Source (RabbitMQ, AMQP 0.9.1)
    [string] $SourceQueue,
    [string] $SourceHost,
    [int]    $SourcePort = 5672,
    [string] $SourceUsername,
    [securestring] $SourcePassword,

    # Destination (Azure Service Bus, AMQP 1.0)
    [string] $ServiceBusNamespace,
    [string] $ServiceBusQueueName = 'wwexecution-queue',
    [string] $ServiceBusSasKeyName = 'shovel-send',
    [securestring] $ServiceBusSasKey,
    [string] $ServiceBusResourceGroup,
    [switch] $DestUriVerifyNone,
    [string] $DestUriCaCertFile,

    # Shovel behaviour
    [ValidateSet('on-confirm', 'on-publish', 'no-ack')] [string] $AckMode = 'on-confirm',
    [int] $ReconnectDelaySeconds = 5,
    [int] $SrcPrefetchCount = 5,

    # Logging / control
    [string] $LogDir,
    [switch] $NonInteractive,
    [switch] $DryRun,
    [switch] $LoadFunctionsOnly          # test hook — define helpers then return
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers  (same conventions as Deploy-WwExecutionServiceBusWorker.ps1 /
# Deploy-WwJobProcessor.ps1, adapted for the RabbitMQ Management HTTP API instead
# of the az CLI).
# ════════════════════════════════════════════════════════════════════════════

function Write-Phase {
    param([string] $Title)
    Write-Host ''
    Write-Host ('═' * 76) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ('═' * 76) -ForegroundColor Cyan
}

function Write-Step { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }

function Confirm-Yes {
    param([string] $Message, [bool] $DefaultYes = $true)
    if ($NonInteractive) { return $DefaultYes }
    $suffix = if ($DefaultYes) { '[Y/n]' } else { '[y/N]' }
    $answer = Read-Host "  $Message $suffix"
    if ([string]::IsNullOrWhiteSpace($answer)) { return $DefaultYes }
    return $answer -imatch '^y'
}

function Read-Required {
    param([string] $Name, [string] $Current, [string] $Hint)
    if (-not [string]::IsNullOrWhiteSpace($Current)) { return $Current }
    if ($NonInteractive) {
        throw "Required value '$Name' was not supplied. Pass -$Name <value> (running with -NonInteractive)."
    }
    $hintText = if ($Hint) { " ($Hint)" } else { '' }
    do {
        $v = Read-Host "  Enter $Name$hintText"
    } while ([string]::IsNullOrWhiteSpace($v))
    return $v.Trim()
}

function Read-RequiredSecure {
    <# Same contract as Read-Required, but for a SecureString value prompted with -AsSecureString. #>
    param([string] $Name, [securestring] $Current, [string] $Hint)
    if ($null -ne $Current -and $Current.Length -gt 0) { return $Current }
    if ($NonInteractive) {
        throw "Required secure value '$Name' was not supplied. Pass -$Name (SecureString) (running with -NonInteractive)."
    }
    $hintText = if ($Hint) { " ($Hint)" } else { '' }
    do {
        $v = Read-Host "  Enter $Name$hintText" -AsSecureString
    } while ($v.Length -eq 0)
    return $v
}

function ConvertFrom-SecureStringPlain {
    param([securestring] $Value)
    if ($null -eq $Value -or $Value.Length -eq 0) { return '' }
    $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try { return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
    finally { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

function Get-MaskedValue {
    param([string] $Value)
    if ([string]::IsNullOrEmpty($Value)) { return '' }
    if ($Value.Length -le 6) { return '******' }
    return ('{0}…(masked, len={1})' -f $Value.Substring(0, 3), $Value.Length)
}

function Get-MaskedUri {
    <# Masks the userinfo (user:pass@) portion of a URI for safe logging. #>
    param([string] $Uri)
    if ([string]::IsNullOrEmpty($Uri)) { return $Uri }
    return ($Uri -replace '://[^@/]+@', '://***:***@')
}

function Invoke-RabbitMqApi {
    <#
        Thin wrapper over Invoke-RestMethod against the RabbitMQ management API.
          -Mutating  : changes broker state (PUT/DELETE); skipped (echoed) under -DryRun.
    #>
    param(
        [Parameter(Mandatory)][string] $Method,
        [Parameter(Mandatory)][string] $Path,
        [object] $Body,
        [switch] $Mutating,
        [switch] $AllowFail
    )
    $uri = "$($RabbitMqManagementUri.TrimEnd('/'))$Path"
    if ($Mutating -and $DryRun) {
        # Never echo the real src-uri/dest-uri (they carry credentials/SAS keys in the
        # userinfo portion) — mask them the same way the settings summary does before
        # this preview is written to the console AND the transcript log.
        $maskedBody = $Body
        if ($null -ne $Body -and $Body -is [System.Collections.IDictionary] -and $Body.Contains('value')) {
            $maskedValue = [ordered]@{}
            foreach ($k in $Body['value'].Keys) {
                $maskedValue[$k] = if ($k -in @('src-uri', 'dest-uri')) { Get-MaskedUri $Body['value'][$k] } else { $Body['value'][$k] }
            }
            $maskedBody = @{ value = $maskedValue }
        }
        $bodyPreview = if ($null -ne $maskedBody) { ($maskedBody | ConvertTo-Json -Depth 6) } else { '(none)' }
        Write-Host "      [DRYRUN] $Method $uri" -ForegroundColor DarkGray
        Write-Host "      [DRYRUN] body: $bodyPreview" -ForegroundColor DarkGray
        return $null
    }
    $plainPassword = ConvertFrom-SecureStringPlain $RabbitMqPassword
    $pair = "${RabbitMqUsername}:${plainPassword}"
    $authHeader = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($pair)) }
    try {
        $params = @{
            Method      = $Method
            Uri         = $uri
            Headers     = $authHeader
            ContentType = 'application/json'
            TimeoutSec  = 30
        }
        if ($null -ne $Body) { $params['Body'] = ($Body | ConvertTo-Json -Depth 6) }
        return Invoke-RestMethod @params
    } catch {
        if ($AllowFail) { return $null }
        $detail = $_.Exception.Message
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) { $detail = $_.ErrorDetails.Message }
        throw "RabbitMQ management API call failed ($Method $Path): $detail"
    } finally {
        $plainPassword = $null
    }
}

function Format-Amqp091Uri {
    <#
        Builds an AMQP 0.9.1 URI per the RabbitMQ URI spec (https://www.rabbitmq.com/uri-spec.html).
        The default vhost "/" is represented by an EMPTY path (no trailing slash); any
        other vhost is percent-encoded as the path segment.
    #>
    param([string] $HostName, [int] $Port, [string] $User, [string] $Password, [string] $VHostName)
    $userEnc = [Uri]::EscapeDataString($User)
    $passEnc = [Uri]::EscapeDataString($Password)
    $pathSegment = if ($VHostName -eq '/') { '' } else { '/' + [Uri]::EscapeDataString($VHostName) }
    return "amqp://${userEnc}:${passEnc}@${HostName}:${Port}${pathSegment}"
}

function Format-ServiceBusAmqp10Uri {
    <#
        Builds the Service Bus AMQP 1.0 destination URI in the exact shape Microsoft
        documents for RabbitMQ Shovel bridging: amqps://<policy>:<key>@<namespace>
        .servicebus.windows.net:5671/?sasl=plain — the destination queue itself is NOT
        part of the URI; it goes in the shovel's separate "dest-address" field.

        -VerifyNone appends '&verify=verify_none', disabling ALL AMQP 1.0 peer
        certificate validation for this connection — the documented fallback for a
        broker whose advanced.config cannot be given the wildcard-hostname-check fix
        (see this script's header comment / docs/ShovelBridge-Architecture.md
        "Security"). Not recommended outside of that specific, narrow case.

        -CaCertFile appends '&cacertfile=<path>', pointing the AMQP 1.0 TLS client at an
        explicit CA bundle on the broker host — the fix (not a fallback; keeps full
        certificate validation) for Erlang/OTP 26+ brokers, which crash-loop with
        '{cacerts, undefined}' unless one is supplied (see header comment / "Security").
        Mutually exclusive with -VerifyNone (verify_none disables all cert validation,
        making an explicit CA bundle moot); the caller enforces this, not this function.
    #>
    param([string] $Namespace, [string] $PolicyName, [string] $Key, [switch] $VerifyNone, [string] $CaCertFile)
    $policyEnc = [Uri]::EscapeDataString($PolicyName)
    $keyEnc    = [Uri]::EscapeDataString($Key)
    $verifySuffix    = if ($VerifyNone) { '&verify=verify_none' } else { '' }
    $caCertSuffix    = if (-not [string]::IsNullOrWhiteSpace($CaCertFile)) { '&cacertfile=' + [Uri]::EscapeDataString($CaCertFile) } else { '' }
    return "amqps://${policyEnc}:${keyEnc}@${Namespace}.servicebus.windows.net:5671/?sasl=plain${verifySuffix}${caCertSuffix}"
}

function Save-ConfigureSummary {
    param(
        [ValidateSet('in-progress', 'completed', 'failed')] [string] $Status = 'in-progress',
        [string] $ErrorMessage
    )
    if ([string]::IsNullOrWhiteSpace($summaryPath)) { return }
    $summary = [ordered]@{
        timestampUtc         = (Get-Date).ToUniversalTime().ToString('o')
        status               = $Status
        error                = $ErrorMessage
        dryRun               = [bool]$DryRun
        rabbitMqManagementUri = $RabbitMqManagementUri
        vhost                = $VHost
        shovelName           = $ShovelName
        sourceQueue          = $SourceQueue
        sourceUri            = (Get-MaskedUri $script:sourceUri)
        serviceBusNamespace  = $ServiceBusNamespace
        serviceBusQueueName  = $ServiceBusQueueName
        serviceBusSasKeyName = $ServiceBusSasKeyName
        destUri              = (Get-MaskedUri $script:destUri)
        destUriVerifyNone    = [bool]$DestUriVerifyNone
        destUriCaCertFile    = $DestUriCaCertFile
        ackMode              = $AckMode
        reconnectDelaySeconds = $ReconnectDelaySeconds
        srcPrefetchCount     = $SrcPrefetchCount
    }
    $tmp = "$summaryPath.tmp"
    $summary | ConvertTo-Json -Depth 6 | Set-Content -Path $tmp -Encoding UTF8
    Move-Item -LiteralPath $tmp -Destination $summaryPath -Force
}

# Test hook: stop here when only the helper functions are wanted (Pester).
if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight'

$RabbitMqManagementUri = Read-Required -Name 'RabbitMqManagementUri' -Current $RabbitMqManagementUri -Hint 'e.g. http://localhost:15672'
$RabbitMqUsername      = Read-Required -Name 'RabbitMqUsername'      -Current $RabbitMqUsername      -Hint 'admin/policymaker user'
$RabbitMqPassword      = Read-RequiredSecure -Name 'RabbitMqPassword' -Current $RabbitMqPassword

if (-not [string]::IsNullOrWhiteSpace($ServiceBusSasKeyName) -and $null -ne $ServiceBusSasKey -and -not [string]::IsNullOrWhiteSpace($ServiceBusResourceGroup)) {
    throw 'Supply either -ServiceBusSasKey or -ServiceBusResourceGroup (to fetch the key live via az), not both.'
}
if ($DestUriVerifyNone -and -not [string]::IsNullOrWhiteSpace($DestUriCaCertFile)) {
    throw 'Supply either -DestUriVerifyNone or -DestUriCaCertFile, not both: verify_none disables all peer certificate validation, making an explicit CA bundle moot. Prefer -DestUriCaCertFile — it keeps full certificate validation.'
}

Write-Step "Probing RabbitMQ management API at $RabbitMqManagementUri"
$overview = Invoke-RabbitMqApi -Method Get -Path '/api/overview' -AllowFail
if (-not $overview) {
    throw "Could not reach the RabbitMQ management API at $RabbitMqManagementUri (check the URI, credentials, and that the management plugin is enabled)."
}
Write-Ok "Connected (RabbitMQ $($overview.rabbitmq_version), management plugin $($overview.management_version))."

# Probe for the shovel plugin. Not all RabbitMQ versions expose enabled_plugins on
# /api/nodes uniformly, so this is a best-effort warning, not a hard gate — the real
# gate is the PUT in Phase 2, which fails clearly if the plugin is missing.
$nodes = Invoke-RabbitMqApi -Method Get -Path '/api/nodes' -AllowFail
$shovelPluginSeen = $false
if ($nodes) {
    foreach ($n in @($nodes)) {
        if ($n.PSObject.Properties.Name -contains 'enabled_plugins' -and ($n.enabled_plugins -contains 'rabbitmq_shovel')) {
            $shovelPluginSeen = $true
        }
    }
}
if ($shovelPluginSeen) {
    Write-Ok 'rabbitmq_shovel plugin is enabled.'
} else {
    Write-Note 'Could not confirm rabbitmq_shovel is enabled from /api/nodes (older brokers omit enabled_plugins here).'
    Write-Note 'If Phase 2 fails with an "unknown component" error, enable it on the broker host: rabbitmq-plugins enable rabbitmq_shovel rabbitmq_shovel_management'
}

# The dest-uri built below always uses Erlang's default TLS peer verification
# (verify_peer, no '?verify=' override) — which requires the broker's advanced.config
# to include the wildcard-aware amqp10_client hostname-check match_fun documented in
# this script's header comment. There is no Management HTTP API endpoint that can
# confirm advanced.config's contents remotely, so this can only ever be a reminder,
# not a probe: if Phase 2's shovel never reaches 'running' with reason "failed to
# connect to destination" even though the SAS key/queue are confirmed correct, this is
# the first thing to check on the broker host.
Write-Note "Reminder: the destination dest-uri below relies on the broker's advanced.config having the amqp10_client wildcard-hostname-check fix (see this script's header comment) — without it, the shovel will fail with '`"failed to connect to destination`"' against any real Azure Service Bus namespace."

# ════════════════════════════════════════════════════════════════════════════
# Phase 0.5 — PLAN  (resolve every decision; no broker/cloud change yet)
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0.5  Plan (resolve all settings before any change)'

$SourceQueue         = Read-Required -Name 'SourceQueue'         -Current $SourceQueue         -Hint 'existing RabbitMQ source queue name'
$SourceHost          = Read-Required -Name 'SourceHost'          -Current $SourceHost          -Hint 'AMQP 0.9.1 host of the source broker'
if (-not $SourceUsername) { $SourceUsername = $RabbitMqUsername }
if ($null -eq $SourcePassword -or $SourcePassword.Length -eq 0) { $SourcePassword = $RabbitMqPassword }

$ServiceBusNamespace = Read-Required -Name 'ServiceBusNamespace' -Current $ServiceBusNamespace -Hint 'destination Service Bus namespace'

$sasKeyPlain = $null
if ($null -ne $ServiceBusSasKey -and $ServiceBusSasKey.Length -gt 0) {
    $sasKeyPlain = ConvertFrom-SecureStringPlain $ServiceBusSasKey
} elseif (-not [string]::IsNullOrWhiteSpace($ServiceBusResourceGroup)) {
    if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
        throw 'Azure CLI (az) not found, but -ServiceBusResourceGroup was supplied to fetch the SAS key live. Install az, or pass -ServiceBusSasKey directly.'
    }
    Write-Step "Fetching '$ServiceBusSasKeyName' primary key via az (namespace '$ServiceBusNamespace', queue '$ServiceBusQueueName')"
    if ($DryRun) {
        $sasKeyPlain = '<dryrun-sas-key>'
        Write-Host "      [DRYRUN] az servicebus queue authorization-rule keys list --resource-group $ServiceBusResourceGroup --namespace-name $ServiceBusNamespace --queue-name $ServiceBusQueueName --name $ServiceBusSasKeyName --query primaryKey -o tsv" -ForegroundColor DarkGray
    } else {
        $sasKeyPlain = (az servicebus queue authorization-rule keys list --resource-group $ServiceBusResourceGroup --namespace-name $ServiceBusNamespace --queue-name $ServiceBusQueueName --name $ServiceBusSasKeyName --query primaryKey -o tsv 2>&1)
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sasKeyPlain)) {
            throw "Failed to fetch the SAS key via az: $sasKeyPlain"
        }
    }
    Write-Ok 'SAS key fetched (held in memory only; never written to disk).'
} else {
    $ServiceBusSasKey = Read-RequiredSecure -Name 'ServiceBusSasKey' -Current $ServiceBusSasKey -Hint "primary key of the '$ServiceBusSasKeyName' rule, or supply -ServiceBusResourceGroup to fetch it live"
    $sasKeyPlain = ConvertFrom-SecureStringPlain $ServiceBusSasKey
}

$sourcePasswordPlain = ConvertFrom-SecureStringPlain $SourcePassword
$script:sourceUri = Format-Amqp091Uri -HostName $SourceHost -Port $SourcePort -User $SourceUsername -Password $sourcePasswordPlain -VHostName $VHost
$script:destUri   = Format-ServiceBusAmqp10Uri -Namespace $ServiceBusNamespace -PolicyName $ServiceBusSasKeyName -Key $sasKeyPlain -VerifyNone:$DestUriVerifyNone -CaCertFile $DestUriCaCertFile

if ($DestUriVerifyNone) {
    Write-Note 'DestUriVerifyNone is set: the dest-uri disables ALL AMQP 1.0 peer certificate validation (not just the hostname check).'
    Write-Note 'Only use this to diagnose/unblock the advanced.config wildcard-hostname-check issue on a broker you do not control — never on a broker you do control, and never for production use.'
}
if (-not [string]::IsNullOrWhiteSpace($DestUriCaCertFile)) {
    Write-Note "DestUriCaCertFile is set: the dest-uri points the AMQP 1.0 TLS client at '$DestUriCaCertFile' on the broker host (Erlang/OTP 26+ '{cacerts, undefined}' fix)."
}

$shovelDefinition = [ordered]@{
    'src-protocol'      = 'amqp091'
    'src-uri'           = $script:sourceUri
    'src-queue'         = $SourceQueue
    'src-prefetch-count' = $SrcPrefetchCount
    'dest-protocol'     = 'amqp10'
    'dest-uri'          = $script:destUri
    'dest-address'      = $ServiceBusQueueName
    'ack-mode'          = $AckMode
    'reconnect-delay'   = $ReconnectDelaySeconds
}

# ── Settings summary + single confirmation (all secrets masked) ────────────────
Write-Host ''
Write-Host '  ── Resolved shovel settings ────────────────────────────────────────' -ForegroundColor White
Write-Host ("    {0,-24}: {1}" -f 'RabbitMqManagementUri', $RabbitMqManagementUri)
Write-Host ("    {0,-24}: {1}" -f 'VHost', $VHost)
Write-Host ("    {0,-24}: {1}" -f 'ShovelName', $ShovelName)
Write-Host ("    {0,-24}: {1}" -f 'SourceQueue', $SourceQueue)
Write-Host ("    {0,-24}: {1}" -f 'Source URI', (Get-MaskedUri $script:sourceUri))
Write-Host ("    {0,-24}: {1}" -f 'ServiceBusNamespace', $ServiceBusNamespace)
Write-Host ("    {0,-24}: {1}" -f 'ServiceBusQueueName', $ServiceBusQueueName)
Write-Host ("    {0,-24}: {1}" -f 'ServiceBusSasKeyName', $ServiceBusSasKeyName)
Write-Host ("    {0,-24}: {1}" -f 'Destination URI', (Get-MaskedUri $script:destUri))
Write-Host ("    {0,-24}: {1}" -f 'AckMode', $AckMode)
Write-Host ("    {0,-24}: {1}" -f 'ReconnectDelaySeconds', $ReconnectDelaySeconds)
Write-Host ("    {0,-24}: {1}" -f 'SrcPrefetchCount', $SrcPrefetchCount)
Write-Host ("    {0,-24}: {1}" -f 'DryRun', $DryRun)
Write-Host ("    {0,-24}: {1}" -f 'DestUriVerifyNone', $DestUriVerifyNone)
Write-Host ("    {0,-24}: {1}" -f 'DestUriCaCertFile', $DestUriCaCertFile)
Write-Host ''
Write-Note 'Least privilege reminder: ServiceBusSasKeyName should be a Send-only rule'
Write-Note '(e.g. the "shovel-send" rule created by Deploy-WwExecutionServiceBusWorker.ps1) —'
Write-Note 'the Shovel never needs Listen or Manage rights on the destination queue.'
Write-Host ''

if (-not (Confirm-Yes 'Proceed with configuring this shovel?' $true)) {
    Write-Note 'Aborted by user.'
    return
}

# ════════════════════════════════════════════════════════════════════════════
# Begin work — transcript log + summary
# ════════════════════════════════════════════════════════════════════════════

if (-not $LogDir) { $LogDir = Join-Path ([System.IO.Path]::GetTempPath()) 'wwexecution-shovel-logs' }
if (-not (Test-Path -LiteralPath $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$runStamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$logInfix     = if ($DryRun) { 'dryrun.' } else { '' }
$logFile      = Join-Path $LogDir "configure-rabbitmq-shovel-$runStamp.${logInfix}log"
$summaryPath  = Join-Path $LogDir "configure-rabbitmq-shovel-$runStamp.${logInfix}summary.json"
$transcriptOn = $false
try { Start-Transcript -Path $logFile -Append | Out-Null; $transcriptOn = $true; Write-Ok "Logging to $logFile" }
catch { Write-Note "Transcript not started (an outer transcript may be active): $($_.Exception.Message)" }

Save-ConfigureSummary -Status 'in-progress'

try {
    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Verify the source queue exists (fail fast with a clear message)
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Verify source queue'

    $vhostForApi = if ($VHost -eq '/') { '%2f' } else { [Uri]::EscapeDataString($VHost) }
    $queueCheck = Invoke-RabbitMqApi -Method Get -Path "/api/queues/$vhostForApi/$([Uri]::EscapeDataString($SourceQueue))" -AllowFail
    if (-not $queueCheck) {
        throw "Source queue '$SourceQueue' was not found in vhost '$VHost'. Create it (or point -SourceQueue at an existing queue) before configuring the shovel."
    }
    Write-Ok "Source queue '$SourceQueue' exists (vhost '$VHost')."

    # ════════════════════════════════════════════════════════════════════════
    # Phase 2 — Configure the dynamic shovel parameter
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Configure dynamic shovel'

    $body = @{ value = $shovelDefinition }
    Write-Step "PUT /api/parameters/shovel/$VHost/$ShovelName"
    try {
        Invoke-RabbitMqApi -Method Put -Path "/api/parameters/shovel/$vhostForApi/$([Uri]::EscapeDataString($ShovelName))" -Body $body -Mutating | Out-Null
    } catch {
        if ($_.Exception.Message -match '(?i)no such (component|vhost)|unknown.*component|not.*enabled') {
            throw "$($_.Exception.Message)`nThis usually means the rabbitmq_shovel plugin is not enabled on the broker. Run on the broker host: rabbitmq-plugins enable rabbitmq_shovel rabbitmq_shovel_management"
        }
        throw
    }
    Write-Ok "Shovel parameter '$ShovelName' applied."

    # ════════════════════════════════════════════════════════════════════════
    # Phase 3 — Verify the shovel is running
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Verify shovel status'

    if ($DryRun) {
        Write-Note 'Skipped in dry-run (no shovel was actually created).'
    } else {
        $state = $null
        for ($i = 1; $i -le 10; $i++) {
            $shovels = Invoke-RabbitMqApi -Method Get -Path "/api/shovels/$vhostForApi" -AllowFail
            $mine = @($shovels) | Where-Object { $_.name -eq $ShovelName }
            if ($mine) { $state = $mine[0].state; if ($state -eq 'running') { break } }
            Start-Sleep -Seconds 2
        }
        if ($state -eq 'running') {
            Write-Ok "Shovel '$ShovelName' is running."
        } else {
            Write-Note "Shovel '$ShovelName' reported state '$state' (expected 'running') after 20s — check RabbitMQ logs / the management UI's Admin > Shovel Status page."
        }
    }

    Save-ConfigureSummary -Status 'completed'
    Write-Phase ($DryRun ? 'Dry-run complete (no broker changes made)' : 'Shovel configured')
    Write-Host "  Shovel   : $ShovelName ($VHost)" -ForegroundColor White
    Write-Host "  Source   : $SourceQueue @ $SourceHost`:$SourcePort" -ForegroundColor White
    Write-Host "  Dest     : $ServiceBusQueueName @ $ServiceBusNamespace.servicebus.windows.net" -ForegroundColor White
    Write-Host "  Summary  : $summaryPath" -ForegroundColor White
    Write-Host ''
}
catch {
    $errMsg = $_.Exception.Message
    Write-Note "Configuration FAILED: $errMsg"
    try { Save-ConfigureSummary -Status 'failed' -ErrorMessage $errMsg } catch { }
    throw
}
finally {
    if ($transcriptOn) { try { Stop-Transcript | Out-Null } catch { } }
    $sasKeyPlain = $null
    $sourcePasswordPlain = $null
}
