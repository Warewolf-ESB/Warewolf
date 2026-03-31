Param(
  [switch]$DoExit,
  [string]$ResourcesPath,
  [string]$FunctionPath,
  [switch]$Cleanup
)

# Kill any existing func host processes
if ($Cleanup.IsPresent) {
    $FuncProcesses = Get-Process "func" -ErrorAction SilentlyContinue
    if ($FuncProcesses) {
        $FuncProcesses | Stop-Process -Force
        Start-Sleep 2
    }
}

# Determine the function directory (where host.json lives)
if ($FunctionPath -and (Test-Path "$FunctionPath\host.json")) {
    $FuncDir = $FunctionPath
} elseif (Test-Path "$PSScriptRoot\host.json") {
    $FuncDir = $PSScriptRoot
} elseif (Test-Path "$PSScriptRoot\Warewolf.Execution.Lightweight\host.json") {
    $FuncDir = "$PSScriptRoot\Warewolf.Execution.Lightweight"
} else {
    Write-Error "Cannot find host.json. Use -FunctionPath to specify the Azure Functions directory."
    exit 1
}

# Copy workflow resources if requested
if ($ResourcesPath) {
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
        Write-Warning "Resources path not found for '$ResourcesPath'"
    }
}

# Ensure TestResults directory exists
if (!(Test-Path "$PSScriptRoot\TestResults")) {
    New-Item -ItemType Directory "$PSScriptRoot\TestResults" | Out-Null
}

# Find the func CLI
$FuncExe = $null
$FuncCommand = Get-Command "func" -ErrorAction SilentlyContinue
if ($FuncCommand) {
    $FuncExe = $FuncCommand.Source
} else {
    $CommonPaths = @(
        "$env:APPDATA\npm\func.cmd",
        "$env:ProgramFiles\Microsoft\Azure Functions Core Tools\func.exe",
        "C:\tools\azure-functions-core-tools\func.cmd"
    )
    foreach ($path in $CommonPaths) {
        if (Test-Path $path) {
            $FuncExe = $path
            break
        }
    }
}

if (!$FuncExe) {
    Write-Error "Azure Functions Core Tools (func) not found. Install with: npm install -g azure-functions-core-tools@4"
    exit 1
}

# For HTTP-only functions, AzureWebJobsStorage is not required at runtime.
# Override it to empty string if not already set so the host starts without Azurite.
if (!$env:AzureWebJobsStorage) {
    $env:AzureWebJobsStorage = ""
}

Write-Host "Starting Azure Functions host..."
Write-Host "  func    : $FuncExe"
Write-Host "  root    : $FuncDir"

# Start-Process requires a Win32 executable as FilePath.
# func is often a .ps1 or .cmd wrapper installed by npm, so we delegate to the
# appropriate host process based on file extension.
$ext = [System.IO.Path]::GetExtension($FuncExe).ToLower()
switch ($ext) {
    '.ps1' {
        $StartFilePath = (Get-Command powershell.exe).Source
        $StartArgs     = "-NoProfile -NoLogo -File `"$FuncExe`" start"
    }
    '.cmd' {
        $StartFilePath = "$env:ComSpec"
        $StartArgs     = "/c `"$FuncExe`" start"
    }
    default {
        $StartFilePath = $FuncExe
        $StartArgs     = "start"
    }
}

$FuncProcess = Start-Process `
    -FilePath $StartFilePath `
    -ArgumentList $StartArgs `
    -WorkingDirectory $FuncDir `
    -RedirectStandardOutput "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" `
    -RedirectStandardError  "$PSScriptRoot\TestResults\AzureFunctionError.txt" `
    -PassThru

# Wait for the host to accept TCP connections on port 7071
$LoopCounter  = 0
$LoopCounterMax = 60
$Ready = $false

while ($LoopCounter++ -lt $LoopCounterMax -and !$Ready) {
    Start-Sleep 5

    if ($FuncProcess.HasExited) {
        Write-Error "Azure Functions host exited unexpectedly (code $($FuncProcess.ExitCode))."
        Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -ErrorAction SilentlyContinue | Select-Object -Last 50
        exit 1
    }

    $tcp = New-Object System.Net.Sockets.TcpClient
    try {
        $tcp.Connect("localhost", 7071)
        $Ready = $true
    } catch {
        Write-Host "Still waiting for Azure Functions host to start... ($LoopCounter/$LoopCounterMax)"
    } finally {
        $tcp.Close()
    }
}

if (!$Ready) {
    Write-Error "Azure Functions host did not start within the expected time."
    Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -ErrorAction SilentlyContinue | Select-Object -Last 50
    exit 1
}

Write-Host "Azure Functions host is ready on http://localhost:7071"
Write-Host "  pid     : $($FuncProcess.Id)"
Write-Host "  exited  : $($FuncProcess.HasExited)"
Write-Host "--- AzureFunctionOutput.txt (last 20 lines) ---"
Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -ErrorAction SilentlyContinue | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
Write-Host "--- AzureFunctionError.txt (last 20 lines) ---"
Get-Content "$PSScriptRoot\TestResults\AzureFunctionError.txt" -ErrorAction SilentlyContinue | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
Write-Host "-----------------------------------------------"

if (!$DoExit.IsPresent) {
    Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -Wait
}
