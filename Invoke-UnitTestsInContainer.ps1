<#
.SYNOPSIS
    Runs Warewolf unit tests inside the target Azure Functions .NET 8 container.

.DESCRIPTION
    Mirrors what the CI pipeline does: compile test projects on the host with
    dotnet build, then mount the output into the exact production container image
    (mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0-appservice)
    and execute dotnet vstest there.

    This lets any developer verify tests pass in the real target runtime without
    pushing to the build server.

    Prerequisites
    -------------
    - Docker Desktop running
    - .NET 8 SDK installed on the host (for the build step)

.PARAMETER NoBuild
    Skip the dotnet build step and run against the last compiled binaries.
    Useful for iterating quickly after a successful build.

.PARAMETER RebuildImage
    Force a rebuild of the local Docker image even if it already exists.
    Use this after pulling a newer version of the base image.

.PARAMETER ExcludeProjects
    Test assembly names (without .dll) to skip. Defaults to the same set
    excluded by the CI Unit_Tests job.

.PARAMETER ExcludeCategories
    MSTest categories to exclude. Defaults to the same set excluded by CI.

.PARAMETER ImageName
    Local tag for the test-runner image. Defaults to 'warewolf-unit-tests:local'.

.PARAMETER Configuration
    Build configuration passed to dotnet build. Defaults to 'Debug'.

.EXAMPLE
    .\Invoke-UnitTestsInContainer.ps1
    Full build + test run.

.EXAMPLE
    .\Invoke-UnitTestsInContainer.ps1 -NoBuild
    Re-run tests without recompiling.

.EXAMPLE
    .\Invoke-UnitTestsInContainer.ps1 -RebuildImage -NoBuild
    Refresh the container image and re-run tests against existing binaries.
#>
[CmdletBinding()]
param(
    [switch]$NoBuild,
    [switch]$RebuildImage,

    [string[]]$ExcludeProjects = @(
        'Dev2.Integration.Tests',
        'Dev2.Studio.Core.Tests',
        'Dev2.Infrastructure.Tests',
        'Warewolf.UI.Tests',
        'Warewolf.Logger.Tests',
        'Warewolf.Studio.ViewModels.Tests',
        'Warewolf.Web.UI.Tests',
        'Warewolf.Storage.Tests',
        'Warewolf.Auditing.Tests',
        'Dev2.Activities.Designers.Tests',
        'Warewolf.Execution.Lightweight.Tests'
    ),

    [string[]]$ExcludeCategories = @(
        'CannotParallelize',
        'ResourceCatalog_LoadTests',
        'PluginRuntimeHandler',
        'GatherSystemInformation',
        'LocalSchedulerAdmin',
        'Multithread',
        'AnonymousRedis',
        'COMIPCSaxonCSandStudioTests',
        'WFWithRabbitMqConsumeTimeout5',
        'WarewolfCOMIPCClient_Deprecated'
    ),

    [string]$ImageName    = 'warewolf-unit-tests:local',
    [string]$Configuration = 'Debug',

    # Run test assemblies in parallel (one process per DLL).
    # Disabled by default because 30 assemblies in parallel easily exceeds
    # available RAM and causes the container to be OOM-killed (exit 137).
    # Enable only if your machine has plenty of free memory.
    [switch]$Parallel,

    # Cap container memory. Omit (leave empty) for no cap, which is the right
    # default for a developer desk. The CI pipeline uses '4g'.
    [string]$MemoryLimit = ''
)

$ErrorActionPreference = 'Stop'
$Root = $PSScriptRoot

function Write-Step([string]$Label) {
    Write-Host ("`n=== $Label ===") -ForegroundColor Cyan
}

# ---------------------------------------------------------------------------
# 1. Build (or reuse) the Docker test-runner image
# ---------------------------------------------------------------------------
Write-Step 'Docker Image'

# docker image ls -q exits 0 whether the image exists or not, so it never
# triggers $ErrorActionPreference = 'Stop'. Inspect is avoided on purpose.
$imageExists = [bool](docker image ls -q $ImageName 2>$null)

if ($RebuildImage -or -not $imageExists) {
    Write-Host "Building image '$ImageName'..."
    docker build -t $ImageName -f "$Root\Dockerfile.unittests" "$Root"
    if ($LASTEXITCODE -ne 0) { throw "docker build failed." }
} else {
    Write-Host "Image '$ImageName' is up to date. Pass -RebuildImage to refresh."
}

# ---------------------------------------------------------------------------
# 2. Compile test projects on the host (Windows build tools, Windows DLLs)
# ---------------------------------------------------------------------------
# Building on the host is intentional: several test projects reference
# Windows-only assemblies (System.Activities, Infragistics) that cannot be
# compiled inside a Linux container. dotnet build copies all required DLLs
# into each project's output directory, making those outputs self-contained
# and portable into the Linux container for execution.
# ---------------------------------------------------------------------------
if (-not $NoBuild) {
    Write-Step 'Compile Test Projects'
    dotnet build "$Root\Dev\ServerTests.sln" -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }
}

# ---------------------------------------------------------------------------
# 3. Collect pre-built test DLL paths
# ---------------------------------------------------------------------------
Write-Step 'Collect Test Assemblies'

$outputSubPath = [IO.Path]::Combine('bin', $Configuration, 'net8.0')

$dllItems = Get-ChildItem "$Root\Dev" -Recurse -Filter '*.dll' |
    Where-Object {
        # Only DLLs produced by the target build configuration
        $_.FullName.Contains($outputSubPath) -and
        # Match the project naming convention used by the CI pipeline
        $_.Name -match '^(Dev2|Warewolf)\..+\.Tests\.dll$' -and
        # Skip reference assemblies (ref/ subfolder)
        $_.FullName -notmatch '\\ref\\' -and
        # Only the DLL that lives inside its own project folder.
        # dotnet build copies dependency DLLs into every consumer's output
        # directory; this filter keeps only the authoritative copy where the
        # folder immediately above \bin\ matches the assembly name.
        $_.FullName -match ([regex]::Escape("\$($_.BaseName)\bin\"))
    } |
    Where-Object { $ExcludeProjects -notcontains $_.BaseName }

if ($dllItems.Count -eq 0) {
    Write-Error ("No test DLLs found under $Root\Dev matching '$outputSubPath\*.Tests.dll'. " +
                 "Run without -NoBuild, or run Compile.ps1 -ServerTests first.")
    exit 1
}

Write-Host "Found $($dllItems.Count) test assembly(ies):" -ForegroundColor Green
$dllItems | ForEach-Object { Write-Host "  $($_.BaseName)" }

# ---------------------------------------------------------------------------
# 4. Translate host paths to container-side paths
#    Host  : C:\Users\ultra\warewolf\Dev\...
#    Mount : /tests/Dev   (read-only)
# ---------------------------------------------------------------------------
$hostDevRoot      = (Resolve-Path "$Root\Dev").Path          # absolute, no trailing slash
$containerDevRoot = '/tests/Dev'

$containerDllPaths = $dllItems | ForEach-Object {
    $relative = $_.FullName.Substring($hostDevRoot.Length).TrimStart('\')
    "$containerDevRoot/$($relative.Replace('\', '/'))"
}

# ---------------------------------------------------------------------------
# 5. Build the TestCaseFilter expression for dotnet vstest
# ---------------------------------------------------------------------------
$filterArg = $null
if ($ExcludeCategories.Count -gt 0) {
    $clauses   = $ExcludeCategories | ForEach-Object { "(TestCategory!=$_)" }
    $filterArg = '--TestCaseFilter:' + ($clauses -join '&')
}

# ---------------------------------------------------------------------------
# 6. Prepare the results directory on the host
# ---------------------------------------------------------------------------
$resultsDir = Join-Path (Join-Path $Root 'TestResults') 'ContainerRun'
if (Test-Path $resultsDir) { Remove-Item $resultsDir -Recurse -Force }
New-Item -ItemType Directory -Path $resultsDir | Out-Null

# Docker Desktop on Windows accepts forward-slash paths in -v arguments.
$hostDevRootDocker  = $hostDevRoot.Replace('\', '/')
$resultsDirDocker   = $resultsDir.Replace('\', '/')

# ---------------------------------------------------------------------------
# 7. Run dotnet vstest inside the container
# ---------------------------------------------------------------------------
Write-Step 'Run Tests in Container'

$vstestArgs = $containerDllPaths + @(
    '--logger:trx',
    '--ResultsDirectory:/TestResults'
)
if ($Parallel)   { $vstestArgs += '--Parallel' }
if ($filterArg)  { $vstestArgs += $filterArg }

$dockerArgs = @('run', '--rm')
if ($MemoryLimit) { $dockerArgs += '--memory', $MemoryLimit }
$dockerArgs += @(
    '-v', "${hostDevRootDocker}:/tests/Dev:ro",
    '-v', "${resultsDirDocker}:/TestResults",
    $ImageName,
    'dotnet', 'vstest'
) + $vstestArgs

Write-Host "docker $($dockerArgs -join ' ')`n" -ForegroundColor DarkGray
& docker @dockerArgs
$exitCode = $LASTEXITCODE

# ---------------------------------------------------------------------------
# 8. Print a summary
# ---------------------------------------------------------------------------
Write-Step 'Summary'
Write-Host "Results directory: $resultsDir"

$trxFiles = @(Get-ChildItem "$resultsDir\*.trx" -ErrorAction SilentlyContinue)
if ($trxFiles.Count -gt 0) {
    Write-Host "TRX files written:"
    $trxFiles | ForEach-Object { Write-Host "  $($_.Name)" }
} else {
    Write-Host "No .trx files found - check the docker output above for errors." -ForegroundColor Yellow
}

if ($exitCode -ne 0) {
    Write-Host "`nTESTS FAILED (exit code $exitCode)" -ForegroundColor Red
    exit $exitCode
}
Write-Host "`nAll tests passed." -ForegroundColor Green
