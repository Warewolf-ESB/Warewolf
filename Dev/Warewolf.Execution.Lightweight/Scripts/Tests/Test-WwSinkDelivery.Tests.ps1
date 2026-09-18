#Requires -Version 7.0

<#
    Pester 5 test suite for Test-WwSinkDelivery.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Test-WwSinkDelivery.Tests.ps1

    Design notes (same conventions as Monitor-RabbitMqShovel.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no broker action — used to
      unit-test Get-VHostForApi directly.
    * `Invoke-RestMethod` is shadowed with a tracking *function* keyed off the
      request Uri/Method, so the suite runs with no real broker. Functions
      resolve before cmdlets in PowerShell's command lookup, and the shim is
      reachable from the externally-invoked script because `&` runs it in a child
      scope of the caller.
    * $global:apiCalls records every shimmed request, which is what lets the
      suite assert on calls that must NOT happen — the peek being skipped above
      the threshold, and every call being skipped when the password is empty.
    * $global:depthSequence drives Get-QueueDepth across successive polls, so a
      "depth rises on the third read" case needs no real waiting. Timeouts are
      passed as 0 where the test only needs a single read.
    * This is deliberately IN ADDITION TO, not a replacement for, validation
      against a live broker — the behaviour this script exists to fix was only
      visible against a 19,100-message CloudAMQP queue (build 30750).
#>

BeforeAll {
    $script:SinkScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Test-WwSinkDelivery.ps1'
}

Describe 'Test-WwSinkDelivery — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:SinkScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without contacting a broker' {
        . $script:SinkScript -LoadFunctionsOnly
        Get-Command Get-QueueDepth        -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-QueueHeadMessages -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Wait-ForSinkDelivery  -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Invoke-RabbitMqApi    -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-VHostForApi       -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'defaults PeekThreshold, TimeoutSeconds and VHost on the parameter block' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:SinkScript, [ref]$null, [ref]$null)
        $params = $ast.ParamBlock.Parameters
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'PeekThreshold' }).DefaultValue.Value  | Should -Be 200
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'TimeoutSeconds' }).DefaultValue.Value | Should -Be 90
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'VHost' }).DefaultValue.Value          | Should -Be '/'
    }
}

Describe 'Test-WwSinkDelivery — Get-VHostForApi' {

    BeforeAll { . $script:SinkScript -LoadFunctionsOnly }

    It "encodes the default vhost '/' as %2f rather than leaving it bare" {
        Get-VHostForApi '/' | Should -Be '%2f'
    }

    It 'escapes a named vhost' {
        Get-VHostForApi 'bmkzdabu'  | Should -Be 'bmkzdabu'
        Get-VHostForApi 'my vhost'  | Should -Be 'my%20vhost'
    }
}

Describe 'Test-WwSinkDelivery — delivery assertion' {

    BeforeEach {
        # Successive Get-QueueDepth results; the last value repeats once exhausted.
        $global:depthSequence = @(101)
        $global:depthIndex    = 0
        $global:headMessages  = @()
        $global:apiCalls      = [System.Collections.ArrayList]::new()
        $global:failOnGet     = $false

        function Invoke-RestMethod {
            param($Method, $Uri, $Headers, $ContentType, $TimeoutSec, $Body)
            [void] $global:apiCalls.Add([pscustomobject]@{ Method = $Method; Uri = $Uri; Body = $Body })

            if ($global:failOnGet) { throw 'Test shim: the broker is unreachable.' }

            if ($Uri -match '/get$') {
                return $global:headMessages
            }
            if ($Uri -match '/api/queues/') {
                $i = [Math]::Min($global:depthIndex, $global:depthSequence.Count - 1)
                $global:depthIndex++
                return [pscustomobject]@{ messages = $global:depthSequence[$i] }
            }
            throw "Unhandled Uri in test shim: $Uri"
        }

        $script:commonArgs = @{
            ManagementUri       = 'https://broker.example'
            QueueName           = 'collectionv2.profilerrecommendation.request'
            VHost               = 'bmkzdabu'
            Username            = 'test'
            Password            = (ConvertTo-SecureString 'test' -AsPlainText -Force)
            PollIntervalSeconds = 0
        }
    }

    AfterAll {
        Remove-Variable -Name depthSequence, depthIndex, headMessages, apiCalls, failOnGet `
            -Scope Global -ErrorAction SilentlyContinue
    }

    Context 'depth rises and the run is attributable' {

        It 'confirms delivery and attributes it when the token is in the head window' {
            $token = '0b3b071e-7f15-4ad5-a1f9-c7d310fdd669'
            $global:headMessages = @(
                [pscustomobject]@{ payload = '{"MandateId":"11111111-1111-1111-1111-111111111111"}' },
                [pscustomobject]@{ payload = "{`"MandateId`":`"$token`",`"CollectionMethod`":`"DebitOrder`"}" }
            )
            $r = & $script:SinkScript @commonArgs -DepthBefore 100 -CorrelationToken $token
            $r.Delivered    | Should -BeTrue
            $r.Attributable | Should -BeTrue
            $r.DepthBefore  | Should -Be 100
            $r.DepthAfter   | Should -Be 101
            $r.Peeked       | Should -Be 2
        }

        It 'peeks with the one ackmode that does not consume on LavinMQ' {
            # Load-bearing, and the reason this test exists: 'ack_requeue_true'
            # reads as though it requeues but CONSUMES on this broker (measured:
            # depth 3 -> 0 on a single peek, against 3 -> 3 for the mode below).
            # The original smoke test used it and was destroying up to 900 sink
            # messages per build while claiming it consumed nothing.
            $token = 'abc'
            $global:headMessages = @([pscustomobject]@{ payload = '{"MandateId":"abc"}' })
            & $script:SinkScript @commonArgs -DepthBefore 100 -CorrelationToken $token | Out-Null
            $peek = $global:apiCalls | Where-Object { $_.Uri -match '/get$' }
            $peek                     | Should -Not -BeNullOrEmpty
            $peek.Body                | Should -Match '"ackmode":\s*"reject_requeue_true"'
            $peek.Body                | Should -Not -Match 'ack_requeue_true'
        }

        It 'waits across polls until the depth actually rises' {
            $global:depthSequence = @(100, 100, 101)
            $r = & $script:SinkScript @commonArgs -DepthBefore 100 -TimeoutSeconds 30
            $r.Delivered | Should -BeTrue
            $r.Polls     | Should -Be 3
        }
    }

    Context 'depth rises but the run is not attributable' {

        It 'still confirms delivery when the token is absent from the head window' {
            $global:headMessages = @([pscustomobject]@{ payload = '{"MandateId":"other"}' })
            $out = (& $script:SinkScript @commonArgs -DepthBefore 100 -CorrelationToken 'missing-token') 6>&1
            $summary = $out | Where-Object { $_ -is [pscustomobject] -and $_.PSObject.Properties.Name -contains 'Delivered' }
            $summary.Delivered    | Should -BeTrue
            $summary.Attributable | Should -BeFalse
            ($out | Out-String)   | Should -Match 'not run-attributable'
        }

        It 'skips the payload peek entirely when the queue is deeper than -PeekThreshold' {
            $global:depthSequence = @(19101)
            $r = & $script:SinkScript @commonArgs -DepthBefore 19100 -CorrelationToken 'any' -PeekThreshold 200
            $r.Delivered    | Should -BeTrue
            $r.Attributable | Should -BeFalse
            $r.Peeked       | Should -Be 0
            # The whole point: no /get is issued, so the 18 pointless reads that
            # cost build 30750 its poll window cannot happen again.
            ($global:apiCalls | Where-Object { $_.Uri -match '/get$' }).Count | Should -Be 0
        }

        It 'asserts on the delta alone when no correlation token is supplied' {
            $r = & $script:SinkScript @commonArgs -DepthBefore 100
            $r.Delivered    | Should -BeTrue
            $r.Attributable | Should -BeFalse
            ($global:apiCalls | Where-Object { $_.Uri -match '/get$' }).Count | Should -Be 0
        }
    }

    Context 'failures' {

        It 'throws when the depth never rises within the window' {
            $global:depthSequence = @(100)
            { & $script:SinkScript @commonArgs -DepthBefore 100 -TimeoutSeconds 0 } |
                Should -Throw '*did not grow*'
        }

        It 'names the publish as the suspect when the depth never rises' {
            $global:depthSequence = @(100)
            { & $script:SinkScript @commonArgs -DepthBefore 100 -TimeoutSeconds 0 } |
                Should -Throw '*trailing RabbitMQ Publish did not deliver*'
        }

        It 'throws, rather than assuming 0, when no pre-run depth was captured' {
            { & $script:SinkScript @commonArgs } | Should -Throw '*pre-run sink depth*'
            # A broken pre-step must not be able to produce a silent pass.
            $global:apiCalls.Count | Should -Be 0
        }

        It 'throws before any HTTP call when the broker password is empty' {
            $a = $script:commonArgs.Clone()
            $a.Password = (New-Object System.Security.SecureString)
            { & $script:SinkScript @a -DepthBefore 100 } | Should -Throw '*password is empty*'
            $global:apiCalls.Count | Should -Be 0
        }

        It 'throws when the management API is unreachable on the after-run read' {
            $global:failOnGet = $true
            { & $script:SinkScript @commonArgs -DepthBefore 100 -TimeoutSeconds 0 } |
                Should -Throw '*could not reach the management API*'
        }

        It 'throws when the management API is unreachable on the before-run read' {
            $global:failOnGet = $true
            { & $script:SinkScript @commonArgs -ReportDepthOnly } |
                Should -Throw '*could not reach the management API*'
        }
    }

    Context '-ReportDepthOnly' {

        It 'returns the current depth as the only object on the success stream' {
            $global:depthSequence = @(19100)
            $d = & $script:SinkScript @commonArgs -ReportDepthOnly
            $d | Should -Be 19100
            $d | Should -BeOfType [int]
        }

        It 'reads the queue once and never peeks' {
            $global:depthSequence = @(42)
            & $script:SinkScript @commonArgs -ReportDepthOnly | Out-Null
            $global:apiCalls.Count | Should -Be 1
            ($global:apiCalls | Where-Object { $_.Uri -match '/get$' }).Count | Should -Be 0
        }

        It 'encodes the vhost into the queue URI' {
            & $script:SinkScript @commonArgs -ReportDepthOnly | Out-Null
            $global:apiCalls[0].Uri | Should -Match '/api/queues/bmkzdabu/collectionv2\.profilerrecommendation\.request$'
        }
    }
}
