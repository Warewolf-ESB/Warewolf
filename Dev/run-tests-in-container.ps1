# run-tests-in-container.ps1
# Runs tests directly inside the vsut_dockerfile container via vstest.console.dll.
# The repo root (parent of Dev) maps to /mnt/approot inside the container.
# VS vstest tools are mounted at /mnt/vstest inside the container.

[CmdletBinding()]
param(
    # One or more assembly names, e.g. Dev2.Activities.Tests, Dev2.Data.Tests
    [string[]]$Assemblies,

    # Assembly names to skip (resolved after -Assemblies expansion)
    [string[]]$ExcludeAssemblies,

    # vstest filter expression, e.g. "TestCategory=Unit" or "FullyQualifiedName~Foo"
    # Leave blank to run all tests in the selected assemblies.
    [string]$Filter,

    # Flat binary directory (CI / pipeline mode).  When set, DLLs are taken from this
    # directory directly instead of being discovered under the Dev source tree.
    # The directory is mounted at /mnt/approot inside the container.
    # Interactive prompts are suppressed when this parameter is provided.
    [string]$BinDir,

    # Directory on the host where TRX test-result files should be written.
    # When set, vstest is told to write TRX output there (mounted as /mnt/testresults).
    # When omitted, only the console logger is used (suitable for local dev runs).
    [string]$TestResultsDir,

    # Optional override: directory containing vstest.console.dll.
    # When omitted the script auto-discovers from Visual Studio (Windows) or the .NET SDK (Linux).
    [string]$VsTestDllDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Derive paths from the script location ────────────────────────────────────
# Script lives at <RepoRoot>\Dev\run-tests-in-container.ps1
$DevRoot  = $PSScriptRoot                        # …\Dev
$RepoRoot = Split-Path $DevRoot -Parent          # …\warewolf  (mounted as /mnt/approot)

$Dockerfile   = "$DevRoot\Warewolf.Execution.Lightweight\engine\docker\Dockerfile.test"
$DockerContext = "$DevRoot\Warewolf.Execution.Lightweight\engine\docker"

# ── Locate vstest.console.dll ─────────────────────────────────────────────────
if ($VsTestDllDir) {
    $vsTestHostPath = $VsTestDllDir
} elseif ($IsWindows -or ($env:OS -eq 'Windows_NT')) {
    $vsTestHostPath = Get-ChildItem `
        -Path "C:\Program Files\Microsoft Visual Studio" `
        -Recurse -Filter "vstest.console.dll" -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty DirectoryName
} else {
    # Linux: vstest.console.dll ships inside the .NET SDK installation.
    $vsTestHostPath = Get-ChildItem `
        -Path "/usr/share/dotnet/sdk" `
        -Recurse -Filter "vstest.console.dll" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch "/testhost/" } |
        Select-Object -First 1 -ExpandProperty DirectoryName
}

if (-not $vsTestHostPath) {
    Write-Error "Could not find vstest.console.dll. On Windows install Visual Studio; on Linux install the .NET SDK or pass -VsTestDllDir."
    exit 1
}
Write-Host "Found vstest at: $vsTestHostPath" -ForegroundColor Cyan

# ── Find or start the test container ─────────────────────────────────────────
$containerId = docker ps --filter "ancestor=vsut_dockerfile" --format "{{.ID}}" 2>$null | Select-Object -First 1

if (-not $containerId) {
    $imageExists = docker images vsut_dockerfile --format "{{.ID}}" 2>$null
    if (-not $imageExists) {
        Write-Host "Image vsut_dockerfile not found. Building..." -ForegroundColor Yellow
        docker build -t vsut_dockerfile -f $Dockerfile $DockerContext
        if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
    }

    Write-Host "Starting container..." -ForegroundColor Yellow
    $mountSource = if ($BinDir) { $BinDir } else { $RepoRoot }
    $runArgs = @("-d",
        "-v", "${mountSource}:/mnt/approot",
        "-v", "${vsTestHostPath}:/mnt/vstest")
    if ($TestResultsDir) {
        New-Item -ItemType Directory -Force -Path $TestResultsDir | Out-Null
        $runArgs += @("-v", "${TestResultsDir}:/mnt/testresults")
    }
    $runArgs += "vsut_dockerfile"
    $containerId = docker run @runArgs
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to start container."; exit 1 }

    Write-Host "Waiting for container to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
}

Write-Host "Using container: $containerId" -ForegroundColor Cyan

# ── Discover all test assemblies in the repo ─────────────────────────────────
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

# ── Discover test assemblies from a flat binary directory (CI mode) ───────────
function Get-AllTestAssembliesFromBinDir {
    Get-ChildItem -Path $BinDir -Filter "*.dll" |
        Where-Object { $_.BaseName -match "^(Warewolf|Dev2)\." -and $_.BaseName -match "\.(Tests|Specs)$" } |
        Select-Object -ExpandProperty BaseName |
        Sort-Object -Unique
}

# ── Prompt for missing parameters (skipped in CI / BinDir mode) ──────────────
if (-not $Assemblies) {
    if ($BinDir) {
        Write-Host "Discovering all test assemblies in '$BinDir'..." -ForegroundColor Yellow
        $Assemblies = Get-AllTestAssembliesFromBinDir
        Write-Host "Found $($Assemblies.Count) assemblies." -ForegroundColor Cyan
    } else {
        $assemblyInput = Read-Host "Assembly name(s) — comma-separated (blank = all Warewolf & Dev2 test assemblies)"
        if ($assemblyInput.Trim()) {
            $Assemblies = $assemblyInput -split "\s*,\s*" | Where-Object { $_ -ne "" }
        } else {
            Write-Host "Discovering all test assemblies..." -ForegroundColor Yellow
            $Assemblies = Get-AllTestAssemblies
            Write-Host "Found $($Assemblies.Count) assemblies." -ForegroundColor Cyan
        }
    }
}

if (-not $BinDir -and -not $PSBoundParameters.ContainsKey("ExcludeAssemblies") -and -not $ExcludeAssemblies) {
    $excludeInput = Read-Host "Assemblies to exclude — comma-separated (blank = none)"
    if ($excludeInput.Trim()) {
        $ExcludeAssemblies = $excludeInput -split "\s*,\s*" | Where-Object { $_ -ne "" }
    }
}

if (-not $BinDir -and -not $PSBoundParameters.ContainsKey("Filter") -and -not $Filter) {
    Write-Host "Filter examples: MyTestMethod / TestCategory=MyCategory / FullyQualifiedName~ClassName / (blank = all)" -ForegroundColor Gray
    $filterInput = Read-Host "Filter"
    $Filter = $filterInput.Trim()
}

# ── Apply exclusions ──────────────────────────────────────────────────────────
if ($ExcludeAssemblies) {
    $Assemblies = $Assemblies | Where-Object { $_ -notin $ExcludeAssemblies }
    if (-not $Assemblies) {
        Write-Error "All assemblies were excluded. Nothing to run."
        exit 1
    }
}

# ── Locate DLLs on the Windows filesystem ────────────────────────────────────
$containerPaths = @()

foreach ($assembly in $Assemblies) {
    if ($BinDir) {
        # Flat artifact directory — DLLs live directly in $BinDir.
        $dllPath = Join-Path $BinDir "$assembly.dll"
        if (-not (Test-Path $dllPath)) {
            Write-Error "Could not find '$assembly.dll' in '$BinDir'. Ensure the artifact was downloaded."
            exit 1
        }
        $containerPath = "/mnt/approot/$assembly.dll"
    } else {
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
    }

    Write-Host "  $assembly -> $containerPath" -ForegroundColor Cyan

    # Verify the DLL exists in the container
    docker exec $containerId test -f $containerPath 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "DLL not found inside container at: $containerPath`nEnsure the project is built and the workspace is mounted."
        exit 1
    }

    $containerPaths += $containerPath
}

# ── Build vstest.console command ──────────────────────────────────────────────
$dotnet = "/usr/share/dotnet/dotnet"
$vstest = "/mnt/vstest/vstest.console.dll"
$cmd = @($dotnet, $vstest) + $containerPaths + @("/logger:console;verbosity=normal")

if ($TestResultsDir) {
    $cmd += "/logger:trx"
    $cmd += "/ResultsDirectory:/mnt/testresults"
}

if ($Filter) {
    $resolvedFilter = if ($Filter -match "[=~!<>]") { $Filter } else { "FullyQualifiedName~$Filter" }
    $cmd += "/TestCaseFilter:`"$resolvedFilter`""
}

Write-Host ""
Write-Host "Running: docker exec $containerId $($cmd -join ' ')" -ForegroundColor Yellow
Write-Host ""

# ── Execute ───────────────────────────────────────────────────────────────────
docker exec $containerId @cmd
exit $LASTEXITCODE
