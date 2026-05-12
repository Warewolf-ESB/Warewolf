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

# ── Load Key Vault env vars from local.settings.json ─────────
# These are forwarded into the container so the credential chain
# (AzureCliCredential) can reach the same vault as local runs.
# Only non-empty values are forwarded; leave a field blank to omit it.
$LocalSettingsFile = "$PSScriptRoot\..\local.settings.json"
$KvArgs = @()

if (Test-Path $LocalSettingsFile) {
    $ls = Get-Content $LocalSettingsFile -Raw | ConvertFrom-Json
    $keysToForward = @(
        "AZURE_KEYVAULT_NAME",
        "KEYVAULT_SECRET_NAME",
        "DEBUG_AZURE_KEYVAULT_SECRET",
        "AZURE_TENANT_ID",
        "AZURE_CLIENT_ID",
        "AZURE_CLIENT_SECRET",
        "SkipFailureToRetrieveSecret",
        "WorkflowsDirectory"
    )
    foreach ($key in $keysToForward) {
        $val = $ls.Values.$key
        if (-not [string]::IsNullOrWhiteSpace($val)) {
            $KvArgs += "-e"
            $KvArgs += "${key}=${val}"
        }
    }
    if ($KvArgs.Count -gt 0) {
        Write-Host "Forwarding from local.settings.json:" -ForegroundColor DarkGray
        for ($i = 0; $i -lt $KvArgs.Count; $i += 2) {
            Write-Host "  $($KvArgs[$i]) $($KvArgs[$i+1])" -ForegroundColor DarkGray
        }
    }
} else {
    Write-Host "Warning: local.settings.json not found - Key Vault env vars will not be forwarded." -ForegroundColor Yellow
}

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

# ── Skip-publish prompt ──────────────────────────────────────
# Offered only when a valid previous publish already exists so the user
# can skip the slow dotnet publish + clean when nothing has changed.
$skipPublish = $false
if (Test-Path "$PublishDir\.azurefunctions") {
    $publishedAt = (Get-Item $PublishDir).LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")
    $answer = Read-Host "Existing publish found (last built: $publishedAt). Re-publish? [y/N]"
    $skipPublish = ($answer -notmatch '^[Yy]')
}
# ─────────────────────────────────────────────────────────────

# ── Step 1: Clean publish directory ─────────────────────────
Write-Step "Step 1/4 - Cleaning publish directory"

if ($skipPublish) {
    Write-Host "  Skipped." -ForegroundColor DarkGray
} else {
    if (Test-Path $PublishDir) {
        Remove-Item -Recurse -Force $PublishDir
        Write-Host "Cleaned: $PublishDir" -ForegroundColor Yellow
    }
    New-Item -ItemType Directory -Path $PublishDir | Out-Null
    Write-Host "Created: $PublishDir" -ForegroundColor Green
}

# ── Step 2: Publish project ──────────────────────────────────
Write-Step "Step 2/4 - Publishing project"

if ($skipPublish) {
    Write-Host "  Skipped." -ForegroundColor DarkGray
} else {
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
}

# ── Step 3: Build Docker image ───────────────────────────────
Write-Step "Step 3/4 - Building Docker image"

if (-not (Test-Path $DockerfilePath)) {
    Abort "Dockerfile not found: $DockerfilePath"
}

# Pre-pull the base image before docker build.
# BuildKit sends a HEAD request to MCR to resolve the manifest before pulling
# layers; that request can fail with EOF due to transient Docker Desktop /
# MCR connectivity issues on Windows. Pulling first populates the local daemon
# cache so BuildKit uses it directly and skips the live manifest check.
$BaseImage = (Select-String -Path $DockerfilePath -Pattern '^FROM\s+(\S+)' |
    Select-Object -First 1).Matches.Groups[1].Value

if ($BaseImage) {
    Write-Host "Pre-pulling base image: $BaseImage" -ForegroundColor DarkGray
    $MaxPullRetries = 3
    $pulled = $false
    for ($attempt = 1; $attempt -le $MaxPullRetries; $attempt++) {
        docker pull $BaseImage
        if ($LASTEXITCODE -eq 0) { $pulled = $true; break }
        if ($attempt -lt $MaxPullRetries) {
            Write-Host "Pull attempt $attempt/$MaxPullRetries failed - retrying in 10 s..." -ForegroundColor Yellow
            Start-Sleep -Seconds 10
        }
    }
    if (-not $pulled) {
        Write-Host "Warning: base image pull failed after $MaxPullRetries attempts - build will use local cache if available." -ForegroundColor Yellow
    }
}

docker build -f $DockerfilePath -t $FullImageName $BuildContext

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

$RunArgs = @(
    "run", "-d",
    "-p", "${HostPort}:80",
    "-p", "${DebuggerPort}:4024",
    "--name", $ContainerName,
    "-e", "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated",
    "-e", "AzureWebJobsStorage=UseDevelopmentStorage=false",
    "-e", "ASPNETCORE_ENVIRONMENT=Development"
) + $KvArgs + @($FullImageName)

# Print what we're running for diagnostics
$printArgs = $RunArgs | ForEach-Object {
    if ($_ -match '\s') { "`"$_`"" } else { $_ }
}
Write-Host "docker $($printArgs -join ' ')" -ForegroundColor DarkGray

& docker @RunArgs

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

Write-Host "Waiting 8 seconds to confirm container startup..." -ForegroundColor Yellow
Start-Sleep -Seconds 8
docker logs $ContainerName