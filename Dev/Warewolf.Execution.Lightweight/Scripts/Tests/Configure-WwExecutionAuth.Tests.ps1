#Requires -Version 7.0

<#
    Pester 5 test suite for Configure-WwExecutionAuth.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Configure-WwExecutionAuth.Tests.ps1

    Design notes
    ------------
    * The script exposes -LoadFunctionsOnly so its helper functions can be
      dot-sourced and unit-tested without executing any interactive prompt or
      cloud / Graph action.
    * Resolve-AssignmentUser is the Stage 6 guard: the UPN lookup throws on a
      missing/typo'd user, and the stage must "never abort on one row", so the
      helper warns and returns $null (the loop then skips that row). These tests
      mock Invoke-AzCli to drive the three outcomes: valid user, lookup throws,
      and empty result.
#>

BeforeAll {
    $script:AuthScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Configure-WwExecutionAuth.ps1'
}

Describe 'Configure-WwExecutionAuth — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:AuthScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a stage' {
        $out = (. $script:AuthScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Stage 0  Pre-flight'
        Get-Command Resolve-AssignmentUser -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }
}

Describe 'Configure-WwExecutionAuth — Resolve-AssignmentUser (Stage 6 guard)' {

    BeforeAll {
        . $script:AuthScript -LoadFunctionsOnly
    }

    It 'returns the user object for a valid UPN' {
        Mock Invoke-AzCli { '{"id":"oid-12345","userPrincipalName":"good@contoso.com"}' }
        $u = Resolve-AssignmentUser -Upn 'good@contoso.com'
        $u | Should -Not -BeNullOrEmpty
        $u.id | Should -Be 'oid-12345'
    }

    It 'returns $null and warns when the lookup throws (missing/typo UPN)' {
        Mock Invoke-AzCli { throw 'AADSTS: user not found' }
        $u = Resolve-AssignmentUser -Upn 'ghost@contoso.com' -WarningVariable warn 3>$null
        $u | Should -BeNullOrEmpty
        ($warn -join ' ') | Should -Match 'skipping this row'
    }

    It 'returns $null and warns when the lookup yields no object' {
        Mock Invoke-AzCli { $null }
        $u = Resolve-AssignmentUser -Upn 'empty@contoso.com' -WarningVariable warn 3>$null
        $u | Should -BeNullOrEmpty
        ($warn -join ' ') | Should -Match 'skipping this row'
    }
}
