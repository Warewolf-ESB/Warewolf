#Requires -Version 7.0

<#
    Pester 5 test suite for Monitor-RabbitMqShovel.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Monitor-RabbitMqShovel.Tests.ps1

    Design notes (same conventions as Configure-RabbitMqShovel.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no broker/network action —
      used here to unit-test ConvertFrom-AppInsightsConnectionString directly.
    * `Invoke-RestMethod` is shadowed with a tracking *function* keyed off the
      request Uri/Method, so the suite runs with no real RabbitMQ broker and no
      real Application Insights ingestion endpoint. Functions resolve before
      cmdlets in PowerShell's command lookup, and the shim is reachable from the
      externally-invoked script because `&` runs it in a child scope of the
      caller (same mechanism the Configure-RabbitMqShovel suite uses).
    * The script is always run with -DryRun so the (shimmed) Application
      Insights POST is only echoed, never actually attempted — the suite
      asserts on that echoed payload instead of a captured HTTP call.
    * This is deliberately IN ADDITION TO, not a replacement for, real manual
      validation against a live rabbitmq:3-management broker and a real
      Application Insights resource (see docs/ShovelBridge-Architecture.md).
#>

BeforeAll {
    $script:MonitorScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Monitor-RabbitMqShovel.ps1'
}

Describe 'Monitor-RabbitMqShovel — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:MonitorScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:MonitorScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Pre-flight'
        Get-Command Get-ShovelState                     -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Send-AppInsightsEvent                -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command ConvertFrom-AppInsightsConnectionString -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Invoke-RabbitMqApi                   -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'defaults VHost and ShovelName on the parameter block' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:MonitorScript, [ref]$null, [ref]$null)
        $params = $ast.ParamBlock.Parameters
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'VHost' }).DefaultValue.Value      | Should -Be '/'
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'ShovelName' }).DefaultValue.Value  | Should -Be 'wwexecution-shovel'
    }
}

Describe 'Monitor-RabbitMqShovel — ConvertFrom-AppInsightsConnectionString' {

    BeforeAll {
        . $script:MonitorScript -LoadFunctionsOnly
    }

    It 'parses InstrumentationKey and IngestionEndpoint out of a full connection string' {
        $r = ConvertFrom-AppInsightsConnectionString 'InstrumentationKey=abc-123;IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/;LiveEndpoint=https://x'
        $r.InstrumentationKey | Should -Be 'abc-123'
        $r.IngestionEndpoint  | Should -Be 'https://eastus-1.in.applicationinsights.azure.com'
    }

    It 'defaults IngestionEndpoint to the classic global endpoint when omitted' {
        $r = ConvertFrom-AppInsightsConnectionString 'InstrumentationKey=abc-123'
        $r.IngestionEndpoint | Should -Be 'https://dc.services.visualstudio.com'
    }

    It 'throws when InstrumentationKey is missing' {
        { ConvertFrom-AppInsightsConnectionString 'IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/' } | Should -Throw '*InstrumentationKey*'
    }
}

Describe 'Monitor-RabbitMqShovel — end-to-end (DryRun, no broker/telemetry mutation)' {

    BeforeEach {
        $global:shovelState = 'running'
        $global:shovelExists = $true

        function Invoke-RestMethod {
            param($Method, $Uri, $Headers, $ContentType, $TimeoutSec, $Body)
            if ($Uri -match '/api/shovels/') {
                if (-not $global:shovelExists) { return @() }
                return @([pscustomobject]@{ name = 'wwexecution-shovel'; state = $global:shovelState })
            }
            if ($Uri -match '/v2/track$') {
                throw 'Test shim: Application Insights POST should not be attempted under -DryRun.'
            }
            throw "Unhandled Uri in test shim: $Uri"
        }

        $global:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ('wwshovelmonlog-' + [guid]::NewGuid())

        $script:commonArgs = @{
            RabbitMqManagementUri = 'http://localhost:15672'
            RabbitMqUsername      = 'test'
            RabbitMqPassword      = (ConvertTo-SecureString 'test' -AsPlainText -Force)
            LogDir                 = $global:logDir
            NonInteractive         = $true
            DryRun                 = $true
        }
    }

    AfterEach {
        if ($global:logDir -and (Test-Path -LiteralPath $global:logDir)) {
            Remove-Item -LiteralPath $global:logDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    AfterAll {
        Remove-Variable -Name shovelState, shovelExists, logDir -Scope Global -ErrorAction SilentlyContinue
    }

    Context 'healthy shovel' {
        It 'reports healthy and completes without throwing' {
            { & $script:MonitorScript @commonArgs } | Should -Not -Throw
        }

        It 'writes a "healthy" run summary' {
            & $script:MonitorScript @commonArgs
            $summary = Get-Content (Get-ChildItem $global:logDir -Filter '*.summary.json' | Select-Object -First 1).FullName | ConvertFrom-Json
            $summary.status      | Should -Be 'healthy'
            $summary.shovelState | Should -Be 'running'
        }

        It 'does not attempt telemetry when -AppInsightsConnectionString is omitted' {
            $out = (& $script:MonitorScript @commonArgs) 6>&1 | Out-String
            $out | Should -Match 'No -AppInsightsConnectionString supplied'
            $out | Should -Not -Match '\[DRYRUN\] POST'
        }

        It 'does not emit telemetry by default even when configured (requires -SendHeartbeatOnHealthy)' {
            $a = $script:commonArgs.Clone(); $a.AppInsightsConnectionString = 'InstrumentationKey=abc-123;IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/'
            $out = (& $script:MonitorScript @a) 6>&1 | Out-String
            $out | Should -Not -Match '\[DRYRUN\] POST'
        }

        It 'echoes a heartbeat telemetry event with healthy=true when -SendHeartbeatOnHealthy is set' {
            $a = $script:commonArgs.Clone()
            $a.AppInsightsConnectionString = 'InstrumentationKey=abc-123;IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/'
            $a.SendHeartbeatOnHealthy = $true
            $out = (& $script:MonitorScript @a) 6>&1 | Out-String
            $out | Should -Match '\[DRYRUN\] POST https://eastus-1\.in\.applicationinsights\.azure\.com/v2/track'
            $out | Should -Match '"name":\s*"ShovelHealthCheck"'
            $out | Should -Match '"healthy":\s*"true"'
            $out | Should -Match '"state":\s*"running"'
        }
    }

    Context 'unhealthy shovel (wrong state)' {
        BeforeEach { $global:shovelState = 'terminated' }

        It 'throws a clear error naming the unexpected state' {
            { & $script:MonitorScript @commonArgs } | Should -Throw '*terminated*'
        }

        It 'writes a "failed" run summary with the reported state' {
            try { & $script:MonitorScript @commonArgs } catch { }
            $summary = Get-Content (Get-ChildItem $global:logDir -Filter '*.summary.json' | Select-Object -First 1).FullName | ConvertFrom-Json
            $summary.status      | Should -Be 'failed'
            $summary.shovelState | Should -Be 'terminated'
        }

        It 'echoes a failure telemetry event with healthy=false when Application Insights is configured' {
            # The script throws after emitting telemetry, which aborts the pipeline before
            # Out-String's End block would run — capture the Information stream directly via
            # -InformationVariable (a CmdletBinding common parameter) instead, so the transcript
            # survives the terminating error.
            $a = $script:commonArgs.Clone()
            $a.AppInsightsConnectionString = 'InstrumentationKey=abc-123;IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/'
            $infoVar = $null
            try { & $script:MonitorScript @a -InformationVariable infoVar | Out-Null } catch { }
            $out = $infoVar | Out-String
            $out | Should -Match '\[DRYRUN\] POST https://eastus-1\.in\.applicationinsights\.azure\.com/v2/track'
            $out | Should -Match '"healthy":\s*"false"'
            $out | Should -Match '"state":\s*"terminated"'
        }
    }

    Context 'missing shovel (not yet configured / removed)' {
        BeforeEach { $global:shovelExists = $false }

        It 'throws a clear error identifying the shovel as not-configured' {
            { & $script:MonitorScript @commonArgs } | Should -Throw '*not-configured*'
        }
    }

    Context 'secret handling' {
        It 'never prints the plaintext RabbitMQ password' {
            $out = (& $script:MonitorScript @commonArgs) 6>&1 | Out-String
            $out | Should -Not -Match 'test'  -Because 'the literal password value must never appear in output'
        }
    }
}
