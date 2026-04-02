# ============================================================
# run.ps1
# Publish, Build Docker Image, and Start Container
# ============================================================

# ── Configuration ───────────────────────────────────────────
$ProjectFile    = "$PSScriptRoot\..\Warewolf.Execution.Lightweight.csproj"
$PublishDir     = "$PSScriptRoot\publish"
$DockerfilePath = "$PSScriptRoot\docker\Dockerfile"
$BuildContext   = $PSScriptRoot
$ImageName      = "executionengine"
$ImageTag       = "debug"
$ContainerName  = "ExecutionEngine_debug"
$HostPort       = 7071
$DebuggerPort   = 4024
# ────────────────────────────────────────────────────────────

$FullImageName = "${ImageName}:${ImageTag}"

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

# ── Step 1: Clean publish directory ─────────────────────────
Write-Step "Step 1/4 - Cleaning publish directory"

if (Test-Path $PublishDir) {
    Remove-Item -Recurse -Force $PublishDir
    Write-Host "Cleaned: $PublishDir" -ForegroundColor Yellow
}
New-Item -ItemType Directory -Path $PublishDir | Out-Null
Write-Host "Created: $PublishDir" -ForegroundColor Green

# ── Step 2: Publish project ──────────────────────────────────
Write-Step "Step 2/4 - Publishing project"

if (-not (Test-Path $ProjectFile)) {
    Abort "Project file not found: $ProjectFile"
}

dotnet publish $ProjectFile `
    -c Debug `
    -o $PublishDir `
    /p:UseAppHost=false `
    /p:DebugType=portable `
    /p:DebugSymbols=true `
    /p:Optimize=false

if ($LASTEXITCODE -ne 0) {
    Abort "dotnet publish failed with exit code $LASTEXITCODE"
}

if (-not (Test-Path "$PublishDir\.azurefunctions")) {
    Abort ".azurefunctions folder missing from publish output. Check your .csproj is set up for isolated worker model."
}

Write-Host "Publish successful." -ForegroundColor Green

# ── Step 3: Build Docker image ───────────────────────────────
Write-Step "Step 3/4 - Building Docker image"

if (-not (Test-Path $DockerfilePath)) {
    Abort "Dockerfile not found: $DockerfilePath"
}

docker build --platform linux/amd64 -f $DockerfilePath -t $FullImageName $BuildContext

if ($LASTEXITCODE -ne 0) {
    Abort "docker build failed with exit code $LASTEXITCODE"
}

Write-Host "Docker image built: $FullImageName" -ForegroundColor Green

# ── Step 4: Stop and remove existing container ───────────────
Write-Step "Step 4/4 - Starting container"

$existing = docker ps -aq --filter "name=^${ContainerName}$"
if ($existing) {
    Write-Host "Stopping and removing existing container: $ContainerName" -ForegroundColor Yellow
    docker rm -f $ContainerName | Out-Null
}

Write-Host "docker run -d `
    -p ${HostPort}:80 `
    -p ${DebuggerPort}:4024 `
    --name $ContainerName `
    -e FUNCTIONS_WORKER_RUNTIME=dotnet-isolated `
    -e ASPNETCORE_ENVIRONMENT=Development `
    $FullImageName"
	
docker run -d `
    -p "${HostPort}:80" `
    -p "${DebuggerPort}:4024" `
    --name $ContainerName `
    -e FUNCTIONS_WORKER_RUNTIME=dotnet-isolated `
    -e AzureWebJobsStorage=UseDevelopmentStorage=false `
    -e ASPNETCORE_ENVIRONMENT=Development `
    $FullImageName

if ($LASTEXITCODE -ne 0) {
    Abort "docker run failed with exit code $LASTEXITCODE"
}

Write-Host "Container started: $ContainerName" -ForegroundColor Green

# ── Done ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "===================================================" -ForegroundColor Green
Write-Host "  All steps completed successfully!" -ForegroundColor Green
Write-Host "===================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Function URL : http://localhost:$HostPort" -ForegroundColor White
Write-Host "  Container    : $ContainerName" -ForegroundColor White
Write-Host "  Image        : $FullImageName" -ForegroundColor White
Write-Host ""
Write-Host "  To watch logs  : docker logs $ContainerName --follow" -ForegroundColor Gray
Write-Host "  To stop        : docker rm -f $ContainerName" -ForegroundColor Gray
Write-Host ""

Write-Host "Waiting 5 seconds to confirm container startup..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
docker logs $ContainerName
