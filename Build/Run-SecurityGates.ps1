<#
.SYNOPSIS
Pre-compile security gates for the Warewolf lightweight server.

.DESCRIPTION
Two fail-fast checks invoked by Compile.ps1 before any solution is built:

  1. NuGet vulnerability scan
     Runs 'dotnet list package --vulnerable --include-transitive --format json'
     against each in-scope project and exits 1 if any Critical/High CVE is
     reachable (top-level or transitive).

  2. .NET runtime end-of-support window
     Reads <TargetFramework> from each in-scope project, looks the moniker up
     in Build\dotnet-support.json, and exits 1 if the EOS date is past or
     within -WarnDays days of today.

In-scope projects today: Warewolf.Execution.Lightweight (the lightweight server)
and Dev2.Activities (the activity engine it depends on).

The script also writes Bin\security-report.md when the vulnerability gate fails
so CI can attach it as a build artifact.

.PARAMETER RepoRoot
Path to the repo root (the directory containing Compile.ps1 and Dev\). Defaults
to the parent of this script's directory.

.PARAMETER SkipVulnerabilityCheck
Bypass the vulnerability gate. Emits a loud warning so the bypass is recorded.

.PARAMETER SkipEosCheck
Bypass the .NET end-of-support gate. Emits a loud warning so the bypass is recorded.

.PARAMETER WarnDays
Threshold (in days) before the EOS date at which the build starts failing.
Defaults to 90 (3 months).
#>
Param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
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
    Write-Host " Security gate: NuGet vulnerability scan (Critical/High)"
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
        Write-Host "  OK - no Critical/High vulnerabilities detected."
        return
    }
    @(
        "########################################################################",
        "#  BUILD BLOCKED - Vulnerable NuGet packages detected (Critical/High)  #",
        "########################################################################"
    ) | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    foreach ($f in $findings) {
        Write-Host ("  [{0}] {1} {2} {3}  ({4})  {5}" -f $f.Severity.ToUpper(), $f.Project, $f.Package, $f.Version, $f.Kind, $f.Advisory) -ForegroundColor Red
    }
    if ($ReportPath) {
        $dir = Split-Path $ReportPath -Parent
        if ($dir -and -not (Test-Path $dir)) { $null = New-Item -ItemType Directory -Path $dir -Force }
        $md = @()
        $md += "# Warewolf Security Gate - Vulnerable Packages"
        $md += ""
        $md += "Build blocked at " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss") + "."
        $md += ""
        $md += "| Project | Kind | Package | Version | Severity | Advisory |"
        $md += "|---|---|---|---|---|---|"
        foreach ($f in $findings) {
            $md += "| $($f.Project) | $($f.Kind) | $($f.Package) | $($f.Version) | $($f.Severity) | $($f.Advisory) |"
        }
        ($md -join "`n") | Out-File -LiteralPath $ReportPath -Encoding utf8 -Force
        Write-Host "  Report written to: $ReportPath" -ForegroundColor Red
    }
    exit 1
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

# In-scope projects: the lightweight server and the activity engine it depends on.
$SecurityScopedProjects = @(
    "$RepoRoot\Dev\Warewolf.Execution.Lightweight\Warewolf.Execution.Lightweight.csproj",
    "$RepoRoot\Dev\Dev2.Activities\Dev2.Activities.csproj"
)

if ($SkipVulnerabilityCheck.IsPresent) {
    Write-Host ""
    Write-Host "WARNING - vulnerability gate skipped by caller (-SkipVulnerabilityCheck)." -ForegroundColor Yellow
} else {
    Test-VulnerablePackages -ProjectFiles $SecurityScopedProjects `
        -FailOnSeverity @('Critical','High') `
        -ReportPath "$RepoRoot\Bin\security-report.md"
}

if ($SkipEosCheck.IsPresent) {
    Write-Host ""
    Write-Host "WARNING - .NET end-of-support gate skipped by caller (-SkipEosCheck)." -ForegroundColor Yellow
} else {
    Test-DotNetEndOfSupport -ProjectFiles $SecurityScopedProjects `
        -SupportDataFile "$RepoRoot\Build\dotnet-support.json" `
        -WarnDays $WarnDays
}
