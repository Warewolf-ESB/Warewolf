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


Describe 'Test-ShovelBridgeE2E — dot-source scope clobbering (WOLF-8512 regression, builds 30636/30638/30688)' {
    <#
        Test-ShovelBridgeE2E.ps1 dot-sources two other scripts with -LoadFunctionsOnly purely to
        reuse one helper from each: Configure-RabbitMqShovel.ps1 in Phase 0 (Invoke-RabbitMqApi)
        and Invoke-WwQueueLoadTest.ps1 in Phase 4c (Invoke-WwSqlQuery). Dot-sourcing shares scope,
        and merely BINDING a parameter the call site did not pass overwrites a like-named caller
        variable - whether or not the callee ever reads it.

        That has now shipped twice, from the same file:
          * build 30636/30638 - Invoke-WwQueueLoadTest.ps1's unbound [Nullable[int]] $MessageCount
            rebound to $null, turning every Phase 4c reconciliation query into "TOP ()", which SQL
            Server rejects with "Incorrect syntax near ')'".
          * build 30688 - Configure-RabbitMqShovel.ps1's unbound [string] $ServiceBusResourceGroup
            rebound to '', AFTER Phase 0 had already validated it as 'DEV2'. The run published all
            20,000 messages and then died at the Phase 4 drain wait with "Cannot bind argument to
            parameter 'ResourceGroup' because it is an empty string".

        The last It is the one that matters going forward: it AST-diffs the param blocks and fails
        when a newly-shared name is not save/restored, instead of leaving it for a 20,000-message
        pipeline run to find. The two per-site pairs above it document WHY that rule exists.

        No Docker/RabbitMQ/SQL/Service Bus needed: -LoadFunctionsOnly loads only the target
        script's own param block and function definitions before it returns.
    #>

    BeforeAll {
        $script:TargetScript    = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
        $script:LoadTestScript  = Join-Path $PSScriptRoot '..\..\Invoke-WwQueueLoadTest.ps1'
        $script:ConfigureScript = Join-Path $PSScriptRoot '..\..\Configure-RabbitMqShovel.ps1'

        function script:Get-ScriptParameterName {
            param([Parameter(Mandatory)][string] $Path)
            $ast = [System.Management.Automation.Language.Parser]::ParseFile(
                (Resolve-Path -LiteralPath $Path).ProviderPath, [ref] $null, [ref] $null)
            if ($null -eq $ast.ParamBlock) { return @() }
            @($ast.ParamBlock.Parameters | ForEach-Object { $_.Name.VariablePath.UserPath })
        }

        # Runs of spaces/tabs collapsed so the assertions are insensitive to how the '=' in each
        # save/restore block happens to be column-aligned.
        $script:TargetSource = (Get-Content -LiteralPath $script:TargetScript -Raw) -replace '[ \t]+', ' '
    }

    It 'demonstrates the Phase 4c hazard: dot-sourcing Invoke-WwQueueLoadTest.ps1 -LoadFunctionsOnly clobbers an existing $MessageCount to $null' {
        $MessageCount = 1000
        . $script:LoadTestScript -LoadFunctionsOnly
        $MessageCount | Should -BeNullOrEmpty -Because 'Invoke-WwQueueLoadTest.ps1 declares its own unbound [Nullable[int]] $MessageCount, which rebinds in the shared scope'
    }

    It 'confirms the Phase 4c fix: saving and restoring $MessageCount around the dot-source preserves it' {
        # Mirrors exactly the save/restore used in Test-ShovelBridgeE2E.ps1 around its own
        # ". $loadTestScript -LoadFunctionsOnly" call.
        $MessageCount = 1000
        $savedMessageCount = $MessageCount
        . $script:LoadTestScript -LoadFunctionsOnly
        $MessageCount = $savedMessageCount
        $MessageCount | Should -Be 1000
    }

    It 'demonstrates the Phase 0 hazard: dot-sourcing Configure-RabbitMqShovel.ps1 -LoadFunctionsOnly clobbers an existing $ServiceBusResourceGroup to an empty string' {
        # The build 30688 failure in one assertion. Note -ShovelName IS passed here, exactly as the
        # real call site passes it - that is what makes this specific to the parameter nobody
        # noticed the two scripts had started sharing.
        $ServiceBusResourceGroup = 'DEV2'
        . $script:ConfigureScript -LoadFunctionsOnly -ShovelName 'wwexecution-shovel-verify-e2e-loadtest'
        $ServiceBusResourceGroup | Should -BeNullOrEmpty -Because 'Configure-RabbitMqShovel.ps1 declares its own unbound [string] $ServiceBusResourceGroup, which rebinds in the shared scope'
    }

    It 'confirms the Phase 0 fix: saving and restoring the shared names around the dot-source preserves them' {
        $ShovelName              = 'wwexecution-shovel-verify-e2e-loadtest'
        $ServiceBusResourceGroup = 'DEV2'

        $savedShovelName              = $ShovelName
        $savedServiceBusResourceGroup = $ServiceBusResourceGroup
        . $script:ConfigureScript -LoadFunctionsOnly -ShovelName $ShovelName
        $ShovelName              = $savedShovelName
        $ServiceBusResourceGroup = $savedServiceBusResourceGroup

        $ShovelName              | Should -Be 'wwexecution-shovel-verify-e2e-loadtest'
        $ServiceBusResourceGroup | Should -Be 'DEV2'
    }

    It 'pins the rule: every parameter name shared with a dot-sourced script is both saved and restored at that call site' {
        # The self-maintaining guard. Adding a parameter to EITHER script that collides with one of
        # Test-ShovelBridgeE2E.ps1's own fails here the moment it is introduced, rather than being
        # discovered from a pipeline log. -LoadFunctionsOnly is deliberately included in the shared
        # set: it is passed at both call sites and so is harmless today, but exempting it by hand is
        # exactly the kind of judgement call that let -ServiceBusResourceGroup through.
        $targetParams = Get-ScriptParameterName -Path $script:TargetScript
        $targetParams | Should -Not -BeNullOrEmpty

        $violations = @()
        foreach ($dep in @(
            @{ Path = $script:ConfigureScript; Label = 'Configure-RabbitMqShovel.ps1 (Phase 0)' }
            @{ Path = $script:LoadTestScript;  Label = 'Invoke-WwQueueLoadTest.ps1 (Phase 4c)' }
        )) {
            $shared = @(Get-ScriptParameterName -Path $dep.Path | Where-Object { $targetParams -contains $_ })
            $shared | Should -Not -BeNullOrEmpty -Because "$($dep.Label) shares at least -LoadFunctionsOnly with Test-ShovelBridgeE2E.ps1; an empty set here means the AST diff itself has stopped working"

            foreach ($name in $shared) {
                $save    = "`$saved$name = `$$name"
                $restore = "`$$name = `$saved$name"
                if ($script:TargetSource -notlike "*$save*")    { $violations += "$($dep.Label): -$name is never saved before the dot-source (expected the line '$save')" }
                if ($script:TargetSource -notlike "*$restore*") { $violations += "$($dep.Label): -$name is never restored after the dot-source (expected the line '$restore')" }
            }
        }

        ($violations -join [Environment]::NewLine) | Should -BeNullOrEmpty -Because 'dot-sourcing rebinds EVERY unpassed parameter of the callee in the caller''s scope, so each shared name must be saved before the dot-source and restored after it (builds 30638 and 30688)'
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


Describe 'Test-ShovelBridgeE2E — Read-WwShovelDataFile (pure, no DB)' {

    BeforeAll {
        . (Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1') -LoadFunctionsOnly

        function script:New-DataFile {
            param([string] $Json)
            $p = Join-Path $TestDrive ([guid]::NewGuid().ToString('N') + '.json')
            Set-Content -LiteralPath $p -Value $Json
            $p
        }
    }

    It 'returns the record count and each MandateId as an upper-case dashed GUID, in file order' {
        $f = New-DataFile '[{"MandateId":"dcf7b738-bcf6-f011-8406-000d3a48abe3"},{"MandateId":"{1D90C594-3EE5-EF11-8EEA-7C1E52272B2B}"}]'
        $r = Read-WwShovelDataFile -Path $f
        $r.Count | Should -Be 2
        @($r.MandateIds) | Should -Be @('DCF7B738-BCF6-F011-8406-000D3A48ABE3', '1D90C594-3EE5-EF11-8EEA-7C1E52272B2B')
    }

    It 'handles a single-record file' {
        $r = Read-WwShovelDataFile -Path (New-DataFile '[{"MandateId":"DCF7B738-BCF6-F011-8406-000D3A48ABE3"}]')
        $r.Count | Should -Be 1
        @($r.MandateIds).Count | Should -Be 1
    }

    It 'throws for a missing file' {
        { Read-WwShovelDataFile -Path (Join-Path $TestDrive 'nope.json') } | Should -Throw '*not found*'
    }

    It 'throws for an empty array' {
        { Read-WwShovelDataFile -Path (New-DataFile '[]') } | Should -Throw '*contains no records*'
    }

    It 'throws naming the record when a MandateId is absent' {
        { Read-WwShovelDataFile -Path (New-DataFile '[{"MandateId":"DCF7B738-BCF6-F011-8406-000D3A48ABE3"},{"ForPaymentCycle":"202610"}]') } |
            Should -Throw '*Record 2*no MandateId*'
    }

    It 'rejects a MandateId that is not a GUID - it is interpolated into SQL later' {
        { Read-WwShovelDataFile -Path (New-DataFile '[{"MandateId":"x''); DROP TABLE Profiler.ProfilerRecommendation;--"}]') } |
            Should -Throw '*not a GUID*'
    }

    It 'rejects repeated MandateIds' {
        $id = 'DCF7B738-BCF6-F011-8406-000D3A48ABE3'
        { Read-WwShovelDataFile -Path (New-DataFile "[{`"MandateId`":`"$id`"},{`"MandateId`":`"$($id.ToLower())`"}]") } |
            Should -Throw '*repeats MandateIds (1 unique of 2)*'
    }

    It 'throws when an element is not an object' {
        { Read-WwShovelDataFile -Path (New-DataFile '["oops"]') } | Should -Throw '*Record 1*not a JSON object*'
    }
}

Describe 'Test-ShovelBridgeE2E — Get-WwProfilerReconciliationQueries (pure, no DB)' {

    BeforeAll {
        . (Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1') -LoadFunctionsOnly
        function script:New-Ids { param([int] $Count) 0..($Count - 1) | ForEach-Object { '{0:X8}-0000-0000-0000-000000000000' -f $_ } }
    }

    It 'emits one query per ChunkSize ids, and none beyond the list' {
        @(Get-WwProfilerReconciliationQueries -MandateIds (New-Ids 2500) -ChunkSize 1000).Count | Should -Be 3
        @(Get-WwProfilerReconciliationQueries -MandateIds (New-Ids 1000) -ChunkSize 1000).Count | Should -Be 1
        @(Get-WwProfilerReconciliationQueries -MandateIds (New-Ids 1) -ChunkSize 1000).Count | Should -Be 1
    }

    It 'never puts more than 1000 rows in a VALUES constructor (the SQL Server limit)' {
        foreach ($q in Get-WwProfilerReconciliationQueries -MandateIds (New-Ids 2500)) {
            ([regex]::Matches($q, "\(\d+,'[0-9A-F-]{36}'\)")).Count | Should -BeLessOrEqual 1000
        }
    }

    It 'numbers SeqNum by position in the whole list, across chunks' {
        $q = @(Get-WwProfilerReconciliationQueries -MandateIds (New-Ids 5) -ChunkSize 2)
        $q.Count | Should -Be 3
        $q[1] | Should -Match "\(2,'00000002-"
        $q[1] | Should -Match "\(3,'00000003-"
        $q[1] | Should -Not -Match "\(1,'"
        $q[2] | Should -Match "\(4,'00000004-"
    }

    It 'covers every id exactly once across the chunks' {
        $ids = New-Ids 2345
        $text = (Get-WwProfilerReconciliationQueries -MandateIds $ids -ChunkSize 1000) -join "`n"
        ([regex]::Matches($text, "\(\d+,'([0-9A-F-]{36})'\)") | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique).Count | Should -Be 2345
        ([regex]::Matches($text, "\(\d+,'[0-9A-F-]{36}'\)")).Count | Should -Be 2345
    }

    It 'restricts both aggregates to the chunk, returns the columns the verdict code reads, and orders by SeqNum' {
        $q = Get-WwProfilerReconciliationQueries -MandateIds (New-Ids 3)
        $q | Should -Match 'FROM Profiler\.ProfilerRecommendation p WITH \(NOLOCK\)\s+WHERE p\.MandateId IN \(SELECT MandateId FROM Expected\)'
        $q | Should -Match 'FROM Profiler\.MandateScores s WITH \(NOLOCK\)\s+WHERE s\.MandateId IN \(SELECT MandateId FROM Expected\)'
        foreach ($col in 'AS SeqNum', 'AS RowsForMandate', 'AS ScoreRows', 'ORDER BY e\.SeqNum') { $q | Should -Match $col }
    }

    It 'refuses a value that is not a GUID rather than interpolating it' {
        { Get-WwProfilerReconciliationQueries -MandateIds @("x'); DROP TABLE t;--") } | Should -Throw '*not a GUID*'
    }
}

Describe 'Test-ShovelBridgeE2E — -DataFilePath parameter validation' {
    <#
        Every throw here happens in Phase 0, before any Docker/RabbitMQ/Service Bus/SQL access
        (same approach as the late-minted token validation above).
    #>

    BeforeAll {
        $script:TargetScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
        $script:DataFile = Join-Path $TestDrive 'data.json'
        Set-Content -LiteralPath $script:DataFile -Value '[{"MandateId":"DCF7B738-BCF6-F011-8406-000D3A48ABE3"},{"MandateId":"1D90C594-3EE5-EF11-8EEA-7C1E52272B2B"}]'

        function script:Invoke-Target {
            param([hashtable] $Extra, [switch] $NoVerify)
            $splat = @{
                DestinationMode         = 'ExternalServiceBus'
                ExternalShovelDestUri   = 'amqps://example.invalid:5671/?sasl=plain'
                EngineBaseUrl           = 'https://engine.invalid'
                ServiceBusNamespaceName = 'ns'
                ServiceBusResourceGroup = 'rg'
                DataFilePath            = $script:DataFile
            }
            if (-not $NoVerify) { $splat['VerifyWorkflowExecution'] = $true }
            foreach ($k in $Extra.Keys) { $splat[$k] = $Extra[$k] }
            & $script:TargetScript @splat
        }
    }

    It 'requires -VerifyWorkflowExecution' {
        { Invoke-Target -NoVerify -Extra @{ ExternalServiceBusConnectionString = (ConvertTo-SecureString 'x' -AsPlainText -Force) } } |
            Should -Throw '*-DataFilePath requires -VerifyWorkflowExecution*'
    }

    It 'is mutually exclusive with -WorkflowInputsJson' {
        { Invoke-Target -Extra @{ WorkflowInputsJson = '{"a":"b"}' } } | Should -Throw '*mutually exclusive*'
    }

    It 'refuses the Jobs1 verification mode, whose generated-prefix match real records cannot satisfy' {
        { Invoke-Target -Extra @{ JobsDbConnectionString = (ConvertTo-SecureString 'x' -AsPlainText -Force); DbVerificationMode = 'Jobs1' } } |
            Should -Throw '*needs -DbVerificationMode ProfilerRecommendation*'
    }

    It 'refuses an explicit -MessageCount that disagrees with the record count' {
        { Invoke-Target -Extra @{ MessageCount = 5 } } | Should -Throw '*disagrees with the 2 record(s)*'
    }

    It 'accepts an explicit -MessageCount equal to the record count, getting past the data-file checks' {
        # Proceeds into the next Phase 0 guard (the result-poll token), which is the proof the
        # data-file validation passed; nothing past Phase 0 is reached.
        { Invoke-Target -Extra @{ MessageCount = 2 } } | Should -Throw '*-ResultPollAuthToken is required*'
    }

    It 'validates the file before anything else - a missing file fails fast' {
        { Invoke-Target -Extra @{ DataFilePath = (Join-Path $TestDrive 'absent.json') } } | Should -Throw '*not found*'
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

Describe 'Test-ShovelBridgeE2E — Invoke-WwProfilerReconciliation (no DB; builds 31074/31077)' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1') -LoadFunctionsOnly

        # Wraps its rows exactly as the real Invoke-WwSqlQuery does; the first It below reads the
        # real one's source, so this can't silently stop matching it.
        $script:SqlQuerySource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '..\..\Invoke-WwQueueLoadTest.ps1')

        function Invoke-WwSqlQuery {
            param([string] $ConnectionString, [string] $Query)
            $rows = [regex]::Matches($Query, "\((\d+),'[0-9a-fA-F-]{36}'\)") | ForEach-Object {
                [pscustomobject]@{ SeqNum = [int]$_.Groups[1].Value; RowsForMandate = 1; ScoreRows = 1 }
            }
            return , @($rows)
        }
        function New-Ids([int] $Count) { 1..$Count | ForEach-Object { [guid]::NewGuid().ToString() } }
    }

    It 'stub matches how Invoke-WwSqlQuery really returns rows (one wrapped array per call)' {
        $script:SqlQuerySource | Should -Match 'return\s*,\s*@\(\$result\.rows'
    }

    It 'returns one flat row per MandateId across chunks, each readable by the verdict code' {
        $rows = Invoke-WwProfilerReconciliation -ConnectionString 'cs' -MandateIds (New-Ids 2500)

        $rows.Count | Should -Be 2500
        @($rows | Where-Object { [int]$_.RowsForMandate -eq 0 }).Count | Should -Be 0
        @($rows | ForEach-Object { [int]$_.SeqNum }) | Should -Be (0..2499)
    }

    It 'keeps a single chunk flat too' {
        $rows = Invoke-WwProfilerReconciliation -ConnectionString 'cs' -MandateIds (New-Ids 3)

        $rows.Count | Should -Be 3
        [int]$rows[2].SeqNum | Should -Be 2
    }
}

Describe 'Test-ShovelBridgeE2E — -SkipResultPoll parameter validation' {
    <#
        -SkipResultPoll drops the /secure/servicebus-result poll (persistence off + Flex
        scale-to-zero made it 404 on every id: build 31194, 0/20000 while the database held all
        20000). The database and dead-letter checks become the gate, so their inputs are required.
        As in the late-mint suite above, every case asserts a Phase 0 THROW, before any network
        access. The last case also proves the token requirement is lifted: it gets past the
        "-ResultPollAuthToken is required" check and stops at a later guard.
    #>

    BeforeAll {
        $script:TargetScript = Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1'
        function script:New-TestSecureString {
            param([string] $Value = 'secret')
            ConvertTo-SecureString $Value -AsPlainText -Force
        }
        function script:Invoke-SkipPoll {
            param([hashtable] $Extra)
            $splat = @{
                DestinationMode         = 'ExternalServiceBus'
                ExternalShovelDestUri   = 'amqps://example.invalid:5671/?sasl=plain'
                VerifyWorkflowExecution = $true
                SkipResultPoll          = $true
                EngineBaseUrl           = 'https://engine.invalid'
                ServiceBusNamespaceName = 'ns'
                ServiceBusResourceGroup = 'rg'
                JobsDbConnectionString  = (New-TestSecureString 'Server=db.invalid')
            }
            foreach ($k in $Extra.Keys) { $splat[$k] = $Extra[$k] }
            & $script:TargetScript @splat
        }
    }

    It 'throws without -VerifyWorkflowExecution' {
        $extra = @{
            VerifyWorkflowExecution            = $false
            ExternalServiceBusConnectionString = (New-TestSecureString 'Endpoint=sb://ns.invalid/')
        }
        { Invoke-SkipPoll -Extra $extra } | Should -Throw -ExpectedMessage '*-SkipResultPoll requires -VerifyWorkflowExecution*'
    }

    It 'throws when a pre-minted result-poll token is also supplied' {
        { Invoke-SkipPoll -Extra @{ ResultPollAuthToken = New-TestSecureString } } |
            Should -Throw -ExpectedMessage '*-SkipResultPoll takes no result-poll token*'
    }

    It 'throws when late-mint result-poll credentials are also supplied' {
        $extra = @{
            ResultPollTenantId      = 'tenant'
            ResultPollClientId      = 'client'
            ResultPollResourceAppId = 'resource'
            ResultPollClientSecret  = New-TestSecureString
        }
        { Invoke-SkipPoll -Extra $extra } | Should -Throw -ExpectedMessage '*-SkipResultPoll takes no result-poll token*'
    }

    It 'throws without -JobsDbConnectionString, since the database check is the gate' {
        { Invoke-SkipPoll -Extra @{ JobsDbConnectionString = $null } } |
            Should -Throw -ExpectedMessage '*-SkipResultPoll requires -JobsDbConnectionString*'
    }

    It 'needs no result-poll token, but throws without the Service Bus namespace used for the drain and dead-letter gate' {
        $ex = { Invoke-SkipPoll -Extra @{ ServiceBusNamespaceName = '' } } | Should -Throw -PassThru
        $ex.Exception.Message | Should -BeLike '*-SkipResultPoll requires -ServiceBusNamespaceName*'
        $ex.Exception.Message | Should -Not -BeLike '*-ResultPollAuthToken is required*'
    }
}

Describe 'Test-ShovelBridgeE2E — Get-WwSkipPollVerdict (pure)' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Test-ShovelBridgeE2E.ps1') -LoadFunctionsOnly
        function script:New-Db {
            param([bool] $Clean = $true, [string] $Detail = '0 missing and 0 duplicated of 20000', [string] $ErrorText = $null)
            [pscustomobject]@{ Clean = $Clean; Detail = $Detail; Error = $ErrorText }
        }
    }

    It 'passes when the publish succeeded, the database is clean and nothing was dead-lettered (build 31194''s actual outcome)' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 0 -DbOutcome (New-Db) -DlqDelta 0
        $v.Passed | Should -BeTrue
        $v.Reasons | Should -BeNullOrEmpty
    }

    It 'passes when the dead-letter count went down (queue purged mid-run is not a failure)' {
        (Get-WwSkipPollVerdict -PublishExitCode 0 -DbOutcome (New-Db) -DlqDelta -5).Passed | Should -BeTrue
    }

    It 'fails when the publish leg failed' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 1 -DbOutcome (New-Db) -DlqDelta 0
        $v.Passed | Should -BeFalse
        $v.Reasons | Should -Contain 'the publish leg failed (exit code 1)'
    }

    It 'fails when the database reconciliation never ran' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 0 -DbOutcome $null -DlqDelta 0
        $v.Passed | Should -BeFalse
        $v.Reasons | Should -Contain 'the Phase 4c database reconciliation did not run'
    }

    It 'fails, carrying the error, when the database reconciliation threw' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 0 -DbOutcome (New-Db -Clean $false -Detail $null -ErrorText 'Login failed') -DlqDelta 0
        $v.Passed | Should -BeFalse
        $v.Reasons | Should -Contain 'the Phase 4c database reconciliation failed: Login failed'
    }

    It 'fails, carrying the detail, when rows are missing or duplicated' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 0 -DbOutcome (New-Db -Clean $false -Detail '3 missing and 1 duplicated of 20000') -DlqDelta 0
        $v.Passed | Should -BeFalse
        $v.Reasons | Should -Contain 'the Phase 4c database reconciliation was not clean: 3 missing and 1 duplicated of 20000'
    }

    It 'fails when the dead-letter count could not be read' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 0 -DbOutcome (New-Db) -DlqDelta $null
        $v.Passed | Should -BeFalse
        $v.Reasons | Should -Contain 'the Phase 4d dead-letter count could not be read'
    }

    It 'fails when messages were dead-lettered' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 0 -DbOutcome (New-Db) -DlqDelta 7
        $v.Passed | Should -BeFalse
        $v.Reasons | Should -Contain '7 message(s) were dead-lettered during this run'
    }

    It 'reports every failing check, not just the first' {
        $v = Get-WwSkipPollVerdict -PublishExitCode 2 -DbOutcome $null -DlqDelta 1
        $v.Passed | Should -BeFalse
        @($v.Reasons).Count | Should -Be 3
    }
}
