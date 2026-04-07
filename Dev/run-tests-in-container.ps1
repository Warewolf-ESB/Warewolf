# run-tests-in-container.ps1
# Runs tests directly inside the vsut_dockerfile container via vstest.console.dll.
# Windows path C:\Users\ultra\warewolf maps to /mnt/approot inside the container.
# VS vstest tools are mounted at /mnt/vstest inside the container.

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Find or start the test container ─────────────────────────────────────────
$containerId = docker ps --filter "ancestor=vsut_dockerfile" --format "{{.ID}}" 2>$null | Select-Object -First 1

if (-not $containerId) {
    # Check the image exists — build it if not
    $imageExists = docker images vsut_dockerfile --format "{{.ID}}" 2>$null
    if (-not $imageExists) {
        Write-Host "Image vsut_dockerfile not found. Building..." -ForegroundColor Yellow
        $dockerfile = "C:\Users\ultra\warewolf\Dev\Warewolf.Execution.Lightweight\engine\docker\Dockerfile.test"
        docker build -t vsut_dockerfile -f $dockerfile "C:\Users\ultra\warewolf\Dev\Warewolf.Execution.Lightweight\engine\docker"
        if ($LASTEXITCODE -ne 0) { Write-Error "Image build failed."; exit 1 }
    }

    Write-Host "Starting container..." -ForegroundColor Yellow
    $containerId = docker run -d `
        -v "C:\Users\ultra\warewolf:/mnt/approot" `
        -v "C:\Program Files\Microsoft Visual Studio\18\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\VsTest:/mnt/vstest" `
        vsut_dockerfile
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to start container."; exit 1 }

    # Give the container a moment to initialise
    Write-Host "Waiting for container to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
}

Write-Host "Using container: $containerId" -ForegroundColor Cyan

# ── Prompt ────────────────────────────────────────────────────────────────────
$assembly = Read-Host "Assembly name (e.g. Dev2.Activities.Tests)"
Write-Host "Filter examples: MyTestMethod / TestCategory=MyCategory / FullyQualifiedName~ClassName / (blank = all)"
$filterInput = Read-Host "Filter"

# ── Locate the DLL on the Windows filesystem ─────────────────────────────────
$dll = Get-ChildItem -Path "C:\Users\ultra\warewolf\Dev" -Recurse -Filter "$assembly.dll" `
    | Where-Object {
        $_.FullName -match "\\bin\\Debug\\net8\.0\\" -and
        $_.FullName -match "\\$([regex]::Escape($assembly))\\bin\\"
    } `
    | Select-Object -First 1

if (-not $dll) {
    Write-Error "Could not find $assembly\bin\Debug\net8.0\$assembly.dll. Build the project first."
    exit 1
}

# ── Convert Windows path → container path (/mnt/approot) ─────────────────────
$containerPath = $dll.FullName -replace [regex]::Escape("C:\Users\ultra\warewolf\"), "/mnt/approot/"
$containerPath = $containerPath -replace "\\", "/"
Write-Host "Container DLL path: $containerPath" -ForegroundColor Cyan

# ── Verify the DLL exists in the container ────────────────────────────────────
docker exec $containerId test -f $containerPath 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error "DLL not found inside container at: $containerPath`nEnsure the project is built and the workspace is mounted."
    exit 1
}

# ── Build vstest.console command ─────────────────────────────────────────────
# vstest.console filter syntax: /TestCaseFilter:"FullyQualifiedName~Foo&TestCategory=Bar"
$dotnet   = "/usr/share/dotnet/dotnet"
$vstest   = "/mnt/vstest/vstest.console.dll"
$cmd = @($dotnet, $vstest, $containerPath, "/logger:console;verbosity=normal")

if ($filterInput.Trim()) {
    $filter = if ($filterInput -match "[=~!<>]") { $filterInput } else { "FullyQualifiedName~$filterInput" }
    $cmd += "/TestCaseFilter:`"$filter`""
}

Write-Host ""
Write-Host "Running: docker exec $containerId $($cmd -join ' ')" -ForegroundColor Yellow
Write-Host ""

# ── Execute ───────────────────────────────────────────────────────────────────
docker exec $containerId @cmd
exit $LASTEXITCODE
