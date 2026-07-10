#Requires -Version 7.0

<#
    Pester 5 test suite for Configure-WwExecutionAuth-ClientApps.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Configure-WwExecutionAuth-ClientApps.Tests.ps1

    Design notes
    ------------
    * The orchestrator exposes -LoadFunctionsOnly so its pure helpers can be
      dot-sourced and unit-tested without prompts or any cloud / Graph action
      (same pattern as Configure-WwExecutionAuth-Clients.Tests.ps1). Loading the
      orchestrator also dot-sources Configure-WwExecutionAuth-Clients.ps1
      -LoadFunctionsOnly, so its helpers (Test-IsPlaceholder, Write-*) are present.
    * Get-ClientAppDefinitions is the single source of truth for the per-app
      provisioning plan; Get-ValidationPlan is the pure validation-decision helper;
      Format-ClientAppSummary masks secrets. All three are tested in isolation.
#>

BeforeAll {
    $script:ClientAppsScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Configure-WwExecutionAuth-ClientApps.ps1'
}

Describe 'Configure-WwExecutionAuth-ClientApps — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:ClientAppsScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'Apps ValidateSet includes every example app plus All' {
        $validateSet = (Get-Command $script:ClientAppsScript).Parameters['Apps'].Attributes |
            Where-Object { $_ -is [System.Management.Automation.ValidateSetAttribute] }
        foreach ($k in 'angular', 'react', 'webmvc', 'console', 'azurefunction', 'servicebus', 'All') {
            $validateSet.ValidValues | Should -Contain $k
        }
    }

    It 'AppRolesToAssign has no stale default (empty)' {
        . $script:ClientAppsScript -LoadFunctionsOnly
        @($AppRolesToAssign).Count | Should -Be 0
    }

    It 'fails loudly at pre-flight when a Console app is selected without -AppRolesToAssign' {
        # The non-interactive path up to the console guard is pure console output
        # (no az / Graph calls), so the full script can be invoked safely: a
        # roleless console client-credentials flow must throw BEFORE provisioning.
        { & $script:ClientAppsScript -ResourceAppId 'res-app-id' -TenantId 'tenant-id' `
            -Apps console -NonInteractive 6>$null } |
            Should -Throw -ExpectedMessage '*REQUIRES -AppRolesToAssign*'
    }

    It 'does NOT block a daemon-only selection at pre-flight (child defaults to Warewolf_ClientApps)' {
        # Daemon apps are exempt from the pre-flight guard: the child script defaults
        # -AppRolesToAssign to 'Warewolf_ClientApps' and fails loud there instead. So a
        # roleless daemon selection must get PAST the guard; any failure comes from the
        # downstream cloud/Graph calls, never the roleless-guard message.
        $err = $null
        try {
            & $script:ClientAppsScript -ResourceAppId 'res-app-id' -TenantId 'tenant-id' `
                -Apps azurefunction -NonInteractive 6>$null 2>$null
        } catch { $err = $_ }
        if ($err) { $err.Exception.Message | Should -Not -Match 'REQUIRE -AppRolesToAssign' }
    }

    It 'exposes -AzureFunctionClientAppName and -AzureFunctionClientResourceGroup' {
        $params = (Get-Command $script:ClientAppsScript).Parameters
        $params.ContainsKey('AzureFunctionClientAppName')       | Should -BeTrue
        $params.ContainsKey('AzureFunctionClientResourceGroup') | Should -BeTrue
    }
}

Describe 'Configure-WwExecutionAuth-ClientApps — -LoadFunctionsOnly' {

    It 'defines helpers without running a stage' {
        $out = (. $script:ClientAppsScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Provision:'
        $out | Should -Not -Match 'Stage 4'
        Get-Command Get-ClientAppDefinitions -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-ValidationPlan       -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Format-ClientAppSummary  -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'reuses the child script helpers (Test-IsPlaceholder)' {
        . $script:ClientAppsScript -LoadFunctionsOnly
        Get-Command Test-IsPlaceholder -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }
}

Describe 'Configure-WwExecutionAuth-ClientApps — Get-ClientAppDefinitions' {

    BeforeAll { . $script:ClientAppsScript -LoadFunctionsOnly }

    It 'returns exactly the six example apps in canonical order' {
        $defs = Get-ClientAppDefinitions
        @($defs).Count | Should -Be 6
        @($defs).Key | Should -Be @('angular', 'react', 'webmvc', 'console', 'azurefunction', 'servicebus')
    }

    It 'maps each app to the correct registration type' {
        $byKey = @{}
        foreach ($d in Get-ClientAppDefinitions) { $byKey[$d.Key] = $d }
        $byKey['angular'].ClientType       | Should -Be 'SPA'
        $byKey['react'].ClientType         | Should -Be 'SPA'
        $byKey['webmvc'].ClientType        | Should -Be 'Confidential'
        $byKey['console'].ClientType       | Should -Be 'Console'
        $byKey['azurefunction'].ClientType | Should -Be 'Daemon'
        $byKey['servicebus'].ClientType    | Should -Be 'Daemon'
    }

    It 'uses the example apps'' real redirect URIs / ports' {
        $byKey = @{}
        foreach ($d in Get-ClientAppDefinitions) { $byKey[$d.Key] = $d }
        $byKey['angular'].RedirectUris | Should -Contain 'http://localhost:4201'
        $byKey['react'].RedirectUris   | Should -Contain 'http://localhost:5173'
        $byKey['webmvc'].RedirectUris  | Should -Contain 'https://localhost:5001/signin-oidc'
        $byKey['console'].RedirectUris | Should -Contain 'http://localhost'
    }

    It 'flags the app-only clients as role-required and the SPAs as CORS' {
        $byKey = @{}
        foreach ($d in Get-ClientAppDefinitions) { $byKey[$d.Key] = $d }
        $byKey['console'].RequiresRole       | Should -BeTrue
        $byKey['azurefunction'].RequiresRole | Should -BeTrue
        $byKey['servicebus'].RequiresRole    | Should -BeTrue
        $byKey['angular'].RequiresRole       | Should -BeFalse
        $byKey['angular'].Cors               | Should -BeTrue
        $byKey['react'].Cors                 | Should -BeTrue
        $byKey['webmvc'].Cors                | Should -BeFalse
    }

    It 'gives each app a distinct display-name prefix' {
        $prefixes = (Get-ClientAppDefinitions).Prefix
        ($prefixes | Sort-Object -Unique).Count | Should -Be 6
        $prefixes | Should -Contain 'wwexecution-angular'
        $prefixes | Should -Contain 'wwexecution-servicebus'
    }
}

Describe 'Configure-WwExecutionAuth-ClientApps — Resolve-SelectedApps' {

    BeforeAll { . $script:ClientAppsScript -LoadFunctionsOnly }

    It 'expands All to every app in canonical order' {
        (Resolve-SelectedApps -Selection @('All')).Key | Should -Be @('angular', 'react', 'webmvc', 'console', 'azurefunction', 'servicebus')
    }

    It 'expands empty selection to every app' {
        @(Resolve-SelectedApps -Selection @()).Count | Should -Be 6
    }

    It 'filters to the requested subset preserving canonical order' {
        (Resolve-SelectedApps -Selection @('servicebus', 'angular')).Key | Should -Be @('angular', 'servicebus')
    }
}

Describe 'Configure-WwExecutionAuth-ClientApps — Get-ValidationPlan' {

    BeforeAll { . $script:ClientAppsScript -LoadFunctionsOnly }

    It 'auto-validates a secret-bearing daemon via client-credentials' {
        $p = Get-ValidationPlan -ClientType 'Daemon' -HasSecret $true -UseManagedIdentity $false
        $p.Flow | Should -Be 'client-credentials'
        $p.Auto | Should -BeTrue
    }

    It 'skips a managed-identity daemon (no local secret)' {
        $p = Get-ValidationPlan -ClientType 'Daemon' -HasSecret $false -UseManagedIdentity $true
        $p.Flow | Should -Be 'managed-identity'
        $p.Auto | Should -BeFalse
    }

    It 'auto-validates a console client-credentials flow and offers device-code' {
        $p = Get-ValidationPlan -ClientType 'Console' -HasSecret $true -UseManagedIdentity $false
        $p.Flow        | Should -Be 'client-credentials'
        $p.Auto        | Should -BeTrue
        $p.Interactive | Should -BeTrue
    }

    It 'validates SPA interactively (device-code opt-in, never auto)' {
        $p = Get-ValidationPlan -ClientType 'SPA' -HasSecret $false -UseManagedIdentity $false
        $p.Flow        | Should -Be 'device-code'
        $p.Auto        | Should -BeFalse
        $p.Interactive | Should -BeTrue
    }

    It 'marks a confidential web app as manual (device-code unsupported)' {
        $p = Get-ValidationPlan -ClientType 'Confidential' -HasSecret $true -UseManagedIdentity $false
        $p.Flow        | Should -Be 'manual'
        $p.Auto        | Should -BeFalse
        $p.Interactive | Should -BeFalse
    }
}

Describe 'Configure-WwExecutionAuth-ClientApps — Format-ClientAppSummary' {

    BeforeAll { . $script:ClientAppsScript -LoadFunctionsOnly }

    It 'redacts the client secret but keeps ClientId and status' {
        $rec = [pscustomobject]@{
            App = 'console'; DisplayName = '.NET 8 Console'; ClientType = 'Console'
            ClientId = 'client-guid'; GrantType = 'CC'; RedirectUris = @('http://localhost')
            RolesAssigned = @('Warewolf_Developers'); SecretExpiry = '2027-01-01'
            ClientSecret = 'SUPER-SECRET-VALUE'; OutputFile = 'x.json'; Status = 'provisioned'; Validation = $null
        }
        $summary = Format-ClientAppSummary -Results @($rec) -TenantId 't' -ResourceAppId 'r' -FunctionAppName 'fa' -Timestamp 'ts'
        $client = $summary.Clients[0]
        $client.ClientSecret | Should -Be '***REDACTED***'
        $client.HasSecret    | Should -BeTrue
        $client.ClientId     | Should -Be 'client-guid'
        $client.Status       | Should -Be 'provisioned'
        ($summary | ConvertTo-Json -Depth 8) | Should -Not -Match 'SUPER-SECRET-VALUE'
    }

    It 'reports HasSecret false and null secret for public clients' {
        $rec = [pscustomobject]@{
            App = 'angular'; DisplayName = 'Angular 17 SPA'; ClientType = 'SPA'
            ClientId = 'spa-guid'; GrantType = 'PKCE'; RedirectUris = @('http://localhost:4201')
            RolesAssigned = @(); SecretExpiry = $null; ClientSecret = $null; OutputFile = 'a.json'
            Status = 'provisioned'; Validation = $null
        }
        $summary = Format-ClientAppSummary -Results @($rec) -TenantId 't' -ResourceAppId 'r' -FunctionAppName 'fa' -Timestamp 'ts'
        $summary.Clients[0].HasSecret    | Should -BeFalse
        $summary.Clients[0].ClientSecret | Should -BeNullOrEmpty
    }

    It 'derives scope and authority from the resource app and tenant' {
        $summary = Format-ClientAppSummary -Results @() -TenantId 'tid' -ResourceAppId 'rid' -FunctionAppName 'fa' -Timestamp 'ts'
        $summary.Scope          | Should -Be 'api://rid/.default'
        $summary.Authority      | Should -Be 'https://login.microsoftonline.com/tid'
        $summary.FunctionAppUrl | Should -Be 'https://fa.azurewebsites.net'
    }
}
