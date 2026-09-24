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

    It 'names the temp file with a .json extension, not GetTempFileName''s .tmp (az infers the @file payload format from the extension)' {
        # Live incident this guards: a .tmp-named @file argument reproduces the exact
        # "WAREWOLF_ENTRA_CONFIG is not valid JSON" Stage 10c abort this function
        # otherwise fixes - even though the file's CONTENT is correct.
        Mock Invoke-AzCli {
            $script:capturedArgs = @($Arguments)
        }

        Set-FunctionAppSettings -Name 'my-app' -ResourceGroup 'my-rg' `
            -Settings @('WAREWOLF_ENTRA_CONFIG={"tenantId":"t"}')

        $fileArg = @($script:capturedArgs | Where-Object { $_ -like '@*' })[0]
        $fileArg | Should -Not -BeNullOrEmpty
        [System.IO.Path]::GetExtension($fileArg.Substring(1)) | Should -Be '.json'
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

Describe 'Configure-WwExecutionAuth — Merge-EntraConfigJson (Stage 8 read-merge-write)' {
    <#
        Live incident this fixes (2026-09-03): Stage 8 rebuilt WAREWOLF_ENTRA_CONFIG from
        scratch with only tenantId/audience/clientId. This setting is a whole-value app
        setting write (Azure has no JSON-merge primitive), so rebuilding it from scratch
        silently DELETES any other field a different writer had stored in it - the tests
        below use a generic placeholder field ('otherField') to exercise that
        preserve-on-merge behaviour without tying it to any specific field name.

        These tests pin the merge so a future edit cannot reintroduce overwrite-from-scratch.
    #>

    BeforeAll {
        . $script:AuthScript -LoadFunctionsOnly
    }

    It 'preserves a field written by some other process into WAREWOLF_ENTRA_CONFIG' {
        $merged = Merge-EntraConfigJson `
            -ExistingJson '{"tenantId":"old-t","audience":"api://old-a","clientId":"old-c","otherField":"keep-me"}' `
            -TenantId 'new-t' -Audience 'api://new-a' -ClientId 'new-c'

        $merged['otherField'] | Should -Be 'keep-me'
    }

    It 'preserves arbitrary unknown fields it does not own' {
        $merged = Merge-EntraConfigJson `
            -ExistingJson '{"tenantId":"old-t","futureField":"keep-me","anotherOne":"also-keep"}' `
            -TenantId 'new-t' -Audience 'api://new-a' -ClientId 'new-c'

        $merged['futureField'] | Should -Be 'keep-me'
        $merged['anotherOne']  | Should -Be 'also-keep'
    }

    It 'still overwrites the three fields this script owns' {
        $merged = Merge-EntraConfigJson `
            -ExistingJson '{"tenantId":"stale","audience":"api://stale","clientId":"stale","otherField":"keep-me"}' `
            -TenantId 'rotated-t' -Audience 'api://rotated-a' -ClientId 'rotated-c'

        $merged['tenantId'] | Should -Be 'rotated-t'
        $merged['audience'] | Should -Be 'api://rotated-a'
        $merged['clientId'] | Should -Be 'rotated-c'
    }

    It 'produces just the three owned fields on a fresh app with no stored setting' {
        foreach ($absent in @($null, '', '   ')) {
            $merged = Merge-EntraConfigJson -ExistingJson $absent `
                -TenantId 't' -Audience 'api://a' -ClientId 'c'

            @($merged.Keys) | Should -Be @('tenantId', 'audience', 'clientId')
        }
    }

    It "treats az's 'None' rendering of a missing value as absent, not as JSON" {
        $merged = Merge-EntraConfigJson -ExistingJson 'None' `
            -TenantId 't' -Audience 'api://a' -ClientId 'c'

        @($merged.Keys) | Should -Be @('tenantId', 'audience', 'clientId')
    }

    It 'replaces an unparseable stored value and warns instead of throwing' {
        # The corrupted shape seen live: quote-stripped JSON written by an inline
        # `--settings "KEY={json}"` az call.
        $corrupt = '{tenantId:ca0cc53b,audience:api://9901d2f5,otherField:api://e510a863}'

        $warnings = @()
        $merged = Merge-EntraConfigJson -ExistingJson $corrupt `
            -TenantId 't' -Audience 'api://a' -ClientId 'c' -WarningVariable warnings -WarningAction SilentlyContinue

        @($merged.Keys) | Should -Be @('tenantId', 'audience', 'clientId')
        $warnings.Count | Should -BeGreaterThan 0
        ($warnings -join ' ') | Should -Match 'not valid JSON'
    }

    It 'round-trips through ConvertTo-Json as valid JSON with the preserved field intact' {
        $merged = Merge-EntraConfigJson `
            -ExistingJson '{"tenantId":"old","otherField":"keep-me"}' `
            -TenantId 't' -Audience 'api://a' -ClientId 'c'

        $reparsed = $merged | ConvertTo-Json -Compress | ConvertFrom-Json

        $reparsed.tenantId    | Should -Be 't'
        $reparsed.audience    | Should -Be 'api://a'
        $reparsed.clientId    | Should -Be 'c'
        $reparsed.otherField  | Should -Be 'keep-me'
    }
}

Describe 'Configure-WwExecutionAuth — Invoke-EntraConfigRepair (-RepairEntraConfigOnly)' {
    <#
        Drift-repair path: WAREWOLF_ENTRA_CONFIG is an ordinary Function App setting, so a
        delete+recreate of the app (e.g. a Flex Consumption migration) wipes it even though
        the Entra app registration it names is untouched - reproduced live on 'warewolfserver'
        (build 30802, 2026-09-23: ShovelBridge E2E 401 'Authentication required'). This mode
        re-applies just that one setting from the EXISTING registration - no secret rotation,
        no app-role/user-assignment changes, no Easy Auth writes, and it never creates a new
        app registration. These tests drive Invoke-EntraConfigRepair directly (dot-sourced via
        -LoadFunctionsOnly) against a single dispatching Invoke-AzCli mock, the same pattern
        Deploy-WwExecutionEngine.Tests.ps1 uses for its `az` shim.
    #>

    BeforeAll {
        . $script:AuthScript -LoadFunctionsOnly
    }

    BeforeEach {
        $script:functionAppExists = $true
        $script:entraApp          = @{ appId = '11111111-1111-1111-1111-111111111111'; id = 'objid-1' }
        $script:liveAppSettings   = @()
        $script:azCallLog         = New-Object 'System.Collections.Generic.List[string[]]'
        $script:capturedSetFileContent = $null

        Mock Invoke-AzCli {
            $a = @($Arguments)
            $script:azCallLog.Add($a)

            if ($a[0] -eq 'account' -and $a -contains 'set') { return $null }

            if ($a[0] -eq 'functionapp' -and $a -contains 'show') {
                if (-not $script:functionAppExists) { throw 'az CLI failed (3): functionapp not found' }
                return '{"name":"warewolfserver"}'
            }

            if ($a[0] -eq 'ad' -and $a[1] -eq 'app' -and $a[2] -eq 'list') {
                if ($null -eq $script:entraApp) { return 'null' }
                return ($script:entraApp | ConvertTo-Json -Compress)
            }

            if ($a[0] -eq 'functionapp' -and $a -contains 'appsettings' -and $a -contains 'list') {
                return (@($script:liveAppSettings) | ConvertTo-Json -Depth 5 -AsArray)
            }

            if ($a[0] -eq 'functionapp' -and $a -contains 'appsettings' -and $a -contains 'set') {
                $fileArg = @($a | Where-Object { $_ -like '@*' })[0]
                $content = Get-Content -Raw -LiteralPath $fileArg.Substring(1)
                $script:capturedSetFileContent = $content
                $written = @($content | ConvertFrom-Json)
                foreach ($w in $written) {
                    $script:liveAppSettings = @($script:liveAppSettings | Where-Object { $_.name -ne $w.name }) + $w
                }
                return $null
            }

            return $null
        }
    }

    It 'writes only WAREWOLF_ENTRA_CONFIG, and never rotates the secret or touches Easy Auth' {
        $tmpOut = Join-Path ([System.IO.Path]::GetTempPath()) ("repair-out-{0}.json" -f [guid]::NewGuid())
        try {
            $clientId = Invoke-EntraConfigRepair -SubscriptionId 'sub-1' -TenantId 'tenant-1' `
                -ResourceGroupName 'DEV2' -FunctionAppName 'warewolfserver' `
                -EntraAppDisplayName 'warewolfserver-auth' -OutputPath $tmpOut

            $clientId | Should -Be $script:entraApp.appId

            $written = @($script:capturedSetFileContent | ConvertFrom-Json)
            $written.Count | Should -Be 1
            $written[0].name | Should -Be 'WAREWOLF_ENTRA_CONFIG'
            ($written[0].value | ConvertFrom-Json).clientId | Should -Be $script:entraApp.appId

            # Never a secret rotation or Easy Auth call.
            @($script:azCallLog | Where-Object { $_[0] -eq 'ad' -and $_ -contains 'credential' }).Count | Should -Be 0
            @($script:azCallLog | Where-Object { $_[0] -eq 'webapp' -and $_ -contains 'auth' }).Count  | Should -Be 0
            # Never creates a new app registration.
            @($script:azCallLog | Where-Object { $_[0] -eq 'ad' -and $_ -contains 'create' }).Count    | Should -Be 0
        } finally {
            Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
        }
    }

    It 'throws when no matching Entra app registration exists, and never creates one' {
        $script:entraApp = $null
        $tmpOut = Join-Path ([System.IO.Path]::GetTempPath()) ("repair-out-{0}.json" -f [guid]::NewGuid())
        try {
            { Invoke-EntraConfigRepair -SubscriptionId 'sub-1' -TenantId 'tenant-1' `
                -ResourceGroupName 'DEV2' -FunctionAppName 'warewolfserver' `
                -EntraAppDisplayName 'warewolfserver-auth' -OutputPath $tmpOut } |
                Should -Throw '*requires an existing Entra app registration*'

            @($script:azCallLog | Where-Object { $_[0] -eq 'ad' -and $_ -contains 'create' }).Count | Should -Be 0
            @($script:azCallLog | Where-Object { $_[0] -eq 'functionapp' -and $_ -contains 'appsettings' -and $_ -contains 'set' }).Count | Should -Be 0
        } finally {
            Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
        }
    }

    It 'throws when the function app itself does not exist' {
        $script:functionAppExists = $false
        $tmpOut = Join-Path ([System.IO.Path]::GetTempPath()) ("repair-out-{0}.json" -f [guid]::NewGuid())
        try {
            { Invoke-EntraConfigRepair -SubscriptionId 'sub-1' -TenantId 'tenant-1' `
                -ResourceGroupName 'DEV2' -FunctionAppName 'ghost-app' `
                -EntraAppDisplayName 'ghost-app-auth' -OutputPath $tmpOut } |
                Should -Throw "*'ghost-app' was not found*"
        } finally {
            Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
        }
    }

    It 'preserves an unrelated field already stored in WAREWOLF_ENTRA_CONFIG (read-merge-write)' {
        $script:liveAppSettings = @(
            @{ name = 'WAREWOLF_ENTRA_CONFIG'; value = '{"tenantId":"old-t","audience":"api://old","clientId":"old-c","otherField":"keep-me"}' }
        )
        $tmpOut = Join-Path ([System.IO.Path]::GetTempPath()) ("repair-out-{0}.json" -f [guid]::NewGuid())
        try {
            Invoke-EntraConfigRepair -SubscriptionId 'sub-1' -TenantId 'tenant-1' `
                -ResourceGroupName 'DEV2' -FunctionAppName 'warewolfserver' `
                -EntraAppDisplayName 'warewolfserver-auth' -OutputPath $tmpOut | Out-Null

            $written = @($script:capturedSetFileContent | ConvertFrom-Json)[0]
            $writtenValue = $written.value | ConvertFrom-Json
            $writtenValue.otherField | Should -Be 'keep-me'
            $writtenValue.tenantId   | Should -Be 'tenant-1'
            $writtenValue.clientId   | Should -Be $script:entraApp.appId
        } finally {
            Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
        }
    }

    It 'writes a repair-mode summary to -OutputPath' {
        $tmpOut = Join-Path ([System.IO.Path]::GetTempPath()) ("repair-out-{0}.json" -f [guid]::NewGuid())
        try {
            Invoke-EntraConfigRepair -SubscriptionId 'sub-1' -TenantId 'tenant-1' `
                -ResourceGroupName 'DEV2' -FunctionAppName 'warewolfserver' `
                -EntraAppDisplayName 'warewolfserver-auth' -OutputPath $tmpOut | Out-Null

            $summary = Get-Content -Raw -LiteralPath $tmpOut | ConvertFrom-Json
            $summary.Mode               | Should -Be 'RepairEntraConfigOnly'
            $summary.ClientId           | Should -Be $script:entraApp.appId
            $summary.GraphLookupSkipped | Should -Be $false
        } finally {
            Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
        }
    }

    It 'uses -KnownClientId directly and never calls az ad app list' {
        # build 30806, 2026-09-24: warewolfserver's Entra config was hand-set against a shared
        # app registration, not one named "<AppName>-auth", and the pipeline SP has no
        # Microsoft Graph permission to look one up anyway - -KnownClientId bypasses both.
        $knownClientId = '22222222-2222-2222-2222-222222222222'
        $tmpOut = Join-Path ([System.IO.Path]::GetTempPath()) ("repair-out-{0}.json" -f [guid]::NewGuid())
        try {
            $clientId = Invoke-EntraConfigRepair -SubscriptionId 'sub-1' -TenantId 'tenant-1' `
                -ResourceGroupName 'DEV2' -FunctionAppName 'warewolfserver' `
                -EntraAppDisplayName 'Warewolf Security' -OutputPath $tmpOut -KnownClientId $knownClientId

            $clientId | Should -Be $knownClientId

            $written = @($script:capturedSetFileContent | ConvertFrom-Json)
            $written.Count | Should -Be 1
            ($written[0].value | ConvertFrom-Json).clientId | Should -Be $knownClientId

            @($script:azCallLog | Where-Object { $_[0] -eq 'ad' -and $_[1] -eq 'app' -and $_[2] -eq 'list' }).Count | Should -Be 0
        } finally {
            Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
        }
    }

    It 'marks GraphLookupSkipped in the summary when -KnownClientId is used' {
        $knownClientId = '33333333-3333-3333-3333-333333333333'
        $tmpOut = Join-Path ([System.IO.Path]::GetTempPath()) ("repair-out-{0}.json" -f [guid]::NewGuid())
        try {
            Invoke-EntraConfigRepair -SubscriptionId 'sub-1' -TenantId 'tenant-1' `
                -ResourceGroupName 'DEV2' -FunctionAppName 'warewolfserver' `
                -EntraAppDisplayName 'Warewolf Security' -OutputPath $tmpOut -KnownClientId $knownClientId | Out-Null

            $summary = Get-Content -Raw -LiteralPath $tmpOut | ConvertFrom-Json
            $summary.GraphLookupSkipped | Should -Be $true
            $summary.ClientId           | Should -Be $knownClientId
        } finally {
            Remove-Item -LiteralPath $tmpOut -Force -ErrorAction SilentlyContinue
        }
    }
}
