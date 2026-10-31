#Requires -Version 7.0

<#
    Pester 5 test suite for Enable-ServiceBusSecureTrigger.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Enable-ServiceBusSecureTrigger.Tests.ps1

    Design notes
    ------------
    * The script exposes -LoadFunctionsOnly so its helpers can be dot-sourced and unit
      tested without contacting Azure. Its mandatory parameters must still be supplied,
      so placeholders are passed below.
    * Live incident these tests guard (2026-09-03, against a JSON-valued app setting this
      script wrote at the time): a JSON-valued setting written as an inline "KEY={json}"
      argument through `Invoke-Az` -> `& az @CliArgs`. On Windows az resolves to az.cmd (a
      batch wrapper), and PowerShell/cmd.exe silently strip the embedded double quotes
      while re-serialising the argument array, so the value stored in Azure became
      unparseable. az reports success either way. Set-FunctionAppSettingsViaFile routes
      each setting through its own quote-free `@<tempfile>` argument instead, mirroring
      Configure-WwExecutionAuth.ps1's Set-FunctionAppSettings — a generic app-setting
      value is used below to exercise the same quote-safety behaviour, independent of
      which specific setting this script currently writes.
#>

BeforeAll {
    $script:TriggerScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Enable-ServiceBusSecureTrigger.ps1'

    # Placeholder values for the script's mandatory parameters; -LoadFunctionsOnly returns
    # before any of them is used.
    $script:LoadArgs = @{
        FunctionAppName     = 'placeholder-app'
        ResourceGroup       = 'placeholder-rg'
        ServiceBusNamespace = 'placeholder-ns'
        LoadFunctionsOnly   = $true
    }
}

Describe 'Enable-ServiceBusSecureTrigger — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:TriggerScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:TriggerScript @script:LoadArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Pre-flight'
        Get-Command Set-FunctionAppSettingsViaFile -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'writes Phase 3 app settings through Set-FunctionAppSettingsViaFile, not as inline az arguments (quote-stripping regression guard)' {
        # Phase 3 must hand its settings to Set-FunctionAppSettingsViaFile, not build a
        # literal "KEY=..." element inside a direct Invoke-Az argument array.
        $content = Get-Content -Raw -LiteralPath $script:TriggerScript

        $content | Should -Match 'Set-FunctionAppSettingsViaFile[^\r\n]*-Settings @\('
        $content | Should -Match 'ServiceBusConnection__fullyQualifiedNamespace=\$fullyQualifiedNamespace'
    }

    It 'no longer writes WAREWOLF_SERVICEBUS_TRIGGER_QUEUE as an app setting (WOLF-8517: it comes from servicebustrigger.bite now)' {
        $content = Get-Content -Raw -LiteralPath $script:TriggerScript
        $content | Should -Not -Match 'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE=\$TriggerQueueName'
    }

    It 'no longer writes Settings/servicebustrigger.bite itself (WOLF-8517, corrected: baked in at deploy time by Deploy-WwExecutionEngine.ps1 instead)' {
        # The original WOLF-8517 design wrote this file into an already-deployed Function
        # App via the Kudu VFS API - that cannot work once WEBSITE_RUN_FROM_PACKAGE=1 mounts
        # wwwroot read-only (confirmed live against WarewolfServer-UAT, 2026-09-12). Guard
        # against that mechanism silently coming back.
        $content = Get-Content -Raw -LiteralPath $script:TriggerScript
        $content | Should -Not -Match 'Set-ServiceBusTriggerConfigFile'
        $content | Should -Not -Match '/api/vfs/site/wwwroot'
    }

    It 'no longer has a -WorkflowName parameter (moved to Deploy-WwExecutionEngine.ps1 -ServiceBusTriggerWorkflowName)' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:TriggerScript, [ref]$null, [ref]$null)
        $paramNames = $ast.ParamBlock.Parameters | ForEach-Object { $_.Name.VariablePath.UserPath }
        $paramNames | Should -Not -Contain 'WorkflowName'
    }
}

Describe 'Enable-ServiceBusSecureTrigger — Get-FunctionAppStateDisplay (Flex Consumption has no ''state'')' {

    BeforeAll {
        . $script:TriggerScript @script:LoadArgs
    }

    It 'returns the state value for a classic app shape' {
        $app = [PSCustomObject]@{ state = 'Running'; identity = [PSCustomObject]@{} }
        Get-FunctionAppStateDisplay -App $app | Should -Be 'Running'
    }

    It 'does not throw and returns a fallback for a Flex app shape with no state property (build 30775 regression guard)' {
        # Matches az functionapp show's actual top-level shape for wwprofilerlw (Flex
        # Consumption), captured live 2026-09-22: id/identity/kind/location/name/properties/
        # resourceGroup/tags/type - no 'state'. Under Set-StrictMode -Version Latest (line 128
        # of the script under test), $App.state on an object like this throws
        # PropertyNotFoundException rather than returning $null.
        $app = [PSCustomObject]@{ id = '/subscriptions/x'; identity = [PSCustomObject]@{}; kind = 'functionapp,linux' }
        { Get-FunctionAppStateDisplay -App $app } | Should -Not -Throw
        Get-FunctionAppStateDisplay -App $app | Should -Match 'Flex Consumption'
    }
}

Describe 'Enable-ServiceBusSecureTrigger — Set-FunctionAppSettingsViaFile (quote-safe app settings write)' {

    BeforeAll {
        . $script:TriggerScript @script:LoadArgs
    }

    BeforeEach {
        $script:capturedArgs = $null
        $script:capturedFileContents = $null
        $script:capturedFileArg = $null
    }

    It 'writes each setting to a temp file with embedded quotes intact' {
        Mock Invoke-Az {
            $fileArg = @(@($CliArgs) | Where-Object { $_ -like '@*' })[0]
            $script:capturedFileArg = $fileArg
            # Read synchronously - the caller's `finally` deletes the file once this returns.
            $script:capturedFileContents = Get-Content -Raw -LiteralPath $fileArg.Substring(1)
        }

        Set-FunctionAppSettingsViaFile -FunctionAppName 'my-app' -ResourceGroup 'my-rg' `
            -Settings @('SOME_JSON_SETTING={"foo":"bar","baz":"qux"}')

        $script:capturedFileContents | Should -Match '"foo":"bar"'
        $script:capturedFileContents | Should -Match '"baz":"qux"'
    }

    It 'passes az one @-file argument per setting and no argument containing a literal quote' {
        Mock Invoke-Az { $script:capturedArgs = @($CliArgs) }

        Set-FunctionAppSettingsViaFile -FunctionAppName 'my-app' -ResourceGroup 'my-rg' `
            -Settings @(
                'ServiceBusConnection__fullyQualifiedNamespace=ns.servicebus.windows.net',
                'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE=q',
                'SOME_JSON_SETTING={"foo":"bar"}'
            )

        @($script:capturedArgs | Where-Object { $_ -like '@*' }).Count | Should -Be 3
        @($script:capturedArgs | Where-Object { $_ -match '"' }).Count | Should -Be 0
    }

    It 'gives each setting its own file so no value absorbs another setting''s line' {
        Mock Invoke-Az {
            $fileArgs = @(@($CliArgs) | Where-Object { $_ -like '@*' })
            $script:capturedFileContents = $fileArgs | ForEach-Object { Get-Content -Raw -LiteralPath $_.Substring(1) }
        }

        Set-FunctionAppSettingsViaFile -FunctionAppName 'my-app' -ResourceGroup 'my-rg' `
            -Settings @('SOME_JSON_SETTING={"foo":"bar"}', 'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE=q')

        $script:capturedFileContents.Count | Should -Be 2

        $jsonFile = $script:capturedFileContents | Where-Object { $_ -like 'SOME_JSON_SETTING=*' }
        $jsonFile | Should -Not -BeNullOrEmpty
        $jsonFile | Should -Not -Match 'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE'
        $jsonFile | Should -Not -Match "`n"
    }

    It 'deletes the temp files after the call' {
        Mock Invoke-Az {
            $fileArg = @(@($CliArgs) | Where-Object { $_ -like '@*' })[0]
            $script:capturedFileArg = $fileArg
        }

        Set-FunctionAppSettingsViaFile -FunctionAppName 'my-app' -ResourceGroup 'my-rg' -Settings @('A=1')

        $script:capturedFileArg | Should -Not -BeNullOrEmpty
        Test-Path -LiteralPath $script:capturedFileArg.Substring(1) | Should -BeFalse
    }

    It 'deletes the temp files even when the az call throws' {
        Mock Invoke-Az {
            $fileArg = @(@($CliArgs) | Where-Object { $_ -like '@*' })[0]
            $script:capturedFileArg = $fileArg
            throw 'az CLI failed'
        }

        { Set-FunctionAppSettingsViaFile -FunctionAppName 'my-app' -ResourceGroup 'my-rg' -Settings @('A=1') } | Should -Throw

        Test-Path -LiteralPath $script:capturedFileArg.Substring(1) | Should -BeFalse
    }
}

Describe 'Enable-ServiceBusSecureTrigger — Invoke-Az stream separation' {

    <#
        Build 30759 (2026-09-19) died in Phase 2 with

            ConvertFrom-Json : Conversion from JSON failed with error: Unexpected character
            encountered while parsing value: W.

        The W is the W of WARNING. The pipeline service principal signs in with
        --allow-no-subscriptions and cannot read Microsoft Graph, so
        `az role assignment list --assignee <guid>` writes

            WARNING: Failed to query <guid> by invoking Graph API. If you don't have permission
            to query Graph API, please specify --assignee-object-id and --assignee-principal-type.

        to stderr, falls back to treating the GUID as an object id, and EXITS 0. The command
        succeeded; only the parse died, because Invoke-Az's '2>&1' merged that warning line into
        the value it returned and the caller piped it straight into ConvertFrom-Json.

        stderr may inform the FAILURE MESSAGE. It must never reach the VALUE. Same split, and the
        same reason, as Deploy-WwQueueProcessor.ps1's own Invoke-Az.
    #>

    BeforeAll {
        . $script:TriggerScript @script:LoadArgs

        # Stand-in for the az CLI: a shimming FUNCTION resolves ahead of the az application in
        # PowerShell's command lookup, and Write-Error emits the same ErrorRecord objects that
        # '2>&1' wraps a native command's stderr lines in - which is the behaviour under test.
        function az {
            foreach ($line in $global:azStderr) { Write-Error $line }
            foreach ($line in $global:azStdout) { Write-Output $line }
            $global:LASTEXITCODE = $global:azExit
        }
    }

    BeforeEach {
        $global:azStderr = @()
        $global:azStdout = @()
        $global:azExit   = 0
    }

    AfterAll {
        Remove-Variable -Name azStderr, azStdout, azExit -Scope Global -ErrorAction SilentlyContinue
    }

    It 'returns stdout only when az warns on stderr and still exits 0 (the build 30759 failure)' {
        $global:azStderr = @("WARNING: Failed to query 192ff2c9-7cc6-4575-9249-265523987e70 by invoking Graph API. If you don't have permission to query Graph API, please specify --assignee-object-id and --assignee-principal-type.")
        $global:azStdout = @('[', '  {', '    "roleDefinitionName": "Azure Service Bus Data Receiver"', '  }', ']')

        $result = Invoke-Az -AllowFail @('role', 'assignment', 'list', '--assignee', '192ff2c9-7cc6-4575-9249-265523987e70', '-o', 'json')

        @($result | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] }) | Should -BeNullOrEmpty
        $parsed = $result | Out-String | ConvertFrom-Json
        @($parsed)[0].roleDefinitionName | Should -Be 'Azure Service Bus Data Receiver'
    }

    It 'keeps the stderr text in the failure message when az exits non-zero' {
        $global:azStderr = @('ERROR: (AuthorizationFailed) The client does not have authorization to perform action.')
        $global:azExit   = 1

        { Invoke-Az @('role', 'assignment', 'create', '--role', 'Azure Service Bus Data Receiver') } |
            Should -Throw '*AuthorizationFailed*'
    }

    It 'yields no value at all - not the stderr text - when an -AllowFail probe fails' {
        # Otherwise the Phase 2 probe would treat an error message as an assignment list.
        $global:azStderr = @('ERROR: (AuthorizationFailed) The client does not have authorization to perform action.')
        $global:azExit   = 1

        $result = Invoke-Az -AllowFail @('role', 'assignment', 'list', '--assignee', 'x', '-o', 'json')

        @($result).Count | Should -Be 0
    }

    It 'drops stderr from the value it returns (static guard)' {
        $content = Get-Content -Raw -LiteralPath $script:TriggerScript
        $content | Should -Match 'Where-Object \{ \$_ -isnot \[System\.Management\.Automation\.ErrorRecord\] \}'
    }

    It 'never pipes an Invoke-Az result straight into ConvertFrom-Json on an -AllowFail probe' {
        # An -AllowFail probe can now legitimately return nothing, and ConvertFrom-Json cannot
        # bind an empty string - the emptiness must be tested, not parsed.
        $content = Get-Content -Raw -LiteralPath $script:TriggerScript
        $content | Should -Not -Match 'Invoke-Az -AllowFail[^\r\n]*\| ConvertFrom-Json'
        $content | Should -Match '\$existingAssignment = if \(\$assignmentJson\)'
    }
}
