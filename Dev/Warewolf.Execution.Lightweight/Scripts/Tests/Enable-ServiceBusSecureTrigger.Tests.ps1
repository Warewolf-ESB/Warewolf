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
    * Live incident these tests guard (2026-09-03): Phase 3 wrote the JSON-valued
      WAREWOLF_ENTRA_CONFIG setting as an inline "KEY={json}" argument through
      `Invoke-Az` -> `& az @CliArgs`. On Windows az resolves to az.cmd (a batch wrapper),
      and PowerShell/cmd.exe silently strip the embedded double quotes while re-serialising
      the argument array, so the value stored in Azure became unparseable
      ({tenantId:...} with no quotes). az reports success either way. With the config
      unparseable, EntraIdentityOptions falls back to all-null, ServiceBusEntraAuthOptions
      is disabled, and the Service Bus trigger fails closed on every message.
      Set-FunctionAppSettingsViaFile now routes each setting through its own quote-free
      `@<tempfile>` argument, mirroring Configure-WwExecutionAuth.ps1's
      Set-FunctionAppSettings.
#>

BeforeAll {
    $script:TriggerScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Enable-ServiceBusSecureTrigger.ps1'

    # Placeholder values for the script's mandatory parameters; -LoadFunctionsOnly returns
    # before any of them is used.
    $script:LoadArgs = @{
        FunctionAppName         = 'placeholder-app'
        ResourceGroup           = 'placeholder-rg'
        ServiceBusNamespace     = 'placeholder-ns'
        EntraTenantId           = 'placeholder-tenant'
        EntraServiceBusAudience = 'api://placeholder'
        LoadFunctionsOnly       = $true
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

    It 'never writes WAREWOLF_ENTRA_CONFIG as an inline az argument (quote-stripping regression guard)' {
        # Phase 3 must hand the JSON setting to Set-FunctionAppSettingsViaFile, not build a
        # literal "WAREWOLF_ENTRA_CONFIG=..." element inside an Invoke-Az argument array.
        $content = Get-Content -Raw -LiteralPath $script:TriggerScript

        $content | Should -Match 'Set-FunctionAppSettingsViaFile'
        # The only occurrence of the setting-assignment string should be inside the
        # Set-FunctionAppSettingsViaFile call, never in an Invoke-Az array.
        $content | Should -Not -Match "Invoke-Az[^\r\n]*\r?\n(?:[^\r\n]*\r?\n){0,8}[^\r\n]*`"WAREWOLF_ENTRA_CONFIG="
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
            -Settings @('WAREWOLF_ENTRA_CONFIG={"tenantId":"t","serviceBusAudience":"api://sb"}')

        $script:capturedFileContents | Should -Match '"tenantId":"t"'
        $script:capturedFileContents | Should -Match '"serviceBusAudience":"api://sb"'
    }

    It 'passes az one @-file argument per setting and no argument containing a literal quote' {
        Mock Invoke-Az { $script:capturedArgs = @($CliArgs) }

        Set-FunctionAppSettingsViaFile -FunctionAppName 'my-app' -ResourceGroup 'my-rg' `
            -Settings @(
                'ServiceBusConnection__fullyQualifiedNamespace=ns.servicebus.windows.net',
                'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE=q',
                'WAREWOLF_ENTRA_CONFIG={"tenantId":"t"}'
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
            -Settings @('WAREWOLF_ENTRA_CONFIG={"tenantId":"t"}', 'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE=q')

        $script:capturedFileContents.Count | Should -Be 2

        $entraFile = $script:capturedFileContents | Where-Object { $_ -like 'WAREWOLF_ENTRA_CONFIG=*' }
        $entraFile | Should -Not -BeNullOrEmpty
        $entraFile | Should -Not -Match 'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE'
        $entraFile | Should -Not -Match "`n"
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
