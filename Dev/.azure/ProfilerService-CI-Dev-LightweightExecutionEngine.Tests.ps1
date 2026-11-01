<#
    Pester 5 suite for the two Profiler engine-package pipelines, the scripts they run and the queue triggers
    beside them:

        ProfilerService-CI-Dev-LightweightExecutionEngine.yml   + ProfilerDev.trigger.bite
        ProfilerService-CI-UAT-LightweightExecutionEngine.yml   + ProfilerUat.trigger.bite
        profiler-engine/  Set-ProfilerTokens.ps1, Protect-ProfilerSources.ps1, Set-ProfilerEngineConcurrency.ps1

        Invoke-Pester -Path ./ProfilerService-CI-Dev-LightweightExecutionEngine.Tests.ps1

    A pipeline file cannot be run locally, so this suite checks what CAN be proven without Azure:

      * the YAML parses, and every embedded PowerShell script is syntactically valid (a bad
        script otherwise fails only after the agent has spent minutes on the steps before it);
      * both files only BUILD the ProfilerEngine artifact - nothing is deployed - and share ONE
        body: everything from 'jobs:' down is byte-identical, so the environments differ only in
        their variables;
      * every release token the package carries has a value: a pipeline variable, or a Key Vault
        secret that is both loaded and mapped into the token step;
      * the package lays the profiler out so the timer's and the trigger's routes resolve;
      * the token, encryption and concurrency scripts do what the pipeline relies on;
      * each trigger file is internally consistent.

    The YAML tests need the powershell-yaml module and are skipped (not failed) without it.
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
    $script:Environments = @(
        @{ Env = 'DEV'; File = 'ProfilerService-CI-Dev-LightweightExecutionEngine.yml'; Trigger = 'ProfilerDev.trigger.bite' }
        @{ Env = 'UAT'; File = 'ProfilerService-CI-UAT-LightweightExecutionEngine.yml'; Trigger = 'ProfilerUat.trigger.bite' }
    )

    # Every #{Token} in the files the pipeline stages and replaces: the profiler resources, the rabbit
    # resources and the profiler-engine templates.
    $resources = Join-Path $PSScriptRoot '..\Warewolf.Execution.Lightweight\Resources'
    $files = @(Get-ChildItem -LiteralPath (Join-Path $resources 'ProfilerServices'), (Join-Path $resources 'rabbit') -Recurse -File -Filter '*.bite') +
             @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'profiler-engine') -File -Filter '*.txt')
    $script:PackageTokens = @($files | Select-String -Pattern '#\{([A-Za-z0-9_.-]+)\}' -AllMatches | ForEach-Object { $_.Matches } |
        ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
}

BeforeAll {
    $script:DevPath     = Join-Path $PSScriptRoot 'ProfilerService-CI-Dev-LightweightExecutionEngine.yml'
    $script:UatPath     = Join-Path $PSScriptRoot 'ProfilerService-CI-UAT-LightweightExecutionEngine.yml'
    $script:LoadTestTriggerPath = Join-Path $PSScriptRoot 'ProfilerLoadTest.trigger.bite'
    $script:EngineDir   = Join-Path $PSScriptRoot 'profiler-engine'
    $script:Resources   = Join-Path $PSScriptRoot '..\Warewolf.Execution.Lightweight\Resources'

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
        if ($Step.Contains('inputs') -and $Step['inputs'] -is [System.Collections.IDictionary] -and $Step['inputs'].Contains('inlineScript')) {
            return [string]$Step['inputs']['inlineScript']
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

    function script:Get-Body([string] $Path) {
        $text = Get-Content -LiteralPath $Path -Raw
        $text.Substring($text.IndexOf("`njobs:"))
    }

    # The run-phase copy of BeforeDiscovery's token list (discovery variables do not reach the run).
    function script:Get-PackageTokens {
        $files = @(Get-ChildItem -LiteralPath (Join-Path $script:Resources 'ProfilerServices'), (Join-Path $script:Resources 'rabbit') -Recurse -File -Filter '*.bite') +
                 @(Get-ChildItem -LiteralPath $script:EngineDir -File -Filter '*.txt')
        @($files | Select-String -Pattern '#\{([A-Za-z0-9_.-]+)\}' -AllMatches | ForEach-Object { $_.Matches } |
            ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
    }

    function script:Get-SecretsFilter {
        param($Doc)
        @(([string](Find-Step $Doc 'Azure Key Vault: release tokens and the WFAES key')['inputs']['SecretsFilter']).Split(',') | ForEach-Object Trim)
    }

    # The workflow a trigger's WorkflowName ('ProfilerServices\...\Name') resolves to, as XML.
    function script:Get-TriggerWorkflow {
        param([string] $WorkflowName)
        $path = Join-Path $script:Resources "$($WorkflowName.Replace('\', '/')).bite"
        [xml](Get-Content -LiteralPath $path -Raw)
    }
}

Describe 'Profiler <Env> pipeline - structure' -ForEach $script:Environments -Skip:(-not $script:HaveYaml) {

    BeforeAll { $script:Doc = $script:Docs[$File] }

    It 'parses as a single-job, manually-triggered pipeline with no parameters' {
        $script:Doc['trigger'] | Should -Be 'none'
        $script:Doc.Contains('parameters') | Should -BeFalse
        @($script:Doc['jobs']).Count | Should -Be 1
        foreach ($item in (Get-Steps $script:Doc)) { @($item.Keys)[0] | Should -Not -BeLike '${{*' }
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
        # Guard against a vacuous pass.
        $parsed | Should -BeGreaterOrEqual 6
        $failures | Should -BeNullOrEmpty
    }

    It 'only builds: no Azure CLI step and no az call' {
        foreach ($step in (Get-Steps $script:Doc)) {
            [string]$step['task'] | Should -Not -BeLike 'AzureCLI*' -Because "$($step['displayName']) would act on Azure"
            $body = Get-StepScript $step
            if ($body) { $body | Should -Not -Match '(?m)^(?!\s*#).*(^|[\s(=|;{])az\s+[a-z]' -Because "$($step['displayName']) calls az" }
        }
    }

    It 'loads the vault, then stages, replaces, encrypts, verifies, zips and publishes, in that order' {
        $order = 'Azure Key Vault: release tokens and the WFAES key',
                 'Resolve the published Lightweight Engine package',
                 'Download and extract the Lightweight Engine package',
                 'Add the profiler workflows, sources and settings',
                 'Replace tokens in the staged resources and settings',
                 'Encrypt the sources and apply the trigger concurrency',
                 'Verify the staged package',
                 'Zip the profiler engine package',
                 'Publish Artifact: ProfilerEngine'
        $indexes = @($order | ForEach-Object { Get-StepIndex $script:Doc $_ })
        $indexes | Should -Not -Contain -1
        ($indexes -join ',') | Should -Be ((@($indexes) | Sort-Object) -join ',')
        (Get-Steps $script:Doc)[0].Contains('checkout') | Should -BeTrue
    }

    It 'publishes profiler-engine.zip as the ProfilerEngine artifact' {
        $zip = Find-Step $script:Doc 'Zip the profiler engine package'
        $pub = Find-Step $script:Doc 'Publish Artifact: ProfilerEngine'
        $zip['inputs']['archiveFile'] | Should -BeLike '*\ProfilerEngine\profiler-engine.zip'
        $pub['inputs']['artifact'] | Should -Be 'ProfilerEngine'
        # The YAML's own folder, as a string: Split-Path would turn its '\' into '/' on Linux.
        $pub['inputs']['targetPath'] | Should -Be ($zip['inputs']['archiveFile'] -replace '\\[^\\]+$')
    }

    It 'fetches the engine only from the releases storage account, and only the linux-x64 package' {
        $body = Get-StepScript (Find-Step $script:Doc 'Resolve the published Lightweight Engine package')
        $body | Should -Match ([regex]::Escape('$parsed.Host -ne $expectedHost'))
        $body | Should -Match 'engine-linux-zip-link'
        $body | Should -Match "-match 'win-x64'"
    }
}

Describe 'Profiler pipelines - syntax check sanity' {
    It 'the syntax check itself detects a broken script' {
        $tokens = $null; $errors = $null
        [void][System.Management.Automation.Language.Parser]::ParseInput('if ($x { Write-Host "unbalanced"', [ref]$tokens, [ref]$errors)
        $errors.Count | Should -BeGreaterThan 0
    }
}

Describe 'Profiler pipelines - Dev and UAT share one body, with their own engine' -Skip:(-not $script:HaveYaml) {

    BeforeAll { $script:Own = 'ProfilerEnvironment', 'ProfilerEngineAppName', 'ProfilerServiceBusTriggerQueueName' }

    It 'are byte-identical from jobs: down, so the environments can only differ in their variables' {
        $dev = Get-Body $script:DevPath
        $uat = Get-Body $script:UatPath
        $dev.Length | Should -BeGreaterThan 5000
        $uat | Should -BeExactly $dev
    }

    It 'declare the same variable names' {
        (@($script:Uat['variables'].Keys) | Sort-Object) -join ',' | Should -Be ((@($script:Dev['variables'].Keys) | Sort-Object) -join ',')
    }

    It 'give each environment its own <_>' -ForEach @('ProfilerEnvironment', 'ProfilerEngineAppName', 'ProfilerServiceBusTriggerQueueName') {
        $script:Uat['variables'][$_] | Should -Not -Be $script:Dev['variables'][$_] -Because "$_ is per environment"
    }

    It 'keep the rest of the variables identical' {
        foreach ($name in @($script:Dev['variables'].Keys | Where-Object { $_ -notin $script:Own })) {
            $script:Uat['variables'][$name] | Should -Be $script:Dev['variables'][$name] -Because "$name is shared"
        }
    }

    It 'build for wwprofilerlw / wwprofileruatlw, triggered from profiler.profile.request.<Env>' {
        $script:Dev['variables']['ProfilerEngineAppName'] | Should -Be 'wwprofilerlw'
        $script:Uat['variables']['ProfilerEngineAppName'] | Should -Be 'wwprofileruatlw'
        foreach ($doc in $script:Dev, $script:Uat) {
            $doc['variables']['ProfilerServiceBusTriggerQueueName'] | Should -Be "profiler.profile.request.$($doc['variables']['ProfilerEnvironment'])"
        }
    }
}

Describe 'Profiler pipelines - every release token has a value' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:Tokens  = Get-PackageTokens
        $script:Filter  = Get-SecretsFilter $script:Dev
        $script:Replace = Find-Step $script:Dev 'Replace tokens in the staged resources and settings'
        $script:KeyName = $script:Dev['variables']['ProfilerEngineKeyVaultSecretName']
    }

    It 'finds the tokens (guards against a vacuous pass)' {
        $script:Tokens.Count | Should -BeGreaterOrEqual 20
    }

    It 'gives #{<_>} a value: a pipeline variable or a loaded Key Vault secret' -ForEach $script:PackageTokens {
        ($script:Dev['variables'].Contains($_) -or $script:Filter -contains $_) | Should -BeTrue -Because "#{$_} is in a staged file"
    }

    It 'maps every token into the token step''s environment by its own name' {
        foreach ($token in $script:Tokens) {
            $script:Replace['env'][$token] | Should -Be "`$($token)" -Because "Set-ProfilerTokens.ps1 reads #{$token} from env:$token"
        }
    }

    It 'loads no secret that no file uses, bar the WFAES key' {
        foreach ($secret in $script:Filter | Where-Object { $_ -ne $script:KeyName }) {
            $script:Tokens | Should -Contain $secret -Because "$secret is loaded from the vault"
        }
    }

    It 'encrypts with the key the engine is told to decrypt with' {
        $script:Filter | Should -Contain $script:KeyName
        (Find-Step $script:Dev 'Encrypt the sources and apply the trigger concurrency')['env']['WfaesKeyMaterial'] | Should -Be "`$($script:KeyName)"
        $settings = Get-Content -LiteralPath (Join-Path $script:EngineDir 'executionengine.settings.json.txt') -Raw | ConvertFrom-Json
        $settings.keyVaultName | Should -Be '#{ProfilerEngineKeyVaultName}'
        $settings.keyVaultSecretName | Should -Be '#{ProfilerEngineKeyVaultSecretName}'
    }

    It 'reads the vault the variables name, through the service connection' {
        $kv = Find-Step $script:Dev 'Azure Key Vault: release tokens and the WFAES key'
        $kv['inputs']['KeyVaultName'] | Should -Be '$(ProfilerEngineKeyVaultName)'
        # Compile-time form: service connections are authorised before runtime variables exist.
        $kv['inputs']['azureSubscription'] | Should -Be '${{ variables.AzureServiceConnection }}'
    }
}

Describe 'Profiler pipelines - package layout' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:Trigger = Get-Content -LiteralPath (Join-Path $script:EngineDir 'servicebustrigger.bite.txt') -Raw | ConvertFrom-Json
        $script:Stage   = Get-StepScript (Find-Step $script:Dev 'Add the profiler workflows, sources and settings')
        $script:Verify  = Get-StepScript (Find-Step $script:Dev 'Verify the staged package')
    }

    It 'stages ProfilerServices at the Resources root, as the Unlimited does, so its routes resolve' {
        # Staged path = path under Resources/ProfilerServices, so these are the routes on the engine.
        Test-Path -LiteralPath (Join-Path $script:Resources 'ProfilerServices\Profiler\V2\Functions\ProfilerAlerts\GrantDates.bite') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $script:Resources "ProfilerServices\$($script:Trigger.WorkflowName).bite") | Should -BeTrue
        $script:Verify | Should -Match ([regex]::Escape('Resources\Profiler\V2\Functions\ProfilerAlerts\GrantDates.bite'))
        $script:Verify | Should -Match ([regex]::Escape("Resources\$($script:Trigger.WorkflowName.Replace('/', '\')).bite"))
    }

    It 'binds the trigger''s whole message to the consume workflow''s only input' {
        $workflow = Get-TriggerWorkflow "ProfilerServices/$($script:Trigger.WorkflowName)"
        $declared = @($workflow.Service.DataList.ChildNodes | Where-Object { $_.ColumnIODirection -in 'Input', 'Both' } | ForEach-Object LocalName)
        $declared -join ',' | Should -Be $script:Trigger.MapEntireMessageTo
        $script:Trigger.QueueName | Should -Be '#{ProfilerServiceBusTriggerQueueName}'
    }

    It 'ships secure.config and the license at the package root, and persistence under Settings' {
        # secure.config is git-ignored (never committed), so a checkout has none: both pipelines take
        # it from the Library, like the license, and never from the repo.
        foreach ($doc in $script:Dev, $script:Uat) {
            $download = Find-Step $doc 'Download Profiler secure.config'
            $download['task'] | Should -Be 'DownloadSecureFile@1'
            $download['name'] | Should -Be 'profilerSecureConfig'
            $download['inputs']['secureFile'] | Should -Be 'Profiler secure.config'
            $stage = Get-StepScript (Find-Step $doc 'Add the profiler workflows, sources and settings')
            $stage | Should -Match ([regex]::Escape('''$(profilerSecureConfig.secureFilePath)'' -Destination "$stage\secure.config"'))
            $stage | Should -Not -Match ([regex]::Escape('$templates\secure.config'))
            (Get-StepIndex $doc 'Download Profiler secure.config') | Should -BeLessThan (Get-StepIndex $doc 'Add the profiler workflows, sources and settings')
        }
        $script:Stage | Should -Match ([regex]::Escape('"$stage\Settings\persistencesettings.json"'))
        $script:Stage | Should -Match ([regex]::Escape('"$stage\Settings\persistencesettingsdbsource.bite"'))
        $script:Stage | Should -Match ([regex]::Escape('"$stage\Warewolf License.secureconfig"'))
    }

    It 'never replaces tokens or encrypts in the checkout' {
        foreach ($name in 'Replace tokens in the staged resources and settings', 'Encrypt the sources and apply the trigger concurrency') {
            (Get-StepScript (Find-Step $script:Dev $name)) | Should -Not -Match 'Build\.SourcesDirectory\)\\Dev\\Warewolf\.Execution\.Lightweight'
        }
    }
}

Describe 'Set-ProfilerTokens.ps1' {

    BeforeAll {
        $script:TokenScript = Join-Path $script:EngineDir 'Set-ProfilerTokens.ps1'
        $script:Saved = @{}
        foreach ($n in 'PTA', 'PTB', 'PTMissing', 'PTMacro', 'PTUnsafe') { $script:Saved[$n] = [Environment]::GetEnvironmentVariable($n) }
    }
    AfterAll {
        foreach ($n in $script:Saved.Keys) { [Environment]::SetEnvironmentVariable($n, $script:Saved[$n]) }
    }
    BeforeEach {
        $script:Dir = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path "$script:Dir\sub" | Out-Null
        $env:PTA = 'alpha'; $env:PTB = 'beta'
        foreach ($n in 'PTMissing', 'PTMacro', 'PTUnsafe') { [Environment]::SetEnvironmentVariable($n, $null) }
    }

    It 'replaces every token in .bite and .json files, recursively, and reports the count' {
        Set-Content -LiteralPath "$script:Dir\a.bite" -Value '<Source ConnectionString="Host=#{PTA};User=#{PTB};Again=#{PTA}" />' -NoNewline
        Set-Content -LiteralPath "$script:Dir\sub\b.json" -Value '{ "x": "#{PTB}" }' -NoNewline
        Set-Content -LiteralPath "$script:Dir\c.txt" -Value '#{PTA}' -NoNewline
        $r = & $script:TokenScript -Path $script:Dir
        Get-Content -LiteralPath "$script:Dir\a.bite" -Raw | Should -Be '<Source ConnectionString="Host=alpha;User=beta;Again=alpha" />'
        Get-Content -LiteralPath "$script:Dir\sub\b.json" -Raw | Should -Be '{ "x": "beta" }'
        Get-Content -LiteralPath "$script:Dir\c.txt" -Raw | Should -Be '#{PTA}' -Because '.txt is not included'
        $r.Files | Should -Be 2
        $r.Tokens | Should -Be 2
        $r.Replaced | Should -Be 4
    }

    It 'keeps a file''s UTF-8 BOM, or its absence' {
        # Join-Path, not "$dir\x": .NET's file API takes '\' literally on Linux, which would put the
        # files beside $script:Dir, where the token script never looks.
        $bom   = Join-Path $script:Dir 'bom.bite'
        $noBom = Join-Path $script:Dir 'nobom.bite'
        [IO.File]::WriteAllText($bom, '#{PTA}', [Text.UTF8Encoding]::new($true))
        [IO.File]::WriteAllText($noBom, '#{PTA}', [Text.UTF8Encoding]::new($false))
        & $script:TokenScript -Path $script:Dir | Out-Null
        [IO.File]::ReadAllBytes($bom)[0..2] -join ',' | Should -Be '239,187,191'
        [IO.File]::ReadAllBytes($noBom)[0] | Should -Be ([byte][char]'a')
    }

    It 'fails on a token with no value, an unexpanded macro or an unsafe character, changing no file' -ForEach @(
        @{ Name = 'PTMissing'; Value = $null }
        @{ Name = 'PTMacro';   Value = '$(PTMacro)' }
        @{ Name = 'PTUnsafe';  Value = 'a&b' }) {
        if ($Value) { [Environment]::SetEnvironmentVariable($Name, $Value) }
        Set-Content -LiteralPath "$script:Dir\ok.bite" -Value '#{PTA}' -NoNewline
        Set-Content -LiteralPath "$script:Dir\bad.bite" -Value "#{$Name}" -NoNewline
        { & $script:TokenScript -Path $script:Dir 6>$null } | Should -Throw '*cannot be replaced*'
        Get-Content -LiteralPath "$script:Dir\ok.bite" -Raw | Should -Be '#{PTA}'
    }
}

Describe 'Protect-ProfilerSources.ps1' {

    BeforeAll {
        $script:ProtectScript = Join-Path $script:EngineDir 'Protect-ProfilerSources.ps1'
        $script:KeyBytes = [byte[]](1..32)
        $script:KeyFile = Join-Path $TestDrive 'key.json'
        @{ version = 1; keyId = 'test-key'; key = [Convert]::ToBase64String($script:KeyBytes); created = '2026-10-30'; previousKeys = @() } |
            ConvertTo-Json | Set-Content -LiteralPath $script:KeyFile
    }

    It 'WFAES-encrypts a plain ConnectionString so that it decrypts back, and leaves WFAES values alone' {
        $dir = Join-Path $TestDrive 'protect'
        New-Item -ItemType Directory -Path $dir | Out-Null
        Set-Content -LiteralPath "$dir\plain.bite" -Value '<Source ID="1" Name="S" ConnectionString="HostName=h;Password=p" Type="RabbitMQSource"><DisplayName>S</DisplayName></Source>'
        Set-Content -LiteralPath "$dir\done.bite" -Value '<Source ID="2" Name="T" ConnectionString="WFAES::already" Type="SqlDatabase" />'
        Set-Content -LiteralPath "$dir\workflow.bite" -Value '<Service ID="3" Name="W"><Action /></Service>'

        $r = & $script:ProtectScript -Path $dir -KeyMaterialPath $script:KeyFile 6>$null
        $r.Encrypted | Should -Be 1
        $r.AlreadyEncrypted | Should -Be 1

        $value = ([xml](Get-Content -LiteralPath "$dir\plain.bite" -Raw)).Source.ConnectionString
        $value | Should -BeLike 'WFAES::*'
        $data = [Convert]::FromBase64String($value.Substring(7))
        [ProfilerWfAes]::Decrypt($script:KeyBytes, $data) | Should -Be 'HostName=h;Password=p'
        ([xml](Get-Content -LiteralPath "$dir\done.bite" -Raw)).Source.ConnectionString | Should -Be 'WFAES::already'
    }

    It 'refuses key material that is not a 32-byte key' {
        $bad = Join-Path $TestDrive 'bad.json'
        @{ keyId = 'short'; key = [Convert]::ToBase64String([byte[]](1..16)) } | ConvertTo-Json | Set-Content -LiteralPath $bad
        { & $script:ProtectScript -Path $TestDrive -KeyMaterialPath $bad } | Should -Throw '*32 bytes*'
    }
}

Describe 'Set-ProfilerEngineConcurrency.ps1' {

    It 'sets maxConcurrentCalls and pins dynamic concurrency off in host.json, and maxConcurrentExecutions in the settings' {
        $root = Join-Path $TestDrive 'pkg'
        New-Item -ItemType Directory -Path "$root\Settings" | Out-Null
        '{ "version": "2.0", "extensions": { "serviceBus": { "prefetchCount": 0 } } }' | Set-Content -LiteralPath "$root\host.json"
        '{ "keyVaultName": "kv", "keyVaultSecretName": "s" }' | Set-Content -LiteralPath "$root\Settings\executionengine.settings.json"

        & (Join-Path $script:EngineDir 'Set-ProfilerEngineConcurrency.ps1') -PackageRoot $root -MaxConcurrentCalls 2 -MaxConcurrentExecutions 3 -DisableDynamicConcurrency 6>$null

        $hostJson = Get-Content -LiteralPath "$root\host.json" -Raw | ConvertFrom-Json
        $hostJson.extensions.serviceBus.maxConcurrentCalls | Should -Be 2
        $hostJson.extensions.serviceBus.prefetchCount | Should -Be 0
        $hostJson.concurrency.dynamicConcurrencyEnabled | Should -BeFalse
        $settings = Get-Content -LiteralPath "$root\Settings\executionengine.settings.json" -Raw | ConvertFrom-Json
        $settings.serviceBusTrigger.maxConcurrentExecutions | Should -Be 3
        $settings.keyVaultName | Should -Be 'kv'
    }

    It 'refuses a host.json with no serviceBus section' {
        $root = Join-Path $TestDrive 'nosb'
        New-Item -ItemType Directory -Path "$root\Settings" | Out-Null
        '{ "version": "2.0" }' | Set-Content -LiteralPath "$root\host.json"
        '{}' | Set-Content -LiteralPath "$root\Settings\executionengine.settings.json"
        { & (Join-Path $script:EngineDir 'Set-ProfilerEngineConcurrency.ps1') -PackageRoot $root -MaxConcurrentCalls 2 -MaxConcurrentExecutions 2 } | Should -Throw '*serviceBus*'
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
