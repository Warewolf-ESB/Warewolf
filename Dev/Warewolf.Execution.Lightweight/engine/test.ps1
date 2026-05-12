# ============================================================
# test.ps1
# Publish test assemblies and run them inside the container
#
# Prerequisites:
#   - Container must already be running (run run.ps1 first)
#   - Container must have been started with the /tests volume mount
#     (run.ps1 does this automatically)
#
# What this does:
#   1. Publishes the test project as a self-contained linux-x64 binary
#      (no SDK required in the container — only the .NET 8 runtime)
#   2. Runs UNIT tests inside the container via docker exec
#      (filters out integration test categories)
#   3. Runs INTEGRATION tests from the host via dotnet test
#      (they call http://localhost:7071 which maps to the container)
# ============================================================

# ── Configuration ───────────────────────────────────────────
$TestProjectFile    = "$PSScriptRoot\..\..\Warewolf.Execution.Lightweight.Tests\Warewolf.Execution.Lightweight.Tests.csproj"
$TestPublishDir     = "$PSScriptRoot\publish-tests"
$ContainerName      = "ExecutionEngine_debug"
$TestBinary         = "Warewolf.Execution.Lightweight.Tests"

# Add new integration categories here as they are created
$IntegrationCategories = @(
    "WebPostTool_Integration",
    "WebGetTool_Integration"
)

# Filter string that excludes all integration categories (for unit test run)
$UnitTestFilter = ($IntegrationCategories | ForEach-Object { "TestCategory!=$_" }) -join "&"

# Filter string that includes only integration categories (for integration test run from host)
$IntegrationFilter = ($IntegrationCategories | ForEach-Object { "TestCategory=$_" }) -join "|"
# ────────────────────────────────────────────────────────────

function Write-Step($message) {
    Write-Host ""
    Write-Host "===================================================" -ForegroundColor Cyan
    Write-Host "  $message" -ForegroundColor Cyan
    Write-Host "===================================================" -ForegroundColor Cyan
}

function Abort($message) {
    Write-Host ""
    Write-Host "ERROR: $message" -ForegroundColor Red
    Write-Host "Aborting." -ForegroundColor Red
    exit 1
}

# ── Step 1: Verify container is running ─────────────────────
Write-Step "Step 1/3 - Checking container"

$running = docker ps --filter "name=^${ContainerName}$" --filter "status=running" -q
if (-not $running) {
    Abort "Container '$ContainerName' is not running. Run run.ps1 first."
}

Write-Host "Container is running: $ContainerName" -ForegroundColor Green

# ── Step 2: Publish test project for linux-x64 ──────────────
Write-Step "Step 2/3 - Publishing test project (linux-x64, self-contained)"

if (Test-Path $TestPublishDir) {
    Remove-Item -Recurse -Force $TestPublishDir
    Write-Host "Cleaned: $TestPublishDir" -ForegroundColor Yellow
}

dotnet publish $TestProjectFile `
    -c Debug `
    -r linux-x64 `
    --self-contained true `
    -o $TestPublishDir

if ($LASTEXITCODE -ne 0) {
    Abort "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Published to: $TestPublishDir" -ForegroundColor Green

# ── Step 3a: Run unit tests inside the container ────────────
Write-Step "Step 3/3 - Running unit tests inside container (docker exec)"
Write-Host "Filter: $UnitTestFilter" -ForegroundColor Gray

docker exec $ContainerName `
    /tests/$TestBinary `
    --filter "$UnitTestFilter"

$unitExitCode = $LASTEXITCODE

if ($unitExitCode -ne 0) {
    Write-Host ""
    Write-Host "Unit tests FAILED (exit code $unitExitCode)" -ForegroundColor Red
} else {
    Write-Host "Unit tests PASSED" -ForegroundColor Green
}

# ── Step 3b: Run integration tests from the host ────────────
Write-Step "Step 3/3 - Running integration tests from host (http://localhost:7071)"
Write-Host "Filter: $IntegrationFilter" -ForegroundColor Gray

dotnet test $TestProjectFile `
    --filter "$IntegrationFilter" `
    --logger "console;verbosity=normal"

$integrationExitCode = $LASTEXITCODE

if ($integrationExitCode -ne 0) {
    Write-Host ""
    Write-Host "Integration tests FAILED (exit code $integrationExitCode)" -ForegroundColor Red
} else {
    Write-Host "Integration tests PASSED" -ForegroundColor Green
}

# ── Summary ──────────────────────────────────────────────────
Write-Host ""
Write-Host "===================================================" -ForegroundColor $(if ($unitExitCode -eq 0 -and $integrationExitCode -eq 0) { "Green" } else { "Red" })
Write-Host "  Results" -ForegroundColor White
Write-Host "    Unit tests        : $(if ($unitExitCode -eq 0) { 'PASSED' } else { 'FAILED' })" -ForegroundColor $(if ($unitExitCode -eq 0) { "Green" } else { "Red" })
Write-Host "    Integration tests : $(if ($integrationExitCode -eq 0) { 'PASSED' } else { 'FAILED' })" -ForegroundColor $(if ($integrationExitCode -eq 0) { "Green" } else { "Red" })
Write-Host "===================================================" -ForegroundColor White

if ($unitExitCode -ne 0 -or $integrationExitCode -ne 0) {
    exit 1
}
