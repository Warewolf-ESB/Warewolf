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
    attached to it) and -MessageAuthToken / -EngineBaseUrl. Deliberately does
    NOT also run the Service Bus receiver-based arrival check in this mode: a second
    receiver on the same queue would compete with the engine's own subscription and could
    steal the message before the engine processes it.

    WOLF-8517: there is no -WorkflowName parameter any more. The message body no longer
    names a workflow — the deployed trigger's own Settings/servicebustrigger.bite fixes
    which single workflow every message on -DestinationQueueName runs (see
    docs/ServiceBusSecureTrigger-Architecture.md). Point -DestinationQueueName at a trigger
    already deployed/configured for the workflow you want to exercise (e.g.
    Resources/rabbit/RabbitProcess.bite — a dedicated shovel-bridge test workflow whose DB
    activities bind to SourceId b9184f70-… , the shared NewSqlServerSource the pipeline
    downloads).
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
    OPTIONAL, even when -VerifyWorkflowExecution. The Service Bus trigger no longer
    validates a per-message caller token — trust is the configured queue's transport
    boundary instead (see docs/ServiceBusSecureTrigger-Architecture.md). Pass this only to
    prove the trigger genuinely ignores it; when supplied it is embedded as the message's
    Authorization application property, same as before.
.PARAMETER EngineBaseUrl
    REQUIRED when -VerifyWorkflowExecution. Base URL of the Lightweight engine Function App
    that is listening on -DestinationQueueName, e.g.
    https://warewolfserver-uat.azurewebsites.net. Used to poll
    GET /secure/servicebus-result/{correlationId}.
.PARAMETER ResultPollAuthToken
    REQUIRED when -VerifyWorkflowExecution. Bearer token used to call
    GET /secure/servicebus-result/{correlationId} — an ordinary HTTP `/secure/*` route,
    still protected by the EasyAuth/claims/policy pipeline (requires
    WorkflowPermission.View on whichever workflow the deployed trigger's
    Settings/servicebustrigger.bite names). Independent of -MessageAuthToken; no longer
    defaults from it, since that token no longer has any effect on the trigger itself.
.PARAMETER ResultTimeoutSeconds
    Only used when -VerifyWorkflowExecution. How long to poll for ALL correlationIds (1, or N
    when -MessageCount > 1) to reach a terminal result before failing. Default: 90. Raise this
    substantially for large -MessageCount values (e.g. 1000) since each execution takes real
    engine processing time and this timeout is shared across all of them.

    NOTE the meaning changes under the late-mint mode below: there the drain is waited out
    separately (-DrainTimeoutSeconds), so this becomes a budget for the post-drain poll ONLY,
    against results that are already recorded - it does not need to cover execution time.
.PARAMETER ResultPollTenantId
.PARAMETER ResultPollClientId
.PARAMETER ResultPollResourceAppId
.PARAMETER ResultPollClientSecret
    OPT-IN, all four together. Client credentials used to mint the result-poll bearer token
    LATE - after the trigger queue has drained - instead of taking a pre-minted one via
    -ResultPollAuthToken. Supply these INSTEAD OF -ResultPollAuthToken; supplying both is an
    error, and supplying an incomplete set is an error naming what is missing.

    WHY THIS EXISTS. GET /secure/servicebus-result/{correlationId} is an ordinary
    Entra-protected HTTP route - the Service Bus trigger's "trust the configured queue's
    transport boundary" relaxation applies to the MESSAGE path only, never to this read path
    (Functions/ServiceBusResultFunction.cs carries [RequireWorkflowPermission(View)], and
    Auth/Middleware/WorkflowAuthorizationMiddleware.cs rejects an unauthenticated /Secure/*
    request BEFORE policy evaluation, so even WAREWOLF_SECURITY_FLAGS bypassSecureConfig=true
    does not make it anonymous - that flag skips authorization, not authentication). An Entra
    access token lives 60-90 minutes. A 20,000-message drain takes longer, and the harness
    treats 401 as terminal rather than transient (WOLF-8418: a 500/401 from this engine is a
    real denial), so a token minted before publishing expires mid-poll and marks EVERY
    still-pending correlationId as a spurious HttpError - indistinguishable at a glance from
    mass message loss.

    Minting after the drain sidesteps that entirely: the token is fresh when polling starts,
    and because every result is already durably recorded the poll resolves in a few sweeps
    rather than spanning the whole run.

    REQUIRES PERSISTENCE on the target engine (-EnablePersistence on
    Deploy-WwExecutionEngine.ps1). With persistence off, ServiceBusReplayAndResultStore falls
    back to a process-local dictionary, so a result is only visible on the instance that
    produced it and a deferred poll would 404 on most ids.

    -ResultPollClientId is the CALLER app (e.g. the Warewolf-ShovelBridgeE2E-Daemon
    registration); -ResultPollResourceAppId is the ENGINE's app registration, i.e. the
    audience, requested as 'api://<ResourceAppId>/.default'. They are different apps - see
    Get-E2EEngineToken in WwE2E.Common.psm1.
.PARAMETER DrainQuietSeconds
    Only used in the late-mint mode above. How long the trigger queue's active+scheduled
    message counts must stay at ZERO before the drain is considered finished. Default: 360.

    DO NOT LOWER THIS BELOW ~360 without understanding both reasons it is not merely padding:
      1. A locked (in-flight) message is NOT counted as active by Service Bus, so a momentary
         zero can be read while executions are still running.
      2. The trigger queue's lock duration is PT5M, so a message whose lock expired is
         redelivered roughly five minutes later. A quiet window shorter than that declares the
         run finished during the gap and reports the redelivered messages as permanently
         missing - the exact artefact that made an earlier 5,000-message baseline report 878
         false losses.
.PARAMETER DrainTimeoutSeconds
    Only used in the late-mint mode above. Hard ceiling on the drain wait, after which the run
    fails with the last observed queue counts rather than hanging until the CI job's own
    timeout kills it (which would lose every diagnostic this script prints afterwards).
    Default: 3600. Size it from measured throughput - a 20,000-message run against an engine
    capped at 2 concurrent executions per instance has been observed to need well over an hour.
.PARAMETER EnginePrewarmTimeoutSeconds
    Only used when -VerifyWorkflowExecution. -EngineBaseUrl frequently runs on an Azure
    Functions Consumption plan with no 'alwaysOn' (e.g. WarewolfServer-UAT), so it scales to
    zero when idle and a cold start can take a minute or more (see docs/LoadTest-Guide.md).
    -ResultTimeoutSeconds is a budget for Phase 4's timed result poll, NOT for a cold start -
    hitting an idle engine there risks burning the whole budget on transient 503s (the
    platform's generic "The service is unavailable.", not the app's own JSON error body) before
    the host finishes initialising. Phase 0 pings a cheap, unauthenticated public route
    (GET /apis.json) up to this many seconds, in parallel with Phases 1-3's own setup time,
    so the engine is warm well before Phase 4 starts its clock. Default: 120. A failed/timed-out
    pre-warm is logged as a warning, not a hard failure - Phase 4 still runs and may recover on
    its own retries.

.PARAMETER JobsDbConnectionString
    OPT-IN. Only meaningful with -VerifyWorkflowExecution. A SQL-auth connection string (e.g.
    the 'devops_warewolf' login already granted db_datareader/EXECUTE on schema dbo - see
    Resources/rabbit/Provision-ShovelBridgeSchema.sql's Grants section) for the database
    RabbitProcess.bite itself writes to via usp_jobs1_LogStart/_LogProcessing/_LogFinished[
    WithError]. When supplied, a new Phase 4c reconciles dbo.jobs1 directly against
    -MessageCount: usp_jobs1_Summary (scoped to this run's own start time, never the whole
    table - see the 2026-08-13 "shared, reused source queue" caveat elsewhere in this repo for
    why an unscoped query across every historical run would be misleading) for the headline
    succeeded/duplicate/stuck buckets, plus a missing-sequence-number check (which of the N
    expected '<prefix><CorrelationId>-NNNNNN' messages never got so much as a STARTED row at
    all - a message lost/denied/dead-lettered before RabbitProcess's own DB activity ever ran,
    which usp_jobs1_Summary's own aggregation cannot see since it only groups rows that already
    exist). Skipped with a note (not a hard failure) when omitted, same as every other opt-in
    switch in this script.
.PARAMETER DbVerificationMode
    Which schema Phase 4c reconciles against. Only used when -JobsDbConnectionString is supplied.

    'Jobs1' (default) — dbo.jobs1, written by RabbitProcess.bite via usp_jobs1_LogStart/
    _LogProcessing/_LogFinished. Reconciles per message on the unique MessageContent
    ('<prefix><correlationId>'), using -JobsDbMessageContentPrefix.

    'ProfilerRecommendation' — Profiler.ProfilerRecommendation, written by profiler/ProfilerMain
    via Profiler.usp_InsertProfilerRecommendation (see Resources/profiler/Provision-ProfilerSchema.sql).
    ProfilerMain writes nothing to dbo.jobs1, so the default mode would report
    'usp_jobs1_Summary returned no rows' for every run. Reconciles per message on MandateId,
    which the pipeline templates as '{correlationGuid}' so each message carries a distinct,
    run-scoped uniqueidentifier — see WorkflowInputTemplate.cs.

.PARAMETER JobsDbMessageContentPrefix
    Only used when -JobsDbConnectionString is supplied. The literal prefix RabbitProcess's
    MessageContent carries ahead of the '-{correlationId}' suffix - see -WorkflowInputsJson's
    own '{"message":"loadtest-{correlationId}"}' convention. Default: 'loadtest-'.
.PARAMETER JobsDbStuckThresholdMinutes
    Only used when -JobsDbConnectionString is supplied. Passed straight through to
    usp_jobs1_Summary's own @StuckThresholdMinutes. Default: '15' (that procedure's own
    default).
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

    # WOLF-8512 (build 30658): supply BOTH to enable the Phase 4d dead-letter reconciliation.
    # Optional - when either is omitted, Phase 4d reports that it was skipped rather than
    # failing, so existing callers are unaffected. Requires an authenticated 'az' CLI with
    # read access to the namespace.
    [string] $ServiceBusNamespaceName,
    [string] $ServiceBusResourceGroup,

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
    [string] $WorkflowInputsJson,
    [string] $CorrelationId,
    [securestring] $MessageAuthToken,
    [string] $EngineBaseUrl,
    [securestring] $ResultPollAuthToken,
    [int]    $ResultTimeoutSeconds = 90,
    [int]    $EnginePrewarmTimeoutSeconds = 120,

    # ── Late-minted result-poll token (opt-in; for large -MessageCount runs) ──────────
    # Supplying this credential set switches Phase 4 from ONE harness invocation
    # (publish+poll together) to THREE steps: publish, wait for the trigger queue to drain,
    # then mint a fresh token and poll. See -ResultPollClientId for why that matters.
    [string] $ResultPollTenantId,
    [string] $ResultPollClientId,
    [string] $ResultPollResourceAppId,
    [securestring] $ResultPollClientSecret,
    [int]    $DrainQuietSeconds = 360,
    [int]    $DrainTimeoutSeconds = 3600,

    # ── Database verification (opt-in) ──────────────────────────────────────
    [securestring] $JobsDbConnectionString,
    [ValidateSet('Jobs1','ProfilerRecommendation')]
    [string] $DbVerificationMode = 'Jobs1',
    [string] $JobsDbMessageContentPrefix = 'loadtest-',
    [string] $JobsDbStuckThresholdMinutes = '15',

    # Test hook: dot-source the pure helpers for unit testing (matches the same
    # convention already used by Configure-RabbitMqShovel.ps1 / Invoke-WwQueueLoadTest.ps1).
    [switch] $LoadFunctionsOnly
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

function Get-WwDeadLetterCount {
    <#
        WOLF-8512 (build 30658): reads the destination queue's dead-letter depth via the
        Azure CLI.

        Why this matters enough to have its own phase: a message that fails terminally is
        dead-lettered and is therefore COUNTABLE. A message that is destroyed — the failure
        mode this whole change fixes, where an unsettled return got auto-completed — leaves
        no trace at all: no result, no DLQ entry, nothing. Builds 30649 through 30658 lost
        messages exactly that way and the harness could only report "never got a result",
        which looks identical to "still slow". Reconciling the DLQ delta against the
        harness's own failure count separates the two definitively.

        Returns $null (never throws) when the count cannot be read — the caller degrades to
        a note rather than failing the run on a diagnostics gap.
    #>
    param(
        [string] $NamespaceName,
        [string] $ResourceGroup,
        [string] $QueueName
    )

    $counts = Get-WwQueueCountDetails -NamespaceName $NamespaceName -ResourceGroup $ResourceGroup -QueueName $QueueName
    if ($null -eq $counts) { return $null }
    return $counts.DeadLetter
}

function Get-WwQueueCountDetails {
    <#
        Reads a Service Bus queue's whole countDetails block via the Azure CLI, as a plain
        object with Active/DeadLetter/Scheduled/Transfer members.

        Generalises the single-value read Get-WwDeadLetterCount used to do (that function now
        delegates here) because the drain wait needs ACTIVE and SCHEDULED depth, not just the
        dead-letter count, and issuing two separate 'az servicebus queue show' calls to read
        two fields of the same response would just double the polling cost and let the two
        readings drift apart.

        Returns $null (never throws) when the counts cannot be read - same degradation contract
        Get-WwDeadLetterCount has always had. Callers must treat $null as "unknown", NEVER as
        zero: for the drain wait in particular, reading $null as zero would declare a queue
        drained because the CLI hiccuped.
    #>
    param(
        [string] $NamespaceName,
        [string] $ResourceGroup,
        [string] $QueueName
    )

    if (-not $NamespaceName -or -not $ResourceGroup) { return $null }

    try {
        $raw = az servicebus queue show `
            --namespace-name $NamespaceName `
            --resource-group $ResourceGroup `
            --name $QueueName `
            --query 'countDetails' `
            -o json 2>$null
        if ($LASTEXITCODE -ne 0) { return $null }
        $parsed = ($raw | Out-String) | ConvertFrom-Json
        if ($null -eq $parsed) { return $null }

        # az returns camelCase; Set-StrictMode makes a missing member throw, so probe rather
        # than dereference - older api-versions omit the transfer counters entirely.
        function Get-CountOrZero($obj, [string] $name) {
            $prop = $obj.PSObject.Properties[$name]
            if ($null -eq $prop -or $null -eq $prop.Value) { return 0 }
            $n = 0
            if ([int]::TryParse([string] $prop.Value, [ref] $n)) { return $n }
            return 0
        }

        return [pscustomobject]@{
            Active     = Get-CountOrZero $parsed 'activeMessageCount'
            DeadLetter = Get-CountOrZero $parsed 'deadLetterMessageCount'
            Scheduled  = Get-CountOrZero $parsed 'scheduledMessageCount'
            Transfer   = Get-CountOrZero $parsed 'transferMessageCount'
        }
    } catch {
        return $null
    }
}

function Get-WwDrainVerdict {
    <#
        Pure decision function for the drain wait - given one sampling of the trigger queue's
        depth, decides whether the queue counts as quiet right now and whether it has been
        quiet long enough to call the run drained. No Azure access, so it is unit-tested
        directly (Test-ShovelBridgeE2E.Tests.ps1) rather than only via a live run.

        -ActiveCount/-ScheduledCount are $null when the counts could not be read. That is
        deliberately treated as NOT QUIET (and resets any quiet streak), because an unreadable
        queue must never be mistaken for an empty one - the failure mode would be ending the
        wait early and reporting every still-executing message as lost.

        -QuietSince is the caller's carried state: $null when the last sample was not quiet,
        otherwise the UTC time the current quiet streak began. It is returned (updated) so the
        caller can feed it back on the next iteration.

        The quiet window itself is load-bearing, not padding - see -DrainQuietSeconds' help for
        the two independent reasons (locked messages are not counted active; PT5M lock duration
        means redeliveries arrive ~5 minutes apart).
    #>
    param(
        $ActiveCount,
        $ScheduledCount,
        [Parameter(Mandatory)][int] $QuietSeconds,
        $QuietSince,
        [Parameter(Mandatory)][datetime] $NowUtc
    )

    $readable = ($null -ne $ActiveCount) -and ($null -ne $ScheduledCount)
    $isQuiet  = $readable -and (([int] $ActiveCount + [int] $ScheduledCount) -eq 0)

    if (-not $isQuiet) {
        return [pscustomobject]@{
            IsQuiet      = $false
            IsDrained    = $false
            QuietSince   = $null
            SecondsQuiet = 0
            IsReadable   = $readable
        }
    }

    $since = if ($null -eq $QuietSince) { $NowUtc } else { [datetime] $QuietSince }
    $secondsQuiet = [int][Math]::Floor(($NowUtc - $since).TotalSeconds)

    return [pscustomobject]@{
        IsQuiet      = $true
        IsDrained    = ($secondsQuiet -ge $QuietSeconds)
        QuietSince   = $since
        SecondsQuiet = $secondsQuiet
        IsReadable   = $true
    }
}

function Wait-WwTriggerQueueDrain {
    <#
        Blocks until the trigger queue has been quiet for -QuietSeconds, or throws once
        -TimeoutSeconds elapses. Thin loop around Get-WwQueueCountDetails (the I/O) and
        Get-WwDrainVerdict (the decision), so the logic worth testing has no Azure dependency.

        Throwing on timeout rather than returning false is deliberate: a run that never drained
        has not produced a result worth polling for, and failing here surfaces the last observed
        queue depths - which is the diagnostic that says whether messages were still executing,
        still queued, or already dead-lettered.
    #>
    param(
        [Parameter(Mandatory)][string] $NamespaceName,
        [Parameter(Mandatory)][string] $ResourceGroup,
        [Parameter(Mandatory)][string] $QueueName,
        [Parameter(Mandatory)][int]    $QuietSeconds,
        [Parameter(Mandatory)][int]    $TimeoutSeconds,
        [int] $PollIntervalSeconds = 30
    )

    $deadline   = (Get-Date).ToUniversalTime().AddSeconds($TimeoutSeconds)
    $quietSince = $null
    $last       = $null
    $lastReport = [datetime]::MinValue

    while ($true) {
        $counts = Get-WwQueueCountDetails -NamespaceName $NamespaceName -ResourceGroup $ResourceGroup -QueueName $QueueName
        $now    = (Get-Date).ToUniversalTime()

        $activeNow    = $null
        $scheduledNow = $null
        if ($null -ne $counts) {
            $activeNow    = $counts.Active
            $scheduledNow = $counts.Scheduled
        }

        $verdict = Get-WwDrainVerdict `
            -ActiveCount    $activeNow `
            -ScheduledCount $scheduledNow `
            -QuietSeconds   $QuietSeconds `
            -QuietSince     $quietSince `
            -NowUtc         $now

        $quietSince = $verdict.QuietSince
        if ($null -ne $counts) { $last = $counts }

        if ($verdict.IsDrained) {
            Write-Ok "Trigger queue '$QueueName' drained (active+scheduled 0 for $($verdict.SecondsQuiet)s >= ${QuietSeconds}s). Dead-letter depth now: $($last.DeadLetter)."
            return $last
        }

        if ($now -ge $deadline) {
            $detail = 'queue depths were never readable (az servicebus queue show kept failing)'
            if ($null -ne $last) {
                $detail = "last observed: active=$($last.Active), scheduled=$($last.Scheduled), deadLetter=$($last.DeadLetter)"
            }
            throw "Trigger queue '$QueueName' did not drain within ${TimeoutSeconds}s ($detail). Raise -DrainTimeoutSeconds if the run is simply slower than budgeted, or investigate why messages are not being consumed - a non-zero active count that never falls means the engine is not keeping up (or not listening), while a climbing deadLetter count means they are failing terminally."
        }

        # One line per minute, not per poll: a 90-minute drain would otherwise bury the rest of
        # the log, but total silence is indistinguishable from a hang.
        if (($now - $lastReport).TotalSeconds -ge 60) {
            $shown = 'unreadable'
            if ($null -ne $counts) {
                $shown = "active=$($counts.Active), scheduled=$($counts.Scheduled), deadLetter=$($counts.DeadLetter)"
            }
            $quietNote = ''
            if ($verdict.IsQuiet) { $quietNote = " (quiet for $($verdict.SecondsQuiet)s of ${QuietSeconds}s)" }
            $remaining = [int][Math]::Max(0, ($deadline - $now).TotalSeconds)
            Write-Step "Draining '$QueueName': $shown$quietNote - ${remaining}s of budget left"
            $lastReport = $now
        }

        Start-Sleep -Seconds $PollIntervalSeconds
    }
}

function Get-WwCorrelationGuidStem {
    <#
        Derives the constant 30-character GUID "stem" shared by every MandateId in a run —
        i.e. a correlationId prefix reformatted as the leading part of a uniqueidentifier,
        ready for a 6-digit message sequence to be appended.

        This MUST stay byte-for-byte compatible with
        WorkflowInputTemplate.ToCorrelationGuid() in the E2E harness
        (Dev/Warewolf.Execution.ServiceBusWorker.E2EHarness/WorkflowInputTemplate.cs), which is
        what actually stamps MandateId onto each published message. The harness takes 26 hex
        characters of the correlationId prefix and appends the 6-digit sequence, dashed
        8-4-4-4-12; this reproduces the first 30 characters of that (through the 6 hex chars
        of the final group), leaving the caller to append the sequence.

        A drift between the two would not throw — it would silently produce MandateIds that
        match nothing, and Phase 4c would report every message as missing. Pure function, no
        DB access, so both sides are pinned by unit tests (see Test-ShovelBridgeE2E.Tests.ps1).
    #>
    param(
        [Parameter(Mandatory)] [AllowEmptyString()] [string] $CorrelationIdPrefix
    )

    $hex = ($CorrelationIdPrefix -replace '[^0-9a-fA-F]', '')
    if ($hex.Length -lt 26) {
        $hex = $hex.PadRight(26, '0')
    }
    $head26 = $hex.Substring(0, 26)

    return '{0}-{1}-{2}-{3}-{4}' -f $head26.Substring(0, 8),  $head26.Substring(8, 4),
                                    $head26.Substring(12, 4), $head26.Substring(16, 4),
                                    $head26.Substring(20, 6)
}

function Get-WwJobsDbVerdict {
    <#
        Evaluates a usp_jobs1_Summary result row (plus an optional list of sequence numbers
        that never got so much as a STARTED row in dbo.jobs1 at all) against the expected
        -MessageCount and returns a verdict object. Pure function - no DB access - so it is
        unit-testable with fabricated rows (see Test-ShovelBridgeE2E.Tests.ps1).

        "Missing" (a sequence number with zero jobs1 rows) is deliberately tracked SEPARATELY
        from usp_jobs1_Summary's own buckets: that procedure aggregates over rows that already
        exist, so a message dead-lettered/denied/lost before RabbitProcess's own DB activity
        ever ran is invisible to it - it never appears in ANY of Summary's counts, not even
        FailedAllAttempts or StillInProcess_Incomplete.
    #>
    param(
        [Parameter(Mandatory)] $SummaryRow,
        [Parameter(Mandatory)][int] $ExpectedMessageCount,
        [int[]] $MissingSeqNums = @()
    )
    $succeeded          = [int]$SummaryRow.SucceededTotal
    $duplicates         = [int]$SummaryRow.DuplicateSuccess_ReliabilityBug
    $stuckIncomplete    = [int]$SummaryRow.StillInProcess_Incomplete
    $stuckPastThreshold = [int]$SummaryRow.StillInProcess_StuckPastThreshold
    $missingCount       = @($MissingSeqNums).Count

    [pscustomobject]@{
        Expected              = $ExpectedMessageCount
        Succeeded             = $succeeded
        MissingCount          = $missingCount
        MissingSeqNums        = @($MissingSeqNums)
        DuplicateSuccessCount = $duplicates
        StillIncomplete       = $stuckIncomplete
        StuckPastThreshold    = $stuckPastThreshold
        IsClean               = ($succeeded -eq $ExpectedMessageCount) -and ($duplicates -eq 0) -and ($missingCount -eq 0)
    }
}

if ($LoadFunctionsOnly) { return }

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

# Declared unconditionally so the Phase 4 branches can test it under Set-StrictMode even
# when -VerifyWorkflowExecution was never passed (the validation block below is what sets it).
$useLateMintedResultToken = $false

if ($VerifyWorkflowExecution) {
    # A live engine must already be listening (Managed Identity) on -DestinationQueueName —
    # the local emulator has no engine attached to it, so this mode only makes sense against
    # a real, already-provisioned Service Bus namespace/queue.
    if ($DestinationMode -ne 'ExternalServiceBus') {
        throw '-VerifyWorkflowExecution requires -DestinationMode ExternalServiceBus (a live Lightweight engine must already be listening on -DestinationQueueName).'
    }
    # ── Which way is the result-poll token supplied? ──────────────────────────
    # Either a pre-minted -ResultPollAuthToken (original behaviour: one harness invocation
    # publishes and polls together), or the four late-mint credentials (publish, wait for the
    # drain, mint fresh, then poll). Exactly one of the two - see -ResultPollClientId's help
    # for why the late-mint path exists at load-test volumes.
    $lateMintParts = [ordered]@{
        '-ResultPollTenantId'      = $ResultPollTenantId
        '-ResultPollClientId'      = $ResultPollClientId
        '-ResultPollResourceAppId' = $ResultPollResourceAppId
    }
    $lateMintSupplied = @($lateMintParts.Values | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }).Count
    if ($ResultPollClientSecret -and $ResultPollClientSecret.Length -gt 0) { $lateMintSupplied++ }
    $useLateMintedResultToken = $lateMintSupplied -gt 0
    $haveStaticResultToken = ($null -ne $ResultPollAuthToken -and $ResultPollAuthToken.Length -gt 0)

    if ($useLateMintedResultToken) {
        if ($lateMintSupplied -lt 4) {
            $missing = @($lateMintParts.GetEnumerator() | Where-Object { [string]::IsNullOrWhiteSpace($_.Value) } | ForEach-Object { $_.Key })
            if (-not $ResultPollClientSecret -or $ResultPollClientSecret.Length -eq 0) { $missing += '-ResultPollClientSecret' }
            throw "The late-minted result-token credentials are incomplete - missing: $($missing -join ', '). Supply all four (-ResultPollTenantId, -ResultPollClientId, -ResultPollResourceAppId, -ResultPollClientSecret), or none of them and a pre-minted -ResultPollAuthToken instead."
        }
        if ($haveStaticResultToken) {
            throw 'Supply EITHER -ResultPollAuthToken (pre-minted, polled inline) OR the four -ResultPollTenantId/-ResultPollClientId/-ResultPollResourceAppId/-ResultPollClientSecret credentials (minted after the drain) - not both, since the two select different Phase 4 shapes and passing both hides which one is actually in effect.'
        }
        if ([string]::IsNullOrWhiteSpace($ServiceBusNamespaceName) -or [string]::IsNullOrWhiteSpace($ServiceBusResourceGroup)) {
            throw 'The late-minted result-token mode needs -ServiceBusNamespaceName and -ServiceBusResourceGroup: it waits for the trigger queue to drain (reading its active/scheduled depth via the Azure CLI) before minting the token and polling.'
        }
    } elseif (-not $haveStaticResultToken) {
        throw '-ResultPollAuthToken is required when -VerifyWorkflowExecution (used to poll the still Entra-protected GET /secure/servicebus-result endpoint), unless the four late-mint credentials (-ResultPollTenantId/-ResultPollClientId/-ResultPollResourceAppId/-ResultPollClientSecret) are supplied instead.'
    }
    if ([string]::IsNullOrWhiteSpace($EngineBaseUrl)) {
        throw '-EngineBaseUrl is required when -VerifyWorkflowExecution.'
    }
    if ([string]::IsNullOrWhiteSpace($CorrelationId)) {
        $CorrelationId = [Guid]::NewGuid().ToString('N')
    }
    Write-Ok "VerifyWorkflowExecution is set: Phase 4 will publish $(if ($MessageCount -eq 1) { "a workflow-trigger message (correlationId '$CorrelationId')" } else { "$MessageCount workflow-trigger messages (correlationId prefix '$CorrelationId')" }) to the deployed trigger's queue and poll $EngineBaseUrl for $(if ($MessageCount -eq 1) { 'its' } else { 'their' }) execution result, instead of proving bare message arrival. Whichever workflow -DestinationQueueName's trigger is configured to run (Settings/servicebustrigger.bite) is what executes — this script no longer selects it."

    # Fail fast on a stale bearer token rather than discovering it 30 minutes and (at
    # load-test volumes) 1000 messages later as a wall of "401: Authentication required"
    # HttpErrors that look, at a glance, like a real authorization failure rather than an
    # expired token. -ResultTimeoutSeconds bounds Phase 4's OWN polling window; add a fixed
    # buffer for Phases 1-3's setup time (RabbitMQ/shovel/queue provisioning), which elapses
    # BEFORE Phase 4 starts spending the token's remaining lifetime.
    $setupBufferSeconds = 300
    $requiredLifetimeSeconds = $ResultTimeoutSeconds + $setupBufferSeconds
    $tokensToCheck = [ordered]@{}
    if ($haveStaticResultToken) {
        $tokensToCheck['-ResultPollAuthToken'] = ConvertFrom-SecureStringPlain $ResultPollAuthToken
    } else {
        # Nothing to check yet - the whole point of the late-mint mode is that no result-poll
        # token exists at this stage. The SAME assertion is applied to the freshly-minted token
        # in Phase 4, once it has been acquired, so the guard is deferred rather than dropped.
        Write-Ok 'Result-poll token will be minted after the drain (late-mint mode) - its lifetime is asserted then, not now.'
    }
    if ($MessageAuthToken -and $MessageAuthToken.Length -gt 0) {
        $tokensToCheck['-MessageAuthToken'] = ConvertFrom-SecureStringPlain $MessageAuthToken
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

    # ── Pre-warm the target engine ─────────────────────────────────────────
    # -EngineBaseUrl commonly points at an Azure Functions Consumption plan with no
    # 'alwaysOn' (e.g. WarewolfServer-UAT - see docs/Deploy-UAT-Redeploy-Spec.md R3), which
    # scales to zero when idle. Phase 4's -ResultTimeoutSeconds window is a budget for the
    # RESULT POLL, not for a cold start - firing the first request against an idle engine
    # there risks burning the whole budget on transient 503s ("The service is unavailable.",
    # Azure's generic platform response while the host initialises - distinct from the app's
    # own JSON error/result body) before the host is even ready. Ping a cheap, unauthenticated
    # public route now so cold start overlaps with Phases 1-3's own setup time instead.
    Write-Step "Pre-warming $EngineBaseUrl (up to ${EnginePrewarmTimeoutSeconds}s - Consumption-plan cold start can otherwise consume the whole result-poll budget)"
    $warmDeadline = (Get-Date).AddSeconds($EnginePrewarmTimeoutSeconds)
    $engineWarm = $false
    $lastPrewarmError = $null
    do {
        try {
            $prewarmResponse = Invoke-WebRequest -Uri "$EngineBaseUrl/apis.json" -UseBasicParsing -TimeoutSec 20
            if ($prewarmResponse.StatusCode -ge 200 -and $prewarmResponse.StatusCode -lt 500) {
                $engineWarm = $true
            } else {
                $lastPrewarmError = "HTTP $($prewarmResponse.StatusCode)"
            }
        } catch {
            $lastPrewarmError = $_.Exception.Message
        }
        if (-not $engineWarm -and (Get-Date) -lt $warmDeadline) {
            Start-Sleep -Seconds 5
        }
    } while (-not $engineWarm -and (Get-Date) -lt $warmDeadline)

    if ($engineWarm) {
        Write-Ok "$EngineBaseUrl responded - engine is warm."
    } else {
        Write-Note "$EngineBaseUrl did not respond within ${EnginePrewarmTimeoutSeconds}s (last error: $lastPrewarmError). Phase 4 may still hit a cold-start 503 - consider raising -EnginePrewarmTimeoutSeconds and/or -ResultTimeoutSeconds. Continuing anyway; Phase 4's own retries may still recover."
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
# Configure-RabbitMqShovel.ps1 that isn't passed explicitly here reverts to THAT
# script's own default in this scope — merely BINDING an unpassed parameter
# overwrites a like-named caller variable, whether or not the callee ever reads it.
#
# Passing the shared names through one at a time is NOT a general fix, and this
# comment used to claim -ShovelName was "the one parameter name the two scripts
# share". It wasn't: -ServiceBusResourceGroup was added to THIS script's param block
# on 2026-09-14 without anyone noticing Configure-RabbitMqShovel.ps1 already declared
# one, so it silently became '' here — AFTER Phase 0 had validated it as 'DEV2'.
# Build 30688 published all 20,000 messages and then died at the Phase 4 drain wait
# with "Cannot bind argument to parameter 'ResourceGroup' because it is an empty
# string"; Phase 4d's dead-letter baseline read four lines earlier had already gone
# silently null, which was the only earlier symptom. Same hazard, same file, as the
# build 30638 $MessageCount clobber in Phase 4c below.
#
# So: save/restore EVERY name the two param blocks share. Test-ShovelBridgeE2E.Tests.ps1
# pins that set by AST-diffing the two param blocks, so the next parameter either
# script gains fails a unit test instead of a 20,000-message pipeline run.
$savedShovelName              = $ShovelName
$savedServiceBusResourceGroup = $ServiceBusResourceGroup
$savedLoadFunctionsOnly       = $LoadFunctionsOnly
. $configureScript -LoadFunctionsOnly `
    -RabbitMqManagementUri $mgmtUri `
    -RabbitMqUsername $rmqUser `
    -RabbitMqPassword $rmqPasswordSecure `
    -ShovelName $ShovelName
$ShovelName              = $savedShovelName
$ServiceBusResourceGroup = $savedServiceBusResourceGroup
$LoadFunctionsOnly       = $savedLoadFunctionsOnly

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
    # WOLF-8512 (build 30658): baseline the destination DLQ before anything is published, so
    # Phase 4d can reconcile the delta this run is responsible for. The queue accumulates
    # dead-letters across runs (21,461 of them when this was written), so only the delta is
    # meaningful.
    $dlqBaseline = Get-WwDeadLetterCount -NamespaceName $ServiceBusNamespaceName -ResourceGroup $ServiceBusResourceGroup -QueueName $DestinationQueueName
    if ($null -ne $dlqBaseline) {
        Write-Step "Dead-letter baseline for '$DestinationQueueName': $dlqBaseline (Phase 4d reconciles the delta)"
    }

    Write-Phase 'Phase 4  Publish + verify via E2EHarness'

    # Captured before publishing starts so Phase 4c's own dbo.jobs1 query can scope itself to
    # THIS run's own rows (usp_jobs1_Summary @FromUtc=...) rather than the whole, shared,
    # never-purged table (see the 2026-08-13 "correlation looks across the ENTIRE table
    # history" caveat in Resources/rabbit/Provision-ShovelBridgeSchema.sql).
    $runStartUtc = (Get-Date).ToUniversalTime()

    if ($VerifyWorkflowExecution -and $useLateMintedResultToken) {
        # ── Split Phase 4: publish -> drain -> mint -> poll ───────────────────
        # See -ResultPollClientId's help for why these cannot be one invocation at load-test
        # volumes. Note $harnessExitCode is set from the POLL leg (or from the publish leg if
        # that fails first), so Phases 4b-4d downstream read it exactly as before.
        $publishArgs = @(
            'run', '--project', $harnessProjectDir, '-c', 'Release', '--'
            '--mode', 'workflow-execution'
            '--phase', 'publish'
            '--rabbitmq-management-uri', $mgmtUri
            '--rabbitmq-username', $rmqUser
            '--rabbitmq-password', $rmqPassword
            '--rabbitmq-vhost', $rmqVHost
            '--source-queue', $SourceQueueName
            '--correlation-id', $CorrelationId
            '--engine-base-url', $EngineBaseUrl
            '--message-count', $MessageCount
            '--publish-concurrency', $PublishConcurrency
        )
        if (-not [string]::IsNullOrWhiteSpace($WorkflowInputsJson)) { $publishArgs += @('--workflow-inputs-json', $WorkflowInputsJson) }
        if ($MessageAuthToken -and $MessageAuthToken.Length -gt 0) { $publishArgs += @('--message-auth-token', (ConvertFrom-SecureStringPlain $MessageAuthToken)) }

        Write-Step "dotnet run (E2EHarness --phase publish, $MessageCount message(s), no poll token needed)"
        & dotnet @publishArgs 2>&1 | ForEach-Object { Write-Host "    $_" }
        $harnessExitCode = $LASTEXITCODE

        if ($harnessExitCode -ne 0) {
            Write-Note "Publish leg failed (exit $harnessExitCode) - skipping the drain wait and result poll; there is nothing to reconcile."
        } else {
            Write-Step "Waiting for trigger queue '$DestinationQueueName' to drain (quiet window ${DrainQuietSeconds}s, budget ${DrainTimeoutSeconds}s)"
            Wait-WwTriggerQueueDrain `
                -NamespaceName   $ServiceBusNamespaceName `
                -ResourceGroup   $ServiceBusResourceGroup `
                -QueueName       $DestinationQueueName `
                -QuietSeconds    $DrainQuietSeconds `
                -TimeoutSeconds  $DrainTimeoutSeconds | Out-Null

            Write-Step "Minting a fresh result-poll token (client '$ResultPollClientId' -> audience 'api://$ResultPollResourceAppId')"
            Import-Module (Join-Path $PSScriptRoot '..' '..' 'WwE2E.Common.psm1') -Force -ErrorAction Stop
            $freshPollToken = Get-E2EEngineToken `
                -TenantId     $ResultPollTenantId `
                -ClientId     $ResultPollClientId `
                -EngineAppId  $ResultPollResourceAppId `
                -ClientSecret (ConvertFrom-SecureStringPlain $ResultPollClientSecret)
            if ([string]::IsNullOrWhiteSpace($freshPollToken)) {
                throw "Failed to mint a result-poll token for client '$ResultPollClientId' against audience 'api://$ResultPollResourceAppId'. The messages HAVE been published and executed - re-run with '--phase poll --correlation-id $CorrelationId' once the credential is fixed rather than republishing."
            }

            # The deferred half of the pre-flight guard above: the token is brand new, so this
            # should never fire - if it does, the tenant is issuing tokens shorter-lived than
            # the poll budget and -ResultTimeoutSeconds must come down to match.
            $freshRemaining = Get-JwtRemainingLifetimeSeconds -Token $freshPollToken
            if ($null -ne $freshRemaining -and $freshRemaining -lt $ResultTimeoutSeconds) {
                throw "The freshly-minted result-poll token expires in ${freshRemaining}s, which is less than -ResultTimeoutSeconds $ResultTimeoutSeconds. It would expire mid-poll and mark every still-pending correlationId as a spurious HttpError. Lower -ResultTimeoutSeconds below ${freshRemaining}s, or raise the token lifetime for client '$ResultPollClientId'."
            }
            Write-Ok "Minted a fresh result-poll token ($freshRemaining s remaining) - polling now that every result is already recorded."

            $pollArgs = @(
                'run', '--project', $harnessProjectDir, '-c', 'Release', '--'
                '--mode', 'workflow-execution'
                '--phase', 'poll'
                '--correlation-id', $CorrelationId
                '--engine-base-url', $EngineBaseUrl
                '--result-poll-auth-token', $freshPollToken
                '--result-timeout-seconds', $ResultTimeoutSeconds
                '--message-count', $MessageCount
            )
            Write-Step "dotnet run (E2EHarness --phase poll, $MessageCount correlationId(s))"
            & dotnet @pollArgs 2>&1 | ForEach-Object { Write-Host "    $_" }
            $harnessExitCode = $LASTEXITCODE
        }
    } elseif ($VerifyWorkflowExecution) {
        $harnessArgs = @(
            'run', '--project', $harnessProjectDir, '-c', 'Release', '--'
            '--mode', 'workflow-execution'
            '--rabbitmq-management-uri', $mgmtUri
            '--rabbitmq-username', $rmqUser
            '--rabbitmq-password', $rmqPassword
            '--rabbitmq-vhost', $rmqVHost
            '--source-queue', $SourceQueueName
            '--correlation-id', $CorrelationId
            '--engine-base-url', $EngineBaseUrl
            '--result-poll-auth-token', (ConvertFrom-SecureStringPlain $ResultPollAuthToken)
            '--result-timeout-seconds', $ResultTimeoutSeconds
            '--message-count', $MessageCount
            '--publish-concurrency', $PublishConcurrency
        )
        if (-not [string]::IsNullOrWhiteSpace($WorkflowInputsJson)) { $harnessArgs += @('--workflow-inputs-json', $WorkflowInputsJson) }
        if ($MessageAuthToken -and $MessageAuthToken.Length -gt 0) { $harnessArgs += @('--message-auth-token', (ConvertFrom-SecureStringPlain $MessageAuthToken)) }
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
    # The late-mint branch above already ran its own publish/poll invocations (and set
    # $harnessExitCode from them), so there is no single combined command left to run here.
    if (-not ($VerifyWorkflowExecution -and $useLateMintedResultToken)) {
        Write-Step 'dotnet run (Warewolf.Execution.ServiceBusWorker.E2EHarness)'
        # Stream the harness's output live rather than buffering it and dumping it only
        # after the process exits — at load-test volumes this run can take many minutes
        # (build + publish + poll up to -ResultTimeoutSeconds), and a silent console during
        # that window is indistinguishable from a hang.
        & dotnet @harnessArgs 2>&1 | ForEach-Object { Write-Host "    $_" }
        $harnessExitCode = $LASTEXITCODE
    }

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

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4c — Database verification (dbo.jobs1, opt-in via -JobsDbConnectionString)
    # ════════════════════════════════════════════════════════════════════════
    # RabbitProcess.bite writes its own start/finish rows here via usp_jobs1_LogStart/
    # _LogProcessing/_LogFinished[WithError] — see Resources/rabbit/Provision-ShovelBridgeSchema.sql.
    # This is independent evidence from the harness's own /secure/servicebus-result poll: it
    # proves the WORKFLOW BODY actually ran, not just that the trigger recorded a result, and it
    # survives whatever bugs the result-poll path itself has had historically (see
    # docs/ShovelBridge-Architecture.md's 24-unexplained-HttpError and load-balanced-instance-404
    # entries). Always attempted when supplied (pass or fail), same as Phase 4b above.
    if ($JobsDbConnectionString) {
        Write-Phase $(if ($DbVerificationMode -eq 'ProfilerRecommendation') {
            'Phase 4c  Database verification (Profiler.ProfilerRecommendation)'
        } else {
            'Phase 4c  Database verification (dbo.jobs1)'
        })
        try {
            $loadTestScript = Join-Path $PSScriptRoot '..\..\Invoke-WwQueueLoadTest.ps1'
            if (-not (Test-Path -LiteralPath $loadTestScript)) {
                throw "Invoke-WwQueueLoadTest.ps1 not found at $loadTestScript (needed for its Invoke-WwSqlQuery helper)."
            }
            # Dot-sourcing shares scope, and that script's OWN param block declares a
            # [Nullable[int]] $MessageCount (no default) - since it isn't passed here, PowerShell
            # binds it to $null in this shared scope, clobbering THIS script's -MessageCount value.
            # That silently turned every reconciliation query below into "TOP ()", which SQL
            # Server rejects with "Incorrect syntax near ')'" (build 30636/30638). Save/restore
            # around the call rather than trust that an unused param is harmless, and cover
            # EVERY shared name (-LoadFunctionsOnly too), not just the one that bit us - the
            # same AST-diff unit test that pins the Phase 0 dot-source pins this call site.
            $savedMessageCount      = $MessageCount
            $savedLoadFunctionsOnly = $LoadFunctionsOnly
            . $loadTestScript -LoadFunctionsOnly
            $MessageCount      = $savedMessageCount
            $LoadFunctionsOnly = $savedLoadFunctionsOnly

            $jobsDbConnStringPlain = ConvertFrom-SecureStringPlain $JobsDbConnectionString
            $fromUtcSql            = $runStartUtc.ToString('yyyy-MM-ddTHH:mm:ss.fffZ')

            if ($DbVerificationMode -eq 'ProfilerRecommendation') {
            # ── ProfilerMain: reconcile Profiler.ProfilerRecommendation per message ──────
            # ProfilerMain writes NO dbo.jobs1 rows at all, so the Jobs1 mode below would
            # report "usp_jobs1_Summary returned no rows" for every run. Its per-message key
            # is MandateId, which the pipeline templates as {correlationGuid} so each message
            # carries a distinct, run-scoped uniqueidentifier.
            #
            # The GUID shape MUST match WorkflowInputTemplate.ToCorrelationGuid() in the E2E
            # harness: 26 hex characters of this run's correlationId prefix followed by the
            # 6-digit message sequence, dashed 8-4-4-4-12. Both sides are pinned by unit
            # tests — if you change one, change the other.
            #
            # Run-scoping is load-bearing, not cosmetic: Profiler.usp_InsertProfilerRecommendation
            # is an upsert keyed on (MandateId, ForPaymentCycle), so a constant MandateId would
            # return AlreadyExists=1 on every later run, insert nothing, and make a healthy run
            # look like $MessageCount lost messages.
            $guidStem    = Get-WwCorrelationGuidStem -CorrelationIdPrefix $CorrelationId
            $escapedStem = $guidStem.Replace("'", "''")

            Write-Step "Reconciling Profiler.ProfilerRecommendation for MandateId '${guidStem}<NNNNNN>' (expecting $MessageCount)"
            $profilerRows = Invoke-WwSqlQuery -ConnectionString $jobsDbConnStringPlain -Query @"
;WITH Nums AS (
    SELECT TOP ($MessageCount) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS SeqNum
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
),
Expected AS (
    SELECT SeqNum,
           CONVERT(uniqueidentifier, '$escapedStem' + RIGHT('00000' + CAST(SeqNum AS varchar(6)), 6)) AS MandateId
    FROM Nums
),
Recs AS (
    SELECT MandateId, COUNT(*) AS RowsForMandate
    FROM Profiler.ProfilerRecommendation WITH (NOLOCK)
    GROUP BY MandateId
),
Scores AS (
    SELECT MandateId, COUNT(*) AS ScoreRows
    FROM Profiler.MandateScores WITH (NOLOCK)
    GROUP BY MandateId
)
SELECT e.SeqNum                        AS SeqNum,
       ISNULL(r.RowsForMandate, 0)     AS RowsForMandate,
       ISNULL(sc.ScoreRows, 0)         AS ScoreRows
FROM Expected e
LEFT JOIN Recs   r  ON r.MandateId  = e.MandateId
LEFT JOIN Scores sc ON sc.MandateId = e.MandateId
ORDER BY e.SeqNum;
"@
            if (-not $profilerRows -or @($profilerRows).Count -eq 0) {
                throw 'Profiler.ProfilerRecommendation reconciliation query returned no rows.'
            }

            $allRows      = @($profilerRows)
            $missingSeq   = @($allRows | Where-Object { [int]$_.RowsForMandate -eq 0 } | ForEach-Object { [int]$_.SeqNum })
            $duplicateSeq = @($allRows | Where-Object { [int]$_.RowsForMandate -gt 1 } | ForEach-Object { [int]$_.SeqNum })
            $writtenCount = $MessageCount - $missingSeq.Count
            $scoredCount  = @($allRows | Where-Object { [int]$_.ScoreRows -gt 0 }).Count

            Write-Host "  Profiler.ProfilerRecommendation reconciliation (expected $MessageCount):" -ForegroundColor White
            Write-Host "    Recommendation written             : $writtenCount" -ForegroundColor White
            Write-Host "    Missing (no row for that MandateId) : $($missingSeq.Count)" -ForegroundColor White
            Write-Host "    Duplicate rows for one MandateId    : $($duplicateSeq.Count)" -ForegroundColor White
            Write-Host "    Also scored (MandateScores rows)    : $scoredCount" -ForegroundColor White

            if ($missingSeq.Count -eq 0 -and $duplicateSeq.Count -eq 0) {
                Write-Ok "Profiler.ProfilerRecommendation confirms all $MessageCount messages produced exactly one recommendation."
            } else {
                if ($missingSeq.Count -gt 0) {
                    $shown = ($missingSeq | Select-Object -First 20) -join ', '
                    $more  = if ($missingSeq.Count -gt 20) { " (+$($missingSeq.Count - 20) more)" } else { '' }
                    Write-Note "Sequence numbers with no recommendation (never reached ProfilerMain's own DB activity): $shown$more"
                }
                if ($duplicateSeq.Count -gt 0) {
                    $shownDup = ($duplicateSeq | Select-Object -First 20) -join ', '
                    Write-Note "Sequence numbers with MORE THAN ONE recommendation row: $shownDup — usp_InsertProfilerRecommendation is an upsert keyed on (MandateId, ForPaymentCycle), so this indicates a real defect, not contention."
                }
                if ($scoredCount -lt $writtenCount) {
                    Write-Note "$($writtenCount - $scoredCount) mandate(s) got a recommendation but no Profiler.MandateScores row — the scoring leg did not complete for them."
                }
                Write-Note "Profiler.ProfilerRecommendation did NOT confirm clean execution of all $MessageCount messages — see detail above."
            }
            } else {
            $escapedPrefix         = $JobsDbMessageContentPrefix.Replace("'", "''")

            Write-Step "EXEC dbo.usp_jobs1_Summary (scoped to this run, FromUtc=$fromUtcSql)"
            $summaryRows = Invoke-WwSqlQuery -ConnectionString $jobsDbConnStringPlain -Query @"
EXEC dbo.usp_jobs1_Summary @FromUtc = '$fromUtcSql', @StuckThresholdMinutes = '$JobsDbStuckThresholdMinutes';
"@
            if (-not $summaryRows -or @($summaryRows).Count -eq 0) {
                throw 'usp_jobs1_Summary returned no rows.'
            }
            $summaryRow = @($summaryRows)[0]

            Write-Step "Checking for sequence numbers with zero dbo.jobs1 rows (prefix '$JobsDbMessageContentPrefix')"
            $missingRows = Invoke-WwSqlQuery -ConnectionString $jobsDbConnStringPlain -Query @"
;WITH Nums AS (
    SELECT TOP ($MessageCount) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS SeqNum
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
),
Logged AS (
    SELECT DISTINCT TRY_CAST(RIGHT(MessageContent, CHARINDEX('-', REVERSE(MessageContent)) - 1) AS INT) AS SeqNum
    FROM dbo.jobs1
    WHERE MessageContent LIKE '$escapedPrefix%'
      AND TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) >= '$fromUtcSql'
)
SELECT Nums.SeqNum AS MissingSeqNum
FROM Nums
WHERE NOT EXISTS (SELECT 1 FROM Logged l WHERE l.SeqNum = Nums.SeqNum)
ORDER BY Nums.SeqNum;
"@
            $missingSeqNums = @($missingRows | ForEach-Object { [int]$_.MissingSeqNum })

            $verdict = Get-WwJobsDbVerdict -SummaryRow $summaryRow -ExpectedMessageCount $MessageCount -MissingSeqNums $missingSeqNums

            Write-Host "  dbo.jobs1 reconciliation (expected $($verdict.Expected)):" -ForegroundColor White
            Write-Host "    Succeeded (>=1 FINISHED)        : $($verdict.Succeeded)" -ForegroundColor White
            Write-Host "    Missing (zero jobs1 rows at all): $($verdict.MissingCount)" -ForegroundColor White
            Write-Host "    Duplicate successes (bug)       : $($verdict.DuplicateSuccessCount)" -ForegroundColor White
            Write-Host "    Still incomplete                : $($verdict.StillIncomplete)" -ForegroundColor White
            Write-Host "    Stuck past threshold            : $($verdict.StuckPastThreshold)" -ForegroundColor White

            if ($verdict.IsClean) {
                Write-Ok "dbo.jobs1 confirms all $($verdict.Expected) messages were delivered and succeeded exactly once."
            } else {
                if ($verdict.MissingCount -gt 0) {
                    $shown = ($verdict.MissingSeqNums | Select-Object -First 20) -join ', '
                    $more  = if ($verdict.MissingSeqNums.Count -gt 20) { " (+$($verdict.MissingSeqNums.Count - 20) more)" } else { '' }
                    Write-Note "Missing sequence numbers (never reached RabbitProcess's own DB activity at all): $shown$more"
                }
                if ($verdict.DuplicateSuccessCount -gt 0) {
                    Write-Note "Fetching duplicate-success detail (usp_jobs1_GetDuplicateSuccesses)..."
                    $dupRows = Invoke-WwSqlQuery -ConnectionString $jobsDbConnStringPlain -Query @"
EXEC dbo.usp_jobs1_GetDuplicateSuccesses @FromUtc = '$fromUtcSql';
"@
                    $dupRows | Format-Table -AutoSize | Out-String -Width 160 | Write-Host
                }
                if (($verdict.StillIncomplete -gt 0) -or ($verdict.StuckPastThreshold -gt 0)) {
                    Write-Note 'Fetching non-FINISHED row detail...'
                    $notFinishedRows = Invoke-WwSqlQuery -ConnectionString $jobsDbConnStringPlain -Query @"
SELECT TRY_CAST(RIGHT(MessageContent, CHARINDEX('-', REVERSE(MessageContent)) - 1) AS INT) AS SeqNum,
       Status, AttemptNumber, StartedAtUtc, FinishedAtUtc, ErrorCode, ErrorMessage
FROM dbo.jobs1
WHERE MessageContent LIKE '$escapedPrefix%'
  AND TRY_CONVERT(DATETIME2(3), CreatedAtUtc, 127) >= '$fromUtcSql'
  AND Status <> 'FINISHED'
ORDER BY SeqNum, TRY_CONVERT(INT, AttemptNumber) DESC;
"@
                    $notFinishedRows | Format-Table -AutoSize | Out-String -Width 160 | Write-Host
                }
                Write-Note "dbo.jobs1 did NOT confirm clean delivery of all $($verdict.Expected) messages — see detail above."
            }
            }
        } catch {
            Write-Note "Phase 4c database verification itself failed (non-fatal, does not affect the test result): $($_.Exception.Message)"
        }
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4d — Dead-letter reconciliation (WOLF-8512, build 30658)
    # ════════════════════════════════════════════════════════════════════════
    # Every message this run publishes must end in exactly one of three observable states:
    # succeeded (a result + its DB row), dead-lettered (countable here), or still in flight.
    # A message that is in NONE of them has been destroyed, and that is invisible without
    # this check — which is precisely how the auto-complete defect survived builds 30649
    # through 30658 while the harness reported only "never got a result".
    #
    # This is reported on pass AND fail: a clean 1000/1000 with a non-zero DLQ delta is
    # still a finding (messages failed and were retried into success, or something else
    # dead-lettered), and a failing run with a delta SMALLER than its loss count is the
    # silent-destruction signature itself.
    if ($null -ne $dlqBaseline) {
        Write-Phase 'Phase 4d  Dead-letter reconciliation'
        $dlqFinal = Get-WwDeadLetterCount -NamespaceName $ServiceBusNamespaceName -ResourceGroup $ServiceBusResourceGroup -QueueName $DestinationQueueName
        if ($null -eq $dlqFinal) {
            Write-Note 'Could not read the final dead-letter count — skipping reconciliation (does not affect the test result).'
        } else {
            $dlqDelta = $dlqFinal - $dlqBaseline
            Write-Host "  Dead-letter baseline : $dlqBaseline" -ForegroundColor White
            Write-Host "  Dead-letter final    : $dlqFinal" -ForegroundColor White
            Write-Host "  Delta (this run)     : $dlqDelta" -ForegroundColor White

            if ($harnessExitCode -eq 0) {
                if ($dlqDelta -eq 0) {
                    Write-Ok "No messages were dead-lettered — all $MessageCount accounted for as successes."
                } else {
                    Write-Note "All executions succeeded, but $dlqDelta message(s) still reached the dead-letter queue. Investigate: a message should not be both dead-lettered and successful."
                }
            } else {
                # The harness failed. Whatever it could not account for should be sitting in
                # the DLQ; anything missing from BOTH was destroyed in flight.
                if ($dlqDelta -eq 0) {
                    Write-Note 'The harness reported losses but NOTHING was dead-lettered — the missing messages were destroyed in flight, not failed. This is the WOLF-8512 auto-complete signature: check that ServiceBusWorkflowTriggerFunction still declares AutoCompleteMessages = false, and that the host log line "The ''AutoCompleteMessages'' option has been overriden to ..." reports False.'
                } else {
                    Write-Note "The harness reported losses and $dlqDelta message(s) were dead-lettered — those failures are at least accounted for. Inspect the DLQ entries' reason/description for the terminal cause."
                }
            }
        }
    } else {
        Write-Note 'Phase 4d dead-letter reconciliation skipped (-ServiceBusNamespaceName / -ServiceBusResourceGroup not supplied, or the baseline read failed).'
    }

    if ($harnessExitCode -ne 0) {
        throw "E2E harness reported failure (exit code $harnessExitCode). See output above."
    }

    Write-Phase 'ShovelBridge E2E test PASSED'
    if ($VerifyWorkflowExecution) {
        Write-Host "  Mode          : $DestinationMode (VerifyWorkflowExecution)" -ForegroundColor White
        Write-Host "  Source        : $SourceQueueName (RabbitMQ)" -ForegroundColor White
        Write-Host "  Destination   : $DestinationQueueName (Service Bus, Managed Identity trigger)" -ForegroundColor White
        Write-Host "  Workflow      : (fixed by the deployed trigger's Settings/servicebustrigger.bite)" -ForegroundColor White
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
