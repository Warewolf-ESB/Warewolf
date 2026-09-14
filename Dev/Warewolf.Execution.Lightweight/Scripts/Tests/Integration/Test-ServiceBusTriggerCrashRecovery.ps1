#Requires -Version 7.0
<#
.SYNOPSIS
    Integration test for WOLF-8512: proves that killing the Execution Engine's host process
    mid-execution leaves the Service Bus trigger's claim row in the exact "stalled" shape the
    silent-death diagnostic breadcrumb is designed to capture.

.DESCRIPTION
    The 1000-message ShovelBridge load test has repeatedly produced a handful of correlation
    ids that are claimed and then simply vanish — no result, no dead-letter (see
    docs/ShovelBridge-Architecture.md's running log). Build 30655 (2026-09-13) narrowed that
    to a worker-process crash mid-execution: the claim row froze at a non-terminal stage while
    the instance heartbeat carried on under a NEW process id.

    That scenario was only ever reproduced by hand. This script automates it against REAL
    containers (not mocks), so the behaviour the breadcrumb depends on is CI-enforced:

      1. Start Azure SQL Edge — the Hangfire store the trigger writes its claim rows to — and
         a deliberately slow local HTTP endpoint for the triggered workflow to block on.
      2. Start the Azure Service Bus emulator with the trigger's queue.
      3. Stage a throwaway deploy of the engine: its own build output, overlaid with test
         Settings/Resources (never the committed ones).
      4. Start the engine (func start) and send ONE message straight to the queue via the
         E2EHarness's --mode servicebus-send (no RabbitMQ/Shovel — this test makes no claim
         about the bridge, so putting one in the path would only add failure modes).
      5. Wait until the workflow has actually called the slow endpoint — i.e. execution is
         genuinely in flight — then kill the engine's whole process tree.
      6. ASSERT against Hangfire: a claim row exists, its lastStage is non-terminal, and no
         sbtrigger:result row was ever written.

.NOTES
    Why a slow HTTP endpoint holds the workflow open, rather than a command-line "ping": the
    Service Bus emulator and SQL Edge images are Linux-only, so this has to run on a Linux
    agent (the same constraint the ShovelBridgeE2ETest job documents), and
    DsfExecuteCommandLineActivity hardcodes cmd.exe
    (Dev2.Activities/Activities/DsfExecuteCommandLineActivity.cs). Answering the committed
    httpbin WebSource slowly needs no new workflow, no new source, and no shell - the workflow
    and source are both used exactly as shipped.

    All SQL is issued with `docker exec ... sqlcmd` rather than a client library, so the agent
    needs no SQL client of its own and behaves identically on Windows and Linux.

    Containers/networks are suffixed with a per-run id so a local run alongside a CI run never
    collides, and are torn down in a `finally` unless -SkipTeardown is passed.

.PARAMETER DelaySeconds
    How long the slow endpoint sits on the workflow's request before answering. This is the
    window the script has to kill the host inside; the default is deliberately generous
    because a cold CI agent is much slower than a dev box.
.PARAMETER SkipTeardown
    Leave containers, the staged deploy and the engine log in place for inspection.
#>
[CmdletBinding()]
param(
    [string] $SqlEdgeImage = 'mcr.microsoft.com/azure-sql-edge:latest',
    [string] $ServiceBusEmulatorImage = 'mcr.microsoft.com/azure-messaging/servicebus-emulator:latest',
    [securestring] $SqlEdgeSaPassword,

    [string] $QueueName = 'wwexecution-crash-recovery-test',

    # A workflow already shipped in the engine's own Resources and already exercised by CI,
    # rather than a purpose-built one: what makes it slow here is the deliberately slow HTTP
    # endpoint this script stands up, not anything about the workflow itself. This one takes no
    # inputs at all, so a trigger message needs no payload beyond its correlation id.
    [string] $WorkflowName = 'TC015_Get_MapsOrigin_ReturnsIP',
    [string] $WorkflowSourceRelativePath = 'Resources/tools/http get/TC015_Get_MapsOrigin_ReturnsIP.bite',

    # Must match the committed httpbin WebSource the workflow above resolves
    # (Address=http://localhost:4000), which this script deliberately does not edit.
    [int] $SlowEndpointPort = 4000,

    [ValidateRange(5, 600)]
    [int] $DelaySeconds = 20,

    [int] $SqlHostPort = 1433,
    [int] $AmqpHostPort = 5672,
    [int] $EmulatorHttpPort = 5300,

    [string] $FuncExePath = 'func',
    [int] $EngineStartupTimeoutSeconds = 300,

    [switch] $SkipTeardown,

    # Test hook: dot-source the helpers without standing up a single container, so
    # Tests/Integration/Test-ServiceBusTriggerCrashRecovery.Tests.ps1 can unit-test them
    # offline. Same convention as Test-ShovelBridgeE2E.ps1 and the Deploy-Ww* scripts.
    [switch] $LoadFunctionsOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Pinned explicitly because the default flipped between PowerShell 7.x releases: with it on,
# a non-zero native exit code throws, which would turn the deliberately-failing readiness
# probes below (sqlcmd against a still-starting SQL Edge, pgrep with no children) into hard
# errors. Every native exit code this script actually cares about is checked by hand.
$PSNativeCommandUseErrorActionPreference = $false

function Write-Phase {
    param([string] $Title)
    Write-Host ''
    Write-Host ('=' * 76) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ('=' * 76) -ForegroundColor Cyan
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
        [double] $DelaySeconds = 2
    )
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        if (& $Condition) { return $true }
        Start-Sleep -Seconds $DelaySeconds
    }
    Write-Note "Timed out waiting for: $Description (after $($MaxAttempts * $DelaySeconds)s)"
    return $false
}

function Stop-ProcessTree {
    <#
    Kills a process AND its descendants. The engine runs as `func` (the host) plus a separate
    .NET isolated worker child, and the claim/execution code we are interrupting lives in the
    CHILD — killing only the parent would leave the very process under test alive. There is no
    cross-platform primitive for this in PowerShell, hence the per-OS split.
    #>
    param([Parameter(Mandatory)][int] $ProcessId)

    if ($IsWindows) {
        & taskkill /F /T /PID $ProcessId 2>&1 | Out-Null
        return
    }

    $descendants = [System.Collections.Generic.List[int]]::new()
    $frontier = [System.Collections.Generic.Queue[int]]::new()
    $frontier.Enqueue($ProcessId)
    while ($frontier.Count -gt 0) {
        $current = $frontier.Dequeue()
        $descendants.Add($current)
        $children = (& pgrep -P $current 2>$null)
        foreach ($child in $children) {
            if ($child) { $frontier.Enqueue([int]$child) }
        }
    }
    # Children first so a parent cannot respawn/adopt anything while we work upwards.
    [int[]] $ordered = $descendants.ToArray()
    [array]::Reverse($ordered)
    foreach ($target in $ordered) { & kill -9 $target 2>$null | Out-Null }
}

function Invoke-SqlEdgeQuery {
    <#
    Runs a query inside the SQL Edge container itself — the agent needs no SQL client, and the
    behaviour is identical on Windows and Linux. Returns raw stdout lines; callers parse.
    #>
    param(
        [Parameter(Mandatory)][string] $ContainerName,
        [Parameter(Mandatory)][string] $SaPassword,
        [Parameter(Mandatory)][string] $Query,
        [string] $Database = 'master'
    )
    $output = & docker exec $ContainerName /opt/mssql-tools/bin/sqlcmd `
        -S localhost -U sa -P $SaPassword -d $Database -b -h -1 -W -s '|' -Q $Query 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd failed (exit $LASTEXITCODE) for query [$Query]: $($output -join [Environment]::NewLine)"
    }
    return $output
}

function Invoke-DotnetBuild {
    <#
    Builds a project quietly but keeps the output so it can be echoed when the build fails.
    A bare `dotnet build ... | Out-Null` leaves a CI failure undiagnosable — the pipeline log
    for build 30657 carried nothing but "exit 1", while the real cause was an MSB3030 from a
    case-mismatched csproj path that only bites on the Linux agent. Errors are surfaced in
    preference to the tail because `-v quiet` still emits this solution's ~850 warnings, which
    would otherwise push the single error line out of view.
    #>
    param(
        [Parameter(Mandatory)][string] $ProjectPath,
        [Parameter(Mandatory)][string] $Description,
        [int] $TailLines = 40
    )
    $buildOutputLines = & dotnet build $ProjectPath -c Debug --nologo -v quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        $buildExitCode = $LASTEXITCODE
        $errorLines = @($buildOutputLines | Where-Object { $_ -match ':\s*(error|fatal)\s' })
        $detail = if ($errorLines.Count -gt 0) { $errorLines } else { @($buildOutputLines) }
        $detail | Select-Object -Last $TailLines | ForEach-Object { Write-Host "      $_" }
        # The first error also goes in the exception message: Azure DevOps surfaces only that
        # in the step's error summary, which is all a reader sees without opening the raw log.
        $message = "dotnet build failed for $Description (exit $buildExitCode)."
        if ($errorLines.Count -gt 0) { $message += " First error: $(($errorLines[0] | Out-String).Trim())" }
        throw "$message See the build output above."
    }
}

function Get-WwEngineBuildOutputPath {
    <#
    Resolves where `dotnet build -c Debug` leaves the engine — the folder this script stages the
    throwaway deploy from, and the one carrying the .azurefunctions metadata `func start` needs.
    Chained Join-Path keeps the separator platform-correct on the Linux CI agent.

    Deliberately NOT bin/output: that spelling only ever resolved on a dev box still holding a
    stale publish there, and never exists on a clean checkout or a CI agent (build 30657).
    #>
    param(
        [Parameter(Mandatory)][string] $EngineDir,
        [string] $Configuration = 'Debug',
        [string] $TargetFramework = 'net10.0'
    )
    return Join-Path $EngineDir 'bin' | Join-Path -ChildPath $Configuration | Join-Path -ChildPath $TargetFramework
}

if ($LoadFunctionsOnly) { return }

# ── Paths ────────────────────────────────────────────────────────────────────
$engineDir    = Resolve-Path (Join-Path $PSScriptRoot '..\..\..')          # Warewolf.Execution.Lightweight
$devRoot      = Resolve-Path (Join-Path $engineDir '..')                   # Dev
$harnessDir   = Join-Path $devRoot 'Warewolf.Execution.ServiceBusWorker.E2EHarness'

if (-not (Test-Path -LiteralPath $harnessDir)) { throw "E2E harness project not found at $harnessDir" }

$runId                 = [Guid]::NewGuid().ToString('N').Substring(0, 8)
$networkName           = "sbcrash-net-$runId"
$sqlEdgeContainerName  = "sbcrash-sqledge-$runId"
$emulatorContainerName = "sbcrash-sbemulator-$runId"
$correlationId         = "sbcrash-$runId"
$stagingDir            = Join-Path ([System.IO.Path]::GetTempPath()) "sbcrash-engine-$runId"
$engineLogPath         = Join-Path ([System.IO.Path]::GetTempPath()) "sbcrash-engine-$runId.log"
$emulatorConfigPath    = Join-Path ([System.IO.Path]::GetTempPath()) "sbcrash-sbconfig-$runId.json"
$requestMarkerPath     = Join-Path ([System.IO.Path]::GetTempPath()) "sbcrash-requests-$runId.txt"
$hangfireDatabase      = 'hangfiredb'

$saPassword = if ($null -eq $SqlEdgeSaPassword -or $SqlEdgeSaPassword.Length -eq 0) {
    New-RandomPassword
} else {
    ConvertFrom-SecureStringPlain $SqlEdgeSaPassword
}

$containersStarted = [System.Collections.Generic.List[string]]::new()
$engineProcess = $null
$slowEndpointJob = $null

try {
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 0  Pre-flight'
    # ════════════════════════════════════════════════════════════════════════
    foreach ($tool in @('docker', 'dotnet')) {
        if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "'$tool' is required but was not found on PATH." }
    }
    $funcCommand = Get-Command $FuncExePath -ErrorAction SilentlyContinue
    if (-not $funcCommand) {
        throw "Azure Functions Core Tools ('$FuncExePath') is required but was not found. Pass -FuncExePath to point at it."
    }
    # Start-Process needs a real executable. A bare "func" from an npm install is a set of
    # shims (func / func.cmd / func.ps1) and Get-Command prefers the .ps1 - which Start-Process
    # rejects with "%1 is not a valid Win32 application", as does the extensionless one. Prefer
    # a sibling .exe/.cmd on Windows; on Linux Get-Command already yields the real binary.
    $resolvedFuncPath = if ($funcCommand.Source) { $funcCommand.Source } else { $FuncExePath }
    if ($IsWindows -and $resolvedFuncPath -match '\.ps1$') {
        $shimDir  = Split-Path -Parent $resolvedFuncPath
        $shimBase = [System.IO.Path]::GetFileNameWithoutExtension($resolvedFuncPath)
        foreach ($extension in @('.exe', '.cmd', '.bat')) {
            $candidate = Join-Path $shimDir ($shimBase + $extension)
            if (Test-Path -LiteralPath $candidate) { $resolvedFuncPath = $candidate; break }
        }
    }
    if ($resolvedFuncPath -match '\.ps1$') {
        throw "Resolved '$FuncExePath' to the PowerShell shim '$resolvedFuncPath', which cannot be launched directly. Pass -FuncExePath pointing at func.cmd or the func executable."
    }
    Write-Ok "Run ID: $runId (containers/network/staging are suffixed with this to avoid collisions)"
    Write-Ok "CorrelationId under test: $correlationId"

    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Start SQL Edge (Hangfire claim store)'
    # ════════════════════════════════════════════════════════════════════════
    Write-Step "Creating docker network '$networkName'"
    docker network create $networkName | Out-Null

    Write-Step "Starting Azure SQL Edge container '$sqlEdgeContainerName' on port $SqlHostPort"
    docker run -d --name $sqlEdgeContainerName --network $networkName `
        -p "${SqlHostPort}:1433" `
        -e 'ACCEPT_EULA=Y' -e "MSSQL_SA_PASSWORD=$saPassword" `
        $SqlEdgeImage | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "docker run failed for Azure SQL Edge (exit $LASTEXITCODE)." }
    $containersStarted.Add($sqlEdgeContainerName)

    $sqlReady = Wait-ForCondition -Description 'Azure SQL Edge' -MaxAttempts 45 -DelaySeconds 2 -Condition {
        docker exec $sqlEdgeContainerName /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $saPassword -Q 'SELECT 1' 2>&1 | Out-Null
        $LASTEXITCODE -eq 0
    }
    if (-not $sqlReady) { throw 'Azure SQL Edge never became ready.' }
    Write-Ok 'Azure SQL Edge is ready.'

    Write-Step "Creating database '$hangfireDatabase'"
    Invoke-SqlEdgeQuery -ContainerName $sqlEdgeContainerName -SaPassword $saPassword -Query @"
IF DB_ID('$hangfireDatabase') IS NULL CREATE DATABASE [$hangfireDatabase];
"@ | Out-Null
    Write-Ok 'Hangfire database created.'

    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1b  Start the slow HTTP endpoint'
    # ════════════════════════════════════════════════════════════════════════
    # This is what holds the workflow open. The shipped workflow calls the committed httpbin
    # WebSource at localhost:$SlowEndpointPort; answering that call slowly parks the execution
    # inside IWorkflowExecutor.Execute for as long as this test needs, without the workflow,
    # the source, or the engine being modified in any way.
    #
    # Recording each request is also what stops the test passing vacuously: a workflow that
    # stalled for some unrelated reason (never reaching its web activity) would otherwise leave
    # exactly the same claim-row signature as the crash being proven.
    Write-Step "Starting slow endpoint on http://localhost:$SlowEndpointPort/ (responds after ${DelaySeconds}s)"
    $slowEndpointJob = Start-Job -Name "sbcrash-endpoint-$runId" -ScriptBlock {
        param([int] $Port, [string] $MarkerPath, [int] $DelaySeconds)
        $listener = [System.Net.HttpListener]::new()
        $listener.Prefixes.Add("http://localhost:$Port/")
        $listener.Start()
        try {
            while ($listener.IsListening) {
                $context = $listener.GetContext()
                Add-Content -LiteralPath $MarkerPath -Value ('{0:o} {1}' -f [DateTimeOffset]::UtcNow, $context.Request.Url.AbsolutePath)
                Start-Sleep -Seconds $DelaySeconds
                $payload = [System.Text.Encoding]::UTF8.GetBytes('{"origin":"127.0.0.1","url":"http://localhost/get"}')
                $context.Response.ContentType = 'application/json'
                $context.Response.StatusCode = 200
                $context.Response.OutputStream.Write($payload, 0, $payload.Length)
                $context.Response.Close()
            }
        } finally {
            $listener.Stop()
            $listener.Close()
        }
    } -ArgumentList $SlowEndpointPort, $requestMarkerPath, $DelaySeconds

    $endpointReady = Wait-ForCondition -Description 'slow HTTP endpoint' -MaxAttempts 30 -DelaySeconds 1 -Condition {
        try {
            $probe = [System.Net.Sockets.TcpClient]::new()
            $probe.Connect('localhost', $SlowEndpointPort)
            $probe.Close()
            $true
        } catch { $false }
    }
    if (-not $endpointReady) {
        Receive-Job -Job $slowEndpointJob -ErrorAction SilentlyContinue | Write-Host
        throw "The slow HTTP endpoint never started listening on port $SlowEndpointPort (is something else using it?)."
    }
    Write-Ok 'Slow HTTP endpoint is listening.'

    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Start Service Bus emulator'
    # ════════════════════════════════════════════════════════════════════════
    $emulatorConfig = @{
        UserConfig = @{
            Namespaces = @(
                @{
                    Name   = 'sbemulatorns'
                    Queues = @(
                        @{
                            Name       = $QueueName
                            Properties = @{
                                DeadLetteringOnMessageExpiration    = $true
                                DefaultMessageTimeToLive            = 'PT1H'
                                DuplicateDetectionHistoryTimeWindow = 'PT20S'
                                ForwardDeadLetteredMessagesTo       = ''
                                ForwardTo                           = ''
                                LockDuration                        = 'PT1M'
                                MaxDeliveryCount                    = 10
                                RequiresDuplicateDetection          = $false
                                RequiresSession                     = $false
                            }
                        }
                    )
                }
            )
            Logging = @{ Type = 'File' }
        }
    }
    $emulatorConfig | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $emulatorConfigPath -Encoding UTF8

    Write-Step "Starting Service Bus emulator container '$emulatorContainerName' (queue '$QueueName')"
    docker run -d --name $emulatorContainerName --network $networkName `
        -e 'ACCEPT_EULA=Y' -e "SQL_SERVER=$sqlEdgeContainerName" -e "MSSQL_SA_PASSWORD=$saPassword" -e "EMULATOR_HTTP_PORT=$EmulatorHttpPort" `
        -p "${AmqpHostPort}:5672" -p "${EmulatorHttpPort}:$EmulatorHttpPort" `
        -v "${emulatorConfigPath}:/ServiceBus_Emulator/ConfigFiles/Config.json" `
        $ServiceBusEmulatorImage | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "docker run failed for the Service Bus emulator (exit $LASTEXITCODE)." }
    $containersStarted.Add($emulatorContainerName)

    # The emulator opens its AMQP port before it has finished creating entities against SQL
    # Edge, so port-reachability alone is not "ready" - wait for its own success line.
    $emulatorReady = Wait-ForCondition -Description 'Service Bus emulator startup' -MaxAttempts 60 -DelaySeconds 2 -Condition {
        (docker logs $emulatorContainerName 2>&1 | Select-String -SimpleMatch 'Emulator Service is Successfully Up!' -Quiet) -eq $true
    }
    if (-not $emulatorReady) {
        docker logs $emulatorContainerName 2>&1 | Select-Object -Last 40 | Write-Host
        throw 'Service Bus emulator never reported a successful startup.'
    }
    Write-Ok 'Service Bus emulator is ready.'

    $serviceBusConnectionString = "Endpoint=sb://localhost:$AmqpHostPort;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;"

    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Stage a throwaway engine deploy'
    # ════════════════════════════════════════════════════════════════════════
    Write-Step 'Building the Execution Engine'
    Invoke-DotnetBuild -ProjectPath (Join-Path $engineDir 'Warewolf.Execution.Lightweight.csproj') -Description 'the Execution Engine'

    Write-Step 'Building the E2E harness'
    Invoke-DotnetBuild -ProjectPath (Join-Path $harnessDir 'Warewolf.Execution.ServiceBusWorker.E2EHarness.csproj') -Description 'the E2E harness'
    # Invoked as a DLL rather than via `dotnet run`, which eats the first --mode before the
    # app ever sees it.
    $harnessDll = Join-Path $harnessDir 'bin' | Join-Path -ChildPath 'Debug' | Join-Path -ChildPath 'net10.0' | Join-Path -ChildPath 'ShovelBridgeE2EHarness.dll'
    if (-not (Test-Path -LiteralPath $harnessDll)) { throw "E2E harness assembly not found at $harnessDll" }

    $buildOutput = Get-WwEngineBuildOutputPath -EngineDir $engineDir
    if (-not (Test-Path -LiteralPath $buildOutput)) { throw "Engine build output not found at $buildOutput" }

    # Staged copy so the committed Settings/Resources are never mutated by a test run.
    Write-Step "Staging deploy at $stagingDir"
    New-Item -ItemType Directory -Path $stagingDir -Force | Out-Null
    Copy-Item -Path (Join-Path $buildOutput '*') -Destination $stagingDir -Recurse -Force

    $stagedSettings  = Join-Path $stagingDir 'Settings'
    $stagedResources = Join-Path $stagingDir 'Resources'
    New-Item -ItemType Directory -Path $stagedSettings, $stagedResources -Force | Out-Null

    $hangfireConnectionString = "Data Source=localhost,$SqlHostPort;Initial Catalog=$hangfireDatabase;User ID=sa;Password=$saPassword;Connection Timeout=30;TrustServerCertificate=True;Encrypt=False"

    # The engine resolves a trigger's WorkflowName through Resources/workflow-index.json, which
    # ships prebuilt and indexes neither the profiler subfolder nor anything else added after it
    # was generated. Resolution does fall back to a file of that name at the Resources ROOT, so
    # the shipped workflow is copied there rather than renaming/reindexing anything.
    $workflowSource = Join-Path $stagingDir ($WorkflowSourceRelativePath -replace '/', [System.IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $workflowSource)) {
        throw "Expected the shipped workflow at '$workflowSource' (from -WorkflowSourceRelativePath '$WorkflowSourceRelativePath') but it was not in the build output."
    }
    Write-Step "Staging workflow '$WorkflowName' at the Resources root"
    Copy-Item -LiteralPath $workflowSource -Destination (Join-Path $stagedResources "$WorkflowName.bite") -Force

    Write-Step 'Overlaying trigger + persistence settings'
    @{
        QueueName    = $QueueName
        WorkflowName = $WorkflowName
        TriggerId    = '00000000-0000-0000-0000-000000000000'
        Name         = 'ServiceBusCrashRecoveryTestTrigger'
        ResourceId   = '00000000-0000-0000-0000-000000000000'
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $stagedSettings 'servicebustrigger.bite') -Encoding UTF8

    @{
        Enable                  = $true
        PersistenceScheduler    = 'Hangfire'
        EncryptDataSource       = $false
        PrepareSchemaIfNecessary = $true
        UseAsServer             = $false
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $stagedSettings 'persistencesettings.json') -Encoding UTF8

    @"
<Source ID="cfe43056-b935-47ba-89b7-638c0ae76812" Name="hangfiredb" ResourceType="DbSource" IsValid="true" ConnectionString="$hangfireConnectionString" Type="DbSource" ServerType="SqlDatabase" ServerVersion="0.0.0.0" ServerID="51a58300-7e9d-4927-a57b-e5d700b11b55">
  <DisplayName>hangfiredb</DisplayName>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <TypeOf>DbSource</TypeOf>
  <VersionInfo DateTimeStamp="2026-09-13T00:00:00.0000000+00:00" Reason="Save" User="Test-ServiceBusTriggerCrashRecovery" VersionNumber="1" ResourceId="cfe43056-b935-47ba-89b7-638c0ae76812" VersionId="4df2fde5-253d-4391-be7f-a7be82719d4a" />
</Source>
"@ | Set-Content -LiteralPath (Join-Path $stagedSettings 'persistencesettingsdbsource.bite') -Encoding UTF8

    # WAREWOLF_SERVICEBUS_TRIGGER_QUEUE is normally injected into configuration from
    # servicebustrigger.bite before the host is built (HostBuilderExtensions.ConfigureWarewolf).
    # Under `func start` that injection does NOT satisfy the host's own %...% binding-placeholder
    # resolution, so the trigger fails to index with "'%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%' does
    # not resolve to a value" unless it is also present as a plain setting. Same value as above.
    @{
        IsEncrypted = $false
        Values = @{
            AzureWebJobsStorage                  = 'UseDevelopmentStorage=true'
            FUNCTIONS_WORKER_RUNTIME             = 'dotnet-isolated'
            ServiceBusConnection                 = $serviceBusConnectionString
            WAREWOLF_SERVICEBUS_TRIGGER_QUEUE    = $QueueName
            WAREWOLF_LICENSE_CHECK_ENABLED       = 'false'
            WAREWOLF_LOGGING_CONFIG              = '{"elasticsearch":false,"console":true}'
            EXECUTIONLOGLEVEL                    = '6'
        }
    } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $stagingDir 'local.settings.json') -Encoding UTF8
    Write-Ok 'Staged deploy ready.'

    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Start the engine'
    # ════════════════════════════════════════════════════════════════════════
    Write-Step "Starting '$resolvedFuncPath start' (log: $engineLogPath)"
    $engineProcess = Start-Process -FilePath $resolvedFuncPath -ArgumentList 'start' `
        -WorkingDirectory $stagingDir -PassThru -NoNewWindow `
        -RedirectStandardOutput $engineLogPath -RedirectStandardError "$engineLogPath.err"

    $listenerAttempts = [int][Math]::Ceiling($EngineStartupTimeoutSeconds / 2)
    $engineListening = Wait-ForCondition -Description 'engine Service Bus listener' -MaxAttempts $listenerAttempts -DelaySeconds 2 -Condition {
        (Test-Path -LiteralPath $engineLogPath) -and
        ((Get-Content -LiteralPath $engineLogPath -Raw -ErrorAction SilentlyContinue) -match 'ServiceBus listener started')
    }
    if (-not $engineListening) {
        Get-Content -LiteralPath $engineLogPath -Tail 60 -ErrorAction SilentlyContinue | Write-Host
        throw 'The engine never started its Service Bus listener.'
    }
    Write-Ok "Engine is listening on queue '$QueueName' (pid $($engineProcess.Id))."

    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 5  Send one message, then crash the engine mid-execution'
    # ════════════════════════════════════════════════════════════════════════
    Write-Step "E2EHarness --mode servicebus-send (correlationId '$correlationId')"
    & dotnet $harnessDll `
        --mode servicebus-send `
        --servicebus-connection-string $serviceBusConnectionString `
        --destination-queue $QueueName `
        --correlation-id $correlationId
    if ($LASTEXITCODE -ne 0) { throw "E2EHarness send failed (exit $LASTEXITCODE)." }

    # The engine logs this immediately before the call into IWorkflowExecutor.Execute, which is
    # where the delay procedure then holds it for -DelaySeconds. That is the window to kill in.
    $executing = Wait-ForCondition -Description 'engine to begin executing the workflow' -MaxAttempts 240 -DelaySeconds 0.5 -Condition {
        $log = Get-Content -LiteralPath $engineLogPath -Raw -ErrorAction SilentlyContinue
        $log -and ($log -match [regex]::Escape($correlationId)) -and ($log -match 'About to call IWorkflowExecutor\.Execute')
    }
    if (-not $executing) {
        Get-Content -LiteralPath $engineLogPath -Tail 60 -ErrorAction SilentlyContinue | Write-Host
        throw "The engine never reported starting execution for correlationId '$correlationId'."
    }

    # Killing on the log line alone is too early: resolving the web source and opening the
    # request all happen after it, and a kill landing in that gap stops the workflow BEFORE it
    # reaches the delay - which looks identical to the crash being tested but proves nothing.
    # The slow endpoint records each request as it arrives, so waiting for that is a positive
    # signal that execution is genuinely parked, with the rest of -DelaySeconds still to run.
    Write-Step 'Waiting for the workflow to call the slow endpoint'
    $insideDelay = Wait-ForCondition -Description 'workflow to call the slow endpoint' -MaxAttempts 180 -DelaySeconds 1 -Condition {
        (Test-Path -LiteralPath $requestMarkerPath) -and
        ((Get-Content -LiteralPath $requestMarkerPath -ErrorAction SilentlyContinue | Measure-Object).Count -ge 1)
    }
    if (-not $insideDelay) {
        Get-Content -LiteralPath $engineLogPath -Tail 60 -ErrorAction SilentlyContinue | Write-Host
        throw 'The workflow never called the slow endpoint, so there was no execution in flight to interrupt.'
    }
    Write-Ok 'Workflow is parked in the slow HTTP call.'

    $killedPid = $engineProcess.Id
    Write-Step "Killing the engine process tree (pid $killedPid) NOW - mid-execution"
    Stop-ProcessTree -ProcessId $killedPid
    $engineProcess = $null
    Write-Ok 'Engine killed.'

    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 6  Assert the stalled-claim signature'
    # ════════════════════════════════════════════════════════════════════════
    $claimKey  = "sbtrigger:claim:$correlationId"
    $resultKey = "sbtrigger:result:$correlationId"

    $claimRows = Invoke-SqlEdgeQuery -ContainerName $sqlEdgeContainerName -SaPassword $saPassword -Database $hangfireDatabase -Query @"
SET NOCOUNT ON;
SELECT [Field] + '=' + [Value] FROM HangFire.Hash WHERE [Key] = '$claimKey' ORDER BY [Field];
"@
    $claimFields = @{}
    foreach ($line in $claimRows) {
        $text = "$line".Trim()
        if ($text -and $text -notmatch '^\(' -and $text.Contains('=')) {
            $name, $value = $text.Split('=', 2)
            $claimFields[$name] = $value
        }
    }

    $resultRows = Invoke-SqlEdgeQuery -ContainerName $sqlEdgeContainerName -SaPassword $saPassword -Database $hangfireDatabase -Query @"
SET NOCOUNT ON;
SELECT COUNT(*) FROM HangFire.Hash WHERE [Key] = '$resultKey';
"@
    $resultCount = 0
    foreach ($line in $resultRows) {
        if ("$line".Trim() -match '^\d+$') { $resultCount = [int]("$line".Trim()); break }
    }

    $delayEnteredCount = if (Test-Path -LiteralPath $requestMarkerPath) {
        (Get-Content -LiteralPath $requestMarkerPath -ErrorAction SilentlyContinue | Measure-Object).Count
    } else { 0 }

    Write-Host ''
    Write-Host "  Claim row '$claimKey':" -ForegroundColor White
    if ($claimFields.Count -eq 0) {
        Write-Host '    (no fields)' -ForegroundColor White
    } else {
        foreach ($name in ($claimFields.Keys | Sort-Object)) {
            Write-Host ("    {0,-20}: {1}" -f $name, $claimFields[$name]) -ForegroundColor White
        }
    }
    Write-Host "    sbtrigger:result rows : $resultCount" -ForegroundColor White
    Write-Host "    slow-endpoint calls   : $delayEnteredCount" -ForegroundColor White
    Write-Host ''

    $failures = [System.Collections.Generic.List[string]]::new()

    if ($delayEnteredCount -lt 1) {
        $failures.Add("The workflow never called the slow endpoint, so it stalled for some reason other than the deliberate delay — this run does not prove a mid-execution crash.")
    }

    if (-not $claimFields.ContainsKey('claimedAtUtc')) {
        $failures.Add("No claim was recorded for '$correlationId' — the trigger never got as far as TryClaim, so this run proves nothing about a mid-execution crash.")
    }

    $lastStage = if ($claimFields.ContainsKey('lastStage')) { $claimFields['lastStage'] } else { '' }
    if (-not $lastStage) {
        $failures.Add("The claim row has no lastStage — the silent-death breadcrumb (RecordClaimStage) did not record a checkpoint.")
    } elseif ($lastStage -match '^ExecutionReturned') {
        $failures.Add("lastStage is '$lastStage' — execution completed before the kill landed, so the crash window was missed. Raise -DelaySeconds.")
    } elseif ($lastStage -notmatch '^(AboutToExecute|StillExecuting)') {
        $failures.Add("lastStage is '$lastStage' — expected a non-terminal AboutToExecute/StillExecuting checkpoint.")
    }

    if ($resultCount -ne 0) {
        $failures.Add("A result row exists for '$correlationId' — the execution resolved, so the message did not vanish mid-flight as this test requires.")
    }

    if ($failures.Count -gt 0) {
        foreach ($failure in $failures) { Write-Note $failure }
        throw "Stalled-claim assertions failed ($($failures.Count) failure(s))."
    }

    Write-Ok "Claim stalled at '$lastStage' with no result recorded — the crash signature reproduced exactly."
    if ($claimFields.ContainsKey('lastStageProcessId')) {
        Write-Ok "Recorded by process $($claimFields['lastStageProcessId']) (engine host pid was $killedPid)."
    }

    Write-Phase 'ServiceBus trigger crash-recovery test PASSED'
}
catch {
    Write-Note "ServiceBus trigger crash-recovery test FAILED: $($_.Exception.Message)"
    throw
}
finally {
    if ($engineProcess) {
        Write-Note "Stopping engine process tree (pid $($engineProcess.Id))"
        Stop-ProcessTree -ProcessId $engineProcess.Id
    }

    if ($slowEndpointJob) {
        Stop-Job -Job $slowEndpointJob -ErrorAction SilentlyContinue
        Remove-Job -Job $slowEndpointJob -Force -ErrorAction SilentlyContinue
    }

    if ($SkipTeardown) {
        Write-Note "Skipping teardown (-SkipTeardown). Containers: $($containersStarted -join ', ') ; network: $networkName ; staging: $stagingDir ; engine log: $engineLogPath"
    } else {
        Write-Phase 'Teardown'
        foreach ($name in $containersStarted) {
            Write-Step "Removing container '$name'"
            docker rm -f $name 2>&1 | Out-Null
        }
        Write-Step "Removing network '$networkName'"
        docker network rm $networkName 2>&1 | Out-Null
        foreach ($path in @($stagingDir, $engineLogPath, "$engineLogPath.err", $emulatorConfigPath)) {
            if (Test-Path -LiteralPath $path) { Remove-Item -LiteralPath $path -Recurse -Force -ErrorAction SilentlyContinue }
        }
        Write-Ok 'Teardown complete.'
    }
}
