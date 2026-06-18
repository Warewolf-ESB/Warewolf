<#
.SYNOPSIS
Pre-compile security gates for the Warewolf lightweight server.

.DESCRIPTION
Three fail-fast checks invoked by Compile.ps1 before any solution is built:

  1. NuGet vulnerability scan (dotnet list)
     Runs 'dotnet list package --vulnerable --include-transitive --format json'
     against each in-scope project and exits 1 if any Critical/High CVE is
     reachable (top-level or transitive).

  2. NuGet vulnerability scan (NuGetAudit)
     Runs 'dotnet restore' with /p:NuGetAudit=true /p:NuGetAuditMode=all so
     NuGet's own audit feature emits NU1901..NU1904 warnings against the
     advisory feed. Exits 1 if any high (NU1903) or critical (NU1904)
     warnings are emitted. This complements the dotnet list scan because
     NuGetAudit runs as part of dependency resolution and can catch issues
     the post-restore lister misses (e.g. when transitive graph evaluation
     differs).

  3. .NET runtime end-of-support window
     Reads <TargetFramework> from each in-scope project, looks the moniker up
     in Build\dotnet-support.json, and exits 1 if the EOS date is past or
     within -WarnDays days of today.

In-scope projects: Warewolf.Execution.Lightweight (the lightweight server) and
every project in its recursive <ProjectReference> closure (47 projects total,
including the F# Warewolf.Language.Parser leaf). The list is generated from
walking the .csproj/.fsproj graph rooted at Warewolf.Execution.Lightweight and
must be regenerated when new project references are added to that closure.

The script also writes Bin\security-report.md when a vulnerability gate fails
so CI can attach it as a build artifact.

.PARAMETER RepoRoot
Path to the repo root (the directory containing Compile.ps1 and Dev\). Defaults
to the grandparent of this script's directory (Dev\.azure\ -> repo root).

.PARAMETER SkipVulnerabilityCheck
Bypass both vulnerability gates (dotnet list and NuGetAudit). Emits a loud
warning so the bypass is recorded.

.PARAMETER SkipEosCheck
Bypass the .NET end-of-support gate. Emits a loud warning so the bypass is recorded.

.PARAMETER WarnDays
Threshold (in days) before the EOS date at which the build starts failing.
Defaults to 90 (3 months).
#>
Param(
    [string]$RepoRoot = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    [switch]$SkipVulnerabilityCheck,
    [switch]$SkipEosCheck,
    [int]$WarnDays = 90
)

function Test-VulnerablePackages {
    param(
        [string[]]$ProjectFiles,
        [string[]]$FailOnSeverity = @('Critical','High'),
        [string]$ReportPath
    )
    Write-Host ""
    Write-Host "================================================================"
    Write-Host " Security gate: NuGet vulnerability scan (dotnet list)"
    Write-Host "================================================================"
    $findings = @()
    foreach ($projectFile in $ProjectFiles) {
        if (-not (Test-Path $projectFile)) {
            Write-Host "  Project not found, skipping: $projectFile"
            continue
        }
        Write-Host "  Restoring $projectFile..."
        & dotnet restore $projectFile --nologo --verbosity quiet | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ERROR: dotnet restore failed for $projectFile" -ForegroundColor Red
            exit 1
        }
        Write-Host "  Scanning $projectFile..."
        $rawJson = & dotnet list $projectFile package --vulnerable --include-transitive --format json 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ERROR: vulnerability scan failed for $projectFile" -ForegroundColor Red
            Write-Host ($rawJson -join "`n")
            exit 1
        }
        $jsonText = ($rawJson -join "`n")
        $startIdx = $jsonText.IndexOf('{')
        if ($startIdx -lt 0) { continue }
        try {
            $report = $jsonText.Substring($startIdx) | ConvertFrom-Json
        } catch {
            Write-Host "  WARN: could not parse vulnerability JSON for $projectFile"
            continue
        }
        foreach ($proj in @($report.projects)) {
            foreach ($fw in @($proj.frameworks)) {
                foreach ($pkg in @($fw.topLevelPackages)) {
                    foreach ($v in @($pkg.vulnerabilities)) {
                        if ($FailOnSeverity -contains $v.severity) {
                            $findings += [pscustomobject]@{
                                Project  = (Split-Path $projectFile -Leaf)
                                Source   = 'dotnet list'
                                Kind     = 'Top-level'
                                Package  = $pkg.id
                                Version  = $pkg.resolvedVersion
                                Severity = $v.severity
                                Advisory = $v.advisoryurl
                            }
                        }
                    }
                }
                foreach ($pkg in @($fw.transitivePackages)) {
                    foreach ($v in @($pkg.vulnerabilities)) {
                        if ($FailOnSeverity -contains $v.severity) {
                            $findings += [pscustomobject]@{
                                Project  = (Split-Path $projectFile -Leaf)
                                Source   = 'dotnet list'
                                Kind     = 'Transitive'
                                Package  = $pkg.id
                                Version  = $pkg.resolvedVersion
                                Severity = $v.severity
                                Advisory = $v.advisoryurl
                            }
                        }
                    }
                }
            }
        }
    }
    if ($findings.Count -eq 0) {
        Write-Host "  OK - no Critical/High vulnerabilities detected by dotnet list."
        return
    }
    Write-VulnerabilityReport -Findings $findings -ReportPath $ReportPath -Heading "Vulnerable NuGet packages detected (dotnet list)"
    exit 1
}

function Test-NuGetAuditPackages {
    param(
        [string[]]$ProjectFiles,
        [string[]]$FailOnSeverity = @('Critical','High'),
        [string]$ReportPath
    )
    Write-Host ""
    Write-Host "================================================================"
    Write-Host " Security gate: NuGet vulnerability scan (NuGetAudit)"
    Write-Host "================================================================"
    # NuGetAudit emits one warning per advisory during restore:
    #   NU1901 = low, NU1902 = moderate, NU1903 = high, NU1904 = critical
    # We force NuGetAuditLevel=low so all four warning codes are emitted, then
    # filter by $FailOnSeverity here so the gate's severity policy stays in
    # one place.
    $severityByCode = @{
        'NU1901' = 'Low'
        'NU1902' = 'Moderate'
        'NU1903' = 'High'
        'NU1904' = 'Critical'
    }
    $findings = @()
    foreach ($projectFile in $ProjectFiles) {
        if (-not (Test-Path $projectFile)) {
            Write-Host "  Project not found, skipping: $projectFile"
            continue
        }
        Write-Host "  Auditing $projectFile..."
        # --force so NuGetAudit re-evaluates the advisory feed even if a
        # previous restore left a cached project.assets.json behind.
        $output = & dotnet restore $projectFile `
            --nologo `
            --force `
            --verbosity normal `
            /p:NuGetAudit=true `
            /p:NuGetAuditMode=all `
            /p:NuGetAuditLevel=low 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ERROR: NuGetAudit restore failed for $projectFile" -ForegroundColor Red
            Write-Host ($output -join "`n")
            exit 1
        }
        $projLeaf = Split-Path $projectFile -Leaf
        # NuGetAudit warning format from the MSBuild logger looks like:
        #   <path>(0,0): warning NU1903: Package 'X' 1.2.3 has a known high severity vulnerability, https://github.com/advisories/GHSA-xxxx
        $pattern = 'warning\s+(?<code>NU190[1-4])\s*:\s*(?<msg>.+)$'
        foreach ($line in $output) {
            $text = [string]$line
            $m = [regex]::Match($text, $pattern)
            if (-not $m.Success) { continue }
            $code = $m.Groups['code'].Value
            $severity = $severityByCode[$code]
            if (-not ($FailOnSeverity -contains $severity)) { continue }
            $msg = $m.Groups['msg'].Value.Trim()
            $pkgId = ''
            $pkgVer = ''
            $pkgMatch = [regex]::Match($msg, "Package\s+'(?<id>[^']+)'\s+(?<ver>\S+)")
            if ($pkgMatch.Success) {
                $pkgId = $pkgMatch.Groups['id'].Value
                $pkgVer = $pkgMatch.Groups['ver'].Value
            }
            $advisory = ''
            $urlMatch = [regex]::Match($msg, 'https?://\S+')
            if ($urlMatch.Success) { $advisory = $urlMatch.Value.TrimEnd('.',',',')') }
            $findings += [pscustomobject]@{
                Project  = $projLeaf
                Source   = 'NuGetAudit'
                Kind     = $code
                Package  = $pkgId
                Version  = $pkgVer
                Severity = $severity
                Advisory = $advisory
            }
        }
    }
    # NuGetAudit emits one warning per (package, advisory) per target framework,
    # which can yield duplicates when a project multi-targets. Collapse them.
    $findings = $findings | Sort-Object Project,Package,Version,Advisory -Unique
    if (-not $findings -or $findings.Count -eq 0) {
        Write-Host "  OK - no Critical/High vulnerabilities detected by NuGetAudit."
        return
    }
    Write-VulnerabilityReport -Findings $findings -ReportPath $ReportPath -Heading "Vulnerable NuGet packages detected (NuGetAudit)"
    exit 1
}

function Write-VulnerabilityReport {
    param(
        [object[]]$Findings,
        [string]$ReportPath,
        [string]$Heading
    )
    @(
        "########################################################################",
        ("#  BUILD BLOCKED - {0}" -f $Heading),
        "########################################################################"
    ) | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    foreach ($f in $Findings) {
        Write-Host ("  [{0}] ({1}) {2} {3} {4}  ({5})  {6}" -f $f.Severity.ToUpper(), $f.Source, $f.Project, $f.Package, $f.Version, $f.Kind, $f.Advisory) -ForegroundColor Red
    }
    if ($ReportPath) {
        $dir = Split-Path $ReportPath -Parent
        if ($dir -and -not (Test-Path $dir)) { $null = New-Item -ItemType Directory -Path $dir -Force }
        $md = @()
        $md += "# Warewolf Security Gate - $Heading"
        $md += ""
        $md += "Build blocked at " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss") + "."
        $md += ""
        $md += "| Project | Source | Kind | Package | Version | Severity | Advisory |"
        $md += "|---|---|---|---|---|---|---|"
        foreach ($f in $Findings) {
            $md += "| $($f.Project) | $($f.Source) | $($f.Kind) | $($f.Package) | $($f.Version) | $($f.Severity) | $($f.Advisory) |"
        }
        # Append to an existing report so a build that fails both vuln gates
        # produces a single combined artifact rather than overwriting.
        if (Test-Path $ReportPath) {
            Add-Content -LiteralPath $ReportPath -Value "" -Encoding utf8
            Add-Content -LiteralPath $ReportPath -Value ($md -join "`n") -Encoding utf8
        } else {
            ($md -join "`n") | Out-File -LiteralPath $ReportPath -Encoding utf8 -Force
        }
        Write-Host "  Report written to: $ReportPath" -ForegroundColor Red
    }
}

function Test-DotNetEndOfSupport {
    param(
        [string[]]$ProjectFiles,
        [string]$SupportDataFile,
        [int]$WarnDays = 90
    )
    Write-Host ""
    Write-Host "================================================================"
    Write-Host " Security gate: .NET runtime end-of-support window"
    Write-Host "================================================================"
    if (-not (Test-Path $SupportDataFile)) {
        Write-Host "  ERROR: support data file not found: $SupportDataFile" -ForegroundColor Red
        exit 1
    }
    $data = Get-Content $SupportDataFile -Raw | ConvertFrom-Json
    $today = (Get-Date).Date
    $failed = $false
    foreach ($projectFile in $ProjectFiles) {
        if (-not (Test-Path $projectFile)) {
            Write-Host "  Project not found, skipping: $projectFile"
            continue
        }
        [xml]$proj = Get-Content $projectFile
        $tfm = $null
        foreach ($pg in @($proj.Project.PropertyGroup)) {
            if ($pg.TargetFramework)  { $tfm = ([string]$pg.TargetFramework).Trim(); break }
            if ($pg.TargetFrameworks) { $tfm = ([string]$pg.TargetFrameworks).Split(';')[0].Trim(); break }
        }
        if (-not $tfm) {
            Write-Host "  ERROR: could not read <TargetFramework> from $projectFile" -ForegroundColor Red
            $failed = $true
            continue
        }
        $entry = $data.versions.$tfm
        if (-not $entry) {
            Write-Host ("  FAIL: TargetFramework '{0}' from {1} is not registered in {2}. Add it before this gate can pass." -f $tfm, (Split-Path $projectFile -Leaf), (Split-Path $SupportDataFile -Leaf)) -ForegroundColor Red
            $failed = $true
            continue
        }
        $eos = [datetime]::ParseExact($entry.eosDate, 'yyyy-MM-dd', $null)
        $daysLeft = ($eos.Date - $today).Days
        $projName = Split-Path $projectFile -Leaf
        if ($daysLeft -lt 0) {
            Write-Host ("  FAIL: {0} targets {1} which reached end-of-support on {2} ({3} days ago). Warewolf is on an unsupported runtime." -f $projName, $tfm, $entry.eosDate, [Math]::Abs($daysLeft)) -ForegroundColor Red
            $failed = $true
        } elseif ($daysLeft -le $WarnDays) {
            Write-Host ("  FAIL: {0} targets {1} which reaches end-of-support on {2} ({3} days remaining; threshold {4}). Plan the .NET upgrade now." -f $projName, $tfm, $entry.eosDate, $daysLeft, $WarnDays) -ForegroundColor Red
            $failed = $true
        } else {
            Write-Host ("  OK: {0} targets {1} (supported until {2}, {3} days remaining)." -f $projName, $tfm, $entry.eosDate, $daysLeft)
        }
    }
    if ($failed) { exit 1 }
}

# In-scope projects: Warewolf.Execution.Lightweight plus the full recursive
# <ProjectReference> closure walked from its .csproj. Regenerate when the
# project graph rooted at Warewolf.Execution.Lightweight changes.
$SecurityScopedProjects = @(
    "$RepoRoot\Dev\Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj",
    "$RepoRoot\Dev\Dev2.Activities\Dev2.Activities.csproj",
    "$RepoRoot\Dev\Dev2.Common\Dev2.Common.csproj",
    "$RepoRoot\Dev\Dev2.Common.Interfaces\Dev2.Common.Interfaces.csproj",
    "$RepoRoot\Dev\Dev2.Core\Dev2.Core.csproj",
    "$RepoRoot\Dev\Dev2.CustomControls\Dev2.CustomControls.csproj",
    "$RepoRoot\Dev\Dev2.Data\Dev2.Data.csproj",
    "$RepoRoot\Dev\Dev2.Data.Interfaces\Dev2.Data.Interfaces.csproj",
    "$RepoRoot\Dev\Dev2.Development.Languages\Dev2.Development.Languages.csproj",
    "$RepoRoot\Dev\Dev2.Diagnostics\Dev2.Diagnostics.csproj",
    "$RepoRoot\Dev\Dev2.Infrastructure\Dev2.Infrastructure.csproj",
    "$RepoRoot\Dev\Dev2.Net6.Compatibility\Dev2.Net6.Compatibility.csproj",
    "$RepoRoot\Dev\Dev2.Runtime.Configuration\Dev2.Runtime.Configuration.csproj",
    "$RepoRoot\Dev\Dev2.Runtime.Services\Dev2.Runtime.Services.csproj",
    "$RepoRoot\Dev\Dev2.Scheduler\Dev2.Scheduler.csproj",
    "$RepoRoot\Dev\Dev2.Services.Execution\Dev2.Services.Execution.csproj",
    "$RepoRoot\Dev\Dev2.Services.Sql\Dev2.Services.Sql.csproj",
    "$RepoRoot\Dev\Dev2.SignalR.Wrappers\Dev2.SignalR.Wrappers.Interfaces.csproj",
    "$RepoRoot\Dev\Dev2.TaskScheduler.Wrappers\Dev2.TaskScheduler.Wrappers.csproj",
    "$RepoRoot\Dev\Dev2.Util\Dev2.Util.csproj",
    "$RepoRoot\Dev\GACManagerApi\GACManagerApi.csproj",
    "$RepoRoot\Dev\Log4Net.Async\Log4Net.Async.csproj",
    "$RepoRoot\Dev\Warewolf.AI.Harness\Warewolf.AI.Harness.csproj",
    "$RepoRoot\Dev\Warewolf.Auditing\Warewolf.Auditing.csproj",
    "$RepoRoot\Dev\Warewolf.Common\Warewolf.Common.csproj",
    "$RepoRoot\Dev\Warewolf.Common.Interfaces.NetStandard20\Warewolf.Common.Interfaces.NetStandard20.csproj",
    "$RepoRoot\Dev\Warewolf.Common.NetStandard20\Warewolf.Common.NetStandard20.csproj",
    "$RepoRoot\Dev\Warewolf.Core\Warewolf.Core.csproj",
    "$RepoRoot\Dev\Warewolf.Data\Warewolf.Data.csproj",
    "$RepoRoot\Dev\Warewolf.Driver.Persistence\Warewolf.Driver.Persistence.csproj",
    "$RepoRoot\Dev\Warewolf.Driver.RabbitMQ\Warewolf.Driver.RabbitMQ.csproj",
    "$RepoRoot\Dev\Warewolf.Driver.Redis\Warewolf.Driver.Redis.csproj",
    "$RepoRoot\Dev\Warewolf.Driver.Serilog\Warewolf.Driver.Serilog.csproj",
    "$RepoRoot\Dev\Warewolf.Exchange.Email.Wrapper\Warewolf.Exchange.Email.Wrapper.csproj",
    "$RepoRoot\Dev\Warewolf.Interfaces\Warewolf.Interfaces.csproj",
    "$RepoRoot\Dev\Warewolf.Language.Parser\Warewolf.Language.Parser.fsproj",
    "$RepoRoot\Dev\Warewolf.Licensing\Warewolf.Licensing.csproj",
    "$RepoRoot\Dev\Warewolf.Parser.Interop\Warewolf.Parser.Interop.csproj",
    "$RepoRoot\Dev\Warewolf.Resource\Warewolf.Resource.csproj",
    "$RepoRoot\Dev\Warewolf.ResourceManagement\Warewolf.ResourceManagement.csproj",
    "$RepoRoot\Dev\Warewolf.Security\Warewolf.Security.csproj",
    "$RepoRoot\Dev\Warewolf.ServiceDefinitions\Warewolf.ServiceDefinitions.csproj",
    "$RepoRoot\Dev\Warewolf.Sharepoint\Warewolf.Sharepoint.csproj",
    "$RepoRoot\Dev\Warewolf.Storage\Warewolf.Storage.csproj",
    "$RepoRoot\Dev\Warewolf.Storage.Interfaces\Warewolf.Storage.Interfaces.csproj",
    "$RepoRoot\Dev\Warewolf.Studio.Resources\Warewolf.Studio.Resources.csproj",
    "$RepoRoot\Dev\Warewolf.Weave\Warewolf.Weave.csproj"
)

$ReportPath = "$RepoRoot\Bin\security-report.md"
# Clear any stale combined report from a previous run so the two vuln gates
# always produce a fresh artifact.
if (Test-Path $ReportPath) { Remove-Item -LiteralPath $ReportPath -Force }

if ($SkipVulnerabilityCheck.IsPresent) {
    Write-Host ""
    Write-Host "WARNING - vulnerability gates skipped by caller (-SkipVulnerabilityCheck)." -ForegroundColor Yellow
} else {
    Test-VulnerablePackages -ProjectFiles $SecurityScopedProjects `
        -FailOnSeverity @('Critical','High') `
        -ReportPath $ReportPath
    Test-NuGetAuditPackages -ProjectFiles $SecurityScopedProjects `
        -FailOnSeverity @('Critical','High') `
        -ReportPath $ReportPath
}

if ($SkipEosCheck.IsPresent) {
    Write-Host ""
    Write-Host "WARNING - .NET end-of-support gate skipped by caller (-SkipEosCheck)." -ForegroundColor Yellow
} else {
    Test-DotNetEndOfSupport -ProjectFiles $SecurityScopedProjects `
        -SupportDataFile "$RepoRoot\Build\dotnet-support.json" `
        -WarnDays $WarnDays
}

# All gates passed. Each gate function only calls 'exit 1' on failure and returns
# on success, so when no native command runs (e.g. -SkipVulnerabilityCheck skips
# the dotnet-based vuln gates) $LASTEXITCODE is never set and stays $null. The
# caller (Compile.ps1) tests 'if ($LASTEXITCODE -ne 0)', and '$null -ne 0' is
# $true in PowerShell, which would falsely block the build. Exit 0 explicitly so
# success is always reported unambiguously regardless of which gates ran.
exit 0
