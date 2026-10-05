<#
    Pester 5 suite for the two Profiler Lightweight-engine pipelines and their queue triggers:

        ProfilerService-CI-Dev-LightweightExecutionEngine.yml   + ProfilerDev.trigger.bite
        ProfilerService-CI-UAT-LightweightExecutionEngine.yml   + ProfilerUat.trigger.bite

        Invoke-Pester -Path ./ProfilerService-CI-Dev-LightweightExecutionEngine.Tests.ps1

    A pipeline file cannot be run locally, so this suite checks what CAN be proven without Azure:

      * the YAML parses, and every embedded PowerShell script is syntactically valid (a bad
        script otherwise fails only after the agent has spent minutes on the steps before it);
      * both files take the 'queueArchitecture' parameter (ShovelBridge | QueueProcessor) and share
        ONE body: everything from 'jobs:' down is byte-identical, so the environments differ only
        in their variables - their own engine, queues, Service Bus namespace, shovel and workers;
      * each architecture talks only to its own broker (ShovelBridge: the self-hosted broker;
        QueueProcessor: CloudAMQP) and tears the other architecture down;
      * the ShovelBridge's Service Bus dead-letters are forwarded to an error queue and counted;
      * each trigger file is consistent with the pipeline that deploys it.

    Steps inside a '${{ if eq(parameters.queueArchitecture, ...) }}' block are flattened into one
    list, tagged with their architecture; a step outside any block runs for both.

    Requires the powershell-yaml module. The whole suite is skipped (not failed) without it.
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
    $script:Environments = @(
        @{ Env = 'DEV'; File = 'ProfilerService-CI-Dev-LightweightExecutionEngine.yml'; Trigger = 'ProfilerDev.trigger.bite' }
        @{ Env = 'UAT'; File = 'ProfilerService-CI-UAT-LightweightExecutionEngine.yml'; Trigger = 'ProfilerUat.trigger.bite' }
    )
}

BeforeAll {
    $script:DevPath     = Join-Path $PSScriptRoot 'ProfilerService-CI-Dev-LightweightExecutionEngine.yml'
    $script:UatPath     = Join-Path $PSScriptRoot 'ProfilerService-CI-UAT-LightweightExecutionEngine.yml'
    $script:LoadTestTriggerPath = Join-Path $PSScriptRoot 'ProfilerLoadTest.trigger.bite'

    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        $script:Docs = @{
            'ProfilerService-CI-Dev-LightweightExecutionEngine.yml' = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath $script:DevPath -Raw)
            'ProfilerService-CI-UAT-LightweightExecutionEngine.yml' = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath $script:UatPath -Raw)
        }
        $script:Dev = $script:Docs['ProfilerService-CI-Dev-LightweightExecutionEngine.yml']
        $script:Uat = $script:Docs['ProfilerService-CI-UAT-LightweightExecutionEngine.yml']
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

    # Every step in order, '${{ if }}' blocks flattened. Each item: Step, Architecture ($null = both).
    function script:Get-TaggedSteps {
        param($Doc)
        foreach ($item in @($Doc['jobs'][0]['steps'])) {
            $key = @($item.Keys)[0]
            if ($key -match "^\$\{\{\s*if eq\(parameters\.queueArchitecture, '(\w+)'\)\s*\}\}$") {
                foreach ($s in @($item[$key])) { [pscustomobject]@{ Step = $s; Architecture = $Matches[1] } }
            } else {
                [pscustomobject]@{ Step = $item; Architecture = $null }
            }
        }
    }

    # The steps that run for -Architecture (shared + that block's), or every step when omitted.
    function script:Get-Steps {
        param($Doc, [string] $Architecture)
        @(Get-TaggedSteps $Doc | Where-Object { -not $Architecture -or -not $_.Architecture -or $_.Architecture -eq $Architecture } | ForEach-Object Step)
    }

    function script:Get-ArchitectureSteps {
        param($Doc, [string] $Architecture)
        @(Get-TaggedSteps $Doc | Where-Object { $_.Architecture -eq $Architecture } | ForEach-Object Step)
    }

    function script:Find-Step {
        param($Doc, [string] $DisplayName)
        Get-Steps $Doc | Where-Object { $_['displayName'] -eq $DisplayName } | Select-Object -First 1
    }

    # Index in the run order of -Architecture; -1 when the step does not run there.
    function script:Get-StepIndex {
        param($Doc, [string] $DisplayName, [string] $Architecture)
        $steps = @(Get-Steps $Doc $Architecture)
        for ($i = 0; $i -lt $steps.Count; $i++) { if ($steps[$i]['displayName'] -eq $DisplayName) { return $i } }
        return -1
    }

    # Everything a step could reach a broker or a secret through: script, env and task inputs.
    function script:Get-StepText {
        param($Step)
        (Get-StepScript $Step) + ' ' + (($Step['env'] ?? @{}).Values -join ' ') + ' ' + (($Step['inputs'] ?? @{}).Values -join ' ')
    }

    # The -Flag names on a Deploy-WwExecutionEngine.ps1 'arguments' string, sorted, values dropped.
    function script:Get-FlagNames {
        param([string] $Arguments)
        [regex]::Matches($Arguments, '(?<![\w-])-([A-Za-z][A-Za-z0-9]*)') |
            ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
    }

    function script:Get-Body([string] $Path) {
        $text = Get-Content -LiteralPath $Path -Raw
        $text.Substring($text.IndexOf("`njobs:"))
    }
}

Describe 'Profiler <Env> pipeline - structure' -ForEach $script:Environments -Skip:(-not $script:HaveYaml) {

    BeforeAll { $script:Doc = $script:Docs[$File] }

    It 'parses as a single-job, manually-triggered pipeline' {
        $script:Doc['trigger'] | Should -Be 'none'
        @($script:Doc['jobs']).Count | Should -Be 1
        (Get-Steps $script:Doc).Count | Should -BeGreaterThan 40
    }

    It 'takes queueArchitecture (ShovelBridge | QueueProcessor), ShovelBridge by default' {
        $param = @($script:Doc['parameters']) | Where-Object { $_['name'] -eq 'queueArchitecture' }
        $param | Should -Not -BeNullOrEmpty
        $param['default'] | Should -Be 'ShovelBridge'
        (@($param['values']) | Sort-Object) -join ',' | Should -Be 'QueueProcessor,ShovelBridge'
    }

    It 'has a block of steps for each architecture and no other conditional blocks' {
        $tags = @(Get-TaggedSteps $script:Doc | ForEach-Object Architecture | Where-Object { $_ } | Sort-Object -Unique)
        $tags -join ',' | Should -Be 'QueueProcessor,ShovelBridge'
        foreach ($item in @($script:Doc['jobs'][0]['steps'])) {
            $key = @($item.Keys)[0]
            if ($key -like '${{*') { $key | Should -Match "^\$\{\{ if eq\(parameters\.queueArchitecture, '(ShovelBridge|QueueProcessor)'\) \}\}$" }
        }
    }

    It 'has a unique displayName on every step (the tests below look steps up by name)' {
        $names = @(Get-Steps $script:Doc | Where-Object { -not $_.Contains('checkout') } | ForEach-Object { $_['displayName'] })
        ($names | Where-Object { -not $_ }).Count | Should -Be 0
        ($names | Group-Object | Where-Object Count -gt 1 | ForEach-Object Name) | Should -BeNullOrEmpty
    }

    It 'every embedded script is syntactically valid PowerShell' {
        $failures = @()
        $parsed = 0
        foreach ($step in (Get-Steps $script:Doc)) {
            $body = Get-StepScript $step
            if (-not $body) { continue }
            $parsed++
            # Azure DevOps expands $(name) and ${{ ... }} before the script runs; stand a token in for
            # each. A sub-expression '$( ... )' or '$($x)' does not match the name pattern and is left alone.
            $expanded = [regex]::Replace($body, '\$\([A-Za-z_][A-Za-z0-9_.]*\)', 'MACRO')
            $expanded = [regex]::Replace($expanded, '\$\{\{[^}]*\}\}', 'TEMPLATE')
            $tokens = $null; $errors = $null
            [void][System.Management.Automation.Language.Parser]::ParseInput($expanded, [ref]$tokens, [ref]$errors)
            foreach ($e in $errors) { $failures += "[$($step['displayName'])] line $($e.Extent.StartLineNumber): $($e.Message)" }
        }
        # Guard against a vacuous pass: a step-shape change that made Get-StepScript return $null for
        # everything would otherwise leave nothing to fail.
        $parsed | Should -BeGreaterThan 35
        $failures | Should -BeNullOrEmpty
    }

    It 'runs the prerequisite validation straight after checkout, before any download or deploy' {
        (Get-Steps $script:Doc)[0].Contains('checkout') | Should -BeTrue
        (Get-StepIndex $script:Doc 'Validate pipeline prerequisites') | Should -Be 1
        (Get-StepIndex $script:Doc 'Verify trigger file matches pipeline variables') | Should -Be 2
        (Get-StepIndex $script:Doc 'Preflight: Warewolf infrastructure') |
            Should -BeLessThan (Get-StepIndex $script:Doc 'Download Lightweight Engine Package')
    }

    It 'ends the infrastructure preflight by resetting LASTEXITCODE, so a tolerated az failure cannot fail the step' {
        # 'az servicebus queue show' on a not-yet-created queue leaves $LASTEXITCODE at 1, and AzureCLI@2
        # exits with $LASTEXITCODE: build 30932 reported every check as fine, then failed the step.
        $body = Get-StepScript (Find-Step $script:Doc 'Preflight: Warewolf infrastructure')
        $body = [regex]::Replace($body, '\$\([A-Za-z_][A-Za-z0-9_.]*\)', 'MACRO')
        $body = [regex]::Replace($body, '\$\{\{[^}]*\}\}', 'TEMPLATE')
        $tokens = $null; $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseInput($body, [ref]$tokens, [ref]$errors)
        $last = @($ast.EndBlock.Statements)[-1]
        $last.Extent.Text | Should -Be '$global:LASTEXITCODE = 0'
    }
}

Describe 'Profiler pipelines - syntax check sanity' {
    It 'the syntax check itself detects a broken script' {
        $tokens = $null; $errors = $null
        [void][System.Management.Automation.Language.Parser]::ParseInput('if ($x { Write-Host "unbalanced"', [ref]$tokens, [ref]$errors)
        $errors.Count | Should -BeGreaterThan 0
    }
}

Describe 'Profiler pipelines - Dev and UAT share one body, with their own resources' -Skip:(-not $script:HaveYaml) {

    It 'are byte-identical from jobs: down, so the environments can only differ in their variables' {
        $dev = Get-Body $script:DevPath
        $uat = Get-Body $script:UatPath
        $dev.Length | Should -BeGreaterThan 50000
        $uat | Should -BeExactly $dev
    }

    It 'take the same parameters' {
        ($script:Uat['parameters'] | ConvertTo-Json -Depth 5) | Should -Be ($script:Dev['parameters'] | ConvertTo-Json -Depth 5)
    }

    It 'declare the same variable names' {
        (@($script:Uat['variables'].Keys) | Sort-Object) -join ',' | Should -Be ((@($script:Dev['variables'].Keys) | Sort-Object) -join ',')
    }

    It 'give each environment its own <_>' -ForEach @(
        'ProfilerEnvironment', 'ProfilerEngineAppName', 'ProfilerEngineStorageAccount', 'ProfilerQueueTriggerFile',
        'ProfilerQueueName', 'ProfilerDeadLetterQueueName', 'ProfilerServiceBusNamespace',
        'ProfilerServiceBusTriggerQueueName', 'ProfilerServiceBusErrorQueueName', 'ProfilerShovelName', 'ProfilerWorkerAppNamePrefix') {
        $script:Uat['variables'][$_] | Should -Not -Be $script:Dev['variables'][$_] -Because "$_ is per environment"
    }

    It 'keep the rest of the variables identical' {
        $own = 'ProfilerEnvironment', 'ProfilerEngineAppName', 'ProfilerEngineStorageAccount', 'ProfilerQueueTriggerFile',
               'ProfilerQueueName', 'ProfilerDeadLetterQueueName', 'ProfilerServiceBusNamespace',
               'ProfilerServiceBusTriggerQueueName', 'ProfilerServiceBusErrorQueueName', 'ProfilerShovelName', 'ProfilerWorkerAppNamePrefix'
        foreach ($name in @($script:Dev['variables'].Keys | Where-Object { $_ -notin $own })) {
            $script:Uat['variables'][$name] | Should -Be $script:Dev['variables'][$name] -Because "$name is shared"
        }
    }

    It 'never let one environment''s worker prefix match the other''s apps (ShovelBridge stops by prefix)' {
        $dev = $script:Dev['variables']['ProfilerWorkerAppNamePrefix']
        $uat = $script:Uat['variables']['ProfilerWorkerAppNamePrefix']
        $dev.StartsWith($uat) | Should -BeFalse
        $uat.StartsWith($dev) | Should -BeFalse
    }

    It 'keep the UAT engine off the Dev engine' {
        $script:Dev['variables']['ProfilerEngineAppName'] | Should -Be 'wwprofilerlw'
        $script:Uat['variables']['ProfilerEngineAppName'] | Should -Be 'wwprofileruatlw'
        # Storage account names: 3-24 lowercase letters and digits.
        $script:Uat['variables']['ProfilerEngineStorageAccount'] | Should -Match '^[a-z0-9]{3,24}$'
    }
}

Describe 'Profiler <Env> pipeline - per-environment queues' -ForEach $script:Environments -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:Doc  = $script:Docs[$File]
        $script:Vars = $script:Doc['variables']
        $script:Trig = Get-Content -LiteralPath (Join-Path $PSScriptRoot $Trigger) -Raw | ConvertFrom-Json
    }

    It 'names the queues profiler.profile.request.<Env> / profiler.profile.error.request.<Env>' {
        $script:Vars['ProfilerEnvironment']         | Should -Be $Env
        $script:Vars['ProfilerQueueName']           | Should -Be "profiler.profile.request.$Env"
        $script:Vars['ProfilerDeadLetterQueueName'] | Should -Be "profiler.profile.error.request.$Env"
    }

    It 'gives the Service Bus trigger and error queues the same names' {
        $script:Vars['ProfilerServiceBusTriggerQueueName'] | Should -Be $script:Vars['ProfilerQueueName']
        $script:Vars['ProfilerServiceBusErrorQueueName']   | Should -Be $script:Vars['ProfilerDeadLetterQueueName']
    }

    It 'points at its own trigger file, whose queues match the variables' {
        $script:Vars['ProfilerQueueTriggerFile'] | Should -BeLike "*\Dev\.azure\$Trigger"
        $script:Trig.QueueName       | Should -Be $script:Vars['ProfilerQueueName']
        $script:Trig.DeadLetterQueue | Should -Be $script:Vars['ProfilerDeadLetterQueueName']
    }

    It 'passes the Service Bus trigger queue to the engine deploy, so its binding indexes under both architectures' {
        $deploy = Find-Step $script:Doc 'Deploy via Deployment Scripts'
        $deploy['inputs']['arguments'] | Should -Match "-ServiceBusTriggerQueueName '\`$\(ProfilerServiceBusTriggerQueueName\)'"
        $deploy['inputs']['arguments'] | Should -Match '-ServiceBusTriggerWorkflowName'
        (Get-TaggedSteps $script:Doc | Where-Object { $_.Step['displayName'] -eq 'Deploy via Deployment Scripts' }).Architecture | Should -BeNullOrEmpty
    }

    It 'checks the trigger file against the variables and the environment suffix' {
        $check = Get-StepScript (Find-Step $script:Doc 'Verify trigger file matches pipeline variables')
        $check | Should -Match "'\`$\(ProfilerQueueTriggerFile\)'"
        $check | Should -Match 'ProfilerServiceBusTriggerQueueName'
        $check | Should -Match 'ProfilerServiceBusErrorQueueName'
        $check | Should -Match '\.\$\(ProfilerEnvironment\)'
    }

    It 'no longer uses the retired single-environment queue names or variables' {
        $values = @($script:Vars.Values)
        foreach ($old in 'wwprofilerlw-trigger-queue', 'wwprofilerlw.servicebus.trigger.request', 'profiler.profile.request', 'profiler.profile.error.request') {
            $values | Should -Not -Contain $old
        }
        $body = Get-Body (Join-Path $PSScriptRoot $File)
        foreach ($old in 'ProfilerShovelSourceQueueName', 'ProfilerWorkerQueueName', 'ProfilerWorkerDeadLetterQueueName') {
            $script:Vars.Keys | Should -Not -Contain $old
            $body | Should -Not -Match ([regex]::Escape($old))
        }
    }
}

Describe 'Profiler pipelines - engine deploy' -Skip:(-not $script:HaveYaml) {

    It 'never provisions Easy Auth or repairs Entra config through the deploy (the SP has no Graph rights)' {
        $args = (Find-Step $script:Dev 'Deploy via Deployment Scripts')['inputs']['arguments']
        $args | Should -Match '-SkipAuthProvisioning'
        $args | Should -Match '-SkipEntraConfigDriftRepair'
    }

    It 'keeps the four throughput flags together (they revert silently when dropped)' {
        $flags = @(Get-FlagNames (Find-Step $script:Dev 'Deploy via Deployment Scripts')['inputs']['arguments'])
        foreach ($f in 'ServiceBusMaxConcurrentCalls', 'ServiceBusTriggerMaxConcurrentExecutions', 'DisableDynamicConcurrency', 'FlexMaximumInstanceCount') {
            $flags | Should -Contain $f
        }
        $flags.Count | Should -BeGreaterThan 15
    }

    It 'disables Easy Auth after the deploy, before the smoke test, the bridge and the worker' {
        foreach ($arch in 'ShovelBridge', 'QueueProcessor') {
            $step = Get-StepIndex $script:Dev 'Ensure Easy Auth is disabled on the engine' $arch
            $step | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Deploy via Deployment Scripts' $arch)
            $step | Should -BeLessThan (Get-StepIndex $script:Dev 'Smoke Test: Execute Profiler Workflow' $arch)
        }
        (Get-StepIndex $script:Dev 'Ensure Easy Auth is disabled on the engine' 'QueueProcessor') |
            Should -BeLessThan (Get-StepIndex $script:Dev 'Deploy QueueProcessor Container App (az acr build, DEV2/aca-warewolf)' 'QueueProcessor')
    }

    It 'turns platform.enabled off through authsettingsV2 and verifies it after the write' {
        $body = Get-StepScript (Find-Step $script:Dev 'Ensure Easy Auth is disabled on the engine')
        $body | Should -Match 'config/authsettingsV2'
        $body | Should -Match 'platform\.enabled = \$false'
        $body | Should -Match 'az rest --method put'
        $body | Should -Match 'still enabled'
    }

    It 'downloads only the chosen architecture''s orchestrators' {
        $dl = Get-StepScript (Find-Step $script:Dev 'Download Warewolf Deployment Scripts')
        $dl | Should -Match "'\`$\{\{ parameters\.queueArchitecture \}\}' -eq 'ShovelBridge'"
        foreach ($s in 'Configure-RabbitMqShovel.ps1', 'Enable-ServiceBusSecureTrigger.ps1', 'Deploy-WwQueueProcessor.ps1') { $dl | Should -Match ([regex]::Escape($s)) }
        (Find-Step $script:Dev 'Download QueueProcessor Package + stage broker source') | Should -Not -BeNullOrEmpty
        (Get-StepIndex $script:Dev 'Download QueueProcessor Package + stage broker source' 'ShovelBridge') | Should -Be -1
    }
}

Describe 'Profiler pipelines - prerequisites per architecture' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:Pre = Find-Step $script:Dev 'Validate pipeline prerequisites'
        $script:PreScript = Get-StepScript $script:Pre
    }

    It 'always requires both broker secrets: CloudAMQP (the sink) and the self-hosted broker (the shovel)' {
        $script:Pre['env']['CloudAmqpPasswordValue']        | Should -Be '$(CLOUDAMQP_SHOVEL_PASSWORD)'
        $script:Pre['env']['ExternalRabbitMqPasswordValue'] | Should -Be '$(ExternalRabbitMqPassword)'
        $always = [regex]::Match($script:PreScript, '\$secrets = \[ordered\]@\{(?<b>.*?)\}', 'Singleline').Groups['b'].Value
        $always | Should -Match 'CLOUDAMQP_SHOVEL_PASSWORD\s*='
        $always | Should -Match 'ExternalRabbitMqPassword\s*='
    }

    It 'requires the SP secrets and the KEDA Key Vault secret URI only for QueueProcessor' {
        $qp = [regex]::Match($script:PreScript, "if \(\`$architecture -eq 'QueueProcessor'\) \{(?<b>.*?)\r?\n\}", 'Singleline').Groups['b'].Value
        $qp | Should -Match "AzureClientId"
        $qp | Should -Match "AzureClientSecret"
        $qp | Should -Match "AzureTenantId"
        $qp | Should -Match 'ProfilerWorkerRabbitMqSecretUri'
    }

    It 'declares ProfilerWorkerRabbitMqSecretUri as a Key Vault secret URI in the YAML itself' {
        $script:Dev['variables']['ProfilerWorkerRabbitMqSecretUri'] | Should -Match '^https://[^/]+\.vault\.azure\.net/secrets/[^/]+'
    }

    It 'reports the chosen architecture''s infrastructure read-only, failing only on what it cannot create' {
        $pre = Get-StepScript (Find-Step $script:Dev 'Preflight: Warewolf infrastructure')
        $pre | Should -Match 'az resource list'
        $pre | Should -Not -Match 'az \w+ (create|update|delete|set)'
        $pre | Should -Match "Find-Resource 'Microsoft.KeyVault/vaults' '\`$\(ProfilerEngineKeyVaultName\)'\) \`$true"
        $pre | Should -Match "Find-Resource 'Microsoft.App/managedEnvironments' '\`$\(ProfilerAcaEnvironment\)'\) \`$true"
        $pre | Should -Match 'ProfilerServiceBusErrorQueueName'
    }
}

Describe 'Profiler pipelines - each architecture uses its own broker' -Skip:(-not $script:HaveYaml) {

    It 'ShovelBridge steps never reach the QueueProcessor or its CloudAMQP trigger queue' {
        $offenders = @()
        foreach ($step in (Get-ArchitectureSteps $script:Dev 'ShovelBridge')) {
            $text = Get-StepText $step
            foreach ($needle in 'Deploy-WwQueueProcessor', 'QueueProcessorDeployScriptPath', 'ProfilerWorkerRabbitMqSecretUri', 'az login') {
                if ($text -match [regex]::Escape($needle)) { $offenders += "[$($step['displayName'])] mentions '$needle'" }
            }
            # CloudAMQP is the result SINK only: the trigger queue is never declared or published there.
            if ($text -match 'ProfilerSinkRabbitMqManagementUri' -and $text -match '\$\(ProfilerQueueName\)') { $offenders += "[$($step['displayName'])] touches the queue on CloudAMQP" }
        }
        $offenders | Should -BeNullOrEmpty
    }

    It 'QueueProcessor steps never provision or exercise the bridge' {
        $offenders = @()
        foreach ($step in (Get-ArchitectureSteps $script:Dev 'QueueProcessor')) {
            $body = Get-StepScript $step
            if (-not $body) { continue }
            foreach ($needle in 'az servicebus', 'Configure-RabbitMqShovel', 'Enable-ServiceBusSecureTrigger', 'Test-WwShovelBridgeDelivery', 'ShovelScriptPath') {
                if ($body -match [regex]::Escape($needle)) { $offenders += "[$($step['displayName'])] mentions '$needle'" }
            }
        }
        $offenders | Should -BeNullOrEmpty
    }

    It 'QueueProcessor reaches the self-hosted broker only to delete the shovel' {
        $offenders = @()
        foreach ($step in (Get-ArchitectureSteps $script:Dev 'QueueProcessor')) {
            if ($step['displayName'] -eq 'Remove RabbitMQ Shovel (QueueProcessor owns the engine)') { continue }
            $text = Get-StepText $step
            foreach ($needle in 'ProfilerRabbitMqManagementUri', 'ExternalRabbitMqPassword', 'rabbitmq.warewolf.online') {
                if ($text -match [regex]::Escape($needle)) { $offenders += "[$($step['displayName'])] mentions '$needle'" }
            }
        }
        $offenders | Should -BeNullOrEmpty
    }

    It 'shared steps reach neither architecture''s trigger broker (bar the two up-front checks, which cover both)' {
        $offenders = @()
        $checks = 'Validate pipeline prerequisites', 'Preflight: Warewolf infrastructure'
        foreach ($t in @(Get-TaggedSteps $script:Dev | Where-Object { -not $_.Architecture -and $_.Step['displayName'] -notin $checks })) {
            $text = Get-StepText $t.Step
            foreach ($needle in 'ProfilerRabbitMqManagementUri', 'ExternalRabbitMqPassword', 'az servicebus', 'az containerapp') {
                if ($text -match [regex]::Escape($needle)) { $offenders += "[$($t.Step['displayName'])] mentions '$needle'" }
            }
        }
        $offenders | Should -BeNullOrEmpty
    }

    It 'points the sink at CloudAMQP and the shovel source at the self-hosted broker' {
        $script:Dev['variables']['ProfilerSinkRabbitMqHost']          | Should -BeLike '*.cloudamqp.com'
        $script:Dev['variables']['ProfilerSinkRabbitMqManagementUri'] | Should -BeLike 'https://*.cloudamqp.com'
        $script:Dev['variables']['ProfilerRabbitMqManagementUri']     | Should -Be 'https://rabbitmq.warewolf.online'
        $script:Dev['variables']['ProfilerShovelSourceHost']          | Should -Be 'localhost'
    }
}

Describe 'Profiler pipelines - each architecture tears the other down' -Skip:(-not $script:HaveYaml) {

    It 'ShovelBridge stops this environment''s workers (by prefix) before configuring the shovel' {
        $stop = 'Stop QueueProcessor workers (ShovelBridge owns the engine)'
        (Get-StepIndex $script:Dev $stop 'QueueProcessor') | Should -Be -1
        (Get-StepIndex $script:Dev $stop 'ShovelBridge') | Should -BeLessThan (Get-StepIndex $script:Dev 'Configure RabbitMQ Shovel (bridge to Azure Service Bus)' 'ShovelBridge')
        $body = Get-StepScript (Find-Step $script:Dev $stop)
        $body | Should -Match "starts_with\(name, '\`$prefix'\)"
        $body | Should -Match "\`$prefix = '\`$\(ProfilerWorkerAppNamePrefix\)'"
        # The CLI has no 'az containerapp stop' (build 30925): the ARM stop action is POSTed, and it is
        # long-running, so the step must wait for 'Stopped' rather than check once.
        $code = (($body -split "`n") | Where-Object { $_ -notmatch '^\s*#' }) -join "`n"
        $code | Should -Not -Match 'az containerapp stop'
        $code | Should -Match 'az rest --method post --url "https://management\.azure\.com\$id/stop\?api-version=\$apiVersion"'
        $code | Should -Match "if \(\`$after -eq 'Stopped'\) \{ break \}"
        $code | Should -Match "-ne 'Stopped'"
        $body | Should -Not -Match 'containerapp delete'
    }

    It 'QueueProcessor deletes this environment''s shovel before the worker is deployed, and confirms it is gone' {
        $remove = 'Remove RabbitMQ Shovel (QueueProcessor owns the engine)'
        (Get-StepIndex $script:Dev $remove 'ShovelBridge') | Should -Be -1
        (Get-StepIndex $script:Dev $remove 'QueueProcessor') |
            Should -BeLessThan (Get-StepIndex $script:Dev 'Deploy QueueProcessor Container App (az acr build, DEV2/aca-warewolf)' 'QueueProcessor')
        $step = Find-Step $script:Dev $remove
        $body = Get-StepScript $step
        $body | Should -Match "-Method Delete -Uri `"\`$mgmtUri/api/parameters/shovel/"
        $body | Should -Match "\`$name     = '\`$\(ProfilerShovelName\)'"
        $body | Should -Match '-eq 404'
        $body | Should -Match '/api/shovels/'
        $step['env']['RabbitMqPasswordValue'] | Should -Be '$(ExternalRabbitMqPassword)'
    }

    It 'QueueProcessor starts a worker an earlier ShovelBridge run stopped, without leaking the az alias into the deploy script' {
        $body = Get-StepScript (Find-Step $script:Dev 'Deploy QueueProcessor Container App (az acr build, DEV2/aca-warewolf)')
        $body | Should -Match "-eq 'Stopped'"
        # No 'az containerapp start' exists either (build 30925): POST the ARM action, then wait for 'Running'.
        $code = (($body -split "`n") | Where-Object { $_ -notmatch '^\s*#' }) -join "`n"
        $code | Should -Not -Match 'az containerapp start'
        $code | Should -Match 'az rest --method post --url "https://management\.azure\.com\$id/start\?api-version='
        $code | Should -Match "if \(\`$status -eq 'Running'\) \{ break \}"
        $code | Should -Match "-ne 'Running'"
        $body.IndexOf('Import-Module') | Should -BeGreaterThan $body.IndexOf("& '`$(QueueProcessorDeployScriptPath)'")
    }
}

Describe 'Profiler pipelines - ShovelBridge Service Bus error queue' -Skip:(-not $script:HaveYaml) {

    It 'creates the error queue before the trigger queue and forwards the trigger queue''s dead-letters to it' {
        $body = Get-StepScript (Find-Step $script:Dev 'Ensure ShovelBridge trigger queue, error queue + Send-only SAS rule exist')
        $body | Should -Match "\`$errorQueue   = '\`$\(ProfilerServiceBusErrorQueueName\)'"
        $body.IndexOf('queue create --name $errorQueue') | Should -BeLessThan $body.IndexOf('queue create --name $queue')
        $body | Should -Match '--forward-dead-lettered-messages-to \$errorQueue'
        $body | Should -Match 'forwards its dead-letters to .* after the update'
    }

    It 'checks the forward survived Enable-ServiceBusSecureTrigger.ps1' {
        $body = Get-StepScript (Find-Step $script:Dev 'Enable Service Bus Secure Trigger (queue + MI RBAC + listen connection)')
        $body.IndexOf('forwardDeadLetteredMessagesTo') | Should -BeGreaterThan $body.IndexOf("& '`$(StagedSecureTriggerScriptPath)'")
    }

    It 'fails the single-message bridge smoke test on a dead-letter forwarded to the error queue' {
        $capture  = Get-StepScript (Find-Step $script:Dev 'Smoke Test: Capture Sink Depth (bridge)')
        $delivery = Get-StepScript (Find-Step $script:Dev 'Smoke Test: ShovelBridge Delivery (RabbitMQ -> Service Bus -> engine)')
        $capture  | Should -Match 'variable=BridgeFailuresBefore'
        $delivery | Should -Match '\$\(BridgeFailuresBefore\)'
        $delivery | Should -Match 'was dead-lettered'
        foreach ($b in $capture, $delivery) {
            $b | Should -Match 'ProfilerServiceBusErrorQueueName'
            $b | Should -Match '--query countDetails -o json'
        }
    }

    It 'imports the az alias only after any child script in the bridge steps' {
        foreach ($name in 'Smoke Test: Capture Sink Depth (bridge)', 'Smoke Test: ShovelBridge Delivery (RabbitMQ -> Service Bus -> engine)', 'Capture Service Bus + sink baseline (before bridge burst)') {
            $body = Get-StepScript (Find-Step $script:Dev $name)
            $body.IndexOf('Import-Module') | Should -BeGreaterThan $body.LastIndexOf("& '") -Because $name
        }
    }
}

Describe 'Profiler pipelines - ShovelBridge burst' -Skip:(-not $script:HaveYaml) {

    It 'runs after the shovel is verified, in order: baseline, publish, drain, Service Bus check, sink check' {
        $order = 'Verify ShovelBridge shovel is running',
                 'Smoke Test: Verify Sink Queue (bridge)',
                 'Capture Service Bus + sink baseline (before bridge burst)',
                 'Publish the bridge burst ($(ProfilerBurstMessageCount) messages, RequestForProfiling shape)',
                 'Wait for the bridge burst to drain',
                 'Verify the bridge burst was processed (Service Bus)',
                 'Verify results reached the sink (bridge burst)'
        $idx = @($order | ForEach-Object { Get-StepIndex $script:Dev $_ 'ShovelBridge' })
        $idx | Should -Not -Contain -1
        for ($i = 1; $i -lt $idx.Count; $i++) { $idx[$i] | Should -BeGreaterThan $idx[$i - 1] -Because $order[$i] }
        (Get-StepIndex $script:Dev $order[3] 'QueueProcessor') | Should -Be -1
    }

    It 'publishes RequestForProfiling-shaped messages to the source queue''s exchange on the self-hosted broker, counting only routed ones' {
        $step = Find-Step $script:Dev 'Publish the bridge burst ($(ProfilerBurstMessageCount) messages, RequestForProfiling shape)'
        $body = Get-StepScript $step
        $body | Should -Match 'Publish-WwQueueBurst\.ps1'' -LoadFunctionsOnly'
        $body | Should -Match "-BodyShape 'Profiler'"
        $body | Should -Match "\`$\(ProfilerRabbitMqManagementUri\)/api/exchanges/"
        $body | Should -Match "\`$queue    = '\`$\(ProfilerQueueName\)'"
        $body | Should -Match '\$r\.routed'
        $body | Should -Match 'variable=BridgeBurstPublishedCount'
        $step['env']['RabbitMqPasswordValue'] | Should -Be '$(ExternalRabbitMqPassword)'
    }

    It 'waits on the source queue AND the Service Bus trigger queue' {
        $body = Get-StepScript (Find-Step $script:Dev 'Wait for the bridge burst to drain')
        $body | Should -Match 'ProfilerRabbitMqManagementUri'
        $body | Should -Match 'countDetails\.activeMessageCount'
        $body | Should -Match '\$pending = \[long\]\$source\.messages \+ \[long\]\$active'
    }

    It 'fails on any new Service Bus failure or anything left on the trigger queue' {
        $body = Get-StepScript (Find-Step $script:Dev 'Verify the bridge burst was processed (Service Bus)')
        $body | Should -Match '\$\(BridgeBurstFailuresBefore\)'
        $body | Should -Match 'if \(\$deadLettered -gt 0\) \{ throw'
        $body | Should -Match 'if \(\[long\]\$active -gt 0\) \{ throw'
        $body | Should -Match 'servicebus-counts\.json'
    }

    It 'expects at least every routed message at the sink' {
        $body = Get-StepScript (Find-Step $script:Dev 'Verify results reached the sink (bridge burst)')
        $body | Should -Match '\$\(BridgeBurstPublishedCount\)'
        $body | Should -Match '\$\(BridgeBurstSinkPublishedBefore\)'
        $body | Should -Match 'if \(\$delta -lt \[long\]\$expected\)'
    }
}

Describe 'Profiler pipelines - QueueProcessor deploy' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:WorkerName   = 'Deploy QueueProcessor Container App (az acr build, DEV2/aca-warewolf)'
        $script:WorkerDeploy = Find-Step $script:Dev $script:WorkerName
        $script:WorkerScript = Get-StepScript $script:WorkerDeploy
    }

    It 'deploys this environment''s trigger, with the worker config encrypted and the run judged by its summary' {
        $script:WorkerScript | Should -Match "-TriggerFilePath     '\`$\(ProfilerQueueTriggerFile\)'"
        $script:WorkerScript | Should -Match "-AppNamePrefix       '\`$\(ProfilerWorkerAppNamePrefix\)'"
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
        $deploy = Get-StepIndex $script:Dev $script:WorkerName 'QueueProcessor'
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Resolve engine Entra app id (WAREWOLF_ENTRA_CONFIG)' 'QueueProcessor')
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Ensure worker queue + dead-letter topology exists (CloudAMQP)' 'QueueProcessor')
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Deploy via Deployment Scripts' 'QueueProcessor')
        $deploy | Should -BeGreaterThan (Get-StepIndex $script:Dev 'Log in to Azure CLI (QueueProcessor steps)' 'QueueProcessor')
    }

    It 'does not hardcode ProfilerEngineEntraAppId, but resolves it from the engine''s WAREWOLF_ENTRA_CONFIG' {
        foreach ($doc in $script:Dev, $script:Uat) { $doc['variables'].Keys | Should -Not -Contain 'ProfilerEngineEntraAppId' }
        (Get-Content -LiteralPath $script:DevPath -Raw) | Should -Not -Match 'fd3359c7-13e1-4a6e-8ab2-2fe6c56df1f7'
        $resolve = Get-StepScript (Find-Step $script:Dev 'Resolve engine Entra app id (WAREWOLF_ENTRA_CONFIG)')
        $resolve | Should -Match 'WAREWOLF_ENTRA_CONFIG'
        $resolve | Should -Match '\.clientId'
        $resolve | Should -Match 'task\.setvariable variable=ProfilerEngineEntraAppId\]'
        # The value may carry a client secret: it may be parsed, but not written out.
        $resolve | Should -Not -Match 'Write-Host[^\r\n]*\$(cfg|entra)\b'
    }

    It 'passes the resolved Entra app id to the worker, refusing to deploy without one' {
        $script:WorkerScript | Should -Match "\`$engineAppId = '\`$\(ProfilerEngineEntraAppId\)'"
        $script:WorkerScript | Should -Match '-EngineResourceAppId \$engineAppId'
        $script:WorkerScript | Should -Match 'was not resolved'
    }

    It 'publishes the burst only after the worker is deployed, then verifies per message and at the sink' {
        $publish = Get-StepIndex $script:Dev 'Publish the burst ($(ProfilerBurstMessageCount) messages, RequestForProfiling shape)' 'QueueProcessor'
        $publish | Should -BeGreaterThan (Get-StepIndex $script:Dev $script:WorkerName 'QueueProcessor')
        (Get-StepIndex $script:Dev 'Verify the QueueProcessor burst was processed' 'QueueProcessor') | Should -BeGreaterThan $publish
        (Get-StepIndex $script:Dev 'Verify results reached the sink (worker burst)' 'QueueProcessor') |
            Should -BeGreaterThan (Get-StepIndex $script:Dev 'Verify the QueueProcessor burst was processed' 'QueueProcessor')
        (Get-StepScript (Find-Step $script:Dev 'Publish the burst ($(ProfilerBurstMessageCount) messages, RequestForProfiling shape)')) |
            Should -Match "-QueueName      '\`$\(ProfilerQueueName\)'"
    }

    It 'declares BOTH the trigger queue and its dead-letter queue durable, with an exchange binding' {
        $topology = Get-StepScript (Find-Step $script:Dev 'Ensure worker queue + dead-letter topology exists (CloudAMQP)')
        $topology | Should -Match "Confirm-QueueTopology '\$\(ProfilerQueueName\)'"
        $topology | Should -Match "Confirm-QueueTopology '\$\(ProfilerDeadLetterQueueName\)'"
        $topology | Should -Match '"durable":true'
        $topology | Should -Match 'api/bindings/'
    }
}

Describe '<Trigger>' -ForEach $script:Environments -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:TriggerFile = Join-Path $PSScriptRoot $Trigger
        $script:Trig     = Get-Content -LiteralPath $script:TriggerFile -Raw | ConvertFrom-Json
        $script:LoadTest = Get-Content -LiteralPath $script:LoadTestTriggerPath -Raw | ConvertFrom-Json
        $script:Doc      = $script:Docs[$File]
    }

    It 'is valid JSON with no unsubstituted release tokens' {
        (Get-Content -LiteralPath $script:TriggerFile -Raw) | Should -Not -Match '#\{'
        $script:Trig.Name | Should -Match '^Profiler(Dev|Uat)Queue$'
    }

    It 'scales to 20 replicas (Concurrency 20), one message per replica' {
        $script:Trig.Concurrency | Should -Be 20
        $script:Trig.Prefetch    | Should -Be 1
    }

    It 'has its own identity, distinct from the load-test trigger' {
        $script:Trig.TriggerId | Should -Be $script:Trig.Id
        $script:Trig.TriggerId | Should -Not -Be $script:LoadTest.TriggerId
        $script:Trig.ResourceId | Should -Not -Be $script:LoadTest.ResourceId
        [guid]::Parse($script:Trig.TriggerId)  | Should -Not -BeNullOrEmpty
        [guid]::Parse($script:Trig.ResourceId) | Should -Not -BeNullOrEmpty
    }

    It 'consumes the queue and dead-letter queue its pipeline declares' {
        $script:Trig.QueueName       | Should -Be $script:Doc['variables']['ProfilerQueueName']
        $script:Trig.DeadLetterQueue | Should -Be $script:Doc['variables']['ProfilerDeadLetterQueueName']
        $script:Trig.DeadLetterQueue | Should -Not -Be $script:Trig.QueueName
    }

    It 'uses the generated CloudAMQP source for both the queue and the dead-letter queue' {
        $script:Trig.QueueSourceId | Should -Be '0b142714-8f6d-41b7-9832-2aefa8c731ec'
        $script:Trig.QueueSinkId   | Should -Be $script:Trig.QueueSourceId
        # ...and that id is the one the pipeline actually generates a source for.
        (Get-Content -LiteralPath (Join-Path $PSScriptRoot $File) -Raw) | Should -Match ([regex]::Escape('<Source ID="0b142714-8f6d-41b7-9832-2aefa8c731ec"'))
    }

    It 'declares both queues durable, matching how the pipeline declares them' {
        ($script:Trig.Options | Where-Object Name -eq 'Durable').Value            | Should -BeTrue
        ($script:Trig.DeadLetterOptions | Where-Object Name -eq 'Durable').Value  | Should -BeTrue
    }

    It 'runs the same workflow as the smoke test' {
        $script:Trig.WorkflowName.Replace('\', '/') | Should -Be $script:Doc['variables']['ProfilerSmokeWorkflowRoute']
    }

    It 'binds exactly the 16 inputs the smoke test sends' {
        $smoke = Get-StepScript (Find-Step $script:Doc 'Smoke Test: Execute Profiler Workflow')
        $block = [regex]::Match($smoke, '\$inputs = \[ordered\]@\{(?<body>.*?)\r?\n\s*\}', 'Singleline').Groups['body'].Value
        $smokeNames = @([regex]::Matches($block, '(?m)^\s*(\w+)\s*=') | ForEach-Object { $_.Groups[1].Value }) | Sort-Object
        $smokeNames.Count | Should -Be 16
        (@($script:Trig.Inputs | ForEach-Object Name) | Sort-Object) -join ',' | Should -Be ($smokeNames -join ',')
    }

    It 'maps every input to a message field of the same name (flat body, MapEntireMessage false)' {
        $script:Trig.MapEntireMessage | Should -BeFalse
        foreach ($i in $script:Trig.Inputs) { $i.Value | Should -Be $i.Name }
    }
}

Describe 'Profiler trigger files - Dev and UAT are separate triggers' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:DevTrig = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ProfilerDev.trigger.bite') -Raw | ConvertFrom-Json
        $script:UatTrig = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ProfilerUat.trigger.bite') -Raw | ConvertFrom-Json
    }

    It 'differ in <_>' -ForEach @('TriggerId', 'ResourceId', 'Name', 'QueueName', 'DeadLetterQueue') {
        $script:UatTrig.$_ | Should -Not -Be $script:DevTrig.$_
    }

    It 'are otherwise the same trigger (workflow, scaling, source and inputs)' {
        $strip = { param($t) $c = $t | ConvertTo-Json -Depth 10 | ConvertFrom-Json
                   foreach ($p in 'Id', 'TriggerId', 'ResourceId', 'Name', 'QueueName', 'DeadLetterQueue') { $c.PSObject.Properties.Remove($p) }
                   $c | ConvertTo-Json -Depth 10 }
        (& $strip $script:UatTrig) | Should -Be (& $strip $script:DevTrig)
    }
}
