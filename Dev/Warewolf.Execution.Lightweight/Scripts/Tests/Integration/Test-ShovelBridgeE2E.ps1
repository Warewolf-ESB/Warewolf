#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end integration test for the RabbitMQ -> Shovel -> Azure Service Bus
    bridge (see docs/ShovelBridge-Architecture.md) — the second of the two "known
    risks" closed as part of promoting the Service Bus worker to a first-class
    Lightweight Execution Engine component.

.DESCRIPTION
    Proves, against REAL containers (not mocks), that a message published to a
    RabbitMQ queue is actually forwarded by a RabbitMQ Shovel to a Service Bus
    queue, by:

      1. Starting a RabbitMQ container (management plugin + shovel plugin enabled)
         and declaring the source queue.
      2. Standing up the destination — either the local Azure Service Bus
         emulator (+ its Azure SQL Edge metadata-store dependency), or an
         already-provisioned real Service Bus namespace/queue passed in by the
         caller (see -DestinationMode).
      3. Configuring the shovel by dot-sourcing Configure-RabbitMqShovel.ps1
         -LoadFunctionsOnly and reusing its Invoke-RabbitMqApi helper — the
         production script's own Format-ServiceBusAmqp10Uri (Azure-namespace
         shaped, amqps://...:5671) is left completely untouched; this script
         builds its own destination URI appropriate to -DestinationMode.
      4. Running the Warewolf.Execution.ServiceBusWorker.E2EHarness console app,
         which publishes a uniquely-marked message to the RabbitMQ source queue
         (via the RabbitMQ Management HTTP API) and polls the destination Service
         Bus queue (via the Azure.Messaging.ServiceBus SDK) for that same marker,
         asserting the bridge actually delivered it.

    Follows the same plain `docker run`/`docker exec` orchestration style already
    used elsewhere in this repo (see TestRun.ps1's Start-HostRabbitMQServer) —
    deliberately NOT docker-compose or the Testcontainers .NET library, neither of
    which are used anywhere else in this codebase.

    All containers/networks created by this script are named with a per-run
    suffix (-RunId) so concurrent runs (e.g. a local dev run alongside a CI run)
    never collide, and are torn down in a `finally` block unless -SkipTeardown is
    passed (useful when iterating locally).

.PARAMETER DestinationMode
    'Emulator' (default): this script stands up the Azure Service Bus emulator
    and its Azure SQL Edge dependency itself — no live Azure dependency. Used by
    the dedicated CI job in pipeline.yml.
    'ExternalServiceBus': the caller has already provisioned a real Service Bus
    namespace/queue (e.g. via az CLI) and supplies -ExternalShovelDestUri /
    -ExternalServiceBusConnectionString directly; this script only starts
    RabbitMQ, never the emulator/sqledge. Used by the "Test on Azure" stage job
    in pipeline-CLOUD.yml.
.PARAMETER ExternalShovelDestUri
    REQUIRED when -DestinationMode ExternalServiceBus. The full AMQP 1.0
    destination URI for the shovel (amqps://<send-only-policy>:<key>@<namespace>
    .servicebus.windows.net:5671/?sasl=plain) — build it with
    Configure-RabbitMqShovel.ps1's own Format-ServiceBusAmqp10Uri so the exact
    same URI shape used in production is exercised here.

    -RabbitMqMode Container already bind-mounts the amqp10_client
    customize_hostname_check fix (see docs/ShovelBridge-Architecture.md
    "Security") into the broker's advanced.config, so the Shovel's TLS
    handshake against a real Service Bus namespace's wildcard SAN succeeds
    without any caller action. A SEPARATE, additional prerequisite on
    Erlang/OTP 26+ images (including the default rabbitmq:3-management image)
    is still the caller's responsibility: append
    '&cacertfile=/etc/ssl/certs/ca-certificates.crt' to this URI yourself (as
    Format-ServiceBusAmqp10Uri's own -CaCertFile does) or the Shovel
    crash-loops with '{cacerts, undefined}' — this script cannot add it for
    you since it doesn't know which policy/key the caller embedded in the URI.
.PARAMETER ExternalServiceBusConnectionString
    REQUIRED when -DestinationMode ExternalServiceBus. A full Service Bus SAS
    connection string with LISTEN rights on the destination queue (a different,
    least-privilege rule than the Send-only one used for -ExternalShovelDestUri)
    — this is what the harness uses to verify the message actually arrived.
.PARAMETER RabbitMqMode
    'Container' (default): this script starts its own disposable RabbitMQ
    container (-RabbitMqImage) on a per-run docker network, as described above.
    'External': the caller has already got a reachable RabbitMQ broker (with the
    management + shovel + shovel_management plugins enabled) and supplies
    -ExternalRabbitMqManagementUri / -ExternalRabbitMqUsername /
    -ExternalRabbitMqPassword directly; this script never touches docker for the
    RabbitMQ side (no container, and no docker network unless -DestinationMode
    Emulator still needs one for the SQL Edge/Service Bus emulator containers).
    Useful on hosted build agents that cannot pull rabbitmq:3-management (a
    Linux-only image) because their Docker daemon only supports Windows
    containers.
.PARAMETER ExternalRabbitMqManagementUri
    REQUIRED when -RabbitMqMode External. Base URI of the broker's management
    HTTP API, e.g. https://rabbitmq.warewolf.online (no trailing /api/... path).
.PARAMETER ExternalRabbitMqUsername
    REQUIRED when -RabbitMqMode External. Username for the management API — MUST
    already have configure/write/read permissions on the target vhost (queue
    declare, shovel parameter, and the default-exchange publish the harness uses).
.PARAMETER ExternalRabbitMqPassword
    REQUIRED when -RabbitMqMode External. Password for -ExternalRabbitMqUsername.
.PARAMETER ExternalRabbitMqVHost
    Only used when -RabbitMqMode External. The vhost the source queue/shovel are
    declared on. Defaults to '/' (RabbitMQ's own default vhost).
.PARAMETER SkipTeardown
    Leaves all containers/network running after the test (pass/fail) for local
    debugging. Never set this in CI.
.PARAMETER MessageCount
    Number of distinct messages to publish and verify. Default: 1 (the original single-message
    behaviour). Set this higher (e.g. 1000) to load-test the pipeline at scale. Applies to
    BOTH modes:
      * Without -VerifyWorkflowExecution: publishes N uniquely-marked messages to the RabbitMQ
        source queue and waits for all N to arrive on the destination Service Bus queue via
        the shovel — proves the bridge's own throughput, not workflow execution.
      * With -VerifyWorkflowExecution: publishes N distinct workflow-trigger messages
        (correlationId '<CorrelationId>-000000' .. '-{N-1}') and requires ALL N to report a
        Succeeded result from the target engine — proves the full end-to-end pipeline
        (RabbitMQ -> Shovel -> Service Bus -> workflow execution) at that volume.
    Remember to raise -HarnessTimeoutSeconds / -ResultTimeoutSeconds accordingly for larger
    counts.
.PARAMETER PublishConcurrency
    How many of the -MessageCount publishes are in flight at once (passed through to the
    harness's own --publish-concurrency). Default: 1 — the original, fully sequential
    behaviour (one blocking RabbitMQ Management API HTTP call at a time). At load-test volumes
    (e.g. 1000 messages) this sequential publish loop is the dominant cost, NOT the Shovel's
    own bridging throughput — raise this (e.g. 20, matching the harness's own bounded-
    concurrency result polling) to speed up the publish phase specifically. See -ShovelPrefetchCount
    for the Shovel's own, separate throughput knob.
.PARAMETER ShovelPrefetchCount
    The Shovel's 'src-prefetch-count': how many unacknowledged messages it keeps in flight
    between the RabbitMQ source queue and the Service Bus destination (ack-mode is
    'on-confirm', so this bounds how many messages can be pipelined awaiting a destination
    confirm). Default: 5 (the shovel's original hardcoded value). Raising this can increase
    the Shovel's own bridging throughput — most useful once -PublishConcurrency > 1, since
    otherwise the sequential publish loop is the bottleneck and the Shovel is rarely fed faster
    than one message at a time regardless of this setting.
.PARAMETER VerifyWorkflowExecution
    Additive, opt-in "full pipeline" mode. Without this switch, the test only proves
    RabbitMQ -> Shovel -> Service Bus message ARRIVAL (see docs/ShovelBridge-Architecture.md)
    — it never proves a workflow actually executed. With this switch, Phase 4 instead
    publishes the real ServiceBusWorkflowMessage contract (workflow/inputs/correlationId,
    plus the caller's bearer token as a message header the Shovel bridges to a Service Bus
    application property) and polls the target Lightweight engine's own
    GET /secure/servicebus-result/{correlationId} endpoint until it reports a terminal
    outcome — proving the message was actually consumed and executed by
    ServiceBusWorkflowTriggerFunction (the in-process "Model A" secure trigger; see
    docs/ServiceBusSecureTrigger-Architecture.md), not just that it reached the queue.
    Combine with -MessageCount > 1 to load-test the FULL pipeline end-to-end (bridge +
    workflow execution), requiring all N executions to succeed, not just N message arrivals.
    REQUIRES -DestinationMode ExternalServiceBus (a live engine must already be listening
    on -DestinationQueueName via Managed Identity — the local emulator has no engine
    attached to it) and -WorkflowName / -MessageAuthToken / -EngineBaseUrl. Deliberately does
    NOT also run the Service Bus receiver-based arrival check in this mode: a second
    receiver on the same queue would compete with the engine's own subscription and could
    steal the message before the engine processes it.
.PARAMETER WorkflowName
    REQUIRED when -VerifyWorkflowExecution. The workflow to execute, e.g. "RabbitProcess" (see
    Resources/rabbit/RabbitProcess.bite — a dedicated shovel-bridge test workflow, replacing
    the generic "Hello World" smoke-test workflow previously used as the example here. Its DB
    activities bind to SourceId b9184f70-… (the shared NewSqlServerSource the pipeline
    downloads), NOT the unreferenced "NewSqlServerSource (Local Backup)" .bite sitting beside
    it, whose connection string is DPAPI-encrypted under its author's account) — matched against the
    target engine's secure.config exactly as an HTTP /secure/{workflow} path segment would be.
.PARAMETER WorkflowInputsJson
    Only used when -VerifyWorkflowExecution. Optional JSON object of string inputs, e.g.
    '{"message":"FromRabbitMq"}' (RabbitProcess's own single input — see its DataList). Passed
    through to the workflow exactly like HTTP query-string inputs are today.

    Supports one placeholder, '{correlationId}', which the harness expands per message — e.g.
    '{"message":"loadtest-{correlationId}"}'. USE IT FOR ANY -MessageCount > 1 RUN: without it
    every message carries a byte-identical inputs map, and RabbitProcess hashes the message
    content into an EXCLUSIVE sp_getapplock in dbo.usp_jobs1_LogStart (15s timeout). Identical
    bodies therefore serialise the entire run behind a single lock, and are recorded as N retry
    attempts of ONE job instead of N distinct jobs. The harness prints a warning if you omit it
    on a multi-message run.
.PARAMETER CorrelationId
    Only used when -VerifyWorkflowExecution. Caller-supplied idempotency/polling key. When
    -MessageCount is 1 (default), this is used verbatim and, when omitted, a fresh GUID is
    generated and printed so it can be used to poll /secure/servicebus-result/{correlationId}
    independently if this script's own polling times out. When -MessageCount > 1, this value
    (or the generated GUID) is instead used as a PREFIX: N correlationIds are derived as
    '<CorrelationId>-000000' .. '<CorrelationId>-{N-1}' (fixed-width, zero-padded).
.PARAMETER MessageAuthToken
    REQUIRED when -VerifyWorkflowExecution. A valid Entra bearer token (audience =
    WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE on the target engine) for a caller authorized to run
    -WorkflowName. Embedded as the message's Authorization application property — this is
    the CALLER's own delegated identity, not a system/shared credential (see
    docs/ServiceBusSecureTrigger-Architecture.md "Two trust boundaries").
.PARAMETER Jti
    Only used when -VerifyWorkflowExecution. Optional — mirrors -MessageAuthToken's own
    `jti` claim as a message header for a cheaper replay-cache lookup on the engine side.
    Falls back to the claim inside the token itself when omitted.
.PARAMETER EngineBaseUrl
    REQUIRED when -VerifyWorkflowExecution. Base URL of the Lightweight engine Function App
    that is listening on -DestinationQueueName, e.g.
    https://warewolfserver-uat.azurewebsites.net. Used to poll
    GET /secure/servicebus-result/{correlationId}.
.PARAMETER ResultPollAuthToken
    Only used when -VerifyWorkflowExecution. Bearer token used to call
    GET /secure/servicebus-result/{correlationId} (protected by the ordinary HTTP
    EasyAuth/claims/policy pipeline, requiring WorkflowPermission.View on -WorkflowName).
    Defaults to -MessageAuthToken when omitted (the same caller polling their own result).
.PARAMETER ResultTimeoutSeconds
    Only used when -VerifyWorkflowExecution. How long to poll for ALL correlationIds (1, or N
    when -MessageCount > 1) to reach a terminal result before failing. Default: 90. Raise this
    substantially for large -MessageCount values (e.g. 1000) since each execution takes real
    engine processing time and this timeout is shared across all of them.
#>
[CmdletBinding()]
param(
    [ValidateSet('Emulator', 'ExternalServiceBus')]
    [string] $DestinationMode = 'Emulator',

    [ValidateSet('Container', 'External')]
    [string] $RabbitMqMode = 'Container',

    # RabbitMqMode Container only
    [string] $RabbitMqImage = 'rabbitmq:3-management',
    [int]    $RabbitMqAmqpHostPort = 25672,
    [int]    $RabbitMqManagementHostPort = 25673,

    # RabbitMqMode External only (all REQUIRED in that mode)
    [string] $ExternalRabbitMqManagementUri,
    [string] $ExternalRabbitMqUsername,
    [securestring] $ExternalRabbitMqPassword,
    [string] $ExternalRabbitMqVHost = '/',

    # Common to both RabbitMqMode values
    [string] $SourceQueueName = 'wwexecution-shovel-source-e2e',
    [string] $ShovelName = 'wwexecution-shovel-e2e',

    # Destination (common to both modes)
    [string] $DestinationQueueName = 'wwexecution-queue-e2e',

    # Emulator-mode only
    [string] $SqlEdgeImage = 'mcr.microsoft.com/azure-sql-edge:latest',
    [string] $ServiceBusEmulatorImage = 'mcr.microsoft.com/azure-messaging/servicebus-emulator:latest',
    [securestring] $SqlEdgeSaPassword,

    # ExternalServiceBus-mode only (both REQUIRED in that mode)
    [string] $ExternalShovelDestUri,
    [securestring] $ExternalServiceBusConnectionString,

    [int]    $HarnessTimeoutSeconds = 90,
    [switch] $SkipTeardown,

    # Number of distinct messages to publish and verify; applies to both plain arrival
    # checking and -VerifyWorkflowExecution (see the parameter help above for how the
    # -CorrelationId is used as a prefix when this is > 1).
    [ValidateRange(1, [int]::MaxValue)]
    [int]    $MessageCount = 1,

    # How many of the -MessageCount publishes are in flight at once (passed through as the
    # harness's own --publish-concurrency). Default 1 preserves the original fully sequential
    # behaviour. Sequential HTTP-per-message publishing via the RabbitMQ Management API is the
    # dominant cost at load-test volumes (e.g. 1000 messages) — raise this (e.g. 20, matching
    # the harness's own result-polling concurrency) to speed up the publish phase; the Shovel's
    # own -ShovelPrefetchCount below is a separate, later-stage throughput knob.
    [ValidateRange(1, [int]::MaxValue)]
    [int]    $PublishConcurrency = 1,

    # RabbitMQ Shovel 'src-prefetch-count': how many unacknowledged messages the Shovel keeps
    # in flight between the RabbitMQ source queue and the Service Bus destination (ack-mode is
    # 'on-confirm', so this bounds pipelining while awaiting destination confirms). Default 5
    # matches the shovel's original hardcoded value. Raising this can increase the Shovel's own
    # bridging throughput once publishing (-PublishConcurrency above) is no longer the
    # bottleneck — see docs/ShovelBridge-Architecture.md.
    [ValidateRange(1, [int]::MaxValue)]
    [int]    $ShovelPrefetchCount = 5,

    # ── Full-pipeline "did the workflow actually execute" verification (additive) ──────
    [switch] $VerifyWorkflowExecution,
    [string] $WorkflowName,
    [string] $WorkflowInputsJson,
    [string] $CorrelationId,
    [securestring] $MessageAuthToken,
    [string] $Jti,
    [string] $EngineBaseUrl,
    [securestring] $ResultPollAuthToken,
    [int]    $ResultTimeoutSeconds = 90
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers
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

function ConvertFrom-SecureStringPlain {
    param([securestring] $Value)
    if ($null -eq $Value -or $Value.Length -eq 0) { return '' }
    $bstr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try { return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
    finally { [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
}

function Get-JwtRemainingLifetimeSeconds {
    <#
    Best-effort, client-side pre-flight check of a bearer token's remaining lifetime —
    decodes the UNSIGNED payload segment (base64url) of a 3-part JWT and reads the
    standard 'exp' claim. This is deliberately NOT token validation (no signature check,
    no issuer/audience check - the engine itself does that); it exists purely so this
    script can fail fast in Phase 0 with an actionable message instead of a caller
    discovering, after a 30-minute/1000-message run, that a token minted (or merely
    reused from earlier in an interactive session) before the run started expired
    partway through Phase 4's result-polling loop and turned into a wall of
    "401: Authentication required" HttpErrors indistinguishable at a glance from a real
    authorization problem.

    Returns $null — "unknown, don't block" — when the token isn't a 3-segment JWT (e.g.
    an opaque token) or has no parseable 'exp' claim; callers must treat $null as
    "can't tell", never as "expired".
    #>
    param([Parameter(Mandatory)][string] $Token)

    $raw = if ($Token.StartsWith('Bearer ', [StringComparison]::OrdinalIgnoreCase)) { $Token.Substring(7) } else { $Token }
    $parts = $raw.Trim().Split('.')
    if ($parts.Length -ne 3) { return $null }

    try {
        $payloadSegment = $parts[1].Replace('-', '+').Replace('_', '/')
        switch ($payloadSegment.Length % 4) { 2 { $payloadSegment += '==' } 3 { $payloadSegment += '=' } }
        $payload = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($payloadSegment)) | ConvertFrom-Json
        if (-not $payload.exp) { return $null }
        $expUtc = [DateTimeOffset]::FromUnixTimeSeconds([long]$payload.exp).UtcDateTime
        return [int][Math]::Floor(($expUtc - (Get-Date).ToUniversalTime()).TotalSeconds)
    } catch {
        return $null
    }
}

function New-RandomPassword {
    <# Meets SQL Server's complexity policy: 3 of {upper, lower, digit, symbol}, 8-128 chars. #>
    $bytes = New-Object byte[] 24
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return 'Aa1!' + [Convert]::ToBase64String($bytes).Replace('/', 'x').Replace('+', 'y').Substring(0, 20)
}

function Wait-ForCondition {
    param(
        [Parameter(Mandatory)][scriptblock] $Condition,
        [string] $Description = 'condition',
        [int]    $MaxAttempts = 30,
        [int]    $DelaySeconds = 2
    )
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        if (& $Condition) { return $true }
        Start-Sleep -Seconds $DelaySeconds
    }
    Write-Note "Timed out waiting for: $Description (after $($MaxAttempts * $DelaySeconds)s)"
    return $false
}

function Test-ServiceBusQueueExists {
    <#
        Fast, dependency-free existence probe for a Service Bus queue, using the
        Service Bus HTTP management endpoint (Atom feed) signed with a SAS token
        derived from the connection string's own key — no Azure SDK/az CLI needed.

        Returns $true / $false when the check could be performed, or $null when
        the connection string couldn't be parsed (e.g. it uses a shared access
        signature token directly rather than a key name+key pair) — in which
        case the caller should treat this as "unknown" and not fail the test on
        it, since the ORIGINAL (mode-appropriate) failure signal is still the
        shovel-running wait in Phase 3.
    #>
    param(
        [Parameter(Mandatory)][string] $ConnectionString,
        [Parameter(Mandatory)][string] $QueueName
    )
    $parts = @{}
    foreach ($seg in $ConnectionString.Split(';')) {
        if ([string]::IsNullOrWhiteSpace($seg)) { continue }
        $kv = $seg.Split('=', 2)
        if ($kv.Length -eq 2) { $parts[$kv[0].Trim()] = $kv[1].Trim() }
    }
    if (-not ($parts.ContainsKey('Endpoint') -and $parts.ContainsKey('SharedAccessKeyName') -and $parts.ContainsKey('SharedAccessKey'))) {
        return $null
    }
    $endpoint    = ($parts['Endpoint'] -replace '^sb://', 'https://').TrimEnd('/')
    $resourceUri = "$endpoint/$QueueName"
    $expiry      = [DateTimeOffset]::UtcNow.AddMinutes(5).ToUnixTimeSeconds()
    $toSign      = [Uri]::EscapeDataString($resourceUri) + "`n" + $expiry
    $hmac        = New-Object System.Security.Cryptography.HMACSHA256
    $hmac.Key    = [Text.Encoding]::UTF8.GetBytes($parts['SharedAccessKey'])
    $signature   = [Convert]::ToBase64String($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($toSign)))
    $sasToken    = "SharedAccessSignature sr=$([Uri]::EscapeDataString($resourceUri))&sig=$([Uri]::EscapeDataString($signature))&se=$expiry&skn=$($parts['SharedAccessKeyName'])"
    try {
        $null = Invoke-WebRequest -Uri "${resourceUri}?api-version=2021-05" -Headers @{ Authorization = $sasToken } -Method Get -UseBasicParsing -TimeoutSec 15
        return $true
    } catch {
        $statusCode = $null
        if ($_.Exception.PSObject.Properties.Match('Response').Count -gt 0 -and $_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }
        if ($statusCode -eq 404) { return $false }
        # Any other failure (network/DNS/auth) is inconclusive for "does the queue
        # exist" — surface it as a note but don't claim the queue is missing.
        $hint = ''
        if ($statusCode -eq 401) {
            # A SAS-signed call rejected with 401 here is EXPECTED and benign for this
            # script's Send/Listen-only rules: the entity-management REST surface
            # (GET .../{queue}?api-version=...) requires a SAS rule with the "Manage"
            # claim, which neither shovel-e2e-send (Send-only) nor
            # shovel-e2e-listen/-ExternalServiceBusConnectionString's rule (Listen-only)
            # carry by design (least privilege — see "Security" in
            # docs/ShovelBridge-Architecture.md). Confirmed empirically: both rules 401
            # here even when the namespace's disableLocalAuth is false and a
            # Manage-claim rule (e.g. RootManageSharedAccessKey) succeeds with 200
            # against the same queue. A disableLocalAuth=true drift is a SEPARATE,
            # also-possible cause of a 401 here (and would additionally break the
            # Shovel's own dest-uri connection, since SASL PLAIN has no Azure
            # AD/OAuth fallback) but is not the expected/common cause for the rules
            # this script actually uses — do not assume the queue check's 401 implies
            # disableLocalAuth drift without corroborating it independently.
            $hint = " (Expected/benign for this script's Send/Listen-only SAS rules, which lack the Manage claim required by the entity-management REST API — this does not by itself indicate a problem. A disableLocalAuth=true drift is a separate possible cause; check with 'az servicebus namespace show --query disableLocalAuth' if the Shovel itself also fails to connect. See docs/ShovelBridge-Architecture.md.)"
        }
        Write-Note "Could not verify destination queue existence via Service Bus management API: $($_.Exception.Message)$hint"
        return $null
    }
}

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight'

if ($DestinationMode -eq 'ExternalServiceBus') {
    if ([string]::IsNullOrWhiteSpace($ExternalShovelDestUri)) {
        throw '-ExternalShovelDestUri is required when -DestinationMode ExternalServiceBus.'
    }
    # -ExternalServiceBusConnectionString is normally required — it's how the harness's own
    # Service Bus receiver checks arrival (Phase 4). Skipped only under -VerifyWorkflowExecution,
    # where a second receiver on the same queue would compete with the engine's own
    # subscription and could steal the message before the engine processes it.
    if (-not $VerifyWorkflowExecution -and ($null -eq $ExternalServiceBusConnectionString -or $ExternalServiceBusConnectionString.Length -eq 0)) {
        throw '-ExternalServiceBusConnectionString is required when -DestinationMode ExternalServiceBus (unless -VerifyWorkflowExecution is set).'
    }
}

if ($RabbitMqMode -eq 'External') {
    if ([string]::IsNullOrWhiteSpace($ExternalRabbitMqManagementUri)) {
        throw '-ExternalRabbitMqManagementUri is required when -RabbitMqMode External.'
    }
    if ([string]::IsNullOrWhiteSpace($ExternalRabbitMqUsername)) {
        throw '-ExternalRabbitMqUsername is required when -RabbitMqMode External.'
    }
    if ($null -eq $ExternalRabbitMqPassword -or $ExternalRabbitMqPassword.Length -eq 0) {
        throw '-ExternalRabbitMqPassword is required when -RabbitMqMode External.'
    }
}

if ($VerifyWorkflowExecution) {
    # A live engine must already be listening (Managed Identity) on -DestinationQueueName —
    # the local emulator has no engine attached to it, so this mode only makes sense against
    # a real, already-provisioned Service Bus namespace/queue.
    if ($DestinationMode -ne 'ExternalServiceBus') {
        throw '-VerifyWorkflowExecution requires -DestinationMode ExternalServiceBus (a live Lightweight engine must already be listening on -DestinationQueueName).'
    }
    if ([string]::IsNullOrWhiteSpace($WorkflowName)) {
        throw '-WorkflowName is required when -VerifyWorkflowExecution.'
    }
    if ($null -eq $MessageAuthToken -or $MessageAuthToken.Length -eq 0) {
        throw '-MessageAuthToken is required when -VerifyWorkflowExecution.'
    }
    if ([string]::IsNullOrWhiteSpace($EngineBaseUrl)) {
        throw '-EngineBaseUrl is required when -VerifyWorkflowExecution.'
    }
    if ([string]::IsNullOrWhiteSpace($CorrelationId)) {
        $CorrelationId = [Guid]::NewGuid().ToString('N')
    }
    Write-Ok "VerifyWorkflowExecution is set: Phase 4 will publish workflow '$WorkflowName' ($(if ($MessageCount -eq 1) { "correlationId '$CorrelationId'" } else { "$MessageCount messages, correlationId prefix '$CorrelationId'" })) and poll $EngineBaseUrl for $(if ($MessageCount -eq 1) { 'its' } else { 'their' }) execution result, instead of proving bare message arrival."

    # Fail fast on a stale bearer token rather than discovering it 30 minutes and (at
    # load-test volumes) 1000 messages later as a wall of "401: Authentication required"
    # HttpErrors that look, at a glance, like a real authorization failure rather than an
    # expired token. -ResultTimeoutSeconds bounds Phase 4's OWN polling window; add a fixed
    # buffer for Phases 1-3's setup time (RabbitMQ/shovel/queue provisioning), which elapses
    # BEFORE Phase 4 starts spending the token's remaining lifetime.
    $setupBufferSeconds = 300
    $requiredLifetimeSeconds = $ResultTimeoutSeconds + $setupBufferSeconds
    $tokensToCheck = [ordered]@{ '-MessageAuthToken' = (ConvertFrom-SecureStringPlain $MessageAuthToken) }
    if ($ResultPollAuthToken) {
        $tokensToCheck['-ResultPollAuthToken'] = ConvertFrom-SecureStringPlain $ResultPollAuthToken
    }
    foreach ($tokenName in $tokensToCheck.Keys) {
        $remaining = Get-JwtRemainingLifetimeSeconds -Token $tokensToCheck[$tokenName]
        if ($null -eq $remaining) {
            continue # opaque/non-JWT token or unparseable 'exp' claim - can't tell, don't block
        }
        if ($remaining -le 0) {
            throw "$tokenName has ALREADY EXPIRED ($([Math]::Abs($remaining))s ago). Mint a fresh token immediately before invoking this script - reusing one from earlier in an interactive session (or from a pipeline step run well before Phase 4) is the most common cause. See docs/ShovelBridge-Architecture.md (2026-08-14 entry)."
        }
        if ($remaining -lt $requiredLifetimeSeconds) {
            throw "$tokenName expires in ${remaining}s, less than the ${requiredLifetimeSeconds}s this run may need (-ResultTimeoutSeconds $ResultTimeoutSeconds + a ${setupBufferSeconds}s buffer for Phases 1-3 setup). It WILL expire mid-poll and surface as '401: Authentication required' HttpErrors on whichever correlationIds are still pending at that point - indistinguishable at a glance from a real authorization failure. Mint a fresh token immediately before invoking this script rather than reusing an older one."
        }
        Write-Ok "$tokenName has ${remaining}s remaining - comfortably covers this run (needs ${requiredLifetimeSeconds}s)."
    }
}

# Docker is only needed when something is actually going to be containerized:
# a Container-mode RabbitMQ, and/or the Emulator destination's SQL Edge/Service
# Bus emulator containers. An External RabbitMQ + ExternalServiceBus combination
# needs no docker network/containers at all, and no docker on the agent's PATH -
# this is what lets this test run on a hosted Windows agent, whose Docker daemon
# only supports Windows containers and can't pull the Linux-only images this
# script otherwise uses (rabbitmq:3-management, azure-sql-edge, the Service Bus
# emulator).
$needsDocker = ($RabbitMqMode -eq 'Container') -or ($DestinationMode -eq 'Emulator')

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { throw 'dotnet was not found on PATH.' }
if ($needsDocker) {
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'docker was not found on PATH.' }
    docker version --format '{{.Server.Os}}' 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Docker daemon is not reachable (docker version failed). Is Docker running?' }
    Write-Ok 'Docker and .NET SDK are available.'
} else {
    Write-Ok '.NET SDK is available (docker not required: RabbitMQ and the destination are both externally provisioned).'
}

$repoRoot          = Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')
$configureScript   = Join-Path $PSScriptRoot '..\..\Configure-RabbitMqShovel.ps1'
$harnessProjectDir = Join-Path $repoRoot 'Warewolf.Execution.ServiceBusWorker.E2EHarness'
if (-not (Test-Path -LiteralPath $configureScript)) { throw "Configure-RabbitMqShovel.ps1 not found at $configureScript" }
if (-not (Test-Path -LiteralPath $harnessProjectDir)) { throw "E2E harness project not found at $harnessProjectDir" }

$runId               = [Guid]::NewGuid().ToString('N').Substring(0, 8)
$networkName         = "shovel-e2e-net-$runId"
$rabbitContainerName = "shovel-e2e-rabbitmq-$runId"
$sqlEdgeContainerName = "shovel-e2e-sqledge-$runId"
$emulatorContainerName = "shovel-e2e-sbemulator-$runId"
$configTempFile      = $null
$enabledPluginsTempFile = $null
$advancedConfigTempFile = $null

if ($RabbitMqMode -eq 'External') {
    $rmqUser     = $ExternalRabbitMqUsername
    $rmqPassword = ConvertFrom-SecureStringPlain $ExternalRabbitMqPassword
    $mgmtUri     = $ExternalRabbitMqManagementUri.TrimEnd('/')
    $vhostForApi = if ($ExternalRabbitMqVHost -eq '/') { '%2f' } else { [Uri]::EscapeDataString($ExternalRabbitMqVHost) }
    $rmqVHost    = $ExternalRabbitMqVHost
} else {
    $rmqUser     = 'e2euser'
    $rmqPassword = New-RandomPassword
    $mgmtUri     = "http://localhost:$RabbitMqManagementHostPort"
    $vhostForApi = '%2f'
    $rmqVHost    = '/'
}

Write-Ok "Run ID: $runId (all containers/network are suffixed with this to avoid collisions)"

# Dot-source Configure-RabbitMqShovel.ps1 to reuse Invoke-RabbitMqApi/Format-Amqp091Uri
# without duplicating them. -LoadFunctionsOnly stops it before any of ITS OWN
# broker/cloud changes; the params below merely become the script-scope closures
# Invoke-RabbitMqApi reads (management URI + creds), same pattern its own Pester
# suite already uses.
$rmqPasswordSecure = ConvertTo-SecureString $rmqPassword -AsPlainText -Force
# NOTE: dot-sourcing shares scope with the caller, so ANY parameter of
# Configure-RabbitMqShovel.ps1 that isn't passed explicitly here reverts to
# THAT script's own default in this scope — including -ShovelName (its default
# 'wwexecution-shovel' would silently clobber this script's own $ShovelName
# otherwise, since it's the one parameter name the two scripts share).
. $configureScript -LoadFunctionsOnly `
    -RabbitMqManagementUri $mgmtUri `
    -RabbitMqUsername $rmqUser `
    -RabbitMqPassword $rmqPasswordSecure `
    -ShovelName $ShovelName

$containersStarted = [System.Collections.Generic.List[string]]::new()

$needsDockerNetwork = ($RabbitMqMode -eq 'Container') -or ($DestinationMode -eq 'Emulator')

try {
    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Network + RabbitMQ
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Start RabbitMQ'

    if ($needsDockerNetwork) {
        Write-Step "Creating docker network '$networkName'"
        docker network create $networkName | Out-Null
    }

    # The RabbitMQ container was previously observed crashing very early in its
    # boot ("Error when reading /var/lib/rabbitmq/.erlang.cookie: eacces"). Root
    # cause: the readiness checks below used to poll via `docker exec rabbitmqctl
    # await_startup` / `docker exec rabbitmq-plugins list`, and those CLI tools
    # each spin up their own short-lived Erlang node to talk to the broker over
    # distribution (cookie-based auth) — repeatedly invoking them while the main
    # node was still finishing its own boot raced its cookie-file handling and
    # crashed it. Fixed by polling readiness purely over the management HTTP API
    # instead (see below) — no `docker exec` against the RabbitMQ container is
    # used anywhere in this script any more. The retry loop is kept as a cheap
    # defensive safety net (recreate the container if setup fails for any other
    # reason), not as the primary fix. In -RabbitMqMode External there is no
    # container to (re)create, so the loop just retries the same HTTP readiness
    # checks against the already-running broker.
    $rabbitMaxAttempts = 3
    $rabbitSetupDone = $false
    for ($rabbitAttempt = 1; $rabbitAttempt -le $rabbitMaxAttempts -and -not $rabbitSetupDone; $rabbitAttempt++) {
        try {
            if ($RabbitMqMode -eq 'Container') {
                if ($rabbitAttempt -gt 1) {
                    Write-Note "Retrying RabbitMQ container setup (attempt $rabbitAttempt of $rabbitMaxAttempts)..."
                    docker rm -f $rabbitContainerName 2>&1 | Out-Null
                }

                Write-Step "Starting RabbitMQ container '$rabbitContainerName' ($RabbitMqImage)"
                # RABBITMQ_DEFAULT_USER/_PASS deliberately used instead of the built-in
                # guest/guest account: RabbitMQ hardcodes "guest" as loopback-only, and
                # connections arriving via a docker -p published port are NOT seen as
                # loopback by the container even when the host side is localhost — they
                # come from the bridge network gateway, so guest/guest would be rejected.
                #
                # The shovel plugins are pre-baked into a bind-mounted enabled_plugins
                # file rather than enabled via `rabbitmq-plugins enable` + a restart
                # after the node is already up, so the node only ever boots once.
                $enabledPluginsTempFile = Join-Path ([System.IO.Path]::GetTempPath()) "shovel-e2e-enabled-plugins-$runId"
                # rabbitmq:3-management's own baked-in default (rabbitmq_management,
                # rabbitmq_prometheus) plus the two shovel plugins this test needs.
                '[rabbitmq_management,rabbitmq_prometheus,rabbitmq_shovel,rabbitmq_shovel_management].' |
                    Set-Content -LiteralPath $enabledPluginsTempFile -Encoding ASCII -NoNewline

                # Bind-mount the amqp10_client wildcard-hostname-check fix (see
                # docs/ShovelBridge-Architecture.md "Security") so this container-mode
                # broker behaves the same as the choco-installed native broker used
                # against ExternalServiceBus in CI, which writes this same file. Without
                # it, the Shovel's dest-uri TLS handshake against a real Azure Service Bus
                # namespace always fails with 'hostname_check_failed', because Erlang's
                # default verify_peer hostname check is a literal (non-wildcard-aware)
                # match against Service Bus's '*.servicebus.windows.net' SAN. Harmless to
                # bake in unconditionally (also applies to -DestinationMode Emulator,
                # where it's simply inert): it only customises amqp10_client's own TLS
                # peer-verification match function, keeping full certificate validation.
                $advancedConfigTempFile = Join-Path ([System.IO.Path]::GetTempPath()) "shovel-e2e-advanced-config-$runId"
                @'
[
  {amqp10_client, [
    {ssl_options, [
      {customize_hostname_check, [
        {match_fun, public_key:pkix_verify_hostname_match_fun(https)}
      ]}
    ]}
  ]}
].
'@ | Set-Content -LiteralPath $advancedConfigTempFile -Encoding ASCII -NoNewline

                docker run -d --name $rabbitContainerName --network $networkName `
                    -e "RABBITMQ_DEFAULT_USER=$rmqUser" -e "RABBITMQ_DEFAULT_PASS=$rmqPassword" `
                    -v "${enabledPluginsTempFile}:/etc/rabbitmq/enabled_plugins" `
                    -v "${advancedConfigTempFile}:/etc/rabbitmq/advanced.config" `
                    -p "${RabbitMqAmqpHostPort}:5672" -p "${RabbitMqManagementHostPort}:15672" `
                    $RabbitMqImage | Out-Null
                if ($LASTEXITCODE -ne 0) { throw "docker run failed for RabbitMQ (exit $LASTEXITCODE)." }
                if (-not $containersStarted.Contains($rabbitContainerName)) { $containersStarted.Add($rabbitContainerName) }
            } elseif ($rabbitAttempt -eq 1) {
                Write-Step "Using externally-provisioned RabbitMQ instance (management API: $mgmtUri)"
            }

            # NOTE: readiness is polled purely over the management HTTP API — never via
            # `docker exec rabbitmqctl ...` / `docker exec rabbitmq-plugins ...`. Those CLI
            # tools spin up their own short-lived Erlang node to talk to the broker over
            # distribution (cookie-based auth), and repeatedly invoking them while the main
            # node is still finishing its own boot reliably crashed it in this environment
            # ("Error when reading /var/lib/rabbitmq/.erlang.cookie: eacces") — confirmed by
            # isolated reproduction: the crash occurred with nothing but env vars + a polling
            # loop of `docker exec rabbitmqctl await_startup`, and did NOT occur when the
            # exact same readiness+queue-declare sequence was done purely over HTTP instead.
            #
            # Also note: -AllowFail on Invoke-RabbitMqApi swallows the exception and returns
            # $null instead of throwing, so it must NOT be combined with a try/catch that
            # treats "no exception" as success — that pattern always reports ready on the
            # very first attempt regardless of whether the call actually succeeded. The
            # checks below rely on the *outer* try/catch seeing the real exception instead.
            $rabbitReady = Wait-ForCondition -Description 'RabbitMQ management API' -MaxAttempts 30 -DelaySeconds 2 -Condition {
                try { Invoke-RabbitMqApi -Method Get -Path '/api/overview' | Out-Null; $true } catch { $false }
            }
            if (-not $rabbitReady) { throw 'RabbitMQ management API never became reachable.' }
            Write-Ok 'RabbitMQ management API is reachable.'

            # /api/shovels is served by rabbitmq_shovel_management: a 200 response (empty
            # array is fine — no shovels configured yet) proves both shovel plugins are
            # loaded and running, without ever exec'ing into the container.
            $pluginsRunning = Wait-ForCondition -Description 'rabbitmq_shovel plugin running' -MaxAttempts 15 -DelaySeconds 2 -Condition {
                try { Invoke-RabbitMqApi -Method Get -Path '/api/shovels' | Out-Null; $true } catch { $false }
            }
            if (-not $pluginsRunning) { throw 'rabbitmq_shovel did not report as running (pre-baked into enabled_plugins at container start, or already enabled on the external broker).' }
            Write-Ok 'Shovel plugins enabled and running.'

            Write-Step "Declaring source queue '$SourceQueueName'"
            $queueDeclared = Wait-ForCondition -Description "source queue '$SourceQueueName' declared" -MaxAttempts 5 -DelaySeconds 3 -Condition {
                try { Invoke-RabbitMqApi -Method Put -Path "/api/queues/$vhostForApi/$([Uri]::EscapeDataString($SourceQueueName))" -Body @{ durable = $true } -Mutating | Out-Null; $true } catch { $false }
            }
            if (-not $queueDeclared) { throw "Failed to declare source queue '$SourceQueueName' after retries." }
            Write-Ok "Source queue '$SourceQueueName' declared."

            $rabbitSetupDone = $true
        } catch {
            if ($RabbitMqMode -eq 'Container') {
                $containerStatus = docker inspect $rabbitContainerName --format '{{.State.Status}}' 2>&1
                Write-Note "RabbitMQ setup attempt $rabbitAttempt failed: $($_.Exception.Message) (container status: $containerStatus)"
            } else {
                Write-Note "RabbitMQ setup attempt $rabbitAttempt failed: $($_.Exception.Message)"
            }
            if ($rabbitAttempt -ge $rabbitMaxAttempts) { throw }
        }
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 2 — Destination
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase "Phase 2  Prepare destination ($DestinationMode)"

    if ($DestinationMode -eq 'Emulator') {
        if ($null -eq $SqlEdgeSaPassword -or $SqlEdgeSaPassword.Length -eq 0) {
            $sqlEdgePasswordPlain = New-RandomPassword
        } else {
            $sqlEdgePasswordPlain = ConvertFrom-SecureStringPlain $SqlEdgeSaPassword
        }

        Write-Step "Starting Azure SQL Edge container '$sqlEdgeContainerName' (Service Bus emulator metadata store)"
        docker run -d --name $sqlEdgeContainerName --network $networkName `
            -e 'ACCEPT_EULA=Y' -e "MSSQL_SA_PASSWORD=$sqlEdgePasswordPlain" `
            $SqlEdgeImage | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "docker run failed for Azure SQL Edge (exit $LASTEXITCODE)." }
        $containersStarted.Add($sqlEdgeContainerName)

        $sqlReady = Wait-ForCondition -Description 'Azure SQL Edge' -MaxAttempts 30 -DelaySeconds 2 -Condition {
            docker exec $sqlEdgeContainerName /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $sqlEdgePasswordPlain -Q 'SELECT 1' 2>&1 | Out-Null
            $LASTEXITCODE -eq 0
        }
        if (-not $sqlReady) { throw 'Azure SQL Edge never became ready.' }
        Write-Ok 'Azure SQL Edge is ready.'

        $configJson = @{
            UserConfig = @{
                Namespaces = @(
                    @{
                        Name   = 'sbemulatorns'
                        Queues = @(
                            @{
                                Name       = $DestinationQueueName
                                Properties = @{
                                    DeadLetteringOnMessageExpiration        = $false
                                    DefaultMessageTimeToLive                = 'PT1H'
                                    DuplicateDetectionHistoryTimeWindow     = 'PT20S'
                                    ForwardDeadLetteredMessagesTo           = ''
                                    ForwardTo                               = ''
                                    LockDuration                            = 'PT1M'
                                    MaxDeliveryCount                        = 10
                                    RequiresDuplicateDetection               = $false
                                    RequiresSession                          = $false
                                }
                            }
                        )
                    }
                )
                Logging = @{ Type = 'File' }
            }
        }
        $configTempFile = Join-Path ([System.IO.Path]::GetTempPath()) "shovel-e2e-config-$runId.json"
        $configJson | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $configTempFile -Encoding UTF8

        Write-Step "Starting Service Bus emulator container '$emulatorContainerName' (queue '$DestinationQueueName')"
        docker run -d --name $emulatorContainerName --network $networkName `
            -e 'ACCEPT_EULA=Y' -e "SQL_SERVER=$sqlEdgeContainerName" -e "MSSQL_SA_PASSWORD=$sqlEdgePasswordPlain" -e 'EMULATOR_HTTP_PORT=5300' `
            -p '5672:5672' -p '5300:5300' `
            -v "${configTempFile}:/ServiceBus_Emulator/ConfigFiles/Config.json" `
            $ServiceBusEmulatorImage | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "docker run failed for the Service Bus emulator (exit $LASTEXITCODE)." }
        $containersStarted.Add($emulatorContainerName)

        $emulatorReady = Wait-ForCondition -Description 'Service Bus emulator AMQP listener' -MaxAttempts 30 -DelaySeconds 2 -Condition {
            try {
                $client = New-Object System.Net.Sockets.TcpClient
                $client.Connect('localhost', 5672)
                $client.Close()
                $true
            } catch { $false }
        }
        if (-not $emulatorReady) { throw 'Service Bus emulator never opened its AMQP port (5672).' }
        # The emulator's schema/entity initialization against SQL Edge can still be
        # finishing for a few seconds after the AMQP port first opens.
        Start-Sleep -Seconds 10
        Write-Ok 'Service Bus emulator is ready.'

        $shovelDestUri = "amqp://RootManageSharedAccessKey:SAS_KEY_VALUE@${emulatorContainerName}:5672/?sasl=plain"
        $serviceBusConnectionStringPlain = 'Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;'
    } else {
        Write-Step 'Using externally-provisioned Service Bus namespace/queue (ExternalServiceBus mode)'
        $shovelDestUri = $ExternalShovelDestUri

        if ($VerifyWorkflowExecution) {
            # No harness-owned receiver in this mode (see -VerifyWorkflowExecution help) — the
            # queue-exists probe below needs a Listen-capable connection string, which we
            # deliberately don't require here. Verification happens entirely via the engine's
            # own /secure/servicebus-result endpoint in Phase 4.
            $serviceBusConnectionStringPlain = $null
            Write-Note "Skipping the destination queue existence probe (-VerifyWorkflowExecution: no Listen connection string is used in this mode — the target engine's own Managed Identity subscription is the only consumer)."
        } else {
            $serviceBusConnectionStringPlain = ConvertFrom-SecureStringPlain $ExternalServiceBusConnectionString

            # Fail fast with an actionable message rather than burning the full Phase 3
            # shovel-running wait (30s) only to time out with no indication of WHY: a
            # missing destination queue is a common setup mistake in this mode (Phase 2
            # here never creates the queue — it must already exist on the namespace).
            $queueExists = Test-ServiceBusQueueExists -ConnectionString $serviceBusConnectionStringPlain -QueueName $DestinationQueueName
            if ($queueExists -eq $false) {
                throw "Destination queue '$DestinationQueueName' does not exist on the external Service Bus namespace. Create it (e.g. via 'az servicebus queue create') before running this test — see docs/ShovelBridge-Architecture.md."
            } elseif ($queueExists -eq $true) {
                Write-Ok "Destination queue '$DestinationQueueName' confirmed to exist on the external namespace."
            } else {
                # $queueExists is $null for two distinct reasons: the connection string
                # wasn't in key-name/key form (nothing more to say), or the management API
                # call itself failed (network/DNS/auth) — Test-ServiceBusQueueExists already
                # wrote a specific Write-Note with the real reason (and an actionable hint
                # for the common 401/disableLocalAuth case) for the latter, so avoid
                # repeating a misleading blanket "not in key-name/key form" claim here.
                Write-Note "Could not confirm destination queue '$DestinationQueueName' exists — assuming it exists (see note above, if any, for why the check was inconclusive)."
            }
        }
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 3 — Configure the shovel
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Configure shovel'

    # src-uri targets the RabbitMQ node's OWN internal AMQP listener (port 5672
    # inside its own container) — the shovel plugin runs inside that same node,
    # not on the host, so this is unrelated to $RabbitMqAmqpHostPort.
    $sourceUri = Format-Amqp091Uri -HostName 'localhost' -Port 5672 -User $rmqUser -Password $rmqPassword -VHostName $rmqVHost
    $shovelDefinition = [ordered]@{
        'src-protocol'       = 'amqp091'
        'src-uri'            = $sourceUri
        'src-queue'          = $SourceQueueName
        'src-prefetch-count' = $ShovelPrefetchCount
        'dest-protocol'      = 'amqp10'
        'dest-uri'           = $shovelDestUri
        'dest-address'       = $DestinationQueueName
        'ack-mode'           = 'on-confirm'
        'reconnect-delay'    = 5
    }
    Write-Step "PUT /api/parameters/shovel/%2f/$ShovelName"
    Invoke-RabbitMqApi -Method Put -Path "/api/parameters/shovel/$vhostForApi/$([Uri]::EscapeDataString($ShovelName))" -Body @{ value = $shovelDefinition } -Mutating | Out-Null

    $lastShovelState = $null
    # 'flow' is a normal, healthy operational substate of a running shovel — it means
    # the shovel is up and connected on both ends but is currently being throttled by
    # RabbitMQ's own flow-control (e.g. backpressure from the destination), NOT that it
    # failed to connect. Treating only 'running' as success is a false-negative trap:
    # a shovel snapshotted mid flow-control at the poll instant is fully healthy and
    # will keep delivering, so accept both here (see docs/ShovelBridge-Architecture.md).
    $shovelRunning = Wait-ForCondition -Description "shovel '$ShovelName' running" -MaxAttempts 15 -DelaySeconds 2 -Condition {
        $shovels = Invoke-RabbitMqApi -Method Get -Path "/api/shovels/$vhostForApi" -AllowFail
        $mine = @($shovels) | Where-Object { $_.name -eq $ShovelName }
        if ($mine) { $script:lastShovelState = $mine[0] }
        $mine -and $mine[0].state -in @('running', 'flow')
    }
    if (-not $shovelRunning) {
        # The management API only ever reports 'starting'/'running' for the
        # current snapshot — it doesn't retain the connect failure reason once
        # the shovel restarts. Surface what we DO have (last known state) plus a
        # pointer to where the real reason lives (broker logs / Admin > Shovel
        # Status), since a bare "did not reach running" gives no lead on WHICH
        # side (src vs dest) is failing.
        $stateDetail = if ($lastShovelState) { " Last observed state: $($lastShovelState | ConvertTo-Json -Compress)." } else { ' No shovel status was ever observed for this name — check the PUT above succeeded.' }
        $destHint = if ($DestinationMode -eq 'ExternalServiceBus' -and $RabbitMqMode -eq 'External') {
            " If the broker logs show a TLS alert containing 'hostname_check_failed', the broker is missing the amqp10_client wildcard-hostname-check fix in its advanced.config — see the 'Security' section of docs/ShovelBridge-Architecture.md (this is the single most common cause against a real Service Bus namespace, and produces this exact generic 'failed to connect to destination' reason with no other symptom). -RabbitMqMode Container already bakes this fix in, so if you're seeing this in Container mode the cause is something else."
        } elseif ($DestinationMode -eq 'ExternalServiceBus' -and $RabbitMqMode -eq 'Container') {
            " -RabbitMqMode Container already bakes in the amqp10_client wildcard-hostname-check fix, so 'hostname_check_failed' is unlikely here. If the broker logs instead show '{cacerts, undefined}', append '&cacertfile=/etc/ssl/certs/ca-certificates.crt' to -ExternalShovelDestUri (Erlang/OTP 26+ requires an explicit CA bundle) — see the 'Security' section of docs/ShovelBridge-Architecture.md."
        } else { '' }
        throw "Shovel '$ShovelName' did not reach the 'running' state.$stateDetail Check the RabbitMQ broker logs (or Admin > Shovel Status in the management UI) for the connect failure reason — commonly a src-uri auth/permission failure, or a dest-uri (Service Bus) auth/network/queue-not-found failure.$destHint"
    }
    Write-Ok "Shovel '$ShovelName' is running."

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4 — Run the harness (publish + verify)
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Publish + verify via E2EHarness'

    if ($VerifyWorkflowExecution) {
        $resultTokenSecure = if ($ResultPollAuthToken) { $ResultPollAuthToken } else { $MessageAuthToken }
        $harnessArgs = @(
            'run', '--project', $harnessProjectDir, '-c', 'Release', '--'
            '--mode', 'workflow-execution'
            '--rabbitmq-management-uri', $mgmtUri
            '--rabbitmq-username', $rmqUser
            '--rabbitmq-password', $rmqPassword
            '--rabbitmq-vhost', $rmqVHost
            '--source-queue', $SourceQueueName
            '--workflow', $WorkflowName
            '--correlation-id', $CorrelationId
            '--message-auth-token', (ConvertFrom-SecureStringPlain $MessageAuthToken)
            '--engine-base-url', $EngineBaseUrl
            '--result-poll-auth-token', (ConvertFrom-SecureStringPlain $resultTokenSecure)
            '--result-timeout-seconds', $ResultTimeoutSeconds
            '--message-count', $MessageCount
            '--publish-concurrency', $PublishConcurrency
        )
        if (-not [string]::IsNullOrWhiteSpace($WorkflowInputsJson)) { $harnessArgs += @('--workflow-inputs-json', $WorkflowInputsJson) }
        if (-not [string]::IsNullOrWhiteSpace($Jti)) { $harnessArgs += @('--jti', $Jti) }
    } else {
        $harnessArgs = @(
            'run', '--project', $harnessProjectDir, '-c', 'Release', '--'
            '--rabbitmq-management-uri', $mgmtUri
            '--rabbitmq-username', $rmqUser
            '--rabbitmq-password', $rmqPassword
            '--source-queue', $SourceQueueName
            '--servicebus-connection-string', $serviceBusConnectionStringPlain
            '--destination-queue', $DestinationQueueName
            '--timeout-seconds', $HarnessTimeoutSeconds
            '--message-count', $MessageCount
            '--publish-concurrency', $PublishConcurrency
        )
    }
    Write-Step 'dotnet run (Warewolf.Execution.ServiceBusWorker.E2EHarness)'
    # Stream the harness's output live rather than buffering it and dumping it only
    # after the process exits — at load-test volumes this run can take many minutes
    # (build + publish + poll up to -ResultTimeoutSeconds), and a silent console during
    # that window is indistinguishable from a hang.
    & dotnet @harnessArgs 2>&1 | ForEach-Object { Write-Host "    $_" }
    $harnessExitCode = $LASTEXITCODE

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4b — Post-run Shovel/RabbitMQ diagnostics (root-causing message loss)
    # ════════════════════════════════════════════════════════════════════════
    # At load-test volumes a fraction of messages have been observed to never reach
    # the engine (e.g. 943/1000) with NO publish failures from the harness and NO
    # exceptions on the engine side - meaning the loss happens somewhere between this
    # local RabbitMQ source queue and the Service Bus destination (the Shovel's own
    # bridging, or a transient disconnect/reconnect). Always captured (pass or fail)
    # so a healthy run's numbers are available as a baseline for comparison. This is
    # the evidence needed to tell WHICH side lost the messages:
    #   - source queue empty (messages=0) AND publish == deliver_get == ack (all equal
    #     to MessageCount) -> every message was published, shoveled out, and acked;
    #     loss happened downstream of RabbitMQ entirely (Service Bus delivery, or the
    #     engine's own trigger dequeue) - inspect the Service Bus dead-letter queue
    #     for that run's correlationIds (see docs/ShovelBridge-Architecture.md).
    #   - publish == MessageCount but deliver_get/ack < MessageCount -> the Shovel
    #     itself never picked up (or never got an on-confirm ack for) some messages
    #     - a Shovel-side bridging/reliability issue, not a Service Bus/engine one.
    #   - publish < MessageCount -> the harness under-published (would already have
    #     thrown - see PublishToRabbitMqAsync - so should not happen silently).
    Write-Phase 'Phase 4b  Post-run Shovel/RabbitMQ diagnostics'
    try {
        Write-Step "GET /api/queues/$vhostForApi/$SourceQueueName"
        $finalQueue = Invoke-RabbitMqApi -Method Get -Path "/api/queues/$vhostForApi/$([Uri]::EscapeDataString($SourceQueueName))" -AllowFail
        if ($finalQueue) {
            $stats = $finalQueue.message_stats
            Write-Host "  Source queue '$SourceQueueName' final state:" -ForegroundColor White
            Write-Host "    messages (total)        : $($finalQueue.messages)" -ForegroundColor White
            Write-Host "    messages_ready          : $($finalQueue.messages_ready)" -ForegroundColor White
            Write-Host "    messages_unacknowledged : $($finalQueue.messages_unacknowledged)" -ForegroundColor White
            if ($stats) {
                Write-Host "    publish (lifetime)      : $($stats.publish)" -ForegroundColor White
                Write-Host "    deliver_get (lifetime)  : $($stats.deliver_get)" -ForegroundColor White
                Write-Host "    ack (lifetime)          : $($stats.ack)" -ForegroundColor White
                Write-Host "    redeliver (lifetime)    : $($stats.redeliver)" -ForegroundColor White
            } else {
                Write-Note 'No message_stats present on the queue response (RabbitMQ only populates these after some activity/short delay).'
            }
        } else {
            Write-Note "Could not retrieve source queue '$SourceQueueName' status for diagnostics (queue may already be gone, or the management API call failed)."
        }

        Write-Step "GET /api/shovels/$vhostForApi (looking for '$ShovelName')"
        $finalShovels = Invoke-RabbitMqApi -Method Get -Path "/api/shovels/$vhostForApi" -AllowFail
        $finalMine = @($finalShovels) | Where-Object { $_.name -eq $ShovelName }
        if ($finalMine) {
            Write-Host "  Shovel '$ShovelName' final state: $($finalMine[0] | ConvertTo-Json -Compress)" -ForegroundColor White
        } else {
            Write-Note "Shovel '$ShovelName' no longer reported by the management API (may have been torn down or restarted already)."
        }
    } catch {
        Write-Note "Post-run diagnostics query itself failed (non-fatal, does not affect the test result): $($_.Exception.Message)"
    }

    if ($harnessExitCode -ne 0) {
        throw "E2E harness reported failure (exit code $harnessExitCode). See output above."
    }

    Write-Phase 'ShovelBridge E2E test PASSED'
    if ($VerifyWorkflowExecution) {
        Write-Host "  Mode          : $DestinationMode (VerifyWorkflowExecution)" -ForegroundColor White
        Write-Host "  Source        : $SourceQueueName (RabbitMQ)" -ForegroundColor White
        Write-Host "  Destination   : $DestinationQueueName (Service Bus, Managed Identity trigger)" -ForegroundColor White
        Write-Host "  Workflow      : $WorkflowName" -ForegroundColor White
        Write-Host "  Messages      : $MessageCount" -ForegroundColor White
        Write-Host "  CorrelationId : $CorrelationId" -ForegroundColor White
        Write-Host "  Engine        : $EngineBaseUrl" -ForegroundColor White
    } else {
        Write-Host "  Mode        : $DestinationMode" -ForegroundColor White
        Write-Host "  Source      : $SourceQueueName (RabbitMQ)" -ForegroundColor White
        Write-Host "  Destination : $DestinationQueueName (Service Bus)" -ForegroundColor White
        Write-Host "  Messages    : $MessageCount" -ForegroundColor White
    }
    Write-Host ''
}
catch {
    Write-Note "ShovelBridge E2E test FAILED: $($_.Exception.Message)"
    throw
}
finally {
    if ($SkipTeardown) {
        if ($containersStarted.Count -gt 0 -or $needsDockerNetwork) {
            Write-Note "Skipping teardown (-SkipTeardown). Containers left running: $($containersStarted -join ', ') ; network: $networkName"
        } else {
            Write-Note 'Skipping teardown (-SkipTeardown). Nothing docker-managed was started (RabbitMqMode External + DestinationMode ExternalServiceBus).'
        }
    } else {
        Write-Phase 'Teardown'
        foreach ($name in $containersStarted) {
            Write-Step "Removing container '$name'"
            docker rm -f $name 2>&1 | Out-Null
        }
        if ($needsDockerNetwork) {
            Write-Step "Removing network '$networkName'"
            docker network rm $networkName 2>&1 | Out-Null
        }
        if ($configTempFile -and (Test-Path -LiteralPath $configTempFile)) { Remove-Item -LiteralPath $configTempFile -Force -ErrorAction SilentlyContinue }
        if ($enabledPluginsTempFile -and (Test-Path -LiteralPath $enabledPluginsTempFile)) { Remove-Item -LiteralPath $enabledPluginsTempFile -Force -ErrorAction SilentlyContinue }
        if ($advancedConfigTempFile -and (Test-Path -LiteralPath $advancedConfigTempFile)) { Remove-Item -LiteralPath $advancedConfigTempFile -Force -ErrorAction SilentlyContinue }
        Write-Ok 'Teardown complete.'
    }
}
