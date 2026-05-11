# Run-Coverage.ps1
# Mirrors every test job in .azure/pipeline.yml locally in Linux Docker containers,
# collects per-job coverage, merges, filters, and produces an HTML report.
#
# Job catalog (assembly, filter, sidecars, output XML) is parsed from pipeline.yml
# at startup, so any new job added in CI is picked up automatically.
#
# Usage:
#   .\Run-Coverage.ps1
#   .\Run-Coverage.ps1 -Jobs Unit_Tests,Activities_Tests -SkipBuild
#   .\Run-Coverage.ps1 -List
#   .\Run-Coverage.ps1 -Jobs Zip_Tool_Specs,Copy_Tool_Specs_From_FTP -SkipBuild

[CmdletBinding()]
param(
    # Job names from pipeline.yml. Use 'All' (default) for everything.
    [string[]]$Jobs = @('All'),

    [string]$OutputDir = 'coverage',

    [switch]$SkipBuild,
    [switch]$SkipReport,

    [ValidateSet('Html','Badges','Cobertura','TextSummary','HtmlSummary','MarkdownSummary')]
    [string]$ReportFormat = 'Html',

    [switch]$NoParallel,
    [int]$MaxParallel = 4,

    # Print the parsed pipeline.yml job catalog and exit.
    [switch]$List
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

# --- Paths --------------------------------------------------------------------
$Root           = Split-Path $PSScriptRoot -Parent
$BinRoot        = Join-Path $Root 'Bin'
$ServerTestsBin = Join-Path $BinRoot 'ServerTests'
$PipelineYml    = Join-Path $PSScriptRoot '.azure' 'pipeline.yml'
$CoverageDir    = [System.IO.Path]::GetFullPath((Join-Path $Root $OutputDir))
$SettingsFile   = Join-Path $Root 'coverage-settings.xml'
$FilterScript   = Join-Path $PSScriptRoot '.azure' 'filter_coverage.py'
$DockerfileTest = Join-Path $PSScriptRoot 'Warewolf.Execution.Lightweight' 'engine' 'docker' 'Dockerfile.test'
$DockerContext  = Split-Path $DockerfileTest -Parent

$RunId   = Get-Date -Format 'yyyyMMddHHmmss'

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

# Docker Desktop on Windows accepts C:/foo/bar in -v mounts
function dp([string]$path) {
    [System.IO.Path]::GetFullPath($path) -replace '\\', '/'
}

# --- Slug helper: pipeline job name -> filesystem-friendly slug ---------------
function Get-Slug([string]$JobName) {
    return ($JobName -replace '_', '-').ToLower()
}

# --- Pipeline.yml parser ------------------------------------------------------
# Extracts a job catalog by regex-walking pipeline.yml. Returns an array of
# PSCustomObjects with: Name, Type (Unit|EngineSpec), Assembly, Filter, Output,
# SessionId, Sidecars[], Artifact.
function ConvertFrom-PipelineYaml {
    param([Parameter(Mandatory)][string]$YamlPath)

    $text = [System.IO.File]::ReadAllText($YamlPath)
    $skip = @('build', 'Install_Func_CLI', 'MergeCoverage', 'build_release')

    # Walk job blocks: "  - job: NAME\n ... up to next '  - job: ' or EOF"
    $jobRegex = [regex]'(?ms)^  - job: (?<name>\S+)\s*$.*?(?=^  - job: |\Z)'
    $catalog  = New-Object System.Collections.Generic.List[object]

    foreach ($m in $jobRegex.Matches($text)) {
        $name = $m.Groups['name'].Value
        if ($skip -contains $name) { continue }
        $body = $m.Value

        $entry = [PSCustomObject]@{
            Name              = $name
            Slug              = Get-Slug $name
            # An engine-spec job is recognised by EITHER:
            #   - new style: `bash …/start-engine-coverage.sh <sid> "<cov>" …`
            #   - old style: `dotnet-coverage collect ... --session-id <sid> -- func start`
            # so this parser keeps working whether the pipeline.yml refactor is
            # checked in or not.
            Type              = if ($body -match 'start-engine-coverage\.sh' -or
                                    $body -match '--session-id\s+\S+')
                                { 'EngineSpec' } else { 'Unit' }
            Assembly          = $null    # first assembly (back-compat)
            Assemblies        = @()      # full list when -Assemblies "A","B","C"
            ExcludeAssemblies = @()
            Filter            = $null
            Output            = $null      # engine cobertura filename
            SessionId         = $null
            Sidecars          = @()
            Artifact          = $null
        }

        # SessionId
        #   new style: first positional arg to start-engine-coverage.sh
        #   old style: --session-id <name>
        if ($body -match '(?ms)start-engine-coverage\.sh\s*\\\s*\r?\n\s*(\S+)\s*\\') {
            $entry.SessionId = $matches[1]
        } elseif ($body -match '--session-id\s+(\S+)') {
            $entry.SessionId = $matches[1]
        }
        # Output filename
        #   new style: 2nd positional arg "$(Agent.BuildDirectory)/coverage/<X>.cobertura.xml"
        #   old style: --output "$(Agent.BuildDirectory)/coverage/<X>.cobertura.xml"
        if ($body -match '"\$\(Agent\.BuildDirectory\)/coverage/([^"]+\.cobertura\.xml)"') {
            $entry.Output = $matches[1]
        }
        # -Assemblies may carry a comma-separated list:
        #   -Assemblies "A.dll","B.dll","C.dll"
        # Capture the whole list, then split on every quoted segment so jobs
        # that bundle multiple test assemblies (e.g. Other_Specs) run each one.
        if ($body -match '-Assemblies\s+((?:"[^"]+"(?:\s*,\s*)?)+)') {
            $listText = $matches[1]
            # Force array context with @(...) — otherwise a single-match list
            # collapses to a scalar string and $assemblies[0] indexes the first CHAR.
            $assemblies = @([regex]::Matches($listText, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
            $entry.Assembly   = $assemblies[0]   # first (legacy single-DLL consumers)
            $entry.Assemblies = $assemblies      # full list (new multi-DLL consumer path)
        }
        if ($body -match '-ExcludeAssemblies\s+((?:"[^"]+"(?:\s*,\s*)?)+)') {
            # Capture group is the entire list eg: "A","B", "C"
            $listText = $matches[1]
            $entry.ExcludeAssemblies = [regex]::Matches($listText, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value }
        }

        # -Filter accepts either quote style; the value may CONTAIN the other quote style:
        #   -Filter 'TestCategory=Foo'
        #   -Filter 'TestCategory="Server Startup"'   <-- inner double-quote
        #   -Filter "TestCategory!=A&TestCategory!=B"
        # Capture the value verbatim (including any TestCategory= prefix) and let the
        # consumer pass it through unchanged.
        if ($body -match "-Filter\s+'([^']+)'") {
            $entry.Filter = $matches[1]
        } elseif ($body -match '-Filter\s+"([^"]+)"') {
            $entry.Filter = $matches[1]
        }

        if ($body -match "artifactName:\s+'([^']+)'") {
            $entry.Artifact = $matches[1]
        }

        # Sidecar detection by docker image name
        $sidecars = @()
        if ($body -match 'stilliard/pure-ftpd')                        { $sidecars += 'ftp' }
        if ($body -match 'atmoz/sftp')                                  { $sidecars += 'sftp' }
        if ($body -match 'dperson/samba')                               { $sidecars += 'samba' }
        if ($body -match 'mssql/server:2019-latest')                    { $sidecars += 'sqlserver' }
        if ($body -match 'rabbitmq:3-management')                       { $sidecars += 'rabbitmq' }
        if ($body -match 'redis:7-alpine')                              { $sidecars += 'redis' }
        if ($body -match 'docker\.elastic\.co/elasticsearch')           { $sidecars += 'elasticsearch' }
        if ($body -match 'warewolfserver/exchange-connector-testing')   { $sidecars += 'exchange' }
        $entry.Sidecars = $sidecars

        $catalog.Add($entry) | Out-Null
    }
    return ,$catalog.ToArray()
}

# --- Sidecar dispatcher -------------------------------------------------------
# All sidecars share the test container's network namespace via --network=container:X,
# so tests inside the test container can reach them at localhost:<port>.
function Get-SidecarName($Type, $Slug, $RunId) {
    return "ww-cov-$Type-$Slug-$RunId"
}

function Start-Sidecar {
    param(
        [Parameter(Mandatory)][string]$Type,
        [Parameter(Mandatory)][string]$Slug,
        [Parameter(Mandatory)][string]$RunId,
        [Parameter(Mandatory)][string]$TestContainer
    )
    $name = Get-SidecarName $Type $Slug $RunId

    switch ($Type) {
        'ftp' {
            docker run -d --name $name --network="container:$TestContainer" `
                -e FTP_USER_NAME=ftpuser -e FTP_USER_PASS=ftppass `
                -e FTP_USER_HOME=/home/ftpusers/ftpuser `
                stilliard/pure-ftpd | Out-Null
        }
        'sftp' {
            docker run -d --name $name --network="container:$TestContainer" `
                atmoz/sftp ftpuser:ftppass:1001 | Out-Null
        }
        'samba' {
            docker run -d --name $name --network="container:$TestContainer" `
                -e USER='smbuser%smbpass' `
                -e SHARE='share;/share;yes;no;no;smbuser' `
                dperson/samba -u 'smbuser;smbpass' -s 'share;/share;yes;no;no;smbuser' | Out-Null
        }
        'sqlserver' {
            docker run -d --name $name --network="container:$TestContainer" `
                -e ACCEPT_EULA=Y -e SA_PASSWORD='Test123456!' -e MSSQL_PID=Developer `
                mcr.microsoft.com/mssql/server:2019-latest | Out-Null
        }
        'rabbitmq' {
            docker run -d --name $name --network="container:$TestContainer" `
                rabbitmq:3-management | Out-Null
        }
        'redis' {
            docker run -d --name $name --network="container:$TestContainer" `
                redis:7-alpine | Out-Null
        }
        'elasticsearch' {
            docker run -d --name $name --network="container:$TestContainer" `
                -e 'discovery.type=single-node' `
                -e 'xpack.security.enabled=false' `
                -e 'ES_JAVA_OPTS=-Xms512m -Xmx512m' `
                docker.elastic.co/elasticsearch/elasticsearch:8.17.4 | Out-Null
        }
        'exchange' {
            docker run -d --name $name --network="container:$TestContainer" `
                warewolfserver/exchange-connector-testing 2>$null | Out-Null
        }
        default {
            Write-Warn "Unknown sidecar type '$Type' (skipping)"
            return
        }
    }
    if ($LASTEXITCODE -ne 0) {
        Write-Warn "Failed to start sidecar $Type ($name) -- continuing"
    }
}

# --- Banner -------------------------------------------------------------------
Write-Host ''
Write-Host '  Warewolf Coverage Build  (local)' -ForegroundColor White
Write-Host "  Pipeline : $PipelineYml" -ForegroundColor Gray
Write-Host "  Jobs     : $($Jobs -join ', ')" -ForegroundColor Gray
Write-Host "  Output   : $CoverageDir" -ForegroundColor Gray
Write-Host "  Parallel : $(-not $NoParallel.IsPresent) (max $MaxParallel)" -ForegroundColor Gray
Write-Host ''

# --- Build catalog ------------------------------------------------------------
if (-not (Test-Path $PipelineYml)) {
    Write-Error "pipeline.yml not found: $PipelineYml"
    exit 1
}
$JobCatalog = ConvertFrom-PipelineYaml -YamlPath $PipelineYml
Write-Host "Parsed $($JobCatalog.Count) jobs from pipeline.yml" -ForegroundColor Gray

if ($List) {
    $JobCatalog | Sort-Object Name | Format-Table -AutoSize Name, Type, Assembly, Filter, @{n='Sidecars';e={$_.Sidecars -join ','}}
    exit 0
}

# --- Resolve which jobs to run ------------------------------------------------
$jobNames = $JobCatalog | ForEach-Object { $_.Name }
$selected = @()
if ($Jobs -contains 'All' -or $Jobs.Count -eq 0) {
    $selected = $JobCatalog
} else {
    foreach ($req in $Jobs) {
        $match = $JobCatalog | Where-Object { $_.Name -eq $req }
        if (-not $match) {
            Write-Warn "Unknown job '$req' - skipping. Use -List to see available jobs."
            continue
        }
        $selected += $match
    }
}
if ($selected.Count -eq 0) {
    Write-Error "No jobs selected. Use -List to see available jobs."
    exit 1
}
Write-Host "Will run $($selected.Count) job(s): $(( $selected | ForEach-Object {$_.Name} ) -join ', ')" -ForegroundColor Gray

# --- Prerequisites ------------------------------------------------------------
function Ensure-GlobalTool([string]$packageId) {
    if (-not ((dotnet tool list --global 2>$null) -match [regex]::Escape($packageId))) {
        Write-Step "Auto-installing $packageId..."
        dotnet tool install --global $packageId
        Test-Exit "dotnet tool install $packageId"
    }
}

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
    Write-Done 'Build outputs -> Bin\ServerTests'
} else {
    Write-Step 'Skipping build (-SkipBuild)'
    if (-not (Test-Path $ServerTestsBin)) {
        Write-Error "Build output missing: $ServerTestsBin`nRun without -SkipBuild first."
        exit 1
    }
}

# --- Output directories (one per job + merged) --------------------------------
New-Item -ItemType Directory -Force -Path $CoverageDir | Out-Null
New-Item -ItemType Directory -Force -Path "$CoverageDir\merged" | Out-Null
foreach ($j in $selected) {
    New-Item -ItemType Directory -Force -Path "$CoverageDir\$($j.Slug)" | Out-Null
}

# --- Optional secure config + local.settings.json injection -------------------
# Lightweight tests use WAREWOLF_SECURE_CONFIG for F_RealConfig tests; if a
# pipeline secret is provided, mount it. Otherwise the engine container falls
# back to WAREWOLF_GENERATE_CI_CONFIG=1.
$SecureConfigContent = $env:WAREWOLF_SECURE_CONFIG_CONTENT
$LwSecureConfigFile  = $null
$LwSecureConfigMount = $null
$LwSecureConfigEnv   = $null

if (-not [string]::IsNullOrWhiteSpace($SecureConfigContent)) {
    Write-Step 'Writing secure.config from pipeline secret...'
    $LwSecureConfigFile = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $LwSecureConfigFile -Value $SecureConfigContent -Encoding UTF8 -NoNewline
    $LwSecureConfigMount = "$(dp $LwSecureConfigFile):/tmp/secure.config:ro"
    $LwSecureConfigEnv   = 'WAREWOLF_TEST_SECURE_CONFIG=/tmp/secure.config'
    Write-Done 'Secure config mounted at /tmp/secure.config'
} else {
    $LwSecureConfigEnv = 'WAREWOLF_GENERATE_CI_CONFIG=1'
}

# Key Vault config for security specs that decrypt .bite files.
$LocalSettingsContent = $env:WAREWOLF_LOCAL_SETTINGS_JSON
$LocalSettingsFile    = $null
$LocalSettingsMount   = $null
$DownloadsSettings = Join-Path $env:USERPROFILE 'Downloads\local.settings.json'
if (-not [string]::IsNullOrWhiteSpace($LocalSettingsContent)) {
    Write-Step 'Writing local.settings.json from pipeline secret...'
    $LocalSettingsFile  = [System.IO.Path]::GetTempFileName()
    Set-Content -Path $LocalSettingsFile -Value $LocalSettingsContent -Encoding UTF8 -NoNewline
    $LocalSettingsMount = "$(dp $LocalSettingsFile):/server/local.settings.json:ro"
} elseif (Test-Path $DownloadsSettings) {
    $LocalSettingsMount = "$(dp $DownloadsSettings):/server/local.settings.json:ro"
}

# --- Docker images ------------------------------------------------------------
Write-Step 'Building warewolf-test-env...'
$dtfContent = Get-Content $DockerfileTest -Raw
if ($dtfContent -match 'ENTRYPOINT \["/bin/bash"\]') {
    ($dtfContent -replace 'ENTRYPOINT \["/bin/bash"\]', 'ENTRYPOINT []') |
        Set-Content $DockerfileTest -Encoding UTF8
}
docker build -q -t warewolf-test-env -f $DockerfileTest $DockerContext
Test-Exit 'Build warewolf-test-env'
Write-Done 'warewolf-test-env ready'

Write-Step 'Building warewolf-coverage-env (adds dotnet-coverage + Azure Functions CLI)...'
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

# --- Convert pipeline.yml -ExcludeAssemblies globs to a bash ERE -------------
# eg @('Dev2.Integration.Tests','*.Specs') -> '^(Dev2\.Integration\.Tests|.*\.Specs)$'
function ConvertTo-ExcludeRegex([string[]]$Excludes) {
    if (-not $Excludes -or $Excludes.Count -eq 0) { return '' }
    $parts = foreach ($e in $Excludes) {
        ([regex]::Escape($e)) -replace '\\\*', '.*'
    }
    return '^(' + ($parts -join '|') + ')$'
}

# --- Unit-style job runner ----------------------------------------------------
# For jobs that don't start an engine. Two shapes:
#   1. Single DLL: $Job.Assembly is set       -> run that DLL under dotnet-coverage
#   2. Multi DLL : $Job.ExcludeAssemblies set -> loop /tests/*.dll, skip excluded
function Invoke-UnitJob {
    param([Parameter(Mandatory)]$Job)

    $slug = $Job.Slug
    $covDir = "$CoverageDir\$slug"

    if (-not $Job.Assembly -and $Job.ExcludeAssemblies.Count -eq 0) {
        Write-Warn "[$($Job.Name)] no Assembly or ExcludeAssemblies parsed - skipping"
        return
    }

    if ($Job.Assembly) {
        # ---- Single DLL ------------------------------------------------------
        $filterArg = ''
        if (-not [string]::IsNullOrWhiteSpace($Job.Filter)) {
            if ($Job.Filter -match '^[A-Za-z0-9_]+$') {
                $filterArg = "--filter `"TestCategory=$($Job.Filter)`""
            } else {
                $filterArg = "--filter `"$($Job.Filter)`""
            }
        }
        $dll = "$($Job.Assembly).dll"
        $bash = @"
#!/bin/bash
set -e
echo "[$slug] $($Job.Assembly)"
dotnet-coverage collect \
  --output /coverage/$slug.unit.cobertura.xml \
  --output-format cobertura \
  --settings /settings/coverage-settings.xml \
  --nologo \
  -- /usr/share/dotnet/dotnet /tests/$dll \
     $filterArg \
     --results-directory /tmp/testresults \
     --no-progress 2>/dev/null || true
[ -f /coverage/$slug.unit.cobertura.xml ] && \
  echo "[$slug] done: `$(du -k /coverage/$slug.unit.cobertura.xml | cut -f1)KB" || \
  echo '[$slug] WARNING: no coverage file produced'
"@
    } else {
        # ---- Multi DLL loop --------------------------------------------------
        $excludeRegex = ConvertTo-ExcludeRegex $Job.ExcludeAssemblies
        $unitFilter = $Job.Filter
        if (-not $unitFilter) { $unitFilter = '' }
        $bash = @"
#!/bin/bash
set -e
exclude='$excludeRegex'
filter='$unitFilter'
i=0
for dll in /tests/*.dll; do
  [ -f "`$dll" ] || continue
  name=`$(basename "`$dll" .dll)
  echo "`$name" | grep -qE '^(Warewolf|Dev2)\..*(Tests|Specs)$' || continue
  if [ -n "`$exclude" ]; then echo "`$name" | grep -qE "`$exclude" && continue; fi
  echo "[$slug] `$name"
  timeout 300 dotnet-coverage collect \
    --output "/coverage/parts_`${i}.cobertura.xml" \
    --output-format cobertura \
    --settings /settings/coverage-settings.xml \
    --nologo \
    -- /usr/share/dotnet/dotnet "/tests/`${name}.dll" \
       `$([ -n "`$filter" ] && echo "--filter `\"`$filter`\"") \
       --results-directory /tmp/testresults \
       --no-progress 2>/dev/null || true
  i=`$((i+1))
done
xmls=`$(ls /coverage/parts_*.cobertura.xml 2>/dev/null | tr '\n' ' ')
if [ -n "`$xmls" ]; then
  dotnet-coverage merge `$xmls --output /coverage/$slug.unit.cobertura.xml \
    --output-format cobertura --nologo
  echo "[$slug] done: `$(du -k /coverage/$slug.unit.cobertura.xml | cut -f1)KB"
else
  echo "[$slug] WARNING: no coverage files produced"
fi
"@
    }

    $args = @(
        'run', '--rm',
        '--name', "ww-cov-unit-$slug-$RunId",
        '-e', 'DOTNET_ROOT=/usr/share/dotnet'
    )
    if ($Job.Name -eq 'LightweightExecutionUnitTests' -and $LwSecureConfigEnv)   { $args += '-e', $LwSecureConfigEnv }
    if ($Job.Name -eq 'LightweightExecutionUnitTests' -and $LwSecureConfigMount) { $args += '-v', $LwSecureConfigMount }
    $args += @(
        '-v', "$(dp $ServerTestsBin):/tests:ro",
        '-v', "$(dp $covDir):/coverage",
        '-v', "$(dp $SettingsFile):/settings/coverage-settings.xml:ro",
        'warewolf-coverage-env', 'bash', '-c', $bash
    )

    & docker @args
    if ($LASTEXITCODE -ne 0) { Write-Warn "[$($Job.Name)] non-zero exit ($LASTEXITCODE)" }
}

# --- Engine-spec job runner ---------------------------------------------------
# Pattern: per-job docker network + a long-lived test container; sidecars share
# the test container's network namespace (--network=container:X). Engine and
# tests both run inside the test container so localhost:7071 resolves correctly.
function Invoke-EngineSpecJob {
    param([Parameter(Mandatory)]$Job)

    $slug          = $Job.Slug
    $netName       = "ww-cov-$slug-$RunId"
    $testContainer = "ww-cov-test-$slug-$RunId"
    $covDir        = "$CoverageDir\$slug"

    if (-not $Job.Output) {
        Write-Warn "[$($Job.Name)] no Output filename parsed - skipping"
        return
    }
    if (-not $Job.SessionId) {
        Write-Warn "[$($Job.Name)] no SessionId parsed - skipping"
        return
    }
    if (-not $Job.Assembly -and $Job.ExcludeAssemblies.Count -eq 0) {
        Write-Warn "[$($Job.Name)] no Assembly or ExcludeAssemblies parsed - skipping"
        return
    }

    Write-Step "[$($Job.Name)] Creating docker network: $netName"
    docker network create $netName 2>$null | Out-Null

    try {
        # Mounts shared by engine and tests
        $envArgs = @('-e', 'DOTNET_ROOT=/usr/share/dotnet')
        $mountArgs = @(
            '-v', "$(dp $ServerTestsBin):/server:ro",
            '-v', "$(dp $ServerTestsBin):/tests:ro",
            '-v', "$(dp $covDir):/coverage",
            '-v', "$(dp $SettingsFile):/settings/coverage-settings.xml:ro"
        )
        if ($Job.Name -match 'Security') {
            # Security specs: shared writable config dir between engine and tests
            New-Item -ItemType Directory -Force -Path "$covDir\security-config" | Out-Null
            $mountArgs += '-v', "$(dp "$covDir\security-config"):/security-config"
        }
        if ($LocalSettingsMount) { $mountArgs += '-v', $LocalSettingsMount }

        Write-Step "[$($Job.Name)] Starting test container..."
        & docker run -d --name $testContainer --network $netName @envArgs @mountArgs `
            warewolf-coverage-env sleep 3600 | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Failed to start test container' }

        # Start sidecars (each shares testContainer's net namespace)
        foreach ($s in $Job.Sidecars) {
            Write-Step "[$($Job.Name)] Starting sidecar: $s"
            Start-Sidecar -Type $s -Slug $slug -RunId $RunId -TestContainer $testContainer
        }
        if ($Job.Sidecars.Count -gt 0) {
            Write-Step "[$($Job.Name)] Waiting 8s for sidecars to settle..."
            Start-Sleep 8
        }

        # Build the bash run inside the test container.
        # Filter is passed verbatim (parser captures the full value, including any
        # `TestCategory=` prefix). Bash single-quotes preserve inner double quotes
        # like `TestCategory="Server Startup"`.
        $filterStr = if ($Job.Filter) { "--filter '$($Job.Filter)'" } else { '' }
        $securitySetup = ''
        if ($Job.Name -match 'Security') {
            $securitySetup = @"
export WAREWOLF_SECURE_CONFIG=/security-config/secure.config
touch /security-config/secure.config
chmod 666 /security-config/secure.config
"@
        }

        # Shared bash helper: pick the right test invocation for a given assembly.
        # MTP (Microsoft Testing Platform — EnableMSTestRunner=true) projects expose
        # vstest-equivalent flags directly on the DLL when launched via `dotnet <dll>`.
        # vstest-only projects need `dotnet test <dll>`.
        # Both paths use `dotnet <dll>` (NOT the self-contained apphost), matching
        # run-tests-in-container.ps1 — the apphost probes /tests/ for libhostfxr.so
        # before honouring DOTNET_ROOT and trips over sibling self-contained binaries.
        $runOneTest = @"
run_test_dll() {
  local name="`$1"
  local deps="/tests/`${name}.deps.json"
  if [ -f "`$deps" ] && grep -q '"Microsoft.Testing.Platform"' "`$deps"; then
    echo "[$slug]   MTP: `$name"
    /usr/share/dotnet/dotnet "/tests/`${name}.dll" $filterStr --results-directory /tmp/testresults --no-progress 2>/dev/null || true
  else
    echo "[$slug]   vstest: `$name"
    /usr/share/dotnet/dotnet test "/tests/`${name}.dll" $filterStr --results-directory /tmp/testresults --no-progress 2>/dev/null || true
  fi
}
"@

        # Test invocation: single DLL, multi-DLL list (from -Assemblies "A","B","C"), or
        # discover-and-loop (from -ExcludeAssemblies).
        $assemblyList = if ($Job.Assemblies -and $Job.Assemblies.Count -gt 1) { $Job.Assemblies } elseif ($Job.Assembly) { @($Job.Assembly) } else { @() }
        if ($assemblyList.Count -gt 0) {
            $loopBody = ($assemblyList | ForEach-Object { "run_test_dll '$_'" }) -join "`n"
            $testInvocation = @"
$runOneTest
echo "[$slug] Running $($assemblyList.Count) assembly(ies): $($assemblyList -join ', ')"
$loopBody
"@
        } else {
            $excludeRegex = ConvertTo-ExcludeRegex $Job.ExcludeAssemblies
            $testInvocation = @"
$runOneTest
echo "[$slug] Multi-DLL discover-and-loop (excluding: $excludeRegex)"
exclude='$excludeRegex'
for dll in /tests/*.dll; do
  [ -f "`$dll" ] || continue
  name=`$(basename "`$dll" .dll)
  echo "`$name" | grep -qE '^(Warewolf|Dev2)\..*(Tests|Specs)$' || continue
  if [ -n "`$exclude" ]; then echo "`$name" | grep -qE "`$exclude" && continue; fi
  run_test_dll "`$name"
done
"@
        }

        # Static instrumentation + server-mode collector: the previous
        # `dotnet-coverage collect -- func start` pattern only profiled the
        # parent func host. The engine runs in a separate dotnet-isolated
        # worker process whose CORECLR_* env vars are not inherited, so the
        # engine assembly was never instrumented (every spec-job XML showed
        # line-rate=0 for Warewolf.Execution.Lightweight.*). Pre-instrumenting
        # the DLL embeds the trace points directly so any process loading it
        # reports to the named collector session.
        $bash = @"
#!/bin/bash
set -e
export AZURE_KEYVAULT_NAME=''
export SkipFailureToRetrieveSecret='true'
export ENABLECONSOLELOGGING='false'
export ENABLEELASTICSEARCHLOGGING='false'
export FUNCTIONS_WORKER_RUNTIME='dotnet-isolated'
$securitySetup

echo "[$slug] Preparing writable engine copy at /server-rw..."
mkdir -p /server-rw
cp -a /server/. /server-rw/

echo "[$slug] Instrumenting engine DLL (session: $($Job.SessionId))..."
dotnet-coverage instrument /server-rw/Warewolf.Execution.Lightweight.dll \
  --session-id $($Job.SessionId) \
  --nologo

echo "[$slug] Starting collector in server-mode (background)..."
dotnet-coverage collect \
  --session-id $($Job.SessionId) \
  --server-mode \
  --background \
  --output /coverage/$($Job.Output) \
  --output-format cobertura \
  --nologo

cd /server-rw
echo "[$slug] Starting engine..."
nohup func start --port 7071 > /tmp/func-engine.log 2>&1 &
ENGINE_PID=`$!

for i in `$(seq 1 60); do
  STATUS=`$(curl -s -o /dev/null -w '%{http_code}' --max-time 5 http://localhost:7071/ 2>/dev/null || echo 0)
  if [ "`$STATUS" -ge 100 ] 2>/dev/null && [ "`$STATUS" != '503' ]; then
    echo "[$slug] Engine ready (HTTP `$STATUS)"; break
  fi
  echo "[$slug]   [`$i/60] HTTP `${STATUS:-none}..."
  sleep 2
  if [ "`$i" -eq 60 ]; then
    echo '[$slug] ERROR: engine did not start within 120s'
    tail -n 40 /tmp/func-engine.log 2>/dev/null || true
    kill `$ENGINE_PID 2>/dev/null || true
    dotnet-coverage shutdown $($Job.SessionId) 2>/dev/null || true
    exit 1
  fi
done

$testInvocation

echo "[$slug] Shutting down coverage session..."
dotnet-coverage shutdown $($Job.SessionId) 2>/dev/null || true

kill `$ENGINE_PID 2>/dev/null || true
pkill -TERM -f 'func start' 2>/dev/null || true
pkill -TERM -f 'Warewolf.Execution.Lightweight' 2>/dev/null || true

for i in `$(seq 1 30); do
  [ -f /coverage/$($Job.Output) ] && break
  sleep 1
done

[ -f /coverage/$($Job.Output) ] && \
  echo "[$slug] done: `$(du -k /coverage/$($Job.Output) | cut -f1)KB" || \
  echo '[$slug] WARNING: engine coverage file not produced'
"@

        Write-Step "[$($Job.Name)] Running engine + tests..."
        & docker exec $testContainer bash -c $bash
        if ($LASTEXITCODE -ne 0) { Write-Warn "[$($Job.Name)] non-zero exit ($LASTEXITCODE)" }
    } finally {
        # Stop sidecars first, then test container, then network
        foreach ($s in $Job.Sidecars) {
            $sn = Get-SidecarName $s $slug $RunId
            docker rm -f $sn 2>$null | Out-Null
        }
        docker rm -f $testContainer 2>$null | Out-Null
        docker network rm $netName 2>$null | Out-Null
    }
}

# --- Job dispatch -------------------------------------------------------------
function Invoke-Job {
    param([Parameter(Mandatory)]$Job)
    if ($Job.Type -eq 'EngineSpec') {
        Invoke-EngineSpecJob $Job
    } else {
        Invoke-UnitJob $Job
    }
}

# --- Run jobs (sequential or parallel) ----------------------------------------
try {
    if ($NoParallel -or $selected.Count -le 1) {
        foreach ($j in $selected) {
            Write-Step "[$($j.Name)] running ($($j.Type))..."
            Invoke-Job $j
        }
    } else {
        # Parallel via PowerShell jobs. Concurrency capped by $MaxParallel.
        # Jobs with EngineSpec + sidecars each get isolated docker network, so
        # safe to run concurrently; only docker daemon load is a concern.
        $bgJobs  = New-Object System.Collections.Generic.List[System.Management.Automation.Job]
        $queue   = [System.Collections.Queue]::new()
        foreach ($j in $selected) { $queue.Enqueue($j) | Out-Null }

        # Function definitions need to be exported into each background job
        $exportFns = @(
            'Write-Step','Write-Done','Write-Warn','Test-Exit','dp','Get-Slug',
            'Get-SidecarName','Start-Sidecar','ConvertTo-ExcludeRegex',
            'Invoke-UnitJob','Invoke-EngineSpecJob','Invoke-Job'
        ) | ForEach-Object { Get-Command $_ -CommandType Function } | ForEach-Object {
            "function $($_.Name) { $($_.ScriptBlock) }"
        }
        $exportScript = $exportFns -join "`n"

        while ($queue.Count -gt 0 -or $bgJobs.Count -gt 0) {
            while ($bgJobs.Count -lt $MaxParallel -and $queue.Count -gt 0) {
                $j = $queue.Dequeue()
                Write-Step "[$($j.Name)] starting (parallel slot $($bgJobs.Count + 1)/$MaxParallel)"
                $bg = Start-Job -Name "cov-$($j.Slug)" -ScriptBlock {
                    param($exportScript, $job, $ServerTestsBin, $CoverageDir, $SettingsFile, $RunId,
                          $LwSecureConfigEnv, $LwSecureConfigMount, $LocalSettingsMount)
                    Invoke-Expression $exportScript
                    # rehydrate script-scope state used by Invoke-* runners
                    Set-Variable -Name ServerTestsBin      -Value $ServerTestsBin      -Scope Script
                    Set-Variable -Name CoverageDir         -Value $CoverageDir         -Scope Script
                    Set-Variable -Name SettingsFile        -Value $SettingsFile        -Scope Script
                    Set-Variable -Name RunId               -Value $RunId               -Scope Script
                    Set-Variable -Name LwSecureConfigEnv   -Value $LwSecureConfigEnv   -Scope Script
                    Set-Variable -Name LwSecureConfigMount -Value $LwSecureConfigMount -Scope Script
                    Set-Variable -Name LocalSettingsMount  -Value $LocalSettingsMount  -Scope Script
                    Invoke-Job $job
                } -ArgumentList $exportScript, $j, $ServerTestsBin, $CoverageDir, $SettingsFile, $RunId,
                                $LwSecureConfigEnv, $LwSecureConfigMount, $LocalSettingsMount
                $bgJobs.Add($bg) | Out-Null
            }

            $done = Wait-Job -Job $bgJobs -Any -Timeout 5
            if ($done) {
                foreach ($d in @($done)) {
                    Write-Host "`n--- $($d.Name) output ---" -ForegroundColor DarkGray
                    Receive-Job $d
                    if ($d.State -eq 'Failed') { Write-Warn "Job $($d.Name) failed" }
                    Remove-Job $d
                    $bgJobs.Remove($d) | Out-Null
                }
            }
        }
    }

    # --- Merge ----------------------------------------------------------------
    Write-Step 'Merging coverage files...'
    # Glob all per-job XMLs but skip intermediate parts_*.cobertura.xml fragments
    # (those are already merged into their job's final output)
    $xmlsToMerge = @(Get-ChildItem -Path "$CoverageDir" -Recurse -Filter '*.cobertura.xml' -File |
        Where-Object {
            $_.FullName -notmatch '\\merged\\' -and
            $_.Name -notlike 'parts_*.cobertura.xml' -and
            $_.Length -gt 200
        } |
        ForEach-Object { $_.FullName })

    if ($xmlsToMerge.Count -eq 0) {
        Write-Warn 'No coverage files found - skipping merge and report'
    } else {
        Write-Done "Merging $($xmlsToMerge.Count) file(s)"
        $mergedXml = "$CoverageDir\merged\all_merged.cobertura.xml"
        dotnet-coverage merge @xmlsToMerge `
            --output $mergedXml `
            --output-format cobertura `
            --nologo
        Test-Exit 'dotnet-coverage merge'
        Write-Done "Merged: $([Math]::Round((Get-Item $mergedXml).Length / 1KB))KB"

        # Filter third-party packages
        Write-Step 'Filtering third-party packages from coverage XML...'
        $py = Get-Command python3 -ErrorAction SilentlyContinue
        if (-not $py) { $py = Get-Command python -ErrorAction SilentlyContinue }
        if ($py) {
            & $py $FilterScript "$CoverageDir\merged"
            Test-Exit 'filter_coverage.py'
            Write-Done 'Coverage filtered'
        } else {
            Write-Warn 'Python not found - coverage filter skipped'
        }

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
    # Defensive: kill any leftover ww-cov-*-$RunId containers/networks
    $leftover = docker ps -a --filter "name=ww-cov-.*-$RunId" --format '{{.Names}}' 2>$null
    if ($leftover) {
        Write-Step 'Cleaning leftover containers...'
        $leftover -split "`n" | Where-Object { $_ } | ForEach-Object {
            docker rm -f $_ 2>$null | Out-Null
        }
    }
    $netLeft = docker network ls --filter "name=ww-cov-.*-$RunId" --format '{{.Name}}' 2>$null
    if ($netLeft) {
        $netLeft -split "`n" | Where-Object { $_ } | ForEach-Object {
            docker network rm $_ 2>$null | Out-Null
        }
    }

    if ($LwSecureConfigFile -and (Test-Path $LwSecureConfigFile)) {
        Remove-Item $LwSecureConfigFile -Force -ErrorAction SilentlyContinue
    }
    if ($LocalSettingsFile -and (Test-Path $LocalSettingsFile)) {
        Remove-Item $LocalSettingsFile -Force -ErrorAction SilentlyContinue
    }
}
