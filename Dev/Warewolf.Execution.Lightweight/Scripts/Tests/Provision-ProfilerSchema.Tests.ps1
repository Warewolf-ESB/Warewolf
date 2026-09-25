#Requires -Version 7.0
<#
    Pester 5 suite for Resources/ProfilerServices/Provision-ProfilerSchema.sql.

    Offline: reads the repo's own .bite resources and the SQL text. No SQL Server.

    The invariant: every [Profiler] procedure or function named by a workflow reachable from
    ProfilerWrapper/RequestForProfiling (the workflow the profiler pipelines publish to) is DEFINED
    by the provisioning script. Missing ones fail at run time only, as SQL Error 15009, which the
    engine returns as HTTP 500 and the QueueProcessor dead-letters - build 30853 lost 54 of 200
    messages that way to usp_GetPublicHolidaysInDateRange, and builds 30636/30638 to three others.

    Reachability follows resource ids: a workflow references a sub-workflow or DAL service by the
    GUID in that resource's own ID attribute. Procedures named only by UNREACHABLE .bite files are
    deliberately out of scope (see the script's SCOPE header), so they are not asserted.
#>

BeforeAll {
    $script:ProfilerRoot = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) 'Resources/ProfilerServices'
    $script:SchemaSql    = Get-Content -LiteralPath (Join-Path $script:ProfilerRoot 'Provision-ProfilerSchema.sql') -Raw

    $guid = '[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}'

    # id -> @{ Path; Text }
    $script:Resources = @{}
    foreach ($f in Get-ChildItem -LiteralPath $script:ProfilerRoot -Recurse -Filter '*.bite' -File) {
        $text = Get-Content -LiteralPath $f.FullName -Raw
        $m = [regex]::Match($text, "<Service[^>]*\bID=""($guid)""")
        if (-not $m.Success) { $m = [regex]::Match($text, "ID=""($guid)""") }
        if ($m.Success) {
            $script:Resources[$m.Groups[1].Value.ToLowerInvariant()] = @{
                Path = [System.IO.Path]::GetRelativePath($script:ProfilerRoot, $f.FullName).Replace('\', '/')
                Text = $text
            }
        }
    }

    $entry = $script:Resources.GetEnumerator() |
        Where-Object { $_.Value.Path -ieq 'ProfilerWrapper/RequestForProfiling.bite' } | Select-Object -First 1
    $script:EntryId = $entry.Key

    $script:Reachable = [System.Collections.Generic.HashSet[string]]::new()
    $stack = [System.Collections.Generic.Stack[string]]::new()
    if ($script:EntryId) { $stack.Push($script:EntryId) }
    while ($stack.Count -gt 0) {
        $id = $stack.Pop()
        if (-not $script:Reachable.Add($id)) { continue }
        foreach ($g in [regex]::Matches($script:Resources[$id].Text, $guid)) {
            $ref = $g.Value.ToLowerInvariant()
            if ($ref -ne $id -and $script:Resources.ContainsKey($ref)) { $stack.Push($ref) }
        }
    }

    # procedure/function name -> the reachable .bite files that name it
    $script:Referenced = @{}
    foreach ($id in $script:Reachable) {
        foreach ($m in [regex]::Matches($script:Resources[$id].Text, '\[?Profiler\]?\.\[?((?:usp|func)_\w+)', 'IgnoreCase')) {
            $name = $m.Groups[1].Value.ToLowerInvariant()
            if (-not $script:Referenced.ContainsKey($name)) { $script:Referenced[$name] = [System.Collections.Generic.SortedSet[string]]::new() }
            [void]$script:Referenced[$name].Add($script:Resources[$id].Path)
        }
    }

    $script:Defined = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($m in [regex]::Matches($script:SchemaSql,
            'CREATE\s+OR\s+ALTER\s+(?:PROCEDURE|FUNCTION)\s+\[?Profiler\]?\.\[?(\w+)\]?', 'IgnoreCase')) {
        [void]$script:Defined.Add($m.Groups[1].Value.ToLowerInvariant())
    }
}

Describe 'Provision-ProfilerSchema.sql - coverage of the RequestForProfiling call path' {

    It 'finds the RequestForProfiling entry point and walks past it' {
        # Guards the walk itself: a renamed entry point or a changed ID format would otherwise make
        # the coverage assertion below vacuously pass on an empty set.
        $script:EntryId | Should -Not -BeNullOrEmpty
        $script:Reachable.Count | Should -BeGreaterThan 10
        $script:Referenced.Count | Should -BeGreaterThan 10
    }

    It 'defines every [Profiler] procedure and function a reachable workflow calls' {
        $missing = foreach ($name in ($script:Referenced.Keys | Sort-Object)) {
            if (-not $script:Defined.Contains($name)) {
                "Profiler.$name (called by $($script:Referenced[$name] -join ', '))"
            }
        }
        $missing | Should -BeNullOrEmpty -Because (
            'each missing object fails at run time as SQL Error 15009 -> engine HTTP 500 -> dead-letter. ' +
            'Copy it from Warewolf.ProfilerDB (CREATE -> CREATE OR ALTER), with its dependencies')
    }

    It 'defines func_PublicHolidaysByYear before the procedure that selects from it' {
        $fn   = $script:SchemaSql.IndexOf('CREATE OR ALTER FUNCTION [Profiler].[func_PublicHolidaysByYear]')
        $proc = $script:SchemaSql.IndexOf('CREATE OR ALTER PROCEDURE [Profiler].[usp_GetPublicHolidaysInDateRange]')
        $fn   | Should -BeGreaterThan 0
        $proc | Should -BeGreaterThan $fn
    }
}
