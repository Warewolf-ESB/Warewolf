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

Describe 'Configure-WwExecutionAuth — Set-FunctionAppSettings (quote-safe app settings write)' {
    <#
        Two separate live incidents, fixed together here:

        Bug #1 - Stage 8 used to pass "KEY={JSON}" pairs as raw Invoke-AzCli array
        elements, which PowerShell's `& az @Arguments` call operator silently corrupts
        (strips embedded double quotes) when az resolves to az.cmd on Windows - the az
        call still reports success, but the JSON value written to Azure is invalid.

        Bug #2 - the first fix for bug #1 routed ALL settings through one SHARED temp
        file (`--settings @<file>` with every "KEY=VALUE" pair on its own line). az's
        `@file` substitution replaces a single argument occurrence with the file's raw
        content as ONE token - it does not re-split multi-line content back into
        separate `--settings` entries, so every setting after the first collapsed into
        the first setting's value (WAREWOLF_ENTRA_CONFIG's stored value ended up with
        WAREWOLF_SECURE_CONFIG's entire "KEY=VALUE" line appended after a literal
        newline).

        Set-FunctionAppSettings now writes ONE temp file per setting and passes one
        quote-free `@<path>` argument per setting. These tests pin the file content,
        the per-setting separateness, and the argument shape so a future edit can't
        silently reintroduce either bug.
    #>

    BeforeAll {
        . $script:AuthScript -LoadFunctionsOnly
    }

    BeforeEach {
        $script:capturedArgs = $null
        $script:capturedFileArg = $null
        $script:capturedContent = $null
        $script:capturedPathExistedDuringCall = $null
        $script:capturedFileContents = $null
    }

    It 'writes the settings to a temp file with embedded quotes intact' {
        Mock Invoke-AzCli {
            $script:capturedArgs = @($Arguments)
            $fileArg = @($script:capturedArgs | Where-Object { $_ -like '@*' })[0]
            $script:capturedFileArg = $fileArg
            # Read the file HERE, synchronously, before Set-FunctionAppSettings's
            # own `finally` deletes it once this mock returns.
            $script:capturedContent = Get-Content -Raw -LiteralPath $fileArg.Substring(1)
        }

        Set-FunctionAppSettings -Name 'my-app' -ResourceGroup 'my-rg' `
            -Settings @('WAREWOLF_ENTRA_CONFIG={"tenantId":"t","audience":"api://a"}', 'PLAIN_SETTING=value')

        $script:capturedFileArg | Should -Not -BeNullOrEmpty
        $script:capturedContent | Should -Match '"tenantId":"t"'
        $script:capturedContent | Should -Match '"audience":"api://a"'
    }

    It 'gives each setting its OWN file - no setting''s value contains another setting''s line (bug #2 regression guard)' {
        Mock Invoke-AzCli {
            $fileArgs = @(@($Arguments) | Where-Object { $_ -like '@*' })
            $script:capturedFileContents = $fileArgs | ForEach-Object { Get-Content -Raw -LiteralPath $_.Substring(1) }
        }

        Set-FunctionAppSettings -Name 'my-app' -ResourceGroup 'my-rg' `
            -Settings @('WAREWOLF_ENTRA_CONFIG={"tenantId":"t"}', 'WAREWOLF_SECURE_CONFIG=D:\home\site\wwwroot\secure.config', 'THIRD_SETTING=abc')

        # One file per setting, not one shared file.
        $script:capturedFileContents.Count | Should -Be 3

        $entraFile  = $script:capturedFileContents | Where-Object { $_ -like 'WAREWOLF_ENTRA_CONFIG=*' }
        $secureFile = $script:capturedFileContents | Where-Object { $_ -like 'WAREWOLF_SECURE_CONFIG=*' }
        $thirdFile  = $script:capturedFileContents | Where-Object { $_ -like 'THIRD_SETTING=*' }

        $entraFile  | Should -Not -BeNullOrEmpty
        $secureFile | Should -Not -BeNullOrEmpty
        $thirdFile  | Should -Not -BeNullOrEmpty

        # Neither of the other two settings' keys leaked into this one's value.
        $entraFile | Should -Not -Match 'WAREWOLF_SECURE_CONFIG'
        $entraFile | Should -Not -Match 'THIRD_SETTING'
        $entraFile | Should -Not -Match "`n"
    }

    It 'passes az a single @-file argument and never an argument containing a literal quote' {
        Mock Invoke-AzCli {
            $script:capturedArgs = @($Arguments)
        }

        Set-FunctionAppSettings -Name 'my-app' -ResourceGroup 'my-rg' `
            -Settings @('WAREWOLF_ENTRA_CONFIG={"tenantId":"t"}')

        $script:capturedArgs | Should -Not -BeNullOrEmpty
        @($script:capturedArgs | Where-Object { $_ -like '@*' }).Count | Should -Be 1
        @($script:capturedArgs | Where-Object { $_ -match '"' }).Count | Should -Be 0
    }

    It 'deletes the temp file after a successful call' {
        Mock Invoke-AzCli {
            $fileArg = @(@($Arguments) | Where-Object { $_ -like '@*' })[0]
            $script:capturedPathExistedDuringCall = Test-Path -LiteralPath $fileArg.Substring(1)
            $script:capturedFileArg = $fileArg
        }

        Set-FunctionAppSettings -Name 'my-app' -ResourceGroup 'my-rg' -Settings @('A=1')

        $script:capturedPathExistedDuringCall | Should -BeTrue
        Test-Path -LiteralPath $script:capturedFileArg.Substring(1) | Should -BeFalse
    }

    It 'deletes the temp file even when the az call throws' {
        Mock Invoke-AzCli {
            $fileArg = @(@($Arguments) | Where-Object { $_ -like '@*' })[0]
            $script:capturedFileArg = $fileArg
            throw 'az CLI failed'
        }

        { Set-FunctionAppSettings -Name 'my-app' -ResourceGroup 'my-rg' -Settings @('A=1') } | Should -Throw

        $script:capturedFileArg | Should -Not -BeNullOrEmpty
        Test-Path -LiteralPath $script:capturedFileArg.Substring(1) | Should -BeFalse
    }
}

Describe 'Configure-WwExecutionAuth — Get-BalancedJsonSpan / ConvertFrom-AzJson trailing-preamble tolerance' {
    <#
        Live incident this fixes: az CLI's authV2 extension prints a
        "WARNING: ...following extension: authV2" notice to stderr, which Invoke-AzCli
        merges via `2>&1`. When that notice lands AFTER the JSON in the merged buffer
        (observed once authV2 is enabled on the target app), ConvertFrom-Json threw
        "Additional text encountered after finished reading JSON content" even though
        the JSON itself was completely valid. ConvertFrom-AzJson already handled the
        symmetric LEADING case; these tests pin the trailing case.
    #>

    BeforeAll {
        . $script:AuthScript -LoadFunctionsOnly
    }

    It 'Get-BalancedJsonSpan returns just the object, discarding trailing text' {
        $result = Get-BalancedJsonSpan -Text "{`"a`":1}`nWARNING: extension notice"
        $result | Should -Be '{"a":1}'
    }

    It 'Get-BalancedJsonSpan is not fooled by braces inside a string value' {
        $result = Get-BalancedJsonSpan -Text "{`"a`":`"x}y{z`"}`nWARNING: extension notice"
        $result | Should -Be '{"a":"x}y{z"}'
    }

    It 'Get-BalancedJsonSpan handles nested objects/arrays with trailing garbage' {
        $result = Get-BalancedJsonSpan -Text "{`"a`":[1,{`"b`":2}]}`nWARNING: extension notice"
        $result | Should -Be '{"a":[1,{"b":2}]}'
    }

    It 'ConvertFrom-AzJson parses cleanly when a stderr warning trails the JSON (regression guard)' {
        $parsed = @('{"tenantId":"t"}', 'WARNING: The behavior of this command has been altered by the following extension: authV2') | ConvertFrom-AzJson
        $parsed | Should -Not -BeNullOrEmpty
        $parsed.tenantId | Should -Be 't'
    }
}

Describe 'Configure-WwExecutionAuth — Stage 4 app-role reconciliation accepts an empty desired set' {
    <#
        Live incident this fixes: Deploy-WwExecutionEngine.ps1 invokes
        Configure-WwExecutionAuth.ps1 with no -AuthConfigPath (a legitimate "bootstrap
        auth with no groups configured yet" run), so -GroupPermissions defaults to
        empty and $desiredRoles is a genuinely empty array. Invoke-AppRolePatch and
        Read-AppRoleConflictAction both declared their $DesiredRoles parameter as
        [Parameter(Mandatory)][array] - PowerShell's binder treats an empty array bound
        to a Mandatory parameter as "no value supplied" and throws "Cannot bind
        argument... because it is an empty collection", even though the underlying
        logic already handles zero desired roles correctly (writes an empty appRoles
        array / reports no conflict). Both are now optional, defaulting to @().
    #>

    BeforeAll {
        . $script:AuthScript -LoadFunctionsOnly
    }

    It 'Invoke-AppRolePatch accepts an empty -DesiredRoles without a parameter-binding error' {
        $script:patchedRoles = $null
        Mock Invoke-AzCli {
            $bodyArg = @(@($Arguments) | Where-Object { $_ -like '@*' })[0]
            if ($bodyArg) {
                $script:patchedRoles = (Get-Content -Raw -LiteralPath $bodyArg.Substring(1) | ConvertFrom-Json).appRoles
            }
        }

        { Invoke-AppRolePatch -AppObjectId 'obj-id' -ExistingRoles @() -DesiredRoles @() -Mode 'keep' } | Should -Not -Throw

        @($script:patchedRoles).Count | Should -Be 0
    }

    It 'Read-AppRoleConflictAction accepts an empty -DesiredRoles without a parameter-binding error (no conflicts -> keep, no prompt)' {
        $result = Read-AppRoleConflictAction -ExistingRoles @() -DesiredRoles @()
        $result | Should -Be 'keep'
    }
}
