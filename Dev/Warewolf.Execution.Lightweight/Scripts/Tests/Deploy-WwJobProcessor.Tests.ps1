#Requires -Version 7.0

<#
    Pester 5 test suite for Deploy-WwJobProcessor.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Deploy-WwJobProcessor.Tests.ps1

    Design notes (identical conventions to Deploy-WwExecutionEngine.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no cloud/filesystem action.
    * `az`/`func` are shadowed with tracking *functions* (functions win over
      applications), so the suite runs identically with or without the Azure CLI.
    * End-to-end behaviour is exercised in -DryRun: every mutating action is
      printed (not performed); staging targets a fresh dir under the OS temp path
      ('wwjobprocessor-stage-<AppName>-<stamp>[-dryrun]'), so the publish output is
      never modified and the processor stages separately from the engine.
    * The $global:resourcesExist toggle must be $global: — the az shim is invoked
      from inside `& $DeployScript`.
#>

BeforeAll {
    $script:DeployScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwJobProcessor.ps1'
}

Describe 'Deploy-WwJobProcessor — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'rejects an invalid -PublishMethod via ValidateSet' {
        { & $script:DeployScript -PublishMethod 'NotAMethod' -LoadFunctionsOnly } | Should -Throw
    }

    It 'rejects an invalid -ExecutionLogLevel via ValidateSet' {
        { & $script:DeployScript -ExecutionLogLevel 'LOUD' -LoadFunctionsOnly } | Should -Throw
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:DeployScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Pre-flight'
        Get-Command Resolve-Toggle     -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Save-DeploySummary -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Invoke-Az          -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'defaults the timer cadences on the parameter block' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$null)
        $params = $ast.ParamBlock.Parameters
        $poll   = $params | Where-Object { $_.Name.VariablePath.UserPath -eq 'JobPollSchedule' }
        $reaper = $params | Where-Object { $_.Name.VariablePath.UserPath -eq 'JobReaperSchedule' }
        $poll.DefaultValue.Value   | Should -Be '0 */1 * * * *'
        $reaper.DefaultValue.Value | Should -Be '0 */5 * * * *'
    }
}

Describe 'Deploy-WwJobProcessor — Graph /me call sites' {

    It 'reaches the Graph /me endpoint from exactly one place (build 30696 guard)' {
        # Any NEW `az ad signed-in-user show` outside Resolve-CurrentPrincipal would
        # reintroduce the service-principal failure that broke the LOADTEST deploy on
        # the sibling Deploy-WwExecutionEngine.ps1.
        $src = Get-Content -LiteralPath $script:DeployScript -Raw
        ([regex]::Matches($src, "'signed-in-user', 'show'")).Count | Should -Be 1
    }
}

Describe 'Deploy-WwJobProcessor — helper functions' {

    BeforeAll {
        . $script:DeployScript -LoadFunctionsOnly
    }

    Context 'Read-Required' {
        It 'returns the current value when supplied' { Read-Required -Name 'AppName' -Current 'wwjp' | Should -Be 'wwjp' }
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

    Context 'Resolve-CurrentPrincipal' {
        <#
            Mirrors the sibling guard in Deploy-WwExecutionEngine.Tests.ps1. `az ad
            signed-in-user show` is a Microsoft Graph /me call, valid only for a DELEGATED
            token; under `az login --service-principal` it fails with "/me request is only
            valid with delegated authentication flow" - the failure that killed the LOADTEST
            engine deploy (build 30696). This script carried the identical assumption.
        #>
        BeforeEach {
            $global:azSeen      = [System.Collections.Generic.List[string]]::new()
            $global:spShowFails = $false
            $global:accountJson = '{"user":{"name":"dev@x","type":"user"}}'
            function az {
                $a = $args; $global:LASTEXITCODE = 0
                $global:azSeen.Add(($a -join ' '))
                if ($a[0] -eq 'account' -and $a -contains 'show') {
                    if (-not $global:accountJson) { $global:LASTEXITCODE = 1; return $null }
                    return $global:accountJson
                }
                if ($a[0] -eq 'ad' -and $a[1] -eq 'sp') {
                    if ($global:spShowFails) { $global:LASTEXITCODE = 1; return $null }
                    return 'sp-oid-999'
                }
                if ($a[0] -eq 'ad' -and $a[1] -eq 'signed-in-user') { return 'user-oid-111' }
                return '{}'
            }
        }

        AfterEach {
            Remove-Variable -Name azSeen, spShowFails, accountJson -Scope Global -ErrorAction SilentlyContinue
        }

        It 'resolves an interactive user through the Graph /me endpoint' {
            $p = Resolve-CurrentPrincipal
            $p.ObjectId      | Should -Be 'user-oid-111'
            $p.PrincipalType | Should -Be 'User'
        }

        It 'resolves a service principal WITHOUT touching signed-in-user (build 30696)' {
            $global:accountJson = '{"user":{"name":"appid-abc","type":"servicePrincipal"}}'
            $p = Resolve-CurrentPrincipal
            $p.ObjectId      | Should -Be 'sp-oid-999'
            $p.PrincipalType | Should -Be 'ServicePrincipal'
            ($global:azSeen -join '|') | Should -Match 'ad sp show --id appid-abc'
            ($global:azSeen -join '|') | Should -Not -Match 'signed-in-user'
        }

        It 'returns null when a service principal cannot look itself up in Graph' {
            $global:accountJson = '{"user":{"name":"appid-abc","type":"servicePrincipal"}}'
            $global:spShowFails = $true
            Resolve-CurrentPrincipal | Should -BeNullOrEmpty
        }

        It 'returns null when there is no az session at all' {
            $global:accountJson = ''
            Resolve-CurrentPrincipal | Should -BeNullOrEmpty
        }
    }

    Context 'Format-AzArgsForLog' {
        It 'masks a *_SECRET setting but keeps a *_NAME setting' {
            $line = Format-AzArgsForLog @('appsettings', 'set', '--settings', 'MY_SECRET=abc123', 'KEYVAULT_SECRET_NAME=dp-keyring-v1')
            $line | Should -Match 'MY_SECRET=\*\*\*REDACTED\*\*\*'
            $line | Should -Match 'KEYVAULT_SECRET_NAME=dp-keyring-v1'
        }
        It 'masks the value after --client-secret' {
            $line = Format-AzArgsForLog @('ad', 'app', 'credential', 'reset', '--client-secret', 'topsecret')
            $line | Should -Match '--client-secret \*\*\*REDACTED\*\*\*'
        }
    }
}

Describe 'Deploy-WwJobProcessor — end-to-end (DryRun, no side effects)' {

    BeforeEach {
        $global:resourcesExist = $true
        $global:appNetFx       = $null
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a[0] -eq 'group'       -and $a -contains 'exists') { return ($global:resourcesExist ? 'true' : 'false') }
            if ($a[0] -eq 'storage'     -and $a -contains 'show')   { if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null } return '{"name":"st"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'show')   {
                if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null }
                # $global:appNetFx lets a test place an EXISTING app on a specific runtime.
                # $null keeps the original siteConfig-less payload every other test relies on.
                if ($null -eq $global:appNetFx) { return '{"name":"app"}' }
                return ('{"name":"app","siteConfig":{"netFrameworkVersion":"' + $global:appNetFx + '"}}')
            }
            if ($a[0] -eq 'keyvault'    -and $a -contains 'secret' -and $a -contains 'show') { $global:LASTEXITCODE = 1; return $null }
            return '{}'
        }

        # Throwaway publish dir + a temp Settings source dir with the exact-named pair.
        $global:pubDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwjppub-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path $global:pubDir -Force | Out-Null
        '{ "version": "2.0" }' | Set-Content (Join-Path $global:pubDir 'host.json')

        $global:srcDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwjpsrc-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path $global:srcDir -Force | Out-Null
        $global:psFile = Join-Path $global:srcDir 'persistencesettings.json'
        $global:dbFile = Join-Path $global:srcDir 'persistencesettingsdbsource.bite'
        '{ "Enable": true, "PersistenceScheduler": "Hangfire" }' | Set-Content $global:psFile
        '<Source ConnectionString="Data Source=x;Initial Catalog=Hangfire" />' | Set-Content $global:dbFile

        $global:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwjplog-" + [guid]::NewGuid())

        $script:commonArgs = @{
            SubscriptionId          = 'sub-123'
            TenantId                = 'tid-456'
            ResourceGroup           = 'DEV2'
            Location                = 'southafricanorth'
            StorageAccount          = 'stwwjobproc'
            AppName                 = 'wwjobproc'
            PublishPath             = $global:pubDir
            LogDir                  = $global:logDir
            PersistenceSettingsPath = $global:psFile
            PersistenceDbSourcePath = $global:dbFile
            EngineResumeBaseUrl     = 'https://wwengine.azurewebsites.net'
            EngineResumeScope       = 'api://engine-app-id/.default'
            EncryptResources        = $false
            EnableAppInsights       = $false
            NonInteractive          = $true
            DryRun                  = $true
        }
    }

    AfterEach {
        foreach ($d in @($global:pubDir, $global:srcDir, $global:logDir)) {
            if ($d -and (Test-Path -LiteralPath $d)) { Remove-Item -LiteralPath $d -Recurse -Force -ErrorAction SilentlyContinue }
        }
        Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory -Filter 'wwjobprocessor-stage-*' -ErrorAction SilentlyContinue |
            ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
    }

    AfterAll {
        Remove-Variable -Name resourcesExist -Scope Global -ErrorAction SilentlyContinue
        Remove-Variable -Name appNetFx       -Scope Global -ErrorAction SilentlyContinue
        Remove-Variable -Name pubDir, srcDir, logDir, psFile, dbFile -Scope Global -ErrorAction SilentlyContinue
    }

    It 'runs every phase to completion' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Phase 0  Pre-flight'
        $out | Should -Match 'Phase 0.5  Plan'
        $out | Should -Match 'Phase 1  Infrastructure'
        $out | Should -Match 'Phase 2  Key Vault \+ managed identity'
        $out | Should -Match 'Phase 3  Stage persistence settings'
        $out | Should -Match 'Phase 4  Deploy package'
        $out | Should -Match 'Phase 5  Verify'
        $out | Should -Match 'Dry-run complete'
        $out | Should -Match 'Summary written to'
    }

    # The processor never reads a logging toggle (it does not use Warewolf.Execution.Lightweight's
    # LoggingConfiguration), so these settings were written and ignored - under the pre-WOLF-8516
    # names and equally under WAREWOLF_LOGGING_CONFIG.
    It 'writes no logging-toggle app settings the processor does not read' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'JOB_POLL_SCHEDULE' -Because 'the app settings must be in the output for the absence checks to mean anything'
        foreach ($dead in 'ENABLECONSOLELOGGING', 'ENABLEAPPLICATIONINSIGHTS', 'STRUCTURED_LOGS', 'WAREWOLF_LOGGING_CONFIG') {
            $out | Should -Not -Match "\b$dead\b"
        }
    }

    It 'accepts -EnableConsoleLogging and -StructuredLogs for compatibility but says they are ignored' {
        $a = $script:commonArgs.Clone(); $a.EnableConsoleLogging = $true; $a.StructuredLogs = $false
        $out = (& $script:DeployScript @a) 6>&1 | Out-String
        $out | Should -Match '-EnableConsoleLogging is ignored'
        $out | Should -Match '-StructuredLogs is ignored'
        $out | Should -Match 'Dry-run complete'
    }

    # WorkerTelemetry.cs reads the STANDARD connection-string name. Setup-ApplicationInsights.ps1
    # defaults to the engine's WAREWOLF_ one, which this processor never reads - wwuat3-sbworker's App
    # Insights had 0 rows in every table because of exactly that.
    It 'provisions App Insights under the standard connection-string name, without the engine-only flag' {
        $a = $script:commonArgs.Clone(); $a.EnableAppInsights = $true
        $out = (& $script:DeployScript @a) 6>&1 | Out-String
        $out | Should -Match 'APPLICATIONINSIGHTS_CONNECTION_STRING\s*= \(auto-read'
        $out | Should -Not -Match 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING'
        # DryRun renders the child script's parameter NAMES only, so the value is pinned statically below.
        $call = ($out -split "`n" | Where-Object { $_ -match "\[DRYRUN\] & 'Setup-ApplicationInsights\.ps1'" } | Select-Object -First 1)
        $call | Should -Not -BeNullOrEmpty
        $call | Should -Match '-ConnectionStringSettingName'
        $call | Should -Match '-SkipLoggingConfigFlag'
        (Get-Content -LiteralPath $script:DeployScript -Raw) |
            Should -Match "ConnectionStringSettingName\s*=\s*'APPLICATIONINSIGHTS_CONNECTION_STRING'"
    }

    It 'does not prompt for console-logging or structured-logs toggles' {
        $text = Get-Content -LiteralPath $script:DeployScript -Raw
        $text | Should -Not -Match "Resolve-Toggle -Name 'EnableConsoleLogging'"
        $text | Should -Not -Match "Resolve-Toggle -Name 'StructuredLogs'"
    }

    It 'applies the JobProcessor + resume-dispatch app settings' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'JOB_POLL_SCHEDULE\s*= 0 \*/1 \* \* \* \*'
        $out | Should -Match 'JOB_REAPER_SCHEDULE\s*= 0 \*/5 \* \* \* \*'
        $out | Should -Match 'JOB_STALE_MINUTES\s*= 15'
        $out | Should -Match 'ENGINE_RESUME_BASEURL\s*= https://wwengine.azurewebsites.net'
        $out | Should -Match 'ENGINE_RESUME_TIMEOUT_SECONDS\s*= 15'
        $out | Should -Match 'ENGINE_RESUME_AUTH_DISABLED\s*= false'
        $out | Should -Match 'ENGINE_RESUME_SCOPE\s*= api://engine-app-id/.default'
        $out | Should -Match '\[DRYRUN\] az functionapp config appsettings set'
    }

    It 'always enables the system-assigned managed identity' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az functionapp identity assign'
    }

    It 'plans resource creation when resources are missing' {
        $global:resourcesExist = $false
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az group create'
        $out | Should -Match '\[DRYRUN\] az storage account create'
        $out | Should -Match '\[DRYRUN\] az functionapp create'
    }

    It 'Auto deploy uses az zip-deploy (config-zip), never func (pre-built artifact)' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\].*config-zip'
        $out | Should -Not -Match 'func azure functionapp publish'
    }

    It 'stages the persistence pair into Settings\ of a temp staging dir (separate from the publish output), leaving the real publish dir untouched' {
        & $script:DeployScript @commonArgs 6>&1 | Out-Null

        # The real publish dir must be unchanged (only the seeded host.json) and must
        # NOT have gained a Settings\ folder.
        @(Get-ChildItem -LiteralPath $global:pubDir -Recurse -File).Count | Should -Be 1
        Test-Path (Join-Path $global:pubDir 'Settings') | Should -BeFalse

        # The staging dir lives under the OS temp path (NOT beside the publish dir) and
        # carries the staged pair.
        $preview = Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory `
            -Filter 'wwjobprocessor-stage-wwjobproc-*' -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime | Select-Object -Last 1
        $preview | Should -Not -BeNullOrEmpty
        $preview.Name | Should -BeLike '*-dryrun'
        Test-Path (Join-Path $preview.FullName 'Settings\persistencesettings.json')       | Should -BeTrue
        Test-Path (Join-Path $preview.FullName 'Settings\persistencesettingsdbsource.bite') | Should -BeTrue
    }

    It 'stages the DbSource AS-IS when -EncryptResources is off (no WFAES pass)' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'DbSource staged AS-IS'
        $out | Should -Not -Match 'Encrypting persistence DbSource'
    }

    It 'writes a dry-run summary carrying the dryRun flag' {
        & $script:DeployScript @commonArgs 6>&1 | Out-Null
        $summaryFile = Get-ChildItem -LiteralPath $global:logDir -Filter 'deploy-WwJobProcessor-*.dryrun.summary.json' |
            Sort-Object LastWriteTime | Select-Object -Last 1
        $summaryFile | Should -Not -BeNullOrEmpty
        $summary = Get-Content $summaryFile.FullName -Raw | ConvertFrom-Json
        $summary.dryRun | Should -BeTrue
        $summary.status | Should -Be 'completed'
        $summary.appSettings.JOB_POLL_SCHEDULE | Should -Be '0 */1 * * * *'
    }

    # ── Fail-loud validation (reuses the BeforeEach shim + files) ───────────────

    It 'throws when -PersistenceSettingsPath is missing (NonInteractive)' {
        $a = $script:commonArgs.Clone(); $a.Remove('PersistenceSettingsPath')
        { & $script:DeployScript @a } | Should -Throw '*PersistenceSettingsPath*'
    }

    It 'throws when the persistence settings file has the wrong name' {
        $wrong = Join-Path $global:srcDir 'wrongname.json'
        '{ "Enable": true }' | Set-Content $wrong
        $a = $script:commonArgs.Clone(); $a.PersistenceSettingsPath = $wrong
        { & $script:DeployScript @a } | Should -Throw '*must be named exactly*'
    }

    It 'throws when -EngineResumeBaseUrl is missing (NonInteractive)' {
        $a = $script:commonArgs.Clone(); $a.Remove('EngineResumeBaseUrl')
        { & $script:DeployScript @a } | Should -Throw '*EngineResumeBaseUrl*'
    }

    It 'requires -EngineResumeScope only when auth is enabled' {
        $a = $script:commonArgs.Clone(); $a.Remove('EngineResumeScope')
        { & $script:DeployScript @a } | Should -Throw '*EngineResumeScope*'

        # With auth disabled the scope is not required — the run proceeds.
        $b = $script:commonArgs.Clone(); $b.Remove('EngineResumeScope'); $b.EngineResumeAuthDisabled = $true
        $out = (& $script:DeployScript @b) 6>&1 | Out-String
        $out | Should -Match 'ENGINE_RESUME_AUTH_DISABLED\s*= true'
        $out | Should -Not -Match 'ENGINE_RESUME_SCOPE\s*='
    }

    # ── .NET runtime (create + reconcile) ──────────────────────────────────────
    # `functionapp create` is the ONLY place --runtime-version applies, so an app created
    # by an earlier run keeps the runtime it was born with. Deploying a net10.0 package
    # onto a v8.0 host gives a deploy that reports success and a worker that never starts.

    It 'creates a NEW app on dotnet-isolated 10, never 8' {
        $global:resourcesExist = $false
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az functionapp create .*--runtime dotnet-isolated --runtime-version 10'
        $out | Should -Not -Match '--runtime-version 8'
    }

    It 'states the runtime decision in the plan' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Runtime\s+: dotnet-isolated 10 \(Functions v4\)'
    }

    It 'upgrades an existing v8.0 app when -UpgradeRuntimeVersion is passed' {
        $global:appNetFx = 'v8.0'
        $out = (& $script:DeployScript @commonArgs -UpgradeRuntimeVersion) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az functionapp config set .*--net-framework-version v10\.0'
    }

    It 'does NOT touch an existing v8.0 app without the switch, but warns' {
        # Anchor on the [DRYRUN] marker, not the bare command: the warning QUOTES the
        # remediation command, so a looser pattern passes either way.
        $global:appNetFx = 'v8.0'
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match '\[DRYRUN\] az functionapp config set'
        $out | Should -Match 'RUNTIME MISMATCH'
    }

    It 'does nothing when the existing app is already v10.0 (no needless restart)' {
        $global:appNetFx = 'v10.0'
        $out = (& $script:DeployScript @commonArgs -UpgradeRuntimeVersion) 6>&1 | Out-String
        $out | Should -Not -Match '\[DRYRUN\] az functionapp config set'
        $out | Should -Not -Match 'RUNTIME MISMATCH'
    }

    It 'exposes -UpgradeRuntimeVersion as an off-by-default switch' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$null)
        $p   = $ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'UpgradeRuntimeVersion' }
        $p                    | Should -Not -BeNullOrEmpty
        $p.StaticType.Name    | Should -Be 'SwitchParameter'
        $p.DefaultValue       | Should -BeNullOrEmpty
    }
}
