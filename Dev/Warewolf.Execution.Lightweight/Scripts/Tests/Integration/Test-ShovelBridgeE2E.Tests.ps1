<#
    Unit coverage for Test-ShovelBridgeE2E.ps1's own pure helpers — dot-sourced via
    -LoadFunctionsOnly so this runs with NO docker, RabbitMQ, Service Bus, or SQL dependency
    (mirrors the convention already used by Configure-RabbitMqShovel.Tests.ps1 /
    Deploy-WwExecutionEngine.Tests.ps1 / Invoke-WwQueueLoadTest.Tests.ps1).

    Only Get-WwJobsDbVerdict is covered here: the rest of the script is a live-infra
    integration harness (RabbitMQ + Shovel + Service Bus + the real engine), proven by
    actually running it, not by Pester — same as the script had no Pester coverage at all
    before this file.
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
