param(
    [string]$Root = 'C:\Users\ultra\warewolf\coverage-security'
)
$ErrorActionPreference = 'Stop'

# Per-job coverage of the security surface — to see which test job actually exercised which files.
$jobs = Get-ChildItem $Root -Directory | Where-Object Name -notin @('merged','report')
$results = @{}

foreach ($j in $jobs) {
    $xml = Get-ChildItem $j.FullName -Filter '*.cobertura.xml' -File | Where-Object Name -notlike 'parts_*' | Select-Object -First 1
    if (-not $xml) { continue }
    [xml]$doc = Get-Content $xml.FullName
    foreach ($pkg in $doc.coverage.packages.package) {
        if ($null -eq $pkg.classes) { continue }
        foreach ($cls in $pkg.classes.class) {
            $fn = $cls.filename
            if ($null -eq $fn) { continue }
            if (-not ($fn -like '*\Warewolf.Execution.Lightweight\Auth\*' -or $fn -like '*\Warewolf.Execution.Lightweight\Security\*')) { continue }
            $short = $fn.Substring($fn.IndexOf('\Warewolf.Execution.Lightweight\') + 32)
            $lines = @($cls.lines.line)
            $hit = ($lines | Where-Object { [int]$_.hits -gt 0 }).Count
            $tot = $lines.Count
            if (-not $results.ContainsKey($short)) { $results[$short] = @{} }
            # Best (max-hit) coverage per (job,file)
            $key = $j.Name
            if (-not $results[$short].ContainsKey($key) -or $results[$short][$key].Hit -lt $hit) {
                $results[$short][$key] = @{ Hit=$hit; Tot=$tot }
            }
        }
    }
}

# Render: rows=files, cols=jobs
$jobNames = $jobs | ForEach-Object Name | Sort-Object
$out = foreach ($file in ($results.Keys | Sort-Object)) {
    $row = [ordered]@{ File = $file }
    foreach ($jn in $jobNames) {
        if ($results[$file].ContainsKey($jn)) {
            $r = $results[$file][$jn]
            $row[$jn] = if ($r.Tot -gt 0) { [math]::Round(($r.Hit/$r.Tot)*100,0) } else { 0 }
        } else {
            $row[$jn] = '-'
        }
    }
    [PSCustomObject]$row
}

# Use short slug aliases as column headers
$slugMap = @{
    'lightweightexecutionunittests'                                  = 'UT'
    'azurefunctionsintegrationtests'                                  = 'IT'
    'other-security-specs'                                            = 'Oth'
    'resource-permissions-security-specs'                             = 'Res'
    'overlapping-user-groups-permissions-security-specs'              = 'Ovl'
    'no-conflicting-permissions-security-specs'                       = 'NoC'
    'conflicting-view-permissions-security-specs'                     = 'CV'
    'conflicting-execute-permissions-security-specs'                  = 'CE'
    'conflicting-view-execute-permissions-security-specs'             = 'CVE'
    'conflicting-contribute-view-execute-permissions-security-specs'  = 'CCVE'
}

Write-Host "`n=== Per-job line coverage (% of file) ===" -ForegroundColor Cyan
Write-Host "Cols: UT=LightweightUnitTests, IT=AzureFunctionsIntegration, Oth=Other_Security, Res=Resource_Perm, Ovl=Overlapping, NoC=NoConflicting, CV/CE/CVE/CCVE=conflict-spec variants" -ForegroundColor Gray

# Reformat columns
$ordered = $out | ForEach-Object {
    $r = [ordered]@{ File = $_.File }
    foreach ($jn in $jobNames) { $r[$slugMap[$jn]] = $_.$jn }
    [PSCustomObject]$r
}
$ordered | Format-Table -AutoSize
