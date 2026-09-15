#Requires -Version 7.0
<#
    Pester 5 suite for the MANAGEMENT-HTTP queue-depth path in WwE2E.Common.psm1.

    WHY THIS PATH EXISTS AT ALL. Get-E2EQueueDepth reads depth over AMQP. On .NET 10,
    RabbitMQ.Client 7.0.0 stack-overflows inside its socket reader while the connection is torn
    down, which kills the HOST PROCESS outright - no catchable exception, no output flushed.
    Run-WwProfilerExactLoad.ps1 died at its baseline step for exactly this reason, before a single
    message was published. The HTTP path opens no AMQP connection, so nothing is torn down.

    Everything here is shimmed - no broker is contacted.
#>

BeforeAll {
    $script:ModulePath = Join-Path (Split-Path -Parent $PSScriptRoot) 'WwE2E.Common.psm1'
    Import-Module $script:ModulePath -Force
    $script:Cred = [pscredential]::new('u', (ConvertTo-SecureString 'p' -AsPlainText -Force))

    # Builds the shape Invoke-RestMethod would throw for a given HTTP status.
    function script:New-HttpError([int]$Status, [string]$Message = 'http error') {
        $ex = New-Object System.Exception $Message
        $ex | Add-Member -NotePropertyName Response `
                         -NotePropertyValue ([pscustomobject]@{ StatusCode = $Status }) -Force
        return $ex
    }
}

Describe 'Get-E2EBrokerContextFromUri' {

    It 'treats an empty AMQP path as the DEFAULT vhost' {
        # amqp://host:5672/  means the default vhost, NOT a vhost named ''.
        (Get-E2EBrokerContextFromUri -AmqpUri 'amqp://u:p@h:5672/').VHost | Should -Be '/'
    }

    It 'treats a named AMQP path as that vhost' {
        (Get-E2EBrokerContextFromUri -AmqpUri 'amqp://u:p@h:5672/bmkzdabu').VHost | Should -Be 'bmkzdabu'
    }

    It 'extracts the credential' {
        $ctx = Get-E2EBrokerContextFromUri -AmqpUri 'amqp://alice:s3cret@h:5672/'
        $ctx.UserName | Should -Be 'alice'
        $ctx.Credential.GetNetworkCredential().Password | Should -Be 's3cret'
    }

    It 'throws when the URI carries no credentials, rather than probing anonymously' {
        { Get-E2EBrokerContextFromUri -AmqpUri 'amqp://h:5672/' } | Should -Throw '*no credentials*'
    }
}

Describe 'Get-E2EQueueDepthHttp' {

    It 'maps messages_ready and consumers onto Messages/Consumers' {
        Mock Invoke-RestMethod -ModuleName WwE2E.Common {
            [pscustomobject]@{ messages_ready = 7; messages = 9; consumers = 2 }
        }
        $d = Get-E2EQueueDepthHttp -ManagementUri 'https://mq' -QueueName 'q' -VHost '/' -Credential $script:Cred
        $d.Exists    | Should -BeTrue
        $d.Queue     | Should -Be 'q'
        $d.Messages  | Should -Be 7
        $d.Consumers | Should -Be 2
    }

    It 'uses messages_ready, NOT messages' {
        # The AMQP passive declare reports READY messages only, excluding unacked in-flight work -
        # and that is also exactly what the KEDA rabbitmq scaler reads. Reporting 'messages' here
        # would silently change drain semantics: the loop would keep waiting on work already in
        # flight, or (worse, inverted) call a run finished while deliveries were still unacked.
        Mock Invoke-RestMethod -ModuleName WwE2E.Common {
            [pscustomobject]@{ messages_ready = 3; messages = 11; consumers = 1 }
        }
        $d = Get-E2EQueueDepthHttp -ManagementUri 'https://mq' -QueueName 'q' -VHost '/' -Credential $script:Cred
        $d.Messages | Should -Be 3
        $d.Messages | Should -Not -Be 11
    }

    It 'URL-encodes the default vhost as %2F' {
        # A bare '/' in the path splits the management route and 404s, which would look exactly like
        # a missing queue. This broker uses the default vhost, so this is the live case.
        $script:captured = $null
        Mock Invoke-RestMethod -ModuleName WwE2E.Common {
            $script:captured = $Uri
            [pscustomobject]@{ messages_ready = 0; consumers = 0 }
        }
        Get-E2EQueueDepthHttp -ManagementUri 'https://mq' -QueueName 'profiler.profile.request' `
                              -VHost '/' -Credential $script:Cred | Out-Null
        $script:captured | Should -BeLike '*/api/queues/%2F/profiler.profile.request'
        $script:captured | Should -Not -BeLike '*/api/queues///*'
    }

    It 'URL-encodes a named vhost too' {
        $script:captured = $null
        Mock Invoke-RestMethod -ModuleName WwE2E.Common {
            $script:captured = $Uri
            [pscustomobject]@{ messages_ready = 0; consumers = 0 }
        }
        Get-E2EQueueDepthHttp -ManagementUri 'https://mq/' -QueueName 'q' -VHost 'bmkzdabu' -Credential $script:Cred | Out-Null
        $script:captured | Should -Be 'https://mq/api/queues/bmkzdabu/q'
    }

    It 'returns Exists=$false for a 404, mirroring a failed passive declare' {
        Mock Invoke-RestMethod -ModuleName WwE2E.Common { throw (script:New-HttpError 404 'not found') }
        $d = Get-E2EQueueDepthHttp -ManagementUri 'https://mq' -QueueName 'nope' -VHost '/' -Credential $script:Cred
        $d.Exists   | Should -BeFalse
        $d.Messages | Should -Be 0
    }

    It 'THROWS on a non-404 failure instead of reporting depth 0' {
        # The dangerous failure: a probe that swallowed a 500 and returned 0 would make the drain
        # loop conclude the queue had emptied and report a clean run - the same invisible-success
        # shape as a dead-lettered message draining the queue.
        Mock Invoke-RestMethod -ModuleName WwE2E.Common { throw (script:New-HttpError 500 'boom') }
        { Get-E2EQueueDepthHttp -ManagementUri 'https://mq' -QueueName 'q' -VHost '/' -Credential $script:Cred } |
            Should -Throw '*Queue-depth probe failed*'
    }

    It 'THROWS when the failure carries no HTTP status at all (timeout, DNS)' {
        Mock Invoke-RestMethod -ModuleName WwE2E.Common { throw (New-Object System.Exception 'timed out') }
        { Get-E2EQueueDepthHttp -ManagementUri 'https://mq' -QueueName 'q' -VHost '/' -Credential $script:Cred } |
            Should -Throw '*Queue-depth probe failed*'
    }
}

# Run-WwProfilerExactLoad.ps1 is a profiler load-test harness that is not part of the repo's
# script set - it lives alongside this suite only on an operator machine. These tests assert how
# that harness wires itself to WwE2E.Common, so they are skipped rather than failed where it is
# absent: a missing optional harness is not a defect in the module under test.
Describe 'Run-WwProfilerExactLoad - management URI wiring' -Skip:(-not (Test-Path -LiteralPath (Join-Path (Split-Path -Parent $PSScriptRoot) 'Run-WwProfilerExactLoad.ps1'))) {

    BeforeAll {
        $script:Harness = Join-Path (Split-Path -Parent $PSScriptRoot) 'Run-WwProfilerExactLoad.ps1'
        $script:Ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:Harness, [ref]$null, [ref]$null)
        $script:Text = Get-Content -LiteralPath $script:Harness -Raw
    }

    It 'exposes -RabbitMqManagementUri' {
        $p = $script:Ast.ParamBlock.Parameters |
             Where-Object { $_.Name.VariablePath.UserPath -eq 'RabbitMqManagementUri' }
        $p                 | Should -Not -BeNullOrEmpty
        $p.StaticType.Name | Should -Be 'String'
    }

    It 'fails fast with an actionable message when it is not supplied' {
        $script:Text | Should -Match '-RabbitMqManagementUri is required'
    }

    It 'reads depth over HTTP, not AMQP' {
        $script:Text | Should -Match 'Get-E2EQueueDepthHttp'
    }

    It 'opens no AMQP session outside the opt-in -PeekDeadLetters branch' {
        # Every AMQP teardown crashes the host on .NET 10, so a session anywhere on the main path
        # kills the run. -PeekDeadLetters is opt-in and documented as risky, so it keeps its channel.
        $sessions = [regex]::Matches($script:Text, 'New-E2EBrokerSession')
        $sessions.Count | Should -Be 1
        $script:Text | Should -Match 'wwqp-dlq-'
    }
}


Describe 'Get-E2EEngineToken - separating the caller app from the resource app' {
    <#
        -ClientId was added so a DEDICATED DAEMON app can request a token FOR the engine, rather
        than the engine only ever authenticating as itself. pipeline-LOADTEST.yml needs exactly
        that shape: the Warewolf-ShovelBridgeE2E-Daemon registration is the client, while the
        audience must stay the engine's own app id, because that is what the engine validates.

        Every existing caller passes no -ClientId and must keep the original behaviour
        (client_id == EngineAppId), so that case is pinned here too.
    #>

    BeforeAll {
        $script:ModulePath = Join-Path (Split-Path -Parent $PSScriptRoot) 'WwE2E.Common.psm1'
        Import-Module $script:ModulePath -Force
    }

    It 'posts the distinct -ClientId while keeping the ENGINE app id as the audience' {
        InModuleScope -ModuleName 'WwE2E.Common' {
            Mock Invoke-RestMethod { @{ access_token = 'token-for-daemon' } }

            $token = Get-E2EEngineToken `
                -TenantId 'ca0cc53b-9af4-4067-bcdf-be9c648450d1' `
                -ClientId 'dc1182bc-ffc1-4a1d-a414-ab672998eb9a' `
                -EngineAppId 'e200900a-2d5d-4356-94c0-cd7e33232ce0' `
                -ClientSecret 'daemon-secret'

            $token | Should -Be 'token-for-daemon'
            Should -Invoke Invoke-RestMethod -Times 1 -Exactly -ParameterFilter {
                $Body.client_id -eq 'dc1182bc-ffc1-4a1d-a414-ab672998eb9a' -and
                $Body.client_secret -eq 'daemon-secret' -and
                $Body.scope -eq 'api://e200900a-2d5d-4356-94c0-cd7e33232ce0/.default' -and
                $Body.grant_type -eq 'client_credentials'
            }
        }
    }

    It 'defaults client_id to -EngineAppId when -ClientId is omitted (original behaviour)' {
        InModuleScope -ModuleName 'WwE2E.Common' {
            Mock Invoke-RestMethod { @{ access_token = 'token-for-engine' } }

            Get-E2EEngineToken -TenantId 'tenant' -EngineAppId 'engine-app' -ClientSecret 'engine-secret' |
                Should -Be 'token-for-engine'

            Should -Invoke Invoke-RestMethod -Times 1 -Exactly -ParameterFilter {
                $Body.client_id -eq 'engine-app' -and $Body.scope -eq 'api://engine-app/.default'
            }
        }
    }

    It 'refuses to read the app''s Easy Auth secret for a DIFFERENT client app' {
        # MICROSOFT_PROVIDER_AUTHENTICATION_SECRET belongs to the engine app, so using it to
        # authenticate a separate client would post one app's id with another app's secret and
        # fail at Entra with an opaque AADSTS7000215. Fail here instead, naming the cause.
        InModuleScope -ModuleName 'WwE2E.Common' {
            Mock Invoke-E2EAzJson { @() }
            Mock Invoke-RestMethod { throw 'must never be reached' }

            { Get-E2EEngineToken -TenantId 'tenant' -ClientId 'other-client' -EngineAppId 'engine-app' `
                -FunctionAppName 'app' -ResourceGroup 'rg' } |
                Should -Throw -ExpectedMessage '*cannot authenticate a separate client app*'

            Should -Invoke Invoke-RestMethod -Times 0 -Exactly
        }
    }
}
