#Requires -Version 7.0

<#
    Pester 5 test suite for Configure-WwExecutionAuth-Clients.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Configure-WwExecutionAuth-Clients.Tests.ps1

    Design notes
    ------------
    * The script exposes -LoadFunctionsOnly so its helper functions can be
      dot-sourced and unit-tested without executing any prompt or cloud / Graph
      action (same pattern as Configure-WwExecutionAuth.Tests.ps1).
    * Resolve-AppRoleAssignment is the fail-loud guard for app-only clients
      (Daemon / Managed Identity / the Console client-credentials flow): an
      app-only caller with no resolvable role carries no 'roles' claim and is
      rejected by the engine, so the helper THROWS rather than silently creating
      a roleless client. These tests drive it with synthetic role maps.
#>

BeforeAll {
    $script:ClientScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Configure-WwExecutionAuth-Clients.ps1'
}

Describe 'Configure-WwExecutionAuth-Clients — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:ClientScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'ClientType ValidateSet includes Console' {
        $validateSet = (Get-Command $script:ClientScript).Parameters['ClientType'].Attributes |
            Where-Object { $_ -is [System.Management.Automation.ValidateSetAttribute] }
        $validateSet.ValidValues | Should -Contain 'Console'
    }

    It 'AppRolesToAssign has no stale default (empty)' {
        . $script:ClientScript -LoadFunctionsOnly
        @($AppRolesToAssign).Count | Should -Be 0
    }

    It 'ConsoleRedirectUris defaults to http://localhost' {
        . $script:ClientScript -LoadFunctionsOnly
        $ConsoleRedirectUris | Should -Contain 'http://localhost'
    }
}

Describe 'Configure-WwExecutionAuth-Clients — -LoadFunctionsOnly' {

    It 'defines helpers without running a stage' {
        $out = (. $script:ClientScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Stage 0'
        Get-Command Resolve-AppRoleAssignment -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }
}

Describe 'Configure-WwExecutionAuth-Clients — Resolve-AppRoleAssignment' {

    BeforeAll {
        . $script:ClientScript -LoadFunctionsOnly
    }

    It 'resolves a requested role that exists on the resource app' {
        $map = @{ 'Warewolf_Developers' = 'role-guid-1'; 'WorkflowViewer' = 'role-guid-2' }
        $res = Resolve-AppRoleAssignment -RequestedRoles @('Warewolf_Developers') -RoleIdMap $map
        @($res).Count | Should -Be 1
        $res[0].Name  | Should -Be 'Warewolf_Developers'
        $res[0].Id    | Should -Be 'role-guid-1'
    }

    It 'resolves the dedicated Warewolf_ClientApps group role for an MI daemon (fail-loud path satisfied)' {
        # Warewolf_ClientApps is the group the deploy authconfig example ships for
        # app-only client apps (e.g. the AzureFunction example's Managed Identity).
        $map = @{ 'Warewolf_ClientApps' = 'role-guid-ca'; 'Warewolf_Developers' = 'role-guid-1' }
        $res = Resolve-AppRoleAssignment -RequestedRoles @('Warewolf_ClientApps') -RoleIdMap $map -RequireAtLeastOne -ClientKind 'managed identity'
        @($res).Count | Should -Be 1
        $res[0].Name  | Should -Be 'Warewolf_ClientApps'
        $res[0].Id    | Should -Be 'role-guid-ca'
    }

    It 'omits requested roles that do not exist on the resource app' {
        $map = @{ 'Warewolf_Developers' = 'role-guid-1' }
        $res = Resolve-AppRoleAssignment -RequestedRoles @('Warewolf_Developers', 'Ghost') -RoleIdMap $map 6>$null
        @($res).Name | Should -Be 'Warewolf_Developers'
    }

    It 'returns empty (no throw) when no role resolves and -RequireAtLeastOne is NOT set (delegated/console)' {
        $res = Resolve-AppRoleAssignment -RequestedRoles @() -RoleIdMap @{ 'X' = 'y' }
        @($res).Count | Should -Be 0
    }

    It 'THROWS (fail-loud) for a daemon/MI when no role is requested' {
        { Resolve-AppRoleAssignment -RequestedRoles @() -RoleIdMap @{ 'X' = 'y' } -RequireAtLeastOne -ClientKind 'daemon' } |
            Should -Throw -ExpectedMessage '*REJECTED by the engine*'
    }

    It 'THROWS when requested roles match no resource-app role (RequireAtLeastOne)' {
        { Resolve-AppRoleAssignment -RequestedRoles @('Permission.Execute') -RoleIdMap @{ 'Warewolf_Developers' = 'g' } -RequireAtLeastOne -ClientKind 'managed identity' 6>$null } |
            Should -Throw -ExpectedMessage '*No assignable app role*'
    }

    It 'THROWS mentioning none-defined when the resource app exposes no roles' {
        { Resolve-AppRoleAssignment -RequestedRoles @('Anything') -RoleIdMap @{} -RequireAtLeastOne 6>$null } |
            Should -Throw -ExpectedMessage '*no app roles defined*'
    }
}

Describe 'Configure-WwExecutionAuth-Clients — Daemon Managed-Identity params' {

    It 'exposes -DaemonFunctionAppName and -DaemonFunctionAppResourceGroup' {
        $params = (Get-Command $script:ClientScript).Parameters
        $params.ContainsKey('DaemonFunctionAppName')          | Should -BeTrue
        $params.ContainsKey('DaemonFunctionAppResourceGroup') | Should -BeTrue
    }

    It 'documents both new params in comment-based help' {
        $raw = Get-Content $script:ClientScript -Raw
        $raw | Should -Match '\.PARAMETER\s+DaemonFunctionAppName'
        $raw | Should -Match '\.PARAMETER\s+DaemonFunctionAppResourceGroup'
    }
}

Describe 'Configure-WwExecutionAuth-Clients — Resolve-MiPrincipalIdFromCli' {

    BeforeAll {
        . $script:ClientScript -LoadFunctionsOnly
    }

    It 'returns the principalId from a parsed identity object' {
        $obj = [pscustomobject]@{ principalId = '11111111-2222-3333-4444-555555555555'; type = 'SystemAssigned' }
        Resolve-MiPrincipalIdFromCli -IdentityObject $obj | Should -Be '11111111-2222-3333-4444-555555555555'
    }

    It 'ignores unrelated properties and still returns the principalId' {
        $obj = [pscustomobject]@{ tenantId = 'aaaa'; userAssignedIdentities = @{}; principalId = 'pid-123' }
        Resolve-MiPrincipalIdFromCli -IdentityObject $obj | Should -Be 'pid-123'
    }

    It 'returns $null (no throw) when principalId is absent and -AllowNull is set' {
        $obj = [pscustomobject]@{ tenantId = 'aaaa' }
        Resolve-MiPrincipalIdFromCli -IdentityObject $obj -AllowNull | Should -BeNullOrEmpty
    }

    It 'returns $null (no throw) for a $null input when -AllowNull is set' {
        Resolve-MiPrincipalIdFromCli -IdentityObject $null -AllowNull | Should -BeNullOrEmpty
    }

    It 'THROWS when principalId is absent and -AllowNull is NOT set' {
        { Resolve-MiPrincipalIdFromCli -IdentityObject ([pscustomobject]@{ tenantId = 'aaaa' }) } |
            Should -Throw -ExpectedMessage '*principalId*'
    }

    It 'THROWS for a $null input when -AllowNull is NOT set' {
        { Resolve-MiPrincipalIdFromCli -IdentityObject $null } |
            Should -Throw -ExpectedMessage '*principalId*'
    }

    It 'treats a whitespace-only principalId as absent (throws without -AllowNull)' {
        { Resolve-MiPrincipalIdFromCli -IdentityObject ([pscustomobject]@{ principalId = '   ' }) } |
            Should -Throw -ExpectedMessage '*principalId*'
    }
}
