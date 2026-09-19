#Requires -Version 7.0

<#
    Pester 5 test suite for Test-WwShovelBridgeDelivery.ps1.

    Run:
        Import-Module Pester -RequiredVersion 5.7.1
        Invoke-Pester -Path ./Tests/Test-WwShovelBridgeDelivery.Tests.ps1

    Design notes (same conventions as Test-WwSinkDelivery.Tests.ps1)
    -------------------------------------------------------------------------
    * -LoadFunctionsOnly dot-sources the helpers with no broker or az action, so
      the three pure helpers (body format, publish request, verdict) are unit-
      tested directly rather than through a run.
    * `Invoke-RestMethod` AND `az` are both shadowed with tracking *functions*.
      Functions resolve before cmdlets and before applications in PowerShell's
      command lookup, and the shims are reachable from the externally-invoked
      script because `&` runs it in a child scope of the caller.
    * $global:apiCalls records every shimmed request, which is what lets the
      suite assert on calls that must NOT happen (nothing at all when the
      password is empty) and on the exact publish body.
    * $global:sourceDepths / $global:sbCounts drive successive polls, so
      "drains on the third read" needs no real waiting (-PollIntervalSeconds 0).
    * The verdict cases are the ones that matter: the 2026-09-18 wwprofilerlw
      failure (shovel never consumed) and the WOLF-8517 signature (bridge
      healthy, every message dead-lettered) must fail DIFFERENTLY and name
      different legs, or the pipeline log sends the next investigation to the
      wrong side of the bridge.
#>

BeforeAll {
    $script:BridgeScript = Join-Path (Split-Path $PSScriptRoot -Parent) 'Test-WwShovelBridgeDelivery.ps1'

    $script:CanonicalFields = @{
        ForPaymentCycle       = '202402'
        MandateCanTrack       = 'false'
        MandateDRI            = '22'
        MandateId             = '109800B6-2183-EB11-A812-000D3ABE1AD1'
        RelatedMandates       = '5B9800B6-2183-EB11-A812-000D3ABE1AD1'
        ResultDate            = '2024-01-27'
        ResultDebitRunId      = '22'
        ResultMethod          = 'DebiCheck'
        ResultPaymentCycle    = '202401'
        ResultPresentedOnDate = '2024-01-22'
        ResultType            = 'Success'
        ScoreThisResult       = 'true'
        IsFixedExtract        = 'false'
        SourcePaymentId       = '1234'
        RequestType           = ''
        FASGroup              = ''
    }
}

Describe 'Test-WwShovelBridgeDelivery — static' {

    It 'parses without syntax errors' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($script:BridgeScript, [ref]$null, [ref]$parseErrors) | Out-Null
        $parseErrors | Should -BeNullOrEmpty
    }

    It 'defines its helper functions under -LoadFunctionsOnly without contacting a broker' {
        . $script:BridgeScript -LoadFunctionsOnly
        Get-Command Format-WwBridgeMessageBody  -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command New-WwBridgePublishRequest  -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-WwBridgeVerdict         -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Publish-BridgeMessage       -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-ServiceBusQueueCounts   -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Get-QueueDepth              -CommandType Function -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }

    It 'defaults the two windows and VHost on the parameter block' {
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($script:BridgeScript, [ref]$null, [ref]$null)
        $params = $ast.ParamBlock.Parameters
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'BridgeTimeoutSeconds' }).DefaultValue.Value    | Should -Be 120
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'ExecutionTimeoutSeconds' }).DefaultValue.Value | Should -Be 300
        ($params | Where-Object { $_.Name.VariablePath.UserPath -eq 'VHost' }).DefaultValue.Value                   | Should -Be '/'
    }
}

Describe 'Test-WwShovelBridgeDelivery — message body' {

    BeforeAll { . $script:BridgeScript -LoadFunctionsOnly }

    It 'wraps the fields in the post-WOLF-8517 {"inputs":{...}} envelope' {
        $json = Format-WwBridgeMessageBody -Fields @{ MandateId = 'abc'; MandateDRI = '22' }
        $parsed = $json | ConvertFrom-Json
        $parsed.PSObject.Properties.Name | Should -Be 'inputs'
        $parsed.inputs.MandateId  | Should -Be 'abc'
        $parsed.inputs.MandateDRI | Should -Be '22'
    }

    It 'never emits a flat body, which binds as Inputs=null against an empty datalist' {
        $json = Format-WwBridgeMessageBody -Fields @{ MandateId = 'abc' }
        $json | Should -Not -Match '^\{"MandateId"'
    }

    It 'keeps legitimately empty fields rather than dropping them' {
        # 'RequestType' and 'FASGroup' are empty in the canonical request; a body
        # without them is not the shape the real publisher sends.
        $json = Format-WwBridgeMessageBody -Fields $script:CanonicalFields
        $parsed = ($json | ConvertFrom-Json).inputs
        $parsed.PSObject.Properties.Name.Count | Should -Be 16
        $parsed.RequestType | Should -Be ''
        $parsed.FASGroup    | Should -Be ''
    }

    It 'is byte-stable regardless of the hashtable enumeration order' {
        $a = Format-WwBridgeMessageBody -Fields @{ b = '2'; a = '1'; c = '3' }
        $b = Format-WwBridgeMessageBody -Fields @{ c = '3'; a = '1'; b = '2' }
        $a | Should -Be $b
    }
}

Describe 'Test-WwShovelBridgeDelivery — publish request' {

    BeforeAll { . $script:BridgeScript -LoadFunctionsOnly }

    It 'routes through the default exchange with the queue as the routing key' {
        $r = New-WwBridgePublishRequest -RoutingKey 'wwprofilerlw.servicebus.trigger.request' -Body '{}'
        $r.routing_key      | Should -Be 'wwprofilerlw.servicebus.trigger.request'
        $r.payload_encoding | Should -Be 'string'
        $r.payload          | Should -Be '{}'
    }

    It 'publishes persistently, so a broker restart cannot look like a bridge failure' {
        $r = New-WwBridgePublishRequest -RoutingKey 'q' -Body '{}'
        $r.properties.delivery_mode | Should -Be 2
    }
}

Describe 'Test-WwShovelBridgeDelivery — verdict' {

    BeforeAll { . $script:BridgeScript -LoadFunctionsOnly }

    It 'keeps waiting while the message is still on the source queue' {
        $v = Get-WwBridgeVerdict -SourceDepth 1 -SourceBaseline 0 -DeadLetterBefore 0 -DeadLetterNow 0 -ActiveNow 0
        $v.Done  | Should -BeFalse
        $v.Stage | Should -Be 'Shovel'
    }

    It 'fails at the Shovel leg when the source queue never drains — the 2026-09-18 wwprofilerlw failure' {
        $v = Get-WwBridgeVerdict -SourceDepth 1 -SourceBaseline 0 -DeadLetterBefore 0 -DeadLetterNow 0 -ActiveNow 0 -BridgeWindowExpired
        $v.Done   | Should -BeTrue
        $v.Failed | Should -BeTrue
        $v.Stage  | Should -Be 'Shovel'
        # The diagnosis has to point at the src-uri, or the next investigation
        # starts on the Azure side of a bridge whose RabbitMQ side never connected.
        $v.Reason | Should -Match 'src-uri'
        $v.Reason | Should -Match 'loopback'
    }

    It 'fails at the Engine leg when the dead-letter count rises — the WOLF-8517 signature' {
        # Both counts also satisfy "drained": the source queue is empty and
        # activeMessageCount is 0. Judged on drain alone this reads as a pass,
        # which is exactly how 5000/5000 dead-lettered messages once looked clean.
        $v = Get-WwBridgeVerdict -SourceDepth 0 -SourceBaseline 0 -DeadLetterBefore 0 -DeadLetterNow 1 -ActiveNow 0
        $v.Failed | Should -BeTrue
        $v.Stage  | Should -Be 'Engine'
        $v.Reason | Should -Match 'DEAD-LETTERED'
    }

    It 'reports the dead-letter before the drain, whatever the source queue says' {
        $v = Get-WwBridgeVerdict -SourceDepth 1 -SourceBaseline 0 -DeadLetterBefore 3 -DeadLetterNow 4 -ActiveNow 0 -BridgeWindowExpired
        $v.Stage | Should -Be 'Engine'
    }

    It 'waits for the trigger while the message sits active on Service Bus' {
        $v = Get-WwBridgeVerdict -SourceDepth 0 -SourceBaseline 0 -DeadLetterBefore 0 -DeadLetterNow 0 -ActiveNow 1
        $v.Done  | Should -BeFalse
        $v.Stage | Should -Be 'Engine'
    }

    It 'fails when the trigger never settles the delivery' {
        $v = Get-WwBridgeVerdict -SourceDepth 0 -SourceBaseline 0 -DeadLetterBefore 0 -DeadLetterNow 0 -ActiveNow 1 -ExecutionWindowExpired
        $v.Failed | Should -BeTrue
        $v.Stage  | Should -Be 'Engine'
        $v.Reason | Should -Match 'never settled'
    }

    It 'passes only when the source drained, nothing dead-lettered and nothing is left active' {
        $v = Get-WwBridgeVerdict -SourceDepth 0 -SourceBaseline 0 -DeadLetterBefore 0 -DeadLetterNow 0 -ActiveNow 0
        $v.Done   | Should -BeTrue
        $v.Failed | Should -BeFalse
        $v.Stage  | Should -Be 'Complete'
    }

    It 'stops at the Shovel leg, and says so, when Service Bus was not inspected' {
        $v = Get-WwBridgeVerdict -SourceDepth 0 -SourceBaseline 0
        $v.Done   | Should -BeTrue
        $v.Failed | Should -BeFalse
        $v.Stage  | Should -Be 'Shovel'
        $v.Reason | Should -Match 'not inspected'
    }

    It 'measures the drain against the BASELINE, not against zero' {
        # A residual backlog on the source queue must not read as "our message
        # is still queued" forever.
        $v = Get-WwBridgeVerdict -SourceDepth 7 -SourceBaseline 7 -DeadLetterBefore 0 -DeadLetterNow 0 -ActiveNow 0
        $v.Done  | Should -BeTrue
        $v.Stage | Should -Be 'Complete'
    }
}

Describe 'Test-WwShovelBridgeDelivery — end to end, shimmed' {

    BeforeEach {
        $global:sourceDepths = @(0)
        $global:depthIndex   = 0
        $global:sbCounts     = @(@{ active = 0; dlq = 0 })
        $global:sbIndex      = 0
        $global:routed       = $true
        $global:apiCalls     = [System.Collections.ArrayList]::new()
        $global:azCalls      = [System.Collections.ArrayList]::new()
        $global:LASTEXITCODE = 0

        function Invoke-RestMethod {
            param($Method, $Uri, $Headers, $ContentType, $TimeoutSec, $Body)
            [void] $global:apiCalls.Add([pscustomobject]@{ Method = $Method; Uri = $Uri; Body = $Body })

            if ($Uri -match '/publish$') { return [pscustomobject]@{ routed = $global:routed } }
            if ($Uri -match '/api/queues/') {
                $i = [Math]::Min($global:depthIndex, $global:sourceDepths.Count - 1)
                $global:depthIndex++
                return [pscustomobject]@{ messages = $global:sourceDepths[$i] }
            }
            throw "Unhandled Uri in test shim: $Uri"
        }

        function az {
            [void] $global:azCalls.Add(($args -join ' '))
            $i = [Math]::Min($global:sbIndex, $global:sbCounts.Count - 1)
            $global:sbIndex++
            $global:LASTEXITCODE = 0
            return (@{
                countDetails = @{
                    activeMessageCount     = $global:sbCounts[$i].active
                    deadLetterMessageCount = $global:sbCounts[$i].dlq
                }
            } | ConvertTo-Json -Depth 5)
        }

        $script:commonArgs = @{
            ManagementUri       = 'https://rabbitmq.example'
            SourceQueue         = 'wwprofilerlw.servicebus.trigger.request'
            VHost               = '/'
            Username            = 'testuser'
            Password            = (ConvertTo-SecureString 'test' -AsPlainText -Force)
            InputFields         = $script:CanonicalFields
            PollIntervalSeconds = 0
        }
        $script:sbArgs = @{
            ServiceBusNamespace = 'wwprofilerlwsb'
            ServiceBusQueueName = 'wwprofilerlw-trigger-queue'
            ResourceGroup       = 'DEV2'
        }
    }

    AfterAll {
        Remove-Variable -Name sourceDepths, depthIndex, sbCounts, sbIndex, routed, apiCalls, azCalls `
            -Scope Global -ErrorAction SilentlyContinue
    }

    It 'passes when the shovel consumes and the trigger settles the delivery' {
        $global:sourceDepths = @(1, 0)
        $global:sbCounts     = @(@{ active = 0; dlq = 0 }, @{ active = 1; dlq = 0 }, @{ active = 0; dlq = 0 })
        $r = & $script:BridgeScript @commonArgs @sbArgs -CorrelationToken 'abc'
        $r.Delivered         | Should -BeTrue
        $r.Stage             | Should -Be 'Complete'
        $r.ServiceBusChecked | Should -BeTrue
    }

    It 'publishes exactly one message, to the default exchange, in the inputs envelope' {
        & $script:BridgeScript @commonArgs @sbArgs | Out-Null
        $publishes = @($global:apiCalls | Where-Object { $_.Uri -match '/publish$' })
        $publishes.Count | Should -Be 1
        $publishes[0].Uri  | Should -Match '/api/exchanges/%2f/amq\.default/publish$'
        # The envelope is the PAYLOAD of the publish request, so it arrives
        # JSON-escaped inside it — assert the escaped spelling, or this passes
        # on a body that never carried the envelope at all.
        $publishes[0].Body | Should -Match '\\"inputs\\"'
        $publishes[0].Body | Should -Match '"delivery_mode":2'
    }

    It 'throws when the broker does not route the publish — a queue that is not there' {
        $global:routed = $false
        { & $script:BridgeScript @commonArgs @sbArgs } | Should -Throw '*did not route*'
    }

    It 'throws, naming the shovel, when the source queue never drains' {
        # Baseline 0, then the published message sits there for every poll —
        # the 2026-09-18 wwprofilerlw shape (shovel stuck 'starting', consumers 0).
        $global:sourceDepths = @(0, 1)
        { & $script:BridgeScript @commonArgs @sbArgs -BridgeTimeoutSeconds 0 } |
            Should -Throw '*Shovel leg*'
    }

    It 'throws, naming the engine, when the message is dead-lettered' {
        $global:sourceDepths = @(1, 0)
        $global:sbCounts     = @(@{ active = 0; dlq = 4 }, @{ active = 0; dlq = 5 })
        { & $script:BridgeScript @commonArgs @sbArgs } | Should -Throw '*Engine leg*'
    }

    It 'asserts only the shovel leg, and says so, when Service Bus is not supplied' {
        $global:sourceDepths = @(1, 0)
        $out = (& $script:BridgeScript @commonArgs) 6>&1
        $summary = $out | Where-Object { $_ -is [pscustomobject] -and $_.PSObject.Properties.Name -contains 'Delivered' }
        $summary.ServiceBusChecked | Should -BeFalse
        $global:azCalls.Count      | Should -Be 0
        ($out | Out-String)        | Should -Match 'Only the shovel leg was proven'
    }

    It 'reads Service Bus through az servicebus queue show, never through the SDK' {
        # Azure.Messaging.ServiceBus' PeekMessagesAsync access-violates (0xC0000005)
        # under pwsh 7.6/.NET 10 — the reason every diagnosis in this rig goes
        # through az or App Insights instead.
        & $script:BridgeScript @commonArgs @sbArgs | Out-Null
        $global:azCalls[0] | Should -Match 'servicebus queue show'
        $global:azCalls[0] | Should -Match 'wwprofilerlw-trigger-queue'
    }

    It 'fails before any broker call when the password is empty' {
        # NOT $args: that is an automatic variable, and splatting it passes the
        # enclosing scope's argument array rather than this hashtable.
        $noPassword = $script:commonArgs.Clone()
        $noPassword.Password = (New-Object System.Security.SecureString)
        { & $script:BridgeScript @noPassword } | Should -Throw '*password is empty*'
        $global:apiCalls.Count | Should -Be 0
    }

    It 'fails before any broker call when no workflow inputs are supplied' {
        $noInputs = $script:commonArgs.Clone()
        $noInputs.InputFields = @{}
        { & $script:BridgeScript @noInputs } | Should -Throw '*empty datalist*'
        $global:apiCalls.Count | Should -Be 0
    }
}
Describe 'Test-WwShovelBridgeDelivery — Get-ServiceBusQueueCounts (az stderr must not reach the parse)' {

    <#
        Same failure mode that killed build 30759 inside Enable-ServiceBusSecureTrigger.ps1: az
        writes warnings to stderr, '2>&1' merges them into the captured value as ErrorRecords,
        and one warning line ahead of the JSON makes ConvertFrom-Json fail with "Unexpected
        character encountered while parsing value: W". `az servicebus queue show` warns for its
        own reasons (extension notices, deprecations), and this read is what decides whether the
        bridge delivered - a parse failure here reports as a broken bridge.
    #>

    BeforeAll {
        . $script:BridgeScript -LoadFunctionsOnly

        function az {
            # -ErrorAction Continue, deliberately: a NATIVE command's stderr line is never a
            # terminating error no matter what $ErrorActionPreference says, but Write-Error
            # inside a function honours it - and this script leaves the preference at 'Stop'
            # around its az call. Without this the shim would throw where az merely warns.
            foreach ($line in $global:sbStderr) { Write-Error $line -ErrorAction Continue }
            if ($global:sbStdout) { Write-Output $global:sbStdout }
            $global:LASTEXITCODE = $global:sbExit
        }
    }

    BeforeEach {
        $global:sbStderr = @()
        $global:sbStdout = (@{ countDetails = @{ activeMessageCount = 3; deadLetterMessageCount = 1 } } | ConvertTo-Json -Depth 5)
        $global:sbExit   = 0
    }

    AfterAll {
        Remove-Variable -Name sbStderr, sbStdout, sbExit -Scope Global -ErrorAction SilentlyContinue
    }

    It 'parses the counts when az prefixes the JSON with a warning on stderr' {
        $global:sbStderr = @('WARNING: The behavior of this command has been altered by the following extension: servicebus')

        $counts = Get-ServiceBusQueueCounts -Namespace 'wwprofilerlwsb' -Queue 'wwprofilerlw-trigger-queue' -Group 'DEV2'

        $counts.Active     | Should -Be 3
        $counts.DeadLetter | Should -Be 1
    }

    It 'still quotes the stderr text when az exits non-zero' {
        $global:sbStderr = @('ERROR: (ResourceNotFound) The Messaging Entity could not be found.')
        $global:sbStdout = ''
        $global:sbExit   = 1

        { Get-ServiceBusQueueCounts -Namespace 'wwprofilerlwsb' -Queue 'nope' -Group 'DEV2' } |
            Should -Throw '*ResourceNotFound*'
    }
}
