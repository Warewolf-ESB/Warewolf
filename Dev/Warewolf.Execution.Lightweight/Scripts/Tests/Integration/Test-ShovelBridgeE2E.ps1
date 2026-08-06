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
    [switch] $SkipTeardown
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
        Write-Note "Could not verify destination queue existence via Service Bus management API: $($_.Exception.Message)"
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
    if ($null -eq $ExternalServiceBusConnectionString -or $ExternalServiceBusConnectionString.Length -eq 0) {
        throw '-ExternalServiceBusConnectionString is required when -DestinationMode ExternalServiceBus.'
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

                docker run -d --name $rabbitContainerName --network $networkName `
                    -e "RABBITMQ_DEFAULT_USER=$rmqUser" -e "RABBITMQ_DEFAULT_PASS=$rmqPassword" `
                    -v "${enabledPluginsTempFile}:/etc/rabbitmq/enabled_plugins" `
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
            Write-Note "Could not confirm destination queue '$DestinationQueueName' exists (connection string not in key-name/key form) — assuming it exists."
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
        'src-prefetch-count' = 5
        'dest-protocol'      = 'amqp10'
        'dest-uri'           = $shovelDestUri
        'dest-address'       = $DestinationQueueName
        'ack-mode'           = 'on-confirm'
        'reconnect-delay'    = 5
    }
    Write-Step "PUT /api/parameters/shovel/%2f/$ShovelName"
    Invoke-RabbitMqApi -Method Put -Path "/api/parameters/shovel/$vhostForApi/$([Uri]::EscapeDataString($ShovelName))" -Body @{ value = $shovelDefinition } -Mutating | Out-Null

    $lastShovelState = $null
    $shovelRunning = Wait-ForCondition -Description "shovel '$ShovelName' running" -MaxAttempts 15 -DelaySeconds 2 -Condition {
        $shovels = Invoke-RabbitMqApi -Method Get -Path "/api/shovels/$vhostForApi" -AllowFail
        $mine = @($shovels) | Where-Object { $_.name -eq $ShovelName }
        if ($mine) { $script:lastShovelState = $mine[0] }
        $mine -and $mine[0].state -eq 'running'
    }
    if (-not $shovelRunning) {
        # The management API only ever reports 'starting'/'running' for the
        # current snapshot — it doesn't retain the connect failure reason once
        # the shovel restarts. Surface what we DO have (last known state) plus a
        # pointer to where the real reason lives (broker logs / Admin > Shovel
        # Status), since a bare "did not reach running" gives no lead on WHICH
        # side (src vs dest) is failing.
        $stateDetail = if ($lastShovelState) { " Last observed state: $($lastShovelState | ConvertTo-Json -Compress)." } else { ' No shovel status was ever observed for this name — check the PUT above succeeded.' }
        throw "Shovel '$ShovelName' did not reach the 'running' state.$stateDetail Check the RabbitMQ broker logs (or Admin > Shovel Status in the management UI) for the connect failure reason — commonly a src-uri auth/permission failure, or a dest-uri (Service Bus) auth/network/queue-not-found failure."
    }
    Write-Ok "Shovel '$ShovelName' is running."

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4 — Run the harness (publish + verify)
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Publish + verify via E2EHarness'

    $harnessArgs = @(
        'run', '--project', $harnessProjectDir, '-c', 'Release', '--'
        '--rabbitmq-management-uri', $mgmtUri
        '--rabbitmq-username', $rmqUser
        '--rabbitmq-password', $rmqPassword
        '--source-queue', $SourceQueueName
        '--servicebus-connection-string', $serviceBusConnectionStringPlain
        '--destination-queue', $DestinationQueueName
        '--timeout-seconds', $HarnessTimeoutSeconds
    )
    Write-Step 'dotnet run (Warewolf.Execution.ServiceBusWorker.E2EHarness)'
    $harnessOutput = & dotnet @harnessArgs 2>&1
    $harnessExitCode = $LASTEXITCODE
    $harnessOutput | ForEach-Object { Write-Host "    $_" }

    if ($harnessExitCode -ne 0) {
        throw "E2E harness reported failure (exit code $harnessExitCode). See output above."
    }

    Write-Phase 'ShovelBridge E2E test PASSED'
    Write-Host "  Mode        : $DestinationMode" -ForegroundColor White
    Write-Host "  Source      : $SourceQueueName (RabbitMQ)" -ForegroundColor White
    Write-Host "  Destination : $DestinationQueueName (Service Bus)" -ForegroundColor White
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
        Write-Ok 'Teardown complete.'
    }
}
