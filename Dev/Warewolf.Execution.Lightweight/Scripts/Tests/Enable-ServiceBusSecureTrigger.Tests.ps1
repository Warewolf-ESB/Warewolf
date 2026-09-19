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

Describe 'Enable-ServiceBusSecureTrigger — profiler pipeline wiring' {

    <#
        Build 30756 (2026-09-18) is the regression these tests exist for. Every ShovelBridge leg
        passed and the delivery smoke test still failed after 301s, because nothing had ever
        granted wwprofilerlw's managed identity 'Azure Service Bus Data Receiver' or set
        ServiceBusConnection__fullyQualifiedNamespace — this script's job, and it was documented
        as a manual operator step that a newly-stood-up rig simply never got.

        These assert the PIPELINE wiring, not the script: that the step still exists, and that the
        queue name and lock duration are driven off the SAME variables the deploy and the shovel
        destination use. That last one is the quiet failure mode — the script's own output warns
        that a -TriggerQueueName mismatch means "the trigger listens on a queue this run's RBAC
        grant doesn't cover", which fails exactly as silently as the bug being fixed.

        Deliberately text assertions rather than a YAML parse: no YAML module is assumed to be
        present, and these run offline like the rest of this suite.
    #>

    BeforeAll {
        $repoRoot = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent
        $script:PipelinePath = Join-Path $repoRoot '.azure' 'ProfilerService-CI-UAT-LightweightExecutionEngine.yml'
        $script:Pipeline = Get-Content -LiteralPath $script:PipelinePath -Raw
    }

    It 'the profiler pipeline exists where these tests expect it' {
        Test-Path -LiteralPath $script:PipelinePath | Should -BeTrue
    }

    It 'invokes Enable-ServiceBusSecureTrigger.ps1' {
        $script:Pipeline | Should -Match 'StagedSecureTriggerScriptPath'
        $script:Pipeline | Should -Match "displayName: 'Enable Service Bus Secure Trigger"
    }

    It 'resolves the script out of the published scripts package' {
        $script:Pipeline | Should -Match "Filter 'Enable-ServiceBusSecureTrigger\.ps1'"
    }

    It 'stages host.json from the engine package so the delivery-budget invariant can be checked' {
        # Get-EstimatedClaimStaleAfter reads "$PSScriptRoot\..\host.json" and throws without it;
        # the published scripts zip ships none and extracts flat.
        $script:Pipeline | Should -Match "displayName: 'Stage secure-trigger enabler \+ engine host\.json'"
        $script:Pipeline | Should -Match 'ZipFile\]::OpenRead\(.\$\(EngineZipPath\)'
    }

    It 'passes the SAME queue-name variable the engine deploy stages into servicebustrigger.bite' {
        $script:Pipeline | Should -Match "-TriggerQueueName '\`$\(ProfilerServiceBusTriggerQueueName\)'"
        $script:Pipeline | Should -Match "-ServiceBusTriggerQueueName '\`$\(ProfilerServiceBusTriggerQueueName\)'"
    }

    It 'passes the SAME lock duration the queue-provisioning step converges the queue onto' {
        $script:Pipeline | Should -Match "-LockDuration '\`$\(ProfilerServiceBusTriggerQueueLockDuration\)'"
    }

    It 'runs non-interactively' {
        # The script prompts for confirmation otherwise, and Read-Host on a hosted agent hangs
        # until the job times out rather than failing.
        $script:Pipeline | Should -Match '-NonInteractive'
    }

    It 'runs as AzureCLI@2, not a bare pwsh step' {
        # AzureCLI@2 scopes its login to a per-task AZURE_CONFIG_DIR, so a plain pwsh step has no
        # az account at all — the same trap that broke the shovel step in build 30747.
        $enableStep = [regex]::Match($script:Pipeline, "(?s)- task: AzureCLI@2\r?\n\s+displayName: 'Enable Service Bus Secure Trigger.*?(?=\r?\n  - |\r?\n  # )").Value
        $enableStep | Should -Not -BeNullOrEmpty
        $enableStep | Should -Match 'scriptType: pscore'
    }

    It 'enables the trigger BEFORE the delivery smoke test that depends on it' {
        $enableAt = $script:Pipeline.IndexOf("displayName: 'Enable Service Bus Secure Trigger")
        $smokeAt  = $script:Pipeline.IndexOf("displayName: 'Smoke Test: ShovelBridge Delivery")
        $enableAt | Should -BeGreaterThan 0
        $smokeAt  | Should -BeGreaterThan 0
        $enableAt | Should -BeLessThan $smokeAt
    }

    It 'verifies the app setting and the role assignment after running the script' {
        # The two values that ARE the fix; a silent no-op costs a 301s smoke-test timeout to find.
        $script:Pipeline | Should -Match 'ServiceBusConnection__fullyQualifiedNamespace'
        $script:Pipeline | Should -Match "roles -notcontains 'Azure Service Bus Data Receiver'"
    }
}
