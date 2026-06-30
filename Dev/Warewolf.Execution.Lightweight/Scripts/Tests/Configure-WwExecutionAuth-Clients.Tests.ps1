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
