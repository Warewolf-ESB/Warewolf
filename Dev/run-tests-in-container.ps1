# run-tests-in-container.ps1
# Runs tests directly inside the vsut_dockerfile container via dotnet vstest.
# The repo root (parent of Dev) maps to /mnt/approot inside the container.

[CmdletBinding()]
param(
    # One or more assembly names, e.g. Dev2.Activities.Tests, Dev2.Data.Tests
    [string[]]$Assemblies,

    # Assembly names to skip (resolved after -Assemblies expansion)
    [string[]]$ExcludeAssemblies,

    # vstest filter expression, e.g. "TestCategory=Unit" or "FullyQualifiedName~Foo"
    # Leave blank to run all tests in the selected assemblies.
    [string]$Filter,
    # Force a rebuild of the vsut_dockerfile image before starting the container.
    # Use this if the container is stale (e.g. after Dockerfile changes).
    [switch]$RebuildImage
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -- Derive paths from the script location ------------------------------------
# Script lives at <RepoRoot>\Dev\run-tests-in-container.ps1
$DevRoot  = $PSScriptRoot                        # …\Dev
$RepoRoot = Split-Path $DevRoot -Parent          # …\warewolf  (mounted as /mnt/approot)

$Dockerfile   = "$DevRoot\Warewolf.Execution.Lightweight\engine\docker\Dockerfile.test"
$DockerContext = "$DevRoot\Warewolf.Execution.Lightweight\engine\docker"

# -- Find or start the test container -----------------------------------------
$containerId = docker ps --filter "ancestor=vsut_dockerfile" --format "{{.ID}}" 2>$null | Select-Object -First 1

if ($RebuildImage) {
    if ($containerId) {
        Write-Host "Stopping existing container for rebuild..." -ForegroundColor Yellow
        docker stop $containerId | Out-Null
        $containerId = $null
    }
    Write-Host "Rebuilding image vsut_dockerfile..." -ForegroundColor Yellow
    docker build --no-cache -t vsut_dockerfile -f $Dockerfile $DockerContext
    if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
}

if (-not $containerId) {
    $imageExists = docker images vsut_dockerfile --format "{{.ID}}" 2>$null
    if (-not $imageExists) {
        Write-Host "Image vsut_dockerfile not found. Building..." -ForegroundColor Yellow
        docker build -t vsut_dockerfile -f $Dockerfile $DockerContext
        if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
    }

    Write-Host "Starting container..." -ForegroundColor Yellow
    $containerId = docker run -d `
        -v "${RepoRoot}:/mnt/approot" `
        vsut_dockerfile
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to start container."; exit 1 }

    Write-Host "Waiting for container to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
}

Write-Host "Using container: $containerId" -ForegroundColor Cyan

# -- Discover all test assemblies in the repo ---------------------------------
function Get-AllTestAssemblies {
    Get-ChildItem -Path $DevRoot -Recurse -Filter "*.dll" |
        Where-Object {
            $_.BaseName -match "^(Warewolf|Dev2)\." -and
            $_.BaseName -match "\.(Tests|Specs)$" -and
            $_.FullName -match "\\bin\\Debug\\net8\.0\\" -and
            $_.FullName -notmatch "\\linux-x64\\" -and
            $_.FullName -match "\\$([regex]::Escape($_.BaseName))\\bin\\"
        } |
        Select-Object -ExpandProperty BaseName |
        Sort-Object -Unique
}

# -- Prompt for missing parameters --------------------------------------------
if (-not $Assemblies) {
    $assemblyInput = Read-Host "Assembly name(s) - comma-separated (blank = all Warewolf & Dev2 test assemblies)"
    if ($assemblyInput.Trim()) {
        $Assemblies = $assemblyInput -split "\s*,\s*" | Where-Object { $_ -ne "" }
    } else {
        Write-Host "Discovering all test assemblies..." -ForegroundColor Yellow
        $Assemblies = Get-AllTestAssemblies
        Write-Host "Found $($Assemblies.Count) assemblies." -ForegroundColor Cyan
    }
}

if (-not $PSBoundParameters.ContainsKey("ExcludeAssemblies") -and -not $ExcludeAssemblies) {
    $excludeInput = Read-Host "Assemblies to exclude - comma-separated (blank = none)"
    if ($excludeInput.Trim()) {
        $ExcludeAssemblies = $excludeInput -split "\s*,\s*" | Where-Object { $_ -ne "" }
    }
}

if (-not $PSBoundParameters.ContainsKey("Filter") -and -not $Filter) {
    Write-Host "Filter examples: MyTestMethod / TestCategory=MyCategory / FullyQualifiedName~ClassName / (blank = all)" -ForegroundColor Gray
    $filterInput = Read-Host "Filter"
    $Filter = $filterInput.Trim()
}

# -- Apply exclusions ----------------------------------------------------------
if ($ExcludeAssemblies) {
    $Assemblies = $Assemblies | Where-Object { $_ -notin $ExcludeAssemblies }
    if (-not $Assemblies) {
        Write-Error "All assemblies were excluded. Nothing to run."
        exit 1
    }
}

# -- Locate DLLs on the Windows filesystem ------------------------------------
$containerPaths = @()

foreach ($assembly in $Assemblies) {
    $dll = Get-ChildItem -Path $DevRoot -Recurse -Filter "$assembly.dll" `
        | Where-Object {
            $_.FullName -match "\\bin\\Debug\\net8\.0\\" -and
            $_.FullName -match "\\$([regex]::Escape($assembly))\\bin\\"
        } `
        | Select-Object -First 1

    if (-not $dll) {
        Write-Error "Could not find $assembly\bin\Debug\net8.0\$assembly.dll. Build the project first."
        exit 1
    }

    # Convert Windows path → container path (/mnt/approot/…)
    $containerPath = $dll.FullName -replace [regex]::Escape("$RepoRoot\"), "/mnt/approot/"
    $containerPath = $containerPath -replace "\\", "/"
    Write-Host "  $assembly -> $containerPath" -ForegroundColor Cyan

    # Verify the DLL exists in the container
    docker exec $containerId test -f $containerPath 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "DLL not found inside container at: $containerPath`nEnsure the project is built and the workspace is mounted."
        exit 1
    }

    $containerPaths += $containerPath
}

# -- Build vstest command ------------------------------------------------------
$cmd = @("/usr/share/dotnet/dotnet", "vstest") + $containerPaths + @("--logger:console;verbosity=normal")

if ($Filter) {
    $resolvedFilter = if ($Filter -match "[=~!<>]") { $Filter } else { "FullyQualifiedName~$Filter" }
    $cmd += "--TestCaseFilter:`"$resolvedFilter`""
}

Write-Host ""
Write-Host "Running: docker exec $containerId $($cmd -join ' ')" -ForegroundColor Yellow
Write-Host ""

# -- Execute -------------------------------------------------------------------
docker exec $containerId @cmd
exit $LASTEXITCODE
