#Requires -Version 7.0

<#
    Pester 5 test suite for Configure-RabbitMqShovel.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Configure-RabbitMqShovel.Tests.ps1

    Design notes (same conventions as Deploy-WwJobProcessor.Tests.ps1 /
    Deploy-WwExecutionServiceBusWorker.Tests.ps1, adapted for the RabbitMQ
    Management HTTP API instead of the az CLI)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no broker/network action —
      used here to unit-test the AMQP URI builders directly (the highest-value,
      most failure-prone logic in this script).
    * `Invoke-RestMethod` is shadowed with a tracking *function* keyed off the
      request Uri, so the suite runs with no real RabbitMQ broker. Functions
      resolve before cmdlets in PowerShell's command lookup, and the shim is
      reachable from the externally-invoked script because `&` runs it in a
      child scope of the caller (same mechanism the JobProcessor suite uses to
      shadow `az`).
    * Only the Phase 1 "verify source queue" and Phase 0 "/api/overview /
      /api/nodes" probes are non-mutating GETs that run even under -DryRun —
      those are the only calls the shim needs to answer for a -DryRun suite.
      Phase 2's PUT (create/update the shovel) is -Mutating and is skipped
      (echoed only) under -DryRun, so it needs no shim response here.
    * This is deliberately IN ADDITION TO, not a replacement for, the real
      Docker-based validation already performed manually against a live
      rabbitmq:3-management broker (see docs/ShovelBridge-Architecture.md).
#>

BeforeAll {
    $script:ConfigureScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Configure-RabbitMqShovel.ps1'
}

Describe 'Configure-RabbitMqShovel — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:ConfigureScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'rejects an invalid -AckMode via ValidateSet' {
        { & $script:ConfigureScript -AckMode 'sometimes' -LoadFunctionsOnly } | Should -Throw
    }

    It 'defines its helper functions under -LoadFunctionsOnly without running a phase' {
        $out = (. $script:ConfigureScript -LoadFunctionsOnly) 6>&1 | Out-String
        $out | Should -Not -Match 'Phase 0  Pre-flight'
        Get-Command Format-Amqp091Uri         -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Format-ServiceBusAmqp10Uri -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Invoke-RabbitMqApi         -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Save-ConfigureSummary      -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'defaults VHost, ShovelName, ServiceBusQueueName and ServiceBusSasKeyName on the parameter block' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:ConfigureScript, [ref]$null, [ref]$null)
        $params = $ast.ParamBlock.Parameters
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'VHost' }).DefaultValue.Value               | Should -Be '/'
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'ShovelName' }).DefaultValue.Value           | Should -Be 'wwexecution-shovel'
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'ServiceBusQueueName' }).DefaultValue.Value  | Should -Be 'wwexecution-queue'
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'ServiceBusSasKeyName' }).DefaultValue.Value | Should -Be 'shovel-send'
    }
}

Describe 'Configure-RabbitMqShovel — AMQP URI builders' {

    BeforeAll {
        . $script:ConfigureScript -LoadFunctionsOnly
    }

    Context 'Format-Amqp091Uri' {
        It 'represents the default vhost "/" as an EMPTY path (no trailing slash), per the RabbitMQ URI spec' {
            Format-Amqp091Uri -HostName 'localhost' -Port 5672 -User 'test' -Password 'test' -VHostName '/' |
                Should -Be 'amqp://test:test@localhost:5672'
        }

        It 'percent-encodes a non-default vhost as the path segment' {
            Format-Amqp091Uri -HostName 'broker.internal' -Port 5672 -User 'app' -Password 'p@ss' -VHostName 'orders/vh' |
                Should -Be 'amqp://app:p%40ss@broker.internal:5672/orders%2Fvh'
        }

        It 'percent-encodes special characters in user/password' {
            $uri = Format-Amqp091Uri -HostName 'localhost' -Port 5672 -User 'us er' -Password 'p@ss:word' -VHostName '/'
            $uri | Should -Be 'amqp://us%20er:p%40ss%3Aword@localhost:5672'
        }
    }

    Context 'Format-ServiceBusAmqp10Uri' {
        It 'builds the exact Microsoft-documented Service Bus AMQP 1.0 destination shape (sasl=plain, no queue name in the URI)' {
            Format-ServiceBusAmqp10Uri -Namespace 'ns-wwsb-test' -PolicyName 'shovel-send' -Key 'abc123==' |
                Should -Be 'amqps://shovel-send:abc123%3D%3D@ns-wwsb-test.servicebus.windows.net:5671/?sasl=plain'
        }

        It 'never embeds the destination queue name (it belongs in dest-address, not the URI)' {
            Format-ServiceBusAmqp10Uri -Namespace 'ns-wwsb-test' -PolicyName 'shovel-send' -Key 'abc123' |
                Should -Not -Match 'wwexecution-queue'
        }
    }

    Context 'Get-MaskedUri' {
        It 'masks the userinfo portion of a URI, leaving the host/scheme visible' {
            Get-MaskedUri 'amqps://shovel-send:supersecretkey@ns.servicebus.windows.net:5671/?sasl=plain' |
                Should -Be 'amqps://***:***@ns.servicebus.windows.net:5671/?sasl=plain'
        }
    }

    Context 'ConvertFrom-SecureStringPlain' {
        It 'round-trips a SecureString back to plaintext' {
            $secure = ConvertTo-SecureString 'p@ssW0rd' -AsPlainText -Force
            ConvertFrom-SecureStringPlain $secure | Should -Be 'p@ssW0rd'
        }
        It 'returns empty string for a null/empty SecureString' {
            ConvertFrom-SecureStringPlain $null | Should -Be ''
        }
    }
}

Describe 'Configure-RabbitMqShovel — end-to-end (DryRun, no broker mutation)' {

    BeforeEach {
        $global:sourceQueueExists = $true
        $global:nodesHavePlugin   = $true

        function Invoke-RestMethod {
            param($Method, $Uri, $Headers, $ContentType, $TimeoutSec, $Body)
            if ($Uri -match '/api/overview$') {
                return [pscustomobject]@{ rabbitmq_version = '3.12.0'; management_version = '3.12.0' }
            }
            if ($Uri -match '/api/nodes$') {
                if ($global:nodesHavePlugin) {
                    return @([pscustomobject]@{ enabled_plugins = @('rabbitmq_shovel', 'rabbitmq_shovel_management') })
                }
                return @([pscustomobject]@{ name = 'rabbit@node1' })
            }
            if ($Uri -match '/api/queues/') {
                if ($global:sourceQueueExists) { return [pscustomobject]@{ name = 'orders-trigger-queue' } }
                throw 'Object Not Found'
            }
            throw "Unhandled Uri in test shim: $Uri"
        }

        $global:logDir = Join-Path ([System.IO.Path]::GetTempPath()) ("wwshovellog-" + [guid]::NewGuid())

        $script:commonArgs = @{
            RabbitMqManagementUri = 'http://localhost:15672'
            RabbitMqUsername      = 'test'
            RabbitMqPassword      = (ConvertTo-SecureString 'test' -AsPlainText -Force)
            SourceQueue            = 'orders-trigger-queue'
            SourceHost             = 'localhost'
            ServiceBusNamespace    = 'ns-wwsb-test'
            ServiceBusSasKey       = (ConvertTo-SecureString 'fake-sas-key-0123456789' -AsPlainText -Force)
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
        Remove-Variable -Name sourceQueueExists, nodesHavePlugin, logDir -Scope Global -ErrorAction SilentlyContinue
    }

    It 'runs every phase to completion' {
        $out = (& $script:ConfigureScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Phase 0  Pre-flight'
        $out | Should -Match 'Phase 0.5  Plan'
        $out | Should -Match 'Phase 1  Verify source queue'
        $out | Should -Match 'Phase 2  Configure dynamic shovel'
        $out | Should -Match 'Phase 3  Verify shovel status'
        $out | Should -Match 'Dry-run complete'
    }

    It 'verifies the source queue exists before configuring the shovel' {
        $out = (& $script:ConfigureScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match "Source queue 'orders-trigger-queue' exists"
    }

    It 'throws a clear error when the source queue does not exist' {
        $global:sourceQueueExists = $false
        { & $script:ConfigureScript @commonArgs } | Should -Throw '*was not found*'
    }

    It 'echoes the PUT to /api/parameters/shovel with the correct default shovel fields' {
        $out = (& $script:ConfigureScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match '\[DRYRUN\] PUT .*/api/parameters/shovel/'
        $out | Should -Match '"src-protocol":\s*"amqp091"'
        $out | Should -Match '"dest-protocol":\s*"amqp10"'
        $out | Should -Match '"dest-address":\s*"wwexecution-queue"'
        $out | Should -Match '"ack-mode":\s*"on-confirm"'
        $out | Should -Match '"reconnect-delay":\s*5'
        $out | Should -Match '"src-prefetch-count":\s*5'
    }

    It 'honours custom -AckMode / -ReconnectDelaySeconds / -SrcPrefetchCount' {
        $a = $script:commonArgs.Clone(); $a.AckMode = 'on-publish'; $a.ReconnectDelaySeconds = 10; $a.SrcPrefetchCount = 20
        $out = (& $script:ConfigureScript @a) 6>&1 | Out-String
        $out | Should -Match '"ack-mode":\s*"on-publish"'
        $out | Should -Match '"reconnect-delay":\s*10'
        $out | Should -Match '"src-prefetch-count":\s*20'
    }

    It 'never prints the plaintext RabbitMQ password, source password, or Service Bus SAS key' {
        $out = (& $script:ConfigureScript @commonArgs) 6>&1 | Out-String
        $out | Should -Not -Match 'fake-sas-key-0123456789'
        $out | Should -Match '\*\*\*:\*\*\*@'  # masked userinfo in both source + dest URIs
    }

    It 'warns (but does not fail) when the shovel plugin cannot be confirmed from /api/nodes' {
        $global:nodesHavePlugin = $false
        $out = (& $script:ConfigureScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Could not confirm rabbitmq_shovel is enabled'
        $out | Should -Match 'Dry-run complete'
    }

    It 'throws when both -ServiceBusSasKey and -ServiceBusResourceGroup are supplied' {
        $a = $script:commonArgs.Clone(); $a.ServiceBusResourceGroup = 'rg-test-shovel'
        { & $script:ConfigureScript @a } | Should -Throw '*Supply either*'
    }

    It 'writes a dry-run summary with masked URIs and no plaintext secrets' {
        & $script:ConfigureScript @commonArgs 6>&1 | Out-Null
        $summaryFile = Get-ChildItem -LiteralPath $global:logDir -Filter 'configure-rabbitmq-shovel-*.dryrun.summary.json' |
            Sort-Object LastWriteTime | Select-Object -Last 1
        $summaryFile | Should -Not -BeNullOrEmpty
        $summary = Get-Content $summaryFile.FullName -Raw | ConvertFrom-Json
        $summary.dryRun | Should -BeTrue
        $summary.status | Should -Be 'completed'
        $summary.sourceQueue | Should -Be 'orders-trigger-queue'
        $summary.serviceBusQueueName | Should -Be 'wwexecution-queue'
        $summary.destUri | Should -Match '^amqps://\*\*\*:\*\*\*@'
        (Get-Content $summaryFile.FullName -Raw) | Should -Not -Match 'fake-sas-key-0123456789'
    }

    # ── Fail-loud validation (reuses the BeforeEach shim + files) ───────────────

    It 'throws when -SourceQueue is missing (NonInteractive)' {
        $a = $script:commonArgs.Clone(); $a.Remove('SourceQueue')
        { & $script:ConfigureScript @a } | Should -Throw '*SourceQueue*'
    }

    It 'throws when -ServiceBusNamespace is missing (NonInteractive)' {
        $a = $script:commonArgs.Clone(); $a.Remove('ServiceBusNamespace')
        { & $script:ConfigureScript @a } | Should -Throw '*ServiceBusNamespace*'
    }

    It 'defaults -SourceUsername/-SourcePassword to the RabbitMQ management credentials when omitted' {
        $out = (& $script:ConfigureScript @commonArgs) 6>&1 | Out-String
        $out | Should -Match 'Source URI\s*: amqp://\*\*\*:\*\*\*@localhost:5672'
    }
}
