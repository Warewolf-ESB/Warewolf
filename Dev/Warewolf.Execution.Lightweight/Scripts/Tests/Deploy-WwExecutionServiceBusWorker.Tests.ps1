#Requires -Version 7.0

<#
    Pester 5 test suite for Deploy-WwExecutionServiceBusWorker.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Deploy-WwExecutionServiceBusWorker.Tests.ps1

    Design notes (identical conventions to Deploy-WwJobProcessor.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no cloud/filesystem action.
    * `az` is shadowed with a tracking *function* (functions win over applications
      in PowerShell command resolution), so the suite runs identically with or
      without the Azure CLI installed.
    * End-to-end behaviour is exercised in -DryRun: every mutating action is
      printed (not performed) via Invoke-Az's -Mutating + $DryRun gate.
    * App Insights is disabled in the common args (EnableAppInsights = $false) so
      the suite does not need to shim Setup-ApplicationInsights.ps1's own az calls
      — that child script has its own test coverage.
    * The $global:resourcesExist toggle must be $global: — the az shim is invoked
      from inside `& $DeployScript`.
#>

BeforeAll {
    $script:DeployScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwExecutionServiceBusWorker.ps1'
}

Describe 'Deploy-WwExecutionServiceBusWorker — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'rejects an invalid -PublishMethod via ValidateSet' {
        { & $script:DeployScript -PublishMethod 'NotAMethod' -LoadFunctionsOnly } | Should -Throw
    }

    It 'rejects an invalid -ServiceBusSku via ValidateSet' {
        { & $script:DeployScript -ServiceBusSku 'Premiumish' -LoadFunctionsOnly } | Should -Throw
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:DeployScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Pre-flight'
        Get-Command Resolve-Toggle     -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Save-DeploySummary -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Invoke-Az          -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'defaults ServiceBusQueueName and ShovelSendRuleName on the parameter block' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$null)
        $params = $ast.ParamBlock.Parameters
        $queue  = $params | Where-Object { $_.Name.VariablePath.UserPath -eq 'ServiceBusQueueName' }
        $rule   = $params | Where-Object { $_.Name.VariablePath.UserPath -eq 'ShovelSendRuleName' }
        $queue.DefaultValue.Value | Should -Be 'wwexecution-queue'
        $rule.DefaultValue.Value  | Should -Be 'shovel-send'
    }
}

Describe 'Deploy-WwExecutionServiceBusWorker — helper functions' {

    BeforeAll {
        . $script:DeployScript -LoadFunctionsOnly
    }

    Context 'Read-Required' {
        It 'returns the current value when supplied' { Read-Required -Name 'AppName' -Current 'wwsb' | Should -Be 'wwsb' }
        It 'throws when empty and -NonInteractive' {
            $NonInteractive = $true
            { Read-Required -Name 'AppName' -Current '' } | Should -Throw '*Required value*'
        }
    }

    Context 'Resolve-Toggle' {
        It 'uses an explicit $false regardless of mode' {
            $NonInteractive = $true
            Resolve-Toggle -Name 't' -Current $false -Default $true | Should -BeFalse
        }
        It 'returns the default when null and non-interactive' {
            $NonInteractive = $true
            Resolve-Toggle -Name 't' -Current $null -Default $true | Should -BeTrue
        }
    }

    Context 'Format-AzArgsForLog' {
        It 'masks a *_SECRET setting but keeps a *_NAME setting' {
            $line = Format-AzArgsForLog @('appsettings', 'set', '--settings', 'MY_SECRET=abc123', 'SERVICEBUSQUEUE_NAME=wwexecution-queue')
            $line | Should -Match 'MY_SECRET=\*\*\*REDACTED\*\*\*'
            $line | Should -Match 'SERVICEBUSQUEUE_NAME=wwexecution-queue'
        }
        It 'masks a connection-string-suffixed setting name' {
            $line = Format-AzArgsForLog @('appsettings', 'set', '--settings', 'SERVICEBUS_CONNECTIONSTRING=Endpoint=sb://x/;SharedAccessKey=topsecret')
            $line | Should -Match 'SERVICEBUS_CONNECTIONSTRING=\*\*\*REDACTED\*\*\*'
        }
    }

    Context 'Get-MaskedValue' {
        It 'fully masks short secrets' { Get-MaskedValue 'abc' | Should -Be '******' }
        It 'partially masks and reports length for longer secrets' {
            Get-MaskedValue 'Endpoint=sb://ns.servicebus.windows.net/;SharedAccessKeyName=shovel-send;SharedAccessKey=abc123' |
                Should -Match '^End…\(masked, len=\d+\)$'
        }
    }
}

Describe 'Deploy-WwExecutionServiceBusWorker — end-to-end (DryRun, no side effects)' {

    BeforeEach {
        $global:resourcesExist = $true
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a[0] -eq 'group'       -and $a -contains 'exists') { return ($global:resourcesExist ? 'true' : 'false') }
            if ($a[0] -eq 'storage'     -and $a -contains 'show')   { if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null } return '{"name":"st"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'show')   { if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null } return '{"name":"app"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'identity' -and $a -contains 'assign') { return '{"principalId":"11111111-1111-1111-1111-111111111111"}' }
            if ($a[0] -eq 'servicebus'  -and $a -contains 'namespace' -and $a -contains 'show') {
                if ($a -contains '--query') { return 'sub-123/rg-test/ns' }
                if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null }
                return '{"name":"ns"}'
            }
            if ($a[0] -eq 'servicebus'  -and $a -contains 'queue' -and $a -contains 'authorization-rule' -and $a -contains 'show') {
                if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null }
                return '{"name":"rule"}'
            }
            if ($a[0] -eq 'servicebus'  -and $a -contains 'queue' -and $a -contains 'authorization-rule' -and $a -contains 'keys') { return 'Endpoint=sb://ns.servicebus.windows.net/;SharedAccessKeyName=x;SharedAccessKey=abc123' }
            if ($a[0] -eq 'servicebus'  -and $a -contains 'queue' -and $a -contains 'show') {
                if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null }
                return '{"name":"q"}'
            }
            return '{}'
        }

        # Throwaway publish dir (only needs to exist — this suite validates the
        # orchestrator's decisions, not the zip/func publish mechanics themselves).
        $global:pubDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwsbpub-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path $global:pubDir -Force | Out-Null
        '{ "version": "2.0" }' | Set-Content (Join-Path $global:pubDir 'host.json')

        $global:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwsblog-" + [guid]::NewGuid())

        $script:commonArgs = @{
            SubscriptionId           = 'sub-123'
            TenantId                 = 'tid-456'
            ResourceGroup            = 'rg-test-shovel'
            Location                 = 'southafricanorth'
            StorageAccount           = 'stwwsbworker'
            AppName                  = 'wwsbworker'
            PublishPath              = $global:pubDir
            LogDir                   = $global:logDir
            ServiceBusNamespace      = 'ns-wwsb-test'
            EnableAppInsights        = $false
            WwExecutionBaseUrl       = 'https://wwengine.azurewebsites.net'
            WwExecutionResourceAppId = 'engine-app-id'
            NonInteractive           = $true
            DryRun                   = $true
        }
    }

    AfterEach {
        foreach ($d in @($global:pubDir, $global:logDir)) {
            if ($d -and (Test-Path -LiteralPath $d)) { Remove-Item -LiteralPath $d -Recurse -Force -ErrorAction SilentlyContinue }
        }
    }

    AfterAll {
        Remove-Variable -Name resourcesExist -Scope Global -ErrorAction SilentlyContinue
        Remove-Variable -Name pubDir, logDir -Scope Global -ErrorAction SilentlyContinue
    }

    It 'runs every phase to completion' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Phase 0  Pre-flight'
        $out | Should -Match 'Phase 0.5  Plan'
        $out | Should -Match 'Phase 1  Infrastructure'
        $out | Should -Match 'Phase 2  Service Bus namespace \+ queue provisioning'
        $out | Should -Match 'Phase 3  Managed identity \+ Service Bus RBAC'
        $out | Should -Match 'Phase 4  Apply app settings'
        $out | Should -Match 'Phase 5  Deploy package to Function App'
        $out | Should -Match 'Phase 6  Verify'
        $out | Should -Match 'Dry-run complete'
        $out | Should -Match 'Summary written to'
    }

    It 'provisions the queue with dead-lettering and the default delivery/lock settings when created' {
        $global:resourcesExist = $false
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match "maxDeliveryCount=10, lockDuration=PT300S, DLQ-on-exceed-max-delivery"
        $out | Should -Match '--enable-dead-lettering-on-message-expiration true'
    }

    It 'honours custom -ServiceBusMaxDeliveryCount / -ServiceBusLockDurationSeconds' {
        $global:resourcesExist = $false
        $a = $script:commonArgs.Clone(); $a.ServiceBusMaxDeliveryCount = 5; $a.ServiceBusLockDurationSeconds = 60
        $out = (& $script:DeployScript @a) 6>&1 | Out-String
        $out | Should -Match "maxDeliveryCount=5, lockDuration=PT60S"
    }

    It 'defaults to Managed Identity for the Service Bus listen connection and assigns RBAC' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Worker listen auth\s*: Managed Identity'
        $out | Should -Match 'Assigning RBAC: Azure Service Bus Data Receiver'
        $out | Should -Match 'ServiceBusConnection__fullyQualifiedNamespace=ns-wwsb-test\.servicebus\.windows\.net'
        $out | Should -Match 'ServiceBusConnection__credential=managedidentity'
    }

    It 'falls back to a Listen SAS connection string and skips RBAC when -UseManagedIdentityForServiceBus:$false' {
        $global:resourcesExist = $false
        $a = $script:commonArgs.Clone(); $a.UseManagedIdentityForServiceBus = $false
        $out = (& $script:DeployScript @a) 6>&1 | Out-String
        $out | Should -Match 'Worker listen auth\s*: SAS connection string'
        $out | Should -Not -Match 'Assigning RBAC: Azure Service Bus Data Receiver'
        $out | Should -Match "Creating Listen-only SAS authorization rule 'wwexecutionworker-listen'"
    }

    It 'creates the Send-only shovel SAS rule by default' {
        $global:resourcesExist = $false
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match "Creating Send-only SAS authorization rule 'shovel-send' on the queue \(for the RabbitMQ Shovel bridge\)"
        $out | Should -Match 'RabbitMQ Shovel destination credential'
    }

    It 'skips the shovel SAS rule when -CreateShovelSendRule:$false' {
        $a = $script:commonArgs.Clone(); $a.CreateShovelSendRule = $false
        $out = (& $script:DeployScript @a) 6>&1 | Out-String
        $out | Should -Match "Shovel Send SAS rule not requested"
        $out | Should -Not -Match 'RabbitMQ Shovel destination credential'
    }

    It 'honours a custom -ShovelSendRuleName' {
        $global:resourcesExist = $false
        $a = $script:commonArgs.Clone(); $a.ShovelSendRuleName = 'custom-shovel-rule'
        $out = (& $script:DeployScript @a) 6>&1 | Out-String
        $out | Should -Match "Creating Send-only SAS authorization rule 'custom-shovel-rule'"
    }

    It 'applies the WwExecution app settings' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'WwExecution__BaseUrl\s*= https://wwengine\.azurewebsites\.net'
        $out | Should -Match 'WwExecution__ResourceAppId\s*= engine-app-id'
        $out | Should -Match 'WwExecution__UseClientSecretFallback\s*= false'
        $out | Should -Match '\[DRYRUN\] az functionapp config appsettings set'
    }

    It 'always enables the system-assigned managed identity on the Function App' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az functionapp identity assign'
    }

    It 'deploys via az zip-deploy (config-zip), never func' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\].*config-zip'
        $out | Should -Not -Match 'func azure functionapp publish'
    }

    It 'plans resource creation when resources are missing' {
        $global:resourcesExist = $false
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az group create'
        $out | Should -Match '\[DRYRUN\] az storage account create'
        $out | Should -Match '\[DRYRUN\] az functionapp create'
        $out | Should -Match '\[DRYRUN\] az servicebus namespace create'
    }

    It 'writes a dry-run summary carrying the dryRun flag and shovel rule name' {
        & $script:DeployScript @commonArgs 6>&1 | Out-Null
        $summaryFile = Get-ChildItem -LiteralPath $global:logDir -Filter 'deploy-WwExecutionServiceBusWorker-*.dryrun.summary.json' |
            Sort-Object LastWriteTime | Select-Object -Last 1
        $summaryFile | Should -Not -BeNullOrEmpty
        $summary = Get-Content $summaryFile.FullName -Raw | ConvertFrom-Json
        $summary.dryRun | Should -BeTrue
        $summary.status | Should -Be 'completed'
        $summary.serviceBusNamespace | Should -Be 'ns-wwsb-test'
        $summary.shovelSendRuleCreated | Should -BeTrue
        $summary.shovelSendRuleName | Should -Be 'shovel-send'
        # Connection strings/settings must never appear in plaintext in the summary.
        $summary.shovelSendConnectionString | Should -Not -Match 'SharedAccessKey=abc123'
    }

    # ── Fail-loud validation (reuses the BeforeEach shim + files) ───────────────

    It 'throws when -ServiceBusNamespace is missing (NonInteractive)' {
        $a = $script:commonArgs.Clone(); $a.Remove('ServiceBusNamespace')
        { & $script:DeployScript @a } | Should -Throw '*ServiceBusNamespace*'
    }

    It 'throws when -WwExecutionBaseUrl is missing (NonInteractive)' {
        $a = $script:commonArgs.Clone(); $a.Remove('WwExecutionBaseUrl')
        { & $script:DeployScript @a } | Should -Throw '*WwExecutionBaseUrl*'
    }

    It 'throws when -WwExecutionResourceAppId is missing (NonInteractive)' {
        $a = $script:commonArgs.Clone(); $a.Remove('WwExecutionResourceAppId')
        { & $script:DeployScript @a } | Should -Throw '*WwExecutionResourceAppId*'
    }

    It 'throws when -PublishPath does not exist' {
        $a = $script:commonArgs.Clone(); $a.PublishPath = (Join-Path ([System.IO.Path]::GetTempPath()) ([guid]::NewGuid()))
        { & $script:DeployScript @a } | Should -Throw '*PublishPath not found*'
    }
}
