<#
.SYNOPSIS
    Detects tests that are selected by more than one CI job ("duplicate" tests) in the
    Warewolf Azure DevOps pipeline.

.DESCRIPTION
    Tests are partitioned across pipeline jobs by TestCategory filters (Dev/.azure/pipeline.yml).
    Most jobs run a single category; a few are exclusionary (TestCategory!=A&TestCategory!=B...).
    A job expresses that partition through any of TestRun.ps1's four selection flags -
    -Filter, -Category, -Categories, -ExcludeCategories - which Get-JobTestCaseFilter collapses
    into the one /TestCaseFilter expression vstest receives.
    There is no built-in check that a given test isn't selected by two jobs at once. A test that
    carries two categories, or a category that an exclusionary job forgets to exclude, will run in
    more than one job and waste CI time / skew results.

    This script answers "are duplicate tests running?" two ways:

    STATIC (default) -- the authoritative, build-independent answer.
      For every job it parses out of pipeline.yml it resolves the same assemblies TestRun.ps1 would
      (Projects globs minus ExcludeProjects against -BinariesDir), then asks the *real* VSTest engine
      which tests each job would select:
          vstest.console.exe <assembly> /ListFullyQualifiedTests /TestCaseFilter:"<job's filter>"
      (VSTest honours /TestCaseFilter while listing, so this exactly reproduces job selection without
      re-implementing filter semantics.) Any test selected by >= 2 jobs is reported as a duplicate.

    BUILD (-BuildId) -- empirical answer for a specific run, e.g. buildId 29946.
      Queries the Azure DevOps Test Results REST API for every test run (= job) attached to the build
      and groups results by test name. A test present in >= 2 runs is a genuine cross-job duplicate;
      multiple results inside one run are retries (reported separately, NOT counted as duplicates).

.PARAMETER PipelineYml
    Path to pipeline.yml. Default: Dev\.azure\pipeline.yml next to this script.

.PARAMETER BinariesDir
    Folder containing the compiled *.Tests / *.Specs assemblies. Default: <repo>\Bin\ServerTests.

.PARAMETER VsTestPath
    Path to vstest.console.exe. Auto-discovered under -BinariesDir if omitted.

.PARAMETER Jobs
    Optional wildcard(s) limiting which jobs are analysed (matches job name or step display name).

.PARAMETER OutputDir
    Where CSV/report files are written. Default: <repo>\Bin\DuplicateTestReport.

.PARAMETER FailOnDuplicates
    Exit with code 2 if any duplicate is found (use to gate CI).

.PARAMETER BuildId
    Switch to BUILD mode and scan this Azure DevOps build's published test results.

.PARAMETER Organization
    Azure DevOps org URL for BUILD mode. Default: https://dev.azure.com/Warewolf.

.PARAMETER Project
    Azure DevOps project for BUILD mode. Default: Warewolf.

.PARAMETER Pat
    Azure DevOps Personal Access Token (Test Management / Build read) for BUILD mode. If omitted the
    env var AZDO_PAT is used.

.EXAMPLE
    .\Find-DuplicateTests.ps1
    Static audit using Bin\ServerTests; prints a summary and writes a CSV.

.EXAMPLE
    .\Find-DuplicateTests.ps1 -Jobs '*Specs*' -FailOnDuplicates
    Only analyse Spec jobs and fail if any test is shared between jobs.

.EXAMPLE
    .\Find-DuplicateTests.ps1 -BuildId 29946 -Pat $env:AZDO_PAT
    Scan the actual results of build 29946 for tests that ran in more than one job.
#>
[CmdletBinding(DefaultParameterSetName = 'Static')]
param(
    [Parameter(ParameterSetName = 'Static')]
    [string] $PipelineYml,

    [Parameter(ParameterSetName = 'Static')]
    [string] $BinariesDir,

    [Parameter(ParameterSetName = 'Static')]
    [string] $VsTestPath,

    [Parameter(ParameterSetName = 'Static')]
    [string[]] $Jobs = @('*'),

    [string] $OutputDir,

    [Parameter(ParameterSetName = 'Static')]
    [switch] $FailOnDuplicates,

    [Parameter(ParameterSetName = 'Build', Mandatory = $true)]
    [int] $BuildId,

    [Parameter(ParameterSetName = 'Build')]
    [string] $Organization = 'https://dev.azure.com/Warewolf',

    [Parameter(ParameterSetName = 'Build')]
    [string] $Project = 'Warewolf',

    [Parameter(ParameterSetName = 'Build')]
    [string] $Pat
)

$ErrorActionPreference = 'Stop'
$script:RepoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

function Resolve-DefaultPath([string]$value, [string]$default) {
    if ($value) { return $value }
    return $default
}

# ---------------------------------------------------------------------------
# pipeline.yml parsing
# ---------------------------------------------------------------------------

function Get-FlagToken {
    # Reads a flag whose value is a single whitespace-delimited token, e.g.
    #   -Projects "Dev2.*.Tests","Warewolf.*.Tests"   -> the quoted, comma-joined token
    param([string]$ArgString, [string]$Flag)
    $m = [regex]::Match($ArgString, [regex]::Escape($Flag) + '\s+(\S+)')
    if ($m.Success) { return $m.Groups[1].Value }
    return $null
}

function Get-FlagQuoted {
    # Reads a flag whose value is a single quoted string that may contain spaces, e.g.
    #   -Filter 'TestCategory=Server Startup'
    param([string]$ArgString, [string]$Flag)
    $m = [regex]::Match($ArgString, [regex]::Escape($Flag) + "\s+'([^']*)'")
    if ($m.Success) { return $m.Groups[1].Value }
    $m = [regex]::Match($ArgString, [regex]::Escape($Flag) + '\s+"([^"]*)"')
    if ($m.Success) { return $m.Groups[1].Value }
    # bare token fallback
    return (Get-FlagToken $ArgString $Flag)
}

function Split-QuotedList([string]$token) {
    # "A","B"  /  "A"  /  A,B  ->  @('A','B')
    if (-not $token) { return @() }
    return @($token -split ',' | ForEach-Object { $_.Trim().Trim('"').Trim("'") } | Where-Object { $_ })
}

function Get-JobTestCaseFilter {
    # Collapses a TestRun.ps1 argument string into the single /TestCaseFilter expression
    # vstest would actually receive. TestRun.ps1 accepts FOUR selection flags, not one:
    # -ExcludeCategories / -Category / -Categories (mutually exclusive, in that precedence)
    # and -Filter, which is AND-ed onto whichever of those won. This mirrors the builder at
    # TestRun.ps1's vstest invocation - if the two drift, this gate reports phantom
    # duplicates for every job whose real filter it failed to reproduce.
    param([string]$ArgString)

    $filter = Get-FlagQuoted $ArgString '-Filter'
    # ignore the diagnostic "*.trx" Get-ChildItem -Filter that lives elsewhere in the job
    if ($filter -and ($filter -notmatch 'TestCategory')) { $filter = $null }

    # -Category takes a single (possibly space-bearing) category, so it is read as a quoted
    # value; the list flags are read as one whitespace-delimited token exactly like -Projects.
    # None of these regexes cross-match: Get-FlagToken/Get-FlagQuoted both require whitespace
    # straight after the flag, so '-Category' never swallows '-Categories'.
    $excludeCategories = Split-QuotedList (Get-FlagToken  $ArgString '-ExcludeCategories')
    $category          =                   Get-FlagQuoted $ArgString '-Category'
    $categories        = Split-QuotedList (Get-FlagToken  $ArgString '-Categories')

    $categoryArg = $null
    if ($excludeCategories.Count -gt 0) {
        $categoryArg = '(TestCategory!=' + ($excludeCategories -join ')&(TestCategory!=') + ')'
    } elseif ($category) {
        $categoryArg = "(TestCategory=$category)"
    } elseif ($categories.Count -gt 0) {
        $categoryArg = '(TestCategory=' + ($categories -join ')|(TestCategory=') + ')'
    }

    if ($filter) {
        if ($categoryArg) { return "($categoryArg)&($filter)" }
        return $filter
    }
    return $categoryArg
}

function Parse-PipelineJobs {
    param([string]$Path)
    $lines = Get-Content -LiteralPath $Path
    $jobs = New-Object System.Collections.Generic.List[object]
    $currentJob = $null

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]

        $jm = [regex]::Match($line, '^\s*-\s+job:\s*(\S+)')
        if ($jm.Success) { $currentJob = $jm.Groups[1].Value; continue }

        # locate a TestRun.ps1 step
        if ($line -match 'filePath:.*TestRun\.ps1') {
            # nearest preceding displayName (the step label)
            $display = $null
            for ($b = $i; $b -ge [Math]::Max(0, $i - 8); $b--) {
                $dm = [regex]::Match($lines[$b], "^\s*displayName:\s*'?([^']+)'?\s*$")
                if ($dm.Success) { $display = $dm.Groups[1].Value.Trim().Trim('"',"'"); break }
            }
            # find the arguments: block (search forward a few lines)
            $argStart = -1; $argIndent = 0
            for ($f = $i; $f -lt [Math]::Min($lines.Count, $i + 10); $f++) {
                $am = [regex]::Match($lines[$f], '^(\s*)arguments:\s*(\S*)')
                if ($am.Success) { $argStart = $f; $argIndent = $am.Groups[1].Value.Length; break }
            }
            if ($argStart -lt 0) { continue }

            # collect continuation lines (indented deeper than the arguments: key)
            $sb = New-Object System.Text.StringBuilder
            for ($c = $argStart + 1; $c -lt $lines.Count; $c++) {
                $cl = $lines[$c]
                if ($cl.Trim().Length -eq 0) { break }
                $indent = ($cl.Length - $cl.TrimStart().Length)
                if ($indent -le $argIndent) { break }
                [void]$sb.Append(' ').Append($cl.Trim())
            }
            $argString = $sb.ToString()

            $projects        = Split-QuotedList (Get-FlagToken  $argString '-Projects')
            $excludeProjects = Split-QuotedList (Get-FlagToken  $argString '-ExcludeProjects')
            $filter          = Get-JobTestCaseFilter $argString

            if ($projects.Count -gt 0) {
                $jobs.Add([pscustomobject]@{
                    Job             = $currentJob
                    Display         = if ($display) { $display } else { $currentJob }
                    Projects        = $projects
                    ExcludeProjects = $excludeProjects
                    Filter          = $filter
                })
            }
        }
    }
    return $jobs
}

# ---------------------------------------------------------------------------
# assembly resolution + VSTest discovery (mirrors TestRun.ps1)
# ---------------------------------------------------------------------------

function Resolve-JobAssemblies {
    param([pscustomobject]$JobDef, [string]$BinDir)
    $found = New-Object System.Collections.Generic.List[string]
    foreach ($p in $JobDef.Projects) {
        Get-ChildItem -Path $BinDir -Filter "$p.dll" -Recurse -ErrorAction SilentlyContinue |
            ForEach-Object { $found.Add($_.FullName) }
    }
    $result = New-Object System.Collections.Generic.List[string]
    $seen = New-Object System.Collections.Generic.HashSet[string]
    foreach ($a in $found) {
        $base = [IO.Path]::GetFileNameWithoutExtension($a)
        if ($JobDef.ExcludeProjects -contains $base) { continue }
        if ($seen.Add($a)) { $result.Add($a) }
    }
    return $result
}

$script:DiscoveryCache = @{}

function Get-SelectedTests {
    # Returns the FQNs vstest would select from a single assembly for a given filter.
    param([string]$VsTest, [string]$Assembly, [string]$Filter)
    $key = "$Assembly||$Filter"
    if ($script:DiscoveryCache.ContainsKey($key)) { return $script:DiscoveryCache[$key] }

    $tmp = [IO.Path]::Combine([IO.Path]::GetTempPath(), "dt_" + [Guid]::NewGuid().ToString('N') + ".txt")
    $vsArgs = @($Assembly, '/ListFullyQualifiedTests', "/ListTestsTargetPath:$tmp")
    if ($Filter) { $vsArgs += "/TestCaseFilter:$Filter" }

    $tests = @()
    try {
        & $VsTest @vsArgs *> $null
        if (Test-Path $tmp) {
            $tests = @(Get-Content -LiteralPath $tmp | Where-Object { $_.Trim() } | ForEach-Object { $_.Trim() })
        }
    } catch {
        Write-Warning "VSTest discovery failed for $([IO.Path]::GetFileName($Assembly)) (filter: $Filter): $($_.Exception.Message)"
    } finally {
        Remove-Item $tmp -ErrorAction SilentlyContinue
    }
    $script:DiscoveryCache[$key] = $tests
    return $tests
}

function Invoke-StaticAnalysis {
    $pipelinePath = Resolve-DefaultPath $PipelineYml (Join-Path $PSScriptRoot 'pipeline.yml')
    $binDir       = Resolve-DefaultPath $BinariesDir (Join-Path $script:RepoRoot 'Bin\ServerTests')
    $outDir       = Resolve-DefaultPath $OutputDir   (Join-Path $script:RepoRoot 'Bin\DuplicateTestReport')

    if (-not (Test-Path $pipelinePath)) { throw "pipeline.yml not found at $pipelinePath" }
    if (-not (Test-Path $binDir))       { throw "Binaries dir not found at $binDir (build with .\Compile.ps1 -ServerTests)" }

    if (-not $VsTestPath) {
        $VsTestPath = (Get-ChildItem -Path $binDir -Recurse -Filter 'vstest.console.exe' -ErrorAction SilentlyContinue |
                       Select-Object -First 1).FullName
    }
    if (-not $VsTestPath -or -not (Test-Path $VsTestPath)) {
        throw "vstest.console.exe not found under $binDir. Pass -VsTestPath explicitly."
    }

    New-Item -ItemType Directory -Force -Path $outDir | Out-Null

    Write-Host "Pipeline : $pipelinePath"
    Write-Host "Binaries : $binDir"
    Write-Host "VSTest   : $VsTestPath"
    Write-Host ""

    $allJobs = Parse-PipelineJobs -Path $pipelinePath
    $selectedJobs = $allJobs | Where-Object {
        $j = $_
        ($Jobs | Where-Object { $j.Job -like $_ -or $j.Display -like $_ }) }
    Write-Host ("Parsed {0} test jobs from pipeline.yml; analysing {1}." -f $allJobs.Count, @($selectedJobs).Count)
    Write-Host ""

    # testKey ("assembly::FQN") -> list of job display names
    $map = @{}
    $jobInfo = New-Object System.Collections.Generic.List[object]

    $n = 0
    foreach ($job in $selectedJobs) {
        $n++
        $assemblies = Resolve-JobAssemblies -JobDef $job -BinDir $binDir
        $jobTestCount = 0
        if ($assemblies.Count -eq 0) {
            Write-Host ("[{0}/{1}] {2}: no assemblies found for [{3}] (skipped)" -f `
                $n, @($selectedJobs).Count, $job.Display, ($job.Projects -join ',')) -ForegroundColor DarkYellow
        }
        foreach ($asm in $assemblies) {
            $asmName = [IO.Path]::GetFileName($asm)
            $tests = Get-SelectedTests -VsTest $VsTestPath -Assembly $asm -Filter $job.Filter
            foreach ($t in $tests) {
                $k = "$asmName::$t"
                if (-not $map.ContainsKey($k)) { $map[$k] = New-Object System.Collections.Generic.List[string] }
                if (-not $map[$k].Contains($job.Display)) { $map[$k].Add($job.Display) }
            }
            $jobTestCount += $tests.Count
        }
        $jobInfo.Add([pscustomobject]@{ Job = $job.Display; Filter = $job.Filter; Assemblies = $assemblies.Count; Tests = $jobTestCount })
        Write-Host ("[{0}/{1}] {2}  ->  {3} tests across {4} assembly(ies)" -f `
            $n, @($selectedJobs).Count, $job.Display, $jobTestCount, $assemblies.Count)
    }

    $dupes = $map.GetEnumerator() | Where-Object { $_.Value.Count -gt 1 } | Sort-Object { $_.Value.Count } -Descending

    # write reports
    $jobCsv = Join-Path $outDir 'job-test-counts.csv'
    $jobInfo | Sort-Object Job | Export-Csv -NoTypeInformation -Path $jobCsv

    $dupCsv = Join-Path $outDir 'duplicate-tests.csv'
    $rows = foreach ($d in $dupes) {
        $asm, $fqn = $d.Key -split '::', 2
        [pscustomobject]@{ Assembly = $asm; Test = $fqn; JobCount = $d.Value.Count; Jobs = ($d.Value -join ' | ') }
    }
    if ($rows) { $rows | Export-Csv -NoTypeInformation -Path $dupCsv } else { '' | Set-Content $dupCsv }

    Write-Host ""
    Write-Host "================ SUMMARY ================"
    $totalTests = $map.Count
    $dupCount = @($dupes).Count
    Write-Host ("Distinct tests selected across analysed jobs : {0}" -f $totalTests)
    Write-Host ("Tests selected by more than one job          : {0}" -f $dupCount) -ForegroundColor $(if ($dupCount) { 'Red' } else { 'Green' })
    Write-Host ("Job/test-count report                        : {0}" -f $jobCsv)
    if ($dupCount) { Write-Host ("Duplicate detail (CSV)                       : {0}" -f $dupCsv) -ForegroundColor Red }
    Write-Host "========================================="

    if ($dupCount) {
        Write-Host ""
        Write-Host "Top duplicates (test -> jobs):" -ForegroundColor Red
        foreach ($d in ($dupes | Select-Object -First 25)) {
            $asm, $fqn = $d.Key -split '::', 2
            Write-Host ("  [{0}] {1}" -f $d.Value.Count, $fqn) -ForegroundColor Yellow
            Write-Host ("      {0}  ({1})" -f ($d.Value -join '  |  '), $asm)
        }
        if ($dupCount -gt 25) { Write-Host ("  ... and {0} more (see {1})" -f ($dupCount - 25), $dupCsv) }
    }

    if ($FailOnDuplicates -and $dupCount -gt 0) { exit 2 }
    if ($dupCount -gt 0) { exit 1 }
    exit 0
}

# ---------------------------------------------------------------------------
# BUILD mode -- Azure DevOps Test Results REST API
# ---------------------------------------------------------------------------

function Invoke-BuildScan {
    if (-not $Pat) { $Pat = $env:AZDO_PAT }
    if (-not $Pat) { throw "BUILD mode needs a PAT. Pass -Pat or set `$env:AZDO_PAT (scope: Test Management/Build read)." }
    $headers = @{ Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$Pat")) }
    $base = "$Organization/$Project/_apis"

    $runsUri = "$base/test/runs?buildUri=vstfs:///Build/Build/$BuildId&api-version=7.1"
    Write-Host "Querying test runs for build $BuildId ..."
    $runs = (Invoke-RestMethod -Uri $runsUri -Headers $headers -Method Get).value
    if (-not $runs) { Write-Warning "No test runs found for build $BuildId."; return }
    Write-Host ("Found {0} test run(s)." -f $runs.Count)

    # testName -> @{ runs = set of runName; total = count; retries = count }
    $map = @{}
    foreach ($run in $runs) {
        $skip = 0; $top = 1000
        do {
            $resUri = "$base/test/Runs/$($run.id)/results?api-version=7.1&`$top=$top&`$skip=$skip"
            $batch = (Invoke-RestMethod -Uri $resUri -Headers $headers -Method Get).value
            foreach ($r in $batch) {
                $name = $r.automatedTestName
                if (-not $name) { $name = $r.testCaseTitle }
                if (-not $name) { continue }
                if (-not $map.ContainsKey($name)) {
                    $map[$name] = [pscustomobject]@{ Runs = (New-Object System.Collections.Generic.HashSet[string]); Total = 0 }
                }
                [void]$map[$name].Runs.Add($run.name)
                $map[$name].Total++
            }
            $skip += $top
        } while ($batch.Count -eq $top)
        Write-Host ("  run '{0}' (id {1}) processed" -f $run.name, $run.id)
    }

    $crossJob = $map.GetEnumerator() | Where-Object { $_.Value.Runs.Count -gt 1 } | Sort-Object { $_.Value.Runs.Count } -Descending
    $retried  = $map.GetEnumerator() | Where-Object { $_.Value.Runs.Count -eq 1 -and $_.Value.Total -gt 1 }

    $outDir = Resolve-DefaultPath $OutputDir (Join-Path $script:RepoRoot 'Bin\DuplicateTestReport')
    New-Item -ItemType Directory -Force -Path $outDir | Out-Null
    $csv = Join-Path $outDir "build-$BuildId-duplicate-tests.csv"
    $rows = foreach ($d in $crossJob) {
        [pscustomobject]@{ Test = $d.Key; JobCount = $d.Value.Runs.Count; TotalResults = $d.Value.Total; Jobs = (($d.Value.Runs) -join ' | ') }
    }
    if ($rows) { $rows | Export-Csv -NoTypeInformation -Path $csv } else { '' | Set-Content $csv }

    Write-Host ""
    Write-Host "================ BUILD $BuildId SUMMARY ================"
    Write-Host ("Distinct tests in results        : {0}" -f $map.Count)
    Write-Host ("Tests run in MORE THAN ONE job   : {0}" -f @($crossJob).Count) -ForegroundColor $(if (@($crossJob).Count) { 'Red' } else { 'Green' })
    Write-Host ("Tests retried within a single job: {0} (not cross-job duplicates)" -f @($retried).Count)
    Write-Host ("Detail CSV                       : {0}" -f $csv)
    Write-Host "======================================================="
    foreach ($d in ($crossJob | Select-Object -First 25)) {
        Write-Host ("  [{0} jobs] {1}" -f $d.Value.Runs.Count, $d.Key) -ForegroundColor Yellow
        Write-Host ("      {0}" -f (($d.Value.Runs) -join '  |  '))
    }

    if ($FailOnDuplicates -and @($crossJob).Count -gt 0) { exit 2 }
}

# ---------------------------------------------------------------------------

if ($PSCmdlet.ParameterSetName -eq 'Build') {
    Invoke-BuildScan
} else {
    Invoke-StaticAnalysis
}
