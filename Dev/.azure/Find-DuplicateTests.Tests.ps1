<#
    Pester 5 suite for Find-DuplicateTests.ps1's pipeline.yml parsing.

        Invoke-Pester -Path ./Find-DuplicateTests.Tests.ps1

    Focus: Get-JobTestCaseFilter, the translation from a TestRun.ps1 argument string to the
    single /TestCaseFilter expression vstest would receive. The gate originally read only
    -Filter, so the two Warewolf.Execution.Lightweight.Integration.Tests jobs -- which
    partition themselves with -ExcludeCategories / -Category instead -- both looked like
    unfiltered whole-assembly runs and every one of their 306 tests was reported as a
    cross-job duplicate. That is the regression these tests pin.

    The script under test runs its analysis at file scope, so dot-sourcing it would fire a
    real VSTest discovery sweep. Instead the function definitions are lifted out of the
    file with the PowerShell AST and re-declared here -- no production change needed, and
    nothing outside the parsing helpers is executed.
#>

BeforeAll {
    $script:SourceScript = Join-Path $PSScriptRoot 'Find-DuplicateTests.ps1'

    $tokens = $null; $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:SourceScript, [ref]$tokens, [ref]$errors)
    if ($errors -and $errors.Count -gt 0) {
        throw "Find-DuplicateTests.ps1 failed to parse: $($errors[0].Message)"
    }

    $wanted = @('Get-FlagToken', 'Get-FlagQuoted', 'Split-QuotedList', 'Get-JobTestCaseFilter', 'Parse-PipelineJobs')
    $found = $ast.FindAll(
        { param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $true)
    foreach ($name in $wanted) {
        $fn = $found | Where-Object { $_.Name -eq $name } | Select-Object -First 1
        if (-not $fn) { throw "Find-DuplicateTests.ps1 no longer defines $name" }
        . ([scriptblock]::Create($fn.Extent.Text))
    }

    function script:New-PipelineFixture {
        # Minimal pipeline.yml shaped exactly the way Parse-PipelineJobs walks it: a
        # `- job:` line, a TestRun.ps1 filePath, a preceding displayName and a folded
        # `arguments: >-` block terminated by a blank line.
        param([hashtable[]] $Jobs)

        $sb = New-Object System.Text.StringBuilder
        [void]$sb.AppendLine('stages:')
        [void]$sb.AppendLine('- stage: Tests')
        [void]$sb.AppendLine('  jobs:')
        foreach ($j in $Jobs) {
            [void]$sb.AppendLine("  - job: $($j.Name)")
            [void]$sb.AppendLine('    steps:')
            [void]$sb.AppendLine('    - task: PowerShell@2')
            [void]$sb.AppendLine("      displayName: 'Run $($j.Name)'")
            [void]$sb.AppendLine('      inputs:')
            [void]$sb.AppendLine('        filePath: ''$(Agent.BuildDirectory)\WindowsTests\TestRun.ps1''')
            [void]$sb.AppendLine('        arguments: >-')
            foreach ($line in $j.Args) {
                [void]$sb.AppendLine("          $line")
            }
            [void]$sb.AppendLine('')
        }

        $path = Join-Path ([System.IO.Path]::GetTempPath()) ("dup-pipeline-" + [Guid]::NewGuid().ToString('N') + '.yml')
        Set-Content -LiteralPath $path -Value $sb.ToString() -Encoding UTF8
        return $path
    }
}

Describe 'Get-JobTestCaseFilter' {

    It 'translates -Category into an equality TestCategory filter' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -Category "RoundTripFidelity" -TestResultsDir "X"' |
            Should -BeExactly '(TestCategory=RoundTripFidelity)'
    }

    It 'translates a single -ExcludeCategories value into an inequality filter' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -ExcludeCategories "RoundTripFidelity" -TestResultsDir "X"' |
            Should -BeExactly '(TestCategory!=RoundTripFidelity)'
    }

    It 'AND-joins multiple -ExcludeCategories values, matching TestRun.ps1' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -ExcludeCategories "Alpha","Beta"' |
            Should -BeExactly '(TestCategory!=Alpha)&(TestCategory!=Beta)'
    }

    It 'OR-joins multiple -Categories values, matching TestRun.ps1' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -Categories "Alpha","Beta"' |
            Should -BeExactly '(TestCategory=Alpha)|(TestCategory=Beta)'
    }

    It 'does not let the -Category lookup swallow a -Categories list' {
        # '-Category' is a literal prefix of '-Categories'; both readers demand whitespace
        # straight after the flag, so a -Categories-only job must take the OR branch.
        Get-JobTestCaseFilter '-Projects "A.Tests" -Categories "Alpha","Beta"' |
            Should -Not -Match 'TestCategory!='
        Get-JobTestCaseFilter '-Projects "A.Tests" -Categories "Alpha","Beta"' |
            Should -Not -BeExactly '(TestCategory=Alpha,Beta)'
    }

    It 'does not let the -Category lookup swallow -ExcludeCategories' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -ExcludeCategories "Alpha"' |
            Should -BeExactly '(TestCategory!=Alpha)'
    }

    It 'reads a -Category value containing spaces' {
        # pipeline-LOADTEST.yml uses -Category "Load Tests".
        Get-JobTestCaseFilter '-Projects "A.Tests" -Category "Load Tests" -TestResultsDir "X"' |
            Should -BeExactly '(TestCategory=Load Tests)'
    }

    It 'passes a bare -Filter through unchanged' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -Filter "TestCategory=Alpha"' |
            Should -BeExactly 'TestCategory=Alpha'
    }

    It 'reads a -Filter value containing spaces' {
        Get-JobTestCaseFilter "-Projects ""A.Tests"" -Filter 'TestCategory=Server Startup'" |
            Should -BeExactly 'TestCategory=Server Startup'
    }

    It 'AND-joins -Filter onto a category selection' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -Category "Alpha" -Filter "TestCategory!=Beta"' |
            Should -BeExactly '((TestCategory=Alpha))&(TestCategory!=Beta)'
    }

    It 'gives -ExcludeCategories precedence over -Category, as TestRun.ps1 does' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -ExcludeCategories "Alpha" -Category "Beta"' |
            Should -BeExactly '(TestCategory!=Alpha)'
    }

    It 'gives -Category precedence over -Categories, as TestRun.ps1 does' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -Category "Beta" -Categories "Gamma","Delta"' |
            Should -BeExactly '(TestCategory=Beta)'
    }

    It 'ignores a -Filter that is not a TestCategory expression' {
        # Guards the pre-existing carve-out for the diagnostic Get-ChildItem -Filter "*.trx".
        Get-JobTestCaseFilter '-Projects "A.Tests" -Filter "*.trx"' | Should -BeNullOrEmpty
    }

    It 'returns nothing for a job with no selection flags at all' {
        Get-JobTestCaseFilter '-Projects "A.Tests" -TestResultsDir "X"' | Should -BeNullOrEmpty
    }
}

Describe 'Parse-PipelineJobs' {

    It 'gives the two Lightweight integration jobs disjoint, non-null filters' {
        # The exact regression: before Get-JobTestCaseFilter existed both jobs parsed to a
        # null filter, so the gate discovered the whole assembly twice and reported all 306
        # of its tests as cross-job duplicates.
        $path = script:New-PipelineFixture -Jobs @(
            @{ Name = 'AzureFunctionsIntegrationTests'; Args = @(
                '-RetryCount 6',
                '-Projects "Warewolf.Execution.Lightweight.Integration.Tests"',
                '-ExcludeCategories "RoundTripFidelity"',
                '-TestResultsDir "$(Agent.BuildDirectory)\TestResults"') },
            @{ Name = 'RoundTripFidelity_Tests'; Args = @(
                '-Projects "Warewolf.Execution.Lightweight.Integration.Tests"',
                '-Category "RoundTripFidelity"',
                '-TestResultsDir "$(Agent.BuildDirectory)\TestResults"') }
        )
        try {
            $jobs = @(Parse-PipelineJobs -Path $path)
            $jobs.Count | Should -Be 2

            $af = $jobs | Where-Object { $_.Job -eq 'AzureFunctionsIntegrationTests' }
            $rt = $jobs | Where-Object { $_.Job -eq 'RoundTripFidelity_Tests' }

            $af.Filter | Should -BeExactly '(TestCategory!=RoundTripFidelity)'
            $rt.Filter | Should -BeExactly '(TestCategory=RoundTripFidelity)'
            $af.Filter | Should -Not -BeExactly $rt.Filter
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'resolves the same two jobs when they are expressed with -Filter' {
        # The form both jobs now use in pipeline.yml; selection must be identical.
        $path = script:New-PipelineFixture -Jobs @(
            @{ Name = 'AzureFunctionsIntegrationTests'; Args = @(
                '-Projects "Warewolf.Execution.Lightweight.Integration.Tests"',
                '-Filter "TestCategory!=RoundTripFidelity"') },
            @{ Name = 'RoundTripFidelity_Tests'; Args = @(
                '-Projects "Warewolf.Execution.Lightweight.Integration.Tests"',
                '-Filter "TestCategory=RoundTripFidelity"') }
        )
        try {
            $jobs = @(Parse-PipelineJobs -Path $path)
            ($jobs | Where-Object { $_.Job -eq 'AzureFunctionsIntegrationTests' }).Filter |
                Should -BeExactly 'TestCategory!=RoundTripFidelity'
            ($jobs | Where-Object { $_.Job -eq 'RoundTripFidelity_Tests' }).Filter |
                Should -BeExactly 'TestCategory=RoundTripFidelity'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'still carries Projects and ExcludeProjects through untouched' {
        $path = script:New-PipelineFixture -Jobs @(
            @{ Name = 'Unit_Tests'; Args = @(
                '-Projects "Dev2.*.Tests","Warewolf.*.Tests"',
                '-ExcludeProjects "Dev2.Integration.Tests","Warewolf.UI.Tests"',
                '-Filter "TestCategory=UnitTest"') }
        )
        try {
            $job = @(Parse-PipelineJobs -Path $path)[0]
            $job.Projects        | Should -Be @('Dev2.*.Tests', 'Warewolf.*.Tests')
            $job.ExcludeProjects | Should -Be @('Dev2.Integration.Tests', 'Warewolf.UI.Tests')
            $job.Display         | Should -BeExactly 'Run Unit_Tests'
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }

    It 'skips steps that declare no -Projects' {
        $path = script:New-PipelineFixture -Jobs @(
            @{ Name = 'NoProjects'; Args = @('-TestResultsDir "X"', '-Category "Alpha"') }
        )
        try {
            @(Parse-PipelineJobs -Path $path).Count | Should -Be 0
        }
        finally { Remove-Item $path -Force -ErrorAction SilentlyContinue }
    }
}

Describe 'pipeline.yml selection flags' {

    It 'never lets two jobs with the same -Projects list both run unfiltered' {
        # A $null filter means "run this job's whole assembly set", which is legitimate for a
        # job that owns its assemblies outright. Two such jobs over the same assemblies is
        # the duplicate this gate exists to catch -- that is the RoundTripFidelity regression
        # in miniature, and it is what this asserts against.
        #
        # Scope: this compares -Projects lists verbatim, so it is a cheap structural guard,
        # NOT a replacement for the gate. It cannot see a glob overlapping a literal
        # (Warewolf.*.Tests vs Warewolf.Storage.Tests, which pipeline.yml resolves with
        # -ExcludeProjects); only the real VSTest discovery sweep in Find-DuplicateTests.ps1
        # resolves those.
        $pipeline = Join-Path $PSScriptRoot 'pipeline.yml'
        $jobs = @(Parse-PipelineJobs -Path $pipeline)
        $jobs.Count | Should -BeGreaterThan 0

        $unfiltered = @($jobs | Where-Object { -not $_.Filter })
        foreach ($group in ($jobs | Group-Object { ($_.Projects | Sort-Object) -join ',' })) {
            $nullFiltered = @($group.Group | Where-Object { -not $_.Filter })
            if ($group.Count -gt 1) {
                $nullFiltered.Count | Should -Be 0 -Because (
                    "jobs sharing projects '$($group.Name)' must each narrow their selection: " +
                    (($nullFiltered | ForEach-Object { $_.Job }) -join ', '))
            }
        }
        $unfiltered | Out-Null
    }
}
