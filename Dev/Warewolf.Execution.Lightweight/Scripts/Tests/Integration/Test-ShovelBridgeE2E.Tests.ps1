<#
    Unit coverage for Test-ShovelBridgeE2E.ps1's own pure helpers — dot-sourced via
    -LoadFunctionsOnly so this runs with NO docker, RabbitMQ, Service Bus, or SQL dependency
    (mirrors the convention already used by Configure-RabbitMqShovel.Tests.ps1 /
    Deploy-WwExecutionEngine.Tests.ps1 / Invoke-WwQueueLoadTest.Tests.ps1).

    Only the pure helpers are covered here (Get-WwJobsDbVerdict, Get-WwCorrelationGuidStem):
    the rest of the script is a live-infra integration harness (RabbitMQ + Shovel + Service Bus
    + the real engine), proven by actually running it, not by Pester — same as the script had
    no Pester coverage at all before this file.
#>

Describe 'Test-ShovelBridgeE2E — Get-WwJobsDbVerdict (pure, no DB)' {

    BeforeAll {
        $script:TargetScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
        . $script:TargetScript -LoadFunctionsOnly

        function script:New-SummaryRow {
            param(
                [int] $SucceededTotal = 0,
                [int] $DuplicateSuccess_ReliabilityBug = 0,
                [int] $StillInProcess_Incomplete = 0,
                [int] $StillInProcess_StuckPastThreshold = 0
            )
            [pscustomobject]@{
                TotalMessages                      = 0
                SucceededTotal                     = $SucceededTotal
                SucceededFirstTry                  = 0
                SucceededAfterRetry_CorrectBehavior = 0
                DuplicateSuccess_ReliabilityBug     = $DuplicateSuccess_ReliabilityBug
                FailedAllAttempts                  = 0
                StillInProcess_Incomplete          = $StillInProcess_Incomplete
                StillInProcess_StuckPastThreshold  = $StillInProcess_StuckPastThreshold
            }
        }
    }

    It 'reports IsClean when SucceededTotal matches expected, no duplicates, nothing missing' {
        $row     = New-SummaryRow -SucceededTotal 1000
        $verdict = Get-WwJobsDbVerdict -SummaryRow $row -ExpectedMessageCount 1000
        $verdict.IsClean      | Should -BeTrue
        $verdict.Succeeded    | Should -Be 1000
        $verdict.MissingCount | Should -Be 0
    }

    It 'reports NOT clean when SucceededTotal is short of expected' {
        $row     = New-SummaryRow -SucceededTotal 984
        $verdict = Get-WwJobsDbVerdict -SummaryRow $row -ExpectedMessageCount 1000
        $verdict.IsClean   | Should -BeFalse
        $verdict.Succeeded | Should -Be 984
    }

    It 'reports NOT clean and surfaces the missing sequence numbers when some messages never got a jobs1 row at all' {
        $row     = New-SummaryRow -SucceededTotal 998
        $verdict = Get-WwJobsDbVerdict -SummaryRow $row -ExpectedMessageCount 1000 -MissingSeqNums @(42, 917)
        $verdict.IsClean       | Should -BeFalse
        $verdict.MissingCount  | Should -Be 2
        $verdict.MissingSeqNums | Should -Be @(42, 917)
    }

    It 'reports NOT clean when a duplicate success exists, even if SucceededTotal matches expected' {
        # A message that succeeded twice inflates nothing in SucceededTotal (it is still one
        # distinct message, counted once) but IS a genuine reliability bug on its own.
        $row     = New-SummaryRow -SucceededTotal 1000 -DuplicateSuccess_ReliabilityBug 1
        $verdict = Get-WwJobsDbVerdict -SummaryRow $row -ExpectedMessageCount 1000
        $verdict.IsClean               | Should -BeFalse
        $verdict.DuplicateSuccessCount | Should -Be 1
    }

    It 'passes through StillIncomplete / StuckPastThreshold without affecting IsClean on their own when counts otherwise match' {
        # A message can be StillInProcess_Incomplete without having missed SucceededTotal for
        # a DIFFERENT reason first — but per usp_jobs1_Summary's own bucket definitions a
        # message can only be in exactly one bucket, so this scenario (short SucceededTotal
        # explaining the pending count) is what the surrounding fields communicate; IsClean
        # itself only asserts on Succeeded/Duplicates/Missing per Get-WwJobsDbVerdict's own
        # contract, and callers read StillIncomplete/StuckPastThreshold separately.
        $row     = New-SummaryRow -SucceededTotal 995 -StillInProcess_Incomplete 5 -StillInProcess_StuckPastThreshold 2
        $verdict = Get-WwJobsDbVerdict -SummaryRow $row -ExpectedMessageCount 1000
        $verdict.StillIncomplete    | Should -Be 5
        $verdict.StuckPastThreshold | Should -Be 2
        $verdict.IsClean            | Should -BeFalse
    }

    It 'defaults MissingSeqNums to an empty array when omitted' {
        $row     = New-SummaryRow -SucceededTotal 1000
        $verdict = Get-WwJobsDbVerdict -SummaryRow $row -ExpectedMessageCount 1000
        @($verdict.MissingSeqNums).Count | Should -Be 0
    }
}


Describe 'Test-ShovelBridgeE2E — Get-WwCorrelationGuidStem (pure, no DB)' {

    BeforeAll {
        $script:TargetScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
        . $script:TargetScript -LoadFunctionsOnly
    }

    <#
        WHY THIS MATTERS MORE THAN IT LOOKS (WOLF-8512)

        Phase 4c in ProfilerRecommendation mode rebuilds the MandateIds the E2E harness stamped
        onto each published message, then reconciles them against Profiler.ProfilerRecommendation.
        The harness does the stamping in C# (WorkflowInputTemplate.ToCorrelationGuid); this script
        reproduces it in PowerShell.

        A drift between those two implementations does NOT throw. It silently produces MandateIds
        that match nothing, and Phase 4c then reports every single message as missing — a green
        run indistinguishable from total data loss. The parity test below pins the exact literal
        both sides must produce for the same input.
    #>

    It 'takes 26 hex characters of the prefix and formats them as the leading part of a GUID' {
        Get-WwCorrelationGuidStem -CorrelationIdPrefix '648ddd78686d49c792c19bbf0806a1e6-000042' |
            Should -Be '648ddd78-686d-49c7-92c1-9bbf08'
    }

    It 'produces, with the sequence appended, the SAME GUID as the C# harness (parity pin)' {
        # Must equal WorkflowInputTemplate.ToCorrelationGuid('648ddd78686d49c792c19bbf0806a1e6-000042'),
        # which is asserted against this same literal in WorkflowInputTemplateTests.cs
        # (ToCorrelationGuid_TakesTwentySixHexOfPrefixThenTheSixDigitSequence).
        $stem = Get-WwCorrelationGuidStem -CorrelationIdPrefix '648ddd78686d49c792c19bbf0806a1e6-000042'
        ($stem + '000042') | Should -Be '648ddd78-686d-49c7-92c1-9bbf08000042'
    }

    It 'yields a stem that forms a parseable [guid] once the 6-digit sequence is appended' {
        $stem = Get-WwCorrelationGuidStem -CorrelationIdPrefix '648ddd78686d49c792c19bbf0806a1e6-000042'
        foreach ($seq in @('000000', '000042', '000999')) {
            [guid]::TryParse(($stem + $seq), [ref]([guid]::Empty)) | Should -BeTrue
        }
    }

    It 'is stable regardless of which message sequence the correlationId carries' {
        # The stem is per-RUN, so every message in a run must derive the same one.
        $a = Get-WwCorrelationGuidStem -CorrelationIdPrefix '648ddd78686d49c792c19bbf0806a1e6-000000'
        $b = Get-WwCorrelationGuidStem -CorrelationIdPrefix '648ddd78686d49c792c19bbf0806a1e6-000999'
        $a | Should -Be $b
    }

    It 'gives different runs different stems' {
        # Run-scoping is a correctness requirement: usp_InsertProfilerRecommendation is an upsert
        # keyed on (MandateId, ForPaymentCycle), so a repeated stem would make every later run
        # return AlreadyExists=1, insert nothing, and look like total message loss.
        $runOne = Get-WwCorrelationGuidStem -CorrelationIdPrefix '648ddd78686d49c792c19bbf0806a1e6-000042'
        $runTwo = Get-WwCorrelationGuidStem -CorrelationIdPrefix '0f4f49d148c8400ab1c2d3e4f5a6b7c8-000042'
        $runOne | Should -Not -Be $runTwo
    }

    It 'pads a short or non-hex prefix rather than emitting a malformed GUID' {
        # The value is interpolated into a SQL uniqueidentifier literal, so it must always parse.
        foreach ($prefix in @('short-000001', 'zzzzzzzzzzzzzzzzzzzzzzzz-000001', '-000001')) {
            $stem = Get-WwCorrelationGuidStem -CorrelationIdPrefix $prefix
            [guid]::TryParse(($stem + '000001'), [ref]([guid]::Empty)) | Should -BeTrue -Because "prefix '$prefix' must still yield a valid GUID"
        }
    }
}


Describe 'Test-ShovelBridgeE2E — Phase 4c dot-source scope clobbering (WOLF-8512 regression, build 30636/30638)' {
    <#
        Phase 4c dot-sources Invoke-WwQueueLoadTest.ps1 (". `$loadTestScript -LoadFunctionsOnly")
        purely to reuse its Invoke-WwSqlQuery helper. Dot-sourcing shares scope, and that script's
        OWN param block declares a [Nullable[int]] $MessageCount with no default - since it isn't
        passed at that call site, PowerShell binds it to $null in the SHARED scope, clobbering
        THIS script's -MessageCount value. That turned every Phase 4c reconciliation query into
        "TOP ()", which SQL Server rejected with "Incorrect syntax near ')'" - the exact failure
        build 30636/30638 reported. These tests need no Docker/RabbitMQ/SQL/Service Bus: dot-
        sourcing with -LoadFunctionsOnly loads only the target script's own param block and
        function definitions before it returns.
    #>

    BeforeAll {
        $script:LoadTestScript = Join-Path $PSScriptRoot '..\..\Invoke-WwQueueLoadTest.ps1'
    }

    It 'demonstrates the hazard: dot-sourcing Invoke-WwQueueLoadTest.ps1 -LoadFunctionsOnly clobbers an existing $MessageCount to $null' {
        $MessageCount = 1000
        . $script:LoadTestScript -LoadFunctionsOnly
        $MessageCount | Should -BeNullOrEmpty -Because 'Invoke-WwQueueLoadTest.ps1 declares its own unbound [Nullable[int]] $MessageCount, which rebinds in the shared scope'
    }

    It 'confirms the fix: saving and restoring $MessageCount around the dot-source preserves it' {
        # Mirrors exactly the save/restore now used in Test-ShovelBridgeE2E.ps1 around its own
        # ". $loadTestScript -LoadFunctionsOnly" call.
        $MessageCount = 1000
        $savedMessageCount = $MessageCount
        . $script:LoadTestScript -LoadFunctionsOnly
        $MessageCount = $savedMessageCount
        $MessageCount | Should -Be 1000
    }
}


Describe 'Test-ShovelBridgeE2E — Get-WwDrainVerdict (pure, no Azure)' {
    <#
        The drain wait's decision logic, split out from its I/O so it can be tested without an
        Azure subscription. Two behaviours here are load-bearing rather than cosmetic, and both
        have cost real diagnostic time before:

          * The quiet window must be SUSTAINED. The trigger queue's lock duration is PT5M, so a
            message whose lock expired is redelivered roughly five minutes later; a verdict that
            declared "drained" on the first zero reading would end the run inside that gap and
            report every redelivered message as permanently missing.
          * An UNREADABLE count is not a zero. If an 'az servicebus queue show' hiccup were read
            as an empty queue, the run would stop waiting while executions were still in flight.
    #>

    BeforeAll {
        $script:TargetScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
        . $script:TargetScript -LoadFunctionsOnly
        $script:T0 = [datetime]::new(2026, 9, 15, 12, 0, 0, [DateTimeKind]::Utc)
    }

    It 'is quiet but NOT drained on the first zero reading — the window has not elapsed yet' {
        $v = Get-WwDrainVerdict -ActiveCount 0 -ScheduledCount 0 -QuietSeconds 360 -QuietSince $null -NowUtc $script:T0
        $v.IsQuiet      | Should -BeTrue
        $v.IsDrained    | Should -BeFalse
        $v.SecondsQuiet | Should -Be 0
        $v.QuietSince   | Should -Be $script:T0 -Because 'the streak starts now and is carried into the next sample'
    }

    It 'is still NOT drained one second short of the quiet window' {
        $v = Get-WwDrainVerdict -ActiveCount 0 -ScheduledCount 0 -QuietSeconds 360 -QuietSince $script:T0 -NowUtc $script:T0.AddSeconds(359)
        $v.IsDrained    | Should -BeFalse
        $v.SecondsQuiet | Should -Be 359
    }

    It 'is drained once the quiet window is exactly reached' {
        $v = Get-WwDrainVerdict -ActiveCount 0 -ScheduledCount 0 -QuietSeconds 360 -QuietSince $script:T0 -NowUtc $script:T0.AddSeconds(360)
        $v.IsDrained    | Should -BeTrue
        $v.SecondsQuiet | Should -Be 360
    }

    It 'RESETS the quiet streak when messages reappear — the PT5M redelivery case' {
        # 5 minutes of quiet, then a redelivery lands. Without the reset, the next zero reading
        # would inherit the old streak and immediately satisfy a 360s window, ending the run
        # while the redelivered message was still executing.
        $v = Get-WwDrainVerdict -ActiveCount 3 -ScheduledCount 0 -QuietSeconds 360 -QuietSince $script:T0 -NowUtc $script:T0.AddSeconds(300)
        $v.IsQuiet      | Should -BeFalse
        $v.IsDrained    | Should -BeFalse
        $v.QuietSince   | Should -BeNullOrEmpty -Because 'the streak must restart from scratch, not resume'
        $v.SecondsQuiet | Should -Be 0
    }

    It 'counts SCHEDULED messages as not-quiet even when active is zero' {
        $v = Get-WwDrainVerdict -ActiveCount 0 -ScheduledCount 7 -QuietSeconds 360 -QuietSince $script:T0 -NowUtc $script:T0.AddSeconds(400)
        $v.IsQuiet   | Should -BeFalse
        $v.IsDrained | Should -BeFalse
    }

    It 'treats UNREADABLE counts as not-quiet and never as drained' {
        $v = Get-WwDrainVerdict -ActiveCount $null -ScheduledCount $null -QuietSeconds 360 -QuietSince $script:T0 -NowUtc $script:T0.AddSeconds(9999)
        $v.IsReadable | Should -BeFalse
        $v.IsQuiet    | Should -BeFalse
        $v.IsDrained  | Should -BeFalse -Because 'an unreadable queue must never be mistaken for an empty one'
        $v.QuietSince | Should -BeNullOrEmpty
    }

    It 'treats a half-readable sample (active known, scheduled unknown) as not-quiet' {
        $v = Get-WwDrainVerdict -ActiveCount 0 -ScheduledCount $null -QuietSeconds 360 -QuietSince $script:T0 -NowUtc $script:T0.AddSeconds(400)
        $v.IsReadable | Should -BeFalse
        $v.IsDrained  | Should -BeFalse
    }
}


Describe 'Test-ShovelBridgeE2E — late-minted result-token parameter validation' {
    <#
        GET /Secure/servicebus-result/{id} stays Entra-protected (ServiceBusResultFunction's
        [RequireWorkflowPermission(View)]; WorkflowAuthorizationMiddleware rejects an
        unauthenticated /Secure/* request BEFORE policy evaluation, so bypassSecureConfig does
        not make it anonymous). At 20,000 messages the drain outlives a 60-90 minute token, so
        the token is minted AFTER the drain instead - selected by passing four credentials in
        place of -ResultPollAuthToken. These tests pin the guards around that switch.

        All of them assert a THROW, and every one of these throws happens in Phase 0's pure
        validation block before any Docker/RabbitMQ/Service Bus/SQL/network access - which is
        also why the "valid combination" case is deliberately NOT tested here: it would proceed
        into live infrastructure.
    #>

    BeforeAll {
        $script:TargetScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'

        # Minimal arguments that reach the -VerifyWorkflowExecution validation block without
        # tripping an earlier one. -RabbitMqMode defaults to Container, so no External RabbitMQ
        # arguments are needed.
        function script:Invoke-Target {
            param([hashtable] $Extra)
            $splat = @{
                DestinationMode         = 'ExternalServiceBus'
                ExternalShovelDestUri   = 'amqps://example.invalid:5671/?sasl=plain'
                VerifyWorkflowExecution = $true
                EngineBaseUrl           = 'https://engine.invalid'
                ServiceBusNamespaceName = 'ns'
                ServiceBusResourceGroup = 'rg'
            }
            foreach ($k in $Extra.Keys) { $splat[$k] = $Extra[$k] }
            & $script:TargetScript @splat
        }

        function script:New-TestSecureString {
            param([string] $Value = 'secret')
            ConvertTo-SecureString $Value -AsPlainText -Force
        }

        # A syntactically valid 3-segment JWT whose 'exp' is in the past, so
        # Get-JwtRemainingLifetimeSeconds reports it as already expired.
        function script:New-ExpiredJwt {
            $exp = [DateTimeOffset]::UtcNow.AddMinutes(-10).ToUnixTimeSeconds()
            $json = '{"exp":' + $exp + '}'
            $payload = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($json)).TrimEnd('=').Replace('+', '-').Replace('/', '_')
            'eyJhbGciOiJub25lIn0.' + $payload + '.sig'
        }
    }

    It 'throws when NEITHER a pre-minted token nor the late-mint credentials are supplied' {
        { Invoke-Target -Extra @{} } | Should -Throw -ExpectedMessage '*-ResultPollAuthToken is required*'
    }

    It 'throws, naming what is missing, when the late-mint credential set is incomplete' {
        $ex = { Invoke-Target -Extra @{ ResultPollTenantId = 'tenant'; ResultPollClientId = 'client' } } |
            Should -Throw -PassThru
        $ex.Exception.Message | Should -BeLike '*incomplete*'
        $ex.Exception.Message | Should -BeLike '*-ResultPollResourceAppId*'
        $ex.Exception.Message | Should -BeLike '*-ResultPollClientSecret*'
    }

    It 'throws when BOTH a pre-minted token and the late-mint credentials are supplied' {
        # Ambiguous input: the two select different Phase 4 shapes, so silently preferring one
        # would hide which is actually in effect.
        $extra = @{
            ResultPollAuthToken     = New-TestSecureString
            ResultPollTenantId      = 'tenant'
            ResultPollClientId      = 'client'
            ResultPollResourceAppId = 'resource'
            ResultPollClientSecret  = New-TestSecureString
        }
        { Invoke-Target -Extra $extra } | Should -Throw -ExpectedMessage '*not both*'
    }

    It 'throws when late-mint is selected without the Service Bus namespace needed to watch the drain' {
        $extra = @{
            ResultPollTenantId      = 'tenant'
            ResultPollClientId      = 'client'
            ResultPollResourceAppId = 'resource'
            ResultPollClientSecret  = New-TestSecureString
            ServiceBusNamespaceName = ''
        }
        { Invoke-Target -Extra $extra } | Should -Throw -ExpectedMessage '*-ServiceBusNamespaceName*'
    }

    It 'STILL enforces the token-lifetime guard for a pre-minted token (regression pin)' {
        # The guard is deferred in late-mint mode, not removed - the original path must keep
        # failing fast on a stale token exactly as before.
        { Invoke-Target -Extra @{ ResultPollAuthToken = (New-TestSecureString (New-ExpiredJwt)) } } |
            Should -Throw -ExpectedMessage '*ALREADY EXPIRED*'
    }
}


Describe 'Test-ShovelBridgeE2E — Get-WwQueueCountDetails / Get-WwDeadLetterCount' {

    BeforeAll {
        $script:TargetScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
        . $script:TargetScript -LoadFunctionsOnly
    }

    It 'returns $null (not zero) when the namespace or resource group is absent' {
        Get-WwQueueCountDetails -NamespaceName '' -ResourceGroup 'rg' -QueueName 'q' | Should -BeNullOrEmpty
        Get-WwQueueCountDetails -NamespaceName 'ns' -ResourceGroup '' -QueueName 'q' | Should -BeNullOrEmpty
    }

    It 'parses every countDetails field the drain wait relies on' {
        function script:az { $global:LASTEXITCODE = 0; '{"activeMessageCount":12,"deadLetterMessageCount":3,"scheduledMessageCount":4,"transferMessageCount":5}' }
        $counts = Get-WwQueueCountDetails -NamespaceName 'ns' -ResourceGroup 'rg' -QueueName 'q'
        $counts.Active     | Should -Be 12
        $counts.DeadLetter | Should -Be 3
        $counts.Scheduled  | Should -Be 4
        $counts.Transfer   | Should -Be 5
    }

    It 'defaults a countDetails field the API omitted to 0 rather than throwing under StrictMode' {
        function script:az { $global:LASTEXITCODE = 0; '{"activeMessageCount":1}' }
        $counts = Get-WwQueueCountDetails -NamespaceName 'ns' -ResourceGroup 'rg' -QueueName 'q'
        $counts.Active    | Should -Be 1
        $counts.Scheduled | Should -Be 0
        $counts.Transfer  | Should -Be 0
    }

    It 'returns $null when the CLI call fails, so a hiccup is never read as an empty queue' {
        function script:az { $global:LASTEXITCODE = 1; '' }
        Get-WwQueueCountDetails -NamespaceName 'ns' -ResourceGroup 'rg' -QueueName 'q' | Should -BeNullOrEmpty
    }

    It 'keeps the original Get-WwDeadLetterCount contract while delegating to the shared reader' {
        function script:az { $global:LASTEXITCODE = 0; '{"activeMessageCount":0,"deadLetterMessageCount":19459,"scheduledMessageCount":0}' }
        Get-WwDeadLetterCount -NamespaceName 'ns' -ResourceGroup 'rg' -QueueName 'q' | Should -Be 19459
    }

    It 'keeps Get-WwDeadLetterCount returning $null on a failed read' {
        function script:az { $global:LASTEXITCODE = 1; '' }
        Get-WwDeadLetterCount -NamespaceName 'ns' -ResourceGroup 'rg' -QueueName 'q' | Should -BeNullOrEmpty
    }
}
