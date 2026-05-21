# TestRun.ps1 - Warewolf consolidated test runner.
#
# Two operating modes selected by parameter shape:
#
#   Direct mode  - explicit -Projects/-Assemblies. Runs vstest locally (Windows
#                  bare-metal) or inside a Linux container when -InContainer is
#                  set. Manages the SUT lifecycle (LightweightExecution or
#                  FullServer) with optional dotnet-coverage instrumentation.
#                  This is the entry point every Dev\.azure\pipeline.yml step
#                  already invokes; argument surface is preserved.
#
#   Catalog mode - no -Projects/-Assemblies (or -Jobs ...). Parses
#                  Dev\.azure\pipeline.yml into a job catalog and dispatches
#                  every matched test job; default Pattern A (bare-metal
#                  Windows SUT + Linux dependency containers); -SUTRuntime Linux
#                  switches to Pattern B (Linux container SUT, the long-form
#                  Run-Coverage.ps1 path). Merges coverage and produces a
#                  report at the end.
#
# Replaces Dev\Run-Coverage.ps1 and Dev\run-tests-in-container.ps1.

[CmdletBinding()]
param(
    # ---- Direct-mode test selection (existing) ----
    [String[]] $Projects,
    [String[]] $Assemblies,
    [String[]] $ExcludeProjects = @(),
    [String[]] $ExcludeAssemblies = @(),
    [String]   $Category,
    [String[]] $Categories,
    [String[]] $ExcludeCategories,
    [String]   $Filter = "",
    [String]   $TestsToRun = "",
    [Int]      $RetryCount = 0,
    [Switch]   $RetryRebuild,

    # ---- Catalog-mode (new) ----
    [String[]] $Jobs = @(),
    [String]   $PipelineYml = "",
    [Switch]   $List,
    [Switch]   $IncludeDisabled,
    [Switch]   $SkipBuild,
    [Switch]   $SkipReport,
    [ValidateSet('Html','Badges','Cobertura','TextSummary','HtmlSummary','MarkdownSummary')]
    [String]   $ReportFormat = 'Html',
    [Switch]   $NoParallel,
    [Int]      $MaxParallel = 4,

    # ---- Runtime selection ----
    # -Runtime is the canonical knob: Windows = bare-metal SUT + Windows-native
    # deps (pyftpdlib, OpenSSH, choco, etc.); Linux = container SUT + Linux
    # docker deps. -SUTRuntime is kept as a deprecated alias.
    [ValidateSet('Windows','Linux')]
    [String]   $Runtime = "Windows",
    [ValidateSet('Windows','Linux')]
    [String]   $SUTRuntime = "",

    # ---- SUT lifecycle ----
    [ValidateSet('','LightweightExecution','FullServer')]
    [String]   $ServerType = "",
    [String]   $FuncExePath = "",
    [String]   $LightweightExecutionDir = "",
    [String]   $SharedConfigDir = "",

    # ---- Output / coverage ----
    [String]   $TestResultsDir = "",
    [String]   $CoverageDir = "",
    [String[]] $CoverageIncludeFiles = @(),
    [String]   $EngineSessionId = "",
    [String]   $EngineCoverageFile = "",
    [Switch]   $Coverage,
    [Switch]   $STA,
    [Switch]   $Sequential,
    [String]   $PreTestRunScript,
    [String]   $PostTestRunScript,
    [String]   $VSTestPath = "",
    [String]   $NuGet = "",
    [String]   $MSBuildPath = "",

    # ---- Linux container execution ----
    [Switch]   $InContainer,
    [String]   $InContainerImage = "warewolf-coverage-env",
    [Switch]   $RebuildImage,
    [Switch]   $UseHostNetwork,
    [String]   $BinDir = "",
    [String]   $InContainerVersion = "latest",
    [String]   $InContainerCommitID = "latest",

    # ---- Dep startup (runtime-aware: -Runtime Windows = native, Linux = docker) ----
    [Switch]   $StartFTPServer,
    [Switch]   $StartFTPSServer,
    [Switch]   $StartSFTPServer,
    [Switch]   $StartSambaShare,
    [Switch]   $StartMySQLServer,
    [Switch]   $StartElasticsearchServer,
    [Switch]   $StartRabbitMQServer,
    [Switch]   $StartRedisServer,
    [Switch]   $StartExchangeConnector,
    [String]   $StartMSSQLServer = "",

    # ---- Legacy Windows-native deps (deprecated: implied by -Runtime Windows) ----
    [Switch]   $LegacyWindowsDeps,
    [String]   $UNCPassword,
    [Switch]   $CreateUNCPath,
    [Switch]   $UseRegionalSettings,
    [Switch]   $CreateLocalSchedulerAdmin
)

$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

# ----------------------------------------------------------------------------
# Runtime normalisation
# ----------------------------------------------------------------------------
# -SUTRuntime is deprecated; map it to -Runtime when callers still pass it.
# Tracked separately so the catalog-mode sites at lines ~916, ~928, ~941, ~952,
# ~968, ~1029 keep reading $SUTRuntime as a synonym for $Runtime.
if ($PSBoundParameters.ContainsKey('SUTRuntime') -and -not $PSBoundParameters.ContainsKey('Runtime')) {
    Write-Warning "-SUTRuntime is deprecated; use -Runtime $SUTRuntime instead."
    $Runtime = $SUTRuntime
}
$SUTRuntime = $Runtime

# -Runtime Windows implies the existing -LegacyWindowsDeps switch (every
# Start-Host* function already gates its native branch on $LegacyWindowsDeps).
if ($Runtime -eq 'Windows' -and -not $LegacyWindowsDeps.IsPresent) {
    $LegacyWindowsDeps = [switch]::Present
}

# PS 6+ automatic vars - fallbacks for Windows PowerShell 5.1
if (-not (Test-Path Variable:\IsLinux))   { $IsLinux   = $false }
if (-not (Test-Path Variable:\IsMacOS))   { $IsMacOS   = $false }
if (-not (Test-Path Variable:\IsWindows)) { $IsWindows = $true  }

# ============================================================================
# Helpers
# ============================================================================

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

function Get-Slug([string]$JobName) {
    return ($JobName -replace '_', '-').ToLower()
}

function Invoke-Logged {
    Write-Host "+ $($args -join ' ')" -ForegroundColor DarkGray
    & $args[0] $args[1..($args.Count - 1)]
}

# Split comma-bundled single-string params back into arrays. The Azure DevOps
# task arg builder sometimes passes "A,B,C" as one element of a string[].
# The unary comma on every return path prevents PowerShell from unwrapping an
# empty/single-element array result into $null/scalar in the caller's scope.
function Split-CommaArray([string[]]$arr) {
    if ($null -eq $arr -or $arr.Count -eq 0) { return ,@() }
    if ($arr.Count -eq 1 -and $arr[0].Contains(",")) {
        $parts = @($arr[0].Split(",") | ForEach-Object { $_.Trim() } | Where-Object { $_ })
        return ,$parts
    }
    return ,$arr
}

# ============================================================================
# Path resolution
# ============================================================================

$ScriptDir = $PSScriptRoot
# The script may live either at the repo root (Dev\.azure\pipeline.yml visible)
# or in Bin\ServerTests (CI artifact layout, no Dev dir). Walk up if needed.
$RepoRoot = $ScriptDir
if (-not (Test-Path (Join-Path $RepoRoot "Dev\.azure\pipeline.yml"))) {
    $candidate = Split-Path $RepoRoot -Parent
    if ($candidate -and (Test-Path (Join-Path $candidate "Dev\.azure\pipeline.yml"))) {
        $RepoRoot = $candidate
    } else {
        $candidate = Split-Path $candidate -Parent
        if ($candidate -and (Test-Path (Join-Path $candidate "Dev\.azure\pipeline.yml"))) {
            $RepoRoot = $candidate
        }
    }
}

if (-not $PipelineYml) {
    $PipelineYml = Join-Path $RepoRoot "Dev\.azure\pipeline.yml"
}
$BinRoot        = Join-Path $RepoRoot "Bin"
$ServerTestsBin = Join-Path $BinRoot 'ServerTests'
$SettingsFile   = Join-Path $RepoRoot 'coverage-settings.xml'
$FilterScript   = Join-Path $RepoRoot 'Dev\.azure\filter_coverage.py'
$DockerfileTest = Join-Path $RepoRoot 'Dev\Warewolf.Execution.Lightweight\engine\docker\Dockerfile.test'
$DockerContext  = Split-Path $DockerfileTest -Parent
$CompileScript  = Join-Path $RepoRoot 'Compile.ps1'

$RunId = Get-Date -Format 'yyyyMMddHHmmss'

# ============================================================================
# Pipeline.yml parser (ported from Run-Coverage.ps1)
# ============================================================================

function ConvertFrom-PipelineYaml {
    param(
        [Parameter(Mandatory)][string]$YamlPath,
        [Switch]$IncludeDisabled
    )

    $text = [System.IO.File]::ReadAllText($YamlPath)
    $skip = @('build', 'Install_Func_CLI', 'MergeCoverage', 'build_release')

    $jobRegex = [regex]'(?ms)^  - job: (?<name>\S+)\s*$.*?(?=^  - job: |\Z)'
    $catalog  = New-Object System.Collections.Generic.List[object]

    $disabledNames = @{}
    if ($IncludeDisabled) {
        # Track which jobs were sourced from commented blocks so -List can flag them.
        $disabledRegex = [regex]'(?m)^#  - job: (?<name>\S+)\s*$'
        foreach ($dm in $disabledRegex.Matches($text)) {
            $disabledNames[$dm.Groups['name'].Value] = $true
        }
        # Strip the leading '#' from every commented line so the existing job regex
        # picks up disabled job blocks. The commented format in pipeline.yml is a
        # single '#' followed by the original indentation, so removing only '^#'
        # restores the YAML structure exactly.
        $text = [regex]::Replace($text, '(?m)^#', '')
    }

    foreach ($m in $jobRegex.Matches($text)) {
        $name = $m.Groups['name'].Value
        if ($skip -contains $name) { continue }
        $body = $m.Value

        # The job body contains many shell snippets that also use `-Filter`,
        # `-Path`, etc. Narrow argument extraction to the TestRun.ps1
        # invocation's `arguments: >-` block (YAML block scalar) only.
        $argsBlock = $body
        if ($body -match '(?ms)filePath:[^\n]*TestRun\.ps1[^\n]*\n(?<rest>.*?)\n\s*(?:workingDirectory|env|displayName):') {
            $argsBlock = $matches['rest']
        }

        $entry = [PSCustomObject]@{
            Name              = $name
            Slug              = Get-Slug $name
            Type              = if ($body -match 'start-engine-coverage\.sh' -or $body -match '--session-id\s+\S+' -or $body -match '-ServerType\s+LightweightExecution' -or $body -match '-ServerType\s+FullServer') { 'EngineSpec' } else { 'Unit' }
            Assembly          = $null
            Assemblies        = @()
            ExcludeAssemblies = @()
            Filter            = $null
            Output            = $null
            SessionId         = $null
            Sidecars          = @()
            Artifact          = $null
            # Windows-native dep flags lifted from the args block so catalog mode can
            # replay Windows bare-metal jobs that provision pyftpdlib/Samba/SMB share
            # via the Start-Host* / -CreateUNCPath flags rather than docker images.
            WinDepFlags       = @()
            MSSQLArg          = $null
            Disabled          = [bool]$disabledNames[$name]
        }

        if ($argsBlock -match '-EngineSessionId\s+"([^"]+)"') {
            $entry.SessionId = $matches[1]
        } elseif ($body -match '(?ms)start-engine-coverage\.sh\s*\\\s*\r?\n\s*(\S+)\s*\\') {
            $entry.SessionId = $matches[1]
        } elseif ($body -match '--session-id\s+(\S+)') {
            $entry.SessionId = $matches[1]
        }

        if ($argsBlock -match '-EngineCoverageFile\s+"\$\(Agent\.BuildDirectory\)\\coverage\\([^"]+\.cobertura\.xml)"') {
            $entry.Output = $matches[1]
        } elseif ($body -match '"\$\(Agent\.BuildDirectory\)/coverage/([^"]+\.cobertura\.xml)"') {
            $entry.Output = $matches[1]
        }

        # -Projects "A","B","C" or -Assemblies "A","B","C"
        $assemblyMatch = $null
        if ($argsBlock -match '-Projects\s+((?:"[^"]+"(?:\s*,\s*)?)+)') {
            $assemblyMatch = $matches[1]
        } elseif ($argsBlock -match '-Assemblies\s+((?:"[^"]+"(?:\s*,\s*)?)+)') {
            $assemblyMatch = $matches[1]
        }
        if ($assemblyMatch) {
            $assemblies = @([regex]::Matches($assemblyMatch, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
            $entry.Assembly   = $assemblies[0]
            $entry.Assemblies = $assemblies
        }

        if ($argsBlock -match '-ExcludeProjects\s+((?:"[^"]+"(?:\s*,\s*)?)+)') {
            $entry.ExcludeAssemblies = @([regex]::Matches($matches[1], '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
        } elseif ($argsBlock -match '-ExcludeAssemblies\s+((?:"[^"]+"(?:\s*,\s*)?)+)') {
            $entry.ExcludeAssemblies = @([regex]::Matches($matches[1], '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value })
        }

        if ($argsBlock -match "-Filter\s+'([^']+)'") {
            $entry.Filter = $matches[1]
        } elseif ($argsBlock -match '-Filter\s+"([^"]+)"') {
            $entry.Filter = $matches[1]
        }

        if ($body -match "artifactName:\s+'([^']+)'") {
            $entry.Artifact = $matches[1]
        }

        $sidecars = @()
        if ($body -match 'stilliard/pure-ftpd')                       { $sidecars += 'ftp' }
        if ($body -match 'atmoz/sftp')                                { $sidecars += 'sftp' }
        if ($body -match 'dperson/samba')                             { $sidecars += 'samba' }
        if ($body -match 'mssql/server')                              { $sidecars += 'sqlserver' }
        if ($body -match 'rabbitmq:3-management')                     { $sidecars += 'rabbitmq' }
        if ($body -match 'redis:7-alpine')                            { $sidecars += 'redis' }
        if ($body -match 'docker\.elastic\.co/elasticsearch')         { $sidecars += 'elasticsearch' }
        if ($body -match 'warewolfserver/exchange-connector-testing') { $sidecars += 'exchange' }
        $entry.Sidecars = $sidecars

        # Windows-native dep flags from the args block. Recognised switches are
        # added verbatim so Invoke-WindowsBareMetalJob can splat them straight
        # into the recursive TestRun.ps1 invocation.
        $winSwitches = @(
            'StartFTPServer','StartFTPSServer','StartSFTPServer','StartSambaShare',
            'StartMySQLServer','StartElasticsearchServer','StartRabbitMQServer',
            'StartRedisServer','StartExchangeConnector',
            'CreateUNCPath','UseRegionalSettings','CreateLocalSchedulerAdmin','LegacyWindowsDeps'
        )
        foreach ($flag in $winSwitches) {
            if ($argsBlock -match "(?<!\w)-$flag(?!\w)") {
                $entry.WinDepFlags += $flag
            }
        }
        # -StartMSSQLServer takes a string argument (empty "" is valid).
        if ($argsBlock -match '-StartMSSQLServer\s+"([^"]*)"') {
            $entry.MSSQLArg = $matches[1]
        } elseif ($argsBlock -match '(?<!\w)-StartMSSQLServer(?!\w)') {
            $entry.MSSQLArg = ''
        }

        $catalog.Add($entry) | Out-Null
    }
    return ,$catalog.ToArray()
}

# ============================================================================
# Linux-mode sidecar dispatcher (Pattern B; --network=container:X)
# ============================================================================

function Get-SidecarName($Type, $Slug, $RunId) {
    return "ww-cov-$Type-$Slug-$RunId"
}

function Start-LinuxSidecar {
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
                -e USER='smbuser%smbpass' -e SHARE='share;/share;yes;no;no;smbuser' `
                dperson/samba -u 'smbuser;smbpass' -s 'share;/share;yes;no;no;smbuser' | Out-Null
        }
        'sqlserver' {
            docker run -d --name $name --network="container:$TestContainer" `
                -e ACCEPT_EULA=Y -e SA_PASSWORD='Test123456!' -e MSSQL_PID=Developer `
                mcr.microsoft.com/mssql/server:2019-latest | Out-Null
        }
        'rabbitmq' {
            docker run -d --name $name --network="container:$TestContainer" rabbitmq:3-management | Out-Null
        }
        'redis' {
            docker run -d --name $name --network="container:$TestContainer" redis:7-alpine | Out-Null
        }
        'elasticsearch' {
            docker run -d --name $name --network="container:$TestContainer" `
                -e 'discovery.type=single-node' -e 'xpack.security.enabled=false' `
                -e 'ES_JAVA_OPTS=-Xms512m -Xmx512m' `
                docker.elastic.co/elasticsearch/elasticsearch:8.17.4 | Out-Null
        }
        'exchange' {
            docker run -d --name $name --network="container:$TestContainer" `
                warewolfserver/exchange-connector-testing 2>$null | Out-Null
        }
        default {
            Write-Warn "Unknown sidecar type '$Type'"
        }
    }
}

# ============================================================================
# Host-port dep startup (Pattern A; sidecars publish to host ports)
# ============================================================================

# Resolve a writable base directory for native pyftpdlib sandboxes + entrypoint
# scripts. CI hosted agents run as admin and have C:\ available; local dev runs
# fall back to TEMP so the same script works without elevation. Cached for the
# session so FTP and FTPS land on the same root.
function Get-FTPSandboxRoot {
    if ($script:_ftpSandboxRoot) { return $script:_ftpSandboxRoot }
    $candidates = @('C:\', (Join-Path $env:LOCALAPPDATA 'Warewolf-TestRun'), $env:TEMP)
    foreach ($c in $candidates) {
        if (-not $c) { continue }
        if (-not (Test-Path $c)) {
            try { New-Item -ItemType Directory -Force -Path $c -ErrorAction Stop | Out-Null } catch { continue }
        }
        try {
            $probe = Join-Path $c ('_wwprobe_' + [Guid]::NewGuid().ToString('N') + '.tmp')
            Set-Content -LiteralPath $probe -Value 'x' -ErrorAction Stop
            Remove-Item -LiteralPath $probe -Force -ErrorAction SilentlyContinue
            $script:_ftpSandboxRoot = $c
            return $c
        } catch { continue }
    }
    throw "No writable sandbox root found (tried C:\, %LOCALAPPDATA%\Warewolf-TestRun, %TEMP%)."
}

function Start-HostFTPServer {
    if ($LegacyWindowsDeps) {
        # Native pyftpdlib path (Windows runtime).
        $ftpRoot       = Get-FTPSandboxRoot
        $ftpHomeBase   = Join-Path $ftpRoot 'ftp_home\dev2'
        $ftpEntryFile  = Join-Path $ftpRoot 'ftp_entrypoint.py'
        # Subdirs every FileAndFolder .feature file references under
        # ftp://localhost:21/. pyftpdlib does not auto-create parent
        # directories on STOR, so a missing FOR*TESTING folder turns a
        # legitimate write into a 550-failure and the spec reports "Failure".
        foreach ($sub in
            'FORCOPYFILETESTING',
            'FORCREATEFILETESTING',
            'FORDELETEFILETESTING',
            'FOREADFOLDERTTESTING',
            'FORFILERENAMETESTING',
            'FORMOVEFILETESTING',
            'FORREADFILETESTING',
            'FORREADFOLDERTESTING',
            'FORTESTING',
            'FORUNZIPTESTING',
            'FORWRITEFILETESTING',
            'FORZIPTESTING'
        ) {
            $d = Join-Path $ftpHomeBase $sub
            if (!(Test-Path $d)) { mkdir $d | Out-Null }
        }
        pip install pyftpdlib
        # Forward-slash form is what pyftpdlib expects in the embedded Python.
        $ftpHomeForPy = ($ftpHomeBase -replace '\\','/')
        $pyBody = @"
import os
from pyftpdlib.authorizers import DummyAuthorizer
from pyftpdlib.handlers import FTPHandler
from pyftpdlib.servers import FTPServer

PASSIVE_PORTS = '17000-17007'

def main():
    authorizer = DummyAuthorizer()
    user_dir = "$ftpHomeForPy"
    if not os.path.isdir(user_dir): os.makedirs(user_dir)
    authorizer.add_user("dev2", "Q/ulw&]", user_dir, perm="elradfmw")

    handler = FTPHandler
    handler.authorizer = authorizer
    handler.permit_foreign_addresses = True
    passive_ports = list(map(int, PASSIVE_PORTS.split('-')))
    handler.passive_ports = range(passive_ports[0], passive_ports[1])

    server = FTPServer(('0.0.0.0', 21), handler)
    server.serve_forever()

if __name__ == '__main__':
    main()
"@
        # Always overwrite — the embedded user_dir is sandbox-root-dependent and
        # may change between -CI- and -local- runs of the same checkout.
        $pyBody | Out-File -LiteralPath $ftpEntryFile -Encoding utf8 -Force
        # `pythonw -u file.py` foreground would block here forever (serve_forever
        # never returns). Launch via Start-Process so the orchestrator continues,
        # then poll port 21 until the listener is up.
        $pythonwCmd = (Get-Command pythonw -ErrorAction SilentlyContinue).Source
        if (-not $pythonwCmd) { $pythonwCmd = (Get-Command python -ErrorAction SilentlyContinue).Source }
        if (-not $pythonwCmd) { Write-Warn 'pythonw/python not found; cannot start FTP server'; return }
        $script:_ftpProcess = Start-Process -FilePath $pythonwCmd `
            -ArgumentList @('-u', $ftpEntryFile) -PassThru -WindowStyle Hidden
        Write-Host "Waiting for FTP server on port 21..."
        for ($i = 1; $i -le 30; $i++) {
            try { (New-Object System.Net.Sockets.TcpClient('127.0.0.1', 21)).Close(); Write-Host "FTP server ready"; return } catch { Start-Sleep -Milliseconds 500 }
        }
        Write-Warn "FTP server did not bind port 21 within 15s"
        return
    }
    docker run -d --name ftpserver `
        -p 21:21 -p 30000-30009:30000-30009 `
        -e FTP_USER_NAME=dev2 -e "FTP_USER_PASS=Q/ulw&]" `
        -e FTP_USER_HOME=/home/ftpusers/dev2 `
        -e PASV_MIN_PORT=30000 -e PASV_MAX_PORT=30009 `
        stilliard/pure-ftpd | Out-Null
    Start-Sleep -Seconds 3
    docker exec ftpserver mkdir -p /home/ftpusers/dev2/FORCOPYFILETESTING 2>$null | Out-Null
    docker exec ftpserver chmod -R 777 /home/ftpusers/dev2 2>$null | Out-Null
}

function Stop-HostFTPServer {
    if ($LegacyWindowsDeps) {
        # cmd /c swallows taskkill's stderr + non-zero exit when pythonw is
        # already gone (e.g. a sibling Stop-HostFTPSServer already killed it).
        # Stop's ErrorActionPreference would otherwise abort the whole script.
        cmd /c 'taskkill /im pythonw.exe /f >nul 2>nul'
        $global:LASTEXITCODE = 0
        return
    }
    docker rm -f ftpserver 2>$null | Out-Null
}

function Start-HostFTPSServer {
    # pyftpdlib + TLS_FTPHandler on port 1010. Mirrors Start-HostFTPServer's
    # pyftpdlib pattern (port 21, FTPHandler) so dev2 / Q/ulw&] / passive range
    # behaviour is identical across the plain and TLS variants. Replaces four
    # prior attempts:
    #   * Docker stilliard/pure-ftpd  — hosted windows-2022 has no Linux Docker
    #   * FileZilla Server via choco  — choco lands on 0.9.x, not 1.x
    #   * IIS FTPS                    — works on Server but Win11 client + PS7
    #                                   blocked by 530.5.1 LogonUser regression
    #   * pyftpdlib older versions    — earlier TLS_FTPHandler builds hung
    #                                   .NET FtpWebRequest's data close_notify
    # Current pyftpdlib 2.2+ interoperates cleanly with .NET FtpWebRequest.
    $ftpRoot       = Get-FTPSandboxRoot
    $ftpsHomeBase  = Join-Path $ftpRoot 'ftps_home\dev2'
    $ftpsEntryFile = Join-Path $ftpRoot 'ftps_entrypoint.py'
    $ftpsPemFile   = Join-Path $ftpRoot 'ftps_cert.pem'
    $ftpsLogFile   = Join-Path $ftpRoot 'ftps_server.log'
    $ftpsErrFile   = Join-Path $ftpRoot 'ftps_server.err.log'

    # 1. Subdirs every File/Folder .feature references under ftps://localhost:1010/.
    foreach ($sub in
        'FORCOPYFILETESTING','FORCREATEFILETESTING','FORDELETEFILETESTING',
        'FORFILERENAMETESTING','FORMOVEFILETESTING','FORREADFILETESTING',
        'FORREADFOLDERTESTING','FORRENAMETESTING','FORTESTING',
        'FORUNZIPTESTING','FORWRITEFILETESTING','FORZIPTESTING'
    ) {
        $d = Join-Path $ftpsHomeBase $sub
        if (!(Test-Path $d)) { mkdir $d | Out-Null }
    }
    foreach ($i in 0..4) {
        $seed = Join-Path $ftpsHomeBase "FORCOPYFILETESTING\copyfile$i.txt"
        if (!(Test-Path $seed)) { 'testcontent' | Out-File -LiteralPath $seed -Encoding ascii -Force }
    }

    # 2. Self-signed PEM (cert + PKCS8 key) for TLS_FTPHandler. Re-generated each
    # call so an expired/corrupted cert never blocks tests. .NET FtpWebRequest
    # bypasses cert validation when IsNotCertVerifiable=true (set in CopySteps),
    # so chain trust doesn't matter.
    $rsa = [System.Security.Cryptography.RSA]::Create(2048)
    try {
        $req = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
            'CN=localhost-pyftps,O=Warewolf,C=ZA', $rsa,
            [System.Security.Cryptography.HashAlgorithmName]::SHA256,
            [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
        $cert = $req.CreateSelfSigned(
            [DateTimeOffset]::UtcNow.AddDays(-1),
            [DateTimeOffset]::UtcNow.AddYears(5))
        $certB64 = [Convert]::ToBase64String(
            $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert),
            'InsertLineBreaks')
        $keyB64  = [Convert]::ToBase64String($rsa.ExportPkcs8PrivateKey(), 'InsertLineBreaks')
        "-----BEGIN CERTIFICATE-----`n$certB64`n-----END CERTIFICATE-----`n-----BEGIN PRIVATE KEY-----`n$keyB64`n-----END PRIVATE KEY-----`n" |
            Set-Content -LiteralPath $ftpsPemFile -Encoding ascii -NoNewline
    } finally { $rsa.Dispose() }

    # 3. Install pyftpdlib + pyOpenSSL (idempotent). pyftpdlib's TLS_FTPHandler
    # imports `from OpenSSL import SSL, crypto` lazily at instantiation; without
    # pyOpenSSL the server process exits immediately with ImportError and the
    # port-21-style "FTPHandler" install of pyftpdlib alone is not enough.
    pip install pyftpdlib pyOpenSSL | Out-Null

    # 4. Entrypoint script. Forward-slash form is what Python expects in literals.
    $ftpsHomeForPy = ($ftpsHomeBase -replace '\\','/')
    $ftpsPemForPy  = ($ftpsPemFile  -replace '\\','/')
    $pyBody = @"
import logging
import os
from pyftpdlib.authorizers import DummyAuthorizer
from pyftpdlib.handlers import TLS_FTPHandler, TLS_DTPHandler
from pyftpdlib.servers import FTPServer

# Keep DEBUG on — small enough on a per-test-run basis and invaluable when
# the next interop quirk surfaces. Artifacts are scoped per job.
logging.basicConfig(level=logging.DEBUG)

# Workaround for a .NET FtpWebRequest vs pyOpenSSL interop bug. After a
# successful STOR, .NET closes the data-channel TCP socket without sending
# a TLS close_notify; pyOpenSSL's SSL_shutdown() then returns Error([])
# (incomplete shutdown), and pyftpdlib's asyncore loop calls
# _do_ssl_shutdown forever (~60k attempts in a single 100s window — verified
# in a prior CI artifact). The server never sends '226 Transfer Complete'
# because it is stuck tearing down the data channel, so the client's 100s
# Timeout fires and the test reports 'underlying connection was closed: An
# unexpected error occurred on a receive'. Cap shutdown attempts at 3 and
# force-close — DTPHandler.close() triggers the parent FTPHandler's
# transfer-complete path which finally sends the 226. TLS-truncation risk
# is acceptable on a localhost test harness; production FTPS clients that
# send close_notify still get the original shutdown path on attempts 1-3.
_orig_do_ssl_shutdown = TLS_DTPHandler._do_ssl_shutdown
def _capped_do_ssl_shutdown(self):
    attempts = getattr(self, '_ww_ssl_shutdown_attempts', 0) + 1
    self._ww_ssl_shutdown_attempts = attempts
    if attempts > 3:
        self._ssl_established = False
        self._ssl_closing = False
        try:
            self.close()
        except Exception:
            pass
        return
    return _orig_do_ssl_shutdown(self)
TLS_DTPHandler._do_ssl_shutdown = _capped_do_ssl_shutdown

PASSIVE_PORTS = '56001-56008'

def main():
    authorizer = DummyAuthorizer()
    user_dir = "$ftpsHomeForPy"
    if not os.path.isdir(user_dir): os.makedirs(user_dir)
    authorizer.add_user("dev2", "Q/ulw&]", user_dir, perm="elradfmw")

    handler = TLS_FTPHandler
    handler.authorizer = authorizer
    handler.certfile = "$ftpsPemForPy"
    handler.tls_control_required = True
    handler.tls_data_required = True
    handler.permit_foreign_addresses = True
    passive_ports = list(map(int, PASSIVE_PORTS.split('-')))
    handler.passive_ports = range(passive_ports[0], passive_ports[1] + 1)

    server = FTPServer(('0.0.0.0', 1010), handler)
    server.serve_forever()

if __name__ == '__main__':
    main()
"@
    $pyBody | Out-File -LiteralPath $ftpsEntryFile -Encoding utf8 -Force

    # 5. Firewall — control + PASV. pyftpdlib binds the listener itself but the
    # Windows Defender filter still drops inbound on the data ports.
    foreach ($rule in @(
        @{Name='Warewolf-FTPS-Control'; Port='1010'},
        @{Name='Warewolf-FTPS-PASV';    Port='56001-56008'})) {
        if (-not (Get-NetFirewallRule -DisplayName $rule.Name -ErrorAction SilentlyContinue)) {
            New-NetFirewallRule -DisplayName $rule.Name -Direction Inbound -Action Allow `
                -Protocol TCP -LocalPort $rule.Port -ErrorAction SilentlyContinue | Out-Null
        }
    }

    # 6. Launch detached so the orchestrator continues; poll port 1010. Use
    # python.exe (not pythonw) with stdout/stderr redirected so an ImportError
    # or TLS-init crash lands in a log we can dump on timeout — pythonw drops
    # both streams on Windows, which is what hid the previous failure mode.
    $pythonCmd = (Get-Command python -ErrorAction SilentlyContinue).Source
    if (-not $pythonCmd) { $pythonCmd = (Get-Command pythonw -ErrorAction SilentlyContinue).Source }
    if (-not $pythonCmd) { throw 'python/pythonw not found; cannot start FTPS server' }
    if (Test-Path $ftpsLogFile) { Remove-Item -LiteralPath $ftpsLogFile -Force -ErrorAction SilentlyContinue }
    if (Test-Path $ftpsErrFile) { Remove-Item -LiteralPath $ftpsErrFile -Force -ErrorAction SilentlyContinue }
    $script:_ftpsProcess = Start-Process -FilePath $pythonCmd `
        -ArgumentList @('-u', $ftpsEntryFile) -PassThru -WindowStyle Hidden `
        -RedirectStandardOutput $ftpsLogFile -RedirectStandardError $ftpsErrFile

    Write-Host "Waiting for FTPS server on port 1010..."
    for ($i = 1; $i -le 30; $i++) {
        try { (New-Object System.Net.Sockets.TcpClient('127.0.0.1', 1010)).Close(); Write-Host "FTPS server ready"; return } catch { Start-Sleep -Milliseconds 500 }
    }
    Write-Warn "FTPS server did not bind port 1010 within 15s"
    foreach ($pair in @(@($ftpsErrFile,'stderr'), @($ftpsLogFile,'stdout'))) {
        if (Test-Path $pair[0]) {
            $body = (Get-Content -LiteralPath $pair[0] -Raw -ErrorAction SilentlyContinue)
            if ($body) { Write-Host "--- FTPS server $($pair[1]) ($($pair[0])) ---`n$body`n--- end ---" }
        }
    }
}

function Stop-HostFTPSServer {
    # Copy pyftpdlib's stdout/stderr logs into $TestResultsDir before killing
    # the server, so the pipeline's existing PublishBuildArtifacts step picks
    # them up as `*_ServerLogs/<job>/ftps_server.*.log`. Per-call timestamp +
    # PID guards against retries clobbering earlier logs. Wrapped in try/catch
    # so a missing TestResultsDir or a locked log file can never abort the
    # surrounding teardown — the test results matter more than the log copy.
    try {
        if ($TestResultsDir -and (Test-Path $TestResultsDir)) {
            $ftpRoot = Get-FTPSandboxRoot
            $stamp   = (Get-Date -Format 'yyyyMMdd_HHmmss')
            $pidTag  = if ($script:_ftpsProcess) { $script:_ftpsProcess.Id } else { 'na' }
            foreach ($name in 'ftps_server.log','ftps_server.err.log') {
                $src = Join-Path $ftpRoot $name
                if (Test-Path $src) {
                    $dst = Join-Path $TestResultsDir ("{0}_{1}_{2}" -f $stamp, $pidTag, $name)
                    Copy-Item -LiteralPath $src -Destination $dst -Force -ErrorAction SilentlyContinue
                }
            }
        }
    } catch { }
    # Stop-HostFTPServer kills all pythonw.exe — call only one of the two stop
    # functions in a teardown sequence (the second is a no-op). Wrapped in cmd
    # /c so taskkill's stderr + non-zero exit when the process is already gone
    # doesn't abort the surrounding script. taskkill also matches python.exe
    # (the FTPS server now runs under python.exe so stdout/stderr can be
    # redirected — see Start-HostFTPSServer step 6).
    cmd /c 'taskkill /im pythonw.exe /f >nul 2>nul'
    cmd /c 'taskkill /im python.exe /f >nul 2>nul'
    $global:LASTEXITCODE = 0
}

function Start-HostSFTPServer {
    if ($LegacyWindowsDeps) {
        # Windows OpenSSH server on port 2222 (matching atmoz/sftp's published port).
        $cap = Get-WindowsCapability -Online -Name 'OpenSSH.Server*' -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($cap -and $cap.State -ne 'Installed') {
            Add-WindowsCapability -Online -Name $cap.Name | Out-Null
        }
        # Provision the test user if missing. Hosted windows-2022 agents
        # enforce password complexity that rejects 'ftppass'; try the legacy
        # password first to keep atmoz/sftp-equivalent creds for tests, then
        # fall back to a policy-compliant value if rejected.
        if (-not (Get-LocalUser -Name 'ftpuser' -ErrorAction SilentlyContinue)) {
            $created = $false
            foreach ($candidate in 'ftppass','Ftppass!2026') {
                try {
                    $pw = ConvertTo-SecureString $candidate -AsPlainText -Force
                    New-LocalUser -Name 'ftpuser' -Password $pw `
                        -PasswordNeverExpires -AccountNeverExpires `
                        -UserMayNotChangePassword -ErrorAction Stop | Out-Null
                    $created = $true
                    break
                } catch [Microsoft.PowerShell.Commands.InvalidPasswordException] {
                    Write-Warn "Password policy rejected '$candidate' for ftpuser; trying next"
                }
            }
            if (-not $created) { Write-Warn "Could not create ftpuser; SFTP auth will fail" }
        }
        $cfg = "$env:ProgramData\ssh\sshd_config"
        if (Test-Path $cfg) {
            $body = Get-Content $cfg -Raw
            $body = $body -replace '(?m)^\s*#?\s*Port\s+\d+\s*$',          'Port 2222'
            $body = $body -replace '(?m)^\s*#?\s*PasswordAuthentication.*$', 'PasswordAuthentication yes'
            Set-Content -LiteralPath $cfg -Value $body -Encoding ascii
        } else {
            "Port 2222`nPasswordAuthentication yes`nSubsystem sftp sftp-server.exe" | Out-File -LiteralPath $cfg -Encoding ascii -Force
        }
        New-NetFirewallRule -DisplayName 'OpenSSH-Server-2222' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 2222 -ErrorAction SilentlyContinue | Out-Null
        Set-Service -Name sshd       -StartupType Automatic
        Set-Service -Name 'ssh-agent' -StartupType Automatic -ErrorAction SilentlyContinue
        Restart-Service sshd
        Start-Sleep -Seconds 2
        return
    }
    docker run -d --name sftpserver -p 2222:22 atmoz/sftp ftpuser:ftppass:1001 | Out-Null
    Start-Sleep -Seconds 3
}
function Stop-HostSFTPServer {
    if ($LegacyWindowsDeps) {
        Stop-Service sshd -ErrorAction SilentlyContinue
        return
    }
    docker rm -f sftpserver 2>$null | Out-Null
}

function Start-HostMySQLServer {
    if ($LegacyWindowsDeps) {
        # Bare-metal MySQL via chocolatey. Bootstrap schema is taken from
        # C:\Users\ultra\mysql-connector-testing\mysqldump.sql (the source repo
        # of the docker image used in Linux mode); both must stay in sync.
        if (-not (Get-Service -Name 'MySQL*' -ErrorAction SilentlyContinue)) {
            choco install mysql -y --no-progress
        }
        $svc = Get-Service -Name 'MySQL*' -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($svc -and $svc.Status -ne 'Running') { Start-Service $svc.Name }
        # Wait until tcp/3306 answers.
        for ($i = 1; $i -le 20; $i++) {
            try { (New-Object System.Net.Sockets.TcpClient('127.0.0.1', 3306)).Close(); break } catch { Start-Sleep 2 }
        }
        # Idempotent password set + schema load. Tests connect with root/admin
        # to match the docker image's MYSQL_ROOT_PASSWORD=admin baseline.
        $mysqladmin = Get-Command mysqladmin -ErrorAction SilentlyContinue
        $mysql      = Get-Command mysql      -ErrorAction SilentlyContinue
        if (-not $mysql) { Write-Warn "mysql CLI not on PATH after choco install; skipping schema load"; return }
        & $mysqladmin.Path -uroot password 'admin' 2>$null | Out-Null
        $dump = "$PSScriptRoot\mysql-bootstrap.sql"
        if (-not (Test-Path $dump)) { $dump = "C:\mysql-bootstrap.sql" }
        if (Test-Path $dump) {
            cmd /c "`"$($mysql.Path)`" -uroot -padmin < `"$dump`""
        } else {
            Write-Warn "No mysql-bootstrap.sql found at $PSScriptRoot or C:\; tests that depend on schema may fail"
        }
        return
    }
    docker run -d -p 3306:3306 --name mysql-connector-testing registry.gitlab.com/warewolf/mysql-connector-testing | Out-Null
}
function Stop-HostMySQLServer {
    if ($LegacyWindowsDeps) {
        Get-Service -Name 'MySQL*' -ErrorAction SilentlyContinue | Stop-Service -ErrorAction SilentlyContinue
        return
    }
    docker rm -f mysql-connector-testing 2>$null | Out-Null
}

function Start-HostElasticsearchServer {
    if ($LegacyWindowsDeps) {
        # Bare-metal Elasticsearch from the Windows zip. Bundles its own JDK so
        # no JRE install is needed.
        $esVer = '8.17.4'
        $esDir = "C:\elasticsearch-$esVer"
        $esZip = "$env:TEMP\elasticsearch-$esVer.zip"
        if (-not (Test-Path "$esDir\bin\elasticsearch.bat")) {
            if (-not (Test-Path $esZip)) {
                Invoke-WebRequest -UseBasicParsing `
                    -Uri "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-$esVer-windows-x86_64.zip" `
                    -OutFile $esZip
            }
            Expand-Archive -LiteralPath $esZip -DestinationPath 'C:\' -Force
        }
        $yml = "$esDir\config\elasticsearch.yml"
        @"
discovery.type: single-node
xpack.security.enabled: false
xpack.security.enrollment.enabled: false
xpack.security.http.ssl.enabled: false
xpack.security.transport.ssl.enabled: false
network.host: 0.0.0.0
http.port: 9200
"@ | Out-File -LiteralPath $yml -Encoding ascii -Force
        $env:ES_JAVA_OPTS = '-Xms512m -Xmx512m'
        $script:_elasticsearchProcess = Start-Process -FilePath "$esDir\bin\elasticsearch.bat" -PassThru -WindowStyle Hidden
        Write-Host "Waiting for Elasticsearch on port 9200..."
        for ($i = 1; $i -le 60; $i++) {
            try {
                $status = (Invoke-WebRequest -Uri 'http://localhost:9200/_cluster/health' -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop).StatusCode
            } catch { $status = 0 }
            if ($status -eq 200) { Write-Host "Elasticsearch ready"; return }
            Start-Sleep -Seconds 2
        }
        Write-Warn "Elasticsearch did not become ready within 120s"
        return
    }
    docker run -d --name elasticsearch-coverage -p 9200:9200 `
        -e "discovery.type=single-node" -e "xpack.security.enabled=false" `
        -e "ES_JAVA_OPTS=-Xms512m -Xmx512m" `
        docker.elastic.co/elasticsearch/elasticsearch:8.17.4 | Out-Null
    Write-Host "Waiting for Elasticsearch on port 9200..."
    for ($i = 1; $i -le 30; $i++) {
        try {
            $status = (Invoke-WebRequest -Uri 'http://localhost:9200/_cluster/health' -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop).StatusCode
        } catch { $status = 0 }
        if ($status -eq 200) { Write-Host "Elasticsearch ready"; return }
        Start-Sleep -Seconds 2
    }
    Write-Warn "Elasticsearch did not become ready within 60s"
}
function Stop-HostElasticsearchServer {
    if ($LegacyWindowsDeps) {
        if ($script:_elasticsearchProcess -and -not $script:_elasticsearchProcess.HasExited) {
            $script:_elasticsearchProcess | Stop-Process -Force -ErrorAction SilentlyContinue
        }
        Get-Process -Name 'java' -ErrorAction SilentlyContinue |
            Where-Object { $_.Path -and $_.Path -like '*elasticsearch*' } |
            Stop-Process -Force -ErrorAction SilentlyContinue
        return
    }
    docker rm -f elasticsearch-coverage 2>$null | Out-Null
}

function Start-HostRabbitMQServer {
    if ($LegacyWindowsDeps) {
        if (-not (Get-Service -Name 'RabbitMQ' -ErrorAction SilentlyContinue)) {
            choco install rabbitmq -y --no-progress
        }
        Start-Service -Name 'RabbitMQ' -ErrorAction SilentlyContinue
        for ($i = 1; $i -le 30; $i++) {
            try { (New-Object System.Net.Sockets.TcpClient('127.0.0.1', 5672)).Close(); return } catch { Start-Sleep 2 }
        }
        Write-Warn "RabbitMQ did not bind 5672 within 60s"
        return
    }
    docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management | Out-Null
    Start-Sleep -Seconds 5
}
function Stop-HostRabbitMQServer {
    if ($LegacyWindowsDeps) {
        Stop-Service -Name 'RabbitMQ' -ErrorAction SilentlyContinue
        return
    }
    docker rm -f rabbitmq 2>$null | Out-Null
}

function Start-HostRedisServer {
    if ($LegacyWindowsDeps) {
        if (-not (Get-Service -Name 'Redis' -ErrorAction SilentlyContinue)) {
            choco install redis-64 -y --no-progress
        }
        Start-Service -Name 'Redis' -ErrorAction SilentlyContinue
        for ($i = 1; $i -le 15; $i++) {
            try { (New-Object System.Net.Sockets.TcpClient('127.0.0.1', 6379)).Close(); return } catch { Start-Sleep 1 }
        }
        Write-Warn "Redis did not bind 6379 within 15s"
        return
    }
    docker run -d --name redis -p 6379:6379 redis:7-alpine | Out-Null
    Start-Sleep -Seconds 2
}
function Stop-HostRedisServer {
    if ($LegacyWindowsDeps) {
        Stop-Service -Name 'Redis' -ErrorAction SilentlyContinue
        return
    }
    docker rm -f redis 2>$null | Out-Null
}

function Start-HostSambaShare {
    if ($LegacyWindowsDeps) {
        $share = 'C:\smb_share'
        if (-not (Test-Path $share)) { New-Item -ItemType Directory -Force -Path $share | Out-Null }
        if (-not (Get-LocalUser -Name 'smbuser' -ErrorAction SilentlyContinue)) {
            # Hosted Windows agents enforce password complexity that rejects 'smbpass'.
            # Try the legacy value first (matches dperson/samba creds) and fall back
            # to a policy-compliant password if rejected.
            $created = $false
            foreach ($candidate in 'smbpass','Smbpass!2026') {
                try {
                    $pw = ConvertTo-SecureString $candidate -AsPlainText -Force
                    New-LocalUser -Name 'smbuser' -Password $pw `
                        -PasswordNeverExpires -AccountNeverExpires `
                        -UserMayNotChangePassword -ErrorAction Stop | Out-Null
                    $created = $true
                    break
                } catch [Microsoft.PowerShell.Commands.InvalidPasswordException] {
                    Write-Warn "Password policy rejected '$candidate' for smbuser; trying next"
                }
            }
            if (-not $created) { Write-Warn "Could not create smbuser; share will rely on Everyone access" }
        }
        if (-not (Get-SmbShare -Name 'share' -ErrorAction SilentlyContinue)) {
            New-SmbShare -Name 'share' -Path $share -FullAccess 'Everyone' -CachingMode 'None' | Out-Null
        }
        # Make sure smbuser can write to the share filesystem.
        icacls $share /grant 'smbuser:(OI)(CI)F' /T | Out-Null
        return
    }
    docker run -d --name sambaserver -p 445:445 `
        -e USER='smbuser%smbpass' `
        -e SHARE='share;/share;yes;no;no;smbuser' `
        dperson/samba `
        -u 'smbuser;smbpass' `
        -s 'share;/share;yes;no;no;smbuser' | Out-Null
    Start-Sleep -Seconds 3
}
function Stop-HostSambaShare {
    if ($LegacyWindowsDeps) {
        Remove-SmbShare -Name 'share' -Force -ErrorAction SilentlyContinue
        return
    }
    docker rm -f sambaserver 2>$null | Out-Null
}

function Start-HostExchangeConnector {
    if ($LegacyWindowsDeps) {
        # WireMock standalone on :8889 (replaces warewolfserver/exchange-connector-testing,
        # which is a WireMock-based stub). Requires Java; the ES install path drops a
        # bundled JDK under C:\elasticsearch-*\jdk\bin\java.exe if -StartElasticsearchServer
        # ran. Falls back to system `java` on PATH.
        $wmVer = '3.9.1'
        $wmJar = "C:\wiremock-standalone-$wmVer.jar"
        if (-not (Test-Path $wmJar)) {
            Invoke-WebRequest -UseBasicParsing `
                -Uri "https://repo1.maven.org/maven2/org/wiremock/wiremock-standalone/$wmVer/wiremock-standalone-$wmVer.jar" `
                -OutFile $wmJar
        }
        $javaCmd = Get-Command java -ErrorAction SilentlyContinue
        $javaPath = if ($javaCmd) { $javaCmd.Path } else {
            $bundled = Get-ChildItem 'C:\elasticsearch-*\jdk\bin\java.exe' -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($bundled) { $bundled.FullName } else { $null }
        }
        if (-not $javaPath) { Write-Warn "java not found; cannot start WireMock"; return }
        $exchangeRoot = 'C:\exchange-connector-stub'
        if (-not (Test-Path $exchangeRoot)) { New-Item -ItemType Directory -Force -Path $exchangeRoot | Out-Null }
        $script:_exchangeProcess = Start-Process -FilePath $javaPath `
            -ArgumentList @('-jar', $wmJar, '--port', '8889', '--root-dir', $exchangeRoot, '--disable-banner') `
            -PassThru -WindowStyle Hidden
        Start-Sleep -Seconds 5
        return
    }
    docker run -d -p 8889:8080 --name exchange-connector-testing warewolfserver/exchange-connector-testing | Out-Null
    Start-Sleep -Seconds 5
}
function Stop-HostExchangeConnector {
    if ($LegacyWindowsDeps) {
        if ($script:_exchangeProcess -and -not $script:_exchangeProcess.HasExited) {
            $script:_exchangeProcess | Stop-Process -Force -ErrorAction SilentlyContinue
        }
        return
    }
    docker rm -f exchange-connector-testing 2>$null | Out-Null
}

function Start-HostMSSQLServer([string]$BakFile) {
    if ($LegacyWindowsDeps) {
        choco install sql-server-2022 -y
        [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.SqlWmiManagement") | Out-Null
        $wmi = New-Object Microsoft.SqlServer.Management.Smo.Wmi.ManagedComputer
        $comp = $env:ComputerName
        $Tcp = $wmi.GetSmoObject("ManagedComputer[@Name='$comp']/ServerInstance[@Name='MSSQLSERVER']/ServerProtocol[@Name='Tcp']")
        $Tcp.IsEnabled = $true; $Tcp.Alter()
        $Np = $wmi.GetSmoObject("ManagedComputer[@Name='$comp']/ServerInstance[@Name='MSSQLSERVER']/ServerProtocol[@Name='Np']")
        $Np.IsEnabled = $true; $Np.Alter()
        $sql = [Microsoft.SqlServer.Management.Smo.Server]::new("$comp")
        $sql.Settings.LoginMode = 'Mixed'; $sql.Alter()
        sqlcmd -S "localhost" -E -Q "CREATE LOGIN [testuser] WITH PASSWORD = 'test123', CHECK_POLICY = OFF"
        sqlcmd -S "localhost" -E -Q "SP_ADDSRVROLEMEMBER 'testuser','SYSADMIN'"
        if (!(Test-Path "C:\Builds")) { New-Item -ItemType Directory "C:\Builds" | Out-Null }
        sqlcmd -S "localhost" -E -Q "RESTORE DATABASE [Dev2TestingDB] FROM DISK='$BakFile' WITH MOVE 'Dev2TestingDB' TO 'C:\Builds\Dev2TestingDB.mdf', MOVE 'Dev2TestingDB_log' TO 'C:\Builds\Dev2TestingDB.ldf'"
        sqlcmd -S "localhost" -E -Q "USE Dev2TestingDB EXEC sp_change_users_login 'AUTO_FIX', 'testuser'"
        Get-Service -Name 'MSSQLSERVER' | Restart-Service -Force
        return
    }
    docker run -d --name sqlserver `
        -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Test123456!" -p 1433:1433 `
        mcr.microsoft.com/mssql/server:2019-latest | Out-Null
    Write-Host "Waiting for SQL Server..."
    for ($i = 1; $i -le 30; $i++) {
        docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Test123456!" -Q "SELECT 1" *> $null
        if ($LASTEXITCODE -eq 0) { Write-Host "SQL Server ready"; return }
        Start-Sleep -Seconds 3
    }
    Write-Warn "SQL Server did not become ready within 90s"
}
function Stop-HostMSSQLServer { docker rm -f sqlserver 2>$null | Out-Null }

# ============================================================================
# SUT lifecycle (Pattern A; bare-metal Windows process)
# ============================================================================

$script:_serverProcess   = $null
$script:_coverageProcess = $null
$script:_sessionId       = ""

function Resolve-FuncExe {
    if ($FuncExePath -and (Test-Path $FuncExePath)) { return $FuncExePath }
    # Prefer the real func.exe under the npm install dir. The npm-prefix
    # `func.cmd` shim doesn't survive dotnet-coverage instrumentation
    # (dotnet-coverage can only attach to .NET processes, not batch shims).
    $candidates = @(
        "$env:APPDATA\npm\node_modules\azure-functions-core-tools\bin\func.exe",
        "$env:ProgramFiles\Microsoft\Azure Functions Core Tools\func.exe",
        "func.exe",
        "$env:APPDATA\npm\func.cmd",
        "func"
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) { return $c }
        $cmd = Get-Command $c -ErrorAction SilentlyContinue
        if ($cmd) { return $cmd.Source }
    }
    throw "func.exe not found. Provide -FuncExePath or install azure-functions-core-tools@4."
}

function Wait-ForEngine {
    param([int]$Port = 7071, [int]$MaxSeconds = 120)
    Write-Host "Waiting for engine on port $Port (up to ${MaxSeconds}s)..."
    $deadline = (Get-Date).AddSeconds($MaxSeconds)
    while ((Get-Date) -lt $deadline) {
        try {
            $resp = Invoke-WebRequest -Uri "http://localhost:$Port/" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            if ($resp.StatusCode -ge 100 -and $resp.StatusCode -ne 503) {
                Write-Host "Engine ready (HTTP $($resp.StatusCode))."
                return
            }
        } catch { }
        Start-Sleep -Seconds 2
    }
    Write-Warn "Engine did not become ready within $MaxSeconds seconds."
}

function Start-LightweightExecution {
    $runDir = if ($LightweightExecutionDir) { $LightweightExecutionDir } else { "$PWD" }
    $func   = Resolve-FuncExe
    Write-Host "Starting Lightweight Execution from $runDir using $func"
    # Match pipeline.yml engine env: avoid Azure Key Vault lookups, force the
    # dotnet-isolated worker model, and use Development environment so that
    # dev-only features (e.g. X-WW-Bypass-Auth header) behave the same as CI.
    if (-not $env:AZURE_KEYVAULT_NAME)          { $env:AZURE_KEYVAULT_NAME = '' }
    if (-not $env:SkipFailureToRetrieveSecret)  { $env:SkipFailureToRetrieveSecret = 'true' }
    if (-not $env:FUNCTIONS_WORKER_RUNTIME)     { $env:FUNCTIONS_WORKER_RUNTIME = 'dotnet-isolated' }
    if (-not $env:ASPNETCORE_ENVIRONMENT)       { $env:ASPNETCORE_ENVIRONMENT = 'Development' }
    if (-not $env:AZURE_FUNCTIONS_ENVIRONMENT) { $env:AZURE_FUNCTIONS_ENVIRONMENT = 'Development' }
    # Use in-memory distributed lock manager so the engine works without Azurite.
    # CI agents that have a real storage account can override this by setting
    # AzureWebJobsStorage before invoking the script.
    if (-not $env:AzureWebJobsStorage)          { $env:AzureWebJobsStorage = '' }
    if ($SharedConfigDir) {
        # WAREWOLF_SECURE_CONFIG must point to a file path, not the dir.
        # Match the pipeline.yml convention: <SharedConfigDir>\secure.config.
        New-Item -ItemType Directory -Force -Path $SharedConfigDir | Out-Null
        $env:WAREWOLF_SECURE_CONFIG = Join-Path $SharedConfigDir 'secure.config'
    }
    # Workflow resolution: WorkflowHttpFunction reads from
    # <AppContext.BaseDirectory>\Resources\ by default, but the published
    # TestBinaries layout only has Examples\ workflows nested under siblings
    # like "Resources - ServerTests\Resources\Examples\". Probe for the one
    # that holds 'Examples\Control Flow - Decision.bite' (a known
    # security-spec target) and point the engine at it via $env:WorkflowsDirectory.
    if (-not $env:WorkflowsDirectory) {
        $candidates = @(
            (Join-Path $runDir 'Resources - ServerTests\Resources'),
            (Join-Path $runDir 'Resources - Release\Resources'),
            (Join-Path $runDir 'Resources')
        )
        foreach ($c in $candidates) {
            if (Test-Path (Join-Path $c 'Examples\Control Flow - Decision.bite')) {
                $env:WorkflowsDirectory = $c
                Write-Host "WorkflowsDirectory resolved to: $c"
                break
            }
        }
        if (-not $env:WorkflowsDirectory) {
            Write-Warn "WorkflowsDirectory probe found no Examples\Control Flow - Decision.bite under any of: $($candidates -join ', ')"
        }
    }
    # Merge integration-test tool workflows into WorkflowsDirectory.
    #
    # The published TestBinaries layout stores security-spec workflows under
    # "Resources - ServerTests\Resources" and integration-test workflows (e.g.
    # tools\http get\TC013*.bite) under the sibling "Resources" directory.  The
    # probe above picks "Resources - ServerTests\Resources" first (it contains
    # the security-spec sentinel), but the Azure Functions Integration Tests job
    # then fails because the tools\ workflows are absent from that directory.
    #
    # Fix: after resolving WorkflowsDirectory, scan every sibling Resources*
    # directory for a "tools\" subfolder that is missing from WorkflowsDirectory
    # and create a junction point for it.  Junctions are zero-copy and instant.
    # If junction creation fails (e.g. cross-volume), fall back to a recursive
    # file copy.  Either way, delete any stale workflow-index.json in
    # WorkflowsDirectory afterwards so the engine rebuilds the index from disk
    # and picks up the newly merged workflows.
    if ($env:WorkflowsDirectory) {
        $toolsTarget = Join-Path $env:WorkflowsDirectory 'tools'
        if (-not (Test-Path $toolsTarget)) {
            foreach ($sibling in @(Get-ChildItem -LiteralPath $runDir -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like 'Resources*' })) {
                $siblingTools = Join-Path $sibling.FullName 'tools'
                if (Test-Path $siblingTools) {
                    Write-Host "Merging tools\ from '$siblingTools' into WorkflowsDirectory via junction"
                    try {
                        & cmd.exe /c mklink /J "$toolsTarget" "$siblingTools" 2>&1 | Write-Host
                    } catch {
                        Write-Host "Junction failed, falling back to file copy: $_"
                        Copy-Item -LiteralPath $siblingTools -Destination $toolsTarget -Recurse -Force
                    }
                    if (Test-Path $toolsTarget) {
                        Write-Host "tools\ merged successfully into '$env:WorkflowsDirectory'"
                        # Remove stale index so the engine rescans and indexes the new workflows.
                        $staleIndex = Join-Path $env:WorkflowsDirectory 'workflow-index.json'
                        if (Test-Path $staleIndex) {
                            Remove-Item $staleIndex -Force
                            Write-Host "Removed stale workflow-index.json from WorkflowsDirectory"
                        }
                    }
                    break
                }
            }
        }
    }
    # Bump Azure Functions worker log level so /Secure/<slug> 500s leave their
    # exception text in warewolf-server.log instead of being swallowed.
    if (-not $env:AzureFunctionsJobHost__Logging__LogLevel__Default) {
        $env:AzureFunctionsJobHost__Logging__LogLevel__Default = 'Debug'
    }
    # Capture engine stdout/stderr to a log so a 500 from /Secure/<slug>
    # leaves a trail. PublishBuildArtifacts in pipeline.yml uploads
    # $TestResultsPath\warewolf-server.log when the test step finishes.
    $logBase = if ($TestResultsDir) { $TestResultsDir } else { Join-Path $PWD 'TestResults' }
    if (-not (Test-Path $logBase)) { New-Item -ItemType Directory -Force -Path $logBase | Out-Null }
    $stdoutLog = Join-Path $logBase 'warewolf-server.log'
    $stderrLog = Join-Path $logBase 'warewolf-server.err.log'
    # Diagnostic snapshot: record the resolved engine inputs to a separate
    # file (Start-Process -RedirectStandardOutput truncates $stdoutLog when
    # the process opens it, so we can't share that file with the header).
    # Both the task log (via Write-Host) and the artifact get a copy.
    $diagFile = Join-Path $logBase 'warewolf-engine-init.log'
    # List runDir top-level + any "Resources*" subfolders' immediate
    # children so we can see why the WorkflowsDirectory probe missed.
    # (Avoid PS7 ternary — Azure DevOps runs Windows PowerShell 5.1.)
    function Format-DirEntry($item) {
        if ($item.PSIsContainer) { return "[D] $($item.Name)" }
        return "[F] $($item.Name)"
    }
    $runDirTop = @(Get-ChildItem -LiteralPath $runDir -ErrorAction SilentlyContinue |
        Select-Object -First 60 |
        ForEach-Object { Format-DirEntry $_ })
    $resourcesProbe = @()
    foreach ($d in @(Get-ChildItem -LiteralPath $runDir -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like 'Resources*' })) {
        $resourcesProbe += "  $($d.Name)/"
        $resourcesProbe += @(Get-ChildItem -LiteralPath $d.FullName -ErrorAction SilentlyContinue |
            Select-Object -First 20 |
            ForEach-Object { "    " + (Format-DirEntry $_) })
    }
    # Drill into $env:WorkflowsDirectory specifically — the engine reads .bite
    # workflow files from there, and the SecuritySpecs jobs need
    # Examples\Control Flow - Decision.bite to exist. The top-level probe above
    # only shows immediate children of "Resources*", which stops one level
    # short of the workflow files.
    $wfProbe = @()
    if ($env:WorkflowsDirectory -and (Test-Path -LiteralPath $env:WorkflowsDirectory)) {
        $wfProbe += "WorkflowsDirectory contents (up to 40 entries, 2 levels deep):"
        $wfProbe += "  $($env:WorkflowsDirectory)/"
        $top = @(Get-ChildItem -LiteralPath $env:WorkflowsDirectory -ErrorAction SilentlyContinue | Select-Object -First 40)
        foreach ($t in $top) {
            $wfProbe += "    " + (Format-DirEntry $t)
            if ($t.PSIsContainer) {
                $wfProbe += @(Get-ChildItem -LiteralPath $t.FullName -ErrorAction SilentlyContinue |
                    Select-Object -First 20 |
                    ForEach-Object { "      " + (Format-DirEntry $_) })
            }
        }
        $biteCount = @(Get-ChildItem -LiteralPath $env:WorkflowsDirectory -Recurse -Filter '*.bite' -ErrorAction SilentlyContinue).Count
        $wfProbe += "Total *.bite files under WorkflowsDirectory (recursive): $biteCount"
    } else {
        $wfProbe += "WorkflowsDirectory not set or does not exist: '$($env:WorkflowsDirectory)'"
    }
    $diagLines = @(
        "=== TestRun.ps1 engine diagnostic snapshot ==="
        "Time          : $(Get-Date -Format o)"
        "RunDir        : $runDir"
        "Func          : $func"
        "WAREWOLF_SECURE_CONFIG : $($env:WAREWOLF_SECURE_CONFIG)"
        "WorkflowsDirectory     : $($env:WorkflowsDirectory)"
        "AzureFunctionsJobHost__Logging__LogLevel__Default : $($env:AzureFunctionsJobHost__Logging__LogLevel__Default)"
        ""
        "RunDir top-level (first 60):"
    ) + $runDirTop + @(
        ""
        "Resources* subfolder contents (first 20 each):"
    ) + $resourcesProbe + @(
        ""
    ) + $wfProbe + @(
        "==============================================="
    )
    $diagLines | ForEach-Object { Write-Host $_ }
    $diagLines | Out-File -FilePath $diagFile -Encoding utf8 -Force
    if ($CoverageDir) {
        $null = New-Item -Path $CoverageDir -ItemType Directory -Force
        $sid = if ($EngineSessionId) { $EngineSessionId } else { [guid]::NewGuid().ToString("N") }
        $script:_sessionId = $sid
        $outFile = if ($EngineCoverageFile) { $EngineCoverageFile } else { Join-Path $CoverageDir "engine.cobertura.xml" }
        $includeArgs = @(); foreach ($f in $CoverageIncludeFiles) { $includeArgs += @("--include-files", $f) }
        $collectArgs = @("collect", "--session-id", $sid, "--output", $outFile, "--output-format", "cobertura") + $includeArgs + @("--", $func, "start", "--port", "7071", "--verbose")
        Push-Location $runDir
        $script:_coverageProcess = Start-Process "dotnet-coverage" -ArgumentList $collectArgs -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdoutLog -RedirectStandardError $stderrLog
        Pop-Location
    } else {
        Push-Location $runDir
        $script:_serverProcess = Start-Process $func -ArgumentList @("start", "--port", "7071", "--verbose") -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdoutLog -RedirectStandardError $stderrLog
        Pop-Location
    }
    Wait-ForEngine -Port 7071 -MaxSeconds 180
}

function Start-WarewolfServer {
    $runDir    = if ($LightweightExecutionDir) { $LightweightExecutionDir } else { "$PWD" }
    $serverExe = Join-Path $runDir "Warewolf Server.exe"
    if (!(Test-Path $serverExe)) { throw "Warewolf Server.exe not found in $runDir" }
    Write-Host "Starting Warewolf Server from $runDir"
    if ($CoverageDir) {
        $null = New-Item -Path $CoverageDir -ItemType Directory -Force
        $sid = if ($EngineSessionId) { $EngineSessionId } else { [guid]::NewGuid().ToString("N") }
        $script:_sessionId = $sid
        $outFile = if ($EngineCoverageFile) { $EngineCoverageFile } else { Join-Path $CoverageDir "engine.cobertura.xml" }
        $includeArgs = @(); foreach ($f in $CoverageIncludeFiles) { $includeArgs += @("--include-files", $f) }
        $collectArgs = @("collect", "--session-id", $sid, "--output", $outFile, "--output-format", "cobertura") + $includeArgs + @("--", "`"$serverExe`"")
        Push-Location $runDir
        $script:_coverageProcess = Start-Process "dotnet-coverage" -ArgumentList $collectArgs -PassThru -WindowStyle Hidden
        Pop-Location
    } else {
        Push-Location $runDir
        $script:_serverProcess = Start-Process $serverExe -PassThru -WindowStyle Hidden
        Pop-Location
    }
    Wait-ForEngine -Port 3142
}

function Stop-Engine {
    if ($script:_sessionId) {
        Write-Host "Shutting down dotnet-coverage session $($script:_sessionId)..."
        # cmd /c swallows non-zero exit + stderr so a shutdown timeout
        # ("The operation has timed out.") doesn't bubble up under
        # ErrorActionPreference=Stop and fail the whole step after the
        # tests have already run and passed.
        cmd /c "dotnet-coverage shutdown $($script:_sessionId) >nul 2>nul"
        $global:LASTEXITCODE = 0
        Start-Sleep -Seconds 5
    }
    if ($script:_coverageProcess -and -not $script:_coverageProcess.HasExited) {
        $script:_coverageProcess | Stop-Process -Force -ErrorAction SilentlyContinue
    }
    if ($script:_serverProcess -and -not $script:_serverProcess.HasExited) {
        $script:_serverProcess | Stop-Process -Force -ErrorAction SilentlyContinue
    }
    Get-Process -Name "func"                           -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Get-Process -Name "Warewolf Server"                -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Get-Process -Name "Warewolf.Execution.Lightweight" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}

# ============================================================================
# Linux container helpers (Pattern B + -InContainer per-assembly mode)
# ============================================================================

function ConvertTo-ExcludeRegex([string[]]$Excludes) {
    if (-not $Excludes -or $Excludes.Count -eq 0) { return '' }
    $parts = foreach ($e in $Excludes) { ([regex]::Escape($e)) -replace '\\\*', '.*' }
    return '^(' + ($parts -join '|') + ')$'
}

function Ensure-CoverageImages {
    Write-Step "Building $InContainerImage Docker images..."
    if (-not (Test-Path $DockerfileTest)) { throw "Dockerfile.test not found at $DockerfileTest" }
    $dtfContent = Get-Content $DockerfileTest -Raw
    if ($dtfContent -match 'ENTRYPOINT \["/bin/bash"\]') {
        ($dtfContent -replace 'ENTRYPOINT \["/bin/bash"\]', 'ENTRYPOINT []') | Set-Content $DockerfileTest -Encoding UTF8
    }
    docker build -q -t warewolf-test-env -f $DockerfileTest $DockerContext | Out-Null
    Test-Exit 'Build warewolf-test-env'
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
"@ | docker build -q -t warewolf-coverage-env - | Out-Null
    Test-Exit 'Build warewolf-coverage-env'
    Write-Done 'Docker images ready'
}

# Linux unit-job runner (no engine; dotnet-coverage wrapping dotnet test).
function Invoke-LinuxUnitJob {
    param([Parameter(Mandatory)]$Job, [Parameter(Mandatory)][string]$CoverageOutDir)
    $slug = $Job.Slug
    $covDir = Join-Path $CoverageOutDir $slug
    New-Item -ItemType Directory -Force -Path $covDir | Out-Null

    if (-not $Job.Assembly -and $Job.ExcludeAssemblies.Count -eq 0) {
        Write-Warn "[$($Job.Name)] no Assembly or ExcludeAssemblies parsed - skipping"
        return
    }

    if ($Job.Assembly) {
        $filterArg = ''
        if (-not [string]::IsNullOrWhiteSpace($Job.Filter)) {
            $filterArg = if ($Job.Filter -match '^[A-Za-z0-9_]+$') { "--filter `"TestCategory=$($Job.Filter)`"" } else { "--filter `"$($Job.Filter)`"" }
        }
        $dll = "$($Job.Assembly).dll"
        $bash = @"
#!/bin/bash
set -e
echo "[$slug] $($Job.Assembly)"
dotnet-coverage collect --output /coverage/$slug.unit.cobertura.xml --output-format cobertura --settings /settings/coverage-settings.xml --nologo \
  -- /usr/share/dotnet/dotnet /tests/$dll $filterArg --results-directory /tmp/testresults --no-progress 2>/dev/null || true
[ -f /coverage/$slug.unit.cobertura.xml ] && echo "[$slug] done: `$(du -k /coverage/$slug.unit.cobertura.xml | cut -f1)KB" || echo '[$slug] WARNING: no coverage'
"@
    } else {
        $excludeRegex = ConvertTo-ExcludeRegex $Job.ExcludeAssemblies
        $unitFilter = $Job.Filter; if (-not $unitFilter) { $unitFilter = '' }
        $bash = @"
#!/bin/bash
set -e
exclude='$excludeRegex'; filter='$unitFilter'; i=0
for dll in /tests/*.dll; do
  [ -f "`$dll" ] || continue
  name=`$(basename "`$dll" .dll)
  echo "`$name" | grep -qE '^(Warewolf|Dev2)\..*(Tests|Specs)$' || continue
  if [ -n "`$exclude" ]; then echo "`$name" | grep -qE "`$exclude" && continue; fi
  echo "[$slug] `$name"
  timeout 300 dotnet-coverage collect --output "/coverage/parts_`${i}.cobertura.xml" --output-format cobertura --settings /settings/coverage-settings.xml --nologo \
    -- /usr/share/dotnet/dotnet "/tests/`${name}.dll" `$([ -n "`$filter" ] && echo "--filter `\"`$filter`\"") --results-directory /tmp/testresults --no-progress 2>/dev/null || true
  i=`$((i+1))
done
xmls=`$(ls /coverage/parts_*.cobertura.xml 2>/dev/null | tr '\n' ' ')
if [ -n "`$xmls" ]; then
  dotnet-coverage merge `$xmls --output /coverage/$slug.unit.cobertura.xml --output-format cobertura --nologo
  echo "[$slug] merged: `$(du -k /coverage/$slug.unit.cobertura.xml | cut -f1)KB"
else echo '[$slug] WARNING: no parts produced'
fi
"@
    }

    $args = @('run', '--rm', '--name', "ww-cov-unit-$slug-$RunId", '-e', 'DOTNET_ROOT=/usr/share/dotnet')
    $args += @('-v', "$(dp $ServerTestsBin):/tests:ro",
               '-v', "$(dp $covDir):/coverage",
               '-v', "$(dp $SettingsFile):/settings/coverage-settings.xml:ro",
               'warewolf-coverage-env', 'bash', '-c', $bash)
    & docker @args
    if ($LASTEXITCODE -ne 0) { Write-Warn "[$($Job.Name)] non-zero exit ($LASTEXITCODE)" }
}

# Linux engine-spec runner (per-job docker network + sidecars share net).
function Invoke-LinuxEngineSpecJob {
    param([Parameter(Mandatory)]$Job, [Parameter(Mandatory)][string]$CoverageOutDir)
    $slug          = $Job.Slug
    $netName       = "ww-cov-$slug-$RunId"
    $testContainer = "ww-cov-test-$slug-$RunId"
    $covDir        = Join-Path $CoverageOutDir $slug
    New-Item -ItemType Directory -Force -Path $covDir | Out-Null

    if (-not $Job.Output)    { Write-Warn "[$($Job.Name)] no Output filename - skipping"; return }
    if (-not $Job.SessionId) { Write-Warn "[$($Job.Name)] no SessionId - skipping"; return }
    if (-not $Job.Assembly -and $Job.ExcludeAssemblies.Count -eq 0) {
        Write-Warn "[$($Job.Name)] no Assembly or ExcludeAssemblies - skipping"; return
    }

    docker network create $netName 2>$null | Out-Null
    try {
        $envArgs   = @('-e', 'DOTNET_ROOT=/usr/share/dotnet')
        $mountArgs = @('-v', "$(dp $ServerTestsBin):/server:ro",
                       '-v', "$(dp $ServerTestsBin):/tests:ro",
                       '-v', "$(dp $covDir):/coverage",
                       '-v', "$(dp $SettingsFile):/settings/coverage-settings.xml:ro")
        if ($Job.Name -match 'Security') {
            New-Item -ItemType Directory -Force -Path (Join-Path $covDir 'security-config') | Out-Null
            $mountArgs += '-v', "$(dp (Join-Path $covDir 'security-config')):/security-config"
        }

        & docker run -d --name $testContainer --network $netName @envArgs @mountArgs warewolf-coverage-env sleep 3600 | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Failed to start test container' }

        foreach ($s in $Job.Sidecars) { Start-LinuxSidecar -Type $s -Slug $slug -RunId $RunId -TestContainer $testContainer }
        if ($Job.Sidecars.Count -gt 0) { Start-Sleep 8 }

        $filterStr = if ($Job.Filter) { "--filter '$($Job.Filter)'" } else { '' }
        $securitySetup = ''
        if ($Job.Name -match 'Security') {
            $securitySetup = @"
export WAREWOLF_SECURE_CONFIG=/security-config/secure.config
touch /security-config/secure.config
chmod 666 /security-config/secure.config
"@
        }
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

        $assemblyList = if ($Job.Assemblies -and $Job.Assemblies.Count -gt 1) { $Job.Assemblies } elseif ($Job.Assembly) { @($Job.Assembly) } else { @() }
        if ($assemblyList.Count -gt 0) {
            $loopBody = ($assemblyList | ForEach-Object { "run_test_dll '$_'" }) -join "`n"
            $testInvocation = "$runOneTest`necho '[$slug] Running $($assemblyList.Count) assembly(ies)'`n$loopBody"
        } else {
            $excludeRegex = ConvertTo-ExcludeRegex $Job.ExcludeAssemblies
            $testInvocation = @"
$runOneTest
echo "[$slug] discover-and-loop (excluding: $excludeRegex)"
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

        $bash = @"
#!/bin/bash
set -e
export AZURE_KEYVAULT_NAME=''
export SkipFailureToRetrieveSecret='true'
export ENABLECONSOLELOGGING='false'
export ENABLEELASTICSEARCHLOGGING='false'
export FUNCTIONS_WORKER_RUNTIME='dotnet-isolated'
$securitySetup
mkdir -p /server-rw && cp -a /server/. /server-rw/
dotnet-coverage instrument /server-rw/Warewolf.Execution.Lightweight.dll --session-id $($Job.SessionId) --nologo
dotnet-coverage collect --session-id $($Job.SessionId) --server-mode --background --output /coverage/$($Job.Output) --output-format cobertura --nologo
cd /server-rw
nohup func start --port 7071 > /tmp/func-engine.log 2>&1 &
ENGINE_PID=`$!
for i in `$(seq 1 60); do
  STATUS=`$(curl -s -o /dev/null -w '%{http_code}' --max-time 5 http://localhost:7071/ 2>/dev/null || echo 0)
  if [ "`$STATUS" -ge 100 ] 2>/dev/null && [ "`$STATUS" != '503' ]; then break; fi
  sleep 2
  if [ "`$i" -eq 60 ]; then
    tail -n 40 /tmp/func-engine.log 2>/dev/null || true
    kill `$ENGINE_PID 2>/dev/null || true
    dotnet-coverage shutdown $($Job.SessionId) 2>/dev/null || true
    exit 1
  fi
done
$testInvocation
dotnet-coverage shutdown $($Job.SessionId) 2>/dev/null || true
kill `$ENGINE_PID 2>/dev/null || true
pkill -TERM -f 'func start' 2>/dev/null || true
pkill -TERM -f 'Warewolf.Execution.Lightweight' 2>/dev/null || true
for i in `$(seq 1 30); do [ -f /coverage/$($Job.Output) ] && break; sleep 1; done
[ -f /coverage/$($Job.Output) ] && echo "[$slug] done: `$(du -k /coverage/$($Job.Output) | cut -f1)KB" || echo '[$slug] WARNING: no engine coverage'
"@
        & docker exec $testContainer bash -c $bash
        if ($LASTEXITCODE -ne 0) { Write-Warn "[$($Job.Name)] non-zero exit ($LASTEXITCODE)" }
    } finally {
        foreach ($s in $Job.Sidecars) { docker rm -f (Get-SidecarName $s $slug $RunId) 2>$null | Out-Null }
        docker rm -f $testContainer 2>$null | Out-Null
        docker network rm $netName 2>$null | Out-Null
    }
}

# Pattern A: bare-metal Windows job runner. Starts host-port sidecars, then
# self-invokes TestRun.ps1 in direct mode with the job's parameters.
function Invoke-WindowsBareMetalJob {
    param([Parameter(Mandatory)]$Job, [Parameter(Mandatory)][string]$CoverageOutDir)

    $slug = $Job.Slug
    $sidecarStarted = @()
    try {
        foreach ($s in $Job.Sidecars) {
            switch ($s) {
                'ftp'           { Start-HostFTPServer;          $sidecarStarted += 'ftp' }
                'sftp'          { Start-HostSFTPServer;         $sidecarStarted += 'sftp' }
                'sqlserver'     { Start-HostMSSQLServer "";     $sidecarStarted += 'sqlserver' }
                'elasticsearch' { Start-HostElasticsearchServer; $sidecarStarted += 'elasticsearch' }
                'rabbitmq'      { docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management | Out-Null; Start-Sleep 5; $sidecarStarted += 'rabbitmq' }
                'redis'         { docker run -d --name redis -p 6379:6379 redis:7-alpine | Out-Null; Start-Sleep 2; $sidecarStarted += 'redis' }
                'samba'         { docker run -d --name sambaserver -p 445:445 -e USER="smbuser%smbpass" -e SHARE="share;/share;yes;no;no;smbuser" dperson/samba -u "smbuser;smbpass" -s "share;/share;yes;no;no;smbuser" | Out-Null; Start-Sleep 3; $sidecarStarted += 'samba' }
                'exchange'      { docker run -d -p 8889:8080 --name exchange-connector-testing warewolfserver/exchange-connector-testing 2>$null | Out-Null; Start-Sleep 5; $sidecarStarted += 'exchange' }
                default         { Write-Warn "[$($Job.Name)] unknown sidecar '$s'" }
            }
        }

        # $args is an automatic variable in function scope; use a different name
        # so the splat at the call site below resolves to our hashtable.
        $splat = @{
            Projects        = $Job.Assemblies
            TestResultsDir  = Join-Path $CoverageOutDir "$slug\TestResults"
        }
        if ($Job.Filter) { $splat.Filter = $Job.Filter }
        if ($Job.Type -eq 'EngineSpec') {
            $splat.ServerType              = 'LightweightExecution'
            $splat.LightweightExecutionDir = $ServerTestsBin
            $splat.CoverageDir             = Join-Path $CoverageOutDir $slug
            if ($Job.Output)    { $splat.EngineCoverageFile = Join-Path $splat.CoverageDir $Job.Output }
            if ($Job.SessionId) { $splat.EngineSessionId    = $Job.SessionId }
            $splat.CoverageIncludeFiles    = @((Join-Path $ServerTestsBin 'Warewolf.Execution.Lightweight.dll'))
        } else {
            # Unit jobs: enable vstest's /EnableCodeCoverage and the Cobertura.xml
            # post-process. Without this, vstest produces TRX only, no .coverage
            # snapshots are emitted, and the merge step at the end of catalog mode
            # sees nothing from this job.
            $splat.Coverage = $true
        }
        if ($Job.Name -match 'Security') {
            $splat.SharedConfigDir = Join-Path $CoverageOutDir "$slug\security-config"
            New-Item -ItemType Directory -Force -Path $splat.SharedConfigDir | Out-Null
        }
        if ($Job.ExcludeAssemblies.Count -gt 0) { $splat.ExcludeProjects = $Job.ExcludeAssemblies }

        # Forward Windows-native dep flags (-StartFTPServer / -CreateUNCPath / etc.)
        # captured by ConvertFrom-PipelineYaml. These provision the same host-side
        # services the pipeline does and let local catalog-mode runs replicate
        # Microsoft-hosted agent failures bit-for-bit.
        foreach ($flag in $Job.WinDepFlags) { $splat[$flag] = $true }
        if ($null -ne $Job.MSSQLArg) { $splat.StartMSSQLServer = $Job.MSSQLArg }

        Push-Location $ServerTestsBin
        try {
            & "$PSScriptRoot\TestRun.ps1" @splat
        } finally {
            Pop-Location
        }

        # Unit-job Cobertura.xml -> <slug>.cobertura.xml so the catalog merge
        # picks it up. The pipeline.yml does the same rename step for CI; we
        # replicate it here so catalog mode produces the same shape.
        if ($Job.Type -ne 'EngineSpec') {
            $srcCob  = Join-Path $splat.TestResultsDir 'Cobertura.xml'
            $destDir = Join-Path $CoverageOutDir $slug
            $destCob = Join-Path $destDir "$slug.cobertura.xml"
            if (Test-Path $srcCob) {
                New-Item -ItemType Directory -Force -Path $destDir | Out-Null
                Move-Item $srcCob $destCob -Force
                Write-Done "[$($Job.Name)] -> $destCob"
            } else {
                Write-Warn "[$($Job.Name)] no Cobertura.xml produced at $srcCob"
            }
        }
    } finally {
        foreach ($s in $sidecarStarted) {
            switch ($s) {
                'ftp'           { Stop-HostFTPServer }
                'sftp'          { Stop-HostSFTPServer }
                'sqlserver'     { Stop-HostMSSQLServer }
                'elasticsearch' { Stop-HostElasticsearchServer }
                'rabbitmq'      { docker rm -f rabbitmq 2>$null | Out-Null }
                'redis'         { docker rm -f redis 2>$null | Out-Null }
                'samba'         { docker rm -f sambaserver 2>$null | Out-Null }
                'exchange'      { docker rm -f exchange-connector-testing 2>$null | Out-Null }
            }
        }
    }
}

# ============================================================================
# Catalog mode entry point
# ============================================================================

function Invoke-CatalogMode {
    if (-not (Test-Path $PipelineYml)) {
        Write-Error "pipeline.yml not found: $PipelineYml"
        exit 1
    }
    $catalog = ConvertFrom-PipelineYaml -YamlPath $PipelineYml -IncludeDisabled:$IncludeDisabled
    $enabledCount  = ($catalog | Where-Object { -not $_.Disabled }).Count
    $disabledCount = ($catalog | Where-Object { $_.Disabled }).Count
    if ($IncludeDisabled) {
        Write-Host "Parsed $($catalog.Count) jobs from pipeline.yml ($enabledCount enabled, $disabledCount disabled)"
    } else {
        Write-Host "Parsed $($catalog.Count) jobs from pipeline.yml"
    }

    if ($List) {
        $catalog | Sort-Object Name | Format-Table -AutoSize Name, Type, Assembly, Filter,
            @{n='Sidecars';e={$_.Sidecars -join ','}},
            @{n='WinDeps';e={(($_.WinDepFlags | ForEach-Object { $_ -replace '^Start','' -replace 'Server$','' }) -join ',')}},
            @{n='Disabled';e={if ($_.Disabled) {'*'} else {''}}}
        exit 0
    }

    # Resolve selection
    $selected = @()
    if (-not $Jobs -or $Jobs.Count -eq 0 -or $Jobs -contains 'All') {
        $selected = $catalog
    } else {
        foreach ($req in $Jobs) {
            $m = $catalog | Where-Object { $_.Name -eq $req }
            if ($m) { $selected += $m } else { Write-Warn "Unknown job '$req' (use -List to see available)" }
        }
    }
    if ($selected.Count -eq 0) { Write-Error "No jobs selected."; exit 1 }
    Write-Host "Selected $($selected.Count) job(s): $(($selected | ForEach-Object {$_.Name}) -join ', ')"

    # Auto-build if Bin\ServerTests is empty
    $haveBinaries = (Test-Path $ServerTestsBin) -and ((Get-ChildItem $ServerTestsBin -Filter "*.dll" -ErrorAction SilentlyContinue).Count -gt 0)
    if (-not $haveBinaries) {
        if ($SkipBuild) {
            Write-Error "Bin\ServerTests is empty and -SkipBuild was specified. Run Compile.ps1 first."
            exit 1
        }
        Write-Step 'Compiling solution-wide ServerTests output...'
        $runtime = if ($SUTRuntime -eq 'Linux') { 'linux-x64' } else { 'win-x64' }
        & $CompileScript -ServerTests -Runtime $runtime
        Test-Exit 'Compile.ps1'
    } else {
        Write-Done "Reusing existing Bin\ServerTests"
    }

    # Prereqs
    if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) {
        Write-Step 'Installing dotnet-coverage...'
        dotnet tool install --global dotnet-coverage
    }
    if ($SUTRuntime -eq 'Linux' -or $InContainer) {
        if (-not (docker info 2>$null)) { Write-Error 'Docker is not running.'; exit 1 }
        Ensure-CoverageImages
    }

    $CoverageOutDir = if ($CoverageDir) { [System.IO.Path]::GetFullPath($CoverageDir) } else { Join-Path $RepoRoot 'coverage' }
    New-Item -ItemType Directory -Force -Path $CoverageOutDir | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $CoverageOutDir 'merged') | Out-Null

    # Pattern-A parallel risk: bare-metal Windows shares host state. EngineSpec
    # jobs all bind the lightweight engine on port 7071; sidecars publish on
    # host ports (21/22/990/1433/...); WinDepFlags spin up pyftpdlib/OpenSSH/UNC
    # shares on fixed host ports. Any of these in two parallel jobs => collision.
    # Only pure Unit jobs with no deps are safe to fan out.
    $effectiveMaxParallel = $MaxParallel
    if ($NoParallel) { $effectiveMaxParallel = 1 }
    elseif ($SUTRuntime -eq 'Windows') {
        $unsafeJobs = $selected | Where-Object {
            $_.Type -eq 'EngineSpec' -or $_.Sidecars.Count -gt 0 -or $_.WinDepFlags.Count -gt 0 -or $null -ne $_.MSSQLArg
        }
        if ($unsafeJobs.Count -gt 0 -and $effectiveMaxParallel -gt 1) {
            Write-Warn "Bare-metal Windows with EngineSpec/sidecar/host-dep jobs: forcing sequential (port 7071 / FTP / UNC collision risk). Use -Runtime Linux for parallel."
            $effectiveMaxParallel = 1
        }
    }

    try {
        if ($effectiveMaxParallel -le 1) {
            foreach ($j in $selected) {
                Write-Step "[$($j.Name)] running ($($j.Type), $SUTRuntime)..."
                Invoke-JobByRuntime -Job $j -CoverageOutDir $CoverageOutDir
            }
        } else {
            # Parallel via Start-Job; only safe path is Linux-container jobs (each
            # gets its own docker network).
            $bgJobs = New-Object System.Collections.Generic.List[System.Management.Automation.Job]
            $queue  = [System.Collections.Queue]::new()
            foreach ($j in $selected) { $queue.Enqueue($j) | Out-Null }
            $scriptPath = $PSCommandPath
            while ($queue.Count -gt 0 -or $bgJobs.Count -gt 0) {
                while ($bgJobs.Count -lt $effectiveMaxParallel -and $queue.Count -gt 0) {
                    $j = $queue.Dequeue()
                    Write-Step "[$($j.Name)] queued (slot $($bgJobs.Count + 1)/$effectiveMaxParallel)"
                    $bg = Start-Job -Name "ww-$($j.Slug)" -ScriptBlock {
                        param($scriptPath, $jobName, $runtime, $covOut)
                        & $scriptPath -Jobs $jobName -SUTRuntime $runtime -CoverageDir $covOut -SkipBuild -SkipReport -NoParallel
                    } -ArgumentList $scriptPath, $j.Name, $SUTRuntime, $CoverageOutDir
                    $bgJobs.Add($bg) | Out-Null
                }
                $done = Wait-Job -Job $bgJobs -Any -Timeout 5
                if ($done) {
                    foreach ($d in @($done)) {
                        Write-Host "`n--- $($d.Name) ---" -ForegroundColor DarkGray
                        Receive-Job $d
                        if ($d.State -eq 'Failed') { Write-Warn "Job $($d.Name) failed" }
                        Remove-Job $d
                        $bgJobs.Remove($d) | Out-Null
                    }
                }
            }
        }

        # Merge
        Write-Step 'Merging coverage files...'
        $xmls = @(Get-ChildItem -Path $CoverageOutDir -Recurse -Filter '*.cobertura.xml' -File |
            Where-Object { $_.FullName -notmatch '\\merged\\' -and $_.Name -notlike 'parts_*.cobertura.xml' -and $_.Length -gt 200 } |
            ForEach-Object { $_.FullName })
        if ($xmls.Count -eq 0) {
            Write-Warn 'No coverage files - skipping merge and report'
        } else {
            $mergedXml = Join-Path $CoverageOutDir 'merged\all_merged.cobertura.xml'
            dotnet-coverage merge @xmls --output $mergedXml --output-format cobertura --nologo
            Test-Exit 'dotnet-coverage merge'
            Write-Done "Merged $($xmls.Count) file(s) -> $mergedXml"

            if (Test-Path $FilterScript) {
                $py = Get-Command python3 -ErrorAction SilentlyContinue
                if (-not $py) { $py = Get-Command python -ErrorAction SilentlyContinue }
                if ($py) { & $py.Source $FilterScript (Join-Path $CoverageOutDir 'merged') | Out-Null }
            }

            if (-not $SkipReport) {
                Write-Step "Generating $ReportFormat report..."
                if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
                    dotnet tool install --global dotnet-reportgenerator-globaltool
                }
                $reportDir = Join-Path $CoverageOutDir 'report'
                reportgenerator "-reports:$mergedXml" "-targetdir:$reportDir" "-reporttypes:$ReportFormat" '-title:Warewolf Coverage' '-verbosity:Warning'
                Test-Exit 'reportgenerator'
                $index = Join-Path $reportDir 'index.html'
                if ((Test-Path $index) -and -not $env:TF_BUILD) { Start-Process $index }
                Write-Done "Report: $index"
            }
        }

        Write-Host "`n[DONE] $($selected.Count) job(s) complete.`n" -ForegroundColor Green
    } finally {
        $leftover = docker ps -a --filter "name=ww-cov-.*-$RunId" --format '{{.Names}}' 2>$null
        if ($leftover) { $leftover -split "`n" | Where-Object { $_ } | ForEach-Object { docker rm -f $_ 2>$null | Out-Null } }
        $netLeft = docker network ls --filter "name=ww-cov-.*-$RunId" --format '{{.Name}}' 2>$null
        if ($netLeft) { $netLeft -split "`n" | Where-Object { $_ } | ForEach-Object { docker network rm $_ 2>$null | Out-Null } }
    }
}

function Invoke-JobByRuntime {
    param($Job, $CoverageOutDir)
    if ($SUTRuntime -eq 'Linux') {
        if ($Job.Type -eq 'EngineSpec') { Invoke-LinuxEngineSpecJob -Job $Job -CoverageOutDir $CoverageOutDir }
        else                            { Invoke-LinuxUnitJob       -Job $Job -CoverageOutDir $CoverageOutDir }
    } else {
        Invoke-WindowsBareMetalJob -Job $Job -CoverageOutDir $CoverageOutDir
    }
}

# ============================================================================
# Direct-mode helpers (TRX merge, retry resolution, container per-assembly run)
# ============================================================================

function Merge-RetryTrx {
    param([string]$TestResultsPath)
    [System.Collections.ArrayList]$xmlFiles = @(Get-ChildItem "$TestResultsPath\*.trx")
    if ($xmlFiles.Count -gt 1) {
        $maxCount = 0; $maxIdx = 0; $idx = 0
        foreach ($f in $xmlFiles) {
            $xml = [xml](Get-Content $f.FullName)
            if ([int]$xml.TestRun.ResultSummary.Counters.total -gt $maxCount) { $maxCount = $xml.TestRun.ResultSummary.Counters.total; $maxIdx = $idx }
            $idx++
        }
        $base = $xmlFiles[$maxIdx]
        $baseXml = [xml](Get-Content $base.FullName)
        $xmlFiles.RemoveAt($maxIdx)
        foreach ($f in $xmlFiles) {
            $xml = [xml](Get-Content $f.FullName)
            foreach ($retry in $xml.TestRun.Results.UnitTestResult) {
                foreach ($orig in $baseXml.TestRun.Results.UnitTestResult) {
                    if ($retry.testName -eq $orig.testName -and $retry.outcome -eq 'Passed' -and $orig.outcome -eq 'Failed') {
                        [void]$orig.ParentNode.AppendChild($baseXml.ImportNode($retry, $true))
                        $orig.ParentNode.ParentNode.ResultSummary.Counters.SetAttribute("passed", [int]$orig.ParentNode.ParentNode.ResultSummary.Counters.passed + 1)
                        $orig.ParentNode.ParentNode.ResultSummary.Counters.SetAttribute("failed", [int]$orig.ParentNode.ParentNode.ResultSummary.Counters.failed - 1)
                        [void]$orig.ParentNode.RemoveChild($orig)
                    }
                }
            }
            Remove-Item $f.FullName
        }
        $baseXml.Save($base.FullName)
        return ($baseXml.TestRun.ResultSummary.Counters.passed -eq $baseXml.TestRun.ResultSummary.Counters.executed)
    } else {
        $baseXml = [xml](Get-Content $xmlFiles[0].FullName)
        return ($baseXml.TestRun.ResultSummary.Counters.passed -eq $baseXml.TestRun.ResultSummary.Counters.executed)
    }
}

function Get-FailedTestNames {
    param([string]$TestResultsPath)
    $files = @(Get-ChildItem "$TestResultsPath\*.trx")
    if ($files.Count -eq 0) { return @() }
    $xml = [xml](Get-Content $files[0].FullName)
    return ($xml.TestRun.Results.UnitTestResult | Where-Object {$_.outcome -ne "Passed"}).testName
}

function Invoke-LinuxContainerRun {
    param([string[]]$Assemblies, [string]$Filter, [string[]]$Categories, [string[]]$ExcludeCategories)
    $effectiveBinDir = if ($BinDir) { $BinDir } else { $PWD }
    $resultsDir = (Resolve-Path $TestResultsPath).Path
    Ensure-CoverageImages

    foreach ($asm in $Assemblies) {
        $name = $asm.TrimEnd('.dll')
        $dllPath = Join-Path $effectiveBinDir "$name.dll"
        if (-not (Test-Path $dllPath)) { Write-Warn "Missing $dllPath"; continue }

        # MTP detection via deps.json
        $isMtp = $false
        $depsJson = Join-Path $effectiveBinDir "$name.deps.json"
        if (Test-Path $depsJson) {
            try {
                $deps = Get-Content $depsJson -Raw | ConvertFrom-Json
                if ($deps) {
                    $firstTarget = $deps.targets.PSObject.Properties | Select-Object -First 1
                    if ($firstTarget) {
                        $main = $firstTarget.Value.PSObject.Properties | Where-Object { $_.Name -like "$name/*" } | Select-Object -First 1
                        if ($main) { $isMtp = ($main.Value.dependencies.PSObject.Properties.Name -contains 'Microsoft.Testing.Platform') }
                    }
                }
            } catch { }
        }

        $networkArgs = if ($UseHostNetwork) { if ($IsLinux) { @('--network=host') } else { @('--add-host=localhost:host-gateway') } } else { @() }
        $covMount = @(); $covPrefix = @()
        if ($CoverageDir) {
            New-Item -ItemType Directory -Force -Path $CoverageDir | Out-Null
            $covMount = @('-v', "$(dp $CoverageDir):/coverage")
            $covPrefix = @('/root/.dotnet/tools/dotnet-coverage', 'collect', '--output', "/coverage/$name.cobertura.xml", '--output-format', 'cobertura', '--nologo')
            foreach ($f in $CoverageIncludeFiles) { $covPrefix += '--include-files', "/tests/$f" }
            $covPrefix += '--'
        }
        $shared = @()
        if ($SharedConfigDir) {
            New-Item -ItemType Directory -Force -Path $SharedConfigDir | Out-Null
            $shared = @('-v', "$(dp $SharedConfigDir):/shared-config", '-e', 'WAREWOLF_SECURE_CONFIG=/shared-config/secure.config')
        }
        $trxName = "$name.trx"
        if (Test-Path (Join-Path $resultsDir $trxName)) { Remove-Item (Join-Path $resultsDir $trxName) -Force }

        $args = @('run', '--rm') + $networkArgs + @('-e', 'DOTNET_ROOT=/usr/share/dotnet') + $covMount + $shared + @(
            '-v', "$(dp $effectiveBinDir):/tests:ro",
            '-v', "$(dp $resultsDir):/results",
            $InContainerImage
        ) + $covPrefix
        if ($isMtp) {
            $args += @('/usr/share/dotnet/dotnet', "/tests/$name.dll", '--report-trx', '--report-trx-filename', $trxName, '--results-directory', '/results', '--no-progress')
        } else {
            $args += @('/usr/share/dotnet/dotnet', 'test', "/tests/$name.dll", '--logger', "trx;LogFileName=$trxName", '--results-directory', '/results')
        }
        if ($Filter) { $args += '--filter', $Filter }

        Write-Host "+ docker $($args -join ' ')" -ForegroundColor DarkGray
        & docker @args
        $exit = $LASTEXITCODE
        if ($exit -eq 8) {
            Write-Warn "$name - zero tests ran (exit 8); treating as warning"
        } elseif ($exit -ne 0) {
            Write-Warn "$name failed (exit $exit)"
        }
    }
}

# ============================================================================
# Mode router
# ============================================================================

$Projects          = Split-CommaArray $Projects
$Assemblies        = Split-CommaArray $Assemblies
$ExcludeProjects   = Split-CommaArray $ExcludeProjects
$ExcludeAssemblies = Split-CommaArray $ExcludeAssemblies
$ExcludeCategories = Split-CommaArray $ExcludeCategories
$Categories        = Split-CommaArray $Categories

# Unify Projects/Assemblies into Projects (everything below uses $Projects).
if ($Assemblies.Count -gt 0 -and $Projects.Count -eq 0) { $Projects = $Assemblies }
if ($ExcludeAssemblies.Count -gt 0 -and $ExcludeProjects.Count -eq 0) { $ExcludeProjects = $ExcludeAssemblies }

$hasAnyDepFlag = $StartFTPServer.IsPresent -or $StartFTPSServer.IsPresent -or `
                 $StartSFTPServer.IsPresent -or $StartSambaShare.IsPresent -or `
                 $StartMySQLServer.IsPresent -or $StartElasticsearchServer.IsPresent -or `
                 $StartRabbitMQServer.IsPresent -or $StartRedisServer.IsPresent -or `
                 $StartExchangeConnector.IsPresent -or ($StartMSSQLServer -ne "")

# Any signal that the caller meant "run this specific selection in direct mode"
# routes around catalog scanning. -ExcludeProjects alone (e.g. Unit_Tests job)
# counts even when -Projects is absent.
$hasDirectModeSelection = ($Projects.Count -gt 0) -or `
                          ($ExcludeProjects.Count -gt 0) -or `
                          ($TestsToRun -ne "") -or `
                          $hasAnyDepFlag -or $InContainer.IsPresent

$catalogMode = $List.IsPresent -or `
               ($Jobs.Count -gt 0) -or `
               (-not $hasDirectModeSelection)

# -Runtime Linux in direct mode means "run tests in a Linux container" — set
# -InContainer unless the caller explicitly opted out.
if (-not $catalogMode -and $Runtime -eq 'Linux' -and -not $PSBoundParameters.ContainsKey('InContainer')) {
    $InContainer = [switch]::Present
}

if ($catalogMode) {
    Invoke-CatalogMode
    exit 0
}

# ============================================================================
# Direct mode (ported from previous TestRun.ps1, with small refinements)
# ============================================================================

if ($PreTestRunScript -and $Coverage.IsPresent -and -not $PreTestRunScript.Contains("-Coverage"))   { $PreTestRunScript += " -Coverage" }
if ($PostTestRunScript -and $Coverage.IsPresent -and -not $PostTestRunScript.Contains("-Coverage")) { $PostTestRunScript += " -Coverage" }

$TestResultsPath = if ($TestResultsDir) { $TestResultsDir } else { ".\TestResults" }
if (Test-Path $TestResultsPath) { Remove-Item -Force -Recurse $TestResultsPath }
New-Item -ItemType Directory $TestResultsPath -ErrorAction SilentlyContinue | Out-Null

# vstest resolution
if (-not $VSTestPath -or -not (Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
    $VSTestPath = ".\Microsoft.TestPlatform\tools\net462\common7\ide"
}
if (-not (Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
    if (-not $NuGet -or -not (Test-Path $NuGet)) {
        $NuGetCommand = Get-Command NuGet -ErrorAction SilentlyContinue
        if ($NuGetCommand) { $NuGet = $NuGetCommand.Path }
    }
    if ((-not $NuGet -or -not (Test-Path $NuGet)) -and (Test-Path $env:windir)) {
        Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "$env:windir\nuget.exe"
        $NuGet = "$env:windir\nuget.exe"
    }
    if ($Projects.Count -gt 0 -and -not (Test-Path "$VSTestPath\Extensions\TestPlatform\vstest.console.exe")) {
        & $NuGet install Microsoft.TestPlatform -ExcludeVersion -NonInteractive -OutputDirectory .
    }
}

if ($Coverage.IsPresent) {
    if (Test-Path "$TestResultsPath\Merged.coveragexml") { Remove-Item "$TestResultsPath\Merged.coveragexml" }
    if (Test-Path "$TestResultsPath\Cobertura.xml")      { Remove-Item "$TestResultsPath\Cobertura.xml" }
    $CoverageConfigPath = ".\Microsoft.TestPlatform\tools\net462\Team Tools\Dynamic Code Coverage Tools\CodeCoverage.config"
    if (Test-Path $CoverageConfigPath) {
        (Get-Content $CoverageConfigPath).Replace('<UseVerifiableInstrumentation>true</UseVerifiableInstrumentation>',
                                                  '<UseVerifiableInstrumentation>false</UseVerifiableInstrumentation>') | Set-Content $CoverageConfigPath
    }
}

# Legacy Windows-only host setup (gated behind -LegacyWindowsDeps)
if ($LegacyWindowsDeps) {
    if ($CreateLocalSchedulerAdmin) {
        cmd /c NET user "LocalSchedulerAdmin" "987Sched#@!" /ADD /Y
        Add-LocalGroupMember -Group 'Administrators'          -Member 'LocalSchedulerAdmin' -ErrorAction SilentlyContinue
        Add-LocalGroupMember -Group 'Warewolf Administrators' -Member 'LocalSchedulerAdmin' -ErrorAction SilentlyContinue
    }
    if ($CreateUNCPath) {
        # Subdirs the File/Folder spec outlines reference under
        # \\localhost\FileSystemShareTestingSite. Source files inside Copy/Move/
        # Rename/Delete dirs are written at runtime by CommonSteps'
        # CreateSourceFileWithSomeDummyData (path = literal feature value +
        # AddGuidToPath suffix), but the *parent dir* must exist beforehand or
        # the PutRaw fails and the test reports Failure.
        $shareRoot = 'C:\FileSystemShareTestingSite'
        foreach ($sub in @(
            'ReadFileSharedTestingSite',
            'ReadFolderSharedTestingSite',
            'ReadFolderSharedTestingSite\emptydir',
            'FileCopySharedTestingSite',
            'FileMoveSharedTestingSite',
            'FileRenameSharedTestingSite',
            'FileCreateSharedTestingSite',
            'FileDeleteSharedTestingSite',
            'FileZipSharedTestingSite'
        )) {
            mkdir (Join-Path $shareRoot $sub) -Force | Out-Null
        }
        "file contents to read" | Out-File -LiteralPath "$shareRoot\ReadFileSharedTestingSite\filetoread.txt" -Encoding utf8 -Force
        # Delete-from-UNC reads pre-existing files (not created at runtime).
        'delete me'  | Out-File -LiteralPath "$shareRoot\FileDeleteSharedTestingSite\filetodelete.txt" -Encoding ascii -Force
        'memo body'  | Out-File -LiteralPath "$shareRoot\FileDeleteSharedTestingSite\Memo.txt" -Encoding ascii -Force
        New-SmbShare -Path $shareRoot -FullAccess Everyone -Name FileSystemShareTestingSite -ErrorAction SilentlyContinue
    }
    if ($UseRegionalSettings) {
        $culture = [System.Globalization.CultureInfo]::CreateSpecificCulture("en-ZA")
        [System.Globalization.CultureInfo]::DefaultThreadCurrentCulture   = $culture
        [System.Globalization.CultureInfo]::DefaultThreadCurrentUICulture = $culture
        [System.Threading.Thread]::CurrentThread.CurrentCulture   = $culture
        [System.Threading.Thread]::CurrentThread.CurrentUICulture = $culture
        # PS 5.1 exposed Microsoft.PowerShell.NativeCultureResolver with private static
        # m_uiCulture/m_culture fields that had to be poked via reflection to make the
        # running session pick up the new culture without a restart. The type/fields
        # are gone in PS 7+, so treat the reflection path as best-effort.
        $assembly = [System.Reflection.Assembly]::Load("System.Management.Automation")
        $type = $assembly.GetType("Microsoft.PowerShell.NativeCultureResolver")
        if ($null -ne $type) {
            $flags = [Reflection.BindingFlags]::NonPublic -bor [Reflection.BindingFlags]::Static
            foreach ($name in 'm_uiCulture','m_culture') {
                $field = $type.GetField($name, $flags)
                if ($null -ne $field) { $field.SetValue($null, $culture) }
            }
        }
        Set-Culture en-ZA
        Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sTimeFormat -Value 'hh:mm:ss tt' } }
        Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sShortTime -Value 'hh:mm tt' } }
        Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sLongDate -Value 'dddd, dd MMMM yyyy' } }
        Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sShortDate -Value 'yyyy/MM/dd' } }
        Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name sDecimal -Value '.' } }
        Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name s1159 -Value 'AM' } }
        Get-ChildItem -Path 'Microsoft.PowerShell.Core\Registry::HKEY_USERS' | % { $SubKeyName = $_.Name;if (!($SubKeyName.EndsWith('-500_Classes'))) { Set-ItemProperty -Path "Microsoft.PowerShell.Core\Registry::$SubKeyName\Control Panel\International" -Name s2359 -Value 'PM' } }
        Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sTimeFormat -Value 'hh:mm:ss tt'
        Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sShortTime -Value 'hh:mm tt'
        Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sLongDate -Value 'dddd, dd MMMM yyyy'
        Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sShortDate -Value 'yyyy/MM/dd'
        Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name sDecimal -Value '.'
        Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name s1159 -Value 'AM'
        Set-ItemProperty -Path 'Microsoft.PowerShell.Core\Registry::HKEY_CURRENT_USER\Control Panel\International' -Name s2359 -Value 'PM'
    }
}

if ($STA.IsPresent -or $Sequential.IsPresent) {
    # -Sequential forces a single MSTest worker, overriding the assembly-level
    # [Parallelize(Workers=0)] (= ProcessorCount). Used by the Not Parallelizable
    # Unit Tests job because [DoNotParallelize] does not reliably isolate tests
    # that mutate static state (ResourceCatalog.Instance, GlobalConstants, etc.).
    $rcBlock = if ($STA.IsPresent) {
@"
  <RunConfiguration>
    <ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState>
  </RunConfiguration>

"@
    } else { "" }
    $msBlock = if ($Sequential.IsPresent) {
@"
  <MSTest>
    <Parallelize>
      <Workers>1</Workers>
    </Parallelize>
  </MSTest>

"@
    } else { "" }
@"
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
$rcBlock$msBlock</RunSettings>
"@ | Out-File -LiteralPath "$TestResultsPath\vstest.runsettings" -Encoding utf8 -Force
}

# SUT start
if ($ServerType -eq 'LightweightExecution') { Start-LightweightExecution }
elseif ($ServerType -eq 'FullServer')       { Start-WarewolfServer }

try {
    if ($Projects.Count -gt 0) {
        for ($loop = 0; $loop -le $RetryCount; $loop++) {
            if ($StartFTPServer.IsPresent)           { Start-HostFTPServer }
            if ($StartFTPSServer.IsPresent)          { Start-HostFTPSServer }
            if ($StartSFTPServer.IsPresent)          { Start-HostSFTPServer }
            if ($StartSambaShare.IsPresent)          { Start-HostSambaShare }
            if ($StartMySQLServer.IsPresent)         { Start-HostMySQLServer }
            if ($StartElasticsearchServer.IsPresent) { Start-HostElasticsearchServer }
            if ($StartRabbitMQServer.IsPresent)      { Start-HostRabbitMQServer }
            if ($StartRedisServer.IsPresent)         { Start-HostRedisServer }
            if ($StartExchangeConnector.IsPresent)   { Start-HostExchangeConnector }
            if ($StartMSSQLServer)                   { Start-HostMSSQLServer $StartMSSQLServer }

            if ($RetryRebuild.IsPresent) {
                if (Test-Path "$PWD\..\..\Compile.ps1") {
                    & "$PWD\..\..\Compile.ps1" -AcceptanceTesting -NuGet $NuGet -MSBuildPath $MSBuildPath
                } elseif (Test-Path "$PWD\Compile.ps1") {
                    & "$PWD\Compile.ps1" -AcceptanceTesting -NuGet $NuGet -MSBuildPath $MSBuildPath
                    Set-Location "$PWD\bin\AcceptanceTesting"
                }
            }

            # Resolve assemblies in $PWD
            $allAsm = @()
            foreach ($p in $Projects) { $allAsm += @(Get-ChildItem ".\$p.dll" -Recurse -ErrorAction SilentlyContinue) }
            if ($allAsm.Count -eq 0) {
                Write-Error "Could not find any assemblies matching: $($Projects -join ',')"
                exit 1
            }
            $asmList = @()
            foreach ($a in $allAsm) {
                if ([array]::indexof($ExcludeProjects, $a.Name.TrimEnd(".dll")) -eq -1) { $asmList += $a.Name }
            }
            if (Test-Path "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx") {
                Remove-Item "$VSTestPath\Extensions\TestPlatform\TestResults" -Force -Recurse
            }
            # Roll prior RunTests / coverage artifacts. warewolf-server.log is
            # held open by the engine for the full TestRun.ps1 invocation, so
            # rotating it here would fail with a file-in-use error; let it
            # accumulate across retries instead.
            if (Test-Path "$TestResultsPath\RunTests.ps1")          { Move-Item "$TestResultsPath\RunTests.ps1"          "$TestResultsPath\RunTests($loop).ps1" }
            if (Test-Path "$TestResultsPath\Snapshot.coverage")     { Move-Item "$TestResultsPath\Snapshot.coverage"     "$TestResultsPath\Snapshot($loop).coverage" }
            if (Test-Path "$TestResultsPath\Snapshot_Backup.coverage") { Move-Item "$TestResultsPath\Snapshot_Backup.coverage" "$TestResultsPath\Snapshot_Backup($loop).coverage" }

            $asmArg = ".\" + ($asmList -join " .\")
            if ($UNCPassword) {
                "net use \\localhost\FileSystemShareTestingSite /user:Administrator $UNCPassword" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
            }
            $settingsArg = if ($STA.IsPresent -or $Sequential.IsPresent) { "--settings:`"$TestResultsPath\vstest.runsettings`"" } else { "" }
            # Pin vstest's results directory so TRX lands at $TestResultsPath regardless of CWD.
            $resultsDirArg = "/ResultsDirectory:`"$TestResultsPath`""

            # Build vstest invocation
            $vstestExe = "$VSTestPath\Extensions\TestPlatform\vstest.console.exe"
            if ($TestsToRun) {
                if ($PreTestRunScript) { "&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append }
                $covArg = if ($Coverage.IsPresent -and -not $PreTestRunScript) { "/EnableCodeCoverage" } else { "" }
                "&`"$vstestExe`" /logger:trx $asmArg /Tests:`"$TestsToRun`" $settingsArg $covArg $resultsDirArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
            } else {
                $categoryArg = ""
                if ($ExcludeCategories -and $ExcludeCategories.Count -gt 0) {
                    $categoryArg = "/TestCaseFilter:`"(TestCategory!=" + ($ExcludeCategories -join ")&(TestCategory!=") + ")`""
                } elseif ($Category) {
                    $categoryArg = "/TestCaseFilter:`"(TestCategory=$Category)`""
                } elseif ($Categories -and $Categories.Count -gt 0) {
                    $categoryArg = "/TestCaseFilter:`"(TestCategory=" + ($Categories -join ")|(TestCategory=") + ")`""
                }
                if ($Filter) {
                    if ($categoryArg) {
                        $existing = $categoryArg -replace '^/TestCaseFilter:"(.+)"$', '$1'
                        $categoryArg = "/TestCaseFilter:`"($existing)&($Filter)`""
                    } else {
                        $categoryArg = "/TestCaseFilter:`"$Filter`""
                    }
                }
                if ($PreTestRunScript) { "&.\$PreTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append }
                $covArg = if ($Coverage.IsPresent -and -not $PreTestRunScript) { "/EnableCodeCoverage" } else { "" }
                "&`"$vstestExe`" /logger:trx $asmArg $categoryArg $settingsArg $covArg $resultsDirArg" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
            }
            if ($PostTestRunScript) { "&.\$PostTestRunScript" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append }
            if ($UNCPassword) {
                "net use \\localhost\FileSystemShareTestingSite /delete" | Out-File "$TestResultsPath\RunTests.ps1" -Encoding ascii -Append
            }

            Get-Content "$TestResultsPath\RunTests.ps1"
            if (-not $InContainer.IsPresent -and $InContainerCommitID -eq "latest" -and $InContainerVersion -eq "latest") {
                & "$TestResultsPath\RunTests.ps1"
            } else {
                # Linux container per-assembly run (ported from run-tests-in-container.ps1)
                Invoke-LinuxContainerRun -Assemblies $asmList -Filter $Filter -Categories $Categories -ExcludeCategories $ExcludeCategories
            }
            if (Test-Path "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx") {
                Copy-Item "$VSTestPath\Extensions\TestPlatform\TestResults\*.trx" "$TestResultsPath" -Force -Recurse
            }

            # Merge TRX retry results, decide whether to loop
            if (-not (Test-Path "$TestResultsPath\*.trx")) {
                Write-Error "No test results found."
                exit 1
            }
            $break = Merge-RetryTrx -TestResultsPath $TestResultsPath
            if ($break) { break }
            $TestsToRun = (Get-FailedTestNames -TestResultsPath $TestResultsPath) -join ","

            if ($StartFTPServer.IsPresent -or $StartFTPSServer.IsPresent) { Stop-HostFTPServer; Stop-HostFTPSServer }
            if ($StartSFTPServer.IsPresent)          { Stop-HostSFTPServer }
            if ($StartSambaShare.IsPresent)          { Stop-HostSambaShare }
            if ($StartMySQLServer.IsPresent)         { Stop-HostMySQLServer }
            if ($StartElasticsearchServer.IsPresent) { Stop-HostElasticsearchServer }
            if ($StartRabbitMQServer.IsPresent)      { Stop-HostRabbitMQServer }
            if ($StartRedisServer.IsPresent)         { Stop-HostRedisServer }
            if ($StartExchangeConnector.IsPresent)   { Stop-HostExchangeConnector }
        }
    } else {
        # No projects: only dependency startup was requested.
        if ($StartFTPServer.IsPresent)           { Start-HostFTPServer }
        if ($StartFTPSServer.IsPresent)          { Start-HostFTPSServer }
        if ($StartSFTPServer.IsPresent)          { Start-HostSFTPServer }
        if ($StartSambaShare.IsPresent)          { Start-HostSambaShare }
        if ($StartMySQLServer.IsPresent)         { Start-HostMySQLServer }
        if ($StartElasticsearchServer.IsPresent) { Start-HostElasticsearchServer }
        if ($StartRabbitMQServer.IsPresent)      { Start-HostRabbitMQServer }
        if ($StartRedisServer.IsPresent)         { Start-HostRedisServer }
        if ($StartExchangeConnector.IsPresent)   { Start-HostExchangeConnector }
        if ($StartMSSQLServer)                   { Start-HostMSSQLServer $StartMSSQLServer }
    }
} finally {
    if ($ServerType) { Stop-Engine }
}

# Convert vstest's per-run .coverage snapshots into Cobertura.xml. CodeCoverage.exe
# in VS 17.x prints a deprecation banner and refuses to merge, so we use
# dotnet-coverage which emits cobertura directly (replacing the old
# CodeCoverage.exe merge + reportgenerator pipeline).
if ($Coverage.IsPresent) {
    $snaps = @(Get-ChildItem $TestResultsPath -Recurse -Filter '*.coverage' -File -ErrorAction SilentlyContinue)
    if ($snaps.Count -gt 0) {
        if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) {
            dotnet tool install --global dotnet-coverage --ignore-failed-sources 2>&1 | Write-Host
        }
        $cobertura = "$TestResultsPath\Cobertura.xml"
        $mergeArgs = @('merge') + ($snaps | ForEach-Object { $_.FullName }) + @('--output', $cobertura, '--output-format', 'cobertura', '--nologo')
        & dotnet-coverage @mergeArgs
        if ($LASTEXITCODE -ne 0) { Write-Warn "dotnet-coverage merge exited $LASTEXITCODE" }
    } else {
        Write-Warn "No .coverage snapshots found under $TestResultsPath; skipping Cobertura conversion"
    }
}

exit 0
