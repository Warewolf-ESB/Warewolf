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

    It 'reaches the Graph /me endpoint from exactly one place (build 30696 guard)' {
        # Any NEW `az ad signed-in-user show` outside Resolve-CurrentPrincipal would
        # reintroduce the service-principal failure that broke the LOADTEST deploy.
        $src = Get-Content -LiteralPath $script:DeployScript -Raw
        ([regex]::Matches($src, "'signed-in-user', 'show'")).Count | Should -Be 1
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

    It 'throws a fully-substituted error when the role assignment never replicates (build 30715 regression)' {
        # REGRESSION (build 30715). `-f` binds tighter than `+` in PowerShell, so
        # ("A" + "B" -f args) only formats "B" (which has no {n} tokens, so args
        # were silently dropped) and concatenates "A" unformatted - the thrown
        # message showed literal '{0}' .. '{4}' instead of the role/scope/attempts.
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        function az { $global:LASTEXITCODE = 1; '' }
        $err = { Grant-RoleAssignment -PrincipalId 'pid-1' -Role 'Key Vault Secrets User' `
                                       -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                                       -Label 'the test identity' -MaxAttempts 1 -DelaySeconds 0 } |
            Should -Throw -PassThru
        $msg = $err.Exception.Message
        $msg | Should -Match ([regex]::Escape('Key Vault Secrets User'))
        $msg | Should -Match ([regex]::Escape('kv1'))
        $msg | Should -Match '1 attempts'
        $msg | Should -Not -Match '\{[0-4]\}'
    }

    It 'includes a ready-to-run az role assignment command in the failure message' {
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        function az { $global:LASTEXITCODE = 1; '' }
        $err = { Grant-RoleAssignment -PrincipalId 'pid-1' -Role 'Key Vault Secrets User' `
                                       -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                                       -PrincipalType ServicePrincipal -MaxAttempts 1 -DelaySeconds 0 } |
            Should -Throw -PassThru
        $msg = $err.Exception.Message
        $msg | Should -Match ([regex]::Escape('az role assignment create'))
        $msg | Should -Match ([regex]::Escape('--assignee-object-id pid-1'))
        $msg | Should -Match ([regex]::Escape('--assignee-principal-type ServicePrincipal'))
        $msg | Should -Match ([regex]::Escape("--role 'Key Vault Secrets User'"))
        $msg | Should -Match ([regex]::Escape("--scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1'"))
        $msg | Should -Not -Match '\{[0-9]\}'
    }

    It 'surfaces the real az CLI error instead of always assuming Entra replication lag (build 30726/30727 regression)' {
        # REGRESSION (build 30726/30727). The retry loop used to log a hardcoded
        # "Entra replication lag" guess on every failed attempt and never looked at
        # what az actually said, so a persistent non-replication failure (e.g. a
        # Graph-layer permission error) was misdiagnosed as transient lag build after
        # build. Grant-RoleAssignment must surface the real az output instead.
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        function az { $global:LASTEXITCODE = 1; 'ERROR: AADSTS500011: insufficient privileges to complete the operation.' }
        $err = { Grant-RoleAssignment -PrincipalId 'pid-1' -Role 'Key Vault Secrets User' `
                                       -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                                       -PrincipalType ServicePrincipal -MaxAttempts 1 -DelaySeconds 0 } |
            Should -Throw -PassThru
        $msg = $err.Exception.Message
        $msg | Should -Match ([regex]::Escape('AADSTS500011'))
    }

    It 'skips the grant when the principal already holds the role (build 30731 regression)' {
        # REGRESSION (build 30731). Re-deploying 'wwprofilerlw' died granting 'Key Vault Secrets
        # User' to a managed identity that ALREADY held it: `role assignment create` is a PUT
        # needing Microsoft.Authorization/roleAssignments/write, which the build service principal
        # does not hold. Probe first - reading assignments back needs only roleAssignments/read.
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        $global:azCalls = @()
        function az {
            $global:azCalls += , @($args)
            $global:LASTEXITCODE = 0
            if ($args[2] -eq 'list') { '[{"principalId":"pid-1","roleDefinitionName":"Key Vault Secrets User"}]' }
            else { '{}' }
        }
        { Grant-RoleAssignment -PrincipalId 'pid-1' -Role 'Key Vault Secrets User' `
                               -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                               -PrincipalType ServicePrincipal -MaxAttempts 10 -DelaySeconds 0 } |
            Should -Not -Throw
        @($global:azCalls | Where-Object { $_[2] -eq 'create' }).Count | Should -Be 0
        @($global:azCalls | Where-Object { $_[2] -eq 'list' }).Count   | Should -Be 1
        # The probe must filter principalId client-side: `--assignee` resolves through Graph and
        # returns another principal's rows when the object id has not replicated there.
        $probe = @($global:azCalls | Where-Object { $_[2] -eq 'list' })[0]
        $probe | Should -Not -Contain '--assignee'
        $probe | Should -Contain "[?principalId=='pid-1']"
        Remove-Variable -Name azCalls -Scope Global -ErrorAction SilentlyContinue
    }

    It 'still grants when the probe finds no existing assignment' {
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        $global:azCalls = @()
        function az {
            $global:azCalls += , @($args)
            $global:LASTEXITCODE = 0
            if ($args[2] -eq 'list') { '[]' } else { '{}' }
        }
        { Grant-RoleAssignment -PrincipalId 'pid-1' -Role 'Key Vault Secrets User' `
                               -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                               -PrincipalType ServicePrincipal -MaxAttempts 10 -DelaySeconds 0 } |
            Should -Not -Throw
        @($global:azCalls | Where-Object { $_[2] -eq 'create' }).Count | Should -Be 1
        Remove-Variable -Name azCalls -Scope Global -ErrorAction SilentlyContinue
    }

    It 'fails fast on AuthorizationFailed instead of retrying (build 30731 regression)' {
        # REGRESSION (build 30731). AuthorizationFailed on roleAssignments/write is deterministic,
        # not Entra replication lag: all 10 attempts returned it unchanged, wasting 150s and
        # burying the cause under nine identical "retrying" lines. Stop at the first one.
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        $global:azCalls = @()
        function az {
            $global:azCalls += , @($args)
            if ($args[2] -eq 'list') { $global:LASTEXITCODE = 0; '[]'; return }
            $global:LASTEXITCODE = 1
            "ERROR: (AuthorizationFailed) The client '***' with object id '326ac627' does not have " +
            "authorization to perform action 'Microsoft.Authorization/roleAssignments/write' over scope '/x'."
        }
        $err = { Grant-RoleAssignment -PrincipalId 'pid-1' -Role 'Key Vault Secrets User' `
                                      -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                                      -PrincipalType ServicePrincipal -MaxAttempts 10 -DelaySeconds 0 } |
            Should -Throw -PassThru
        @($global:azCalls | Where-Object { $_[2] -eq 'create' }).Count | Should -Be 1
        $msg = $err.Exception.Message
        $msg | Should -Match ([regex]::Escape('AuthorizationFailed'))
        $msg | Should -Match 'PERMISSION error'
        # The ready-to-run remediation command must survive the new advice branch.
        $msg | Should -Match ([regex]::Escape('az role assignment create'))
        $msg | Should -Match ([regex]::Escape('--assignee-object-id pid-1'))
        $msg | Should -Not -Match '\{[0-9]\}'
        Remove-Variable -Name azCalls -Scope Global -ErrorAction SilentlyContinue
    }

    It 'still retries a transient non-authorization failure' {
        # Guards the fail-fast matcher against being too broad - replication lag must keep its
        # full retry budget.
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        $global:azCalls = @()
        function az {
            $global:azCalls += , @($args)
            if ($args[2] -eq 'list') { $global:LASTEXITCODE = 0; '[]'; return }
            $global:LASTEXITCODE = 1
            "Cannot find user or service principal in graph database for 'pid-1'"
        }
        { Grant-RoleAssignment -PrincipalId 'pid-1' -Role 'Key Vault Secrets User' `
                               -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                               -PrincipalType ServicePrincipal -MaxAttempts 3 -DelaySeconds 0 } |
            Should -Throw
        @($global:azCalls | Where-Object { $_[2] -eq 'create' }).Count | Should -Be 3
        Remove-Variable -Name azCalls -Scope Global -ErrorAction SilentlyContinue
    }

    It 'surfaces the data-plane pre-grant as the remedy when the self-grant is denied (build 30735)' {
        # REGRESSION (build 30735). The deploying service principal held only Contributor, so it
        # could not READ the AES key and then could not self-grant either. The generic advice only
        # offers 'get RBAC-write', but the real remedy is a Key Vault DATA-PLANE pre-grant, which
        # needs no roleAssignments/write at all and makes the deploy skip this grant thereafter.
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        function az {
            if ($args[2] -eq 'list') { $global:LASTEXITCODE = 0; '[]'; return }
            $global:LASTEXITCODE = 1
            "ERROR: (AuthorizationFailed) does not have authorization to perform action " +
            "'Microsoft.Authorization/roleAssignments/write' over scope '/x'."
        }
        $err = { Grant-RoleAssignment -PrincipalId 'sp-1' -Role 'Key Vault Secrets User' `
                                      -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                                      -PrincipalType ServicePrincipal -MaxAttempts 1 -DelaySeconds 0 `
                                      -RemediationHint 'A Key Vault DATA-PLANE pre-grant avoids it entirely.' } |
            Should -Throw -PassThru
        $msg = $err.Exception.Message
        $msg | Should -Match ([regex]::Escape('DATA-PLANE pre-grant'))
        # The generic remedy and the ready-to-run command must survive alongside the hint.
        $msg | Should -Match 'PERMISSION error'
        $msg | Should -Match ([regex]::Escape('az role assignment create'))
        $msg | Should -Not -Match '\{[0-9]\}'
    }

    It 'omits the hint section when no -RemediationHint is passed' {
        # The managed-identity call site passes none; its message must not pick up
        # Key-Vault-specific advice.
        . $script:DeployScript -LoadFunctionsOnly
        $DryRun = $false
        function az {
            if ($args[2] -eq 'list') { $global:LASTEXITCODE = 0; '[]'; return }
            $global:LASTEXITCODE = 1
            'ERROR: (AuthorizationFailed) roleAssignments/write denied.'
        }
        $err = { Grant-RoleAssignment -PrincipalId 'sp-1' -Role 'Key Vault Secrets User' `
                                      -Scope '/subscriptions/sub/resourceGroups/DEV2/providers/Microsoft.KeyVault/vaults/kv1' `
                                      -PrincipalType ServicePrincipal -MaxAttempts 1 -DelaySeconds 0 } |
            Should -Throw -PassThru
        $err.Exception.Message | Should -Not -Match 'DATA-PLANE'
    }

    It 'retries the Key Vault role grant for at least 2 minutes before giving up (build 30715 regression)' {
        # The 60s budget (6 attempts x 10s) that failed in build 30715 proved too
        # short for Entra replication lag on a freshly-created managed identity.
        $ast  = [System.Management.Automation.Language.Parser]::ParseFile($script:DeployScript, [ref]$null, [ref]$null)
        $func = $ast.Find({ $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                             $args[0].Name -eq 'Grant-RoleAssignment' }, $true)
        $params       = $func.Body.ParamBlock.Parameters
        $maxAttempts  = ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'MaxAttempts' }).DefaultValue.Value
        $delaySeconds = ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'DelaySeconds' }).DefaultValue.Value
        $maxAttempts  | Should -Be 10
        $delaySeconds | Should -Be 15
        ($maxAttempts * $delaySeconds) | Should -BeGreaterOrEqual 120
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

    Context 'Get-WwPackageRuntimeIdentifier' {
        BeforeEach {
            $script:pkg = Join-Path ([System.IO.Path]::GetTempPath()) ("wwrid-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path $script:pkg -Force | Out-Null
            'x' | Set-Content -LiteralPath (Join-Path $script:pkg 'host.json')
        }
        AfterEach {
            Remove-Item -LiteralPath $script:pkg -Recurse -Force -ErrorAction SilentlyContinue
            Remove-Item -LiteralPath "$($script:pkg).zip" -Force -ErrorAction SilentlyContinue
        }
        BeforeAll {
            function script:Write-Deps([string]$Dir, [string]$Target) {
                @{ runtimeTarget = @{ name = $Target } } | ConvertTo-Json |
                    Set-Content -LiteralPath (Join-Path $Dir 'Warewolf.Execution.Lightweight.deps.json')
            }
        }

        It 'reads the RID from a package folder' {
            script:Write-Deps $script:pkg '.NETCoreApp,Version=v10.0/linux-x64'
            Get-WwPackageRuntimeIdentifier -Path $script:pkg | Should -Be 'linux-x64'
        }

        It 'reads the RID from a .zip whose contents sit at the root' {
            script:Write-Deps $script:pkg '.NETCoreApp,Version=v10.0/win-x64'
            Compress-Archive -Path (Join-Path $script:pkg '*') -DestinationPath "$($script:pkg).zip" -Force
            Get-WwPackageRuntimeIdentifier -Path "$($script:pkg).zip" | Should -Be 'win-x64'
        }

        It 'reads the RID from a .zip that wraps its contents in one folder' {
            script:Write-Deps $script:pkg '.NETCoreApp,Version=v10.0/linux-x64'
            Compress-Archive -Path $script:pkg -DestinationPath "$($script:pkg).zip" -Force
            Get-WwPackageRuntimeIdentifier -Path "$($script:pkg).zip" | Should -Be 'linux-x64'
        }

        It 'returns $null for a portable (RID-less) publish' {
            script:Write-Deps $script:pkg '.NETCoreApp,Version=v10.0'
            Get-WwPackageRuntimeIdentifier -Path $script:pkg | Should -BeNullOrEmpty
        }

        It 'returns $null when the package has no deps.json' {
            Get-WwPackageRuntimeIdentifier -Path $script:pkg | Should -BeNullOrEmpty
        }
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

    Context 'Resolve-CurrentPrincipal' {
        <#
            Build 30696 regression. `az ad signed-in-user show` is a Microsoft Graph /me
            call, valid only for a DELEGATED token; under the `az login --service-principal`
            that Dev/.azure/pipeline-LOADTEST.yml performs it fails outright with
            "/me request is only valid with delegated authentication flow", which killed
            Phase 3 the first time -EncryptResources reached this path.
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
            ($global:azSeen -join '|') | Should -Match 'signed-in-user'
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
        It 'captures the real az error text for a caller to inspect when -AllowFail is set' {
            $DryRun = $false; $script:azExit = 1
            $script:azOut = 'ERROR: AADSTS500011: insufficient privileges to complete the operation.'
            Invoke-Az -Args @('functionapp', 'show') -AllowFail | Should -BeNullOrEmpty
            $script:LastAzErrorOutput | Should -Match ([regex]::Escape('AADSTS500011'))
        }
        It 'throws on failure when -AllowFail is not set' {
            $DryRun = $false; $script:azExit = 1
            { Invoke-Az -Args @('functionapp', 'show') } | Should -Throw '*az CLI failed*'
        }
    }

    Context 'Invoke-Az transient retry (WwAzCli.psm1)' {
        <#
            Build 30801 failed Phase 1 on ONE ARM front-door 503. Invoke-Az now retries a transient
            failure before its throw / -AllowFail contract applies, and must still fail fast on a
            real error so an -AllowFail existence probe of a missing resource stays instant.
        #>
        BeforeAll {
            $script:outage503 = "ERROR: Service Unavailable(<!DOCTYPE html><html><body><h2>Our services aren't available right now</h2></body></html>)"
        }
        BeforeEach {
            $script:AzRetryDelaySeconds = @(0)
            # One scripted @{ Exit; Out } per call; the last step repeats.
            $script:azSteps = @(@{ Exit = 0; Out = '{}' }); $script:azCalls = 0
            function az {
                $step = $script:azSteps[[Math]::Min($script:azCalls, $script:azSteps.Count - 1)]
                $script:azCalls++
                $global:LASTEXITCODE = $step.Exit
                $step.Out
            }
        }
        AfterAll { $script:AzRetryDelaySeconds = @(3, 7, 15, 30) }

        It 'retries a read that hits the ARM outage page and returns its eventual output' {
            $DryRun = $false
            $script:azSteps = @(@{ Exit = 1; Out = $script:outage503 }, @{ Exit = 0; Out = '{"name":"app"}' })
            $out = (Invoke-Az -Args @('functionapp', 'show') 6>$null)
            $out | Should -Be '{"name":"app"}'
            $script:azCalls | Should -Be 2
        }

        It 'retries a mutating call too (the build-30801 failure was a write)' {
            $DryRun = $false
            $script:azSteps = @(@{ Exit = 1; Out = $script:outage503 }, @{ Exit = 0; Out = $null })
            { Invoke-Az -Args @('functionapp', 'config', 'appsettings', 'set') -Mutating 6>$null } | Should -Not -Throw
            $script:azCalls | Should -Be 2
        }

        It 'throws after 5 attempts on a persistent outage, naming the attempt count and the real cause' {
            $DryRun = $false
            $script:azSteps = @(@{ Exit = 1; Out = $script:outage503 })
            { Invoke-Az -Args @('functionapp', 'show') 6>$null } | Should -Throw "*after 5 attempt(s)*aren't available right now*"
            $script:azCalls | Should -Be 5
        }

        It 'does NOT retry an -AllowFail probe of a missing resource (still returns $null at once)' {
            $DryRun = $false
            $script:azSteps = @(@{ Exit = 3; Out = "ERROR: (ResourceNotFound) The Resource 'Microsoft.Web/sites/x' was not found." })
            Invoke-Az -Args @('functionapp', 'show') -AllowFail | Should -BeNullOrEmpty
            $script:azCalls | Should -Be 1
            $script:LastAzErrorOutput | Should -Match 'ResourceNotFound'
        }

        It 'returns $null from -AllowFail only after exhausting retries on a persistent outage' {
            $DryRun = $false
            $script:azSteps = @(@{ Exit = 1; Out = $script:outage503 })
            Invoke-Az -Args @('functionapp', 'show') -AllowFail 6>$null | Should -BeNullOrEmpty
            $script:azCalls | Should -Be 5
            $script:LastAzErrorOutput | Should -Match "aren't available right now"
        }

        It 'skips a mutating call under -DryRun before any retry is attempted' {
            $DryRun = $true
            $script:azSteps = @(@{ Exit = 1; Out = $script:outage503 })
            Invoke-Az -Args @('group', 'create') -Mutating 6>$null | Should -BeNullOrEmpty
            $script:azCalls | Should -Be 0
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

    Context 'dynamic-concurrency helper' {
        # Dynamic concurrency overrides the static cap exercised above, so these two helpers
        # are only meaningful together — see Update-HostJsonDynamicConcurrency's comment.
        It 'Update-HostJsonDynamicConcurrency sets dynamicConcurrencyEnabled false and preserves siblings' {
            $hj = Join-Path ([System.IO.Path]::GetTempPath()) ("hj-" + [guid]::NewGuid() + '.json')
            '{ "concurrency": { "dynamicConcurrencyEnabled": true, "snapshotPersistenceEnabled": true } }' |
                Set-Content -LiteralPath $hj -Encoding UTF8
            try {
                Update-HostJsonDynamicConcurrency -HostJsonPath $hj -Enabled $false | Should -BeTrue
                $j = Get-Content -LiteralPath $hj -Raw | ConvertFrom-Json
                $j.concurrency.dynamicConcurrencyEnabled | Should -BeFalse
                $j.concurrency.snapshotPersistenceEnabled | Should -BeTrue
            } finally { Remove-Item -LiteralPath $hj -Force -ErrorAction SilentlyContinue }
        }
        It 'Update-HostJsonDynamicConcurrency overwrites an already-set value (idempotent)' {
            $hj = Join-Path ([System.IO.Path]::GetTempPath()) ("hj-" + [guid]::NewGuid() + '.json')
            '{ "concurrency": { "dynamicConcurrencyEnabled": true } }' | Set-Content -LiteralPath $hj -Encoding UTF8
            try {
                Update-HostJsonDynamicConcurrency -HostJsonPath $hj -Enabled $false | Should -BeTrue
                (Get-Content -LiteralPath $hj -Raw | ConvertFrom-Json).concurrency.dynamicConcurrencyEnabled | Should -BeFalse
            } finally { Remove-Item -LiteralPath $hj -Force -ErrorAction SilentlyContinue }
        }
        It 'Update-HostJsonDynamicConcurrency CREATES the concurrency block when absent, preserving the rest' {
            # Deliberately unlike Update-HostJsonServiceBusConcurrency's no-op: an absent
            # concurrency block means "inherit the host default", and the point of the call is
            # to pin the value rather than inherit it.
            $hj = Join-Path ([System.IO.Path]::GetTempPath()) ("hj-" + [guid]::NewGuid() + '.json')
            '{ "version": "2.0", "functionTimeout": "00:10:00" }' | Set-Content -LiteralPath $hj -Encoding UTF8
            try {
                Update-HostJsonDynamicConcurrency -HostJsonPath $hj -Enabled $false | Should -BeTrue
                $j = Get-Content -LiteralPath $hj -Raw | ConvertFrom-Json
                $j.concurrency.dynamicConcurrencyEnabled | Should -BeFalse
                $j.functionTimeout | Should -Be '00:10:00'
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

    It 'does not touch the scale-out limit by default (opt-in only)' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'Setting Function App scale-out limit'
        $out | Should -Not -Match 'functionAppScaleLimit'
    }

    It 'applies -FunctionAppScaleLimit by writing siteConfig.functionAppScaleLimit' {
        # Guards the broker connection budget: one cached connection PER INSTANCE against a
        # vhost-wide cap, so instance count is the thing that has to be bounded.
        #
        # REGRESSION (2026-09-15): this asserted `--functions-scale-limit`, an argument
        # `az functionapp config set` DOES NOT HAVE. The test passed anyway, because the harness
        # stubs `az` and so only ever checked the string this script chose to emit — never that
        # the CLI accepts it. The real deploy failed with "unrecognized arguments". Assert the
        # generic resource write that classic Consumption actually needs (`az functionapp scale
        # config set --maximum-instance-count` is Flex-only), and keep the negative assertion so
        # the dead argument cannot come back.
        $callArgs = $script:commonArgs.Clone(); $callArgs.FunctionAppScaleLimit = 30
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Scale-out limit\s+: 30 instance\(s\)'
        $out | Should -Match 'Setting Function App scale-out limit -> 30'
        $out | Should -Match 'resource update'
        $out | Should -Match 'properties\.functionAppScaleLimit=30'
        $out | Should -Not -Match '--functions-scale-limit'
    }

    It 'does not claim the scale-out limit was verified during a dry run' {
        # The write is -Mutating, so -DryRun skips it; claiming "verified" there would be a lie,
        # and read-back against an unwritten value would fail the run outright.
        $callArgs = $script:commonArgs.Clone(); $callArgs.FunctionAppScaleLimit = 30
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'scale-out limit set to 30 \(verified\)'
        $out | Should -Match 'Dry-run: scale-out limit not applied, so not verified'
    }

    It 'records functionAppScaleLimit in the run summary' {
        $callArgs = $script:commonArgs.Clone(); $callArgs.FunctionAppScaleLimit = 30
        $null = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $summary = Get-ChildItem -Path $global:logDir -Filter '*.summary.json' |
                   Sort-Object LastWriteTime -Descending | Select-Object -First 1
        $summary | Should -Not -BeNullOrEmpty
        (Get-Content -LiteralPath $summary.FullName -Raw | ConvertFrom-Json).functionAppScaleLimit |
            Should -Be 30
    }

    It 'defaults to the classic Consumption plan and never emits Flex-only arguments' {
        # Every caller written before Flex support existed must keep getting a Y1 app.
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Hosting plan\s+: Consumption \(classic Y1\)'
        $out | Should -Not -Match '--flexconsumption-location'
        $out | Should -Not -Match '--instance-memory'
        $out | Should -Not -Match 'scale config set'
    }

    It 'states the Flex hosting decision in the Phase 0.5 plan' {
        $callArgs = $script:commonArgs.Clone()
        $callArgs.HostingPlan = 'Flex'
        $callArgs.FlexInstanceMemoryMB = 4096
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Hosting plan\s+: Flex Consumption \(4096 MB/instance'
    }

    It 'applies -FlexMaximumInstanceCount via the Flex-only scale config command' {
        # `az functionapp scale config set --maximum-instance-count` is Flex-ONLY; the classic
        # branch must keep using the generic resource writer instead (asserted above).
        $callArgs = $script:commonArgs.Clone()
        $callArgs.HostingPlan = 'Flex'
        $callArgs.FlexMaximumInstanceCount = 30
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Setting Flex scale ceiling -> 30'
        $out | Should -Match 'scale config set'
        $out | Should -Match '--maximum-instance-count 30'
        $out | Should -Not -Match 'properties\.functionAppScaleLimit'
        # Read-back must go through `scale config show`. REGRESSION (2026-09-21): it queried
        # `functionapp show --query functionAppConfig.scaleAndConcurrency.maximumInstanceCount`,
        # which returns EMPTY on az 2.x, so a correctly-applied ceiling threw and failed the
        # deploy mid-Phase-1 against a freshly created app.
        $out | Should -Not -Match 'functionAppConfig\.scaleAndConcurrency'
    }

    It 'does not claim the Flex scale ceiling was verified during a dry run' {
        $callArgs = $script:commonArgs.Clone()
        $callArgs.HostingPlan = 'Flex'
        $callArgs.FlexMaximumInstanceCount = 30
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'Flex scale ceiling set to 30 \(verified\)'
        $out | Should -Match 'Dry-run: Flex scale ceiling not applied, so not verified'
    }

    It 'pins a writable Linux data directory on Flex, and only on Flex' {
        # Flex is Linux, where the engine's data directory resolves to '/usr/share/Warewolf' —
        # a path that exists but is not writable in the container. Without this setting the
        # parameterless PublishRabbitMQActivity ctor throws and EVERY message dead-letters
        # (verified live on wwuat3 2026-09-21). Set by hand it would be silently reverted by the
        # next deploy, so it has to be part of the managed app settings.
        $callArgs = $script:commonArgs.Clone()
        $callArgs.HostingPlan = 'Flex'
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'webServerBasePath\s+= /tmp'

        # Classic Consumption is Windows and resolves a writable ProgramData path on its own;
        # forcing /tmp there would move the engine's data directory for no reason.
        $classic = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $classic | Should -Not -Match 'webServerBasePath'
    }

    It 'rejects -FunctionAppScaleLimit combined with -HostingPlan Flex' {
        # siteConfig.functionAppScaleLimit does not exist on Flex: the write would be silently
        # ignored, and an unapplied ceiling is indistinguishable from an applied one until the
        # broker connection budget blows under load.
        $callArgs = $script:commonArgs.Clone()
        $callArgs.HostingPlan = 'Flex'
        $callArgs.FunctionAppScaleLimit = 30
        { & $script:DeployScript @callArgs } | Should -Throw '*-FlexMaximumInstanceCount 30 instead*'
    }

    It 'rejects -FlexMaximumInstanceCount without -HostingPlan Flex' {
        $callArgs = $script:commonArgs.Clone()
        $callArgs.FlexMaximumInstanceCount = 30
        { & $script:DeployScript @callArgs } | Should -Throw '*requires -HostingPlan Flex*'
    }

    Context 'package runtime must match -HostingPlan' {
        # A mismatched package deploys with no error at all - the host just never starts the
        # worker - so the plan has to catch it. The runtime comes from the package's deps.json.
        BeforeAll {
            function script:Set-PackageRid([string]$Dir, [string]$Rid) {
                $target = if ($Rid) { ".NETCoreApp,Version=v10.0/$Rid" } else { '.NETCoreApp,Version=v10.0' }
                @{ runtimeTarget = @{ name = $target } } | ConvertTo-Json |
                    Set-Content -LiteralPath (Join-Path $Dir 'Warewolf.Execution.Lightweight.deps.json')
            }
        }

        It 'rejects a win-x64 package on Flex Consumption' {
            script:Set-PackageRid $global:pubDir 'win-x64'
            $callArgs = $script:commonArgs.Clone(); $callArgs.HostingPlan = 'Flex'
            { & $script:DeployScript @callArgs } | Should -Throw '*win-x64 package*Linux-only Flex*linux-x64*'
        }

        It 'rejects a linux-x64 package on classic (Windows) Consumption' {
            script:Set-PackageRid $global:pubDir 'linux-x64'
            { & $script:DeployScript @commonArgs } | Should -Throw '*linux-x64 package*Windows app*win-x64*'
        }

        It 'accepts a linux-x64 package on Flex, reading it from a .zip without extracting first' {
            script:Set-PackageRid $global:pubDir 'linux-x64'
            $zipPath = "$($global:pubDir).zip"
            Compress-Archive -Path (Join-Path $global:pubDir '*') -DestinationPath $zipPath -Force
            try {
                $callArgs = $script:commonArgs.Clone(); $callArgs.HostingPlan = 'Flex'; $callArgs.PublishPath = $zipPath
                $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
                $out | Should -Match 'Package runtime\s+: linux-x64 \(matches -HostingPlan Flex\)'
            } finally {
                Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
            }
        }

        It 'accepts a win-x64 package on classic Consumption' {
            script:Set-PackageRid $global:pubDir 'win-x64'
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match 'Package runtime\s+: win-x64 \(matches -HostingPlan Consumption\)'
        }

        It 'lets a portable (RID-less) package through on either plan, with a note' {
            script:Set-PackageRid $global:pubDir $null
            $callArgs = $script:commonArgs.Clone(); $callArgs.HostingPlan = 'Flex'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'states no runtime identifier'
            $out | Should -Match 'Package runtime\s+: portable'
        }

        It 'skips the check with -SkipPackageRuntimeCheck, and says so in the plan' {
            script:Set-PackageRid $global:pubDir 'win-x64'
            $callArgs = $script:commonArgs.Clone(); $callArgs.HostingPlan = 'Flex'; $callArgs.SkipPackageRuntimeCheck = $true
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'Package runtime\s+: check SKIPPED \(-SkipPackageRuntimeCheck\)'
        }
    }

    It 'rejects a non-positive -FunctionAppScaleLimit via ValidateRange' {
        # [nullable[int]] + ValidateRange, matching the other numeric params: a bare [int]
        # defaults to 0, which is indistinguishable from an explicit 0 and would silently
        # deploy "scale to zero instances".
        $callArgs = $script:commonArgs.Clone(); $callArgs.FunctionAppScaleLimit = 0
        { & $script:DeployScript @callArgs } | Should -Throw
    }

    It 'leaves dynamic concurrency alone by default (opt-in only)' {
        $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'Setting host.json concurrency.dynamicConcurrencyEnabled'
    }

    It 'pins dynamicConcurrencyEnabled to false with -DisableDynamicConcurrency' {
        $callArgs = $script:commonArgs.Clone(); $callArgs.DisableDynamicConcurrency = $true
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Setting host.json concurrency.dynamicConcurrencyEnabled -> false'
        $out | Should -Match 'static concurrency caps now apply'
    }

    It 'WARNS that -ServiceBusMaxConcurrentCalls is inert unless dynamic concurrency is disabled' {
        # The trap this closes: the cap is written to host.json and then ignored by the host,
        # so the deploy looks correct while running at the host's own chosen concurrency.
        '{ "extensions": { "serviceBus": { "autoCompleteMessages": false } } }' |
            Set-Content (Join-Path $global:pubDir 'host.json')
        $callArgs = $script:commonArgs.Clone(); $callArgs.ServiceBusMaxConcurrentCalls = 2
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'will NOT take effect'
        $out | Should -Match '-DisableDynamicConcurrency'
    }

    It 'does NOT warn when the cap is paired with -DisableDynamicConcurrency' {
        '{ "extensions": { "serviceBus": { "autoCompleteMessages": false } } }' |
            Set-Content (Join-Path $global:pubDir 'host.json')
        $callArgs = $script:commonArgs.Clone()
        $callArgs.ServiceBusMaxConcurrentCalls = 2
        $callArgs.DisableDynamicConcurrency    = $true
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'will NOT take effect'
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

    Context 'WAREWOLF_ENTRA_CONFIG drift repair under -SkipAuthProvisioning' {
        <#
            WAREWOLF_ENTRA_CONFIG is an ordinary Function App setting - a delete+recreate of the
            app (e.g. a Flex Consumption migration) wipes it even though the Entra app
            registration it names is untouched. Reproduced live on 'warewolfserver' (build 30802,
            2026-09-23): -SkipAuthProvisioning skips Phase 2 entirely, so nothing used to notice
            or repair the missing setting until a /secure/* call 401'd. Phase 2 now probes the
            live setting under -SkipAuthProvisioning and repairs it via Configure-WwExecutionAuth.ps1
            -RepairEntraConfigOnly when it's missing - see that script's own repair-mode tests for
            what the repair itself does and does not touch.
        #>

        It 'repairs WAREWOLF_ENTRA_CONFIG when it is missing on the live app (the baseline az shim returns none)' {
            $callArgs = $script:commonArgs.Clone(); $callArgs.SkipAuthProvisioning = $true
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'WAREWOLF_ENTRA_CONFIG is missing on the live app - repairing it now'
            $out | Should -Match "\[DRYRUN\] & 'Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'.*-RepairEntraConfigOnly"
        }

        It 'does NOT repair when WAREWOLF_ENTRA_CONFIG is already present on the live app' {
            function az {
                $a = $args; $global:LASTEXITCODE = 0
                if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
                if ($a[0] -eq 'group'       -and $a -contains 'exists') { return 'true' }
                if ($a[0] -eq 'storage'     -and $a -contains 'show')   { return '{"name":"st"}' }
                if ($a[0] -eq 'functionapp' -and $a -contains 'show')   { return '{"name":"app"}' }
                if ($a[0] -eq 'keyvault'    -and $a -contains 'secret' -and $a -contains 'show') { $global:LASTEXITCODE = 1; return $null }
                if ($a[0] -eq 'functionapp' -and $a -contains 'appsettings' -and $a -contains 'list') {
                    return '[{"name":"WAREWOLF_ENTRA_CONFIG","value":"{\"tenantId\":\"t\",\"audience\":\"api://a\",\"clientId\":\"a\"}"}]'
                }
                return '{}'
            }
            $callArgs = $script:commonArgs.Clone(); $callArgs.SkipAuthProvisioning = $true
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'WAREWOLF_ENTRA_CONFIG already present'
            # The Phase 0.5 plan line always mentions -RepairEntraConfigOnly as a possibility under
            # -SkipAuthProvisioning (see the other test) - what must NOT happen here is Phase 2
            # actually invoking it.
            $out | Should -Not -Match "repairing it now"
            $out | Should -Not -Match "\[DRYRUN\] & 'Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'"
        }

        It 'never probes for drift when auth provisioning is NOT skipped (unchanged full Phase 2)' {
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Not -Match 'RepairEntraConfigOnly'
            $out | Should -Not -Match 'checking for WAREWOLF_ENTRA_CONFIG drift'
        }

        It 'threads -EntraConfigRepairClientId/-AppDisplayName into the repair child-script call' {
            # build 30806, 2026-09-24: the repair's own `az ad app list` needs Microsoft Graph
            # permission the pipeline SP doesn't have, and warewolfserver's real Entra app isn't
            # named "<AppName>-auth" anyway - -EntraConfigRepairClientId skips that lookup.
            $callArgs = $script:commonArgs.Clone()
            $callArgs.SkipAuthProvisioning              = $true
            $callArgs.EntraConfigRepairClientId          = '05794411-b275-4801-97ac-8b078ed7196c'
            $callArgs.EntraConfigRepairAppDisplayName    = 'Warewolf Security'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "\[DRYRUN\] & 'Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'.*-KnownClientId"
            $out | Should -Match "\[DRYRUN\] & 'Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'.*-EntraAppDisplayName"
        }

        It 'omits -KnownClientId/-EntraAppDisplayName from the repair call when not supplied (unchanged default)' {
            $callArgs = $script:commonArgs.Clone(); $callArgs.SkipAuthProvisioning = $true
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "\[DRYRUN\] & 'Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'"
            $out | Should -Not -Match "\[DRYRUN\] & 'Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'.*-KnownClientId"
            $out | Should -Not -Match "\[DRYRUN\] & 'Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'.*-EntraAppDisplayName"
        }

        It 'never invokes the repair child-script with -SkipEntraConfigDriftRepair, even when WAREWOLF_ENTRA_CONFIG is missing' {
            # build 30815, 2026-09-24: wwprofilerlw has no Entra app registration at all
            # ("wwprofilerlw-auth" does not exist) and is intentionally open access, so the
            # repair's own `az ad app list` Graph lookup is a false positive that either fails on
            # privilege or finds nothing to repair either way. -SkipEntraConfigDriftRepair skips
            # the drift check (and its live-settings query) entirely.
            $callArgs = $script:commonArgs.Clone()
            $callArgs.SkipAuthProvisioning        = $true
            $callArgs.SkipEntraConfigDriftRepair  = $true
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'WAREWOLF_ENTRA_CONFIG drift check skipped \(-SkipEntraConfigDriftRepair\)'
            $out | Should -Not -Match 'repairing it now'
            $out | Should -Not -Match "Configure-WwExecutionAuth\.ps1 \(-RepairEntraConfigOnly\)'"
            $out | Should -Not -Match 'checking for WAREWOLF_ENTRA_CONFIG drift'
        }
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

    It 'asks for Secrets User, not Officer, when the key already exists (build 30735)' {
        # REGRESSION (build 30735). The dev-side grant always asked for 'Key Vault Secrets
        # Officer'. Officer's WRITE is only needed when this run GENERATES the key; encrypting
        # with an EXISTING key is a plain secret GET, covered by 'Key Vault Secrets User'.
        # Asking for Officer regardless printed an over-privileged remediation command.
        $callArgs = $script:commonArgs.Clone()
        $callArgs.EncryptResources   = $true
        $callArgs.KeyVaultName       = 'kv-test'
        $callArgs.KeyVaultSecretName = 'dp-keyring-v1'
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Assigning RBAC: Key Vault Secrets User -> .*\(for encryption\)'
        $out | Should -Not -Match 'Assigning RBAC: Key Vault Secrets Officer'
    }

    It 'asks for Secrets Officer when this run generates the key' {
        # Vault absent -> the script creates it and sets GenerateNewKey, so Encrypt-Config.ps1
        # will WRITE a key and Officer is genuinely required. Guards the least-privilege change
        # against swinging too far.
        function az {
            $a = $args; $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
            if ($a[0] -eq 'group'       -and $a -contains 'exists') { return 'true' }
            if ($a[0] -eq 'storage'     -and $a -contains 'show')   { return '{"name":"st"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'show')   { return '{"name":"app"}' }
            # The vault itself is ABSENT (both the 'show' probe and the secret read fail).
            if ($a[0] -eq 'keyvault'    -and $a -contains 'show')   { $global:LASTEXITCODE = 1; return $null }
            return '{}'
        }
        $callArgs = $script:commonArgs.Clone()
        $callArgs.EncryptResources   = $true
        $callArgs.KeyVaultName       = 'kv-test'
        $callArgs.KeyVaultSecretName = 'dp-keyring-v1'
        $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
        $out | Should -Match 'Assigning RBAC: Key Vault Secrets Officer -> .*\(for encryption\)'
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

        It 'still plans the dev-side grant when encrypting (probe-skip is a real-run-only shortcut)' {
            # The "key already readable -> skip the grant" shortcut is gated on
            # (-not $DryRun): a dry run never probes the vault, so the PLANNED grant must
            # still appear. Without this, the probe added for build 30696 could silently
            # swallow the grant for every run, not just entitled ones.
            # WHICH role is planned is deliberately not asserted here - that is the
            # least-privilege pair above ('asks for Secrets User/Officer ...'). This test
            # owns one invariant only: a grant is still planned under -DryRun.
            $callArgs = $script:commonArgs.Clone()
            $callArgs.EncryptResources   = $true
            $callArgs.KeyVaultName       = 'kv-test'
            $callArgs.KeyVaultSecretName = 'dp-keyring-v1'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'Assigning RBAC: Key Vault Secrets (User|Officer) ->'
            $out | Should -Match 'the signed-in user'
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
            $out | Should -Match 'serviceBusTrigger=.*"executionTimeoutSeconds":null'
            $out | Should -Match 'serviceBusTrigger=.*"slotWaitTimeoutSeconds":null'
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

    Context 'ServiceBusTrigger queue/workflow selection (servicebustrigger.bite, WOLF-8517 corrected)' {
        It 'stages servicebustrigger.bite with the supplied QueueName and WorkflowName' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName    = 'wwexecution-secure-trigger-queue-e2e'
            $callArgs.ServiceBusTriggerWorkflowName = 'profiler/ProfilerMain'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "Staging 'servicebustrigger.bite'.*QueueName=wwexecution-secure-trigger-queue-e2e.*WorkflowName=profiler/ProfilerMain"
        }

        It 'does NOT stage servicebustrigger.bite when neither param is supplied (checked-in dev/sample file ships unchanged)' {
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Not -Match "Staging 'servicebustrigger.bite'"
        }

        It 'throws when only -ServiceBusTriggerQueueName is supplied (both-or-neither)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName = 'wwexecution-secure-trigger-queue-e2e'
            { & $script:DeployScript @callArgs } | Should -Throw '*together*'
        }

        It 'throws when only -ServiceBusTriggerWorkflowName is supplied (both-or-neither)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerWorkflowName = 'profiler/ProfilerMain'
            { & $script:DeployScript @callArgs } | Should -Throw '*together*'
        }

        # -ServiceBusTriggerMapEntireMessageTo: the queue trigger's MapEntireMessage, carried to the
        # in-process trigger. Without it the profiler DEV ShovelBridge run failed every message with
        # "Scalar value { ProfileRequest } is NULL" (RequestForProfilingConsume's only input).
        It 'stages MapEntireMessageTo into servicebustrigger.bite when supplied' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName         = 'profiler.profile.request.DEV'
            $callArgs.ServiceBusTriggerWorkflowName      = 'ProfilerServices/ProfilerWrapper/Queue/RequestForProfilingConsume'
            $callArgs.ServiceBusTriggerMapEntireMessageTo = ' ProfileRequest '
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'MapEntireMessageTo=ProfileRequest'

            $staged = Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory -Filter "wwexecutionengine-stage-$($script:appName)-*" |
                ForEach-Object { Join-Path $_.FullName 'Settings/servicebustrigger.bite' } | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
            $staged | Should -Not -BeNullOrEmpty
            $json = Get-Content -LiteralPath $staged -Raw | ConvertFrom-Json
            $json.MapEntireMessageTo | Should -Be 'ProfileRequest'
            $json.QueueName          | Should -Be 'profiler.profile.request.DEV'
        }

        It 'does NOT write MapEntireMessageTo when the param is omitted (existing deploys stage the same file)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName    = 'wwexecution-secure-trigger-queue-e2e'
            $callArgs.ServiceBusTriggerWorkflowName = 'profiler/ProfilerMain'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Not -Match 'MapEntireMessageTo'

            $staged = Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory -Filter "wwexecutionengine-stage-$($script:appName)-*" |
                ForEach-Object { Join-Path $_.FullName 'Settings/servicebustrigger.bite' } | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
            $staged | Should -Not -BeNullOrEmpty
            @((Get-Content -LiteralPath $staged -Raw | ConvertFrom-Json).PSObject.Properties.Name) | Should -Be @('QueueName', 'WorkflowName')
        }

        It 'throws when -ServiceBusTriggerMapEntireMessageTo is supplied without the queue/workflow pair' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerMapEntireMessageTo = 'ProfileRequest'
            { & $script:DeployScript @callArgs } | Should -Throw '*requires -ServiceBusTriggerQueueName*'
        }

        It 'throws on an @-prefixed -ServiceBusTriggerMapEntireMessageTo (the engine would refuse it at cold start)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName         = 'q'
            $callArgs.ServiceBusTriggerWorkflowName      = 'wf'
            $callArgs.ServiceBusTriggerMapEntireMessageTo = '@RequestData'
            { & $script:DeployScript @callArgs } | Should -Throw "*'@'-prefixed*"
        }

        It 'stages servicebustrigger.bite independently of executionengine.settings.json (no Key Vault / ServiceBusTrigger tunables supplied)' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName    = 'wwexecution-secure-trigger-queue-e2e'
            $callArgs.ServiceBusTriggerWorkflowName = 'profiler/ProfilerMain'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "Staging 'servicebustrigger.bite'"
            $out | Should -Not -Match "Staging 'executionengine.settings.json'"
        }

        It 'stages BOTH servicebustrigger.bite and executionengine.settings.json when both concerns are supplied' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.KeyVaultName                  = 'kv-test'
            $callArgs.KeyVaultSecretName             = 'dp-keyring-v1'
            $callArgs.ServiceBusTriggerQueueName    = 'wwexecution-secure-trigger-queue-e2e'
            $callArgs.ServiceBusTriggerWorkflowName = 'profiler/ProfilerMain'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match "Staging 'servicebustrigger.bite'.*QueueName=wwexecution-secure-trigger-queue-e2e.*WorkflowName=profiler/ProfilerMain"
            $out | Should -Match "Staging 'executionengine.settings.json'.*keyVaultName=kv-test.*keyVaultSecretName=dp-keyring-v1"
        }

        It 'also applies QueueName as the WAREWOLF_SERVICEBUS_TRIGGER_QUEUE app setting (dotnet-isolated attribute-placeholder regression guard)' {
            <#
                On dotnet-isolated, [ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%")] is
                resolved by the out-of-process Functions Host at function-indexing time - before
                the isolated worker process (where ConfigureWarewolf's in-memory config injection
                runs) ever starts. Staging servicebustrigger.bite alone left the function unable to
                index ("does not resolve to a value"), permanently disabling the trigger even
                though the file was staged correctly. This pins that the queue name also becomes a
                real app setting so the host can resolve the placeholder.
            #>
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName    = 'wwexecution-secure-trigger-queue-e2e'
            $callArgs.ServiceBusTriggerWorkflowName = 'profiler/ProfilerMain'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE\s*=\s*wwexecution-secure-trigger-queue-e2e'
        }

        It 'does NOT set WAREWOLF_SERVICEBUS_TRIGGER_QUEUE when neither trigger param is supplied' {
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Not -Match 'WAREWOLF_SERVICEBUS_TRIGGER_QUEUE'
        }

        It 'disables the trigger in the pre-upload app settings when neither trigger param is supplied' {
            <#
                On Flex Consumption az config-zip health-checks the host straight after the upload,
                so a caller setting this switch in a later step is too late - the deploy itself
                fails with "Failed to fetch host key". It must be in the app settings this script
                applies before Phase 4.
            #>
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match 'AzureWebJobs\.ServiceBusWorkflowTrigger\.Disabled\s*=\s*true'
        }

        It 'explicitly enables the trigger when both trigger params are supplied' {
            $callArgs = $script:commonArgs.Clone()
            $callArgs.ServiceBusTriggerQueueName    = 'wwexecution-secure-trigger-queue-e2e'
            $callArgs.ServiceBusTriggerWorkflowName = 'profiler/ProfilerMain'
            $out = (& $script:DeployScript @callArgs) 6>&1 | Out-String
            $out | Should -Match 'AzureWebJobs\.ServiceBusWorkflowTrigger\.Disabled\s*=\s*false'
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

Describe 'Deploy-WwExecutionEngine — .NET runtime (create + reconcile)' {

    <#
        The Function App host does not adapt to its payload. `functionapp create` is the ONLY
        place --runtime-version applies, so an app created by an earlier run keeps the runtime
        it was born with — deploying a net10.0 package onto a v8.0 host produces a deploy that
        reports success and an engine that never starts ("You must install or update .NET to
        run this application."). These tests pin both halves: creation on 10, and what the run
        does about an app that is already on something else.
    #>

    Context 'Resolve-FunctionRuntimePlan (pure decision)' {

        BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

        It 'plans a create when the app does not exist yet' {
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion $null
            $p.Action | Should -Be 'create'
            $p.Target | Should -Be 'v10.0'
        }

        It 'plans nothing when the app is already on the target' {
            (Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion 'v10.0').Action | Should -Be 'none'
        }

        It 'WARNS (never changes) on a mismatch without -UpgradeRuntimeVersion' {
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion 'v8.0'
            $p.Action  | Should -Be 'warn'
            $p.Current | Should -Be 'v8.0'
            $p.Message | Should -Match 'pass -UpgradeRuntimeVersion'
        }

        It 'upgrades on a mismatch only when -UpgradeRuntimeVersion is passed' {
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion 'v8.0' -Upgrade
            $p.Action  | Should -Be 'upgrade'
            $p.Message | Should -Match 'RESTARTS'
        }

        It 'treats whitespace/casing from az as the same version (no spurious upgrade)' {
            # `az ... -o tsv`-shaped values and ARM casing must not look like a mismatch and
            # trigger a needless app restart.
            (Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion '  V10.0 ' -Upgrade).Action |
                Should -Be 'none'
        }

        It 'treats an empty netFrameworkVersion as "no app" when the app does NOT exist' {
            (Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion '   ').Action | Should -Be 'create'
        }

        It 'reports UNKNOWN — never a create — when the app exists but its runtime is unreadable' {
            # The distinction this pins is the whole point of the action. `functionapp create` is the
            # only call that pins a runtime and it does not run for an existing app, so treating an
            # unreadable runtime as a create means the run reconciles nothing and then says it did.
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion '   ' -AppExists
            $p.Action  | Should -Be 'unknown'
            $p.Current | Should -BeNullOrEmpty
            $p.Message | Should -Match 'COULD NOT BE READ'
        }

        It 'does not let -UpgradeRuntimeVersion turn an unreadable runtime into a blind config set' {
            # An upgrade RESTARTS the app. Doing that on a runtime nobody could read would be acting
            # on an inference, which is exactly what the warn-by-default posture exists to prevent.
            (Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion $null -AppExists -Upgrade).Action |
                Should -Be 'unknown'
        }
    }

    Context 'Resolve-FunctionRuntimePlan on Flex (pure decision)' {

        <#
            Flex keeps the runtime in functionAppConfig.runtime as '10.0' (no leading 'v'), and it is
            read and reconciled with `az functionapp runtime config show/set`. Build 30801 (the first
            deploy onto an EXISTING Flex app) reported it 'unknown' and pointed the operator at
            `functionapp config show`, which reads a meaningless 'v4.0' on Flex.
        #>

        BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

        It 'targets 10.0 and plans nothing when a Flex app is already on it' {
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion '10.0' -AppExists -HostingPlan Flex
            $p.Target | Should -Be '10.0'
            $p.Action | Should -Be 'none'
        }

        It 'WARNS on a Flex mismatch without -UpgradeRuntimeVersion, naming the Flex fix command' {
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion '8.0' -AppExists -HostingPlan Flex -AppName 'a' -ResourceGroup 'g'
            $p.Action     | Should -Be 'warn'
            $p.FixCommand | Should -Be 'az functionapp runtime config set -n a -g g --runtime-version 10.0'
        }

        It 'upgrades a Flex mismatch only with -UpgradeRuntimeVersion' {
            (Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion '8.0' -AppExists -Upgrade -HostingPlan Flex).Action |
                Should -Be 'upgrade'
        }

        It 'treats v10.0 and 10.0 as the same version (no spurious upgrade/restart)' {
            (Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion 'v10.0' -AppExists -Upgrade -HostingPlan Flex).Action |
                Should -Be 'none'
            (Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion '10.0' -AppExists -Upgrade).Action |
                Should -Be 'none'
        }

        It 'points an unreadable Flex runtime at `runtime config show`, never `config show`' {
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion $null -AppExists -HostingPlan Flex -AppName 'a' -ResourceGroup 'g'
            $p.Action       | Should -Be 'unknown'
            $p.CheckCommand | Should -Be 'az functionapp runtime config show -n a -g g'
            $p.Message      | Should -Match 'runtime config show'
        }

        It 'keeps the classic check/fix commands on Consumption' {
            $p = Resolve-FunctionRuntimePlan -CurrentNetFrameworkVersion 'v8.0' -AppExists -AppName 'a' -ResourceGroup 'g'
            $p.CheckCommand | Should -Be 'az functionapp config show -n a -g g --query netFrameworkVersion'
            $p.FixCommand   | Should -Be 'az functionapp config set -n a -g g --net-framework-version v10.0'
        }
    }

    Context 'Get-WwExistingRuntimeVersion (pure payload parsing)' {

        BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

        It 'reads siteConfig.netFrameworkVersion from a flattened classic payload' {
            $r = Get-WwExistingRuntimeVersion -AppShowJson '{"name":"app","siteConfig":{"netFrameworkVersion":"v8.0"}}'
            $r.Version | Should -Be 'v8.0'
            $r.Source  | Should -Be 'siteConfig'
        }

        It 'reads siteConfig nested under properties (ARM shape)' {
            $r = Get-WwExistingRuntimeVersion -AppShowJson '{"name":"app","properties":{"siteConfig":{"netFrameworkVersion":"v10.0"}}}'
            $r.Version | Should -Be 'v10.0'
            $r.Source  | Should -Be 'siteConfig'
        }

        It 'reads functionAppConfig.runtime from the Flex ARM payload, ignoring its null netFrameworkVersion' {
            # The live WarewolfServer-UAT shape (az 2.83, 2026-09-23).
            $json = '{"name":"WarewolfServer-UAT","kind":"functionapp,linux","properties":{"siteConfig":{"netFrameworkVersion":null},' +
                    '"functionAppConfig":{"runtime":{"name":"dotnet-isolated","version":"10.0"}}}}'
            $r = Get-WwExistingRuntimeVersion -AppShowJson $json
            $r.Version | Should -Be '10.0'
            $r.Source  | Should -Be 'functionAppConfig'
        }

        It 'returns $null when the payload carries neither' {
            Get-WwExistingRuntimeVersion -AppShowJson '{"name":"app"}' | Should -BeNullOrEmpty
            Get-WwExistingRuntimeVersion -AppShowJson '{"name":"app","properties":{"siteConfig":{"netFrameworkVersion":null}}}' |
                Should -BeNullOrEmpty
        }

        It 'returns $null for unreadable input rather than throwing' {
            Get-WwExistingRuntimeVersion -AppShowJson 'ERROR: not json {' | Should -BeNullOrEmpty
            Get-WwExistingRuntimeVersion -AppShowJson ''                  | Should -BeNullOrEmpty
            Get-WwExistingRuntimeVersion -AppShowJson '[1,2]'             | Should -BeNullOrEmpty
        }
    }

    Context 'end-to-end (DryRun)' {

        BeforeEach {
            # Shim distinguishes "app absent" from "app present on <netFx>". The Phase 0.5 probe
            # and the Phase 1 existence check both go through `functionapp show`.
            $global:appExists       = $true
            $global:appNetFx        = 'v8.0'
            $global:appFlexPayload  = $false
            $global:appFlexRuntime  = $null
            $global:flexRuntimeShow = $null
            function az {
                $a = $args; $global:LASTEXITCODE = 0
                if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
                if ($a[0] -eq 'group'       -and $a -contains 'exists') { return 'true' }
                if ($a[0] -eq 'storage'     -and $a -contains 'show')   { return '{"name":"st"}' }
                if ($a[0] -eq 'functionapp' -and $a[1] -eq 'runtime' -and $a -contains 'show') {
                    # Flex-only `runtime config show`; $global:flexRuntimeShow = $null means it fails.
                    if ($null -eq $global:flexRuntimeShow) { $global:LASTEXITCODE = 1; return 'ERROR: (ResourceNotFound) not a Flex app' }
                    return ('{"name":"dotnet-isolated","version":"' + $global:flexRuntimeShow + '"}')
                }
                if ($a[0] -eq 'functionapp' -and $a -contains 'show')   {
                    if (-not $global:appExists) { $global:LASTEXITCODE = 1; return $null }
                    if ($global:appFlexPayload) {
                        # The ARM shape az returns for a Flex app: nested under properties, null netFrameworkVersion.
                        $fac = if ($global:appFlexRuntime) { ',"functionAppConfig":{"runtime":{"name":"dotnet-isolated","version":"' + $global:appFlexRuntime + '"}}' } else { '' }
                        return ('{"name":"app","properties":{"siteConfig":{"netFrameworkVersion":null}' + $fac + '}}')
                    }
                    if ($null -eq $global:appNetFx) { return '{"name":"app"}' }   # payload with no siteConfig
                    return ('{"name":"app","siteConfig":{"netFrameworkVersion":"' + $global:appNetFx + '"}}')
                }
                if ($a[0] -eq 'keyvault' -and $a -contains 'secret' -and $a -contains 'show') { $global:LASTEXITCODE = 1; return $null }
                return '{}'
            }

            $global:pubDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwpub-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path $global:pubDir -Force | Out-Null
            '{ "logging": { "logLevel": { "default": "Information" } } }' | Set-Content (Join-Path $global:pubDir 'host.json')
            $global:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwlog-" + [guid]::NewGuid())
            $script:appName = 'wwrt-' + [guid]::NewGuid().ToString('N').Substring(0, 8)

            $script:commonArgs = @{
                SubscriptionId      = 'sub-123'
                TenantId            = 'tid-456'
                ResourceGroup       = 'DEV2'
                Location            = 'southafricanorth'
                StorageAccount      = 'stwwrt'
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
            Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory -Filter "wwexecutionengine-stage-$($script:appName)-*" -ErrorAction SilentlyContinue |
                ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
        }

        AfterAll {
            Remove-Variable -Name appExists -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name appNetFx  -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name appFlexPayload  -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name appFlexRuntime  -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name flexRuntimeShow -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name pubDir    -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name logDir    -Scope Global -ErrorAction SilentlyContinue
        }

        It 'creates a NEW app on dotnet-isolated 10, never 8' {
            $global:appExists = $false
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match '\[DRYRUN\] az functionapp create .*--runtime dotnet-isolated --runtime-version 10'
            $out | Should -Not -Match '--runtime-version 8'
            $out | Should -Match 'Consumption Y1, dotnet-isolated 10, Functions v4'
        }

        It 'states the runtime decision in the Phase 0.5 plan' {
            $global:appExists = $false
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match 'Runtime\s+: dotnet-isolated 10 \(Functions v4\)'
        }

        It 'creates a Flex Consumption app with --flexconsumption-location and --instance-memory' {
            # Flex pins instance size at create time and takes --flexconsumption-location in place
            # of --consumption-plan-location. It is also LINUX-ONLY, so --os-type Windows (which
            # the classic branch passes) must not appear or the create is rejected outright.
            $global:appExists = $false
            $out = (& $script:DeployScript @commonArgs -HostingPlan Flex -FlexInstanceMemoryMB 4096) 6>&1 | Out-String
            $out | Should -Match '\[DRYRUN\] az functionapp create .*--flexconsumption-location'
            $out | Should -Match '--instance-memory 4096'
            $out | Should -Match 'Flex Consumption, 4096 MB/instance'
            $out | Should -Not -Match '--consumption-plan-location'
            $out | Should -Not -Match '--os-type Windows'
        }

        It 'still creates a classic Y1 app with --consumption-plan-location by default' {
            # Regression guard: every pre-Flex caller must keep getting a Windows Y1 app.
            $global:appExists = $false
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match '\[DRYRUN\] az functionapp create .*--consumption-plan-location'
            $out | Should -Match '--os-type Windows'
            $out | Should -Not -Match '--flexconsumption-location'
            $out | Should -Not -Match '--instance-memory'
        }

        It 'upgrades an existing v8.0 app when -UpgradeRuntimeVersion is passed' {
            $global:appExists = $true; $global:appNetFx = 'v8.0'
            $out = (& $script:DeployScript @commonArgs -UpgradeRuntimeVersion) 6>&1 | Out-String
            $out | Should -Match '\[DRYRUN\] az functionapp config set .*--net-framework-version v10\.0'
            $out | Should -Match 'Upgrading Function App runtime v8\.0 -> v10\.0'
        }

        It 'does NOT touch an existing v8.0 app without the switch, but warns before the confirmation' {
            # The default must never restart a live app on inference alone — but silence here
            # would hand the operator a "successful" deploy whose engine cannot start.
            $global:appExists = $true; $global:appNetFx = 'v8.0'
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            # Anchor on the [DRYRUN] marker, not the bare command: the warning itself QUOTES the
            # remediation command (`az functionapp config set ...`), so a looser pattern passes
            # whether or not the call was actually made.
            $out | Should -Not -Match '\[DRYRUN\] az functionapp config set'
            $out | Should -Match 'RUNTIME MISMATCH'
            $out | Should -Match 'Re-run with -UpgradeRuntimeVersion'
        }

        It 'does nothing when the existing app is already v10.0 (no needless restart)' {
            $global:appExists = $true; $global:appNetFx = 'v10.0'
            $out = (& $script:DeployScript @commonArgs -UpgradeRuntimeVersion) 6>&1 | Out-String
            $out | Should -Not -Match '\[DRYRUN\] az functionapp config set'
            $out | Should -Not -Match 'RUNTIME MISMATCH'
            $out | Should -Match 'Function App runtime is v10\.0'
        }

        It 'flags a functionapp show payload with no siteConfig instead of claiming the app is on v10.0' {
            # This used to assert only that nothing blew up, which let the run print
            # "Function App runtime is v10.0" about an app whose runtime it never read - the exact
            # deploy-succeeds/engine-dies outcome the runtime check exists to catch.
            $global:appExists = $true; $global:appNetFx = $null
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String

            $out | Should -Match 'RUNTIME UNVERIFIED'
            $out | Should -Match 'netFrameworkVersion could not'
            # Never asserts a version it did not read, and never restarts on that inference.
            $out | Should -Not -Match 'Function App runtime is v10\.0'
            $out | Should -Not -Match '\[DRYRUN\] az functionapp config set'
            # A mismatch is a DIFFERENT finding: we did not read a version, so we cannot call it one.
            $out | Should -Not -Match 'RUNTIME MISMATCH'
            $out | Should -Match 'Dry-run complete'
        }

        It 'records the runtime decision in the summary for the operator' {
            $global:appExists = $true; $global:appNetFx = 'v8.0'
            (& $script:DeployScript @commonArgs) 6>&1 | Out-Null
            $summary = Get-ChildItem -Path $global:logDir -Filter '*.summary.json' | Select-Object -First 1
            $json = Get-Content $summary.FullName -Raw | ConvertFrom-Json
            $json.runtime.runtimeVersion     | Should -Be '10'
            $json.runtime.netFrameworkTarget | Should -Be 'v10.0'
            $json.runtime.netFrameworkBefore | Should -Be 'v8.0'
            $json.runtime.action             | Should -Be 'warn'
        }

        It 'reads an EXISTING Flex app''s runtime from functionAppConfig and plans nothing (build 30801)' {
            $global:appExists = $true; $global:appFlexPayload = $true; $global:appFlexRuntime = '10.0'
            $out = (& $script:DeployScript @commonArgs -HostingPlan Flex) 6>&1 | Out-String
            $out | Should -Not -Match 'RUNTIME UNVERIFIED'
            $out | Should -Match '10\.0 — already correct'
            $out | Should -Match 'Function App runtime is 10\.0'
            $out | Should -Not -Match '\[DRYRUN\] az functionapp (runtime )?config set'
        }

        It 'warns on an existing Flex app behind the target, naming the Flex fix command, and changes nothing' {
            $global:appExists = $true; $global:appFlexPayload = $true; $global:appFlexRuntime = '8.0'
            $out = (& $script:DeployScript @commonArgs -HostingPlan Flex) 6>&1 | Out-String
            $out | Should -Match 'RUNTIME MISMATCH'
            $out | Should -Match 'az functionapp runtime config set -n \S+ -g DEV2 --runtime-version 10\.0'
            $out | Should -Not -Match '\[DRYRUN\] az functionapp (runtime )?config set'
        }

        It 'upgrades an existing Flex app with `runtime config set`, never --net-framework-version' {
            $global:appExists = $true; $global:appFlexPayload = $true; $global:appFlexRuntime = '8.0'
            $out = (& $script:DeployScript @commonArgs -HostingPlan Flex -UpgradeRuntimeVersion) 6>&1 | Out-String
            $out | Should -Match '\[DRYRUN\] az functionapp runtime config set .*--runtime-version 10\.0'
            $out | Should -Not -Match '--net-framework-version'
            $out | Should -Match 'Upgrading Function App runtime 8\.0 -> 10\.0'
        }

        It 'falls back to `runtime config show` when the Flex payload has no functionAppConfig' {
            $global:appExists = $true; $global:appFlexPayload = $true; $global:appFlexRuntime = $null
            $global:flexRuntimeShow = '10.0'
            $out = (& $script:DeployScript @commonArgs -HostingPlan Flex) 6>&1 | Out-String
            $out | Should -Not -Match 'RUNTIME UNVERIFIED'
            $out | Should -Match 'Function App runtime is 10\.0'
        }

        It 'reports an unreadable Flex runtime as UNVERIFIED and points at `runtime config show`' {
            $global:appExists = $true; $global:appFlexPayload = $true; $global:appFlexRuntime = $null
            $global:flexRuntimeShow = $null
            $out = (& $script:DeployScript @commonArgs -HostingPlan Flex) 6>&1 | Out-String
            $out | Should -Match 'RUNTIME UNVERIFIED'
            $out | Should -Match 'Check it with: az functionapp runtime config show -n \S+ -g DEV2'
            # `config show` reads a meaningless 'v4.0' on Flex - it must not be the advice.
            $out | Should -Not -Match '--query netFrameworkVersion'
            $out | Should -Not -Match '\[DRYRUN\] az functionapp (runtime )?config set'
        }
    }
}

Describe 'Deploy-WwExecutionEngine — Function App storage wiring (create + reconcile)' {

    <#
        `--storage-account` is honoured ONLY by `functionapp create`. An app created by an
        earlier run therefore keeps whatever storage it was born with, and Phase 1's
        "app already exists" branch reconciled none of it — the same shape of hole the
        .NET runtime check above exists to close.

        BUILD 30739 (2026-09-18). `wwprofilerlw` had been created against storage account
        `stwwprofilerlw`, which a later cleanup sweep deleted. The deploy plan printed
        `StorageAccount : wwprofilerlwsa` and `[+] Storage account 'wwprofilerlwsa' already
        exists.` — both true, both about an account the app had never pointed at. Phase 4 then
        died inside Kudu with `500 ... System.IO.IOException: The network path was not found.`
        (the content share could not be mounted; on a Consumption Y1 plan that share is
        mandatory). These tests pin the decision and what Phase 1 does with it.
    #>

    Context 'Get-StorageAccountNameFromConnectionString' {

        BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

        It 'pulls AccountName out of a real AzureWebJobsStorage value' {
            Get-StorageAccountNameFromConnectionString 'DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=stwwprofilerlw;AccountKey=abc==' |
                Should -Be 'stwwprofilerlw'
        }

        It 'does not depend on key order or casing (az promises neither)' {
            Get-StorageAccountNameFromConnectionString 'accountname=wwprofilerlwsa;DefaultEndpointsProtocol=https' |
                Should -Be 'wwprofilerlwsa'
        }

        It 'returns $null — never a bogus name — for a value carrying no AccountName' {
            # "could not be determined" and "differs" are different findings; a SAS-based or
            # identity-based connection string must not be read as an account name.
            Get-StorageAccountNameFromConnectionString 'BlobEndpoint=https://x.blob.core.windows.net/;SharedAccessSignature=sv=2022' |
                Should -BeNullOrEmpty
            Get-StorageAccountNameFromConnectionString ''    | Should -BeNullOrEmpty
            Get-StorageAccountNameFromConnectionString $null | Should -BeNullOrEmpty
        }
    }

    Context 'Get-ContentShareName' {

        BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

        It 'is deterministic and share-name legal' {
            $n = Get-ContentShareName -AppName 'wwprofilerlw'
            $n | Should -Be 'wwprofilerlw-content'
            $n | Should -Match '^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$'
        }

        It 'strips illegal characters and stays within 63 chars' {
            $n = Get-ContentShareName -AppName ('WW_Profiler.LW' + ('x' * 80))
            $n.Length        | Should -BeLessOrEqual 63
            $n               | Should -Match '^[a-z0-9-]+$'
            $n.EndsWith('-') | Should -BeFalse
        }
    }

    Context 'Resolve-FunctionStoragePlan (pure decision)' {

        BeforeAll { . $script:DeployScript -LoadFunctionsOnly }

        It 'plans a create when the app does not exist yet' {
            $p = Resolve-FunctionStoragePlan -CurrentAccountName $null -TargetStorageAccount 'wwprofilerlwsa'
            $p.Action | Should -Be 'create'
            $p.Target | Should -Be 'wwprofilerlwsa'
        }

        It 'RECONCILES when the app points at an account that no longer exists (build 30739)' {
            $p = Resolve-FunctionStoragePlan -CurrentAccountName 'stwwprofilerlw' `
                                             -TargetStorageAccount 'wwprofilerlwsa' `
                                             -AppExists -CurrentAccountExists $false
            $p.Action  | Should -Be 'reconcile'
            $p.Current | Should -Be 'stwwprofilerlw'
            $p.Message | Should -Match 'DOES NOT EXIST'
        }

        It 'REPORTS but never silently repoints an app whose account exists (no state orphaning)' {
            $p = Resolve-FunctionStoragePlan -CurrentAccountName 'stwwshared' `
                                             -TargetStorageAccount 'wwprofilerlwsa' `
                                             -AppExists -CurrentAccountExists $true
            $p.Action  | Should -Be 'mismatch'
            $p.Message | Should -Match 'NOT changed'
        }

        It 'plans nothing when the app already points at -StorageAccount (case/whitespace tolerant)' {
            (Resolve-FunctionStoragePlan -CurrentAccountName ' WWProfilerLWSA ' `
                                         -TargetStorageAccount 'wwprofilerlwsa' -AppExists).Action |
                Should -Be 'none'
        }

        It 'reports UNKNOWN — never a reconcile — when the app exists but its setting is unreadable' {
            # An unreadable AzureWebJobsStorage is not evidence of a missing account, and
            # repointing on that inference would move a working app off its own storage.
            $p = Resolve-FunctionStoragePlan -CurrentAccountName '   ' -TargetStorageAccount 'wwprofilerlwsa' -AppExists
            $p.Action  | Should -Be 'unknown'
            $p.Current | Should -BeNullOrEmpty
            $p.Message | Should -Match 'COULD NOT BE READ'
        }

        It 'treats "existence not checked" ($null) as unknown, not as a missing account' {
            # $null and $false are different answers: conflating them turns every unchecked
            # account into a spurious repoint.
            (Resolve-FunctionStoragePlan -CurrentAccountName 'stwwshared' `
                                         -TargetStorageAccount 'wwprofilerlwsa' `
                                         -AppExists -CurrentAccountExists $null).Action |
                Should -Be 'unknown'
        }
    }

    Context 'end-to-end (DryRun)' {

        BeforeEach {
            # Shim models the three states that matter: no app; an app on a storage account
            # that exists; an app on one that does not. `storage account show` is the
            # existence probe, so it must fail for the orphaned account and only for it.
            $global:appExists      = $true
            $global:appStorage     = 'stwwprofilerlw'
            $global:missingStorage = @('stwwprofilerlw')
            function az {
                $a = $args; $global:LASTEXITCODE = 0
                if ($a[0] -eq 'account'     -and $a -contains 'show')   { return '{"id":"sub-123","tenantId":"tid-456","user":{"name":"dev@x"},"name":"My Sub"}' }
                if ($a[0] -eq 'group'       -and $a -contains 'exists') { return 'true' }
                if ($a[0] -eq 'storage'     -and $a -contains 'show-connection-string') {
                    return 'DefaultEndpointsProtocol=https;AccountName=wwprofilerlwsa;AccountKey=k=='
                }
                if ($a[0] -eq 'storage'     -and $a -contains 'account' -and $a -contains 'show') {
                    $i = [array]::IndexOf($a, '--name')
                    $name = ($i -ge 0) ? $a[$i + 1] : ''
                    if ($global:missingStorage -contains $name) { $global:LASTEXITCODE = 1; return $null }
                    return ('{"name":"' + $name + '"}')
                }
                if ($a[0] -eq 'functionapp' -and $a -contains 'appsettings' -and $a -contains 'list') {
                    if (-not $global:appStorage) { return '' }
                    return ('DefaultEndpointsProtocol=https;AccountName=' + $global:appStorage + ';AccountKey=k==')
                }
                if ($a[0] -eq 'functionapp' -and $a -contains 'show') {
                    if (-not $global:appExists) { $global:LASTEXITCODE = 1; return $null }
                    return '{"name":"app","siteConfig":{"netFrameworkVersion":"v10.0"}}'
                }
                if ($a[0] -eq 'keyvault' -and $a -contains 'secret' -and $a -contains 'show') { $global:LASTEXITCODE = 1; return $null }
                return '{}'
            }

            $global:pubDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwpub-" + [guid]::NewGuid())
            New-Item -ItemType Directory -Path $global:pubDir -Force | Out-Null
            '{ "logging": { "logLevel": { "default": "Information" } } }' | Set-Content (Join-Path $global:pubDir 'host.json')
            $global:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwlog-" + [guid]::NewGuid())
            $script:appName = 'wwst-' + [guid]::NewGuid().ToString('N').Substring(0, 8)

            $script:commonArgs = @{
                SubscriptionId      = 'sub-123'
                TenantId            = 'tid-456'
                ResourceGroup       = 'DEV2'
                Location            = 'southafricanorth'
                StorageAccount      = 'wwprofilerlwsa'
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
            Get-ChildItem -Path ([System.IO.Path]::GetTempPath()) -Directory -Filter "wwexecutionengine-stage-$($script:appName)-*" -ErrorAction SilentlyContinue |
                ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
        }

        AfterAll {
            Remove-Variable -Name appExists      -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name appStorage     -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name missingStorage -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name pubDir         -Scope Global -ErrorAction SilentlyContinue
            Remove-Variable -Name logDir         -Scope Global -ErrorAction SilentlyContinue
        }

        It 'repoints an orphaned app BEFORE Phase 4, and says so before the confirmation (build 30739)' {
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String

            $out | Should -Match 'STORAGE ORPHANED'
            $out | Should -Match "points at storage account 'stwwprofilerlw', which does not exist"
            $out | Should -Match '\[DRYRUN\] az storage share-rm create .*--name .*-content'
            # Phase 3 ALSO runs `functionapp config appsettings set` (the env-var apply), so the
            # bare command matches on every run. The reconcile is identifiable only by the
            # settings file it hands az: ww-storage-* vs Phase 3's ww-appsettings-*.
            $out | Should -Match '\[DRYRUN\] az functionapp config appsettings set .*ww-storage-'
            $out | Should -Match 'Dry-run complete'
        }

        It 'states the storage decision in the Phase 0.5 plan' {
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match "Storage wiring\s+: 'stwwprofilerlw' DOES NOT EXIST"
        }

        It 'does NOT repoint an app whose storage account exists — only reports the mismatch' {
            $global:appStorage = 'stwwshared'; $global:missingStorage = @()
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String

            $out | Should -Match 'STORAGE MISMATCH'
            $out | Should -Not -Match '\[DRYRUN\] az functionapp config appsettings set .*ww-storage-'
            $out | Should -Not -Match '\[DRYRUN\] az storage share-rm create'
            $out | Should -Not -Match 'STORAGE ORPHANED'
        }

        It 'does nothing when the existing app already uses -StorageAccount' {
            $global:appStorage = 'wwprofilerlwsa'; $global:missingStorage = @()
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String

            $out | Should -Match "Function App storage is 'wwprofilerlwsa'"
            $out | Should -Not -Match 'STORAGE ORPHANED'
            $out | Should -Not -Match 'STORAGE MISMATCH'
            $out | Should -Not -Match '\[DRYRUN\] az functionapp config appsettings set .*ww-storage-'
        }

        It 'leaves the CREATE path alone — a new app is still born on -StorageAccount' {
            # Regression guard: the reconcile lives on the "already exists" branch only, and
            # `functionapp create` must remain the thing that wires a brand-new app.
            $global:appExists = $false
            $out = (& $script:DeployScript @commonArgs) 6>&1 | Out-String

            $out | Should -Match '\[DRYRUN\] az functionapp create .*--storage-account wwprofilerlwsa'
            $out | Should -Not -Match 'STORAGE ORPHANED'
            $out | Should -Not -Match '\[DRYRUN\] az functionapp config appsettings set .*ww-storage-'
        }

        It 'records the storage decision in the summary for the operator' {
            (& $script:DeployScript @commonArgs) 6>&1 | Out-Null
            $summary = Get-ChildItem -Path $global:logDir -Filter '*.summary.json' | Select-Object -First 1
            $json = Get-Content $summary.FullName -Raw | ConvertFrom-Json
            $json.storage.target | Should -Be 'wwprofilerlwsa'
            $json.storage.before | Should -Be 'stwwprofilerlw'
            $json.storage.action | Should -Be 'reconcile'
        }
    }
}
