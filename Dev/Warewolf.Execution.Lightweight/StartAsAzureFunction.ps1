Param(
  [switch]$DoExit,
  [string]$ResourcesPath,
  [string]$FunctionPath,
  [switch]$Cleanup,
  [int]$Port            = 7071,
  [int]$TimeoutSeconds  = 120,
  [int]$PollIntervalSec = 5
)

# -----------------------------------------------------------------------------
# region: Pipeline Logging Helpers
# -----------------------------------------------------------------------------

function Write-PipelineSection ([string]$Message) {
    Write-Host ""
    Write-Host "##[section]$Message"
}

function Write-PipelineWarning ([string]$Message) {
    Write-Host "##vso[task.logissue type=warning]$Message"
}

function Write-PipelineError ([string]$Message) {
    Write-Host "##vso[task.logissue type=error]$Message"
}

function Fail-Pipeline ([string]$Message) {
    Write-PipelineError $Message
    Write-Host "##vso[task.complete result=Failed;]$Message"

    # Dump last 50 lines of func output to help diagnose failures
    $outLog = "$PSScriptRoot\TestResults\AzureFunctionOutput.txt"
    $errLog = "$PSScriptRoot\TestResults\AzureFunctionError.txt"
    if (Test-Path $outLog) {
        Write-Host "--- AzureFunctionOutput.txt (last 50 lines) ---"
        Get-Content $outLog -ErrorAction SilentlyContinue | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
    }
    if (Test-Path $errLog) {
        Write-Host "--- AzureFunctionError.txt (last 50 lines) ---"
        Get-Content $errLog -ErrorAction SilentlyContinue | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
    }
    exit 1
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 1. Kill any existing func host processes (-Cleanup switch)
# -----------------------------------------------------------------------------

if ($Cleanup.IsPresent) {
    Write-PipelineSection "Cleanup: stopping existing func processes..."
    $FuncProcesses = Get-Process "func" -ErrorAction SilentlyContinue
    if ($FuncProcesses) {
        $FuncProcesses | Stop-Process -Force
        Start-Sleep 2
        Write-Host "Stopped $($FuncProcesses.Count) existing func process(es)."
    } else {
        Write-Host "No existing func processes found."
    }
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 2. Kill any stale process occupying the target port
# -----------------------------------------------------------------------------

Write-PipelineSection "Checking port $Port availability..."

$IsLinux = $PSVersionTable.Platform -eq "Unix"

if ($IsLinux) {
    # Linux: use lsof to find and kill whatever owns the port
    $stalePids = & bash -c "lsof -ti tcp:$Port 2>/dev/null"
    if ($stalePids) {
        Write-PipelineWarning "Port $Port is in use by PID(s): $stalePids - killing..."
        & bash -c "kill -9 $stalePids 2>/dev/null"
        Start-Sleep 2
        Write-Host "Port $Port cleared."
    } else {
        Write-Host "Port $Port is free."
    }
} else {
    # Windows: use netstat to find the PID and kill it
    $netstatLine = netstat -ano | Select-String ":$Port\s+.*LISTENING"
    if ($netstatLine) {
        $stalePid = ($netstatLine -split '\s+')[-1]
        Write-PipelineWarning "Port $Port is in use by PID $stalePid - killing..."
        try {
            Stop-Process -Id $stalePid -Force -ErrorAction Stop
            Start-Sleep 2
            Write-Host "Port $Port cleared."
        } catch {
            Write-PipelineWarning "Could not kill PID $stalePid : $_"
        }
    } else {
        Write-Host "Port $Port is free."
    }
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 3. Install Azure Functions Core Tools v4 via npm if missing
# -----------------------------------------------------------------------------

Write-PipelineSection "Checking Azure Functions Core Tools (func)..."

$FuncExe     = $null
$FuncCommand = Get-Command "func" -ErrorAction SilentlyContinue

if ($FuncCommand) {
    $FuncExe = $FuncCommand.Source
    $FuncVer = & func --version 2>&1
    Write-Host "func already installed: $FuncExe (v$FuncVer)"
} else {
    Write-Host "func not found - installing azure-functions-core-tools@4 via npm..."

    $npmCommand = Get-Command "npm" -ErrorAction SilentlyContinue
    if (-not $npmCommand) {
        Fail-Pipeline "npm is not available on this agent. Cannot auto-install Azure Functions Core Tools."
    }

    # --unsafe-perm is required on Linux agents running as root (common in pipelines)
    & npm install -g azure-functions-core-tools@4 --unsafe-perm true
    if ($LASTEXITCODE -ne 0) {
        Fail-Pipeline "npm install of azure-functions-core-tools@4 failed (exit code $LASTEXITCODE)."
    }

    # Re-resolve after install
    $FuncCommand = Get-Command "func" -ErrorAction SilentlyContinue
    if (-not $FuncCommand) {
        # npm global bin may not yet be in PATH - try to add it dynamically
        $npmGlobalBin = & npm bin -g 2>/dev/null
        if ($npmGlobalBin -and (Test-Path "$npmGlobalBin/func")) {
            $env:PATH = "$npmGlobalBin$([System.IO.Path]::PathSeparator)$env:PATH"
            $FuncCommand = Get-Command "func" -ErrorAction SilentlyContinue
        }
    }

    if (-not $FuncCommand) {
        Fail-Pipeline "func still not found after npm install. Ensure npm global bin directory is in PATH."
    }

    $FuncExe = $FuncCommand.Source
    $FuncVer = & func --version 2>&1
    Write-Host "func installed successfully: $FuncExe (v$FuncVer)"
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Resolve host.json directory
# -----------------------------------------------------------------------------

Write-PipelineSection "Resolving Function project directory..."

if ($FunctionPath -and (Test-Path "$FunctionPath\host.json")) {
    $FuncDir = $FunctionPath
} elseif (Test-Path "$PSScriptRoot\host.json") {
    $FuncDir = $PSScriptRoot
} elseif (Test-Path "$PSScriptRoot\Warewolf.Execution.Lightweight\host.json") {
    $FuncDir = "$PSScriptRoot\Warewolf.Execution.Lightweight"
} else {
    Fail-Pipeline "Cannot find host.json. Use -FunctionPath to specify the Azure Functions directory."
}

Write-Host "Function directory: $FuncDir"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Pre-flight - verify required assemblies are present
# -----------------------------------------------------------------------------
#
# The dotnet-isolated worker process is spawned by the func host and probes
# for its dependencies in $FuncDir.  If any required assembly is absent the
# worker crashes immediately with a FileNotFoundException and the host enters
# an infinite restart loop - burning the entire timeout with no useful output.
# Catching this here gives a fast, actionable failure instead.
#
# -----------------------------------------------------------------------------

Write-PipelineSection "Pre-flight: checking required assemblies in '$FuncDir'..."

# Each entry is @{ File = "name.dll"; MinVersion = [version]"x.y.z.w" }
# MinVersion is the minimum acceptable assembly version (inclusive).
$RequiredAssemblies = @(
    @{ File = "Google.Protobuf.dll"; MinVersion = [version]"3.25.2.0" }
)

$missingAssemblies  = @()
$wrongVersionAsms   = @()

foreach ($req in $RequiredAssemblies) {
    $path = "$FuncDir\$($req.File)"

    if (-not (Test-Path $path)) {
        Write-PipelineError "  [MISSING] $($req.File)"
        $missingAssemblies += $req.File
        continue
    }

    $asmName  = [System.Reflection.AssemblyName]::GetAssemblyName($path)
    $actual   = $asmName.Version
    $required = $req.MinVersion

    if ($actual -lt $required) {
        Write-PipelineError ("  [WRONG VERSION] $($req.File) — found $actual, need >= $required")
        $wrongVersionAsms += "$($req.File) (found $actual, need >= $required)"
    } else {
        Write-Host "  [OK] $($req.File) — $actual"
    }
}

if ($missingAssemblies.Count -gt 0) {
    Fail-Pipeline ("One or more required assemblies are missing from '$FuncDir': " +
        ($missingAssemblies -join ", ") +
        ".  The dotnet-isolated worker cannot start without them.  " +
        "Ensure each is an explicit <PackageReference> in the project file so MSBuild copies it to the output directory.")
}

if ($wrongVersionAsms.Count -gt 0) {
    Fail-Pipeline ("One or more assemblies in '$FuncDir' are the wrong version: " +
        ($wrongVersionAsms -join "; ") +
        ".  Another project in the solution is likely overwriting the correct DLL with an older transitive copy.  " +
        "Pin the required version in Directory.Build.props so every project resolves the same version.")
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Copy workflow resources if requested
# -----------------------------------------------------------------------------

if ($ResourcesPath) {
    Write-PipelineSection "Copying resources from '$ResourcesPath'..."
    $TargetResourcesDir = "$FuncDir\Resources"
    if (!(Test-Path $TargetResourcesDir)) {
        New-Item -ItemType Directory -Path $TargetResourcesDir | Out-Null
    }
    if (Test-Path "$ResourcesPath\Resources") {
        Copy-Item -Path "$ResourcesPath\Resources\*" -Destination $TargetResourcesDir -Recurse -Force
    } elseif (Test-Path "$PSScriptRoot\Resources - $ResourcesPath\Resources") {
        Copy-Item -Path "$PSScriptRoot\Resources - $ResourcesPath\Resources\*" -Destination $TargetResourcesDir -Recurse -Force
    } elseif (Test-Path "$PSScriptRoot\..\Resources - $ResourcesPath\Resources") {
        Copy-Item -Path "$PSScriptRoot\..\Resources - $ResourcesPath\Resources\*" -Destination $TargetResourcesDir -Recurse -Force
    } else {
        Write-PipelineWarning "Resources path not found for '$ResourcesPath'"
    }
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Ensure TestResults directory exists
# -----------------------------------------------------------------------------

if (!(Test-Path "$PSScriptRoot\TestResults")) {
    New-Item -ItemType Directory "$PSScriptRoot\TestResults" | Out-Null
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Resolve how to invoke func (handles .ps1 / .cmd / .exe wrappers)
# -----------------------------------------------------------------------------

Write-PipelineSection "Starting Azure Functions host..."

# For HTTP-only functions, AzureWebJobsStorage is not required at runtime.
# Override to empty string if not already set so the host starts without Azurite.
if (!$env:AzureWebJobsStorage) {
    $env:AzureWebJobsStorage = ""
}

# func v4.9+ prompts interactively to select the worker runtime when
# FUNCTIONS_WORKER_RUNTIME is not set and local.settings.json is absent
# (local.settings.json is intentionally excluded from CI artifacts).
# Set the default here so the host starts non-interactively.
if (!$env:FUNCTIONS_WORKER_RUNTIME) {
    $env:FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
}

Write-Host "  func    : $FuncExe"
Write-Host "  root    : $FuncDir"
Write-Host "  port    : $Port"
Write-Host "  timeout : ${TimeoutSeconds}s"

# Start-Process requires a Win32 executable as FilePath.
# func is often a .ps1 or .cmd wrapper installed by npm on Windows;
# on Linux it is a plain binary - switch on extension accordingly.
$ext = [System.IO.Path]::GetExtension($FuncExe).ToLower()
switch ($ext) {
    '.ps1' {
        $StartFilePath = (Get-Command powershell.exe -ErrorAction SilentlyContinue).Source
        if (-not $StartFilePath) { $StartFilePath = "pwsh" }
        $StartArgs = "-NoProfile -NoLogo -File `"$FuncExe`" start --port $Port"
    }
    '.cmd' {
        $StartFilePath = "$env:ComSpec"
        $StartArgs     = "/c `"$FuncExe`" start --port $Port"
    }
    default {
        $StartFilePath = $FuncExe
        $StartArgs     = "start --port $Port"
    }
}

$FuncProcess = Start-Process `
    -FilePath    $StartFilePath `
    -ArgumentList $StartArgs `
    -WorkingDirectory $FuncDir `
    -RedirectStandardOutput "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" `
    -RedirectStandardError  "$PSScriptRoot\TestResults\AzureFunctionError.txt" `
    -PassThru

if (-not $FuncProcess) {
    Fail-Pipeline "Start-Process did not return a process object - func failed to launch."
}

Write-Host "func host launched. PID: $($FuncProcess.Id)"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 4. Wait for host to be ready - HTTP health probe with timeout
# -----------------------------------------------------------------------------
#
# We use /admin/host/ping (returns HTTP 200 only when the host is fully
# initialised and ready to serve requests) rather than a raw TCP probe.
# A TCP connect can succeed briefly while the host is still loading or
# crashing, producing a false-positive that causes all tests to fail.
#
# -----------------------------------------------------------------------------

Write-PipelineSection "Waiting for Azure Functions host to be ready on port $Port (timeout: ${TimeoutSeconds}s)..."

$pingUrl = "http://localhost:$Port/admin/host/ping"
$elapsed = 0
$Ready   = $false

while ($elapsed -lt $TimeoutSeconds -and !$Ready) {
    Start-Sleep $PollIntervalSec
    $elapsed += $PollIntervalSec

    # Detect early exit before timeout
    if ($FuncProcess.HasExited) {
        Fail-Pipeline "Azure Functions host exited unexpectedly after ${elapsed}s (exit code $($FuncProcess.ExitCode))."
    }

    # HTTP health probe - only 200 means the host is fully ready
    try {
        $response = Invoke-WebRequest -Uri $pingUrl -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            $Ready = $true
            Write-Host "  [${elapsed}s] $pingUrl -> $($response.StatusCode) [OK]"
        } else {
            Write-Host "  [${elapsed}s] $pingUrl -> $($response.StatusCode) (not ready yet)"
        }
    } catch {
        Write-Host "  [${elapsed}s] $pingUrl - not ready yet: $($_.Exception.Message)"
    }
}

if (!$Ready) {
    Fail-Pipeline "Azure Functions host did not respond on $pingUrl within ${TimeoutSeconds}s."
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Success - print summary
# -----------------------------------------------------------------------------

Write-Host ""
Write-Host "##[section]Azure Functions host is ready [OK]"
Write-Host "  url     : http://localhost:$Port"
Write-Host "  pid     : $($FuncProcess.Id)"
Write-Host "  elapsed : ${elapsed}s"
Write-Host "  exited  : $($FuncProcess.HasExited)"

Write-Host ""
Write-Host "--- AzureFunctionOutput.txt (last 20 lines) ---"
Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -ErrorAction SilentlyContinue | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
Write-Host "--- AzureFunctionError.txt (last 20 lines) ---"
Get-Content "$PSScriptRoot\TestResults\AzureFunctionError.txt"  -ErrorAction SilentlyContinue | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
Write-Host "-----------------------------------------------"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Tail log (interactive / non-pipeline mode)
# -----------------------------------------------------------------------------

if (!$DoExit.IsPresent) {
    Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -Wait
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------
