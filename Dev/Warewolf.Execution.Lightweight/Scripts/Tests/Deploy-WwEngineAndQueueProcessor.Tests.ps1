#Requires -Version 7.0

<#
    Pester 5 test suite for Deploy-WwEngineAndQueueProcessor.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Deploy-WwEngineAndQueueProcessor.Tests.ps1

    Design notes (same conventions as Deploy-WwExecutionEngine.Tests.ps1)
    ---------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no cloud/filesystem action, so every
      test below is a real unit test - no az, no Azure, no deploy.
    * The splat builders, the capture/assert step and the handover-file shape are FUNCTIONS
      above that return, precisely so they can be tested here rather than through a mocked
      end-to-end run.
    * Fixtures are written to $TestDrive, which Pester discards after the run.
#>

BeforeAll {
    $script:DeployScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwEngineAndQueueProcessor.ps1'
    $script:EngineScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwExecutionEngine.ps1'
    $script:QpScript     = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwQueueProcessor.ps1'

    # A complete, VALID pair of engine outputs. Individual tests clone and damage one field,
    # so each assertion is exercised in isolation against an otherwise-good fixture.
    function New-GoodSummary {
        param([string]$AppName = 'wwengine-test')
        [ordered]@{
            status = 'completed'; lastPhase = 'Phase 5'; error = $null; dryRun = $false
            runId = 'wwx-20260827-101500'; appName = $AppName
            endpoint = "https://$AppName.azurewebsites.net"
            appInsightsName = "$AppName-ai"; entraAppDisplayName = "$AppName-auth"
            appInsightsConnectionString = 'Inst...xxxx'   # masked by the engine, deliberately unusable
        }
    }
    function New-GoodAuthOut {
        param([string]$AppName = 'wwengine-test')
        [ordered]@{
            FunctionAppName = $AppName; EntraAppDisplayName = "$AppName-auth"
            ClientId = '11111111-2222-3333-4444-555555555555'
            AppObjectId = 'aaaaaaaa-0000-0000-0000-000000000000'
            SpObjectId = 'bbbbbbbb-0000-0000-0000-000000000000'
            Audience = 'api://11111111-2222-3333-4444-555555555555'
            Issuer = 'https://login.microsoftonline.com/tid/v2.0'
            AppRoles = @(
                @{ value = 'Warewolf_Administrator'; displayName = 'Admin'; id = 'cccccccc-0000-0000-0000-000000000000' }
                @{ value = 'Warewolf_QueueProcessor'; displayName = 'QP'; id = 'dddddddd-0000-0000-0000-000000000000' }
            )
        }
    }
    # Writes a summary + auth output into a fresh log dir and returns both paths.
    function New-Fixture {
        param([hashtable]$Summary, [hashtable]$AuthOut, [string]$Name = 'fx', [switch]$NoAuthOut, [switch]$DryRunSummary)
        $log = Join-Path $TestDrive $Name
        New-Item -ItemType Directory -Force -Path $log | Out-Null
        $infix = if ($DryRunSummary) { 'dryrun.' } else { '' }
        $sp = Join-Path $log "deploy-WwExecutionEngine-20260827-101500.${infix}summary.json"
        if ($Summary) { $Summary | ConvertTo-Json -Depth 6 | Set-Content $sp -Encoding UTF8 }
        $ap = Join-Path $log 'Configure-WwExecutionAuth.output.json'
        if (-not $NoAuthOut) { $AuthOut | ConvertTo-Json -Depth 6 | Set-Content $ap -Encoding UTF8 }
        [pscustomobject]@{ LogDir = $log; SummaryPath = $sp; AuthPath = $ap }
    }
    function New-Ctx {
        @{
            Sub = 'sub-1'; TenantId = 'tid-1'; Rg = 'rg-1'; Loc = 'southafricanorth'
            Kv = 'kv-1'; KvSecret = 'wwaeskey'; Acr = 'acr1'; AcaEnv = 'aca-1'
            LogDir = 'D:\logs'; SecureConfig = 'D:\s\secure.config'; AuthConfig = 'D:\s\auth.json'
            LicenseConfig = 'D:\s\lic.secureconfig'; WorkflowsSrc = 'D:\s\resources'
            SourceDir = 'D:\s\sources'; TriggerDir = 'D:\s\triggers'
            EngineApp = 'wwengine-test'; EngineStorage = 'wwenginestore'; EnginePublish = 'D:\pub\engine'
            EngineAi = 'wwengine-test-ai'; EngineAuthApp = 'wwengine-test-auth'
            QpPublish = 'D:\pub\qp'; QpPrefix = 'wwqp-'; ImageRepo = 'warewolf/queueprocessor'
            DockerFile = 'D:\s\build\Dockerfile'
            RabbitSecretUri = 'https://kv-1.vault.azure.net/secrets/rabbitmq-uri'
            EngineUrl = 'https://wwengine-test.azurewebsites.net'
            EngineAppId = '11111111-2222-3333-4444-555555555555'
        }
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a step' {
        $out = (. $script:DeployScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Step 1 - Preflight'
        foreach ($fn in 'Invoke-AzJson', 'Test-WwAppName', 'Test-WwPlaceholder', 'Assert-WwRequired', 'Expand-WwPublishPath',
                        'Assert-WwPackage', 'Get-WwMasked', 'Get-WwProp', 'New-WwEngineParams',
                        'New-WwQpParams', 'Resolve-WwEngineHandover', 'New-WwHandoverObject') {
            Get-Command $fn -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty -Because "$fn should be defined"
        }
    }

    It 'checks every sibling script the orchestrators resolve at run time' {
        # The orchestrators Join-Path these off their own folder and do NOT pre-check them, so a
        # partial extraction fails mid-phase, after resources exist. The wrapper must catch it first.
        $src = Get-Content $script:DeployScript -Raw
        foreach ($sib in 'Configure-WwExecutionAuth.ps1', 'Setup-ApplicationInsights.ps1', 'Encrypt-Config.ps1') {
            $src | Should -Match ([regex]::Escape($sib)) -Because "$sib is a hard dependency of the engine deploy"
        }
    }

    It 'requires an explicit Dockerfile rather than the source-tree default' {
        # Deploy-WwQueueProcessor.ps1 falls back to <repo>\Warewolf.Execution.QueueProcessor\Dockerfile,
        # which does not exist when the scripts were extracted from a zip.
        $src = Get-Content $script:DeployScript -Raw
        $src | Should -Match 'Dockerfile not found'
    }

    It 'never hard-codes a real subscription, tenant or environment value' {
        $src = Get-Content $script:DeployScript -Raw
        # Any bare GUID outside the documented all-zero/example placeholders would be a leak.
        $guids = [regex]::Matches($src, '[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}') |
                 ForEach-Object { $_.Value } | Where-Object { $_ -ne '00000000-0000-0000-0000-000000000000' }
        $guids | Should -BeNullOrEmpty
        $src | Should -Not -Match '\.vault\.azure\.net'
        $src | Should -Not -Match '\.azurecr\.io'
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — child invocation' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'does not impose its own StrictMode on the orchestrators it calls' {
        # REGRESSION (operator machine, real -DryRun run). Set-StrictMode is DYNAMICALLY SCOPED,
        # so -Version Latest at the top of the wrapper was inherited by the child. Under Latest a
        # bare .Count throws for $null / string / int / FileInfo / EMPTY ARRAY, and
        # Deploy-WwQueueProcessor.ps1 - which sets no StrictMode of its own - died in Phase 0 on
        # `$triggerSet.Count` with:
        #     The property 'Count' cannot be found on this object. Verify that the property exists.
        $child = Join-Path $TestDrive 'strict-probe.ps1'
        # Mirrors the failing line: .Count on a single FileInfo, no StrictMode of its own.
        Set-Content -LiteralPath $child -Value '$s = Get-Item $PSCommandPath; "resolved=" + $s.Count'

        Set-StrictMode -Version Latest       # reproduce the wrapper's own top-level setting
        $result = Invoke-WwChild -Path $child -Params @{} -Label 'strict-probe' 6>&1 | Out-String
        $result | Should -Match 'resolved=1'
    }

    It 'refuses --query, because cmd.exe mangles JMESPath' {
        # REGRESSION (operator machine). The old helper QUOTED the JMESPath instead of avoiding it;
        # az.cmd echoed its own resolved command line to stdout and that echo was captured as the
        # Container App list, then written into the handover file.
        { Invoke-AzJson -AzArgs @('containerapp', 'list', '-g', 'rg', '--query', "[?starts_with(name,'x')].name") } |
            Should -Throw '*does not accept --query*'
    }

    Context 'Test-WwAppName' {
        It 'accepts a real Container App name' {
            Test-WwAppName 'wwqp-staging2-orders' | Should -BeTrue
        }
        It 'rejects the az command-line echo that poisoned a handover file' {
            $echo = 'G:\Deployment\Scripts>  "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\..\python.exe" -IBm azure.cli containerapp list -g DEV2'
            Test-WwAppName $echo | Should -BeFalse
        }
        It 'rejects empty, uppercase and edge-hyphen names' {
            Test-WwAppName ''            | Should -BeFalse
            Test-WwAppName 'WWQP-Orders' | Should -BeFalse
            Test-WwAppName '-leading'    | Should -BeFalse
            Test-WwAppName 'trailing-'   | Should -BeFalse
        }
    }

    It 'reports a missing child script by label rather than failing obscurely' {
        { Invoke-WwChild -Path (Join-Path $TestDrive 'no-such-child.ps1') -Params @{} -Label 'Deploy-WwQueueProcessor.ps1' } |
            Should -Throw '*Deploy-WwQueueProcessor.ps1 not found*'
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — placeholder guards' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    Context 'Test-WwPlaceholder' {
        # No angle brackets in the test NAME - Pester treats them as -ForEach templating.
        It 'treats an unedited angle-bracket value as a placeholder' {
            Test-WwPlaceholder '<resource group>' | Should -BeTrue
        }
        It 'treats empty and null as placeholders' {
            Test-WwPlaceholder ''    | Should -BeTrue
            Test-WwPlaceholder $null | Should -BeTrue
        }
        It 'accepts a real value' {
            Test-WwPlaceholder 'rg-warewolf-uat' | Should -BeFalse
            Test-WwPlaceholder 'wwqp-'           | Should -BeFalse
        }
        It 'catches a placeholder embedded in a composed path' {
            # $LogDir = "$Stage\logs\..." with $Stage unedited must not slip through.
            Test-WwPlaceholder '<local staging directory>\logs\deploy-both' | Should -BeTrue
        }
    }

    Context 'Assert-WwRequired' {
        It 'passes when every value is real' {
            { Assert-WwRequired @{ A = 'x'; B = 'y' } } | Should -Not -Throw
        }
        It 'reports EVERY placeholder in one throw, not just the first' {
            $msg = $null
            try { Assert-WwRequired @{ A = '<a>'; B = 'ok'; C = '<c>'; D = '' } } catch { $msg = $_.Exception.Message }
            $msg | Should -Match '3 value\(s\)'
            $msg | Should -Match '\bA\b'
            $msg | Should -Match '\bC\b'
            $msg | Should -Match '\bD\b'
            $msg | Should -Not -Match '\bB\b'
        }
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — publish paths' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'passes a folder through unchanged' {
        $d = Join-Path $TestDrive 'pub-folder'
        New-Item -ItemType Directory -Force -Path $d | Out-Null
        (Expand-WwPublishPath -Path $d -Label 'Engine') | Should -Be (Get-Item $d).FullName
    }

    It 'extracts a .zip to a sibling folder and returns the FOLDER' {
        $src = Join-Path $TestDrive 'zipsrc'
        New-Item -ItemType Directory -Force -Path $src | Out-Null
        Set-Content (Join-Path $src 'Warewolf.Execution.QueueProcessor.dll') 'x'
        $zip = Join-Path $TestDrive 'qppkg.zip'
        Compress-Archive -Path (Join-Path $src '*') -DestinationPath $zip -Force

        $out = Expand-WwPublishPath -Path $zip -Label 'QP'
        $out | Should -Be (Join-Path $TestDrive 'qppkg')
        Test-Path (Join-Path $out 'Warewolf.Execution.QueueProcessor.dll') | Should -BeTrue
    }

    It 'reuses an existing extraction instead of re-extracting' {
        $src = Join-Path $TestDrive 'zipsrc2'
        New-Item -ItemType Directory -Force -Path $src | Out-Null
        Set-Content (Join-Path $src 'a.dll') 'x'
        $zip = Join-Path $TestDrive 'pkg2.zip'
        Compress-Archive -Path (Join-Path $src '*') -DestinationPath $zip -Force
        $first = Expand-WwPublishPath -Path $zip -Label 'QP'
        Set-Content (Join-Path $first 'marker.txt') 'kept'
        $second = Expand-WwPublishPath -Path $zip -Label 'QP'
        $second | Should -Be $first
        Test-Path (Join-Path $second 'marker.txt') | Should -BeTrue
    }

    It 'rejects a file that is neither a folder nor a .zip' {
        $f = Join-Path $TestDrive 'notapackage.txt'
        Set-Content $f 'x'
        { Expand-WwPublishPath -Path $f -Label 'QP' } | Should -Throw '*must be a folder or a .zip*'
    }

    It 'reports a missing path with the label' {
        { Expand-WwPublishPath -Path (Join-Path $TestDrive 'nope') -Label 'Engine publish' } |
            Should -Throw '*Engine publish not found*'
    }

    Context 'Assert-WwPackage' {
        It 'accepts the right publish output' {
            $d = Join-Path $TestDrive 'ok-pub'
            New-Item -ItemType Directory -Force -Path $d | Out-Null
            Set-Content (Join-Path $d 'Warewolf.Execution.Lightweight.dll') 'x'
            { Assert-WwPackage -Dir $d -Assembly 'Warewolf.Execution.Lightweight.dll' -Label 'Engine' } | Should -Not -Throw
        }
        It 'catches engine and worker publish paths being swapped' {
            $d = Join-Path $TestDrive 'swapped'
            New-Item -ItemType Directory -Force -Path $d | Out-Null
            Set-Content (Join-Path $d 'Warewolf.Execution.Lightweight.dll') 'x'
            { Assert-WwPackage -Dir $d -Assembly 'Warewolf.Execution.QueueProcessor.dll' -Label 'QueueProcessor publish' } |
                Should -Throw '*does not contain Warewolf.Execution.QueueProcessor.dll*'
        }
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — engine splat' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'passes only real Deploy-WwExecutionEngine.ps1 parameters' {
        $real = ([System.Management.Automation.Language.Parser]::ParseFile($script:EngineScript, [ref]$null, [ref]$null)).ParamBlock.Parameters |
                ForEach-Object { $_.Name.VariablePath.UserPath }
        $p = New-WwEngineParams -Context (New-Ctx) -EncryptResources $true -NonInteractive $true -DryRun $true
        foreach ($k in $p.Keys) { $k | Should -BeIn $real }
    }

    It 'omits -EncryptResources unless the operator opted in' {
        (New-WwEngineParams -Context (New-Ctx)).ContainsKey('EncryptResources') | Should -BeFalse
        (New-WwEngineParams -Context (New-Ctx) -EncryptResources $true).EncryptResources | Should -BeTrue
    }

    It 'omits -NonInteractive and -DryRun unless requested' {
        $p = New-WwEngineParams -Context (New-Ctx)
        $p.ContainsKey('NonInteractive') | Should -BeFalse
        $p.ContainsKey('DryRun')         | Should -BeFalse
    }

    It 'always asks for App Insights BY NAME so the capture step has something to read' {
        $p = New-WwEngineParams -Context (New-Ctx)
        $p.EnableAppInsights | Should -BeTrue
        $p.AppInsightsName   | Should -Be 'wwengine-test-ai'
    }

    It 'does not enable the engine-side queue deploy (that would double-provision the workers)' {
        (New-WwEngineParams -Context (New-Ctx)).ContainsKey('DeployRabbitMqTriggers') | Should -BeFalse
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — QueueProcessor splat' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'passes only real Deploy-WwQueueProcessor.ps1 parameters' {
        $real = ([System.Management.Automation.Language.Parser]::ParseFile($script:QpScript, [ref]$null, [ref]$null)).ParamBlock.Parameters |
                ForEach-Object { $_.Name.VariablePath.UserPath }
        $p = New-WwQpParams -Context (New-Ctx) -AppInsightsConnectionString 'InstrumentationKey=abc' -NonInteractive $true -DryRun $true
        foreach ($k in $p.Keys) { $k | Should -BeIn $real }
    }

    It 'uses -DockerfilePath, never the prefix-abbreviated -Dockerfile' {
        $p = New-WwQpParams -Context (New-Ctx)
        $p.ContainsKey('DockerfilePath') | Should -BeTrue
        $p.ContainsKey('Dockerfile')     | Should -BeFalse
    }

    It 'omits -EnableAppInsights entirely when no connection string was resolved' {
        $p = New-WwQpParams -Context (New-Ctx) -AppInsightsConnectionString ''
        $p.ContainsKey('EnableAppInsights')           | Should -BeFalse
        $p.ContainsKey('AppInsightsConnectionString') | Should -BeFalse
    }

    It 'passes both App Insights parameters together when one was resolved' {
        $p = New-WwQpParams -Context (New-Ctx) -AppInsightsConnectionString 'InstrumentationKey=abc'
        $p.EnableAppInsights            | Should -BeTrue
        $p.AppInsightsConnectionString  | Should -Be 'InstrumentationKey=abc'
    }

    It 'wires the engine values that step 5 produced, not placeholders' {
        $p = New-WwQpParams -Context (New-Ctx)
        $p.EngineBaseUrl       | Should -Be 'https://wwengine-test.azurewebsites.net'
        $p.EngineResourceAppId | Should -Be '11111111-2222-3333-4444-555555555555'
        $p.RabbitMqSecretUri   | Should -Be 'https://kv-1.vault.azure.net/secrets/rabbitmq-uri'
    }

    It 'reuses the shared values rather than declaring them twice' {
        $ctx = New-Ctx
        $e = New-WwEngineParams -Context $ctx
        $q = New-WwQpParams     -Context $ctx
        $q.ResourceGroup      | Should -Be $e.ResourceGroup
        $q.Location           | Should -Be $e.Location
        $q.KeyVaultName       | Should -Be $e.KeyVaultName
        $q.KeyVaultSecretName | Should -Be $e.KeyVaultSecretName
        $q.EngineTenantId     | Should -Be $e.TenantId
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — capture assertions' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    It 'resolves every handover value from a good pair of outputs' {
        $fx = New-Fixture -Summary (New-GoodSummary) -AuthOut (New-GoodAuthOut) -Name 'good'
        $h  = Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath `
                                       -ExpectedAppName 'wwengine-test' -ExpectedAuthAppName 'wwengine-test-auth'
        $h.RunId                | Should -Be 'wwx-20260827-101500'
        $h.Endpoint             | Should -Be 'https://wwengine-test.azurewebsites.net'
        $h.AppInsightsName      | Should -Be 'wwengine-test-ai'
        $h.ClientId             | Should -Be '11111111-2222-3333-4444-555555555555'
        $h.SpObjectId           | Should -Be 'bbbbbbbb-0000-0000-0000-000000000000'
        $h.QueueProcessorRoleId | Should -Be 'dddddddd-0000-0000-0000-000000000000'
        $h.Notes                | Should -BeNullOrEmpty
    }

    It 'takes the endpoint from the summary rather than hand-building it' {
        $s = New-GoodSummary
        $s.endpoint = 'https://wwengine-test.privatelink.azurewebsites.net'
        $fx = New-Fixture -Summary $s -AuthOut (New-GoodAuthOut) -Name 'endpoint'
        (Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test').Endpoint |
            Should -Be 'https://wwengine-test.privatelink.azurewebsites.net'
    }

    It 'refuses a summary whose status is not completed' {
        $s = New-GoodSummary; $s.status = 'failed'; $s.lastPhase = 'Phase 4'; $s.error = 'zip deploy failed'
        $fx = New-Fixture -Summary $s -AuthOut (New-GoodAuthOut) -Name 'failed'
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw '*did not complete*Phase 4*zip deploy failed*'
    }

    It 'refuses a summary still marked in-progress' {
        $s = New-GoodSummary; $s.status = 'in-progress'
        $fx = New-Fixture -Summary $s -AuthOut (New-GoodAuthOut) -Name 'inprogress'
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw '*did not complete*'
    }

    It 'refuses a dry-run summary even when its status is completed' {
        $s = New-GoodSummary; $s.dryRun = $true
        $fx = New-Fixture -Summary $s -AuthOut (New-GoodAuthOut) -Name 'dryflag'
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw '*is a dry run*'
    }

    It 'ignores *.dryrun.summary.json when picking the newest summary' {
        $fx = New-Fixture -Summary (New-GoodSummary) -AuthOut (New-GoodAuthOut) -Name 'mixed' -DryRunSummary
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw '*No non-dry-run engine summary*'
    }

    It 'refuses a summary belonging to a different app (shared LogDir)' {
        $fx = New-Fixture -Summary (New-GoodSummary -AppName 'other-engine') -AuthOut (New-GoodAuthOut) -Name 'otherapp'
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw "*is for app 'other-engine'*"
    }

    It 'refuses an auth output left behind by a DIFFERENT deploy — the overwrite hazard' {
        # The file is one fixed path in Scripts\, rewritten by every engine run.
        $fx = New-Fixture -Summary (New-GoodSummary) -AuthOut (New-GoodAuthOut -AppName 'previous-engine') -Name 'stale'
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw "*belongs to 'previous-engine'*overwritten by every deploy*"
    }

    It 'reports a missing auth output as a skipped auth provisioning' {
        $fx = New-Fixture -Summary (New-GoodSummary) -AuthOut (New-GoodAuthOut) -Name 'noauth' -NoAuthOut
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw '*SkipAuthProvisioning*'
    }

    It 'refuses to continue when ClientId is missing' {
        $a = New-GoodAuthOut; $a.ClientId = ''
        $fx = New-Fixture -Summary (New-GoodSummary) -AuthOut $a -Name 'noclient'
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Throw '*cannot authenticate*'
    }

    It 'reports an empty log directory rather than deploying an unwired worker' {
        $empty = Join-Path $TestDrive 'emptylog'
        New-Item -ItemType Directory -Force -Path $empty | Out-Null
        { Resolve-WwEngineHandover -LogDir $empty -AuthOutputPath (Join-Path $empty 'x.json') -ExpectedAppName 'wwengine-test' } |
            Should -Throw '*No non-dry-run engine summary*'
    }

    It 'warns via Notes — not a throw — when the QueueProcessor app role is undefined' {
        $a = New-GoodAuthOut
        $a.AppRoles = @(@{ value = 'Warewolf_Administrator'; id = 'cccccccc-0000-0000-0000-000000000000' })
        $fx = New-Fixture -Summary (New-GoodSummary) -AuthOut $a -Name 'norole'
        $h = Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test'
        $h.QueueProcessorRoleId | Should -BeNullOrEmpty
        ($h.Notes -join ' ')    | Should -Match 'Warewolf_QueueProcessor'
    }

    It 'warns via Notes when the Entra app name differs from the derived default' {
        $a = New-GoodAuthOut; $a.EntraAppDisplayName = 'custom-auth-app'
        $fx = New-Fixture -Summary (New-GoodSummary) -AuthOut $a -Name 'customname'
        $h = Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath `
                                      -ExpectedAppName 'wwengine-test' -ExpectedAuthAppName 'wwengine-test-auth'
        ($h.Notes -join ' ')     | Should -Match 'custom-auth-app'
        $h.EntraAppDisplayName   | Should -Be 'custom-auth-app'
    }

    It 'survives an output file missing an optional field (StrictMode-safe reads)' {
        $s = New-GoodSummary; $s.Remove('appInsightsName')
        $fx = New-Fixture -Summary $s -AuthOut (New-GoodAuthOut) -Name 'nofield'
        { Resolve-WwEngineHandover -LogDir $fx.LogDir -AuthOutputPath $fx.AuthPath -ExpectedAppName 'wwengine-test' } |
            Should -Not -Throw
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — handover file' {

    BeforeAll {
        . $script:DeployScript -LoadFunctionsOnly
        $script:Ho = @{
            RunId = 'wwx-20260827-101500'; Endpoint = 'https://wwengine-test.azurewebsites.net'
            AppInsightsName = 'wwengine-test-ai'; EntraAppDisplayName = 'wwengine-test-auth'
            SummaryPath = 'D:\logs\deploy-WwExecutionEngine-20260827-101500.summary.json'
            ClientId = '11111111-2222-3333-4444-555555555555'
            SpObjectId = 'bbbbbbbb-0000-0000-0000-000000000000'
            Audience = 'api://11111111-2222-3333-4444-555555555555'
            QueueProcessorRoleId = 'dddddddd-0000-0000-0000-000000000000'
            AuthOutputPath = 'D:\Scripts\Configure-WwExecutionAuth.output.json'
        }
    }

    It 'carries everything the app-role grant needs' {
        $h = New-WwHandoverObject -Context (New-Ctx) -Handover $script:Ho -QueueProcessorApps @('wwqp-orders')
        $h.entra.spObjectId              | Should -Not -BeNullOrEmpty
        $h.entra.queueProcessorAppRoleId | Should -Not -BeNullOrEmpty
        $h.engine.resourceGroup          | Should -Be 'rg-1'
        $h.queueProcessor.apps           | Should -Contain 'wwqp-orders'
    }

    It 'carries the rollback tag and summary path' {
        $h = New-WwHandoverObject -Context (New-Ctx) -Handover $script:Ho
        $h.resourceTags       | Should -Contain 'wwx-test-run=wwx-20260827-101500'
        $h.engine.summaryPath | Should -Be $script:Ho.SummaryPath
    }

    It 'NEVER records a secret value or the App Insights connection string' {
        $ctx = New-Ctx
        $json = New-WwHandoverObject -Context $ctx -Handover $script:Ho | ConvertTo-Json -Depth 8
        $json | Should -Not -Match 'InstrumentationKey'
        $json | Should -Not -Match 'IngestionEndpoint'
        $json | Should -Not -Match 'amqp://'
        # The vault and secret NAME are recorded; the secret VALUE is not.
        $json | Should -Match '"name":\s*"kv-1"'
        $json | Should -Match 'rabbitmq-uri'
    }

    It 'marks a dry-run handover as synthetic so downstream steps can refuse it' {
        $h = New-WwHandoverObject -Context (New-Ctx) -Handover $script:Ho -Synthetic $true -DryRun $true
        $h.synthetic | Should -BeTrue
        $h.dryRun    | Should -BeTrue
    }

    It 'defaults to a non-synthetic real run' {
        $h = New-WwHandoverObject -Context (New-Ctx) -Handover $script:Ho
        $h.synthetic | Should -BeFalse
    }

    It 'round-trips through JSON with the documented shape' {
        $h = New-WwHandoverObject -Context (New-Ctx) -Handover $script:Ho -QueueProcessorApps @('wwqp-a', 'wwqp-b')
        $r = $h | ConvertTo-Json -Depth 8 | ConvertFrom-Json
        $r.producedBy                | Should -Be 'Deploy-WwEngineAndQueueProcessor.ps1'
        $r.engine.endpoint           | Should -Be 'https://wwengine-test.azurewebsites.net'
        $r.entra.clientId            | Should -Be '11111111-2222-3333-4444-555555555555'
        $r.keyVault.rabbitMqSecretUri| Should -Match '/secrets/rabbitmq-uri$'
        $r.queueProcessor.apps.Count | Should -Be 2
    }
}

Describe 'Deploy-WwEngineAndQueueProcessor — misc helpers' {

    BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

    Context 'Get-WwProp' {
        It 'returns the value when present' {
            Get-WwProp ([pscustomobject]@{ a = 1 }) 'a' | Should -Be 1
        }
        It 'returns the default for a missing property instead of throwing under StrictMode' {
            Get-WwProp ([pscustomobject]@{ a = 1 }) 'missing' 'fallback' | Should -Be 'fallback'
        }
        It 'returns the default for a null object' {
            Get-WwProp $null 'a' 'fallback' | Should -Be 'fallback'
        }
    }

    Context 'Get-WwMasked' {
        It 'masks a short value entirely' { Get-WwMasked 'abc' | Should -Be '********' }
        It 'shows only the first and last four characters' {
            Get-WwMasked 'InstrumentationKey=1234567890abcdef' | Should -Be 'Inst...cdef'
        }
        It 'handles an empty value' { Get-WwMasked '' | Should -Be '' }
    }
}
