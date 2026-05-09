# Run-Coverage.ps1
# Runs all pipeline coverage jobs locally in Linux Docker containers,
# merges the coverage snapshots, and generates an HTML report.
#
# Usage:
#   .\Run-Coverage.ps1
#   .\Run-Coverage.ps1 -Jobs Unit,Activities -SkipBuild
#   .\Run-Coverage.ps1 -NoParallel -SkipReport
#   .\Run-Coverage.ps1 -Jobs Integration -SkipBuild -ReportFormat Cobertura

[CmdletBinding()]
param(
    [ValidateSet('Unit', 'Activities', 'Lightweight', 'Integration')]
    [string[]]$Jobs = @('Unit', 'Activities', 'Lightweight', 'Integration'),

    # Where to write coverage artifacts and the final HTML report
    [string]$OutputDir = 'coverage',

    # Skip dotnet publish (Compile.ps1 -ServerTests)
    [switch]$SkipBuild,

    # Skip reportgenerator HTML generation
    [switch]$SkipReport,

    [ValidateSet('Html','Badges','Cobertura','TextSummary','HtmlSummary','MarkdownSummary')]
    [string]$ReportFormat = 'Html',

    # Disable parallel job execution (Unit / Activities / Lightweight run sequentially)
    [switch]$NoParallel
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

# --- Paths --------------------------------------------------------------------
# Script lives in Dev\; repo root is one level up
$Root           = Split-Path $PSScriptRoot -Parent
$BinRoot        = Join-Path $Root 'Bin'
$ServerTestsBin = Join-Path $BinRoot 'ServerTests'
$NewServerBin   = $ServerTestsBin   # server app and tests now publish to the same dir
$CoverageDir    = [System.IO.Path]::GetFullPath((Join-Path $Root $OutputDir))
$SettingsFile   = Join-Path $Root 'coverage-settings.xml'
$FilterScript   = Join-Path $Root 'Dev' '.azure' 'filter_coverage.py'
$DockerfileTest = Join-Path $Root 'Dev' 'Warewolf.Execution.Lightweight' 'engine' 'docker' 'Dockerfile.test'
$DockerContext  = Split-Path $DockerfileTest -Parent

$RunId   = Get-Date -Format 'yyyyMMddHHmmss'
$NetName = "ww-cov-$RunId"

# Reference-type list so Invoke-IntegrationJob can mutate it from its own scope
$ToCleanup = [System.Collections.Generic.List[string]]::new()

# --- Logging ------------------------------------------------------------------
function Write-Step($m) { Write-Host "`n==> $m" -ForegroundColor Cyan }
function Write-Done($m) { Write-Host "    OK  $m" -ForegroundColor Green }
function Write-Warn($m) { Write-Host "    !!  $m" -ForegroundColor Yellow }

function Test-Exit([string]$label) {
    if ($LASTEXITCODE -ne 0) {
        Write-Error "$label failed (exit $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
}

# --- Docker path helper -------------------------------------------------------
# Docker Desktop on Windows accepts C:/foo/bar in -v mounts
function dp([string]$path) {
    [System.IO.Path]::GetFullPath($path) -replace '\\', '/'
}

# --- Prerequisite checks / auto-install ---------------------------------------
function Ensure-GlobalTool([string]$packageId) {
    if (-not ((dotnet tool list --global 2>$null) -match [regex]::Escape($packageId))) {
        Write-Step "Auto-installing $packageId..."
        dotnet tool install --global $packageId
        Test-Exit "dotnet tool install $packageId"
    }
}

# --- Banner -------------------------------------------------------------------
Write-Host ''
Write-Host '  Warewolf Coverage Build  (local)' -ForegroundColor White
Write-Host "  Jobs     : $($Jobs -join ', ')" -ForegroundColor Gray
Write-Host "  Output   : $CoverageDir" -ForegroundColor Gray
Write-Host "  Parallel : $(-not $NoParallel.IsPresent)" -ForegroundColor Gray
Write-Host ''

Write-Step 'Checking prerequisites...'
Ensure-GlobalTool 'dotnet-coverage'
if (-not $SkipReport) { Ensure-GlobalTool 'dotnet-reportgenerator-globaltool' }
if (-not (docker info 2>$null)) { Write-Error 'Docker is not running. Start Docker Desktop first.'; exit 1 }
Write-Done 'Prerequisites OK'

# --- Build --------------------------------------------------------------------
if (-not $SkipBuild) {
    Write-Step 'Building ServerTests (linux-x64)...'
    & "$Root\Compile.ps1" -ServerTests -ProjectSpecificOutputs
    Test-Exit 'Compile.ps1'
    Write-Done "Build outputs -> Bin\ServerTests"
} else {
    Write-Step 'Skipping build (-SkipBuild)'
    if (-not (Test-Path $ServerTestsBin)) {
        Write-Error "Build output missing: $ServerTestsBin`nRun without -SkipBuild first."
        exit 1
    }
}

# --- Output directories -------------------------------------------------------
foreach ($sub in 'unit', 'activities', 'lightweight', 'integration', 'merged') {
    New-Item -ItemType Directory -Force -Path "$CoverageDir\$sub" | Out-Null
}

# --- Docker images ------------------------------------------------------------
Write-Step 'Building warewolf-test-env (Azure Functions + .NET 8 SDK)...'

# Neutralise the ENTRYPOINT so `docker run <cmd>` executes <cmd> directly.
# run-tests-in-container.ps1 normally does this; we mirror it here so we don't
# depend on that script being invoked first.
$dtfContent = Get-Content $DockerfileTest -Raw
if ($dtfContent -match 'ENTRYPOINT \["/bin/bash"\]') {
    ($dtfContent -replace 'ENTRYPOINT \["/bin/bash"\]', 'ENTRYPOINT []') |
        Set-Content $DockerfileTest -Encoding UTF8
}

docker build -q -t warewolf-test-env -f $DockerfileTest $DockerContext
Test-Exit 'Build warewolf-test-env'
Write-Done 'warewolf-test-env ready'

Write-Step 'Building warewolf-coverage-env (adds dotnet-coverage + Azure Functions CLI)...'
# Note: `$PATH below is backtick-escaped so PowerShell emits literal $PATH for Dockerfile
@"
FROM warewolf-test-env
RUN apt-get update \
 && apt-get install -y --no-install-recommends libxml2 curl gnupg \
 && curl -sS https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft.gpg \
 && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/dotnetdev.list \
 && apt-get update \
 && apt-get install -y --no-install-recommends azure-functions-core-tools-4 \
 && rm -rf /var/lib/apt/lists/*
RUN dotnet tool install --tool-path /opt/dotnet-tools dotnet-coverage
ENV PATH="/opt/dotnet-tools:/root/.dotnet/tools:`$PATH"
"@ | docker build -q -t warewolf-coverage-env -
Test-Exit 'Build warewolf-coverage-env'
Write-Done 'warewolf-coverage-env ready'

# --- Filter constants (mirror pipeline exactly) -------------------------------
$UnitExclude = 'Warewolf\.Execution\.Lightweight\.(Tests|Integration\.Tests)|Dev2\.(Integration|Activities)\.Tests'

$UnitFilter = @(
    'TestCategory!=CannotParallelize'
    'TestCategory!=ResourceCatalog_LoadTests'
    'TestCategory!=PluginRuntimeHandler'
    'TestCategory!=GatherSystemInformation'
    'TestCategory!=LocalSchedulerAdmin'
    'TestCategory!=Multithread'
    'TestCategory!=AnonymousRedis'
    'TestCategory!=COMIPCSaxonCSandStudioTests'
    'TestCategory!=WFWithRabbitMqConsumeTimeout5'
    'TestCategory!=WarewolfCOMIPCClient_Deprecated'
    'TestCategory!=WebPostTool_Integration'
    'TestCategory!=WebGetTool_Integration'
) -join '&'

$ActivitiesFilter = @(
    'TestCategory!=COMIPCSaxonCSandStudioTests'
    'TestCategory!=WarewolfCOMIPCClient_Deprecated'
    'TestCategory!=WebPostTool_Integration'
    'TestCategory!=WebGetTool_Integration'
) -join '&'

# --- Bash job scripts ---------------------------------------------------------
# In PS @"..."@ here-strings, bash variables are escaped with backtick: `$var, `$(cmd)
# PS variables ($UnitExclude, $UnitFilter, etc.) are interpolated by PowerShell.

$UnitBash = @"
#!/bin/bash
set -e
exclude='$UnitExclude'
filter='$UnitFilter'
i=0
for dll in /tests/*.dll; do
  [ -f "`$dll" ] || continue
  name=`$(basename "`$dll" .dll)
  echo "`$name" | grep -qE '^(Warewolf|Dev2)\..*(Tests|Specs)$' || continue
  echo "`$name" | grep -qE "`$exclude" && continue
  echo "[unit] `$name"
  timeout 300 dotnet-coverage collect \
    --output "/coverage/units_`${i}.cobertura.xml" \
    --output-format cobertura \
    --settings /settings/coverage-settings.xml \
    --nologo \
    -- /usr/share/dotnet/dotnet "/tests/`${name}.dll" \
       --filter "`$filter" \
       --results-directory /tmp/testresults \
       --no-progress 2>/dev/null || true
  i=`$((i+1))
done
xmls=`$(ls /coverage/units_*.cobertura.xml 2>/dev/null | tr '\n' ' ')
if [ -n "`$xmls" ]; then
  dotnet-coverage merge `$xmls \
    --output /coverage/unit_tests.cobertura.xml \
    --output-format cobertura --nologo
  echo "[unit] done: `$(du -k /coverage/unit_tests.cobertura.xml | cut -f1)KB"
else
  echo "[unit] WARNING: no coverage files produced"
fi
"@

$ActivitiesBash = @"
#!/bin/bash
set -e
filter='$ActivitiesFilter'
echo "[activities] Dev2.Activities.Tests"
dotnet-coverage collect \
  --output /coverage/activities.cobertura.xml \
  --output-format cobertura \
  --settings /settings/coverage-settings.xml \
  --nologo \
  -- /usr/share/dotnet/dotnet /tests/Dev2.Activities.Tests.dll \
     --filter "`$filter" \
     --results-directory /tmp/testresults \
     --no-progress 2>/dev/null || true
[ -f /coverage/activities.cobertura.xml ] && \
  echo "[activities] done: `$(du -k /coverage/activities.cobertura.xml | cut -f1)KB" || \
  echo "[activities] WARNING: no coverage file produced"
"@

$LightweightBash = @"
#!/bin/bash
set -e
echo "[lightweight] Warewolf.Execution.Lightweight.Tests"
dotnet-coverage collect \
  --output /coverage/lightweight.cobertura.xml \
  --output-format cobertura \
  --settings /settings/coverage-settings.xml \
  --nologo \
  -- /usr/share/dotnet/dotnet /tests/Warewolf.Execution.Lightweight.Tests.dll \
     --results-directory /tmp/testresults \
     --no-progress 2>/dev/null || true
[ -f /coverage/lightweight.cobertura.xml ] && \
  echo "[lightweight] done: `$(du -k /coverage/lightweight.cobertura.xml | cut -f1)KB" || \
  echo "[lightweight] WARNING: no coverage file produced"
"@

# Integration: engine + tests run in the SAME container so localhost:7071 resolves
# correctly without --network=host (which Docker Desktop for Windows doesn't support).
# Elasticsearch and exchange-connector run as sidecars on the same Docker network;
# they are accessible inside this container via their --network-alias names.
$IntegrationBash = @"
#!/bin/bash
set -e

export AZURE_KEYVAULT_NAME=''
export SkipFailureToRetrieveSecret='true'
export ENABLECONSOLELOGGING='false'
export ENABLEELASTICSEARCHLOGGING='false'
export FUNCTIONS_WORKER_RUNTIME='dotnet-isolated'

echo "[integration] Starting engine under dotnet-coverage (session: engine-coverage-session)..."
cd /server
dotnet-coverage collect \
  --output /coverage/engine.cobertura.xml \
  --output-format cobertura \
  --include-files /server/Warewolf.Execution.Lightweight.dll \
  --session-id engine-coverage-session \
  --nologo \
  -- func start --port 7071 &
COV_PID=`$!

echo "[integration] Waiting for engine on :7071..."
for i in `$(seq 1 60); do
  STATUS=`$(curl -s -o /dev/null -w '%{http_code}' --max-time 5 http://localhost:7071/ 2>/dev/null || echo 0)
  if [ "`$STATUS" -ge 100 ] 2>/dev/null && [ "`$STATUS" != '503' ]; then
    echo "[integration] Engine ready (HTTP `$STATUS)"
    break
  fi
  echo "[integration]   [`$i/60] HTTP `${STATUS:-none}..."
  sleep 2
  if [ "`$i" -eq 60 ]; then
    echo '[integration] ERROR: engine did not start within 120s'
    kill `$COV_PID 2>/dev/null || true
    exit 1
  fi
done

echo "[integration] Running Warewolf.Execution.Lightweight.Integration.Tests..."
/usr/share/dotnet/dotnet \
  /tests/Warewolf.Execution.Lightweight.Integration.Tests.dll \
  --results-directory /tmp/testresults \
  --no-progress || true

echo "[integration] Flushing coverage (shutdown session)..."
dotnet-coverage shutdown engine-coverage-session 2>/dev/null || true

echo "[integration] Waiting for /coverage/engine.cobertura.xml..."
for i in `$(seq 1 30); do
  [ -f /coverage/engine.cobertura.xml ] && break
  sleep 1
done

kill `$COV_PID 2>/dev/null || true
wait `$COV_PID 2>/dev/null || true

[ -f /coverage/engine.cobertura.xml ] && \
  echo "[integration] done: `$(du -k /coverage/engine.cobertura.xml | cut -f1)KB" || \
  echo '[integration] WARNING: engine coverage file not produced'
"@

# --- Docker run arg arrays (Unit / Activities / Lightweight) ------------------
$SettingsMount = "$(dp $SettingsFile):/settings/coverage-settings.xml:ro"

$UnitArgs = @(
    'run', '--rm',
    '--name', "ww-cov-unit-$RunId",
    '-e', 'DOTNET_ROOT=/usr/share/dotnet',
    '-v', "$(dp $ServerTestsBin):/tests:ro",
    '-v', "$(dp "$CoverageDir\unit"):/coverage",
    '-v', $SettingsMount,
    'warewolf-coverage-env', 'bash', '-c', $UnitBash
)

$ActArgs = @(
    'run', '--rm',
    '--name', "ww-cov-act-$RunId",
    '-e', 'DOTNET_ROOT=/usr/share/dotnet',
    '-v', "$(dp $ServerTestsBin):/tests:ro",
    '-v', "$(dp "$CoverageDir\activities"):/coverage",
    '-v', $SettingsMount,
    'warewolf-coverage-env', 'bash', '-c', $ActivitiesBash
)

$LwArgs = @(
    'run', '--rm',
    '--name', "ww-cov-lw-$RunId",
    '-e', 'DOTNET_ROOT=/usr/share/dotnet',
    '-v', "$(dp $ServerTestsBin):/tests:ro",
    '-v', "$(dp "$CoverageDir\lightweight"):/coverage",
    '-v', $SettingsMount,
    'warewolf-coverage-env', 'bash', '-c', $LightweightBash
)

# --- Integration job ----------------------------------------------------------
function Invoke-IntegrationJob {
    Write-Step "[Integration] Creating docker network: $NetName"
    docker network create $NetName | Out-Null

    $esName = "ww-cov-es-$RunId"
    Write-Step "[Integration] Starting Elasticsearch 8.17.4..."
    docker run -d `
        --name $esName `
        --network $NetName `
        --network-alias elasticsearch `
        -p 9200:9200 `
        -e 'discovery.type=single-node' `
        -e 'xpack.security.enabled=false' `
        -e 'ES_JAVA_OPTS=-Xms512m -Xmx512m' `
        docker.elastic.co/elasticsearch/elasticsearch:8.17.4 | Out-Null
    $ToCleanup.Add($esName)

    $exName = "ww-cov-exchange-$RunId"
    Write-Step "[Integration] Starting exchange-connector-testing..."
    docker run -d `
        --name $exName `
        --network $NetName `
        --network-alias exchange `
        -p 8889:8080 `
        warewolfserver/exchange-connector-testing 2>$null | Out-Null
    if ($LASTEXITCODE -eq 0) {
        $ToCleanup.Add($exName)
    } else {
        Write-Warn 'exchange-connector-testing unavailable - continuing without it'
    }

    Write-Step "[Integration] Running engine + tests container..."
    # Engine and integration tests share the same container so localhost:7071 resolves
    # correctly. Elasticsearch is reachable at http://elasticsearch:9200 via Docker
    # network alias (the engine may also need this if ENABLEELASTICSEARCHLOGGING is true).
    docker run --rm `
        --name "ww-cov-int-$RunId" `
        --network $NetName `
        -v "$(dp $NewServerBin):/server:ro" `
        -v "$(dp $ServerTestsBin):/tests:ro" `
        -v "$(dp "$CoverageDir\integration"):/coverage" `
        -v $SettingsMount `
        warewolf-coverage-env `
        bash -c $IntegrationBash

    if ($LASTEXITCODE -ne 0) {
        Write-Warn "[Integration] Container exited with code $LASTEXITCODE — coverage may be partial"
    }
}

# --- Cleanup (always runs via finally) ----------------------------------------
function Invoke-Cleanup {
    if ($null -ne $ToCleanup -and $ToCleanup.Count -gt 0) {
        Write-Step 'Cleaning up integration sidecar containers...'
        foreach ($c in $ToCleanup) {
            docker rm -f $c 2>$null | Out-Null
        }
    }
    $exists = docker network ls --filter "name=^${NetName}$" --format '{{.Name}}' 2>$null
    if ($exists) { docker network rm $NetName 2>$null | Out-Null }
}

# --- Run jobs -----------------------------------------------------------------
try {
    if ($NoParallel -or $Jobs.Count -le 1) {
        # Sequential mode
        if ('Unit'        -in $Jobs) { Write-Step '[Unit] Running...';        & docker @UnitArgs; if ($LASTEXITCODE -ne 0) { Write-Warn 'Unit container non-zero exit' } }
        if ('Activities'  -in $Jobs) { Write-Step '[Activities] Running...';  & docker @ActArgs;  if ($LASTEXITCODE -ne 0) { Write-Warn 'Activities container non-zero exit' } }
        if ('Lightweight' -in $Jobs) { Write-Step '[Lightweight] Running...'; & docker @LwArgs;   if ($LASTEXITCODE -ne 0) { Write-Warn 'Lightweight container non-zero exit' } }
        if ('Integration' -in $Jobs) { Invoke-IntegrationJob }
    } else {
        # Parallel mode: Unit / Activities / Lightweight launch as PS background jobs
        # (each is an independent docker run). Integration runs in the current thread
        # because it manages its own docker network and sidecar containers.
        $bgJobs = [System.Collections.Generic.List[System.Management.Automation.Job]]::new()

        if ('Unit' -in $Jobs) {
            Write-Step '[Unit] Starting (parallel)...'
            $bgJobs.Add((Start-Job -Name 'cov-unit' -ScriptBlock { param($a) & docker @a } -ArgumentList (, $UnitArgs)))
        }
        if ('Activities' -in $Jobs) {
            Write-Step '[Activities] Starting (parallel)...'
            $bgJobs.Add((Start-Job -Name 'cov-activities' -ScriptBlock { param($a) & docker @a } -ArgumentList (, $ActArgs)))
        }
        if ('Lightweight' -in $Jobs) {
            Write-Step '[Lightweight] Starting (parallel)...'
            $bgJobs.Add((Start-Job -Name 'cov-lightweight' -ScriptBlock { param($a) & docker @a } -ArgumentList (, $LwArgs)))
        }

        if ('Integration' -in $Jobs) {
            Invoke-IntegrationJob
        }

        if ($bgJobs.Count -gt 0) {
            Write-Step 'Waiting for parallel jobs to finish...'
            $bgJobs | Wait-Job | Out-Null
            foreach ($j in $bgJobs) {
                Write-Host "`n--- $($j.Name) output ---" -ForegroundColor DarkGray
                Receive-Job $j
                if ($j.State -eq 'Failed') { Write-Warn "Job $($j.Name) reported failure" }
            }
            $bgJobs | Remove-Job
        }
    }

    # --- Merge ----------------------------------------------------------------
    Write-Step 'Merging coverage files...'
    $xmlsToMerge = @(
        "$CoverageDir\unit\unit_tests.cobertura.xml"
        "$CoverageDir\activities\activities.cobertura.xml"
        "$CoverageDir\lightweight\lightweight.cobertura.xml"
        "$CoverageDir\integration\engine.cobertura.xml"
    ) | Where-Object { (Test-Path $_) -and (Get-Item $_).Length -gt 200 }

    if ($xmlsToMerge.Count -eq 0) {
        Write-Warn 'No coverage files found — skipping merge and report'
    } else {
        $fileNames = ($xmlsToMerge | ForEach-Object { [IO.Path]::GetFileName($_) }) -join ', '
        Write-Done "Merging $($xmlsToMerge.Count) file(s): $fileNames"

        $mergedXml = "$CoverageDir\merged\all_merged.cobertura.xml"
        dotnet-coverage merge @xmlsToMerge `
            --output $mergedXml `
            --output-format cobertura `
            --nologo
        Test-Exit 'dotnet-coverage merge'
        Write-Done "Merged: $([Math]::Round((Get-Item $mergedXml).Length / 1KB))KB -> $mergedXml"

        # --- Filter -----------------------------------------------------------
        Write-Step 'Filtering third-party packages from coverage XML...'
        $py = Get-Command python3 -ErrorAction SilentlyContinue
        if (-not $py) { $py = Get-Command python -ErrorAction SilentlyContinue }
        if ($py) {
            & $py $FilterScript "$CoverageDir\merged"
            Test-Exit 'filter_coverage.py'
            Write-Done 'Coverage filtered'
        } else {
            Write-Warn 'Python not found — coverage filter skipped'
        }

        # --- Report -----------------------------------------------------------
        if (-not $SkipReport) {
            Write-Step "Generating $ReportFormat report..."
            $reportDir = "$CoverageDir\report"
            reportgenerator `
                "-reports:$mergedXml" `
                "-targetdir:$reportDir" `
                "-reporttypes:$ReportFormat" `
                '-title:Warewolf Coverage' `
                '-verbosity:Warning'
            Test-Exit 'reportgenerator'
            $index = "$reportDir\index.html"
            Write-Done "Report: $index"
            if (Test-Path $index) { Start-Process $index }
        }
    }

    Write-Host "`n[DONE] Coverage build complete.`n" -ForegroundColor Green

} finally {
    Invoke-Cleanup
}
