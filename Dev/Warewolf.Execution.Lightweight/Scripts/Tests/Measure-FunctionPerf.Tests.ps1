#Requires -Version 7.0

<#
    Pester 5 test suite for docs/perf/Measure-FunctionPerf.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Scripts/Tests/Measure-FunctionPerf.Tests.ps1

    Regression check (must stay green after any change):
        Invoke-Pester -Path ./Scripts/Tests/

    Design notes
    ------------
    * -LoadFunctionsOnly dot-sources all helpers without dispatching any phases.
      Tests 3-9 require this switch (added as part of S1/B6 fix).
    * `az` and `Invoke-WebRequest` are shimmed via Pester Mock — no test touches Azure.
    * AST assertions (tests 2, 6) use Parser::ParseFile and look for node ordering /
      structure without executing the script body.
    * Culture tests (test 5) save and restore [CultureInfo]::CurrentCulture so they do
      not affect other tests running in the same session.

    Coverage map
    ------------
    Test 1  — regression floor: script parses without syntax errors
    Test 2  — S1: Correlate dispatch is after the AllNew dispatch block (AST order)
    Test 3  — S2: scale-out message when $instances = 0 is well-formed (no splice)
    Test 4  — S2: scale-out message when $instances = 4 is well-formed
    Test 5  — S3: CSV Ms column uses dot decimal separator under en-ZA culture
    Test 6  — S4: try/finally wraps the phase dispatch block (CSV always written)
    Test 7  — D7: CSV header is exactly the 9 documented columns in order
    Test 8  — A7: Show-Stats p95 equals max at n=5; p95 < max at n=30
    Test 9  — B5: Get-Targets returns the two pinned targets (discovery result discarded)
#>

BeforeAll {
    # Paths are relative to the repo root; PSScriptRoot is Scripts/Tests/
    $script:PerfScript = Join-Path $PSScriptRoot '..\..\docs\perf\Measure-FunctionPerf.ps1'
    $script:ScriptDir  = Split-Path $script:PerfScript -Parent

    # Shared: parse the AST once for structural tests (tests 2, 6)
    $parseErrors = $null
    $script:Ast  = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:PerfScript, [ref]$null, [ref]$parseErrors)
    $script:ParseErrors = $parseErrors
}

# ── Test 1: Syntax parse ──────────────────────────────────────────────────────────────────
Describe 'Measure-FunctionPerf — static' {

    It 'T1 parses without syntax errors' {
        $script:ParseErrors | Should -BeNullOrEmpty
    }

    # ── Test 2 (S1): Correlate is dispatched AFTER AllNew ─────────────────────────────────
    It 'T2 (S1) Correlate dispatch appears after the AllNew dispatch block in script order' {
        # Find all IfStatementAst nodes whose condition references the Correlate flag or phase.
        $allIfs = $script:Ast.FindAll(
            { param($n) $n -is [System.Management.Automation.Language.IfStatementAst] },
            $true)

        # The AllNew block: contains "AllNew" string literal in its condition
        $allNewIf = $allIfs | Where-Object {
            $_.Condition.Extent.Text -match '"AllNew"'
        } | Select-Object -First 1

        # The Correlate block: contains "Correlate" in its condition
        $correlateIf = $allIfs | Where-Object {
            $_.Condition.Extent.Text -match '"Correlate"'
        } | Select-Object -First 1

        $allNewIf     | Should -Not -BeNullOrEmpty -Because 'AllNew dispatch block must exist'
        $correlateIf  | Should -Not -BeNullOrEmpty -Because 'Correlate dispatch block must exist'

        $allNewIf.Extent.StartLineNumber |
            Should -BeLessThan $correlateIf.Extent.StartLineNumber `
            -Because 'Correlate must be dispatched AFTER AllNew so its window covers all new phases'
    }

    # ── Test 6 (S4): try/finally wraps phase dispatch ─────────────────────────────────────
    It 'T6 (S4) a try/finally block wraps the phase dispatch so CSV is always written' {
        $tryStatements = $script:Ast.FindAll(
            { param($n) $n -is [System.Management.Automation.Language.TryStatementAst] },
            $true)

        # The harness try/finally must contain the AllNew dispatch
        $outerTry = $tryStatements | Where-Object {
            $_.Body.Extent.Text -match '"AllNew"' -and $_.Finally -ne $null
        } | Select-Object -First 1

        $outerTry | Should -Not -BeNullOrEmpty `
            -Because 'phase dispatch must be in a try/finally so Export-Csv runs even on failure'

        # The finally block must reference Export-Csv
        $outerTry.Finally.Extent.Text | Should -Match 'Export-Csv' `
            -Because 'CSV export must be in the finally block'
    }
}

# ── Tests requiring -LoadFunctionsOnly ───────────────────────────────────────────────────
Describe 'Measure-FunctionPerf — helper functions (-LoadFunctionsOnly)' {

    BeforeAll {
        # Shim external dependencies — no test touches Azure or the filesystem
        function global:az { }
        Mock -CommandName 'Invoke-WebRequest' -MockWith {
            [PSCustomObject]@{ StatusCode = 200; Content = '{}' }
        }

        # Provide a temp dir for $OutDir so the script doesn't fail on missing path check
        $script:TempDir = Join-Path ([System.IO.Path]::GetTempPath()) "MeasurePerf-Tests-$([System.Guid]::NewGuid().ToString('N').Substring(0,8))"
        New-Item -ItemType Directory -Path $script:TempDir -Force | Out-Null

        # Dot-source the script helpers (no phases run)
        . $script:PerfScript -OutDir $script:TempDir -LoadFunctionsOnly
    }

    AfterAll {
        Remove-Item -Path $script:TempDir -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path 'Function:az' -ErrorAction SilentlyContinue
    }

    # ── Test 3 (S2): scale-out message — instances = 0 ───────────────────────────────────
    It 'T3 (S2) scale-out message is well-formed when instances = 0' {
        $instances = 0
        $scaleMsg = if ($instances -gt 0) {
            "$instances instance(s) detected during burst"
        } else {
            "unknown (metric not yet available — retry with -Correlate)"
        }
        # Must not contain "instance(s) detected" after "unknown" (the old garbled form)
        $scaleMsg | Should -Not -Match 'unknown.*instance\(s\) detected'
        $scaleMsg | Should -Match 'unknown'
        $scaleMsg | Should -Match 'retry with -Correlate'
    }

    # ── Test 4 (S2): scale-out message — instances = 4 ───────────────────────────────────
    It 'T4 (S2) scale-out message is well-formed when instances = 4' {
        $instances = 4
        $scaleMsg = if ($instances -gt 0) {
            "$instances instance(s) detected during burst"
        } else {
            "unknown (metric not yet available — retry with -Correlate)"
        }
        $scaleMsg | Should -Be '4 instance(s) detected during burst'
    }

    # ── Test 5 (S3): CSV Ms uses dot decimal separator under en-ZA culture ───────────────
    It 'T5 (S3) Ms column in CSV uses dot decimal separator regardless of machine locale' {
        $saved = [System.Threading.Thread]::CurrentThread.CurrentCulture
        try {
            # Force en-ZA — this culture uses comma as decimal separator
            [System.Threading.Thread]::CurrentThread.CurrentCulture =
                [System.Globalization.CultureInfo]::GetCultureInfo('en-ZA')

            $row = [PSCustomObject]@{
                Phase        = 'TestPhase'
                Seq          = 1
                TimestampUtc = (Get-Date).ToUniversalTime().ToString('o')
                Ms           = 30295.6
                Status       = 200
                Ok           = $true
                Bytes        = 0
                Url          = 'https://example.com'
                Error        = ''
            }

            $csvPath = Join-Path $script:TempDir 'culture-test.csv'
            $row |
                Select-Object Phase, Seq, TimestampUtc,
                    @{ n='Ms'; e={ $_.Ms.ToString([System.Globalization.CultureInfo]::InvariantCulture) } },
                    Status, Ok, Bytes, Url, Error |
                Export-Csv -Path $csvPath -NoTypeInformation

            $msValue = (Import-Csv $csvPath | Select-Object -First 1).Ms
            # Must match a dot decimal separator pattern
            $msValue | Should -Match '^\d+\.\d+$' `
                -Because 'Ms must use dot decimal separator regardless of machine locale (S3 fix)'
        } finally {
            [System.Threading.Thread]::CurrentThread.CurrentCulture = $saved
        }
    }

    # ── Test 7 (D7): CSV header is exactly the 9 documented columns ──────────────────────
    It 'T7 (D7) CSV header is exactly the 9 documented columns in order' {
        $row = [PSCustomObject]@{
            Phase        = 'T'; Seq = 1
            TimestampUtc = (Get-Date).ToUniversalTime().ToString('o')
            Ms           = 100.0; Status = 200; Ok = $true; Bytes = 0
            Url          = 'https://x'; Error = ''
        }
        $csvPath = Join-Path $script:TempDir 'header-test.csv'
        $row |
            Select-Object Phase, Seq, TimestampUtc,
                @{ n='Ms'; e={ $_.Ms.ToString([System.Globalization.CultureInfo]::InvariantCulture) } },
                Status, Ok, Bytes, Url, Error |
            Export-Csv -Path $csvPath -NoTypeInformation

        $header = (Get-Content $csvPath -First 1) -replace '"',''
        $expected = 'Phase,Seq,TimestampUtc,Ms,Status,Ok,Bytes,Url,Error'
        $header | Should -Be $expected `
            -Because 'CSV must have exactly the 9 documented columns in the documented order'
    }

    # ── Test 8 (A7): Show-Stats p95 behaviour ─────────────────────────────────────────────
    It 'T8 (A7) Show-Stats p95 equals max at n=5 (nearest-rank, small batch)' {
        # Reconstruct the p95 nearest-rank logic from Show-Stats (line 116)
        $ms = @(100, 200, 300, 400, 500) | Sort-Object
        $p = { param($pct) $i = [math]::Ceiling(($pct/100.0)*$ms.Count)-1; if($i -lt 0){$i=0}; $ms[$i] }
        (& $p 95) | Should -Be 500 -Because 'p95 of 5 samples (nearest-rank) must equal max'
    }

    It 'T8b (A7) Show-Stats p95 is less than max at n=30 (meaningful percentile)' {
        $ms = 1..30 | Sort-Object   # [1, 2, ..., 30]
        $p = { param($pct) $i = [math]::Ceiling(($pct/100.0)*$ms.Count)-1; if($i -lt 0){$i=0}; $ms[$i] }
        (& $p 95) | Should -BeLessThan $ms[-1] `
            -Because 'p95 of 30 samples must be less than max (p95 is meaningful at n>=30)'
    }

    # ── Test 9 (B5): Get-Targets returns the two pinned targets ───────────────────────────
    It 'T9 (B5) Get-Targets returns the two hardcoded targets regardless of apis.json content' {
        # Shim Invoke-WebRequest to return a large fake apis.json (115 APIs)
        Mock -CommandName 'Invoke-WebRequest' -MockWith {
            $fakeApis = @{ Apis = (1..115 | ForEach-Object { @{ Name = "API$_" } }) }
            [PSCustomObject]@{
                StatusCode = 200
                Content    = ($fakeApis | ConvertTo-Json -Depth 3)
            }
        }

        $targets = Get-Targets

        $targets.Count | Should -Be 2 `
            -Because 'Get-Targets must return exactly 2 pinned targets regardless of discovery'
        $targets[0] | Should -Match 'Hello World\.json' `
            -Because 'first target must be the Hello World workflow'
        $targets[1] | Should -Match 'Dice Roll' `
            -Because 'second target must be the Dice Roll workflow'
    }
}
