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
        # Successive message_stats.publish values, indexed alongside depthSequence.
        # $null (the default) omits message_stats entirely, as a broker with no
        # traffic since its last stats reset does.
        $global:publishedSequence = $null
        $global:queueIndex    = 0
        # Length cap, reported as policy (effective_policy_definition.max-length)
        # or as a queue argument (arguments.x-max-length). $null = uncapped.
        $global:policyMaxLength   = $null
        $global:argumentMaxLength = $null
        # $true sends every absent optional object as an empty {} instead of
        # omitting it, as a real broker does for a queue with no arguments or
        # policy (build 30847).
        $global:emptyObjects  = $false
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
                $i = $global:queueIndex
                $global:queueIndex++
                $q = [ordered]@{ messages = $global:depthSequence[[Math]::Min($i, $global:depthSequence.Count - 1)] }
                if ($null -ne $global:publishedSequence) {
                    $q.message_stats = [pscustomobject]@{
                        publish = $global:publishedSequence[[Math]::Min($i, $global:publishedSequence.Count - 1)]
                    }
                }
                if ($null -ne $global:policyMaxLength) {
                    $q.effective_policy_definition = [pscustomobject]@{ 'max-length' = $global:policyMaxLength }
                }
                if ($null -ne $global:argumentMaxLength) {
                    $q.arguments = [pscustomobject]@{ 'x-max-length' = $global:argumentMaxLength }
                }
                if ($global:emptyObjects) {
                    foreach ($k in 'message_stats', 'effective_policy_definition', 'arguments') {
                        if (-not $q.Contains($k)) { $q[$k] = [pscustomobject]@{} }
                    }
                }
                return [pscustomobject]$q
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
        Remove-Variable -Name depthSequence, publishedSequence, queueIndex, policyMaxLength, `
            argumentMaxLength, headMessages, apiCalls, failOnGet `
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

        It 'treats a queue with empty arguments and policy objects as uncapped' {
            $global:depthSequence = @(100)
            $global:emptyObjects  = $true
            . $script:SinkScript @commonArgs -LoadFunctionsOnly
            $state = Get-QueueState -VHostName 'bmkzdabu' -Name 'q'
            $state.Depth     | Should -Be 100
            $state.MaxLength | Should -Be -1
            $state.Published | Should -Be -1
        }

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

    Context 'capped queue — the publish counter (builds 30775 / 30778 / 30797)' {
        # The sink has a CloudAMQP policy of max-length 20000. Once full, each
        # publish drops a message off the head and the depth never moves — the
        # three builds above all read "depth 20000 before the run, 20000 now" over
        # a healthy engine. These cases pin the depth AT the cap throughout.

        BeforeEach {
            $global:depthSequence   = @(20000)
            $global:policyMaxLength = 20000
        }

        It 'confirms delivery from the publish counter while the depth stays at the cap' {
            $global:publishedSequence = @(500001)
            $r = & $script:SinkScript @commonArgs -DepthBefore 20000 -PublishedBefore 500000 -TimeoutSeconds 0
            $r.Delivered      | Should -BeTrue
            $r.Signal         | Should -Be 'PublishCounter'
            $r.DepthAfter     | Should -Be 20000
            $r.PublishedAfter | Should -Be 500001
        }

        It 'keeps polling while the counter lags the stats collection interval' {
            $global:publishedSequence = @(500000, 500000, 500001)
            $r = & $script:SinkScript @commonArgs -DepthBefore 20000 -PublishedBefore 500000 -TimeoutSeconds 30
            $r.Delivered | Should -BeTrue
            $r.Polls     | Should -Be 3
        }

        It 'still accepts a depth rise when the counter has not caught up yet' {
            $global:policyMaxLength   = $null
            $global:depthSequence     = @(101)
            $global:publishedSequence = @(500000)
            $r = & $script:SinkScript @commonArgs -DepthBefore 100 -PublishedBefore 500000 -TimeoutSeconds 0
            $r.Delivered | Should -BeTrue
            $r.Signal    | Should -Be 'Depth'
        }

        It 'blames the publish, naming both counters, when the counter does not move' {
            $global:publishedSequence = @(500000)
            { & $script:SinkScript @commonArgs -DepthBefore 20000 -PublishedBefore 500000 -TimeoutSeconds 0 } |
                Should -Throw '*published counter 500000 before, 500000 now*trailing RabbitMQ Publish did not deliver*'
        }

        It 'falls back to depth when the broker stops reporting the counter' {
            $global:policyMaxLength = $null
            $global:depthSequence   = @(101)
            # publishedSequence stays $null: no message_stats on the after-run read.
            $r = & $script:SinkScript @commonArgs -DepthBefore 100 -PublishedBefore 500000 -TimeoutSeconds 0
            $r.Delivered      | Should -BeTrue
            $r.Signal         | Should -Be 'Depth'
            $r.PublishedAfter | Should -Be -1
        }

        It 'says the queue is full, rather than blaming the publish, when only depth is available' {
            $err = { & $script:SinkScript @commonArgs -DepthBefore 20000 -TimeoutSeconds 0 } |
                       Should -Throw -PassThru
            $err.Exception.Message | Should -Match 'max-length of 20000'
            $err.Exception.Message | Should -Match 'ReportBaselineOnly'
            $err.Exception.Message | Should -Not -Match 'did not deliver'
        }

        It 'recognises a cap set as an x-max-length queue argument, not only by policy' {
            $global:policyMaxLength   = $null
            $global:argumentMaxLength = 20000
            { & $script:SinkScript @commonArgs -DepthBefore 20000 -TimeoutSeconds 0 } |
                Should -Throw '*max-length of 20000*'
        }

        It 'reports a counter that went backwards as a stats reset, not a failed publish' {
            $global:publishedSequence = @(12)
            $err = { & $script:SinkScript @commonArgs -DepthBefore 20000 -PublishedBefore 500000 -TimeoutSeconds 0 } |
                       Should -Throw -PassThru
            $err.Exception.Message | Should -Match 'stats were reset'
            $err.Exception.Message | Should -Not -Match 'did not deliver'
        }
    }

    Context '-ReportBaselineOnly' {

        It 'returns depth and publish counter as the only object on the success stream' {
            $global:depthSequence     = @(20000)
            $global:publishedSequence = @(500000)
            $b = @(& $script:SinkScript @commonArgs -ReportBaselineOnly)
            $b.Count        | Should -Be 1
            $b[0].Depth     | Should -Be 20000
            $b[0].Published | Should -Be 500000
        }

        It 'returns Published -1 when the broker reports no message_stats' {
            $global:depthSequence = @(7)
            $b = & $script:SinkScript @commonArgs -ReportBaselineOnly
            $b.Depth     | Should -Be 7
            $b.Published | Should -Be -1
        }

        It 'reads an uncapped queue whose arguments and policy are empty objects (build 30847)' {
            $global:depthSequence     = @(12)
            $global:publishedSequence = @(900)
            $global:emptyObjects      = $true
            $b = & $script:SinkScript @commonArgs -ReportBaselineOnly
            $b.Depth     | Should -Be 12
            $b.Published | Should -Be 900
        }

        It 'returns Published -1 when message_stats is an empty object' {
            $global:depthSequence = @(7)
            $global:emptyObjects  = $true
            $b = & $script:SinkScript @commonArgs -ReportBaselineOnly
            $b.Depth     | Should -Be 7
            $b.Published | Should -Be -1
        }

        It 'reads the queue once and never peeks' {
            & $script:SinkScript @commonArgs -ReportBaselineOnly | Out-Null
            $global:apiCalls.Count | Should -Be 1
            ($global:apiCalls | Where-Object { $_.Uri -match '/get$' }).Count | Should -Be 0
        }
    }
}
