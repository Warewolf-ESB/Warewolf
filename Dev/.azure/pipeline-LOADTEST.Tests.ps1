#Requires -Version 7.0
<#
    Pester 5 suite: pipeline-LOADTEST.yml deploys the PUBLISHED engine, QueueProcessor worker and
    deployment scripts (warewolf.online/release-notes.php), as the Profiler Dev/UAT pipelines do.

        Invoke-Pester -Path ./pipeline-LOADTEST.Tests.ps1

    A pipeline cannot be run locally, so this suite checks what CAN be proven offline:

      * the YAML parses and every embedded PowerShell script is syntactically valid;
      * nothing compiles the engine or the worker any more, and nothing deploys from the checkout's
        Deploy-WwExecutionEngine.ps1 / Deploy-WwQueueProcessor.ps1;
      * 'Resolve Published Release URLs' - EXECUTED against a stubbed release page - resolves all
        three links, refuses an untrusted host and a win-x64 engine, honours a pin, and publishes
        the worker/scripts URLs as output variables the QueueProcessor job actually reads;
      * 'Deploy via Deployment Scripts' - EXECUTED against stub orchestrators - fails before
        deploying when the released script lacks a parameter, and passes every argument otherwise;
      * the splatted argument set is one the checkout's own Deploy-WwExecutionEngine.ps1 accepts.

    Requires the powershell-yaml module. The whole suite is skipped (not failed) without it.
#>

BeforeDiscovery {
    $script:HaveYaml = [bool](Get-Module -ListAvailable -Name powershell-yaml)
}

BeforeAll {
    $script:YamlPath     = Join-Path $PSScriptRoot 'pipeline-LOADTEST.yml'
    $script:Yaml         = Get-Content -LiteralPath $script:YamlPath -Raw
    $script:EngineScript = Join-Path $PSScriptRoot '..\Warewolf.Execution.Lightweight\Scripts\Deploy-WwExecutionEngine.ps1'

    if (Get-Module -ListAvailable -Name powershell-yaml) {
        Import-Module powershell-yaml -ErrorAction Stop
        $script:Doc = ConvertFrom-Yaml -Ordered $script:Yaml
    }

    function script:Get-StepScript {
        param($Step)
        if ($Step.Contains('pwsh'))       { return [string]$Step['pwsh'] }
        if ($Step.Contains('powershell')) { return [string]$Step['powershell'] }
        return $null
    }

    function script:Get-Job {
        param([string] $Name)
        foreach ($stage in @($script:Doc['stages'])) {
            foreach ($job in @($stage['jobs'])) { if ($job['job'] -eq $Name) { return $job } }
        }
    }

    function script:Get-Stage {
        param([string] $Name)
        @($script:Doc['stages']) | Where-Object { $_['stage'] -eq $Name } | Select-Object -First 1
    }

    function script:Find-Step {
        param([string] $Job, [string] $DisplayName)
        @((Get-Job $Job)['steps']) | Where-Object { $_['displayName'] -eq $DisplayName } | Select-Object -First 1
    }

    # Replaces every Azure Pipelines $(Macro) with its value. A PowerShell subexpression such as
    # $($x -join ',') is untouched: its first character after '(' is not a macro-name character.
    function script:Expand-Macros {
        param([string] $Script, [hashtable] $Values)
        [regex]::Replace($Script, '\$\(([A-Za-z][A-Za-z0-9_.]*)\)', {
            param($m)
            $name = $m.Groups[1].Value
            if (-not $Values.ContainsKey($name)) { throw "Test supplied no value for pipeline macro `$($name)." }
            [string]$Values[$name]
        })
    }

    # The keys of the deploy step's $deployArgs splat, in order.
    function script:Get-DeployArgKeys {
        param([string] $Script)
        $block = [regex]::Match($Script, '(?s)\$deployArgs = \[ordered\]@\{(.*?)\n\s*\}').Groups[1].Value
        [regex]::Matches($block, '(?m)^\s*([A-Za-z][A-Za-z0-9]*)\s*=') | ForEach-Object { $_.Groups[1].Value }
    }

    # A stub orchestrator declaring -Params; when run it records its bound parameters to $Record.
    function script:New-StubScript {
        param([string] $Path, [string[]] $Params, [string] $Record)
        $decl = ($Params | ForEach-Object { "[object] `$$_" }) -join ', '
        Set-Content -LiteralPath $Path -Value "param($decl)`n`$PSBoundParameters.Keys | Set-Content -LiteralPath '$Record'"
    }

    $script:Host_ = 'https://stwwreleases.blob.core.windows.net/releases'
    function script:New-ReleasePage {
        param([string] $EngineUrl = "$script:Host_/AzureFunctionsPackage-linux-x64-9.9.9.9.zip",
              [string] $ScriptsUrl = "$script:Host_/DeploymentScripts-9.9.9.9.zip",
              [string] $WorkerUrl = "$script:Host_/Warewolf-QueueProcessor-9.9.9.9.zip")
        @(
            "<a class=`"btn`" data-role=`"engine-linux-zip-link`" href=`"$EngineUrl`">Linux</a>"
            "<a class=`"btn`" data-role=`"engine-windows-zip-link`" href=`"$script:Host_/AzureFunctionsPackage-win-x64-9.9.9.9.zip`">Windows</a>"
            "<a class=`"btn`" data-role=`"scripts-zip-link`" href=`"$ScriptsUrl`">Scripts</a>"
            "<a class=`"btn`" data-role=`"queueworker-zip-link`" href=`"$WorkerUrl`">Worker</a>"
        ) -join "`n"
    }

    # Runs the resolve step with Invoke-WebRequest stubbed to return -Page. Returns the Write-Host
    # lines; the step's own throw propagates.
    function script:Invoke-ResolveStep {
        param([string] $Page, [string] $EngineZipUrl = '', [string] $ScriptsZipUrl = '', [string] $QueueWorkerZipUrl = '')
        $script:PageRequests = 0
        $body = Expand-Macros (Get-StepScript (Find-Step 'Deploy_UAT' 'Resolve Published Release URLs')) @{
            WarewolfReleasesStorageAccount = 'stwwreleases'
            WarewolfEngineZipUrl           = $EngineZipUrl
            WarewolfScriptsZipUrl          = $ScriptsZipUrl
            WarewolfQueueWorkerZipUrl      = $QueueWorkerZipUrl
        }
        $sb = [scriptblock]::Create($body)
        & {
            function Invoke-WebRequest { $script:PageRequests++; [pscustomobject]@{ Content = $Page } }
            . $sb
        } 6>&1 | ForEach-Object { [string]$_ }
    }
}

Describe 'pipeline-LOADTEST - released bits' -Skip:(-not $script:HaveYaml) {

    It 'parses, and every embedded PowerShell script is syntactically valid' {
        $bad = foreach ($stage in @($script:Doc['stages'])) {
            foreach ($job in @($stage['jobs'])) {
                foreach ($step in @($job['steps'])) {
                    $body = Get-StepScript $step
                    if (-not $body) { continue }
                    $errors = $null
                    [System.Management.Automation.Language.Parser]::ParseInput($body, [ref]$null, [ref]$errors) | Out-Null
                    if ($errors) { "$($job['job']) / $($step['displayName']): $($errors[0].Message)" }
                }
            }
        }
        $bad | Should -BeNullOrEmpty
    }

    It 'tracks the latest release by default, with all three pins empty' {
        $vars = $script:Doc['variables']
        $vars['WarewolfReleasesStorageAccount'] | Should -Be 'stwwreleases'
        foreach ($v in 'WarewolfEngineZipUrl', 'WarewolfScriptsZipUrl', 'WarewolfQueueWorkerZipUrl') {
            $vars.Contains($v) | Should -BeTrue -Because "$v is the pin for that package"
            $vars[$v] | Should -Be ''
        }
    }

    It 'no longer compiles the engine or the QueueProcessor' {
        (Get-Job 'Build_QueueProcessor') | Should -BeNullOrEmpty
        (Get-Job 'Build_And_Publish_UAT') | Should -BeNullOrEmpty
        $script:Yaml | Should -Not -Match 'Warewolf\.Execution\.Lightweight\.csproj'
        $script:Yaml | Should -Not -Match 'Warewolf\.Execution\.QueueProcessor\.csproj'
        $script:Yaml | Should -Not -Match "artifact(Name)?:\s*'(LightweightUATPublish|QueueProcessorPublish)'"
    }

    It 'still stages and publishes the UAT workflow resources' {
        $publish = @((Get-Job 'Stage_UAT_Resources')['steps']) | Where-Object { $_['task'] -eq 'PublishPipelineArtifact@1' }
        @($publish).Count | Should -Be 1
        $publish['inputs']['artifact'] | Should -Be 'UATWorkflowResources'
    }

    It 'never deploys from the checkout''s copy of either orchestrator' {
        $script:Yaml | Should -Not -Match '(?m)^\s*&\s*["''][^"'']*Build\.SourcesDirectory[^"'']*\\Deploy-WwExecutionEngine\.ps1'
        $script:Yaml | Should -Not -Match '(?m)^\s*&\s*["''][^"'']*Build\.SourcesDirectory[^"'']*\\Deploy-WwQueueProcessor\.ps1'
    }
}

Describe 'pipeline-LOADTEST - Resolve Published Release URLs' -Skip:(-not $script:HaveYaml) {

    It 'runs in Deploy_UAT as the ResolveRelease step, before both downloads' {
        $steps = @((Get-Job 'Deploy_UAT')['steps'])
        $names = @($steps | ForEach-Object { $_['displayName'] })
        $resolve = [array]::IndexOf($names, 'Resolve Published Release URLs')
        $resolve | Should -BeGreaterOrEqual 0
        $steps[$resolve]['name'] | Should -Be 'ResolveRelease'
        [array]::IndexOf($names, 'Download Lightweight Engine Package') | Should -BeGreaterThan $resolve
        [array]::IndexOf($names, 'Download Warewolf Deployment Scripts') | Should -BeGreaterThan $resolve
    }

    It 'resolves all three links off the page and publishes the worker + scripts URLs as outputs' {
        $out = Invoke-ResolveStep -Page (New-ReleasePage)
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedEngineZipUrl]$script:Host_/AzureFunctionsPackage-linux-x64-9.9.9.9.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedScriptsZipUrl]$script:Host_/DeploymentScripts-9.9.9.9.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedScriptsZipUrl;isOutput=true]$script:Host_/DeploymentScripts-9.9.9.9.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedQueueWorkerZipUrl;isOutput=true]$script:Host_/Warewolf-QueueProcessor-9.9.9.9.zip"
    }

    It 'uses the pins without fetching the page when all three are set' {
        $out = Invoke-ResolveStep -Page '' -EngineZipUrl "$script:Host_/AzureFunctionsPackage-linux-x64-1.2.3.4.zip" `
            -ScriptsZipUrl "$script:Host_/DeploymentScripts-1.2.3.4.zip" -QueueWorkerZipUrl "$script:Host_/Warewolf-QueueProcessor-1.2.3.4.zip"
        $script:PageRequests | Should -Be 0
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedEngineZipUrl]$script:Host_/AzureFunctionsPackage-linux-x64-1.2.3.4.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedQueueWorkerZipUrl;isOutput=true]$script:Host_/Warewolf-QueueProcessor-1.2.3.4.zip"
    }

    It 'refuses a <What> link on an untrusted host' -ForEach @(
        @{ What = 'engine';  Page = @{ EngineUrl  = 'https://evil.example.com/AzureFunctionsPackage-linux-x64-9.zip' } }
        @{ What = 'scripts'; Page = @{ ScriptsUrl = 'https://evil.example.com/DeploymentScripts-9.zip' } }
        @{ What = 'worker';  Page = @{ WorkerUrl  = 'http://stwwreleases.blob.core.windows.net/releases/Warewolf-QueueProcessor-9.zip' } }
    ) {
        { Invoke-ResolveStep -Page (New-ReleasePage @Page) } | Should -Throw '*Refusing to fetch*'
    }

    It 'refuses a pinned win-x64 engine package' {
        { Invoke-ResolveStep -Page (New-ReleasePage) -EngineZipUrl "$script:Host_/AzureFunctionsPackage-win-x64-9.9.9.9.zip" } |
            Should -Throw '*win-x64*'
    }

    It 'fails naming the pin when the page lacks the <Role> link' -ForEach @(
        @{ Role = 'engine-linux-zip-link'; Pin = 'WarewolfEngineZipUrl' }
        @{ Role = 'scripts-zip-link';      Pin = 'WarewolfScriptsZipUrl' }
        @{ Role = 'queueworker-zip-link';  Pin = 'WarewolfQueueWorkerZipUrl' }
    ) {
        $page = (New-ReleasePage) -replace "data-role=`"$Role`"", 'data-role="removed"'
        { Invoke-ResolveStep -Page $page } | Should -Throw "*$Pin*"
    }
}

Describe 'pipeline-LOADTEST - Deploy via Deployment Scripts' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:Deploy = Get-StepScript (Find-Step 'Deploy_UAT' 'Deploy via Deployment Scripts')
        $script:Keys   = @(Get-DeployArgKeys $script:Deploy)
        $script:Macros = @{
            EngineZipPath                            = 'C:\agent\LightweightEngine\AzureFunctionsPackage.zip'
            'Agent.BuildDirectory'                   = 'C:\agent'
            'warewolfLicense.secureFilePath'         = 'C:\agent\_temp\license'
            WwExecutionServiceBusTriggerQueueName    = 'q'
            WwExecutionServiceBusTriggerWorkflowName = 'w'
        }
    }

    It 'deploys the downloaded engine zip with the downloaded orchestrator' {
        $script:Deploy | Should -Match "PublishPath\s+=\s+'\`$\(EngineZipPath\)'"
        $script:Deploy | Should -Match "(?m)^\s*&\s*'\`$\(DeployScriptPath\)'\s+@deployArgs\s*$"
    }

    It 'keeps the four throughput settings the load test is calibrated against' {
        $script:Deploy | Should -Match 'ServiceBusMaxConcurrentCalls\s+=\s+2\b'
        $script:Deploy | Should -Match 'ServiceBusTriggerMaxConcurrentExecutions\s+=\s+2\b'
        $script:Deploy | Should -Match 'FlexMaximumInstanceCount\s+=\s+16\b'
        $script:Deploy | Should -Match 'DisableDynamicConcurrency\s+=\s+\$true'
    }

    It 'passes only parameters the checkout''s Deploy-WwExecutionEngine.ps1 declares' {
        $script:Keys.Count | Should -BeGreaterOrEqual 20 -Because 'the splat must be found (guards against a vacuous pass)'
        $declared = (Get-Command -Name (Resolve-Path $script:EngineScript).Path -CommandType ExternalScript).Parameters
        @($script:Keys | Where-Object { -not $declared.ContainsKey($_) }) | Should -BeNullOrEmpty
    }

    It 'fails before deploying when the released orchestrator lacks a parameter, naming it and the pin' {
        $stub   = Join-Path $TestDrive 'Old-Deploy.ps1'
        $record = Join-Path $TestDrive 'old.bound'
        New-StubScript -Path $stub -Params @($script:Keys | Where-Object { $_ -ne 'FlexMaximumInstanceCount' }) -Record $record
        $body = Expand-Macros $script:Deploy ($script:Macros + @{ DeployScriptPath = $stub })

        { & ([scriptblock]::Create($body)) } | Should -Throw '*-FlexMaximumInstanceCount*WarewolfScriptsZipUrl*'
        Test-Path -LiteralPath $record | Should -BeFalse -Because 'nothing may be deployed once a parameter is missing'
    }

    It 'invokes the released orchestrator with every argument when it declares them all' {
        $stub   = Join-Path $TestDrive 'Current-Deploy.ps1'
        $record = Join-Path $TestDrive 'current.bound'
        New-StubScript -Path $stub -Params $script:Keys -Record $record
        $body = Expand-Macros $script:Deploy ($script:Macros + @{ DeployScriptPath = $stub })

        & ([scriptblock]::Create($body))
        @(Get-Content -LiteralPath $record) | Should -Be $script:Keys
    }
}

Describe 'pipeline-LOADTEST - ProfilerLoadTest_QueueProcessor uses the same release' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:QpJob = Get-Job 'ProfilerLoadTest_QueueProcessor'
        $script:QpText = (@($script:QpJob['steps']) | ForEach-Object { Get-StepScript $_ }) -join "`n"
    }

    It 'reads the worker and scripts URLs from Deploy_UAT''s ResolveRelease outputs' {
        (Get-Stage 'Load_Test')['dependsOn'] | Should -Be 'Deploy_UAT'
        foreach ($v in 'ResolvedQueueWorkerZipUrl', 'ResolvedScriptsZipUrl') {
            $script:QpJob['variables'][$v] | Should -Be "`$[ stageDependencies.Deploy_UAT.Deploy_UAT.outputs['ResolveRelease.$v'] ]"
        }
    }

    It 'trust-checks both URLs before downloading, and locates the worker root and orchestrator' {
        $dl = Get-StepScript (@($script:QpJob['steps']) | Where-Object { $_['displayName'] -eq 'Download QueueProcessor Package + Deployment Scripts' })
        $dl | Should -Match 'Refusing to fetch'
        $dl.IndexOf('Refusing to fetch') | Should -BeLessThan $dl.IndexOf('Invoke-WebRequest')
        $dl | Should -Match 'Warewolf\.Execution\.QueueProcessor\.dll'
        $dl | Should -Match 'variable=QueueWorkerPublishPath\]'
        $dl | Should -Match 'Deploy-WwQueueProcessor\.ps1'
        $dl | Should -Match 'variable=QueueProcessorDeployScriptPath\]'
    }

    It 'deploys, seeds topology and publishes the burst against the downloaded worker' {
        $script:QpText | Should -Match "(?m)^\s*&\s*'\`$\(QueueProcessorDeployScriptPath\)'"
        ([regex]::Matches($script:QpText, "-PublishPath\s+'\`$\(QueueWorkerPublishPath\)'")).Count | Should -Be 3
        $script:QpText | Should -Not -Match '-PublishPath\s+"[^"]*\\QueueProcessorPublish"'
        @($script:QpJob['steps']) | Where-Object { $_['task'] -like 'DownloadPipelineArtifact*' } | Should -BeNullOrEmpty
    }
}
