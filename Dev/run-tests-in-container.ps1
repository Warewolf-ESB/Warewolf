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
    [string]$TestResultsDir,

    # When set, each test assembly is run under dotnet-coverage inside the container.
    # Coverage XML files are written to this directory, one per assembly, named
    # <Assembly>.cobertura.xml.  The directory is mounted as /coverage in the container.
    [string]$CoverageDir,

    # Optional list of DLL filenames (relative to BinDir) to pass as --include-files
    # to dotnet-coverage.  Narrows coverage to specific assemblies-under-test.
    [string[]]$CoverageIncludeFiles,

    # When set, the test container shares the host network stack (--network=host).
    # Required when the server under test is a host-mapped Docker container
    # (e.g. the Azure Functions engine listening on host port 7071).
    # Effective on Linux only; ignored silently on Windows/macOS.
    [switch]$UseHostNetwork,

    # When set, the host directory is mounted at /shared-config (writable) inside
    # every test container, and WAREWOLF_SECURE_CONFIG is set to
    # /shared-config/secure.config.
    # Required for security spec tests that write secure.config at runtime —
    # the BinDir mount is read-only, so the config must live in a separate
    # writable volume that both the host func-start process and the container share.
    [string]$SharedConfigDir
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

#Ensure entrypoint is blank or the run will hang at the end because the function host is running.
(Get-Content $Dockerfile).Replace('ENTRYPOINT ["/bin/bash"]', 'ENTRYPOINT []') | Set-Content $Dockerfile

# -- Find or start the test container -----------------------------------------
if ($CIMode) {
    # In CI always build a fresh image and run a one-shot container per test suite.
    Write-Host "Building test image from $Dockerfile ..." -ForegroundColor Yellow
    Invoke-Logged docker build -t warewolf-test-env -f $Dockerfile $DockerContext
    if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
    $containerId = $null   # we will use docker run --rm below

    # Verify dotnet-coverage was actually installed into the image.
    # A stale Docker layer cache or a transient NuGet failure can leave the
    # tool directory empty even though the build exits 0.
    Write-Host "--- Verifying dotnet-coverage in image ---" -ForegroundColor Cyan
    $toolCheck = & docker run --rm warewolf-test-env sh -c "ls -la /root/.dotnet/tools/ 2>&1; echo EXIT:$?"
    Write-Host $toolCheck
    $coveragePresent = & docker run --rm warewolf-test-env sh -c "test -x /root/.dotnet/tools/dotnet-coverage && echo FOUND || echo MISSING"
    if ($coveragePresent -notmatch "FOUND") {
        Write-Host "##[error] dotnet-coverage is MISSING from the image." -ForegroundColor Red
        Write-Host "Installed global tools:" -ForegroundColor Yellow
        & docker run --rm warewolf-test-env sh -c "/usr/share/dotnet/dotnet tool list --global 2>&1 || true"
        Write-Host "DOTNET_ROOT / dotnet location:" -ForegroundColor Yellow
        & docker run --rm warewolf-test-env sh -c "which dotnet 2>&1 || true; ls /usr/share/dotnet/ 2>&1 || true"
        Write-Error "dotnet-coverage not found in image — aborting. Rebuild with --no-cache to re-run the tool install step."
        exit 1
    }
    Write-Host "dotnet-coverage: OK" -ForegroundColor Green
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
# Entries in $ExcludeAssemblies may contain wildcards (e.g. "*.Specs").
# Use -like for each entry so wildcard patterns are honoured.
if ($ExcludeAssemblies) {
    $Assemblies = $Assemblies | Where-Object {
        $name = $_
        -not ($ExcludeAssemblies | Where-Object { $name -like $_ })
    }
    if (-not $Assemblies) {
        Write-Error "All assemblies were excluded. Nothing to run."
        exit 1
    }
}

# -- CI: run each assembly as a separate docker run ---------------------------
if ($CIMode) {
    $failed = 0
    $failedAssemblies = New-Object 'System.Collections.Generic.List[string]'

    if ($CoverageDir) {
        New-Item -ItemType Directory -Force -Path $CoverageDir | Out-Null
        if ($IsLinux -or $IsMacOS) { & chmod 777 $CoverageDir }
    }

    Write-Host "CI: assemblies to run: $($Assemblies -join ', ')" -ForegroundColor Cyan
    $filterDisplay = $FilterValues | ForEach-Object { if ($null -eq $_) { '<none>' } else { $_ } }
    Write-Host "CI: filter values    : $($filterDisplay -join ', ')" -ForegroundColor Cyan

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

            # Detect whether this is a Microsoft Testing Platform (EnableMSTestRunner=true)
            # project by parsing its .deps.json and confirming that Microsoft.Testing.Platform
            # is a DIRECT dependency of the project's own entry — not just a transitive dep
            # pulled in by MSTest.TestAdapter 3.x (which fools a simple string-search).
            # We cannot rely on the presence of a no-extension ELF binary alone: publishing the
            # whole solution with --self-contained true -p:UseAppHost=true creates an app-host
            # binary for EVERY project (including OutputType=Library ones like Security.Specs)
            # even though they are plain vstest assemblies, not MTP projects.
            $binaryPath = Join-Path $BinDir $assembly
            if (-not (Test-Path $binaryPath)) {
                Write-Host "  [MTP] No app-host binary found for $assembly; using dotnet test." -ForegroundColor DarkGray
                $binaryPath = $null
            } else {
                $depsJson = Join-Path $BinDir "$assembly.deps.json"
                $isMtp = $false
                if (Test-Path $depsJson) {
                    # True MTP projects (EnableMSTestRunner=true) list Microsoft.Testing.Platform as
                    # a DIRECT dependency of the project entry in the deps.json. Plain vstest
                    # assemblies that reference MSTest.TestAdapter 3.x also contain the string
                    # "Microsoft.Testing.Platform" in their deps.json (as a transitive dep of the
                    # adapter), which causes a false positive if we just grep the whole file.
                    # Parse the JSON and check only the project's own dependency list.
                    try {
                        $depsObj = Get-Content $depsJson -Raw -ErrorAction SilentlyContinue | ConvertFrom-Json -ErrorAction SilentlyContinue
                        if ($depsObj) {
                            $firstTarget = $depsObj.targets.PSObject.Properties | Select-Object -First 1
                            if ($firstTarget) {
                                $mainEntry = $firstTarget.Value.PSObject.Properties |
                                    Where-Object { $_.Name -like "$assembly/*" } |
                                    Select-Object -First 1
                                if ($mainEntry) {
                                    $directDeps = $mainEntry.Value.dependencies.PSObject.Properties.Name
                                    $isMtp = $directDeps -contains 'Microsoft.Testing.Platform'
                                    Write-Host "  [MTP] $assembly direct deps include MTP: $isMtp" -ForegroundColor DarkGray
                                } else {
                                    Write-Host "  [MTP] No main entry found in deps.json for $assembly — falling back to dotnet test." -ForegroundColor DarkGray
                                }
                            }
                        }
                    } catch {
                        Write-Host "  [MTP] Failed to parse deps.json for ${assembly}: $_" -ForegroundColor Yellow
                    }
                }
                if ($isMtp) {
                    Write-Host "  [MTP] $assembly detected as Microsoft Testing Platform project." -ForegroundColor DarkGray
                } else {
                    Write-Host "  [MTP] $assembly has app-host binary but MTP is not its direct runner — using dotnet test." -ForegroundColor DarkGray
                    $binaryPath = $null
                }
            }

            # Ensure the Linux self-contained binary has the execute bit set.
            # DownloadPipelineArtifact does not preserve file permissions.
            if ($binaryPath -and ($IsLinux -or $IsMacOS)) {
                & chmod +x $binaryPath
            }

            # --network=host is Linux-only; silently omit it on other platforms.
            $networkArgs = if ($UseHostNetwork -and $IsLinux) { @('--network=host') } else { @() }

            # DOTNET_ROOT tells the apphost where to find the installed .NET runtime.
            # Without it, the apphost finds .NET native libs (libcoreclr.so etc.) that
            # ship alongside the test binaries and mistakes /tests/ for the .NET root,
            # causing "No frameworks were found."
            $dotnetRootArgs = @('-e', 'DOTNET_ROOT=/usr/share/dotnet')

            # Build coverage wrapper args (per-assembly since output path includes the name).
            $coverageVolumeArgs = @()
            $coveragePrefix = @()
            if ($CoverageDir) {
                $coverageVolumeArgs = @('-v', "${CoverageDir}:/coverage")
                $coveragePrefix = @(
                    '/root/.dotnet/tools/dotnet-coverage', 'collect',
                    '--output', "/coverage/$assembly.cobertura.xml",
                    '--output-format', 'cobertura',
                    '--nologo'
                )
                if ($CoverageIncludeFiles) {
                    foreach ($f in $CoverageIncludeFiles) {
                        $coveragePrefix += '--include-files'
                        $coveragePrefix += "/tests/$f"
                    }
                }
                $coveragePrefix += '--'
            }

            # Build shared-config volume and env args when -SharedConfigDir is set.
            # Security spec tests write secure.config at runtime; BinDir is read-only,
            # so config must live in a separate writable volume shared with the server.
            $sharedConfigArgs = @()
            if ($SharedConfigDir) {
                New-Item -ItemType Directory -Force -Path $SharedConfigDir | Out-Null
                if ($IsLinux -or $IsMacOS) { & chmod 777 $SharedConfigDir }
                $sharedConfigArgs = @(
                    '-v', "${SharedConfigDir}:/shared-config",
                    '-e', 'WAREWOLF_SECURE_CONFIG=/shared-config/secure.config'
                )
            }

            if ($binaryPath) {
                # MTP invocation via `dotnet <assembly>.dll` — avoids the ELF apphost
                # probing /tests/ for libhostfxr.so (which lands there from other
                # self-contained test projects) before honoring DOTNET_ROOT, which caused
                # "No frameworks were found." when running the apphost directly.
                # EnableMSTestRunner=true DLLs accept all --report-trx args when run this way.
                $dockerRunArgs = @('run', '--rm') + $networkArgs + $dotnetRootArgs + $coverageVolumeArgs + $sharedConfigArgs + @(
                    '-v', "${BinDir}:/tests:ro",
                    '-v', "${TestResultsDir}:/results",
                    'warewolf-test-env'
                ) + $coveragePrefix + @(
                    '/usr/share/dotnet/dotnet', "/tests/$assembly.dll",
                    '--report-trx',
                    '--report-trx-filename', $trxName,
                    '--results-directory', '/results',
                    '--no-progress'
                )
                if ($filterValue) { $dockerRunArgs += '--filter'; $dockerRunArgs += $filterValue }
            } else {
                # Fallback: vstest path for assemblies that are not MTP self-contained binaries.
                $dockerRunArgs = @('run', '--rm') + $networkArgs + $dotnetRootArgs + $coverageVolumeArgs + $sharedConfigArgs + @(
                    '-v', "${BinDir}:/tests:ro",
                    '-v', "${TestResultsDir}:/results",
                    'warewolf-test-env'
                ) + $coveragePrefix + @(
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

            Write-Host "  Expected TRX: $trxFullPath" -ForegroundColor DarkGray

            & docker @dockerRunArgs
            $dockerExit = $LASTEXITCODE

            Write-Host "  Container exit code: $dockerExit" -ForegroundColor Cyan

            # Check whether the expected TRX was actually written by this run.
            if (Test-Path $trxFullPath) {
                $trxSize = (Get-Item $trxFullPath).Length
                Write-Host "  ✓ TRX written: $trxName ($trxSize bytes)" -ForegroundColor Green
            } else {
                Write-Host "  ✗ TRX NOT found: $trxFullPath" -ForegroundColor Yellow
                Write-Host "  Contents of ${TestResultsDir} after this run:" -ForegroundColor Yellow
                Get-ChildItem -Path $TestResultsDir -Recurse -ErrorAction SilentlyContinue |
                    ForEach-Object { Write-Host "    $($_.FullName) ($($_.Length) bytes)" -ForegroundColor Gray }
            }

            if ($dockerExit -eq 8) {
                # Exit code 8 = Microsoft Testing Platform "ZeroTestsRan":
                # all tests were filtered out by category or all were skipped/inconclusive.
                # This is expected (e.g. CannotParallelize-only assemblies, or
                # integration assemblies whose external services aren't available).
                # Treat as a warning, not a failure.
                Write-Warning "WARN: $assembly$filterSuffix - zero tests ran (all filtered or skipped). Exit 8."
            } elseif ($dockerExit -ne 0) {
                Write-Warning "FAILED: $assembly$filterSuffix (exit $dockerExit)."
                $failedAssemblies.Add("$assembly$filterSuffix")
                $failed++
            }
        }
    }

    Write-Host "--- TRX files written to $TestResultsDir ---" -ForegroundColor Cyan
    $trxFiles = Get-ChildItem -Path $TestResultsDir -Recurse -Filter "*.trx" -ErrorAction SilentlyContinue
    if ($trxFiles) {
        $trxFiles | ForEach-Object { Write-Host "  $($_.FullName) ($($_.Length) bytes)" -ForegroundColor Green }
    } else {
        Write-Warning "No .trx files found in $TestResultsDir"
        Write-Host "  Full recursive listing of ${TestResultsDir}:" -ForegroundColor Yellow
        $allFiles = Get-ChildItem -Path $TestResultsDir -Recurse -ErrorAction SilentlyContinue
        if ($allFiles) {
            $allFiles | ForEach-Object { Write-Host "  $($_.FullName) ($($_.Length) bytes)" -ForegroundColor Gray }
        } else {
            Write-Host "  (directory is empty)" -ForegroundColor Gray
        }
    }

    if ($failed -gt 0) {
        Write-Host ""
        Write-Host "==================== FAILED ASSEMBLIES ====================" -ForegroundColor Red
        foreach ($name in $failedAssemblies) {
            Write-Host "  FAILED: $name" -ForegroundColor Red
        }
        Write-Host "===========================================================" -ForegroundColor Red
        Write-Error "$failed assembly/filter run(s) reported test failures: $($failedAssemblies -join ', ')"
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
