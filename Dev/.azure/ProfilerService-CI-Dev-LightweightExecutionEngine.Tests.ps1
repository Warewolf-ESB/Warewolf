<#
    Pester 5 suite for ProfilerService-CI-Dev-LightweightExecutionEngine.yml and
    ProfilerDev.trigger.bite.

        Invoke-Pester -Path ./ProfilerService-CI-Dev-LightweightExecutionEngine.Tests.ps1

    A pipeline file cannot be run locally, so this suite checks what CAN be proven without Azure:

      * the YAML parses, and every embedded PowerShell script is syntactically valid (a bad
        script otherwise fails only after the agent has spent minutes on the steps before it);
      * the Dev file deploys THE SAME ENGINE as the UAT file - same app, same deploy flags - so the
        two cannot drift into leaving the shared app in different states;
      * the Dev file no longer builds the ShovelBridge, and deletes (then verifies the absence of)
        the legacy shovel before anything is deployed;
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

Describe 'ProfilerService-CI-Dev pipeline - ShovelBridge is gone, and removed if present' -Skip:(-not $script:HaveYaml) {

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

    Context 'the shovel-disable step' {
        BeforeAll {
            $script:ShovelStepName = 'Disable the legacy ShovelBridge shovel (delete if present)'
            $script:ShovelStep     = Find-Step $script:Dev $script:ShovelStepName
            $script:ShovelScript   = Get-StepScript $script:ShovelStep
        }

        It 'exists and runs before anything is deployed' {
            $script:ShovelStep | Should -Not -BeNullOrEmpty
            (Get-StepIndex $script:Dev $script:ShovelStepName) |
                Should -BeLessThan (Get-StepIndex $script:Dev 'Deploy via Deployment Scripts')
        }

        It 'lists shovels read-only, then deletes via the parameters resource the shovel is created with' {
            # /api/shovels/... is the read-only status listing; a dynamic shovel is removed by clearing
            # its runtime parameter, the same resource Configure-RabbitMqShovel.ps1 PUTs to create it.
            $script:ShovelScript | Should -Match '/api/shovels/\$vhostEnc"'
            $script:ShovelScript | Should -Match '-Method Delete -Uri "\$mgmtUri/api/parameters/shovel/\$vhostEnc/'
            $script:ShovelScript | Should -Not -Match '-Method Delete -Uri "\$mgmtUri/api/shovels'
        }

        It 'only touches the one shovel named by ProfilerShovelName' {
            $script:ShovelScript | Should -Match '\$shovel\s*=\s*''\$\(ProfilerShovelName\)'''
            @([regex]::Matches($script:ShovelScript, '-Method Delete')).Count | Should -Be 1
            $script:ShovelScript | Should -Not -Match 'api/queues|api/exchanges|api/bindings'
        }

        It 'is a no-op when the shovel is already absent, and treats a 404 on the delete as already gone' {
            $script:ShovelScript | Should -Match 'nothing to delete'
            $script:ShovelScript | Should -Match '-eq 404'
        }

        It 'verifies the shovel is really gone, and fails the build if it is not' {
            $script:ShovelScript | Should -Match '\$deadline'
            $script:ShovelScript | Should -Match 'still listed'
            $script:ShovelScript | Should -Match 'throw'
        }

        It 'uses the self-hosted broker credential, not the CloudAMQP one' {
            $script:ShovelStep['env'].Keys | Should -Contain 'RabbitMqPasswordValue'
            $script:ShovelStep['env']['RabbitMqPasswordValue'] | Should -Be '$(ExternalRabbitMqPassword)'
        }
    }

    # Runs the step's REAL script against a local stand-in for the RabbitMQ management API, so the
    # behaviour (not just the text) is proven: which calls are made, in what order, and what fails.
    Context 'the shovel-disable step, executed against a fake management API' {
        BeforeAll {
            $script:ShovelName = $script:Dev['variables']['ProfilerShovelName']

            function script:Start-FakeBroker {
                # -Shovels: names listed initially. -DeleteStatus: what DELETE answers.
                # -DeleteRemoves: whether an accepted DELETE actually removes the shovel.
                param([string[]] $Shovels, [int] $DeleteStatus = 204, [bool] $DeleteRemoves = $true)

                $port = Get-Random -Minimum 20000 -Maximum 40000
                $state = [hashtable]::Synchronized(@{
                    Shovels = [System.Collections.ArrayList]@($Shovels)
                    DeleteStatus = $DeleteStatus; DeleteRemoves = $DeleteRemoves
                    Calls = [System.Collections.ArrayList]::new(); Stop = $false
                })
                $listener = [System.Net.HttpListener]::new()
                $listener.Prefixes.Add("http://localhost:$port/")
                $listener.Start()

                $ps = [powershell]::Create()
                [void]$ps.AddScript({
                    param($listener, $state)
                    while (-not $state.Stop) {
                        try { $ctx = $listener.GetContext() } catch { break }
                        $req = $ctx.Request; $res = $ctx.Response
                        [void]$state.Calls.Add("$($req.HttpMethod) $($req.RawUrl)")
                        $body = ''
                        if ($req.HttpMethod -eq 'GET' -and $req.RawUrl -like '/api/shovels/*') {
                            $items = @($state.Shovels | ForEach-Object { @{ name = $_; state = 'running'; vhost = '/' } })
                            $body = if ($items.Count -eq 0) { '[]' } else { ConvertTo-Json -InputObject $items -Compress }
                            $res.StatusCode = 200
                        } elseif ($req.HttpMethod -eq 'DELETE' -and $req.RawUrl -like '/api/parameters/shovel/*') {
                            $name = [Uri]::UnescapeDataString(($req.RawUrl -split '/')[-1])
                            if ($state.DeleteRemoves) { $state.Shovels.Remove($name) }
                            $res.StatusCode = $state.DeleteStatus
                        } else { $res.StatusCode = 400 }
                        $res.ContentType = 'application/json'
                        $bytes = [Text.Encoding]::UTF8.GetBytes($body)
                        $res.ContentLength64 = $bytes.Length
                        $res.OutputStream.Write($bytes, 0, $bytes.Length)
                        $res.Close()
                    }
                }).AddArgument($listener).AddArgument($state)
                $handle = $ps.BeginInvoke()
                [pscustomobject]@{ Port = $port; State = $state; Listener = $listener; Ps = $ps; Handle = $handle }
            }

            function script:Stop-FakeBroker {
                param($Broker)
                $Broker.State.Stop = $true
                try { $Broker.Listener.Stop(); $Broker.Listener.Close() } catch { }
                try { $Broker.Ps.Stop(); $Broker.Ps.Dispose() } catch { }
            }

            # Executes the step script as Azure DevOps would (macros expanded, secret in the env),
            # pointed at the fake broker. The 30s verification window is shortened so the "delete
            # accepted but shovel remains" case does not take half a minute.
            function script:Invoke-ShovelStep {
                param($Broker)
                $body = $script:ShovelScript
                $values = @{
                    ProfilerRabbitMqManagementUri = "http://localhost:$($Broker.Port)"
                    ProfilerRabbitMqVirtualHost   = $script:Dev['variables']['ProfilerRabbitMqVirtualHost']
                    ProfilerRabbitMqUsername      = $script:Dev['variables']['ProfilerRabbitMqUsername']
                    ProfilerShovelName            = $script:ShovelName
                }
                $body = [regex]::Replace($body, '\$\(([A-Za-z_][A-Za-z0-9_.]*)\)', {
                    param($m) if ($values.ContainsKey($m.Groups[1].Value)) { $values[$m.Groups[1].Value] } else { 'MACRO' }
                })
                $body = $body.Replace('AddSeconds(30)', 'AddSeconds(1)').Replace('Start-Sleep -Seconds 3', 'Start-Sleep -Milliseconds 100')

                $env:RabbitMqPasswordValue = 'not-a-real-password'
                $threw = $null; $out = $null
                try { $out = (& ([scriptblock]::Create($body))) 6>&1 | Out-String } catch { $threw = $_.Exception.Message }
                [pscustomobject]@{ Output = $out; Error = $threw; Calls = @($Broker.State.Calls) }
            }
        }

        It 'deletes a present shovel via /api/parameters/shovel, then re-lists and passes' {
            $b = Start-FakeBroker -Shovels @($script:ShovelName, 'someone-elses-shovel')
            try { $r = Invoke-ShovelStep $b } finally { Stop-FakeBroker $b }
            $r.Error | Should -BeNullOrEmpty
            $r.Calls | Should -Contain "DELETE /api/parameters/shovel/%2f/$($script:ShovelName)"
            @($r.Calls | Where-Object { $_ -like 'DELETE *' }).Count | Should -Be 1
            # list -> delete -> list (verification), in that order
            $r.Calls[0] | Should -BeLike 'GET /api/shovels/*'
            $r.Calls[1] | Should -BeLike 'DELETE *'
            $r.Calls[-1] | Should -BeLike 'GET /api/shovels/*'
            $r.Output | Should -Match 'Verified: shovel .* is gone'
            $b.State.Shovels | Should -Contain 'someone-elses-shovel'   # other shovels are left alone
        }

        It 'makes no DELETE at all when the shovel is already absent' {
            $b = Start-FakeBroker -Shovels @('someone-elses-shovel')
            try { $r = Invoke-ShovelStep $b } finally { Stop-FakeBroker $b }
            $r.Error | Should -BeNullOrEmpty
            @($r.Calls | Where-Object { $_ -like 'DELETE *' }).Count | Should -Be 0
            $r.Output | Should -Match 'nothing to delete'
        }

        It 'treats a 404 on the delete as already gone' {
            $b = Start-FakeBroker -Shovels @($script:ShovelName) -DeleteStatus 404 -DeleteRemoves $true
            try { $r = Invoke-ShovelStep $b } finally { Stop-FakeBroker $b }
            $r.Error | Should -BeNullOrEmpty
            $r.Output | Should -Match 'already gone'
        }

        It 'fails the build when the delete is accepted but the shovel is still listed' {
            $b = Start-FakeBroker -Shovels @($script:ShovelName) -DeleteStatus 204 -DeleteRemoves $false
            try { $r = Invoke-ShovelStep $b } finally { Stop-FakeBroker $b }
            $r.Error | Should -Match 'still listed'
        }

        It 'fails the build, naming the required tag, when the broker refuses the delete' {
            $b = Start-FakeBroker -Shovels @($script:ShovelName) -DeleteStatus 403 -DeleteRemoves $false
            try { $r = Invoke-ShovelStep $b } finally { Stop-FakeBroker $b }
            $r.Error | Should -Match 'DELETE /api/parameters/shovel'
            $r.Error | Should -Match 'policymaker'
        }
    }

    It 'runs the prerequisite validation straight after checkout, before any download or deploy' {
        # Step 0 is 'checkout: self'; nothing else may sit between it and the fail-fast checks.
        (Get-Steps $script:Dev)[0].Contains('checkout') | Should -BeTrue
        (Get-StepIndex $script:Dev 'Validate Dev QueueProcessor prerequisites') | Should -Be 1
        (Get-StepIndex $script:Dev 'Validate Dev QueueProcessor prerequisites') |
            Should -BeLessThan (Get-StepIndex $script:Dev 'Download Lightweight Engine Package')
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
