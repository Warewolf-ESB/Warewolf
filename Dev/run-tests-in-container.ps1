# run-tests-in-container.ps1
# Runs Warewolf unit/integration tests inside a Docker container.
# Supports Microsoft Testing Platform (MTP) self-contained binaries and vstest DLLs.
#
# LOCAL DEV MODE  (no -BinDir): mounts the repo root into a long-lived container
#   and resolves DLLs from the source build output tree.
#
# CI MODE  (-BinDir <dir>): uses a pre-built flat artifact directory and writes
#   TRX results to -TestResultsDir. No interactive prompts.
#
# Script lives at <RepoRoot>\Dev\run-tests-in-container.ps1

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
    [switch]$RebuildImage,

    # CI MODE: path to directory containing pre-built self-contained linux-x64 binaries.
    # When provided the script uses the flat artifact layout instead of the source tree.
    [string]$BinDir,

    # CI MODE: directory where .trx result files are written.
    # Defaults to BinDir/../TestResults when -BinDir is used.
    [string]$TestResultsDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# PS 6+ automatic variables — define fallbacks for Windows PowerShell 5.1
if (-not (Test-Path Variable:\IsLinux))   { $IsLinux   = $false }
if (-not (Test-Path Variable:\IsMacOS))   { $IsMacOS   = $false }
if (-not (Test-Path Variable:\IsWindows)) { $IsWindows  = $true  }

function Invoke-Logged {
    Write-Host "+ $($args -join ' ')" -ForegroundColor DarkGray
    & $args[0] $args[1..($args.Count - 1)]
}

# Detect CI mode: either explicitly via -BinDir or by the Azure DevOps agent env var.
$CIMode = $PSBoundParameters.ContainsKey("BinDir") -or [bool]$env:TF_BUILD

# -- Derive paths from the script location ------------------------------------
# Script lives at <RepoRoot>\Dev\run-tests-in-container.ps1
$DevRoot  = $PSScriptRoot                        # …\Dev
$RepoRoot = Split-Path $DevRoot -Parent          # …\warewolf  (mounted as /mnt/approot)

$Dockerfile   = [System.IO.Path]::Combine($DevRoot, "Warewolf.Execution.Lightweight", "engine", "docker", "Dockerfile.test")
$DockerContext = [System.IO.Path]::Combine($DevRoot, "Warewolf.Execution.Lightweight", "engine", "docker")

if ($CIMode) {
    # Normalise paths supplied from the pipeline (may use forward slashes on Linux agents)
    if (-not $BinDir) {
        Write-Error "-BinDir is required in CI mode (TF_BUILD is set)."
        exit 1
    }
    $BinDir = $BinDir.TrimEnd('/\')
    if (-not $TestResultsDir) {
        $TestResultsDir = Join-Path (Split-Path $BinDir -Parent) "TestResults"
    }
    $TestResultsDir = $TestResultsDir.TrimEnd('/\')
    New-Item -ItemType Directory -Force -Path $TestResultsDir | Out-Null
    # Ensure the Docker container (which may run as a different user) can write results.
    if ($IsLinux -or $IsMacOS) { & chmod 777 $TestResultsDir }

    # In CI the Dockerfile travels with the binaries artifact.
    $CIDockerfile = Join-Path $BinDir "Dockerfile.test"
    if (Test-Path $CIDockerfile) {
        $Dockerfile   = $CIDockerfile
        $DockerContext = $BinDir
    }
}

# -- Prompt for assemblies early (before container startup) -------------------
if (-not $Assemblies -and -not $CIMode) {
    $assemblyInput = Read-Host "Assembly name(s) - comma-separated (blank = all Warewolf & Dev2 test assemblies)"
    if ($assemblyInput.Trim()) {
        $Assemblies = $assemblyInput -split "\s*,\s*" | Where-Object { $_ -ne "" }
    }
}

# -- Find or start the test container -----------------------------------------
if ($CIMode) {
    # In CI always build a fresh image and run a one-shot container per test suite.
    Write-Host "Building test image from $Dockerfile ..." -ForegroundColor Yellow
    Invoke-Logged docker build -t warewolf-test-env -f $Dockerfile $DockerContext
    if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
    $containerId = $null   # we will use docker run --rm below
} else {
    $containerId = docker ps --filter "ancestor=vsut_dockerfile" --format "{{.ID}}" 2>$null | Select-Object -First 1

    if ($RebuildImage) {
        if ($containerId) {
            Write-Host "Stopping existing container for rebuild..." -ForegroundColor Yellow
            Invoke-Logged docker stop $containerId | Out-Null
            $containerId = $null
        }
        Write-Host "Rebuilding image vsut_dockerfile..." -ForegroundColor Yellow
        Invoke-Logged docker build --no-cache -t vsut_dockerfile -f $Dockerfile $DockerContext
        if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
    }

    if (-not $containerId) {
        $imageExists = docker images vsut_dockerfile --format "{{.ID}}" 2>$null
        if (-not $imageExists) {
            Write-Host "Image vsut_dockerfile not found. Building..." -ForegroundColor Yellow
            Invoke-Logged docker build -t vsut_dockerfile -f $Dockerfile $DockerContext
            if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
        }

        Write-Host "Starting container..." -ForegroundColor Yellow
        $containerId = Invoke-Logged docker run -d -v "${RepoRoot}:/mnt/approot" vsut_dockerfile
        if ($LASTEXITCODE -ne 0) { Write-Error "Failed to start container."; exit 1 }

        Write-Host "Waiting for container to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }

    Write-Host "Using container: $containerId" -ForegroundColor Cyan
}

# -- Discover all test assemblies in the repo (local dev mode) ----------------
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

# -- Discover test assemblies from a flat BinDir (CI mode) --------------------
function Get-CITestAssemblies {
    Get-ChildItem -Path $BinDir -Filter "*.dll" |
        Where-Object {
            $_.BaseName -match "^(Warewolf|Dev2)\." -and
            $_.BaseName -match "\.(Tests|Specs)$"
        } |
        Select-Object -ExpandProperty BaseName |
        Sort-Object -Unique
}

# -- Prompt / default for missing parameters ----------------------------------
if (-not $Assemblies) {
    if ($CIMode) {
        Write-Host "Discovering test assemblies from $BinDir ..." -ForegroundColor Yellow
        $Assemblies = Get-CITestAssemblies
        Write-Host "Found $($Assemblies.Count) assemblies." -ForegroundColor Cyan
        if (-not $Assemblies) {
            Write-Error "No test assemblies (Warewolf/Dev2 *.Tests.dll) found in '$BinDir'. Ensure the LinuxTestBinaries artifact was published with linux-x64 targets."
            exit 1
        }
    } else {
        Write-Host "Discovering all test assemblies..." -ForegroundColor Yellow
        $Assemblies = Get-AllTestAssemblies
        Write-Host "Found $($Assemblies.Count) assemblies." -ForegroundColor Cyan
    }
}

if (-not $CIMode -and -not $PSBoundParameters.ContainsKey("ExcludeAssemblies") -and -not $ExcludeAssemblies) {
    $excludeInput = try { Read-Host "Assemblies to exclude - comma-separated (blank = none)" } catch { "" }
    if ($excludeInput.Trim()) {
        $ExcludeAssemblies = $excludeInput -split "\s*,\s*" | Where-Object { $_ -ne "" }
    }
}

if (-not $PSBoundParameters.ContainsKey("Filter") -and -not $Filter -and -not $CIMode) {
    Write-Host "Filter examples: MyTestMethod / TestCategory=MyCategory / FullyQualifiedName~ClassName / (blank = all)" -ForegroundColor Gray
    $filterInput = try { Read-Host "Filter" } catch { "" }
    $Filter = $filterInput.Trim()
}

# -- Split filter on commas (each value becomes a separate vstest run) --------
# Use explicit branches so the @($null) is a direct assignment, not a pipeline
# output — otherwise PowerShell unwraps @($null) to $null and foreach skips it.
if ($Filter) {
    $FilterValues = $Filter -split "\s*,\s*" | Where-Object { $_ -ne "" }
} else {
    $FilterValues = @($null)   # one run with no filter
}

# -- Apply exclusions ----------------------------------------------------------
if ($ExcludeAssemblies) {
    $Assemblies = $Assemblies | Where-Object { $_ -notin $ExcludeAssemblies }
    if (-not $Assemblies) {
        Write-Error "All assemblies were excluded. Nothing to run."
        exit 1
    }
}

# -- CI: run each assembly as a separate docker run ---------------------------
if ($CIMode) {
    $failed = 0

    Write-Host "CI: assemblies to run: $($Assemblies -join ', ')" -ForegroundColor Cyan
    Write-Host "CI: filter values    : $($FilterValues | ForEach-Object { if ($null -eq $_) { '<none>' } else { $_ } })" -ForegroundColor Cyan

    foreach ($filterValue in $FilterValues) {
        # Sanitise the filter value for use in filenames (replaces non-word chars with _)
        $rawSuffix = if ($filterValue) { ".$($filterValue -replace '[^a-zA-Z0-9_-]', '_')" } else { "" }

        foreach ($assembly in $Assemblies) {
            $dllPath = Join-Path $BinDir "$assembly.dll"
            if (-not (Test-Path $dllPath)) {
                Write-Warning "Assembly not found, skipping: $dllPath"
                continue
            }

            # Cap TRX filename at 200 chars to stay within the Linux ext4 255-byte limit
            $maxSuffix = [Math]::Max(8, 200 - $assembly.Length - 4)
            $filterSuffix = if ($rawSuffix.Length -gt $maxSuffix) { $rawSuffix.Substring(0, $maxSuffix) } else { $rawSuffix }

            Write-Host "=== Running $assembly$filterSuffix ===" -ForegroundColor Yellow

            $trxName = "$assembly$filterSuffix.trx"

            # These test projects use EnableMSTestRunner=true (Microsoft Testing Platform).
            # Run the self-contained binary directly rather than via `dotnet test assembly.dll`,
            # because the vstest host path requires the ELF binary to be executable and may
            # fail silently.  The MTP binary accepts --report-trx natively.
            $binaryPath = Join-Path $BinDir $assembly
            if (-not (Test-Path $binaryPath)) {
                Write-Warning "MTP binary not found at '$binaryPath'; falling back to dotnet test on DLL."
                $binaryPath = $null
            }

            # Ensure the Linux self-contained binary has the execute bit set.
            # DownloadPipelineArtifact does not preserve file permissions.
            if ($binaryPath -and ($IsLinux -or $IsMacOS)) {
                & chmod +x $binaryPath
            }

            if ($binaryPath) {
                # MTP native invocation — produces TRX via the TrxReport extension.
                $dockerRunArgs = @(
                    'run', '--rm',
                    '-v', "${BinDir}:/tests:ro",
                    '-v', "${TestResultsDir}:/results",
                    'warewolf-test-env',
                    "/tests/$assembly",
                    '--report-trx',
                    '--report-trx-filename', $trxName,
                    '--results-directory', '/results',
                    '--no-progress'
                )
                if ($filterValue) { $dockerRunArgs += '--filter'; $dockerRunArgs += $filterValue }
            } else {
                # Fallback: vstest path for assemblies that are not MTP self-contained binaries.
                $dockerRunArgs = @(
                    'run', '--rm',
                    '-v', "${BinDir}:/tests:ro",
                    '-v', "${TestResultsDir}:/results",
                    'warewolf-test-env',
                    '/usr/share/dotnet/dotnet', 'test', "/tests/$assembly.dll",
                    '--logger', "trx;LogFileName=$trxName",
                    '--results-directory', '/results'
                )
                if ($filterValue) { $dockerRunArgs += '--filter'; $dockerRunArgs += $filterValue }
            }

            Write-Host "+ docker $($dockerRunArgs -join ' ')" -ForegroundColor DarkGray

            # Remove any existing TRX so MTP doesn't throw "file already exists"
            $trxFullPath = Join-Path $TestResultsDir $trxName
            if (Test-Path $trxFullPath) { Remove-Item $trxFullPath -Force }

            & docker @dockerRunArgs

            if ($LASTEXITCODE -ne 0) {
                Write-Warning "$assembly$filterSuffix reported failures (exit $LASTEXITCODE)."
                $failed++
            }
        }
    }

    Write-Host "--- TRX files written to $TestResultsDir ---" -ForegroundColor Cyan
    $trxFiles = Get-ChildItem -Path $TestResultsDir -Recurse -Filter "*.trx" -ErrorAction SilentlyContinue
    if ($trxFiles) {
        $trxFiles | ForEach-Object { Write-Host "  $($_.FullName)" -ForegroundColor Green }
    } else {
        Write-Warning "No .trx files found in $TestResultsDir"
        # Show top-level contents for diagnosis
        Get-ChildItem -Path $TestResultsDir -ErrorAction SilentlyContinue | ForEach-Object {
            Write-Host "  $($_.FullName)" -ForegroundColor Gray
        }
    }

    if ($failed -gt 0) {
        Write-Error "$failed assembly/filter run(s) reported test failures."
        exit 1
    }
    exit 0
}

# -- LOCAL DEV: locate DLLs on the Windows filesystem ------------------------
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

    $containerPaths += $containerPath
}

# -- Build base vstest command (shared across all filter runs) ----------------
$baseCmd = @("/usr/share/dotnet/dotnet", "vstest") + $containerPaths + @('--logger:"console;verbosity=normal"')

# -- Execute one run per filter value -----------------------------------------
Write-Host ""
$overallExit = 0
if ($FilterValues) {
    foreach ($filterValue in $FilterValues) {
        $cmd = $baseCmd
        if ($filterValue) {
            $resolvedFilter = if ($filterValue -match "[=~!<>]") { $filterValue } else { "FullyQualifiedName~$filterValue" }
            $cmd += "--TestCaseFilter:`"$resolvedFilter`""
        }
        Invoke-Logged docker exec $containerId @cmd
        if ($LASTEXITCODE -ne 0) { $overallExit = $LASTEXITCODE }
    }
    exit $overallExit
} else {
    Invoke-Logged docker exec $containerId /usr/share/dotnet/dotnet vstest $containerPaths --logger:"console;verbosity=normal"
}
