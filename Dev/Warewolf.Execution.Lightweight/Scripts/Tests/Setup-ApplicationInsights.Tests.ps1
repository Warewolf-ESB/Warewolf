#Requires -Version 7.0
#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0.0' }
<#
    Pester 5 suite for Setup-ApplicationInsights.ps1 — the transient-retry behaviour.

    Build 30801 failed the whole engine deploy on ONE ARM front-door 503 ("Service
    Unavailable(<!DOCTYPE ...>Our services aren't available right now") while setting
    WAREWOLF_APPINSIGHTS_CONNECTION_STRING. These tests pin that a transient failure is
    retried, that a real error is not, and that the connection string never reaches the log.

    Fully offline: `az` is shimmed as a global function; -RetryDelaySeconds @(0) keeps the
    retries from sleeping.
#>

BeforeAll {
    $script:AiScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Setup-ApplicationInsights.ps1'

    # The exact error az printed in build 30801 (trimmed HTML body, same signature).
    $script:ArmOutage503 = "ERROR: Service Unavailable(<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' " +
        "'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'><html><head><title>AzureResourceManager</title></head>" +
        "<body><div id='message'><h2>Our services aren't available right now</h2><p>We're working to restore all services " +
        "as soon as possible. Please check back soon.</p></div></body></html>)"

    $script:ConnString = 'InstrumentationKey=a6927f24-974a-48e3-8e32-f0fa98d00000;IngestionEndpoint=https://southafricanorth-1.in.applicationinsights.azure.com/;ApplicationId=secret-app-id-9999'

    function Invoke-AiScript {
        (& $script:AiScript -ResourceGroup 'DEV2' -FunctionAppName 'WarewolfServer-UAT' -Location 'southafricanorth' -RetryDelaySeconds @(0)) 6>&1 |
            Out-String
    }

    function Get-AzCallCount([string] $Pattern) {
        @($global:azCalls | Where-Object { $_ -match $Pattern }).Count
    }
}

Describe 'Setup-ApplicationInsights — transient retry' {

    BeforeEach {
        $global:azCalls       = [System.Collections.Generic.List[string]]::new()
        $global:aiMissing     = $false
        # Outputs to fail `appsettings set` with, one per call, before it succeeds.
        $global:setFailures   = [System.Collections.Generic.Queue[string]]::new()
        $global:shimConn      = $script:ConnString

        function global:az {
            $a = $args; $line = ($a -join ' '); $global:azCalls.Add($line); $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account' -and $a -contains '--query') { return 'South Africa Subscription' }
            if ($a[0] -eq 'account')                             { return '{"name":"South Africa Subscription"}' }
            if ($a[0] -eq 'group' -and $a[1] -eq 'exists')       { return 'true' }
            if ($a[0] -eq 'monitor' -and $a -contains 'create')  { return '{"name":"ai"}' }
            if ($a[0] -eq 'monitor' -and $a -contains '--query') { return $global:shimConn }
            if ($a[0] -eq 'monitor' -and $a -contains 'show') {
                if ($global:aiMissing) {
                    $global:LASTEXITCODE = 3
                    return "ERROR: (ResourceNotFound) The Resource 'Microsoft.Insights/components/WarewolfServer-UAT-ai' was not found."
                }
                return '{"name":"WarewolfServer-UAT-ai"}'
            }
            if ($a[0] -eq 'functionapp' -and $a -contains 'set') {
                if ($global:setFailures.Count -gt 0) { $global:LASTEXITCODE = 1; return $global:setFailures.Dequeue() }
                return $null
            }
            if ($a[0] -eq 'functionapp' -and $a -contains 'list' -and $line -match "\]\.value") { return '{"console":true}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'list') {
                return ('[{"name":"WAREWOLF_APPINSIGHTS_CONNECTION_STRING","value":"x"},' +
                        '{"name":"WAREWOLF_LOGGING_CONFIG","value":"{\"console\":true,\"appInsights\":true}"}]')
            }
            return $null
        }
    }

    AfterEach {
        Remove-Item Function:\az -ErrorAction SilentlyContinue
    }

    AfterAll {
        foreach ($v in 'azCalls', 'aiMissing', 'setFailures', 'shimConn') {
            Remove-Variable -Name $v -Scope Global -ErrorAction SilentlyContinue
        }
    }

    It 'configures the app with each az call made once on the happy path' {
        $out = Invoke-AiScript
        $LASTEXITCODE | Should -Be 0
        $out | Should -Match 'Function App configured successfully'
        $out | Should -Match 'Configuration verified successfully'
        Get-AzCallCount 'appsettings set' | Should -Be 2
        Get-AzCallCount 'component create' | Should -Be 0
        $out | Should -Not -Match 'transient Azure error'
    }

    It 'retries the build-30801 ARM outage page once and then succeeds' {
        $global:setFailures.Enqueue($script:ArmOutage503)
        $out = Invoke-AiScript
        $LASTEXITCODE | Should -Be 0
        $out | Should -Match 'transient Azure error \(attempt 1/5\)'
        $out | Should -Match 'Function App configured successfully'
        # 2 for the connection string (one failed, one retried) + 1 for WAREWOLF_LOGGING_CONFIG.
        Get-AzCallCount 'appsettings set' | Should -Be 3
    }

    It 'gives up after 5 attempts on a persistent 503 and keeps the existing failure message' {
        1..5 | ForEach-Object { $global:setFailures.Enqueue($script:ArmOutage503) }
        $out = Invoke-AiScript
        $LASTEXITCODE | Should -Be 1
        $out | Should -Match 'Failed to set connection string'
        $out | Should -Match 'transient Azure error \(attempt 4/5\)'
        $out | Should -Not -Match 'attempt 5/5'
        Get-AzCallCount 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING=' | Should -Be 5
        # The real cause still reaches the log.
        $out | Should -Match "Our services aren't available right now"
    }

    It 'does NOT retry a real error' {
        $global:setFailures.Enqueue("ERROR: (AuthorizationFailed) The client does not have authorization to perform action 'Microsoft.Web/sites/config/write'.")
        $out = Invoke-AiScript
        $LASTEXITCODE | Should -Be 1
        $out | Should -Match 'Failed to set connection string'
        $out | Should -Match 'AuthorizationFailed'
        $out | Should -Not -Match 'transient Azure error'
        Get-AzCallCount 'appsettings set' | Should -Be 1
    }

    It 'treats "component not found" as the create path, not a retry' {
        $global:aiMissing = $true
        $out = Invoke-AiScript
        $LASTEXITCODE | Should -Be 0
        $out | Should -Match "Application Insights 'WarewolfServer-UAT-ai' not found. Creating"
        $out | Should -Not -Match 'transient Azure error'
        Get-AzCallCount 'component show --app' | Should -Be 2   # the probe (once) + the connection-string read
        Get-AzCallCount 'component create' | Should -Be 1
    }

    It 'never writes the full connection string to the output, retries included' {
        1..2 | ForEach-Object { $global:setFailures.Enqueue($script:ArmOutage503) }
        $out = Invoke-AiScript
        $LASTEXITCODE | Should -Be 0
        $out | Should -Match 'transient Azure error'
        $out | Should -Not -Match ([regex]::Escape($script:ConnString))
        $out | Should -Not -Match 'secret-app-id-9999'
    }
}

Describe 'Setup-ApplicationInsights — connection-string setting name' {
    # The engine reads the non-standard WAREWOLF_APPINSIGHTS_CONNECTION_STRING; the Service Bus worker
    # and job processor read the standard APPLICATIONINSIGHTS_CONNECTION_STRING. Writing only the
    # engine's name to those apps left their App Insights empty (wwuat3-sbworker: 0 rows in every table).

    BeforeEach {
        $global:azCalls = [System.Collections.Generic.List[string]]::new()
        # A global function cannot see this file's $script: scope - hand the value over globally.
        $global:shimConnName = $script:ConnString

        function global:az {
            $a = $args; $line = ($a -join ' '); $global:azCalls.Add($line); $global:LASTEXITCODE = 0
            if ($a[0] -eq 'account' -and $a -contains '--query') { return 'South Africa Subscription' }
            if ($a[0] -eq 'account')                             { return '{"name":"South Africa Subscription"}' }
            if ($a[0] -eq 'group' -and $a[1] -eq 'exists')       { return 'true' }
            if ($a[0] -eq 'monitor' -and $a -contains '--query') { return $global:shimConnName }
            if ($a[0] -eq 'monitor' -and $a -contains 'show')    { return '{"name":"app-ai"}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'set') { return $null }
            if ($a[0] -eq 'functionapp' -and $a -contains 'list' -and $line -match "\]\.value") { return '{"console":true}' }
            if ($a[0] -eq 'functionapp' -and $a -contains 'list') {
                # Echo back whichever connection-string name the verify step asked for.
                $name = [regex]::Match($line, "\[\?name=='([A-Z_]+)'").Groups[1].Value
                return ('[{"name":"' + $name + '","value":"x"},' +
                        '{"name":"WAREWOLF_LOGGING_CONFIG","value":"{\"console\":true,\"appInsights\":true}"}]')
            }
            return $null
        }

        function script:Invoke-Setup([hashtable] $Extra = @{}) {
            (& $script:AiScript -ResourceGroup 'DEV2' -FunctionAppName 'wwsbworker' -Location 'southafricanorth' `
                -RetryDelaySeconds @(0) @Extra) 6>&1 | Out-String
        }

        function script:Get-SettingsSetCalls { @($global:azCalls | Where-Object { $_ -match 'appsettings set' }) }
    }

    AfterEach { Remove-Item Function:\az -ErrorAction SilentlyContinue }
    AfterAll  { Remove-Variable -Name azCalls, shimConnName -Scope Global -ErrorAction SilentlyContinue }

    It 'defaults to the engine''s non-standard name and sets the logging flag (engine unchanged)' {
        $out = Invoke-Setup
        $LASTEXITCODE | Should -Be 0
        $sets = Get-SettingsSetCalls
        $sets.Count | Should -Be 2
        $sets[0] | Should -Match '--settings WAREWOLF_APPINSIGHTS_CONNECTION_STRING='
        $sets[1] | Should -Match '--settings WAREWOLF_LOGGING_CONFIG='
        @($sets | Where-Object { $_ -match '--settings APPLICATIONINSIGHTS_CONNECTION_STRING=' }).Count | Should -Be 0
        $out | Should -Match 'Configuration verified successfully'
    }

    It 'writes the standard name when asked, and verifies against that name' {
        $out = Invoke-Setup @{ ConnectionStringSettingName = 'APPLICATIONINSIGHTS_CONNECTION_STRING' }
        $LASTEXITCODE | Should -Be 0
        (Get-SettingsSetCalls)[0] | Should -Match '--settings APPLICATIONINSIGHTS_CONNECTION_STRING='
        @(Get-SettingsSetCalls | Where-Object { $_ -match 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING' }).Count | Should -Be 0
        @($global:azCalls | Where-Object { $_ -match "appsettings list" -and $_ -match "name=='APPLICATIONINSIGHTS_CONNECTION_STRING'" }).Count |
            Should -Be 1 -Because 'the verify step must look for the name it wrote'
        $out | Should -Match 'Configuration verified successfully'
    }

    It '-SkipLoggingConfigFlag writes no WAREWOLF_LOGGING_CONFIG and still verifies' {
        $out = Invoke-Setup @{ ConnectionStringSettingName = 'APPLICATIONINSIGHTS_CONNECTION_STRING'; SkipLoggingConfigFlag = $true }
        $LASTEXITCODE | Should -Be 0
        $sets = Get-SettingsSetCalls
        $sets.Count | Should -Be 1
        $sets[0] | Should -Not -Match 'WAREWOLF_LOGGING_CONFIG'
        $out | Should -Match 'Configuration verified successfully'
    }

    It 'rejects a misspelt setting name rather than writing a setting nothing reads' {
        { & $script:AiScript -ResourceGroup 'DEV2' -FunctionAppName 'x' -ConnectionStringSettingName 'APPINSIGHTS_CONNECTIONSTRING' } |
            Should -Throw -ExpectedMessage '*does not belong to the set*'   # the ValidateSet, not a missing parameter
    }
}
