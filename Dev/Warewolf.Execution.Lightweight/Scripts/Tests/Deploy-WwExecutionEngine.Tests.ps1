#Requires -Version 7.0

<#
    Pester 5 test suite for Deploy-WwExecutionEngine.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Deploy-WwExecutionEngine.Tests.ps1

    Design notes
    ------------
    * The script exposes -LoadFunctionsOnly so its helper functions can be
      dot-sourced and unit-tested without executing any cloud / filesystem action.
    * `az`/`func`/`dotnet` are shadowed with tracking *functions* (functions win
      over applications in command resolution), so the suite runs identically
      whether or not the Azure CLI is installed.
    * End-to-end behaviour is exercised in -DryRun: every mutating action is
      printed (not performed) and nothing is written to disk or the cloud.
    * The az shim branches on $a[0] (the command group) so "storage account show"
      is never mistaken for the top-level "account show" probe, and the
      $global:resourcesExist toggle must be $global: because the shim is invoked
      from inside `& $DeployScript`.
#>

BeforeAll {
    $script:DeployScript        = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwExecutionEngine.ps1'
    $script:ExampleAuthConfig   = Join-Path (Split-Path $PSScriptRoot -Parent) 'Deploy-WwExecutionEngine.authconfig.example.json'
    $script:ExampleSecureConfig = Join-Path (Split-Path $PSScriptRoot -Parent) 'secure.config.example.json'
    $script:CloudSecureConfig   = Join-Path (Split-Path $PSScriptRoot -Parent) 'secure.config.cloud.json'
}

Describe 'Deploy-WwExecutionEngine — static' {

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

    It 'assigns roles by object id + principal type, never by --assignee' {
        # REGRESSION (operator machine). `--assignee` makes az resolve the principal through
        # Microsoft Graph. A system-assigned managed identity enabled seconds earlier has not
        # replicated there yet, so Phase 3 died with:
        #   Cannot find user or service principal in graph database for '<principalId>'
        # The object-id form skips the lookup, matching Deploy-WwQueueProcessor.ps1.
        $src = Get-Content $script:DeployScript -Raw
        $src | Should -Not -Match "'role',\s*'assignment',\s*'create'[^)]*'--assignee',"
        $src | Should -Match '--assignee-object-id'
        $src | Should -Match '--assignee-principal-type'
    }

    It 'retries the role assignment while the directory replicates' {
        . $script:DeployScript -LoadFunctionsOnly
        $cmd = Get-Command Grant-RoleAssignment -CommandType Function -ErrorAction SilentlyContinue
        $cmd | Should -Not -BeNullOrEmpty
        $cmd.Parameters['MaxAttempts'].Attributes.Where({ $_ -is [System.Management.Automation.ParameterAttribute] }) |
            Should -Not -BeNullOrEmpty
        # PrincipalType must be constrained - a User assigned as ServicePrincipal silently fails.
        ($cmd.Parameters['PrincipalType'].Attributes |
            Where-Object { $_ -is [System.Management.Automation.ValidateSetAttribute] }).ValidValues |
            Should -Contain 'User'
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:DeployScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Pre-flight'
        Get-Command Resolve-Toggle -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Test-SecureConfig -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'exposes the persistence + JobProcessor parameters (DeployJobProcessor is an off-by-default switch)' {
        $ast   = [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$null)
        $names = $ast.ParamBlock.Parameters | ForEach-Object { $_.Name.VariablePath.UserPath }
        foreach ($p in 'EnablePersistence','PersistenceSettingsPath','PersistenceDbSourcePath',
                       'DeployJobProcessor','JobProcessorAppName','JobProcessorPublishPath',
                       'JobProcessorStorageAccount','EngineResumeScope') {
            $names | Should -Contain $p
        }
        $dj = $ast.ParamBlock.Parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'DeployJobProcessor' }
        $dj.StaticType.Name | Should -Be 'SwitchParameter'   # switch -> off unless passed
    }
}

Describe 'Deploy-WwExecutionEngine — auth config template' {
    It 'ships a valid JSON example that maps to the expected shapes' {
        Test-Path $script:ExampleAuthConfig | Should -BeTrue
        $cfg = Get-Content $script:ExampleAuthConfig -Raw | ConvertFrom-Json -AsHashtable
        $cfg.GroupPermissions | Should -BeOfType [hashtable]
        @($cfg.UserAssignments).Count | Should -BeGreaterThan 0
    }

    It 'ships the dedicated Warewolf_ClientApps group for app-only client apps' {
        $cfg = Get-Content $script:ExampleAuthConfig -Raw | ConvertFrom-Json -AsHashtable
        $cfg.GroupPermissions.Keys | Should -Contain 'Warewolf_ClientApps'
    }

    It 'maps every UserAssignments group to an existing GroupPermissions key' {
        # Guards against group-name drift/typos (e.g. Adminstrators vs Administrators):
        # a UPN mapped to a non-existent group would silently receive only a group
        # role that no GroupPermissions entry defines.
        $cfg = Get-Content $script:ExampleAuthConfig -Raw | ConvertFrom-Json -AsHashtable
        foreach ($u in @($cfg.UserAssignments)) {
            $cfg.GroupPermissions.Keys | Should -Contain $u.Group
        }
    }
}

Describe 'Deploy-WwExecutionEngine — secure.config templates' {
    # The Warewolf_ClientApps app role only authorizes app-only callers when
    # secure.config carries a matching per-workflow WindowsGroup row — these
    # tests keep the templates in lockstep with the authconfig example.

    It 'example template grants Warewolf_ClientApps per-workflow View+Execute' {
        $cfg  = Get-Content $script:ExampleSecureConfig -Raw | ConvertFrom-Json
        $rows = @($cfg.WindowsGroupPermissions | Where-Object { $_.WindowsGroup -eq 'Warewolf_ClientApps' })
        $rows.Count | Should -BeGreaterThan 0
        foreach ($row in $rows) {
            $row.IsServer | Should -BeFalse
            [string]::IsNullOrWhiteSpace($row.ResourceName) | Should -BeFalse
            $row.View     | Should -BeTrue
            $row.Execute  | Should -BeTrue
        }
    }

    It 'cloud template grants Warewolf_ClientApps per-workflow View+Execute' {
        $cfg  = Get-Content $script:CloudSecureConfig -Raw | ConvertFrom-Json
        $rows = @($cfg.WindowsGroupPermissions | Where-Object { $_.WindowsGroup -eq 'Warewolf_ClientApps' })
        $rows.Count | Should -BeGreaterThan 0
        foreach ($row in $rows) {
            $row.IsServer | Should -BeFalse
            [string]::IsNullOrWhiteSpace($row.ResourceName) | Should -BeFalse
            $row.View     | Should -BeTrue
            $row.Execute  | Should -BeTrue
        }
    }
}

Describe 'Deploy-WwExecutionEngine — helper functions' {

    BeforeAll {
        . $script:DeployScript -LoadFunctionsOnly
    }

    Context 'Test-CommandExists' {
        It 'returns true for a command that exists' { Test-CommandExists 'Get-Command' | Should -BeTrue }
        It 'returns false for a command that does not exist' { Test-CommandExists 'definitely_not_a_real_command_4f2a' | Should -BeFalse }
    }

    Context 'Confirm-Yes (non-interactive returns default)' {
        It 'returns true default'  { $NonInteractive = $true; Confirm-Yes 'x' $true  | Should -BeTrue }
        It 'returns false default' { $NonInteractive = $true; Confirm-Yes 'x' $false | Should -BeFalse }
    }

    Context 'Resolve-Toggle' {
        It 'uses an explicit $true regardless of mode' {
            $NonInteractive = $true
            Resolve-Toggle -Name 't' -Current $true -Default $false | Should -BeTrue
        }
        It 'uses an explicit $false regardless of mode' {
            $NonInteractive = $true
            Resolve-Toggle -Name 't' -Current $false -Default $true | Should -BeFalse
        }
        It 'returns the default when null and non-interactive' {
            $NonInteractive = $true
            Resolve-Toggle -Name 't' -Current $null -Default $true | Should -BeTrue
        }
        It 'prompts when null and interactive' {
            $NonInteractive = $false
            Mock Read-Host { 'y' }
            Resolve-Toggle -Name 't' -Current $null -Default $false | Should -BeTrue
        }
    }

    Context 'Read-Required' {
        It 'returns the current value when supplied' { Read-Required -Name 'AppName' -Current 'wwx' | Should -Be 'wwx' }
        It 'throws when empty and -NonInteractive' {
            $NonInteractive = $true
            { Read-Required -Name 'AppName' -Current '' } | Should -Throw '*Required value*'
        }
    }

    Context 'Invoke-Az' {
        BeforeEach {
            $script:azExit = 0; $script:azOut = '{}'; $script:azCalls = 0
            function az { $script:azCalls++; $global:LASTEXITCODE = $script:azExit; $script:azOut }
        }
        It 'skips a mutating call under -DryRun and returns null' {
            $DryRun = $true
            Invoke-Az -Args @('group', 'create') -Mutating | Should -BeNullOrEmpty
            $script:azCalls | Should -Be 0
        }
        It 'returns null on failure when -AllowFail is set' {
            $DryRun = $false; $script:azExit = 1
            Invoke-Az -Args @('functionapp', 'show') -AllowFail | Should -BeNullOrEmpty
        }
        It 'throws on failure when -AllowFail is not set' {
            $DryRun = $false; $script:azExit = 1
            { Invoke-Az -Args @('functionapp', 'show') } | Should -Throw '*az CLI failed*'
        }
    }

    Context 'secure.config classification + crypto round-trip' {
        BeforeAll {
            $script:scDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwsc-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path $script:scDir -Force | Out-Null

            $script:plainPath = Join-Path $script:scDir 'plain.config'
            '{ "SecretKey": "abc", "WindowsGroupPermissions": [] }' | Set-Content -LiteralPath $script:plainPath -Encoding UTF8

            $script:encPath = Join-Path $script:scDir 'encrypted.config'
            Protect-SecureConfig -InPath $script:plainPath -OutPath $script:encPath

            $script:garbagePath = Join-Path $script:scDir 'garbage.config'
            'this is not json and not base64 @@@' | Set-Content -LiteralPath $script:garbagePath -Encoding UTF8
        }
        AfterAll {
            if (Test-Path -LiteralPath $script:scDir) { Remove-Item -LiteralPath $script:scDir -Recurse -Force }
        }

        It 'classifies plaintext JSON as Plaintext/Valid' {
            $r = Test-SecureConfig -Path $script:plainPath
            $r.Kind | Should -Be 'Plaintext'; $r.Valid | Should -BeTrue
        }
        It 'classifies an AES-encrypted config as Encrypted/Valid' {
            $r = Test-SecureConfig -Path $script:encPath
            $r.Kind | Should -Be 'Encrypted'; $r.Valid | Should -BeTrue
        }
        It 'round-trips: our Decrypt of Protect output recovers the original JSON' {
            $back = [WwSecureConfigCrypto]::Decrypt((Get-Content -LiteralPath $script:encPath -Raw)).TrimEnd([char]0).Trim()
            ($back | ConvertFrom-Json).SecretKey | Should -Be 'abc'
        }
        It 'classifies undecryptable content as Invalid' {
            (Test-SecureConfig -Path $script:garbagePath).Kind | Should -Be 'Invalid'
        }
    }

    Context 'logging-level helpers' {
        It 'maps Dev2 EXECUTIONLOGLEVEL to MEL' {
            Convert-ToMelLevel 'INFO'  | Should -Be 'Information'
            Convert-ToMelLevel 'debug' | Should -Be 'Debug'
            Convert-ToMelLevel 'FATAL' | Should -Be 'Critical'
            Convert-ToMelLevel 'OFF'   | Should -Be 'None'
        }
        It 'Read-ExecutionLogLevel returns the current value in non-interactive mode' {
            $NonInteractive = $true
            Read-ExecutionLogLevel -Current 'WARN' | Should -Be 'WARN'
        }
        It 'Read-ExecutionLogLevel prompts and normalises interactively' {
            $NonInteractive = $false
            Mock Read-Host { 'debug' }
            Read-ExecutionLogLevel -Current 'INFO' | Should -Be 'DEBUG'
        }
        It 'Update-HostJsonLogLevel sets default + Warewolf.* and leaves framework categories intact' {
            $hj = Join-Path ([System.IO.Path]::GetTempPath()) ("hj-" + [guid]::NewGuid() + '.json')
            '{ "logging": { "logLevel": { "default": "Information", "Microsoft.Azure.WebJobs": "Trace", "Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger": "Trace" } } }' |
                Set-Content -LiteralPath $hj -Encoding UTF8
            try {
                Update-HostJsonLogLevel -HostJsonPath $hj -ExecutionLogLevel 'ERROR' | Should -Be 'Error'
                $j = Get-Content -LiteralPath $hj -Raw | ConvertFrom-Json
                $j.logging.logLevel.default | Should -Be 'Error'
                $j.logging.logLevel.'Warewolf.Execution.Lightweight.Logging.AzureExecutionLogger' | Should -Be 'Error'
                $j.logging.logLevel.'Microsoft.Azure.WebJobs' | Should -Be 'Trace'
            } finally { Remove-Item -LiteralPath $hj -Force -ErrorAction SilentlyContinue }
        }
    }

    Context 'service-bus concurrency helper' {
        It 'Update-HostJsonServiceBusConcurrency sets maxConcurrentCalls and preserves sibling properties' {
            $hj = Join-Path ([System.IO.Path]::GetTempPath()) ("hj-" + [guid]::NewGuid() + '.json')
            '{ "extensions": { "serviceBus": { "autoCompleteMessages": false, "maxAutoLockRenewalDuration": "00:11:00" } } }' |
                Set-Content -LiteralPath $hj -Encoding UTF8
            try {
                Update-HostJsonServiceBusConcurrency -HostJsonPath $hj -MaxConcurrentCalls 2 | Should -BeTrue
                $j = Get-Content -LiteralPath $hj -Raw | ConvertFrom-Json
                $j.extensions.serviceBus.maxConcurrentCalls | Should -Be 2
                $j.extensions.serviceBus.autoCompleteMessages | Should -BeFalse
                $j.extensions.serviceBus.maxAutoLockRenewalDuration | Should -Be '00:11:00'
            } finally { Remove-Item -LiteralPath $hj -Force -ErrorAction SilentlyContinue }
        }
        It 'Update-HostJsonServiceBusConcurrency overwrites an already-set value (idempotent)' {
            $hj = Join-Path ([System.IO.Path]::GetTempPath()) ("hj-" + [guid]::NewGuid() + '.json')
            '{ "extensions": { "serviceBus": { "maxConcurrentCalls": 8 } } }' |
                Set-Content -LiteralPath $hj -Encoding UTF8
            try {
                Update-HostJsonServiceBusConcurrency -HostJsonPath $hj -MaxConcurrentCalls 2 | Should -BeTrue
                (Get-Content -LiteralPath $hj -Raw | ConvertFrom-Json).extensions.serviceBus.maxConcurrentCalls | Should -Be 2
            } finally { Remove-Item -LiteralPath $hj -Force -ErrorAction SilentlyContinue }
        }
        It 'Update-HostJsonServiceBusConcurrency no-ops (returns $false) when extensions.serviceBus is absent' {
            $hj = Join-Path ([System.IO.Path]::GetTempPath()) ("hj-" + [guid]::NewGuid() + '.json')
            '{ "logging": { "logLevel": { "default": "Information" } } }' | Set-Content -LiteralPath $hj -Encoding UTF8
            try {
                Update-HostJsonServiceBusConcurrency -HostJsonPath $hj -MaxConcurrentCalls 2 | Should -BeFalse
                (Get-Content -LiteralPath $hj -Raw | ConvertFrom-Json).PSObject.Properties['extensions'] | Should -BeNullOrEmpty
            } finally { Remove-Item -LiteralPath $hj -Force -ErrorAction SilentlyContinue }
        }
    }
}

Describe 'Deploy-WwExecutionEngine — end-to-end (DryRun, no side effects)' {

    BeforeEach {
        $global:resourcesExist = $true
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a[0] -eq 'group'       -and $a -contains 'exists') { return ($global:resourcesExist ? 'true' : 'false') }
            if ($a[0] -eq 'storage'     -and $a -contains 'show')   { if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null } return '{"name":"st"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'show')   { if (-not $global:resourcesExist) { $global:LASTEXITCODE = 1; return $null } return '{"name":"app"}' }
            # KV secret is NOT present in the unit harness -> keyReachable=$false in
            # dry-run, so real encryption (Encrypt-Config.ps1) is never invoked and
            # the source is staged unencrypted ("deferred"). Real encryption needs a
            # live Key Vault and belongs to integration, not these unit tests.
            if ($a[0] -eq 'keyvault'    -and $a -contains 'secret' -and $a -contains 'show') { $global:LASTEXITCODE = 1; return $null }
            return '{}'
        }

        # A throwaway publish directory (its contents are copied into a fresh temp
        # staging dir and staged into; the real dir is never modified by any run).
        $global:pubDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwpub-" + [guid]::NewGuid())
        New-Item -ItemType Directory -Path $global:pubDir -Force | Out-Null
        '{ "logging": { "logLevel": { "default": "Information" } } }' | Set-Content (Join-Path $global:pubDir 'host.json')
        $global:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwlog-" + [guid]::NewGuid())

        # Logging toggles default ON (AI) / OFF (ES); baseline pins both off
        # explicitly to keep most tests deterministic (no ES source / KV required).
        # UNIQUE PER TEST. The script names its staging dir
        # 'wwexecutionengine-stage-<AppName>-<yyyyMMdd-HHmmss>[-dryrun]' — a SECOND-resolution
        # stamp — so a fixed AppName made every test that ran inside the same second share one
        # directory. Combined with this block's AfterEach, which deletes every
        # 'wwexecutionengine-stage-*' globally, that intermittently removed a directory another
        # run was still using (or left one Windows still held a handle to, so the next run's
        # Remove-Item threw). Three tests failed on roughly one run in four. Unique names make
        # each run's staging dir its own.
        $script:appName = 'wwenginetest-' + [guid]::NewGuid().ToString('N').Substring(0, 8)

        $script:commonArgs = @{
            SubscriptionId      = 'sub-123'
            TenantId            = 'tid-456'
            ResourceGroup       = 'DEV2'
            Location            = 'southafricanorth'
            StorageAccount      = 'stwwenginetest'
            AppName             = $script:appName
            PublishPath         = $global:pubDir
            LogDir              = $global:logDir
            EncryptResources    = $false
            EnableAppInsights   = $false
            EnableElasticsearch = $false
            NonInteractive      = $true
            DryRun              = $true
        }
    }

    AfterEach {
        if ($global:pubDir -and (Test-Path -LiteralPath $global:pubDir)) { Remove-Item -LiteralPath $global:pubDir -Recurse -Force }
        if ($global:logDir -and (Test-Path -LiteralPath $global:logDir)) { Remove-Item -LiteralPath $global:logDir -Recurse -Force }
        # Temp staging dirs. SCOPED to this test's own AppName, deliberately: a global
        # 'wwexecutionengine-stage-*' sweep also removes directories belonging to runs other than
        # the one just finished, and the engine's own post-copy guard then fails with
        # "Staging directory '...' does not exist after resolution".
        Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory -Filter 'wwexecutionengine-stage-*' -ErrorAction SilentlyContinue |
            ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
    }

    AfterAll {
        Remove-Variable -Name resourcesExist -Scope Global -ErrorAction SilentlyContinue
        Remove-Variable -Name pubDir         -Scope Global -ErrorAction SilentlyContinue
        Remove-Variable -Name logDir         -Scope Global -ErrorAction SilentlyContinue
    }

    It 'runs every phase to completion' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Phase 0.5  Plan'
        $out | Should -Match 'Phase 1  Infrastructure'
        $out | Should -Match 'Phase 2  Auth provisioning'
        $out | Should -Match 'Phase 3  Stage package'
        $out | Should -Match 'Phase 4  Deploy package'
        $out | Should -Match 'Phase 5  Verify'
        $out | Should -Match 'Dry-run complete'
        $out | Should -Match 'Summary written to'
    }

    It 'does NOT plan resource creation when resources already exist' {
        $global:resourcesExist = $true
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match '\[DRYRUN\] az group create'
        $out | Should -Match 'already exists'
        $out | Should -Match 'building preview artifact'
    }

    It 'plans resource creation when resources are missing' {
        $global:resourcesExist = $false
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az group create'
        $out | Should -Match '\[DRYRUN\] az storage account create'
        $out | Should -Match '\[DRYRUN\] az functionapp create'
    }

    It 'plans env-var application and a publish step' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] az functionapp config appsettings set'
        $out | Should -Match '\[DRYRUN\].*(func azure functionapp publish|config-zip)'
    }

    It 'Auto deploy uses az zip-deploy (config-zip), never func (pre-built artifact)' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\].*config-zip'
        $out | Should -Not -Match 'func azure functionapp publish'
    }

    It 'PublishMethod Func uses func publish with --dotnet-isolated --no-build' {
        function func { }   # shim so Test-CommandExists 'func' succeeds in CI
        $callArgs = $script:commonArgs.Clone(); $callArgs.PublishMethod = 'Func'
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'func azure functionapp publish .*--dotnet-isolated --no-build'
    }

    It 'reflects explicit toggle values and the license default (on)' {
        # WOLF-8516: ENABLEAPPLICATIONINSIGHTS/ENABLEELASTICSEARCHLOGGING etc. were merged
        # into one WAREWOLF_LOGGING_CONFIG JSON app setting.
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'WAREWOLF_LICENSE_CHECK_ENABLED\s*= true'   # default on
        $out | Should -Match 'WAREWOLF_LOGGING_CONFIG\s*=.*"appInsights":false'   # explicitly off in baseline
        $out | Should -Match 'WAREWOLF_LOGGING_CONFIG\s*=.*"elasticsearch":false' # explicitly off in baseline
        $out | Should -Match 'Application Insights disabled'
    }

    It 'skips host.json logLevel alignment by default (opt-in only)' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'set host.json logLevel'
        $out | Should -Match 'host.json logLevel alignment skipped'
    }

    It 'aligns host.json with -AlignHostJsonLogLevel (INFO -> Information)' {
        $callArgs = $script:commonArgs.Clone(); $callArgs.AlignHostJsonLogLevel = $true
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match "Aligning host.json logLevel \(default \+ Warewolf\.\*\) -> 'Information'"
    }

    It 'maps a chosen EXECUTIONLOGLEVEL onto the host.json MEL level (opt-in)' {
        $callArgs = $script:commonArgs.Clone(); $callArgs.ExecutionLogLevel = 'DEBUG'; $callArgs.AlignHostJsonLogLevel = $true
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match "Aligning host.json logLevel \(default \+ Warewolf\.\*\) -> 'Debug'"
    }

    It 'skips host.json serviceBus maxConcurrentCalls override by default (opt-in only)' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'Setting host.json extensions.serviceBus.maxConcurrentCalls'
    }

    It 'sets host.json serviceBus maxConcurrentCalls with -ServiceBusMaxConcurrentCalls' {
        # The baseline fixture host.json (BeforeEach above) has no extensions.serviceBus
        # section, so this test writes one matching the real, checked-in host.json shape
        # (autoCompleteMessages + maxAutoLockRenewalDuration) before invoking the deploy.
        '{ "extensions": { "serviceBus": { "autoCompleteMessages": false, "maxAutoLockRenewalDuration": "00:11:00" } } }' |
            Set-Content (Join-Path $global:pubDir 'host.json')
        $callArgs = $script:commonArgs.Clone(); $callArgs.ServiceBusMaxConcurrentCalls = 2
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Setting host.json extensions.serviceBus.maxConcurrentCalls -> 2'
        $out | Should -Match 'host.json serviceBus maxConcurrentCalls set.'
    }

    It 'notes when -ServiceBusMaxConcurrentCalls is set but host.json has no extensions.serviceBus section' {
        # Baseline fixture host.json (BeforeEach above) has no extensions.serviceBus section.
        $callArgs = $script:commonArgs.Clone(); $callArgs.ServiceBusMaxConcurrentCalls = 2
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'host.json has no extensions.serviceBus section; skipping maxConcurrentCalls override.'
    }

    It 'uses the WAREWOLF_ App Insights variable (not the standard name) when AI enabled' {
        $callArgs = $script:commonArgs.Clone(); $callArgs.EnableAppInsights = $true
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING'
        $out | Should -Match 'WAREWOLF_LOGGING_CONFIG\s*=.*"appInsights":true'   # WOLF-8516
        # The host-pipeline name must never be set by this deployment.
        $out | Should -Not -Match 'APPLICATIONINSIGHTS_CONNECTION_STRING='
    }

    It 'honours -SkipAuthProvisioning' {
        $callArgs = $script:commonArgs.Clone(); $callArgs.SkipAuthProvisioning = $true
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Auth provisioning skipped'
    }

    It 'fixes ASPNETCORE_ENVIRONMENT to Production' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'ASPNETCORE_ENVIRONMENT\s*= Production'
    }

    It 'does NOT emit BYPASS_SECURE_CONFIG / WAREWOLF_SUPER_ADMIN_ENABLED / WAREWOLF_SECURITY_FLAGS (engine defaults apply)' {
        # WOLF-8516: these were merged into WAREWOLF_SECURITY_FLAGS, still never set by this script.
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'BYPASS_SECURE_CONFIG'
        $out | Should -Not -Match 'WAREWOLF_SUPER_ADMIN_ENABLED'
        $out | Should -Not -Match 'WAREWOLF_SECURITY_FLAGS'
    }

    It 'does NOT emit SkipFailureToRetrieveSecret / WAREWOLF_SECURITY_FLAGS even when Key Vault is required' {
        $esFile  = Join-Path $global:pubDir 'ElasticsearchLoggingSource.bite'
        '<Source><ConnectionString>x</ConnectionString></Source>' | Set-Content $esFile
        $callArgs = $script:commonArgs.Clone()
        $callArgs.EnableElasticsearch     = $true
        $callArgs.ElasticsearchSourcePath = $esFile
        $callArgs.KeyVaultName            = 'kv-test'
        $callArgs.KeyVaultSecretName      = 'dp-keyring-v1'
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'SkipFailureToRetrieveSecret'
        $out | Should -Not -Match 'WAREWOLF_SECURITY_FLAGS'
        # ES source is staged AS-IS (encryption off by default).
        $out | Should -Match 'Elasticsearch source staged AS-IS'
    }

    It 'no longer accepts the removed parameters' {
        { & $script:DeployScript @commonArgs -Environment 'Development' } |
            Should -Throw '*parameter cannot be found*'
        { & $script:DeployScript @commonArgs -BypassSecureConfig $true } |
            Should -Throw '*parameter cannot be found*'
    }

    It 'stages the Warewolf License.secureconfig when supplied' {
        $lic = Join-Path ([System.IO.Path]::GetTempPath()) ("Warewolf License.secureconfig")
        '<licence/>' | Set-Content $lic
        try {
            $callArgs = $script:commonArgs.Clone(); $callArgs.LicenseConfigPath = $lic
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'Staging license'
            $out | Should -Match 'Warewolf License\.secureconfig staged'
        } finally { Remove-Item -LiteralPath $lic -Force -ErrorAction SilentlyContinue }
    }

    It 'warns when no license is supplied' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'No license supplied'
    }

    It 'plan shows source encryption OFF + verify n/a by default' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Encrypt sources \(this run\)\s*: no'
        $out | Should -Match 'Verify decryption\s*: n/a'
    }

    It 'plan shows -VerifyDecryption only when encrypting' {
        $callArgs = $script:commonArgs.Clone()
        $callArgs.EncryptResources   = $true
        $callArgs.VerifyDecryption   = $true
        $callArgs.KeyVaultName       = 'kv-test'
        $callArgs.KeyVaultSecretName = 'dp-keyring-v1'
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Encrypt sources \(this run\)\s*: YES'
        $out | Should -Match 'Verify decryption\s*: yes \(in-memory\)'
    }

    Context 'publish source (folder / zip)' {
        It 'builds the dry-run preview from a folder publish path' {
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match 'building preview artifact'
        }
        It 'extracts a .zip into the dry-run preview artifact' {
            $zipSrc = Join-Path ([System.IO.Path]::GetTempPath()) ("wwzipsrc-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path $zipSrc -Force | Out-Null
            'x' | Set-Content -LiteralPath (Join-Path $zipSrc 'host.json')
            $zipPath = "$zipSrc.zip"
            Compress-Archive -Path (Join-Path $zipSrc '*') -DestinationPath $zipPath -Force
            try {
                $callArgs = $script:commonArgs.Clone(); $callArgs.PublishPath = $zipPath
                $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
                $out | Should -Match 'extracted from zip'          # pre-confirm summary line
                $out | Should -Match 'building preview artifact'    # dry-run extracts into the temp staging dir
            } finally {
                Remove-Item -LiteralPath $zipSrc -Recurse -Force -ErrorAction SilentlyContinue
                Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
                Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory -Filter 'wwexecutionengine-stage-*' -ErrorAction SilentlyContinue |
                    ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
            }
        }
        # NOTE: the staging-directory uniquifier added to Deploy-WwExecutionEngine.ps1 (Phase 3.0)
        # is NOT covered here. An end-to-end test of it — two runs of one AppName, asserting two
        # distinct staging dirs — was written and then removed: it failed ~25% of the time for the
        # SAME unresolved reason as the other intermittent failures in this file (the engine's own
        # post-copy guard, "Staging directory '...' does not exist after resolution", fires on a
        # directory it has just created and copied into). Adding a flaky test would have made that
        # noise worse, not caught a regression. The property was verified directly instead: three
        # sequential runs of one app, two inside the same second, produce three distinct
        # directories. Re-add the test once the underlying intermittency is understood.

        It 'throws on a PublishPath that is neither a folder nor a .zip' {
            $txt = Join-Path ([System.IO.Path]::GetTempPath()) ("wwbad-" + [guid]::NewGuid() + '.txt')
            'x' | Set-Content -LiteralPath $txt
            try {
                $callArgs = $script:commonArgs.Clone(); $callArgs.PublishPath = $txt
                { & $script:DeployScript @callArgs } | Should -Throw '*folder or a .zip*'
            } finally { Remove-Item -LiteralPath $txt -Force -ErrorAction SilentlyContinue }
        }
    }

    Context 'JobProcessor companion — separate publish directory' {
        BeforeEach {
            # A DISTINCT publish output for the processor (different Function App / csproj).
            $global:jpPubDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwjppub-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path $global:jpPubDir -Force | Out-Null
            '{ "version": "2.0" }' | Set-Content -LiteralPath (Join-Path $global:jpPubDir 'host.json')
        }
        AfterEach {
            if (Test-Path -LiteralPath $global:jpPubDir) { Remove-Item -LiteralPath $global:jpPubDir -Recurse -Force -ErrorAction SilentlyContinue }
            Remove-Variable -Name jpPubDir -Scope Global -ErrorAction SilentlyContinue
        }

        It 'throws at plan time when JobProcessorPublishPath equals the engine PublishPath' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.DeployJobProcessor      = $true
            $callArgs.JobProcessorPublishPath = $global:pubDir   # SAME dir as the engine -> illegal
            { & $script:DeployScript @callArgs } | Should -Throw '*SAME directory*'
        }

        It 'throws when -DeployJobProcessor is set but JobProcessorPublishPath is omitted (NonInteractive)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.DeployJobProcessor = $true              # no JobProcessorPublishPath supplied
            { & $script:DeployScript @callArgs } | Should -Throw '*JobProcessorPublishPath*'
        }

        It 'accepts a distinct JobProcessorPublishPath and invokes the companion child (dry-run)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.DeployJobProcessor      = $true
            $callArgs.JobProcessorPublishPath = $global:jpPubDir
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'JobProcessor PublishPath'
            $out | Should -Match 'separate from engine PublishDir'
            $out | Should -Match 'Phase 6  Deploy ExecutionEngineJobProcessor'
            $out | Should -Match "\[DRYRUN\] & 'Deploy-WwJobProcessor.ps1'"
        }
    }

    Context 'Elasticsearch staging' {
        BeforeEach {
            $global:esDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwes-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path $global:esDir -Force | Out-Null
            $global:esGood = Join-Path $global:esDir 'ElasticsearchLoggingSource.bite'
            '<Source ConnectionString="HostName=http://x;Port=9200" />' | Set-Content -LiteralPath $global:esGood -Encoding UTF8
            $global:esWrong = Join-Path $global:esDir 'WrongName.bite'
            '<Source ConnectionString="x" />' | Set-Content -LiteralPath $global:esWrong -Encoding UTF8
        }
        AfterEach {
            if (Test-Path -LiteralPath $global:esDir) { Remove-Item -LiteralPath $global:esDir -Recurse -Force }
            Remove-Variable -Name esDir, esGood, esWrong -Scope Global -ErrorAction SilentlyContinue
        }

        It 'defaults App Insights + console ON and Elasticsearch OFF when toggles omitted' {
            # WOLF-8516: the 5 ENABLE*/STRUCTURED_LOGS toggles were merged into one
            # WAREWOLF_LOGGING_CONFIG JSON app setting.
            $callArgs = $script:commonArgs.Clone()
            $callArgs.Remove('EnableAppInsights')      # let it default (-> ON)
            $callArgs.Remove('EnableElasticsearch')    # let it default (-> OFF; no ES source / KV needed)
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'WAREWOLF_LOGGING_CONFIG\s*=.*"console":true'
            $out | Should -Match 'WAREWOLF_LOGGING_CONFIG\s*=.*"appInsights":true'
            $out | Should -Match 'WAREWOLF_LOGGING_CONFIG\s*=.*"elasticsearch":false'
        }

        It 'stages the ES source AS-IS when encryption is off (default)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.EnableElasticsearch     = $true
            $callArgs.ElasticsearchSourcePath = $global:esGood
            $callArgs.KeyVaultName            = 'kv-test'
            $callArgs.KeyVaultSecretName      = 'dp-keyring-v1'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'WAREWOLF_LOGGING_CONFIG\s*=.*"elasticsearch":true'
            $out | Should -Match 'ElasticsearchLoggingSource.bite'
            $out | Should -Match 'Elasticsearch source staged AS-IS'
        }

        It 'enables Elasticsearch with NO Key Vault when not encrypting (no throw)' {
            # WOLF-8516: Key Vault topology now goes into Settings/executionengine.settings.json,
            # not an AZURE_KEYVAULT_NAME app setting — absence is asserted via the staging
            # step's log line rather than the (now permanently absent) env-var name.
            $callArgs = $script:commonArgs.Clone()
            $callArgs.EnableElasticsearch     = $true
            $callArgs.ElasticsearchSourcePath = $global:esGood   # no KeyVaultName, encryption off
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'Elasticsearch source staged AS-IS'
            $out | Should -Not -Match "Staging 'executionengine.settings.json'"   # no KV wiring without a vault
        }

        It 'wires Key Vault for runtime decrypt when -KeyVaultName is supplied without encryption' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.KeyVaultName       = 'kv-test'
            $callArgs.KeyVaultSecretName = 'dp-keyring-v1'        # encryption off (default)
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            # WOLF-8516: staged into executionengine.settings.json instead of an
            # AZURE_KEYVAULT_NAME app setting.
            $out | Should -Match "Staging 'executionengine.settings.json'.*keyVaultName=kv-test.*keyVaultSecretName=dp-keyring-v1"
            $out | Should -Match 'Key Vault Secrets User'         # MI gets read access
            $out | Should -Not -Match 'Secrets Officer'           # dev role only when encrypting
        }

        It 'rejects an ES source file with the wrong name' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.EnableElasticsearch     = $true
            $callArgs.ElasticsearchSourcePath = $global:esWrong
            { & $script:DeployScript @callArgs } | Should -Throw '*named exactly*'
        }

        It 'requires Key Vault when -EncryptResources is on (KeyVaultName omitted -> throws)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.EncryptResources        = $true
            $callArgs.EnableElasticsearch     = $true
            $callArgs.ElasticsearchSourcePath = $global:esGood
            { & $script:DeployScript @callArgs } | Should -Throw '*Required value*KeyVaultName*'
        }
    }

    Context 'ServiceBusTrigger tunables (executionengine.settings.json, WOLF-8516)' {
        It 'stages serviceBusTrigger independent of Key Vault when a tunable is supplied (no -KeyVaultName)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerMaxConcurrentExecutions = 2
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "Staging 'executionengine.settings.json'.*keyVaultName=,\s*keyVaultSecretName=,\s*serviceBusTrigger=.*maxConcurrentExecutions.:2"
        }

        It 'leaves unsupplied ServiceBusTrigger fields null alongside a supplied one' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerMaxConcurrentExecutions = 2
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'serviceBusTrigger=.*"jtiWindowHours":null'
            $out | Should -Match 'serviceBusTrigger=.*"executionTimeoutSeconds":null'
        }

        It 'regression: -KeyVaultName alone (no ServiceBusTrigger params) stages with NO serviceBusTrigger key' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.KeyVaultName       = 'kv-test'
            $callArgs.KeyVaultSecretName = 'dp-keyring-v1'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "Staging 'executionengine.settings.json'.*keyVaultName=kv-test.*keyVaultSecretName=dp-keyring-v1"
            $out | Should -Not -Match 'serviceBusTrigger='
        }

        It 'stages BOTH Key Vault topology and ServiceBusTrigger tunables when both are supplied' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.KeyVaultName       = 'kv-test'
            $callArgs.KeyVaultSecretName = 'dp-keyring-v1'
            $callArgs.ServiceBusTriggerMaxConcurrentExecutions = 2
            $callArgs.ServiceBusTriggerSettlementTimeoutSeconds = 30
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "Staging 'executionengine.settings.json'.*keyVaultName=kv-test.*keyVaultSecretName=dp-keyring-v1.*serviceBusTrigger="
            $out | Should -Match 'serviceBusTrigger=.*"maxConcurrentExecutions":2'
            $out | Should -Match 'serviceBusTrigger=.*"settlementTimeoutSeconds":30'
        }
    }
    Context 'breaking change — required params have no defaults' {
        It 'throws under -NonInteractive when <Param> is omitted' -ForEach @(
            @{ Param = 'ResourceGroup' }
            @{ Param = 'Location' }
            @{ Param = 'StorageAccount' }
            @{ Param = 'AppName' }
            @{ Param = 'PublishPath' }
        ) {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.Remove($Param)
            { & $script:DeployScript @callArgs } | Should -Throw '*Required value*'
        }

        It 'throws when encrypting but KeyVaultSecretName is omitted' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.EncryptResources = $true
            $callArgs.KeyVaultName = 'kv-test'   # supply name so it reaches the secret check
            { & $script:DeployScript @callArgs } | Should -Throw '*Required value*KeyVaultSecretName*'
        }

        It 'does NOT require KeyVaultSecretName when encryption is off' {
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match 'Dry-run complete'
        }
    }

    Context 'workflow index generation (step 3.6)' {
        BeforeEach {
            # A workflow source with one .bite so Resources is staged + indexed.
            $global:wfSrc = Join-Path ([System.IO.Path]::GetTempPath()) ("wwwf-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path (Join-Path $global:wfSrc 'tools') -Force | Out-Null
            '<Service Name="Hello World" />' | Set-Content -LiteralPath (Join-Path $global:wfSrc 'tools/Hello World.bite') -Encoding UTF8
        }
        AfterEach {
            if (Test-Path -LiteralPath $global:wfSrc) { Remove-Item -LiteralPath $global:wfSrc -Recurse -Force }
            Remove-Variable -Name wfSrc -Scope Global -ErrorAction SilentlyContinue
        }

        It 'generates workflow-index.json inside the staged Resources (bundled into the zip)' {
            $callArgs = $script:commonArgs.Clone(); $callArgs.WorkflowsSourcePath = $global:wfSrc
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'Generating workflow index'
            $out | Should -Match 'workflow-index\.json generated \(1 entry\)'

            # The file must exist in the temp staging dir's Resources folder. $script:appName is
            # unique to THIS test, so exactly one staging dir can match — no 'newest by
            # LastWriteTime' heuristic, which could select a directory left by another test.
            # (Parsing the path out of $out is not an option: Out-String wraps at the console
            # width, so a long temp path is split across lines.)
            $previewDir = Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory `
                -Filter "wwexecutionengine-stage-$($script:appName)-*" -ErrorAction SilentlyContinue
            $previewDir | Should -Not -BeNullOrEmpty
            @($previewDir).Count | Should -Be 1 -Because 'a unique AppName must yield exactly one staging dir'
            $indexPath = Join-Path @($previewDir)[0].FullName 'Resources/workflow-index.json'
            Test-Path -LiteralPath $indexPath | Should -BeTrue
            $idx = Get-Content -LiteralPath $indexPath -Raw | ConvertFrom-Json
            $idx.'tools/hello world' | Should -Be 'tools/Hello World.bite'
        }

        It 'skips index generation gracefully when no workflow source is supplied' {
            # Baseline commonArgs has no WorkflowsSourcePath -> no Resources staged.
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match 'No staged Resources folder — workflow index generation skipped'
            $out | Should -Match 'Dry-run complete'
        }
    }

    Context 'crash-safe summary (incremental + on failure)' {
        BeforeAll {
            # Return the single *.summary.json the run wrote to its (per-test) LogDir.
            function Get-RunSummary {
                $f = Get-ChildItem -Path $global:logDir -Filter '*summary.json' -ErrorAction SilentlyContinue | Select-Object -Last 1
                if (-not $f) { return $null }
                Get-Content -LiteralPath $f.FullName -Raw | ConvertFrom-Json
            }
        }

        It 'writes a completed summary on the happy (dry-run) path' {
            (& $script:DeployScript @commonArgs) 6>&1 | Out-Null
            $s = Get-RunSummary
            $s | Should -Not -BeNullOrEmpty
            $s.status    | Should -Be 'completed'
            $s.lastPhase | Should -Match 'Phase 5'
        }

        It 'records creation INTENT in the created-map before anything is created (resources missing)' {
            $global:resourcesExist = $false           # group/storage/app all "absent"
            (& $script:DeployScript @commonArgs) 6>&1 | Out-Null
            $s = Get-RunSummary
            $s.created.resourceGroup  | Should -BeTrue
            $s.created.storageAccount | Should -BeTrue
            $s.created.functionApp    | Should -BeTrue
        }

        It 'marks pre-existing resources as NOT created (created=false)' {
            $global:resourcesExist = $true            # everything already there
            (& $script:DeployScript @commonArgs) 6>&1 | Out-Null
            $s = Get-RunSummary
            $s.created.resourceGroup  | Should -BeFalse
            $s.created.storageAccount | Should -BeFalse
            $s.created.functionApp    | Should -BeFalse
        }

        It 'still writes a FAILED summary (with the created-map) when a mid-run step throws' {
            # A real (non-dry-run) run that fails while creating the Function App in
            # Phase 1 — the exact class of failure that previously left NO summary.
            function az {
                $a = $args; $global:LASTEXITCODE = 0
                if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
                if ($a[0] -eq 'group'       -and $a -contains 'exists') { return 'false' }
                if ($a[0] -eq 'storage'     -and $a -contains 'show')   { $global:LASTEXITCODE = 1; return $null }
                if ($a[0] -eq 'functionapp' -and $a -contains 'show')   { $global:LASTEXITCODE = 1; return $null }
                if ($a[0] -eq 'functionapp' -and $a -contains 'create') { $global:LASTEXITCODE = 1; return 'boom: functionapp create failed' }
                return '{}'
            }
            $callArgs = $script:commonArgs.Clone(); $callArgs.DryRun = $false
            { & $script:DeployScript @callArgs } | Should -Throw '*az CLI failed*'

            $s = Get-RunSummary
            $s | Should -Not -BeNullOrEmpty
            $s.status              | Should -Be 'failed'
            $s.lastPhase           | Should -Match 'Phase 1'
            $s.error               | Should -Match 'az CLI failed'
            # Intent was recorded for everything attempted BEFORE the failing create,
            # including the Function App itself — so rollback can clean a partial run.
            $s.created.resourceGroup  | Should -BeTrue
            $s.created.storageAccount | Should -BeTrue
            $s.created.functionApp    | Should -BeTrue
        }
    }
}

Describe 'Deploy-WwExecutionEngine — RabbitMQ queue-trigger companion (parameter surface)' {

    # The -DeployRabbitMqTriggers fan-out is exercised end-to-end in
    # Deploy-WwQueueProcessor.Tests.ps1 (the child does the work). What must be pinned HERE is
    # the engine orchestrator's parameter contract and its ValidateSet, so a rename or a
    # dropped default is caught before it reaches an operator's command line.

    BeforeAll {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:DeployScript, [ref]$null, [ref]$null)
        $script:QpParams = $ast.ParamBlock.Parameters
    }

    It 'declares the queue-trigger companion parameters' {
        $names = $script:QpParams | ForEach-Object { $_.Name.VariablePath.UserPath }

        foreach ($expected in @('DeployRabbitMqTriggers', 'QueueTriggerPath', 'QueueTriggerFilter',
                                'QueueTriggerFilePath', 'QueueTriggerManifestPath', 'QueueSourcePath',
                                'AcaEnvironment', 'AcrName', 'QueueProcessorPublishPath',
                                'QueueProcessorImage', 'QueueEngineResourceAppId', 'RabbitMqSecretUri',
                                'QueueScalingMode', 'ContinueOnQueueTriggerError')) {
            $names | Should -Contain $expected
        }
    }

    It 'defaults -QueueTriggerFilter to every .bite in the trigger folder' {
        # Triggers live in their own folder (Settings\triggers\), so the glob matches ANY .bite
        # rather than a 'triggers*' filename prefix - the prefix was a leftover from the earlier
        # flat layout and would have matched nothing in a per-trigger folder.
        $p = $script:QpParams | Where-Object { $_.Name.VariablePath.UserPath -eq 'QueueTriggerFilter' }
        $p.DefaultValue.Extent.Text | Should -Match '\*\.bite'
        $p.DefaultValue.Extent.Text | Should -Not -Match 'triggers\*'
    }

    It 'passes the resolved TenantId through to the queue companion' {
        # A blank tenant is legal ONLY for a system-assigned MI; for any other credential the
        # chain fails with 'Invalid tenant id provided', which reads like a missing app role.
        $text = Get-Content -LiteralPath $script:DeployScript -Raw
        $text | Should -Match 'EngineTenantId\s*=\s*\$TenantId'
    }

    It 'defaults -QueueScalingMode to Elastic (the standard; Fixed/Warm are exceptions)' {
        $p = $script:QpParams | Where-Object { $_.Name.VariablePath.UserPath -eq 'QueueScalingMode' }
        $p.DefaultValue.Extent.Text | Should -Match 'Elastic'
    }

    It 'constrains -QueueScalingMode with a ValidateSet' {
        { & $script:DeployScript -QueueScalingMode 'Turbo' -LoadFunctionsOnly } | Should -Throw
    }

    It 'exposes -DeployRabbitMqTriggers and -ContinueOnQueueTriggerError as switches' {
        foreach ($switch in @('DeployRabbitMqTriggers', 'ContinueOnQueueTriggerError')) {
            $p = $script:QpParams | Where-Object { $_.Name.VariablePath.UserPath -eq $switch }
            $p.StaticType.Name | Should -Be 'SwitchParameter'
        }
    }

    It 'resolves the child script path beside itself' {
        $text = Get-Content $script:DeployScript -Raw
        $text | Should -Match "QueueProcessorScript\s*=\s*Join-Path\s+\`$ScriptDir\s+'Deploy-WwQueueProcessor\.ps1'"
    }

    It 'guards the queue publish path against the engine AND JobProcessor publish paths' {
        $text = Get-Content $script:DeployScript -Raw
        $text | Should -Match 'QueueProcessorPublishPath resolves to the SAME directory as the engine PublishPath'
        $text | Should -Match 'QueueProcessorPublishPath resolves to the SAME directory as'
        $text | Should -Match 'JobProcessorPublishPath'
    }

    It 'fails at plan time on zero matching triggers and on an unsubstituted release token' {
        $text = Get-Content $script:DeployScript -Raw
        $text | Should -Match 'Refusing to run a queue-trigger deploy that would deploy nothing'
        $text | Should -Match 'unsubstituted release token'
    }

    It 'reports the companion state in the plan summary' {
        $text = Get-Content $script:DeployScript -Raw
        $text | Should -Match "Deploy RabbitMQ triggers"
    }

    It 'invokes the child once per trigger and reminds about the per-app role assignment' {
        $text = Get-Content $script:DeployScript -Raw
        $text | Should -Match 'foreach \(\$qpTriggerFile in \$script:QueueTriggerFiles\)'
        $text | Should -Match 'Warewolf_QueueProcessor'
    }
}
