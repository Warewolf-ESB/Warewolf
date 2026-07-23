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
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a[0] -eq 'group'       -and $a -contains 'exists') { return ($global:resourcesExist ? 'true' : 'false') }
            if ($a[0] -eq 'storage'     -and $a -contains 'show')   { if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null } return '{"name":"st"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'show')   { if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null } return '{"name":"app"}' }
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
}
