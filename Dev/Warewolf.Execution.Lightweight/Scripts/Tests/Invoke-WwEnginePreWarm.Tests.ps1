#Requires -Version 7.0

<#
    Pester 5 test suite for Invoke-WwEnginePreWarm.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Invoke-WwEnginePreWarm.Tests.ps1

    Design notes (same conventions as Monitor-RabbitMqShovel.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the pure helpers with no HTTP/network action.
    * No live engine call is made anywhere in this suite — Phase A/B and their
      Invoke-WebRequest usage are exercised only via a live pipeline run
      (pipeline-LOADTEST.yml), matching this script's own -DryRun guidance.
    * This is deliberately IN ADDITION TO, not a replacement for, manual
      validation against a real Consumption-plan engine (see
      docs/ShovelBridge-Architecture.md's 2026-08-16 entry for why this script
      exists).
#>

BeforeAll {
    $script:PreWarmScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Invoke-WwEnginePreWarm.ps1'
}

Describe 'Invoke-WwEnginePreWarm — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:PreWarmScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running Phase A/B' {
        $out = (. $script:PreWarmScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase A'
        $out | Should -Not -Match 'Phase B'
        Get-Command Get-WarmupUri            -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Format-WarmupRequestBody -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Update-LatencyStreak     -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Test-WarmupRoundClean    -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'requires -EngineBaseUrl and -AccessToken when not -LoadFunctionsOnly or -DryRun' {
        { & $script:PreWarmScript } | Should -Throw '*EngineBaseUrl*'
        $token = ConvertTo-SecureString 'dummy' -AsPlainText -Force
        { & $script:PreWarmScript -AccessToken $token } | Should -Throw '*EngineBaseUrl*'
        { & $script:PreWarmScript -EngineBaseUrl 'https://example.azurewebsites.net' } | Should -Throw '*AccessToken*'
    }

    It 'DryRun requires no live call and prints the phase plan' {
        $token = ConvertTo-SecureString 'dummy' -AsPlainText -Force
        $out = (& $script:PreWarmScript -EngineBaseUrl 'https://example.azurewebsites.net' -AccessToken $token -DryRun) 6>&1 | Out-String
        $out | Should -Match '\[DryRun\] Phase A'
        $out | Should -Match '\[DryRun\] Phase B'
    }
}

Describe 'Get-WarmupUri' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'joins a base URL and route with exactly one slash' {
        Get-WarmupUri -BaseUrl 'https://example.azurewebsites.net' -Route 'Secure/rabbit/RabbitProcess' |
            Should -Be 'https://example.azurewebsites.net/Secure/rabbit/RabbitProcess'
    }

    It 'tolerates a trailing slash on the base URL and a leading slash on the route' {
        Get-WarmupUri -BaseUrl 'https://example.azurewebsites.net/' -Route '/Secure/rabbit/RabbitProcess' |
            Should -Be 'https://example.azurewebsites.net/Secure/rabbit/RabbitProcess'
    }
}

Describe 'Format-WarmupRequestBody' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'produces the inputParameters envelope WorkflowFunctionHelper.cs expects' {
        $json = Format-WarmupRequestBody -MessagePrefix 'WARMUP' -Label 'seq-1'
        $obj  = $json | ConvertFrom-Json
        $obj.inputParameters.message | Should -Be 'WARMUP-seq-1'
    }
}

Describe 'Update-LatencyStreak' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'extends the streak on a fast 200' {
        Update-LatencyStreak -CurrentStreak 2 -StatusCode 200 -ElapsedMs 100 -ThresholdMs 5000 | Should -Be 3
    }

    It 'resets the streak on a non-200' {
        Update-LatencyStreak -CurrentStreak 3 -StatusCode 500 -ElapsedMs 100 -ThresholdMs 5000 | Should -Be 0
    }

    It 'resets the streak on a slow 200 (at or over the threshold)' {
        Update-LatencyStreak -CurrentStreak 3 -StatusCode 200 -ElapsedMs 5000 -ThresholdMs 5000 | Should -Be 0
    }

    It 'starts a new streak of 1 from zero on a fast 200' {
        Update-LatencyStreak -CurrentStreak 0 -StatusCode 200 -ElapsedMs 50 -ThresholdMs 5000 | Should -Be 1
    }
}

Describe 'Test-WarmupRoundClean' {
    BeforeAll { . $script:PreWarmScript -LoadFunctionsOnly }

    It 'is clean when every result is 200 and the count matches' {
        $results = 1..6 | ForEach-Object { [pscustomobject]@{ Code = 200; Ms = 42 } }
        Test-WarmupRoundClean -Results $results -ExpectedCount 6 | Should -BeTrue
    }

    It 'is not clean when any result is not 200' {
        $results = @(
            [pscustomobject]@{ Code = 200; Ms = 42 }
            [pscustomobject]@{ Code = 503; Ms = 42 }
        )
        Test-WarmupRoundClean -Results $results -ExpectedCount 2 | Should -BeFalse
    }

    It 'is not clean when fewer results came back than expected (e.g. a dropped call)' {
        $results = 1..5 | ForEach-Object { [pscustomobject]@{ Code = 200; Ms = 42 } }
        Test-WarmupRoundClean -Results $results -ExpectedCount 6 | Should -BeFalse
    }
}
