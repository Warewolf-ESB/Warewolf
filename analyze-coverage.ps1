param(
    [string]$Xml = 'C:\Users\ultra\warewolf\coverage-security\merged\all_merged.cobertura.xml'
)
$ErrorActionPreference = 'Stop'
[xml]$doc = Get-Content $Xml

# Cobertura emits one <class> per CLR type — main class, lambdas, async state machines all
# count separately even when they share a source file. To get per-FILE coverage we need to
# UNION line hits across every class element that references the same filename, then count
# covered / total once.
$perFile = @{}
foreach ($pkg in $doc.coverage.packages.package) {
    if ($null -eq $pkg.classes) { continue }
    foreach ($cls in $pkg.classes.class) {
        $fn = $cls.filename
        if ($null -eq $fn) { continue }
        $isAuth     = $fn -like '*\Warewolf.Execution.Lightweight\Auth\*'
        $isSecurity = $fn -like '*\Warewolf.Execution.Lightweight\Security\*'
        if (-not ($isAuth -or $isSecurity)) { continue }
        if (-not $perFile.ContainsKey($fn)) {
            $perFile[$fn] = @{
                Group = if ($isAuth) { 'Auth' } else { 'Security' }
                Lines = @{}     # number -> best (max) hit count seen
            }
        }
        foreach ($line in $cls.lines.line) {
            $n = [int]$line.number
            $h = [int]$line.hits
            if (-not $perFile[$fn].Lines.ContainsKey($n) -or $perFile[$fn].Lines[$n] -lt $h) {
                $perFile[$fn].Lines[$n] = $h
            }
        }
    }
}

$rows = foreach ($fn in $perFile.Keys) {
    $entry = $perFile[$fn]
    $tot = $entry.Lines.Count
    $hit = ($entry.Lines.Values | Where-Object { $_ -gt 0 }).Count
    $short = $fn.Substring($fn.IndexOf('\Warewolf.Execution.Lightweight\') + 'Warewolf.Execution.Lightweight\'.Length + 1)
    [PSCustomObject]@{
        Group    = $entry.Group
        File     = $short
        LinesCov = $hit
        LinesTot = $tot
        Pct      = if ($tot -gt 0) { [math]::Round(($hit/$tot)*100, 1) } else { 0 }
    }
}

Write-Host "`n=== Per-file line coverage (security surface) ===" -ForegroundColor Cyan
$rows | Sort-Object Group, @{Expression='Pct';Descending=$true}, File | Format-Table Group, File, LinesCov, LinesTot, Pct -AutoSize

Write-Host "`n=== Group totals ===" -ForegroundColor Cyan
$rows | Group-Object Group | ForEach-Object {
    $c = ($_.Group | Measure-Object LinesCov -Sum).Sum
    $t = ($_.Group | Measure-Object LinesTot -Sum).Sum
    $p = if ($t -gt 0) { [math]::Round(($c/$t)*100, 1) } else { 0 }
    [PSCustomObject]@{ Group=$_.Name; Files=$_.Count; LinesCov=$c; LinesTot=$t; Pct=$p }
} | Format-Table -AutoSize

$c = ($rows | Measure-Object LinesCov -Sum).Sum
$t = ($rows | Measure-Object LinesTot -Sum).Sum
$p = if ($t -gt 0) { [math]::Round(($c/$t)*100, 1) } else { 0 }
Write-Host "Combined Auth + Security: $c / $t lines ($p%)" -ForegroundColor White
