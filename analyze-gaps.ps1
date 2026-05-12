param(
    [string]$Xml  = 'C:\Users\ultra\warewolf\coverage-security\merged\all_merged.cobertura.xml',
    [string]$Repo = 'C:\Users\ultra\warewolf\Dev'
)
$ErrorActionPreference = 'Stop'
[xml]$doc = Get-Content $Xml

# Target files: any Auth/* or Security/* under the lightweight engine
$rows = @{}
foreach ($pkg in $doc.coverage.packages.package) {
    if ($null -eq $pkg.classes) { continue }
    foreach ($cls in $pkg.classes.class) {
        $fn = $cls.filename
        if ($null -eq $fn) { continue }
        if (-not ($fn -like '*\Warewolf.Execution.Lightweight\Auth\*' -or
                  $fn -like '*\Warewolf.Execution.Lightweight\Security\*')) { continue }
        $short = $fn.Substring($fn.IndexOf('\Warewolf.Execution.Lightweight\') + 33)
        if (-not $rows.ContainsKey($short)) {
            $rows[$short] = @{ File=$fn; Class=$cls.name; Uncovered=@(); Lines=@() }
        }
        foreach ($line in $cls.lines.line) {
            $n = [int]$line.number
            $h = [int]$line.hits
            # Track best hits per line across class merge instances
            $existing = $rows[$short].Lines | Where-Object { $_.Number -eq $n } | Select-Object -First 1
            if ($existing) {
                if ($h -gt $existing.Hits) { $existing.Hits = $h }
            } else {
                $rows[$short].Lines += [PSCustomObject]@{ Number=$n; Hits=$h }
            }
        }
    }
}

# Build uncovered ranges + method names per file
foreach ($key in $rows.Keys | Sort-Object) {
    $r = $rows[$key]
    $sourcePath = $r.File
    if (-not (Test-Path $sourcePath)) { continue }
    $source = Get-Content $sourcePath
    $uncovered = $r.Lines | Where-Object { $_.Hits -eq 0 } | Sort-Object Number
    if ($uncovered.Count -eq 0) { continue }

    Write-Host ''
    Write-Host "=== $key (uncovered $($uncovered.Count) / $($r.Lines.Count) lines) ===" -ForegroundColor Yellow

    # Group consecutive uncovered into ranges + map to nearest method declaration above
    $sortedNums = $uncovered.Number
    $i = 0
    while ($i -lt $sortedNums.Count) {
        $start = $sortedNums[$i]
        $end = $start
        while ($i + 1 -lt $sortedNums.Count -and $sortedNums[$i+1] -eq $end + 1) {
            $end = $sortedNums[$i+1]
            $i++
        }
        $i++
        # Find nearest method/property declaration line at or above $start
        $methodHint = ''
        for ($j = $start - 1; $j -ge [Math]::Max(1, $start - 60); $j--) {
            $line = $source[$j-1]
            if ($line -match '^\s*(public|internal|private|protected|static|async|sealed|virtual|override)\s.*\b(\w+)\s*\(' ) {
                $methodHint = ($line.Trim() -replace '\s+', ' ')
                if ($methodHint.Length -gt 110) { $methodHint = $methodHint.Substring(0, 107) + '...' }
                break
            }
        }
        $rangeText = if ($start -eq $end) { "L$start" } else { "L$start-$end" }
        Write-Host ("  {0,-12}  {1}" -f $rangeText, $methodHint) -ForegroundColor Gray
    }
}
