<#
    Pester 5 suite for the two Profiler pipelines that test, locally, the Unlimited.ProfilerServices CI
    pipelines, and the queue triggers beside them:

        ProfilerService-CI-Dev-LightweightExecutionEngine.yml   mirrors  Unlimited.ProfilerServices/ProfilerService-CI-Dev.yml  + ProfilerDev.trigger.bite
        ProfilerService-CI-UAT-LightweightExecutionEngine.yml   mirrors  Unlimited.ProfilerServices/ProfilerService-CI-UAT.yml  + ProfilerUat.trigger.bite

        Invoke-Pester -Path ./ProfilerService-CI-Dev-LightweightExecutionEngine.Tests.ps1

    The mirror takes everything from the Unlimited pipelines except the container tests (Docker build to
    Docker push), and adds the timer function app deployment. A pipeline file cannot be run locally, so this
    suite checks what CAN be proven without Azure:

      * the YAML parses, and every embedded PowerShell script is syntactically valid;
      * the container steps are gone and the timer function app is deployed;
      * Dev and UAT are the same pipeline apart from their environment names;
      * every repo path a step reads exists under Dev/Unlimited.ProfilerServices;
      * the sources and settings are encrypted in place by Encrypt-Resources.ps1 with the vault key, and the
        key file never reaches the 'drop' artifact;
      * each trigger file is internally consistent.

    The YAML tests need the powershell-yaml module and are skipped (not failed) without it.
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
    $script:Resources   = Join-Path $PSScriptRoot '..\Unlimited.ProfilerServices\Warewolf.ProfilerServices\Profiler'
    $script:Mirror      = Join-Path $PSScriptRoot '..\Unlimited.ProfilerServices'
    $script:RepoRoot    = Join-Path $PSScriptRoot '..\..'

    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        $script:Docs = @{
            'ProfilerService-CI-Dev-LightweightExecutionEngine.yml' = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath $script:DevPath -Raw)
            'ProfilerService-CI-UAT-LightweightExecutionEngine.yml' = ConvertFrom-Yaml -Ordered (Get-Content -LiteralPath $script:UatPath -Raw)
        }
        $script:Dev = $script:Docs['ProfilerService-CI-Dev-LightweightExecutionEngine.yml']
        $script:Uat = $script:Docs['ProfilerService-CI-UAT-LightweightExecutionEngine.yml']
    }

    # The PowerShell body of a step, or $null for a task with no script.
    function script:Get-StepScript {
        param($Step)
        if ($Step.Contains('pwsh'))       { return [string]$Step['pwsh'] }
        if ($Step.Contains('powershell')) { return [string]$Step['powershell'] }
        if ($Step.Contains('inputs') -and $Step['inputs'] -is [System.Collections.IDictionary]) {
            foreach ($key in 'inlineScript', 'script') {
                if ($Step['inputs'].Contains($key)) { return [string]$Step['inputs'][$key] }
            }
        }
        return $null
    }

    function script:Get-Steps {
        param($Doc)
        @($Doc['jobs'][0]['steps'])
    }

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

    # The workflow a trigger's WorkflowName ('ProfilerServices\...\Name') resolves to, as XML.
    function script:Get-TriggerWorkflow {
        param([string] $WorkflowName)
        $relative = $WorkflowName -replace '^ProfilerServices[\\/]', ''
        $path = Join-Path $script:Resources "$($relative.Replace('\', '/')).bite"
        [xml](Get-Content -LiteralPath $path -Raw)
    }
}

Describe 'Profiler <Env> pipeline - structure' -ForEach $script:Environments -Skip:(-not $script:HaveYaml) {

    BeforeAll { $script:Doc = $script:Docs[$File] }

    It 'parses as a single-job, manually-triggered pipeline' {
        $script:Doc['trigger'] | Should -Be 'none'
        @($script:Doc['jobs']).Count | Should -Be 1
        $script:Doc['jobs'][0]['pool']['vmImage'] | Should -Be 'windows-2022'
    }

    It 'has a syntactically valid script in every PowerShell step' {
        foreach ($step in Get-Steps $script:Doc) {
            $body = Get-StepScript $step
            if ($null -eq $body) { continue }
            $tokens = $null; $errors = $null
            [void][System.Management.Automation.Language.Parser]::ParseInput($body, [ref]$tokens, [ref]$errors)
            $errors | Should -BeNullOrEmpty -Because "step '$($step['displayName'])' must parse"
        }
    }

    It 'has no container steps and no second repository checkout' {
        $tasks = @(Get-Steps $script:Doc | ForEach-Object { $_['task'] })
        $tasks | Where-Object { $_ -like 'Docker@*' } | Should -BeNullOrEmpty
        @(Get-Steps $script:Doc | Where-Object { $_.Contains('checkout') }).Count | Should -Be 1
        (Get-Content -LiteralPath (Join-Path $PSScriptRoot $File) -Raw) | Should -Not -Match 'unlimited\.devops|docker-stop|Unit Tests'
    }

    It 'reads the tokens and the WFAES key from this environment''s vault, through its service connection' {
        $prefix = $Env.Substring(0, 1) + $Env.Substring(1).ToLowerInvariant()   # Dev / Uat
        $vault = $Env -eq 'DEV' ? 'Dev2AzureDevopsKeyVault' : 'UATAzureDevopsKeyVault'
        $kv = Find-Step $script:Doc "Azure Key Vault: $vault"
        $kv | Should -Not -BeNullOrEmpty
        $kv['inputs']['KeyVaultName'] | Should -Be $vault
        $kv['inputs']['ConnectedServiceName'] | Should -Be "$($Env.ToLowerInvariant())-sp-devops"
    }

    It 'deploys the published timer function package after publishing it and before generating Swagger' {
        $publish = Get-StepIndex $script:Doc 'Publish Azure Functions'
        $deploy  = Get-StepIndex $script:Doc 'Deploy the timer function app'
        $swagger = Get-StepIndex $script:Doc 'Swagger: Generate Swagger.json'
        $publish | Should -BeGreaterOrEqual 0
        $deploy  | Should -BeGreaterThan $publish
        $swagger | Should -BeGreaterThan $deploy
        $step = Find-Step $script:Doc 'Deploy the timer function app'
        $step['task'] | Should -Be 'AzureFunctionApp@2'
        $step['inputs']['connectedServiceNameARM'] | Should -Be "$($Env.ToLowerInvariant())-sp-devops"
        $step['inputs']['appName'] | Should -Be '$(ProfilerTimerFunctionAppName)'
        $step['inputs']['package'] | Should -BeLike '*/publish/AzureFunctions.ProfilerServices.zip'
        $script:Doc['variables']['ProfilerTimerFunctionAppName'] | Should -Be "profilerlw$($Env.ToLowerInvariant())-timer-function"
        $script:Doc['variables']['ProfilerTimerFunctionResourceGroup'] | Should -Be 'DEV2'
    }

    It 'publishes the engine package and the drop artifacts' {
        (Find-Step $script:Doc 'Publish Artifact: ProfilerEngine')['inputs']['artifact'] | Should -Be 'ProfilerEngine'
        (Find-Step $script:Doc 'Publish Artifact: drop')['inputs']['artifact'] | Should -Be 'drop'
    }
}

Describe 'Profiler pipelines - Dev and UAT are the same pipeline apart from their environment' -Skip:(-not $script:HaveYaml) {

    It 'differ only in the names of the environment' {
        $dev = (Get-Content -LiteralPath $script:DevPath -Raw)
        $uat = (Get-Content -LiteralPath $script:UatPath -Raw)
        $expected = $dev.Replace('Dev2AzureDevopsKeyVault', 'UATAzureDevopsKeyVault').Replace('dev-sp-devops', 'uat-sp-devops').
            Replace("ProfilerEnvironment: 'dev'", "ProfilerEnvironment: 'uat'").Replace('profilerlwdev-timer-function', 'profilerlwuat-timer-function')
        $uat | Should -Be $expected
    }
}

Describe 'Profiler pipelines - the mirrored Unlimited.ProfilerServices content' -Skip:(-not $script:HaveYaml) {

    It 'has every repo path the Dev pipeline reads' {
        $text = Get-Content -LiteralPath $script:DevPath -Raw
        $paths = [regex]::Matches($text, 'Dev[\\/]Unlimited\.ProfilerServices[\\/][A-Za-z0-9_.\\/-]+') | ForEach-Object Value | Sort-Object -Unique
        $paths | Should -Not -BeNullOrEmpty
        foreach ($p in $paths) {
            $clean = $p.TrimEnd('/', '\', '.', "'")
            # Wildcards and files created at run time (templates are renamed, the key is token-replaced).
            if ($clean -match '[*]' -or $clean -match 'EngineEncryptionKey') { continue }
            Test-Path -LiteralPath (Join-Path $script:RepoRoot $clean) | Should -BeTrue -Because "$clean is read by the pipeline"
        }
    }

    It 'ships the engine templates but never a token-replaced key or a committed secret' {
        $engine = Join-Path $script:Mirror 'Warewolf.ProfilerServices\Engine'
        foreach ($f in 'executionengine.settings.json.txt', 'profilerinfrastructure.json.txt', 'servicebustrigger.bite.txt') {
            Test-Path -LiteralPath (Join-Path $engine $f) | Should -BeTrue
        }
        # EngineEncryptionKey.txt holds the key's token only; it must never hold the key itself.
        (Get-Content -LiteralPath (Join-Path $engine 'EngineEncryptionKey.txt') -Raw -ErrorAction SilentlyContinue) | Should -Not -Match '"key"\s*:\s*"[A-Za-z0-9+/=]{40,}"'
    }

    It 'has the timer function project' {
        Test-Path -LiteralPath (Join-Path $script:Mirror 'AzureFunctions.ProfilerServices\AzureFunctions.ProfilerServices.csproj') | Should -BeTrue
    }
}

Describe 'Profiler pipelines - source encryption' -Skip:(-not $script:HaveYaml) -ForEach $script:Environments {

    BeforeAll {
        $script:Doc    = $script:Docs[$File]
        $script:Encrypt = Get-StepScript (Find-Step $script:Doc 'Encrypt the sources and apply the trigger concurrency')
    }

    It 'encrypts the staged Resources and Settings in place with Encrypt-Resources.ps1 and the vault key' {
        $script:Encrypt | Should -Match 'Encrypt-Resources\.ps1'
        $script:Encrypt | Should -Match "foreach \(\`$dir in 'Resources', 'Settings'\)"
        $script:Encrypt | Should -Match '-SourceDirectory "\$stage\\\$dir" -OutputDirectory "\$stage\\\$dir"'
        $script:Encrypt | Should -Match 'EngineEncryptionKey\.txt'
        $script:Encrypt | Should -Not -Match 'Protect-ProfilerSources'
        Test-Path -LiteralPath (Join-Path $PSScriptRoot '..\Warewolf.Execution.Lightweight\Scripts\Encrypt-Resources.ps1') | Should -BeTrue
    }

    It 'fails when nothing was encrypted and applies the trigger concurrency afterwards' {
        $script:Encrypt | Should -Match "No source ConnectionString was encrypted"
        $script:Encrypt.IndexOf('Encrypt-Resources.ps1') | Should -BeLessThan $script:Encrypt.IndexOf('Set-ProfilerEngineConcurrency.ps1')
        Test-Path -LiteralPath (Join-Path $script:Mirror 'SolutionItems\Scripts\Set-ProfilerEngineConcurrency.ps1') | Should -BeTrue
    }

    It 'removes the key file even on failure, before drop is copied' {
        $remove = Find-Step $script:Doc 'Remove the WFAES key file'
        $remove['condition'] | Should -Be 'always()'
        (Get-StepIndex $script:Doc 'Remove the WFAES key file') | Should -BeLessThan (Get-StepIndex $script:Doc 'Copy Files to: $(Build.ArtifactStagingDirectory)')
    }

    It 'places secure.config and the license from the Library before staging the package' {
        (Find-Step $script:Doc 'Download Profiler secure.config')['task'] | Should -Be 'DownloadSecureFile@1'
        (Find-Step $script:Doc 'Download Warewolf License.secureconfig')['task'] | Should -Be 'DownloadSecureFile@1'
        (Get-StepIndex $script:Doc 'Place secure.config and the license in the Engine folder') | Should -BeLessThan (Get-StepIndex $script:Doc 'Add the profiler workflows, sources and settings')
    }
}

Describe '<Trigger>' -ForEach $script:Environments {

    BeforeAll {
        $script:TriggerFile = Join-Path $PSScriptRoot $Trigger
        $script:Trig     = Get-Content -LiteralPath $script:TriggerFile -Raw | ConvertFrom-Json
        $script:LoadTest = Get-Content -LiteralPath $script:LoadTestTriggerPath -Raw | ConvertFrom-Json
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
        [guid]::Parse($script:Trig.TriggerId) | Should -Not -BeNullOrEmpty
    }

    It 'consumes this environment''s queue and a separate dead-letter queue' {
        $script:Trig.QueueName       | Should -BeLike "*.$Env"
        $script:Trig.DeadLetterQueue | Should -BeLike "*.$Env"
        $script:Trig.DeadLetterQueue | Should -Not -Be $script:Trig.QueueName
    }

    It 'uses one broker source for both the queue and the dead-letter queue' {
        $script:Trig.QueueSourceId | Should -Be '0b142714-8f6d-41b7-9832-2aefa8c731ec'
        $script:Trig.QueueSinkId   | Should -Be $script:Trig.QueueSourceId
    }

    It 'declares both queues durable' {
        ($script:Trig.Options | Where-Object Name -eq 'Durable').Value            | Should -BeTrue
        ($script:Trig.DeadLetterOptions | Where-Object Name -eq 'Durable').Value  | Should -BeTrue
    }

    It 'maps the whole message to the workflow''s single input (MapEntireMessage)' {
        # RequestForProfilingConsume reads the whole document from ONE scalar input and parses it into
        # [[@RequestData]] itself, so the trigger must hand it the raw message, not bind fields by name.
        $script:Trig.MapEntireMessage | Should -BeTrue
        @($script:Trig.Inputs | ForEach-Object Name) -join ',' | Should -Be 'ProfileRequest'
        $workflow = Get-TriggerWorkflow $script:Trig.WorkflowName
        $declared = @($workflow.Service.DataList.ChildNodes | Where-Object { $_.ColumnIODirection -in 'Input', 'Both' } | ForEach-Object LocalName)
        $declared -join ',' | Should -Be 'ProfileRequest' -Because 'the trigger''s one input must be the workflow''s only input'
    }
}

Describe 'Profiler trigger files - Dev and UAT are separate triggers' {

    BeforeAll {
        $script:DevTrig = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ProfilerDev.trigger.bite') -Raw | ConvertFrom-Json
        $script:UatTrig = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ProfilerUat.trigger.bite') -Raw | ConvertFrom-Json
    }

    It 'differ in <_>' -ForEach @('TriggerId', 'Name', 'QueueName', 'DeadLetterQueue') {
        $script:UatTrig.$_ | Should -Not -Be $script:DevTrig.$_
    }

    It 'are otherwise the same trigger (workflow, scaling, source and inputs)' {
        $strip = { param($t) $c = $t | ConvertTo-Json -Depth 10 | ConvertFrom-Json
                   foreach ($p in 'Id', 'TriggerId', 'Name', 'QueueName', 'DeadLetterQueue') { $c.PSObject.Properties.Remove($p) }
                   $c | ConvertTo-Json -Depth 10 }
        (& $strip $script:UatTrig) | Should -Be (& $strip $script:DevTrig)
    }
}

Describe 'Profiler trigger files - ResourceId names the workflow' {

    # A trigger's ResourceId is the id of the WORKFLOW it runs, not of the trigger: the catalog looks
    # a workflow's queues up by it (TriggersCatalog.LoadQueuesByResourceId). Dev and UAT once carried
    # RequestForProfiling's id while naming RequestForProfilingConsume - production's trigger
    # (Unlimited.ProfilerServices Triggers/ProfilerProfileRequest.txt) carries the consume workflow's.
    It '<_> carries the ID of the workflow it names' -ForEach @('ProfilerDev.trigger.bite', 'ProfilerUat.trigger.bite', 'ProfilerLoadTest.trigger.bite') {
        $trig = Get-Content -LiteralPath (Join-Path $PSScriptRoot $_) -Raw | ConvertFrom-Json
        $workflow = Get-TriggerWorkflow $trig.WorkflowName
        $trig.ResourceId | Should -Be $workflow.Service.ID -Because "$_ runs $($trig.WorkflowName)"
    }
}
