<#
    Pester 5 suite for ProfilerService-CI-Dev-LightweightExecutionEngine.yml and
    ProfilerDev.trigger.bite.

        Invoke-Pester -Path ./ProfilerService-CI-Dev-LightweightExecutionEngine.Tests.ps1

    A pipeline file cannot be run locally, so this suite checks what CAN be proven without Azure:

      * the YAML parses, and every embedded PowerShell script is syntactically valid (a bad
        script otherwise fails only after the agent has spent minutes on the steps before it);
      * the Dev file deploys THE SAME ENGINE as the UAT file - same app, same deploy flags - so the
        two cannot drift into leaving the shared app in different states;
      * the Dev file no longer builds the ShovelBridge and talks only to the CloudAMQP broker -
        nothing in it reaches the self-hosted rabbitmq.warewolf.online broker the UAT shovel uses;
      * the trigger file is consistent with the pipeline that deploys it.

    Requires the powershell-yaml module. The whole suite is skipped (not failed) without it.
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
}

BeforeAll {
    $script:DevPath     = Join-Path $PSScriptRoot 'ProfilerService-CI-Dev-LightweightExecutionEngine.yml'
    $script:UatPath     = Join-Path $PSScriptRoot 'ProfilerService-CI-UAT-LightweightExecutionEngine.yml'
    $script:TriggerPath = Join-Path $PSScriptRoot 'ProfilerDev.trigger.bite'
    $script:LoadTestTriggerPath = Join-Path $PSScriptRoot 'ProfilerLoadTest.trigger.bite'

    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        $script:Dev = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath $script:DevPath -Raw)
        $script:Uat = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath $script:UatPath -Raw)
    }

    # The PowerShell (or AzureCLI) body of a step, or $null for a task with no script.
    function script:Get-StepScript {
        param($Step)
        if ($Step.Contains('pwsh'))       { return [string]$Step['pwsh'] }
        if ($Step.Contains('powershell')) { return [string]$Step['powershell'] }
        if ($Step.Contains('inputs') -and $Step['inputs'] -is [System.Collections.IDictionary] -and $Step['inputs'].Contains('inlineScript')) {
            return [string]$Step['inputs']['inlineScript']
        }
        return $null
    }

    function script:Get-Steps { param($Doc) @($Doc['jobs'][0]['steps']) }

    function script:Find-Step {
        param($Doc, [string] $DisplayName)
        Get-Steps $Doc | Where-Object { $_['displayName'] -eq $DisplayName } | Select-Object -First 1
    }

    function script:Get-StepIndex {
        param($Doc, [string] $DisplayName)
        $steps = @(Get-Steps $Doc)
        for ($i = 0; $i -lt $steps.Count; $i++) { if ($steps[$i]['displayName'] -eq $DisplayName) { return $i } }
        return -1
    }

    # The -Flag names on a Deploy-WwExecutionEngine.ps1 'arguments' string, sorted, values dropped.
    function script:Get-FlagNames {
        param([string] $Arguments)
        [regex]::Matches($Arguments, '(?<![\w-])-([A-Za-z][A-Za-z0-9]*)') |
            ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
    }
}

Describe 'ProfilerService-CI-Dev pipeline - structure' -Skip:(-not $script:HaveYaml) {

    It 'parses as a single-job, manually-triggered pipeline' {
        $script:Dev['trigger'] | Should -Be 'none'
        @($script:Dev['jobs']).Count | Should -Be 1
        (Get-Steps $script:Dev).Count | Should -BeGreaterThan 20
    }

    It 'has a unique displayName on every step (the tests below look steps up by name)' {
        $names = @(Get-Steps $script:Dev | ForEach-Object { $_['displayName'] })
        ($names | Where-Object { -not $_ }).Count | Should -Be 0
        ($names | Group-Object | Where-Object Count -gt 1 | ForEach-Object Name) | Should -BeNullOrEmpty
    }

    It 'every embedded script is syntactically valid PowerShell' {
        $failures = @()
        $parsed = 0
        foreach ($step in (Get-Steps $script:Dev)) {
            $body = Get-StepScript $step
            if (-not $body) { continue }
            $parsed++
            # Azure DevOps expands $(name) before the script runs; stand a token in for it. A
            # sub-expression '$( ... )' or '$($x)' does not match the name pattern and is left alone.
            $expanded = [regex]::Replace($body, '\$\([A-Za-z_][A-Za-z0-9_.]*\)', 'MACRO')
            $tokens = $null; $errors = $null
            [void][System.Management.Automation.Language.Parser]::ParseInput($expanded, [ref]$tokens, [ref]$errors)
            foreach ($e in $errors) { $failures += "[$($step['displayName'])] line $($e.Extent.StartLineNumber): $($e.Message)" }
        }
        # Guard against a vacuous pass: a step-shape change that made Get-StepScript return $null for
        # everything would otherwise leave nothing to fail.
        $parsed | Should -BeGreaterThan 20
        $failures | Should -BeNullOrEmpty
    }

    It 'the syntax check itself detects a broken script' {
        $tokens = $null; $errors = $null
        [void][System.Management.Automation.Language.Parser]::ParseInput('if ($x { Write-Host "unbalanced"', [ref]$tokens, [ref]$errors)
        $errors.Count | Should -BeGreaterThan 0
    }
}

Describe 'ProfilerService-CI-Dev pipeline - same engine as UAT' -Skip:(-not $script:HaveYaml) {

    It 'targets the same engine app, storage account, resource group and region' {
        foreach ($name in 'ProfilerEngineAppName', 'ProfilerEngineStorageAccount', 'ProfilerEngineResourceGroup', 'ProfilerEngineLocation') {
            $script:Dev['variables'][$name] | Should -Be $script:Uat['variables'][$name] -Because "both files deploy the SAME app; $name must not diverge"
        }
    }

    It 'passes the same Deploy-WwExecutionEngine.ps1 flags as the UAT file' {
        $devArgs = (Find-Step $script:Dev 'Deploy via Deployment Scripts')['inputs']['arguments']
        $uatArgs = (Find-Step $script:Uat 'Deploy via Deployment Scripts')['inputs']['arguments']
        $dev = @(Get-FlagNames $devArgs)
        $uat = @(Get-FlagNames $uatArgs)
        $uat.Count | Should -BeGreaterThan 10
        ($dev -join ',') | Should -Be ($uat -join ',') -Because 'flags dropped from a later deploy revert silently on the shared app'
    }

    It 'keeps the engine ServiceBusTrigger settings so its %WAREWOLF_SERVICEBUS_TRIGGER_QUEUE% binding still indexes' {
        $devArgs = (Find-Step $script:Dev 'Deploy via Deployment Scripts')['inputs']['arguments']
        $devArgs | Should -Match '-ServiceBusTriggerQueueName'
        $devArgs | Should -Match '-ServiceBusTriggerWorkflowName'
        $script:Dev['variables']['ProfilerServiceBusTriggerQueueName']    | Should -Be $script:Uat['variables']['ProfilerServiceBusTriggerQueueName']
        $script:Dev['variables']['ProfilerServiceBusTriggerWorkflowName'] | Should -Be $script:Uat['variables']['ProfilerServiceBusTriggerWorkflowName']
    }
}

Describe 'ProfilerService-CI-Dev pipeline - ShovelBridge is gone, CloudAMQP only' -Skip:(-not $script:HaveYaml) {

    It 'does not provision or exercise the bridge in any step' {
        $offenders = @()
        foreach ($step in (Get-Steps $script:Dev)) {
            $body = Get-StepScript $step
            if (-not $body) { continue }
            foreach ($needle in 'az servicebus', 'Configure-RabbitMqShovel', 'Enable-ServiceBusSecureTrigger', 'Test-WwShovelBridgeDelivery', 'ShovelScriptPath') {
                if ($body -match [regex]::Escape($needle)) { $offenders += "[$($step['displayName'])] mentions '$needle'" }
            }
        }
        $offenders | Should -BeNullOrEmpty
    }

    # Build 30843: the old shovel-delete step 401'd against the self-hosted broker. That broker (and
    # its ngrok tunnel) exists only for the UAT ShovelBridge; this file must not depend on it at all.
    Context 'no dependency on the self-hosted broker' {
        It 'has no step whose script or env reaches rabbitmq.warewolf.online or its credential' {
            $offenders = @()
            foreach ($step in (Get-Steps $script:Dev)) {
                $text = (Get-StepScript $step) + ' ' + (($step['env'] ?? @{}).Values -join ' ') + ' ' + (($step['inputs'] ?? @{}).Values -join ' ')
                foreach ($needle in 'rabbitmq.warewolf.online', 'ExternalRabbitMqPassword', 'testuser', '/api/shovels', '/api/parameters/shovel') {
                    if ($text -match [regex]::Escape($needle)) { $offenders += "[$($step['displayName'])] mentions '$needle'" }
                }
            }
            $offenders | Should -BeNullOrEmpty
        }

        It 'declares no self-hosted broker or shovel variables' {
            @($script:Dev['variables'].Keys | Where-Object { $_ -like 'ProfilerRabbitMq*' -or $_ -like 'ProfilerShovel*' }) |
                Should -BeNullOrEmpty
        }

        It 'points every broker variable at the CloudAMQP instance' {
            $script:Dev['variables']['ProfilerSinkRabbitMqHost']          | Should -BeLike '*.cloudamqp.com'
            $script:Dev['variables']['ProfilerSinkRabbitMqManagementUri'] | Should -BeLike 'https://*.cloudamqp.com'
        }

        It 'still requires the CloudAMQP secret, and no longer the self-hosted one, in the prerequisite check' {
            $pre = Find-Step $script:Dev 'Validate Dev QueueProcessor prerequisites'
            $pre['env']['CloudAmqpPasswordValue'] | Should -Be '$(CLOUDAMQP_SHOVEL_PASSWORD)'
            $pre['env'].Keys | Should -Not -Contain 'ExternalRabbitMqPasswordValue'
            (Get-StepScript $pre) | Should -Match 'CLOUDAMQP_SHOVEL_PASSWORD\s*='
        }
    }

    It 'runs the prerequisite validation straight after checkout, before any download or deploy' {
        # Step 0 is 'checkout: self'; nothing else may sit between it and the fail-fast checks.
        (Get-Steps $script:Dev)[0].Contains('checkout') | Should -BeTrue
        (Get-StepIndex $script:Dev 'Validate Dev QueueProcessor prerequisites') | Should -Be 1
        (Get-StepIndex $script:Dev 'Validate Dev QueueProcessor prerequisites') |
            Should -BeLessThan (Get-StepIndex $script:Dev 'Download Lightweight Engine Package')
    }

    # Build 30835: both were '' in the YAML. A YAML-declared variable shadows a UI/queue-time value
    # of the same name, so an empty one here can never be fixed from the pipeline definition.
    It 'declares ProfilerEngineEntraAppId as a GUID in the YAML itself' {
        $script:Dev['variables']['ProfilerEngineEntraAppId'] |
            Should -Match '^[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}$'
    }

    It 'declares ProfilerWorkerRabbitMqSecretUri as a Key Vault secret URI in the YAML itself' {
        $script:Dev['variables']['ProfilerWorkerRabbitMqSecretUri'] |
            Should -Match '^https://[^/]+\.vault\.azure\.net/secrets/[^/]+'
    }
}

Describe 'ProfilerService-CI-Dev pipeline - QueueProcessor deploy' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:WorkerDeploy = Find-Step $script:Dev 'Deploy QueueProcessor Container App (az acr build, DEV2/aca-warewolf)'
        $script:WorkerScript = Get-StepScript $script:WorkerDeploy
    }

    It 'has a QueueProcessor deploy step' {
        $script:WorkerDeploy | Should -Not -BeNullOrEmpty
    }

    It 'deploys the Dev trigger, with the worker config encrypted and the run judged by its summary' {
        $script:WorkerScript | Should -Match 'ProfilerDev\.trigger\.bite'
        $script:WorkerScript | Should -Match '-EncryptStagedSettings'
        $script:WorkerScript | Should -Match '-NonInteractive'
        $script:WorkerScript | Should -Match 'summary\.json'
        $script:WorkerScript | Should -Match "status -ne 'deployed'"
    }

    It 'never skips the engine app-role grant or runs as a dry run' {
        $script:WorkerScript | Should -Not -Match '-SkipEngineAppRoleGrant'
        $script:WorkerScript | Should -Not -Match '-DryRun'
    }

    It 'passes the KEDA broker secret through the environment, not as a literal in the script' {
        $script:WorkerScript | Should -Match '\$env:WorkerRabbitMqSecretUriValue'
        $script:WorkerScript | Should -Not -Match 'vault\.azure\.net'
    }

    It 'runs after the engine can authenticate the worker and after its queue exists' {
        $deploy = Get-StepIndex $script:Dev 'Deploy QueueProcessor Container App (az acr build, DEV2/aca-warewolf)'
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Verify engine Entra config (WAREWOLF_ENTRA_CONFIG)')
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Ensure worker queue + dead-letter topology exists (CloudAMQP)')
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Deploy via Deployment Scripts')
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Log in to Azure CLI (QueueProcessor steps)')
    }

    It 'checks the engine Entra config without ever printing it' {
        $check = Get-StepScript (Find-Step $script:Dev 'Verify engine Entra config (WAREWOLF_ENTRA_CONFIG)')
        $check | Should -Match 'WAREWOLF_ENTRA_CONFIG'
        # The value may carry a client secret: it may be tested and matched, but not written out.
        $check | Should -Not -Match 'Write-Host[^\r\n]*\$cfg'
    }

    It 'publishes the burst only after the worker is deployed, then verifies per message and at the sink' {
        $publish = Get-StepIndex $script:Dev 'Publish the burst ($(ProfilerBurstMessageCount) messages, RequestForProfiling shape)'
        $publish | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Deploy QueueProcessor Container App (az acr build, DEV2/aca-warewolf)')
        (Get-StepIndex $script:Dev 'Verify the QueueProcessor burst was processed') | Should -BeGreaterThan $publish
        (Get-StepIndex $script:Dev 'Verify results reached the sink (worker burst)') |
            Should -BeGreaterThan (Get-StepIndex $script:Dev 'Verify the QueueProcessor burst was processed')
    }

    It 'declares BOTH the trigger queue and its dead-letter queue durable, with an exchange binding' {
        $topology = Get-StepScript (Find-Step $script:Dev 'Ensure worker queue + dead-letter topology exists (CloudAMQP)')
        $topology | Should -Match "Confirm-QueueTopology '\$\(ProfilerWorkerQueueName\)'"
        $topology | Should -Match "Confirm-QueueTopology '\$\(ProfilerWorkerDeadLetterQueueName\)'"
        $topology | Should -Match '"durable":true'
        $topology | Should -Match 'api/bindings/'
    }
}

Describe 'ProfilerDev.trigger.bite' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:Trigger = Get-Content -LiteralPath $script:TriggerPath -Raw | ConvertFrom-Json
        $script:LoadTest = Get-Content -LiteralPath $script:LoadTestTriggerPath -Raw | ConvertFrom-Json
    }

    It 'is valid JSON with no unsubstituted release tokens' {
        (Get-Content -LiteralPath $script:TriggerPath -Raw) | Should -Not -Match '#\{'
        $script:Trigger.Name | Should -Be 'ProfilerDevQueue'
    }

    It 'scales to 20 replicas (Concurrency 20), one message per replica' {
        $script:Trigger.Concurrency | Should -Be 20
        $script:Trigger.Prefetch    | Should -Be 1
    }

    It 'has its own identity, distinct from the load-test trigger' {
        $script:Trigger.TriggerId | Should -Be $script:Trigger.Id
        $script:Trigger.TriggerId | Should -Not -Be $script:LoadTest.TriggerId
        $script:Trigger.ResourceId | Should -Not -Be $script:LoadTest.ResourceId
        [guid]::Parse($script:Trigger.TriggerId)  | Should -Not -BeNullOrEmpty
        [guid]::Parse($script:Trigger.ResourceId) | Should -Not -BeNullOrEmpty
    }

    It 'consumes the queue and dead-letter queue the pipeline declares' {
        $script:Trigger.QueueName       | Should -Be $script:Dev['variables']['ProfilerWorkerQueueName']
        $script:Trigger.DeadLetterQueue | Should -Be $script:Dev['variables']['ProfilerWorkerDeadLetterQueueName']
        $script:Trigger.DeadLetterQueue | Should -Not -Be $script:Trigger.QueueName
    }

    It 'uses the generated CloudAMQP source for both the queue and the dead-letter queue' {
        $script:Trigger.QueueSourceId | Should -Be '0b142714-8f6d-41b7-9832-2aefa8c731ec'
        $script:Trigger.QueueSinkId   | Should -Be $script:Trigger.QueueSourceId
        # ...and that id is the one the pipeline actually generates a source for.
        (Get-Content -LiteralPath $script:DevPath -Raw) | Should -Match ([regex]::Escape('<Source ID="0b142714-8f6d-41b7-9832-2aefa8c731ec"'))
    }

    It 'declares both queues durable, matching how the pipeline declares them' {
        ($script:Trigger.Options | Where-Object Name -eq 'Durable').Value            | Should -BeTrue
        ($script:Trigger.DeadLetterOptions | Where-Object Name -eq 'Durable').Value  | Should -BeTrue
    }

    It 'runs the same workflow as the smoke test' {
        $script:Trigger.WorkflowName.Replace('\', '/') | Should -Be $script:Dev['variables']['ProfilerSmokeWorkflowRoute']
    }

    It 'binds exactly the 16 inputs the smoke test sends' {
        $smoke = Get-StepScript (Find-Step $script:Dev 'Smoke Test: Execute Profiler Workflow')
        $block = [regex]::Match($smoke, '\$inputs = \[ordered\]@\{(?<body>.*?)\r?\n\s*\}', 'Singleline').Groups['body'].Value
        $smokeNames = @([regex]::Matches($block, '(?m)^\s*(\w+)\s*=') | ForEach-Object { $_.Groups[1].Value }) | Sort-Object
        $smokeNames.Count | Should -Be 16
        (@($script:Trigger.Inputs | ForEach-Object Name) | Sort-Object) -join ',' | Should -Be ($smokeNames -join ',')
    }

    It 'maps every input to a message field of the same name (flat body, MapEntireMessage false)' {
        $script:Trigger.MapEntireMessage | Should -BeFalse
        foreach ($i in $script:Trigger.Inputs) { $i.Value | Should -Be $i.Name }
    }
}
