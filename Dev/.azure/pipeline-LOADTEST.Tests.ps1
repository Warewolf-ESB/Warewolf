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
        three links, refuses an untrusted host and a linux-x64 engine, honours a pin, and publishes
        the worker/scripts URLs as output variables the QueueProcessor job actually reads;
      * 'Deploy via Deployment Scripts' - EXECUTED against stub orchestrators - fails before
        deploying when the released script lacks a parameter, and passes every argument otherwise;
      * the splatted argument set is one the checkout's own Deploy-WwExecutionEngine.ps1 accepts,
        and targets the Windows Consumption (Y1) plan rather than Flex;
      * 'Assert WarewolfServer-UAT is not a Linux (Flex) app' - EXECUTED against a stubbed az -
        fails on a Linux app and passes a Windows or missing one.

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
        param([string] $EngineUrl = "$script:Host_/AzureFunctionsPackage-win-x64-9.9.9.9.zip",
              [string] $ScriptsUrl = "$script:Host_/DeploymentScripts-9.9.9.9.zip",
              [string] $WorkerUrl = "$script:Host_/Warewolf-QueueProcessor-9.9.9.9.zip")
        @(
            "<a class=`"btn`" data-role=`"engine-linux-zip-link`" href=`"$script:Host_/AzureFunctionsPackage-linux-x64-9.9.9.9.zip`">Linux</a>"
            "<a class=`"btn`" data-role=`"engine-windows-zip-link`" href=`"$EngineUrl`">Windows</a>"
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
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedEngineZipUrl]$script:Host_/AzureFunctionsPackage-win-x64-9.9.9.9.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedScriptsZipUrl]$script:Host_/DeploymentScripts-9.9.9.9.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedScriptsZipUrl;isOutput=true]$script:Host_/DeploymentScripts-9.9.9.9.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedQueueWorkerZipUrl;isOutput=true]$script:Host_/Warewolf-QueueProcessor-9.9.9.9.zip"
    }

    It 'uses the pins without fetching the page when all three are set' {
        $out = Invoke-ResolveStep -Page '' -EngineZipUrl "$script:Host_/AzureFunctionsPackage-win-x64-1.2.3.4.zip" `
            -ScriptsZipUrl "$script:Host_/DeploymentScripts-1.2.3.4.zip" -QueueWorkerZipUrl "$script:Host_/Warewolf-QueueProcessor-1.2.3.4.zip"
        $script:PageRequests | Should -Be 0
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedEngineZipUrl]$script:Host_/AzureFunctionsPackage-win-x64-1.2.3.4.zip"
        $out | Should -Contain "##vso[task.setvariable variable=ResolvedQueueWorkerZipUrl;isOutput=true]$script:Host_/Warewolf-QueueProcessor-1.2.3.4.zip"
    }

    It 'refuses a <What> link on an untrusted host' -ForEach @(
        @{ What = 'engine';  Page = @{ EngineUrl  = 'https://evil.example.com/AzureFunctionsPackage-win-x64-9.zip' } }
        @{ What = 'scripts'; Page = @{ ScriptsUrl = 'https://evil.example.com/DeploymentScripts-9.zip' } }
        @{ What = 'worker';  Page = @{ WorkerUrl  = 'http://stwwreleases.blob.core.windows.net/releases/Warewolf-QueueProcessor-9.zip' } }
    ) {
        { Invoke-ResolveStep -Page (New-ReleasePage @Page) } | Should -Throw '*Refusing to fetch*'
    }

    It 'refuses a pinned linux-x64 engine package' {
        { Invoke-ResolveStep -Page (New-ReleasePage) -EngineZipUrl "$script:Host_/AzureFunctionsPackage-linux-x64-9.9.9.9.zip" } |
            Should -Throw '*linux-x64*'
    }

    It 'fails naming the pin when the page lacks the <Role> link' -ForEach @(
        @{ Role = 'engine-windows-zip-link'; Pin = 'WarewolfEngineZipUrl' }
        @{ Role = 'scripts-zip-link';        Pin = 'WarewolfScriptsZipUrl' }
        @{ Role = 'queueworker-zip-link';    Pin = 'WarewolfQueueWorkerZipUrl' }
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
        $script:Deploy | Should -Match 'FunctionAppScaleLimit\s+=\s+30\b'
        $script:Deploy | Should -Match 'DisableDynamicConcurrency\s+=\s+\$true'
    }

    It 'targets the Windows Consumption (Y1) plan, with no Flex-only parameter' {
        $script:Deploy | Should -Match "HostingPlan\s+=\s+'Consumption'"
        @($script:Keys | Where-Object { $_ -like 'Flex*' }) | Should -BeNullOrEmpty -Because 'Deploy-WwExecutionEngine.ps1 rejects -FlexMaximumInstanceCount without -HostingPlan Flex'
    }

    It 'gives the WAREWOLF_ENTRA_CONFIG self-heal a Graph-free appId for a recreated app' {
        $script:Deploy | Should -Match "EntraConfigRepairClientId\s+=\s+'e200900a-2d5d-4356-94c0-cd7e33232ce0'"
        $script:Deploy | Should -Match "SkipAuthProvisioning\s+=\s+\`$true"
    }

    It 'passes only parameters the checkout''s Deploy-WwExecutionEngine.ps1 declares' {
        $script:Keys.Count | Should -BeGreaterOrEqual 20 -Because 'the splat must be found (guards against a vacuous pass)'
        $declared = (Get-Command -Name (Resolve-Path $script:EngineScript).Path -CommandType ExternalScript).Parameters
        @($script:Keys | Where-Object { -not $declared.ContainsKey($_) }) | Should -BeNullOrEmpty
    }

    It 'fails before deploying when the released orchestrator lacks a parameter, naming it and the pin' {
        $stub   = Join-Path $TestDrive 'Old-Deploy.ps1'
        $record = Join-Path $TestDrive 'old.bound'
        New-StubScript -Path $stub -Params @($script:Keys | Where-Object { $_ -ne 'FunctionAppScaleLimit' }) -Record $record
        $body = Expand-Macros $script:Deploy ($script:Macros + @{ DeployScriptPath = $stub })

        { & ([scriptblock]::Create($body)) } | Should -Throw '*-FunctionAppScaleLimit*WarewolfScriptsZipUrl*'
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

Describe 'pipeline-LOADTEST - Assert WarewolfServer-UAT is not a Linux (Flex) app' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:GuardName = 'Assert WarewolfServer-UAT is not a Linux (Flex) app'
        $script:Guard     = Get-StepScript (Find-Step 'Deploy_UAT' $script:GuardName)

        # A stand-in for Scripts/WwAzCli.psm1: Invoke-WwAz (what the step aliases `az` to) records
        # its arguments and answers with $global:WwAzStubKind / $global:WwAzStubExit.
        $script:StubRoot = Join-Path $TestDrive 'src'
        $stubDir = Join-Path $script:StubRoot 'Dev\Warewolf.Execution.Lightweight\Scripts'
        New-Item -ItemType Directory -Force -Path $stubDir | Out-Null
        Set-Content -LiteralPath (Join-Path $stubDir 'WwAzCli.psm1') -Value @'
function Invoke-WwAz {
    $global:WwAzStubArgs = $args
    $global:LASTEXITCODE = $global:WwAzStubExit
    $global:WwAzStubKind
}
Export-ModuleMember -Function Invoke-WwAz
'@

        function script:Invoke-GuardStep {
            param([string] $Kind, [int] $Exit = 0)
            $global:WwAzStubKind = $Kind
            $global:WwAzStubExit = $Exit
            $body = Expand-Macros $script:Guard @{ 'Build.SourcesDirectory' = $script:StubRoot }
            & ([scriptblock]::Create($body)) 6>&1 | ForEach-Object { [string]$_ }
        }
    }

    AfterAll {
        Remove-Variable -Scope Global -Name WwAzStubKind, WwAzStubExit, WwAzStubArgs -ErrorAction SilentlyContinue
    }

    It 'runs after the settings backup is published and before the deploy' {
        $names = @(@((Get-Job 'Deploy_UAT')['steps']) | ForEach-Object { $_['displayName'] })
        $guard = [array]::IndexOf($names, $script:GuardName)
        $guard | Should -BeGreaterThan ([array]::IndexOf($names, 'Publish UAT Settings Backup'))
        $guard | Should -BeLessThan ([array]::IndexOf($names, 'Deploy via Deployment Scripts'))
    }

    It 'fails on an existing Linux app, naming the delete it needs' {
        { Invoke-GuardStep -Kind 'functionapp,linux' } | Should -Throw '*LINUX app*az functionapp delete --name WarewolfServer-UAT*'
        $global:WwAzStubArgs[0..1] | Should -Be @('functionapp', 'list')
    }

    It 'passes an existing Windows app' {
        Invoke-GuardStep -Kind 'functionapp' | Should -Match 'is a Windows app'
    }

    It 'passes when the app does not exist yet' {
        Invoke-GuardStep -Kind '' | Should -Match 'does not exist'
    }

    It 'fails when az itself fails, rather than reading that as a missing app' {
        { Invoke-GuardStep -Kind '' -Exit 1 } | Should -Throw '*az failed to list*'
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

    It 'is opt-in via RunQueueProcessorLoadTest (default off), and gates only this job' {
        $param = @($script:Doc['parameters']) | Where-Object { $_['name'] -eq 'RunQueueProcessorLoadTest' }
        $param | Should -Not -BeNullOrEmpty
        $param['type'] | Should -Be 'boolean'
        $param['default'] | Should -BeFalse

        $cond = [string]$script:QpJob['condition']
        $cond | Should -Match 'succeeded\(\)'
        $cond | Should -Match "eq\('\`$\{\{ parameters\.RunQueueProcessorLoadTest \}\}', 'True'\)"

        foreach ($other in 'Load_Tests', 'ShovelBridgeLoadTest_ExternalServiceBus') {
            [string](Get-Job $other)['condition'] | Should -Not -Match 'RunQueueProcessorLoadTest'
        }
    }
}

Describe 'pipeline-LOADTEST - both load-test jobs publish the real-world data file' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:QpJob    = Get-Job 'ProfilerLoadTest_QueueProcessor'
        $script:QpSteps  = @($script:QpJob['steps'])
        $script:QpNames  = @($script:QpSteps | ForEach-Object { $_['displayName'] })
        $script:Download = 'Download the load-test data file (secure file)'
        $script:Expand   = 'Expand and validate the load-test data file'
        $script:Publish  = 'Publish the burst (one message per data-file record, RequestForProfiling shape)'
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

        # Runs the expand step for real: -Records becomes a zip holding CollectionsForProfiling.json
        # (or -EntryName), standing in for the downloaded secure file. Returns the Write-Host lines.
        function script:Invoke-ExpandStep {
            param([object[]] $Records, [string] $EntryName = 'CollectionsForProfiling.json', [int] $MessageCount = 3,
                  [string] $Job = 'ProfilerLoadTest_QueueProcessor')
            $work = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Force -Path (Join-Path $work 'src') | Out-Null
            Set-Content -LiteralPath (Join-Path $work "src\$EntryName") -Value (ConvertTo-Json -InputObject @($Records) -Depth 5)
            $zip = Join-Path $work 'data.zip'
            Compress-Archive -Path (Join-Path $work "src\$EntryName") -DestinationPath $zip
            $body = Expand-Macros (Get-StepScript (Find-Step $Job $script:Expand)) @{
                ProfilerLoadTestDataFile           = Join-Path $work 'out\CollectionsForProfiling.json'
                'profilerLoadTestData.secureFilePath' = $zip
                'Build.SourcesDirectory'           = $script:RepoRoot
                MessageCount                       = $MessageCount
            }
            & ([scriptblock]::Create($body)) 6>&1 | ForEach-Object { [string]$_ }
        }

        function script:New-Record { param([string] $MandateId) [ordered]@{ MandateId = $MandateId; ForPaymentCycle = '202610' } }
    }

    It 'downloads the secure file, then expands it, then publishes - all before the burst' {
        $d = [array]::IndexOf($script:QpNames, $script:Download)
        $e = [array]::IndexOf($script:QpNames, $script:Expand)
        $p = [array]::IndexOf($script:QpNames, $script:Publish)
        $d | Should -BeGreaterOrEqual 0
        $e | Should -BeGreaterThan $d
        $p | Should -BeGreaterThan $e
        $script:QpSteps[$d]['task'] | Should -Be 'DownloadSecureFile@1'
        $script:QpSteps[$d]['name'] | Should -Be 'profilerLoadTestData'
        $script:QpSteps[$d]['inputs']['secureFile'] | Should -Be 'CollectionsForProfiling.zip'
    }

    It 'names the secure file literally - its authorisation is checked before variables are expanded' {
        # '$(ProfilerLoadTestDataSecureFile)' made Azure DevOps look for a file with that literal
        # name and reject the whole pipeline as invalid.
        foreach ($job in 'ProfilerLoadTest_QueueProcessor', 'ShovelBridgeLoadTest_ExternalServiceBus') {
            $dl = @((Get-Job $job)['steps']) | Where-Object { $_['task'] -eq 'DownloadSecureFile@1' -and $_['name'] -eq 'profilerLoadTestData' }
            $dl['inputs']['secureFile'] | Should -Not -Match '\$\(' -Because "$job's secure-file name must be a literal"
        }
    }

    It 'defines the data-file variable the steps read' {
        $vars = $script:QpJob['variables']
        $vars['ProfilerLoadTestDataFile'] | Should -Be '$(Agent.BuildDirectory)\ProfilerLoadTestData\CollectionsForProfiling.json'
    }

    It 'publishes from -DataFilePath and no longer sizes or shapes the burst itself' {
        $publish = Get-StepScript (Find-Step 'ProfilerLoadTest_QueueProcessor' $script:Publish)
        $publish | Should -Match "-DataFilePath\s+'\`$\(ProfilerLoadTestDataFile\)'"
        $publish | Should -Not -Match '-SuccessCount'
        $publish | Should -Not -Match '-BodyShape'
        $publish | Should -Match '-ManifestPath'
    }

    It 'expands a valid file to the path the publish step reads, and reports the record count' {
        $out = Invoke-ExpandStep -Records @((New-Record 'A'), (New-Record 'B'), (New-Record 'C'))
        $out | Should -Contain 'Data file: 3 record(s), 3 distinct MandateId(s).'
    }

    It 'warns, rather than fails, when the record count differs from MessageCount' {
        $out = Invoke-ExpandStep -Records @((New-Record 'A'), (New-Record 'B'), (New-Record 'C')) -MessageCount 20000
        ($out -join "`n") | Should -Match 'Data file has 3 records but MessageCount is 20000'
    }

    It 'fails before publishing when MandateIds repeat' {
        { Invoke-ExpandStep -Records @((New-Record 'A'), (New-Record 'A')) } | Should -Throw '*duplicate MandateIds (1 unique of 2)*'
    }

    It 'fails when the zip does not hold CollectionsForProfiling.json at its root' {
        { Invoke-ExpandStep -Records @((New-Record 'A')) -EntryName 'other.json' } | Should -Throw '*did not contain CollectionsForProfiling.json*'
    }

    It 'fails when the data file holds no records' {
        { Invoke-ExpandStep -Records @() } | Should -Throw '*contains no records*'
    }

    Context 'ShovelBridgeLoadTest_ExternalServiceBus' {

        BeforeAll {
            $script:SbJob   = Get-Job 'ShovelBridgeLoadTest_ExternalServiceBus'
            $script:SbSteps = @($script:SbJob['steps'])
            $script:SbNames = @($script:SbSteps | ForEach-Object { $_['displayName'] })
            # The step that INVOKES the script (an earlier step only mentions it in a comment).
            $script:SbRun   = $script:SbSteps | Where-Object { (Get-StepScript $_) -match "(?m)^\s*&\s*'[^']*Test-ShovelBridgeE2E\.ps1'" } | Select-Object -First 1
        }

        It 'downloads the secure file, then expands it, before the burst runs' {
            $d = [array]::IndexOf($script:SbNames, $script:Download)
            $e = [array]::IndexOf($script:SbNames, $script:Expand)
            $r = [array]::IndexOf($script:SbNames, $script:SbRun['displayName'])
            $d | Should -BeGreaterOrEqual 0
            $e | Should -BeGreaterThan $d
            $r | Should -BeGreaterThan $e
            $script:SbSteps[$d]['task'] | Should -Be 'DownloadSecureFile@1'
            $script:SbSteps[$d]['name'] | Should -Be 'profilerLoadTestData'
            $script:SbSteps[$d]['inputs']['secureFile'] | Should -Be 'CollectionsForProfiling.zip'
        }

        It 'defines the same data-file variable as the QueueProcessor job' {
            $vars = $script:SbJob['variables']
            $vars['ProfilerLoadTestDataFile'] | Should -Be $script:QpJob['variables']['ProfilerLoadTestDataFile']
        }

        It 'runs Test-ShovelBridgeE2E.ps1 with -DataFilePath, and neither a templated body nor a message count' {
            $run = Get-StepScript $script:SbRun
            $run | Should -Match "-DataFilePath\s+'\`$\(ProfilerLoadTestDataFile\)'"
            $run | Should -Match '-VerifyWorkflowExecution'
            $run | Should -Match '-DbVerificationMode\s+ProfilerRecommendation'
            $run | Should -Not -Match '-WorkflowInputsJson'
            $run | Should -Not -Match '-MessageCount' -Because 'the data file sizes the run and a disagreeing -MessageCount is an error'
        }

        It 'passes only parameters Test-ShovelBridgeE2E.ps1 declares' {
            $script = Join-Path $script:RepoRoot 'Dev\Warewolf.Execution.Lightweight\Scripts\Tests\Integration\Test-ShovelBridgeE2E.ps1'
            $declared = (Get-Command -Name $script -CommandType ExternalScript).Parameters
            $passed = [regex]::Matches((Get-StepScript $script:SbRun), '(?m)^\s+-([A-Za-z][A-Za-z0-9]*)\b') | ForEach-Object { $_.Groups[1].Value }
            $passed | Should -Contain 'DataFilePath'
            @($passed | Where-Object { -not $declared.ContainsKey($_) }) | Should -BeNullOrEmpty
        }

        It 'expands a valid file and reports the record count' {
            $out = Invoke-ExpandStep -Job 'ShovelBridgeLoadTest_ExternalServiceBus' -Records @((New-Record 'A'), (New-Record 'B'), (New-Record 'C'))
            $out | Should -Contain 'Data file: 3 record(s), 3 distinct MandateId(s).'
        }

        It 'fails before publishing when MandateIds repeat' {
            { Invoke-ExpandStep -Job 'ShovelBridgeLoadTest_ExternalServiceBus' -Records @((New-Record 'A'), (New-Record 'A')) } |
                Should -Throw '*duplicate MandateIds (1 unique of 2)*'
        }

        It 'fails when the zip does not hold CollectionsForProfiling.json at its root' {
            { Invoke-ExpandStep -Job 'ShovelBridgeLoadTest_ExternalServiceBus' -Records @((New-Record 'A')) -EntryName 'other.json' } |
                Should -Throw '*did not contain CollectionsForProfiling.json*'
        }

        It 'still pre-warms with VerifyWorkflowInputsJson, which is why that variable stays' {
            $script:SbJob['variables'].Contains('VerifyWorkflowInputsJson') | Should -BeTrue
            (@($script:SbSteps | ForEach-Object { Get-StepScript $_ }) -join "`n") | Should -Match 'ConvertFrom-Json ''\$\(VerifyWorkflowInputsJson\)'''
        }
    }
}

Describe 'pipeline-LOADTEST - end-to-end message processing time' -Skip:(-not $script:HaveYaml) {

    BeforeAll {
        $script:Report = Get-Job 'Report_Step_Timings'
        $script:PrintStep = @($script:Report['steps']) | Where-Object { $_['displayName'] -eq 'Print end-to-end message processing time' }

        function script:Invoke-PrintStep {
            param([hashtable] $Values)
            $sb = [scriptblock]::Create((Expand-Macros (Get-StepScript $script:PrintStep) $Values))
            & $sb 6>&1 | ForEach-Object { [string]$_ }
        }
    }

    It 'names the steps that publish the processing figures' {
        (Find-Step 'ShovelBridgeLoadTest_ExternalServiceBus' 'Run ShovelBridge load test (20000 messages, VerifyWorkflowExecution mode, UAT engine)')['name'] |
            Should -Be 'ShovelBridgeRun'
        $drain = Find-Step 'ProfilerLoadTest_QueueProcessor' 'Wait for the queue to drain'
        $drain['name'] | Should -Be 'QueueProcessorDrain'
        foreach ($v in 'MessageCount', 'ProcessingSeconds', 'MessagesPerSecond') {
            Get-StepScript $drain | Should -Match ([regex]::Escape("variable=$v;isOutput=true]"))
        }
    }

    It 'stamps the QueueProcessor publish start before publishing the burst' {
        $publish = Get-StepScript (Find-Step 'ProfilerLoadTest_QueueProcessor' 'Publish the burst (one message per data-file record, RequestForProfiling shape)')
        $publish.IndexOf('variable=BurstPublishStartUtc]') | Should -BeGreaterThan -1
        $publish.IndexOf('variable=BurstPublishStartUtc]') | Should -BeLessThan $publish.IndexOf('Publish-WwQueueBurst.ps1')
        Get-StepScript (Find-Step 'ProfilerLoadTest_QueueProcessor' 'Wait for the queue to drain') | Should -Match '\$\(BurstPublishStartUtc\)'
    }

    It 'reads every figure from an existing job and named step in Load_Test' {
        $expected = @{
            Shovel = 'ShovelBridgeLoadTest_ExternalServiceBus', 'ShovelBridgeRun'
            Qp     = 'ProfilerLoadTest_QueueProcessor', 'QueueProcessorDrain'
        }
        foreach ($prefix in $expected.Keys) {
            $job, $step = $expected[$prefix]
            (Get-Job $job) | Should -Not -BeNullOrEmpty
            foreach ($v in 'MessageCount', 'ProcessingSeconds', 'MessagesPerSecond') {
                $script:Report['variables']["$prefix$v"] | Should -Be "`$[ stageDependencies.Load_Test.$job.outputs['$step.$v'] ]"
            }
        }
        (Get-Stage 'Report_Step_Timings')['dependsOn'] | Should -Contain 'Load_Test'
    }

    It 'prints the headline first, and even when Load_Test failed' {
        $script:Report['steps'][1]['displayName'] | Should -Be 'Print end-to-end message processing time'
        $script:PrintStep['condition'] | Should -Be 'always()'
    }

    It 'prints each job''s processing time, or "not recorded" for a job that did not drain' {
        $out = Invoke-PrintStep @{
            ShovelMessageCount = '20000'; ShovelProcessingSeconds = '1350.4'; ShovelMessagesPerSecond = '14.8'
            QpMessageCount     = '';      QpProcessingSeconds     = '';       QpMessagesPerSecond     = ''
        }
        $out[0] | Should -Match 'END-TO-END PROCESSING TIME'
        $out | Should -Contain '  RabbitMQ Shovel -> Service Bus: 20000 message(s) in 0h 22m 30s (14.8 msg/s)'
        ($out | Where-Object { $_ -like '*QueueProcessor*' }) | Should -Match 'not recorded'
    }
}

Describe 'pipeline-LOADTEST - Profiler schema provisioning survives a paused serverless database' -Skip:(-not $script:HaveYaml) {
    <#
        Build 31060: WarewolfDevOpsTestDb (serverless, autoPauseDelay 60) was resuming, and the
        step's single 30s connection attempt timed out in the post-login phase. EXECUTED against a
        stubbed Invoke-Sqlcmd that fails with each queued message in turn.
    #>

    BeforeAll {
        $script:ProvisionStep = Find-Step 'ShovelBridgeLoadTest_ExternalServiceBus' 'Provision Profiler schema on WarewolfDevOpsTestDb (idempotent)'

        function script:Invoke-ProvisionStep {
            param([string[]] $Failures)
            $script:SqlCalls = 0
            $script:SqlFailures = [System.Collections.Generic.Queue[string]]::new([string[]]@($Failures))
            $sb = [scriptblock]::Create((Expand-Macros (Get-StepScript $script:ProvisionStep) @{ 'Build.SourcesDirectory' = 'C:\src' }))
            & {
                function Get-Module { $true }
                function Import-Module {}
                function Start-Sleep {}
                function Invoke-Sqlcmd { $script:SqlCalls++; if ($script:SqlFailures.Count) { throw $script:SqlFailures.Dequeue() } }
                . $sb
            } 6>&1 | ForEach-Object { [string]$_ }
        }
    }

    It 'retries a post-login connection timeout and then succeeds' {
        $out = Invoke-ProvisionStep -Failures 'Connection Timeout Expired.  The timeout period elapsed during the post-login phase.'
        $script:SqlCalls | Should -Be 2
        $out | Should -Contain 'Profiler schema/procedures provisioned (idempotent) on WarewolfDevOpsTestDb.'
        ($out | Where-Object { $_ -like '##`[warning]Attempt 1/5*' }) | Should -Not -BeNullOrEmpty
    }

    It 'retries 40613 "not currently available" while the database resumes' {
        $null = Invoke-ProvisionStep -Failures 'Database ''WarewolfDevOpsTestDb'' on server is not currently available.', 'Error 40613'
        $script:SqlCalls | Should -Be 3
    }

    It 'fails at once, without retrying, on an error in the script itself' {
        { Invoke-ProvisionStep -Failures "Invalid object name 'Profiler.Foo'." } | Should -Throw "*Invalid object name*"
        $script:SqlCalls | Should -Be 1
    }

    It 'gives up after five attempts when the database never comes back' {
        { Invoke-ProvisionStep -Failures (1..6 | ForEach-Object { 'Connection Timeout Expired.' }) } | Should -Throw '*Connection Timeout Expired*'
        $script:SqlCalls | Should -Be 5
    }
}
